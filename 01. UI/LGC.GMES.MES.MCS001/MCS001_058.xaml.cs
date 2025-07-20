/*************************************************************************************
 Created Date : 2021.04.01
      Creator : ������
   Decription : ���� ��Ʈ�� �ݼ۰���(����) ���� ����
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.01  ������ : Initial Created. 
  2022.09.14  ��ȭ�� : DA_MCS_SEL_MCS_CONFIG_CONN ==> DA_MHS_SEL_CONFIG_CONN
  2023.07.31  �ӽ��� : E20230731-000133. �ִ뼳�����ɹݼۼ����߰��� ���/���� ���ۼ��� �и� �Է�
  2024.02.27  ������ : E20231215-001596 ���� �̼� ��� ��Ȳ Ȯ�� �˾� ȭ�� �߰�
  2024.08.17  ����� : ESST VD590 SI PROJECT > �ؼ� �÷� ����ó��(�����ڵ� : DIFFUSION_SITE �� �б�ó��)
  2025.03.24  �̹��� : BR_MHS_UPD_CURR_PORT_MAX_TRF_QTY ȣ��� �ý��� ����[SYSTEM_TYPE_CODE] �������� ����, Ȱ��ȭ�� ������ �� BR�� ó��
  2025.04.28  �̹��� : Intelligence Buffer����� ���� �÷� �߰�
  2025.04.30  �̹��� : ��ȸ ���� ���ڰ� �߰� (db_conn, systemid)
  2025.07.07  �躴�� : dtResult2 null �� ��� ������ �߰�.
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
using LGC.GMES.MES.ControlsLibrary;  // E20230731-000133 �߰� �ӽ���
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
            //2024-08-17 by �����
            //diffusion_site �����ڵ� ��ȸ(ȭ�� : '�ؼ�' �÷� �б�ó��)
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
            //2024-08-17 by ����� END

            CommonCombo comboSet = new CommonCombo(); 
            String[] sFilter = { string.Empty };

            //��
            comboSet.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // �ý��� ����
            cboSystemType.SetCommonCode("MES_SYSTEM_TYPE_CODE", CommonCombo.ComboStatus.SELECT, true);
            cboSystemType.SelectedValue = LoginInfo.CFG_SYSTEM_TYPE_CODE;

            //����
            string[] arrColumn1 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition1 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue() };
            cboEqpType.SetDataComboItem("DA_MHS_SEL_EQUIPMENTGROUP_PORT_CBO", arrColumn1, arrCondition1, CommonCombo.ComboStatus.SELECT);

            //����
            string[] arrColumn0 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE", "EQGRID" };
            string[] arrCondition0 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue(), cboEqpType.GetStringValue() };
            cboLine.SetDataComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_SYSTYPE_CBO", arrColumn0, arrCondition0, CommonCombo.ComboStatus.ALL);

            //����
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

            //����
            string[] arrColumn1 = { "LANGID", "AREAID", "SYSTEM_TYPE" };
            string[] arrCondition1 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue() };
            cboEqpType.SetDataComboItem("DA_MHS_SEL_EQUIPMENTGROUP_PORT_CBO", arrColumn1, arrCondition1, CommonCombo.ComboStatus.SELECT);
        }
        
        private void cboEqpType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            //����
            string[] arrColumn0 = { "LANGID", "AREAID", "SYSTEM_TYPE_CODE", "EQGRID" };
            string[] arrCondition0 = { LoginInfo.LANGID, cboArea.GetStringValue(), cboSystemType.GetStringValue(), cboEqpType.GetStringValue() };
            cboLine.SetDataComboItem("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_SYSTYPE_CBO", arrColumn0, arrCondition0, CommonCombo.ComboStatus.ALL);

            //����
            string[] arrColumn2 = { "LANGID", "SHOPID", "AREAID", "EQGRID", "EQSGID", "SYSTEM_TYPE_CODE" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.GetStringValue(), cboEqpType.GetStringValue(),
                cboLine.GetStringValue().Equals(string.Empty) ? null : cboLine.GetStringValue(),
                cboSystemType.GetStringValue().Equals(string.Empty) ? null : cboSystemType.GetStringValue() };
            cboEqp.SetDataComboItem("DA_MHS_SEL_EQUIPMENT_PORT_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.ALL, true);
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            //����
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
                    // ���õ� �׸��� �����ϴ�.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                //�����Ͻðڽ��ϱ�?
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
                // E20230731 - 000133 Start. �ؼ��� �ִ� ��� ���� Port ID �ٸ� �ؼ��� ���� �ڵ� üũó��
                int maxTrfQty = Util.NVC_Int(dgMaxTrfQtyList.GetValue(e.Cell.Row.Index, "MAX_TRF_QTY"));                

                if (!Convert.ToInt32(e.Cell.Value).Equals(maxTrfQty))
                {
                    dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 1);

                    //���� PORT�� CHK �ȵǾ��ִ°��� ������ ã�Ƽ� üũó���Ѵ�.
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "POLARITY"))))
                    {
                        strPortId = Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "PORT_ID"));

                        for (int idx = 0; idx < dgMaxTrfQtyList.Rows.Count; idx++)
                        {
                            // ������ Port�̸鼭 CHK�ȵȰ��� �ִ��� ã�´�.
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
                        // ���� Port�� �ٸ� Row�� Max������ �ٸ����� �ִ��� ã�´�.
                        strPortId = Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[e.Cell.Row.Index].DataItem, "PORT_ID"));

                        for (int idx = 0; idx < dgMaxTrfQtyList.Rows.Count; idx++)
                        {
                            // ������ Port�̸鼭 CHK�ȵȰ��� �ִ��� ã�´�.
                            if (strPortId.Equals(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "PORT_ID"))))
                            {
                                if (!idx.Equals(e.Cell.Row.Index))
                                {
                                    // �� Row�� Max������ ��������� �����ϸ�?
                                    nSamePortOtherPolarityMaxTrfQty = Util.NVC_Int(dgMaxTrfQtyList.GetValue(idx, "MAX_TRF_QTY"));

                                    if (!Util.NVC_Int(dgMaxTrfQtyList.GetValue(idx, e.Cell.Column.Name.ToString())).Equals(nSamePortOtherPolarityMaxTrfQty))
                                    {
                                        dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 1);  // ������ Row�� CHK�� True��
                                        dgMaxTrfQtyList.SetValue(idx, "CHK", 1);               // ���� Port �ٸ� �ؼ� Row�� ���� CHK�� True��.
                                    }
                                    else
                                    {
                                        dgMaxTrfQtyList.SetValue(e.Cell.Row.Index, "CHK", 0);  // ������ Row�� CHK�� False��
                                        dgMaxTrfQtyList.SetValue(idx, "CHK", 0);               // ���� Port �ٸ� �ؼ� Row�� ���� CHK�� False��.
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
        /// // E20230731 - 000133 Start. ����Ÿ ��ȸ/��� �� PORT ID �������� CELL ����
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
                // ���� �����ϼ���.
                Util.MessageValidation("SFU4925", lblArea.Text);
                cboArea.Focus();
                return false;
            }

            if (cboSystemType.SelectedValue == null || cboSystemType.SelectedValue.Equals("SELECT"))
            {
                // �ý��� ������ �����ϼ���.
                Util.MessageValidation("SFU4925", lblSystemType.Text);
                cboSystemType.Focus();
                return false;
            }

            if (cboEqpType.SelectedValue == null || cboEqpType.SelectedValue.Equals("SELECT"))
            {
                // ������ �����ϼ���.
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

                GetBizActorServerInfo(); // 2024.10.21. �迵�� - MSC���� ����ϴ� DA_MCS_SEL_MCS_CONFIG_INFO �κ� �ּ� ó��

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
                // �ؼ��� ������ Port�� ��� BUF Qty�� MAX_TRF_QTY, CHG_TRF_QTY Į���� �����Ѵ�.
                // �ؼ��� ���� ���� MAX_TRF_QTY�� CHG_TRF_QTY Į���� �����Ѵ�.
                SetPortPolarityBufferQty();

                // ����Ÿ ��ȸ/��� �� PORT��, �ִ뼳�����ɼ��� Į�� Row ����
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
                // CHKó���� Validation
                //-----------------------------------------
                // dgMaxTrfQtyList_CommittedEdit()���� ��������� üũó�������� �����ư ������ ���� ����� Ȥ�ö� üũ�����ϴ� ��찡 �߻��Ҽ��־� �ؼ��� ������ �� �� �ϳ��� üũ�Ǿ��ִµ� �ϳ��� üũ�ȵǾ������� �ΰ� �� üũó���ǰ� �Ѵ�.
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
                        // �ش� Port ID�� ���� üũ �ȵǾ������� üũó���Ѵ�.
                        if (Util.NVC(kvp.Value).Equals(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "PORT_ID"))))
                        {
                            if (!Convert.ToBoolean(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[idx].DataItem, "CHK")))
                            {
                                dgMaxTrfQtyList.SetValue(idx, "CHK", 1);
                            }
                        }
                    }
                }

                // Port�� �ؼ��� ������ ��� Buffer�� ��������� Max������ Buffer�� ������ ������ �Ѵ�.
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

                // Port�� ���� �Է��Ѱ��� �ִ� ���� ���� �ݼ� ������ �ʰ��ϴ��� üũ. MAX_SET_ENABLE_TRF_QTY ���� ���� ���� ���� üũ���� ����.
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
                                Util.MessageValidation("100000210", errMsgParam);  //�ִ� ���� ���� �ݼ� ����(%1)���� ���氪�� Ů�ϴ�. \\nPORT ID[%2]
                                return;
                            }
                        }
                    }
                }

                //-----------------------------------------
                // ����Ÿ ����
                //-----------------------------------------
                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("PORT_ID", typeof(string));
                INDATA.Columns.Add("UPDUSER", typeof(string));
                INDATA.Columns.Add("EQUIPMENT_ID", typeof(string));   // 2024.11.01. �迵�� - EQUIPMENT_ID �߰� (����� ��� ��û)
                INDATA.Columns.Add("MAX_TRF_QTY", typeof(string));
                INDATA.Columns.Add("POLARITY", typeof(string));  // �ӽ���
                INDATA.Columns.Add("BUF_QTY", typeof(string));   // �ӽ���
                INDATA.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));  // 2025.03.25 �ں��� å�� ��û���� �߰�
                INDATA.Columns.Add("STOP_BAS_SEC", typeof(string));      // 2025.04.29 �ں��� å�� ��û���� �߰�
                INDATA.Columns.Add("RUN_BAS_SEC", typeof(string));       // 2025.04.29 �ں��� å�� ��û���� �߰�
                                
                foreach (DataRow drSelect in dgMaxTrfQtyList.GetCheckedDataRow("CHK"))
                {
                    //if (!drSelect["CONN_DB"].Equals(cboSystemType.GetBindValue())) continue;

                    DataRow newRow = INDATA.NewRow();
                    newRow["PORT_ID"] = drSelect["PORT_ID"];
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    newRow["EQUIPMENT_ID"] = drSelect["EQPTID"]; // 2024.11.01. �迵�� - EQUIPMENT_ID �߰� (����� ��� ��û)

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
                    new ClientProxy().ExecuteServiceSync("BR_MHS_UPD_CURR_PORT_MAX_TRF_QTY", "INDATA", "OUTDATA", INDATA); // 2024.11.01. �迵�� - ���� ȣ��Ǵ� DA���� BR�� ����.
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
                Util.MessageInfo("SFU1275");    //���� ó�� �Ǿ����ϴ�.

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// E20230731 - 000133. �ؼ��� ���� ���� �ִ�ݼۼ����� ������ Į���� ����ϰ� �ؼ��� �ִ� ��� �ؼ��� ���۰��� ������ Į���� ���
        /// </summary>
        private void SetPortPolarityBufferQty()
        {
            if (dgMaxTrfQtyList.Rows.Count > 0)
            {
                for (int i = 0; i < dgMaxTrfQtyList.Rows.Count; i++)
                {
                    // �ؼ��� ������
                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[i].DataItem, "POLARITY"))))
                    {
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[i].DataItem, "POLARITY_NAME", "-");
                        DataTableConverter.SetValue(dgMaxTrfQtyList.Rows[i].DataItem, "CHG_TRF_QTY", DataTableConverter.GetValue(dgMaxTrfQtyList.Rows[i].DataItem, "MAX_TRF_QTY"));
                    }
                    // �ؼ��� ������
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

        //2024-08-17 ����� �߰� - �����ڵ� ��ȸ
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