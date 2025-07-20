/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 정문교C
   Decription : Winding 초소형 공정 착공
--------------------------------------------------------------------------------------
 [Change History]
   2017.03.02   INS 정문교C : Initial Created.
   2017.07.03   INS 이대영D : UI 수정
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Forms;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_004_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string lineID = string.Empty;        // Line
        private string procID = string.Empty;        // 공정코드
        private string prodID = string.Empty;        // Work Order 그리드의 제품ID
        private string elecTrodeType = string.Empty; // 극성
        private string lotID = string.Empty;         // LOTID
        private string eqptID = string.Empty;        // 설비코드
        private string eqptName = string.Empty;
        
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

        public ASSY002_004_RUNSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;
            lineID = tmps[1] as string;
            eqptID = tmps[2] as string;
            eqptName = tmps[3] as string;
            lotID = tmps[4] as string;

            // SET COMMON
            txtEquipment.Text = eqptName;

            // SET WORKORDER
            DataRow workOrder = tmps[5] as DataRow;   // WORKORDER

            if (workOrder == null)
                return;

            DataTable workOrderBind = new DataTable();
            workOrderBind.Columns.Add("WOID", typeof(string));          // W/O
            workOrderBind.Columns.Add("WO_DETL_ID", typeof(string));    // W/O 상세
            workOrderBind.Columns.Add("LOTYNAME", typeof(string));      // W/O Type
            workOrderBind.Columns.Add("PRJT_NAME", typeof(string));     // 프로젝트명
            workOrderBind.Columns.Add("PROD_VER_CODE", typeof(string)); // 버전
            workOrderBind.Columns.Add("PRODID", typeof(string));        // 제품ID
            workOrderBind.Columns.Add("PRODNAME", typeof(string));      // 제품명
            workOrderBind.Columns.Add("INPUT_QTY", typeof(string));     // 계획수량

            DataRow newRow = workOrderBind.NewRow();

            newRow["WOID"] = workOrder["WOID"];
            newRow["WO_DETL_ID"] = workOrder["WO_DETL_ID"];
            newRow["LOTYNAME"] = workOrder["LOTYNAME"];
            newRow["PRJT_NAME"] = workOrder["PRJT_NAME"];
            newRow["PROD_VER_CODE"] = workOrder["PROD_VER_CODE"];
            newRow["PRODID"] = workOrder["PRODID"];
            newRow["PRODNAME"] = workOrder["PRODNAME"];
            newRow["INPUT_QTY"] = workOrder["INPUT_QTY"];

            workOrderBind.Rows.Add(newRow);

            dgWorkOrder.ItemsSource = DataTableConverter.Convert(workOrderBind);

            DataTable dt = new DataTable(); // 투입반제품 그리드에 바인딩할 Data
            GetGdInputProduct(ref dt);

            // SET INPUTPRODUCT
            DataTable cathodeBind = new DataTable();
            cathodeBind.Columns.Add("ELECTRODECODE", typeof(string));
            cathodeBind.Columns.Add("EQPT_MOUNT_PSTN_NAME", typeof(string));
            cathodeBind.Columns.Add("PRDT_CLSS_CODE", typeof(string));
            cathodeBind.Columns.Add("LOTID", typeof(string));
            cathodeBind.Columns.Add("PRODID", typeof(string));
            cathodeBind.Columns.Add("PRODNAME", typeof(string));
            cathodeBind.Columns.Add("WIPQTY", typeof(Decimal));
            cathodeBind.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            cathodeBind.Columns.Add("PRJT_NAME", typeof(string));
            cathodeBind.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i == 0)
                {
                    newRow = cathodeBind.NewRow();

                    newRow["ELECTRODECODE"] = "C";
                    newRow["EQPT_MOUNT_PSTN_NAME"] = dt.Rows[i]["EQPT_MOUNT_PSTN_NAME"];
                    newRow["PRDT_CLSS_CODE"] = dt.Rows[i]["PRDT_CLSS_CODE"];
                    newRow["LOTID"] = dt.Rows[i]["INPUT_LOTID"];
                    newRow["PRODID"] = dt.Rows[i]["MTRLID"];
                    newRow["PRODNAME"] = dt.Rows[i]["MTRLNAME"];
                    newRow["WIPQTY"] = dt.Rows[i]["INPUT_QTY"];
                    newRow["EQPT_MOUNT_PSTN_ID"] = dt.Rows[i]["EQPT_MOUNT_PSTN_ID"];
                    newRow["PRJT_NAME"] = dt.Rows[i]["PRJT_NAME"];
                    newRow["MOUNT_MTRL_TYPE_CODE"] = dt.Rows[i]["MOUNT_MTRL_TYPE_CODE"];

                    cathodeBind.Rows.Add(newRow);
                }
                else
                {
                    newRow = cathodeBind.NewRow();

                    newRow["ELECTRODECODE"] = "A";
                    newRow["EQPT_MOUNT_PSTN_NAME"] = dt.Rows[i]["EQPT_MOUNT_PSTN_NAME"];
                    newRow["PRDT_CLSS_CODE"] = dt.Rows[i]["PRDT_CLSS_CODE"];
                    newRow["LOTID"] = dt.Rows[i]["INPUT_LOTID"];
                    newRow["PRODID"] = dt.Rows[i]["MTRLID"];
                    newRow["PRODNAME"] = dt.Rows[i]["MTRLNAME"];
                    newRow["WIPQTY"] = dt.Rows[i]["INPUT_QTY"];
                    newRow["EQPT_MOUNT_PSTN_ID"] = dt.Rows[i]["EQPT_MOUNT_PSTN_ID"];
                    newRow["PRJT_NAME"] = dt.Rows[i]["PRJT_NAME"];
                    newRow["MOUNT_MTRL_TYPE_CODE"] = dt.Rows[i]["MOUNT_MTRL_TYPE_CODE"];
                    cathodeBind.Rows.Add(newRow);
                }
            }

            dgInputProduct.ItemsSource = DataTableConverter.Convert(AddVisibilityColumn(cathodeBind));
        }
        private static DataTable AddVisibilityColumn(DataTable dt)
        {
            var dtBinding = dt.Copy();
         
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "VisibilityButton", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "INDEX", DataType = typeof(string) });

            int tabIdx = 0;
            foreach (DataRow row in dtBinding.Rows)
            {
                if (row["MOUNT_MTRL_TYPE_CODE"].GetString() == "PROD_MTRL")
                {
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




        private DataTable GetGdInputProduct(ref DataTable selectDt)
        {
            Util.gridClear(dgInputProduct);

            string sBizName = "DA_PRD_SEL_CURR_PROD_IN_LOT_WN";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["EQPTID"] = Util.NVC(eqptID);

            inDataTable.Rows.Add(inDataRow);

            selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);
            return selectDt;
        }
        #endregion

        #region [DataGrid] - btnSearch_Click

        private void txtLotId_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)   // BARCODE 입력 시
            {
                System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;

                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((System.Windows.FrameworkElement)(((System.Windows.FrameworkElement)sender).Parent)).Parent).Row.Index;

                if (Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRDT_CLSS_CODE")) == string.Empty)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(txtBox.Text.Trim()))
                {
                    //LOTID가 10자리일 경우 
                    if (txtBox.Text.Trim().Length == 10)
                    {
                        DataTable dt = this.GetHalfProductList(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "ELECTRODECODE")), txtBox.Text.Trim());

                        if (dt != null && dt.Rows.Count == 1)
                        {
                            // SET VALUE
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "LOTID", Convert.ToString(dt.Rows[0]["LOTID"]));
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODID", Convert.ToString(dt.Rows[0]["PRODID"]));
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODNAME", Convert.ToString(dt.Rows[0]["PRODNAME"]));
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "WIPQTY", Convert.ToString(dt.Rows[0]["WIPQTY"]));
                            DataTableConverter.SetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRJT_NAME", Convert.ToString(dt.Rows[0]["PRJT_NAME"]));
                            return;
                        }
                    }
                }

                CMM_WAITING_PANCAKE_SEARCH _InputProduct = new CMM_WAITING_PANCAKE_SEARCH();
                _InputProduct.FrameOperation = FrameOperation;

                if (_InputProduct != null)
                {
                    // SET PARAMETER
                    object[] parameters = new object[11];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[0].DataItem, "WOID"));
                    parameters[1] = Util.NVC(lineID);
                    parameters[2] = Util.NVC(procID);
                    parameters[3] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "ELECTRODECODE"));
                    parameters[4] = "1";    // Winding 공정은 이 시점에서는 양/음극 하나만 선택 가능(???)
                    parameters[5] = Util.NVC(txtBox.Text.Trim()); // 기능추가로 LOTID 추가
                    parameters[6] = "D";
                    parameters[7] = Util.NVC(eqptID);
                    parameters[8] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODID"));
                    parameters[9] = true;
                    parameters[10] = "Y"; //엔터/조회버튼 클릭여부 
                    C1WindowExtension.SetParameters(_InputProduct, parameters);

                    _InputProduct.Closed += new EventHandler(InputHalfProduct_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => _InputProduct.ShowModal()));
                    /*
                    //_InputProduct.ShowModal();
                    //_InputProduct.CenterOnScreen();
                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(_InputProduct);
                            _InputProduct.BringToFront();
                            break;
                        }
                    }
                    */
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Grid Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            CMM_WAITING_PANCAKE_SEARCH _InputProduct = new CMM_WAITING_PANCAKE_SEARCH();
            _InputProduct.FrameOperation = FrameOperation;

            if (_InputProduct != null)
            {
                DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView; // 돋보기 버튼 클릭한 Row
                string sElectrodeCode = dataRow.Row["ELECTRODECODE"].ToString();
                int rowIndex = -1;

                for (int i = 0; i < dgInputProduct.Rows.Count; i++)
                {
                    if (string.Equals(sElectrodeCode, dgInputProduct[i, 0].Text))
                    {
                        rowIndex = i;
                        break;
                    }
                }

                // SET PARAMETER
                object[] parameters = new object[11];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[0].DataItem, "WOID"));
                parameters[1] = Util.NVC(lineID); // Line
                parameters[2] = Util.NVC(procID); // 공정코드
                parameters[3] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "ELECTRODECODE"));
                parameters[4] = "1";    // Winding 공정은 이 시점에서는 양/음극 하나만 선택 가능(???)
                parameters[5] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "LOTID")); // 기능추가로 LOTID 추가
                parameters[6] = "D"; // 페이지 구분자
                parameters[7] = Util.NVC(eqptID); // 설비코드
                parameters[8] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[rowIndex].DataItem, "PRODID")); // 제품ID
                parameters[9] = true; //초소형 구분
                parameters[10] = "N"; //엔터/조회버튼 클릭여부 
                C1WindowExtension.SetParameters(_InputProduct, parameters);

                _InputProduct.Closed += new EventHandler(InputHalfProduct_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _InputProduct.ShowModal()));
                /*
                //_InputProduct.ShowModal();
                //_InputProduct.CenterOnScreen();
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(_InputProduct);
                        _InputProduct.BringToFront();
                        break;
                    }
                }
                */
            }
        }
        private void InputHalfProduct_Closed(object sender, EventArgs e)
        {
            //ASSY002_004_WAITING_PANCAKE_SEARCH _InputProduct = sender as ASSY002_004_WAITING_PANCAKE_SEARCH;
            CMM_WAITING_PANCAKE_SEARCH _InputProduct = sender as CMM_WAITING_PANCAKE_SEARCH;

            DataRow selectRow = null;

            if (_InputProduct.DialogResult == MessageBoxResult.OK)
            {
                selectRow = _InputProduct.SELECTEDROW;

                for (int i = 0; i < dgInputProduct.Rows.Count; i++)
                {
                    if (string.Equals(_InputProduct.ELECTRODETYPE, dgInputProduct[i, 0].Text))
                    {
                        DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "LOTID", Convert.ToString(selectRow["LOTID"]));
                        DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRODID", Convert.ToString(selectRow["PRODID"]));
                        DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRODNAME", Convert.ToString(selectRow["PRODNAME"]));
                        DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "WIPQTY", Convert.ToString(selectRow["WIPQTY"]));
                        DataTableConverter.SetValue(dgInputProduct.Rows[i].DataItem, "PRJT_NAME", Convert.ToString(selectRow["PRJT_NAME"]));
                        break;
                    }
                }
            }

            //this.BringToFront();
        }

        #endregion

        #region [작업시작]
        /// <summary>
        /// 작업시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private DataTable GetEqptSlotList(string sMaterialType)
        {
            // SELECT EQPT SLOT
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_ASSY();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = eqptID;
                Indata["MOUNT_MTRL_TYPE_CODE"] = sMaterialType;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 SLOT 정보가 없습니다.");
                    Util.MessageValidation("SFU1899");
                    return new DataTable();
                }
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetHalfProductList(string sElectrodeType, string sLotId)
        {
            // SELECT HALF PRODUCT
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_HALFPROD_LIST_WN();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = lineID;
                Indata["PROCID"] = procID;

                if (!string.IsNullOrEmpty(sElectrodeType))
                    Indata["ELECTRODETYPE"] = Util.NVC(sElectrodeType);

                Indata["LOTID"] = Util.NVC(sLotId);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HALFPROD_LIST_WN", "INDATA", "RSLTDT", IndataTable);

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_WN";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("EQPT_LOTID", typeof(string));
                inEQP.Columns.Add("WO_DETL_ID", typeof(string));
                inEQP.Columns.Add("LANGID", typeof(string));

                // INPUT PRODUCT SET
                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(eqptID);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["EQPT_LOTID"] = string.Empty;
                newRow["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[0].DataItem, "WO_DETL_ID"));
                newRow["LANGID"] = LoginInfo.LANGID;
                inEQP.Rows.Add(newRow);

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("INPUT_QTY", typeof(Decimal));

                foreach (DataGridRow dRow in dgInputProduct.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LOTID")).ToString() != string.Empty)
                    {
                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LOTID"));
                        newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "WIPQTY")) == "" ? 0 : Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "WIPQTY"));
                        inInput.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
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

            // CHECK INPUT HALFPRODUCT (양극/음극 존재 유무 체크)
            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "LOTID"))))
            {
                //Util.Alert("투입 반제품 정보에 양극 Pancake Lot ID가 없습니다.");
                Util.MessageValidation("SFU1947");
                return false;
            }

            ////if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(gdInputProduct.Rows[0].DataItem, "SLOTNO"))))
            ////{
            ////    Util.Alert("투입 반제품 정보에 양극 Slot No가 지정되지 않았습니다.");
            ////    return false;
            ////}

            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[1].DataItem, "LOTID"))))
            {
                //Util.Alert("투입 반제품 정보에 음극 Pancake Lot ID가 없습니다.");
                Util.MessageValidation("SFU1949");
                return false;
            }

            ////if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(gdInputProduct.Rows[1].DataItem, "SLOTNO"))))
            ////{
            ////    Util.Alert("투입 반제품 정보에 음극 Slot No가 지정되지 않았습니다.");
            ////    return false;
            ////}
            return true;
        }
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
