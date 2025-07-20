/*************************************************************************************
 Created Date : 2023.05.29
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.29  주재홍 : Initial Created.
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_386 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _StackingYN = string.Empty;

        Util _Util = new Util();

        private BizDataSet _Biz = new BizDataSet();
        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;

        public COM001_386()
        {
            InitializeComponent();
            InitCombo();
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
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboBldg };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //cboLocation.ApplyTemplate();

            SetcboAreaBldg(cboBldg);
            SetcboSection(cboSection);
            //SetcboLocation(cboLocation);
        }

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

                cbo.SelectedIndex = 0;

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
            //SetcboLocation(cboLocation);
            //}

        }

        #endregion

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetPalletListByLocation();
            GetPalletListByModel();
            GetPalletListByEmptyPallet();
        }
        #endregion


        #region [BizCall]

        
        #region [### 왼쪽 그리드 Location List 조회 ###]
        public void GetPalletListByLocation()
        {/*
            ShowLoadingIndicator();

            try
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

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_LOCATION";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
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

                        Util.GridSetData(dgPalletListByLocation, result, FrameOperation, true);

                        //string[] sCol = new string[] { "ROW_NUM","WH_NAME", "RACK_NAME", "LOCATION_STAT", "MIX_ENABLE", "CAPA", "CST_QTY" };
                        //_Util.SetDataGridMergeExtensionCol(dgPalletListByLocation, sCol, DataGridMergeMode.VERTICALHIERARCHI);

                        dgPalletListByLocation.MergingCells -= dgLocation_MergingCells;
                        dgPalletListByLocation.MergingCells += dgLocation_MergingCells;


                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
            */
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
            /*
            // ShowLoadingIndicator();

            try
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

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_STAT_MODEL";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgPalletListByModel, result, FrameOperation, true);
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
            */
        }

        #endregion

        #region [### 왼쪽 그리드 Empty Pallet List 조회 ###]
        public void GetPalletListByEmptyPallet()
        {
            /*
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
            */

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

           // Util.gridClear(dgPalletListByEmptyPallet);
           // Util.gridClear(dgPalletListByEmptyPalletDetail);

            /*
           // txtSelectLot.Text = "";
           // _AREAID = "";
           // Util.gridClear(dgDefect);
           // grdMBomTypeCnt.Visibility = Visibility.Collapsed;
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
                dr["WH_ID"] = drvRow["WH_ID"].ToString();
                dr["RACK_ID"] = drvRow["RACK_ID"].ToString();
                dr["RACK_NAME"] = drvRow["RACK_NAME"].ToString();
                dr["PRJT_NAME"] = drvRow["PRJT_NAME"].ToString();
                if (_shpRsn.Equals(string.Empty))
                    dr["PRJT_NAME"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;
                dr["MDLLOT_ID"] = Util.NVC(_MDLLOT_ID).Equals(string.Empty) ? null : _MDLLOT_ID;
                dr["SHIPPING_RESN"] = Util.NVC(_shpRsn).Equals(string.Empty) ? null : _shpRsn;
                if (_shpRsn.Equals("PRJT_NAME"))
                    dr["SHIPPING_RESN"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_GET_CELL_PLLT_LOCATION_STAT_PLLT_LIST", "INDATA", "OUTDATA", INDATA, (result, Exception) =>
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
            /*
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
                dr["PRJT_NAME"] = drvRow["PRJT_NAME"].ToString();
                if (_shpRsn.Equals(string.Empty))
                    dr["PRJT_NAME"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;
                dr["MDLLOT_ID"] = drvRow["MDLLOT_ID"].ToString();
                dr["SHIPPING_RESN"] = Util.NVC(_shpRsn).Equals(string.Empty) ? null : _shpRsn;
                if (_shpRsn.Equals("PRJT_NAME"))
                    dr["SHIPPING_RESN"] = Util.NVC(string.Empty).Equals(string.Empty) ? null : string.Empty;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_GET_CELL_PLLT_LOCATION_STAT_PLLT_LIST", "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Util.GridSetData(dgPalletListByModelDetail, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
            */
        }


        // Empty Type 대한 Detail 조회로직 구성
        private void ShowEmptyPalletDetail(DataRowView drvRow)
        {
            /*
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
                dr["BLDG"] = Util.NVC(_BLDG).Equals(string.Empty) ? null : _BLDG;  
                dr["WH_ID"] = Util.NVC(_WH_ID).Equals(string.Empty) ? null : _WH_ID;
                dr["RACK_ID"] = Util.NVC(_RACK_ID).Equals(string.Empty) ? null : _RACK_ID;
                dr["TYPE"] = drvRow["CSTPROD"].ToString();
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
            */

        }

        #endregion

        #region [Event 이벤트]

        private void dgPalletListByLocation_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    ShowLocationDetail(drvRow , GetShippingReason(cell.Column.Name));
                        
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
                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    ShowModelDetail(drvRow, GetShippingReason(cell.Column.Name));
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

        private void dgPalletListByEmptyPallet_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    ShowEmptyPalletDetail(drvRow);
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
            }

            return _shpRsn;
        }

        #endregion

        private void cboLocation_SelectionChanged(object sender, EventArgs e)
        {

        }


        private void dgPalletListByLocation_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (    _col == "WH_NAME" 
                         || _col == "CST_QTY"
                         || _col == "PRJT_NAME"
                         || _col == "OK_QTY"
                         || _col == "NO_INSP_QTY"
                         || _col == "HOLD_QTY"
                         || _col == "LONG_TERM_QTY")
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
                    if (    _col == "MDLLOT_ID"
                         || _col == "PRJT_NAME"
                         || _col == "CST_QTY"
                         || _col == "OK_QTY"
                         || _col == "NO_INSP_QTY"
                         || _col == "HOLD_QTY"
                         || _col == "LONG_TERM_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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
            /*
            COM001_381_LOCATION_SETTING _popupLoad = new COM001_381_LOCATION_SETTING();
            _popupLoad.FrameOperation = FrameOperation;

            if (_popupLoad != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Util.NVC(cboArea.SelectedValue);
                Parameters[1] = Util.NVC(cboBldg.SelectedValue);
                Parameters[2] = Util.NVC(cboSection.SelectedValue);

                string _rackId   = string.Empty;
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
                Parameters[3] = Util.NVC(_rackId);

                C1WindowExtension.SetParameters(_popupLoad, Parameters);

                _popupLoad.Closed += new EventHandler(_popupLocationSettingLoad_Closed);
                _popupLoad.ShowModal();
                _popupLoad.CenterOnScreen();
            }
            */
        }


        private void btnChangeLocation_Click(object sender, RoutedEventArgs e)
        {
            btnChangeLocation_Process(sender, e, "location");
        }
        private void btnChangeLocationModel_Click(object sender, RoutedEventArgs e)
        {
            btnChangeLocation_Process(sender, e, "model");
        }
        private void btnChangeLocationEmptyPallet_Click(object sender, RoutedEventArgs e)
        {
            btnChangeLocation_Process(sender, e, "emptyPallet");
        }

        private void btnChangeLocation_Process(object sender, RoutedEventArgs e , string btnName)
        {
            /*
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

                    DataTable dt = DataTableConverter.Convert(dgPalletListByLocationDetail.ItemsSource);
                    if (btnName.Equals("model"))
                        dt = DataTableConverter.Convert(dgPalletListByModelDetail.ItemsSource);
                    if (btnName.Equals("emptyPallet"))
                        dt = DataTableConverter.Convert(dgPalletListByEmptyPalletDetail.ItemsSource);
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
            */
        }


/*
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgPalletListByLocationDetail);
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgPalletListByLocationDetail);
        }

*/
        private void _popupLocationSettingLoad_Closed(object sender, EventArgs e)
        {
            COM001_381_LOCATION_SETTING runStartWindow = sender as COM001_381_LOCATION_SETTING;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {

            }
        }


        private void _popupChangeLocationLoad_Closed(object sender, EventArgs e)
        {
            COM001_381_CHANGE_LOCATION runStartWindow = sender as COM001_381_CHANGE_LOCATION;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
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

        private void btnAddSeq_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}