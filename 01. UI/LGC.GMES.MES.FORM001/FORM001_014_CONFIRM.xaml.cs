/*************************************************************************************
 Created Date : 2017.11.28
      Creator : 
   Decription : 전압검사 - Pallet 생성, 수정
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
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_014_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration
        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }


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

        public FORM001_014_CONFIRM()
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
                SetGridProduct();
                _load = false;
            }

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

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            // SET 생산 Lot 정보
            DataRow prodLot = tmps[4] as DataRow;

            if (prodLot == null)
                return;

            DataTable prodLotBind = new DataTable();

            prodLotBind = prodLot.Table.Clone();
            prodLotBind.ImportRow(prodLot);

            Util.GridSetData(dgLot, prodLotBind, null, true);

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

        #endregion

        #region 차이수량 Red 색상 처리
        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DIFF_QTY"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));

        }
        #endregion

        #region [작업완료]
        /// <summary>
        /// 작업완료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateConfirmRun())
                return;

            // 실적확정 하시겠습니까?
            Util.MessageConfirm("SFU1716", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
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
        /// <summary>
        /// 생산 Lot 실적 조회
        /// </summary>
        private void SetGridProduct()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID").GetString();
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ").GetString();
                newRow["PROCID"] = _procID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_PRODUCT_SUM_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProduct, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void ConfirmProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_END_PROD_LOT_CM";

                // DATA Table
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("INPUTQTY", typeof(Decimal));
                inTable.Columns.Add("OUTPUTQTY", typeof(Decimal));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));

                DataTable inResn = inDataSet.Tables.Add("INRESN");
                inResn.Columns.Add("LOTID", typeof(string));
                inResn.Columns.Add("WIP_DFCT_CODE", typeof(string));
                inResn.Columns.Add("WIPQTY", typeof(Decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[0].DataItem, "INPUT_QTY"));
                newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[0].DataItem, "PRODUCT_QTY"));
                newRow["SHIFT"] = UcFormShift.TextShift.Tag.ToString();
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag.ToString();
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

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

        #endregion

        #region[[Validation]
        private bool ValidateConfirmRun()
        {
            if (dgLot.Rows.Count <= 0)
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (dgProduct.Rows.Count <= 0)
            {
                // 생산량이 없습니다.
                Util.MessageValidation("SFU1613");
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
