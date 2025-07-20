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
    public partial class PGM_GUI_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_015()
        {
            InitializeComponent();
            InitCombo();
        }
        string LOTID = null;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        private void InitCombo()
        {

            string[] str = { /*LoginInfo.CONFIG_SHOP_ID, */"E2"/*LoginInfo.CONFIG_AREA_ID*/ };
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: str);

            String[] sFilter = { "E2000" };//Util.R/p
            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cbEquipmentParent, sFilter: sFilter);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //SetWorkOrderWindow();

        }
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
          
        }
        //private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!CanRunComplete())
        //        return;

        //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
        //    {
        //        if (result == MessageBoxResult.OK)
        //        {
        //            if (RunComplete())
        //            {
        //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            }

        //            GetProductLot();
        //        }
        //    });
        //}

        //실적확인버튼
        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된LOTID" + LOTID), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            selectLabel(LOTID);
        }

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
            //GetWorkOrder();//공통모듈추가
            SearchWorkOrder();
            //SearchData();
        }

      

        private void chkWorkOrder_Click(object sender, RoutedEventArgs e) {
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

        /*******************************product lot 체크할때 **************************************************/
        private void chkProductLot_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductLot.CurrentRow.DataItem == null)
            {
                return;
            }
            


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

            DataRowView selectedRow = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem as DataRowView;
            LOTID = selectedRow["LOTID"].ToString();
            GetValidConfirm(LOTID);
            GetProcessActReason(LOTID);
            GetProductProcessQualItem(LOTID);
            GetCommentCombo(LOTID);

        }


        private void btnColorSave_Click(object sender, RoutedEventArgs e)
        {
            //??저장
        }

        //불량정보저장
        private void btnFaultyInfoSave_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("RESNQTY", typeof(int));

            DataRow inData = null;
            DataRowView rowview = null;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgFaultyInfo.Rows)
            {

                rowview = row.DataItem as DataRowView;

                inData = inDataTable.NewRow();

                inData["LOTID"] = LOTID;
                inData["RESNCODE"] = rowview["RESNCODE"].ToString();
                if (rowview["RESNQTY"].ToString().Equals(""))
                {
                    inData["RESNQTY"] = 0;
                }
                else
                {
                    inData["RESNQTY"] = rowview["RESNQTY"];
                }
                inDataTable.Rows.Add(inData);


            }

            new ClientProxy().ExecuteService("DA_PRD_INS_DEFECT", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            });

        }

        private void btnQulityInfoSave_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("CLCTITEM", typeof(string));
            inDataTable.Columns.Add("CLCTAVG", typeof(int));

            DataRow inData = null;
            DataRowView rowview = null;

            //foreach (C1.WPF.DataGrid.DataGridRow row in dgQulityInfo.Rows)
            //{

            //    rowview = row.DataItem as DataRowView;

            //    inData = inDataTable.NewRow();

            //    inData["LOTID"] = LOTID;
            //    inData["CLCTITEM"] = rowview["CLCTITEM"].ToString();
            //    inData["CLCTAVG"] = rowview["CLCTAVG"];

            //    inDataTable.Rows.Add(inData);


            //}

            //new ClientProxy().ExecuteService("DA_PRD_INS_QUALITY", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    if (ex != null)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("품질 정보 저장완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            //});
        }
        #endregion

        #region Mehod
        //private void SetWorkOrderWindow()
        //{
        //    if (grdWorkOrder.Children.Count == 0)
        //    {
        //        //IWorkArea winWorkOrder = obj as IWorkArea;
        //        winWorkOrder.FrameOperation = FrameOperation;

        //        winWorkOrder._UCParent = this;
        //        grdWorkOrder.Children.Add(winWorkOrder);
        //    }
        //}
        //private void GetWorkOrder()
        //{
        //    if (winWorkOrder == null)
        //        return;

        //    winWorkOrder.LINEID = cboEquipmentSegment.SelectedValue.ToString();
        //    winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();

        //    winWorkOrder.GetWorkOrder();
        //}

        //workorder조회
        private void SearchWorkOrder()
        {
            bool statusCheck = false;

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

        //lotid조회
        private void GeProducttLot(Object SelectedItem)
        {

            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            Util.gridClear(dgLotInfo);

            //string sWorkOrder = rowview["WO"].ToString();
            string sWorkOrder = "1K16810CT001";
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sWorkOrder), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);


            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("WORKORDER", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["WORKORDER"] = sWorkOrder;
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRO_SEL_PRODUCTLOT", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
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
      
        //실적확인조회
        private void GetValidConfirm(string lotid)
        {

            if (lotid == null) {
                return;
            }
            //DataRowView rowview = SelectedItem as DataRowView;

            //if (rowview == null)
            //{
            //    return;
            //}

            Util.gridClear(dgLotInfo);

            DataRow row = null;


            //string sLotId = rowview["LOTID"].ToString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = LOTID;
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_SEL_LOTINFO", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
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
                txtDepth.Text = row["DEPTH"].ToString();
                txtWorkStartTime.Text = row["STARTTIME"].ToString();
                txtWorkStartTime.IsReadOnly = true;
                txtFinishTime.Text = row["ENDTIME"].ToString();
                txtOperationTime.Text = row["WORKINGTIME"].ToString();
                txtOperationTime.IsReadOnly = true;

                txtLotComment.Text = row["LOTCOMMENT"].ToString();
                txtLotComment.IsReadOnly = true;


            });

        }

        //불량정보조회
        private void GetProcessActReason(string lotid)
        {

            if (lotid == null) {
                return;
            }

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = lotid;
            inData["PRODID"] = "E3000";
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT_RP", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
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
        //품질정보조회
        private void GetProductProcessQualItem(string lotid)
        {
            //DataRowView rowview = SelectedItem as DataRowView;

            //if (rowview == null)
            //{
            //    return;
            //}

            //Util.gridClear(dgQulityInfo);

            //string sLotId = rowview["LOTID"].ToString();

            if (lotid == null) {
                return;
            }
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = lotid;
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

             //   dgTopQulity.ItemsSource = DataTableConverter.Convert(result);
                








            });
        }

        private void GetCommentCombo(string lotId)
        {
            try
            {
                if (lotId == null) {
                    return;
                }


                DataTable dtResult = new DataTable();
                dtResult.Columns.Add("CBO_LOTID", typeof(string));

                DataRow newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { lotId };
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
        private void selectLabel(string lotId) {

            if (lotId == null)
            {
                return;
            }

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = lotId;
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("BR_PRD_GET_PROCESS_LOT_LABEL_RP", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataRow row = result.Rows[0];
                string resultLotid = row["LOTID"].ToString();
                string resultQTY = row["QTY"].ToString();
                string resultMODEL = row["MODEL"].ToString();
                string resultEQPT = row["EQUPTNUM"].ToString();
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("LOTID : " + resultLotid+ ", 수량 :" + resultQTY), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            });
        }

       
    }


        #endregion

 }