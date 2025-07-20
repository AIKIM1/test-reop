/*************************************************************************************
 Created Date : 2017.06.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  INS 김동일K : Initial Created.
  2021.03.11 최상민 : 소형 노칭 실적 확정시 불량/LOSS/물청 여러번 실행 및 V/D 공정 불량/LOSS/물청 데이터 생성 문제 처리
  2021.06.23 김동일 : C20210514-000019 설정의 설비정보로 극성 및 설비 콤보 자동 선택 기능 추가 건
  2023.06.25 조영대 : 설비 Loss Level 2 Code 사용 체크 및 변환
  2024.02.14 남기운 : 허용비율 초과시 사유 등록 프로세서 추가
  2024.02.20 조범모   [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
  2024.05.07 조범모   [E20240428-001991] TWS 로딩량 추적관리 업무개선 요청 건
  2024.07.15 조범모 : [E20240715-000063] 전극공정진척 Roll in & Roll out 방향 기본 세팅값을 시스템 상 미리 표기
  2024.08.22 유명환 : [E20240807-000878] 하위 잔존가치를 50M -> 5M으로 변경
  2024.08.30 유명환 : [E20240619-000917] Loding lebel 추가
**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_001.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_001 : UserControl, IWorkArea
    {   
        #region Declaration & Constructor

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;

        private bool bTestMode = false;
        private string sTestModeType = string.Empty;

        private bool bLoaded = true;
        private bool bchkWait = true;
        private bool bchkRun = true;
        private bool bchkEqpEnd = true;

        private bool bWndConfirmJobEnd = false;  //로직이 중복으로 타는 현상 발생해 방지하기 위해

        private string _MUST_CHG_FLAG = string.Empty;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        CurrentLotInfo _CurrentLotInfo = new CurrentLotInfo();

        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();

        private string _Max_Pre_Proc_End_Day = string.Empty;
        private DateTime _Min_Valid_Date;

        string _sPGM_ID = "ASSY003_001";

        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;        
        bool _bInputErpRate = false;


        #region Popup 처리 로직 변경
        //ASSY003_001_CONFIRM wndConfirm;
        ASSY003_001_EQPEND wndEqpEnd;
        ASSY003_001_HIST wndHist;
        ASSY003_001_LOTCOMMENTHIST wndCommentHist;
        ASSY003_001_RUNSTART wndRunStart;
        ASSY003_001_WAITLOT wndWaitLot;
        CMM_ASSY_QUALITY_PKG wndQualitySearch;
        CMM_ASSY_PU_EQPT_COND wndEqptCond;
        CMM_ASSY_CANCEL_TERM wndCancelTerm;
        CMM_COM_SELF_INSP wndQualityInput;
        CMM_COM_EQPCOMMENT wndEqpComment;
        CMM_SHIFT_USER2 wndShiftUser;
        ASSY003_001_CHANGE_WO wndChgWorkOrder;
        CMM_ASSY_LOTCUT wndLotCut;
        ASSY003_001_WAITLOT_PART_SCRAP wndPartScrap;
        #endregion

        //[E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 
        bool isTWS_LOADING_TRACKING = false;
        bool isSaveTWS_LOADING_TRACKING = false;
        bool isPassSaveTWS_LOADING_TRACKING = true;

        //[E20240715-000063] 전극공정진척 Roll in & Roll out 방향 기본 세팅값을 시스템 상 미리 표기
        bool isAutoSetRollDir = false;
        bool isSilentlySave = false;

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_001()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, null, Process.NOTCHING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");

            String[] sFilter3 = { "ELEC_TYPE", GetBasicPolarity() };
            C1ComboBox[] cboLineChild2 = { cboEquipment };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild2, sFilter: sFilter3, sCase: "POLARITY");

            String[] sFilter2 = { Process.NOTCHING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboElecType };  //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboElecType };
            C1ComboBox[] cboChild = { cboMountPstsID };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbChild: cboChild, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_NT_AUTO_CFG");// sCase: "EQUIPMENT_NT");


            // 투입위치 코드
            String[] sFilter4 = { "PROD" };
            C1ComboBox[] cboMountPstParent = { cboEquipment };
            _combo.SetCombo(cboMountPstsID, CommonCombo.ComboStatus.NONE, cbParent: cboMountPstParent, sFilter: sFilter4, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
        }

        private void InitFaultyDataGrid(bool bClearAll = false)
        {
            if (bClearAll)
            {
                Util.gridClear(dgFaulty);

                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    if (dgFaulty.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                    {
                        dgFaulty.Columns.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    if (dgFaulty.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgFaulty.ItemsSource);
                        if (dt.Columns.Count > i)
                            if (dt.Columns[i].ColumnName.Equals(dgFaulty.Columns[i].Name))
                                dt.Columns.RemoveAt(i);

                        //if(dgFaulty.Columns[i].Header.ToString().StartsWith("DEFECTQTY"))
                        dgFaulty.Columns.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region Event

        #region [Main Window]

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);

            InitCombo();  

            //HideTestMode();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경
            //if (wndConfirm != null)
            //    wndConfirm.BringToFront();

            if (wndEqpEnd != null)
                wndEqpEnd.BringToFront();

            if (wndHist != null)
                wndHist.BringToFront();

            if (wndCommentHist != null)
                wndCommentHist.BringToFront();

            if (wndRunStart != null)
                wndRunStart.BringToFront();

            if (wndWaitLot != null)
                wndWaitLot.BringToFront();

            if (wndQualitySearch != null)
                wndQualitySearch.BringToFront();

            if (wndEqptCond != null)
                wndEqptCond.BringToFront();

            if (wndCancelTerm != null)
                wndCancelTerm.BringToFront();

            if (wndQualityInput != null)
                wndQualityInput.BringToFront();

            if (wndEqpComment != null)
                wndEqpComment.BringToFront();

            if (wndShiftUser != null)
                wndShiftUser.BringToFront();

            if (wndChgWorkOrder != null)
                wndChgWorkOrder.BringToFront();

            if (wndLotCut != null)
                wndLotCut.BringToFront();

            if (wndPartScrap != null)
                wndPartScrap.BringToFront();
            #endregion

                //GetTestMode();

            ApplyPermissions();

            SetWorkOrderWindow();


            //if (chkWait != null && chkWait.IsChecked.HasValue)
            //    chkWait.IsChecked = true;
            //if (chkRun != null && chkRun.IsChecked.HasValue)
            //    chkRun.IsChecked = true;
            //if (chkEqpEnd != null && chkEqpEnd.IsChecked.HasValue)
            //    chkEqpEnd.IsChecked = true;

            chkWait.Checked -= chkWait_Checked;
            chkWait.Unchecked -= chkWait_Unchecked;
            chkRun.Checked -= chkRun_Checked;
            chkRun.Unchecked -= chkRun_Unchecked;
            chkEqpEnd.Checked -= chkEqpEnd_Checked;
            chkEqpEnd.Unchecked -= chkEqpEnd_Unchecked;

            chkWait.Checked += chkWait_Checked;
            chkWait.Unchecked += chkWait_Unchecked;
            chkRun.Checked += chkRun_Checked;
            chkRun.Unchecked += chkRun_Unchecked;
            chkEqpEnd.Checked += chkEqpEnd_Checked;
            chkEqpEnd.Unchecked += chkEqpEnd_Unchecked;

            bLoaded = false;

            #region [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            btnSaveRollDir.Click -= OnClickSaveRollDir;
            btnSaveRollDir.Click += OnClickSaveRollDir;
            cboEquipmentSegment_SelectedValueChanged(cboEquipmentSegment, null);
            #endregion

            #region [E20240715-000063]  전극공정진척 Roll in & Roll out 방향 기본 세팅값을 시스템 상 미리 표기
            rbRollinUp.Checked += rbRollDirectCheck;
            rbRollinDown.Checked += rbRollDirectCheck;
            rbInputRollLaneForward.Checked += rbRollDirectCheck;
            rbInputRollLaneReverse.Checked += rbRollDirectCheck;
            #endregion
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HiddenLoadingIndicator();
                return;
            }

            GetWorkOrder();
            GetProductLot();

            GetEqptWrkInfo();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (iRow < 0)
            {
                if (wndRunStart != null)
                {
                    wndRunStart = null;
                    //return;
                }

                wndRunStart = new ASSY003_001_RUNSTART();
                wndRunStart.FrameOperation = FrameOperation;

                if (wndRunStart != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = cboElecType.SelectedValue.ToString();
                    C1WindowExtension.SetParameters(wndRunStart, Parameters);

                    wndRunStart.Closed += new EventHandler(wndRun_Closed);
                    
                    this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));                                       
                }
            }
            else
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSTAT")).Equals("WAIT") ||
                    (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC") &&
                    Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIP_TYPE_CODE")).Equals("IN")))
                {
                    // 작지 선택 여부 확인
                    if (GetEIOInfo().Equals(""))
                    {
                        //Util.Alert("해당 설비에 선택된 작업지시가 없습니다.");
                        Util.MessageValidation("SFU2018");
                        return;
                    }

                    Util.MessageConfirm("SFU1240", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            string sNewLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "PR_LOTID"));
                            if (sNewLot.Equals("")) return;
                            StartRun(sNewLot);
                        }
                    });
                }
                else
                {
                    if (wndRunStart != null)
                    {
                        wndRunStart = null;
                        //return;
                    }

                    wndRunStart = new ASSY003_001_RUNSTART();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[3];
                        Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[1] = cboEquipment.SelectedValue.ToString();
                        Parameters[2] = cboElecType.SelectedValue.ToString();
                        C1WindowExtension.SetParameters(wndRunStart, Parameters);

                        wndRunStart.Closed += new EventHandler(wndRun_Closed);
                        
                        this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                    }
                }
            }
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCancelRun())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunComplete())
                return;

            if (wndEqpEnd != null)
            {
                wndEqpEnd = null;
                //return;
            }

            wndEqpEnd = new ASSY003_001_EQPEND();
            wndEqpEnd.FrameOperation = FrameOperation;

            if (wndEqpEnd != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODID"));
                Parameters[5] = cboMountPstsID != null && cboMountPstsID.SelectedValue != null ? cboMountPstsID.SelectedValue.ToString() : "";
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPQTY_EA"));
                C1WindowExtension.SetParameters(wndEqpEnd, Parameters);

                wndEqpEnd.Closed += new EventHandler(wndEqpend_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpEnd.ShowModal()));
            }
        }

        /// <summary>
        /// /////////////////////////////////////
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
                    newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
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
                      
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //???
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
                ConfirmProcess_Rate();
                /////////////////////////////////////
                //Rate저장 Biz 호출

            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    //  grid.Children.Remove(pop);
                    break;
                }
            }

        }



        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;
            if (dgDetail != null && dgDetail.Rows.Count > 0)
            {
                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    //생산 제품이 여러개인 경우?
                    if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")).Equals(sLotID))
                    {
                        dInputQty = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                        break;
                    }
                }
            }
            return dInputQty;
        }



        private bool PERMIT_RATE_Input_All()
        {
            bool bFlag = false;
        
            try
            {
                dgFaulty.EndEdit();

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
            
              
                string sLotID = "";
                string sWipSeq = "";
                string sColName = "";
                for (int i = 0; i < dgFaulty.Columns.Count; i++)
                {
                    if (dgFaulty.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                    {
                        sLotID = dgFaulty.Columns[i].Header.ToString().Replace("[#]", "").Trim();
                        sWipSeq = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", sLotID)].DataItem, "WIPSEQ"));
                        sColName = dgFaulty.Columns[i].Name.ToString();
                        PERMIT_RATE_input(sColName, sLotID, sWipSeq, ref  data);
                       // break;
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

        private void PERMIT_RATE_input(string sColName, string sLotID, string sWipSeq, ref DataTable data)
        {
           
            try
            {
                dgFaulty.EndEdit();
                double goodQty =  GetLotINPUTQTY(sLotID);
                decimal dRate = 0;
                decimal dQty = 0;
                decimal dAllowQty = 0;
                decimal OverQty = 0;
                for (int j = 0; j < dgFaulty.Rows.Count - dgFaulty.BottomRows.Count; j++)
                {
                    // double dRate = 0;
                    dRate = Util.NVC_Decimal(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "PERMIT_RATE"));
                    //등록된 Rate가 0보다 큰것인 것만 적용
                    if (dRate > 0)
                    {
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)); //여러개인 경우 어떻게?                                 
                                                                                                               //dAllowQty = Math.Truncate(goodQty * dRate / 100); //버림 
                        dAllowQty = Convert.ToDecimal(goodQty) * dRate / 100; 

                        if (dAllowQty < dQty)
                        {
                            OverQty = dQty - dAllowQty;
                            OverQty = Math.Ceiling(OverQty); //소수점 첫자리 올림

                            DataRow newRow =  data.NewRow();
                            newRow["LOTID"] = sLotID; //필수
                            newRow["WIPSEQ"] = sWipSeq; //필수
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID")); //필수
                            newRow["ACTNAME"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTNAME")); //필수
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "RESNCODE")); //필수
                            newRow["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "RESNNAME")); //필수
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               
            }

        }


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               

                btnConfirm.IsEnabled = false; //2022-08-02

                if (!CanConfirmCommon())
                    return;

                // Loss 입력 여부 체크.
                DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.NOTCHING);

                if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                {
                    string sInfo = string.Empty;
                    string sLossInfo = string.Empty;

                    for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                    {
                        sInfo = Util.NVC(dtEqpLossInfo.Rows[iCnt]["JOBDATE"]) + " : " + Util.NVC(dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"]);
                        sLossInfo = sLossInfo + "\n" + sInfo;
                    }

                    object[] param = new object[] { sLossInfo };

                    // 미입력한 설비 Loss가 존재합니다. 확정하시겠습니까? %1
                    Util.MessageConfirm("SFU3501", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Confirm_Process();
                        }
                    }, param);
                }
                else
                {
                    Confirm_Process();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally //2022-08-02
            {
                btnConfirm.IsEnabled = true;
            }
        }

        private void btnRemarkHist_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (wndCommentHist != null)
            {
                wndCommentHist = null;
                //return;
            }

            wndCommentHist = new ASSY003_001_LOTCOMMENTHIST();
            wndCommentHist.FrameOperation = FrameOperation;

            if (wndCommentHist != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                C1WindowExtension.SetParameters(wndCommentHist, Parameters);

                wndCommentHist.Closed += new EventHandler(wndLotCommentHist_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndCommentHist.ShowModal()));                
            }
        }

        private void btnEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            if (wndEqpComment != null)
            {
                wndEqpComment = null;
                //return;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            wndEqpComment = new CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.NOTCHING;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                Parameters[6] = txtShift.Text; // 작업조명
                Parameters[7] = txtShift.Tag; // 작업조코드
                Parameters[8] = txtWorker.Text; // 작업자명
                Parameters[9] = txtWorker.Tag; // 작업자 ID

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }
        
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrintPopup())
                return;

            if (wndHist != null)
            {
                wndHist = null;
                //return;
            }

            wndHist = new ASSY003_001_HIST();
            wndHist.FrameOperation = FrameOperation;

            if (wndHist != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();

                C1WindowExtension.SetParameters(wndHist, Parameters);

                wndHist.Closed += new EventHandler(wndPrint_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgProductLot);
                    ClearLotInfo();
                    ClearDetailData();
                    //chkEqpEnd.IsChecked = false;


                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkWait == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void chkRun_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgProductLot);
                    ClearLotInfo();
                    ClearDetailData();
                    //chkEqpEnd.IsChecked = false;

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkRun == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void chkEqpEnd_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgProductLot);
                    ClearLotInfo();
                    ClearDetailData();
                    //chkWait.IsChecked = false;
                    //chkRun.IsChecked = false;

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkEqpEnd == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                if (cboEquipment.SelectedIndex == 0)
                {
                    if (winWorkOrder != null)
                    {
                        winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
                        winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
                        winWorkOrder.PROCID = Process.NOTCHING;

                        winWorkOrder.ClearWorkOrderInfo();
                    }
                }

                ClearControls();

                txtWorker.Text = "";
                txtWorker.Tag = "";
                txtShift.Text = "";
                txtShift.Tag = "";
                txtShiftStartTime.Text = "";
                txtShiftEndTime.Text = "";
                txtShiftDateTime.Text = "";
                txtLossCnt.Text = "";
                txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);

                // 설비 선택 시 자동 조회 처리
                if (cboEquipment.SelectedIndex > 0 && cboEquipment.Items.Count > cboEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        ShowLoadingIndicator();

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            btnSearch_Click(null, null);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEqptCondInfo())
                return;

            if (wndEqptCond != null)
            {
                wndEqptCond = null;
                //return;
            }

            wndEqptCond = new CMM_ASSY_PU_EQPT_COND();
            wndEqptCond.FrameOperation = FrameOperation;

            if (wndEqptCond != null)
            {
                DataTable dtTmp = DataTableConverter.Convert(dgProductLot.ItemsSource);

                DataRow[] dtRow = dtTmp.Select("CHK = '1'");

                if (dtRow != null && dtRow.Length > 1)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = cboEquipmentSegment.SelectedValue;
                    Parameters[1] = cboEquipment.SelectedValue;
                    Parameters[2] = Process.NOTCHING;
                    Parameters[3] = Util.NVC(dtRow[0]["LOTID"]);
                    Parameters[4] = Util.NVC(dtRow[0]["WIPSEQ"]);
                    Parameters[5] = cboEquipment.Text;
                    Parameters[6] = Util.NVC(dtRow[1]["LOTID"]);

                    C1WindowExtension.SetParameters(wndEqptCond, Parameters);
                }
                else
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = cboEquipmentSegment.SelectedValue;
                    Parameters[1] = cboEquipment.SelectedValue;
                    Parameters[2] = Process.NOTCHING;
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[5] = cboEquipment.Text;

                    C1WindowExtension.SetParameters(wndEqptCond, Parameters);
                }

                wndEqptCond.Closed += new EventHandler(wndEqptCond_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            }
        }

        private void btnCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            if (wndCancelTerm != null)
            {
                wndCancelTerm = null;
                //return;
            }

            wndCancelTerm = new CMM_ASSY_CANCEL_TERM();
            wndCancelTerm.FrameOperation = FrameOperation;

            if (wndCancelTerm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Process.NOTCHING;

                C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                wndCancelTerm.Closed += new EventHandler(wndCancelTerm_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
            }
        }

        private void cboEqptDfctLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null)
                return;

            GetEqpFaultyData();
        }

        private void cboRollwayLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null)
                return;

            txtLoadinglevel.Text = SetLotIdLoadingLevel();
        }

        private void chkWait_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkWait = false;
            }
        }

        private void chkRun_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkRun = false;
            }
        }

        private void chkEqpEnd_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkEqpEnd = false;
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals("")) return;

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
            }));
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            if (wndWaitLot != null)
            {
                wndWaitLot = null;
                //return;
            }

            wndWaitLot = new ASSY003_001_WAITLOT();

            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWait_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
            }
        }
        
        private void btnQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            if (wndQualitySearch != null)
            {
                wndQualitySearch = null;
                //return;
            }

            wndQualitySearch = new CMM_ASSY_QUALITY_PKG();
            wndQualitySearch.FrameOperation = FrameOperation;

            if (wndQualitySearch != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.NOTCHING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndQualitySearch, Parameters);

                wndQualitySearch.Closed += new EventHandler(wndQualityRslt_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndQualitySearch.ShowModal()));                
            }
        }

        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            CMM_ASSY_QUALITY_INPUT wndQualityInput = new CMM_ASSY_QUALITY_INPUT();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.NOTCHING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                //Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInputAutoMobile_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }

        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCompleteCancelRun())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunCompleteCancel();
                }
            });
        }

        #endregion

        #region [작업대상]

        private void dgProductLot_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                                    _CurrentLotInfo.resetCurrentLotInfo();
                                    ClearDetailData();

                                    if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                        return;

                                    ProdListClickedProcess(e.Cell.Row.Index);

                                    //SetEnabled(dg.Rows[e.Cell.Row.Index].DataItem);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                    #region [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
                                    if (isTWS_LOADING_TRACKING)
                                    {
                                        isSaveTWS_LOADING_TRACKING = false;
                                        GetTWS_LOADING_TRACKING();
                                    }
                                    #endregion
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    _CurrentLotInfo.resetCurrentLotInfo();

                                    ClearDetailData();

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);
                                }
                                break;
                        }

                        if (dgProductLot.CurrentCell != null)
                            dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.CurrentCell.Row.Index, dgProductLot.Columns.Count - 1);
                        else if (dgProductLot.Rows.Count > 0)
                            dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.Rows.Count, dgProductLot.Columns.Count - 1);
                    }
                }
            }));
        }

        #endregion

        #region [실적확인]
        private void btnShift_Click(object sender, RoutedEventArgs e)
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
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.NOTCHING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShift_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));
            }
        }

        private void txtInputQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtInputQty.Text, 0))
                {
                    txtInputQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    SetInputQty();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInputQty_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //SetInputQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                switch (Convert.ToString(e.Cell.Column.Name))
                {
                    case "INPUTQTY":
                        //SetLotInfoCalc();
                        SetParentQty();
                        break;
                }
            }
        }

        #endregion

        #region [Tabs]
        private void btnSaveFaulty_Click(object sender, RoutedEventArgs e)
        {
            if (dgFaulty.ItemsSource == null || dgFaulty.Rows.Count < 1)
            {
                //Util.Alert("불량항목이 없습니다.");
                Util.MessageValidation("SFU1588");
                return;
            }
            //불량정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetFaulty();
                }
            });
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboRemarkLot.SelectedIndex < 0)
            {
                //Util.Alert("LOT정보가 없습니다.");
                Util.MessageValidation("SFU1386");
                return;
            }

            //저장하시겠습니까
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemark();
                }
            });
        }

        private void cboRemarkLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null)
                return;

            GetRemark();
        }

        private void dgFaulty_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (!e.Cell.Column.Name.StartsWith("DEFECTQTY"))
                return;

            double dfct, loss, charge_prd;

            double sum = SumDefectQty(dgFaulty, e.Cell.Column.Name, out dfct, out loss, out charge_prd);

            double totSum = sum;

            if (!_CurrentLotInfo.STATUS.Equals("WAIT"))
            {
                int iRow = -1;
                double inputqty = 0;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    if (e.Cell.Column.Header.ToString().IndexOf(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"))) >= 0)
                    {
                        inputqty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                        iRow = i;
                        break;
                    }
                }

                if (iRow < 0) return;

            
                if (inputqty < totSum)
                {

                    Util.MessageValidation("SFU1608");
                    //Util.Alert("생산량보다 불량이 크게 입력 될 수 없습니다.");

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);

                    sum = SumDefectQty(dgFaulty, e.Cell.Column.Name, out dfct, out loss, out charge_prd);
                    totSum = sum;

                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DFCT_SUM", totSum);
                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_DEFECT_LOT", dfct);
                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_LOSS_LOT", loss);
                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd);

                    SetLotInfoCalc();
                    return;
                }
               

                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DFCT_SUM", totSum);

                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_DEFECT_LOT", dfct);
                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_LOSS_LOT", loss);
                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd);

                SetLotInfoCalc();
            }
        }

        private void btnSearchEqpFaulty_Click(object sender, RoutedEventArgs e)
        {
            if (dgDetail == null || dgDetail.Rows.Count < 1)
                return;

            GetEqpFaultyData();
        }

        private void rbRollDirectCheck(object sender, RoutedEventArgs e)
        {
            if (isTWS_LOADING_TRACKING)
            {
                RadioButton rb = sender as RadioButton;
                string rbName = rb.Name;
                bool isChecked = rb.IsChecked.Value;

                if (isAutoSetRollDir == true)
                {
                    //자동으로 세팅된 값입니다 바꾸시겠습니까 ?
                    Util.MessageConfirm("SFU3901", (result) =>
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            isAutoSetRollDir = false;

                            rb.IsChecked = !rb.IsChecked.Value;
                            if (rbName == "rbRollinUp")
                                rbRollinDown.IsChecked = !rbRollinDown.IsChecked;
                            else if (rbName == "rbRollinDown")
                                rbRollinUp.IsChecked = !rbRollinUp.IsChecked;
                            else if (rbName == "rbInputRollLaneForward")
                                rbInputRollLaneReverse.IsChecked = !rbInputRollLaneReverse.IsChecked;
                            else if (rbName == "rbInputRollLaneReverse")
                                rbInputRollLaneForward.IsChecked = !rbInputRollLaneForward.IsChecked;

                            isAutoSetRollDir = true;

                            return;
                        }
                        else
                        {
                            isAutoSetRollDir = false;
                        }
                    });
                }

            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetProductLot(bool bSelPrv = true)
        {
            try
            {
                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                {
                    HiddenLoadingIndicator();
                    return;
                }

                _CurrentLotInfo.resetCurrentLotInfo();

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                ShowLoadingIndicator();

                // 선입선출 기준일 조회.
                GetProcMtrlInputRule();

                string sInQuery = string.Empty;

                if (chkWait.IsChecked.HasValue && (bool)chkWait.IsChecked)
                    sInQuery = "WAIT";

                if (chkRun.IsChecked.HasValue && (bool)chkRun.IsChecked)
                {
                    if (sInQuery.Equals(""))
                        sInQuery = "PROC";
                    else
                        sInQuery = sInQuery + ",PROC";
                }

                if (chkEqpEnd.IsChecked.HasValue && (bool)chkEqpEnd.IsChecked)
                {
                    if (sInQuery.Equals(""))
                        sInQuery = "EQPT_END";
                    else
                        sInQuery = sInQuery + ",EQPT_END";
                }
                //sInQuery = "PROC,EQPT_END";
                if (bLoaded == true)
                {
                    sInQuery = "WAIT,PROC,EQPT_END";
                }

                DataTable inTable = new DataTable();
                
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["WIPSTAT"] = sInQuery;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_NT_NJ", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            DataRow[] dtRow = searchResult.Select("WIPSTAT = 'WAIT'");
                            if(dtRow.Length > 0)
                                DateTime.TryParse(Util.NVC(dtRow[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }

                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                                SetCheckProdListSameChildSeq(dgProductLot.Rows[idx]);

                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;

                                ProdListClickedProcess(idx);

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                dgProductLot.CurrentCell = dgProductLot.GetCell(idx, dgProductLot.Columns.Count - 1);
                            }
                        }
                        else
                        {
                            // 임시 주석.
                            if (searchResult.Rows.Count > 0)
                            {
                                int iRowRun = _Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)  // 진행중인 경우에만 자동 체크 처리.
                                {

                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    SetCheckProdListSameChildSeq(dgProductLot.Rows[iRowRun]);

                                    ProdListClickedProcess(iRowRun);

                                    // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(iRowRun, dgProductLot.Columns.Count - 1);
                                }
                            }

                            if (dgProductLot.Rows.Count > 0)
                            {
                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                dgProductLot.CurrentCell = dgProductLot.GetCell(0, dgProductLot.Columns.Count - 1);
                            }
                        }

                        // 2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
                        GetLossCnt();
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

        private void GetDetailData()
        {
            try
            {
                ShowLoadingIndicator();

                int iGrpSeq = 0;
                int.TryParse(_CurrentLotInfo.CHILD_GR_SEQNO, out iGrpSeq);

                DataTable inTable = _Biz.GetDA_PRD_SEL_CHILDINFO_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _CurrentLotInfo.PR_LOTID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CHILD_GR_SEQNO"] = iGrpSeq;
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_INFO_BY_PR_NT", "INDATA", "OUTDATA", inTable);

                //dgDetail.ItemsSource = DataTableConverter.Convert(searchResult);
                Util.GridSetData(dgDetail, searchResult, FrameOperation, false);

                SetRemarkCombo();

                SetEqptDfctCombo();
                
                SetRollwayLotCombo();

                #region [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
                if (isTWS_LOADING_TRACKING)
                {
                    isSaveTWS_LOADING_TRACKING = false;
                    GetTWS_LOADING_TRACKING();
                }
                #endregion
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

        private void GetFaultyData()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PROD_SEL_ACTVITYREASON_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["EQPTID"] = cboEquipment.SelectedValue == null ? "" : cboEquipment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROCACTRSN_CODE", "INDATA", "OUTDATA", inTable);

                //dgFaulty.ItemsSource = DataTableConverter.Convert(searchResult);
                Util.GridSetData(dgFaulty, searchResult, FrameOperation, false);

                // Defect Column 생성..
                if (dgDetail.Rows.Count - dgDetail.TopRows.Count > 0)
                {
                    InitFaultyDataGrid();

                    for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                    {
                        string sColName = "DEFECTQTY" + (i + 1).ToString();

                        // 불량 수량 컬럼 위치 변경.
                        int iColIdx = 0;

                        iColIdx = dgFaulty.Columns["COST_CNTR_NAME"].Index;

                        Util.SetGridColumnNumeric(dgFaulty, sColName, null, Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // 부품 항목 앞으로 위치 이동.

                        if (dgFaulty.Columns.Contains(sColName))
                        {
                            (dgFaulty.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                            (dgFaulty.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                            (dgFaulty.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                        }

                        if (dgFaulty.Rows.Count == 0) continue;

                        DataTable dt = GetFaultyDataByLot(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));

                        BindingDataGrid(dgFaulty, dt, sColName);
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

        private DataTable GetFaultyDataByLot(string sLotID, string sWipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = RQSTDT.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipseq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                RQSTDT.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
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

        private void GetEqpFaultyData()
        {
            try
            {
                if (cboEqptDfctLot.SelectedValue == null || cboEqptDfctLot.SelectedValue.ToString().Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                //newRow["LOTID"] = sLot; // Util.NVC(rowview["LOTID"]);
                //newRow["WIPSEQ"] = sWipSeq; // Util.NVC(rowview["WIPSEQ"]);

                newRow["LOTID"] = cboEqptDfctLot.SelectedValue;
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", cboEqptDfctLot.SelectedValue.ToString())].DataItem, "WIPSEQ"));

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

                        //dgEqpFaulty.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, false);
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

        private string SetLotIdLoadingLevel()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COMPLOTID", typeof(string));
                inTable.Columns.Add("INPUTLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COMPLOTID"] = Util.IsNVC(cboRollwayLot.SelectedValue) ? "" : Util.NVC(cboRollwayLot.SelectedValue.ToString());
                newRow["INPUTLOTID"] = Util.IsNVC(cboRollwayLot.SelectedValue) ? "" : getCompLotId();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOADINGLEVEL_INFO", "RQSTDT", "RSLTDT", inTable);

                string sLevelCode = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sLevelCode = Util.NVC(dtResult.Rows[0]["LEVEL_CODE"]);
                }
                else
                {
                    sLevelCode = "";
                }

                return sLevelCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string getCompLotId()
        {
            string compLotid = "";

            if (CommonVerify.HasDataGridRow(dgProductLot))
            {
                DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<Int32>("CHK") == 1
                                 select t).ToList();

                if (queryEdit.Any())
                {
                    foreach (var item in queryEdit)
                    {
                        compLotid = Util.NVC(item["PR_LOTID"]).GetString();

                        if (compLotid.IsNotEmpty())
                            return compLotid;
                    }
                }
            }

            return compLotid;
        }

        private void GetRemark()
        {
            try
            {
                if (cboRemarkLot.SelectedValue == null || cboRemarkLot.SelectedValue.ToString().Equals(""))
                    return;

                ShowLoadingIndicator();

                rtxRemark.Document.Blocks.Clear();

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = cboRemarkLot.SelectedValue;
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", cboRemarkLot.SelectedValue.ToString())].DataItem, "WIPSEQ"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count > 0 && !Util.NVC(searchResult.Rows[0]["WIP_NOTE"]).Equals(""))
                            rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

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

        private void SetRemark()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_UPD_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(cboRemarkLot.SelectedValue);
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", cboRemarkLot.SelectedValue.ToString())].DataItem, "WIPSEQ"));
                newRow["WIP_NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("저장 되었습니다.");
                        Util.MessageInfo("SFU1270");
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetNewLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_NEW_LOT_NT();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID_MO"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_LOTID_NT", "INDATA", "OUTDATA", inTable);

                string sNewLot = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dtResult.Rows[0]["LOTID"]);
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void StartRun(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_START_LOT_NT();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                //newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                newRow = inMtrlTable.NewRow();
                newRow["INPUT_LOTID"] = sNewLot;
                //newRow["MTRLID"] = "";
                newRow["EQPT_MOUNT_PSTN_ID"] = cboMountPstsID.SelectedValue.ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                inMtrlTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_NT_EIF_S", "IN_EQP,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                //int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = _Biz.GetBR_PRD_REG_CNL_START_LOT_NT();

                for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID"));
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_NT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void Confirm()
        {
            try
            {
                // 자동 불량 저장 처리.
                //SaveDefectAllBeforeConfirm(false);

                ShowLoadingIndicator();

                DateTime dtTime;

                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_CONFIRM_LOT_NT();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_VER_CODE"] = "";
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["WIPNOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                //newRow["CUTYN"] = chkCut.IsChecked.HasValue && (bool)chkCut.IsChecked ? "Y" : "N";
                double dRemain = 0;
                if (double.TryParse(txtParent2_M.Text.Trim(), out dRemain))
                {
                    //newRow["CUTYN"] = "Y";
                    newRow["REMAINQTY"] = Convert.ToDecimal(dRemain);
                }
                else
                {
                    //newRow["CUTYN"] = "N";
                    newRow["REMAINQTY"] = 0;
                }

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inPRLot = indataSet.Tables["IN_INPUT"];

                newRow = inPRLot.NewRow();
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
                newRow["EQPT_MOUNT_PSTN_ID"] = cboMountPstsID.SelectedValue.ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                inPRLot.Rows.Add(newRow);
                newRow = null;

                DataTable inLot = indataSet.Tables["IN_LOT"];

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    newRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                    newRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_NT_UI_S", "INDATA,IN_INPUT,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        /////////////////////////
                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                                         
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        // 발행 여부 체크 및 미발행 시 자동 발행 처리
                        AutoPrint();

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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

        private void ConfirmLength_Lack()
        {
            try
            {
                // 자동 불량 저장 처리. 
                //2021 -03-11 최상민 ConfirmLength_Lack() 처리전 SaveLoss 처리를 하여 주석처리
                // SaveDefectAllBeforeConfirm(true);

                ShowLoadingIndicator();

                DateTime dtTime;

                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _Biz.GetBR_PRD_REG_CONFIRM_LOT_NT();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_VER_CODE"] = "";
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["WIPNOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["REMAINQTY"] = 0;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inPRLot = indataSet.Tables["IN_INPUT"];

                newRow = inPRLot.NewRow();
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
                newRow["EQPT_MOUNT_PSTN_ID"] = cboMountPstsID.SelectedValue.ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                inPRLot.Rows.Add(newRow);
                newRow = null;

                DataTable inLot = indataSet.Tables["IN_LOT"];

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    newRow["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                    newRow["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_NT_UI_S", "INDATA,IN_INPUT,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        /////////////////////////
                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }



                        // 발행 여부 체크 및 미발행 시 자동 발행 처리
                        AutoPrint();

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1275");
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

        private void SetFaulty(bool bMsgShow = true)
        {
            try
            {
                ShowLoadingIndicator();
                
                dgFaulty.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgFaulty.Columns.Count; i++)
                {
                    if (dgFaulty.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                    {
                        string sLot = dgFaulty.Columns[i].Header.ToString().Replace("[#]", "").Trim();
                        string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", sLot)].DataItem, "WIPSEQ"));
                        string sColName = dgFaulty.Columns[i].Name.ToString();

                        for (int j = 0; j < dgFaulty.Rows.Count - dgFaulty.BottomRows.Count; j++)
                        {
                            newRow = null;

                            newRow = inDEFECT_LOT.NewRow();
                            newRow["LOTID"] = sLot;
                            newRow["WIPSEQ"] = sWipSeq;
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID"));
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "RESNCODE"));
                            newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)));
                            newRow["RESNCODE_CAUSE"] = "";
                            newRow["PROCID_CAUSE"] = "";
                            newRow["RESNNOTE"] = "";
                            //newRow["DFCT_TAG_QTY"] = 0;
                            newRow["LANE_QTY"] = 1;
                            newRow["LANE_PTN_QTY"] = 1;

                            if (Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "COST_CNTR_ID"));
                            else
                                newRow["COST_CNTR_ID"] = "";

                            newRow["A_TYPE_DFCT_QTY"] = 0;
                            newRow["C_TYPE_DFCT_QTY"] = 0;

                            inDEFECT_LOT.Rows.Add(newRow);
                        }
                    }
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL_NT_NJ", "INDATA,INRESN", null, indataSet);

                if (bMsgShow)
                    Util.MessageInfo("SFU1275");

                GetFaultyData();
                //new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                /* new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL_NT_NJ", "INDATA,INRESN", null, (bizResult, bizException) =>
                 {
                     try
                     {
                         if (bizException != null)
                         {
                             Util.MessageException(bizException);
                             return;
                         }

                         if (bMsgShow)
                             Util.MessageInfo("SFU1275");

                         GetFaultyData();
                     }
                     catch (Exception ex)
                     {
                         Util.MessageException(ex);
                     }
                     finally
                     {
                        // loadingIndicator.Visibility = Visibility.Collapsed;
                     }


                }, indataSet);
              */
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

        private string GetMaxChildGRPSeq(string sPLot)
        {
            if (sPLot.Equals(""))
                return "";

            string sRet = string.Empty;

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MAXCHILDGRSEQ();

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = sPLot;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CHILDGRSEQ", "INDATA", "OUTDATA", inTable);

                if (searchResult != null && searchResult.Rows.Count > 0)
                    sRet = Util.NVC(searchResult.Rows[0]["CHILD_GR_SEQNO"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetChildEqpQty(string sLot, string sChildLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CHILD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["LOTID"] = sLot;
                newRow["CHILD_LOTID"] = sChildLot;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTQTY_NT_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;
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

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals(""))
                    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                else
                    newRow["LBCD"] = "LBL0001"; // LABEL CODE
                newRow["NT_WAIT_YN"] = "N"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                string sBizRule = "BR_PRD_REG_LABEL_PRINT_HIST";

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "NOTCHED PANCAKE";

                // 프린터 셋팅정보
                if (drPrtInfo?.Table != null)
                {
                    newRow["PRT_ITEM04"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT]) : "";          // DEFAULTYN
                    newRow["PRT_ITEM05"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME]) : "";        // PORTNAME
                    newRow["PRT_ITEM06"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME]) : "";  // PRINTERNAME
                    newRow["PRT_ITEM07"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE]) : "";  // PRINTERTYPE
                    newRow["PRT_ITEM08"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI]) : "";                  // DPI
                    newRow["PRT_ITEM09"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_X) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_X]) : "";                      // XPOS
                    newRow["PRT_ITEM10"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_Y) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y]) : "";                      // YPOS
                    newRow["PRT_ITEM11"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS]) : "";        // DARKNESS
                    newRow["PRT_ITEM12"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]) : "";      // EQUIPMENT
                    newRow["PRT_ITEM13"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY]) : "";    // PRINTERKEY
                    newRow["PRT_ITEM14"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE]) : "";        // ISACTIVE
                }                
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;

                inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                new ClientProxy().ExecuteService(sBizRule, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void AutoPrint()
        {
            try
            {
                ShowLoadingIndicator();

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    DataTable inTable = _Biz.GetDA_PRD_SEL_PRINT_YN();

                    DataRow newRow = inTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));

                    inTable.Rows.Add(newRow);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_PRT_CHK", "INDATA", "OUTDATA", inTable);

                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {
                        if (!Util.NVC(dtRslt.Rows[0]["PROC_LABEL_PRT_FLAG"]).Equals("Y"))
                        {
                            // 프린터 정보 조회
                            string sPrt = string.Empty;
                            string sRes = string.Empty;
                            string sCopy = string.Empty;
                            string sXpos = string.Empty;
                            string sYpos = string.Empty;
                            string sDark = string.Empty;
                            string sLBCD = string.Empty;    // 리턴 라벨 타입 코드
                            string sEqpt = string.Empty;
                            DataRow drPrtInfo = null;

                            // 2017-07-04 Lee. D. R
                            // Line별 라벨 독립 발행 기능
                            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                                return;
                            }
                            else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                            {
                                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                    return;
                            }
                            else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                            {
                                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                                {
                                    if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(cboEquipment.SelectedValue.ToString()))
                                    {
                                        sPrt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                        sRes = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                        sCopy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                        sXpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                        sYpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                        sDark = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                        sEqpt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                        drPrtInfo = dr;
                                    }
                                }

                                if (sEqpt.Equals(""))
                                {
                                    Util.MessageValidation("SFU3615"); //프린터 환경설정에 설비 정보를 확인하세요.
                                    return;
                                }
                            }

                            string sZPL = GetPrintInfo(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);

                            if (sZPL.Equals(""))
                            {
                                Util.MessageValidation("SFU1498");
                                return;
                            }

                            if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                            {
                                if (PrintLabel(sZPL.Substring(2), drPrtInfo))
                                    SetLabelPrtHist(sZPL.Substring(2), drPrtInfo, Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")), sLBCD);
                            }
                            else
                            {
                                Util.Alert(sZPL.Substring(2));
                            }
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

        private void GetMinChildGrSeq(out string sLot, out int iMinChildSeq)
        {
            try
            {
                sLot = "";
                iMinChildSeq = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MIN_CHILD_GR_SEQ_NT();

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.NOTCHING;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIN_CHILD_GR_SEQ_BY_PROD_LOT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    iMinChildSeq = Util.NVC(dtRslt.Rows[0]["MIN_CHILD_GR_SEQNO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["MIN_CHILD_GR_SEQNO"]));
                }
            }
            catch (Exception ex)
            {
                sLot = "";
                iMinChildSeq = 0;
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetMaxChildGrSeq(out string sLot, out int iMaxChildSeq)
        {
            try
            {
                sLot = "";
                iMaxChildSeq = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MIN_CHILD_GR_SEQ_NT();

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.NOTCHING;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CHILD_GR_SEQ_BY_PROD_LOT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    iMaxChildSeq = Util.NVC(dtRslt.Rows[0]["MAX_CHILD_GR_SEQNO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["MAX_CHILD_GR_SEQNO"]));
                }
            }
            catch (Exception ex)
            {
                sLot = "";
                iMaxChildSeq = 0;
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetEIOInfo()
        {
            try
            {
                string sWoDetlID = "";
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_PLAN_DETAIL_BYEQPTID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORER_PLAN_DETAIL_BYEQPTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sWoDetlID = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);
                }

                return sWoDetlID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SaveLoss(double dLoss, string sType, bool bMsgShow, bool bSearch = true)
        {
            try
            {
                int iChild = 1;
                if (dgDetail != null)
                    iChild = (dgDetail.Rows.Count - dgDetail.TopRows.Count - dgDetail.BottomRows.Count) < 1 ? 1 : (dgDetail.Rows.Count - dgDetail.TopRows.Count - dgDetail.BottomRows.Count);

                if (dLoss > 0) dLoss = Math.Truncate(dLoss / iChild);
                if (dgFaulty != null)
                {
                    // 길이초과 : PL03L07
                    // 길이부족 : PL02L07
                    for (int i = 0; i < dgFaulty.Columns.Count; i++)
                    {
                        if (dgFaulty.Columns[i].Name.StartsWith("DEFECTQTY"))
                        {
                            if (sType.Equals("PLUS"))
                            {
                                int idx = _Util.GetDataGridRowIndex(dgFaulty, "PRCS_ITEM_CODE", "LENGTH_EXCEED");

                                if (idx >= 0)
                                    DataTableConverter.SetValue(dgFaulty.Rows[idx].DataItem, dgFaulty.Columns[i].Name, dLoss);


                                // 길이부족 0 처리.
                                idx = _Util.GetDataGridRowIndex(dgFaulty, "PRCS_ITEM_CODE", "LENGTH_LACK");
                                if (idx >= 0)
                                    DataTableConverter.SetValue(dgFaulty.Rows[idx].DataItem, dgFaulty.Columns[i].Name, 0);
                            }
                            else if (sType.Equals("MINUS"))
                            {
                                int idx = _Util.GetDataGridRowIndex(dgFaulty, "PRCS_ITEM_CODE", "LENGTH_LACK");

                                if (idx >= 0)
                                    DataTableConverter.SetValue(dgFaulty.Rows[idx].DataItem, dgFaulty.Columns[i].Name, dLoss);


                                // 길이초과 0 처리.
                                idx = _Util.GetDataGridRowIndex(dgFaulty, "PRCS_ITEM_CODE", "LENGTH_EXCEED");
                                if (idx >= 0)
                                    DataTableConverter.SetValue(dgFaulty.Rows[idx].DataItem, dgFaulty.Columns[i].Name, 0);
                            }
                        }
                    }

                    SetFaulty(bMsgShow);

                    // 길이부족 확정 시 재조회 하면 입력 수량이 없어지는 문제 발생.
                    if (bSearch)
                        GetDetailData();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefectAllBeforeConfirm(bool bLossSaved)
        {
            try
            {
                SetFaulty(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckSelWOInfo()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SET_WORKORDER_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals(""))
                    {
                        bRet = false;
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N"))
                    {
                        bRet = false;
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                    }
                    else
                    {
                        bRet = true;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CheckModelChange()
        {
            bool bRet = true;

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LAST_EQP_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("PRJT_NAME"))
                    {
                        string productProjectName = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRJT_NAME"));
                        string serchProjectName = Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]);
                        if (serchProjectName.Length > 1 && serchProjectName.Equals(productProjectName))
                            bRet = false;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CheckInputEqptCond()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.NOTCHING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool SetTestMode(bool bOn, bool bShutdownMode = false)
        {
            try
            {
                string sBizName = string.Empty;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_MODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bShutdownMode)
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE_LOSS";

                    newRow["IFMODE"] = "ON";
                    newRow["UI_LOSS_MODE"] = bOn ? "ON" : "OFF";
                    newRow["UI_LOSS_CODE"] = bOn ? Util.ConvertEqptLossLevel2Change("LC003") : ""; // 계획정지 loss 코드.
                }
                else
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE";

                    newRow["IFMODE"] = bOn ? "TEST" : "ON";
                }

                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(sBizName, "IN_EQP", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedValue == null) return;
                if (Util.NVC(cboEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO_S", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN"))
                {
                    sTestModeType = Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        //HideTestMode();

                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            ShowScheduledShutdown();
                        }
                        else
                        {
                            HideScheduledShutdown();
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
                //HiddenLoadingIndicator();
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void RunCompleteCancel()
        {
            try
            {
                ShowLoadingIndicator();

                //int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID"));
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_EQPT_END_LOT_NT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool GetExcptTime(out double dHour)
        {
            dHour = 0;

            try
            {                
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_WORK_EXCP_TIME", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("EXCP_TIME"))
                {
                    if (!double.TryParse(Util.NVC(dtRslt.Rows[0]["EXCP_TIME"]), out dHour))
                        dHour = 0;

                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private string GetBasicPolarity()
        {
            try
            {
                string sRet = "";

                if (Util.NVC(LoginInfo.CFG_EQPT_ID).Equals("") || Util.NVC(LoginInfo.CFG_PROC_ID) != Process.NOTCHING)
                {
                    return sRet;
                }

                DataTable inTable = new DataTable();

                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = Util.NVC(LoginInfo.CFG_EQPT_ID);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VWEQUIPMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("ELTR_TYPE_CODE"))
                    sRet = Util.NVC(dtRslt.Rows[0]["ELTR_TYPE_CODE"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

        #region [Validation]

        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            if (bLoaded == false)
            {
                if (!(bool)chkRun.IsChecked && !(bool)chkEqpEnd.IsChecked && !(bool)chkWait.IsChecked)
                {
                    //Util.Alert("LOT 상태 선택 조건을 하나 이상 선택하세요.");
                    Util.MessageValidation("SFU1370");
                    return bRet;
                }
            }                

            if (cboElecType.SelectedIndex < 0)
            {
                //Util.Alert("극성을 선택 하세요.");
                Util.MessageValidation("SFU1467");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanStartRun()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            // 작업대상에서 직접 대기중 선택 후 착공 하는 경우 validation.
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx >= 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
                {
                    if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
                    {
                        //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                        Util.MessageValidation("SFU1543");
                        return bRet;
                    }
                }
            }

            // 요청번호 CR-404 에 의한 주석 처리
            //for (int i = 0; i < dgProductLot.Rows.Count; i++)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
            //    {
            //        //Util.Alert("작업중인 LOT 이 존재 합니다.");
            //        Util.MessageValidation("SFU1847");
            //        return bRet;
            //    }
            //}

            if (!CheckSelWOInfo())
            {
                //Util.Alert("선택된 W/O가 없습니다.");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanCancelRun()
        {
            bool bRet = false;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 진행 중 여부 체크
            for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    //Util.Alert("작업중인 LOT이 아닙니다.");
                    Util.MessageValidation("SFU1846");
                    return bRet;
                }
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            // 최종 작업 lot 부터 취소 가능.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > iCut)
                        {
                            //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // Max CUT DB 확인.
            string sTmp = "";
            int iTmpMinSeq = 0;

            sTmp = GetMaxChildGRPSeq(sPRLot);

            int.TryParse(sTmp, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq > iCut)
            {
                //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.");
                Util.MessageValidation("SFU1790");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanRunComplete()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            string sWipStat = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

            if (!sWipStat.Equals("PROC"))
            {
                Util.MessageValidation("SFU1866");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanConfirm()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 장비완료 process 추가로 수정.
            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("실적확인할 수 있는 상태가 아닙니다.");
                Util.MessageValidation("SFU1714");
                return bRet;
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) < iCut)
                        {
                            //Util.Alert("이전 CUT이 실적확정 되지 않았습니다. 먼저 실적확정 하세요.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1786", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // 이전 CUT DB 확인.
            string sTmpLot = "";
            int iTmpMinSeq = 0;

            GetMinChildGrSeq(out sTmpLot, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq < iCut)
            {
                //Util.Alert("이전 CUT이 실적확정 되지 않았습니다. 먼저 실적확정 하세요.\n( LOT : {0} )", sTmpLot);
                Util.MessageValidation("SFU1786", sTmpLot);
                return bRet;
            }


            if (txtParent2.Text.Trim().Equals(""))
                return bRet;

            if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
            {
                //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                Util.MessageValidation("SFU1543");
                return bRet;
            }

            if (dgDetail != null && dgDetail.Rows.Count > 0)
            {
                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")).Equals("0"))
                    {
                        //Util.Alert("양품수량이 0 이므로 확정할 수 없습니다.");
                        Util.MessageValidation("SFU1724");
                        return bRet;
                    }
                }
            }

            if (dgFaulty.ItemsSource != null)
            {
                foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgFaulty))
                {
                    //Util.Alert("저장하지 않은 불량 정보가 있습니다.");
                    Util.MessageValidation("SFU1878");
                    return bRet;
                }
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.Alert("근무조를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("근무조"));
                return bRet;
            }

            if (txtWorker.Text.Trim().Equals(""))
            {
                //Util.Alert("근무자를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("근무자"));
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanConfirmLength_Lack()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 장비완료 process 추가로 수정.
            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("실적확인할 수 있는 상태가 아닙니다.");
                Util.MessageValidation("SFU1714");
                return bRet;
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) < iCut)
                        {
                            //Util.Alert("이전 CUT이 실적확정 되지 않았습니다. 먼저 실적확정 하세요.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1786", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // 이전 CUT DB 확인.
            string sTmpLot = "";
            int iTmpMinSeq = 0;

            GetMinChildGrSeq(out sTmpLot, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq < iCut)
            {
                //Util.Alert("이전 CUT이 실적확정 되지 않았습니다. 먼저 실적확정 하세요.\n( LOT : {0} )", sTmpLot);
                Util.MessageValidation("SFU1786", sTmpLot);
                return bRet;
            }

            if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
            {
                //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                Util.MessageValidation("SFU1543");
                return bRet;
            }

            if (dgDetail != null && dgDetail.Rows.Count > 0)
            {
                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")).Equals("0"))
                    {
                        //Util.Alert("양품수량이 0 이므로 확정할 수 없습니다.");
                        Util.MessageValidation("SFU1724");
                        return bRet;
                    }
                }
            }

            if (dgFaulty.ItemsSource != null)
            {
                foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgFaulty))
                {
                    //Util.Alert("저장하지 않은 불량 정보가 있습니다.");
                    Util.MessageValidation("SFU1878");
                    return bRet;
                }
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.Alert("근무조를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("근무조"));
                return bRet;
            }

            if (txtWorker.Text.Trim().Equals(""))
            {
                //Util.Alert("근무자를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("근무자"));
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanConfirmCommon()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 장비완료 process 추가로 수정.
            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("실적확인할 수 있는 상태가 아닙니다.");
                Util.MessageValidation("SFU1714");
                return bRet;
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) < iCut)
                        {
                            //Util.Alert("이전 CUT이 실적확정 되지 않았습니다. 먼저 실적확정 하세요.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1786", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // 이전 CUT DB 확인.
            string sTmpLot = "";
            int iTmpMinSeq = 0;

            GetMinChildGrSeq(out sTmpLot, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq < iCut)
            {
                //Util.Alert("이전 CUT이 실적확정 되지 않았습니다. 먼저 실적확정 하세요.\n( LOT : {0} )", sTmpLot);
                Util.MessageValidation("SFU1786", sTmpLot);
                return bRet;
            }


            if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
            {
                //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                Util.MessageValidation("SFU1543");
                return bRet;
            }

            if (dgDetail != null && dgDetail.Rows.Count > 0)
            {
                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "GOODQTY")).Equals("0"))
                    {
                        //Util.Alert("양품수량이 0 이므로 확정할 수 없습니다.");
                        Util.MessageValidation("SFU1724");
                        return bRet;
                    }
                }
            }

            if (dgFaulty.ItemsSource != null)
            {
                foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgFaulty))
                {
                    //Util.Alert("저장하지 않은 불량 정보가 있습니다.");
                    Util.MessageValidation("SFU1878");
                    return bRet;
                }
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.Alert("근무조를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업조"));
                return bRet;
            }

            if (txtWorker.Text.Trim().Equals(""))
            {
                //Util.Alert("근무자를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                return bRet;
            }

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            {
                DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();

                // 공통코드에 등록된 시간 내의 경우에는 초기화 하지 않도록..
                double dHour = 0;
                if (GetExcptTime(out dHour))
                {
                    if (dHour > 0)
                    {
                        shiftStartDateTime = shiftStartDateTime.AddHours(-dHour);
                        shiftEndDateTime = shiftEndDateTime.AddHours(-dHour);
                    }
                }

                int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                if (prevCheck < 0 || nextCheck > 0)
                {
                    Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    return bRet;
                }
            }


            // 다모델 인 경우 필수 W/O 변경 여부 체크.
            if (_MUST_CHG_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU4473");
                return bRet;
            }

            // [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
            if (isTWS_LOADING_TRACKING)
            {
                #region [E20240715-000063] 전극공정진척 Roll in & Roll out 방향 기본 세팅값을 시스템 상 미리 표기
                //[PREACTION] Roll 방향 정보 자동 저장
                if (isAutoSetRollDir == true
                    && isSaveTWS_LOADING_TRACKING == false && isPassSaveTWS_LOADING_TRACKING == false)
                {
                    if ((rbRollinUp.IsChecked == true || rbRollinDown.IsChecked == true)
                        && (rbInputRollLaneReverse.IsChecked == true || rbInputRollLaneForward.IsChecked == true))
                    {
                        isSilentlySave = true;
                        SetTWS_LOADING_TRACKING();
                        isSaveTWS_LOADING_TRACKING = true;
                    }
                }
                #endregion

                if (isSaveTWS_LOADING_TRACKING == false && isPassSaveTWS_LOADING_TRACKING == false)
                {
                    //Util.MessageValidation("SFU0000");  //Roll 방향 정보를 저장하세요.
                    Util.MessageValidation("SFU8116", ObjectDic.Instance.GetObjectName("ROLL방향"));
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanPrintPopup()
        {
            bool bRet = false;
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("프린트 환경 설정값이 없습니다.");
                Util.MessageValidation("SFU2003");
                return bRet;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanPrint()
        {
            bool bRet = false;

            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("프린트 환경 설정값이 없습니다.");
                Util.MessageValidation("SFU2003");
                return bRet;
            }

            if (dgDetail.Rows.Count - dgDetail.TopRows.Count < 1)
            {
                //Util.Alert("인쇄 할 LOT이 없습니다.");
                Util.MessageValidation("SFU1792");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            //for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
            //{
            //    DataTable dtDefect = GetFaultyDataByLot(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));

            //    DataRow[] fndRows = dtDefect.Select("RESNQTY > 0");

            //    if(fndRows == null || fndRows.Length < 1)
            //    {
            //        Util.Alert("최소 하나 이상에 불량정보를 등록해야 합니다.");
            //        return bRet;
            //    }
            //}

            bRet = true;
            return bRet;
        }

        private bool CanEqptCondInfo()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIP_TYPE_CODE")).Equals("OUT"))
            {
                //Util.Alert("{0} LOT은 공정조건을 입력할 수 있는 상태가 아닙니다.", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID")));
                Util.MessageValidation("SFU3086", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID")));
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        
        private bool CanCompleteCancelRun()
        {
            bool bRet = false;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 장비완료 여부 체크
            for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("EQPT_END"))
                {
                    Util.MessageValidation("SFU1864");  // 장비완료 상태의 LOT이 아닙니다.
                    return bRet;
                }
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            // 최종 작업 lot 부터 취소 가능.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > iCut)
                        {
                            //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // Max CUT DB 확인.
            string sTmp = "";
            int iTmpMinSeq = 0;

            sTmp = GetMaxChildGRPSeq(sPRLot);

            int.TryParse(sTmp, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq > iCut)
            {
                //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.");
                Util.MessageValidation("SFU1790");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [PopUp Event]

        private void wndEqpend_Closed(object sender, EventArgs e)
        {
            wndEqpEnd = null;

            ASSY003_001_EQPEND window = sender as ASSY003_001_EQPEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            wndEqpComment = null;
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndLotCommentHist_Closed(object sender, EventArgs e)
        {
            wndCommentHist = null;

            ASSY003_001_LOTCOMMENTHIST window = sender as ASSY003_001_LOTCOMMENTHIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        
        private void wndRun_Closed(object sender, EventArgs e)
        {
            wndRunStart = null;

            ASSY003_001_RUNSTART window = sender as ASSY003_001_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            wndShiftUser = null;

            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                //txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                //txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                //txtWorker.Tag = Util.NVC(wndPopup.USERID);
                //txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                //txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                //txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);

                GetEqptWrkInfo();
            }
        }

        private void wndPrint_Closed(object sender, EventArgs e)
        {
            wndHist = null;
            ASSY003_001_HIST window = sender as ASSY003_001_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            wndEqptCond = null;
            CMM_ASSY_PU_EQPT_COND window = sender as CMM_ASSY_PU_EQPT_COND;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndCancelTerm_Closed(object sender, EventArgs e)
        {
            wndCancelTerm = null;

            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndWait_Closed(object sender, EventArgs e)
        {
            wndWaitLot = null;

            ASSY003_001_WAITLOT window = sender as ASSY003_001_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            //  wndConfirm = null;

            // ASSY003_001_CONFIRM window = sender as ASSY003_001_CONFIRM;    
            int iTimes = 0;
            ASSY003_001_CONFIRM window = sender as ASSY003_001_CONFIRM;

            try
            {
                if (window == null) return;

                iTimes++;

                if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                //if (wndConfirm.DialogResult == MessageBoxResult.OK && bWndConfirmJobEnd == true)
                if (window.DialogResult == MessageBoxResult.OK && bWndConfirmJobEnd == true)
                {
                    bWndConfirmJobEnd = false;

                    if (window.SELECT_TYPE.Equals("W"))
                    {
                        // 자동 불량 저장 처리.
                        //SaveDefectAllBeforeConfirm(false);
                        //Confirm();
                        
                        try
                        {
                            // 자동 불량 저장 처리.
                            
                            if (window == null) return;
                            window = null;
                            SaveDefectAllBeforeConfirm(false);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            Thread.Sleep(2000);
                            Confirm();
                            Thread.Sleep(2000);
                            //  this.grdMain.Children.Remove(wndConfirm);
                        }

                    }
                    else if (window.SELECT_TYPE.Equals("L"))
                    {
                        double dRemain = 0;
                        double.TryParse(window.REMAIN_EA, out dRemain);
                        try
                        {
                           
                            if (window == null) return;
                            window = null;
                            SaveLoss(dRemain, "MINUS", false, false);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            Thread.Sleep(2000);
                            ConfirmLength_Lack();
                            Thread.Sleep(2000);
                            // this.grdMain.Children.Remove(wndConfirm);
                        }
                    }
                }
                

                /*
                if (wndConfirm.DialogResult == MessageBoxResult.OK && bWndConfirmJobEnd == true)
                {
                    bWndConfirmJobEnd = false;

                    if (wndConfirm.SELECT_TYPE.Equals("W"))
                    {
                        // 자동 불량 저장 처리.
                        SaveDefectAllBeforeConfirm(false);
                        Confirm();
                    }
                    else if (wndConfirm.SELECT_TYPE.Equals("L"))
                    {
                        double dRemain = 0;
                        double.TryParse(wndConfirm.REMAIN_EA, out dRemain);

                        SaveLoss(dRemain, "MINUS", false, false);

                        ConfirmLength_Lack();
                    }
                }
                */
                //this.grdMain.Children.Remove(window);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                bWndConfirmJobEnd = true;
                this.grdMain.Children.Remove(window);
                //this.grdMain.Children.Remove(wndConfirm);                
            }
        }

        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            wndQualitySearch = null;

            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndQualityInput_Closed(object sender, EventArgs e)
        {
            wndQualityInput = null;
            CMM_COM_QUALITY window = sender as CMM_COM_QUALITY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndQualityInputAutoMobile_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_INPUT window = sender as CMM_ASSY_QUALITY_INPUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndQualityInputAutoMobile_New_Closed(object sender, EventArgs e)
        {
            wndQualityInput = null;
            CMM_COM_SELF_INSP window = sender as CMM_COM_SELF_INSP;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        

        private void wndLotCut_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_LOTCUT window = sender as CMM_ASSY_LOTCUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        
        private void wndPartScrap_Closed(object sender, EventArgs e)
        {
            ASSY003_001_WAITLOT_PART_SCRAP window = sender as ASSY003_001_WAITLOT_PART_SCRAP;
            if (window.DialogResult == MessageBoxResult.OK)
            {
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRunStart);
            listAuth.Add(btnRunCancel);
            listAuth.Add(btnConfirm);

            listAuth.Add(btnSaveFaulty);
            listAuth.Add(btnSaveRemark);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }

        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.NOTCHING;

            winWorkOrder.GetWorkOrder();
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.NOTCHING;
                sEqsg = cboEquipmentSegment.SelectedIndex >= 0 ? cboEquipmentSegment.SelectedValue.ToString() : "";
                sEqpt = cboEquipment.SelectedIndex >= 0 ? cboEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }

        }

        private DataRow GetSelectWorkOrderInfo()
        {
            if (winWorkOrder == null)
                return null;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.PACKAGING;

            return winWorkOrder.GetSelectWorkOrderRow();
        }

        /// <summary>
        /// 작업지시 공통 UserControl에서 작업지시 선택/선택취소 시 메인 화면 정보 재조회 처리
        /// 설비 선택하면 자동 조회 처리 기능 추가로 작지 변경시에도 자동 조회 되도록 구현.
        /// </summary>
        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        /// <summary>
        /// Main Windows Data Clear 처리
        /// UC_WORKORDER 컨트롤에서 Main Window Data Clear
        /// </summary>
        /// <returns>Clear 완료 여부</returns>
        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailData();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            string sInputLot = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "CHILD_GR_SEQNO"));
            string sChildLot = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "LOTID"));

            // 모두 Uncheck 처리 및 동일 자랏의 경우는 Check 처리.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (dataitem.Index != i)
                {
                    if (sInputLot == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")) &&
                        sChildSeq == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")) &&
                        !Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")).Equals(""))
                    {
                        if (sChildLot.Equals(""))
                        {
                            if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                            {
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                            }
                            DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                        }
                        else
                        {
                            if (bUncheckAll)
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                {
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                }
                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                {
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                                }
                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", true);
                            }
                        }

                    }
                    else
                    {
                        if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                            dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                        {
                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                        }
                        DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                    }
                }
            }

            return true;
        }

        private void ProcessDetail(object obj)
        {
            DataRowView rowview = (obj as DataRowView);

            if (rowview == null)
            {
                //Util.Alert("LOT 상세정보가 잘못 되어 있습니다.");
                Util.MessageValidation("SFU1215");
                return;
            }

            _CurrentLotInfo.LOTID = Util.NVC(rowview["LOTID"]);
            _CurrentLotInfo.PR_LOTID = Util.NVC(rowview["PR_LOTID"]);
            _CurrentLotInfo.WIPSEQ = Util.NVC(rowview["WIPSEQ"]);
            _CurrentLotInfo.INPUTQTY = Util.NVC(rowview["WIPQTY"]);
            _CurrentLotInfo.WORKORDER = Util.NVC(rowview["WOID"]); // 장비완료 process 추가로 수정.
            _CurrentLotInfo.STATUS = Util.NVC(rowview["WIPSTAT"]);
            _CurrentLotInfo.STATUSNAME = Util.NVC(rowview["WIPSNAME"]);
            _CurrentLotInfo.WORKDATE = Util.NVC(rowview["CALDATE_LOT"]);
            _CurrentLotInfo.STARTTIME_CHAR = Util.NVC(rowview["WIPDTTM_ST"]);
            //_CurrentLotInfo.REMARK = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, ""));
            _CurrentLotInfo.PRODID = Util.NVC(rowview["PRODID"]);
            _CurrentLotInfo.VERSION = Util.NVC(rowview["PROD_VER_CODE"]);
            _CurrentLotInfo.ENDTIME_CHAR = Util.NVC(rowview["EQPT_END_DTTM"]);
            _CurrentLotInfo.CHILD_GR_SEQNO = Util.NVC(rowview["CHILD_GR_SEQNO"]);
            _CurrentLotInfo.WIP_TYPE_CODE = Util.NVC(rowview["WIP_TYPE_CODE"]);

            _CurrentLotInfo.STARTTIME_CHAR_ORG = Util.NVC(rowview["WIPDTTM_ST_ORG"]);
            _CurrentLotInfo.ENDTIME_CHAR_ORG = Util.NVC(rowview["EQPT_END_DTTM_ORG"]);

            _CurrentLotInfo.PR_PRODID = Util.NVC(rowview["MO_PRODID"]);


            // Caldate Lot의 Caldate로...
            if (Util.NVC(rowview["CALDATE_LOT"]).Trim().Equals(""))
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"]));

                sCaldate = Util.NVC(rowview["NOW_CALDATE_YMD"]);
                dtCaldate = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"]));
            }
            else
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"]));

                sCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"])).ToString("yyyyMMdd");
                dtCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"]));
            }


            // 생산량 입력 Hidden
            tbInputQty.Visibility = Visibility.Hidden;
            txtInputQty.Visibility = Visibility.Hidden;

            string sLot = "";
            if (!_CurrentLotInfo.STATUS.Equals("WAIT"))
            {
                if (_CurrentLotInfo.STATUS.Equals("EQPT_END"))
                {
                    FillLotInfo();
                    sLot = Util.NVC(rowview["LOTID"]);

                    // 생산량 입력 Visible
                    tbInputQty.Visibility = Visibility.Visible;
                    txtInputQty.Visibility = Visibility.Visible;
                }
                else if (_CurrentLotInfo.STATUS.Equals("PROC") && _CurrentLotInfo.WIP_TYPE_CODE.Equals("OUT"))
                {
                    //FillLotInfo(); // 장비완료 process 추가로 수정.
                    sLot = Util.NVC(rowview["LOTID"]);
                }
                else
                {
                    sLot = Util.NVC(rowview["PR_LOTID"]);
                }

                GetDetailData();

                GetFaultyData();

                SetLotInfoCalc();

                if (_CurrentLotInfo.STATUS.Equals("EQPT_END"))// || (_CurrentLotInfo.STATUS.Equals("PROC") && _CurrentLotInfo.WIP_TYPE_CODE.Equals("OUT"))) // 장비완료 process 추가로 수정.
                {
                    SetParentQty();

                    CheckMultiProduction();
                }
            }
        }

        private void FillLotInfo()
        {
            /******************** 상세 정보 Set... ******************/
            txtWorkorder.Text = _CurrentLotInfo.WORKORDER;
            txtLotStatus.Text = _CurrentLotInfo.STATUSNAME;
            txtStartTime.Text = _CurrentLotInfo.STARTTIME_CHAR;
            txtEndTime.Text = _CurrentLotInfo.ENDTIME_CHAR;

            if (!_CurrentLotInfo.STARTTIME_CHAR_ORG.Equals("") && !_CurrentLotInfo.ENDTIME_CHAR_ORG.Equals(""))
            {
                DateTime dTmpEnd;
                DateTime dTmpStart;

                if (DateTime.TryParse(_CurrentLotInfo.ENDTIME_CHAR_ORG, out dTmpEnd) && DateTime.TryParse(_CurrentLotInfo.STARTTIME_CHAR_ORG, out dTmpStart))
                    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString();
            }
        }

        private void ClearLotInfo()
        {
            sCaldate = "";

            txtParentQty.Text = "";
            txtParent2.Text = "";

            txtParentQty_M.Text = "";
            txtParent2_M.Text = "";

            txtWorkorder.Text = "";
            txtLotStatus.Text = "";
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            txtWorkMinute.Text = "";

            if (btnChangeWO != null) btnChangeWO.Visibility = Visibility.Collapsed;
            if (dgDetail != null && dgDetail.Columns.Contains("CHK")) dgDetail.Columns["CHK"].Visibility = Visibility.Collapsed;
            if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].IsReadOnly = true;
            if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].EditOnSelection = false;

            _MUST_CHG_FLAG = "";

            #region [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 
            rbInputRollLaneForward.IsChecked = false;
            rbInputRollLaneReverse.IsChecked = false;
            rbRollinUp.IsChecked = false;
            rbRollinDown.IsChecked = false;
            isAutoSetRollDir = false;
            isSilentlySave = false;
            #endregion
        }

        private void SetLotInfoCalc()
        {
            if (dgDetail.ItemsSource == null)
                return;

            double inputQty = 0;
            double goodQty = 0;
            double lossQty = 0;

            for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
            {
                inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));
                goodQty = inputQty - lossQty;

                DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", goodQty);
            }
        }

        private double SumDefectQty(C1.WPF.DataGrid.C1DataGrid dg, string sColName, out double dfct, out double loss, out double charge_prd)
        {
            double sum = 0;
            dfct = 0;
            loss = 0;
            charge_prd = 0;

            if (!dg.Columns.Contains(sColName))
                return sum;

            for (int i = 0; i < dg.Rows.Count - dg.Rows.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")).Equals("N"))  // 실적 제외 여부 확인.
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("DEFECT_LOT"))
                        dfct += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("LOSS_LOT"))
                        loss += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                        charge_prd += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());

                    sum += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                }
            }
            return sum;
        }

        private void BindingDataGrid(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dtRslt, string sColName)
        {
            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);

            if (!dt.Columns.Contains(sColName))
            {
                dt.Columns.Add(sColName, typeof(int));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][sColName] = 0;
                }
            }

            if (dtRslt != null)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                    {
                        if (dtRslt.Rows[j]["ACTID"].Equals(dt.Rows[k]["ACTID"]) &&
                            dtRslt.Rows[j]["RESNCODE"].Equals(dt.Rows[k]["RESNCODE"]))
                        {
                            dt.Rows[k][sColName] = dtRslt.Rows[j]["RESNQTY"];
                            //NER 추가
                            dt.Rows[k]["PERMIT_RATE"] = dtRslt.Rows[j]["PERMIT_RATE"];

                            if (dt.Columns.Contains("RESNNOTE") && dtRslt.Columns.Contains("RESNNOTE"))
                            {
                                dt.Rows[k]["RESNNOTE"] = dtRslt.Rows[j]["RESNNOTE"];
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][sColName] = 0;
                }
            }

            dataGrid.BeginEdit();
            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            Util.GridSetData(dataGrid, dt, FrameOperation, false);

            C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dataGrid.Columns[sColName]
                , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

            dataGrid.EndEdit();

        }

        private void ClearDetailData()
        {
            Util.gridClear(dgDetail);

            InitFaultyDataGrid(bClearAll: true);

            Util.gridClear(dgEqpFaulty);

            ClearLotInfo();

            rtxRemark.Document.Blocks.Clear();

            cboRemarkLot.ItemsSource = null;
            cboRemarkLot.Text = "";

            cboEqptDfctLot.ItemsSource = null;
            cboEqptDfctLot.Text = "";

            cboRollwayLot.ItemsSource = null;
            cboRollwayLot.Text = "";

        }

        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            ClearLotInfo();

            if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            ProcessDetail(dgProductLot.Rows[iRow].DataItem);
        }

        private void SetRemarkCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    dt.Rows.Add(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                }
                cboRemarkLot.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboRemarkLot.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqptDfctCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    dt.Rows.Add(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                }
                cboEqptDfctLot.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboEqptDfctLot.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRollwayLotCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    dt.Rows.Add(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                }
                cboRollwayLot.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboRollwayLot.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        private void SetInputQty()
        {
            if (!Util.CheckDecimal(txtInputQty.Text, 0))
            {
                txtInputQty.Text = "";
                return;
            }

            if (dgDetail.Rows.Count - dgDetail.TopRows.Count < 1)
                return;


            // 모랏 재공수량과 자랏의 생산수량 정보 비교.
            if (!txtParentQty.Text.Equals("") && !txtParent2.Text.Equals(""))
            {
                double iInputQty = double.Parse(txtInputQty.Text);
                double iParentWipQty = double.Parse(txtParentQty.Text);

                // N 건인 경우.
                int iChild = 1;
                if (dgDetail != null)
                    iChild = (dgDetail.Rows.Count - dgDetail.TopRows.Count - dgDetail.BottomRows.Count) < 1 ? 1 : (dgDetail.Rows.Count - dgDetail.TopRows.Count - dgDetail.BottomRows.Count);
                iInputQty = iInputQty * iChild;

                if (iInputQty > iParentWipQty)
                {
                    // 생산량 입력 후 엔터 시 처리 방법.
                    // 1. 마지막 완성LOT : 차이수량 만큼 길이초과 처리.
                    // 2. 마지막 완성LOT 아니면 : 생산량이 투입량을 초과할 수 없다.

                    // 1. 마지막 완성 LOT 체크
                    int iCut = 0;
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx < 0)
                        return;

                    bool bFndAnotherChild = false;
                    string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "CHILD_GR_SEQNO"));

                    if (!int.TryParse(sChildSeq, out iCut))
                    {
                        //Util.Alert("CUT이 숫자가 아닙니다.");
                        //return bRet;
                    }

                    string sTmpLot = "";
                    int iTmpMaxSeq = 0;


                    GetMaxChildGrSeq(out sTmpLot, out iTmpMaxSeq);

                    if (iTmpMaxSeq > 0 && iTmpMaxSeq > iCut)
                    {
                        bFndAnotherChild = true;
                    }

                    // 2. 마지막 완성랏이 아닌 경우 : 생산량이 투입량을 초과할 수 없습니다.
                    if (bFndAnotherChild)
                    {
                        //Util.Alert("생산량이 투입량을 초과할 수 없습니다.");
                        Util.MessageValidation("SFU1614");
                        txtInputQty.Text = "";
                        return;
                    }
                    else // 3. 마지막 완성랏인 경우 : 차이수량 길이초과 자동 등록
                    {
                        Util.MessageConfirm("SFU1921", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SaveLoss(iInputQty - iParentWipQty, "PLUS", true);

                                double inputQty = double.Parse(txtInputQty.Text);
                                //double inputQty = 0;
                                double lossQty = 0;

                                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                                {
                                    //inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                                    lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

                                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "INPUTQTY", inputQty);
                                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
                                }

                                txtInputQty.Text = "";

                                SetParentRemainQty();
                            }
                            else
                            {
                                txtInputQty.Text = "";
                                return;
                            }
                        }, (iInputQty - iParentWipQty).ToString(CultureInfo.InvariantCulture));

                    }
                }
                else
                {
                    double inputQty = double.Parse(txtInputQty.Text);
                    //double inputQty = 0;
                    double lossQty = 0;

                    for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                    {
                        //inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                        lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

                        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "INPUTQTY", inputQty);
                        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
                    }

                    txtInputQty.Text = "";

                    SetParentRemainQty();
                }
            }
            else
            {
                double inputQty = double.Parse(txtInputQty.Text);
                //double inputQty = 0;
                double lossQty = 0;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    //inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                    lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "INPUTQTY", inputQty);
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
                }

                txtInputQty.Text = "";

                SetParentRemainQty();
            }
        }

        private void SetParentRemainQty()
        {
            try
            {
                if (dgDetail.Rows.Count - dgDetail.TopRows.Count > 0)
                {
                    double dEqpInputSum = (double)(from tmp in DataTableConverter.Convert(dgDetail.ItemsSource).AsEnumerable() select tmp.Field<decimal>("INPUTQTY")).Sum();

                    if (double.Parse(txtParentQty.Text) - dEqpInputSum < 0)
                    {
                        txtParent2.Text = "0";
                        txtParent2_M.Text = "0";
                    }
                    else
                    {
                        // 버림 처리.
                        txtParent2.Text = Math.Truncate((double.Parse(txtParentQty.Text) - dEqpInputSum)).ToString("#,##0");

                        // 잔량 Meter 로 계산 필요.... 컬럼 없음.
                        int iPRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                        if (iPRow >= 0)
                        {
                            string sTmp = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iPRow].DataItem, "PTN_LEN"));
                            if (sTmp.Equals(""))
                            {
                                txtParent2_M.Text = txtParent2.Text;
                            }
                            else
                            {
                                double dTmp = 0;
                                double.TryParse(sTmp, out dTmp);
                                double dInputEa = dEqpInputSum;// double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.TopRows.Count].DataItem, "INPUTQTY")));
                                // EA = 재공(M) / 패턴길이
                                // M = 재공(EA) * 패턴길이
                                txtParent2_M.Text = Math.Truncate((double.Parse(txtParentQty_M.Text) - (dInputEa * dTmp))).ToString("#,##0");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEnabled(object selectItem)
        {

            DataRowView rowview = selectItem as DataRowView;

            if (rowview == null)
                return;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                btnRunStart.IsEnabled = false;
                btnRunCancel.IsEnabled = false;
                btnRunComplete.IsEnabled = false;
                btnConfirm.IsEnabled = false;
                btnPrint.IsEnabled = false;

                btnSaveFaulty.IsEnabled = false;
                //btnSaveRemark.IsEnabled = false;

                tbInputQty.Visibility = Visibility.Hidden;
                txtInputQty.Visibility = Visibility.Hidden;
            }
            else
            {
                if (Util.NVC(rowview["WIPSTAT"]).Equals("WAIT"))
                {
                    btnRunStart.IsEnabled = true;
                    btnRunCancel.IsEnabled = false;
                    btnRunComplete.IsEnabled = false;
                    btnConfirm.IsEnabled = false;
                    btnPrint.IsEnabled = false;

                    btnSaveFaulty.IsEnabled = false;
                    //btnSaveRemark.IsEnabled = false;

                    tbInputQty.Visibility = Visibility.Hidden;
                    txtInputQty.Visibility = Visibility.Hidden;
                }
                else if (Util.NVC(rowview["WIPSTAT"]).Equals("PROC") && Util.NVC(rowview["WIP_TYPE_CODE"]).Equals("IN"))
                {
                    btnRunStart.IsEnabled = true;
                    btnRunCancel.IsEnabled = false;
                    btnRunComplete.IsEnabled = false;
                    btnConfirm.IsEnabled = false;
                    btnPrint.IsEnabled = false;

                    btnSaveFaulty.IsEnabled = false;
                    //btnSaveRemark.IsEnabled = false;

                    tbInputQty.Visibility = Visibility.Hidden;
                    txtInputQty.Visibility = Visibility.Hidden;
                }
                else if (Util.NVC(rowview["WIPSTAT"]).Equals("PROC") && Util.NVC(rowview["WIP_TYPE_CODE"]).Equals("OUT"))
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = true;
                    btnRunComplete.IsEnabled = true;
                    btnConfirm.IsEnabled = false;
                    btnPrint.IsEnabled = false;

                    btnSaveFaulty.IsEnabled = true;
                    //btnSaveRemark.IsEnabled = true;

                    // 장비완료 process 추가로 수정.
                    tbInputQty.Visibility = Visibility.Hidden;
                    txtInputQty.Visibility = Visibility.Hidden;
                }
                else if (Util.NVC(rowview["WIPSTAT"]).Equals("EQPT_END"))
                {
                    btnRunStart.IsEnabled = false;
                    btnRunCancel.IsEnabled = false;
                    btnRunComplete.IsEnabled = false;
                    btnConfirm.IsEnabled = true;
                    btnPrint.IsEnabled = true;

                    btnSaveFaulty.IsEnabled = true;
                    //btnSaveRemark.IsEnabled = true;

                    tbInputQty.Visibility = Visibility.Visible;
                    txtInputQty.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetParentQty()
        {
            DataTable dt = GetChildEqpQty(_CurrentLotInfo.PR_LOTID, _CurrentLotInfo.LOTID);

            if (dt != null && dt.Rows.Count > 0)
            {
                string dTmp = "0";
                string dTmp2 = "0";

                dTmp = Util.NVC(dt.Rows[0]["WIPQTY_EA"]).Equals("") ? "0" : double.Parse(Util.NVC(dt.Rows[0]["WIPQTY_EA"])).ToString("#,##0");
                dTmp2 = Util.NVC(dt.Rows[0]["WIPQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(dt.Rows[0]["WIPQTY"])).ToString("#,##0");

                txtParentQty.Text = dTmp.ToString();
                txtParentQty_M.Text = dTmp2.ToString();

                SetParentRemainQty();
            }
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(sZPL, cboEquipment.SelectedValue.ToString());
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void ConfirmProcess()
        {
            int iTimes = 0;

            // 마지막 완성 LOT Check.
            int iCut = 0;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
                return;


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Rate 관련 팝업
            //완료 처리 하기 전에 팝업 표시
            //체크필요 멀티 LOT
            //생산 제품이 여러개인 경우 어떻게?           
            string sLOT = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
            _bInputErpRate = false;
            _dtRet_Data.Clear();
            _sUserID = string.Empty;
            _sDepID = string.Empty;

            if (PERMIT_RATE_Input_All())
            {               
                return;
            }
            else
            {
                //해당 사유 없으면 그냥 진행
                //return; //주석
                ///////////////////////
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            bool bFndAnotherChild = false;
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "CHILD_GR_SEQNO"));

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            string sTmpLot = "";
            int iTmpMaxSeq = 0;


            GetMaxChildGrSeq(out sTmpLot, out iTmpMaxSeq);

            if (iTmpMaxSeq > 0 && iTmpMaxSeq > iCut)
            {
                bFndAnotherChild = true;
            }

            // 마지막 완성 LOT이 아닌 경우
            if (bFndAnotherChild)
            {
                if (txtParent2.Text.Trim().Equals("0"))
                {
                    //Util.Alert("다음 완성LOT이 존재하여 실적확정 불가합니다.");
                    Util.MessageValidation("SFU1483");
                }
                else
                {
                    //string sMsg = "투입LOT 잔량 {0}가 남게 됩니다. 실적확정 하시겠습니까?";
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg, txtParent2.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1963", (result) =>
                    {
                        iTimes++;

                        if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                        if (result == MessageBoxResult.OK)
                        {
                            try
                            {
                                // 자동 불량 저장 처리.
                                SaveDefectAllBeforeConfirm(false);
                            } catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }finally{
                                Thread.Sleep(2000);
                                Confirm();
                            }                           
                            
                        }
                    }, txtParent2_M.Text.Trim());
                }
            }
            else // 마지막 완성 LOT인 경우
            {
                if (txtParent2.Text.Trim().Equals("0"))
                {
                    //string sMsg = "투입LOT 잔량이 없습니다. 실적확정 하시겠습니까?";
                    Util.MessageConfirm("SFU1965", (result) =>
                    {
                        iTimes++;

                        if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                        if (result == MessageBoxResult.OK)
                        {
                            // 자동 불량 저장 처리.
                           // SaveDefectAllBeforeConfirm(false);
                         //   Confirm();
                            try
                            {
                                // 자동 불량 저장 처리.
                                SaveDefectAllBeforeConfirm(false);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                Thread.Sleep(2000);
                                Confirm();
                            }
                        }
                    });
                }
                else
                {
                    double dRemain = 0;
                    double dRemain_M = 0;

                    double.TryParse(txtParent2.Text.Trim(), out dRemain);
                    double.TryParse(txtParent2_M.Text.Trim(), out dRemain_M);

                    if (dRemain_M >= 5) //수정부분
                    {
                        ////LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입LOT 잔량 {0}가 대기됩니다.\n실적확정 하시겠습니까?", txtParent2.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //Util.MessageConfirm("SFU1964", (result) =>
                        //{
                        //    if (result == MessageBoxResult.OK)
                        //    {
                        //        Confirm();
                        //    }
                        //}, txtParent2_M.Text.Trim());

                        //if (wndConfirm != null)
                        //{
                        //    wndConfirm = null;
                        //    //return;
                        //}

                        ASSY003_001_CONFIRM wndConfirm = new ASSY003_001_CONFIRM();

                        wndConfirm.FrameOperation = FrameOperation;

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = txtParent2.Text.Trim();
                            Parameters[1] = txtParent2_M.Text.Trim();
                            C1WindowExtension.SetParameters(wndConfirm, Parameters);

                            //wndConfirm.Closed -= new EventHandler(wndConfirm_Closed);
                            wndConfirm.Closed -= new EventHandler(wndConfirm_Closed);
                            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);


                            // 팝업 화면 숨겨지는 문제 수정.
                            this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                            //grdMain.Children.Add(wndConfirm);
                            //bWndConfirmJobEnd = true;
                            //wndConfirm.BringToFront();
                        }
                    }
                    else
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("잔량 {0}가 길이부족으로 등록되고, 투입LOT 잔량이 없습니다.\n실적확정 하시겠습니까?", txtParent2.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        Util.MessageConfirm("SFU1853", (result) =>
                        {
                            iTimes++;

                            if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                            if (result == MessageBoxResult.OK)
                            {
                                //SaveLoss(dRemain, "MINUS", false, false);

                                //ConfirmLength_Lack();

                                try
                                {
                                    SaveLoss(dRemain, "MINUS", false, false);
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    Thread.Sleep(2000);
                                    ConfirmLength_Lack();
                                }
                            }
                            else
                            {
                                return;
                            }
                        }, txtParent2.Text.Trim());
                    }
                }
            }
        }

        private void ConfirmProcess_Rate()
        {
            int iTimes = 0;

            // 마지막 완성 LOT Check.
            int iCut = 0;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
                return;

            bool bFndAnotherChild = false;
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "CHILD_GR_SEQNO"));

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            string sTmpLot = "";
            int iTmpMaxSeq = 0;


            GetMaxChildGrSeq(out sTmpLot, out iTmpMaxSeq);

            if (iTmpMaxSeq > 0 && iTmpMaxSeq > iCut)
            {
                bFndAnotherChild = true;
            }

            // 마지막 완성 LOT이 아닌 경우
            if (bFndAnotherChild)
            {
                if (txtParent2.Text.Trim().Equals("0"))
                {
                    //Util.Alert("다음 완성LOT이 존재하여 실적확정 불가합니다.");
                    Util.MessageValidation("SFU1483");
                }
                else
                {
                    //string sMsg = "투입LOT 잔량 {0}가 남게 됩니다. 실적확정 하시겠습니까?";
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg, txtParent2.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1963", (result) =>
                    {
                        iTimes++;

                        if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                        if (result == MessageBoxResult.OK)
                        {
                            try
                            {
                                // 자동 불량 저장 처리.
                                SaveDefectAllBeforeConfirm(false);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                Thread.Sleep(2000);
                                Confirm();
                            }

                        }
                    }, txtParent2_M.Text.Trim());
                }
            }
            else // 마지막 완성 LOT인 경우
            {
                if (txtParent2.Text.Trim().Equals("0"))
                {
                    //string sMsg = "투입LOT 잔량이 없습니다. 실적확정 하시겠습니까?";
                    Util.MessageConfirm("SFU1965", (result) =>
                    {
                        iTimes++;

                        if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                        if (result == MessageBoxResult.OK)
                        {
                     
                            try
                            {
                                // 자동 불량 저장 처리.
                                SaveDefectAllBeforeConfirm(false);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                Thread.Sleep(2000);
                                Confirm();
                            }
                        }
                    });
                }
                else
                {
                    double dRemain = 0;
                    double dRemain_M = 0;

                    double.TryParse(txtParent2.Text.Trim(), out dRemain);
                    double.TryParse(txtParent2_M.Text.Trim(), out dRemain_M);

                    if (dRemain_M >= 5) //수정부분
                    {
                        ASSY003_001_CONFIRM wndConfirm = new ASSY003_001_CONFIRM();

                        wndConfirm.FrameOperation = FrameOperation;

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = txtParent2.Text.Trim();
                            Parameters[1] = txtParent2_M.Text.Trim();
                            C1WindowExtension.SetParameters(wndConfirm, Parameters);

                            wndConfirm.Closed -= new EventHandler(wndConfirm_Closed);
                            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);


                            // 팝업 화면 숨겨지는 문제 수정.
                            this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                        }
                    }
                    else
                    {
                        Util.MessageConfirm("SFU1853", (result) =>
                        {
                            iTimes++;

                            if (iTimes > 1) return;     // 확정 처리 로직 2번 타는 경우 발생하는 문제(VD Loss로 잘못 저장)로 인한 수정.

                            if (result == MessageBoxResult.OK)
                            {

                                try
                                {
                                    SaveLoss(dRemain, "MINUS", false, false);
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    Thread.Sleep(2000);
                                    ConfirmLength_Lack();
                                }
                            }
                            else
                            {
                                return;
                            }
                        }, txtParent2.Text.Trim());
                    }
                }
            }
        }


        private void Confirm_Process()
        {
            try
            {
                if (CheckModelChange() && !CheckInputEqptCond())
                {
                    // 해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?
                    Util.MessageConfirm("SFU2817", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ConfirmProcess();
                        }
                    });
                }
                else
                {
                    ConfirmProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 2017.07.20  Lee. D. R 
        /// 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
        /// </summary>
        private void GetLossCnt()
        {
            try
            {
                txtLossCnt.Text = "";
                txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);

                DataTable dtEqpLossCnt = Util.Get_EqpLossCnt(cboEquipment.SelectedValue.ToString());

                if (dtEqpLossCnt != null && dtEqpLossCnt.Rows.Count > 0)
                {
                    txtLossCnt.Text = Util.NVC(dtEqpLossCnt.Rows[0]["CNT"]);

                    if (Util.NVC_Int(dtEqpLossCnt.Rows[0]["CNT"]) > 0)
                    {
                        txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.LightPink);
                    }
                    else
                    {
                        txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 계획정지, 테스트 Run
        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTestMode()) return;

            if (bTestMode)
            {
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

                        SetTestMode(true);
                        GetTestMode();
                    }
                });
            }
        }

        private void btnScheduledShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanScheduledShutdownMode()) return;

                if (bTestMode)
                {
                    SetTestMode(false, bShutdownMode: true);
                    GetTestMode();
                }
                else
                {
                    Util.MessageConfirm("SFU4460", (result) => // 계획정지를 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

                            SetTestMode(true, bShutdownMode: true);
                            GetTestMode();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanTestMode()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (bScheduledShutdown)
            //{
            //    Util.MessageValidation("SFU4464"); // 계획정지중 입니다. 계획정지를 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CanScheduledShutdownMode()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (bTestMode)
            //{
            //    Util.MessageValidation("SFU4465"); // 테스트 Run 중 입니다. 테스트 Run 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void HideScheduledShutdown()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowScheduledShutdown()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(false);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(false);
        }

        private void HideScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ColorAnimationInRectangle(bool bTest)
        {
            try
            {
                string sname = string.Empty;
                if (bTest)
                {
                    recTestMode.Fill = redBrush;
                    sname = "redBrush";
                }
                else
                {
                    recTestMode.Fill = yellowBrush;
                    sname = "yellowBrush";
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1.0;
                opacityAnimation.To = 0.0;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
                opacityAnimation.AutoReverse = true;
                opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTargetName(opacityAnimation, sname);
                Storyboard.SetTargetProperty(
                    opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowTestMode()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(true);
                    return;
                }
                
                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(true);
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }
        #endregion

        #endregion

        #endregion

        #region 선입선출 로직 추가

        private void GetProcMtrlInputRule()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_PRE_PROC_RCPT_DAY", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _Max_Pre_Proc_End_Day = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals("WAIT"))
                    {
                        if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                        {
                            e.Cell.Presenter.Background = null;
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            int iDay = 0;
                            int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                            if (iDay > 0)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))
                                {
                                    e.Cell.Presenter.Background = null;
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                }
                                else
                                {
                                    DateTime dtValid;
                                    DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                    if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = null;
                                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                    }
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                        }
                    }
                }
            }));
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }
        #endregion

        private void dgFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                    {
                        string sColName = "DEFECTQTY" + (i + 1).ToString();
                        if (Convert.ToString(e.Cell.Column.Name) == sColName)
                        {
                            string sFlag = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                            if (sFlag == "Y")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                //e.Cell.Column.IsReadOnly = false;
                                //e.Cell.Column.EditOnSelection = true;
                            }
                        }
                    }
                }
            }));
        }

        private void dgFaulty_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    string sColName = "DEFECTQTY" + (i + 1).ToString();
                    if (e.Column.Name.Equals(sColName))
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    if (dgProductLot == null) return;

                    if (dgProductLot.Columns.Contains("PTN_LEN"))
                        dgProductLot.Columns["PTN_LEN"].Visibility = Visibility.Visible;
                    if (dgProductLot.Columns.Contains("PTN_LEN_OLD"))
                        dgProductLot.Columns["PTN_LEN_OLD"].Visibility = Visibility.Visible;
                    if (dgProductLot.Columns.Contains("WIPQTY_EA_OLD"))
                        dgProductLot.Columns["WIPQTY_EA_OLD"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckMultiProduction()
        {
            try
            {
                _MUST_CHG_FLAG = "";

                bool bRet = false;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["MTRLID"] = _CurrentLotInfo.PR_PRODID;

                inTable.Rows.Add(newRow);

                DataTable dtRet = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MULTI_PROD_CHK", "INDATA", "OUTDATA", inTable);

                if (dtRet != null && dtRet.Rows.Count > 1)
                {
                    DataRow[] drTmp = dtRet.Select("PRODID = '" + _CurrentLotInfo.PRODID + "'");
                    if (drTmp.Length > 0 && dtRet.Columns.Contains("MUST_CHG_FLAG"))
                        _MUST_CHG_FLAG = Util.NVC(drTmp[0]["MUST_CHG_FLAG"]);

                    bRet = true;
                }

                if (bRet)
                {
                    btnChangeWO.Visibility = Visibility.Visible;
                    if (dgDetail != null && dgDetail.Columns.Contains("CHK")) dgDetail.Columns["CHK"].Visibility = Visibility.Visible;
                    //if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].IsReadOnly = false;
                    //if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].EditOnSelection = true;
                }
                else
                {
                    btnChangeWO.Visibility = Visibility.Collapsed;
                    if (dgDetail != null && dgDetail.Columns.Contains("CHK")) dgDetail.Columns["CHK"].Visibility = Visibility.Collapsed;
                    //if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].IsReadOnly = true;
                    //if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].EditOnSelection = false;
                }

                HiddenLoadingIndicator();

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnChangeWO.Visibility = Visibility.Collapsed;
                if (dgDetail != null && dgDetail.Columns.Contains("CHK")) dgDetail.Columns["CHK"].Visibility = Visibility.Collapsed;
                //if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].IsReadOnly = true;
                //if (dgDetail != null && dgDetail.Columns.Contains("INPUTQTY")) dgDetail.Columns["INPUTQTY"].EditOnSelection = false;

                HiddenLoadingIndicator();
                return false;
            }
        }
        
        private void btnChangeWO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanChangeWorkOrder()) return;

                
                wndChgWorkOrder = new ASSY003_001_CHANGE_WO();
                wndChgWorkOrder.FrameOperation = FrameOperation;

                if (wndChgWorkOrder != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");
                    object[] Parameters = new object[8];
                    Parameters[0] = Process.NOTCHING;
                    Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "WIPSEQ"));

                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "INPUTQTY"));
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "GOODQTY"));
                    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "DFCT_SUM"));

                    C1WindowExtension.SetParameters(wndChgWorkOrder, Parameters);

                    wndChgWorkOrder.Closed += new EventHandler(wndChgWorkOrder_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndChgWorkOrder.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndChgWorkOrder_Closed(object sender, EventArgs e)
        {
            wndChgWorkOrder = null;

            ASSY003_001_CHANGE_WO window = sender as ASSY003_001_CHANGE_WO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetDetailData();

                // W/O 변경 여부 체크.
                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    if (i > dgDetail.TopRows.Count &&
                        !Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRODID")).Equals(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i - 1].DataItem, "PRODID"))))
                    {
                        _MUST_CHG_FLAG = "N";
                    }
                }

                GetFaultyData();

                SetLotInfoCalc();

                //if (_CurrentLotInfo.STATUS.Equals("EQPT_END"))// || (_CurrentLotInfo.STATUS.Equals("PROC") && _CurrentLotInfo.WIP_TYPE_CODE.Equals("OUT"))) // 장비완료 process 추가로 수정.
                //{
                    SetParentQty();

                    //CheckMultiProduction();
                //}
            }
        }

        private void dgOutLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                    }

                    //row 색 바꾸기
                    dgDetail.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanChangeWorkOrder()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }


            DataTable dtTmp = DataTableConverter.Convert(dgDetail.ItemsSource);
            DataRow[] dtRows = dtTmp.Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODID")) + "' AND CHK <> 1 ");

            if (dtRows != null && dtRows.Length < 1)
            {
                Util.MessageValidation("SFU4461");  // W/O 변경은 완성LOT 모두다 변경할 수 없습니다.
                return bRet;
            }
            
            bRet = true;
            return bRet;
        }

        private void btnLotCut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (wndLotCut != null)
                {
                    wndLotCut = null;
                    //return;
                }

                wndLotCut = new CMM_ASSY_LOTCUT();
                wndLotCut.FrameOperation = FrameOperation;

                if (wndLotCut != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = Process.NOTCHING; 
                    Parameters[1] = cboEquipmentSegment.SelectedValue;
                    Parameters[2] = cboEquipment.SelectedValue;                    

                    C1WindowExtension.SetParameters(wndLotCut, Parameters);

                    wndLotCut.Closed += new EventHandler(wndLotCut_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndLotCut.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPartScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (wndPartScrap != null)
                {
                    wndPartScrap = null;
                    //return;
                }

                wndPartScrap = new ASSY003_001_WAITLOT_PART_SCRAP();
                wndPartScrap.FrameOperation = FrameOperation;

                if (wndPartScrap != null)
                {
                    object[] Parameters = new object[2];                    
                    Parameters[0] = cboEquipmentSegment.SelectedValue;
                    Parameters[1] = cboEquipment.SelectedValue;

                    C1WindowExtension.SetParameters(wndPartScrap, Parameters);

                    wndPartScrap.Closed += new EventHandler(wndPartScrap_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPartScrap.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnQualityInput_New_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (wndQualityInput != null)
            {
                wndQualityInput = null;
                //return;
            }

            wndQualityInput = new CMM_COM_SELF_INSP();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Process.NOTCHING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInputAutoMobile_New_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }

        #region [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cbobox = sender as C1ComboBox;

            if (cbobox != null && cbobox.SelectedValue.Equals("") || cbobox.SelectedValue.Equals("SELECT")) return;

            CheckLineUseTWS(cbobox.SelectedValue.ToString());
            SetVisibleObject_TWS();
        }


        private void CheckLineUseTWS(string sEQSGID)
        {
            try
            {
                isTWS_LOADING_TRACKING = false;
                isPassSaveTWS_LOADING_TRACKING = false;
                isAutoSetRollDir = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = sEQSGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_TWS_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    isTWS_LOADING_TRACKING = true;
                    if (dtResult.Rows[0]["ATTR1"].ToString() == "P") isPassSaveTWS_LOADING_TRACKING = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetVisibleObject_TWS()
        {
            if (isTWS_LOADING_TRACKING == true)
            {
                tiRollDirection.Visibility = Visibility.Visible;
            }
        }

        protected virtual void OnClickSaveRollDir(object sender, RoutedEventArgs e)
        {
            SetTWS_LOADING_TRACKING();
        }

        private void SetTWS_LOADING_TRACKING()
        {
            if (dgDetail.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                return;
            }


            string strRollin = "", strInputRollLane = "";

            if (rbRollinUp.IsChecked.Value == true)        strRollin = "U";
            else if (rbRollinDown.IsChecked.Value == true) strRollin = "D";

            if (rbInputRollLaneForward.IsChecked.Value == true)      strInputRollLane = "F";
            else if (rbInputRollLaneReverse.IsChecked.Value == true) strInputRollLane = "R";


            // 사용자 확정값이 모두 있는지 확인
            if (string.IsNullOrEmpty(strRollin))
            {
                string item = ObjectDic.Instance.GetObjectName("ROLLIN");
                Util.MessageValidation("FM_ME_0251", new object[] { item });  //필수항목누락 : %1
                return;
            }
            else if (string.IsNullOrEmpty(strInputRollLane))
            {
                string item = ObjectDic.Instance.GetObjectName("투입 구간 롤 LANE 방향");
                Util.MessageValidation("FM_ME_0251", new object[] { item });  //필수항목누락 : %1
                return;
            }

            //비즈 호출하여 값을 세팅한다.
            DataSet inDataSet = new DataSet();

            DataTable rqstDt = inDataSet.Tables.Add("INDATA");
            rqstDt.Columns.Add("AREAID", typeof(string));
            rqstDt.Columns.Add("LOTID", typeof(string));
            rqstDt.Columns.Add("PROCID", typeof(string));
            rqstDt.Columns.Add("INPUT_SECTION_ROLL_LANE_DIRCTN", typeof(string));
            rqstDt.Columns.Add("INPUT_SECTION_ROLL_DIRCTN", typeof(string));
            rqstDt.Columns.Add("PR_LOTID", typeof(string));
            rqstDt.Columns.Add("USERID", typeof(string));

            DataRow rqstDr = null;

            for (int i = 2; i < dgDetail.Rows.Count; i++)
            {
                rqstDr = rqstDt.NewRow();
                rqstDr["AREAID"] = LoginInfo.CFG_AREA_ID;
                rqstDr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                rqstDr["PROCID"] = Process.NOTCHING;
                rqstDr["INPUT_SECTION_ROLL_LANE_DIRCTN"] = strInputRollLane;
                rqstDr["INPUT_SECTION_ROLL_DIRCTN"] = strRollin;
                rqstDr["PR_LOTID"] = _CurrentLotInfo.PR_LOTID;
                rqstDr["USERID"] = LoginInfo.USERID;

                rqstDt.Rows.Add(rqstDr);
            }


            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TB_SFC_WIP_TWS_LOAD_NT", "INDATA", null, (result, ex) =>
            {
                if (ex != null)
                {
                    if (isPassSaveTWS_LOADING_TRACKING)
                        GetTWS_LOADING_TRACKING();
                    else
                        Util.MessageException(ex);

                    return;
                }

                if (isSilentlySave == false)
                {
                    GetTWS_LOADING_TRACKING();
                    Util.MessageInfo("SFU1270");
                }
            }, inDataSet);
        }

        private void GetTWS_LOADING_TRACKING()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _CurrentLotInfo.PR_LOTID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_WIP_TWS_LOAD", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    isAutoSetRollDir = false;

                    string strOutputRollLane = Util.NVC(result.Rows[0]["EM_SECTION_ROLL_LANE_DIRCTN"]);
                    if (strOutputRollLane.Equals("F"))
                    {
                        rbInputRollLaneForward.IsChecked = true;
                    }
                    else if (strOutputRollLane.Equals("R"))
                    {
                        rbInputRollLaneReverse.IsChecked = true;
                    }
                    else
                    {
                        rbInputRollLaneForward.IsChecked = false;
                        rbInputRollLaneReverse.IsChecked = false;
                    }
                }

                if (string.IsNullOrEmpty(_CurrentLotInfo.LOTID) == false)
                {

                    IndataTable.Clear();
                    Indata = IndataTable.NewRow();
                    Indata["LOTID"] = _CurrentLotInfo.LOTID;
                    IndataTable.Rows.Add(Indata);

                    result = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_WIP_TWS_LOAD", "INDATA", "RSLTDT", IndataTable);

                    if (result != null && result.Rows.Count > 0)
                    {
                        string strInputRollLane = Util.NVC(result.Rows[0]["INPUT_SECTION_ROLL_LANE_DIRCTN"]);

                        isAutoSetRollDir = false;

                        if (strInputRollLane.Equals("F"))
                        {
                            rbInputRollLaneForward.IsChecked = true;
                        }
                        else if (strInputRollLane.Equals("R"))
                        {
                            rbInputRollLaneReverse.IsChecked = true;
                        }


                        string strInputRollin = Util.NVC(result.Rows[0]["INPUT_SECTION_ROLL_DIRCTN"]);

                        if (strInputRollin.Equals("U"))
                        {
                            rbRollinUp.IsChecked = true;
                        }
                        else if (strInputRollin.Equals("D"))
                        {
                            rbRollinDown.IsChecked = true;
                        }
                        else
                        {
                            rbRollinUp.IsChecked = false;
                            rbRollinDown.IsChecked = false;
                        }
                    }
                }

                if ((rbRollinUp.IsChecked == true || rbRollinDown.IsChecked == true)
                    && (rbInputRollLaneReverse.IsChecked == true || rbInputRollLaneForward.IsChecked == true))
                {
                    isSaveTWS_LOADING_TRACKING = true;
                }

                //롤방향 검증
                if (isAutoSetRollDir == false)
                    SetTWS_RULE_PROCESS_EQPT_ROLL_DIR(Process.NOTCHING, _CurrentLotInfo.PR_LOTID);
            }
            catch (Exception ex) { }
        }
        #endregion

        #region [E20240715-000063] 전극공정진척 Roll in & Roll out 방향 기본 세팅값을 시스템 상 미리 표기
        private void SetTWS_RULE_PROCESS_EQPT_ROLL_DIR(string ProcessID, string LotID)
        {
            if (string.IsNullOrEmpty(ProcessID) || string.IsNullOrEmpty(LotID)) return;

            string sComeCode = string.Format("{0}_{1}", ProcessID, LotID.Substring(1, 1));
            string strRollin = "";
            DataTable dtResult = _Util.GetAreaCommonCodeUse("TWS_RULE_PROCESS_EQPT_ROLL_DIR", sComeCode);
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                strRollin = dtResult.Rows[0]["ATTR1"].ToString();

                // 자동 세팅값과 일치할 때 자동세팅한 것으로 간주한다
                if ((string.IsNullOrEmpty(strRollin) == false))
                {
                    bool check1 = false;

                    if (strRollin == "U" && rbRollinUp.IsChecked == true) check1 = true;
                    if (strRollin == "D" && rbRollinDown.IsChecked == true) check1 = true;

                    // 기준정보와 일치된다면 자동세팅 된 것으로 간주한다
                    if (check1)
                    {
                        isAutoSetRollDir = true;
                    }
                    else
                    {
                        // 자동세팅을 무시하고 저장된 값으로 간주한다
                        isAutoSetRollDir = false;
                    }
                }

                //자동세팅하지 않았거나 저장된 롤 방향이 없으면 자동세팅한다
                if (isAutoSetRollDir == false && isSaveTWS_LOADING_TRACKING == false)
                {
                    if (string.IsNullOrEmpty(strRollin) == false)
                    {
                        if (strRollin == "U") rbRollinUp.IsChecked = true;
                        else if (strRollin == "D") rbRollinDown.IsChecked = true;
                    }

                    if ((rbRollinUp.IsChecked.Value || rbRollinDown.IsChecked.Value)
                        && (rbInputRollLaneForward.IsChecked.Value || rbInputRollLaneReverse.IsChecked.Value))
                    {
                        isAutoSetRollDir = true;
                    }
                }
            }
        }
        #endregion

    }
}
