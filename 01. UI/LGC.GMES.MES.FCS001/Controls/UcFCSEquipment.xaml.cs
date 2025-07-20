/*************************************************************************************
 Created Date : 2020.10.12
      Creator : Kang Dong Hee
   Decription : 활성화 공정진척 - 설비 Tree 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  NAME : Initial Created
  2021.04.19  KDH  : 설비 정보 출력 내용 변경
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using System.Reflection;

namespace LGC.GMES.MES.FCS001.Controls
{
    /// <summary>
    /// UcFCSEquipment.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFCSEquipment : UserControl
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public C1DataGrid DgEquipment { get; set; }

        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private DataTable _dtEquipment;
        private string _equipmentCode;

        public UcFCSEquipment()
        {
            InitializeComponent();

            InitializeControls();
            SetControl();
            SetButtons();
            SetControlVisibility();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            Util.gridClear(dgEquipment);

            _dtEquipment = new DataTable();
        }

        private void SetControl()
        {
            DgEquipment = dgEquipment;
        }
        private void SetButtons()
        {
        }

        public void SetControlVisibility()
        {
        }

        #endregion

        #region Event

        private void dgEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    int Seq = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SEQ"));
                    string inputYN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_YN"));
                    string Val001 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL001"));             // 1.PRJT_NAME, 2.SHFT_NAME, 3.대 Lot(Title), 4.Foil(Title), 5.Slurry (Top)(Title), 6.Slurry (Back)(Title)
                    string Val002 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL002"));             // 1.PROD_VER_CODE, 2.WRK_USERNAME, 3.대Lot, 4.Foil, 5.Slurry (Top), 6.Slurry (Top)

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                    ///////////////////////////////////////////////////////////////////////////////////

                    //2021.04.19 설비 정보 출력 내용 변경 START
                    // 설비 On-Line여부 배경색 표시 : 녹색 – On-line / 적색 – Off-line
                    string EqptID = e.Cell.Row.DataItem.ToString().Split(':')[0].Trim();
                    DataRow[] dr = _dtEquipment.Select("EQPTID = '" + EqptID + "' And SEQ = 0");

                    if (dr.Length > 0)
                    {
                        if (dr[0]["EQPT_ONLINE_FLAG"].ToString() == "Y")
                        {
                            e.Cell.Row.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff67e09c"));
                        }
                        else
                        {
                            e.Cell.Row.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fffe7a7a"));
                        }
                    }
                    //if (Seq == 2)
                    //{
                    //    #region 작업조, 작업자   
                    //    if (e.Cell.Column.Name.ToString() == "VAL002")
                    //    {
                    //        if (inputYN == "Y")
                    //        {
                    //            e.Cell.Presenter.FontStyle = FontStyles.Italic;
                    //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                    //        }
                    //    }
                    //    else if (e.Cell.Column.Name.ToString() == "VAL006")
                    //    {
                    //        e.Cell.Presenter.Cursor = Cursors.Hand;

                    //        if (inputYN == "Y")
                    //        {
                    //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    //        }
                    //        else
                    //        {
                    //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //        }

                    //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //    }

                    //    // ToolTip 작업자의 작업시작일시 ~ 작업종료일시
                    //    if (e.Cell.Column.Name.ToString() == "VAL001")
                    //        ToolTipService.SetToolTip(e.Cell.Presenter, Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_STRT_DTTM")) + " ~ " + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_END_DTTM")));

                    //    #endregion
                    //}

                    //if (e.Cell.Row.ParentGroup.Presenter == null)
                    //{
                    //    return;
                    //}

                    //if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                    //{
                    //    // 설비 On-Line여부 배경색 표시 : 녹색 – On-line / 적색 – Off-line
                    //    string EqptID = e.Cell.Row.ParentGroup.DataItem.ToString().Split(':')[0].Trim();
                    //    DataRow[] dr = _dtEquipment.Select("EQPTID = '" + EqptID + "' And SEQ = 0");

                    //    if (dr.Length > 0)
                    //    {
                    //        if (dr[0]["EQPT_ONLINE_FLAG"].ToString() == "Y")
                    //        {
                    //            e.Cell.Row.ParentGroup.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff67e09c"));
                    //        }
                    //        else
                    //        {
                    //            e.Cell.Row.ParentGroup.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fffe7a7a"));
                    //        }
                    //    }
                    //}
                    //2021.04.19 설비 정보 출력 내용 변경 END

                }
            }));

        }

        private void dgEquipment_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            for (int row = 0; row < dg.Rows.Count; row++)
            {
                int Seq = Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[row].DataItem, "SEQ"));

                if (Seq == 2)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                }
            }
        }

        private void dgEquipment_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null || dg.SelectedIndex == -1) return;

            if (dg.CurrentCell.Column.Name == "VAL002")
            {
                int Seq = Util.NVC_Int(dg.GetCell(dg.CurrentRow.Index, dg.Columns["SEQ"].Index).Value);

                if (Seq == 4)
                {
                    e.Cancel = false;   // Editing 가능
                }
                else
                {
                    e.Cancel = true;    // Editing 불가능
                }

            }
        }

        private void dgEquipment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEquipment.GetCellFromPoint(pnt);

            if (cell == null) return;

            // 선택한 셀의 Row 위치
            int rowIdx = cell.Row.Index;
            DataRowView dv = dgEquipment.Rows[rowIdx].DataItem as DataRowView;

            if (dv == null) return;

            _equipmentCode = dv["EQPTID"].ToString();

            // 자재 장착, 자재 탈착
            int seq = Util.NVC_Int(dv["SEQ"]);
        }

        private void dgEquipment_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEquipment.GetCellFromPoint(pnt);

            if (cell != null)
            {
                DataRowView dr = dgEquipment.Rows[cell.Row.Index].DataItem as DataRowView;

                int Seq = Util.NVC_Int(dr["SEQ"]);
                string Shft_ID = Util.NVC(dr["SHFT_ID"]);
                string WorkUser = Util.NVC(dr["WRK_USERID"]);
                string Val001 = Util.NVC(dr["VAL001"]);
                string Val002 = Util.NVC(dr["VAL002"]);

                if (Seq == 2)
                {
                    if (cell.Column.Name.ToString() == "VAL006")
                    {
                        // 팝업호출 : 작업자
                        PopupWorker(Shft_ID, WorkUser);
                    }
                }
            }

        }

        private void dgFoilChoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    int row = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    int col = ((DataGridCellPresenter)rb.Parent).Column.Index;

                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null)
                    {
                        if ((bool)rb.IsChecked)
                        {
                            if (col == dgEquipment.Columns["VAL005"].Index)
                            {
                                (dgEquipment.GetCell(row, dgEquipment.Columns["VAL003"].Index).Presenter.Content as RadioButton).IsChecked = false;

                                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[row].DataItem, "VAL003", false);
                                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[row].DataItem, "VAL005", true);
                            }
                            else
                            {
                                (dgEquipment.GetCell(row, dgEquipment.Columns["VAL005"].Index).Presenter.Content as RadioButton).IsChecked = false;

                                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[row].DataItem, "VAL003", true);
                                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[row].DataItem, "VAL005", false);
                            }
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

        #region [BizCall]
        public void SetApplyPermissions()
        {
            // 추가작성 필요~~~~~~~~~
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void ChangeEquipment(string processCode, string equipmentCode, bool bEquipmentTable = false)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["PROCID"] = processCode;
                newRow["EQPTID"] = equipmentCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_FCS_EQUIPMENT_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgEquipment, bizResult, null);
                        _dtEquipment = bizResult.Copy();

                        //dgEquipment.GroupBy(dgEquipment.Columns["EQPTNAME"], DataGridSortDirection.None); //2021.04.19 설비 정보 출력 내용 변경
                        //dgEquipment.GroupRowPosition = DataGridGroupRowPosition.AboveData; //2021.04.19 설비 정보 출력 내용 변경

                        // 설비 Table 재생성
                        SetUserControlEquipmentDataTable();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion;

        #region [Func]
        protected virtual void SetUserControlEquipmentDataTable()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetUserControlEquipmentDataTable");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                methodInfo.Invoke(UcParentControl, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void SetUserControlProductionResult()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetUserControlProductionResult");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                methodInfo.Invoke(UcParentControl,null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        #endregion;

        #region 팝업
        /// <summary>
        /// 작업자 팝업
        /// </summary>
        private void PopupWorker(string Shft_ID, string WorkUser)
        {
            CMM_SHIFT_USER2_DRB popupWorker = new CMM_SHIFT_USER2_DRB();
            popupWorker.FrameOperation = this.FrameOperation;

            if (popupWorker != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = EquipmentSegmentCode;
                Parameters[3] = ProcessCode;
                Parameters[4] = Shft_ID;
                Parameters[5] = WorkUser;
                Parameters[6] = _equipmentCode;
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(popupWorker, Parameters);

                popupWorker.Closed += new EventHandler(PopupWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorker.ShowModal()));
            }
        }

        private void PopupWorker_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2_DRB popup = sender as CMM_SHIFT_USER2_DRB;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ChangeEquipment(ProcessCode, EquipmentCode, true);
            }
        }

        #endregion

        #endregion

    }
}
