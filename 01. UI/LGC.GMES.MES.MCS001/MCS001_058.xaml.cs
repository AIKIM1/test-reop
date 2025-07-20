/*************************************************************************************
 Created Date : 2021.04.01
      Creator : 조영대
   Decription : 설비 포트별 반송가능(버퍼) 수량 관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.01  조영대 : Initial Created. 
  2022.09.14  오화백 : DA_MCS_SEL_MCS_CONFIG_CONN ==> DA_MHS_SEL_CONFIG_CONN
  2023.07.31  임시혁 : E20230731-000133. 최대설정가능반송수량추가및 양극/음극 버퍼수량 분리 입력
  2024.02.27  안유수 : E20231215-001596 설비별 이송 명령 현황 확인 팝업 화면 추가
  2024.08.17  최평부 : ESST VD590 SI PROJECT > 극성 컬럼 숨김처리(공통코드 : DIFFUSION_SITE 로 분기처리)
  2025.03.24  이민형 : BR_MHS_UPD_CURR_PORT_MAX_TRF_QTY 호출시 시스템 구분[SYSTEM_TYPE_CODE] 아이템을 포함, 활성화와 조립은 한 BR로 처리
  2025.04.28  이민형 : Intelligence Buffer사용을 위한 컬럼 추가
  2025.04.30  이민형 : 조회 조건 인자값 추가 (db_conn, systemid)
  2025.07.07  김병보 : dtResult2 null 일 경우 방어로직 추가.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.ControlsLibrary;  // E20230731-000133 추가 임시혁
using System.Windows.Media;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_058 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        class BizRule
        {
            //public string _bizRuleIp;
            //public string _bizRuleProtocol;
            //public string _bizRulePort;
            //public string _bizRuleServiceMode;
            //public string _bizRuleServiceIndex;
        }

        Dictionary<string, BizRule> connectDb = null;

        bool _DiffusionSiteFlag = false;
        int nInitialQTY = 0;      
    
        public MCS001_058()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            ApplyPermissions();

            InitializeControls();
            InitializeCombo();            
        }

        private void InitializeControls()
        {
        }

        private void InitializeCombo()
        {
            //2024-08-17 by 최평부
            //diffusion_site 공통코드 조회(화면 : '극성' 컬럼 분기처리)
            DataTable dtDiffusionSite = new DataTable();
            dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "AUTO_LOGIS");

            string shop_id = string.Empty;

            if (dtDiffusionSite.Rows.Count > 0)
            {
                shop_id = dtDiffusionSite.Rows[0]["ATTRIBUTE1"].ToString();

                if (shop_id.Contains(LoginInfo.CFG_SHOP_ID))
                {
                    _DiffusionSiteFlag = true;
                }
            }

            if (_DiffusionSiteFlag)
            {
                dgMaxTrfQtyList.Columns["POLARITY_NAME"].Visibility = Visibility.Collapsed;
            }
            //2024-08-17 by 최평부 END

            CommonCombo comboSet = new CommonCombo(); 
            String[] sFilter = { string.Empty };

            //동
            comboSet.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // 시스템 구분
            cboSystemType.SetCommonCode("MES_SYSTEM_TYPE_CODE", CommonCombo.ComboStatus.SELECT, true);
            cboSystemType.SelectedValue = LoginInfo.CFG_SYSTEM_TYPE_CODE;

            //설비군
            string[] arrColumn1 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition1 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue() };
            cboEqpType.SetDataComboItem("DA_MHS_SEL_EQUIPMENTGROUP_PORT_CBO", arrColumn1, arrCondition1, CommonCombo.ComboStatus.SELECT);

            //라인
            string[] arrColumn0 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE", "EQGRID" };
            string[] arrCondition0 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue(), cboEqpType.GetStringValue() };
            cboLine.SetDataComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_SYSTYPE_CBO", arrColumn0, arrCondition0, CommonCombo.ComboStatus.ALL);

            //설비
            string[] arrColumn2 = { "LANGID", "SHOPID", "AREAID", "EQGRID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(),
                cboEqpType.GetStringValue().Equals(string.Empty) ? null : cboEqpType.GetStringValue(),
                cboSystemType.GetStringValue().Equals(string.Empty) ? null : cboSystemType.GetStringValue() };
            cboEqp.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_PORT_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, true);

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            Loaded -= UserControl_Loaded;
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();            
        }
        
        private void cboSystemType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            //설비군
            string[] arrColumn1 = { "LANGID", "AREAID", "SYSTEM_TYPE" };
            string[] arrCondition1 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue() };
            cboEqpType.SetDataComboItem("DA_MHS_SEL_EQUIPMENTGROUP_PORT_CBO", arrColumn1, arrCondition1, CommonCombo.ComboStatus.SELECT);
        }
        
        private void cboEqpType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            //라인
            string[] arrColumn0 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE", "EQGRID" };
            string[] arrCondition0 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue(), cboEqpType.GetStringValue() };
            cboLine.SetDataComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_SYSTYPE_CBO", arrColumn0, arrCondition0, CommonCombo.ComboStatus.ALL);

            //설비
            string[] arrColumn2 = { "LANGID", "SHOPID", "AREAID", "EQGRID", "EQSGID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(), cboEqpType.GetStringValue(),
                cboLine.GetStringValue().Equals(string.Empty) ? null : cboLine.GetStringValue(),
                cboSystemType.GetStringValue().Equals(string.Empty) ? null : cboSystemType.GetStringValue() };
            cboEqp.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_PORT_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, true);
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            //설비
            string[] arrColumn2 = { "LANGID", "SHOPID", "AREAID", "EQGRID", "EQSGID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(), cboEqpType.GetStringValue(),
                cboLine.GetStringValue().Equals(string.Empty) ? null : cboLine.GetStringValue(),
                cboSystemType.GetStringValue().Equals(string.Empty) ? null : cboSystemType.GetStringValue() };
            cboEqp.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_PORT_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, true);
        }
        
        private void cboEqp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();
        }

        private void cboPortDirection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {          
            try
            {
                if (!dgMaxTrfQtyList.IsCheckedRow("CHK"))
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMaxTrfQtyList_CheckedChanged(object sender, RoutedEventArgs e)
        {
            dgMaxTrfQtyList.SelectedIndex = ((DataGridCellPresenter)((sender as Control).Parent)).Row.Index;
        }

        private void dgMaxTrfQtyList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            string strPortId = string.Empty;
            int nSamePortOtherPolarityMaxTrfQty = 0;

            if (e.Cell.Value != null)
            {
                // E20230731 - 000133 Start. 극성이 있는 경우 동일 Port ID 다른 극성에 대해 자동 체크처리
                int maxTrfQty = Util.NVC_Int(dgMaxTrfQtyList.GetValue(e.Cell.Row.Index, "MAX_TRF_QTY"));                

                if (!Convert.ToInt32(e.Cell.Value).Equals(maxTrfQty))
                {
                    dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 1);

                    //동일 PORT에 CHK 안되어있는것이 있으면 찾아서 체크처리한다.
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "POLARITY"))))
                    {
                        strPortId = Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "PORT_ID"));

                        for (int idx = 0; idx < dgMaxTrfQtyList.Rows.Count; idx++)
                        {
                            // 동일한 Port이면서 CHK안된것이 있는지 찾는다.
                            if (strPortId.Equals(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "PORT_ID"))))
                            {
                                if (!Convert.ToBoolean(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "CHK")))
                                {
                                    dgMaxTrfQtyList.SetValue(idx, "CHK", 1);
                                }
                            }
                        }
                    }                  
                }
                else
                {
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "POLARITY"))))
                    {
                        // 동일 Port에 다른 Row에 Max수량와 다른것이 있는지 찾는다.
                        strPortId = Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "PORT_ID"));

                        for (int idx = 0; idx < dgMaxTrfQtyList.Rows.Count; idx++)
                        {
                            // 동일한 Port이면서 CHK안된것이 있는지 찾는다.
                            if (strPortId.Equals(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "PORT_ID"))))
                            {
                                if (!idx.Equals(e.Cell.Row.Index))
                                {
                                    // 이 Row의 Max수량과 변경수량이 동일하면?
                                    nSamePortOtherPolarityMaxTrfQty = Util.NVC_Int(dgMaxTrfQtyList.GetValue(idx, "MAX_TRF_QTY"));

                                    if (!Util.NVC_Int(dgMaxTrfQtyList.GetValue(idx, e.Cell.Column.Name.ToString())).Equals(nSamePortOtherPolarityMaxTrfQty))
                                    {
                                        dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 1);  // 변경한 Row의 CHK를 True로
                                        dgMaxTrfQtyList.SetValue(idx, "CHK", 1);               // 동일 Port 다른 극성 Row에 대해 CHK를 True로.
                                    }
                                    else
                                    {
                                        dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 0);  // 변경한 Row의 CHK를 False로
                                        dgMaxTrfQtyList.SetValue(idx, "CHK", 0);               // 동일 Port 다른 극성 Row에 대해 CHK를 False로.
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 0);
                    }
                }

                if(e.Cell.Column.Name == "CHG_TRF_QTY")
                {
                    if (dgMaxTrfQtyList.Columns.Contains("USE_INT_BUF_FLAG")
                        && !string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "USE_INT_BUF_FLAG")))
                        && DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "USE_INT_BUF_FLAG").Equals("Y"))
                    {
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "CHG_TRF_QTY", nInitialQTY);                        
                    }
                } 
                // E20230731 - 000133 End
            }

        }

        private void dgMaxTrfQtyList_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!dgMaxTrfQtyList.IsKeyboardFocusWithin)
            {
                int currentRow = dgMaxTrfQtyList.SelectedIndex;
                dgMaxTrfQtyList.EndEdit();

                dgMaxTrfQtyList.CurrentRow = null;
                dgMaxTrfQtyList.SelectRow(currentRow);
            }
        }

        /// <summary>
        /// // E20230731 - 000133 Start. 데이타 조회/출력 후 PORT ID 기준으로 CELL 병합
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMaxTrfQtyList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {

            try
            {
                if(dgMaxTrfQtyList.Rows.Count > 0)
                {
                    
                    for(int idx = 0; idx < dgMaxTrfQtyList.Rows.Count;)
                    {
                        string strPortId = Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "PORT_ID"));

                        int nRowMergeCnt = 0;

                        for(int jdx = idx;jdx < dgMaxTrfQtyList.Rows.Count; jdx++)
                        {
                            if( strPortId.Equals(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[jdx].DataItem, "PORT_ID"))))
                            {
                                nRowMergeCnt++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["CONN_DB_NAME"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["CONN_DB_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["EQSGNAME"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["EQSGNAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["EQGRNAME"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["EQGRNAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["EQPTNAME"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["EQPTNAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["INOUT_TYPE_NAME"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["INOUT_TYPE_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["PORTNAME"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["PORTNAME"].Index)));
                        e.Merge(new DataGridCellsRange(dgMaxTrfQtyList.GetCell(idx, dgMaxTrfQtyList.Columns["MAX_SET_ENABLE_TRF_QTY"].Index), dgMaxTrfQtyList.GetCell(idx + nRowMergeCnt - 1, dgMaxTrfQtyList.Columns["MAX_SET_ENABLE_TRF_QTY"].Index)));

                        idx = idx + nRowMergeCnt;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnSave
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void GetBizActorServerInfo()
        {
            if (connectDb == null)
            {
                connectDb = new Dictionary<string, MCS001.MCS001_058.BizRule>();
            }
            connectDb.Clear();

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = cboArea.GetBindValue();
            dr["SYSTEM_TYPE_CODE"] = cboSystemType.GetBindValue();
            dr["EQGRID"] = cboEqpType.GetBindValue();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_CONFIG_CONN", "RQSTDT", "RSLTDT", inTable);
           
            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    string systemType = Util.NVC(newRow["CONN_DB"]);
                    BizRule bizRule;
                    if (!connectDb.ContainsKey(systemType))
                    {
                        bizRule = new BizRule();
                        connectDb.Add(systemType, bizRule);
                    }
                    else
                    {
                        bizRule = connectDb[systemType];
                    }

                    //if (newRow["KEYID"].ToString() == "BizActorIP")
                    //    bizRule._bizRuleIp = newRow["KEYVALUE"].ToString();
                    //else if (newRow["KEYID"].ToString() == "BizActorPort")
                    //    bizRule._bizRulePort = newRow["KEYVALUE"].ToString();
                    //else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                    //    bizRule._bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    //else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                    //    bizRule._bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    //else
                    //    bizRule._bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

            if (connectDb == null || connectDb.Count == 0)
            {
                dgMaxTrfQtyList.Columns["CONN_DB_NAME"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgMaxTrfQtyList.Columns["CONN_DB_NAME"].Visibility = Visibility.Visible;
            }
        }

        private void ClearDataGrid()
        {
            dgMaxTrfQtyList.ClearRows();       
        }
        
        private void Refresh()
        {
            try
            {
                if (!ChkValidation()) return;

                SelectMaxTrfQtyList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private bool ChkValidation()
        {
            if (cboArea.SelectedValue==null || cboArea.SelectedValue.Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU4925", lblArea.Text);
                cboArea.Focus();
                return false;
            }

            if (cboSystemType.SelectedValue == null || cboSystemType.SelectedValue.Equals("SELECT"))
            {
                // 시스템 구분을 선택하세요.
                Util.MessageValidation("SFU4925", lblSystemType.Text);
                cboSystemType.Focus();
                return false;
            }

            if (cboEqpType.SelectedValue == null || cboEqpType.SelectedValue.Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU4925", lblEqpType.Text);
                cboEqpType.Focus();
                return false;
            }

            return true;
        }

        private void SelectMaxTrfQtyList()
        {
            try
            {
                ShowLoadingIndicator();

                dgMaxTrfQtyList.ClearRows();

                GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리

                DataTable INDATA = new DataTable("RQSTDT");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("EQGRID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("INOUT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("CONN_DB", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.GetBindValue();
                inData["SYSTEM_TYPE_CODE"] = cboSystemType.GetBindValue();
                inData["EQSGID"] = cboLine.GetBindValue();
                inData["EQGRID"] = cboEqpType.GetBindValue();
                inData["EQPTID"] = cboEqp.GetBindValue();
                inData["CONN_DB"] = cboSystemType.GetBindValue();
                INDATA.Rows.Add(inData);

                DataTable dtResult1 = null, dtResult2 = null;

                dtResult1 = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_TRF_QTY_LIST", "RQSTDT", "RSLTDT", INDATA);
                if (connectDb != null && connectDb.Count > 0)
                {
                    foreach (KeyValuePair<string, BizRule> bizInfo in connectDb)
                    {
                        if (bizInfo.Key.Equals("F"))
                        {
                            INDATA.Rows[0]["CONN_DB"] = bizInfo.Key;

                            //dtResult2 = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_TRF_QTY_LIST_FORM_CNB_2", "RQSTDT", "RSLTDT", INDATA);
                            dtResult2 = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_TRF_QTY_LIST_FORM", "RQSTDT", "RSLTDT", INDATA);
                            if (CommonVerify.HasTableRow(dtResult2))
                            {
                                if(dtResult1.Rows.Count > 0
                                    && dtResult2.Rows.Count > 0)
                                {
                                    dtResult1.Merge(dtResult2);
                                }                                
                            }
                        }
                    }
                }

                if(dtResult1.Rows.Count > 0)
                {
                    dgMaxTrfQtyList.SetItemsSource(dtResult1, FrameOperation, true);
                }
                else if(dtResult2 != null && dtResult2.Rows.Count > 0)
                {
                    dgMaxTrfQtyList.SetItemsSource(dtResult2, FrameOperation, true);
                }
                else
                {
                    dgMaxTrfQtyList.SetItemsSource(dtResult1, FrameOperation, true);
                }

                // E20230731 - 000133 Start
                // 극성이 나뉘는 Port의 경우 BUF Qty를 MAX_TRF_QTY, CHG_TRF_QTY 칼럼에 설정한다.
                // 극성이 없는 경우는 MAX_TRF_QTY를 CHG_TRF_QTY 칼럼에 설정한다.
                SetPortPolarityBufferQty();

                // 데이타 조회/출력 후 PORT명, 최대설정가능수량 칼럼 Row 머지
                dgMaxTrfQtyList.MergingCells -= dgMaxTrfQtyList_MergingCells;
                dgMaxTrfQtyList.MergingCells += dgMaxTrfQtyList_MergingCells;
                // E20230731 - 000133 End

                HiddenLoadingIndicator();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void Save()
        {
            try
            {

                // E20230731 - 000133 Start
                //-----------------------------------------
                // CHK처리및 Validation
                //-----------------------------------------
                // dgMaxTrfQtyList_CommittedEdit()에서 수량변경시 체크처리되지만 저장버튼 누르기 전에 사람이 혹시라도 체크해제하는 경우가 발생할수있어 극성을 가지는 것 중 하나는 체크되어있는데 하나는 체크안되어있으면 두개 다 체크처리되게 한다.
                Dictionary<string, string> dicPolarityPortId = new Dictionary<string, string>();

                foreach (DataRow drSelect in dgMaxTrfQtyList.GetCheckedDataRow("CHK"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(drSelect["POLARITY"])))
                    {
                        if (!dicPolarityPortId.ContainsKey(Util.NVC(drSelect["PORT_ID"])))
                        {
                            dicPolarityPortId.Add(Util.NVC(drSelect["PORT_ID"]), Util.NVC(drSelect["PORT_ID"]));
                        }
                    }
                }

                foreach (KeyValuePair<string, string> kvp in dicPolarityPortId)
                {
                    for (int idx = 0; idx < dgMaxTrfQtyList.Rows.Count; idx++)
                    {
                        // 해당 Port ID에 대해 체크 안되어있으면 체크처리한다.
                        if (Util.NVC(kvp.Value).Equals(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "PORT_ID"))))
                        {
                            if (!Convert.ToBoolean(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "CHK")))
                            {
                                dgMaxTrfQtyList.SetValue(idx, "CHK", 1);
                            }
                        }
                    }
                }

                // Port가 극성을 가지는 경우 Buffer별 수량변경시 Max수량을 Buffer별 수량의 합으로 한다.
                Dictionary<string, int> dicPortMaxTrfQty = new Dictionary<string, int>();

                string strPortId = string.Empty;
                int nTransferQty = 0;

                foreach (DataRow drSelect in dgMaxTrfQtyList.GetCheckedDataRow("CHK"))
                {
                    strPortId = drSelect["PORT_ID"].ToString();

                    nTransferQty = Util.NVC_Int(drSelect["CHG_TRF_QTY"]);

                    if (dicPortMaxTrfQty.ContainsKey(strPortId))
                    {
                        dicPortMaxTrfQty[strPortId] += nTransferQty;
                    }
                    else
                    {
                        dicPortMaxTrfQty.Add(strPortId, nTransferQty);
                    }
                }

                // Port에 수량 입력한값이 최대 설정 가능 반송 수량을 초과하는지 체크. MAX_SET_ENABLE_TRF_QTY 값이 없는 경우는 수량 체크하지 않음.
                foreach (DataRow drSelect in dgMaxTrfQtyList.GetCheckedDataRow("CHK"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(drSelect["MAX_SET_ENABLE_TRF_QTY"])))
                    {
                        strPortId = drSelect["PORT_ID"].ToString();

                        if (dicPortMaxTrfQty.ContainsKey(strPortId))
                        {
                            if (dicPortMaxTrfQty[strPortId] > Util.NVC_Int(drSelect["MAX_SET_ENABLE_TRF_QTY"]))
                            {
                                object[] errMsgParam = new object[2];
                                errMsgParam[0] = Util.NVC(drSelect["MAX_SET_ENABLE_TRF_QTY"]);
                                errMsgParam[1] = strPortId.ToString();
                                Util.MessageValidation("100000210", errMsgParam);  //최대 설정 가능 반송 수량(%1)보다 변경값이 큽니다. \\nPORT ID[%2]
                                return;
                            }
                        }
                    }
                }

                //-----------------------------------------
                // 데이타 저장
                //-----------------------------------------
                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("PORT_ID", typeof(string));
                INDATA.Columns.Add("UPDUSER", typeof(string));
                INDATA.Columns.Add("EQUIPMENT_ID", typeof(string));   // 2024.11.01. 김영국 - EQUIPMENT_ID 추가 (김기융 사원 요청)
                INDATA.Columns.Add("MAX_TRF_QTY", typeof(string));
                INDATA.Columns.Add("POLARITY", typeof(string));  // 임시혁
                INDATA.Columns.Add("BUF_QTY", typeof(string));   // 임시혁
                INDATA.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));  // 2025.03.25 박병성 책임 요청으로 추가
                INDATA.Columns.Add("STOP_BAS_SEC", typeof(string));      // 2025.04.29 박병성 책임 요청으로 추가
                INDATA.Columns.Add("RUN_BAS_SEC", typeof(string));       // 2025.04.29 박병성 책임 요청으로 추가
                                
                foreach (DataRow drSelect in dgMaxTrfQtyList.GetCheckedDataRow("CHK"))
                {
                    //if (!drSelect["CONN_DB"].Equals(cboSystemType.GetBindValue())) continue;

                    DataRow newRow = INDATA.NewRow();
                    newRow["PORT_ID"] = drSelect["PORT_ID"];
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    newRow["EQUIPMENT_ID"] = drSelect["EQPTID"]; // 2024.11.01. 김영국 - EQUIPMENT_ID 추가 (김기융 사원 요청)

                    if (string.IsNullOrEmpty(Util.NVC(drSelect["POLARITY"])))
                    {
                        newRow["MAX_TRF_QTY"] = drSelect["CHG_TRF_QTY"];
                        newRow["POLARITY"] = "";
                        newRow["BUF_QTY"] = "";
                    }
                    else
                    {
                        newRow["MAX_TRF_QTY"] = dicPortMaxTrfQty[Util.NVC(drSelect["PORT_ID"])];
                        newRow["POLARITY"] = drSelect["POLARITY"];
                        newRow["BUF_QTY"] = drSelect["CHG_TRF_QTY"];
                    }
                    newRow["SYSTEM_TYPE_CODE"] = drSelect["CONN_DB"];
                    newRow["STOP_BAS_SEC"] = drSelect["STOP_BAS_SEC"];
                    newRow["RUN_BAS_SEC"] = drSelect["RUN_BAS_SEC"];

                    INDATA.Rows.Add(newRow);
                }

                if (INDATA.Rows.Count > 0)
                {
                    //new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_CURR_PORT_MAX_TRF_QTY", "INDATA", null, INDATA);
                    new ClientProxy().ExecuteServiceSync("BR_MHS_UPD_CURR_PORT_MAX_TRF_QTY", "INDATA", "OUTDATA", INDATA); // 2024.11.01. 김영국 - 기존 호출되던 DA에서 BR로 변경.
                } 

                //if (connectDb != null && connectDb.Count > 0)
                //{
                //    foreach (KeyValuePair<string, BizRule> bizInfo in connectDb)
                //    {
                //        if (bizInfo.Key.Equals("F"))
                //        {
                //            INDATA.Rows.Clear();
                //            foreach (DataRow drSelect in dgMaxTrfQtyList.GetCheckedDataRow("CHK"))
                //            {
                //                if (!drSelect["CONN_DB"].Equals(bizInfo.Key)) continue;

                //                DataRow newRow = INDATA.NewRow();
                //                newRow["PORT_ID"] = drSelect["PORT_ID"];
                //                newRow["UPDUSER"] = LoginInfo.USERID;

                //                if (string.IsNullOrEmpty(Util.NVC(drSelect["POLARITY"])))
                //                {
                //                    newRow["MAX_TRF_QTY"] = drSelect["CHG_TRF_QTY"];
                //                    newRow["POLARITY"] = "";
                //                    newRow["BUF_QTY"] = "";
                //                }
                //                else
                //                {
                //                    newRow["MAX_TRF_QTY"] = dicPortMaxTrfQty[Util.NVC(drSelect["PORT_ID"])];
                //                    newRow["POLARITY"] = drSelect["POLARITY"];
                //                    newRow["BUF_QTY"] = drSelect["CHG_TRF_QTY"];
                //                }

                //                INDATA.Rows.Add(newRow);
                //            }

                //            //new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_CURR_PORT_MAX_TRF_QTY_FORM_CNB_2", "INDATA", null, INDATA);
                //            if (INDATA.Rows.Count != 0)
                //            {
                //                new ClientProxy().ExecuteServiceSync("DA_MHS_UPD_CURR_PORT_MAX_TRF_QTY_FORM", "INDATA", null, INDATA);
                //            }
                //        }
                //    }
                //}
                // E20230731 - 000133 End

                Refresh();
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// E20230731 - 000133. 극성이 없는 경우는 최대반송수량을 설정값 칼럼에 출력하고 극성이 있는 경우 극성별 버퍼값을 설정값 칼럼에 출력
        /// </summary>
        private void SetPortPolarityBufferQty()
        {
            if (dgMaxTrfQtyList.Rows.Count > 0)
            {
                for (int i = 0; i < dgMaxTrfQtyList.Rows.Count; i++)
                {
                    // 극성이 없을때
                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[i].DataItem, "POLARITY"))))
                    {
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[i].DataItem, "POLARITY_NAME", "-");
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[i].DataItem, "CHG_TRF_QTY", DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[i].DataItem, "MAX_TRF_QTY"));
                    }
                    // 극성이 있을때
                    else
                    {
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[i].DataItem, "MAX_TRF_QTY", DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[i].DataItem, "BUF_QTY"));
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[i].DataItem, "CHG_TRF_QTY", DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[i].DataItem, "BUF_QTY"));
                    }
                }
            }
        }

        #endregion

        private void dgMaxTrfQtyList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "TRF_CMD_CNT")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMaxTrfQtyList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            string sPortid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PORT_ID"].Index).Value);

            DataRowView drv = dgMaxTrfQtyList.Rows[datagrid.CurrentRow.Index].DataItem as DataRowView;
            string sDBConnForPopup = drv.Row["CONN_DB"].ToString();               
            string sSystemid = cboSystemType.GetBindValue().ToString();

            if (cell != null)
            {
                if (cell.Column.Name == "TRF_CMD_CNT")
                {
                    MCS001_058_TRF_CMD_STATUS_LIST wndPopup = new MCS001_058_TRF_CMD_STATUS_LIST();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[3];
                        Parameters[0] = sPortid;
                        Parameters[1] = sDBConnForPopup;
                        Parameters[2] = sSystemid;

                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        //wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
            }
        }

        //2024-08-17 최평부 추가 - 공통코드 조회
        private DataTable GetCommonCode(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CBO_CODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }

        private void dgMaxTrfQtyList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name != null &&  e.Column.Name.Equals("CHG_TRF_QTY"))
            {
                nInitialQTY = Util.NVC_Int(dgMaxTrfQtyList.GetValue(e.Row.Index, "CHG_TRF_QTY"));
            }
        }       
    }
}