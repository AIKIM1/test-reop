using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{

    /// <summary>
    /// PGM_GUI_015.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_011 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_011()
        {
            InitializeComponent();
        }

        #region event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initCombo();
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            int selectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            Util.Alert(selectedIndex.ToString());
            //(sender as CheckBox). //equipid를 가져옴
            string sEquipId = DataTableConverter.GetValue(dgEquipment.Rows[selectedIndex].DataItem, "EQUIPID").ToString();

            SearchReservedLot(sEquipId);//true, equipid
        }
        #endregion

        #region method
        private void initCombo()
        {
            CommonCombo _combo = new CommonCombo();
            string[] sFliter = { LoginInfo.CFG_SHOP_ID };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboProcess }; //설비 room은 기준 정보가 와야함
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sFilter: sFliter);

            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent, cbChild: cboLineChild);

            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent);
            //C1ComboBox[] cboAreaParent = { cboShop };
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment};
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbParent: cboAreaParent, cbChild: cboAreaChild, sFilter: null);
            //string[] str = { "A1" };//LoginInfo.CONFIG_AREA_ID
            //CommonCombo _combo = new CommonCombo();

            //C1ComboBox[] cboLineChild = { cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: str);

            //String[] sFilter = { Process.ROLL_PRESSING };
            //C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cbEquipmentParent, sFilter: sFilter);
        }

        private void SearchData()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("LINEID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            //inDataTable.Columns.Add("ELECTRODE", typeof(string));
            //inDataTable.Columns.Add("MODELID", typeof(string));
            //inDataTable.Columns.Add("VERSION", typeof(string));
            //inDataTable.Columns.Add("KEY", typeof(string));

            DataRow row = inDataTable.NewRow();
            row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            row["LINEID"] = cboArea.SelectedValue.ToString();
            row["PROCID"] = cboProcess.SelectedValue.ToString();
            inDataTable.Rows.Add(row);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_LOT_LIST", "RQSTDT", "RSLTDT", inDataTable);
            dgWip.ItemsSource = DataTableConverter.Convert(dtResult);

            DataTable inEquipTable = new DataTable();
            inEquipTable.Columns.Add("SHOPID", typeof(string));
            inEquipTable.Columns.Add("LINEID", typeof(string));
            inEquipTable.Columns.Add("PROCID", typeof(string));
            //inDataTable.Columns.Add("ELECTRODE", typeof(string));

            DataRow rowEquip = inEquipTable.NewRow();
            rowEquip["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            rowEquip["LINEID"] = cboArea.SelectedValue.ToString();
            rowEquip["PROCID"] = cboProcess.SelectedValue.ToString();
            inEquipTable.Rows.Add(rowEquip);

            DataTable EuipResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIP", "RQSTDT", "RSLTDT", inDataTable);
            dgEquipment.ItemsSource = DataTableConverter.Convert(EuipResult);


        }
        private void SearchReservedLot(string eqptId)
        {
            if (eqptId == null)
            {
                Util.Alert("equipid가 null");
                return;
            }

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("LINEID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow row = inDataTable.NewRow();
            row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            row["LINEID"] = cboArea.SelectedValue.ToString();
            row["PROCID"] = cboProcess.SelectedValue.ToString();
            row["EQPTID"] = eqptId;
            inDataTable.Rows.Add(row);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("RESERVE_LOT_LIST", "RQSTDT", "RSLTDT", inDataTable);

            dgReservedLotList.ItemsSource = DataTableConverter.Convert(dtResult);


        }



        #endregion


    }
}
