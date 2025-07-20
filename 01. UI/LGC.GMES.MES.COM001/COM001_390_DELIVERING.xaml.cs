/*************************************************************************************
 Created Date : 2023.09.23
      Creator : 백광영
   Decription : 조립 원자재 재고현황 - Delivering 자재 현황
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.19  백광영 : 라입별 자재Port 조회 시 AREAID 조회 조건 추가

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_390_DELIVERING : C1Window, IWorkArea
    {
        string _RackType = string.Empty;
        string _ReqStatus = string.Empty;
        string _MtrlPortID = string.Empty;
        string _sEquipmentSegment = string.Empty;
        bool _bFirst = false;

        public COM001_390_DELIVERING()
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
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            _RackType = Util.NVC(tmps[0]);
            _ReqStatus = Util.NVC(tmps[1]);
            _MtrlPortID = Util.NVC(tmps[2]);

            getMtrlStock();
            getMtrlPortInfo();
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            cboEquipmentSegment.SelectedValue = _sEquipmentSegment;
            
        }

        private void Init()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgComplete);
            txtMtrlPortID.Text = string.Empty;
            txtAvailQty.Value = 0;
            getMtrlStock();
            getDeliveringPort();
            dgLine_Checked();
        }


        /// <summary>
        /// Delivering 자재 현황
        /// </summary>
        private void getMtrlStock()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("REQ_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("RACK_TYPE", typeof(string));
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_STAT_CODE"] = _ReqStatus;
                dr["RACK_TYPE"] = _RackType;
                dr["MTRL_PORT_ID"] = _MtrlPortID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROD_RACK_MTRL_DELIVERING_BOX_STCK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    return;
                }
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
        private void getMtrlPortInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_PORT_ID"] = _MtrlPortID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_MMD_ELTR_ASSY_MTRL_PORT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    return;
                }
                _sEquipmentSegment = dtRslt.Rows[0]["EQSGID"].ToString();
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
        /// Line별 자재 Port
        /// </summary>
        private void getDeliveringPort()
        {
            try
            {
                Util.gridClear(dgLine);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_CLSS_CODE"] = _RackType;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PROD_RACK_DELIVERING_PORT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    return;
                }
                Util.GridSetData(dgLine, dtRslt, FrameOperation, true);
                
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
        /// Port ID, 투입가능 수량
        /// </summary>
        /// <param name="_portid"></param>
        private void GetPortInfo(string _portid)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_PORT_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_PORT_ID"] = _portid;

                inTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(inTable);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SEL_MTRL_PORT_KANBAN", "INDATA", "OUTDATA", inTable);
                if (dtResult.Rows.Count == 0)
                {
                    //조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905", result =>
                    {
                    });
                    return;
                }
                txtAvailQty.Value = Convert.ToDouble(dtResult.Rows[0]["AVAIL_COMPLETE"]);
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

        /// <summary>
        /// Port별 투입가능 자재 확인
        /// </summary>
        /// <param name="_portid"></param>
        /// <param name="_mtrlid"></param>
        /// <returns></returns>
        private bool _getPortMaterial(string _portid, string _mtrlid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MTRL_PORT_ID"] = _portid;
                dr["MTRLID"] = _mtrlid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_CHK_PORT_ID_KANBAN", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    return true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return true;
            }
        }
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Init();
        }

        private void dgLine_Checked()
        {
            for (int i = 0; i < dgLine.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgLine.Rows[i].DataItem, "MTRL_PORT_ID").Equals(_MtrlPortID)) 
                {
                    _bFirst = true;
                    DataTableConverter.SetValue(dgLine.Rows[i].DataItem, "CHK", true);
                    dgLine.SelectedIndex = i;
                }
            }
        }
        private void chkport_Click(object sender, RoutedEventArgs e)
        {
            dgList.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {
                DataTable dtTo = DataTableConverter.Convert(dgComplete.ItemsSource);

                if (dtTo.Columns.Count == 0)
                {
                    dtTo.Columns.Add("CHK", typeof(Boolean));
                    dtTo.Columns.Add("KANBAN_ID", typeof(string));
                }

                DataRowView drv = cb.DataContext as DataRowView;

                if (string.Equals(txtMtrlPortID.Text, ""))
                {
                    foreach (DataRowView item in dgList.ItemsSource)
                    {
                        if (drv["REQ_NO"].ToString().Equals(item["REQ_NO"].ToString()))
                        {
                            item["CHK"] = false;
                        }
                    }
                    // 투입포트를 선택하세요..
                    Util.MessageValidation("SFU9008");
                    return;
                }

                if (dgComplete.GetRowCount() + 1 > txtAvailQty.Value)
                {
                    foreach (DataRowView item in dgList.ItemsSource)
                    {
                        if (drv["REQ_NO"].ToString().Equals(item["REQ_NO"].ToString()))
                        {
                            item["CHK"] = false;
                        }
                    }
                    // 수량을 초과하였습니다
                    Util.MessageValidation("SFU1500");
                    return;
                }

                //중복 체크
                if (dtTo.Select("KANBAN_ID = '" + DataTableConverter.GetValue(cb.DataContext, "KANBAN_ID") + "'").Length > 0) 
                {
                    return;
                }

                if (!_getPortMaterial(txtMtrlPortID.Text, Util.NVC(drv["MTRLID"])))
                {
                    // Port에 투입할 수 없는 자재입니다. 그래도 투입하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU9010"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (!result.ToString().Equals("OK"))
                        {
                            foreach (DataRowView item in dgList.ItemsSource)
                            {
                                if (drv["REQ_NO"].ToString().Equals(item["REQ_NO"].ToString()))
                                {
                                    item["CHK"] = false;
                                }
                            }
                            return;
                        }
                        DataRow dr = dtTo.NewRow();
                        foreach (DataColumn dc in dtTo.Columns)
                        {
                            if (dc.DataType.Equals(typeof(Boolean)))
                            {
                                dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                            }
                            else
                            {
                                dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                            }
                        }

                        dtTo.Rows.Add(dr);
                        dgComplete.ItemsSource = DataTableConverter.Convert(dtTo);

                        DataRow[] drUnchk = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 0");
                    });
                }
                else
                {
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        if (dc.DataType.Equals(typeof(Boolean)))
                        {
                            dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                        }
                        else
                        {
                            dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                        }
                    }

                    dtTo.Rows.Add(dr);
                    dgComplete.ItemsSource = DataTableConverter.Convert(dtTo);

                    DataRow[] drUnchk = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 0");
                }
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgComplete.ItemsSource);

                if (dtTo.Rows.Count > 0)
                    dtTo.Rows.Remove(dtTo.Select("KANBAN_ID = '" + DataTableConverter.GetValue(cb.DataContext, "KANBAN_ID") + "'")[0]);

                dgComplete.ItemsSource = DataTableConverter.Convert(dtTo);
            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLine.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sPortID = DataTableConverter.GetValue(rb.DataContext, "MTRL_PORT_ID").ToString();

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                txtMtrlPortID.Text = sPortID;
                GetPortInfo(sPortID);
            }
            if (_bFirst)
            {
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;
                string sPortID = DataTableConverter.GetValue(rb.DataContext, "MTRL_PORT_ID").ToString();
                txtMtrlPortID.Text = sPortID;
                GetPortInfo(sPortID);
                _bFirst = false;
            }           
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (dgComplete.ItemsSource == null || dgComplete.Rows.Count < 0)
            {
                // 자재를 선택해주세요
                Util.MessageValidation("SFU3523");
                return;
            }

            if (string.Equals(txtMtrlPortID.Text, ""))
            {
                // 투입포트를 선택하세요..
                Util.MessageValidation("SFU9008");
                return;
            }

            if (dgComplete.GetRowCount() > txtAvailQty.Value)
            {
                // 수량을 초과하였습니다
                Util.MessageValidation("SFU1500");  
                return;
            }

            try
            {
                DataTable dt = ((DataView)dgComplete.ItemsSource).Table;

                var query = dt.AsEnumerable().Where(x => x.Field<Boolean>("CHK").Equals(true));
                if (query.Count() <= 0)
                {
                    Util.MessageValidation("SFU3538");    //선택된 데이터가 없습니다
                    return;
                }

                // 저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        _Complete();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _Complete()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dt = ((DataView)dgComplete.ItemsSource).Table;

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("KANBAN_ID", typeof(string));
                dtIndata.Columns.Add("CNFM_MTRL_PORT_ID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<Boolean>("CHK") == true
                             select new
                             {
                                 _kanbnaid = t.Field<string>("KANBAN_ID")
                             }).ToList();

                foreach (var x in query)
                {
                    dr = dtIndata.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["KANBAN_ID"] = x._kanbnaid;
                    dr["CNFM_MTRL_PORT_ID"] = txtMtrlPortID.Text;
                    dr["USERID"] = LoginInfo.USERID;
                    dtIndata.Rows.Add(dr);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_COMPLETE_PORT_ID_KANBAN", "INDATA", null, ds);

                Util.MessageInfo("SFU1275");

                Init();
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


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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
    }
}
