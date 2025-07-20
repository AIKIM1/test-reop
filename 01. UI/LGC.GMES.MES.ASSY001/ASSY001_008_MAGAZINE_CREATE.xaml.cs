/*************************************************************************************
 Created Date : 2017.02.06
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - 대기매거진 - 매거진신규구성 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.06  INS 정문교C : Initial Created.
 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_008_MAGAZINE_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoDetlID = string.Empty;
        private string _ProcID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
        #endregion

        #region Initialize

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY001_008_MAGAZINE_CREATE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _WoDetlID = Util.NVC(tmps[2]);
                _ProcID = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _WoDetlID = "";
                _ProcID = "";
            }
            ApplyPermissions();

        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInpuMagProdInfo();
        }
        #endregion

        #region [종료]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [생성]
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
        #endregion

        #region [일자]
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (sender == null)
                //    return;

                //LGCDatePicker dtPik = (sender as LGCDatePicker);

                //if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                //{
                //    dtPik.Text = System.DateTime.Now.ToLongDateString();
                //    dtPik.SelectedDateTime = System.DateTime.Now;
                //    Util.Alert("오늘 이전 날짜는 선택할 수 없습니다.");
                //    //e.Handled = false;
                //    return;
                //}
            }));
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
                    //Util.Alert("시작일자 이전 날짜는 선택할 수 없습니다.");
                    Util.MessageValidation("SFU1698");
                    //e.Handled = false;
                    return;
                }
            }));
        }
        #endregion

        #region [그리드]
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
        #endregion

        #region [수량]
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
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 매거진 리스트 
        /// </summary>
        private void GetInpuMagProdInfo()
        {
            try
            {
                Util.gridClear(dgProd);

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_MAG_PROD_INFO_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _LineID;
                newRow["FROM_DT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_PROD_LIST", "INDATA", "OUTDATA", inTable, (bizResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgProd, bizResult, null, true);

                        if (dgProd.CurrentCell != null)
                            dgProd.CurrentCell = dgProd.GetCell(dgProd.CurrentCell.Row.Index, dgProd.Columns.Count - 1);
                        else if (dgProd.Rows.Count > 0 && dgProd.GetCell(dgProd.Rows.Count, dgProd.Columns.Count - 1) != null)
                            dgProd.CurrentCell = dgProd.GetCell(dgProd.Rows.Count, dgProd.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
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

        /// <summary>
        /// 매거진 생성
        /// </summary>
        private void CreateMagazine()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                string sBizName = string.Empty;

                if (_ProcID.Equals(Process.SRC))
                {
                    inTable = _Biz.GetBR_PRD_REG_MAGAZINE_SRC();
                    sBizName = "BR_PRD_REG_MAGAZINE_SRC";
                }
                else if (_ProcID.Equals(Process.STP))
                {
                    inTable = _Biz.GetBR_PRD_REG_MAGAZINE_STP();
                    sBizName = "BR_PRD_REG_MAGAZINE_STP";
                }
                else
                {
                    inTable = _Biz.GetBR_PRD_REG_MAGAZINE_SSCBI();
                    sBizName = "BR_PRD_REG_MAGAZINE_SSCBI";

                }

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = txtProdID.Text;
                newRow["CSTID"] = txtCstID.Text;
                newRow["WIPQTY"] = txtQty.Text.Equals("") ? 0 : int.Parse(txtQty.Text);
                newRow["USERID"] = LoginInfo.USERID;

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

                    //Util.AlertInfo("신규 매거진 [{0}]이 생성완료 되었습니다.", sTmp);
                    Util.MessageInfo("SFU1700", sTmp);

                    //this.DialogResult = MessageBoxResult.OK;
                }
                else
                {
                    //if (chkCut.IsChecked.HasValue && (bool)chkCut.IsChecked)
                    //    Util.AlertInfo("생성된 매거진ID가 없어 발행하지 못하였습니다.");
                    //else
                    //    Util.AlertInfo("생성완료 되었습니다.");
                    if (chkCut.IsChecked.HasValue && (bool)chkCut.IsChecked)
                        Util.MessageValidation("SFU1624");
                    else
                        Util.MessageInfo("SFU1625");

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
                //Util.Alert("수량이 없습니다.");
                Util.MessageValidation("SFU3063");
                return bRet;
            }

            int iQty = 0;
            if (int.TryParse(txtQty.Text.Trim(), out iQty))
            {
                if (iQty < 1)
                {
                    //Util.Alert("수량은 0보다 큰 정수로 입력 하세요.");
                    Util.MessageValidation("SFU3064");
                    return bRet;
                }
            }
            else
            {
                //Util.Alert("수량을 정수로 입력 하세요.");
                Util.MessageValidation("SFU3064");
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
