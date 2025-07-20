/*************************************************************************************
 Created Date : 2019.02.26
      Creator : 신광희 차장
   Decription : Skid 현황 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.02.26  신광희 차장 : Initial Created.
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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_018_SKID_STATE : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        private DataTable _dtSkidStateInfo;
        private bool _isSearchResult = false;
        private bool _isSetAutoSelectTime = false;
        private bool _isLoaded = false;
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();

        private readonly Util _util = new Util();

        public MCS001_018_SKID_STATE()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            TimerSetting();
            InitializeSkidStateInfo();
            InitializeCombo();
            _isLoaded = true;
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeCombo()
        {
            SetSkidTypeCombo(cboSKIDType);
        }

        private void InitializeSkidStateInfo()
        {
            _dtSkidStateInfo = new DataTable();
            _dtSkidStateInfo.Columns.Add("PCW_TYPE", typeof(string));
            _dtSkidStateInfo.Columns.Add("PJT", typeof(string));
            _dtSkidStateInfo.Columns.Add("C_NORM", typeof(Int32));              //양극 정상 SKID 수량
            _dtSkidStateInfo.Columns.Add("C_NORM_KCELL", typeof(decimal));      //양극 정상 QTY 수량
            _dtSkidStateInfo.Columns.Add("C_HOLD", typeof(Int32));              //양극 정상 SKID 수량
            _dtSkidStateInfo.Columns.Add("C_HOLD_KCELL", typeof(decimal));      //양극 HOLD QTY 수량
            _dtSkidStateInfo.Columns.Add("C_QMS_HOLD", typeof(Int32));          //양극 QMS NG SKID 수량
            _dtSkidStateInfo.Columns.Add("C_QMS_HOLD_KCELL", typeof(decimal));  //양극 QMS NG QTY 수량
            _dtSkidStateInfo.Columns.Add("A_NORM", typeof(Int32));              //음극 정상 SKID 수량
            _dtSkidStateInfo.Columns.Add("A_NORM_KCELL", typeof(decimal));      //음극 정상 QTY 수량
            _dtSkidStateInfo.Columns.Add("A_HOLD", typeof(Int32));              //음극 정상 SKID 수량
            _dtSkidStateInfo.Columns.Add("A_HOLD_KCELL", typeof(decimal));      //음극 HOLD QTY 수량
            _dtSkidStateInfo.Columns.Add("A_QMS_HOLD", typeof(Int32));          //음극 QMS NG SKID 수량
            _dtSkidStateInfo.Columns.Add("A_QMS_HOLD_KCELL", typeof(decimal));  //음극 QMS NG QTY 수량
            _dtSkidStateInfo.Columns.Add("TARGET", typeof(Int32));              //전극, 조립 TARGET 수량
            _dtSkidStateInfo.Columns.Add("ACTUAL", typeof(Int32));              //전극, 조립 ACTUAL 수량
            _dtSkidStateInfo.Columns.Add("INUSE", typeof(Int32));               //전극, 조립 사용중 공SKID 수
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgList);
                _dtSkidStateInfo.Clear();

                ShowLoadingIndicator();
                DoEvents();

                int cathodeNormal = 0;
                decimal cathodeNormalKcell = 0;
                int cathodeHold = 0;
                decimal cathodeHoldKcell = 0;
                int cathodeQMS = 0;
                decimal cathodeQMSKcell = 0;
                int anodeNormal = 0;
                decimal anodeNormalKcell = 0;
                int anodeHold = 0;
                decimal anodeHoldKcell = 0;
                int anodeQMS = 0;
                decimal anodeQMSKcell = 0;

                int actualElectrode = 0;    // 전극 ACTUAL 수량
                int actualAssembly = 0;     // 조립 ACTUAL 수량
                int inUseElectrode = 0;     // 전극 사용중 공SKID
                int inUseAssembly = 0;      // 조립 사용중 공SKID
                int actualSum = 0;          // ACTUAL 수량 합
                int inUseSum = 0;           // 사용중 공SKID 수량 합
                int targetSum = 0;          // TARGET 수량 합

                const string bizRuleName = "BR_MCS_SEL_PCW_SKID_INFO";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("LANG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["CSTPROD"] = cboSKIDType.SelectedValue;
                dr["LANG"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA,OUTSUM", ds);

                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    if (CommonVerify.HasTableRow(dsResult.Tables["OUTSUM"]))
                    {
                        cathodeNormal = (int)dsResult.Tables["OUTSUM"].Rows[0]["C_NORM_SUBSUM"];
                        cathodeNormalKcell = (decimal)dsResult.Tables["OUTSUM"].Rows[0]["C_NORM_KCELL_SUM"];
                        cathodeHold = (int)dsResult.Tables["OUTSUM"].Rows[0]["C_HOLD_SUBSUM"];
                        cathodeHoldKcell = (decimal)dsResult.Tables["OUTSUM"].Rows[0]["C_HOLD_KCELL_SUM"];
                        cathodeQMS = (int)dsResult.Tables["OUTSUM"].Rows[0]["C_QMS_HOLD_SUBSUM"];
                        cathodeQMSKcell = (decimal)dsResult.Tables["OUTSUM"].Rows[0]["C_QMS_HOLD_KCELL_SUM"];

                        anodeNormal = (int)dsResult.Tables["OUTSUM"].Rows[0]["A_NORM_SUBSUM"];
                        anodeNormalKcell = (decimal)dsResult.Tables["OUTSUM"].Rows[0]["A_NORM_KCELL_SUM"];
                        anodeHold = (int)dsResult.Tables["OUTSUM"].Rows[0]["A_HOLD_SUBSUM"];
                        anodeHoldKcell = (decimal)dsResult.Tables["OUTSUM"].Rows[0]["A_HOLD_KCELL_SUM"];
                        anodeQMS = (int)dsResult.Tables["OUTSUM"].Rows[0]["A_QMS_HOLD_SUBSUM"];
                        anodeQMSKcell = (decimal)dsResult.Tables["OUTSUM"].Rows[0]["A_QMS_HOLD_KCELL_SUM"];

                        actualElectrode = (int)dsResult.Tables["OUTSUM"].Rows[0]["ELTR_ACTUAL"];
                        actualAssembly = (int)dsResult.Tables["OUTSUM"].Rows[0]["ASSY_ACTUAL"];
                        inUseElectrode = (int)dsResult.Tables["OUTSUM"].Rows[0]["ELTR_INUSE"];
                        inUseAssembly = (int)dsResult.Tables["OUTSUM"].Rows[0]["ASSY_INUSE"];

                        actualSum = (int)dsResult.Tables["OUTSUM"].Rows[0]["ACTUAL_SUM"];
                        inUseSum = (int)dsResult.Tables["OUTSUM"].Rows[0]["INUSE_SUM"];
                        targetSum = (int)dsResult.Tables["OUTSUM"].Rows[0]["TARGET_SUM"];
                    }

                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        DataRow newRow = _dtSkidStateInfo.NewRow();
                        newRow["PCW_TYPE"] = dsResult.Tables["OUTDATA"].Rows[i]["PCW_TYPE"];
                        newRow["PJT"] = dsResult.Tables["OUTDATA"].Rows[i]["PJT"];
                        newRow["C_NORM"] = dsResult.Tables["OUTDATA"].Rows[i]["C_NORM"];
                        newRow["C_NORM_KCELL"] = dsResult.Tables["OUTDATA"].Rows[i]["C_NORM_KCELL"];
                        newRow["C_HOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["C_HOLD"];
                        newRow["C_HOLD_KCELL"] = dsResult.Tables["OUTDATA"].Rows[i]["C_HOLD_KCELL"];
                        newRow["C_QMS_HOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["C_QMS_HOLD"];
                        newRow["C_QMS_HOLD_KCELL"] = dsResult.Tables["OUTDATA"].Rows[i]["C_QMS_HOLD_KCELL"];

                        newRow["A_NORM"] = dsResult.Tables["OUTDATA"].Rows[i]["A_NORM"];
                        newRow["A_NORM_KCELL"] = dsResult.Tables["OUTDATA"].Rows[i]["A_NORM_KCELL"];
                        newRow["A_HOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["A_HOLD"];
                        newRow["A_HOLD_KCELL"] = dsResult.Tables["OUTDATA"].Rows[i]["A_HOLD_KCELL"];
                        newRow["A_QMS_HOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["A_QMS_HOLD"];
                        newRow["A_QMS_HOLD_KCELL"] = dsResult.Tables["OUTDATA"].Rows[i]["A_QMS_HOLD_KCELL"];

                        if (dsResult.Tables["OUTDATA"].Rows[i]["PCW_TYPE_CODE"].GetString() == "E")
                        {
                            newRow["ACTUAL"] = actualElectrode;
                            newRow["INUSE"] = inUseElectrode;
                        }
                        else
                        {
                            newRow["ACTUAL"] = actualAssembly;
                            newRow["INUSE"] = inUseAssembly;
                        }
                        newRow["TARGET"] = targetSum;
                        _dtSkidStateInfo.Rows.Add(newRow);
                    }

                    if (CommonVerify.HasTableRow(dsResult.Tables["OUTSUM"]))
                    {
                        DataRow totalRow = _dtSkidStateInfo.NewRow();
                        totalRow["PCW_TYPE"] = "Total";
                        totalRow["PJT"] = "Total";
                        totalRow["C_NORM"] = cathodeNormal;
                        totalRow["C_NORM_KCELL"] = cathodeNormalKcell;
                        totalRow["C_HOLD"] = cathodeHold;
                        totalRow["C_HOLD_KCELL"] = cathodeHoldKcell;
                        totalRow["C_QMS_HOLD"] = cathodeQMS;
                        totalRow["C_QMS_HOLD_KCELL"] = cathodeQMSKcell;
                        totalRow["A_NORM"] = anodeNormal;
                        totalRow["A_NORM_KCELL"] = anodeNormalKcell;
                        totalRow["A_HOLD"] = anodeHold;
                        totalRow["A_HOLD_KCELL"] = anodeHoldKcell;
                        totalRow["A_QMS_HOLD"] = anodeQMS;
                        totalRow["A_QMS_HOLD_KCELL"] = anodeQMSKcell;

                        totalRow["ACTUAL"] = actualSum;
                        totalRow["INUSE"] = inUseSum;
                        totalRow["TARGET"] = targetSum;
                        _dtSkidStateInfo.Rows.Add(totalRow);
                    }

                    Util.GridSetData(dgList, _dtSkidStateInfo, null, true);

                    _isSearchResult = CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]);
                    HiddenLoadingIndicator();
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cboSKIDType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(btnSearch, null);
        }

        private void _monitorTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(btnSearch, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PCW_TYPE")), "Total"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "ACTUAL" || Convert.ToString(e.Cell.Column.Name) == "INUSE" || Convert.ToString(e.Cell.Column.Name) == "TARGET")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (_isSearchResult == false)
                {
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == "Total")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(i, dg.Columns["PCW_TYPE"].Index + 1)));
                        }
                    }
                }
                else
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        TypeId = x.Field<string>("PCW_TYPE")
                    }).Select(g => new
                    {
                        TypeCode = g.Key.TypeId,
                        Count = g.Count()
                    }).ToList();

                    var subquery = dt.AsEnumerable().GroupBy(x => new
                    {
                        target = x.Field<Int32>("TARGET")
                    }).Select(g => new
                    {
                        Target = g.Key.target,
                        Count = g.Count()
                    }).ToList();

                    string previewTypeCode = string.Empty;
                    string previewTarget = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        foreach (var item in query)
                        {
                            int rowIndex = i;

                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString()))
                            {
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == item.TypeCode && previewTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PCW_TYPE"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ACTUAL"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ACTUAL"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["INUSE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["INUSE"].Index)));
                                }
                            }
                        }

                        foreach (var item in subquery)
                        {
                            int rowIndex = i;

                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dg.Rows[i].DataItem, "TARGET").GetString()))
                            {
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "TARGET").GetString() == item.Target.GetString() && previewTarget != DataTableConverter.GetValue(dg.Rows[i].DataItem, "TARGET").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["TARGET"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["TARGET"].Index)));
                                }
                            }
                        }

                        previewTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString();
                        previewTarget = DataTableConverter.GetValue(dg.Rows[i].DataItem, "TARGET").GetString();
                    }

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "PCW_TYPE").GetString() == "Total")
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PCW_TYPE"].Index), dg.GetCell(i, dg.Columns["PCW_TYPE"].Index + 1)));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboAutoSearch?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboAutoSearch.SelectedValue.ToString());
                        _isSetAutoSelectTime = true;
                    }
                    else
                    {
                        _isSetAutoSelectTime = false;
                    }


                    if (second == 0 && !_isSetAutoSelectTime)
                    {
                        Util.MessageValidation("SFU6018");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSetAutoSelectTime)
                    {
                        //Skid 현황 모니터링 자동조회  %1초로 변경 되었습니다.
                        if (cboAutoSearch != null)
                            Util.MessageInfo("SFU6017", cboAutoSearch.SelectedValue.GetString());
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


        private void TimerSetting()
        {

            CommonCombo combo = new CommonCombo();
            // 자동 조회 시간 Combo
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboAutoSearch, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboAutoSearch != null && cboAutoSearch.Items.Count > 0)
                cboAutoSearch.SelectedIndex = 0;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboAutoSearch?.SelectedValue?.ToString()))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _monitorTimer.Tick += _monitorTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        private static void SetSkidTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_SKID_TYPE";
            string[] arrColumn = { "LANG"};
            string[] arrCondition = { LoginInfo.LANGID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
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


    }
}