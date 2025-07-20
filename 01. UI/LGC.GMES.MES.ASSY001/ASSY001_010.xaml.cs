/*************************************************************************************
 Created Date : 2017.02.16
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - SSC Bi-Cell 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.16  INS 정문교C : Initial Created.
   
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Windows.Media.Animation;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_010 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();
        private UC_IN_OUTPUT_UNUSUAL winInput = null;

        private bool bSetAutoSelTime = false;
        private bool bTestMode = false;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public ASSY001_010()
        {
            InitializeComponent();
        }
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.SSC_BICELL, EquipmentGroup.SSC_BICELL };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);//, sCase: "EQUIPMENTSEGMENT_BY_EQGRID");
            
            String[] sFilter1 = { Process.SSC_BICELL };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };

            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1);

            // 자동 조회 시간 Combo
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            SetWorkOrderWindow();

            SetInputWindow();

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
        }
        #endregion

        #region [설비 콤보]
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
                    winWorkOrder.PROCID = Process.SSC_BICELL;

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

                // 설비 선택 시 자동 조회 처리
                if (cboEquipment.SelectedIndex > 0 && cboEquipment.Items.Count > cboEquipment.SelectedIndex)
                {
                    if (!Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).Equals("SELECT"))
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
        #endregion

        #region [조회]
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
        #endregion

        #region [추가기능]
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        /// <summary>
        /// 대기LOT조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_010_WAITLOT wndWaitLot = new ASSY001_010_WAITLOT();
            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = Process.SSC_BICELL;
                Parameters[1] = Util.GetCondition(cboEquipmentSegment);
                C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWaitLot_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
                grdMain.Children.Add(wndWaitLot);
                wndWaitLot.BringToFront();
            }
        }
        private void wndWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY001_010_WAITLOT window = sender as ASSY001_010_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        /// <summary>
        /// 품질정보조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                Parameters[1] = Process.SSC_BICELL;
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
        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        /// <summary>
        /// 불량정보관리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDefectInfo())
                return;

            CMM_ASSY_DEFECT wndDefect = new CMM_ASSY_DEFECT();
            wndDefect.FrameOperation = FrameOperation;

            if (wndDefect != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.SSC_BICELL;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = "N";
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));

                C1WindowExtension.SetParameters(wndDefect, Parameters);

                wndDefect.Closed += new EventHandler(wndDefect_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndDefect.ShowModal()));
                grdMain.Children.Add(wndDefect);
                wndDefect.BringToFront();
            }
        }
        private void wndDefect_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DEFECT window = sender as CMM_ASSY_DEFECT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);

            GetProductLot();
        }

        /// <summary>
        /// 투입LOT종료취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_CANCEL_TERM wndCancelTerm = new CMM_ASSY_CANCEL_TERM();
            wndCancelTerm.FrameOperation = FrameOperation;

            if (wndCancelTerm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Process.SSC_BICELL;

                C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                wndCancelTerm.Closed += new EventHandler(wndCancelTerm_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
                grdMain.Children.Add(wndCancelTerm);
                wndCancelTerm.BringToFront();
            }
        }
        private void wndCancelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
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

        #endregion

        #region [작업조건등록]
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
                Parameters[2] = Process.SSC_BICELL;
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
        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND window = sender as CMM_ASSY_EQPT_COND;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [품질정보입력]
        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;

            CMM_ASSY_QUALITY_INPUT wndQualityInput = new CMM_ASSY_QUALITY_INPUT();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.SSC_BICELL;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInput_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }
        private void wndQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_INPUT window = sender as CMM_ASSY_QUALITY_INPUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [인수인계노트]
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
                Parameters[2] = Process.SSC_BICELL;
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
        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [작업시작]
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            ASSY001_010_RUNSTART wndRunStart = new ASSY001_010_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.Closed += new EventHandler(wndRunStart_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                grdMain.Children.Add(wndRunStart);
                wndRunStart.BringToFront();
            }
        }
        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            ASSY001_010_RUNSTART window = sender as ASSY001_010_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true, window.NEW_PROD_LOT);
            }
        }
        #endregion

        #region [장비완료] - 설비완공
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
                Parameters[2] = Process.SSC_BICELL;
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

            //Util.MessageConfirm("SFU1865", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        RunComplete();
            //    }
            //});
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
        }

        #endregion

        #region [실적확정]
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm())
                return;

            if (!CheckInputEqptCond())
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
            ASSY001_010_CONFIRM wndConfirm = new ASSY001_010_CONFIRM();
            wndConfirm.FrameOperation = FrameOperation;

            if (wndConfirm != null)
            {
                object[] Parameters = new object[11];
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
                C1WindowExtension.SetParameters(wndConfirm, Parameters);

                wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                grdMain.Children.Add(wndConfirm);
                wndConfirm.BringToFront();
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY001_010_CONFIRM window = sender as ASSY001_010_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
            GetEqptWrkInfo();

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

        #region [생산매거진]
        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutMagazine())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        ////OutSave();
                        OutSaveEndState();
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
        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrintOutMagazine())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1237", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        btnOutPrint.IsEnabled = false;

                        List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                        for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                        {
                            if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                            DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));

                            if (dtRslt == null || dtRslt.Rows.Count < 1) continue;

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

                            dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

                            dicList.Add(dicParam);
                        }

                        CMM_THERMAL_PRINT_LAMI print = new CMM_THERMAL_PRINT_LAMI();
                        print.FrameOperation = FrameOperation;

                        if (print != null)
                        {
                            object[] Parameters = new object[7];
                            Parameters[0] = dicList;
                            Parameters[1] = Process.SSC_BICELL;
                            Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[3] = cboEquipment.SelectedValue.ToString();
                            Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                            Parameters[5] = "Y";   // 디스패치 처리.
                            Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                            C1WindowExtension.SetParameters(print, Parameters);

                            print.Closed += new EventHandler(print_Closed);

                            print.Show();
                        }
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
        private void dgOut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    }
                }
            }));
        }
        private void dgOut_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        private void dgData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
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
            }));
        }
        private void dgData_CommittedEdit(object sender, DataGridCellEventArgs e)
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
                            break;
                    }
                }
            }
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgOut.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["DISPATCH_YN"]).Equals("N") && !Util.NVC(row["WIPSTAT"]).Equals("PROC"))
                {
                    row["CHK"] = true;
                }
            }
            dgOut.ItemsSource = DataTableConverter.Convert(dt);
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgOut.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgOut.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                row["CHK"] = false;
            }
            dgOut.ItemsSource = DataTableConverter.Convert(dt);
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
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_LAMI window = sender as CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetOutMagazine(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem as DataRowView);

            if (chkAll != null && chkAll.IsChecked.HasValue)
                chkAll.IsChecked = false;
        }
        #endregion

        #region [작업조, 작업자]
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
                Parameters[3] = Process.SSC_BICELL;
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
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        #region [### 작업 대상 조회 ###]
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
                }

                ClearControls();

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_SSCBI();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROCID"] = Process.SSC_BICELL;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_SSCBI", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals(""))
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
        #endregion

        #region [### 생산 매거진 조회 ###]
        private void GetOutMagazine(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                chkAll.IsChecked = false;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LOT_LIST_SSCBI();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = Util.NVC(rowview["LOTID"]);

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_SSCBI", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOut, searchResult, FrameOperation);

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

        #region ###################### 생산 매거진 저장 주석 처리
        ////private void OutSave()
        ////{
        ////    try
        ////    {
        ////        ShowLoadingIndicator();

        ////        dgOut.EndEdit();

        ////        DataSet indataSet = _Biz.GetBR_PRD_REG_SAVE_MAGAZINE_LM();

        ////        DataTable inTable = indataSet.Tables["IN_DATA"];
        ////        DataRow newRow = inTable.NewRow();

        ////        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
        ////        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
        ////        newRow["EQPTID"] = cboEquipment.SelectedValue;
        ////        newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
        ////        newRow["USERID"] = LoginInfo.USERID;

        ////        inTable.Rows.Add(newRow);
        ////        newRow = null;

        ////        DataTable outLotTable = indataSet.Tables["IN_OUTLOT"];

        ////        for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
        ////        {
        ////            if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

        ////            if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
        ////            {
        ////                newRow = outLotTable.NewRow();

        ////                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
        ////                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")));
        ////                //newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));

        ////                outLotTable.Rows.Add(newRow);
        ////            }
        ////        }

        ////        if (outLotTable.Rows.Count < 1)
        ////            return;

        ////        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_QTY", "IN_DATA,IN_OUTLOT", null, indataSet);

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Util.MessageException(ex);
        ////    }
        ////    finally
        ////    {
        ////        HiddenLoadingIndicator();
        ////    }
        ////}
        #endregion

        #region [### 생산 매거진 저장 ###]
        private void OutSaveEndState()
        {
            try
            {
                ShowLoadingIndicator();

                dgOut.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_MODIFY_LOT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSTAT")).Equals("END"))
                    {
                        newRow = outLotTable.NewRow();

                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSEQ")));
                        newRow["WIPQTY_ED"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")));

                        outLotTable.Rows.Add(newRow);
                    }
                }

                if (outLotTable == null || outLotTable.Rows.Count <= 0)
                {
                    //Util.Alert("투입된 매거진을 모두 완료 후 실행 하세요.");
                    Util.MessageValidation("SFU1246");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MODIFY_LOT_WIPQTY", "INDATA,IN_INPUT", null, indataSet);

                GetWorkOrder(); // 작지 생산수량 정보 재조회.
                GetProductLot();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx >= 0)
                {
                    GetOutMagazine(dgProductLot.Rows[idx].DataItem as DataRowView);
                }
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 생산매거진 발행 ###]
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
        #endregion
        
        #region [### 생산매거진 발행후 DISPATCH ###]
        private void SetDispatch()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DISPATCH_LOT_LM();
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

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;
                    newRow = inDataTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")));
                    newRow["ACTUQTY"] = 0;
                    newRow["WIPNOTE"] = "";

                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

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

        #region [### 작업시작시 W/O 체크 ###]
        private bool CheckSelWOInfo()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SET_WORKORDER_INFO_SRC();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO_SRC", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals("") && Util.NVC(dtRslt.Rows[0]["WO_DETL_ID2"]).Equals(""))
                    {
                        bRet = false;
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N") && Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK2"]).Equals("N"))
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
        #endregion

        #region [### 실적확정시 설비 LOT 조건 체크 ###]
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
        #endregion

        #region [### 실적확정시 매거진 발행 체크 ###]
        private int GetNotPrintCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_PRT_LABEL_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOT_PRT_LABEL_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOT_PRT_LABEL_CNT"]));
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
        #endregion

        #region [### 설비별 작업조, 작업자, 작업시간 조회 ###]
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
                Indata["PROCID"] = Process.SSC_BICELL;

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
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### 투입 취소 ###]
        public void OnCurrInCancel(DataTable inMtrl)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (iRow < 0)
                    return;

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_CANCEL_INPUT_LOT_SSCBI();

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
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_LOT_SSCBI", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### 투입 완료 ###]
        public void OnCurrInComplete(DataTable inMtrl)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_END_INPUT_LOT_SSCBI();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                //newRow["LANGID"] = LoginInfo.LANGID;

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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_LOT_SSCBI", "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### 투입 LOT 자동 투입 ###] - 사용제외
        public bool OnCurrAutoInputLot(string sInputLot, string sPstnID, double dQty)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return false;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_START_INPUT_LOT_SRC();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = sInputLot;
                //newRow["ACTQTY"] = dQty;

                inInputTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_INPUT_LOT_SRC", "INDATA,IN_INPUT", "OUT_LOT", indataSet);

                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");

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
        #endregion

        #region [### DB 시간 조회 ###]
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
        #endregion

        #region [### TEST모드 저장 ###] - 추가기능
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
        #endregion

        private int GetInputCompleteCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_MOUNT_LOT_CNT_CHK", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return -1;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [Validation]

        #region [### 조회 체크 ###]
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
        #endregion

        #region [### 작업시작 체크 ###]
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
        #endregion

        #region [### 작업종료 - 설비완공 체크 ###]
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

            // Zone 별 투입 완료 여부 체크.
            int iCnt = GetInputCompleteCnt();
            if (iCnt < 0) return bRet;
            if (iCnt != 0)
            {
                Util.MessageValidation("100132");   // 투입완료되지 않은 메거진이 존재 합니다.
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
        #endregion

        #region [### 실적확정 체크 ###]
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

            ////BC이면 발행 여부 체크.
            ////if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODUCT_LEVEL2_CODE")).Equals("BC"))
            ////{
            //발행 여부 체크.
            if (GetNotPrintCnt() > 0)
            {
                //Util.Alert("완공된 매거진 중 발행처리 하지 않은 매거진이 존재 합니다.");
                Util.MessageValidation("SFU1743");
                return bRet;
            }
            ////}

            // 생산 반제품이 PROC 상태가 존재하는 경우 처리 후 확정 되도록 변경.
            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            {
                if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "DISPATCH_YN")).Equals("Y"))
                {
                    //Util.Alert("진행중인 매거진이 존재 합니다.\n완공 처리 후 확정 하시기 바랍니다.");
                    Util.MessageValidation("SFU1920");
                    return bRet;
                }
            }

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            {
                DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();
                int result = DateTime.Compare(shiftEndDateTime, systemDateTime);

                if (result < 0)
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
        #endregion

        #region [### 완성매거진 저장 체크 ###]
        private bool CanSaveOutMagazine()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bool bFnd = false;
            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                bFnd = true;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [### 완성매거진 발행 체크 ###]
        private bool CanPrintOutMagazine()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("라미LOT이 선택되지 않았습니다.");
                Util.MessageValidation("SFU1519");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    //Util.Alert("진행중인 매거진은 발행할 수 없습니다.");
                    Util.MessageValidation("SFU1919");
                    return bRet;
                }

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [### 품질정보관리 체크 ###] - 추가기능
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
        #endregion

        #region [### 작업조건등록 체크 ###] - 추가기능
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
        #endregion

        #region [### TEST모드 체크###] - 추가기능
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
            ////listAuth.Add(btnOutAdd);
            ////listAuth.Add(btnOutDel);
            listAuth.Add(btnOutSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                //IWorkArea winWorkOrder = obj as IWorkArea;
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
                    winInput = new UC_IN_OUTPUT_UNUSUAL(Process.SSC_BICELL, sEqsg, sEqpt);

                //IWorkArea winWorkOrder = obj as IWorkArea;
                winInput.FrameOperation = FrameOperation;

                winInput._UCParent = this;
                grdInput.Children.Add(winInput);
            }
        }
        public void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.SSC_BICELL;

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
            //winInput.PROD_NAME = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "PRODNAME"));

            winInput.SearchAll();
        }
        private void GetCurrInputLotList()
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count < 1)
            {
                winInput.PROD_LOTID = "";
                winInput.PROD_WIPSEQ = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
                winInput.PROD_NAME = "";
            }

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROCID = Process.SSC_BICELL;

            winInput.GetCurrInList();
        }
        private void GetWaitMagazineList()
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count < 1)
            {
                winInput.PROD_LOTID = "";
                winInput.PROD_WIPSEQ = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
                winInput.PROD_NAME = "";
            }

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROCID = Process.SSC_BICELL;

            winInput.GetWaitMagazine();
        }
        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.SSC_BICELL;
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
            Util.gridClear(dgOut);

            if (winInput != null)
            {
                winInput.txtProdCurrIn.Text = string.Empty;
                winInput.txtProdMagazine.Text = string.Empty;
                //winInput.txtProdMagazineSTP.Text = string.Empty;
                winInput.txtProdHist.Text = string.Empty;
            }            
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
            ProcessDetail(dgProductLot.Rows[iRow].DataItem);

        }
        private void ProcessDetail(object obj)
        {
            DataRowView rowview = (obj as DataRowView);

            if (rowview == null)
                return;

            // 생산매거진 조회
            GetOutMagazine(rowview);
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

                    if (dgProductLot.Rows[iRow] != null && dgProductLot.Rows[iRow].DataItem != null)
                        GetOutMagazine((dgProductLot.Rows[iRow].DataItem as DataRowView));

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
        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }
        #endregion

        #endregion


    }
}
