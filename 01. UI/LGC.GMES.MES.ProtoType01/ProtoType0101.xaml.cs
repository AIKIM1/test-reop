/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;

namespace LGC.GMES.MES.ProtoType01
{
    public partial class ProtoType0101 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        BizDataSet _Biz = new BizDataSet();
        CommonDataSet _Com = new CommonDataSet();
        Util _Util = new Util();
        DataTable dtData = new DataTable();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            Content = "ALL",
            IsChecked = true,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        C1XLBook xlbook = new C1XLBook();
        XLSheet xlsheet;
        XLStyle sty_Board, sty_NoBoard;

        public ProtoType0101()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            InitControl();
        }

        #endregion

        #region Initialize

        private void InitControl()
        {
            //dgData.Style = null;

            InitControlCombo();
            Get_Data();
            Get_DataColor();
            InitControlTree();
            InitControlC1Tree();
            InitControlC1TileListBox();
            InitControlC1ListBox();
            InitControlCell01();
            InitControlCell02();
            SetEquipmentpopup(SampleEquipment);
        }

        private void SetEquipmentpopup(PopupFindControl pop)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, "A2A11", "A8000", null };
            string selectedValueText = (string)pop.SelectedValuePath;
            string displayMemberText = (string)pop.DisplayMemberPath;

            CommonCombo.SetFindPopupCombo(bizRuleName, pop, arrColumn, arrCondition, selectedValueText, displayMemberText);
        }


        private void InitControlCombo()
        {
            //Normal Combo Box ( 비동기식 )
            _Com.SetCOR_SEL_SHOP_ORG_G_TEST(cboTest01, null, null, null, null, null, "FGI", null, CommonDataSet.ComboStatus.SELECT, loadingIndicator);

            //Multi Combo Box ( 비동기식 )
            _Com.SetCOR_SEL_MLOT_TP_CODE_CBO_G_TEST(cboTest02, null, null, null, null, null, null, loadingIndicator);

            //Normal Combo Box ( 동기식 )
            _Com.SetCOR_SEL_SHOP_ORG_G_TEST(cboTest03, null, null, null, null, null, "FGI", null, CommonDataSet.ComboStatus.ALL, loadingIndicator, (X, Y) =>
            {
                //Combo 설정 후 작업 내용
            });

            //Multi Combo Box ( 비동기식 )
            _Com.SetCOR_SEL_MLOT_TP_CODE_CBO_G_TEST(cboTest04, null, null, null, null, null, null, loadingIndicator, (X, Y) =>
            {
                //Multi Combo 설정 후 작업 내용
            });

            //Normal Combo Box ( 비동기식 )
            _Com.SetCOR_SEL_SHOP_ORG_G_TEST(cboTest05, null, null, null, null, null, "FGI", null, CommonDataSet.ComboStatus.SELECT, loadingIndicator);

            //Normal Combo Box ( 동기식 )
            _Com.SetCOR_SEL_SHOP_ORG_G_TEST(cboTest06, null, null, null, null, null, "FGI", null, CommonDataSet.ComboStatus.ALL, loadingIndicator, (X, Y) =>
            {
                //Combo 설정 후 작업 내용
            });
        }

        private void InitControlTree()
        {
            //trvTree.Items.Clear();
            //for (int idx01 = 1; idx01 < 3; idx01++)
            //{
            //    TreeViewItem item01 = new TreeViewItem();
            //    item01.Header = "Item" + idx01.ToString();
            //    for (int idx02 = 1; idx02 < 5; idx02++)
            //    {
            //        TreeViewItem item02 = new TreeViewItem();
            //        item02.Header = "Item" + " : " + idx01.ToString() + " : " + idx02.ToString();

            //        for (int idx03 = 1; idx03 < 5; idx03++)
            //        {
            //            TreeViewItem item03 = new TreeViewItem();
            //            item03.Header = "Item" + " : " + idx01.ToString() + " : " + idx02.ToString() + " : " + idx03.ToString();
            //            item02.Items.Add(item03);
            //        }
            //        item01.Items.Add(item02);
            //    }
            //    item01.IsExpanded = true;
            //    trvTree.Items.Add(item01);
            //}
        }

        private void InitControlC1Tree()
        {
            trvC1Tree.Items.Clear();
            for (int idx01 = 1; idx01 < 3; idx01++)
            {
                C1TreeViewItem item01 = new C1TreeViewItem();
                item01.Header = "Item" + idx01.ToString();
                //item01.Header = SetC1TreeViewItemHeader("Item" + idx01.ToString());
                for (int idx02 = 1; idx02 < 5; idx02++)
                {
                    C1TreeViewItem item02 = new C1TreeViewItem();
                    item02.Header = "Item" + " : " + idx01.ToString() + " : " + idx02.ToString();
                    //item02.Header = SetC1TreeViewItemHeader("Item" + " : " + idx01.ToString() + " : " + idx02.ToString());
                    for (int idx03 = 1; idx03 < 5; idx03++)
                    {
                        C1TreeViewItem item03 = new C1TreeViewItem();
                        item03.Header = "Item" + " : " + idx01.ToString() + " : " + idx02.ToString() + " : " + idx03.ToString();
                        //item03.Header = SetC1TreeViewItemHeader("Item" + " : " + idx01.ToString() + " : " + idx02.ToString() + " : " + idx03.ToString());
                        item02.Items.Add(item03);
                    }
                    item01.Items.Add(item02);
                }
                item01.IsExpanded = true;
                trvC1Tree.Items.Add(item01);
            }
        }

        private void InitControlC1TileListBox()
        {
            for ( int idx =0; idx < 10; idx++)
            {
                Object item = new Object();
                item = "Item " + idx.ToString("00");
                lstC1TileListBox01.Items.Add(item);
            }

            //========================================================================

            DataTable dtC1TileListBox = new DataTable();

            dtC1TileListBox.Columns.Add("CODE", typeof(string));
            dtC1TileListBox.Columns.Add("NAME", typeof(string));

            DataRow newRow = null;

            newRow = dtC1TileListBox.NewRow();
            newRow.ItemArray = new object[] { "CODE01", "NAME01" };
            dtC1TileListBox.Rows.Add(newRow);

            newRow = dtC1TileListBox.NewRow();
            newRow.ItemArray = new object[] { "CODE02", "NAME02" };
            dtC1TileListBox.Rows.Add(newRow);

            newRow = dtC1TileListBox.NewRow();
            newRow.ItemArray = new object[] { "CODE03", "NAME03" };
            dtC1TileListBox.Rows.Add(newRow);

            newRow = dtC1TileListBox.NewRow();
            newRow.ItemArray = new object[] { "CODE04", "NAME04" };
            dtC1TileListBox.Rows.Add(newRow);

            newRow = dtC1TileListBox.NewRow();
            newRow.ItemArray = new object[] { "CODE05", "NAME05" };
            dtC1TileListBox.Rows.Add(newRow);

            IEnumerable menuList = DataTableConverter.Convert(dtC1TileListBox);
            lstC1TileListBox02.ItemsSource = menuList;

        }

        private void InitControlC1ListBox()
        {
            for (int idx = 0; idx < 10; idx++)
            {
                Object item = new Object();
                item = "Item " + idx.ToString("00");
                lstC1TileListBox01.Items.Add(item);
            }

            //========================================================================

            DataTable dtC1ListBox = new DataTable();

            dtC1ListBox.Columns.Add("CHK", typeof(bool));
            dtC1ListBox.Columns.Add("COL1", typeof(string));
            dtC1ListBox.Columns.Add("COL2", typeof(string));
            dtC1ListBox.Columns.Add("COL3", typeof(string));

            DataRow newRow = null;

            newRow = dtC1ListBox.NewRow();
            newRow.ItemArray = new object[] { true, "COL1", "COL2", "COL2" };
            dtC1ListBox.Rows.Add(newRow);

            newRow = dtC1ListBox.NewRow();
            newRow.ItemArray = new object[] { false, "COL1", "COL2", "COL2" };
            dtC1ListBox.Rows.Add(newRow);

            newRow = dtC1ListBox.NewRow();
            newRow.ItemArray = new object[] { true, "COL1", "COL2", "COL2" };
            dtC1ListBox.Rows.Add(newRow);

            IEnumerable ieList = DataTableConverter.Convert(dtC1ListBox);
            lstC1ListBox.ItemsSource = ieList;
        }

        private void InitControlCell01()
        {
            DataTable dtCell01 = new DataTable();

            dtCell01.Columns.Add("NO", typeof(string));
            dtCell01.Columns.Add("A", typeof(string));
            dtCell01.Columns.Add("A_JUDGE", typeof(string));
            dtCell01.Columns.Add("A_LOT", typeof(string));

            DataRow newRow = null;

            newRow = dtCell01.NewRow();
            newRow.ItemArray = new object[] { "1", "LOT001", "SC", "LOT001,LOT002" };
            dtCell01.Rows.Add(newRow);

            newRow = dtCell01.NewRow();
            newRow.ItemArray = new object[] { "2", "LOT010", "NR", "LOT010,LOT011,LOT012" };
            dtCell01.Rows.Add(newRow);

            newRow = dtCell01.NewRow();
            newRow.ItemArray = new object[] { "3", "LOT020", "DL", "LOT020,LOT021,LOT022,LOT023" };
            dtCell01.Rows.Add(newRow);

            newRow = dtCell01.NewRow();
            newRow.ItemArray = new object[] { "4", "LOT030", "ID", "LOT030,LOT031,LOT032,LOT033,LOT034" };
            dtCell01.Rows.Add(newRow);

            newRow = dtCell01.NewRow();
            newRow.ItemArray = new object[] { "5", "LOT040", "PD", "LOT040,LOT041,LOT042" };
            dtCell01.Rows.Add(newRow);

            newRow = dtCell01.NewRow();
            newRow.ItemArray = new object[] { "6", "LOT050", "NI", "LOT050,LOT051" };
            dtCell01.Rows.Add(newRow);

            dgCell01.ItemsSource = DataTableConverter.Convert(dtCell01);
        }

        private void InitControlCell02()
        {
            DataTable dtCell02 = new DataTable();

            dtCell02.Columns.Add("NO", typeof(string));
            dtCell02.Columns.Add("A", typeof(string));
            dtCell02.Columns.Add("A_JUDGE", typeof(string));
            dtCell02.Columns.Add("A_LOT", typeof(string));
            dtCell02.Columns.Add("B", typeof(string));
            dtCell02.Columns.Add("B_JUDGE", typeof(string));
            dtCell02.Columns.Add("B_LOT", typeof(string));

            DataRow newRow = null;

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "1", "LOT001", "SC", "LOT001,LOT002,LOT003", "", "", "" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "1", "LOT001", "SC", "LOT001,LOT002,LOT003", "BLOT001", "SC", "BLOT001,BLOT002" };
            dtCell02.Rows.Add(newRow);


            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "2", "LOT010", "NR", "LOT010,LOT011,LOT012", "BLOT001", "SC", "BLOT001,BLOT002" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "2", "LOT010", "NR", "LOT010,LOT011,LOT012", "BLOT010", "NR", "BLOT010,BLOT011,BLOT012" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "3", "LOT020", "DL", "LOT020,LOT021,LOT022,LOT023", "BLOT010", "NR", "BLOT010,BLOT011,BLOT012" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "3", "LOT020", "DL", "LOT020,LOT021,LOT022,LOT023", "BLOT020", "DL", "BLOT020,BLOT021,BLOT022,BLOT023" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "4", "LOT030", "ID", "LOT030,LOT031,LOT032,LOT033,LOT034", "BLOT020", "DL", "BLOT020,BLOT021,BLOT022,BLOT023" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "4", "LOT030", "ID", "LOT030,LOT031,LOT032,LOT033,LOT034", "BLOT030", "ID", "BLOT030,BLOT031,BLOT032,BLOT033,BLOT034" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "5", "LOT040", "PD", "LOT040,LOT041,LOT042,LOT043", "BLOT030", "ID", "BLOT030,BLOT031,BLOT032,BLOT033,BLOT034" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "5", "LOT040", "PD", "LOT040,LOT041,LOT042,LOT043", "BLOT040", "PD", "BLOT040,BLOT041,BLOT042,BLOT043" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "6", "LOT050", "NI", "LOT050,LOT051,LOT052", "BLOT040", "PD", "BLOT040,BLOT041,BLOT042,BLOT043" };
            dtCell02.Rows.Add(newRow);

            newRow = dtCell02.NewRow();
            newRow.ItemArray = new object[] { "6", "LOT050", "NI", "LOT050,LOT051,LOT052", "", "", "" };
            dtCell02.Rows.Add(newRow);


            dgCell02.ItemsSource = DataTableConverter.Convert(dtCell02);

            //foreach (C1.WPF.DataGrid.DataGridColumn column in dgCell02.Columns)
            //{
            //    DataGridMergeExtension.SetMergeMode(column, DataGridMergeMode.VERTICAL);
            //}
            //dgCell02.ReMerge();

            dgCell02.MergingCells += dtCell02_MergingCells;

        }

        #endregion

        #region Event

        private void dgData_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHECKBOXCOLUMN02":
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHECKBOXCOLUMN02", true);
                                chk.IsChecked = true;
                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHECKBOXCOLUMN02", false);
                                    }
                                }
                                break;
                        }
                    }
                }
            }));
        }

        private void dgData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHECKBOXCOLUMN01"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgData.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHECKBOXCOLUMN01"] = true;
            }
            dgData.ItemsSource = DataTableConverter.Convert(dt);


            //for (int idx = 0; idx < dgData.Rows.Count; idx++)
            //{
            //    C1.WPF.DataGrid.DataGridRow row = dgData.Rows[idx];
            //    DataTableConverter.SetValue(row.DataItem, "CHECKBOXCOLUMN01", true);
            //}
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgData.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHECKBOXCOLUMN01"] = true;
            }
            dgData.ItemsSource = DataTableConverter.Convert(dt);

            //for (int idx = 0; idx < dgData.Rows.Count; idx++)
            //{
            //    C1.WPF.DataGrid.DataGridRow row = dgData.Rows[idx];
            //    DataTableConverter.SetValue(row.DataItem, "CHECKBOXCOLUMN01", false);
            //}
        }

        private void chkPageFixed_Checked(object sender, RoutedEventArgs e)
        {
            FrameOperation.PageFixed(true);
        }

        private void chkPageFixed_Unchecked(object sender, RoutedEventArgs e)
        {
            FrameOperation.PageFixed(false);
        }

        private void trvTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
          
        }

        private void btnExcleload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    LoadExcelHelper.LoadExcel(dgExcleload, stream, 0);
                }
            }
        }

        private void btnExcleDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExcleReport_Click(object sender, RoutedEventArgs e)
        {
            int fontsize = 20;
            //=================================================================================
            sty_Board = new XLStyle(xlbook);
            sty_Board.SetBorderStyle(XLLineStyleEnum.Thin);// = XLLineStyleEnum.Medium;
            sty_Board.BorderBottom = XLLineStyleEnum.Thin;
            sty_Board.BorderLeft = XLLineStyleEnum.Thin;
            sty_Board.BorderRight = XLLineStyleEnum.Thin;
            sty_Board.BorderTop = XLLineStyleEnum.Thin;
            sty_Board.BorderColorBottom = Color.FromArgb(255, 255, 0, 0);
            sty_Board.BorderColorLeft = Color.FromArgb(255, 255, 0, 0);
            sty_Board.BorderColorRight = Color.FromArgb(255, 255, 0, 0);
            sty_Board.BorderColorTop = Color.FromArgb(255, 255, 0, 0);
            sty_Board.AlignVert = XLAlignVertEnum.Top;
            sty_Board.Font = new XLFont("Arial", fontsize, false, false);
            sty_Board.WordWrap = true;
            //=================================================================================
            sty_NoBoard = new XLStyle(xlbook);
            sty_NoBoard.AlignVert = XLAlignVertEnum.Top;
            sty_NoBoard.WordWrap = true;
            //=================================================================================

            xlbook.Sheets.RemoveAt(0);
            xlbook.Sheets.Add("Sheet01");

            xlsheet = xlbook.Sheets["Sheet01"];

            xlsheet.PrintSettings.MarginLeft = 0.45;
            xlsheet.PrintSettings.MarginRight = 0.45;
            xlsheet.PrintSettings.MarginBottom = 0.5;

            MergeXLCell(xlsheet, 1, 2, 2, 5, 8, "AAAAAAAAAAAA", "C");

            //==================================================================================
            WriteableBitmap img = new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/LGC.GMES.MES.ProtoType01;component/Images/GMES_icon.ico")));
            xlsheet[0, 0].Value = img;
            //==================================================================================
            XLPictureShape pic = new XLPictureShape(img, 3000, 3500, 2500, 900);
            pic.Rotation = 30.0f;
            pic.LineColor = Colors.DarkRed;
            pic.LineWidth = 100;
            xlsheet.Shapes.Add(pic);
            //==================================================================================

            xlbook.Save(@"c:\mybook.xls");

            System.Diagnostics.Process.Start(@"c:\mybook.xls");
        }

        private void btnButton01_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(txtBasAttrName.Text, null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //LGC.GMES.MES.CMM001.Class.Util.SetDataGridCurrentCell(dgData, dgData[6, 3]);
        }

        private void btnButton02_Click(object sender, RoutedEventArgs e)
        {
            //===================================================================================================
            if (dgData.Resources.Contains("ExportRemove"))
            {
                dgData.Resources.Remove("ExportRemove");
            }
            List<int> exportremove01 = new List<int>() { 1, 2 };
            dgData.Resources.Add("ExportRemove", exportremove01);
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgData);

            
            //===================================================================================================
            C1DataGrid[] dataGridArray = new C1DataGrid[2];
            dataGridArray[0] = dgData;
            dataGridArray[1] = dgDataColor;
            string[] excelTabNameArray = new string[2] {"Main", "Detail"};
            //===================================================================================================
            if (dgData.Resources.Contains("ExportRemove"))
            {
                dgData.Resources.Remove("ExportRemove");
            }
            List<int> exportremove02 = new List<int>() { 1, 2 };
            dgData.Resources.Add("ExportRemove", exportremove02);

            if (dgDataColor.Resources.Contains("ExportRemove"))
            {
                dgDataColor.Resources.Remove("ExportRemove");
            }
            List<int> exportremove03 = new List<int>() { 1, 2 };
            dgDataColor.Resources.Add("ExportRemove", exportremove03);
            //===================================================================================================
            new LGC.GMES.MES.Common.ExcelExporter().Export(dataGridArray, excelTabNameArray);
        }

        private void btnButton03_Click(object sender, RoutedEventArgs e)
        {
            Logger.Instance.WriteLine("[FRAME] Logger Test ================================", Logger.MESSAGE_OPERATION_START);
        }

        private void btnButton04_Click(object sender, RoutedEventArgs e)
        {
            string zpl = @"^XA
            ^LH100,50^FS
            ^SEE:UHANGUL.DAT^FS
            ^CW1,E:KFONT3.FNT^CI26^FS
            ^FO0,0^GB1120,1120,4^FS
            ^FO2,80^GB1116,0,3^FS
            ^FO2,160^GB1116,0,3^FS
            ^FO2,240^GB1116,0,3^FS
            ^FO2,320^GB1116,0,3^FS
            ^FO2,400^GB1116,0,3^FS
            ^FO2,480^GB1116,0,3^FS
            ^FO2,560^GB1116,0,3^FS
            ^FO2,640^GB1116,0,3^FS
            ^FO2,720^GB1116,0,3^FS
            ^FO2,800^GB1116,0,3^FS
            ^FO2,860^GB1116,0,3^FS
            ^FO230,2^GB0,800,3^FS
            ^FO788,2^GB0,80,3^FS
            ^FO558,2^GB0,80,3^FS
            ^FO558,160^GB0,640,3^FS
            ^FO788,160^GB0,640,3^FS
            ^FO18,20^A0N,50,50^FDLOT ID^FS
            ^FO18,100^A0N,50,50^FDPlateNo^FS
            ^FO18,180^A0N,50,50^FDModel^FS
            ^FO18,260^A0N,50,50^FDThickness^FS
            ^FO18,340^A0N,50,50^FDBTIR^FS
            ^FO18,420^A0N,50,50^FDLGD-F1^FS
            ^FO18,500^A0N,50,50^FDUFM-F1^FS
            ^FO18,580^A0N,50,50^FDUFM-F2^FS
            ^FO18,660^A1N,40,40^FD제조사^FS
            ^FO18,740^A1N,40,40^FD현재공정^FS
            ^FO570,20^A1N,35,40^FD자재LOT^FS
            ^FO570,180^A0N,50,50^FDSize X,Y^FS
            ^FO570,260^A0N,50,50^FDFTIR^FS
            ^FO570,340^A0N,50,50^FDTTV^FS
            ^FO570,420^A1N,35,40^FDLGD 판정^FS
            ^FO570,500^A1N,40,35^FDUFM1 판정^FS
            ^FO570,580^A1N,35,40^FDSize X,Y^FS
            ^FO570,660^A1N,35,40^FD출하일자^FS
            ^FO570,740^A1N,35,40^FD출하담당자^FS
            ^FO500,810^A0N,50,50^FDRemark^FS
            ^FO248,20^A0N,50,50^FDN15A001^FS
            ^FO248,180^A0N,50,50^FDQ85120F20L^FS
            ^FO815,15^A0N,50,50^FDPP15A001^FS
            ^XZ";

            //string zpl = @"^XA
            //               ^LH30,30
            //               ^FO20,10^AD^FDZEBRA^FS
            //               ^FO20,60^B3N,Y,20,N^FDAAA001^FS
            //               ^XZ"; //300 DPI

            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
            {
                if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    bool brtndefault = FrameOperation.Barcode_ZPL_Print(dr, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print 실폐"));
                    }
                }
            }
        }

        private void btnButton05_Click(object sender, RoutedEventArgs e)
        {
            string zpl = @"^XA
            ^LH100,50^FS
            ^SEE:UHANGUL.DAT^FS
            ^CW1,E:KFONT3.FNT^CI26^FS
            ^FO0,0^GB1120,1120,4^FS
            ^FO2,80^GB1116,0,3^FS
            ^FO2,160^GB1116,0,3^FS
            ^FO2,240^GB1116,0,3^FS
            ^FO2,320^GB1116,0,3^FS
            ^FO2,400^GB1116,0,3^FS
            ^FO2,480^GB1116,0,3^FS
            ^FO2,560^GB1116,0,3^FS
            ^FO2,640^GB1116,0,3^FS
            ^FO2,720^GB1116,0,3^FS
            ^FO2,800^GB1116,0,3^FS
            ^FO2,860^GB1116,0,3^FS
            ^FO230,2^GB0,800,3^FS
            ^FO788,2^GB0,80,3^FS
            ^FO558,2^GB0,80,3^FS
            ^FO558,160^GB0,640,3^FS
            ^FO788,160^GB0,640,3^FS
            ^FO18,20^A0N,50,50^FDLOT ID^FS
            ^FO18,100^A0N,50,50^FDPlateNo^FS
            ^FO18,180^A0N,50,50^FDModel^FS
            ^FO18,260^A0N,50,50^FDThickness^FS
            ^FO18,340^A0N,50,50^FDBTIR^FS
            ^FO18,420^A0N,50,50^FDLGD-F1^FS
            ^FO18,500^A0N,50,50^FDUFM-F1^FS
            ^FO18,580^A0N,50,50^FDUFM-F2^FS
            ^FO18,660^A1N,40,40^FD제조사^FS
            ^FO18,740^A1N,40,40^FD현재공정^FS
            ^FO570,20^A1N,35,40^FD자재LOT^FS
            ^FO570,180^A0N,50,50^FDSize X,Y^FS
            ^FO570,260^A0N,50,50^FDFTIR^FS
            ^FO570,340^A0N,50,50^FDTTV^FS
            ^FO570,420^A1N,35,40^FDLGD 판정^FS
            ^FO570,500^A1N,40,35^FDUFM1 판정^FS
            ^FO570,580^A1N,35,40^FDSize X,Y^FS
            ^FO570,660^A1N,35,40^FD출하일자^FS
            ^FO570,740^A1N,35,40^FD출하담당자^FS
            ^FO500,810^A0N,50,50^FDRemark^FS
            ^FO248,20^A0N,50,50^FDN15A001^FS
            ^FO248,180^A0N,50,50^FDQ85120F20L^FS
            ^FO815,15^A0N,50,50^FDPP15A001^FS
            ^XZ";

            FrameOperation.PrintFrameMessage(string.Empty);
            bool brtndefault = FrameOperation.Barcode_ZPL_Print(LoginInfo.CFG_SERIAL_PRINT.Rows[0], zpl);
            if (brtndefault == false)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print 실폐"));
            }
        }

        private void btnButton06_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("10007", 1000, 2000), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1101"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("10008"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            );
        }

        private void chkd_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            Viewbox view = chk.Parent as Viewbox;
            StackPanel sp = view.Parent as StackPanel;

            C1TreeViewItem trvi = sp.FindParent<C1TreeViewItem>();



            System.Diagnostics.Debug.Write("");
        }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.Write("");
        }

        private void dgData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                string strtmp = string.Format(@"CurrentCell.Row.Index : {0} CurrentCell.Column.Index : {1}", dgData.CurrentCell.Row.Index, dgData.CurrentCell.Column.Index);
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(strtmp);
            }));
        }

        private void dgData_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //Grid Resources 이용한 Border 그리기
                    e.Cell.Presenter.Background = null;

                    int rowidx = e.Cell.Row.Index - dataGrid.TopRows.Count;
                    e.Cell.Presenter.BorderThickness = new Thickness(0);
                    e.Cell.Presenter.LeftLineBrush = null;
                    e.Cell.Presenter.TopLineBrush = null;
                    e.Cell.Presenter.RightLineBrush = null;
                    e.Cell.Presenter.BottomLineBrush = null;
                    if (dataGrid.Resources.Contains("RowBorderInfo"))
                    {
                        Dictionary<int, System.Windows.Media.Brush> rowborderInfo = dataGrid.Resources["RowBorderInfo"] as Dictionary<int, System.Windows.Media.Brush>;

                        if (rowborderInfo.ContainsKey(rowidx))
                        {
                            e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 0, 1);
                            e.Cell.Presenter.LeftLineBrush = null;
                            e.Cell.Presenter.TopLineBrush = null;
                            e.Cell.Presenter.RightLineBrush = null;
                            e.Cell.Presenter.BottomLineBrush = rowborderInfo[rowidx];
                        }
                    }

                    int colidx = e.Cell.Column.Index;
                    if (dataGrid.Resources.Contains("ColBorderInfo"))
                    {
                        Dictionary<int, System.Windows.Media.Brush> colborderInfo = dataGrid.Resources["ColBorderInfo"] as Dictionary<int, System.Windows.Media.Brush>;

                        if (colborderInfo.ContainsKey(colidx))
                        {
                            if (dataGrid.Resources.Contains("RowBorderInfo"))
                            {
                                Dictionary<int, System.Windows.Media.Brush> rowborderInfo = dataGrid.Resources["RowBorderInfo"] as Dictionary<int, System.Windows.Media.Brush>;

                                if (rowborderInfo.ContainsKey(rowidx))
                                {
                                    e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 0, 1);
                                    e.Cell.Presenter.LeftLineBrush = null;
                                    e.Cell.Presenter.TopLineBrush = null;
                                    e.Cell.Presenter.RightLineBrush = colborderInfo[colidx];
                                    e.Cell.Presenter.BottomLineBrush = rowborderInfo[rowidx];
                                }
                                else
                                {
                                    e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 0);
                                    e.Cell.Presenter.LeftLineBrush = null;
                                    e.Cell.Presenter.TopLineBrush = null;
                                    e.Cell.Presenter.RightLineBrush = colborderInfo[colidx];
                                    e.Cell.Presenter.BottomLineBrush = null;
                                }
                            }
                        }
                    }
                    //Grid Resources 이용한 Background 색 변경
                    if (dataGrid.Resources.Contains("BackgroundInfo"))
                    {
                        Dictionary<int, System.Windows.Media.Brush> backgroundInfo = dataGrid.Resources["BackgroundInfo"] as Dictionary<int, System.Windows.Media.Brush>;
                        if (backgroundInfo.ContainsKey(rowidx))
                        {
                            e.Cell.Presenter.Background = backgroundInfo[rowidx];
                        }
                    }
                    //if (e.Cell.Presenter.Content.GetType() == typeof(System.Windows.Controls.TextBlock))
                    //{
                    //    TextBlock tb = e.Cell.Presenter.Content as TextBlock;
                    //    if (tb != null)
                    //    {
                    //        Thickness thickness = tb.Margin;
                    //        thickness.Left = -2;
                    //        thickness.Right = -2;
                    //        tb.Margin = thickness;
                    //    }
                    //}
                }
            }));
        }

        private void dgData_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgDataColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string strcolor =Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BACKGROUNDCOLOR"));
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strcolor)); ;
                }
            }));
        }

        private void dgDataColor_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void ldpDatePicker01_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnC1ListBox_Click(object sender, RoutedEventArgs e)
        {
            bool chk = Convert.ToBoolean(DataTableConverter.GetValue(lstC1ListBox.SelectedItem, "CHK"));
            string col1 = Convert.ToString(DataTableConverter.GetValue(lstC1ListBox.SelectedItem, "COL1"));
            string col2 = Convert.ToString(DataTableConverter.GetValue(lstC1ListBox.SelectedItem, "COL2"));
            string col3 = Convert.ToString(DataTableConverter.GetValue(lstC1ListBox.SelectedItem, "COL3"));

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(chk.ToString() + col1 + col2 + col3), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

        }

        private void dgData_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {


        }

        private void dgData_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHECKBOXCOLUMN01":
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);

                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                            if (!chk.IsChecked.Value)
                            {
                                chkAll.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHECKBOXCOLUMN01"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll.IsChecked = true;
                            }

                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            break;
                    }
                }
            }


        }

        private void btnExcleLoadData01_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    LoadExcelHelper.LoadExcelData(dgExcleloadData, stream, 0, 1);
                }
            }
        }

        #endregion

        #region Mehod

        private void Get_Data()
        {
            SetGridCboItem(dgData.Columns["COMBOBOXCOLUMN"]);

            dtData = new DataTable();

            dtData.Columns.Add("CHECKBOXCOLUMN01", typeof(bool));
            dtData.Columns.Add("CHECKBOXCOLUMN02", typeof(bool));
            dtData.Columns.Add("TEXTCOLUMN01", typeof(string));
            dtData.Columns.Add("TEXTCOLUMN02", typeof(string));
            dtData.Columns.Add("TEXTCOLUMN03", typeof(string));
            dtData.Columns.Add("TEXTCOLUMN04", typeof(string));
            dtData.Columns.Add("MULTILANG", typeof(string));
            dtData.Columns.Add("NUMERICCOLUMN01", typeof(decimal));
            dtData.Columns.Add("NUMERICCOLUMN02", typeof(decimal));
            dtData.Columns.Add("NUMERICCOLUMN03", typeof(decimal));
            dtData.Columns.Add("NUMERICCOLUMN04", typeof(decimal));
            dtData.Columns.Add("COMBOBOXCOLUMN", typeof(string));

            DataRow newRow = null;

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, false, "Text01", "Text02", "Text03", "Text04", @"ko-KR\한글01|en-US\English01|zh-CN\中文01", 1, 2, 3, 4, "CODE01" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, true, "Text01", "Text02", "Text03", "Text04", @"ko-KR\한글02|en-US\영어02|zh-CN\중문02", 1, 2, 3, 4, "CODE02" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, false, "Text01", "Text02", "Text03", "Text04", @"ko-KR\한글03|en-US\영어03|zh-CN\중문03", 1, 2, 3, 4, "CODE03" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, false, "Text11", "Text02", "Text03", "Text04", @"ko-KR\한글04|en-US\영어04|zh-CN\중문04", 1, 2, 3, 4, "CODE01" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, false, "Text11", "Text02", "Text03", "Text04", @"ko-KR\한글05|en-US\영어05|zh-CN\중문05", 1, 2, 3, 4, "CODE02" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, false, "Text21", "Text02", "Text03", "Text04", @"ko-KR\한글06|en-US\영어06|zh-CN\중문06", 1, 2, 3, 4, "CODE03" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { true, false, "Text21", "Text02", "Text03", "Text04", @"ko-KR\한글06|en-US\영어06|zh-CN\중문06", 1, 2, 3, 4, "CODE01" };
            dtData.Rows.Add(newRow);

            dgData.ItemsSource = DataTableConverter.Convert(dtData);

            //==========================================================================================================
            Dictionary<int, System.Windows.Media.Brush> rowborderInfo = new Dictionary<int, System.Windows.Media.Brush>();

            System.Windows.Media.Brush rowborderColor;

            rowborderColor = new SolidColorBrush(Colors.Red);
            rowborderInfo.Add(1, rowborderColor);

            rowborderColor = new SolidColorBrush(Colors.Red);
            rowborderInfo.Add(3, rowborderColor);

            if (dgData.Resources.Contains("RowBorderInfo"))
            {
                dgData.Resources.Remove("RowBorderInfo");
            }

            dgData.Resources.Add("RowBorderInfo", rowborderInfo);
            //==========================================================================================================
            Dictionary<int, System.Windows.Media.Brush> colborderInfo = new Dictionary<int, System.Windows.Media.Brush>();

            System.Windows.Media.Brush colborderColor;

            colborderColor = new SolidColorBrush(Colors.SteelBlue);
            colborderInfo.Add(1, colborderColor);

            colborderColor = new SolidColorBrush(Colors.SteelBlue);
            colborderInfo.Add(3, colborderColor);

            if (dgData.Resources.Contains("ColBorderInfo"))
            {
                dgData.Resources.Remove("ColBorderInfo");
            }

            dgData.Resources.Add("ColBorderInfo", colborderInfo);
            //==========================================================================================================
            Dictionary<int, System.Windows.Media.Brush> backgroundInfo = new Dictionary<int, System.Windows.Media.Brush>();

            System.Windows.Media.Brush backgroundInfoColor;

            backgroundInfoColor = new SolidColorBrush(Colors.SteelBlue);
            backgroundInfo.Add(1, backgroundInfoColor);

            backgroundInfoColor = new SolidColorBrush(Colors.SteelBlue);
            backgroundInfo.Add(3, backgroundInfoColor);

            if (dgData.Resources.Contains("BackgroundInfo"))
            {
                dgData.Resources.Remove("BackgroundInfo");
            }

            dgData.Resources.Add("BackgroundInfo", backgroundInfo);
            //==========================================================================================================
            //if (dgData.Resources.Contains("ExportRemove"))
            //{
            //    dgData.Resources.Remove("ExportRemove");
            //}
            //List<int> exportremove01 = new List<int>() { 0 };
            //dgData.Resources.Add("ExportRemove", exportremove01);
        }

        private void Get_DataColor()
        {
            dtData = new DataTable();

            dtData.Columns.Add("TEXTCOLUMN01", typeof(string));
            dtData.Columns.Add("TEXTCOLUMN02", typeof(string));
            dtData.Columns.Add("TEXTCOLUMN03", typeof(string));
            dtData.Columns.Add("DATE", typeof(DateTime));
            dtData.Columns.Add("MONTH", typeof(DateTime));
            dtData.Columns.Add("BACKGROUNDCOLOR", typeof(string));

            DataRow newRow = null;

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now, "#FFDC143C" };
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now , "#FFFFFFFF"};
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now , "#FFDC143C"};
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now , "#FFFFFFFF"};
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now , "#FFDC143C"};
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now , "#FFFFFFFF"};
            dtData.Rows.Add(newRow);

            newRow = dtData.NewRow();
            newRow.ItemArray = new object[] { "Text01", "Text02", "Text03", System.DateTime.Now, System.DateTime.Now , "#FFDC143C"};
            dtData.Rows.Add(newRow);

            dgDataColor.ItemsSource = DataTableConverter.Convert(dtData);
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            DataRow newRow = null;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "CODE01", "Combo 01" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "CODE02", "Combo 02" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "CODE03", "Combo 03" };
            dt.Rows.Add(newRow);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }

        private StackPanel SetC1TreeViewItemHeader(string item)
        {
            StackPanel sp = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            CheckBox chk = new CheckBox();
            chk.Checked += chkd_Checked;
            chk.Unchecked += chk_Unchecked;
            Viewbox view = new Viewbox();
            view.MaxWidth = 20;
            view.MaxHeight = 20;
            view.Child = chk;
            sp.Children.Add(view);
            TextBlock txt = new TextBlock();
            txt.Text = item;
            sp.Children.Add(txt);
            return sp;
        }

        private void btnSaveFileDialog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog od = new SaveFileDialog();
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                od.InitialDirectory = @"\\Client\C$";
            }
            od.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (od.ShowDialog() == true)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(od.FileName.ToString(), null, "Infomation", MessageBoxButton.OK, MessageBoxIcon.Warning);



            }
        }

        private void MergeXLCell(XLSheet xlsheet, int style, int x1, int x2, int y1, int y2, string value, string align)
        {
            // style : 0 - no ,  1 - board , 2 - board + color
            XLCellRange range = new XLCellRange();
            range = new XLCellRange(x1, x2, y1, y2);

            range.Style = sty_Board;
            xlsheet.MergedCells.Add(range);

            for (int i = x1; i < x2 + 1; i++)
            {
                xlsheet.Rows[i].Height = xlsheet.DefaultRowHeight; // DefaultRowHeight 
                for (int j = y1; j < y2 + 1; j++)
                {
                    if (style == 2)
                    {
                        xlsheet[i, j].Style = sty_Board.Clone();
                        xlsheet[i, j].Style.BackColor = Color.FromArgb(100, 242, 242, 242);
                        //xlsheet[i, j].Style.Font = new XLFont("Arial", 9, false, false);
                    }
                    else if (style == 1)
                    {
                        xlsheet[i, j].Style = sty_Board.Clone();
                    }
                    else
                    {
                        xlsheet[i, j].Style = sty_NoBoard.Clone();
                    }
                }
            }

            xlsheet[x1, y1].Value = value;

            if (align.Equals("R"))
            {
                xlsheet[x1, y1].Style.AlignHorz = XLAlignHorzEnum.Right;
            }
            else if (align.Equals("L"))
            {
                xlsheet[x1, y1].Style.AlignHorz = XLAlignHorzEnum.Left;
            }
            else
            {
                xlsheet[x1, y1].Style.AlignHorz = XLAlignHorzEnum.Center;
            }
        }

        void dtCell02_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            for (int idx01 = 0; idx01 < dg.Rows.Count - 1; idx01 = idx01 + 2)
            {
                e.Merge(new DataGridCellsRange(dg.GetCell(idx01, 0), dg.GetCell(idx01 + 1, 0)));
            }

            for (int idx01 = 1; idx01 < dg.Columns.Count; idx01++)
            {
                if (idx01 % 2 == 0)
                {
                    for (int idx02 = 1; idx02 < dg.Rows.Count -1; idx02 = idx02 + 2)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(idx02, idx01), dg.GetCell(idx02 + 1, idx01)));
                    }
                }
                else
                {
                    for (int idx02 = 0; idx02 < dg.Rows.Count - 1; idx02 = idx02 + 2)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(idx02, idx01), dg.GetCell(idx02 + 1, idx01)));
                    }
                }
            }

        }

        private void DataGridMultiLangColumn_FilterLoading(object sender, DataGridFilterLoadingEventArgs e)
        {
            C1.WPF.DataGrid.DataGridFilter dgfilter = new C1.WPF.DataGrid.DataGridFilter();
            dgfilter.InnerControl = new C1.WPF.DataGrid.DataGridTextFilter();
            e.Filter = dgfilter;

            //C1.WPF.DataGrid.DataGridTextFilter txtfilter = new C1.WPF.DataGrid.DataGridTextFilter();
            //txtfilter.Filter = new C1.WPF.DataGrid.DataGridFilterState();
            //var filter = new C1.WPF.DataGrid.Filters.DataGridContentFilter
            //{
            //    Content = new C1.WPF.DataGrid.Filters.DataGridFilterList
            //    {
            //        Items = { txtfilter }
            //    }
            //};
            //e.Filter = filter;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //throw new ArgumentNullException("Exception");
            //dgData.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);
        }

        private void btnButton32_Click(object sender, RoutedEventArgs e)
        {
            string zpl = "^XA"
                           + "^LH{0},{1}"                      // 좌표
                           + "^FO50,20^A0N,20,30^FD{2}^FS"     // 날짜
                           + "^FO150,20^A0N,20,30^FD{5}^FS"    // 모델
                           + "^FO230,20^A0N,20,30^FD{6}^FS"    // 장비
                           + "^FO330,20^A0N,20,30^FD{3}^FS"    // 수량
                           + "^FO50,50^BY2,2.5,100^B3N,N,100,N,N^FD{4}^FS"     // BARCODE
                           + "^FO70,170^A0N,45,60^FD{4}^FS"                    // BARCODE TEXT
                           + "^PQ1"
                           + "^XZ";

            FrameOperation.PrintFrameMessage(string.Empty);
            bool brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(LoginInfo.CFG_SERIAL_PRINT.Rows[1], zpl);
            if (brtndefault == false)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print 실폐"));
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        #endregion
    }

}