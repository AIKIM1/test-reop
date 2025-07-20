/*************************************************************************************
 Created Date : 2018.05.23
      Creator : 신광희
   Decription : 공정별 생산LOT 정보변경
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.23  신광희 : Initial Created.
  2021.06.25  이상훈  C20210625-000089 '공정별 생산LOT 정보변경'에서 W/O 미선택시 에러 발생 요청
  2022.11.04  강호운 : C20221107-000542 - LASER_ABLATION 공정추가
  2024.03.11  안유수 : E20240125-001319 LOT 정보 변경 확인 팝업창 추가
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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_237 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private string _areaCode;
        private string _processCode;
        private string _equipmentSegmentCode;
        private string _equipmentCode;
        private string _productCode;
        private int _wipseq;
        private string _referenceProcessCode;
        private string _planManagementTypeCode;
        private string _lotTypeCode;

        private DataTable dtlotinfo = new DataTable();
        private DataTable dtbefore = new DataTable();
        private DataTable dtafter = new DataTable();

        public COM001_237()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
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

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> {btnSearch, btnSelectWO, btnSave};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotTree(txtLotID.Text);
        }
        #endregion

        #region [생성,삭제]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            GetDataTableSet();//저장 시 데이터 확인 팝업창 파라메타 데이터테이블 저장
            COM001_237_SAVE_DATA_CHECK wndPopup = new COM001_237_SAVE_DATA_CHECK();
            //wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dtbefore;
                Parameters[1] = dtafter;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndSave_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }

            //if (wndPopup.DialogResult == MessageBoxResult.OK)
            //{
            //    // 변경하시겠습니까?
            //    Util.MessageConfirm("SFU2875", (result) =>
            //    {
            //        if (result == MessageBoxResult.OK)
            //        {
            //            Save();
            //        }
            //    });
            //}

        }

        private void wndSave_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.COM001_237_SAVE_DATA_CHECK wndPopup = sender as LGC.GMES.MES.COM001.COM001_237_SAVE_DATA_CHECK;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    // 변경하시겠습니까?
                    Util.MessageConfirm("SFU2875", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                             Save();
                        }
                    });                                     
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

            #region [LOT 선택]
        private void rbCheck_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                SetDataGridCheckHeaderInitialize(dgOutLot);
                GetLotInfo((rb.DataContext as DataRowView)?.Row["LOTID"].ToString(), (rb.DataContext as DataRowView)?.Row["PROCID"].ToString());
                GetOutLot((rb.DataContext as DataRowView)?.Row["LOTID"].ToString());
            }
                
        }

        #endregion

        #region [Wo 일자 변경]
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
        #endregion

        #region [공정] - W/O 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipment();
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcess();
        }

        #endregion

        #region [Wo 조회]
        private void btnSelectWO_Click(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }
        #endregion

        #region [WO 선택하기]
        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb?.DataContext == null)
                return;

            if (rb.IsChecked != null && (bool)rb.IsChecked && ((DataRowView)rb.DataContext).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dtWorkOrder = DataTableConverter.Convert(dgWorkOrder.ItemsSource);

                // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                dtWorkOrder.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                dtWorkOrder.Rows[idx]["CHK"] = 1;
                dtWorkOrder.AcceptChanges();

                //Util.GridSetData(dataGrid, dtLot, null, false);
                dgWorkOrder.ItemsSource = DataTableConverter.Convert(dtWorkOrder);

                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = idx;

                txtSelectWO.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID"));
                txtSelectWODetail.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));
                txtSelectProdid.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                txtSelectModelid.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "MODLID"));
                txtLotType.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "LOTYNAME"));
                txtMarketType.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "MKT_TYPE_CODE"));

                _productCode = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                _lotTypeCode = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "LOTTYPE"));
            }

        }
        #endregion
  

        #region [LOT ID]
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotTree(txtLotID.Text);
            }
        }
        #endregion

        #region [요청자]
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

        #region [공정 체크박스]
        private void chkProcess_Checked(object sender, RoutedEventArgs e)
        {
            cboEquipment.IsEnabled = false;
        }

        private void chkProcess_Unchecked(object sender, RoutedEventArgs e)
        {
            cboEquipment.IsEnabled = true;
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllChecked(dgOutLot);

            foreach (C1.WPF.DataGrid.DataGridRow row in dgOutLot.Rows)
            {
                if (DataTableConverter.GetValue(row.DataItem, "CLOSING").GetString() != "CLOSE")
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
            }
            dgOutLot.EndEdit();
            dgOutLot.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgOutLot);
        }

        private void dgOutLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "CHK")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            if (drv["CLOSING"].GetString() == "CLOSE")
                            {
                                Util.MessageValidation("SFU1172");
                                e.Cancel = true;
                            }
                            else
                            {
                                e.Cancel = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #endregion

        #region Mehod

        #region [Tree 목록 가져오기]
        private void GetLotTree(string lotid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lotid))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageValidation("SFU1190");
                    return;
                }

                const string bizRuleName = "BR_PRD_SEL_LOT_INFO_END";

                ShowLoadingIndicator();
                DoEvents();

                SetClearLotInfo();


                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("GUBUN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotid;
                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "LOTSTATUS,TREEDATA", inData);

                dsRslt.Relations.Add("Relations", dsRslt.Tables["TREEDATA"].Columns["LOTID"], dsRslt.Tables["TREEDATA"].Columns["FROM_LOTID"]);
                DataView dvRootNodes;
                dvRootNodes = dsRslt.Tables["TREEDATA"].DefaultView;
                dvRootNodes.RowFilter = "FROM_LOTID IS NULL";

                trvLotTrace.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

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

        #region [LOT 정보 가져오기]
        private void GetLotInfo(string lotId, string processCode)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_LOT_INFO";
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
                dr["PROCID"] = processCode;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    SetLotInfo(dtResult.Rows[0]);
                    GetWorkOrder();
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
        private void GetProcessFactoryPlanInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_PROCESS_FP_INFO();

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = _processCode;

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    _referenceProcessCode = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                    _planManagementTypeCode = Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]);
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
                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment?.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", inTable);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                var query = (from t in dtResult.AsEnumerable()
                    where t.Field<string>("CBO_CODE") == _processCode
                             select t).FirstOrDefault();

                if (query != null)
                {
                    cboProcess.SelectedValue = _processCode;
                }
                else
                {
                    if (dtResult.Rows.Count > 0)
                        cboProcess.SelectedIndex = 0;
                    else if (dtResult.Rows.Count == 0)
                        cboProcess.SelectedItem = null;
                }
                //cboProcess.SelectedValue = _procid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void SetEquipmentSegment()
        {
            try
            {
                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaCode;
                //dr["PROCID"] = processCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", inTable);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

                cboEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipmentSegment.SelectedValue = _equipmentSegmentCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaCode;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.GetString();
                dr["PROCID"] = cboProcess.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", inTable);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipment.SelectedValue = _equipmentCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [WO 정보 가져오기]
        private DataTable GetMixingWorkOrder()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST_WITH_FP_MX";

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _referenceProcessCode.Equals("") ? cboProcess.SelectedValue.ToString() : _referenceProcessCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
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
                const string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP";

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _referenceProcessCode.Equals("") ? cboProcess.SelectedValue.ToString() : _referenceProcessCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
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
                dr["PROCID"] = _referenceProcessCode.Equals("") ? cboProcess.SelectedValue.ToString() : _referenceProcessCode;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
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

        private DataTable GetEquipmentWorkOrderByProc()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST_BY_PROCID";
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _referenceProcessCode.Equals("") ? cboProcess.SelectedValue.ToString() : _referenceProcessCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

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

        private DataTable GetEquipmentWorkOrder()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST";
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _referenceProcessCode.Equals("") ? cboProcess.SelectedValue.ToString() : _referenceProcessCode;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

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
        #endregion


        private void GetDataTableSet()
        {
            dtafter = new DataTable();
            dtbefore = new DataTable();

            dtafter.Columns.Add("LOTID", typeof(string));
            dtafter.Columns.Add("LOTTYPE", typeof(string));
            dtafter.Columns.Add("WOID", typeof(string));
            dtafter.Columns.Add("WO_DETL_ID", typeof(string));
            dtafter.Columns.Add("PRODID", typeof(string));
            dtafter.Columns.Add("MODLID", typeof(string));
            dtafter.Columns.Add("MKT_TYPE_CODE", typeof(string));

            dtbefore.Columns.Add("LOTID", typeof(string));
            dtbefore.Columns.Add("LOTTYPE", typeof(string));
            dtbefore.Columns.Add("WOID", typeof(string));
            dtbefore.Columns.Add("WO_DETL_ID", typeof(string));
            dtbefore.Columns.Add("PRODID", typeof(string));
            dtbefore.Columns.Add("MODLID", typeof(string));
            dtbefore.Columns.Add("MKT_TYPE_CODE", typeof(string));

            foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in dgOutLot.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")) == "True" || Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")) == "1")
                {
                    DataRow param = dtafter.NewRow();
                    param["LOTID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "LOTID");
                    param["LOTTYPE"] = txtLotType.Text;
                    param["WOID"] = txtSelectWO.Text;
                    param["WO_DETL_ID"] = txtSelectWODetail.Text;
                    param["PRODID"] = txtSelectProdid.Text;
                    param["MODLID"] = txtSelectModelid.Text;
                    param["MKT_TYPE_CODE"] = txtMarketType.Text;

                    dtafter.Rows.Add(param);


                    DataRow[] drinfo = dtlotinfo.Select("LOTID = '" + DataTableConverter.GetValue(dataGridRow.DataItem, "LOTID") + "'");

                    DataRow param2 = dtbefore.NewRow();
                    param2["LOTID"] = Util.NVC(drinfo[0]["LOTID"]);
                    param2["LOTTYPE"] = Util.NVC(drinfo[0]["LOTYNAME"]);
                    param2["WOID"] = Util.NVC(drinfo[0]["WOID"]);
                    param2["WO_DETL_ID"] = Util.NVC(drinfo[0]["WO_DETL_ID"]);
                    param2["PRODID"] = Util.NVC(drinfo[0]["PRODID"]);
                    param2["MODLID"] = Util.NVC(drinfo[0]["MODLID"]);
                    param2["MKT_TYPE_CODE"] = Util.NVC(drinfo[0]["MKT_TYPE_CODE"]);
                    dtbefore.Rows.Add(param2);
                }

                //else
                //{
                //    DataRow param = dtafter.NewRow();
                //    param["LOTID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "LOTID");
                //    param["LOTTYPE"] = DataTableConverter.GetValue(dataGridRow.DataItem, "LOTTYPE");  
                //    param["WOID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "WOID");
                //    param["WO_DETL_ID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "WO_DETL_ID");
                //    param["PRODID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "PRODID");
                //    param["MODLID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "MODLID");
                //    param["MKT_TYPE_CODE"] = DataTableConverter.GetValue(dataGridRow.DataItem, "MKT_TYPE_CODE");

                //    dtafter.Rows.Add(param);
                //}
            }
        }

        #region [Lot 정보 변경]
        private void Save()
        {
            try
            {
                const string bizRuleName = "BR_ACT_REG_MODIFY_LOT_PROD_LOT";
                ShowLoadingIndicator();
                DoEvents();

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("LOTTYPE", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["WOID"] = txtSelectWO.Text;
                row["WO_DETL_ID"] = txtSelectWODetail.Text;
                row["PRODID"] = txtSelectProdid.Text;
                row["LOTTYPE"] = _lotTypeCode;
                row["MKT_TYPE_CODE"] = txtMarketType.Text;
                row["PROD_LOTID"] = txtSelectLot.Text;
                row["NOTE"] = txtWipNote.Text;
                row["REQ_USERID"] = txtUserName.Tag.ToString();
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(decimal));

                foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in dgOutLot.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")) == "True" || Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")) == "1")
                    {
                        DataRow param = inLot.NewRow();
                        param["LOTID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "LOTID");
                        param["WIPSEQ"] = DataTableConverter.GetValue(dataGridRow.DataItem, "WIPSEQ");
                       

                        inLot.Rows.Add(param);
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
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            
            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                // 변경 대상이 없습니다. 변경 할 LOT을 선택 하세요.
                Util.MessageValidation("SFU1565");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWipNote.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (txtUserName.Tag == null || string.IsNullOrEmpty(txtUserName.Text.Trim()) || string.IsNullOrEmpty(txtUserName.Tag.ToString().Trim()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgOutLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // C20210625-000089 workorder 선택 체크
            if (_util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1441");
                return false;
            }
            

            return true;
        }
        #endregion

        #region [Func]

        #region [요청자]
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
        #endregion

        #region Lot 정보 Clear
        private void SetClearLotInfo()
        {
            txtSelectLot.Text = string.Empty;
            txtSelectWO.Text = string.Empty;
            txtSelectWODetail.Text = string.Empty;
            txtSelectProdid.Text = string.Empty;
            txtSelectModelid.Text = string.Empty;
            txtSelectUnit.Text = string.Empty;
            txtSelectOutQty.Value = 0;
            txtSelectDefectQty.Value = 0;
            txtSelectLossQty.Value = 0;
            txtSelectPrdtReqQty.Value = 0;
            txtLotType.Text = string.Empty;
            txtMarketType.Text = string.Empty;
            txtTopBack.Text = string.Empty;

            _wipseq = 0;
            _processCode = string.Empty;
            _equipmentSegmentCode = string.Empty;
            _equipmentCode = string.Empty;
            _productCode = string.Empty;
            _areaCode = string.Empty;
            _referenceProcessCode = string.Empty;
            _lotTypeCode = string.Empty;

            cboProcess.ItemsSource = null;
            chkProcess.IsChecked = false;

            SetDataGridCheckHeaderInitialize(dgOutLot);
            Util.gridClear(dgWorkOrder);
            Util.gridClear(dgOutLot);
        }
        #endregion

        #region Lot 정보 Setting
        private void SetLotInfo(DataRow dr)
        {
            txtSelectLot.Text = dr["LOTID"].ToString();
            txtSelectWO.Text = dr["WOID"].ToString();
            txtSelectWODetail.Text = dr["WO_DETL_ID"].ToString();
            txtSelectProdid.Text = dr["PRODID"].ToString();
            txtSelectModelid.Text = dr["MODLID"].ToString();
            txtSelectUnit.Text = dr["UNIT_CODE"].ToString();
            txtSelectOutQty.Value = Convert.ToDouble(dr["WIPQTY_ED"].ToString());
            txtSelectDefectQty.Value = Convert.ToDouble(dr["CNFM_DFCT_QTY"].ToString());
            txtSelectLossQty.Value = Convert.ToDouble(dr["CNFM_LOSS_QTY"].ToString());
            txtSelectPrdtReqQty.Value = Convert.ToDouble(dr["CNFM_PRDT_REQ_QTY"].ToString());
            txtTopBack.Text = dr["COATING_SIDE_TYPE_CODE"].ToString();
            txtLotType.Text = dr["LOTTYPE_NAME"].ToString();
            //txtMarketType.Text = dr["MKT_TYPE_CODE"].ToString();
            txtMarketType.Text = dr["MKT_TYPE_CD"].ToString();
            _wipseq = Util.NVC_Int(dr["WIPSEQ"].ToString());
            _areaCode = dr["AREAID"].ToString();
            _processCode = dr["PROCID"].ToString();
            _equipmentSegmentCode = dr["EQSGID"].ToString();
            _equipmentCode = dr["EQPTID"].ToString();
            _productCode = dr["PRODID"].ToString();
            _lotTypeCode = dr["LOTTYPE"].ToString();

            SetEquipmentSegment();

            Util.gridClear(dgWorkOrder);
            Util.gridClear(dgOutLot);
        }
        #endregion

        #region WO 정보 조회
        private void GetWorkOrder()
        {
            Util.gridClear(dgWorkOrder);

            // Process 정보 조회
            GetProcessFactoryPlanInfo();
            // WO Grid 공정별 칼럼 Visibility
            InitializeGridColumns();

            DataTable dtResult;

            if (_planManagementTypeCode.Equals("WO"))  // ERP 실적 전송인 경우는 Workorder Inner Join..
            {
                if (_processCode.Equals(Process.MIXING))
                {
                    dtResult = GetMixingWorkOrder();
                }
                else
                {
                    if (chkProcess.IsChecked.HasValue && (bool)chkProcess.IsChecked)
                        dtResult = GetEquipmentWorkOrderByProcWithInnerJoin();
                    else
                        dtResult = GetEquipmentWorkOrderWithInnerJoin();
                }
            }
            else
            {
                if (chkProcess.IsChecked.HasValue && (bool)chkProcess.IsChecked)
                    dtResult = GetEquipmentWorkOrderByProc();
                else
                    dtResult = GetEquipmentWorkOrder();
            }

            if (dtResult == null || dtResult.Rows.Count < 1)
                return;

            // 현재 선택된 W/O CHK = false로 UPDATE
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

            // 공정 조회인 경우 설비 정보 Visible 처리.
            if (chkProcess.IsChecked.HasValue && (bool)chkProcess.IsChecked)
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            else
                dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

        }

        private void GetOutLot(string lotId)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_EDIT_SUBLOT_LIST_SM";

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = lotId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgOutLot, dtRslt, FrameOperation, true);
                dtlotinfo = dtRslt;
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

        #region WO 공정별 칼럼 Visibility
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
            if (_processCode.Equals(Process.TOP_COATING) ||
                _processCode.Equals(Process.INS_COATING) ||
                _processCode.Equals(Process.HALF_SLITTING) ||
                _processCode.Equals(Process.ROLL_PRESSING) ||
                _processCode.Equals(Process.TAPING) ||
                _processCode.Equals(Process.REWINDER) || 
                _processCode.Equals(Process.LASER_ABLATION) ||
                _processCode.Equals(Process.BACK_WINDER))
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
        #endregion

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

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }



        #endregion

        #endregion


    }
}
