/*************************************************************************************
 Created Date : 2018.03.22
      Creator : 정문교
   Decription : 투입 잔량 처리
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_INPUT_REMAIN : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;          // 공정코드
        private string _procName = string.Empty;        // 공정명
        private string _eqptID = string.Empty;          // 설비코드
        private string _prodLotID = string.Empty;       // 생산LOT
        private DataTable _inboxList;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public int DifferenceQty { get; set; }
        public string EquipmentSegmentCode { get; set; }

        public bool ConfirmSave { get; set; }

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_INPUT_REMAIN()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetParameters();
                SetControl();
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {

        }
        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _prodLotID = tmps[5] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            DataRow RemainLot = tmps[4] as DataRow;

            _inboxList = RemainLot.Table.Clone();
            _inboxList.ImportRow(RemainLot);
        }

        private void SetControl()
        {
            // Grid
            Util.GridSetData(dgList, _inboxList, null, true);

            if (Util.NVC(_inboxList.Rows[0]["WIP_QLTY_TYPE_CODE"]).Equals("G"))
            {
                dgList.Columns["CELLID"].Header = ObjectDic.Instance.GetObjectName("InBox ID");
                dgList.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["RESNGRNAME"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgList.Columns["CELLID"].Header = ObjectDic.Instance.GetObjectName("불량그룹LOT");
                dgList.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;
                dgList.Columns["RESNGRNAME"].Visibility = Visibility.Visible;
            }

            //txtCartID.Text = Util.NVC(_inboxList.Rows[0]["CTNR_ID"]);

            // 버튼 활성, 비활성
            SetControlEnabled(false);

            txtRemainQty.Focus();
        }

        private void SetControlEnabled(bool Enabled)
        {
            btnTagPrint.IsEnabled = Enabled;
            btnCartRePrint.IsEnabled = Enabled;

            if (Enabled)
            {
                btnRemainWait.IsEnabled = false;
                txtCartID.IsEnabled = false;
                chkNewCart.IsEnabled = false;
            }
        }

        private void SetCombo()
        {
        }

        #endregion

        #region 대차 GotFocus, PreviewKeyDown
        private void txtCartID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtCartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidationCart();
            }

        }
        #endregion

        #region [신규대차생성투입 체크]
        private void chkNewCart_Checked(object sender, RoutedEventArgs e)
        {
            txtCartID.Text = "NEW";
        }

        private void chkNewCart_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCartID.Text = string.Empty;
        }
        #endregion

        #region [태그 발행]
        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }
        #endregion

        #region 대차Sheet발행
        private void btnCartRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }

            // Page수 산출
            int PageMax = Util.NVC(_inboxList.Rows[0]["WIP_QLTY_TYPE_CODE"]).Equals("N") ? 50 : 40;
            int PageCount = dt.Rows.Count % PageMax != 0 ? (dt.Rows.Count / PageMax) + 1 : dt.Rows.Count / PageMax;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 50) + 1;
                end = ((cnt + 1) * 50);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);

                // 대차Sheet발행
                CartRePrint(dr, cnt + 1);
            }
        }
        #endregion

        #region [잔량대기]
        private void btnRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemainWait())
                return;

            // 잔량처리 하시겠습니까?
            Util.MessageConfirm("SFU1862", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputRemainProcess();
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

        #region Mehod

        /// <summary>
        /// 대차 출력 자료
        /// </summary>
        private DataTable ChkCaart()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CART_ID"] = txtCartID.Text;
                newRow["PROCID"] = Util.NVC(_procID);
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_TB_SFC_CTNR_PC", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 잔량대기
        /// </summary>
        private void InputRemainProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_REMAIN_INBOX";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("MOD_FLAG", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("INPUT_SEQNO", typeof(Decimal));
                inLot.Columns.Add("INPUT_LOTID", typeof(string));
                inLot.Columns.Add("INPUT_QTY", typeof(Decimal));
                inLot.Columns.Add("ACTQTY", typeof(Decimal));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                newRow["CTNR_ID"] = (bool)chkNewCart.IsChecked ? null : Util.NVC(_inboxList.Rows[0]["CTNR_ID"]);  // Util.NVC(_inboxList.Rows[0]["CTNR_ID"]);
                newRow["WIPNOTE"] = txtNote.Text;
                newRow["PROD_LOTID"] = _prodLotID;
                inTable.Rows.Add(newRow);

                newRow = inLot.NewRow();
                newRow["INPUT_SEQNO"] = Util.NVC_Decimal(Util.NVC(_inboxList.Rows[0]["INPUT_SEQNO"]));
                newRow["INPUT_LOTID"] = Util.NVC(_inboxList.Rows[0]["CELLID"]);
                newRow["INPUT_QTY"] = Util.NVC_Decimal(Util.NVC(_inboxList.Rows[0]["INPUT_QTY"]));
                newRow["ACTQTY"] = txtRemainQty.Value;
                inLot.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", "OUT_INBOX,OUT_CTNR", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 정상 처리 되었습니다.
                        Util.MessageInfo("SFU1889");

                        // 버튼 활성, 비활성
                        SetControlEnabled(true);

                        // 생성된 inbox 정보 및 Print 버튼 표시
                        if (bizResult != null && bizResult.Tables["OUT_INBOX"].Rows.Count > 0)
                        {
                            txtNewInbox.Text = bizResult.Tables["OUT_INBOX"].Rows[0]["LOTID"].ToString();
                        }

                        if (bizResult != null && bizResult.Tables["OUT_CTNR"].Rows.Count > 0)
                        {
                            if ((bool)chkNewCart.IsChecked)
                                txtCartID.Text = bizResult.Tables["OUT_CTNR"].Rows[0]["CTNR_ID"].ToString();
                        }

                        ConfirmSave = true;
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

        /// <summary>
        /// 양품 태그 라벨
        /// </summary>
        private DataTable PrintInboxLabel()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtNewInbox.Text;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_REMAIN_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 대차 출력 자료
        /// </summary>
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = txtCartID.Text;
                //newRow["PROCID"] = Util.NVC(_procID);
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion

        #region [Func]

        private bool ValidationCartRePrint()
        {
            if (string.IsNullOrWhiteSpace(txtCartID.Text))
            {
                // 생성대차 정보가 없습니다.
                Util.MessageValidation("SFU4380");
                return false;
            }

            return true;
        }

        private bool ValidationCart()
        {
            if (!(bool)chkNewCart.IsChecked)
            {
                //if (string.IsNullOrWhiteSpace(txtCartID.Text))
                //{
                //    // 대차 정보가 없습니다.
                //    Util.MessageValidation("SFU4365");
                //    return false;
                //}

                //DataTable dt = ChkCaart();

                //if (dt == null || dt.Rows.Count == 0)
                //{
                //    // 대차 정보가 없습니다.
                //    Util.MessageValidation("SFU4365");
                //    return false;
                //}

                //if (Util.NVC(dt.Rows[0]["CTNR_STAT_CODE"]).Equals("WORKING"))
                //{
                //    // [%1] 공정에 진행중인 대차로는 처리 불가합니다.
                //    Util.MessageValidation("SFU4621", _procName);
                //    return false;
                //}

                //if (Util.NVC(dt.Rows[0]["MKT_TYPE_CODE"]) != Util.NVC(_inboxList.Rows[0]["MKT_TYPE_CODE"]))
                //{
                //    // 동일한 시장유형이 아닙니다.
                //    Util.MessageValidation("SFU4271");
                //    return false;
                //}

                //if (Util.NVC(dt.Rows[0]["PRODID"]) != Util.NVC(_inboxList.Rows[0]["PRODID"]))
                //{
                //    // 동일 제품이 아닙니다.
                //    Util.MessageValidation("SFU1502");
                //    return false;
                //}

                //if (Util.NVC(dt.Rows[0]["CURR_PROCID"]) != _procID)
                //{
                //    object[] parameters = new object[2];
                //    parameters[0] = ObjectDic.Instance.GetObjectName("잔량대기");
                //    parameters[1] = txtCartID.Text;

                //    //  [%1]하는 LOT대차 [%2]은 다른 공정에 있는 대차입니다.
                //    Util.MessageValidation("SFU4906", parameters);
                //    return false;
                //}
            }

            return true;
        }

        private bool ValidationRemainWait()
        {
            if (txtRemainQty.Value == 0)
            {
                // 잔량은 1이상이어야 합니다.
                Util.MessageValidation("SFU4207");
                return false;
            }

            int InputQty = Util.NVC_Int(_inboxList.Rows[0]["INPUT_QTY"]);

            if (InputQty <= txtRemainQty.Value)
            {
                // 잔량은 투입수량보다 작아야 합니다.
                Util.MessageValidation("SFU4053");
                return false;
            }

            if (!ValidationCart())
            {
                return false;
            }

            return true;
        }

         /// <summary>
        /// 태그 발행
        /// </summary>
        private void TagPrint()
        {

            if (string.IsNullOrWhiteSpace(txtNewInbox.Text))
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4025");
                return;
            }

            DataTable dt = PrintInboxLabel();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4025");
                return;
            }

            string processName = Util.NVC(dt.Rows[0]["PROCNAME"]);
            string modelId = Util.NVC(dt.Rows[0]["MODLID"]);
            string projectName = Util.NVC(dt.Rows[0]["PRJT_NAME"]);
            string marketTypeName = Util.NVC(dt.Rows[0]["MKT_TYPE_NAME"]);
            string assyLotId = Util.NVC(dt.Rows[0]["LOTID_RT"]);
            string calDate = Util.NVC(dt.Rows[0]["CALDATE"]);
            //string shiftName = Util.NVC(dt.Rows[0]["SHFT_NAME"]);
            string shiftName = Util.NVC(ShiftName);
            string equipmentShortName = Util.NVC(dt.Rows[0]["EQPTSHORTNAME"]);
            //string inspectorId = Util.NVC(dt.Rows[0]["INSPECTORID"]);
            string inspectorId = string.Empty;

            // 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 양품 Tag인 경우 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            DataRow dr = dtLabelItem.NewRow();
            dr["LABEL_CODE"] = "LBL0106";
            dr["ITEM001"] = Util.NVC(dt.Rows[0]["CAPA_GRD_CODE"]);
            dr["ITEM002"] = modelId + "(" + projectName + ") ";
            dr["ITEM003"] = assyLotId;
            dr["ITEM004"] = Util.NVC_Int(dt.Rows[0]["WIPQTY"]).GetString();
            dr["ITEM005"] = equipmentShortName;
            dr["ITEM006"] = calDate + "(" + shiftName + ")";
            dr["ITEM007"] = inspectorId;
            dr["ITEM008"] = marketTypeName;
            dr["ITEM009"] = Util.NVC(dt.Rows[0]["INBOX_ID"]);
            dr["ITEM010"] = null;
            dr["ITEM011"] = null;
            dtLabelItem.Rows.Add(dr);

            // 라벨 발행 이력 저장
            DataRow newRow = inTable.NewRow();
            newRow["LABEL_PRT_COUNT"] = 1;                                    // 발행 수량
            newRow["PRT_ITEM01"] = Util.NVC(dt.Rows[0]["INBOX_ID"]);
            newRow["PRT_ITEM02"] = Util.NVC(dt.Rows[0]["WIPSEQ"]);
            newRow["PRT_ITEM04"] = "N";                                       // 재발행 여부
            newRow["INSUSER"] = LoginInfo.USERID;
            newRow["LOTID"] = Util.NVC(dt.Rows[0]["INBOX_ID"]);
            inTable.Rows.Add(newRow);

            ////////////////////// 라벨 출력
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                // 라벨 발행이력 저장
                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
        }

        /// <summary>
        /// 대차 Sheet 팝업
        /// </summary>
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;
            if (Util.NVC(_inboxList.Rows[0]["WIP_QLTY_TYPE_CODE"]).Equals("N"))
            {
                popupCartPrint.DefectCartYN = "Y";
            }

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _eqptID;
            parameters[2] = txtCartID.Text;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }

        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

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