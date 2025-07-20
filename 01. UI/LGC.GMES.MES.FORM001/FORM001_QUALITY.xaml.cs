/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : 자주검사
--------------------------------------------------------------------------------------
 [Change History]
 2024.05.20 이병윤 : E20240516-000918 : MMD에서 상한,하한 조회 추가 입력시 Validation 추가
                                        CMM_COM_QUALITY.xaml.cs[집합번호 : 64758]을 참조해서 수정
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;


namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_QUALITY : C1Window, IWorkArea
    {
        #region Declaration

        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _wipSeq = string.Empty;

        int _maxColumn = 0;

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }

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

        public FORM001_QUALITY()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                _load = false;
            }
        }
        private void InitializeUserControls()
        {
            UcFormShift = grdShift.Children[0] as UcFormShift;
        }
        private void SetControl()
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;
            _wipSeq = tmps[5] as string;

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtLotID.Text = tmps[4] as string;

            // 자주검사 주기 콤보 조회
            SetQualityTime();

            cboQualityTime.SelectedValueChanged += cboQualityTime_SelectedValueChanged;

            // 자주검사 항목 조회
            GetGQualityInfo();

            // 작업자, 작업조
            UcFormShift.TextShift.Tag = ShiftID;
            UcFormShift.TextShift.Text = ShiftName;
            UcFormShift.TextWorker.Tag = WorkerID;
            UcFormShift.TextWorker.Text = WorkerName;
            UcFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcFormShift = grdShift.Children[0] as UcFormShift;
            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

            if ("F6000".Equals(_procID))
            {
                const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = "SELF_INSP_CM_GAP_LIMIT";

                inTable.Rows.Add(newRow);

                string selfInspCmGapLimitMsg = MessageDic.Instance.GetMessage("SFU4990", "") + "\n" + MessageDic.Instance.GetMessage("SFU4991", "");
                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            string selfInspCmGapLimitMsgSubSI513 = string.Empty;
                            string selfInspCmGapLimitMsgSubSI514 = string.Empty;

                            for (int inx = 0; inx < result.Rows.Count; inx++)
                            {
                                if ("SI513-001".Equals(result.Rows[inx]["COM_CODE"]))
                                {
                                    selfInspCmGapLimitMsgSubSI513 = MessageDic.Instance.GetMessage("SFU4990", result.Rows[inx]["ATTR1"]);
                                }
                                else if ("SI514-001".Equals(result.Rows[inx]["COM_CODE"]))
                                {
                                    selfInspCmGapLimitMsgSubSI514 = MessageDic.Instance.GetMessage("SFU4991", result.Rows[inx]["ATTR1"]);
                                }
                            }

                            selfInspCmGapLimitMsg = selfInspCmGapLimitMsgSubSI513 + "\n" + selfInspCmGapLimitMsgSubSI514;
                        }

                        txtSelfInspCmGapLimit.Text = selfInspCmGapLimitMsg;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            else
            {
                txtSelfInspCmGapLimit.Text = "";
            }

        }

        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateQualitySave())
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
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [검사값 입력 : dgQuality: LoadedCellPresenter,UnloadedCellPresenter,LoadedRowPresenter,PreviewMouseWheel]
        // E20240516-000918 : 해당 이벤트 사용하지 않음
        //private void dgQuality_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        //{
        //    if (!e.Row.Type.Equals(DataGridRowType.Top))
        //    {
        //        if (e.Column.Name.Length > 2)
        //        {
        //            string ColName = e.Column.Name.Substring(e.Column.Name.Length - 2, 2).ToString();
        //            int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Row.DataItem, "CLCT_COUNT"));
        //            int ColCnt = 0;
        //            if (int.TryParse(ColName, out ColCnt))
        //            {
        //                if (ColCnt > MaxCount)
        //                {
        //                    e.Cancel = true;
        //                }
        //            }
        //        }
        //    }
        //}
        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            // E20240516-000918 : 사용하지 않아 주석처리
            //if (sender == null)
            //    return;
            //if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
            //    return;
            //C1DataGrid dg = sender as C1DataGrid;
            //dg?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }
            //    if (e.Cell.Column.Name.Length > 2)
            //    {
            //        string ColName = e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 2, 2).ToString();
            //        int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_COUNT"));
            //        int ColCnt = 0;
            //        if (int.TryParse(ColName, out ColCnt))
            //        {
            //            if (ColCnt > MaxCount)
            //            {
            //                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
            //            }
            //        }
            //    }
            //}));

            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid grid = sender as C1DataGrid;
            int rIdx = 0;

            grid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                {
                    string ColName = e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 2, 2).ToString();
                    int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_COUNT"));
                    int ColCnt = 0;

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

                                        rIdx = e.Cell.Row.Index;
                                        if (!EDCCheck(grid, rIdx, e.Cell.Column.Name))
                                        {
                                            n.Background = new SolidColorBrush(Colors.Red);
                                            n.Foreground = new SolidColorBrush(Colors.Black);
                                            n.FontWeight = FontWeights.Bold;
                                            DataTableConverter.SetValue(rIdx, e.Cell.Column.Name, string.Empty);

                                            if (e.Cell.Presenter != null)
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                            }
                                        }
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
                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            e.Cell.Presenter.Background = null;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            
                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush(Colors.White);
                                        n.Foreground = new SolidColorBrush(Colors.Black);
                                        n.IsEnabled = true;

                                        rIdx = e.Cell.Row.Index;
                                        if (!EDCCheck(grid, rIdx, e.Cell.Column.Name))
                                        {
                                            n.Background = new SolidColorBrush(Colors.Red);
                                            n.Foreground = new SolidColorBrush(Colors.Black);
                                            n.FontWeight = FontWeights.Bold;
                                        }
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush(Colors.White);
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

                                    n.Background = new SolidColorBrush(Colors.White);
                                    n.Foreground = new SolidColorBrush(Colors.Black);
                                    n.IsEnabled = true;

                                    rIdx = e.Cell.Row.Index;
                                    if (!EDCCheck(grid, rIdx, e.Cell.Column.Name))
                                    {
                                        n.Background = new SolidColorBrush(Colors.Red);
                                        n.Foreground = new SolidColorBrush(Colors.Black);
                                        n.FontWeight = FontWeights.Bold;
                                    }
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

        /// <summary>
        /// E20240516-000918 : 기능 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid grid = sender as C1DataGrid;

            grid.Dispatcher.BeginInvoke(new Action(() =>
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
                            int rIdx = 0;
                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;

                                        n.Background = new SolidColorBrush(Colors.White);
                                        n.Foreground = new SolidColorBrush(Colors.Black);
                                        n.IsEnabled = true;

                                        rIdx = e.Cell.Row.Index;
                                        if (!EDCCheck(grid, rIdx, e.Cell.Column.Name))
                                        {
                                            n.Background = new SolidColorBrush(Colors.Red);
                                            n.Foreground = new SolidColorBrush(Colors.Black);
                                            n.FontWeight = FontWeights.Bold;
                                        }
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush(Colors.White);
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

        /// <summary>
        /// E20240516-000918 : 기능추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            dgQuality.Focus();
        }

        /// <summary>
        /// E20240516-000918 ; 기능추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            dgQuality.Focus();
        }

        #endregion

        #region [검사 주기 변경: cboQualityTime_SelectedValueChanged]
        private void cboQualityTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
        }
        #endregion

        #region [E20240516-000918: MMD SPEC 체크 추가 CLCTVAL_KeyDown, PreviewKeyDown,ValueChanged, GotFocus, LostFocus, cbVal_Loaded, EDCCheck]
        private void CLCTVAL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Tab)
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

                        n.Background = new SolidColorBrush(Colors.White);
                        n.Foreground = new SolidColorBrush(Colors.Black);
                        n.FontWeight = FontWeights.Normal;

                        if (n.Name.IndexOf("txtVal") < 0)
                            return;

                        // 음수값 허용 기존로직 주석 
                        //if (!Util.IS_NUMBER(n.Value.ToString()))
                        //    return;
                        if (double.IsNaN(n.Value) == true)
                        {
                            return;
                        }

                        if (!EDCCheck(grid, rIdx, p.Cell.Column.Name))
                        {
                            n.Background = new SolidColorBrush(Colors.Red);
                            n.Foreground = new SolidColorBrush(Colors.Black);
                            n.FontWeight = FontWeights.Bold;
                            //DataTableConverter.SetValue(rIdx, p.Cell.Column.Name, string.Empty);

                            p.Cell.Presenter.Background = new SolidColorBrush(Colors.White);

                            Util.MessageValidation("SFU5038"); //측정 값이 Spec을 벗어납니다. 입력값 및 Spec 값을 확인 바랍니다.
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool EDCCheck(C1.WPF.DataGrid.C1DataGrid dg, int rIdx, string ValueName)
        {
            string _USL = string.Empty;
            string _LSL = string.Empty;
            string _VALUE = string.Empty;

            _LSL = Util.NVC(DataTableConverter.GetValue(dg.Rows[rIdx].DataItem, "LSL"));
            _USL = Util.NVC(DataTableConverter.GetValue(dg.Rows[rIdx].DataItem, "USL"));
            _VALUE = Util.NVC(DataTableConverter.GetValue(dg.Rows[rIdx].DataItem, ValueName));

            if (_USL == "" && _LSL == "")
                return true;

            double vLSL, vUSL, vVALUE;

            //상한값/하한값
            if (_USL != "" && _LSL != "")
            {
                if (double.TryParse(_LSL, out vLSL) && double.TryParse(_USL, out vUSL))
                {
                    if (double.TryParse(_VALUE, out vVALUE))
                    {
                        if (vLSL > vVALUE || vUSL < vVALUE)
                        {
                            return false;
                        }
                    }
                }
            }
            //상한값
            if (_USL != "" && _LSL == "")
            {
                if (double.TryParse(_USL, out vUSL))
                {
                    if (double.TryParse(_VALUE, out vVALUE))
                    {
                        if (vUSL < vVALUE)
                        {
                            return false;
                        }
                    }
                }
            }
            //하한값
            if (_USL == "" && _LSL != "")
            {
                if (double.TryParse(_LSL, out vLSL))
                {
                    if (double.TryParse(_VALUE, out vVALUE))
                    {
                        if (vLSL > vVALUE)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void CLCTVAL_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // 2023.12.26 Key.Down 와 Key.Up 겹치는 부분 합침
                if (e.Key == Key.Down || e.Key == Key.Up)
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

                        n.Background = new SolidColorBrush(Colors.White);
                        n.Foreground = new SolidColorBrush(Colors.Black);

                        if (!EDCCheck(grid, rIdx, p.Cell.Column.Name))
                        {
                            n.Background = new SolidColorBrush(Colors.Red);
                            n.Foreground = new SolidColorBrush(Colors.Black);
                            n.FontWeight = FontWeights.Bold;

                            p.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
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

                    if (e.Key == Key.Down)
                    {
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
                    }
                    else if (e.Key == Key.Up)
                    {
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
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
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

                n.Background = new SolidColorBrush(Colors.White);
                n.Foreground = new SolidColorBrush(Colors.Black);
                n.FontWeight = FontWeights.Normal;

                if (n.Name.IndexOf("txtVal") < 0)
                    return;

                // 음수 처리를 위해서 주석
                //if (!Util.IS_NUMBER(n.Value.ToString()))
                //    return;
                if(double.IsNaN(n.Value) == true)
                {
                    return;
                }

                if (!EDCCheck(grid, rIdx, p.Cell.Column.Name))
                {
                    n.Background = new SolidColorBrush(Colors.Red);
                    n.Foreground = new SolidColorBrush(Colors.Black);
                    n.FontWeight = FontWeights.Bold;

                    p.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                return;
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.IsVisible)
                cbo.SelectedIndex = 0;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// 검사 시간
        /// </summary>
        private void SetQualityTime()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CBO_FO", "INDATA", "OUTDATA", inTable);

                DataTable dtQualityTime = new DataTable();
                dtQualityTime.Columns.Add(cboQualityTime.DisplayMemberPath.ToString(), typeof(string));
                dtQualityTime.Columns.Add(cboQualityTime.SelectedValuePath.ToString(), typeof(string));

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dtResult.Rows[0]["START_HHMMDD"].ToString()))
                    {
                        DateTime dtStart = Convert.ToDateTime(dtResult.Rows[0]["NOW_YMD"].ToString() + " " + dtResult.Rows[0]["START_HHMMDD"].ToString());
                        DateTime dtEnd = Convert.ToDateTime(dtResult.Rows[0]["NXT_YMD"].ToString() + " " + dtResult.Rows[0]["START_HHMMDD"].ToString());
                        DateTime dtCombo = dtStart;

                        int itvl =Util.NVC_Int(dtResult.Rows[0]["CLCT_ITVL"].ToString());

                        for (int loop = 0; loop < 999; loop++)
                        {
                            DataRow dr = dtQualityTime.NewRow();
                            dr[cboQualityTime.DisplayMemberPath.ToString()] = dtCombo.ToString("HH:mm");
                            dr[cboQualityTime.SelectedValuePath.ToString()] = dtCombo.ToString("yyyy-MM-dd HH:mm:ss"); 
                            dtQualityTime.Rows.Add(dr);

                            dtCombo = dtCombo.AddHours(itvl);

                            if (dtEnd <= dtCombo)
                                break;

                        }
                    }
                }

                DataRow drSelect = dtQualityTime.NewRow();
                drSelect[cboQualityTime.DisplayMemberPath.ToString()] = "-SELECT-";
                drSelect[cboQualityTime.SelectedValuePath.ToString()] = "SELECT";
                dtQualityTime.Rows.InsertAt(drSelect, 0);

                cboQualityTime.DisplayMemberPath = cboQualityTime.DisplayMemberPath.ToString();
                cboQualityTime.SelectedValuePath = cboQualityTime.SelectedValuePath.ToString();
                cboQualityTime.ItemsSource = dtQualityTime.Copy().AsDataView();
                cboQualityTime.SelectedIndex = 0;
                


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetGQualityInfo()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.gridClear(dgQuality);
                return;
            }

            try
            {
                string sBizName = "DA_QCA_SEL_SELF_INSP_FO";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                //inTable.Columns.Add("ACTDTTM", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(string));
                // E20240516-000918 : 검색조건 동 추가
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["CLCTITEM_CLSS4"] = "A";
                newRow["LOTID"] = txtLotID.Text;
                newRow["WIPSEQ"] = _wipSeq;
                //newRow["ACTDTTM"] = cboQualityTime.SelectedValue ?? cboQualityTime.SelectedValue.ToString();
                newRow["CLCTSEQ"] = cboQualityTime.SelectedIndex;
                // E20240516-000918 : 검색조건 동 추가
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQuality, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = 0;
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);

                int Startcol = dgQuality.Columns["ACTDTTM"].Index;
                int ForCount = 0;

                for (int col = Startcol + 1; col < dgQuality.Columns.Count; col++)
                {
                    ForCount++;
                    //if (ForCount > _maxColumn)
                    //    dgQuality.Columns[col].Visibility = Visibility.Collapsed;

                    //if ("F6200".Equals(_procID) && col == (21 + Startcol))
                    //{
                    //    dgQuality.Columns[col].Header = ObjectDic.Instance.GetObjectName("더블탭 NG 수량");
                    //    dgQuality.Columns[col].MaxWidth = 100;
                    //}

                    // : else 구문 추가
                    if (ForCount > _maxColumn)
                    {
                        dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgQuality.Columns[col].Visibility = Visibility.Visible;
                    }
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
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));

                // E20240516-000918 : DataGridRow ->  C1.WPF.DataGrid.DataGridRow 으로 변경 
                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQuality.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top))
                        continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = txtLotID.Text;
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["EQPTID"] = _eqptID;
                    newRow["CLCTSEQ"] = cboQualityTime.SelectedIndex;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                    newRow["ACTDTTM"] = cboQualityTime.SelectedValue.ToString();

                    int colValue = 0;

                    for (int col = dgQuality.Columns.Count - _maxColumn; col < dgQuality.Columns.Count; col++)
                    {
                        colValue++;

                        if (colValue > _maxColumn)
                            break;

                        colName = "CLCTVAL" + colValue.ToString("00");

                        // E20240516-000918 : 컬럼타입 변경으로 주석처리
                        //newRow[colName] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, colName));
                        // E20240516-000918 : Null값 처리 추가
                        DataRowView drvTmp = (dRow.DataItem as DataRowView);
                        newRow[colName] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals("-")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";
                        if (newRow[colName].ToString().Equals("NaN"))
                        {
                            newRow[colName] = "";
                        }
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

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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
        private bool ValidateQualitySave()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                // 검사 주기를 선택 하세요.
                Util.MessageValidation("SFU4024");
                return false;
            }

            ////if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            ////{
            ////    // 작업조를 입력해 주세요.
            ////    Util.MessageValidation("SFU1845");
            ////    return false;
            ////}

            ////if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            ////{
            ////    // 작업자를 입력 해 주세요.
            ////    Util.MessageValidation("SFU1843");
            ////    return false;
            ////}

            // E20240516-000918 : DataGridRow ->  C1.WPF.DataGrid.DataGridRow 으로 변경 
            foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQuality.Rows)
            {
                if (dRow.Type.Equals(DataGridRowType.Top))
                {
                    continue;
                }

                String sInspClctItem = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                String sClctVal21 = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTVAL21"));

                //외부탭용접 공정, 인장강도(양극)/인장강도(음극) 21번째 입력값이 공백이면
                if ("A010".Equals(LoginInfo.CFG_SHOP_ID) && "F6200".Equals(_procID) && ("SI194-001".Equals(sInspClctItem) || "SI195-001".Equals(sInspClctItem)) && String.IsNullOrEmpty(sClctVal21))
                {
                    Util.MessageValidation("SFU4980", 21);
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region [Func]

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _procID; ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcFormShift.TextShiftEndTime.Text))
                                {
                                    UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcFormShift.TextWorker.Text = string.Empty;
                                    UcFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcFormShift.TextShift.Tag = string.Empty;
                                    UcFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcFormShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (System.Windows.Controls.Grid tmp in Util.FindVisualChildren<System.Windows.Controls.Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.Focus();
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
    }
}
