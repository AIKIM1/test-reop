/*************************************************************************************
 Created Date : 2018.03.22
      Creator : 정문교
   Decription : INBOX 투입 취소
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
    public partial class CMM_POLYMER_FORM_CART_INPUT_CANCEL : C1Window, IWorkArea
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

        public CMM_POLYMER_FORM_CART_INPUT_CANCEL()
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

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            DataRow[] CancelLot = tmps[4] as DataRow[];
            _inboxList = CancelLot.CopyToDataTable<DataRow>();
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

            //// 동일 대차 여부 체크
            //int cntrCount = _inboxList.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;

            //if (cntrCount == 1)
            //{
            //    txtCartID.Text = Util.NVC(_inboxList.Rows[0]["CTNR_ID"]);
            //}

            // 버튼 활성, 비활성
            SetControlEnabled(false);
        }

        private void SetControlEnabled(bool Enabled)
        {
            btnCartRePrint.IsEnabled = Enabled;

            if (Enabled)
            {
                btnInputCancel.IsEnabled = false;
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

        #region [투입취소]
        private void btnInputCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCart())
                return;

            // 투입을 취소 하시겠습니까?
            Util.MessageConfirm("SFU1982", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputCancelProcess();
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
        private DataTable ChkCart()
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
        /// 투입취소
        /// </summary>
        private void InputCancelProcess()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName = "BR_PRD_CANCEL_INPUT_LOT_INBOX";

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                //inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("NEW_CTNR_FLAG", typeof(string));
                inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("INPUT_SEQNO", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(Decimal));
                inLot.Columns.Add("WIPQTY2", typeof(Decimal));
                inLot.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["USERID"] = LoginInfo.USERID;
                //newRow["CTNR_ID"] = (bool)chkNewCart.IsChecked ? null : txtCartID.Text;
                newRow["NEW_CTNR_FLAG"] = (bool)chkNewCart.IsChecked ? "Y" : "N";
                newRow["WIP_QLTY_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE"));
                inTable.Rows.Add(newRow);

                foreach (DataGridRow row in dgList.Rows)
                {
                    if (row.Type != DataGridRowType.Item)
                        continue;

                    newRow = inLot.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INPUT_SEQNO"));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELLID"));
                    newRow["WIPQTY"] = Util.NVC_Decimal(Util.NVC(DataTableConverter.GetValue(row.DataItem, "INPUT_QTY")));
                    newRow["WIPQTY2"] = Util.NVC_Decimal(Util.NVC(DataTableConverter.GetValue(row.DataItem, "INPUT_QTY")));
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 정상 처리 되었습니다.
                        Util.MessageInfo("SFU1889");

                        // 버튼 활성, 비활성
                        SetControlEnabled(true);

                        if (bizResult != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            // 신규 대차
                            if ((bool)chkNewCart.IsChecked)
                                txtCartID.Text = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();

                        }
                    }
                    catch (Exception ex)
                    {
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

                //DataTable dt = ChkCart();

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
                //    parameters[0] = ObjectDic.Instance.GetObjectName("투입취소");
                //    parameters[1] = txtCartID.Text;

                //    //  [%1]하는 LOT대차 [%2]은 다른 공정에 있는 대차입니다.
                //    Util.MessageValidation("SFU4906", parameters);
                //    return false;
                //}

            }

            return true;
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