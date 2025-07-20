/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using System.Windows;


namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_006_TANKLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public ELEC001_006_TANKLOT()
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
            IndataTable.Columns.Add("LANGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;

            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PREMIX_LOT", "RQSTDT", "RSLTDT", IndataTable);
           
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        private void SaveData()
        {
            //LOT을 생성 하시겠습니까?
            Util.MessageConfirm("SFU1371", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("EQPT", typeof(string));

                }
            });
            SearchData();
        }

        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
