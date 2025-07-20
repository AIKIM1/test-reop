/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_017 : UserControl
    {
        #region Declaration & Constructor 
        public PGM_GUI_017()
        {
            InitializeComponent();
            InitCombo();
        }


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
        private void InitCombo()
        {

            string[] str = { "A1" };
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: str);

            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cbEquipmentParent);

        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipment.SelectedIndex < 0)
            {
                return;
            }

            if (cboEquipmentSegment.SelectedValue.ToString().Trim().Length < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라인을 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboEquipment.SelectedValue.ToString().Trim().Length < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("설비를 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            loadingIndicator.Visibility = Visibility.Visible;
            SearchWorkOrder();
        }

        //workoder의 체크박스 클릭시
        private void chkWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgWorkOrder.CurrentRow.DataItem == null)
            {
                return;
            }

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
            {
                if (rowIndex != i)
                    if (dgWorkOrder.GetCell(i, dgWorkOrder.Columns["CHK"].Index).Presenter != null
                        && (dgWorkOrder.GetCell(i, dgWorkOrder.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
                        (dgWorkOrder.GetCell(i, dgWorkOrder.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
            }
            loadingIndicator.Visibility = Visibility.Visible;
            GeProducttLot(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }

        private void chkProductLot_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductLot.CurrentRow.DataItem == null)
            {
                return;
            }

            //DataTableConverter.GetValue();

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            Boolean isCheked = true;

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (rowIndex != i)
                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null
                        && (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

            }


            loadingIndicator.Visibility = Visibility.Visible;
            GetValidConfirm(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            GetProcessActReason(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            GetProductProcessQualItem(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
            GetCommentCombo(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);



        }

        #endregion

        #region Mehod
        private void SearchWorkOrder()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LINE", typeof(string));
            inDataTable.Columns.Add("EQUIP", typeof(string));
            inDataTable.Columns.Add("CHKWATING", typeof(bool));
            inDataTable.Columns.Add("CHKWORKING", typeof(bool));
            inDataTable.Columns.Add("CHKFINISH", typeof(bool));

            DataRow inData = inDataTable.NewRow();
            inData["LINE"] = cboEquipmentSegment.SelectedValue.ToString();
            inData["EQUIP"] = cboEquipmentSegment.SelectedValue.ToString();
            inData["CHKWATING"] = cbWorking.IsChecked;
            inData["CHKWORKING"] = cbWorking.IsChecked;
            inData["CHKFINISH"] = cbWorking.IsChecked;

            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_SEL_DUAL_WORKORDER", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                int planQty = 0;
                int prodQty = 0;
                //int remainQty = 0;

                dgWorkOrder.ItemsSource = DataTableConverter.Convert(result);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgWorkOrder.Rows)
                {
                    DataRowView rowview = row.DataItem as DataRowView;
                    planQty += Int32.Parse(rowview["PLANQTY"].ToString());
                    prodQty += Int32.Parse(rowview["PRODQTY"].ToString());

                }

                txtPlanQty.Text = planQty.ToString();
                txtProdQty.Text = prodQty.ToString();

            });


        }

        private void GeProducttLot(Object SelectedItem)
        {

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            Util.gridClear(dgLotInfo);

            string sWorkOrder = rowview["WO"].ToString();
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sWorkOrder), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);


            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("WORKORDER", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["WORKORDER"] = sWorkOrder;
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_SEL_REWINDER_PRODUCTLOT", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgProductLot.ItemsSource = DataTableConverter.Convert(result);
            });

        }

        private void GetValidConfirm(Object SelectedItem)
        {

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            Util.gridClear(dgLotInfo);

            DataRow row = null;


            string sLotId = rowview["LOTID"].ToString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = sLotId;
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_SEL_LOTINFO_ROLLPRESS", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("GETLOTINFO 예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgLotInfo.ItemsSource = DataTableConverter.Convert(result);

                row = result.Rows[0];

                txtWorkorder.Text = row["WORKWORDER"].ToString();
                txtWorkorder.IsReadOnly = true;

                txtLotStatus.Text = row["STATUS"].ToString();
                txtLotStatus.IsReadOnly = true;

                txtVersion.Text = row["VERSION"].ToString();
                txtVersion.IsReadOnly = true;

                txtChangeRatio.Text = row["RATIO"].ToString();
                txtChangeRatio.IsReadOnly = true;

                txtWorkDay.Text = row["WORKDATE"].ToString();
                txtOutputQty.Text = row["PRODUCTQTY"].ToString();
                txtMotherInputLot.Text = row["MOTHERINPUTLOT"].ToString();
                txtRemainQty2.Text = row["REMAINQTY"].ToString();
                txtRemainQty2.IsReadOnly = true;

                txtWorkGroup.Text = row["WORKGROUP"].ToString();
                txtWorker.Text = row["WORPERSON"].ToString();
                txtWorkStartTime.Text = row["STARTTIME"].ToString();
                txtWorkStartTime.IsReadOnly = true;
                txtFinishTime.Text = row["ENDTIME"].ToString();
                txtOperationTime.Text = row["WORKINGTIME"].ToString();
                txtOperationTime.IsReadOnly = true;

                txtLotComment.Text = row["LOTCOMMENT"].ToString();
                txtLotComment.IsReadOnly = true;


            });

        }

        private void GetProcessActReason(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            Util.gridClear(dgFaultyInfo);

            string sLotId = rowview["LOTID"].ToString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = sLotId;
            inData["PRODID"] = "E3000";
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_SEL_FAULTY", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보 예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgFaultyInfo.ItemsSource = DataTableConverter.Convert(result);


            });
        }
        private void GetProductProcessQualItem(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            Util.gridClear(dgQulityInfo);

            string sLotId = rowview["LOTID"].ToString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = sLotId;
            inData["PROCID"] = "E3000";
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_QUALITY", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                dgQulityInfo.ItemsSource = DataTableConverter.Convert(result);

            });
        }

        private void GetCommentCombo(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                {
                    return;
                }

                string sLotId = rowview["LOTID"].ToString();
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sLotId), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("CBO_LOTID", typeof(string));

                DataRow newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { sLotId };
                dtResult.Rows.Add(newRow);

                cboLotID.DisplayMemberPath = "CBO_LOTID";
                cboLotID.SelectedValuePath = "CBO_LOTID";

                cboLotID.ItemsSource = DataTableConverter.Convert(dtResult);

                cboLotID.SelectedIndex = 0;
            }
            catch (Exception e)
            {
            }



        }
        #endregion
    }
}
