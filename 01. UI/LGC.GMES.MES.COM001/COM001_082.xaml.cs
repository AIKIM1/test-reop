/*************************************************************************************
 Created Date : 2017.05.30
      Creator : 
   Decription : 코터 작업조건 데이터 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.23  DEVELOPER : Initial Created.
  2024.09.23  김도형    : [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송

 

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_082 : UserControl, IWorkArea
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

        private string _SLT_LOTID = string.Empty;  // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송

        private BizDataSet _Biz = new BizDataSet();
        Util _Util = new Util();

        public COM001_082()
        {
            InitializeComponent();
            InitCombo();
            SetElec();
        }
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnRoute);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //if (FrameOperation.AUTHORITY.Equals("W"))
            //{
            //    dgLotList.Columns["CHK"].Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
            //}
            //여기까지 사용자 권한별로 버튼 숨기기
        }


        //화면내 combo 셋팅
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);

            //공정
            /*
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);
            cboProcess.SelectedValue = Process.COATING;
            if (cboProcess.Items.Count < 1)
                SetProcess();
            */
            string[] sFilters = new string[] { LoginInfo.LANGID, "EQPT_CLCT_PROC" };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, sFilter: sFilters, sCase: "COMMCODES");


            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);
            cboEquipmentSegment.SelectedIndex = 0;

            //경로흐름
            string[] sFilter = { "FLOWTYPE" };
            _combo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // Top/Back
            String[] sFilter3 = { "COAT_SIDE_TYPE" };
            _combo.SetCombo(cboTopBack, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilters1 = { "CLCTTYPE", "Y" };

            _combo.SetCombo(cboClctType, CommonCombo.ComboStatus.ALL, sFilter: sFilters1, sCase: "COMMCODEATTR");

            cboClctType.SelectedValue = "SV";

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                cboEquipmentSegment.SelectedIndex = 0;
                cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
                //SetProcess();
                cboProcess.SelectedIndex = 0;
                cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
                SetEquipment();
            }
        }

        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                ClearValue();

                GetAreaType(cboProcess.SelectedValue.ToString());
                //AreaCheck(cboProcess.SelectedValue.ToString());
                //cboProcess.SelectedValue = Process.COATING;
            }
        }

        private void AreaCheck(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                cboTopBackTiltle.Visibility = Visibility.Collapsed;
                cboTopBack.Visibility = Visibility.Collapsed;
                cboElecTypeTiltle.Visibility = Visibility.Collapsed;
                cboElecType.Visibility = Visibility.Collapsed;

                dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.COATING) || sProcID.Equals(Process.INS_COATING) || sProcID.Equals(Process.INS_SLIT_COATING))
                {
                    cboTopBackTiltle.Visibility = Visibility.Visible;
                    cboTopBack.Visibility = Visibility.Visible;
                }

                // 길이초과 칼럼은 전극에서 MIXING제외 조립에선 NOTCHING만
                if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                     ||
                    (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                {

                    dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Visible;
                }

                if (_AREATYPE.Equals("E"))
                {
                    cboElecTypeTiltle.Visibility = Visibility.Visible;
                    cboElecType.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        private void ClearValue()
        {
            _AREAID = "";
            _PROCID = "";
            _EQSGID = "";
            _EQPTID = "";
            _LOTID = "";
            _WIPSEQ = "";
            _LANEPTNQTY = "";
            Util.gridClear(dgDetailLotList);

        }

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

        // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
        private string GetEltrSltLotThicknessViewYn(string sProcid)
        {
            string sEltrSltLotThicknessViewYn = string.Empty;

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELTR_SLT_LOT_THICKNESS_VIEW_YN";  // 전극 슬리터 LOT 두께 보기 여부
            sCmCode = sProcid;                             // 공정 

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrSltLotThicknessViewYn = "Y";
                }
                else
                {
                    sEltrSltLotThicknessViewYn = "N";
                }

                return sEltrSltLotThicknessViewYn;
            }
            catch (Exception ex)
            {
                sEltrSltLotThicknessViewYn = "N";
                //Util.MessageException(ex);
                return sEltrSltLotThicknessViewYn;
            }
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        private void GetDetailLot(string sLOTID, string sEqptId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                DataTable dtRslt = new DataTable();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptId;
                dr["LOTID"] = sLOTID;

                dtRqst.Rows.Add(dr);

                var bizName = cboClctType.SelectedValue.ToString().Equals("SV") ? "DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO_V01" : "DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO_ALL";

                //dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO", "RQSTDT", "RSLTDT", dtRqst);
                dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgDetailLotList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDetailPrLot(string sLOTID, string sEqptId)
        {
            try
            {
                string sEltrSltLotThicknessViewYn = GetEltrSltLotThicknessViewYn(_PROCID); // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송

                DataTable dtRqst = null;
                DataTable dtRslt = null;

                if (sEltrSltLotThicknessViewYn.Equals("Y") && _PROCID.Equals(Process.SLITTING)) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                {
                    dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));
                    dtRqst.Columns.Add("CLCTTYPE", typeof(string));

                    // cboClctType.SelectedValue.
                    DataRow dr = dtRqst.NewRow();

                    dtRslt = new DataTable();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                    dr["LOTID"] = _SLT_LOTID;  // 자리수 변환 안된 LOTID (sLotId = sLotId.Substring(0, 9);)
                    dr["CLCTTYPE"] = cboClctType.SelectedValue.ToString();

                    dtRqst.Rows.Add(dr);
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO_COLLECT_SLT_THICK", "RQSTDT", "RSLTDT", dtRqst);
                }
                else
                {
                    dtRqst = new DataTable();
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();

                    dtRslt = new DataTable();

                    dr["EQPTID"] = sEqptId;

                    if (_PROCID == "E2000")
                    {
                        dr["LOTID"] = sLOTID.Substring(0, 7);
                    }
                    else
                    {
                        dr["LOTID"] = sLOTID;
                    }
                    dtRqst.Rows.Add(dr);
                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO_COLLECT", "RQSTDT", "RSLTDT", dtRqst);
                }

                Util.GridSetData(dgDetailLotList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        public void GetLotList()
        {
            try
            {
                Util.gridClear(dgDetailLotList);
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

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                string sEqptID = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEqptID) ? null : sEqptID;

                dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);

                if (cboTopBack.Visibility.Equals(Visibility.Visible))
                    dr["TOPBACK"] = Util.GetCondition(cboTopBack, bAllNull: true);
                if (cboElecType.Visibility.Equals(Visibility.Visible))
                    dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType, bAllNull: true);

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["AREATYPE"] = _AREATYPE;

                if (!string.IsNullOrWhiteSpace(txtLotId.Text))
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

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation);

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

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
                cboProcess.SelectedValue = Process.COATING;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetElec()
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["S01"].ToString().Equals("E"))
                    {
                        cboElecType.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        cboElecType.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void dgDetailLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").IsFalse())
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                string sEqptId = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();
                string sDate = DataTableConverter.GetValue(rb.DataContext, "STARTDTTM").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));

                _SLT_LOTID = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();  // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);

                if (string.Equals(cboProcess.SelectedValue, Process.COATING))
                    sLotId = sLotId.Substring(0, 8);
                else if (string.Equals(cboProcess.SelectedValue, Process.SLITTING))
                    sLotId = sLotId.Substring(0, 9);

                if (cboClctType.SelectedValue.Equals("E"))
                    GetDetailPrLot(sLotId, sEqptId);
                else
                    GetDetailLot(sLotId, sEqptId);
            }
        }

        private void SetValue(object oContext)
        {
            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _LANEPTNQTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_PTN_QTY"));
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void clctType_ItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrWhiteSpace(cboClctType.SelectedValue.ToString()) || !cboClctType.SelectedValue.ToString().Equals(""))
            {
                if (cboClctType.SelectedValue.Equals("E"))
                {
                    dgDetailLotList.Columns["SV_VALUE"].Visibility = Visibility.Collapsed;
                    dgDetailLotList.Columns["GAP_VALUE"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgDetailLotList.Columns["SV_VALUE"].Visibility = Visibility.Visible;
                    dgDetailLotList.Columns["GAP_VALUE"].Visibility = Visibility.Visible;
                }
            }
            else
            {
                dgDetailLotList.Columns["SV_VALUE"].Visibility = Visibility.Visible;
                dgDetailLotList.Columns["GAP_VALUE"].Visibility = Visibility.Visible;
            }
        }
    }
}
