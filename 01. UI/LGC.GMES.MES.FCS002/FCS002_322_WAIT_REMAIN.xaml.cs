/*************************************************************************************
 Created Date : 2018.01.26
      Creator : 
   Decription : 투입 Pallet 잔량 대기
--------------------------------------------------------------------------------------
 [Change History]
  2023.03.27  이홍주: 소형활성화MES 복사
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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_322_WAIT_REMAIN : C1Window, IWorkArea
    {
        #region Declaration
        public UcFormShift UcFormShift { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
 
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private int _inputQty = 0;
        private int _beforeBoxQty = 0;
        private double _baseInboxQty = 0;
        

        public bool ConfirmSave { get; set; }

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public int DifferenceQty { get; set; }
        public string EquipmentSegmentCode { get; set; }

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

        public FCS002_322_WAIT_REMAIN()
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
            txtShift.Tag = ShiftID;
            txtShift.Text = ShiftName;
            txtWorker.Tag = WorkerID;
            txtWorker.Text = WorkerName;
          
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
        }

        #endregion

        #region [Inbox 잔량대기]
        /// <summary>
        /// Inbox 잔량대기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateremainWait())
                return;

            // 잔량처리 하시겠습니까?
            Util.MessageConfirm("SFU1862", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetCreateInbox();
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

        #region [태그 발행]
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

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

            if (!_Util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
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
       
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// Inbox 잔량 대기
        /// </summary>
        private void SetCreateInbox()
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
                newRow["SHIFT"] = txtShift.Tag;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["WIPNOTE"] = txtNote.Text;
                newRow["MOD_FLAG"] = "Y";
                inTable.Rows.Add(newRow);

                // inLot SET
                newRow = inLot.NewRow();
                newRow["INPUT_SEQNO"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "INPUT_SEQNO").GetString());
                newRow["INPUT_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "CELLID").GetString();
                newRow["WIPQTY_OUT"] = txtRemainQty.Value;
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
                            txtNewInbox.Text = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
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
                // 잔량은 1이상이어야 합니다.
                Util.MessageValidation("SFU4207");
                return false;
            }

            if (_inputQty <= txtRemainQty.Value)
            {
                // 잔량은 투입수량보다 작아야 합니다.
                Util.MessageValidation("SFU4053");
                return false;
            }

            if (txtShift.Tag == null || string.IsNullOrEmpty(txtShift.Tag.ToString()))
            {
                // 작업조를 입력해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (txtWorker.Tag == null || string.IsNullOrEmpty(txtWorker.Tag.ToString()))
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
            inDataTable.Columns.Add("MOD_FLAG", typeof(string));
            

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
                newRow["EQSGID"] = EquipmentSegmentCode == null ? LoginInfo.CFG_EQSG_ID : EquipmentSegmentCode;
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
            parameters[2] = EquipmentSegmentCode == null ? LoginInfo.CFG_EQSG_ID : EquipmentSegmentCode;
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
