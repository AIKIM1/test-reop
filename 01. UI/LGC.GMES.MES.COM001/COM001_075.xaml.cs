/*************************************************************************************
 Created Date : 2017.05.08
      Creator : JMK
   Decription : BizRule 오류 로그 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.08  DEVELOPER : Initial Created.
  2017.08.30  cnsrendk : PGM ID 삭제
  2023.10.13  김대현 : Biz Error Log 조회조건 추가및 수정(인도네시아 개발자 개발본 머지)
  2023.11.02  조영대 : BizActor 버젼 비교
  2025.06.19  이민형 : 기간 1달 한정 및 그리드 더블 클릭 시 상세조회
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Data;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_075 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private LGC.GMES.MES.CMM001.Class.BizActorInfo _BizActorInfo = new LGC.GMES.MES.CMM001.Class.BizActorInfo();

        private System.Windows.Media.SolidColorBrush differentVersionColor = 
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 230, 187, 123));

        private System.Windows.Media.SolidColorBrush differentDescriptionColor =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 153, 217, 234));

        private class BizServerInfo
        {
            public string ColumnName { get; set; }
            public string HeaderName { get; set; }
            public string ServerIp { get; set; }
            public string ServerIndex { get; set; }
        }

        private Dictionary<string, string> dicBizDescription = new Dictionary<string, string>();

        private DataTable dtMain = new DataTable();
        private Util _Util = new Util();

        public COM001_075()
        {
            InitializeComponent();

            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            #region Combo Setting

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            C1ComboBox[] cboSrcTypeChild = { cboSrcType };
            _combo.SetCombo(cboSrcType, CommonCombo.ComboStatus.NONE);

            cboIfMode.Items.Add("ON");
            cboIfMode.Items.Add("OFF");
            cboIfMode.SelectedIndex = 0;

            //공정군
            C1ComboBox[] cboProcessSegmentParent = { cboArea };
            C1ComboBox[] cboProcessSegmentChild = { cboEquipmentSegment };
            _combo.SetCombo(cboProcessSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessSegmentChild, cbParent: cboProcessSegmentParent);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboProcessSegment };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "PROCESSSEGMENTLINE");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);
            #endregion

            #region Time Setting
            DateTime dtNowTime = System.DateTime.Now;
            if (dtpFrom != null)
                dtpFrom.SelectedDateTime = dtNowTime;
            if (tmedtFrom != null)
                tmedtFrom.Value = new TimeSpan(dtNowTime.Hour - 1, dtNowTime.Minute, dtNowTime.Second);

            if (dtpTo != null)
                dtpTo.SelectedDateTime = dtNowTime;
            if (tmedtTo != null)
                tmedtTo.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);

            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
            #endregion

            GetBizServerList();
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }

        #region 기간
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime.Year > 1 && dtpTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");

                    dtpFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                    dtpTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                    if (LGCdp.Name.Equals("dtpDateTo"))
                        dtpTo.SelectedDateTime = dtpFrom.SelectedDateTime.AddDays(+30);
                    else
                        dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime.AddDays(-30);

                    dtpFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                    dtpTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                    return;
                }

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region 설비
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //GetPGMID();
        }
        #endregion

        #region 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetSearch();
        }
        #endregion

        #region dgList_LoadedCellPresenter
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                switch (e.Cell.Column.Name)
                {
                    case "EXCT_MSG":
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(400);
                        break;
                    case "EXCT_TYPE":
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                        break;
                    case "INSDTTM":
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(140);
                        break;

                }

            }));            
        }
        #endregion

        private void txtEqptid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetSearch();
            }
        }

        private void txtBizName_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            if (dgBizList.ItemsSource == null)
            {
                DataTable dtBizList = new DataTable();
                dtBizList.Columns.Add("CHK", typeof(bool));
                dtBizList.Columns.Add("BIZNAME", typeof(string));

                dgBizList.ItemsSource = dtBizList.DefaultView;
            }

            text = text.Replace("\r\n", ",");

            string[] bizNameList = text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string bizName in bizNameList)
            {
                if (dgBizList.GetDataTable().AsEnumerable().Where(w => w.Field<string>("BIZNAME").Equals(bizName.Trim())).Count() > 0) continue;

                dgBizList.AddRows(1);
                dgBizList.SetValue(dgBizList.Rows.Count - 1, "BIZNAME", bizName.Trim());
                dgBizList.SetValue(dgBizList.Rows.Count - 1, "CHK", true);
            }
            txtBizName.Text = string.Empty;
        }
        
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!btnSearchCompare.IsEnabled) return;
                
                Button bt = sender as Button;

                C1.WPF.DataGrid.DataGridCellPresenter dgcp = bt.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (dgcp.Row == null) return;

                C1.WPF.DataGrid.C1DataGrid dg = dgcp.DataGrid;

                string bizName = Util.NVC(dgBizList.GetValue(dgcp.Row.Index, "BIZNAME"));
                DataRow drDelete = dgBizList.GetDataTable(false).AsEnumerable().Where(w => w.Field<string>("BIZNAME").Equals(bizName)).FirstOrDefault();
                drDelete.Delete();
                drDelete.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnBizListClear_Click(object sender, RoutedEventArgs e)
        {
            dgBizList.ClearRows();
            dgBizList.ClearValidation();

            txtBizDesc.Text = string.Empty;
        }

        private void btnAddSite_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSiteName.Text) || string.IsNullOrEmpty(txtIP.Text) || string.IsNullOrEmpty(txtInstance.Text)) return;

            if (dgBizList.Columns.Contains(txtSiteName.Text.Replace(" ", "_"))) return;

            dgBizList.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = txtSiteName.Text.Replace(" ", "_"),
                Header = "[*]" + txtSiteName.Text,
                Binding = new Binding()
                {
                    Path = new PropertyPath(txtSiteName.Text.Replace(" ", "_")),
                    Mode = BindingMode.TwoWay
                },
                IsReadOnly = true,
                Width = new C1.WPF.DataGrid.DataGridLength(100, DataGridUnitType.Pixel),
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = txtSiteName.Text.Replace(" ", "_") + "," + txtSiteName.Text + "," + txtIP.Text + "," + txtInstance.Text
            });

            DataTable dtBizList = null;
            if (dgBizList.ItemsSource == null)
            {
                dtBizList = new DataTable();
                dtBizList.Columns.Add("CHK", typeof(bool));
                dtBizList.Columns.Add("BIZNAME", typeof(string));
                dtBizList.Columns.Add(txtSiteName.Text.Replace(" ", "_"), typeof(string));

                dgBizList.ItemsSource = dtBizList.DefaultView;
            }
            else
            {
                dtBizList = dgBizList.GetDataTable(false);
                if (!dtBizList.Columns.Contains(txtSiteName.Text.Replace(" ", "_")))
                {
                    dtBizList.Columns.Add(txtSiteName.Text.Replace(" ", "_"), typeof(string));
                }
            }
        }

        private void btnDeleteSite_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSiteName.Text)) return;

            if (!dgBizList.Columns.Contains(txtSiteName.Text.Replace(" ", "_"))) return;

            dgBizList.ClearValidation();

            dgBizList.Columns.Remove(dgBizList.Columns[txtSiteName.Text.Replace(" ", "_")]);

            if (dgBizList.ItemsSource != null)
            {
                DataTable dtBizList = dgBizList.GetDataTable(false);
                if (dtBizList.Columns.Contains(txtSiteName.Text.Replace(" ", "_")))
                {
                    dtBizList.Columns.Remove(txtSiteName.Text.Replace(" ", "_"));
                }
            }

        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyBizServerList();
        }

        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            PasteBizServerList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveBizServerList();
        }

        private void btnSearchCompare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgBizList.ClearValidation();               

                DataTable dtBizList = dgBizList.GetDataTable(false);
                List<int> listCheckRow = dgBizList.GetCheckedRowIndex("CHK");
                Dictionary<string, string> colList = new Dictionary<string, string>();
                foreach (C1.WPF.DataGrid.DataGridColumn col in dgBizList.Columns)
                {
                    if (col.Tag is string) colList.Add(col.Name, Util.NVC(col.Tag));
                }

                // 백그라운드 실행 (xProgress_WorkProcess)
                object[] argument = new object[3] { dtBizList, listCheckRow, colList };

                xProgress.Percent = 0;
                xProgress.Visibility = Visibility.Visible;

                btnBizListClear.IsEnabled = false;
                btnSearchCompare.IsEnabled = false;
                gdBottomArea.IsEnabled = false;

                xProgress.RunWorker(argument);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                DataTable dtBizList = arguments[0] as DataTable;
                List<int> listCheckRow = arguments[1] as List<int>;
                Dictionary<string, string> colList = arguments[2] as Dictionary<string, string>;

                int totalCount = listCheckRow.Count * dtBizList.Columns.Count - 2;
                int processCount = 0;

                foreach (int rowIndex in listCheckRow)
                {
                    DataRow drBiz = dtBizList.Rows[rowIndex];

                    string bizName = Util.NVC(drBiz["BIZNAME"]);
                    if (!string.IsNullOrEmpty(bizName))
                    {
                        foreach (KeyValuePair<string, string> col in colList)
                        {
                            if (col.Value == null) continue;

                            if (col.Value.Length > 0 && Util.NVC(drBiz[col.Key]).Equals(string.Empty))
                            {
                                processCount++;

                                string[] bizInfo = col.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                string sBizDesc = _BizActorInfo.GetBizDesc(Util.NVC(bizInfo[2]), Util.NVC(bizInfo[3]), bizName);
                                string sBizLastVer = _BizActorInfo.GetBizVersion(sBizDesc);
                                if (string.IsNullOrEmpty(sBizLastVer) || sBizLastVer.Equals("0"))
                                {
                                    if (string.IsNullOrEmpty(sBizDesc)) sBizDesc = "No Description";

                                    List<string> argument = new List<string>();
                                    argument.Add("Validation");
                                    argument.Add(Util.NVC(rowIndex));
                                    argument.Add(col.Key);
                                    argument.Add(sBizDesc);
                                    
                                    e.Worker.ReportProgress(processCount * 100 / totalCount, argument);
                                }
                                else
                                {
                                    if (!dicBizDescription.ContainsKey(rowIndex.ToString() + "," + col.Key))
                                    {
                                        dicBizDescription.Add(rowIndex.ToString() + "," + col.Key, sBizDesc);
                                    }
                                    else
                                    {
                                        dicBizDescription[rowIndex.ToString() + "," + col.Key] = sBizDesc;
                                    }

                                    List<string> argument = new List<string>();
                                    argument.Add("Set Version");
                                    argument.Add(Util.NVC(rowIndex));
                                    argument.Add(col.Key);
                                    argument.Add(sBizLastVer);

                                    e.Worker.ReportProgress(processCount * 100 / totalCount, argument);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


            return null;
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                List<string> argument = e.Arguments as List<string>;
                string workType = argument[0];
                
                switch (workType)
                {
                    case "Validation":
                        {
                            int rowIndex = Util.NVC_Int(argument[1]);
                            string colName = argument[2];
                            string bizDesc = argument[3];

                            dgBizList.SetCellValidation(rowIndex, colName, bizDesc);
                            dgBizList.SetValue(rowIndex, colName, "Error");
                        }
                        break;
                    case "Set Version":
                        {
                            int rowIndex = Util.NVC_Int(argument[1]);
                            string colName = argument[2];
                            string bizVersion = argument[3];

                            dgBizList.SetValue(rowIndex, colName, bizVersion);
                        }
                        break;
                }
                

                xProgress.Percent = percent;
                xProgress.ProgressText = "Working...";
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            xProgress.Visibility = Visibility.Collapsed;

            btnBizListClear.IsEnabled = true;
            btnSearchCompare.IsEnabled = true;
            gdBottomArea.IsEnabled = true;

            dgBizList.Refresh();
        }

        private void dgBizList_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgBizList.CurrentRow == null || dgBizList.CurrentColumn == null) return;

            string key = Util.NVC(dgBizList.CurrentRow.Index) + "," + dgBizList.CurrentColumn.Name;
            if (dicBizDescription.ContainsKey(key))
            {
                txtBizDesc.Text = dicBizDescription[key];
            
                txtBizDesc.ScrollToEnd();
            }
            else
            {
                txtBizDesc.Text = string.Empty;
            }
        }

        private void dgBizList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = sender as C1DataGrid;
                Point point = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(point);

                if (cell != null)
                {
                    if (cell.Column.Index > 2)
                    {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            for (int row = 0; row < dgBizList.Rows.Count; row++)
                            {
                                dgBizList.SetValue(row, cell.Column.Name, null);
                            }
                        }
                        else
                        {
                            cell.Value = null;
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgBizList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            dgBizList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Column.Index < 3) return;

                e.Cell.Presenter.Background = null;

                string baseVer = Util.NVC(dgBizList.GetValue(e.Cell.Row.Index, dgBizList.Columns[3].Name));
                if (string.IsNullOrEmpty(baseVer)) return;

                string compVer = Util.NVC(dgBizList.GetValue(e.Cell.Row.Index, e.Cell.Column.Name));
                if (string.IsNullOrEmpty(compVer)) return;

                if (!baseVer.Equals(compVer))
                {
                    e.Cell.Presenter.Background = differentVersionColor;
                    return;
                }

                if (!dicBizDescription.ContainsKey(e.Cell.Row.Index.ToString() + "," + dgBizList.Columns[3].Name)) return;
                string baseDesc = dicBizDescription[e.Cell.Row.Index.ToString() + "," + dgBizList.Columns[3].Name];

                if (!dicBizDescription.ContainsKey(e.Cell.Row.Index.ToString() + "," + e.Cell.Column.Name)) return;
                string compDesc = dicBizDescription[e.Cell.Row.Index.ToString() + "," + e.Cell.Column.Name];

                if (!baseDesc.Equals(compDesc)) e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 153, 217, 234)); //differentDescriptionColor;

            }));
        }
        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// PGM ID 콤보 설정
        /// </summary>
        private void GetPGMID()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PROD_SEL_BZRULE_ERR_PGMID", "INDATA", "OUTDATA", dtRqst);

                DataRow drCombo = dtRslt.NewRow();
                drCombo["PGM_NAME"] = " - ALL-";
                drCombo["PGM_ID"] = "";
                dtRslt.Rows.InsertAt(drCombo, 0);

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

        /// <summary>
        /// 조회
        /// </summary>
        private void GetSearch()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DateTime dtFromTime;
                DateTime dtToTime;
                TimeSpan spn;
                if (tmedtFrom.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtFrom.Value);
                    dtFromTime = new DateTime(dtpFrom.SelectedDateTime.Year, dtpFrom.SelectedDateTime.Month, dtpFrom.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtFromTime = new DateTime(dtpFrom.SelectedDateTime.Year, dtpFrom.SelectedDateTime.Month, dtpFrom.SelectedDateTime.Day);
                }

                if (tmedtTo.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtTo.Value);
                    dtToTime = new DateTime(dtpTo.SelectedDateTime.Year, dtpTo.SelectedDateTime.Month, dtpTo.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtToTime = new DateTime(dtpTo.SelectedDateTime.Year, dtpTo.SelectedDateTime.Month, dtpTo.SelectedDateTime.Day);
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("EXCTCODE", typeof(string));
                dtRqst.Columns.Add("BIZRULE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtFromTime;
                dr["TO_DATE"] = dtToTime;


                if (cboIfMode.Text != "SELECT")
                {
                    dr["IFMODE"] = cboIfMode.Text.Trim();
                }

                if (cboSrcType.Text != "SELECT")
                {
                    dr["SRCTYPE"] = cboSrcType.Text.Trim();
                }


                if (txtExtCode.Text != "")
                {
                    dr["EXCTCODE"] = txtExtCode.Text.Trim();
                }


                if (txtBizRule.Text != "")
                {
                    dr["BIZRULE"] = txtBizRule.Text.Trim();
                }

                if (txtEqptid.Text == "")
                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                else
                    dr["EQPTID"] = txtEqptid.Text.Trim();
                
                dtRqst.Rows.Add(dr);

                string bizRuleName = "DA_PROD_SEL_BZRULE_ERR";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
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
        #endregion

        #region [Func]
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        private void GetBizServerList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "SELECT";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_TYPE"] = "USER_BIZACTOR_SERVER_LIST";
                dr["CONF_KEY1"] = "CFG01";
                dr["CONF_KEY2"] = "NONE";
                dr["CONF_KEY3"] = "NONE";
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow drConf in dtResult.Rows)
                    {
                        string confString = Util.NVC(drConf["USER_CONF01"]) + Util.NVC(drConf["USER_CONF02"]) + Util.NVC(drConf["USER_CONF03"]) + Util.NVC(drConf["USER_CONF04"]) + Util.NVC(drConf["USER_CONF05"]) +
                                            Util.NVC(drConf["USER_CONF06"]) + Util.NVC(drConf["USER_CONF07"]) + Util.NVC(drConf["USER_CONF08"]) + Util.NVC(drConf["USER_CONF09"]) + Util.NVC(drConf["USER_CONF10"]);

                        List<string> confColumn = confString.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        foreach (string confInfoString in confColumn)
                        {
                            List<string> confSub = confInfoString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                            if (confSub.Count == 4)
                            {
                                dgBizList.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                                {
                                    Name = confSub[0],
                                    Header = "[*]" + confSub[1],
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath(confSub[0]),
                                        Mode = BindingMode.TwoWay
                                    },
                                    IsReadOnly = true,
                                    Width = new C1.WPF.DataGrid.DataGridLength(100, DataGridUnitType.Pixel),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Tag = confSub[0] + "," + confSub[1] + "," + confSub[2] + "," + confSub[3]
                                });

                                DataTable dtBizList = null;
                                if (dgBizList.ItemsSource == null)
                                {
                                    dtBizList = new DataTable();
                                    dtBizList.Columns.Add("CHK", typeof(bool));
                                    dtBizList.Columns.Add("BIZNAME", typeof(string));
                                    dtBizList.Columns.Add(confSub[0], typeof(string));

                                    dgBizList.ItemsSource = dtBizList.DefaultView;
                                }
                                else
                                {
                                    dtBizList = dgBizList.GetDataTable(false);
                                    if (!dtBizList.Columns.Contains(confSub[0]))
                                    {
                                        dtBizList.Columns.Add(confSub[0], typeof(string));
                                    }
                                }
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

        private void SaveBizServerList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));
                dtRqst.Columns.Add("USER_CONF01", typeof(string));
                dtRqst.Columns.Add("USER_CONF02", typeof(string));
                dtRqst.Columns.Add("USER_CONF03", typeof(string));
                dtRqst.Columns.Add("USER_CONF04", typeof(string));
                dtRqst.Columns.Add("USER_CONF05", typeof(string));
                dtRqst.Columns.Add("USER_CONF06", typeof(string));
                dtRqst.Columns.Add("USER_CONF07", typeof(string));
                dtRqst.Columns.Add("USER_CONF08", typeof(string));
                dtRqst.Columns.Add("USER_CONF09", typeof(string));
                dtRqst.Columns.Add("USER_CONF10", typeof(string));

                StringBuilder saveServerList = new StringBuilder();
                foreach (C1.WPF.DataGrid.DataGridColumn column in dgBizList.Columns)
                {
                    if (column.Tag == null) continue;

                    if (column.Tag is string)
                    {
                        saveServerList.Append(Util.NVC(column.Tag) + "|");
                    }
                }

                DataRow drNew = dtRqst.NewRow();
                drNew["WRK_TYPE"] = "SAVE";
                drNew["USERID"] = LoginInfo.USERID;
                drNew["CONF_TYPE"] = "USER_BIZACTOR_SERVER_LIST";
                drNew["CONF_KEY1"] = "CFG01";
                drNew["CONF_KEY2"] = "NONE";
                drNew["CONF_KEY3"] = "NONE";
                drNew["USER_CONF01"] = GetConfString(saveServerList);
                drNew["USER_CONF02"] = GetConfString(saveServerList);
                drNew["USER_CONF03"] = GetConfString(saveServerList);
                drNew["USER_CONF04"] = GetConfString(saveServerList);
                drNew["USER_CONF05"] = GetConfString(saveServerList);
                drNew["USER_CONF06"] = GetConfString(saveServerList);
                drNew["USER_CONF07"] = GetConfString(saveServerList);
                drNew["USER_CONF08"] = GetConfString(saveServerList);
                drNew["USER_CONF09"] = GetConfString(saveServerList);
                drNew["USER_CONF10"] = GetConfString(saveServerList);
                dtRqst.Rows.Add(drNew);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null)
                {
                    Util.MessageInfo("SFU3532", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CopyBizServerList()
        {
            try
            {
                StringBuilder saveServerList = new StringBuilder();
                saveServerList.AppendLine("Copy of BizActor Server List");
                foreach (C1.WPF.DataGrid.DataGridColumn column in dgBizList.Columns)
                {
                    if (column.Tag == null) continue;

                    if (column.Tag is string)
                    {
                        saveServerList.AppendLine(Util.NVC(column.Tag) + "|");
                    }
                }
                Clipboard.SetText(saveServerList.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PasteBizServerList()
        {
            try
            {
                string confString = Clipboard.GetText();

                if (!confString.Substring(0, 28).Equals("Copy of BizActor Server List")) return;

                List<C1.WPF.DataGrid.DataGridColumn> colList = dgBizList.Columns.ToList();
                foreach (C1.WPF.DataGrid.DataGridColumn column in colList)
                {
                    if (column.Tag == null) continue;

                    if (column.Tag is string)
                    {
                        dgBizList.Columns.Remove(column);
                    }
                }

                confString = confString.Replace("Copy of BizActor Server List", "").Replace("\r", "").Replace("\n", "");

                List<string> confColumn = confString.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                foreach (string confInfoString in confColumn)
                {
                    List<string> confSub = confInfoString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    if (confSub.Count == 4)
                    {
                        dgBizList.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                        {
                            Name = confSub[0],
                            Header = "[*]" + confSub[1],
                            Binding = new Binding()
                            {
                                Path = new PropertyPath(confSub[0]),
                                Mode = BindingMode.TwoWay
                            },
                            IsReadOnly = true,
                            Width = new C1.WPF.DataGrid.DataGridLength(100, DataGridUnitType.Pixel),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Tag = confSub[0] + "," + confSub[1] + "," + confSub[2] + "," + confSub[3]
                        });

                        DataTable dtBizList = null;
                        if (dgBizList.ItemsSource == null)
                        {
                            dtBizList = new DataTable();
                            dtBizList.Columns.Add("CHK", typeof(bool));
                            dtBizList.Columns.Add("BIZNAME", typeof(string));
                            dtBizList.Columns.Add(confSub[0], typeof(string));

                            dgBizList.ItemsSource = dtBizList.DefaultView;
                        }
                        else
                        {
                            dtBizList = dgBizList.GetDataTable(false);
                            if (!dtBizList.Columns.Contains(confSub[0]))
                            {
                                dtBizList.Columns.Add(confSub[0], typeof(string));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU3465");
            }

        }

        private string GetConfString(StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return null;

            string returnString = string.Empty;

            if (sb.Length > 4000)
            {
                returnString = sb.ToString().Substring(0, 4000);
                sb.Remove(0, 4000);
            }
            else
            {
                returnString = sb.ToString();
                sb.Remove(0, sb.Length);
            }

            return returnString;
        }

        private void NoThreadProcess(object argumentList)
        {
            try
            {
                object[] arguments = argumentList as object[];

                DataTable dtBizList = arguments[0] as DataTable;
                List<int> listCheckRow = arguments[1] as List<int>;
                Dictionary<string, string> colList = arguments[2] as Dictionary<string, string>;

                foreach (int rowIndex in listCheckRow)
                {
                    DataRow drBiz = dtBizList.Rows[rowIndex];

                    string bizName = Util.NVC(drBiz["BIZNAME"]);
                    if (!string.IsNullOrEmpty(bizName))
                    {
                        foreach (KeyValuePair<string, string> col in colList)
                        {
                            if (col.Value == null) continue;

                            if (col.Value.Length > 0)
                            {
                                string[] bizInfo = col.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                string sBizDesc = _BizActorInfo.GetBizDesc(Util.NVC(bizInfo[2]), Util.NVC(bizInfo[3]), bizName);
                                string sBizLastVer = _BizActorInfo.GetBizVersion(sBizDesc);
                                if (string.IsNullOrEmpty(sBizLastVer) || sBizLastVer.Equals("0"))
                                {
                                    dgBizList.SetCellValidation(rowIndex, col.Key, sBizDesc);
                                    dgBizList.SetValue(rowIndex, col.Key, "Error");
                                }
                                else
                                {
                                    if (!dicBizDescription.ContainsKey(rowIndex.ToString() + "," + col.Key))
                                    {
                                        dicBizDescription.Add(rowIndex.ToString() + "," + col.Key, sBizDesc);
                                    }
                                    dgBizList.SetValue(rowIndex, col.Key, sBizLastVer);
                                }
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

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("DATASET"))
            {
                COM001_075_DETAIL popupLogDetail = new COM001_075_DETAIL { FrameOperation = FrameOperation };
                popupLogDetail.FrameOperation = FrameOperation;

                if (popupLogDetail != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "HIST_SEQNO"));                  

                    C1WindowExtension.SetParameters(popupLogDetail, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => popupLogDetail.ShowModal()));
                }
            }         
        }
    }
}
