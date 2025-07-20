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
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_094 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;
        public PGM_GUI_094()
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

            //���̺��� �غ� ���� �ʾ� ��ȸ���� ������ �Ұ���
            //InitCombo();

        }
        #endregion
        

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int currentRow = dgSearchResult.CurrentColumn.Index;


            if (dgSearchResult.CurrentColumn.Index == 2)
            {
                string selectLot = DataTableConverter.GetValue(dgSearchResult.Rows[currentRow].DataItem, "LOT_ID").ToString();

                string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
                string MAINFORMNAME = "PGM_GUI_079_LOTEHISTORY";

                PopUpOpen(MAINFORMPATH, MAINFORMNAME);
            }

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            if (e.Cell.Presenter == null)
            {
                return;
            }
            //link ������
            if (e.Cell.Column.Name.Equals("OUT_INSP"))
            {
                //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);

                string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_INSP"));
                if (sCheck.Equals("�ҷ�"))
                {
                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);

                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["OUT_INSP"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["PALLET_ID"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                }
            }

            //Ʋ��������
            if (e.Cell.Column.Name.Equals("YN"))
            {
                string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "YN"));
                if (sCheck.Equals("�ҷ�"))
                {
                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);

                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["YN"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["LOT_ID"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["LOT_ID"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
            }
            
            //}));
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void popup_Closed(object sender, EventArgs e)
        {

        }
        #endregion

        #region Mehod

        private void InitCombo()
        {

        }

        private void testData()
        {
            testGridData();
            testComboSet();
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();
            
            dtSearchResult.Columns.Add("PALLET_ID",         typeof(string));
            dtSearchResult.Columns.Add("BOX_ID",            typeof(string));
            dtSearchResult.Columns.Add("LOT_ID",            typeof(string));
            dtSearchResult.Columns.Add("MODEL",             typeof(string));
            dtSearchResult.Columns.Add("OUT_REV_DATE",      typeof(string));
            dtSearchResult.Columns.Add("OUT_PLACE",         typeof(string));
            dtSearchResult.Columns.Add("PALLET_CONFIG_DATE",typeof(string));
            dtSearchResult.Columns.Add("BOX_CONFIG_DATE",   typeof(string));
            dtSearchResult.Columns.Add("PRODUCT_CODE",      typeof(string));
            dtSearchResult.Columns.Add("PRODUCT_NAME",      typeof(string));
            dtSearchResult.Columns.Add("OUT_INSP",          typeof(string));
            dtSearchResult.Columns.Add("YN",                typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "PP00077398", "LGC-KOR17.07.16C0010100", "LGC-KOR17.07.16C0010100", "37AH", "2016-07-15 ���� 10:35:1", "MAGNA", "2016-07-15 ���� 10:35:13", "2016-07-15 ���� 10:35:13", "EVPVPCMPL65B0", "Audi_37h CMA(9ȣ��)", "�ҷ�", "�հ�" });
            menulist.Add(new object[] { "PP00077398", "LGC-KOR17.07.16C0010098", "LGC-KOR17.07.16C0010098", "37AH", "2016-07-15 ���� 10:35:1", "MAGNA", "2016-07-15 ���� 10:35:13", "2016-07-15 ���� 10:35:13", "EVPVPCMPL65B0", "Audi_37h CMA(9ȣ��)", "�ҷ�", "�ҷ�" });
            menulist.Add(new object[] { "PP00077398", "LGC-KOR17.07.16C0010097", "LGC-KOR17.07.16C0010097", "37AH", "2016-07-15 ���� 10:35:1", "MAGNA", "2016-07-15 ���� 10:35:13", "2016-07-15 ���� 10:35:13", "EVPVPCMPL65B0", "Audi_37h CMA(9ȣ��)", "�ҷ�", "�հ�" });

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
            testSet_Cbo_1(cboPalletConfig, "1", "CMA"); 
            testSet_Cbo(cboModel, "1", "PILOT CMA���� ����");
            testSet_Cbo(cboOutPlace, "1", "MAGNA");

            //Get_Data_Modify();
        }

        public void testSet_Cbo(C1.WPF.C1ComboBox cbo, string key, string value)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "��ü", "0" };
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
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "��ü", "0" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "�ȷ�Ʈ������", "1" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] {"�ڽ�������", "2" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "���Ͽ�����", "3" };
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
