/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
                1. 수정시 COM001_011-재고조사, COM001_125-재고조사(활성화), COM001_214-재고조사(소형파우치)화면을 고려해 주세요.
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.04.28  강지수    : 전산재고 탭의 창고ID,랙ID 추가 / 재고조사 탭의 리스트 조회기능 추가
  2019.02.18  이대근     [CSR ID:3863368] Add Change Status Time to Stock Inspection Tables | [요청번호]C20181206_63368 
  2019.03.20  JS.HONG    C20190208_17512         GMES 재고비교 TAB ‘실물 Rack ID’ , ‘전극유효기간’ 추가 요청의 건
  2019.12.20  JS.HONG    GMES19-R188             GMES 재고비교 TAB 재교비교 Summary : 자동창고, 재고정리 , 재고비교 Detail : 차기공정(투입완료시간), 자동창고여부, 재고정리(재고정리여부, 재고정리항목, 재고정리시간, 재고정리전기일) 추가
  2020.03.27  정문교     조회 그리드에 위치구분, 위치, 상세위치구분, 상세위치 칼럼 추가
  2021.05.03  장희만     C20220501-000014        조회옵션 PROCESS, LINE 선택사항 조건에 누락되어 추가
  2023.01.31  윤지해     C20220718-000125        1bldg-재고조사계면에 생산유형구분(P/X/L)을 추가합니다_생산유형구분 추가
  2023.07.31  강성묵     C20230103-000963        엑셀 파일 업로드 IOException 에러 메세지 수정
  2024.01.10  오수현     E20231009-000017        [제외, 제외취소] 처리시 한번에 단일 공정 Lot만 처리. 동별공통코드에 사용자 ID 등록하여 [제외, 제외취소] 버튼 활성화 권한 설정 가능(등록되지 않은 동은 Default 활성화)
  2024.01.23  오수현     E20231009-000017        [제외, 제외취소] 처리시 한번에 단일 공정 Lot만 처리 로직에서 동 하드코딩을 공통코드화 전환
  2025.02.25  안민호     영문UI에서 - '차수는필수입니다' 메시지 다국어 처리
  2025.04.03  김영택     NERP 대응               [NERP 대응] NERP 회계마감 진행중인 경우 : 차수마감 및 차수추가 불가
                                                 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Configuration;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_011 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        public COM001_011()
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
        private string _sStckGrCompare = string.Empty;
        private string _sPilotProdDivsCode = string.Empty; // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다
        
        // 2025.04.03 NERP 대응 처리 
        private string _sNerpApplyFlag = string.Empty; // NERP 적용 여부 
        private string _sNerpCloseFlag = string.Empty;  // NERP 화계마감 여부 
        private string _sMaxSeq = string.Empty;         // 재고실사 년월, 동에 따른 마지막 차수 조회
        private string _sMaxSeqCmplFlag = string.Empty; // 재고실사 년월, 동에 따른 마지막 차수 마감상태 조회

        // 조회 조건 저장 용도 (전산재고)
        //  E20250310-000144      NERP 대응 프로젝트 : 제외/제외취소/선택재고변경 시 차수마감여부 재조회 하도록 수정 
        private string _sDgNerpCloseFlagShot = string.Empty;
        private string _sDgMesAreaShot = string.Empty;       
        private string _sDgMesSeqShot = string.Empty;       
        private string _sDgMesYMShot = string.Empty;

        // 조회조건 저장 용도 (실사재고) 
        private string _sDgMesAreaStock = string.Empty;     
        private string _sDgMesSeqStock = string.Empty;   
        private string _sDgMesYMStock = string.Empty;


        private const string _sLOTID = "LOTID";
        private const string _sBOXID = "BOXID";

        DataView _dvSTCKCNT { get; set; }

        string _sSTCK_CNT_CMPL_FLAG = string.Empty;
        bool _isExcludeAuthority = true; // 2024.01.09[E20231009-000017]

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            //CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaShotChild = { cboStockSeqShot };
            _combo.SetCombo(cboAreaShot, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaShotChild);

            C1ComboBox[] cboAreaUploadChild = { cboStockSeqUpload };
            _combo.SetCombo(cboAreaUpload, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaUploadChild);

            C1ComboBox[] cboAreaCompareChild = { cboStockSeqCompare };
            _combo.SetCombo(cboAreaCompare, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaCompareChild);

            // 극성
            String[] sFilterElectype = { "", "ELEC_TYPE" };
            _combo.SetCombo(cboElecTypeShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");
            _combo.SetCombo(cboElecTypeUpload, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");
            _combo.SetCombo(cboElecTypeCompare, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");

            //재고실사 제외 여부
            String[] sFilterExclFlag = { "", "STCK_CNT_EXCL_FLAG" };
            _combo.SetCombo(cboExclFlagShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterExclFlag, sCase: "COMMCODES");

            #region 2023.01.31 [C20220718-000125] 1bldg - 재고조사계면에 생산유형구분(P/X/L)을 추가합니다
            // 생산구분
            string[] sFilterProdDiv = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDivShot, CommonCombo.ComboStatus.ALL, sFilter: sFilterProdDiv, sCase: "COMMCODE");
            _combo.SetCombo(cboProductDivUpload, CommonCombo.ComboStatus.ALL, sFilter: sFilterProdDiv, sCase: "COMMCODE");
            _combo.SetCombo(cboProductDivCompare, CommonCombo.ComboStatus.ALL, sFilter: sFilterProdDiv, sCase: "COMMCODE");
            #endregion

            if (cboExclFlagShot.Items.Count > 0) cboExclFlagShot.SelectedIndex = 1;

            object[] objStockSeqShotParent = { cboAreaShot, ldpMonthShot };
            String[] sFilterAll = { "" };
            _combo.SetComboObjParent(cboStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterAll);

            object[] cboStockSeqUploadParent = { cboAreaUpload, ldpMonthUpload };
            _combo.SetComboObjParent(cboStockSeqUpload, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboStockSeqUploadParent, sFilter: sFilterAll);

            object[] cboStockSeqCompareParent = { cboAreaCompare, ldpMonthCompare };
            _combo.SetComboObjParent(cboStockSeqCompare, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: cboStockSeqCompareParent, sFilter: sFilterAll);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetProcessCombo(cboProcShot, cboAreaShot, cboStckCntGrShot);
            SetProcessCombo(cboProcUpload, cboAreaUpload, cboStckCntGrUpload);
            SetProcessCombo(cboProcCompare, cboAreaCompare, cboStckCntGrCompare);

            // 재고 비교 Detail 자동실사상세 칼럼 숨김처리 
            SetGridColumnsVisibility(false);

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnExclude_SNAP);
            listAuth.Add(btnExclude_RSLT);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            // [E20231009-000017]
            if (GetExcludAuthorityArea(cboAreaShot)) // AREA 권한 체크 - 2024.01.23
            {
                GetExcludAuthorityUserIDChk(cboAreaShot); //USERID  권한 체크 - 2024.01.10
            }

            ChkNerpApplyFlag();

            ShowBtnCloseCancel(ldpMonthShot);
        }

        #region 차수마감
        private void btnDegreeClose_Click(object sender, RoutedEventArgs e)
        {
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            //마감하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeClose();
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
                ChkNerpApplyFlag();
                ChkNerpFlag(ldpMonthShot);

                string[] sAttrbute = { "Y" };

                if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sNerpCloseFlag.Equals("N"))
                {
                    Util.MessageValidation("SFU3686");  // NERP 회계 마감기간 중 차수 추가를 할 수 없습니다.
                    return;
                }

                COM001_011_STOCKCNT_START wndSTOCKCNT_START = new COM001_011_STOCKCNT_START();
                wndSTOCKCNT_START.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_START != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = Convert.ToString(cboAreaShot.SelectedValue);
                    Parameters[1] = ldpMonthShot.SelectedDateTime;

                    C1WindowExtension.SetParameters(wndSTOCKCNT_START, Parameters);

                    wndSTOCKCNT_START.Closed += new EventHandler(wndSTOCKCNT_START_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
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
                COM001_011_STOCKCNT_START window = sender as COM001_011_STOCKCNT_START;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    //CommonCombo _combo = new CommonCombo();
                    _combo.SetCombo(cboStockSeqShot);
                    _combo.SetCombo(cboStockSeqUpload);
                    _combo.SetCombo(cboStockSeqCompare);

                    Util.gridClear(dgListShot);
                    Util.gridClear(dgListStock);
                    Util.gridClear(dgListCompare);
                    Util.gridClear(dgListCompareDetail);

                    SetListShot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 전산재고 조회
        private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        {
            SetListShot();
        }
        #endregion

        #region 재고조사 조회
        private void btnSearchStock_Click(object sender, RoutedEventArgs e)
        {
            SetListStock();
        }
        #endregion

        #region 재고비교 조회
        private void btnSearchCompare_Click(object sender, RoutedEventArgs e)
        {
            int iEqsgCompareItemCnt = (cboEqsgCompare.ItemsSource == null ? 0 : ((DataView)cboEqsgCompare.ItemsSource).Count);
            int iEqsgCompareSelectedCnt = cboEqsgCompare.SelectedItemsToString.Split(',').Length;

            int iProcCompareItemCnt = (cboProcCompare.ItemsSource == null ? 0 : ((DataView)cboProcCompare.ItemsSource).Count);
            int iProcCompareSelectedCnt = cboProcCompare.SelectedItemsToString.Split(',').Length;

            //_sEqsgID = (iEqsgCompareItemCnt == iEqsgCompareSelectedCnt ? null : Util.ConvertEmptyToNull(cboEqsgCompare.SelectedItemsToString));
            //_sProcID = (iProcCompareItemCnt == iProcCompareSelectedCnt ? null : Util.ConvertEmptyToNull(cboProcCompare.SelectedItemsToString));
            _sEqsgID = Util.ConvertEmptyToNull(cboEqsgCompare.SelectedItemsToString);
            _sProcID = Util.ConvertEmptyToNull(cboProcCompare.SelectedItemsToString);

            _sProdID = Util.ConvertEmptyToNull(txtProdCompare.Text);
            _sElecType = Util.ConvertEmptyToNull(Util.GetCondition(cboElecTypeCompare));
            _sPrjtName = Util.ConvertEmptyToNull(txtPrjtNameCompare.Text);
            _sAutoWhStckFlag = chkAutoWhStckFlag.IsChecked == true ? "Y" : "N";
            _sStckAdjFlag = chkStckAdjFlag.IsChecked == true ? "Y" : "N";
            _sPilotProdDivsCode = Util.GetCondition(cboProductDivCompare, bAllNull: true);  // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다


            //Summary 조회
            SetListCompare(_sProdID, _sEqsgID, _sProcID, _sElecType, _sPrjtName, _sAutoWhStckFlag, _sStckAdjFlag, _sPilotProdDivsCode);

            //Detail 조회
            SetListCompareDetail(_sProdID, _sEqsgID, _sProcID, _sElecType, _sPrjtName, _sAutoWhStckFlag, _sStckAdjFlag, _sPilotProdDivsCode);
        }
        #endregion

        private void ldpMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboStockSeqShot);
            ShowBtnCloseCancel(ldpMonthShot);
        }

        private void ldpMonthUpload_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboStockSeqUpload);
        }

        private void ldpMonthCompare_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboStockSeqCompare);
        }

        //private void dgListCompare_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (dgListCompare.CurrentColumn.Name.Equals("PRODID") && dgListCompare.CurrentRow !=null)
        //    {
        //        SetListCompareDetail(Util.NVC(DataTableConverter.GetValue(dgListCompare.CurrentRow.DataItem, "PRODID")));
        //    }
        //}

        private void dgListCompareChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").IsFalse())
            {
                //체크시 처리될 로직
                SetListCompareDetail(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID")), _sEqsgID, _sProcID, _sElecType, _sPrjtName, _sAutoWhStckFlag, _sStckAdjFlag);


                //여기까지 체크시 처리될 로직
                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

            }
        }

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
                    //link 색변경
                    //if (e.Cell.Column.Name.Equals("PRODID"))
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    //}

                    //틀린색변경
                    if (e.Cell.Column.Name.Equals("SNAP_CNT"))
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW_COLOR"));
                        if (e.Cell.Presenter != null && sCheck.Equals("NG"))
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);

                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["SNAP_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STCK_CNT"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STCK_SUM"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
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
                        //전산재고와 실물 수량이 맞지않으면 Yellow
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

                            //실물만 있는 LOTID면 Red
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

                            //재고비교 상세에 전산 재고만 있고 재고실사가 않된 항목에 대하여는 붉은색(BOLD)로 표시
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
                try
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
                        //for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                        //{
                        //    dataTable.Columns.Add(getExcelColumnName(colInx), typeof(string));
                        //}
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            //DataRow dataRow = dataTable.NewRow();
                            //for (int colInx = 0; colInx < sheet.Rows.Count; colInx++)
                            //{
                            //    XLCell cell = sheet.GetCell(rowInx, colInx);
                            //    if (cell != null)
                            //    {
                            //        dataRow[getExcelColumnName(colInx)] = cell.Text;
                            //    }
                            //}
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
                catch (System.IO.IOException IOE)
                {
                    // 2023.07.31 강성묵 엑셀 파일 업로드 IOException 에러 메세지 수정
                    System.Text.StringBuilder sbMsg = new System.Text.StringBuilder();

                    if (string.IsNullOrEmpty(IOE.Message) == false)
                    {
                        sbMsg.Append(IOE.Message).Append("\n\n");
                    }

                    string sMethodName = new System.Diagnostics.StackFrame(0, true).GetMethod().Name;
                    string[] aSplit = Convert.ToString(IOE.StackTrace).Split('\r');
                    int iCnt = 0;

                    foreach (string sItem in aSplit)
                    {
                        sbMsg.Append(sItem);
                        iCnt++;

                        if (iCnt > 2)
                        {
                            break;
                        }
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sbMsg.ToString(), null, "Error", MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
                }
            }
        }

        private void btnUploadSave_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            //저장 하시겠습니까?
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

        #region 동/기준월/차수별  Note설정
        private void cboStockSeqShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _dvSTCKCNT = cboStockSeqShot.ItemsSource as DataView;
            txtStckCntCmplFlagShot.Text = string.Empty;

            string sStckCntSeq = cboStockSeqShot.Text;
            if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
            {
                _dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
                txtStckCntCmplFlagShot.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
                _sSTCK_CNT_CMPL_FLAG = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();

                _dvSTCKCNT.RowFilter = null;
            }

            ShowBtnCloseCancel(ldpMonthShot);
        }

        private void cboStockSeqUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _dvSTCKCNT = cboStockSeqUpload.ItemsSource as DataView;
            txtStckCntCmplFlagUpload.Text = string.Empty;

            string sStckCntSeq = cboStockSeqUpload.Text;
            if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
            {
                _dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
                txtStckCntCmplFlagUpload.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
                _dvSTCKCNT.RowFilter = null;
            }
        }

        private void cboStockSeqCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _dvSTCKCNT = cboStockSeqCompare.ItemsSource as DataView;
            txtStckCntCmplFlagCompare.Text = string.Empty;

            string sStckCntSeq = cboStockSeqCompare.Text;
            if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
            {
                _dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
                txtStckCntCmplFlagCompare.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
                _dvSTCKCNT.RowFilter = null;
            }
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
                // 동일한 물류단위만 전체 선택 가능하도록
                if (dgListShot.GetRowCount() > 0)
                {
                    if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4550"); //동일한 재고실사 제외여부만 전체선택이 가능합니다.
                        chkAll.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListShot.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListShot.Rows[inx].DataItem, "CHK", true);
                }

                if (_isExcludeAuthority)
                {
                    //전산재고 제외/제외취소 버튼 Display
                    SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")));
                }
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
                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    if (DataTableConverter.Convert(dgListShot.ItemsSource).Select("CHK = 1 AND STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListShot.Rows[idx].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4549"); //동일한 재고실사 제외여부만 선택이 가능합니다.
                        DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", false);
                        return;
                    }

                    DataTableConverter.SetValue(dgListShot.Rows[idx].DataItem, "CHK", true);

                    if (_isExcludeAuthority)
                    {
                        //전산재고 제외/제외취소 버튼 Display
                        SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(objRowIdx, "STCK_CNT_EXCL_FLAG")));
                    }
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
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 2025.04.04 조회된 데이터의 MES 재고실사 마감여부, NERP 회계마감 여부 체크하도록 수정(전산재고 재공제외)
            if (ChkCmplFlag(_sDgMesYMShot, _sDgMesAreaShot, _sDgMesSeqShot))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            string[] sAttrbute = { "Y" };

            if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sDgNerpCloseFlagShot.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }



            // 단일 공정 LOT을 선택해 주세요.
            if (_isExcludeAuthority && !ValidateSingleProcess())
            {
                return;
            }

            if (string.IsNullOrEmpty(txtExcludeNote_SNAP.Text.Trim()))
            {
                Util.MessageValidation("SFU1590");  //비고를 입력해 주세요.
                return;
            }

            //전산재고 LOTID를 제외 하시겠습니까?
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
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 2025.04.04 조회된 데이터의 MES 재고실사 마감여부, NERP 회계마감 여부 체크하도록 수정(전산재고 재공제외취소)
            if (ChkCmplFlag(_sDgMesYMShot, _sDgMesAreaShot, _sDgMesSeqShot))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            string[] sAttrbute = { "Y" };

            if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sDgNerpCloseFlagShot.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }

            // 단일 공정 LOT을 선택해 주세요.
            if (_isExcludeAuthority && !ValidateSingleProcess())
            {
                return;
            }

            //전산재고 LOTID를 제외 취소 하시겠습니까?
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
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 2025.04.04 : 조회된 데이터의 MES 재고실사 마감여부, NERP 회계마감 여부 체크하도록 수정(전산재고 선택재고변경)
            if (ChkCmplFlag(_sDgMesYMShot, _sDgMesAreaShot, _sDgMesSeqShot))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            string[] sAttrbute = { "Y" };

            if (ChkAreaComCode("STCK_CNT_APPLY_SYSTEM_ID", LoginInfo.SYSID, sAttrbute) && _sNerpApplyFlag.Equals("Y") && _sMaxSeq.Equals("1") && _sDgNerpCloseFlagShot.Equals("Y"))
            {
                Util.MessageValidation("SFU3687"); // NERP 마감되어 수정할 수 없습니다.
                return;
            }

            //전산재고 재공정보를 현재 재공정보로 변경 하시겠습니까?
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
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 2025.04.04 조회된 데이터의 MES 재고실사 마감여부, NERP 회계마감 여부 체크하도록 수정(재고실사 재공제외)
            if (ChkCmplFlag(_sDgMesYMStock, _sDgMesAreaStock, _sDgMesSeqStock))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            //재고실사 LOTID를 제외 하시겠습니까?
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
                    Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }

                // 2025.04.04 조회된 데이터의 MES 재고실사 마감여부, NERP 회계마감 여부 체크하도록 수정(재고실사 재공 정보변경)
                if (ChkCmplFlag(_sDgMesYMStock, _sDgMesAreaStock, _sDgMesSeqStock))
                {
                    Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                    return;
                }

                COM001_011_STOCKCNT_RSLT wndSTOCKCNT_RSLT = new COM001_011_STOCKCNT_RSLT();
                wndSTOCKCNT_RSLT.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_RSLT != null)
                {
                    DataTable dtRSLT = DataTableConverter.Convert(dgListStock.ItemsSource);
                    DataRow[] drRSLT = dtRSLT.Select(" CHK = 1 ");

                    for(int i=0;i<drRSLT.Length;i++)
                    {
                        if(drRSLT[0]["AREAID"].Equals(""))
                        {
                            throw new ArgumentException();
                        }
                    }

                    object[] Parameters = new object[5];
                    Parameters[0] = "COMMON";
                    Parameters[1] = Convert.ToString(cboAreaUpload.SelectedValue);
                    Parameters[2] = ldpMonthUpload.SelectedDateTime;
                    Parameters[3] = Convert.ToString(cboStockSeqUpload.SelectedValue);
                    Parameters[4] = drRSLT;

                    C1WindowExtension.SetParameters(wndSTOCKCNT_RSLT, Parameters);

                    wndSTOCKCNT_RSLT.Closed += new EventHandler(wndSTOCKCNT_RSLT_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_RSLT.ShowModal()));
                    wndSTOCKCNT_RSLT.BringToFront();
                }
            }
            catch (ArgumentException)
            {
                Util.MessageValidation("SFU4215");
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
                    SetListStock();
                    Util.MessageInfo("SFU1275");//정상처리 되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        /// <summary>
        /// 자동창고 재고조사 포함 Checked, Unchecked
        /// </summary>
        private void chkAutoWhStckFlag_Checked(object sender, RoutedEventArgs e)
        {
            SetGridColumnsVisibility(true);
        }

        private void chkAutoWhStckFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            SetGridColumnsVisibility(false);
        }


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
                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCKCNT", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);

                //CommonCombo _combo = new CommonCombo();
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

        #region 전산재고 조회
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
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
                }
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                if (string.IsNullOrEmpty(txtLotIdShot.Text.Trim()))
                {
                    int iEqsgShotItemCnt = (cboEqsgShot.ItemsSource == null ? 0 : ((DataView)cboEqsgShot.ItemsSource).Count);
                    int iEqsgShotSelectedCnt = cboEqsgShot.SelectedItemsToString.Split(',').Length;
                    int iProcShotItemCnt = (cboProcShot.ItemsSource == null ? 0 : ((DataView)cboProcShot.ItemsSource).Count);
                    int iProcShotSelectedCnt = cboProcShot.SelectedItemsToString.Split(',').Length;

                    dr["EQSGID"] = Util.ConvertEmptyToNull(cboEqsgShot.SelectedItemsToString);
                    dr["PROCID"] = Util.ConvertEmptyToNull(cboProcShot.SelectedItemsToString);
                    dr["PRODID"] = Util.ConvertEmptyToNull(txtProdShot.Text);
                    dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboElecTypeShot));
                    dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtNameShot.Text);
                    dr["STCK_CNT_EXCL_FLAG"] = Util.ConvertEmptyToNull(Util.GetCondition(cboExclFlagShot));
                    dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDivShot, bAllNull: true);  // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다
                    #region CSTID
                    if (!Util.GetCondition(txtCSTIDShot).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDShot);
                    #endregion
                }
                else
                {
                    dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdShot.Text);
                }



                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MESSTOCK", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListShot, dtRslt, FrameOperation);

                // 조회 데이터의 조건 저장 
                _sDgNerpCloseFlagShot = _sNerpCloseFlag;
                _sDgMesAreaShot = Util.GetCondition(cboAreaShot);
                _sDgMesYMShot = Util.GetCondition(ldpMonthShot);
                _sDgMesSeqShot = Util.GetCondition(cboStockSeqShot);
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

        #region 재고조사 조회
        private void SetListStock()
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
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthUpload);
                if (!Util.GetCondition(cboStockSeqUpload, "SFU2958").Equals("")) //차수는필수입니다.
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqUpload));
                }
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                dr["AREAID"] = Util.GetCondition(cboAreaUpload, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                if (string.IsNullOrEmpty(txtLotIdUpload.Text.Trim()))
                {
                    int iEqsgUploadItemCnt = (cboEqsgUpload.ItemsSource == null ? 0 : ((DataView)cboEqsgUpload.ItemsSource).Count);
                    int iEqsgUploadSelectedCnt = cboEqsgUpload.SelectedItemsToString.Split(',').Length;
                    int iProcUploadItemCnt = (cboProcUpload.ItemsSource == null ? 0 : ((DataView)cboProcUpload.ItemsSource).Count);
                    int iProcUploadSelectedCnt = cboProcUpload.SelectedItemsToString.Split(',').Length;

                    dr["EQSGID"] = Util.ConvertEmptyToNull(cboEqsgUpload.SelectedItemsToString);
                    dr["PROCID"] = Util.ConvertEmptyToNull(cboProcUpload.SelectedItemsToString);
                    dr["PRODID"] = Util.ConvertEmptyToNull(txtProdUpload.Text);
                    dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboElecTypeUpload));
                    dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtNameUpload.Text);
                    dr["CSTID"] = Util.ConvertEmptyToNull(txtCSTIdUpload.Text);
                    dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDivUpload, bAllNull: true);  // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다
                }
                else
                {
                    dr["LOTID"] = Util.ConvertEmptyToNull(txtLotIdUpload.Text);
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListStock, dtRslt, FrameOperation);

                // 2025.04.04 조회한 데이터의 동/기준월/차수 데이터 저장
                _sDgMesAreaStock = Util.GetCondition(cboAreaUpload);
                _sDgMesYMStock = Util.GetCondition(ldpMonthUpload);
                _sDgMesSeqStock = Util.GetCondition(cboStockSeqUpload);
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

        #region 재고비교 조회
        private void SetListCompare(string sProdID = null, string sEqsgID = null, string sProcID = null, string sElecType = null, string sPrjtName = null, string sAutoWhStckFlag = null, string sStckAdjFlag = null, string sPilotProdDivsCode = null)
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
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("AUTO_WH_STCK_FLAG", typeof(string));
                RQSTDT.Columns.Add("STCK_ADJ_FLAG", typeof(string));
                RQSTDT.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthCompare);
                if (!Util.GetCondition(cboStockSeqCompare, "SFU2958").Equals(""))
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqCompare));
                }
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                dr["AREAID"] = Util.GetCondition(cboAreaCompare, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = sEqsgID;
                dr["PROCID"] = sProcID;
                dr["PRODID"] = sProdID;
                dr["PRDT_CLSS_CODE"] = sElecType;
                dr["PRJT_NAME"] = sPrjtName;
                dr["AUTO_WH_STCK_FLAG"] = sAutoWhStckFlag;
                dr["STCK_ADJ_FLAG"] = sStckAdjFlag;
                dr["PILOT_PROD_DIVS_CODE"] = sPilotProdDivsCode;  // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_COMPARE", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListCompare, dtRslt, FrameOperation);
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

        private void SetListCompareDetail(string sProdID = null, string sEqsgID = null, string sProcID = null, string sElecType = null, string sPrjtName = null, string sAutoWhStckFlag = null, string sStckAdjFlag = null, string sPilotProdDivsCode = null)
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
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("AUTO_WH_STCK_FLAG", typeof(string));
                RQSTDT.Columns.Add("STCK_ADJ_FLAG", typeof(string));
                RQSTDT.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthCompare);
                if (!Util.GetCondition(cboStockSeqCompare, "SFU2958").Equals(""))
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqCompare));
                }
                if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                dr["AREAID"] = Util.GetCondition(cboAreaCompare, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = sEqsgID;
                dr["PROCID"] = sProcID;
                dr["PRODID"] = sProdID;
                dr["PRDT_CLSS_CODE"] = sElecType;
                dr["PRJT_NAME"] = sPrjtName;
                dr["AUTO_WH_STCK_FLAG"] = sAutoWhStckFlag;
                dr["STCK_ADJ_FLAG"] = sStckAdjFlag;
                dr["PILOT_PROD_DIVS_CODE"] = sPilotProdDivsCode;  // 2023.01.31[C20220718 - 000125] 1bldg - 재고조사계면에 생산유형구분(P / X / L)을 추가합니다

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_COMPARE_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListCompareDetail, dtRslt, FrameOperation);
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
                //if (dgListStock.GetRowCount() < 1)
                //{
                //    Util.MessageValidation("SFU2946"); //재고조사 파일을 먼저 선택 해 주세요.
                //    return;
                //}

                string sMonth = Util.GetCondition(ldpMonthUpload);
                string sArea = Util.GetCondition(cboAreaUpload, "동은 필수입니다.");
                if (sArea.Equals("")) return;
                Int16 iCnt = 0;
                if (!Util.GetCondition(cboStockSeqUpload, "SFU2958").Equals(""))
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

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EXCEL_UPLOAD", "RQSTDT", "RSLTDT", RQSTDT);

                Util.AlertInfo("SFU1270");  //저장되었습니다.
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

        #region 전산재고 제외/제외취소 버튼 - 한번에 단일 공정 Lot만 제외 처리
        private bool ValidateSingleProcess()
        {
            string sProcName = string.Empty;
            DataTable dtListShot = DataTableConverter.Convert(dgListShot.ItemsSource);
            foreach (DataRow _iRow in dtListShot.Rows)
            {
                if (_iRow["CHK"].IsTrue())
                {
                    if (!sProcName.Equals(string.Empty) && !sProcName.Equals(_iRow["PROCNAME"].ToString()))
                    {
                        Util.MessageValidation("SFU9912"); // 단일 공정 LOT을 선택해 주세요.
                        return false;
                    }
                    sProcName = _iRow["PROCNAME"].ToString();
                }
            }
            return true;
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

                //RQSTDT
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
                    if (dtRSLT.Rows[i]["CHK"].IsTrue())
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

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

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
                //INDATA
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
                    if (dtRSLT.Rows[i]["CHK"].IsTrue())
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

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

                        SetListStock();
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
            catch (ArgumentException ex)
            {
                Util.MessageValidation("SFU4215");
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

                //RQSTDT
                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].IsTrue())
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

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

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

        #region 전산재고 [제외, 제외취소] 버튼 - 제외 버튼 권한(AREA)
        private bool GetExcludAuthorityArea(C1ComboBox cboArea)
        {
            bool bRet = false;

            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "INVENTORY_EXCLUDE_AREA";
            dr["CBO_CODE"] = (string)cboArea.SelectedValue;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }

            return bRet;
        }
        #endregion

        #region 전산재고 [제외, 제외취소] 버튼 - 제외 버튼 권한(사용자ID)
        private void GetExcludAuthorityUserIDChk(C1ComboBox cboArea)
        {
            try
            {
                DataTable inTableUserID = new DataTable();
                inTableUserID.Columns.Add("LANGID", typeof(string));
                inTableUserID.Columns.Add("AREAID", typeof(string));
                inTableUserID.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTableUserID.Columns.Add("ATTR2", typeof(string));
                inTableUserID.Columns.Add("USE_FLAG", typeof(string));

                DataRow newRow = inTableUserID.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = (string)cboArea.SelectedValue;
                newRow["COM_TYPE_CODE"] = "INVENTORY_EXCLUDE_USER"; // 제외 버튼 권한 담당자
                newRow["ATTR2"] = LoginInfo.USERID;
                newRow["USE_FLAG"] = "Y";

                inTableUserID.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", inTableUserID);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _isExcludeAuthority = true;
                }
                else
                {
                    _isExcludeAuthority = false;

                    tblExcludeNote_SNAP.Visibility = Visibility.Collapsed;
                    txtExcludeNote_SNAP.Visibility = Visibility.Collapsed;
                    btnExclude_SNAP.Visibility = Visibility.Collapsed;
                    btnExclude_SNAP_Cancel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _isExcludeAuthority = true;

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
                    SetStckCntGrShotCombo(cboStckCntGrShot, cboAreaShot);
                }));

                ShowBtnCloseCancel(ldpMonthShot);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboAreaUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetStckCntGrShotCombo(cboStckCntGrUpload, cboAreaUpload);
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
                    SetStckCntGrShotCombo(cboStckCntGrCompare, cboAreaCompare);
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

        #region 그룹선택 - SelectedValueChanged
        private void cboStckCntGrShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetProcessCombo(cboProcShot, cboAreaShot, cboStckCntGrShot);
            }));
        }

        private void cboStckCntGrUpload_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetProcessCombo(cboProcUpload, cboAreaUpload, cboStckCntGrUpload);
            }));
        }

        private void cboStckCntGrCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetProcessCombo(cboProcCompare, cboAreaCompare, cboStckCntGrCompare);
            }));
        }
        #endregion

        #region 공정 Combo 조회
        private void SetProcessCombo(MultiSelectionBox cboProc, C1ComboBox cboArea, C1ComboBox cboStckCntGr)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue ?? null;
                dr["STCK_CNT_GR_CODE"] = cboStckCntGr.SelectedValue ?? null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProc.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    cboProc.CheckAll();
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        #endregion

        #region 공정 선택 - SelectionChanged
        private void cboProcShot_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetEqsgCombo(cboEqsgShot, cboProcShot, cboAreaShot);
            }));
        }

        private void cboProcUpload_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetEqsgCombo(cboEqsgUpload, cboProcUpload, cboAreaUpload);
            }));
        }

        private void cboProcCompare_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                SetEqsgCombo(cboEqsgCompare, cboProcCompare, cboAreaCompare);
            }));
        }
        #endregion

        #region 라인 Combo 조회
        private void SetEqsgCombo(MultiSelectionBox cboEqsg, MultiSelectionBox cboProc, C1ComboBox cboArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = cboProc.SelectedItemsToString ?? null;
                dr["AREAID"] = cboArea.SelectedValue ?? null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_EQSG_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEqsg.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    cboEqsg.CheckAll();
                }
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

            if (e.Cell.Column.IsReadOnly == false)
            {
                DataTableConverter.SetValue(dgListStock.Rows[e.Cell.Row.Index].DataItem, "CHK", true);

                //row 색 바꾸기
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

        #region NERP 대응 (2025.04.03)

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

        // 차수 마감 취소 
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

        #region ChkCmplFlag : 마감여부 조회 로직 
        private bool ChkCmplFlag(string sStckYm, string sStckArea, string sSeqno)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = sStckYm;
                dr["AREAID"] = sStckArea;
                dr["STCK_CNT_SEQNO"] = sSeqno;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_ORD", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Rows[0]["STCK_CNT_CMPL_FLAG"].ToString().Equals("Y"))
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


        #endregion


    }
}
