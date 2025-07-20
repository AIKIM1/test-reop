/*************************************************************************************
 Created Date : 2017.01.25
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - SRC 실적 확정 화면 - 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.25  INS 정문교C : Initial Created.
  2024.02.14  남기운      : 허용비율 초과시 사유 등록 프로세서 추가
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
using System.Windows.Media;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_008_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_008_CONFIRM : C1Window, IWorkArea
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

        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;


        private BizDataSet _Biz = new BizDataSet();

        #region Popup 처리 로직 변경        
        CMM_SHIFT_USER2 wndShiftUser;
        #endregion
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
        public ASSY003_008_CONFIRM()
        {
            InitializeComponent();
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
        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (wndShiftUser != null)
                wndShiftUser.BringToFront();

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

        /// <summary>
        /// ////////////
        /// </summary>
        /// <param name="sLot"></param>
        /// <param name="sWIPSEQ"></param>


        private void BR_PRD_REG_PERMIT_RATE_OVER_HIST()
        {
            try
            {
                DataTable lotTab = _dtRet_Data.DefaultView.ToTable(true, new string[] { "LOTID", "WIPSEQ" });
                string sLot = "";

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataTable inRESN = indataSet.Tables.Add("IN_RESN");
                inRESN.Columns.Add("PERMIT_RATE", typeof(decimal));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(decimal));
                inRESN.Columns.Add("OVER_QTY", typeof(decimal));
                inRESN.Columns.Add("REQ_USERID", typeof(string));
                inRESN.Columns.Add("REQ_DEPTID", typeof(string));
                inRESN.Columns.Add("DIFF_RSN_CODE", typeof(string));
                inRESN.Columns.Add("NOTE", typeof(string));
                for (int j = 0; j < lotTab.Rows.Count; j++)
                {
                    inTable.Rows.Clear();
                    inRESN.Rows.Clear();
                    sLot = lotTab.Rows[j]["LOTID"].ToString();

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EqptID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = sLot;
                    newRow["WIPSEQ"] = lotTab.Rows[j]["WIPSEQ"].ToString();
                    inTable.Rows.Add(newRow);
                    newRow = null;


                    for (int i = 0; i < _dtRet_Data.Rows.Count; i++)
                    {
                        if (sLot.Equals(_dtRet_Data.Rows[i]["LOTID"].ToString()))
                        {
                            newRow = inRESN.NewRow();
                            newRow["PERMIT_RATE"] = Convert.ToDecimal(_dtRet_Data.Rows[i]["PERMIT_RATE"].ToString());
                            newRow["ACTID"] = _dtRet_Data.Rows[i]["ACTID"].ToString();
                            newRow["RESNCODE"] = _dtRet_Data.Rows[i]["RESNCODE"].ToString();
                            newRow["RESNQTY"] = _dtRet_Data.Rows[i]["RESNQTY"].ToString();
                            newRow["OVER_QTY"] = _dtRet_Data.Rows[i]["OVER_QTY"].ToString();
                            newRow["REQ_USERID"] = _sUserID;
                            newRow["REQ_DEPTID"] = _sDepID;
                            newRow["DIFF_RSN_CODE"] = _dtRet_Data.Rows[i]["SPCL_RSNCODE"].ToString();
                            newRow["NOTE"] = _dtRet_Data.Rows[i]["RESNNOTE"].ToString();
                            inRESN.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERMIT_RATE_OVER_HIST", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                        //Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1275");
                    }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }


        private void popRermitRate_Closed(object sender, EventArgs e)
        {

            CMM_PERMIT_RATE popRermitRate = sender as CMM_PERMIT_RATE;
            if (popRermitRate != null && popRermitRate.DialogResult == MessageBoxResult.OK)
            {
                _dtRet_Data.Clear();
                _dtRet_Data = popRermitRate.PERMIT_RATE.Copy();
                _sUserID = popRermitRate.UserID;
                _sDepID = popRermitRate.DeptID;
                _bInputErpRate = true;

                //////////////////////////////
                //확정 처리
                Save_Rate();

            }

        }

        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;
            if (dgDfctDTL != null && dgDfctDTL.Rows.Count == 3)
            {
                if (dgDfctDTL.Rows.Count > 0)
                {
                    dInputQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY")));
                }
            }
            return dInputQty;
        }

        private bool PERMIT_RATE_input(string sLotID, string sWipSeq)
        {
            bool bFlag = false;
            try
            {
                dgDefect.EndEdit();
                //양품 수량을 가지고 온다.
                //double goodQty = GetLotGoodQty(sLotID);
                ////////생산수량을 가지고 온다
                double goodQty = GetLotINPUTQTY(sLotID);

                DataTable data = new DataTable();
                data.Columns.Add("LOTID", typeof(string));
                data.Columns.Add("WIPSEQ", typeof(string));
                data.Columns.Add("ACTID", typeof(string));
                data.Columns.Add("ACTNAME", typeof(string));
                data.Columns.Add("RESNCODE", typeof(string));

                data.Columns.Add("RESNNAME", typeof(string));
                data.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
                data.Columns.Add("RESNQTY", typeof(string));
                data.Columns.Add("PERMIT_RATE", typeof(string));
                data.Columns.Add("OVER_QTY", typeof(string));
                data.Columns.Add("SPCL_RSNCODE", typeof(string));
                data.Columns.Add("SPCL_RSNCODE_NAME", typeof(string));
                data.Columns.Add("RESNNOTE", typeof(string));

                
                decimal dRate = 0;
                decimal dQty = 0;
                decimal dAllowQty = 0;
                decimal OverQty = 0;

                for (int j = 0; j < dgDefect.Rows.Count - dgDefect.BottomRows.Count; j++)
                {
                    // double dRate = 0;
                    dRate = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "PERMIT_RATE"));
                    //등록된 Rate가 0보다 큰것인 것만 적용
                    if (dRate > 0)
                    {
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNQTY")); //여러개인 경우 어떻게?                        
                        //dAllowQty = Math.Truncate(goodQty * dRate / 100); //버림 
                        dAllowQty = Convert.ToDecimal(goodQty) * dRate / 100;
                        if (dAllowQty < dQty)
                        {
                            OverQty = dQty - dAllowQty;
                            OverQty = Math.Ceiling(OverQty); //소수점 첫자리 올림

                            DataRow newRow = data.NewRow();
                            newRow["LOTID"] = sLotID; //필수
                            newRow["WIPSEQ"] = sWipSeq; //필수
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTID")); //필수
                            newRow["ACTNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTNAME")); //필수
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNCODE")); //필수
                            newRow["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNNAME")); //필수
                            newRow["DFCT_CODE_DETL_NAME"] = "";


                            newRow["RESNQTY"] = dQty.ToString("G29"); //필수
                            newRow["PERMIT_RATE"] = dRate.ToString("0.00");  //필수 0제거 
                            newRow["OVER_QTY"] = OverQty.ToString("G29"); //필수 (dQty - dAllowQty).ToString("0.000"); //소수점 3자리

                            newRow["SPCL_RSNCODE"] = "";
                            newRow["SPCL_RSNCODE_NAME"] = "";
                            newRow["RESNNOTE"] = "";
                            data.Rows.Add(newRow);
                        }
                    }
                }


                //등록 할 정보가 있으면 
                if (data.Rows.Count > 0)
                {

                    CMM_PERMIT_RATE popRermitRate = new CMM_PERMIT_RATE { FrameOperation = FrameOperation };
                    object[] parameters = new object[2];
                    parameters[0] = sLotID;
                    parameters[1] = data;
                    C1WindowExtension.SetParameters(popRermitRate, parameters);

                    popRermitRate.Closed += popRermitRate_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popRermitRate.ShowModal()));

                    bFlag = true;
                }

                return bFlag;
                ///////////////////////////////////////////////                  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bFlag;
            }

        }
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

            string messageCode = _WipStat == "PROC" ? "SFU1916" : "SFU2039";
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
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
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
            if (wndShiftUser != null)
                wndShiftUser = null;

            wndShiftUser = new CMM_SHIFT_USER2();
            wndShiftUser.FrameOperation = this.FrameOperation;

            if (wndShiftUser != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_LineID);
                Parameters[3] = Process.SRC;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);  //EQPTID 추가 
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShiftUser_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));                
            }

        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            wndShiftUser = null;

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
        #endregion

        #region [투입정보 탭]
        private void dgInput_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            //try
            //{
            //    if (dgInput != null && dgInput.Rows.Count > 0 && dgInput.Columns.Contains("EQPT_MOUNT_PSTN_NAME"))
            //    {
            //        e.Merge(new DataGridCellsRange(dgInput.GetCell(0, dgInput.Columns["EQPT_MOUNT_PSTN_NAME"].Index), dgInput.GetCell(dgInput.Rows.Count - 1, dgInput.Columns["EQPT_MOUNT_PSTN_NAME"].Index)));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }
        #endregion

        #region [설비불량정보 탭]
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
        private void GetLamiLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                ////new ClientProxy().ExecuteService("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
        /// 매거진 정보
        /// </summary>
        private void GetMagazine()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_MAGAZINE_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_LM", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgMagazine, dtRslt, null);

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
        /// 투입정보
        /// </summary>
        private void GetInputInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["PROD_WIPSEQ"] = _WipSeq.Equals("") ? 1 : Convert.ToDecimal(_WipSeq);
                newRow["INPUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInput, searchResult, null);
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
                newRow["PROCID"] = Process.SRC;
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
        /// 설비불량정보
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
        /// 불량/LOSS/물품청구 저장
        /// </summary>
        /// <param name="bShowMsg"></param>
        private void SetDefect(bool bShowMsg = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                int iSeq = 0;

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
                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
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

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDEFECT_LOT.Rows.Add(newRow);

                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                if (bShowMsg)
                    //Util.AlertInfo("정상 처리 되었습니다.");
                    Util.MessageInfo("SFU1275");

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

                /////////////////////////////////////////////////////////////////////////
                //Rate 관련 팝업
                //완료 처리 하기 전에 팝업 표시
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_LotID, _WipSeq))
                {
                    return;
                }
                /////////////////////////////////////////////////////////////////////////////// 

                //// 실적 확정 처리시 투입수량 > 양품수량 + 불량수량일 경우만 실적 처리 가능하도록 개선
                //// 기본적으로 Flag 에 상관없이 투입수량이 양품 + 불량 수량 보다 크면 SKIP
                //DataTable SearchResult = Util.Get_ConfirmQty(txtLotId.Text.Trim(), Process.SRC, _EqptID, _LineID);

                //if (SearchResult != null && SearchResult.Rows.Count > 0)
                //{
                //    // 체크 여부 
                //    string sChkFlag = SearchResult.Rows[0]["RSLT_CNFM_QTY_CHK_TYPE"].ToString();  // N : 체크안함 , Y : 수량일치, Z : 차이 수량 허용
                //                                                                                  // 허용수량
                //    double dDIFF_PERMIT = Convert.ToDouble(SearchResult.Rows[0]["IN_OUT_DIFF_PERMIT_QTY"].ToString());

                //    // 투입수량
                //    double dInputQty = Convert.ToDouble(SearchResult.Rows[0]["INPUT_QTY"].ToString());
                //    // 양품수량
                //    double dGoodQty = Convert.ToDouble(SearchResult.Rows[0]["GOOD_QTY"].ToString());
                //    // 불량수량
                //    double dDefectQty = Convert.ToDouble(SearchResult.Rows[0]["DEFECT_QTY"].ToString());
                //    // 양품수량 + 불량수량
                //    double dOutQty = dGoodQty + dDefectQty;

                //    // 투입/완성 수량 일치
                //    if (sChkFlag == "Y")
                //    {
                //        if (dInputQty != dOutQty)
                //        {
                //            // 투입수량과 양품+불량 수량이 일치하지 않습니다.
                //            Util.MessageValidation("SFU4259");
                //            return;
                //        }
                //        else if (dInputQty < dOutQty)
                //        {
                //            // 투입수량이 양품+불량 수량보다 적습니다.
                //            Util.MessageValidation("SFU4260");
                //            return;
                //        }
                //    }
                //    // 투입/완성 수량 허용범위 체크
                //    else if (sChkFlag == "Z")
                //    {
                //        if (dInputQty < dOutQty)
                //        {
                //            double dCalc = dOutQty - dInputQty;

                //            if (dDIFF_PERMIT < dCalc)
                //            {
                //                // 허용범위를 초과하였습니다.
                //                Util.MessageValidation("SFU4261");
                //                return;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    //Util.Alert("LOT 정보가 없습니다.");
                //    Util.MessageValidation("SFU1195");
                //    return;
                //}

                ShowLoadingIndicator();

                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_END_LOT_SRC();

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
                newRow["WIPNOTE"] = txtRemark.Text;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = dtTime;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable input_LOT = indataSet.Tables["IN_INPUT"];
                //newRow = input_LOT.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";
                //newRow["INPUT_LOTID"] = "";

                //input_LOT.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_SRC", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
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

                        Util.MessageInfo("SFU1275");

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


        private void Save_Rate()
        {
            try
            {
           
                ShowLoadingIndicator();

                DateTime dtTime;
                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_END_LOT_SRC();

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
                newRow["WIPNOTE"] = txtRemark.Text;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = dtTime;

                inTable.Rows.Add(newRow);
                newRow = null;

    

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_SRC", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        btnDefectSave.IsEnabled = false;
                        btnSave.IsEnabled = false;

                        Util.MessageInfo("SFU1275");

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

        private void GetAllData()
        {
            ClearControls();

            InitializeDfctDtl();
            GetLamiLotInfo();
            GetMagazine();
            GetInputInfo();
            GetDefectInfo();
            GetEqpDefectfo();
        }
        
        private void ClearControls()
        {
            Util.gridClear(dgDfctDTL);
            Util.gridClear(dgMagazine);
            Util.gridClear(dgInput);
            Util.gridClear(dgDefect);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDefectSave);
            listAuth.Add(btnSave);

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

        #endregion

        #endregion

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "RESNQTY")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            //e.Cell.Column.IsReadOnly = false;
                            //e.Cell.Column.EditOnSelection = true;
                        }
                    }
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
