/*************************************************************************************
 Created Date : 2017.12.06
      Creator : 
   Decription : 폴리머 실적 확정 - 작업완료
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
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_215_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration
        public UcPolymerFormShift UcPolymerFormShift { get; set; }
        public UcFormInputConfirm UcFormInputConfirm { get; set; } 

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _processCode = string.Empty;        // 공정코드
        private string _equipmentCode = string.Empty;        // 설비코드
        private string _divisionCode = string.Empty;
        private string _wipNote = string.Empty;

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public C1DataGrid DgInputPallet { get; set; } 


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

        public BOX001_215_CONFIRM()
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
            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;
            _equipmentCode = tmps[2] as string;
            // SET COMMON
            txtProcess.Text = (string) tmps[1];
            txtEquipment.Text = (string) tmps[3];
            // SET 생산 Lot 정보
            DataRow prodLot = tmps[4] as DataRow;
            _divisionCode = tmps[5] as string;
            _wipNote = tmps[6] as string;

            if (prodLot == null)
                return;

            DataTable prodLotBind = new DataTable();

            prodLotBind = prodLot.Table.Clone();
            prodLotBind.ImportRow(prodLot);

            Util.GridSetData(dgLot, prodLotBind, null, true);
            GetDefectInfo();

            /////////////////////////////// 작업자, 작업조
            UcPolymerFormShift.TextShift.Tag = ShiftID;
            UcPolymerFormShift.TextShift.Text = ShiftName;
            UcPolymerFormShift.TextWorker.Tag = WorkerID;
            UcPolymerFormShift.TextWorker.Text = WorkerName;
            UcPolymerFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
            if (UcPolymerFormShift != null)
            {
                UcPolymerFormShift.ButtonShift.Click += ButtonShift_Click;
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
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            try
            {
                dgDefect.EndEditRow(true);
            }
            catch
            { }

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
        public void SetGridProduct()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DEVISION", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID").GetString();
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ").GetString();
                newRow["PROCID"] = _processCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_PRODUCT_SUM_PC", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProduct, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 불량, 로스, 물청 정보 조회
        /// </summary>
        private void GetDefectInfo()
        {
            try
            {
                DataTable inTable = _bizRule.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ");

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_INFO_FORMATION_PC", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgDefect, dtResult, null);

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
                //if (string.Equals(_processCode, Process.PolymerDegas) && _divisionCode == "Tray")
                //{
                //    if (!SaveBeforeConfirm()) return;
                //}

                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_END_PROD";

                // DATA Table
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "PRODUCT_QTY"));
                newRow["SHIFT"] = UcPolymerFormShift.TextShift.Tag.ToString();
                newRow["WRK_USERID"] = UcPolymerFormShift.TextWorker.Tag.ToString();
                newRow["WRK_USER_NAME"] = UcPolymerFormShift.TextWorker.Text;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable,(bizResult, bizException) =>
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
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool SaveBeforeConfirm()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_SAVE_PROD";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("WIP_NOTE", typeof(string));
                inDataTable.Columns.Add("EQPT_END_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID").GetString();
                dr["WIP_NOTE"] = _wipNote;
                dr["EQPT_END_QTY"] = DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "GOOD_QTY").GetDecimal();
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                DataTable inResonTable = ds.Tables.Add("INRESN");
                inResonTable.Columns.Add("RESNCODE", typeof(string));
                inResonTable.Columns.Add("RESNQTY", typeof(decimal));

                if (CommonVerify.HasDataGridRow(dgDefect))
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
                    {
                        if (row.Type == DataGridRowType.Item)
                        {
                            DataRow newRow = inResonTable.NewRow();
                            newRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                            newRow["RESNQTY"] = DataTableConverter.GetValue(row.DataItem, "RESNQTY").GetDecimal();
                            inResonTable.Rows.Add(newRow);
                        }
                    }
                }
                //string xml = ds.GetXml();
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, ds);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

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

            if (UcPolymerFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcPolymerFormShift.TextShift.Tag.ToString()))
            {
                // 작업조를 입력해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (UcPolymerFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcPolymerFormShift.TextWorker.Tag.ToString()))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]

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
                newRow["EQPTID"] = _equipmentCode;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _processCode; ;

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

                        if (UcPolymerFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcPolymerFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcPolymerFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcPolymerFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcPolymerFormShift.TextShiftEndTime.Text))
                                {
                                    UcPolymerFormShift.TextShiftDateTime.Text = UcPolymerFormShift.TextShiftStartTime.Text + " ~ " + UcPolymerFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcPolymerFormShift.TextWorker.Text = string.Empty;
                                    UcPolymerFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcPolymerFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcPolymerFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcPolymerFormShift.TextShift.Tag = string.Empty;
                                    UcPolymerFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcPolymerFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcPolymerFormShift.ClearShiftControl();
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

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            parameters[6] = _equipmentCode;
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
        #endregion

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
