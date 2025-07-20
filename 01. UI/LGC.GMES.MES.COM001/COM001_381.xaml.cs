/*************************************************************************************
 Created Date : 2023.05.29
      Creator : 
   Decription : 포장 Pallet Loaction 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.29  주재홍 : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.28  주재홍 : 다국어
  2024.08.06  최석준 : 사외반품Cell 포함여부 컬럼 추가 (2025년 적용예정, 수정 시 연락부탁드립니다)
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
    public partial class COM001_381 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _StackingYN = string.Empty;

        private string _sTabName = string.Empty;


        Util _Util = new Util();

        private BizDataSet _Biz = new BizDataSet();
        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;

        private DataRowView _currentLocationRow = null;
        //private int _currentLocationIndex = -1;
        private string _currentLocationColumnName = string.Empty;

        private string _dicNameTotal = string.Empty;
        private string _dicNameLongTerm = string.Empty;

        private int _sindexEPDoubleClick = -1;
        private bool _sLocationLoadingYN = false;

        public COM001_381()
        {
            InitializeComponent();
            InitCombo();
            Initialize();

            _dicNameTotal = ObjectDic.Instance.GetObjectName("합계");
            _dicNameLongTerm = ObjectDic.Instance.GetObjectName("VLD_DATE");

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgPalletListByLocation.Viewport.HorizontalOffset;
        }


        private void dgPalletListByLocation_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
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

        #endregion

        #region Initialize

        private void Initialize()
        {
            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgPalletListByLocationDetail.Columns["OCOP_RTN_CELL_ICL_FLAG"].Visibility = Visibility.Visible;
                dgPalletListByModelDetail.Columns["OCOP_RTN_CELL_ICL_FLAG"].Visibility = Visibility.Visible;
            }

        }
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboBldg };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            cboLocation.ApplyTemplate();

            SetcboAreaBldg(cboBldg);
            SetcboSection(cboSection);
            SetcboLocation(cboLocation);
        }

        /*
        private void cboLocation_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboLocation.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }
        */

        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay , string statusType)
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
        
        private void SetcboAreaBldg(C1ComboBox cbo)
        {
            try
            {
                if (cboArea.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_BLDG_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "WH_PHYS_PSTN_CODE";

                cbo.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetcboSection(C1ComboBox cbo)
        {
            try
            {
                if (cboArea.Items.Count <= 0) return;
                if (cboBldg.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_PHYS_PSTN_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                dr["WH_PHYS_PSTN_CODE"] = Util.GetCondition(cboBldg, MessageDic.Instance.GetMessage("SFU1957"));
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_SECTION_BY_BLDG", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "WH_NAME";
                cbo.SelectedValuePath = "WH_ID";

                cbo.ItemsSource = AddStatus(dtResult, "WH_ID", "WH_NAME", "ALL").Copy().AsDataView();

                //cbo.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));

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
                if (cboArea.Items.Count <= 0) return;
                if (cboBldg.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BLDG", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BLDG"] = Util.GetCondition(cboBldg, MessageDic.Instance.GetMessage("SFU2961"));
                String _whId = Util.GetCondition(cboSection, MessageDic.Instance.GetMessage("SFU2961"));
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_RACK_ID_BY_SECTION", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "RACK_ID";

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

        private void cboBldg_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboBldg.SelectedValue);
            //if (sTemp != "" && sTemp != "SELECT")
            //{
            SetcboSection(cboSection);
            //}

        }


        private void cboSection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboBldg.SelectedValue);
            //if (sTemp != "" && sTemp != "SELECT")
            //{
            SetcboLocation(cboLocation);
            //}

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
            GetPalletListByEmptyPallet();  // btnSearch_Click
        }
        #endregion


        #region [BizCall]

        
        #region [### 왼쪽 그리드 Location List 조회 ###]
        public void GetPalletListByLocation()
        {

                if (cboArea.Items.Count <= 0) return;
                if (cboSection.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("RACK_NAME", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                dr["BLDGCODE"] = Util.GetCondition(cboBldg, bAllNull: true).ToString();
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
                dr["PRJT_NAME"] = Util.NVC(txtProjectName.Text).Equals(string.Empty) ? null : txtProjectName.Text;  // Text 일경우 null 로
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                ShowLoadingIndicator();
                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_LOCATION";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, bizex) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        if (result.Rows.Count < 1) return;

                        result.Columns.Add("ROW_NUM", typeof(System.Int32));

                        DataTable GrTray = result.Clone();

                        List<string> sIdList = result.AsEnumerable().Select(c => c.Field<string>("WH_NAME")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["WH_NAME"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {
                            for (int j = 0; j < result.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["WH_NAME"].ToString() == result.Rows[j]["WH_NAME"].ToString())
                                {
                                    result.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }

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

                        //string[] sCol = new string[] { "ROW_NUM","WH_NAME", "RACK_NAME", "LOCATION_STAT", "MIX_ENABLE", "CAPA", "CST_QTY" };
                        //_Util.SetDataGridMergeExtensionCol(dgPalletListByLocation, sCol, DataGridMergeMode.VERTICALHIERARCHI);

                        //dgPalletListByLocation.MergingCells -= dgLocation_MergingCells;
                        //dgPalletListByLocation.MergingCells += dgLocation_MergingCells;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });
            
        }

        
        private void dgLocation_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        rkName = x.Field<string>("RACK_NAME")
                    }).Select(g => new
                    {
                        GroupRkName = g.Key.rkName,
                        Count = g.Count()
                    }).ToList();

                    string GroupRkName = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        foreach (var item in query)
                        {
                            int rowIndex = i;
                            string rowRkName = DataTableConverter.GetValue(dg.Rows[i].DataItem, "RACK_NAME").GetString();
                            if (rowRkName == item.GroupRkName && GroupRkName != rowRkName)
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["LOCATION_STAT"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["LOCATION_STAT"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["MIX_ENABLE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["MIX_ENABLE"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["CAPA"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["CAPA"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["CST_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["CST_QTY"].Index)));
                            }
                        }
                        GroupRkName = DataTableConverter.GetValue(dg.Rows[i].DataItem, "RACK_NAME").GetString();
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        #endregion

        #region [### 왼쪽 그리드 Model List 조회 ###]
        public void GetPalletListByModel()
        {

                if (cboArea.Items.Count <= 0) return;
                if (cboSection.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("RACK_NAME", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                dr["BLDGCODE"] = Util.GetCondition(cboBldg, bAllNull: true).ToString();
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                //dr["WH_ID"] = Util.GetCondition(cboSection, bAllNull: false).ToString();

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
                //dr["RACK_NAME"] = Util.NVC(_rackName).Equals(string.Empty) ? null : _rackName;

                dr["PRJT_NAME"] = Util.NVC(txtProjectName.Text).Equals(string.Empty) ? null : txtProjectName.Text;  // Text 일경우 null 로
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_MODEL";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, bizex) =>
                {
                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        if (result.Rows.Count < 1) return;

                        // Pallet 합계 수량 계산
                        DataTable seldt = result.Select().CopyToDataTable();
                        DataTable distinctDt = seldt.DefaultView.ToTable(true, "MDLLOT_ID", "CST_QTY");

                        var querySum = distinctDt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                        {
                            Sumcstqty = g.Sum(x => x.Field<Int32>("CST_QTY")),
                            Count = g.Count()
                        }).FirstOrDefault();

                        Util.GridSetData(dgPalletListByModel, result, FrameOperation, true);
                        DataGridAggregate.SetAggregateFunctions(dgPalletListByModel.Columns["CST_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = Util.NVC(querySum.Sumcstqty) } });
                        DataGridAggregate.SetAggregateFunctions(dgPalletListByModel.Columns["MDLLOT_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
           

        }

        #endregion

        #region [### 왼쪽 그리드 Empty Pallet List 조회 ###]
        public void GetPalletListByEmptyPallet()
        {
            //ShowLoadingIndicator();

            try
            {
                if (cboArea.Items.Count <= 0) return;
                if (cboSection.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDG", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                String _bldg = Util.GetCondition(cboBldg, bAllNull: true).ToString();
                dr["BLDG"] = Util.NVC(_bldg).Equals(string.Empty) ? null : _bldg;
                // Section 과 Rack 은 필요없슴 *********************
                String _whId = null;
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                string _rackId = null;
                dr["RACK_ID"] = Util.NVC(_rackId).Equals(string.Empty) ? null : _rackId;
                //*************************************************
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_EMPTY";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgPalletListByEmptyPallet, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
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

            Util.gridClear(dgPalletListByEmptyPallet);
            Util.gridClear(dgPalletListByEmptyPalletDetail);

            chkAllLocation.IsChecked = false;
            chkAllModel.IsChecked = false;

            /*
            txtSelectLot.Text = "";
            _AREAID = "";
            Util.gridClear(dgDefect);
            grdMBomTypeCnt.Visibility = Visibility.Collapsed;
            */
        }
        #endregion

        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #region [ 상세 Func ]
        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        

        // Location에 대한 Detail 조회로직 구성
        private void ShowLocationDetail(DataRowView drvRow , string _shpRsn)
        {
            ShowLoadingIndicator();

            Util.gridClear(dgPalletListByLocationDetail);

            try
            {
                if (cboArea.Items.Count <= 0) return;

                String _MDLLOT_ID = String.Empty;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("RACK_NAME", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("MDLLOT_ID", typeof(string));
                INDATA.Columns.Add("SHIPPING_RESN", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = drvRow["AREAID"].ToString();
                dr["BLDGCODE"] = drvRow["WH_PHYS_PSTN_CODE"].ToString();
                string _whid = drvRow["WH_ID"].ToString();
                dr["WH_ID"] = Util.NVC(_whid).Equals(string.Empty) ? null : _whid;
                string _rackid = drvRow["RACK_ID"].ToString();
                dr["RACK_ID"] = Util.NVC(_rackid).Equals(string.Empty) ? null : _rackid;
                string _rackname = drvRow["RACK_NAME"].ToString();
                dr["RACK_NAME"] = Util.NVC(_rackname).Equals(string.Empty) ? null : _rackname;
                string _prjtname = drvRow["PRJT_NAME"].ToString();
                dr["PRJT_NAME"] = Util.NVC(_prjtname).Equals(string.Empty) ? null : _prjtname;
                dr["MDLLOT_ID"] = Util.NVC(_MDLLOT_ID).Equals(string.Empty) ? null : _MDLLOT_ID;

                // 규격칼럼 외 선택시
                dr["SHIPPING_RESN"] = Util.NVC(_shpRsn).Equals(string.Empty) ? null : _shpRsn;
                // 규격칼럼 선택시   SHIPPING_RESN = *blank 
                if (_shpRsn.Equals("PRJT_NAME") || _shpRsn.Equals("CST_QTY")) {
                    dr["SHIPPING_RESN"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;
                }
                // 합계부분이면 상단의 PRJ를 규격에.
                // 합계칼럼 선택시   PRJ = *blank
                if (drvRow["WH_NAME"].ToString().Equals(_dicNameTotal))  
                {
                    string _sPrjName = txtProjectName.Text;
                    dr["PRJT_NAME"] = Util.NVC(_sPrjName).Equals(string.Empty) ? null : _sPrjName;

                    // Location List 는 ㅣLocation List 그리드 기준으로
                    DataTable dt = DataTableConverter.Convert(dgPalletListByLocation.ItemsSource);
                    string _rackId = Convert.ToString(dt.Rows[0]["RACK_ID"]);
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!_rackId.Equals(Convert.ToString(row["RACK_ID"]))) _rackId += "," + Convert.ToString(row["RACK_ID"]);
                    }
                    dr["RACK_ID"] = Util.NVC(_rackId).Equals(string.Empty) ? null : _rackId;
                }

                if (_shpRsn.Equals("CST_QTY"))
                {
                    string _sPrjName = txtProjectName.Text;
                    dr["PRJT_NAME"] = Util.NVC(_sPrjName).Equals(string.Empty) ? null : _sPrjName;
                }
                // 합계부분이고 PRJT_NAME 칼럼에 값이없고 PRJT_NAME 칼럼이면 "@@@" 처리 - 리스트 없슴 
                if (drvRow["WH_NAME"].ToString().Equals(_dicNameTotal) && string.IsNullOrEmpty(_prjtname) && _shpRsn.Equals("PRJT_NAME"))
                {
                    string _sPrjName = "@@@";
                    dr["PRJT_NAME"] = Util.NVC(_sPrjName).Equals(string.Empty) ? null : _sPrjName;
                }

                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                String _sbizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_PLLT_LIST";
                new ClientProxy().ExecuteService(_sbizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgPalletListByLocationDetail, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        // Model에 대한 Detail 조회로직 구성
        private void ShowModelDetail(DataRowView drvRow, string _shpRsn)
        {
            Util.gridClear(dgPalletListByModelDetail);

            ShowLoadingIndicator();

            try
            {
                if (cboArea.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("RACK_NAME", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("MDLLOT_ID", typeof(string));
                INDATA.Columns.Add("SHIPPING_RESN", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                dr["BLDGCODE"] = Util.GetCondition(cboBldg, bAllNull: true).ToString();
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                string _prjtname = drvRow["PRJT_NAME"].ToString();
                dr["PRJT_NAME"] = Util.NVC(_prjtname).Equals(string.Empty) ? null : _prjtname;

                string _mdllot = drvRow["MDLLOT_ID"].ToString();
                if (_mdllot.Equals(_dicNameTotal))
                {
                    // 합계부분이면 Model ID 에 *Blank
                    _mdllot = string.Empty;
                    // 합계부분이면 상단의 PRJ를 규격에.
                    // 합계부분이고 PRJT_NAME 칼럼에 값이 없으면 PRJT_NAME "@@@" 처리 - 리스트 없슴 
                    string _sPrjName = txtProjectName.Text;
                    if (string.IsNullOrEmpty(_prjtname) && _shpRsn.Equals("PRJT_NAME") ) _sPrjName = "@@@";
                    dr["PRJT_NAME"] = Util.NVC(_sPrjName).Equals(string.Empty) ? null : _sPrjName;

                    // Location List 는 ㅣLocation List 그리드 기준으로
                    DataTable dt = DataTableConverter.Convert(dgPalletListByLocation.ItemsSource);
                    string _rackId = Convert.ToString(dt.Rows[0]["RACK_ID"]);
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!_rackId.Equals(Convert.ToString(row["RACK_ID"]))) _rackId += "," + Convert.ToString(row["RACK_ID"]);
                    }
                    dr["RACK_ID"] = Util.NVC(_rackId).Equals(string.Empty) ? null : _rackId;

                }
                dr["MDLLOT_ID"] = Util.NVC(_mdllot).Equals(string.Empty) ? null : _mdllot;

                dr["SHIPPING_RESN"] = Util.NVC(_shpRsn).Equals(string.Empty) ? null : _shpRsn;
                // 규격칼럼 선택시   SHIPPING_RESN = *blank 
                if (_shpRsn.Equals("PRJT_NAME") || _shpRsn.Equals("CST_QTY"))
                    dr["SHIPPING_RESN"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;

                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string _sbizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_PLLT_LIST";
                new ClientProxy().ExecuteService(_sbizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgPalletListByModelDetail, result, FrameOperation, true);

                   // drvRow["MDLLOT_ID"] = _dicNameTotal;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }


        // Empty Type 대한 Detail 조회로직 구성
        private void ShowEmptyPalletDetail(DataRowView drvRow , string colName)
        {

            ShowLoadingIndicator();


            try
            {
                if (cboArea.Items.Count <= 0) return;


                String _BLDG = String.Empty;    
                String _WH_ID = String.Empty;   
                String _RACK_ID = String.Empty; 
                                                                                                                  
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDG", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("TYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                _BLDG = Util.GetCondition(cboBldg, bAllNull: true).ToString();
                dr["BLDG"] = Util.NVC(_BLDG).Equals(string.Empty) ? null : _BLDG;  
                dr["WH_ID"] = Util.NVC(_WH_ID).Equals(string.Empty) ? null : _WH_ID;
                dr["RACK_ID"] = Util.NVC(_RACK_ID).Equals(string.Empty) ? null : _RACK_ID;
                string _cstprod = drvRow["CSTPROD"].ToString();
                dr["TYPE"] = Util.NVC(_cstprod).Equals(string.Empty) ? null : _cstprod;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_GET_CELL_PLLT_LOCATION_STAT_EMPTY_PLLT_LIST", "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgPalletListByEmptyPalletDetail, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
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
                    || colName == "LOCATION_STAT"
                    || colName == "MIX_ENABLE"
                    || colName == "CAPA"
                    )
                {
                    return;
                }                

                if (dg.CurrentRow != null)
                {
                    chkAllLocation.IsChecked = false;
                    chkAllModel.IsChecked = false;

                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    _currentLocationColumnName = colName;
                    _currentLocationRow = drvRow;
                    if (dg.CurrentCell.Row.Type.ToString().Equals("Bottom"))
                    {
                        DataTable dt = ((DataView)dg.ItemsSource).ToTable().Clone();
                        DataView view = dt.AsDataView();

                        drvRow = view.AddNew();
                        drvRow["AREAID"] = cboArea.SelectedValue;
                        drvRow["WH_PHYS_PSTN_CODE"] = cboBldg.SelectedValue;
                        drvRow["WH_ID"] = string.Empty;
                        drvRow["RACK_ID"] = string.Empty;
                        drvRow["RACK_NAME"] = string.Empty;
                        string _prjName = txtProjectName.Text;
                        drvRow.EndEdit();
                        _currentLocationColumnName = colName;
                        _currentLocationRow = drvRow;
                    }

                    ShowLocationDetail(drvRow , GetShippingReason(colName));  //dgPalletListByLocation_DoubleClick

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
                if (colName.Equals("MDLLOT_ID"))
                {
                    return;
                }


                if (dg.CurrentRow != null)
                {

                    chkAllLocation.IsChecked = false;
                    chkAllModel.IsChecked = false;

                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));

                    if (dg.CurrentCell.Row.Type.ToString().Equals("Bottom"))
                    {
                        DataTable dt = ((DataView)dg.ItemsSource).ToTable().Clone();
                        DataView view = dt.AsDataView();

                        drvRow = view.AddNew();
                        drvRow["PRJT_NAME"] = Util.NVC(txtProjectName.Text.Trim());
                        drvRow.EndEdit();
                    }

                    _currentLocationColumnName = colName;
                    _currentLocationRow = drvRow;

                    ShowModelDetail(drvRow, GetShippingReason(colName));
                    
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

        private void dgPalletListByEmptyPallet_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;

                    Util.gridClear(dgPalletListByEmptyPalletDetail);
                    ShowEmptyPalletDetail(drvRow, colName);
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
                    if (    _col == "CST_QTY"
                         || _col == "PRJT_NAME"
                         || _col == "OK_QTY"
                         || _col == "NO_INSP_QTY"
                         || _col == "HOLD_QTY"
                         || _col == "LONG_TERM_QTY")
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
                    if (    _col == "PRJT_NAME"
                         || _col == "CST_QTY"
                         || _col == "OK_QTY"
                         || _col == "NO_INSP_QTY"
                         || _col == "HOLD_QTY"
                         || _col == "LONG_TERM_QTY")
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
                        if (!string.Equals(_col, "MDLLOT_ID"))
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


        private void dgPalletListByEmptyPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (_col == "WH_NAME"
                         || _col == "CSTPROD"
                         || _col == "PLLT_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }));

                if (_isscrollToHorizontalOffset)
                {
                    dataGrid.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void btnLocationSetting_Click(object sender, RoutedEventArgs e)
        {
            COM001_381_LOCATION_SETTING _popupLoad = new COM001_381_LOCATION_SETTING();
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
                Parameters[0] = Util.NVC(cboArea.SelectedValue);
                Parameters[1] = Util.NVC(cboBldg.SelectedValue);
                Parameters[2] = Util.NVC(cboSection.SelectedValue);

                string _rackId   = string.Empty;
                /*
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
                */
                Parameters[3] = Util.NVC(_rackId);

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


        private void btnChangeLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_381_CHANGE_LOCATION _popupLoad = new COM001_381_CHANGE_LOCATION();
                _popupLoad.FrameOperation = FrameOperation;

                if (_popupLoad != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(cboArea.SelectedValue);
                    Parameters[1] = Util.NVC(cboBldg.SelectedValue);
                    Parameters[2] = Util.NVC(cboSection.SelectedValue);

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

                    DataTable sdtBOX = dt.Clone();

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["CHK"] != null && !dr["CHK"].ToString().Equals(""))
                        {
                            if (Convert.ToBoolean(dr["CHK"]))
                            {
                                sdtBOX.ImportRow(dr);
                            }
                        }
                    }

                    if (sdtBOX.Rows.Count < 1)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1187"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    Parameters[3] = sdtBOX;

                    C1WindowExtension.SetParameters(_popupLoad, Parameters);

                    _popupLoad.Closed += new EventHandler(_popupChangeLocationLoad_Closed);
                    _popupLoad.ShowModal();
                    _popupLoad.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }


        }

        private void _popupLocationSettingLoad_Closed(object sender, EventArgs e)
        {
            COM001_381_LOCATION_SETTING runStartWindow = sender as COM001_381_LOCATION_SETTING;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                if (_sLocationLoadingYN)
                {
                    ClearValue();
                    GetPalletListByLocation();  // Location Setting POPUP Close
                    GetPalletListByModel();  // Location Setting POPUP Close
                    GetPalletListByEmptyPallet();  // Location Setting POPUP Close
                }
            }
        }


        private void _popupChangeLocationLoad_Closed(object sender, EventArgs e)
        {

            chkAllLocation.IsChecked = false;
            chkAllModel.IsChecked = false;

            COM001_381_CHANGE_LOCATION runStartWindow = sender as COM001_381_CHANGE_LOCATION;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                switch (_sTabName)
                {
                    case "Location":

                        Util.gridClear(dgPalletListByLocation);
                        ReLoadLocation( true );   // Popup Change Location Close
                        break;

                    case "Model":
                        Util.gridClear(dgPalletListByModel);
                        ReLoadModel();
                        break;

                    case "EmptyPallet":
                        break;

                }

                // btnSearch_Click(sender,null);
            }
            
        }


        public void ReLoadLocation( bool _sLoChgCloseYN )
        {

                if (cboArea.Items.Count <= 0) return;
                if (cboSection.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("RACK_NAME", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                dr["BLDGCODE"] = Util.GetCondition(cboBldg, bAllNull: true).ToString();
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
                dr["PRJT_NAME"] = Util.NVC(txtProjectName.Text).Equals(string.Empty) ? null : txtProjectName.Text;  // Text 일경우 null 로
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                ShowLoadingIndicator();

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_LOCATION";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, bizex) =>
                {
                    HiddenLoadingIndicator();


                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        if (result.Rows.Count < 1) return;

                        result.Columns.Add("ROW_NUM", typeof(System.Int32));

                        DataTable GrTray = result.Clone();

                        List<string> sIdList = result.AsEnumerable().Select(c => c.Field<string>("WH_NAME")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["WH_NAME"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {

                            for (int j = 0; j < result.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["WH_NAME"].ToString() == result.Rows[j]["WH_NAME"].ToString())
                                {
                                    result.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }

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


                        // Location Change POPUP 종료일때는 Pallet List 조회처리
                        if (_sLoChgCloseYN)
                        {
                            ReLoadLocationDetail();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });
        }

        private void ReLoadLocationDetail()
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = dgPalletListByLocation;

                String colName = _currentLocationColumnName;
                if (
                       colName == "ROW_NUM"
                    || colName == "WH_NAME"
                    || colName == "RACK_NAME"
                    || colName == "LOCATION_STAT"
                    || colName == "MIX_ENABLE"
                    || colName == "CAPA"
                    )
                {
                    return;
                }
                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = _currentLocationRow;

                    ShowLocationDetail(drvRow, GetShippingReason(colName));     // POPUP Location 변경후 ==> ReLoadLocationDetail()

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }



        public void ReLoadModel()
        {
                if (cboArea.Items.Count <= 0) return;
                if (cboSection.Items.Count <= 0) return;
                //if (cboLocation.Items.Count <= 0) return;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("RACK_NAME", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                dr["BLDGCODE"] = Util.GetCondition(cboBldg, bAllNull: true).ToString();
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                //dr["WH_ID"] = Util.GetCondition(cboSection, bAllNull: false).ToString();

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
                //dr["RACK_NAME"] = Util.NVC(_rackName).Equals(string.Empty) ? null : _rackName;

                dr["PRJT_NAME"] = Util.NVC(txtProjectName.Text).Equals(string.Empty) ? null : txtProjectName.Text;  // Text 일경우 null 로
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                 ShowLoadingIndicator();

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_MODEL";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, bizex) =>
                {
                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        if (result.Rows.Count < 1) return;

 
                        // Pallet 합계 수량 계산
                        DataTable seldt = result.Select().CopyToDataTable();
                        DataTable distinctDt = seldt.DefaultView.ToTable(true, "MDLLOT_ID", "CST_QTY");

                        var querySum = distinctDt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                        {
                            Sumcstqty = g.Sum(x => x.Field<Int32>("CST_QTY")),
                            Count = g.Count()
                        }).FirstOrDefault();


                        Util.GridSetData(dgPalletListByModel, result, FrameOperation, true);

                        DataGridAggregate.SetAggregateFunctions(dgPalletListByModel.Columns["CST_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = Util.NVC(querySum.Sumcstqty) } });
                        DataGridAggregate.SetAggregateFunctions(dgPalletListByModel.Columns["MDLLOT_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });


                        ReLoadModelDetail();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
        }

        private void ReLoadModelDetail()
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = dgPalletListByModel;

                String colName = _currentLocationColumnName;
                if (colName.Equals("MDLLOT_ID"))
                {
                    return;
                }


                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = _currentLocationRow;

                     ShowModelDetail(drvRow, GetShippingReason(_currentLocationColumnName));
                    
                }
            }
            catch (Exception ex)
            {
                //  Util.MessageException(ex);
            }
            finally
            {
            }



        }


        private void chkAllLocation_Click(object sender, RoutedEventArgs e)
        {
            if (chkAllLocation.IsChecked.Equals(true))
                Util.DataGridCheckAllChecked(dgPalletListByLocationDetail);
            else
                Util.DataGridCheckAllUnChecked(dgPalletListByLocationDetail);
        }

        private void chkAllModel_Click(object sender, RoutedEventArgs e)
        {
            if (chkAllModel.IsChecked.Equals(true))
                Util.DataGridCheckAllChecked(dgPalletListByModelDetail);
            else
                Util.DataGridCheckAllUnChecked(dgPalletListByModelDetail);
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

        private void txtProjectName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(sender, e);
            }
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

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("CSTID", typeof(string));
            INDATA.Columns.Add("WH_ID", typeof(string));
            INDATA.Columns.Add("RACK_ID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("ACTID", typeof(string));

            foreach (DataRow row in dtBox.Rows)
            {
                DataRow dr = INDATA.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["SRCTYPE"] = "UI";
                dr["ACTID"] = "RELEASE_LOCATION";
                dr["WH_ID"] = Util.NVC(Convert.ToString(row["WH_ID"]));
                dr["RACK_ID"] = Util.NVC(Convert.ToString(row["RACK_ID"]));
                dr["BOXID"] = Util.NVC(Convert.ToString(row["BOXID"]));
                dr["CSTID"] = Util.NVC(Convert.ToString(row["CSTID"]));

                INDATA.Rows.Add(dr);
            }


            ShowLoadingIndicator();   

            string sBizName = "BR_PRD_REG_CELL_PLLT_LOC_MNG_LOCATION_ACTID_BOUND";
            new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", null, (result, bizex) =>
            {

                if (bizex != null)
                {
                    Util.MessageException(bizex); 
                    return;
                }

                Util.MessageInfo("SFU1275");  // 정상처리 되었습니다.
                btnSearch_Click(null,null);

            }, inDataSet);

        }

        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}