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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_089 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtResult;
        DataTable dtGridChek;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_089()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize
        private void Initialize()
        {
            testData();
        }
        #endregion

        #region Event

        #endregion

        #region Mehod
        private void testData()
        {
            testGridData();
            testComboSet();
            Combo_Init();
        }

        private void testGridData()
        {
            dtResult = new DataTable();
            dtResult.Columns.Add("CHK", typeof(bool));
            dtResult.Columns.Add("EXCUTE_DATE", typeof(string));
            dtResult.Columns.Add("INPUT_DATE", typeof(string));
            dtResult.Columns.Add("MODEL", typeof(string));
            dtResult.Columns.Add("PRODUCT_NAME", typeof(string));
            dtResult.Columns.Add("LOT_ID", typeof(string));
            dtResult.Columns.Add("FAIL_NAME", typeof(string));
            dtResult.Columns.Add("PATH", typeof(string));
            dtResult.Columns.Add("MAKE_DATE", typeof(string));
            dtResult.Columns.Add("NOTE", typeof(string));
            dtResult.Columns.Add("DESC", typeof(string));

            List<object[]> menulist = new List<object[]>();

            //menulist.Add(new object[] { false, "20160715", "2016-07-13 오후 2:30:18", "B10", "B10 ph2 CMA", "295B99236RTE3TA00001", "용접 HIGHT 에러", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18","", "" });
            //menulist.Add(new object[] { false, "20160715", "2016-07-13 오후 2:35:18", "B10", "B10 ph2 CMA", "295B99236RTE3TA00001", "용접 POWER 에러", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            //menulist.Add(new object[] { false, "20160715", "2016-07-13 오후 2:50:18", "B10", "B10 ph2 CMA", "295B99236RTE3TA00001", "용접 에너지 하한","4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            //menulist.Add(new object[] { false, "20160716", "2016-07-13 오후 2:30:18", "B11", "B11 ph2 CMA", "295B99236RTE3TA00001", "용접 HIGHT 에러", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            //menulist.Add(new object[] { false, "20160716", "2016-07-13 오후 2:30:18", "B11", "B11 ph2 CMA", "295B99236RTE3TA00001", "용접 HIGHT 에러", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            //menulist.Add(new object[] { false, "20160716", "2016-07-13 오후 2:30:18", "B11", "B11 ph2 CMA", "295B99236RTE3TA00001", "용접 HIGHT 에러", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });


            menulist.Add(new object[] { false, "20160715", "2016-07-13 오후 2:30:18", "B10", "B10 ph2 CMA", "295B99236RTE3TA00001", "A", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            menulist.Add(new object[] { false, "20160715", "2016-07-13 오후 2:35:18", "B10", "B10 ph2 CMA", "295B99236RTE3TA00001", "B", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            menulist.Add(new object[] { false, "20160715", "2016-07-13 오후 2:50:18", "B10", "B10 ph2 CMA", "295B99236RTE3TA00001", "C", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            menulist.Add(new object[] { false, "20160716", "2016-07-13 오후 2:30:18", "B11", "B11 ph2 CMA", "295B99236RTE3TA00001", "D", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            menulist.Add(new object[] { false, "20160716", "2016-07-13 오후 2:30:18", "B11", "B11 ph2 CMA", "295B99236RTE3TA00001", "E", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });
            menulist.Add(new object[] { false, "20160716", "2016-07-13 오후 2:30:18", "B11", "B11 ph2 CMA", "295B99236RTE3TA00001", "F", "4호기 CMA조립경로", "2016-07-13 오후 2:30:18", "", "" });


            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = item;
                dtResult.Rows.Add(newRow);
            }

            SetBinding(dgSearchResult, dtResult);
        }

        private void testComboSet()
        {
            testSet_Cbo(cboShop, "1", "4호기");
            testSet_Cbo(cboProduct, "1", "VEVPCMB102A0");
            testSet_Cbo(cboModel, "1", "B10");
            testSet_Cbo(cboProductType, "1", "CMA");            
            testSet_Cbo(cboFailOper, "1", "A");
            testSet_Cbo(cboWordGroup, "1", "4호기 CMA용접공정");     

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

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "4", "4" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "5", "5" };
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

        void popup_Closed(object sender, EventArgs e)
        {

        }

        private void Combo_Init()
        {
            //ComboBox 세팅
            SetGridCboItem(dgSearchResult.Columns["FAIL_NAME"]);           
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("VALUE");
            dt.Columns.Add("KEY");

            DataRow newRow;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "용접 ENERGY 상한 에러", "A" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "용접 HIGHT 에러", "B" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "용접 POWER 에러", "C" };
            dt.Rows.Add(newRow);
            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "용접 TIME 에러", "D" };
            dt.Rows.Add(newRow);
            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "용접_에너지 하한", "E" };
            dt.Rows.Add(newRow);
            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "외관_찍힘_파우치", "F" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "조립_클린칭", "G" };
            dt.Rows.Add(newRow);

            dt.AcceptChanges();

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            

            //if (dgSearchResult.CurrentColumn.Index == 5)
            //{
            //    string selectLot = DataTableConverter.GetValue(dgSearchResult.Rows[currentRow].DataItem, "LOT_ID").ToString();

            //    string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            //    string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

            //    PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            //}
            

            if (dgSearchResult.CurrentColumn.Index == 5)
            {
                int currentRow = dgSearchResult.CurrentRow.Index;
                string selectLot = DataTableConverter.GetValue(dgSearchResult.Rows[currentRow].DataItem, "LOT_ID").ToString();

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

        }

        private void dgSearchResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int current_row = dgSearchResult.CurrentRow.Index;
            int current_col = dgSearchResult.CurrentColumn.Index;
            var value = dgSearchResult.CurrentCell.Value;

            //불량 이름 클릭시 처리
            if (current_col == 6)
            {
                if(Convert.ToBoolean(DataTableConverter.GetValue(dgSearchResult.Rows[current_row].DataItem, "CHK")))
                {
                    (dgSearchResult.Columns["FAIL_NAME"] as C1.WPF.DataGrid.DataGridComboBoxColumn).IsReadOnly = false;
                }
                else
                {
                    (dgSearchResult.Columns["FAIL_NAME"] as C1.WPF.DataGrid.DataGridComboBoxColumn).IsReadOnly = true;
                }
            }

            //체크박스 아닌 컬럼은 제외
            if (current_col != 0)
            {
                return;
            }          

            for (int i = 0; i < dtResult.Rows.Count; i++)
            {
                dtResult.Rows[i][0] = false;
            }

            if (Convert.ToBoolean(value))
            {
                dtResult.Rows[current_row][0] = true;

                (dgSearchResult.Columns["FAIL_NAME"] as C1.WPF.DataGrid.DataGridComboBoxColumn).IsReadOnly = false;
            }
            else
            {
                dtResult.Rows[current_row][0] = false;
                (dgSearchResult.Columns["FAIL_NAME"] as C1.WPF.DataGrid.DataGridComboBoxColumn).IsReadOnly = true;
            }

            dtResult.AcceptChanges();

            dgSearchResult.ItemsSource = DataTableConverter.Convert(dtResult);

            

        }
    }
}
