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
    public partial class PGM_GUI_079 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;

        bool fullCheck = false;
        public PGM_GUI_079()
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
            //아직 테이블이 완성되지 않아서 실제 data 가져올수 없어서 주석처리
            //getSpecdata();

            //TABLE이 구성되지 않아 TEST롤 데이터 뿌려줌...
            //testData();

            InitCombo();


        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //string area = LoginInfo.CFG_AREA_ID.ToString();

            //동
            //C1ComboBox[] cboAreaChild = { cboLine };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //제품
            //C1ComboBox[] cboLineParent = { cboArea };
            //C1ComboBox[] cboLineChild = { cboProcess };
            string[] params1 = { "", "P" };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, sFilter: params1);

            //제품유형
            string[] params2 = { "P" };
            _combo.SetCombo(cboProductType, CommonCombo.ComboStatus.ALL, sFilter: params2);

            //파일럿 공정
            string[] params3 = { "P" };
            _combo.SetCombo(cboPilotProc, CommonCombo.ComboStatus.ALL, sFilter: params3);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable inDataTable = new DataTable();

            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PRODTYPE", typeof(string));

            DataRow searchCondition = inDataTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["PROCID"] = cboPilotProc.SelectedValue;
            searchCondition["PRODID"] = "ACEP1043I-A1";// pilot 관련 제품정보가 없어서 데이터 나오는 것으로 하드코딩 : cboProduct.SelectedValue;
            searchCondition["PRODTYPE"] = cboProductType.SelectedValue;

            inDataTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("DA_PRD_SEL_PILOTLOT_SEARCH", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    dtSearchResult = searchResult;
                    dgSearchResult.ItemsSource = DataTableConverter.Convert(searchResult);

                    //SetWorkOrderQtyInfo(GetWorkOrderInfo("", true));
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        private void btnAllSelect_Click(object sender, RoutedEventArgs e)
        {
            dtGridChek = dtSearchResult;

            if (fullCheck == false)
            {
                for (int i = 0; i < dtGridChek.Rows.Count; i++)
                {

                    dtGridChek.Rows[i][0] = true;

                }
                fullCheck = true;
            }
            else
            {
                for (int i = 0; i < dtGridChek.Rows.Count; i++)
                {

                    dtGridChek.Rows[i][0] = false;

                }
                fullCheck = false;
            }

            SetBinding(dgSearchResult, dtGridChek);

        }

        private void btnAllEndOper_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearchResult.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    string lotId = DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "LOTID").ToString();
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(i+1 + " 번째 ROW의 LOT : " + lotId + " 이 완공 처리 되었습니다.");
                }
            }
        }

        private void btnExel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgSearchResult,"excel_test");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text, "LOTID");
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }



 /*





            int currentRow = dgSearchResult.CurrentRow.Index;

            //if (dgSearchResult.CurrentColumn.Index ==1)
            //{


            //    string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            //    string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

            //    PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            // }

            // popup02 = new PGM_GUI_206_SPECCHANGEHISTORY();
            PGM_GUI_079_LOTEHISTORY LotPopUp = new PGM_GUI_079_LOTEHISTORY();
            LotPopUp.FrameOperation = this.FrameOperation;

            if (LotPopUp != null)
            {
                string selectLot = DataTableConverter.GetValue(dgSearchResult.Rows[currentRow].DataItem, "LOTID").ToString();

                DataTable dtData = new DataTable();

                dtData.Columns.Add("VALUE", typeof(string));
                //dtData.Columns.Add("COL02", typeof(string));

                DataRow newRow = null;

                newRow = dtData.NewRow();
                //newRow.ItemArray = new object[] { selectProdect + " + " + clctitem };
                newRow.ItemArray = new object[] { selectLot };
                dtData.Rows.Add(newRow);

                //newRow = dtData.NewRow();
                //newRow.ItemArray = new object[] { "Text01", "Text02" };
                //dtData.Rows.Add(newRow);

                //========================================================================
                string Parameter = "Parameter";
                C1WindowExtension.SetParameter(LotPopUp, Parameter);
                //========================================================================
                object[] Parameters = new object[2];
                Parameters[0] = "Parameter01";
                Parameters[1] = dtData;
                C1WindowExtension.SetParameters(LotPopUp, Parameters);
                //========================================================================

                //popup02.Closed -= popup02_Closed;
                //popup02.Closed += popup02_Closed;
                LotPopUp.ShowModal();
                LotPopUp.CenterOnScreen();
            }

*/
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }

        #endregion

        #region Mehod
        private void getSpecdata()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("PRODTYPE", typeof(string)); //제품 유형          
            RQSTDT.Columns.Add("PROCID", typeof(string));   //일괄작업공정           
            RQSTDT.Columns.Add("PRODID", typeof(string));   //제품           

            DataRow dr = RQSTDT.NewRow();
            dr["PRODTYPE"] = cboProductType.SelectedValue;
            dr["PROCID"] = cboPilotProc.SelectedValue;
            dr["PRODID"] = cboProduct.SelectedValue;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PILOTLOT_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);

            SetBinding(dgSearchResult, dtResult);
        }

        private void testData()
        {
            testGridData();
            testComboSet();
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();
            dtSearchResult.Columns.Add("CHK", typeof(bool));
            dtSearchResult.Columns.Add("LOTID", typeof(string));
            dtSearchResult.Columns.Add("PRODNAME", typeof(string));
            dtSearchResult.Columns.Add("INSDTTM", typeof(string));
            dtSearchResult.Columns.Add("PROCNAME", typeof(string));
            dtSearchResult.Columns.Add("WIPSTAT", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "LGC-KOR13.07.16A4MD0028", "Audi BEV A4MF", "2016-07-13 19:30:18", "PILOT2호 CMA 조립", "작업중" });
            menulist.Add(new object[] { false, "LGC-KOR13.07.16A4MD0027", "Audi BEV A4MF", "2016-07-13 19:30:18", "PILOT2호 CMA 조립", "작업중" });
            menulist.Add(new object[] { false, "LGC-KOR13.07.16A4MD0026", "Audi BEV A4MF", "2016-07-13 19:30:18", "PILOT2호 CMA 조립", "작업중" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgSearchResult, dtSearchResult);
        }
        private void testComboSet()
        {
            testSet_Cbo(cboProductType, "1", "CMA");
            testSet_Cbo(cboPilotProc, "1", "PILOT CMA조립 공정");
            testSet_Cbo(cboProduct, "1", "Audi BEV A4MF");

            //Get_Data_Modify();
        }

        public void testSet_Cbo(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

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

        
    }
}
