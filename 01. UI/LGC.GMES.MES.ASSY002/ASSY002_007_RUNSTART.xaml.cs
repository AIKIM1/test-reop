/*************************************************************************************
 Created Date : 2017.07.28
      Creator : 신 광희
   Decription : Washing 재작업 공정진척 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2017.07.28   신광희C
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_007_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration

        private readonly Util _util = new Util();

        private DataTable selectDt = null;
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;

      
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

        public ASSY002_007_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void ASSY002_007_RUNSTART_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;
            _equipmentSegmentCode = tmps[1] as string;
            _equipmentCode = tmps[2] as string;
            _equipmentName = tmps[3] as string;
            DataRow workOrder = tmps[4] as DataRow;
            txtEquipment.Text = _equipmentName;

            InitCombo();

            SetWorkOrderInfo(workOrder);
            //GetWashingRunList();
            GetInputProduct();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);
        }

        private void dgInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            /*
            if (selectDt == null || selectDt.Rows.Count < 0)
                return;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowIndex = 0;
                int colIndex = 0;

                if (e.Cell?.Presenter?.Content != null && dgInputProduct.SelectedIndex >= 0)
                {
                    rowIndex = e.Cell.Row.Index;
                    colIndex = e.Cell.Column.Index;

                    int selectedIdx = dgInputProduct.SelectedIndex;

                    if (selectedIdx < 0 || selectDt.Rows.Count < 0 || selectDt == null)
                        return;

                    for (int row = 0; row < selectDt.Rows.Count; row++)
                    {
                        if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "1"))
                        {
                            if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "0"))
                            {
                                dgInputProduct.SelectedIndex = row;
                            }
                        }
                    }
                }
            }));
            */
        }

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgInputProduct.SelectedIndex = idx;

                        selectDt = dtRow.Table;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally { }
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWashingRunList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkCreate_Checked(object sender, RoutedEventArgs e)
        {
            txtLotID.Text = string.Empty;
            txtLotID.IsEnabled = false;
            btnSearch.IsEnabled = false;
            dgInputProduct.IsEnabled = false;

            cboArea.IsEnabled = true;
            cboEquipmentSegment.IsEnabled = true;
            txtNewLotID.IsEnabled = true;
        }

        private void chkCreate_Unchecked(object sender, RoutedEventArgs e)
        {
            txtLotID.IsEnabled = true;
            btnSearch.IsEnabled = true;
            dgInputProduct.IsEnabled = true;

            cboArea.IsEnabled = false;
            cboEquipmentSegment.IsEnabled = false;
            txtNewLotID.Text = string.Empty;
            txtNewLotID.IsEnabled = false;
        }

        private void btnTraySearch_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH popProductSearch = new CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH
            {
                FrameOperation = FrameOperation
            };

            Button btn = sender as Button;
            C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
            System.Collections.Generic.IList<FrameworkElement> ilist = btn.GetAllParents();

            foreach (var item in ilist)
            {
                DataGridRowPresenter presenter = item as DataGridRowPresenter;
                if (presenter != null)
                {
                    row = presenter.Row;
                }
            }
            dgInput.SelectedItem = row.DataItem;

            object[] parameters = new object[5];
            parameters[0] = Util.NVC(_processCode);                                                                       // 공정코드
            parameters[1] = Util.NVC(_equipmentSegmentCode);                                                              // Line
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgInput.SelectedItem, "INPUT_LOTID"));                   // LOT ID
            parameters[3] = "N";  //엔터/조회버튼 클릭여부 
            parameters[4] = _equipmentCode;
            C1WindowExtension.SetParameters(popProductSearch, parameters);

            popProductSearch.Closed += new EventHandler(popProductSearch_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popProductSearch.ShowModal()));
            //foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //{
            //    if (grid.Name == "grdMain")
            //    {
            //        grid.Children.Add(popProductSearch);
            //        popProductSearch.BringToFront();
            //        break;
            //    }
            //}
        }

        private void popProductSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH pop = sender as CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH;

            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                DataRow selectRow = pop.SELECTEDROW;
                DataTableConverter.SetValue(dgInput.SelectedItem, "INPUT_LOTID", Convert.ToString(selectRow["LOTID"]));
                DataTableConverter.SetValue(dgInput.SelectedItem, "MTRLID", Convert.ToString(selectRow["PRODID"]));
                DataTableConverter.SetValue(dgInput.SelectedItem, "MTRLNAME", Convert.ToString(selectRow["PRODNAME"]));
                DataTableConverter.SetValue(dgInput.SelectedItem, "INPUT_QTY", Convert.ToString(selectRow["WIPQTY2"]));

                if (!string.IsNullOrEmpty(selectRow["LOTID_RT"].GetString()))
                {
                    //chkCreate_Unchecked(chkCreate, null);
                    txtLotID.IsEnabled = true;
                    btnSearch.IsEnabled = true;
                    dgInputProduct.IsEnabled = true;
                    txtNewLotID.Text = string.Empty;
                    chkCreate.IsChecked = false;
                    txtLotID.Text = Convert.ToString(selectRow["LOTID_RT"]);
                    GetWashingRunList();

                }
            }

            /*
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    grid.Children.Remove(pop);
                    break;
                }
            }
            */
        }

        #region [작업시작]
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // LOT 착공
                    StartRunProcess();
                }
            });
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void GetFactoryPlanProcessInfo(ref string refValue)
        {
            if (refValue == null) throw new ArgumentNullException(nameof(refValue));

            refValue = "";

            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PROCESS_FP_INFO";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["PROCID"] = _processCode;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "OUTDATA", inDataTable);

                if(!CommonVerify.HasTableRow(dtRslt))
                {
                    return;
                }

                if (Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("REF") && !Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]).Equals(""))
                {
                    refValue = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                }

                return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetWorkOrderInfo(DataRow drworkOrder)
        {
            if (drworkOrder == null) return;

            DataTable dtworkOrder = new DataTable();
            dtworkOrder.Columns.Add("WOID", typeof(string));        // W/O
            dtworkOrder.Columns.Add("WO_DETL_ID", typeof(string));  // W/O 상세
            dtworkOrder.Columns.Add("LOTYNAME", typeof(string));    // W/OTYPE
            dtworkOrder.Columns.Add("PRODID", typeof(string));      // 제품ID
            dtworkOrder.Columns.Add("PRODNAME", typeof(string));    // 제품명
            dtworkOrder.Columns.Add("PRJT_NAME", typeof(string));   // 프로젝트명
            dtworkOrder.Columns.Add("INPUT_QTY", typeof(string));   // 계획수량
            

            DataRow dr = dtworkOrder.NewRow();
            dr["WOID"] = drworkOrder["WOID"];
            dr["WO_DETL_ID"] = drworkOrder["WO_DETL_ID"];
            dr["LOTYNAME"] = drworkOrder["LOTYNAME"];
            dr["PRODID"] = drworkOrder["PRODID"];
            dr["PRODNAME"] = drworkOrder["PRODNAME"];
            dr["PRJT_NAME"] = drworkOrder["PRJT_NAME"];
            dr["INPUT_QTY"] = drworkOrder["INPUT_QTY"];
            dtworkOrder.Rows.Add(dr);

            dgWorkOrder.ItemsSource = DataTableConverter.Convert(dtworkOrder);
        }

        private void GetWashingRunList()
        {
            try
            {

                string bizRuleName = string.IsNullOrEmpty(txtLotID.Text.Trim()) ? "DA_PRD_SEL_LOT_LIST_WS" : "DA_PRD_SEL_LOT_LIST_BY_LOTID_WS";
                Util.gridClear(dgInputProduct);

                //string factoryPlanProcessCode = string.Empty;
                //GetFactoryPlanProcessInfo(ref factoryPlanProcessCode);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));

                if (string.IsNullOrEmpty(txtLotID.Text.Trim()))
                {
                    inDataTable.Columns.Add("EQSGID", typeof(string));
                }
                else
                {
                    inDataTable.Columns.Add("LOTID", typeof(string));
                }

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                //inDataRow["PROCID"] = string.IsNullOrWhiteSpace(factoryPlanProcessCode) ? _processCode : factoryPlanProcessCode;
                inDataRow["PROCID"] = _processCode;

                if (string.IsNullOrEmpty(txtLotID.Text.Trim()))
                {
                    inDataRow["EQSGID"] = Util.NVC(_equipmentSegmentCode);
                }
                else
                {
                    inDataRow["LOTID"] = Util.NVC(txtLotID.Text.TrimEnd());
                }
                    
                inDataTable.Rows.Add(inDataRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                /*
                if(!CommonVerify.HasTableRow(searchResult))
                {
                    //Util.AlertInfo("조회 가능한 투입 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1903");
                    return;
                }
                */

                dgInputProduct.ItemsSource = DataTableConverter.Convert(searchResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputProduct()
        {
            Util.gridClear(dgInput);

            const string bizRuleName = "DA_PRD_SEL_CURR_PROD_IN_LOT_WS";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["EQPTID"] = _equipmentCode;

            inDataTable.Rows.Add(inDataRow);

            DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (CommonVerify.HasTableRow(dt))
            {
                dgInput.ItemsSource = DataTableConverter.Convert(AddVisibilityColumn(dt));
            }
        }

        private static DataTable AddVisibilityColumn(DataTable dt)
        {
            var dtBinding = dt.Copy();
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "IsEnabledText", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "VisibilityButton", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "INDEX", DataType = typeof(string) });

            int tabIdx = 0;
            foreach (DataRow row in dtBinding.Rows)
            {
                if (row["MOUNT_MTRL_TYPE_CODE"].GetString() == "PROD")
                {
                    row["IsEnabledText"] = "true";
                    row["VisibilityButton"] = "Visible";
                }
                else
                {
                    row["IsEnabledText"] = "true";
                    row["VisibilityButton"] = "Collapsed";
                }
                row["INDEX"] = tabIdx;
                tabIdx ++;
            }
            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_WS_FOR_RW"; // Washing 재작업 작업시작

                string inputLotId = chkCreate.IsChecked == true ? txtNewLotID.Text : _util.GetDataGridFirstRowBycheck(dgInputProduct, "CHK").Field<string>("LOTID").GetString();

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_LOTID", typeof(string));
                inTable.Columns.Add("RWK_ORIG_EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INPUT_LOTID"] = inputLotId;
                newRow["EQPT_LOTID"] = string.Empty;

                if (chkCreate.IsChecked == true)
                {
                    newRow["RWK_ORIG_EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                }

                inTable.Rows.Add(newRow);

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("PRODID", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("INPUT_QTY", typeof(Decimal));

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgInput.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INPUT_LOTID")) != string.Empty)
                    {
                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "MTRLID"));
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INPUT_LOTID"));
                        newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INPUT_QTY")) == "" ? 0 : DataTableConverter.GetValue(dRow.DataItem, "INPUT_QTY").GetDecimal();
                        inInput.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_EQP", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        private bool ValidateStartRun()
        {
            if (chkCreate.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtNewLotID.Text))
                {
                    // LOT ID 가 없습니다.
                    Util.MessageInfo("SFU1361");
                    return false; 
                }
                if (txtNewLotID.Text.Length != 10)
                {
                    //SFU1362	LOT ID 형식이 올바르지 않습니다. 확인 후 다시 등록해 주세요.
                    Util.MessageInfo("SFU1362");
                    return false;
                }
            }
            else
            {
                if (!CommonVerify.HasDataGridRow(dgInputProduct))
                {
                    //투입 반제품이 존재하지 않습니다.
                    Util.MessageValidation("SFU1952");
                    return false;
                }

                if (_util.GetDataGridCheckCnt(dgInputProduct, "CHK") == 0)
                {
                    //Util.Alert("선택된 투입 반제품이 존재하지 않습니다.");
                    Util.MessageValidation("SFU1650");
                    return false;
                }
            }

            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInput.Rows[0].DataItem, "INPUT_LOTID"))))
            {
                //Util.Alert("WORK ORDER ID가 지정되지 않거나 없습니다.");
                Util.MessageValidation("SFU4069");
                return false;
            }

            return true;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion
    }
}
