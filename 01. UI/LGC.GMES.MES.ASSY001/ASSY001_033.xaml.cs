/*************************************************************************************
 Created Date : 2017.05.25
      Creator : ������S
   Decription : Folding,Packaging �ܰ��˻�
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.24  ������S : Initial Created.




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using System.Windows.Data;
using C1.WPF;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using Microsoft.Win32;
using C1.WPF.Excel;
using System.Configuration;
using System.IO;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_033 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private DateTime _dtSystemTime = new DateTime();
        private readonly int _rowCount = 24;
        private readonly int _dataBeginRowIndex = 2;
        private readonly int _dataBeginColumnIndex = 2;
        private int _inspColumnIndex = 0;
        private int _sumExptColumnIndex = 0;
        private int _totalColumnIndex = 0;
        private int _noteBeginIndex = 0;
        private readonly string CMCDTYPE = "VISUAL_INSPECTION_INPUT_TYPE";
        private string COM_CODE = "";
        private string attr1 = "";
        private DataTable loadedSearchResult = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_033()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitCombo()
        {
            SetEquipmentSegmentCombo(cboEquipmentSegment);

            SetProcessCombo(cboProcess);

            SetEquipmentCombo(cboEquipment);

            cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
           
            cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

            cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

            SetFontSize();
        }

        #endregion

        #region Event

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            LoadDataGrid();
            ApplyPermissions();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ( cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString() == "SELECT" )
            {
                Util.MessageValidation("SFU1673");
                return;
            }
            LoadDataGrid();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if ( cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString() == "SELECT" )
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            //�ҷ������� ���� �Ͻðڽ��ϱ�?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if ( result == MessageBoxResult.OK )
                {
                    dgVislInsp.EndEdit();
                    dgVislInsp.EndEditRow(true);

                    UpdateVislInspData(dgVislInsp);
                }
            });
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDataGrid();
        }

        private void dgVislInsp_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid C1dt = sender as C1DataGrid;

            if ( e.Cell.Value is string )
                e.Cell.Value = Util.NVC(e.Cell.Value) == "" ? null : e.Cell.Value;
            else
                e.Cell.Value = Util.NVC_Int(e.Cell.Value) <= 0 ? null : e.Cell.Value;

            int rowIndex = e.Cell.Row.Index;

            //OK ����
            if ( e.Cell.Column.Index % 2 == 0 )
            {
                int sum = 0;
                for ( int i = _dataBeginColumnIndex ; i < _inspColumnIndex ; i += 2 )
                    sum += Util.NVC_Int(C1dt[rowIndex, i].Value);

                //�˻���� ����
                if ( attr1 == "GN" )
                {
                    if ( sum > 0 )
                        dgVislInsp[rowIndex, _totalColumnIndex].Value = sum;
                    else
                        dgVislInsp[rowIndex, _totalColumnIndex].Value = null;
                }

                if ( sum > 0 )
                    dgVislInsp[rowIndex, _inspColumnIndex].Value = sum;
                else
                    dgVislInsp[rowIndex, _inspColumnIndex].Value = null;


                if ( attr1 == "GN" )
                {
                    //�����հ� ����
                    int sumExecpt = 0;
                    for ( int i = _dataBeginColumnIndex ; i < _inspColumnIndex ; i += 2 )
                    {
                        bool sum_excl_flag = ((from t in loadedSearchResult.AsEnumerable()
                                               where t.Field<string>("CLCTITEM") == GetCLCTITEM(dgVislInsp.Columns[i].Name)
                                               select t.Field<string>("SUM_EXCL_FLAG")).FirstOrDefault<string>() == "Y") ? true : false;

                        //�ش��׸��� ���������׸��� ���
                        if ( sum_excl_flag )
                            sumExecpt += Util.NVC_Int(dgVislInsp[rowIndex, i].Value);
                    }
                    if ( sumExecpt > 0 )
                        dgVislInsp[rowIndex, _sumExptColumnIndex].Value = sumExecpt;
                    else
                        dgVislInsp[rowIndex, _sumExptColumnIndex].Value = null;

                    //����(���������׸� ������) ����
                    if ( sum - sumExecpt > 0 )
                        dgVislInsp[rowIndex, _totalColumnIndex].Value = sum - sumExecpt;
                    else
                        dgVislInsp[rowIndex, _totalColumnIndex].Value = null;
                }
            }

            //NG ����
            else
            {
                int sum = 0;
                for ( int i = _dataBeginColumnIndex + 1 ; i < _inspColumnIndex ; i += 2 )
                    sum += Util.NVC_Int(C1dt[rowIndex, i].Value);

                //�˻���� ����
                if ( attr1 == "GN" )
                {
                    if ( sum > 0 )
                        dgVislInsp[rowIndex, _totalColumnIndex + 1].Value = sum;
                    else
                        dgVislInsp[rowIndex, _totalColumnIndex + 1].Value = null;
                }

                if ( sum > 0 )
                    dgVislInsp[rowIndex, _inspColumnIndex + 1].Value = sum;
                else
                    dgVislInsp[rowIndex, _inspColumnIndex + 1].Value = null;


                if ( attr1 == "GN" )
                {
                    //�����հ� ����
                    int sumExecpt = 0;
                    for ( int i = _dataBeginColumnIndex + 1 ; i < _inspColumnIndex ; i += 2 )
                    {
                        bool sum_excl_flag = ((from t in loadedSearchResult.AsEnumerable()
                                               where t.Field<string>("CLCTITEM") == GetCLCTITEM(dgVislInsp.Columns[i].Name)
                                               select t.Field<string>("SUM_EXCL_FLAG")).FirstOrDefault<string>() == "Y") ? true : false;

                        //�ش��׸��� ���������׸��� ���
                        if ( sum_excl_flag )
                            sumExecpt += Util.NVC_Int(dgVislInsp[rowIndex, i].Value);
                    }
                    if ( sumExecpt > 0 )
                        dgVislInsp[rowIndex, _sumExptColumnIndex + 1].Value = sumExecpt;
                    else
                        dgVislInsp[rowIndex, _sumExptColumnIndex + 1].Value = null;

                    //����(���������׸� ������) ����
                    if ( sum - sumExecpt > 0 )
                        dgVislInsp[rowIndex, _totalColumnIndex + 1].Value = sum - sumExecpt;
                    else
                        dgVislInsp[rowIndex, _totalColumnIndex + 1].Value = null;
                }

            }
        }

        private void btnCellExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgVislInsp);
            }
            catch ( Exception ex )
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgVislInsp_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if ( sender == null )
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if ( e.Cell.Presenter == null )
                {
                    return;
                }

                //Grid Data Binding �̿��� Background �� ����
                if ( e.Cell.Row.Type == DataGridRowType.Item )
                {
                    if ( e.Cell.Column.Name.Equals("INSP_COUNT_NG")
                    || e.Cell.Column.Name.Equals("TOTAL_COUNT_NG")
                    || e.Cell.Column.Name.Equals("INSP_COUNT_OK")
                    || e.Cell.Column.Name.Equals("TOTAL_COUNT_OK") )
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#EEEEEE"));
                    else
                        e.Cell.Presenter.Background = null;
                }
            }));
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboProcess);
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if ( cboEquipment.SelectedValue == null
                || cboEquipment.SelectedValue.ToString() == "SELECT"
                || (Util.NVC(e.OldValue) == Util.NVC(e.NewValue)) )
                return;
            LoadDataGrid();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            COM_CODE = (string)cboProcess.SelectedValue;
            setAttr1Value();
            SetEquipmentCombo(cboEquipment);
        }

        private void dgVislInsp_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if ( !dgVislInsp.CurrentCell.IsEditing )
                return;
             
            if ( _dataBeginColumnIndex <= dgVislInsp.CurrentCell.Column.Index
                 && dgVislInsp.CurrentCell.Column.Index < _inspColumnIndex )
            {
                if ( e.Delta > 0 )
                    dgVislInsp.CurrentCell.Value = Util.NVC_Int(dgVislInsp.CurrentCell.Value) + 1;
                else
                {
                    if ( Util.NVC_Int(dgVislInsp.CurrentCell.Value) > 0 )
                        dgVislInsp.CurrentCell.Value = Util.NVC_Int(dgVislInsp.CurrentCell.Value) - 1;
                }
                if ( Util.NVC_Int(dgVislInsp.CurrentCell.Value) == 0 )
                    dgVislInsp.CurrentCell.Value = null;
            }
        }

        #endregion

        #region Method

        private void LoadDataGrid()
        {

            dgVislInsp.Columns.Clear();
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                //CONTENTä���
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("INSP_DATE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["INSP_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_VISL_INSP", "INDATA", "OUTDATA", inTable);

                loadedSearchResult = searchResult.Copy();

                loadingIndicator.Visibility = Visibility.Collapsed;

                //�ð�,�ð��ڵ� COL
                DataTable dt = new DataTable();
                dt.Columns.Add("TIME", typeof(string));
                dt.Columns.Add("TIME_ELPS_CODE", typeof(string));

                //�ߺ��� �÷� ����
                var columns = (from t in searchResult.AsEnumerable()
                               select new { CLCTNAME = t.Field<string>("CLCTNAME"), CLCTIEM = t.Field<string>("CLCTITEM") }).Distinct().ToList();

                //�˻�,���κҸ�,���� COL
                _inspColumnIndex = _dataBeginColumnIndex + columns.Count * 2;
                _sumExptColumnIndex = _inspColumnIndex + 2;
                _totalColumnIndex = _sumExptColumnIndex + 2;
                _noteBeginIndex = _inspColumnIndex + 2;

                //�ߺ������� �÷��� �߰�(�÷���+_OK,�÷���+_NG)
                foreach ( var col in columns )
                {
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = col.CLCTIEM + "_OK",
                        Caption = col.CLCTNAME,
                        DataType = typeof(int)                        
                    });
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = col.CLCTIEM + "_NG",
                        Caption = col.CLCTNAME,
                        DataType = typeof(int)
                    });
                }

                //�˻���� COL �߰�
                dt.Columns.Add(new DataColumn()
                {
                    ColumnName = "INSP_COUNT" + "_OK",
                    Caption = "�˻����",
                    DataType = typeof(int)
                });
                dt.Columns.Add(new DataColumn()
                {
                    ColumnName = "INSP_COUNT" + "_NG",
                    DataType = typeof(int)
                });

                if ( attr1 == "GN" )
                {
                    //�����հ� COL �߰�
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = "SUM_EXCEPT" + "_OK",
                        DataType = typeof(int)
                    });
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = "SUM_EXCEPT" + "_NG",
                        DataType = typeof(int)
                    });

                    //�հ� COL �߰�
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = "TOTAL_COUNT" + "_OK",
                        DataType = typeof(int)
                    });
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = "TOTAL_COUNT" + "_NG",
                        DataType = typeof(int)
                    });
                }

                else
                {
                    //��ġ��� COL �߰�
                    dt.Columns.Add(new DataColumn()
                    {
                        ColumnName = "NOTE",
                        DataType = typeof(string)
                    });
                }

                //ROW����ֱ�
                for ( int i = 0 ; i < _rowCount ; i++ )
                {
                    DataRow dr = dt.NewRow();

                    //�ð��ֱ�
                    if ( i + 6 < _rowCount )
                    {
                        dr["TIME"] = string.Format("{0:00}:00~{1:00}:00", i + 6, i + 7);
                        dr["TIME_ELPS_CODE"] = string.Format("{0:00}{1:00}", i + 6, i + 7);
                    }

                    else
                    {
                        dr["TIME"] = string.Format("{0:00}:00~{1:00}:00", i - 18, i - 17);
                        dr["TIME_ELPS_CODE"] = string.Format("{0:00}{1:00}", i - 18, i - 17);
                    }

                    for ( int j = _dataBeginColumnIndex ; j < _inspColumnIndex ; j += 2 )
                    {
                        var queryEdit = (from t in searchResult.AsEnumerable()
                                         where t.Field<string>("CLCTITEM") == GetCLCTITEM(dt.Columns[j].ToString())
                                     && t.Field<string>("TIME_ELPS_CODE") == dr["TIME_ELPS_CODE"].ToString()
                                         select new
                                         {
                                             DFCT_QTY = t.Field<decimal?>("DFCT_QTY"),
                                             GOOD_QTY = t.Field<decimal?>("GOOD_QTY")
                                         }).ToList();

                        foreach ( var item in queryEdit )
                        {
                            if ( item.GOOD_QTY == null ) { dr[j] = DBNull.Value; }
                            else { dr[j] = item.GOOD_QTY; }
                            if ( item.DFCT_QTY == null ) { dr[j + 1] = DBNull.Value; }
                            else { dr[j + 1] = item.DFCT_QTY; }
                        }
                    }

                    if ( attr1 == "GN" )
                    {
                        int sumExptOK = (from t in searchResult.AsEnumerable()
                                         where t.Field<string>("TIME_ELPS_CODE") == dr["TIME_ELPS_CODE"].ToString()
                                         && t.Field<string>("SUM_EXCL_FLAG") == "Y"
                                         select t.Field<decimal?>("GOOD_QTY")).Sum<decimal?>(e => e == null ? 0 : (int)e.Value);
                        int sumExptNG = (from t in searchResult.AsEnumerable()
                                         where t.Field<string>("TIME_ELPS_CODE") == dr["TIME_ELPS_CODE"].ToString()
                                         && t.Field<string>("SUM_EXCL_FLAG") == "Y"
                                         select t.Field<decimal?>("DFCT_QTY")).Sum<decimal?>(e => e == null ? 0 : (int)e.Value);

                        if ( sumExptOK == 0 )
                            dr[_sumExptColumnIndex] = DBNull.Value;
                        else
                            dr[_sumExptColumnIndex] = sumExptOK;

                        if ( sumExptNG == 0 )
                            dr[_sumExptColumnIndex + 1] = DBNull.Value;
                        else
                            dr[_sumExptColumnIndex + 1] = sumExptNG;
                    }

                    dt.Rows.Add(dr);
                }

                //Grid�� ������ SETTING
                SetGridData(dt);

                //�˻���� SETTING
                SetInspCountColumn();

                if ( attr1 == "GN" )
                    //���� SETTING
                    SetTotalCountColumn();

                else if ( attr1 == "N" )
                    //PACK�� ��� ��Ʈ������ LOAD
                    SetNote();

                else { }

                // 2017-07-07 alternating row color ����
                dgVislInsp.RowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
                dgVislInsp.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.LightGray);

                //if (txtFont.Text == "")
                //{
                //    dgVislInsp.FontSize = 12;
                //}
                //else
                //{
                //    dgVislInsp.FontSize = double.Parse(txtFont.Text);
                //}

                if (cboFontSize.SelectedIndex < 0)
                {
                    dgVislInsp.FontSize = 10;
                }
                else
                {
                    dgVislInsp.FontSize = double.Parse(cboFontSize.SelectedValue.ToString());
                }

            }

            catch ( Exception ex )
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetNote()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("INSP_DATE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["INSP_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
            newRow["PROCID"] = cboProcess.SelectedValue;
            newRow["EQPTID"] = cboEquipment.SelectedValue;

            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_QCA_SEL_VISL_INSP_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if ( searchException != null )
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    for ( int i = _dataBeginRowIndex ; i < _dataBeginRowIndex + _rowCount ; i++ )
                    {
                        dgVislInsp[i, _noteBeginIndex].Value = (from t in searchResult.AsEnumerable()
                                                                where t.Field<string>("TIME_ELPS_CODE") == dgVislInsp[i, 1].Value.ToString()
                                                                select t.Field<string>("NOTE")).FirstOrDefault<string>();
                    }
                }

                catch ( Exception ex )
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageException(ex);
                }
            });
        }

        private void setAttr1Value()
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["COM_TYPE_CODE"] = CMCDTYPE;
            newRow["COM_CODE"] = COM_CODE;

            inTable.Rows.Add(newRow);
            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

            attr1 = (from t in searchResult.AsEnumerable()
                     select t.Field<string>("ATTR1")).FirstOrDefault<string>();
        }

        private void SetTotalCountColumn()
        {
            dgVislInsp.Columns[_totalColumnIndex].IsReadOnly = true;
            dgVislInsp.Columns[_totalColumnIndex + 1].IsReadOnly = true;

            for ( int i = _dataBeginRowIndex ; i < _rowCount + _dataBeginRowIndex ; i++ )
            {
                //OK�κ�
                int total = Util.NVC_Int(dgVislInsp[i, _inspColumnIndex].Value) - Util.NVC_Int(dgVislInsp[i, _sumExptColumnIndex].Value);
                if ( total > 0 )
                    dgVislInsp[i, _totalColumnIndex].Value = total;
                else
                    dgVislInsp[i, _totalColumnIndex].Value = null;

                //NG�κ�
                total = Util.NVC_Int(dgVislInsp[i, _inspColumnIndex + 1].Value) - Util.NVC_Int(dgVislInsp[i, _sumExptColumnIndex + 1].Value);
                if ( total > 0 )
                    dgVislInsp[i, _totalColumnIndex + 1].Value = total;
                else
                    dgVislInsp[i, _totalColumnIndex + 1].Value = null;
            }
        }

        private void SetInspCountColumn()
        {
            dgVislInsp.Columns[_inspColumnIndex].IsReadOnly = true;
            dgVislInsp.Columns[_inspColumnIndex + 1].IsReadOnly = true;

            if ( attr1 == "N" )
            {
                dgVislInsp.Columns[_inspColumnIndex].Visibility = Visibility.Collapsed;
                //OK�κ� Collapsedó��
                for ( int i = _dataBeginColumnIndex ; i < _inspColumnIndex ; i += 2 )
                    dgVislInsp.Columns[i].Visibility = Visibility.Collapsed;
            }

            dgVislInsp.Columns[_inspColumnIndex].DisplayIndex = 2;
            dgVislInsp.Columns[_inspColumnIndex + 1].DisplayIndex = 3;

            for ( int i = _dataBeginRowIndex ; i < _rowCount + _dataBeginRowIndex ; i++ )
            {
                decimal sum = 0;
                //OK�κ�
                for ( int j = _dataBeginColumnIndex ; j < _inspColumnIndex ; j += 2 )
                    sum += Util.NVC_Int(dgVislInsp[i, j].Value);

                if ( sum > 0 )
                    dgVislInsp[i, _inspColumnIndex].Value = sum;
                else
                    dgVislInsp[i, _inspColumnIndex].Value = null;

                //NG�κ�
                sum = 0;
                for ( int j = _dataBeginColumnIndex + 1 ; j < _inspColumnIndex ; j += 2 )
                    sum += Util.NVC_Int(dgVislInsp[i, j].Value);

                if ( sum > 0 )
                    dgVislInsp[i, _inspColumnIndex + 1].Value = sum;
                else
                    dgVislInsp[i, _inspColumnIndex + 1].Value = null;

            }

        }

        private string GetCLCTITEM(string CLCTITEM)
        {
            return CLCTITEM.Substring(0, CLCTITEM.Length - 3);
        }

        private void UpdateVislInspData(C1DataGrid dgVislInsp)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable { };
                inDataTable.Columns.Add("INSP_DATE", typeof(string));
                inDataTable.Columns.Add("TIME_ELPS_CODE", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CLCTITEM", typeof(string));
                inDataTable.Columns.Add("GOOD_QTY", typeof(decimal));
                inDataTable.Columns.Add("DFCT_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));

                //dgVislInsp.EndEdit();

                for ( int i = _dataBeginRowIndex ; i < _dataBeginRowIndex + _rowCount ; i++ )
                {
                    for ( int j = _dataBeginColumnIndex ; j < _inspColumnIndex ; j += 2 )
                    {
                        DataRow dr = inDataTable.NewRow();

                        dr["INSP_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                        dr["PROCID"] = cboProcess.SelectedValue;
                        dr["EQPTID"] = cboEquipment.SelectedValue;
                        dr["TIME_ELPS_CODE"] = dgVislInsp[i, 1].Value;

                        dr["CLCTITEM"] = GetCLCTITEM(dgVislInsp[0, j].Column.Name);

                        dr["GOOD_QTY"] = dgVislInsp[i, j].Value == null ? DBNull.Value : dgVislInsp[i, j].Value;
                        dr["DFCT_QTY"] = dgVislInsp[i, j + 1].Value == null ? DBNull.Value : dgVislInsp[i, j + 1].Value;
                        dr["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(dr);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_VISL_INSP", "INDATA", null, inDataTable);

                if ( attr1 == "N" )
                    UpdateNote();

                Util.MessageInfo("SFU1270");      //����Ǿ����ϴ�.

            }

            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }

            finally
            {
                HiddenLoadingIndicator();
            }

        }

        private void UpdateNote()
        {
            DataTable inDataTable = new DataTable { };
            inDataTable.Columns.Add("INSP_DATE", typeof(string));
            inDataTable.Columns.Add("TIME_ELPS_CODE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            //dgVislInsp.EndEdit();

            for ( int i = _dataBeginRowIndex ; i < _dataBeginRowIndex + _rowCount ; i++ )
            {
                DataRow dr = inDataTable.NewRow();

                dr["INSP_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TIME_ELPS_CODE"] = dgVislInsp[i, 1].Value;
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["NOTE"] = dgVislInsp[i, _noteBeginIndex].Value;
                dr["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);
            }

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_VISL_INSP_NOTE", "INDATA", null, inDataTable);
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, (string)cboEquipmentSegment.SelectedValue };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, (string)cboEquipmentSegment.SelectedValue, (string)cboProcess.SelectedValue, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void SetGridData(DataTable dt)
        {
            for ( int i = 0 ; i < _inspColumnIndex ; i++ )
            {
                DataColumn dc = dt.Columns[i];

                //TIME COL
                if ( i == 0 )
                    dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = dc.ColumnName,
                        Header = "�ð�",
                        Binding = new Binding { Path = new PropertyPath(dc.Caption) },
                        IsReadOnly = true,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        CanUserSort = false,
                        MaxWidth = 180,
                        Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                        CanUserFilter = false                        
                    });

                //TIME_ELPS_CODE COL
                else if ( i == 1 )
                    dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = dc.ColumnName,
                        Visibility = Visibility.Collapsed,
                        Binding = new Binding { Path = new PropertyPath(dc.Caption) },
                    });

                //DYNAMIC COL
                else
                {
                    //OK COL
                    if ( i % 2 == 0 )
                        dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        {
                            AllowNull = true,
                            Header = (attr1 == "GN") ? new string[] { dc.Caption, "OK" }.ToList<string>() :
                                                    new string[] { "�˻���", dc.Caption }.ToList<string>(),
                            Binding = new Binding { Path = new PropertyPath(dc.ColumnName) },
                            IsReadOnly = false,
                            CanUserFilter = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            CanUserSort = false,
                            Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                            ShowButtons = false,
                            
                        });

                    //NG COL
                    else
                        dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        {
                            AllowNull = true,
                            Header = (attr1 == "GN") ? new string[] { dc.Caption, "NG" }.ToList<string>() :
                                                       new string[] { "�˻���", dc.Caption }.ToList<string>(),
                            Binding = new Binding { Path = new PropertyPath(dc.ColumnName) },
                            IsReadOnly = false,
                            CanUserFilter = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            CanUserSort = false,
                            Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                            ShowButtons = false,
                        });
                }
            }

            if ( attr1 == "GN" )
            {
                //�˻���� COL
                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = new string[] { "�˻����", "OK" }.ToList<string>(),
                    Binding = new Binding { Path = new PropertyPath("INSP_COUNT_OK") },
                    IsReadOnly = false,
                    CanUserFilter = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CanUserSort = false,
                    Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                    ShowButtons = false,
                });

                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = new string[] { "�˻����", "NG" }.ToList<string>(),
                    Binding = new Binding { Path = new PropertyPath("INSP_COUNT_NG") },
                    IsReadOnly = false,
                    CanUserFilter = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CanUserSort = false,
                    Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                    ShowButtons = false
                });

                //�����հ���� COL
                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = new string[] { "�����հ�", "OK" }.ToList<string>(),
                    Binding = new Binding { Path = new PropertyPath("SUM_EXCEPT_OK") },
                    Visibility = Visibility.Collapsed
                });

                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = new string[] { "�����հ�", "NG" }.ToList<string>(),
                    Binding = new Binding { Path = new PropertyPath("SUM_EXCEPT_NG") },
                    Visibility = Visibility.Collapsed
                });

                //���� COL
                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = new string[] { "����(���������׸� ������)", "OK" }.ToList<string>(),
                    Binding = new Binding { Path = new PropertyPath("TOTAL_COUNT_OK") },
                    IsReadOnly = false,
                    CanUserFilter = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CanUserSort = false,
                    Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                    ShowButtons = false,

                });

                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = new string[] { "����(���������׸� ������)", "NG" }.ToList<string>(),
                    Binding = new Binding { Path = new PropertyPath("TOTAL_COUNT_NG") },
                    IsReadOnly = false,
                    CanUserFilter = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CanUserSort = false,
                    Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star),
                    ShowButtons = false
                });
            }

            else
            {
                //�˻���� COL
                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Binding = new Binding { Path = new PropertyPath("INSP_COUNT_OK") },
                    Visibility = Visibility.Collapsed
                });

                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    AllowNull = true,
                    Header = "�˻����",
                    Binding = new Binding { Path = new PropertyPath("INSP_COUNT_NG") },
                    IsReadOnly = false,
                    CanUserFilter = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CanUserSort = false,
                    ShowButtons = false
                });

                dgVislInsp.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    AllowNull = true,
                    Header = "��ġ���",
                    Binding = new Binding { Path = new PropertyPath("NOTE") },
                    IsReadOnly = false,
                    CanUserFilter = true,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = new C1.WPF.DataGrid.DataGridLength(2, DataGridUnitType.Star),
                    CanUserSort = false
                });
            }

            if (cboProcess.SelectedValue.ToString() == "A8000")
            {
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgVislInsp.Columns)
                    //dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    if (Util.NVC(dgc.Header).ToString() == "�ð�")
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    else
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(50);
            }

            dgVislInsp.ItemsSource = dt.AsDataView();

        }

        private void SetFontSize()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

            DataRow row = dt.NewRow();

            for (int icnt = 10; icnt < 31; icnt++)
            {
                row = dt.NewRow();
                row["CBO_CODE"] = icnt;
                row["CBO_NAME"] = icnt;
                dt.Rows.Add(row);
            }

            cboFontSize.ItemsSource = DataTableConverter.Convert(dt);

            cboFontSize.SelectedIndex = 0;

        }

        private void cboFontSize_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            LoadDataGrid();
        }

        #endregion
    }
}
