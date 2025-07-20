/*************************************************************************************
 Created Date : 2023.11.30
      Creator : 이정미 
   Decription : 공 Tray 관리 (ESWA 전용)
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.30 DEVELOPER : Initial Created.
  2023.12.27 이정미    : 상태 콤보박스 수정
  2023.12.28 이정미    : 세척시간 추가 및 조회 오류 수정
  2023.12.28 이정미    : Offiline 클릭 시 위치 콤보박스 수정
  2024.01.31 이정미    : 엑셀 조회 오류 수정 
  2024.05.21 양강주    : TRAY 자동 세척 기능을 사용하는 경우, MMD 동별 공통코드(CARRIER_CLEAN_USE_COUNT_MNGT_PROC) 규칙을 따르도록 기능 수정
                         AS-IS : Y(세척 필요), N(세척 완료) , TO-BE : Q(세척 필요), Y(세척 완료), N(초기 상태)
  2024.08.23 조영대    : 상태 비정상(U) 추가
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_026_ESWA : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sOnOff;
        private string _sTrayType;
        private string _sLocGrp;
        private string _sBldgCd;
        private string _sActYN = "N";
        private string _sCMCODE;
        private string _sEQPTYPE = "N";

        Util _Util = new Util();

        private readonly Util _util = new Util();
        private bool _useTrayCleanOp = false;   // 2024.05.21 TRAY 자동 세척 기능을 사용하는 경우 true, AS-IS 방식의 경우(MMD 등록하지 않은 경우) false


        public string ON_OFF
        {
            set { this._sOnOff = value; }
        }

        public string TRAY_TYPE
        {
            set { this._sTrayType = value; }
        }

        public string LOC_GRP
        {
            set { this._sLocGrp = value; }
        }

        public string BLDG_CD
        {
            set { this._sBldgCd = value; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }

        public FCS001_026_ESWA()
        {
            InitializeComponent();
            InitCombo();
            initDefault();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                _sOnOff = Util.NVC(parameters[0]);
                _sTrayType = Util.NVC(parameters[1]);
                _sLocGrp = Util.NVC(parameters[2]);
                _sBldgCd = Util.NVC(parameters[3]);
                _sActYN = Util.NVC(parameters[4]);

                if (parameters.Length > 5)
                {
                    _sCMCODE = Util.NVC(parameters[6]);
                    _sEQPTYPE = Util.NVC(parameters[7]);

                }

                InitCombo();
                initDefault();
                GetList();
            }

            #region 활성화 공정 화면 추가 제어
            {
                _useTrayCleanOp = (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") == true && _util.IsAreaCommoncodeAttrUse("CARRIER_CLEAN_USE_COUNT_MNGT_PROC", "FORM_TRAY", new string[] { "" }) == true);

                #region [ 활성화 Tray 세척 기능 사용여부에 따른 숨김 처리]
                if (_useTrayCleanOp == false)
                {
                    dgEmptyTray.Columns["CLEAN_BAS_COUNT"].Visibility = Visibility.Hidden;
                    dgEmptyTray.Columns["USE_COUNT"].Visibility = Visibility.Hidden;
                    dgEmptyTray.Columns["ACCU_USE_COUNT"].Visibility = Visibility.Hidden;
                    dgEmptyTray.Columns["LAST_CLEAN_TIME"].Visibility = Visibility.Hidden;
                }
                #endregion
            }
            #endregion

            Loaded -= UserControl_Loaded;
        }

        private void initDefault()
        {
            if (!string.IsNullOrEmpty(_sOnOff))
            {
                if (_sOnOff.Equals("ON"))
                    rdoOn.IsChecked = true;
                else if (_sOnOff.Equals("OFF"))
                    rdoOff.IsChecked = true;
            }

            if (!string.IsNullOrEmpty(_sTrayType))
                cboTrayType.SelectedValue = _sTrayType;
            if (!string.IsNullOrEmpty(_sLocGrp))
                cboLoc.SelectedValue = _sLocGrp;

            // 수동 OFF 사용권한 설정
            SetManualOffButtonVisible();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            #region 조회   

            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.NONE, sCase: "TRAYTYPE");

            string[] sFilter1 = { "COMBO_TRAY_STATUS" };
            _combo.SetCombo(cboState, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter1);

            //2023.10.03 위치 콤보박스 변경 START
            string[] sFilter = { "EQP_LOC_GRP_CD" };
            SetEqptLoc(cboLoc, sFilter: sFilter);
            //2023.10.03 위치 콤보박스 변경 END
            #endregion

            #region 폐기 Tray 조회  
            _combo.SetCombo(cboTrayTypeScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE");
            //2021.04.21 위치정보 그룹화 대응 START
            //_combo.SetCombo(cboLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "ETRAY_LOC");
            string[] sFilterSrp = { "EQP_LOC_GRP_CD" };
            //_combo.SetCombo(cboLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterSrp);
            //2021.04.21 위치정보 그룹화 대응 END
            _combo.SetCombo(cboLocScrap, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilterSrp);
            #endregion
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgEmptyTray);

                string sBiz = "";

                if ((bool)rdoOn.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_CARRIER";
                else sBiz = "DA_SEL_OFFLINE_EMPTY_CARRIER";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("EQPTSHORTNAME", typeof(string));
                inDataTable.Columns.Add("OFLN_LOCATION_CODE", typeof(string));
                inDataTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
                inDataTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType);
                dr["CST_MNGT_STAT_CODE"] = "I";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!string.IsNullOrEmpty(Util.GetCondition(cboLoc, bAllNull: true)))
                {
                    if ((bool)rdoOn.IsChecked)
                        dr["EQPTSHORTNAME"] = Util.GetCondition(cboLoc, bAllNull: true);
                    if ((bool)rdoOff.IsChecked)
                        dr["OFLN_LOCATION_CODE"] = Util.GetCondition(cboLoc).Equals("OFFLINE") ? " " : Util.GetCondition(cboLoc);
                }

                if (!string.IsNullOrEmpty(Util.GetCondition(cboState, bAllNull: true)))
                {
                    String toClean = (_useTrayCleanOp == true) ? "Q" : "Y";
                    String cleanOK = (_useTrayCleanOp == true) ? "Y,N" : "N";

                    dr["CST_CLEAN_FLAG"] = Util.GetCondition(cboState).Equals("ONCLEAN") ? toClean : cleanOK;
                    dr["ABNORM_TRF_RSN_CODE"] = "INNERDETECT;CELLDETECT;U".IndexOf(Util.GetCondition(cboState)) >= 0 ? Util.GetCondition(cboState) : " ";
                }

                if (!string.IsNullOrEmpty(txtTrayID.Text)) dr["CSTID"] = txtTrayID.Text;

                inDataTable.Rows.Add(dr);

                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = false;

                //백그라운드 처리, 비즈 실행 후 dgEmptyTray_ExecuteDataModify, dgEmptyTray_ExecuteDataCompleted 실행.
                dgEmptyTray.ExecuteService(sBiz, "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = true;
            }
        }

        private void GetList(string sTrayList)
        {
            try
            {
                string sBiz = "";

                if ((bool)rdoOn.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_CARRIER";
                else sBiz = "DA_SEL_OFFLINE_EMPTY_CARRIER";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["CSTID"] = sTrayList;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(dr);

                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = false;

                // 백그라운드 처리, 비즈 실행 후 dgEmptyTray_ExecuteDataModify, dgEmptyTray_ExecuteDataCompleted 실행.
                dgEmptyTray.ExecuteService(sBiz, "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = true;
            }
        }

        private void dgEmptyTray_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtRslt = e.ResultData as DataTable;
            if (dtRslt.Rows.Count > 0) dtRslt.Columns.Add("CHK", typeof(bool));
            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                dtRslt.Rows[i]["CHK"] = false;
            }
        }

        private void dgEmptyTray_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            // 조회 완료
            btnSearch.IsEnabled = btnExcel.IsEnabled = btnSelect.IsEnabled = btnManualOff.IsEnabled = btnManualOut.IsEnabled = btnStatusChange.IsEnabled = true;
        }

        private void GetListScrap()
        {
            Util.gridClear(dgScrapTray);

            string sBiz = "";

            if ((bool)rdoOnScrap.IsChecked) sBiz = "DA_SEL_ONLINE_EMPTY_CARRIER";
            else sBiz = "DA_SEL_OFFLINE_EMPTY_CARRIER";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("TRAY_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
            inDataTable.Columns.Add("EQPTSHORTNAME", typeof(string));
            inDataTable.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));


            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayTypeScrap, bAllNull: true);
            dr["EQPTSHORTNAME"] = Util.GetCondition(cboLocScrap, bAllNull: true);
            dr["CST_MNGT_STAT_CODE"] = "S"; //폐기
            dr["ABNORM_TRF_RSN_CODE"] = "INNERDETECT;CELLDETECT;U".IndexOf(Util.GetCondition(cboState)) >= 0 ? Util.GetCondition(cboState) : " ";
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            if (!string.IsNullOrEmpty(txtTrayIDScrap.Text)) dr["CSTID"] = txtTrayIDScrap.Text;

            inDataTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", inDataTable);
            if (dtRslt.Rows.Count > 0)
            {
                dtRslt.Columns.Add("CHK", typeof(bool));
            }
            Util.GridSetData(dgScrapTray, dtRslt, this.FrameOperation);
        }

        private void SetTrayEnd(string sTray, int iRow)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("TRAY_ID", typeof(string));
                inDataTable.Columns.Add("MDF_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["TRAY_ID"] = sTray;
                dr["MDF_ID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EMPTY_TRAY_END", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void SetEqptIdName(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO_EMPTYTRAY", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "] " + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private void EmptyStorageDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);
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
                        dr["USE_FLAG"] = "Y";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
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

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                DataRow dr = RQSTDT.NewRow();
                RQSTDT.TableName = "RQSTDT";
                string bizRuleName = "";

                if (sClassId == "USE_FLAG")
                {
                    bizRuleName = "DA_BAS_SEL_CMN_CBO";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = sClassId;
                }
                /*else if (sClassId == "EQPTID")
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_EMPTYTRAY";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                }*/
                else if (sClassId == "EQPT_GR_TYPE_CODE")
                {
                    bizRuleName = "DA_BAS_SEL_CMN_CBO";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = sClassId;
                    dr["CMCODE_LIST"] = "3, 7"; // 3: 상온Aging, 7:출하Aging
                }

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
        
        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - dg.TopRows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        //2023.10.03 위치 콤보박스 변경 START 
        private void SetEqptLoc(C1ComboBox cbo, String[] sFilter, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //2023.10.03 위치 콤보박스 변경 END 

        private void SetManualOffButtonVisible()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDTYPE"] = "FORM_EMPTY_TRAY_MANUAL_USE";
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_USER_AUTH_INFO_FOR_BTN", "RQSTDT", "RSLTDT", inTable);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    // 공통코드가 등록되어 있으면 들어옴.
                    bool isAllowButton = false; // 기본 사용금지로 설정
                    foreach (DataRow drAuth in dtResult.Rows)
                    {
                        if (Util.NVC(drAuth["AUTH_USE_YN"]).Equals("Y"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(drAuth["AUTHID"]))) isAllowButton = true;
                        }
                        else if (Util.NVC(drAuth["AUTH_USE_YN"]).Equals("N"))
                        {
                            if (string.IsNullOrEmpty(Util.NVC(drAuth["AUTHID"]))) isAllowButton = true;
                        }
                    }

                    if (isAllowButton)
                    {
                        btnManualOff.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnManualOff.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Event]

        #region [조회 Tab]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void btnStatusChange_Click(object sender, RoutedEventArgs e)
        {

            FCS001_026_STATUS_CHANGE wndPopup = new FCS001_026_STATUS_CHANGE();
            wndPopup.FrameOperation = FrameOperation;

            string sTrayList = string.Empty;

            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + "|";
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            FCS001_026_STATUS_CHANGE fcs001_026_status_change = new FCS001_026_STATUS_CHANGE();
            fcs001_026_status_change.FrameOperation = FrameOperation;
            if (fcs001_026_status_change != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;
                C1WindowExtension.SetParameters(fcs001_026_status_change, Parameters);

                fcs001_026_status_change.Closed += new EventHandler(wndPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => fcs001_026_status_change.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_026_STATUS_CHANGE window = sender as FCS001_026_STATUS_CHANGE;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnManualOut_Click(object sender, RoutedEventArgs e)
        {
            FCS001_026_MANUAL_OUT_TRAY manualOutTray = new FCS001_026_MANUAL_OUT_TRAY();
            manualOutTray.FrameOperation = FrameOperation;

            if (manualOutTray != null)
            {
                string sTrayList = string.Empty;

                for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                    {
                        sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")) + "|";
                    }
                }

                if (string.IsNullOrEmpty(sTrayList))
                {
                    Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                    return;
                }

                object[] Parameters = new object[2];
                Parameters[0] = sTrayList;
                Parameters[1] = cboTrayType.SelectedValue;

                C1WindowExtension.SetParameters(manualOutTray, Parameters);

                manualOutTray.Closed += new EventHandler(ManualOutTray_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => manualOutTray.ShowModal()));
            }
        }

        private void ManualOut_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT window = sender as FCS001_026_MANUAL_OUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutLoc_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT_LOC window = sender as FCS001_026_MANUAL_OUT_LOC;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutPort_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT_PORT window = sender as FCS001_026_MANUAL_OUT_PORT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutTray_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OUT_TRAY window = sender as FCS001_026_MANUAL_OUT_TRAY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnManualOff_Click(object sender, RoutedEventArgs e)
        {
            string sTrayList = string.Empty;
            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + "|";
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            FCS001_026_MANUAL_OFF fcs001_026_manual_off = new FCS001_026_MANUAL_OFF();
            fcs001_026_manual_off.FrameOperation = FrameOperation;
            if (fcs001_026_manual_off != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;
                C1WindowExtension.SetParameters(fcs001_026_manual_off, Parameters);

                fcs001_026_manual_off.Closed += new EventHandler(fcs001_026_manual_off_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => fcs001_026_manual_off.ShowModal()));
            }
        }

        private void fcs001_026_manual_off_Closed(object sender, EventArgs e)
        {
            FCS001_026_MANUAL_OFF window = sender as FCS001_026_MANUAL_OFF;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnShippingCtl_Click(object sender, EventArgs e)
        {
            // 2023.11.13 SHIPPING OUTPUT CONTROL ADD
            FCS001_026_SHIPPING_OUTPUT_CONTROL shippingOutputControl = new FCS001_026_SHIPPING_OUTPUT_CONTROL();
            shippingOutputControl.FrameOperation = FrameOperation;

            this.Dispatcher.BeginInvoke(new Action(() => shippingOutputControl.ShowModal()));

            shippingOutputControl.Closed += new EventHandler(btnShippingCtl_Closed);
        }

        private void btnShippingCtl_Closed(object sender, EventArgs e)
        {
            FCS001_026_SHIPPING_OUTPUT_CONTROL window = sender as FCS001_026_SHIPPING_OUTPUT_CONTROL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < numCnt.Value && i < dgEmptyTray.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", true);
            }
        }

        /// <summary>
        /// Excel 업로드
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            GetList(GetExcelData());

        }

        private string GetExcelData()
        {
            string sColData = string.Empty;

            Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    sColData = LoadExcel(stream, (int)0);
                }
            }
            return sColData;
        }

        private string LoadExcel(Stream excelFileStream, int sheetNo)
        {
            string sColData = string.Empty;
            try
            {
                string sSeparator = "|";
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                }

                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null)
                    {
                        sColData += sheet.GetCell(rowInx, 0).Text + sSeparator;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sColData;
        }
        // 전체 선택, 해제
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgEmptyTray);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgEmptyTray);

        }
        #endregion

        #region [폐기 Tray 조회 Tab]
        private void btnSearchScrap_Click(object sender, RoutedEventArgs e)
        {
            GetListScrap();
        }

        //전체선택 , 전체선택 해제
        private void chkHeaderAllScrap_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgScrapTray);
        }

        private void chkHeaderAllScrap_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgScrapTray);
        }

        #endregion


        private void dgEmptyTray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //동일한 설비 체크하기
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEmptyTray.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                bool bCheck = !(bool)DataTableConverter.GetValue(dgEmptyTray.Rows[cell.Row.Index].DataItem, "CHK");
                string sEqpID = string.Empty;
                sEqpID = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[cell.Row.Index].DataItem, "LOAD_REP_CSTID"));

                if (string.IsNullOrEmpty(sEqpID)) return;
                for (int i = 0; i < this.dgEmptyTray.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")).Equals(sEqpID)
                        && i != cell.Row.Index)
                    {
                        DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", bCheck);
                    }
                }
            }
        }

        private void dgEmptyTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgEmptyTray.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgScrapTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgScrapTray.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private bool IsCnvrLocationOutLocUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CNVR_LOCATION_OUT_LOC_USE";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private bool IsBothPortIdUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "BOTH_PORT_ID_USE";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private bool IsCnvDestinationOutUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CNVR_DESTINATION_OUT_USE";
                dr["COM_CODE"] = "CNVR_DESTINATION_OUT_USE";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        #endregion

        private void rdoOn_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked)
            {
                CommonCombo_Form _combo = new CommonCombo_Form();
                string[] sFilter = { "EQP_LOC_GRP_CD" };
                SetEqptLoc(cboLoc, sFilter: sFilter);
            }
        }

        private void rdoOff_CheckedChanged(object sender, bool isChecked, RoutedEventArgs e)
        {
            if (isChecked)
            {
                CommonCombo_Form _combo = new CommonCombo_Form();
                string[] sFilter = { "FORM_OFF_EQP_LOC_GRP_CD" };
                SetEqptLoc(cboLoc, sFilter: sFilter);
            }

        }
    }
}


