/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_206 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtSearchResult1;
        DataTable dtGridChek;

        PGM_GUI_206_SPECCHANGEHISTORY specPopUp;
        PGM_GUI_206_CONTROLCHANGEHISTORY controlPopUp;

        bool loadYn = true;
        public PGM_GUI_206()
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
            testData();


            InitCombo();

            loadYn = false;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }
        #endregion

        #region Mehod
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //string area = LoginInfo.CFG_AREA_ID.ToString();

            //동
            //C1ComboBox[] cboAreaChild = { cboLine };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            //C1ComboBox[] cboLineParent = { cboArea };
            //C1ComboBox[] cboLineChild = { cboProcess };
            string[] area = { LoginInfo.CFG_AREA_ID.ToString() };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: area);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);


        }
        private void testData()
        {
            testGridData();
            testGridData1();
            testComboSet();
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("CHANG_HIS", typeof(string));
            dtSearchResult.Columns.Add("PRODID", typeof(string));
            dtSearchResult.Columns.Add("ROUTID", typeof(string));
            dtSearchResult.Columns.Add("ROUTNAME", typeof(string));
            dtSearchResult.Columns.Add("PROCID", typeof(string));
            dtSearchResult.Columns.Add("PROCNAME", typeof(string));
            dtSearchResult.Columns.Add("CLCTITEM", typeof(string));
            dtSearchResult.Columns.Add("CLCTNAME", typeof(string));
            dtSearchResult.Columns.Add("CLCTUSL", typeof(string));
            dtSearchResult.Columns.Add("CLCTLSL", typeof(string));


            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "조회", "MBHV0501AP", "CELL01", "1호 CELL 가공경로", "CT001", "1호기 CELL 검사공정", "CTG001", "Cell 전압", "3.84", "3.88" });
            menulist.Add(new object[] { "조회", "MBHV0501AP", "CELL01", "1호 CELL 가공경로", "CT001", "1호기 CELL 검사공정", "CTG001", "Cell 통전 저항", "0", "100" });
            menulist.Add(new object[] { "조회", "MBHV0501AP", "CELL01", "1호 CELL 가공경로", "CT001", "1호기 CELL 검사공정", "CTG001", "Cell 절연 전압", "100", "1000" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgSpec, dtSearchResult);
        }

        private void testGridData1()
        {
            dtSearchResult1 = new DataTable();

            dtSearchResult1.Columns.Add("CHANG_HIS", typeof(string));
            dtSearchResult1.Columns.Add("PRODID", typeof(string));
            dtSearchResult1.Columns.Add("ROUTID", typeof(string));
            dtSearchResult1.Columns.Add("ROUTNAME", typeof(string));
            dtSearchResult1.Columns.Add("PROCID", typeof(string));
            dtSearchResult1.Columns.Add("PROCNAME", typeof(string));
            dtSearchResult1.Columns.Add("FLOWID", typeof(string));
            dtSearchResult1.Columns.Add("FLOWNAME", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL1", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL2", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL3", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL4", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL5", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL6", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL7", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL8", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL9", typeof(string));
            dtSearchResult1.Columns.Add("CTRLVAL10", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "조회", "MMEV4201AVMKA-E0", "MODL00", "2호 CMA 조립경로", "MT101", "2호기 CMA 검사공정", "MTC101", "검사 Rest 시간", "20", "", "", "", "", "", "", "", "", "", "" });
            menulist.Add(new object[] { "조회", "MMEV4201AVMKA-E0", "MODL00", "2호 CMA 조립경로", "MT101", "2호기 CMA 검사공정", "MTC101", "검사 충전전류", "387", "", "", "", "", "", "", "", "", "", "" });
            menulist.Add(new object[] { "조회", "MMEV4201AVMKA-E0", "MODL00", "2호 CMA 조립경로", "MT101", "2호기 CMA 검사공정", "MTC101", "검사 충전시간", "10", "", "", "", "", "", "", "", "", "", "" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult1.NewRow();
                newRow.ItemArray = item;
                dtSearchResult1.Rows.Add(newRow);
            }

            SetBinding(dgControlValue, dtSearchResult1);
        }

        private void testComboSet()
        {
            testSet_Cbo_1(cboEquipmentSegment, "1", "PACK 2라인");
            testSet_Cbo(cboProduct, "1", "MMEV4201AVMKA-E0");
            testSet_Cbo(cboPath, "1", "2호 CMA 조립");
            testSet_Cbo(cboProcess, "1", "2호 CMA 검사");

            //Get_Data_Modify();
        }

        public void testSet_Cbo(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(string));
            dtResult.Columns.Add("CBO_CODE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "전체", "0" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + key, "1" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "2", "2" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "3", "3" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if (dtResult.Rows.Count > 0)
            {
                //CheckAll();
            }
        }

        public void testSet_Cbo_1(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(string));
            dtResult.Columns.Add("CBO_CODE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "전체", "0" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "팔래트구성일", "1" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "박스구성일", "2" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "출하예정일", "3" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if (dtResult.Rows.Count > 0)
            {
                //CheckAll();
            }
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        #endregion

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSpec_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //int currentRow = dgSpec.CurrentColumn.Index;

            //if (dgSpec.CurrentColumn.Index == 0)
            //{
            //    string changeHis = DataTableConverter.GetValue(dgSpec.Rows[currentRow].DataItem, "CHANG_HIS").ToString();

            //    string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            //    string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

            //    PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            //}



            ////
            int currentRow = dgSpec.CurrentRow.Index;

            if (dgSpec.CurrentColumn.Index == 0)
            {
                string selectLot = DataTableConverter.GetValue(dgSpec.Rows[currentRow].DataItem, "PRODID").ToString();

                PGM_GUI_079_LOTEHISTORY popup02 = new PGM_GUI_079_LOTEHISTORY();
                popup02.FrameOperation = this.FrameOperation;

                if (popup02 != null)
                {
                    DataTable dtData = new DataTable();

                    dtData.Columns.Add("VALUE", typeof(string));
                    //dtData.Columns.Add("COL02", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow.ItemArray = new object[] { selectLot };
                    dtData.Rows.Add(newRow);

                    //newRow = dtData.NewRow();
                    //newRow.ItemArray = new object[] { "Text01", "Text02" };
                    //dtData.Rows.Add(newRow);

                    //========================================================================
                    string Parameter = "Parameter";
                    C1WindowExtension.SetParameter(popup02, Parameter);
                    //========================================================================
                    object[] Parameters = new object[2];
                    Parameters[0] = "Parameter01";
                    Parameters[1] = dtData;
                    C1WindowExtension.SetParameters(popup02, Parameters);
                    //========================================================================

                    //popup02.Closed -= popup02_Closed;
                    //popup02.Closed += popup02_Closed;
                    popup02.ShowModal();
                    popup02.CenterOnScreen();
                }

                //string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
                //string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

                //PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            }

            ///


        }

        private void dgControlValue_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int currentRow = dgControlValue.CurrentRow.Index;

            if (dgControlValue.CurrentColumn.Index == 0)
            {
                string selectLot = DataTableConverter.GetValue(dgControlValue.Rows[currentRow].DataItem, "PRODID").ToString();

                PGM_GUI_079_LOTEHISTORY popup02 = new PGM_GUI_079_LOTEHISTORY();
                popup02.FrameOperation = this.FrameOperation;

                if (popup02 != null)
                {
                    DataTable dtData = new DataTable();

                    dtData.Columns.Add("VALUE", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow.ItemArray = new object[] { selectLot };
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    string Parameter = "Parameter";
                    C1WindowExtension.SetParameter(popup02, Parameter);
                    //========================================================================
                    object[] Parameters = new object[2];
                    Parameters[0] = "Parameter01";
                    Parameters[1] = dtData;
                    C1WindowExtension.SetParameters(popup02, Parameters);
                    //========================================================================

                    popup02.ShowModal();
                    popup02.CenterOnScreen();
                }
            }
        }

        private void tcMain_ItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //if(loadYn)
            //{
            //    return;
            //}

            //CommonCombo _combo = new CommonCombo();

            //string selected_value = cboEquipmentSegment.SelectedValue.ToString();

            //int selected_index = cboEquipmentSegment.SelectedIndex;

            //공정
            //C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (loadYn)
            {
                return;
            }

            CommonCombo _combo = new CommonCombo();

            //string selected_value = cboEquipmentSegment.SelectedValue.ToString();

            //int selected_index = cboEquipmentSegment.SelectedIndex;

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                specSearch();
                //controlSearch();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void specSearch()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQSGID", typeof(string)); //라인
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품
            RQSTDT.Columns.Add("ROUTID", typeof(string)); //경로
            RQSTDT.Columns.Add("PROCID", typeof(string)); //공정

            DataRow dr = RQSTDT.NewRow();
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PRODID"] = "PRODID"; //cboProduct.SelectedValue;
            dr["ROUTID"] = "RT_ELEC_MLB_001"; // cboPath.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPEC_INFO", "RQSTDT", "RSLTDT", RQSTDT);

            SetBinding(dgSpec, dtResult);
        }

        private void controlSearch()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQSGID", typeof(string)); //라인
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품
            RQSTDT.Columns.Add("ROUTID", typeof(string)); //경로
            RQSTDT.Columns.Add("PROCID", typeof(string)); //공정

            DataRow dr = RQSTDT.NewRow();
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PRODID"] = "PRODID"; //cboProduct.SelectedValue;
            dr["ROUTID"] = "RT_ELEC_MLB_001"; // cboPath.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONTROL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

            SetBinding(dgControlValue, dtResult);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            buttonAccess(sender);
        }

        private void buttonAccess(object sender)
        {
            Button btn = sender as Button;

            string grid_name = "dgSpec";

            C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
            System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
            foreach (var item in ilist)
            {
                C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                if (presenter != null)
                {
                    row = presenter.Row;

                    grid_name = presenter.DataGrid.Name;
                }
            }

            dgSpec.SelectedItem = row.DataItem;
            object selectedItem = dgSpec.SelectedItem;

            DataRowView drv = selectedItem as DataRowView;

            int currentRow = dgSpec.CurrentRow.Index;
            string selectProdect = drv["PRODID"].ToString();
            //string clctitem = drv["CLCTITEM"].ToString();

            if (grid_name == "dgSpec")
            {
                // popup02 = new PGM_GUI_206_SPECCHANGEHISTORY();
                specPopUp = new PGM_GUI_206_SPECCHANGEHISTORY();
                specPopUp.FrameOperation = this.FrameOperation;

                if (specPopUp != null)
                {
                    DataTable dtData = new DataTable();

                    dtData.Columns.Add("VALUE", typeof(string));
                    //dtData.Columns.Add("COL02", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    //newRow.ItemArray = new object[] { selectProdect + " + " + clctitem };
                    newRow.ItemArray = new object[] { selectProdect };
                    dtData.Rows.Add(newRow);

                    //newRow = dtData.NewRow();
                    //newRow.ItemArray = new object[] { "Text01", "Text02" };
                    //dtData.Rows.Add(newRow);

                    //========================================================================
                    string Parameter = "Parameter";
                    C1WindowExtension.SetParameter(specPopUp, Parameter);
                    //========================================================================
                    object[] Parameters = new object[2];
                    Parameters[0] = "Parameter01";
                    Parameters[1] = dtData;
                    C1WindowExtension.SetParameters(specPopUp, Parameters);
                    //========================================================================

                    //popup02.Closed -= popup02_Closed;
                    //popup02.Closed += popup02_Closed;
                    specPopUp.ShowModal();
                    specPopUp.CenterOnScreen();
                }

            }
            else if (grid_name == "dgControlValue")
            {
                controlPopUp = new PGM_GUI_206_CONTROLCHANGEHISTORY();
                controlPopUp.FrameOperation = this.FrameOperation;

                if (controlPopUp != null)
                {
                    DataTable dtData = new DataTable();

                    dtData.Columns.Add("VALUE", typeof(string));
                    //dtData.Columns.Add("COL02", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    //newRow.ItemArray = new object[] { selectProdect + " + " + clctitem };
                    newRow.ItemArray = new object[] { selectProdect };
                    dtData.Rows.Add(newRow);

                    //newRow = dtData.NewRow();
                    //newRow.ItemArray = new object[] { "Text01", "Text02" };
                    //dtData.Rows.Add(newRow);

                    //========================================================================
                    string Parameter = "Parameter";
                    C1WindowExtension.SetParameter(controlPopUp, Parameter);
                    //========================================================================
                    object[] Parameters = new object[2];
                    Parameters[0] = "Parameter01";
                    Parameters[1] = dtData;
                    C1WindowExtension.SetParameters(controlPopUp, Parameters);
                    //========================================================================

                    //popup02.Closed -= popup02_Closed;
                    //popup02.Closed += popup02_Closed;
                    controlPopUp.ShowModal();
                    controlPopUp.CenterOnScreen();
                }
            }
        }
    }
}