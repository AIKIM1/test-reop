/*************************************************************************************
 Created Date : 2017.02.09
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - SSC FOLDED CELL
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.09  신광희 : Initial Created.
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
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using Action = System.Action;
using DataSet = System.Data.DataSet;
using Process = LGC.GMES.MES.CMM001.Class.Process;
using Visibility = System.Windows.Visibility;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_030 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _util = new Util();
        BizDataSet _biz = new BizDataSet();
        private UC_WORKORDER _ucWorkOrder = new UC_WORKORDER();
        private UC_IN_OUTPUT_UNUSUAL _ucInOutPut = null;

        private bool bTestMode = false;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        #endregion

        #region Initialize
        public ASSY001_030()
        {
            InitializeComponent();
        }

        private void InitializeCombo()
        {
            //SetEquipmentSegmentCombo(cboEquipmentSegment);
            //SetEquipmentCombo(cboEquipment);

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.SSC_FOLDED_BICELL, EquipmentGroup.SSC_FOLDEDCELL };            
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);//, sCase: "EQUIPMENTSEGMENT_BY_EQGRID");

            String[] sFilter1 = { Process.SSC_FOLDED_BICELL };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };

            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1);
        }
        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            SetWorkOrderWindow();
            SetInputWindow();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitializeCombo();
            this.RegisterName("redBrush", redBrush);
            HideTestMode();
        }
        #endregion

        #region [LINE 콤보]
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SetEquipmentCombo(cboEquipment);
        }
        #endregion

        #region [설비 콤보]
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                ClearControls();

                string equipmentSegmentCode = cboEquipmentSegment.SelectedValue?.ToString() ?? "";
                string equipmentCode = cboEquipment.SelectedValue?.ToString() ?? "";

                if (_ucWorkOrder != null)
                {
                    _ucWorkOrder.EQPTSEGMENT = equipmentSegmentCode;
                    _ucWorkOrder.EQPTID = equipmentCode;
                    _ucWorkOrder.PROCID = Process.SSC_FOLDED_BICELL;
                    _ucWorkOrder.ClearWorkOrderInfo();
                }

                _ucInOutPut?.ChangeEquipment(equipmentSegmentCode, equipmentCode);

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
            GetEquipmentWorkInfo();
        }
        #endregion

        #region [추가기능]
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_010_WAITLOT wndWaitLot = new ASSY001_010_WAITLOT { FrameOperation = FrameOperation };

            object[] parameters = new object[2];
            parameters[0] = Process.SSC_FOLDED_BICELL;
            parameters[1] = Util.GetCondition(cboEquipmentSegment);                        
            C1WindowExtension.SetParameters(wndWaitLot, parameters);

            wndWaitLot.Closed += new EventHandler(wndWaitLot_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
            grdMain.Children.Add(wndWaitLot);
            wndWaitLot.BringToFront();
        }

        private void wndWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY001_010_WAITLOT window = sender as ASSY001_010_WAITLOT;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
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
                Parameters[1] = Process.SSC_FOLDED_BICELL;
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

            DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                         where t.Field<Int32>("CHK") == 1
                         select new
                         {
                             LotId = t.Field<string>("LOTID"),
                             WipSeq = t.Field<decimal>("WIPSEQ"),
                             WorkOrderDetailId = t.Field<string>("WO_DETL_ID")
                         }).FirstOrDefault();

            if (query == null) return;

            CMM_ASSY_DEFECT wndDefect = new CMM_ASSY_DEFECT { FrameOperation = FrameOperation };

            object[] parameters = new object[7];
            parameters[0] = cboEquipmentSegment.SelectedValue;
            parameters[1] = cboEquipment.SelectedValue;
            parameters[2] = Process.SSC_FOLDED_BICELL;
            parameters[3] = query.LotId.GetString();
            parameters[4] = query.WipSeq.GetString();
            parameters[5] = "N";
            parameters[6] = query.WorkOrderDetailId.GetString();

            C1WindowExtension.SetParameters(wndDefect, parameters);

            wndDefect.Closed += new EventHandler(wndDefect_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndDefect.ShowModal()));
            grdMain.Children.Add(wndDefect);
            wndDefect.BringToFront();
        }

        private void wndDefect_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DEFECT window = sender as CMM_ASSY_DEFECT;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
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
            //if (!CanCancelTerm())
            //    return;

            CMM_ASSY_CANCEL_TERM wndCancelTerm = new CMM_ASSY_CANCEL_TERM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = Process.SSC_FOLDED_BICELL;

            C1WindowExtension.SetParameters(wndCancelTerm, parameters);

            wndCancelTerm.Closed += new EventHandler(wndCancelTerm_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
            grdMain.Children.Add(wndCancelTerm);
            wndCancelTerm.BringToFront();
        }

        private void wndCancelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
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

            DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                         where t.Field<Int32>("CHK") == 1
                         select new
                         {
                             LotId = t.Field<string>("LOTID"),
                             WipSeq = t.Field<decimal>("WIPSEQ")
                         }).FirstOrDefault();
            if (query == null) return;

            CMM_ASSY_EQPT_COND wndEqptCond = new CMM_ASSY_EQPT_COND { FrameOperation = FrameOperation };

            object[] parameters = new object[6];
            parameters[0] = cboEquipmentSegment.SelectedValue;
            parameters[1] = cboEquipment.SelectedValue;
            parameters[2] = Process.SSC_FOLDED_BICELL;
            parameters[3] = query.LotId.GetString();
            parameters[4] = query.WipSeq.GetString();
            parameters[5] = cboEquipment.Text;

            C1WindowExtension.SetParameters(wndEqptCond, parameters);

            wndEqptCond.Closed += new EventHandler(wndEqptCond_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            grdMain.Children.Add(wndEqptCond);
            wndEqptCond.BringToFront();
        }
        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND window = sender as CMM_ASSY_EQPT_COND;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
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
                Parameters[2] = Process.SSC_FOLDED_BICELL;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));

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
            //if (!CanEqpRemark())
            //    return;

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
            CMM_COM_EQPCOMMENT wndEqpComment = new CMM_COM_EQPCOMMENT { FrameOperation = FrameOperation };

            object[] parameters = new object[10];
            parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
            parameters[1] = cboEquipment.SelectedValue.ToString();
            parameters[2] = Process.SSC_FOLDED_BICELL;
            parameters[3] = rowIndex < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[4] = rowIndex < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            parameters[5] = cboEquipment.Text;
            parameters[6] = txtShift.Text;      // 작업조명
            parameters[7] = txtShift.Tag;       // 작업조코드
            parameters[8] = txtWorker.Text;     // 작업자명
            parameters[9] = txtWorker.Tag;      // 작업자 ID

            C1WindowExtension.SetParameters(wndEqpComment, parameters);

            wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);
            //this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            grdMain.Children.Add(wndEqpComment);
            wndEqpComment.BringToFront();
        }
        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        #endregion

        #region [작업시작]
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            ASSY001_030_RUNSTART wndRunStart = new ASSY001_030_RUNSTART {FrameOperation = FrameOperation};
            object[] parameters = new object[3];
            parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
            parameters[1] = cboEquipment.SelectedValue.ToString();
            parameters[2] = Process.SSC_FOLDED_BICELL;
            C1WindowExtension.SetParameters(wndRunStart, parameters);

            wndRunStart.Closed += new EventHandler(wndRunStart_Closed);
            grdMain.Children.Add(wndRunStart);
            wndRunStart.BringToFront();
            //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
        }
        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            ASSY001_030_RUNSTART window = sender as ASSY001_030_RUNSTART;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
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


            DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                         where t.Field<Int32>("CHK") == 1
                         select new
                         {
                             LotId = t.Field<string>("LOTID"),
                             WipSeq = t.Field<decimal>("WIPSEQ"),
                             StartDateTime = t.Field<string>("WIPDTTM_ST_ORG"),
                             NowDateTime = t.Field<string>("DTTM_NOW")
                         }).FirstOrDefault();

            if (query == null) return;


            CMM_ASSY_EQPEND wndPop = new CMM_ASSY_EQPEND {FrameOperation = FrameOperation};
            object[] parameters = new object[8];
            parameters[0] = cboEquipmentSegment.SelectedValue;
            parameters[1] = cboEquipment.SelectedValue;
            parameters[2] = Process.SSC_FOLDED_BICELL;
            parameters[3] = query.LotId;
            parameters[4] = query.WipSeq;
            parameters[5] = "N";
            parameters[6] = query.StartDateTime;
            parameters[7] = query.NowDateTime;
            C1WindowExtension.SetParameters(wndPop, parameters);
            wndPop.Closed += new EventHandler(wndEqpEnd_Closed);

            grdMain.Children.Add(wndPop);
            wndPop.BringToFront();
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

            DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                         where t.Field<Int32>("CHK") == 1
                         select new
                         {
                             LotId = t.Field<string>("LOTID"),
                             WipSeq = t.Field<decimal>("WIPSEQ"),
                             WipState = t.Field<string>("WIPSTAT")
                         }).FirstOrDefault();

            if (query == null) return;

            if (!CheckInputEquipmentCondition())
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU2817", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY001_030_CONFIRM wndConfirm = new ASSY001_030_CONFIRM { FrameOperation = FrameOperation };
                        object[] parameters = new object[12];
                        parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                        parameters[1] = cboEquipment.SelectedValue.ToString();
                        parameters[2] = query.LotId.GetString(); //_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                        parameters[3] = query.WipSeq.GetString();
                        parameters[4] = query.WipState.GetString();
                        parameters[5] = Process.SSC_FOLDED_BICELL;

                        parameters[6] = Util.NVC(txtShift.Text);
                        parameters[7] = Util.NVC(txtShift.Tag);
                        parameters[8] = Util.NVC(txtWorker.Text);
                        parameters[9] = Util.NVC(txtWorker.Tag);
                        parameters[10] = Util.NVC(txtShiftStartTime.Text);
                        parameters[11] = Util.NVC(txtShiftEndTime.Text);

                        C1WindowExtension.SetParameters(wndConfirm, parameters);

                        wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                        //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                        grdMain.Children.Add(wndConfirm);
                        wndConfirm.BringToFront();
                    }
                });
            }
            else
            {
                ASSY001_030_CONFIRM wndConfirm = new ASSY001_030_CONFIRM { FrameOperation = FrameOperation };
                object[] parameters = new object[6];
                parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[1] = cboEquipment.SelectedValue.ToString();
                parameters[2] = query.LotId.GetString();
                parameters[3] = query.WipSeq.GetString();
                parameters[4] = query.WipState.GetString();
                parameters[5] = Process.SSC_FOLDED_BICELL;
                C1WindowExtension.SetParameters(wndConfirm, parameters);

                wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                grdMain.Children.Add(wndConfirm);
                wndConfirm.BringToFront();
            }            
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY001_030_CONFIRM window = sender as ASSY001_030_CONFIRM;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            GetEquipmentWorkInfo();
        }

        #endregion

        #region [작업대상]

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            if (rb.IsChecked != null)
            {
                DataRowView drv = rb.DataContext as DataRowView;
                if (drv != null && (bool) rb.IsChecked && drv.Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter) rb.Parent).DataGrid.Rows[i].DataItem, "CHK",idx == i);
                    }
                    //row 색 바꾸기
                    dgProductLot.SelectedIndex = idx;
                    // 상세 정보 조회
                    ProdListClickedProcess(idx);
                }
            }

        }
        #endregion

        #region [작업조, 작업자]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 pop = new CMM_SHIFT_USER2 {FrameOperation = FrameOperation};
            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
            parameters[3] = Process.SSC_FOLDED_BICELL;
            parameters[4] = Util.NVC(txtShift.Tag);
            parameters[5] = Util.NVC(txtWorker.Tag);
            parameters[6] = Util.NVC(cboEquipment.SelectedValue);
            parameters[7] = "Y";
            C1WindowExtension.SetParameters(pop, parameters);
            pop.Closed += new EventHandler(wndShift_Closed);
            grdMain.Children.Add(pop);
            pop.BringToFront();
        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup != null && wndPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEquipmentWorkInfo();
            }
        }

        #endregion

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateBox())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutBox(GetNewOutLotid());
                }
            });
        }

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1237", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    BoxIdPrint();
                }
            });
        }

        private void btnTESTPrint_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_006_TEST_PRINT wndTestPrint = new ASSY001_006_TEST_PRINT { FrameOperation = FrameOperation };
            wndTestPrint.ShowModal();
        }

        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeleteBox())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("삭제 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteOutBox();
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

        private void txtOutBoxQty_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutBox())
                return;

            Util.MessageConfirm("SFU1241", (result) =>
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveOutBox();
                }
            });
        }

        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            ASSY001_005_MAGAZINE_CREATE window = sender as ASSY001_005_MAGAZINE_CREATE;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndReplace_Closed(object sender, EventArgs e)
        {
            ASSY001_004_PAN_REPLACE window = sender as ASSY001_004_PAN_REPLACE;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void dgOutProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgOutProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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
        #endregion

        #region Mehod

        #region [BizCall]

        public void GetProductLot(bool bSelPrv = true, string newProductLotId = "")
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
                if (CommonVerify.HasDataGridRow(dgProductLot))
                {
                    int idx = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                    // 착공 후 조회 시..
                    if (newProductLotId != null && !newProductLotId.Equals(""))
                        sPrvLot = newProductLotId;
                }

                ClearControls();
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_WIP_SSC_FD";
                DataTable inTable = _biz.GetDA_PRD_SEL_WIPINFO_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.SSC_FOLDED_BICELL;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

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
                                GetWaitBoxList();

                            }
                        }
                        else
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                int iRowRun = _util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
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
                                GetWaitBoxList();
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

        private void GetOutProduct()
        {
            try
            {
                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                if (rowIndex < 0)
                    return;

                ShowLoadingIndicator();
                DataTable inTable = _biz.GetDA_PRD_SEL_OUT_BOX_FD();
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_SSC_FD"; 

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgOutProduct, searchResult, FrameOperation);
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
                const string bizRuleName = "BR_PRD_GET_NEW_OUT_LOTID_SSC_FD";

                DataTable inTable = _biz.GetBR_PRD_GET_NEW_OUT_LOT_FD();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK")].DataItem, "LOTID"));
                //newRow["WIP_TYPE_CODE"] = INOUT_TYPE.OUT;
                //newRow["CALDATE_YMD"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                string newLotId = string.Empty;
                if (CommonVerify.HasTableRow(dtResult))
                {
                    newLotId = Util.NVC(dtResult.Rows[0]["OUT_LOTID"]);
                }
                HiddenLoadingIndicator();
                return newLotId;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return "";
            }
        }

        private void CreateOutBox(string newOutLotId)
        {
            try
            {
                if (string.IsNullOrEmpty(newOutLotId))
                    return;

                const string bizRuleName = "BR_PRD_REG_CREATE_OUT_LOT_SSC_FD";

                ShowLoadingIndicator();

                DataTable inTable = _biz.GetBR_PRD_REG_CREATE_BOX_FD();

                //inTable.Columns.Remove("LANGID");


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["OUT_LOTID"] = newOutLotId;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["WO_DETL_ID"] = null;
                newRow["INPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                newRow["RESNQTY"] = 0;
                newRow["SHIFT"] = null;
                newRow["WIPNOTE"] = "";
                newRow["WRK_USER_NAME"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                //"BR_PRD_REG_CREATE_OUT_LOT_FD"
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                            BoxIdPrint(newOutLotId, Convert.ToDecimal(txtOutBoxQty.Text));

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();

                        int idx = _util.GetDataGridRowIndex(dgOutProduct, "LOTID", newOutLotId);
                        if (idx >= 0)
                            DataTableConverter.SetValue(dgOutProduct.Rows[idx].DataItem, "CHK", true);
                        
                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
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

        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _biz.GetBR_PRD_REG_DELETE_BOX_ST();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtInput = indataSet.Tables["IN_INPUT"];
                foreach (C1.WPF.DataGrid.DataGridRow row in dgOutProduct.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                    {
                        newRow = dtInput.NewRow();
                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID"));
                        dtInput.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_SSC_FD", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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

        private void SetDispatch(string boxId = "", decimal qty = 0)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _biz.GetBR_PRD_REG_DISPATCH_LOT_FD();
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

                if (boxId.Equals(""))
                {
                    for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                    {
                        if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;
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
                    newRow["LOTID"] = boxId;
                    newRow["ACTQTY"] = qty;
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

        private DataTable GetThermalPaperPrintingInfo(string lotId)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = lotId;

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

        private void SaveOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = _biz.GetBR_PRD_REG_MODIFY_LOT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = outLotTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")));
                    newRow["WIPQTY_ED"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));

                    outLotTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_LOT_WIPQTY", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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
        
        private bool GetErpSendInfo(string lotId, string wipSeqNo)
        {
            try
            {
                ShowLoadingIndicator();

                bool bRet = false;
                DataTable inTable = _biz.GetDA_PRD_SEL_ERP_SEND_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeqNo;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
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

                DataTable inTable = _biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();
                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK")].DataItem, "LOTID"));
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_DISPATCH_CNT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
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

        private bool CheckSelectWorkOrderInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = _biz.GetDA_PRD_SEL_SET_WORKORDER_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO", "INDATA", "OUTDATA", inTable);
                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals(""))
                    {
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                        return false;
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N"))
                    {
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                        return false;
                    }
                    else
                        return true;
                }

                return false;
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

        private bool CheckInputEquipmentCondition()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _biz.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_CNT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
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

        #region [### TEST모드 저장 ###] - 추가기능
        private bool SetTestMode(bool bTestMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _biz.GetBR_EQP_REG_TESTMODE();

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

                DataTable inTable = _biz.GetDA_EQP_SEL_TESTMODE();

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

        public void OnCurrInCancel(DataTable inMtrl)
        {
            try
            {
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK");
                if (iRow < 0)
                    return;

                ShowLoadingIndicator();
                DataSet indataSet = _biz.GetBR_PRD_REG_INPUT_CANCEL_FD();

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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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

        public void OnCurrInComplete(DataTable inMtrl)
        {
            try
            {
                //if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                //    return;

                if (_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK") == null)
                {
                    return;
                }
                string productLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();

                ShowLoadingIndicator();

                DataSet indataSet = _biz.GetBR_PRD_REG_INPUT_COMPLETE_LM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                //newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROD_LOTID"] = productLotId;

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

        public bool OnCurrAutoInputLot(string inputLotId, string sPstnID)
        {
            try
            {
                string productLotId;
                if (_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK") == null)
                {
                    return false;
                }
                else
                {
                    productLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                }

                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_SSC_FD";

                ShowLoadingIndicator();

                DataSet indataSet = _biz.GetBR_PRD_REG_MTRL_INPUT_FD();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                //newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROD_LOTID"] = productLotId;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLotId;

                inInputTable.Rows.Add(newRow);

                //"BR_PRD_REG_START_INPUT_IN_LOT_FD"
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);

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

        private void GetEquipmentWorkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable indataTable = new DataTable("RQSTDT");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("SHOPID", typeof(string));
                indataTable.Columns.Add("AREAID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));

                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                indata["PROCID"] = Process.SSC_FOLDED_BICELL;

                indataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", indataTable, (result, searchException) =>
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
                            txtShiftStartTime.Text = !result.Rows[0].ItemArray[0].ToString().Equals("") ? Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]) : string.Empty;
                            txtShiftEndTime.Text = !result.Rows[0].ItemArray[1].ToString().Equals("") ? Util.NVC(result.Rows[0]["WRK_END_DTTM"]) : string.Empty;

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

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_BY_EQGRID_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, Process.SSC_FOLDED_BICELL, EquipmentGroup.SSC_FOLDEDCELL };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegment.SelectedValue.GetString(), Process.SSC_FOLDED_BICELL, EquipmentGroup.SSC_FOLDEDCELL };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
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
        #endregion

        #endregion

        #region [Validation]

        private bool CanSearch()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }
            return true;
        }

        private bool CanStartRun()
        {
            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (CommonVerify.HasDataGridRow(dgProductLot))
            {
                DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("WIPSTAT") == "PROC"
                             select t).ToList();
                if (query.Any())
                {
                    Util.MessageValidation("SFU1847");  //작업중인 LOT이 존재 합니다.
                    return false;
                }
            }

            if (!CheckSelectWorkOrderInfo())
            {
                //Util.Alert("선택된 W/O가 없습니다.");
                return false;
            }
            return true;
        }

        private bool CanRunComplete()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("장비완료 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU1866");
                return false;
            }

            return true;
        }

        private bool CanConfirm()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return false;
            }

            // DISPATCH 여부 체크.            
            if (GetNotDispatchCnt() > 0)
            {
                //Util.Alert("발행처리 하지 않은 생산 반제품이 존재 합니다.");
                Util.MessageInfo("SFU1558");
                return false;
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
                    return false;
                }
            }

            return true;
        }

        private bool CanQuality()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool CanDefectInfo()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool CanEqpRemark()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool CanCreateBox()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (txtOutBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return false;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) < 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutBoxQty.SelectAll();
                return false;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) <= 0)
            {
                //Util.Alert("수량은 0보다 커야 합니다.");
                Util.MessageValidation("SFU1683");
                txtOutBoxQty.SelectAll();
                return false;
            }

            return true;
        }

        private bool CanPrint()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return false;
                }
            }

            return true;
        }

        private bool CanDeleteBox()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int idx = _util.GetDataGridFirstRowIndexByCheck(dgOutProduct, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            // ERP 전송 여부 확인.
            foreach (C1.WPF.DataGrid.DataGridRow row in dgOutProduct.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                {
                    if (!GetErpSendInfo(DataTableConverter.GetValue(row.DataItem, "LOTID").GetString(), DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString()))
                    {
                        //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                        Util.MessageValidation("SFU1283", DataTableConverter.GetValue(row.DataItem, "LOTID").GetString());
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CanSaveOutBox()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return false;
                }
            }
            return true;
        }

        private bool CanEqptCondInfo()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

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
            List<Button> listAuth = new List<Button> { btnCreateBOX, btnDeleteBOX, btnOutPrint };
            //listAuth.Add(btnOutSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                _ucWorkOrder.FrameOperation = FrameOperation;
                _ucWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(_ucWorkOrder);
            }
        }

        private void SetInputWindow()
        {
            if (grdInput.Children.Count == 0)
            {
                string equipmentSegmentCode = "";
                string equipmentCode = "";
                if (cboEquipmentSegment?.SelectedValue != null)
                    equipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
                if (cboEquipment?.SelectedValue != null)
                    equipmentCode = cboEquipment.SelectedValue.ToString();

                if (_ucInOutPut == null)
                    _ucInOutPut = new UC_IN_OUTPUT_UNUSUAL(Process.SSC_FOLDED_BICELL, equipmentSegmentCode, equipmentCode);

                //IWorkArea winWorkOrder = obj as IWorkArea;
                _ucInOutPut.FrameOperation = FrameOperation;

                _ucInOutPut._UCParent = this;
                grdInput.Children.Add(_ucInOutPut);
            }
        }

        public void GetWorkOrder()
        {
            if (_ucWorkOrder == null)
                return;

            _ucWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            _ucWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            _ucWorkOrder.PROCID = Process.SSC_FOLDED_BICELL;

            _ucWorkOrder.GetWorkOrder();
        }

        private void GetInputAllInfo(int rowIndex)
        {
            if (_ucInOutPut == null)
                return;

            if (rowIndex < 0 || rowIndex > dgProductLot.Rows.Count)
                return;

            _ucInOutPut.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            _ucInOutPut.EQPTID = cboEquipment.SelectedValue.ToString();
            _ucInOutPut.PROD_LOTID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            _ucInOutPut.PROD_WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            _ucInOutPut.PROD_WOID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WOID"));
            _ucInOutPut.PROD_WODTLID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WO_DETL_ID"));

            _ucInOutPut.SearchAll();
        }

        private void GetCurrInputLotList()
        {
            if (_ucInOutPut == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                _ucInOutPut.PROD_LOTID = "";
                _ucInOutPut.PROD_WIPSEQ = "";
                _ucInOutPut.PROD_WOID = "";
                _ucInOutPut.PROD_WODTLID = "";
                _ucInOutPut.PROD_LOT_STAT = "";
            }

            _ucInOutPut.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            _ucInOutPut.EQPTID = cboEquipment.SelectedValue.ToString();
            _ucInOutPut.PROCID = Process.SSC_FOLDED_BICELL;

            _ucInOutPut.GetCurrInList();
        }

        private void GetWaitBoxList()
        {
            if (_ucInOutPut == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                _ucInOutPut.PROD_LOTID = "";
                _ucInOutPut.PROD_WIPSEQ = "";
                _ucInOutPut.PROD_WOID = "";
                _ucInOutPut.PROD_WODTLID = "";
                _ucInOutPut.PROD_LOT_STAT = "";
            }

            _ucInOutPut.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            _ucInOutPut.EQPTID = cboEquipment.SelectedValue.ToString();
            _ucInOutPut.PROCID = Process.SSC_FOLDED_BICELL;

            _ucInOutPut.GetWaitBox();
        }

        public bool GetSearchConditions(out string processCode, out string equipmentSegmentCode, out string equipmentCode)
        {
            try
            {
                processCode = Process.SSC_FOLDED_BICELL;
                equipmentSegmentCode = cboEquipmentSegment.SelectedIndex >= 0 ? cboEquipmentSegment.SelectedValue.ToString() : "";
                equipmentCode = cboEquipment.SelectedIndex >= 0 ? cboEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception)
            {
                processCode = "";
                equipmentSegmentCode = "";
                equipmentCode = "";
                return false;
                throw;
            }
        }

        private DataRow GetSelectWorkOrderInfo()
        {
            if (_ucWorkOrder == null)
                return null;

            _ucWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            _ucWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            _ucWorkOrder.PROCID = Process.SSC_FOLDED_BICELL;

            return _ucWorkOrder.GetSelectWorkOrderRow();
        }

        public bool ClearControls()
        {
            bool bRet;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailControls();

                _ucInOutPut?.ClearDataGrid();
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
            //Util.gridClear(dgInMagazine);
            //Util.gridClear(dgMaterial);
            Util.gridClear(dgOutProduct);
            //Util.gridClear(dgWaitMagazine);

        }

        private void ProdListClickedProcess(int rowIndex)
        {
            if (rowIndex < 0)
                return;

            ClearDetailControls();

            if (!_util.GetDataGridCheckValue(dgProductLot, "CHK", rowIndex))
            {
                return;
            }

            GetInputAllInfo(rowIndex);

            //GetWaitMagazine();
            //GetInputMagazine(dgProductLot.Rows[iRow].DataItem);
            GetOutProduct();
        }

        private void BoxIdPrint(string sBoxID = "", decimal qty = 0)
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
                        Parameters[1] = Process.SSC_FOLDED_BICELL;
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
                        if (!_util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;


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
                        Parameters[1] = Process.SSC_FOLDED_BICELL;
                        Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[3] = cboEquipment.SelectedValue.ToString();
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
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

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetOutProduct();
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
