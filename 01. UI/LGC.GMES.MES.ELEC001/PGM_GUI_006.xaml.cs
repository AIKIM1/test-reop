using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{

    /// <summary>
    /// PGM_GUI_015.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

       

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_006()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        private void initCombo()
        {

            CommonCombo _combo = new CommonCombo();

            string[] str = { LoginInfo.CFG_AREA_ID };//LoginInfo.CONFIG_AREA_ID "A1" or "A2"
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: str);

            String[] sFilter = { LoginInfo.CFG_PROC_ID }; // SRS이송탱크
            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cbEquipmentParent, sFilter: sFilter);

        }

        #endregion

        #region Event

        #region [Button]

      
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

           
        }
     
        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initCombo();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
          
        }
       
        //불량정보저장
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {

        }

        //품질정보저장
        private void btnQulityInfoSave_Click(object sender, RoutedEventArgs e)
        {
          
        }

       
        //private void dgDefectInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        //{
            
        //    if (e.Cell.Column.Index == 0)
        //    {
        //        return;
        //    }
        //    gDefectChangeFlag = true;

        //    double sum = SumDefectQty(e.Cell.Column.Index - 1);

        //    double inputqty = 0;
        //    int iRow = -1;

        //    for (int i = 0; i < dgLotInfo.Rows.Count; i++)
        //    {
        //        if (e.Cell.Column.Header.ToString().IndexOf(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"))) > 0)
        //        {
        //            inputqty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY")));
        //            iRow = i;
        //            break;
        //        }
        //    }

        //    if (cLot.STATUS.Equals("END"))
        //    {
        //        if (inputqty < sum)
        //        {
        //            Util.Alert("SFU1608");  //생산량보다 불량이 크게 입력 될 수 없습니다.
        //            DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);
        //            return;
        //        }
        //    }

        //    DataTableConverter.SetValue(dgLotInfo.Rows[iRow].DataItem, "LOSSQTY", sum);
        //    SetLotInfoCalc();
        //}

        //private void cboLotID_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    SetReamark();
        //}

        //private void CheckBox_Click(object sender, RoutedEventArgs e)
        //{
        //    ClearData();

        //    if (sender == null)
        //        return;

        //    if ((sender as CheckBox).IsChecked.HasValue && !(bool)(sender as CheckBox).IsChecked)
        //    {
        //        ClearData();
        //        return;
        //    }

        //    int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        //    _Util.SetDataGridUncheck(dgProductLot, "CHK", checkIndex);

        //    if (DataTableConverter.GetValue(dgProductLot.Rows[checkIndex].DataItem, "STATUS").Equals("END"))
        //    {
        //        for (int i = 0; i < dgProductLot.Rows.Count; i++)
        //        {
        //            if (DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "STATUS").Equals("END")
        //                && DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PARENTLOT").Equals(DataTableConverter.GetValue(dgProductLot.Rows[checkIndex].DataItem, "PARENTLOT")))
        //            {
        //                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", true);
        //            }
        //        }
        //    }

        //    ProcessDetail(Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "LOTID")));
        //    SetEnabled(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);

        //}

        //private void txtInputQty_KeyUp(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Key == Key.Enter)
        //        {
        //            SetInputQty();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        #endregion

        #region Mehod

    
        #endregion

      
    }
}
