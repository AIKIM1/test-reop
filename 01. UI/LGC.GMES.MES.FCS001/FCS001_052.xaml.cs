/*************************************************************************************
 Created Date : 2020.12.10
      Creator : Kang Dong Hee
   Decription : 요청 데이타 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.10  NAME : Initial Created
  2021.03.31  KDH : 각 Sheet별 Row No 출력되도록 수정 대응
  2021.04.08  KDH : Tray ID입력 후 조회 시 GetValue에 대한 에러 발생 조치 및 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 수정.
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2021.05.06  KDH : CSTID -> LOTID로 변경
  2022.12.20  LJM : Degas 전 데이터 Tab, Degas 후 데이터 Tab SUBLOTID 검색 기능 추가
  2023.01.31  HJW : 디가스 전, 후 데이터 부족한 부분 추가
  2023.02.05  HJW : DataTable Primary 에러나는 부분 수정
  2023.02.13  HJW : 디가스 후 데이터 조회 오류 수정
  2023.02.16  HJW : 최희곤책임 요청사항 적용
  2023.03.12  HJW : Node 필터링 추가
  2023.03.27  HJW : Node 필터에 따른 헤더표기 변경
  2023.04.03  HJW : 최희곤책임 요청사항 적용
  2023.04.17  suyong.kim : 보정dOCV 관련 컬럼 추가
  2023.06.12  최도훈 : Degas 전 데이터 JIG PP 추가
  2023.08.18  이의철 : 일괄 데이터 조회 탭 추가 
  2023.09.12  조영대 : Message 수정
  2023.09.20  이의철 : 일괄 데이터 조회 조건 추가  - DA_SEL_TRAY_NO_CELL_ID, DA_SEL_IROCV_INFO_BY_CELL_ID
  2023.09.28  손동혁 : Message 수정
  2023.10.06  손동혁 : Degas 전 데이터 및 Degas 후 데이터 LOTID 공백이 아니고 6자리 미만 입력시 팝업창 추가
  2023.10.12  김최일 : pallet cell sel 탭 추가(물류이관/출하완료 된 pallet 조회)
  2023.10.18  김최일 : pallet cell sel 탭 : 초기화 조회 건수 100개로 수정
  2023.10.19  이준영 : E20230811-001339 [업무혁신.생산PI.활성화/MES팀] GMES(활성화) 요청데이터 조회 UI 추가 요청의 건 #JF/PP, Nomal #1,2 , SHIP #1,2,3,4 추가
  2023.11.21  이의철 : 일괄 데이터 조회 - 해더 자동생성 변경
  2023.11.22  이의철 : 일괄 데이터 조회 - DA_SEL_CELL_INFO_BY_CELL_ID  수정 , Cell 정보조회와 동일하게 수정
  2023.11.23  이의철 : 일괄 데이터 조회 - DA_SEL_HPCD_INFO_BY_LOT 해더값 원복
  2023.11.27  이의철 : 일괄 데이터 조회 - 조회 조건 콥보 All 제거
  2023.12.10  이의철 : 일괄 데이터 조회 - IV_CA_VAL 추가
  2023.12.12  이의철 : 일괄 데이터 조회 - DA_SEL_IROCV_INFO_BY_CELL_ID , DCIR_VAL -> IR_VAL 변경
  2023.12.15  이의철 : 일괄 데이터 조회 - cboSearchItems selectedchange 이벤트 삭제 -> NA 이외 사이트 오류 발생으로 인해
  2023.12.15  최도훈 : DCIR_VAL '직류저항'으로 수정, IR_VAL(절연저항 값) 추가
  2024.01.10  조영대 : 특성 재작업 횟수 기본측정데이터 EOL 하단 체크로 이동
  2024.01.24  조영대 : 입력된 Cell과 조회된 Cell의 순서 동기화
  2024.02.19  권순범 : DA_SEL_NEW_CELL - LANGID Indata 추가
  2024.02.21  이해령 : Degas 전,후 판정 공정 추가
  2024.05.07  남형희 : E20240319-000687 요청데이터 조회 UI 추가 요청 Degas 전 - J/G Rest, J/G RestHeating  추가
  2024.06.12  윤홍기 : Slope OCV 항목 추가
  2024.09.10  김상영 : E20240816-001103 요청 데이터 조회 컬럼 Sorting 비정상 처리 개선을 위한 타입변경 (text -> double)
  2025.04.01  양윤호 : Catch-Up  AVGVLTG_STDEV 항목 추가
  2025.04.04  복현수 : MES 2.0 전환 오류 수정, AVGVLTG_STDEV 항목은 캐치업 대상이라 임시 주석처리
  2025.04.09  복현수 : 충전/방전 정상조회 되도록 DA와 UI 칼럼 동기화 (UI에 3차 충전/방전 추가), 셀 입력하여 조회시 PP 조회 오류 수정, SOCV 칼럼 로직 오류 수정
  2025.06.10  복현수 : 요청데이터 조회 수정 취합본 적용 (허경님 요청)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.ObjectModel;
using System.Configuration;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// 요청데이터조회
    /// </summary>
    public partial class FCS001_052 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        bool chkbeforetab = false;
        bool chkaftertab = false;
        bool chkpocv = false;
        bool chkdocv = false;
        bool chksocv = false;       // 2024.06.12 YHK : Slope OCV 항목 추가

        private bool BATCH_DATA_INQUIRY = false; //일괄 데이터 조회 탭 추가

        public class MenuItem
        {
            public MenuItem()
            {
                this.Items = new ObservableCollection<MenuItem>();
            }

            public string Title { get; set; }

            public string Tag { get; set; }

            public bool IsChecked { get; set; }


            public ObservableCollection<MenuItem> Items { get; set; }
        }

        private DataTable dtTemp = new DataTable();
        private DataTable _dtHeader;

        private DataTable dtRsltCell;
        private DataTable dtRsltHpcd;
        private DataTable dtRsltPre;
        private DataTable dtRsltHigh;
        private DataTable dtRsltChgBefore;
        private DataTable dtRsltOcvBefore;
        private DataTable dtRsltDegas;
        private DataTable dtRsltPp;
        private DataTable dtRsltRest;
        private DataTable dtRsltRestHeating;
        private DataTable dtRsltNomal1;
        private DataTable dtRsltNomal2;
        private DataTable dtRsltJudgBefore;

        private DataTable dtRsltEolRework;
        private DataTable dtRsltAfter;
        private DataTable dtRslt0;
        private DataTable dtRslt1;
        private DataTable dtRslt2;
        private DataTable dtRslt2_1;
        private DataTable dtRslt3;
        private DataTable dtRslt3_1;
        private DataTable dtRslt4;
        private DataTable dtRslt5;
        private DataTable dtRslt6;
        private DataTable dtRslt7;
        private DataTable dtRslt8;
        private DataTable dtRslt9;
        private DataTable dtRsltShip;
        private DataTable dtRsltJudgAfter;

        private string sLTCColumnName = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_052()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //BATCH_DATA_INQUIRY = IsBATCH_DATA_INQUIRY(); //일괄 데이터 조회 탭 추가
            BATCH_DATA_INQUIRY = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_052_BATCH_DATA_INQUIRY"); //일괄 데이터 조회 탭 추가

            lblHelp.Text = ObjectDic.Instance.GetObjectName("UC_0003");  //※ 기본측정데이터 선택조회 시 약 1분이상 소요됩니다. (조회데이터 10,000건 기준)
            DataGridRowAdd(dgBeforeSerchList, Convert.ToInt32(100));
            DataGridRowAdd(dgAfterSerchList, Convert.ToInt32(100));

            //Combo Setting
            InitCombo(); //일괄 데이터 조회 탭 추가 

            InitControl(); //일괄 데이터 조회 탭 추가

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거


        }

        //일괄 데이터 조회 탭 추가
        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (BATCH_DATA_INQUIRY.Equals(true))
            {
                string[] sFilter = { "FORMLGS_LARGE_TRAY_CELL_COMBO_CODE" };
                _combo.SetCombo(cboSearchItems, CommonCombo_Form.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter);
            }
        }

        //일괄 데이터 조회 탭 추가
        private void InitControl()
        {
            if (BATCH_DATA_INQUIRY.Equals(true))
            {
                this.tpLotTrayCell.Visibility = Visibility.Visible;

            }
            else
            {
                this.tpLotTrayCell.Visibility = Visibility.Hidden;
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
        #endregion

        #region Event

        private void btnSearchLot_Click(object sender, RoutedEventArgs e)
        {
            Button buttion = sender as Button;
            GetProdLotList(buttion.Name);
        }

        private void btnSearchbefore_Click(object sender, RoutedEventArgs e)
        {
            GetBeforeList();
        }

        private void btnSearchAfter_Click(object sender, RoutedEventArgs e)
        {
            GetAfterList();
        }

        private void btnCellReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCellData);
            if (Convert.ToInt32(txtRowCntCell.Value) > 100000)
            {
                Util.AlertInfo("FM_ME_0242");  //최대 100,000 까지 입력할 수 있습니다.
                return;
            }
            DataGridRowAdd(dgCellData, Convert.ToInt32(txtRowCntCell.Value));
        }

        private void btnSearchCell_Click(object sender, RoutedEventArgs e)
        {
            GetCellList();
        }

        private void btnSearchBWIP_Click(object sender, RoutedEventArgs e)
        {
            GetBWIPList();
        }

        private void btnTCReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgTrayCell);
            if (Convert.ToInt32(txtTCRowCntCell.Value) > 100000)
            {
                Util.AlertInfo("FM_ME_0242");  //최대 100,000 까지 입력할 수 있습니다.
                return;
            }
            DataGridRowAdd(dgTrayCell, Convert.ToInt32(txtTCRowCntCell.Value));

            Util _Util = new Util();
            string[] sColumnName = new string[] { "LOTID" };
            string[] LColumnName = new string[] { "CSTID" };
            _Util.SetDataGridMergeExtensionCol(dgTrayCell, sColumnName, DataGridMergeMode.NONE);
            _Util.SetDataGridMergeExtensionCol(dgTrayCell, LColumnName, DataGridMergeMode.NONE);

        }

        private void btnTCSearch_Click(object sender, RoutedEventArgs e)
        {
            GetTCList();
        }

        //20231012
        private void btnPCReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgPalletCell);
            if (Convert.ToInt32(txtPCRowCntCell.Value) > 100)
            {
                Util.AlertInfo("FM_ME_0520");  //최대 100 까지 입력할 수 있습니다.
                return;
            }
            DataGridRowAdd(dgPalletCell, Convert.ToInt32(txtPCRowCntCell.Value));

            Util _Util = new Util();
            string[] sColumnName = new string[] { "PALLETID" };
            _Util.SetDataGridMergeExtensionCol(dgPalletCell, sColumnName, DataGridMergeMode.NONE);

        }

        //20231012
        private void btnPCSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPCList();
        }

        private void btnDegasBCellReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgBeforeSerchList);
            if (Convert.ToInt32(txtDegaasRowCntCell.Value) > 100000)
            {
                Util.AlertInfo("FM_ME_0242");  //최대 100,000 까지 입력할 수 있습니다.
                return;
            }
            DataGridRowAdd(dgBeforeSerchList, Convert.ToInt32(txtDegaasRowCntCell.Value));
        }

        private void btnDegCellReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgAfterSerchList);
            if (Convert.ToInt32(txtDegRowCntCell.Value) > 100000)
            {
                Util.AlertInfo("FM_ME_0242");  //최대 100,000 까지 입력할 수 있습니다.
                return;
            }
            DataGridRowAdd(dgAfterSerchList, Convert.ToInt32(txtDegRowCntCell.Value));
        }

        private void txtLotbefore_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotbefore.Text)) && (e.Key == Key.Enter))
            {
                btnSearchbefore_Click(null, null);
            }
        }

        private void txtLotAfter_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotAfter.Text)) && (e.Key == Key.Enter))
            {
                btnSearchAfter_Click(null, null);
            }
        }

        private void txtLotBWIP_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotBWIP.Text)) && (e.Key == Key.Enter))
            {
                btnSearchBWIP_Click(null, null);
            }
        }

        private void txtTCRowCntCell_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTCRowCntCell.Value.ToString())) && (e.Key == Key.Enter))
            {
                btnTCReset_Click(null, null);
            }
        }

        //20231012
        private void txtPCRowCntCell_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtPCRowCntCell.Value.ToString())) && (e.Key == Key.Enter))
            {
                btnPCReset_Click(null, null);
            }
        }

        private void dgBWIP_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "TOTL_LOT_QTY"
                        || e.Cell.Column.Name.ToString() == "TOTL_INPUT_SUBLOT_QTY"
                        || e.Cell.Column.Name.ToString() == "TOTL_WIP_SUBLOT_QTY")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);
                    }

                    if (e.Cell.Column.Name.ToString() == "SPECIALTRAY" || e.Cell.Column.Name.ToString() == "SPECIALCELL")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.MistyRose);
                    }
                }
            }));
        }

        private void dgCellData_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "SUBLOTID")
                    {
                        e.Cell.Presenter.IsEnabled = true;
                    }
                    else
                    {
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void dgCellData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column.HeaderPresenter == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }

                    else
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        //20210408 Tray ID입력 후 조회 시 GetValue에 대한 에러 발생 조치 START
        private void dgTrayCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "LOTID" || e.Cell.Column.Name.ToString() == "CSTID") //20221207
                    {
                        e.Cell.Presenter.IsEnabled = true;
                    }
                    else
                    {
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }
        //20210408 Tray ID입력 후 조회 시 GetValue에 대한 에러 발생 조치 END

        private void dgTrayCell_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //20210408 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 START
                if (e.Column.HeaderPresenter == null)
                {
                    return;
                }
                //20210408 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 END

                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CSTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        //20231012 Tray ID입력 후 조회 시 GetValue에 대한 에러 발생 조치 START
        private void dgPalletCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "PALLETID")
                    {
                        e.Cell.Presenter.IsEnabled = true;
                    }
                    else
                    {
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        //20231012 Tray ID입력 후 조회 시 GetValue에 대한 에러 발생 조치 END
        private void dgPalletCell_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //20210408 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 START
                if (e.Column.HeaderPresenter == null)
                {
                    return;
                }
                //20210408 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 END

                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PALLETID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgBefore_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Text = (e.Row.Index + 1 - dgBefore.TopRows.Count).ToString();
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgAfter_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e) //2021.03.31 : 각 Sheet별 Row No 출력되도록 수정 대응
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Text = (e.Row.Index + 1 - dgAfter.TopRows.Count).ToString();
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgCellData_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e) //2021.03.31 : 각 Sheet별 Row No 출력되도록 수정 대응
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgCellData.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgTrayCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e) //2021.03.31 : 각 Sheet별 Row No 출력되도록 수정 대응
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgTrayCell.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgBWIP_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e) //2021.03.31 : 각 Sheet별 Row No 출력되도록 수정 대응
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index < dgBWIP.TopRows.Count)
            {
                tb.Text = string.Empty;
            }
            else
            {
                tb.Text = (e.Row.Index + 1 - dgBWIP.TopRows.Count).ToString();
            }
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        //20231012
        private void dgPalletCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e) //2021.03.31 : 각 Sheet별 Row No 출력되도록 수정 대응
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgTrayCell.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgBeforeSerchList_ClipboardPasted(object sender, DataObjectPastingEventArgs e)
        {
            string cell = Clipboard.GetText().ToUpper().Replace(" ", "").Replace("\r", ",").Replace("\n", ",");
            string[] cells = cell.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            cells = cells.Distinct().ToArray();
            DataTable dtTemp = new DataTable();
            dtTemp.Columns.Add("SUBLOTID");
            foreach (string s in cells)
            {
                DataRow dr = dtTemp.NewRow();
                dr["SUBLOTID"] = s;
                dtTemp.Rows.Add(dr);
            }

            Util.gridClear(dgBeforeSerchList);
            Util.GridSetData(dgBeforeSerchList, dtTemp, FrameOperation, true);
        }

        private void dgAfterSerchList_ClipboardPasted(object sender, DataObjectPastingEventArgs e)
        {
            string cell = Clipboard.GetText().ToUpper().Replace(" ", "").Replace("\r", ",").Replace("\n", ",");
            string[] cells = cell.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            cells = cells.Distinct().ToArray();
            DataTable dtTemp = new DataTable();
            dtTemp.Columns.Add("SUBLOTID");
            foreach (string s in cells)
            {
                DataRow dr = dtTemp.NewRow();
                dr["SUBLOTID"] = s;
                dtTemp.Rows.Add(dr);
            }

            Util.gridClear(dgAfterSerchList);
            Util.GridSetData(dgAfterSerchList, dtTemp, FrameOperation, true);
        }

        private void nudPOCVS_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0 || e.NewValue == 0 || Double.IsNaN(e.NewValue) || e.NewValue > Convert.ToInt16(nudPOCVE.Value) || e.OldValue > Convert.ToInt16(nudPOCVE.Value))
                return;

            if (chkpocv)
            {
                if (Double.IsNaN(e.OldValue))
                {
                    for (int i = 1; i <= 10; i++)
                        for (int j = dgAfter.Columns["DEDICATE_OCV" + i + "_LOTID"].Index; j <= dgAfter.Columns["DEDICATE_OCV" + i].Index; j++)
                            dgAfter.Columns[j].Visibility = Visibility.Collapsed;
                }
                else
                {
                    for (int i = dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(e.OldValue) + "_LOTID"].Index; i <= dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVE.Value)].Index; i++)
                        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
                for (int i = dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(e.NewValue) + "_LOTID"].Index; i <= dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVE.Value)].Index; i++)
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
            }
        }

        private void nudPOCVE_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0 || e.NewValue == 0 || Double.IsNaN(e.NewValue))
                return;

            if (chkpocv)
            {
                if (Double.IsNaN(e.OldValue))
                {
                    for (int i = 1; i <= 10; i++)
                        for (int j = dgAfter.Columns["DEDICATE_OCV" + i + "_LOTID"].Index; j <= dgAfter.Columns["DEDICATE_OCV" + i].Index; j++)
                            dgAfter.Columns[j].Visibility = Visibility.Collapsed;
                }
                else
                {
                    for (int i = dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(e.OldValue)].Index; i++)
                        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
                for (int i = dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(e.NewValue)].Index; i++)
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
            }
        }

        private void nudDOCVS_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0 || e.NewValue == 0 || Double.IsNaN(e.NewValue) || e.NewValue > Convert.ToInt16(nudDOCVE.Value) || e.OldValue > Convert.ToInt16(nudDOCVE.Value))
                return;

            if (chkdocv)
            {
                if (Double.IsNaN(e.OldValue))
                {
                    for (int i = 1; i <= 10; i++)
                        for (int j = dgAfter.Columns["DOCV" + i + "_LOTID"].Index; j <= dgAfter.Columns["DOCV" + i].Index; j++)
                            dgAfter.Columns[j].Visibility = Visibility.Collapsed;
                }
                else
                {
                    for (int i = dgAfter.Columns["DOCV" + Convert.ToInt16(e.OldValue) + "_LOTID"].Index; i <= dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVE.Value)].Index; i++)
                        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
                for (int i = dgAfter.Columns["DOCV" + Convert.ToInt16(e.NewValue) + "_LOTID"].Index; i <= dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVE.Value)].Index; i++)
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
            }
        }

        private void nudDOCVE_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0 || e.NewValue == 0 || Double.IsNaN(e.NewValue))
                return;

            if (chkdocv)
            {
                if (Double.IsNaN(e.OldValue))
                {
                    for (int i = 1; i <= 10; i++)
                        for (int j = dgAfter.Columns["DOCV" + i + "_LOTID"].Index; j <= dgAfter.Columns["DOCV" + i].Index; j++)
                            dgAfter.Columns[j].Visibility = Visibility.Collapsed;
                }
                else
                {
                    for (int i = dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DOCV" + Convert.ToInt16(e.OldValue)].Index; i++)
                        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
                for (int i = dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DOCV" + Convert.ToInt16(e.NewValue)].Index; i++)
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
            }
        }

        private void nudSOCVS_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0 || e.NewValue == 0 || Double.IsNaN(e.NewValue) || e.NewValue > Convert.ToInt16(nudSOCVE.Value) || e.OldValue > Convert.ToInt16(nudSOCVE.Value))
                return;

            if (chksocv)
            {
                if (Double.IsNaN(e.OldValue))
                {
                    for (int i = 1; i <= 10; i++)
                        for (int j = dgAfter.Columns["SOCV" + i + "_LOTID"].Index; j <= dgAfter.Columns["SOCV" + i].Index; j++)
                            dgAfter.Columns[j].Visibility = Visibility.Collapsed;
                }
                else
                {
                    for (int i = dgAfter.Columns["SOCV" + Convert.ToInt16(e.OldValue) + "_LOTID"].Index; i <= dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVE.Value)].Index; i++)
                        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
                for (int i = dgAfter.Columns["SOCV" + Convert.ToInt16(e.NewValue) + "_LOTID"].Index; i <= dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVE.Value)].Index; i++)
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
            }
        }

        private void nudSOCVE_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0 || e.NewValue == 0 || Double.IsNaN(e.NewValue))
                return;

            if (chksocv)
            {
                if (Double.IsNaN(e.OldValue))
                {
                    for (int i = 1; i <= 10; i++)
                        for (int j = dgAfter.Columns["SOCV" + i + "_LOTID"].Index; j <= dgAfter.Columns["SOCV" + i].Index; j++)
                            dgAfter.Columns[j].Visibility = Visibility.Collapsed;
                }
                else
                {
                    for (int i = dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["SOCV" + Convert.ToInt16(e.OldValue)].Index; i++)
                        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
                for (int i = dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["SOCV" + Convert.ToInt16(e.NewValue)].Index; i++)
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
            }
        }

        private void bfCheck_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tvBefore, typeof(CheckBox), ref items);
            String[] temp = chk.Tag.ToString().Split('/');
            foreach (CheckBox item in items)
            {
                //선택한 체크박스가 TOP 이면 자식들을 체크한다.
                String[] key = item.Tag.ToString().Split('/');
                //Tag : PKEY_VAL / KEY_VAL
                if (temp[0].Equals("TOP"))
                {
                    if (key[0].Equals(temp[1]))
                    {
                        item.IsChecked = true;
                    }
                }
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_C"))
            {
                for (int i = dgBefore.Columns["SUBLOTID"].Index; i <= dgBefore.Columns["PRODID"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_CELL"))
                    dgBefore.Columns[dgBefore.Columns["SUBLOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_PKGLOT"))
                    dgBefore.Columns[dgBefore.Columns["PROD_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_LINE"))
                    dgBefore.Columns[dgBefore.Columns["EQSGID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_PROD"))
                    dgBefore.Columns[dgBefore.Columns["PRODID"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_91"))
            {
                for (int i = dgBefore.Columns["PRE_LOTID"].Index; i <= dgBefore.Columns["PREAGING"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_91_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["PRE_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_CELL"))
                    dgBefore.Columns[dgBefore.Columns["PRE_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_EQP"))
                    dgBefore.Columns[dgBefore.Columns["PRE_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["PRE_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["PRE_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["PRE_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["PREAGING"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_91_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["PRE_STARTTIME"].Index].Visibility = Visibility.Visible;

            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_U1"))
            {
                for (int i = dgBefore.Columns["HPCD_LOTID"].Index; i <= dgBefore.Columns["JIG_PRESS_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_U1_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_CELL"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_EQP"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_STARTTIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_CURR"))
                    dgBefore.Columns[dgBefore.Columns["CURNT_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_VOLT"))
                    dgBefore.Columns[dgBefore.Columns["VLTG_VAL"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DB_U1_INIVOLT"))
                //    dgBefore.Columns[dgBefore.Columns["INI_VLTG_VALUE"].Index].Visibility = Visibility.Visible; // 2023.11.13 초기전압 추가
                //if (temp[1].Equals("DB_U1_ENDVOLT"))
                //    dgBefore.Columns[dgBefore.Columns["END_VLTG_VALUE"].Index].Visibility = Visibility.Visible; // 2023.11.13 종료전압 추가
                if (temp[1].Equals("DB_U1_DELTA"))
                    dgBefore.Columns[dgBefore.Columns["DELTA_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_U1_PRESS"))
                    dgBefore.Columns[dgBefore.Columns["JIG_PRESS_VAL"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J1"))
            {
                for (int i = dgBefore.Columns["CHG1_LOTID"].Index; i <= dgBefore.Columns["CHG3_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J1_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_CAPA"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_AVGVOLT"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_AVG_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_AVG_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_AVG_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_ENDVOLT"))
                {

                    dgBefore.Columns[dgBefore.Columns["CHG1_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J1_ENDCURR"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_END_CURR_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_END_CURR_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_END_CURR_VAL"].Index].Visibility = Visibility.Visible;
                }
                //if (temp[1].Equals("DB_J1_AVGTMPR"))
                //{
                //    dgBefore.Columns[dgBefore.Columns["CHG1_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                //    dgBefore.Columns[dgBefore.Columns["CHG2_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                //    dgBefore.Columns[dgBefore.Columns["CHG3_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                //}

                if (temp[1].Equals("DB_J1_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                }

                if (temp[1].Equals("DB_J1_PRESS"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG2_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["CHG3_PRES_VAL"].Index].Visibility = Visibility.Visible;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_41"))
            {
                for (int i = dgBefore.Columns["HIGH_LOTID"].Index; i <= dgBefore.Columns["HIGHAGING"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_41_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_CELL"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_EQP"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["HIGHAGING"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_41_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_STARTTIME"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J3"))
            {
                for (int i = dgBefore.Columns["OCV1_LOTID"].Index; i <= dgBefore.Columns["OCV3_JIG_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J3_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_OCV"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_JIG_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_JIG_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_JIG_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J3_PRESS"))
                {

                    dgBefore.Columns[dgBefore.Columns["OCV1_JIG_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV2_JIG_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["OCV3_JIG_PRES_VAL"].Index].Visibility = Visibility.Visible;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_D1"))
            {
                for (int i = dgBefore.Columns["DEGAS_LOTID"].Index; i <= dgBefore.Columns["HOTPRES_PORT_NO"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_D1_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_CELL"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_EQP"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_TIME"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_STARTTIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_WEIGHT"))
                    dgBefore.Columns[dgBefore.Columns["WEIGHT_VALUE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_IV_VAL"))
                    dgBefore.Columns[dgBefore.Columns["IVLTG_VALUE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_DCIR_VAL"))
                    dgBefore.Columns[dgBefore.Columns["DCIR_VALUE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_CHAMBERNO"))
                    dgBefore.Columns[dgBefore.Columns["CHAMBER_LOCATION_NO"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_NESTNO"))
                    dgBefore.Columns[dgBefore.Columns["CHAMBER_NEST_NO"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_SEALINGNO"))
                    dgBefore.Columns[dgBefore.Columns["MAIN_SEAL_LOCATION_NO"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_COMPUTE"))
                    dgBefore.Columns[dgBefore.Columns["COMPUTE_VALUE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_FILLINGLOSS"))
                    dgBefore.Columns[dgBefore.Columns["FILLING_LOSS"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_HOTPRES_WEIGHT"))
                    dgBefore.Columns[dgBefore.Columns["PRESS_LOAD_CELL_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_D1_PORT"))
                    dgBefore.Columns[dgBefore.Columns["HOTPRES_PORT_NO"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J7"))
            {
                for (int i = dgBefore.Columns["PP1_LOTID"].Index; i <= dgBefore.Columns["PP3_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J7_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J7_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J7_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J7_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J7_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J7_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J7_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                //if (temp[1].Equals("DB_J7_AVGTMPR"))
                //{
                //    dgBefore.Columns[dgBefore.Columns["PP1_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                //    dgBefore.Columns[dgBefore.Columns["PP2_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                //    dgBefore.Columns[dgBefore.Columns["PP3_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                //}

                if (temp[1].Equals("DB_J7_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                }

                if (temp[1].Equals("DB_J7_PRESS"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP2_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["PP3_PRES_VAL"].Index].Visibility = Visibility.Visible;
                }
            }



            if (temp[0].Equals("TOP") && temp[1].Equals("DB_31"))
            {
                for (int i = dgBefore.Columns["NOMAL_LOTID"].Index; i <= dgBefore.Columns["NOMAL_STARTTIME"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;

                }
            }
            else
            {
                if (temp[1].Equals("DB_31_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_CELL"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_EQP"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMALAGING"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_31_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_STARTTIME"].Index].Visibility = Visibility.Visible;
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_32"))
            {
                for (int i = dgBefore.Columns["NOMAL2_LOTID"].Index; i <= dgBefore.Columns["NOMAL2_STARTTIME"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_32_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_CELL"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_EQP"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2AGING"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DB_32_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_STARTTIME"].Index].Visibility = Visibility.Visible;
            }
            // 2024.02.20 판정 추가
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_B1"))
            {
                for (int i = dgBefore.Columns["JUDG1_LOTID"].Index; i <= dgBefore.Columns["JUDG6_TIME"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_B1_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_B1_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_B1_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_B1_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_B1_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_B1_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_TIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_TIME"].Index].Visibility = Visibility.Visible;
                }
            }
            //if (temp[0].Equals("TOP") && temp[1].Equals("DB_J5"))
            //{
            //    for (int i = dgBefore.Columns["RESTHEATING1_LOTID"].Index; i <= dgBefore.Columns["RESTHEATING3_PRES_VAL"].Index; i++)
            //    {
            //        dgBefore.Columns[i].Visibility = Visibility.Visible;
            //    }
            //}
            //else
            //{
            //    if (temp[1].Equals("DB_J5_TRAY"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_LOTID"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_LOTID"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_LOTID"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_CELL"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_CSTSLOT"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_CSTSLOT"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_CSTSLOT"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_EQP"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_EQP_ID"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_EQP_ID"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_EQP_ID"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_ROUTE"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_ROUTE"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_ROUTE"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_ROUTE"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_GRADE"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_STARTTIME"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_STARTTIME"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_STARTTIME"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_STARTTIME"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_ENDTIME"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_ENDTIME"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_ENDTIME"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_ENDTIME"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DB_J5_AVGTMPR"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
            //    }

            //    if (temp[1].Equals("DB_J5_TEMP"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_TEMP_VAL"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_TEMP_VAL"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_TEMP_VAL"].Index].Visibility = Visibility.Visible;
            //    }

            //    if (temp[1].Equals("DB_J5_PRESS"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_PRES_VAL"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_PRES_VAL"].Index].Visibility = Visibility.Visible;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_PRES_VAL"].Index].Visibility = Visibility.Visible;
            //    }
            //}

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J6"))
            {
                for (int i = dgBefore.Columns["REST1_LOTID"].Index; i <= dgBefore.Columns["REST3_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J6_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_ENDTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_ENDTIME"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_ENDTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DB_J6_AVGTMPR"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_AVG_TMPR_VAL"].Index].Visibility = Visibility.Visible;
                }

                if (temp[1].Equals("DB_J6_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_TEMP_VAL"].Index].Visibility = Visibility.Visible;
                }

                if (temp[1].Equals("DB_J6_PRESS"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST2_PRES_VAL"].Index].Visibility = Visibility.Visible;
                    dgBefore.Columns[dgBefore.Columns["REST3_PRES_VAL"].Index].Visibility = Visibility.Visible;
                }
            }

            dgBefore.TopRowHeaderMerge();
        }

        private void bfCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tvBefore, typeof(CheckBox), ref items);
            String[] temp = chk.Tag.ToString().Split('/');
            foreach (CheckBox item in items)
            {
                String[] key = item.Tag.ToString().Split('/');
                //Tag : PKEY_VAL / KEY_VAL
                if (temp[0].Equals("TOP"))
                {
                    if (key[0].Equals(temp[1]))
                    {
                        item.IsChecked = false;
                    }
                }
                if (temp[0].Equals(key[1]))
                {
                    if (key[0].Equals("TOP"))
                    {
                        item.Unchecked -= new RoutedEventHandler(bfCheck_Unchecked);
                        item.IsChecked = false;
                        item.Unchecked += new RoutedEventHandler(bfCheck_Unchecked);
                    }
                }
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_C"))
            {
                for (int i = dgBefore.Columns["SUBLOTID"].Index; i <= dgBefore.Columns["PRODID"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_CELL"))
                    dgBefore.Columns[dgBefore.Columns["SUBLOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_PKGLOT"))
                    dgBefore.Columns[dgBefore.Columns["PROD_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_LINE"))
                    dgBefore.Columns[dgBefore.Columns["EQSGID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_PROD"))
                    dgBefore.Columns[dgBefore.Columns["PRODID"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_91"))
            {
                for (int i = dgBefore.Columns["PRE_LOTID"].Index; i <= dgBefore.Columns["PREAGING"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_91_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["PRE_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_CELL"))
                    dgBefore.Columns[dgBefore.Columns["PRE_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_EQP"))
                    dgBefore.Columns[dgBefore.Columns["PRE_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["PRE_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["PRE_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["PRE_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["PRE_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_91_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["PREAGING"].Index].Visibility = Visibility.Collapsed;

            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_U1"))
            {
                for (int i = dgBefore.Columns["HPCD_LOTID"].Index; i <= dgBefore.Columns["JIG_PRESS_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_U1_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_CELL"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_EQP"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["HPCD_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_CURR"))
                    dgBefore.Columns[dgBefore.Columns["CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_VOLT"))
                    dgBefore.Columns[dgBefore.Columns["VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DB_U1_INIVOLT"))
                //    dgBefore.Columns[dgBefore.Columns["INI_VLTG_VALUE"].Index].Visibility = Visibility.Collapsed; // 2023.11.13 초기전압 추가
                //if (temp[1].Equals("DB_U1_ENDVOLT"))
                //    dgBefore.Columns[dgBefore.Columns["END_VLTG_VALUE"].Index].Visibility = Visibility.Collapsed; // 2023.11.13 종료전압 추가
                if (temp[1].Equals("DB_U1_DELTA"))
                    dgBefore.Columns[dgBefore.Columns["DELTA_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_U1_PRESS"))
                    dgBefore.Columns[dgBefore.Columns["JIG_PRESS_VAL"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_K1"))
            {
                for (int i = dgBefore.Columns["CHG1_LOTID"].Index; i <= dgBefore.Columns["CHG3_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J1_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_CAPA"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_AVGVOLT"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_AVG_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_AVG_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_AVG_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_ENDVOLT"))
                {

                    dgBefore.Columns[dgBefore.Columns["CHG1_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J1_ENDCURR"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_END_CURR_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_END_CURR_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_END_CURR_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                //if (temp[1].Equals("DB_J1_AVGTMPR"))
                //{
                //    dgBefore.Columns[dgBefore.Columns["CHG1_AVG_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                //    dgBefore.Columns[dgBefore.Columns["CHG2_AVG_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                //    dgBefore.Columns[dgBefore.Columns["CHG3_AVG_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                //}

                if (temp[1].Equals("DB_J1_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                }

                if (temp[1].Equals("DB_J1_PRESS"))
                {
                    dgBefore.Columns[dgBefore.Columns["CHG1_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG2_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["CHG3_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_41"))
            {
                for (int i = dgBefore.Columns["HIGH_LOTID"].Index; i <= dgBefore.Columns["HIGHAGING"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_41_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_CELL"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_EQP"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["HIGHAGING"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_41_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["HIGH_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J3"))
            {
                for (int i = dgBefore.Columns["OCV1_LOTID"].Index; i <= dgBefore.Columns["OCV3_JIG_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J3_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_OCV"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["OCV1_JIG_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_JIG_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_JIG_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J3_PRESS"))
                {

                    dgBefore.Columns[dgBefore.Columns["OCV1_JIG_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV2_JIG_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["OCV3_JIG_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_D1"))
            {
                for (int i = dgBefore.Columns["DEGAS_LOTID"].Index; i <= dgBefore.Columns["HOTPRES_PORT_NO"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_D1_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_CELL"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_EQP"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_TIME"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["DEGAS_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_WEIGHT"))
                    dgBefore.Columns[dgBefore.Columns["WEIGHT_VALUE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_IV_VAL"))
                    dgBefore.Columns[dgBefore.Columns["IVLTG_VALUE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_DCIR_VAL"))
                    dgBefore.Columns[dgBefore.Columns["DCIR_VALUE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_CHAMBERNO"))
                    dgBefore.Columns[dgBefore.Columns["CHAMBER_LOCATION_NO"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_NESTNO"))
                    dgBefore.Columns[dgBefore.Columns["CHAMBER_NEST_NO"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_SEALINGNO"))
                    dgBefore.Columns[dgBefore.Columns["MAIN_SEAL_LOCATION_NO"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_COMPUTE"))
                    dgBefore.Columns[dgBefore.Columns["COMPUTE_VALUE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_FILLINGLOSS"))
                    dgBefore.Columns[dgBefore.Columns["FILLING_LOSS"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_HOTPRES_WEIGHT"))
                    dgBefore.Columns[dgBefore.Columns["PRESS_LOAD_CELL_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_D1_PORT"))
                    dgBefore.Columns[dgBefore.Columns["HOTPRES_PORT_NO"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J7"))
            {
                for (int i = dgBefore.Columns["PP1_LOTID"].Index; i <= dgBefore.Columns["PP3_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J7_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J7_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J7_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J7_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J7_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J7_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J7_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                //if (temp[1].Equals("DB_J7_AVGTMPR"))
                //{
                //    dgBefore.Columns[dgBefore.Columns["PP1_AVG_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                //    dgBefore.Columns[dgBefore.Columns["PP2_AVG_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                //    dgBefore.Columns[dgBefore.Columns["PP3_AVG_TMPR_VAL"].Index].Visibility = Visibility.Collapsed;
                //}

                if (temp[1].Equals("DB_J7_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                }

                if (temp[1].Equals("DB_J7_PRESS"))
                {
                    dgBefore.Columns[dgBefore.Columns["PP1_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP2_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["PP3_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                }
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_31"))
            {
                for (int i = dgBefore.Columns["NOMAL_LOTID"].Index; i <= dgBefore.Columns["NOMAL_STARTTIME"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;

                }
            }
            else
            {
                if (temp[1].Equals("DB_31_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_CELL"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_EQP"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMALAGING"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_31_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_32"))
            {
                for (int i = dgBefore.Columns["NOMAL2_LOTID"].Index; i <= dgBefore.Columns["NOMAL2_STARTTIME"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_32_TRAY"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_CELL"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_EQP"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_ROUTE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_GRADE"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_ENDTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_AGINGTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2AGING"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DB_32_STARTTIME"))
                    dgBefore.Columns[dgBefore.Columns["NOMAL2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            }
            // 2024.02.20 판정 추가
            if (temp[0].Equals("TOP") && temp[1].Equals("DB_B1"))
            {
                for (int i = dgBefore.Columns["JUDG1_LOTID"].Index; i <= dgBefore.Columns["JUDG6_TIME"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_B1_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_B1_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_B1_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_B1_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_B1_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_B1_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["JUDG1_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG2_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG3_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG4_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG5_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["JUDG6_TIME"].Index].Visibility = Visibility.Collapsed;
                }
            }
            //if (temp[0].Equals("TOP") && temp[1].Equals("DB_J5"))
            //{
            //    for (int i = dgBefore.Columns["RESTHEATING1_LOTID"].Index; i <= dgBefore.Columns["RESTHEATING3_PRES_VAL"].Index; i++)
            //    {
            //        dgBefore.Columns[i].Visibility = Visibility.Collapsed;
            //    }
            //}
            //else
            //{
            //    if (temp[1].Equals("DB_J5_TRAY"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_LOTID"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_LOTID"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_LOTID"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_CELL"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_EQP"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_ROUTE"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_ROUTE"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_ROUTE"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_ROUTE"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_GRADE"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_STARTTIME"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_ENDTIME"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_ENDTIME"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_ENDTIME"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_ENDTIME"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_TEMP"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DB_J5_PRESS"))
            //    {
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING1_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING2_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
            //        dgBefore.Columns[dgBefore.Columns["RESTHEATING3_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
            //    }
            //}

            if (temp[0].Equals("TOP") && temp[1].Equals("DB_J6"))
            {
                for (int i = dgBefore.Columns["REST1_LOTID"].Index; i <= dgBefore.Columns["REST3_PRES_VAL"].Index; i++)
                {
                    dgBefore.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DB_J6_TRAY"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_CELL"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_EQP"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_ROUTE"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_GRADE"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_STARTTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_ENDTIME"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_ENDTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_ENDTIME"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_ENDTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_TEMP"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_TEMP_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DB_J6_PRESS"))
                {
                    dgBefore.Columns[dgBefore.Columns["REST1_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST2_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgBefore.Columns[dgBefore.Columns["REST3_PRES_VAL"].Index].Visibility = Visibility.Collapsed;
                }
            }

            dgBefore.TopRowHeaderMerge();
        }

        private void afCheck_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tvAfter, typeof(CheckBox), ref items);
            String[] temp = chk.Tag.ToString().Split('/');
            foreach (CheckBox item in items)
            {
                //선택한 체크박스가 TOP 이면 자식들을 체크한다.
                String[] key = item.Tag.ToString().Split('/');
                //Tag : PKEY_VAL / KEY_VAL
                if (temp[0].Equals("TOP"))
                {
                    if (key[0].Equals(temp[1]))
                    {
                        item.IsChecked = true;
                    }
                }
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DA_C"))
            {
                for (int i = dgAfter.Columns["SUBLOTID"].Index; i <= dgAfter.Columns["PRODID"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_CELL"))
                    dgAfter.Columns[dgAfter.Columns["SUBLOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_PKGLOT"))
                    dgAfter.Columns[dgAfter.Columns["PROD_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_LINE"))
                    dgAfter.Columns[dgAfter.Columns["EQSGID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_PROD"))
                    dgAfter.Columns[dgAfter.Columns["PRODID"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_13"))
            {
                for (int i = dgAfter.Columns["OCV1_LOTID"].Index; i <= dgAfter.Columns["OCV1_VAL"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_13_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_CELL"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_EQP"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_GRADE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_ENDTIME"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_STARTTIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_13_OCV"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_VAL"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_11"))
            {
                for (int i = dgAfter.Columns["CHG1_LOTID"].Index; i <= dgAfter.Columns["CHG3_END_TEMP"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_11_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_GRADE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_GRADE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_GRADE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_STARTTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_CAPA"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_AVGVOLT"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_AVG_VLTG_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_AVG_VLTG_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_AVG_VLTG_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_ENDVOLT"))
                {

                    dgAfter.Columns[dgAfter.Columns["CHG1_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_11_ENDCURR"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_END_CURNT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_END_CURNT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_END_CURNT_VAL"].Index].Visibility = Visibility.Visible;
                }
                //if (temp[1].Equals("DA_11_FITCAPA"))
                //{
                //    dgAfter.Columns[dgAfter.Columns["FITTED_CHG1_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                //    dgAfter.Columns[dgAfter.Columns["FITTED_CHG2_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                //    dgAfter.Columns[dgAfter.Columns["FITTED_CHG3_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                //}

                if (temp[1].Equals("DA_11_ENDTEMP"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_END_TEMP"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG2_END_TEMP"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["CHG3_END_TEMP"].Index].Visibility = Visibility.Visible;
                }
                //if (temp[1].Equals("DA_11_STDEV"))
                //{
                //    dgAfter.Columns[dgAfter.Columns["CHG2_AVGVLTG_STDEV"].Index].Visibility = Visibility.Visible;
                //}
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_12"))
            {
                for (int i = dgAfter.Columns["DCHG1_LOTID"].Index; i <= dgAfter.Columns["DCHG3_AVG_TEMP"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_12_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_GRADE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_GRADE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_GRADE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_STARTTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_CAPA"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_AVGVOLT"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_AVG_VLTG_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_AVG_VLTG_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_AVG_VLTG_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_ENDVOLT"))
                {

                    dgAfter.Columns[dgAfter.Columns["DCHG1_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_END_VOLT_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_ENDCURR"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_END_CURNT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_END_CURNT_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_END_CURNT_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_FITCAPA"))
                {
                    dgAfter.Columns[dgAfter.Columns["FITTED_DCHG1_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["FITTED_DCHG2_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["FITTED_DCHG3_CAPA_VAL"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_12_AVGTEMP"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_AVG_TEMP"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_AVG_TEMP"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_AVG_TEMP"].Index].Visibility = Visibility.Visible;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_19"))
            {
                for (int i = dgAfter.Columns["POWERCHG1_LOTID"].Index; i <= dgAfter.Columns["FITTED_POWERCHG1_IMP_VAL"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_19_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_CELL"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_EQP"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_GRADE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_ENDTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_STARTTIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_IMP"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_IMP_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_19_FITIMP"))
                    dgAfter.Columns[dgAfter.Columns["FITTED_POWERCHG1_IMP_VAL"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_17"))
            {
                for (int i = dgAfter.Columns["POWERDCHG1_LOTID"].Index; i <= dgAfter.Columns["FITTED_POWERDCHG1_IMP_VAL"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_17_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_CELL"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_EQP"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_GRADE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_ENDTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_STARTTIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_IMP"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_IMP_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_17_FITIMP"))
                    dgAfter.Columns[dgAfter.Columns["FITTED_POWERDCHG1_IMP_VAL"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_17_SASFITCAPA"))
                //    dgAfter.Columns[dgAfter.Columns["ML_FITTED_POWERDCHG1_CAPA_VAL"].Index].Visibility = Visibility.Visible;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_81"))
            {
                chkpocv = true;
                for (int i = dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVE.Value)].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_81_TRAY"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_CELL"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_EQP"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_ROUTE"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_GRADE"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_GRADE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_ENDTIME"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_STARTTIME"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_81_OCV"))
                {
                    chkpocv = true;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i].Index].Visibility = Visibility.Visible;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_A1"))
            {
                chkdocv = true;
                for (int i = dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVE.Value)].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_A1_TRAY"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_CELL"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_EQP"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_ROUTE"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_GRADE"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_GRADE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_ENDTIME"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_STARTTIME"))
                {
                    chkdocv = true;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_A1_OCV"))
                {
                    chkdocv = true;
                    for (int i = 1; i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i].Index].Visibility = Visibility.Visible;
                }
            }
            // 2024.06.12 YHK : Slope OCV 항목 추가
            //if (temp[0].Equals("TOP") && temp[1].Equals("DA_A3"))
            //{
            //    chksocv = true;
            //    for (int i = dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVE.Value)].Index; i++)
            //    {
            //        dgAfter.Columns[i].Visibility = Visibility.Visible;
            //    }
            //}
            //else
            //{
            //    if (temp[1].Equals("DA_A3_TRAY"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_LOTID"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_CELL"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_CSTSLOT"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_EQP"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_EQP_ID"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_ROUTE"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_ROUTE"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_GRADE"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_GRADE"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_ENDTIME"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_TIME"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_STARTTIME"))
            //    {
            //        chksocv = true;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_STARTTIME"].Index].Visibility = Visibility.Visible;
            //    }
            //    if (temp[1].Equals("DA_A3_OCV"))
            //    {
            //        chksocv = true;
            //        for (int i = 1; i <= Convert.ToInt16(nudPOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i].Index].Visibility = Visibility.Visible;
            //    }
            //}
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_51"))
            {
                for (int i = dgAfter.Columns["EOL_LOTID"].Index; i <= dgAfter.Columns["TAPE_CSTT_NO"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_51_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["EOL_LOTID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_CELL"))
                    dgAfter.Columns[dgAfter.Columns["EOL_CSTSLOT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_EQP"))
                    dgAfter.Columns[dgAfter.Columns["EOL_EQP_ID"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["EOL_ROUTE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["EOL_GRADE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_EOLTIME"))
                    dgAfter.Columns[dgAfter.Columns["EOL_TIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["EOL_STARTTIME"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_THIC"))
                    dgAfter.Columns[dgAfter.Columns["THIC_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_WEIGHT"))
                    dgAfter.Columns[dgAfter.Columns["WEIGHT_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_COMPUTE"))
                    dgAfter.Columns[dgAfter.Columns["EOL_COMPUTE_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_FILLINGLOSS"))
                    dgAfter.Columns[dgAfter.Columns["EOL_FILLING_LOSS"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_VOLT"))
                {
                    dgAfter.Columns[dgAfter.Columns["VOLT_VAL1"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["VOLT_VAL2"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_51_ACIR"))
                    dgAfter.Columns[dgAfter.Columns["ACIR_VAL"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_51_DCIR"))
                //    dgAfter.Columns[dgAfter.Columns["DCIR_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_IR"))
                    dgAfter.Columns[dgAfter.Columns["IR_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_MVDAY"))
                    dgAfter.Columns[dgAfter.Columns["MVDAY"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_MVDAYSPEC"))
                    dgAfter.Columns[dgAfter.Columns["MVDAY_SPEC"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_IVCA"))
                    dgAfter.Columns[dgAfter.Columns["IV_CA_VAL"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_51_MAXTHIC"))
                //    dgAfter.Columns[dgAfter.Columns["TCK_MAX_VAL"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_51_MAXTHIC"))
                //    dgAfter.Columns[dgAfter.Columns["TCK_MAX_VAL"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_VERIFYGRADE"))
                    dgAfter.Columns[dgAfter.Columns["VERIFY_GRADE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_REWORK_CNT"))
                    dgAfter.Columns[dgAfter.Columns["EOL_REWORK_CNT"].Index].Visibility = Visibility.Visible;

                if (temp[1].Equals("DA_51_OCV_TMPR"))
                    dgAfter.Columns[dgAfter.Columns["OCV_TMPR"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_MVDAY_TMPR"))
                    dgAfter.Columns[dgAfter.Columns["MVDAY_TMPR"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_ADJ_MVDAY_VALUE"))
                    dgAfter.Columns[dgAfter.Columns["ADJ_MVDAY_VALUE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_TMPR_DIFF"))
                    dgAfter.Columns[dgAfter.Columns["TMPR_DIFF"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_SLOPE"))
                    dgAfter.Columns[dgAfter.Columns["SLOPE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_INTERCEPT"))
                    dgAfter.Columns[dgAfter.Columns["INTERCEPT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_ADJ_DOCV"))
                    dgAfter.Columns[dgAfter.Columns["ADJ_DOCV"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_TAPE_UNIT"))
                    dgAfter.Columns[dgAfter.Columns["TAPE_UNIT"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_51_TAPE_CSTT_NO"))
                    dgAfter.Columns[dgAfter.Columns["TAPE_CSTT_NO"].Index].Visibility = Visibility.Visible;
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DA_D1"))
            {
                //dgAfter.Columns[dgAfter.Columns["PREDCT_DOCV2"].Index].Visibility = Visibility.Visible;
                //dgAfter.Columns[dgAfter.Columns["TRGT_DOCV2"].Index].Visibility = Visibility.Visible;
                //dgAfter.Columns[dgAfter.Columns["ALG_VER"].Index].Visibility = Visibility.Visible;
                dgAfter.Columns[dgAfter.Columns["FITTED_DOCV2"].Index].Visibility = Visibility.Visible;
                //dgAfter.Columns[dgAfter.Columns["DOCV_AVG"].Index].Visibility = Visibility.Visible;
                dgAfter.Columns[dgAfter.Columns["DOCV_STARTTIME"].Index].Visibility = Visibility.Visible;
                //dgAfter.Columns[dgAfter.Columns["DOCV_STDEV"].Index].Visibility = Visibility.Visible;
                //dgAfter.Columns[dgAfter.Columns["DOCV_LSPEC"].Index].Visibility = Visibility.Visible;
                //dgAfter.Columns[dgAfter.Columns["SIGMA_VALUE"].Index].Visibility = Visibility.Visible;
                dgAfter.Columns[dgAfter.Columns["DOCV2_JUDG_STATUS"].Index].Visibility = Visibility.Visible;
            }
            else
            {
                //if (temp[1].Equals("DA_D1_PREDCT_DOCV2"))
                //    dgAfter.Columns[dgAfter.Columns["PREDCT_DOCV2"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_D1_TRGT_DOCV2"))
                //    dgAfter.Columns[dgAfter.Columns["TRGT_DOCV2"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_D1_ALG_VER"))
                //    dgAfter.Columns[dgAfter.Columns["ALG_VER"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_D1_FITTED_DOCV2"))
                    dgAfter.Columns[dgAfter.Columns["FITTED_DOCV2"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_D1_DOCV_AVG"))
                //    dgAfter.Columns[dgAfter.Columns["DOCV_AVG"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_D1_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["DOCV_STARTTIME"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_D1_DOCV_STDEV"))
                //    dgAfter.Columns[dgAfter.Columns["DOCV_STDEV"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_D1_DOCV_LSPEC"))
                //    dgAfter.Columns[dgAfter.Columns["DOCV_LSPEC"].Index].Visibility = Visibility.Visible;
                //if (temp[1].Equals("DA_D1_SIGMA"))
                //    dgAfter.Columns[dgAfter.Columns["SIGMA_VALUE"].Index].Visibility = Visibility.Visible;
                if (temp[1].Equals("DA_D1_DOCV2_JUDG_STATUS"))
                    dgAfter.Columns[dgAfter.Columns["DOCV2_JUDG_STATUS"].Index].Visibility = Visibility.Visible;
            }


            if (temp[0].Equals("TOP") && temp[1].Equals("DA_71"))
            {
                for (int i = dgAfter.Columns["SHIP1_LOTID"].Index; i <= dgAfter.Columns["SHIP4_STARTTIME"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_71_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_END_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_END_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_END_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_END_TIME"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_AGINGTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1AGING"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2AGING"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3AGING"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4AGING"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_71_STARTTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_STARTTIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_STARTTIME"].Index].Visibility = Visibility.Visible;
                }
            }

            // 2024.02.20 판정 추가
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_B1"))
            {
                for (int i = dgAfter.Columns["JUDG1_LOTID"].Index; i <= dgAfter.Columns["JUDG6_TIME"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (temp[1].Equals("DA_B1_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_LOTID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_LOTID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_B1_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_CSTSLOT"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_CSTSLOT"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_B1_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_EQP_ID"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_EQP_ID"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_B1_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_ROUTE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_ROUTE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_B1_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_FINL_JUDG_CODE"].Index].Visibility = Visibility.Visible;
                }
                if (temp[1].Equals("DA_B1_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_TIME"].Index].Visibility = Visibility.Visible;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_TIME"].Index].Visibility = Visibility.Visible;
                }
            }
            dgAfter.TopRowHeaderMerge();

        }

        private void afCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tvAfter, typeof(CheckBox), ref items);
            String[] temp = chk.Tag.ToString().Split('/');
            foreach (CheckBox item in items)
            {
                String[] key = item.Tag.ToString().Split('/');
                //Tag : PKEY_VAL / KEY_VAL
                if (temp[0].Equals("TOP"))
                {
                    if (key[0].Equals(temp[1]))
                    {
                        item.IsChecked = false;
                    }
                }
                if (temp[0].Equals(key[1]))
                {
                    if (key[0].Equals("TOP"))
                    {
                        item.Unchecked -= new RoutedEventHandler(afCheck_Unchecked);
                        item.IsChecked = false;
                        item.Unchecked += new RoutedEventHandler(afCheck_Unchecked);
                    }
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_C"))
            {
                for (int i = dgAfter.Columns["SUBLOTID"].Index; i <= dgAfter.Columns["PRODID"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_CELL"))
                    dgAfter.Columns[dgAfter.Columns["SUBLOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_PKGLOT"))
                    dgAfter.Columns[dgAfter.Columns["PROD_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_LINE"))
                    dgAfter.Columns[dgAfter.Columns["EQSGID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_PROD"))
                    dgAfter.Columns[dgAfter.Columns["PRODID"].Index].Visibility = Visibility.Collapsed;
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DA_13"))
            {
                for (int i = dgAfter.Columns["OCV1_LOTID"].Index; i <= dgAfter.Columns["OCV1_VAL"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_13_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_CELL"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_EQP"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_GRADE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_ENDTIME"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_13_OCV"))
                    dgAfter.Columns[dgAfter.Columns["OCV1_VAL"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_11"))
            {
                for (int i = dgAfter.Columns["CHG1_LOTID"].Index; i <= dgAfter.Columns["CHG3_END_TEMP"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_11_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_GRADE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_GRADE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_GRADE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_STARTTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_CAPA"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_AVGVOLT"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_AVG_VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_AVG_VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_AVG_VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_ENDVOLT"))
                {

                    dgAfter.Columns[dgAfter.Columns["CHG1_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_11_ENDCURR"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_END_CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_END_CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_END_CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                //if (temp[1].Equals("DA_11_FITCAPA"))
                //{
                //    dgAfter.Columns[dgAfter.Columns["FITTED_CHG1_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                //    dgAfter.Columns[dgAfter.Columns["FITTED_CHG2_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                //    dgAfter.Columns[dgAfter.Columns["FITTED_CHG3_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                //}

                if (temp[1].Equals("DA_11_ENDTEMP"))
                {
                    dgAfter.Columns[dgAfter.Columns["CHG1_END_TEMP"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG2_END_TEMP"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["CHG3_END_TEMP"].Index].Visibility = Visibility.Collapsed;
                }
                //if (temp[1].Equals("DA_11_STDEV"))
                //{
                //    dgAfter.Columns[dgAfter.Columns["CHG2_AVGVLTG_STDEV"].Index].Visibility = Visibility.Collapsed;
                //}
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_12"))
            {
                for (int i = dgAfter.Columns["DCHG1_LOTID"].Index; i <= dgAfter.Columns["DCHG3_AVG_TEMP"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_12_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_GRADE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_GRADE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_GRADE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_STARTTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_CAPA"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_AVGVOLT"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_AVG_VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_AVG_VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_AVG_VLTG_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_ENDVOLT"))
                {

                    dgAfter.Columns[dgAfter.Columns["DCHG1_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_END_VOLT_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_ENDCURR"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_END_CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_END_CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_END_CURNT_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_FITCAPA"))
                {
                    dgAfter.Columns[dgAfter.Columns["FITTED_DCHG1_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["FITTED_DCHG2_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["FITTED_DCHG3_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_12_AVGTEMP"))
                {
                    dgAfter.Columns[dgAfter.Columns["DCHG1_AVG_TEMP"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG2_AVG_TEMP"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["DCHG3_AVG_TEMP"].Index].Visibility = Visibility.Collapsed;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_19"))
            {
                for (int i = dgAfter.Columns["POWERCHG1_LOTID"].Index; i <= dgAfter.Columns["FITTED_POWERCHG1_IMP_VAL"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_19_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_CELL"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_EQP"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_GRADE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_ENDTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_IMP"))
                    dgAfter.Columns[dgAfter.Columns["POWERCHG1_IMP_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_19_FITIMP"))
                    dgAfter.Columns[dgAfter.Columns["FITTED_POWERCHG1_IMP_VAL"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_17"))
            {
                for (int i = dgAfter.Columns["POWERDCHG1_LOTID"].Index; i <= dgAfter.Columns["FITTED_POWERDCHG1_IMP_VAL"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_17_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_CELL"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_EQP"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_GRADE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_ENDTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_IMP"))
                    dgAfter.Columns[dgAfter.Columns["POWERDCHG1_IMP_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_17_FITIMP"))
                    dgAfter.Columns[dgAfter.Columns["FITTED_POWERDCHG1_IMP_VAL"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_17_SASFITCAPA"))
                //    dgAfter.Columns[dgAfter.Columns["ML_FITTED_POWERDCHG1_CAPA_VAL"].Index].Visibility = Visibility.Collapsed;
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_81"))
            {
                chkpocv = false;
                for (int i = dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DEDICATE_OCV" + Convert.ToInt16(nudPOCVE.Value)].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_81_TRAY"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_CELL"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_EQP"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_ROUTE"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_GRADE"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_GRADE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_ENDTIME"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_STARTTIME"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i + "_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_81_OCV"))
                {
                    chkpocv = false;
                    for (int i = Convert.ToInt16(nudPOCVS.Value); i <= Convert.ToInt16(nudPOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DEDICATE_OCV" + i].Index].Visibility = Visibility.Collapsed;
                }
            }
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_A1"))
            {
                chkdocv = false;
                for (int i = dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["DOCV" + Convert.ToInt16(nudDOCVE.Value)].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_A1_TRAY"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_CELL"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_EQP"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_ROUTE"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_GRADE"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_GRADE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_ENDTIME"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_STARTTIME"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i + "_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_A1_OCV"))
                {
                    chkdocv = false;
                    for (int i = Convert.ToInt16(nudDOCVS.Value); i <= Convert.ToInt16(nudDOCVE.Value); i++)
                        dgAfter.Columns[dgAfter.Columns["DOCV" + i].Index].Visibility = Visibility.Collapsed;
                }
            }
            // 2024.06.12 YHK : Slope OCV 항목 추가
            //if (temp[0].Equals("TOP") && temp[1].Equals("DA_A3"))
            //{
            //    chksocv = false;
            //    for (int i = dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVS.Value) + "_LOTID"].Index; i <= dgAfter.Columns["SOCV" + Convert.ToInt16(nudSOCVE.Value)].Index; i++)
            //    {
            //        dgAfter.Columns[i].Visibility = Visibility.Collapsed;
            //    }
            //}
            //else
            //{
            //    if (temp[1].Equals("DA_A3_TRAY"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_LOTID"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_CELL"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_EQP"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_EQP_ID"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_ROUTE"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_ROUTE"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_GRADE"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_GRADE"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_ENDTIME"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_TIME"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_STARTTIME"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i + "_STARTTIME"].Index].Visibility = Visibility.Collapsed;
            //    }
            //    if (temp[1].Equals("DA_A3_OCV"))
            //    {
            //        chksocv = false;
            //        for (int i = Convert.ToInt16(nudSOCVS.Value); i <= Convert.ToInt16(nudSOCVE.Value); i++)
            //            dgAfter.Columns[dgAfter.Columns["SOCV" + i].Index].Visibility = Visibility.Collapsed;
            //    }
            //}
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_51"))
            {
                for (int i = dgAfter.Columns["EOL_LOTID"].Index; i <= dgAfter.Columns["TAPE_CSTT_NO"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_51_TRAY"))
                    dgAfter.Columns[dgAfter.Columns["EOL_LOTID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_CELL"))
                    dgAfter.Columns[dgAfter.Columns["EOL_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_EQP"))
                    dgAfter.Columns[dgAfter.Columns["EOL_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_ROUTE"))
                    dgAfter.Columns[dgAfter.Columns["EOL_ROUTE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_GRADE"))
                    dgAfter.Columns[dgAfter.Columns["EOL_GRADE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_EOLTIME"))
                    dgAfter.Columns[dgAfter.Columns["EOL_TIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["EOL_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_THIC"))
                    dgAfter.Columns[dgAfter.Columns["THIC_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_WEIGHT"))
                    dgAfter.Columns[dgAfter.Columns["WEIGHT_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_COMPUTE"))
                    dgAfter.Columns[dgAfter.Columns["EOL_COMPUTE_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_FILLINGLOSS"))
                    dgAfter.Columns[dgAfter.Columns["EOL_FILLING_LOSS"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_VOLT"))
                {
                    dgAfter.Columns[dgAfter.Columns["VOLT_VAL1"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["VOLT_VAL2"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_51_ACIR"))
                    dgAfter.Columns[dgAfter.Columns["ACIR_VAL"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_51_DCIR"))
                //    dgAfter.Columns[dgAfter.Columns["DCIR_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_IR"))
                    dgAfter.Columns[dgAfter.Columns["IR_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_MVDAY"))
                    dgAfter.Columns[dgAfter.Columns["MVDAY"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_MVDAYSPEC"))
                    dgAfter.Columns[dgAfter.Columns["MVDAY_SPEC"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_IVCA"))
                    dgAfter.Columns[dgAfter.Columns["IV_CA_VAL"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_51_MAXTHIC"))
                //    dgAfter.Columns[dgAfter.Columns["TCK_MAX_VAL"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_51_MAXTHIC"))
                //    dgAfter.Columns[dgAfter.Columns["TCK_MAX_VAL"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_VERIFYGRADE"))
                    dgAfter.Columns[dgAfter.Columns["VERIFY_GRADE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_REWORK_CNT"))
                    dgAfter.Columns[dgAfter.Columns["EOL_REWORK_CNT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_OCV_TMPR"))
                    dgAfter.Columns[dgAfter.Columns["OCV_TMPR"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_MVDAY_TMPR"))
                    dgAfter.Columns[dgAfter.Columns["MVDAY_TMPR"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_ADJ_MVDAY_VALUE"))
                    dgAfter.Columns[dgAfter.Columns["ADJ_MVDAY_VALUE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_TMPR_DIFF"))
                    dgAfter.Columns[dgAfter.Columns["TMPR_DIFF"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_SLOPE"))
                    dgAfter.Columns[dgAfter.Columns["SLOPE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_INTERCEPT"))
                    dgAfter.Columns[dgAfter.Columns["INTERCEPT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_ADJ_DOCV"))
                    dgAfter.Columns[dgAfter.Columns["ADJ_DOCV"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_TAPE_UNIT"))
                    dgAfter.Columns[dgAfter.Columns["TAPE_UNIT"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_51_TAPE_CSTT_NO"))
                    dgAfter.Columns[dgAfter.Columns["TAPE_CSTT_NO"].Index].Visibility = Visibility.Collapsed;
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DA_D1"))
            {
                //dgAfter.Columns[dgAfter.Columns["PREDCT_DOCV2"].Index].Visibility = Visibility.Collapsed;
                //dgAfter.Columns[dgAfter.Columns["TRGT_DOCV2"].Index].Visibility = Visibility.Collapsed;
                //dgAfter.Columns[dgAfter.Columns["ALG_VER"].Index].Visibility = Visibility.Collapsed;
                dgAfter.Columns[dgAfter.Columns["FITTED_DOCV2"].Index].Visibility = Visibility.Collapsed;
                //dgAfter.Columns[dgAfter.Columns["DOCV_AVG"].Index].Visibility = Visibility.Collapsed;
                dgAfter.Columns[dgAfter.Columns["DOCV_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                //dgAfter.Columns[dgAfter.Columns["DOCV_STDEV"].Index].Visibility = Visibility.Collapsed;
                //dgAfter.Columns[dgAfter.Columns["DOCV_LSPEC"].Index].Visibility = Visibility.Collapsed;
                //dgAfter.Columns[dgAfter.Columns["SIGMA_VALUE"].Index].Visibility = Visibility.Collapsed;
                dgAfter.Columns[dgAfter.Columns["DOCV2_JUDG_STATUS"].Index].Visibility = Visibility.Collapsed;


            }
            else
            {
                //if (temp[1].Equals("DA_D1_PREDCT_DOCV2"))
                //    dgAfter.Columns[dgAfter.Columns["PREDCT_DOCV2"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_D1_TRGT_DOCV2"))
                //    dgAfter.Columns[dgAfter.Columns["TRGT_DOCV2"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_D1_ALG_VER"))
                //    dgAfter.Columns[dgAfter.Columns["ALG_VER"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_D1_FITTED_DOCV2"))
                    dgAfter.Columns[dgAfter.Columns["FITTED_DOCV2"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_D1_DOCV_AVG"))
                //    dgAfter.Columns[dgAfter.Columns["DOCV_AVG"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_D1_STARTTIME"))
                    dgAfter.Columns[dgAfter.Columns["DOCV_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_D1_DOCV_STDEV"))
                //    dgAfter.Columns[dgAfter.Columns["DOCV_STDEV"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_D1_DOCV_LSPEC"))
                //    dgAfter.Columns[dgAfter.Columns["DOCV_LSPEC"].Index].Visibility = Visibility.Collapsed;
                //if (temp[1].Equals("DA_D1_SIGMA"))
                //    dgAfter.Columns[dgAfter.Columns["SIGMA_VALUE"].Index].Visibility = Visibility.Collapsed;
                if (temp[1].Equals("DA_D1_DOCV2_JUDG_STATUS"))
                    dgAfter.Columns[dgAfter.Columns["DOCV2_JUDG_STATUS"].Index].Visibility = Visibility.Collapsed;
            }

            if (temp[0].Equals("TOP") && temp[1].Equals("DA_71"))
            {
                for (int i = dgAfter.Columns["SHIP1_LOTID"].Index; i <= dgAfter.Columns["SHIP4_STARTTIME"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_71_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_END_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_END_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_END_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_END_TIME"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_AGINGTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1AGING"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2AGING"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3AGING"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4AGING"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_71_STARTTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["SHIP1_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP2_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP3_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["SHIP4_STARTTIME"].Index].Visibility = Visibility.Collapsed;
                }
            }

            // 2024.02.20 판정 추가
            if (temp[0].Equals("TOP") && temp[1].Equals("DA_B1"))
            {
                for (int i = dgAfter.Columns["JUDG1_LOTID"].Index; i <= dgAfter.Columns["JUDG6_TIME"].Index; i++)
                {
                    dgAfter.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (temp[1].Equals("DA_B1_TRAY"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_LOTID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_LOTID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_B1_CELL"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_CSTSLOT"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_B1_EQP"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_EQP_ID"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_B1_ROUTE"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_ROUTE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_ROUTE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_B1_GRADE"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_FINL_JUDG_CODE"].Index].Visibility = Visibility.Collapsed;
                }
                if (temp[1].Equals("DA_B1_ENDTIME"))
                {
                    dgAfter.Columns[dgAfter.Columns["JUDG1_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG2_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG3_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG4_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG5_TIME"].Index].Visibility = Visibility.Collapsed;
                    dgAfter.Columns[dgAfter.Columns["JUDG6_TIME"].Index].Visibility = Visibility.Collapsed;
                }
            }
            dgAfter.TopRowHeaderMerge();
        }

        private void rdoEOLRework_Checked(object sender, RoutedEventArgs e)
        {
            if (tvAfter == null)
                return;
            tvAfter.Visibility = Visibility.Collapsed;
            SetColumns(dgAfter, false);
        }

        private void rdoOCV1After_Checked(object sender, RoutedEventArgs e)
        {
            if (tvAfter == null)
                return;
            tvAfter.Visibility = Visibility.Visible;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tvAfter, typeof(CheckBox), ref items);
            foreach (CheckBox item in items)
            {
                if (item.Tag.ToString().Contains("DA_C"))
                    item.IsChecked = true;
            }
            SetColumns(dgAfter, true);
        }

        private void tbcWip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            String TabItem = ((sender as C1TabControl).SelectedItem as C1TabItem).Name;
            Dispatcher.BeginInvoke(new Action(() => GetInitNode(TabItem)), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
        }

        #endregion

        #region Method

        private void InitSpread(C1.WPF.DataGrid.C1DataGrid dg)
        {
            Util.gridClear(dg); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dg.TopRows.Add(HR);
            }

            #region 디가스 전
            if (dg.Name.Equals("dgBefore"))
            {
                DataRow[] drcominfo = dtTemp.AsEnumerable().ToArray();

                foreach (DataRow dr in drcominfo)
                {
                    if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_C"))
                    {
                        //Cell 기준정보
                        FixedMultiHeader(dg, "DEFAULT_INFO|CELL_ID", "SUBLOTID", true);
                        FixedMultiHeader(dg, "DEFAULT_INFO|PKG_LOT_ID", "PROD_LOTID", true);
                        FixedMultiHeader(dg, "DEFAULT_INFO|LINE_ID", "EQSGID", true);
                        FixedMultiHeader(dg, "DEFAULT_INFO|PRODUCT_ID", "PRODID", true);

                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_91"))
                    {
                        //PreAging
                        FixedMultiHeader(dg, "PRE_AGING|TRAY_LOT_ID", "PRE_LOTID", false);
                        FixedMultiHeader(dg, "PRE_AGING|CHANNEL", "PRE_CSTSLOT", false);
                        FixedMultiHeader(dg, "PRE_AGING|ROUTE_ID", "PRE_ROUTE", false);
                        FixedMultiHeader(dg, "PRE_AGING|GRADE", "PRE_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "PRE_AGING|BOX_ID", "PRE_EQP_ID", false);
                        FixedMultiHeader(dg, "PRE_AGING|START_TIME", "PRE_STARTTIME", false);
                        FixedMultiHeader(dg, "PRE_AGING|AGING_END_DATE", "PRE_TIME", false);
                        FixedMultiHeader(dg, "PRE_AGING|PRE_AGING_TIME", "PREAGING", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_U1"))
                    {
                        //HPCD
                        FixedMultiHeader(dg, "HPCD|TRAY_LOT_ID", "HPCD_LOTID", false);
                        FixedMultiHeader(dg, "HPCD|CHANNEL", "HPCD_CSTSLOT", false);
                        FixedMultiHeader(dg, "HPCD|ROUTE_ID", "HPCD_ROUTE", false);
                        FixedMultiHeader(dg, "HPCD|GRADE", "HPCD_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "HPCD|BOX_ID", "HPCD_EQP_ID", false);
                        FixedMultiHeader(dg, "HPCD|START_TIME", "HPCD_STARTTIME", false);
                        FixedMultiHeader(dg, "HPCD|HPCD_TIME", "HPCD_TIME", false);
                        FixedMultiHeader(dg, "HPCD|HPCD_CURNT_VAL", "CURNT_VAL", false);
                        FixedMultiHeader(dg, "HPCD|HPCD_VLTG_VAL", "VLTG_VAL", false);
                        //FixedMultiHeader(dg, "HPCD|HPCD_INT_VLTG_VALUE", "INI_VLTG_VALUE", false); // 2023.11.13 초기전압 추가
                        //FixedMultiHeader(dg, "HPCD|HPCD_END_VLTG_VALUE", "END_VLTG_VALUE", false); // 2023.11.13 종료전압 추가
                        FixedMultiHeader(dg, "HPCD|HPCD_DELTA_TMPR_VAL", "DELTA_TMPR_VAL", false);
                        FixedMultiHeader(dg, "HPCD|HPCD_JIG_PRESS_VAL", "JIG_PRESS_VAL", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_J1"))
                    {
                        //JIG
                        FixedMultiHeader(dg, "JF_CHARGE|TRAY_LOT_ID", "CHG1_LOTID", false);
                        FixedMultiHeader(dg, "JF_CHARGE|CHANNEL", "CHG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_CHARGE|ROUTE_ID", "CHG1_ROUTE", false);
                        FixedMultiHeader(dg, "JF_CHARGE|GRADE", "CHG1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_CHARGE|BOX_ID", "CHG1_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_START_TIME", "CHG1_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_TIME", "CHG1_TIME", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_CAPA_VAL", "CHG1_CAPA_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_AVG_VOLT_VAL", "CHG1_AVG_VOLT_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_END_VOLT_VAL", "CHG1_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_END_CURR_VAL", "CHG1_END_CURR_VAL", false);
                        //FixedMultiHeader(dg, "JIG_FORMATION|JIG1_AVG_TMPR_VAL", "CHG1_AVG_TMPR_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_TEMP_VAL", "CHG1_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG1_PRES_VAL", "CHG1_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|TRAY_LOT_ID", "CHG2_LOTID", false);
                        FixedMultiHeader(dg, "JF_CHARGE|CHANNEL", "CHG2_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_CHARGE|ROUTE_ID", "CHG2_ROUTE", false);
                        FixedMultiHeader(dg, "JF_CHARGE|GRADE", "CHG2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_CHARGE|BOX_ID", "CHG2_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_START_TIME", "CHG2_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_TIME", "CHG2_TIME", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_CAPA_VAL", "CHG2_CAPA_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_AVG_VOLT_VAL", "CHG2_AVG_VOLT_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_END_VOLT_VAL", "CHG2_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_END_CURR_VAL", "CHG2_END_CURR_VAL", false);
                        //FixedMultiHeader(dg, "JIG_FORMATION|JIG2_AVG_TMPR_VAL", "CHG2_AVG_TMPR_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_TEMP_VAL", "CHG2_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG2_PRES_VAL", "CHG2_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|TRAY_LOT_ID", "CHG3_LOTID", false);
                        FixedMultiHeader(dg, "JF_CHARGE|CHANNEL", "CHG3_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_CHARGE|ROUTE_ID", "CHG3_ROUTE", false);
                        FixedMultiHeader(dg, "JF_CHARGE|GRADE", "CHG3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_CHARGE|BOX_ID", "CHG3_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_START_TIME", "CHG3_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_TIME", "CHG3_TIME", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_CAPA_VAL", "CHG3_CAPA_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_AVG_VOLT_VAL", "CHG3_AVG_VOLT_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_END_VOLT_VAL", "CHG3_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_END_CURR_VAL", "CHG3_END_CURR_VAL", false);
                        //FixedMultiHeader(dg, "JIG_FORMATION|JIG3_AVG_TMPR_VAL", "CHG3_AVG_TMPR_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_TEMP_VAL", "CHG3_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_CHARGE|JIG3_PRES_VAL", "CHG3_PRES_VAL", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_41"))
                    {
                        //HighAging
                        FixedMultiHeader(dg, "HIGH_AGING|TRAY_LOT_ID", "HIGH_LOTID", false);
                        FixedMultiHeader(dg, "HIGH_AGING|CHANNEL", "HIGH_CSTSLOT", false);
                        FixedMultiHeader(dg, "HIGH_AGING|ROUTE_ID", "HIGH_ROUTE", false);
                        FixedMultiHeader(dg, "HIGH_AGING|GRADE", "HIGH_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "HIGH_AGING|BOX_ID", "HIGH_EQP_ID", false);
                        FixedMultiHeader(dg, "HIGH_AGING|START_TIME", "HIGH_STARTTIME", false);
                        FixedMultiHeader(dg, "HIGH_AGING|AGING_END_DATE", "HIGH_TIME", false);
                        FixedMultiHeader(dg, "HIGH_AGING|HIGH_AGING_TIME", "HIGHAGING", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_J3"))
                    {
                        //OCV
                        FixedMultiHeader(dg, "JF_OCV|TRAY_LOT_ID", "OCV1_LOTID", false);
                        FixedMultiHeader(dg, "JF_OCV|CHANNEL", "OCV1_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_OCV|ROUTE_ID", "OCV1_ROUTE", false);
                        FixedMultiHeader(dg, "JF_OCV|GRADE", "OCV1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_OCV|BOX_ID", "OCV1_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_OCV|JIG_OCV1_START_TIME", "OCV1_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_OCV|JF_OCV_01_TIME", "OCV1_TIME", false);
                        FixedMultiHeader(dg, "JF_OCV|JF_OCV_01", "OCV1_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|OCV_01_JIG_TEMP_VAL", "OCV1_JIG_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|OCV_01_JIG_PRES_VAL", "OCV1_JIG_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|TRAY_LOT_ID", "OCV2_LOTID", false);
                        FixedMultiHeader(dg, "JF_OCV|CHANNEL", "OCV2_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_OCV|ROUTE_ID", "OCV2_ROUTE", false);
                        FixedMultiHeader(dg, "JF_OCV|GRADE", "OCV2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_OCV|BOX_ID", "OCV2_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_OCV|JIG_OCV2_START_TIME", "OCV2_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_OCV|JF_OCV_02_TIME", "OCV2_TIME", false);
                        FixedMultiHeader(dg, "JF_OCV|JF_OCV_02", "OCV2_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|OCV_02_JIG_TEMP_VAL", "OCV2_JIG_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|OCV_02_JIG_PRES_VAL", "OCV2_JIG_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|TRAY_LOT_ID", "OCV3_LOTID", false);
                        FixedMultiHeader(dg, "JF_OCV|CHANNEL", "OCV3_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_OCV|ROUTE_ID", "OCV3_ROUTE", false);
                        FixedMultiHeader(dg, "JF_OCV|GRADE", "OCV3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_OCV|BOX_ID", "OCV3_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_OCV|JIG_OCV3_START_TIME", "OCV3_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_OCV|JF_OCV_03_TIME", "OCV3_TIME", false);
                        FixedMultiHeader(dg, "JF_OCV|JF_OCV_03", "OCV3_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|OCV_03_JIG_TEMP_VAL", "OCV3_JIG_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_OCV|OCV_03_JIG_PRES_VAL", "OCV3_JIG_PRES_VAL", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_D1"))
                    {
                        //DEGAS
                        FixedMultiHeader(dg, "DEGAS|TRAY_LOT_ID", "DEGAS_LOTID", false);
                        FixedMultiHeader(dg, "DEGAS|CHANNEL", "DEGAS_CSTSLOT", false);
                        FixedMultiHeader(dg, "DEGAS|ROUTE_ID", "DEGAS_ROUTE", false);
                        FixedMultiHeader(dg, "DEGAS|GRADE", "DEGAS_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "DEGAS|BOX_ID", "DEGAS_EQP_ID", false);
                        FixedMultiHeader(dg, "DEGAS|START_TIME", "DEGAS_STARTTIME", false);
                        FixedMultiHeader(dg, "DEGAS|DEGAS_TIME", "DEGAS_TIME", false);
                        FixedMultiHeader(dg, "DEGAS|DEGAS_WEIGHT_VAL", "WEIGHT_VALUE", false);
                        FixedMultiHeader(dg, "DEGAS|IV_VAL", "IVLTG_VALUE", false);
                        FixedMultiHeader(dg, "DEGAS|DCIR_VALUE", "DCIR_VALUE", false);
                        FixedMultiHeader(dg, "DEGAS|CHAMBER_LOCATION_NO", "CHAMBER_LOCATION_NO", false);
                        FixedMultiHeader(dg, "DEGAS|CHAMBER_NEST_NO", "CHAMBER_NEST_NO", false);
                        FixedMultiHeader(dg, "DEGAS|SEALING_LOCATION_NO", "MAIN_SEAL_LOCATION_NO", false);
                        FixedMultiHeader(dg, "DEGAS|COMPUTE_VAL", "COMPUTE_VALUE", false);
                        FixedMultiHeader(dg, "DEGAS|FILLING_LOSS", "FILLING_LOSS", false);
                        FixedMultiHeader(dg, "DEGAS|HOTPRES_WEIGHT_VAL", "PRESS_LOAD_CELL_VAL", false);
                        FixedMultiHeader(dg, "DEGAS|HOTPRES_PORT_NO", "HOTPRES_PORT_NO", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_J7"))
                    {
                        //JIG PP
                        FixedMultiHeader(dg, "JF_PP|TRAY_LOT_ID", "PP1_LOTID", false);
                        FixedMultiHeader(dg, "JF_PP|CHANNEL", "PP1_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_PP|ROUTE_ID", "PP1_ROUTE", false);
                        FixedMultiHeader(dg, "JF_PP|GRADE", "PP1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_PP|BOX_ID", "PP1_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_PP|PP1_START_TIME", "PP1_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_PP|PP1_TIME", "PP1_TIME", false);
                        //FixedMultiHeader(dg, "PP|PP1_AVG_TMPR_VAL", "PP1_AVG_TMPR_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|PP1_TEMP_VAL", "PP1_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|PP1_PRES_VAL", "PP1_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|TRAY_LOT_ID", "PP2_LOTID", false);
                        FixedMultiHeader(dg, "JF_PP|CHANNEL", "PP2_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_PP|ROUTE_ID", "PP2_ROUTE", false);
                        FixedMultiHeader(dg, "JF_PP|GRADE", "PP2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_PP|BOX_ID", "PP2_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_PP|PP2_START_TIME", "PP2_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_PP|PP2_TIME", "PP2_TIME", false);
                        //FixedMultiHeader(dg, "PP|PP2_AVG_TMPR_VAL", "PP2_AVG_TMPR_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|PP2_TEMP_VAL", "PP2_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|PP2_PRES_VAL", "PP2_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|TRAY_LOT_ID", "PP3_LOTID", false);
                        FixedMultiHeader(dg, "JF_PP|CHANNEL", "PP3_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_PP|ROUTE_ID", "PP3_ROUTE", false);
                        FixedMultiHeader(dg, "JF_PP|GRADE", "PP3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_PP|BOX_ID", "PP3_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_PP|PP3_START_TIME", "PP3_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_PP|PP3_TIME", "PP3_TIME", false);
                        //FixedMultiHeader(dg, "PP|PP3_AVG_TMPR_VAL", "PP3_AVG_TMPR_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|PP3_TEMP_VAL", "PP3_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_PP|PP3_PRES_VAL", "PP3_PRES_VAL", false);
                    }

                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_31"))
                    {
                        //NomalTempAging#1
                        FixedMultiHeader(dg, "NOMAL_AGING#1|TRAY_LOT_ID", "NOMAL_LOTID", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|CHANNEL", "NOMAL_CSTSLOT", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|ROUTE_ID", "NOMAL_ROUTE", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|GRADE", "NOMAL_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|BOX_ID", "NOMAL_EQP_ID", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|START_TIME", "NOMAL_STARTTIME", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|AGING_END_DATE", "NOMAL_TIME", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#1|NOMAL1_AGING_TIME", "NOMALAGING", false);
                    }

                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_32"))
                    {

                        //NomalTempAging#2
                        FixedMultiHeader(dg, "NOMAL_AGING#2|TRAY_LOT_ID", "NOMAL2_LOTID", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|CHANNEL", "NOMAL2_CSTSLOT", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|ROUTE_ID", "NOMAL2_ROUTE", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|GRADE", "NOMAL2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|BOX_ID", "NOMAL2_EQP_ID", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|START_TIME", "NOMAL2_STARTTIME", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|AGING_END_DATE", "NOMAL2_TIME", false);
                        FixedMultiHeader(dg, "NOMAL_AGING#2|NOMAL2_AGING_TIME", "NOMAL2AGING", false);
                    }

                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_B1"))   // 2024.02.20 판정 추가
                    {
                        //JUDGE
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG1_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG1_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG1_GRADE", "JUDG1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG1_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG1_TIME", "JUDG1_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG2_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG2_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG2_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG2_GRADE", "JUDG2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG2_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG2_TIME", "JUDG2_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG3_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG3_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG3_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG3_GRADE", "JUDG3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG3_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG3_TIME", "JUDG3_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG4_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG4_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG4_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG4_GRADE", "JUDG4_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG4_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG4_TIME", "JUDG4_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG5_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG5_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG5_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG5_GRADE", "JUDG5_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG5_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG5_TIME", "JUDG5_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG6_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG6_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG6_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG6_GRADE", "JUDG6_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG6_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG6_TIME", "JUDG6_TIME", false);
                    }

                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_J6"))
                    {
                        //JIG REST
                        FixedMultiHeader(dg, "JF_REST|TRAY_LOT_ID", "REST1_LOTID", false);
                        FixedMultiHeader(dg, "JF_REST|CHANNEL", "REST1_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_REST|ROUTE_ID", "REST1_ROUTE", false);
                        FixedMultiHeader(dg, "JF_REST|GRADE", "REST1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_REST|BOX_ID", "REST1_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_REST|REST1_STARTTIME", "REST1_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_REST|REST1_ENDTIME", "REST1_ENDTIME", false);
                        FixedMultiHeader(dg, "JF_REST|REST1_TEMP_VAL", "REST1_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_REST|REST1_PRES_VAL", "REST1_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_REST|TRAY_LOT_ID", "REST2_LOTID", false);
                        FixedMultiHeader(dg, "JF_REST|CHANNEL", "REST2_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_REST|ROUTE_ID", "REST2_ROUTE", false);
                        FixedMultiHeader(dg, "JF_REST|GRADE", "REST2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_REST|BOX_ID", "REST2_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_REST|REST2_STARTTIME", "REST2_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_REST|REST2_ENDTIME", "REST2_ENDTIME", false);
                        FixedMultiHeader(dg, "JF_REST|REST2_TEMP_VAL", "REST2_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_REST|REST2_PRES_VAL", "REST2_PRES_VAL", false);
                        FixedMultiHeader(dg, "JF_REST|TRAY_LOT_ID", "REST3_LOTID", false);
                        FixedMultiHeader(dg, "JF_REST|CHANNEL", "REST3_CSTSLOT", false);
                        FixedMultiHeader(dg, "JF_REST|ROUTE_ID", "REST3_ROUTE", false);
                        FixedMultiHeader(dg, "JF_REST|GRADE", "REST3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JF_REST|BOX_ID", "REST3_EQP_ID", false);
                        FixedMultiHeader(dg, "JF_REST|REST3_STARTTIME", "REST3_STARTTIME", false);
                        FixedMultiHeader(dg, "JF_REST|REST3_ENDTIME", "REST3_ENDTIME", false);
                        FixedMultiHeader(dg, "JF_REST|REST3_TEMP_VAL", "REST3_TEMP_VAL", false);
                        FixedMultiHeader(dg, "JF_REST|REST3_PRES_VAL", "REST3_PRES_VAL", false);
                    }

                    //else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DB_J5"))
                    //{
                    //    //JIG RESTHEATING
                    //    FixedMultiHeader(dg, "RESTHEATING|TRAY_LOT_ID", "RESTHEATING1_LOTID", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|CHANNEL", "RESTHEATING1_CSTSLOT", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|BOX_ID", "RESTHEATING1_EQP_ID", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|ROUTE_ID", "RESTHEATING1_ROUTE", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|GRADE", "RESTHEATING1_FINL_JUDG_CODE", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING1_STARTTIME", "RESTHEATING1_STARTTIME", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING1_ENDTIME", "RESTHEATING1_ENDTIME", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING1_TEMP_VAL", "RESTHEATING1_TEMP_VAL", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING1_PRES_VAL", "RESTHEATING1_PRES_VAL", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|TRAY_LOT_ID", "RESTHEATING2_LOTID", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|CHANNEL", "RESTHEATING2_CSTSLOT", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|BOX_ID", "RESTHEATING2_EQP_ID", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|ROUTE_ID", "RESTHEATING2_ROUTE", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|GRADE", "RESTHEATING2_FINL_JUDG_CODE", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING2_STARTTIME", "RESTHEATING2_STARTTIME", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING2_ENDTIME", "RESTHEATING2_ENDTIME", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING2_TEMP_VAL", "RESTHEATING2_TEMP_VAL", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING2_PRES_VAL", "RESTHEATING2_PRES_VAL", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|TRAY_LOT_ID", "RESTHEATING3_LOTID", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|CHANNEL", "RESTHEATING3_CSTSLOT", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|BOX_ID", "RESTHEATING3_EQP_ID", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|ROUTE_ID", "RESTHEATING3_ROUTE", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|GRADE", "RESTHEATING3_FINL_JUDG_CODE", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING3_STARTTIME", "RESTHEATING3_STARTTIME", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING3_ENDTIME", "RESTHEATING3_ENDTIME", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING3_TEMP_VAL", "RESTHEATING3_TEMP_VAL", false);
                    //    FixedMultiHeader(dg, "RESTHEATING|RESTHEATING3_PRES_VAL", "RESTHEATING3_PRES_VAL", false);
                    //}
                }
                dgBefore.TopRowHeaderMerge();
            }
            #endregion

            #region 디가스 후
            if (dg.Name.Equals("dgAfter"))
            {
                DataRow[] drcominfo = dtTemp.AsEnumerable().ToArray();

                foreach (DataRow dr in drcominfo)
                {
                    if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_C"))
                    {
                        //Cell 기준정보
                        FixedMultiHeader(dg, "DEFAULT_INFO|CELL_ID", "SUBLOTID", true);
                        FixedMultiHeader(dg, "DEFAULT_INFO|PKG_LOT_ID", "PROD_LOTID", true);
                        FixedMultiHeader(dg, "DEFAULT_INFO|LINE_ID", "EQSGID", true);
                        FixedMultiHeader(dg, "DEFAULT_INFO|PRODUCT_ID", "PRODID", true);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_13"))
                    {
                        //OCV
                        FixedMultiHeader(dg, "OCV|TRAY_LOT_ID", "OCV1_LOTID", false);
                        FixedMultiHeader(dg, "OCV|CHANNEL", "OCV1_CSTSLOT", false);
                        FixedMultiHeader(dg, "OCV|ROUTE_ID", "OCV1_ROUTE", false);
                        FixedMultiHeader(dg, "OCV|GRADE", "OCV1_GRADE", false);
                        FixedMultiHeader(dg, "OCV|BOX_ID", "OCV1_EQP_ID", false);
                        FixedMultiHeader(dg, "OCV|START_TIME", "OCV1_STARTTIME", false);
                        FixedMultiHeader(dg, "OCV|OCV_01_TIME", "OCV1_TIME", false);
                        FixedMultiHeader(dg, "OCV|OCV_01", "OCV1_VAL", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_11"))
                    {
                        //CHARGE
                        FixedMultiHeader(dg, "CHG|TRAY_LOT_ID", "CHG1_LOTID", false);
                        FixedMultiHeader(dg, "CHG|CHANNEL", "CHG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "CHG|ROUTE_ID", "CHG1_ROUTE", false);
                        FixedMultiHeader(dg, "CHG|GRADE", "CHG1_GRADE", false);
                        FixedMultiHeader(dg, "CHG|BOX_ID", "CHG1_EQP_ID", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_START_TIME", "CHG1_STARTTIME", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_TIME", "CHG1TIME", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_CAPA_VAL", "CHG1_CAPA_VAL", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_AVG_VOLT_VAL", "CHG1_AVG_VLTG_VAL", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_END_VOLT_VAL", "CHG1_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_END_CURNT_VAL", "CHG1_END_CURNT_VAL", false);
                        //FixedMultiHeader(dg, "CHARGE|FIT_1_CHG_CAPA_VAL", "FITTED_CHG1_CAPA_VAL", false);
                        FixedMultiHeader(dg, "CHG|1_CHG_END_TEMP", "CHG1_END_TEMP", false);

                        FixedMultiHeader(dg, "CHG|TRAY_LOT_ID", "CHG2_LOTID", false);
                        FixedMultiHeader(dg, "CHG|CHANNEL", "CHG2_CSTSLOT", false);
                        FixedMultiHeader(dg, "CHG|ROUTE_ID", "CHG2_ROUTE", false);
                        FixedMultiHeader(dg, "CHG|GRADE", "CHG2_GRADE", false);
                        FixedMultiHeader(dg, "CHG|BOX_ID", "CHG2_EQP_ID", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_START_TIME", "CHG2_STARTTIME", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_TIME", "CHG2TIME", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_CAPA_VAL", "CHG2_CAPA_VAL", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_AVG_VOLT_VAL", "CHG2_AVG_VLTG_VAL", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_END_VOLT_VAL", "CHG2_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_END_CURNT_VAL", "CHG2_END_CURNT_VAL", false);
                        //FixedMultiHeader(dg, "CHARGE|FIT_2_CHG_CAPA_VAL", "FITTED_CHG2_CAPA_VAL", false);
                        FixedMultiHeader(dg, "CHG|2_CHG_END_TEMP", "CHG2_END_TEMP", false);
                        //FixedMultiHeader(dg, "CHARGE|2_CHG_AVGVLTG_STDEV", "CHG2_AVGVLTG_STDEV", false);

                        FixedMultiHeader(dg, "CHG|TRAY_LOT_ID", "CHG3_LOTID", false);
                        FixedMultiHeader(dg, "CHG|CHANNEL", "CHG3_CSTSLOT", false);
                        FixedMultiHeader(dg, "CHG|ROUTE_ID", "CHG3_ROUTE", false);
                        FixedMultiHeader(dg, "CHG|GRADE", "CHG3_GRADE", false);
                        FixedMultiHeader(dg, "CHG|BOX_ID", "CHG3_EQP_ID", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_START_TIME", "CHG3_STARTTIME", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_TIME", "CHG3TIME", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_CAPA_VAL", "CHG3_CAPA_VAL", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_AVG_VOLT_VAL", "CHG3_AVG_VLTG_VAL", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_END_VOLT_VAL", "CHG3_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_END_CURNT_VAL", "CHG3_END_CURNT_VAL", false);
                        //FixedMultiHeader(dg, "CHARGE|FIT_3_CHG_CAPA_VAL", "FITTED_CHG3_CAPA_VAL", false);
                        FixedMultiHeader(dg, "CHG|3_CHG_END_TEMP", "CHG3_END_TEMP", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_12"))
                    {
                        //DISCHARGE
                        FixedMultiHeader(dg, "DCHG|TRAY_LOT_ID", "DCHG1_LOTID", false);
                        FixedMultiHeader(dg, "DCHG|CHANNEL", "DCHG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "DCHG|ROUTE_ID", "DCHG1_ROUTE", false);
                        FixedMultiHeader(dg, "DCHG|GRADE", "DCHG1_GRADE", false);
                        FixedMultiHeader(dg, "DCHG|BOX_ID", "DCHG1_EQP_ID", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_START_TIME", "DCHG1_STARTTIME", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_TIME", "DCHG1TIME", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_CAPA_VAL", "DCHG1_CAPA_VAL", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_AVG_VOLT_VAL", "DCHG1_AVG_VLTG_VAL", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_END_VOLT_VAL", "DCHG1_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_END_CURNT_VAL", "DCHG1_END_CURNT_VAL", false);
                        FixedMultiHeader(dg, "DCHG|FIT_1_DCHG_CAPA_VAL", "FITTED_DCHG1_CAPA_VAL", false);
                        FixedMultiHeader(dg, "DCHG|1_DCHG_AVG_TEMP", "DCHG1_AVG_TEMP", false);

                        FixedMultiHeader(dg, "DCHG|TRAY_LOT_ID", "DCHG2_LOTID", false);
                        FixedMultiHeader(dg, "DCHG|CHANNEL", "DCHG2_CSTSLOT", false);
                        FixedMultiHeader(dg, "DCHG|ROUTE_ID", "DCHG2_ROUTE", false);
                        FixedMultiHeader(dg, "DCHG|GRADE", "DCHG2_GRADE", false);
                        FixedMultiHeader(dg, "DCHG|BOX_ID", "DCHG2_EQP_ID", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_START_TIME", "DCHG2_STARTTIME", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_TIME", "DCHG2TIME", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_CAPA_VAL", "DCHG2_CAPA_VAL", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_AVG_VOLT_VAL", "DCHG2_AVG_VLTG_VAL", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_END_VOLT_VAL", "DCHG2_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_END_CURNT_VAL", "DCHG2_END_CURNT_VAL", false);
                        FixedMultiHeader(dg, "DCHG|FIT_2_DCHG_CAPA_VAL", "FITTED_DCHG2_CAPA_VAL", false);
                        FixedMultiHeader(dg, "DCHG|2_DCHG_AVG_TEMP", "DCHG2_AVG_TEMP", false);

                        FixedMultiHeader(dg, "DCHG|TRAY_LOT_ID", "DCHG3_LOTID", false);
                        FixedMultiHeader(dg, "DCHG|CHANNEL", "DCHG3_CSTSLOT", false);
                        FixedMultiHeader(dg, "DCHG|ROUTE_ID", "DCHG3_ROUTE", false);
                        FixedMultiHeader(dg, "DCHG|GRADE", "DCHG3_GRADE", false);
                        FixedMultiHeader(dg, "DCHG|BOX_ID", "DCHG3_EQP_ID", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_START_TIME", "DCHG3_STARTTIME", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_TIME", "DCHG3TIME", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_CAPA_VAL", "DCHG3_CAPA_VAL", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_AVG_VOLT_VAL", "DCHG3_AVG_VLTG_VAL", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_END_VOLT_VAL", "DCHG3_END_VOLT_VAL", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_END_CURNT_VAL", "DCHG3_END_CURNT_VAL", false);
                        FixedMultiHeader(dg, "DCHG|FIT_3_DCHG_CAPA_VAL", "FITTED_DCHG3_CAPA_VAL", false);
                        FixedMultiHeader(dg, "DCHG|3_DCHG_AVG_TEMP", "DCHG3_AVG_TEMP", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_19"))
                    {
                        //POWERDISCHARGE
                        FixedMultiHeader(dg, "POWERDISCHARGE|TRAY_LOT_ID", "POWERDCHG1_LOTID", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|CHANNEL", "POWERDCHG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|ROUTE_ID", "POWERDCHG1_ROUTE", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|GRADE", "POWERDCHG1_GRADE", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|BOX_ID", "POWERDCHG1_EQP_ID", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|START_TIME", "POWERDCHG1_STARTTIME", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|1_POWER_DCHG_TIME", "POWERDCHG1TIME", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|1_POWER_DCHG_IMP_VAL", "POWERDCHG1_IMP_VAL", false);
                        FixedMultiHeader(dg, "POWERDISCHARGE|FIT_1_POWERDCHG_IMP_VAL", "FITTED_POWERDCHG1_IMP_VAL", false);
                        //FixedMultiHeader(dg, "POWERDISCHARGE|ML_FIT_1_POWERDCHG_CAPA_VAL", "ML_FITTED_POWERDCHG1_CAPA_VAL", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_17"))
                    {
                        //POWERCHARGE
                        FixedMultiHeader(dg, "POWERCHARGE|TRAY_LOT_ID", "POWERCHG1_LOTID", false);
                        FixedMultiHeader(dg, "POWERCHARGE|CHANNEL", "POWERCHG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "POWERCHARGE|ROUTE_ID", "POWERCHG1_ROUTE", false);
                        FixedMultiHeader(dg, "POWERCHARGE|GRADE", "POWERCHG1_GRADE", false);
                        FixedMultiHeader(dg, "POWERCHARGE|BOX_ID", "POWERCHG1_EQP_ID", false);
                        FixedMultiHeader(dg, "POWERCHARGE|START_TIME", "POWERCHG1_STARTTIME", false);
                        FixedMultiHeader(dg, "POWERCHARGE|1_POWER_CHG_TIME", "POWERCHG1TIME", false);
                        FixedMultiHeader(dg, "POWERCHARGE|1_POWER_CHG_IMP_VAL", "POWERCHG1_IMP_VAL", false);
                        FixedMultiHeader(dg, "POWERCHARGE|FIT_1_POWERCHG_IMP_VAL", "FITTED_POWERCHG1_IMP_VAL", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_81"))
                    {
                        //PRIVATE OCV
                        for (int i = 1; i <= 10; i++)
                        {
                            FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV" + i + "_LOTID", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV" + i + "_CSTSLOT", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV" + i + "_ROUTE", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV" + i + "_GRADE", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|" + i + "_POCV_BOX_ID", "DEDICATE_OCV" + i + "_EQP_ID", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|" + i + "_POCV_START_TIME", "DEDICATE_OCV" + i + "_STARTTIME", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|" + i + "_POCV_TIME", "DEDICATE_OCV" + i + "_TIME", false);
                            FixedMultiHeader(dg, "PRIVT_OCV|" + i + "_POCV", "DEDICATE_OCV" + i, false);
                        }

                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV1_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV1_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV1_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV1_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV1_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV1_TIME", "DEDICATE_OCV1_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV1", "DEDICATE_OCV1", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV2_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV2_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV2_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV2_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV2_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV2_TIME", "DEDICATE_OCV2_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV2", "DEDICATE_OCV2", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV3_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV3_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV3_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV3_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV3_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV3_TIME", "DEDICATE_OCV3_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV3", "DEDICATE_OCV3", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV4_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV4_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV4_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV4_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV4_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV4_TIME", "DEDICATE_OCV4_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV4", "DEDICATE_OCV4", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV5_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV5_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV5_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV5_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV5_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV5_TIME", "DEDICATE_OCV5_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV5", "DEDICATE_OCV5", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV6_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV6_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV6_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV6_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV6_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV6_TIME", "DEDICATE_OCV6_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV6", "DEDICATE_OCV6", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV7_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV7_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV7_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV7_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV7_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV7_TIME", "DEDICATE_OCV7_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV7", "DEDICATE_OCV7", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV8_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV8_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV8_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV8_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV8_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV8_TIME", "DEDICATE_OCV8_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV8", "DEDICATE_OCV8", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV9_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV9_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV9_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV9_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV9_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV9_TIME", "DEDICATE_OCV9_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV9", "DEDICATE_OCV9", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|TRAY_LOT_ID", "DEDICATE_OCV10_LOTID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|CHANNEL", "DEDICATE_OCV10_CSTSLOT", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|ROUTE_ID", "DEDICATE_OCV10_ROUTE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|GRADE", "DEDICATE_OCV10_GRADE", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|BOX_ID", "DEDICATE_OCV10_EQP_ID", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV10_TIME", "DEDICATE_OCV10_TIME", false);
                        //FixedMultiHeader(dg, "PRIVT_OCV|DEDICATE_OCV10", "DEDICATE_OCV10", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_A1"))
                    {
                        //DELTA OCV
                        for (int i = 1; i <= 10; i++)
                        {
                            FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV" + i + "_LOTID", false);
                            FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV" + i + "_CSTSLOT", false);
                            FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV" + i + "_ROUTE", false);
                            FixedMultiHeader(dg, "DOCV|GRADE", "DOCV" + i + "_GRADE", false);
                            FixedMultiHeader(dg, "DOCV|" + i + "_DOCV_BOX_ID", "DOCV" + i + "_EQP_ID", false);
                            FixedMultiHeader(dg, "DOCV|" + i + "_DOCV_START_TIME", "DOCV" + i + "_STARTTIME", false);
                            FixedMultiHeader(dg, "DOCV|" + i + "_DOCV_TIME", "DOCV" + i + "_TIME", false);
                            FixedMultiHeader(dg, "DOCV|" + i + "_DOCV", "DOCV" + i, false);
                        }

                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV1_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV1_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV1_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV1_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV1_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV1_TIME", "DOCV1_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV1", "DOCV1", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV2_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV2_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV2_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV2_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV2_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV2_TIME", "DOCV2_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV2", "DOCV2", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV3_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV3_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV3_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV3_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV3_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV3_TIME", "DOCV3_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV3", "DOCV3", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV4_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV4_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV4_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV4_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV4_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV4_TIME", "DOCV4_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV4", "DOCV4", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV5_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV5_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV5_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV5_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV5_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV5_TIME", "DOCV5_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV5", "DOCV5", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV6_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV6_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV6_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV6_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV6_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV6_TIME", "DOCV6_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV6", "DOCV6", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV7_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV7_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV7_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV7_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV7_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV7_TIME", "DOCV7_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV7", "DOCV7", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV8_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV8_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV8_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV8_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV8_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV8_TIME", "DOCV8_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV8", "DOCV8", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV9_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV9_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV9_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV9_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV9_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV9_TIME", "DOCV9_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV9", "DOCV9", false);
                        //FixedMultiHeader(dg, "DOCV|TRAY_LOT_ID", "DOCV10_LOTID", false);
                        //FixedMultiHeader(dg, "DOCV|CHANNEL", "DOCV10_CSTSLOT", false);
                        //FixedMultiHeader(dg, "DOCV|ROUTE_ID", "DOCV10_ROUTE", false);
                        //FixedMultiHeader(dg, "DOCV|GRADE", "DOCV10_GRADE", false);
                        //FixedMultiHeader(dg, "DOCV|BOX_ID", "DOCV10_EQP_ID", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV10_TIME", "DOCV10_TIME", false);
                        //FixedMultiHeader(dg, "DOCV|DOCV10", "DOCV10", false);
                    }
                    // 2024.06.12 YHK : Slope OCV 항목 추가
                    //else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_A3"))
                    //{
                    //    //SLOPE OCV
                    //    for (int i = 1; i <= 10; i++)
                    //    {
                    //        FixedMultiHeader(dg, "SOCV|TRAY_LOT_ID", "SOCV" + i + "_LOTID", false);
                    //        FixedMultiHeader(dg, "SOCV|CHANNEL", "SOCV" + i + "_CSTSLOT", false);
                    //        FixedMultiHeader(dg, "SOCV|ROUTE_ID", "SOCV" + i + "_ROUTE", false);
                    //        FixedMultiHeader(dg, "SOCV|GRADE", "SOCV" + i + "_GRADE", false);
                    //        FixedMultiHeader(dg, "SOCV|BOX_ID", "SOCV" + i + "_EQP_ID", false);
                    //        FixedMultiHeader(dg, "SOCV|START_TIME", "SOCV" + i + "_STARTTIME", false);
                    //        FixedMultiHeader(dg, "SOCV|SOCV" + i + "_TIME", "SOCV" + i + "_TIME", false);
                    //        FixedMultiHeader(dg, "SOCV|SOCV" + i, "SOCV" + i, false);
                    //    }
                    //}
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_51"))
                    {
                        //EOL
                        FixedMultiHeader(dg, "EOL|TRAY_LOT_ID", "EOL_LOTID", false);
                        FixedMultiHeader(dg, "EOL|CHANNEL", "EOL_CSTSLOT", false);
                        FixedMultiHeader(dg, "EOL|ROUTE_ID", "EOL_ROUTE", false);
                        FixedMultiHeader(dg, "EOL|GRADE", "EOL_GRADE", false);
                        FixedMultiHeader(dg, "EOL|BOX_ID", "EOL_EQP_ID", false);
                        FixedMultiHeader(dg, "EOL|START_TIME", "EOL_STARTTIME", false);
                        FixedMultiHeader(dg, "EOL|REPORT_TIME", "EOL_TIME", false);
                        FixedMultiHeader(dg, "EOL|THICKNESS", "THIC_VAL", false);
                        FixedMultiHeader(dg, "EOL|WEIGHT_VAL", "WEIGHT_VAL", false);
                        FixedMultiHeader(dg, "EOL|EOL_COMPUTE_VAL", "EOL_COMPUTE_VAL", false);
                        FixedMultiHeader(dg, "EOL|EOL_FILLING_LOSS", "EOL_FILLING_LOSS", false);
                        FixedMultiHeader(dg, "EOL|VOLT_VAL1", "VOLT_VAL1", false);
                        FixedMultiHeader(dg, "EOL|VOLT_VAL2", "VOLT_VAL2", false);
                        FixedMultiHeader(dg, "EOL|ACIR_VAL", "ACIR_VAL", false);
                        //FixedMultiHeader(dg, "EOL|DCIR_VAL", "DCIR_VAL", false);
                        FixedMultiHeader(dg, "EOL|IR_VAL", "IR_VAL", false);
                        FixedMultiHeader(dg, "EOL|MVDAY", "MVDAY", false);
                        FixedMultiHeader(dg, "EOL|MVDAY_SPEC", "MVDAY_SPEC", false);
                        FixedMultiHeader(dg, "EOL|IV_VAL_V", "IV_CA_VAL", false);
                        //FixedMultiHeader(dg, "EOL|TCK_MAX_VAL", "TCK_MAX_VAL", false);
                        FixedMultiHeader(dg, "EOL|GRADE_2D_PRINT", "VERIFY_GRADE", false);
                        FixedMultiHeader(dg, "EOL|EOL_REWORK_CNT", "EOL_REWORK_CNT", false);
                        FixedMultiHeader(dg, "EOL|OCV_TMPR", "OCV_TMPR", false);
                        FixedMultiHeader(dg, "EOL|MVDAY_TMPR", "MVDAY_TMPR", false);
                        FixedMultiHeader(dg, "EOL|ADJ_MVDAY_VALUE", "ADJ_MVDAY_VALUE", false);
                        FixedMultiHeader(dg, "EOL|TMPR_DIFF", "TMPR_DIFF", false);
                        FixedMultiHeader(dg, "EOL|SLOPE", "SLOPE", false);
                        FixedMultiHeader(dg, "EOL|INTERCEPT", "INTERCEPT", false);
                        FixedMultiHeader(dg, "EOL|ADJ_DOCV", "ADJ_DOCV", false);
                        FixedMultiHeader(dg, "EOL|TAPE_UNIT", "TAPE_UNIT", false);
                        FixedMultiHeader(dg, "EOL|TAPE_CSTT_NO", "TAPE_CSTT_NO", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_D1"))
                    {
                        //FITTED_DOCV
                        //FixedMultiHeader(dg, "FITTED_DOCV2|PREDCT_DOCV2", "PREDCT_DOCV2", false);
                        //FixedMultiHeader(dg, "FITTED_DOCV2|TRGT_DOCV2", "TRGT_DOCV2", false);
                        //FixedMultiHeader(dg, "FITTED_DOCV2|ALG_VER", "ALG_VER", false);
                        FixedMultiHeader(dg, "FITTED_DOCV2|FITTED_DOCV2", "FITTED_DOCV2", false);
                        //FixedMultiHeader(dg, "FITTED_DOCV2|AVG_VALUE", "DOCV_AVG", false);
                        FixedMultiHeader(dg, "FITTED_DOCV2|DOCV_STARTTIME", "DOCV_STARTTIME", false);
                        //FixedMultiHeader(dg, "FITTED_DOCV2|STDEV_VALUE", "DOCV_STDEV", false);
                        //FixedMultiHeader(dg, "FITTED_DOCV2|L_SPEC", "DOCV_LSPEC", false);
                        //FixedMultiHeader(dg, "FITTED_DOCV2|SIGMA", "SIGMA_VALUE", false);
                        FixedMultiHeader(dg, "FITTED_DOCV2|DOCV2_JUDG_STATUS", "DOCV2_JUDG_STATUS", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_71"))
                    {
                        //ShipAging
                        FixedMultiHeader(dg, "SHIP_AGING|TRAY_LOT_ID", "SHIP1_LOTID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|CHANNEL", "SHIP1_CSTSLOT", false);
                        FixedMultiHeader(dg, "SHIP_AGING|ROUTE_ID", "SHIP1_ROUTE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|GRADE", "SHIP1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|BOX_ID", "SHIP1_EQP_ID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP1_AGING_START_TIME", "SHIP1_STARTTIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP1_AGING_END_DATE", "SHIP1_END_TIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP1_AGING_TIME", "SHIP1AGING", false);

                        FixedMultiHeader(dg, "SHIP_AGING|TRAY_LOT_ID", "SHIP2_LOTID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|CHANNEL", "SHIP2_CSTSLOT", false);
                        FixedMultiHeader(dg, "SHIP_AGING|ROUTE_ID", "SHIP2_ROUTE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|GRADE", "SHIP2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|BOX_ID", "SHIP2_EQP_ID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP2_AGING_START_TIME", "SHIP2_STARTTIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP2_AGING_END_DATE", "SHIP2_END_TIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP2_AGING_TIME", "SHIP2AGING", false);

                        FixedMultiHeader(dg, "SHIP_AGING|TRAY_LOT_ID", "SHIP3_LOTID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|CHANNEL", "SHIP3_CSTSLOT", false);
                        FixedMultiHeader(dg, "SHIP_AGING|ROUTE_ID", "SHIP3_ROUTE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|GRADE", "SHIP3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|BOX_ID", "SHIP3_EQP_ID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP3_AGING_START_TIME", "SHIP3_STARTTIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP3_AGING_END_DATE", "SHIP3_END_TIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP3_AGING_TIME", "SHIP3AGING", false);

                        FixedMultiHeader(dg, "SHIP_AGING|TRAY_LOT_ID", "SHIP4_LOTID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|CHANNEL", "SHIP4_CSTSLOT", false);
                        FixedMultiHeader(dg, "SHIP_AGING|ROUTE_ID", "SHIP4_ROUTE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|GRADE", "SHIP4_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "SHIP_AGING|BOX_ID", "SHIP4_EQP_ID", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP4_AGING_START_TIME", "SHIP4_STARTTIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP4_AGING_END_DATE", "SHIP4_END_TIME", false);
                        FixedMultiHeader(dg, "SHIP_AGING|SHIP4_AGING_TIME", "SHIP4AGING", false);
                    }
                    else if (Util.NVC(dr["PKEY_VAL"]).Equals("TOP") && Util.NVC(dr["KEY_VAL"]).Equals("DA_B1"))   // 2024.02.20 판정 추가
                    {
                        //JUDGE
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG1_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG1_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG1_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG1_GRADE", "JUDG1_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG1_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG1_TIME", "JUDG1_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG2_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG2_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG2_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG2_GRADE", "JUDG2_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG2_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG2_TIME", "JUDG2_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG3_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG3_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG3_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG3_GRADE", "JUDG3_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG3_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG3_TIME", "JUDG3_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG4_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG4_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG4_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG4_GRADE", "JUDG4_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG4_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG4_TIME", "JUDG4_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG5_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG5_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG5_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG5_GRADE", "JUDG5_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG5_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG5_TIME", "JUDG5_TIME", false);
                        FixedMultiHeader(dg, "JUDGE|TRAY_LOT_ID", "JUDG6_LOTID", false);
                        FixedMultiHeader(dg, "JUDGE|CHANNEL", "JUDG6_CSTSLOT", false);
                        FixedMultiHeader(dg, "JUDGE|ROUTE_ID", "JUDG6_ROUTE", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG6_GRADE", "JUDG6_FINL_JUDG_CODE", false);
                        FixedMultiHeader(dg, "JUDGE|BOX_ID", "JUDG6_EQP_ID", false);
                        FixedMultiHeader(dg, "JUDGE|JUDG6_TIME", "JUDG6_TIME", false);

                    }
                }
                dgAfter.TopRowHeaderMerge();
            }
            #endregion
        }

        private void GetProdLotList(string button)
        {
            try
            {
                ShowLoadingIndicator();

                if (button.Equals("btnBeforeSearchLot"))
                    Util.gridClear(dgBeforeSerchList);
                if (button.Equals("btnAfterSearchLot"))
                    Util.gridClear(dgAfterSerchList);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow drPkgLot = dtRqst.NewRow();

                if (button.Equals("btnBeforeSearchLot"))
                    drPkgLot["PROD_LOTID"] = Util.GetCondition(txtLotbefore);
                if (button.Equals("btnAfterSearchLot"))
                    drPkgLot["PROD_LOTID"] = Util.GetCondition(txtLotAfter);

                if (string.IsNullOrEmpty(drPkgLot["PROD_LOTID"].ToString()))
                {
                    Util.MessageInfo("FM_ME_0049");  //Lot ID를 입력해주세요.
                    HiddenLoadingIndicator();
                    return;
                }

                if (drPkgLot["PROD_LOTID"].ToString().Length < 6)
                {
                    Util.MessageInfo("FM_ME_0048");  //Lot ID는 최소 7자리이상 입력해주세요.
                    HiddenLoadingIndicator();
                    return;
                }

                dtRqst.Rows.Add(drPkgLot);

                if (button.Equals("btnBeforeSearchLot"))
                    dgBeforeSerchList.ExecuteService("DA_SEL_SUBLOTID_LIST_BY_PRODLOTID", "RQSTDT", "RSLTDT", dtRqst, false);
                if (button.Equals("btnAfterSearchLot"))
                    dgAfterSerchList.ExecuteService("DA_SEL_SUBLOTID_LIST_BY_PRODLOTID", "RQSTDT", "RSLTDT", dtRqst, false);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetBeforeList()
        {
            try
            {

                if (!string.IsNullOrEmpty(txtLotbefore.Text) && Util.GetCondition(txtLotbefore).Length < 6)
                {
                    Util.MessageInfo("SFU4074", "6");  // LOTID %1자리 이상 입력시 조회 가능합니다.
                    return;

                }

                ShowLoadingIndicator();

                Util.gridClear(dgBefore);

                bool chk = false;
                String option = String.Empty;
                DataTable dtRslt = new DataTable();

                dtRsltCell = null;
                dtRsltHpcd = null;
                dtRsltPre = null;
                dtRsltHigh = null;
                dtRsltChgBefore = null;
                dtRsltOcvBefore = null;
                dtRsltDegas = null;
                dtRsltPp = null;
                dtRsltRest = null;
                dtRsltRestHeating = null;
                dtRsltNomal1 = null;
                dtRsltNomal2 = null;
                dtRsltJudgBefore = null;

                DataTable dtCellList = dgBeforeSerchList.GetDataTable();
                dtCellList.AsEnumerable().Where(w => Util.NVC(w.Field<string>("SUBLOTID")).Equals(string.Empty)).ToList().ForEach(row => row.Delete());
                dtCellList.AcceptChanges();

                for (int i = 0; i < tvBefore.Items.Count; i++)
                {
                    MenuItem mn = tvBefore.Items[i] as MenuItem;
                    for (int j = 0; j < mn.Items.Count; j++)
                    {
                        chk = mn.Items[j].IsChecked;
                        option = mn.Items[j].Tag;

                        if (dtCellList != null && dtCellList.Rows.Count > 0)
                        {
                            GetBeforeCellData(chk, option, ref dtRslt);
                        }
                        else if (!string.IsNullOrEmpty(txtLotbefore.Text.Trim()))
                        {
                            GetBeforeLotData(chk, option, ref dtRslt);
                        }
                    }
                }

                // 입력된 Cell과 순서 동기화
                if (dtCellList != null && dtCellList.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("SORT_ORDER", typeof(int));
                    foreach (DataRow dr in dtRslt.Rows)
                    {
                        List<DataRow> findRows = dtCellList.AsEnumerable().Where(w => w.Field<string>("SUBLOTID") != null && w.Field<string>("SUBLOTID").Equals(Util.NVC(dr["SUBLOTID"]))).ToList();
                        if (findRows != null && findRows.Count > 0)
                        {
                            dr["SORT_ORDER"] = dtCellList.Rows.IndexOf(findRows[0]);
                        }
                    }
                    DataView dvRslt = dtRslt.DefaultView;
                    dvRslt.Sort = "SORT_ORDER";
                    dtRslt = dvRslt.ToTable();
                }

                Util.GridSetData(dgBefore, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HiddenLoadingIndicator();
            }
        }

        private void GetBeforeLotData(bool chk, String option, ref DataTable dtRsltTemp)
        {
            if (Util.GetCondition(txtLotbefore).Length < 6)
            {
                Util.MessageInfo("SFU4074", "6");  // LOTID %1자리 이상 입력시 조회 가능합니다.
                HiddenLoadingIndicator();
                return;
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("SEQ", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow drBefore = dtRqst.NewRow();
            drBefore["LANGID"] = LoginInfo.LANGID;
            drBefore["PROD_LOTID"] = Util.GetCondition(txtLotbefore);
            drBefore["PROCID"] = DBNull.Value;
            drBefore["S26"] = DBNull.Value;
            drBefore["SEQ"] = DBNull.Value;
            drBefore["AREAID"] = LoginInfo.CFG_AREA_ID;
            drBefore["SUBLOTID"] = DBNull.Value;
            dtRqst.Rows.Add(drBefore);

            if (chk && option.Contains("DB_C"))
            {
                if (dtRsltCell != null)
                    return;

                //Cell LIst
                dtRsltCell = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_CELL", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltCell.PrimaryKey = new DataColumn[] { dtRsltCell.Columns["PROD_LOTID"], dtRsltCell.Columns["EQSGID"], dtRsltCell.Columns["SUBLOTID"], dtRsltCell.Columns["ROUTID"] };
                dtRsltTemp.Merge(dtRsltCell);
            }
            if (chk && option.Contains("DB_U1"))
            {
                if (dtRsltHpcd != null)
                    return;

                //HPCD
                dtRsltHpcd = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_HPCD", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltHpcd.PrimaryKey = new DataColumn[] { dtRsltHpcd.Columns["PROD_LOTID"], dtRsltHpcd.Columns["EQSGID"], dtRsltHpcd.Columns["SUBLOTID"], dtRsltHpcd.Columns["ROUTID"] };
                dtRsltTemp.Merge(dtRsltHpcd);
            }
            if (chk && option.Contains("DB_91"))
            {
                if (dtRsltPre != null)
                    return;

                //AGING
                dtRqst.Rows[0]["PROCID"] = "FF9101";  //PreAging
                dtRqst.Rows[0]["S26"] = "9";  //PreAging
                dtRsltPre = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltPre.PrimaryKey = new DataColumn[] { dtRsltPre.Columns["PROD_LOTID"], dtRsltPre.Columns["EQSGID"], dtRsltPre.Columns["SUBLOTID"], dtRsltPre.Columns["ROUTID"] };

                //표시 대상 칼럼 저장
                List<string> displayList = new List<string>();
                string compStr = "PRE";
                foreach (DataColumn column in dtRsltPre.Columns)
                {
                    if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                        || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                        || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                        || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                        || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr))
                    {
                        displayList.Add(column.ColumnName.ToString().ToUpper());
                    }
                }

                //표시 대상 칼럼 이외는 제거하기 위해 복사
                DataTable dtTemp = dtRsltPre.Clone();

                //표시 대상 칼럼 이외는 제거
                foreach (DataColumn column in dtTemp.Columns)
                {
                    if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                    {
                        dtRsltPre.Columns.Remove(dtRsltPre.Columns[column.ColumnName]);
                    }
                }

                dtRsltTemp.Merge(dtRsltPre);
            }
            if (chk && option.Contains("DB_41"))
            {
                if (dtRsltHigh != null)
                    return;

                dtRqst.Rows[0]["PROCID"] = "FF4101";  //고온 Aging
                dtRqst.Rows[0]["S26"] = "4";  //고온 Aging
                dtRsltHigh = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltHigh.PrimaryKey = new DataColumn[] { dtRsltHigh.Columns["PROD_LOTID"], dtRsltHigh.Columns["EQSGID"], dtRsltHigh.Columns["SUBLOTID"], dtRsltHigh.Columns["ROUTID"] };

                //표시 대상 칼럼 저장
                List<string> displayList = new List<string>();
                string compStr = "HIGH";
                foreach (DataColumn column in dtRsltHigh.Columns)
                {
                    if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                        || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                        || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                        || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                        || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr))
                    {
                        displayList.Add(column.ColumnName.ToString().ToUpper());
                    }
                }

                //표시 대상 칼럼 이외는 제거하기 위해 복사
                DataTable dtTemp = dtRsltHigh.Clone();

                //표시 대상 칼럼 이외는 제거
                foreach (DataColumn column in dtTemp.Columns)
                {
                    if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                    {
                        dtRsltHigh.Columns.Remove(dtRsltHigh.Columns[column.ColumnName]);
                    }
                }

                dtRsltTemp.Merge(dtRsltHigh);
            }
            if (chk && option.Contains("DB_J1"))
            {
                if (dtRsltChgBefore != null)
                    return;

                //CHG, JIG
                for (int iChg = 1; iChg <= 3; iChg++)
                {
                    dtRqst.Rows[0]["SEQ"] = iChg.ToString();
                    dtRsltChgBefore = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRsltChgBefore.PrimaryKey = new DataColumn[] { dtRsltChgBefore.Columns["PROD_LOTID"], dtRsltChgBefore.Columns["EQSGID"], dtRsltChgBefore.Columns["SUBLOTID"], dtRsltChgBefore.Columns["ROUTID"] };

                    List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                    foreach (DataColumn column in dtRsltChgBefore.Columns)
                    {
                        // 컬럼이름 CHG1_LOTID, CHG2_LOTID...
                        if (column.ColumnName.Substring(0, 3).Equals("CHG") == false) continue; // 컬럼이름 앞 3자리가 CHG 가 아니면 스킵

                        // 앞 3자리가 CHG 이면...
                        string compareName = "CHG" + iChg.ToString() + "_"; // 비교하기 위해 앞 5자리 구성
                        if (column.ColumnName.Substring(0, 5).Equals(compareName) == false)  // 앞 5자리가 비교이름과 다르면 삭제하기 위해 담음.
                        {
                            deleteColumns.Add(column);
                        }
                    }
                    // 담아놓은 컬럼 삭제
                    deleteColumns.ForEach(delColumn => dtRsltChgBefore.Columns.Remove(delColumn));

                    dtRsltTemp.Merge(dtRsltChgBefore);
                }
            }
            if (chk && option.Contains("DB_J3"))
            {
                if (dtRsltOcvBefore != null)
                    return;

                //OCV
                for (int iOcv = 1; iOcv <= 3; iOcv++)
                {
                    dtRqst.Rows[0]["SEQ"] = iOcv.ToString();
                    dtRsltOcvBefore = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_OCV", "RQSTDT", "RSLTDT", dtRqst);
                    dtRsltOcvBefore.PrimaryKey = new DataColumn[] { dtRsltOcvBefore.Columns["PROD_LOTID"], dtRsltOcvBefore.Columns["EQSGID"], dtRsltOcvBefore.Columns["SUBLOTID"], dtRsltOcvBefore.Columns["ROUTID"] };

                    List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                    foreach (DataColumn column in dtRsltOcvBefore.Columns)
                    {
                        // 컬럼이름 OCV1_LOTID, OCV2_LOTID...
                        if (column.ColumnName.Substring(0, 3).Equals("OCV") == false) continue; // 컬럼이름 앞 3자리가 CHG 가 아니면 스킵

                        // 앞 3자리가 OCVG 이면...
                        string compareName = "OCV" + iOcv.ToString() + "_"; // 비교하기 위해 앞 5자리 구성
                        if (column.ColumnName.Substring(0, 5).Equals(compareName) == false)  // 앞 5자리가 비교이름과 다르면 삭제하기 위해 담음.
                        {
                            deleteColumns.Add(column);
                        }
                    }
                    // 담아놓은 컬럼 삭제
                    deleteColumns.ForEach(delColumn => dtRsltOcvBefore.Columns.Remove(delColumn));

                    dtRsltTemp.Merge(dtRsltOcvBefore);
                }
            }
            if (chk && option.Contains("DB_D1"))
            {
                if (dtRsltDegas != null)
                    return;

                //DEGAS
                dtRsltDegas = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_DEGAS", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltDegas.PrimaryKey = new DataColumn[] { dtRsltDegas.Columns["PROD_LOTID"], dtRsltDegas.Columns["EQSGID"], dtRsltDegas.Columns["SUBLOTID"], dtRsltDegas.Columns["ROUTID"] };
                dtRsltTemp.Merge(dtRsltDegas);
            }
            if (chk && option.Contains("DB_J7"))
            {
                if (dtRsltPp != null)
                    return;

                //PP, JIG
                for (int iPp = 1; iPp <= 3; iPp++)
                {
                    dtRqst.Rows[0]["SEQ"] = iPp.ToString();
                    dtRsltPp = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_PP", "RQSTDT", "RSLTDT", dtRqst);
                    dtRsltPp.PrimaryKey = new DataColumn[] { dtRsltPp.Columns["PROD_LOTID"], dtRsltPp.Columns["EQSGID"], dtRsltPp.Columns["SUBLOTID"], dtRsltPp.Columns["ROUTID"] };

                    List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                    foreach (DataColumn column in dtRsltPp.Columns)
                    {
                        // 컬럼이름 PP1_LOTID, PP2_LOTID...
                        if (column.ColumnName.Substring(0, 2).Equals("PP") == false) continue; // 컬럼이름 앞 2자리가 PP 가 아니면 스킵

                        // 앞 2자리가 CHG 이면...
                        string compareName = "PP" + iPp.ToString() + "_"; // 비교하기 위해 앞 4자리 구성
                        if (column.ColumnName.Substring(0, 4).Equals(compareName) == false)  // 앞 4자리가 비교이름과 다르면 삭제하기 위해 담음.
                        {
                            deleteColumns.Add(column);
                        }
                    }
                    // 담아놓은 컬럼 삭제
                    deleteColumns.ForEach(delColumn => dtRsltPp.Columns.Remove(delColumn));

                    dtRsltTemp.Merge(dtRsltPp);
                }
            }

            if (chk && option.Contains("DB_31"))
            {
                if (dtRsltNomal1 != null)
                    return;

                dtRqst.Rows[0]["PROCID"] = "FF3101";  //Nomal Aging
                dtRqst.Rows[0]["S26"] = "31";  //Nomal Aging
                dtRsltNomal1 = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltNomal1.PrimaryKey = new DataColumn[] { dtRsltNomal1.Columns["PROD_LOTID"], dtRsltNomal1.Columns["EQSGID"], dtRsltNomal1.Columns["SUBLOTID"], dtRsltNomal1.Columns["ROUTID"] };

                //표시 대상 칼럼 저장
                List<string> displayList = new List<string>();
                //string compStr = "NOMAL";
                foreach (DataColumn column in dtRsltNomal1.Columns)
                {
                    if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                        || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                        || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                        || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                        || column.ColumnName.ToString().ToUpper().Contains("NOMAL_")
                        || column.ColumnName.ToString().ToUpper().Equals("NOMALAGING"))
                    {
                        displayList.Add(column.ColumnName.ToString().ToUpper());
                    }
                }

                //표시 대상 칼럼 이외는 제거하기 위해 복사
                DataTable dtTemp = dtRsltNomal1.Clone();

                //표시 대상 칼럼 이외는 제거
                foreach (DataColumn column in dtTemp.Columns)
                {
                    if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                    {
                        dtRsltNomal1.Columns.Remove(dtRsltNomal1.Columns[column.ColumnName]);
                    }
                }

                dtRsltTemp.Merge(dtRsltNomal1);
            }

            if (chk && option.Contains("DB_32"))
            {
                if (dtRsltNomal2 != null)
                    return;

                dtRqst.Rows[0]["PROCID"] = "FF3102";  //Nomal Aging
                dtRqst.Rows[0]["S26"] = "32";  //Nomal Aging
                dtRsltNomal2 = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst);
                dtRsltNomal2.PrimaryKey = new DataColumn[] { dtRsltNomal2.Columns["PROD_LOTID"], dtRsltNomal2.Columns["EQSGID"], dtRsltNomal2.Columns["SUBLOTID"], dtRsltNomal2.Columns["ROUTID"] };

                //표시 대상 칼럼 저장
                List<string> displayList = new List<string>();
                string compStr = "NOMAL2";
                foreach (DataColumn column in dtRsltNomal2.Columns)
                {
                    if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                        || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                        || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                        || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                        || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                        || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr))
                    {
                        displayList.Add(column.ColumnName.ToString().ToUpper());
                    }
                }

                //표시 대상 칼럼 이외는 제거하기 위해 복사
                DataTable dtTemp = dtRsltNomal2.Clone();

                //표시 대상 칼럼 이외는 제거
                foreach (DataColumn column in dtTemp.Columns)
                {
                    if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                    {
                        dtRsltNomal2.Columns.Remove(dtRsltNomal2.Columns[column.ColumnName]);
                    }
                }

                dtRsltTemp.Merge(dtRsltNomal2);
            }
            if (chk && option.Contains("DB_B1")) // 2024.02.20 판정 추가
            {
                if (dtRsltJudgBefore != null)
                    return;

                //JIG JUDGE
                for (int iJudg = 1; iJudg <= 6; iJudg++)
                {
                    dtRqst.Rows[0]["SEQ"] = iJudg.ToString();
                    dtRsltJudgBefore = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_JUDG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRsltJudgBefore.PrimaryKey = new DataColumn[] { dtRsltJudgBefore.Columns["PROD_LOTID"], dtRsltJudgBefore.Columns["EQSGID"], dtRsltJudgBefore.Columns["SUBLOTID"], dtRsltJudgBefore.Columns["ROUTID"] };

                    List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                    foreach (DataColumn column in dtRsltJudgBefore.Columns)
                    {
                        // 컬럼이름 JUDG1_LOTID, JUDG2_LOTID...
                        if (column.ColumnName.Substring(0, 4).Equals("JUDG") == false) continue; // 컬럼이름 앞 4자리가 JUDG 가 아니면 스킵

                        // 앞 4자리가 JUDG 이면...
                        string compareName = "JUDG" + iJudg.ToString() + "_"; // 비교하기 위해 앞 6자리 구성
                        if (column.ColumnName.Substring(0, 6).Equals(compareName) == false)  // 앞 6자리가 비교이름과 다르면 삭제하기 위해 담음.
                        {
                            deleteColumns.Add(column);
                        }
                    }
                    // 담아놓은 컬럼 삭제
                    deleteColumns.ForEach(delColumn => dtRsltJudgBefore.Columns.Remove(delColumn));

                    dtRsltTemp.Merge(dtRsltJudgBefore);
                }
            }
            if (chk && option.Contains("DB_J5"))
            {
                if (dtRsltRestHeating != null)
                    return;

                //RESTHEATING, JIG
                for (int iRh = 1; iRh <= 3; iRh++)
                {
                    dtRqst.Rows[0]["SEQ"] = iRh.ToString();
                    dtRsltRestHeating = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_RESTHEATING", "RQSTDT", "RSLTDT", dtRqst);
                    dtRsltRestHeating.PrimaryKey = new DataColumn[] { dtRsltRestHeating.Columns["PROD_LOTID"], dtRsltRestHeating.Columns["EQSGID"], dtRsltRestHeating.Columns["SUBLOTID"], dtRsltRestHeating.Columns["ROUTID"] };

                    List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                    foreach (DataColumn column in dtRsltRestHeating.Columns)
                    {
                        // 컬럼이름 RESTHEATING1_LOTID, RESTHEATING2_LOTID...
                        //if (column.ColumnName.Substring(0, 11).Equals("RESTHEATING") == false) continue; // 컬럼이름 앞 11자리가 RESTHEATING 가 아니면 스킵
                        if (column.ColumnName.StartsWith("RESTHEATING") == false) continue; // 컬럼이름이 RESTHEATING으로 시작하지 않으면 스킵

                        // 앞 11자리가 RESTHEATING 이면...
                        string compareName = "RESTHEATING" + iRh.ToString() + "_"; // 비교하기 위해 앞 13자리 구성
                        if (column.ColumnName.Substring(0, 13).Equals(compareName) == false)  // 앞 13자리가 비교이름과 다르면 삭제하기 위해 담음.
                        {
                            deleteColumns.Add(column);
                        }
                    }
                    // 담아놓은 컬럼 삭제
                    deleteColumns.ForEach(delColumn => dtRsltRestHeating.Columns.Remove(delColumn));

                    dtRsltTemp.Merge(dtRsltRestHeating);
                }
            }
            if (chk && option.Contains("DB_J6"))
            {
                if (dtRsltRest != null)
                    return;

                //REST, JIG
                for (int iRest = 1; iRest <= 3; iRest++)
                {
                    dtRqst.Rows[0]["SEQ"] = iRest.ToString();
                    dtRsltRest = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_REST", "RQSTDT", "RSLTDT", dtRqst);
                    dtRsltRest.PrimaryKey = new DataColumn[] { dtRsltRest.Columns["PROD_LOTID"], dtRsltRest.Columns["EQSGID"], dtRsltRest.Columns["SUBLOTID"], dtRsltRest.Columns["ROUTID"] };

                    List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                    foreach (DataColumn column in dtRsltRest.Columns)
                    {
                        // 컬럼이름 RESTHEATING1_LOTID, RESTHEATING2_LOTID...
                        if (column.ColumnName.Substring(0, 4).Equals("REST") == false) continue; // 컬럼이름 앞 4자리가 RESTHEATING 가 아니면 스킵

                        // 앞 11자리가 RESTHEATING 이면...
                        string compareName = "REST" + iRest.ToString() + "_"; // 비교하기 위해 앞 6자리 구성
                        if (column.ColumnName.Substring(0, 6).Equals(compareName) == false)  // 앞 6자리가 비교이름과 다르면 삭제하기 위해 담음.
                        {
                            deleteColumns.Add(column);
                        }
                    }
                    // 담아놓은 컬럼 삭제
                    deleteColumns.ForEach(delColumn => dtRsltRest.Columns.Remove(delColumn));

                    dtRsltTemp.Merge(dtRsltRest);
                }
            }

        }

        private void GetBeforeCellData(bool chk, String option, ref DataTable dtRsltTemp)
        {
            if (dgBeforeSerchList.Rows.Count == 0)
            {
                Util.MessageInfo("FM_ME_0465");  //Cell ID와 PKG LOTID 중 하나는 반드시 입력해야 합니다.
                HiddenLoadingIndicator();
                return;
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("SEQ", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));

            String cellid = String.Empty;

            for (int i = 0; i < dgBeforeSerchList.Rows.Count; i++)
            {
                cellid = Util.NVC(DataTableConverter.GetValue(dgBeforeSerchList.Rows[i].DataItem, "SUBLOTID"));
                if (!string.IsNullOrEmpty(cellid))
                {
                    if (i % 200 == 0)
                    {
                        DataRow drBefore = dtRqst.NewRow();
                        drBefore["LANGID"] = LoginInfo.LANGID;
                        drBefore["PROCID"] = DBNull.Value;
                        drBefore["S26"] = DBNull.Value;
                        drBefore["SEQ"] = DBNull.Value;
                        drBefore["AREAID"] = LoginInfo.CFG_AREA_ID;
                        drBefore["PROD_LOTID"] = DBNull.Value;
                        drBefore["SUBLOTID"] = cellid + ",";
                        dtRqst.Rows.Add(drBefore);
                    }
                    else
                    {
                        dtRqst.Rows[dtRqst.Rows.Count - 1]["SUBLOTID"] = dtRqst.Rows[dtRqst.Rows.Count - 1]["SUBLOTID"].ToString() + cellid + ",";
                    }
                }
            }

            if (chk && option.Contains("DB_C"))
            {
                if (dtRsltCell != null)
                    return;

                //xmlbuffer 에러로 인해 여러번 호출하도록
                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //Cell LIst
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRsltCell = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_CELL", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltCell.PrimaryKey = new DataColumn[] { dtRsltCell.Columns["PROD_LOTID"], dtRsltCell.Columns["EQSGID"], dtRsltCell.Columns["SUBLOTID"], dtRsltCell.Columns["ROUTID"] };
                    dtRsltTemp.Merge(dtRsltCell);
                }
            }

            if (chk && option.Contains("DB_U1"))
            {
                if (dtRsltHpcd != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //HPCD
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRsltHpcd = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_HPCD", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltHpcd.PrimaryKey = new DataColumn[] { dtRsltHpcd.Columns["PROD_LOTID"], dtRsltHpcd.Columns["EQSGID"], dtRsltHpcd.Columns["SUBLOTID"], dtRsltHpcd.Columns["ROUTID"] };
                    dtRsltTemp.Merge(dtRsltHpcd);
                }
            }

            if (chk && option.Contains("DB_91"))
            {
                if (dtRsltPre != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //PREAGING
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRqst1.Rows[0]["PROCID"] = "FF9101";  //PreAging
                    dtRqst1.Rows[0]["S26"] = "9";  //PreAging
                    dtRsltPre = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltPre.PrimaryKey = new DataColumn[] { dtRsltPre.Columns["PROD_LOTID"], dtRsltPre.Columns["EQSGID"], dtRsltPre.Columns["SUBLOTID"], dtRsltPre.Columns["ROUTID"] };

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "PRE";
                    foreach (DataColumn column in dtRsltPre.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRsltPre.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRsltPre.Columns.Remove(dtRsltPre.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRsltPre);
                }
            }

            if (chk && option.Contains("DB_41"))
            {
                if (dtRsltHigh != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //HIGHAGING
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRqst1.Rows[0]["PROCID"] = "FF4101";  //고온 Aging
                    dtRqst1.Rows[0]["S26"] = "4";  //고온 Aging
                    dtRsltHigh = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltHigh.PrimaryKey = new DataColumn[] { dtRsltHigh.Columns["PROD_LOTID"], dtRsltHigh.Columns["EQSGID"], dtRsltHigh.Columns["SUBLOTID"], dtRsltHigh.Columns["ROUTID"] };

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "HIGH";
                    foreach (DataColumn column in dtRsltHigh.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRsltHigh.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRsltHigh.Columns.Remove(dtRsltHigh.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRsltHigh);
                }
            }

            if (chk && option.Contains("DB_J1"))
            {
                if (dtRsltChgBefore != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    for (int iChg = 1; iChg <= 3; iChg++)
                    {
                        //CHG, JIG
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["SEQ"] = iChg.ToString();
                        dtRsltChgBefore = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRsltChgBefore.PrimaryKey = new DataColumn[] { dtRsltChgBefore.Columns["PROD_LOTID"], dtRsltChgBefore.Columns["EQSGID"], dtRsltChgBefore.Columns["SUBLOTID"], dtRsltChgBefore.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltChgBefore.Columns)
                        {
                            // 컬럼이름 CHG1_LOTID, CHG2_LOTID...
                            if (column.ColumnName.Substring(0, 3).Equals("CHG") == false) continue; // 컬럼이름 앞 3자리가 CHG 가 아니면 스킵

                            // 앞 3자리가 CHG 이면...
                            string compareName = "CHG" + iChg.ToString() + "_"; // 비교하기 위해 앞 5자리 구성
                            if (column.ColumnName.Substring(0, 5).Equals(compareName) == false)  // 앞 5자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltChgBefore.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltChgBefore);
                    }
                }
            }

            if (chk && option.Contains("DB_J3"))
            {
                if (dtRsltOcvBefore != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    for (int iOcv = 1; iOcv <= 3; iOcv++)
                    {
                        //OCV
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["SEQ"] = iOcv.ToString();
                        dtRsltOcvBefore = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_OCV", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRsltOcvBefore.PrimaryKey = new DataColumn[] { dtRsltOcvBefore.Columns["PROD_LOTID"], dtRsltOcvBefore.Columns["EQSGID"], dtRsltOcvBefore.Columns["SUBLOTID"], dtRsltOcvBefore.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltOcvBefore.Columns)
                        {
                            // 컬럼이름 OCV1_LOTID, OCV2_LOTID...
                            if (column.ColumnName.Substring(0, 3).Equals("OCV") == false) continue; // 컬럼이름 앞 3자리가 CHG 가 아니면 스킵

                            // 앞 3자리가 OCVG 이면...
                            string compareName = "OCV" + iOcv.ToString() + "_"; // 비교하기 위해 앞 5자리 구성
                            if (column.ColumnName.Substring(0, 5).Equals(compareName) == false)  // 앞 5자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltOcvBefore.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltOcvBefore);
                    }
                }
            }

            if (chk && option.Contains("DB_D1"))
            {
                if (dtRsltDegas != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //DEGAS
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRsltDegas = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_DEGAS", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltDegas.PrimaryKey = new DataColumn[] { dtRsltDegas.Columns["PROD_LOTID"], dtRsltDegas.Columns["EQSGID"], dtRsltDegas.Columns["SUBLOTID"], dtRsltDegas.Columns["ROUTID"] };
                    dtRsltTemp.Merge(dtRsltDegas);
                }
            }

            if (chk && option.Contains("DB_J7"))
            {
                if (dtRsltPp != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    for (int iPp = 1; iPp <= 3; iPp++)
                    {
                        //PP, JIG
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["SEQ"] = iPp.ToString();
                        dtRsltPp = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_PP", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRsltPp.PrimaryKey = new DataColumn[] { dtRsltPp.Columns["PROD_LOTID"], dtRsltPp.Columns["EQSGID"], dtRsltPp.Columns["SUBLOTID"], dtRsltPp.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltPp.Columns)
                        {
                            // 컬럼이름 PP1_LOTID, PP2_LOTID...
                            if (column.ColumnName.Substring(0, 2).Equals("PP") == false) continue; // 컬럼이름 앞 2자리가 PP 가 아니면 스킵

                            // 앞 2자리가 CHG 이면...
                            string compareName = "PP" + iPp.ToString() + "_"; // 비교하기 위해 앞 4자리 구성
                            if (column.ColumnName.Substring(0, 4).Equals(compareName) == false)  // 앞 4자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltPp.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltPp);
                    }
                }
            }

            if (chk && option.Contains("DB_31"))
            {
                if (dtRsltNomal1 != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //NomalAGING#1
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRqst1.Rows[0]["PROCID"] = "FF3101";  //상온 Aging
                    dtRqst1.Rows[0]["S26"] = "31";  //상온 Aging
                    dtRsltNomal1 = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltNomal1.PrimaryKey = new DataColumn[] { dtRsltNomal1.Columns["PROD_LOTID"], dtRsltNomal1.Columns["EQSGID"], dtRsltNomal1.Columns["SUBLOTID"], dtRsltNomal1.Columns["ROUTID"] };

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    //string compStr = "NOMAL";
                    foreach (DataColumn column in dtRsltNomal1.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Contains("NOMAL_")
                            || column.ColumnName.ToString().ToUpper().Equals("NOMALAGING"))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRsltNomal1.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRsltNomal1.Columns.Remove(dtRsltNomal1.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRsltNomal1);
                }
            }

            if (chk && option.Contains("DB_32"))
            {
                if (dtRsltNomal2 != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    //NomalAGING#2
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRqst1.Rows[0]["PROCID"] = "FF3102";  //상온 Aging
                    dtRqst1.Rows[0]["S26"] = "32";  //상온 Aging
                    dtRsltNomal2 = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_AGING", "RQSTDT", "RSLTDT", dtRqst1);
                    dtRsltNomal2.PrimaryKey = new DataColumn[] { dtRsltNomal2.Columns["PROD_LOTID"], dtRsltNomal2.Columns["EQSGID"], dtRsltNomal2.Columns["SUBLOTID"], dtRsltNomal2.Columns["ROUTID"] };

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "NOMAL2";
                    foreach (DataColumn column in dtRsltNomal2.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            || column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRsltNomal2.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRsltNomal2.Columns.Remove(dtRsltNomal2.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRsltNomal2);
                }
            }

            if (chk && option.Contains("DB_B1"))
            {
                if (dtRsltJudgBefore != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    for (int iJudg = 1; iJudg <= 6; iJudg++)
                    {
                        //JIG JUDGE
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["SEQ"] = iJudg.ToString();
                        dtRsltJudgBefore = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_JUDG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRsltJudgBefore.PrimaryKey = new DataColumn[] { dtRsltJudgBefore.Columns["PROD_LOTID"], dtRsltJudgBefore.Columns["EQSGID"], dtRsltJudgBefore.Columns["SUBLOTID"], dtRsltJudgBefore.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltJudgBefore.Columns)
                        {
                            // 컬럼이름 JUDG1_LOTID, JUDG2_LOTID...
                            if (column.ColumnName.Substring(0, 4).Equals("JUDG") == false) continue; // 컬럼이름 앞 4자리가 JUDG 가 아니면 스킵

                            // 앞 4자리가 JUDG 이면...
                            string compareName = "JUDG" + iJudg.ToString() + "_"; // 비교하기 위해 앞 6자리 구성
                            if (column.ColumnName.Substring(0, 6).Equals(compareName) == false)  // 앞 6자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltJudgBefore.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltJudgBefore);
                    }
                }
            }

            if (chk && option.Contains("DB_J5"))
            {
                if (dtRsltRestHeating != null)
                    return;

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    for (int iRh = 1; iRh <= 3; iRh++)
                    {
                        //JF RESTHEATING
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["SEQ"] = iRh.ToString();
                        dtRsltRestHeating = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_RESTHEATING", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRsltRestHeating.PrimaryKey = new DataColumn[] { dtRsltRestHeating.Columns["PROD_LOTID"], dtRsltRestHeating.Columns["EQSGID"], dtRsltRestHeating.Columns["SUBLOTID"], dtRsltRestHeating.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltRestHeating.Columns)
                        {
                            // 컬럼이름 RESTHEATING1_LOTID, RESTHEATING2_LOTID...
                            //if (column.ColumnName.Substring(0, 11).Equals("RESTHEATING") == false) continue; // 컬럼이름 앞 11자리가 RESTHEATING 가 아니면 스킵
                            if (column.ColumnName.StartsWith("RESTHEATING") == false) continue; // 컬럼이름이 RESTHEATING으로 시작하지 않으면 스킵

                            // 앞 11자리가 RESTHEATING 이면...
                            string compareName = "RESTHEATING" + iRh.ToString() + "_"; // 비교하기 위해 앞 13자리 구성
                            if (column.ColumnName.Substring(0, 13).Equals(compareName) == false)  // 앞 13자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltRestHeating.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltRestHeating);
                    }
                }
            }

            if (chk && option.Contains("DB_J6"))
            {
                if (dtRsltRest != null)
                {
                    return;
                }

                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    for (int iRest = 1; iRest <= 3; iRest++)
                    {
                        //JF Rest
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["SEQ"] = iRest.ToString();
                        dtRsltRest = new ClientProxy().ExecuteServiceSync("DA_SEL_BEFORE_LOT_REST", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRsltRest.PrimaryKey = new DataColumn[] { dtRsltRest.Columns["PROD_LOTID"], dtRsltRest.Columns["EQSGID"], dtRsltRest.Columns["SUBLOTID"], dtRsltRest.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltRest.Columns)
                        {
                            // 컬럼이름 RESTHEATING1_LOTID, RESTHEATING2_LOTID...
                            if (column.ColumnName.Substring(0, 4).Equals("REST") == false) continue; // 컬럼이름 앞 4자리가 RESTHEATING 가 아니면 스킵

                            // 앞 11자리가 RESTHEATING 이면...
                            string compareName = "REST" + iRest.ToString() + "_"; // 비교하기 위해 앞 6자리 구성
                            if (column.ColumnName.Substring(0, 6).Equals(compareName) == false)  // 앞 6자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltRest.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltRest);
                    }
                }
            }
        }

        private void GetAfterList()
        {
            try
            {

                if (!string.IsNullOrEmpty(txtLotAfter.Text) && Util.GetCondition(txtLotAfter).Length < 6)
                {
                    Util.MessageInfo("SFU4074", "6");  // LOTID %1자리 이상 입력시 조회 가능합니다.
                    return;

                }
                ShowLoadingIndicator();

                Util.gridClear(dgAfter);

                bool chk = false;
                String option = String.Empty;
                String cellid = String.Empty;
                DataTable dtRslt = new DataTable();

                dtRsltEolRework = null;
                dtRsltAfter = null;
                dtRslt0 = null;
                dtRslt1 = null;
                dtRslt2 = null;
                dtRslt2_1 = null;
                dtRslt3 = null;
                dtRslt3_1 = null;
                dtRslt4 = null;
                dtRslt5 = null;
                dtRslt6 = null;
                dtRslt7 = null;
                dtRslt8 = null;
                dtRslt9 = null;
                dtRsltShip = null;
                dtRsltJudgAfter = null;

                DataTable dtCellList = dgAfterSerchList.GetDataTable();
                dtCellList.AsEnumerable().Where(w => Util.NVC(w.Field<string>("SUBLOTID")).Equals(string.Empty)).ToList().ForEach(row => row.Delete());
                dtCellList.AcceptChanges();

                for (int i = 0; i < tvAfter.Items.Count; i++)
                {
                    MenuItem mn = tvAfter.Items[i] as MenuItem;
                    for (int j = 0; j < mn.Items.Count; j++)
                    {
                        chk = mn.Items[j].IsChecked;
                        option = mn.Items[j].Tag;

                        if (dtCellList != null && dtCellList.Rows.Count > 0)
                        {
                            GetAfterCellData(chk, option, ref dtRslt);
                        }
                        else if (!string.IsNullOrEmpty(txtLotAfter.Text.Trim()))
                        {
                            GetAfterLotData(chk, option, ref dtRslt);
                        }
                    }
                }

                // 입력된 Cell과 순서 동기화
                if (dtCellList != null && dtCellList.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("SORT_ORDER", typeof(int));
                    foreach (DataRow dr in dtRslt.Rows)
                    {
                        List<DataRow> findRows = dtCellList.AsEnumerable().Where(w => w.Field<string>("SUBLOTID") != null && w.Field<string>("SUBLOTID").Equals(Util.NVC(dr["SUBLOTID"]))).ToList();
                        if (findRows != null && findRows.Count > 0)
                        {
                            dr["SORT_ORDER"] = dtCellList.Rows.IndexOf(findRows[0]);
                        }
                    }
                    DataView dvRslt = dtRslt.DefaultView;
                    dvRslt.Sort = "SORT_ORDER";
                    dtRslt = dvRslt.ToTable();
                }

                Util.GridSetData(dgAfter, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void GetAfterLotData(bool chk, String option, ref DataTable dtRsltTemp)
        {
            if (Util.GetCondition(txtLotAfter).Length < 6)
            {
                Util.MessageInfo("FM_ME_0049");  //Lot ID는 최소 7자리이상 입력해주세요.
                HiddenLoadingIndicator();
                return;
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("PROC_DIV", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("SEQ", typeof(string));

            DataRow drAfter = dtRqst.NewRow();
            drAfter["LANGID"] = LoginInfo.LANGID;
            drAfter["PROD_LOTID"] = Util.GetCondition(txtLotAfter);
            drAfter["PROCID"] = DBNull.Value;
            drAfter["PROC_DIV"] = DBNull.Value;
            drAfter["AREAID"] = LoginInfo.CFG_AREA_ID;
            drAfter["SUBLOTID"] = DBNull.Value;
            drAfter["S26"] = DBNull.Value;
            drAfter["SEQ"] = DBNull.Value;
            dtRqst.Rows.Add(drAfter);


            if (rdoEOLRework.IsChecked == true)
            {
                if (dtRsltEolRework != null)
                    return;

                //xmlbuffer 에러로 인해 여러번 호출하도록
                //for (int i = 0; i < dtRqst.Rows.Count; i++)
                //{
                //    dtRsltEolRework = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_EOL_REWORK", "RQSTDT", "RSLTDT", dtRqst);
                //    dtRsltTemp.Merge(dtRsltEolRework);
                //}
            }
            else if (rdoOCV1After.IsChecked == true)
            {
                if (chk && option.Contains("DA_C"))
                {
                    if (dtRslt0 != null)
                        return;

                    //Cell LIst
                    dtRslt0 = new ClientProxy().ExecuteServiceSync("DA_SEL_AFTER_LOT_CELL", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt0.PrimaryKey = new DataColumn[] { dtRslt0.Columns["PROD_LOTID"], dtRslt0.Columns["EQSGID"], dtRslt0.Columns["SUBLOTID"], dtRslt0.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경
                    dtRsltTemp.Merge(dtRslt0);
                }

                if (chk && option.Contains("DA_13"))
                {
                    if (dtRslt1 != null)
                        return;

                    //EQP_ID, OCV1, OCV1TIME
                    dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_OCV", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt1.PrimaryKey = new DataColumn[] { dtRslt1.Columns["PROD_LOTID"], dtRslt1.Columns["EQSGID"], dtRslt1.Columns["SUBLOTID"], dtRslt1.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경
                    dtRsltTemp.Merge(dtRslt1);
                }

                if (chk && option.Contains("DA_11"))
                {
                    if (dtRslt6 != null && dtRslt7 != null)
                        return;

                    //***_END_VOLT_VAL, ***_CAPA_VAL, FITTED_*** _CAPA_VAL, CHG2_END_TEMP, DCHG1_AVG_TEMP
                    dtRqst.Rows[0]["PROCID"] = "FF1101";
                    dtRqst.Rows[0]["PROC_DIV"] = "1";
                    dtRslt6 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt6.PrimaryKey = new DataColumn[] { dtRslt6.Columns["PROD_LOTID"], dtRslt6.Columns["EQSGID"], dtRslt6.Columns["SUBLOTID"], dtRslt6.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "CHG1";
                    foreach (DataColumn column in dtRslt6.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                            || column.ColumnName.ToString().ToUpper().Equals("FITTED_CHG1_CAPA_VAL"))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRslt6.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt6.Columns.Remove(dtRslt6.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt6);

                    dtRqst.Rows[0]["PROCID"] = "FF1102";
                    dtRqst.Rows[0]["PROC_DIV"] = "1";
                    dtRslt7 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt7.PrimaryKey = new DataColumn[] { dtRslt7.Columns["PROD_LOTID"], dtRslt7.Columns["EQSGID"], dtRslt7.Columns["SUBLOTID"], dtRslt7.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList2 = new List<string>();
                    string compStr2 = "CHG2";
                    foreach (DataColumn column in dtRslt7.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr2.Length).Equals(compStr2)
                            || column.ColumnName.ToString().ToUpper().Equals("FITTED_CHG2_CAPA_VAL"))
                        {
                            displayList2.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp2 = dtRslt7.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp2.Columns)
                    {
                        if (!displayList2.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt7.Columns.Remove(dtRslt7.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt7);

                    dtRqst.Rows[0]["PROCID"] = "FF1103";
                    dtRqst.Rows[0]["PROC_DIV"] = "1";
                    dtRslt7 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt7.PrimaryKey = new DataColumn[] { dtRslt7.Columns["PROD_LOTID"], dtRslt7.Columns["EQSGID"], dtRslt7.Columns["SUBLOTID"], dtRslt7.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList3 = new List<string>();
                    string compStr3 = "CHG3";
                    foreach (DataColumn column in dtRslt7.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr3.Length).Equals(compStr3)
                            || column.ColumnName.ToString().ToUpper().Equals("FITTED_CHG3_CAPA_VAL"))
                        {
                            displayList3.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp3 = dtRslt7.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp3.Columns)
                    {
                        if (!displayList3.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt7.Columns.Remove(dtRslt7.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt7);
                }

                if (chk && option.Contains("DA_12"))
                {
                    if (dtRslt8 != null && dtRslt9 != null)
                        return;

                    dtRqst.Rows[0]["PROCID"] = "FF1201";
                    dtRqst.Rows[0]["PROC_DIV"] = "2";
                    dtRslt8 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt8.PrimaryKey = new DataColumn[] { dtRslt8.Columns["PROD_LOTID"], dtRslt8.Columns["EQSGID"], dtRslt8.Columns["SUBLOTID"], dtRslt8.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "DCHG1";
                    foreach (DataColumn column in dtRslt8.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                            || column.ColumnName.ToString().ToUpper().Equals("FITTED_DCHG1_CAPA_VAL"))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRslt8.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt8.Columns.Remove(dtRslt8.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt8);

                    dtRqst.Rows[0]["PROCID"] = "FF1202";
                    dtRqst.Rows[0]["PROC_DIV"] = "2";
                    dtRslt9 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt9.PrimaryKey = new DataColumn[] { dtRslt9.Columns["PROD_LOTID"], dtRslt9.Columns["EQSGID"], dtRslt9.Columns["SUBLOTID"], dtRslt9.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList2 = new List<string>();
                    string compStr2 = "DCHG2";
                    foreach (DataColumn column in dtRslt9.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr2.Length).Equals(compStr2)
                            || column.ColumnName.ToString().ToUpper().Equals("FITTED_DCHG2_CAPA_VAL"))
                        {
                            displayList2.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp2 = dtRslt9.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp2.Columns)
                    {
                        if (!displayList2.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt9.Columns.Remove(dtRslt9.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt9);

                    dtRqst.Rows[0]["PROCID"] = "FF1203";
                    dtRqst.Rows[0]["PROC_DIV"] = "2";
                    dtRslt9 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt9.PrimaryKey = new DataColumn[] { dtRslt9.Columns["PROD_LOTID"], dtRslt9.Columns["EQSGID"], dtRslt9.Columns["SUBLOTID"], dtRslt9.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList3 = new List<string>();
                    string compStr3 = "DCHG3";
                    foreach (DataColumn column in dtRslt9.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Substring(0, compStr3.Length).Equals(compStr3)
                            || column.ColumnName.ToString().ToUpper().Equals("FITTED_DCHG3_CAPA_VAL"))
                        {
                            displayList3.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp3 = dtRslt9.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp3.Columns)
                    {
                        if (!displayList3.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt9.Columns.Remove(dtRslt9.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt9);
                }

                if (chk && option.Contains("DA_17"))
                {
                    if (dtRslt2 != null)
                        return;

                    //IMP, FITTEDIMP
                    dtRqst.Rows[0]["PROCID"] = "FF1701";
                    dtRqst.Rows[0]["PROC_DIV"] = "7";
                    dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_IMP", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt2.PrimaryKey = new DataColumn[] { dtRslt2.Columns["PROD_LOTID"], dtRslt2.Columns["EQSGID"], dtRslt2.Columns["SUBLOTID"], dtRslt2.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "POWERDCHG1";
                    foreach (DataColumn column in dtRslt2.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Contains(compStr))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRslt2.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt2.Columns.Remove(dtRslt2.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt2);
                }

                if (chk && option.Contains("DA_19"))
                {
                    if (dtRslt2_1 != null)
                        return;

                    dtRqst.Rows[0]["PROCID"] = "FF1901";
                    dtRqst.Rows[0]["PROC_DIV"] = "9";
                    dtRslt2_1 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_IMP", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt2_1.PrimaryKey = new DataColumn[] { dtRslt2_1.Columns["PROD_LOTID"], dtRslt2_1.Columns["EQSGID"], dtRslt2_1.Columns["SUBLOTID"], dtRslt2_1.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //표시 대상 칼럼 저장
                    List<string> displayList = new List<string>();
                    string compStr = "POWERCHG1";
                    foreach (DataColumn column in dtRslt2_1.Columns)
                    {
                        if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                            || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                            || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                            || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                            //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                            || column.ColumnName.ToString().ToUpper().Contains(compStr))
                        {
                            displayList.Add(column.ColumnName.ToString().ToUpper());
                        }
                    }

                    //표시 대상 칼럼 이외는 제거하기 위해 복사
                    DataTable dtTemp = dtRslt2_1.Clone();

                    //표시 대상 칼럼 이외는 제거
                    foreach (DataColumn column in dtTemp.Columns)
                    {
                        if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                        {
                            dtRslt2_1.Columns.Remove(dtRslt2_1.Columns[column.ColumnName]);
                        }
                    }

                    dtRsltTemp.Merge(dtRslt2_1);
                }

                int iPDOCVStart = Convert.ToInt16(nudDOCVS.Value);
                int iPDOCVEnd = Convert.ToInt16(nudDOCVE.Value);

                if (chk && option.Contains("DA_A1"))
                {
                    if (dtRslt3 != null)
                        return;

                    for (int iAfter = iPDOCVStart; iAfter <= iPDOCVEnd; iAfter++)
                    {
                        //DOCV
                        dtRqst.Rows[0]["PROCID"] = "FFA1" + iAfter.ToString("00");
                        dtRslt3 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_A11", "RQSTDT", "RSLTDT", dtRqst);
                        dtRslt3.PrimaryKey = new DataColumn[] { dtRslt3.Columns["PROD_LOTID"], dtRslt3.Columns["EQSGID"], dtRslt3.Columns["LOTID"], dtRslt3.Columns["SUBLOTID"], dtRslt3.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRslt3.Columns)
                        {
                            // 컬럼이름 DOCV1_LOTID, DOCV2_LOTID...
                            if (column.ColumnName.Substring(0, 4).Equals("DOCV") == false) continue; // 컬럼이름 앞 4자리가 DOCV 가 아니면 스킵
                            // 앞 4자리가 DOCV 이면...
                            string compareName = "DOCV" + iAfter.ToString(); // 비교하기 위해 앞 5자리 구성
                            if (column.ColumnName.Substring(0, 5).Equals(compareName) == false)  // 앞 5자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRslt3.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRslt3);
                    }
                }

                // 2024.06.12 YHK : Slope OCV 항목 추가
                int iPSOCVStart = Convert.ToInt16(nudSOCVS.Value);
                int iPSOCVEnd = Convert.ToInt16(nudSOCVE.Value);
                if (chk && option.Contains("DA_A3"))
                {
                    if (dtRslt3_1 != null)
                        return;

                    for (int iAfter = iPSOCVStart; iAfter <= iPSOCVEnd; iAfter++)
                    {
                        //SOCV
                        dtRqst.Rows[0]["PROCID"] = "FFA3" + iAfter.ToString("00");
                        dtRslt3_1 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_A31", "RQSTDT", "RSLTDT", dtRqst);
                        dtRslt3_1.PrimaryKey = new DataColumn[] { dtRslt3_1.Columns["PROD_LOTID"], dtRslt3_1.Columns["EQSGID"], dtRslt3_1.Columns["SUBLOTID"], dtRslt3_1.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRslt3_1.Columns)
                        {
                            // 컬럼이름 SOCV1_LOTID, SOCV2_LOTID...
                            if (column.ColumnName.Substring(0, 4).Equals("SOCV") == false) continue; // 컬럼이름 앞 4자리가 SOCV 가 아니면 스킵
                                                                                                     // 앞 4자리가 SOCV 이면...
                            string compareName = "SOCV" + iAfter.ToString(); // 비교하기 위해 앞자리 구성
                            if (column.ColumnName.Contains(compareName) == false)  // 앞자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRslt3_1.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRslt3_1);
                    }
                }

                if (chk && option.Contains("DA_51"))
                {
                    if (dtRslt4 != null)
                        return;

                    //VOLT_VAL, MDF_TIME, WEIGHT_VAL, THIC_VAL, ACIR_VAL, DCIR_VAL, MVDAY, IV_CA_VAL
                    dtRslt4 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_EOL", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt4.PrimaryKey = new DataColumn[] { dtRslt4.Columns["PROD_LOTID"], dtRslt4.Columns["EQSGID"], dtRslt4.Columns["SUBLOTID"], dtRslt4.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                    //E20240816 - 001103 요청 데이터 조회 컬럼 Sorting 비정상 처리 개선을 위한 타입변경 (text-> double)
                    DataTable dtTemp = dtRslt4.Clone();
                    dtTemp.Columns["IR_VAL"].DataType = typeof(double);           //절연저항
                    dtTemp.Columns["IV_CA_VAL"].DataType = typeof(double);        //절연전압
                    dtTemp.Columns["EOL_COMPUTE_VAL"].DataType = typeof(double);  //EOL 후 잔존량
                    dtTemp.Columns["EOL_FILLING_LOSS"].DataType = typeof(double); //EOL 토출량
                    if (dtRslt4.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtRslt4.Rows)
                        {
                            DataRow drTemp = dtTemp.NewRow();
                            drTemp.ItemArray = dr.ItemArray;

                            dtTemp.Rows.Add(drTemp);
                        }
                    }
                    dtRsltTemp.Merge(dtTemp);
                    //dtRsltTemp.Merge(dtRslt4);

                    //dtRslt4 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_EOL_REWORK", "RQSTDT", "RSLTDT", dtRqst);
                    //dtRslt4.PrimaryKey = new DataColumn[] { dtRslt4.Columns["PROD_LOTID"], dtRslt4.Columns["EQSGID"], dtRslt4.Columns["SUBLOTID"], dtRslt4.Columns["ROUTID"] };
                    //dtRsltTemp.Merge(dtRslt4);
                }

                int iPOCVStart = Convert.ToInt16(nudPOCVS.Value);
                int iPOCVEnd = Convert.ToInt16(nudPOCVE.Value);

                if (chk && option.Contains("DA_81"))
                {
                    if (dtRsltAfter != null)
                        return;

                    for (int iAfter = iPOCVStart; iAfter <= iPOCVEnd; iAfter++)
                    {
                        dtRqst.Rows[0]["PROCID"] = "FF81" + iAfter.ToString("00");
                        dtRsltAfter = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_DOCV", "RQSTDT", "RSLTDT", dtRqst);
                        dtRsltAfter.PrimaryKey = new DataColumn[] { dtRsltAfter.Columns["PROD_LOTID"], dtRsltAfter.Columns["EQSGID"], dtRsltAfter.Columns["SUBLOTID"], dtRsltAfter.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltAfter.Columns)
                        {
                            // 컬럼이름 DEDICATE_OCV1_LOTID, DEDICATE_OCV2_LOTID...
                            // if (column.ColumnName.Substring(0, 12).Equals("DEDICATE_OCV") == false) continue; // 컬럼이름 앞 12자리가 DEDICATE_OCV 가 아니면 스킵
                            if (column.ColumnName.StartsWith("DEDICATE_OCV") == false) continue; // 컬럼이름이 DEDICATE_OCV 시작하지 않으면 스킵
                            // 앞 12자리가 DEDICATE_OCV 이면...
                            string compareName = "DEDICATE_OCV" + iAfter.ToString(); // 비교하기 위해 앞 13자리 구성
                            if (column.ColumnName.Substring(0, 13).Equals(compareName) == false)  // 앞 13자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltAfter.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltAfter);
                    }
                }

                //2023.04.17 suyong.kim 보정dOCV 정보 조회
                if (chk && option.Contains("DA_D1"))
                {
                    if (dtRslt5 != null) return;

                    dtRslt5 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_FITTED_DOCV", "RQSTDT", "RSLTDT", dtRqst);
                    dtRslt5.PrimaryKey = new DataColumn[] { dtRslt5.Columns["PROD_LOTID"], dtRslt5.Columns["EQSGID"], dtRslt5.Columns["SUBLOTID"], dtRslt5.Columns["ROUTID"] };
                    dtRsltTemp.Merge(dtRslt5);
                }

                ///2023.10.19 Ship AGING 추가
                if (chk && option.Contains("DA_71"))
                {
                    if (dtRsltShip != null)
                        return;

                    for (int iShip = 1; iShip <= 4; iShip++)
                    {
                        //Ship Aging
                        //DataTable dtRqst1 = dtRqst.Clone();
                        // dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst.Rows[0]["SEQ"] = iShip.ToString();
                        dtRsltShip = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_SHIP", "RQSTDT", "RSLTDT", dtRqst);
                        dtRsltShip.PrimaryKey = new DataColumn[] { dtRsltShip.Columns["PROD_LOTID"], dtRsltShip.Columns["EQSGID"], dtRsltShip.Columns["SUBLOTID"], dtRsltShip.Columns["ROUTID"] };

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "SHIP" + iShip.ToString();
                        foreach (DataColumn column in dtRsltShip.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Contains(compStr))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRsltShip.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRsltShip.Columns.Remove(dtRsltShip.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRsltShip);
                    }
                }

                if (chk && option.Contains("DA_B1")) // 2024.02.20 판정 추가
                {
                    if (dtRsltJudgAfter != null)
                        return;

                    //JIG JUDGE
                    for (int iJudg = 1; iJudg <= 6; iJudg++)
                    {
                        dtRqst.Rows[0]["SEQ"] = iJudg.ToString();
                        dtRsltJudgAfter = new ClientProxy().ExecuteServiceSync("DA_SEL_AFTER_LOT_JUDG", "RQSTDT", "RSLTDT", dtRqst);
                        dtRsltJudgAfter.PrimaryKey = new DataColumn[] { dtRsltJudgAfter.Columns["PROD_LOTID"], dtRsltJudgAfter.Columns["EQSGID"], dtRsltJudgAfter.Columns["SUBLOTID"], dtRsltJudgAfter.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltJudgAfter.Columns)
                        {
                            // 컬럼이름 JUDG1_LOTID, JUDG2_LOTID...
                            if (column.ColumnName.Substring(0, 4).Equals("JUDG") == false) continue; // 컬럼이름 앞 4자리가 JUDG 가 아니면 스킵
                            // 앞 4자리가 JUDG 이면...
                            string compareName = "JUDG" + iJudg.ToString() + "_"; // 비교하기 위해 앞 6자리 구성
                            if (column.ColumnName.Substring(0, 6).Equals(compareName) == false)  // 앞 6자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltJudgAfter.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltJudgAfter);
                    }
                }

            }
        }

        private void GetAfterCellData(bool chk, String option, ref DataTable dtRsltTemp)
        {
            if (dgAfterSerchList.Rows.Count == 0)
            {
                Util.MessageInfo("FM_ME_0465");  //Cell ID와 PKG LOTID 중 하나는 반드시 입력해야 합니다.
                HiddenLoadingIndicator();
                return;
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("PROD_LOTID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("PROC_DIV", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("S26", typeof(string));
            dtRqst.Columns.Add("SEQ", typeof(string));

            String Cellid = "";

            for (int i = 0; i < dgAfterSerchList.Rows.Count; i++)
            {
                Cellid = Util.NVC(DataTableConverter.GetValue(dgAfterSerchList.Rows[i].DataItem, "SUBLOTID"));
                if (!string.IsNullOrEmpty(Cellid))
                {
                    if (i % 200 == 0)
                    {
                        DataRow drAfter = dtRqst.NewRow();
                        drAfter["LANGID"] = LoginInfo.LANGID;
                        drAfter["PROD_LOTID"] = DBNull.Value;
                        drAfter["PROCID"] = DBNull.Value;
                        drAfter["PROC_DIV"] = DBNull.Value;
                        drAfter["S26"] = DBNull.Value;
                        drAfter["SEQ"] = DBNull.Value;
                        drAfter["AREAID"] = LoginInfo.CFG_AREA_ID;
                        drAfter["SUBLOTID"] = Cellid + ",";
                        dtRqst.Rows.Add(drAfter);
                    }
                    else
                    {
                        dtRqst.Rows[dtRqst.Rows.Count - 1]["SUBLOTID"] = dtRqst.Rows[dtRqst.Rows.Count - 1]["SUBLOTID"].ToString() + Cellid + ",";
                    }
                }
            }


            if (rdoEOLRework.IsChecked == true)
            {
                if (dtRsltEolRework != null)
                    return;

                //xmlbuffer 에러로 인해 여러번 호출하도록
                //for (int i = 0; i < dtRqst.Rows.Count; i++)
                //{
                //    DataTable dtRqst1 = dtRqst.Clone();
                //    dtRqst1.ImportRow(dtRqst.Rows[i]);
                //    dtRsltEolRework = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_EOL_REWORK", "RQSTDT", "RSLTDT", dtRqst1);
                //    dtRsltTemp.Merge(dtRsltEolRework);
                //}
            }
            else if (rdoOCV1After.IsChecked == true)
            {
                if (chk && option.Contains("DA_C"))
                {
                    if (dtRslt0 != null)
                        return;

                    //xmlbuffer 에러로 인해 여러번 호출하도록
                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        //Cell LIst
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRslt0 = new ClientProxy().ExecuteServiceSync("DA_SEL_AFTER_LOT_CELL", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt0.PrimaryKey = new DataColumn[] { dtRslt0.Columns["PROD_LOTID"], dtRslt0.Columns["EQSGID"], dtRslt0.Columns["SUBLOTID"], dtRslt0.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경
                        dtRsltTemp.Merge(dtRslt0);
                    }
                }

                if (chk && option.Contains("DA_13"))
                {
                    if (dtRslt1 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        //EQP_ID, OCV1, OCV1TIME
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_OCV", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt1.PrimaryKey = new DataColumn[] { dtRslt1.Columns["PROD_LOTID"], dtRslt1.Columns["EQSGID"], dtRslt1.Columns["SUBLOTID"], dtRslt1.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경
                        dtRsltTemp.Merge(dtRslt1);
                    }
                }

                if (chk && option.Contains("DA_11"))
                {
                    if (dtRslt6 != null && dtRslt7 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        //***_END_VOLT_VAL, ***_CAPA_VAL, FITTED_*** _CAPA_VAL, CHG2_END_TEMP, DCHG1_AVG_TEMP
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1101";
                        dtRqst1.Rows[0]["PROC_DIV"] = "1";
                        dtRslt6 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt6.PrimaryKey = new DataColumn[] { dtRslt6.Columns["PROD_LOTID"], dtRslt6.Columns["EQSGID"], dtRslt6.Columns["SUBLOTID"], dtRslt6.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "CHG1";
                        foreach (DataColumn column in dtRslt6.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                                || column.ColumnName.ToString().ToUpper().Equals("FITTED_CHG1_CAPA_VAL"))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt6.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt6.Columns.Remove(dtRslt6.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt6);
                    }

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1102";
                        dtRqst1.Rows[0]["PROC_DIV"] = "1";
                        dtRslt7 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt7.PrimaryKey = new DataColumn[] { dtRslt7.Columns["PROD_LOTID"], dtRslt7.Columns["EQSGID"], dtRslt7.Columns["SUBLOTID"], dtRslt7.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "CHG2";
                        foreach (DataColumn column in dtRslt7.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                                || column.ColumnName.ToString().ToUpper().Equals("FITTED_CHG2_CAPA_VAL"))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt7.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt7.Columns.Remove(dtRslt7.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt7);
                    }

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1103";
                        dtRqst1.Rows[0]["PROC_DIV"] = "1";
                        dtRslt7 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt7.PrimaryKey = new DataColumn[] { dtRslt7.Columns["PROD_LOTID"], dtRslt7.Columns["EQSGID"], dtRslt7.Columns["SUBLOTID"], dtRslt7.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "CHG3";
                        foreach (DataColumn column in dtRslt7.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                                || column.ColumnName.ToString().ToUpper().Equals("FITTED_CHG3_CAPA_VAL"))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt7.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt7.Columns.Remove(dtRslt7.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt7);
                    }
                }

                if (chk && option.Contains("DA_12"))
                {
                    if (dtRslt8 != null && dtRslt9 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1201";
                        dtRqst1.Rows[0]["PROC_DIV"] = "2";
                        dtRslt8 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt8.PrimaryKey = new DataColumn[] { dtRslt8.Columns["PROD_LOTID"], dtRslt8.Columns["EQSGID"], dtRslt8.Columns["SUBLOTID"], dtRslt8.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "DCHG1";
                        foreach (DataColumn column in dtRslt8.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                                || column.ColumnName.ToString().ToUpper().Equals("FITTED_DCHG1_CAPA_VAL"))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt8.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt8.Columns.Remove(dtRslt8.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt8);
                    }

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1202";
                        dtRqst1.Rows[0]["PROC_DIV"] = "2";
                        dtRslt9 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt9.PrimaryKey = new DataColumn[] { dtRslt9.Columns["PROD_LOTID"], dtRslt9.Columns["EQSGID"], dtRslt9.Columns["SUBLOTID"], dtRslt9.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "DCHG2";
                        foreach (DataColumn column in dtRslt9.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                                || column.ColumnName.ToString().ToUpper().Equals("FITTED_DCHG2_CAPA_VAL"))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt9.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt9.Columns.Remove(dtRslt9.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt9);
                    }

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1203";
                        dtRqst1.Rows[0]["PROC_DIV"] = "2";
                        dtRslt9 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_CHG", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt9.PrimaryKey = new DataColumn[] { dtRslt9.Columns["PROD_LOTID"], dtRslt9.Columns["EQSGID"], dtRslt9.Columns["SUBLOTID"], dtRslt9.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "DCHG3";
                        foreach (DataColumn column in dtRslt9.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Substring(0, compStr.Length).Equals(compStr)
                                || column.ColumnName.ToString().ToUpper().Equals("FITTED_DCHG3_CAPA_VAL"))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt9.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt9.Columns.Remove(dtRslt9.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt9);
                    }
                }

                if (chk && option.Contains("DA_17"))
                {
                    if (dtRslt2 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        //IMP, FITTEDIMP
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1701";
                        dtRqst1.Rows[0]["PROC_DIV"] = "7";
                        dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_IMP", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt2.PrimaryKey = new DataColumn[] { dtRslt2.Columns["PROD_LOTID"], dtRslt2.Columns["EQSGID"], dtRslt2.Columns["SUBLOTID"], dtRslt2.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "POWERDCHG1";
                        foreach (DataColumn column in dtRslt2.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Contains(compStr))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt2.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt2.Columns.Remove(dtRslt2.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt2);
                    }
                }

                if (chk && option.Contains("DA_19"))
                {
                    if (dtRslt2_1 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRqst1.Rows[0]["PROCID"] = "FF1901";
                        dtRqst1.Rows[0]["PROC_DIV"] = "9";
                        dtRslt2_1 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_IMP", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt2_1.PrimaryKey = new DataColumn[] { dtRslt2_1.Columns["PROD_LOTID"], dtRslt2_1.Columns["EQSGID"], dtRslt2_1.Columns["SUBLOTID"], dtRslt2_1.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //표시 대상 칼럼 저장
                        List<string> displayList = new List<string>();
                        string compStr = "POWERCHG1";
                        foreach (DataColumn column in dtRslt2_1.Columns)
                        {
                            if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                || column.ColumnName.ToString().ToUpper().Contains(compStr))
                            {
                                displayList.Add(column.ColumnName.ToString().ToUpper());
                            }
                        }

                        //표시 대상 칼럼 이외는 제거하기 위해 복사
                        DataTable dtTemp = dtRslt2_1.Clone();

                        //표시 대상 칼럼 이외는 제거
                        foreach (DataColumn column in dtTemp.Columns)
                        {
                            if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                            {
                                dtRslt2_1.Columns.Remove(dtRslt2_1.Columns[column.ColumnName]);
                            }
                        }

                        dtRsltTemp.Merge(dtRslt2_1);
                    }
                }

                int iDOCVStart = Convert.ToInt16(nudDOCVS.Value);
                int iDOCVEnd = Convert.ToInt16(nudDOCVE.Value);

                if (chk && option.Contains("DA_A1"))
                {
                    if (dtRslt3 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        for (int iAfter = iDOCVStart; iAfter <= iDOCVEnd; iAfter++)
                        {
                            //DOCV
                            DataTable dtRqst1 = dtRqst.Clone();
                            dtRqst1.ImportRow(dtRqst.Rows[i]);
                            dtRqst1.Rows[0]["PROCID"] = "FFA1" + iAfter.ToString("00");
                            dtRslt3 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_A11", "RQSTDT", "RSLTDT", dtRqst1);
                            dtRslt3.PrimaryKey = new DataColumn[] { dtRslt3.Columns["PROD_LOTID"], dtRslt3.Columns["EQSGID"], dtRslt3.Columns["LOTID"], dtRslt3.Columns["SUBLOTID"], dtRslt3.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                            List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                            foreach (DataColumn column in dtRslt3.Columns)
                            {
                                // 컬럼이름 DOCV1_LOTID, DOCV2_LOTID...
                                if (column.ColumnName.Substring(0, 4).Equals("DOCV") == false) continue; // 컬럼이름 앞 4자리가 DOCV 가 아니면 스킵
                                // 앞 4자리가 DOCV 이면...
                                string compareName = "DOCV" + iAfter.ToString(); // 비교하기 위해 앞 5자리 구성
                                if (column.ColumnName.Substring(0, 5).Equals(compareName) == false)  // 앞 5자리가 비교이름과 다르면 삭제하기 위해 담음.
                                {
                                    deleteColumns.Add(column);
                                }
                            }
                            // 담아놓은 컬럼 삭제
                            deleteColumns.ForEach(delColumn => dtRslt3.Columns.Remove(delColumn));

                            dtRsltTemp.Merge(dtRslt3);
                        }
                    }
                }

                // 2024.06.12 YHK : Slope OCV 항목 추가
                int iSOCVStart = Convert.ToInt16(nudSOCVS.Value);
                int iSOCVEnd = Convert.ToInt16(nudSOCVE.Value);

                // 2024.06.12 YHK : Slope OCV 항목 추가
                if (chk && option.Contains("DA_A3"))
                {
                    if (dtRslt3_1 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        for (int iAfter = iSOCVStart; iAfter <= iSOCVEnd; iAfter++)
                        {
                            //SOCV
                            DataTable dtRqst1 = dtRqst.Clone();
                            dtRqst1.ImportRow(dtRqst.Rows[i]);
                            dtRqst1.Rows[0]["PROCID"] = "FFA3" + iAfter.ToString("00");
                            dtRslt3_1 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_A31", "RQSTDT", "RSLTDT", dtRqst1);
                            dtRslt3_1.PrimaryKey = new DataColumn[] { dtRslt3_1.Columns["PROD_LOTID"], dtRslt3_1.Columns["EQSGID"], dtRslt3_1.Columns["SUBLOTID"], dtRslt3_1.Columns["ROUTID"] };

                            List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                            foreach (DataColumn column in dtRslt3_1.Columns)
                            {
                                // 컬럼이름 SOCV1_LOTID, SOCV2_LOTID...
                                if (column.ColumnName.Substring(0, 4).Equals("SOCV") == false) continue; // 컬럼이름 앞 4자리가 SOCV 가 아니면 스킵
                                                                                                         // 앞 4자리가 SOCV 이면...
                                string compareName = "SOCV" + iAfter.ToString(); // 비교하기 위해 앞자리 구성
                                if (column.ColumnName.Contains(compareName) == false)  // 앞자리가 비교이름과 다르면 삭제하기 위해 담음.
                                {
                                    deleteColumns.Add(column);
                                }
                            }
                            // 담아놓은 컬럼 삭제
                            deleteColumns.ForEach(delColumn => dtRslt3_1.Columns.Remove(delColumn));

                            dtRsltTemp.Merge(dtRslt3_1);
                        }
                    }
                }

                if (chk && option.Contains("DA_51"))
                {
                    if (dtRslt4 != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        //VOLT_VAL, MDF_TIME, WEIGHT_VAL, THIC_VAL, ACIR_VAL, DCIR_VAL, MVDAY, IV_CA_VAL
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRslt4 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_EOL", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt4.PrimaryKey = new DataColumn[] { dtRslt4.Columns["PROD_LOTID"], dtRslt4.Columns["EQSGID"], dtRslt4.Columns["SUBLOTID"], dtRslt4.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경

                        //E20240816 - 001103 요청 데이터 조회 컬럼 Sorting 비정상 처리 개선을 위한 타입변경 (text-> double)
                        DataTable dtTemp = dtRslt4.Clone();
                        dtTemp.Columns["IR_VAL"].DataType = typeof(double);           //절연저항
                        dtTemp.Columns["IV_CA_VAL"].DataType = typeof(double);        //절연전압
                        dtTemp.Columns["EOL_COMPUTE_VAL"].DataType = typeof(double);  //EOL 후 잔존량
                        dtTemp.Columns["EOL_FILLING_LOSS"].DataType = typeof(double); //EOL 토출량
                        if (dtRslt4.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtRslt4.Rows)
                            {
                                DataRow drTemp = dtTemp.NewRow();
                                drTemp.ItemArray = dr.ItemArray;

                                dtTemp.Rows.Add(drTemp);
                            }
                        }
                        dtRsltTemp.Merge(dtTemp);
                        //dtRsltTemp.Merge(dtRslt4);

                        //dtRslt4 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_EOL_REWORK", "RQSTDT", "RSLTDT", dtRqst1);
                        //dtRslt4.PrimaryKey = new DataColumn[] { dtRslt4.Columns["PROD_LOTID"], dtRslt4.Columns["EQSGID"], dtRslt4.Columns["SUBLOTID"], dtRslt4.Columns["ROUTID"] };
                        //dtRsltTemp.Merge(dtRslt4);
                    }
                }

                int iPOCVStart = Convert.ToInt16(nudPOCVS.Value);
                int iPOCVEnd = Convert.ToInt16(nudPOCVE.Value);

                if (chk && option.Contains("DA_81"))
                {
                    if (dtRsltAfter != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        for (int iAfter = iPOCVStart; iAfter <= iPOCVEnd; iAfter++)
                        {
                            DataTable dtRqst1 = dtRqst.Clone();
                            dtRqst1.ImportRow(dtRqst.Rows[i]);
                            dtRqst1.Rows[0]["PROCID"] = "FF81" + iAfter.ToString("00");
                            dtRsltAfter = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_DOCV", "RQSTDT", "RSLTDT", dtRqst1);
                            dtRsltAfter.PrimaryKey = new DataColumn[] { dtRsltAfter.Columns["PROD_LOTID"], dtRsltAfter.Columns["EQSGID"], dtRsltAfter.Columns["SUBLOTID"], dtRsltAfter.Columns["ROUTID"] }; //2021.05.06 CSTID -> LOTID로 변경}; //2021.05.06 CSTID -> LOTID로 변경

                            List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                            foreach (DataColumn column in dtRsltAfter.Columns)
                            {
                                // 컬럼이름 DEDICATE_OCV1_LOTID, DEDICATE_OCV2_LOTID...
                                // if (column.ColumnName.Substring(0, 12).Equals("DEDICATE_OCV") == false) continue; // 컬럼이름 앞 12자리가 DEDICATE_OCV 가 아니면 스킵
                                if (column.ColumnName.StartsWith("DEDICATE_OCV") == false) continue; // 컬럼이름이 DEDICATE_OCV 시작하지 않으면 스킵
                                // 앞 12자리가 DEDICATE_OCV 이면...
                                string compareName = "DEDICATE_OCV" + iAfter.ToString(); // 비교하기 위해 앞 13자리 구성
                                if (column.ColumnName.Substring(0, 13).Equals(compareName) == false)  // 앞 13자리가 비교이름과 다르면 삭제하기 위해 담음.
                                {
                                    deleteColumns.Add(column);
                                }
                            }
                            // 담아놓은 컬럼 삭제
                            deleteColumns.ForEach(delColumn => dtRsltAfter.Columns.Remove(delColumn));

                            dtRsltTemp.Merge(dtRsltAfter);
                        }
                    }
                }

                //2023.04.17 suyong.kim 보정dOCV 정보 조회
                if (chk && option.Contains("DA_D1"))
                {
                    if (dtRslt5 != null) return;

                    //xmlbuffer 에러로 인해 여러번 호출하도록
                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        //Cell LIst
                        DataTable dtRqst1 = dtRqst.Clone();
                        dtRqst1.ImportRow(dtRqst.Rows[i]);
                        dtRslt5 = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_FITTED_DOCV", "RQSTDT", "RSLTDT", dtRqst1);
                        dtRslt5.PrimaryKey = new DataColumn[] { dtRslt5.Columns["PROD_LOTID"], dtRslt5.Columns["EQSGID"], dtRslt5.Columns["SUBLOTID"], dtRslt5.Columns["ROUTID"] };
                        dtRsltTemp.Merge(dtRslt5);
                    }
                }

                if (chk && option.Contains("DA_71"))
                {
                    if (dtRsltShip != null)
                        return;

                    for (int i = 0; i < dtRqst.Rows.Count; i++)
                    {
                        for (int iShip = 1; iShip <= 4; iShip++)
                        {
                            //Ship Aging
                            DataTable dtRqst1 = dtRqst.Clone();
                            dtRqst1.ImportRow(dtRqst.Rows[i]);
                            dtRqst1.Rows[0]["SEQ"] = iShip.ToString();
                            dtRsltShip = new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_LOT_SHIP", "RQSTDT", "RSLTDT", dtRqst1);
                            dtRsltShip.PrimaryKey = new DataColumn[] { dtRsltShip.Columns["PROD_LOTID"], dtRsltShip.Columns["EQSGID"], dtRsltShip.Columns["SUBLOTID"], dtRsltShip.Columns["ROUTID"] };

                            //표시 대상 칼럼 저장
                            List<string> displayList = new List<string>();
                            string compStr = "SHIP" + iShip.ToString();
                            foreach (DataColumn column in dtRsltShip.Columns)
                            {
                                if (column.ColumnName.ToString().ToUpper().Equals("PROD_LOTID")
                                    || column.ColumnName.ToString().ToUpper().Equals("SUBLOTID")
                                    || column.ColumnName.ToString().ToUpper().Equals("EQSGID")
                                    || column.ColumnName.ToString().ToUpper().Equals("LOTID")
                                    || column.ColumnName.ToString().ToUpper().Equals("ROUTID")
                                    || column.ColumnName.ToString().ToUpper().Equals("GRADE")
                                    //|| column.ColumnName.ToString().ToUpper().Equals("CSTID")
                                    || column.ColumnName.ToString().ToUpper().Contains(compStr))
                                {
                                    displayList.Add(column.ColumnName.ToString().ToUpper());
                                }
                            }

                            //표시 대상 칼럼 이외는 제거하기 위해 복사
                            DataTable dtTemp = dtRsltShip.Clone();

                            //표시 대상 칼럼 이외는 제거
                            foreach (DataColumn column in dtTemp.Columns)
                            {
                                if (!displayList.Contains(column.ColumnName.ToString().ToUpper()))
                                {
                                    dtRsltShip.Columns.Remove(dtRsltShip.Columns[column.ColumnName]);
                                }
                            }

                            dtRsltTemp.Merge(dtRsltShip);
                        }
                    }
                }

                if (chk && option.Contains("DA_B1")) // 2024.02.20 판정 추가
                {
                    if (dtRsltJudgAfter != null)
                        return;

                    //JIG JUDGE
                    for (int iJudg = 1; iJudg <= 6; iJudg++)
                    {
                        dtRqst.Rows[0]["SEQ"] = iJudg.ToString();
                        dtRsltJudgAfter = new ClientProxy().ExecuteServiceSync("DA_SEL_AFTER_LOT_JUDG", "RQSTDT", "RSLTDT", dtRqst);
                        dtRsltJudgAfter.PrimaryKey = new DataColumn[] { dtRsltJudgAfter.Columns["PROD_LOTID"], dtRsltJudgAfter.Columns["EQSGID"], dtRsltJudgAfter.Columns["SUBLOTID"], dtRsltJudgAfter.Columns["ROUTID"] };

                        List<DataColumn> deleteColumns = new List<DataColumn>(); // 삭제컬럼 담기위해 (컬랙션 루프 내에서 삭제시 오류나기 때문)
                        foreach (DataColumn column in dtRsltJudgAfter.Columns)
                        {
                            // 컬럼이름 JUDG1_LOTID, JUDG2_LOTID...
                            if (column.ColumnName.Substring(0, 4).Equals("JUDG") == false) continue; // 컬럼이름 앞 4자리가 JUDG 가 아니면 스킵
                            // 앞 4자리가 JUDG 이면...
                            string compareName = "JUDG" + iJudg.ToString() + "_"; // 비교하기 위해 앞 6자리 구성
                            if (column.ColumnName.Substring(0, 6).Equals(compareName) == false)  // 앞 6자리가 비교이름과 다르면 삭제하기 위해 담음.
                            {
                                deleteColumns.Add(column);
                            }
                        }
                        // 담아놓은 컬럼 삭제
                        deleteColumns.ForEach(delColumn => dtRsltJudgAfter.Columns.Remove(delColumn));

                        dtRsltTemp.Merge(dtRsltJudgAfter);
                    }
                }
            }
        }

        private void GetCellList()
        {
            try
            {
                ShowLoadingIndicator();

                string sTemp;
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataTable dtRslt = new DataTable();

                for (int i = 0; i < dgCellData.Rows.Count; i++)
                {
                    sTemp = Util.NVC(DataTableConverter.GetValue(dgCellData.Rows[i].DataItem, "SUBLOTID"));
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        if (i % 10 == 0)
                        {
                            DataRow drCell = dtRqst.NewRow();
                            drCell["SUBLOTID"] = sTemp + ",";
                            drCell["LANGID"] = LoginInfo.LANGID;
                            dtRqst.Rows.Add(drCell);
                        }
                        else
                        {
                            dtRqst.Rows[dtRqst.Rows.Count - 1]["SUBLOTID"] = dtRqst.Rows[dtRqst.Rows.Count - 1]["SUBLOTID"].ToString() + sTemp + ",";
                            dtRqst.Rows[dtRqst.Rows.Count - 1]["LANGID"] = LoginInfo.LANGID;
                        }
                    }
                }

                //xmlbuffer 에러로 인해 여러번 호출하도록
                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);

                    dtRslt.Merge(new ClientProxy().ExecuteServiceSync("DA_SEL_NEW_CELL", "RQSTDT", "RSLTDT", dtRqst1));
                }

                Util.GridSetData(dgCellData, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetBWIPList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("INSDTTM", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow drRqst = dtRqst.NewRow();
                drRqst["INSDTTM"] = dtpTargetBWIP.SelectedDateTime.ToString("yyyyMMdd");
                drRqst["PROD_LOTID"] = Util.GetCondition(txtLotBWIP, bAllNull: true);
                drRqst["LANGID"] = LoginInfo.LANGID;
                drRqst["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!string.IsNullOrEmpty(drRqst["PROD_LOTID"].ToString()) && drRqst["PROD_LOTID"].ToString().Length < 6)
                {
                    Util.MessageInfo("FM_ME_0048");  //Lot ID는 최소 7자리이상 입력해주세요.
                    return;
                }
                dtRqst.Rows.Add(drRqst);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_MONTH_WIPDATA", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgBWIP, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTCList()
        {
            try
            {
                ShowLoadingIndicator();

                string sTemp, cTemp;  // 20221207 cTemp 추가
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string)); //20221207

                DataTable dtRslt = new DataTable();

                for (int i = 0; i < dgTrayCell.Rows.Count; i++)
                {
                    sTemp = Util.NVC(DataTableConverter.GetValue(dgTrayCell.Rows[i].DataItem, "LOTID"));
                    cTemp = Util.NVC(DataTableConverter.GetValue(dgTrayCell.Rows[i].DataItem, "CSTID")); //20221207
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        if (i % 100 == 0)
                        {
                            DataRow drTray = dtRqst.NewRow();
                            drTray["LOTID"] = sTemp + ",";
                            dtRqst.Rows.Add(drTray);
                        }
                        else
                        {
                            dtRqst.Rows[dtRqst.Rows.Count - 1]["LOTID"] = dtRqst.Rows[dtRqst.Rows.Count - 1]["LOTID"].ToString() + sTemp + ",";
                        }
                    }
                    if (!string.IsNullOrEmpty(cTemp)) //20221207
                    {
                        if (i % 100 == 0)
                        {
                            DataRow drTray = dtRqst.NewRow();
                            drTray["CSTID"] = cTemp + ",";  //20221207
                            dtRqst.Rows.Add(drTray);
                        }
                        else
                        {
                            dtRqst.Rows[dtRqst.Rows.Count - 1]["CSTID"] = dtRqst.Rows[dtRqst.Rows.Count - 1]["CSTID"].ToString() + cTemp + ",";//20221207
                        }
                    }
                }

                //xmlbuffer 에러로 인해 여러번 호출하도록
                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRslt.Merge(new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_ID_BY_TRAY", "RQSTDT", "RSLTDT", dtRqst1));
                }

                Util.GridSetData(dgTrayCell, dtRslt, FrameOperation, true);

                Util _Util = new Util();
                string[] sColumnName = new string[] { "LOTID" };
                _Util.SetDataGridMergeExtensionCol(dgTrayCell, sColumnName, DataGridMergeMode.VERTICAL);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        //20231012
        private void GetPCList()
        {
            try
            {
                ShowLoadingIndicator();

                string sTemp;
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PALLETID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string)); //20231012

                DataTable dtRslt = new DataTable();

                for (int i = 0; i < dgPalletCell.Rows.Count; i++)
                {
                    sTemp = Util.NVC(DataTableConverter.GetValue(dgPalletCell.Rows[i].DataItem, "PALLETID"));
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        if (i % 100 == 0)
                        {
                            DataRow drTray = dtRqst.NewRow();
                            drTray["PALLETID"] = sTemp + ",";
                            drTray["LANGID"] = LoginInfo.LANGID; //20231012
                            dtRqst.Rows.Add(drTray);
                        }
                        else
                        {
                            dtRqst.Rows[dtRqst.Rows.Count - 1]["PALLETID"] = dtRqst.Rows[dtRqst.Rows.Count - 1]["PALLETID"].ToString() + sTemp + ",";
                        }
                    }
                }

                //xmlbuffer 에러로 인해 여러번 호출하도록
                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRslt.Merge(new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_ID_BY_PALLET", "RQSTDT", "RSLTDT", dtRqst1));
                }

                Util.GridSetData(dgPalletCell, dtRslt, FrameOperation, true);

                Util _Util = new Util();
                string[] sColumnName = new string[] { "PALLETID" };
                _Util.SetDataGridMergeExtensionCol(dgPalletCell, sColumnName, DataGridMergeMode.VERTICAL);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void RemoveBlankRow(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColName)
        {
            // Cell ID 가 없는 Row 삭제
            for (int i = dgTrayCell.Rows.Count - 1; i > 0; i--)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayCell.Rows[i].DataItem, sColName))))
                {
                    dgTrayCell.RemoveRow(i);
                }
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowCount)
        {
            try
            {
                DataTable dt = new DataTable();

                if (dg.ItemsSource != null)
                {
                    dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.EndEdit();
                    }
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                    {
                        dt.Columns.Add(Convert.ToString(col.Name));
                    }

                    for (int i = 0; i < iRowCount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.EndEdit();
                    }
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetColumns(C1.WPF.DataGrid.C1DataGrid dataGrid, bool AllFlag)
        {
            if (AllFlag == true)
            {
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    dataGrid.Columns[dataGrid.Columns[i].Name].Visibility = Visibility.Collapsed;

                    if (dataGrid.Columns[i].Name.Equals("PROD_LOTID") || dataGrid.Columns[i].Name.Equals("EQSGID")
                        || dataGrid.Columns[i].Name.Equals("SUBLOTID") || dataGrid.Columns[i].Name.Equals("PRODID"))
                    {
                        dataGrid.Columns[dataGrid.Columns[i].Name].Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    dataGrid.Columns[dataGrid.Columns[i].Name].Visibility = Visibility.Collapsed;

                    if (dataGrid.Columns[i].Name.Equals("PROD_LOTID") || dataGrid.Columns[i].Name.Equals("EQSGID")
                        || dataGrid.Columns[i].Name.Equals("SUBLOTID") || dataGrid.Columns[i].Name.Equals("PRODID") || dataGrid.Columns[i].Name.Equals("EOL_REWORK_CNT"))
                    {
                        dataGrid.Columns[dataGrid.Columns[i].Name].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        //노드 시작
        private void GetReqDegasTree(C1TreeView tv, string sDegasAB)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DEGAS_B_A", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DEGAS_B_A"] = sDegasAB;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_REQ_DATA_ITEM_TREE", "RQSTDT", "RSLTDT", dtRqst);

                tv.Items.Clear();

                dtTemp = dtRslt.Copy();

                foreach (DataRow dr1 in dtRslt.Rows)
                {
                    MenuItem root = new MenuItem();
                    if (dr1["PKEY_VAL"].ToString().Equals("TOP"))
                    {
                        root = new MenuItem() { Title = dr1["NAME_VAL"].ToString(), Tag = dr1["PKEY_VAL"].ToString() + "/" + dr1["KEY_VAL"].ToString() };
                        if (dr1["KEY_VAL"].ToString().Contains("DB_C") || dr1["KEY_VAL"].ToString().Contains("DA_C"))
                            root.IsChecked = true;

                        foreach (DataRow dr2 in dtRslt.Rows)
                        {
                            if (dr1["KEY_VAL"].ToString().Equals(dr2["PKEY_VAL"].ToString()))
                            {
                                MenuItem child = new MenuItem() { Title = dr2["NAME_VAL"].ToString(), Tag = dr2["PKEY_VAL"].ToString() + "/" + dr2["KEY_VAL"].ToString() };
                                if ((dr2["PKEY_VAL"].ToString().Contains("DB_C") || dr2["PKEY_VAL"].ToString().Contains("DA_C")) && !dr2["KEY_VAL"].Equals("DA_EOL_REWORK"))
                                    child.IsChecked = true;
                                root.Items.Add(child);
                            }
                        }
                        tv.Items.Add(root);
                    }
                }
                TreeItemExpandAll(tv);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void TreeItemExpandAll(C1TreeView tv)
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tv, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }

        private void trvData_ItemExpanded(object sender, SourcedEventArgs e)
        {
            //C1TreeView tv = sender as C1TreeView;
            //rb_Checked(tv);
        }

        private void rb_Checked(C1TreeView tv)
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(tv, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNode(item);
            }
        }

        public void TreeItemExpandNode(C1TreeViewItem item)
        {

            item.IsExpanded = true;
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
            foreach (C1TreeViewItem childItem in items)
            {
                TreeItemExpandNode(childItem);
            }
        }

        private void GetInitNode(String TabItem)
        {
            switch (TabItem)
            {
                case "tpBeforeDegas":
                    if (!chkbeforetab)
                    {
                        _dtHeader = new DataTable();
                        GetReqDegasTree(tvBefore, "B");
                        InitSpread(dgBefore);
                        chkbeforetab = true;
                    }
                    break;
                case "tpAfterDegas":
                    if (!chkaftertab)
                    {
                        _dtHeader = new DataTable();
                        GetReqDegasTree(tvAfter, "A");
                        InitSpread(dgAfter);
                        chkaftertab = true;
                    }
                    break;
                case "tpLotTrayCell":
                    if (!chkaftertab)
                    {
                        _dtHeader = new DataTable();
                        InitSpread_LotTrayCell(this.dgLotTrayCell);
                        chkaftertab = true;
                    }
                    break;
            }
        }

        private void FixedMultiHeader(C1.WPF.DataGrid.C1DataGrid dg, string sName, string sBindName, bool bVisible, int iWidth = 75)
        {
            bool bReadOnly = true;
            bool bEditable = false;

            string[] sColName = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColName.ToList();

            var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible);
            dg.Columns.Add(column_TEXT);
        }
        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header
                                                                        , List<string> Multi_Header
                                                                        , string sName
                                                                        , string sBinding
                                                                        , int iWidth
                                                                        , bool bReadOnly = false
                                                                        , bool bEditable = true
                                                                        , bool bVisible = true
                                                                        , bool bPercent = false
                                                                         , HorizontalAlignment HorizonAlign = HorizontalAlignment.Center
                                                                        , VerticalAlignment VerticalAlign = VerticalAlignment.Center
                                                       )
        {
            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = HorizonAlign;
            Col.VerticalAlignment = VerticalAlign;

            if (iWidth == 0)
                Col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            else
                Col.Width = new C1.WPF.DataGrid.DataGridLength(iWidth, DataGridUnitType.Pixel);

            if (bPercent)
                Col.Format = "P2";

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;
            else
                Col.Header = Multi_Header;

            _dtHeader.Columns.Add(sBinding, typeof(string));

            return Col;
        }

        #endregion

        private void btnLTCReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgLotTrayCellSearchList);
            if (Convert.ToInt32(txtLTCRowCntCell.Value) > 100000)
            {
                Util.AlertInfo("FM_ME_0242");  //최대 100,000 까지 입력할 수 있습니다.
                return;
            }
            DataGridRowAdd(dgLotTrayCellSearchList, Convert.ToInt32(txtLTCRowCntCell.Value));

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotTrayCell);
        }

        private void btnLTCSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboSearchItems.SelectedValue).IsEmpty())
            {
                Util.Alert("FM_ME_0165");  //선택된 데이터가 없습니다.
                return;
            }
            if (dgLotTrayCellSearchList.Rows.Count <= 0)
            {
                Util.Alert("FM_ME_0165");  //선택된 데이터가 없습니다.
                return;
            }
            _dtHeader = new DataTable();
            InitSpread_LotTrayCell(this.dgLotTrayCell);

            GetLTCList();
        }

        private void GetLTCList()
        {
            try
            {
                ShowLoadingIndicator();

                string sBiz = string.Empty;
                string sKeyName = string.Empty;
                sBiz = Util.GetCondition(cboSearchItems, bAllNull: true);

                switch (sBiz)
                {
                    case "DA_SEL_CELLID_BY_TRAYID":
                        sKeyName = "TRAYID";
                        break;
                    case "DA_SEL_TRAYID_BY_CELLID":
                        sKeyName = "CELLID";
                        break;
                    case "DA_TST_SEL_PALLETID_BY_CELLID_2":
                        sKeyName = "CELLID";
                        break;
                    case "DA_TST_SEL_CELLID_BY_PALLETID_2":
                        sKeyName = "PALLETID";
                        break;
                    case "DA_SEL_OT820_TAB6_1TH_V01":
                        sKeyName = "CELLID";
                        break;
                    case "DA_SEL_CELL_INFO_BY_CELL_ID":
                        sKeyName = "CELLID";
                        break;
                    case "DA_TST_SEL_CELLID_BY_PKG_LOTID":
                        sKeyName = "PRODID";
                        break;
                    case "DA_SEL_DGS_INFO_BY_CELL_ID":
                        sKeyName = "CELLID";
                        break;
                    case "DA_SEL_HPCD_INFO_BY_LOT":
                        sKeyName = "ASSY_LOTID"; //ASSY_LOT_ID
                        break;
                    //일괄 데이터 조회 조건 추가  - DA_SEL_TRAY_NO_CELL_ID, DA_SEL_IROCV_INFO_BY_CELL_ID
                    case "DA_SEL_TRAY_NO_CELL_ID":
                        sKeyName = "TRAY_NO";
                        break;
                    case "DA_SEL_IROCV_INFO_BY_CELL_ID":
                        sKeyName = "CELLID";
                        break;
                }

                string sTemp;
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add(sKeyName, typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataTable dtRslt = new DataTable();

                for (int i = 0; i < dgLotTrayCellSearchList.Rows.Count; i++)
                {
                    sTemp = Util.NVC(DataTableConverter.GetValue(dgLotTrayCellSearchList.Rows[i].DataItem, "CELLID"));
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        /**************************************************************************************************************
                        //대용량 조회시 문제점
                        //대용량 조회의 경우 키값 100개씩 보내는 방법 체크
                        //일부 쿼리는 단일 키값일 경우에도 실행 시간이 오래 걸리기 때문에 단일 키값 전송으로 사용
                        // DA_TST_SEL_CELLID_BY_PKG_LOTID : like 검색 사용중
                        // DA_SEL_HPCD_INFO_BY_LOT : 조회되는 건수가 많아서 제한 (1 key -> 1500건)
                        **************************************************************************************************************/
                        if (sBiz.Equals("DA_TST_SEL_CELLID_BY_PKG_LOTID") || sBiz.Equals("DA_SEL_HPCD_INFO_BY_LOT")) //like  검색 사용중
                        {
                            DataRow drTray = dtRqst.NewRow();
                            drTray[sKeyName] = sTemp;
                            drTray["LANGID"] = LoginInfo.LANGID;
                            dtRqst.Rows.Add(drTray);
                        }
                        else
                        {
                            if (i % 100 == 0)
                            {
                                DataRow drTray = dtRqst.NewRow();
                                drTray[sKeyName] = sTemp + ",";
                                drTray["LANGID"] = LoginInfo.LANGID;
                                dtRqst.Rows.Add(drTray);
                            }
                            else
                            {
                                dtRqst.Rows[dtRqst.Rows.Count - 1][sKeyName] = dtRqst.Rows[dtRqst.Rows.Count - 1][sKeyName].ToString() + sTemp + ",";
                            }
                        }

                    }
                }

                //xmlbuffer 에러로 인해 여러번 호출하도록
                for (int i = 0; i < dtRqst.Rows.Count; i++)
                {
                    DataTable dtRqst1 = dtRqst.Clone();
                    dtRqst1.ImportRow(dtRqst.Rows[i]);
                    dtRslt.Merge(new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst1));
                }

                Util.GridSetData(dgLotTrayCell, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void dgLotTrayCellSearchList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "CELLID")
                    {
                        e.Cell.Presenter.IsEnabled = true;
                    }
                    else
                    {
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void dgLotTrayCellSearchList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //20210408 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 START
                if (e.Column.HeaderPresenter == null)
                {
                    return;
                }
                //20210408 Cell Merge 후 HeaderPresenter 에 대한 Null 처리 추가 END

                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CELLID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgLotTrayCellSearchList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgTrayCell.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void InitSpread_LotTrayCell(C1.WPF.DataGrid.C1DataGrid dg)
        {
            Util.gridClear(dg); //Grid clear

            int Header_Row_count = 1;

            if (dg.TopRows.Count <= 0)
            {
                //칼럼 헤더 행 추가
                for (int i = 0; i < Header_Row_count; i++)
                {
                    DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                    dg.TopRows.Add(HR);
                }
            }

            #region LotTrayCell
            if (dg.Name.Equals("dgLotTrayCell"))
            {

                //모든 컬럼 감추기
                for (int i = 0; i < dg.Columns.Count; i++)
                {
                    dg.Columns[dg.Columns[i].Name].Visibility = Visibility.Collapsed;
                }

                string sBiz = string.Empty;
                sBiz = Util.GetCondition(cboSearchItems, bAllNull: true);

                switch (sBiz)
                {
                    case "DA_SEL_CELLID_BY_TRAYID": // "SEL_CELLID_BY_TRAYID":                                                    
                        FixedHeader(dg, "CSTID", "CSTID", true);
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "CSTSLOT", "CSTSLOT", true);
                        FixedHeader(dg, "LOTID", "LOTID", true);
                        FixedHeader(dg, "FINL_JUDG_CODE", "FINL_JUDG_CODE", true);
                        FixedHeader(dg, "SPLIT_FLAG", "SPLIT_FLAG", true);
                        FixedHeader(dg, "CURR_TRAY_ID", "CURR_TRAY_ID", true);
                        break;
                    case "DA_SEL_TRAYID_BY_CELLID": //"SEL_TRAYID_BY_CELLID":
                        FixedHeader(dg, "CSTID", "CSTID", true);
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "CSTSLOT", "CSTSLOT", true);
                        FixedHeader(dg, "LOTID", "LOTID", true);
                        FixedHeader(dg, "PROD_LOTID", "PROD_LOTID", true);
                        FixedHeader(dg, "FINL_JUDG_CODE", "FINL_JUDG_CODE", true);
                        FixedHeader(dg, "SPLIT_FLAG", "SPLIT_FLAG", true);
                        break;
                    case "DA_TST_SEL_PALLETID_BY_CELLID_2":
                        FixedHeader(dg, "CELLID", "CELLID", true);
                        FixedHeader(dg, "CELL위치", "CELL_POS", true);
                        FixedHeader(dg, "BOXID", "BOXID", true);
                        FixedHeader(dg, "TRAY_PSTN", "TRAY_PSTN", true);
                        FixedHeader(dg, "PALLETID", "PALLETID", true);
                        FixedHeader(dg, "포장상태", "BOX_STATNAME", true);
                        FixedHeader(dg, "출하상태", "SHIP_STATNAME", true);
                        FixedHeader(dg, "PRODID", "PRODID", true);
                        FixedHeader(dg, "PACK_EQPTNAME", "PACK_EQPTID", true);
                        FixedHeader(dg, "MODELID", "MODELID", true);
                        FixedHeader(dg, "QTY", "QTY", true);
                        FixedHeader(dg, "SHIP_PLAN_DATE", "SHIPDATE_SCHEDULE", true);
                        FixedHeader(dg, "SHIPTO_NAME", "SHIPTO_NAME", true);
                        FixedHeader(dg, "RCV_ISS_ID", "RELSID", true);
                        FixedHeader(dg, "ISS_DTTM", "RELSDATE", true);
                        FixedHeader(dg, "PACK_TYPE", "LOT_TYPENAME", true);
                        FixedHeader(dg, "EQSGNAME", "EQSGNAME", true);
                        FixedHeader(dg, "REG_USERNAME", "REG_USERNAME", true);
                        FixedHeader(dg, "PRJT_NAME", "PROJECTNAME", true);
                        break;
                    case "DA_TST_SEL_CELLID_BY_PALLETID_2":
                        FixedHeader(dg, "PALLETID", "PALLETID", true);
                        FixedHeader(dg, "TRAYID", "TRAYID", true);
                        FixedHeader(dg, "TRAY_PSTN", "TRAY_PSTN", true);
                        FixedHeader(dg, "CELLID", "CELLID", true);
                        FixedHeader(dg, "BOX_LOC", "BOX_LOC", true);
                        FixedHeader(dg, "ASSY_LOTID", "ASSY_LOTID", true);
                        break;
                    case "DA_SEL_OT820_TAB6_1TH_V01": //"SEL_OT820_TAB6_1TH_V01"
                        FixedHeader(dg, "EQSGID", "EQSGID", true);
                        FixedHeader(dg, "ROUTID", "ROUTID", true);
                        FixedHeader(dg, "LOTID", "LOTID", true);
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "FINL_JUDG_CODE", "FINL_JUDG_CODE", true);
                        FixedHeader(dg, "1_CAPA_VAL", "CAPA_1_VALUE", true);
                        FixedHeader(dg, "FIT_1_CAPA_VAL", "FIT_1_CAPA_VALUE", true);
                        FixedHeader(dg, "3_CAPA_VAL", "CAPA_3_VALUE", true);
                        FixedHeader(dg, "FIT_3_CAPA_VAL", "FIT_3_CAPA_VALUE", true);
                        FixedHeader(dg, "DOCV", "DOCV", true);
                        FixedHeader(dg, "VLTG_VALUE", "VLTG_VALUE", true);
                        FixedHeader(dg, "EOL_TIME", "EOL_TIME", true);
                        FixedHeader(dg, "DCIR_VALUE", "DCIR_VALUE", true);
                        FixedHeader(dg, "AVG_THK", "AVG_TCK_VALUE", true);
                        FixedHeader(dg, "ACIR_VAL", "ACIR_VALUE", true);
                        FixedHeader(dg, "IR", "IR", true);
                        //FixedHeader(dg, "IV", "IV", true);
                        //FixedHeader(dg, "IV_VAL", "IV", true);
                        FixedHeader(dg, "IV_CA_VAL", "IV_CA_VAL", true);
                        FixedHeader(dg, "MVDAY", "MVDAY", true);
                        FixedHeader(dg, "VERIF_GRD_VALUE", "VERIF_GRD_VALUE", true);
                        FixedHeader(dg, "IMP", "IMP", true);
                        FixedHeader(dg, "FIT_IMP", "FITTEDIMP", true);
                        FixedHeader(dg, "OCV_01", "OCV1", true);
                        FixedHeader(dg, "OCV_01_TIME", "OCV1TIME", true);
                        FixedHeader(dg, "OCV_02", "OCV2", true);
                        FixedHeader(dg, "OCV_02_TIME", "OCV2TIME", true);
                        FixedHeader(dg, "OCV_03", "OCV3", true);
                        FixedHeader(dg, "OCV_03_TIME", "OCV3TIME", true);
                        FixedHeader(dg, "OCV_04", "OCV4", true);
                        FixedHeader(dg, "OCV_04_TIME", "OCV4TIME", true);
                        FixedHeader(dg, "OCV_05", "OCV5", true);
                        FixedHeader(dg, "OCV_05_TIME", "OCV5TIME", true);
                        FixedHeader(dg, "OCV_06", "OCV6", true);
                        FixedHeader(dg, "OCV_06_TIME", "OCV6TIME", true);
                        FixedHeader(dg, "OCV_07", "OCV7", true);
                        FixedHeader(dg, "OCV_07_TIME", "OCV7TIME", true);
                        FixedHeader(dg, "OCV_08", "OCV8", true);
                        FixedHeader(dg, "OCV_08_TIME", "OCV8TIME", true);
                        FixedHeader(dg, "OCV_09", "OCV9", true);
                        FixedHeader(dg, "OCV_09_TIME", "OCV9TIME", true);
                        FixedHeader(dg, "OCV_10", "OCV10", true);
                        FixedHeader(dg, "OCV_10_TIME", "OCV10TIME", true);
                        FixedHeader(dg, "WEIGHT", "WEIGHT", true);
                        break;
                    case "DA_SEL_CELL_INFO_BY_CELL_ID": //"SEL_CELL_INFO_BY_CELL_ID"
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "TRAY_LOT_ID", "LOTID", true);
                        FixedHeader(dg, "CHANNEL", "CSTSLOT", true);
                        FixedHeader(dg, "CSTID", "CSTID", true);
                        FixedHeader(dg, "ROUTID", "ROUTID", true);
                        FixedHeader(dg, "OP_NAME", "PROCNAME", true);
                        FixedHeader(dg, "START_TIME", "WRK_STRT_DTTM", true);
                        FixedHeader(dg, "AGING_INPUT_TIME", "AGING_INPUT_TIME", true);
                        FixedHeader(dg, "END_TIME", "WRK_END_DTTM", true);
                        FixedHeader(dg, "CELL_END_DTTM", "CELL_END_DTTM", true);
                        FixedHeader(dg, "MINUTE", "OP_TIME", true);
                        FixedHeader(dg, "GRADE", "SUBLOTJUDGE", true);
                        FixedHeader(dg, "CAPA_VAL", "CAPA_VALUE", true);
                        FixedHeader(dg, "FITCAPA_VAL", "FITCAPA_VAL", true);
                        FixedHeader(dg, "FITCAPA_VAL_SAS", "FITCAPA_VAL_SAS", true);
                        FixedHeader(dg, "FITIMP_VAL_MU", "FITTED_DCIR_VALUE", true);
                        FixedHeader(dg, "AVG_VOLT_VAL", "AVG_VLTG_VALUE", true);
                        FixedHeader(dg, "END_VOLT_VAL", "END_VLTG_VALUE", true);
                        FixedHeader(dg, "END_CURR_VAL", "END_CURNT_VALUE", true);
                        FixedHeader(dg, "MOCV_VAL_MV", "OCV_VALUE", true);
                        FixedHeader(dg, "IMP_VAL_MU", "DCIR_VALUE", true);
                        FixedHeader(dg, "JIG_TEMP_VAL", "JIG_TMPR_VALUE", true);
                        FixedHeader(dg, "JIG_PRESS", "JIG_PRESS_VALUE", true);
                        FixedHeader(dg, "CURR_VAL_MA", "CURNT_VALUE", true);
                        FixedHeader(dg, "VOLT_VAL_MV", "VLTG_VALUE", true);
                        FixedHeader(dg, "PRESS", "HPCD_PRESS", true);
                        FixedHeader(dg, "WORK_EQP", "EQPTID", true);
                        break;
                    case "DA_TST_SEL_CELLID_BY_PKG_LOTID":
                        FixedHeader(dg, "LOTID", "LOTID", true);
                        FixedHeader(dg, "PRODID", "PRODID", true);
                        FixedHeader(dg, "TRAYID", "TRAYID", true);
                        FixedHeader(dg, "CELLID", "CELLID", true);
                        FixedHeader(dg, "PALLETID", "PALLETID", true);
                        break;
                    case "DA_SEL_DGS_INFO_BY_CELL_ID": //"SEL_DGS_INFO_BY_CELL_ID":
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "LANE_NAME", "LANE_NAME", true);
                        FixedHeader(dg, "CHAMBER_LOCATION_NO", "CHAMBER_LOCATION_NO", true);
                        FixedHeader(dg, "WEIGHT_VAL", "WEIGHT_VALUE", true);
                        FixedHeader(dg, "DCIR_VALUE", "DCIR_VALUE", true);
                        FixedHeader(dg, "DCIR_RESULT", "DCIR_RESULT", true);
                        FixedHeader(dg, "절연전압", "IVLTG_VALUE", true);
                        FixedHeader(dg, "DEGAS_JUDG_RSLT", "DEGAS_JUDG_RSLT_CODE", true);
                        FixedHeader(dg, "UPDDTTM", "UPDDTTM", true);
                        FixedHeader(dg, "PRE_SEAL_TOP_TMPR1_VALUE", "PRE_SEAL_TOP_TMPR1_VALUE", true);
                        FixedHeader(dg, "PRE_SEAL_BTM_TMPR1_VALUE", "PRE_SEAL_BTM_TMPR1_VALUE", true);
                        FixedHeader(dg, "PRE_SEAL_TIME", "PRE_SEAL_TIME", true);
                        FixedHeader(dg, "VAC_TIME", "VACM_TIME", true);
                        FixedHeader(dg, "VACM_REACH_TIME", "VACM_REACH_TIME", true);
                        FixedHeader(dg, "VACM_KEEP_TIME", "VACM_KEEP_TIME", true);
                        FixedHeader(dg, "PRE_SEAL_PRESS_VALUE", "PRE_SEAL_PRESS_VALUE", true);
                        FixedHeader(dg, "LOAD_CELL_VAL", "LOAD_CELL_VAL", true);
                        FixedHeader(dg, "HOT_PRESS_V_BEFORE", "HOT_PRESS_V_BEFORE", true);
                        FixedHeader(dg, "HOT_PRESS_V_MAX", "HOT_PRESS_V_MAX", true);
                        FixedHeader(dg, "HOT_PRESS_V_MIN", "HOT_PRESS_V_MIN", true);
                        FixedHeader(dg, "END_VOLT_VAL", "END_VOLT_VAL", true);
                        FixedHeader(dg, "HOT_PRESS_V_RESULT", "HOT_PRESS_V_RESULT", true);
                        FixedHeader(dg, "HOTPRES_PORT_NO", "HOTPRES_PORT_NO", true);
                        FixedHeader(dg, "PRESS_VOLT1_VAL", "PRESS_VOLT1_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT2_VAL", "PRESS_VOLT2_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT3_VAL", "PRESS_VOLT3_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT4_VAL", "PRESS_VOLT4_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT5_VAL", "PRESS_VOLT5_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT6_VAL", "PRESS_VOLT6_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT7_VAL", "PRESS_VOLT7_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT8_VAL", "PRESS_VOLT8_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT9_VAL", "PRESS_VOLT9_VAL", true);
                        FixedHeader(dg, "PRESS_VOLT10_VAL", "PRESS_VOLT10_VAL", true);
                        FixedHeader(dg, "PRESS_TEMP", "PRESS_TEMP", true);
                        FixedHeader(dg, "PRESS_VALUE", "PRESS_VAL", true);
                        FixedHeader(dg, "PRESS_TIME", "PRESS_TIME", true);
                        FixedHeader(dg, "DOCV_PRESS_VAL", "DOCV_PRESS_VAL", true);
                        FixedHeader(dg, "MAIN_SEAL_TOP_TMPR1_VALUE", "MAIN_SEAL_TOP_TMPR1_VALUE", true);
                        FixedHeader(dg, "MAIN_SEAL_BTM_TMPR1_VALUE", "MAIN_SEAL_BTM_TMPR1_VALUE", true);
                        FixedHeader(dg, "MAIN_SEAL_TIME", "MAIN_SEAL_TIME", true);
                        FixedHeader(dg, "MAIN_SEAL_PRESS_VALUE", "MAIN_SEAL_PRESS_VALUE", true);
                        FixedHeader(dg, "MAIN_SEAL_LOCATION_NO", "MAIN_SEAL_LOCATION_NO", true);
                        FixedHeader(dg, "VAC_DGR_VALUE", "VACM_DGR_VALUE", true);
                        break;
                    case "DA_SEL_HPCD_INFO_BY_LOT": //"SEL_HPCD_INFO_BY_LOT":
                        FixedHeader(dg, "ROUTID", "ROUTID", true);
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "CSTSLOT", "CSTSLOT", true);
                        FixedHeader(dg, "LOTID", "LOTID", true);
                        FixedHeader(dg, "CSTID", "CSTID", true);
                        FixedHeader(dg, "DELTA_TEMP_VAL", "DELTA_TMPR_VALUE", true);
                        FixedHeader(dg, "CCURNT_VALUE", "CURNT_VALUE", true);
                        FixedHeader(dg, "JIG_PRESS", "JIG_PRESS_VALUE", true);
                        FixedHeader(dg, "CURNT_MAX_VALUE", "CURNT_MAX_VALUE", true);
                        FixedHeader(dg, "CURNT_MIN_VALUE", "CURNT_MIN_VALUE", true);
                        FixedHeader(dg, "FINL_JUDG_CODE", "FINL_JUDG_CODE", true);
                        FixedHeader(dg, "START_TIME", "WRK_STRT_DTTM", true);
                        break;
                    //일괄 데이터 조회 조건 추가  - DA_SEL_TRAY_NO_CELL_ID, DA_SEL_IROCV_INFO_BY_CELL_ID
                    case "DA_SEL_TRAY_NO_CELL_ID":
                        FixedHeader(dg, "LOTID", "LOTID", true);
                        FixedHeader(dg, "CSTSLOT", "CSTSLOT", true);
                        FixedHeader(dg, "FINL_JUDG_CODE", "FINL_JUDG_CODE", true);
                        FixedHeader(dg, "SPLT_FLAG", "SPLT_FLAG", true);
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "TMPR1_VAL", "TMPR1_VAL", true);
                        FixedHeader(dg, "TMPR2_VAL", "TMPR2_VAL", true);
                        FixedHeader(dg, "AVG_TMPR_VAL", "AVG_TMPR_VAL", true);
                        FixedHeader(dg, "PRSPLATE_TMPR1_VAL", "PRSPLATE_TMPR1_VAL", true);
                        FixedHeader(dg, "PRSPLATE_TMPR2_VAL", "PRSPLATE_TMPR2_VAL", true);
                        FixedHeader(dg, "PRSPLATE_AVG_TMPR_VAL", "PRSPLATE_AVG_TMPR_VAL", true);
                        FixedHeader(dg, "DELTA_TEMP_VAL", "DELTA_TMPR_VALUE", true);
                        break;
                    case "DA_SEL_IROCV_INFO_BY_CELL_ID":
                        FixedHeader(dg, "SUBLOTID", "SUBLOTID", true);
                        FixedHeader(dg, "MODLID", "MODLID", true);
                        FixedHeader(dg, "PRODID", "PRODID", true);
                        FixedHeader(dg, "UPDDTTM", "UPDDTTM", true);
                        FixedHeader(dg, "EOL_JUDG_RSLT_CODE", "EOL_JUDG_RSLT_CODE", true);
                        FixedHeader(dg, "AVG_THK", "AVG_TCK_VALUE", true);
                        FixedHeader(dg, "TCK_MAX", "TCK_MAX_VALUE", true);
                        FixedHeader(dg, "TCK_MIN", "TCK_MIN_VALUE", true);
                        FixedHeader(dg, "WEIGHT_VAL", "WEIGHT_VALUE", true);
                        FixedHeader(dg, "VLTG_VALUE", "VLTG_VALUE", true);
                        FixedHeader(dg, "ACIR_VAL", "ACIR_VALUE", true);
                        FixedHeader(dg, "IR_VAL", "IR_VALUE", true);
                        FixedHeader(dg, "MVDAY", "MVDAY_VALUE", true);
                        FixedHeader(dg, "MVDAY_SPEC_VALUE", "MVDAY_SPEC_VALUE", true);
                        FixedHeader(dg, "FINL_JUDG_CODE", "FINL_JUDG_CODE", true);
                        FixedHeader(dg, "생산LOTID", "PROD_LOTID", true);
                        FixedHeader(dg, "MAGAZINE_ID", "MAGAZINE_ID", true);
                        FixedHeader(dg, "AN절연전압", "AN_IVLTG_VALUE", true);
                        FixedHeader(dg, "IV_CA_VAL", "CA_IVLTG_VALUE", true);
                        FixedHeader(dg, "DCIR_PASS_YN", "DCIR_PASS_YN", true);
                        FixedHeader(dg, "IV_PASS_YN", "IV_PASS_YN", true);
                        FixedHeader(dg, "IV_CA_PASS_YN", "IV_CA_PASS_YN", true);
                        FixedHeader(dg, "DSF_IV_PASS_YN", "DSF_IV_PASS_YN", true);
                        FixedHeader(dg, "DSF_IV_VAL", "DSF_IV_VAL", true);
                        FixedHeader(dg, "DSF_POSITION_INFO", "DSF_DISTANCE_PSTN_CODE", true);
                        FixedHeader(dg, "CONTACTLESS_THIC", "CONTACTLESS_THIC_VAL", true);
                        FixedHeader(dg, "CONTACTLESS_THIC_OFFSET", "CONTACTLESS_THIC_OFFSET_VAL", true);
                        FixedHeader(dg, "CONTACTLESS_THIC_JUDG", "CONTACTLESS_THIC_JUDG_VAL", true);
                        FixedHeader(dg, "JUDGED_POS_NAME", "JUDGED_POS_NAME", true);
                        FixedHeader(dg, "PSTN1_TCK", "PSTN1_TCK_VALUE", true);
                        FixedHeader(dg, "PSTN2_TCK", "PSTN2_TCK_VALUE", true);
                        FixedHeader(dg, "PSTN3_TCK", "PSTN3_TCK_VALUE", true);
                        FixedHeader(dg, "PSTN4_TCK", "PSTN4_TCK_VALUE", true);
                        FixedHeader(dg, "VLTG_MAX_VALUE", "VLTG_MAX_VALUE", true);
                        FixedHeader(dg, "DOCV값", "DOCV_VALUE", true);
                        FixedHeader(dg, "READ_BCR", "READ_DATA_VALUE", true);
                        FixedHeader(dg, "RESID_ULMT_VAL", "EL_RMN_UCL", true);
                        FixedHeader(dg, "RESID_LLMT_VAL", "EL_RMN_LCL", true);
                        FixedHeader(dg, "EOL_WEIGHT_CONST_1", "EOL_WEIGHT_CONST", true);
                        FixedHeader(dg, "EOL_WEIGHT_VALUE", "EOL_WEIGHT_VALUE", true);
                        FixedHeader(dg, "EOL_RMN_ELCTRLT_QTY", "EOL_ELCTRLT_RMN_QTY", true);
                        FixedHeader(dg, "EOL_FILLING_LOSS", "EOL_DSCHG_VALUE", true);
                        FixedHeader(dg, "EOL_JUDG_RSLT", "EOL_JUDG_RSLT", true);

                        break;
                }

                //dgLotTrayCell.TopRowHeaderMerge();
            }
            #endregion

        }


        private void FixedHeader(C1.WPF.DataGrid.C1DataGrid dg, string sName, string sBindName, bool bVisible, int iWidth = 100)
        {
            bool bReadOnly = true;
            bool bEditable = false;
            bool bExist = false;
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                //dg.Columns[dg.Columns[i].Name].Visibility = Visibility.Collapsed;

                //일괄 데이터 조회 - 해더 자동생성 변경
                //if (dg.Columns[i].Name.Equals(sName))
                if (dg.Columns[i].Name.Equals(sBindName))
                {
                    dg.Columns[dg.Columns[i].Name].Visibility = Visibility.Visible;
                    bExist = true;
                }
            }

            if (bExist == false)
            {
                var column_TEXT = CreateTextColumn(sName, null, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible);
                //var column_TEXT = CreateTextColumn(sName, null, sName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible);
                dg.Columns.Add(column_TEXT);
            }
        }

        private void btnExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = GetExcelData();

            Util.GridSetData(dgLotTrayCellSearchList, dt, FrameOperation, true);
        }

        private DataTable GetExcelData()
        {

            DataTable dt = new DataTable();

            Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    dt = LoadExcel(stream, (int)0);
                }
            }
            //return sColData;
            return dt;
        }

        private DataTable LoadExcel(Stream excelFileStream, int sheetNo)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("CELLID", typeof(string));

            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                }

                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null)
                    {
                        DataRow dr = dt.NewRow();
                        dr["CELLID"] = sheet.GetCell(rowInx, 0).Text;
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //return sColData;
            return dt;
        }

        private bool IsBATCH_DATA_INQUIRY()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORMLGS_BATCH_DATA_INQUIRY_YN";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
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
            }
            return false;
        }
    }
}
