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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_008_1 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtResult;

        public PACK001_008_1()
        {
            InitializeComponent();
            this.Loaded += PACK001_008_1_Loaded;
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
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            testData();
        }
        #endregion

        #region Event
        private void PACK001_008_1_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_008_1_Loaded;

            txtAdvice.Text = "31607151";
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //Label Layout ���
        }

        private void btnAdvice_Click(object sender, RoutedEventArgs e)
        {
            //Label Layout�� Advice Note No �κ� ���
        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int currentRow = dgResult.CurrentColumn.Index;


            if (dgResult.CurrentColumn.Index == 1)
            {
                string selectLot = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOT_ID").ToString();

                //������ LOT_ID�� Dock, Serial Number, Batch No �����ͼ� Text�ڽ��� ����
            }
        }

        private void dtpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            //��¥ ���ý� Advice Note No, Date ������ �����ͼ� Text �ڽ��� ����
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //��ȸ ���ǵ鿡 �ش��ϴ� LOT_ID�� PALLET_ID �����ͼ� Grid�� ���ε�
        }
        #endregion

        #region Mehod
        private void testData()
        {
            testGridData();            
        }

        private void testGridData()
        {
            dtResult = new DataTable();
            dtResult.Columns.Add("LOT_ID", typeof(string));
            dtResult.Columns.Add("PALLET_ID", typeof(string));           

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "31419205T715122801", "pallet001" });
            menulist.Add(new object[] { "31419205T715122802", "pallet001" });
            menulist.Add(new object[] { "31419205T715122803", "pallet002" });
            menulist.Add(new object[] { "31419205T715122804", "pallet003" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = item;
                dtResult.Rows.Add(newRow);
            }

            SetBinding(dgResult, dtResult);
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        private void txtAdvice_TextChanged(object sender, TextChangedEventArgs e)
        {
            bcAdvice.Text = txtAdvice.Text;
        }
    }
}
