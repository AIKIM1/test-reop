/*************************************************************************************
 Created Date : 2022.02.11
      Creator : 
   Decription : CT 검사 공정 실적확정
--------------------------------------------------------------------------------------
 [Change History]
  2022.02.11  INS 김동일   : Initial Created.  
  2023.04.25  INS 안유수   E20230405-000880  투입량 = 양품량 + 불량/LOSS 수량 차이 발생 시 확정하기 전 알림 팝업창 추가  
  2024.01.25  남기운K      : (NERP 대응 프로젝트)실적처리 허용비율 초과 입력 기능추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_011_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_011_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private object ob_lock = new object();

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;
        private bool bSave = false;
        private System.DateTime dtNow;
        private bool bEndSetTime = false;

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;

        private double gDfctAQty = 0;
        private double gDfctCQty = 0;

        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;

        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;

        private string _EQPT_DFCT_APPLY_FLAG = string.Empty;
        private string _WO_DETL_ID = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _WorkTypeCode = string.Empty;

        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;

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
        public ASSY001_011_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (dgBox.Columns.Contains("CSTID"))
                    dgBox.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgBox.Columns.Contains("CSTID"))
                    dgBox.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }
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

            if (tmps != null && tmps.Length >= 13)
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

                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[11]);
                _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[12]);
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

                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";
            }

            ApplyPermissions();
            InitializeControls();

            #region SCRAP
            _WorkTypeCode = GetWorkTypeCode();

            if (_WorkTypeCode.Equals("SCRAP"))
                dgDefect.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Visible;
            #endregion

            GetAllData();

            bEndSetTime = true;

            txtShift.Text = _RetShiftName;
            txtShift.Tag = _RetShiftCode;
            txtWorker.Text = _RetUserName;
            txtWorker.Tag = _RetUserID;

        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeDfctDtl();
        }
        /// <summary>
        /// ////////////////////
        /// </summary>
        /// <param name="sLot"></param>
        /// <param name="sProcID"></param>
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

        private double GetLotGoodQty(string sLotID)
        {
            double goodQty = 0;

            if (dgDfctDTL != null && dgDfctDTL.Rows.Count == 3)
            {
                if (dgDfctDTL.Rows.Count > 0)
                {
                    goodQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[2].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[2].DataItem, "GOODQTY")));
                }
            }

            return goodQty;
        }

        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;
            if (dgDfctDTL != null && dgDfctDTL.Rows.Count == 3)
            {
                if (dgDfctDTL.Rows.Count > 0)
                {
                    dInputQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[2].DataItem, "OUTPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[2].DataItem, "OUTPUTQTY")));
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

                string sRate = "";
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
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNQTY")); //불량수량                      
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
                            newRow["DFCT_CODE_DETL_NAME"] = "";// Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "DFCT_CODE_DETL_NAME"));
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
            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (inputqtyConfirm())
                    {
                        Util.MessageConfirm("SUF9015", (result2) =>
                        {
                            if (result2 == MessageBoxResult.OK)
                            {
                                Save();
                            }
                        });
                    }
                    else
                    {
                        Save();
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = bSave ? MessageBoxResult.OK : MessageBoxResult.Cancel;
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

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT wndPopup = new CMM_SHIFT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = _LineID;
                Parameters[3] = Process.CT_INSP;
                Parameters[4] = Util.NVC(txtShift.Tag);
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

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
                Parameters[3] = Process.CT_INSP;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);  //EQPTID 추가 
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));                
            }
        }

        private void btnEqpDefectSearch_Click(object sender, RoutedEventArgs e)
        {
            GetEqpDefectfo();
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                {
                    this.Focus();
                    return;
                }

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals(""))
                {
                    this.Focus();
                    return;
                }

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }

                this.Focus();
            }));
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetProdLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtLotId.Text = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    txtProdId.Text = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    txtWorkOrder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
                    _WO_DETL_ID = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);

                    txtStartTime.Text = Util.NVC(dtRslt.Rows[0]["WIPDTTM_ST"]);
                    txtEndTime.Text = Util.NVC(dtRslt.Rows[0]["EQPT_END_DTTM"]);

                    txtRemark.Text = Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]);

                    // Caldate Lot의 Caldate로...
                    if (Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]).Trim().Equals(""))
                    {
                        dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"])).ToLongDateString();
                        dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));

                        sCaldate = Util.NVC(dtRslt.Rows[0]["NOW_CALDATE_YMD"]);
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["NOW_CALDATE"]));
                    }
                    else
                    {
                        dtpCaldate.Text = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"])).ToLongDateString();
                        dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]));

                        sCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"])).ToString("yyyyMMdd");
                        dtCaldate = Convert.ToDateTime(Util.NVC(dtRslt.Rows[0]["CALDATE_LOT"]));
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

        private void GetBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CI", "INDATA", "OUTDATA", inTable);



                DataTable inTable2 = new DataTable();
                inTable2.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow2 = inTable2.NewRow();
                newRow2["PROD_LOTID"] = _LotID;

                inTable2.Rows.Add(newRow2);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_MTRL_QTY_CT", "INDATA", "OUTDATA", inTable2); //생산량(투입량)

                //dgBox.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgBox, dtRslt, null, true);

                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY",  dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtRslt.Compute("SUM(WIPQTY)", string.Empty).ToString()));
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY", dtRslt2.Rows[0]["INPUT_QTY"].ToString().Equals("") ? 0 : dtRslt2.Rows[0]["INPUT_QTY"]);
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

        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.CT_INSP;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", inTable);
                
                if (searchResult != null)
                {
                    //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                    Util.GridSetData(dgDefect, searchResult, null, true);

                    SumDefectQty();
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

                        //dgEqpDefect.ItemsSource = DataTableConverter.Convert(searchResult);
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
                //DataTable inLOSS_LOT = indataSet.Tables["INLOSS_LOT"];
                //DataTable inREPAIR_LOT = indataSet.Tables["INREPAIR_LOT"];
                //DataTable inSCRAP_LOT = indataSet.Tables["INSCRAP_LOT"];

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

        private void Save()
        {
            try
            {

                lock (ob_lock)
                {
                    // 자동 불량 저장 처리.
                    SaveDefectAllBeforeConfirm();
                }

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
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


                ShowLoadingIndicator();

                DateTime dtTime;

                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_CONFIRM_LOT_FD();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["INPUTQTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")));
                newRow["OUTPUTQTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CI", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void Save_Rate()
        {
            try
            {
                ShowLoadingIndicator();

                DateTime dtTime;

                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_CONFIRM_LOT_FD();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = txtLotId.Text;
                newRow["INPUTQTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")));
                newRow["OUTPUTQTY"] = 0;// Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WIPNOTE"] = txtRemark.Text;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

         

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CI", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
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

                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        bSave = true;



                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                   
                }, indataSet);
           



            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
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

            if (_WorkTypeCode.Equals("SCRAP"))
            {
                DataTable dt = ((DataView)dgDefect.ItemsSource).Table.Select().CopyToDataTable();
                int sum = (int)dt.AsEnumerable().Sum(r => r.Field<decimal>("DFCT_QTY_DDT_RATE"));
                if ((sum != 100))
                {
                    Util.MessageValidation("SFU4518");      //불량 재생비율의 합은 100% 입니다.
                    return bRet;
                }
            }

            bRet = !qtyConfirm();

            //bRet = true;

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

        #region [PopUp Event]
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
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
                //txtShiftDateTime = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                _RetWrkStrtTime = Util.NVC(wndPopup.WRKSTRTTIME);
                _RetWrkEndTime = Util.NVC(wndPopup.WRKENDTTIME);

                _RetShiftCode = Util.NVC(wndPopup.SHIFTCODE);
                _RetShiftName = Util.NVC(wndPopup.SHIFTNAME);
                _RetUserID = Util.NVC(wndPopup.USERID);
                _RetUserName = Util.NVC(wndPopup.USERNAME);
            }
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
                dgDfctDTL.Refresh();
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
            GetProdLotInfo();
            GetBox();
            GetEqpt_Dfct_Apply_Flag();
            GetDefectInfo();
            GetEqpDefectfo();
        }

        private void GetEqpt_Dfct_Apply_Flag()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Process.CT_INSP;
                dr["EQSGID"] = _LineID;

                RQSTDT.Rows.Add(dr);

                DataTable SResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DFCT_APPLY_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

                _EQPT_DFCT_APPLY_FLAG = SResult.Rows[0]["EQPT_DFCT_APPLY_FLAG"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private string GetWorkTypeCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = _LotID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    return Util.NVC(row["WIP_WRK_TYPE_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }
        
        #endregion

        #endregion

        private void dgDefect_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    e.Cell.Presenter.Background = null;
                }
            }));
        }
        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(e.Column.Name).Equals("RESNQTY"))
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

        private void dgDefect_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            SumDefectQty();
        }

        private void dgDefect_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1.WPF.DataGrid.C1DataGrid grd = sender as C1.WPF.DataGrid.C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }

        private bool qtyConfirm()
        {
            // 언로더 공용 설비 차감후 양품수량 음수 체크.
            if (GetUnldrSharFlag())
            {
                DataTable dtTgt = DataTableConverter.Convert(dgDfctDTL.ItemsSource);
                DataTable dtSrc = DataTableConverter.Convert(dgDefect.ItemsSource);

                double dtlDfct = 0;

                if (dtSrc != null && dtSrc.Rows.Count > 0)
                {
                    // 완성LOT 수량차감
                    dtlDfct = dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') = 'OUT_LOT_QTY_DEDUCT'").ToString().Equals("") ? 0 : double.Parse(dtSrc.Compute("SUM(RESNQTY)", "RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') = 'OUT_LOT_QTY_DEDUCT'").ToString());
                }
                else
                {
                    dtlDfct = 0;
                }

                double dGoodQty = Util.NVC(dtTgt.Rows[0]["GOODQTY"]).Equals("") ? 0 : double.Parse(Util.NVC(dtTgt.Rows[0]["GOODQTY"]));                

                double dGoodQtyAfterDeduct = dGoodQty - dtlDfct;

                if (dGoodQtyAfterDeduct < 0)
                {
                    // 입력오류 : 차감 후 양품수량이 음수로 처리할 수 없습니다.
                    Util.MessageValidation("SFU6054");
                    return true;
                }
            }

            //미확인 LOSS를 사용하는지 확인
            if (GetAreaByCheckData())
            {
                return false;
            }
            return false;
        }

        private bool inputqtyConfirm()
        {
            double dConsValue = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY")) == "" ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY")))  // 생산량
                              - (Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")) == "" ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY"))))   // 양품량
                              - (Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT")) == "" ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT")))) // 불량량
                              - (Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS")) == "" ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS")))) // Loss량
                              - (Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD")) == "" ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD")))); // 물품청구량

            if (dConsValue != 0)
            {
                return true;
            }
            return false;
        }

        private bool GetAreaByCheckData()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CT_INSP_AREA_BY_CHECK";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", inTable);

                if (dtResult.Rows[0]["ATTRIBUTE2"].Equals("Y") || string.IsNullOrEmpty(dtResult.Rows[0]["ATTRIBUTE2"].ToString()))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private bool GetUnldrSharFlag()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["UNLDR_SHR_FLAG"]).Equals("Y"))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return false;
        }
    }
}
