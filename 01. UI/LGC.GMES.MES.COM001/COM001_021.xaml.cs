/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtSearchResult1;
        DataTable dtGridChek;

        COM001_021_SPECCHANGEHISTORY specPopUp;
        COM001_021_CONTROLCHANGEHISTORY controlPopUp;
       
        public COM001_021()
        {
            InitializeComponent();

            this.Loaded += COM001_021_Loaded;
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

            tbSpecHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbControlHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }
        #endregion

        #region Event
        private void COM001_021_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= COM001_021_Loaded;
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }
        #endregion

        #region Mehod
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";
          
            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProduct };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //제품코드  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            C1ComboBox[] cboProductChild = { cboRout };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, cbChild: cboProductChild, sCase: "PRJ_PRODUCT");

            //경로
            C1ComboBox[] cbocboRoutParent = { cboAreaByAreaType, cboEquipmentSegment, cboProduct};
            C1ComboBox[] cboRoutChild = { cboProcessRout };
            _combo.SetCombo(cboRout, CommonCombo.ComboStatus.SELECT, cbParent: cbocboRoutParent, cbChild: cboRoutChild);

            //공정
            C1ComboBox[] cboProcessRoutParent = { cboAreaByAreaType, cboEquipmentSegment, cboProduct, cboRout };            
            _combo.SetCombo(cboProcessRout, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessRoutParent);
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
            try
            {
                C1DataGrid dg = new C1DataGrid();

                if (tcMain.SelectedIndex == 0)
                {
                    dg = dgSpec;
                }
                else
                {
                    dg = dgControlValue;
                }

                if(dg == null || dg.GetRowCount() == 0)
                {
                    return;
                }

                new ExcelExporter().Export(dg);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgSpec_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            return;
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

                COM001_021_LOTEHISTORY popup02 = new COM001_021_LOTEHISTORY();
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
            return;

            int currentRow = dgControlValue.CurrentRow.Index;

            if (dgControlValue.CurrentColumn.Index == 0)
            {
                string selectLot = DataTableConverter.GetValue(dgControlValue.Rows[currentRow].DataItem, "PRODID").ToString();

                COM001_021_LOTEHISTORY popup02 = new COM001_021_LOTEHISTORY();
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
/*
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo _combo = new CommonCombo();

            //string selected_value = cboEquipmentSegment.SelectedValue.ToString();

            //int selected_index = cboEquipmentSegment.SelectedIndex;

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);

        }
*/
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(cboEquipmentSegment.SelectedIndex == 0)
                {
                    Util.AlertInfo("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (cboProduct.SelectedIndex == 0)
                {
                    Util.AlertInfo("SFU1895");  //제품을 선택하세요.
                    return;
                }

                if (cboRout.SelectedIndex == 0)
                {
                    Util.AlertInfo("SFU1455");  //경로를 선택하세요.
                    return;
                }

                if (cboProcessRout.SelectedIndex == 0)
                {
                    Util.AlertInfo("SFU1459");  //공정을 선택하세요.
                    return;
                }

                specSearch(); //스펙값               

                controlSearch(); //제어값
               
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void specSearch()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string)); //동
                RQSTDT.Columns.Add("EQSGID", typeof(string)); //라인
                RQSTDT.Columns.Add("PRODID", typeof(string)); //제품
                RQSTDT.Columns.Add("ROUTID", typeof(string)); //경로
                RQSTDT.Columns.Add("PROCID", typeof(string)); //공정

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["ROUTID"] = Util.GetCondition(cboRout);
                dr["PROCID"] = Util.GetCondition(cboProcessRout);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPEC_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                dgSpec.ItemsSource = null;                

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgSpec, dtResult, FrameOperation,true);                    
                }

                Util.SetTextBlockText_DataGridRowCount(tbSpecHistory_cnt, Util.NVC(dtResult.Rows.Count));                
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void controlSearch()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string)); //동
            RQSTDT.Columns.Add("EQSGID", typeof(string)); //라인
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품
            RQSTDT.Columns.Add("ROUTID", typeof(string)); //경로
            RQSTDT.Columns.Add("PROCID", typeof(string)); //공정

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
            dr["PRODID"] = Util.GetCondition(cboProduct);
            dr["ROUTID"] = Util.GetCondition(cboRout);
            dr["PROCID"] = Util.GetCondition(cboProcessRout);
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONTROL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

            dgControlValue.ItemsSource = null;

            if (dtResult.Rows.Count != 0)
            {
                Util.GridSetData(dgControlValue, dtResult, FrameOperation , true);
            }

            Util.SetTextBlockText_DataGridRowCount(tbControlHistory_cnt, Util.NVC(dtResult.Rows.Count));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccess(sender);
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }            
        }
        
        private void buttonAccess(object sender)
        {
            try
            {
                C1DataGrid dg = new C1DataGrid();
                Button btn = sender as Button;
                string grid_name = string.Empty;

                //if (tcMain.SelectedIndex == 0)
                //{
                //    grid_name = "dgSpec";
                //}
                //else
                //{
                //    grid_name = "dgControlValue";
                //}

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

                dg.SelectedItem = row.DataItem;
                object selectedItem = dg.SelectedItem;

                DataRowView drv = selectedItem as DataRowView;

                int currentRow = dg.CurrentRow.Index;
                string selectProdect = drv["PRODID"].ToString();
                //string clctitem = drv["CLCTITEM"].ToString();

                if (grid_name == "dgSpec")
                {
                    // popup02 = new PGM_GUI_206_SPECCHANGEHISTORY();
                    specPopUp = new COM001_021_SPECCHANGEHISTORY();
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
                    controlPopUp = new COM001_021_CONTROLCHANGEHISTORY();
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
            catch(Exception ex)
            {
                throw ex;
            }

            
        }
    }
}
