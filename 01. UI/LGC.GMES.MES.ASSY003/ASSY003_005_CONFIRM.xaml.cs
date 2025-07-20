/*************************************************************************************
 Created Date : 2017.06.15
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Folding 공정진척 화면 - 실적 확인 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.15  INS 김동일K : Initial Created.
  2024.03.01  남기운K      : (NERP 대응 프로젝트)실적처리 허용비율 초과 입력 기능추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_005_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_005_CONFIRM : C1Window, IWorkArea
    {   
        #region Declaration & Constructor
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

        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;

        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;
        private string _WO_DETL_ID = string.Empty;

        private int iCnt = 0;
        private DataTable dtBOM = new DataTable();
        private DataTable dtBOM_CHK = new DataTable();
        private int iBomcnt = 0;


        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;



        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

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
        public ASSY003_005_CONFIRM()
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
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경        
            if (wndShiftUser != null)
                wndShiftUser.BringToFront();
            #endregion

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
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeDfctDtl();
        }

        /// <summary>
        /// /////////////////
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
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "CALC_FC")); //계산수량? //INPUT_FC
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
                    Save();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = bSave ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }

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
                int iChk = 1;
                int iTot = 0;
                double Min = 0;
                double Max = 0;
                List<double> dIndex = new List<double>();

                string sColname = string.Empty;
                string sCol = string.Empty;
                string sNo = string.Empty;

                sColname = e.Cell.Column.Name;

                sCol = sColname.Substring(0, 3);
                sNo = sColname.Substring(3, 1);

                int iColIdx = 0;
                int iRowIdx = 0;

                iColIdx = dgDefect.Columns[sColname].Index;
                iRowIdx = e.Cell.Row.Index;

                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                {
                    if (iBomcnt == 1)
                    {
                        //for (int i = 0; i < iCnt + 1; i++)
                        //{
                        int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                        string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG" + ichk.ToString()));

                        double dTemp = double.Parse(sTemp);

                        if (dTemp == 0)
                        {
                            iChk = 0;
                        }

                        double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                        double Division = Math.Floor(dTemp / dInputCnt);
                        dIndex.Add(Division);

                        iTot = iTot + 1;
                        //iFcChk = 0;
                        //}
                    }
                    else
                    {
                        for (int i = 0; i < iCnt + 1; i++)
                        {
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "REG" + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                            //iFcChk = 0;
                        }
                    }
                }
                else
                {
                    if (iBomcnt == 1)
                    {
                        //for (int i = 0; i < iCnt + 1; i++)
                        //{
                        int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                        string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                        double dTemp = double.Parse(sTemp);

                        if (dTemp == 0)
                        {
                            iChk = 0;
                        }

                        double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                        double Division = Math.Floor(dTemp / dInputCnt);
                        dIndex.Add(Division);

                        iTot = iTot + 1;
                        //}
                    }
                    else
                    {
                        for (int i = 0; i < iCnt + 1; i++)
                        {
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;
                        }
                    }
                }

                Max = dIndex[0];
                Min = dIndex[0];

                for (int icnt = 0; icnt < iTot; icnt++)
                {
                    if (Max < dIndex[icnt])
                    {
                        Max = dIndex[icnt];
                    }

                    if (Min > dIndex[icnt])
                    {
                        Min = dIndex[icnt];
                    }
                }


                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                {
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "INPUT_FC"));
                    double Reg_Resnqty = double.Parse(sReg_FC);

                    //if (iChk == 0 && iFcChk == 0)
                    if (iChk == 0)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty + Min);
                    }
                }
                else
                {
                    if (iChk != 0)
                    {
                        if (iBomcnt == 1)
                        {
                            //for (int i = 0; i < iCnt + 1; i++)
                            //{
                            int ichk = Get_Type_Chk(Util.NVC(dtBOM_CHK.Rows[0]["PRODUCT_LEVEL3_CODE"]));

                            string sTemp = String.Empty;

                            sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                            double dTemp = double.Parse(sTemp);

                            if (dTemp == 0)
                            {
                                iChk = 0;
                            }

                            double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM_CHK.Rows[0]["PROC_INPUT_CNT"]));

                            double Division = Math.Floor(dTemp / dInputCnt);
                            dIndex.Add(Division);

                            iTot = iTot + 1;

                            if (e.Cell.Column.Name.Equals("INPUT_FC"))
                                DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", dTemp - (Min * dInputCnt));
                            //else
                            //    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString(), dTemp - (Min * dInputCnt));
                            //}
                        }
                        else
                        {
                            for (int i = 0; i < iCnt + 1; i++)
                            {
                                int ichk = Get_Type_Chk(Util.NVC(dtBOM.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                                string sTemp = String.Empty;

                                sTemp = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString()));

                                double dTemp = double.Parse(sTemp);

                                if (dTemp == 0)
                                {
                                    iChk = 0;
                                }

                                double dInputCnt = Convert.ToDouble(Util.NVC(dtBOM.Rows[i]["PROC_INPUT_CNT"]));

                                double Division = Math.Floor(dTemp / dInputCnt);
                                dIndex.Add(Division);

                                iTot = iTot + 1;

                                if (e.Cell.Column.Name.Equals("INPUT_FC"))
                                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", dTemp - (Min * dInputCnt));
                                //else
                                //    DataTableConverter.SetValue(dgDefect_NJ.Rows[e.Cell.Row.Index].DataItem, sCol + ichk.ToString(), dTemp - (Min * dInputCnt));
                            }
                        }
                    }
                    string sReg_FC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "INPUT_FC"));
                    double Reg_Resnqty = double.Parse(sReg_FC);
                    DataTableConverter.SetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "CALC_FC", Reg_Resnqty + Min);
                    //}
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
                Parameters[3] = Process.STACKING_FOLDING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

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
                Parameters[3] = Process.STACKING_FOLDING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);  //EQPTID 추가 
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShiftUser_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));                
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
        private void GetFoldingLotInfo()
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

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_BOX_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                //dgBox.ItemsSource = DataTableConverter.Convert(dtRslt);
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
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetProductionResult()
        {
            try
            {
                ShowLoadingIndicator();

                // 실적 확정 처리시 투입수량 > 양품수량 + 불량수량일 경우만 실적 처리 가능하도록 개선
                // 기본적으로 Flag 에 상관없이 투입수량이 양품 + 불량 수량 보다 크면 SKIP
                DataTable SearchResult = Util.Get_ResultQty_Chk(_LotID, Process.STACKING_FOLDING, _EqptID, _LineID, _WipSeq);

                Util.GridSetData(dgResult, SearchResult, FrameOperation, true);

                if (SearchResult.Rows.Count > 0 && SearchResult.Select("ALT_ITEM_YN = 'Y'").Length > 0)
                {
                    //string[] sColumnName = new string[] { "PRODUCT_LEVEL123_CODE", "MTRLID" };
                    //_Util.SetDataGridMergeExtensionCol(dgResult, sColumnName, DataGridMergeMode.VERTICAL);

                    dgResult.GroupBy(dgResult.Columns["PRODUCT_LEVEL123_CODE"], DataGridSortDirection.None);
                    dgResult.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["MTRLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["MTRL_INPUT_QTY_1EA"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["MTRLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["PRODUCT_LEVEL1_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["PRODUCT_LEVEL2_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["PRODUCT_LEVEL3_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["PRODUCT_LEVEL123_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["MTRL_INPUT_QTY_1EA"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["MTRL_INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["PROD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["GOOD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgResult.Columns["DEFECT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["STATUS"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });
                    DataGridAggregate.SetAggregateFunctions(dgResult.Columns["RSLT_CNFM_QTY_CHK_TYPE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = "" } });

                    // Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));

                    dgResult.MergingCells -= dgResult_MergingCells;
                    dgResult.MergingCells += dgResult_MergingCells;
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

        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(string));
                inDataTable.Columns.Add("ACTID", typeof(string));
                //inDataTable.Columns.Add("TYPE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                //newRow["TYPE"] = _StackingYN;

                inDataTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_CELL_TYPE_DFCT_HIST_NJ", "INDATA", "OUTDATA", inDataTable);

                if (searchResult != null)
                {
                    //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
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

        private void GetMBOMInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = _WO_DETL_ID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["CMCDTYPE"] = "NJ_FOLDING_TYPE";

                inTable.Rows.Add(newRow);

                dtBOM_CHK = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MBOM_INFO_STP", "RQSTDT", "RSLTDT", inTable);

                iBomcnt = dtBOM_CHK.Rows.Count;

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //타입별 불량 기준정보가 존재하지 않습니다.
                            return;
                        }

                        InitDefectDataGrid();

                        dtBOM = searchResult.Copy();

                        for (int i = 0; i < searchResult.Rows.Count; i++)
                        {
                            string sColName = string.Empty;
                            string sColName2 = string.Empty;
                            string sHeader = string.Empty;

                            List<string> dIndex = new List<string>();
                            List<string> dIndex2 = new List<string>();

                            int ichk = 0;

                            ichk = Get_Type_Chk(Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]));

                            sColName = "REG" + (ichk).ToString();
                            sHeader = Util.NVC(searchResult.Rows[i]["ATTRIBUTE3"]).ToString();

                            // 불량 수량 컬럼 위치 변경.
                            int iColIdx = 0;

                            dIndex.Add(ObjectDic.Instance.GetObjectName("입력"));
                            dIndex.Add(sHeader);

                            iColIdx = dgDefect.Columns["INPUT_FC"].Index;

                            Util.SetGridColumnNumeric(dgDefect, sColName, dIndex, sHeader, true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // [입력, FOLDED CELL]     

                            if (dgDefect.Columns.Contains(sColName))
                            {
                                (dgDefect.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                                (dgDefect.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                                (dgDefect.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                            }

                            if (dgDefect.Rows.Count == 0) continue;

                            SetBOMCnt(i, Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]), Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"]));

                            iCnt = i;

                            C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dgDefect.Columns[sColName]
                            , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

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

                string sInAType = string.Empty;
                string sInCType = string.Empty;
                string sInLType = string.Empty;
                string sInRType = string.Empty;
                string sInMLType = string.Empty;
                string sInMRType = string.Empty;

                string sOut = string.Empty;

                string sAtype = string.Empty;
                string sCtype = string.Empty;
                string sFtype = string.Empty;

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("SHOPID", typeof(String));
                RQSTDT1.Columns.Add("PROCID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = _LotID;
                dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr1["PROCID"] = Process.STACKING_FOLDING;
                RQSTDT1.Rows.Add(dr1);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PRODUCT_LEVEL_CODE_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("WIPSEQ", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("ACTID", typeof(String));
                RQSTDT.Columns.Add("RESNCODE", typeof(String));
                RQSTDT.Columns.Add("REG_QTY", typeof(Decimal));
                RQSTDT.Columns.Add("CALC_QTY", typeof(Decimal));
                RQSTDT.Columns.Add("USERID", typeof(String));

                for (int icnt = 2; icnt < dgDefect.GetRowCount() + 2; icnt++)
                {
                    for (int jcnt = 0; jcnt < SearchResult.Rows.Count; jcnt++)
                    {
                        string sCode = Util.NVC(SearchResult.Rows[jcnt]["PRODUCT_LEVEL3_CODE"]);

                        if (sCode.Equals("FC") || sCode.Equals("SC"))
                        {
                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = Util.NVC(SearchResult.Rows[jcnt]["PRODID"]);
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "INPUT_FC"));
                            dr["CALC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "CALC_FC"));
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                        else
                        {
                            int ichk = Get_Type_Chk(sCode);

                            DataRow dr = RQSTDT.NewRow();
                            dr["LOTID"] = _LotID;
                            dr["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                            dr["PRODID"] = Util.NVC(SearchResult.Rows[jcnt]["PRODID"]);
                            dr["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "ACTID"));
                            dr["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "RESNCODE"));
                            dr["REG_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[icnt].DataItem, "REG" + ichk.ToString()));
                            //dr["CALC_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[icnt].DataItem, "CALC_FC"));
                            dr["CALC_QTY"] = 0;
                            dr["USERID"] = LoginInfo.USERID;

                            RQSTDT.Rows.Add(dr);
                        }
                    }
                }

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CELL_TYPE_DFCT_HIST", "INDATA", null, RQSTDT);

                //new ClientProxy().ExecuteService("BR_PRD_REG_CELL_TYPE_DFCT_HIST", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        if (bizException != null)
                //        {
                //            Util.MessageException(bizException);
                //            return;
                //        }

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                //for (int i = 0; i < dgDefect_NJ.Rows.Count - dgDefect_NJ.BottomRows.Count; i++)
                for (int i = 2; i < dgDefect.GetRowCount() + 2; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = _LotID.Trim();
                    newRow["WIPSEQ"] = int.TryParse(_WipSeq, out iSeq) ? iSeq : 1;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_FC")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "CALC_FC")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_A")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_A")));
                    //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_C")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect_NJ.Rows[i].DataItem, "CALC_C")));

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult2, bizException2) =>
                {
                    try
                    {
                        if (bizException2 != null)
                        {
                            Util.MessageException(bizException2);
                            return;
                        }

                        if (bShowMsg)
                            Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

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
                }, indataSet);

                GetDefectInfo();
                GetProductionResult();
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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_FD", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
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
                newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WIPNOTE"] = txtRemark.Text;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_FD", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
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

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }

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
            
            if (!(bool)chkSkip.IsChecked)
            {
                for (int i = dgResult.TopRows.Count; i < dgResult.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgResult.Rows[i].DataItem, "STATUS")) == "NG")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgResult.Rows[i].DataItem, "RSLT_CNFM_QTY_CHK_TYPE")) == "Y")
                        {
                            // 투입수량과 양품+불량 수량이 일치하지 않습니다.
                            Util.MessageValidation("SFU4259");
                            return bRet;
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(dgResult.Rows[i].DataItem, "RSLT_CNFM_QTY_CHK_TYPE")) == "Z")
                        {
                            // 허용범위를 초과하였습니다.
                            Util.MessageValidation("SFU4261");
                            return bRet;
                        }
                    }
                }
            }            

            bRet = true;
            return bRet;
        }

        private bool CanSaveDefect()
        {
            bool bRet = false;

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return bRet;
            }

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
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(CALC_FC)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(CALC_FC)", "ACTID = 'DEFECT_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(CALC_FC)", "ACTID = 'LOSS_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(CALC_FC)", "ACTID = 'LOSS_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(CALC_FC)", "ACTID = 'CHARGE_PROD_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(CALC_FC)", "ACTID = 'CHARGE_PROD_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(CALC_FC)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(CALC_FC)", string.Empty).ToString()));
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
            GetProductionResult();
            GetMBOMInfo();
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

        private void dgResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("INPUT_QTY") || e.Cell.Column.Name.Equals("IN_OUT_DIFF_PERMIT_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                    }
                    else if (e.Cell.Column.Name.Equals("MTRL_INPUT_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                    else if (e.Cell.Column.Name.Equals("STATUS"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals("NG"))
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                        e.Cell.Presenter.Background = null;
                }
            }));
        }

        private void dgResult_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

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
                    //if (Convert.ToString(e.Cell.Column.Name) == "REG_A" || Convert.ToString(e.Cell.Column.Name) == "REG_C" || Convert.ToString(e.Cell.Column.Name) == "REG_L" 
                    //|| Convert.ToString(e.Cell.Column.Name) == "REG_R" || Convert.ToString(e.Cell.Column.Name) == "REG_ML" || Convert.ToString(e.Cell.Column.Name) == "REG_MR")
                    //{
                    if (e.Cell.Column.Name.ToString().StartsWith("REG"))
                    {
                        string sActid = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "ACTID"));
                        if (sActid == "DEFECT_LOT")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        }
                        else
                        {
                            //e.Cell.Column.EditOnSelection = false;
                            //e.Cell.Column.IsReadOnly = true;
                            //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "INPUT_FC")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
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

                //if (e.Column.Name.Equals("REG_A") || e.Column.Name.Equals("REG_C") || e.Column.Name.Equals("REG_L") || e.Column.Name.Equals("REG_R")
                //    || e.Column.Name.Equals("REG_ML") || e.Column.Name.Equals("REG_MR"))
                if (e.Column.Name.ToString().StartsWith("REG"))
                {
                    string sActid = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ACTID"));
                    if (sActid != "DEFECT_LOT")
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

        private void C1TabItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                     (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                     (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    chkSkip.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgResult_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {                
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgResult.TopRows.Count; i < dgResult.Rows.Count; i++)
                {
                    if (dgResult.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgResult.Rows[i].DataItem, "PRODUCT_LEVEL123_CODE"));
                            idxS = i;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgResult.Rows[i].DataItem, "PRODUCT_LEVEL123_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["MTRL_INPUT_QTY_1EA"].Index), dgResult.GetCell(idxE, dgResult.Columns["MTRL_INPUT_QTY_1EA"].Index)));
                                e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["INPUT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["INPUT_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["MTRL_INPUT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["MTRL_INPUT_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["STATUS"].Index), dgResult.GetCell(idxE, dgResult.Columns["STATUS"].Index)));
                                bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["MTRL_INPUT_QTY_1EA"].Index), dgResult.GetCell(idxE, dgResult.Columns["MTRL_INPUT_QTY_1EA"].Index)));
                            e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["INPUT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["INPUT_QTY"].Index)));
                            e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"].Index)));
                            e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["MTRL_INPUT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["MTRL_INPUT_QTY"].Index)));
                            e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["STATUS"].Index), dgResult.GetCell(idxE, dgResult.Columns["STATUS"].Index)));
                            bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["MTRL_INPUT_QTY_1EA"].Index), dgResult.GetCell(idxE, dgResult.Columns["MTRL_INPUT_QTY_1EA"].Index)));
                    e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["INPUT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["INPUT_QTY"].Index)));
                    e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["IN_OUT_DIFF_PERMIT_QTY"].Index)));
                    e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["MTRL_INPUT_QTY"].Index), dgResult.GetCell(idxE, dgResult.Columns["MTRL_INPUT_QTY"].Index)));
                    e.Merge(new DataGridCellsRange(dgResult.GetCell(idxS, dgResult.Columns["STATUS"].Index), dgResult.GetCell(idxE, dgResult.Columns["STATUS"].Index)));
                    bStrt = false;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void InitDefectDataGrid(bool bClearAll = false)
        {
            if (bClearAll)
            {
                Util.gridClear(dgDefect);

                for (int i = dgDefect.Columns.Count; i-- > 0;)
                {
                    if (dgDefect.Columns[i].Name.ToString().StartsWith("REG"))
                    {
                        dgDefect.Columns.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgDefect.Columns.Count; i-- > 0;)
                {
                    if (dgDefect.Columns[i].Name.ToString().StartsWith("REG"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);
                        if (dt.Columns.Count > i)
                            if (dt.Columns[i].ColumnName.Equals(dgDefect.Columns[i].Name))
                                dt.Columns.RemoveAt(i);

                        dgDefect.Columns.RemoveAt(i);
                    }
                }
            }
        }

        private int Get_Type_Chk(string sType)
        {
            int iCode = 0;

            if (sType == "AM")
            {
                iCode = 0;
            }
            else if (sType == "AT")
            {
                iCode = 1;
            }
            else if (sType == "CM")
            {
                iCode = 2;
            }
            else if (sType == "CT")
            {
                iCode = 3;
            }
            else if (sType == "LM")
            {
                iCode = 4;
            }
            else if (sType == "LT")
            {
                iCode = 5;
            }
            else if (sType == "MA")
            {
                iCode = 6;
            }
            else if (sType == "MB")
            {
                iCode = 7;
            }
            else if (sType == "ML")
            {
                iCode = 8;
            }
            else if (sType == "MM")
            {
                iCode = 9;
            }
            else if (sType == "MR")
            {
                iCode = 10;
            }
            else if (sType == "MT")
            {
                iCode = 11;
            }
            else if (sType == "RM")
            {
                iCode = 12;
            }
            else if (sType == "RT")
            {
                iCode = 13;
            }

            return iCode;
        }

        private void SetBOMCnt(int i, string sCode, string sCnt)
        {
            if (i == 0)
            {
                tbType0.Visibility = Visibility.Visible;
                txtType0.Visibility = Visibility.Visible;

                tbType0.Text = sCode;
                txtType0.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 1)
            {
                tbType1.Visibility = Visibility.Visible;
                txtType1.Visibility = Visibility.Visible;

                tbType1.Text = sCode;
                txtType1.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 2)
            {
                tbType2.Visibility = Visibility.Visible;
                txtType2.Visibility = Visibility.Visible;

                tbType2.Text = sCode;
                txtType2.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 3)
            {
                tbType3.Visibility = Visibility.Visible;
                txtType3.Visibility = Visibility.Visible;

                tbType3.Text = sCode;
                txtType3.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 4)
            {
                tbType4.Visibility = Visibility.Visible;
                txtType4.Visibility = Visibility.Visible;

                tbType4.Text = sCode;
                txtType4.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 5)
            {
                tbType5.Visibility = Visibility.Visible;
                txtType5.Visibility = Visibility.Visible;

                tbType5.Text = sCode;
                txtType5.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 6)
            {
                tbType6.Visibility = Visibility.Visible;
                txtType6.Visibility = Visibility.Visible;

                tbType6.Text = sCode;
                txtType6.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 7)
            {
                tbType7.Visibility = Visibility.Visible;
                txtType7.Visibility = Visibility.Visible;

                tbType7.Text = sCode;
                txtType7.Text = Convert.ToDouble(sCnt).ToString();
            }
            else if (i == 8)
            {
                tbType8.Visibility = Visibility.Visible;
                txtType8.Visibility = Visibility.Visible;

                tbType8.Text = sCode;
                txtType8.Text = Convert.ToDouble(sCnt).ToString();
            }
        }
    }
}
