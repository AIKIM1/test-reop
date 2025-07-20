/*************************************************************************************
 Created Date : 2017.12.13
      Creator : 
   Decription : 자주검사
--------------------------------------------------------------------------------------
 [Change History]
 [Change History]
   2022.05.07   손우석   : 2022-05-07 운영소스
   2023.12.26   오수현   : E20231106-000046  
                           1)	Limit Out 대상 선 체크 :  Spec 초과시 파란색 글짜로 표시(기존 로직 유지)
                           2)	1)번사항 체크 후 상하한값 체크 : Spec 초과시 빨간색 배경 + 검정색 글짜 (수정대상) 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Controls;
using System.Windows.Input;



namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_SELF_INSP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_SELF_INSP : C1Window
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _lineID = string.Empty;        // 라인코드
        private string _wipSeq = string.Empty;

        int _maxColumn = 0;
        DataTable _clctItem;
        private bool _load = true;
        private string ValueToSheet = string.Empty;
        DataTable ValueToDT;

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

        public CMM_COM_SELF_INSP()
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
                SetControl();
                _load = false;
            }
        }

        private void SetControl()
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = Util.NVC(tmps[0]);
            _eqptID = Util.NVC(tmps[2]);
            _wipSeq = Util.NVC(tmps[5]);
            _lineID = Util.NVC(tmps[1]);

            // SET COMMON
            //txtMProcess.Text = Util.NVC(tmps[1]);
            txtEquipment.Text = Util.NVC(tmps[3]);
            txtMEquipment.Text = Util.NVC(tmps[3]);
            txtLotID.Text = Util.NVC(tmps[4]);


            DataTable dtTmp = new DataTable();
            dtTmp.Columns.Add(cboLotID.DisplayMemberPath.ToString(), typeof(string));
            dtTmp.Columns.Add(cboLotID.SelectedValuePath.ToString(), typeof(string));

            DataRow dr = dtTmp.NewRow();
            dr[cboLotID.DisplayMemberPath.ToString()] = Util.NVC(tmps[4]);
            dr[cboLotID.SelectedValuePath.ToString()] = Util.NVC(tmps[4]);
            dtTmp.Rows.Add(dr);

            if (tmps.Length > 6 && !Util.NVC(tmps[6]).Equals(""))
            {
                dr = dtTmp.NewRow();
                dr[cboLotID.DisplayMemberPath.ToString()] = Util.NVC(tmps[6]);
                dr[cboLotID.SelectedValuePath.ToString()] = Util.NVC(tmps[6]);
                dtTmp.Rows.Add(dr);
            }

            cboLotID.DisplayMemberPath = cboLotID.DisplayMemberPath.ToString();
            cboLotID.SelectedValuePath = cboLotID.SelectedValuePath.ToString();
            cboLotID.ItemsSource = dtTmp.Copy().AsDataView();
            cboLotID.SelectedIndex = 0;

            //if (_procID.Equals(Process.NOTCHING))
            //{
            //    cboLotID.IsEnabled = true;
            //}
            //else
            //{
            //    cboLotID.IsEnabled = false;
            //}

            #region 자주검사 Sheet
            ValueToSheet = GetSelfInspSheet();
            switch (ValueToSheet)
            {
                case "ALL": //ALL
                    tiQualityDefault.Visibility = Visibility.Visible;
                    tiQualityMulti.Visibility = Visibility.Visible;

                    #region TAB 1
                    // 자주검사 주기 콤보 조회
                    SetQualityTimeSheet1();
                    cboQualityTime.SelectedValueChanged += cboQualityTime_SelectedValueChanged;

                    // 자주검사 그룹 콤보 조회
                    SetQualityGroup();
                    cboQualityGroup.SelectedValueChanged += cboQualityGroup_SelectedValueChanged;

                    AddNoteGrid(dgdNote);
                    // 자주검사 항목 조회
                    GetGQualityInfo();
                    #endregion

                    #region TAB 2
                    SetQualityTimeSheet2();
                    cboMQualityTime.SelectedValueChanged += cboMQualityTime_SelectedValueChanged;

                    // 자주검사 그룹 콤보 조회
                    SetQualityMultiGroup();
                    cboMQualityGroup.SelectedValueChanged += cboQualityMultiGroup_SelectedValueChanged;

                    AddNoteGrid(dgdMNote);
                    GetGQualityMultiInfo();
                    #endregion
                    break;
                case "M": // Multi
                    tiQualityDefault.Visibility = Visibility.Collapsed;
                    tiQualityMulti.Visibility = Visibility.Visible;

                    #region TAB 2
                    SetQualityTimeSheet2();
                    cboMQualityTime.SelectedValueChanged += cboMQualityTime_SelectedValueChanged;

                    // 자주검사 그룹 콤보 조회
                    SetQualityMultiGroup();
                    cboMQualityGroup.SelectedValueChanged += cboQualityMultiGroup_SelectedValueChanged;

                    AddNoteGrid(dgdMNote);
                    GetGQualityMultiInfo();
                    #endregion
                    break;
                default:  // Single
                    tiQualityDefault.Visibility = Visibility.Visible;
                    tiQualityMulti.Visibility = Visibility.Collapsed;

                    #region TAB 1
                    // 자주검사 주기 콤보 조회
                    SetQualityTimeSheet1();
                    cboQualityTime.SelectedValueChanged += cboQualityTime_SelectedValueChanged;

                    // 자주검사 그룹 콤보 조회
                    SetQualityGroup();
                    cboQualityGroup.SelectedValueChanged += cboQualityGroup_SelectedValueChanged;

                    AddNoteGrid(dgdNote);
                    // 자주검사 항목 조회
                    GetGQualityInfo();
                    #endregion
                    break;

            }
            #endregion

            cboLotID.SelectedValueChanged += cboLotID_SelectedValueChanged;

            SelectProcessName();

            // TODO : CNB2동 조립의 경우에는 월력에 등록된 사용자 정보를 바인딩 한다. 운영MMD 동정보 확인 후 수정 필요 함.
            if (LoginInfo.CFG_AREA_ID == "AA")
            {
                GetWorkCalander();
            }

            if (string.IsNullOrEmpty(txtWorker.Text) || string.IsNullOrEmpty(txtWorker.Tag.GetString()))
                GetEqptWrkInfo();
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
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [검사값 입력 : dgQuality_BeginningEdit, LoadedCellPresenter]
        private void dgQuality_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //if (!e.Row.Type.Equals(DataGridRowType.Top))
            //{
            //    if (e.Column.Name.Length > 2)
            //    {
            //        string ColName = e.Column.Name.Substring(e.Column.Name.Length - 2, 2).ToString();
            //        int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Row.DataItem, "CLCT_COUNT"));
            //        int ColCnt = 0;

            //        if (int.TryParse(ColName, out ColCnt))
            //        {
            //            if (ColCnt > MaxCount)
            //            {
            //                e.Cancel = true;
            //            }
            //        }
            //    }

            //}

        }
        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid grid = sender as C1DataGrid;

            grid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                string mandatoryInspectionItemFlag = DataTableConverter.GetValue(e.Cell.Row.DataItem, "MAND_INSP_ITEM_FLAG").GetString();

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    if (e.Cell.Column.Name == "CLCTITEM_CLSS_NAME1")
                    {
                        if (mandatoryInspectionItemFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                        }
                    }
                    else
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));

                }

                if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                {
                    string ColName = e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 2, 2).ToString();
                    int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_COUNT"));
                    int ColCnt = 0;

                    bool InputCheck = true;
                    //string mandatoryInspectionItemFlag = DataTableConverter.GetValue(e.Cell.Row.DataItem, "MAND_INSP_ITEM_FLAG").GetString();

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
                                        SetTextBoxLoadedCellPresenter(sender, panel, cnt, e, InputCheck); // 2023.12.21 동일 소스 합침
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

            C1DataGrid grid = sender as C1DataGrid;

            grid?.Dispatcher.BeginInvoke(new Action(() =>
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
                                SetTextBoxLoadedCellPresenter(sender, panel, cnt, e, InputCheck); // 2023.12.26 동일 소스 합침
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


        // LoadedCellPresenter시 TextBox 제어 부분
        private void SetTextBoxLoadedCellPresenter(object sender, StackPanel panel, int cnt, DataGridCellEventArgs e, bool InputCheck)
        {
            C1DataGrid grid = sender as C1DataGrid;

            TextBox n = panel.Children[cnt] as TextBox;
            n.Background = new SolidColorBrush(Colors.White);
            n.Foreground = new SolidColorBrush(Colors.Black);
            n.FontWeight = FontWeights.Normal;
            n.IsEnabled = true;

            #region Spec Control
            string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[e.Cell.Row.Index].DataItem, "LSL"));
            string _ValueToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[e.Cell.Row.Index].DataItem, "USL"));
            string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[e.Cell.Row.Index].DataItem, "MIN_VALUE"));
            string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[e.Cell.Row.Index].DataItem, "MAX_VALUE"));
            string _ValueToControlVALUE = n.Text;

            string ValueToFind = SpecControl(_ValueToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, false);

            switch (ValueToFind)
            {
                case "SPEC_OUT":
                    n.Background = new SolidColorBrush(Colors.Red);
                    n.Foreground = new SolidColorBrush(Colors.Black);
                    n.FontWeight = FontWeights.Bold;
                    break;
                case "LIMIT_OUT":
                    n.Background = new SolidColorBrush(Colors.White);
                    n.Foreground = new SolidColorBrush(Colors.Blue);
                    n.FontWeight = FontWeights.Bold;
                    break;
                default:
                    n.Background = new SolidColorBrush(Colors.White);
                    n.Foreground = new SolidColorBrush(Colors.Black);
                    n.FontWeight = FontWeights.Normal;
                    break;
            }
            #endregion

            #region SpecExclFlag
            if (InputCheck)
            {
                //n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
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
        #endregion

        #region [검사 주기 변경: cboQualityTime_SelectedValueChanged]
        private void cboQualityTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
        }
        private void cboMQualityTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityMultiInfo();
        }
        private void cboQualityGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
        }

        private void cboLotID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
        }

        private void cboQualityMultiGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityMultiInfo();
        }

        private void cboQualityClctCnt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dgQuality == null || dgQuality.Columns == null || dgQuality.Columns.Count < 1) return;

                if (Util.NVC(cboQualityClctCnt.SelectedValue).Equals(""))
                {
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
                }
                else
                {
                    int Startcol = dgQuality.Columns["ACTDTTM"].Index;

                    for (int col = Startcol + 1; col < dgQuality.Columns.Count; col++)  // CLCTVAL01
                    {
                        dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                    }

                    if (dgQuality.Columns.Contains("CLCTVAL" + Util.NVC(cboQualityClctCnt.SelectedValue)))
                        dgQuality.Columns["CLCTVAL" + Util.NVC(cboQualityClctCnt.SelectedValue)].Visibility = Visibility.Visible;

                    dgQuality.AlternatingRowBackground = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void CLCTVAL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // 2023.12.26 Key.Enter 와 Key.Tab 겹치는 부분 합침
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

                        n.Background = new SolidColorBrush(Colors.White);
                        n.Foreground = new SolidColorBrush(Colors.Black);
                        n.FontWeight = FontWeights.Normal;

                        if (n.Name.IndexOf("txtVal") < 0)
                            return;

                        if (!Util.IS_NUMBER(n.Text))
                        {
                            n.Text = string.Empty;
                            return;
                        }

                        #region Spec Control
                        string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "LSL"));
                        string _ValueToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                        string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                        string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                        string _ValueToControlVALUE = n.Text;

                        string ValueToFind = SpecControl(_ValueToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, true);

                        switch (ValueToFind)
                        {
                            case "SPEC_OUT":
                                n.Background = new SolidColorBrush(Colors.Red);
                                n.Foreground = new SolidColorBrush(Colors.Black);
                                n.FontWeight = FontWeights.Bold;
                                //n.Text = String.Empty;

                                Util.MessageValidation("SFU5038"); //측정 값이 Spec을 벗어납니다. 입력값 및 Spec 값을 확인 바랍니다.
                                break;
                            case "LIMIT_OUT":
                                n.Background = new SolidColorBrush(Colors.White);
                                n.Foreground = new SolidColorBrush(Colors.Blue);
                                n.FontWeight = FontWeights.Bold;
                                break;
                            default:
                                n.Background = new SolidColorBrush(Colors.White);
                                n.Foreground = new SolidColorBrush(Colors.Black);
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

                    if (e.Key == Key.Enter)
                    {
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

                    grid = p.DataGrid;

                    n.Background = new SolidColorBrush(Colors.White);
                    n.Foreground = new SolidColorBrush(Colors.Black);
                    n.FontWeight = FontWeights.Normal;

                    #region Spec Control
                    string _ValueToLSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "LSL"));
                    string _ValueToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                    string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                    string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                    string _ValueToControlVALUE = n.Text;

                    string ValueToFind = SpecControl(_ValueToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                    switch (ValueToFind)
                    {
                        case "SPEC_OUT":
                            n.Background = new SolidColorBrush(Colors.Red);
                            n.Foreground = new SolidColorBrush(Colors.Black);
                            n.FontWeight = FontWeights.Bold;
                            break;
                        case "LIMIT_OUT":
                            n.Background = new SolidColorBrush(Colors.White);
                            n.Foreground = new SolidColorBrush(Colors.Blue);
                            n.FontWeight = FontWeights.Bold;
                            break;
                        default:
                            n.Background = new SolidColorBrush(Colors.White);
                            n.Foreground = new SolidColorBrush(Colors.Black);
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
                    string _ValueToUSL = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "USL"));
                    string _ValueToMIN = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MIN_VALUE"));
                    string _ValueToMAX = Util.NVC(DataTableConverter.GetValue(grid.Rows[p.Cell.Row.Index].DataItem, "MAX_VALUE"));
                    string _ValueToControlVALUE = n.Text;

                    string ValueToFind = SpecControl(_ValueToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                    #region Spec Control
                    switch (ValueToFind)
                    {
                        case "SPEC_OUT":
                            n.Background = new SolidColorBrush(Colors.Red);
                            n.Foreground = new SolidColorBrush(Colors.Black);
                            n.FontWeight = FontWeights.Bold;
                            break;
                        case "LIMIT_OUT":
                            n.Background = new SolidColorBrush(Colors.White);
                            n.Foreground = new SolidColorBrush(Colors.Blue);
                            n.FontWeight = FontWeights.Bold;
                            break;
                        default:
                            n.Background = new SolidColorBrush(Colors.White);
                            n.Foreground = new SolidColorBrush(Colors.Black);
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
        #endregion 

        #region User Method

        #region [BizCall]

        private void SelectProcessName()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtProcess.Text = dtResult.Rows[0]["PROCNAME"].ToString();
                    txtMProcess.Text = dtResult.Rows[0]["PROCNAME"].ToString();
                }
                else
                {
                    txtProcess.Text = string.Empty;
                    txtMProcess.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 검사 시간
        /// </summary>
        private void SetQualityTimeSheet1()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["TYPE_FLAG"] = "S";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_TIME_CBO", "INDATA", "OUTDATA", inTable);

                cboQualityTime.ItemsSource = dtResult.AsDataView();

                cboQualityTime.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetQualityTimeSheet2()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["TYPE_FLAG"] = "M";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_TIME_CBO", "INDATA", "OUTDATA", inTable);

                cboMQualityTime.ItemsSource = dtResult.AsDataView();

                cboMQualityTime.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetQualityGroup()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["TYPE_FLAG"] = "S";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_GRP_CBO_CMM", "INDATA", "OUTDATA", inTable);

                DataTable dtQualityGroup = new DataTable();
                dtQualityGroup.Columns.Add(cboQualityGroup.DisplayMemberPath.ToString(), typeof(string));
                dtQualityGroup.Columns.Add(cboQualityGroup.SelectedValuePath.ToString(), typeof(string));

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtQualityGroup = dtResult.Copy();
                }

                DataRow drSelect = dtQualityGroup.NewRow();
                drSelect[cboQualityGroup.DisplayMemberPath.ToString()] = "-ALL-";
                drSelect[cboQualityGroup.SelectedValuePath.ToString()] = "";
                dtQualityGroup.Rows.InsertAt(drSelect, 0);

                cboQualityGroup.DisplayMemberPath = cboQualityGroup.DisplayMemberPath.ToString();
                cboQualityGroup.SelectedValuePath = cboQualityGroup.SelectedValuePath.ToString();
                cboQualityGroup.ItemsSource = dtQualityGroup.Copy().AsDataView();
                cboQualityGroup.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetQualityMultiGroup()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["TYPE_FLAG"] = "M";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_GRP_CBO_CMM", "INDATA", "OUTDATA", inTable);

                DataTable dtQualityGroup = new DataTable();
                dtQualityGroup.Columns.Add(cboMQualityGroup.DisplayMemberPath.ToString(), typeof(string));
                dtQualityGroup.Columns.Add(cboMQualityGroup.SelectedValuePath.ToString(), typeof(string));

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtQualityGroup = dtResult.Copy();
                }

                DataRow drSelect = dtQualityGroup.NewRow();
                drSelect[cboMQualityGroup.DisplayMemberPath.ToString()] = "-SELECT-";
                drSelect[cboMQualityGroup.SelectedValuePath.ToString()] = "SELECT";
                dtQualityGroup.Rows.InsertAt(drSelect, 0);

                cboMQualityGroup.DisplayMemberPath = cboMQualityGroup.DisplayMemberPath.ToString();
                cboMQualityGroup.SelectedValuePath = cboMQualityGroup.SelectedValuePath.ToString();
                cboMQualityGroup.ItemsSource = dtQualityGroup.Copy().AsDataView();
                cboMQualityGroup.SelectedIndex = dtQualityGroup.Rows.Count > 1 ? 1 : 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetQualityClctCount(int iMax)
        {
            try
            {
                DataTable dtClctCount = new DataTable();
                dtClctCount.Columns.Add(cboQualityClctCnt.DisplayMemberPath.ToString(), typeof(string));
                dtClctCount.Columns.Add(cboQualityClctCnt.SelectedValuePath.ToString(), typeof(string));

                for (int i = 0; i < iMax; i++)
                {
                    DataRow newRow = dtClctCount.NewRow();
                    newRow[cboQualityClctCnt.DisplayMemberPath.ToString()] = (i + 1).ToString();
                    newRow[cboQualityClctCnt.SelectedValuePath.ToString()] = (i + 1).ToString("00");

                    dtClctCount.Rows.Add(newRow);
                }

                DataRow drSelect = dtClctCount.NewRow();
                drSelect[cboQualityClctCnt.DisplayMemberPath.ToString()] = "-ALL-";
                drSelect[cboQualityClctCnt.SelectedValuePath.ToString()] = "";
                dtClctCount.Rows.InsertAt(drSelect, 0);

                cboQualityClctCnt.DisplayMemberPath = cboQualityClctCnt.DisplayMemberPath.ToString();
                cboQualityClctCnt.SelectedValuePath = cboQualityClctCnt.SelectedValuePath.ToString();
                cboQualityClctCnt.ItemsSource = dtClctCount.Copy().AsDataView();
                cboQualityClctCnt.SelectedIndex = 0;

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
                string sBizName = "DA_QCA_COM_SEL_SELF_INSP_CMM";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));
                inTable.Columns.Add("MAND_INSP_ITEM_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["CLCTITEM_CLSS4"] = Util.NVC(cboQualityGroup.SelectedValue).Equals("") ? null : Util.NVC(cboQualityGroup.SelectedValue);
                newRow["LOTID"] = Util.NVC(cboLotID.SelectedValue);
                newRow["WIPSEQ"] = _wipSeq;
                //newRow["ACTDTTM"] = cboQualityTime.SelectedValue ?? cboQualityTime.SelectedValue.ToString();
                newRow["ACTDTTM"] = (cboQualityTime.SelectedValue.ToString().Equals("LOT") || cboQualityTime.SelectedValue.ToString().Trim().Equals("")) ? GetDBDateTime().ToString("yyyy-MM-dd HH:mm:ss") : cboQualityTime.SelectedValue.ToString();
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["TYPE_FLAG"] = "S";
                newRow["MAND_INSP_ITEM_FLAG"] = Util.NVC("");

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQuality, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                var topRows = dtResult.AsEnumerable().Max(x => x.Field<string>("NOTE"));

                DataTableConverter.SetValue(dgdNote.Rows[0].DataItem, "NOTE", Util.NVC(topRows));

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = 0;
                //_maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => Convert.ToInt16(r["CLCT_COUNT"])); // 2024.11.13. 김영국 - DB Type 문제로 인하여 Int로 Parsing.

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


                // 차수 콤보 설정
                cboQualityClctCnt.SelectedValueChanged -= cboQualityClctCnt_SelectedValueChanged;
                SetQualityClctCount(_maxColumn);
                cboQualityClctCnt.SelectedValueChanged += cboQualityClctCnt_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetGQualityMultiInfo()
        {
            if (cboMQualityGroup.SelectedIndex < 0 || cboMQualityGroup.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.gridClear(dgQualityMulti);
                return;
            }

            if (cboMQualityTime.SelectedIndex < 0 || cboMQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.gridClear(dgQualityMulti);
                return;
            }

            try
            {
                #region 자주검사 등록시 검사코드  저장시 매핑, 해더 컨트롤용
                _clctItem = new DataTable();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("TYPE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["CLCTITEM_CLSS4"] = Util.NVC(cboQualityGroup.SelectedValue).Equals("") ? null : Util.NVC(cboQualityGroup.SelectedValue);
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

                inTable.Rows[0]["LOTID"] = txtLotID.Text;
                inTable.Rows[0]["WIPSEQ"] = _wipSeq;
                inTable.Rows[0]["ACTDTTM"] = (cboMQualityTime.SelectedValue.ToString().Equals("LOT") || cboMQualityTime.SelectedValue.ToString().Equals("")) ? GetDBDateTime().ToString("yyyy-MM-dd HH:mm:ss") : cboMQualityTime.SelectedValue.ToString();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_COM_SEL_SELF_INSP_LIST", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQualityMulti, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                var topRows = dtResult.AsEnumerable().Max(x => x.Field<string>("NOTE"));

                DataTableConverter.SetValue(dgdMNote.Rows[0].DataItem, "NOTE", Util.NVC(topRows));

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
                if (!cboQualityTime.SelectedValue.ToString().Equals(""))
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
                Boolean mand_insp_check = false;
                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQuality.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top))
                        continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = Util.NVC(cboLotID.SelectedValue);
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["EQPTID"] = _eqptID;

                    //검사시간: LOT의 경우 차수=1
                    if (!cboQualityTime.SelectedValue.ToString().Equals(""))
                        newRow["CLCTSEQ"] = cboQualityTime.SelectedValue.ToString().Equals("LOT") ? 1 : cboQualityTime.SelectedIndex + 2;

                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                    newRow["ACTDTTM"] = (cboQualityTime.SelectedValue.ToString().Equals("LOT") || cboQualityTime.SelectedValue.ToString().Equals("")) ? GetDBDateTime().ToString("yyyy-MM-dd HH:mm:ss") : cboQualityTime.SelectedValue.ToString();
                    newRow["NOTE"] = DataTableConverter.GetValue(dgdNote.Rows[0].DataItem, "NOTE").GetString();

                    int colValue = 0;

                    for (int col = dgQuality.Columns.Count - _maxColumn; col < dgQuality.Columns.Count; col++)
                    {
                        colValue++;

                        if (colValue > _maxColumn)
                            break;

                        colName = "CLCTVAL" + colValue.ToString("00");

                        //newRow[colName] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, colName));
                        DataRowView drvTmp = (dRow.DataItem as DataRowView);
                        newRow[colName] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals("-")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                        if (newRow[colName].ToString().Equals("NaN")){
                            newRow[colName] = "";
                        }
                    }
                    inTable.Rows.Add(newRow);
                    if (_procID.Equals("A7000")) // 라미 공정 만 필수 값 처리 하도록 변경
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "MAND_INSP_ITEM_FLAG")).Equals("Y"))
                        {
                            if (string.IsNullOrEmpty(newRow[colName].ToString()) || newRow[colName].ToString().Equals("NaN"))
                            {
                                mand_insp_check = true;
                            }
                        }
                    }
                }

                if (mand_insp_check)
                {
                    Util.MessageInfo("SFU3584");      //필수 검사 항목의 측정값을 입력하세요.
                    HiddenLoadingIndicator();
                    return;
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
                        //Util.MessageInfo("SFU1889");

                        if (!ValueToSheet.Equals("ALL"))
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
                if (!cboMQualityTime.SelectedValue.ToString().Equals(""))
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
                            newRow["LOTID"] = txtLotID.Text;
                            newRow["WIPSEQ"] = _wipSeq;
                            newRow["EQPTID"] = _eqptID;
                            //검사시간: LOT의 경우 차수=1
                            if (!cboMQualityTime.SelectedValue.ToString().Equals(""))
                                newRow["CLCTSEQ"] = cboMQualityTime.SelectedValue.ToString().Equals("LOT") ? 1 : cboMQualityTime.SelectedIndex + 2;
                            newRow["USERID"] = LoginInfo.USERID;
                            //newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                            newRow["CLCTITEM"] = Util.NVC(dr[0]["INSP_CLCTITEM"]);
                            newRow["ACTDTTM"] = (cboMQualityTime.SelectedValue.ToString().Equals("LOT") || cboMQualityTime.SelectedValue.ToString().Equals("")) ? GetDBDateTime().ToString("yyyy-MM-dd HH:mm:ss") : cboMQualityTime.SelectedValue.ToString();

                            DataRowView drvTmp = (dRow.DataItem as DataRowView);
                            newRow["CLCTVAL01"] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals(" - ")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";
                            newRow["NOTE"] = DataTableConverter.GetValue(dgdMNote.Rows[0].DataItem, "NOTE").GetString();
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

                        if (!ValueToSheet.Equals("ALL"))
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
        private bool Sheet1ValidateQualitySave()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                // 검사 주기를 선택 하세요.
                Util.MessageValidation("SFU4024");
                return false;
            }

            if (!SpecLimit(dgQuality))
            {
                // 입력값이 최대값/최소값의 범위를 벗어났습니다.
                Util.MessageValidation("SFU4901");
                return false;
            }
            
            if (dgQuality.Rows.Count < 2)
            {
                //저장할 데이터가 존재하지 않습니다.
                Util.MessageValidation("MMD0002");      
                return false;
            }


            return true;
        }

        private bool Sheet2ValidateQualitySave()
        {
            if (cboMQualityTime.SelectedIndex < 0 || cboMQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                // 검사 주기를 선택 하세요.
                Util.MessageValidation("SFU4024");
                return false;
            }

            if (!SpecLimit(dgQualityMulti))
            {
                // 입력값이 최대값/최소값의 범위를 벗어났습니다.
                Util.MessageValidation("SFU4901");
                return false;
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
                newRow["EQSGID"] = _lineID;
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

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
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

        #region 월력관리를 통한 작업자 정보 조회
        private void GetWorkCalander()
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = _lineID;
                Indata["PROCID"] = _procID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORK_CALENDAR_WRKR_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                    txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                    txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                    txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                    txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                    txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                    txtShiftDateTime.Text = Util.NVC(result.Rows[0]["WRK_DTTM_FR_TO"]);
                }
                else
                {
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = _lineID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(txtShift.Tag);
            parameters[5] = Util.NVC(txtWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popupShiftUser.ShowModal()));
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

        private string GetSelfInspSheet()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = _procID;
                dtRow["EQPTID"] = _eqptID;

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

        #region SpecControl
        /** 
         1)	Limit Out 대상 선 체크
         2)	1)번사항 체크 후 상하한값 체크
         **/
        private string SpecControl(string _USL, string _LSL, string _MAX, string _MIN, string _VALUE, bool _Popup)
        {
            double vLSL, vUSL;
            double vMIN, vMAX;
            double vVALUE;
            string ValueToFind = string.Empty;

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

            if (!string.IsNullOrEmpty(ValueToFind))
            {
                return ValueToFind;
            }

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
                    string _ValueToUSL = string.Empty;
                    string _ValueToLSL = string.Empty;
                    string _ValueToMIN = Util.NVC(drvTmp["MIN_VALUE"]);
                    string _ValueToMAX = Util.NVC(drvTmp["MAX_VALUE"]);
                    string _ValueToControlVALUE = Util.NVC(drvTmp[colName]);

                    string ValueToFind = SpecControl(_ValueToUSL, _ValueToLSL, _ValueToMAX, _ValueToMIN, _ValueToControlVALUE, false);

                    if (string.Equals(ValueToFind, "LIMIT_OUT"))
                    {
                        return false;
                    }
                }

            }
            return true;
        }
        #endregion

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            //if (cbo.IsVisible)
            //    cbo.SelectedIndex = 0;
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

        #endregion
        #endregion
    }
}
