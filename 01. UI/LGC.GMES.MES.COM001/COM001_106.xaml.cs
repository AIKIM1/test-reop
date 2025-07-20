/*************************************************************************************
 Created Date : 2017.08.31
      Creator : J.S HONG
   Decription : 비용처리 등록/취소 < 특이작업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.31  J.S HONG : Initial Created.
  2023.03.29    정재홍 : [E20230317-000067] - GMES 시스템 비용처리 대상 등록/취소 화면 Lot ID 다중입력 기능 개선 요청 건
  2023.03.29    정재홍 : [E20230317-000073] - GMES 시스템 비용처리 대상 완료등록 관련 개선 요청 건

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
    public partial class COM001_106 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        List<string> LotList = new List<string>();

        public COM001_106()
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

            #region [Combo]비용처리 대상 등록  

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

            string[] sFilter1 = { "CHARGE_PROD_LOT" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);

            string[] sFilter2 = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboCostSloc, CommonCombo.ComboStatus.SELECT, sCase: "COST_SLOC", sFilter: sFilter2);



            #endregion

            #region [Combo]비용처리 완료 등록

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
            String[] sFilterReason = { "","PRDT_REQ_TYPE_CODE" };
            _combo.SetCombo(cboPrdtReqType, CommonCombo.ComboStatus.ALL, sFilter: sFilterReason, sCase: "COMMCODES");

            #endregion

            #region [Combo]비용처리 대상 등록 취소

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

            #region [Combo]비용처리 대상 이력 조회

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
            String[] sFilterPrdtReqTypeHis = { "", "PRDT_REQ_TYPE_CODE" };
            _combo.SetCombo(cboPrdtReqTypeHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterPrdtReqTypeHis, sCase: "COMMCODES");
            #endregion

            #region [Combo]FCS 비용 등록  
            
            SetComboBox(cboShop);
            SetComboBox(cboStockLocation);
            SetComboBox(cboCostFCS);
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
                    GetLotList_Register();
                }
            }

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveRegister);
            listAuth.Add(btnSaveComplete);
            listAuth.Add(btnSaveCancel);
            listAuth.Add(btnSaveHistory);
            listAuth.Add(btnCancelFCS);
            listAuth.Add(btnSaveRegisterFCS);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회&저장]비용처리 대상 등록
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
                //비용처리 대상 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4086"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        #region [조회&저장]비용처리 완료 등록 
        private void btnSearchComplete_Click(object sender, RoutedEventArgs e)
        {
            chkAllComplete.IsChecked = false;
            GetLotList_Complete();
        }

        //비용처리 완료 등록
        private void btnSaveComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Complete())
                {
                    return;
                }
                //비용처리 완료 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4087"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        #region [조회&저장]비용처리 대상 등록 취소
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
                //비용처리 등록 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4088"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        #region [조회&저장]비용처리 대상 이력 조회
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
                //비용처리 대상등록 완료 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4133"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        #region [조회&저장]FCS 비용 등록
        private void btnSearchFCS_Click(object sender, RoutedEventArgs e)
        {
            chkAll.IsChecked = false;
            //GetLotList_Register();
            GetLotList_RegisterFCS();
        }

        private void btnSaveRegisterFCS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_FCSRegister())
                {
                    return;
                }
                //비용처리 대상 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4977"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                FCS_Register();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }            
        }

        private void btnCancelFCS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_FCSCancel())
                {
                    return;
                }
                //FCS 등록취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4978"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                FCS_Cancel();
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

        #region 비용처리 대상 등록
        private void CheckBoxRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (TabFCS.IsSelected)
                {                    
                }
                else
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                    object objRowIdx = dgListRegister.Rows[idx].DataItem;

                    AddToSelectRegister(objRowIdx);
                }

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

        #region 비용처리 등록 완료
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

        #region 비용처리 대상 등록 취소
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

        #region 비용처리 대상 이력 조회
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
                    if (!Util.NVC(DataTableConverter.GetValue(objRowIdx, "PRDT_REQ_PRCS_STAT_CODE")).Equals("COMPLETE"))
                    {
                        Util.MessageValidation("SFU4138", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));//해당LOTID[%1]는 완료 등록된 LOTID가 아닙니다.
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }

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
                GetUserWindow(txtPrscUser.Text);
            }
        }

        private void txtPrscUserFCS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow(txtPrscUserFCS.Text);
            }
        }

        private void btnPrscUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow(txtPrscUser.Text);
        }

        private void btnPrscUserFCS_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow(txtPrscUserFCS.Text);
        }

        private void GetUserWindow(string UserName)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = UserName;
                //Parameters[0] = txtPrscUser.Text;
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
                if(TabFCS.IsSelected)
                {
                    txtPrscUserFCS.Text = wndPerson.USERNAME;
                    txtPrscUserFCS.Tag = wndPerson.USERID;

                    txtDeptFCS.Text = wndPerson.DEPTNAME;
                    txtDeptFCS.Tag = wndPerson.DEPTID;
                }
                else
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
        }

        /// <summary>
        /// 등록자 클리어 시 등록부서 클리어
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrscUserFCS_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPrscUserFCS.Text))
            {
                txtDeptFCS.Text = string.Empty;
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
                    Decimal dLANE_QTY = Util.NVC_Decimal(DataTableConverter.GetValue(dgSelectRegister.Rows[dgSelectRegister.SelectedIndex].DataItem, "LANE_QTY"));

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

        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region 비용처리 대상 등록 조회

        public void GetLotList_Register(string sLotID = "",  bool bButton = true)
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
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaRegister, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                string sEqsgID = Util.ConvertEmptyToNull((string)cboEqsgRegister.SelectedValue);
                string sProcID = Util.ConvertEmptyToNull((string)cboProcRegister.SelectedValue);
                string sProdID = Util.ConvertEmptyToNull(txtProdRegister.Text.Trim());
                string sModlID = Util.ConvertEmptyToNull(txtModlRegister.Text.Trim());
                string sPrjtName = Util.ConvertEmptyToNull(txtPrjtRegister.Text.Trim());
                // string sLotID = Util.ConvertEmptyToNull(txtLotidRegister.Text.Trim());
                string sCtnrID = Util.ConvertEmptyToNull(txtCtnridRegister.Text.Trim());
                string sTrayID = Util.ConvertEmptyToNull(txtTrayRegister.Text.Trim());

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
                if (!string.IsNullOrEmpty(sTrayID))
                    dr["CSTID"] = sTrayID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_WIP", "INDATA", "OUTDATA", dtRqst);

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
                        Util.GridSetData(dgListRegister, dtSource, FrameOperation, false);

                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, null, false);

                        txtLotidRegister.Text = string.Empty;
                        txtLotidRegister.Focus();
                    }
                    else
                    {
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);

                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, null, false);

                        txtLotidRegister.Text = string.Empty;
                        txtLotidRegister.Focus();
                    }
                }
                else
                {
                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    DataTable dtSelect = GetDtRegister();
                    Util.GridSetData(dgSelectRegister, dtSelect, null, false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 완료 등록 조회
        public void GetLotList_Complete()
        {
            try
            {
                DataTable dtRqst = new DataTable();
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
                dr["AREAID"] = Util.GetCondition(cboAreaComplete, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEqsgComplete.SelectedValue);
                dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcComplete.SelectedValue);
                dr["PRDT_REQ_TYPE_CODE"] = (string)cboPrdtReqType.SelectedValue == "SELECT" ? null :(string)cboPrdtReqType.SelectedValue;
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdComplete.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlComplete.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtComplete.Text.Trim());
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidComplete.Text.Trim());
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_CREATE", "INDATA", "OUTDATA", dtRqst);

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
                Util.GridSetData(dgSelectComplete, dtSelect, null, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 등록 취소 조회
        private void GetLotList_Cancel()
        {
            try
            {
                const string sPRDT_REQ_TYPE_CODE = "STOCK";
                DataTable dtRqst = new DataTable();
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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_CREATE", "INDATA", "OUTDATA", dtRqst);

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
                Util.GridSetData(dgSelectCancel, dtSelect, null, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 이력 조회 조회
        public void GetLotList_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_REQ_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.ConvertEmptyToNull((string)cboEqsgHistory.SelectedValue);
                dr["PROCID"] = Util.ConvertEmptyToNull((string)cboProcHistory.SelectedValue);
                dr["PRDT_REQ_TYPE_CODE"] = Util.ConvertEmptyToNull((string)cboPrdtReqTypeHistory.SelectedValue);
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdHistory.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlHistory.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtHistory.Text.Trim());
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidHistory.Text.Trim());
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_ALL", "INDATA", "OUTDATA", dtRqst);

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
                Util.GridSetData(dgSelectHistory, dtSelect, null, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region FCS 비용 등록 조회
        public void GetLotList_RegisterFCS(bool bButton = true)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                string sProdID = Util.ConvertEmptyToNull(txtProdFCS.Text.Trim());
                string sLotID = Util.ConvertEmptyToNull(txtTrayFCS.Text.Trim());

                if (!string.IsNullOrEmpty(sProdID))
                    dr["PRODID"] = sProdID;
                if (!string.IsNullOrEmpty(sLotID))
                    dr["LOTID"] = sLotID;

                dr["FROMDATE"] = dtpDateFromFCS.SelectedDateTime.ToShortDateString();
                dr["TODATE"] = dtpDateToFCS.SelectedDateTime.ToShortDateString();
                dtRqst.Rows.Add(dr);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FCS_PRDT_REQ_HIST_ALL", "INDATA", "OUTDATA", dtRqst);
                //dtRslt.Columns.Add("CHK", typeof(bool));

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListFCS, dtRslt, FrameOperation, false);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    Util.MessageConfirm("SFU1905", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTrayFCS.Focus();
                            txtTrayFCS.Text = string.Empty;
                            txtProdFCS.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListFCS.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListFCS);
                        Util.GridSetData(dgListFCS, dtSource, FrameOperation, true);
                        
                        txtLotidRegister.Text = string.Empty;
                        txtLotidRegister.Focus();
                    }
                    else
                    {
                        Util.gridClear(dgListFCS);
                        Util.GridSetData(dgListFCS, dtRslt, FrameOperation, true);                        
                        txtLotidRegister.Text = string.Empty;
                        txtLotidRegister.Focus();
                    }
                }
                else
                {
                    Util.GridSetData(dgListFCS, dtRslt, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                chkAllCompleteFCS.IsChecked = false;
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
            dtSelect.Columns.Add("LOTYNAME", typeof(string));
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
            dtSelect.Columns.Add("COST_PRCS_NOTE", typeof(string));
            dtSelect.Columns.Add("LOTYNAME", typeof(string));

            return dtSelect;
        }
        #endregion
        #endregion

        #region [선택목록 등록]

        #region 비용처리 대상 등록
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

                for (int i = 0; i < dtSelectRegister.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectRegister.Rows[i]["LOTID"]);
                    row["ACTQTY"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY"]);
                    row["ACTQTY2"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY2"]);
                    row["WIPNOTE"] = txtNoteRegister.Text;
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

                //비용처리 대상 등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STOCK", "INDATA,INLOT,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Register();

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

        #region 비용처리 완료 등록
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

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["PRDT_REQ_PRCS_STAT_CODE"] = "COMPLETE";
                row["NOTE"] = txtNoteComplete.Text;
                row["BIZ_WF_PRCS_END_DATE"] = ldpCmplDateComplete.SelectedDateTime.ToString("yyyyMMdd");
                row["USERID"] = LoginInfo.USERID;
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

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
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

        #region 비용처리 대상 등록 취소
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

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["PRDT_REQ_PRCS_STAT_CODE"] = "CANCEL";
                row["NOTE"] = txtNoteCancel.Text;
                row["USERID"] = LoginInfo.USERID;
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

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
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

        #region 비용처리 대상 이력 조회
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

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["PRDT_REQ_PRCS_STAT_CODE"] = "CREATE";
                row["NOTE"] = txtNoteHistory.Text;
                row["USERID"] = LoginInfo.USERID;
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

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
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

        #region FCS 비용등록
        /// <summary>
        /// FCS비용 등록 처리
        /// </summary>
        private void FCS_Register()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("FROM_SLOC_ID", typeof(string));
                inDataTable.Columns.Add("SLOC_ID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("RESNQTY", typeof(decimal));
                inDataTable.Columns.Add("BIZ_WF_DEPTID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_USERID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));                
                inDataTable.Columns.Add("COST_PRCS_SLOC_ID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["IFMODE"] = "OFF";
                row["LOTID"] = txtRegTrayFCS.Text;
                row["SHOPID"] = cboShop.SelectedValue.ToString();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["FROM_SLOC_ID"] = cboStockLocation.SelectedValue;
                row["SLOC_ID"] = cboStockLocation.SelectedValue;
                row["PRODID"] = txtRegProdFCS.Text;
                row["RESNQTY"] = txtQty.Value;
                row["BIZ_WF_DEPTID"] = txtDeptFCS.Tag;
                row["BIZ_WF_PRCS_USERID"] = txtPrscUserFCS.Tag; 
                row["BIZ_WF_PRCS_SCHD_CMPL_DATE"] = ldpCmplDateRegister.SelectedDateTime.ToString("yyyyMMdd");
                row["COST_PRCS_SLOC_ID"] = cboCostFCS.SelectedValue;
                row["NOTE"] = txtFCSNote.Text;
                inDataTable.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;

                //FCS 비용등록 처리
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_REG_COST_PRDT_REQ_FCS", "INDATA", null, inDataTable);

                Util.MessageInfo("SFU3532");//저장되었습니다.

                GetLotList_RegisterFCS();

                cboShop.SelectedIndex = 0;
                cboStockLocation.SelectedIndex = 0;
                cboCostFCS.SelectedIndex = 0;
                txtRegTrayFCS.Text = string.Empty;
                txtRegTrayFCS.Tag = string.Empty;
                txtRegProdFCS.Text = string.Empty;
                txtRegProdFCS.Tag = string.Empty;
                txtQty.Value = 0.0d;
                txtDeptFCS.Text = string.Empty;
                txtDeptFCS.Tag = string.Empty;
                txtPrscUserFCS.Text = string.Empty;
                txtPrscUserFCS.Tag = string.Empty;
                ldpCmplDateRegisterFCS.SelectedDateTime = DateTime.Now;
                txtFCSNote.Text = string.Empty;
                txtFCSNote.Tag = string.Empty;                
                chkAll.IsChecked = false;
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_FCS", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                chkAllCompleteFCS.IsChecked = false;
            }
        }
        #endregion
        #region FCS 비용등록 취소
        private void FCS_Cancel()
        {
            try
            {
                //INDATA
                DataTable inDataTable = new DataTable();                
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("HIST_SEQNO", typeof(Int64));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable dtSelectCancel = DataTableConverter.Convert(dgListFCS.ItemsSource);
                for (int i = 0; i < dtSelectCancel.Rows.Count; i++)
                {
                    if (dtSelectCancel.Rows[i]["CHK"].ToString() == "True")
                    { 
                        DataRow row = inDataTable.NewRow();
                        row["LOTID"] = Convert.ToString(dtSelectCancel.Rows[i]["LOTID"]);
                        row["HIST_SEQNO"] = Convert.ToInt64(dtSelectCancel.Rows[i]["HIST_SEQNO"]);
                        row["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(row);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 등록취소 
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_REG_COST_PRDT_REQ_FCS_CANCEL", "INDATA", null, inDataTable);

                Util.MessageInfo("SFU3532");//저장되었습니다.

                GetLotList_RegisterFCS();
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_STOCK", ex.Message, ex.ToString());
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                chkAllCompleteFCS.IsChecked = false;
            }
        }
        #endregion
        #endregion

        #region [Validation]

        #region 비용처리 대상 등록
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

            if (cboResnCode.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1593"); //사유를 선택하세요.
                return false;
            }

            if (cboCostSloc.SelectedIndex == 0)
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

        #region 비용처리 완료 등록
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

        #region 비용처리 등록 취소
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

        #region 비용처리 대상 이력조회
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

        #region FCS 비용 등록
        private bool Validation_FCSRegister()
        {
            if (cboShop.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU3561"); //공장을 선택하세요
                return false;
            }

            if (String.IsNullOrEmpty(txtRegTrayFCS.Text))
            {
                Util.MessageValidation("SFU4975"); //TRAY ID를 입력하세요
                return false;
            }

            if (String.IsNullOrEmpty(txtRegProdFCS.Text))
            {
                Util.MessageValidation("SFU2949"); //제품ID를 입력하세요
                return false;
            }

            if (txtQty.Value <= 0)
            {
                Util.MessageValidation("SFU1684");  //수량을 입력하세요
                return false;
            }

            if (cboStockLocation.SelectedIndex < 0 || cboStockLocation.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU4136"); //저장위치를 선택해 주세요
                return false;
            }

            if (cboCostFCS.SelectedIndex < 0 || cboCostFCS.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU4131"); //비용저장위치를 선택해 주세요.
                return false;
            }

            if (String.IsNullOrEmpty(txtPrscUserFCS.Text))
            {
                Util.MessageValidation("SFU4106"); //BizWF 처리 담당자를 선택해 주세요.
                return false;
            }

            return true;
        }
        #endregion

        #region FCS 비용 등록 취소
        private bool Validation_FCSCancel()
        {
            bool Cancel = false;            
            foreach (var row in dgListFCS.ItemsSource)
            {
                string value = Util.NVC(DataTableConverter.GetValue(row, "CHK"));
                if (value.Equals("True"))
                {
                    Cancel = true;
                    break;
                }
            }            

            if (!Cancel)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }            

            return true;
        }
        #endregion

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
                    GetLotList_Register(sLotid, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotidRegister_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringLot = "";

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    Util.gridClear(dgListRegister);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;
                        }

                        sPasteStringLot += sPasteStrings[i] + ",";

                        System.Windows.Forms.Application.DoEvents();
                    }

                    GetLotList_Register(sPasteStringLot, false);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLotidRegister.Text = "";
                    txtLotidRegister.Focus();
                }

                e.Handled = true;
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
            if (TabFCS.IsSelected)
            {
                for (int inx = 0; inx < dgListFCS.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListFCS.Rows[inx].DataItem, "CHK", true);                    
                }
            }
            else
            {
                Util.gridClear(dgSelectRegister);

                if ((bool)chkAll.IsChecked)
                {

                    // 동일한 PJT 만 전체 선택 가능하도록
                    if (dgListRegister.GetRowCount() > 0)
                    {
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
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (TabFCS.IsSelected)
            {
                for (int inx = 0; inx < dgListFCS.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListFCS.Rows[inx].DataItem, "CHK", false);
                }
            }
            else
            {
                Util.gridClear(dgSelectRegister);

                for (int inx = 0; inx < dgListRegister.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListRegister.Rows[inx].DataItem, "CHK", false);
                }
            }
        }

        private void AddToSelectRegister(object objRowIdx)
        {
            try
            {
                if (objRowIdx == null) return;

                if (TabRegister.IsSelected)
                {
                    // Validation
                    if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
                    {
                        if (DataTableConverter.Convert(dgSelectRegister.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "'").Length == 1)
                        {
                            //LOT이 이미 선택되었습니다.
                            Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                            DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                            return;
                        }
                        if (DataTableConverter.GetValue(objRowIdx, "WIPHOLD").Equals("Y"))
                        {
                            //%1이 HOLD상태 입니다.
                            Util.MessageValidation("SFU1761", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                            DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                            return;
                        }
                    }

                    if (DataTableConverter.GetValue(objRowIdx, "CHK").IsTrue())
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
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        // chkAllCompleteFCS
        void checkAllComplete_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSelectComplete);

            for (int inx = 0; inx < dgListComplete.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListComplete.Rows[inx + 2].DataItem, "CHK", false);
            }
        }

        private void chkAllCompleteFCS_Checked(object sender, RoutedEventArgs e)
        {
            for (int inx = 0; inx < dgListFCS.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListFCS.Rows[inx + 2].DataItem, "CHK", true);                
            }
        }

        void chkAllCompleteFCS_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int inx = 0; inx < dgListFCS.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListFCS.Rows[inx + 2].DataItem, "CHK", false);
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

        void SetComboBox(C1ComboBox cbo)
        {
            switch (cbo.Name)
            {
                case "cboShop":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_AUTH_SHOP_CBO", cbo, new string[] { "SYSTEM_ID", "LANGID", "USERID", "USE_FLAG" }, new string[] { LoginInfo.SYSID, LoginInfo.LANGID, LoginInfo.USERID, "Y" }, CommonCombo.ComboStatus.NONE, "CBO_CODE", "CBO_NAME");                    
                    break;

                case "cboStockLocation":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_SHOP", cbo, new string[] { "SHOPID", "AREAID" }, new string[] { cboShop.SelectedValue as string, LoginInfo.CFG_AREA_ID }, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME");                    
                    break;

                case "cboCostFCS":
                    CommonCombo.CommonBaseCombo("DA_PRD_SEL_COST_SLOC_CBO", cbo, new string[] { "SHOPID" }, new string[] { cboShop.SelectedValue as string }, CommonCombo.ComboStatus.SELECT, "SLOC_ID", "SLOC_NAME");
                    break;
            }
        }

        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetComboBox(cboStockLocation);
            SetComboBox(cboCostFCS);
        }

        
    }
}
