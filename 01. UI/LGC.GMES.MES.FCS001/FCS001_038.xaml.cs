/*************************************************************************************
 Created Date : 2020.12.07
      Creator : kang Dong Hee
   Decription : 상대판정 SPEC 이력 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.07  NAME : Initial Created
  2021.04.05  KDH : 공정 그룹 추가 및 Lot ID 명칭 변경(Lot ID -> PKG Lot ID)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_038 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string DATATYPE_CURRENT = "C";
        private string DATATYPE_HISTORY = "H";

        private string _sLineId;
        private string _sModelId;
        private string _sLotID;
        private string _sRouteId;
        private string _sJudgOper;
        private string _sActOper;
        private string _sFromDate;
        private string _sFromTime;
        private string _sToDate;
        private string _sToTime;
        private string _sActYN = "N";

        public string LINEID
        {
            set { this._sLineId = value; }
        }

        public string MODELID
        {
            set { this._sModelId = value; }
        }

        public string LOTID
        {
            set { this._sLotID = value; }
        }

        public string ROUTEID
        {
            set { this._sRouteId = value; }
        }

        public string JUDGOPER
        {
            set { this._sJudgOper = value; }
        }

        public string ACTOPER
        {
            set { this._sActOper = value; }
        }

        public string FROMDATE
        {
            set { this._sFromDate = value; }
        }

        public string FROMTIME
        {
            set { this._sFromTime = value; }
        }

        public string TODATE
        {
            set { this._sToDate = value; }
        }

        public string TOTIME
        {
            set { this._sToTime = value; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }

        #endregion

        #region [Initialize]
        public FCS001_038()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            //Control Setting
            InitControl();

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                object[] parameters = this.FrameOperation.Parameters;
                _sLineId = Util.NVC(parameters[0]);
                _sModelId = Util.NVC(parameters[1]);
                _sLotID = Util.NVC(parameters[2]);
                _sRouteId = Util.NVC(parameters[3]);
                _sJudgOper = Util.NVC(parameters[4]);
                _sActOper = Util.NVC(parameters[5]);
                _sFromDate = Util.NVC(parameters[6]);
                _sFromTime = Util.NVC(parameters[7]);
                _sToDate = Util.NVC(parameters[8]);
                _sToTime = Util.NVC(parameters[9]);
                _sActYN = Util.NVC(parameters[10]);
            }

            if (_sActYN.Equals("N"))
            {
                return;
            }

            if (!string.IsNullOrEmpty(_sLineId))
            {
                cboLine.SelectedValue = _sLineId;
            }

            if (!string.IsNullOrEmpty(_sModelId))
            {
                cboModel.SelectedValue = _sModelId;
            }

            if (!string.IsNullOrEmpty(_sRouteId))
            {
                cboRoute.SelectedValue = _sRouteId;
            }

            if (!string.IsNullOrEmpty(_sActOper))
            {
                cboOper.SelectedValue = _sActOper;
            }

            if (!string.IsNullOrEmpty(_sFromDate))
            {
                dtpFromDate.SelectedDateTime = Convert.ToDateTime(_sFromDate).AddDays(-1);
            }

            if (!string.IsNullOrEmpty(_sFromTime))
            {
                dtpFromTime.DateTime = Convert.ToDateTime(_sFromTime);
            }

            if (!string.IsNullOrEmpty(_sToDate))
            {
                dtpToDate.SelectedDateTime = Convert.ToDateTime(_sToDate).AddDays(1);
            }

            if (!string.IsNullOrEmpty(_sToTime))
            {
                dtpToTime.DateTime = Convert.ToDateTime(_sToTime);
            }

            if (!string.IsNullOrEmpty(_sLotID))
            {
                txtLotId.Text = _sLotID;
            }

            _sActYN = "N";

            SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["MEASR_TYPE_CODE"], "MEASR_TYPE_CODE");
            SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["JUDG_MTHD_CODE"], "JUDG_MTHD_CODE");
            SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["REF_VALUE_CODE"], "REF_VALUE_CODE");

            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            //공정경로 별 조회
            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOper };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            C1ComboBox[] cboOperParent = { cboRoute };
            string[] sFilterProcGrCode = { "3,7" };
            ComCombo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent, sFilter: sFilterProcGrCode); //20210405 공정 그룹 추가
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpFromTime.DateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);
            dtpToTime.DateTime = DateTime.Now.AddDays(1);
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgRjudgSpecHist);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROG_PROCID", typeof(string));
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("HIST_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["JUDG_PROG_PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dr["DAY_GR_LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                if (chkHist.IsChecked.Equals(true))
                {
                    dr["HIST_FLAG"] = "Y";
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RJUDG_SPEC_DATA", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgRjudgSpecHist, dtRslt, FrameOperation, true);

                    for (int i = 0; i < dgRjudgSpecHist.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[i].DataItem, "FLAG")).Equals(DATATYPE_HISTORY))
                        {
                            DataTableConverter.SetValue(dgRjudgSpecHist.Rows[i].DataItem, "FLAG", ObjectDic.Instance.GetObjectName("SPEC_CALC_HIST"));
                        }
                    }

                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "DAY_GR_LOTID", "ROUTID", "JUDG_PROG_PROCID" };
                    _Util.SetDataGridMergeExtensionCol(dgRjudgSpecHist, sColumnName, DataGridMergeMode.VERTICAL);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetGridCboItem_CommonCode(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;
                if (!string.IsNullOrEmpty(sCmnCd))
                {
                    dr["CMCODE_LIST"] = sCmnCd;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (dtpToDate.SelectedDateTime.Date < dtpFromDate.SelectedDateTime.Date)
            {
                Util.MessageValidation("FM_ME_0182");  //시작일이 종료일보다 클 수 없습니다. 다시 선택해주세요.
            }
            else if (dtpToDate.SelectedDateTime.Date == dtpFromDate.SelectedDateTime.Date)
            {
                if (dtpToTime.DateTime.Value.TimeOfDay < dtpFromTime.DateTime.Value.TimeOfDay)
                {
                    Util.MessageValidation("FM_ME_0182");  //시작일이 종료일보다 클 수 없습니다. 다시 선택해주세요.
                }
            }

            SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["MEASR_TYPE_CODE"], "MEASR_TYPE_CODE");
            SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["JUDG_MTHD_CODE"], "JUDG_MTHD_CODE");
            SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["REF_VALUE_CODE"], "REF_VALUE_CODE");

            GetList();
        }

        private void dgRjudgSpecHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRjudgSpecHist.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "FLAG")).Equals(DATATYPE_CURRENT))
                    {
                        if (cell.Column.Name.Equals("JUDG_GRADE_CELL_CNT") || cell.Column.Name.Equals("JUDG_GRADE_CELL_RATE"))
                        {
                            FCS001_038_CELL_LIST wndPopup = new FCS001_038_CELL_LIST();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] Parameters = new object[3];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString();
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "ROUTID")).ToString();
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "SUBLOT_GRD_CODE")).ToString();

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                        else
                        {
                            //상대판정 Tray List 화면 실행
                            loadingIndicator.Visibility = Visibility.Visible;

                            object[] parameters = new object[12];
                            parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "EQSGID")).ToString();
                            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "EQSGNAME")).ToString();
                            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "MDLLOT_ID")).Split("[".ToCharArray())[0].Trim();
                            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "MDLLOT_ID"));
                            parameters[4] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString();
                            parameters[5] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "ROUTID")).ToString();
                            parameters[6] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "ROUTID")).ToString();
                            parameters[7] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "JUDG_PROCID")).ToString();
                            parameters[8] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "JUDG_PROC_NAME")).ToString();
                            parameters[9] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "JUDG_PROG_PROCID")).ToString();
                            parameters[10] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.Rows[cell.Row.Index].DataItem, "ACT_PROC_NAME")).ToString();
                            parameters[11] = "Y";

                            //연계 화면 확인 후 수정
                            this.FrameOperation.OpenMenuFORM("SFU010710211", "FCS001_038_TRAY_LIST", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("상대판정 Tray List"), true, parameters);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgRjudgSpecHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string Flag = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FLAG"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (Flag.Equals(DATATYPE_HISTORY))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    }
                }
            }));
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccessClear(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccessClear(object sender)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROG_PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "ROUTID"));
                dr["JUDG_PROG_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "JUDG_PROG_PROCID"));
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_REL_JUDG_OP", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt == null || dtRslt.Rows.Count <= 0 || !dtRslt.Rows[0]["JUDG_TMP_STOP_FLAG"].ToString().Equals("Y"))
                {
                    Util.MessageValidation("FM_ME_0298");  //초기화/재산출 시행 전 해당 상대 판정 Route를 일시정지해야합니다.
                    return;
                }

                Util.MessageConfirm("FM_ME_0292", result => //초기화 대상 Lot : {0}\r\n초기화 대상 Route : {1}\r\n초기화 대상 등급 : {2}\r\n해당 SPEC을 초기화하시겠습니까?
                {
                    if (result == MessageBoxResult.No) return;
                    try
                    {
                        DataTable dtRqst2 = new DataTable();

                        dtRqst2.TableName = "INDATA";
                        dtRqst2.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst2.Columns.Add("ROUTID", typeof(string));
                        dtRqst2.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                        dtRqst2.Columns.Add("GRD_ROW_NO", typeof(string));
                        dtRqst2.Columns.Add("GRD_COL_NO", typeof(string));
                        dtRqst2.Columns.Add("USERID", typeof(string));

                        DataRow dr2 = dtRqst2.NewRow();
                        dr2["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "DAY_GR_LOTID"));
                        dr2["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "ROUTID"));
                        dr2["SUBLOT_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "SUBLOT_GRD_CODE"));
                        dr2["GRD_ROW_NO"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "GRD_ROW_NO"));
                        dr2["GRD_COL_NO"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "GRD_COL_NO"));
                        dr2["USERID"] = LoginInfo.USERID;
                        dtRqst2.Rows.Add(dr2);

                        DataSet dsRqst = new DataSet();
                        dsRqst.Tables.Add(dtRqst2);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REL_JUDG_SPEC_LIST_INIT", "INDATA", "OUTDATA", dsRqst);
                        if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageInfo("FM_ME_0294");  //초기화가 완료되었습니다.
                        }
                        else
                        {
                            Util.Alert("FM_ME_0295");  //초기화 도중 문제가 발생하였습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, new string[] { Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "DAY_GR_LOTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "ROUTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "SUBLOT_GRD_CODE")) });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnSpec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccessSpec(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccessSpec(object sender)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROG_PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "ROUTID"));
                dr["JUDG_PROG_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "JUDG_PROG_PROCID"));
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_REL_JUDG_OP", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt == null || dtRslt.Rows.Count <= 0 || !dtRslt.Rows[0]["JUDG_TMP_STOP_FLAG"].ToString().Equals("Y"))
                {
                    Util.MessageValidation("FM_ME_0298");  //초기화/재산출 시행 전 해당 상대 판정 Route를 일시정지해야합니다.
                    return;
                }

                Util.MessageConfirm("FM_ME_0293", result => //SPEC 재산출 대상 Lot : {0}\r\nSPEC 재산출 대상 Route : {1}\r\nSPEC 재산출 대상 등급 : {2}\r\n해당 SPEC을 재산출하시겠습니까?
                {
                    if (result == MessageBoxResult.No) return;
                    try
                    {
                        DataTable dtRqst2 = new DataTable();
                        dtRqst2.TableName = "INDATA";
                        dtRqst2.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst2.Columns.Add("ROUTID", typeof(string));
                        dtRqst2.Columns.Add("JUDG_PROG_PROCID", typeof(string));
                        dtRqst2.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                        dtRqst2.Columns.Add("GRD_ROW_NO", typeof(string));
                        dtRqst2.Columns.Add("GRD_COL_NO", typeof(string));
                        dtRqst2.Columns.Add("USERID", typeof(string));

                        DataRow dr2 = dtRqst2.NewRow();
                        dr2["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "DAY_GR_LOTID"));
                        dr2["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "ROUTID"));
                        dr2["JUDG_PROG_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "JUDG_PROG_PROCID"));
                        dr2["SUBLOT_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "SUBLOT_GRD_CODE"));
                        dr2["GRD_ROW_NO"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "GRD_ROW_NO"));
                        dr2["GRD_COL_NO"] = Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "GRD_COL_NO"));
                        dr2["USERID"] = LoginInfo.USERID;
                        dtRqst2.Rows.Add(dr2);

                        DataSet dsRqst = new DataSet();
                        dsRqst.Tables.Add(dtRqst2);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REL_JUDG_SPEC_MAN_CALCUL", "INDATA", "OUTDATA", dsRqst);
                        if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageInfo("FM_ME_0296");  //SPEC 재산출이 완료되었습니다.
                        }
                        else
                        {
                            Util.Alert("FM_ME_0297");  //SPEC 재산출 도중 문제가 발생하였습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, new string[] { Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "DAY_GR_LOTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "ROUTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRjudgSpecHist.CurrentRow.DataItem, "SUBLOT_GRD_CODE")) });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_038_CELL_LIST window = sender as FCS001_038_CELL_LIST;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["MEASR_TYPE_CODE"], "MEASR_TYPE_CODE");
                SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["JUDG_MTHD_CODE"], "JUDG_MTHD_CODE");
                SetGridCboItem_CommonCode(dgRjudgSpecHist.Columns["REF_VALUE_CODE"], "REF_VALUE_CODE");

                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        #endregion

    }
}
