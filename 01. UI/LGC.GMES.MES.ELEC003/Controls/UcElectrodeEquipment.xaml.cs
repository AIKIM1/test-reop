/*************************************************************************************
 Created Date : 2020.09.09
      Creator : 정문교
   Decription : 전극 공정진척 - 설비 Tree 
--------------------------------------------------------------------------------------
 [Change History]
 2020.09.09  정문교 : Initial Created.
 2021.02.11  정문교 : 시생산 Row 추가
 2021.07.05  조영대 : W/O 투입가능자재 보기여부 추가
 2021.08.12  김지은 : 시생산샘플설정/해제 기능 추가
 2021.10.12  김지은 : SEQ = 1의 VAR004(기존:WO_DETL_ID) -> VAL009 / VAR008(기존:DEMAND_TYPE) -> VAL004 위치 변경
 2022.11.14  윤기업 : SLURRY_TOP_L2/SLURRY_BACK_L2 추가 
 2023.07.13  김태우 : DAM_MIXING (E0430) 추가
 2024.08.10  : 배현우 [E20240807-000861] : Coater 공정진척 Dam Slurry 투입위치 추가 
 2024.11.29  이동주 : E20240904-000991 [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건(CatchUp)
 2025.04.30  조범모 [MI_LS_OSS_0117] : 슬러리 장착시 더블레이어코팅설비일 경우 코팅면의 위치(Upper/Lower)로 대상호기 기준정보로 선별
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
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ELEC003.Controls
{
    /// <summary>
    /// UcElectrodeEquipment.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcElectrodeEquipment : UserControl
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

        //public bool IsInputUseAuthority { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private DataTable _dtEquipment;
        private string _equipmentCode;
        private bool _isInputUseAuthority;

        public UcElectrodeEquipment()
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
            dgEquipment.SelectedBackground = new SolidColorBrush(Colors.Transparent);
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
                    string equipmentState = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HYPHEN"));
                    string inputYN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_YN"));
                    string Val001 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL001"));             // 1.PRJT_NAME, 2.SHFT_NAME, 3.대 Lot(Title), 4.Foil(Title), 5.Slurry (Top)(Title), 6.Slurry (Back)(Title)
                    string Val002 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL002"));             // 1.PROD_VER_CODE, 2.WRK_USERNAME, 3.대Lot, 4.Foil, 5.Slurry (Top), 6.Slurry (Top)
                    string Val003 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL003")) == "1" ? "True" : "False";             // 4.Foil RadioButton
                    string Val004 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL004"));             // 0.CP_VER, 1.DEMAND_TYPE, 3.대Lot, 4.Foil, 5.Slurry (Back), 6.Slurry (Back)
                    string Val005 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL005")) == "1" ? "True" : "False";             // 4.Foil RadioButton
                    string Val009 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL009"));             // 1.WO_DETL_ID

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                    e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (Seq == 0)
                    {
                        #region 설비 상태(PILOT)
                        if (e.Cell.Column.Name.ToString() == "HYPHEN" && inputYN == "Y")
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                            if (ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION") == equipmentState
                                || ObjectDic.Instance.GetObjectName("PILOT_SMPL_PROD") == equipmentState)   //2021.08.12 : 시생산 샘플 설정 추가
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                            else
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(() =>
                            {
                                var ca = new ColorAnimation();

                                //2021.08.12 : 시생산 샘플 설정 추가
                                if (ObjectDic.Instance.GetObjectName("PILOT_SMPL_PROD") == equipmentState)
                                {
                                    ca.From = (Color)ColorConverter.ConvertFromString("#ffe8ebed");
                                    ca.To = (Color)ColorConverter.ConvertFromString("#ffff9808");
                                    ca.Duration = TimeSpan.FromSeconds(1);
                                    ca.RepeatBehavior = RepeatBehavior.Forever;
                                }
                                else
                                {
                                    ca.From = (Color)ColorConverter.ConvertFromString("#ffe8ebed");
                                    ca.To = Colors.Red;
                                    ca.Duration = TimeSpan.FromSeconds(1);
                                    ca.RepeatBehavior = RepeatBehavior.Forever;
                                }

                                Storyboard.SetTarget(ca, e.Cell.Presenter);
                                Storyboard.SetTargetProperty(ca, new PropertyPath("(Background).(SolidColorBrush.Color)"));
                                Storyboard sb = new Storyboard
                                {
                                    Duration = TimeSpan.FromSeconds(1),
                                    Children = { ca },
                                    AutoReverse = true,
                                    RepeatBehavior = RepeatBehavior.Forever
                                };
                                sb.Begin();
                            }), System.Windows.Threading.DispatcherPriority.Loaded);

                        }

                        #endregion
                    }
                    else if (Seq == 1)
                    {
                        #region 프로젝트, 생산버전, CP버전
                        if (e.Cell.Column.Name.ToString() == "VAL001" && !string.IsNullOrWhiteSpace(Val001))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            e.Cell.Presenter.Cursor = Cursors.Hand;

                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }

                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL008")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }
                        #endregion
                    }
                    else if (Seq == 11)
                    {
                        #region DEMAND유형, WO, 제품
                        if (e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            e.Cell.Presenter.Cursor = Cursors.Hand;

                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }

                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL008")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }
                        #endregion
                    }
                    else if (Seq == 2)
                    {
                        #region 작업조, 작업자   
                        if (e.Cell.Column.Name.ToString() == "VAL002")
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            e.Cell.Presenter.Cursor = Cursors.Hand;

                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }

                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL008")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }

                        // ToolTip 작업자의 작업시작일시 ~ 작업종료일시
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                            ToolTipService.SetToolTip(e.Cell.Presenter, Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_STRT_DTTM")) + " ~ " + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_END_DTTM")));

                        #endregion
                    }
                    else if (Seq == 3)
                    {
                        #region 대 Lot 
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL002")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        #endregion
                    }
                    else if (Seq == 4)
                    {
                        #region Foil  
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL002")
                        {
                            if (Val003 == "1" || Boolean.Parse(Val003)) // 2024.10.31. 김영국 - Val값이 True인 경우도 색 변경 가능하도록 수정.
                            {
                                RadioButton rb = e.Cell.Presenter.Content as RadioButton;
                                (dgEquipment.GetCell(e.Cell.Row.Index, dgEquipment.Columns["VAL005"].Index).Presenter.Content as RadioButton).IsChecked = false;
                                DataTableConverter.SetValue(dgEquipment.Rows[e.Cell.Row.Index].DataItem, "VAL003", true);
                                DataTableConverter.SetValue(dgEquipment.Rows[e.Cell.Row.Index].DataItem, "VAL005", false);
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF80FF00"));
                            }
                            else
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL003" || e.Cell.Column.Name.ToString() == "VAL005")
                        {
                            RadioButton rodbtn = e.Cell.Presenter.Content as RadioButton;
                            if (_isInputUseAuthority)
                                rodbtn.IsEnabled = true;
                            else
                                rodbtn.IsEnabled = false;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            if (Val005 == "1" || Boolean.Parse(Val005)) // 2024.10.31. 김영국 - Val값이 True인 경우도 색 변경 가능하도록 수정.
                            {
                                RadioButton rodbtn = e.Cell.Presenter.Content as RadioButton;
                                (dgEquipment.GetCell(e.Cell.Row.Index, dgEquipment.Columns["VAL003"].Index).Presenter.Content as RadioButton).IsChecked = false;
                                DataTableConverter.SetValue(dgEquipment.Rows[e.Cell.Row.Index].DataItem, "VAL003", false);
                                DataTableConverter.SetValue(dgEquipment.Rows[e.Cell.Row.Index].DataItem, "VAL005", true);
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF80FF00"));
                            }
                            else
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                        #endregion

                    }
                    else if (Seq == 5 || Seq == 6)
                    {
                        #region Slurry (Top) / Slurry (Back)
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL002" || e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            if (Seq == 5)
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                            }
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                        #endregion
                    }
                    else if (Seq == 7)
                    {
                        #region 재와인딩 Lot 
                        if (e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        #endregion
                    }
                    else if (Seq == 8)
                    {
                        #region [RollPress 투입부 장착 Lot/보빈]
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        if (e.Cell.Column.Name.ToString() == "VAL002" || e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                            }
                        }
                        #endregion
                    }
                    else if (Seq == 9)
                    {
                        #region [재와인딩 투입부 장착 Lot/보빈]
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        if (e.Cell.Column.Name.ToString() == "VAL002" || e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                            }
                        }
                        if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            if (e.Cell.Value != null && !string.IsNullOrEmpty(e.Cell.Value.ToString()))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.Cursor = Cursors.Hand;
                            }
                        }
                        #endregion
                    }
                    else if (Seq == 10)
                    {
                        #region Slurry (Dam)  2024-07-03 Dam Mixer 슬러리 장착 위치 추가
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL002" || e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }

                        #endregion
                    }
                    if (e.Cell.Row.ParentGroup.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                    {
                        // 설비 On-Line여부 배경색 표시 : 녹색 – On-line / 적색 – Off-line
                        string EqptID = e.Cell.Row.ParentGroup.DataItem.ToString().Split(':')[0].Trim();
                        DataRow[] dr = _dtEquipment.Select("EQPTID = '" + EqptID + "' And SEQ = 2");

                        if (dr.Length > 0)
                        {
                            if (dr[0]["EQPT_ONLINE_FLAG"].ToString() == "Y")
                            {
                                e.Cell.Row.ParentGroup.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff67e09c"));
                            }
                            else
                            {
                                e.Cell.Row.ParentGroup.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fffe7a7a"));
                            }

                        }
                    }

                }
            }));

        }

        private void dgEquipment_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (e.Cell.Row.ParentGroup.Presenter != null)
                //{
                //    if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                //    {
                //        e.Cell.Row.ParentGroup.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                //    }
                //}

                //if (e.Cell.Presenter != null)
                //{
                //    if (e.Cell.Row.Type == DataGridRowType.Item)
                //    {
                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Transparent);
                //        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                //    }
                //}
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

                if (Seq == 0)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["HYPHEN"].Index), dg.GetCell(row, dg.Columns["VAL008"].Index)));
                }
                else if (Seq == 1)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL007"].Index), dg.GetCell(row, dg.Columns["VAL008"].Index)));
                }
                else if (Seq == 11)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL007"].Index), dg.GetCell(row, dg.Columns["VAL008"].Index)));
                }
                else if (Seq == 2)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                }
                else if (Seq == 4)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                }
                else if (Seq == 8)
                {
                    // 롤프레스 투입부 장착 Lot/보빈
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                }
                else if (Seq == 9)
                {
                    // 재와인딩 투입부 장착 Lot/보빈
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                }
                else
                {
                    // Coating : Foil이 아니면 
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));

                    if (Seq == 5)   // Coating : Slurry(Top)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }

                    if (Seq == 7)
                    {
                        // 재와인딩
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                    }
                }
            }
        }

        private void dgEquipment_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null || dg.SelectedIndex == -1) return;

            int Seq = Util.NVC_Int(dg.GetCell(dg.CurrentRow.Index, dg.Columns["SEQ"].Index).Value);

            if (dg.CurrentCell.Column.Name == "VAL002")
            {
                if (_isInputUseAuthority == false)
                {
                    e.Cancel = true;    // Editing 불가능
                    return;
                }

                if (Seq == 4)
                    e.Cancel = false;   // Editing 가능 (투입lot, 재와인딩 LOT)
                else
                    e.Cancel = true;    // Editing 불가능
            }
            else if (dg.CurrentCell.Column.Name == "VAL004")
            {
                if (_isInputUseAuthority == false)
                {
                    e.Cancel = true;    // Editing 불가능
                    return;
                }

                if (Seq == 4 || Seq == 7)
                    e.Cancel = false;   // Editing 가능 (투입lot, 재와인딩 LOT)
                else
                    e.Cancel = true;    // Editing 불가능
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

            if (seq == 4)
            {
                if (cell.Column.Name.ToString() == "VAL006")
                {
                    // 자재 장착
                    if (_isInputUseAuthority)
                        SetMtrlMount();
                }
            }
            else if (seq == 5)
            {
                if (cell.Column.Name.ToString() == "VAL006")
                {
                    // 자재 탈착
                    if (_isInputUseAuthority)
                        PopupMtrlUnmount();
                }
            }
            // 재와인딩 투입부 장착 Lot/보빈 탈착
            else if (seq == 9)
            {
                if (cell.Column.Name.ToString() == "VAL006")
                {
                    DataRow[] drInputLot = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And (SEQ = 9)");
                    if (drInputLot != null && drInputLot.Length > 0 && !string.IsNullOrEmpty(drInputLot[0]["VAL006"].ToString()))
                    {
                        // 투입취소 하시겠습니까?
                        Util.MessageConfirm("SFU1988", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                // 자재 탈착
                                SetReWindUnMount(drInputLot[0]);
                            }
                        });
                    }
                }
            }
        }

        private void dgEquipment_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEquipment.GetCellFromPoint(pnt);

            if (cell != null)
            {
                DataRowView dr = dgEquipment.Rows[cell.Row.Index].DataItem as DataRowView;

                // 2024.10.29. 김영국 - 코팅 Slurry 장착 시 VAL004값이 Null인 경우는 Popup을 띄우지 않는다.
                if (dr.Row["EQPTNAME"].ToString().Contains("(C)"))
                {
                    if (cell.Column.Name == "VAL004")
                        if (cell.Text == "")
                        {
                            return;
                        }
                }

                int Seq = Util.NVC_Int(dr["SEQ"]);
                string Shft_ID = Util.NVC(dr["SHFT_ID"]);
                string WorkUser = Util.NVC(dr["WRK_USERID"]);
                string inputYN = Util.NVC(dr["INPUT_YN"]);
                string Val001 = Util.NVC(dr["VAL001"]); // 1.PRJT_NAME, 2.SHFT_NAME, 3.대 Lot(Title), 4.Foil(Title), 5.Slurry (Top)(Title), 6.Slurry (Back)(Title)
                string Val002 = Util.NVC(dr["VAL002"].ToString().Split(' ')[0]); // 1.PROD_VER_CODE, 2.WRK_USERNAME, 3.대Lot, 4.Foil, 5.Slurry (Top), 6.Slurry (Top)
                string Val004 = Util.NVC(dr["VAL004"].ToString().Split(' ')[0]); // 1.DEMAND_TYPE, 3.대Lot, 4.Foil, 5.Slurry (Back), 6.Slurry (Back)
                string Val007 = Util.NVC(dr["VAL007"]); // 1.PRODID
                string Val009 = Util.NVC(dr["VAL009"]); // 1.WO_DETL_ID
                string Val010 = string.IsNullOrEmpty(Val002) ? null : Util.NVC(dr["VAL002"].ToString().Replace(Val002, "").Trim().Replace("(", "").Replace(")", "")); // SurfacePosition (Upper/Lower)
                string Val011 = string.IsNullOrEmpty(Val004) ? null : Util.NVC(dr["VAL004"].ToString().Replace(Val004, "").Trim().Replace("(", "").Replace(")", "")); // SurfacePosition (Upper/Lower)

                if (String.IsNullOrEmpty(Val007))
                {
                    DataRow[] drTemp = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 11");
                    if (drTemp.Length > 0)
                    {
                        Val007 = drTemp[0]["VAL007"].ToString();
                    }
                }

                if (Seq == 1)
                {
                    if (cell.Column.Name.ToString() == "VAL001")
                    {
                        // 팝업호출 : 작업지시서
                        PopupGPLM(Val007);
                    }
                }
                else if (Seq == 11)
                {
                    if (cell.Column.Name.ToString() == "VAL004" || cell.Column.Name.ToString() == "VAL006")
                    {
                        // 팝업호출 : WorkOrder
                        if (ProcessCode == Process.MIXING ||
                            ProcessCode == Process.PRE_MIXING ||
                            ProcessCode == Process.BS ||
                            ProcessCode == Process.CMC ||
                            ProcessCode == Process.InsulationMixing ||
                            ProcessCode == Process.DAM_MIXING
                            )
                            PopupWorkOrderMix();
                        else
                            PopupWorkOrder();
                    }
                }
                else if (Seq == 2)
                {
                    if (cell.Column.Name.ToString() == "VAL006")
                    {
                        // 팝업호출 : 작업자
                        PopupWorker(Shft_ID, WorkUser);
                    }
                }
                //else if (Seq == 4)
                //{
                //    if (cell.Column.Name.ToString() == "VAL006")
                //    {
                //        // 자재 장착
                //        SetMtrlMount();
                //    }
                //}
                else if (Seq == 5)
                {
                    if (cell.Column.Name.ToString() == "VAL002")
                    {
                        // 팝업호출 : Top Slurry
                        if (_isInputUseAuthority)
                        {
                            if (inputYN == "Y")
                                PopupSlurry(0, null);
                            else
                                PopupSlurry(0, Val002, Val010);
                        }
                    }
                    else if (cell.Column.Name.ToString() == "VAL004")
                    {
                        // 팝업호출 : Top Slurry
                        if (_isInputUseAuthority)
                            PopupSlurry(2, Val004, Val011);
                    }
                    //else if (cell.Column.Name.ToString() == "VAL006")
                    //{
                    //    // 자재 탈착
                    //    PopupMtrlUnmount();
                    //}
                }
                else if (Seq == 6)
                {
                    if (cell.Column.Name.ToString() == "VAL002")
                    {
                        // 팝업호출 : Back Slurry
                        if (_isInputUseAuthority)
                        {
                            if (inputYN == "Y")
                                PopupSlurry(1, null);
                            else
                                PopupSlurry(1, Val002, Val010);
                        }
                    }
                    else if (cell.Column.Name.ToString() == "VAL004")
                    {
                        // 팝업호출 : Back Slurry
                        if (_isInputUseAuthority)
                            PopupSlurry(3, Val004, Val011);
                    }
                }
                else if (Seq == 10)
                {
                    if (cell.Column.Name.ToString() == "VAL002")
                    {
                        // 팝업호출 : Back Slurry
                        if (_isInputUseAuthority)
                        {
                            if (inputYN == "Y")
                                PopupSlurry(4, null);
                            else
                                PopupSlurry(4, Val002);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// ReWinding 공정이동 처리
        /// </summary>
        private void dgEquipment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null) return;

                if (dg.CurrentCell == null) return;

                if (dg.CurrentCell.Column.Name != "VAL004") return;

                int Seq = Util.NVC_Int(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SEQ"));

                if (Seq != 7) return;

                string MoveLotID = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "VAL004"));

                if (!ValidationReWindingMove(MoveLotID)) return;

                //재와인더로 이동하시겠습니까?
                Util.MessageConfirm("SFU1872", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MoveProcess(MoveLotID);

                        // Clear
                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, "VAL004", string.Empty);
                    }
                });
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_ELEC_EQUIPMENT_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //dtresult.Rows.Add("N3ECOTM01", "N3ECOTM01 : CNB1동 COATER 양극 1호", "Y", "Y", 1, "", "", "－", "E63F", "001", "c20823CT0009", "[변경]", "APCCA1368A", "양산");
                        //dtresult.Rows.Add("N3ECOTM01", "N3ECOTM01 : CNB1동 COATER 양극 1호", "Y", "Y", 2, "B1", "dgsoul,sjkim2019", "－", "주간", "홍길동, 김철수, 김영희", "", "[변경]", "", "");

                        Util.GridSetData(dgEquipment, bizResult, null);
                        _dtEquipment = bizResult.Copy();

                        dgEquipment.GroupBy(dgEquipment.Columns["EQPTNAME"], DataGridSortDirection.None);
                        dgEquipment.GroupRowPosition = DataGridGroupRowPosition.AboveData;

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

        /// <summary>
        /// 자재 장착 처리
        /// </summary>
        private void SaveMountChange(bool IsCurrentFoil = true, bool IsSlurryTerm = false, bool IsCoreTerm = false, bool IsTopSlurryChange = true, bool IsBackSlurryChange = true, bool IsSlurryL2Change = false)
        {
            DataTable mountDt = GetCurrentMount(_equipmentCode);

            BizDataSet bizRule = new BizDataSet();
            DataSet indataSet = new DataSet();

            DataTable inTable = indataSet.Tables.Add("INDATA");
            inTable.Columns.Add("SRCTYPE", typeof(string));
            inTable.Columns.Add("IFMODE", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataTable inInputLot = indataSet.Tables.Add("INPUT_LOT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("MTRLID", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
            inInputLot.Columns.Add("TERM_FLAG", typeof(string));
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            DataRow newRow = inTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            newRow["EQPTID"] = _equipmentCode;
            newRow["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            #region Foil 변경

            // PSTN ID QUERY해서 변경하여 변경된것만 체크 후 저장하게 변경 ( 2017-01-23 )
            DataRow[] rows = mountDt.Copy().Select("MTRL_CLSS_CODE = 'MFL'");

            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2987", new object[] { _equipmentCode });  //해당 설비({%1})의 등록된 Foil정보가 존재하지 않습니다.
                return;
            }

            // SET CORE
            // 직접 입력 받는 부분은 DataGrid에서 갖고 온다
            ////DataRow[] drFoil = _dtEquipment.Select("SEQ = 4");
            DataRow[] drFoil = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4");

            string sMtrlID = string.Empty;
            string sInputLotID = string.Empty;
            string sCheck = string.Empty;

            if (rows.Length > 0 && IsCurrentFoil == true && drFoil.Length > 0)
            {
                sMtrlID = drFoil[0]["MTRLID1"].ToString();
                sInputLotID = drFoil[0]["VAL002"].ToString();
                sCheck = drFoil[0]["VAL003"].ToString();

                newRow = null;
                newRow = inInputLot.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = sCheck == "1" ? !string.IsNullOrWhiteSpace(sInputLotID.Trim()) ? "A" : "S" : "S";
                newRow["MTRLID"] = sMtrlID;
                newRow["INPUT_LOTID"] = sInputLotID;
                newRow["TERM_FLAG"] = IsCoreTerm ? "Y" : string.Empty;
                inInputLot.Rows.Add(newRow);
            }

            if (rows.Length > 1 && IsCurrentFoil == true && drFoil.Length > 0)
            {
                sMtrlID = drFoil[0]["MTRLID2"].ToString();
                sInputLotID = drFoil[0]["VAL004"].ToString();
                sCheck = drFoil[0]["VAL005"].ToString();

                newRow = null;
                newRow = inInputLot.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = sCheck == "1" ? !string.IsNullOrWhiteSpace(sInputLotID.Trim()) ? "A" : "S" : "S";
                newRow["MTRLID"] = sMtrlID;
                newRow["INPUT_LOTID"] = sInputLotID;
                newRow["TERM_FLAG"] = IsCoreTerm ? "Y" : string.Empty;

                inInputLot.Rows.Add(newRow);
            }
            #endregion

            #region SLURRY 변경

            rows = null;
            rows = mountDt.Copy().Select("PRDT_CLSS_CODE = 'ASL'");

            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2988", new object[] { _equipmentCode });  //해당 설비({%1})의 등록된 Slurry정보가 존재하지 않습니다.
                return;
            }

            // SET SLURRY TOP 
            DataRow[] drSlurryTop = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5");
            sInputLotID = string.Empty;

            if (rows.Length > 0 && IsTopSlurryChange == true && drSlurryTop.Length > 0)
            {
                if (!IsSlurryL2Change)
                {
                    sMtrlID = drSlurryTop[0]["MTRLID1"].ToString();
                    if (drSlurryTop[0]["INPUT_YN"].ToString() == "N")
                        sInputLotID = drSlurryTop[0]["VAL002"].ToString().Split(' ')[0];
                }
                else
                {
                    sMtrlID = drSlurryTop[0]["MTRLID2"].ToString();
                    if (drSlurryTop[0]["INPUT_YN"].ToString() == "N")
                        sInputLotID = drSlurryTop[0]["VAL004"].ToString().Split(' ')[0];
                }


                newRow = null;
                newRow = inInputLot.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = !IsSlurryL2Change ? Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]) : Util.NVC(rows[2]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrWhiteSpace(sInputLotID) ? "A" : "S";
                newRow["MTRLID"] = sMtrlID;
                newRow["INPUT_LOTID"] = sInputLotID;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inInputLot.Rows.Add(newRow);
            }

            // SET SLURRY BACK 
            DataRow[] drSlurryBack = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6");
            sInputLotID = string.Empty;

            if (rows.Length > 1 && IsBackSlurryChange == true && drSlurryBack.Length > 0)
            {
                if (!IsSlurryL2Change)
                {
                    sMtrlID = drSlurryBack[0]["MTRLID1"].ToString();
                    if (drSlurryBack[0]["INPUT_YN"].ToString() == "N")
                        sInputLotID = drSlurryBack[0]["VAL002"].ToString().Split(' ')[0];
                }
                else
                {
                    sMtrlID = drSlurryBack[0]["MTRLID2"].ToString();
                    if (drSlurryBack[0]["INPUT_YN"].ToString() == "N")
                        sInputLotID = drSlurryBack[0]["VAL004"].ToString().Split(' ')[0];
                }
                newRow = null;
                newRow = inInputLot.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = !IsSlurryL2Change ? Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]) : Util.NVC(rows[3]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrWhiteSpace(sInputLotID) ? "A" : "S";
                newRow["MTRLID"] = sMtrlID;
                newRow["INPUT_LOTID"] = sInputLotID;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inInputLot.Rows.Add(newRow);
            }
            #endregion
            #region DAM SLURRY 변경
            rows = null;
            rows = mountDt.Copy().Select("PRDT_CLSS_CODE = 'ASL' AND EQPT_MOUNT_PSTN_ID LIKE '%DM%'");


            // SET SLURRY DAM 
            DataRow[] drSlurryDam = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 10");
            sInputLotID = string.Empty;

            if (rows.Length > 0 && drSlurryDam.Length > 0)
            {

                sMtrlID = drSlurryDam[0]["MTRLID1"].ToString();
                if (drSlurryDam[0]["INPUT_YN"].ToString() == "N")
                    sInputLotID = drSlurryDam[0]["VAL002"].ToString();

                newRow = null;
                newRow = inInputLot.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrWhiteSpace(sInputLotID) ? "A" : "S";
                newRow["MTRLID"] = sMtrlID;
                newRow["INPUT_LOTID"] = sInputLotID;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inInputLot.Rows.Add(newRow);
            }
            #endregion
            if (inInputLot.Rows.Count == 0)
                return;

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_USE_MTRL_LOT_CT", "INDATA,INPUT_LOT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    ChangeEquipment(ProcessCode, EquipmentCode);

                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                ChangeEquipment(ProcessCode, EquipmentCode);

            }, indataSet);
        }

        /// <summary>
        /// 설비 장착정보 조회
        /// </summary>
        private DataTable GetCurrentMount(string sEqptID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = sEqptID;
                inTable.Rows.Add(newRow);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        /// <summary>
        /// 재와인더 이동 Lot 정보
        /// </summary>
        private DataTable GetLotInfo(string MoveLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["LOTID"] = MoveLotID;
            newRow["EQSGID"] = EquipmentSegmentCode;
            inTable.Rows.Add(newRow);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_RW_INFO", "INDATA", "OUTDATA", inTable);
        }

        /// <summary>
        /// 재와인더 이동
        /// </summary>
        private void MoveProcess(string MoveLotID)
        {
            try
            {
                DataTable dtLot = GetLotInfo(MoveLotID);

                if (dtLot.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2025");  // 해당하는 LOT정보가 없습니다.
                    return;
                }

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID_FR", typeof(string));
                inTable.Columns.Add("PROCID_TO", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID_FR"] = dtLot.Rows[0]["PROCID"].ToString();
                newRow["PROCID_TO"] = ProcessCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["NOTE"] = "";
                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                newRow = inLot.NewRow();
                newRow["LOTID"] = MoveLotID;
                inLot.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVE_RW", "INDATA,IN_LOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1766");     // 이동완료

                    // 생산Lot 재조회
                    SetUserControlProductLotList(MoveLotID);
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion;

        #region [Func]
        public void SetPermissionPerButton(string groupCode)
        {
            try
            {
                switch (groupCode)
                {
                    case "INPUT_W": // 투입 사용 권한
                        _isInputUseAuthority = true;
                        break;
                    default:
                        _isInputUseAuthority = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 자재 장착
        /// </summary>
        private void SetMtrlMount()
        {
            string WorkOrderID = string.Empty;

            DataRow[] dr = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 11");

            if (dr.Length > 0)
            {
                WorkOrderID = dr[0]["VAL009"].ToString();
            }

            if (!string.IsNullOrWhiteSpace(_equipmentCode) && !string.IsNullOrEmpty(WorkOrderID))
            {
                // 해당 자재를 변경하시겠습니까?
                Util.MessageConfirm("SFU2989", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        SaveMountChange();
                    }
                });
            }
        }

        /// <summary>
        /// 재와인딩 투입부 장착 Lot/보빈 탈착
        /// </summary>
        private void SetReWindUnMount(DataRow dr)
        {
            try
            {
                if (dr == null)
                {
                    Util.MessageValidation("SFU2025");  // 해당하는 LOT정보가 없습니다.
                    return;
                }

                DataTable mountDt = GetCurrentMount(_equipmentCode);
                if (mountDt == null || mountDt.Rows.Count == 0)
                    return;

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = ProcessCode;
                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("IN_INPUT");
                inLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inLot.Columns.Add("INPUT_LOTID", typeof(string));

                newRow = inLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = mountDt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = mountDt.Rows[0]["INPUT_STATE_CODE"].ToString();
                newRow["INPUT_LOTID"] = mountDt.Rows[0]["INPUT_LOTID"].ToString();
                inLot.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_LOT_CANCEL2_RW_DRB", "INDATA,IN_INPUT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상처리되었습니다.

                    // 설비 정보 재 조회
                    ChangeEquipment(ProcessCode, EquipmentCode);

                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        protected virtual void SetUserControlProductLotList(string MoveLot)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("OutSideProductLotList");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = MoveLot;

                methodInfo.Invoke(UcParentControl, parameterArrys);
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

        #region[[Validation]
        private bool ValidationReWindingMove(string MoveLotID)
        {
            if (string.IsNullOrWhiteSpace(MoveLotID))
            {
                Util.MessageValidation("SFU1366");     // LOT ID를 입력해주세요
                return false;
            }

            return true;
        }

        #endregion

        #region 팝업
        /// <summary>
        /// 작업지시서 팝업
        /// </summary>
        private void PopupGPLM(string ProdID)
        {
            CMM_ELEC_PRDT_GPLM popupGPLM = new CMM_ELEC_PRDT_GPLM();
            popupGPLM.FrameOperation = this.FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = ProdID;

            if (string.Equals(ProcessCode, Process.PRE_MIXING) || string.Equals(ProcessCode, Process.MIXING) || string.Equals(ProcessCode, Process.SRS_MIXING))
                parameters[1] = Gplm_Process_Type.MIXING;
            else if (string.Equals(ProcessCode, Process.COATING) || string.Equals(ProcessCode, Process.SRS_COATING))
                parameters[1] = Gplm_Process_Type.COATING;
            else
                parameters[1] = Gplm_Process_Type.RTS;

            C1WindowExtension.SetParameters(popupGPLM, parameters);
            this.Dispatcher.BeginInvoke(new Action(() => popupGPLM.ShowModal()));
        }

        /// <summary>
        /// WorkOrder Mixing 팝업
        /// </summary>
        private void PopupWorkOrderMix()
        {
            CMM_WORKORDER_MX_DRB popupWorkOrderMix = new CMM_WORKORDER_MX_DRB();
            popupWorkOrderMix.FrameOperation = this.FrameOperation;

            if (popupWorkOrderMix != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = EquipmentSegmentCode;
                Parameters[1] = ProcessCode;
                Parameters[2] = _equipmentCode;
                C1WindowExtension.SetParameters(popupWorkOrderMix, Parameters);

                popupWorkOrderMix.Closed += new EventHandler(PopupWorkOrderMix_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorkOrderMix.ShowModal()));
            }
        }

        private void PopupWorkOrderMix_Closed(object sender, EventArgs e)
        {
            CMM_WORKORDER_MX_DRB popup = sender as CMM_WORKORDER_MX_DRB;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            ChangeEquipment(ProcessCode, EquipmentCode, true);
            //}
        }

        /// <summary>
        /// WorkOrder 팝업
        /// </summary>
        private void PopupWorkOrder()
        {
            CMM_WORKORDER_DRB popupWorkOrder = new CMM_WORKORDER_DRB();
            popupWorkOrder.FrameOperation = this.FrameOperation;

            // W/O 투입가능자재 보기여부 추가
            switch (ProcessCode)
            {
                case ElectrodeProcesses.SLITTING:
                    popupWorkOrder.IsViewWOInputMaterial = false;
                    break;
            }

            if (popupWorkOrder != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = EquipmentSegmentCode;
                Parameters[1] = ProcessCode;
                Parameters[2] = _equipmentCode;
                C1WindowExtension.SetParameters(popupWorkOrder, Parameters);

                popupWorkOrder.Closed += new EventHandler(PopupWorkOrder_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorkOrder.ShowModal()));
            }
        }

        private void PopupWorkOrder_Closed(object sender, EventArgs e)
        {
            CMM_WORKORDER_DRB popup = sender as CMM_WORKORDER_DRB;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            ChangeEquipment(ProcessCode, EquipmentCode, true);
            //}
        }

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
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            ChangeEquipment(ProcessCode, EquipmentCode, true);
            //}
        }

        /// <summary>
        /// Slurry 팝업
        /// Top Bool 형식( TOP/BACK) 에서 INT 0(TOP),1(BACK), 2(TOP_L2), 3(BACK_L2) 로 변경 [윤기업 2022-11-14 ]
        /// </summary>
        private void PopupSlurry(int Top, string SlurryID, string SurfacePosition = null)
        {
            string WorkOrderID = string.Empty;
            string WideRollFlag = string.Empty;

            DataRow[] dr = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 11");

            if (dr.Length > 0)
            {
                WorkOrderID = dr[0]["VAL009"].ToString();
                WideRollFlag = dr[0]["WIDE_ROLL_FLAG"].ToString();
            }

            CMM_ELEC_SLURRY popupSlurry = new CMM_ELEC_SLURRY();
            popupSlurry.FrameOperation = this.FrameOperation;

            if (popupSlurry != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = ProcessCode;
                Parameters[1] = EquipmentSegmentCode;
                Parameters[2] = WorkOrderID;
                Parameters[3] = _equipmentCode;
                Parameters[4] = Top;
                Parameters[5] = SlurryID;
                Parameters[6] = "N";                                // isSingleCoater == true ? "Y" : "N";
                Parameters[7] = WideRollFlag;
                Parameters[8] = SurfacePosition;
                C1WindowExtension.SetParameters(popupSlurry, Parameters);

                popupSlurry.Closed += new EventHandler(PopupSlurry_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupSlurry.ShowModal()));
            }
        }

        private void PopupSlurry_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY popup = sender as CMM_ELEC_SLURRY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    // SLURRY_TOP
                    if (popup._ReturnPosition == 0)
                    {
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["VAL002"] = popup._ReturnLotID);
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["MTRLID1"] = popup._ReturnPRODID);

                        if (popup._IsAllConfirm == true)
                        {
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["VAL002"] = popup._ReturnLotID);
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["MTRLID1"] = popup._ReturnPRODID);
                        }
                    }
                    // SLURRY_TOP_L2
                    else if (popup._ReturnPosition == 2)
                    {
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["VAL004"] = popup._ReturnLotID);
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["MTRLID2"] = popup._ReturnPRODID);

                        if (popup._IsAllConfirm == true)
                        {
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["VAL004"] = popup._ReturnLotID);
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["MTRLID2"] = popup._ReturnPRODID);
                        }
                    }
                    // SLURRY_BACK
                    else if (popup._ReturnPosition == 1)
                    {
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["VAL002"] = popup._ReturnLotID);
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["MTRLID1"] = popup._ReturnPRODID);

                        if (popup._IsAllConfirm == true)
                        {
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["VAL002"] = popup._ReturnLotID);
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["MTRLID1"] = popup._ReturnPRODID);
                        }
                    }
                    else if (popup._ReturnPosition == 4) //DAM SLURRY
                    {
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 10").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 10").ToList<DataRow>().ForEach(r => r["VAL002"] = popup._ReturnLotID);
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 10").ToList<DataRow>().ForEach(r => r["MTRLID1"] = popup._ReturnPRODID);
                    }
                    // SLURRY_BACK_L2
                    else
                    {
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["VAL004"] = popup._ReturnLotID);
                        _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 6").ToList<DataRow>().ForEach(r => r["MTRLID2"] = popup._ReturnPRODID);

                        if (popup._IsAllConfirm == true)
                        {
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["INPUT_YN"] = "N");
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["VAL004"] = popup._ReturnLotID);
                            _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5").ToList<DataRow>().ForEach(r => r["MTRLID2"] = popup._ReturnPRODID);
                        }
                    }

                    _dtEquipment.AcceptChanges();

                    // Slurry만 장착 처리
                    if (popup._IsAllConfirm == true)
                        SaveMountChange(false, popup._IsSlurryTerm, false, true, true, popup._ReturnPosition == 2 || popup._ReturnPosition == 3 ? true : false);
                    else
                        SaveMountChange(false, popup._IsSlurryTerm, false, popup._ReturnPosition == 0 || popup._ReturnPosition == 2 ? true : false, popup._ReturnPosition == 1 || popup._ReturnPosition == 3 ? true : false, popup._ReturnPosition == 2 || popup._ReturnPosition == 3 ? true : false);
                }
            }
        }

        /// <summary>
        /// 자재 탈착
        /// </summary>
        private void PopupMtrlUnmount()
        {
            string WorkOrderID = string.Empty;
            string sSlurry = string.Empty;

            DataRow[] dr = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 11");

            if (dr.Length > 0)
                WorkOrderID = dr[0]["VAL009"].ToString();

            DataRow[] drSlurry = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And (SEQ = 5 Or SEQ = 6)");

            if (drSlurry.Length > 0)
            {
                for (int row = 0; row < drSlurry.Length; row++)
                {
                    if (drSlurry[row]["INPUT_YN"].ToString() != "Y")
                    {
                        sSlurry = drSlurry[row]["VAL002"].ToString().Split(' ')[0];
                        break;
                    }
                }
            }

            CMM_ELEC_SLURRY_TERM popupSlurryTerm = new CMM_ELEC_SLURRY_TERM();
            popupSlurryTerm.FrameOperation = this.FrameOperation;

            if (popupSlurryTerm != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = ProcessCode;
                Parameters[1] = EquipmentSegmentCode;
                Parameters[2] = WorkOrderID;
                Parameters[3] = _equipmentCode;
                Parameters[4] = 1;                                                               // 의미 없음 : 팝업에서 설비의 장착정보 리스트 조회 ((Button)sender).Name == "btnTopSlurry1" ? 0 : 1;
                Parameters[5] = sSlurry;                                                         // 의미 없음 ((Button)sender).Name == "btnTopSlurry1" ? txtTopSlurry1.Text : txtBackSlurry1.Text;
                Parameters[6] = "N";
                C1WindowExtension.SetParameters(popupSlurryTerm, Parameters);

                popupSlurryTerm.Closed += new EventHandler(PopupSlurryTerm_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupSlurryTerm.ShowModal()));
            }

        }

        private void PopupSlurryTerm_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY_TERM popup = sender as CMM_ELEC_SLURRY_TERM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
        }

        #endregion

        #endregion

    }
}
