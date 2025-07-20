/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Folding 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER   : Initial Created.
  2016.10.05  INS 김동일K : 공정 진척 화면 수정
  2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
  2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
  2017.07.24  INS 염규범S : FOLDING 자동 닫김
  2017.11.01  INS 염규범S : 폴딩/스태킹 불량 장비 반영 여부에 따른 로직 변경
  2017.12.06  INS 염규범S : GMES 폴딩 진척 현황에서 바코드 발행 오류 개선 - C20171107_25230
  2020.08.31  김동일K     : C20200717-000299 - 자동 조회 기능 추가
  2020.10.08  김동일K     : C20200717-000299 - 미발행 바구니 전체 선택 기능 추가.
                                               확정 시 진행중 LOT 존재 시 삭제 처리 메시지 확인 후 처리 기능 추가.
                                               발행, 저장 시 진행중인 LOT 처리 관련 로직 수정
  2021.01.06  이상훈        C20200806-000180   폴딩 공정진척 반제품 발행오류 개선
  2023.01.31  김광섭 : C20221201-000464 [생산PI팀] 조립공정별 완공처리시 Validation 체크 로직 추가 요청
  2023.05.17  안유수 : E20230427-001043 ESMI 공정PC GMES 계정으로 접근 시 투입 LOT 종료취소 버튼 비활성화 처리
  2023.07.17  안유수 : E20230707-001494 ESMI법인의 경우, 추가 기능 - 불량정보관리 버튼 숨김 처리
  2024.02.14  김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_005 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private bool bSetAutoSelTime = false;
        private bool bTestMode = false;
        private bool _B_PROC_BOX_AUTO_DEL_AREA = false;    // 진행중 매거진 자동 삭제 여부
        private bool _BOX_CREATE_PROCESSING = false;
        private string _LABEL_PRT_RESTRCT_FLAG = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();
        private UC_IN_OUTPUT winInput = null;


        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        #endregion

        #region Initialize
        public ASSY001_005()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.STACKING_FOLDING, EquipmentGroup.FOLDING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            //ESMI 1동(A4) 1~5 Line 만 조회
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_BY_EQGRID");
            if (IsCmiExceptLine())
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "ESMI_A4_EQUIPMENTSEGMENT_BY_EXCEPT_EQGRID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_BY_EQGRID");
            }

            String[] sFilter2 = { Process.STACKING_FOLDING, EquipmentGroup.FOLDING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_BY_EQSGID_NEW");
            // 2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
            // EQUIPMENT_BY_EQSGID => EQUIPMENT_BY_EQSGID_NEW 신규 생성

            // 자동 조회 시간 Combo
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;
        }

        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //GetTestMode();

            ApplyPermissions();

            SetWorkOrderWindow();

            SetInputWindow();

            #region 폴란드 Op 권한 "투입LOT종료취소" 메뉴 제거.
            if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
            {
                if (CheckManagerAuth())
                    btnCancelTerm.Visibility = Visibility.Visible;
                else
                    btnCancelTerm.Visibility = Visibility.Collapsed;
            }
            #endregion

            #region ESMI 공정PC GMES 계정 접근 시 투입 LOT 종료취소 버튼 비활성화
            if (LoginInfo.CFG_SHOP_ID.Equals("G382"))
            {
                btnDefect.Visibility = Visibility.Collapsed;

                if (LoginInfo.USERTYPE == "P")
                    btnCancelTerm.Visibility = Visibility.Collapsed;
                else
                    btnCancelTerm.Visibility = Visibility.Visible;
            }
            #endregion

            // 생산 반제품 조회 Timer Start.
            if (dispatcherTimer != null)
                dispatcherTimer.Start();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();

            // 생산 반제품 조회 Timer
            if (dispatcherTimer != null)
            {
                int iSec = 0;

                if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                //dispatcherTimer.Start();
            }

            this.RegisterName("redBrush", redBrush);

            HideTestMode();

            //SetUserCheckFlag();
            txtUserNameCr.Text = string.Empty;
            txtUserNameCr.Tag = string.Empty;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HiddenLoadingIndicator();
                return;
            }

            // 기준정보 조회
            GetLotIdentBasCode();
            SetInOutCtrlByLotIdentBasCode();    // Lot 식별 기준 코드에 따른 컨트롤 변경.
            SetUserCheckFlag();


            GetWorkOrder();
            GetProductLot();

            GetEqptWrkInfo();
            SetUserCheckFlag();
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            ASSY001_005_RUNSTART wndRunStart = new ASSY001_005_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = _LDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.Closed += new EventHandler(wndRunStart_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                grdMain.Children.Add(wndRunStart);
                wndRunStart.BringToFront();
            }
        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunComplete())
                return;

            CMM_ASSY_EQPEND wndPop = new CMM_ASSY_EQPEND();
            wndPop.FrameOperation = FrameOperation;

            if (wndPop != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = "N";    // Stacking.
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPDTTM_ST_ORG"));
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "DTTM_NOW"));
                C1WindowExtension.SetParameters(wndPop, Parameters);

                wndPop.Closed += new EventHandler(wndEqpEnd_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                grdMain.Children.Add(wndPop);
                wndPop.BringToFront();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm())
                return;

            #region 작업조건/품질정보 입력 여부
            string _ValueToLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

            // 작업조건 등록 여부
            if (Util.EQPTCondition(Process.STACKING_FOLDING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnEqptCond_Click(null, null);
                return;
            }
            // 품질정보 등록 여부
            if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, Process.STACKING_FOLDING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnQualityInput_Click(null, null);
                return;
            }
            #endregion

            // 설비 Loss 등록 여부 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.STACKING_FOLDING);

            if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
            {
                string sInfo = string.Empty;
                string sLossInfo = string.Empty;

                for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                {
                    sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
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

        private void Confirm_Process()
        {
            if (CheckModelChange() && !CheckInputEqptCond())
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        private void ConfirmProcess()
        {
            string sTmpMag = "";
            bool bFind = false;

            // 생산 반제품이 PROC 상태가 존재하는 경우 처리 문의 후 삭제 처리.

            DataTable dtTmp = DataTableConverter.Convert(dgOutProduct.ItemsSource);

            DataRow[] drList = dtTmp.Select("WIPSTAT = 'PROC'");
            if (drList?.Length > 0)
            {
                bFind = true;

                for (int i = 0; i < drList.Length; i++)
                {
                    sTmpMag = sTmpMag.Equals("") ? Util.NVC(drList[i]["LOTID"]) : ", " + Util.NVC(drList[i]["LOTID"]);
                }
            }

            if (bFind && _B_PROC_BOX_AUTO_DEL_AREA)
            {
                // PROC 삭제 처리 후 확정
                Util.MessageConfirm("SFU1918", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteProcOutBox();

                        ASSY001_005_CONFIRM wndConfirm = new ASSY001_005_CONFIRM();
                        wndConfirm.FrameOperation = FrameOperation;

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[13];
                            Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[1] = cboEquipment.SelectedValue.ToString();
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                            Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

                            Parameters[5] = Util.NVC(txtShift.Text); //= Util.NVC(wndPopup.SHIFTNAME);
                            Parameters[6] = Util.NVC(txtShift.Tag); //= Util.NVC(wndPopup.SHIFTCODE);
                            Parameters[7] = Util.NVC(txtWorker.Text); //= Util.NVC(wndPopup.USERNAME);
                            Parameters[8] = Util.NVC(txtWorker.Tag); //= Util.NVC(wndPopup.USERID);                            
                            Parameters[9] = Util.NVC(txtShiftStartTime.Text); //= Util.NVC(wndPopup.WRKSTRTTIME);
                            Parameters[10] = Util.NVC(txtShiftEndTime.Text); //= Util.NVC(wndPopup.WRKENDTTIME);

                            Parameters[11] = _LDR_LOT_IDENT_BAS_CODE;
                            Parameters[12] = _UNLDR_LOT_IDENT_BAS_CODE;

                            C1WindowExtension.SetParameters(wndConfirm, Parameters);

                            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                            // 팝업 화면 숨겨지는 문제 수정.
                            //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                            grdMain.Children.Add(wndConfirm);
                            wndConfirm.BringToFront();
                        }
                    }
                }, sTmpMag);
            }
            else
            {
                ASSY001_005_CONFIRM wndConfirm = new ASSY001_005_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

                    Parameters[5] = Util.NVC(txtShift.Text); //= Util.NVC(wndPopup.SHIFTNAME);
                    Parameters[6] = Util.NVC(txtShift.Tag); //= Util.NVC(wndPopup.SHIFTCODE);
                    Parameters[7] = Util.NVC(txtWorker.Text); //= Util.NVC(wndPopup.USERNAME);
                    Parameters[8] = Util.NVC(txtWorker.Tag); //= Util.NVC(wndPopup.USERID);                            
                    Parameters[9] = Util.NVC(txtShiftStartTime.Text); //= Util.NVC(wndPopup.WRKSTRTTIME);
                    Parameters[10] = Util.NVC(txtShiftEndTime.Text); //= Util.NVC(wndPopup.WRKENDTTIME);

                    Parameters[11] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[12] = _UNLDR_LOT_IDENT_BAS_CODE;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_005_WAITLOT wndWaitLot = new ASSY001_005_WAITLOT();
            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Util.GetCondition(cboEquipmentSegment);
                C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWaitLot_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
                grdMain.Children.Add(wndWaitLot);
                wndWaitLot.BringToFront();
            }
        }

        private void btnWaitLotDelete_Click(object sender, RoutedEventArgs e)
        {

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("9080");
                return;
            }

            try
            {
                CMM_ASSY_WAITLOT_DELETE popWait = new CMM_ASSY_WAITLOT_DELETE { FrameOperation = FrameOperation };

                object[] parameters = new object[4];
                parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[1] = cboEquipment.SelectedValue.ToString();
                parameters[2] = Process.STACKING_FOLDING;
                parameters[3] = ProcessType.Fold;
                C1WindowExtension.SetParameters(popWait, parameters);

                popWait.Closed += new EventHandler(popWait_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => popWait.ShowModal()));
                grdMain.Children.Add(popWait);
                popWait.BringToFront();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popWait_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WAITLOT_DELETE pop = sender as CMM_ASSY_WAITLOT_DELETE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(pop);
        }

        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;

            CMM_ASSY_QUALITY wndQuality = new CMM_ASSY_QUALITY();
            wndQuality.FrameOperation = FrameOperation;

            if (wndQuality != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Process.STACKING_FOLDING;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQuality, Parameters);

                wndQuality.Closed += new EventHandler(wndQuality_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndQuality.ShowModal()));
                grdMain.Children.Add(wndQuality);
                wndQuality.BringToFront();
            }
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            CMM_COM_EQPCOMMENT wndEqpComment = new CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                Parameters[6] = txtShift.Text; // 작업조명
                Parameters[7] = txtShift.Tag; // 작업조코드
                Parameters[8] = txtWorker.Text; // 작업자명
                Parameters[9] = txtWorker.Tag; // 작업자 ID

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                grdMain.Children.Add(wndEqpComment);
                wndEqpComment.BringToFront();
            }
        }

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //if (winWorkOrder == null)
            //    return;

            //winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            //winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            //winWorkOrder.PROCID = Process.STACKING_FOLDING;

            //winWorkOrder.ClearWorkOrderInfo();


            //if (winInput == null)
            //    return;

            //winInput.ChangeEquipment(cboEquipmentSegment.SelectedValue.ToString(), cboEquipment.SelectedValue.ToString());

            //ClearControls();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                ClearControls();

                string sEqsgID = string.Empty;
                string sEqptID = string.Empty;

                if (cboEquipmentSegment.SelectedValue != null)
                    sEqsgID = cboEquipmentSegment.SelectedValue.ToString();
                else
                    sEqsgID = "";

                if (cboEquipment.SelectedValue != null)
                    sEqptID = cboEquipment.SelectedValue.ToString();
                else
                    sEqptID = "";

                if (winWorkOrder != null)
                {
                    winWorkOrder.EQPTSEGMENT = sEqsgID;
                    winWorkOrder.EQPTID = sEqptID;
                    winWorkOrder.PROCID = Process.STACKING_FOLDING;

                    winWorkOrder.ClearWorkOrderInfo();
                }

                if (winInput != null)
                {
                    winInput.ChangeEquipment(sEqsgID, sEqptID);
                }

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

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));

                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                #region EDC BCR Reading Info
                if (winInput != null)
                {
                    winInput.EQPTSEGMENT = Util.NVC(cboEquipmentSegment.SelectedValue);

                    if (!(Util.NVC(cboEquipmentSegment.SelectedValue).Equals("SELECT") || Util.NVC(cboEquipmentSegment.SelectedValue).Equals("")))
                        winInput.CheckEDCVisibity();
                }
                #endregion

                if (cboEquipmentSegment.SelectedIndex > 0 && cboEquipmentSegment.Items.Count > cboEquipmentSegment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipmentSegment.Items[cboEquipmentSegment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        GetLotIdentBasCode();

                        // Lot 식별 기준 코드에 따른 컨트롤 변경.
                        SetInOutCtrlByLotIdentBasCode();

                        SetUserCheckFlag();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    CreateOutProdWithOutMsg();
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

            CMM_ASSY_EQPT_COND wndEqptCond = new CMM_ASSY_EQPT_COND();
            wndEqptCond.FrameOperation = FrameOperation;

            if (wndEqptCond != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                C1WindowExtension.SetParameters(wndEqptCond, Parameters);

                wndEqptCond.Closed += new EventHandler(wndEqptCond_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
                grdMain.Children.Add(wndEqptCond);
                wndEqptCond.BringToFront();
            }
        }

        private void btnCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  // 라인을 선택 하세요.
                return;
            }

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                CMM_ASSY_CANCEL_TERM_CST wndCancelTerm = new CMM_ASSY_CANCEL_TERM_CST();
                wndCancelTerm.FrameOperation = FrameOperation;

                if (wndCancelTerm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Process.STACKING_FOLDING;

                    C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                    wndCancelTerm.Closed += new EventHandler(wndCancelTermCST_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
                    grdMain.Children.Add(wndCancelTerm);
                    wndCancelTerm.BringToFront();
                }
            }
            else
            {
                CMM_ASSY_CANCEL_TERM wndCancelTerm = new CMM_ASSY_CANCEL_TERM();
                wndCancelTerm.FrameOperation = FrameOperation;

                if (wndCancelTerm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Process.STACKING_FOLDING;

                    C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                    wndCancelTerm.Closed += new EventHandler(wndCancelTerm_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
                    grdMain.Children.Add(wndCancelTerm);
                    wndCancelTerm.BringToFront();
                }
            }
        }

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDefectInfo())
                return;

            //CMM_ASSY_DEFECT wndDefect = new CMM_ASSY_DEFECT();
            CMM_ASSY_DEFECT_FOLDING wndDefect = new CMM_ASSY_DEFECT_FOLDING();
            wndDefect.FrameOperation = FrameOperation;

            if (wndDefect != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = "N";
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));

                C1WindowExtension.SetParameters(wndDefect, Parameters);

                wndDefect.Closed += new EventHandler(wndDefect_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndDefect.ShowModal()));
                grdMain.Children.Add(wndDefect);
                wndDefect.BringToFront();
            }
        }

        private void dgOutProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                // Grid Data Binding 이용한 Background 색 변경
                // 프린트에 Y/N 따른 색변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {


                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));

                        // INS 염규범S
                        // Dispath에 N에 따른 색 변경 
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DISPATCH_YN")).Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                            // 2020-10-31 C20200806-000180
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        // 2020-10-31 C20200806-000180
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DISPATCH_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                }
            }));
        }

        private void dgOutProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.STACKING_FOLDING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (btnOutPrint != null)
                btnOutPrint.IsEnabled = true;

            //if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            //    btnOutPrint_Org.Visibility = Visibility.Visible;
        }

        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTestMode()) return;

            if (bTestMode)
            {
                if (SetTestMode(false))
                    HideTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (SetTestMode(true))
                            ShowTestMode();
                    }
                });
            }
        }

        private void btnQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            CMM_ASSY_QUALITY_PKG wndPopup = new CMM_ASSY_QUALITY_PKG();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.STACKING_FOLDING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndQualityRslt_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                //foreach (System.Windows.UIElement child in grdMain.Children)
                //{
                //    if (child.GetType() == typeof(CMM_ASSY_QUALITY_PKG))
                //    {
                //        grdMain.Children.Remove(child);
                //        break;
                //    }
                //}

                //grdMain.Children.Add(wndPopup);
                //wndPopup.BringToFront();
            }
        }

        private void btnEqptCondSearch_Click(object sender, RoutedEventArgs e)
        {

            if (!CanSearch())
                return;

            CMM_ASSY_EQPT_COND_SEARCH wndEqptCondSearch = new CMM_ASSY_EQPT_COND_SEARCH();
            wndEqptCondSearch.FrameOperation = FrameOperation;

            if (wndEqptCondSearch != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.STACKING_FOLDING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndEqptCondSearch, Parameters);

                wndEqptCondSearch.Closed += new EventHandler(wndEqptCondSearch_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCondSearch.ShowModal()));
            }
        }

        private void wndEqptCondSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND_SEARCH window = sender as CMM_ASSY_EQPT_COND_SEARCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;

            CMM_COM_QUALITY wndQualityInput = new CMM_COM_QUALITY();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Process.STACKING_FOLDING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInput_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }
        #endregion

        #region [작업대상]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

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
                dgProductLot.SelectedIndex = idx;

                // 상세 정보 조회
                ProdListClickedProcess(idx);
            }
        }
        #endregion

        #region [투입]

        #endregion

        #region [생산]

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this._BOX_CREATE_PROCESSING = true;

                if (!CanCreateBox())
                    return;

                if (CheckFirstOutProduction())
                {
                    //"생산 수량 %1 개로 생성 하시겠습니까?"
                    Util.MessageConfirm("SFU4888", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CreateOutBox(GetNewOutLotid());
                        }
                        else
                        {
                            this._BOX_CREATE_PROCESSING = false;
                        }
                    }, txtOutBoxQty.Text);
                }
                else
                {
                    //"생성 하시겠습니까?"
                    Util.MessageConfirm("SFU1621", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CreateOutBox(GetNewOutLotid());
                        }
                        else
                        {
                            this._BOX_CREATE_PROCESSING = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            string sPrintMessage = "SFU1237";

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y"))
                {
                    sPrintMessage = "SFU8263";
                    break;
                }
            }


            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm(sPrintMessage, (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            try
                            {
                                btnOutPrint.IsEnabled = false;

                                DataTable dtTmp = null;

                                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                                {
                                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;


                                    DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));

                                    if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                                    if (!dtRslt.Columns.Contains("DISPATCH_YN"))
                                    {
                                        DataColumn dcTmp = new DataColumn("DISPATCH_YN", typeof(string));
                                        dcTmp.DefaultValue = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "DISPATCH_YN")).Equals("Y") ? "Y" : "N";
                                        dtRslt.Columns.Add(dcTmp);
                                    }

                                    if (!dtRslt.Columns.Contains("RE_PRT_YN"))
                                    {
                                        DataColumn dcTmp = new DataColumn("RE_PRT_YN", typeof(string));
                                        dcTmp.DefaultValue = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N";
                                        dtRslt.Columns.Add(dcTmp);
                                    }

                                    if (dtTmp == null)
                                        dtTmp = dtRslt.Copy();
                                    else
                                        dtTmp.Merge(dtRslt);
                                }

                                if (dtTmp == null) return;

                                using (ThermalPrint thmrPrt = new ThermalPrint())
                                {
                                    THERMAL_PRT_TYPE type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD;
                                    if (LoginInfo.CFG_SHOP_ID.Equals("G382"))
                                    {
                                        type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD;
                                    }

                                    thmrPrt.Print(sEqsgID: Util.NVC(cboEquipmentSegment.SelectedValue),
                                                  sEqptID: Util.NVC(cboEquipment.SelectedValue),
                                                  sProcID: Process.STACKING_FOLDING,
                                                  inData: dtTmp,
                                                  iType: type,
                                                  iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                                  bSavePrtHist: true,
                                                  bDispatch: true);

                                    GetOutProduct();

                                    if (chkAll != null && chkAll.IsChecked.HasValue)
                                        chkAll.IsChecked = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                btnOutPrint.IsEnabled = true;
                            }
                        }
                        else
                            BoxIDPrint();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();

                    btnOutPrint.IsEnabled = true;
                }
            });
        }

        private void btnTESTPrint_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_006_TEST_PRINT wndTestPrint = new ASSY001_006_TEST_PRINT();
            wndTestPrint.FrameOperation = FrameOperation;

            wndTestPrint.Closed += new EventHandler(wndTestPrint_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //wndTestPrint.ShowModal();
            grdMain.Children.Add(wndTestPrint);
            wndTestPrint.BringToFront();
        }

        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeleteBox())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("삭제 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        DeleteOutBox();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }

        private void txtOutBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutBoxQty.Text, 0))
                {
                    txtOutBoxQty.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutBox())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            Util.MessageConfirm("SFU1241", (result) =>
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        SaveProcOutBox();
                        SaveEndOutBox();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }

        private void btnOutRework_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }


            ASSY001_005_REWORK wndRework = new ASSY001_005_REWORK();
            wndRework.FrameOperation = FrameOperation;

            if (wndRework != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndRework, Parameters);

                wndRework.Closed += new EventHandler(wndRework_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //wndRework.ShowModal();
                grdMain.Children.Add(wndRework);
                wndRework.BringToFront();
            }
        }

        private void CreateOutProdWithOutMsg()
        {
            if (!CanCreateBox())
                return;

            if (CheckFirstOutProduction())
            {
                //"생산 수량 %1 개로 생성 하시겠습니까?"
                Util.MessageConfirm("SFU4888", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateOutBox(GetNewOutLotid());
                    }
                }, txtOutBoxQty.Text);
            }
            else
            {
                CreateOutBox(GetNewOutLotid());
            }
        }

        #endregion

        #region [작업대상 변경]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            ASSY001_005_CHANGE wndChange = new ASSY001_005_CHANGE();
            wndChange.FrameOperation = FrameOperation;

            if (wndChange != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIP_WRK_TYPE_CODE"));

                C1WindowExtension.SetParameters(wndChange, Parameters);

                wndChange.Closed += new EventHandler(wndChange_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                grdMain.Children.Add(wndChange);
                wndChange.BringToFront();
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        public void GetProductLot(bool bSelPrv = true, string sNewLot = "")
        {
            try
            {
                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                {
                    HiddenLoadingIndicator();
                    return;
                }

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                    // 착공 후 조회 시..
                    if (sNewLot != null && !sNewLot.Equals(""))
                        sPrvLot = sNewLot;
                }

                ClearControls();

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIPINFO_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                //newRow["WOID"] = Util.NVC(drWorkOrderInfo["WOID"]);
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                //newRow["WIPSTAT"] = "WAIT,PROC,END";
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_FD", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgProductLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;

                                ProdListClickedProcess(idx);
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                GetWaitMagazineList();
                            }
                        }
                        else
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                int iRowRun = _Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (dgProductLot.TopRows.Count > 0)
                                        iRowRun = dgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;

                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;

                                    ProdListClickedProcess(iRowRun);
                                }
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                GetWaitMagazineList();
                            }
                        }

                        // 2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
                        GetLossCnt();

                        if (txtOutCa.Visibility == Visibility.Visible)
                            txtOutCa.Focus();
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

        private void GetOutProduct()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_BOX_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutProduct, searchResult, FrameOperation, false);

                if (!_BOX_CREATE_PROCESSING)
                {

                    if (dgOutProduct.GetRowCount() > 0)
                    {

                        DataTable tmpdt = searchResult.Select("WIPSTAT <> 'PROC'").Count<DataRow>() > 0 ? searchResult.Select("WIPSTAT <> 'PROC'").CopyToDataTable() : null;
                        //txtOutBoxQty.Text = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[0].DataItem, "WIPQTY"));
                        //double dQty = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[0].DataItem, "WIPQTY")));

                        double dQty = (tmpdt != null && tmpdt.Rows.Count > 0) ? Convert.ToDouble(Convert.ToDecimal(tmpdt.Rows[0]["WIPQTY"].ToString())) : 0.0d;

                        txtOutBoxQty.Text = Convert.ToString(dQty);
                    }
                    else
                    {
                        txtOutBoxQty.Text = "";
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

        private string GetNewOutLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_NEW_OUT_LOT_FD();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                //newRow["WIP_TYPE_CODE"] = INOUT_TYPE.OUT;
                //newRow["CALDATE_YMD"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_OUT_LOTID_FD", "INDATA", "OUTDATA", inTable);

                string sNewLot = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dtResult.Rows[0]["OUT_LOTID"]);
                }
                HiddenLoadingIndicator();
                return sNewLot;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return "";
            }
        }

        /// <summary>
        /// 2017.06.28 Add by Kim Joonphil
        /// CST 사용 가능 확인 (폴란드에서 사용)
        /// </summary>
        /// <returns></returns>
        private bool CheckedUseCassette()
        {
            try
            {
                DataSet IndataSet = new DataSet();

                DataTable dtIN_EQP = IndataSet.Tables.Add("IN_EQP");
                dtIN_EQP.Columns.Add("SRCTYPE", typeof(string));
                dtIN_EQP.Columns.Add("IFMODE", typeof(string));
                dtIN_EQP.Columns.Add("CSTID", typeof(string));
                dtIN_EQP.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow dr = dtIN_EQP.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["CSTID"] = Util.NVC(txtOutCa.Text.Trim());
                dr["WIP_TYPE_CODE"] = "OUT";
                dtIN_EQP.Rows.Add(dr);


                DataTable dtIN_INPUT = IndataSet.Tables.Add("IN_INPUT");
                dtIN_INPUT.Columns.Add("LANGID", typeof(string));
                dtIN_INPUT.Columns.Add("PROCID", typeof(string));
                dtIN_INPUT.Columns.Add("EQSGID", typeof(string));

                dr = dtIN_INPUT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.STACKING_FOLDING;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                dtIN_INPUT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CST_MAPPING_DUP", "IN_EQP,IN_INPUT", null, IndataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        private void CreateOutBox(string sNewOutLot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("IFMODE", typeof(String));
                RQSTDT.Columns.Add("EQPTID", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));
                RQSTDT.Columns.Add("PROD_LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                dr["USERID"] = _LABEL_PRT_RESTRCT_FLAG == "Y" ? txtUserNameCr.Tag.ToString() : LoginInfo.USERID;
                dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_CHK_LOT_LABEL_PRT_RESTRCT", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException, (msgResult) =>
                            {
                                if (txtOutCa.Visibility == Visibility.Visible)
                                {
                                    txtOutCa.Text = "";
                                    txtOutCa.Focus();
                                }
                            });

                            return;
                        }

                        if (sNewOutLot.Equals(""))
                            return;

                        ShowLoadingIndicator();

                        DataTable inTable = _Biz.GetBR_PRD_REG_CREATE_BOX_FD();

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                        newRow["OUT_LOTID"] = sNewOutLot;
                        newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        newRow["WO_DETL_ID"] = null;
                        newRow["INPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                        newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                        newRow["RESNQTY"] = 0;
                        newRow["SHIFT"] = null;
                        newRow["WIPNOTE"] = "";
                        newRow["WRK_USER_NAME"] = "";
                        //newRow["USERID"] = LoginInfo.USERID;
                        newRow["USERID"] = _LABEL_PRT_RESTRCT_FLAG == "Y" ? txtUserNameCr.Tag.ToString() : LoginInfo.USERID;
                        newRow["CSTID"] = Util.NVC(txtOutCa.Text.Trim());

                        inTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_OUT_LOT_FD", "INDATA", null, inTable, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    //if (GetMessageCode(searchException).Equals("1036")) // Carrier[XXXX]는 이미 LOT[yyyy]가 할당되어 있습니다.
                                    //{
                                    //    // Carrier가 이미 할당되어 있습니다. 초기화 후 자동 생성 하시겠습니까?
                                    //    Util.MessageConfirm("SFU4933", (result) =>
                                    //    {
                                    //        if (result == MessageBoxResult.OK)
                                    //        {
                                    //            // Carrier Empty 처리.
                                    //            CarrierEmpty();

                                    //            CreateOutBox(GetNewOutLotid());
                                    //        }
                                    //        else
                                    //        {
                                    //            if (txtOutCa.Visibility == Visibility.Visible)
                                    //            {
                                    //                txtOutCa.Text = "";
                                    //                txtOutCa.Focus();
                                    //            }
                                    //        }
                                    //    });                                        
                                    //}
                                    //else
                                    //{
                                    if (GetMessageCode(searchException).Equals("1036")) // Carrier[XXXX]는 이미 LOT[yyyy]가 할당되어 있습니다.
                                    {
                                        if (txtOutCa.Visibility == Visibility.Visible)
                                        {
                                            txtOutCa.Text = "";
                                            txtOutCa.Focus();
                                        }

                                        Util.MessageInfoAutoClosing(searchException.Message.ToString()); // 메시지 출력 후 바로 close
                                        //Util.MessageException(searchException);
                                    }
                                    else
                                    {
                                        Util.MessageException(searchException, (msgResult) =>
                                        {
                                            if (txtOutCa.Visibility == Visibility.Visible)
                                            {
                                                txtOutCa.Text = "";
                                                txtOutCa.Focus();
                                            }
                                        });
                                    }


                                    return;
                                }

                                #region ** 2020.05.18 FOL 고정식 바코드 사용일 경우 BOX 라벨자동발행 Blocking 
                                if (_Util.IsCommonCodeUse("FOL_AUTO_LABEL_PRNT_EXCEPT_PLANT", LoginInfo.CFG_SHOP_ID))
                                {
                                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                                    {
                                        chkAutoPrint.IsChecked = false;
                                    }
                                }
                                else
                                {
                                    chkAutoPrint.IsChecked = true;
                                }
                                #endregion


                                if (chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                                {
                                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                                    {
                                        try
                                        {
                                            btnOutPrint.IsEnabled = false;

                                            DataTable dtRslt = GetThermalPaperPrintingInfo(sNewOutLot);

                                            if (!dtRslt.Columns.Contains("DISPATCH_YN"))
                                            {
                                                DataColumn dcTmp = new DataColumn("DISPATCH_YN", typeof(string));
                                                dcTmp.DefaultValue = "N";
                                                dtRslt.Columns.Add(dcTmp);
                                            }

                                            if (!dtRslt.Columns.Contains("RE_PRT_YN"))
                                            {
                                                DataColumn dcTmp = new DataColumn("RE_PRT_YN", typeof(string));
                                                dcTmp.DefaultValue = "N";
                                                dtRslt.Columns.Add(dcTmp);
                                            }

                                            using (ThermalPrint thmrPrt = new ThermalPrint())
                                            {
                                                THERMAL_PRT_TYPE type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD;
                                                if (LoginInfo.CFG_SHOP_ID.Equals("G382"))
                                                {
                                                    type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD;
                                                }

                                                thmrPrt.Print(sEqsgID: Util.NVC(cboEquipmentSegment.SelectedValue),
                                                              sEqptID: Util.NVC(cboEquipment.SelectedValue),
                                                              sProcID: Process.STACKING_FOLDING,
                                                              inData: dtRslt,
                                                              iType: type,
                                                              iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                                              bSavePrtHist: true,
                                                              bDispatch: true);

                                                GetOutProduct();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Util.MessageException(ex);
                                        }
                                        finally
                                        {
                                            btnOutPrint.IsEnabled = true;
                                        }
                                    }
                                    else
                                        BoxIDPrint(sNewOutLot, Convert.ToDecimal(txtOutBoxQty.Text));
                                }

                                GetWorkOrder(); // 작지 생산수량 정보 재조회.
                                GetProductLot();

                                int idx = _Util.GetDataGridRowIndex(dgOutProduct, "LOTID", sNewOutLot);
                                if (idx >= 0)
                                    DataTableConverter.SetValue(dgOutProduct.Rows[idx].DataItem, "CHK", true);

                                Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.
                                txtOutCa.Text = "";

                                if (txtOutCa.Visibility == Visibility.Visible)
                                    txtOutCa.Focus();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                this._BOX_CREATE_PROCESSING = false;
                                HiddenLoadingIndicator();
                            }
                        }
                        );

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_BOX_ST();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));

                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_FD", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteProcOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_BOX_ST();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                    {
                        newRow = input_LOT.NewRow();
                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));

                        input_LOT.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_OUT_LOT_FD", "INDATA,IN_INPUT", "OUT_LOT", indataSet);
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

        private void SetDispatch(string sBoxID = "", decimal dQty = 0)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DISPATCH_LOT_FD();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["REWORK"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDataTable = indataSet.Tables["INLOT"];

                if (sBoxID.Equals(""))
                {
                    for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;
                        newRow = inDataTable.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                        newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));
                        newRow["ACTUQTY"] = 0;
                        newRow["WIPNOTE"] = "";

                        inDataTable.Rows.Add(newRow);
                    }
                }
                else
                {
                    newRow = inDataTable.NewRow();
                    newRow["LOTID"] = sBoxID;
                    newRow["ACTQTY"] = dQty;
                    newRow["ACTUQTY"] = 0;
                    newRow["WIPNOTE"] = "";

                    inDataTable.Rows.Add(newRow);
                }


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
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

        private DataTable GetThermalPaperPrintingInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));

                    inTable.Rows.Add(newRow);
                }

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
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

        private void SaveProcOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_DATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable outLotTable = indataSet.Tables.Add("IN_OUTLOT");
                outLotTable.Columns.Add("OUT_LOTID", typeof(string));
                outLotTable.Columns.Add("OUTPUT_QTY", typeof(int));

                DataRow newRow = inTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                    {
                        newRow = outLotTable.NewRow();

                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                        newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));
                        //newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));

                        outLotTable.Rows.Add(newRow);
                    }
                }

                if (outLotTable.Rows.Count < 1)
                    return;

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_QTY", "IN_DATA,IN_OUTLOT", null, indataSet);

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

        private void SaveEndOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_MODIFY_LOT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSTAT")).Equals("END"))
                    {
                        newRow = outLotTable.NewRow();

                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                        newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")));
                        newRow["WIPQTY_ED"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));
                        newRow["BONUSQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY")));

                        outLotTable.Rows.Add(newRow);
                    }
                }

                if (outLotTable.Rows.Count < 1)
                {
                    GetOutProduct();
                    GetWorkOrder(); // 작지 생산수량 정보 재조회.
                    GetProductLot();
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_LOT_WIPQTY", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            GetOutProduct();
                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();
                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool GetErpSendInfo(string sLotID, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                bool bRet = false;
                DataTable inTable = _Biz.GetDA_PRD_SEL_ERP_SEND_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    // 'S' 가 아닌 경우는 삭제 가능.
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // S : ERP 전송 중 , P : ERP 전송 대기, Y : ERP 전송 완료
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

        private int GetNotDispatchCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_DISPATCH_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private int GetNotDispatchCntWithOutProc()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_DISPATCH_CNT_WITHOUT_PROC", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
            finally
            {
                HiddenLoadingIndicator();
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
                newRow["PROCID"] = Process.STACKING_FOLDING;

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
                Indata["PROCID"] = Process.STACKING_FOLDING;

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

        private bool SetTestMode(bool bTestMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_EQP_REG_TESTMODE();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = bTestMode ? "TEST" : "ON";
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_EQP_REG_EQPT_OPMODE", "IN_EQP", null, inTable);

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
                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        HideTestMode();
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

        public void OnCurrInDelete(DataTable inMtrl)
        {
            try
            {

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_COMPLETE_LM();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in inMtrl.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);


                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_INPUT_IN_LOT_FD", "INDATA,IN_INPUT", null, indataSet);

                inInputTable.Clear();

                GetProductLot();

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void OnCurrInCancel(DataTable inMtrl)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (iRow < 0)
                    return;

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_FD();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")); // biz에서 찾음.

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in inMtrl.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);

                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, indataSet);

                    inInputTable.Clear();
                }

                GetProductLot();

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void OnCurrInComplete(DataTable inMtrl)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_COMPLETE_LM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                foreach (DataRow sourcerow in inMtrl.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public bool OnCurrAutoInputLot(string sInputLot, string sPstnID)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return false;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_MTRL_INPUT_FD();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = sInputLot;

                inInputTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_INPUT_IN_LOT_FD", "INDATA,IN_INPUT", null, indataSet);

                HiddenLoadingIndicator();

                GetProductLot();
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
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

        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.STACKING_FOLDING;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
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

        private DataTable GetGroupPrintInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                string sTmp = string.Empty;

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    if (sTmp.Length < 1)
                        sTmp = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    else
                        sTmp = sTmp + "," + Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                }

                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sTmp;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_GRP_PRT_INFO_FD", "INDATA", "OUTDATA", inTable);
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

        private bool CheckFirstOutProduction()
        {
            try
            {
                bool bRet = false;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT_BY_PR_LOTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("OUT_PROD_CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["OUT_PROD_CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt < 1)
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

        private void CarrierEmpty()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["CSTID"] = Util.NVC(txtOutCa.Text);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_EMPTY_RFID_BY_CSTID", "INDATA", null, inTable);

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

        private int GetOutTermCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_TERM_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["TERM_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["TERM_CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CheckManagerAuth()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "ASSYAU_MANA,MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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

            bRet = true;
            return bRet;
        }

        private bool CanStartRun()
        {
            bool bRet = false;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            for (int i = 0; i < dgProductLot.Rows.Count - dgProductLot.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    //Util.Alert("작업중인 LOT이 존재 합니다.");
                    Util.MessageValidation("SFU1847");
                    return bRet;
                }
            }

            if (!CheckSelWOInfo())
            {
                //Util.Alert("선택된 W/O가 없습니다.");
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
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("장비완료 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU1866");
                return bRet;
            }

            //if (dgInMagazine.Rows.Count > 0)
            //{
            //    for (int i = 0; i < dgInMagazine.Rows.Count - dgInMagazine.BottomRows.Count; i++)
            //    {
            //        if (!Util.NVC(DataTableConverter.GetValue(dgInMagazine.Rows[i].DataItem, "WIPSTAT")).Equals("TERM"))
            //        {
            //            Util.Alert("투입된 매거진을 모두 완료 후 실행 하세요.");
            //            return bRet;
            //        }
            //    }
            //}
            bRet = true;
            return bRet;
        }

        private bool CanConfirm()
        {
            bool bRet = false;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return bRet;
            }

            // 자동 재공종료 존재 시 확정 불가 처리.
            if (GetOutTermCnt() > 0)
            {
                // 재공종료 상태인 생산반제품이 존재 합니다. 삭제 후 진행 하세요.
                Util.MessageValidation("SUF4962");
                return bRet;
            }

            // 진행중인 lot 존재 시 확정 가능 여부 공통코드 체크.
            if (ChkProcBoxChkArea())
            {
                // DISPATCH 여부 체크. (진행중 제외)
                if (GetNotDispatchCntWithOutProc() > 0)
                {
                    // 완공된 LOT중 발행처리 하지 않은 LOT이 존재 합니다.
                    Util.MessageInfo("SFU3770");
                    return bRet;
                }
            }
            else
            {
                // DISPATCH 여부 체크.            
                if (GetNotDispatchCnt() > 0)
                {
                    //Util.Alert("발행처리 하지 않은 생산 반제품이 존재 합니다.");
                    Util.MessageInfo("SFU1558");
                    return bRet;
                }
            }

            // 로직 팝업으로 이동..
            //// Short 불량 Check...
            ////int iSumBoxCnt = int.Parse(DataTableConverter.GetValue(dgOutProduct.Rows[dgOutProduct.Rows.Count - 1].DataItem, "OUTQTY").ToString());  // 바구니수량 합계
            //int iSumBoxCnt = int.Parse(DataTableConverter.Convert(dgOutProduct.ItemsSource).Compute("SUM(OUTQTY)", String.Empty).ToString());   // 바구니수량 합계
            //int iShortCnt = GetShortDefectCount();

            //double dShortRate = double.Parse(iShortCnt.ToString()) / double.Parse(iSumBoxCnt.ToString());   // Short 불량율
            //double dMaxShortRate = GetShorDefectRate();    // 시스템에 등록된 최대 범위 불량율

            //if (dShortRate > dMaxShortRate)
            //{
            //    string sMsg = "쇼트불량률이 범위를 초과하였습니다!  \r\n\r\n" + "수량합계 : " + iSumBoxCnt.ToString() + "개 | 불량쇼트 : " + iShortCnt.ToString() + "개 | 불량률 : "
            //                 + (dShortRate * 100).ToString("###.#") + "% \r\n\r\n"
            //                 + "쇼트불량 개수를 수정하시고 다시 진행하십시요!   \r\n\r\n" + "수정할려면 [취소] / 무시하고 계속진행은 [확인]";

            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //    {
            //        if (result == MessageBoxResult.Cancel)
            //        {
            //            bRet = false;
            //        }
            //    });
            //}

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            {
                DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();

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

            bRet = true;
            return bRet;
        }

        private bool CanQuality()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanDefectInfo()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanEqpRemark()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanCreateBox()
        {
            bool bRet = false;
            string returnMsg = string.Empty;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtOutBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if ((Convert.ToDecimal(txtOutBoxQty.Text) % 1) > 0)
            {
                //Util.Alert("소수점 입력은 불가능 합니다. 수량을 확인해 주세요.");
                Util.MessageValidation("SFU2342");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if (!CheckedUseCassette())  //Cassette 중복여부 체크. 2017.06.28 Add by Kim Joonphil
            {
                txtOutCa.SelectAll();
                return bRet;
            }
            //if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            //{
            //    Util.Alert("장비완료된 LOT에는 생성 할 수 없습니다.");
            //    return bRet;
            //}

            if (_LABEL_PRT_RESTRCT_FLAG == "Y")
            {
                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserNameCr.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    txtUserNameCr.Focus();
                    return bRet;
                }
            }

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                //카세트ID 10자리 확인 [2018.03.14]
                if (txtOutCa.Text.Length != 10)
                {
                    Util.MessageValidation("SFU4571");
                    txtOutCa.SelectAll();
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanPrint()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    // 진행중인 Lot[%1]은 발행할 수 없습니다. 
                    Util.MessageValidation("SFU3769", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private bool CanDeleteBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private bool CanSaveOutBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }

                // 양품수량(전체수량 - 재작업수량) 이 음수 여부 체크
                double dBonusQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDouble(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY"));
                if ((dTmp - dBonusQty) < 0)
                {
                    Util.MessageValidation("SFU1721");  // 양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
                    return bRet;
                }
            }

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
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
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

            bRet = true;
            return bRet;
        }
        #endregion

        #region [PopUp Event]

        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            ASSY001_005_RUNSTART window = sender as ASSY001_005_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true, window.NEW_PROD_LOT);
            }

            this.grdMain.Children.Remove(window);
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY001_005_CONFIRM window = sender as ASSY001_005_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }

            GetEqptWrkInfo();

            this.grdMain.Children.Remove(window);
        }

        private void wndWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY001_005_WAITLOT window = sender as ASSY001_005_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY window = sender as CMM_ASSY_QUALITY;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            ASSY001_005_MAGAZINE_CREATE window = sender as ASSY001_005_MAGAZINE_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndReplace_Closed(object sender, EventArgs e)
        {
            ASSY001_004_PAN_REPLACE window = sender as ASSY001_004_PAN_REPLACE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndDefect_Closed(object sender, EventArgs e)
        {
            //CMM_ASSY_DEFECT window = sender as CMM_ASSY_DEFECT;
            CMM_ASSY_DEFECT_FOLDING window = sender as CMM_ASSY_DEFECT_FOLDING;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetProductLot();

            this.grdMain.Children.Remove(window);
        }

        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND window = sender as CMM_ASSY_EQPT_COND;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndCancelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndCancelTermCST_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM_CST window = sender as CMM_ASSY_CANCEL_TERM_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
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

            this.grdMain.Children.Remove(wndPopup);
        }

        private void wndEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPEND window = sender as CMM_ASSY_EQPEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }

            this.grdMain.Children.Remove(window);
        }

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetOutProduct();

            if (chkAll != null && chkAll.IsChecked.HasValue)
                chkAll.IsChecked = false;
        }

        private void wndTestPrint_Closed(object sender, EventArgs e)
        {
            ASSY001_006_TEST_PRINT window = sender as ASSY001_006_TEST_PRINT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }

        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_COM_QUALITY window = sender as CMM_COM_QUALITY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndQualityInput_New_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP window = sender as CMM_COM_SELF_INSP;

            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndRework_Closed(object sender, EventArgs e)
        {
            ASSY001_005_REWORK window = sender as ASSY001_005_REWORK;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!window.OUT_LOT_ID.Equals(""))
                {
                    if (chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                    {
                        if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            try
                            {
                                btnOutPrint.IsEnabled = false;

                                DataTable dtRslt = GetThermalPaperPrintingInfo(window.OUT_LOT_ID);

                                if (!dtRslt.Columns.Contains("DISPATCH_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("DISPATCH_YN", typeof(string));
                                    dcTmp.DefaultValue = "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (!dtRslt.Columns.Contains("RE_PRT_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("RE_PRT_YN", typeof(string));
                                    dcTmp.DefaultValue = "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                using (ThermalPrint thmrPrt = new ThermalPrint())
                                {
                                    THERMAL_PRT_TYPE type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD;
                                    if (LoginInfo.CFG_SHOP_ID.Equals("G382"))
                                    {
                                        type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD;
                                    }

                                    thmrPrt.Print(sEqsgID: Util.NVC(cboEquipmentSegment.SelectedValue),
                                                  sEqptID: Util.NVC(cboEquipment.SelectedValue),
                                                  sProcID: Process.STACKING_FOLDING,
                                                  inData: dtRslt,
                                                  iType: type,
                                                  iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                                  bSavePrtHist: true,
                                                  bDispatch: true);

                                    GetOutProduct();
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                btnOutPrint.IsEnabled = true;
                            }
                        }
                        else
                            BoxIDPrint(window.OUT_LOT_ID, window.OUT_QTY);
                    }
                }

                GetOutProduct();
            }

            this.grdMain.Children.Remove(window);
        }

        private void wndChange_Closed(object sender, EventArgs e)
        {
            ASSY001_005_CHANGE window = sender as ASSY001_005_CHANGE;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }

            GetOutProduct();

            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [Func]

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreateBOX);
            listAuth.Add(btnDeleteBOX);
            listAuth.Add(btnOutSave);
            listAuth.Add(btnOutPrint);

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

        private void SetInputWindow()
        {
            if (grdInput.Children.Count == 0)
            {
                string sEqsg = "";
                string sEqpt = "";
                if (cboEquipmentSegment != null && cboEquipmentSegment.SelectedValue != null)
                    sEqsg = cboEquipmentSegment.SelectedValue.ToString();
                if (cboEquipment != null && cboEquipment.SelectedValue != null)
                    sEqpt = cboEquipment.SelectedValue.ToString();

                if (winInput == null)
                    winInput = new UC_IN_OUTPUT(Process.STACKING_FOLDING, sEqsg, sEqpt);

                winInput.FrameOperation = FrameOperation;

                winInput._UCParent = this;
                grdInput.Children.Add(winInput);

                // Lot 식별 기준 코드에 따른 컨트롤 변경.
                SetInOutCtrlByLotIdentBasCode();
            }
        }

        private void SetUserCheckFlag()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();
                dr["PROCID"] = Process.STACKING_FOLDING;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_LABEL_FLAG", "RQSTDT", "RSLDT", dtRQSTDT);

                if (dtRslt.Rows[0]["LABEL_PRT_RESTRCT_FLAG"].ToString().Trim().ToUpper().Equals("Y"))
                {
                    //grdCreater.Visibility = Visibility.Visible;
                    txtCreater.Visibility = Visibility.Visible;
                    txtUserNameCr.Visibility = Visibility.Visible;
                    btnUserCr.Visibility = Visibility.Visible;

                    _LABEL_PRT_RESTRCT_FLAG = "Y";
                }
                else
                {
                    //grdCreater.Visibility = Visibility.Collapsed;
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

        public void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.STACKING_FOLDING;

            winWorkOrder.GetWorkOrder();
        }

        private void GetInputAllInfo(int iRow)
        {
            if (winInput == null)
                return;

            if (iRow < 0 || iRow > dgProductLot.Rows.Count)
                return;

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROD_LOTID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
            winInput.PROD_WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
            winInput.PROD_WOID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WOID"));
            winInput.PROD_WODTLID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WO_DETL_ID"));

            winInput.SearchAll();
        }

        private void GetCurrInputLotList()
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                winInput.PROD_LOTID = "";
                winInput.PROD_WIPSEQ = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
            }

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROCID = Process.STACKING_FOLDING;

            winInput.GetCurrInList();
        }

        private void GetWaitMagazineList()
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                winInput.PROD_LOTID = "";
                winInput.PROD_WIPSEQ = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
            }

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROCID = Process.STACKING_FOLDING;

            winInput.GetWaitMagazine();
        }
        //설비불량로스 
        private void GetEqptLoss(int iRow)
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                winInput.EQPTSEGMENT = "";
                winInput.EQPTID = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
            }
            winInput.PROD_LOTID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
            winInput.PROD_WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
            winInput.PROCID = Process.STACKING_FOLDING;

            winInput.GetEqptLoss();
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.STACKING_FOLDING;
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
            winWorkOrder.PROCID = Process.STACKING_FOLDING;

            return winWorkOrder.GetSelectWorkOrderRow();
        }

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailControls();

                if (winInput != null)
                {
                    winInput.ClearDataGrid();
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private void ClearDetailControls()
        {
            Util.gridClear(dgOutProduct);
            txtOutBoxQty.Text = "";
        }

        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            ClearDetailControls();

            if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            GetInputAllInfo(iRow);
            GetEqptLoss(iRow);
            GetOutProduct();
        }

        private void BoxIDPrint(string sBoxID = "", decimal dQty = 0)
        {

            try
            {
                int iCopys = 2;


                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                btnOutPrint.IsEnabled = false;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                if (!sBoxID.Equals(""))
                {
                    // 발행..
                    DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);


                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //폴딩
                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    dicList.Add(dicParam);


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = null;
                        Parameters[1] = Process.STACKING_FOLDING;
                        Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[3] = cboEquipment.SelectedValue.ToString();
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();

                    }
                }
                else
                {
                    for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;


                        DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));

                        if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                        Dictionary<string, string> dicParam = new Dictionary<string, string>();

                        //폴딩
                        dicParam.Add("reportName", "Fold");
                        dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                        dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                        dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                        dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                        dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                        dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                        dicParam.Add("TITLEX", "BASKET ID");

                        dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                        dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

                        dicList.Add(dicParam);

                    }


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD();
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.STACKING_FOLDING;
                        Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[3] = cboEquipment.SelectedValue.ToString();
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
                }

                //LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD();
                //print.FrameOperation = FrameOperation;

                //if (print != null)
                //{
                //    object[] Parameters = new object[6];
                //    Parameters[0] = dicList;
                //    Parameters[1] = Process.STACKING_FOLDING;
                //    Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                //    Parameters[3] = cboEquipment.SelectedValue.ToString();
                //    Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                //    Parameters[5] = "Y";   // 디스패치 처리.

                //    C1WindowExtension.SetParameters(print, Parameters);

                //    print.Closed += new EventHandler(print_Closed);

                //    print.Show();
                //}

                //LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_NEW print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_NEW();
                //print.FrameOperation = FrameOperation;

                //if (print != null)
                //{
                //    object[] Parameters = new object[6];
                //    Parameters[0] = dicList;
                //    Parameters[1] = Process.STACKING_FOLDING;
                //    Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                //    Parameters[3] = cboEquipment.SelectedValue.ToString();
                //    Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                //    Parameters[5] = "Y";   // 디스패치 처리.

                //    C1WindowExtension.SetParameters(print, Parameters);

                //    print.Closed += new EventHandler(print_Closed);

                //    print.Show();
                //}     

                //Util.MessageInfo("이곳");          

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnOutPrint.IsEnabled = true;
            }
        }

        /// <summary>
        /// 작업지시 공통 UserControl에서 작업지시 선택/선택취소 시 메인 화면 정보 재조회 처리
        /// 설비 선택하면 자동 조회 처리 기능 추가로 작지 변경시에도 자동 조회 되도록 구현.
        /// </summary>
        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        private void ColorAnimationInredRectangle()
        {
            recTestMode.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
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
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0) return;

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
            ColorAnimationInredRectangle();
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 2017.07.20  Lee. D. R 
        /// 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
        /// </summary>
        private void GetLossCnt()
        {
            try
            {
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

        private void SetInOutCtrlByLotIdentBasCode()
        {
            try
            {
                // Loader
                if (winInput != null)
                {
                    winInput.LDR_LOT_IDENT_BAS_CODE = _LDR_LOT_IDENT_BAS_CODE;

                    winInput.SetCtrlByLotIdentBasCode();
                }


                // Unloader                
                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
                {
                    if (dgOutProduct.Columns.Contains("CSTID"))
                        dgOutProduct.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCST.Visibility = Visibility.Visible;
                    txtOutCa.Visibility = Visibility.Visible;

                    chkAutoPrint.IsChecked = true;
                    chkAutoPrint.Visibility = Visibility.Visible;

                    SetGridData(dgOutProduct, false);
                }
                else if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    if (dgOutProduct.Columns.Contains("CSTID"))
                        dgOutProduct.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCST.Visibility = Visibility.Visible;
                    txtOutCa.Visibility = Visibility.Visible;

                    chkAutoPrint.IsChecked = true;
                    chkAutoPrint.Visibility = Visibility.Collapsed;

                    SetGridData(dgOutProduct, true);
                }
                else
                {
                    if (dgOutProduct.Columns.Contains("CSTID"))
                        dgOutProduct.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    lblCST.Visibility = Visibility.Collapsed;
                    txtOutCa.Visibility = Visibility.Collapsed;

                    chkAutoPrint.IsChecked = true;
                    chkAutoPrint.Visibility = Visibility.Visible;

                    SetGridData(dgOutProduct, false);
                }

                // RFID 관련 숨김 기능...
                btnOutPrint_Org.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetMessageCode(Exception ex)
        {
            try
            {
                string sRet = "";

                if (ex == null) return sRet;

                if (ex.Data.Contains("TYPE") && ex.Data["TYPE"].ToString().Equals("USER") &&
                    ex.Data.Contains("DATA"))
                {
                    sRet = Util.NVC(ex.Data["DATA"]);
                }

                return sRet;
            }
            catch (Exception excp)
            {
                Util.MessageException(excp);
                return "";
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
                //grdMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();
                //wndPerson.Closed += new EventHandler(wndUser_Closed);
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

            //this.grdMain.Children.Remove(wndPerson);
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
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
        private bool CanCancelRun()
        {
            bool bRet = false;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return bRet;
            }

            string sLotid = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
            string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));

            // 투입 이력 정보 존재여부 확인            
            if (ChkInputHistCnt(sLotid, sWipSeq))
            {
                Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                return bRet;
            }

            // 완성 이력 정보 존재여부 확인
            if (ChkOutCstCnt(sLotid, sWipSeq))
            {
                Util.MessageValidation("SFU4322");   // 생산매거진이 존재하여 취소할 수 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        private bool ChkInputHistCnt(string sLotid, string sWipSeq)
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLotid;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_MTRL_HIST_CNT", "INDATA", "OUTDATA", inTable);

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

        private bool ChkOutCstCnt(string sLotid, string sWipSeq)
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = sLotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT", "INDATA", "OUTDATA", inTable);

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

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_CL", "INDATA", null, inTable, (bizResult, bizException) =>
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

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!CanMerge())
                return;

            ASSY001_OUTLOT_MERGE wndMerge = new ASSY001_OUTLOT_MERGE();
            wndMerge.FrameOperation = FrameOperation;

            if (wndMerge != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = cboEquipmentSegment.Text;
                Parameters[4] = _LDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndMerge, Parameters);

                wndMerge.Closed += new EventHandler(wndMerge_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndMerge.ShowModal()));
            }
        }

        private bool CanMerge()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("설비를 선택 하세요.");
            //    Util.MessageValidation("SFU1673");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void wndMerge_Closed(object sender, EventArgs e)
        {
            ASSY001_OUTLOT_MERGE window = sender as ASSY001_OUTLOT_MERGE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                return;

            this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
        }

        // RFID 가 아닌 원래 발행 로직 처리.. 파일럿 라인 오픈 전 요청 사항으로 숨김 기능 추가.
        private void btnOutPrint_Org_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1237", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    BoxIDPrint();
                }
            });
        }

        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

            bRet = true;
            return bRet;
        }

        private void RunCompleteCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_EQPT_END_LOT", "INDATA", null, inTable, (bizResult, bizException) =>
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

        private void txtOutCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetGridData(C1DataGrid dg, bool bRfid)
        {
            try
            {
                if (bRfid)
                {
                    dg.Columns["CHK"].DisplayIndex = 0;
                    dg.Columns["ROWNUM"].DisplayIndex = 1;
                    dg.Columns["LOTID"].DisplayIndex = 2;
                    dg.Columns["WIPSEQ"].DisplayIndex = 4;
                    dg.Columns["PRODID"].DisplayIndex = 5;

                    dg.Columns["WIPQTY"].DisplayIndex = 6;
                    dg.Columns["BONUS_QTY"].DisplayIndex = 7;
                    dg.Columns["UNIT_CODE"].DisplayIndex = 8;
                    dg.Columns["PRINT_YN"].DisplayIndex = 9;
                    dg.Columns["PRINT_YN_NAME"].DisplayIndex = 10;

                    dg.Columns["WIP_WRK_TYPE_CODE"].DisplayIndex = 11;
                    dg.Columns["WIP_WRK_TYPE_CODE_DESC"].DisplayIndex = 12;
                    dg.Columns["LOTDTTM_CR"].DisplayIndex = 13;
                    dg.Columns["CSTID"].DisplayIndex = 3;
                    dg.Columns["DISPATCH_YN"].DisplayIndex = 14;

                    dg.Columns["INSUSERNAME"].DisplayIndex = 15;

                    //dg.FrozenColumnCount = 5;
                }
                else
                {
                    dg.Columns["CHK"].DisplayIndex = 0;
                    dg.Columns["ROWNUM"].DisplayIndex = 1;
                    dg.Columns["LOTID"].DisplayIndex = 2;
                    dg.Columns["WIPSEQ"].DisplayIndex = 3;
                    dg.Columns["PRODID"].DisplayIndex = 4;

                    dg.Columns["WIPQTY"].DisplayIndex = 5;
                    dg.Columns["BONUS_QTY"].DisplayIndex = 6;
                    dg.Columns["UNIT_CODE"].DisplayIndex = 7;
                    dg.Columns["PRINT_YN"].DisplayIndex = 8;
                    dg.Columns["PRINT_YN_NAME"].DisplayIndex = 9;

                    dg.Columns["WIP_WRK_TYPE_CODE"].DisplayIndex = 10;
                    dg.Columns["WIP_WRK_TYPE_CODE_DESC"].DisplayIndex = 11;
                    dg.Columns["LOTDTTM_CR"].DisplayIndex = 12;
                    dg.Columns["CSTID"].DisplayIndex = 13;
                    dg.Columns["DISPATCH_YN"].DisplayIndex = 14;

                    dg.Columns["INSUSERNAME"].DisplayIndex = 15;

                    //dg.FrozenColumnCount = 5;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnQualityInput_New_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;

            CMM_COM_SELF_INSP wndQualityInput = new CMM_COM_SELF_INSP();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Process.STACKING_FOLDING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInput_New_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }

        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetAutoSelTime = true;
                        return;
                    }

                    dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    dispatcherTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageValidation("SFU1605", cboAutoSearchOut.SelectedValue.ToString());
                    }

                    bSetAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //FrameOperation.PrintFrameMessage(DateTime.Now.ToLongTimeString() + ">>" + dpcTmr.Interval.TotalSeconds.ToString());

                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    if (dgProductLot == null || dgProductLot.Rows.Count < 1)
                        return;

                    int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                    if (iRow < 0)
                        return;

                    GetOutProduct();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr != null && dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));


        }

        private void dgOutProduct_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                    if (pre == null) return;

                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgOutProduct_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                            if (chk != null)
                            {
                                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                                if (!chk.IsChecked.Value)
                                {
                                    chkAll.IsChecked = false;
                                }
                                else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                                {
                                    chkAll.IsChecked = true;
                                }

                                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOutProduct.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgOutProduct.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["PRINT_YN"]).Equals("N") && !Util.NVC(row["WIPSTAT"]).Equals("PROC"))
                {
                    row["CHK"] = true;
                }
            }
            dgOutProduct.ItemsSource = DataTableConverter.Convert(dt);
        }


        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgOutProduct.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgOutProduct.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                row["CHK"] = false;
            }
            dgOutProduct.ItemsSource = DataTableConverter.Convert(dt);
        }

        private bool ChkProcBoxChkArea()
        {
            try
            {
                bool bRet = false;
                _B_PROC_BOX_AUTO_DEL_AREA = false;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "FOL_CFM_PROC_BOX_CHK_EXCP_AREA";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bRet = true;
                    _B_PROC_BOX_AUTO_DEL_AREA = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                _B_PROC_BOX_AUTO_DEL_AREA = false;
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //ESMI 1동(A4) 1~5 Line 만 조회
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_EXCEPT_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        //private void btnCancelInputAll_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!CanQuality())
        //        return;

        //    CMM_ASSY_CANCEL_INPUT_ALL wndCanclInputAllPopup = new CMM_ASSY_CANCEL_INPUT_ALL();
        //    wndCanclInputAllPopup.FrameOperation = FrameOperation;

        //    if (wndCanclInputAllPopup != null)
        //    {
        //        object[] Parameters = new object[4];
        //        Parameters[0] = Process.STACKING_FOLDING;
        //        Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
        //        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
        //        Parameters[3] = _LDR_LOT_IDENT_BAS_CODE;

        //        C1WindowExtension.SetParameters(wndCanclInputAllPopup, Parameters);

        //        wndCanclInputAllPopup.Closed += new EventHandler(wndCanclInputAllPopup_Closed);

        //        // 팝업 화면 숨겨지는 문제 수정.
        //        this.Dispatcher.BeginInvoke(new Action(() => wndCanclInputAllPopup.ShowModal()));
        //        //grdMain.Children.Add(wndQualityInput);
        //        //wndQualityInput.BringToFront();
        //    }
        //}

        //private void wndCanclInputAllPopup_Closed(object sender, EventArgs e)
        //{
        //    CMM_ASSY_CANCEL_INPUT_ALL window = sender as CMM_ASSY_CANCEL_INPUT_ALL;

        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //    }
        //    this.grdMain.Children.Remove(window);
        //}
    }
}
