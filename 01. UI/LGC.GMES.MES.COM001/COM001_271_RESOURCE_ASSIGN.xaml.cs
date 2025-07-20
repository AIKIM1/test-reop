/*************************************************************************************
 Created Date : 2019.09.20
      Creator : 
   Decription : RTS 전극 설비별 생산 Item Assign Popup
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.20  : Initial Created. 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_271_RESOURCE_ASSIGN : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtiUse = new DataTable();

        private Util util = new Util();

        private string sCompany;
        private string sDivision;
        private string sSite;
        private string sShop;
        private string sArea;
        private string sProcid;
        private string sEltrType;
        private string sEqptid;

        public COM001_271_RESOURCE_ASSIGN()
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
            InitCombo();

            Util.gridClear(dgTargetList);
            Util.gridClear(dgSourceList);

            if (!sEqptid.Equals(string.Empty))
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
        }
        private void InitCombo()
        {
            C1ComboBox[] cboAreaChild = { cboProcess, cboEquipment };
            SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            C1ComboBox[] cboProcessChild = { cboEquipment };
            C1ComboBox[] cboProcessParent = { cboArea };
            SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            String[] sFilter2 = { "ELEC_TYPE" };
            C1ComboBox[] cboElecChild = { cboEquipment };
            SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, cbChild: cboElecChild, sFilter: sFilter2);

            C1ComboBox[] cboEquipmentParent = { cboArea, cboProcess, cboElecType };
            SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            dtiUse = CommonCodeS("IUSE");

            cboArea.SelectedValue = sArea;
            cboProcess.SelectedValue = sProcid;
            cboElecType.SelectedValue = sEltrType;

            cboEquipment.SelectedValue = sEqptid.Equals(string.Empty)? "SELECT" : sEqptid;

        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                sCompany = Util.NVC(tmps[0]);
                sDivision = Util.NVC(tmps[1]);
                sSite = Util.NVC(tmps[2]);
                sShop = Util.NVC(tmps[3]);
                sArea = Util.NVC(tmps[4]);
                sProcid = Util.NVC(tmps[5]);
                sEltrType = Util.NVC(tmps[6]);
                sEqptid = Util.NVC(tmps[7]);

                Initialize();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Util.GetCondition(cboEquipment, "", true)))
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
                return;
            }
            SearchAssignedData();
            SearchAssignTargetData();
        }
        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnResource_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        AssignResource();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {
            AddRow(dgTargetList, dgSourceList);
        }

        private void btnUnMapping_Click(object sender, RoutedEventArgs e)
        {
            DelRow(dgTargetList, dgSourceList);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
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

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgTargetList);
            Util.gridClear(dgSourceList);
        }

        private void dgTargetList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgTargetList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                if (e.Column.Index != grid.Columns["CHK"].Index && DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(0))
                    e.Cancel = true;
            }
        }

        private void dgTargetList_KeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (!char.IsNumber((char)e.Key.ToString()[(e.Key.ToString().Length - 1)])) { return; }

                if (string.Equals(dataGrid.CurrentCell.Column.Name, "FROM_YYYYMMDD") || string.Equals(dataGrid.CurrentCell.Column.Name, "TO_YYYYMMDD"))
                    (dgTargetList.GetCell(dgTargetList.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = true;
            }
        }

        private void dgTargetList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Name.Equals("ACTIVE_YN") || e.Cell.Column.Name.Equals("FROM_YYYYMMDD") || e.Cell.Column.Name.Equals("TO_YYYYMMDD"))
                    (dgTargetList.GetCell(dgTargetList.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = true;

                //dgList.CurrentRow.Refresh();
            }
            catch (Exception ex) { }
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
                        foreach (string s in sFilter1)
                        {
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
                Util.gridClear(dgTargetList);
                Util.gridClear(dgSourceList);

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0];
                drnewrow["PROCID"] = sFilter[1]; ;
                drnewrow["ELTR_TYPE_CODE"] = Util.NVC(sFilter[2]).Equals("") ? null : sFilter[2];
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
        private DataTable CommonCodeS(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }
        private void SearchAssignedData()
        {
            try
            {
                Util.gridClear(dgTargetList);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("COMPANY_ID", typeof(string));
                IndataTable.Columns.Add("DIVISION_ID", typeof(string));
                IndataTable.Columns.Add("SITE_ID", typeof(string));
                IndataTable.Columns.Add("PLANT_ID", typeof(string));
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));
                IndataTable.Columns.Add("OPERATION_ID", typeof(string));
                IndataTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["COMPANY_ID"] = sCompany;
                dtRow["DIVISION_ID"] = sDivision;
                dtRow["SITE_ID"] = sSite;
                dtRow["PLANT_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["FACTORY_ID"] = Util.GetCondition(cboArea);
                dtRow["OPERATION_ID"] = Util.GetCondition(cboProcess);
                dtRow["EQUIPMENT_ID"] = Util.GetCondition(cboEquipment);
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_MES_ASSIGNED", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgTargetList, dtResult, FrameOperation, false);

                    (dgTargetList.Columns["ACTIVE_YN"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchAssignTargetData()
        {
            try
            {
                Util.gridClear(dgSourceList);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("COMPANY_ID", typeof(string));
                IndataTable.Columns.Add("DIVISION_ID", typeof(string));
                IndataTable.Columns.Add("SITE_ID", typeof(string));
                IndataTable.Columns.Add("PLANT_ID", typeof(string));
                IndataTable.Columns.Add("FACTORY_ID", typeof(string));
                IndataTable.Columns.Add("OPERATION_ID", typeof(string));
                IndataTable.Columns.Add("ITEM_ID", typeof(string));
                IndataTable.Columns.Add("NICK_NAME", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["COMPANY_ID"] = sCompany;
                dtRow["DIVISION_ID"] = sDivision;
                dtRow["SITE_ID"] = sSite;
                dtRow["PLANT_ID"] = LoginInfo.CFG_SHOP_ID;
                dtRow["FACTORY_ID"] = Util.GetCondition(cboArea);
                dtRow["OPERATION_ID"] = Util.GetCondition(cboProcess);
                dtRow["ITEM_ID"] = Util.GetCondition(txtITEM).Equals("") ? null : Util.GetCondition(txtITEM);
                dtRow["NICK_NAME"] = Util.GetCondition(txtPRJ).Equals("") ? null : Util.GetCondition(txtPRJ);

                IndataTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_MES_ASSIGN_TARGET", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgSourceList, dtResult, FrameOperation, false);

                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Assign
        private bool CanSave()
        {
            try
            {
                bool bRet = false;

                if (util.GetDataGridCheckCnt(dgTargetList, "CHK") < 1)
                {
                    Util.MessageValidation("SFU1651");
                    return bRet;
                }

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void AssignResource()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("COMPANY_ID", typeof(string));
                inDataTable.Columns.Add("DIVISION_ID", typeof(string));
                inDataTable.Columns.Add("SITE_ID", typeof(string));
                inDataTable.Columns.Add("PLANT_ID", typeof(string));
                inDataTable.Columns.Add("FACTORY_ID", typeof(string));
                inDataTable.Columns.Add("ITEM_ID", typeof(string));
                inDataTable.Columns.Add("OPERATION_ID", typeof(string));
                inDataTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                inDataTable.Columns.Add("FROM_YMD", typeof(string));
                inDataTable.Columns.Add("TO_YMD", typeof(string));
                inDataTable.Columns.Add("ACTIVE_YN", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgTargetList.GetRowCount(); i++)
                {
                    if (!util.GetDataGridCheckValue(dgTargetList, "CHK", i)) continue;

                    DateTime _ValueFrom = (DateTime)DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "FROM_YYYYMMDD");
                    DateTime _ValueTo = (DateTime)DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "TO_YYYYMMDD");
                    
                    DataRow newRow = inDataTable.NewRow();
                    newRow["COMPANY_ID"] = sCompany;
                    newRow["DIVISION_ID"] = sDivision;
                    newRow["SITE_ID"] = sSite;
                    newRow["PLANT_ID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["FACTORY_ID"] = Util.GetCondition(cboArea);
                    newRow["ITEM_ID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "ITEM_ID"));
                    newRow["OPERATION_ID"] = Util.GetCondition(cboProcess);
                    newRow["EQUIPMENT_ID"] = Util.GetCondition(cboEquipment);
                    newRow["FROM_YMD"] = _ValueFrom.ToString("yyyyMMdd"); ;
                    newRow["TO_YMD"] = _ValueTo.ToString("yyyyMMdd"); ;
                    newRow["ACTIVE_YN"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "ACTIVE_YN"));
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);
                }

                if (inDataTable.Rows.Count < 1)
                {
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService("BR_RTS_REG_MES_ASSIGN", "RQSTDT", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void AddRow(C1.WPF.DataGrid.C1DataGrid Selected, C1.WPF.DataGrid.C1DataGrid Remain)
        {

            if (Selected.GetRowCount() == 0)
            {
                DataTable dtAdd = new DataTable();
                dtAdd.Columns.Add("CHK", typeof(bool));
                dtAdd.Columns.Add("ITEM_ID", typeof(string));
                dtAdd.Columns.Add("NICK_NAME", typeof(string));
                dtAdd.Columns.Add("FROM_YYYYMMDD", typeof(DateTime));
                dtAdd.Columns.Add("TO_YYYYMMDD", typeof(DateTime));
                dtAdd.Columns.Add("ACTIVE_YN", typeof(string));
                dtAdd.Columns.Add("DEL_FLAG", typeof(string));

                Selected.ItemsSource = DataTableConverter.Convert(dtAdd);

                for (int i = 0; i < Remain.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CHK").Equals(1))
                    {
                        DataRow dr = dtAdd.NewRow();
                        dr["CHK"] = true;
                        dr["ITEM_ID"] = DataTableConverter.GetValue(Remain.Rows[i].DataItem, "ITEM_ID");
                        dr["NICK_NAME"] = DataTableConverter.GetValue(Remain.Rows[i].DataItem, "NICK_NAME");
                        dr["FROM_YYYYMMDD"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                        dr["TO_YYYYMMDD"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddYears(100).ToString("yyyy-MM-dd"));
                        dr["ACTIVE_YN"] = "Y";
                        dr["DEL_FLAG"] = "Y";
                        dtAdd.Rows.Add(dr);
                    }
                }
                Selected.ItemsSource = DataTableConverter.Convert(dtAdd);

                (Selected.Columns["ACTIVE_YN"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());

                //for (int i = 0; i < Remain.GetRowCount(); i++)
                //{
                //    if (DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CHK").Equals(1))
                //    {
                //        Selected.IsReadOnly = false;
                //        Selected.BeginNewRow();
                //        Selected.EndNewRow(true);
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CHK", true);
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "ITEM_ID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "ITEM_ID"));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "NICK_NAME", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "NICK_NAME"));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_YYYYMMDD", string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd")));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "TO_YYYYMMDD", string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddYears(100).ToString("yyyy-MM-dd")));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "ACTIVE_YN", "Y");
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "DEL_FLAG", "Y");

                //        (Selected.Columns["ACTIVE_YN"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                //    }
                //}
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgTargetList.ItemsSource);

                for (int i = 0; i < Remain.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CHK").Equals(1))
                    {
                        if (dtTo.Select("ITEM_ID = '" + DataTableConverter.GetValue(Remain.Rows[i].DataItem, "ITEM_ID") + "'").Length > 0) continue;
                        
                        DataRow dr = dtTo.NewRow();
                        dr["CHK"] = true;
                        dr["ITEM_ID"] = DataTableConverter.GetValue(Remain.Rows[i].DataItem, "ITEM_ID");
                        dr["NICK_NAME"] = DataTableConverter.GetValue(Remain.Rows[i].DataItem, "NICK_NAME");
                        dr["FROM_YYYYMMDD"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                        dr["TO_YYYYMMDD"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddYears(100).ToString("yyyy-MM-dd"));
                        dr["ACTIVE_YN"] = "Y";
                        dr["DEL_FLAG"] = "Y";
                        dtTo.Rows.Add(dr);
                    }
                }
                Selected.ItemsSource = DataTableConverter.Convert(dtTo);

                (Selected.Columns["ACTIVE_YN"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());

                //for (int i = 0; i < Remain.GetRowCount(); i++)
                //{
                //    if (DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CHK").Equals(1))
                //    {
                //        Selected.IsReadOnly = false;
                //        Selected.BeginNewRow();
                //        Selected.EndNewRow(true);
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CHK", true);
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "ITEM_ID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "ITEM_ID"));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "NICK_NAME", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "NICK_NAME"));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_YYYYMMDD", string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd")));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "TO_YYYYMMDD", string.Format("{0:yyyy-MM-dd}", DateTime.Now.AddYears(100).ToString("yyyy-MM-dd")));
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "ACTIVE_YN", "Y");
                //        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "DEL_FLAG", "Y");

                //        (Selected.Columns["ACTIVE_YN"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                //    }
                //}
            }
        }

        private void DelRow(C1.WPF.DataGrid.C1DataGrid Selected, C1.WPF.DataGrid.C1DataGrid Remain)
        {
            if (Selected.Rows.Count == 0)
                return;

            for (int i = Selected.GetRowCount(); i > 0; i--)
            {
                int k = 0;
                k = i - 1;
                if (DataTableConverter.GetValue(Selected.Rows[k].DataItem, "CHK").Equals(1) && DataTableConverter.GetValue(Selected.Rows[k].DataItem, "DEL_FLAG").ToString() != "N")
                {
                    //Selected.IsReadOnly = false;
                    Selected.RemoveRow(k);
                    //Selected.IsReadOnly = true;
                }
            }
        }
        #endregion

        #endregion
    }
}
