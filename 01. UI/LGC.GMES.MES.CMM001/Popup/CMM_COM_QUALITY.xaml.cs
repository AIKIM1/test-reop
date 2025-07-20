/*************************************************************************************
 Created Date : 2017.12.13
      Creator : 
   Decription : 자주검사
--------------------------------------------------------------------------------------
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
    /// CMM_COM_QUALITY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_QUALITY : C1Window
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

        private bool _load = true;

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

        public CMM_COM_QUALITY()
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
            //txtProcess.Text = Util.NVC(tmps[1]);
            txtEquipment.Text = Util.NVC(tmps[3]);
            //txtLotID.Text = Util.NVC(tmps[4]);


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

            if (_procID.Equals(Process.NOTCHING))
            {
                cboLotID.IsEnabled = true;
            }
            else
            {
                cboLotID.IsEnabled = false;
            }

            cboLotID.SelectedValueChanged += cboLotID_SelectedValueChanged;

            SelectProcessName();
            GetEqptWrkInfo();

            // 자주검사 주기 콤보 조회
            SetQualityTime();

            cboQualityTime.SelectedValueChanged += cboQualityTime_SelectedValueChanged;

            // 자주검사 그룹 콤보 조회
            SetQualityGroup();

            cboQualityGroup.SelectedValueChanged += cboQualityGroup_SelectedValueChanged;

            // 자주검사 항목 조회
            GetGQualityInfo();

            AddNoteGrid(dgdNote);
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

                                        rIdx = e.Cell.Column.Index;
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

        private void dgQuality_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            dgQuality.Focus();
        }

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

        private void cboQualityGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
        }

        private void cboLotID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
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
                    int ForCount = 0;

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
                    txtProcess.Text = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    txtProcess.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 검사 시간
        /// </summary>
        private void SetQualityTime()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_TIME_CBO", "INDATA", "OUTDATA", inTable);

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

                        int itvl = Util.NVC_Int(dtResult.Rows[0]["CLCT_ITVL"].ToString());

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
                drSelect[cboQualityTime.DisplayMemberPath.ToString()] = "-ALL-";
                drSelect[cboQualityTime.SelectedValuePath.ToString()] = "";
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

        private void SetQualityGroup()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_GRP_CBO_CMM", "INDATA", "OUTDATA", inTable);

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
                string sBizName = "DA_QCA_SEL_SELF_INSP_CMM";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MAND_INSP_ITEM_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["CLCTITEM_CLSS4"] = Util.NVC(cboQualityGroup.SelectedValue).Equals("") ? null : Util.NVC(cboQualityGroup.SelectedValue);
                newRow["LOTID"] = Util.NVC(cboLotID.SelectedValue);
                newRow["WIPSEQ"] = _wipSeq;

                //newRow["ACTDTTM"] = cboQualityTime.SelectedValue ?? cboQualityTime.SelectedValue.ToString();
                newRow["ACTDTTM"] = cboQualityTime.SelectedValue.ToString().Equals("") ? GetDBDateTime().ToString("yyyy-MM-dd HH:mm:ss") : cboQualityTime.SelectedValue.ToString();
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["MAND_INSP_ITEM_FLAG"] = Util.NVC("");

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
                Boolean nct_insp_Usl_Lsl_check = false;

                double dUsl = 0;
                double dLsl = 0;

                double dULValue = 0;
                String Value_Type = string.Empty;

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQuality.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top))
                        continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = Util.NVC(cboLotID.SelectedValue);
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["EQPTID"] = _eqptID;
                    if (!cboQualityTime.SelectedValue.ToString().Equals(""))
                        newRow["CLCTSEQ"] = cboQualityTime.SelectedIndex;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                    newRow["ACTDTTM"] = cboQualityTime.SelectedValue.ToString().Equals("") ? GetDBDateTime().ToString("yyyy-MM-dd HH:mm:ss") : cboQualityTime.SelectedValue.ToString();
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
/*                        
                        Value_Type = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_VALUE_TYPE_CODE")).Equals("") ? "" : Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_VALUE_TYPE_CODE"));

                        if (_procID.Equals("A5000") && Value_Type == "NUM") // num type 항목에 대하여 상하한치 확인
                        {
                            dUsl = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "USL")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "USL")));
                            dLsl = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LSL")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LSL")));
                            dULValue = Util.NVC(drvTmp[colName].ToString()).Equals("") ? 0 : double.Parse(Util.NVC(drvTmp[colName].ToString()));

                            if (dULValue > dUsl || dULValue < dLsl)
                            {
                                nct_insp_Usl_Lsl_check = true;
                                Util.MessageInfo(Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME1")) + " Spec의 상하한치와 맞지 않습니다.");      //Spec의 상하한치와 맞지 않습니다.
                                HiddenLoadingIndicator();
                                return;
                            }
                        }                       
*/
                        if (newRow[colName].ToString().Equals("NaN"))
                        {
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

        #endregion

        #endregion

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

                        if (!Util.IS_NUMBER(n.Value.ToString()))
                            return;

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

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (sender == null) return;

                //if (sender.GetType().Name == "C1NumericBox")
                //{
                //    C1NumericBox n = sender as C1NumericBox;

                //    if (n.Parent == null || n.Parent.GetType() != typeof(StackPanel)) return;

                //    StackPanel p = (n.Parent as StackPanel);

                //    if (p == null || p.Parent == null) return;

                //    if (p.Parent.GetType() == typeof(DataGridCellPresenter))
                //    {
                //        if ((p.Parent as DataGridCellPresenter).Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                //        {
                //            string ColName = (p.Parent as DataGridCellPresenter).Cell.Column.Name.Substring((p.Parent as DataGridCellPresenter).Cell.Column.Name.Length - 2, 2).ToString();
                //            int MaxCount = Util.NVC_Int(DataTableConverter.GetValue((p.Parent as DataGridCellPresenter).Cell.Row.DataItem, "CLCT_COUNT"));
                //            int ColCnt = 0;

                //            if (int.TryParse(ColName, out ColCnt))
                //            {
                //                if (ColCnt > MaxCount)
                //                {
                //                    n.IsEnabled = false;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (sender.GetType().Name == "ComboBox")
                //{
                //    ComboBox n = sender as ComboBox;

                //    if (n.Parent == null || n.Parent.GetType() != typeof(StackPanel)) return;

                //    StackPanel p = (n.Parent as StackPanel);

                //    if (p == null || p.Parent == null) return;

                //    if (p.Parent.GetType() == typeof(DataGridCellPresenter))
                //    {
                //        if ((p.Parent as DataGridCellPresenter).Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                //        {
                //            string ColName = (p.Parent as DataGridCellPresenter).Cell.Column.Name.Substring((p.Parent as DataGridCellPresenter).Cell.Column.Name.Length - 2, 2).ToString();
                //            int MaxCount = Util.NVC_Int(DataTableConverter.GetValue((p.Parent as DataGridCellPresenter).Cell.Row.DataItem, "CLCT_COUNT"));
                //            int ColCnt = 0;

                //            if (int.TryParse(ColName, out ColCnt))
                //            {
                //                if (ColCnt > MaxCount)
                //                {
                //                    n.IsEnabled = false;
                //                }
                //            }
                //        }
                //    }
                //}
                //else
                //    return;
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
        private void AddNoteGrid(C1DataGrid dgd)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TITLE", typeof(string));
            dt.Columns.Add("NOTE", typeof(string));

            DataRow dr = dt.NewRow();

            string title = "비고";
            dr["TITLE"] = ObjectDic.Instance.GetObjectName(title);
            dt.Rows.Add(dr);

            Util.GridSetData(dgd, dt, FrameOperation, false);
        }

    }
}
