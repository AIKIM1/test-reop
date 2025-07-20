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
    /// FORM001_ROUTE_MMD_RouteGradeRJudgSet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_ROUTE_MMD_RouteGradeRJudgSet : C1Window, IWorkArea
    {
        
        
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //LGC.GMES.MMD.MMD001.Common.CommonDataSet _Com = new LGC.GMES.MMD.MMD001.Common.CommonDataSet();

        DataView _dvCMCD { get; set; }

        public FORM001_ROUTE_MMD_RouteGradeRJudgSet()
        {
            InitializeComponent();
            InitializeCombo();
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

        private void dgRjudg_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //string[] Col = { "ROUTID", "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgDeltaOCV, Col);
        }

        private void dgRjudg_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgDeltaOCV);
        }

        #endregion

        #region Mehod
        private void InitializeCombo()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                #region ACTION_COMPLETED
                //IUSE : 사용유무, JUDG_MTHD_CODE : 상대판정 등급, MEASR_TYPE_CODE : 측정유형코드, REF_VALUE_CODE : 참조 기준값 코드, RJUDG_BAS_CODE : 상대판정 기준코드,  STDEV1_FIX_FLAG : 표준편차 고정값
                Get_MMD_SEL_CMCD_TYPE_MULT("'IUSE','JUDG_MTHD_CODE','MEASR_TYPE_CODE','REF_VALUE_CODE','RJUDG_BAS_CODE', 'STDEV1_FIX_FLAG'", null, (dt, ex) =>
                {
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = null;

                    _dvCMCD = dt.DefaultView;

                    _dvCMCD.RowFilter = "CMCDTYPE = 'IUSE'";
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["PAUSE_YN"], CommonCombo.ComboStatus.EMPTY);
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["OUT_PROTECT_YN"], CommonCombo.ComboStatus.EMPTY);

                    _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_MTHD_CODE'";
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["JUDG_MTHD_CODE"], CommonCombo.ComboStatus.EMPTY);

                    _dvCMCD.RowFilter = "CMCDTYPE = 'MEASR_TYPE_CODE'";
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["MEASR_TYPE_CODE"], CommonCombo.ComboStatus.EMPTY);

                    _dvCMCD.RowFilter = "CMCDTYPE = 'REF_VALUE_CODE'";
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["REF_VALUE_CODE"], CommonCombo.ComboStatus.EMPTY);

                    _dvCMCD.RowFilter = "CMCDTYPE = 'RJUDG_BAS_CODE'";
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["RJUDG_BAS_CODE"], CommonCombo.ComboStatus.EMPTY);

                    _dvCMCD.RowFilter = "CMCDTYPE = 'STDEV1_FIX_FLAG'";
                    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRjudg.Columns["STDEV1_FIX_FLAG"], CommonCombo.ComboStatus.EMPTY);

                });
                #endregion
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void InitializeGrid()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgRjudg);

                const string bizRuleName = "MMD_SEL_ROUT_GRD_RJUDG_SET_LIST";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
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

                    dgRjudg.ItemsSource = DataTableConverter.Convert(result);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region SEL COMMONCODE(Multi)
        public void Get_MMD_SEL_CMCD_TYPE_MULT(string sCMCDTYPE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCMCDTYPE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("MMD_SEL_CMCD_TYPE_MULT", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion
    }
}
