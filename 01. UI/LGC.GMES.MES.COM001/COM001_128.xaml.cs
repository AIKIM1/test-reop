/*************************************************************************************
 Created Date : 2018.06.20
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 전극 재공현황 조회 (조립 공정에서 사용)
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.20  INS 김동일K : Initial Created.

**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_128.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_128 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util(); 
        String _PRODID = "";

        public COM001_128()
        {
            InitializeComponent();
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
            try
            {
                //사용자 권한별로 버튼 숨기기
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnRoute);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                if (FrameOperation.AUTHORITY.Equals("W"))
                {
                    dgLotList.Columns["CHK"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
                }
                //여기까지 사용자 권한별로 버튼 숨기기

                //if (LoginInfo.CFG_AREA_ID.Length > 0 && LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("E"))
                //{
                    // 전극
                    dgLotList.Columns["COATING_DT"].Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    dgLotList.Columns["COATING_DT"].Visibility = Visibility.Collapsed;
                //}

                dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Collapsed;

                InitCombo();
                SetElec();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "ELTR_AREA_FOR_ASSY");

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;

            _combo.SetCombo(cboWipStat, CommonCombo.ComboStatus.ALL);

            DataTable dtWipStat = DataTableConverter.Convert(cboWipStat.ItemsSource);

            DataRow drIns = dtWipStat.NewRow();
            drIns["CBO_NAME"] = "HOLD:HOLD";
            drIns["CBO_CODE"] = "HOLD";
            dtWipStat.Rows.Add(drIns);

            cboWipStat.ItemsSource = dtWipStat.Copy().AsDataView();

            //cboWipStat.SelectedValue = "WAIT";

            string selectedWipstat = string.Empty;

            if (LoginInfo.CFG_ETC.Rows.Count > 0)
            {
                if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_WIPSTAT))
                {
                    selectedWipstat = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_WIPSTAT].ToString();
                }
            }

            if (dtWipStat.Rows.Count > 0)
                cboWipStat.SelectedValue = selectedWipstat;
            else
                cboWipStat.SelectedItem = null;

            cboWipStat.SelectedItemChanged += cboWipStat_SelectedItemChanged;
        }

        #endregion

        #region Event
        private void chkProdVerCode_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
        }

        private void chkProdVerCode_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
        }

        private void chkCollapsed_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["MODLID"].Visibility = Visibility.Collapsed;
            //dgSummary.Columns["PRODID"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["WAIT_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["WAIT_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PROC_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PROC_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["END_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["END_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["CONV_MOVING_OUT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_MOVING_OUT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_IN_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_MOVING_IN_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["SHIP_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["SHIP_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["SUM_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["SUM_QTY2"].Visibility = Visibility.Collapsed;

        }

        private void chkCollapsed_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["MODLID"].Visibility = Visibility.Visible;
            //dgSummary.Columns["PRODID"].Visibility = Visibility.Visible;
            dgSummary.Columns["PRODNAME"].Visibility = Visibility.Visible;
            dgSummary.Columns["WAIT_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["WAIT_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["PROC_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["PROC_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["END_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["END_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["CONV_MOVING_OUT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_MOVING_OUT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_IN_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_IN_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_MOVING_IN_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_MOVING_IN_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["SHIP_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["SHIP_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["SUM_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["SUM_QTY2"].Visibility = Visibility.Visible;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStock();
        }

        private void btnRoute_Click(object sender, RoutedEventArgs e)
        {
            if (_PRODID.Equals(""))
            {
                Util.MessageValidation("SFU1895");  //제품을 선택하세요.
                return;
            }

            COM001_018_ROUTE wndPopup = new COM001_018_ROUTE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = _PRODID;
                //Parameters[1] = org_set["TIME"].ToString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndRoute_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }


        private void wndRoute_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_018_ROUTE window = sender as COM001_018_ROUTE;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    string sRouteID = window.ROUTID;
                    string sFlowID = window.FLOWID;
                    string sProcId = window.PROCID;
                    string sWipTypeCode = window.WIP_TYPE;

                    //변경하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "변경하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {

                                    DataSet inData = new DataSet();
                                    DataTable dtRqst = inData.Tables.Add("INDATA");

                                    dtRqst.Columns.Add("SRCTYPE", typeof(string));
                                    dtRqst.Columns.Add("ROUTE_TO", typeof(string));
                                    dtRqst.Columns.Add("FLOW_TO", typeof(string));
                                    dtRqst.Columns.Add("PROCID_TO", typeof(string));
                                    dtRqst.Columns.Add("USERID", typeof(string));
                                    dtRqst.Columns.Add("WIP_TYPE_CODE", typeof(string));

                                    DataRow dr = dtRqst.NewRow();
                                    dr["SRCTYPE"] = "UI";
                                    dr["ROUTE_TO"] = sRouteID;
                                    dr["FLOW_TO"] = sFlowID;
                                    dr["PROCID_TO"] = sProcId;
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["WIP_TYPE_CODE"] = sWipTypeCode.Equals("SELECT") ? "" : sWipTypeCode;

                                    dtRqst.Rows.Add(dr);


                                    DataTable dtLot = inData.Tables.Add("INLOT");

                                    dtLot.Columns.Add("LOTID", typeof(string));

                                    DataRow[] chkRow = Util.gridGetChecked(ref dgLotList, "CHK");

                                    if (chkRow.Length == 0)
                                    {
                                        Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                                        return;
                                    }

                                    for (int i = 0; i < chkRow.Length; i++)
                                    {
                                        if (chkRow[i]["WIPSTAT"].ToString().Equals("PROC"))
                                        {
                                            Util.MessageValidation("SFU1917");  //진행중인 LOT이 있습니다.
                                            return;
                                        }

                                        DataRow drLot = dtLot.NewRow();
                                        drLot["LOTID"] = chkRow[i]["LOTID"].ToString();
                                        dtLot.Rows.Add(drLot);
                                    }

                                    try
                                    {
                                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_CHANGE_ROUTE", "INDATA,INLOT", null, inData);

                                        Util.AlertInfo("SFU1166");  //변경되었습니다.
                                        cboWipStat_SelectedItemChanged(null, null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                    }
                                }

                            });

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboWipStat_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgSummary.GetRowCount() > 0 && dgSummary.CurrentRow != null)
            {
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROD_VER_CODE")));
            }
        }


        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("PROCNAME"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROD_VER_CODE")));

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), "", null);

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }
                else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["JUDG_FLAG"] != null &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_FLAG")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                }

            }));
        }

        private void GetDetailLot(string sProcId, string sProdId, string sProdVerCode)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = sProcId;
                dr["PRODID"] = sProdId == "" ? null : sProdId;
                dr["WIPSTAT"] = Util.GetCondition(cboWipStat).Trim() == "" ? null : Util.GetCondition(cboWipStat);

                if ((bool)chkProdVerCode.IsChecked)
                    dr["PROD_VER_CODE"] = sProdVerCode;

                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

                dtRqst.Rows.Add(dr);


                string bizRuleName;
                //if (string.Equals(sProcId, Process.ASSEMBLY))
                //{
                //    bizRuleName = "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_V01_MOBILE";
                //    dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Visible;
                //}
                //else
                //{
                    dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Collapsed;

                // 전극창고 PANCAKE QMS정보가 필요하다고 하여 BIZ 분리
                //2022-12-29 오화백  동 :EP 추가
                if ((string.Equals(Util.NVC(cboArea.SelectedValue), "E5") || string.Equals(Util.NVC(cboArea.SelectedValue), "E6") || string.Equals(Util.NVC(cboArea.SelectedValue), "EP")) && (string.Equals(sProcId, Process.ELEC_STORAGE) || string.IsNullOrEmpty(sProcId)))
                {
                    if (string.IsNullOrEmpty(sProdId))
                    {
                        bizRuleName = "DA_INF_ELTR_SEL_STOCK_IN_AREA_DETAIL_TO_INNER_QMS";

                    }
                    else
                    {
                        bizRuleName = "DA_INF_ELTR_SEL_STOCK_IN_AREA_DETAIL_TO_INNER_QMS1";
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(sProdId))
                    {
                        bizRuleName = "DA_INF_ELTR_SEL_STOCK_IN_AREA_DETAIL_TO";
                    }
                    else
                    {
                        bizRuleName = "DA_INF_ELTR_SEL_STOCK_IN_AREA_DETAIL_TO_V01";
                    }
                }
                //}

                //dtRslt = new ClientProxy().ExecuteServiceSync(string.IsNullOrEmpty(sProdId) ? "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO" : "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_V01", "INDATA", "OUTDATA", dtRqst);
                dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotList);
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgLotList_AutoGeneratedColumns(object sender, EventArgs e)
        {
            dgLotList.GroupBy(dgLotList.Columns["HOLD"]);
            dgLotList.FilterBy(dgLotList.Columns["HOLD"], new DataGridFilterState()
            {
                FilterInfo = new List<DataGridFilterInfo>(
                    new DataGridFilterInfo[1]
                    {
                        new DataGridFilterInfo()
                        {
                            FilterOperation = DataGridFilterOperation.Contains, FilterType = DataGridFilterType.Text, Value = "Y"
                        }
                    })
            });
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Method
        private void GetStock()
        {
            try
            {
                if (Util.NVC(cboProcess.SelectedItemsToString) == "")
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                //string sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER" : "DA_PRD_SEL_STOCK_IN_AREA";
                string sBizName = chkProdVerCode.IsChecked == true ? "DA_INF_ELTR_SEL_STOCK_IN_AREA_VER_TO" : "DA_INF_ELTR_SEL_STOCK_IN_AREA_TO";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = cboProcess.SelectedItemsToString;  //1888 Util.GetCondition(cboProcess);
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                dr["MODLID"] = Util.GetCondition(txtModlId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgLotList);


                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "PROCNAME", "MODLID" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MODLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRDT_CLSS_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["UNIT_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_OUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_OUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_OUT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_OUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_OUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_OUT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_PLANT_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SetElec()
        {

            try
            {
                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("AREAID", typeof(string));

                //DataRow dr = dtRqst.NewRow();

                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                //dtRqst.Rows.Add(dr);

                //DataTable dtRslt = new DataTable();

                //dtRslt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "OUTDATA", dtRqst);

                //if (dtRslt.Rows.Count > 0)
                //{
                //    if (dtRslt.Rows[0]["S01"].ToString().Equals("E"))
                //    {
                tbElecType.Visibility = Visibility.Visible;
                cboElecType.Visibility = Visibility.Visible;
                dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Visible;
                //btnRoute.Visibility = Visibility.Visible;
                //    }
                //    else
                //    {
                //        tbElecType.Visibility = Visibility.Collapsed;
                //        cboElecType.Visibility = Visibility.Collapsed;
                //        dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Collapsed;
                btnRoute.Visibility = Visibility.Collapsed;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INF_ELTR_SEL_WIP_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboProcess.Check(i);

                    //if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals(LoginInfo.CFG_EQSG_ID))
                    //{
                    //    cboProcess.Check(i);
                    //    break;
                    //}
                    //else
                    //    cboProcess.Check(i);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void CheckHold_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null)
                return;
            try
            {

                DataGridFilterState fs = null;
                DataGridFilterInfo fsFilter = null;
                List<DataGridFilterInfo> fsList = null;

                fsFilter = new DataGridFilterInfo();
                fs = new DataGridFilterState();
                fsList = new List<DataGridFilterInfo>();


                fsFilter.FilterType = DataGridFilterType.Text;
                fsFilter.Value = "Y";

                fsList.Add(fsFilter);
                fs.FilterInfo = fsList;
                dgLotList.FilterBy(dgLotList.Columns[15], fs, false); //"WIPHOLD"
                dgLotList.Reload(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckHold_Unhecked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null)
                return;
            try
            {
                DataGridFilterState fs = null;
                DataGridFilterInfo fsFilter = null;
                List<DataGridFilterInfo> fsList = null;

                fsFilter = new DataGridFilterInfo();
                fs = new DataGridFilterState();
                fsList = new List<DataGridFilterInfo>();


                fsFilter.FilterType = DataGridFilterType.Text;
                //fsFilter.Value = "N";

                fsList.Add(fsFilter);
                fs.FilterInfo = fsList;
                dgLotList.FilterBy(dgLotList.Columns[15], fs, false); //"WIPHOLD"
                dgLotList.Reload(false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        
        
    }
}
