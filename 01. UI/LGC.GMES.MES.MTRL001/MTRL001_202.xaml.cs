/*************************************************************************************
 Created Date : 2024.03.12
      Creator : 이병윤
   Decription : 원자재관리 - 자재 Loss 관리
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.12  이병윤 : Initial Created.
  2024.05.08  유재홍 : 전극 Back Coating 컬럼제거, 조립 현재 자재LOT 컬럼 추가
  2024.08.09  유재홍 : dgHISTORY 컬럼 전체 수정
  2024.08.09  유재홍 : list에서 믹서의 경우에만 투입요청서 컬럼 보이도록 수정
  2025.06.30  이주원 : CAT_UP_0384[E20240826-000253]-자재 LOSS 관리 화면 개선(공정 특성 반영) 요청 수정 적용
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;

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


namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_202 : UserControl, IWorkArea
    {
        #region Declaration & Constructor  

        private Util _Util = new Util();
        private int rowIndex = -1;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_202()
        {
            // System.Windows.MessageBox.Show(LoginInfo.CFG_AREA_ID.Substring(0, 1));
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeGrid()
        {
            Util.gridClear(dgLoss);
            Util.gridClear(dgHistory);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //동
                C1ComboBox[] cboAreaHisChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaHisChild, sCase: "AREA");

                //공정
                C1ComboBox[] cboProcessParent = { cboArea };
                C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
                _combo.SetCombo(cboMatlLossProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboMatlLossProcess };
                C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "cboMatlLossEquipmentSegment");

                //설비
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboMatlLossProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "cboMatlLossEquipment");

                dtpDateFrom.SelectedDateTime = System.DateTime.Now;
                dtpDateTo.SelectedDateTime = System.DateTime.Now;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControl()
        {
            try
            {
                // 전극 & 조립 : LoginInfo.CFG_PROC_ID.Equals(Process.COATING)해당공정을 선택하지 않고 화면접속시 방어                
                if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("E") || LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("A"))
                {
                    //System.Windows.MessageBox.Show(cboMatlLossProcess.SelectedValue.ToString());
                    if (cboMatlLossProcess.SelectedValue.ToString().Equals("E1000")) //Mixing
                    {
                        (dgList.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible; //투입 요청서 ID
                        (dgList.Columns["DSP_LOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;  // LOTID
                        (dgList.Columns["DSP_LOTTYPE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;  // LOTTYPE
                        (dgList.Columns["DSP_LOTYNAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed; //LOTTYPE명
                        (dgList.Columns["DSP_PRODID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed; //제품ID
                        (dgList.Columns["DSP_MODLID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;  //모델ID
                        (dgList.Columns["PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;


                    }
                    else
                    {
                        (dgList.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed; //투입 요청서 ID
                        (dgList.Columns["DSP_LOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;  // LOTID
                        (dgList.Columns["DSP_LOTTYPE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;  // LOTTYPE
                        (dgList.Columns["DSP_LOTYNAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible; //LOTTYPE명
                        (dgList.Columns["DSP_PRODID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible; //제품ID
                        (dgList.Columns["DSP_MODLID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;  //모델ID 

                        if (cboMatlLossProcess.SelectedValue.ToString().Equals("E2000") || cboMatlLossProcess.SelectedValue.ToString().Equals("A2000")) //Coating & Winding
                        {
                            (dgList.Columns["PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                        }
                        else
                        {
                            (dgList.Columns["PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                        }

                    }

                    if (cboMatlLossProcess.SelectedValue.ToString().Equals("E2000")) //Coating
                    {
                        (dgList.Columns["DSP_E2000_ORIG_WIPQTY_ST"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible; //장착수량2
                        (dgList.Columns["DSP_E2000_ORIG_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible; //사용수량2
                        (dgList.Columns["DSP_E2000_ORIG_LOSS_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible; //LOSS수량2
                        (dgList.Columns["DSP_E2000_ORIG_WIPQTY_ED"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible; //잔량수량2
                        (dgList.Columns["DSP_E2000_ORIG_UNIT_CODE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;  //단위2
                        (dgList.Columns["DSP_E2000_TCK"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;  //두깨
                        (dgList.Columns["DSP_E2000_WIDTH"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;  // 폭
                        (dgList.Columns["DSP_E2000_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;  // 변환개수
                        (dgList.Columns["DSP_E2000_TOP_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible; //Top Offset                        
                        (dgList.Columns["DSP_E2000_ORIG_MTRL_ISS_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;  //출고수량2

                    }
                    else
                    {
                        (dgList.Columns["DSP_E2000_ORIG_WIPQTY_ST"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed; //장착수량2
                        (dgList.Columns["DSP_E2000_ORIG_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed; //사용수량2
                        (dgList.Columns["DSP_E2000_ORIG_LOSS_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed; //LOSS수량2
                        (dgList.Columns["DSP_E2000_ORIG_WIPQTY_ED"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed; //잔량수량2
                        (dgList.Columns["DSP_E2000_ORIG_UNIT_CODE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;  //단위2
                        (dgList.Columns["DSP_E2000_TCK"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;  //두깨
                        (dgList.Columns["DSP_E2000_WIDTH"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;  // 폭
                        (dgList.Columns["DSP_E2000_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;  // 변환개수
                        (dgList.Columns["DSP_E2000_TOP_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed; //Top Offset                        
                        (dgList.Columns["DSP_E2000_ORIG_MTRL_ISS_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;  //출고수량2

                    }

                    /* 이력그리드
                     * 위치_EQPT_PSTN_VALUE	
                     * Offset_EQPT_CURR_PSTN_VALUE	
                     * 위치 Offset_PSTN_OFFSET_VALUE	
                     * 이전 위치 Offset_PRE_EQPT_PSTN_OFFSET_VALUE	
                     * TOP Uncoating 여부_TOP_UNCOATING_FLAG	
                     * BACK Uncoating 여부_BACK_UNCOATING_FLAG	
                     * 현재 자재Lot_MLOTID	
                     * 이전 자재Lot_PRE_MLOTID
                     */
                    /*
                   (dgHistory.Columns["EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                   //(dgHistory.Columns["BACK_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;20240.05.08 유재홍
                   (dgHistory.Columns["MLOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["PRE_MLOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;

                   (dgHistory.Columns["EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["PTN_CONV_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["EQPT_INPUT_SUM_TYPE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                   //(dgHistory.Columns["LOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                   //(dgHistory.Columns["REF_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   */
                    //chkOffset.Visibility = Visibility.Visible;

                    //(dgLoss.Columns["ORIG_RESNQTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    //(dgLoss.Columns["ORIG_UNIT_CODE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;

                }
                // 소형조립 : LoginInfo.CFG_PROC_ID.Equals(Process.WINDING) 해당공정을 선택하지 않고 화면접속시 방어
                else if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("M"))
                {
                    // 페턴비율(PTN_CONV_RATE)
                    (dgList.Columns["PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;

                    (dgList.Columns["DSP_E2000_ORIG_WIPQTY_ST"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["DSP_E2000_ORIG_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["ORIG_CNFM_LOSS_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["ORIG_WIPQTY_ED"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["ORIG_UNIT_CODE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["TCK"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["WIDTH"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["TOP_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["BACK_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgList.Columns["DSP_E2000_ORIG_MTRL_ISS_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;


                    /*
                     * 설비불량명[EQPT_DFCT_NAME]
                     * 이전 수량[PRE_EQPT_QTY]
                     * 패턴 수량[PTN_CONV_QTY]
                     * 패턴 비율[PTN_CONV_RATE]
                     * 설비 투입 합계 여부[EQPT_INPUT_SUM_TYPE]
                     * 생산 LOTID[LOTID]
                     * 참조 누적 수량[REF_EQPT_QTY]
                     */
                    /*
                   (dgHistory.Columns["EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["PTN_CONV_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                   (dgHistory.Columns["EQPT_INPUT_SUM_TYPE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;


                   //(dgHistory.Columns["LOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                   //(dgHistory.Columns["REF_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;

                   (dgHistory.Columns["EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["BACK_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                   (dgHistory.Columns["MLOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;//2024.05.08 유재홍
                   (dgHistory.Columns["PRE_MLOTID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                   */

                    chkOffset.Visibility = Visibility.Collapsed;


                    // 수량2[ORIG_RESNQTY], 단위2[ORIG_UNIT_CODE]
                    (dgLoss.Columns["ORIG_RESNQTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgLoss.Columns["ORIG_UNIT_CODE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnLoss);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();
            InitCombo();
            cboMatlLossProcess.SelectedIndex = 0;

            //SearchMtrlLoss("");
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchMtrlLoss("lot");
            }
        }

        /// <summary>
        /// 설비투입위치 콤보박스 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            // 설비 투입 위치
            string eqpt = "";
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                eqpt = string.Empty;
            }
            else
            {
                eqpt = cboEquipment.SelectedValue.ToString();
            }

            String[] sFilter2 = { eqpt, "PROD" };
            CommonCombo combo = new CommonCombo();
            combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_ECLD_CBO");
        }

        private void chkOffset_Click(object sender, RoutedEventArgs e)
        {
            SearchHistory();
        }

        private void btnLoss_Click(object sender, RoutedEventArgs e)
        {
            // loss정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1355", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveLoss();
                }
            });
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
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

                        // row 색 바꾸기
                        dgList.SelectedIndex = idx;

                        InitializeGrid();

                        // 이력 조회
                        SearchHistory();
                        // Loss 조회
                        SearchLoss();
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

            if (String.IsNullOrEmpty(txtMLotId.Text))
                SearchMtrlLoss("");
            else
                SearchMtrlLoss("lot");
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Column.Name.Contains("RE_CALC"))
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        Button Buttn = e.Cell.Presenter.Content as Button;

                        if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "UNMNT_DTTM"))))
                            Buttn.IsEnabled = false;
                        else
                            Buttn.IsEnabled = true;
                    }
                }));
            }
        }

        private void dgLoss_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("RESNQTY"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                }

                if ((e.Cell.Column.Name.Equals("ORIG_RESNQTY") || e.Cell.Column.Name.Equals("EQPT_RESNQTY")) && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

            }));

        }

        private void btnReCalc_Click(object sender, RoutedEventArgs e)
        {
            Button btnReCalc = sender as Button;
            if (btnReCalc != null)
            {
                DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((btnReCalc as Button).Parent)).Row.Index;

                string sReCalcYn = string.Empty;
                string sMLotId = string.Empty;
                string sSeqno = string.Empty;
                string sCalcFlag = string.Empty;
                string sUnmmtDT = string.Empty;

                sReCalcYn = Util.NVC(dataRow.Row["DSP_RE_CALC_YN"]);
                sMLotId = Util.NVC(dataRow.Row["MLOTID"]);
                sSeqno = Util.NVC(dataRow.Row["INPUTSEQNO"]);
                sCalcFlag = Util.NVC(dataRow.Row["CALC_FLAG"]);
                sUnmmtDT = Util.NVC(dataRow.Row["UNMNT_DTTM"]);

                if (string.Equals(sReCalcYn, "N") || String.IsNullOrEmpty(sUnmmtDT))
                {
                    Util.MessageInfo("SFU2089", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });

                    Logger.Instance.WriteLine("MTRL001_202_N", "sUpdYn: " + sReCalcYn, LogCategory.FRAME);
                    return;
                }
                MTRL001_202_RECALC popReCalc = new MTRL001_202_RECALC { FrameOperation = FrameOperation };
                object[] parameters = new object[4];
                parameters[0] = sMLotId;
                parameters[1] = sSeqno;
                parameters[2] = sCalcFlag;

                C1WindowExtension.SetParameters(popReCalc, parameters);
                popReCalc.Closed += popReCalc_Closed;
                Dispatcher.BeginInvoke(new Action(() => popReCalc.ShowModal()));
            }
        }

        private void popReCalc_Closed(object sender, EventArgs e)
        {
            MTRL001_202_RECALC popup = sender as MTRL001_202_RECALC;
            if (popup != null)
            {
                DataTableConverter.SetValue(dgList.Rows[rowIndex].DataItem, "CALC_FLAG", Util.NVC(popup.txtSave.Text));
                rowIndex = -1;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 자재 Loss 관리 조회
        /// </summary>
        private void SearchMtrlLoss(string gb)
        {
            try
            {
                //System.Windows.MessageBox.Show("SearchMtrlLoss Start");
                // Grid Clear
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STDT", typeof(string));
                inTable.Columns.Add("EDDT", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("MLOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                if (gb.Equals("lot"))
                {   // 스켄된 lot 만 별도로 조회처리
                    newRow["MLOTID"] = txtMLotId.Text.ToString();
                    newRow["PROCID"] = cboMatlLossProcess.SelectedValue.ToString();//procid필수화
                }
                else
                {
                    if (cboMatlLossProcess.SelectedValue == null || cboMatlLossProcess.SelectedValue.ToString().Equals("SELECT"))
                    {
                        Util.gridClear(dgList);
                        return;
                    }
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["AREAID"] = cboArea.SelectedValue.ToString();
                    if (!Util.NVC(cboMatlLossProcess.SelectedValue.ToString()).Equals(""))
                    {
                        newRow["PROCID"] = cboMatlLossProcess.SelectedValue.ToString();
                    }

                    if (!Util.NVC(cboEquipmentSegment.SelectedValue.ToString()).Equals(""))
                    {
                        newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    }

                    if (!Util.NVC(cboEquipment.SelectedValue.ToString()).Equals(""))
                    {
                        newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    }

                    if (!Util.NVC(cboPancakeMountPstnID.SelectedValue.ToString()).Equals(""))
                    {
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(cboPancakeMountPstnID.SelectedValue.ToString());
                    }

                    newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                }

                inTable.Rows.Add(newRow);



                ShowLoadingIndicator();

                SetControl();
                //System.Windows.MessageBox.Show("DA_BAS_SEL_MTRL_LOSS Bf Start");
                new ClientProxy().ExecuteService("DA_BAS_SEL_MTRL_LOSS", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        //System.Windows.MessageBox.Show("DA_BAS_SEL_MTRL_LOSS Start");
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);

                        // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
                        for (int i = 0; i < dgList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                dgList.SelectedIndex = i;
                                break;
                            }
                        }

                        //  ActHistoy 조회
                        SearchHistory();

                        // Loss 조회
                        SearchLoss();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ActHistoy 조회
        /// </summary>
        private void SearchHistory()
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgList, "CHK");

                if (rowIndex < 0) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MLOTID", typeof(string));
                inTable.Columns.Add("INPUTSEQNO", typeof(string));
                inTable.Columns.Add("SORT_EQPT_PSTN_OFFSET_VALUE_FLAG", typeof(string));

                string bizName = "DA_BAS_SEL_MTRL_LOSS_ACT_HIST_FOR_UI";

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "MLOTID"));
                newRow["INPUTSEQNO"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "INPUTSEQNO"));
                if (chkOffset.IsChecked == true)
                {
                    newRow["SORT_EQPT_PSTN_OFFSET_VALUE_FLAG"] = "Y";
                }
                else
                {
                    newRow["SORT_EQPT_PSTN_OFFSET_VALUE_FLAG"] = "N";
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                String PROCID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PROCID"));
                if (PROCID.Equals("E1000"))
                {
                    (dgHistory.Columns["DSP_E1000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_SUM_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;

                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PSTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_A2000_EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PTN_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_A7000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                }
                else if (PROCID.Equals("E2000"))
                {
                    (dgHistory.Columns["DSP_E1000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_SUM_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_PSTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;

                    (dgHistory.Columns["DSP_A2000_EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PTN_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_A7000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals("A2000"))
                {
                    (dgHistory.Columns["DSP_E1000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_SUM_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PSTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_A2000_EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A2000_PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A2000_PTN_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A2000_PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;

                    (dgHistory.Columns["DSP_A7000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A7000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                }

                else if (PROCID.Equals("A7000") || PROCID.Equals("A7400") || PROCID.Equals("A8000") || PROCID.Equals("A8400"))
                {
                    (dgHistory.Columns["DSP_E1000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_SUM_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_E2000_PSTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_A2000_EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PTN_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;
                    (dgHistory.Columns["DSP_A2000_PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Collapsed;

                    (dgHistory.Columns["DSP_A7000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A7000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A7000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A7000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                }
                else
                {
                    (dgHistory.Columns["DSP_E1000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_SUM_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E1000_INPUT_REQ_ID"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;

                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_PRE_EQPT_PSTN_OFFSET_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_TOP_UNCOATING_FLAG"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_E2000_PSTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;

                    (dgHistory.Columns["DSP_A2000_EQPT_DFCT_NAME"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A2000_PRE_EQPT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A2000_PTN_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A2000_PTN_CONV_RATE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;

                    (dgHistory.Columns["DSP_A7000_FRST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A7000_INPUT_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A7000_LAST_QTY"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                    (dgHistory.Columns["DSP_A7000_EQPT_PSTN_VALUE"] as C1.WPF.DataGrid.DataGridNumericColumn).Visibility = Visibility.Visible;
                }








                new ClientProxy().ExecuteService(bizName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Loss 조회
        /// </summary>
        private void SearchLoss()
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgList, "CHK");

                if (rowIndex < 0) return;

                // 시작시간이 없을 경우 그리드 및 버튼 감추기
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WIPDTTM_ST_YN").ToString()).Equals("Y"))
                {
                    btnLoss.Visibility = Visibility.Collapsed;
                    Util.gridClear(dgLoss);
                    return;
                }


                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MLOTID", typeof(string));
                inTable.Columns.Add("INPUTSEQNO", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("CLSS_CODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "MLOTID"));
                newRow["INPUTSEQNO"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "INPUTSEQNO"));
                newRow["ACTID"] = "LOSS_MTRL_LOT";
                newRow["CLSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "MTRL_CLSS_CODE"));
                newRow["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "PROCID"));
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_BAS_SEL_MTRL_LOSS_RSN_CLCT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgLoss, bizResult, FrameOperation, true);
                        // 종료시간이 없을 경우 버튼 감추기
                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "WIPDTTM_ED_YN").ToString()).Equals("Y"))
                        {
                            btnLoss.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            btnLoss.Visibility = Visibility.Visible;
                        }
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Loss 저장
        /// </summary>
        private void SaveLoss()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inMlot = inDataSet.Tables.Add("INMLOT");
                inMlot.Columns.Add("MLOTID", typeof(string));
                inMlot.Columns.Add("INPUTSEQNO", typeof(int));
                inMlot.Columns.Add("USERID", typeof(string));

                DataTable inLoss = inDataSet.Tables.Add("INRESN");
                inLoss.Columns.Add("RESNCODE", typeof(string));
                inLoss.Columns.Add("RESNQTY", typeof(Decimal));

                /////////////////////////////////////////////////////////////////
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgList, "CHK");

                DataRow newRow = inMlot.NewRow();
                newRow["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "MLOTID").ToString());
                newRow["INPUTSEQNO"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgList.Rows[rowIndex].DataItem, "INPUTSEQNO").ToString());
                newRow["USERID"] = LoginInfo.USERID;
                inMlot.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgLoss.Rows)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNCODE")).Equals(""))
                    {
                        newRow = inLoss.NewRow();
                        newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNCODE"));
                        newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "RESNQTY"));
                        inLoss.Rows.Add(newRow);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MTRL_LOSS_RSN_CLCT", "INMLOT,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 저장되었습니다.
                        Util.MessageInfo("SFU1270");

                        // 재조회
                        SearchLoss();
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

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {

            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택해주세요
                Util.MessageValidation("SFU1499");
                return false;
            }
            if (cboMatlLossProcess.SelectedValue == null || cboMatlLossProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을 선택해주세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            //if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    // 라인을 선택해주세요
            //    Util.MessageValidation("SFU4050");
            //    return false;
            //}

            //if (cboMatlLossProcess.SelectedValue == null || cboMatlLossProcess.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    // 설비를 선택하세요
            //    Util.MessageValidation("SFU1459");
            //    return false;
            //}

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


        #endregion

        #endregion

        #endregion
    }
}
