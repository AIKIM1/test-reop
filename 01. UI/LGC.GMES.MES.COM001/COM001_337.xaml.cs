/*************************************************************************************
 Created Date : 2020.10.21
      Creator : 안인효
   Decription : Ink Marking 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.21  안인효 : Initial Created.
  2020.11.27  안인효 : Marking 관련 수정  
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
using System.Windows.Threading;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_337 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        DataTable dta;

        DataRow _drSelectRow;

        #region Parameter

        private string _LotId = string.Empty;
        private string _AreaID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;
        private string _PrjtName = string.Empty;
        private string _PrdtClssCode = string.Empty;
        private string _FromDate = string.Empty;
        private string _ToDate = string.Empty;

        #endregion

        int iCheck = 0;
        int iRowMark = 0;
        int iColMark = 0;
        private const int const_iMaxColCount = 50;      // MAP의 1라인 컬럼수 설정
        int iInQty = 0;     // 생산량(투입량) - MAP 그릴때 사용

        public COM001_337()
        {
            InitializeComponent();
            //InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Initialize

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

        }

        #endregion


        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //사용자 권한별로 버튼 숨기기
                List<Button> listAuth = new List<Button>();

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //여기까지 사용자 권한별로 버튼 숨기기

                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps.Length >= 9)
                {
                    _LotId = tmps[0].ToString();
                    _AreaID = tmps[1].ToString();
                    _EqsgID = tmps[2].ToString();
                    _ProcID = tmps[3].ToString();
                    _EqptID = tmps[4].ToString();
                    _PrjtName = tmps[5].ToString();
                    _PrdtClssCode = tmps[6].ToString();
                    _FromDate = tmps[7].ToString();
                    _ToDate = tmps[8].ToString();
                }

                InitCombo();

                cboEquipmentSegment.SelectedValue = _EqsgID;
                cboProcess.SelectedValue = _ProcID;
                cboEquipment.SelectedValue = _EqptID;
                txtPrjtName.Text = _PrjtName;
                txtLotId.Text = _LotId;
                cboElecType.SelectedValue = _PrdtClssCode;

                //dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                dtpDateFrom.SelectedDateTime = Convert.ToDateTime(_FromDate);
                dtpDateTo.SelectedDateTime = Convert.ToDateTime(_ToDate);

                btnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgLotList);
                Util.gridClear(dgNGmap);

                for (int i = dgNGmap.Rows.Count; i > 0; i--)
                {
                    dgNGmap.BottomRows.RemoveAt(i - 1);
                }

                for (int i = dgNGmap.Columns.Count; i > 0; i--)
                {
                    dgNGmap.Columns.RemoveAt(i - 1);
                }

                txtNgCount.Text = "";
                txtRollAmount.Text = "";
                txtPrjtName.Text = "";
                cboElecType.SelectedIndex = 0;
                iInQty = 0;

                if (!ValidationSearch())
                    return;

                GetLotList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }
        #endregion

        #region [작업일] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "31");

                //    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                //    //dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    if (LGCdp.Name.Equals("dtpDateTo"))
                //        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                //    else
                //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                //    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

                //// To 일자 변경시 From일자 1일자로 변경
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
        }
        #endregion

        #region # 조회조건 : Lot KeyDown
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    btnSearch_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 팝업 닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 그리드 셀 처리 (맵 그리드)
        private void dgNGmap_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Text != "")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                        e.Cell.Presenter.VerticalContentAlignment = VerticalAlignment.Top;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region 맵 그리드 로우 NUMBER
        private void dgNGmap_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                //_Util.gridLoadedRowHeaderPresenter(sender, e);

                if (e.Row.HeaderPresenter == null)
                {
                    return;
                }

                e.Row.HeaderPresenter.Content = null;

                TextBlock tb = new TextBlock();
                tb.Text = (e.Row.Index + 1 - dgNGmap.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        } 
        #endregion

        #endregion


        #region Mehod

        #region [BizCall]

        #region [### 조회 ###]
        public void GetList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                // 동을 선택하세요.
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                // 라인을선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals("")) return;

                // 공정을선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                // 설비를 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, MessageDic.Instance.GetMessage("SFU1673"));
                if (dr["EQPTID"].Equals("")) return;

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_NOTE_LIST", "INDATA", "OUTDATA", dtRqst);

               // Util.GridSetData(dgNote, dtRslt, FrameOperation, true);

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

        #region # NG Lot 리스트 조회
        /// <summary>
        /// 생산 Lot 리스트 조회
        /// </summary>
        public void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                dr["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                //dr["PRDT_CLSS_CODE"] = cboElecType.SelectedValue.ToString() == "" || cboElecType.SelectedValue.ToString() == "SELECT" ? null : cboElecType.SelectedValue.ToString();

                if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                    dr["LOTID"] = txtLotId.Text;
                else if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;

                bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_DFCT_DATA_CLCT";
                //InitUsrControl();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLotList, bizResult, FrameOperation, true);


                    #region 맵 관련
                    if (bizResult.Rows.Count > 0)
                    {
                        DataTable dtable = new DataTable();
                        DataColumn dcol;
                        DataRow drow;

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            iInQty = Convert.ToInt32(bizResult.Rows[0]["IN_QTY"]);
                        }

                        int iRowCount = (iInQty % const_iMaxColCount) == 0 ? iInQty / const_iMaxColCount : (iInQty / const_iMaxColCount) + 1;
                        int iTotal = 0;
                        for (int r = 0; r < iRowCount; r++)
                        {
                            for (int c = 0; c < const_iMaxColCount; c++)
                            {
                                ++iTotal;
                                dcol = new DataColumn();
                                dcol.ColumnName = ((r * const_iMaxColCount) + (c + 1)).ToString();
                                dtable.Columns.Add(dcol.ToString(), typeof(string));
                                if (iTotal == iInQty)
                                {
                                    break;
                                }
                            }
                            C1.WPF.DataGrid.DataGridRow rowGrid = new C1.WPF.DataGrid.DataGridRow();
                            drow = dtable.NewRow();
                            dtable.Rows.Add(drow);
                        }

                        // 불량 위치 marking
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            iRowMark = ((Convert.ToInt32(bizResult.Rows[i]["DFCT_LOCATION"]) % const_iMaxColCount) == 0) ? (Convert.ToInt32(bizResult.Rows[i]["DFCT_LOCATION"]) / const_iMaxColCount) - 1 : Convert.ToInt32(bizResult.Rows[i]["DFCT_LOCATION"]) / const_iMaxColCount;
                            iColMark = (Convert.ToInt32(bizResult.Rows[i]["DFCT_LOCATION"]) - (iRowMark * const_iMaxColCount)) - 1;
                            dtable.Rows[iRowMark][iColMark] = bizResult.Rows[i]["DFCT_LOCATION"].ToString();
                        }

                        Util.GridSetData(dgNGmap, dtable, null, false);

                        dgNGmap.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(0.00001, C1.WPF.DataGrid.DataGridUnitType.Star);
                        dgNGmap.RowHeight = new C1.WPF.DataGrid.DataGridLength(10, C1.WPF.DataGrid.DataGridUnitType.Star);
                        dgNGmap.Columns[0].CanUserFilter = false;

                        for (int gc = 0; gc < const_iMaxColCount - 1; gc++)
                        {
                            dgNGmap.Columns[gc].CanUserFilter = false;
                            dgNGmap.Columns[gc].CanUserSort = false;
                        }

                        for (int cd = const_iMaxColCount; cd < dgNGmap.Columns.Count; cd++)
                        {
                            dgNGmap.Columns[cd].Visibility = Visibility.Hidden;
                        }

                        txtNgCount.Text = string.Format("{0:#,###}", bizResult.Rows.Count.ToString());

                        txtRollAmount.Text = Convert.ToString(bizResult.Rows[0]["IN_QTY"]);
                        txtPrjtName.Text = Convert.ToString(bizResult.Rows[0]["PRJT_NAME"]);
                        cboElecType.SelectedValue = Convert.ToString(bizResult.Rows[0]["PRDT_CLSS_CODE"]);
                    }
                    #endregion
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool ValidationSearch(bool isLot = false)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (!isLot)
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 동을선택하세요
                    Util.MessageValidation("SFU1499");
                    return false;
                }
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrEmpty(txtLotId.Text.Trim()))
            {
                // LOTID를 입력하세요
                Util.MessageValidation("SFU2839");
                return false;
            }

            return true;
        }

        private bool ValidationPopupQuality()
        {
            if (string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]
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

        #endregion

        #endregion
    }
}