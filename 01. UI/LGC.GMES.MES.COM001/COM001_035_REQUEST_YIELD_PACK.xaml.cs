/*************************************************************************************
 Created Date : 2016.12.27
      Creator : 정규환
  Description : Hold 해제 승인요청(Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  : Initial Created.
  2023.02.13  : 정용석 : 승인자 Popup Open시 승인자/참조자 구분 추가
  2023.02.20  정용석 : 결재요청 순서도 OUTPUT Parameter 추가로 인한 수정
**************************************************************************************/

using C1.WPF;
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
using System.Linq;

using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_YIELD_PACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private string _scrapType = string.Empty;
        private string _TabStat = string.Empty;
        //private string _searchYM = string.Empty;
        private bool _bERPclose_YN = false;  // 전월 ERP 마감 여부 ( true : 마감완료,  false : 미 마감)
        Double dDiffDate;

        DataTable dtSearchResult;
        CommonCombo _combo = new CommonCombo();

        DateTime dDefaultCalDate;

        private struct MOVE_TYPE
        {
            public const string DOWN = "DOWN";
            public const string UP = "UP";
        }

        public COM001_035_REQUEST_YIELD_PACK()
        {
            InitializeComponent();

            InitCombo();

            // 사용자 임의 선택 금지
            cboCauseShop.IsEnabled = false;
            cboCauseArea.IsEnabled = false;
            cboCauseProc.IsEnabled = false;
            dtCalDate.IsEnabled = false;
            cboLossResnCode.IsEnabled = false;
            txtCauseProd.IsEnabled = false;
            btnCauseProd.IsEnabled = false;

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //Loss 사유
                string[] sFilter2 = { "SCRAP_LOT_YIELD" };
                C1ComboBox[] cboLossResnCodeChild = { cboCauseProc };
                _combo.SetCombo(cboLossResnCode, CommonCombo.ComboStatus.SELECT, cbChild: cboLossResnCodeChild, sCase: "YIELDREASON", sFilter: sFilter2);

                //원인공정
                C1ComboBox[] cboCauseProcParent = { cboLossResnCode };
                _combo.SetCombo(cboCauseProc, CommonCombo.ComboStatus.NONE, cbParent: cboCauseProcParent, sCase: "YIELDCAUSEPROC", sFilter: sFilter2);

                cboLossResnCode.SelectedValue = "Y05"; // 허수 재공

                CauseCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CauseCombo()
        {
            //원인SHOP
            string[] sFilter3 = { "P" };
            C1ComboBox[] cboCauseShopChild = { cboCauseArea };
            _combo.SetCombo(cboCauseShop, CommonCombo.ComboStatus.ALL, cbChild: cboCauseShopChild, sCase: "SHOP_AREATYPE", sFilter: sFilter3);

            //원인동
            C1ComboBox[] cboCauseAreaParent = { cboCauseShop };
            C1ComboBox[] cboCauseAreaChild = { cboCauseEqsg };
            _combo.SetCombo(cboCauseArea, CommonCombo.ComboStatus.ALL, cbParent: cboCauseAreaParent, cbChild: cboCauseShopChild, sCase: "AREA");

            //라인            
            C1ComboBox[] cboCauseEqsgParent = { cboCauseArea };
            _combo.SetCombo(cboCauseEqsg, CommonCombo.ComboStatus.NONE, cbParent: cboCauseEqsgParent, sCase: "EQUIPMENTSEGMENT");

        }
        #endregion

        #region Load Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null && tmps.Length >= 1)
                {
                    dDiffDate = Double.Parse(Util.NVC(tmps[0]));

                    //_searchYM = dtCalDate.SelectedDateTime.ToString("yyyyMM");
                    _bERPclose_YN = Util.NVC(tmps[1]).Equals("Y") ? true : false;

                    if (_bERPclose_YN)
                    {
                        dtCalDate.SelectedDateTime = (DateTime)System.DateTime.Now;
                        // Util.MessageValidation("SFU8339"); // ERP 생산실적이 마감 되었습니다. \n월초 부터 마감전까지 사용가능합니다.
                        rdoExMonth.Visibility = Visibility.Collapsed;
                        rdoNow.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //dtCalDate.SelectedDateTime = (DateTime)DateTime.Now.AddDays(dDiffDate);
                        dtCalDate.SelectedDateTime = (DateTime)DateTime.MaxValue.Date;
                        rdoExMonth.Visibility = Visibility.Visible; 
                        rdoNow.Visibility = Visibility.Visible;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            
            //dtpDateFrom.SelectedDateTime = (DateTime)DateTime.Now.AddDays(-7);
            //dtpDateTo.SelectedDateTime = (DateTime)DateTime.Now;
            //dtpDateFromHist.SelectedDateTime = (DateTime)DateTime.Now.AddDays(-7);
            //dtpDateToHist.SelectedDateTime = (DateTime)DateTime.Now;
        }
        #endregion

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dDefaultCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dDefaultCalDate = DateTime.Now;

                return dDefaultCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        #region [승인자 입력]
        private void btnGrator_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void GetUserWindow()
        {
            COM001_035_PACK_PERSON wndPerson = new COM001_035_PACK_PERSON();
            wndPerson.FrameOperation = this.FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = txtGratorLoss.Text;
                Parameters[1] = "APPROVER";
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                wndPerson.ShowModal();
                wndPerson.CenterOnScreen();
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            COM001_035_PACK_PERSON wndPerson = sender as COM001_035_PACK_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                DataTable dtTo = null;
                dtTo = DataTableConverter.Convert(dgGratorLoss.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("APPR_SEQS", typeof(string));
                    dtTo.Columns.Add("USERID", typeof(string));
                    dtTo.Columns.Add("USERNAME", typeof(string));
                    dtTo.Columns.Add("DEPTNAME", typeof(string));
                }

                if (dtTo.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
                {
                    Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                    return;
                }

                DataRow drFrom = dtTo.NewRow();
                drFrom["APPR_SEQS"] = dtTo.Rows.Count + 1;
                drFrom["USERID"] = wndPerson.USERID;
                drFrom["USERNAME"] = wndPerson.USERNAME;
                drFrom["DEPTNAME"] = wndPerson.DEPTNAME;
                dtTo.Rows.Add(drFrom);

                dgGratorLoss.ItemsSource = DataTableConverter.Convert(dtTo);

                txtGratorLoss.Text = string.Empty;
                txtGratorLoss.Tag = string.Empty;
            }
        }

        #endregion

        #region [참조자 입력]
        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNoticeLoss.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtTo = DataTableConverter.Convert(dgNoticeLoss.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);

                        dgNoticeLoss.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtNoticeLoss.Text = string.Empty;
                    }
                    else
                    {
                        dgNoticeSelectLoss.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelectLoss);

                        dgNoticeSelectLoss.ItemsSource = DataTableConverter.Convert(dtRslt);
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [추가(다운)/삭제(업) 버튼]

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            LossLotMove(MOVE_TYPE.DOWN);

        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            LossLotMove(MOVE_TYPE.UP);
        }

        private void LossLotMove(string sMoveArrow)
        {

            if (sMoveArrow == MOVE_TYPE.DOWN)
            {
                if (dgListLoss.ItemsSource == null) return;
                if (dgListLoss.GetRowCount() == 0) return;
            }
            else
            {
                if (dgRequestLoss.ItemsSource == null) return;
                if (dgRequestLoss.GetRowCount() == 0) return;
            }

            DataTable dtTarget = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dgRequestLoss.ItemsSource : dgListLoss.ItemsSource);
            DataTable dtSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dgListLoss.ItemsSource : dgRequestLoss.ItemsSource);
            DataRow newRow = null;

            if (dtTarget.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTarget.Columns.Add("CHK", typeof(Boolean));
                dtTarget.Columns.Add("LOTID", typeof(string));
                dtTarget.Columns.Add("WIPSTAT", typeof(string));
                dtTarget.Columns.Add("WIPSNAME", typeof(string));
                dtTarget.Columns.Add("WIPHOLD", typeof(string));
                dtTarget.Columns.Add("LOTSTAT", typeof(string));
                dtTarget.Columns.Add("BOXING_YN", typeof(string));
                dtTarget.Columns.Add("EQSGNAME", typeof(string));
                dtTarget.Columns.Add("PROCID", typeof(string));
                dtTarget.Columns.Add("PROCNAME", typeof(string));
                dtTarget.Columns.Add("EQPTNAME", typeof(string));
                dtTarget.Columns.Add("MODELDESC", typeof(string));
                dtTarget.Columns.Add("PRDTYPE", typeof(string));
                dtTarget.Columns.Add("PRODID", typeof(string));
                dtTarget.Columns.Add("BOXID", typeof(string));

            }

            for (int i = dtSource.Rows.Count; i > 0; i--)
            {
                if (string.Equals(dtSource.Rows[i - 1]["CHK"].ToString(), "True"))
                {
                    newRow = dtTarget.NewRow();
                    newRow["CHK"] = false;
                    newRow["LOTID"] = dtSource.Rows[i - 1]["LOTID"].ToString();
                    newRow["WIPSTAT"] = dtSource.Rows[i - 1]["WIPSTAT"].ToString();
                    newRow["WIPSNAME"] = dtSource.Rows[i - 1]["WIPSNAME"].ToString();
                    newRow["WIPHOLD"] = dtSource.Rows[i - 1]["WIPHOLD"].ToString();
                    newRow["LOTSTAT"] = dtSource.Rows[i - 1]["LOTSTAT"].ToString();
                    newRow["BOXING_YN"] = dtSource.Rows[i - 1]["BOXING_YN"].ToString();
                    newRow["EQSGNAME"] = dtSource.Rows[i - 1]["EQSGNAME"].ToString();
                    newRow["PROCID"] = dtSource.Rows[i - 1]["PROCID"].ToString();
                    newRow["PROCNAME"] = dtSource.Rows[i - 1]["PROCNAME"].ToString();
                    newRow["EQPTNAME"] = dtSource.Rows[i - 1]["EQPTNAME"].ToString();
                    newRow["MODELDESC"] = dtSource.Rows[i - 1]["MODELDESC"].ToString();
                    newRow["PRDTYPE"] = dtSource.Rows[i - 1]["PRDTYPE"].ToString();
                    newRow["PRODID"] = dtSource.Rows[i - 1]["PRODID"].ToString();
                    newRow["BOXID"] = dtSource.Rows[i - 1]["BOXID"].ToString();

                    dtTarget.Rows.Add(newRow);

                    dtSource.Rows[i - 1].Delete();
                }
            }

            dgRequestLoss.ItemsSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dtTarget : dtSource);
            dgListLoss.ItemsSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dtSource : dtTarget);

            chkAll_HOLD.IsChecked = false;
            chkAll_REQ.IsChecked = false;

        }
        #endregion

        #region [참조자 검색결과 여러개일경우]
        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = dtTo = DataTableConverter.Convert(dgNoticeLoss.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgNoticeLoss.ItemsSource = DataTableConverter.Convert(dtTo);
                dgNoticeSelectLoss.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);

            dgNoticeLoss.ItemsSource = DataTableConverter.Convert(dtTo);
            dgNoticeSelectLoss.Visibility = Visibility.Collapsed;

            txtNoticeLoss.Text = string.Empty;
        }
        #endregion

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelectLoss.Visibility = Visibility.Collapsed;
        }

        #region [제거 처리]
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회클릭]
        private void btnLossSearch_Click(object sender, RoutedEventArgs e)
        {
            // 2021.01.01 버튼 비활성화함.
            //GetLossLotList();
        }
        #endregion

        #region EVENT

        private void dgRequest_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgRequestLoss.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
            }));
        }
        #endregion

        #region [Loss요청클릭]
        private void btnLossReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (_bUSE_YN)
                //{
                //    Util.MessageValidation("SFU8339"); // ERP 생산실적이 마감 되었습니다. \n월초 부터 마감전까지 사용가능합니다.
                //    return;
                //}

                if (dgRequestLoss.GetRowCount() == 0)
                {
                    Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                    return;
                }

                if (dgGratorLoss.GetRowCount() == 0)
                {
                    Util.Alert("SFU1692");  //승인자가 필요합니다.
                    return;
                }

                if (Util.GetCondition(cboLossResnCode, "SFU1593") == "") //사유는필수입니다. >> 사유를 선택하세요.
                {
                    return;
                }

                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                LossRequest();
                            }
                        });


            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [진행중인 lot 색 변경]
        private void dgListHold_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //진행중인 색 변경
                if (e.Cell.Column.Name.Equals("REQ_ING_CNT"))
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_ING_CNT"));
                    if (sCheck.Equals("ING"))
                    {
                        foreach (C1.WPF.DataGrid.DataGridColumn dc in dataGrid.Columns)
                        {
                            if (dc.Visibility == Visibility.Visible)
                            {
                                if (dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter != null)
                                    dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            }
                        }


                        CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
                        cb.Visibility = Visibility.Hidden;
                    }
                }


            }));
        }
        #endregion

        #region Mehod

        public void GetLossLotList(string sLotList = "")
        {
            try
            {
                if (!_bERPclose_YN && !(bool)rdoExMonth.IsChecked && !(bool)rdoNow.IsChecked)
                {
                    Util.MessageInfo("SFU8498");
                    return;
                }

                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YMD", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                GetComSelCalDate();
                searchCondition["STCK_CNT_YMD"] = dDefaultCalDate.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(sLotList))
                {
                    searchCondition["LOTID"] = sLotList;
                }
                else if (!string.IsNullOrEmpty(Util.NVC(txtLossLot.Text)))
                {
                    searchCondition["LOTID"] = Util.NVC(txtLossLot.Text);
                }

                RQSTDT.Rows.Add(searchCondition);

                // GMES 마감 후에는 WIP 재공 내 포함되기만하면 조회 가능,
                // GMES 마감 전이라면 요청 월의 1일 snap에 정보가 존재 해야만 작업 가능 할수 있도록 조회
                if (_bERPclose_YN || !(bool)rdoExMonth.IsChecked)
                {
                    dtSearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_YIELD_REQ_LOT_SEARCH_V2", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    dtSearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_YIELD_REQ_LOT_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);
                }
                

                dgListLoss.ItemsSource = null;
                dgGratorLoss.ItemsSource = null;
                dgNoticeLoss.ItemsSource = null;
                dgRequestLoss.ItemsSource = null;

                if (dtSearchResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgListLoss, dtSearchResult, FrameOperation);

                    chkAll_HOLD.IsChecked = false;
                    chkAll_REQ.IsChecked = false;
                }
                else
                {
                    Util.AlertInfo("SFU2816"); // 조회 결과가 없습니다.
                    return;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void LossRequest()
        {
            string sTo = "";
            string sCC = "";

            string resnCode = "P" + Util.GetCondition(cboLossResnCode) + Util.GetCondition(cboCauseProc);

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("CAUSE_EQSGID", typeof(string));
            inDataTable.Columns.Add("CAUSE_PRODID", typeof(string));
            inDataTable.Columns.Add("UNHOLD_CALDATE", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = "LOT_SCRAP_YIELD";
            row["USERID"] = LoginInfo.USERID;
            row["REQ_NOTE"] = Util.GetCondition(txtLossNote);
            row["RESNCODE"] = resnCode;
            if (row["RESNCODE"].Equals("")) return;
            row["AREAID"] = LoginInfo.CFG_AREA_ID; //Modified By Jaeyoung Ko(2019.07.08) [CSR ID:4032652]

            string sCauseProc = cboCauseProc.SelectedValue.ToString();
            if ("00000".Equals(sCauseProc))
            {
                //처리없음
            }
            else
            {
                row["CAUSE_EQSGID"] = Util.GetCondition(cboCauseEqsg);
                row["CAUSE_PRODID"] = Util.GetCondition(txtCauseProd);
            }
            //C20190530_07166 : [CSR ID:4007166] GMES 전기일입력 / 전공정 Loss 사유 추가 및 구분자 변경 요청의 건
            //UNHOLD_CALDATE 는 LOT Release 시 전기일을 지정하는 컬럼이나 전공정 Loss 에서도 전기일을 지정하게 해달라는 요청이 발생해
            //기존에 사용하는 UNHOLD_CALDATE 사용함.
            row["UNHOLD_CALDATE"] = dtCalDate.SelectedDateTime.ToString("yyyy-MM-dd");

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            inLot.Columns.Add("WOID", typeof(string));

            for (int i = 0; i < dgRequestLoss.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "LOTID"));
                /*
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "REQQTY"));
                row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "REQQTY")) *
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "LANE_QTY")) *
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "LANE_PTN_QTY"));
                */
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "PRODID"));
                row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "MODELDESC"));
                inLot.Rows.Add(row);
            }

            //승인자
            DataTable inProg = inData.Tables.Add("INPROG");
            inProg.Columns.Add("APPR_SEQS", typeof(string));
            inProg.Columns.Add("APPR_USERID", typeof(string));

            for (int i = 0; i < dgGratorLoss.Rows.Count; i++)
            {
                row = inProg.NewRow();
                row["APPR_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgGratorLoss.Rows[i].DataItem, "APPR_SEQS"));
                row["APPR_USERID"] = Util.NVC(DataTableConverter.GetValue(dgGratorLoss.Rows[i].DataItem, "USERID"));
                inProg.Rows.Add(row);

                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(dgGratorLoss.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            for (int i = 0; i < dgNoticeLoss.Rows.Count; i++)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgNoticeLoss.Rows[i].DataItem, "USERID"));
                inRef.Rows.Add(row);

                sCC += Util.NVC(DataTableConverter.GetValue(dgNoticeLoss.Rows[i].DataItem, "USERID")) + ";";
            }

            try
            {
                // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);

                if (CommonVerify.HasTableInDataSet(ds) && CommonVerify.HasTableRow(ds.Tables["OUTDATA_LOT"]))
                {
                    this.Show_COM001_035_PACK_EXCEPTION_POPUP(ds.Tables["OUTDATA_LOT"]);
                    return;
                }

                if (ds.Tables["OUTDATA"].Rows.Count > 0)
                {
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = ds.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString();

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtLossNote), inLot));
                }
                Util.AlertInfo("SFU1747");  //요청되었습니다.

                rdoNow.IsChecked = false;
                rdoExMonth.IsChecked = false;

                SetLossClear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLossClear()
        {
            txtLossLot.Text = string.Empty;
            txtGratorLoss.Text = string.Empty;
            txtNoticeLoss.Text = string.Empty;

            chkAll_HOLD.IsChecked = false;
            chkAll_REQ.IsChecked = false;
            Util.gridClear(dgListLoss);
            Util.gridClear(dgRequestLoss);
            Util.gridClear(dgGratorLoss);
            Util.gridClear(dgNoticeLoss);
            Reset();

        }
        #endregion

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter preReq = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll_HOLD = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        CheckBox chkAll_REQ = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    preHold.Content = chkAll_HOLD;
                    e.Column.HeaderPresenter.Content = preHold;
                    chkAll_HOLD.Checked -= new RoutedEventHandler(chkAll_HOLD_Checked);
                    chkAll_HOLD.Unchecked -= new RoutedEventHandler(chkAll_HOLD_Unchecked);
                    chkAll_HOLD.Checked += new RoutedEventHandler(chkAll_HOLD_Checked);
                    chkAll_HOLD.Unchecked += new RoutedEventHandler(chkAll_HOLD_Unchecked);
                }
            }
        }

        private void dgRequest_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    preReq.Content = chkAll_REQ;
                    e.Column.HeaderPresenter.Content = preReq;
                    chkAll_REQ.Checked -= new RoutedEventHandler(chkAll_REQ_Checked);
                    chkAll_REQ.Unchecked -= new RoutedEventHandler(chkAll_REQ_Unchecked);
                    chkAll_REQ.Checked += new RoutedEventHandler(chkAll_REQ_Checked);
                    chkAll_REQ.Unchecked += new RoutedEventHandler(chkAll_REQ_Unchecked);
                }
            }
        }

        private void chkAll_HOLD_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListLoss.ItemsSource == null) return;

            int idgcount = DataTableConverter.Convert(dgListLoss.ItemsSource).Rows.Count;

            // 홀드상태의 경우에도 작업 가능하도록 변경 요청
            for (int i = 0; i < idgcount; i++)
            {    
                if (!(DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "WIPSTAT").Equals("TERM") 
                      || DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "WIPSTAT").Equals("MOVING")
                      || DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "WIPSTAT").Equals("BIZWF"))
                    //&& DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "WIPHOLD").Equals("N")
                    && DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "BOXING_YN").Equals("N")
                    && !(DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "PROCID").Equals("PB000") 
                       || DataTableConverter.GetValue(dgListLoss.Rows[i].DataItem, "PROCID").Equals("PD000"))
                   )
                {
                    DataTableConverter.SetValue(dgListLoss.Rows[i].DataItem, "CHK", true);
                }
            }
            
        }
        private void chkAll_REQ_Checked(object sender, RoutedEventArgs e)
        {
            if (dgRequestLoss.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRequestLoss.ItemsSource).Table;

            dt.Select("CHK = false").ToList<DataRow>().ForEach(r => r["CHK"] = true);
            dt.AcceptChanges();
        }
        private void chkAll_HOLD_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListLoss.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListLoss.ItemsSource).Table;

            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

        private void chkAll_REQ_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgRequestLoss.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRequestLoss.ItemsSource).Table;

            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            SetLossClear();
            rdoNow.IsChecked = false;
            rdoExMonth.IsChecked = false;
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLossLot.Text.Trim() == string.Empty)
                        return;
                    GetLossLotList();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    int maxLOTIDCount = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? 500 : 100;
                    string messageID = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? "SFU8217" : "SFU3695";
                    if (sPasteStrings.Count() > maxLOTIDCount)
                    {
                        if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK))
                        {
                            Util.MessageValidation(messageID, 500);     // 최대 500개 까지 가능합니다.
                        }
                        else
                        {
                            Util.MessageValidation(messageID);          // 최대 100개 까지 가능합니다.
                        }
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }
                    GetLossLotList(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }
        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        // 사용안함, 전기일은 마감월의 마지막날로 고정.
        /*
        private void dtCalDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DATE"] = dtCalDate.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                dtCalDate.SelectedDateTime = dDefaultCalDate;
                dtCalDate.Focus();
            }
        }ds
        */

        private void btnCauseProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRequestLoss.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU4940"); // 요청내용이 없습니다.
                    return;
                }

                string sProdid = string.Empty;

                for (int i = 0; i < dgRequestLoss.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "CHK").Equals(true) ||
                        DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "CHK").ToString() == "True" ||
                        DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "CHK").ToString() == "1")
                    {
                        sProdid = DataTableConverter.GetValue(dgRequestLoss.Rows[i].DataItem, "PRODID").ToString();
                    }
                }

                COM001_035_PRODTREE wndPopup = new COM001_035_PRODTREE();
                wndPopup.FrameOperation = this.FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = LoginInfo.CFG_SHOP_ID;
                    Parameters[1] = sProdid;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(popup_PRODTREE_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                    wndPopup.BringToFront();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        void popup_PRODTREE_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_035_PRODTREE popup = sender as COM001_035_PRODTREE;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtCauseProd.Text = popup.PRODID;
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void cboCauseProc_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sLossResnCode = cboLossResnCode.SelectedValue.ToString();
                if (string.IsNullOrEmpty(sLossResnCode) || "SELECT".Equals(sLossResnCode))
                {
                    return;
                }

                string sCauseProc = cboCauseProc.SelectedValue.ToString();
                if (string.IsNullOrEmpty(sCauseProc) || "00000".Equals(sCauseProc))
                {
                    //btnCauseProd.IsEnabled = false;
                    txtCauseProd.Text = string.Empty;
                    cboCauseEqsg.IsEnabled = false;

                    return;
                }
                else
                {
                    //btnCauseProd.IsEnabled = true;
                    cboCauseEqsg.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgdgListLoss_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgListLoss.CurrentRow == null || dgListLoss.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgListLoss.CurrentColumn.Name;
                string sChkValue = string.Empty;
                string sWIPSTAT = string.Empty;
                string sWIPHOLD = string.Empty;
                string sBOXING_YN = string.Empty;
                string sPROCID = string.Empty;


                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgListLoss.CurrentRow.Index;
                    int indexColumn = dgListLoss.CurrentColumn.Index;


                    sChkValue = Util.NVC(dgListLoss.GetCell(indexRow, indexColumn).Value);

                    sWIPSTAT = Util.NVC(dgListLoss.GetCell(indexRow, dgListLoss.Columns["WIPSTAT"].Index).Value);
                    sWIPHOLD = Util.NVC(dgListLoss.GetCell(indexRow, dgListLoss.Columns["WIPHOLD"].Index).Value);
                    sBOXING_YN = Util.NVC(dgListLoss.GetCell(indexRow, dgListLoss.Columns["BOXING_YN"].Index).Value);
                    sPROCID = Util.NVC(dgListLoss.GetCell(indexRow, dgListLoss.Columns["PROCID"].Index).Value);

                    if (string.IsNullOrEmpty(sChkValue) || sChkValue.Equals(""))
                        return;

                    // 홀드상태의 경우에도 작업 가능하도록 변경 요청
                    if (!(sWIPSTAT.Equals("TERM") || sWIPSTAT.Equals("MOVING") || sWIPSTAT.Equals("BIZWF")) &&
                         //sWIPHOLD.Equals("N") &&
                         sBOXING_YN.Equals("N") &&
                         !(sPROCID.Equals("PB000") || sPROCID.Equals("PD000"))
                       )
                    {
                        if (sChkValue == "0" || sChkValue == "False")
                        {
                            DataTableConverter.SetValue(dgListLoss.Rows[indexRow].DataItem, sColName, true);
                        }
                        else if (sChkValue == "1" || sChkValue == "True")
                        {
                            DataTableConverter.SetValue(dgListLoss.Rows[indexRow].DataItem, sColName, false);
                        }
                    }
                    else
                    {
                        Util.Alert("SFU1628"); // SFU1628 : 선택할수 없습니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Reset()
        {
            try
            {
                ChkErpCloseYn();

                if (_bERPclose_YN)
                {
                    dtCalDate.SelectedDateTime = (DateTime)System.DateTime.Now;
                    rdoExMonth.Visibility = Visibility.Collapsed;
                    rdoNow.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //dtCalDate.SelectedDateTime = (DateTime)DateTime.Now.AddDays(dDiffDate);
                    rdoExMonth.Visibility = Visibility.Visible;
                    rdoNow.Visibility = Visibility.Visible;
                    dtCalDate.SelectedDateTime = (DateTime)DateTime.MaxValue.Date;
                }

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChkErpCloseYn()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_END_INFO", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count != 0)
                {
                    _bERPclose_YN = Util.NVC(dtRslt.Rows[0]["CLOSE_INFO"].ToString()).Equals("Y") ? true : false; 
                    dDiffDate = Util.StringToDouble(dtRslt.Rows[0]["DIFF_DAY"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoExMonth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtCalDate.SelectedDateTime = (DateTime)DateTime.Now.AddDays(dDiffDate);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoNow_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtCalDate.SelectedDateTime = (DateTime)System.DateTime.Now;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 불건전 Data 표출 Popup Open
        private void Show_COM001_035_PACK_EXCEPTION_POPUP(DataTable dt)
        {
            COM001_035_PACK_EXCEPTION_POPUP wndPopUp = new COM001_035_PACK_EXCEPTION_POPUP();
            wndPopUp.FrameOperation = FrameOperation;

            if (wndPopUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = "APPR_BIZ";

                C1WindowExtension.SetParameters(wndPopUp, Parameters);
                wndPopUp.ShowModal();
                wndPopUp.CenterOnScreen();
                wndPopUp.BringToFront();
            }
        }
    }    
}

