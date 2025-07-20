using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_353.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_353 : UserControl, IWorkArea
    {
        // Variable

        #region Variable
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;
        private object[] paramList = null;
        #endregion

        // Constructor

        #region Constructor
        /// <summary>
        /// COM001_353 Constructor
        /// </summary>
        public COM001_353()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += UserControl_Loaded;
        }
        #endregion

        // Initialize

        #region FrameOperation : Frame과 상호 작용 하기 위한 객체
        /// <summary>
        /// Frame과 상호 작용 하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region UserControl_Loaded : UserControl Load Event
        /// <summary>
        /// UserControl_Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region Initialize : 컨트롤 초기화
        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            selectedArea = LoginInfo.CFG_AREA_ID;
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = LoginInfo.CFG_PROC_ID;
            InitCombo();
        }
        #endregion

        #region InitCombo : Combo 초기화
        /// <summary>
        /// InitCombo 초기화
        /// </summary>
        private void InitCombo()
        {
            dtpFrom.SelectedDateTime = System.DateTime.Now;
            dtpTo.SelectedDateTime = System.DateTime.Now;

            Set_Combo_Area(cboArea);
            Set_Combo_EquipmentSegmant(cboEquipmentSegment);
            Set_Combo_COMMCODE(cboPmYn);
        }
        #endregion

        // Event

        #region cboArea_SelectedItemChanged : Area(동) Combo Selected Item Change Event
        /// <summary>
        /// Area(동) Selected Item Change Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_Process(cboProcess);
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }
        #endregion

        #region cboEquipmentSegmant_SelectedItemChanged : EquipmentSegment(라인) Combo Selected Item Change Event 
        /// <summary>
        /// EquipmentSegment(라인) Selected Item Change Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #region cboProcess_SelectedItemChanged : Process (공정) Combo Selected Item Change Event
        /// <summary>
        /// Process (공정) Combo Selected Item Change Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    selectedProcess = Convert.ToString(cboProcess.SelectedValue);
                    Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }
        #endregion

        #region btnSearch_Click : Search Button Click Event
        /// <summary>
        /// Search Button Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        #endregion

        // Method

        #region Set_Combo_Equipment : Equipment Combo Data Set
        /// <summary>
        /// Equipment Combo Data Set
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
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region Set_Combo_Area : Area (동) Combo Data Set
        /// <summary>
        /// Area (동) Combo Data Set
        /// </summary>
        /// <param name="cbo"></param>
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
        #endregion

        #region Set_Combo_COMMCODE : Common Code 기준정보 Combo Data Set
        /// <summary>
        /// Common Code 기준정보 Combo Data Set
        /// </summary>
        /// <param name="cbo"></param>
        private void Set_Combo_COMMCODE(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            if (cbo == cboPmYn)
            {
                drnewrow["CMCDTYPE"] = "FLAG_YN";

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
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
        }
        #endregion

        #region Set_Combo_EquipmentSegmant : EquipmentSegment (라인) Combo Data Set
        /// <summary>
        /// Equipment Combo Data Set
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
                //DataRow dRow = result.NewRow();
                //dRow["CBO_NAME"] = "-ALL-";
                //dRow["CBO_CODE"] = "";
                //result.Rows.InsertAt(dRow, 0);

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

        #region Set_Combo_Process : Process(공정) Combo Data Set
        /// <summary>
        /// Process(공정) Combo Data Set
        /// </summary>
        /// <param name="cbo"></param>
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
                //DataRow dRow = result.NewRow();

                //dRow["CBO_NAME"] = "-ALL-";
                //dRow["CBO_CODE"] = "";
                //result.Rows.InsertAt(dRow, 0);

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

        #region SearchData : Data Search
        /// <summary>
        /// Data Search
        /// </summary>
        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                InitGrid();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("PLANSDATE", typeof(string));
                IndataTable.Columns.Add("PLANEDATE", typeof(string));
                IndataTable.Columns.Add("PM_CNFM_FLAG", typeof(string));

                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpTo.SelectedDateTime);

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["PLANSDATE"] = _ValueFrom;
                Indata["PLANEDATE"] = _ValueTo;
                Indata["PM_CNFM_FLAG"] = Util.NVC(cboPmYn.SelectedValue);

                IndataTable.Rows.Add(Indata);

                paramList = Indata.ItemArray;

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_PM_PLAN", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
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

        #region InitGrid : InitGrid
        /// <summary>
        /// InitGrid
        /// </summary>
        private void InitGrid()
        {
            Util.gridClear(dgList);
        }
        #endregion
    }
}
