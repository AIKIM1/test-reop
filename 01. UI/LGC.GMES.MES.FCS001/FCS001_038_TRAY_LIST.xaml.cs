/*************************************************************************************
 Created Date : 2020.11.11
      Creator : kang Dong Hee
   Decription : 상대판정 Tray List
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.07 NAME : Initial Created
  2021.04.16 KDH  : 기능 추가 및 보완
  2024.08.07 복현수 : 수동판정 버튼에 수동판정 로직 누락되어 추가
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
    public partial class FCS001_038_TRAY_LIST : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sLineId;
        private string _sLineName;
        private string _sModelId;
        private string _sModelName;
        private string _sLotID;
        private string _sRouteId;
        private string _sRouteName;
        private string _sJudgOper;
        private string _sJudgOperName;
        private string _sActOper;
        private string _sActOperName;
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

        public string LINENAME
        {
            set { this._sLineName = value; }
        }

        public string MODELNAME
        {
            set { this._sModelName = value; }
        }

        public string ROUTENAME
        {
            set { this._sRouteName = value; }
        }

        public string JUDGOPERNAME
        {
            set { this._sJudgOperName = value; }
        }

        public string ACTOPERNAME
        {
            set { this._sActOperName = value; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
        }
        #endregion

        #region [Initialize]
        public FCS001_038_TRAY_LIST()
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
            lblHelp.Text = ObjectDic.Instance.GetObjectName("UC_0020");  //※ 불량으로 판정된 등급은 변경되지 않습니다.

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                object[] parameters = this.FrameOperation.Parameters;
                _sLineId = Util.NVC(parameters[0]);
                _sLineName = Util.NVC(parameters[1]);
                _sModelId = Util.NVC(parameters[2]);
                _sModelName = Util.NVC(parameters[3]);
                _sLotID = Util.NVC(parameters[4]);
                _sRouteId = Util.NVC(parameters[5]);
                _sRouteName = Util.NVC(parameters[6]);
                _sJudgOper = Util.NVC(parameters[7]);
                _sJudgOperName = Util.NVC(parameters[8]);
                _sActOper = Util.NVC(parameters[9]);
                _sActOperName = Util.NVC(parameters[10]);
            }

            //Text Setting
            InitText();

            //Combo Setting
            InitCombo();

            chkDetail_Checked(null, null);

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            string[] sFilter = { "COMBO_GR_LOT_RJUDG_TRAY_LIST_SEARCH_CONDITION" };
            ComCombo.SetCombo(cboSort, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "FORM_CMN");
            if (cboSort.Items.Count > 0)
            {
                cboSort.SelectedIndex = 6;
            }
        }

        private void InitText()
        {
            txtLine.Text = _sLineName;
            txtModel.Text = _sModelName;
            txtLotID.Text = _sLotID;
            txtRoute.Text = _sRouteName;
            txtJudgOp.Text = _sJudgOperName;
            txtActOp.Text = _sActOperName;
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgTrayList);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROCID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROG_PROCID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("SORT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = (string.IsNullOrEmpty(_sLineId) ? null : _sLineId);
                dr["MDLLOT_ID"] = (string.IsNullOrEmpty(_sModelId) ? null : _sModelId);
                dr["DAY_GR_LOTID"] = (string.IsNullOrEmpty(_sLotID) ? null : _sLotID);
                dr["ROUTID"] = (string.IsNullOrEmpty(_sRouteId) ? null : _sRouteId);
                dr["JUDG_PROCID"] = (string.IsNullOrEmpty(_sJudgOper) ? null : _sJudgOper);
                dr["JUDG_PROG_PROCID"] = (string.IsNullOrEmpty(_sActOper) ? null : _sActOper);
                dr["CSTID"] = (string.IsNullOrEmpty(txtTrayID.Text) ? null : txtTrayID.Text);
                dr["CMCDTYPE"] = "FLOOR_CODE";
                dr["SORT"] = Util.GetCondition(cboSort);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RJUDG_TRAY_LIST", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgTrayList, dtRslt, FrameOperation, true);

                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "EQP_LOC" };
                    _Util.SetDataGridMergeExtensionCol(dgTrayList, sColumnName, DataGridMergeMode.VERTICAL);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckTray()
        {
            bool bFind = true;
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "TRAY_ID")).Equals(txtTrayID.Text))
                {
                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                    bFind = false;
                    break;
                }
            }

            if (bFind)
            {
                Util.MessageValidation("FM_ME_0260");  //해당 Tray ID가 존재하지 않습니다.
            }

            txtTrayID.Focus();
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void btnRelJudg_Click(object sender, EventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgTrayList.ItemsSource);
            DataRow[] drTrayList = dt.Select("CHK = True");

            if (drTrayList.Length == 0)
            {
                Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                return;
            }

            Util.MessageConfirm("FM_ME_0290", result => //▶전체 Tray 수 : {0} ▶판정대상 Tray 수 : {1}\r\n▶판정시점 : {2}\r\n▶판정공정 : {3}\r\n수동판정을 진행하시겠습니까?
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("JUDG_PROCID", typeof(string));

                        foreach (DataRow drTray in drTrayList)
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["DAY_GR_LOTID"] = Util.NVC(drTray["DAY_GR_LOTID"]);
                            dr["ROUTID"] = Util.NVC(drTray["ROUTID"]);
                            dr["JUDG_PROCID"] = Util.NVC(drTray["JUDG_OP_ID"]);
                            dtRqst.Rows.Add(dr);
                        }

                        //판정공정 Spec 산출 확인
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_REL_JUDG_DO_JOB_CHECK", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            DataTable dtRqst2 = new DataTable();
                            dtRqst2.TableName = "INDATA";
                            dtRqst2.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst2.Columns.Add("IFMODE", typeof(string));
                            dtRqst2.Columns.Add("EQPTID", typeof(string));
                            dtRqst2.Columns.Add("USERID", typeof(string));
                            dtRqst2.Columns.Add("CSTID", typeof(string));
                            dtRqst2.Columns.Add("JUDG_PROG_PROCID", typeof(string));
                            dtRqst2.Columns.Add("JUDG_PROCID", typeof(string));
                            
                            foreach (DataRow drTray in drTrayList)
                            {
                                DataRow dr2 = dtRqst2.NewRow();
                                dr2["SRCTYPE"] = "UI";
                                dr2["IFMODE"] = "OFF";
                                dr2["EQPTID"] = Util.NVC(drTray["EQPTID"]);
                                dr2["USERID"] = LoginInfo.USERID;
                                dr2["CSTID"] = Util.NVC(drTray["CSTID"]);
                                dr2["JUDG_PROG_PROCID"] = Util.NVC(drTray["ACT_OP_ID"]); //Aging 공정 ID
                                dr2["JUDG_PROCID"] = Util.NVC(drTray["JUDG_OP_ID"]); //판정 공정 ID
                                dtRqst2.Rows.Add(dr2);
                            }

                            //수동판정
                            DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("BR_SET_RJUDG_DO_JOB_UI", "INDATA", "OUTDATA", dtRqst2);

                            if (dtRslt2.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.MessageInfo("FM_ME_0186"); //요청 완료하였습니다.
                            }
                            else
                            {
                                Util.Alert("FM_ME_0185"); //요청 실패하였습니다.
                            }
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0291");  //판정에 필요한 Spec이 존재하지 않습니다.
                        }

                        HiddenLoadingIndicator();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
            }, new string[] { dt.Rows.Count.ToString(), drTrayList.Length.ToString(), _sActOperName + '[' + _sActOper + ']', _sJudgOperName + '[' + _sJudgOper + ']' });

        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if ((!string.IsNullOrEmpty(txtTrayID.Text)) && (e.Key == Key.Enter))
                {
                    CheckTray();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboSort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetList();
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["A_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["B_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["C_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["D_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["E_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["U_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["V_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["W_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["Z_GRD_QTY"].Visibility = Visibility.Visible;
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["A_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["B_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["C_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["D_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["E_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["U_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["V_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["W_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["Z_GRD_QTY"].Visibility = Visibility.Collapsed;
        }


        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTrayList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("CSTID"))
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        //Tray 정보조회 화면 연계
                        object[] parameters = new object[6];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[cell.Row.Index].DataItem, "CSTID"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[cell.Row.Index].DataItem, "LOTID"));
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = string.Empty;

                        this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    else if(cell.Column.Name.Equals("ROUTID"))
                    {
                        //DataTable dtRqst = new DataTable();
                        //dtRqst.TableName = "RQSTDT";
                        //dtRqst.Columns.Add("LOTID", typeof(string));

                        //DataRow dr = dtRqst.NewRow();
                        //dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();
                        //dtRqst.Rows.Add(dr);

                        //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LINE_MODEL_ROUTE_FROM_TRAY", "RQSTDT", "RSLTDT", dtRqst);

                        //loadingIndicator.Visibility = Visibility.Visible;
                        ////Route 관리
                        //object[] parameters = new object[3];
                        //parameters[0] = dtRslt.Rows[0]["EQSGID"].ToString();
                        //parameters[1] = dtRslt.Rows[0]["MDLLOT_ID"].ToString();
                        //parameters[2] = dtRslt.Rows[0]["ROUTID"].ToString();
                        //this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //ROUTE 관리 MMD로 이동되어 사용 불가
                        //loadingIndicator.Visibility = Visibility.Collapsed;

                        //Util.OpenRouteForm(dtRslt.Rows[0]["LINE_ID"].ToString(), dtRslt.Rows[0]["MODEL_ID"].ToString(), dtRslt.Rows[0]["ROUTE_ID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            string sOLD_EQPTID = string.Empty;
            SolidColorBrush c1 = new SolidColorBrush(Colors.White);

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
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    string sEQPTID = Util.NVC(dr.Row["EQPTID"]);
                    string sDUMMY_FLAG = Util.NVC(dr.Row["DUMMY_FLAG"]);
                    string sSPCL_FLAG = Util.NVC(dr.Row["SPCL_FLAG"]);

                    if (string.IsNullOrEmpty(sOLD_EQPTID))
                    {
                        sOLD_EQPTID = sEQPTID;
                    }

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (sOLD_EQPTID.Equals(sEQPTID))
                    {
                        e.Cell.Presenter.Background = c1;
                    }
                    else
                    {
                        if (c1.Equals(new SolidColorBrush(Colors.White)))
                        {
                            c1 = new SolidColorBrush(Color.FromArgb(242, 242, 242, 0));
                        }
                        else
                        {
                            c1 = new SolidColorBrush(Colors.White);
                        }
                        e.Cell.Presenter.Background = c1;
                    }

                    if (sDUMMY_FLAG.Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (sSPCL_FLAG.Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (sSPCL_FLAG.Equals("P"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgTrayList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTrayList);
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

    }
}
