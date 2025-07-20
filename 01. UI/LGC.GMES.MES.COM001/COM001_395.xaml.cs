/*************************************************************************************
 Created Date : 2023.12.11
      Creator : 백광영
   Decription : 공 Pallet Loaction 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.11  백광영 : COM001_381 Copy
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_395 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        
        private string _sTabName = string.Empty;

        Util _Util = new Util();
        
        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
        private DataRowView _currentLocationRow = null;
        private string _currentLocationColumnName = string.Empty;

        private bool _sLocationLoadingYN = false;
        private string _whtypecode = "EEP";

        public COM001_395()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter prem = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox mchkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //C1ComboBox[] cboAreaChild = { cboBldg };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            cboLocation.ApplyTemplate();

            //SetcboAreaBldg(cboBldg);
            SetcboSection(cboSection);
            SetcboLocation(cboLocation);
        }

        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay, string statusType)
        {
            DataRow dr = dt.NewRow();
            switch (statusType)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "EMPTY":
                    dr[sValue] = string.Empty;
                    dr[sDisplay] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void SetcboSection(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_TYPE_CODE"] = _whtypecode;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_PRDT_WH_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "WH_NAME";
                cbo.SelectedValuePath = "WH_ID";

                cbo.ItemsSource = AddStatus(dtResult, "WH_ID", "WH_NAME", "ALL").Copy().AsDataView();

                cbo.SelectedIndex = 1;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetcboLocation(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                String _whId = Util.GetCondition(cboSection, MessageDic.Instance.GetMessage("SFU2961"));
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ELEC_RACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                cbo.CheckAll();

                //cbo.SelectionChanged += new System.EventHandler(this.cboLocation_SelectionChanged);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion
        
        #region Event

        private void cboSection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetcboLocation(cboLocation);
        }

        #endregion

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnLocationSetting);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetPalletListByLocation();  // btnSearch_Click
            GetPalletListByModel();  // btnSearch_Click
        }
        #endregion


        #region [BizCall]


        #region [### 왼쪽 그리드 Location List 조회 ###]
        public void GetPalletListByLocation()
        {
            try
            {
                if (cboSection.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                string _rackId = null;
                for (int i = 0; i < cboLocation.SelectedItems.Count; i++)
                {
                    if (i != cboLocation.SelectedItems.Count - 1)
                    {
                        _rackId += cboLocation.SelectedItems[i] + ",";
                    }
                    else
                    {
                        _rackId += cboLocation.SelectedItems[i];
                    }
                }
                dr["RACK_ID"] = Util.NVC(_rackId).Equals(string.Empty) ? null : _rackId;

                INDATA.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_EMPTY_PLLT_LOCATION_STAT", "RQSTDT", "RSLTDT", INDATA);

                if (result != null && result.Rows.Count > 0)
                {

                    // Pallet 합계 수량 계산
                    DataTable seldt = result.Select().CopyToDataTable();
                    DataTable distinctDt = seldt.DefaultView.ToTable(true, "RACK_ID", "CST_QTY");

                    var querySum = distinctDt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        Sumcstqty = g.Sum(x => x.Field<Int32>("CST_QTY")),
                        Count = g.Count()
                    }).FirstOrDefault();


                    Util.GridSetData(dgPalletListByLocation, result, FrameOperation, true);

                    DataGridAggregate.SetAggregateFunctions(dgPalletListByLocation.Columns["CST_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = Util.NVC(querySum.Sumcstqty) } });
                    DataGridAggregate.SetAggregateFunctions(dgPalletListByLocation.Columns["WH_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });

                }
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

        #region [### 왼쪽 그리드 Model List 조회 ###]
        public void GetPalletListByModel()
        {
            try
            {
                if (cboSection.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                string _rackId = null;
                for (int i = 0; i < cboLocation.SelectedItems.Count; i++)
                {
                    if (i != cboLocation.SelectedItems.Count - 1)
                    {
                        _rackId += cboLocation.SelectedItems[i] + ",";
                    }
                    else
                    {
                        _rackId += cboLocation.SelectedItems[i];
                    }
                }
                dr["RACK_ID"] = Util.NVC(_rackId).Equals(string.Empty) ? null : _rackId;

                INDATA.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_EMPTY_PLLT_TYPE_STAT", "RQSTDT", "RSLTDT", INDATA);

                if (result != null && result.Rows.Count > 0)
                {

                    // Pallet 합계 수량 계산
                    DataTable seldt = result.Select().CopyToDataTable();
                    DataTable distinctDt = seldt.DefaultView.ToTable(true, "CSTPROD", "CST_QTY");

                    var querySum = distinctDt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        Sumcstqty = g.Sum(x => x.Field<Int32>("CST_QTY")),
                        Count = g.Count()
                    }).FirstOrDefault();


                    Util.GridSetData(dgPalletListByModel, result, FrameOperation, true);

                    DataGridAggregate.SetAggregateFunctions(dgPalletListByModel.Columns["CST_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = Util.NVC(querySum.Sumcstqty) } });
                    DataGridAggregate.SetAggregateFunctions(dgPalletListByModel.Columns["CSTPROD"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });

                }
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

        #endregion

        #region [ Loading Func]
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

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgPalletListByLocation);
            Util.gridClear(dgPalletListByLocationDetail);

            Util.gridClear(dgPalletListByModel);
            Util.gridClear(dgPalletListByModelDetail);
        }
        #endregion

        #endregion

        #region [ 상세 Func ]

        // Location에 대한 Detail 조회로직 구성
        private void ShowLocationDetail(DataRowView drvRow, string _shpRsn)
        {
            ShowLoadingIndicator();

            Util.gridClear(dgPalletListByLocationDetail);

            try
            {
                string _whid = drvRow["WH_ID"].ToString();
                string _rackid = string.Empty; 
                string _cstprod = string.Empty;

                switch (_shpRsn)
                {
                    case "CST_QTY":
                        _rackid = drvRow["RACK_ID"].ToString();
                        break;
                    case "CSTPNAME":
                    case "QTY":
                        _rackid = drvRow["RACK_ID"].ToString();
                        _cstprod = drvRow["CSTPROD"].ToString();
                        break;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("CSTPROD", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = Util.NVC(_whid).Equals(string.Empty) ? null : _whid;
                dr["RACK_ID"] = Util.NVC(_rackid).Equals(string.Empty) ? null : _rackid;
                dr["CSTPROD"] = Util.NVC(_cstprod).Equals(string.Empty) ? null : _cstprod;

                INDATA.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_EMPTY_PLLT_LOCATION_STAT_LIST", "RQSTDT", "RSLTDT", INDATA);
                Util.GridSetData(dgPalletListByLocationDetail, result, FrameOperation, false);
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

        // Model에 대한 Detail 조회로직 구성
        private void ShowModelDetail(DataRowView drvRow, string _shpRsn)
        {
            Util.gridClear(dgPalletListByModelDetail);

            ShowLoadingIndicator();

            try
            {
                string _whid = drvRow["WH_ID"].ToString();
                string _cstprod = drvRow["CSTPROD"].ToString();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("CSTPROD", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = Util.NVC(_whid).Equals(string.Empty) ? null : _whid;
                dr["CSTPROD"] = Util.NVC(_cstprod).Equals(string.Empty) ? null : _cstprod;

                INDATA.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_EMPTY_PLLT_LOCATION_STAT_LIST", "RQSTDT", "RSLTDT", INDATA);
                Util.GridSetData(dgPalletListByModelDetail, result, FrameOperation, false);
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

        #region [Event 이벤트]

        private void dgPalletListByLocation_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (
                       colName == "ROW_NUM"
                    || colName == "WH_NAME"
                    || colName == "RACK_NAME"
                    || colName == "MIX_ENABLE"
                    || colName == "CAPA"
                    )
                {
                    return;
                }

                if (dg.CurrentRow != null)
                {
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;

                    if (dg.CurrentCell.Row.Type.ToString().Equals("Bottom"))
                    {
                        DataTable dt = ((DataView)dg.ItemsSource).ToTable().Clone();
                        DataView view = dt.AsDataView();

                        drvRow = view.AddNew();
                        drvRow["WH_ID"] = Util.GetCondition(cboSection.SelectedValue);
                        drvRow["RACK_ID"] = string.Empty;
                        drvRow["RACK_NAME"] = string.Empty;
                        drvRow.EndEdit();
                    }
                    _currentLocationColumnName = colName;
                    _currentLocationRow = drvRow;

                    ShowLocationDetail(drvRow, colName); 

                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void dgPalletListByModel_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (colName.Equals("CSTPROD"))
                {
                    return;
                }

                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));

                    if (dg.CurrentCell.Row.Type.ToString().Equals("Bottom"))
                    {
                        DataTable dt = ((DataView)dg.ItemsSource).ToTable().Clone();
                        DataView view = dt.AsDataView();

                        drvRow = view.AddNew();
                        drvRow["WH_ID"] = Util.GetCondition(cboSection.SelectedValue);
                        drvRow.EndEdit();
                    }

                    _currentLocationColumnName = colName;
                    _currentLocationRow = drvRow;

                    ShowModelDetail(drvRow, colName);
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
            finally
            {
            }
        }


        private string GetShippingReason(String _colName)
        {
            string _shpRsn = string.Empty;
            switch (_colName)
            {
                case "OK_QTY":
                    _shpRsn = "OK";
                    break;
                case "NO_INSP_QTY":
                    _shpRsn = "INSP_WAIT";
                    break;
                case "HOLD_QTY":
                    _shpRsn = "HOLD";
                    break;
                case "LONG_TERM_QTY":
                    _shpRsn = "VLD_DATE";
                    break;
                case "PRJT_NAME":
                    _shpRsn = "PRJT_NAME";
                    break;
                case "CST_QTY":
                    _shpRsn = "CST_QTY";
                    break;
            }

            return _shpRsn;
        }

        #endregion

        private void dgPalletListByLocation_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string _col = e.Cell.Column.Name.ToString();
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (_col == "CST_QTY" || _col == "QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Regular;
                    }

                    if (e.Cell.Row.Type.ToString().Equals("Bottom"))
                    {
                        if (!string.Equals(_col, "WH_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                }));

                if (_isscrollToHorizontalOffset)
                {
                    dg.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgPalletListByModel_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string _col = e.Cell.Column.Name.ToString();
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (_col == "CST_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Regular;
                    }

                    if (e.Cell.Row.Type.ToString().Equals("Bottom"))
                    {
                        if (!string.Equals(_col, "CSTPROD"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnLocationSetting_Click(object sender, RoutedEventArgs e)
        {
            COM001_395_LOCATION_SETTING _popupLoad = new COM001_395_LOCATION_SETTING();
            _popupLoad.FrameOperation = FrameOperation;

            string sSECTION = Convert.ToString(cboSection.SelectedValue);
            if (string.IsNullOrEmpty(sSECTION))
            {
                Util.MessageValidation("SFU2961");
                return;
            }

            if (_popupLoad != null)
            {
                _sLocationLoadingYN = false;
                if (GetGridRowCount(dgPalletListByLocation) > 0)
                {
                    _sLocationLoadingYN = true;
                }

                object[] Parameters = new object[4];
                Parameters[0] = Util.NVC(cboSection.SelectedValue);

                C1WindowExtension.SetParameters(_popupLoad, Parameters);

                _popupLoad.Closed += new EventHandler(_popupLocationSettingLoad_Closed);
                _popupLoad.ShowModal();
                _popupLoad.CenterOnScreen();
            }
        }

        private int GetGridRowCount(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            return dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count;
        }

        
        private void _popupLocationSettingLoad_Closed(object sender, EventArgs e)
        {
            COM001_395_LOCATION_SETTING runStartWindow = sender as COM001_395_LOCATION_SETTING;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                if (_sLocationLoadingYN)
                {
                    ClearValue();
                    GetPalletListByLocation();  // Location Setting POPUP Close
                    GetPalletListByModel();  // Location Setting POPUP Close
                }
            }
        }

        private void dgPalletListByLocation_UnloadedCellPresenter_1(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sTabName = ((System.Windows.FrameworkElement)tbcList.SelectedItem).Name;
        }
        
        private void btnReleaseLocation_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            switch (_sTabName)
            {
                case "Location":
                    dt = DataTableConverter.Convert(dgPalletListByLocationDetail.ItemsSource);
                    break;

                case "Model":
                    dt = DataTableConverter.Convert(dgPalletListByModelDetail.ItemsSource);
                    break;

                default:
                    return;
            }

            DataTable dtBox = dt.Clone();

            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(dr["CHK"])))
                    if (Convert.ToBoolean(dr["CHK"])) dtBox.ImportRow(dr);
            }

            if (dtBox.Rows.Count < 1)
            {
                Util.MessageValidation("SFU8573");  // 선택된 Pallet이 없습니다.
                return;
            }

            Util.MessageConfirm("SFU8574", result =>   // Location 정보를 해제 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    SetReleaseLocationProcess(dtBox);
                }
            });

        }

        public void SetReleaseLocationProcess(DataTable dtBox)
        {
            try
            {
                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("ACTID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                foreach (DataRow row in dtBox.Rows)
                {
                    DataRow dr = INDATA.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["WH_ID"] = Util.NVC(Convert.ToString(row["WH_ID"]));
                    dr["RACK_ID"] = Util.NVC(Convert.ToString(row["RACK_ID"]));
                    dr["CSTID"] = Util.NVC(Convert.ToString(row["CSTID"]));
                    dr["ACTID"] = "RELEASE_LOCATION";
                    dr["USERID"] = LoginInfo.USERID;
                    INDATA.Rows.Add(dr);
                }

                ShowLoadingIndicator();

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_REG_EMPTY_PALLET_ACTID", "RQSTDT", null, INDATA);

                Util.MessageInfo("SFU1275");  // 정상처리 되었습니다.
                btnSearch_Click(null, null);

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

        private void dgPalletListByLocationDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgPalletListByLocationDetail.ItemsSource == null) return;

            DataTable dt = ((DataView)dgPalletListByLocationDetail.ItemsSource).Table;

            foreach (DataRow dr in dt.Rows)
            {
                if (!Convert.ToBoolean(dr["CHK"])) dr["CHK"] = true;
            }
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgPalletListByLocationDetail.ItemsSource == null) return;

            DataTable dt = ((DataView)dgPalletListByLocationDetail.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

        private void dgPalletListByModelDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        prem.Content = mchkAll;
                        e.Column.HeaderPresenter.Content = prem;
                        mchkAll.Checked -= new RoutedEventHandler(mcheckAll_Checked);
                        mchkAll.Unchecked -= new RoutedEventHandler(mcheckAll_Unchecked);
                        mchkAll.Checked += new RoutedEventHandler(mcheckAll_Checked);
                        mchkAll.Unchecked += new RoutedEventHandler(mcheckAll_Unchecked);
                    }
                }
            }));
        }

        private void mcheckAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgPalletListByModelDetail.ItemsSource == null) return;

            DataTable dt = ((DataView)dgPalletListByModelDetail.ItemsSource).Table;

            foreach (DataRow dr in dt.Rows)
            {
                if (!Convert.ToBoolean(dr["CHK"])) dr["CHK"] = true;
            }
            dt.AcceptChanges();

        }
        private void mcheckAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgPalletListByModelDetail.ItemsSource == null) return;

            DataTable dt = ((DataView)dgPalletListByModelDetail.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }
    }
}