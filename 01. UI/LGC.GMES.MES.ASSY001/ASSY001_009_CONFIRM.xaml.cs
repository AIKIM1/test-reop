/*************************************************************************************
 Created Date : 2017.02.08
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - STP 실적 확정 화면 - 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.08  INS 정문교C : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_009_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;
        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;
        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;

        private bool bSave = false;
        private bool bEndSetTime = false;

        private System.DateTime dtNow;
        private System.DateTime dtCaldate;

        private double gDfctAQty = 0;
        private double gDfctCQty = 0;

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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
        public ASSY001_009_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;
        }

        private void InitializeDfctDtl()
        {
            DataTable dtTmp = _Biz.GetDA_PRD_SEL_DEFECT_DTL();

            DataRow dtRow = dtTmp.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;

            dtTmp.Rows.Add(dtRow);

            dgDfctDTL.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 11)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _WipStat = Util.NVC(tmps[4]);
                _RetShiftName = Util.NVC(tmps[5]);
                _RetShiftCode = Util.NVC(tmps[6]);
                _RetUserName = Util.NVC(tmps[7]);
                _RetUserID = Util.NVC(tmps[8]);
                _RetWrkStrtTime = Util.NVC(tmps[9]);
                _RetWrkEndTime = Util.NVC(tmps[10]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _WipStat = "";
                _RetShiftName = "";
                _RetShiftCode = "";
                _RetUserName = "";
                _RetUserID = "";
                _RetWrkStrtTime = "";
                _RetWrkEndTime = "";
            }

            ApplyPermissions();
            InitializeControls();

            GetAllData();

            bEndSetTime = true;

            txtShift.Text = _RetShiftName;
            txtShift.Tag = _RetShiftCode;
            txtWorker.Text = _RetUserName;
            txtWorker.Tag = _RetUserID;

            dtpCaldate.SelectedDataTimeChanged += dtpCaldate_SelectedDataTimeChanged;

        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeDfctDtl();
        }
        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            string sMsg = string.Empty;

            //if (_WipStat.Equals("PROC"))
            //    sMsg = "진행중인 LOT을 실적 확인처리 하시겠습니까?";
            //else
            //    sMsg = "확인처리 하시겠습니까?";

            string messageCode = _WipStat.Equals("PROC") ? "SFU1915" : "SFU2039";
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }
        #endregion

        #region [종료]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = bSave ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }
        #endregion

        #region [작업일자]
        private void dtpCaldate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
                else
                    dtPik.Focus();
            }));
        }
        #endregion

        #region [작업조, 작업자]
        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_LineID);
                Parameters[3] = Process.STP;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);  //EQPTID 추가 
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPopup);
                        wndPopup.BringToFront();
                        break;
                    }
                }
            }

        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
        }
        #endregion

        #region [불량/LOSS/물품청구 탭]
        private void btnTypeCntSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보를 저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }
        private void dgDefect_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                double atypeqty;
                double ctypeqty;
                double resnqty;

                if (e.Cell.Column.Name.Equals("A_TYPE_DFCT_QTY") || e.Cell.Column.Name.Equals("C_TYPE_DFCT_QTY"))
                {
                    string sAtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "A_TYPE_DFCT_QTY"));
                    string sCtype = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "C_TYPE_DFCT_QTY"));
                    atypeqty = double.Parse(sAtype);
                    ctypeqty = double.Parse(sCtype);

                    if (gDfctAQty > 0 && gDfctCQty > 0)
                    {
                        resnqty = Math.Round((atypeqty / gDfctAQty + ctypeqty / gDfctCQty) / 2, 0);

                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", resnqty);
                    }
                }
                else if (e.Cell.Column.Name.Equals("RESNQTY"))
                {
                    string sResnQty = e.Cell.Text;
                    resnqty = double.Parse(sResnQty);

                    atypeqty = (resnqty * 2) * gDfctAQty;
                    ctypeqty = (resnqty * 2) * gDfctCQty;

                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "A_TYPE_DFCT_QTY", atypeqty / 2);
                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "C_TYPE_DFCT_QTY", ctypeqty / 2);
                }

                SumDefectQty();

                if (dgDfctDTL.Rows.Count - dgDfctDTL.TopRows.Count > 0)
                {
                    double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                    double dDefectQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                    double dOutQty = dGoodQty + dDefectQty;

                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY", dOutQty);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY", dOutQty);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 불량 정보 탭]
        private void btnEqpDefectSearch_Click(object sender, RoutedEventArgs e)
        {
            GetEqpDefectfo();
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// LOT 정보
        /// </summary>
        private void GetFoldingLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                ////DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "OUTDATA", inTable);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtLotId.Text = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    txtProdId.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                    txtWorkOrder.Text = Util.NVC(dtResult.Rows[0]["WOID"]);
                    txtStartTime.Text = Util.NVC(dtResult.Rows[0]["WIPDTTM_ST"]);
                    txtEndTime.Text = Util.NVC(dtResult.Rows[0]["EQPT_END_DTTM"]);
                    txtRemark.Text = Util.NVC(dtResult.Rows[0]["WIP_NOTE"]);

                    // Caldate Lot의 Caldate로
                    if (Util.NVC(dtResult.Rows[0]["CALDATE_LOT"]).Trim().Equals(""))
                    {
                        dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["NOW_CALDATE"])).ToLongDateString();
                        dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["NOW_CALDATE"]));
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["NOW_CALDATE"]));
                    }
                    else
                    {
                        dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE_LOT"])).ToLongDateString();
                        dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE_LOT"]));
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE_LOT"]));
                    }
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
        /// <summary>
        /// 바구니 정보
        /// </summary>
        private void GetBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_BOX_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgBox, dtRslt, null);

                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY", dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY", dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY", dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", 0);
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", 0);
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
        /// 불량/LOSS/물품청구 
        /// </summary>
        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.STP;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", inTable);

                if (searchResult != null)
                {
                    Util.GridSetData(dgDefect, searchResult, null);

                    SumDefectQty();

                    if (dgDfctDTL != null)
                    {
                        if (dgDfctDTL.Rows.Count - dgDfctDTL.TopRows.Count > 0)
                        {
                            double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                            double dDefectQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                            double dOutQty = dGoodQty + dDefectQty;

                            DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY", dOutQty);
                            DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY", dOutQty);
                        }
                    }
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
        /// <summary>
        /// 설비 불량 정보
        /// </summary>
        private void GetEqpDefectfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID; // Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = _WipSeq; // Util.NVC(rowview["WIPSEQ"]);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgEqpDefect, searchResult, null);
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
                );
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
        /// 
        /// </summary>
        private void GetMBOMInfo()
        {
            ////try
            ////{
            ////    ShowLoadingIndicator();

            ////    DataTable inTable = _Biz.GetDA_PRD_SEL_MBOM();

            ////    DataRow newRow = inTable.NewRow();
            ////    newRow["WO_DETL_ID"] = txtWorkOrder.Text;
            ////    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            ////    //newRow["EQSGID"] = _LineID;
            ////    //newRow["INPUT_PROCID"] = Process.STACKING_FOLDING;

            ////    inTable.Rows.Add(newRow);

            ////    new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
            ////    {
            ////        try
            ////        {
            ////            if (searchException != null)
            ////            {
            ////                Util.MessageException(searchException);
            ////                return;
            ////            }

            ////            if (searchResult == null || searchResult.Rows.Count < 1)
            ////            {
            ////                //Util.Alert("타입별 불량 기준정보가 존재하지 않습니다.");
            ////                Util.MessageValidation("SFU1941");
            ////                return;
            ////            }
            ////            else
            ////            {
            ////                for (int i = 0; i < searchResult.Rows.Count; i++)
            ////                {
            ////                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
            ////                    {
            ////                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
            ////                    }
            ////                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
            ////                    {
            ////                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
            ////                    }
            ////                }

            ////                if (txtInAType.Text.Equals(""))
            ////                {
            ////                    //Util.Alert("ATYPE 불량 기준정보가 존재하지 않습니다.");
            ////                    Util.MessageValidation("SFU1306");
            ////                    return;
            ////                }

            ////                if (txtInCType.Text.Equals(""))
            ////                {
            ////                    //Util.Alert("CTYPE 불량 기준정보가 존재하지 않습니다.");
            ////                    Util.MessageValidation("SFU1326");
            ////                    return;
            ////                }

            ////                gDfctAQty = double.Parse(txtInAType.Text);
            ////                gDfctCQty = double.Parse(txtInCType.Text);
            ////            }
            ////        }
            ////        catch (Exception ex)
            ////        {
            ////            Util.MessageException(ex);
            ////        }
            ////        finally
            ////        {
            ////            HiddenLoadingIndicator();
            ////        }
            ////    }
            ////    );
            ////}
            ////catch (Exception ex)
            ////{
            ////    Util.MessageException(ex);
            ////}
            ////finally
            ////{
            ////    HiddenLoadingIndicator();
            ////}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bShowMsg"></param>
        private void SetDefect(bool bShowMsg = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;
                    //if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("DEFECT_LOT"))
                    //{
                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = _WipSeq;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    ////newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    ////newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));
                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                if (bShowMsg)
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                GetDefectInfo();
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
        /// 실적 확정
        /// </summary>
         private void Save()
        {
            try
            {
                // 자동 불량 저장 처리.
                SaveDefectAllBeforeConfirm();

                ShowLoadingIndicator();

                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_END_LOT_STP();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["INPUTQTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")));
                newRow["OUTPUTQTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WIPNOTE"] = txtRemark.Text;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable input_LOT = indataSet.Tables["IN_INPUT"];
                //newRow = input_LOT.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";
                //newRow["INPUT_LOTID"] = "";

                //input_LOT.Rows.Add(newRow);

                ////new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_FD", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_STP", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //btnLossDfctSave.IsEnabled = false;
                        //btnPrdChgDfctSave.IsEnabled = false;
                        btnDefectSave.IsEnabled = false;
                        btnSave.IsEnabled = false;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        bSave = true;

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
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

        #endregion

        #region [Validation]
        private bool CanSave()
        {
            bool bRet = false;

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.Alert("작업조를 선택 하세요.");
                Util.MessageValidation("SFU1844");
                return bRet;
            }

            if (txtWorker.Text.Trim().Equals(""))
            {
                //Util.Alert("작업자를 선택 하세요.");
                Util.MessageValidation("SFU1842");
                return bRet;
            }

            //if (dgDefect.ItemsSource != null)
            //{
            //    foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgDefect))
            //    {
            //        Util.Alert("저장하지 않은 불량 정보가 있습니다.");
            //        return bRet;
            //    }
            //}

            //if (dgBox.ItemsSource != null && dgBox.Rows.Count > 0)
            //{
            //    // Short 불량 Check...
            //    //int iSumBoxCnt = int.Parse(DataTableConverter.GetValue(dgOutProduct.Rows[dgOutProduct.Rows.Count - 1].DataItem, "OUTQTY").ToString());  // 바구니수량 합계
            //    int iSumBoxCnt = int.Parse(Convert.ToDouble(DataTableConverter.Convert(dgBox.ItemsSource).Compute("SUM(WIPQTY)", String.Empty).ToString()).ToString());   // 바구니수량 합계
            //    int iShortCnt = GetShortDefectCount();

            //    double dShortRate = double.Parse(iShortCnt.ToString()) / double.Parse(iSumBoxCnt.ToString());   // Short 불량율
            //    double dMaxShortRate = GetShorDefectRate();    // 시스템에 등록된 최대 범위 불량율

            //    if (dShortRate > dMaxShortRate)
            //    {
            //        string sMsg = "쇼트불량률이 범위를 초과하였습니다!  \r\n\r\n" + "수량합계 : " + iSumBoxCnt.ToString() + "개 | 불량쇼트 : " + iShortCnt.ToString() + "개 | 불량률 : "
            //                     + (dShortRate * 100).ToString("###.#") + "% \r\n\r\n"
            //                     + "쇼트불량 개수를 수정하시고 다시 진행하십시요!   \r\n\r\n" + "수정할려면 [취소] / 무시하고 계속진행은 [확인]";

            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //        {
            //            if (result == MessageBoxResult.Cancel)
            //            {
            //                bRet = false;
            //            }
            //        });
            //    }
            //}

            bRet = true;

            return bRet;
        }
        private bool CanSaveDefect()
        {
            bool bRet = false;

            if (dgDefect.ItemsSource == null || dgDefect.Rows.Count < 1)
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return bRet;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                //Util.Alert("LOT 정보가 없습니다.");
                Util.MessageValidation("SFU1195");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveDefectAllBeforeConfirm()
        {
            try
            {
                SetDefect(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(dgDefect.ItemsSource);

                if (dtTmp != null && dtTmp.Rows.Count > 0)
                {
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString()));
                }
                else
                {
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", 0);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetAllData()
        {
            ClearControls();

            InitializeDfctDtl();
            GetFoldingLotInfo();
            GetBox();
            GetDefectInfo();
            GetEqpDefectfo();

            GetMBOMInfo();
        }
        private void ClearControls()
        {
            Util.gridClear(dgDfctDTL);
            Util.gridClear(dgBox);
            Util.gridClear(dgDefect);
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
