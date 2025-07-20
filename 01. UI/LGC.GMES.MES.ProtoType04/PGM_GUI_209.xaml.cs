/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_209 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;

        bool fullCheck = false;
        public PGM_GUI_209()
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
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(i + " 번째 ROW 완공 처리");
                }
            }
        }

        private void btnExel_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Mehod
        private void testData()
        {
            testGridData();
            testComboSet();
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();
            dtSearchResult.Columns.Add("CHK", typeof(bool));
            dtSearchResult.Columns.Add("DATE", typeof(string));
            dtSearchResult.Columns.Add("MAT_CODE", typeof(string));
            dtSearchResult.Columns.Add("MAT_NAME", typeof(string));
            dtSearchResult.Columns.Add("OUT_PLANT", typeof(string));
            dtSearchResult.Columns.Add("IN_PLANT", typeof(string));
            dtSearchResult.Columns.Add("OUT_LOCATION", typeof(string));
            dtSearchResult.Columns.Add("IN_LOCATION", typeof(string));
            dtSearchResult.Columns.Add("CNT", typeof(string));
            dtSearchResult.Columns.Add("UNIT", typeof(string));
            dtSearchResult.Columns.Add("TRAN_FLAG", typeof(string));
            dtSearchResult.Columns.Add("STATUS", typeof(string));
            dtSearchResult.Columns.Add("MESAGE", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { false, "20160715", "EVEVFCP0650I0", "E63_B10_LREV1", "A040", "A040", "4310", "2420", "40", "PC", "전송", "OK", "OK" });
            menulist.Add(new object[] { false, "20160716", "EVEVFCP0650I1", "E63_B10_LREV2", "A040", "A040", "4310", "2420", "40", "PC", "전송실패", "OK", "OK" });
            menulist.Add(new object[] { false, "20160717", "EVEVFCP0650I2", "E63_B10_LREV3", "A040", "A040", "4310", "2420", "40", "PC", "전송", "OK", "OK" });

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
            testSet_Cbo(cboTranGubun, "1", "전송");

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
            newRow.ItemArray = new object[] { value, "1" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + key, "2" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { value + "2", "3" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "전송실패", "4" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "전송대기", "5" };
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

        #endregion

        private void btnReTran_Click(object sender, RoutedEventArgs e)
        {
            string mat_list = "";
            bool chk_vali = false;
            string tran_flag;
            int tran_cnt = 0;

            //재전송 작업 시작
            if (dgSearchResult.ItemsSource != null)
            {
                for (int i = 0; i < dgSearchResult.Rows.Count; i++)
                {
                    var chkYn = DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "CHK");

                    if (chkYn != null)
                    {
                        if ((bool)chkYn)
                        {
                            chk_vali = true;

                            tran_flag = DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "TRAN_FLAG").ToString();

                            if (tran_flag == "전송실패")
                            {
                                mat_list += " / " + DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "MAT_NAME").ToString();

                                DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "TRAN_FLAG", "전송");

                                tran_cnt++;
                            }
                            else
                            {

                            }
                        }
                    }
                }

            }

            if (!chk_vali)
            {
                MessageBox.Show("선택된 항목이 없습니다.");
                return;
            }

            if (tran_cnt == 0)
            {
                MessageBox.Show("선택된 항목이 전송실패 상태가 아닙니다.");
                return;
            }

            MessageBox.Show("선택하신 " + mat_list + " 가 재전송 됐습니다.");

            for (int i = 0; i < dgSearchResult.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
            }
        }
    }
}