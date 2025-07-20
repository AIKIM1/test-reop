/*************************************************************************************
 Created Date : 2017.06.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 공정진척화면의 작업지시 공통 화면(LINE 단위의 WO)
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.22  INS 김동일K : Initial Created.
  2017.08.09  신광희C : Process.WASHING 공정인 경우 -> 선택 버튼 Validation 추가, 참조공정 가져오는 부분 수정, 조회 BizRule 수정
  2017.11.11  김동일K : W/O FP 계획생성 기준코드에 따른 계획 조회되도록 변경 (기존 타라인에 생산 라인 표시 하고 설비 단위 계획인 경우 라인을 선택하면 라인단위로 보이도록 변경)
  2017.12.11  백광영  : 라미,폴딩,패키징 이전공정 재공현황 조회 팝업 추가
  2019.03.06  강민준  : 원형9,10호기 In-Line 설비로 인한 수정(타라인) InitializeCombo()
  2021.09.03  심찬보S : 오창 소형조립UI 버전추가 및 활성화(PROD_VER_CODE)
  2023.12.18  박성진  : CSR E20231025-001044 전법인 자동차조립 생산(투입)량 숨김처리
  2024.02.20  오수현  : CSR E20240112-001725  [ESNJ ESS]작업지시 저장 전에 W/O Type이 양산이 아닌 경우 interlock 할 동을 체크(공통코드로 관리)
  2024.02.21  김용군  : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
  2024.02.28  오수현  : CSR E20240112-001725  [ESNJ ESS] 작업지시 선택 누락으로 else 문 추가
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_WORKORDER_LINE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_WORKORDER_LINE : UserControl, IWorkArea
    {   
        #region Declaration & Constructor        
        private string _EqptSegment = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _CoatSideType = string.Empty;
        private string _FP_REF_PROCID = string.Empty;
        private string _Process_ErpUseYN = string.Empty;        // Workorder 사용 공정 여부.
        private string _Process_Plan_Level_Code = string.Empty; // 계획 Level 코드. (EQPT, PROC .. )
        private string _Process_Plan_Mngt_Type_Code = string.Empty; // 계획 관리 유형 (WO, MO, REF..)
        private string _OtherEqsgID_OLD = string.Empty; // 타라인 이전 선택 값.
        private string strOUT_LOT_TYPE = string.Empty;  // 언로더 기준 생성 LOT 유형 (LOT_ID / CST_ID)
        int iCntInline = 0;

        public string InlineFlag { get; set; }
        private bool _bShowEqptName = false;

        public UserControl _UCParent; //Caller
        public UcBaseElec _UCElec;
        public C1DataGrid DgWorkOrder { get; set; }

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        public string EQPTSEGMENT
        {
            get { return _EqptSegment; }
            set { _EqptSegment = value; }
        }

        public string EQPTID
        {
            get { return _EqptID; }
            set { _EqptID = value; }
        }

        public string PROCID
        {
            get { return _ProcID; }
            set { _ProcID = value; }
        }

        public string COATSIDETYPE
        {
            get { return _CoatSideType; }
            set { _CoatSideType = value; }
        }

        public string PRODID { get; set; }


        LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM wndBOM;

        LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_WIPLIST wndWIP;

        LGC.GMES.MES.CMM001.CMM_ASSY_PRDT_GPLM wndPROD;

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }
        
        public UC_WORKORDER_LINE()
        {
            InitializeComponent();

            this.Dispatcher.BeginInvoke
            (
                System.Windows.Threading.DispatcherPriority.Input, (System.Threading.ThreadStart)(() =>
                {
                    SetChangeDatePlan();
                }
            ));
        }

        private void InitializeGridColumns()
        {
            if (dgWorkOrder == null)
                return;

            /*
             * C/Roll, S/Roll, Lane수 적용 공정.
             *     C/ROLL = PLAN_QTY(S/ROLL) / LANE_QTY
             * E2000  - TOP_COATING
             * E2300  - INS_COATING
             * S2000  - SRS_COATING
             * E2500  - HALF_SLITTING
             * E3000  - ROLL_PRESSING
             * E3500  - TAPING
             * E3800  - REWINDER
             * E3900  - BACK_WINDER
             */
            if (_ProcID.Equals(Process.TOP_COATING) ||
                _ProcID.Equals(Process.INS_COATING) ||
                _ProcID.Equals(Process.SRS_COATING) ||
                _ProcID.Equals(Process.HALF_SLITTING) ||
                _ProcID.Equals(Process.ROLL_PRESSING) ||
                _ProcID.Equals(Process.TAPING) ||
                _ProcID.Equals(Process.REWINDER) ||
                _ProcID.Equals(Process.BACK_WINDER))
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                }
            }

            // 자동차조립 공정인 경우 버전 컬럼 HIDDEN.
            if (_ProcID.Equals(Process.NOTCHING) ||
                _ProcID.Equals(Process.LAMINATION) ||
                _ProcID.Equals(Process.STACKING_FOLDING) ||
                _ProcID.Equals(Process.PACKAGING)
                //_ProcID.Equals(Process.WINDING) ||
                //_ProcID.Equals(Process.ASSEMBLY) ||
                //_ProcID.Equals(Process.WASHING)
                )
            {
                if (dgWorkOrder.Columns.Contains("PROD_VER_CODE"))
                {
                    dgWorkOrder.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }
            }

            // 패키지 공정인 경우만 모델랏 정보 표시
            if (dgWorkOrder.Columns.Contains("MDLLOT_ID"))
            {
                if (_ProcID.Equals(Process.PACKAGING) || _ProcID.Equals(Process.WINDING) || _ProcID.Equals(Process.ASSEMBLY))
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                }
            }
            
            // 라미 공정일 경우 Cell Type (CLSS_NAME : 분류명) 컬럼 표시 -> 극성 컬럼 Hidden
            if (_ProcID.Equals(Process.LAMINATION))
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;

                // 남경 라미인 경우 CLSS_NAME 대신에 PRODNAME으로 표시 처리.
                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                    if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;
                    if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Visible;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
            }

            // 노칭을 제외한 조립 공정 극성 컬럼 Hidden
            if (_ProcID.Equals(Process.STACKING_FOLDING) ||
                _ProcID.Equals(Process.SRC) ||
                _ProcID.Equals(Process.STP) ||
                _ProcID.Equals(Process.SSC_BICELL) ||
                _ProcID.Equals(Process.SSC_FOLDED_BICELL) ||
                _ProcID.Equals(Process.PACKAGING) ||
                _ProcID.Equals(Process.WINDING) ||
                _ProcID.Equals(Process.ASSEMBLY) ||
                _ProcID.Equals(Process.WASHING)
                )
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
            }
            if (_ProcID.Equals(Process.NOTCHING) ||
                _ProcID.Equals(Process.VD_LMN) ||
                _ProcID.Equals(Process.LAMINATION) ||
                _ProcID.Equals(Process.STACKING_FOLDING) ||
                _ProcID.Equals(Process.ZZS) ||
                _ProcID.Equals(Process.PACKAGING) ||
                _ProcID.Equals(Process.AZS_ECUTTER) ||
                _ProcID.Equals(Process.AZS_STACKING) ||
                _ProcID.Equals(Process.CT_INSP)
                )
            {
                if (dgWorkOrder.Columns.Contains("OUTQTY")) dgWorkOrder.Columns["OUTQTY"].Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeCombo()
        {
            DataTable dtTemp = null;

            if (PROCID.Equals(Process.VD_LMN))
                dtTemp = GetEquipmentSegmentComboForVD();
            else
                dtTemp = GetEquipmentSegmentCombo();

            // Inline정보 확인
            if (PROCID.Equals("A2000")) { 
                if (iCntInline > 0)
                {
                    strOUT_LOT_TYPE = "CST_ID";
                    dtTemp = GetEquipmentSegmentComboInline();
                }
                else
                {
                    strOUT_LOT_TYPE = "LOT_ID";
                    dtTemp = GetEquipmentSegmentComboInline();
                }
            }

            if (dtTemp != null)
            {
                cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

                cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();
                cboEquipmentSegment.SelectedIndex = 0;
                if (!Util.NVC(_OtherEqsgID_OLD).Equals("") && dtTemp?.Select("CBO_CODE = '" + _OtherEqsgID_OLD + "'").Length > 0)
                    cboEquipmentSegment.SelectedValue = _OtherEqsgID_OLD;
                else
                    cboEquipmentSegment.SelectedIndex = 0;
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            }

            

            
        }

        private void ReSetCombo()
        {
            //string sPrvEqsg = "";

            //if (cboEquipmentSegment != null) // && cboEquipmentSegment.SelectedIndex > 0 && cboEquipmentSegment.Items.Count > cboEquipmentSegment.SelectedIndex)
            //{
            //    if (!Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
            //    {
            //        sPrvEqsg = Util.NVC(cboEquipmentSegment.SelectedValue);
            //    }
            //}
            
            //DataTable dtTemp = null;

            //if (PROCID.Equals(Process.VD_LMN))
            //    dtTemp = GetEquipmentSegmentComboForVD();
            //else
            //    dtTemp = GetEquipmentSegmentCombo();

            //if (dtTemp != null)
            //{
            //    cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
            //    cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
            //    cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

            //    cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();

            //    if (!sPrvEqsg.Equals("") && !EQPTSEGMENT.Equals(sPrvEqsg))
            //    {
            //        cboEquipmentSegment.SelectedValue = sPrvEqsg;
            //    }
            //    else
            //    {
            //        cboEquipmentSegment.SelectedIndex = 0;
            //    }

            //    cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            //}            
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (wndBOM != null)
                wndBOM.BringToFront();

            if (wndWIP != null)
                wndWIP.BringToFront();

            ApplyPermissions();

            InitializeCombo();

            InitializeGridColumns();

            btnSelectCancel.IsEnabled = false;

            if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
            {
                SetGridData(dgWorkOrder);

                // 남경 조립 VD 이면 시장 유형 hidden
                if (_ProcID.Equals(Process.VD_CDE_PRS) ||
                    _ProcID.Equals(Process.VD_LMN)
                    )
                {
                    if (dgWorkOrder.Columns.Contains("MKT_TYPE_NAME"))
                    {
                        dgWorkOrder.Columns["MKT_TYPE_NAME"].Visibility = Visibility.Collapsed;
                    }
                }
            }

            //if (chkLine != null && chkLine.IsChecked.HasValue)
            //    chkLine.IsChecked = false;

            //if (cboEquipmentSegment != null)
            //    cboEquipmentSegment.IsEnabled = false;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            DgWorkOrder = dgWorkOrder;
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = idx;

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    string workState = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "EIO_WO_SEL_STAT").GetString();
                    btnSelectCancel.IsEnabled = workState == "Y";
                }
            }
        }

        private void dgWorkOrder_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                //Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (LoginInfo.CFG_SHOP_ID == "A050")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLSS_NAME")).Equals("Half-cell"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Lime);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            //e.Cell.Presenter.FontSize = 13;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    SetGridData(dataGrid);
                }
            }));
        }

        public void SetGridData(C1DataGrid dg)
        {
            dg.Columns["CHK"].DisplayIndex = 0;
            dg.Columns["EIO_WO_SEL_STAT"].DisplayIndex = 1;
            dg.Columns["PRJT_NAME"].DisplayIndex = 2;
            dg.Columns["PROD_VER_CODE"].DisplayIndex = 3;
            dg.Columns["WOID"].DisplayIndex = 4;
            dg.Columns["MDLLOT_ID"].DisplayIndex = 5;
            dg.Columns["PRODID"].DisplayIndex = 6;
            dg.Columns["MKT_TYPE_NAME"].DisplayIndex = 7;
            dg.Columns["CELL_3DTYPE"].DisplayIndex = 8;
            dg.Columns["PRODNAME"].DisplayIndex = 9;
            dg.Columns["ELECTYPE"].DisplayIndex = 10;

            dg.Columns["MODLID"].DisplayIndex = 11;
            dg.Columns["LOTYNAME"].DisplayIndex = 12;
            dg.Columns["INPUT_QTY"].DisplayIndex = 13;
            dg.Columns["C_ROLL_QTY"].DisplayIndex = 14;
            dg.Columns["S_ROLL_QTY"].DisplayIndex = 15;
            dg.Columns["LANE_QTY"].DisplayIndex = 16;
            dg.Columns["OUTQTY"].DisplayIndex = 17;
            dg.Columns["UNIT_CODE"].DisplayIndex = 18;
            dg.Columns["STRT_DTTM"].DisplayIndex = 19;
            dg.Columns["END_DTTM"].DisplayIndex = 20;

            dg.Columns["WO_STAT_NAME"].DisplayIndex = 21;
            dg.Columns["WO_STAT_CODE"].DisplayIndex = 22;
            dg.Columns["WO_DETL_ID"].DisplayIndex = 23;
            dg.Columns["EQSGID"].DisplayIndex = 24;
            dg.Columns["EQSGNAME"].DisplayIndex = 25;
            dg.Columns["EQPTID"].DisplayIndex = 26;
            dg.Columns["EQPTNAME"].DisplayIndex = 27;
            dg.Columns["CLSS_ID"].DisplayIndex = 28;            
            dg.Columns["CLSS_NAME"].DisplayIndex = 29;
            dg.Columns["PLAN_TYPE_NAME"].DisplayIndex = 30;

            dg.Columns["PLAN_TYPE"].DisplayIndex = 31;
            dg.Columns["WOTYPE"].DisplayIndex = 32;
            dg.Columns["EIO_WO_DETL_ID"].DisplayIndex = 33;
            dg.Columns["PRDT_CLSS_CODE"].DisplayIndex = 34;
            dg.Columns["DEMAND_TYPE"].DisplayIndex = 35;

            dg.FrozenColumnCount = 5;

        }

        private void dgWorkOrder_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    //if (e.Cell.Row.Type == DataGridRowType.Item)
                    //{
                    //    //e.Cell.Presenter.Background = null;
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //}
                }
            }));
        }

        #region [선택] 버튼 클릭시
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                if (idx < 0)
                    return;

                DataRow dtRow = (dgWorkOrder.Rows[idx].DataItem as DataRowView).Row;

                if (!CanChangeWorkOrder(dtRow))
                    return;
                
                if (string.Equals(PROCID, Process.WASHING))
                {
                    if (!ValidationWashingReWork())
                        return;
                }

                string sWoTypeName = Util.NVC(dtRow["LOTYNAME"]);

                // E20240112-001725 W/O Type interlock
                if (GetChkWorkOrderSelection() 
                    && !sWoTypeName.ToUpper().Equals(GetLotTypeName("P").ToUpper())) // W/O Type이 양산("P")이 아닌 경우
                {
                    Util.MessageConfirm("SFU9913", (result) =>  // [W/O Type : %1] W/O 선택 여부를 확인해 주세요.\n 계속하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            WorkOrderSave(dtRow);
                        }
                    }, new object[]{ sWoTypeName });
                }
                else
                {
                    WorkOrderSave(dtRow);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void WorkOrderSave(DataRow dtRow)
        {
            PRODID = dtRow["PRODID"].ToString();

            string stringMsgCode = string.Empty;
            DataTable dtWipProcChkForWoChg = WipProcChkForWoChg();

            if (dtWipProcChkForWoChg != null && dtWipProcChkForWoChg.Rows.Count > 0)
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    for (int inx = 0; inx < dtWipProcChkForWoChg.Rows.Count; inx++)
                    {
                        if (dtWipProcChkForWoChg.Rows[inx]["PRODID"].ToString() != PRODID)
                        {
                            //선택한 WO 의 제품[%1]과 다른 진행중인 LOT[%2] 이 존재합니다.
                            Util.MessageValidation("SFU3814", new object[] { PRODID, dtWipProcChkForWoChg.Rows[inx]["LOTID"] });

                            return;
                        }
                    }
                }

                // 설비에 진행중인 LOT 이 존재합니다. 진행중인 LOT 이 존재할 때 WO 변경시 실적저장에 이상이 발생할 수 있습니다. WO 를 변경하시겠습니까?
                stringMsgCode = "SFU3730";
            }
            else
            {
                // 작업지시를 변경하시겠습니까?
                stringMsgCode = "SFU2943";
            }

            Util.MessageConfirm(stringMsgCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    WorkOrderChange(dtRow);
                    //SetWorkOrderQtyInfo(sWorkOrder);
                }
            });
        }
        #endregion

        #region LotType 명 조회
        private string GetLotTypeName(string sLotType)
        {
            string lotyName = string.Empty;

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTTYPE"] = sLotType;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTYPE", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count != 0)
                {
                    lotyName = dtResult.Rows[0]["LOTYNAME"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return lotyName;
        }
        #endregion

        #region W/O 작업지시 선택 lnterlock
        private bool GetChkWorkOrderSelection()
        {
            bool isChkWorkOrderSelection = false;

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CMCDTYPE"] = "CHK_WORKORDER_SELECTION";
                dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
                {
                    isChkWorkOrderSelection = true;
                }
                else
                {
                    isChkWorkOrderSelection = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return isChkWorkOrderSelection;
        }
        #endregion

        private DataTable WipProcChkForWoChg()
        {
            DataTable dtResult = null;

            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = PROCID;
                newRow["EQPTID"] = EQPTID;

                inDataTable.Rows.Add(newRow);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_WIP_PROC_FOR_WO_CHG", "INDATA", "OUTDATA", inDataTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtResult;
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EQPTID) || string.IsNullOrEmpty(PROCID) || string.IsNullOrEmpty(EQPTSEGMENT) || !CommonVerify.HasDataGridRow(dgWorkOrder))
                    return;

                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                if (idx < 0) return;

                DataRowView drv = dgWorkOrder.Rows[idx].DataItem as DataRowView;
                if (drv != null)
                {
                    DataRow dr = drv.Row;
                    PRODID = dr["PRODID"].ToString();
                    
                    // 작업지시를 선택취소 하시겠습니까?
                    Util.MessageConfirm("SFU2944", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SelectWorkInProcessStatus(EQPTID, PROCID, EQPTSEGMENT, (table, ex) =>
                            {
                                if (CommonVerify.HasTableRow(table))
                                {
                                    if (LoginInfo.CFG_AREA_ID != "A7" || _ProcID.Equals(Process.STACKING_FOLDING))
                                    {
                                        if (table.Rows[0]["WIPSTAT"].GetString() == "PROC")
                                        {
                                            Util.MessageValidation("SFU1917");
                                            return;
                                        }
                                        else
                                        {
                                            WorkOrderChange(dr, false);
                                        }
                                    }
                                }
                                else
                                {
                                    WorkOrderChange(dr, false);
                                }
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetParentSearchConditions();

                // 자동 조회 처리....
                //if (!CanSearch())
                //    return;

                if (EQPTSEGMENT.Equals("") || EQPTSEGMENT.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                if (EQPTID.Equals("") || EQPTID.ToString().Trim().Equals("SELECT"))
                {
                    return;
                }

                GetWorkOrder();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                // FP안정화 때까지 이전날짜 VALIDATION 무효화
                /*DateTime dtCaldate;
                string sCalDateYMD = "";
                string sCalDateYYYY = "";
                string sCalDateMM = "";
                string sCalDateDD = "";

                CheckCalDateByMonth(dtPik, out dtCaldate, out sCalDateYMD, out sCalDateYYYY, out sCalDateMM, out sCalDateDD);

                if (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;
                    //Util.Alert("SFU1738");      //오늘 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1738");
                    //e.Handled = false;
                    return;
                }*/

                // BASETIME 기준설정
                DateTime currDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string sCurrTime = string.Empty;
                string sBaseTime = string.Empty;

                GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                    baseDate = currDate.AddDays(-1);

                // W/O 공정인 경우에만 체크.
                if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                {
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    //if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                }
                else
                {
                    //if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                }

                //if (Convert.ToDecimal(currDate.ToString("yyyyMM")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                //{
                //    dtPik.Text = baseDate.ToLongDateString();
                //    dtPik.SelectedDateTime = baseDate;
                //    Util.MessageValidation("SFU3448");  //이달 이후 날짜는 선택할 수 없습니다.
                //    return;
                //}

                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다
                    //e.Handled = false;
                    return;
                }

                btnSearch_Click(null, null);
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                // BASETIME 기준설정
                DateTime currDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string sCurrTime = string.Empty;
                string sBaseTime = string.Empty;

                GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                    baseDate = currDate.AddDays(-1);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    baseDate = dtpDateFrom.SelectedDateTime;

                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.
                    return;
                }

                btnSearch_Click(null, null);
            }));
        }
        #endregion

        #region Method

        #region [BizCall]
        private void GetProcessFPInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_PROCESS_FP_INFO();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["PROCID"] = PROCID;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                {
                    _FP_REF_PROCID = "";
                    _Process_ErpUseYN = "";
                    _Process_Plan_Level_Code = "";
                    return;
                }

                // WorkOrder 사용여부, 계획LEVEL 코드.
                _Process_ErpUseYN = Util.NVC(dtRslt.Rows[0]["ERPRPTIUSE"]);
                _Process_Plan_Level_Code = Util.NVC(dtRslt.Rows[0]["PLAN_LEVEL_CODE"]);
                _Process_Plan_Mngt_Type_Code = Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]);

                if (_Process_Plan_Level_Code.Equals("PROC"))//if (!_Process_ErpUseYN.Equals("Y") && _Process_Plan_Level_Code.Equals("PROC")) // PROCESS 인 경우 공정 자동 체크 및 disable.
                {
                    _FP_REF_PROCID = "";

                    //chkProc.IsChecked = true;
                    //chkProc.IsEnabled = false;
                }
                else
                {
                    _FP_REF_PROCID = "";

                    //if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                    //{
                    //    chkProc.IsChecked = true;
                    //    chkProc.IsEnabled = true;
                    //}
                    //else
                    //{
                    //    chkProc.IsChecked = false;
                    //    chkProc.IsEnabled = true;
                    //}
                }

                //2017-10-12  Lee. D. R 
                //FP 에서 W/O 내려주기로 변경됨에 따라 주석처리함
                // Reference 공정인 경우는 REF 공정 정보 설정.
                //if (!_Process_ErpUseYN.Equals("Y") && Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]).Equals("REF") && !Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]).Equals(""))
                //{
                //    _FP_REF_PROCID = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWOInfo(string sWOID, out string sRet, out string sMsg)
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO();

                DataRow newRow = inTable.NewRow();
                newRow["WOID"] = sWOID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WORKORDER", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["WO_STAT_CODE"]).Equals("20") ||
                        Util.NVC(dtResult.Rows[0]["WO_STAT_CODE"]).Equals("40"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        sMsg = "SFU3058";    // 선택 가능한 상태의 작업지시가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                sRet = "NG";
                sMsg = ex.Message;
            }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private bool ChkFPDtlInfoByMonth(string sWODtl, string sCalDateYMD, out string sOutMsg)
        {
            sOutMsg = "";

            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("WO_DETL_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = sWODtl;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FP_DETL_PLAN", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("STRT_DTTM") && dtResult.Columns.Contains("END_DTTM"))
                {
                    DateTime dtStrtDate;
                    DateTime dtEndDate;
                    DateTime.TryParse(Util.NVC(dtResult.Rows[0]["STRT_DTTM"]), out dtStrtDate);
                    DateTime.TryParse(Util.NVC(dtResult.Rows[0]["END_DTTM"]), out dtEndDate);

                    if (dtEndDate != null)
                    {
                        // W/O 공정인 경우에만 체크.
                        if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                        {
                            if (Util.NVC_Int(dtEndDate.ToString("yyyyMMdd")) >= Util.NVC_Int(sCalDateYMD))
                            {
                                bRet = true;
                            }
                            else
                                sOutMsg = "SFU3517";    // 계획일자가 이미 지난 WO는 선택할 수 없습니다.
                        }
                        else
                        {
                            bRet = true;
                        }
                    }
                }

                // Plan date 기준..
                //if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("PLAN_DATE"))
                //{
                //    string sPlanDate = Util.NVC(dtResult.Rows[0]["PLAN_DATE"]);
                //    if (sPlanDate.Length >= 6 && sCalDateYMD.Length >= 6)
                //    {
                //        //if (sPlanDate.Substring(0, 6).Equals(sCalDateYMD.Substring(0, 6)))  // 동일 월인 경우.
                //        //{
                //            // W/O 공정인 경우에만 체크.
                //            if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                //            {
                //                if (Util.NVC_Int(sPlanDate) >= Util.NVC_Int(sCalDateYMD))  // Today ~ 해당 월의 W/O만 선택 가능.
                //                {
                //                    bRet = true;
                //                }
                //                else
                //                    sOutMsg = "SFU3517";    // 계획일자가 이미 지난 WO는 선택할 수 없습니다.
                //            }
                //            else
                //            {
                //                bRet = true;
                //            }
                //        //}
                //        //else
                //        //    sOutMsg = "SFU3443";    // 해당월의 WO만 선택 가능합니다.
                //    }
                //}
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckCalDateByMonth(LGCDatePicker dtPik, out DateTime dtCaldate, out string sCalDateYMD, out string sCalDateYYYY, out string sCalDateMM, out string sCalDateDD)
        {
            try
            {
                bool bRet = false;

                dtCaldate = System.DateTime.Now;
                sCalDateYMD = "";
                sCalDateYYYY = "";
                sCalDateMM = "";
                sCalDateDD = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EQPTID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("CALDATE"))
                    {
                        if (Util.NVC(dtResult.Rows[0]["CALDATE"]).Equals(""))
                            return bRet;


                        DateTime.TryParse(Util.NVC(dtResult.Rows[0]["CALDATE"]), out dtCaldate);
                        //dtCaldate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                    }
                    if (dtResult.Columns.Contains("CALDATE_YMD"))
                        sCalDateYMD = Util.NVC(dtResult.Rows[0]["CALDATE_YMD"]);
                    if (dtResult.Columns.Contains("CALDATE_YYYY"))
                        sCalDateYYYY = Util.NVC(dtResult.Rows[0]["CALDATE_YYYY"]);
                    if (dtResult.Columns.Contains("CALDATE_MM"))
                        sCalDateMM = Util.NVC(dtResult.Rows[0]["CALDATE_MM"]);
                    if (dtResult.Columns.Contains("CALDATE_DD"))
                        sCalDateDD = Util.NVC(dtResult.Rows[0]["CALDATE_DD"]);

                    if (dtResult.Columns.Contains("CALDATE_YYYY") && dtResult.Columns.Contains("CALDATE_MM"))
                    {
                        int iYM = 0;
                        int.TryParse(Util.NVC(dtResult.Rows[0]["CALDATE_YYYY"]) + Util.NVC(dtResult.Rows[0]["CALDATE_MM"]), out iYM);
                        if (dtPik != null && iYM != Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMM")))
                        {
                            bRet = true;
                        }
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                dtCaldate = System.DateTime.Now;
                sCalDateYMD = "";
                sCalDateYYYY = "";
                sCalDateMM = "";
                sCalDateDD = "";

                Util.MessageException(ex);
                return false;
            }
        }

        /// <summary>
        /// 작업지시 선택 biz 호출 처리
        /// </summary>
        /// <param name="dr">선택 한 작업지시 정보 DataRow</param>
        /// <param name="isSelectFlag">선택 처리:true 선택 취소:fals</param>
        private void SetWorkOrderSelect(DataRow dr, bool isSelectFlag = true)
        {
            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = isSelectFlag ? Util.NVC(dr["WO_DETL_ID"]) : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);

                if (isSelectFlag)
                    Util.MessageInfo("SFU2940");    //작업지시가 변경 되었습니다.
                else
                    Util.MessageInfo("SFU2942");    //작업지시가 선택취소 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 작업지시 리스트 조회
        /// </summary>
        public void GetWorkOrder(bool isSelectFlag = true)
        {
            try
            {
                InitializeCombo();

                //SetChangeDatePlan(false);

                if (PROCID.Length < 1 || EQPTID.Length < 1 || EQPTSEGMENT.Length < 1)
                    return;

                // 일자 설정이 안된경우 RETURN.
                if (dtpDateFrom.SelectedDateTime.Year < 2000 || dtpDateTo.SelectedDateTime.Year < 2000)
                    return;

                // Process 정보 조회
                GetProcessFPInfo();

                // 현 작지 정보 조회.
                //string sWODetl = GetEIOInfo();

                string sPrvWODTL = string.Empty;

                if (dgWorkOrder.ItemsSource != null && dgWorkOrder.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                    if (idx >= 0)
                    {
                        sPrvWODTL = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));

                        //if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                        //    btnSelectCancel.IsEnabled = true;
                    }
                }

                // 취소인 경우에는 선택 없애도록..
                if (!isSelectFlag)
                    sPrvWODTL = "";

                InitializeGridColumns();

                ClearWorkOrderInfo();
                //ParentDataClear();  // Caller 화면 Data Clear.

                btnSelectCancel.IsEnabled = false;

                DataTable searchResult = null;

                //if (_Process_ErpUseYN.Equals("Y"))  // ERP 실적 전송인 경우는 Workorder Inner Join..
                //{
                //    if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                //        searchResult = GetEquipmentWorkOrderByProcWithInnerJoin();
                //    else
                //        searchResult = GetEquipmentWorkOrderWithInnerJoin();
                //}
                //else
                //{
                //    if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                //        searchResult = GetEquipmentWorkOrderByProc();
                //    else
                //        searchResult = GetEquipmentWorkOrder();
                //}

                //searchResult = GetWorkOrderListEltrAssy();  // W/O 조회 1개 BIZ로 통합 관련 수정.

                searchResult = GetWorkOrderListByEquipmentSegment();

                if (searchResult == null)
                    return;


                //dgWorkOrder.ItemsSource = DataTableConverter.Convert(searchResult);
                Util.GridSetData(dgWorkOrder, searchResult, FrameOperation, true);


                // 3D 제품이 존재하는 경우
                if (dgWorkOrder.Columns.Contains("CELL_3DTYPE"))
                {
                    if (searchResult.Columns.Contains("CELL_3DYN"))
                    {
                        DataRow[] drTmp = searchResult.Select("CELL_3DYN = 'Y'");
                        if (drTmp.Length > 0)
                        {
                            dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        dgWorkOrder.Columns["CELL_3DTYPE"].Visibility = Visibility.Collapsed;
                    }
                }

                // 현 작업지시 정보 Top Row 처리 및 고정..
                if (searchResult.Rows.Count > 0)
                {
                    if (!Util.NVC(searchResult.Rows[0]["EIO_WO_DETL_ID"]).Equals(""))
                        dgWorkOrder.FrozenTopRowsCount = 1;
                    else
                        dgWorkOrder.FrozenTopRowsCount = 0;
                }

                // 이전 선택 작지 선택
                if (!sPrvWODTL.Equals(""))
                {
                    int idx = _Util.GetDataGridRowIndex(dgWorkOrder, "WO_DETL_ID", sPrvWODTL);

                    if (idx >= 0)
                    {
                        for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                            if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                                DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", true);
                            else
                                DataTableConverter.SetValue(dgWorkOrder.Rows[i].DataItem, "CHK", false);

                        DataTableConverter.SetValue(dgWorkOrder.Rows[idx].DataItem, "CHK", true);

                        // 재조회 처리.
                        ReChecked(idx);

                        PRODID = DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID").ToString();
                    }
                }
                else // 최초 조회 시 쿼리에서 CHK 값이 있는경우 Row Select 처리.
                {
                    for (int i = 0; i < dgWorkOrder.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            dgWorkOrder.SelectedIndex = i;
                            PRODID = DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PRODID").ToString();
                            break;
                        }
                    }
                }

                //선택 취소 버튼 Enabled 속성 설정
                if (CommonVerify.HasDataGridRow(dgWorkOrder))
                {
                    int idx = _Util.GetDataGridFirstRowIndexByCheck(dgWorkOrder, "CHK");
                    if (idx != -1)
                    {
                        string workState = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"));
                        btnSelectCancel.IsEnabled = workState == "Y";
                    }
                }

                //// 공정 조회인 경우 설비 정보 Visible 처리.
                //if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                //    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                //else
                //    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

                //if (chkLine.IsChecked.HasValue && (bool)chkLine.IsChecked)
                if (cboEquipmentSegment != null && Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    dgWorkOrder.Columns["EQSGNAME"].Visibility = Visibility.Collapsed;
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgWorkOrder.Columns["EQSGNAME"].Visibility = Visibility.Visible;
                    if (_bShowEqptName)
                        dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }

                // Summary 조회.
                //SetWorkOrderQtyInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비별 작업지시 리스트 조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetEquipmentWorkOrder()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(_CoatSideType))
                    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 공정별 작업지시 리스트 조회
        /// </summary>
        /// <returns></returns>
        private DataTable GetEquipmentWorkOrderByProc()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderByProcWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(_CoatSideType))
                    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public DataTable GetWorkOrderSummaryInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public DataTable GetWorkOrderSummaryInfoWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY_WITH_FP", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public DataTable GetWorkOrderSummaryInfoByProcID()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO_BY_PROCID();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY_BY_PROCID", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public DataTable GetWorkOrderSummaryInfoByProcIDWithInnerJoin()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WO_SUMMARY_INFO_BY_PROCID();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _EqptSegment;
                newRow["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_SUMMARY_BY_PROCID_WITH_FP", "INDATA", "OUTDATA", inTable);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private string GetEIOInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_PLAN_DETAIL_BYEQPTID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = EQPTID;

                inTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORER_PLAN_DETAIL_BYEQPTID", "INDATA", "OUTDATA", inTable);

                if (dtResult == null || dtResult.Rows.Count < 1)
                    return "";
                
                return Util.NVC(dtResult.Rows[0]["WO_DETL_ID"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        
        private DataTable GetWorkOrderListByEquipmentSegment()
        {
            try
            {
                string bizRuleName = string.Equals(PROCID, Process.WASHING) ? "DA_PRD_SEL_WORKORDER_LIST_WASH_RW" : "DA_PRD_SEL_WORKORDER_LIST_WITH_FP_BY_LINE";

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_LINE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _FP_REF_PROCID.Equals("") ? PROCID : _FP_REF_PROCID;
                //searchCondition["EQSGID"] = EQPTSEGMENT;
                searchCondition["EQPTID"] = EQPTID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                //searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode();

                if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0 &&
                    !Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    searchCondition["OTHER_EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    searchCondition["EQSGID"] = "";
                    searchCondition["PROC_EQPT_FLAG"] = "LINE";
                }
                else
                {
                    searchCondition["OTHER_EQSGID"] = "";
                    searchCondition["EQSGID"] = EQPTSEGMENT;
                    searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode();
                }

                //if (!string.IsNullOrEmpty(_CoatSideType))
                //    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;

                inTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentSegmentCombo()
        {       
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = PROCID;
                dr["EQPTID"] = EQPTID;
                RQSTDT.Rows.Add(dr);

                //ESMI-A4동 6Line 제외처리
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new DataTable();
                if (IsCmiExceptLine())
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_EXCEPT_LINE_CBO_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);
                }

                DataTable dtTemp = null;

                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentSegmentComboInline()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("OUT_LOT_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = PROCID;
                dr["EQPTID"] = EQPTID;
                dr["OUT_LOT_TYPE"] = strOUT_LOT_TYPE;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR_WND", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = null;

                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentSegmentComboForVD()
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
                dr["PROCID"] = "A1000," + Process.VD_LMN + "," + Process.VD_ELEC;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_FOR_VD", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = null;

                //if (dtResult.Select("CBO_CODE <> '" + EQPTSEGMENT + "'").Length > 0)
                //{
                //    dtTemp = dtResult.Select("CBO_CODE <> '" + EQPTSEGMENT + "'").CopyToDataTable();
                //}
                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return null;
            }
        }

        private string GetFpPlanGnrtBasCode()
        {
            try
            {
                string sPlanType = "";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = PROCID;
                dr["EQSGID"] = EQPTSEGMENT;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_PLAN_GNRT_BAS_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("FP_PLAN_GNRT_BAS_CODE"))
                {
                    if (Util.NVC(dtResult.Rows[0]["FP_PLAN_GNRT_BAS_CODE"]).Equals("E"))
                    {
                        sPlanType = "EQPT";
                        _bShowEqptName = true;
                    }
                    else
                    {
                        sPlanType = "LINE";
                        _bShowEqptName = false;
                    }
                }

                return sPlanType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "LINE";
            }
        }
        #endregion

        #region [Validation]
        private bool CanSearch()
        {
            bool bRet = false;

            if (PROCID.Length < 1)
            {
                Util.MessageValidation("SFU1456");      //공정 정보가 없습니다.
                return bRet;
            }

            if (EQPTSEGMENT.Equals("") || EQPTSEGMENT.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return bRet;
            }

            if (EQPTID.Equals("") || EQPTID.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnSelectCancel);

            if (FrameOperation != null)
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 작업지시 선택 가능 Validation 처리
        /// </summary>
        /// <param name="iRow">선택한 작업지시 정보 Row Number</param>
        /// <returns></returns>
        private bool CanChangeWorkOrder(DataRow dtRow)
        {
            bool bRet = false;

            if (dtRow == null)
                return bRet;

            if (EQPTID.Trim().Equals("") || PROCID.Trim().Equals("") || EQPTSEGMENT.Trim().Equals(""))
                return bRet;

            if (Util.NVC(dtRow["EIO_WO_SEL_STAT"]).Equals("Y"))
            {
                Util.MessageValidation("SFU3061");  //이미 선택된 작업지시 입니다.
                return bRet;
            }

            if (_ProcID.Equals(Process.LAMINATION) && LoginInfo.CFG_AREA_ID != "A7")
            {
                if (SelectProcStateLot())
                {
                    Util.MessageValidation("SFU1917");
                    return bRet;
                }
            }

            if (_ProcID.Equals(Process.STACKING_FOLDING))
            {
                if (SelectProcStateLot())
                {
                    Util.MessageValidation("SFU1917");
                    return bRet;
                }
            }

            // Workorder 내려오는 공정만 체크 필요.
            if (_Process_ErpUseYN.Equals("Y"))
            {
                // 선택 가능한 작지 여부 확인.
                string sRet = string.Empty;
                string sMsg = string.Empty;

                GetWOInfo(Util.NVC(dtRow["WOID"]), out sRet, out sMsg);
                if (sRet.Equals("NG"))
                {
                    Util.MessageValidation(sMsg);
                    return bRet;
                }
            }


            // 해당 월의 W/O만 선택 가능
            DateTime dtCaldate;
            string sCalDateYMD = "";
            string sCalDateYYYY = "";
            string sCalDateMM = "";
            string sCalDateDD = "";
            string sOutMsg = "";

            CheckCalDateByMonth(null, out dtCaldate, out sCalDateYMD, out sCalDateYYYY, out sCalDateMM, out sCalDateDD);
            if (!ChkFPDtlInfoByMonth(Util.NVC(dtRow["WO_DETL_ID"]), sCalDateYMD, out sOutMsg))
            {
                Util.MessageValidation(sOutMsg);
                return bRet;
            }

            // 자동 Rolling에 따라 순차적 WO 처리를 위한 Validation
            DataTable dt = DataTableConverter.Convert(dgWorkOrder.ItemsSource);
            DataRow[] dr = dt?.Select("EIO_WO_SEL_STAT = 'Y'");
            if (dr?.Length > 0 && dt.Columns.Contains("DEMAND_TYPE") && dt.Columns.Contains("PRODID") && dt.Columns.Contains("MKT_TYPE_CODE"))
            {
                foreach (DataRow drTmp in dr)
                {
                    if (Util.NVC(dtRow["DEMAND_TYPE"]).Equals(Util.NVC(drTmp["DEMAND_TYPE"])) && 
                        Util.NVC(dtRow["PRODID"]).Equals(Util.NVC(drTmp["PRODID"])) && 
                        Util.NVC(dtRow["MKT_TYPE_CODE"]).Equals(Util.NVC(drTmp["MKT_TYPE_CODE"])))
                    {
                        Util.MessageValidation("SFU4117"); // 동일한 모델, WO Type의 WO가 이미 선택되어 있습니다.
                        return bRet;
                    }
                }
            }

            if(ValidationByInputCompleteOptiojn(dtRow) == false)
            {
                Util.MessageValidation("SFU3790"); //투입중인 LOT 이 존재할 경우 제품ID가 다른 WO를 선택할 수 없습니다.
                bRet = false;
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool ValidationWashingReWork()
        {
            if (string.IsNullOrEmpty(EQPTID))
                return false;

            const string bizRuleName = "DA_BAS_SEL_PROD_LOT_BY_EQPTID_WS";
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PR_LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["EQPTID"] = EQPTID;
            dr["PR_LOTID"] = null;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(searchResult))
            {
                //진행중인 LOT이 있습니다.\r\nLOT ID : {%1}
                Util.MessageValidation("SFU3199", searchResult.Rows[0]["LOTID"]);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 작업지시 선택 처리
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="isSelectFlag"> 선택 처리:true 선택 취소:false </param>
        private void WorkOrderChange(DataRow dr, bool isSelectFlag = true)
        {
            if (dr == null) return;

            SetWorkOrderSelect(dr, isSelectFlag);

            GetWorkOrder(isSelectFlag);

            if (_UCParent != null)
            {
                ParentDataClear();  // Caller 화면 Data Clear.

                SearchParentAll();  // Caller 화면 Data 모두 조회
            }
            else if (_UCElec != null)
            {
                _UCElec.REFRESH = true;
            }
        }

        /// <summary>
        /// Main 화면 실적 실적 조회 Call
        /// </summary>
        /// <param name="drSelWorkOrder">선택한 작지 정보</param>
        private void SearchParentProductInfo(DataRow dataRow)
        {
            //if (_UCParent == null)
            //    return;

            //if (dataRow != null)
            //{
            //    try
            //    {   
            //        Type type = _UCParent.GetType();
            //        MethodInfo methodInfo = type.GetMethod("GetProductLot");
            //        ParameterInfo[] parameters = methodInfo.GetParameters();

            //        object[] parameterArrys = new object[parameters.Length];
            //        parameterArrys[0] = dataRow;
            //        if(parameterArrys.Length > 1)
            //            parameterArrys[1] = true;  //  이전 선택 Lot 선택 여부. (true:이전선택 Lot 자동 선택, false:첫번째 PROC 상태 Lot 선택)

            //        methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            //    }
            //    catch(Exception ex)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //    }
            //}
        }

        private void SearchParentAll()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetAllInfoFromChild");
                if (methodInfo == null)
                    return;
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                // 전극 조립 모두 사용 하므로 없는 공정 존재. 하여 exception  처리 안함.
            }
        }

        /// <summary>
        /// Main 화면 Data Clear 처리
        /// </summary>
        private void ParentDataClear()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("ClearControls");
                methodInfo.Invoke(_UCParent, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetParentSearchConditions()
        {
            if (_UCParent == null)
                return;

            try
            {
                Type type = _UCParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetSearchConditions");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                    parameterArrys[i] = null;

                object result = methodInfo.Invoke(_UCParent, parameters.Length == 0 ? null : parameterArrys);

                if ((bool)result)
                {
                    PROCID = parameterArrys[0].ToString();
                    EQPTSEGMENT = parameterArrys[1].ToString();
                    EQPTID = parameterArrys[2].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 작업지시 Datagrid Clear
        /// </summary>
        public void ClearWorkOrderInfo()
        {
            Util.gridClear(dgWorkOrder);
            //InitializeWorkorderQuantityInfo();
            //txtWOID.Text = "";

            ReSetCombo();
        }

        public DataRow GetSelectWorkOrderRow()
        {
            DataRow row = null;

            try
            {
                DataRow[] dr = Util.gridGetChecked(ref dgWorkOrder, "CHK");

                if (dr == null || dr.Length < 1)
                    row = null;
                else
                    row = dr[0];

                return row;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void ReChecked(int iRow)
        {
            if (iRow < 0)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count - dgWorkOrder.BottomRows.Count < iRow)
                return;

            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "CHK")).Equals("1"))
            {
                //row 색 바꾸기
                dgWorkOrder.SelectedIndex = iRow;

                // 선택 작지 수량 설정
                //SetWorkOrderQtyInfo((dgWorkOrder.Rows[iRow].DataItem as DataRowView).Row);

                // 실적 조회 호출..
                //DataRow[] selRow = GetWorkOrderInfo(sWOID);
                //SearchParentProductInfo((dgWorkOrder.Rows[iRow].DataItem as DataRowView).Row);
            }
        }

        private void SetChangeDatePlan(bool isInitFlag = true)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (isInitFlag)
            {
                if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateFrom.Tag = "CHANGE";

                    dtpDateTo.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateTo.Tag = "CHANGE";
                }
            }
            else
            {
                if (Util.NVC_Decimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate;
                    dtpDateFrom.Tag = "CHANGE";
                }

                if (Util.NVC_Decimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateTo.SelectedDateTime = currDate;
                    dtpDateTo.Tag = "CHANGE";
                }
            }
        }

        private void GetChangeDatePlan(out DateTime currDate, out string sCurrTime, out string sBaseTime)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            currDate = GetCurrentTime();
            sCurrTime = currDate.ToString("HHmmss");
            sBaseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);
        }
        #endregion

        #endregion

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void dgWorkOrder_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;
                        default:
                            if (!dg.Columns.Contains("CHK"))
                                return;

                            if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content is RadioButton)
                            {
                                RadioButton rdoButton = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

                                if (rdoButton != null)
                                {
                                    if (rdoButton.DataContext == null)
                                        return;

                                    // 부모 조회 없으므로 로직 수정..
                                    if (!(bool)rdoButton.IsChecked && (rdoButton.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                                    {
                                        DataRow dtRow = (rdoButton.DataContext as DataRowView).Row;

                                        for (int i = 0; i < dg.Rows.Count; i++)
                                            if (e.Cell.Row.Index == i)   // Mode = OneWay 이므로 Set 처리.
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", true);
                                            else
                                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                        //row 색 바꾸기
                                        dgWorkOrder.SelectedIndex = e.Cell.Row.Index;

                                        // 선택 작지 수량 설정
                                        //SetWorkOrderQtyInfo(dtRow);

                                        // 실적 조회 호출..
                                        //DataRow[] selRow = GetWorkOrderInfo(sWOID);
                                        //SearchParentProductInfo(dtRow);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgWorkOrder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (wndBOM != null)
                        return;

                    if (wndWIP != null)
                        return;

                    //if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                    //    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    //    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt &&
                    //    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
                    //{
                        C1DataGrid dg = sender as C1DataGrid;

                        if (dg == null) return;

                        C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                        if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                        switch (Convert.ToString(currCell.Column.Name))
                        {
                            case "WOID":

                                wndBOM = new LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM();
                                wndBOM.FrameOperation = FrameOperation;

                                if (wndBOM != null)
                                {
                                    object[] Parameters = new object[7];
                                    Parameters[0] = currCell.Text;

                                    C1WindowExtension.SetParameters(wndBOM, Parameters);

                                    wndBOM.Closed += new EventHandler(wndBOM_Closed);
                                    this.Dispatcher.BeginInvoke(new Action(() => wndBOM.ShowModal()));
                                }
                                break;
                        case "PRODID":
                            if (_ProcID.Equals(Process.LAMINATION) || _ProcID.Equals(Process.STACKING_FOLDING) || _ProcID.Equals(Process.PACKAGING))
                            {
                                wndWIP = new LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_WIPLIST();
                                wndWIP.FrameOperation = FrameOperation;

                                if (wndWIP != null)
                                {
                                    object[] Parameters = new object[7];

                                    C1.WPF.DataGrid.DataGridRow currRow = dg.CurrentRow;

                                    DataRow dtRow = (dgWorkOrder.Rows[currRow.Index].DataItem as DataRowView).Row;
                                    string ValueToWOID = dtRow["WOID"].ToString();

                                    #region 
                                    string ValueToPRODID = dtRow["PRODID"].ToString();
                                    #endregion

                                    Parameters[0] = ValueToWOID;
                                    Parameters[1] = LoginInfo.CFG_AREA_ID;
                                    Parameters[2] = _EqptSegment;
                                    Parameters[3] = _ProcID;
                                    #region
                                    Parameters[4] = Area_Type.ASSY;
                                    Parameters[5] = ValueToPRODID;
                                    #endregion
                                    C1WindowExtension.SetParameters(wndWIP, Parameters);

                                    wndWIP.Closed += new EventHandler(wndWIP_Closed);
                                    this.Dispatcher.BeginInvoke(new Action(() => wndWIP.ShowModal()));
                                }
                            }
                            break;
                        case "PRJT_NAME":
                            if (_ProcID.Equals(Process.WINDING) || _ProcID.Equals(Process.ASSEMBLY) || _ProcID.Equals(Process.WASHING))
                                wndPROD = new LGC.GMES.MES.CMM001.CMM_ASSY_PRDT_GPLM();
                                wndPROD.FrameOperation = FrameOperation;

                            if (wndPROD != null)
                            {
                                            object[] Parameters = new object[2];
                                            Parameters[0] = DataTableConverter.GetValue(currCell.Row.DataItem, "PRODID");

                                            if (string.Equals(_ProcID, Process.WINDING))
                                                Parameters[1] = Gplm_Process_Type.WINDING;
                                            else if(string.Equals(_ProcID, Process.ASSEMBLY))
                                                Parameters[1] = Gplm_Process_Type.ASSEMBLY;
                                            else
                                                Parameters[1] = Gplm_Process_Type.WASHING;

                                            C1WindowExtension.SetParameters(wndPROD, Parameters);

                                            this.Dispatcher.BeginInvoke(new Action(() => wndPROD.ShowModal()));
                            }
                            break;
                    }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                    //}
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void wndBOM_Closed(object sender, EventArgs e)
        {
            wndBOM = null;

            LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM window = sender as LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndWIP_Closed(object sender, EventArgs e)
        {
            wndWIP = null;

            LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_WIPLIST window = sender as LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_WIPLIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndPROD_Closed(object sender, EventArgs e)
        {
            wndPROD = null;

            LGC.GMES.MES.CMM001.CMM_ASSY_PRDT_GPLM window = sender as LGC.GMES.MES.CMM001.CMM_ASSY_PRDT_GPLM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private static void SelectWorkInProcessStatus(string eqptCode, string processCode, string eqptSegmentCode, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = eqptCode;
                inData["PROCID"] = processCode;
                inData["EQSGID"] = eqptSegmentCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    actionCompleted?.Invoke(result, null);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private bool SelectProcStateLot()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_WIP_STATUS";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = EQPTID;
                inData["PROCID"] = PROCID;
                inData["EQSGID"] = EQPTSEGMENT;
                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("WIPSTAT"))
                {
                    if (dtRslt.Rows[0]["WIPSTAT"].Equals("PROC"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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

        private bool ValidationByInputCompleteOptiojn(DataRow pDtRow)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_LOT_INPUT_COMPLETE_OPTION";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));      // 설비 ID
                inDataTable.Columns.Add("PROCID", typeof(string));      // 공정 ID
                inDataTable.Columns.Add("EQSGID", typeof(string));      // 설비 세그먼트 ID

                DataRow inData = inDataTable.NewRow();
                inData["EQPTID"] = EQPTID;
                inData["PROCID"] = PROCID;
                inData["EQSGID"] = EQPTSEGMENT;
                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("PRODID"))
                {
                    //이전LOT 과 변경하려는 WO 의 PRODID가 동일한지 확인
                    if (Util.NVC(dtRslt.Rows[0]["PRODID"]) == Util.NVC(pDtRow["PRODID"]))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        private void chkLine_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                cboEquipmentSegment.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkLine_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0)
                    cboEquipmentSegment.SelectedIndex = 0;

                cboEquipmentSegment.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboEquipmentSegment?.SelectedValue != null)
                    _OtherEqsgID_OLD = cboEquipmentSegment.SelectedValue.ToString();
                else
                    _OtherEqsgID_OLD = "";

                btnSearch_Click(null, null);
           
                //// 라인 선택 시 자동 조회 처리
                //if (cboEquipmentSegment.SelectedIndex > 0 && cboEquipmentSegment.Items.Count > cboEquipmentSegment.SelectedIndex)
                //{
                //    if (!Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                //    {
                //        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkOrder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (wndBOM != null)
                        return;

                    C1DataGrid dg = sender as C1DataGrid;

                    if (dg == null) return;

                    C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                    if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                    switch (Convert.ToString(currCell.Column.Name))
                    {
                        case "WOID":                            
                            wndBOM = new LGC.GMES.MES.CMM001.Popup.CMM_WORKORDER_BOM();
                            wndBOM.FrameOperation = FrameOperation;

                            if (wndBOM != null)
                            {
                                object[] Parameters = new object[7];
                                Parameters[0] = currCell.Text;

                                C1WindowExtension.SetParameters(wndBOM, Parameters);

                                wndBOM.Closed += new EventHandler(wndBOM_Closed);
                                this.Dispatcher.BeginInvoke(new Action(() => wndBOM.ShowModal()));
                            }
                            break;
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void HideLineSelectArea()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (LineSelectContents.ColumnDefinitions[3].Width.Value <= 0) return;

                //LineSelectContents.ColumnDefinitions[2].Width = new GridLength(0);

                // Hide Animation.
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(150); //new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0); //new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                //gla.Completed += HideTestAnimationCompleted;
                LineSelectContents.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, gla);
                LineSelectContents.ColumnDefinitions[3].BeginAnimation(ColumnDefinition.WidthProperty, gla);

                // Show Animation.
                LGC.GMES.MES.CMM001.GridLengthAnimation glaHide = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                glaHide.From = new GridLength(0); //new GridLength(0, GridUnitType.Star);
                glaHide.To = new GridLength(80);  //new GridLength(1, GridUnitType.Star);
                glaHide.Duration = new TimeSpan(0, 0, 0, 0, 500);
                LineSelectContents.ColumnDefinitions[7].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);
                LineSelectContents.ColumnDefinitions[8].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);
                LineSelectContents.ColumnDefinitions[9].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);
                LineSelectContents.ColumnDefinitions[10].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);

            }));
        }

        private void ShowLineSelectArea()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (LineSelectContents.ColumnDefinitions[3].Width.Value > 0) return;

                //LineSelectContents.ColumnDefinitions[2].Width = new GridLength(4);

                // Show Animation.
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0); //new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(150);  //new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                //gla.Completed += showTestAnimationCompleted;
                LineSelectContents.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, gla);
                LineSelectContents.ColumnDefinitions[3].BeginAnimation(ColumnDefinition.WidthProperty, gla);

                // Hide Animation.
                LGC.GMES.MES.CMM001.GridLengthAnimation glaHide = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                glaHide.From = new GridLength(80); //new GridLength(0, GridUnitType.Star);
                glaHide.To = new GridLength(0);  //new GridLength(1, GridUnitType.Star);
                glaHide.Duration = new TimeSpan(0, 0, 0, 0, 500);
                LineSelectContents.ColumnDefinitions[7].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);
                LineSelectContents.ColumnDefinitions[8].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);
                LineSelectContents.ColumnDefinitions[9].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);
                LineSelectContents.ColumnDefinitions[10].BeginAnimation(ColumnDefinition.WidthProperty, glaHide);

                //ColorAnimationInredRectangle();
            }));            
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            HideLineSelectArea();
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowLineSelectArea();
        }

        public void SetWorkOrderForVD(string sWoDetlID, bool isSelectFlag, bool bShowMsg)
        {
            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_EIO_WO_DETL_ID();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = EQPTID;
                newRow["WO_DETL_ID"] = isSelectFlag ? Util.NVC(sWoDetlID) : string.Empty;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIO_WO_DETL_ID", "INDATA", "", inTable);

                if (bShowMsg)
                {
                    if (isSelectFlag)
                        Util.MessageInfo("SFU2940");    //작업지시가 변경 되었습니다.
                    else
                        Util.MessageInfo("SFU2942");    //작업지시가 선택취소 되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
               

        public void CheckInline(string strline)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_UNLDR_FLAG";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["EQSGID"] = strline;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            iCntInline = searchResult.Rows.Count;

        }

        //ESMI 1동(A4) 1~5 Line 만 조회
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_EXCEPT_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}
