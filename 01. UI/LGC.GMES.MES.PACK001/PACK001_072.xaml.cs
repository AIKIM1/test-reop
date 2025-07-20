/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 

--------------------------------------------------------------------------------------
 [Change History]
 2025.04.04   김영택    NERP 대응    NERP 회계마감 진행중 관련 처리 (차수 추가, 차수마감 취소 불가 등) 
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_072 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        DataTable dtCompareDetail = new DataTable();
        DataTable dtCompareNotRsltDetail = new DataTable();
        DataTable dtCompareNotSnapDetail = new DataTable();

        DataTable dtDiffDetail = new DataTable();
        DataTable dtDiffNotRsltDetail = new DataTable();
        DataTable dtDiffNotSnapDetail = new DataTable();

        public PACK001_072()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();

        private string _sEqsgID = string.Empty;
        private string _sProcID = string.Empty;
        private string _sProdID = string.Empty;
        private string _sElecType = string.Empty;
        private string _sPrjtName = string.Empty;
        private string _sAutoWhStckFlag = string.Empty;
        private string _sStckAdjFlag = string.Empty;
        private string _sStckAdjDiffFlag = string.Empty;

        // NERP 대응 관련 변수 추가 (2025.04.04)
        private string _sNerpCloseFlag = string.Empty;  // NERP 회계 마감 체크
        private string _sMaxSeq = string.Empty;  // 재고실사 년월, 동에 따른 마지막 차수 조회
        private string _sMaxSeqCmplFlag = string.Empty;  // 재고실사 년월, 동에 따른 마지막 차수 마감상태 조회
        private string _sNerpApplyFlag = string.Empty;  //NERP 적용 여부 조회

        private const string _sLOTID = "LOTID";
        private const string _sBOXID = "BOXID";

        DataView _dvSTCKCNT { get; set; }

        string _sSTCK_CNT_CMPL_FLAG = string.Empty;

        #endregion

        #region Initialize
        private void InitCombo()
        {
            // 전산재고 탭
            // 동
            C1ComboBox[] cboAreaShotChild = { cboStockSeqShot, cboEqsgShot };
            string[] sFiltercboArea = { Area_Type.PACK };
            _combo.SetCombo(cboAreaShot, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_PACK", cbChild: cboAreaShotChild);

            // 라인
            C1ComboBox[] cboEqsgShotParent = { cboAreaShot };
            C1ComboBox[] cboEqsgShotChild = { cboModelShot };
            _combo.SetCombo(cboEqsgShot, CommonCombo.ComboStatus.ALL, cbParent: cboEqsgShotParent, cbChild: cboEqsgShotChild, sCase: "cboEquipmentSegment");

            // PRJT
            C1ComboBox[] cboModelShotParent = { cboAreaShot, cboEqsgShot };
            _combo.SetCombo(cboModelShot, CommonCombo.ComboStatus.ALL, cbParent: cboModelShotParent, sCase: "cboPRJModelPack");

            // 제품 구분
            string[] sFilterBizType = { "", LoginInfo.CFG_AREA_ID, "", Area_Type.PACK, "" };
            _combo.SetCombo(cboSnapTypeShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterBizType, sCase: "cboPrdtClassByProcId");


            // SNAP SEQ
            object[] objStockSeqShotParent = { cboAreaShot, ldpMonthShot };
            string[] sFilterAll = { "" };
            _combo.SetComboObjParent(cboStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterAll);

            // 재고실사 제외 여부
            string[] sFilterExclFlag = { "", "STCK_CNT_EXCL_FLAG" };
            _combo.SetCombo(cboExclFlagShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterExclFlag, sCase: "COMMCODES");

            if (cboExclFlagShot.Items.Count > 0) cboExclFlagShot.SelectedIndex = 1;

            // 실물재고조사 탭
            C1ComboBox[] cboAreaRsltChild = { cboStockSeqUpload };
            string[] sFiltercboAreaRslt = { Area_Type.PACK };
            _combo.SetCombo(cboRsltArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboAreaRslt, sCase: "AREA_PACK", cbChild: cboAreaRsltChild);

            // 라인
            C1ComboBox[] cboRsltEqsgParent = { cboRsltArea };
            C1ComboBox[] cboRsltEqsgChild = { cboRsltModel };
            _combo.SetCombo(cboRsltEqsg, CommonCombo.ComboStatus.ALL, cbParent: cboRsltEqsgParent, cbChild: cboRsltEqsgChild, sCase: "cboEquipmentSegment");

            // PRJT
            C1ComboBox[] cboRsltModelParent = { cboRsltArea, cboRsltEqsg };
            _combo.SetCombo(cboRsltModel, CommonCombo.ComboStatus.ALL, cbParent: cboRsltModelParent, sCase: "cboPRJModelPack");

            // 제품 구분 
            string[] sFilterRsltBizType = { "", LoginInfo.CFG_AREA_ID, "", Area_Type.PACK, "" };
            _combo.SetCombo(cboRsltTypeRslt, CommonCombo.ComboStatus.ALL, sFilter: sFilterRsltBizType, sCase: "cboPrdtClassByProcId");

            object[] cboStockSeqUploadParent = { cboRsltArea, ldpMonthUpload };
            string[] sFilterAllUpload = { "" };
            _combo.SetComboObjParent(cboStockSeqUpload, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboStockSeqUploadParent, sFilter: sFilterAllUpload);

            // 재고비교 탭
            C1ComboBox[] cboCompareAreaChild = { cboStockSeqCompare };
            string[] sFiltercboCompareArea = { Area_Type.PACK };
            _combo.SetCombo(cboCompareArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboCompareArea, sCase: "AREA_PACK", cbChild: cboCompareAreaChild);

            // PRJT
            C1ComboBox[] cboCompareModelParent = { cboCompareArea };
            _combo.SetCombo(cboCompareModel, CommonCombo.ComboStatus.ALL, cbParent: cboCompareModelParent, sCase: "cboPRJModelPack");

            // 제품 타입
            string[] sFilterCompareBizType = { "", LoginInfo.CFG_AREA_ID, "", Area_Type.PACK, "" };
            _combo.SetCombo(cboCompareTypeShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterCompareBizType, sCase: "cboPrdtClassByProcId");

            object[] cboCompareSeqUploadParent = { cboCompareArea, ldpMonthCompare };
            string[] sFilterAllCompare = { "" };
            _combo.SetComboObjParent(cboStockSeqCompare, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboCompareSeqUploadParent, sFilter: sFilterAllCompare);

            if (!makeDetailTypeCombo(ref cboDetailTypeCompare))
            {
                cboDetailTypeCompare.Visibility = Visibility.Collapsed;
            }

            // 실사차사유 탭
            C1ComboBox[] cboDiffAreaChild = { cboStockSeqDiff };
            string[] sFiltercboDiffArea = { Area_Type.PACK };
            _combo.SetCombo(this.cboDiffArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboDiffArea, sCase: "AREA_PACK", cbChild: cboDiffAreaChild);

            // PRJT
            C1ComboBox[] cboDiffModelParent = { cboDiffArea };
            _combo.SetCombo(cboDiffModel, CommonCombo.ComboStatus.ALL, cbParent: cboDiffModelParent, sCase: "cboPRJModelPack");

            // 제품 타입
            string[] sFilterDiffBizType = { "", LoginInfo.CFG_AREA_ID, "", Area_Type.PACK, "" };
            _combo.SetCombo(cboDiffTypeShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterDiffBizType, sCase: "cboPrdtClassByProcId");

            object[] cboDiffSeqUploadParent = { cboDiffArea, ldpMonthDiff };
            string[] sFilterDiffCompare = { "" };
            _combo.SetComboObjParent(cboStockSeqDiff, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboDiffSeqUploadParent, sFilter: sFilterDiffCompare);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnExclude_SNAP);
            listAuth.Add(btnExclude_RSLT);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            SetExcelButton();
            // 여기까지 사용자 권한별로 버튼 숨기기

            // 전산재고 Tab
            // SetEqsgCombo(mboEqsgShot, cboAreaShot);
            // SetModelMultiCombo(mboModelShot, mboEqsgShot, cboAreaShot);
            SetWhId(mboSnapWhId, cboAreaShot);
            SetRackId(mboSnapRackId, mboSnapWhId, cboAreaShot);

            // 실물재고실사 Tab
            // SetEqsgCombo(mboEqsgRslt, cboAreaRslt);
            // SetModelMultiCombo(mboModelRslt, mboEqsgRslt, cboAreaRslt);
            SetWhId(mboRsltWhId, cboRsltArea);
            SetRackId(mboRsltRackId, mboRsltWhId, cboRsltArea);
            this.Loaded -= UserControl_Loaded;

            // NERP 적용 여부 
            ChkNerpApplyFlag();

            // 차수마감/취소 버튼 제어
            ShowBtnCloseCancel(ldpMonthShot);
        }
        #endregion

        #region Event

        #region 차수마감
        private void btnDegreeClose_Click(object sender, RoutedEventArgs e)
        {
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); // 마감된 차수입니다.
                return;
            }

            // 마감하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeClose();                  // 차수마감 Transaction
                    this.RegStockCountDiff("Y", null);       // 전산재고, 실사재고 차이 저장

                    _combo.SetCombo(cboStockSeqShot);
                    _combo.SetCombo(cboStockSeqUpload);
                    _combo.SetCombo(cboStockSeqCompare);
                    _combo.SetCombo(this.cboStockSeqDiff);
                }
            }
            );
        }
        #endregion

        #region 차수추가
        private void btnDegreeAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NERP 대응 추가 (2025.04.04)
                ChkNerpApplyFlag();
                ChkNerpFlag(ldpMonthShot);

                string[] sAttrbute = { "Y" };

                if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sNerpCloseFlag.Equals("N"))
                {
                    Util.MessageValidation("SFU3686");  // NERP 회계 마감기간 중 차수 추가를 할 수 없습니다.
                    return;
                }


                PACK001_072_STOCKCNT_START wndSTOCKCNT_START = new PACK001_072_STOCKCNT_START();
                wndSTOCKCNT_START.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_START != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = Convert.ToString(cboAreaShot.SelectedValue);
                    Parameters[1] = ldpMonthShot.SelectedDateTime;
                    Parameters[2] = Util.GetCondition(cboStockSeqShot);

                    C1WindowExtension.SetParameters(wndSTOCKCNT_START, Parameters);

                    wndSTOCKCNT_START.Closed += new EventHandler(wndSTOCKCNT_START_Closed);

                    //  팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_START.ShowModal()));
                    wndSTOCKCNT_START.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndSTOCKCNT_START_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_072_STOCKCNT_START window = sender as PACK001_072_STOCKCNT_START;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    _combo.SetCombo(cboStockSeqShot);
                    _combo.SetCombo(cboStockSeqUpload);
                    _combo.SetCombo(cboStockSeqCompare);
                    _combo.SetCombo(cboStockSeqDiff);

                    Util.gridClear(dgListShot);
                    Util.gridClear(dgListStock);
                    Util.gridClear(dgListCompare);
                    Util.gridClear(dgListCompareDetail);
                    Util.gridClear(dgListDiff);

                    SetListShot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 조회 Series
        // 전산재고 조회
        private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        {
            SetListShot();
        }

        // 재고조사 조회
        private void btnSearchStock_Click(object sender, RoutedEventArgs e)
        {
            SetListRslt();
        }

        // 재고비교 조회
        private void btnSearchCompare_Click(object sender, RoutedEventArgs e)
        {
            _sStckAdjFlag = chkStckAdjFlagCompare.IsChecked == true ? "Y" : "N";

            // Summary 조회
            SetListCompare();
        }

        // 실사차 사유 조회
        private void btnSearchDiff_Click(object sender, RoutedEventArgs e)
        {
            this.SetListDiff();
        }
        #endregion

        #region 기준월 변경
        private void ldpMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboStockSeqShot);
            DataTable dt = DataTableConverter.Convert(cboStockSeqShot.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                snapNote.Text = "";
                return;
            }

            snapNote.Text = dt.Rows[0]["STCK_CNT_NOTE"].ToString();
        }

        private void ldpMonthUpload_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboStockSeqUpload);
            DataTable dt = DataTableConverter.Convert(cboStockSeqUpload.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                rsltNote.Text = "";
                return;
            }

            rsltNote.Text = dt.Rows[0]["STCK_CNT_NOTE"].ToString();
        }

        private void ldpMonthCompare_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboStockSeqCompare);

            DataTable dt = DataTableConverter.Convert(cboStockSeqCompare.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                summaryCompareNote.Text = "";
                return;
            }

            summaryCompareNote.Text = dt.Rows[cboStockSeqCompare.SelectedIndex]["STCK_CNT_NOTE"].ToString();
        }

        private void ldpMonthDiff_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            _combo.SetCombo(this.cboStockSeqDiff);

            DataTable dt = DataTableConverter.Convert(cboStockSeqDiff.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                this.summaryDiffNote.Text = "";
                return;
            }

            summaryDiffNote.Text = dt.Rows[cboStockSeqDiff.SelectedIndex]["STCK_CNT_NOTE"].ToString();
        }

        #endregion

        #region 비교 화면 체크시
        private void dgListCompareChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtCompareDetail = new DataTable();

                RadioButton rb = sender as RadioButton;

                // 최초 체크시에만 로직 타도록 구현

                Int64 iStckCntSeqNo = Util.NVC_Int(DataTableConverter.GetValue(rb.DataContext, "STCK_CNT_SEQNO"));
                string strStckCntYM = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "STCK_CNT_YM"));
                string strAreaId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "AREAID"));
                string strProdId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID"));
                string strProdType = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRDTYPE"));
                string strPrjtName = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRJT_NAME"));
                string strModeId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MODELID"));

                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                    RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                    RQSTDT.Columns.Add("PRJT", typeof(string));
                    RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                    RQSTDT.Columns.Add("FINL_WIP_FLAG", typeof(string));


                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["STCK_CNT_SEQNO"] = iStckCntSeqNo;
                    dr["STCK_CNT_YM"] = strStckCntYM;
                    dr["AREAID"] = strAreaId;
                    dr["PRODID"] = strProdId;
                    dr["PRDTYPE"] = strProdType;
                    dr["PRJT"] = strModeId;
                    dr["PRJT_NAME"] = strPrjtName;
                    dr["FINL_WIP_FLAG"] = chkFinlwipCompare.IsChecked == true ? "Y" : null;

                    RQSTDT.Rows.Add(dr);

                    dtCompareDetail = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_CNT_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);


                    if (dtCompareDetail != null && dtCompareDetail.Rows.Count > 0)
                    {
                        DataRow[] drCompareNotRsltDetail = null;
                        DataRow[] drCompareNotSnapDetail = null;

                        drCompareNotRsltDetail = dtCompareDetail.Select("RSLT_LOTID IS NOT NULL AND SNAP_LOTID IS NULL");
                        drCompareNotSnapDetail = dtCompareDetail.Select("SNAP_LOTID IS NOT NULL AND  RSLT_LOTID IS NULL AND INPUT_YN <> 'Y'");

                        if (drCompareNotRsltDetail != null && drCompareNotRsltDetail.Length > 0)
                        {
                            dtCompareNotRsltDetail = drCompareNotRsltDetail.CopyToDataTable();
                        }
                        else
                        {
                            dtCompareNotRsltDetail = new DataTable();
                        }

                        if (drCompareNotSnapDetail != null && drCompareNotSnapDetail.Length > 0)
                        {
                            dtCompareNotSnapDetail = drCompareNotSnapDetail.CopyToDataTable();
                        }
                        else
                        {
                            dtCompareNotSnapDetail = new DataTable();
                        }
                    }
                    else
                    {
                        dtCompareNotRsltDetail = new DataTable();
                        dtCompareNotSnapDetail = new DataTable();
                    }

                    string strDetailType = Util.NVC(cboDetailTypeCompare.SelectedValue);

                    if (string.IsNullOrEmpty(strDetailType))
                    {
                        Util.GridSetData(dgListCompareDetail, dtCompareDetail, FrameOperation);
                    }
                    else
                    {
                        if (strDetailType == "NOT_SNAP")
                        {
                            Util.GridSetData(dgListCompareDetail, dtCompareNotSnapDetail, FrameOperation);
                        }
                        else if (strDetailType == "NOT_RSLT")
                        {
                            Util.GridSetData(dgListCompareDetail, dtCompareNotRsltDetail, FrameOperation);
                        }
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 재고비교 컬러설정
        private void dgListCompare_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    // link 색변경
                    // if (e.Cell.Column.Name.Equals("PRODID"))
                    // {
                    //     e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    // }
                    // else
                    // {
                    //     e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    // }

                    // 틀린색변경
                    if (e.Cell.Column.Name.Equals("SNAP_CNT"))
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));
                        if (e.Cell.Presenter != null && sCheck.Equals("NG"))
                        {
                            // e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);

                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STCK_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            // e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STCK_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListCompareDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        // 전산재고와 실물 수량이 맞지않으면 Yellow
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));

                        if (e.Cell.Presenter != null && sCheck.Equals("NG"))
                        {
                            string[] Col = { "SNAP_LOTID", "REAL_LOTID", "SNAP_QTY", "REAL_QTY", "MOVE_ORD_STAT", "REAL_STCK_CNT_DTTM", "SNAP_ASSY_LOT", "REAL_ASSY_LOT", "REAL_RACK_ID", "VLD_DATE" };
                            foreach (string column in Col)
                            {
                                if (column == e.Cell.Column.Name)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    break;
                                }
                            }

                            // 실물만 있는 LOTID면 Red
                            DataRowView dr = (DataRowView)e.Cell.Row.DataItem;
                            string sSNAP_LOTID = Util.NVC(dr.Row["SNAP_LOTID"]);
                            string sREAL_LOTID = Util.NVC(dr.Row["REAL_LOTID"]);

                            if (String.IsNullOrEmpty(sSNAP_LOTID) && !String.IsNullOrEmpty(sREAL_LOTID))
                            {
                                string[] Col1 = { "REAL_LOTID", "REAL_QTY", "REAL_STCK_CNT_DTTM", "REAL_ASSY_LOT", "REAL_RACK_ID", "VLD_DATE" };
                                foreach (string column1 in Col1)
                                {
                                    if (column1 == e.Cell.Column.Name)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                                        break;
                                    }
                                }
                            }

                            // 재고비교 상세에 전산 재고만 있고 재고실사가 않된 항목에 대하여는 붉은색(BOLD)로 표시
                            if (e.Cell.Column.Name.Equals("SNAP_LOTID") || e.Cell.Column.Name.Equals("SNAP_ASSY_LOT") || e.Cell.Column.Name.Equals("SNAP_QTY") || e.Cell.Column.Name.Equals("MOVE_ORD_STAT"))
                            {
                                string sNEXT_WIPSTAT = Util.NVC(dr.Row["NEXT_WIPSTAT"]);
                                if (string.IsNullOrEmpty(sNEXT_WIPSTAT) && string.IsNullOrEmpty(sREAL_LOTID))
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                }
                                else
                                {
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                    e.Cell.Presenter.Foreground = dgListCompareDetail.Foreground;
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Foreground = dgListCompareDetail.Foreground;
                            }
                        }
                        else
                        {
                            if (e.Cell.Presenter != null)
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Foreground = dgListCompareDetail.Foreground;
                            }
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListCompareDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = dgListCompareDetail.Foreground;
                    }
                }
            }));
        }

        private void dgListDiff_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    // 틀린색변경
                    if (e.Cell.Column.Name.Equals("SNAP_CNT"))
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));
                        if (e.Cell.Presenter != null && sCheck.Equals("NG"))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STCK_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 재고조사 엑셀업로드
        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("LOTID", typeof(string));
                    dataTable.Columns.Add("CHK", typeof(bool));
                    dataTable.Columns.Add("WIP_QTY", typeof(decimal));
                    // for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                    // {
                    //     dataTable.Columns.Add(getExcelColumnName(colInx), typeof(string));
                    // }
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // DataRow dataRow = dataTable.NewRow();
                        // for (int colInx = 0; colInx < sheet.Rows.Count; colInx++)
                        // {
                        //     XLCell cell = sheet.GetCell(rowInx, colInx);
                        //     if (cell != null)
                        //     {
                        //         dataRow[getExcelColumnName(colInx)] = cell.Text;
                        //     }
                        // }
                        DataRow dataRow = dataTable.NewRow();
                        XLCell cell = sheet.GetCell(rowInx, 0);
                        if (cell != null)
                        {
                            dataRow["LOTID"] = cell.Text;
                            dataRow["CHK"] = true;
                        }

                        dataTable.Rows.Add(dataRow);
                    }
                    dataTable.AcceptChanges();

                    dgListStock.ItemsSource = DataTableConverter.Convert(dataTable);
                }
            }
        }

        private void btnUploadSave_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                return;
            }

            // 저장 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    SaveLotList();
                }
            }
            );
        }
        #endregion

        #region 재고비교 엑셀저장
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

            C1DataGrid[] dataGridArray = new C1DataGrid[2];
            dataGridArray[0] = dgListCompare;
            dataGridArray[1] = dgListCompareDetail;
            string[] excelTabNameArray = new string[2] { "Summary", "Detail" };

            new LGC.GMES.MES.Common.ExcelExporter().Export(dataGridArray, excelTabNameArray);
        }
        #endregion

        #region 전산재고 전체선택 & 전체해제
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void dgListShot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                //  동일한 물류단위만 전체 선택 가능하도록
                if (dgListShot.GetRowCount() > 0)
                {
                    if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4550"); // 동일한 재고실사 제외여부만 전체선택이 가능합니다.
                        chkAll.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListShot.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListShot.Rows[inx].DataItem, "CHK", true);
                }

                // 전산재고 제외/제외취소 버튼 Display
                SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")));
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int inx = 0; inx < dgListShot.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListShot.Rows[inx].DataItem, "CHK", false);
            }
        }

        private void chkHeader_SNAP_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
            object objRowIdx = dgListShot.Rows[idx].DataItem;

            if (objRowIdx != null)
            {
                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("CHK = 'True' AND STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[idx].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4549"); // 동일한 재고실사 제외여부만 선택이 가능합니다.
                        DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", false);
                        return;
                    }

                    DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", true);

                    // 전산재고 제외/제외취소 버튼 Display
                    SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(objRowIdx, "STCK_CNT_EXCL_FLAG")));
                }
            }
        }
        #endregion

        #region 재고실사 전체선택 & 전체해제
        private void chkHeaderAll_RSLT_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgListStock);
        }
        private void chkHeaderAll_RSLT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgListStock);
        }
        #endregion

        #region 전산재고 재공제외
        private void btnExclude_SNAP_Click(object sender, RoutedEventArgs e)
        {
            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                return;
            }

            // 2025.04.04 조회된 데이터의 MES 재고실사 마감여부, NERP 회계마감 여부 체크하도록 수정(전산재고 재공제외)
            string[] sAttrbute = { "Y" };
            ChkNerpFlag(ldpMonthShot);
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }
            else if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sNerpCloseFlag.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(txtExcludeNote_SNAP.Text.Trim()))
            {
                Util.MessageValidation("SFU1590");  // 비고를 입력해 주세요.
                return;
            }

            // 전산재고 LOTID를 제외 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4212"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    string sSTCK_CNT_EXCL_FLAG = "Y";
                    Exclude_SNAP(sSTCK_CNT_EXCL_FLAG);
                }
            }
            );
        }
        #endregion

        #region 전산재고 재공제외취소
        private void btnExclude_SNAP_Cancel_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                return;
            }

            // NERP 대응 처리 추가 (2025.04.04)
            string[] sAttrbute = { "Y" };
            ChkNerpFlag(ldpMonthShot);
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }
            else if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sNerpCloseFlag.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }

            // 전산재고 LOTID를 제외 취소 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4551"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    string sSTCK_CNT_EXCL_FLAG = "N";
                    Exclude_SNAP(sSTCK_CNT_EXCL_FLAG);
                }
            }
            );
        }
        #endregion

        #region 전산재고 선택재고변경
        private void btnRowReSet_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListShot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                return;
            }

            // NERP 대응 처리 추가 (2025.04.04)
            string[] sAttrbute = { "Y" };
            ChkNerpFlag(ldpMonthShot);
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }
            else if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sNerpCloseFlag.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }

            // 전산재고 재공정보를 현재 재공정보로 변경 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4588"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    SetRowLotUpdate_SNAP();
                }
            }
            );
        }
        #endregion

        #region 재고실사 재공제외
        private void btnExclude_RSLT_Click(object sender, RoutedEventArgs e)
        {
            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                return;
            }

            // 2025.04.04 차수마감일 경우 제외 안되도록 수정
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            // 재고실사 LOTID를 제외 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4213"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    Exclude_RSLT();
                }
            }
            );
        }
        #endregion

        #region 재고실사 재공 정보변경
        private void btnStckCntRslt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1632");  // 선택된 LOT이 없습니다.
                    return;
                }

                // 2025.04.04 윤지해 차수마감일 경우 제외 안되도록 수정
                if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
                {
                    Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                    return;
                }

                COM001_011_STOCKCNT_RSLT wndSTOCKCNT_RSLT = new COM001_011_STOCKCNT_RSLT();
                wndSTOCKCNT_RSLT.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_RSLT != null)
                {
                    DataTable dtRSLT = DataTableConverter.Convert(dgListStock.ItemsSource);
                    DataRow[] drRSLT = dtRSLT.Select(" CHK = 'True' ");

                    object[] Parameters = new object[5];
                    Parameters[0] = "COMMON";
                    Parameters[1] = Convert.ToString(cboRsltArea.SelectedValue);
                    Parameters[2] = ldpMonthUpload.SelectedDateTime;
                    Parameters[3] = Convert.ToString(cboStockSeqUpload.SelectedValue);
                    Parameters[4] = drRSLT;

                    C1WindowExtension.SetParameters(wndSTOCKCNT_RSLT, Parameters);

                    wndSTOCKCNT_RSLT.Closed += new EventHandler(wndSTOCKCNT_RSLT_Closed);

                    //  팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_RSLT.ShowModal()));
                    wndSTOCKCNT_RSLT.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndSTOCKCNT_RSLT_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_011_STOCKCNT_RSLT window = sender as COM001_011_STOCKCNT_RSLT;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SetListRslt();
                    Util.MessageInfo("SFU1275");// 정상처리 되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 전산재고 동 value changed
        private void cboAreaShot_SelectedValueChanged_1(object sender, PropertyChangedEventArgs<object> e)
        {

            try
            {
                SetWhId(mboSnapWhId, cboAreaShot);
                SetRackId(mboSnapRackId, mboSnapWhId, cboAreaShot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 실물재고조사 동 value changed
        private void cboRsltArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetWhId(mboRsltWhId, cboRsltArea);
                SetRackId(mboRsltRackId, mboRsltWhId, cboRsltArea);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 전산재고 창고 selection changed
        private void mboSnapWhId_SelectionChanged_1(object sender, EventArgs e)
        {

            try
            {
                SetRackId(mboSnapRackId, mboSnapWhId, cboAreaShot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 실물재고조사 창고 selection changed
        private void mboRsltWhId_SelectionChanged_1(object sender, EventArgs e)
        {
            try
            {
                SetRackId(mboRsltRackId, mboRsltWhId, cboRsltArea);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region 차수마감
        private void DegreeClose()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); // 동은필수입니다.
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        // 전산재고 실사재고 비교 저장
        private void RegStockCountDiff(string stockCountCompleteFlag, DataTable dt = null)
        {

            string bizRuleName = "BR_PRD_REG_STCK_CNT_DIFF";

            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("STCK_CNT_YM", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                dtINDATA.Columns.Add("COM_STCK_DIFF_QTY", typeof(int));
                dtINDATA.Columns.Add("RLTH_STCK_DIFF_QTY", typeof(int));
                dtINDATA.Columns.Add("COM_STCK_QTY", typeof(int));
                dtINDATA.Columns.Add("RLTH_STCK_QTY", typeof(int));
                dtINDATA.Columns.Add("NEXT_PROC_INPUT_QTY", typeof(int));
                dtINDATA.Columns.Add("NOTE", typeof(string));
                dtINDATA.Columns.Add("REGUSER", typeof(string));
                dtINDATA.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));

                if (dt == null)
                {
                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                    drINDATA["STCK_CNT_SEQNO"] = Util.GetCondition(cboStockSeqShot);
                    drINDATA["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); // 동은필수입니다.
                    drINDATA["COM_STCK_DIFF_QTY"] = -1;
                    drINDATA["RLTH_STCK_DIFF_QTY"] = -1;
                    drINDATA["COM_STCK_QTY"] = -1;
                    drINDATA["RLTH_STCK_QTY"] = -1;
                    drINDATA["NEXT_PROC_INPUT_QTY"] = -1;
                    drINDATA["REGUSER"] = LoginInfo.USERID;
                    drINDATA["NOTE"] = string.Empty;
                    drINDATA["STCK_CNT_CMPL_FLAG"] = stockCountCompleteFlag;           // 차수마감시에는 Y, 실사차사유입력시에는 N
                    dtINDATA.Rows.Add(drINDATA);
                    dsINDATA.Tables.Add(dtINDATA);
                }
                else
                {
                    foreach (DataRowView drv in dt.AsDataView())
                    {
                        DataRow drINDATA = dtINDATA.NewRow();
                        drINDATA["LANGID"] = LoginInfo.LANGID;
                        drINDATA["STCK_CNT_YM"] = drv["STCK_CNT_YM"].ToString();
                        drINDATA["STCK_CNT_SEQNO"] = drv["STCK_CNT_SEQNO"].ToString();
                        drINDATA["AREAID"] = drv["AREAID"].ToString();
                        drINDATA["PRODID"] = drv["PRODID"].ToString();
                        drINDATA["COM_STCK_DIFF_QTY"] = Convert.ToDecimal(drv["NOT_SNAP_QTY"]);
                        drINDATA["RLTH_STCK_DIFF_QTY"] = Convert.ToDecimal(drv["NOT_RSLT_QTY"]);
                        drINDATA["COM_STCK_QTY"] = Convert.ToDecimal(drv["SNAP_QTY"]);
                        drINDATA["RLTH_STCK_QTY"] = Convert.ToDecimal(drv["RSLT_STAY_QTY"]);
                        drINDATA["NEXT_PROC_INPUT_QTY"] = Convert.ToDecimal(drv["RSLT_NEXT_QTY"]);
                        drINDATA["NOTE"] = drv["NOTE"].ToString();
                        drINDATA["REGUSER"] = LoginInfo.USERID;
                        drINDATA["STCK_CNT_CMPL_FLAG"] = stockCountCompleteFlag;           // 차수마감시에는 Y, 실사차사유입력시에는 N
                        dtINDATA.Rows.Add(drINDATA);
                    }
                    dsINDATA.Tables.Add(dtINDATA);
                }
                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 전산재고 조회
        // 전산재고
        private void SetListShot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                // RQSTDT.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) // 차수는필수입니다.
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
                }
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); // 동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                if (string.IsNullOrEmpty(txtLotIdShot.Text.Trim()) && string.IsNullOrEmpty(txtCSTIdShot.Text.Trim()))
                {

                    RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT.Columns.Add("WH_ID", typeof(string));
                    RQSTDT.Columns.Add("PRJT", typeof(string));
                    RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
                    RQSTDT.Columns.Add("FINL_WIP_FLAG", typeof(string));

                    dr["PRDTYPE"] = string.IsNullOrWhiteSpace(cboSnapTypeShot.SelectedValue.ToString()) ? null : cboSnapTypeShot.SelectedValue.ToString();
                    dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEqsgShot.SelectedValue.ToString()) ? null : cboEqsgShot.SelectedValue.ToString();
                    // dr["EQSGID"] = mboEqsgShot.SelectedItemsToString == "" ? null : mboEqsgShot.SelectedItemsToString;
                    //  dr["PANCAKE_GR_ID"] = cboSnapTypeShot.SelectedValue == null ? null : cboSnapTypeShot.SelectedValue.ToString();

                    // dr["RACK_ID"] = string.IsNullOrWhiteSpace(cboSnapRackId.SelectedValue.ToString()) ? null : cboSnapRackId.SelectedValue.ToString();
                    // dr["RACK_ID"] = mboSnapRackId.SelectedItemsToString == "" ? null : mboSnapRackId.SelectedItemsToString;
                    dr["RACK_ID"] = Util.NVC(mboSnapRackId.SelectedItemsToString) == "" ? null : mboSnapRackId.SelectedItemsToString;

                    // dr["WH_ID"] = string.IsNullOrWhiteSpace(cboSnapWhId.SelectedValue.ToString()) ? null : cboSnapWhId.SelectedValue.ToString();
                    // dr["WH_ID"] = mboSnapWhId.SelectedItemsToString == "" ? null : mboSnapWhId.SelectedItemsToString;
                    dr["WH_ID"] = Util.NVC(mboSnapWhId.SelectedItemsToString) == "" ? null : mboSnapWhId.SelectedItemsToString;

                    dr["PRJT"] = string.IsNullOrWhiteSpace(cboModelShot.SelectedValue.ToString()) ? null : cboModelShot.SelectedValue.ToString();
                    // dr["PRJT"] = mboModelShot.SelectedItemsToString == "" ? null : mboModelShot.SelectedItemsToString;

                    if (!string.IsNullOrEmpty(cboExclFlagShot.SelectedValue.ToString()) || !cboExclFlagShot.SelectedValue.ToString().Equals(""))
                    {
                        dr["STCK_CNT_EXCL_FLAG"] = cboExclFlagShot.SelectedValue == null ? null : cboExclFlagShot.SelectedValue.ToString();
                    }

                    dr["FINL_WIP_FLAG"] = chkFinlwip.IsChecked == true ? "Y" : null;


                }
                else if (string.IsNullOrEmpty(txtLotIdShot.Text.Trim()))
                {
                    RQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));
                    dr["PANCAKE_GR_ID"] = Util.ConvertEmptyToNull(txtCSTIdShot.Text);
                }
                else
                {
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdShot.Text);
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_SNAP", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListShot, dtRslt, FrameOperation);
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

        // 재고조사 조회
        private void SetListRslt()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                // 

                // 사용안함 - 기존이랑 로직 변경의건
                // RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                // RQSTDT.Columns.Add("PRJT_ABBR_NAME", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthUpload);
                if (!Util.GetCondition(cboStockSeqUpload, "SFU2958").Equals("")) // 차수는필수입니다.
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqUpload));
                }
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                dr["AREAID"] = Util.GetCondition(cboRsltArea, "SFU3203"); // 동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                if (string.IsNullOrEmpty(txtLotIdUpload.Text.Trim()) && string.IsNullOrEmpty(txtCSTIdUpload.Text.Trim()))
                {

                    RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT.Columns.Add("WH_ID", typeof(string));
                    RQSTDT.Columns.Add("PRJT", typeof(string));

                    dr["PRDTYPE"] = string.IsNullOrWhiteSpace(cboRsltTypeRslt.SelectedValue.ToString()) ? null : cboRsltTypeRslt.SelectedValue.ToString();
                    dr["EQSGID"] = string.IsNullOrWhiteSpace(cboRsltEqsg.SelectedValue.ToString()) ? null : cboRsltEqsg.SelectedValue.ToString();

                    // cboRsltRackId => mboRsltRackId로 변경
                    // dr["RACK_ID"] = string.IsNullOrWhiteSpace(cboRsltRackId.SelectedValue.ToString()) ? null : cboRsltRackId.SelectedValue.ToString();
                    dr["RACK_ID"] = Util.NVC(mboRsltRackId.SelectedItemsToString) == "" ? null : mboRsltRackId.SelectedItemsToString;

                    // cboRsltWhId => mboRsltWhId로 변경
                    // dr["WH_ID"]   = string.IsNullOrWhiteSpace(cboRsltWhId.SelectedValue.ToString()) ? null : cboRsltWhId.SelectedValue.ToString();
                    dr["WH_ID"] = Util.NVC(mboRsltWhId.SelectedItemsToString) == "" ? null : mboRsltWhId.SelectedItemsToString;

                    if (!string.IsNullOrEmpty(Util.GetCondition(cboCompareModel)) || Util.GetCondition(cboCompareModel) != "")
                    {
                        dr["PRJT"] = Util.GetCondition(cboCompareModel);
                    }
                    else
                    {
                        dr["PRJT"] = null;
                    }


                }
                else if (string.IsNullOrEmpty(txtLotIdUpload.Text.Trim()))
                {
                    RQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));
                    dr["PANCAKE_GR_ID"] = Util.ConvertEmptyToNull(txtCSTIdUpload.Text);
                }
                else
                {
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdUpload.Text);
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListStock, dtRslt, FrameOperation);
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

        // 재고비교 조회
        private void SetListCompare()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64)); //MI3 증설 형변환 오류로 주석
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                RQSTDT.Columns.Add("PRJT", typeof(string));
                RQSTDT.Columns.Add("FINL_WIP_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_SEQNO"] = Util.GetCondition(cboStockSeqCompare);
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthCompare);
                dr["AREAID"] = Util.GetCondition(cboCompareArea);
                dr["PRDTYPE"] = string.IsNullOrWhiteSpace(cboCompareTypeShot.SelectedValue.ToString()) ? null : Util.ConvertEmptyToNull(cboCompareTypeShot.SelectedValue.ToString());

                dr["PRJT"] = Util.ConvertEmptyToNull(cboCompareModel.SelectedValue.ToString()) == null ? null : Util.ConvertEmptyToNull(cboCompareModel.SelectedValue.ToString());
                dr["FINL_WIP_FLAG"] = chkFinlwipCompare.IsChecked == true ? "Y" : null;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_CNT_SUMMARY", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListCompare, dtRslt, FrameOperation);

                dtCompareDetail = new DataTable();
                dtCompareNotRsltDetail = new DataTable();
                dtCompareNotSnapDetail = new DataTable();
                Util.gridClear(dgListCompareDetail);

                // int j = 0;

                // for(int i =0; dtRslt.Rows.Count > i; i++)
                // {
                //     int temp = dtRslt.Rows[i]["RSLT_QTY"].SafeToInt32() - (dtRslt.Rows[i]["RSLT_STAY_QTY"].SafeToInt32() + dtRslt.Rows[i]["RSLT_NEXT_QTY"].SafeToInt32());

                //     if(temp != 0)
                //     {
                //         j++;
                //     }
                // }

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

        // 실사차 사유 조회
        private void SetListDiff()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataSet dsINDATA = new DataSet();

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                dtINDATA.Columns.Add("STCK_CNT_YM", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("PRDTYPE", typeof(string));
                dtINDATA.Columns.Add("PRJT_NAME", typeof(string));
                dtINDATA.Columns.Add("PRJT_ABBR_NAME", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("PRJT", typeof(string));
                dtINDATA.Columns.Add("FINL_WIP_FLAG", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["STCK_CNT_SEQNO"] = Util.NVC_Int(Util.GetCondition(cboStockSeqDiff)); //MI3 증설 형변환 오류로 INT로 변경
                drINDATA["STCK_CNT_YM"] = Util.GetCondition(ldpMonthDiff);
                drINDATA["AREAID"] = Util.GetCondition(cboDiffArea);
                drINDATA["PRDTYPE"] = string.IsNullOrWhiteSpace(cboDiffTypeShot.SelectedValue.ToString()) ? null : Util.ConvertEmptyToNull(cboDiffTypeShot.SelectedValue.ToString());
                drINDATA["PRJT"] = Util.ConvertEmptyToNull(cboDiffModel.SelectedValue.ToString()) == null ? null : Util.ConvertEmptyToNull(cboDiffModel.SelectedValue.ToString());
                drINDATA["FINL_WIP_FLAG"] = chkFinlwipDiff.IsChecked == true ? "Y" : null;

                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_STCK_CNT_DIFF_LIST", "INDATA", "OUTDATA,OUTDATA_ORDFLAG", dsINDATA);

                Util.gridClear(this.dgListDiff);
                if (!CommonVerify.HasTableInDataSet(dsResult))
                {
                    Util.gridClear(this.dgListDiff);
                    dtDiffDetail = new DataTable();
                    dtDiffNotRsltDetail = new DataTable();
                    dtDiffNotSnapDetail = new DataTable();
                    return;
                }

                // ORDFlag에 따른 읽기 쓰기 처리
                foreach (DataRowView dr in dsResult.Tables["OUTDATA_ORDFLAG"].AsDataView())
                {
                    if (dr["ORD_FLAG"].ToString().Equals("N"))
                    {
                        this.dgListDiff.IsEnabled = true;
                        this.dgListDiff.IsReadOnly = true;
                        this.colNote.IsReadOnly = true;
                        dtDiffDetail = new DataTable();
                        dtDiffNotRsltDetail = new DataTable();
                        dtDiffNotSnapDetail = new DataTable();
                        return;
                    }
                    else
                    {
                        this.dgListDiff.IsEnabled = true;
                        this.dgListDiff.IsReadOnly = false;
                        this.colNote.IsReadOnly = false;
                    }
                }

                if (CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
                {
                    Util.GridSetData(dgListDiff, dsResult.Tables["OUTDATA"], FrameOperation);
                }
                dtDiffDetail = new DataTable();
                dtDiffNotRsltDetail = new DataTable();
                dtDiffNotSnapDetail = new DataTable();
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


        #region 재고조사 엑셀업로드
        private void SaveLotList()
        {
            try
            {
                // if (dgListStock.GetRowCount() < 1)
                // {
                //     Util.MessageValidation("SFU2946"); // 재고조사 파일을 먼저 선택 해 주세요.
                //     return;
                // }

                string sMonth = Util.GetCondition(ldpMonthUpload);
                string sArea = Util.GetCondition(cboRsltArea, "동은 필수입니다.");
                if (sArea.Equals("")) return;
                Int16 iCnt = 0;
                if (!Util.GetCondition(cboStockSeqUpload, "차수는 필수입니다.").Equals(""))
                {
                    iCnt = Convert.ToInt16(Util.GetCondition(cboStockSeqUpload));
                }

                if (iCnt.Equals(0)) return;

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SCAN_TYPE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("WIP_QTY", typeof(decimal));
                RQSTDT.Columns.Add("WIP_QTY2", typeof(decimal));

                DataRowView rowview = null;

                foreach (C1.WPF.DataGrid.DataGridRow row in dgListStock.Rows)
                {
                    rowview = row.DataItem as DataRowView;

                    if (!String.IsNullOrEmpty(rowview["LOTID"].ToString()))
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["STCK_CNT_YM"] = sMonth;
                        dr["AREAID"] = sArea;
                        dr["STCK_CNT_SEQNO"] = iCnt;
                        dr["NOTE"] = "SFU";
                        dr["LOTID"] = rowview["LOTID"].ToString();
                        dr["USERID"] = LoginInfo.USERID;

                        if (rdoBox.IsChecked == true)
                        { dr["SCAN_TYPE"] = _sBOXID; }
                        else
                        { dr["SCAN_TYPE"] = _sLOTID; }

                        if (!String.IsNullOrEmpty(rowview["WIP_QTY"].ToString()))
                        {
                            dr["WIP_QTY"] = rowview["WIP_QTY"].ToString();
                            dr["WIP_QTY2"] = rowview["WIP_QTY"].ToString();
                        }

                        RQSTDT.Rows.Add(dr);
                    }
                }

                // DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EXCEL_UPLOAD", "RQSTDT", "RSLTDT", RQSTDT);

                Util.AlertInfo("SFU1270");  // 저장되었습니다.
                dgListStock.ItemsSource = null;
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

        #region 전산재고 제외/제외취소 버튼 Display
        private void SetExcludeDisplay(string sSTCK_CNT_EXCL_FLAG)
        {
            if (sSTCK_CNT_EXCL_FLAG.Equals("N"))
            {
                tblExcludeNote_SNAP.Visibility = Visibility.Visible;
                txtExcludeNote_SNAP.Visibility = Visibility.Visible;
                btnExclude_SNAP.Visibility = Visibility.Visible;
                btnExclude_SNAP_Cancel.Visibility = Visibility.Collapsed;
            }
            else
            {
                tblExcludeNote_SNAP.Visibility = Visibility.Collapsed;
                txtExcludeNote_SNAP.Visibility = Visibility.Collapsed;
                btnExclude_SNAP.Visibility = Visibility.Collapsed;
                btnExclude_SNAP_Cancel.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region 전산재고 제외처리/제외취소처리
        private void Exclude_SNAP(string sSTCK_CNT_EXCL_FLAG)
        {
            try
            {
                this.dgListShot.EndEdit();
                this.dgListShot.EndEditRow(true);

                DataTable dtRSLT = ((DataView)dgListShot.ItemsSource).Table;

                // RQSTDT
                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_USERID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_NOTE", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
                        dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
                        dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
                        dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
                        dr["STCK_CNT_EXCL_FLAG"] = sSTCK_CNT_EXCL_FLAG;
                        dr["STCK_CNT_EXCL_USERID"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? LoginInfo.USERID : "";
                        dr["STCK_CNT_EXCL_NOTE"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? txtExcludeNote_SNAP.Text : "";
                        dr["USERID"] = LoginInfo.USERID;

                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_SNAP", "RQSTDT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");// 정상처리되었습니다.

                        SetListShot();
                        txtExcludeNote_SNAP.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_SNAP", ex.Message, ex.ToString());
            }
        }
        #endregion

        #region 재고실사 제외처리
        private void Exclude_RSLT()
        {
            try
            {
                // INDATA
                this.dgListStock.EndEdit();
                this.dgListStock.EndEditRow(true);

                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("REAL_WIP_QTY", typeof(decimal));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataTable dtRSLT = ((DataView)dgListStock.ItemsSource).Table;
                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
                        dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
                        dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
                        dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
                        dr["REAL_WIP_QTY"] = string.IsNullOrEmpty(dtRSLT.Rows[i]["WIP_QTY"].ToString()) ? 0 : Convert.ToDecimal(dtRSLT.Rows[i]["WIP_QTY"].ToString());
                        dr["USERID"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = "N";

                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_RSLT", "RQSTDT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");// 정상처리되었습니다.

                        SetListRslt();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_RSLT", ex.Message, ex.ToString());
            }
        }


        #endregion

        #region 전산재고 선택 LOTID 현상태 반영
        private void SetRowLotUpdate_SNAP()
        {
            try
            {

                this.dgListShot.EndEdit();
                this.dgListShot.EndEditRow(true);
                DataTable dtRSLT = ((DataView)dgListShot.ItemsSource).Table;

                // RQSTDT
                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
                        dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
                        dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
                        dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
                        dr["USERID"] = LoginInfo.USERID;

                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_SNAP_LOTID", "RQSTDT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");// 정상처리되었습니다.

                        SetListShot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_SNAP_LOTID", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 엑셀 권한 
        private void SetExcelButton()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CMCDTYPE", typeof(string));
                INDATA.Columns.Add("CMCODE", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["CMCDTYPE"] = "PACK_UI_STOCK_EXCEL_UPLOAD";
                drINDATA["CMCODE"] = LoginInfo.USERID;

                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", INDATA);

                if (dtResult.Rows.Count > 0)
                {
                    rdoLot.Visibility = Visibility.Visible;
                    rdoBox.Visibility = Visibility.Visible;
                    btnUploadFile.Visibility = Visibility.Visible;
                    //  btnUploadSave.Visibility = Visibility.Visible;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Area - SelectedValueChanged
        private void cboAreaShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetStckCntGrShotCombo(null, cboAreaShot);
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboAreaRslt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetStckCntGrShotCombo(null, cboRsltArea);
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboAreaCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetStckCntGrShotCombo(null, cboCompareArea);
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 그룹선택 Combo 조회
        private void SetStckCntGrShotCombo(C1ComboBox cboStckCntGr, C1ComboBox cboArea)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_STOCK_CNT_GR_CBO";
                string[] arrColumn = { "LANGID", "AREAID" };
                string[] arrCondition = { LoginInfo.LANGID, (string)cboArea.SelectedValue ?? null };
                string selectedValueText = cboStckCntGr.SelectedValuePath;
                string displayMemberText = cboStckCntGr.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cboStckCntGr, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);

                int index = cboStckCntGr.Items.Count == 2 ? 1 : 0;
                cboStckCntGr.SelectedIndex = index;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 라인 Combo 조회
        private void SetEqsgCombo(MultiSelectionBox mboProc, C1ComboBox cboArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue ?? null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    mboProc.ItemsSource = DataTableConverter.Convert(dtResult);
                    mboProc.CheckAll();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        #endregion

        #region 라인 선택 - SelectionChanged
        private void mboEqsgShot_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // SetModelMultiCombo(mboModelShot, mboEqsgShot, cboAreaShot);
            }));
        }

        private void mboEqsgRslt_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // SetModelMultiCombo(mboModelRslt, mboEqsgRslt, cboAreaRslt);
            }));
        }

        private void mboEqsgCompare_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // SetModelMultiCombo(mboModelCompare, null , cboAreaCompare);
            }));
        }
        #endregion

        #region Model Combo 조회
        private void SetModelMultiCombo(MultiSelectionBox mboModel, MultiSelectionBox mboEqsg, C1ComboBox cboArea)
        {
            try
            {
                string sSelectedValue = mboModel.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea == null ? null : cboArea.SelectedValue.ToString();
                dr["EQSGID"] = mboEqsg == null ? null : mboEqsg.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    mboModel.ItemsSource = DataTableConverter.Convert(dtResult);
                    mboModel.CheckAll();
                }

            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
        }

        #endregion

        #region WHID 조회
        private void SetWhId(MultiSelectionBox mboWhId, C1ComboBox cb = null)
        {
            try
            {
                string sSelectedValue = mboWhId.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cb == null ? null : cb.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WHID_CBO_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    mboWhId.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    mboWhId.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    mboWhId.ItemsSource = DataTableConverter.Convert(dtResult);
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
        }
        #endregion

        #region WHID - SelectionChanged
        private void mboSnapWhId_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // SetmboRackId(mboSnapRackId, mboSnapWhId, cboAreaShot);
            }));
        }

        private void mboRsltWhId_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // SetmboRackId(mboRsltRackId, mboRsltWhId, cboAreaRslt);
            }));
        }

        private void mboCompareWhId_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // SetmboRackId(mboCompareRackId, mboCompareWhId, cboAreaCompare);
            }));
        }
        #endregion

        #region  RACK ID 조회
        private void SetRackId(MultiSelectionBox mboRackId, MultiSelectionBox msb = null, C1ComboBox cb = null)
        {
            try
            {
                string sSelectedValue = mboRackId.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cb == null ? null : cb.SelectedValue.ToString();
                // dr["WHID"] = msb == null ? null : msb.SelectedItemsToString;
                dr["WHID"] = Util.NVC(msb.SelectedItemsToString) == "" ? null : msb.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PACK_RACK_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                // if (dtResult.Rows.Count > 0)
                // {
                mboRackId.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboRackId.Check(i);
                                break;
                            }
                        }
                    }
                }
                // }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
        }
        private void includeStocker(string sAreaID, string sStockYM, string sStockType, string sStckSumDate)
        {
            try
            {
                string sTmpStockYM = string.Empty;
                string sTmpStockType = string.Empty;
                string sTmpStockArea = string.Empty;
                string sTmpsStckSumDate = string.Empty;

                DataTable dtIndata = new DataTable();
                sTmpStockYM = sStockYM.Trim();
                sTmpStockType = sStockType.Trim();
                sTmpStockArea = sAreaID.Trim();
                sTmpsStckSumDate = sStckSumDate.Trim();

                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SCAN_TYPE", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("STOCKYM", typeof(string));
                dtIndata.Columns.Add("STOCKTYPE", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));
                dtIndata.Columns.Add("SUM_DATE", typeof(string));

                DataRow drIndata = dtIndata.NewRow();
                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["SCAN_TYPE"] = SRCTYPE.SRCTYPE_UI;
                drIndata["AREAID"] = sTmpStockArea;
                drIndata["STOCKYM"] = sTmpStockYM;
                drIndata["STOCKTYPE"] = sTmpStockType;
                drIndata["USERID"] = LoginInfo.USERID;
                drIndata["SUM_DATE"] = sTmpsStckSumDate;

                dtIndata.Rows.Add(drIndata);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOCK_BY_STOCKER", "INDATA", null, dtIndata);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgListStock_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgListStock.Rows[e.Cell.Row.Index] == null)
                return;

            // MES 2.0 오류 Patch
            if (e.Cell.Column.IsReadOnly == false && !e.Cell.Column.Name.Equals("CHK"))
            {
                DataTableConverter.SetValue(dgListStock.Rows[e.Cell.Row.Index].DataItem, "CHK", true);

                // row 색 바꾸기
                dgListStock.SelectedIndex = e.Cell.Row.Index;

            }
        }

        private void SetGridColumnsVisibility(bool visibility)
        {
            if (visibility)
            {
                dgListCompareDetail.Columns["MCS_EQGRNAME"].Visibility = Visibility.Visible;
                dgListCompareDetail.Columns["MCS_EQPTNAME"].Visibility = Visibility.Visible;
                dgListCompareDetail.Columns["MCS_PSTN_GR_NAME"].Visibility = Visibility.Visible;
                dgListCompareDetail.Columns["MCS_PSTN_NAME"].Visibility = Visibility.Visible;
            }
            else
            {
                dgListCompareDetail.Columns["MCS_EQGRNAME"].Visibility = Visibility.Collapsed;
                dgListCompareDetail.Columns["MCS_EQPTNAME"].Visibility = Visibility.Collapsed;
                dgListCompareDetail.Columns["MCS_PSTN_GR_NAME"].Visibility = Visibility.Collapsed;
                dgListCompareDetail.Columns["MCS_PSTN_NAME"].Visibility = Visibility.Collapsed;
            }
        }


        #region 엑셀 다운로드
        private void btnExcel_Snap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListShot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Rslt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListStock);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnExcel_Compare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListCompare);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Compare_Detail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListCompareDetail);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Diff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListDiff);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void cboStockSeqShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(cboStockSeqShot.ItemsSource);

                if (dt == null)
                {
                    snapNote.Text = "";
                    return;
                }

                snapNote.Text = dt.Rows[cboStockSeqShot.SelectedIndex]["STCK_CNT_NOTE"].ToString();

                if (dt.Rows[cboStockSeqShot.SelectedIndex]["CBO_CODE"].ToString().Equals("1"))
                {
                    chkFinlwip.IsEnabled = true;
                }
                else
                {
                    chkFinlwip.IsChecked = false;
                    chkFinlwip.IsEnabled = false;
                }

                // 2025.04.04 마감여부 체크 추가
                _sSTCK_CNT_CMPL_FLAG = dt.Rows[cboStockSeqShot.SelectedIndex]["STCK_CNT_CMPL_FLAG"].ToString();

                ShowBtnCloseCancel(ldpMonthShot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboStockSeqUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(cboStockSeqUpload.ItemsSource);

                if (dt == null)
                {
                    rsltNote.Text = "";
                    return;
                }

                rsltNote.Text = dt.Rows[cboStockSeqUpload.SelectedIndex]["STCK_CNT_NOTE"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboStockSeqCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(cboStockSeqCompare.ItemsSource);

                if (dt == null)
                {
                    summaryCompareNote.Text = "";
                    return;
                }

                summaryCompareNote.Text = dt.Rows[cboStockSeqCompare.SelectedIndex]["STCK_CNT_NOTE"].ToString();

                if (dt.Rows[cboStockSeqCompare.SelectedIndex]["CBO_CODE"].ToString().Equals("1"))
                {
                    chkFinlwipCompare.IsEnabled = true;
                }
                else
                {
                    chkFinlwipCompare.IsChecked = false;
                    chkFinlwipCompare.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboStockSeqDiff_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(cboStockSeqDiff.ItemsSource);

                if (dt == null)
                {
                    this.summaryDiffNote.Text = "";
                    return;
                }

                summaryDiffNote.Text = dt.Rows[cboStockSeqDiff.SelectedIndex]["STCK_CNT_NOTE"].ToString();

                if (dt.Rows[cboStockSeqDiff.SelectedIndex]["CBO_CODE"].ToString().Equals("1"))
                {
                    chkFinlwipDiff.IsEnabled = true;
                }
                else
                {
                    chkFinlwipDiff.IsChecked = false;
                    chkFinlwipDiff.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean makeDetailTypeCombo(ref C1ComboBox c1ComboBox)
        {
            try
            {
                DataTable dtCombo = new DataTable();

                dtCombo.Columns.Add("CBO_NAME", typeof(string));
                dtCombo.Columns.Add("CBO_CODE", typeof(string));

                DataRow newRow = dtCombo.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("ALL");
                newRow["CBO_CODE"] = null;

                dtCombo.Rows.Add(newRow);

                newRow = dtCombo.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("전산재고 대비 차이");
                newRow["CBO_CODE"] = "NOT_SNAP";
                dtCombo.Rows.Add(newRow);

                newRow = dtCombo.NewRow();
                newRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("실물재고 대비 차이");
                newRow["CBO_CODE"] = "NOT_RSLT";
                dtCombo.Rows.Add(newRow);

                c1ComboBox.ItemsSource = dtCombo.Copy().AsDataView();

                c1ComboBox.SelectedIndex = 0;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        private void cboDetailTypeCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string strDetailType = Util.NVC(cboDetailTypeCompare.SelectedValue);

                if (string.IsNullOrEmpty(strDetailType))
                {
                    Util.GridSetData(dgListCompareDetail, dtCompareDetail, FrameOperation);
                }
                else
                {
                    if (strDetailType == "NOT_SNAP")
                    {
                        Util.GridSetData(dgListCompareDetail, dtCompareNotSnapDetail, FrameOperation);
                    }
                    else if (strDetailType == "NOT_RSLT")
                    {
                        Util.GridSetData(dgListCompareDetail, dtCompareNotRsltDetail, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDiffNoteSave_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result3) =>
            {
                if (result3.ToString().Equals("OK"))
                {
                    this.RegStockCountDiff("N", DataTableConverter.Convert(this.dgListDiff.ItemsSource));
                    Util.AlertInfo("SFU1270");      //저장되었습니다.

                    this.SetListDiff();
                }
            }
            );
        }

        #region NERP 대응 추가 (2025.04.04)

        #region 차수마감 취소
        private void btnDegreeCloseCancel_Click(object sender, RoutedEventArgs e)
        {
            ChkNerpFlag(ldpMonthShot);

            if (_sNerpCloseFlag.Equals("N"))
            {
                // 마감 취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3685"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        DegreeCloseCancel();
                    }
                }
                );
            }
            else
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                ShowBtnCloseCancel(ldpMonthShot);
                return;
            }
        }
        #endregion

        #region DegreeCloseCancel : 마감 취소
        private void DegreeCloseCancel()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
                }
                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("") || dr["STCK_CNT_SEQNO"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT_CANCEL", "INDATA", null, RQSTDT);

                _combo.SetCombo(cboStockSeqShot);
                _combo.SetCombo(cboStockSeqUpload);
                _combo.SetCombo(cboStockSeqCompare);

                ShowBtnCloseCancel(ldpMonthShot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ChkNerpApplyFlag : NERP 적용 여부
        private void ChkNerpApplyFlag()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(newRow);

            // GET SYSTEM_ID 
            try
            {
                DataTable dtNerpFlag = new ClientProxy().ExecuteServiceSync("BR_SEL_MMD_NERP_APPLY_FLAG", "RQSTDT", "RSLTDT", inTable);
                if (dtNerpFlag != null && dtNerpFlag.Rows.Count > 0)
                {
                    _sNerpApplyFlag = dtNerpFlag.Rows[0]["NERP_APPLY_FLAG"].ToString();
                }
            }
            catch (Exception ex)
            {
                _sNerpApplyFlag = "N";
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region ShowBtnCloseCancel : 차수마감/취소 버튼 제어 
        private void ShowBtnCloseCancel(LGCDatePicker ldpMonthShot)
        {
            ChkNerpApplyFlag();

            string[] sAttrbute = { "Y" };

            // 기본 
            btnDegreeClose.Visibility = Visibility.Visible;
            btnDegreeCloseCancel.Visibility = Visibility.Collapsed;

            if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y"))
            {
                ChkNerpFlag(ldpMonthShot);
                ChkMaxSeq(ldpMonthShot);

                // 최고 차수 : 1, 차수마감 완료, NERP 회계마감 진행중인 경우: 마감 취소 활성화 
                if (_sNerpCloseFlag.Equals("N") && _sMaxSeq.Equals("1") && _sMaxSeqCmplFlag.Equals("Y"))
                {
                    btnDegreeClose.Visibility = Visibility.Collapsed;
                    btnDegreeCloseCancel.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region ChkAreaComCode : 동별코드 조회
        private bool ChkAreaComCode(string sCodeType, string sCodeName, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboAreaShot);
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                {
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region ChkMaxSeq : 재고실사 년월에 따른 마지막 차수 및 마감여부 조회
        private void ChkMaxSeq(LGCDatePicker ldpMonthShot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.

                if (dr["STCK_CNT_YM"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_MAX_SEQ_TOP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _sMaxSeq = dtRslt.Rows[0]["STCK_CNT_SEQNO"].ToString();
                    _sMaxSeqCmplFlag = dtRslt.Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();
                }
                else
                {
                    _sMaxSeq = "0";
                    _sMaxSeqCmplFlag = "N";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ChkNerpFlag : NERP 회계마감 여부 
        private void ChkNerpFlag(LGCDatePicker ldpMonthShot)
        {
            try
            {
                // 2024.08.02 Addumairi Change to reduce minus 1 month for input in DA
                DateTime prevMonth = DateTime.Now.AddMonths(-1);

                if (ldpMonthShot.SelectedDateTime.Year > DateTime.MinValue.Year || ldpMonthShot.SelectedDateTime.Month > DateTime.MinValue.Month)
                {
                    prevMonth = ldpMonthShot.SelectedDateTime.AddMonths(-1);
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CLOSE_YM", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                // dr["CLOSE_YM"] = Util.GetCondition(ldpMonthShot);
                dr["CLOSE_YM"] = prevMonth.ToString("yyyyMM");

                if (dr["CLOSE_YM"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_NERP_ACTG_CLOSE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _sNerpCloseFlag = dtRslt.Rows[0]["CLOSE_FLAG"].ToString();
                }
                else
                {
                    _sNerpCloseFlag = "N";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        

        #endregion
    }
}
