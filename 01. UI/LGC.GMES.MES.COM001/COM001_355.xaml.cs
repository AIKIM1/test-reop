/*COM001_045(생산실적 조회(생산Lot 기준))를 복사하여 사용*/

using C1.WPF;
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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_355 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _StackingYN = string.Empty;
        string _AREATYPE = "";
        string _AREAID = "";
        string _PROCID = "";
        string _EQSGID = "";
        string _EQPTID = "";
        string _LOTID = "";
        string _WIPSEQ = "";
        string _LANEPTNQTY = "";
        string _WIP_NOTE = "";

        string _EQGRID = string.Empty;

        bool _bLoad = true;

        List<string> _MColumns1;
        List<string> _MColumns2;

        int idx = 0;
        int idx2 = 0;

        private BizDataSet _Biz = new BizDataSet();
        public COM001_355()
        {
            InitializeComponent();

            InitCombo();
            InitColumnsList();          // 단위 환산 체크시 칼럼 Visible 

            GetAreaType(cboProcess.SelectedValue.ToString());
            AreaCheck(cboProcess.SelectedValue.ToString());
            SetProcessNumFormat(cboProcess.SelectedValue.ToString());
            SetBtnVisibility();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_ASSY", cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS_ASSY");

            //if (cboProcess.Items.Count < 1)
            //    SetProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENT_ASSY", cbParent: cboEquipmentParent);

            //경로흐름
            string[] sFilter = { "FLOWTYPE" };
            _combo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 시장유형
            string[] sFilterMKType = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKTtype, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterMKType);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;

            cboProcess.IsEnabled = false;

            setComboLotType();

            //// 모델 AutoComplete
            //GetModel();

        }

        #endregion
        private void SetBtnVisibility()
        {
            DataTable dtInTable = new DataTable();
            dtInTable.Columns.Add("CMCDTYPE", typeof(string));
            dtInTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dtInTable.NewRow();
            dr["CMCDTYPE"] = "PILOT_PROD_INPUT_USER";
            dr["CMCODE"] = LoginInfo.USERID;

            dtInTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtInTable);

            if (dtRslt.Rows.Count > 0)
            {
                btnPilotProdInfo.Visibility = Visibility.Visible;
            }
            else
            {
                btnPilotProdInfo.Visibility = Visibility.Collapsed;
            }
        }


        private void InitColumnsList()
        {
            _MColumns1 = new List<string>();
            _MColumns1.Add("EQPT_END_QTY");
            _MColumns1.Add("INPUT_QTY");
            _MColumns1.Add("WIPQTY_ED");
            _MColumns1.Add("CNFM_DFCT_QTY");
            _MColumns1.Add("CNFM_LOSS_QTY");
            _MColumns1.Add("CNFM_PRDT_REQ_QTY");
            _MColumns1.Add("LENGTH_EXCEED");
            _MColumns1.Add("WIPQTY2_ED");
            _MColumns1.Add("CNFM_DFCT_QTY2");
            _MColumns1.Add("CNFM_LOSS_QTY2");
            _MColumns1.Add("CNFM_PRDT_REQ_QTY2");
            _MColumns1.Add("LENGTH_EXCEED2");

            _MColumns2 = new List<string>();
            _MColumns2.Add("EQPT_END_QTY_EA");
            _MColumns2.Add("INPUT_QTY_EA");
            _MColumns2.Add("WIPQTY_ED_EA");
            _MColumns2.Add("CNFM_DFCT_QTY_EA");
            _MColumns2.Add("CNFM_LOSS_QTY_EA");
            _MColumns2.Add("CNFM_PRDT_REQ_QTY_EA");
            _MColumns2.Add("LENGTH_EXCEED_EA");
            _MColumns2.Add("WIPQTY2_ED_EA");
            _MColumns2.Add("CNFM_DFCT_QTY2_EA");
            _MColumns2.Add("CNFM_LOSS_QTY2_EA");
            _MColumns2.Add("CNFM_PRDT_REQ_QTY2_EA");
            _MColumns2.Add("LENGTH_EXCEED2_EA");
        }

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;


            // 전극 등급 표시여부
            EltrGrdCodeColumnVisible();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();
        }
        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
                //cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
                //SetProcess();
                //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
                //SetEquipment();
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                ClearValue();

                GetAreaType(cboProcess.SelectedValue.ToString());
                AreaCheck(cboProcess.SelectedValue.ToString());
                SetProcessNumFormat(cboProcess.SelectedValue.ToString());

                //2019.09.25 김대근 : CNB전극이고 CT/RP/ST에서만 CarrierID를 보여주도록 했음
                if (LoginInfo.CFG_AREA_ID.Equals("EC"))
                {
                    if (cboProcess.SelectedValue.ToString().Equals(Process.COATING)
                        || cboProcess.SelectedValue.ToString().Equals(Process.ROLL_PRESSING)
                        || cboProcess.SelectedValue.ToString().Equals(Process.SLITTING))
                    {
                        dgLotList.Columns["PR_CSTID"].Visibility = Visibility.Visible;
                        dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLotList.Columns["PR_CSTID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        #endregion

        #region [동] - 조회 조건
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 전극 등급 표시여부
            EltrGrdCodeColumnVisible();
        }
        #endregion

        #region [작업일] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                // 조회 버튼 클릭시로 변경
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "31");

                //    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                //    //dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    if (LGCdp.Name.Equals("dtpDateTo"))
                //        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                //    else
                //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                //    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

                //// To 일자 변경시 From일자 1일자로 변경
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
        }
        #endregion

        #region [LOT] - 조회 조건
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [모델] - 조회 조건
        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {
            // 모델 AutoComplete
            if (_bLoad)
            {
                GetModel();
                _bLoad = false;
            }
        }
        #endregion

        #region [프로젝트] - 조회 조건
        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [M수량환산] - 조회 조건
        private void chkPtnLen_Checked(object sender, RoutedEventArgs e)
        {
            // Visibility.Collapsed
            foreach (string str in _MColumns1)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;
            }

            // Visibility.Visible
            foreach (string str in _MColumns2)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;
            }

        }

        private void chkPtnLen_Unchecked(object sender, RoutedEventArgs e)
        {
            // Visibility.Visible
            foreach (string str in _MColumns1)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;
            }

            // Visibility.Collapsed
            foreach (string str in _MColumns2)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;
            }

        }
        #endregion

        #region [모델합산] - 조회 조건
        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 1;
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 0;
        }
        #endregion

        #region [작업대상 목록 에서 선택]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                string sDate = DataTableConverter.GetValue(rb.DataContext, "STARTDTTM").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));
                string sEqptID = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                SetProcessNumFormat(_PROCID);

            }
        }
        #endregion


        #endregion

        #region Mehod

        #region [BizCall]

        #region [### 공정의 AreaType ###]
        public void GetAreaType(string sProcID)
        {
            try
            {
                ShowLoadingIndicator();

                _AREATYPE = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = sProcID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTPROCESS", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                    _AREATYPE = dtRslt.Rows[0]["PCSGID"].ToString();
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

        #region [### 실적 일자로 조회 ###]
        public void GetCaldate()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
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
        #endregion

        #region [동별 전극 등급 Visible]  
        private void EltrGrdCodeColumnVisible()
        {
            try
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString() == "SELECT")
                {
                    dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### 작업대상 조회 ###]
        public void GetLotList()
        {
            try
            {

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("TOPBACK", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                ////dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                string sEqptID = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEqptID) ? null : sEqptID;

                string sLotType = Util.GetCondition(cboLotType);
                dr["LOTTYPE"] = string.IsNullOrWhiteSpace(sLotType) ? null : sLotType;

                dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);


                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["AREATYPE"] = _AREATYPE;

                if (cboProcess.SelectedValue.Equals(Process.SLIT_REWINDING) || cboProcess.SelectedValue.Equals(Process.SLITTING) || cboProcess.SelectedValue.Equals(Process.HEAT_TREATMENT))
                {
                    FRST_MKT.Visibility = Visibility.Visible;
                }
                else
                {
                    FRST_MKT.Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrWhiteSpace(txtPRLOTID.Text))
                {
                    dr["PR_LOTID"] = Util.GetCondition(txtPRLOTID);
                }
                else if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotId);
                    bLot = true;
                }

                if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                if (chkProc.IsChecked == false)
                    dr["RUNYN"] = "Y";

                dr["MKT_TYPE_CODE"] = Util.GetCondition(cboMKTtype, bAllNull: true);

                dtRqst.Rows.Add(dr);

                string sBizName = string.Empty;


                sBizName = "DA_PRD_SEL_LOT_LIST_MOBILE_TEST_PROD";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);


                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && bLot == true)
                {
                    _AREATYPE = dtRslt.Rows[0]["AREATYPE"].ToString();
                    AreaCheck(dtRslt.Rows[0]["PROCID"].ToString());
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

        #endregion



        #region[### 조회 조건 모델 조회 ###]
        private void GetModel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //표시 텍스트
                    //string[] keywordString = new string[r["MODLID"].ToString().Length - 1]; //검색 필요 최소 글자수(Threshold)가 2이므로 두 글자씩 묶어서 배열(이진선은 총 세 글자이고 '이진'과 '진선'의 2개의 묶음으로 나눌 수 있으므로 배열의 Count는 2가 된다)로 던져야 검색 가능.
                    string keywordString;


                    //for (int i = 0; i < displayString.Length - 1; i++)
                    //keywordString[i] = displayString.Substring(i, txtModlId.Threshold); //Threshold 만큼 잘라서 배열에 담는다 (위 주석 참조)

                    keywordString = displayString;

                    txtModlId.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString)); //표시 텍스트와 검색어 텍스트(배열)를 AutoCompleteTextBox의 Item에 추가한다.
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
        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void setComboLotType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PILOT_PROD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboLotType.DisplayMemberPath = "CBO_NAME";
                cboLotType.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboLotType.ItemsSource = dtResult.Copy().AsDataView();

                cboLotType.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #region [FOLDING & STACKING 구분]
        //2017.11.21  INS염규범 GEMS FOLDING 실적 마감 단어 수정 요청 CSR20171011_02178
        private void chkFoldingStacking()
        {

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["LOTID"] = _LOTID;

            inDataTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_EQGRID_BYLOTID", "INDATA", "OUTDATA", inDataTable, (dtResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    _EQGRID = dtResult.Rows[0]["EQGRID"].ToString();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            });
        }
        #endregion

        #region [전극 조립 구분]
        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }
        #endregion


        #endregion

        #region [Func]
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

        #region [초기화]
        private void ClearValue()
        {

            _AREAID = "";
            _PROCID = "";
            _EQSGID = "";
            _EQPTID = "";
            _LOTID = "";
            _WIPSEQ = "";
            _LANEPTNQTY = "";
            _WIP_NOTE = "";
            _StackingYN = "";

        }
        #endregion

        #region 탭, Grid Column Visibility
        private void AreaCheck(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;
                chkPtnLen.IsChecked = false;

                #region ################ 주석 #########################
                //dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["SRS1_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["SRS2_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["ROLLPRESS_COUNT"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;

                //dgLotList.Columns["EQPT_END_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["INPUT_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY_ED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY2_ED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED2_EA"].Visibility = Visibility.Collapsed;
                #endregion #######################################

                List<string> ColumnsVisibility = new List<string>();
                ColumnsVisibility.Add("EQPTID_COATER");
                ColumnsVisibility.Add("EQPT_END_PSTN_ID");
                ColumnsVisibility.Add("INPUT_DIFF_QTY");
                ColumnsVisibility.Add("PR_LOTID");
                ColumnsVisibility.Add("WIPQTY2_ED");
                ColumnsVisibility.Add("CNFM_LOSS_QTY2");
                ColumnsVisibility.Add("CNFM_DFCT_QTY2");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY2");
                ColumnsVisibility.Add("PROD_VER_CODE");
                ColumnsVisibility.Add("EQPT_END_QTY");
                ColumnsVisibility.Add("SRS1_QTY");
                ColumnsVisibility.Add("SRS2_QTY");
                ColumnsVisibility.Add("COUNTQTY");
                ColumnsVisibility.Add("ROLLPRESS_COUNT");
                ColumnsVisibility.Add("LENGTH_EXCEED");
                ColumnsVisibility.Add("LENGTH_EXCEED2");
                ColumnsVisibility.Add("TAG_QTY");
                ColumnsVisibility.Add("EQP_TAG_QTY");

                ColumnsVisibility.Add("EQPT_END_QTY_EA");
                ColumnsVisibility.Add("INPUT_QTY_EA");
                ColumnsVisibility.Add("WIPQTY_ED_EA");
                ColumnsVisibility.Add("CNFM_DFCT_QTY_EA");
                ColumnsVisibility.Add("CNFM_LOSS_QTY_EA");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY_EA");
                ColumnsVisibility.Add("LENGTH_EXCEED_EA");
                ColumnsVisibility.Add("WIPQTY2_ED_EA");
                ColumnsVisibility.Add("CNFM_DFCT_QTY2_EA");
                ColumnsVisibility.Add("CNFM_LOSS_QTY2_EA");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY2_EA");
                ColumnsVisibility.Add("LENGTH_EXCEED2_EA");
                ColumnsVisibility.Add("FOIL_LOTID");
                ColumnsVisibility.Add("AUTO_STOP_FLAG");

                // 원형, 조소형 관련
                ColumnsVisibility.Add("DIFF_QTY");
                ColumnsVisibility.Add("INPUT_DIFF_QTY_WS");
                ColumnsVisibility.Add("WIPQTY_END_WS");
                ColumnsVisibility.Add("DFCT_QTY_WS");
                ColumnsVisibility.Add("LOSS_QTY_WS");
                ColumnsVisibility.Add("REQ_QTY_WS");
                ColumnsVisibility.Add("BOXQTY_IN");
                ColumnsVisibility.Add("BOXQTY");
                ColumnsVisibility.Add("WINDING_RUNCARD_ID");
                ColumnsVisibility.Add("LOTID_AS");
                ColumnsVisibility.Add("MODL_CHG_FRST_PROD_LOT_FLAG");
                ColumnsVisibility.Add("WIP_WRK_TYPE_CODE_DESC");

                //C20200519-000286
                dgLotList.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;

                // Visibility.Collapsed
                foreach (string str in ColumnsVisibility)
                {
                    if (dgLotList.Columns.Contains(str))
                        dgLotList.Columns[str].Visibility = Visibility.Collapsed;
                }

                dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                dgLotList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("생산량");

                chkPtnLen.IsEnabled = false;

                dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량");
                dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량");
                dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량");
                dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구");
                dgLotList.Columns["MOLD_ID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["MOLD_USE_COUNT"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                dgLotList.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
                dgLotList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Visible;
                dgLotList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Visible;
                dgLotList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Visible;
                dgLotList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Visible;
                dgLotList.Columns["REQ_QTY_WS"].Visibility = Visibility.Visible;
                dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion

        #region 공정별 숫자 Format
        private void SetProcessNumFormat(string sProcid)
        {
            // 숫자값 Format 
            string sFormat = string.Empty;
            // 조립
            sFormat = "###,##0";

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_DIFF_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED2"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED2_EA"])).Format = sFormat;

        }
        #endregion

        #region [값셋팅]
        private void SetValue(object oContext)
        {
            //txtNote.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));

            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _LANEPTNQTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_PTN_QTY"));
            ////_ORGQTY = Util.NVC(DataTableConverter.GetValue(oContext, "ORG_QTY"));
            _WIP_NOTE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));

            //txtWorkorder.FontWeight = FontWeights.Normal;
            //txtLotStatus.FontWeight = FontWeights.Normal;
            //txtWorkdate.FontWeight = FontWeights.Normal;
            //txtShift.FontWeight = FontWeights.Normal;
            //txtStartTime.FontWeight = FontWeights.Normal;
            //txtWorker.FontWeight = FontWeights.Normal;
            //ldpDatePicker.FontWeight = FontWeights.Normal;
            //txtOutQty.FontWeight = FontWeights.Normal;
            //txtDefectQty.FontWeight = FontWeights.Normal;
            //txtLossQty.FontWeight = FontWeights.Normal;
            //txtPrdtReqQty.FontWeight = FontWeights.Normal;
            //txtNote.FontWeight = FontWeights.Normal;
        }
        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion

        private void dgLotList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (Util.NVC(dg.CurrentColumn.Name).Equals("WIPHOLD") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                    || Util.NVC(dg.CurrentColumn.Name).Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                {
                    ShowHoldDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID")));
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid tmpDg = sender as C1DataGrid;

                    if (tmpDg == null) return;

                    C1.WPF.DataGrid.DataGridCell currCell = tmpDg.CurrentCell;

                    if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                    switch (Convert.ToString(currCell.Column.Name))
                    {
                        case "PILOT_PROD_CHARGE_USERNAME":

                            COM001_355_USER_DETAIL wndDetail = new COM001_355_USER_DETAIL();
                            wndDetail.FrameOperation = FrameOperation;

                            if (wndDetail != null)
                            {
                               
                                object[] Parameters = new object[3];

                                for(int i = 0; i < Parameters.Length; i++)
                                {
                                    Parameters[i] = null;
                                }

                                string sUsers = currCell.Text.ToString();

                                if (string.IsNullOrEmpty(sUsers))
                                    return;

                                string[] result = sUsers.Split(new char[] { ',' });  

                                for (int i = 0; i < result.Length; i++)
                                {
                                    if (result[i].IndexOf("(") < 0)
                                        continue;
                                    Parameters[i] = GetUserId(result[i]);
                                }

                                C1WindowExtension.SetParameters(wndDetail, Parameters);

                                this.Dispatcher.BeginInvoke(new Action(() => wndDetail.ShowModal()));
                            }
                            break;
                    }

                    if (tmpDg.CurrentCell != null)
                        tmpDg.CurrentCell = tmpDg.GetCell(tmpDg.CurrentCell.Row.Index, tmpDg.Columns.Count - 1);
                    else if (tmpDg.Rows.Count > 0)
                        tmpDg.CurrentCell = tmpDg.GetCell(tmpDg.Rows.Count, tmpDg.Columns.Count - 1);

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void ShowHoldDetail(string pLotid)
        {
            COM001_018_HOLD_DETL wndRunStart = new COM001_018_HOLD_DETL();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = pLotid;
                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.ShowModal();
            }
        }

        private void btnHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                CMM001.Popup.CMM_COM_WIPHIST_HIST wndHist = new CMM001.Popup.CMM_COM_WIPHIST_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);

                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_COM_WIPHIST_HIST window = sender as CMM001.Popup.CMM_COM_WIPHIST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnPilotProdInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLotList.ItemsSource == null)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataRow[] drSelect = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = 1");

                if (drSelect.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                COM001_355_TEST_PROD popupTestProd = new COM001_355_TEST_PROD();
                popupTestProd.FrameOperation = this.FrameOperation;

                popupTestProd.Closed += new EventHandler(popupTestProd_Closed);

                object[] parameters = new object[1];
                parameters[0] = drSelect;

                C1WindowExtension.SetParameters(popupTestProd, parameters);


                //popupWipRemarks.Closed += new EventHandler(popupWipRemarks_Closed);

                grdMain.Children.Add(popupTestProd);
                popupTestProd.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void popupTestProd_Closed(object sender, EventArgs e)
        {
            COM001_355_TEST_PROD window = sender as COM001_355_TEST_PROD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetLotList();
            }
        }

        private string GetUserId(string str)
        {
            string sUserId = "";

            idx = str.IndexOf("(");

            sUserId = str.Substring(idx + 1);

            sUserId = sUserId.Replace(")", "");

            return sUserId;
        }
    }
}