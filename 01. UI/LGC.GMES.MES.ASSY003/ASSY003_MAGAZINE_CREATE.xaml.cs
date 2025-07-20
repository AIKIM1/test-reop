/*************************************************************************************
 Created Date : 2017.06.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  DEVELOPER : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_MAGAZINE_CREATE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_MAGAZINE_CREATE : C1Window, IWorkArea
    {   
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoID = string.Empty;
        private string _ProcID = string.Empty;
        private string _WoDetlID = string.Empty;

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

        public ASSY003_MAGAZINE_CREATE()
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
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 5)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _WoID = Util.NVC(tmps[2]);
                    _ProcID = Util.NVC(tmps[3]);
                    _WoDetlID = Util.NVC(tmps[4]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _WoID = "";
                    _ProcID = "";
                    _WoDetlID = "";
                }
                ApplyPermissions();

                if (_ProcID.Equals(Process.STP))
                {
                    if (dgProd.Columns.Contains("CLSS_NAME"))
                        dgProd.Columns["CLSS_NAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgProd.Columns.Contains("CLSS_NAME"))
                        dgProd.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                }


                //
                CommonCombo _combo = new CommonCombo();

                String[] sFilter = { "MKT_TYPE_CODE" };
                _combo.SetCombo(cboMkTypeCode, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

                if ( rdoSerchTypeWorkOrder.IsChecked != null && (bool)rdoSerchTypeWorkOrder.IsChecked )
                {

                    //SSC Bi-cell 공정인 경우
                    if ( _ProcID.Equals(Process.SSC_BICELL))
                    {
                        bizRuleID = "DA_PRD_SEL_INPUT_PROD_LIST_BY_WOID_SSCBI";

                        inTable = new DataTable();
                        inTable.Columns.Add("LANGID", typeof(string));
                        inTable.Columns.Add("WOID", typeof(string));

                        DataRow newRow = inTable.NewRow();

                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["WOID"] = _WoDetlID;

                        inTable.Rows.Add(newRow);
                    }

                    else
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
                }

                else
                {
                    // 일자에 의한 조회
                    bizRuleID = "DA_PRD_SEL_INPUT_PROD_LIST";

                    inTable = _Biz.GetDA_PRD_SEL_INPUT_MAG_PROD_INFO_FD();

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _ProcID;
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

                        //dgProd.ItemsSource = DataTableConverter.Convert(bizResult);
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

                DataTable inTable = new DataTable();
                string sBizName = string.Empty;

                if (_ProcID.Equals(Process.SRC))
                {
                    inTable = new DataTable();

                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PRODID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("WIPQTY", typeof(int));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("WO_DETL_ID", typeof(string));
                    inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));


                    sBizName = "BR_PRD_REG_MAGAZINE_SRC";
                }
                else if (_ProcID.Equals(Process.STP))
                {
                    inTable = new DataTable();

                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PRODID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("WIPQTY", typeof(int));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                    sBizName = "BR_PRD_REG_MAGAZINE_STP";
                }
                else if (_ProcID.Equals(Process.SSC_BICELL) || _ProcID.Equals(Process.SSC_FOLDED_BICELL))
                {
                    inTable = new DataTable();

                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PRODID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("WIPQTY", typeof(int));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                    sBizName = "BR_PRD_REG_MAGAZINE_SSCBI";
                }
                else
                {
                    inTable = new DataTable();

                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PRODID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("WIPQTY", typeof(int));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("MGZN_RECONF_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                    sBizName = "BR_PRD_REG_MAGAZINE_FD_S";
                }

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = txtProdID.Text;
                newRow["CSTID"] = txtCstID.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPQTY"] = txtQty.Text.Equals("") ? 0 : int.Parse(txtQty.Text);
                newRow["MKT_TYPE_CODE"] = Util.NVC(cboMkTypeCode.SelectedValue);

                // 구분 재생,잔량
                if (rdoRecover.IsChecked != null && (bool)rdoRecover.IsChecked)
                {
                    //재생
                    newRow["MGZN_RECONF_TYPE_CODE"] = "RECOVER";
                }
                else
                {
                    //잔량
                    newRow["MGZN_RECONF_TYPE_CODE"] = "REMAIN";
                }

                if (_ProcID.Equals(Process.SRC))
                {
                    newRow["WO_DETL_ID"] = _WoDetlID;
                }

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    string sTmp = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    if (chkCut.IsChecked.HasValue && (bool)chkCut.IsChecked)
                    {
                        PrintLabel(sTmp);
                    }

                    Util.MessageInfo("SFU1700", sTmp);  //신규 매거진 [{0}]이 생성완료 되었습니다.
                }
                else
                {
                    if (chkCut.IsChecked.HasValue && (bool)chkCut.IsChecked)
                        Util.MessageValidation("SFU1624");  // 생성된 매거진ID가 없어 발행하지 못하였습니다.
                    else
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

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow newRow = RQSTDT.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK_NJ", "INDATA", "OUTDATA", RQSTDT);

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

            if (Util.NVC(cboMkTypeCode.SelectedValue).Equals("") || Util.NVC(cboMkTypeCode.SelectedValue).IndexOf("SELECT") >= 0)
            {
                Util.MessageValidation("SFU4371");  // 시장유형을 선택하세요.
                return bRet;
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

                if (rdoSerchTypeWorkOrder.IsChecked.HasValue && (bool)rdoSerchTypeWorkOrder.IsChecked)
                {
                    cboMkTypeCode.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgProd.Rows[iRow].DataItem, "MKT_TYPE_CODE"));
                    cboMkTypeCode.IsEnabled = false;
                }
                else
                {
                    cboMkTypeCode.IsEnabled = true;
                    cboMkTypeCode.SelectedIndex = 0;
                }
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

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    dicParam.Add("MKT_TYPE_CODE", Util.NVC(dtRslt.Rows[0]["MKT_TYPE_CODE"]));
                }

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

                //LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_NEW print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_NEW();
                //print.FrameOperation = FrameOperation;

                //if (print != null)
                //{
                //    object[] Parameters = new object[6];
                //    Parameters[0] = dicList;
                //    Parameters[1] = Process.LAMINATION;
                //    Parameters[2] = "";
                //    Parameters[3] = "";
                //    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                //    Parameters[5] = "N";   // 디스패치 처리.

                //    C1WindowExtension.SetParameters(print, Parameters);

                //    print.Closed += new EventHandler(print_Closed);

                //    print.Show();
                //}
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
    }
}
