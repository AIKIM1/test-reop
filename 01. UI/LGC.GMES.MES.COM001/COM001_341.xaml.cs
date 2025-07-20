/*************************************************************************************
 Created Date : 2020.11.12
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 월력관리 신규 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.12  조영대 : Initial Created.
  2021.11.17  조영대 : 근무자 ID 컬럼 추가
  2022.09.05  양정훈 : 일 마감 시간 전이면 근무조 수정 가능하도록 변경
  2023.06.22  문혜림 : ESNB조립 전체 라인 선택 기능 추가
  2023.07.07  문혜림 : ComboBox 개체 참조시 null 체크 구문 추가
  2024.02.14  김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
  2024.07.18  문혜림 : E20240717-000970 같은 일자에 2교대, 3교대 혼합하여 등록할 경우 팝업 발생
  2025.06.17  김영국 : 월력관리 화면에서 마우스 오른쪽 버튼 누를 경우 Indata 값이 ""으로 넘어가서 Exception 발생함. "" -> null로 변경.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_341 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;
        private string INPUT_QTY = string.Empty;
        private string END_QTY = string.Empty;
        private object[] paramList = null;
        private string closingTime = string.Empty;

        private Dictionary<string, string> shftDic = new Dictionary<string, string>();
        private Dictionary<string, string> shftGroupCode = new Dictionary<string, string>();
        private Dictionary<string, string> shftGroupDic = new Dictionary<string, string>();

        ContextMenu popMenu = null;

        Dictionary<string, SolidColorBrush> shftSetCodeColor = new Dictionary<string, SolidColorBrush>();
        List<SolidColorBrush> shftColors = null;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Foreground = new SolidColorBrush(Colors.Red)
        };

        public COM001_341()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            // 색상 5개까지 지정.
            shftColors = new List<SolidColorBrush>();
            shftColors.Add(new SolidColorBrush(Color.FromArgb(255, 255, 192, 0)));
            shftColors.Add(new SolidColorBrush(Color.FromArgb(255, 153, 204, 255)));
            shftColors.Add(new SolidColorBrush(Color.FromArgb(255, 254, 122, 122)));
            shftColors.Add(new SolidColorBrush(Color.FromArgb(255, 254, 122, 122)));
            shftColors.Add(new SolidColorBrush(Color.FromArgb(255, 254, 122, 122)));

            selectedArea = LoginInfo.CFG_AREA_ID;
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = LoginInfo.CFG_PROC_ID;
            InitCombo();

            // ESNB 조립인 경우에만 UI 보이도록 설정
            if (LoginInfo.CFG_AREA_ID.Equals("A8") || LoginInfo.CFG_AREA_ID.Equals("AA"))
            {
                // DataGrid 라인 컬럼 추가
                C1.WPF.DataGrid.DataGridTextColumn Line = new C1.WPF.DataGrid.DataGridTextColumn();
                Line.Header = "라인";
                Line.Binding = new System.Windows.Data.Binding("EQSGID");
                Line.HorizontalAlignment = HorizontalAlignment.Left;
                Line.IsReadOnly = true;
                Line.Width = C1.WPF.DataGrid.DataGridLength.Auto;
                dgList.Columns.Insert(1, Line);
                dgList.Columns[1].DisplayIndex = 1;

                // ※ 한 번에 많은 데이터를 저장하면 오류가 발생할 수 있습니다.
                txtWarning.Visibility = Visibility;
            }

        }

        private void InitCombo()
        {
            dtpMonth.SelectedDateTime = System.DateTime.Now;
            closingTime = GetClosingTime();
            SetComboArea(cboArea);
            SetComboEquipmentSegmant(cboEquipmentSegment);   
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);

            int columnLimit = 0;

            // ESNB 조립인 경우 라인 컬럼 추가로 index 조정
            if (LoginInfo.CFG_AREA_ID.Equals("A8") || LoginInfo.CFG_AREA_ID.Equals("AA"))
            {
                columnLimit = 4;
                dgList.FrozenColumnCount = 3;
            }
            else
            {
                columnLimit = 3;
            }

                for (int index = dgList.Columns.Count; index > columnLimit; index--)
            {
                dgList.Columns.RemoveAt(index - 1);
            }

            SetGridDate();

            // 팝업 메뉴 클리어
            ContextMenu menu = this.FindResource("gridMenu") as ContextMenu;
            menu.Items.Clear();
        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboArea.SelectedValue).Equals(string.Empty))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return;
            }

            //if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals(string.Empty))
            if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals(string.Empty) && !cboEquipmentSegment.Text.Equals("-ALL-")) 
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return;
            }

            if (Util.NVC(cboProcess.SelectedValue).Equals(string.Empty))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return;
            }

            if (Util.NVC(cboWorkGroup.SelectedValue).Equals(string.Empty))
            {
                // 선택된 근무자그룹이 없습니다.
                Util.MessageValidation("SFU2049");
                return;
            }
            SearchData();
        }

        private void btnWorkerMapping_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            object[] parameters = new object[1];
            parameters[0] = string.Empty;
            this.FrameOperation.OpenMenu("SFU010013420", true, parameters);

            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgList == null || dgList.GetRowCount().Equals(0))
                {
                    Util.MessageValidation("SFU1566");
                    return;
                }

                //변경하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    SaveData();
                }
            });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegmant_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgList);

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegmant = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    if (cboProcess.SelectedItem != null || cboProcess.SelectedIndex != -1)
                    {
                        SetComboProcess(cboProcess);
                    } 
                }
            }));
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgList);

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    SetComboEquipmentSegmant(cboEquipmentSegment);
                    SetComboProcess(cboProcess); 
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgList);

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    selectedProcess = Convert.ToString(cboProcess.SelectedValue);
                    SetComboWorkerGroup(cboWorkGroup);
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }

        private void cboWorkGroup_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgList);
        }

        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.dgList.Loaded -= dgList_Loaded;

                InitGrid();

                popMenu = this.FindResource("gridMenu") as ContextMenu;
            }));
        }

        private void dgList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pos = e.GetPosition(Window.GetWindow(this));

                C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pos);
                if (cell == null) return;

                if (cell != null && !cell.IsSelected) dgList.CurrentCell = cell;

                if (dgList == null || dgList.CurrentRow == null || cell.Row.Index < 2 ||
                    dgList.CurrentColumn.Name.IndexOf("WRK_DAY_") < 0)
                    return;

                // 다중 선택이고 현재 일자보다 작을 경우 팝업을 보이지 않는다.
                if (dgList.Selection.SelectedCells.Count <= 1)
                {
                    dgList.Selection.Clear();
                    dgList.Selection.Add(cell);
                }

                    foreach (C1.WPF.DataGrid.DataGridCell item in dgList.Selection.SelectedCells)
                    {
                        if (GetDBTime() < (DateTime)item.Column.Tag)
                        {
                            break;
                        }
                        if (item.Column.Tag != null && item.Column.Tag is DateTime && GetDBTime() > (DateTime)item.Column.Tag)
                        {
                            return;
                        }
                    }
                
                // 팝업메뉴
                popMenu.PlacementTarget = sender as Border;
                popMenu.IsOpen = true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void workAction_Click(object sender, RoutedEventArgs e)
        {
            SetActiveWork(e.OriginalSource as MenuItem);
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null || e.Cell.Row.Type.Equals(DataGridRowType.Top))
                    {
                        return;
                    }

                    //날짜 셀 구분
                    if (e.Cell.Column.Name.Contains("WRK_DAY_"))
                    {
                        DataView dv = e.Cell.DataGrid.ItemsSource as DataView;
                        if (Util.NVC(dv.Table.Rows[e.Cell.Row.Index - 2][e.Cell.Column.Name, DataRowVersion.Original])
                            .Equals(Util.NVC(dv.Table.Rows[e.Cell.Row.Index - 2][e.Cell.Column.Name, DataRowVersion.Current])))
                        {
                            if (e.Cell.Value != null && !e.Cell.Value.ToString().Equals(string.Empty))
                            {
                                if (shftSetCodeColor.ContainsKey(e.Cell.Value.ToString()))
                                {
                                    e.Cell.Presenter.Background = shftSetCodeColor[e.Cell.Value.ToString()];
                                }
                                if (e.Cell.Column.Tag is DateTime && DateTime.Today > (DateTime)e.Cell.Column.Tag)
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_MATCH_" + ((DateTime)e.Cell.Column.Tag).Day.ToString("00"))).Equals("Y"))
                                    {
                                        e.Cell.Presenter.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                                        e.Cell.Presenter.BorderThickness = new Thickness(2);
                                    }
                                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WRK_MATCH_" + ((DateTime)e.Cell.Column.Tag).Day.ToString("00"))).Equals("N"))
                                    {
                                        e.Cell.Presenter.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                                        e.Cell.Presenter.BorderThickness = new Thickness(2);
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Silver);
                                        e.Cell.Presenter.BorderThickness = new Thickness(1);
                                    }
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 127, 127, 127));
                            }
                        }
                        else
                        {
                            if (e.Cell.Presenter != null)
                                e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 173, 163, 204));
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
                {
                    if (dgList.CurrentColumn.Tag != null && dgList.CurrentColumn.Tag is DateTime && DateTime.Today > (DateTime)dgList.CurrentColumn.Tag)
                    {
                        e.Handled = true;
                        // 오늘 이전 날짜는 수정 할 수 없습니다.
                        Util.MessageValidation("9137");
                        return;
                    }

                    string clipString = Clipboard.GetText();
                    clipString = clipString.Replace("\t", "");
                    clipString = clipString.Replace("\r", "");
                    clipString = clipString.Replace("\n", "");
                    foreach (KeyValuePair<string, SolidColorBrush> item in shftSetCodeColor)
                    {
                        clipString = clipString.Replace(item.Key, "");
                    }
                    if (!clipString.Equals(string.Empty))
                    {
                        e.Handled = true;

                        StringBuilder useText = new StringBuilder();
                        foreach (KeyValuePair<string, SolidColorBrush> item in shftSetCodeColor)
                        {
                            if (useText.Length == 0)
                            {
                                useText.Append(item.Key);
                            }
                            else
                            {
                                useText.Append("/" + item.Key);
                            }
                        }

                        // 오늘 이전 날짜는 수정 할 수 없습니다.
                        Util.MessageValidation("SFU8272", useText.ToString());
                        return;
                    }

                    dgList.Refresh(false);
                    dgList.Selection.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnModify,
                //btnSearch,
                btnWorkerMapping
            };

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


        private void SetComboArea(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedArea) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedArea;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboArea_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboEquipmentSegmant(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = selectedArea;
                dtRQSTDT.Rows.Add(drnewrow);

                //ESMI 1동(A4) 6 Line 만 조회
                //new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                string bizRuleName = string.Empty;
                if (IsCmiExceptLine())
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FIRST_PRIORITY_LINE_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    // NB 조립인 경우에만 ALL 콤보 추가
                    if (LoginInfo.CFG_AREA_ID.Equals("A8") || LoginInfo.CFG_AREA_ID.Equals("AA"))
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedEquipmentSegmant;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboEquipmentSegmant_SelectedItemChanged(cbo, null);
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboProcess(C1ComboBox cbo)
        {
            try
            {
                //ESMI 1동(A4) 6 Line 만 조회
                //if (cboEquipmentSegment.Text.Equals("-ALL-"))
                if ("-ALL-".Equals(cboEquipmentSegment.Text))
                {
                    CommonCombo combo = new CommonCombo();
                    C1ComboBox[] cboProcessParent = { cboArea };
                    combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent, sCase: "PROCESS_BY_AREAID_PCSG");
                }
                else
                {
                    DataTable dtRQSTDT = new DataTable();
                    dtRQSTDT.TableName = "RQSTDT";
                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                    DataRow drnewrow = dtRQSTDT.NewRow();
                    drnewrow["LANGID"] = LoginInfo.LANGID;
                    drnewrow["EQSGID"] = selectedEquipmentSegmant;
                    dtRQSTDT.Rows.Add(drnewrow);

                    new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        cbo.ItemsSource = DataTableConverter.Convert(result);

                        if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedProcess) select dr).Count() > 0)
                        {
                            cbo.SelectedValue = selectedProcess;
                        }
                        else if (result.Rows.Count > 0)
                        {
                            cbo.SelectedIndex = 0;
                        }
                        else if (result.Rows.Count == 0)
                        {
                            cbo.SelectedItem = null;
                        }
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboWorkerGroup(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_WRK_GR_CBO_BY_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridDate()
        {
            System.DateTime dtNow = new DateTime(dtpMonth.SelectedDateTime.Year, dtpMonth.SelectedDateTime.Month, 1);
            TimeSpan ts = string.IsNullOrEmpty(closingTime) ? new TimeSpan(0, 0, 0) : TimeSpan.Parse(closingTime);

            C1.WPF.DataGrid.DataGridColumnHeaderPresenter todayHeader = new C1.WPF.DataGrid.DataGridColumnHeaderPresenter()
            {
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent)
            };


            for (int i = 1; i <= dtNow.AddMonths(1).AddDays(-1).Day; i++)
            {
                string dayColumnName = string.Empty;

                dayColumnName = "WRK_DAY_" + i.ToString("00");

                List<string> sHeader = new List<string>() { dtNow.Month.ToString() + "/" + i.ToString("00"), dtNow.AddDays(i - 1).ToString("ddd") };

                Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Center, Visibility.Visible);

                if (!string.IsNullOrEmpty(closingTime))
                {
                    dgList.Columns[dayColumnName].Tag = new DateTime(dtNow.Year, dtNow.Month, i, ts.Hours, ts.Minutes, ts.Seconds);
                   
                }
                else
                {
                    dgList.Columns[dayColumnName].Tag = new DateTime(dtNow.Year, dtNow.Month, i);
                }
                
                // Today
                if ((new DateTime(dtNow.Year, dtNow.Month, dtNow.AddDays(i - 1).Day)).Equals(DateTime.Today))
                {
                    Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                    style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromArgb(100, 153, 204, 255)) });
                    style.Setters.Add(new Setter { Property = ForegroundProperty, Value = Brushes.Blue });
                    style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
                    style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
                    style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });
                    style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, ObjectDic.Instance.GetObjectName("오늘")));
                    dgList.Columns[dayColumnName].HeaderStyle = style;
                }

                // Sunday
                else if ((new DateTime(dtNow.Year, dtNow.Month, dtNow.AddDays(i - 1).Day)).DayOfWeek.Equals(DayOfWeek.Sunday))
                {
                    Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                    style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromArgb(255, 255, 204, 204)) });
                    style.Setters.Add(new Setter { Property = ForegroundProperty, Value = Brushes.Red });
                    style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
                    style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
                    style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });
                    dgList.Columns[dayColumnName].HeaderStyle = style;
                }

                // 날짜 다국어 제외 처리
                dgList.Columns[dayColumnName].Header = sHeader;
            }
        }

        private void SearchData()
        {
            try
            { 
                InitGrid();

                if (!SetPopupMenu()) return;
                
                ShowLoadingIndicator();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MONTH", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WRKGRID", typeof(string));

                string month = string.Format("{0:yyyy-MM}", dtpMonth.SelectedDateTime);

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MONTH"] = month;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WRKGRID"] = Util.NVC(cboWorkGroup.SelectedValue);

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new DataTable();

                paramList = Indata.ItemArray;
                /*new ClientProxy().ExecuteService("DA_PRD_SEL_DAILY_PLAN_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgList, result, FrameOperation, false);


                });*/

                if (Util.NVC(cboEquipmentSegment.Text).Equals("-ALL-")) 
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_DAILY_WORK_PLAN_DRB", "INDATA", "RSLTDT", IndataTable);
                }
                else
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DAILY_PLAN_DRB", "RQSTDT", "RSLTDT", IndataTable);
                }
                              
                Util.GridSetData(dgList, dtResult, FrameOperation, true); 
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return;
            }
        }

        private bool SetPopupMenu()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                //Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.Text).Equals("-ALL-") ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT", "RQSTDT", "RSLTDT", IndataTable);
                if (dtResult == null) return false;

                if (dtResult.Rows.Count.Equals(0))
                {
                    Util.MessageValidation("90073");
                    return false;
                }

                shftDic.Clear();
                shftGroupCode.Clear();
                
                if (popMenu != null)
                {
                    popMenu.Items.Clear();

                    shftSetCodeColor.Clear();

                    string shiftCount = dtResult.Rows.Count.ToString();
                    List<string> shftHeaders = new List<string>();
                    foreach (DataRow dr in dtResult.Rows)
                    {
                        MenuItem newMenu = new MenuItem();
                        newMenu.Header = "[" + dr["SHFT_GR_NAME"] + "] " + dr["SHFT_NAME"];
                        newMenu.Tag = dr;
                        newMenu.Click -= workAction_Click;
                        newMenu.Click += workAction_Click;

                        //popMenu.Items.Add(newMenu);
                        if (!shftHeaders.Contains(dr["SHFT_ID"]))
                        {
                            popMenu.Items.Add(newMenu);
                            shftHeaders.Add(dr["SHFT_ID"].ToString());
                            shftDic.Add(Util.NVC(dr["SHFT_NAME"]), Util.NVC(dr["SHFT_ID"]));
                            shftGroupCode.Add(Util.NVC(dr["SHFT_ID"]), Util.NVC(dr["SHFT_GR_CODE"]));
                        }
                        // EQSGID마다 조회되는 조교대 중복되지 않도록 저장
                        //if (shftSetCodeColor.Count < shftColors.Count)
                        if (shftSetCodeColor.Count < shftColors.Count && !shftSetCodeColor.ContainsKey(dr["SHFT_NAME"].ToString()))
                        {
                            shftSetCodeColor.Add(dr["SHFT_NAME"].ToString(), shftColors[shftSetCodeColor.Count]);
                        }

                        //shftDic.Add(Util.NVC(dr["SHFT_NAME"]), Util.NVC(dr["SHFT_ID"]));                  
                    }

                    Separator newSep = new Separator();
                    popMenu.Items.Add(newSep);

                    MenuItem newClear = new MenuItem();
                    newClear.Header = ObjectDic.Instance.GetObjectName("CLEAR");
                    newClear.Click -= workAction_Click;
                    newClear.Click += workAction_Click;
                    popMenu.Items.Add(newClear);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private void SetActiveWork(MenuItem menuItem)
        {
            string shftName = string.Empty;
            string shftCode = string.Empty;

            if (menuItem.Tag is DataRow)
            {
                DataRow drShft = menuItem.Tag as DataRow;

                shftName = drShft["SHFT_NAME"].ToString();
                shftCode = drShft["SHFT_ID"].ToString();
            }

            foreach (C1.WPF.DataGrid.DataGridCell item in dgList.Selection.SelectedCells)
            { 
                DataTableConverter.SetValue(item.Row.DataItem, item.Column.Name, shftName);
                DataTableConverter.SetValue(item.Row.DataItem, item.Column.Name.Replace("WRK_DAY_", "SHFT_ID_"), shftCode);

                if (item.Presenter != null)
                    item.Presenter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 173, 163, 204));
            }   
            
            dgList.Selection.Clear();
        }

        private void SaveData()
        {
            try
            {
                ShowLoadingIndicator();

                // 기존 데이터 삭제 처리
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHFT_ID_OLD", typeof(string));
                inTable.Columns.Add("SHFT_ID_NEW", typeof(string));
                inTable.Columns.Add("WRK_GR_ID", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                // 입력 및 수정 데이터 수집
                DataTable dt = (dgList.ItemsSource as DataView).Table;
                String EQSGID_OLD = "";
                
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName.IndexOf("WRK_DAY_") < 0) continue;

                        string day = col.ColumnName.Replace("WRK_DAY_", "");

                        if (!Util.NVC(dt.Rows[row][col.ColumnName, DataRowVersion.Original])
                            .Equals(Util.NVC(dt.Rows[row][col.ColumnName, DataRowVersion.Current])))
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["LANGID"] = LoginInfo.LANGID;
                            newRow["CALDATE"] = dtpMonth.SelectedDateTime.ToString("yyyy-MM-") + day;
                            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            newRow["AREAID"] = dt.Rows[row]["AREAID"];
                            newRow["EQSGID"] = dt.Rows[row]["EQSGID"];
                            newRow["PROCID"] = dt.Rows[row]["PROCID"];

                            newRow["SHFT_ID_OLD"] = dt.Rows[row]["SHFT_ID_" + day, DataRowVersion.Original];
                            if (shftDic.ContainsKey(Util.NVC(dt.Rows[row]["WRK_DAY_" + day, DataRowVersion.Current])))
                            {
                                newRow["SHFT_ID_NEW"] = shftDic[Util.NVC(dt.Rows[row]["WRK_DAY_" + day, DataRowVersion.Current])];
                            }
                            else
                            {
                                newRow["SHFT_ID_NEW"] = dt.Rows[row]["SHFT_ID_" + day, DataRowVersion.Current];
                            }

                            newRow["WRK_GR_ID"] = dt.Rows[row]["WRK_GR_ID"];
                            newRow["WRK_USERID"] = dt.Rows[row]["WRK_USERID"];
                            newRow["USE_FLAG"] = "Y";
                            newRow["INSUSER"] = LoginInfo.USERID;
                            newRow["UPDUSER"] = LoginInfo.USERID;
                            inTable.Rows.Add(newRow);
                        }

                        // NB의 경우 라인 ALL로 조회하여 선택 가능하므로, 라인이 다른 경우에는 작업조 혼합 리셋
                        if (!EQSGID_OLD.Equals(dt.Rows[row]["EQSGID"].ToString()))
                        {
                            EQSGID_OLD = dt.Rows[row]["EQSGID"].ToString();
                            shftGroupDic.Clear();
                        }

                        // 같은 일자에 작업조 혼합하여 저장할 경우 팝업 발생
                        if (!string.IsNullOrEmpty(dt.Rows[row]["SHFT_ID_" + day, DataRowVersion.Current].ToString()))
                        {
                            if (!shftGroupDic.ContainsKey(day))
                            {
                                shftGroupDic.Add(day, shftGroupCode[dt.Rows[row]["SHFT_ID_" + day, DataRowVersion.Current].ToString()]);
                            }
                            else
                            {
                                if (!shftGroupDic[day].Equals(shftGroupCode[dt.Rows[row]["SHFT_ID_" + day, DataRowVersion.Current].ToString()]))
                                {
                                    Util.MessageValidation("SFU9917");
                                    HiddenLoadingIndicator();
                                    return;
                                }
                            }
                        }
                    }
                }

                // 선택수량 [%1] : 최대 6000건까지 저장할 수 있습니다.
                if (inTable.Rows.Count > 6000)
                {
                    Util.MessageValidation("SFU8557", inTable.Rows.Count);
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService("BR_PRD_UPD_WORK_CALENDAR_DRB", "INDATA", "", inTable, (result, ex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        Util.MessageValidation("SFU1270");
                        SearchData();
                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private DateTime GetDBTime()
        {
            try
            {
                DateTime dt = DateTime.Now;
                DataTable result2 = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", null, "RSLTDT", null);

                if (result2.Rows.Count > 0)
                {
                    dt = DateTime.ParseExact(result2.Rows[0]["DATE_YMDHMS"].ToString(), "yyyyMMddHHmmss", null).AddDays(-1);
                }

                return dt;
            }
            catch
            {
                if (string.IsNullOrEmpty(closingTime))
                {
                    return DateTime.Now;
                }
                else
                {
                    return DateTime.Now.AddDays(-1);
                }
            }
        }

        /// <summary>
        /// 동별 실적 마감 시간 조회
        /// </summary>
        /// <returns></returns>
        private string GetClosingTime()
        {
            string time = string.Empty;

            DataTable dt = new DataTable()
            {
                Columns =
                {
                    new DataColumn("AREAID")
                },
                Rows =
                {
                    new object[] { LoginInfo.CFG_AREA_ID }
                }

            };

            DataTable result = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "RQSTDT", "RSLTDT", dt);

            if (result.Rows.Count > 0 && !string.IsNullOrEmpty(result.Rows[0]["S02"].ToString()))
            {
                time = TimeSpan.ParseExact(result.Rows[0]["S02"].ToString(), "hhmmss", null).ToString();
            }

            return time;
        }

        //ESMI 1동(A4) 6 Line 만 조회
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion
    }
}
