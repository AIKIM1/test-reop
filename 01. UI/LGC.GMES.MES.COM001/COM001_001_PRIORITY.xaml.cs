/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_001_PRIORITY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_001_PRIORITY()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        /// <summary>
        /// Parameter
        /// </summary>
        public string _sEqpt;
        public string _sMonth;
        public string _sDay;

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        
        #endregion

        #region Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }
        #endregion

        #region Mehod
        private void SearchData()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("EQPT", typeof(string));
            IndataTable.Columns.Add("MONTH", typeof(string));
            IndataTable.Columns.Add("DAY", typeof(string));
            DataRow Indata = IndataTable.NewRow();
            Indata["EQPT"] = _sEqpt;
            Indata["MONTH"] = _sMonth;
            Indata["DAY"] = _sDay;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTPLAN_PRIORITY", "RQSTDT", "RSLTDT", IndataTable);
            dgWorkOrderList.ItemsSource = DataTableConverter.Convert(dtResult);

            //new ClientProxy().ExecuteService("DA_PRD_SEL_PRODUCTPLAN_PRIORITY", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;

            //    if (ex != null)
            //    {
            //        return;
            //    }

            //    dgWorkOrderList.ItemsSource = DataTableConverter.Convert(result);
            //});

        }

        private void SaveData()
        {
            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("EQPT", typeof(string));
                    IndataTable.Columns.Add("WOID", typeof(string));
                    IndataTable.Columns.Add("PRIORITY", typeof(string));
                    IndataTable.Columns.Add("PLANQTY", typeof(string));

                    for (int _iRow = 0; _iRow < dgWorkOrderList.Rows.Count; _iRow++)
                    {
                        DataRow Indata = IndataTable.NewRow();
                        Indata["EQPT"] = "COATER1";
                        Indata["WOID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[_iRow].DataItem, dgWorkOrderList.Columns[4].Name));
                        Indata["PRIORITY"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[_iRow].DataItem, dgWorkOrderList.Columns[5].Name));
                        Indata["PLANQTY"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[_iRow].DataItem, dgWorkOrderList.Columns[8].Name));
                        IndataTable.Rows.Add(Indata);
                    }
                    string _BizRule = "BR_PRD_INS_WORKORDER";
                    _BizRule = "DA_PRD_INS_WORKORDER";
                    new ClientProxy().ExecuteService(_BizRule, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                    });


                }
            });
            SearchData();
        }
        #endregion


    }
}
