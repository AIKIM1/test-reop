/*************************************************************************************
 Created Date : 2023.01.30
      Creator : 김용군
   Decription : ESHM 증설 - 조립공정진척 화면 - 재작업 매거진 재구성 팝업 (ASSY004.ASSY004_005_MAGAZINE_CREATE 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.24  김용군 : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_005_STACKING_MAGAZINE_CREATE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_005_STACKING_MAGAZINE_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoID = string.Empty;
        private string _LABEL_PRT_RESTRCT_FLAG = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _PROCID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY005_005_STACKING_MAGAZINE_CREATE()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInpuMagProdInfo();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _WoID = Util.NVC(tmps[2]);
                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _WoID = "";
                _LDR_LOT_IDENT_BAS_CODE = "";
            }
            ApplyPermissions();
            SetUserCheckFlag();
            
            txtUserNameCr.Text = string.Empty;
            txtUserNameCr.Tag = string.Empty;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = System.DateTime.Now.ToLongDateString();
                    dtPik.SelectedDateTime = System.DateTime.Now;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
            }));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgProd_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    SetSelectInfo(e.Cell.Row.Index);

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    txtType.Text = "";
                                    txtProdID.Text = "";
                                    txtQty.Text = "";
                                    txtCstID.Text = "";
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreate())
                return;

            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateMagazine();
                }
            });
        }

        private void txtQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtQty.Text, 0))
                {
                    txtQty.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoSerchType_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoSerchTypeDate != null && dtpDateFrom != null && dtpDateTo != null)
            {
                dtpDateFrom.IsEnabled = dtpDateTo.IsEnabled = (bool)(rdoSerchTypeDate.IsChecked);
                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetInpuMagProdInfo()
        {
            try
            {
                txtType.Text = string.Empty;
                txtProdID.Text = string.Empty;
                txtQty.Text = string.Empty;
                txtCstID.Text = string.Empty;

                Util.gridClear(dgProd);

                ShowLoadingIndicator();

                string bizRuleID = string.Empty;
                DataTable inTable = null;

                if (rdoSerchTypeWorkOrder.IsChecked != null && (bool)rdoSerchTypeWorkOrder.IsChecked)
                {
                    // WorkOrder 에 의한 조회
                    bizRuleID = "DA_PRD_SEL_INPUT_PROD_LIST_BY_WOID";

                    inTable = new DataTable();
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("WOID", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["WOID"] = _WoID;
                    newRow["EQPTID"] = _EqptID;

                    inTable.Rows.Add(newRow);
                }
                else
                {
                    // 일자에 의한 조회
                    bizRuleID = "DA_PRD_SEL_INPUT_PROD_LIST";

                    inTable = _Biz.GetDA_PRD_SEL_INPUT_MAG_PROD_INFO_FD();

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = Process.STACKING_FOLDING;
                    newRow["EQPTID"] = _EqptID;
                    newRow["EQSGID"] = _LineID;
                    newRow["FROM_DT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    newRow["TO_DT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleID, "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        
                        Util.GridSetData(dgProd, bizResult, null, true);

                        if (dgProd.CurrentCell != null)
                            dgProd.CurrentCell = dgProd.GetCell(dgProd.CurrentCell.Row.Index, dgProd.Columns.Count - 1);
                        else if (dgProd.Rows.Count > 0 && dgProd.GetCell(dgProd.Rows.Count, dgProd.Columns.Count - 1) != null)
                            dgProd.CurrentCell = dgProd.GetCell(dgProd.Rows.Count, dgProd.Columns.Count - 1);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CreateMagazine()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_CREATE_REWORK_MAG_FD();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = txtProdID.Text;
                newRow["CSTID"] = txtCstID.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPQTY"] = txtQty.Text.Equals("") ? 0 : int.Parse(txtQty.Text);

                //// 구분 재생,잔량
                //if (rdoRecover.IsChecked != null && (bool)rdoRecover.IsChecked)
                //{
                    //재생
                    newRow["MGZN_RECONF_TYPE_CODE"] = "RECOVER";
                //}
                //else if (rdoRemain.IsChecked != null && (bool)rdoRemain.IsChecked)
                //{
                //    //잔량
                //    newRow["MGZN_RECONF_TYPE_CODE"] = "REMAIN";
                //}
                //else
                //{
                //    //폐기재생
                //    newRow["MGZN_RECONF_TYPE_CODE"] = "SCRAP_RECOVER";
                //}

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_MAGAZINE_FD_L", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    string sTmp = Util.NVC(dtRslt.Rows[0]["LOTID"]);

                    if (LoginInfo.CFG_AREA_ID.Equals("A9"))
                    {
                        PrintLabel(sTmp);
                    }                    

                    Util.MessageInfo("SFU1700", sTmp);  //신규 매거진 [{0}]이 생성완료 되었습니다.
                }
                else
                {
                    Util.MessageInfo("SFU1625");    // 생성완료 되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [Validation]
        private bool CanCreate()
        {
            bool bRet = false;

            if (txtProdID.Text.Equals(""))
            {
                //Util.Alert("생성할 매거진 제품을 선택 하세요.");
                Util.MessageValidation("SFU1627");
                return bRet;
            }
            
            if (txtQty.Text.Trim().Equals("") || txtQty.Text.Trim().Equals("0"))
            {
                Util.MessageValidation("SFU1802"); //Util.Alert("입력 수량이 잘못 되었습니다.");
                return bRet;
            }

            int iQty = 0;
            if (int.TryParse(txtQty.Text.Trim(), out iQty))
            {
                if (iQty < 1)
                {
                    Util.MessageValidation("SFU3064"); // 수량은 0보다 큰 정수로 입력 하세요.
                    return bRet;
                }
            }
            else
            {
                Util.MessageValidation("SFU3065");  // 수량을 정수로 입력 하세요.
                return bRet;
            }

            if (_LABEL_PRT_RESTRCT_FLAG == "Y")
            {
                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserNameCr.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return bRet;
                }
            }

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (Util.NVC(txtCstID.Text).Trim().Equals(""))
                {
                    Util.MessageValidation("SFU6051");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetSelectInfo(int iRow)
        {
            try
            {
                if (iRow < 0)
                    return;

                txtType.Text = Util.NVC(DataTableConverter.GetValue(dgProd.Rows[iRow].DataItem, "PRDT_CLSS_CODE"));
                txtProdID.Text = Util.NVC(DataTableConverter.GetValue(dgProd.Rows[iRow].DataItem, "PRODID"));
                txtQty.Text = "";
                txtCstID.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintLabel(string sMagLot)
        {
            try
            {
                // 발행..

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                DataTable dtRslt = GetThermalPaperPrintingInfo(sMagLot);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;


                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                //라미
                dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                dicParam.Add("TITLEX", "MAGAZINE ID");


                dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                dicList.Add(dicParam);

                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.LAMINATION;
                    Parameters[2] = "";
                    Parameters[3] = "";
                    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                    Parameters[5] = "N";   // 디스패치 처리.
                    Parameters[6] = "MAGAZINE_RECONSTITUTION";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.Show();
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #endregion

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserNameCr.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void SetUserCheckFlag()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();                
                dr["PROCID"] = Process.AZS_STACKING;
                dr["EQSGID"] = _LineID;
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_LABEL_FLAG", "RQSTDT", "RSLDT", dtRQSTDT);

                if (dtRslt.Rows[0]["LABEL_PRT_RESTRCT_FLAG"].ToString().Trim().ToUpper().Equals("Y"))
                {
                    txtCreater.Visibility = Visibility.Visible;
                    txtUserNameCr.Visibility = Visibility.Visible;
                    btnUserCr.Visibility = Visibility.Visible;

                    _LABEL_PRT_RESTRCT_FLAG = "Y";
                }
                else
                {
                    txtCreater.Visibility = Visibility.Collapsed;
                    txtUserNameCr.Visibility = Visibility.Collapsed;
                    btnUserCr.Visibility = Visibility.Collapsed;

                    _LABEL_PRT_RESTRCT_FLAG = "N";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCstID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCstID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCstID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
