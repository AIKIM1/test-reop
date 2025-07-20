/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이슬아
   Decription : 포장출고
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.06  이슬아 : Initial Created.
  2023.03.08  이홍주 : 소형활성화 MES복사
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_300 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        public FCS002_300()
        {
            InitializeComponent();
        }

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
            InitCombo();
            InitControl();
            SetEvent();
            Init();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            GetEqptWrkInfo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: new C1ComboBox[] { cboLine }, sCase: "ALLAREA");

            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea }, cbChild: new C1ComboBox[] { cboEqpt });

            _combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "PACK_RCV_ISS_STAT_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
          
            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { "F6000" }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
                  
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA", cbChild : new C1ComboBox[] { cboLine2 });

            _combo.SetCombo(cboArea3, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");

            _combo.SetCombo(cboLine2, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea2 }, sFilter: new string[] { "B" }, sCase: "PROCESSSEGMENTLINE");


        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom2.SelectedDateTime = DateTime.Today;
            dtpDateTo2.SelectedDateTime = DateTime.Today;
            dtpDateFrom3.SelectedDateTime = DateTime.Today;
            dtpDateTo3.SelectedDateTime = DateTime.Today;
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

            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;

            dtpDateFrom3.SelectedDataTimeChanged += dtpDateFrom3_SelectedDataTimeChanged;
            dtpDateTo3.SelectedDataTimeChanged += dtpDateTo3_SelectedDataTimeChanged;
        }

        #region 기간
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


        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                return;
            }
        }
        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
                return;
            }
        }


        private void dtpDateFrom3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo3.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo3.SelectedDateTime;
                return;
            }
        }
        private void dtpDateTo3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom3.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom3.SelectedDateTime;
                return;
            }
        }

        #endregion


        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void cboArea2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtWorker.Tag = txtWorker.Text = string.Empty;
        }
        private void dgSearhResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }


                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblReady.Tag))
                    {
                        e.Cell.Presenter.Background = lblReady.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipped.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipped.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
                    {
                        e.Cell.Presenter.Background = lblCancel.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        private void dgSearhResultChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearhResult.ItemsSource)
                {
                    if (drv["BOXID"].ToString().Equals(item["BOXID"].ToString()))
                    {
                        item["CHK"] = true;
                    }
                }
                //row색 변경부분 추가

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void btnShipCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (!Validation(btn.Name))
                    return;

                // SFU4120	출고취소시 물류창고에서 팔레트 태그를 회수하셔야 합니다. 출고취소 하시겠습니까?	
                Util.MessageConfirm("SFU4120", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("USERID");

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["USERID"] = txtWorker.Tag;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataTable dt = ((DataView)dgSearhResult.ItemsSource).Table;
                        var query = (from t in dt.AsEnumerable()
                                     where t.Field<bool>("CHK")
                                     select t).ToList();
                        foreach (DataRow dr in query)
                        {
                            DataRow newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = dr["BOXID"].ToString();
                            inBoxTable.Rows.Add(newRow);
                        }


                        new ClientProxy().ExecuteService_Multi("BR_PRD_END_CANCEL_INPALLET_LIST_MB", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                Search();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);
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
        }
        private void btnSplitMerge_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (!Validation(btn.Name))
                return;

            DataTable dtParam = new DataTable();
            dtParam.Columns.Add("BOXID");
            dtParam.Columns.Add("LANGID");


            DataTable dt = ((DataView)dgSearhResult.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                         where t.Field<bool>("CHK")
                         select t.Field<string>("BOXID")).ToList().Distinct();
            // string boxId = query[0].Field<string>("BOXID");
                foreach (string id in query)
            {
                DataRow newRow = dtParam.NewRow();
                newRow["BOXID"] = id;
                newRow["LANGID"] = LoginInfo.LANGID;
                dtParam.Rows.Add(newRow);
            }

            //FCS002_300_SPLIT_MERGE popUp = new FCS002_300_SPLIT_MERGE { FrameOperation = FrameOperation };
            //if (popUp != null)
            //{
            //    object[] Parameters = new object[5];
            //    Parameters[0] = dtParam;
            //    Parameters[1] = txtWorker.Text;  //userid
            //    Parameters[2] = txtShift.Text; // shitid
            //    Parameters[3] = cboWrkSupplier.Text; //wrkSupplier
            //    C1WindowExtension.SetParameters(popUp, Parameters);
            //    popUp.ShowModal();
            //    popUp.CenterOnScreen();
            //    popUp.Closed += new EventHandler(SplitMerge_Closed);
            //}

        }

        private void dgSearhResultChoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearhResult.ItemsSource)
                {
                    if (drv["BOXID"].ToString().Equals(item["BOXID"].ToString()))
                    {
                        item["CHK"] = false;
                    }
                }
                //drv["CHK"] = false;
                //row색 변경부분 추가

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }
        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnPrintTag_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (!Validation(btn.Name))
                    return;

                // 메세지처리 출고 하시겠습니까?
                Util.MessageConfirm("출고 하시겠습니까?", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        DataRow inDataRow = inDataTable.NewRow();
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("SHFT_ID");
                        inDataTable.Columns.Add("WRK_SUPPLIERID");
                        inDataTable.Columns.Add("EXP_DOM_TYPE_CODE");
                        inDataTable.Columns.Add("LANGID");

                        inDataRow["USERID"] = txtWorker.Tag;
                        inDataRow["SHFT_ID"] = txtShift.Tag;
                       // inDataRow["WRK_SUPPLIERID"] = cboWrkSupplier.SelectedValue;
                        inDataRow["EXP_DOM_TYPE_CODE"] = ""; //?
                        inDataRow["LANGID"] = LoginInfo.LANGID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataTable dt = ((DataView)dgSearhResult.ItemsSource).Table;
                        var query = (from t in dt.AsEnumerable()
                                     where t.Field<bool>("CHK")
                                     select t).ToList();

                        foreach (DataRow dr in query)
                        {
                            DataRow newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = dr["BOXID"].ToString();
                            inBoxTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_INPALLET_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                Search();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {

                            }
                        }, indataSet);

                    }
                });              
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
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_INPALLET_FOR_SHIP_FM
        /// </summary>
        private void Search()
        {
            try
            {


                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROJECT", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("CHR_EQPTID", typeof(string));
                RQSTDT.Columns.Add("ASSY_EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
             
                if (!string.IsNullOrWhiteSpace(txtBoxID.Text))
                {
                    dr["BOXID"] = txtBoxID.Text;
                }

               else
                {
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["RCV_ISS_STAT_CODE"] = string.IsNullOrWhiteSpace(Util.NVC(cboStat.SelectedValue)) ? null : cboStat.SelectedValue;
                    dr["CHR_EQPTID"] = string.IsNullOrWhiteSpace(Util.NVC(cboEqpt.SelectedValue)) ? null : cboEqpt.SelectedValue;
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtPkgLotID.Text)? null : txtPkgLotID.Text;
                    dr["PROJECT"] = string.IsNullOrWhiteSpace(txtProject.Text) ? null : txtProject.Text;
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProd.Text) ? null : txtProd.Text;
                    dr["AREAID"] = String.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) ? null : cboArea.SelectedValue;
                    dr["EQSGID"] = String.IsNullOrWhiteSpace(Util.NVC(cboLine2.SelectedValue)) ? null : cboLine2.SelectedValue;
                }             

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_FOR_SHIP_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgSearhResult, dtResult, FrameOperation, true);
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

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC(cboArea2.SelectedValue) == string.Empty || Util.NVC(cboArea2.SelectedValue) == "SELECT")
                {
                    // SFU1499 동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                    return;
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("PROJECT", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtLotID2.Text))
                {
                    dr["LOTID"] = txtLotID2.Text.Trim();
                }
                else
                {
                    dr["AREAID"] = cboArea2.SelectedValue;
                    dr["EQSGID"] = string.IsNullOrWhiteSpace(Util.NVC(cboLine2.SelectedValue))? null : cboLine2.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["PROJECT"] = String.IsNullOrWhiteSpace(txtProject2.Text) ? null :  txtProject2.Text;
                    dr["PRODID"] = String.IsNullOrWhiteSpace(txtProdId2.Text) ? null : txtProdId2.Text;
                    dr["PKG_LOTID"] = String.IsNullOrWhiteSpace(txtPkgLotID2.Text) ? null : txtPkgLotID2.Text;
                    dr["BOXID"] = String.IsNullOrWhiteSpace(txtBoxID2.Text) ? null : txtBoxID2.Text;


                }

                RQSTDT.Rows.Add(dr); 

                 DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_SHIP_ERP_TRSF_HIST_MB", "INDATA", "OUTDATA", RQSTDT);
                Util.GridSetData(dgSearhResult2, SearchResult, FrameOperation, true);

                if (dgSearhResult2.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult2.Columns["RCV_ISS_ID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult2.Columns["ISS_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtLotID2.Text))
                {
                    dr["LOTID"] = txtLotID2.Text.Trim();
                }
                else
                {
                    dr["AREAID"] = String.IsNullOrWhiteSpace(Util.NVC(cboArea3.SelectedValue)) ? null : cboArea3.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom3.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo3.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                }

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_OWMS_SHIP_HIST_MB", "INDATA", "OUTDATA", RQSTDT);
                Util.GridSetData(dgSearhResult3, SearchResult, FrameOperation, true);

                if (dgSearhResult3.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult3.Columns["LOTID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult3.Columns["LOTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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
        private bool Validation(string btnName)
        {
            if (dgSearhResult.ItemsSource == null || dgSearhResult.Rows.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            //bool result = true;
            DataTable dt = ((DataView)dgSearhResult.ItemsSource).Table;
            var query = (from t in dt.AsEnumerable()
                         where t.Field<bool>("CHK")
                         select t).ToList();

            if (!query.Any())
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker.Text) || string.IsNullOrEmpty(Util.NVC(txtWorker.Tag)))
            {
                // SFU1843 작업자를 입력 해주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if (string.IsNullOrEmpty(txtShift.Text) || string.IsNullOrEmpty(Util.NVC(txtShift.Tag)))
            {
                // SFU1844 작업조를 입력 해주세요.
                Util.MessageValidation("SFU1844");
                return false;
            }
            //if (cboWrkSupplier.SelectedItem == null || Util.NVC(cboWrkSupplier.SelectedValue) == "SELECT")
            //{
            //    ControlsLibrary.MessageBox.Show("작업업체를 선택해주세요.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
            //    return false;
            //}

            switch (btnName)
            {
                case "btnSplitMerge":
                    DataRow firstRow = query.First();
                    foreach (DataRow dr in query)
                    {
                        if (!(dr["RCV_ISS_STAT_CODE"].Equals("READY_SHIP") || dr["RCV_ISS_STAT_CODE"].Equals("CANCEL_SHIP")))
                        {
                            ControlsLibrary.MessageBox.Show("출고대기 또는 출고취소 상태인 팔레트만 분할/병합 가능합니다.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return false;
                        }
                        if(dr["EQSGID"].ToString()!=firstRow["EQSGID"].ToString() || dr["PRODID"].ToString() != firstRow["PRODID"].ToString() || dr["SOC_VALUE"].ToString() != firstRow["SOC_VALUE"].ToString())
                        {
                            ControlsLibrary.MessageBox.Show("같은 Line, 제품, SOC 팔레트만 분할/병합 가능합니다.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return false;
                        }

                    }
                    break;

                case "btnShip":
                    foreach (DataRow dr in query)
                    {

                        if (!(dr["RCV_ISS_STAT_CODE"].Equals("READY_SHIP") || dr["RCV_ISS_STAT_CODE"].Equals("CANCEL_SHIP")))
                        {
                            ControlsLibrary.MessageBox.Show("출고대기 또는 출고취소 상태인 팔레트만 출고 가능합니다.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return false;
                        }

                        if (dr["WIP_QLTY_TYPE_CODE"].Equals("N"))
                        {
                            ControlsLibrary.MessageBox.Show("불량팔레트는 출고 불가능합니다..", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return false;
                        }
                        
                    }
                    break;

                case "btnShipCancel":
                    foreach (DataRow dr in query)
                    {
                        if (!(dr["RCV_ISS_STAT_CODE"].Equals("SHIPPING")))
                        {
                            ControlsLibrary.MessageBox.Show("출고중 상태인 팔레트만 출고취소 가능합니다.", "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return false;
                        }
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }
        #endregion

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = LoginInfo.CFG_EQPT_ID;
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
        }
        #endregion

        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
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
                });
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgMap);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgMap);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (dgMap.Rows.Count > 1)
                //{
                //    // 한건만 선택하세요.
                //    Util.MessageInfo("SFU4159");
                //    return;
                //}

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("USERID");
                RQSTDT.Columns.Add("TESLA_BCR_1");
                RQSTDT.Columns.Add("TESLA_BCR_2");
                RQSTDT.Columns.Add("TESLA_BCR_3");
                RQSTDT.Columns.Add("TESLA_BCR_4");
                RQSTDT.Columns.Add("SHIP_NO");

                for (int i = 0; i < dgMap.Rows.Count; i++)
                {
                    DataRow dr = RQSTDT.NewRow();

                    dr["BOXID"] = DataTableConverter.GetValue(dgMap.Rows[i].DataItem, "PALLETID") as string;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["TESLA_BCR_1"] = DataTableConverter.GetValue(dgMap.Rows[i].DataItem, "TESLA_BCR_1") as string;
                    dr["SHIP_NO"] = DataTableConverter.GetValue(dgMap.Rows[i].DataItem, "SHIP_NO") as string;

                    RQSTDT.Rows.Add(dr);
                }
                
                new ClientProxy().ExecuteService("BR_PRD_REG_TESLA_PLT_LABEL_MAP_MB", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    // 저장이 완료되었습니다.
                    Util.MessageInfo("10004");
                    Init();
                });
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
        private void DataGridRowAdd(C1DataGrid dg)
        {
            try
            {
                //if (dg.GetRowCount() > 0)
                //{
                //    return;
                //}

                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);

                DataRow dr2 = dt.NewRow();

                dr2["PALLETID"] = "";
                dr2["TESLA_BCR_1"] = "";
                dr2["TESLA_BCR_2"] = "";
                dr2["TESLA_BCR_3"] = "";
                dr2["TESLA_BCR_4"] = "";
                dr2["SHIP_NO"] = "";
                dt.Rows.Add(dr2);
                dt.AcceptChanges();
           
                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DataGridRowRemove(C1DataGrid dg)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dg.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnUnMap_Click(object sender, RoutedEventArgs e)
        {
            FCS002_300_TESLA_UNMAP popup = new FCS002_300_TESLA_UNMAP();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] = LoginInfo.USERID;


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puInboxLabel_Closed(object sender, EventArgs e)
        {
            FCS002_300_TESLA_UNMAP popup = sender as FCS002_300_TESLA_UNMAP;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
            Init();
        }

        void Init()
        {
            dgMap.ItemsSource = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("PALLETID", typeof(string));
            dt.Columns.Add("TESLA_BCR_1", typeof(string));
            dt.Columns.Add("TESLA_BCR_2", typeof(string));
            dt.Columns.Add("TESLA_BCR_3", typeof(string));
            dt.Columns.Add("TESLA_BCR_4", typeof(string));
            dt.Columns.Add("SHIP_NO", typeof(string));

            dgMap.ItemsSource = DataTableConverter.Convert(dt);

        }

        private void dgMap_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMap_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
            }));

        }
    }
}
