/*************************************************************************************
 Created Date : 2018.02.28
      Creator : M.C. JANG
   Decription : 시생산 LOT 등록/취소 < 특이작업
--------------------------------------------------------------------------------------
 [Change History]
  2018.02.28  M.C. JANG : Initial Created.
  2019.05.01  정문교  : 폴란드3동 대응 Carrier ID(CSTID) 조회조건, 조회칼럼 추가
  2019.12.13  정문교  : 1.폐기 > 시생산 전환 탭 추가
                        2.이력 조회탭에 AREA_TYPE_CODE 칼럼 추가

  2024.03.19  김동일  : E20240215-000888 - "이력조회" Tab -> "완료취소" 명칭 변경 (기존 기능 유지)
                                         - "이력조회" Tab 신규 추가
  2025.02.05  이민형  : 고해선 책임님 요청으로 탭 위치 변경
  2025.07.04  이민형  : 처리수량 입력 시 M,KG 단위일때는 소수점 입력 가능하도록 수정
**************************************************************************************/

using C1.WPF;
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_218 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        bool formatFlag = false;
        public COM001_218()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

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
        CheckBox chkAllScrapConv = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            #region [Combo]시생산 LOT 대상 등록  

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChildRegister = { cboEqsgRegister };
            _combo.SetCombo(cboAreaRegister, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildRegister, sCase: "AREA");

            //라인
            C1ComboBox[] cboLineParentRegister = { cboAreaRegister };
            C1ComboBox[] cboLineChildRegister = { cboProcRegister };
            _combo.SetCombo(cboEqsgRegister, CommonCombo.ComboStatus.ALL, cbChild: cboLineChildRegister, cbParent: cboLineParentRegister, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboProcessParentRegister = { cboAreaRegister, cboEqsgRegister };
            _combo.SetCombo(cboProcRegister, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentRegister, sCase: "PROCESSWITHAREA");

            if (cboEqsgRegister.Items.Count > 0) cboEqsgRegister.SelectedIndex = 0;
            if (cboProcRegister.Items.Count > 0) cboProcRegister.SelectedIndex = 0;

            string[] sFilter1 = { "PILOT_PROD_LOT" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);

            //비용창고
            string[] sFilter2 = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboCostSloc, CommonCombo.ComboStatus.SELECT, sCase: "PILOT_COST_SLOC", sFilter: sFilter2);



            #endregion

            #region [Combo]폐기 > 시생산 LOT 대상 등록  

            String[] sFilterScrapConv = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChildScrapConv = { cboEqsgScrapConv };
            _combo.SetCombo(cboAreaScrapConv, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildScrapConv, sCase: "AREA");

            //라인
            C1ComboBox[] cboLineParentScrapConv = { cboAreaScrapConv };
            C1ComboBox[] cboLineChildScrapConv = { cboProcScrapConv };
            _combo.SetCombo(cboEqsgScrapConv, CommonCombo.ComboStatus.ALL, cbChild: cboLineChildScrapConv, cbParent: cboLineParentScrapConv, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboProcessParentScrapConv = { cboAreaScrapConv, cboEqsgScrapConv };
            _combo.SetCombo(cboProcScrapConv, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentScrapConv, sCase: "PROCESSWITHAREA");

            if (cboEqsgScrapConv.Items.Count > 0) cboEqsgScrapConv.SelectedIndex = 0;
            if (cboProcScrapConv.Items.Count > 0) cboProcScrapConv.SelectedIndex = 0;

            #endregion

            #region [Combo]시생산 LOT 완료 등록

            //String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChildComplete = { cboEqsgComplete };
            _combo.SetCombo(cboAreaComplete, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildComplete, sCase: "AREA");

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParentComplete = { cboAreaComplete };
            C1ComboBox[] cboLineChildComplete = { cboProcComplete };
            _combo.SetCombo(cboEqsgComplete, CommonCombo.ComboStatus.ALL, cbChild: cboLineChildComplete, cbParent: cboLineParentComplete, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboProcessParentComplete = { cboAreaComplete, cboEqsgComplete };
            _combo.SetCombo(cboProcComplete, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentComplete, sCase: "PROCESSWITHAREA");

            if (cboEqsgComplete.Items.Count > 0) cboEqsgComplete.SelectedIndex = 0;
            if (cboProcComplete.Items.Count > 0) cboProcComplete.SelectedIndex = 0;

            // 구분
            //String[] sFilterReason = { "","PRDT_REQ_TYPE_CODE" };
            //_combo.SetCombo(cboPrdtReqType, CommonCombo.ComboStatus.ALL, sFilter: sFilterReason, sCase: "COMMCODES");

            #endregion

            #region [Combo]시생산 LOT 대상 등록 취소

            //동
            C1ComboBox[] cboAreaChildCancel = { cboEqsgCancel };
            _combo.SetCombo(cboAreaCancel, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildCancel, sCase: "AREA");

            //라인
            C1ComboBox[] cboLineParentCancel = { cboAreaCancel };
            C1ComboBox[] cboLineChildCancel = { cboProcCancel };
            _combo.SetCombo(cboEqsgCancel, CommonCombo.ComboStatus.ALL, cbChild: cboLineChildCancel, cbParent: cboLineParentCancel, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentCancel = { cboAreaCancel, cboEqsgCancel };
            _combo.SetCombo(cboProcCancel, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentCancel, sCase: "PROCESSWITHAREA");

            if (cboEqsgCancel.Items.Count > 0) cboEqsgCancel.SelectedIndex = 0;
            if (cboProcCancel.Items.Count > 0) cboProcCancel.SelectedIndex = 0;

            #endregion

            #region [Combo]시생산 LOT 대상 이력 조회

            //등록일시
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            //동
            C1ComboBox[] cboAreaChildHistory = { cboEqsgHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParentHistory = { cboAreaHistory };
            C1ComboBox[] cboLineChildHistory = { cboProcHistory };
            _combo.SetCombo(cboEqsgHistory, CommonCombo.ComboStatus.ALL, cbChild: cboLineChildHistory, cbParent: cboLineParentHistory, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboProcessParentHistory = { cboAreaHistory, cboEqsgHistory };
            _combo.SetCombo(cboProcHistory, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHistory, sCase: "PROCESSWITHAREA");

            //cboReqTypeHistory
            if (cboEqsgHistory.Items.Count > 0) cboEqsgHistory.SelectedIndex = 0;
            if (cboProcHistory.Items.Count > 0) cboProcHistory.SelectedIndex = 0;

            // 구분
            //String[] sFilterPrdtReqTypeHis = { "", "PRDT_REQ_TYPE_CODE" };
            //_combo.SetCombo(cboPrdtReqTypeHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterPrdtReqTypeHis, sCase: "COMMCODES");
            #endregion

            #region [Combo] 이력 조회 Tab
            //등록일시
            dtpDateFromHST.SelectedDateTime = DateTime.Now;
            dtpDateToHST.SelectedDateTime = DateTime.Now;

            //동
            C1ComboBox[] cboAreaChildHST = { cboEqsgHST };
            _combo.SetCombo(cboAreaHST, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHST, sCase: "AREA");

            //라인
            C1ComboBox[] cboLineParentHST = { cboAreaHST };
            C1ComboBox[] cboLineChildHST = { cboProcHST };
            _combo.SetCombo(cboEqsgHST, CommonCombo.ComboStatus.ALL, cbChild: cboLineChildHST, cbParent: cboLineParentHST, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboProcessParentHST = { cboAreaHST, cboEqsgHST };
            _combo.SetCombo(cboProcHST, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHST, sCase: "PROCESSWITHAREA");
                        
            if (cboEqsgHST.Items.Count > 0) cboEqsgHST.SelectedIndex = 0;
            if (cboProcHST.Items.Count > 0) cboProcHST.SelectedIndex = 0;
            
            #endregion
        }
        #endregion

        #region Event

        #region 버튼권한
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                DataTable dtInfo = ary.GetValue(0) as DataTable;

                foreach (DataRow dr in dtInfo.Rows)
                {
                    txtLotidRegister.Text = Util.NVC(dr["PALLETID"]);
                    GetLotList_Register(false);
                }
            }

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveRegister);
            listAuth.Add(btnSaveComplete);
            listAuth.Add(btnSaveCancel);
            listAuth.Add(btnSaveHistory);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회&저장]시생산 LOT 대상 등록
        private void btnSearchRegister_Click(object sender, RoutedEventArgs e)
        {
            chkAll.IsChecked = false;
            GetLotList_Register();
        }
      
        private void btnSaveRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Register())
                {
                    return;
                }
                //시생산 LOT 대상으로 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4555"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Register();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회&저장]시생산 LOT 완료 등록 
        private void btnSearchComplete_Click(object sender, RoutedEventArgs e)
        {
            chkAllComplete.IsChecked = false;
            GetLotList_Complete();
        }

        //시생산 LOT 완료 등록
        private void btnSaveComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Complete())
                {
                    return;
                }
                //시생산 LOT 대상을 완료처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4556"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Complete();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회&저장]시생산 LOT 대상 등록 취소
        private void btnSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Cancel();
        }

        private void btnSaveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Cancel())
                {
                    return;
                }
                //시생산 LOT 대상을 취소처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4557"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Cancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [조회&저장]시생산 LOT 대상 이력 조회
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_History();
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_History())
                {
                    return;
                }
                //완료처리된 시생산 LOT을 등록 상태로 변경하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4559"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_History();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [대상 선택하기]

        #region 시생산 LOT 대상 등록
        private void CheckBoxRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListRegister.Rows[idx].DataItem;

                AddToSelectRegister(objRowIdx);

                //if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                //{
                //    if (DataTableConverter.Convert(dgSelectRegister.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "'").Length == 1)
                //    {
                //        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                //        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                //        return;
                //    }
                //}

                //if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                //{
                //    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                //    DataRow dr = dtTo.NewRow();
                //    foreach (DataColumn dc in dtTo.Columns)
                //    {
                //        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                //    }
                //    dtTo.Rows.Add(dr);                    
                //    dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                //}
                //else
                //{
                //    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                //    if (dtTo.Rows.Count > 0)
                //    {
                //        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                //        {
                //            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                //            dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 시생산 LOT 등록 완료
        private void CheckBoxComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListComplete.Rows[idx].DataItem;

                AddToSelectComplete(objRowIdx);

                //if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                //{
                //    if (DataTableConverter.Convert(dgSelectComplete.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                //    {
                //        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                //        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                //        return;
                //    }
                //}

                //if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                //{
                //    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                //    DataRow dr = dtTo.NewRow();
                //    foreach (DataColumn dc in dtTo.Columns)
                //    {
                //        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                //    }
                //    dtTo.Rows.Add(dr);
                //    dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                //}
                //else
                //{
                //    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                //    if (dtTo.Rows.Count > 0)
                //    {
                //        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                //        {
                //            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                //            dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 시생산 LOT 대상 등록 취소
        private void CheckBoxCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListCancel.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    if (DataTableConverter.Convert(dgSelectCancel.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectCancel.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectCancel.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 시생산 LOT 대상 이력 조회
        private void CheckBoxHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListHistory.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    if (DataTableConverter.Convert(dgSelectHistory.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    if (!DataTableConverter.GetValue(objRowIdx, "PILOT_PROD_LOT_PROG_STAT_CODE").Equals("COMPLETE"))
                    {
                        //Util.MessageValidation("SFU4552", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        //Util.MessageValidation("SFU4552");
                        Util.MessageValidation("SFU4558", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));//해당 시생산 LOT ID[%1]은(는) 완료 등록된 시생산 LOT ID가 아닙니다.
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    //if (!Util.NVC(DataTableConverter.GetValue(objRowIdx, "PRDT_REQ_PRCS_STAT_CODE")).Equals("COMPLETE"))
                    //{
                    //    Util.MessageValidation("SFU4558", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));//해당 시생산 LOT ID[%1]은(는) 완료 등록된 시생산 LOT ID가 아닙니다.
                    //    DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                    //    return;
                    //}

                    DataTable dtTo = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectHistory.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectHistory.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region BizWF 처리 담당자
        private void txtPrscUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnPrscUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtPrscUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtPrscUser.Text = wndPerson.USERNAME;
                txtPrscUser.Tag = wndPerson.USERID;

                if (string.IsNullOrEmpty(txtDept.Text))
                {
                    txtDept.Text = wndPerson.DEPTNAME;
                    txtDept.Tag = wndPerson.DEPTID;
                }
                else
                {
                    txtDept.Text = string.Empty;
                    txtDept.Tag = string.Empty;
                }
            }
        }
        #endregion

        #region BizWF 부서
        private void txtDept_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetDepartmentWindow();
            }
        }

        private void btnDept_Click(object sender, RoutedEventArgs e)
        {
            GetDepartmentWindow();
        }

        private void GetDepartmentWindow()
        {
            CMM_DEPARTMENT wndDept = new CMM_DEPARTMENT();
            wndDept.FrameOperation = FrameOperation;

            if (wndDept != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtDept.Text;
                C1WindowExtension.SetParameters(wndDept, Parameters);

                wndDept.Closed += new EventHandler(wndDept_Closed);
                grdMain.Children.Add(wndDept);
                wndDept.BringToFront();
            }
        }
        private void wndDept_Closed(object sender, EventArgs e)
        {
            CMM_DEPARTMENT wndDept = sender as CMM_DEPARTMENT;
            if (wndDept.DialogResult == MessageBoxResult.OK)
            {
                txtDept.Text = wndDept.DEPTNAME;
                txtDept.Tag = wndDept.DEPTID;
            }
        }

        #endregion

        #region 처리수량(Lane) 자동계산(처리수량(Roll) * Lane)
        private void dgSelectRegister_LostFocus(object sender, RoutedEventArgs e)
        {
            IInputElement input = Keyboard.FocusedElement;

            if (input != sender)
            {
                var parent = VisualTreeHelper.GetParent(input as FrameworkElement) as FrameworkElement;

                while (parent != null)
                {
                    if (parent == sender)
                    {
                        break;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                }

                if (parent != null && parent is C1.WPF.DataGrid.C1DataGrid)
                {

                }
                else
                {
                    C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
                    datagrid.EndEdit();
                    datagrid.EndEditRow(true);
                }
            }
        }

        private void dgSelectRegister_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgSelectRegister.GetRowCount() == 0) return;

            try
            {
                if (Convert.ToString(e.Cell.Column.Name) == "ACTQTY")
                {                   
                    Decimal dACTQTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "ACTQTY"));
                    Decimal dWIPQTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "WIPQTY"));
                    Decimal dLANE_QTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "LANE_QTY"));

                    //일단 주석처리
