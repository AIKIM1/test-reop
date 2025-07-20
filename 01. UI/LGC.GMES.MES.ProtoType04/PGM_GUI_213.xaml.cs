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

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_213 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_213()
        {
            InitializeComponent();
        }


        #endregion
        private void Initialize()
        {
            testData();

        }
        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        #endregion

        #region Mehod
        private void testData()
        {
            testGridData();
            testGridData1();
            testComboSet();
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("CHK", typeof(bool));
            dtSearchResult.Columns.Add("WO", typeof(string));
            dtSearchResult.Columns.Add("MAT_GUBUN", typeof(string));
            dtSearchResult.Columns.Add("PROD_CODE", typeof(string));
            dtSearchResult.Columns.Add("CNT", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "4746962", "CELL", "", "" });
            menulist.Add(new object[] { false, "4746962", "CMA", "", "" });
            menulist.Add(new object[] { false, "4746962", "CMA", "", "" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgWordOrder, dtSearchResult);
        }

        private void testGridData1()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("CHK", typeof(bool));
            dtSearchResult.Columns.Add("WO", typeof(string));
            dtSearchResult.Columns.Add("MAT_GUBUN", typeof(string));
            dtSearchResult.Columns.Add("PROD_CODE", typeof(string));
            dtSearchResult.Columns.Add("CNT", typeof(string));
            dtSearchResult.Columns.Add("NOW_STATUS", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "4746962", "CELL", "", "", "" });
            menulist.Add(new object[] { false, "4746962", "CMA", "", "", "" });
            menulist.Add(new object[] { false, "4746962", "CMA", "", "", "" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgFalseList, dtSearchResult);
        }
        private void testComboSet()
        {
            testSet_Cbo(cboLine, "1", "PACK 1라인");
            testSet_Cbo(cboWordCenter, "1", "1호 CMA");
            testSet_Cbo(cboWorkOrder, "1", "474696");

            testSet_Cbo(cboPart, "1", "테스트부서");
            testSet_Cbo(cboStatus, "1", "불량");

            //Get_Data_Modify();
        }

        public void testSet_Cbo(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "---select---", "0" };
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
        void popup_Closed(object sender, EventArgs e)
        {

        }

        #endregion

        //private void dgBoxhistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    int currentRow = dgBoxhistory.CurrentRow.Index;


        //    if (dgBoxhistory.CurrentColumn.Index == 1)
        //    {
        //        string selectLot = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "LOT_ID").ToString();

        //        string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
        //        string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

        //        PopUpOpen(MAINFORMPATH, MAINFORMNAME);
        //    }
        //    else if(dgBoxhistory.CurrentColumn.Index == 0)
        //    {
        //        string selectBox = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOX_ID").ToString();

        //        txtBoxIdR.Text = selectBox;
        //    }
        //}

        private void btnBoxLabel_Click(object sender, RoutedEventArgs e)
        {
            //위에 선택된 : txtBoxIdR.Text = selectBox; BOX의 라벨 발행
        }

        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            //전체 취소
        }

        //private void btnPack_Click(object sender, RoutedEventArgs e)
        //{
        //    //BOX ID 생성 체크인 경우 포장시 BOX Label 발행
        //    if ((bool)chkBoxId.IsChecked)
        //    {
        //        //BOX Label 발행
        //    }
        //}

        //private void chkBoxId_Click(object sender, RoutedEventArgs e)
        //{
        //    if((bool)chkBoxId.IsChecked)
        //    {
        //        txtBoxId.IsReadOnly = true;
        //    }
        //    else
        //    {
        //        txtBoxId.IsReadOnly = false;
        //    }
        //}

        private void dgBoxLot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnTran_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgWordOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnReTran_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnWoSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgFalseList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}