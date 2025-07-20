/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : Folding 공정진척 화면 - 실적 확인 팝업 복사 [Folded Bi Cell 공정 추가로 인한 공정진척 화면 복사 및 수정]
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER   : Initial Created.
  2016.10.05  INS 김동일K : 프로그램 구현.
  2017.02.17  신광희 : Folded Bi Cell 공정 추가로 인한 공정진척 화면 복사 및 수정
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

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _lineCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _lotCode = string.Empty;
        private string _wipSeq = string.Empty;
        private string _wipState = string.Empty;
        private string _processCode = string.Empty;

        private string _shiftName = string.Empty;
        private string _shiftCode = string.Empty;
        private string _workUserName = string.Empty;
        private string _workUserId = string.Empty;
        private string _workStartTime = string.Empty;
        private string _workEndTime = String.Empty;

        private bool _isSave = false;
        private System.DateTime _dtNow;
        private bool _isEndSetTime = false;

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
        public CMM_ASSY_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            _dtNow = System.DateTime.Now;

            if (ldpDatePicker != null)
                ldpDatePicker.SelectedDateTime = (DateTime)_dtNow;
            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(_dtNow.Hour, _dtNow.Minute, _dtNow.Second);
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
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 12)
            {
                _lineCode = Util.NVC(tmps[0]);
                _equipmentCode = Util.NVC(tmps[1]);
                _lotCode = Util.NVC(tmps[2]);
                _wipSeq = Util.NVC(tmps[3]);
                _wipState = Util.NVC(tmps[4]);
                _processCode = Util.NVC(tmps[5]);

                _shiftName = Util.NVC(tmps[6]);
                _shiftCode = Util.NVC(tmps[7]);
                _workUserName = Util.NVC(tmps[8]);
                _workUserId = Util.NVC(tmps[9]);
                _workStartTime = Util.NVC(tmps[10]);
                _workEndTime = Util.NVC(tmps[11]);
            }


            ApplyPermissions();
            InitializeControls();

            GetAllData();
            _isEndSetTime = true;

            txtShift.Text = _shiftName;
            txtShift.Tag = _shiftCode;
            txtWorker.Text = _workUserName;
            txtWorker.Tag = _workUserId;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeDfctDtl();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            //"진행중인 LOT을 실적 확인처리 하시겠습니까?" : "확인처리 하시겠습니까?"
            string messageCode = _wipState.Equals("PROC") ? "SFU1915" : "SFU2039";
            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = _isSave ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

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
                double resnqty;

                string sResnQty = e.Cell.Text;
                resnqty = double.Parse(sResnQty);

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

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT wndPopup = new CMM_SHIFT { FrameOperation = FrameOperation };
            object[] parameters = new object[5];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = _lineCode;
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(txtShift.Tag);
            C1WindowExtension.SetParameters(wndPopup, parameters);

            wndPopup.Closed += new EventHandler(wndShift_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };
            object[] parameters = new object[7];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = Util.NVC(_lineCode);
            parameters[3] = Process.STACKING_FOLDING;
            parameters[4] = Util.NVC(txtShift.Tag);
            parameters[5] = Util.NVC(txtWorker.Tag);
            parameters[6] = Util.NVC(_equipmentCode);  //EQPTID 추가 

            C1WindowExtension.SetParameters(wndPopup, parameters);
            wndPopup.Closed += new EventHandler(wndShiftUser_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        }

        private void btnEqpDefectSearch_Click(object sender, RoutedEventArgs e)
        {
            GetEqpDefectfo();
        }

        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    if (!_isEndSetTime)
                        return;

                    TimeSpan spn = ((TimeSpan)teTimeEditor.Value);
                    var dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                    if (Math.Truncate(dtTime.Subtract(_dtNow).TotalSeconds) == 0)
                    {
                    }
                    else
                    {
                        double dSecond = Math.Truncate(dtTime.Subtract(DateTime.Parse(txtStartTime.Text)).TotalSeconds);

                        if (dSecond < 0)
                        {
                            //Util.Alert("시작시간보다 이전은 선택할 수 없습니다.");
                            Util.MessageValidation("SFU3032");
                            // 시작시간보다 작으면 초기화.
                            if (ldpDatePicker != null)
                                ldpDatePicker.SelectedDateTime = (DateTime)_dtNow;
                            if (teTimeEditor != null)
                                teTimeEditor.Value = new TimeSpan(_dtNow.Hour, _dtNow.Minute, _dtNow.Second);
                        }
                        else
                        {
                            //"날짜를 변경 하시겠습니까?"
                            Util.MessageConfirm("SFU1473", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    _dtNow = dtTime;
                                }
                                else
                                {
                                    if (ldpDatePicker != null)
                                        ldpDatePicker.SelectedDateTime = (DateTime)_dtNow;
                                    if (teTimeEditor != null)
                                        teTimeEditor.Value = new TimeSpan(_dtNow.Hour, _dtNow.Minute, _dtNow.Second);
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void teTimeEditor_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    if (!_isEndSetTime)
                        return;

                    DateTime dtTime;
                    TimeSpan spn = ((TimeSpan)teTimeEditor.Value);
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                    if (Math.Truncate(dtTime.Subtract(_dtNow).TotalSeconds) == 0)
                    {
                    }
                    else
                    {
                        double dSecond = Math.Truncate(dtTime.Subtract(DateTime.Parse(txtStartTime.Text)).TotalSeconds);

                        if (dSecond < 0)
                        {
                            //Util.Alert("시작시간보다 이전은 선택할 수 없습니다.");
                            Util.MessageValidation("SFU3032");

                            // 시작시간보다 작으면 초기화.
                            if (ldpDatePicker != null)
                                ldpDatePicker.SelectedDateTime = (DateTime)_dtNow;
                            if (teTimeEditor != null)
                                teTimeEditor.Value = new TimeSpan(_dtNow.Hour, _dtNow.Minute, _dtNow.Second);
                        }
                        else
                        {
                            _dtNow = dtTime;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetFoldingLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _lotCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    txtWorkOrder.Text = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);
                    txtJobDate.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);

                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    txtShift.Text = "";
                    txtWorker.Text = "";
                    txtRemark.Text = "";
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

        private void GetBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_BOX_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _lotCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgBox, dtRslt, null, true);

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
        }

        private void GetDefectInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _lotCode;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", inTable);

                if (searchResult != null)
                {
                    Util.GridSetData(dgDefect, searchResult, null, true);

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
        }

        private void GetEqpDefectfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _lotCode;
                newRow["WIPSEQ"] = _wipSeq;

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

                        Util.GridSetData(dgEqpDefect, searchResult, null, true);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private int GetShortDefectCount()
        {
            int iShortCnt = 0;

            return iShortCnt;
        }

        private double GetShorDefectRate()
        {
            double dShortRate = 0;


            dShortRate = dShortRate / 100;
            return dShortRate;
        }

        private void SetDefect(bool bShowMsg = true)
        {
            try
            {
                dgDefect.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);


                DataTable inDefectLot = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;
                    newRow = inDefectLot.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;
                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    inDefectLot.Rows.Add(newRow);

                }

                if (!CommonVerify.HasTableRow(inDefectLot)) return;

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                if (bShowMsg)
                    Util.MessageInfo("SFU1275");

                GetDefectInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Save()
        {
            try
            {
                // 자동 불량 저장 처리.
                SaveDefectAllBeforeConfirm();
                ShowLoadingIndicator();

                DateTime dtTime;
                TimeSpan spn;
                if (teTimeEditor.Value.HasValue)
                {
                    spn = ((TimeSpan)teTimeEditor.Value);
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day);
                }

                DataSet indataSet = _Biz.GetBR_PRD_REG_CONFIRM_LOT_FD();

                string bizRuleName = string.Empty;

                if(string.Equals(_processCode, Process.WINDING))
                    bizRuleName = "BR_PRD_REG_END_LOT_WN";
                else if(string.Equals(_processCode, Process.ASSEMBLY))
                    bizRuleName = "BR_PRD_REG_END_LOT_AS";
                else if(string.Equals(_processCode,Process.WASHING))
                    bizRuleName = "BR_PRD_REG_END_LOT_WS";

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["INPUTQTY"] = 0;
                newRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WIPNOTE"] = txtRemark.Text;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        btnDefectSave.IsEnabled = false;
                        btnSave.IsEnabled = false;

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        _isSave = true;
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

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

        #endregion

        #region [Validation]
        private bool CanSave()
        {
            if(string.IsNullOrEmpty(txtShift.Text))
            {
                //"작업조를 선택 하세요."
                Util.MessageValidation("SFU1844");
                return false;
            }

            if(string.IsNullOrEmpty(txtWorker.Text))
            {
                //"작업자를 선택 하세요."
                Util.MessageValidation("SFU1842");
                return false;
            }

            return true;
        }

        private bool CanSaveDefect()
        {
            if (dgDefect.ItemsSource == null || dgDefect.Rows.Count < 1)
            {
                //"불량 항목이 없습니다."
                Util.MessageValidation("SFU1578");
                return false;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                //"LOT 정보가 없습니다."
                Util.MessageValidation("SFU1195");
                return false;
            }

            return true;
        }

        #endregion

        #region [PopUp Event]
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;
            if (wndPopup != null && wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);

                _workStartTime = Util.NVC(wndPopup.WRKSTRTTIME);
                _workEndTime = Util.NVC(wndPopup.WRKENDTTIME);
                _shiftCode = Util.NVC(wndPopup.SHIFTCODE);
                _shiftName = Util.NVC(wndPopup.SHIFTNAME);
                _workUserId = Util.NVC(wndPopup.USERID);
                _workUserName = Util.NVC(wndPopup.USERNAME);

            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSave, btnDefectSave };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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
