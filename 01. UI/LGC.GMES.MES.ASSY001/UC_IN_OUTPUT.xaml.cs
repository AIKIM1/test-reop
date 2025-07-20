/*************************************************************************************
 Created Date : 2016.11.03
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척화면의 투입 & 생산 부분 공통 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.10.25  INS 김동일K : Initial Created.
  2017.01.17  신광희 : 바구니 투입 취소 처리(BoxInputCancel)
  2018.08.13  이진선 : 투입 이력 생성 버튼 추가
  2019.02.08  황기근 : 불량/Loss/물품청구 탭 및 관련 이벤트/함수 추가
  2020.06.26  김동일      C20200625-000271 기준정보를 통한 투입관련 Tab의 투입 및 투입취소버튼 제어기능 추가
  2021.05.12  김동일 : 툴 관리 탭 추가
  2022.04.07  오광택 : 폴딩공정진척 LOT 다중일괄처리
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// UC_IN_OUTPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_IN_OUTPUT : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private string _EqptSegment = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WoID = string.Empty;
        private string _WoDtlID = string.Empty;
        private string _LotStat = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;

        private string _Max_Pre_Proc_End_Day = string.Empty;

        //투입 언마운트 타입 코드.
        string _INPUT_LOT_UNMOUNT_TYPE_CODE = "";

        private DateTime _Min_Valid_Date;

        private bool _StackingYN = false;

        public UserControl _UCParent;     // Caller

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private LGC.GMES.MES.CMM001.UserControls.UcAssyToolinfo winInputHistTool = new LGC.GMES.MES.CMM001.UserControls.UcAssyToolinfo();

        UC_IN_HIST_CREATE wndInputCreate;

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


        public string EQPTSEGMENT
        {
            get { return _EqptSegment; }
            set { _EqptSegment = value; }
        }

        public string EQPTID
        {
            get { return _EqptID; }
            set { _EqptID = value; }
        }

        public string PROCID
        {
            get { return _ProcID; }
            set { _ProcID = value; }
        }

        public string PROD_LOTID
        {
            get { return _LotID; }
            set { _LotID = value; }
        }

        public string PROD_WIPSEQ
        {
            get { return _WipSeq; }
            set { _WipSeq = value; }
        }

        public string PROD_WOID
        {
            get { return _WoID; }
            set { _WoID = value; }
        }

        public string PROD_WODTLID
        {
            get { return _WoDtlID; }
            set { _WoDtlID = value; }
        }

        public string PROD_LOT_STAT
        {
            get { return _LotStat; }
            set { _LotStat = value; }
        }

        public string LDR_LOT_IDENT_BAS_CODE
        {
            get { return _LDR_LOT_IDENT_BAS_CODE; }
            set { _LDR_LOT_IDENT_BAS_CODE = value; }
        }

        private struct PRV_VALUES
        {
            public string sPrvOutTray;
            public string sPrvCurrIn;
            public string sPrvOutBox;

            public PRV_VALUES(string sTray, string sIn, string sBox)
            {
                this.sPrvOutTray = sTray;
                this.sPrvCurrIn = sIn;
                this.sPrvOutBox = sBox;
            }
        }

        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("", "", "");

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

        public UC_IN_OUTPUT(string sProcID, string sEqsgID, string sEqptID, bool bStacking = false, string sLoaderIdentBasCode = "")
        {
            PROCID = sProcID;
            EQPTSEGMENT = sEqsgID;
            EQPTID = sEqptID;
            _StackingYN = bStacking;
            _LDR_LOT_IDENT_BAS_CODE = sLoaderIdentBasCode;

            // 선입선출 기준일 조회.
            GetProcMtrlInputRule();

            InitializeComponent();
        }

        private void InitializeControls()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                // 자재 투입위치 코드
                String[] sFilter1 = { EQPTID, "PROD" };
                String[] sFilter2 = { EQPTID, null }; // 자재,제품 전체
                String[] sFilter3 = { "PKG_STK_PSTN_ID" };

                _combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                _combo.SetCombo(cboMagMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                _combo.SetCombo(cboBoxMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                _combo.SetCombo(cboStackPosition, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

                bool isStackUsingArea = GetStackUsingArea();
                tbStackInputHist.Visibility = Visibility.Collapsed;

                if (PROCID.Equals(Process.LAMINATION))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Visible;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;
                    tbEqptLoss.Visibility = Visibility.Visible;
                    tbDefect.Visibility = Visibility.Visible;

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals(Process.STACKING_FOLDING))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Visible;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;
                    tbEqptLoss.Visibility = Visibility.Visible;

                    grdMagTypeCntInfo.Visibility = Visibility.Visible;

                    btnCurrDelete.Visibility = Visibility.Visible;
                    tbDefect.Visibility = Visibility.Collapsed;

                    if (_StackingYN)
                    {
                        rdoMagAType.Content = "HALFTYPE";
                        rdoMagAType.Tag = "HC";
                        tbATypeTitle.Text = "HALFTYPE";

                        rdoMagCtype.Content = "MONOTYPE";
                        rdoMagCtype.Tag = "MC";
                        tbCTypeTitle.Text = "MONOTYPE";
                    }

                }
                else if (PROCID.Equals(Process.PACKAGING))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Visible;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Visible;
                    tbEqptLoss.Visibility = Visibility.Visible;
                    btnInBoxInputHist.Visibility = Visibility.Visible;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                    if (isStackUsingArea)
                        tbStackInputHist.Visibility = Visibility.Visible;
                    else
                        tbStackInputHist.Visibility = Visibility.Collapsed;
                    tbDefect.Visibility = Visibility.Collapsed;

                    // 교체처리 버튼 없애기.
                    grdCurrIn.ColumnDefinitions[11].Width = new GridLength(0);
                    grdCurrIn.ColumnDefinitions[12].Width = new GridLength(0);
                    btnCurrInReplace.IsEnabled = false;

                    // Tray Type Combo
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    dt.Rows.Add("25", "25");
                    dt.Rows.Add("50", "50");

                }
                else if (PROCID == Process.SSC_FOLDED_BICELL)
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Visible;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Visible;
                    tbEqptLoss.Visibility = Visibility.Collapsed;

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                    tbDefect.Visibility = Visibility.Collapsed;

                    // 교체처리 버튼 없애기.
                    grdCurrIn.ColumnDefinitions[11].Width = new GridLength(0);
                    grdCurrIn.ColumnDefinitions[12].Width = new GridLength(0);
                    btnCurrInReplace.IsEnabled = false;

                    // Tray Type Combo
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    dt.Rows.Add("25", "25");
                    dt.Rows.Add("50", "50");
                }
                else if (PROCID == Process.CT_INSP)
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;
                    tbEqptLoss.Visibility = Visibility.Collapsed;
                    //tbBox.Visibility = Visibility.Collapsed;
                    if (LoginInfo.CFG_SHOP_ID == "G382" && _ProcID == "A8800")
                        tbBox.Visibility = Visibility.Visible;
                    else
                        tbBox.Visibility = Visibility.Collapsed;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    btnCurrDelete.Visibility = Visibility.Collapsed;
                    //btnCurrInCancel.Visibility = Visibility.Collapsed;
                    btnCurrInReplace.Visibility = Visibility.Collapsed;
                    tbDefect.Visibility = Visibility.Collapsed;

                    tbl_WaitBox_MountPstn.Visibility = Visibility.Collapsed;
                    cboBoxMountPstsID.Visibility = Visibility.Collapsed;
                    bdr_WaitBox_MountPstn.Visibility = Visibility.Collapsed;
                    btnWaitBoxInPut.Visibility = Visibility.Collapsed;

                }

                if (tbHist.Visibility == Visibility.Visible)
                {
                    if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                        dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                        dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                        dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    if (dgInputHist.Columns.Contains("RMN_QTY"))
                        dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                }

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Virtual Function
        protected virtual void GetProductLot()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCurrInCancel(DataTable inMtrl)
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrInCancel");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = inMtrl;
                    else parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCurrInDelete(DataTable inMtrl)
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrInDelete");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = inMtrl;
                    else parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        protected virtual void OnCurrInComplete(DataTable inMtrl)
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrInComplete");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = inMtrl;
                    else parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCurrAutoInputLot(string sInputLot, string sPstnID, string sInMtrlID, string sInQty)
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrAutoInputLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = sInputLot;
                    else if (i == 1) parameterArrys[i] = sPstnID;
                    else if (i == 2) parameterArrys[i] = sInMtrlID;
                    else if (i == 3) parameterArrys[i] = sInQty;
                    else parameterArrys[i] = null;
                }

                object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                if ((bool)result)
                {

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region [Main]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            SetInputHistToolWindow();

            #region EDC BCR Reading Info
            CheckEDCVisibity();
            #endregion
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitializeControls();

            #region EDC BCR Reading Info
            // 자동 조회 시간 Combo
            CommonCombo _combo = new CommonCombo();
            String[] sFilter9 = { "EDC_BCD_RATE_INTERVAL" };
            _combo.SetCombo(cboEdcAutoSearch, CommonCombo.ComboStatus.NA, sFilter: sFilter9, sCase: "COMMCODE");

            if (cboEdcAutoSearch != null && cboEdcAutoSearch.Items != null && cboEdcAutoSearch.Items.Count > 0)
                cboEdcAutoSearch.SelectedIndex = cboEdcAutoSearch.Items.Count - 1;


            this.RegisterName("BCR_WARNING", edcBrush);

            if (dspTmr_EdcWarn != null)
            {
                int iSec = 0;

                if (cboEdcAutoSearch != null && cboEdcAutoSearch.SelectedValue != null && !cboEdcAutoSearch.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboEdcAutoSearch.SelectedValue.ToString());

                dspTmr_EdcWarn.Tick -= dspTmr_EdcWarn_Tick;
                dspTmr_EdcWarn.Tick += dspTmr_EdcWarn_Tick;
                dspTmr_EdcWarn.Interval = new TimeSpan(0, 0, iSec);
                //dispatcherTimer.Start();
            }
            #endregion            
        }

        #endregion

        #region [투입현황]

        private void btnCurrInCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInCancel())
                    return;

                //if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                //{
                //    ASSY001_INPUT_CANCEL_CST wndCancel = new ASSY001_INPUT_CANCEL_CST();
                //    wndCancel.FrameOperation = FrameOperation;

                //    if (wndCancel != null)
                //    {
                //        int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                //        object[] Parameters = new object[13];
                //        Parameters[0] = EQPTSEGMENT;
                //        Parameters[1] = EQPTID;
                //        Parameters[2] = PROCID;
                //        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                //        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
                //        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "CSTID"));
                //        Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
                //        Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MTRLID"));
                //        Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_STAT_CHG_DTTM"));
                //        Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                //        Parameters[10] = "CURR";
                //        Parameters[11] = "";
                //        Parameters[12] = PROD_LOTID;

                //        C1WindowExtension.SetParameters(wndCancel, Parameters);

                //        wndCancel.Closed += new EventHandler(wndCancel_Closed);

                //        // 팝업 화면 숨겨지는 문제 수정.
                //        //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
                //        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                //        {
                //            if (tmp.Name == "grdMain")
                //            {
                //                tmp.Children.Add(wndCancel);
                //                wndCancel.BringToFront();
                //                break;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable ininDataTable = _Biz.GetUC_BR_PRD_REG_CURR_INPUT();

                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_Util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                            {
                                DataRow newRow = ininDataTable.NewRow();
                                newRow["WIPNOTE"] = "";
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));
                                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "CSTID"));
                                //Int64 iSeq = 0;
                                //Int64.TryParse(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_SEQNO")), out iSeq);

                                //newRow["INPUT_SEQNO"] = iSeq;

                                ininDataTable.Rows.Add(newRow);
                            }
                        }

                        OnCurrInCancel(ininDataTable);
                    }
                });
                //}                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrInReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInReplace())
                    return;

                ASSY001_004_PAN_REPLACE wndReplace = new ASSY001_004_PAN_REPLACE();
                wndReplace.FrameOperation = FrameOperation;

                if (wndReplace != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                    object[] Parameters = new object[10];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "WIPSEQ"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    Parameters[6] = PROCID;
                    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
                    Parameters[8] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "CSTID"));
                    C1WindowExtension.SetParameters(wndReplace, Parameters);

                    wndReplace.Closed += new EventHandler(wndReplace_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(wndReplace);
                            wndReplace.BringToFront();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrDelete())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("탈착처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU5047", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable ininDataTable = _Biz.GetUC_BR_PRD_REG_CURR_INPUT();

                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_Util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                            {
                                DataRow newRow = ininDataTable.NewRow();
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                                newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MTRLID"));
                                newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));
                                //newRow["WIPNOTE"] = "";

                                ininDataTable.Rows.Add(newRow);
                            }
                        }

                        OnCurrInDelete(ininDataTable);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCurrInComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInComplete())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입완료 처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1972", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable ininDataTable = _Biz.GetUC_BR_PRD_REG_CURR_INPUT();

                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_Util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                            {
                                DataRow newRow = ininDataTable.NewRow();
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                                newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MTRLID"));
                                newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));
                                //newRow["WIPNOTE"] = "";

                                ininDataTable.Rows.Add(newRow);
                            }
                        }

                        OnCurrInComplete(ininDataTable);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCurrInLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!CanCurrAutoInputLot())
                    {
                        txtOutCa.Text = "";
                        txtCurrInLotID.Text = "";
                        return;
                    }

                    string sInPos = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string sInPosName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));

                    if (PROCID.Equals(Process.PACKAGING) && Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    {
                        OnCurrAutoInputLot(txtCurrInLotID.Text.Trim(), sInPos, "", "");

                        txtOutCa.Text = "";
                        txtCurrInLotID.Text = "";
                    }
                    else
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("{0} 위치에 {1} 을 투입 하시겠습니까?", sInPosName, txtCurrInLotID.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        object[] parameters = new object[2];
                        parameters[0] = sInPosName;
                        parameters[1] = txtCurrInLotID.Text.Trim();

                        Util.MessageConfirm("SFU1291", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                OnCurrAutoInputLot(txtCurrInLotID.Text.Trim(), sInPos, "", "");

                                txtOutCa.Text = "";
                                txtCurrInLotID.Text = "";
                            }
                        }, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtOutCa.Text = "";
                txtCurrInLotID.Text = "";
            }
        }

        private void txtOutCa_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotID = GetLotIDbyCSTID(Util.NVC(txtOutCa.Text), "WAIT");

                    if (!string.IsNullOrEmpty(sLotID))
                    {
                        txtCurrInLotID.Text = sLotID;
                        txtCurrInLotID_KeyDown(txtCurrInLotID, e);
                    }
                    else
                    {
                        txtOutCa.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    txtOutCa.Text = "";
                }
            }

        }

        private void dgCurrIn_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //this.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    C1DataGrid dg = sender as C1DataGrid;
                //    if (e.Cell != null &&
                //        e.Cell.Presenter != null &&
                //        e.Cell.Presenter.Content != null)
                //    {
                //        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                //        if (chk != null)
                //        {
                //            switch (Convert.ToString(e.Cell.Column.Name))
                //            {
                //                case "CHK":
                //                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                //                       dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                //                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                //                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                //                       !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                //                        chk.IsChecked = true;

                //                        _PRV_VLAUES.sPrvCurrIn = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_MOUNT_PSTN_ID"));

                //                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                //                        {
                //                            if (e.Cell.Row.Index != idx)
                //                            {
                //                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                //                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                //                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                //                                {
                //                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                //                                }
                //                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                //                            }
                //                        }
                //                    }
                //                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                //                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                //                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                //                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                //                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                //                        chk.IsChecked = false;

                //                        _PRV_VLAUES.sPrvCurrIn = "";
                //                    }
                //                    break;
                //            }

                //            if (dg.CurrentCell != null)
                //                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                //            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                //                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                //        }
                //    }
                //}));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Pancake 투입]

        private void rdoWaitPancake_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtWaitPancakeLot != null)
                    txtWaitPancakeLot.Text = "";

                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitPancake();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakeInPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitPanCakeInput())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PancakeInput();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
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
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboPancakeMountPstnID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitPancake();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [매거진 투입]

        private void rdoWaitMaz_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue)
                {
                    if ((bool)(sender as RadioButton).IsChecked)
                    {
                        GetWaitMagazine();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitMazID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtWaitMazID.Text.Length != 10)
                    {
                        //Util.Alert("MAGAZINE ID 자릿수(10자리)가 맞지 않습니다.");
                        Util.MessageValidation("SFU1391");
                        return;
                    }

                    GetWaitMagazine(txtWaitMazID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitMgz_CST_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotID = GetLotIDbyCSTID(Util.NVC(txtWaitMgz_CST.Text), "WAIT");

                    if (!string.IsNullOrEmpty(sLotID))
                        GetWaitMagazine(sLotID);

                    txtWaitMgz_CST.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtWaitMgz_CST.Text = "";
            }
        }

        private void txtHist_CST_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotID = GetLotIDbyCSTIDHist(Util.NVC(txtHist_CST.Text), "PROC,TERM,END");

                    if (!string.IsNullOrEmpty(sLotID))
                    {
                        txtHistLotID.Text = sLotID;
                        GetInputHistory();
                    }
                    else
                    {
                        txtHist_CST.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtHist_CST.Text = "";
                txtHistLotID.Text = "";
            }
        }

        private void btnWaitMagInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitMagInput())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MagazineInput();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitMagRework_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitMagRework())
                    return;

                ASSY001_005_MAGAZINE_CREATE wndMAZCreate = new ASSY001_005_MAGAZINE_CREATE();
                wndMAZCreate.FrameOperation = FrameOperation;

                if (wndMAZCreate != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = PROD_WOID;
                    Parameters[3] = _LDR_LOT_IDENT_BAS_CODE;

                    C1WindowExtension.SetParameters(wndMAZCreate, Parameters);

                    wndMAZCreate.Closed += new EventHandler(wndMAZCreate_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndMAZCreate.ShowModal()));
                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(wndMAZCreate);
                            wndMAZCreate.BringToFront();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitMagazine_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
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
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [바구니 투입]

        private void btnWaitBoxInPut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitBoxInPut())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1248", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BasketInput(false, -1);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitBox_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
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
                                    }
                                    break;
                            }

                            if (dg.CurrentCell != null)
                                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                            else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [투입이력]

        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputHistory();
        }

        private void txtHistLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInputHistory();
            }
        }

        private void btnHistCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHistDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHistSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHistMountPstsID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetInputHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [투입 바구니(PKG Input Box List]
        private void btnInBoxInputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanInBoxInputCancel(dgInputHist))
                    return;

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    DataTable dtTmp = DataTableConverter.Convert(dgInputHist.ItemsSource);
                    DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

                    if (drTmp == null || drTmp.Length < 1)
                    {
                        Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                        return;
                    }
                    else if (drTmp.Length > 1)
                    {
                        Util.MessageValidation("SFU4468");  // 한개의 데이터만 선택하세요.
                        return;
                    }


                    ASSY001_INPUT_CANCEL_CST wndCancel = new ASSY001_INPUT_CANCEL_CST();
                    wndCancel.FrameOperation = FrameOperation;

                    if (wndCancel != null)
                    {
                        int idx = _Util.GetDataGridCheckFirstRowIndex(dgInputHist, "CHK");

                        object[] Parameters = new object[13];
                        Parameters[0] = EQPTSEGMENT;
                        Parameters[1] = EQPTID;
                        Parameters[2] = PROCID;
                        Parameters[3] = "";
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[idx].DataItem, "INPUT_LOTID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[idx].DataItem, "CSTID"));
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[idx].DataItem, "INPUT_QTY"));
                        Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[idx].DataItem, "MTRLID"));
                        Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[idx].DataItem, "INPUT_DTTM"));
                        Parameters[9] = "";
                        Parameters[10] = "HIST";
                        Parameters[11] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[idx].DataItem, "INPUT_SEQNO"));
                        Parameters[12] = PROD_LOTID;

                        C1WindowExtension.SetParameters(wndCancel, Parameters);

                        wndCancel.Closed += new EventHandler(wndInputHistCancel);

                        // 팝업 화면 숨겨지는 문제 수정.
                        //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(wndCancel);
                                wndCancel.BringToFront();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1988", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            BoxInputCancel();
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInputBoxCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanInBoxInputCancel(dgInputBox))
                    return;

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID")
                    || _LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
                {
                    DataTable dtTmp = DataTableConverter.Convert(dgInputBox.ItemsSource);
                    DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

                    if (drTmp == null || drTmp.Length < 1)
                    {
                        Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                        return;
                    }
                    else if (drTmp.Length > 1)
                    {
                        Util.MessageValidation("SFU4468");  // 한개의 데이터만 선택하세요.
                        return;
                    }

                    ASSY001_INPUT_CANCEL_CST wndCancel = new ASSY001_INPUT_CANCEL_CST();
                    wndCancel.FrameOperation = FrameOperation;

                    if (wndCancel != null)
                    {
                        int idx = _Util.GetDataGridCheckFirstRowIndex(dgInputBox, "CHK");

                        object[] Parameters = new object[13];
                        Parameters[0] = EQPTSEGMENT;
                        Parameters[1] = EQPTID;
                        Parameters[2] = PROCID;
                        Parameters[3] = "";
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[idx].DataItem, "INPUT_LOTID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[idx].DataItem, "CSTID"));
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[idx].DataItem, "INPUT_QTY"));
                        Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[idx].DataItem, "PRODID"));
                        Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[idx].DataItem, "INPUT_DTTM"));
                        Parameters[9] = "";
                        Parameters[10] = "HIST";
                        Parameters[11] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[idx].DataItem, "INPUT_SEQNO"));
                        Parameters[12] = PROD_LOTID;

                        C1WindowExtension.SetParameters(wndCancel, Parameters);

                        wndCancel.Closed += new EventHandler(wndInputHistBoxCancel);

                        // 팝업 화면 숨겨지는 문제 수정.
                        //this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(wndCancel);
                                wndCancel.BringToFront();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //"투입취소 하시겠습니까?"
                    Util.MessageConfirm("SFU1988", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            BoxInputCancel2();
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Method

        #region [BizCall]

        #region [공통]

        public void GetCurrInList()
        {
            try
            {
                // 메인 LOT이 없는 경우 disable 처리..
                if (PROD_LOTID.Equals(""))
                {
                    if (PROCID.Equals(Process.PACKAGING))
                        btnCurrInCancel.IsEnabled = true;
                    else
                        btnCurrInCancel.IsEnabled = false;

                    btnCurrInComplete.IsEnabled = false;

                }
                else
                {
                    btnCurrInCancel.IsEnabled = true;
                    btnCurrInComplete.IsEnabled = true;
                }

                string sBizNAme = string.Empty;

                if (PROCID.Equals(Process.LAMINATION))
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_LM";
                else
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST";

                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["PROCID"] = PROCID;
                //newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = EQPTID;
                //newRow["PROD_LOTID"] = PROD_LOTID;
                //newRow["PROD_WIPSEQ"] = PROD_WIPSEQ.Equals("") ? 1 : Convert.ToDecimal(PROD_WIPSEQ);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgCurrIn.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation);

                        if (!_PRV_VLAUES.sPrvCurrIn.Equals(""))
                        {
                            int idx = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", _PRV_VLAUES.sPrvCurrIn);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgCurrIn.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgCurrIn.SelectedIndex = idx;

                                dgCurrIn.ScrollIntoView(idx, dgCurrIn.Columns["CHK"].Index);
                            }
                        }

                        // 라미의 경우 컬럼 다르게 보이도록 수정.
                        if (PROCID.Equals(Process.LAMINATION))
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                            //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;

                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                            //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;

                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        }


                        // 폴딩의 경우 타입별 투입 수량 설정
                        if (PROCID.Equals(Process.STACKING_FOLDING))
                        {
                            SetElecTypeCount();
                        }

                        if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            if (dgCurrIn.Columns.Contains("CSTID"))
                            {
                                dgCurrIn.Columns["CSTID"].IsReadOnly = false;
                                dgCurrIn.Columns["CSTID"].EditOnSelection = true;
                            }
                        }
                        else
                        {
                            if (dgCurrIn.Columns.Contains("CSTID"))
                            {
                                dgCurrIn.Columns["CSTID"].IsReadOnly = true;
                                dgCurrIn.Columns["CSTID"].EditOnSelection = false;
                            }
                        }

                        //if (dgCurrIn.CurrentCell != null)
                        //    dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.CurrentCell.Row.Index, dgCurrIn.Columns.Count - 1);
                        //else if (dgCurrIn.Rows.Count > 0 && dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1) != null)
                        //    dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            try
            {
                // 투입 언마운트 타입 조회
                _INPUT_LOT_UNMOUNT_TYPE_CODE = GetInputUnMountType();

                string sBizName = string.Empty;

                if (_INPUT_LOT_UNMOUNT_TYPE_CODE.Equals("UNMOUNT"))             // 3Loss 이력 저장 타입.
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_L";
                else if (_INPUT_LOT_UNMOUNT_TYPE_CODE.Equals("UNMOUNT_LOSS"))   // CNB, CWA3동 3Loss 타입.
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_L";
                else if (LoginInfo.CFG_SHOP_ID == "G382" && _ProcID == "A8800") // ESMI CT검사
                {
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_MI";
                }
                else
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST";


                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["PROD_WIPSEQ"] = PROD_WIPSEQ.Equals("") ? 1 : Convert.ToDecimal(PROD_WIPSEQ);
                newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString().Equals("") ? null : cboHistMountPstsID.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        switch (_INPUT_LOT_UNMOUNT_TYPE_CODE)
                        {
                            case "UNMOUNT":         // 3Loss 이력 저장 타입.
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Visible;

                                break;
                            case "UNMOUNT_LOSS":    // CNB, CWA3동 3Loss 타입.
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY")
                                    && (_ProcID.Equals(Process.NOTCHING) || _ProcID.Equals(Process.PACKAGING)))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY")
                                    && (_ProcID.Equals(Process.NOTCHING) || _ProcID.Equals(Process.LAMINATION)))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY")
                                    && (_ProcID.Equals(Process.NOTCHING) || _ProcID.Equals(Process.LAMINATION) || _ProcID.Equals(Process.STACKING_FOLDING)))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Visible;
                                break;
                            default:
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("INPUT_STAT_CODE"))
                                    dgInputHist.Columns["INPUT_STAT_CODE"].Visibility = Visibility.Visible;
                                break;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation, true);

                        if (dgInputHist.CurrentCell != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                        else if (dgInputHist.Rows.Count > 0 && dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1) != null)
                            dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                        txtHist_CST.Text = "";
                        txtHistLotID.Text = "";
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetInputUnMountType()
        {
            try
            {
                string sRet = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.NVC(_EqptID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0)
                    if (dtResult.Columns.Contains("INPUT_LOT_UNMOUNT_TYPE_CODE"))
                        sRet = Util.NVC(dtResult.Rows[0]["INPUT_LOT_UNMOUNT_TYPE_CODE"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void GetStackInputHistory()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["CMCDTYPE"] = "PKG_STK_PSTN_ID";
                newRow["INPUT_LOTID"] = String.IsNullOrEmpty(tbxStackId.Text) ? null : tbxStackId.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = String.IsNullOrEmpty(cboStackPosition.SelectedValue.ToString()) ? null : cboStackPosition.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_INPUT_MTRL_HIST_BY_CMCODE", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgStackInputHist, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                //HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetProcMtrlInputRule()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_MTRL_INPUT_RULE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _Max_Pre_Proc_End_Day = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                //HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetEqptLoss()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EQPTID;
                newRow["LOTID"] = PROD_LOTID;
                newRow["WIPSEQ"] = PROD_WIPSEQ;

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

                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [LAMINATION]

        public void GetWaitPancake()
        {
            try
            {
                string BizRule = "DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE";
                string sInMtrlClssCode = GetInputMtrlClssCode();


                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = EQPTID;
                newRow["PROCID"] = Process.LAMINATION;
                newRow["WOID"] = PROD_WOID;
                newRow["IN_LOTID"] = txtWaitPancakeLot.Text;
                //newRow["PRDT_CLSS_CODE"] = null;  -- 설비 조건으로 PROD_CLASS_CODE 조회 함..
                newRow["INPUT_MTRL_CLSS_CODE"] = sInMtrlClssCode;

                inTable.Rows.Add(newRow);

                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    BizRule = "DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE_LAMI";
                }

                new ClientProxy().ExecuteService(BizRule, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PancakeInput()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_MTRL_INPUT_LM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot(GetSelectWorkOrderInfo());
                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void GetInputPancakeValid(string sPos, string sInLot, out string sRet, out string sMsg)
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_SEL_IN_LOT_VALID_LM();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPos;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = sInLot;

                inInputTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_LM", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    sRet = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["REQCODE"]);
                    sMsg = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["REQMSG"]);
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }
                HiddenParentLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                sRet = "NG";
                sMsg = ex.Message;
            }
        }

        private string GetInputMtrlClssCode()
        {
            try
            {
                if (cboPancakeMountPstnID == null || cboPancakeMountPstnID.SelectedValue == null)
                {
                    return "";
                }

                string sInputMtrlClssCode = "";
                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EQPTID;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                    txtWaitPancakeInputClssCode.Text = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }
        }

        #endregion

        #region [FOLDING]

        public void GetWaitMagazine(string sLot = "")
        {
            try
            {
                string sElec = string.Empty;

                if (rdoMagAType.IsChecked.HasValue && (bool)rdoMagAType.IsChecked)
                {
                    sElec = rdoMagAType.Tag.ToString();
                }
                else if (rdoMagCtype.IsChecked.HasValue && (bool)rdoMagCtype.IsChecked)
                {
                    sElec = rdoMagCtype.Tag.ToString();
                }
                else
                    return;

                string sBizName = "";

                if (_StackingYN)
                    sBizName = "DA_PRD_SEL_WAIT_MAG_ST";
                else
                    sBizName = "DA_PRD_SEL_WAIT_MAG_FD";

                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["PROCID"] = PROCID;
                newRow["EQPTID"] = EQPTID;
                newRow["PRODUCT_LEVEL2_CODE"] = _StackingYN ? sElec : "BC"; //BI-CELL, Stacking 경우 Lv2가 Lv3와 동일 코드.
                if (!_StackingYN)
                    newRow["PRODUCT_LEVEL3_CODE"] = sElec;

                newRow["WOID"] = PROD_WOID;
                if (!sLot.Equals(""))
                    newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        // FIFO 기준 Date
                        try
                        {
                            if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                            {
                                DataRow row = (from t in bizResult.AsEnumerable()
                                               where (t.Field<string>("VALID_DATE_YMDHMS") != null)
                                               select t).FirstOrDefault();


                                if (row != null)
                                {
                                    DateTime.TryParse(Util.NVC(row["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        //dgWaitMagazine.ItemsSource = DataTableConverter.Convert(bizResult);
                        Util.GridSetData(dgWaitMagazine, bizResult, FrameOperation);

                        if (dgWaitMagazine.CurrentCell != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.CurrentCell.Row.Index, dgWaitMagazine.Columns.Count - 1);
                        else if (dgWaitMagazine.Rows.Count > 0 && dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1) != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetWaitMagazineInfo(string sLot)
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_FD();

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["PROCID"] = PROCID;
                //newRow["PRODUCT_LEVEL2_CODE"] = _StackingYN ? sElec : "BC"; //BI-CELL, Stacking 경우 Lv2가 Lv3와 동일 코드.
                //newRow["PRODUCT_LEVEL3_CODE"] = sElec;
                newRow["WOID"] = PROD_WOID;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_MAG_INFO_FD", "INDATA", "OUTDATA", inTable);

                HiddenParentLoadingIndicator();

                return dtRslt;
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void MagazineInput()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_MTRL_INPUT_FD();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitMagazine.Rows.Count - dgWaitMagazine.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgWaitMagazine, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboMagMountPstnID.SelectedValue.ToString();
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitMagazine.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPUT_IN_LOT_FD", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();

                        if (cboMagMountPstnID.Items.Count > 0)
                            cboMagMountPstnID.SelectedIndex = 0;

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [STACKING]
        #endregion

        #region [PACKAGING]

        public void GetWaitBox()
        {
            try
            {
                ShowParentLoadingIndicator();

                string bizRuleName = string.Empty;
                string sProcID = PROCID != Process.SSC_FOLDED_BICELL ? Process.PACKAGING : Process.SSC_FOLDED_BICELL;

                if (PROCID.Equals(Process.CT_INSP))
                {
                    if(LoginInfo.CFG_SHOP_ID == "G382")
                        bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_MI";
                    else
                        bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_CI";
                    sProcID = Process.CT_INSP;
                }
                else if (PROCID.Equals(Process.SSC_FOLDED_BICELL))
                {
                    bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_SSC_FD";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_CL";
                }

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = sProcID; //PROCID != Process.SSC_FOLDED_BICELL ? Process.PACKAGING : Process.SSC_FOLDED_BICELL;
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = PROD_WODTLID;
                if ( PROCID.Equals(Process.CT_INSP) && LoginInfo.CFG_SHOP_ID == "G382")
                    newRow["PROCID"] = "A9000";

                inTable.Rows.Add(newRow);

                //string bizRuleName = PROCID == Process.SSC_FOLDED_BICELL ? "DA_PRD_SEL_WAIT_LOT_LIST_SSC_FD" : "DA_PRD_SEL_WAIT_LOT_LIST_CL";

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }


                        Util.GridSetData(dgWaitBox, searchResult, FrameOperation);

                        if (dgWaitBox.CurrentCell != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.CurrentCell.Row.Index, dgWaitBox.Columns.Count - 1);
                        else if (dgWaitBox.Rows.Count > 0 && dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1) != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BasketInput(bool bAuto, int iRow)
        {
            try
            {
                string bizRuleID = string.Empty;

                if (PROCID.Equals(Process.CT_INSP))
                    bizRuleID = "BR_PRD_CHK_INPUT_LOT_CI";
                else
                    bizRuleID = "BR_PRD_CHK_INPUT_LOT_CL";

                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_BASKET_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                if (bAuto)
                {
                    if (iRow < 0)
                        return;

                    newRow = null;

                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                    newRow = inMtrlTable.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "CSTID")); ;
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "LOTID"));
                    newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "PRODID"));
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                else
                {
                    newRow = null;

                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];

                    for (int i = 0; i < dgWaitBox.Rows.Count - dgWaitBox.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgWaitBox, "CHK", i)) continue;
                        newRow = inMtrlTable.NewRow();
                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "CSTID")); ;
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "LOTID"));
                        newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")));

                        inMtrlTable.Rows.Add(newRow);
                    }
                }

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleID, "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
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
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInBoxList()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_IN_BOX_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = PROD_LOTID;
                newRow["WIPSEQ"] = PROD_WIPSEQ.Equals("") ? 1 : Convert.ToDecimal(PROD_WIPSEQ);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_BOX_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputBox.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputBox, searchResult, FrameOperation);

                        //if (dgInputBox.CurrentCell != null)
                        //    dgInputBox.CurrentCell = dgInputBox.GetCell(dgInputBox.CurrentCell.Row.Index, dgInputBox.Columns.Count - 1);
                        //else if (dgInputBox.Rows.Count > 0)
                        //    dgInputBox.CurrentCell = dgInputBox.GetCell(dgInputBox.Rows.Count, dgInputBox.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BoxInputCancel()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputHist.Rows.Count - dgInputHist.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgInputHist, "CHK", i)) continue;
                    newRow = null;
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetInBoxList();
                        GetInputHistory();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BoxInputCancel2()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];

                for (int i = 0; i < dgInputBox.Rows.Count - dgInputBox.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgInputBox, "CHK", i)) continue;
                    newRow = null;
                    newRow = inMtrlTable.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_SEQNO")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_SEQNO")));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_LOTID"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputBox.Rows[i].DataItem, "INPUT_QTY")));

                    inMtrlTable.Rows.Add(newRow);
                }
                //string xml = indataSet.GetXml();
                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetInBoxList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetNowRunProdLot()
        {
            try
            {
                ShowParentLoadingIndicator();

                string sNowLot = "";
                DataTable inTable = _Biz.GetDA_PRD_SEL_NOW_PROD_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["PROCID"] = PROCID;
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_NOW_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sNowLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                }

                return sNowLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }
        }
        #endregion

        #endregion

        #region [Validation]

        #region [공통]

        private bool CanCurrInCancel()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!PROCID.Equals(Process.PACKAGING))
            {
                if (Util.NVC(PROD_LOTID).Equals(""))
                {
                    //Util.Alert("선택된 실적정보가 없습니다.");
                    Util.MessageValidation("SFU1640");
                    return bRet;
                }
            }

            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                {
                    //Util.Alert("투입 LOT이 없습니다.");
                    Util.MessageValidation("SFU1945");
                    return bRet;
                }

                //if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "CSTID")).Equals(""))
                //    {
                //        Util.MessageValidation("SFU1244");
                //        return bRet;
                //    }
                //}
            }

            bRet = true;

            return bRet;
        }

        private bool CanCurrInReplace()
        {
            bool bRet = false;

            DataTable dtTmp = DataTableConverter.Convert(dgCurrIn.ItemsSource);
            DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

            if (drTmp == null || drTmp.Length < 1)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }
            else if (drTmp.Length > 1)
            {
                Util.MessageValidation("SUF4961");  // 하나의 투입 위치만 선택하세요.
                return bRet;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            // 자동 Change 모드인 경우 투입취소 불가.
            if (_ProcID.Equals(Process.LAMINATION) &&
                DataTableConverter.Convert(dgCurrIn.ItemsSource).Columns.Contains("AUTO_STOP_FLAG") &&
                Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "AUTO_STOP_FLAG")).Equals("Y"))
            {
                // 잔량처리 불가 : 설비 자동 Change 모드로 투입 완료처리된 LOT은 처리 불가.
                Util.MessageValidation("SFU6038");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanCurrInComplete()
        {
            bool bRet = false;

            DataTable dtTmp = DataTableConverter.Convert(dgCurrIn.ItemsSource);
            DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

            if (drTmp == null || drTmp.Length < 1)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }
            else if (drTmp.Length > 1)
            {
                Util.MessageValidation("SUF4961");  // 하나의 투입 위치만 선택하세요.
                return bRet;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanCurrDelete()
        {
            bool bRet = false;

            DataTable dtTmp = DataTableConverter.Convert(dgCurrIn.ItemsSource);
            DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

            if (drTmp == null || drTmp.Length < 1)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanCurrAutoInputLot()
        {
            bool bRet = false;

            if (txtCurrInLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1379");
                return bRet;
            }

            if (PROD_LOTID.Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return bRet;
            }

            DataTable dtTmp = DataTableConverter.Convert(dgCurrIn.ItemsSource);
            DataRow[] drTmp = dtTmp != null && dtTmp.Columns.Contains("CHK") ? dtTmp?.Select("CHK = 1") : null;

            if (drTmp == null || drTmp.Length < 1)
            {
                Util.MessageValidation("SFU1957");  // 투입 위치를 선택하세요.
                return bRet;
            }
            else if (drTmp.Length > 1)
            {
                Util.MessageValidation("SUF4961");  // 하나의 투입 위치만 선택하세요.
                return bRet;
            }

            //if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            //{
            //    //Util.Alert("투입 위치를 선택하세요.");
            //    Util.MessageValidation("SFU1957");
            //    return bRet;
            //}

            if (PROCID.Equals(Process.LAMINATION) || PROCID.Equals(Process.STACKING_FOLDING))
            {
                for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                    {
                        Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"))); // %1 에 이미 투입되었습니다.
                        return bRet;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                        {
                            Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));    // %1 에 이미 투입되었습니다.
                            return bRet;
                        }
                    }
                }
            }

            // 패키지 공정인 경우 최근 LOT에만 투입 처리 가능
            // 마지막 PROD LOT에만 투입 가능하도록 처리.
            if (PROCID.Equals(Process.PACKAGING))
            {
                string sSelProd = PROD_LOTID;
                string sNowProd = GetNowRunProdLot();
                if (!sNowProd.Equals("") && sSelProd != sNowProd)
                {
                    //Util.Alert("선택한 조립LOT({0})은 마지막 작업중인 LOT이 아닙니다.\n마지막 작업중인 LOT({1})에만 투입할 수 있습니다.", sSelProd, sNowProd);
                    object[] parameters = new object[2];
                    parameters[0] = sSelProd;
                    parameters[1] = sNowProd;

                    Util.MessageValidation("SFU1666", parameters);
                    return false;
                }
            }

            //// 폴딩 & 스태킹인 경우 타입별 투입 수량 체크.
            //if (PROCID.Equals(Process.STACKING_FOLDING))
            //{
            //    // 스캔 LOT 재품 타입 조회.
            //    DataTable dtRslt = GetWaitMagazineInfo(txtCurrInLotID.Text);

            //    if (dtRslt != null && dtRslt.Rows.Count > 0)
            //    {
            //        if (Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
            //        {
            //            if (int.Parse(tbATypeCnt.Text) >= int.Parse(tbATypeTotCnt.Text))
            //            {
            //                Util.Alert("A 타입은 더이상 투입할 수 없습니다.");
            //                return bRet;
            //            }
            //        }
            //        else if (Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
            //        {
            //            if (int.Parse(tbCTypeCnt.Text) >= int.Parse(tbCTypeTotCnt.Text))
            //            {
            //                Util.Alert("C 타입은 더이상 투입할 수 없습니다.");
            //                return bRet;
            //            }
            //        }
            //        if (Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]).Equals("HC"))
            //        {
            //            if (int.Parse(tbATypeCnt.Text) >= int.Parse(tbATypeTotCnt.Text))
            //            {
            //                Util.Alert("HALF 타입은 더이상 투입할 수 없습니다.");
            //                return bRet;
            //            }
            //        }
            //        else if (Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]).Equals("MC"))
            //        {
            //            if (int.Parse(tbCTypeCnt.Text) >= int.Parse(tbCTypeTotCnt.Text))
            //            {
            //                Util.Alert("MONO 타입은 더이상 투입할 수 없습니다.");
            //                return bRet;
            //            }
            //        }
            //    }
            //}

            bRet = true;

            return bRet;
        }

        public bool CanPkgConfirm(string sProdLot)
        {
            bool bRet = false;

            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "WIPSTAT")).Equals("PROC") &&
                        Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PROD_LOTID")).Equals(sProdLot))
                    {
                        //Util.Alert("[{0}] 위치에 투입완료되지 않은 바구니[{1}]가 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")), Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")));
                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));

                        Util.MessageValidation("SFU1282", parameters);
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }

        #endregion

        #region [LAMINATION]
        private bool CanWaitPanCakeInput()
        {
            bool bRet = false;

            if (Util.NVC(PROD_LOTID).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return bRet;
            }

            if (cboPancakeMountPstnID.SelectedValue == null || cboPancakeMountPstnID.SelectedValue.Equals("") || cboPancakeMountPstnID.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            if (_Util.GetDataGridCheckCnt(dgWaitPancake, "CHK") < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            int iRow = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cboPancakeMountPstnID.SelectedValue.ToString());

            if (iRow >= 0)
            {
                string sInPancake = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID"));
                string sInState = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT"));
                string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
                {
                    //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                    Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                    return bRet;
                }
            }


            // 투입 BIZ에서 VALIDATION 처리 하므로 주석.
            //string sRet = string.Empty;
            //string sMsg = string.Empty;

            //for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
            //{
            //    if (!_Util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

            //    GetInputPancakeValid(cboPancakeMountPstnID.SelectedValue.ToString(), Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID")), out sRet, out sMsg);
            //    if (sRet.Equals("NG"))
            //    {
            //        Util.Alert(sMsg);
            //        return bRet;
            //    }
            //}

            bRet = true;
            return bRet;
        }

        #endregion

        #region [FOLDING]

        private bool CanWaitMagInput()
        {
            bool bRet = false;

            if (Util.NVC(PROD_LOTID).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgWaitMagazine, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (cboMagMountPstnID.SelectedValue == null || cboMagMountPstnID.SelectedValue.Equals("") || cboMagMountPstnID.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            int iRow = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cboMagMountPstnID.SelectedValue.ToString());

            if (iRow >= 0)
            {
                string sInMag = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "INPUT_LOTID"));
                string sInState = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT"));
                string sMtgrid = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                if (sMtgrid.Equals("PROD") && sInState.Equals("PROC"))//if (!sInPancake.Trim().Equals(""))
                {
                    //Util.Alert("{0} 에 진행중인 LOT이 존재 합니다.", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                    Util.MessageValidation("SFU1290", Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                    return bRet;
                }
            }

            // 투입 BIZ에서 VALIDATION 처리 하므로 주석.
            //string sRet = string.Empty;
            //string sMsg = string.Empty;

            //for (int i = 0; i < dgWaitMagazine.Rows.Count - dgWaitMagazine.BottomRows.Count; i++)
            //{
            //    if (!_Util.GetDataGridCheckValue(dgWaitMagazine, "CHK", i)) continue;

            //    GetInputMagazineValid(cboMagMountPstnID.SelectedValue.ToString(), Util.NVC(DataTableConverter.GetValue(dgWaitMagazine.Rows[i].DataItem, "LOTID")), out sRet, out sMsg);
            //    if (sRet.Equals("NG"))
            //    {
            //        Util.Alert(sMsg);
            //        return bRet;
            //    }
            //}

            bRet = true;

            return bRet;
        }

        private bool CanWaitMagRework()
        {
            bool bRet = false;

            if (Util.NVC(EQPTSEGMENT).Equals("") || Util.NVC(EQPTSEGMENT).Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (Util.NVC(EQPTID).Equals("") || Util.NVC(EQPTID).Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        #endregion

        #region [STACKING]
        public bool CanStkConfirm(string sProdLot)
        {
            bool bRet = false;

            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PROD_LOTID")).Equals(sProdLot) &&
                        Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRODUCT_LEVEL2_CODE")).Equals("MC"))   // Mono Cell 만 체크..
                    {
                        object[] parameters = new object[1];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                        //parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));

                        Util.MessageValidation("SFU1290", parameters);
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }
        #endregion

        #region [PACKAGING]

        private bool CanWaitBoxInPut()
        {
            bool bRet = false;

            if (Util.NVC(PROD_LOTID).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgWaitBox, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 패키지 바구니 투입 시 복수개의 PROD가 존재하는 경우 처리 되도록 변경.
            if (cboBoxMountPstsID == null || cboBoxMountPstsID.SelectedValue == null || cboBoxMountPstsID.SelectedIndex < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            int iRow = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cboBoxMountPstsID.SelectedValue.ToString());
            if (iRow < 0)
            {
                //Util.Alert("자재 투입 위치에 존재하지 않는 투입 위치 입니다.");
                Util.MessageValidation("SFU1819");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("[{0}] 위치에 RUN 상태의 바구니가 존재 합니다.\n완료 처리 후 투입 하십시오.", cboBoxMountPstsID.Text);
                Util.MessageValidation("SFU1281", cboBoxMountPstsID.Text);
                return bRet;
            }

            // 패키지 공정인 경우 최근 LOT에만 투입 처리 가능
            // 마지막 PROD LOT에만 투입 가능하도록 처리.
            if (PROCID.Equals(Process.PACKAGING))
            {
                string sSelProd = PROD_LOTID;
                string sNowProd = GetNowRunProdLot();
                if (!sNowProd.Equals("") && sSelProd != sNowProd)
                {
                    //Util.Alert("선택한 조립LOT({0})은 마지막 작업중인 LOT이 아닙니다.\n마지막 작업중인 LOT({1})에만 투입할 수 있습니다.", sSelProd, sNowProd);
                    object[] parameters = new object[2];
                    parameters[0] = sSelProd;
                    parameters[1] = sNowProd;
                    Util.MessageValidation("SFU1666", parameters);
                    return false;
                }
            }

            //int iRow = _Util.GetDataGridRowIndex(dgCurrIn, "MOUNT_MTRL_TYPE_CODE", "PROD");
            //if (iRow > -1)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "WIPSTAT")).Equals("PROC"))
            //    {
            //        Util.Alert("RUN 상태의 바구니가 존재 합니다.\n완료 처리 후 투입 하십시오.");
            //        return bRet;
            //    }
            //}

            //for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
            //        {
            //            Util.Alert("RUN 상태의 바구니가 존재 합니다.\n완료 처리 후 투입 하십시오.");
            //            return bRet;
            //        }
            //    }
            //}

            bRet = true;
            return bRet;
        }

        private bool CanInBoxInputCancel(C1.WPF.DataGrid.C1DataGrid dg)
        {
            if (_Util.GetDataGridFirstRowIndexByCheck(dg, "CHK") < 0 || !CommonVerify.HasDataGridRow(dg))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        #endregion

        #endregion

        #region [Func]

        #region [공통]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnCurrInCancel);
                listAuth.Add(btnCurrDelete);
                listAuth.Add(btnCurrInComplete);
                listAuth.Add(btnWaitPancakeInPut);
                listAuth.Add(btnWaitMagInput);
                listAuth.Add(btnWaitBoxInPut);
                listAuth.Add(btnInBoxInputCancel);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearAll()
        {
            try
            {
                //투입현황
                if (dgCurrIn != null)
                    Util.gridClear(dgCurrIn);
                if (tbATypeCnt != null)
                    tbATypeCnt.Text = "0";
                if (tbCTypeCnt != null)
                    tbCTypeCnt.Text = "0";
                if (txtCurrInLotID != null)
                    txtCurrInLotID.Text = "";

                //Pancake투입
                if (dgWaitPancake != null)
                    Util.gridClear(dgWaitPancake);
                if (txtWaitPancakeLot != null)
                    txtWaitPancakeLot.Text = "";

                //대기매거진
                if (dgWaitMagazine != null)
                    Util.gridClear(dgWaitMagazine);
                if (txtWaitMazID != null)
                    txtWaitMazID.Text = "";

                //바구니투입
                if (dgWaitBox != null)
                    Util.gridClear(dgWaitBox);

                //투입 바구니(PKG INPUT)
                if (dgInputBox != null)
                    Util.gridClear(dgInputBox);

                //투입이력
                if (dgInputHist != null)
                    Util.gridClear(dgInputHist);

                //설비불량로스
                if (dgEqpFaulty != null)
                    Util.gridClear(dgEqpFaulty);


                #region EDC BCR Reading Info
                if (dgEDCInfo != null)
                    Util.gridClear(dgEDCInfo);
                #endregion

                ClearInputHistTool();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetCtrlByLotIdentBasCode()
        {
            try
            {
                //자재투입
                if (tbCurrIn.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgCurrIn.Columns.Contains("CSTID"))
                            dgCurrIn.Columns["CSTID"].Visibility = Visibility.Visible;

                        lblCST.Visibility = Visibility.Visible;
                        txtOutCa.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgCurrIn.Columns.Contains("CSTID"))
                            dgCurrIn.Columns["CSTID"].Visibility = Visibility.Collapsed;

                        lblCST.Visibility = Visibility.Collapsed;
                        txtOutCa.Visibility = Visibility.Collapsed;
                    }
                }

                //대기 Pancake
                if (tbPancake.Visibility == Visibility.Visible)
                {

                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgWaitPancake.Columns.Contains("CSTID"))
                            dgWaitPancake.Columns["CSTID"].Visibility = Visibility.Visible;

                        lblWait_CST.Visibility = Visibility.Visible;
                        txtWait_CST.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgWaitPancake.Columns.Contains("CSTID"))
                            dgWaitPancake.Columns["CSTID"].Visibility = Visibility.Collapsed;

                        lblWait_CST.Visibility = Visibility.Collapsed;
                        txtWait_CST.Visibility = Visibility.Collapsed;
                    }
                }

                //대기매거진
                if (tbMagazine.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgWaitMagazine.Columns.Contains("CSTID"))
                            dgWaitMagazine.Columns["CSTID"].Visibility = Visibility.Visible;

                        lblWaitMgz_CST.Visibility = Visibility.Visible;
                        txtWaitMgz_CST.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgWaitMagazine.Columns.Contains("CSTID"))
                            dgWaitMagazine.Columns["CSTID"].Visibility = Visibility.Collapsed;

                        lblWaitMgz_CST.Visibility = Visibility.Collapsed;
                        txtWaitMgz_CST.Visibility = Visibility.Collapsed;
                    }
                }

                //대기바구니
                if (tbBox.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgWaitBox.Columns.Contains("CSTID"))
                            dgWaitBox.Columns["CSTID"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgWaitBox.Columns.Contains("CSTID"))
                            dgWaitBox.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    }
                }

                //투입 바구니(PKG INPUT)
                if (tbInBox.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgInputBox.Columns.Contains("CSTID"))
                            dgInputBox.Columns["CSTID"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgInputBox.Columns.Contains("CSTID"))
                            dgInputBox.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    }
                }

                //투입이력
                if (tbHist.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgInputHist.Columns.Contains("CSTID"))
                            dgInputHist.Columns["CSTID"].Visibility = Visibility.Visible;

                        lblHist_CST.Visibility = Visibility.Visible;
                        txtHist_CST.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgInputHist.Columns.Contains("CSTID"))
                            dgInputHist.Columns["CSTID"].Visibility = Visibility.Collapsed;

                        lblHist_CST.Visibility = Visibility.Collapsed;
                        txtHist_CST.Visibility = Visibility.Collapsed;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearDataGrid()
        {
            try
            {
                //투입현황
                if (dgCurrIn != null)
                    Util.gridClear(dgCurrIn);
                if (txtCurrInLotID != null)
                    txtCurrInLotID.Text = "";

                //Pancake투입
                if (dgWaitPancake != null)
                    Util.gridClear(dgWaitPancake);
                if (txtWaitPancakeLot != null)
                    txtWaitPancakeLot.Text = "";

                //대기매거진
                if (dgWaitMagazine != null)
                    Util.gridClear(dgWaitMagazine);
                if (txtWaitMazID != null)
                    txtWaitMazID.Text = "";

                //바구니투입
                if (dgWaitBox != null)
                    Util.gridClear(dgWaitBox);

                //투입 바구니(PKG INPUT)
                if (dgInputBox != null)
                    Util.gridClear(dgInputBox);

                //투입이력
                if (dgInputHist != null)
                    Util.gridClear(dgInputHist);

                //설비불량로스
                if (dgEqpFaulty != null)
                    Util.gridClear(dgEqpFaulty);

                ClearInputHistTool();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [PARENT Fnc Call]

        private void SearchParentProductInfo()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetLotIDbyCSTID(string sCSTID, string sWipStat)
        {
            try
            {
                string sLotID = "";

                if (string.IsNullOrEmpty(sCSTID))
                {
                    Util.MessageValidation("SFU1244");  //카세트ID를 입력 하세요.
                    return sLotID;
                }
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("CSTID", typeof(string));
                dtRQSTDT.Columns.Add("WIPSTAT", typeof(string));


                DataRow dr = dtRQSTDT.NewRow();
                dr["CSTID"] = Util.NVC(sCSTID);
                dr["WIPSTAT"] = Util.NVC(sWipStat);
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_BY_CSTID", "RQSTDT", "RSLTDT", dtRQSTDT); //CSTID로 LOTID 찾기.

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLotID = dtRslt.Rows[0]["LOTID"].ToString();
                }
                else
                {
                    Util.MessageValidation("100182", new object[] { sCSTID }); //카세트[%1]에 연결된 LOTID 가 존재하지 않습니다.                    
                }

                return sLotID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private string GetLotIDbyCSTIDHist(string sCSTID, string sWipStat)
        {
            try
            {
                string sLotID = "";

                if (string.IsNullOrEmpty(sCSTID))
                {
                    Util.MessageValidation("SFU1244");  //카세트ID를 입력 하세요.
                    return sLotID;
                }
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("CSTID", typeof(string));
                dtRQSTDT.Columns.Add("WIPSTAT", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("EQPTID", typeof(string));


                DataRow dr = dtRQSTDT.NewRow();
                dr["CSTID"] = Util.NVC(sCSTID);
                dr["WIPSTAT"] = Util.NVC(sWipStat);
                dr["PROCID"] = Util.NVC(_ProcID);
                dr["EQSGID"] = Util.NVC(_EqptSegment);
                dr["EQPTID"] = Util.NVC(_EqptID);

                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_BY_CSTID_HIST", "RQSTDT", "RSLTDT", dtRQSTDT); //CSTID로 LOTID 찾기.

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLotID = dtRslt.Rows[0]["LOTID"].ToString();
                }
                else
                {
                    Util.MessageValidation("100182", new object[] { sCSTID }); //카세트[%1]에 연결된 LOTID 가 존재하지 않습니다.                    
                }

                return sLotID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void GetParentProductInfo()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetParentProductInfo");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                if ((bool)result)
                {
                    PROD_LOTID = parameterArrys[0].ToString();
                    PROD_WIPSEQ = parameterArrys[1].ToString();
                    PROD_WOID = parameterArrys[2].ToString();
                    PROD_WODTLID = parameterArrys[3].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowParentLoadingIndicator()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HiddenParentLoadingIndicator()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("HiddenLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Use PARENT]

        public void ChangeEquipment(string sEqsgID, string sEqptID)
        {
            try
            {
                EQPTSEGMENT = sEqsgID;
                EQPTID = sEqptID;


                PROD_LOTID = "";
                PROD_WIPSEQ = "";
                PROD_WOID = "";
                PROD_WODTLID = "";
                PROD_LOT_STAT = "";


                InitializeControls();

                ClearAll();

                // 폴딩 타입별 투입수량 조회
                if (PROCID.Equals(Process.STACKING_FOLDING))
                {
                    //GetEquipmentDefault();
                }

                // 현재 설비 투입 자재 조회 처리.
                //GetCurrInList();


                #region EDC BCR Reading Info
                if (tbEDCBCRInfo.Visibility == Visibility.Visible)
                {
                    GetBcrReadingRate(true);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SearchAll()
        {
            try
            {
                //ClearAll();

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls();

                if (tbCurrIn.Visibility == Visibility.Visible)
                {
                    GetCurrInList();
                }
                if (tbPancake.Visibility == Visibility.Visible)
                {
                    GetWaitPancake();
                }
                if (tbMagazine.Visibility == Visibility.Visible)
                {
                    GetWaitMagazine();
                }
                if (tbBox.Visibility == Visibility.Visible)
                {
                    GetWaitBox();
                }
                if (tbHist.Visibility == Visibility.Visible)
                {
                    GetInputHistory();
                }
                if (tbInBox.Visibility == Visibility.Visible)
                {
                    GetInBoxList();
                }
                if (tbEqptLoss.Visibility == Visibility.Visible)
                {
                    GetEqptLoss();
                }
                if (tbStackInputHist.Visibility == Visibility.Visible)
                {
                    GetStackInputHistory();
                }
                if (tbDefect.Visibility == Visibility.Visible)
                {
                    GetDefectInfo();
                }
                if (tbInputHistTool.Visibility == Visibility.Visible)
                {
                    GetInputHistTool();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }




        #endregion

        #endregion

        #region [LAMINATION]
        #endregion

        #region [FOLDING]
        private void SetElecTypeCount()
        {
            int iCtype = 0;
            int iAtype = 0;

            if (dgCurrIn.ItemsSource == null || dgCurrIn.Rows.Count < 1)
                return;

            for (int i = 0; i < dgCurrIn.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("CT")) iCtype++;
                    else if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("AT")) iAtype++;
                    else if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("MC")) iCtype++;
                    else if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "PRDT_CLSS_CODE")).Equals("HC")) iAtype++;
                }
            }
            tbATypeCnt.Text = iAtype.ToString();
            tbCTypeCnt.Text = iCtype.ToString();
        }
        #endregion

        #region [STACKING]
        #endregion

        #region [PACKAGING]
        #endregion

        #region [PopUp Event]

        private void wndReplace_Closed(object sender, EventArgs e)
        {
            ASSY001_004_PAN_REPLACE window = sender as ASSY001_004_PAN_REPLACE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            ASSY001_005_MAGAZINE_CREATE window = sender as ASSY001_005_MAGAZINE_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetWaitMagazine();
        }


        private void wndCancel_Closed(object sender, EventArgs e)
        {
            ASSY001_INPUT_CANCEL_CST window = sender as ASSY001_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetCurrInList();
            }
        }

        private void wndInputHistBoxCancel(object sender, EventArgs e)
        {
            ASSY001_INPUT_CANCEL_CST window = sender as ASSY001_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetInBoxList();
            }
        }

        private void wndInputHistCancel(object sender, EventArgs e)
        {
            ASSY001_INPUT_CANCEL_CST window = sender as ASSY001_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetInputHistory();
            }
        }
        #endregion

        #endregion

        #endregion



        private void dgWaitBox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
            }));
        }

        private void dgWaitBox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgWaitMagazine_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (txtWaitMazID.Text.Trim().Length > 0)
                            {
                                e.Cell.Presenter.Background = null;
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))    // 신규 매거진 구성 data의 경우 biz에서 FIFO 무시 됨.
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
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
            }));
        }

        private void dgWaitMagazine_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgWaitPancake_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtWaitPancakeLot.Text.Trim().Length > 0)
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
            }));
        }

        private void dgWaitPancake_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void btnWaitMagazinePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                String sMazID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sMazID.Equals(""))
                {
                    List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                    DataTable dtRslt = GetThermalPaperPrintingInfo(sMazID);

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

                    dicParam.Add("RE_PRT_YN", "F"); // 재발행 여부.

                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = null;
                        Parameters[1] = PROCID;
                        Parameters[2] = EQPTSEGMENT;
                        Parameters[3] = EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.
                        Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(printWaitMaz_Closed);

                        print.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowParentLoadingIndicator();

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
                HiddenParentLoadingIndicator();
            }
        }

        private void printWaitMaz_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnWaitPancakePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                string sPancakeID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sPancakeID.Equals(""))
                {
                    List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                    DataTable dtRslt = GetThermalPaperPrintingInfo(sPancakeID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;


                    Dictionary<string, string> dicParam = new Dictionary<string, string>();


                    dicParam.Add("PANCAKEID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("TOT_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("REMAIN_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("NOTE", "");
                    dicParam.Add("PRINTQTY", "1");  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = null;
                        Parameters[1] = Process.LAMINATION;
                        Parameters[2] = EQPTSEGMENT;
                        Parameters[3] = EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(printWaitPancake_Closed);

                        print.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void printWaitPancake_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN;

            if (window.DialogResult == MessageBoxResult.OK)
            {

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

        private void txtWaitMgz_CST_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtWaitMgz_CST == null) return;
                InputMethod.SetPreferredImeConversionMode(txtWaitMgz_CST, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHist_CST_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtHist_CST == null) return;
                InputMethod.SetPreferredImeConversionMode(txtHist_CST, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCurrIn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                            {
                                pre.Content = chkAll;
                                e.Column.HeaderPresenter.Content = pre;
                                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            }
                            else
                            {
                                pre.Content = chkAll;
                                e.Column.HeaderPresenter.Content = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgCurrIn_UnloadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                            e.Column.HeaderPresenter.Content = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCurrIn.ItemsSource == null) return;

                DataTable dt = DataTableConverter.Convert(dgCurrIn.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    //if (Util.NVC(row["DISPATCH_YN"]).Equals("N") && !Util.NVC(row["WIPSTAT"]).Equals("PROC"))
                    //{
                    row["CHK"] = true;
                    //}
                }
                dgCurrIn.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCurrIn.ItemsSource == null) return;

                DataTable dt = DataTableConverter.Convert(dgCurrIn.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    row["CHK"] = false;
                }
                dgCurrIn.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInBoxInputHist_Click(object sender, RoutedEventArgs e)
        {
            if (wndInputCreate != null)
                wndInputCreate = null;

            wndInputCreate = new UC_IN_HIST_CREATE();
            wndInputCreate.FrameOperation = FrameOperation;

            if (wndInputCreate != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = EQPTSEGMENT;
                Parameters[1] = EQPTID;
                Parameters[2] = PROD_WODTLID;
                Parameters[3] = PROCID;
                Parameters[4] = null;
                Parameters[5] = null;
                Parameters[6] = PROD_LOTID;

                C1WindowExtension.SetParameters(wndInputCreate, Parameters);

                wndInputCreate.Closed += new EventHandler(wndInputCreate_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndInputCreate.ShowModal()));
                wndInputCreate.BringToFront();

            }
        }

        private void wndInputCreate_Closed(object sender, EventArgs e)
        {
            wndInputCreate = null;
            UC_IN_HIST_CREATE window = sender as UC_IN_HIST_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetInputHistory();
        }

        private bool GetStackUsingArea()
        {
            try
            {
                bool bRet = false;
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("CMCODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CMCDTYPE"] = "PKG_STK_PSTN_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "INDATA", "OUTDATA", dt);
                if (dtResult.Rows.Count > 0)
                {
                    bRet = true;
                    return bRet;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void tbxStackId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetStackInputHistory();
            }
        }

        private void btnStackSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStackInputHistory();
        }

        private void btnCancelStackInput_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCancel(dgStackInputHist))
                return;

            //투입취소 하시겠습니까?
            string messageId = "SFU1988";
            Util.MessageConfirm(messageId, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //BR_PRD_REG_CANCEL_TERMINATE_LOT
                    CancelStackInputHist();
                }
            });
        }

        private void CancelStackInputHist()
        {
            try
            {
                ShowParentLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_BASKET_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrlTable = indataSet.Tables["INLOT"];
                for (int i = dgStackInputHist.FrozenTopRowsCount; i < dgStackInputHist.Rows.Count - (dgStackInputHist.FrozenTopRowsCount + dgStackInputHist.FrozenBottomRowsCount); i++)
                {
                    if ((int)DataTableConverter.GetValue(dgStackInputHist.Rows[i].DataItem, "CHK") > 0)
                    {
                        newRow = inMtrlTable.NewRow();
                        newRow["INPUT_SEQNO"] = DataTableConverter.GetValue(dgStackInputHist.Rows[i].DataItem, "INPUT_SEQNO");
                        newRow["LOTID"] = DataTableConverter.GetValue(dgStackInputHist.Rows[i].DataItem, "INPUT_LOTID");
                        newRow["WIPQTY"] = DataTableConverter.GetValue(dgStackInputHist.Rows[i].DataItem, "INPUT_QTY");
                        inMtrlTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_LOT", "INDATA,INLOT", null, (serviceResult, serviceException) =>
                {
                    GetStackInputHistory();
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }
        }

        private bool CanCancel(C1DataGrid pDataGrid)
        {
            if (_Util.GetDataGridFirstRowIndexByCheck(pDataGrid, "CHK") < 0 || !CommonVerify.HasDataGridRow(pDataGrid))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private void dgStackInputHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_LOT_STAT_CODE") == null)
                    return;

                string state = DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_LOT_STAT_CODE").ToString();
                if (state == "START")
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
            }));
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1587"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void SetDefect(bool bShowMsg = true)
        {
            try
            {
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
                    newRow["LOTID"] = _LotID.Trim();
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

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
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
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }
            if (_LotID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
                return false;
            }

            return true;
        }

        private void GetDefectInfo()
        {
            try
            {
                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                //newRow["ACTID"] = "DEFECT_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT"; //"DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgDefect, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

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
                    if (Util.NVC(e.Cell.Column.Name) != "ACTNAME")
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
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


        private void SetInputHistButtonControls()
        {
            try
            {
                bool bRet = false;
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("ATTRIBUTE1", typeof(string));
                dt.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INPUT_LOT_CANCEL_TERM_USE";
                dr["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
                dr["ATTRIBUTE2"] = PROCID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", dt);
                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE3"]).Trim().Equals("Y"))
                {
                    btnInBoxInputHist.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInBoxInputHist.Visibility = Visibility.Collapsed;
                }

                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE5"]).Trim().Equals("Y"))
                {
                    btnInBoxInputCancel.Visibility = Visibility.Visible;
                    btnInputBoxCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;
                    btnInputBoxCancel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region EDC BCR Reading Info
        private bool bSetEdcAutoSelTime = false;
        private Storyboard sbBcrWarning = new Storyboard();
        private SolidColorBrush edcBrush = new SolidColorBrush(Colors.DarkOrange);

        private System.Windows.Threading.DispatcherTimer dspTmr_EdcWarn = new System.Windows.Threading.DispatcherTimer();

        private void HideBcrWarning()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdBcrWarning.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);

                sbBcrWarning.Stop();
            }));
        }

        private void ShowBcrWarning()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdBcrWarning.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                BcrWarnAnimationInRectangle(recBcrWarning);
            }));
        }

        private void BcrWarnAnimationInRectangle(System.Windows.Shapes.Rectangle rec)
        {
            rec.Fill = edcBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(1.1);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "BCR_WARNING");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));

            sbBcrWarning.Children.Add(opacityAnimation);
            sbBcrWarning.Begin(this);
        }

        private void GetBcrReadingRate(bool bChgEqptID)
        {
            try
            {
                if (EQPTID.Equals("") || EQPTID.IndexOf("SELECT") >= 0)
                {
                    HideBcrWarning();
                    Util.gridClear(dgEDCInfo);
                    return;
                }

                Edc_LoadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EDC_EQPT_BCR_READ_RATE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Select("WARN_DISP = 'Y'").Length > 0)
                        {
                            ShowBcrWarning();

                            if (bChgEqptID)
                                tbEDCBCRInfo.IsSelected = true;
                        }
                        else
                            HideBcrWarning();

                        Util.GridSetData(dgEDCInfo, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        Edc_LoadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                Edc_LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dspTmr_EdcWarn_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (tbEDCBCRInfo.Visibility != Visibility.Visible) return;

                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    GetBcrReadingRate(false);
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

        private void cboEdcAutoSearch_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dspTmr_EdcWarn != null)
                {
                    dspTmr_EdcWarn.Stop();

                    int iSec = 0;

                    if (cboEdcAutoSearch != null && cboEdcAutoSearch.SelectedValue != null && !cboEdcAutoSearch.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboEdcAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && bSetEdcAutoSelTime)
                    {
                        dspTmr_EdcWarn.Interval = new TimeSpan(0, 0, iSec);
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetEdcAutoSelTime = true;
                        return;
                    }

                    dspTmr_EdcWarn.Interval = new TimeSpan(0, 0, iSec);
                    dspTmr_EdcWarn.Start();

                    bSetEdcAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void CheckEDCVisibity()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = _ProcID;
                dtRow["EQSGID"] = _EqptSegment;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("BCD_READ_RATE_DISP_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["BCD_READ_RATE_DISP_FLAG"]).Equals("Y"))
                    {
                        tbEDCBCRInfo.Visibility = Visibility.Visible;

                        if (dspTmr_EdcWarn != null && dspTmr_EdcWarn.Interval.TotalSeconds > 0)
                            dspTmr_EdcWarn.Start();
                    }
                    else
                    {
                        tbEDCBCRInfo.Visibility = Visibility.Collapsed;

                        if (dspTmr_EdcWarn != null)
                            dspTmr_EdcWarn.Stop();
                    }
                }
                else
                {
                    tbEDCBCRInfo.Visibility = Visibility.Collapsed;

                    if (dspTmr_EdcWarn != null)
                        dspTmr_EdcWarn.Stop();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dspTmr_EdcWarn != null)
                    this.dspTmr_EdcWarn.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void SetInputHistToolWindow()
        {
            if (grdInputHistTool.Children.Count == 0)
            {
                winInputHistTool.FrameOperation = FrameOperation;

                winInputHistTool._UCParent = this;
                grdInputHistTool.Children.Add(winInputHistTool);

                CheckHistToolVisibity();
            }
        }

        public void CheckHistToolVisibity()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("ATTRIBUTE1", typeof(string));
                inTable.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["CMCDTYPE"] = "ASSY_TOOL_INFO_MGT";
                dtRow["ATTRIBUTE1"] = LoginInfo.CFG_AREA_ID;
                dtRow["ATTRIBUTE2"] = _ProcID;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tbInputHistTool.Visibility = Visibility.Visible;
                }
                else
                {
                    tbInputHistTool.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetInputHistTool()
        {
            if (winInputHistTool == null)
                return;

            winInputHistTool.EQPTID = EQPTID;
            winInputHistTool.PROD_LOTID = PROD_LOTID;
            winInputHistTool.GetInputHistTool();
        }

        private void ClearInputHistTool()
        {
            if (tbInputHistTool.Visibility == Visibility.Collapsed || winInputHistTool == null)
                return;

            winInputHistTool.EQPTID = EQPTID;
            winInputHistTool.PROD_LOTID = PROD_LOTID;
            winInputHistTool.ClearInfo();
        }

        private void txtWait_CST_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotID = GetLotIDbyCSTID(Util.NVC(txtWait_CST.Text), "WAIT");

                    if (!string.IsNullOrEmpty(sLotID))
                    {
                        txtWaitPancakeLot.Text = sLotID;
                        txtWaitPancakeLot_KeyDown(txtWaitPancakeLot, e);
                    }
                    else
                    {
                        txtWait_CST.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    txtWait_CST.Text = "";
                }
            }
        }
    }
}