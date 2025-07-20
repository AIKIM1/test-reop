/*************************************************************************************
 Created Date : 2020.10.16
      Creator : 신광희
   Decription : 조립 공정진척(CNB 2동) - 트리구조형태의 UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.08.12  김지은 : 시생산샘플설정/해제 기능 추가
 2021.09.02  조영대 : 폴딩 추가
 2021.09.09  김지은 : W/O(SEQ=1) 조회 시 W/O DEMAND TYPE을 기존 W/O자리로 변경(W/O는 보여주지 않음)
 2022.07.15  윤기업 : 대기lot 조회시 선택된 lot이 없다는 validation message 제거
 2023.02.24  김용군 : ESHM 증설 - AZS-STAKING 대응(Stack Machine밑에 투입위치 표기)
 2023.03.15  김용군 : ESHM 증설 - AZS-STAKING 투입위치명 Left 정렬
 2023.05.31  김용군 : ESHM 증설 - AZS_ECUTTER 투입 Lot BIZ 분기 처리
 2023.07.05  장영철 : Stacking / ASZ Stacing / Packagin 공정 CSTID 표현을 위한 셀Merge 수정
 2024.01.23  남재현 : STK 특별 Tray 설정 추가. Package 의 경우, 특별관리설정 시 SPCL_MNGT_FLAG 컬럼 보도록 설정 (다른 공정 또한 수정 시 분기문 제거, _DRB Biz 수정 필요) 
 2024.07.10  박성진 : E20240705-001881 생산버전 짤리지 않도록 수정
 2024.11.29  이동주 : E20240904-000991 [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건(CatchUp)
 2025.05.20  천진수 : ESHG 증설 조립공정진척 DNC공정추가 
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

namespace LGC.GMES.MES.ASSY005.Controls
{
    /// <summary>
    /// UcAssemblyEquipment.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyEquipment : UserControl
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
        public string EquipmentName { get; set; }
        public string SelectEquipmentCode { get; set; }
        public string ProductLotID { get; set; }
        public string LdrLotIdentBasCode { get; set; }
        public bool IsInputUseAuthority { get; set; }
        public bool IsWaitUseAuthority { get; set; }
        public bool IsInputHistoryAuthority { get; set; }

        public string EquipmentGroupCode { get; set; } // STACKING, FOLDING 구분을 위한 그룹 추가


        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private DataTable _dtEquipment;
        private string _equipmentCode;
        private string _eqptMountPstnID;

        //public DataRow drCheck { get; set; }
        //public DataRow drDuplicate { get; set; }

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

        private void SetControlClear()
        {
            _dtEquipment.Clear();
            _equipmentCode = string.Empty;
            _eqptMountPstnID = string.Empty;
        }

        #endregion

        #region Event

        private void dgEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            // 김용군
            if (ProcessCode == Process.AZS_STACKING)
            {
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
                        string val001 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL001"));
                        string val002 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL002"));
                        string val003 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL003"));
                        string val004 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL004"));
                        string val005 = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VAL005"));
                        string mountMaterialTypeCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MOUNT_MTRL_TYPE_CODE"));
                        string equipmentState = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HYPHEN"));

                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));

                        if (Seq == 0)
                        {
                            #region 설비 상태(TEST, PILOT, SPECIAL PRODUCTION
                            if (e.Cell.Column.Name.ToString() == "HYPHEN" && inputYN == "Y")
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;

                                if (ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION") == equipmentState
                                    || ObjectDic.Instance.GetObjectName("PILOT_SMPL_PROD") == equipmentState)
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
                                        ca.From = (Color)ColorConverter.ConvertFromString("#ffe8ebed");
                                        ca.To = Colors.Green;
                                        ca.Duration = TimeSpan.FromSeconds(1);
                                        ca.RepeatBehavior = RepeatBehavior.Forever;
                                    }
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
                        else if (Seq == 1)
                        {
                            #region 작업지시서, 버전, WO, 제품
                            if (e.Cell.Column.Name.ToString() == "VAL001" && !string.IsNullOrWhiteSpace(val001))    //모델에 배경색 넣는것으로 변경
                            {
                                if (inputYN == "Y")
                                {
                                    e.Cell.Presenter.FontStyle = FontStyles.Italic;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkGray);
                                }

                                e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDAFFF6"));
                            }

                            #endregion

                            #region Left 정렬

                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                            }

                            #endregion

                        }
                        else if (Seq == 11)
                        {
                            #region 작업지시서, 버전, WO, 제품
                            if (e.Cell.Column.Name.ToString() == "VAL005")
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
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8ebed"));
                                }
                            }
                            else if (e.Cell.Column.Name.ToString() == "VAL005")
                            {
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
                        else if (Seq == 3)
                        {
                            if (e.Cell.Column.Name == "HYPHEN")
                            {
                                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HYPHEN").GetString()))
                                {
                                    e.Cell.Presenter.FontSize = 22;
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Cursor = Cursors.Hand;
                                }
                            }

                            //AZS Stacking Mahcine  
                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                e.Cell.Presenter.Cursor = Cursors.Arrow;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                            }
                        }
                        else if (Seq == 4)
                        {
                            #region 투입 이벤트처리
                            if (e.Cell.Column.Name.ToString() == "VAL004")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);

                                TextBox tx = e.Cell.Presenter.Content as TextBox;

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
                                if (IsInputUseAuthority)
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
                            }

                            #endregion

                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                            }
                        }

                        else if (Seq == 5)
                        {
                            #region 투입자재  
                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                if (mountMaterialTypeCode == "PROD" && ProcessCode != Process.NOTCHING)
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
                            else if (e.Cell.Column.Name.ToString() == "VAL003")
                            {
                                CheckBox checkBox = e.Cell.Presenter.Content as CheckBox;
                                checkBox.Visibility = Visibility.Visible;

                            }
                            else if (e.Cell.Column.Name.ToString() == "VAL004")
                            {

                                TextBox tx = e.Cell.Presenter.Content as TextBox;
                                tx.Visibility = Visibility.Visible;
                                tx.IsEnabled = false;

                            }

                            else if (e.Cell.Column.Name.ToString() == "VAL006")
                            {

                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;

                            }

                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                // 김용군 AZS Stacking 투입위치명 Left정렬                                
                                //e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                                e.Cell.Presenter.Width = 175;
                                e.Cell.Presenter.Padding = new Thickness(0, 0, 0, 0);
                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                            }
                            #endregion
                        }

                        if (e.Cell.Row.ParentGroup.Presenter == null)
                        {
                            return;
                        }

                        if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                        {
                            e.Cell.Presenter.IsHitTestVisible = true;

                            if (e.Cell.Row.ParentGroup.Presenter == null || Seq != 1)
                            {
                                return;
                            }
                            // 설비 On-Line여부 배경색 표시 : 녹색 – On-line / 적색 – Off-line
                            string EqptID = e.Cell.Row.ParentGroup.DataItem.ToString().Split(':')[0].Trim();
                            DataRow[] dr = _dtEquipment.Select("EQPTID = '" + EqptID + "' And SEQ = 11");

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
            else
            {
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

                        if (Seq == 0)
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

                                /*
                                ColorAnimation colorAnimation = new ColorAnimation();
                                if (ObjectDic.Instance.GetObjectName("SPCL_PRODUCTION") == equipmentState)
                                {
                                    colorAnimation.From = (Color) ColorConverter.ConvertFromString("#ffe8ebed");
                                    colorAnimation.To = (Color)ColorConverter.ConvertFromString("#ffe8ebed");
                                    colorAnimation.Duration = TimeSpan.FromSeconds(1);
                                    colorAnimation.RepeatBehavior = RepeatBehavior.Forever;
                                }
                                else if (ObjectDic.Instance.GetObjectName("TESTMODE") == equipmentState)
                                {
                                    //solidColor = new SolidColorBrush(Colors.Red);
                                }
                                else
                                {
                                    //solidColor = new SolidColorBrush(Colors.Red);
                                }
                                */

                                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(() =>
                                {
                                    var ca = new ColorAnimation();

                                    if (ObjectDic.Instance.GetObjectName("SPCL_PRODUCTION") == equipmentState)
                                    {
                                        ca.From = (Color)ColorConverter.ConvertFromString("#ffe8ebed");
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
                        else if (Seq == 1)
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
                            //else if (e.Cell.Column.Name.ToString() == "VAL005")
                            //{
                            //    e.Cell.Presenter.Cursor = Cursors.Hand;

                            //    if (inputYN == "Y")
                            //    {
                            //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            //    }
                            //    else
                            //    {
                            //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            //    }
                            //}
                            #endregion

                            #region Left 정렬

                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                            }

                            #endregion

                        }
                        else if (Seq == 11)
                        {
                            #region 작업지시서, 버전, WO, 제품
                            if (e.Cell.Column.Name.ToString() == "VAL005")
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
                        else if (Seq == 3)
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
                                if (IsInputUseAuthority)
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
                        else if (Seq == 4)
                        {
                            #region 투입자재  
                            if (e.Cell.Column.Name.ToString() == "VAL001")
                            {
                                if (mountMaterialTypeCode == "PROD" && ProcessCode != Process.NOTCHING)
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
                                //A8000,A9000 공정 CSTID 추가에 따른 정렬
                                //e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                                e.Cell.Presenter.Width = 255;
                                e.Cell.Presenter.Padding = new Thickness(0, 0, 0, 0);
                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;

                            }
                        }

                        if (e.Cell.Row.ParentGroup.Presenter == null)
                        {
                            return;
                        }

                        if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                        {
                            e.Cell.Presenter.IsHitTestVisible = true;
                            ////dg.ContextMenu.IsOpen = true;
                            //dgEquipmentContextMenu.IsOpen = true;
                            //if (ProcessCode == Process.NOTCHING)
                            //{
                            //    // 투입이력조회 버튼 숨김
                            //    MenuItem item = dg.ContextMenu.Items[0] as MenuItem;
                            //    item.Visibility = Visibility.Collapsed;
                            //}

                            if (e.Cell.Row.ParentGroup.Presenter == null || Seq != 1)
                            {
                                return;
                            }
                            // 설비 On-Line여부 배경색 표시 : 녹색 – On-line / 적색 – Off-line
                            string EqptID = e.Cell.Row.ParentGroup.DataItem.ToString().Split(':')[0].Trim();
                            DataRow[] dr = _dtEquipment.Select("EQPTID = '" + EqptID + "' And SEQ = 11");

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
        }

        private void dgEquipment_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null) return;

                //if (e.Cell.Row.ParentGroup.Presenter != null)
                //{
                //    if (e.Cell.Row.ParentGroup.Type == DataGridRowType.Group)
                //    {
                //        e.Cell.Row.ParentGroup.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                //    }
                //}

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
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            // 김용군
            if (ProcessCode == Process.AZS_STACKING)
            {
                for (int row = 0; row < dg.Rows.Count; row++)
                {
                    int Seq = Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[row].DataItem, "SEQ"));
                    String MountMtrlTypeCode = Util.NVC(DataTableConverter.GetValue(dg.Rows[row].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                    if (Seq == 0)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["HYPHEN"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else if (Seq == 1)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else if (Seq == 11)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else if (Seq == 2)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                    }
                    else if (Seq == 3)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else if (Seq == 4)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL002"].Index)));

                        //2023.07.05  장영철: Stacking / ASZ Stacing / Packagin 공정 CSTID 표현을 위한 셀Merge 수정
                        //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                        if (MountMtrlTypeCode == "PROD")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                        }
                        else
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                        }
                    }
                }
            }
            else
            {
                for (int row = 0; row < dg.Rows.Count; row++)
                {
                    int Seq = Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[row].DataItem, "SEQ"));
                    String MountMtrlTypeCode = Util.NVC(DataTableConverter.GetValue(dg.Rows[row].DataItem, "MOUNT_MTRL_TYPE_CODE"));

                    if (Seq == 0)
                    {
                        //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["HYPHEN"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else if (Seq == 1)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    else if (Seq == 11)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                    }
                    //else if (Seq == 1 || Seq == 2)
                    else if (Seq == 2)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL002"].Index), dg.GetCell(row, dg.Columns["VAL004"].Index)));
                    }
                    else if (Seq == 3)
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                    }
                    else
                    {
                        //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL002"].Index)));
                        //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                        if (ProcessCode == Process.NOTCHING)
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL003"].Index)));
                        }
                        else
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL001"].Index), dg.GetCell(row, dg.Columns["VAL002"].Index)));
                        }

                        if (MountMtrlTypeCode == "PROD" && ProcessCode != Process.STACKING_FOLDING)
                        {
                            //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                            //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                        }
                        else
                        {
                            //2023.07.05  장영철: Stacking / ASZ Stacing / Packagin 공정 CSTID 표현을 위한 셀Merge 수정
                            //e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL004"].Index), dg.GetCell(row, dg.Columns["VAL005"].Index)));
                            e.Merge(new DataGridCellsRange(dg.GetCell(row, dg.Columns["VAL006"].Index), dg.GetCell(row, dg.Columns["VAL007"].Index)));
                        }
                    }
                }
            }

        }

        private void dgEquipment_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null || dg.SelectedIndex == -1) return;

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

                // 김용군
                //if (Seq == 3)
                //{
                //    e.Cancel = false;   // Editing 가능
                //}
                //else
                //{
                //    e.Cancel = true;    // Editing 불가능
                //}
                if (ProcessCode == Process.AZS_STACKING)
                {
                    if (Seq == 4)
                    {
                        e.Cancel = false;   // Editing 가능
                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
                else
                {
                    if (Seq == 3)
                    {
                        e.Cancel = false;   // Editing 가능
                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
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

            // 투입취소, 투입배출
            int seq = Util.NVC_Int(dv["SEQ"]);

            // 김용군
            //if (seq == 3)
            //{
            //    if (cell.Column.Name.ToString() == "VAL005")
            //    {
            //        if (IsInputUseAuthority == false && ProcessCode != Process.NOTCHING)
            //        {
            //            //Util.MessageInfo("투입 사용권한이 없습니다.");
            //            Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
            //            return;
            //        }

            //        // 투입취소
            //        CurrInCancelProcess();
            //    }
            //    else if (cell.Column.Name.ToString() == "VAL006")
            //    {
            //        if (IsInputUseAuthority == false && ProcessCode != Process.NOTCHING)
            //        {
            //            //투입 사용권한이 없습니다.
            //            Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
            //            return;
            //        }

            //        // 투입배출
            //        CurrInEndProcess();
            //    }

            //}
            if (ProcessCode == Process.AZS_STACKING)
            {
                if (seq == 4)
                {
                    if (cell.Column.Name.ToString() == "VAL005")
                    {
                        if (IsInputUseAuthority == false && ProcessCode != Process.NOTCHING)
                        {
                            //Util.MessageInfo("투입 사용권한이 없습니다.");
                            Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                            return;
                        }

                        // 투입취소
                        CurrInCancelProcess();
                    }
                    else if (cell.Column.Name.ToString() == "VAL006")
                    {
                        if (IsInputUseAuthority == false && ProcessCode != Process.NOTCHING)
                        {
                            //투입 사용권한이 없습니다.
                            Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                            return;
                        }

                        // 투입배출
                        CurrInEndProcess();
                    }

                }

            }
            else
            {
                if (seq == 3)
                {
                    if (cell.Column.Name.ToString() == "VAL005")
                    {
                        if (IsInputUseAuthority == false && ProcessCode != Process.NOTCHING)
                        {
                            //Util.MessageInfo("투입 사용권한이 없습니다.");
                            Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                            return;
                        }

                        // 투입취소
                        CurrInCancelProcess();
                    }
                    else if (cell.Column.Name.ToString() == "VAL006")
                    {
                        if (IsInputUseAuthority == false && ProcessCode != Process.NOTCHING)
                        {
                            //투입 사용권한이 없습니다.
                            Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                            return;
                        }

                        // 투입배출
                        CurrInEndProcess();
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

                int seq = Util.NVC_Int(dr["SEQ"]);
                string equipmentCode = Util.NVC(dr["EQPTID"]);
                string shiftId = Util.NVC(dr["SHFT_ID"]);
                string workUser = Util.NVC(dr["WRK_USERID"]);
                string MountMtrlTypeCode = Util.NVC(dr["MOUNT_MTRL_TYPE_CODE"]);
                string val002 = Util.NVC(dr["VAL002"]);
                string val003 = Util.NVC(dr["VAL003"]);
                string val004 = Util.NVC(dr["VAL004"]);
                string val006 = Util.NVC(dr["VAL006"]);

                _eqptMountPstnID = Util.NVC(dr["EQPT_MOUNT_PSTN_ID"]);

                if (seq == 11)
                {
                    if (cell.Column.Name.ToString() == "VAL005")
                    {
                        // 팝업호출 : WorkOrder
                        PopupWorkOrder(equipmentCode);
                    }
                }
                else if (seq == 2)
                {
                    //if (cell.Column.Name.ToString() == "VAL005")
                    //{
                    //    // 팝업호출 : 작업자
                    //    PopupWorker(equipmentCode, shiftId, workUser);
                    //}
                }
                //else if (seq == 3)
                //{
                //    if (cell.Column.Name.ToString() == "VAL005")
                //    {
                //        // 투입취소
                //        CurrInCancelProcess();
                //    }
                //    else if (cell.Column.Name.ToString() == "VAL006")
                //    {
                //        // 투입배출
                //        CurrInEndProcess();
                //    }
                //}

                // 김용군
                //else if (seq == 4)
                else if (seq == 4 && ProcessCode != Process.AZS_STACKING)
                {
                    if (cell.Column.Name.ToString() == "VAL001")
                    {
                        // 팝업호출 : 대기 Pancake, 대기 매거진, 대기 바구니
                        if (ProcessCode == Process.PACKAGING)
                        {
                            if (MountMtrlTypeCode == "PROD")
                                PopupWaitLot();
                        }
                        else
                        {
                            if (ProcessCode == Process.NOTCHING)
                            {
                                // 노칭공정은 팝업발생하지 않음
                                return;
                            }
                            else if (ProcessCode == Process.DNC || ProcessCode == Process.LAMINATION)       // 20250428 ESHG DNC공정신설
                            {
                                // 라미공정진척인 경우 장착자재유형이 PROD[반제품] 인것 만 팝업 호출하도록 변경
                                if (MountMtrlTypeCode != "PROD") return;
                            }
                            //  김용군
                            else if (ProcessCode == Process.AZS_ECUTTER)
                            {
                                // AZS_ECUTTER 공정진척인 경우 장착자재유형이 PROD[반제품] 인것 만 팝업 호출하도록 변경
                                if (MountMtrlTypeCode != "PROD") return;
                            }


                            PopupWaitLot();
                        }
                    }
                }

                // 김용군
                else if (seq == 5 && ProcessCode == Process.AZS_STACKING)
                {
                    if (cell.Column.Name.ToString() == "VAL001")
                    {
                        if (MountMtrlTypeCode == "PROD")
                        {
                            PopupWaitLot();
                        }
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

            // 김용군 AZS-Stacking Machine 투입 위치 숨김처리
            if (ProcessCode == Process.AZS_STACKING)
            {
                if (drv["SEQ"].GetString() == "4" && drv["VAL002"].GetString() != "CHK")
                {
                    row.Visibility = Visibility.Collapsed;
                    DataTableConverter.SetValue(drv, "VAL002", "CHK");
                }

                if (drv["SEQ"].GetString() == "5" && drv["VAL002"].GetString() != "CHK")
                {
                    row.Visibility = Visibility.Collapsed;
                    DataTableConverter.SetValue(drv, "VAL002", "CHK");
                }
            }
        }

        private void txtVAL004_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ProcessCode != Process.NOTCHING)
            {

                TextBox textBox = sender as TextBox;
                if (textBox?.DataContext == null) return;
                int rowIndex = ((DataGridCellPresenter)((FrameworkElement)sender).Parent).Cell.Row.Index;

                string seq = DataTableConverter.GetValue(dgEquipment.Rows[rowIndex].DataItem, "SEQ").GetString();

                // 김용군
                //if (IsInputUseAuthority && seq == "3")
                //{
                //    if (textBox.IsReadOnly == true) textBox.IsReadOnly = false;
                //}
                if (ProcessCode == Process.AZS_STACKING)
                {
                    if (IsInputUseAuthority && seq == "4")
                    {
                        if (textBox.IsReadOnly == true) textBox.IsReadOnly = false;
                    }

                }
                else
                {
                    if (IsInputUseAuthority && seq == "3")
                    {
                        if (textBox.IsReadOnly == true) textBox.IsReadOnly = false;
                    }
                }

                if (IsInputUseAuthority == false)
                {
                    Util.MessageInfo("SFU8324", ObjectDic.Instance.GetObjectName("투입"));
                    return;
                }
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
            // 김용군
            //const string bizRuleName = "DA_PRD_SEL_ASSY_EQUIPMENT_DRB";
            string bizRuleName = string.Empty;

            if (processCode == Process.AZS_STACKING)
            {
                bizRuleName = "DA_PRD_SEL_ASSY_AZS_STK_EQP_DRB";
            }
            // 2024.01.23 남재현: STK 특별 Tray 설정 추가. Package 의 경우, 특별관리설정 시 SPCL_MNGT_FLAG 컬럼 보도록 설정 (다른 공정 또한 수정 시 분기문 제거, _DRB Biz 수정 필요) 
            else if (processCode == Process.PACKAGING)
            {
                bizRuleName = "DA_PRD_SEL_ASSY_EQUIPMENT_DRB_PKG";
            }
            else
            {
                bizRuleName = "DA_PRD_SEL_ASSY_EQUIPMENT_DRB";
            }

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

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

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

        private string GetNowRunProdLot()
        {
            try
            {
                ShowLoadingIndicator();

                string sNowLot = "";
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_NOW_PROD_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_NOW_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sNowLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                }

                return sNowLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void OnCurrAutoInputLot(string sInputLot, string sPstnID)
        {
            try
            {
                string bizRuleName = string.Empty;
                if (ProcessCode == Process.LAMINATION)        
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_LM_L";
                else if (ProcessCode == Process.STACKING_FOLDING)
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_FD_L";
                // 김용군 AZS_STACKING 투입LOT Biz
                else if (ProcessCode == Process.AZS_STACKING)
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_AZS_L";
                // 김용군 AZS_ECUTTER 투입LOT Biz
                else if (ProcessCode == Process.AZS_ECUTTER)
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_AZC";
                else if (ProcessCode == Process.DNC)            // ESHG증설 DNC공정추가 250522 천진수
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_DNC";
                else
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CL_L";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_ID", typeof(string));
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = ProductLotID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_ID"] = sInputLot;
                inInput.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 설비 다시 조회
                        ChangeEquipment(ProcessCode, EquipmentCode);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void OnCurrInCancel()
        {
            try
            {
                string bizRuleName = string.Empty;
                if (ProcessCode == Process.PACKAGING)
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_LOT_CL";
                else
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_IN_LOT";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("WIPNOTE", typeof(string));
                inInput.Columns.Add("INPUT_SEQNO", typeof(string));
                inInput.Columns.Add("CSTID", typeof(string));
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProductLotID;
                inTable.Rows.Add(newRow);

                // 김용군
                DataRow[] dr = null;
                if (ProcessCode == Process.AZS_STACKING)
                {
                    dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
                }
                else
                {
                    dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
                }

                //foreach(DataRow row in dr)
                //{
                //    inInput.Clear();
                //    newRow = inInput.NewRow();
                //    newRow["EQPT_MOUNT_PSTN_ID"] = row["EQPT_MOUNT_PSTN_ID"].ToString();
                //    newRow["INPUT_LOTID"] = row["VAL004"].ToString();
                //    newRow["WIPNOTE"] = "";
                //    newRow["CSTID"] = row["CSTID"].ToString();
                //    inInput.Rows.Add(newRow);

                //    ////////////////////////////////////// IN_INPUT 한건씩 처리?
                //    new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, inDataSet);
                //}

                //// 설비 다시 조회
                //ChangeEquipment(ProcessCode, EquipmentCode);

                foreach (DataRow row in dr)
                {
                    newRow = inInput.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = row["EQPT_MOUNT_PSTN_ID"].ToString();
                    newRow["INPUT_LOTID"] = row["VAL004"].ToString();
                    newRow["WIPNOTE"] = "";
                    newRow["CSTID"] = row["CSTID"].ToString();
                    inInput.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 설비 다시 조회
                        ChangeEquipment(ProcessCode, EquipmentCode);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
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
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                return;

            if (!ValidationCurrAutoInputLot(tx.Text))
            {
                //tx.Text = string.Empty;
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgEquipment.ItemsSource);
            // 김용군
            DataRow[] dr = null;
            if (ProcessCode == Process.AZS_STACKING)
            {
                dr = dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
            }
            else
            {
                dr = dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
            }

            string InputLotID = tx.Text.Trim();
            string sInPos = dr[0]["EQPT_MOUNT_PSTN_ID"].ToString();
            string sInPosName = dr[0]["VAL001"].ToString();
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            if (ProcessCode == Process.PACKAGING && dr[0]["MOUNT_MTRL_TYPE_CODE"].ToString() == "PROD")
            {
                OnCurrAutoInputLot(tx.Text.Trim(), sInPos);
            }
            else
            {
                object[] parameters = new object[2];
                parameters[0] = sInPosName;
                parameters[1] = InputLotID;

                //  {0} 위치에 {1} 을 투입 하시겠습니까?
                Util.MessageConfirm("SFU1291", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        OnCurrAutoInputLot(InputLotID, sInPos);
                    }
                }, parameters);
            }

            tx.Text = string.Empty;
        }


        /// <summary>
        /// 투입취소
        /// </summary>
        private void CurrInCancelProcess()
        {
            if (!ValidationCurrInCancel()) return;

            if (LdrLotIdentBasCode.Equals("RF_ID"))
            {
                PopupInputCancelCST();
            }
            else
            {
                //투입취소 하시겠습니까?
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        OnCurrInCancel();
                    }
                });
            }
        }

        /// <summary>
        /// 투입배출
        /// </summary>
        private void CurrInEndProcess()
        {
            if (!ValidationCurrInEnd()) return;

            PopupInputLotEnd();
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    foreach (UIElement ui in tmp.Children)
                    {
                        if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                        {
                            // 프로그램이 이미 실행 중 입니다. 
                            Util.MessageValidation("SFU3193");
                            return false;
                        }
                    }
                }
            }

            return true;
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
            // 김용군
            DataRow[] drCheck = null;
            DataRow[] drDuplicate = null;

            if (ProcessCode == Process.AZS_STACKING)
            {
                drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
                drDuplicate = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5");
            }
            else
            {
                drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
                drDuplicate = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4");
            }
            //DataRow[] drCheck = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
            //DataRow[] drDuplicate = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4");

            if (string.IsNullOrWhiteSpace(InputLotID))
            {
                // 투입 LOT이 없습니다.
                Util.MessageValidation("SFU1945");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProductLotID))
            {
                // 선택한 작업대상 LOT이 없어 투입할 수 없습니다."
                Util.MessageValidation("SFU1664");
                return false;
            }

            if (!IspopUp)
            {
                // 대기 팝업 호출인 아닌 경우만 체크
                if (drCheck.Length == 0)
                {
                    // 투입 위치를 선택하세요.
                    Util.MessageValidation("SFU1957");
                    return false;
                }

                if (drCheck.Length > 1)
                {
                    // 하나의 투입 위치만 선택하세요.
                    Util.MessageValidation("SUF4961");
                    return false;
                }

                // 현재 설비와 선택된 셍산LOT의 설비가 틀리면 오류
                if (_equipmentCode != SelectEquipmentCode)
                {
                    // 생산LOT 과 투입LOT의 인계설비가 상이합니다.
                    //Util.MessageValidation("90130");
                    //return false;

                    // 선택된 LOT 이 존재하지 않습니다."
                    Util.MessageValidation("SFU1137");
                    return false;
                }
            }

            if (ProcessCode == Process.DNC || ProcessCode == Process.LAMINATION || ProcessCode == Process.STACKING_FOLDING)     // 20250428 ESHG DNC공정신설 
            {
                foreach (DataRow row in drDuplicate)
                {
                    if (row["VAL004"].ToString() == InputLotID)
                    {
                        // %1 에 이미 투입되었습니다.
                        Util.MessageValidation("SFU3278", row["VAL001"].ToString());
                        return false;
                    }
                }
            }
            else
            {
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
            }

            // 패키지 공정인 경우 최근 LOT에만 투입 처리 가능
            // 마지막 PROD LOT에만 투입 가능하도록 처리.
            if (ProcessCode == Process.PACKAGING)
            {
                string sNowProd = GetNowRunProdLot();

                if (!sNowProd.Equals("") && ProductLotID != sNowProd)
                {
                    // 선택한 조립LOT({0})은 마지막 작업중인 LOT이 아닙니다.\n마지막 작업중인 LOT({1})에만 투입할 수 있습니다.", sSelProd, sNowProd;
                    object[] parameters = new object[2];
                    parameters[0] = ProductLotID;
                    parameters[1] = sNowProd;

                    Util.MessageValidation("SFU1666", parameters);
                    return false;
                }
            }

            return true;
        }

        private bool ValidationCurrInCancel()
        {
            // 김용군
            DataRow[] dr = null;
            if (ProcessCode == Process.AZS_STACKING)
            {
                dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
            }
            else
            {
                dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
            }

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

            if (ProcessCode != Process.PACKAGING)
            {
                if (string.IsNullOrWhiteSpace(ProductLotID))
                {
                    // 선택된 실적정보가 없습니다.
                    Util.MessageValidation("SFU1640");
                    return false;
                }
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
            // 김용군
            DataRow[] dr = null;
            if (ProcessCode == Process.AZS_STACKING)
            {
                dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
            }
            else
            {
                dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
            }

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

            if (string.IsNullOrWhiteSpace(dr[0]["VAL004"].ToString()))
            {
                // 투입 LOT이 없습니다.
                Util.MessageValidation("SFU1945");
                return false;
            }

            return true;
        }

        private bool ValidationWaitLot()
        {
            // 선택된 LOT 이 존재하지 않더라도 대기 LOT 조회 팝업을 띄우며, 투입 버튼 클릭시 생산 LOT 선택 여부에 따라 오류 메시지를 띄우는 방식으로 수정 함.
            if (!string.IsNullOrEmpty(ProductLotID))
            {
                if (_equipmentCode != SelectEquipmentCode)
                {
                    Util.MessageValidation("SFU1137");
                    return false;
                }
            }

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
        /// 투입취소(RFID) 팝업
        /// </summary>
        private void PopupInputCancelCST()
        {
            ASSY005_INPUT_CANCEL_CST popupInputLotEnd = new ASSY005_INPUT_CANCEL_CST();
            popupInputLotEnd.FrameOperation = this.FrameOperation;

            if (popupInputLotEnd != null)
            {
                // 김용군
                DataRow[] dr = null;
                if (ProcessCode == Process.AZS_STACKING)
                {
                    dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
                }
                else
                {
                    dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
                }

                object[] parameters = new object[13];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = _equipmentCode;
                parameters[2] = ProcessCode;
                parameters[3] = dr[0]["VAL001"].ToString();                 // EQPT_MOUNT_PSTN_NAME
                parameters[4] = dr[0]["VAL004"].ToString();                 // INPUT_LOTID
                parameters[5] = dr[0]["CSTID"].ToString();                  // CSTID
                parameters[6] = dr[0]["INPUT_QTY"].ToString();              // INPUT_QTY
                parameters[7] = dr[0]["MTRLID"].ToString();                 // MTRLID
                parameters[8] = dr[0]["MOUNT_STAT_CHG_DTTM"].ToString();    // MOUNT_STAT_CHG_DTTM
                parameters[9] = dr[0]["EQPT_MOUNT_PSTN_ID"].ToString();     // EQPT_MOUNT_PSTN_ID
                parameters[10] = "CURR";
                parameters[11] = "";
                parameters[12] = ProductLotID;
                C1WindowExtension.SetParameters(popupInputLotEnd, parameters);

                popupInputLotEnd.Closed += new EventHandler(PopupInputCancelCST_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupInputLotEnd.ShowModal()));
            }
        }

        private void PopupInputCancelCST_Closed(object sender, EventArgs e)
        {
            ASSY005_INPUT_CANCEL_CST popup = sender as ASSY005_INPUT_CANCEL_CST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
        }

        /// <summary>
        /// 투입배출 팝업
        /// </summary>
        private void PopupInputLotEnd()
        {
            ASSY005_COM_INPUT_LOT_END popupInputLotEnd = new ASSY005_COM_INPUT_LOT_END();
            popupInputLotEnd.FrameOperation = this.FrameOperation;

            if (popupInputLotEnd != null)
            {
                // 김용군
                DataRow[] dr = null;
                if (ProcessCode == Process.AZS_STACKING)
                {
                    dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 5 And VAL003 = 1");
                }
                else
                {
                    dr = DataTableConverter.Convert(dgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = 4 And VAL003 = 1");
                }

                object[] parameters = new object[12];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = _equipmentCode;
                parameters[2] = dr[0]["VAL004"].ToString();
                parameters[3] = dr[0]["WIPSEQ"].ToString();
                parameters[4] = dr[0]["INPUT_QTY"].ToString();
                parameters[5] = dr[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                parameters[6] = ProcessCode;
                parameters[7] = dr[0]["INPUT_LOT_TYPE_CODE"].ToString();
                parameters[8] = LdrLotIdentBasCode;
                parameters[9] = dr[0]["CSTID"].ToString();
                parameters[10] = dr[0]["AUTO_STOP_FLAG"].ToString();
                parameters[11] = ProductLotID;
                C1WindowExtension.SetParameters(popupInputLotEnd, parameters);

                popupInputLotEnd.Closed += new EventHandler(PopupInputLotEnd_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupInputLotEnd.ShowModal()));
            }
        }

        private void PopupInputLotEnd_Closed(object sender, EventArgs e)
        {
            ASSY005_COM_INPUT_LOT_END popup = sender as ASSY005_COM_INPUT_LOT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비 다시 조회
                ChangeEquipment(ProcessCode, EquipmentCode);
            }
        }

        /// <summary>
        /// 대기Pancake, 매거진, 바구니 팝업
        /// </summary>
        private void PopupWaitLot()
        {
            //if (!ValidationWaitLot()) return;

            ASSY005_WAIT_LOT popupWaitLot = new ASSY005_WAIT_LOT();
            popupWaitLot.FrameOperation = this.FrameOperation;

            if (popupWaitLot != null)
            {
                object[] parameters = new object[7];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = ProcessCode;
                parameters[2] = _equipmentCode;
                parameters[3] = ProductLotID;
                parameters[4] = _eqptMountPstnID;
                parameters[5] = IsWaitUseAuthority;
                parameters[6] = EquipmentGroupCode;

                C1WindowExtension.SetParameters(popupWaitLot, parameters);

                popupWaitLot.Closed += new EventHandler(PopupWaitLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupWaitLot.ShowModal()));
            }
        }

        private void PopupWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY005_WAIT_LOT popup = sender as ASSY005_WAIT_LOT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (!ValidationCurrAutoInputLot(popup.InputLotID, true))
                {
                    return;
                }

                OnCurrAutoInputLot(popup.InputLotID, _eqptMountPstnID);
            }
        }

        // 김용군 HYPHEN '+', '-' 인 투입위치 클릭시 펼치기 접기
        private void dgEquipment_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ProcessCode == Process.AZS_STACKING)
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgEquipment.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;

                DataRowView drv = dgEquipment.Rows[rowIdx].DataItem as DataRowView;

                if (drv == null || string.IsNullOrEmpty(drv["HYPHEN"].GetString())) return;

                string displayMode = drv["HYPHEN"].GetString();
                int Seq = Util.NVC_Int(drv["SEQ"].GetString());


                if (cell.Column.Name == "HYPHEN" && !string.IsNullOrEmpty(displayMode) && Seq == 3)
                {
                    if (displayMode == "+")
                    {
                        int x = 0;

                        for (int i = rowIdx + 1; i < dg.Rows.Count; i++)
                        {
                            DataRowView drtoggle = dg.Rows[i].DataItem as DataRowView;
                            if (drtoggle == null)
                            {
                                x = i;
                                break;
                            }
                            else
                            {
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "SEQ").GetString() == "3" || i == dg.Rows.Count - 1)
                                {
                                    x = i;
                                    break;
                                }
                            }
                        }

                        for (int i = rowIdx + 1; i <= x; i++)
                        {
                            DataRowView drtoggle = dg.Rows[i].DataItem as DataRowView;

                            if (drtoggle == null)
                            {
                                continue;
                            }

                            int Seqsuv = Util.NVC_Int(drtoggle["SEQ"].GetString());

                            if (Seqsuv == 4 || Seqsuv == 5)
                            {
                                dgEquipment.Rows[i].Visibility = Visibility.Visible;
                                dgEquipment.Rows[i].Refresh();
                            }
                        }
                    }
                    else
                    {
                        int x = 0;

                        for (int i = rowIdx + 1; i < dg.Rows.Count; i++)
                        {
                            DataRowView drtoggle = dg.Rows[i].DataItem as DataRowView;
                            if (drtoggle == null)
                            {
                                x = i;
                                break;
                            }
                            else
                            {
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "SEQ").GetString() == "3" || i == dg.Rows.Count - 1)
                                {
                                    x = i;
                                    break;
                                }
                            }
                        }

                        for (int i = rowIdx + 1; i <= x; i++)
                        {
                            DataRowView drtoggle = dg.Rows[i].DataItem as DataRowView;

                            if (drtoggle == null)
                            {
                                continue;
                            }

                            int Seqsuv = Util.NVC_Int(drtoggle["SEQ"].GetString());

                            if (Seqsuv == 4 || Seqsuv == 5)
                            {
                                dgEquipment.Rows[i].Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                    DataTableConverter.SetValue(drv, "HYPHEN", displayMode.Equals("+") ? "-" : "+");
                    dg.EndEdit();
                    dg.EndEditRow(true);
                }
            }

        }

        #endregion

        #endregion
    }
}
