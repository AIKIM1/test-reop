/*************************************************************************************
 Created Date : 2016.08.18
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 작업조 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.18  INS 김동일K : Initial Created.
  2017.01.23  유관수K : 조별선택 시간 내보내기, 선택조 자동선택 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_SHIFT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;

        private string _Shop = string.Empty;
        private string _Area = string.Empty;
        private string _Segment = string.Empty;
        private string _Proc = string.Empty;
        private string _Shift = string.Empty;

        Util _Util = new Util();

        public string SHIFTCODE
        {
            get { return _RetShiftCode; }
        }

        public string SHIFTNAME
        {
            get { return _RetShiftName; }
        }

        public string WRKSTRTTIME
        {
            get { return _RetWrkStrtTime; }
        }

        public string WRKENDTTIME
        {
            get { return _RetWrkEndTime; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_SHIFT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 4)
            {
                _Shop = tmps[0].ToString();
                _Area = tmps[1].ToString();
                _Segment = tmps[2].ToString();
                _Proc = tmps[3].ToString();
                if (tmps.Length > 4 && tmps[4] != null)
                {
                    _Shift = tmps[4].ToString();
                }
                else
                {
                    _Shift = "";
                }

            }
            else
            {
                _Shop = "";
                _Area = "";
                _Segment = "";
                _Proc = "";
                _Shift = "";
            }
            GetShift();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (dgShift.SelectedIndex < 0)
            {
                Util.MessageInfo("SFU1275");//선택된 항목이 없습니다.
                return;
            }

            _RetShiftCode = DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_ID").ToString();
            _RetShiftName = DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_NAME").ToString();
            if(DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_STRT_HMS") != null)
            {
                _RetWrkStrtTime = DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_STRT_HMS").ToString();
            }
            else
            {
                _RetWrkStrtTime = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
            }
                
            if (DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_END_HMS") != null)
                _RetWrkEndTime = DataTableConverter.GetValue(dgShift.Rows[dgShift.SelectedIndex].DataItem, "SHFT_END_HMS").ToString();

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgShift_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            if (datagrid.CurrentRow == null || datagrid.CurrentRow.Index < 0)
                return;

            if (_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK") < 0)
            {
                Util.MessageInfo("SFU1275");//선택된 항목이 없습니다.
                return;
            }

            _RetShiftCode = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_ID").ToString();
            _RetShiftName = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_NAME").ToString();
            if (DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_STRT_HMS") != null)
            {
                _RetWrkStrtTime = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_STRT_HMS").ToString();
            }
            else
            {
                _RetWrkStrtTime = string.Format("{0:yyyy-MM-dd HH:mm}", DateTime.Now);
            }
            if (DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_END_HMS") != null)
            {
                _RetWrkEndTime = DataTableConverter.GetValue(datagrid.Rows[_Util.GetDataGridCheckFirstRowIndex(datagrid, "CHK")].DataItem, "SHFT_END_HMS").ToString();
            }

            this.DialogResult = MessageBoxResult.OK;
        }

        private void dgShift_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dgShift.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }
        private void dgShiftChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgShift.SelectedIndex = idx;
            }
        }

        private void dgShift_Checked()
        {
            for (int i = 0; i < dgShift.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgShift.Rows[i].DataItem, "SHFT_ID").Equals(_Shift))  //_Shift
                {
                    DataTableConverter.SetValue(dgShift.Rows[i].DataItem, "CHK", true);
                    dgShift.SelectedIndex = i;
                }
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetShift()
        {
            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("LANGID", typeof(string));
            searchConditionTable.Columns.Add("SHOPID", typeof(string));
            searchConditionTable.Columns.Add("AREAID", typeof(string));
            searchConditionTable.Columns.Add("EQSGID", typeof(string));
            searchConditionTable.Columns.Add("PROCID", typeof(string));

            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = _Shop;
            searchCondition["AREAID"] = _Area;
            searchCondition["EQSGID"] = _Segment;
            searchCondition["PROCID"] = _Proc;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("DA_BAS_SEL_SHIFT3", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.AlertByBiz("DA_BAS_SEL_SHIFT3", searchException.Message, searchException.ToString());
                        return;
                    }

                    dgShift.ItemsSource = DataTableConverter.Convert(searchResult);
                    dgShift.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    dgShift_Checked();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        #endregion

        #endregion
    }
}