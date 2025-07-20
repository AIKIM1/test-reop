/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_360 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        int _maxColumn = 0;

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _lotID = string.Empty;         // lot
        private string _wipSeq = string.Empty;
        private string _actdttm = string.Empty;
        private string _clctseq = string.Empty;
        private string _sSELFTYPE = string.Empty;

        private string ValueToSheet = string.Empty;
        DataTable _clctItem;
        public COM001_360()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
            //CboProcess_SelectedItemChanged(null, null);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            //cboEquipment.SelectedItemChanged += cboEquipment_SelectedItemChanged;
            //cboEquipment_SelectedItemChanged(null, null);

        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            //AddNoteGrid(dgdNote);
            //AddNoteGrid(dgdNoteMulti);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgQuality);


            GetSpecLotList();
        }

        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                //GetSheet();
            }
        }

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                return;
            }
            //GetSheet();
        }

       

        private string GetSelfInspSheet()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_PROCESSEQUIPMENTSEG", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    foreach (DataRow row in dtRslt.Rows)
                        return Util.NVC(row["SELF_INSP_REG_UI_TYPE_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return "";
        }

        private void GetSelfInspItemId()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dtRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dtRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_ITEMID_CBO", "INDATA", "OUTDATA", inTable);

                DataTable dt = new DataTable();
                dt.Columns.Add(cboClctItem.DisplayMemberPath.ToString(), typeof(string));
                dt.Columns.Add(cboClctItem.SelectedValuePath.ToString(), typeof(string));

                dt = dtRslt.Copy();

                DataRow drSelect = dt.NewRow();
                drSelect[cboClctItem.DisplayMemberPath.ToString()] = "-ALL-";
                drSelect[cboClctItem.SelectedValuePath.ToString()] = "";
                dt.Rows.InsertAt(drSelect, 0);

                cboClctItem.DisplayMemberPath = cboClctItem.DisplayMemberPath.ToString();
                cboClctItem.SelectedValuePath = cboClctItem.SelectedValuePath.ToString();
                cboClctItem.ItemsSource = dt.Copy().AsDataView();
                cboClctItem.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                {
                    string ColName = e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 2, 2).ToString();
                    int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_COUNT"));
                    int ColCnt = 0;

                    bool InputCheck = true;

                    if (int.TryParse(ColName, out ColCnt))
                    {
                        if (ColCnt > MaxCount)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));

                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                        n.IsEnabled = false;

                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                        n.IsEnabled = false;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                        n.IsEnabled = false;
                                        int idx = n.SelectedIndex;

                                        n.SelectedIndex = idx == -1 ? 0 : idx;
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;

                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                        int idx = n.SelectedIndex;

                                        n.SelectedIndex = idx == -1 ? 0 : idx;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                        if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                        if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                            {
                                if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                {
                                    C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                                else if (panel.Children[cnt].GetType().Name == "TextBox")
                                {
                                    TextBox n = panel.Children[cnt] as TextBox;
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                                else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                {
                                    ComboBox n = panel.Children[cnt] as ComboBox;
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                    int idx = n.SelectedIndex;

                                    n.SelectedIndex = idx == -1 ? 0 : idx;
                                }
                            }
                        }
                    }
                }

            }));
        }

        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                        if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                        {
                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                        int idx = n.SelectedIndex;

                                        n.SelectedIndex = idx == -1 ? 0 : idx;
                                    }
                                }
                            }
                        }
                    }
                }
            }));
        }

        private void dgQualityMulti_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                {
                    bool InputCheck = true;

                    e.Cell.Presenter.Background = null;
                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                    if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                    if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if (panel.Children[cnt].Visibility == Visibility.Visible)
                        {
                            if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                            {
                                C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                n.IsEnabled = true;
                            }
                            else if (panel.Children[cnt].GetType().Name == "TextBox")
                            {
                                TextBox n = panel.Children[cnt] as TextBox;
                                n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                n.IsEnabled = true;                        
                            }
                            else if (panel.Children[cnt].GetType().Name == "ComboBox")
                            {
                                ComboBox n = panel.Children[cnt] as ComboBox;
                                n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                n.IsEnabled = true;
                                int idx = n.SelectedIndex;

                                n.SelectedIndex = idx == -1 ? 0 : idx;
                            }
                        }
                    }
                }

            }));
        }

        private void dgQualityMulti_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                        if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                        {
                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                        int idx = n.SelectedIndex;

                                        n.SelectedIndex = idx == -1 ? 0 : idx;
                                    }
                                }
                            }
                        }
                    }
                }
            }));
        }

        private void dgQuality_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            dgQuality.Focus();
        }

        private void dgQuality_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            dgQuality.Focus();
        }



        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            //if (cbo.IsVisible)
            //    cbo.SelectedIndex = 0;
        }

        private void dgSpecLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;
            DataRowView drv = rb.DataContext as DataRowView;

            //최초 체크시에만 로직 타도록 구현
            //if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
            {
                //체크시 처리될 로직
                _lotID = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                _wipSeq = DataTableConverter.GetValue(rb.DataContext, "WIPSEQ").ToString();
                _eqptID = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();
                _procID = DataTableConverter.GetValue(rb.DataContext, "PROCID").ToString();
                _actdttm = DataTableConverter.GetValue(rb.DataContext, "ACTDTTM").ToString();

                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                string sHeader = _lotID + "-" + _clctseq + " (" + _actdttm + ")"; ;

                GetQuality();
            }

        }

        private void dgSpecLotChoice_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }
        private void dgSpecLotChoice_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            HiddenLoadingIndicator();
        }
        #endregion

        #region Mehod
        #region [작업대상 가져오기]
        public void GetSpecLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable("INDATA");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FDATE", typeof(string));
                dtRqst.Columns.Add("TDATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                    dr["LOTID"] = txtLotID.Text;

                dr["FDATE"] = Util.GetCondition(dtpDateFrom);
                dr["TDATE"] = Util.GetCondition(dtpDateTo);

                dtRqst.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_MODL_CHG_CHECK_LIST_HIST", "INDATA", "OUTDATA", ds);

                
                Util.GridSetData(dgLotList, dsRslt.Tables["OUTDATA"], null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetQuality()
        {
           GetGQualityInfo();
        }
        private void GetGQualityInfo()
        {
            Util.gridClear(dgQuality);

            try
            {
                string sBizName = "DA_PRD_SEL_MODL_CHG_CHECK_LIST_DETAIL";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                newRow["LOTID"] = _lotID;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ACTDTTM"] = _actdttm;
                newRow["EQPTID"] = _eqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQuality, dtResult, null);



                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                var topRows = dtResult.AsEnumerable().Max(x => x.Field<string>("NOTE"));
                txtNOTESingle.Text = topRows;
                //DataTableConverter.SetValue(dgdNote.Rows[0].DataItem, "NOTE", topRows);

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = 0;
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);

                int Startcol = dgQuality.Columns["ACTDTTM"].Index;
                int ForCount = 0;

                for (int col = Startcol + 1; col < dgQuality.Columns.Count; col++)
                {
                    ForCount++;

                    if (ForCount > _maxColumn)
                        dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                    else
                        dgQuality.Columns[col].Visibility = Visibility.Visible;
                }

                dgQuality.AlternatingRowBackground = null;

                // Merge
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME1" };
                _Util.SetDataGridMergeExtensionCol(dgQuality, sColumnName, DataGridMergeMode.VERTICAL);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

     
        #endregion

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
        private void AddNoteGrid(C1DataGrid dgd)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TITLE", typeof(string));
            dt.Columns.Add("NOTE", typeof(string));

            DataRow dr = dt.NewRow();

            dr["TITLE"] = ObjectDic.Instance.GetObjectName("비고");
            dt.Rows.Add(dr);

            Util.GridSetData(dgd, dt, FrameOperation, false);
        }
        #endregion

        private void btnSearch_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void CLCTVAL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;

                        if (!Util.IS_NUMBER(n.Text))
                        {
                            n.Text = string.Empty;
                            return;
                        }                     

                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() + grid.TopRows.Count > ++rIdx)
                    {
                        C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;

                        if (p == null || p.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }
                }
                else if (e.Key == Key.Tab)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;

                        if (!Util.IS_NUMBER(n.Text))
                        {
                            n.Text = string.Empty;
                            return;
                        }
                    

                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    int Startcol = grid.Columns["ACTDTTM"].Index;

                    for (int col = Startcol + 1; col < grid.Columns.Count; col++)
                    {
                        C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;

                        if (p == null || p.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() + grid.TopRows.Count > ++rIdx)
                    {
                        C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;

                        if (p == null || p.Content == null) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() + grid.TopRows.Count > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;

                        if (p == null || p.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InputMethod.Current != null)
                    InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                if (sender.GetType().Name == "TextBox")
                {
                    TextBox n = sender as TextBox;
                    n.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int rIdx = 0;
                int cIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (InputMethod.Current != null)
                    InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                if (sender.GetType().Name == "TextBox")
                {
                    TextBox n = sender as TextBox;
                    StackPanel panel = n.Parent as StackPanel;
                    DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                    if (p.Cell == null)
                        return;

                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    #region Spec Control
                    //string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "LSL"));
                    //string _ValutToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                    //string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                    //string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                    //string _ValueToControlVALUE = n.Text;

                    //string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, true);

                    //switch (ValueToFind)
                    //{
                    //    case "SPEC_OUT":
                    //        n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                    //        n.FontWeight = FontWeights.Bold;
                    //        break;
                    //    case "LIMIT_OUT":
                    //        n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0054FF"));
                    //        n.FontWeight = FontWeights.Bold;
                    //        break;
                    //    default:
                    //        n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                    //        n.FontWeight = FontWeights.Normal;
                    //        break;
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
