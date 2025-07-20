/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_054 : UserControl, IWorkArea
    {

        #region Declaration & Constructor       


        public COM001_054()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            // Area 셋팅
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            string[] sFilter1 = { "CELL_PROD_TYPE" };
            combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");


            // Area 셋팅
            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            string[] sFilter2 = { "CELL_DFCT_TYPE" };
            combo.SetCombo(cboType2, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
        }

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        #endregion

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Cell();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_Cell();
        }

        private void Search_Cell()
        {
            try
            {
                string sArea = string.Empty;

                // 동 선택 확인
                if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("MMD0004");   //동을 선택해 주세요.
                    return;
                }
                
                string[] toInfo = Convert.ToString(cboArea.SelectedValue).Split('^');

                string sLot_id = txtLotid.Text.Trim();
                string sProd_id = txtProdid.Text.Trim();
                string sType = cboType.SelectedValue.ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("VIEW_FCS", typeof(String));
                RQSTDT.Columns.Add("VIEW_DEFECT", typeof(String));
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = toInfo[0];
                dr["PRODID"] = sProd_id == "" ? null : sProd_id;

                if (cboType.SelectedValue.ToString() == "DEFECT")
                {
                    dr["VIEW_FCS"] = null;
                    dr["VIEW_DEFECT"] = sType;
                }
                else if (cboType.SelectedValue.ToString() == "FCS")
                {
                    dr["VIEW_FCS"] = sType;
                    dr["VIEW_DEFECT"] = null;
                }
                else
                {
                    dr["VIEW_FCS"] = null;
                    dr["VIEW_DEFECT"] = null;
                }

                dr["SUBLOTID"] = sLot_id == "" ? null : sLot_id;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new DataTable();

                if (cboType.SelectedValue.ToString().Trim().Equals(""))
                {
                    SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_STOCK_ALL", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_STOCK", "RQSTDT", "RSLTDT", RQSTDT);
                }

                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");      //재공 정보가 없습니다.
                    return;
                }

                Util.gridClear(dgCell);
                Util.gridClear(dgCell_Detail);

                Util.GridSetData(dgCell, SearchResult, FrameOperation);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgCell.Resources["ResultTemplate1"] as DataTemplate;
                dac.Add(dagsum);

                DataGridAggregate.SetAggregateFunctions(dgCell.Columns["TOTAL_CNT"], dac);
                DataGridAggregate.SetAggregateFunctions(dgCell.Columns["FCS_CNT"], dac);
                DataGridAggregate.SetAggregateFunctions(dgCell.Columns["DEFECT_CNT"], dac);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgCellChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgCell.SelectedIndex = idx;

                    Search_Lotinfo(DataTableConverter.GetValue(dgCell.Rows[idx].DataItem, "PRODID").ToString(), dgCell_Detail);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void Search_Lotinfo(String sProdid, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            {
                string[] toInfo = Convert.ToString(cboArea.SelectedValue).Split('^');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = sProdid;
                dr["AREAID"] = toInfo[0];

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_STOCK_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCell_Detail, SearchResult, FrameOperation);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgCell_Detail.Resources["ResultTemplate2"] as DataTemplate;
                dac.Add(dagsum);

                DataGridAggregate.SetAggregateFunctions(dgCell_Detail.Columns["TOTAL_CNT"], dac);
                DataGridAggregate.SetAggregateFunctions(dgCell_Detail.Columns["FCS_CNT"], dac);
                DataGridAggregate.SetAggregateFunctions(dgCell_Detail.Columns["DEFECT_CNT"], dac);
                DataGridAggregate.SetAggregateFunctions(dgCell_Detail.Columns["SCRAP_CNT"], dac);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgCell_Detail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
                if (datagrid.CurrentRow == null || datagrid.CurrentRow.Index < 0)
                    return;

                string sProdid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PRODID"].Index).Value);
                string sLotid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value);
                string[] toInfo = Convert.ToString(cboArea.SelectedValue).Split('^');

                COM001_054_DETAIL wndConfirm = new COM001_054_DETAIL();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = sLotid;
                    Parameters[1] = sProdid;
                    Parameters[2] = toInfo[0];

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_054_DETAIL window = sender as COM001_054_DETAIL;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtLotid2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Hist();
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Search_Hist();
        }

        private void Search_Hist()
        {
            try
            {
                string[] toInfo = Convert.ToString(cboArea2.SelectedValue).Split('^');

                string sProd_id = txtProdid2.Text.Trim();
                string sLot_id = txtLotid2.Text.Trim();
                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("DEFECT", typeof(String));
                RQSTDT.Columns.Add("SCRAP", typeof(String));
                RQSTDT.Columns.Add("RETURN", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));


                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = toInfo[0];
                dr["PRODID"] = sProd_id == "" ? null : sProd_id;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;

                //if (cboType.SelectedValue.ToString() == "DEFECT")
                //{
                //    dr["DEFECT"] = sProd_id == "" ? null : sProd_id;
                //    dr["SCRAP"] = sProd_id == "" ? null : sProd_id;
                //    dr["RETURN"] = sProd_id == "" ? null : sProd_id;
                //}
                //else if (cboType.SelectedValue.ToString() == "DISPOSAL")
                //{
                //    dr["DEFECT"] = sProd_id == "" ? null : sProd_id;
                //    dr["SCRAP"] = sProd_id == "" ? null : sProd_id;
                //    dr["RETURN"] = sProd_id == "" ? null : sProd_id;
                //}
                //else if (cboType.SelectedValue.ToString() == "RETURN")
                //{
                //    dr["DEFECT"] = sProd_id == "" ? null : sProd_id;
                //    dr["SCRAP"] = sProd_id == "" ? null : sProd_id;
                //    dr["RETURN"] = sProd_id == "" ? null : sProd_id;
                //}
                //else
                //{
                //    dr["DEFECT"] = null;
                //    dr["SCRAP"] = null;
                //    dr["RETURN"] = null;
                //}

                dr["DEFECT"] = cboType2.SelectedValue.ToString() == "DEFECT" ? cboType2.SelectedValue.ToString() : null;
                dr["SCRAP"] = cboType2.SelectedValue.ToString() == "DISPOSAL" ? cboType2.SelectedValue.ToString() : null;
                dr["RETURN"] = cboType2.SelectedValue.ToString() == "RETURN" ? cboType2.SelectedValue.ToString() : null;


                dr["LOTID"] = sLot_id == "" ? null : sLot_id;


                RQSTDT.Rows.Add(dr);

                Util.gridClear(dgCell_Detail2);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_DEFECT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCell2, SearchResult, FrameOperation);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgCell2.Resources["ResultTemplate3"] as DataTemplate;
                dac.Add(dagsum);

                DataGridAggregate.SetAggregateFunctions(dgCell2.Columns["SCRAP_CNT"], dac);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgCell2Choice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgCell2.SelectedIndex = idx;

                    Search_Detail(DataTableConverter.GetValue(dgCell2.Rows[idx].DataItem, "PRODID").ToString(), dgCell_Detail2);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void Search_Detail(String sProdid, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            {
                string[] toInfo = Convert.ToString(cboArea.SelectedValue).Split('^');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = sProdid;
                dr["AREAID"] = toInfo[0];

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_DEFECT_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCell_Detail2, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }


    }
}
