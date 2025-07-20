/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.08.18  신광희    : 품질정보 입력 버튼 추가(Process.NOTCHING 인 경우에만 해당 함.)  
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
    public partial class COM001_226 : UserControl, IWorkArea
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
        public COM001_226()
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
            Util.gridClear(dgQualityMulti);
            //DataTableConverter.SetValue(dgdNote.Rows[0].DataItem, "NOTE", string.Empty);
            //DataTableConverter.SetValue(dgdNoteMulti.Rows[0].DataItem, "NOTE", string.Empty);
            //tiQualityDefault.Header = ObjectDic.Instance.GetObjectName("자주검사수정");
            //tiQualityMulti.Header = ObjectDic.Instance.GetObjectName("자주검사수정");

            GetSpecLotList();
        }

        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualityInput()) return;

            CMM001.CMM_COM_SELF_INSP popupQualityInput = new CMM001.CMM_COM_SELF_INSP();
            popupQualityInput.FrameOperation = FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            parameters[3] = cboEquipment.Text;
            parameters[4] = txtLotID.Text;
            parameters[5] = "1"; //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupQualityInput, parameters);
            popupQualityInput.Closed += popupQualityInput_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupQualityInput.ShowModal()));
        }

        private void popupQualityInput_Closed(object sender, EventArgs e)
        {
            CMM001.CMM_COM_SELF_INSP pop = sender as CMM001.CMM_COM_SELF_INSP;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                //GetSheet();
            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnQualityInput.Visibility = string.Equals(cboProcess?.SelectedValue?.GetString(), Process.NOTCHING) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                return;
            }
            //GetSheet();
        }

        private void GetSheet()
        {
            #region 자주검사 Sheet
            ValueToSheet = GetSelfInspSheet();
            switch (ValueToSheet)
            {
                case "ALL": //ALL
                    tiQualityDefault.Visibility = Visibility.Visible;
                    tiQualityMulti.Visibility = Visibility.Visible;
                    break;
                case "M": // Multi
                    tiQualityDefault.Visibility = Visibility.Collapsed;
                    tiQualityMulti.Visibility = Visibility.Visible;
                    break;
                default:  // Single
                    tiQualityDefault.Visibility = Visibility.Visible;
                    tiQualityMulti.Visibility = Visibility.Collapsed;
                    break;
            }
            #endregion
            GetSelfInspItemId();
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

                    // Spec에 자주검사등록제외
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SELF_INSP_REG_EXCL_FLAG")).Equals("Y"))
                        InputCheck = false;

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

                                        #region Spec Control
                                        string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LSL"));
                                        string _ValutToUSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "USL"));
                                        string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MIN_VALUE"));
                                        string _ValutToMAX = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MAX_VALUE"));
                                        string _ValueToControlVALUE = n.Text;

                                        string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValutToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                                        switch (ValueToFind)
                                        {
                                            case "SPEC_OUT":
                                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                                n.FontWeight = FontWeights.Bold;
                                                break;
                                            case "LIMIT_OUT":
                                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0054FF"));
                                                n.FontWeight = FontWeights.Bold;
                                                break;
                                            default:
                                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                                                n.FontWeight = FontWeights.Normal;
                                                break;
                                        }
                                        #endregion

                                        #region SpecExclFlag
                                        if (InputCheck)
                                        {
                                            n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                            n.IsEnabled = true;
                                        }
                                        else
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                            n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                            n.IsEnabled = false;
                                        }
                                        #endregion
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

                    // Spec에 자주검사등록제외
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SELF_INSP_REG_EXCL_FLAG")).Equals("Y"))
                        InputCheck = false;

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

                                #region Spec Control
                                string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LSL"));
                                string _ValutToUSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "USL"));
                                string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MIN_VALUE"));
                                string _ValutToMAX = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MAX_VALUE"));
                                string _ValueToControlVALUE = n.Text;

                                string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValutToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                                switch (ValueToFind)
                                {
                                    case "SPEC_OUT":
                                        n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                        n.FontWeight = FontWeights.Bold;
                                        break;
                                    case "LIMIT_OUT":
                                        n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0054FF"));
                                        n.FontWeight = FontWeights.Bold;
                                        break;
                                    default:
                                        n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                                        n.FontWeight = FontWeights.Normal;
                                        break;
                                }
                                #endregion

                                #region SpecExclFlag
                                if (InputCheck)
                                {
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                    n.IsEnabled = false;
                                }
                                #endregion
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

        private void dgQualityMulti_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            dgQualityMulti.Focus();
        }

        private void dgQualityMulti_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            dgQualityMulti.Focus();
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
                _clctseq = DataTableConverter.GetValue(rb.DataContext, "CLCTSEQ").ToString();
                _sSELFTYPE = DataTableConverter.GetValue(rb.DataContext, "SELF_INSP_REG_TYPE_CODE").ToString();

                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                string sHeader = _lotID + "-" + _clctseq + " (" + _actdttm + ")"; ;

                switch (_sSELFTYPE)
                {
                    case "M":
                        tabQuality.SelectedIndex = 1;
                        tiQualityMulti.Visibility = Visibility.Visible;
                        tiQualityMulti.Header = sHeader;
                        tiQualityDefault.Visibility = Visibility.Collapsed;
                        break;
                    case "S":
                        tabQuality.SelectedIndex = 0;
                        tiQualityDefault.Visibility = Visibility.Visible;
                        tiQualityDefault.Header = sHeader;
                        tiQualityMulti.Visibility = Visibility.Collapsed;
                        break;
                }

                GetQuality(_sSELFTYPE);
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

                dr["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");

                dtRqst.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_COM_SEL_SELF_INSP_LOTLIST", "INDATA", "OUTDATA", ds);

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

        private void GetQuality(string sType)
        {
            #region 자주검사 Sheet
            switch (sType)
            {
                case "M": // Multi
                    GetGQualityMultiInfo();
                    break;
                default:  // Single
                    GetGQualityInfo();
                    break;
            }
            #endregion
        }
        private void GetGQualityInfo()
        {
            Util.gridClear(dgQuality);

            try
            {
                string sBizName = "DA_QCA_COM_SEL_SELF_INSP_CMM";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                newRow["LOTID"] = _lotID;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ACTDTTM"] = _actdttm;
                newRow["EQPTID"] = _eqptID;
                newRow["TYPE_FLAG"] = "S";

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
                //_maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => Convert.ToInt16(r["CLCT_COUNT"])); // 2024.11.18 - DB Type 문제로 인하여 Int로 Parsing.


                DataRow[] dr = dtResult.Select("(USL IS NULL OR USL ='') AND (LSL IS NULL OR LSL ='')");
                if (dr.Length == dtResult.Rows.Count)
                {
                    dgQuality.Columns["USL"].Visibility = Visibility.Collapsed;
                    dgQuality.Columns["LSL"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgQuality.Columns["USL"].Visibility = Visibility.Visible;
                    dgQuality.Columns["LSL"].Visibility = Visibility.Visible;
                }
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
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME4", "CLCTITEM_CLSS_NAME1" };
                _Util.SetDataGridMergeExtensionCol(dgQuality, sColumnName, DataGridMergeMode.VERTICAL);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetGQualityMultiInfo()
        {
            Util.gridClear(dgQualityMulti);

            try
            {
                #region 자주검사 등록시 검사코드  저장시 매핑, 해더 컨트롤용
                _clctItem = new DataTable();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["TYPE_FLAG"] = "M";
                inTable.Rows.Add(newRow);

                _clctItem = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_CLCTITEM", "RQSTDT", "RSLTDT", inTable);
                if (_clctItem == null || _clctItem.Rows.Count == 0)
                    return;

                #endregion

                #region 자주검사 등록시 LIST
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));

                inTable.Rows[0]["LOTID"] = _lotID;
                inTable.Rows[0]["WIPSEQ"] = _wipSeq;
                inTable.Rows[0]["ACTDTTM"] = _actdttm;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_LIST", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQualityMulti, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                var topRows = dtResult.AsEnumerable().Max(x => x.Field<string>("NOTE"));

                txtNOTEMulti.Text = topRows;

                //DataTableConverter.SetValue(dgdNoteMulti.Rows[0].DataItem, "NOTE", topRows);

                DataRow[] dr = dtResult.Select("(USL IS NULL OR USL ='') AND (LSL IS NULL OR LSL ='')");
                if (dr.Length == dtResult.Rows.Count)
                {
                    dgQualityMulti.Columns["USL"].Visibility = Visibility.Collapsed;
                    dgQualityMulti.Columns["LSL"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgQualityMulti.Columns["USL"].Visibility = Visibility.Visible;
                    dgQualityMulti.Columns["LSL"].Visibility = Visibility.Visible;
                }
                #endregion

                #region 그리드 header Setting

                DataTable dt = _clctItem.DefaultView.ToTable(true, "CLCTITEM_CLSS_NAME3");

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = Util.NVC_Int(_clctItem.Rows[0]["COLUMN_COUNNT"]);

                int startcol = dgQualityMulti.Columns["ACTDTTM"].Index;
                int forCount = 0;

                for (int col = startcol + 1; col < dgQualityMulti.Columns.Count; col++)
                {
                    forCount++;

                    if (forCount > _maxColumn)
                    {
                        dgQualityMulti.Columns[col].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgQualityMulti.Columns[col].Visibility = Visibility.Visible;
                        dgQualityMulti.Columns[col].Header = dt.Rows[forCount - 1]["CLCTITEM_CLSS_NAME3"].ToString();
                        dgQualityMulti.Columns[col].Tag = dt.Rows[forCount - 1]["CLCTITEM_CLSS_NAME3"].ToString();
                    }
                }

                dgQualityMulti.AlternatingRowBackground = null;

                // Merge
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME4", "CLCTITEM_CLSS_NAME1" };
                _Util.SetDataGridMergeExtensionCol(dgQualityMulti, sColumnName, DataGridMergeMode.VERTICAL);

                #endregion

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

                        #region Spec Control
                        string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "LSL"));
                        string _ValutToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                        string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                        string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                        string _ValueToControlVALUE = n.Text;

                        string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, true);

                        switch (ValueToFind)
                        {
                            case "SPEC_OUT":
                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                n.FontWeight = FontWeights.Bold;
                                break;
                            case "LIMIT_OUT":
                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0054FF"));
                                n.FontWeight = FontWeights.Bold;
                                break;
                            default:
                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                                n.FontWeight = FontWeights.Normal;
                                break;
                        }
                        #endregion

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

                        #region Spec Control
                        string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "LSL"));
                        string _ValutToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                        string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                        string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                        string _ValueToControlVALUE = n.Text;

                        string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, true);

                        switch (ValueToFind)
                        {
                            case "SPEC_OUT":
                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                n.FontWeight = FontWeights.Bold;
                                break;
                            case "LIMIT_OUT":
                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0054FF"));
                                n.FontWeight = FontWeights.Bold;
                                break;
                            default:
                                n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                                n.FontWeight = FontWeights.Normal;
                                break;
                        }
                        #endregion

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


        private void CLCTVAL_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "TextBox")
                {
                    TextBox n = sender as TextBox;
                    StackPanel panel = n.Parent as StackPanel;
                    DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;

                    grid = p.DataGrid;

                    if (!Util.IS_NUMBER(n.Text))
                    {
                        n.Text = string.Empty;
                        return;
                    }
                    #region Spec Control
                    string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "LSL"));
                    string _ValutToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                    string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                    string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                    string _ValueToControlVALUE = n.Text;

                    string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                    switch (ValueToFind)
                    {
                        case "SPEC_OUT":
                            n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                            n.FontWeight = FontWeights.Bold;
                            break;
                        case "LIMIT_OUT":
                            n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0054FF"));
                            n.FontWeight = FontWeights.Bold;
                            break;
                        default:
                            n.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            n.FontWeight = FontWeights.Normal;
                            break;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region SpecControl
        private string SpecControl(string _USL, string _LSL, string _MAX, string _MIN, string _VALUE, bool _Popup)
        {
            double vLSL, vUSL;
            double vMIN, vMAX;
            double vVALUE;
            string ValueToFind = string.Empty;

            #region Spec USL / LSL
            //상한값/하한값
            if (_USL != "" && _LSL != "")
            {
                if (double.TryParse(_LSL, out vLSL) && double.TryParse(_USL, out vUSL))
                {
                    if (!string.Equals(_VALUE, string.Empty))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vLSL > vVALUE || vUSL < vVALUE)
                                ValueToFind = "SPEC_OUT";
                        }
                    }
                }
            }
            //상한값
            if (_USL != "" && _LSL == "")
            {
                if (double.TryParse(_USL, out vUSL))
                {
                    if (!string.Equals(_VALUE, string.Empty))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vUSL < vVALUE)
                                ValueToFind = "SPEC_OUT";
                        }
                    }
                }
            }
            //하한값
            if (_USL == "" && _LSL != "")
            {
                if (double.TryParse(_LSL, out vLSL))
                {
                    if (!string.Equals(_VALUE, string.Empty))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vLSL > vVALUE)
                                ValueToFind = "SPEC_OUT";
                        }
                    }
                }
            }
            #endregion

            #region Spec MIN / MAX
            //MAX/MIN
            if (_MAX != "" && _MIN != "")
            {
                if (double.TryParse(_MIN, out vMIN) && double.TryParse(_MAX, out vMAX))
                {
                    if (!string.Equals(_VALUE, string.Empty))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vMIN > vVALUE || vMAX < vVALUE)
                                ValueToFind = "LIMIT_OUT";
                        }
                    }
                }
            }
            //MAX
            if (_MAX != "" && _MIN == "")
            {
                if (double.TryParse(_MAX, out vMAX))
                {
                    if (!string.Equals(_VALUE, string.Empty))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vMAX < vVALUE)
                            {
                                // 입력값이 기준치의 최대값 보다 큽니다
                                if (_Popup)
                                    Util.MessageInfo("SFU1803");
                                ValueToFind = "LIMIT_OUT";
                            }
                        }
                    }
                }
            }
            //MIN
            if (_MAX == "" && _MIN != "")
            {
                if (double.TryParse(_MIN, out vMIN))
                {
                    if (!string.Equals(_VALUE, string.Empty))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vMIN > vVALUE)
                            {
                                // 입력값이 기준치의 최소값 보다 작습니다
                                if (_Popup)
                                    Util.MessageInfo("SFU1804");
                                ValueToFind = "LIMIT_OUT";
                            }
                        }
                    }
                }
            }
            #endregion
            return ValueToFind;
        }

        private bool SpecLimit(C1DataGrid dg)
        {
            string colName = string.Empty;

            foreach (C1.WPF.DataGrid.DataGridRow dRow in dg.Rows)
            {
                if (dRow.Type.Equals(DataGridRowType.Top))
                    continue;

                int colValue = 0;

                for (int col = dg.Columns.Count - _maxColumn; col < dg.Columns.Count; col++)
                {
                    colValue++;

                    if (colValue > _maxColumn)
                        break;

                    colName = "CLCTVAL" + colValue.ToString("00");

                    DataRowView drvTmp = (dRow.DataItem as DataRowView);
                    string _ValutToUSL = string.Empty;
                    string _ValueToLSL = string.Empty;
                    string _ValueToMIN = Util.NVC(drvTmp["MIN_VALUE"]);
                    string _ValueToMAX = Util.NVC(drvTmp["MAX_VALUE"]);
                    string _ValueToControlVALUE = Util.NVC(drvTmp[colName]);

                    string ValueToFind = SpecControl(_ValutToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                    if (string.Equals(ValueToFind, "LIMIT_OUT"))
                    {
                        return false;
                    }
                }

            }
            return true;
        }
        #endregion

        #region [저장]
        private void btnSheet1Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Sheet1ValidateQualitySave())
                return;

            // 검사 결과를 저장 하시겠습니까?
            Util.MessageConfirm("SFU2811", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    QualitySave();
                }
            });
        }

        private void btnSheet2Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Sheet2ValidateQualitySave())
                return;

            // 검사 결과를 저장 하시겠습니까?
            Util.MessageConfirm("SFU2811", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    QualityMultiSave();
                }
            });
        }

        private void QualitySave()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_QCA_REG_WIP_DATA_CLCT";
                string colName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));

                for (int col = 0; col < _maxColumn; col++)
                {
                    colName = "CLCTVAL" + (col + 1).ToString("00");
                    inTable.Columns.Add(colName, typeof(string));
                }
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQuality.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top))
                        continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = _lotID;
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["EQPTID"] = _eqptID;
                    newRow["CLCTSEQ"] = _clctseq;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                    newRow["ACTDTTM"] = _actdttm;
                    newRow["NOTE"] = txtNOTESingle.Text; // DataTableConverter.GetValue(dgdNote.Rows[0].DataItem, "NOTE").GetString();

                    int colValue = 0;

                    for (int col = dgQuality.Columns.Count - _maxColumn; col < dgQuality.Columns.Count; col++)
                    {
                        colValue++;

                        if (colValue > _maxColumn)
                            break;

                        colName = "CLCTVAL" + colValue.ToString("00");

                        DataRowView drvTmp = (dRow.DataItem as DataRowView);
                        newRow[colName] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals("-")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                    }
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1270");      //저장되었습니다.

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

        private void QualityMultiSave()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_QCA_REG_WIP_DATA_CLCT";
                string colName;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));
                inTable.Columns.Add("CLCTVAL01", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));
                inTable.Columns.Add("NOTE", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQualityMulti.Rows)
                {
                    if (!dRow.Type.Equals(DataGridRowType.Item))
                        continue;

                    int colValue = 0;

                    for (int col = dgQualityMulti.Columns.Count - 20; col < dgQualityMulti.Columns.Count; col++)
                    {
                        colValue++;
                        colName = "CLCTVAL" + colValue.ToString("00");

                        if (colValue > _maxColumn)
                            break;

                        DataRow[] dr = _clctItem.Select("CLCTITEM_CLSS_NAME4 = '" + Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME4")) + "' And "
                                                      + "CLCTITEM_CLSS_NAME1 = '" + Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME1")) + "' And "
                                                      + "CLCTITEM_CLSS_NAME2 = '" + Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME2")) + "' And "
                                                      + "CLCTITEM_CLSS_NAME3 = '" + dgQualityMulti.Columns[col].Tag.ToString() + "'");

                        if (dr.Length > 0)
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["LOTID"] = _lotID;
                            newRow["WIPSEQ"] = _wipSeq;
                            newRow["EQPTID"] = _eqptID;
                            newRow["CLCTSEQ"] = _clctseq;
                            newRow["USERID"] = LoginInfo.USERID;
                            //newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                            newRow["CLCTITEM"] = Util.NVC(dr[0]["INSP_CLCTITEM"]);
                            newRow["ACTDTTM"] = _actdttm;

                            DataRowView drvTmp = (dRow.DataItem as DataRowView);
                            newRow["CLCTVAL01"] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals(" - ")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";
                            newRow["NOTE"] = txtNOTEMulti.Text; // DataTableConverter.GetValue(dgdNoteMulti.Rows[0].DataItem, "NOTE").GetString();
                            inTable.Rows.Add(newRow);
                        }
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1270");      //저장되었습니다.

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
        #endregion

        #region[[Validation]
        private bool Sheet1ValidateQualitySave()
        {

            if (!SpecLimit(dgQuality))
            {
                // 입력값이 최대값/최소값의 범위를 벗어났습니다.
                Util.MessageValidation("SFU4901");
                return false;
            }

            return true;
        }

        private bool Sheet2ValidateQualitySave()
        {
            if (!SpecLimit(dgQualityMulti))
            {
                // 입력값이 최대값/최소값의 범위를 벗어났습니다.
                Util.MessageValidation("SFU4901");
                return false;
            }

            return true;
        }

        private bool ValidationQualityInput()
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue?.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue?.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue?.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU3207");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue?.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrEmpty(txtLotID.Text.Trim()))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }

            return true;
        }
        #endregion


    }
}
