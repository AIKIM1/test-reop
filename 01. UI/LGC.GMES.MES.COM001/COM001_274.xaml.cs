/*************************************************************************************
 Created Date : 2019.09.20
      Creator : 
   Decription : MSS 재고조회 상세
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.20  : Initial Created. 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_274.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_274 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sPlanID = string.Empty;
        private string sCutOffYmd = string.Empty;
        private string sCutOffHour = string.Empty;
        private string sArea = string.Empty;
        private string sOperation = string.Empty;
        private string sElec = string.Empty;
        private string sItem = string.Empty;
        private string sMSS = string.Empty;

        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_274()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
            getPlanID();
            object[] parameters = this.FrameOperation.Parameters;

            if (parameters.Length > 0)
            {
                //sCutOffYmd = Util.NVC(parameters[0]);
                //sCutOffHour = Util.NVC(parameters[1]);
                sOperation = Util.NVC(parameters[2]);
                sItem = Util.NVC(parameters[3]);
                sMSS = Util.NVC(parameters[4]);
                sArea = Util.NVC(parameters[5]);

                cboArea.SelectedValue = sArea;
                cboProcess.SelectedValue = sOperation;
                txtITEM.Text = sItem;
                txtMSS.Text = sMSS;

                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
        }

        private void Initialize()
        {
            InitCombo();

            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals(Area_Type.ASSY))
            {
                dgInventoryList.Columns["WIP_QTY2"].Visibility = Visibility.Collapsed;
                cboArea.IsEnabled = true;
            }                
            else
            {
                dgInventoryList.Columns["WIP_QTY2"].Visibility = Visibility.Visible;
                cboArea.IsEnabled = false;
            }                
        }

        private void InitCombo()
        {

            C1ComboBox[] cboAreaChild = { cboProcess };
            SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            C1ComboBox[] cboProcessChild = { cboEquipmentSegment };

            String[] sFilter1 = { "ASY_OP", "ELE_OP" };
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals(Area_Type.ASSY))
            {
                SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, sFilter: sFilter1[0].Split(','), sCase: "RTS_PROC");
            }
            else
            {
                SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, sFilter: sFilter1[1].Split(','), sCase: "RTS_PROC");
            }               

            C1ComboBox[] cboLineParent = { cboArea, cboProcess };
            SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);

            String[] sFilter3 = { "ELEC_TYPE" };
            SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);

            String[] sFilter4 = { "WIPHOLD" };
            SetCombo(cboMESHold, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODES");
            SetCombo(cboQMSHold, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODES");
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchInventoryListdData();
        }

        private void Cb_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    SetCombo(cbChild);
                }
            }
        }

        #endregion

        #region Mehod

        #region # Combo
        public void SetCombo(C1ComboBox cb, CommonCombo.ComboStatus cs, C1ComboBox[] cbChild = null, C1ComboBox[] cbParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_cbo", cbParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {
                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
            }
        }

        private void SetCombo(C1ComboBox cb)
        {
            try
            {
                Hashtable hashTag = cb.Tag as Hashtable;
                CommonCombo.ComboStatus cs = (CommonCombo.ComboStatus)Enum.Parse(typeof(CommonCombo.ComboStatus), hashTag["all_status"].ToString());
                String[] sFilter = new String[10];
                

                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    C1ComboBox[] cbParentArray = hashTag["parent_cbo"] as C1ComboBox[];
                    int i = 0;
                    for (i = 0; i < cbParentArray.Length; i++)
                    {
                        if (cbParentArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbParentArray[i].SelectedValue.ToString();
                        }
                        else
                        {
                            sFilter[i] = "";
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1) {
                            sFilter[i] = s;
                            i++;
                        }
                        
                    }
                }                
                else if (hashTag.Contains("filter") && hashTag["filter"] != null)
                {
                    sFilter = hashTag["filter"] as String[];
                }
                else
                {
                    sFilter[0] = "";
                }

                switch (hashTag["combo_case"].ToString())
                {
                    case "cboArea":
                        SetComboArea(cb, cs, sFilter);
                        break;
                    case "cboProcess":
                        SetComboProcess(cb, cs, sFilter);
                        break;
                    case "RTS_PROC":
                        SetComboRTSProcess(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegment":
                        SetComboEquipmentSegmant(cb, cs, sFilter);
                        break;
                    case "cboElecType":
                        SetComboEltr(cb, cs, sFilter);
                        break;
                    case "cboEquipment":
                        SetComboEquipment(cb, cs, sFilter);
                        break;
                    case "COMMCODES":
                        SetCommonCode(cb, cs, sFilter);
                        break;
                    default:
                        SetDefaultCbo(cb, cs);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }        

        private void SetComboArea(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboProcess(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0]; ;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                DataTable result = result2.Clone();
                var drTmp = result2?.Select("CBO_CODE IN ('" + Process.ROLL_PRESSING + "','" + Process.SLITTING + "','" + Process.NOTCHING + "')");
                if (drTmp.Length > 0)
                    result = drTmp.CopyToDataTable();

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboRTSProcess(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CATEGORYID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CATEGORYID"] = sFilter[0];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEquipmentSegmant(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0];
                drnewrow["PROCID"] = sFilter[1];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEltr(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEquipment(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string)); 

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = sFilter[2];
                drnewrow["PROCID"] = sFilter[0]; ;
                drnewrow["ELTR_TYPE_CODE"] = Util.NVC(sFilter[1]).Equals("") ? null : sFilter[1];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetCommonCode(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetDefaultCbo(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable dtResult = new DataTable();

                dtResult.Columns.Add("CBO_CODE", typeof(string));
                dtResult.Columns.Add("CBO_NAME", typeof(string));

                DataRow newRow = dtResult.NewRow();

                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { "NA", "구현안됨" };
                dtResult.Rows.Add(newRow);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        #region # Plan ID
        private void getPlanID()
        {
            try
            {
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_PLANID", null, "RSLTDT", null);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    sPlanID = Util.NVC(dtMain.Rows[0]["PLAN_ID"]);
                    sCutOffYmd = Util.NVC(dtMain.Rows[0]["CUTOFF_YYYYMMDD"]);
                    sCutOffHour = Util.NVC(dtMain.Rows[0]["CUTOFF_HOUR"]);

                    txtMSS.Text = sCutOffYmd.Substring(0, 4) + '-' + sCutOffYmd.Substring(4, 2) + '-' + sCutOffYmd.Substring(6, 2) + ' ' + sCutOffHour + ":00"; //ObjectDic.Instance.GetObjectName("시");
                }
            }
            catch (Exception ex) { }
        }
        #endregion
        #region # Inventory List
        private void SearchInventoryListdData()
        {
            try
            {
                Util.gridClear(dgInventoryList);

                getPlanID();

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CUTOFF_YYYYMMDD", typeof(string));
                IndataTable.Columns.Add("CUTOFF_HOUR", typeof(string));
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));
                IndataTable.Columns.Add("OPERATION_ID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));
                IndataTable.Columns.Add("MES_HOLD", typeof(string));
                IndataTable.Columns.Add("QMS_HOLD", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["CUTOFF_YYYYMMDD"] = sCutOffYmd;
                dtRow["CUTOFF_HOUR"] = sCutOffHour;
                dtRow["FACTORY_ID"] = Util.GetCondition(cboArea).Equals("") ? null : Util.GetCondition(cboArea);
                dtRow["OPERATION_ID"] = Util.GetCondition(cboProcess);
                dtRow["ELTR_TYPE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);
                dtRow["MES_HOLD"] = Util.GetCondition(cboMESHold).Equals("") ? null : Util.GetCondition(cboMESHold);
                dtRow["QMS_HOLD"] = Util.GetCondition(cboQMSHold).Equals("") ? null : Util.GetCondition(cboQMSHold);

                IndataTable.Rows.Add(dtRow);

                new ClientProxy().ExecuteService("DA_RTS_SEL_PSI_WIP_DETAIL_LIST", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgInventoryList, result, FrameOperation, false);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion
        
        #endregion
    }
}
