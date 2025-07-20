/*************************************************************************************
 Created Date : 2024.11.05
      Creator : 조영대
   Decription : Ultium Cells GMES 구축 Proj. - 원자재보충요청(원자재창고->STO)
--------------------------------------------------------------------------------------
 [Change History]
  2024.11.05  조영대 : Initial Created
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_219 : UserControl, IWorkArea
    {
        #region [Declaration]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string selectedVersionCheckFlag = string.Empty;
        #endregion

        #region [Initialize]
        public MTRL001_219()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            SetComboBox_tab1();
            InitializeControls_tab1();

            SetComboBox_tab2();
            InitializeControls_tab2();

            object[] parameters = this.FrameOperation.Parameters;
        }

        #region [요청 탭]

        private void SetComboBox_tab1()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            CommonCombo combo = new CommonCombo();

            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            string[] FilterExcept = new string[] { "E2000" };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sFilter: FilterExcept);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            cboEquipment.SelectedIndex = 0;
        }

        private void InitializeControls_tab1()
        {
            SetDate(dtpDateFrom, dtpDateTo);
            string preFix = "▶▶▶ ";
            btnRequest.Content = preFix + Util.NVC(btnRequest.Content);
        }
        #endregion

        #region [요청이력 탭]

        private void SetComboBox_tab2()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            CommonCombo combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment2, cboProcess2, cboEquipment2 };
            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea2 };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess2, cboEquipment2 };
            combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment2 };
            C1ComboBox[] cboProcessChild = { cboEquipment2 };
            string[] FilterExcept = new string[] { "E2000" };
            combo.SetCombo(cboProcess2, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS", sFilter: FilterExcept);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment2, cboProcess2 };
            combo.SetCombo(cboEquipment2, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "EQUIPMENT");

            cboEquipment2.SelectedIndex = 0;

            //요청서 상태
            string[] FilterStatus = new string[] { "MTRL_SPLY_REQ_STAT_CODE", "", "", "A,R" };
            combo.SetCombo(cboStatus2, CommonCombo.ComboStatus.ALL, sFilter: FilterStatus, sCase: "COMMCODEATTRSMULTI");
        }

        private void InitializeControls_tab2()
        {
            SetDate(dtpDateFrom2, dtpDateTo2);

            btnCancel.Content = "▶▶▶ " + Util.NVC(btnCancel.Content);
        }
        #endregion

        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            Initialize();

            this.Loaded -= UserControl_Loaded;
            if (LoginInfo.CFG_AREA_ID == "MB" && LoginInfo.CFG_PROC_ID == "A3000")
                dgInputMaterial.LoadedCellPresenter += DgInputMaterial_LoadedCellPresenter;
        }

        #region [요청 탭]

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            if (e.NewValue != null && e.OldValue != null && !e.NewValue.Equals(e.OldValue))
            {
                Refresh();
            }
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                int rowIndex = ((DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

                if (selectedVersionCheckFlag.Equals("Y"))
                {
                    DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();
                    if (selectWO != null)
                    {
                        string Version = Util.NVC(selectWO["PROD_VER_CODE"]);

                        if (Version.Equals(string.Empty))
                        {
                            Util.gridClear(dgInputMaterial);
                            Util.gridClear(dgRequestMtrlList);
                            Util.MessageValidation("SFU5036");
                            return;
                        }
                    }
                }

                dgWorkOrder.SelectedIndex = rowIndex;

                SearchInputMaterial();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgInputMaterial_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgInputMaterial.Rows.Count == 0)
                return;

            btnRight_Click(sender, null);
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow(txtWorker);
        }

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow(txtWorker);
            }
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = dgInputMaterial.SelectedIndex;

                if (rowIndex < 0)
                    return;

                DataView preView = dgInputMaterial.ItemsSource as DataView;
                DataTable preTable = preView.ToTable().Copy();
                if (!preTable.Columns.Contains("MTRL_QTY"))
                {
                    preTable.Columns.Add("MTRL_QTY");
                }

                if (!preTable.Columns.Contains("SUPPLIER"))
                {
                    preTable.Columns.Add("SUPPLIER");

                }

                if (!preTable.Columns.Contains("CHK"))
                {
                    preTable.Columns.Add("CHK");
                }
                DataRow preRow = preTable.Rows[rowIndex];

                DataView aftView = dgRequestMtrlList.ItemsSource as DataView;
                DataTable aftTable;
                if (aftView == null)
                {
                    aftTable = preTable.Clone();
                }
                else
                {
                    aftTable = aftView.ToTable().Copy();
                }

                aftTable.ImportRow(preRow);
                preTable.Rows.Remove(preRow);

                if (preTable.Columns.Contains("MTRL_QTY"))
                {
                    preTable.Columns.Remove("MTRL_QTY");
                }
                if (preTable.Columns.Contains("SUPPLIER"))
                {
                    preTable.Columns.Remove("SUPPLIER");

                }
                if (preTable.Columns.Contains("CHK"))
                {
                    preTable.Columns.Remove("CHK");
                }

                Util.GridSetData(dgRequestMtrlList, aftTable, FrameOperation, true);
                Util.GridSetData(dgInputMaterial, preTable, FrameOperation, true);
                Util.gridSetFocusRow(ref dgRequestMtrlList, dgRequestMtrlList.Rows.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnAllRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgInputMaterial.ItemsSource == null) return;

                dgRequestMtrlList.ClearRows();
                DataView preView = dgInputMaterial.ItemsSource as DataView;
                DataTable preTable = preView.ToTable().Copy();

                if (preTable.Rows.Count == 0)
                    return;

                if (!preTable.Columns.Contains("MTRL_QTY"))
                {
                    preTable.Columns.Add("MTRL_QTY");
                }

                if (!preTable.Columns.Contains("SUPPLIER"))
                {
                    preTable.Columns.Add("SUPPLIER");

                }

                if (!preTable.Columns.Contains("CHK"))
                {
                    preTable.Columns.Add("CHK");
                }


                DataView aftView = dgRequestMtrlList.ItemsSource as DataView;
                DataTable aftTable;
                if (aftView == null || aftView.Count == 0) // 2024.10.07 김영국 - Data를 여러번 넘길 경우 기존에 담겨있는 Column정보를 가지고 있는 문제가 있어 해당 부분 로직 수정함.
                {
                    aftTable = preTable.Clone();
                }
                else
                {
                    aftTable = aftView.ToTable().Copy();
                }

                int preCnt = preTable.Rows.Count;

                for (int i = 0; i < preCnt; i++)
                {
                    aftTable.ImportRow(preTable.Rows[0]);
                    preTable.Rows.Remove(preTable.Rows[0]);
                }


                if (preTable.Columns.Contains("MTRL_QTY"))
                {
                    preTable.Columns.Remove("MTRL_QTY");
                }
                if (preTable.Columns.Contains("SUPPLIER"))
                {
                    preTable.Columns.Remove("SUPPLIER");

                }
                if (preTable.Columns.Contains("CHK"))
                {
                    preTable.Columns.Remove("CHK");
                }

                Util.GridSetData(dgRequestMtrlList, aftTable, FrameOperation, true);
                Util.GridSetData(dgInputMaterial, preTable, FrameOperation, true);
                Util.gridSetFocusRow(ref dgRequestMtrlList, dgRequestMtrlList.Rows.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = dgRequestMtrlList.SelectedIndex;

                if (rowIndex < 0)
                    return;

                DataView preView = dgRequestMtrlList.ItemsSource as DataView;
                DataTable preTable = preView.ToTable().Copy();
                DataRow preRow = preTable.Rows[rowIndex];
                DataTable preTable2 = preView.ToTable().Copy();
                if (preTable2.Columns.Contains("MTRL_QTY"))
                {
                    preTable2.Columns.Remove("MTRL_QTY");
                }

                if (preTable2.Columns.Contains("SUPPLIER"))
                {
                    preTable2.Columns.Remove("SUPPLIER");
                }

                if (preTable2.Columns.Contains("CHK"))
                {
                    preTable2.Columns.Remove("CHK");
                }

                DataRow preRow2 = preTable2.Rows[rowIndex];

                DataView aftView = dgInputMaterial.ItemsSource as DataView;
                DataTable aftTable;
                if (aftView == null)
                {
                    aftTable = preTable2.Clone();
                }
                else
                {
                    aftTable = aftView.ToTable().Copy();
                }

                preTable.Rows.Remove(preRow);
                aftTable.ImportRow(preRow2);

                Util.GridSetData(dgInputMaterial, aftTable, FrameOperation, true);
                Util.GridSetData(dgRequestMtrlList, preTable, FrameOperation, true);
                Util.gridSetFocusRow(ref dgInputMaterial, dgInputMaterial.Rows.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAllLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgInputMaterial.ItemsSource == null) return;

                DataView preView = dgRequestMtrlList.ItemsSource as DataView;
                DataTable preTable = preView.ToTable().Copy();

                if (preTable.Rows.Count == 0)
                    return;

                if (preTable.Columns.Contains("MTRL_QTY"))
                {
                    preTable.Columns.Remove("MTRL_QTY");
                }

                if (preTable.Columns.Contains("SUPPLIER"))
                {
                    preTable.Columns.Remove("SUPPLIER");
                }

                if (preTable.Columns.Contains("CHK"))
                {
                    preTable.Columns.Remove("CHK");
                }


                DataView aftView = dgInputMaterial.ItemsSource as DataView;
                DataTable aftTable;
                if (aftView == null)
                {
                    aftTable = preTable.Clone();
                }
                else
                {
                    aftTable = aftView.ToTable().Copy();
                }

                int preCnt = preTable.Rows.Count;

                for (int i = 0; i < preCnt; i++)
                {
                    aftTable.ImportRow(preTable.Rows[0]);
                    preTable.Rows.Remove(preTable.Rows[0]);
                }

                Util.GridSetData(dgInputMaterial, aftTable, FrameOperation, true);
                Util.GridSetData(dgRequestMtrlList, preTable, FrameOperation, true);
                Util.gridSetFocusRow(ref dgInputMaterial, dgInputMaterial.Rows.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime > dtpDateTo.SelectedDateTime)
            {
                dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime;
            }

            SearchWorkOrder();
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateTo.SelectedDateTime < dtpDateFrom.SelectedDateTime)
            {
                dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            }

            SearchWorkOrder();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchWorkOrder();
        }

        private void dgRequestMtrlList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRequestMtrlList.Rows.Count == 0)
                return;

            if (dgRequestMtrlList.CurrentColumn.Name.Equals("MTRL_QTY") || dgRequestMtrlList.CurrentColumn.Name.Equals("SUPPLIER") || dgRequestMtrlList.CurrentColumn.Name.Equals("CHK"))
                return;

            btnLeft_Click(sender, null);
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            SaveInputRequest();
        }
        #endregion

        #region [요청이력 탭]

        private void cboArea2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl2();
        }

        private void cboEquipmentSegment2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl2();
        }

        private void cboProcess2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl2();
        }

        private void cboEquipment2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl2();
        }

        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom2.SelectedDateTime > dtpDateTo2.SelectedDateTime)
            {
                dtpDateTo2.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
            }
            if (dtpDateTo2.SelectedDateTime.Subtract(dtpDateFrom2.SelectedDateTime).TotalDays > 31)
            {
                //기간은 %1일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                dtpDateTo2.SelectedDateTime = dtpDateFrom2.SelectedDateTime.AddDays(31);
            }
        }

        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateTo2.SelectedDateTime < dtpDateFrom2.SelectedDateTime)
            {
                dtpDateFrom2.SelectedDateTime = dtpDateTo2.SelectedDateTime;
            }
            if (dtpDateTo2.SelectedDateTime.Subtract(dtpDateFrom2.SelectedDateTime).TotalDays > 31)
            {
                //기간은 %1일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                dtpDateFrom2.SelectedDateTime = dtpDateTo2.SelectedDateTime.AddDays(-31);
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            SearchReqHist();
        }

        private void dgReqChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            //if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").ToString().Equals("0")) // 2024.10.10. 김영국 - CHK값이 DB에서 넘어올때 long으로 넘어옴. String 비교로 변경함.
            {
                if (rb.DataContext == null)
                    return;

                int rowIndex = ((DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

                foreach (C1.WPF.DataGrid.DataGridRow row in ((DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);

                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                SearchInputMtrlHist(dgReqList.GetValue(rowIndex, "MTRL_SPLY_REQ_ID").ToString());
            }
        }

        private void txtWorker2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow(txtWorker2);
            }
        }

        private void btnReqUser2_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow(txtWorker2);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelInputRequest();
        }
        #endregion

        #endregion

        #region [Method]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnRequest,
                btnCancel
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

        private void SetDate(LGCDatePicker dtpFrom, LGCDatePicker dtpTo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    dtpFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                    dtpTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    return;
                }

                dtpTo.SelectedDateTime = DateTime.Parse(result.Rows[0]["DATETO"].ToString());
                dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime.AddDays(-7);
                
            }
            );
        }

        private void GetUserWindow(TextBox tb)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = tb.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                TextBox tb1 = (tabMain.SelectedItem as C1TabItem == ctbRequest) ? txtWorker : txtWorker2;
                TextBox tb2 = (tabMain.SelectedItem as C1TabItem == ctbRequest) ? txtPersonId : txtPersonId2;

                tb1.Text = wndPerson.USERNAME;
                tb1.Tag = wndPerson.USERID;
                tb2.Text = wndPerson.USERID;
            }
        }

        #region [요청 탭]

        private void GetVersionCheckFlag()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQUIPID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQUIPID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_VER_CHK_FLAG", "INDATA", "OUTDATA", IndataTable);
                if (dt.Rows.Count > 0)
                {
                    selectedVersionCheckFlag = dt.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearControl()
        {
            Util.gridClear(dgWorkOrder);
            Util.gridClear(dgInputMaterial);
            Util.gridClear(dgRequestMtrlList);

            this.txtRemark.Clear();
            this.txtWorker.Clear();
            this.txtWorker.Tag = string.Empty;
            this.txtPersonId.Clear();
        }

        private void Refresh()
        {
            SearchWorkOrder();
            GetVersionCheckFlag();
        }

        private void SearchWorkOrder()
        {
            try
            {
                if (Util.IsNVC(cboEquipmentSegment.SelectedValue) ||
                    Util.IsNVC(cboProcess.SelectedValue) ||
                    Util.IsNVC(cboEquipment.SelectedValue)) return;

                ShowLoadingIndicator();

                Util.gridClear(dgWorkOrder);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_MTRL_REL", "RQSTDT", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                if (IsAreaCommonCodeUse("SUPPLIER_SPECIAL_CHK_VIEW", Util.NVC(cboProcess.SelectedValue)))
                {
                    CommonCombo.SetDataGridComboItem("DA_PRD_SEL_EQPT_MOUNT_PSTN_SUPPLIER_NJ", new string[] { "LANGID", "EQPTID" }, new string[] { LoginInfo.LANGID, Util.NVC(cboEquipment.SelectedValue) as string }, CommonCombo.ComboStatus.NONE, dgRequestMtrlList.Columns["SUPPLIER"], "SUPPLIER_NAME", "SUPPLIERID");
                }
                Util.GridSetData(dgWorkOrder, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SearchInputMaterial()
        {
            try
            {
                dgInputMaterial.ClearRows();
                Util.gridClear(dgRequestMtrlList);

                DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();
                if (selectWO == null)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WOID"] = Util.NVC(selectWO["WOID"]);
                Indata["PRODID"] = Util.NVC(selectWO["PRODID"]);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_MTR_GET_MTRL_INFO_WITH_FP", "INDATA", "OUTDATA", IndataTable);
                if (dtMain.Rows.Count == 0)
                {
                    Util.gridClear(dgInputMaterial);
                    Util.gridClear(dgRequestMtrlList);
                    return;
                }
                Util.GridSetData(dgInputMaterial, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool CheckSaveValidation()
        {
            if (dgRequestMtrlList.Rows.Count < 1 || dgRequestMtrlList.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                return false;
            }

            for (int i = 0; i < dgRequestMtrlList.Rows.Count; i++)
            {
                if (LoginInfo.CFG_AREA_ID != "MB")
                {
                    if (string.IsNullOrEmpty(Util.NVC(dgRequestMtrlList.GetValue(i, "MTRL_QTY"))) || Util.NVC(dgRequestMtrlList.GetValue(i, "MTRL_QTY")).Equals("0"))
                    {
                        //투입요청수량을 입력 하세요.
                        Util.MessageValidation("SFU1978");
                        return false;
                    }

                    if (Util.NVC_Decimal(dgRequestMtrlList.GetValue(i, "MTRL_QTY")) < 0)
                    {
                        //양수를 입력하세요.
                        Util.MessageValidation("SFU4209");
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Util.NVC(dgRequestMtrlList.GetValue(i, "MTRL_QTY"))))
                    {
                        //투입요청수량을 입력 하세요.
                        Util.MessageValidation("SFU1978");
                        return false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(txtWorker.Text) || string.IsNullOrWhiteSpace((string)txtWorker.Tag))
            {
                // 요청자를 선택해 주세요.
                Util.MessageValidation("SFU3467");
                return false;
            }

            DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();
            if (selectWO == null)
            {
                Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                return false;
            }

            return true;
        }

        private void SaveInputRequest()
        {
            try
            {
                dgRequestMtrlList.EndEdit();

                if (!CheckSaveValidation()) return;

                DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_USERID", typeof(string));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EMRG_REQ_FLAG", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));
                inMtrl.Columns.Add("UNIT_CODE", typeof(string));
                inMtrl.Columns.Add("SUPPLIERID", typeof(string));
                inMtrl.Columns.Add("SPECIAL", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Request;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["MTRL_SPLY_REQ_USERID"] = (string)txtWorker.Tag;
                newRow["WO_DETL_ID"] = Util.NVC(selectWO["WOID"]);
                newRow["NOTE"] = Util.NVC(txtRemark.Text.ToString());
                newRow["USERID"] = LoginInfo.USERID;
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = Mtrl_Request_TypeCode.Request;
                newRow["EMRG_REQ_FLAG"] = "N";
                newRow["MKT_TYPE_CODE"] = Util.NVC(selectWO["MKT_TYPE_CODE"]);
                newRow["ELTR_TYPE_CODE"] = Util.NVC(selectWO["ELTR_TYPE_CODE"]);
                inTable.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgRequestMtrlList.Rows)
                {
                    newRow = inMtrl.NewRow();
                    newRow["MTRLID"] = DataTableConverter.GetValue(row.DataItem, "MTRLID");
                    newRow["MTRL_SPLY_REQ_QTY"] = DataTableConverter.GetValue(row.DataItem, "MTRL_QTY");
                    newRow["UNIT_CODE"] = DataTableConverter.GetValue(row.DataItem, "UNIT_CODE");
                    if (LoginInfo.CFG_AREA_ID.Equals("MB"))
                    {
                        if (DataTableConverter.GetValue(row.DataItem, "SUPPLIER") != null)
                            newRow["SUPPLIERID"] = DataTableConverter.GetValue(row.DataItem, "SUPPLIER");
                        if (DataTableConverter.GetValue(row.DataItem, "CHK") == null || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("false"))
                            newRow["SPECIAL"] = "N";
                        else
                            newRow["SPECIAL"] = "Y";
                    }

                    inMtrl.Rows.Add(newRow);
                }

                //출고요청 하시겠습니까?
                Util.MessageConfirm("SFU2086", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_REL_REQUEST", "INDATA,INMTRL", "OUTDATA,OUT_REQID", (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                DataTable result = bizResult.Tables["OUTDATA"];

                                if (result.Rows.Count != 0 && result.Rows[0]["PROC_FLAG"].ToString().Equals("Y"))
                                {
                                    // 요청되었습니다.
                                    Util.MessageInfo("SFU1747");

                                    this.txtRemark.Clear();
                                    this.txtWorker.Clear();
                                    this.txtWorker.Tag = string.Empty;
                                    this.txtPersonId.Clear();

                                    // 재조회
                                    SearchInputMaterial();
                                }
                                else if (result.Rows.Count != 0 && result.Rows[0]["PROC_FLAG"].ToString().Equals("N"))
                                {
                                    Util.MessageValidation(result.Rows[0]["ERR_MSG"].ToString());
                                }
                                else
                                {
                                    // 요청 실패하였습니다.
                                    Util.MessageValidation("FM_ME_0185");
                                }
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region [요청이력 탭]
        private void ClearControl2()
        {
            Util.gridClear(dgReqList);
            Util.gridClear(dgMtrlList);

            this.txtRemark2.Clear();
            this.txtWorker2.Clear();
            this.txtWorker2.Tag = string.Empty;
            this.txtPersonId2.Clear();
        }

        private void SearchReqHist()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgReqList);
                Util.gridClear(dgMtrlList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = string.IsNullOrEmpty(Util.NVC(cboArea2.SelectedValue)) ? DBNull.Value : cboArea2.SelectedValue;
                Indata["EQSGID"] = string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment2.SelectedValue)) ? DBNull.Value : cboEquipmentSegment2.SelectedValue;
                Indata["PROCID"] = string.IsNullOrEmpty(Util.NVC(cboProcess2.SelectedValue)) ? DBNull.Value : cboProcess2.SelectedValue;
                Indata["EQPTID"] = string.IsNullOrEmpty(Util.NVC(cboEquipment2.SelectedValue)) ? DBNull.Value : cboEquipment2.SelectedValue;
                Indata["MTRL_SPLY_REQ_STAT_CODE"] = string.IsNullOrEmpty(Util.NVC(cboStatus2.SelectedValue)) ? DBNull.Value : cboStatus2.SelectedValue;
                Indata["STDT"] = dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EDDT"] = dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd");
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MTR_SEL_MATERIAL_REQUEST_HIST_ELEC", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgReqList, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SearchInputMtrlHist(string sReqId)
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgMtrlList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MTRL_SPLY_REQ_ID"] = sReqId;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MTR_SEL_MATERIAL_REQUEST_SPLY_HIST_ELEC", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgMtrlList, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CheckCancelValidation()
        {
            if (dgReqList.ItemsSource == null || Util.gridGetChecked(ref dgReqList, "CHK").Length == 0) // 2024.10.11. 김영국 - 선택없이 저장 시 해당 MESSAGE가 보여지도록 추가. 
            {
                Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                return false;
            }

            DataRow selectReq = Util.gridGetChecked(ref dgReqList, "CHK").First();

            if (selectReq == null)
            {
                Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                return false;
            }

            if (selectReq["MTRL_SPLY_REQ_STAT_NAME"].ToString() == ObjectDic.Instance.GetObjectName("요청취소")) //2024.10.15. 김영국 - 요청취소된 요청서를 재 취소시 로직 처리하도록 추가.
            {
                Util.MessageValidation("SFU9936");  // 이미 취소된 요청서 입니다..        
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWorker2.Text) || string.IsNullOrWhiteSpace((string)txtWorker2.Tag))
            {
                // 요청자를 선택해 주세요.
                Util.MessageValidation("SFU3467");
                return false;
            }

            return true;
        }

        private void CancelInputRequest()
        {
            try
            {
                if (!CheckCancelValidation()) return;

                DataRow selectReq = Util.gridGetChecked(ref dgReqList, "CHK").First();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_USERID", typeof(string));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EMRG_REQ_FLAG", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataTable inMtrl = inDataSet.Tables.Add("INMTRL");
                inMtrl.Columns.Add("MTRLID", typeof(string));
                inMtrl.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["MTRL_SPLY_REQ_ID"] = Util.NVC(selectReq["MTRL_SPLY_REQ_ID"]);
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = Mtrl_Request_StatCode.Cancel;
                newRow["MTRL_SPLY_REQ_USERID"] = (string)txtWorker2.Tag;
                newRow["NOTE"] = Util.NVC(txtRemark2.Text.ToString());
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                //취소하시겠습니까?
                Util.MessageConfirm("SFU4616", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_MTR_REG_MATERIAL_REL_REQUEST", "INDATA,INMTRL", "OUTDATA,OUT_REQID", (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                DataTable result = bizResult.Tables["OUTDATA"];

                                if (result.Rows.Count != 0 && result.Rows[0]["PROC_FLAG"].ToString().Equals("Y"))
                                {
                                    // 취소되었습니다.
                                    Util.MessageInfo("SFU1937");

                                    this.txtRemark2.Clear();
                                    this.txtWorker2.Clear();
                                    this.txtWorker2.Tag = string.Empty;
                                    this.txtPersonId2.Clear();

                                    // 재조회
                                    SearchReqHist();
                                }
                                else if (result.Rows.Count != 0 && result.Rows[0]["PROC_FLAG"].ToString().Equals("N"))
                                {
                                    Util.MessageValidation(result.Rows[0]["ERR_MSG"].ToString());
                                }
                                else
                                {
                                    // 요청 실패하였습니다.
                                    Util.MessageValidation("FM_ME_0185");
                                }
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }



        #endregion

        #endregion

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = sCodeType;
                newRow["COM_CODE"] = sCodeName;
                inTable.Rows.Add(newRow);



                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["ATTR1"].ToString().Equals("Y") && dtResult.Rows[0]["ATTR2"].ToString().Equals("Y"))
                    {
                        dgRequestMtrlList.Columns["SUPPLIER"].Visibility = Visibility.Visible;
                        dgRequestMtrlList.Columns["CHK"].Visibility = Visibility.Visible;
                        return true;
                    }
                    else if (dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                    {
                        dgRequestMtrlList.Columns["SUPPLIER"].Visibility = Visibility.Visible;
                        dgRequestMtrlList.Columns["CHK"].Visibility = Visibility.Collapsed;
                        return true;
                    }
                    else if (dtResult.Rows[0]["ATTR2"].ToString().Equals("Y"))
                    {
                        dgRequestMtrlList.Columns["CHK"].Visibility = Visibility.Visible;
                        dgRequestMtrlList.Columns["SUPPLIER"].Visibility = Visibility.Collapsed;
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return false;
        }



        private void DgInputMaterial_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    Console.WriteLine(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID")) + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID")).Contains("MCT"));
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID")).Contains("MCT") || Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID")).Contains("MCB"))
                    {

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    }

                }
            }));
        }
    }
}
