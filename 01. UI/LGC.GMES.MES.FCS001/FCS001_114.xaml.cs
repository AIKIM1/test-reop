/********************************************************************************************************************************
 Created Date : 2023.09.12
      Creator : 이정미
   Decription : 불량 셀 등록
---------------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.09.12  NAME   : Initial Created
  2023.12.27  형준우 : FCS 기준으로 변경
  2024.01.03  형준우 : USER PC TYPE 인 경우에는 생성자 이름 표기가 안되는 부분 수정 & 불량코드 콤보박스 비즈 수정
  2024.01.05  형준우 : 조회 Tab에서 불량그룹에 대하여 조건 추가
                       복구 Tab에서 치수불량에 대하여 복구 못하도록 되어있는 부분 수정
  2024.01.09  형준우 : 복구 Tab에서 Cell 정보 조회 후, 특정 Row를 삭제하고 재조회 시 삭제된 row가 다시 살아나는 현상 수정
  2024.01.18  형준우 : 복구이력 조회 수정
  2024.01.24  형준우 : 조회 Tab에서 설비투입금지해제 버튼 추가
  2024.02.14  형준우 : 설비투입금지 해제 Tab 추가
  2024.07.30  Ivan P : E20240613-001552 Add capability to search after paste in txtSearchCellId (GDC)
  2024.08.12  주경호 : E20240422-000378 Julia -Form#1/2/3 /4-> Add easy possibility to change Vision Apperance defect function
 *********************************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.Text;
using System.Linq;
//using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FCS001_114 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private string sRowCount = string.Empty; // 2024.07.23 Ivan P Add sRowCount to store max number of cell can be inputted in Cell ID textbox
        private Boolean isCellChgLossTab = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();

        public FCS001_114()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();

            isCellChgLossTab = GetCellChgLossTab();

            if (isCellChgLossTab == false)
            {
                Chg_Loss.Visibility = Visibility.Hidden;
            }

            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();

            // 2024.07.23 Ivan P Add max number of cell can be inputted in the Cell ID textbox
            sRowCount = GetCommonCode("SELECT_ROW_COUNT", "SEARCH_DEFECT_CELL");
            InitControl();

            //if (LoginInfo.LOGGEDBYSSO == true)
            //{
            txtUserID.Text = LoginInfo.USERID;
            txtUserName.Text = LoginInfo.USERNAME;
            txtRestoreUserID.Text = LoginInfo.USERID;
            txtRestoreUserName.Text = LoginInfo.USERNAME;
            txtReleaseUserID.Text = LoginInfo.USERID;
            txtReleaseUserName.Text = LoginInfo.USERNAME;
            //}

            Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            #region 기존
            // string[] sFilterEqp = { gsEqpKind, null };
            //string[] sFilterDefect = { gsEqpKind };
            // string[] sFilterOp = { "CLO", "A,B,C,D" };
            //string[] sFilterEqpInput = { "EQP_INPUT_YN" };

            //C1ComboBox[] cboLineChild = { cboModel };

            //if (gsEqpKind.Equals("5") || gsEqpKind.Equals("D"))   //Degas EOL만 설비 Display
            //{
            //    C1ComboBox[] cboSearchLaneChild = { cboSearchEqp };
            //    _combo.SetCombo(cboSearchLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchLaneChild, cbChild: cboSearchLaneChild, sCase: "LANE");

            //    C1ComboBox[] cboSearchEqpParent = { cboSearchLane };
            //    _combo.SetCombo(cboSearchEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchEqpParent, sFilter: sFilterEqp, sCase: "EQPIDBYLANE");

            //    C1ComboBox[] cboHistLaneChild = { cboHistEqp };
            //    _combo.SetCombo(cboHistLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistLaneChild, cbChild: cboHistLaneChild, sCase: "LANE");

            //    C1ComboBox[] cboHistEqpParent = { cboHistLane };
            //    _combo.SetCombo(cboHistEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistEqpParent, sFilter: sFilterEqp, sCase: "EQPIDBYLANE");
            //}      

            //C1ComboBox[] cboDefectParent = { cboInsertGroup };
            //_combo.SetCombo(cboInsertDefect, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "CELL_DEFECT", cbParent: cboDefectParent);
            //C1ComboBox[] cboGroupChild = { cboInsertDefect };
            //_combo.SetCombo(cboInsertGroup, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "DEFECT_KIND", cbChild: cboGroupChild);

            //_combo.SetCombo(cboHistOp, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterOp, sCase: "CMN");

            //string[] sFilterYN = { "USE_YN" };
            //_combo.SetCombo(cboSelectYN, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilterYN, sCase: "CMN", bCodeDisplay: true);
            #endregion

            CommonCombo_Form _combo = new CommonCombo_Form();

            #region 등록

            //불량그룹유형
            string[] sFilter1 = { "FORM_DFCT_GR_TYPE_CODE", "A,B,5,D,G" };
            _combo.SetCombo(cboInsertDfctGrType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilter1);

            SetGridCboItem(dgInputList.Columns["SHIFT"], "COMBO_SHIFT");

            #endregion

            #region 설비투입금지해제

            string[] sFilterRelease = { "FORM_DFCT_GR_TYPE_CODE", "5" };
            _combo.SetCombo(cboReleaseProcType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterRelease);

            #endregion

            #region 복구

            string[] sFilterRecover = { "FORM_DFCT_GR_TYPE_CODE", "5,D" };
            _combo.SetCombo(cboProcType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterRecover);

            #endregion

            #region 조회

            //공정
            string[] sFilterOp = { "FORM_DFCT_GR_TYPE_CODE", "A,B,D,5,G" };
            C1ComboBox[] cboSearchOpChild = { cboSearchEqp, cboSearchDefectKind };
            _combo.SetCombo(cboSearchOp, CommonCombo_Form.ComboStatus.NONE, cbChild: cboSearchOpChild, sCase: "FORM_CMN", sFilter: sFilterOp);

            //불량유무
            string[] sFilterBadYn = { "FORM_DEL_FLAG" };
            _combo.SetCombo(cboBadYn, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterBadYn);
            cboBadYn.SelectedValue = "N";

            //투입가능여부
            cboEqpInputYN.SetCommonCode("EQPT_INPUT_AVAIL_FLAG", CommonCombo.ComboStatus.ALL, true);

            //MODEL
            _combo.SetCombo(cboSearchModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");

            //활성화레인
            C1ComboBox[] cboSearchLaneParent = { cboSearchEqp };
            C1ComboBox[] cboSearchLaneChild = { cboSearchEqp };
            _combo.SetCombo(cboSearchLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchLaneParent, cbChild: cboSearchLaneChild, sCase: "LANE");

            //설비
            C1ComboBox[] cboSearchEqpParent = { cboSearchOp, null, cboSearchLane };
            _combo.SetCombo(cboSearchEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchEqpParent, sCase: "EQPDEGASEOL");

            //작업조
            string[] arrColumnShift = { "LANGID", "AREAID" };
            string[] arrConditionShift = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboSearchShift.SetDataComboItem("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", arrColumnShift, arrConditionShift, CommonCombo.ComboStatus.ALL, true);

            //불량그룹
            C1ComboBox[] cboSearchDefectKindChild = { cboSearchDefect };
            C1ComboBox[] cboSearchDefectKindParent = { cboSearchOp };
            _combo.SetCombo(cboSearchDefectKind, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchDefectKindParent, cbChild: cboSearchDefectKindChild, sCase: "DEFECT_KIND");

            //불량코드
            C1ComboBox[] cboSearchDefectParent = { cboSearchDefectKind, cboSearchOp };
            _combo.SetCombo(cboSearchDefect, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchDefectParent, sCase: "CELL_DEFECT");

            #endregion

            #region 이력

            //공정
            C1ComboBox[] cboHistOpChild = { cboHistEqp, cboHistDefectKind };
            _combo.SetCombo(cboHistOp, CommonCombo_Form.ComboStatus.NONE, cbChild: cboHistOpChild, sCase: "FORM_CMN", sFilter: sFilterRecover);

            //불량유무
            _combo.SetCombo(cboHistBadYn, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterBadYn);
            cboHistBadYn.SelectedValue = "N";

            //투입가능여부
            cboHistEqpInputYN.SetCommonCode("EQPT_INPUT_AVAIL_FLAG", CommonCombo.ComboStatus.ALL, true);

            //MODEL
            _combo.SetCombo(cboHistModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");

            //활성화레인
            C1ComboBox[] cboHistLaneParent = { cboHistEqp };
            C1ComboBox[] cboHistLanehild = { cboHistEqp };
            _combo.SetCombo(cboHistLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistLaneParent, cbChild: cboHistLanehild, sCase: "LANE");

            //설비
            C1ComboBox[] cboHistEqpParent = { cboHistOp, null, cboHistLane };
            _combo.SetCombo(cboHistEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistEqpParent, sCase: "EQPDEGASEOL");

            //작업조
            string[] arrColumnHistShift = { "LANGID", "AREAID" };
            string[] arrConditionHistShift = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboHistShift.SetDataComboItem("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", arrColumnHistShift, arrConditionHistShift, CommonCombo.ComboStatus.ALL, true);

            //불량그룹
            C1ComboBox[] cboHistDefectKindChild = { cboHistDefect };
            C1ComboBox[] cboHistDefectKindParent = { cboHistOp };
            _combo.SetCombo(cboHistDefectKind, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistDefectKindParent, cbChild: cboHistDefectKindChild, sCase: "DEFECT_KIND");

            //불량코드
            C1ComboBox[] cboHistDefectParent = { cboHistDefectKind, cboHistOp };
            _combo.SetCombo(cboHistDefect, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistDefectParent, sCase: "CELL_DEFECT");

            #endregion

            // 신규 탭 추가 관련 default 값 설정
            string[] sFilterDefect = { "5" };
            C1ComboBox[] cboGroupChild1 = { cboInsertDefect1 };
            _combo.SetCombo(cboInsertGroup1, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "DEFECT_KIND", cbChild: cboGroupChild1);

            C1ComboBox[] cboDefectParent1 = { cboInsertGroup1 };
            _combo.SetCombo(cboInsertDefect1, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "CELL_DEFECT", cbParent: cboDefectParent1);

            // 1번째가 외관불량으로 강제 세팅 후 disable 하여 선택 금지함
            cboInsertGroup1.SelectedIndex = 2;
        }

        #endregion

        #region Event

        #region 등록 

        private void dgInputList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgInputList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DENY_DESC"))
                {
                    if (dataGrid[e.Cell.Row.Index, 0].Presenter != null)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                        {
                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            if (cb != null) cb.Visibility = Visibility.Hidden;
                        }
                        else
                        {

                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            cb.Visibility = Visibility.Visible;
                        }
                    }
                }

                if (e.Cell.Row.Index == 0 && e.Cell.Column.Name.Equals("SUBLOTID"))
                {
                    C1.WPF.DataGrid.DataGridColumnHeaderPresenter dgcp =
                    (C1.WPF.DataGrid.DataGridColumnHeaderPresenter)e.Cell.Presenter.MergedRange.TopLeftCell.Presenter.Content;
                    dgcp.Foreground = Brushes.Red;
                }
            }));
        }

        private void cboInsertDfctGrType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (_combo != null)
            {
                string sDfctGrpTypeCode = Util.NVC(cboInsertDfctGrType.SelectedValue);

                DataTable dtTemp = null;
                DataTable dt = SetDfctKindCode(sDfctGrpTypeCode);
                dtTemp = dt.Copy();

                cboInsertGroup.ItemsSource = DataTableConverter.Convert(dt);
                cboInsertGroup.DisplayMemberPath = "CBO_NAME";
                cboInsertGroup.SelectedValuePath = "CBO_CODE";
                cboInsertGroup.ItemsSource = AddStatus(dtTemp, "CBO_CODE", "CBO_NAME").AsDataView();

                cboInsertGroup.SelectedIndex = 0;
            }
        }

        private void cboInsertGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (_combo != null)
            {
                string sDfctGrpTypeCode = Util.NVC(cboInsertDfctGrType.SelectedValue);
                string sDfctTypeCode = Util.NVC(cboInsertGroup.SelectedValue);

                //함수사용하여 LOT DETIL TYPE 구하기

                DataTable dtTemp = null;
                DataTable dt = SetDfctCode(sDfctGrpTypeCode, sDfctTypeCode);
                dtTemp = dt.Copy();

                cboInsertDefect.ItemsSource = DataTableConverter.Convert(dt);
                cboInsertDefect.DisplayMemberPath = "CBO_NAME";
                cboInsertDefect.SelectedValuePath = "CBO_CODE";
                cboInsertDefect.ItemsSource = AddStatus(dtTemp, "CBO_CODE", "CBO_NAME").AsDataView();

                cboInsertDefect.SelectedIndex = 0;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //fpsInsert.ActiveSheet.Columns["CELL_ID"].Locked = true;
            dgInputList.Columns["SUBLOTID"].IsReadOnly = true;
            cboInsertDfctGrType.IsEnabled = false;

            string sLossCode = Util.GetCondition(cboInsertDefect, bAllNull: true);
            if (string.IsNullOrEmpty(sLossCode) || sLossCode.Contains("SELECT"))
            {
                Util.Alert("FM_ME_0149"); //불량코드를 선택해주세요.
                return;
            }

            GetInsertCellDataFromSpread(dgInputList, dgInputList, dgInputList.Columns["SUBLOTID"].Index);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgInputList.ClearRows();
            txtInsertCellCnt.Text = "";
            txtBadInsertRow.Text = "";
            dgInputList.Columns["SUBLOTID"].IsReadOnly = false;
            cboInsertDfctGrType.IsEnabled = true;
            DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnBadCellReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sProcessType = Util.NVC(cboInsertDfctGrType.SelectedValue);
                string sDfctKind = Util.NVC(cboInsertGroup.SelectedValue);

                DataTable dt = DataTableConverter.Convert(dgInputList.ItemsSource);
                if (dt.Columns.Count < 2)
                {
                    //조회된 값이 없습니다.
                    Util.MessageValidation("FM_ME_0232");
                    return;
                }
                if (!dgInputList.IsCheckedRow("CHK"))
                {
                    //선택된 Cell ID가 없습니다.
                    Util.MessageValidation("FM_ME_0161");
                    return;
                }

                for (int i = 2; i < dgInputList.Rows.Count; i++)
                {
                    if (dgInputList.GetValue(i, "FINL_JUDG_CODE") == null || string.IsNullOrEmpty(dgInputList.GetValue(i, "FINL_JUDG_CODE").ToString())
                        || dgInputList.GetValue(i, "EQPTID") == null || string.IsNullOrEmpty(dgInputList.GetValue(i, "EQPTID").ToString())
                        || dgInputList.GetValue(i, "PROCNAME") == null || string.IsNullOrEmpty(dgInputList.GetValue(i, "PROCNAME").ToString()))
                    {
                        //데이터를 조회하십시오.
                        Util.Alert("9059");
                        return;
                    }
                }

                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sLossCode = Util.GetCondition(cboInsertDefect, bAllNull: true);
                        if (string.IsNullOrEmpty(sLossCode) || sLossCode.Contains("SELECT"))
                        {
                            Util.Alert("FM_ME_0149"); //불량코드를 선택해주세요.
                            return;
                        }

                        if (dgInputList.GetRowCount() <= 0)
                        {
                            //등록할 대상이 존재하지 않습니다.
                            Util.MessageValidation("FM_ME_0125");
                            return;
                        }

                        DataSet dsInDataSet = new DataSet();

                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));
                        dtINDATA.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                        dtINDATA.Columns.Add("REMARKS_CNTT", typeof(string));
                        dtINDATA.Columns.Add("CALDATE", typeof(DateTime));
                        dtINDATA.Columns.Add("MENUID", typeof(string));
                        dtINDATA.Columns.Add("USER_IP", typeof(string));
                        dtINDATA.Columns.Add("PC_NAME", typeof(string));
                        dsInDataSet.Tables.Add(dtINDATA);

                        DataRow drInData = dtINDATA.NewRow();
                        drInData["SRCTYPE"] = "UI";
                        drInData["IFMODE"] = "OFF";
                        drInData["USERID"] = txtUserID.Text;
                        drInData["LOT_DETL_TYPE_CODE"] = GetDfctLotDetlType(sDfctKind);
                        drInData["REMARKS_CNTT"] = txtRemark.Text;
                        drInData["MENUID"] = LoginInfo.CFG_MENUID;
                        drInData["USER_IP"] = LoginInfo.USER_IP;
                        drInData["PC_NAME"] = LoginInfo.PC_NAME;
                        dtINDATA.Rows.Add(drInData);

                        DataTable dtIN_SUBLOT = new DataTable();
                        dtIN_SUBLOT.TableName = "IN_SUBLOT";
                        dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                        dtIN_SUBLOT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                        dtIN_SUBLOT.Columns.Add("DFCT_CODE", typeof(string));
                        dsInDataSet.Tables.Add(dtIN_SUBLOT);

                        for (int i = 2; i < dgInputList.Rows.Count; i++)
                        {
                            if (dgInputList.IsCheckedRow("CHK", i - 2)
                                && string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "DENY_DESC"))))
                            {
                                dtIN_SUBLOT.Rows.Clear();

                                DataRow drInSublot = dtIN_SUBLOT.NewRow();
                                drInSublot["SUBLOTID"] = dgInputList.GetValue(i, "SUBLOTID");
                                drInSublot["DFCT_GR_TYPE_CODE"] = sProcessType;
                                drInSublot["DFCT_CODE"] = sLossCode;
                                dtIN_SUBLOT.Rows.Add(drInSublot);

                                try
                                {
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet, FrameOperation.MENUID);

                                    if (dsResult.Tables[0].Rows[0]["RETVAL"].ToString().Equals("0"))
                                    {
                                        dgInputList.SetValue(i, "DENY_DESC", "SUCCESS");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    dgInputList.SetValue(i, "DENY_DESC", "FAIL");
                                }
                            }
                        }
                        // 비고란을 통해 성공여부를 확인할 수 있습니다.
                        Util.MessageInfo("FM_ME_0448");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgInputList);
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetUserWindow();
        }

        #endregion

        #region 설비투입금지해제

        private void dgRelease_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK_DIGIT")).Equals("2"))
                    {
                        //설비투입 금지인 경우 설정가능
                        if (!e.Cell.Column.Name.IsNullOrEmpty() && e.Cell.Column.Name.Equals("CHK"))
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.IsEnabled = true;
                    }
                    else
                    {
                        if (!e.Cell.Column.Name.IsNullOrEmpty() && e.Cell.Column.Name.Equals("CHK"))
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void txtReleaseUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetReleaseUserWindow();
        }

        private void btnReleaseRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgRelease.ClearRows();
            dgRelease.Columns["SUBLOTID"].IsReadOnly = false;
            txtReleaseCellCount.Text = "";
            DataGridRowAdd(dgRelease, Convert.ToInt32(txtReleaseRowCntInsertCell.Text));
        }

        private void btnReleaseSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetReleaseUserWindow();
        }

        private void btnReleaseExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgRelease);
        }

        private void btnReleaseSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iProcessingCnt = 100;
                double dNumberOfProcessingCnt = 0.0;
                dNumberOfProcessingCnt = Math.Ceiling(dgRelease.Rows.Count / (double)iProcessingCnt);//처리수량

                dgRelease.Columns["SUBLOTID"].IsReadOnly = true;

                DataTable dtRslt = new DataTable();

                DataSet inDataSet = new DataSet();
                DataTable dtRqst = inDataSet.Tables.Add("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                for (int iRow = 0; iRow < dgRelease.GetRowCount(); iRow++)
                {
                    string sCellID = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[iRow].DataItem, "SUBLOTID"));
                    //스프레드에 있는지 확인
                    if (sCellID.Trim() == string.Empty)
                        break;

                    if (iRow != 0)
                    {
                        for (int i = 0; i <= iRow - 1; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "SUBLOTID")).Equals(sCellID))
                            {
                                Util.MessageInfo("FM_ME_0287", new string[] { sCellID });  //[CELL ID : {0}]목록에 기존재하는 Cell 입니다.
                                continue;
                            }
                        }
                    }
                }

                for (int i = 0; i < dNumberOfProcessingCnt; i++)
                {
                    dtRqst.Clear();

                    DataRow dr2 = dtRqst.NewRow();
                    string sCellID = string.Empty;

                    for (int k = (i * Convert.ToInt32(iProcessingCnt)); k < (i * iProcessingCnt + iProcessingCnt); k++)
                    {
                        if (k >= dgRelease.Rows.Count) break;

                        if (Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[k].DataItem, "SUBLOTID")) != string.Empty)
                            sCellID += Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[k].DataItem, "SUBLOTID")) + ",";
                    }

                    dr2["LANGID"] = LoginInfo.LANGID;
                    dr2["SUBLOTID"] = sCellID;
                    dtRqst.Rows.Add(dr2);

                    try
                    {
                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_FOR_RELEASE", "RQSTDT", "RSLTDT", dtRqst);
                        Util.GridSetData(dgRelease, dtResult, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void btnCellRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRelease.GetRowCount() == 0)
                {
                    Util.Alert("FM_ME_0499");  //설비투입금지 해제할 대상이 없습니다.
                    return;
                }

                if (!dgRelease.IsCheckedRow("CHK"))
                {
                    //선택된 Cell ID가 없습니다.
                    Util.MessageValidation("FM_ME_0161");
                    return;
                }

                for (int j = 0; j < dgRelease.GetRowCount(); j++)
                {
                    if (dgRelease.GetValue(j, "ROUTID") == null || string.IsNullOrEmpty(dgRelease.GetValue(j, "ROUTID").ToString())
                        || dgRelease.GetValue(j, "LOTID") == null || string.IsNullOrEmpty(dgRelease.GetValue(j, "LOTID").ToString()))
                    {
                        //데이터를 조회하십시오.
                        Util.Alert("9059");
                        return;
                    }
                }

                //설비투입금지를 해제하시겠습니까?
                Util.MessageConfirm("FM_ME_0383", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("SUBLOTID", typeof(string));
                        dtINDATA.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                        dtINDATA.Columns.Add("EQPT_INPUT_AVAIL_FLAG", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgRelease.Rows.Count; i++)
                        {
                            if (dgRelease.IsCheckedRow("CHK", i))
                            {
                                DataRow drInSublot = dtINDATA.NewRow();
                                drInSublot["SRCTYPE"] = "UI";
                                drInSublot["IFMODE"] = "OFF";
                                drInSublot["SUBLOTID"] = dgRelease.GetValue(i, "SUBLOTID");
                                drInSublot["DFCT_GR_TYPE_CODE"] = dgRelease.GetValue(i, "DFCT_GR_TYPE_CODE");
                                drInSublot["EQPT_INPUT_AVAIL_FLAG"] = "Y";
                                drInSublot["USERID"] = LoginInfo.USERID;
                                dtINDATA.Rows.Add(drInSublot);
                            }
                        }
                        try
                        {
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EQP_INPUT_RELEASE", "INDATA", "OUTDATA", dtINDATA);

                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                            {
                                Util.MessageValidation("FM_ME_0498");  //설비투입금지를 해제하였습니다.
                            }
                            else
                            {
                                Util.MessageValidation("FM_ME_0311");  //저장 실패하였습니다.
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        btnReleaseRefresh_Click(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRelease_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name != null && e.Column.Name.Equals("SUBLOTID"))
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Red;
            }
            else
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Black;
            }
        }

        private void btnReleaseDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btnDelete = sender as Button;

                if (btnDelete != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;
                    int rowidx = -1;
                    int chkcnt = 0;

                    for (int i = 0; i < dgRelease.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "SUBLOTID").Equals(dataRow.Row[1]))
                        {
                            rowidx = i;
                        }
                        if (DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "CHK").Equals(1))
                        {
                            chkcnt++;
                        }
                    }

                    DataTable dt = DataTableConverter.Convert(dgRelease.ItemsSource);
                    dt.Rows.RemoveAt(rowidx);
                    Util.GridSetData(dgRelease, dt, this.FrameOperation);
                    txtReleaseCellCount.Text = chkcnt.ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRelease_chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            int chkcnt = 0;

            for (int i = 0; i < dgRelease.Rows.Count; i++)
            {
                string badYN = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "BAD_YN"));
                string availYN = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "EQPT_INPUT_AVAIL_FLAG"));
                if (badYN.Equals("N") && availYN.Equals("N")) //불량 & 투입불가
                {
                    DataTableConverter.SetValue(dgRelease.Rows[i].DataItem, "CHK", true);
                }

                if (Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    chkcnt++;
                }
            }
            txtReleaseCellCount.Text = chkcnt.ToString();
        }

        private void dgRelease_chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            int chkcnt = 0;

            for (int i = 0; i < dgRelease.Rows.Count; i++)
            {
                string badYN = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "BAD_YN"));
                string availYN = Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "EQPT_INPUT_AVAIL_FLAG"));
                if (badYN.Equals("N") && availYN.Equals("N"))
                {
                    DataTableConverter.SetValue(dgRelease.Rows[i].DataItem, "CHK", false);
                }

                if (Util.NVC(DataTableConverter.GetValue(dgRelease.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    chkcnt++;
                }
            }
            txtReleaseCellCount.Text = chkcnt.ToString();
        }
        #endregion

        #region 복구 

        private void dgRecover_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name != null && e.Column.Name.Equals("SUBLOTID"))
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Red;
            }
            else
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Black;
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder cellIDs = new StringBuilder();

                DataTable dtRslt = new DataTable();

                if (cboProcType.GetBindValue() == null)
                {
                    Util.Alert("FM_ME_0107"); //공정을 선택해주세요.
                    return;
                }

                for (int iRow = 0; iRow < dgRecover.GetRowCount(); iRow++)
                {
                    int indexCellCol = dgRecover.Columns["SUBLOTID"].Index;
                    string sCellID = string.Empty;
                    string sTemp = Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[iRow].DataItem, "SUBLOTID"));
                    if (sTemp.Trim() == string.Empty)
                        break;

                    sCellID = sTemp;

                    //스프레드에 있는지 확인
                    if (iRow != 0)
                    {
                        for (int i = 0; i <= iRow - 1; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID")).Equals(sCellID))
                            {
                                Util.MessageInfo("FM_ME_0287", new string[] { sCellID });  //[CELL ID : {0}]목록에 기존재하는 Cell 입니다.
                                continue;
                            }
                        }
                    }

                    #region BizWF 
                    int RetVal = BizWF_Check(sCellID);

                    if (RetVal != 0)
                    {
                        //ShowLoadingIndicator();
                        loadingIndicator.Visibility = Visibility.Hidden;
                        return;
                    }
                    #endregion

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                    dr["PROCESS_TYPE"] = Util.NVC(cboProcType.SelectedValue);
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);

                    //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_CELLID_FOR_RECOVER", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                    if (dtRslt1.Rows.Count > 0)
                    {
                        dtRslt.Merge(dtRslt1);
                    }
                    else
                    {
                        cellIDs.Append(sTemp + ", ");
                    }
                }

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    dgRecover.ClearRows();
                }
                else
                {
                    Util.GridSetData(dgRecover, dtRslt, FrameOperation, true);
                }

                if (cellIDs.Length > 0)
                {
                    Util.MessageInfo(MessageDic.Instance.GetMessage("SFU1585") + "\r\n\r\n" + cellIDs.ToString(0, cellIDs.Length - 2));  //불량정보가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel2_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgRecover);
        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            dgRecover.ClearRows();
            DataGridRowAdd(dgRecover, Convert.ToInt32(txtRowCntInsertCell2.Text));
        }

        private void txtRestoreUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetRestoreUserWindow();
        }

        private void btnRestoreSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetRestoreUserWindow();
        }

        private void btnBadCellResotre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRecover.GetRowCount() == 0)
                {
                    Util.Alert("FM_ME_0139");  //복구 대상이 없습니다.
                    return;
                }

                for (int j = 0; j < dgRecover.GetRowCount(); j++)
                {
                    if (dgRecover.GetValue(j, "SUBLOTID") == null || string.IsNullOrEmpty(dgRecover.GetValue(j, "SUBLOTID").ToString()))
                    {
                        //복구 대상이 없습니다.
                        Util.Alert("FM_ME_0139");
                        return;
                    }

                    if (dgRecover.GetValue(j, "ROUTID") == null || string.IsNullOrEmpty(dgRecover.GetValue(j, "ROUTID").ToString())
                        || dgRecover.GetValue(j, "PROD_LOTID") == null || string.IsNullOrEmpty(dgRecover.GetValue(j, "PROD_LOTID").ToString()))
                    {
                        //데이터를 조회하십시오.
                        Util.Alert("9059");
                        return;
                    }
                }

                Util.MessageConfirm("FM_ME_0147", (result) => //불량 Cell 복구를 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "INDATA";
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("IFMODE", typeof(string));
                            dtRqst.Columns.Add("SUBLOTID", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));

                            string errorString = string.Empty;
                            for (int i = 0; i < dgRecover.GetRowCount(); i++)
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["SUBLOTID"] = dgRecover.GetValue(i, "SUBLOTID");
                                dr["USERID"] = txtRestoreUserID.Text;
                                dtRqst.Rows.Add(dr);

                                //Vision Demension 인 경우, 복구 불가
                                //if (Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "DFCT_CODE")).Equals("320"))
                                //{
                                //    Util.MessageInfo("FM_ME_0561", DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID")); //[%1] 복구 불가능한 Cell이 포함되어있어, 복구할 수 없습니다.
                                //    return;
                                //}
                            }

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_SUBLOT_DFCT_RESTORE", "INDATA", "OUTDATA", dtRqst);

                            if (!string.IsNullOrEmpty(dtRslt.Rows[0]["NO_SUBLOTID"].ToString()))
                            {
                                errorString += dtRslt.Rows[0]["NO_SUBLOTID"].ToString() + ",";
                            }

                            if (!string.IsNullOrEmpty(errorString))
                            {
                                //NO_SUBLOTID String 불량항목 없어서 복구되지 않은 SUBLOTID LIST, 리턴 값 예시) AK21E12955,AK21E12956,AK21E12957
                                Util.MessageInfo("FM_ME_0560", errorString);  //[%1] Cell의 불량이력이 없어 복구할 수 없습니다.
                            }
                            else
                            {
                                Util.MessageValidation("FM_ME_0140");  //저장하였습니다.
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        btnRefresh2_Click(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Button btn = sender as Button;
            //int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            //dgRecover.RemoveRow(idx);

            try
            {
                Button btnDelete = sender as Button;

                if (btnDelete != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;
                    int rowidx = -1;

                    for (int i = 0; i < dgRecover.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID").Equals(dataRow.Row[0]))
                        {
                            rowidx = i;
                        }
                    }

                    DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);
                    dt.Rows.RemoveAt(rowidx);
                    Util.GridSetData(dgRecover, dt, this.FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 조회

        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BAD_YN")).Equals("Y"))
                    {
                        //양품 설정불가
                        if (e.Cell.Column.Name.Equals("CHK"))
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.IsEnabled = false;
                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_INPUT_AVAIL_FLAG")).Equals("N"))
                        {
                            //불량 중 설비투입 금지인 경우 설정가능
                            //설비투입금지해제 기능을 사용하기 위해
                            if (e.Cell.Column.Name.Equals("CHK"))
                                dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.IsEnabled = true;
                        }
                        else
                        {
                            if (e.Cell.Column.Name.Equals("CHK"))
                                dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.IsEnabled = false;
                        }
                    }
                }
            }));
        }

        //private void rdoReg_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (rdoReg.IsChecked == true)
        //    {
        //        dtpFromTime.IsEnabled = true;
        //        dtpToTime.IsEnabled = true;
        //    }
        //    else
        //    {
        //        dtpFromTime.IsEnabled = false;
        //        dtpToTime.IsEnabled = false;
        //    }
        //}

        //2024.07.23 Ivan P add new event to directly search after paste in txtSearchCellId
        private void txtSearchCellId_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            btnSearch3.PerformClick();
        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboSearchOp.GetBindValue() == null)
                {
                    Util.Alert("FM_ME_0107"); //공정을 선택해주세요.
                    return;
                }

                //2024.07.23 Ivan P add max limit for cell input to be 900 cells
                if (txtSearchCellId.Text.Count(c => c == ',') + 1 > Convert.ToInt32(sRowCount))
                {
                    Util.Alert("FM_ME_0619");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                //if (rdoReg.IsChecked == true)
                //{
                //    dtRqst.Columns.Add("FROM_REG_DATE", typeof(DateTime));
                //    dtRqst.Columns.Add("TO_REG_DATE", typeof(DateTime));
                //}
                //else if (rdoWork.IsChecked == true)
                //{
                //    dtRqst.Columns.Add("FROM_WORK_DATE", typeof(DateTime));
                //    dtRqst.Columns.Add("TO_WORK_DATE", typeof(DateTime));
                //}
                //else
                //{
                //    dtRqst.Columns.Add("FROM_REG_DATE", typeof(DateTime));
                //    dtRqst.Columns.Add("TO_REG_DATE", typeof(DateTime));
                //}
                dtRqst.Columns.Add("FROM_REG_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_REG_DATE", typeof(DateTime));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("BAD_YN", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("EQPT_INPUT_AVAIL_FLAG", typeof(string));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                //if (rdoReg.IsChecked == true)
                //{
                //    dr["FROM_REG_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                //    dr["TO_REG_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:00");
                //}
                //else if (rdoWork.IsChecked == true)
                //{
                //    dr["FROM_WORK_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 07:00:00");
                //    dr["TO_WORK_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 06:59:59");
                //}
                dr["FROM_REG_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:ss.000");
                dr["TO_REG_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:ss.997");
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BAD_YN"] = cboBadYn.GetBindValue();
                dr["SHFT_ID"] = cboSearchShift.GetBindValue();
                dr["DFCT_GR_TYPE_CODE"] = cboSearchOp.GetBindValue();
                dr["DFCT_TYPE_CODE"] = cboSearchDefectKind.GetBindValue();
                dr["DFCT_CODE"] = cboSearchDefect.GetBindValue();
                dr["SUBLOTID"] = txtSearchCellId.GetBindValue();
                dr["S71"] = cboSearchLane.GetBindValue();

                if (cboSearchOp.GetBindValue().Equals("D") || cboSearchOp.GetBindValue().Equals("5"))
                    dr["EQPTID"] = cboSearchEqp.GetBindValue();

                dr["MDLLOT_ID"] = cboSearchModel.GetBindValue();
                dr["PROD_LOTID"] = txtSearchLotId.GetBindValue();
                dr["EQPT_INPUT_AVAIL_FLAG"] = cboEqpInputYN.GetBindValue();
                dr["REMARKS_CNTT"] = txtHoldDesc.GetBindValue();
                dr["INSUSER"] = txtSearchRegUser.GetBindValue();
                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_DFCT_BY_COND", "RQSTDT", "RSLTDT", dtRqst, "");

                // CHK 추가
                if (!dtRslt.Columns.Contains("CHK"))
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    dtRslt.Select().ToList().ForEach(row => row["CHK"] = false);
                    dtRslt.AcceptChanges();
                }

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //설비투입금지를 해제하시겠습니까?
                Util.MessageConfirm("FM_ME_0383", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("SUBLOTID", typeof(string));
                        dtINDATA.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                        dtINDATA.Columns.Add("EQPT_INPUT_AVAIL_FLAG", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgSearch.Rows.Count; i++)
                        {
                            if (dgSearch.IsCheckedRow("CHK", i))
                            {
                                DataRow drInSublot = dtINDATA.NewRow();
                                drInSublot["SRCTYPE"] = "UI";
                                drInSublot["IFMODE"] = "OFF";
                                drInSublot["SUBLOTID"] = dgSearch.GetValue(i, "SUBLOTID");
                                drInSublot["DFCT_GR_TYPE_CODE"] = dgSearch.GetValue(i, "DFCT_GR_TYPE_CODE");
                                drInSublot["EQPT_INPUT_AVAIL_FLAG"] = "Y";
                                drInSublot["USERID"] = LoginInfo.USERID;
                                dtINDATA.Rows.Add(drInSublot);
                            }
                        }
                        try
                        {
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EQP_INPUT_RELEASE", "INDATA", "OUTDATA", dtINDATA);

                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                            {
                                Util.MessageValidation("FM_ME_0498");  //설비투입금지를 해제하였습니다.
                            }
                            else
                            {
                                Util.MessageValidation("FM_ME_0311");  //저장 실패하였습니다.
                            }
                            btnSearch3_Click(null, null);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearch_chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                string badYN = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "BAD_YN"));
                string availYN = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "EQPT_INPUT_AVAIL_FLAG"));
                if (badYN.Equals("N") && availYN.Equals("N")) //불량 & 투입불가
                {
                    DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void dgSearch_chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                string badYN = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "BAD_YN"));
                string availYN = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "EQPT_INPUT_AVAIL_FLAG"));
                if (badYN.Equals("N") && availYN.Equals("N"))
                {
                    DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void cboSearchOp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (e.NewValue.Equals("D") || e.NewValue.Equals("5"))
            {
                cboSearchEqp.IsEnabled = true;

                //설비투입금지해제는 eol 만 가능하게 진행
                if (e.NewValue.Equals("5"))
                    btnSearchRelease.IsEnabled = true;
                else
                    btnSearchRelease.IsEnabled = false;
            }
            else
            {
                cboSearchEqp.IsEnabled = false;
                btnSearchRelease.IsEnabled = false;
            }
        }
        #endregion

        #region 이력 

        private void btnSearch4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("BAD_YN", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpHistFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpHistFromTime.DateTime.Value.ToString(" HH:mm:00");
                dr["TO_DATE"] = dtpHistToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpHistToTime.DateTime.Value.ToString(" HH:mm:00");
                dr["SUBLOTID"] = txtHistCellId.GetBindValue();
                dr["BAD_YN"] = cboHistBadYn.GetBindValue();
                dr["SHFT_ID"] = cboHistShift.GetBindValue();
                dr["DFCT_GR_TYPE_CODE"] = cboHistOp.GetBindValue();
                dr["DFCT_TYPE_CODE"] = cboHistDefectKind.GetBindValue();
                dr["DFCT_CODE"] = cboHistDefect.GetBindValue();
                dr["LANE_ID"] = cboHistLane.GetBindValue();
                dr["EQPTID"] = cboHistEqp.GetBindValue();
                dr["MDLLOT_ID"] = cboHistModel.GetBindValue();
                dr["PROD_LOTID"] = txtHistLotId.GetBindValue();
                dr["REMARKS_CNTT"] = txtHistHoldDesc.GetBindValue();
                dr["INSUSER"] = txtHistUser.GetBindValue();

                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_LOSS_DEL", "RQSTDT", "RSLTDT", dtRqst, menuid: Tag.ToString());

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_DFCT_HIST", "RQSTDT", "RSLTDT", dtRqst, ""); //gsEqpKind

                Util.GridSetData(dgHistList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region Method

        private void GetInsertCellDataFromSpread(C1DataGrid fromSpread, C1DataGrid toSpread, int indexCellCol)
        {
            string sCellID = string.Empty;

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("EQPT_KIND", typeof(string));
            dtRqst.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("LOSS_CODE", typeof(string));

            for (int iRow = 0; iRow < fromSpread.GetRowCount(); iRow++)
            {

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(fromSpread.Rows[iRow + 2].DataItem, "SUBLOTID"))))
                    continue;

                sCellID = fromSpread.GetCell(iRow + 2, indexCellCol).Text.ToString();

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = sCellID;
                dr["EQPT_KIND"] = Util.NVC(cboInsertDfctGrType.SelectedValue);
                dr["LOT_DETL_TYPE_CODE"] = GetDfctLotDetlType(Util.NVC(cboInsertGroup.SelectedValue));
                dr["LOSS_CODE"] = Util.NVC(cboInsertDefect.SelectedValue);
                dtRqst.Rows.Add(dr);

            }

            //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_CELL_INFO_INSERT_LOSS_UI", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
            Util.GridSetData(toSpread, dtRslt, FrameOperation, false);

            txtBadInsertRow.Text = dtRslt.Select("CHK = '0'").Length.ToString();
            txtInsertCellCnt.Text = dtRslt.Rows.Count.ToString();

            toSpread.Refresh();
        }

        public static DataTable GetLossEqp(string sLaneId, string sEqpType, bool bCodeDisplay = false)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = sLaneId;
                dr["EQPTTYPE"] = sEqpType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_EQPT_IN_EQPTTYPE", "RQSTDT", "RSLTDT", RQSTDT);

                return SetCodeDisplay(dtResult, bCodeDisplay);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "]" + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Math.Abs(iRowcount) > 0)
                {
                    if (iRowcount > 900)
                    {
                        //최대 ROW수는 900입니다.
                        Util.MessageValidation("SFU4648", "900");
                        return;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkErpClosingFlag(DateTime RegDttm)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("REG_DTTM", typeof(DateTime));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REG_DTTM"] = RegDttm;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_CHK_ERP_CLOSING_FLAG", "INDATA", null, dtRqst);

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("SUBLOTID", typeof(string));
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["SUBLOTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dataGrid, dataTable, FrameOperation);
                }
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void GetRestoreUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtRestoreUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndRestoreUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void GetReleaseUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtReleaseUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndReleaseUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserID.Text = wndPerson.USERID;
            }
        }

        private void wndRestoreUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtRestoreUserName.Text = wndPerson.USERNAME;
                txtRestoreUserID.Text = wndPerson.USERID;
            }
        }

        private void wndReleaseUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtReleaseUserName.Text = wndPerson.USERNAME;
                txtReleaseUserID.Text = wndPerson.USERID;
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = window.USERNAME;
                txtUserID.Text = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void wndRestoreShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtRestoreUserName.Text = window.USERNAME;
                txtRestoreUserID.Text = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private int BizWF_Check(string SubLotID)
        {
            int RetVal = -1;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TD_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = SubLotID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TD_FLAG"] = "Z";
                dtRqst.Rows.Add(dr);

                //ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SMPL_COM_CHK", "INDATA", "OUTDATA", dtRqst);

                RetVal = Convert.ToInt16(dtRslt.Rows[0]["RETVAL"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return RetVal;
            }

            return RetVal;
        }

        //불량그룹
        private DataTable SetDfctKindCode(string sDfctGrpTypeCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_GR_TYPE_CODE"] = Util.NVC(cboInsertDfctGrType.SelectedValue);
                dr["USE_FLAG"] = 'Y';
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DEFECT_KIND", "RQSTDT", "RSLTDT", RQSTDT);

                //cbo.DisplayMemberPath = "CBO_NAME";
                //cbo.SelectedValuePath = "CBO_CODE";
                //cbo.ItemsSource = DataTableConverter.Convert(SetCodeDisplay(RSLTDT, bCodeDisplay));

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //불량코드 
        private DataTable SetDfctCode(string sDfctGrpTypeCode, string sDfctTypeCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                dr["DFCT_TYPE_CODE"] = sDfctTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DFCT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                //cbo.DisplayMemberPath = "CBO_NAME";
                //cbo.SelectedValuePath = "CBO_CODE";
                //cbo.ItemsSource = DataTableConverter.Convert(SetCodeDisplay(RSLTDT, bCodeDisplay));

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetDfctLotDetlType(string sDfctTypeCode)
        {
            string sLotDetlType = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_DFCT_TYPE_CODE";
                dr["CMCODE"] = sDfctTypeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    if (String.IsNullOrEmpty(dtRslt.Rows[0]["ATTRIBUTE2"].ToString()))
                        sLotDetlType = "N"; //공통코드 속성2번을 활용하여 하고 싶지만, 현재 셀렉터에서 사용하고 있는 듯하여 추후 확인 후, 수정하도록
                    else
                        sLotDetlType = dtRslt.Rows[0]["ATTRIBUTE2"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return sLotDetlType;
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-SELECT-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                DataRow dr = RQSTDT.NewRow();
                RQSTDT.TableName = "RQSTDT";
                string bizRuleName = "";

                bizRuleName = "DA_BAS_SEL_CMN_CBO";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        // 2024.07.23 Ivan P add a method to retrieve common code value for max number of cell can be entered in Cell ID texbox
        private string GetCommonCode(string sComTypeCode, string sComCode)
        {
            string sValue = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sComTypeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ALL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (row["CBO_CODE"].ToString().Equals(sComCode))
                    {
                        sValue = row["ATTR1"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return sValue;
        }
        #endregion

        private void btnChgLoss_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgChgLossList.ItemsSource);
            if (dt.Columns.Count < 2)
            {
                //조회된 값이 없습니다.
                Util.MessageValidation("FM_ME_0232");
                return;
            }

            if (!dgChgLossList.IsCheckedRow("CHK"))
            {
                //선택된 Cell ID가 없습니다.
                Util.MessageValidation("FM_ME_0161");
                return;
            }

            string sLossCode = Util.GetCondition(cboInsertDefect1, bAllNull: true);
            if (string.IsNullOrEmpty(sLossCode) || sLossCode.Contains("SELECT"))
            {
                Util.Alert("FM_ME_0149"); //불량코드를 선택해주세요.
                return;
            }

            Util.MessageConfirm("SFU10023", (result) => //불량 Cell 복구를 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    int k = 0;

                    DataSet dsInDataSet = new DataSet();

                    // 복구 data set 구성
                    DataTable restoreInData = new DataTable();
                    restoreInData.TableName = "RES_INDATA";
                    restoreInData.Columns.Add("SRCTYPE", typeof(string));
                    restoreInData.Columns.Add("IFMODE", typeof(string));
                    restoreInData.Columns.Add("SUBLOTID", typeof(string));
                    restoreInData.Columns.Add("USERID", typeof(string));
                    dsInDataSet.Tables.Add(restoreInData);

                    // 등록 data set 구성
                    DataTable registerInData = new DataTable();
                    registerInData.TableName = "REG_INDATA";
                    registerInData.Columns.Add("SRCTYPE", typeof(string));
                    registerInData.Columns.Add("IFMODE", typeof(string));
                    registerInData.Columns.Add("USERID", typeof(string));
                    registerInData.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                    registerInData.Columns.Add("REMARKS_CNTT", typeof(string));
                    registerInData.Columns.Add("CALDATE", typeof(DateTime));
                    registerInData.Columns.Add("MENUID", typeof(string));
                    registerInData.Columns.Add("USER_IP", typeof(string));
                    registerInData.Columns.Add("PC_NAME", typeof(string));
                    dsInDataSet.Tables.Add(registerInData);

                    DataRow drInData = registerInData.NewRow();
                    drInData["SRCTYPE"] = "UI";
                    drInData["IFMODE"] = "OFF";
                    drInData["USERID"] = LoginInfo.USERID;
                    drInData["LOT_DETL_TYPE_CODE"] = 'N';
                    drInData["REMARKS_CNTT"] = txtRemark1.Text;
                    drInData["CALDATE"] = DateTime.Now;
                    drInData["MENUID"] = LoginInfo.CFG_MENUID;
                    drInData["USER_IP"] = LoginInfo.USER_IP;
                    drInData["PC_NAME"] = LoginInfo.PC_NAME;
                    registerInData.Rows.Add(drInData);


                    DataTable dtIN_SUBLOT = new DataTable();
                    dtIN_SUBLOT.TableName = "REG_IN_SUBLOT";
                    dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                    dtIN_SUBLOT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                    dtIN_SUBLOT.Columns.Add("DFCT_CODE", typeof(string));
                    dsInDataSet.Tables.Add(dtIN_SUBLOT);

                    string errorString = string.Empty;
                    string[] resultString = new string[dgChgLossList.GetRowCount()];

                    for (int i = 2; i < dgChgLossList.Rows.Count; i++)
                    {
                        // check 한 대상만 포함
                        if (dgChgLossList.GetValue(i, "CHK").ToString().Equals("0"))
                        {
                            // check 하지 않은 데이터는 비고에 ""
                            resultString[i - 2] = "";
                            break;
                        }

                        try
                        {
                            restoreInData.Rows.Clear();
                            k = i;
                            DataRow dr1 = restoreInData.NewRow();
                            dr1["SRCTYPE"] = "UI";
                            dr1["IFMODE"] = "OFF";
                            dr1["SUBLOTID"] = dgChgLossList.GetValue(i, "SUBLOTID");
                            dr1["USERID"] = LoginInfo.USERID;
                            restoreInData.Rows.Add(dr1);

                            dtIN_SUBLOT.Rows.Clear();

                            DataRow drInSublot = dtIN_SUBLOT.NewRow();
                            drInSublot["SUBLOTID"] = dgChgLossList.GetValue(i, "SUBLOTID");
                            drInSublot["DFCT_GR_TYPE_CODE"] = "5";
                            drInSublot["DFCT_CODE"] = sLossCode;
                            dtIN_SUBLOT.Rows.Add(drInSublot);

                            DataSet dtRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_CELL_CHG_LOSS_UI", "RES_INDATA,REG_INDATA,REG_IN_SUBLOT", "RES_OUTDATA,REG_OUTDATA", dsInDataSet, FrameOperation.MENUID);

                            // RES_OUTDATA : 복구 시 RETURN 값
                            if (!string.IsNullOrEmpty(dtRslt.Tables[0].Rows[0]["NO_SUBLOTID"].ToString()))
                            {
                                //NO_SUBLOTID String 불량항목 없어서 복구되지 않은 SUBLOTID
                                //Util.MessageInfo("FM_ME_0366", dtRslt.Tables[0].Rows[0]["NO_SUBLOTID"].ToString());
                                resultString[i - 2] = "FAIL";
                                continue;
                            }

                            // REG_OUTDATA : 등록 시 RETURN 값
                            if (dtRslt.Tables[1].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //dgChgLossList.SetValue(i, "DENY_DESC", "SUCCESS");
                                // 정상 변경
                                resultString[i - 2] = "SUCCESS";
                            }
                        }
                        catch (Exception ex)
                        {
                            //dgChgLossList.SetValue(k, "DENY_DESC", "FAIL");
                            resultString[i - 2] = "FAIL";
                            continue;
                        }

                    }

                    // 비고란을 통해 성공여부를 확인할 수 있습니다.
                    Util.MessageInfo("FM_ME_0448");

                    //정상 종료시 데이터 변경을 위해서 재 조회
                    btnSearch5_Click(null, null);

                    // 조회 이후에 비고 입력함
                    for (int i = 0; i < resultString.Length; i++)
                    {
                        dgChgLossList.SetValue(i + 2, "DENY_DESC", resultString[i]);
                    }

                    txtBadInsertRow1.Text = resultString.Where(s => s == "FAIL").Count().ToString();
                    txtInsertCellCnt1.Text = resultString.Where(s => s == "SUCCESS").Count().ToString();
                }
            });
        }

        private void btnRefresh3_Click(object sender, RoutedEventArgs e)
        {
            dgChgLossList.ClearRows();
            DataGridRowAdd(dgChgLossList, Convert.ToInt32(txtRowCntInsertCell1.Text));
        }

        private void btnExcel3_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgChgLossList);
        }

        private void btnSearch5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder cellIDs = new StringBuilder();
                DataTable dtRslt = new DataTable();
                string sCellID = string.Empty;

                for (int iRow = 0; iRow < dgChgLossList.GetRowCount(); iRow++)
                {
                    string sTemp = Util.NVC(DataTableConverter.GetValue(dgChgLossList.Rows[iRow + 2].DataItem, "SUBLOTID"));
                    if (sTemp.Trim() == string.Empty)
                        break;

                    sCellID += sTemp + ",";

                    #region BizWF 
                    int RetVal = BizWF_Check(sTemp);

                    if (RetVal != 0)
                    {
                        return;
                    }
                    #endregion
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellID;
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_DFCT_INFO", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                Util.GridSetData(dgChgLossList, dtRslt1, FrameOperation, false);

                txtInsertCellCnt1.Text = dtRslt1.Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgChgLossList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name != null && e.Column.Name.Equals("SUBLOTID"))
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Red;
            }
            else
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Black;
            }
        }

        private void dgChgLossList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DENY_DESC"))
                {
                    if (dataGrid[e.Cell.Row.Index, 0].Presenter != null)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                        {
                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            if (cb != null)
                            {
                                cb.Visibility = Visibility.Hidden;
                                cb.IsChecked = false;
                            }
                        }
                        else
                        {

                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            cb.Visibility = Visibility.Visible;
                        }
                    }
                }

                if (e.Cell.Row.Index == 0 && e.Cell.Column.Name.Equals("SUBLOTID"))
                {
                    C1.WPF.DataGrid.DataGridColumnHeaderPresenter dgcp =
                    (C1.WPF.DataGrid.DataGridColumnHeaderPresenter)e.Cell.Presenter.MergedRange.TopLeftCell.Presenter.Content;
                    dgcp.Foreground = Brushes.Red;
                }
            }));
        }

        // [E20240422-000378] 건으로 폴란드 활성화만 반영하기 위해서 함수 생성
        private bool GetCellChgLossTab()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_CELL_CHG_LOSS_TAB";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}

