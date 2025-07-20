/*************************************************************************************
 Created Date : 2019.02.21
      Creator : 비즈테크 이동우 사원
   Decription : SKID 현황 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
   
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_018_SKID_Search : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        private DataTable _dtSkidStateInfo;
        private bool _isSearchResult = false;

        private readonly Util _util = new Util();

        public MCS001_018_SKID_Search()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeSkidStateInfo();
            InitializeCombo();
            object[] parameters = C1WindowExtension.GetParameters( this );
			
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeCombo()
        {
            SetSkidTypeCombo(cboSKIDType);
        }

        private void InitializeSkidStateInfo()
        {
            _dtSkidStateInfo = new DataTable();
            _dtSkidStateInfo.Columns.Add("PCW_TYPE", typeof(string));
            _dtSkidStateInfo.Columns.Add("PJT", typeof(string));
            _dtSkidStateInfo.Columns.Add("C_NORM", typeof(Int32));  //양극 정상
            _dtSkidStateInfo.Columns.Add("C_HOLD", typeof(Int32));  //양극 홀드
            _dtSkidStateInfo.Columns.Add("A_NORM", typeof(Int32));  //음극 정상
            _dtSkidStateInfo.Columns.Add("A_HOLD", typeof(Int32));  //음극 홀드
            _dtSkidStateInfo.Columns.Add("REMAIN", typeof(Int32)); //공SKID 수
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ShowLoadingIndicator();

                Util.gridClear(dgSKIDCondition);
                _dtSkidStateInfo.Clear();

                int subSumC = 0;
                int subSumA = 0;
                int total = 0;
                int remain = 0;

                const string bizRuleName = "BR_MCS_SEL_PCW_SKID_INFO";

                DataSet ds = new DataSet();

                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("LANG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["CSTPROD"] = cboSKIDType.SelectedValue;
                dr["LANG"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA,OUTSUM", ds);

                if (CommonVerify.HasTableInDataSet(dsResult))
                {

                    if (CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
                    {
                        _isSearchResult = true;
                    }
                    else
                    {
                        _isSearchResult = false;
                    }
                    

                    if (CommonVerify.HasTableRow(dsResult.Tables["OUTSUM"]))
                    {
                        remain = (int) dsResult.Tables["OUTSUM"].Rows[0]["REMAIN"];
                        subSumA = (int) dsResult.Tables["OUTSUM"].Rows[0]["A_SUBSUM"];
                        subSumC = (int) dsResult.Tables["OUTSUM"].Rows[0]["C_SUBSUM"];
                        total = (int) dsResult.Tables["OUTSUM"].Rows[0]["TOTAL"];
                    }

                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        DataRow newRow = _dtSkidStateInfo.NewRow();
                        newRow["PCW_TYPE"] = dsResult.Tables["OUTDATA"].Rows[i]["PCW_TYPE"];
                        newRow["PJT"] = dsResult.Tables["OUTDATA"].Rows[i]["PJT"];
                        newRow["C_NORM"] = dsResult.Tables["OUTDATA"].Rows[i]["C_NORM"];
                        newRow["C_HOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["C_HOLD"];
                        newRow["A_NORM"] = dsResult.Tables["OUTDATA"].Rows[i]["A_NORM"];
                        newRow["A_HOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["A_HOLD"];
                        newRow["REMAIN"] = remain;

                        _dtSkidStateInfo.Rows.Add(newRow);
                    }

                    // SubTotal, Total
                    if (CommonVerify.HasTableRow(dsResult.Tables["OUTSUM"]))
                    {
                        DataRow subTotalRow = _dtSkidStateInfo.NewRow();
                        subTotalRow["PCW_TYPE"] = "SubTotal";
                        subTotalRow["PJT"] = "SubTotal";
                        subTotalRow["C_NORM"] = subSumC;
                        subTotalRow["C_HOLD"] = subSumC;
                        subTotalRow["A_NORM"] = subSumA;
                        subTotalRow["A_HOLD"] = subSumA;
                        subTotalRow["REMAIN"] = remain;
                        _dtSkidStateInfo.Rows.Add(subTotalRow);

                        DataRow totalRow = _dtSkidStateInfo.NewRow();
                        totalRow["PCW_TYPE"] = "Total";
                        totalRow["PJT"] = "Total";
                        totalRow["C_NORM"] = total;
                        totalRow["C_HOLD"] = total;
                        totalRow["A_NORM"] = total;
                        totalRow["A_HOLD"] = total;
                        totalRow["REMAIN"] = total;
                        _dtSkidStateInfo.Rows.Add(totalRow);
                    }

                    Util.GridSetData(dgSKIDCondition, _dtSkidStateInfo, null, true);


                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cboSKIDType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(btnSearch, null);
        }

        private void dgSKIDCondition_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PCW_TYPE")),"SubTotal") ||string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PCW_TYPE")), "Total"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color) convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        private void dgSKIDCondition_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgSKIDCondition_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;
            

            try
            {

                if (_isSearchResult == false)
                {
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == "SubTotal")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(i, dg.Columns["PCW_TYPE"].Index + 1)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["C_NORM"].Index), dg.GetCell(i, dg.Columns["C_NORM"].Index + 1)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["A_NORM"].Index), dg.GetCell(i, dg.Columns["A_NORM"].Index + 1)));
                        }
                        else if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == "Total")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(i, dg.Columns["PCW_TYPE"].Index + 1)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["C_NORM"].Index), dg.GetCell(i, dg.Columns["C_NORM"].Index + 4)));
                        }
                    }
                }
                else
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        TypeId = x.Field<string>("PCW_TYPE")
                    }).Select(g => new
                    {
                        TypeCode = g.Key.TypeId,
                        Count = g.Count()
                    }).ToList();


                    var queryRemain = dt.AsEnumerable().GroupBy(x => new
                    {
                        Remain = x.Field<Int32>("REMAIN")
                    }).Select(g => new
                    {
                        Remain = g.Key.Remain,
                        Count = g.Count()
                    }).ToList();


                    string previewTypeCode = string.Empty;
                    string previewRemain = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        foreach (var item in query)
                        {
                            int rowIndex = i;

                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString()))
                            {
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == item.TypeCode && previewTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PCW_TYPE"].Index)));
                                }
                            }

                        }

                        foreach (var item in queryRemain)
                        {
                            int rowIndex = i;

                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dg.Rows[i].DataItem, "REMAIN").GetString()))
                            {
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "REMAIN").GetString() == item.Remain.GetString() && previewRemain != DataTableConverter.GetValue(dg.Rows[i].DataItem, "REMAIN").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["REMAIN"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["REMAIN"].Index)));
                                }
                            }
                        }

                        previewTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString();
                        previewRemain = DataTableConverter.GetValue(dg.Rows[i].DataItem, "REMAIN").GetString();
                    }

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == "SubTotal")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(i, dg.Columns["PCW_TYPE"].Index + 1)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["C_NORM"].Index), dg.GetCell(i, dg.Columns["C_NORM"].Index + 1)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["A_NORM"].Index), dg.GetCell(i, dg.Columns["A_NORM"].Index + 1)));
                        }
                        else if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == "Total")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(i, dg.Columns["PCW_TYPE"].Index + 1)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["C_NORM"].Index), dg.GetCell(i, dg.Columns["C_NORM"].Index + 4)));
                        }
                    }
                }







            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private bool ValidationSelectPancakeWareHousing()
        {

            return true;
        }

        private static void SetSkidTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_SKID_TYPE";
            string[] arrColumn = { "LANG"};
            string[] arrCondition = { LoginInfo.LANGID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }






        #endregion


    }
}