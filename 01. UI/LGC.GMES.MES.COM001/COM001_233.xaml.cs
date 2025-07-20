/*************************************************************************************
 Created Date : 2018.04.24
      Creator : 신광희
   Decription : 코터 대LOT 정보변경
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.24  신광희 : Initial Created.
  2022.11.04  강호운 : C20221107-000542 - LASER_ABLATION 공정추가
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_233 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private const string ProcessCode = "E2000";
        private string _areaCode;
        private string _equipmentSegmentCode;
        private string _productCode;
        private int _wipseq;
        private string _referenceProcessCode;

        private DataTable TreeNodes
        {
            get;
            set;
        }

        public COM001_233()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> {btnSearch, btnSelectWorkOrder, btnSave};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            CommonCombo combo = new CommonCombo();
            string[] sFilter = { "MKT_TYPE_CODE" };
            combo.SetCombo(cboMarketType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
            combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT);

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotTree(txtLotID.Text);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 변경하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void chkTreeView_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null) return;
                string lotid = (checkBox.DataContext as DataRowView)?.Row["LOTID"].ToString();
                DataRow[] rows = TreeNodes.Select("FROM_LOTID = '" + lotid + "' ");

                foreach (DataRow row in rows)
                {
                    row["CHK"] = true;
                }
                //trvLotTrace.UpdateLayout();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkTreeView_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null) return;

                string lotid = (checkBox.DataContext as DataRowView)?.Row["LOTID"].ToString();
                DataRow[] rows = TreeNodes.Select("FROM_LOTID = '" + lotid + "' ");

                foreach (DataRow row in rows)
                {
                    row["CHK"] = false;
                }
                //trvLotTrace.UpdateLayout();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = sender as LGCDatePicker;

                if (dtPik != null && Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = DateTime.Now.ToLongDateString();
                    dtPik.SelectedDateTime = DateTime.Now;
                    //Util.Alert("SFU1698");      //시작일자 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1698");
                    //e.Handled = false;
                }
            }));
        }

        private void btnSelectWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb?.DataContext == null)
                return;

            if (rb.IsChecked != null && (bool)rb.IsChecked && ((DataRowView) rb.DataContext).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dtWorkOrder = DataTableConverter.Convert(dgWorkOrder.ItemsSource);

                dtWorkOrder.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                dtWorkOrder.Rows[idx]["CHK"] = 1;
                dtWorkOrder.AcceptChanges();

                //Util.GridSetData(dataGrid, dtLot, null, false);
                dgWorkOrder.ItemsSource = DataTableConverter.Convert(dtWorkOrder);
                dgWorkOrder.SelectedIndex = idx;

                txtSelectWO.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID"));
                txtSelectWODetail.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));
                txtSelectProdid.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                txtSelectModelid.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "MODLID"));
                _productCode = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));

                if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "MKT_TYPE_CODE").GetString()))
                {
                    cboMarketType.SelectedIndex = 0;
                }
                else
                {
                    cboMarketType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "MKT_TYPE_CODE"));
                }
                cboLotType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "LOTTYPE"));

                GetRecipeNo();
            }
        }

        private void dgVerChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb?.DataContext == null)
                return;

            if (rb.IsChecked != null && (bool)rb.IsChecked && (((DataRowView) rb.DataContext).Row["CHK"].ToString().Equals("0") || ((DataRowView)rb.DataContext).Row["CHK"].ToString() == "True"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTableConverter.SetValue(dgProdVer.Rows[idx].DataItem, "CHK", true);
                dgProdVer.SelectedIndex = idx;

                txtSelectProdVer.Text = Util.NVC(DataTableConverter.GetValue(dgProdVer.Rows[idx].DataItem, "PROD_VER_CODE"));
            }
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotTree(txtLotID.Text);
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        #endregion

        #region Mehod

        private void GetLotTree(string lotid)
        {
            try
            {
                if (string.IsNullOrEmpty(lotid))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageValidation("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();
                SetClearLotInfo();

                trvLotTrace.ItemsSource = null;
                TreeNodes = null;

                const string bizRuleName = "BR_PRD_SEL_LOT_INFO_COATER_PROD";

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotid;
                dtRqst.Rows.Add(dr);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "LOTSTATUS,TREEDATA", inData);

                // TreeView 의 CheckBox 선택을 위한 DataSet 재정의
                DataSet ds = new DataSet();
                DataTable dtTreeData = Util.CheckBoxColumnAddTable(dsRslt.Tables["TREEDATA"], true).Copy();
                DataTable dtLotStatus = dsRslt.Tables["LOTSTATUS"].Copy();
                ds.Tables.Add(dtTreeData);
                ds.Tables.Add(dtLotStatus);

                ds.Relations.Add("Relations", ds.Tables["TREEDATA"].Columns["LOTID"], ds.Tables["TREEDATA"].Columns["FROM_LOTID"]);

                TreeNodes = ds.Tables["TREEDATA"];
                DataView dvRootNodes = ds.Tables["TREEDATA"].DefaultView;
                dvRootNodes.RowFilter = "FROM_LOTID IS NULL";
                trvLotTrace.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

                if (CommonVerify.HasTableRow(dsRslt.Tables["LOTSTATUS"]))
                {
                    GetLotInfo(lotid, ProcessCode);
                    SetEquipment(dsRslt.Tables["LOTSTATUS"].Rows[0]["AREAID"].GetString(), dsRslt.Tables["LOTSTATUS"].Rows[0]["EQSGID"].GetString(), dsRslt.Tables["LOTSTATUS"].Rows[0]["EQPTID"].GetString());
                    GetWorkOrder();
                    GetRecipeNo();

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

        private void GetLotInfo(string lotId, string processId)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                SetClearLotInfo();

                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["PROCID"] = processId;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    SetLotInfo(dtResult.Rows[0]);
                    //GetWorkOrder();
                    //GetRecipeNo();
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

        private void GetProcessFactoryPlanInfo()
        {
            try
            {
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_PROCESS_FP_INFO();
                DataRow dr = inTable.NewRow();
                dr["PROCID"] = ProcessCode;
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    _referenceProcessCode = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipment(string areaCode, string equipmentSegmentCode, string equipmentCode)
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";
                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = areaCode;
                dr["EQSGID"] = equipmentSegmentCode;
                dr["PROCID"] = ProcessCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();


                var query = (from t in dtResult.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == equipmentCode
                             select t).FirstOrDefault();

                if (query != null)
                {
                    cboEquipment.SelectedValue = equipmentCode;
                }
                else
                {
                    if (dtResult.Rows.Count > 0)
                        cboEquipment.SelectedIndex = 0;
                    else if (dtResult.Rows.Count == 0)
                        cboEquipment.SelectedItem = null;
                }

                //cboEquipment.SelectedValue = equipmentCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetEquipmentWorkOrderWithInnerJoin()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST_WITH_FP";
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _referenceProcessCode.Equals("") ? ProcessCode : _referenceProcessCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue?.ToString()) ? null : cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(txtTopBack.Text))
                    dr["COAT_SIDE_TYPE"] = txtTopBack.Text;

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetEquipmentWorkOrderByProcWithInnerJoin()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _referenceProcessCode.Equals("") ? ProcessCode : _referenceProcessCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(txtTopBack.Text))
                    dr["COAT_SIDE_TYPE"] = txtTopBack.Text;

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetRecipeNo()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_CONV_RATE";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PRODID"] = _productCode;
                dr["AREAID"] = _areaCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inTable);

                Util.GridSetData(dgProdVer, dtResult, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Save()
        {
            try
            {
                const string bizRuleName = "BR_ACT_REG_MODIFY_LOT_COATER_PROD_LOT";
                ShowLoadingIndicator();
                DoEvents();

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("COATER_LOTID", typeof(string));
                inDataTable.Columns.Add("COATER_WIPSEQ", typeof(decimal));
                inDataTable.Columns.Add("COATER_EQSGID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("LOTTYPE", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("LANE_QTY", typeof(int));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["COATER_LOTID"] = txtSelectLot.Text;
                row["COATER_WIPSEQ"] = _wipseq;
                row["COATER_EQSGID"] = _equipmentSegmentCode;
                row["WOID"] = txtSelectWO.Text;
                row["WO_DETL_ID"] = txtSelectWODetail.Text;
                row["PRODID"] = txtSelectProdid.Text;
                row["PROD_VER_CODE"] = txtSelectProdVer.Text;
                row["LOTTYPE"] = cboLotType.SelectedValue.GetString();
                row["MKT_TYPE_CODE"] = cboMarketType.SelectedValue.GetString();
                row["LANE_QTY"] = txtSelectLaneQty.Value;
                row["NOTE"] = txtWipNote.Text;
                row["REQ_USERID"] = txtUserName.Tag.ToString();
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(decimal));

                for (int i = 0; i < TreeNodes.Rows.Count; i++)
                {
                    DataRow dr = inLot.NewRow();

                    if (TreeNodes.Rows[i]["CHK"].GetString() == "True")
                    {
                        dr["LOTID"] = TreeNodes.Rows[i]["LOTID"];
                        dr["WIPSEQ"] = TreeNodes.Rows[i]["WIPSEQ"];

                        inLot.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1166");

                        GetLotTree(txtSelectLot.Text);
                        SetClearLotInfo();

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
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

        private void GetUserWindow()
        {
            CMM_PERSON popPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = txtUserName.Text;
            C1WindowExtension.SetParameters(popPerson, parameters);

            popPerson.Closed += popPerson_Closed;
            grdMain.Children.Add(popPerson);
            popPerson.BringToFront();
        }

        private void popPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;
            }
        }

        private void SetClearLotInfo()
        {
            txtSelectLot.Text = string.Empty;
            txtSelectWO.Text = string.Empty;
            txtSelectWODetail.Text = string.Empty;
            txtSelectProdid.Text = string.Empty;
            txtSelectModelid.Text = string.Empty;
            txtSelectProdVer.Text = string.Empty;
            txtSelectLaneQty.Value = 0;
            txtSelectUnit.Text = string.Empty;
            txtSelectOutQty.Value = 0;
            txtSelectDefectQty.Value = 0;
            txtSelectLossQty.Value = 0;
            txtSelectPrdtReqQty.Value = 0;
            txtTopBack.Text = string.Empty;

            _wipseq = 0;
            _equipmentSegmentCode = string.Empty;
            _productCode = string.Empty;
            _areaCode = string.Empty;
            _referenceProcessCode = string.Empty;

            cboLotType.SelectedIndex = 0;
            cboMarketType.SelectedIndex = 0;
            chkProcess.IsChecked = false;

            Util.gridClear(dgWorkOrder);
            Util.gridClear(dgProdVer);
        }

        private void SetLotInfo(DataRow dr)
        {
            txtSelectLot.Text = dr["LOTID"].ToString();
            txtSelectWO.Text = dr["WOID"].ToString();
            txtSelectWODetail.Text = dr["WO_DETL_ID"].ToString();
            txtSelectProdid.Text = dr["PRODID"].ToString();
            txtSelectModelid.Text = dr["MODLID"].ToString();
            txtSelectProdVer.Text = dr["PROD_VER_CODE"].ToString();
            txtSelectLaneQty.Value = Util.NVC_Int(dr["LANE_QTY"].ToString());
            txtSelectUnit.Text = dr["UNIT_CODE"].ToString();
            txtSelectOutQty.Value = Convert.ToDouble(dr["WIPQTY_ED"].ToString());
            txtSelectDefectQty.Value = Convert.ToDouble(dr["CNFM_DFCT_QTY"].ToString());
            txtSelectLossQty.Value = Convert.ToDouble(dr["CNFM_LOSS_QTY"].ToString());
            txtSelectPrdtReqQty.Value = Convert.ToDouble(dr["CNFM_PRDT_REQ_QTY"].ToString());
            txtTopBack.Text = dr["COATING_SIDE_TYPE_CODE"].ToString();
            cboMarketType.SelectedValue = dr["MKT_TYPE_CD"].GetString();
            cboLotType.SelectedValue = dr["LOTTYPE"].GetString();
            _wipseq = Util.NVC_Int(dr["WIPSEQ"].ToString());
            _areaCode = dr["AREAID"].ToString();
            _productCode = dr["PRODID"].ToString();
            _equipmentSegmentCode = dr["EQSGID"].ToString();

            Util.gridClear(dgWorkOrder);
            Util.gridClear(dgProdVer);
        }

        private void GetWorkOrder()
        {
            Util.gridClear(dgWorkOrder);

            GetProcessFactoryPlanInfo();
            InitializeGridColumns();

            DataTable dtResult = chkProcess.IsChecked == true ? GetEquipmentWorkOrderByProcWithInnerJoin() : GetEquipmentWorkOrderWithInnerJoin();
            cboMarketType.IsEnabled = false;
            cboLotType.IsEnabled = false;

            if (dtResult == null || dtResult.Rows.Count < 1)
                return;

            DataRow[] drUpdate = dtResult.Select("CHK = 1");

            if (drUpdate.Length > 0)
                drUpdate[0]["CHK"] = 0;

            Util.GridSetData(dgWorkOrder, dtResult, FrameOperation, true);

            if (string.IsNullOrWhiteSpace(txtSelectWO.Text))
            {
                int idx = _util.GetDataGridRowIndex(dgWorkOrder, "WOID", txtSelectWO.Text);

                if (idx >= 0)
                {
                    DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);
                    dgWorkOrder.SelectedIndex = idx;
                }
            }

            if (chkProcess.IsChecked.HasValue && (bool)chkProcess.IsChecked)
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            else
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
        }

        private void InitializeGridColumns()
        {
            if (dgWorkOrder == null)
                return;

            if (string.IsNullOrWhiteSpace(txtSelectLot.Text))
                return;

            /*
             * C/Roll, S/Roll, Lane수 적용 공정.
             *     C/ROLL = PLAN_QTY(S/ROLL) / LANE_QTY
             * E2000  - TOP_COATING
             * E2300  - INS_COATING
             * E2500  - HALF_SLITTING
             * E3000  - ROLL_PRESSING
             * E3500  - TAPING
             * E3800  - REWINDER
             * E3300  - LASER_ABLATION [C20221107-000542]
             * E3900  - BACK_WINDER
             */
            if (ProcessCode.Equals(Process.TOP_COATING) ||
                ProcessCode.Equals(Process.INS_COATING) ||
                ProcessCode.Equals(Process.HALF_SLITTING) ||
                ProcessCode.Equals(Process.ROLL_PRESSING) ||
                ProcessCode.Equals(Process.TAPING) ||
                ProcessCode.Equals(Process.REWINDER) || 
                ProcessCode.Equals(Process.LASER_ABLATION) ||
                ProcessCode.Equals(Process.BACK_WINDER))
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Visible;

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Visible;

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Visible;

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Collapsed;

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Collapsed;

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
            }
        }

        private bool ValidationSave()
        {

            if (string.IsNullOrEmpty(txtLotID.Text))
            {
                // 변경 대상이 없습니다. 변경 할 LOT을 선택 하세요.
                Util.MessageValidation("SFU1565");
                return false;
            }

            if (string.IsNullOrEmpty(txtWipNote.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtUserName.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            if (cboLotType.SelectedValue == null || cboLotType.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU4068");
                return false;
            }

            if (cboMarketType.SelectedValue == null || cboMarketType.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU4371");
                return false;
            }

            return true;
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

        private void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvLotTrace, typeof(C1TreeViewItem), ref items);

            foreach (var o in items)
            {
                var item = (C1TreeViewItem)o;
                TreeItemExpandNodes(item);
            }
        }

        private void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            item.UpdateLayout();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);

                foreach (var o in items)
                {
                    var childItem = (C1TreeViewItem)o;
                    TreeItemExpandNodes(childItem);
                }
            }));
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion


    }
}
