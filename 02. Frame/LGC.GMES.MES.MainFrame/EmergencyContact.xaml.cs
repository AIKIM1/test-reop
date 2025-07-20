using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.MainFrame
{
    /// <summary>
    /// EmergencyContact.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EmergencyContact : C1Window
    {
        #region Constructor
        public EmergencyContact()
        {
            InitializeComponent();
            Loaded += EmergencyContact_Loaded;
        }
        #endregion Constructor

        #region Events
        private void EmergencyContact_Loaded(object sender, RoutedEventArgs e)
        {
            GetGmesChargerList();
            GetEquipmentChargerList();

            lblAlarm.Content = MessageDic.Instance.GetMessage("SFU4442");   //  보안 정책상 전화번호 미표시. GPortal 검색 후 연락 요망.
        }
        #endregion Events

        #region Fucntion
        void GetGmesChargerList()
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_EMRG_CONTACT", "INDATA", "RSLTDT", IndataTable);
            dgGmes.ItemsSource = DataTableConverter.Convert(dt);
        }

        void GetEquipmentChargerList()
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_EQPT_EMRG_CONTACT", "INDATA", "RSLTDT", IndataTable);
            dgEquipment.ItemsSource = DataTableConverter.Convert(dt); 
        }

        void MergeCells(C1DataGrid _grid, DataGridMergingCellsEventArgs e)
        {
            var _mergeList = new List<DataGridCellsRange>();

            if (_grid.Rows.Count > _grid.TopRows.Count)
            {
                for (int j = 0; j < _grid.Columns.Count; j++)
                {
                    int startRowIndex = 0;

                    for (int i = _grid.TopRows.Count; i < _grid.Rows.Count; i++)
                    {
                        if (i.Equals(_grid.TopRows.Count))
                        {
                            startRowIndex = i;
                        }
                        else if (i > _grid.TopRows.Count)
                        {
                            DataRowView drvOld = _grid.Rows[i - 1].DataItem as DataRowView;
                            DataRowView drvNew = _grid.Rows[i].DataItem as DataRowView;

                            if (!drvNew[j].ToString().Trim().Equals(drvOld[j].ToString().Trim()))
                            {
                                if (!startRowIndex.Equals(i)&&!startRowIndex.Equals(i - 1))
                                    _mergeList.Add(new DataGridCellsRange(_grid.GetCell(startRowIndex, j), _grid.GetCell(i - 1, j)));

                                startRowIndex = i;
                            }

                            if (i.Equals(_grid.Rows.Count - 1) && drvNew[j].ToString().Trim().Equals(drvOld[j].ToString().Trim()))
                                _mergeList.Add(new DataGridCellsRange(_grid.GetCell(startRowIndex, j), _grid.GetCell(i, j)));
                        }
                    }
                }

                foreach (var range in _mergeList)
                    e.Merge(range);
            }
        }
        #endregion Function

        private void dgGmes_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            MergeCells(sender as C1DataGrid, e);
        }

        private void dgEquipment_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            MergeCells(sender as C1DataGrid, e);
        }

        private void dgGmes_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Text.Contains(","))
            {
                var _textBlock = e.Cell.Presenter.Content as TextBlock;
                _textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                _textBlock.Text = e.Cell.Text.Replace(",", Environment.NewLine);
            }
        }

        private void dgEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Text.Contains(","))
            {
                var _textBlock = e.Cell.Presenter.Content as TextBlock;
                _textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                _textBlock.Text = e.Cell.Text.Replace(",", Environment.NewLine);
            }
        }
    }
}