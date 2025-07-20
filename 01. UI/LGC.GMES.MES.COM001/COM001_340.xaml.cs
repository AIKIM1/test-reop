/*************************************************************************************
 Created Date : 2020.11.11
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 생산계획 신규 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.11  조영대 : Initial Created.(COM001_002.xaml Copy 후 적용) 4
  2023.07.05  유재홍 : 작업자 PC에서 조회 가능하도록 수정
  2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경
  2024.05.03  김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
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
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_340 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;
        private string INPUT_QTY = string.Empty;
        private string END_QTY = string.Empty;
        private object[] paramList = null;

        public COM001_340()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
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

            // 2024.11.11. 김영국 - 조립이면 FP, 등록 버튼 비활성화.
            if(LoginInfo.CFG_SYSTEM_TYPE_CODE == "A")
            {
                btnFp.Visibility = Visibility.Collapsed;
                btnLoad.Visibility = Visibility.Collapsed;
            }
            else if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "E")
            {
                btnFp.Visibility = Visibility.Collapsed; // 전극일 경우는 FP버튼 비활성화.
            }
            
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = LoginInfo.CFG_PROC_ID;
            InitCombo();
            rdoEquipment.IsChecked = true;
            rdoSRoll.IsChecked = true;
            btnSave.IsEnabled = false;
            cboEquipment2.IsEnabled = false;

            if (!LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals(Area_Type.ELEC))
            {
                lblRollMandMark.Visibility = Visibility.Collapsed;
                lblRollType.Visibility = Visibility.Collapsed;
                rdoCRoll.Visibility = Visibility.Collapsed;
                rdoSRoll.Visibility = Visibility.Collapsed;
            }

            if (selectedArea.Equals("E4")&& string.Equals(selectedProcess, Process.MIXING))
            {
                ResultDetail.Visibility = Visibility.Visible;
                roweqpt.Height = new GridLength(35);
            }
            else
            {
                ResultDetail.Visibility = Visibility.Collapsed;
                Grid.SetRowSpan(dgListMaterial, 3);
                roweqpt.Height = new GridLength(0);
            }


            // 2023.10.09 강성묵 E20230927-000880 전극 Nickname 표기 변경
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "E")
            {
                dgcPjt.Visibility = Visibility.Collapsed;
                dgcModlid.Visibility = Visibility.Collapsed;
                dgcPjtNew.Visibility = Visibility.Visible;
                dgcModlidNew.Visibility = Visibility.Visible;
            }
            else
            {
                dgcPjt.Visibility = Visibility.Visible;
                dgcModlid.Visibility = Visibility.Visible;
                dgcPjtNew.Visibility = Visibility.Collapsed;
                dgcModlidNew.Visibility = Visibility.Collapsed;
            }
        }
        
        private void InitCombo()
        {
            dtpFrom.SelectedDateTime = System.DateTime.Now;
            dtpTo.SelectedDateTime = System.DateTime.Now;

            Set_Combo_Area(cboArea);
            Set_Combo_EquipmentSegmant(cboEquipmentSegment);
            Set_Combo_COMMCODE(cboStatus);
            Set_Combo_COMMCODE(cboWOStatus);
            Set_Combo_COMMCODE(cboDEMAND_TYPE);
            Set_Combo_COMMCODE(cboPlanCnfm);
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgListMaterial);
            Util.gridClear(dgListVersion);

            txtMtrlWO.Clear();
            txtMtrlPrj.Clear();
            txtMtrlProd.Clear();
        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            COM001_001_LOAD _xlsLoad = new COM001_001_LOAD();
            _xlsLoad.FrameOperation = FrameOperation;

            if (_xlsLoad != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = Util.NVC(cboArea.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Util.NVC(cboProcess.SelectedValue);

                C1WindowExtension.SetParameters(_xlsLoad, Parameters);

                _xlsLoad.Closed += new EventHandler(xlsLoad_Closed);
                _xlsLoad.ShowModal();
                _xlsLoad.CenterOnScreen();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.Rows.Count == 0)
            {
                Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
                return;
            }
            if (dgList.CurrentRow.DataItem == null)
                return;

            DataRowView _dRow = dgList.CurrentRow.DataItem as DataRowView;

            SaveData(_dRow["WOID"].ToString());
        }

        private void cboEquipmentSegmant_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegmant = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    Set_Combo_Process(cboProcess);
                    Set_Combo_Equipment(cboEquipment);
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
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_Process(cboProcess);

                    if (selectedArea.Equals("E4") && string.Equals(selectedProcess, Process.MIXING))
                    {
                        ResultDetail.Visibility = Visibility.Visible;
                        roweqpt.Height = new GridLength(35);
                    }
                    else
                    {
                        ResultDetail.Visibility = Visibility.Collapsed;
                        Grid.SetRowSpan(dgListMaterial, 3);
                        roweqpt.Height = new GridLength(0);
                    }
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
                    Set_Combo_Equipment(cboEquipment);

                    if (selectedArea.Equals("E4") && string.Equals(selectedProcess, Process.MIXING))
                    {
                        ResultDetail.Visibility = Visibility.Visible;
                        roweqpt.Height = new GridLength(35);
                    }
                    else
                    {
                        ResultDetail.Visibility = Visibility.Collapsed;
                        Grid.SetRowSpan(dgListMaterial, 3);
                        roweqpt.Height = new GridLength(0);
                    }
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            if (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("True"))
            {
                // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 Start
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "E")
                {
                    GetWorkorderMaterial((DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WOID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRJT_NAME_NEW") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PROCID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRODID") ?? String.Empty).ToString());
                    GetWorkoEqpt((DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WOID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRJT_NAME_NEW") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRODID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "EQPTID") ?? String.Empty).ToString());
                }
                else
                {
                    GetWorkorderMaterial((DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WOID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRJT_NAME") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PROCID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRODID") ?? String.Empty).ToString());
                    GetWorkoEqpt((DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WOID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRJT_NAME") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRODID") ?? String.Empty).ToString(),
                        (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "EQPTID") ?? String.Empty).ToString());
                }
                // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 End

                dgList.SelectedIndex = rowIndex;
            }
            else
            {
                return;
            }
        }

        private void dgList_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgList.ItemsSource == null || dgList.Rows.Count == 0)
                return;

            if (dgList.CurrentRow == null || dgList.CurrentRow.Index < 0 || dgList.SelectedIndex < 0)
            {
                return;
            }

            if (dgList.CurrentRow.DataItem == null)
                return;

            int Select = dgList.SelectedIndex;

            // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 Start
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "E")
            {
                GetWorkorderMaterial((DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "WOID") ?? String.Empty).ToString(),
                (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PRJT_NAME_NEW") ?? String.Empty).ToString(),
                (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PROCID") ?? String.Empty).ToString(),
                (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PRODID") ?? String.Empty).ToString());
            }
            else
            {
                GetWorkorderMaterial((DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "WOID") ?? String.Empty).ToString(),
                    (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PRJT_NAME") ?? String.Empty).ToString(),
                    (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PROCID") ?? String.Empty).ToString(),
                    (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PRODID") ?? String.Empty).ToString());
            }
            // 2023.10.09  강성묵 : E20230927-000880 전극 Nickname 표기 변경 End

            // 전극 버전 팝업을 화면내 처리
            GetElectroVersion(Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.SelectedIndex].DataItem, "PRODID")),
                              Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.SelectedIndex].DataItem, "AREAID")),
                              Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.SelectedIndex].DataItem, "EQSGID")));
        }

        private void xlsLoad_Closed(object sender, EventArgs e)
        {
            COM001_001_LOAD runStartWindow = sender as COM001_001_LOAD;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgListVersion_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && string.Equals(Util.NVC(cboProcess.SelectedValue), "E2000"))
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (!string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LANE_QTY_CNFM_FLAG"), "Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Silver);
                            }
                        }
                    }
                }));
            }
        }

        private void dgList_SelectionDragStarted(object sender, DataGridSelectionDragStartedEventArgs e)
        {
            e.Cancel = true;
        }

        private void dgListMaterial_SelectionDragStarted(object sender, DataGridSelectionDragStartedEventArgs e)
        {
            e.Cancel = true;
        }

        private void dgListVersion_SelectionDragStarted(object sender, DataGridSelectionDragStartedEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region Mehod
        private void Set_Combo_Area(C1ComboBox cbo)
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

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedArea) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedArea;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboArea_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        {
            //ESMI-A4동 1~5 Line 제외처리
            string bizRuleName = string.Empty;

            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FIRST_PRIORITY_LINE_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["AREAID"] = selectedArea;
            dtRQSTDT.Rows.Add(drnewrow);

            //new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                DataRow dRow = result.NewRow();
                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = "";
                result.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);
                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                {
                    cbo.SelectedValue = selectedEquipmentSegmant;
                }
                else if (result.Rows.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else if (result.Rows.Count == 0)
                {
                    cbo.SelectedItem = null;
                }
                cboEquipmentSegmant_SelectedItemChanged(cbo, null);
            }
            );
        }

        private void Set_Combo_Process(C1ComboBox cbo)
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
                DataRow dRow = result.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = "";
                result.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedProcess) select dr).Count() > 0)
                {
                    cbo.SelectedValue = selectedProcess;
                }
                else if (result.Rows.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else if (result.Rows.Count == 0)
                {
                    cbo.SelectedItem = null;
                }
            }
            );
        }

        private void Set_Combo_Equipment(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = selectedEquipmentSegmant;
                drnewrow["PROCID"] = selectedProcess;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipment) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedEquipment;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Set_Combo_Equipment2(C1ComboBox cbo,string sEQPTID)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = selectedEquipmentSegmant;
                drnewrow["PROCID"] = selectedProcess;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    cbo.ItemsSource = DataTableConverter.Convert(result);

                    cbo.SelectedValue = sEQPTID;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Set_Combo_COMMCODE(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            if (cbo == cboStatus)
            {
                drnewrow["CMCDTYPE"] = "FP_DETL_PLAN_STAT_CODE";
            }
            if(cbo == cboWOStatus)
            {
                drnewrow["CMCDTYPE"] = "WO_STAT_CODE";
            }
            if(cbo == cboDEMAND_TYPE)
            {
                drnewrow["CMCDTYPE"] = "DEMAND_TYPE";
            }
            if(cbo == cboPlanCnfm)
            {
                drnewrow["CMCDTYPE"] = "CNFM_FLAG";
            }
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
                    if (cbo == cboStatus)
                    {
                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        for (int i = 0; i < cbo.Items.Count; i++)
                        {
                            //if (((DataRowView)cbo.Items[i]).Row.ItemArray[1].ToString().Equals("R")) // 2024.10.10. 김영국 - DB상에서는 CODE, NAME으로 던져주는데, 받는건 NAME, CODE로 받아 상태값 가져오기 위해 ITEM 0 -> 1로 변경함.
                            if (((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString().Equals("R")) // 2024.10.10. 김영국 - DB상에서는 CODE, NAME으로 던져주는데, 받는건 NAME, CODE로 받아 상태값 가져오기 위해 ITEM 0 -> 1로 변경함.
                            {
                                cbo.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                    else if (cbo == cboWOStatus || cbo == cboDEMAND_TYPE)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                    else if (cbo == cboPlanCnfm)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 1;
                    }
                }
                else if (result.Rows.Count == 0)
                {
                    cbo.SelectedItem = null;
                }
            });
        }

        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                InitGrid();

                if (cboStatus.SelectedValue.ToString().Equals("C"))
                {
                    btnClose.IsEnabled = false;
                }
                else
                {
                    btnClose.IsEnabled = true;
                }


                if (rdoProcess.IsChecked.Value)
                {
                    dgList.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }
                else if (rdoEquipment.IsChecked.Value)
                {
                    dgList.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                    dgList.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PLANSDATE", typeof(string));
                IndataTable.Columns.Add("PLANEDATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("DEMAND_TYPE", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("LVL", typeof(string));
                IndataTable.Columns.Add("ROLL_TYPE", typeof(string));
                IndataTable.Columns.Add("FP_DETL_PLAN_STAT_CODE", typeof(string));
                IndataTable.Columns.Add("CNFM_FLAG", typeof(string));
                IndataTable.Columns.Add("WO_STAT_CODE", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));

                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpTo.SelectedDateTime);

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PLANSDATE"] = _ValueFrom;
                Indata["PLANEDATE"] = _ValueTo;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["DEMAND_TYPE"] = Util.NVC(cboDEMAND_TYPE.SelectedValue);
                Indata["PRJT_NAME"] = Util.NVC(txtProject.Text.Trim());
                Indata["PRODID"] = Util.NVC(txtProduct.Text.Trim());
                Indata["MODLID"] = Util.NVC(txtModel.Text.Trim());

                if (rdoProcess.IsChecked.Value)
                    Indata["LVL"] = "PROC";
                else if (rdoEquipment.IsChecked.Value)
                    Indata["LVL"] = "EQPT";

                if (rdoCRoll.IsChecked.Value)
                    Indata["ROLL_TYPE"] = "CROLL";
                else if (rdoSRoll.IsChecked.Value)
                    Indata["ROLL_TYPE"] = "SROLL";

                Indata["FP_DETL_PLAN_STAT_CODE"] = Util.NVC(cboStatus.SelectedValue.ToString());
                Indata["CNFM_FLAG"] = Util.NVC(cboPlanCnfm.SelectedValue.ToString());
                Indata["WO_STAT_CODE"] = Util.NVC(cboWOStatus.SelectedValue.ToString());
                Indata["WOID"] = Util.NVC(txtWO.Text.Trim());
                IndataTable.Rows.Add(Indata);

                paramList = Indata.ItemArray;

                new ClientProxy().ExecuteService("DA_PRD_SEL_FP_DETL_PLAN", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgList, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return;
            }
        }

        private void GetWorkorderMaterial(string sWOID, string sPRJTNAME, string sPROCID, string sPRODID)
        {
            Util.gridClear(dgListMaterial);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("PRODID", typeof(string));
            IndataTable.Columns.Add("ALL_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            Indata["PROCID"] = sPROCID;
            Indata["PRODID"] = sPRODID;
            if (sPRODID.Substring(3, 2).Equals("CA"))
            {
                Indata["ALL_FLAG"] = "Y";
            }
            else
            {
                Indata["ALL_FLAG"] = null;
            }
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_COM_SEL_TB_SFC_WO_MTRL_ALL", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    return;
                }

                txtMtrlWO.Text = sWOID;
                txtMtrlPrj.Text = sPRJTNAME;
                txtMtrlProd.Text = sPRODID;
                Util.GridSetData(dgListMaterial, result, FrameOperation, true);
            });
        }

        private void GetWorkoEqpt(string sWOID, string sPRJTNAME, string sPRODID, string sEQPTID)
        {
            Set_Combo_Equipment2(cboEquipment2, sEQPTID);
        }
        
        private void SaveData(string sWOID)
        {
            try
            {
                //마감하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("WOID", typeof(string));
                        IndataTable.Columns.Add("SHOPID", typeof(string));
                        IndataTable.Columns.Add("AREAID", typeof(string));
                        IndataTable.Columns.Add("CLOSE_FLAG", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));
                        IndataTable.Columns.Add("PLAN_TYPE", typeof(string));

                        DataTable dt = ((DataView)dgList.ItemsSource).Table;

                        foreach (DataRow _iRow in dt.Rows)
                        {
                            if (_iRow["CHK"].Equals("True"))
                            {
                                DataRow Indata = IndataTable.NewRow();
                                Indata["WOID"] = _iRow["WOID"];
                                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                Indata["AREAID"] = _iRow["AREAID"];
                                Indata["CLOSE_FLAG"] = "Y";
                                Indata["USERID"] = LoginInfo.USERID;
                                Indata["PLAN_TYPE"] = _iRow["PLAN_TYPE"];
                                IndataTable.Rows.Add(Indata);
                            }
                        }
                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService("BR_PRD_REG_CLOSE_WO_TO_ERP", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    //Util.MessageException(ex); // 2024.10.16. 김영국 - Validation 오류 Message Popup 변경 MessageException -> AlertError
                                    Util.AlertError(ex);
                                    return;
                                }
                                Util.AlertInfo("SFU1277");    //마감처리되었습니다.
                            });

                            SearchData();
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnLoad,
                btnClose,
                btnApplyVersion,
                btnFp,
                btnLoad,
                btnPilotWO,
                btnSave
                //btnSearch 2023-07-05 유재홍
                
            };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //변경하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("WOID", typeof(string)); //
                        IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                        IndataTable.Columns.Add("SHOPID", typeof(string));
                        IndataTable.Columns.Add("AREAID", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));
                        IndataTable.Columns.Add("EQPTID", typeof(string));

                        DataTable dt = ((DataView)dgList.ItemsSource).Table;

                        foreach (DataRow _iRow in dt.Rows)
                        {
                            if (_iRow["CHK"].Equals("True"))
                            {
                                DataRow Indata = IndataTable.NewRow();
                                Indata["WOID"] = _iRow["WOID"];
                                Indata["WO_DETL_ID"] = _iRow["WO_DETL_ID"];
                                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                Indata["AREAID"] = _iRow["AREAID"];
                                Indata["USERID"] = LoginInfo.USERID;
                                Indata["EQPTID"] = Util.NVC(cboEquipment2.SelectedValue);
                                IndataTable.Rows.Add(Indata);
                            }
                        }
                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService("DA_PRD_UPD_TB_SFC_FP_DETL_PLAN", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                Util.AlertInfo("SFU3059");    //작업지시가 변경 되었습니다.
                            });
                            Set_Combo_Equipment2(cboEquipment2, "");
                            chkEqpt.IsChecked = false;
                            SearchData();
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkEqpt_Checked(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = true;
            cboEquipment2.IsEnabled = true;
        }

        private void chkEqpt_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = false;
            cboEquipment2.IsEnabled = false;
        }

        #region FP 연결
        private void btnFp_Click(object sender, RoutedEventArgs e)
        {
            //string url = "http://165.244.95.220:8100/install.jsp?str_gmes_id=GMES_00&str_gmes_pw=gmesview!";
            string url = string.Format("http://165.244.95.220:8100/install.jsp?str_gmes_id={0}", LoginInfo.USERID);
            System.Diagnostics.Process.Start(url);
        }
        #endregion

        #region 시생산 W/O 생성
        private void btnPilotWO_Click(object sender, RoutedEventArgs e)
        {
            COM001_002_PILOT_WO_REGISTER popPilotWO = new COM001_002_PILOT_WO_REGISTER();
            popPilotWO.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popPilotWO.Name.ToString()) == false)
            {
                return;
            }

            // 2024.10.11. 김영국 - Main 화면에서 선택된 Process ID를 넘기도록 수정함.
            object[] Parameters = new object[1];
            Parameters[0] = cboProcess.SelectedValue;

            //object[] Parameters = new object[0];
            C1WindowExtension.SetParameters(popPilotWO, Parameters);

            popPilotWO.Closed += new EventHandler(popPilotWO_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popPilotWO.ShowModal()));
        }

        private void popPilotWO_Closed(object sender, EventArgs e)
        {
            COM001_002_PILOT_WO_REGISTER popup = sender as COM001_002_PILOT_WO_REGISTER;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private bool ValidationGridAdd(string popName)
        {
            //foreach (UIElement ui in grdMain.Children)
            //{
            //    if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
            //    {
            //        // 프로그램이 이미 실행 중 입니다. 
            //        Util.MessageValidation("SFU3193");
            //        return false;
            //    }
            //}

            return true;
        }

        #endregion

        #region 전극 버전 팝업을 화면내 처리
        private void GetElectroVersion(string sPRODID, string sAREAID,string sEQSGID)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["PRODID"] = sPRODID;
                drnewrow["AREAID"] = sAREAID;
                drnewrow["EQSGID"] = sEQSGID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CONV_RATE_PROD_VER", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                    
                    Util.GridSetData(dgListVersion, result, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void btnApplyVersion_Click(object sender, RoutedEventArgs e)
        {
            SaveVersionData();
        }

        private void SaveVersionData()
        {
            try
            {
                if (dgList.SelectedIndex < 0 || dgListVersion.SelectedIndex < 0)
                    return;

                if (Util.NVC(cboProcess.SelectedValue).Equals("E2000") &&
                    Util.NVC(DataTableConverter.GetValue(dgListVersion.Rows[dgListVersion.SelectedIndex].DataItem, "LANE_QTY_CNFM_FLAG")).Equals("N"))
                {
                    //Lane 수량이 확정되지 않은 버전입니다.
                    Util.AlertInfo("SFU8300");
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                        IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();                        
                        Indata["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.SelectedIndex].DataItem, "WOID"));
                        Indata["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgListVersion.Rows[dgListVersion.SelectedIndex].DataItem, "PROD_VER_CODE"));
                        Indata["USERID"] = LoginInfo.USERID;
                        IndataTable.Rows.Add(Indata);

                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService("BR_PRD_REG_TB_SFC_FP_DETL_PLAN_PROD_VER_CODE", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.AlertInfo("SFU1270");  //저장되었습니다.
                                this.dgList.ItemsSource = null;
                                SearchData();
                            });
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }




        #endregion


    }
}
