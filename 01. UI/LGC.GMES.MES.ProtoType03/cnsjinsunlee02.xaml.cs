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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType03
{
    public partial class cnsjinsunlee02 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private BizDataSet _Biz = new BizDataSet();
        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public cnsjinsunlee02()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void initCombo()
        {
            string[] str = { "A2" };//LoginInfo.CFG_AREA_ID
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: str);

            String[] sFilter = { Process.ROLL_PRESSING };
            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cbEquipmentParent, sFilter: sFilter);

        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initCombo();
            SetWorkOrderWindow();
       
    }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라인을 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("설비를 선택 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            if (!(bool)chkRun.IsChecked && !(bool)chkEqpEnd.IsChecked && !(bool)chkWait.IsChecked)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택 조건을 하나 이상 선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

           GetWorkOrder();
           GetProductLot();

        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as CheckBox).IsChecked.HasValue && !(bool)(sender as CheckBox).IsChecked)
            {
                ClearGrid();
                return;
            }

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (rowIndex != i)
                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null
                        && (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

            }

            ClearLotInfo();
            ProcessDetail(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);

           
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ClearLotInfo();
            ClearGrid();
            
        }
        private void dgDefectInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Index == 0) {
                return;
            }

            double sum = SumDefectQty(e.Cell.Column.Index);

            double inputqty = 0;
            int iRow = -1;

            //상태가wait가 아닐때 추가 해줘야함

            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                if (e.Cell.Column.Header.ToString().IndexOf(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"))) > 0)
                {
                    inputqty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY")));
                    iRow = i;
                    break;
                }
            }

            if (inputqty < sum)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생산량보다 불량이 크게 입력 될 수 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);
                return;
            }

            DataTableConverter.SetValue(dgLotInfo.Rows[iRow].DataItem, "LOSSQTY", sum);

            SetLotInfoCalc();

            //if (.STATUS.Equals("WAIT"))
            //{
            //    int iRow = -1;
            //    double inputqty = 0;

            //    for (int i = 0; i < dgDetail.Rows.Count; i++)
            //    {
            //        if (e.Cell.Column.Header.ToString().IndexOf(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"))) > 0)
            //        {
            //            inputqty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
            //            iRow = i;
            //            break;
            //        }
            //    }

            //    if (inputqty < sum)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생산량보다 불량이 크게 입력 될 수 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);
            //        return;
            //    }

            //    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "LOSSQTY", sum);

            //    SetLotInfoCalc();
            //}
        }

        #endregion

        #region Mehod
        private void GetWorkOrder()
        {
            //if (winWorkOrder == null)
            //    return;

            //winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            //winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            //winWorkOrder.PROCID = LoginInfo.CFG_PROC_ID;

            //winWorkOrder.GetWorkOrder();
        }
        private void GetProductLot(DataRow dgWorkorderInfo = null)
        {
            Util.gridClear(dgProductLot);

            string sInQuery = string.Empty;

            if ((bool)chkWait.IsChecked)
                sInQuery = "'WAIT'";

            if ((bool)chkRun.IsChecked)
            {
                if (sInQuery.Equals(""))
                    sInQuery = "'PROC'";
                else
                    sInQuery = sInQuery + ", 'PROC'";
            }

            if ((bool)chkEqpEnd.IsChecked)
            {
                if (sInQuery.Equals(""))
                    sInQuery = "'EQPEND'";
                else
                    sInQuery = sInQuery + ", 'EQPEND'";
            }

            DataTable inTable = new DataTable();

            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("WOID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("WIPSTAT", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["PROCID"] = LoginInfo.CFG_PROC_ID;
            newRow["WOID"] = "aa";/*dgWorkOrder.Columns["WO"].ToString();*/
            newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
            newRow["WIPSTAT"] = sInQuery;

            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_PRODUCTLOT_REWINDER", "INDATA", "OUTDATA", inTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    dgProductLot.ItemsSource = DataTableConverter.Convert(result);
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }
            );


        }
        private void ProcessDetail(object selectItem)
        {
            DataRowView rowview = selectItem as DataRowView;

            if (rowview == null)
                return;

            if (!rowview["STAT"].Equals("WAIT"))
            {
                if (rowview["STAT"].Equals("EQPEND"))
                {
                   FillLotInfo(rowview);
                }
                GetDetailData(rowview);
                GetDefectInfo(rowview);
                GetQualityProcess(rowview);
                // GetEqpFaultyData(rowview);
                //Get_QualityData(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
               // GetRemark(rowview);

            }
        }
        private void GetDefectInfo(object selectItem)
        {
            DataRowView rowview = selectItem as DataRowView;

            if (rowview == null)
                return;


            DataTable inTable = _Biz.GetDA_PROD_SEL_ACTVITYREASON_CODE();

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["PROCID"] = LoginInfo.CFG_PROC_ID;

            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_DEFECT_TEST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    dgDefectInfo.ItemsSource = DataTableConverter.Convert(searchResult);

                    // Defect Column 생성..
                    if (dgDefectInfo.Rows.Count > 0)
                    {
                        // 기존 추가된 Col 삭제..                
                        for (int i = dgDefectInfo.Columns.Count; i-- > 0;)
                        {
                            //DataTableConverter.GetValue(dgFaulty.Columns[i].Header, "PR_LOTID").ToString();
                            if (dgDefectInfo.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                                //if(dgFaulty.Columns[i].Header.ToString().StartsWith("DEFECTQTY"))
                                dgDefectInfo.Columns.RemoveAt(i);
                        }

                        double DefectSum = 0;

                        for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                        {
                            string sColName = "DEFECTQTY" + (i + 1).ToString();

                            Util.SetGridColumnNumeric(dgDefectInfo, sColName, null, Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")), true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                            if (dgDefectInfo.Rows.Count == 0) continue;

                            DataTable dt = GetDefectDataByLot(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));//자식으로 찾은 데이터 테이블 나올것임
                            if (dt != null)
                            {
                                for (int j = 0; j < dt.Rows.Count; j++)
                                {
                                    if (dt.Rows[j]["RESNQTY"].ToString().Equals(""))
                                    {
                                        BindingDataGrid(dgDefectInfo, j, dgDefectInfo.Columns.Count, 0);
                                    }
                                    else
                                    {
                                        BindingDataGrid(dgDefectInfo, j, dgDefectInfo.Columns.Count, dt.Rows[j]["RESNQTY"]);
                                    }
                                    
                                    ////DataTableConverter.SetValue(dgFaulty.Rows[j].DataItem, sColName, dt.Rows[j]["RESNQTY"].ToString());
                                }
                            }
                            //lotinfo gird다시 셋팅
                            DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY", SumDefectQty(i+2).ToString());//lotinfo에 loss설정
                                                                                                          // Util.Alert(Defect);
                                                                                                          //  BindingDataGrid(dgDefectInfo, 0, dgDefectInfo.Columns.Count, 123);
                                                                                                          // DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY", 123);
                            SetLotInfoCalc();//lotinfo에 goodqty설정
                        }
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }

            );

        }
        private void GetDetailData(object selectItem)
        {
            DataRowView rowview = selectItem as DataRowView;

            if (rowview == null)
            {
                    return;
            }


            DataTable dtMain = new DataTable();
            dtMain.Columns.Add("LOTID", typeof(string));

            DataRow row = dtMain.NewRow();
            row["LOTID"] = "?";
            dtMain.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_REWINDER_TEST", "INDATA", "RSLTDT", dtMain);
            dgLotInfo.ItemsSource = DataTableConverter.Convert(result);

            SetRemarkCombo();

        }
       
        private DataTable GetDefectDataByLot(string LotId)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = LotId;
                inDataTable.Rows.Add(newRow);
              
                 DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_DEFECT_QTY_TEST", "INDATA", "RSLTDT", inDataTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void BindingDataGrid(C1.WPF.DataGrid.C1DataGrid datagrid, int iRow, int iCol, object sValue)
        {
            if (datagrid.ItemsSource == null)
                return;
            else
            {
               // Util.Alert("cc");

                DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
                if (dt.Columns.Count < datagrid.Columns.Count)
                {
                    for (int i = dt.Columns.Count; i < datagrid.Columns.Count; i++)
                    {
                        dt.Columns.Add(datagrid.Columns[i].Name);
                    }
                }
                if (sValue.Equals("") || sValue.Equals(null))
                {
                    sValue = 0;
                }
                dt.Rows[iRow][iCol - 1] = sValue;

                datagrid.BeginEdit();
                datagrid.ItemsSource = DataTableConverter.Convert(dt);
                datagrid.EndEdit();
            }
        }
        private void FillLotInfo(DataRowView rowview)
        {
            if (rowview == null)
            {
                return;
            }

            /******************** 상세 정보 Set... ******************/
            txtWorkorder.Text = rowview["LOTID"].ToString();
            //txtWorkseq.Text = rowview["OPERCODE"].ToString();
            txtLotStatus.Text = rowview["STAT"].ToString();

            txtStartTime.Text = rowview["STARTTIME"].ToString();

            txtEndTime.Text = rowview["ENDTIME"].ToString();
            if (!Util.NVC(txtEndTime.Text).Equals("") && !Util.NVC(txtStartTime.Text).Equals(""))
            {
                txtOperationTime.Text = Math.Truncate(Convert.ToDateTime(txtEndTime.Text).Subtract(Convert.ToDateTime(txtStartTime.Text)).TotalMinutes).ToString();
            }
                
        }

        private void ClearLotInfo()
        {
            txtWorkorder.Text = "";
            txtLotStatus.Text = "";
            txtVersion.Text = "";
            txtWorkDay.Text = "";
            txtInputQty.Text = "";
            txtParentInputLot.Text = "";
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            txtOperationTime.Text = "";
            txtLotComment.Text = "";
        }

        private void ClearGrid()
        {
            dgLotInfo.ItemsSource = null;
            dgDefectInfo.ItemsSource = null;
            for (int i = dgDefectInfo.Columns.Count; i-- > 0;)
            {
                if (dgDefectInfo.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                    dgDefectInfo.Columns.RemoveAt(i);
            }
            dgTopQulity.ItemsSource = null;
            dgBottomQulity.ItemsSource = null;

            //Util.gridClear(dgLotInfo);
            // Util.gridClear(dgDefectInfo);
            //Util.gridClear(dgEqpFaulty);
        }
        private void GetQualityProcess(object selectedItem)
        {
            DataRowView rowview = selectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LOTID"] = rowview["LOTID"];
            inData["PROCID"] = Process.ROLL_PRESSING;//??리와인더
            inDataTable.Rows.Add(inData);

            new ClientProxy().ExecuteService("DA_PRD_QUALITY", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    dgTopQulity.ItemsSource = DataTableConverter.Convert(searchResult);

                    // Defect Column 생성..
                    if (dgTopQulity.Rows.Count > 0)
                    {
                        // 기존 추가된 Col 삭제..                
                        for (int i = dgTopQulity.Columns.Count; i-- > 0;)
                        {
                            if (dgTopQulity.Columns[i].Name.ToString().StartsWith("CLCTAVG"))
                                dgTopQulity.Columns.RemoveAt(i);
                        }


                        for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                        {
                            string sColName = "CLCTAVG" + (i + 1).ToString();

                            Util.SetGridColumnNumeric(dgTopQulity, sColName, null, Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")), true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible);

                            if (dgTopQulity.Rows.Count == 0) continue;

                           // DataTable dt = GetDefectDataByLot(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));//자식으로 찾은 데이터 테이블 나올것임
                            //if (dt != null)
                            //{
                            //    for (int j = 0; j < dt.Rows.Count; j++)
                            //    {
                            //        if (dt.Rows[j]["CLCTAVG"].ToString().Equals(""))
                            //        {
                            //            BindingDataGrid(dgTopQulity, j, dgTopQulity.Columns.Count, 0);
                            //        }
                            //        else
                            //        {
                            //            BindingDataGrid(dgTopQulity, j, dgTopQulity.Columns.Count, dt.Rows[j]["CLCTAVG"]);
                            //        }

                            //        ////DataTableConverter.SetValue(dgFaulty.Rows[j].DataItem, sColName, dt.Rows[j]["RESNQTY"].ToString());
                            //    }
                            //}
                        
                        }
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }

         );

            //DataTable inDataTable = new DataTable();
            //inDataTable.Columns.Add("LOTID", typeof(string));
            //inDataTable.Columns.Add("PROCID", typeof(string));

            //DataRow inData = inDataTable.NewRow();
            //inData["LOTID"] = rowview["LOTID"];
            //inData["PROCID"] = Process.ROLL_PRESSING;//??리와인더
            //inDataTable.Rows.Add(inData);

            //new ClientProxy().ExecuteService("DA_PRD_QUALITY", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    if (ex != null)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    dgTopQulity.ItemsSource = DataTableConverter.Convert(result);
            //    dgBottomQulity.ItemsSource = DataTableConverter.Convert(result);
            //});
        }
        private void SetLotInfoCalc()
        {
            if (dgLotInfo.ItemsSource == null)
                return;

            double inputQty = 0;
            double goodQty = 0;
            double lossQty = 0;

            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUTQTY")));
                lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOSSQTY")));
                goodQty = inputQty - lossQty;

                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "GOODQTY", goodQty);
            }
        }
        private void setLotInfo(string lotid, double inputQty, double lossQty)
        {

            double GOODQTY = inputQty - lossQty;

            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("INPUTQTY", typeof(string));
            dt.Columns.Add("GOODQTY", typeof(string));
            dt.Columns.Add("LOSSQTY", typeof(string));

            DataRow row = dt.NewRow();
            row["LOTID"] = lotid;
            row["INPUTQTY"] = inputQty;
            row["GOODQTY"] = GOODQTY;
            row["lossQty"] = lossQty;

            dt.Rows.Add(row);
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
        }
        //private void dgDefectInfo_KeyUp(object sender, KeyEventArgs e)
        //{
        //    int lotInfoIndex = dgDefectInfo.CurrentCell.Column.Index - 2;

        //    if (lotInfoIndex < 0)
        //    {
        //        return;
        //    }
        //    if (Convert.ToString(dgDefectInfo.CurrentCell.Column.Name).StartsWith("DEFECTQTY"))
        //    {
        //        TextBox txt = e.OriginalSource as TextBox;
        //        // Util.Alert("지금 눌려지는 column의 인덱스" + dgDefectInfo.CurrentCell.Column.Index.ToString());

        //        double inputQty = double.Parse(DataTableConverter.GetValue(dgLotInfo.Rows[lotInfoIndex].DataItem, "INPUTQTY").ToString());
        //        double lossQty = double.Parse(SumDefectQty(dgDefectInfo.CurrentCell.Column.Index).ToString());

        //        DataTableConverter.SetValue(dgLotInfo.Rows[lotInfoIndex].DataItem, "LOSSQTY", lossQty);
        //        DataTableConverter.SetValue(dgLotInfo.Rows[lotInfoIndex].DataItem, "GOODQTY", inputQty - lossQty);

        //        //cLot.LOSSQTY = SumDefectQty(i).ToString();
        //        ////if (cLot.STATUS == "EQPEND")
        //        ////{
        //        ////    if (Double.Parse(cLot.INPUTQTY) < Double.Parse(cLot.LOSSQTY))
        //        ////    {
        //        ////        Util.Alert("생산량보다 불량이 크게 입력될 수 없습니다.");
        //        ////        return;
        //        ////    }
        //        ////}

        //        //SetLotInfoGrid(cLot.LOTID, cLot.STATUS);
        //    }
        //}
        private double SumDefectQty(int index)
        {
            double sum = 0;

            for (int i = 0; i < dgDefectInfo.Rows.Count; i++)
            {
                if (!Util.NVC(DataTableConverter.GetValue(dgDefectInfo.Rows[i].DataItem, "DEFECTQTY" + (index - 1).ToString())).Equals(""))
                {
                    sum += double.Parse(DataTableConverter.GetValue(dgDefectInfo.Rows[i].DataItem, "DEFECTQTY" + (index - 1).ToString()).ToString());
                }

            }
            return sum;
        }

        private void SetRemarkCombo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_LOTID");

            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                dt.Rows.Add(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));
            }
            cboLotID.ItemsSource = dt.Copy().AsDataView();
            cboLotID.SelectedIndex = 0;
        }
        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                //IWorkArea winWorkOrder = obj as IWorkArea;
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }

        #endregion

    }
}
