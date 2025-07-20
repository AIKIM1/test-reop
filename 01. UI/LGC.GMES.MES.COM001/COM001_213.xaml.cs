/*************************************************************************************
 Created Date : 2018.01.15
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_213 : UserControl, IWorkArea
    {

        Util _Util = new Util();

        #region Declaration & Constructor 
        public COM001_213()
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
            Initialize();
            SetControl();
            SetCombo();

            SetEvent();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            btnMoveCancel.IsEnabled = false;
        }

        private void SetControl()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMoveCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            // 인계일자
            dtpDateFrom_Move.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo_Move.SelectedDateTime = (DateTime)System.DateTime.Now;

            // 인수일자
            dtpDateFrom_Receive.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo_Receive.SelectedDateTime = (DateTime)System.DateTime.Now;
        }

        private void SetCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 이동유형
            string[] sFilter2 = { "MOVE_TYPE_CODE_MP_FM" };
            _combo.SetCombo(cboMoveTransType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 이동상태
            string[] sFilter1 = { "MOVE_ORD_STAT_CODE" };
            _combo.SetCombo(cboMoveStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            // 재공구분
            String[] sFilterQLTY = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cbowipType, CommonCombo.ComboStatus.ALL, sFilter: sFilterQLTY, sCase: "COMMCODES");

            // 인계동
            cboArea_Move.ApplyTemplate();
            SetComboArea(cboArea_Move);

            // 인계공정
            cboProcess_Move.ApplyTemplate();
            SetComboProcess(cboProcess_Move, cboArea_Move);

            // 인계라인
            cboEquipmentSegment_Move.ApplyTemplate();
            SetComboquipmentSegment(cboEquipmentSegment_Move, cboArea_Move, cboProcess_Move);

            // 인수동
            cboArea_Receive.ApplyTemplate();
            SetComboArea(cboArea_Receive);

            // 인수공정
            cboProcess_Receive.ApplyTemplate();
            SetComboProcess(cboProcess_Receive, cboArea_Receive);

            // 인수라인
            cboEquipmentSegment_Receive.ApplyTemplate();
            SetComboquipmentSegment(cboEquipmentSegment_Receive, cboArea_Receive, cboProcess_Receive);

            cboArea_Move.SelectionChanged += cboArea_Move_SelectionChanged;
            cboProcess_Move.SelectionChanged += cboProcess_Move_SelectionChanged;
            cboArea_Receive.SelectionChanged += cboArea_Receive_SelectionChanged;
            cboProcess_Receive.SelectionChanged += cboProcess_Receive_SelectionChanged;
   
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            //인계이력 - 인계일자
            dtpDateFrom_Move.SelectedDataTimeChanged += dtpDateFrom_Move_SelectedDataTimeChanged;
            dtpDateTo_Move.SelectedDataTimeChanged += dtpDateTo_Move_SelectedDataTimeChanged;
            //인계이력 - 인수일자
            dtpDateFrom_Receive.SelectedDataTimeChanged += dtpDateFrom_Receive_SelectedDataTimeChanged;
            dtpDateTo_Receive.SelectedDataTimeChanged += dtpDateTo_Receive_SelectedDataTimeChanged;
        }

        #endregion

        #region Event

        private void cboArea_Move_SelectionChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();

            cboProcess_Move.SelectionChanged -= cboProcess_Move_SelectionChanged;
            SetComboProcess(cboProcess_Move, cboArea_Move);
            SetComboquipmentSegment(cboEquipmentSegment_Move, cboArea_Move, cboProcess_Move);
            cboProcess_Move.SelectionChanged += cboProcess_Move_SelectionChanged;
        }

        private void cboProcess_Move_SelectionChanged(object sender, EventArgs e)
        {
            SetComboquipmentSegment(cboEquipmentSegment_Move, cboArea_Move, cboProcess_Move);
        }

        private void cboArea_Receive_SelectionChanged(object sender, EventArgs e)
        {
            cboProcess_Receive.SelectionChanged -= cboProcess_Receive_SelectionChanged;
            SetComboProcess(cboProcess_Receive, cboArea_Receive);
            SetComboquipmentSegment(cboEquipmentSegment_Receive, cboArea_Receive, cboProcess_Receive);
            cboProcess_Receive.SelectionChanged += cboProcess_Receive_SelectionChanged;
        }

        private void cboProcess_Receive_SelectionChanged(object sender, EventArgs e)
        {
            SetComboquipmentSegment(cboEquipmentSegment_Receive, cboArea_Receive, cboProcess_Receive);
        }

        // 인계 Check
        private void chkMove_Checked(object sender, RoutedEventArgs e)
        {
            dtpDateFrom_Move.IsEnabled = true;
            dtpDateTo_Move.IsEnabled = true;
        }
        // 인계 UnCheck
        private void chkMove_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpDateFrom_Move.IsEnabled = false;
            dtpDateTo_Move.IsEnabled = false;
        }

        // 인수 Check
        private void chkRecive_Checked(object sender, RoutedEventArgs e)
        {
            dtpDateFrom_Receive.IsEnabled = true;
            dtpDateTo_Receive.IsEnabled = true;
        }
        // 인수 UnCheck
        private void chkRecive_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpDateFrom_Receive.IsEnabled = false;
            dtpDateTo_Receive.IsEnabled = false;
        }

        // 인계일자(From)
        private void dtpDateFrom_Move_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Move.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Move.SelectedDateTime;
                return;
            }
        }
        // 인계일자(TO)
        private void dtpDateTo_Move_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Move.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Move.SelectedDateTime;
                return;
            }
        }

        // 인수일자(From)
        private void dtpDateFrom_Receive_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Receive.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Receive.SelectedDateTime;
                return;
            }
        }
        // 인수일자(TO)
        private void dtpDateTo_Receive_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Receive.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Receive.SelectedDateTime;
                return;
            }
        }

        private void dgResult_Move_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgResult_Move.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if(e.Cell.Column.Name == "MOVE_ORD_ID2")
                    {
                        dgResult_Move.Columns["MOVE_ORD_ID2"].Width = new C1.WPF.DataGrid.DataGridLength(0);
                    }

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MOVE_ORD_ID2").ToString() == string.Empty)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D8BFD8"));

                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                    if (e.Cell.Column.Name.Equals("CTNR_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }


                }
            }));
        }

        private void dgResult_Move_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgResult_Move_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            if (dg.CurrentColumn.Name.Equals("CTNR_ID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                // 활성화 Pallet별 생산실적 조회로 이동
                loadingIndicator.Visibility = Visibility.Visible;

                object[] parameters = new object[1];
                parameters[0] = DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CTNR_ID").GetString();
                this.FrameOperation.OpenMenu("SFU010160710", true, parameters);

                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void dgCartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        //row 색 바꾸기
                        dgResult_Move.SelectedIndex = idx;

                        // 버튼 활성화 >>> 이동조료이고 이동공정이 Formation인 경우
                        if (Util.NVC(drv.Row["MOVE_ORD_STAT_CODE"]).Equals("CLOSE_MOVE") && Util.NVC(drv.Row["TO_PROCID"]).Equals("F1000"))
                        {
                            btnMoveCancel.IsEnabled = true;
                        }
                        else
                        {
                            btnMoveCancel.IsEnabled = false;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            Search();
        }

        /// <summary>
        /// 활성화이동취소
        /// </summary>
        private void btnMoveCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMoveCancel())
                return;

            // %1 취소 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("이동");

            Util.MessageConfirm("SFU4620", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    MoveCancel();
                }
            }, parameters);
        }

        #endregion

        #region Mehod

        private void SetComboArea(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
         
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
           
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ALL_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                    cbo.Check(LoginInfo.CFG_AREA_ID);
                }
                else
                {
                    cbo.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboProcess(MultiSelectionBox cbo, MultiSelectionBox cboarea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboarea.SelectedItemsToString);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_PROCESS", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                    cbo.Check(LoginInfo.CFG_PROC_ID);
                }
                else
                {
                    cbo.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboquipmentSegment(MultiSelectionBox cbo, MultiSelectionBox cboarea, MultiSelectionBox cboprocess)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboarea.SelectedItemsToString);
                dr["PROCID"] = Util.NVC(cboprocess.SelectedItemsToString);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cbo.isAllUsed = true;
                    cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        cbo.Check(i);
                    }
                }
                else
                {
                    cbo.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //조회
        private void Search()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = "DA_PRD_SEL_CTNR_MOVE_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("MOVE_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("MOVE_ORD_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(String));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("CHK_MOVE", typeof(string));
                dtRqst.Columns.Add("FROM_MOVE_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_MOVE_DTTM", typeof(string));
                dtRqst.Columns.Add("FROM_AREA_ID", typeof(string));
                dtRqst.Columns.Add("FROM_PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_EQSGID", typeof(string));

                dtRqst.Columns.Add("CHK_RECEIVE", typeof(string));
                dtRqst.Columns.Add("FROM_END_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_END_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_AREA_ID", typeof(string));
                dtRqst.Columns.Add("TO_PROCID", typeof(string));
                dtRqst.Columns.Add("TO_EQSGID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["MOVE_TYPE_CODE"] = cboMoveTransType.SelectedValue ?? null;                                          //이동유형
                dr["MOVE_ORD_STAT_CODE"] = cboMoveStat.SelectedValue ?? null;                                           //이동상태
                dr["WIP_QLTY_TYPE_CODE"] = cbowipType.SelectedValue ?? null;
                dr["PJT_NAME"] = txtMovePJT.Text.ToString();                                                            //PRJT명
                dr["PRODID"] = txtMoveProd.Text.ToString();                                                             //PRODID
                dr["LOTID_RT"] = txtMoveLotRt.Text.ToString();                                                          //조립LOT
                dr["CTNR_ID"] = txtCTNR_ID.Text.ToString();                                                             //대차ID
                dr["CHK_MOVE"] = (bool)chkMove.IsChecked ? "Y" : null;
                dr["FROM_MOVE_DTTM"] = Util.GetCondition(dtpDateFrom_Move);                                             //인계일자(From)
                dr["TO_MOVE_DTTM"] = Util.GetCondition(dtpDateTo_Move);                                                 //인계일자(To) 
                dr["FROM_AREA_ID"] = Util.NVC(cboArea_Move.SelectedItemsToString);                                      //인계동
                dr["FROM_PROCID"] = Util.NVC(cboProcess_Move.SelectedItemsToString);                                    //인계공정은 필수입니다
                dr["FROM_EQSGID"] = Util.NVC(cboEquipmentSegment_Move.SelectedItemsToString);                           //인계라인
                dr["CHK_RECEIVE"] = (bool)chkRecive.IsChecked ? "Y" : null;
                dr["FROM_END_DTTM"] = Util.GetCondition(dtpDateFrom_Receive);                                           //인수일자(From)
                dr["TO_END_DTTM"] = Util.GetCondition(dtpDateTo_Receive);                                               //인수일자(To) 
                dr["TO_AREA_ID"] = Util.NVC(cboArea_Receive.SelectedItemsToString);                                     //인수동
                dr["TO_PROCID"] = Util.NVC(cboProcess_Receive.SelectedItemsToString);                                   //인수공정은 필수입니다
                dr["TO_EQSGID"] = Util.NVC(cboEquipmentSegment_Receive.SelectedItemsToString);                          //인수라인
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgResult_Move, bizResult, FrameOperation, true);

                        // 대차 개수
                        int CtnrCount = bizResult.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;
                        DataGridAggregate.SetAggregateFunctions(dgResult_Move.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("대차") + ": " + CtnrCount.ToString("###,###") } });
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// FCS 이동 취소
        /// </summary>
        private void MoveCancel()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("MOVE_MTHD_CODE", typeof(string));
                inTable.Columns.Add("CNCL_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["MOVE_MTHD_CODE"] = "COMMON";
                newRow["CNCL_USERID"] = LoginInfo.USERID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = _Util.GetDataGridFirstRowBycheck(dgResult_Move, "CHK").Field<string>("CTNR_ID").GetString();
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_SEND_CTNR_TO_FORM", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        Search();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]
        private bool ValidationSearch()
        {
            if ((bool)chkMove.IsChecked  == false && (bool)chkRecive.IsChecked == false)
            {
                // 인계일자 또는 인수일자 둘다 체크 해제 상태 입니다.
                Util.MessageValidation("SFU4619");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(cboArea_Move.SelectedItemsToString)))
            {
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("동");

                // 인계 [%1]을 선택하세요.
                Util.MessageValidation("SFU4622", parameters);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(cboProcess_Move.SelectedItemsToString)))
            {
                // 인계공정을 선택하세요
                Util.MessageValidation("SFU4394");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(cboArea_Receive.SelectedItemsToString)))
            {
                // 인수동을 선택하세요
                Util.MessageValidation("SFU4395");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(cboProcess_Receive.SelectedItemsToString)))
            {
                // 인수공정은 필수입니다
                Util.MessageValidation("SFU4476");
                return false;
            }

            return true;
        }

        private bool ValidationMoveCancel()
        {
            if (dgResult_Move.Rows.Count - dgResult_Move.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            int rowIndex = _Util.GetDataGridCheckFirstRowIndex(dgResult_Move, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgResult_Move.Rows[rowIndex].DataItem, "MOVE_ORD_STAT_CODE")).Equals("CLOSE_MOVE") &&
                Util.NVC(DataTableConverter.GetValue(dgResult_Move.Rows[rowIndex].DataItem, "TO_PROCID")).Equals("F1000"))
            {
                // 활성화 이동취소 가능
            }
            else
            {
                object[] parameters = new object[2];
                parameters[0] = ObjectDic.Instance.GetObjectName("이동") + ObjectDic.Instance.GetObjectName("종료");
                parameters[1] = ObjectDic.Instance.GetObjectName("이동취소");

                // [%1]인 대차만 [%2]할 수 있습니다.
                Util.MessageValidation("SFU4623", parameters);
                return false;
            }

            return true;
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
