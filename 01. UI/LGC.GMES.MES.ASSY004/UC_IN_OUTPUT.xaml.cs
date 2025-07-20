/*************************************************************************************
 Created Date : 2019.04.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정진척화면의 투입 부분 공통 화면 (ASSY0001.UC_IN_OUTPUT 2019/04/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.15  INS 김동일K : Initial Created.
  2019.09.25  LG CNS 김대근 : 금형관리 탭 추가
  2021.11.09  CNS 김지은C : CT검사 공정 추가
  2022.10.07  최재욱 : [C20221006-000307] CT검사, WIP_QTY_DIFF Header 하드코딩 주석 처리
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
using LGC.GMES.MES.COM001;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY004
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
        private DateTime _Min_Valid_Date;

        private bool _StackingYN = false;

        public UserControl _UCParent;     // Caller

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private COM001_314_HIST winInputHistTool = new COM001_314_HIST();

        //UC_IN_HIST_CREATE wndInputCreate;

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
                
                if (PROCID.Equals(Process.LAMINATION))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Visible;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    dgInputHist.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed; 

                    dgCurrIn.Columns["PRDT_CLSS3_CODE_NAME"].Visibility = Visibility.Collapsed;
                    dgCurrIn.Columns["REVS_MOUNT_FLAG_NAME"].Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals(Process.STACKING_FOLDING))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Visible;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;

                    grdMagTypeCntInfo.Visibility = Visibility.Visible;

                    if (_StackingYN)
                    {

                        // 자동자 ZZS의 경우 셀타입 종류 변경, 셀타입 정의시 변경-- AUTO_ZZS_CELL_TYPE
                        if (_Util.IsCommonCodeUse("AUTO_ZZS_EQPT", EQPTID))
                        {

                            // 변수 선언
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.Columns.Add("LANGID", typeof(string));
                            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                            DataRow newRow = RQSTDT.NewRow();
                            newRow["LANGID"] = LoginInfo.LANGID;
                            newRow["CMCDTYPE"] = "AUTO_ZZS_CELL_TYPE";

                            RQSTDT.Rows.Add(newRow);

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);


                            DataRow row1 = dtResult.Rows[0];
                            rdoMagAType.Content = row1["CBO_NAME"];
                            rdoMagAType.Tag = row1["CBO_CODE"]; 
                            tbATypeTitle.Text = row1["CBO_NAME"].ToString();

                            DataRow row2 = dtResult.Rows[1];
                            rdoMagCtype.Content = row2["CBO_NAME"];
                            rdoMagCtype.Tag = row2["CBO_CODE"]; 
                            tbCTypeTitle.Text = row2["CBO_NAME"].ToString();

                        }
                        else
                        {
                            rdoMagAType.Content = "HALFTYPE";
                            rdoMagAType.Tag = "HC";
                            tbATypeTitle.Text = "HALFTYPE";

                            rdoMagCtype.Content = "MONOTYPE";
                            rdoMagCtype.Tag = "MC";
                            tbCTypeTitle.Text = "MONOTYPE";

                        }

                        // 오창 소형3동 해당 버튼 사용 요청으로 추가.
                        if (LoginInfo.CFG_AREA_ID.Equals("A9"))
                        {
                            btnWaitMagRework.Visibility = Visibility.Visible;
                            dgWaitMagazine.Columns["AUTO_STOP_FLAG"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            btnWaitMagRework.Visibility = Visibility.Collapsed;
                            dgWaitMagazine.Columns["AUTO_STOP_FLAG"].Visibility = Visibility.Collapsed;
                        }

                        dgInputHist.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                        dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                        dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                        //dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgInputHist.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                        dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                        dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    }

                    if (dgInputHist.TopRows.Count > 1)
                    {
                        dgInputHist.TopRows[0].Visibility = Visibility.Collapsed;
                        dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Header = "자공정LOSS"; // ObjectDic.Instance.GetObjectName("자공정LOSS");
                    }

                    dgCurrIn.Columns["PRDT_CLSS3_CODE_NAME"].Visibility = Visibility.Visible;
                    dgCurrIn.Columns["REVS_MOUNT_FLAG_NAME"].Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals(Process.CT_INSP))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Visible;
                    btnCreateReworkBox.Visibility = Visibility.Visible; //CT검사는 재작업BOX생성 사용
                    tbInBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInputHistTool.Visibility = Visibility.Collapsed;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    dgInputHist.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;

                    if (dgInputHist.TopRows.Count > 1)
                    {
                        dgInputHist.TopRows[0].Visibility = Visibility.Collapsed;
                        //dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Header = "WIP_QTY_DIFF";// ObjectDic.Instance.GetObjectName("전공정 LOSS");
                    }

                    dgCurrIn.Columns["PRDT_CLSS3_CODE_NAME"].Visibility = Visibility.Collapsed;
                    dgCurrIn.Columns["REVS_MOUNT_FLAG_NAME"].Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals(Process.PACKAGING))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Visible;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Visible;
                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    dgInputHist.Columns["CUT_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;

                    if (dgInputHist.TopRows.Count > 1)
                    {
                        dgInputHist.TopRows[0].Visibility = Visibility.Collapsed;
                        dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Header = "WIP_QTY_DIFF";// ObjectDic.Instance.GetObjectName("전공정 LOSS");
                    }

                    dgCurrIn.Columns["PRDT_CLSS3_CODE_NAME"].Visibility = Visibility.Collapsed;
                    dgCurrIn.Columns["REVS_MOUNT_FLAG_NAME"].Visibility = Visibility.Collapsed;
                }                
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

        protected virtual void GetParentProductLot()
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

            #region [버튼 권한 적용에 따른 처리]
            // 자재 투입 tab
            grdCurrTranBtn.Visibility = Visibility.Collapsed;

            // 대기 Pancake tab
            btnWaitPancakeInPut.Visibility = Visibility.Collapsed;

            // 대기 매거진 tab
            grdWaitMgTranBtn.Visibility = Visibility.Collapsed;

            // 대기 바구니 tab
            grdWaitBoxTranBtn.Visibility = Visibility.Collapsed;

            // 투입 바구니 이력 tab
            btnInputBoxCancel.Visibility = Visibility.Collapsed;

            // 투입 이력 tab
            btnInBoxInputCancel.Visibility = Visibility.Collapsed;
            #endregion

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

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    ASSY004_INPUT_CANCEL_CST wndCancel = new ASSY004_INPUT_CANCEL_CST();
                    wndCancel.FrameOperation = FrameOperation;

                    if (wndCancel != null)
                    {
                        int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                        object[] Parameters = new object[13];
                        Parameters[0] = EQPTSEGMENT;
                        Parameters[1] = EQPTID;
                        Parameters[2] = PROCID;
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_NAME"));
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "CSTID"));
                        Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
                        Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MTRLID"));
                        Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_STAT_CHG_DTTM"));
                        Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        Parameters[10] = "CURR";
                        Parameters[11] = "";
                        Parameters[12] = PROD_LOTID;
                        
                        C1WindowExtension.SetParameters(wndCancel, Parameters);

                        wndCancel.Closed += new EventHandler(wndCancel_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndCancel.ShowModal()));                        
                    }
                }
                else
                {
                    //투입취소 하시겠습니까?
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
                        txtCurrInLotID.Text = "";
                        return;
                    }

                    string sInPos = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string sInPosName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));

                    if (PROCID.Equals(Process.PACKAGING) && Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    {
                        OnCurrAutoInputLot(txtCurrInLotID.Text.Trim(), sInPos, "", "");
                        
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
                                
                                txtCurrInLotID.Text = "";
                            }
                        }, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtCurrInLotID.Text = "";
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

        private void btnCurrInEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInReplace())
                    return;

                if(CanUnmtWaitMtrl())
                    return;


                ASSY004_COM_INPUT_LOT_END wndEnd = new ASSY004_COM_INPUT_LOT_END();
                wndEnd.FrameOperation = FrameOperation;

                if (wndEnd != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                    object[] Parameters = new object[12];
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
                    Parameters[10] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "AUTO_STOP_FLAG"));
                    Parameters[11] = PROD_LOTID;
                    C1WindowExtension.SetParameters(wndEnd, Parameters);

                    wndEnd.Closed += new EventHandler(wndEnd_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndEnd.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [대기 Pancake]

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

        private void txtWaitPancakeLot_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtWaitPancakeLot == null) return;
                InputMethod.SetPreferredImeConversionMode(txtWaitPancakeLot, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
        #endregion

        #region [대기 매거진]

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
                    //if (txtWaitMazID.Text.Length != 10)
                    //{
                    //    //Util.Alert("MAGAZINE ID 자릿수(10자리)가 맞지 않습니다.");
                    //    Util.MessageValidation("SFU1391");
                    //    return;
                    //}

                    GetWaitMagazine(txtWaitMazID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                ASSY004_005_MAGAZINE_CREATE wndMAZCreate = new ASSY004_005_MAGAZINE_CREATE();
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
                    
                    this.Dispatcher.BeginInvoke(new Action(() => wndMAZCreate.ShowModal()));                    
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

        private void txtWaitMazID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtWaitMazID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtWaitMazID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [대기 바구니]

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

        private void btnCreateReworkBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreateReworkBox())
                    return;

                ASSY004_007_RWK_BOX_CREATE wndBoxCreate = new ASSY004_007_RWK_BOX_CREATE();
                wndBoxCreate.FrameOperation = FrameOperation;

                if (wndBoxCreate != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = PROD_WOID;
                    Parameters[3] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[4] = PROCID;

                    C1WindowExtension.SetParameters(wndBoxCreate, Parameters);

                    wndBoxCreate.Closed += new EventHandler(wndBoxCreate_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndBoxCreate.ShowModal()));
                }
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

        private void txtHistLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtHistLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtHistLotID, ImeConversionModeValues.Alphanumeric);
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

                    ASSY004_INPUT_CANCEL_CST wndCancel = new ASSY004_INPUT_CANCEL_CST();
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

                        this.Dispatcher.BeginInvoke(new Action(() => wndCancel.ShowModal()));
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

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
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

                    ASSY004_INPUT_CANCEL_CST wndCancel = new ASSY004_INPUT_CANCEL_CST();
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

                        this.Dispatcher.BeginInvoke(new Action(() => wndCancel.ShowModal()));                       
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

        private void SetInputHistToolWindow()
        {
            if (grdInputHistTool.Children.Count == 0)
            {
                winInputHistTool.FrameOperation = FrameOperation;

                winInputHistTool._UCParent = this;
                grdInputHistTool.Children.Add(winInputHistTool);
            }
        }

        public void GetInputHistTool(string prodLotID, string eqptID)
        {
            if (winInputHistTool == null)
                return;

            winInputHistTool.EQPTID = eqptID;
            winInputHistTool.PROD_LOTID = prodLotID;
            winInputHistTool.GetInputHistTool();
        }

        public void GetCurrInList()
        {
            try
            {
                if (PROD_LOTID.Equals(""))
                {
                    //if (PROCID.Equals(Process.PACKAGING))
                    //    btnCurrInCancel.IsEnabled = true;
                    //else
                    //    btnCurrInCancel.IsEnabled = false;

                    //btnCurrInComplete.IsEnabled = false;
                    btnCurrInEnd.IsEnabled = false;
                }
                else
                {
                    //btnCurrInCancel.IsEnabled = true;
                    //btnCurrInComplete.IsEnabled = true;
                    btnCurrInEnd.IsEnabled = true;
                }

                string sBizNAme = string.Empty;

                if (PROCID.Equals(Process.LAMINATION))
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_LM_L";
                else
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_L";

                ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EQPTID;
                newRow["LOTID"] = PROD_LOTID;

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
                        
                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation, true);

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
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            try
            {
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
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
                        HideParentLoadingIndicator();

                        txtHistLotID.Text = "";
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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
                //HideParentLoadingIndicator();
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
                HideParentLoadingIndicator();
            }
        }

        #endregion

        #region [LAMINATION]

        public void GetWaitPancake()
        {
            try
            {
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation, true);

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
                        HideParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PancakeInput()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_ID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                inTable = indataSet.Tables["IN_INPUT"];

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgWaitPancake, "CHK");
                if (idx < 0)
                {
                    HideParentLoadingIndicator();
                    return;
                }

                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = cboPancakeMountPstnID.SelectedValue.ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[idx].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_LM_L", "IN_EQP,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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
                        HideParentLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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
                HideParentLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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
                HideParentLoadingIndicator();
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
                    sBizName = "DA_PRD_SEL_WAIT_MAG_ST_L";
                else
                    sBizName = "DA_PRD_SEL_WAIT_MAG_FD_L";

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

                // 자동자 ZZS의 경우 셀타입 종류 변경, 셀타입 정의시 변경-- DA_PRD_SEL_WAIT_MAG_ZZS_L
                if (_Util.IsCommonCodeUse("AUTO_ZZS_EQPT", EQPTID))
                {
                    sBizName = "DA_PRD_SEL_WAIT_MAG_ZZS_L";
                    newRow["PRODUCT_LEVEL2_CODE"] = "NC"; //자동차 ZZS의 경우 NC type(notched cut)
                    newRow["PRODUCT_LEVEL3_CODE"] = sElec;
                }



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
                        Util.GridSetData(dgWaitMagazine, bizResult, FrameOperation, true);

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
                        HideParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void MagazineInput()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_ID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                inTable = indataSet.Tables["IN_INPUT"];

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgWaitMagazine, "CHK");
                if (idx < 0)
                {
                    HideParentLoadingIndicator();
                    return;
                }

                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = cboMagMountPstnID.SelectedValue.ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitMagazine.Rows[idx].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_FD_L", "IN_EQP,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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
                        HideParentLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [STACKING]
        #endregion

        #region [CT INSP]

        #endregion

        #region [PACKAGING]

        public void GetWaitBox()
        {
            try
            {
                ShowParentLoadingIndicator();

                string bizRuleID = string.Empty;

                if (PROCID.Equals(Process.CT_INSP))
                    bizRuleID = "DA_PRD_SEL_WAIT_LOT_LIST_CI_L";
                else
                    bizRuleID = "DA_PRD_SEL_WAIT_LOT_LIST_CL_L";

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = PROCID;
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = PROD_WODTLID;

                // 기존 CT검사 공정진척에 있던 source 테스트를 위해 가져옴 2023.07.19
                if (PROCID.Equals(Process.CT_INSP) && LoginInfo.CFG_SHOP_ID == "G382")
                    newRow["PROCID"] = "A9000";
                // 기존 CT검사 공정진척에 있던 source 테스트를 위해 가져옴 2023.07.19

                inTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService(bizRuleID, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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


                        Util.GridSetData(dgWaitBox, searchResult, FrameOperation, true);

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
                        HideParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void BasketInput(bool bAuto, int iRow)
        {
            try
            {
                string bizRuleID = string.Empty;

                if (PROCID.Equals(Process.CT_INSP))
                    bizRuleID = "BR_PRD_CHK_INPUT_LOT_CI_L";
                else
                    bizRuleID = "BR_PRD_CHK_INPUT_LOT_CL_L";

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("INPUT_ID", typeof(string));               
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];

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
                    newRow["INPUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "LOTID"));
                    //newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "PRODID"));
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    //newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")));

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
                        newRow["INPUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "LOTID"));
                        //newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        //newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")));

                        inMtrlTable.Rows.Add(newRow);
                    }
                }

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleID, "IN_EQP,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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
                        HideParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_BOX_LIST_CL_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInputBox.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputBox, searchResult, FrameOperation, true);

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
                        HideParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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
                        HideParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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
                        HideParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
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
                HideParentLoadingIndicator();
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

            bRet = true;

            return bRet;
        }

        private bool CanUnmtWaitMtrl()
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_PSTN_GR_CODE")).Equals("S")
                && _Util.IsCommonCodeUse("INPUT_MTRL_WAIT_WIP_AREA", LoginInfo.CFG_AREA_ID))
            {
                try
                {
                    // 탈착처리 하시겠습니까?
                    Util.MessageConfirm("SFU5047", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            UnmtWaitMtrl();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UnmtWaitMtrl()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_ID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                inTable = indataSet.Tables["IN_INPUT"];

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");
                if (idx < 0)
                {
                    HideParentLoadingIndicator();
                    return;
                }

                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                newRow["EQPT_MOUNT_PSTN_STATE"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "MOUNT_PSTN_STAT_CODE"));
                newRow["INPUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNMOUNT_MTRL_LOT_WAIT", "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");    
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideParentLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
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

                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iRow].DataItem, "MOUNT_PSTN_GR_CODE")).Equals("S")
                && _Util.IsCommonCodeUse("INPUT_MTRL_WAIT_WIP_AREA", LoginInfo.CFG_AREA_ID))
                {
                    // 대기 투입위치에는 투입 처리할 수 없습니다.
                    Util.MessageValidation("SFU8298");
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

        private bool CanCreateReworkBox()
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
                //listAuth.Add(btnCurrInCancel);
                //listAuth.Add(btnCurrDelete);
                //listAuth.Add(btnCurrInComplete);
                listAuth.Add(btnWaitPancakeInPut);
                listAuth.Add(btnWaitMagInput);
                listAuth.Add(btnWaitBoxInPut);
                listAuth.Add(btnCreateReworkBox);
                listAuth.Add(btnInBoxInputCancel);
                listAuth.Add(btnCurrInEnd);
                listAuth.Add(btnWaitMagRework);

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

                #region EDC BCR Reading Info
                if (dgEDCInfo != null)
                    Util.gridClear(dgEDCInfo);
                #endregion

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
                    }
                    else
                    {
                        if (dgCurrIn.Columns.Contains("CSTID"))
                            dgCurrIn.Columns["CSTID"].Visibility = Visibility.Collapsed;                        
                    }
                }

                //대기 Pancake
                if (tbPancake.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgWaitPancake.Columns.Contains("CSTID"))
                            dgWaitPancake.Columns["CSTID"].Visibility = Visibility.Visible;

                    }
                    else
                    {
                        if (dgWaitPancake.Columns.Contains("CSTID"))
                            dgWaitPancake.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    }
                }

                //대기매거진
                if (tbMagazine.Visibility == Visibility.Visible)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        if (dgWaitMagazine.Columns.Contains("CSTID"))
                            dgWaitMagazine.Columns["CSTID"].Visibility = Visibility.Visible;

                    }
                    else
                    {
                        if (dgWaitMagazine.Columns.Contains("CSTID"))
                            dgWaitMagazine.Columns["CSTID"].Visibility = Visibility.Collapsed;                        
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
                        
                    }
                    else
                    {
                        if (dgInputHist.Columns.Contains("CSTID"))
                            dgInputHist.Columns["CSTID"].Visibility = Visibility.Collapsed;
                        
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
                    Util.MessageValidation("SFU6051");  //카세트ID를 입력 하세요.
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

        private void HideParentLoadingIndicator()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("HideLoadingIndicator");
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetPermissionPerButton(string sGrpCode)
        {
            try
            {
                switch (sGrpCode)
                {
                    case "INPUT_W": // 투입 사용 권한
                        if (tbCurrIn.Visibility == Visibility.Visible)
                            grdCurrTranBtn.Visibility = Visibility.Visible;
                        break;
                    case "WAIT_W": // 대기 사용 권한
                        if (tbPancake.Visibility == Visibility.Visible)
                            btnWaitPancakeInPut.Visibility = Visibility.Visible;
                        if (tbMagazine.Visibility == Visibility.Visible)
                            grdWaitMgTranBtn.Visibility = Visibility.Visible;
                        if (tbBox.Visibility == Visibility.Visible)
                            grdWaitBoxTranBtn.Visibility = Visibility.Visible;
                        break;
                    case "INPUTHIST_W": // 투입이력 사용 권한      
                        if (tbInBox.Visibility == Visibility.Visible)
                            btnInputBoxCancel.Visibility = Visibility.Visible;
                        if (tbHist.Visibility == Visibility.Visible)
                            btnInBoxInputCancel.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }

                // 투입이력의 투입취소 기능 제거요청에 의한 변경.
                btnInBoxInputCancel.Visibility = Visibility.Collapsed;
                btnInputBoxCancel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void InitializePermissionPerInputButton()
        {
            try
            {
                //if (tbCurrIn.Visibility == Visibility.Visible)
                    grdCurrTranBtn.Visibility = Visibility.Collapsed;

                //if (tbPancake.Visibility == Visibility.Visible)
                    btnWaitPancakeInPut.Visibility = Visibility.Collapsed;
                //if (tbMagazine.Visibility == Visibility.Visible)
                    grdWaitMgTranBtn.Visibility = Visibility.Collapsed;
                //if (tbBox.Visibility == Visibility.Visible)
                    grdWaitBoxTranBtn.Visibility = Visibility.Collapsed;

                //if (tbInBox.Visibility == Visibility.Visible)
                    btnInputBoxCancel.Visibility = Visibility.Collapsed;
                //if (tbHist.Visibility == Visibility.Visible)
                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;
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
            ASSY004_004_PAN_REPLACE window = sender as ASSY004_004_PAN_REPLACE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_INPUT_LOT_END window = sender as ASSY004_COM_INPUT_LOT_END;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }        

        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            ASSY004_005_MAGAZINE_CREATE window = sender as ASSY004_005_MAGAZINE_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetWaitMagazine();
        }
        
        private void wndCancel_Closed(object sender, EventArgs e)
        {
            ASSY004_INPUT_CANCEL_CST window = sender as ASSY004_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetCurrInList();
            }
        }

        private void wndInputHistBoxCancel(object sender, EventArgs e)
        {
            ASSY004_INPUT_CANCEL_CST window = sender as ASSY004_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetInBoxList();
            }
        }

        private void wndInputHistCancel(object sender, EventArgs e)
        {
            ASSY004_INPUT_CANCEL_CST window = sender as ASSY004_INPUT_CANCEL_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetInputHistory();
            }
        }

        private void printWaitMaz_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void printWaitPancake_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndBoxCreate_Closed(object sender, EventArgs e)
        {
            ASSY004_007_RWK_BOX_CREATE window = sender as ASSY004_007_RWK_BOX_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetWaitBox();
        }
        #endregion

        #endregion

        #endregion



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
        
    }
}
