/*************************************************************************************
 Created Date : 2019.09.20
      Creator : 
   Decription : RTS 전극 설비별 생산 Item Assign
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_271.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_271 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sCompany = string.Empty;
        private string sDivision = string.Empty;
        private string sSite = string.Empty;
        private string sPlant = string.Empty;

        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_271()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
        }

        private void Initialize()
        {
            InitCombo();
            ApplyPermissions();
        }

        private void InitCombo()
        {
            C1ComboBox[] cboAreaChild = { cboProcess };
            SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
            C1ComboBox[] cboProcessParent = { cboArea };
            SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            C1ComboBox[] cboLineParent = { cboArea, cboProcess };
            SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            String[] sFilter3 = { "ELEC_TYPE" };
            //C1ComboBox[] cboElecChild = { cboEquipment };
            SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);

            SearchCompany();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnResource);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        private void btnResource_Click(object sender, RoutedEventArgs e)
        {
            popupResourceAssign(Util.GetCondition(cboProcess), Util.GetCondition(cboElecType), Util.GetCondition(cboEquipment), Util.GetCondition(cboArea));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboEquipment.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
                return;
            }

            SearchAssigListdData();
        }

        private void cboProcess_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboElecType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        private void dgAssignList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgAssignList == null || dgAssignList.CurrentRow == null || dgAssignList.CurrentRow.Index < 1)
                return;

            popupResourceAssign(Util.NVC(DataTableConverter.GetValue(dgAssignList.Rows[dgAssignList.CurrentRow.Index].DataItem, "OPERATION_ID")),
                                Util.NVC(DataTableConverter.GetValue(dgAssignList.Rows[dgAssignList.CurrentRow.Index].DataItem, "ELTR_TYPE")),
                                Util.NVC(DataTableConverter.GetValue(dgAssignList.Rows[dgAssignList.CurrentRow.Index].DataItem, "EQUIPMENT_ID")),
                                Util.NVC(DataTableConverter.GetValue(dgAssignList.Rows[dgAssignList.CurrentRow.Index].DataItem, "FACTORY_ID")));
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
                    case "cboEquipmentSegment":
                        SetComboEquipmentSegmant(cb, cs, sFilter);
                        break;
                    case "cboElecType":
                        SetComboEltr(cb, cs, sFilter);
                        break;
                    case "cboEquipment":
                        SetComboEquipment(cb, cs, sFilter);
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

        private void SetCboEquipment()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                drnewrow["ELTR_TYPE_CODE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboEquipment.ItemsSource = DataTableConverter.Convert(result);
                cboEquipment.CheckAll();
            }
            catch (Exception ex)
            {

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

        #region # Assign List
        private void SearchCompany()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["FACTORY_ID"] = LoginInfo.CFG_AREA_ID;

                IndataTable.Rows.Add(dtRow);

                new ClientProxy().ExecuteService("DA_RTS_SEL_FACTORY", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        sCompany =  Util.NVC(result.Rows[0]["COMPANY_ID"]);
                        sDivision = Util.NVC(result.Rows[0]["DIVISION_ID"]);
                        sSite = Util.NVC(result.Rows[0]["SITE_ID"]);
                        sPlant = Util.NVC(result.Rows[0]["PLANT_ID"]);
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
        private void SearchAssigListdData()
        {
            try
            {
                Util.gridClear(dgAssignList);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("COMPANY_ID", typeof(string));
                IndataTable.Columns.Add("DIVISION_ID", typeof(string));
                IndataTable.Columns.Add("SITE_ID", typeof(string));
                IndataTable.Columns.Add("PLANT_ID", typeof(string));
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));
                IndataTable.Columns.Add("OPERATION_ID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE", typeof(string));
                IndataTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));
                IndataTable.Columns.Add("ASSIGN_FLAG", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["COMPANY_ID"] = sCompany;
                dtRow["DIVISION_ID"] = sDivision;
                dtRow["SITE_ID"] = sSite;
                dtRow["PLANT_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["FACTORY_ID"] = Util.GetCondition(cboArea);
                dtRow["ELTR_TYPE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRow["OPERATION_ID"] = Util.GetCondition(cboProcess);
                dtRow["EQUIPMENT_ID"] = cboEquipment.SelectedItemsToString;
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);
                dtRow["ASSIGN_FLAG"] = chkAsssignFlag.IsChecked == true ? "Y" : "N";

                IndataTable.Rows.Add(dtRow);

                new ClientProxy().ExecuteService("DA_RTS_SEL_MES_ASSIGNED_LIST", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgAssignList, result, FrameOperation, false);
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

        #region # Resource Assign Popup
        private void popupResourceAssign(string sPROCID, string sELECTYPE, string sEQPTID, string sAREAID)
        {
            COM001_271_RESOURCE_ASSIGN popResourceAssign = new COM001_271_RESOURCE_ASSIGN();
            popResourceAssign.FrameOperation = FrameOperation;

            object[] Parameters = new object[9];

            Parameters[0] = sCompany;
            Parameters[1] = sDivision;
            Parameters[2] = sSite;
            Parameters[3] = sPlant;
            Parameters[4] = sAREAID;
            Parameters[5] = sPROCID;
            Parameters[6] = sELECTYPE;
            Parameters[7] = sEQPTID;

            C1WindowExtension.SetParameters(popResourceAssign, Parameters);

            popResourceAssign.Closed += new EventHandler(popResourceAssign_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            this.Dispatcher.BeginInvoke(new Action(() => popResourceAssign.ShowModal()));
        }

        private void popResourceAssign_Closed(object sender, EventArgs e)
        {
            COM001_271_RESOURCE_ASSIGN popup = sender as COM001_271_RESOURCE_ASSIGN;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
        }

        #endregion

        #endregion

       
    }
}
