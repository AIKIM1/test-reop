using System.Windows;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_075_PROCESSINFO
    {
        #region Declaration & Constructor 

        public PGM_GUI_075 PGM_GUI_075;
        public PGM_GUI_077 PGM_GUI_077;
        public PGM_GUI_078 PGM_GUI_078;

        public PGM_GUI_075_PROCESSINFO()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //dgDatagrid.ItemsSource = CreateSampleData();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void dgDatagrid_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.Write("");
            //ProtoType0205.dgDatagrid.ItemsSource = ProtoType0205.CreateSampleData();
        }

        #endregion

        #region Mehod

        public void setProcess(string sLine, string sProcess)
        {
            setProcessInfo(sLine, sProcess);
        }

        /// <summary>
        /// 공정정보 조회 표시
        /// </summary>
        /// <param name="sLine"></param>
        /// <param name="sProcess"></param>
        private void setProcessInfo(string sLineID, string sProcessID)
        {
            //임시 임의로 표시
            txtSelectedProcess.Text = sProcessID;
            txtSelectedEquipment.Text = sProcessID + "TEST표시 설비명";
            txtSelectedProduct.Text = sProcessID + "TEST표시 제품명";
            txtSelectedWorkOrder.Text = sProcessID + "TEST표시 워크오더";

        }

        #endregion
    }
}