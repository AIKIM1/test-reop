/*************************************************************************************
 Created Date : 2017.01.23
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척화면의 투입 & 생산 부분 공통 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.23  INS 정문교C : Initial Created.
  2020.06.26  김동일      C20200625-000271 기준정보를 통한 투입관련 Tab의 투입 및 투입취소버튼 제어기능 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// UC_IN_OUTPUT_MOBILE_UNUSUAL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_IN_OUTPUT_MOBILE_UNUSUAL : UserControl, IWorkArea
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
        private string _MountPstnGrCode = string.Empty;
        private bool _isCstIdNeed = false;
        private bool _StackingYN = false;

        private string _Max_Pre_Proc_End_Day = string.Empty;

        private DateTime _Min_Valid_Date;

        public UserControl _UCParent;     // Caller

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        UC_IN_HIST_CREATE wndInputCreate;

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
        public string PROD_NAME { get; set; }
        public string PROD_ID { get; set; }
        public string MOUNT_PSTN_GR_CODE
        {
            get { return _MountPstnGrCode; }
            set { _MountPstnGrCode = value; }
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

        #region Popup 처리 로직 변경
        ASSY003_PAN_REPLACE wndPanReplace;
        ASSY003_MAGAZINE_CREATE wndMazCreate;
        #endregion
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
        public UC_IN_OUTPUT_MOBILE_UNUSUAL(string sProcID, string sEqsgID, string sEqptID, bool bStacking = false)
        {
            PROCID = sProcID;
            EQPTSEGMENT = sEqsgID;
            EQPTID = sEqptID;
            _StackingYN = bStacking;

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
                String[] sFilter1 = { EQPTID, "PROD", null };
                String[] sFilter2 = { EQPTID, null }; // 자재,제품 전체

                _combo.SetCombo(cboMagMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_SRC");
                _combo.SetCombo(cboMagMountPstnIDSTP, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_STP");

                if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.SSC_BICELL))
                {
                    // SRC
                    tbMagazineSTP.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;
                    if (dgInputHist.Columns.Contains("CHK"))
                        dgInputHist.Columns["CHK"].Visibility = Visibility.Collapsed;


                    if (dgCurrIn != null && dgCurrIn.Columns.Contains("BICELL_LEVEL3_NAME") && dgCurrIn.Columns.Contains("BICELL_LEVEL3_CODE"))
                    {
                        dgCurrIn.Columns["BICELL_LEVEL3_CODE"].Visibility = Visibility.Collapsed;
                        dgCurrIn.Columns["BICELL_LEVEL3_NAME"].Visibility = Visibility.Collapsed;
                    }
                }
                else if (PROCID.Equals(Process.STP))
                {
                    // STP
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_STP");

                    string[] sFilter5 = { "PRDT_SIZE" };
                    _combo.SetCombo(cboSizeSTP, CommonCombo.ComboStatus.NA, sFilter: sFilter5, sCase: "COMMCODE");

                    string[] sFilter6 = { "PRDT_DIRCTN" };
                    _combo.SetCombo(cboDirectionSTP, CommonCombo.ComboStatus.NA, sFilter: sFilter6, sCase: "COMMCODE");

                    if (dgCurrIn != null && dgCurrIn.Columns.Contains("PRDT_CLSS_CODE"))
                    {
                        dgCurrIn.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Collapsed;
                    }
                }
                else if (PROCID.Equals(Process.SSC_FOLDED_BICELL))
                {
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbMagazineSTP.Visibility = Visibility.Collapsed;

                    _combo.SetCombo(cboBoxMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                    if (dgCurrIn != null && dgCurrIn.Columns.Contains("BICELL_LEVEL3_NAME") && dgCurrIn.Columns.Contains("BICELL_LEVEL3_CODE"))
                    {
                        dgCurrIn.Columns["BICELL_LEVEL3_CODE"].Visibility = Visibility.Collapsed;
                        dgCurrIn.Columns["BICELL_LEVEL3_NAME"].Visibility = Visibility.Collapsed;
                    }
                }

                if (PROCID.Equals(Process.STP) || PROCID.Equals(Process.SRC) )
                {
                    dgCurrIn.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["CSTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgCurrIn.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["CSTID"].Visibility = Visibility.Collapsed;
                }

                txtProdCurrIn.Text = string.Empty;
                txtProdMagazine.Text = string.Empty;
                txtProdHist.Text = string.Empty;

                //if (_ProcID.Equals(Process.STP))
                //    cboMagMountPstnIDSTP.SelectedValueChanged += cboMagMountPstnIDSTP_SelectedValueChanged;

                // 투입이력 투입취소 버튼 사용 설정
                SetInputHistButtonControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경
            if (wndPanReplace != null)
                wndPanReplace.BringToFront();

            if (wndMazCreate != null)
                wndMazCreate.BringToFront();
            #endregion

            ApplyPermissions();
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitializeControls();
        }
        #endregion

        #region [Virtual Function]
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
        protected virtual void OnCurrAutoInputLot(string sInputLot, string sPstnID, double dInQty)
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
                    else if (i == 2) parameterArrys[i] = dInQty;
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

        #region [자재투입, 투입튀소]
        /// <summary>
        /// 투입취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCurrInCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInCancel())
                    return;

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
                                ininDataTable.Rows.Add(newRow);
                            }
                        }

                        OnCurrInCancel(ininDataTable);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 잔량처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCurrInReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInReplace())
                    return;

                if (wndPanReplace != null)
                    wndPanReplace = null;

                wndPanReplace = new ASSY003_PAN_REPLACE();
                wndPanReplace.FrameOperation = FrameOperation;

                if (wndPanReplace != null)
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
                    Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "PROD_LOTID"));
                    Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "CSTID"));
                    C1WindowExtension.SetParameters(wndPanReplace, Parameters);

                    wndPanReplace.Closed += new EventHandler(wndReplace_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPanReplace.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void wndReplace_Closed(object sender, EventArgs e)
        {
            wndPanReplace = null;
            ASSY003_PAN_REPLACE window = sender as ASSY003_PAN_REPLACE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }
        /// <summary>
        /// 투입완료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        DataTable ininDataTable = new DataTable();

                        ininDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        ininDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        ininDataTable.Columns.Add("MTRLID", typeof(string));
                        ininDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        ininDataTable.Columns.Add("OUTPUT_QTY", typeof(int));
                        ininDataTable.Columns.Add("WIPNOTE", typeof(string));
                        ininDataTable.Columns.Add("INPUT_SEQNO", typeof(Int64));

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
                                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));

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

        /// <summary>
        /// ############# 사용 제외 #############
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        return;

                    string sInPos = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    string sInPosName = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));

                    // 대기 매거진 조회에서 찾는다.  조회 이후 발생된 대기 매거진일경우 문제됨..  biz 호출 필요
                    int iRow = _Util.GetDataGridRowIndex(dgWaitMagazine, "LOTID", txtCurrInLotID.Text);

                    double dInQty = Convert.ToDouble(DataTableConverter.GetValue(dgWaitMagazine.Rows[iRow].DataItem, "WIPQTY").ToString());

                    ////if (PROCID.Equals(Process.PACKAGING) && Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                    ////{
                    ////    OnCurrAutoInputLot(txtCurrInLotID.Text.Trim(), sInPos, "", "");

                    ////    txtCurrInLotID.Text = "";
                    ////}
                    ////else
                    ////{

                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("{0} 위치에 {1} 을 투입 하시겠습니까?", sInPosName, txtCurrInLotID.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    object[] parameters = new object[2];
                    parameters[0] = sInPosName;
                    parameters[1] = txtCurrInLotID.Text.Trim();

                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            OnCurrAutoInputLot(txtCurrInLotID.Text.Trim(), sInPos, dInQty);

                            txtCurrInLotID.Text = "";
                        }
                    }
                    );
                    ////}
                }
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
                if (e == null || e.Column == null || e.Column.HeaderPresenter == null)
                    return;

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
        private void dgCurrIn_CommittedEdit(object sender, DataGridCellEventArgs e)
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
            if (dgCurrIn.ItemsSource == null) return;

            DataTable dt = ((DataView)dgCurrIn.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                for (int idx = 0; idx < dgCurrIn.Rows.Count; idx++)
                {
                    row["CHK"] = true;
                }
            }

            dt.AcceptChanges();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgCurrIn.ItemsSource == null) return;

            DataTable dt = ((DataView)dgCurrIn.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                for (int idx = 0; idx < dgCurrIn.Rows.Count; idx++)
                {
                    row["CHK"] = false;
                }
            }

            dt.AcceptChanges();
        }

        #endregion

        #region [대기매거진 SRC, STP]
        private void txtWaitMazID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    TextBox tb = sender as TextBox;

                    //if (tb.Text.Length != 10)
                    //{
                    //    //Util.Alert("MAGAZINE ID 자릿수(10자리)가 맞지 않습니다.");
                    //    Util.MessageValidation("SFU1391");
                    //    return;
                    //}

                    GetWaitMagazine(tb.Text);
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

                // PROD_WODTLID LOT 선택시 선택 LOT의 WO_DETL_ID 정보
                // 대기 매거진 재구성은 선택 LOT과 상관없이 호출 -> 설비의 W/O 정보를 갖고 온다
                string sWodetlid = GetWoDetlID();

                if (wndMazCreate != null)
                    wndMazCreate = null;

                wndMazCreate = new ASSY003_MAGAZINE_CREATE();
                wndMazCreate.FrameOperation = FrameOperation;

                if (wndMazCreate != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = PROD_WOID;
                    Parameters[3] = PROCID;
                    Parameters[4] = string.IsNullOrWhiteSpace(sWodetlid) ? null : sWodetlid;
                    
                    C1WindowExtension.SetParameters(wndMazCreate, Parameters);

                    wndMazCreate.Closed += new EventHandler(wndMAZCreate_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndMazCreate.ShowModal()));                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            wndMazCreate = null;
            ASSY003_MAGAZINE_CREATE window = sender as ASSY003_MAGAZINE_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetWaitMagazine();
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
        private void cboMagMountPstnIDSTP_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitMagazine();
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

        #endregion

        #region User Method

        #region [BizCall]
        public void GetCurrInList()
        {
            try
            {
                chkAll.IsChecked = false;

                ////// 메인 LOT이 없는 경우 disable 처리..
                ////if (PROD_LOTID.Equals(""))
                ////{
                ////    btnCurrInCancel.IsEnabled = false;
                ////    btnCurrInComplete.IsEnabled = false;
                ////}
                ////else
                ////{
                ////    btnCurrInCancel.IsEnabled = true;
                ////    btnCurrInComplete.IsEnabled = true;
                ////}

                DataTable inTable = new DataTable();
                string sBizNAme = string.Empty;

                if (PROCID.Equals(Process.SRC))
                {
                    inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LIST_SRC();
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_SRC";
                }
                else if (PROCID.Equals(Process.STP))
                {
                    inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LM();
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_STP";
                }
                else if (PROCID.Equals(Process.SSC_BICELL))
                {
                    inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LIST_SSCBI();
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_SSCBI";
                }
                else
                {
                    inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LM();
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST";
                }

                ShowParentLoadingIndicator();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EQPTID;

                if (PROCID.Equals(Process.SRC))
                    newRow["MOUNT_PSTN_GR_CODE"] = MOUNT_PSTN_GR_CODE;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.AlertByBiz(sBizNAme, searchException.Message, searchException.ToString());
                            return;
                        }

                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation);

                        dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                        dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                        dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;

                        if (dgCurrIn.CurrentCell != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.CurrentCell.Row.Index, dgCurrIn.Columns.Count - 1);
                        else if (dgCurrIn.Rows.Count > 0 && dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1) != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1);
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation);

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
        public void GetWaitMagazine(string sLot = "")
        {
            try
            {
                ShowParentLoadingIndicator();

                string sBizName = string.Empty;
                DataTable inTable = new DataTable();
                DataRow newRow;

                if (PROCID.Equals(Process.SRC))
                {
                    inTable = _Biz.GetDA_PRD_SEL_WAIT_MAG_SRC();

                    newRow = inTable.NewRow();
                    sBizName = "DA_PRD_SEL_WAIT_MAG_SRC";
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = PROCID;
                    newRow["EQSGID"] = EQPTSEGMENT;
                    newRow["EQPTID"] = EQPTID;
                    //newRow["WO_DETL_ID"] = PROD_WODTLID;
                    newRow["WOID"] = PROD_WOID;

                    if (!sLot.Equals(""))
                        newRow["LOTID"] = sLot;
                }
                else if (PROCID.Equals(Process.STP))
                {
                    // View 에 SIZE/방향 콤보 설정 (3D 모델 여부에 따른..)
                    bool b3DModel = GetWoProdInfo();

                    if (b3DModel) // 3D Model
                    {
                        grdWatMagSTP.RowDefinitions[1].Height = new GridLength(35);
                        grdWatMagSTP.RowDefinitions[2].Height = new GridLength(4);

                        // Control 이동..                        
                        txtMagSTP.SetValue(Grid.RowProperty, 3);
                        txtMagSTP.SetValue(Grid.ColumnProperty, 0);
                        txtWaitMazIDSTP.SetValue(Grid.RowProperty, 3);
                        txtWaitMazIDSTP.SetValue(Grid.ColumnProperty, 2);

                        txtInputPstnSTP.SetValue(Grid.RowProperty, 1);
                        txtInputPstnSTP.SetValue(Grid.ColumnProperty, 8);
                        cboMagMountPstnIDSTP.SetValue(Grid.RowProperty, 1);
                        cboMagMountPstnIDSTP.SetValue(Grid.ColumnProperty, 10);
                    }
                    else
                    {
                        grdWatMagSTP.RowDefinitions[1].Height = new GridLength(0);
                        grdWatMagSTP.RowDefinitions[2].Height = new GridLength(0);

                        // Control 이동..                        
                        txtMagSTP.SetValue(Grid.RowProperty, 3);
                        txtMagSTP.SetValue(Grid.ColumnProperty, 4);
                        txtWaitMazIDSTP.SetValue(Grid.RowProperty, 3);
                        txtWaitMazIDSTP.SetValue(Grid.ColumnProperty, 6);

                        txtInputPstnSTP.SetValue(Grid.RowProperty, 3);
                        txtInputPstnSTP.SetValue(Grid.ColumnProperty, 0);
                        cboMagMountPstnIDSTP.SetValue(Grid.RowProperty, 3);
                        cboMagMountPstnIDSTP.SetValue(Grid.ColumnProperty, 2);
                    }


                    inTable = _Biz.GetDA_PRD_SEL_WAIT_MAG_STP();

                    DataRowView rowview = cboMagMountPstnIDSTP.SelectedItem as DataRowView;

                    newRow = inTable.NewRow();
                    sBizName = "DA_PRD_SEL_WAIT_MAG_STP";
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = PROCID;
                    newRow["EQSGID"] = EQPTSEGMENT;
                    newRow["EQPTID"] = EQPTID;
                    newRow["PRODID"] = null;
                    if (!sLot.Equals(""))
                        newRow["LOTID"] = sLot;
                    newRow["BICELL_LEVEL3_CODE"] = rowview["BICELL_LEVEL3_CODE"].ToString().Equals("") ? null : rowview["BICELL_LEVEL3_CODE"].ToString();

                    if (b3DModel && cboSizeSTP != null && !Util.NVC(cboSizeSTP.SelectedValue).Equals(""))
                        newRow["PRDT_SIZE"] = cboSizeSTP.SelectedValue.ToString();

                    if (b3DModel && cboDirectionSTP != null && !Util.NVC(cboDirectionSTP.SelectedValue).Equals(""))
                        newRow["PRDT_DIRCTN"] = cboDirectionSTP.SelectedValue.ToString();
                }
                else
                {
                    // SSC Bi-Cell
                    inTable = _Biz.GetDA_PRD_SEL_WAIT_MAG_SSCBI();

                    //DataRowView rowview = cboMagMountPstnIDSTP.SelectedItem as DataRowView;

                    newRow = inTable.NewRow();
                    sBizName = "DA_PRD_SEL_WAIT_MAG_SSCBI";
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = PROCID;
                    newRow["EQSGID"] = EQPTSEGMENT;
                    newRow["EQPTID"] = EQPTID;
                    //newRow["WO_DETL_ID"] = PROD_WODTLID;
                    if (!sLot.Equals(""))
                        newRow["LOTID"] = sLot;
                }

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (bizResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(bizResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }

                        if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.SSC_BICELL))
                        {
                            Util.GridSetData(dgWaitMagazine, bizResult, FrameOperation);

                            if (dgWaitMagazine.CurrentCell != null)
                                dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.CurrentCell.Row.Index, dgWaitMagazine.Columns.Count - 1);
                            else if (dgWaitMagazine.Rows.Count > 0 && dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1) != null)
                                dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1);
                        }
                        else
                        {
                            Util.GridSetData(dgWaitMagazineSTP, bizResult, FrameOperation);

                            if (dgWaitMagazineSTP.CurrentCell != null)
                                dgWaitMagazineSTP.CurrentCell = dgWaitMagazineSTP.GetCell(dgWaitMagazineSTP.CurrentCell.Row.Index, dgWaitMagazineSTP.Columns.Count - 1);
                            else if (dgWaitMagazineSTP.Rows.Count > 0 && dgWaitMagazineSTP.GetCell(dgWaitMagazineSTP.Rows.Count, dgWaitMagazineSTP.Columns.Count - 1) != null)
                                dgWaitMagazineSTP.CurrentCell = dgWaitMagazineSTP.GetCell(dgWaitMagazineSTP.Rows.Count, dgWaitMagazineSTP.Columns.Count - 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
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

        private void MagazineInput()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = new DataSet();
                string sBizName = string.Empty;
                string sReturnTable = null;

                if (PROCID.Equals(Process.SRC))
                {
                    indataSet = _Biz.GetBR_PRD_REG_START_INPUT_LOT_SRC();
                    sBizName = "BR_PRD_REG_START_INPUT_LOT_SRC";
                    sReturnTable = "OUT_LOT,OUT_INPUT";
                }
                else if (PROCID.Equals(Process.STP))
                {
                    indataSet = _Biz.GetBR_PRD_REG_START_INPUT_LOT_STP();
                    sBizName = "BR_PRD_REG_START_INPUT_LOT_STP";
                    sReturnTable = "OUT_INPUT";
                }
                else
                {
                    // SSC Bi-Cell
                    indataSet = _Biz.GetBR_PRD_REG_START_INPUT_LOT_SSCBI();
                    sBizName = "BR_PRD_REG_START_INPUT_LOT_SSCBI";
                    sReturnTable = "OUT_LOT,OUT_INPUT";
                }

             

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                C1DataGrid dg = new C1DataGrid();
                String sEqptMountPstnID = string.Empty;

                if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.SSC_BICELL))
                {
                    dg = dgWaitMagazine;
                    sEqptMountPstnID = cboMagMountPstnID.SelectedValue.ToString();
                }
                else
                {
                    dg = dgWaitMagazineSTP;
                    sEqptMountPstnID = cboMagMountPstnIDSTP.SelectedValue.ToString();
                }

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dg.Rows.Count - dg.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dg, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = sEqptMountPstnID;
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID"));

                    //SRC공정 -> 기준정보 고정매거진 투입이면 팝업
                    if (PROCID == Process.SRC && _isCstIdNeed)
                        newRow["OUT_CSTID"] = txtCstID.Text.ToString();

                    //if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.SSC_BICELL))
                    //{
                    //    newRow["ACTQTY"] = Util.NVC_NUMBER(DataTableConverter.GetValue(dg.Rows[i].DataItem, "WIPQTY"));
                    //}
                    inInputTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_INPUT", sReturnTable, (searchResult, searchException) =>
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

        /// <summary>
        /// WO_DETL_ID
        /// </summary>
        /// <returns></returns>
        private string GetWoDetlID()
        {
            try
            {
                string sWoDetlID = string.Empty;

                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SET_WORKORDER_INFO_SRC();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO_SRC", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (!Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals("") || !Util.NVC(dtRslt.Rows[0]["WO_DETL_ID2"]).Equals(""))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals(""))
                            sWoDetlID = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID2"]);
                        else
                            sWoDetlID = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);
                    }
                    else
                    {
                        sWoDetlID = "";
                    }
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
                HiddenParentLoadingIndicator();
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

        public void GetWaitBox()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = PROCID;
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = PROD_WODTLID;
                newRow["LOTID"] = txtWaitBoxLot.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_SSC_FD", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "LOTID"));
                    //newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "PRODID"));
                    newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    //newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "WIPQTY")));

                    //newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[iRow].DataItem, "CSTID"));

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
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "LOTID"));
                        //newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        //newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")));

                        //newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "CSTID"));

                        inMtrlTable.Rows.Add(newRow);
                    }
                }

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPUT_LOT_SSC_FD", "IN_EQP,IN_INPUT", "OUT_INPUT", (searchResult, searchException) =>
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
                    newRow["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[i].DataItem, "INPUT_QTY")));

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

        private bool GetWoProdInfo()
        {
            try
            {
                bool b3DModel = false;

                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SET_WORKORDER_INFO_SRC();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_INFO_BY_WO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("PRDT_SIZE") && Util.NVC(dtRslt.Rows[0]["PRDT_SIZE"]).Trim().Length > 0)
                    {
                        b3DModel = true;
                    }
                }

                return b3DModel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }
        }
        #endregion

        #region [Validation]
        private bool CanCurrInCancel()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            //STP:조립랏없이 장착가능
            if (_ProcID != Process.STP && Util.NVC(PROD_LOTID).Equals(""))
            {
                //Util.Alert("선택된 실적정보가 없습니다.");
                Util.MessageValidation("SFU1640");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        private bool CanCurrInReplace()
        {
            bool bRet = false;

            int iRow = _Util.GetDataGridCheckCnt(dgCurrIn, "CHK");
            int iFirstRow = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

            if (iRow < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (iRow > 1)
            {
                // 잔량처리는 1개만 선택할 수 있습니다.
                Util.MessageValidation("SFU3508");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[iFirstRow].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        private bool CanCurrInComplete()
        {
            bool bRet = false;

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

            if (PROD_LOTID.Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(txtCurrInLotID.Text.Trim()))
                    {
                        string sMsg = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")) + " 에 이미 투입되었습니다.";
                        Util.Alert(sMsg);
                        return bRet;
                    }
                }
            }

            // 대기 매거진 조회에서 찾는다.  조회 이후 발생된 대기 매거진일경우 문제됨..  biz 호출 필요
            int iRow = _Util.GetDataGridRowIndex(dgWaitMagazine, "LOTID", txtCurrInLotID.Text);

            if (iRow < 0)
            {
                // 대기 매거진에 없는 LOT 입니다.
                Util.MessageValidation("SFU3509");
                return bRet;
            }

            ////// 패키지 공정인 경우 최근 LOT에만 투입 처리 가능
            ////// 마지막 PROD LOT에만 투입 가능하도록 처리.
            ////if (PROCID.Equals(Process.PACKAGING))
            ////{
            ////    string sSelProd = PROD_LOTID;
            ////    string sNowProd = GetNowRunProdLot();
            ////    if (!sNowProd.Equals("") && sSelProd != sNowProd)
            ////    {
            ////        Util.Alert("선택한 조립LOT({0})은 마지막 작업중인 LOT이 아닙니다.\n마지막 작업중인 LOT({1})에만 투입할 수 있습니다.", sSelProd, sNowProd);
            ////        return false;
            ////    }
            ////}            

            bRet = true;

            return bRet;
        }

        private bool CanWaitMagInput()
        {
            bool bRet = false;

            if (Util.NVC(PROD_LOTID).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없습니다.");
                Util.MessageValidation("SFU1663");
                return bRet;
            }

            C1DataGrid dg = new C1DataGrid();
            if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.SSC_BICELL))
                dg = dgWaitMagazine;
            else
                dg = dgWaitMagazineSTP;

            if (_Util.GetDataGridCheckFirstRowIndex(dg, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            C1ComboBox cb = new C1ComboBox();

            if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.SSC_BICELL))
                cb = cboMagMountPstnID;
            else
                cb = cboMagMountPstnIDSTP;

            if (cb.SelectedValue == null || cb.SelectedValue.Equals("") || cb.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            int iRow = _Util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", cb.SelectedValue.ToString());

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

        #region [Func]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnCurrInCancel);
                listAuth.Add(btnCurrInComplete);
                listAuth.Add(btnCurrInReplace);
                listAuth.Add(btnWaitMagInput);
                listAuth.Add(btnWaitMagRework);
                listAuth.Add(btnInBoxInputCancel);
                listAuth.Add(btnWaitBoxInPut);
                listAuth.Add(btnWaitMagInputSTP);
                listAuth.Add(btnWaitMagReworkSTP);


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
                if (txtCurrInLotID != null)
                    txtCurrInLotID.Text = "";

                //대기매거진
                if (dgWaitMagazine != null)
                    Util.gridClear(dgWaitMagazine);
                if (txtWaitMazID != null)
                {
                    txtWaitMazID.Text = "";
                    txtCstID.Text = "";
                }

                if (dgWaitMagazineSTP != null)
                    Util.gridClear(dgWaitMagazineSTP);
                if (txtWaitMazIDSTP != null)
                    txtWaitMazIDSTP.Text = "";

                if (dgWaitBox != null)
                    Util.gridClear(dgWaitBox);

                //투입이력
                if (dgInputHist != null)
                    Util.gridClear(dgInputHist);

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

                //대기매거진
                if (dgWaitMagazine != null)
                    Util.gridClear(dgWaitMagazine);
                if (txtWaitMazID != null)
                {
                    txtWaitMazID.Text = "";
                    txtCstID.Text = "";
                }

                if (dgWaitMagazineSTP != null)
                    Util.gridClear(dgWaitMagazineSTP);
                if (txtWaitMazIDSTP != null)
                    txtWaitMazIDSTP.Text = "";

                if (dgWaitBox != null)
                    Util.gridClear(dgWaitBox);

                //투입이력
                if (dgInputHist != null)
                    Util.gridClear(dgInputHist);
                

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

        #region [PARENT Fnc Call / Use PARENT]


        public void SetVisiblity()
        {
            stpCstInput.Visibility = Visibility.Collapsed;
            txtCstID.IsEnabled = false;
            _isCstIdNeed = false;

            if (PROCID == Process.SRC)
            {
                stpCstInput.Visibility = Visibility.Visible;

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["PROCID"] = PROCID;
                dr["EQSGID"] = EQPTSEGMENT;
                dt.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_CELL_DETL_CLSS_MNGT_FLAG", "INDATA", "OUTDATA", dt);

                if (searchResult.Rows.Count == 0)
                    return;

                //UNLOADER 고정매거진사용
                if (searchResult.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString() == "CST_ID")
                {
                    txtCstID.IsEnabled = true;
                    _isCstIdNeed = true;
                }
            }
        }

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
                PROD_NAME = "";

                InitializeControls();

                ClearAll();

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

                if (PROCID.Equals(Process.SRC))
                {
                    txtProdCurrIn.Text = PROD_ID + " : " + PROD_NAME;
                    txtProdMagazine.Text = PROD_ID + " : " + PROD_NAME; ;
                    txtProdHist.Text = PROD_ID + " : " + PROD_NAME;
                    //txtProdMagazineSTP.Text = PROD_ID + " : " + PROD_NAME;
                }

                string sMountPstnGrCode = MOUNT_PSTN_GR_CODE.Equals("ALL") ? null : MOUNT_PSTN_GR_CODE;
                String[] sFilter1 = { EQPTID, "PROD", sMountPstnGrCode };

                CommonCombo _combo = new CommonCombo();

                if (PROCID.Equals(Process.SRC))
                    _combo.SetCombo(cboMagMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_SRC");
                ////else
                ////    _combo.SetCombo(cboMagMountPstnIDSTP, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_STP");

                if (PROCID.Equals(Process.SSC_FOLDED_BICELL))
                    GetWaitBox();

                GetCurrInList();
                GetWaitMagazine();
                GetInputHistory();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

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
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtWaitMazID.Text.Trim().Length > 0)
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

        private void dgWaitMagazineSTP_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtWaitMazIDSTP.Text.Trim().Length > 0)
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

        private void dgWaitMagazineSTP_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void btnInBoxInputCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanInBoxInputCancel(dgInputHist))
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInputCancel();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboSizeSTP_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitMagazine();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboDirectionSTP_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitMagazine();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitBoxLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitBox();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitBox();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCurrInLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCurrInLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCurrInLotID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInputHist_Click(object sender, RoutedEventArgs e)
        {
            if (wndInputCreate != null)
                wndInputCreate = null;

            wndInputCreate = new UC_IN_HIST_CREATE();
            wndInputCreate.FrameOperation = FrameOperation;

            if (wndInputCreate != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = _EqptSegment;
                Parameters[1] = _EqptID;
                Parameters[2] = _ProcID;
                Parameters[3] = _LotID;
                Parameters[4] = _WipSeq;

                C1WindowExtension.SetParameters(wndInputCreate, Parameters);

                wndInputCreate.Closed += new EventHandler(wndInputCreate_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndInputCreate.ShowModal()));
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
                    btnInputHist.Visibility = Visibility.Visible;
                }
                else
                {
                    btnInputHist.Visibility = Visibility.Collapsed;
                }

                if (dtResult?.Rows?.Count > 0 && Util.NVC(dtResult.Rows[0]["ATTRIBUTE5"]).Trim().Equals("Y"))
                {
                    dgInputHist.Columns["CHK"].Visibility = Visibility.Visible;
                    btnInBoxInputCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    dgInputHist.Columns["CHK"].Visibility = Visibility.Collapsed;
                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;
                }

                if (PROCID.Equals(Process.SRC) || PROCID.Equals(Process.STP))
                {
                    SetControlButton_SRC_STP();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControlButton_SRC_STP()
        {
            try
            {

                btnWaitMagRework.Visibility = Visibility.Collapsed; //대기매거진 -> 재구성
                btnWaitMagReworkSTP.Visibility = Visibility.Collapsed; // STP 대기매거진 -> 재구성

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("USERID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH", "INDATA", "OUTDATA", dt);

                if (dtResult?.Rows?.Count > 0)
                {
                    DataRow[] searchedRow = dtResult.Select("(AUTHID = 'MESADMIN' AND USE_FLAG = 'Y') OR (AUTHID = 'PROD_RSLT_MGMT_NJ' AND USE_FLAG = 'Y')");
                    if (searchedRow.Length > 0)
                    {
                        if (PROCID.Equals(Process.SRC))
                        {
                            btnWaitMagRework.Visibility = Visibility.Visible;     //대기매거진->재구성
                        }
                        else if (PROCID.Equals(Process.STP))
                        {
                            btnWaitMagReworkSTP.Visibility = Visibility.Visible;
                        }
                    
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
