/*************************************************************************************
 Created Date : 2020.11.25
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 근무자 그룹 - 근무자 매핑 신규 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.25  조영대 : Initial Created.
  2021.03.02  조영대 : 조회없이 근무자 조회시 오류 수정.
  2021.03.24  조영대 : EQSGID 인수전달 제거
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_342 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;
        private string INPUT_QTY = string.Empty;
        private string END_QTY = string.Empty;

        Util GridUtil = new Util();

        private CheckBoxHeaderType _HeaderType;
        private enum CheckBoxHeaderType
        {
            Zero,
            One
        }

        private enum ActionMode
        {
            CHECKED_REMOVE,
            CURRENT_ROW_REMOVE,
            WORKER_ADD
        }
        public COM001_342()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            selectedArea = LoginInfo.CFG_AREA_ID;
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = LoginInfo.CFG_PROC_ID;

            InitCombo();

            btnRight.Content = "▲";
            btnLeft.Content = "▼";
        }
        
        private void InitCombo()
        {
            SetComboArea();
            SetComboEquipmentSegmant();
            SetComboUseFlag();
        }

        private void InitGrid()
        {
            Util.gridClear(dgWorkGroup);
            Util.gridClear(dgWorkList);
            Util.gridClear(dgWorkNew);

            _HeaderType = CheckBoxHeaderType.Zero;

            txtWorkerId.Clear();
            txtWorkerName.Clear();
        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboArea.SelectedValue).Equals(string.Empty))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return;
            }

            if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals(string.Empty))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return;
            }

            if (Util.NVC(cboProcess.SelectedValue).Equals(string.Empty))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return;
            }

            InitGrid();

            SearchWorkGroupData();
        }

        private void btnNewUserReg_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void btnSearchWorker_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(txtWorkerId.Text).Equals(string.Empty) && Util.NVC(txtWorkerName.Text).Equals(string.Empty))
            {
                // 입력하신 검색 조건에 맞는 정보가 없습니다. 검색 조건을 확인 하시기 바랍니다.
                Util.MessageValidation("10016");
                return;
            }

            // 2021.03.02  조영대 : 조회없이 근무자 조회시 오류 수정.
            if (dgWorkGroup.GetRowCount().Equals(0) || dgWorkGroup?.ItemsSource == null || dgWorkList?.ItemsSource == null)
            {
                // 선택된 근무자그룹이 없습니다.
                Util.MessageValidation("SFU2049");
                return;
            }

            SearchNotInWorkGroupUserData(string.Empty);
        }

        private void txtWorkerSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (dgWorkGroup.GetRowCount().Equals(0) || dgWorkGroup?.ItemsSource == null || dgWorkList?.ItemsSource == null)
                {
                    // 선택된 근무자그룹이 없습니다.
                    Util.MessageValidation("SFU2049");
                    return;
                }

                if (Util.NVC(txtWorkerId.Text).Equals(string.Empty) && Util.NVC(txtWorkerName.Text).Equals(string.Empty))
                {
                    // 입력하신 검색 조건에 맞는 정보가 없습니다. 검색 조건을 확인 하시기 바랍니다.
                    Util.MessageValidation("10016");
                    return;
                }

                SearchNotInWorkGroupUserData(string.Empty);
            }
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgWorkList;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void cboEquipmentSegmant_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegmant = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    SetComboProcess();
                }
            }));
        }
        
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    SetComboEquipmentSegmant();
                    SetComboProcess();                    
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    selectedProcess = Convert.ToString(cboProcess.SelectedValue);
                    SetComboWorkerGroup();
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }

        private void dgWorkGroup_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgWorkGroup.CurrentRow == null || dgWorkGroup.CurrentRow.Index < 0 ||
                dgWorkGroup.SelectedIndex < 0 || dgWorkGroup.CurrentRow.DataItem == null)
            {
                Util.gridClear(dgWorkList);
                return;
            }

            string workGroupId = Util.NVC(DataTableConverter.GetValue(dgWorkGroup.CurrentRow.DataItem, "WRK_GR_ID"));
            SearchWorkGroupInUserData(workGroupId);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            if (dgWorkGroup.CurrentRow == null || dgWorkGroup.CurrentRow.Index < 0 || 
                dgWorkGroup.SelectedIndex < 0 || dgWorkList.CurrentRow.DataItem == null)
            {
                Util.Alert("SFU2049");  //선택된 근무자그룹이 없습니다.                
                return;
            }

            if (GridUtil.GetDataGridCheckCnt(dgWorkList, "CHK") > 0)
            {
                // 체크된내역만 제거
                SaveData(ActionMode.CHECKED_REMOVE);
            }
            else
            {
                // 현재 로우 제거
                if (dgWorkList.CurrentRow == null || dgWorkList.CurrentRow.Index < 0 ||
                    dgWorkList.SelectedIndex < 0 || dgWorkList.CurrentRow.DataItem == null)
                {
                    Util.Alert("SFU8294");  //선택된 근무자가 없습니다.                
                    return;
                }

                SaveData(ActionMode.CURRENT_ROW_REMOVE);
            }
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (dgWorkGroup.CurrentRow == null || dgWorkGroup.CurrentRow.Index < 0 ||
                dgWorkGroup.SelectedIndex < 0 || dgWorkGroup.CurrentRow.DataItem == null)
            {
                Util.Alert("SFU2049");  //선택된 근무자그룹이 없습니다.                
                return;
            }

            if (dgWorkNew.CurrentRow == null || dgWorkNew.CurrentRow.Index < 0 ||
                dgWorkNew.SelectedIndex < 0 || dgWorkNew.CurrentRow.DataItem == null)
            {
                Util.Alert("SFU8294");  //선택된 근무자가 없습니다.                
                return;
            }

            SaveData(ActionMode.WORKER_ADD);
        }

        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
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

        private void SetComboArea()
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

                new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboArea.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedArea) select dr).Count() > 0)
                    {
                        cboArea.SelectedValue = selectedArea;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cboArea.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cboArea.SelectedItem = null;
                    }
                    cboArea_SelectedItemChanged(cboArea, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboEquipmentSegmant()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = selectedArea;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    
                    cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                    {
                        cboEquipmentSegment.SelectedValue = selectedEquipmentSegmant;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cboEquipmentSegment.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cboEquipmentSegment.SelectedItem = null;
                    }
                    cboEquipmentSegmant_SelectedItemChanged(cboEquipmentSegment, null);
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboProcess()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = selectedEquipmentSegmant;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    
                    cboProcess.ItemsSource = DataTableConverter.Convert(result);

                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedProcess) select dr).Count() > 0)
                    {
                        cboProcess.SelectedValue = selectedProcess;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cboProcess.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cboProcess.SelectedItem = null;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboWorkerGroup()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_WRK_GR_CBO_BY_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataRow dRow = result.NewRow();
                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cboWorkGroup.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count > 0)
                    {
                        cboWorkGroup.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cboWorkGroup.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboUseFlag()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "USE_FLAG";
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        cboUseFlag.ItemsSource = DataTableConverter.Convert(result);
                        cboUseFlag.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cboUseFlag.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchWorkGroupData()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgWorkGroup);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WRK_GR_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WRK_GR_ID"] = Util.NVC(cboWorkGroup.SelectedValue).Equals(string.Empty) ? null : Util.NVC(cboWorkGroup.SelectedValue);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_AREA_PROC_GR", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgWorkGroup, result, FrameOperation, false);

                    Util.gridSetFocusRow(ref dgWorkGroup, 0);

                });
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

        private void SearchWorkGroupInUserData(string userId)
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgWorkList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WRK_GR_ID", typeof(string));
                IndataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WRK_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkGroup.CurrentRow.DataItem, "WRK_GR_ID"));
                Indata["USE_FLAG"] = Util.NVC(cboUseFlag.SelectedValue);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_MMD_AREA_PROC_GR_USER_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgWorkList, result, FrameOperation, false);

                    if (!userId.Equals(string.Empty))
                    {
                        Util.gridFindDataRow(ref dgWorkList, "USERID", userId, true);
                    }
                    else if (result != null && result.Rows.Count > 0)
                    {
                        Util.gridSetFocusRow(ref dgWorkList, 0);
                    }
                });
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

        private void SearchNotInWorkGroupUserData(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorkerId.Text.Trim()) &&
                    string.IsNullOrEmpty(txtWorkerName.Text.Trim())) return;

                ShowLoadingIndicator();

                Util.gridClear(dgWorkNew);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WRK_GR_ID", typeof(string));
                IndataTable.Columns.Add("USER_ID", typeof(string));
                IndataTable.Columns.Add("USER_NAME", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WRK_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkGroup.CurrentRow.DataItem, "WRK_GR_ID"));
                Indata["USER_ID"] = Util.NVC(txtWorkerId.Text).Equals(string.Empty) ? null : Util.NVC(txtWorkerId.Text);
                Indata["USER_NAME"] = Util.NVC(txtWorkerName.Text).Equals(string.Empty) ? null : Util.NVC(txtWorkerName.Text);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PERSON_EXCEPT_PROC_GR_USER_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgWorkNew, result, FrameOperation, false);

                    if (!userId.Equals(string.Empty))
                    {
                        int findRow = Util.gridFindDataRow(ref dgWorkNew, "USERID", userId, true);
                        if (findRow > -1)
                        {
                            dgWorkNew.ScrollIntoView(findRow, 0);
                            dgWorkNew.CurrentRow = dgWorkNew.Rows[findRow];
                            dgWorkNew.SelectedIndex = findRow;
                        }
                    }
                });
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

        private void SaveData(ActionMode actionMode)
        {
            try
            {

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("UPDMODE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                //2021.03.24  조영대 : EQSGID 인수전달 제거
                //inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WRK_GR_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                // 입력 및 수정 데이터 수집
                DataTable dt = (dgWorkGroup.ItemsSource as DataView).Table;
                switch (actionMode)
                {
                    case ActionMode.CHECKED_REMOVE:
                        {
                            List<DataRow> selectList = DataTableConverter.Convert(dgWorkList.ItemsSource).AsEnumerable()
                                //.Where(f => f["CHK"].Equals(1)).ToList();
                                .Where(f => f["CHK"].ToString().Equals("1")).ToList(); // 2024.11.05. 김영국 - CHK Data COlumn의 DataType이 long으로 올라오는 문제점 발생하여 ToString으로 비교.
                            foreach (DataRow dr in selectList)
                            {
                                DataRow newRow = inTable.NewRow();
                                newRow["UPDMODE"] = "CHECKED_REMOVE";
                                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                newRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                                //2021.03.24  조영대 : EQSGID 인수전달 제거
                                //newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                                newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                                newRow["WRK_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkGroup.CurrentRow.DataItem, "WRK_GR_ID"));
                                newRow["USERID"] = Util.NVC(dr["USERID"]);
                                newRow["USE_FLAG"] = "N";
                                newRow["INSUSER"] = LoginInfo.USERID;
                                newRow["UPDUSER"] = LoginInfo.USERID;
                                inTable.Rows.Add(newRow);
                            }
                        }
                        break;
                    case ActionMode.CURRENT_ROW_REMOVE:
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["UPDMODE"] = "CURRENT_ROW_REMOVE";
                            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            newRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                            //2021.03.24  조영대 : EQSGID 인수전달 제거
                            //newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                            newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                            newRow["WRK_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkGroup.CurrentRow.DataItem, "WRK_GR_ID"));
                            newRow["USERID"] = Util.NVC(DataTableConverter.GetValue(dgWorkList.CurrentRow.DataItem, "USERID"));
                            newRow["USE_FLAG"] = "N";
                            newRow["INSUSER"] = LoginInfo.USERID;
                            newRow["UPDUSER"] = LoginInfo.USERID;
                            inTable.Rows.Add(newRow);
                        }
                        break;
                    case ActionMode.WORKER_ADD:
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["UPDMODE"] = "WORKER_ADD";
                            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            newRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                            //2021.03.24  조영대 : EQSGID 인수전달 제거
                            //newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                            newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                            newRow["WRK_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkGroup.CurrentRow.DataItem, "WRK_GR_ID"));
                            newRow["USERID"] = Util.NVC(DataTableConverter.GetValue(dgWorkNew.CurrentRow.DataItem, "USERID"));
                            newRow["USE_FLAG"] = "Y";
                            newRow["INSUSER"] = LoginInfo.USERID;
                            newRow["UPDUSER"] = LoginInfo.USERID;
                            inTable.Rows.Add(newRow);
                        }
                        break;
                    default:
                        break;
                }
                
                new ClientProxy().ExecuteService("BR_PRD_UPD_TB_MMD_AREA_PROC_GR_USER_DRB", "INDATA", "OUTDATA", inTable, (result, ex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        if (result != null && result.Rows.Count > 0)
                        {
                            SearchWorkGroupInUserData(Util.NVC(result.Rows[0]["USERID"]));
                            SearchNotInWorkGroupUserData(Util.NVC(result.Rows[0]["USERID"]));
                        }
                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });

                

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }            
        }





        #endregion

    
    }
}
