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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_207 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;

        public PGM_GUI_207()
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            dgCellId.ItemsSource = null;
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfigCancel_Click(object sender, RoutedEventArgs e)
        {

        }



        private void chkPalletId_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgPalletId_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnExcelDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcelLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgCellHistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgCellId_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            string cell_id = "";
            string reason;
            bool chk_vali = false;

            //홀드 사유 필수
            if (cboHoldReason.SelectedIndex < 1)
            {
                MessageBox.Show("HOLD 사유를 선택하세요");
                return;
            }

            reason = cboHoldReason.Text;

            //홀드 작업 시작
            if (dgCellId.ItemsSource != null)
            {

                for (int i = 0; i < dgCellId.Rows.Count; i++)
                {
                    var chkYn = DataTableConverter.GetValue(dgCellId.Rows[i].DataItem, "CHK");

                    if (chkYn != null)
                    {
                        if ((bool)chkYn)
                        {
                            cell_id += " / " + DataTableConverter.GetValue(dgCellId.Rows[i].DataItem, "CELL_ID").ToString();

                            chk_vali = true;
                        }
                    }


                }

            }

            if (!chk_vali)
            {
                MessageBox.Show("선택된 항목이 없습니다.");
                return;
            }

            MessageBox.Show("선택하신 " + cell_id + " 가 HOLD 됐습니다. \n HOLD 사유 : " + reason);

            //hold 후 cell 리스트에서 삭제
            //hold 리스트에 update

            for (int i = dgCellId.Rows.Count; 0 < i; i--)
            {
                var chkYn = DataTableConverter.GetValue(dgCellId.Rows[i - 1].DataItem, "CHK");
                var hold_cell_id = DataTableConverter.GetValue(dgCellId.Rows[i - 1].DataItem, "CELL_ID");

                if (chkYn == null)
                {
                    dgCellId.RemoveRow(i - 1);
                }
                else if ((bool)chkYn)
                {
                    dgCellId.EndNewRow(true);
                    dgCellId.RemoveRow(i - 1);

                    int TotalRow = dgCellHistory.Rows.Count;

                    DataGridRowAdd(dgCellHistory);

                    DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow - 1].DataItem, "CELL_ID", hold_cell_id);
                    DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow - 1].DataItem, "CHK", false);
                    DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow - 1].DataItem, "HOLD_DATE", DateTime.Now.ToString());
                    DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow - 1].DataItem, "HOLD_REASON", reason);
                    DataTableConverter.SetValue(dgCellHistory.Rows[TotalRow - 1].DataItem, "USER", "admin");
                }
            }

        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellId.ItemsSource != null)
            {
                for (int i = dgCellId.Rows.Count; 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgCellId.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgCellId.Rows[i - 1].DataItem, "CELL_ID");

                    if (chkYn == null)
                    {
                        dgCellId.RemoveRow(i - 1);
                    }
                    else if ((bool)chkYn)
                    {
                        dgCellId.EndNewRow(true);
                        dgCellId.RemoveRow(i - 1);
                    }
                }

            }
        }

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
            dtSearchResult.Columns.Add("CELL_ID", typeof(string));
            dtSearchResult.Columns.Add("OPER_ID", typeof(string));
            dtSearchResult.Columns.Add("STATUS_ID", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "NF15082901", "", "" });
            menulist.Add(new object[] { false, "NF15082902", "", "" });
            menulist.Add(new object[] { false, "NF15082903", "", "" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgCellId, dtSearchResult);
        }

        private void testGridData1()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("CHK", typeof(bool));
            dtSearchResult.Columns.Add("CELL_ID", typeof(string));
            dtSearchResult.Columns.Add("NOW_OPER", typeof(string));
            dtSearchResult.Columns.Add("OPER_STATUS", typeof(string));
            dtSearchResult.Columns.Add("HOLD_DATE", typeof(string));
            dtSearchResult.Columns.Add("HOLD_REASON", typeof(string));
            dtSearchResult.Columns.Add("USER", typeof(string));


            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "NF15082904", "", "", "2016-07-15 오전 08:30:10", "", "관리자" });
            menulist.Add(new object[] { false, "NF15082905", "", "", "2016-07-15 오전 08:31:13", "", "관리자" });
            menulist.Add(new object[] { false, "NF15082906", "", "", "2016-07-15 오전 08:31:13", "", "관리자" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgCellHistory, dtSearchResult);
        }
        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void testComboSet()
        {
            testSet_Cbo(cboHoldReason, "1", "HOLD_test");
            testSet_Cbo(cboClearReason, "1", "해제_test");
            //testSet_Cbo(cboOutPlace, "1", "MAGNA");

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





        #endregion

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtCellId.Text != "")
                {
                    int TotalRow = dgCellId.Rows.Count;

                    DataGridRowAdd(dgCellId);

                    DataTableConverter.SetValue(dgCellId.Rows[TotalRow - 1].DataItem, "CELL_ID", txtCellId.Text);
                    DataTableConverter.SetValue(dgCellId.Rows[TotalRow - 1].DataItem, "CHK", false);
                }


            }
        }

        private void btnHoldClear_Click(object sender, RoutedEventArgs e)
        {
            string cell_id = "";
            string reason;
            bool chk_vali = false;


            //해제 사유 필수
            if (cboClearReason.SelectedIndex < 1)
            {
                MessageBox.Show("해제 사유를 선택하세요");

                return;
            }

            reason = cboClearReason.Text;

            //해제 작업 시작
            if (dgCellHistory.ItemsSource != null)
            {

                for (int i = 0; i < dgCellHistory.Rows.Count; i++)
                {
                    var chkYn = DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK");

                    if (chkYn != null)
                    {
                        if ((bool)chkYn)
                        {
                            cell_id += " / " + DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CELL_ID").ToString();

                            chk_vali = true;
                        }
                    }
                }

            }

            if (!chk_vali)
            {
                MessageBox.Show("선택된 항목이 없습니다.");
                return;
            }

            MessageBox.Show("선택하신 " + cell_id + " 가 해제 됐습니다. \n 해제 사유 : " + reason);

            for (int i = dgCellHistory.Rows.Count; 0 < i; i--)
            {
                var chkYn = DataTableConverter.GetValue(dgCellHistory.Rows[i - 1].DataItem, "CHK");
                var hold_cell_id = DataTableConverter.GetValue(dgCellHistory.Rows[i - 1].DataItem, "CELL_ID");

                if (chkYn == null)
                {
                    dgCellHistory.RemoveRow(i - 1);
                }
                else if ((bool)chkYn)
                {
                    dgCellHistory.EndNewRow(true);
                    dgCellHistory.RemoveRow(i - 1);
                }
            }

        }
    }
}