/*
                    if (dACTQTY > dWIPQTY)
                    {
                        Util.MessageValidation("SFU4554");
                        DataTableConverter.SetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "ACTQTY", dWIPQTY);
                        return;
                    }
*/
                    DataTableConverter.SetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "ACTQTY2", dACTQTY * dLANE_QTY);
                }
                
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region 입력란 표시 -  처리수량(Roll, EA) Grid 색상변경
        private void dgSelectRegister_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "ACTQTY")
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                }));
            }
        }
        #endregion

        #region 폐기>시생산 탭

        /// <summary>
        /// ALL 체크 
        /// </summary>
        private void dgListScrapConv_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAllScrapConv;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAllScrapConv.Checked -= new RoutedEventHandler(checkAllScrapConv_Checked);
                        chkAllScrapConv.Unchecked -= new RoutedEventHandler(checkAllScrapConv_Unchecked);
                        chkAllScrapConv.Checked += new RoutedEventHandler(checkAllScrapConv_Checked);
                        chkAllScrapConv.Unchecked += new RoutedEventHandler(checkAllScrapConv_Unchecked);
                    }
                }
            }));
        }

        void checkAllScrapConv_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectScrapConv);

            if ((bool)chkAllScrapConv.IsChecked)
            {
                //// 동일한 PJT 만 전체 선택 가능하도록
                //if (dgListScrapCOnv.GetRowCount() > 0)
                //{
                //    if (DataTableConverter.Convert(dgListScrapCOnv.ItemsSource).Select("PRJT_NAME <> '" + Util.NVC(DataTableConverter.GetValue(dgListScrapCOnv.Rows[0].DataItem, "PRJT_NAME")) + "'").Length >= 1)
                //    {
                //        Util.MessageValidation("SFU4492"); //동일한 PJT 만 전체 선택이 가능합니다.
                //        chkAllScrapConv.IsChecked = false;
                //        return;
                //    }
                //}

                for (int inx = 0; inx < dgListScrapConv.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListScrapConv.Rows[inx].DataItem, "CHK", true);
                    object objRowIdx = dgListScrapConv.Rows[inx].DataItem;
                    AddToSelectScrapConv(objRowIdx);
                }
            }
        }

        void checkAllScrapConv_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectScrapConv);

            for (int inx = 0; inx < dgListScrapConv.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListScrapConv.Rows[inx].DataItem, "CHK", false);
            }
        }

        /// <summary>
        /// 시생산 LOT 대상 등록
        /// </summary>
        private void CheckBoxScrapConv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListScrapConv.Rows[idx].DataItem;

                AddToSelectScrapConv(objRowIdx);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 처리수량(Lane) 자동계산(처리수량(Roll) * Lane)
        /// </summary>
        private void dgSelectScrapConv_LostFocus(object sender, RoutedEventArgs e)
        {
            IInputElement input = Keyboard.FocusedElement;

            if (input != sender)
            {
                var parent = VisualTreeHelper.GetParent(input as FrameworkElement) as FrameworkElement;

                while (parent != null)
                {
                    if (parent == sender)
                    {
                        break;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                }

                if (parent != null && parent is C1.WPF.DataGrid.C1DataGrid)
                {

                }
                else
                {
                    C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
                    datagrid.EndEdit();
                    datagrid.EndEditRow(true);
                }
            }
        }

        private void dgSelectScrapConv_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgSelectScrapConv.GetRowCount() == 0) return;

            try
            {
                if (Convert.ToString(e.Cell.Column.Name) == "ACTQTY")
                {
                    Decimal dACTQTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectScrapConv.Rows[dgSelectScrapConv.SelectedIndex].DataItem, "ACTQTY"));
                    Decimal dWIPQTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectScrapConv.Rows[dgSelectScrapConv.SelectedIndex].DataItem, "WIPQTY"));
                    Decimal dLANE_QTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectScrapConv.Rows[dgSelectScrapConv.SelectedIndex].DataItem, "LANE_QTY"));


                    if (!formatFlag)
                    {
                        dACTQTY = Math.Floor(dACTQTY);
                        DataTableConverter.SetValue(dgSelectScrapConv.Rows[dgSelectScrapConv.SelectedIndex].DataItem, "ACTQTY", dACTQTY);
                    }

                    //일단 주석처리
                    /*
                                        if (dACTQTY > dWIPQTY)
                                        {
                                            Util.MessageValidation("SFU4554");
                                            DataTableConverter.SetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "ACTQTY", dWIPQTY);
                                            return;
                                        }
                    */
                    DataTableConverter.SetValue(dgSelectScrapConv.Rows[dgSelectScrapConv.SelectedIndex].DataItem, "ACTQTY2", dACTQTY * dLANE_QTY);
                }

            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void dgSelectScrapConv_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "ACTQTY")
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                }));
            }


        }
        
        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearchScrapConv_Click(object sender, RoutedEventArgs e)
        {
            chkAllScrapConv.IsChecked = false;
            GetLotList_ScrapConv();
        }

        /// <summary>
        /// 등록
        /// </summary>
        private void btnSaveScrapConv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_ScrapConv())
                {
                    return;
                }
                //시생산 LOT 대상으로 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4555"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_ScrapConv();
                            }
                        });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [이력조회 탭]
        private void btnSearchHST_Click(object sender, RoutedEventArgs e)
        {
            GetHistoryLotList();
        }
        #endregion

        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region 시생산 LOT 대상 등록 조회

        public void GetLotList_Register(bool bButton = true)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_WIP";

                DataTable dtRqst = new DataTable();

                if (String.IsNullOrWhiteSpace(txtCstidRegister.Text))
                {
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("EQSGID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    dtRqst.Columns.Add("PRODID", typeof(string));
                    dtRqst.Columns.Add("MODLID", typeof(string));
                    dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));
                    dtRqst.Columns.Add("CTNR_ID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboAreaRegister, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;

                    string sEqsgID = Util.ConvertEmptyToNull((string)cboEqsgRegister.SelectedValue);
                    string sProcID = Util.ConvertEmptyToNull((string)cboProcRegister.SelectedValue);
                    string sProdID = Util.ConvertEmptyToNull(txtProdRegister.Text.Trim());
                    string sModlID = Util.ConvertEmptyToNull(txtModlRegister.Text.Trim());
                    string sPrjtName = Util.ConvertEmptyToNull(txtPrjtRegister.Text.Trim());
                    string sLotID = Util.ConvertEmptyToNull(txtLotidRegister.Text.Trim());
                    string sCtnrID = Util.ConvertEmptyToNull(txtCtnridRegister.Text.Trim());

                    if (!string.IsNullOrEmpty(sEqsgID))
                        dr["EQSGID"] = sEqsgID;
                    if (!string.IsNullOrEmpty(sProcID))
                        dr["PROCID"] = sProcID;
                    if (!string.IsNullOrEmpty(sProdID))
                        dr["PRODID"] = sProdID;
                    if (!string.IsNullOrEmpty(sModlID))
                        dr["MODLID"] = sModlID;
                    if (!string.IsNullOrEmpty(sPrjtName))
                        dr["PRJT_NAME"] = sPrjtName;
                    if (!string.IsNullOrEmpty(sLotID))
                        dr["LOTID"] = sLotID;
                    if (!string.IsNullOrEmpty(sCtnrID))
                        dr["CTNR_ID"] = sCtnrID;

                    dtRqst.Rows.Add(dr);
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_WIP_L";

                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = txtCstidRegister.Text;

                    dtRqst.Rows.Add(dr);
                }

               // DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_WIP", "INDATA", "OUTDATA", dtRqst);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    Util.MessageConfirm("SFU1905", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotidRegister.Focus();
                            txtLotidRegister.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListRegister.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtSource, FrameOperation, true);
                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);

                        if (string.IsNullOrWhiteSpace(txtLotidRegister.Text))
                        {
                            txtCstidRegister.Text = string.Empty;
                            txtCstidRegister.Focus();
                        }
                        else
                        {
                            txtLotidRegister.Text = string.Empty;
                            txtLotidRegister.Focus();
                        }
                    }
                    else
                    {
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtRslt, FrameOperation, true); DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);

                        if (string.IsNullOrWhiteSpace(txtLotidRegister.Text))
                        {
                            txtCstidRegister.Text = string.Empty;
                            txtCstidRegister.Focus();
                        }
                        else
                        {
                            txtLotidRegister.Text = string.Empty;
                            txtLotidRegister.Focus();
                        }
                    }


                }
                else
                {
                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    DataTable dtSelect = GetDtRegister();
                    Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 폐기 > 시생산 조회

        private void GetLotList_ScrapConv(bool bButton = true)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_WIP_SCRAPPED";

                DataTable dtRqst = new DataTable();

                if (String.IsNullOrWhiteSpace(txtCstidScrapConv.Text))
                {
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("EQSGID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));
                    dtRqst.Columns.Add("PRODID", typeof(string));
                    dtRqst.Columns.Add("MODLID", typeof(string));
                    dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                    dtRqst.Columns.Add("CTNR_ID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboAreaScrapConv, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;

                    string sEqsgID = Util.ConvertEmptyToNull((string)cboEqsgScrapConv.SelectedValue);
                    string sProcID = Util.ConvertEmptyToNull((string)cboProcScrapConv.SelectedValue);
                    string sLotID = Util.ConvertEmptyToNull(txtLotidScrapConv.Text.Trim());
                    string sProdID = Util.ConvertEmptyToNull(txtProdScrapConv.Text.Trim());
                    string sModlID = Util.ConvertEmptyToNull(txtModlScrapConv.Text.Trim());
                    string sPrjtName = Util.ConvertEmptyToNull(txtPrjtScrapConv.Text.Trim());
                    string sCtnrID = Util.ConvertEmptyToNull(txtCtnridScrapConv.Text.Trim());

                    if (!string.IsNullOrEmpty(sEqsgID))
                        dr["EQSGID"] = sEqsgID;
                    if (!string.IsNullOrEmpty(sProcID))
                        dr["PROCID"] = sProcID;
                    if (!string.IsNullOrEmpty(sLotID))
                        dr["LOTID"] = sLotID;
                    if (!string.IsNullOrEmpty(sProdID))
                        dr["PRODID"] = sProdID;
                    if (!string.IsNullOrEmpty(sModlID))
                        dr["MODLID"] = sModlID;
                    if (!string.IsNullOrEmpty(sPrjtName))
                        dr["PRJT_NAME"] = sPrjtName;
                    if (!string.IsNullOrEmpty(sCtnrID))
                        dr["CTNR_ID"] = sCtnrID;

                    dtRqst.Rows.Add(dr);
                }
                else
                {
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = txtCstidScrapConv.Text;

                    dtRqst.Rows.Add(dr);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    // 조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905");

                    Util.GridSetData(dgListScrapConv, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectScrapConv);

                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    Util.MessageConfirm("SFU1905", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotidScrapConv.Focus();
                            txtLotidScrapConv.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListScrapConv.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListScrapConv);
                        Util.GridSetData(dgListScrapConv, dtSource, FrameOperation, true);
                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectScrapConv, dtSelect, FrameOperation, false);

                        if (string.IsNullOrWhiteSpace(txtLotidScrapConv.Text))
                        {
                            txtLotidScrapConv.Text = string.Empty;
                            txtLotidScrapConv.Focus();
                        }
                        else
                        {
                            txtLotidScrapConv.Text = string.Empty;
                            txtLotidScrapConv.Focus();
                        }
                    }
                    else
                    {
                        Util.gridClear(dgListScrapConv);
                        Util.GridSetData(dgListScrapConv, dtRslt, FrameOperation, true);
                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectScrapConv, dtSelect, FrameOperation, false);

                        if (string.IsNullOrWhiteSpace(txtLotidScrapConv.Text))
                        {
                            txtLotidScrapConv.Text = string.Empty;
                            txtLotidScrapConv.Focus();
                        }
                        else
                        {
                            txtLotidScrapConv.Text = string.Empty;
                            txtLotidScrapConv.Focus();
                        }
                    }
                }
                else
                {
                    Util.GridSetData(dgListScrapConv, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectScrapConv);

                    DataTable dtSelect = GetDtRegister();
                    Util.GridSetData(dgSelectScrapConv, dtSelect, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #region 시생산 LOT 완료 등록 조회
        public void GetLotList_Complete()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_CREATE";

                DataTable dtRqst = new DataTable();

                if (String.IsNullOrWhiteSpace(txtCstidComplete.Text))
                {
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("EQSGID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    //dtRqst.Columns.Add("PRDT_REQ_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("PRODID", typeof(string));
                    dtRqst.Columns.Add("MODLID", typeof(string));
                    dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboAreaComplete, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEqsgComplete.SelectedValue);
                    dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcComplete.SelectedValue);
                    //dr["PRDT_REQ_TYPE_CODE"] = (string)cboPrdtReqType.SelectedValue == "SELECT" ? null :(string)cboPrdtReqType.SelectedValue;
                    dr["PRODID"] = Util.ConvertEmptyToNull(txtProdComplete.Text.Trim());
                    dr["MODLID"] = Util.ConvertEmptyToNull(txtModlComplete.Text.Trim());
                    dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtComplete.Text.Trim());
                    dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidComplete.Text.Trim());
                    dtRqst.Rows.Add(dr);
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_CREATE_L";

                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = Util.ConvertEmptyToNull(txtCstidComplete.Text.Trim());
                    dtRqst.Rows.Add(dr);
                }

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_CREATE", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListComplete, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectComplete);

                    return;
                }

                Util.GridSetData(dgListComplete, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectComplete);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectComplete, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 시생산 LOT 대상 등록 취소 조회
        private void GetLotList_Cancel()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_CREATE";

                const string sPRDT_REQ_TYPE_CODE = "STOCK";
                DataTable dtRqst = new DataTable();

                if (String.IsNullOrWhiteSpace(txtCstidCancel.Text))
                {
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("EQSGID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    dtRqst.Columns.Add("PRDT_REQ_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("PRODID", typeof(string));
                    dtRqst.Columns.Add("MODLID", typeof(string));
                    dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboAreaCancel, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEqsgCancel.SelectedValue);
                    dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcCancel.SelectedValue);
                    dr["PRDT_REQ_TYPE_CODE"] = sPRDT_REQ_TYPE_CODE;
                    dr["PRODID"] = Util.ConvertEmptyToNull(txtProdCancel.Text.Trim());
                    dr["MODLID"] = Util.ConvertEmptyToNull(txtModlCancel.Text.Trim());
                    dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtCancel.Text.Trim());
                    dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidCancel.Text.Trim());
                    dtRqst.Rows.Add(dr);
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_CREATE_L";

                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = Util.ConvertEmptyToNull(txtCstidCancel.Text.Trim());
                    dtRqst.Rows.Add(dr);
                }


                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_CREATE", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListCancel, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectCancel);

                    return;
                }

                Util.GridSetData(dgListCancel, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectCancel);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectCancel, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 시생산 LOT 대상 이력 조회 조회
        public void GetLotList_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                //dtRqst.Columns.Add("PRDT_REQ_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEqsgHistory.SelectedValue);
                dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcHistory.SelectedValue);
                //dr["PRDT_REQ_TYPE_CODE"] = Util.ConvertEmptyToNull((string)cboPrdtReqTypeHistory.SelectedValue);
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdHistory.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlHistory.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtHistory.Text.Trim());
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidHistory.Text.Trim());
                //dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                //dr["TODATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["CSTID"] = Util.ConvertEmptyToNull(txtCstidHistory.Text.Trim());
                dtRqst.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_ALL", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_ALL", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListHistory, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectHistory);

                    return;
                }

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectHistory);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectHistory, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 선택목록 DataTable
        private DataTable GetDtRegister()
        {
            DataTable dtSelect = new DataTable();

            dtSelect.Columns.Add("EQSGID", typeof(string));
            dtSelect.Columns.Add("EQSGNAME", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("MODLID", typeof(string));
            dtSelect.Columns.Add("PRODNAME", typeof(string));
            dtSelect.Columns.Add("WIPQTY", typeof(string));
            dtSelect.Columns.Add("WIPQTY2", typeof(string));
            dtSelect.Columns.Add("ACTQTY", typeof(string));
            dtSelect.Columns.Add("ACTQTY2", typeof(string));
            dtSelect.Columns.Add("UNIT_CODE", typeof(string));
            dtSelect.Columns.Add("LANE_QTY", typeof(string));
            dtSelect.Columns.Add("LOTTYPE", typeof(string));
            dtSelect.Columns.Add("CSTID", typeof(string));

            return dtSelect;
        }

        private DataTable GetDtComplete()
        {
            DataTable dtSelect = new DataTable();

            dtSelect.Columns.Add("EQSGNAME", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("WIPSEQ", typeof(string));
            dtSelect.Columns.Add("RESNCODE", typeof(string));
            dtSelect.Columns.Add("ACTID", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("MODLID", typeof(string));
            dtSelect.Columns.Add("PRODNAME", typeof(string));
            dtSelect.Columns.Add("RESNQTY", typeof(string));
            dtSelect.Columns.Add("RESNQTY2", typeof(string));
            dtSelect.Columns.Add("UNIT_CODE", typeof(string));
            dtSelect.Columns.Add("RESNNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_DEPTNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_USERNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
            dtSelect.Columns.Add("REG_USERNAME", typeof(string));
            dtSelect.Columns.Add("REG_DTTM", typeof(string));
            dtSelect.Columns.Add("CMPL_USERNAME", typeof(string));
            dtSelect.Columns.Add("CMPL_DTTM", typeof(string));
            dtSelect.Columns.Add("PILOT_PROD_LOT_NOTE", typeof(string));
            dtSelect.Columns.Add("CSTID", typeof(string));

            return dtSelect;
        }
        #endregion
        #endregion

        #region [선택목록 등록]

        #region 시생산 LOT 대상 등록
        private void REQ_Register()
        {
            try
            {
                DataSet inData = new DataSet();

                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_DEPTID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_USERID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
                inDataTable.Columns.Add("COST_PRCS_SLOC_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["BIZ_WF_DEPTID"] = txtDept.Tag;
                row["BIZ_WF_PRCS_USERID"] = txtPrscUser.Tag;
                row["BIZ_WF_PRCS_SCHD_CMPL_DATE"] = ldpCmplDateRegister.SelectedDateTime.ToString("yyyyMMdd");
                row["COST_PRCS_SLOC_ID"] = (string)cboCostSloc.SelectedValue; ;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable dtSelectRegister = DataTableConverter.Convert(dgSelectRegister.ItemsSource);

                //INLOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("ACTQTY", typeof(string));
                inLot.Columns.Add("ACTQTY2", typeof(string));
                inLot.Columns.Add("WIPNOTE", typeof(string));
                inLot.Columns.Add("LOTTYPE", typeof(string));

                for (int i = 0; i < dtSelectRegister.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectRegister.Rows[i]["LOTID"]);
                    row["ACTQTY"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY"]);
                    row["ACTQTY2"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY2"]);
                    row["WIPNOTE"] = txtNoteRegister.Text;
                    row["LOTTYPE"] = Convert.ToString(dtSelectRegister.Rows[i]["LOTTYPE"]); 
                    inLot.Rows.Add(row);
                }

                DataTable inResn = inData.Tables.Add("INRESN");
                inResn.Columns.Add("LOTID", typeof(string));
                inResn.Columns.Add("RESNCODE", typeof(string));
                inResn.Columns.Add("RESNQTY", typeof(decimal));
                inResn.Columns.Add("RESNQTY2", typeof(decimal));
                inResn.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inResn.Columns.Add("PROCID_CAUSE", typeof(string));
                inResn.Columns.Add("RESNNOTE", typeof(string));
                inResn.Columns.Add("COST_CNTR_ID", typeof(string));

                
                for (int i = 0; i < dtSelectRegister.Rows.Count; i++)
                {
                    row = inResn.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectRegister.Rows[i]["LOTID"]);
                    row["RESNCODE"] = (string)cboResnCode.SelectedValue;
                    row["RESNQTY"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY"]);
                    row["RESNQTY2"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY2"]);
                    inResn.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //시생산 LOT 대상 등록 처리
                //new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STOCK", "INDATA,INLOT,INRESN", null, (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_PILOT_PROD_LOT", "INDATA,INLOT,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        //GetLotList_Register();

                        cboResnCode.SelectedIndex = 0;
                        cboCostSloc.SelectedIndex = 0;
                        txtDept.Text = string.Empty;
                        txtDept.Tag = string.Empty;
                        txtPrscUser.Text = string.Empty;
                        txtPrscUser.Tag = string.Empty;
                        ldpCmplDateRegister.SelectedDateTime = DateTime.Now;
                        txtNoteRegister.Text = string.Empty;
                        chkAll.IsChecked = false;

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
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_STOCK", ex.Message, ex.ToString());
            }
        }
        #endregion

        #region 폐기 > 시생산 LOT 대상 등록
        private void REQ_ScrapConv()
        {
            try
            {
                DataSet inData = new DataSet();

                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["IFMODE"] = "OFF";
                row["NOTE"] = txtNoteScrapConv.Text;
                inDataTable.Rows.Add(row);

                DataTable dtSelectScrapConv = DataTableConverter.Convert(dgSelectScrapConv.ItemsSource);

                //INLOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("ACTQTY", typeof(string));
                inLot.Columns.Add("ACTQTY2", typeof(string));

                for (int i = 0; i < dtSelectScrapConv.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectScrapConv.Rows[i]["LOTID"]);
                    row["ACTQTY"] = Convert.ToString(dtSelectScrapConv.Rows[i]["ACTQTY"]);
                    row["ACTQTY2"] = Convert.ToString(dtSelectScrapConv.Rows[i]["ACTQTY2"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //시생산 LOT 대상 등록 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PILOT_SCRAPPED_LOT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_ScrapConv();

                        txtNoteScrapConv.Text = string.Empty;
                        chkAllScrapConv.IsChecked = false;
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
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 시생산 LOT 완료 등록
        private void REQ_Complete()
        {
            try
            {
                DataSet inData = new DataSet();
                
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_END_DATE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PILOT_PROD_LOT_PROG_STAT_CODE", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["PRDT_REQ_PRCS_STAT_CODE"] = "COMPLETE";
                row["NOTE"] = txtNoteComplete.Text;
                row["BIZ_WF_PRCS_END_DATE"] = ldpCmplDateComplete.SelectedDateTime.ToString("yyyyMMdd");
                row["USERID"] = LoginInfo.USERID;
                row["PILOT_PROD_LOT_PROG_STAT_CODE"] = "COMPLETE";
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(string));
                inLot.Columns.Add("ACTID", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));

                DataTable dtSelectComplete = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                for (int i = 0; i < dtSelectComplete.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectComplete.Rows[i]["LOTID"]);
                    row["WIPSEQ"] = Convert.ToString(dtSelectComplete.Rows[i]["WIPSEQ"]);
                    row["ACTID"] = Convert.ToString(dtSelectComplete.Rows[i]["ACTID"]);
                    row["RESNCODE"] = Convert.ToString(dtSelectComplete.Rows[i]["RESNCODE"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //시생산 LOT 대상 완료등록 처리
                //new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_PILOT_PROD_LOT_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Complete();

                        ldpCmplDateComplete.SelectedDateTime = DateTime.Now;
                        txtNoteComplete.Text = string.Empty;
                        chkAllComplete.IsChecked = false;
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
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_STATUS", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 시생산 LOT 대상 등록 취소
        private void REQ_Cancel()
        {
            try
            {
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PILOT_PROD_LOT_PROG_STAT_CODE", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["PRDT_REQ_PRCS_STAT_CODE"] = "CANCEL";
                row["NOTE"] = txtNoteCancel.Text;
                row["USERID"] = LoginInfo.USERID;
                row["PILOT_PROD_LOT_PROG_STAT_CODE"] = "CANCEL";
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(decimal));
                inLot.Columns.Add("ACTID", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));

                DataTable dtSelectCancel = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                for (int i = 0; i < dtSelectCancel.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectCancel.Rows[i]["LOTID"]);
                    row["WIPSEQ"] = Convert.ToString(dtSelectCancel.Rows[i]["WIPSEQ"]);
                    row["ACTID"] = Convert.ToString(dtSelectCancel.Rows[i]["ACTID"]); 
                    row["RESNCODE"] = Convert.ToString(dtSelectCancel.Rows[i]["RESNCODE"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //시생산 LOT 대상 완료등록 처리
                //new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_PILOT_PROD_LOT_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                 {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Cancel();

                        txtNoteCancel.Text = string.Empty;

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
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_STATUS", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 시생산 LOT 대상 이력 조회
        private void REQ_History()
        {
            try
            {
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PRDT_REQ_PRCS_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PILOT_PROD_LOT_PROG_STAT_CODE", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["PRDT_REQ_PRCS_STAT_CODE"] = "CREATE";
                row["NOTE"] = txtNoteHistory.Text;
                row["USERID"] = LoginInfo.USERID;
                row["PILOT_PROD_LOT_PROG_STAT_CODE"] = "CREATE";
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(decimal));
                inLot.Columns.Add("ACTID", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));

                DataTable dtSelectHistory = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                for (int i = 0; i < dtSelectHistory.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectHistory.Rows[i]["LOTID"]);
                    row["WIPSEQ"] = Convert.ToString(dtSelectHistory.Rows[i]["WIPSEQ"]);
                    row["ACTID"] = Convert.ToString(dtSelectHistory.Rows[i]["ACTID"]);
                    row["RESNCODE"] = Convert.ToString(dtSelectHistory.Rows[i]["RESNCODE"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //시생산 LOT 대상 완료등록 처리
                //new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_PILOT_PROD_LOT_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_History();

                        txtNoteHistory.Text = string.Empty;

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
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_STATUS", ex.Message, ex.ToString());
            }
        }

        #endregion

        #endregion

        #region [Validation]

        #region 시생산 LOT 대상 등록
        private bool Validation_Register()
        {
            if (dgSelectRegister.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            DataTable dtSelectRegister = DataTableConverter.Convert(dgSelectRegister.ItemsSource);

            int iACTQTY = dtSelectRegister.Select("ACTQTY = '' ").Count();
            int iACTQTY2 = dtSelectRegister.Select("ACTQTY2 = '' ").Count();

            if (iACTQTY > 0 || iACTQTY2 > 0)
            {
                Util.MessageValidation("SFU4132"); //처리수량(Roll, Lane)을 입력 하세요.
                return false;
            }

            if (cboResnCode.SelectedValue == null || cboResnCode.SelectedValue.ToString().Equals("SELECT"))
            {
                    Util.MessageValidation("SFU1593"); //사유를 선택하세요.
                return false;
            }

            if (cboCostSloc.SelectedValue == null || cboCostSloc.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU4131"); //비용저장위치를 선택해 주세요.
                return false;
            }

            //if (String.IsNullOrEmpty((String)txtDept.Tag))
            //{
            //    Util.MessageValidation("SFU4105"); //BizWF 처리 부서를 선택해 주세요.
            //    return false;
            //}

            if (String.IsNullOrEmpty((String)txtPrscUser.Tag))
            {
                Util.MessageValidation("SFU4106"); //BizWF 처리 담당자를 선택해 주세요.
                return false;
            }

            return true;
        }
        #endregion

        #region 폐기 > 시생산 LOT 대상 등록
        private bool Validation_ScrapConv()
        {
            if (dgSelectScrapConv.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            DataTable dtSelectScrapConv = DataTableConverter.Convert(dgSelectScrapConv.ItemsSource);

            //int iCount = dtSelectScrapConv.AsEnumerable().Count(row => row.Field<string>("ACTQTY") == "0" || row.Field<string>("ACTQTY2") == "0");
            foreach (DataRow dr in dtSelectScrapConv.Rows)
            {
                Decimal dActQty = Util.NVC_Decimal(dr["ACTQTY"]);
                Decimal dActQty2 = Util.NVC_Decimal(dr["ACTQTY2"]);

                if (dActQty == 0 || dActQty2 == 0)
                {
                    Util.MessageValidation("SFU4132"); //처리수량(Roll, Lane)을 입력 하세요.
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(txtNoteScrapConv.Text))
            {
                Util.MessageValidation("SFU1590"); //비고를 입력 하세요.
                return false;
            }

            return true;
        }
        #endregion

        #region 시생산 LOT 완료 등록
        private bool Validation_Complete()
        {
            if (dgSelectComplete.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region 시생산 LOT 등록 취소
        private bool Validation_Cancel()
        {
            if (dgSelectCancel.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region 시생산 LOT 대상 이력조회
        private bool Validation_History()
        {
            if (dgSelectHistory.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }



        #endregion

        #endregion

        #region [이력조회]
        public void GetHistoryLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHST, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEqsgHST.SelectedValue);
                dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcHST.SelectedValue);
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdHST.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlHST.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtHST.Text.Trim());
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidHST.Text.Trim());
                //dr["FROMDATE"] = dtpDateFromHST.SelectedDateTime.ToShortDateString();
                //dr["TODATE"] = dtpDateToHST.SelectedDateTime.ToShortDateString();
                dr["FROMDATE"] = dtpDateFromHST.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateToHST.SelectedDateTime.ToString("yyyyMMdd");
                dr["CSTID"] = Util.ConvertEmptyToNull(txtCstidHST.Text.Trim());
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_PILOT_PROD_LOT_REG_HIST_FOR_HISTORY", "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count == 0)
                        {
                            Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                        }

                        Util.GridSetData(dgListHST, searchResult, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion
        #endregion

        private void txtLotidRegister_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtLotidRegister.Text.Trim();
                    if (dgListRegister.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListRegister.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListRegister.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {

                                dgListRegister.SelectedIndex = i;
                                dgListRegister.ScrollIntoView(i, dgListRegister.Columns["CHK"].Index);
                                txtLotidRegister.Focus();
                                txtLotidRegister.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Register(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCstidRegister_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sCstid = txtCstidRegister.Text.Trim();
                    if (dgListRegister.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListRegister.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgListRegister.Rows[i].DataItem, "CSTID")) == sCstid)
                            {

                                dgListRegister.SelectedIndex = i;
                                dgListRegister.ScrollIntoView(i, dgListRegister.Columns["CHK"].Index);
                                txtCstidRegister.Focus();
                                txtCstidRegister.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Register(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListRegister_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
            Util.gridClear(dgSelectRegister);

            if ((bool)chkAll.IsChecked)
            {
                
                // 동일한 PJT 만 전체 선택 가능하도록
                if (dgListRegister.GetRowCount() > 0) {
                    if (DataTableConverter.Convert(dgListRegister.ItemsSource).Select("PRJT_NAME <> '" + Util.NVC(DataTableConverter.GetValue(dgListRegister.Rows[0].DataItem, "PRJT_NAME")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4492"); //동일한 PJT 만 전체 선택이 가능합니다.
                        chkAll.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListRegister.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListRegister.Rows[inx].DataItem, "CHK", true);
                    object objRowIdx = dgListRegister.Rows[inx].DataItem;
                    AddToSelectRegister(objRowIdx);
                }
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectRegister);

            for (int inx = 0; inx < dgListRegister.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListRegister.Rows[inx].DataItem, "CHK", false);
            }
        }

        private void AddToSelectRegister(object objRowIdx)
        {
            try
            {
                if (objRowIdx == null) return;

                //if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True") || DataTableConverter.GetValue(objRowIdx, "CHK").GetInt() == 1) // 2024.11.12. 김영국 - DataType 변경에 따른 CHK값 1로 비교 로직 추가.
                {
                    if (DataTableConverter.Convert(dgSelectRegister.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                   
                }

                //if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True") || DataTableConverter.GetValue(objRowIdx, "CHK").GetInt() == 1)  // 2024.11.12. 김영국 - DataType 변경에 따른 CHK값 1로 비교 로직 추가.
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddToSelectScrapConv(object objRowIdx)
        {
            try
            {
                if (objRowIdx == null) return;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    if (DataTableConverter.Convert(dgSelectScrapConv.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }

                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectScrapConv.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectScrapConv.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectScrapConv.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectScrapConv.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAllComplete_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectComplete);

            if ((bool)chkAllComplete.IsChecked)
            {

                // 동일한 PJT 만 전체 선택 가능하도록
                if (dgListComplete.GetRowCount() > 0)
                {
                    if (DataTableConverter.Convert(dgListComplete.ItemsSource).Select("PRJT_NAME <> '" + Util.NVC(DataTableConverter.GetValue(dgListComplete.Rows[2].DataItem, "PRJT_NAME")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4492"); //동일한 PJT 만 전체 선택이 가능합니다.
                        chkAllComplete.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListComplete.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListComplete.Rows[inx + 2].DataItem, "CHK", true);
                    object objRowIdx = dgListComplete.Rows[inx + 2].DataItem;
                    AddToSelectComplete(objRowIdx);
                }
            }
        }

        void checkAllComplete_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectComplete);

            for (int inx = 0; inx < dgListComplete.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListComplete.Rows[inx + 2].DataItem, "CHK", false);
            }
        }

        // 전체선택 체크박스 그리드 헤더에 넣을려고 했으나 결국 실패함. 작업한 내용 주석으로 남김. 혹시 나중에 아는 사람있으면 코드 수정해서 그리드 헤더에 넣어주세요.
        //C1.WPF.DataGrid.DataGridCellPresenter preCellComplete = new C1.WPF.DataGrid.DataGridCellPresenter()
        //{
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        //};
        //C1.WPF.DataGrid.DataGridColumnHeaderPresenter preHeaderComplete = new C1.WPF.DataGrid.DataGridColumnHeaderPresenter()
        //{
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        //};
        //CheckBox chkAllComplete = new CheckBox()
        //{
        //    IsChecked = false,
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    VerticalAlignment = System.Windows.VerticalAlignment.Center,
        //    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        //};

        //private void dgListComplete_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (String.IsNullOrEmpty(e.Cell.Column.Name) == false && e.Cell.Column.Name.Equals("CHK"))
        //        {
        //            if (e.Cell.Row.Index == 0 || e.Cell.Row.Index == 1)
        //            {
        //                preCellComplete.Content = chkAllComplete;
        //                e.Cell.Presenter.Content = preCellComplete;
        //                chkAllComplete.Checked -= new RoutedEventHandler(checkAllComplete_Checked);
        //                chkAllComplete.Unchecked -= new RoutedEventHandler(checkAllComplete_Unchecked);
        //                chkAllComplete.Checked += new RoutedEventHandler(checkAllComplete_Checked);
        //                chkAllComplete.Unchecked += new RoutedEventHandler(checkAllComplete_Unchecked);
        //            }
        //        }
        //    }));
        //}

        //private void dgListComplete_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (string.IsNullOrEmpty(e.Column.Name) == false)
        //        {
        //            if (e.Column.Name.Equals("CHK"))
        //            {
        //                preHeaderComplete.Content = chkAllComplete;
        //                e.Column.HeaderPresenter.Content = preHeaderComplete;
        //                chkAll.Checked -= new RoutedEventHandler(checkAllComplete_Checked);
        //                chkAll.Unchecked -= new RoutedEventHandler(checkAllComplete_Unchecked);
        //                chkAll.Checked += new RoutedEventHandler(checkAllComplete_Checked);
        //                chkAll.Unchecked += new RoutedEventHandler(checkAllComplete_Unchecked);
        //            }
        //        }
        //    }));
        //}

        private void AddToSelectComplete(object objRowIdx)
        {
            try
            {
                if (objRowIdx == null) return;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    if (DataTableConverter.Convert(dgSelectComplete.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND WIPSEQ = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "WIPSEQ")) + "' AND RESNCODE = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "RESNCODE")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region 이력조회 라벨 발행
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button dg = sender as Button;
                if (dg != null &&
                    dg.DataContext != null &&
                    (dg.DataContext as DataRowView).Row != null)
                {
                    PrintLabel((dg.DataContext as DataRowView).Row);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void PrintLabel(DataRow dr)
        {
            if (dr["AREA_TYPE_CODE"].ToString() == "E")
            {
                // 전극
                Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(dr["LOTID"]), Util.NVC(dr["PROCID"]), "");
            }
            else
            {
                // 조립
                // 프린터 정보 조회
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;
                string sLBCD = string.Empty;    // 리턴 라벨 타입 코드
                string sEqpt = string.Empty;
                DataRow drPrtInfo = null;

                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                    return;
                }
                else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                {
                    if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                        return;
                }
                else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                {
                    foreach (DataRow drPrint in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        sPrt = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                        sRes = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                        sCopy = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                        sXpos = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                        sYpos = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                        sDark = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                        sEqpt = drPrint[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                        drPrtInfo = drPrint;
                    }
                }

                string sZPL = GetPrintInfo(Util.NVC(dr["LOTID"]), Util.NVC(dr["WIPSEQ"]), Util.NVC(dr["PROCID"]), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);
                string sLot = Util.NVC(dr["LOTID"]);

                if (sZPL.Equals(""))
                {
                    Util.MessageValidation("SFU1498");
                    return;
                }

                if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                {
                    PrintLabel(sZPL.Substring(2), drPrtInfo);
                }
            }
        }

        private string GetPrintInfo(string sLot, string sWipSeq, string sProcID, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = sProcID;
                newRow["EQPTID"] = DBNull.Value;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness

                //  노칭 라벨 바코드 품질 테스트를 하기 위한 신규건
                // 염규범                
                if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals(""))
                    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                else
                    newRow["LBCD"] = "LBL0001"; // LABEL CODE
                newRow["NT_WAIT_YN"] = "N"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030")); // 프린터 환경설정 정보가 없습니다.

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); // Barcode Print 실패
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); // Barcode Print 실패
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); // Barcode Print 실패
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031")); // 프린터 환경설정에 포트명 항목이 없습니다.
                Util.MessageValidation("SFU3031"); // 프린터 환경설정에 포트명 항목이 없습니다.
            }

            return brtndefault;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }



        #endregion

        private void dgSelectScrapConv_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "ACTQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            if (drv["UNIT_CODE"] != null)
                            {
                                if (drv["UNIT_CODE"].GetString() == "M" || drv["UNIT_CODE"].GetString() == "KG")
                                {
                                    formatFlag = true;
                                    //((C1.WPF.DataGrid.DataGridBoundColumn)(dgSelectScrapConv.Columns["ACTQTY"])).Format = "N2";
                                }
                                else
                                {
                                    formatFlag = false;
                                    //((C1.WPF.DataGrid.DataGridBoundColumn)(dgSelectScrapConv.Columns["ACTQTY"])).Format = "N0";
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
