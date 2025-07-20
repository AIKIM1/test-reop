/*************************************************************************************
 Created Date : 2017.02.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2017.02.14  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_QUALITY_PACKAGING : C1Window, IWorkArea
    {
        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _LOTID = string.Empty;
        private string _WIPSEQ = string.Empty;
        private string _PRODID = string.Empty;
        private DataTable _dtDimension;
        private int iDimensionSeq = 1;

        #region Declaration & Constructor 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_QUALITY_PACKAGING()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 4)
            {
                _PROCID = tmps[0].ToString();
                _EQPTID = tmps[1].ToString();
                _LOTID = tmps[2].ToString();
                _WIPSEQ = tmps[3].ToString();
                _PRODID = tmps[4].ToString();
            }
            // 
            InitHeader();
            SearchData();
        }
        #endregion

        #region Mehod
        private void InitHeader()
        {
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgQualityInfo.Columns)
            {
                string header = col.Header.ToString();
                col.Header = ObjectDic.Instance.GetObjectName(header).Replace(@"\r\n", "\r\n");
            }

            foreach (C1.WPF.DataGrid.DataGridColumn col in dgQualityInfoSealing.Columns)
            {
                string header = col.Header.ToString();
                col.Header = ObjectDic.Instance.GetObjectName(header).Replace(@"\r\n", "\r\n");
            }
        }

        private void SearchData()
        {
            SearchQualityInfo();
            SearchDimension();
            SearchSealing();
        }

        private void SearchQualityInfo()
        {
            DataTable dtRqst = new DataTable("INDATA");
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("WIPSEQ", typeof(string));
            dtRqst.Columns.Add("CLCITEM_LIST", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PRODID"] = _PRODID;
            dr["LOTID"] = _LOTID;
            dr["WIPSEQ"] = _WIPSEQ;
            dr["CLCITEM_LIST"] = "SI054-001,SI055-001,SI056-001,SI056-002";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_QUALITY_INFO", "INDATA", "OUTDATA", dtRqst);

            Util.GridSetData(dgQualityInfo, dtRslt, FrameOperation, true);
        }

        private void SearchDimension()
        {
            DataTable dtRqst = new DataTable("INDATA");
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("WIPSEQ", typeof(string));
            dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
            dtRqst.Columns.Add("NEST_LIST", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROCID"] = _PROCID;
            dr["LOTID"] = _LOTID;
            dr["WIPSEQ"] = _WIPSEQ;
            dr["CLCTITEM_CLSS4"] = "B";
            dr["NEST_LIST"] = "Nest1,Nest2,Nest3,Nest4,Nest5,Nest6,Nest7,Nest8";
            dtRqst.Rows.Add(dr);

            DataTable dtRsltDimension = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DIMENSION", "INDATA", "OUTDATA", dtRqst);
            DataTable dtRsltLot = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", dtRqst);
            DataTable dtRslt = GenerateTransposedTable(dtRsltDimension, dtRsltLot);

            Util.GridSetData(dgQualityInfoDimen, dtRslt, FrameOperation, true);
            // dgQualityInfoDimen.TopRows.Add()
        }

        private void SearchSealing()
        {
            DataTable dtRqst = new DataTable("INDATA");
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("WIPSEQ", typeof(string));
            dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));


            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROCID"] = _PROCID;
            dr["LOTID"] = _LOTID;
            dr["WIPSEQ"] = _WIPSEQ;
            dr["CLCTITEM_CLSS4"] = "C";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SEALING", "INDATA", "OUTDATA", dtRqst);
            Util.GridSetData(dgQualityInfoSealing, dtRslt, FrameOperation, true);

        }
        #endregion

        private void dgQualityInfo_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
            {
                var _mergeList = new List<DataGridCellsRange>();
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(2, 0), grid.GetCell(2, 1)));
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(3, 0), grid.GetCell(3, 1)));

                foreach (var range in _mergeList)
                {
                    e.Merge(range);
                }
            }
        }


        private DataTable GenerateTransposedTable(DataTable inputTable, DataTable lotInfo)
        {
            DataTable outputTable = new DataTable();
            _dtDimension = new DataTable();
            outputTable.Columns.Add(inputTable.Columns[0].ColumnName.ToString());
            _dtDimension.Columns.Add(inputTable.Columns[0].ColumnName.ToString());

            foreach (DataRow inRow in inputTable.Rows)
            {
                string newColName = inRow[0].ToString();
                outputTable.Columns.Add(newColName);
                _dtDimension.Columns.Add(newColName);
            }

            for (int rCount = 1; rCount <= inputTable.Columns.Count - 1; rCount++)
            {
                DataRow newRow = outputTable.NewRow();
                DataRow drDimension = _dtDimension.NewRow();

                newRow[0] = inputTable.Columns[rCount].ColumnName.ToString();
                for (int cCount = 0; cCount <= inputTable.Rows.Count - 1; cCount++)
                {
                    string colValue = inputTable.Rows[cCount][rCount].ToString();

                    if (rCount < 3) newRow[cCount + 1] = colValue;

                    else
                    {
                        List<DataRow> drInfo = lotInfo.Select("CLCTITEM = '" + colValue + "'", "CLCTSEQ DESC").ToList();
                        if (drInfo.Count > 0)
                        {
                            newRow[cCount + 1] = drInfo[0]["CLCTVAL01"];
                            iDimensionSeq = Util.NVC_Int(drInfo[0]["CLCTSEQ"]);
                        }
                    }
                    drDimension[cCount + 1] = colValue;
                }
                outputTable.Rows.Add(newRow);
                _dtDimension.Rows.Add(drDimension);
            }

            return outputTable;
        }

        private void dgQualityInfoDimen_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
            {
                var _mergeList = new List<DataGridCellsRange>();
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(0, 3), grid.GetCell(0, 4)));
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(0, 5), grid.GetCell(0, 8)));
                _mergeList.Add(new DataGridCellsRange(grid.GetCell(0, 9), grid.GetCell(0, 12)));

                foreach (var range in _mergeList)
                {
                    e.Merge(range);
                }
            }

        }



        private void dgQualityInfoDimen_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Row.Index < dataGrid.FrozenTopRowsCount)
                e.Cancel = true;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveQuality();
        }

        #region [저장]
        private void SaveQuality()
        {
            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                int idxitem = dgQualityInfo.Columns["INSP_CLCTITEM"].Index;
                int idxSealingitem = dgQualityInfoSealing.Columns["INSP_CLCTITEM"].Index;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                //  dtRqst.Columns.Add("CLCTMAX", typeof(string));
                //  dtRqst.Columns.Add("CLCTMIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                // [인장]탭 저장
                for (int col = dgQualityInfo.FrozenColumnCount; col < 14; col++)
                {
                    for (int row = 0; row < dgQualityInfo.Rows.Count; row++)
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = _LOTID;
                        dr["WIPSEQ"] = _WIPSEQ;
                        dr["CLCTSEQ"] = dgQualityInfo.Columns[col].Name;
                        dr["CLCTITEM"] = dgQualityInfo.GetCell(row, idxitem).Text;
                        dr["CLCTVAL01"] = dgQualityInfo.GetCell(row, col).Text;
                        dr["EQPTID"] = _EQPTID;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }
                }

                DataTable dtRqstDimension = dtRqst.Clone();
                //Dimension           
                for (int row = dgQualityInfoDimen.FrozenTopRowsCount; row < dgQualityInfoDimen.Rows.Count; row++)
                {
                    //  ++seq;
                    for (int col = dgQualityInfoDimen.FrozenColumnCount; col < dgQualityInfoDimen.Columns.Count; col++)
                    {
                        DataRow dr = dtRqstDimension.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = _LOTID;
                        dr["WIPSEQ"] = _WIPSEQ;
                        dr["CLCTSEQ"] = iDimensionSeq;
                        dr["CLCTITEM"] = _dtDimension.Rows[row][col];
                        dr["CLCTVAL01"] = dgQualityInfoDimen.GetCell(row, col).Text;
                        dr["EQPTID"] = _EQPTID;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqstDimension.Rows.Add(dr);
                    }
                }

                //  _dtDimension
                //   dgQualityInfoDimen.ItemsSource


                DataTable dtRqstSealing = dtRqst.Clone();

                // [Sealing]탭 저장
                for (int col = dgQualityInfoSealing.FrozenColumnCount; col < 6; col++)
                {
                    for (int row = 0; row < dgQualityInfoSealing.Rows.Count; row++)
                    {
                        DataRow dr = dtRqstSealing.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = _LOTID;
                        dr["WIPSEQ"] = _WIPSEQ;
                        dr["CLCTSEQ"] = dgQualityInfoSealing.Columns[col].Name;
                        dr["CLCTITEM"] = dgQualityInfoSealing.GetCell(row, idxSealingitem).Text;
                        dr["CLCTVAL01"] = dgQualityInfoSealing.GetCell(row, col).Text;
                        dr["EQPTID"] = _EQPTID;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqstSealing.Rows.Add(dr);
                    }
                }


                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);
                DataTable dtRsltDimension = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqstDimension);
                DataTable dtRsltSealing = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqstSealing);
                Util.Alert("SFU1270");      //저장되었습니다.

                SearchData();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        private void dgQualityInfoDimen_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Index < dataGrid.FrozenTopRowsCount)
                {
                    e.Cell.Presenter.Background = dataGrid.HeaderBackground;
                }

                if (e.Cell.Column.Index == 0)
                {
                    e.Cell.Presenter.Background = dataGrid.HeaderBackground;
                }
            }));
        }

        private void dgQualityInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Index < dataGrid.FrozenTopRowsCount)
                {
                    e.Cell.Presenter.Background = dataGrid.HeaderBackground;
                }

                if (e.Cell.Column.Index <= 1)
                {
                    e.Cell.Presenter.Background = dataGrid.HeaderBackground;
                }
            }));
        }

        private void dgQualityInfoSealing_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Index < dataGrid.FrozenTopRowsCount)
                {
                    e.Cell.Presenter.Background = dataGrid.HeaderBackground;
                }

                if (e.Cell.Column.Index <= 1)
                {
                    e.Cell.Presenter.Background = dataGrid.HeaderBackground;
                }
            }));
        }
    }
}