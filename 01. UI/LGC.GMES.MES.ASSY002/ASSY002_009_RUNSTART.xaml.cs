/*************************************************************************************
 Created Date : 2017.11.01
      Creator : 신 광희
   Decription : Washing 재작업 공정진척(PALLET) 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2017.07.28   신광희C
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_009_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration

        private readonly Util _util = new Util();

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

        public ASSY002_009_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void ASSY002_009_RUNSTART_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;
            _equipmentSegmentCode = tmps[1] as string;
            _equipmentCode = tmps[2] as string;
            _equipmentName = tmps[3] as string;
            DataRow workOrder = tmps[4] as DataRow;
            if (_equipmentName != null) txtEquipment.Text = _equipmentName;

            SetWorkOrderInfo(workOrder);
            GetInputProduct();
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
        }

        private void chkCreate_Unchecked(object sender, RoutedEventArgs e)
        {
            txtLotID.IsEnabled = true;
            btnSearch.IsEnabled = true;
            dgInputProduct.IsEnabled = true;
        }

        private void btnTraySearch_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH popProductSearch = new CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH
            {
                FrameOperation = FrameOperation
            };

            dgInput.SelectedItem = ((FrameworkElement)sender).DataContext;
            object[] parameters = new object[5];
            parameters[0] = Util.NVC(_processCode);                                                                       // 공정코드
            parameters[1] = Util.NVC(_equipmentSegmentCode);                                                              // Line
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgInput.SelectedItem, "INPUT_LOTID"));                   // LOT ID
            parameters[3] = "N";  //엔터/조회버튼 클릭여부 
            parameters[4] = _equipmentCode;
            C1WindowExtension.SetParameters(popProductSearch, parameters);

            popProductSearch.Closed += new EventHandler(popProductSearch_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popProductSearch.ShowModal()));
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
                    txtLotID.IsEnabled = true;
                    btnSearch.IsEnabled = true;
                    dgInputProduct.IsEnabled = true;
                    txtNewLotID.Text = string.Empty;
                    chkCreate.IsChecked = false;
                    txtLotID.Text = Convert.ToString(selectRow["LOTID_RT"]);
                    GetWashingRunList();
                }
            }
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

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add(string.IsNullOrEmpty(txtLotID.Text.Trim()) ? "EQSGID" : "LOTID", typeof(string));

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
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

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INPUT_LOTID"] = inputLotId;
                newRow["EQPT_LOTID"] = string.Empty;
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
                        newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INPUT_QTY")) == string.Empty ? 0 : DataTableConverter.GetValue(dRow.DataItem, "INPUT_QTY").GetDecimal();
                        inInput.Rows.Add(newRow);
                    }
                }

                string xml = inDataSet.GetXml();

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
