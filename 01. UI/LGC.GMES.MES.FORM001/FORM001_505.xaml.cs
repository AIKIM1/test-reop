/*************************************************************************************
 Created Date : 2018.09.20
      Creator : 
   Decription : 자동차 활성화 후공정 - MTM 처리
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using System.Configuration;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_505 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private CheckBoxHeaderType _HeaderTypeLot;
        private CheckBoxHeaderType _HeaderTypeHistory;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public FORM001_505()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            //dtpDateFromHis.SelectedDateTime = DateTime.Now.AddDays(-7);
            //dtpDateToHis.SelectedDateTime = DateTime.Now;

            txtLotID.Text = string.Empty;
            txtPalletID.Text = string.Empty;
            Util.gridClear(dgList);
            txtProdID.Text = string.Empty;
            txtUserName.Text = string.Empty;
            txtUserName.Tag = string.Empty;
            txtResnNote.Text = string.Empty;
            txtFilePath.Text = string.Empty;

            txtLotID.Focus();
        }
        private void SetControl()
        {
            _HeaderTypeLot = CheckBoxHeaderType.Zero;
            _HeaderTypeHistory = CheckBoxHeaderType.Zero;
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboAreaHis, CommonCombo.ComboStatus.NONE, null, sCase: "AREA");
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
            listAuth.Add(btnSave);
            listAuth.Add(btnClear);
            listAuth.Add(btnSearchHis);
            listAuth.Add(btnResendHis);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        #region [제품 변경]

        private void txtLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchLotProcess(false, tb);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchLotProcess(true);
        }

        private void tbCheckHeaderAllLot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgList;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeLot)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeLot)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeLot = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeLot = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            attachFile(txtFilePath);
        }

        /// <summary>
        /// Clear
        /// </summary>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
        }

        /// <summary>
        /// 제품변경
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 저장하시겠습니까??
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });
        }

        #endregion

        #region [제품 변경 이력]
        private void txtHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchHistoryProcess(false, tb);
            }
        }

        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            SearchHistoryProcess(true);
        }

        private void tbCheckHeaderAllHistory_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgHistory;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeHistory)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeHistory)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeHistory = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeHistory = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        /// <summary>
        /// 재전송
        /// </summary>
        private void btnResendHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationErpReSend())
                return;

            // 전송 하시겠습니까?
            Util.MessageConfirm("SFU3609", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ErpReSendProcess();
                }
            });
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 제품 조회
        /// </summary>
        private void SearchProdID()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CLSS3_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["CLSS3_CODE"] = "MCC";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROD_FO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            txtProdID.Text = bizResult.Rows[0]["PRODID"].ToString().Substring(0, 3);

                            foreach (DataRow r in bizResult.Rows)
                            {
                                string displayString = r["PRODID"].ToString(); //표시 텍스트
                                string keywordString;

                                keywordString = displayString;
                                txtProdID.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString));
                            }
                        }

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

        /// <summary>
        /// 제품 변경 Lot 조회
        /// </summary>
        private void SearchLotProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtLotID"))

                        // %1이 입력되지 않았습니다.
                        Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("Lot ID"));
                    }
                    else
                    {
                        // %1이 입력되지 않았습니다.
                        Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("포장PALLETID"));
                    }
                }
                else
                {
                    // %1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("Lot ID") + "," + ObjectDic.Instance.GetObjectName("포장PALLETID"));
                }

                ShowLoadingIndicator();

                //DataTable inTable = new DataTable();
                //inTable.Columns.Add("LANGID", typeof(string));
                //inTable.Columns.Add("PROCID", typeof(string));
                //inTable.Columns.Add("EQSGID", typeof(string));
                //inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                //inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));
                //inTable.Columns.Add("WIPSTAT", typeof(string));
                //inTable.Columns.Add("PJT_NAME", typeof(string));
                //inTable.Columns.Add("PRODID", typeof(string));
                //inTable.Columns.Add("CTNR_ID", typeof(string));
                //inTable.Columns.Add("ASSY_LOTID", typeof(string));
                //inTable.Columns.Add("INBOX_ID", typeof(string));
                //inTable.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));

                //// INDATA SET
                //DataRow newRow = inTable.NewRow();
                //newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["PROCID"] = _procID;
                //newRow["EQSGID"] = Util.NVC(cboEquipmentSegmentOut.SelectedItemsToString);
                //newRow["WIP_QLTY_TYPE_CODE"] = Util.NVC(cboQltyTypeOut.SelectedValue);
                //newRow["CTNR_STAT_CODE"] = Util.NVC(cboCtnrStatOut.SelectedValue);
                //newRow["WIPSTAT"] = Util.NVC(cboInboxStatOut.SelectedValue);
                //newRow["PJT_NAME"] = txtPjtNameOut.Text;
                //newRow["PRODID"] = txtProdIDOut.Text;
                //newRow["CTNR_ID"] = txtCtnrIDOut.Text;
                //newRow["ASSY_LOTID"] = txtAssyLotIDOut.Text;
                //newRow["INBOX_ID"] = txtInboxIDOut.Text;
                //newRow["WIP_PRCS_TYPE_CODE"] = Util.NVC(cboWipPrcsTypeOut.SelectedValue);
                //inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CART_TAKEOVER_OUT_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        HiddenLoadingIndicator();

                //        if (bizException != null)
                //        {
                //            Util.MessageException(bizException);
                //            return;
                //        }

                //        Util.GridSetData(dgDefectLot, bizResult, FrameOperation, true);
                //    }
                //    catch (Exception ex)
                //    {
                //        HiddenLoadingIndicator();
                //        Util.MessageException(ex);
                //    }
                //});
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
 
        /// <summary>
        /// 제품변경
        /// </summary>
        private void SaveProcess()
        {
            //try
            //{
            //    ShowLoadingIndicator();

            //    // DATA SET 
            //    DataSet inDataSet = new DataSet();
            //    DataTable inTable = inDataSet.Tables.Add("INDATA");
            //    inTable.Columns.Add("SRCTYPE", typeof(string));
            //    inTable.Columns.Add("IFMODE", typeof(string));
            //    inTable.Columns.Add("AREAID", typeof(string));
            //    inTable.Columns.Add("PROCID", typeof(string));
            //    inTable.Columns.Add("USERID", typeof(string));
            //    inTable.Columns.Add("CTNR_ID", typeof(string));
            //    inTable.Columns.Add("ACT_USERID", typeof(string));
            //    inTable.Columns.Add("POSTDATE", typeof(string));

            //    DataTable inRESN = inDataSet.Tables.Add("INRESN");
            //    inRESN.Columns.Add("LOTID", typeof(string));
            //    inRESN.Columns.Add("WIPSEQ", typeof(string));
            //    inRESN.Columns.Add("ACTID", typeof(string));
            //    inRESN.Columns.Add("RESNCODE", typeof(string));
            //    inRESN.Columns.Add("RESNQTY", typeof(string));
            //    inRESN.Columns.Add("RESNNOTE", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            //    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            //    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    newRow["PROCID"] = _procID;
            //    newRow["USERID"] = LoginInfo.USERID;
            //    newRow["CTNR_ID"] = _cartId;
            //    newRow["ACT_USERID"] = txtWorkUserID.Text;
            //    newRow["POSTDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
            //    inTable.Rows.Add(newRow);

            //    newRow = inRESN.NewRow();
            //    newRow["LOTID"] = Util.NVC(_defectLot.Rows[0]["LOTID"]);
            //    newRow["WIPSEQ"] = Util.NVC(_defectLot.Rows[0]["WIPSEQ"]);
            //    newRow["ACTID"] = "LOSS_LOT";
            //    newRow["RESNCODE"] = Util.NVC(cboLossCode.SelectedValue);
            //    newRow["RESNQTY"] = txtLossQty.Value;
            //    newRow["RESNNOTE"] = txtResnNote.Text;
            //    inRESN.Rows.Add(newRow);

            //    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOSS_LOT_FOR_DFCT_CTNR", "INDATA,INRESN", null, (bizResult, bizException) =>
            //    {
            //        try
            //        {
            //            if (bizException != null)
            //            {
            //                HiddenLoadingIndicator();
            //                Util.MessageException(bizException);
            //                return;
            //            }

            //            //Util.AlertInfo("정상 처리 되었습니다.");
            //            Util.MessageInfo("SFU1889");

            //            this.DialogResult = MessageBoxResult.OK;
            //        }
            //        catch (Exception ex)
            //        {
            //            HiddenLoadingIndicator();
            //            Util.MessageException(ex);
            //        }
            //    }, inDataSet);
            //}
            //catch (Exception ex)
            //{
            //    HiddenLoadingIndicator();
            //    Util.MessageException(ex);
            //}
        }

        /// <summary>
        /// 제품 변경 이력 조회
        /// </summary>
        private void SearchHistoryProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtLotIDHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("Lot ID"));
                        }
                        else if (tb.Name.Equals("txtProdidHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("요청자"));
                        }
                    }
                }

                ShowLoadingIndicator();

                //DataTable inTable = new DataTable();
                //inTable.Columns.Add("LANGID", typeof(string));
                //inTable.Columns.Add("PROCID", typeof(string));
                //inTable.Columns.Add("EQSGID", typeof(string));
                //inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                //inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));
                //inTable.Columns.Add("WIPSTAT", typeof(string));
                //inTable.Columns.Add("PJT_NAME", typeof(string));
                //inTable.Columns.Add("PRODID", typeof(string));
                //inTable.Columns.Add("CTNR_ID", typeof(string));
                //inTable.Columns.Add("ASSY_LOTID", typeof(string));
                //inTable.Columns.Add("INBOX_ID", typeof(string));
                //inTable.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));

                //// INDATA SET
                //DataRow newRow = inTable.NewRow();
                //newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["PROCID"] = _procID;
                //newRow["EQSGID"] = Util.NVC(cboEquipmentSegmentOut.SelectedItemsToString);
                //newRow["WIP_QLTY_TYPE_CODE"] = Util.NVC(cboQltyTypeOut.SelectedValue);
                //newRow["CTNR_STAT_CODE"] = Util.NVC(cboCtnrStatOut.SelectedValue);
                //newRow["WIPSTAT"] = Util.NVC(cboInboxStatOut.SelectedValue);
                //newRow["PJT_NAME"] = txtPjtNameOut.Text;
                //newRow["PRODID"] = txtProdIDOut.Text;
                //newRow["CTNR_ID"] = txtCtnrIDOut.Text;
                //newRow["ASSY_LOTID"] = txtAssyLotIDOut.Text;
                //newRow["INBOX_ID"] = txtInboxIDOut.Text;
                //newRow["WIP_PRCS_TYPE_CODE"] = Util.NVC(cboWipPrcsTypeOut.SelectedValue);
                //inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CART_TAKEOVER_OUT_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        HiddenLoadingIndicator();

                //        if (bizException != null)
                //        {
                //            Util.MessageException(bizException);
                //            return;
                //        }

                //        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);

                //// 불량Cell수
                //int DfectCellCount = bizResult.Rows.Count;
                //DataGridAggregate.SetAggregateFunctions(dgHistory.Columns["불량수량"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = DfectCellCount.ToString("###,###") } });

                //    }
                //    catch (Exception ex)
                //    {
                //        HiddenLoadingIndicator();
                //        Util.MessageException(ex);
                //    }
                //});
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ERP 재전송
        /// </summary>
        private void ErpReSendProcess()
        {
            //try
            //{
            //    ShowLoadingIndicator();

            //    // DATA SET 
            //    DataSet inDataSet = new DataSet();
            //    DataTable inTable = inDataSet.Tables.Add("INDATA");
            //    inTable.Columns.Add("SRCTYPE", typeof(string));
            //    inTable.Columns.Add("IFMODE", typeof(string));
            //    inTable.Columns.Add("AREAID", typeof(string));
            //    inTable.Columns.Add("PROCID", typeof(string));
            //    inTable.Columns.Add("USERID", typeof(string));
            //    inTable.Columns.Add("CTNR_ID", typeof(string));
            //    inTable.Columns.Add("ACT_USERID", typeof(string));
            //    inTable.Columns.Add("POSTDATE", typeof(string));

            //    DataTable inRESN = inDataSet.Tables.Add("INRESN");
            //    inRESN.Columns.Add("LOTID", typeof(string));
            //    inRESN.Columns.Add("WIPSEQ", typeof(string));
            //    inRESN.Columns.Add("ACTID", typeof(string));
            //    inRESN.Columns.Add("RESNCODE", typeof(string));
            //    inRESN.Columns.Add("RESNQTY", typeof(string));
            //    inRESN.Columns.Add("RESNNOTE", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            //    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            //    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    newRow["PROCID"] = _procID;
            //    newRow["USERID"] = LoginInfo.USERID;
            //    newRow["CTNR_ID"] = _cartId;
            //    newRow["ACT_USERID"] = txtWorkUserID.Text;
            //    newRow["POSTDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
            //    inTable.Rows.Add(newRow);

            //    newRow = inRESN.NewRow();
            //    newRow["LOTID"] = Util.NVC(_defectLot.Rows[0]["LOTID"]);
            //    newRow["WIPSEQ"] = Util.NVC(_defectLot.Rows[0]["WIPSEQ"]);
            //    newRow["ACTID"] = "LOSS_LOT";
            //    newRow["RESNCODE"] = Util.NVC(cboLossCode.SelectedValue);
            //    newRow["RESNQTY"] = txtLossQty.Value;
            //    newRow["RESNNOTE"] = txtResnNote.Text;
            //    inRESN.Rows.Add(newRow);

            //    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOSS_LOT_FOR_DFCT_CTNR", "INDATA,INRESN", null, (bizResult, bizException) =>
            //    {
            //        try
            //        {
            //            if (bizException != null)
            //            {
            //                HiddenLoadingIndicator();
            //                Util.MessageException(bizException);
            //                return;
            //            }

            //            //Util.AlertInfo("정상 처리 되었습니다.");
            //            Util.MessageInfo("SFU1889");

            //            this.DialogResult = MessageBoxResult.OK;
            //        }
            //        catch (Exception ex)
            //        {
            //            HiddenLoadingIndicator();
            //            Util.MessageException(ex);
            //        }
            //    }, inDataSet);
            //}
            //catch (Exception ex)
            //{
            //    HiddenLoadingIndicator();
            //    Util.MessageException(ex);
            //}
        }
        #endregion

        #region [Validation]
        private bool ValidationSave()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgList, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProdID.Text))
            {
                // % 1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("변경제품ID"));
                return false;
            }

            if (txtUserName.Tag == null || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private bool ValidationErpReSend()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgHistory, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
        private void PopupUser()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];

            Parameters[0] = txtUserName.Text;

            C1WindowExtension.SetParameters(wndPerson, Parameters);

            wndPerson.Closed += new EventHandler(popupUser_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(wndPerson);
                    wndPerson.BringToFront();
                    break;
                }
            }
        }

        private void popupUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;
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

        private void attachFile(TextBox txtBox)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "All files (*.*)|*.*";

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }

                else
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        if (new System.IO.FileInfo(filename).Length > 5 * 1024 * 1024) //파일크기 체크
                        {
                            Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.

                            txtBox.Text = string.Empty;
                        }
                        else
                        {
                            txtBox.Text = filename;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        #endregion

    }
}
