/*************************************************************************************
 Created Date : 2023.09.23
      Creator : 백광영
   Decription : 조립 원자재 재고현황 - 수동 자재 요청
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Collections;
using LGC.GMES.MES.COM001;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_390_REQUEST_MTRL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        private string _sProcess = string.Empty;
        private string _sEqsg = string.Empty;
        private string _sEquipment = string.Empty;
        private string _sMtrlClss = string.Empty;

        public COM001_390_REQUEST_MTRL()
        {
            InitializeComponent();
        }

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
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            _sProcess = Util.NVC(tmps[0]);
            _sEqsg = Util.NVC(tmps[1]);
            _sEquipment = Util.NVC(tmps[2]);
            _sMtrlClss = Util.NVC(tmps[3]);

            DateTime firstOfDay = DateTime.Now;
            DateTime endOfDay = DateTime.Now.AddDays(7);

            dtpDateFrom.IsNullInitValue = true;
            dtpDateTo.IsNullInitValue = true;

            dtpDateFrom.SelectedDateTime = firstOfDay;
            dtpDateTo.SelectedDateTime = endOfDay;

            InitCombo();
        }
        #endregion

        #region Event
        private void InitCombo()
        {
            SetEquipmentSegment(cboEquipmentSegment);
            SetMtrlClass(cboMtrlClass);
            SetProcess(cboProcess);
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment(cboEquipment);
            }
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            SetEquipment(cboEquipment);
        }

        private void SetProcess(C1ComboBox _cb)
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PCSGID"] = "A";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.DisplayMemberPath = "CBO_NAME";
                _cb.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                _cb.ItemsSource = dtResult.Copy().AsDataView();

                cboProcess.SelectedValue = _sProcess;

                //if (!LoginInfo.CFG_PROC_ID.Equals(""))
                //{
                //    _cb.SelectedValue = LoginInfo.CFG_PROC_ID;

                //    if (_cb.SelectedIndex < 0)
                //        _cb.SelectedIndex = 0;
                //}
                //else
                //{
                //    if (_cb.Items.Count > 0)
                //        _cb.SelectedIndex = 0;
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSegment(MultiSelectionBox _cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    string[] _eqsgid;
                    _eqsgid = _sEqsg.Split(',');

                    if (_eqsgid.Length == 0)
                        _cb.CheckAll();

                    for (int i = 0; i < _eqsgid.Length; i++)
                    {
                        foreach (MultiSelectionBoxItem _cbitem in _cb.MultiSelectionBoxSource)
                        {
                            if (DataTableConverter.GetValue(_cbitem.Item, "CBO_CODE").GetString().Equals(_eqsgid[i].GetString()))
                                _cbitem.IsChecked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void SetEquipment(C1ComboBox _cb)
        {
            try
            {
                string sProc = Util.GetCondition(cboProcess);

                string sEquipmentSegment = cboEquipmentSegment.SelectedItemsToString;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = string.IsNullOrWhiteSpace(sProc) ? null : sProc;
                dr["PCSGID"] = "A";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_PCSGID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.DisplayMemberPath = "CBO_NAME";
                _cb.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                _cb.ItemsSource = dtResult.Copy().AsDataView();

                cboEquipment.SelectedValue = _sEquipment;

                if (_cb.Items.Count > 0)
                    _cb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMtrlClass(MultiSelectionBox _cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ASSY_MTRL_CLSS2_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                _cb.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    string[] _mtrlclss;
                    _mtrlclss = _sMtrlClss.Split(',');

                    if (_mtrlclss.Length == 0)
                        _cb.CheckAll();

                    for (int i = 0; i < _mtrlclss.Length; i++)
                    {
                        foreach (MultiSelectionBoxItem _cbitem in _cb.MultiSelectionBoxSource)
                        {
                            if (DataTableConverter.GetValue(_cbitem.Item, "CBO_CODE").GetString().Equals(_mtrlclss[i].GetString()))
                                _cbitem.IsChecked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            getRequestList();
        }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            getRequestList();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getRequestList();
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            dgRequestMaterial.EndEdit(true);

            if (dgRequestMaterial.ItemsSource == null || dgRequestMaterial.Rows.Count < 0)
                return;

            DataTable dt = (dgRequestMaterial.ItemsSource as DataView).Table;
            var query1 = dt.AsEnumerable().Where(x => x.Field<Int32>("CHK").Equals(1));
            if (query1.Count() <= 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 데이터가 없습니다
                return;
            }

            var query = dt.AsEnumerable().Where(x => x.Field<Int32>("CHK").Equals(1) && x.Field<Int32>("REQ_QTY") <= 0);
            if (query.Count() > 0)
            {
                Util.MessageValidation("SFU3371");    //수량이 0이상이어야 합니다.
                return;
            }

            // 보관수량 초과 요청 체크
            dt.AcceptChanges();
            DataTable seldt = dt.Select("CHK=1").CopyToDataTable();
            DataTable distinctDt = seldt.DefaultView.ToTable(true, "MTRL_PORT_ID");

            foreach (DataRow _drow in distinctDt.Rows)
            {
                DataTable portdt = seldt.Select("MTRL_PORT_ID = '" + _drow["MTRL_PORT_ID"] + "'").CopyToDataTable();
                var querySum = portdt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                {
                    SumUseqty = g.Sum(x => x.Field<Int32>("REQ_QTY")),
                    Count = g.Count()
                }).FirstOrDefault();

                int _AvailQty = Util.NVC_Int(portdt.Rows[0]["AVAIL_QTY"]);
                int _StckQty = Util.NVC_Int(portdt.Rows[0]["KEP_STCK_QTY"]);
                int _sumValidQty = _StckQty - _AvailQty;

                if (querySum.SumUseqty > _sumValidQty)
                {
                    return;
                }
            }

            //요청 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    _RequestMaterial();
                }
            }
            );
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void getRequestList()
        {
            try
            {
                if (Util.NVC(cboEquipmentSegment.SelectedItemsToString) == "")
                {
                    // 라인을 선택하세요.
                    Util.MessageValidation("SFU1223");
                    return;
                }
                if (Util.NVC(cboMtrlClass.SelectedItemsToString) == "")
                {
                    // 자재분류를 선택하세요.
                    Util.MessageValidation("SFU8676");
                    return;
                }
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (dgRequestMaterial.GetRowCount() > 0)
                   Util.gridClear(dgRequestMaterial);

                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("CLSS2_CODE", typeof(string));
                RQSTDT.Columns.Add("WO_EXCEPTFLAG", typeof(string));
                RQSTDT.Columns.Add("SDTTM", typeof(string));
                RQSTDT.Columns.Add("EDTTM", typeof(string));
                RQSTDT.Columns.Add("ALTER_MTRL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedItemsToString);
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["CLSS2_CODE"] = Util.NVC(cboMtrlClass.SelectedItemsToString);
                dr["WO_EXCEPTFLAG"] = chkwo.IsChecked == true ? "Y" : "N";
                dr["SDTTM"] = Util.GetCondition(dtpDateFrom);
                dr["EDTTM"] = Util.GetCondition(dtpDateTo);
                dr["ALTER_MTRL"] = chkaltnate.IsChecked == true ? null : "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_PORT_MTRL_REQUEST", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;

                Util.GridSetData(dgRequestMaterial, dtRslt, FrameOperation, true);
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

        private void _RequestMaterial()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = ((DataView)dgRequestMaterial.ItemsSource).Table;

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("SHOPID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("MTRL_CLSS_CODE", typeof(string));
                dtIndata.Columns.Add("PRJT_NAME", typeof(string));
                dtIndata.Columns.Add("REQ_QTY", typeof(Int32));
                dtIndata.Columns.Add("AUTO_SPLY_FLAG", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<Int32>("CHK").Equals(1) && t.Field<Int32>("REQ_QTY") > 0
                             select new
                             {
                                 _areaid = t.Field<string>("AREAID"),
                                 _eqsgid = t.Field<string>("EQSGID"),
                                 _portid = t.Field<string>("MTRL_PORT_ID"),
                                 _mtrlid = t.Field<string>("MTRLID"),
                                 _clss2 = t.Field<string>("MTRL_CLSS_CODE"),
                                 _prj = t.Field<string>("PRJT_NAME"),
                                 _reqty = t.Field<Int32>("REQ_QTY"),
                                 _autoflag = t.Field<string>("AUTO_SPLY_FLAG"),
                             }).ToList();

                foreach (var x in query)
                {
                    dr = dtIndata.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = x._areaid;
                    dr["EQSGID"] = x._eqsgid;
                    dr["MTRL_PORT_ID"] = x._portid;
                    dr["MTRLID"] = x._mtrlid;
                    dr["MTRL_CLSS_CODE"] = x._clss2;
                    dr["PRJT_NAME"] = x._prj;
                    dr["REQ_QTY"] = x._reqty;
                    dr["AUTO_SPLY_FLAG"] = x._autoflag;
                    dr["USERID"] = LoginInfo.USERID;
                    dtIndata.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_RACK_MTRL_BOX_STCK_KANBAN", "INDATA", null, ds);

                Util.MessageInfo("SFU1275");

                getRequestList();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgRequestMaterial_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Column.Name == null)
                    return;

                string _col = e.Cell.Column.Name.ToString();

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name != null)
                    {
                        // 가능수량
                        decimal _availqty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "AVAIL_QTY"));
                        // 보관수량
                        decimal _kepqty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "KEP_STCK_QTY"));

                        if (_col == "ON_HAND" || _col == "DELIVERING" || _col == "REQUEST")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.WhiteSmoke);
                        }
                        if (_col == "REQ_QTY")
                        {
                            if (_availqty >= _kepqty)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                            }
                        }
                    }
                }
            }));
        }

        private void dgRequestMaterial_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
            if (dg.CurrentRow != null)
            {
                int _ValueToAvail = Util.NVC_Int(DataTableConverter.GetValue(e.Row.DataItem, "AVAIL_QTY"));
                int _ValueToStock = Util.NVC_Int(DataTableConverter.GetValue(e.Row.DataItem, "KEP_STCK_QTY"));

                if (_ValueToAvail >= _ValueToStock)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                    (dg.GetCell(dg.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = true;
                }
            }
        }
        private void dgRequestMaterial_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
            int iRowIdx = 0;
            int iColIdx = 0;

            iRowIdx = dg.CurrentRow.Index;
            iColIdx = dg.CurrentColumn.Index;

            if (colName == "REQ_QTY")
            {
                string _PortId = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRowIdx].DataItem, "MTRL_PORT_ID"));

                DataTable dt = (dgRequestMaterial.ItemsSource as DataView).Table;
                DataTable portdt = dt.Select("MTRL_PORT_ID = '" + _PortId + "'").CopyToDataTable();

                var querySum = portdt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                {
                    SumUseqty = g.Sum(x => x.Field<Int32>("REQ_QTY")),
                    Count = g.Count()
                }).FirstOrDefault();

                int _AvailQty = Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[iRowIdx].DataItem, "AVAIL_QTY"));
                int _StckQty = Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[iRowIdx].DataItem, "KEP_STCK_QTY"));
                int _sumValidQty = _StckQty - _AvailQty;

                if (querySum.SumUseqty > _sumValidQty)
                {
                    // 가능수량 [%1] 보다 요청수량이 많습니다.
                    Util.MessageValidation("SFU9007", result =>
                    {
                        DataTableConverter.SetValue(dg.Rows[iRowIdx].DataItem, "REQ_QTY", 0);
                        DataTableConverter.SetValue(dg.Rows[iRowIdx].DataItem, "CHK", 0);
                    }, _StckQty);
                }
            }
        }
        #endregion
    }
}
