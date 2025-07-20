/*************************************************************************************
 Created Date : 2021.12.08
      Creator : 신광희
   Decription : 소형 조립 공정진척(NFF) - 트리구조형태의 UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.12.08  신광희 : Initial Created.
 2023.10.25  김용군 : 오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
 2024.02.21  백광영 : 오창 2산단 소형 조립 설비 재작업 모드 조회 추가
 2024.07.18  백광영 : 오창 2산단 소형 조립 설비 재작업 모드 기능 삭제
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Data;
using System.Windows.Media;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using C1.C1Report;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY006.Controls
{
    /// <summary>
    /// UcAssemblyEquipment.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyEquipment : UserControl
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        public UserControl UcParentControl;

        public C1DataGrid DgEquipment { get; set; }

        public string EquipmentSegmentCode { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string SelectEquipmentCode { get; set; }
        public string ProductLotId { get; set; }
        
        public bool IsInputUseAuthority { get; set; }
        public bool IsWaitUseAuthority { get; set; }
        public bool IsInputHistoryAuthority { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private DataTable _dtEquipment;
        private string _equipmentCode;
        private string _equipmentMountPositionId;

        public UcAssemblyEquipment()
        {
            InitializeComponent();

            InitializeControls();
            SetControl();
            SetButtons();
            SetControlVisibility();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
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

        private void SetControlVisibility()
        {
        }

        private void SetControlClear()
        {
            _dtEquipment.Clear();
            _equipmentCode = string.Empty;
            _equipmentMountPositionId = string.Empty;
        }

        #endregion

        #region Event

        private void dgEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    int seq = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SEQ"));
                    string inputYN = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_YN"));
                    string val001 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL001"));
                    string val002 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL002"));
                    string val003 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL003"));
                    string val004 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL004"));
                    string val005 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL005"));
                    string mountMaterialTypeCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MOUNT_MTRL_TYPE_CODE"));
                    string equipmentState = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HYPHEN"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    //e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                    e.Cell.Presenter.Cursor = Cursors.Arrow;
                    e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (seq == 0)
                    {
                        #region 설비 상태(TEST, PILOT, SPECIAL PRODUCTION
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

                                if (ObjectDic.Instance.GetObjectName("SPCL_PRODUCTION") == equipmentState)
                                {
                                    ca.From = (Color) ColorConverter.ConvertFromString("#ffe8ebed");
                                    ca.To = Colors.Green;
                                    ca.Duration = TimeSpan.FromSeconds(1);
                                    ca.RepeatBehavior = RepeatBehavior.Forever;
                                }
                                //2021.08.12 : 시생산 샘플 설정 추가
                                else if (ObjectDic.Instance.GetObjectName("PILOT_SMPL_PROD") == equipmentState)
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
                    else if (seq == 1)
                    {
                        #region 작업지시서, 버전, WO, 제품
                        //if (e.Cell.Column.Name.ToString() == "VAL002" && !string.IsNullOrWhiteSpace(val002))
                        if (e.Cell.Column.Name.ToString() == "VAL001" && !string.IsNullOrWhiteSpace(val001))    //모델에 배경색 넣는것으로 변경
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }

                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL005")
                        {
                            e.Cell.Presenter.Cursor = Cursors.Hand;

                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }
                        #endregion

                        #region Left 정렬

                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }

                        #endregion

                    }
                    else if (seq == 2)
                    {
                        #region 작업조, 작업자   
                        if (e.Cell.Column.Name.ToString() == "VAL002")
                        {
                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                            }
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL005")
                        {
                            //e.Cell.Presenter.Cursor = Cursors.Hand;

                            if (inputYN == "Y")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }

                        // ToolTip 작업자의 작업시작일시 ~ 작업종료일시
                        if (e.Cell.Column.Name.ToString() == "VAL002")
                            ToolTipService.SetToolTip(e.Cell.Presenter, Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_STRT_DTTM")) + " ~ " + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_END_DTTM")));

                        #endregion

                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                        }
                    }
                    else if (seq == 3)
                    {
                        #region 투입 이벤트처리                                               
                        if (e.Cell.Column.Name.ToString() == "VAL003")
                        {
                            CheckBox checkBox = e.Cell.Presenter.Content as CheckBox;

                            if (IsInputUseAuthority)
                                checkBox.IsEnabled = true;
                            else
                                checkBox.IsEnabled = false;
                        }

                        if (e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);

                            TextBox tx = e.Cell.Presenter.Content as TextBox;
                            //tx.IsEnabled = false;

                            if (IsInputUseAuthority)
                            {
                                tx.IsEnabled = true;
                            }
                            else
                            {
                                tx.IsEnabled = false;
                            }

                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL005")
                        {
                            if(IsInputUseAuthority)
                            {
                                e.Cell.Presenter.Cursor = Cursors.Hand;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                                e.Cell.Presenter.IsEnabled = true;
                            }
                            else
                            {
                                e.Cell.Presenter.Cursor = Cursors.Arrow;
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                                e.Cell.Presenter.IsEnabled = false;
                            }

                            //e.Cell.Presenter.Cursor = Cursors.Hand;
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL006")
                        {
                            if (IsInputUseAuthority)
                            {
                                e.Cell.Presenter.Cursor = Cursors.Hand;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.IsEnabled = true;
                            }
                            else
                            {
                                e.Cell.Presenter.Cursor = Cursors.Arrow;
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                                e.Cell.Presenter.IsEnabled = false;
                            }

                            //e.Cell.Presenter.Cursor = Cursors.Hand;
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                        }

                        #endregion

                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        }
                    }
                    else if (seq == 4)
                    {
                        #region 투입자재  
                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                            //if (mountMaterialTypeCode == "PROD" && !string.Equals(ProcessCode, Process.WASHING))
                            if (mountMaterialTypeCode == "PROD" && !string.Equals(ProcessCode, Process.WASHING) && !string.Equals(ProcessCode, Process.ZTZ))
                            {
                                e.Cell.Presenter.Cursor = Cursors.Hand;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            }
                            else
                            {
                                e.Cell.Presenter.Cursor = Cursors.Arrow;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            }
                        }
                        else if (e.Cell.Column.Name.ToString() == "VAL004")
                        {
                            
                            TextBox tx = e.Cell.Presenter.Content as TextBox;
                            //tx.IsEnabled = false;
                            tx.IsReadOnly = true;

                            //if (!string.IsNullOrEmpty(tx.Text))
                            //    tx.IsReadOnly = true;
                            //else
                            //    tx.IsReadOnly = false;
                        }
                        #endregion

                        if (e.Cell.Column.Name.ToString() == "VAL001")
                        {
                            e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        }
                        else if(e.Cell.Column.Name.ToString() == "VAL007")
                        {
                            //EQPT_REMAIN_PSTN
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                        }
                    }
                    // 설비 재작업 모드
                    else if (seq == 8)
                    {
                        if (e.Cell.Column.Name.ToString() == "HYPHEN" && inputYN == "Y")
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                            Dispatcher.BeginInvoke(new System.Threading.ThreadStart(() =>
                            {
                                var ca = new ColorAnimation();

                                ca.From = (Color)ColorConverter.ConvertFromString("#ffe8ebed");
                                ca.To = Colors.Yellow;
                                ca.Duration = TimeSpan.FromSeconds(1);
                                ca.RepeatBehavior = RepeatBehavior.Forever;

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
                    }

                    if (e.Cell.Row.ParentGroup.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                    {
                        e.Cell.Presenter.IsHitTestVisible = true;

                        if (e.Cell.Row.ParentGroup.Presenter == null || seq != 1)
                        {
                            return;
                        }
                        // 설비 On-Line여부 배경색 표시 : 녹색 – On-line / 적색 – Off-line
                        string EqptID = e.Cell.Row.ParentGroup.DataItem.ToString().Split(':')[0].Trim();
                        DataRow[] dr = _dtEquipment.Select("EQPTID = '" + EqptID + "' And SEQ = 1");

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
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null) return;

                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {

                    }
                }
            }));
        }

        private void dgEquipment_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;
            
            for (int row = 0; row < dg.Rows.Count; row++)
            {
                int seq = Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[row].DataItem, "SEQ"));
                string MountMtrlTypeCode = Util.NVC(DataTableConverter.GetValue(dg.Rows[row].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                if (seq == 0)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["HYPHEN"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                }
                else if (seq == 1)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                }
                //else if (Seq == 1 || Seq == 2)
                else if (seq == 2)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                }
                else if (seq == 3)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));

                }
                else if (seq == 8)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["HYPHEN"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                }
                else if (seq == 9)
                {
                    e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["HYPHEN"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                }
                else
                {
                    // seq == 4
                    //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                    //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL002"].Index)));
                    //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    if (string.Equals(ProcessCode, Process.ZTZ))
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL002"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                }
            }
        }

        private void dgEquipment_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null || dg.SelectedIndex == -1) return;

            if (IsInputUseAuthority == false)
            {
                if (dg.CurrentCell.Column.Name == "VAL003")
                {
                    //Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                    e.Cancel = true;    // Editing 불가능
                }
            }

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //if (dg.CurrentCell.Column.Name == "VAL004")
            //{
            //    int seq = Util.NVC_Int(dg.GetCell(dg.CurrentRow.Index, dg.Columns["SEQ"].Index).Value);

            //    if (seq == 3)
            //    {
            //        e.Cancel = false;   // Editing 가능
            //    }
            //    else
            //    {
            //        e.Cancel = true;    // Editing 불가능
            //    }
            //}

            if (!string.Equals(ProcessCode, Process.ZTZ))
            {
                if (dg.CurrentCell.Column.Name == "VAL004")
                {
                    int seq = Util.NVC_Int(dg.GetCell(dg.CurrentRow.Index, dg.Columns["SEQ"].Index).Value);

                    if (seq == 3)
                    {
                        e.Cancel = false;   // Editing 가능
                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }            

            // TODO 투입취소, 투입완료 버튼에 대한 설정 확인 필요
            //UcAssyDataCollectInline.GetMaterialInputList()

            /*
            //IsInputUseAuthority 투입사용 권한이 없는 경우 선택체크박스 미선택 처리, LOT ID 텍스트 박스 포커스 선택못하도록 처리, 투입취소, 투입배출 버튼 선택 메세지 처리

            if (ProcessCode != Process.NOTCHING)
            {
                if (IsInputUseAuthority == false)
                {
                    if (dg.CurrentCell.Column.Name == "VAL003")
                    {
                        //Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }

            if (dg.CurrentCell.Column.Name == "VAL004")
            {
                int Seq = Util.NVC_Int(dg.GetCell(dg.CurrentRow.Index, dg.Columns["SEQ"].Index).Value);

                if (Seq == 3)
                {
                    e.Cancel = false;   // Editing 가능
                }
                else
                {
                    e.Cancel = true;    // Editing 불가능
                }
            }
            */
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

            // 투입취소, 투입배출
            int seq = Util.NVC_Int(dv["SEQ"]);

            if (seq == 3)
            {
                if (cell.Column.Name.ToString() == "VAL005")
                {
                    if (IsInputUseAuthority == false)
                    {
                        Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                        return;
                    }

                    // 투입취소
                    CurrInCancelProcess();
                }
                else if (cell.Column.Name.ToString() == "VAL006")
                {
                    if (IsInputUseAuthority == false)
                    {
                        //투입 사용권한이 없습니다.
                        Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                        return;
                    }

                    // 투입완료
                    CurrInEndProcess();
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

                int seq = Util.NVC_Int(dr["SEQ"]);
                string equipmentCode = Util.NVC(dr["EQPTID"]);
                string shiftId = Util.NVC(dr["SHFT_ID"]);
                string workUser = Util.NVC(dr["WRK_USERID"]);
                string MountMtrlTypeCode = Util.NVC(dr["MOUNT_MTRL_TYPE_CODE"]);
                string val002 = Util.NVC(dr["VAL002"]);
                string val003 = Util.NVC(dr["VAL003"]);
                string val004 = Util.NVC(dr["VAL004"]);
                string val006 = Util.NVC(dr["VAL006"]);
                //추가 2022.06.22
                string val001 = Util.NVC(dr["VAL001"]);

                _equipmentMountPositionId = Util.NVC(dr["EQPT_MOUNT_PSTN_ID"]);

                if (seq == 1)
                {
                    if (cell.Column.Name.ToString() == "VAL005")
                    {
                        // 팝업호출 : WorkOrder
                        PopupWorkOrder(equipmentCode);
                    }
                }
                else if (seq == 2)
                {
                    if (cell.Column.Name.ToString() == "VAL005")
                    {
                        // 팝업호출 : 작업자
                        PopupWorker(equipmentCode, shiftId, workUser);
                    }
                }

                else if (seq == 4)
                {
                    if (cell.Column.Name.ToString() == "VAL001")
                    {
                        // 팝업호출 : 대기 Pancake, 대기 매거진, 대기 바구니

                        if(string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.ASSEMBLY) || string.Equals(ProcessCode, Process.ZZS))
                        {
                            if (MountMtrlTypeCode == "PROD") PopupWaitLot(val001);
                        } 

                        //if (ProcessCode == Process.PACKAGING)
                        //{
                        //    if (MountMtrlTypeCode == "PROD")
                        //        PopupWaitLot();
                        //}
                        //else
                        //{
                        //    if (ProcessCode == Process.LAMINATION)
                        //    {   
                        //        // 라미공정진척인 경우 장착자재유형이 PROD[반제품] 인것 만 팝업 호출하도록 변경
                        //        if (MountMtrlTypeCode != "PROD") return;
                        //    }

                        //    PopupWaitLot();
                        //}
                    }
                }
            }

        }

        private void dgEquipment_LoadingRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;
            
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;

            int rowIndex = e.Row.Index;
            var row = dg.Rows[rowIndex];

            if (drv["SEQ"].GetString() == "0" && drv["INPUT_YN"].GetString() == "N")
            {
                row.Visibility = Visibility.Collapsed;
            }

            //if (drv["SEQ"].GetString() == "0")
            //    row.Visibility = Visibility.Collapsed;

            if (string.Equals(ProcessCode, Process.WASHING))
            {
                //Washing 공정진척에서는 WorkOrder 정보를 보여주지 않음.
                if (drv["SEQ"].GetString() == "1")
                    row.Visibility = Visibility.Collapsed;
            }
        }

        private void txtVAL004_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
            TextBox textBox = sender as TextBox;
            if (textBox?.DataContext == null) return;
            int rowIndex = ((DataGridCellPresenter)((FrameworkElement)sender).Parent).Cell.Row.Index;

            string seq = DataTableConverter.GetValue(dgEquipment.Rows[rowIndex].DataItem, "SEQ").GetString();

            if (IsInputUseAuthority && seq == "3")
            {
                if (textBox.IsReadOnly == true) textBox.IsReadOnly = false;
            }
            if (IsInputUseAuthority == false)
            {
                Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                return;
            }
        }

        private void txtVAL004_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgEquipment.EndEdit(true);

                TextBox tx = sender as TextBox;

                // 자동 투입 처리
                CurrAutoInputLotProcess(tx);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        public void ChangeEquipment(string processCode, string equipmentCode, bool bEquipmentTable = false)
        {
            
            const string bizRuleName = "DA_PRD_SEL_SMALL_ASSY_EQUIPMENT_DRB";

            try
            {
                SetControlClear();

                DataTable inTable = new DataTable("RQSTDT");
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
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = equipmentCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
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

                        dgEquipment.GroupBy(dgEquipment.Columns["EQPTNAME"], DataGridSortDirection.None);
                        dgEquipment.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                        if (bEquipmentTable)
                        {
                            ////////////////////////////////////////////// WO변경, 작업자 변경시 실적 UserControl의 dtEquipment 갱신
                            // 설비 Table 재생성
                            SetUserControlProductionResult();

                            // Lot LIST 재조회
                            SetProductLotList();
                        }
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

        public void SetPermissionPerButton(string groupCode)
        {
            try
            {
                switch (groupCode)
                {
                    case "INPUT_W": // 투입 사용 권한
                        IsInputUseAuthority = true;
                        break;
                    case "WAIT_W": // 대기 사용 권한
                        IsWaitUseAuthority = true;
                        break;
                    case "INPUTHIST_W": // 투입이력 사용 권한      
                        IsInputHistoryAuthority = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void EqptRemainInputCancel()
        {
            try
            {
                DataRow[] drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
                string equipmentMountPositionId = drCheck[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                string equipmentRemainPosition = drCheck[0]["VAL007"].ToString();
                string inputLotId = drCheck[0]["VAL004"].ToString();

                DataSet indataSet = new DataSet();

                DataTable inEqpTable = indataSet.Tables.Add("IN_EQP");
                inEqpTable.Columns.Add("SRCTYPE", typeof(string));
                inEqpTable.Columns.Add("IFMODE", typeof(string));
                inEqpTable.Columns.Add("EQPTID", typeof(string));
                inEqpTable.Columns.Add("USERID", typeof(string));
                inEqpTable.Columns.Add("PROD_LOTID", typeof(string));

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                inInputTable.Columns.Add("EQPT_REMAIN_PSTN", typeof(string));

                DataRow newRow = inEqpTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProductLotId;
                inEqpTable.Rows.Add(newRow);

                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLotId;
                newRow["EQPT_REMAIN_PSTN"] = equipmentRemainPosition;
                inInputTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_EQPT_REMAIN_LOT_WN", "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275");
                    // 설비 다시 조회
                    ChangeEquipment(ProcessCode, EquipmentCode);

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotWinding(string inputLot, string positionId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = ProductLotId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;
                inInputTable.Rows.Add(newRow);


                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotAssembly(string inputLot, string positionId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_AS();

                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _equipmentCode;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_LOTID"] = ProductLotId;
                row["EQPT_LOTID"] = string.Empty;
                inDataTable.Rows.Add(row);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                row = inInputTable.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = positionId;
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["PRODID"] = string.Empty;
                row["WINDING_RUNCARD_ID"] = inputLot;
                inInputTable.Rows.Add(row);

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_EQP", indataSet);
                
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");

                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputAutoLotWashing(string inputLot, string positionId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_INPUT_LOT_WS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_LOT_WS();
                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProductLotId;
                newRow["EQPT_LOTID"] = string.Empty;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = positionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = inputLot;

                inInputTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, indataSet);
                
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]
        protected virtual void SetUserControlProductionResult()
        {
            if (UcParentControl == null) return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetUserControlProductionResult");
                ParameterInfo[] parameters = methodInfo.GetParameters();
                methodInfo.Invoke(UcParentControl, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void SetProductLotList()
        {
            if (UcParentControl == null) return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SelectProductLotList");

                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parameterArrys = new object[parameters.Length];

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 자동투입
        /// </summary>
        private void CurrAutoInputLotProcess(TextBox tx)
        {
            // 권한 없으면 Skip.
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY)) return;
            if (!ValidationCurrAutoInputLot(tx.Text)) return;

            DataTable dt = DataTableConverter.Convert(dgEquipment.ItemsSource);
            DataRow[] dr = dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");

            string InputLotId = tx.Text.Trim();
            string equipmentMountPositionId = dr[0]["EQPT_MOUNT_PSTN_ID"].ToString();
            string equipmentMountPositionName = dr[0]["VAL001"].ToString();

            object[] parameters = new object[2];
            parameters[0] = equipmentMountPositionName;
            parameters[1] = InputLotId;

            Util.MessageConfirm("SFU1291", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (string.Equals(ProcessCode, Process.WINDING))
                    {
                        InputAutoLotWinding(InputLotId.Trim(), equipmentMountPositionId);
                    }
                    else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                    {
                        InputAutoLotAssembly(InputLotId.Trim(), equipmentMountPositionId);
                    }
                    else
                    {   //Process.WASHING
                        InputAutoLotWashing(InputLotId.Trim(), equipmentMountPositionId);
                    }
                    tx.Text = string.Empty;
                }
            }, parameters);
        }


        /// <summary>
        /// 투입취소
        /// </summary>
        private void CurrInCancelProcess()
        {
            if (!ValidationCurrInCancel()) return;

            if(string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                //투입취소 하시겠습니까?
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataRow[] drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
                        if (drCheck.Length > 0 && drCheck[0]["VAL007"].GetString() == "Y")
                        {
                            EqptRemainInputCancel();
                        }
                        else
                        {
                            DataTable inDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();

                            string equipmentMountPositionId;
                            string equipmentRemainPosition;
                            string inputLotId;
                            string inputQty;

                            foreach (DataRow row in drCheck)
                            {
                                if(!string.IsNullOrEmpty(row["VAL004"].GetString()))
                                {
                                    equipmentMountPositionId = row["EQPT_MOUNT_PSTN_ID"].GetString();
                                    equipmentRemainPosition = row["VAL007"].GetString();
                                    inputLotId = row["VAL004"].GetString();
                                    inputQty = row["INPUT_QTY"].GetString();

                                    DataRow newRow = inDataTable.NewRow();
                                    newRow["WIPNOTE"] = "";
                                    newRow["INPUT_LOTID"] = inputLotId;
                                    newRow["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                                    newRow["ACTQTY"] = string.IsNullOrEmpty(inputQty) ? 0 : Convert.ToDecimal(inputQty);
                                    inDataTable.Rows.Add(newRow);
                                }
                            }
                            MaterialInputCancel(inDataTable);
                        }
                    }
                });
            }

            else
            {
                //투입취소 하시겠습니까?
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();
                        DataRow[] drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");

                        string equipmentMountPositionId;
                        string equipmentRemainPosition;
                        string inputLotId;
                        string inputQty;

                        foreach (DataRow row in drCheck)
                        {
                            if (!string.IsNullOrEmpty(row["VAL004"].GetString()))
                            {
                                equipmentMountPositionId = row["EQPT_MOUNT_PSTN_ID"].GetString();
                                equipmentRemainPosition = row["VAL007"].GetString();
                                inputLotId = row["VAL004"].GetString();
                                inputQty = row["INPUT_QTY"].GetString();

                                DataRow newRow = inDataTable.NewRow();
                                newRow["WIPNOTE"] = "";
                                newRow["INPUT_LOTID"] = inputLotId;
                                newRow["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                                newRow["ACTQTY"] = string.IsNullOrEmpty(inputQty) ? 0 : Convert.ToDecimal(inputQty);
                                inDataTable.Rows.Add(newRow);
                            }
                        }
                        MaterialInputCancel(inDataTable);
                    }
                });
            }
        }

        private void MaterialInputCancel(DataTable dt)
        {
            try
            {
                string bizRuleName;

                if (string.Equals(ProcessCode, Process.WINDING))
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT_WN";
                else if (string.Equals(ProcessCode, Process.WINDING))
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT";
                else
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT";


                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_CANCEL_LM();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProductLotId;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in dt.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275");
                    // 설비 다시 조회
                    ChangeEquipment(ProcessCode, EquipmentCode);

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 투입완료
        /// </summary>
        private void CurrInEndProcess()
        {
            if (!ValidationCurrInEnd()) return;

            Util.MessageConfirm("SFU1972", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable inDataTable;

                    if (string.Equals(ProcessCode, Process.WINDING))
                    {
                        inDataTable = _bizDataSet.GetUC_BR_PRD_REG_CURR_INPUT();
                    }
                    else if(string.Equals(ProcessCode, Process.ASSEMBLY))
                    {
                        inDataTable = _bizDataSet.GetBR_PRD_REG_EQPT_END_INPUT_LOT_AS();
                    }
                    else
                    {
                        //Process.WASHING
                        inDataTable = _bizDataSet.GetBR_PRD_REG_END_INPUT_LOT_WS();
                    }
                    

                    DataRow[] drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");

                    string equipmentMountPositionId;
                    string equipmentRemainPosition;
                    string inputLotId;
                    string materialId;
                    string inputQty;

                    foreach (DataRow row in drCheck)
                    {
                        if (!string.IsNullOrEmpty(row["VAL004"].GetString()))
                        {
                            equipmentMountPositionId = row["EQPT_MOUNT_PSTN_ID"].GetString();
                            equipmentRemainPosition = row["VAL007"].GetString();
                            inputLotId = row["VAL004"].GetString();
                            materialId = row["VAL006"].GetString();
                            inputQty = row["INPUT_QTY"].GetString();

                            if (string.Equals(ProcessCode, Process.WINDING))
                            {
                                DataRow newRow = inDataTable.NewRow();
                                newRow["INPUT_LOTID"] = inputLotId;
                                newRow["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                                newRow["MTRLID"] = materialId; //자재id TODO : 확인 필요함. //Util.NVC(DataTableConverter.GetValue(dgMaterialInput.Rows[i].DataItem, "MTRLID"));
                                newRow["ACTQTY"] = string.IsNullOrEmpty(inputQty) ? 0 : Convert.ToDecimal(inputQty);
                                inDataTable.Rows.Add(newRow);
                            }
                            else if(string.Equals(ProcessCode, Process.ASSEMBLY))
                            {
                                DataRow dr = inDataTable.NewRow();
                                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                dr["EQPTID"] = _equipmentCode;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                                dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                dr["INPUT_LOTID"] = inputLotId;
                                dr["PROD_LOTID"] = ProductLotId;
                                dr["EQPT_LOTID"] = string.Empty;
                                inDataTable.Rows.Add(dr);
                            }
                            else
                            {
                                DataRow dr = inDataTable.NewRow();
                                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                                dr["EQPTID"] = EquipmentCode;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["EQPT_MOUNT_PSTN_ID"] = equipmentMountPositionId;
                                dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                                dr["INPUT_LOTID"] = inputLotId;
                                dr["PROD_LOTID"] = ProductLotId;
                                dr["EQPT_LOTID"] = string.Empty;
                                inDataTable.Rows.Add(dr);
                            }
                        }
                    }

                    if (string.Equals(ProcessCode, Process.WINDING))
                    {
                        MaterialInputCompleteWinding(inDataTable);
                    }
                    else if(string.Equals(ProcessCode, Process.ASSEMBLY))
                    {
                        MaterialInputCompleteAssembly(inDataTable);
                    }
                    else
                    {
                        MaterialInputCompleteWashing(inDataTable);
                    }
                }
            });
        }

        private void MaterialInputCompleteWinding(DataTable dt)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_END_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_INPUT_COMPLETE_LM();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProductLotId;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in dt.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");
                    // 설비 다시 조회
                    ChangeEquipment(ProcessCode, EquipmentCode);
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MaterialInputCompleteAssembly(DataTable dt)
        {
            const string bizRuleName = "BR_PRD_REG_EQPT_END_INPUT_LOT_AS";
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ShowLoadingIndicator();

            try
            {
                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    // 설비 다시 조회
                    ChangeEquipment(ProcessCode, EquipmentCode);
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MaterialInputCompleteWashing(DataTable dt)
        {
            const string bizRuleName = "BR_PRD_REG_END_INPUT_LOT_WS";
            
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            ShowLoadingIndicator();
            try
            {
                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    // 설비 다시 조회
                    ChangeEquipment(ProcessCode, EquipmentCode);
                    Util.MessageInfo("SFU1275");
                }, ds, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
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

        private bool ValidationCurrAutoInputLot(string InputLotID, bool IspopUp = false)
        {
            DataRow[] drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
            DataRow[] drDuplicate = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4");

            if (string.IsNullOrWhiteSpace(ProductLotId))
            {
                // 선택한 작업대상 LOT이 없어 투입할 수 없습니다."
                Util.MessageValidation("SFU1664");
                return false;
            }
            if (string.IsNullOrEmpty(InputLotID))
            {
                Util.MessageValidation("SFU1379");
                return false;
            }

            if (drCheck.Length == 0)
            {
                // 투입 위치를 선택하세요.
                Util.MessageValidation("SFU1957");
                return false;
            }

            foreach (DataRow row in drDuplicate)
            {
                if (row["MOUNT_MTRL_TYPE_CODE"].ToString() == "PROD")
                {
                    if (row["VAL004"].ToString() == InputLotID)
                    {
                        // %1 에 이미 투입되었습니다.
                        Util.MessageValidation("SFU3278", row["VAL001"].ToString());
                        return false;
                    }
                }
            }

            if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                if(drCheck[0]["VAL007"].GetString() == "Y")
                {
                    Util.MessageValidation("SFU3805");
                    return false;
                }
            }
            
            return true;
        }

        private bool ValidationCurrInCancel()
        {
            DataRow[] dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr.Length > 1)
            {
                // 하나의 투입 위치만 선택하세요.
                Util.MessageValidation("SUF4961");
                return false;
            }

            foreach (DataRow row in dr)
            {
                if (string.IsNullOrWhiteSpace(row["VAL004"].ToString()))
                {
                    // 투입 LOT이 없습니다.
                    Util.MessageValidation("SFU1945");
                    return false;
                }
            }
            return true;
        }

        private bool ValidationCurrInEnd()
        {
            DataRow[] dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            foreach (DataRow row in dr)
            {
                if (string.IsNullOrEmpty(row["VAL004"].GetString()))
                {
                    // 투입 LOT이 없습니다.
                    Util.MessageValidation("SFU1945");
                    return false;
                }
            }

            if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                foreach (DataRow row in dr)
                {
                    //EQPT_REMAIN_PSTN
                    if (row["VAL007"].GetString() == "Y")
                    {
                        Util.MessageValidation("SFU3805");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidationWaitLot()
        {
            /*
            // 선택된 LOT 이 존재하지 않더라도 대기 LOT 조회 팝업을 띄우며, 투입 버튼 클릭시 생산 LOT 선택 여부에 따라 오류 메시지를 띄우는 방식으로 수정 함.
            if (!string.IsNullOrEmpty(ProductLotId))
            {
                if (_equipmentCode != SelectEquipmentCode)
                {
                    Util.MessageValidation("SFU1137");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(ProductLotId))
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }
            */

            return true;
        }
        #endregion

        #region 팝업
        /// <summary>
        /// WorkOrder 팝업
        /// </summary>
        private void PopupWorkOrder(string equipmentCode)
        {
            CMM_WORKORDER_LINE_DRB popupWorkOrder = new CMM_WORKORDER_LINE_DRB();
            popupWorkOrder.FrameOperation = this.FrameOperation;

            if (popupWorkOrder != null)
            {
                object[] parameters = new object[3];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = ProcessCode;
                parameters[2] = equipmentCode;
                C1WindowExtension.SetParameters(popupWorkOrder, parameters);

                popupWorkOrder.Closed += new EventHandler(PopupWorkOrder_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorkOrder.ShowModal()));
            }
        }

        private void PopupWorkOrder_Closed(object sender, EventArgs e)
        {
            CMM_WORKORDER_LINE_DRB popup = sender as CMM_WORKORDER_LINE_DRB;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
                ChangeEquipment(ProcessCode, EquipmentCode, true);
            //}
        }

        /// <summary>
        /// 작업자 팝업
        /// </summary>
        private void PopupWorker(string equipmentCode, string shftId, string workUser)
        {
            CMM_SHIFT_USER2_DRB popupWorker = new CMM_SHIFT_USER2_DRB();
            popupWorker.FrameOperation = this.FrameOperation;

            if (popupWorker != null)
            {
                object[] parameters = new object[8];
                parameters[0] = LoginInfo.CFG_SHOP_ID;
                parameters[1] = LoginInfo.CFG_AREA_ID;
                parameters[2] = EquipmentSegmentCode;
                parameters[3] = ProcessCode;
                parameters[4] = shftId;
                parameters[5] = workUser;
                parameters[6] = equipmentCode;
                parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(popupWorker, parameters);

                popupWorker.Closed += new EventHandler(popupWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorker.ShowModal()));
            }
        }

        private void popupWorker_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2_DRB popup = sender as CMM_SHIFT_USER2_DRB;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode, true);
            }
        }

        /// <summary>
        /// 대기Pancake, 매거진, 바구니 팝업
        /// </summary>
        private void PopupWaitLot(string sVal001)
        {
            if (!ValidationWaitLot()) return;

            string workOrderId = string.Empty;

            DataRow[] drWorkOrder = _dtEquipment.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 1");
            
            if(drWorkOrder.Length > 0)
                workOrderId = drWorkOrder[0]["CSTID"].ToString();

            ASSY006_WAIT_LOT popupWaitLot = new ASSY006_WAIT_LOT();
            popupWaitLot.FrameOperation = this.FrameOperation;

            if (popupWaitLot != null)
            {
                object[] parameters = new object[8];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = ProcessCode;
                parameters[2] = _equipmentCode;
                parameters[3] = ProductLotId;
                parameters[4] = _equipmentMountPositionId;
                //parameters[5] = true;
                parameters[5] = IsWaitUseAuthority;
                parameters[6] = workOrderId;
                parameters[7] = sVal001;

                C1WindowExtension.SetParameters(popupWaitLot, parameters);

                popupWaitLot.Closed += new EventHandler(PopupWaitLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupWaitLot.ShowModal()));
            }
        }

        private void PopupWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY006_WAIT_LOT popup = sender as ASSY006_WAIT_LOT;
            if (popup != null && popup.IsUpdated)
            {
                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
        }        

        #endregion

        #endregion


    }
}
