/*************************************************************************************
 Created Date : 2017.03.30
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 품질정보 입력 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.30  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_QUALITY_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_QUALITY_INPUT_LOT_TIME : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _Proc = string.Empty;
        private string _Line = string.Empty;
        private string _Eqpt = string.Empty;
        private string _LotId = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WO_DETL_ID = string.Empty;
        private bool bLoad = false;
        private readonly Util _util = new Util();
        #endregion

        #region Initialize        
        public CMM_ASSY_QUALITY_INPUT_LOT_TIME()
        {
            InitializeComponent();
            InitCombo();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            // 수집 기준 코드 셋팅
            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { "CLCT_BAS_CODE" };
            _combo.SetCombo(cboBasCode, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);

            DataTable dt = ((DataView)cboBasCode.ItemsSource).Table;
            if (dt.Rows.Count > 1)
                cboBasCode.SelectedIndex = 2;

            cboBasCode.SelectedItemChanged += cboBasCode_SelectedItemChanged;
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 6)
            {
                _Line = Util.NVC(tmps[0]);
                _Eqpt = Util.NVC(tmps[1]);
                _Proc = Util.NVC(tmps[2]);
                _LotId = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
                _WO_DETL_ID = Util.NVC(tmps[5]);
            }

            GetBasicInfo(dg: dgQualityInfo);

            bLoad = true;
        }

        private void cboBasCode_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnQualitySave.IsEnabled = true;

            if (cboBasCode.SelectedValue.GetString().ToUpper() == "LOT")
            {
                txtTime.Visibility = Visibility.Collapsed;
                cboTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtTime.Visibility = Visibility.Visible;
                cboTime.Visibility = Visibility.Visible;
            }

            if (cboBasCode.Items.Count > 0 && cboBasCode.SelectedValue != null && !cboBasCode.SelectedValue.Equals("SELECT"))
            {
                GetBasicInfo(dg: dgQualityInfo);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfo == null || dgQualityInfo.Rows.Count < 1) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality(dgQualityInfo);
                }
            });
        }

        private void CLCTVAL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                int rIdx = 0;
                int cIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
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


                if (grid.GetRowCount() > ++rIdx)
                {
                    if (grid.GetRowCount() - 1 != rIdx)
                    {
                        grid.ScrollIntoView(rIdx + 1, cIdx);
                    }

                    C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                    if (p == null) return;
                    StackPanel panel = p.Content as StackPanel;

                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if (panel.Children[cnt].Visibility == Visibility.Visible)
                            panel.Children[cnt].Focus();
                    }
                }
                else if (CommonVerify.HasDataGridRow(grid) && grid.GetRowCount() == rIdx)
                {
                    return;
                    //btnQualitySave.Focus();
                }
            }
        }

        private void dgQualityInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.ToUpper().IndexOf("CLCTVAL01", StringComparison.Ordinal) >= 0)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));

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
                                    //n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                                else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                {
                                    ComboBox n = panel.Children[cnt] as ComboBox;
                                    //n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                    int idx = n.SelectedIndex;

                                    n.SelectedIndex = idx == -1 ? 0 : idx;
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgQualityInfo_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void CLCTVAL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
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
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() > ++rIdx)
                    {
                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (p == null) return;
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
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
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
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        //if (grid.GetRowCount() - 1 != rIdx)
                        if (rIdx > 0)
                        {
                            grid.ScrollIntoView(rIdx - 1, cIdx);
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (p == null) return;
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
                if (sender.GetType().Name == "C1NumericBox")
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        n.Background = new SolidColorBrush((Color)convertFromString);
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF");
                    if (convertFromString != null)
                        n.Background = new SolidColorBrush((Color)convertFromString);
                }
                else
                    return;
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
                DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;
                if (drv == null) return;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        if (n != null) n.Background = new SolidColorBrush((Color)convertFromString);

                    string usl = drv["USL"].GetString();
                    string lsl = drv["LSL"].GetString();

                    if (n != null)
                    {
                        if (!string.IsNullOrEmpty(usl) && !string.IsNullOrEmpty(lsl))
                        {
                            if (n.Value.GetDecimal() > usl.GetDecimal() || n.Value.GetDecimal() < lsl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                        else if (!string.IsNullOrEmpty(usl))
                        {
                            if (n.Value.GetDecimal() > usl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                        else if (!string.IsNullOrEmpty(lsl))
                        {
                            if (n.Value.GetDecimal() < usl.GetDecimal())
                            {
                                n.FontWeight = FontWeights.Bold;
                                n.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                n.FontWeight = FontWeights.Normal;
                                var foregroundString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                                if (foregroundString != null)
                                    n.Foreground = new SolidColorBrush((Color)foregroundString);
                            }
                        }
                    }
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0");
                    if (convertFromString != null)
                        n.Background = new SolidColorBrush((Color)convertFromString);
                }

                else
                    return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        #region Biz
        private void GetBasicInfo(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));
                //dtRqstAdd.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                //dtRqstAdd.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqstAdd.Columns.Add("EQPTID", typeof(string));
                dtRqstAdd.Columns.Add("CLCT_BAS_CODE", typeof(string));
                dtRqstAdd.Columns.Add("CLCT_ITVL", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _Proc;
                drAdd["LOTID"] = _LotId;
                //drAdd["CLCTITEM_CLSS4"] = sType;
                drAdd["EQPTID"] = _Eqpt;
                drAdd["CLCT_BAS_CODE"] = cboBasCode.SelectedValue?.ToString() ?? "";
                //drAdd["CLCT_ITVL"] = cboTime.SelectedValue;
                if (cboTime.Items.Count > 0)
                    drAdd["CLCT_ITVL"] = cboTime.SelectedValue;

                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT_TIME", "INDATA", "OUTDATA", dtRqstAdd);

                Util.gridClear(dg);

                if (cboBasCode.SelectedValue.GetString().ToUpper() == "TIME" && cboTime.Items.Count < 1)
                {
                    MakeTimeCombo(dtRsltAdd.Copy());
                }
                //dg.ItemsSource = DataTableConverter.Convert(dtRsltAdd);

                Util.GridSetData(dgQualityInfo, dtRsltAdd, FrameOperation, true);

                _util.SetDataGridMergeExtensionCol(dgQualityInfo, new string[] { "CLSS_NAME1", "CLSS_NAME2", "CLSS_NAME3" }, DataGridMergeMode.VERTICALHIERARCHI);
                //dtRsltAdd.DefaultView;//DataTableConverter.Convert(dtRslt);

                AddNoteGrid(dgdNote);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void AddNoteGrid(C1DataGrid dgd)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TITLE",typeof(string));
            dt.Columns.Add("NOTE",typeof(string));

            DataRow dr = dt.NewRow();

            string title = "비고";
            dr["TITLE"] = ObjectDic.Instance.GetObjectName(title);
            dt.Rows.Add(dr);

            Util.GridSetData(dgd, dt, FrameOperation, false);
        }

        private DateTime GetDBDateTime()
        {
            try
            {
                DateTime tmpDttm = DateTime.Now;
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", null, "OUTDATA", null);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tmpDttm = (DateTime)dtRslt.Rows[0]["DATE"];
                }

                return tmpDttm;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private void SaveQuality(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                dtRqst.Columns.Add("CLCTMAX", typeof(string));
                dtRqst.Columns.Add("CLCTMIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CLCTSEQ_ORG", typeof(Int16));
                dtRqst.Columns.Add("NOTE", typeof(string));

                dg.EndEdit();

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows) // DataGridHandler.GetAddedItems(dg))
                {
                    DataRowView drvTmp = (row.DataItem as DataRowView);
                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LOTID"] = _LotId;
                    dr["WIPSEQ"] = _WipSeq;
                    dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                    dr["CLCTITEM"] = drvTmp["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                    dr["CLCTVAL01"] = (!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");

                    if (Util.NVC(drvTmp["MAND_INSP_ITEM_FLAG"]).ToUpper().Equals("Y"))
                    {
                        if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                            Util.MessageInfo("SFU3589", parameters); // 품질 항목[%1]은 필수 항목 입니다.

                            return;
                        }
                    }

                    dr["CLCTMAX"] = drvTmp["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                    dr["CLCTMIN"] = drvTmp["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                    dr["EQPTID"] = _Eqpt;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["CLCTSEQ_ORG"] = drvTmp["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                    //dgdNote[0, dgdNote.Columns["NOTE"].Index].Text;
                    dr["NOTE"] = DataTableConverter.GetValue(dgdNote.Rows[0].DataItem, "NOTE").GetString();
                    dtRqst.Rows.Add(dr);
                }

                if (dtRqst.Rows.Count > 0)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);

                    Util.MessageInfo("SFU1270");      //저장되었습니다.

                    btnQualitySave.IsEnabled = false;
                }
                else
                {
                    Util.MessageInfo("SFU1566");      //변경된데이타가없습니다.
                }
                  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void MakeTimeCombo(DataTable dt)
        {
            var query = dt.Copy().AsEnumerable()
                .OrderBy(o => o.Field<Int32>("CLCT_ITVL"))
                .GroupBy(g => new { clctVal = g.Field<Int32>("CLCT_ITVL") })
                .Select(s => s.Key.clctVal).ToList();

            DataTable dtTime = new DataTable();
            dtTime.Columns.Add("CODE", typeof(string));
            dtTime.Columns.Add("CODENAME", typeof(string));

            foreach (var item in query)
            {
                DataRow dr = dtTime.NewRow();
                dr["CODE"] = item.GetString();
                dr["CODENAME"] = item.GetString();
                dtTime.Rows.Add(dr);
            }

            DataRow newRow = dtTime.NewRow();
            newRow["CODE"] = null;
            newRow["CODENAME"] = "-ALL-";
            dtTime.Rows.InsertAt(newRow, 0);

            cboTime.DisplayMemberPath = "CODENAME";
            cboTime.SelectedValue = "CODE";
            cboTime.ItemsSource = dtTime.Copy().AsDataView();
            cboTime.SelectedIndex = 0;

            cboTime.SelectedValueChanged += cboTime_SelectedValueChanged;
        }

        #endregion

        #region Func
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnQualitySave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.IsVisible)
                cbo.SelectedIndex = 0;
        }

        private void cboTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetBasicInfo(dg: dgQualityInfo);
        }

        private void dgQualityInfo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            dgQualityInfo.Focus();
        }
    }
}
