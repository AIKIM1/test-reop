/*************************************************************************************
 Created Date : 2022.08.25
      Creator : 오화백
   Decription : Stocker 재고그룹에 대한 WO별 자재 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.25  오화백 : Initial Created.   
   

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;


namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_076_WORKORDER_MX_MTRL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_076_WORKORDER_MX_MTRL : C1Window, IWorkArea
    {
        #region Declaration & Constructor        
        private int _Row = 0;
      
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private string _MTRLID = string.Empty;
        private string _MTRLDESC = string.Empty;
        private string _PROCESS = string.Empty;

        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;

        public string MTRLID
        {
            get { return _MTRLID; }
        }
        public string MTRLDESC
        {
            get { return _MTRLDESC; }
        }
        public int ROW
        {
            get { return _Row; }
        }
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public MCS001_076_WORKORDER_MX_MTRL()
        {
            InitializeComponent();
        
            this.Loaded += C1Window_Loaded;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetParameters();
            InitCombo();
            this.Loaded -= C1Window_Loaded;
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgListMaterial);

        }
        /// <summary>
        /// 콤보 박스 초기화
        /// </summary>
        private void InitCombo()
        {
            selectedArea = LoginInfo.CFG_AREA_ID;
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = _PROCESS;

            dtpFrom.SelectedDateTime = System.DateTime.Now;
            dtpTo.SelectedDateTime = System.DateTime.Now;

            Set_Combo_Area(cboArea);
            Set_Combo_EquipmentSegmant(cboEquipmentSegment);

        }

        /// <summary>
        /// 팝업 호출 파라미터 셋팅
        /// </summary>
        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _Row = Convert.ToInt16(Util.NVC(tmps[0]));
            _PROCESS = Util.NVC(tmps[1]);
        }
     
        #endregion

        #region Event
         
        #region  조회 버튼 클릭 : btnSearch_Click()

        /// <summary>
        /// 조회 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 선택 버튼 클릭 : btnSelect_Click()

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgListMaterial.SelectedIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region 닫기 버튼 클릭 : btnClose_Click()

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region 동 콤보 박스 이벤트 : cboArea_SelectedItemChanged()

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_Process(cboProcess);

                    if (!string.Equals(selectedProcess, Process.MIXING))
                    {
                        Grid.SetRowSpan(dgListMaterial, 3);
                    }
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        #endregion

        #region 라인 콤보 박스 이벤트 : cboEquipmentSegmant_SelectedItemChanged()

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


        #endregion

        #region 공정 콤보 박스 이벤트 : cboProcess_SelectedItemChanged()

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    selectedProcess = Convert.ToString(cboProcess.SelectedValue);
                    Set_Combo_Equipment(cboEquipment);

                    if (!string.Equals(selectedProcess, Process.MIXING))
                    {
                        Grid.SetRowSpan(dgListMaterial, 3);
                    }
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }


        #endregion

        #region WO 리스트 선택시 : dgWorkOrderChoice_Checked()
        /// <summary>
        /// WO 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            if (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("True"))
            {
                GetWorkorderMaterial((DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WOID") ?? String.Empty).ToString(),
                    (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRJT_NAME") ?? String.Empty).ToString(),
                    (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PROCID") ?? String.Empty).ToString(),
                    (DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PRODID") ?? String.Empty).ToString());
                dgList.SelectedIndex = rowIndex;
            }
            else
            {
                return;
            }
        }


        #endregion

        #region WO 리스트 이벤트 : dgList_()
        /// <summary>
        /// WO 리스트 수정 금지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_SelectionDragStarted(object sender, DataGridSelectionDragStartedEventArgs e)
        {
            e.Cancel = true;
        }
        /// <summary>
        /// WO 리스트 선택 변경시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            GetWorkorderMaterial((DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "WOID") ?? String.Empty).ToString(),
                (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PRJT_NAME") ?? String.Empty).ToString(),
                (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PROCID") ?? String.Empty).ToString(),
                (DataTableConverter.GetValue(dgList.Rows[Select].DataItem, "PRODID") ?? String.Empty).ToString());


        }


        #endregion

        #region 자재 리스트 선택시 : dgListMaterialChoiceGroup_Checked()

        /// <summary>
        /// 자재선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgListMaterialChoiceGroup_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListMaterial.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

            DataTable dt = ((DataView)dgListMaterial.ItemsSource).Table;

            if (DataTableConverter.GetValue(dgListMaterial.Rows[rowIndex].DataItem, "CHK").ToString().Equals("True") || DataTableConverter.GetValue(dgListMaterial.Rows[rowIndex].DataItem, "CHK").ToString().Equals("1"))
            {
                dgListMaterial.SelectedIndex = rowIndex;

                _MTRLID = DataTableConverter.GetValue(dgListMaterial.Rows[rowIndex].DataItem, "MTRLID").ToString();
                _MTRLDESC = DataTableConverter.GetValue(dgListMaterial.Rows[rowIndex].DataItem, "MTRLNAME").ToString();
            }
            else
            {
                return;
            }
        }


        #endregion

        #endregion

        #region Method

        #region 동 정보 조회 : Set_Combo_Area()
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
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
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


        #endregion

        #region 라인 정보 조회 : Set_Combo_EquipmentSegmant()

        /// <summary>
        ///  라인 콤보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
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

        #endregion

        #region 공정 정보 조회 : Set_Combo_Process()

        /// <summary>
        /// 공정 콤보 조회 
        /// </summary>
        /// <param name="cbo"></param>
        private void Set_Combo_Process(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["EQSGID"] = selectedEquipmentSegmant;
            drnewrow["PROCID"] = _PROCESS;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_CBO_BY_PROCID", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                DataRow dRow = result.NewRow();

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




        #endregion

        #region 설비 정보 조회 : Set_Combo_Equipment()

        /// <summary>
        /// 설비 콤보 조회
        /// </summary>
        /// <param name="cbo"></param>
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
                    //WO 정보 조회
                    SearchData();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        #region WO 리스트 조회 : SearchData()

        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        private void SearchData()
        {
            try
            {
                object[] paramList = null;
                loadingIndicator.Visibility = Visibility.Visible;
                InitGrid();

                dgList.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                dgList.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;

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
                Indata["DEMAND_TYPE"] = string.Empty;
                Indata["PRJT_NAME"] = string.Empty;
                Indata["PRODID"] = string.Empty;
                Indata["MODLID"] = string.Empty;
                Indata["LVL"] = "EQPT";
                Indata["ROLL_TYPE"] = "SROLL";


                Indata["FP_DETL_PLAN_STAT_CODE"] = "R";
                Indata["CNFM_FLAG"] = "Y";
                Indata["WO_STAT_CODE"] = string.Empty;
                Indata["WOID"] = string.Empty;
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







        #endregion

        #region 자재 정보 조회 : GetWorkorderMaterial()

        /// <summary>
        /// WO 에 대한 자재 정보 조회
        /// </summary>
        /// <param name="sWOID"></param>
        /// <param name="sPRJTNAME"></param>
        /// <param name="sPROCID"></param>
        /// <param name="sPRODID"></param>
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

                Util.GridSetData(dgListMaterial, result, FrameOperation, true);
            });
        }


        #endregion

        #endregion

    }

}