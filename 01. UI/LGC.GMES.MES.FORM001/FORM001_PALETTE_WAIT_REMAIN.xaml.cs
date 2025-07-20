/*************************************************************************************
 Created Date : 2017.07.29
      Creator : 
   Decription : 투입 Pallet 잔량 대기
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_PALETTE_WAIT_REMAIN : C1Window, IWorkArea
    {
        #region Declaration
        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
 
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private int _inputQty = 0;
        private int _beforeBoxQty = 0;
        private double _baseInboxQty = 0;
        

        public bool ConfirmSave { get; set; }

        private bool _load = true;
        private bool _tagPrint = false;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public int DifferenceQty { get; set; }

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

        public FORM001_PALETTE_WAIT_REMAIN()
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
                SetControl();
                SetControlVisibility();
                SetControlHeader();
                _load = false;
            }

            txtRemainQty.Focus();
        }

        private void InitializeUserControls()
        {
            UcFormShift = grdShift.Children[0] as UcFormShift;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            DataRow prodPallet = tmps[4] as DataRow;

            if (prodPallet == null)
                return;

            DataTable prodPalletBind = new DataTable();
            prodPalletBind = prodPallet.Table.Clone();
            prodPalletBind.ImportRow(prodPallet);

            Util.GridSetData(dgLot, prodPalletBind, null, true);

            _inputQty = Util.NVC_Int(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "INPUT_QTY"));
            _beforeBoxQty = Util.NVC_Int(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "INBOX_QTY"));
            _baseInboxQty = Convert.ToDouble(Util.NVC_Decimal(Util.NVC_Decimal(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "INBOX_LOAD_QTY"))));

            if (DifferenceQty > 0)
            {
                if (_inputQty > DifferenceQty)
                    txtRemainQty.Value = DifferenceQty;
            }

            // 작업자, 작업조
            UcFormShift.TextShift.Tag = ShiftID;
            UcFormShift.TextShift.Text = ShiftName;
            UcFormShift.TextWorker.Tag = WorkerID;
            UcFormShift.TextWorker.Text = WorkerName;
            UcFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcFormShift = grdShift.Children[0] as UcFormShift;
            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

        }
        private void SetControlVisibility()
        {
        }

        private void SetControlHeader()
        {
            if (string.Equals(_procID, Process.SmallOcv) || string.Equals(_procID, Process.SmallLeak) || string.Equals(_procID, Process.SmallDoubleTab))
            {
                // 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭
                this.Header = ObjectDic.Instance.GetObjectName("투입대차잔량대기");
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("투입 Pallet 잔량 대기");
            }
        }

        #endregion

        #region [Pallet 잔량대기]
        /// <summary>
        /// Pallet 잔량대기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateremainWait())
                return;

            // Pallet를 잔량대기 하시겠습니까?
            Util.MessageConfirm("SFU4017", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetCreatePallet();
                }
            });

        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (btnTagPrint.IsEnabled == true)
            {
                if (_tagPrint == false)
                {
                    // 잔량 태그를 발행 하세요.
                    Util.MessageValidation("SFU4110");
                    e.Cancel = true;
                }

            }
        }
        #endregion

        #region 잔량 수량 변경, InBox 수량 변경
        private void txtRemainQty_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (txtRemainQty.Value.ToString().Equals("NaN") || txtRemainQty.Value == 0)
            {
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "CHANGE_QTY", 0);
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "INBOX_QTY", _beforeBoxQty);
                txtInBoxQty.Value = 0;
                return;
            }

            // 잔량 산출
            double RemainQty = 0;

            RemainQty = txtRemainQty.Value;

            DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "CHANGE_QTY", _inputQty - RemainQty);

            // Inbox 산출
            int InboxQty = 0;

            if (RemainQty == 0 || _baseInboxQty == 0)
            {
                InboxQty = 0;
            }
            else
            {
                if (RemainQty % _baseInboxQty > 0)
                    InboxQty = Convert.ToInt16(Math.Truncate(RemainQty / _baseInboxQty)) + 1;
                else
                    InboxQty = Convert.ToInt16(RemainQty / _baseInboxQty);
            }

            // 잔량입력후 투입 InBox 산출
            DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "INBOX_QTY", _beforeBoxQty - InboxQty);

            txtInBoxQty.Value = InboxQty;

        }

        #endregion

        #region [등급별 Cell수량 변경시]
        private void dgLot_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //if (e.Cell.Column.Name.Equals("REMAIN_QTY"))
            //{
            //    // 잔량 산출
            //    int InputQty = 0;
            //    int RemainQty = 0;

            //    InputQty = Util.NVC_Int(DataTableConverter.GetValue(dgLot.Rows[e.Cell.Row.Index].DataItem, "INPUT_QTY"));
            //    RemainQty = Util.NVC_Int(DataTableConverter.GetValue(dgLot.Rows[e.Cell.Row.Index].DataItem, "REMAIN_QTY"));

            //    DataTableConverter.SetValue(dgLot.Rows[e.Cell.Row.Index].DataItem, "CHANGE_QTY", InputQty - RemainQty);

            //    // Inbox 산출
            //    double BaseInboxQty = 0;
            //    int InboxQty = 0;

            //    BaseInboxQty = Convert.ToDouble(Util.NVC_Decimal(DataTableConverter.GetValue(dgLot.Rows[e.Cell.Row.Index].DataItem, "INBOX_LOAD_QTY")));

            //    if (RemainQty == 0 || BaseInboxQty == 0)
            //    {
            //        InboxQty = 0;
            //    }
            //    else
            //    {
            //        if (RemainQty % BaseInboxQty > 0)
            //            InboxQty = Convert.ToInt16(Math.Truncate(RemainQty / BaseInboxQty)) + 1;
            //        else
            //            InboxQty = Convert.ToInt16(RemainQty / BaseInboxQty);
            //    }

            //    DataTableConverter.SetValue(dgLot.Rows[e.Cell.Row.Index].DataItem, "INBOX_QTY", InboxQty);

            //}

        }
        #endregion

        #region  dgLot LoadedCellPresenter
        private void dgLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("INBOX_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }

                    if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                    {
                        e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                    }

                }
            }));
        }
        #endregion

        #region [태그 발행]
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (string.IsNullOrWhiteSpace(txtNewPallet.Text))
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4025");
                return;
            }

            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.RemainPalletYN = "Y";

            object[] parameters = new object[8];
            parameters[0] = _procID;
            parameters[1] = _eqptID;
            //parameters[2] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
            //parameters[3] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ").GetString();
            parameters[2] = txtNewPallet.Text;
            parameters[3] = "1";      // 생성되는거라 1로 
            parameters[4] = "0";      // Dispatch에 넘겨줄때 사용 => 사용안함 
            parameters[5] = "N";      // 디스패치 처리
            parameters[6] = "N";      // 출력여부
            parameters[7] = "N";      // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);

            ////popupTagPrint.Show();
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupTagPrint);
                    popupTagPrint.BringToFront();
                    break;
                }
            }
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _tagPrint = true;
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
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// Pallet 잔량 대기
        /// </summary>
        private void SetCreatePallet()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_REMAIN_PALLET";

                // DATA SET
                DataSet inDataSet = GetBR_PRD_REG_REMAIN_PALLET();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inLot = inDataSet.Tables["INLOT"];

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PROD_LOTID").GetString();
                newRow["SHIFT"] = UcFormShift.TextShift.Tag;
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;
                newRow["WIPNOTE"] = txtNote.Text;
                inTable.Rows.Add(newRow);

                // inLot SET
                newRow = inLot.NewRow();
                newRow["INPUT_SEQNO"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "INPUT_SEQNO").GetString());
                newRow["INPUT_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PALLETID").GetString();
                newRow["WIPQTY_OUT"] = txtRemainQty.Value;
                newRow["INBOX_QTY_OUT"] = txtInBoxQty.Value;
                newRow["INBOX_QTY_IN"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "INBOX_QTY").GetString());
                inLot.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 생성된 Pallet 정보 및 Print 버튼 표시
                        if (bizResult != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            txtNewPallet.Text = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
                            btnTagPrint.IsEnabled = true;
                        }

                        ConfirmSave = true;
                        btnRemainWait.IsEnabled = false;

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        ////this.DialogResult = MessageBoxResult.OK;
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

        private bool ValidateremainWait()
        {
            if (dgLot.Rows.Count == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (txtRemainQty.Value == 0 )
            {
                // Pallet 잔량 수량을 입력 하세요.
                Util.MessageValidation("SFU4019");
                return false;
            }

            if (_inputQty <= txtRemainQty.Value)
            {
                // 잔량수량은  Pallet 투입 수량보다 작아야 됩니다.
                Util.MessageValidation("SFU4018");
                return false;
            }

            if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            {
                // 작업조를 입력해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]

        private DataSet GetBR_PRD_REG_REMAIN_PALLET()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));

            DataTable inBox = indataSet.Tables.Add("INLOT");
            inBox.Columns.Add("INPUT_SEQNO", typeof(Decimal));
            inBox.Columns.Add("INPUT_LOTID", typeof(string));
            inBox.Columns.Add("WIPQTY_OUT", typeof(Decimal));
            inBox.Columns.Add("INBOX_QTY_OUT", typeof(Decimal));
            inBox.Columns.Add("INBOX_QTY_IN", typeof(Decimal));

            return indataSet;
        }

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _procID; ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcFormShift.TextShiftEndTime.Text))
                                {
                                    UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcFormShift.TextWorker.Text = string.Empty;
                                    UcFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcFormShift.TextShift.Tag = string.Empty;
                                    UcFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcFormShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.Focus();
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

        #endregion

    }
}
