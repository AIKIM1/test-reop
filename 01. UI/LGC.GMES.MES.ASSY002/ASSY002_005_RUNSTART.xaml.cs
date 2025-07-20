/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : Assembly 초소형 공정 착공
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.
   2017.06.12   INS 신광희C : 투입반제품 추가 삭제 버튼 수정 및 대기Pancake 조회 파라메터 수정
   2017.07.03   INS 이대영D : 변경된 BizRule에 따른 UI 및 파라미터 재정의
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_005_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private string _processCode = string.Empty;             // 공정코드
        private string _equipmentSegmentCode = string.Empty;    // Line
        private string _equipmentCode = string.Empty;           // 설비코드
        private string _equipmentName = string.Empty;           // 설비
        private string _emptyValue = string.Empty;              // 빈값
        private string _trayID = string.Empty;                  // TrayID
        private string _lotID = string.Empty;                   // LOTID
        private string _windingRuncardID = string.Empty;        // 이력카드ID

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

        public ASSY002_005_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;           // 공정코드
            _equipmentSegmentCode = tmps[1] as string;  // Line
            _equipmentCode = tmps[2] as string;         // 설비코드
            _equipmentName = tmps[3] as string;         // 설비
            _emptyValue = tmps[4] as string;            // 빈값

            // SET COMMON
            txtEquipment.Text = _equipmentName;

            // SET WORKORDER
            DataRow workOrder = tmps[5] as DataRow;   // WORKORDER

            if (workOrder == null)
                return;

            DataTable workOrderBind = new DataTable();
            workOrderBind.Columns.Add("WOID", typeof(string));       // W/O
            workOrderBind.Columns.Add("WO_DETL_ID", typeof(string)); // W/O 상세
            workOrderBind.Columns.Add("LOTYNAME", typeof(string));   // 계획유형
            workOrderBind.Columns.Add("PRODID", typeof(string));     // 제품ID
            workOrderBind.Columns.Add("PRODNAME", typeof(string));   // 제품명
            workOrderBind.Columns.Add("INPUT_QTY", typeof(string));  // 계획수량

            DataRow newRow = workOrderBind.NewRow();

            newRow["WOID"] = workOrder["WOID"];
            newRow["WO_DETL_ID"] = workOrder["WO_DETL_ID"];
            newRow["LOTYNAME"] = workOrder["LOTYNAME"];
            newRow["PRODID"] = workOrder["PRODID"];
            newRow["PRODNAME"] = workOrder["PRODNAME"];
            newRow["INPUT_QTY"] = workOrder["INPUT_QTY"];

            workOrderBind.Rows.Add(newRow);

            dgWorkOrder.ItemsSource = DataTableConverter.Convert(workOrderBind);

            DataTable dtDgInputProduct = new DataTable(); // 투입반제품 그리드에 바인딩할 Data
            GetDgInputProduct(ref dtDgInputProduct);

            DataTable cathodeBind = new DataTable();
            cathodeBind.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));   // 설비 장착 위치 아이디
            cathodeBind.Columns.Add("EQPT_MOUNT_PSTN_NAME", typeof(string)); // 설비 장착 위치 명
            cathodeBind.Columns.Add("PRDT_CLSS_CODE", typeof(string));       // 극성
            cathodeBind.Columns.Add("CSTID", typeof(string));                // TRAYID
            cathodeBind.Columns.Add("INPUT_LOTID", typeof(string));          // INPUT_LOTID
            cathodeBind.Columns.Add("PRODID", typeof(string));               // 제품ID
            cathodeBind.Columns.Add("PRODNAME", typeof(string));             // 제품명
            cathodeBind.Columns.Add("INPUT_QTY", typeof(Decimal));           // 재공수량
            cathodeBind.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string)); //자재구분


            if (dtDgInputProduct.Rows.Count > 0)
            {
                for (int i = 0; i < dtDgInputProduct.Rows.Count; i++)
                {
                    newRow = cathodeBind.NewRow();

                    newRow["EQPT_MOUNT_PSTN_ID"] = dtDgInputProduct.Rows[i]["EQPT_MOUNT_PSTN_ID"];
                    newRow["EQPT_MOUNT_PSTN_NAME"] = dtDgInputProduct.Rows[i]["EQPT_MOUNT_PSTN_NAME"];
                    newRow["PRDT_CLSS_CODE"] = dtDgInputProduct.Rows[i]["PRDT_CLSS_CODE"];
                    if(dtDgInputProduct.Rows[i]["MOUNT_MTRL_TYPE_CODE"].ToString() == "MTRL")
                    {
                        newRow["CSTID"] = dtDgInputProduct.Rows[i]["INPUT_LOTID"];
                    }
                    else
                    {
                        newRow["CSTID"] = dtDgInputProduct.Rows[i]["CSTID"];
                    }
                   
                    newRow["INPUT_LOTID"] = dtDgInputProduct.Rows[i]["INPUT_LOTID"];
                    newRow["PRODID"] = dtDgInputProduct.Rows[i]["MTRLID"];
                    newRow["PRODNAME"] = dtDgInputProduct.Rows[i]["MTRLNAME"];
                    newRow["INPUT_QTY"] = dtDgInputProduct.Rows[i]["INPUT_QTY"];
                    //newRow["WINDING_RUNCARD_ID"] = dtDgInputProduct.Rows[0]["WINDING_RUNCARD_ID"];
                    newRow["MOUNT_MTRL_TYPE_CODE"] = dtDgInputProduct.Rows[i]["MOUNT_MTRL_TYPE_CODE"];

                    cathodeBind.Rows.Add(newRow);
                }
              
            }
            dgInputProduct.ItemsSource = DataTableConverter.Convert(AddVisibilityColumn(cathodeBind));
        }
        #endregion

        private DataTable GetDgInputProduct(ref DataTable selectDt)
        {
            Util.gridClear(dgInputProduct);

            string sBizName = "DA_PRD_SEL_CURR_PROD_IN_LOT_AS"; // 초소형, 원각 공통 사용

            DataTable inData = new DataTable();
            inData.Columns.Add("LANGID", typeof(string));
            inData.Columns.Add("EQPTID", typeof(string));
             
            DataRow inDataRow = inData.NewRow();
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["EQPTID"] = Util.NVC(_equipmentCode);

            inData.Rows.Add(inDataRow);

            DataSet ds = new DataSet();
            inData.TableName = "RQSTDT";
            ds.Tables.Add(inData);
            string sTestXml = ds.GetXml();

            selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inData);

            return selectDt;
        }

        private static DataTable AddVisibilityColumn(DataTable dt)
        {
            var dtBinding = dt.Copy();
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "VisibilityButton", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "INDEX", DataType = typeof(string) });

            int tabIdx = 0;
            foreach (DataRow row in dtBinding.Rows)
            {
                if (row["MOUNT_MTRL_TYPE_CODE"].GetString() == "MTRL")
                {
                    //자재도 수정이 가능하도록 됨
                    row["VisibilityButton"] = "Collapsed";
                }
                else
                {
                    row["VisibilityButton"] = "Visible";
                }
                row["INDEX"] = tabIdx;
                tabIdx++;
            }
            dtBinding.AcceptChanges();
            return dtBinding;
        }

        #region [DataGrid] - btnSearch_PreviewMouseLeftButtonDown, txtLotId_PreviewKeyDown
        private void btnSearch(object sender, MouseButtonEventArgs e)
        {
            CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH _InputProduct = new CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH();
            _InputProduct.FrameOperation = FrameOperation;

            if (_InputProduct != null)
            {
                DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView; // 돋보기 버튼 클릭한 Row
            }

            // SET PARAMETER
            object[] parameters = new object[4];
            parameters[0] = Util.NVC(_processCode);                                                          // 공정코드
            parameters[1] = Util.NVC(_equipmentSegmentCode);                                                 // Line
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "CSTID")); // TRAYID
            parameters[3] = "N";  //엔터/조회버튼 클릭여부 
            C1WindowExtension.SetParameters(_InputProduct, parameters);

            _InputProduct.Closed += new EventHandler(InputProduct_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => _InputProduct.ShowModal()));
            //_InputProduct.ShowModal();
            //_InputProduct.CenterOnScreen();
            //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //{
            //    if (tmp.Name == "grdMain")
            //    {
            //        tmp.Children.Add(_InputProduct);
            //        _InputProduct.BringToFront();
            //        break;
            //    }
            //}
        }

        private void InputProduct_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH _InputProduct = sender as CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH;

            DataRow selectRow = null;

            if (_InputProduct.DialogResult == MessageBoxResult.OK)
            {
                selectRow = _InputProduct.SELECTEDROW;

                for (int i = 0; i < dgInputProduct.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "INPUT_LOTID", Convert.ToString(selectRow["LOTID"]));                     // LOTID
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "CSTID", Convert.ToString(selectRow["TRAYID"]));                          // Tray
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRODID", Convert.ToString(selectRow["PRODID"]));                         // 제품ID
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRODNAME", Convert.ToString(selectRow["PRODNAME"]));                     // 제품명
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "INPUT_QTY", Convert.ToString(selectRow["WIPQTY2"]));                     // 재공수량
                    //DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "WINDING_RUNCARD_ID", Convert.ToString(selectRow["WINDING_RUNCARD_ID"])); // 이력카드ID
                    break;
                }
            }

            if (!string.IsNullOrEmpty(txtAssemblyLotId.Text))
                txtAssemblyLotId.Text = string.Empty;

            //this.BringToFront();
        }

        private void dgInputProduct_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", true);
            //e.Item.SetValue("WIPQTY", 0);
        }

        private void txtLotId_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)   // BARCODE 입력 시
            {
                System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;

                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((System.Windows.FrameworkElement)(((System.Windows.FrameworkElement)sender).Parent)).Parent).Row.Index;
                if (Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODNAME")) == string.Empty)
                {
                    return;
                }

                DataTable resultDt = new DataTable();

                if (!string.IsNullOrEmpty(txtBox.Text.Trim()))
                {
                    //LOTID가 10자리일 경우 
                    if (txtBox.Text.Trim().Length == 10)
                    {
                        resultDt = this.GetHalfProductList(txtBox.Text.Trim()); // Winding Tray 대리 리스트 조회

                        if (resultDt != null && resultDt.Rows.Count == 1)
                        {
                            // SET VALUE
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "INPUT_LOTID", Convert.ToString(resultDt.Rows[0]["LOTID"])); // LOTID
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "CSTID", Convert.ToString(resultDt.Rows[0]["TRAYID"]));      // TRAYID
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODID", Convert.ToString(resultDt.Rows[0]["PRODID"]));     // 제품ID
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODNAME", Convert.ToString(resultDt.Rows[0]["PRODNAME"])); // 제품명
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "INPUT_QTY", Convert.ToString(resultDt.Rows[0]["WIPQTY2"])); // 재공수량
                                                                                                                                                             //DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "WINDING_RUNCARD_ID", Convert.ToString(resultDt.Rows[0]["WINDING_RUNCARD_ID"]));                                                                                                      // 이력카드ID
                            return;
                        }
                    }
                }

                CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH _InputProduct = new CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH();
                _InputProduct.FrameOperation = FrameOperation;

                if (_InputProduct != null)
                {
                    // SET PARAMETER
                    object[] parameters = new object[4];
                    parameters[0] = _processCode;           // 공정코드
                    parameters[1] = _equipmentSegmentCode;  // Line
                    parameters[2] = Util.NVC(txtBox.Text);  // TRAY ID
                    parameters[3] = "Y";  //엔터/조회버튼 클릭여부 
                    C1WindowExtension.SetParameters(_InputProduct, parameters);

                    _InputProduct.Closed += new EventHandler(InputHalfProduct_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => _InputProduct.ShowModal()));

                    //_InputProduct.ShowModal();
                    //_InputProduct.CenterOnScreen();
                    //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    //{
                    //    if (tmp.Name == "grdMain")
                    //    {
                    //        tmp.Children.Add(_InputProduct);
                    //        _InputProduct.BringToFront();
                    //        break;
                    //    }
                    //}
                }
            }
        }

        private void InputHalfProduct_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH _InputProduct = sender as CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH;

            DataRow selectRow = null;

            if (_InputProduct.DialogResult == MessageBoxResult.OK)
            {
                selectRow = _InputProduct.SELECTEDROW;

                for (int i = 0; i < dgInputProduct.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "INPUT_LOTID", Convert.ToString(selectRow["LOTID"]));                     // LOTID
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "CSTID", Convert.ToString(selectRow["TRAYID"]));                          // TRAYID
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRODID", Convert.ToString(selectRow["PRODID"]));                         // 제품ID
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRODNAME", Convert.ToString(selectRow["PRODNAME"]));                     // 제품명
                    DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "INPUT_QTY", Convert.ToString(selectRow["WIPQTY2"]));                     // 재공수량
                    //DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "WINDING_RUNCARD_ID", Convert.ToString(selectRow["WINDING_RUNCARD_ID"])); // 재공수량
                    break;
                }
            }

            if (!string.IsNullOrEmpty(txtAssemblyLotId.Text))
                txtAssemblyLotId.Text = string.Empty;

            //this.BringToFront();
        }
        #endregion

        #region [발번]
        private void btnGenerateId_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputProduct.Rows.Count == 0)
            {
                //Util.Alert("반제품 정보가 없습니다.");
                Util.MessageValidation("SFU1542");
                return;
            }

            // ASSEMBLY LOT ID GENERATE
            this.SetAssyLotIdGenerate();
        }
        #endregion

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

        private DataTable GetHalfProductList(string sTrayId)
        {
            // SELECT HALF PRODUCT
            try
            {
                string bizRuleName = "DA_PRD_SEL_WINDING_TRAY_WAIT_LIST";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("TRAYID", typeof(string));

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["PROCID"] = _processCode;
                inDataRow["EQSGID"] = _equipmentSegmentCode;
                inDataRow["TRAYID"] = Util.NVC(sTrayId);
                inDataTable.Rows.Add(inDataRow);

                DataTable selectDt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);

                return selectDt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
        }

        private void SetAssyLotIdGenerate()
        {
            // SELECT EQPT SLOT
            txtAssemblyLotId.Text = string.Empty;

            try
            {
                string bizRuleName = "BR_PRD_GET_NEW_PROD_LOTID_ASS";

                DataSet inDataSet = new DataSet();

                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("NEXT_DAY_WORK", typeof(string));

                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["NEXT_DAY_WORK"] = string.Empty;
                inEQP.Rows.Add(newRow);

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                
                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "EQPT_MOUNT_PSTN_ID"));
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "CSTID"));
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "INPUT_LOTID"));
                inInput.Rows.Add(newRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        txtAssemblyLotId.Text = Util.NVC(bizResult.Tables["OUT_LOT"].Rows[0]["PROD_LOTID"]);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally { }
        }

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_REG_START_PROD_LOT_ASS";

                DataSet inDataSet = new DataSet();
                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("PROD_LOTID", typeof(string));
                inEQP.Columns.Add("EQPT_LOTID", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("PRODID", typeof(string));
                inInput.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
                inInput.Columns.Add("INPUT_QTY", typeof(Decimal));

                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(txtAssemblyLotId.Text);
                newRow["EQPT_LOTID"] = string.Empty;

                inEQP.Rows.Add(newRow);

                for (int i = 0; i < dgInputProduct.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "CSTID")).ToString() != string.Empty)
                    {
                        newRow = null;
                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"));
                        //newRow["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "WINDING_RUNCARD_ID"));
                        newRow["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "CSTID"));
                        newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUT_QTY")) == "" ? 0 : Util.NVC_Int(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUT_QTY"));
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

        #region[[Validation]
        private bool ValidateStartRun()
        {
            // CHECK WORKORDER
            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[0].DataItem, "WOID"))))
            {
                //Util.Alert("WORK ORDER ID가 지정되지 않거나 없습니다.");
                Util.MessageValidation("SFU1442");
                return false;
            }

            // CHECK INPUT HALFPRODUCT
            if (dgInputProduct.Rows.Count == 0)
            {
                //Util.Alert("투입 반제품이 존재하지 않습니다.");
                Util.MessageValidation("SFU1952");
                return false;
            }

            for (int i = 0; i < dgInputProduct.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")) == "PROD")
                {
                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "INPUT_LOTID"))))
                    {
                        //Util.Alert("투입 반제품 정보에 LOT ID가 없습니다.");
                        Util.MessageValidation("SFU1946");
                        dgInputProduct.SelectedIndex = i;
                        return false;
                    }

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[i].DataItem, "PRODID"))))
                    {
                        //Util.Alert("투입 반제품 정보에 제품ID가 없습니다.");
                        Util.MessageValidation("SFU1950");
                        dgInputProduct.SelectedIndex = i;
                        return false;
                    }
                }
            }

            // CHECK ASSEMBLY LOT ID
            if (string.IsNullOrEmpty(txtAssemblyLotId.Text))
            {
                //Util.Alert("ASSEMBLY LOT ID가 발번되지 않았습니다.");
                Util.MessageValidation("SFU1304");
                return false;
            }

            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtAssemblyLotId.Text.ToUpper(), @"^[a-zA-Z0-9]+$");
            if (!chk || txtAssemblyLotId.Text.Length != 10)
            {
                Util.MessageInfo("SFU4045", (result) =>
                {
                    txtAssemblyLotId.SelectAll();
                    txtAssemblyLotId.Focus();
                });
                return false;
            }

            return true;
        }

        //private bool ValidationDupplicateLot(C1DataGrid dg, string lotId)
        //{
        //    if (CommonVerify.HasDataGridRow(dg))
        //    {
        //        DataTable dt = ((DataView)dg.ItemsSource).Table;
        //        var query = (from t in dt.AsEnumerable()
        //                     where t.Field<string>("LOTID") == lotId
        //                     select t).ToList();

        //        if (query.Any())
        //        {
        //            Util.MessageValidation("SFU2051", lotId);
        //            return false;
        //        }
        //        return true;
        //    }
        //    return true;
        //}

        #endregion

        #region [Func]
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

        #endregion
    }
}
