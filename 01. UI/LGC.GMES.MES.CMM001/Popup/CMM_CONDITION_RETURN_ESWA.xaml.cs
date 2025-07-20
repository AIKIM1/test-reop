/*************************************************************************************
 Created Date : 2023.05.23
      Creator : 정재홍
   Decription : 물류 반송 조건 설정(ESWA 자동차2 동) - CMM_CONDITION_RETURN Copy
                자동차 2동 전용 사유 : 'NON_COATED_WINDING_DIRCTN_USE_AREA' 등록시 장비완료(롤프레스/코터 공정 장비완료 검증이 필요)
                                        코드 사용하는 ui 로직 전체 검증 필요로 별도 팝업 실행 되도록 개발
--------------------------------------------------------------------------------------
[Change History]
  2023.05.23  정재홍 : 최초생성 [E20230210-000354]
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    public partial class CMM_CONDITION_RETURN_ESWA : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _equipmentCode = string.Empty; 
        private string _equipmentName = string.Empty;
        private string _Pjt = string.Empty;
        private string _Side = string.Empty;
        private string _Version = string.Empty;
        private string _Lottyple = string.Empty;
        private string _process = string.Empty;
        private string _areachk = string.Empty;  //전극 및 조립 화면에서 호출 구분  : 전극 :E , 조립 : A
        private string _woid = string.Empty;


        private string _Non_coated_winding_dirctn_use_area = string.Empty; //무지부/권취방향 모두 사용하는 AREA 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public CMM_CONDITION_RETURN_ESWA()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 화면로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetControl();
            this.Loaded -= C1Window_Loaded;
        }

        /// <summary>
        ///  컨트롤 셋팅
        /// </summary>
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _equipmentCode = tmps[0] as string;
            _equipmentName = tmps[1] as string;
            _Pjt = tmps[2] as string;
            _Version = tmps[3] as string;
            _process = tmps[4] as string;
            _areachk = tmps[5] as string;
            _woid = tmps[6] as string;
            GetEioAttr(_equipmentCode);
            Non_coated_winding_dirctn_use_area_Chk(); // 무지부/권취 모두 사용하는동
            
            EnableComboChk(_process);

            InitCombo();
            txtEquipment.Text = _equipmentName;
            txtPjt.Text = _Pjt;

        }
        #endregion

        #region Event

        #region 설정 버튼 클릭  : btnSelect_Click()

        /// <summary>
        /// 설정 버튼 클릭
        /// </summary>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                //2023.02.09 hjyeon - LotType, Ver, Side Enable / Visible 시 Send 하도록 수정
                if (Util.Equals(cboLotType.Visibility, Visibility.Visible)
                    || Util.Equals(cboSide.Visibility, Visibility.Visible)
                    || Util.Equals(cboVersion.Visibility, Visibility.Visible))
                {
                    const string bizRuleName = "BR_PRD_REG_EQPT_LOGIS_COND";

                    string sHSSCode = string.Empty;  //무지부방향
                    string sWDCode = string.Empty;  //권취방향

                    if (cboSide.SelectedValue != null)
                    {
                        sHSSCode = cboSide.SelectedValue.ToString().Substring(0, 1);
                        sWDCode = cboSide.SelectedValue.ToString().Substring(2, 1);
                    }

                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                    inDataTable.Columns.Add("WRK_HALF_SLIT_SIDE", typeof(string));
                    inDataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                    inDataTable.Columns.Add("LOTTYPE", typeof(string));
                    inDataTable.Columns.Add("UPDUSER", typeof(string));
                    inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["EQPTID"] = _equipmentCode;
                    dr["WO_DETL_ID"] = _woid;
                    dr["WRK_HALF_SLIT_SIDE"] = sHSSCode;
                    dr["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
                    dr["LOTTYPE"] = cboLotType.SelectedValue == null ? string.Empty : cboLotType.SelectedValue.ToString();
                    dr["UPDUSER"] = LoginInfo.USERID;
                    dr["PROD_VER_CODE"] = cboVersion.SelectedValue == null ? string.Empty : cboVersion.SelectedValue.ToString();
                    inDataTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (Util.Equals(mcbLane.Visibility, Visibility.Visible))
                        {
                            //Do Nothing
                            //Lane 까지 저장되고 난 후에 "저장되었습니다" 나오고 창 닫히게..
                        } else
                        {
                            Util.MessageInfo("SFU1270");      //저장되었습니다.
                            DialogResult = MessageBoxResult.OK;
                        }
                        
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            //-----------------
            //2023.02.09 hjyeon - Lane Visible 시 Send 하도록 수정
            try
            {
                if (Util.Equals(mcbLane.Visibility, Visibility.Visible))
                {
                    DataSet dsInput = new DataSet();

                    const string bizRuleName = "BR_PRD_REG_EQPT_LOGIS_COND_LANE";

                    DataTable inDataTable =  dsInput.Tables.Add("INDATA");
                    DataTable inLotTable = dsInput.Tables.Add("INLOT");

                    //INPUT TABLE - LANE
                    inLotTable.Columns.Add("LANE_ID", typeof(string));
                    inLotTable.Columns.Add("USE_FLAG", typeof(string));

                    int iMaxLane = 0;
                    foreach (MultiSelectionBoxItem cbo in mcbLane.MultiSelectionBoxSource)
                    {
                        if (cbo.IsAll)
                        {
                            continue;
                        }

                        iMaxLane++;
                        
                        DataRow drLane = inLotTable.NewRow();
                        drLane["LANE_ID"] = (cbo.Item as DataRowView)["CBO_CODE"];

                        if (cbo.IsChecked)
                        {
                            drLane["USE_FLAG"] = "Y";
                        }
                        else
                        {
                            drLane["USE_FLAG"] = "N";
                        }

                        inLotTable.Rows.Add(drLane);
                    }

                    //INPUT TABLE - EQP
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("MAX_LANE_ID", typeof(string));
                    inDataTable.Columns.Add("UPDUSER", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["EQPTID"] = _equipmentCode;
                    dr["MAX_LANE_ID"] = Util.NVC(iMaxLane);
                    dr["UPDUSER"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(dr);

                    new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT", null,  dsInput);

                    Util.MessageInfo("SFU1270");      //저장되었습니다.
                    DialogResult = MessageBoxResult.OK;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //-----------------
        }

        #endregion

        #region 닫기 버튼 클릭 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion

        #region Method

        #region 해당설비의 무지부/권취 정보 및 LOTTYPE 확인  : GetEioAttr()
        /// <summary>
        ///  해당 설비의 무지부/권취 정보 및 LOTTYPE 확인
        /// </summary>
        /// <param name="sEqptID"></param>
        public void GetEioAttr(string sEqptID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR", "RQSTDT", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    _Side = Util.NVC(result.Rows[0]["WRK_HALF_SLIT_SIDE"]) + "/" + Util.NVC(result.Rows[0]["EM_SECTION_ROLL_DIRCTN"]);
                    _Lottyple = Util.NVC(result.Rows[0]["LOTTYPE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        #endregion
        
        #region  무지부, 버전, LOT 유형 Enable 체크 여부 : EnableComboChk()
        /// <summary>
        /// 무지부, 버전, LOT 유형 Enable 체크 여부 
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        public void EnableComboChk(string Procid)
        {

            if (string.IsNullOrEmpty(Procid)) return;

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "LOGIS_COND_BY_PROC";
                dr["COM_CODE"] = Procid;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if(Util.NVC(dtResult.Rows[0]["ATTR1"]) == "Y")
                    {
                        //cboSide.IsEnabled = true;
                        SideName.Visibility = Visibility.Visible;
                        cboSide.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //cboSide.IsEnabled = false;
                        SideName.Visibility = Visibility.Collapsed;
                        cboSide.Visibility = Visibility.Collapsed;

                    }

                    if (Util.NVC(dtResult.Rows[0]["ATTR2"]) == "Y")
                    {
                        txtVersion.Visibility = Visibility.Visible;
                        //cboVersion.IsEnabled = true;
                        cboVersion.Visibility = Visibility.Visible; ;

                    }
                    else
                    {
                        //cboVersion.IsEnabled = false;
                        txtVersion.Visibility = Visibility.Collapsed;
                        cboVersion.Visibility = Visibility.Collapsed; ;
                    }

                    if (Util.NVC(dtResult.Rows[0]["ATTR3"]) == "Y")
                    {
                        txtLotTyp.Visibility = Visibility.Visible;
                        //cboLotType.IsEnabled = true;
                        cboLotType.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //cboLotType.IsEnabled = false;
                        txtLotTyp.Visibility = Visibility.Collapsed;
                        cboLotType.Visibility = Visibility.Collapsed;
                    }

                    //2023.02.07 hjyeon Lane 추가
                    if (Util.NVC(dtResult.Rows[0]["ATTR4"]) == "Y")
                    {
                        txtLane.Visibility = Visibility.Visible;
                        mcbLane.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        txtLane.Visibility = Visibility.Collapsed;
                        mcbLane.Visibility = Visibility.Collapsed;
                    }

                }
          
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
             }
        }
        #endregion

        #region 무지부/권취방향 모두 사용하는 AREA 확인 : Non_coated_winding_dirctn_use_area_Chk()
        /// <summary>
        /// 무지부/권취방향 모두 사용하는 AREA 확인
        /// </summary>
        private void Non_coated_winding_dirctn_use_area_Chk()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));
                inTable.Columns.Add("CMCDIUSE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "UNCOATED_UNWINDING_DIRECTION";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDIUSE"] = "Y";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _Non_coated_winding_dirctn_use_area = "3";
                    SideName.Text = ObjectDic.Instance.GetObjectName("무지부/권취방향");
                }
                else
                {
                    SideName.Text = ObjectDic.Instance.GetObjectName("무지부");
                }

            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 콤보박스 셋팅  : InitCombo()
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            //2023.02.07 hjyeon Visible 일 때만 Setting 하도록 함
            if (Util.Equals(cboSide.Visibility, Visibility.Visible))
            {
                SetSide(cboSide); // 무지부
            }

            //2023.02.07 hjyeon Visible 일 때만 Setting 하도록 함
            if (Util.Equals(cboVersion.Visibility, Visibility.Visible))
            {
                SetVersion(cboVersion); //버전
            }

            //2023.02.07 hjyeon Visible 일 때만 Setting 하도록 함
            if (Util.Equals(cboLotType.Visibility, Visibility.Visible))
            {
                SetLottype(cboLotType); // LOTTYPE
            }

            //2023.02.07 hjyeon
            if (Util.Equals(mcbLane.Visibility, Visibility.Visible))
            {
                SetLane();  //LANE
            }
        }

        #endregion

        #region 무지부 콤보박스 조회 : SetSide()

        /// <summary>
        /// 무지부 콤보박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetSide(C1ComboBox cbo)
        {
            
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "WRK_HALF_SLIT_SIDE", _Non_coated_winding_dirctn_use_area };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo("DA_BAS_SEL_COMMCODE_ATTR_CBO", cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);

            //기존데이터 콤보박스셋팅
            if (_Side != string.Empty)
            {
                cboSide.SelectedValue = _Side;
            }

        }

        #endregion

        #region 버전 콤보박스 조회 : SetVersion()
        /// <summary>
        /// 버전 콤보박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetVersion(C1ComboBox cbo)
        {
            //조립에서 호출할 경우
            if (_areachk == "A")
            {
                const string bizRuleName = "DA_BAS_SEL_TRNS_CONDITION_VERSION_CBO_ASSY";
                string[] arrColumn = { "EQPTID" };
                string[] arrCondition = { _equipmentCode };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            }
            else
            {
                const string bizRuleName = "DA_BAS_SEL_TRNS_CONDITION_VERSION_CBO_ELEC";
                string[] arrColumn = { "EQPTID" };
                string[] arrCondition = { _equipmentCode };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            }

            if (_Version != string.Empty)
            {
                cboVersion.SelectedValue = _Version;
            }
        }

        #endregion

        #region LOTTYPE 콤보박스 조회 : SetLottype()
        /// <summary>
        /// LOTTYPE 콤보박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetLottype(C1ComboBox cbo)
        {
            //조립에서 호출할 경우
            if (_areachk == "A")
            {
                const string bizRuleName = "DA_BAS_SEL_TRNS_CONDITION_LOTTYPE_CBO_ASSY";
                string[] arrColumn = { "LANGID", "EQPTID" };
                string[] arrCondition = { LoginInfo.LANGID, _equipmentCode };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            }
            else
            {
                const string bizRuleName = "DA_BAS_SEL_TRNS_CONDITION_LOTTYPE_CBO_ELEC";
                string[] arrColumn = { "LANGID", "EQPTID" };
                string[] arrCondition = { LoginInfo.LANGID, _equipmentCode };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            }

            if (_Lottyple != string.Empty)
            {
                cboLotType.SelectedValue = _Lottyple;
            }


        }


        #endregion

        #region LANE 콤보박스 조회 : SetLane()
        /// <summary>
        /// LANE 콤보박스
        /// </summary>
        private void SetLane()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TRNS_CONDITION_LANE_CBO_ASSY", "INDATA", "OUTDATA", inTable);
                if (CommonVerify.HasTableRow(dtResult))
                {
                    mcbLane.ItemsSource = DataTableConverter.Convert(dtResult);

                    //기존 장비에 설정된 LANE 이 있으면 CHECKED 상태로 표시
                    for (int row = 0; row < dtResult.Rows.Count; row++)
                    {
                        foreach (MultiSelectionBoxItem cbo in mcbLane.MultiSelectionBoxSource)
                        {
                            if (cbo.IsAll)
                            {
                                continue;
                            }

                            if (String.Equals((cbo.Item as DataRowView)["CBO_CODE"], dtResult.Rows[row]["CBO_CODE"])
                                && Util.NVC(dtResult.Rows[row]["USE_FLAG"]) == "Y")
                            {
                                cbo.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            
        }

        #endregion

        #endregion


    }
}
