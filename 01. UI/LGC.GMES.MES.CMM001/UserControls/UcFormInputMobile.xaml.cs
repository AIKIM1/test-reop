/*************************************************************************************
 Created Date : 2017.06.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 활성화 공정진척화면의 투입 & 생산 부분 공통 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  INS 김동일K : Initial Created.
  2017.09.18  INS 김동일K : 조립 Prj 에서 CMM Prj 로 이동
**************************************************************************************/

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
namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// UcFormInputMobile.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFormInputMobile : UserControl, IWorkArea
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

        private string _Max_Pre_Proc_End_Day = string.Empty;
        private DateTime _Min_Valid_Date;

        private bool _StackingYN = false;

        public UserControl _UCParent;     // Caller

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        #region Popup 처리 로직 변경
        CMM_ASSY_REPLACE wndPanReplace;
        CMM_ASSY_MAGAZINE_CREATE wndMazCreate;
        #endregion

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

        public UcFormInputMobile(string sProcID, string sEqsgID, string sEqptID, bool bStacking = false)
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
                String[] sFilter1 = { EQPTID, "PROD" };
                String[] sFilter2 = { EQPTID, null }; // 자재,제품 전체
                String[] sFilter3 = { "BICELL_TYPE_FD" };

                _combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                _combo.SetCombo(cboMagMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                _combo.SetCombo(cboBoxMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                if (PROCID.Equals(Process.DSF))
                {
                    String[] sFilterHistMountPstsID = { EQPTID }; // 투입 위치 모두..
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilterHistMountPstsID, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_ALL");
                }
                else
                {
                    _combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                }

                _combo.SetCombo(cboCellType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");

                if (PROCID.Equals(Process.LAMINATION))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Visible;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;

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

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    //if (_StackingYN)
                    //{
                    //    rdoMagAType.Content = "HALFTYPE";
                    //    rdoMagAType.Tag = "HC";
                    //    tbATypeTitle.Text = "HALFTYPE";

                    //    rdoMagCtype.Content = "MONOTYPE";
                    //    rdoMagCtype.Tag = "MC";
                    //    tbCTypeTitle.Text = "MONOTYPE";
                    //}
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

                    // 교체처리 버튼 없애기.
                    grdCurrIn.ColumnDefinitions[9].Width = new GridLength(0);
                    grdCurrIn.ColumnDefinitions[10].Width = new GridLength(0);
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

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;

                    // 교체처리 버튼 없애기.
                    grdCurrIn.ColumnDefinitions[9].Width = new GridLength(0);
                    grdCurrIn.ColumnDefinitions[10].Width = new GridLength(0);
                    btnCurrInReplace.IsEnabled = false;

                    // Tray Type Combo
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    dt.Rows.Add("25", "25");
                    dt.Rows.Add("50", "50");

                }
                else if (PROCID.Equals(Process.DSF))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Collapsed;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Collapsed;

                    btnInBoxInputCancel.Visibility = Visibility.Collapsed;

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                    //btnCurrInReplace.IsEnabled = false;
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

        #region [투입현황]

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

                if (wndPanReplace != null)
                    wndPanReplace = null;

                wndPanReplace = new CMM_ASSY_REPLACE();
                wndPanReplace.FrameOperation = FrameOperation;

                if (wndPanReplace != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK");

                    object[] Parameters = new object[9];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "WIPSEQ"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_QTY"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    Parameters[6] = PROCID;
                    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[idx].DataItem, "INPUT_LOT_TYPE_CODE"));
                    Parameters[8] = "";
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
                        return;

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
            }
        }

        private void dgCurrIn_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
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

                                        _PRV_VLAUES.sPrvCurrIn = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_MOUNT_PSTN_ID"));

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

                                        _PRV_VLAUES.sPrvCurrIn = "";
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

                if (wndMazCreate != null)
                    wndMazCreate = null;

                wndMazCreate = new CMM_ASSY_MAGAZINE_CREATE();
                wndMazCreate.FrameOperation = FrameOperation;

                if (wndMazCreate != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = PROD_WOID;
                    Parameters[3] = PROCID;
                    Parameters[4] = "";
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

        private void btnInputBoxCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanInBoxInputCancel(dgInputBox))
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInputCancel2();
                    }
                });
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

                string sBizNAme = string.Empty;

                if (PROCID.Equals(Process.DSF))
                    sBizNAme = "DA_PRD_SEL_INPUT_MTRL_HIST_DSF";
                else
                    sBizNAme = "DA_PRD_SEL_INPUT_MTRL_HIST";

                DataTable inTable = _Biz.GetUC_DA_PRD_SEL_INPUT_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EQPTID;
                newRow["PROD_LOTID"] = PROD_LOTID;
                newRow["PROD_WIPSEQ"] = PROD_WIPSEQ.Equals("") ? 1 : Convert.ToDecimal(PROD_WIPSEQ);
                newRow["INPUT_LOTID"] = txtHistLotID.Text.Trim().Equals("") ? null : txtHistLotID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboHistMountPstsID.SelectedValue.ToString().Equals("") ? null : cboHistMountPstsID.SelectedValue.ToString();

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

                        //dgInputHist.ItemsSource = DataTableConverter.Convert(searchResult);
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPUT_IN_LOT_LM_S", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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
                else
                {
                    txtWaitPancakeInputClssCode.Text = "";
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

                if (cboCellType != null && cboCellType.SelectedValue.Equals("") || cboCellType.SelectedValue.Equals("SELECT"))
                    sElec = "";
                else
                    sElec = cboCellType.SelectedValue.ToString();

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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPUT_IN_LOT_FD_S", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = PROCID != Process.SSC_FOLDED_BICELL ? Process.PACKAGING : Process.SSC_FOLDED_BICELL;
                newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = PROD_WODTLID;

                inTable.Rows.Add(newRow);

                string bizRuleName = PROCID == Process.SSC_FOLDED_BICELL ? "DA_PRD_SEL_WAIT_LOT_LIST_SSC_FD" : "DA_PRD_SEL_WAIT_LOT_LIST_CL";

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
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "LOTID"));
                        newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = cboBoxMountPstsID.SelectedValue != null ? cboBoxMountPstsID.SelectedValue.ToString() : "";
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "WIPQTY")));

                        inMtrlTable.Rows.Add(newRow);
                    }
                }

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_CL_S", "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
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

            if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

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
            wndPanReplace = null;
            CMM_ASSY_REPLACE window = sender as CMM_ASSY_REPLACE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            wndMazCreate = null;
            CMM_ASSY_MAGAZINE_CREATE window = sender as CMM_ASSY_MAGAZINE_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetWaitMagazine();
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

        private void cboCellType_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
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
    }
}
