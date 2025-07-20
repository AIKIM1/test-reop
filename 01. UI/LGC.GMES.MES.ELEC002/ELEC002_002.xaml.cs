/*****************************************
 Created Date : 2019.10.02
      Creator : JEONG
   Decription : 코터 공정진척
------------------------------------------
 [Change History]
 2019-10-11   : BASEFORM UI분리
 2021-09-03  오화백  : Fast Track 체크박스 추가
 2022.09.13  정재홍  : C20220622-000589 - Coater Process Progress(New) W/O 변경시 슬러치 자동 탈착
 2023.04.28  김도형  : [E20230228-000007] CSR - Marking logic in GMES
 2023.05.10  김도형  : [E20230127-000272] 코터 공정진척 2번컷 특이사항 자동 입력 건
 2023.08.28  김도형  : [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
 2023.09.05  임재형 : 롤맵 홀드 조건 및 노트 추가
 2024.01.10  조성근  : 롤맵 코터 수불 실적 적용
 2024.02.26  조성근  : TEST CUT 관련하여 TEST CUT 사용시만 저장여부 체크하도록 수정
 2024.08.07  유명환  : [E20240701-000286] MES 시스템의 전극 LOT별 로딩량 데이터 연동
******************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Globalization;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF;
using C1.WPF.DataGrid;
using System.Linq;
using System.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_002 : UserControl, IWorkArea
    {
        #region Initialize
        private Util _Util = new Util();

        private GridLength ExpandFrame;

        private string _LDR_LOT_IDENT_BAS_CODE;
        private string _UNLDR_LOT_IDENT_BAS_CODE;
        private string _IS_POSTING_HOLD;
        private string _LSL_USL_HOLD;

        private DataTable _CURRENT_LOTINFO = new DataTable();

        private int dgLVIndex1 = 0;
        private int dgLVIndex2 = 0;
        private int dgLVIndex3 = 0;

        private bool isDefectLevel = false;
        private bool isChangeReason = false;
        private bool isChangeQuality = false;
        private bool isChangeMaterial = false;
        private bool isChangeRemark = false;

        private bool isDupplicatePopup = false;

        //2021-09-09 오화백
        private string _FastTrackLot = string.Empty;

        // RollMap 관련 변수 선언
        private bool _isRollMapEquipment = false;
        private bool _isRollMapResultLink = false;  // 동별 공정별 롤맵 실적 연계 여부
        private bool _isOriginRollMapEquipment = false;
        bool _isTestCutApply = true;       //nathan 2024.01.10 롤맵 코터 수불 실적 적용

        private DataTable _dtRollmapEqptDectModifyTarget = new DataTable();    // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private string _ProcRollmapEqptDectModifyApplyFlag = "N";              // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선 

        // CSR : C20220622-000589 
        private bool _isWoParent = false;

        // 코터 롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////
        Dictionary<string, string> holdLotClassCode = new Dictionary<string, string>();
        // 코터 롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////

        public ELEC002_002()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }
        public DataTable WIPCOLORLEGEND { get; private set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            _isWoParent = false;
            
            grdWorkOrder.Children.Add(new UC_WORKORDER_CWA());
            InitControlVisible();
            InitComboBox();
            InitDataTable();
            SetRollMapEquipment();
            ApplyPermissions();

            //공정 설비 불량코드 수정 제외 목록 정보           
            SetProcRollmapEqptDectModifyApplyFlag();
            if (string.Equals(Util.NVC(_ProcRollmapEqptDectModifyApplyFlag), "Y"))
            {
                SetRollmapEqptDectModifyTarget();
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            foreach (Button button in Util.FindVisualChildren<Button>(mainGrid))
                listAuth.Add(button);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitControlVisible()
        {
            // TO-DO : 법인별로 Control 활성화 여부 표시 분기 시 이쪽에 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "ED"))
                btnBarcodeLabel.Visibility = Visibility.Visible;

            List<Button> popupbtnList = new List<Button>();
            popupbtnList.Add(btnScrapLot);

            SetManualMode(popupbtnList);

            //if (_Util.IsCommonCodeUse("PROD_RSLT_MGMT_CWA", LoginInfo.USERID) == false)
            //{
            //    btnScrapLot.Visibility = Visibility.Visible;
            //}

            lblSlurryTop2.Visibility = Visibility.Hidden;
            txtTopSlurry2.Visibility = Visibility.Hidden;
            lblSlurryBack2.Visibility = Visibility.Hidden;
            txtBackSlurry2.Visibility = Visibility.Hidden;
        }

        private void InitComboBox()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: new string[] { Process.COATING, "DSC", "A" });

            // 2020.07.06 공정 Interlock - 범례 속성 공통코드로 조회
            SetWipColorLegendCombo();
        }

        private void InitDataTable()
        {
            _CURRENT_LOTINFO.Columns.Add("LOTID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("WIPSEQ", typeof(Int32));
            _CURRENT_LOTINFO.Columns.Add("WIPSTAT", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("LOTID_PR", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("CSTID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("OUT_CSTID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("CUT_ID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("PRODID", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("CUT", typeof(string));
            _CURRENT_LOTINFO.Columns.Add("INPUT_BACK_QTY", typeof(decimal));
            _CURRENT_LOTINFO.Columns.Add("INPUT_TOP_QTY", typeof(decimal));
        }

        private void InitAllClearControl()
        {
            Util.gridClear(((UC_WORKORDER_CWA)grdWorkOrder.Children[0]).dgWorkOrder);
            Util.gridClear(dgLargeLot);
            Util.gridClear(dgProductLot);
            this.InitClearControl();
        }

        private void InitClearControl()
        {
            Util.gridClear(dgLotInfo);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgWipReason2);
            Util.gridClear(dgQuality);
            Util.gridClear(dgQuality2);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgRemark);
            Util.gridClear(dgRemarkHistory);

            _CURRENT_LOTINFO.Clear();

            _IS_POSTING_HOLD = string.Empty;
            txtUnit.Text = string.Empty;

            txtInputQty.Value = 0;
            txtParentQty.Value = 0;
            txtRemainQty.Value = 0;

            txtStartDateTime.Text = string.Empty;
            txtEndDateTime.Text = string.Empty;
            txtWorkDate.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtLaneQty.Value = 0;
            txtCurLaneQty.Value = 0;

            chkFinalCut.IsChecked = false;

            isChangeReason = false;
            isChangeQuality = false;
            isChangeMaterial = false;
            isChangeRemark = false;

            btnSaveWipReason.IsEnabled = false;
            btnPublicWipSave.IsEnabled = false;

            //20210903 FastTrack 초기화  : 오화백 
            chkFastTrack.IsChecked = false;
            chkFastTrack.Visibility = Visibility.Collapsed;
            titleFastTrack.Visibility = Visibility.Collapsed;

            btnRollMap.IsEnabled = false;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

                if (Convert.ToDateTime(sShiftTime) < System.DateTime.Now)
                {
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                }
            }

            setRollmapResultLinkBtn();  //nathan 2024.01.10 롤맵 코터 수불 실적 적용
        }
        #endregion

        #region CWA용 LV Filter 로직
        private void ClearDefectLV()
        {
            if (chkDefectFilter.IsChecked == true)
            {
                isDefectLevel = true;
                OnClickDefetectFilter(chkDefectFilter, null);
                isDefectLevel = false;
            }
        }

        private void OnClickDefetectFilter(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (dgLotInfo.GetRowCount() < 1)
            {
                if (isDefectLevel == false)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                }
                cb.IsChecked = false;
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
                return;
            }

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                GetDefectLevel();
                if (isDefectLevel == false)
                    yPosition.Height = new GridLength(yPosition.ActualHeight + grdDefectLVFilter.ActualHeight);
            }
            else
            {
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
            }
        }

        private void DefectVisibleLV(DataTable dt, int LV, bool chk)
        {
            if (LV == 1)
            {
                DefectVisibleLV1(dt, chk);
            }
            else if (LV == 2)
            {
                DefectVisibleLV2(dt, chk);
            }
            else if (LV == 3)
            {
                DefectVisibleLV3(dt, chk);
            }
        }

        private void DefectVisibleLV1(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason2.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason2.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }
        private void DefectVisibleLV2(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if (chk == true)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason2.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex3 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel3.Rows[dgLVIndex3].DataItem, "LV_NAME"))) &&
                            dgWipReason2.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLV3(DataTable dt, bool chk)
        {
            if (chk == false)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            if (chk == true)
            {
                if (dgLVIndex1 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel1.Rows[dgLVIndex1].DataItem, "LV_NAME"))) &&
                            dgWipReason2.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
                if (dgLVIndex2 != 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                             Util.NVC(DataTableConverter.GetValue(dgLevel2.Rows[dgLVIndex2].DataItem, "LV_NAME"))) &&
                            dgWipReason2.Rows[i].Visibility == Visibility.Visible)
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DefectVisibleLVAll()
        {
            DataTable dt = (dgWipReason.ItemsSource as DataView).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgWipReason.Rows[i].Visibility = Visibility.Visible;
            }

            DataTable dt2 = (dgWipReason2.ItemsSource as DataView).Table;

            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                dgWipReason2.Rows[i].Visibility = Visibility.Visible;
            }
        }
        #endregion
        #region Event Definition
        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataRowView drv = e.NewValue as DataRowView;

            if (drv != null)
            {
                InitAllClearControl();

                // CSR : C20220622-000589 
                if (cboEquipment.SelectedIndex < 1)
                    _isWoParent = false;
                else
                    _isWoParent = true;

                GetWorkOrder();                                             // W/O 조회
                GetLargeLot();                                              // 대LOT 조회
                GetCurrentMount(Util.NVC(cboEquipment.SelectedValue), "");  // FOIL, SLURRY 조회
                SetIdentInfo();                                             // 로더, 언로더 CARRIER
                SetLotAutoSelected();                                       // LOT자동선택
                SetRollMapEquipment();                                      // Roll Map 대상 설비에 버튼 컨트롤 속성 설정
                GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(GetStatus()))
            {
                Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                return;
            }

            InitAllClearControl();

            _isWoParent = true;

            GetWorkOrder();                                             // W/O 조회
            GetLargeLot();                                              // 대LOT 조회
            GetCurrentMount(Util.NVC(cboEquipment.SelectedValue), "");  // FOIL, SLURRY 조회
            SetIdentInfo();                                             // 로더, 언로더 CARRIER
            SetLotAutoSelected();                                       // LOT자동선택
            GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
        }

        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {
            // 버전정보 체크
            if (!ValidVersion()) return;

            // Roll Map 호출 
            string mainFormPath = "LGC.GMES.MES.COM001";
            string mainFormName = "COM001_RM_CHART_CT";

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] Parameters = new object[10];
            Parameters[0] = Process.COATING;
            Parameters[1] = cboEquipmentSegment.SelectedValue;
            Parameters[2] = cboEquipment.SelectedValue;
            Parameters[3] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            Parameters[4] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetInt();
            Parameters[5] = txtLaneQty.Value;
            Parameters[6] = cboEquipment.Text;
            Parameters[7] = txtVersion.Text.Trim();

            C1Window popupRollMap = obj as C1Window;
            popupRollMap.Closed += new EventHandler(popupRollMap_Closed);
            C1WindowExtension.SetParameters(popupRollMap, Parameters);
            if (popupRollMap != null)
            {
                popupRollMap.ShowModal();
                popupRollMap.CenterOnScreen();
            }
        }

        private void popupRollMap_Closed(object sender, EventArgs e)
        {

        }

        private void btnRollMapInputMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int rowIndex = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            CMM_ELEC_MTRL_INPUT_INFO popMaterialInputInfo = new CMM_ELEC_MTRL_INPUT_INFO();
            popMaterialInputInfo.FrameOperation = FrameOperation;

            btnExtra.IsDropDownOpen = false;
            object[] parameters = new object[10];
            parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
            parameters[1] = cboEquipment.SelectedValue.ToString();
            parameters[2] = Process.COATING;
            parameters[3] = rowIndex < 0 ? string.Empty : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));

            C1WindowExtension.SetParameters(popMaterialInputInfo, parameters);
            Dispatcher.BeginInvoke(new Action(() => popMaterialInputInfo.ShowModal()));
        }

        private void RefreshData(bool isConfirm = false)
        {
            if (isConfirm && _CURRENT_LOTINFO.Rows.Count > 0)
            {
                txtEndLotId.Text = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                int iSamplingCount;
                if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                {
                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                        foreach (DataRow _iRow in _CURRENT_LOTINFO.Rows)
                        {
                            iSamplingCount = 1;
                            string[] sCompany = null;

                            foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                            {
                                iSamplingCount = Util.NVC_Int(items.Key);
                                sCompany = Util.NVC(items.Value).Split(',');
                            }

                            for (int i = 0; i < iSamplingCount; i++)
                                Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.COATING, i > sCompany.Length - 1 ? "" : sCompany[i]);
                        }
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        if (string.Equals(LoginInfo.CFG_CARD_POPUP, "Y") || string.Equals(LoginInfo.CFG_CARD_AUTO, "Y"))
                            PrintHistoryCard();

                    }
                });
            }

            Thread.Sleep(500);

            InitAllClearControl();

            GetWorkOrder();                                             // W/O 조회
            GetLargeLot();                                              // 대LOT 조회
            GetCurrentMount(Util.NVC(cboEquipment.SelectedValue), "");  // FOIL, SLURRY 조회
            SetIdentInfo();                                             // 로더, 언로더 CARRIER
            SetLotAutoSelected();                                       // LOT자동선택
            GetWrkShftUser();                                           // SHIFT_CODE, SHIFT_USER 자동 SET
        }

        private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            e.Handled = false;

            if (chkRun != null)
                chkRun.IsChecked = true;

            if (chkEqpEnd != null)
                chkEqpEnd.IsChecked = true;

            if (chkConfirm != null)
                chkConfirm.IsChecked = true;
        }

        private void OnGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgLargeLot":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;

                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }

                                            InitClearControl();
                                            GetProductLot(dgLargeLot.Rows[e.Cell.Row.Index].DataItem as DataRowView);
                                            ClearDefectLV();
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            InitClearControl();
                                            dgProductLot.ItemsSource = null;

                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            ClearDefectLV();
                                        }
                                        break;
                                }

                                if (dgLargeLot.CurrentCell != null)
                                    dgLargeLot.CurrentCell = dgLargeLot.GetCell(dgLargeLot.CurrentCell.Row.Index, dgLargeLot.Columns.Count - 1);
                                else if (dgLargeLot.Rows.Count > 0)
                                    dgLargeLot.CurrentCell = dgLargeLot.GetCell(dgLargeLot.Rows.Count, dgLargeLot.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgProductLot":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;

                                            InitClearControl();

                                            if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                                return;

                                            dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                            // LOT별 ROLLMAP 속성 구분 (2021-08-18)
                                            SetRollMapLotAttribute(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID").GetString());

                                            GetLotInfo(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetDefectList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetQualityList(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetMaterial(dgProductLot.Rows[e.Cell.Row.Index].DataItem);
                                            GetRemark(dgProductLot.Rows[e.Cell.Row.Index].DataItem);

                                            dgProductLot.SelectedIndex = e.Cell.Row.Index;

                                            if (string.Equals(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT").GetString(), Wip_State.EQPT_END) || string.Equals(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT").GetString(), Wip_State.END))
                                            {
                                                btnRollMap.IsEnabled = true;
                                            }
                                            else
                                            {
                                                btnRollMap.IsEnabled = false;
                                            }

                                            setRollmapResultLinkBtn();  //nathan 2024.01.10 롤맵 코터 수불 실적 적용
                                            if(tiTestcut.Visibility == Visibility.Visible)
                                            {
                                                SetControlTestCut();        //nathan 2024.01.10 롤맵 코터 수불 실적 적용
                                                SelectTestCut(dgTestCut);   //nathan 2024.01.10 롤맵 코터 수불 실적 적용
                                            }
                                            

                                            ClearDefectLV();
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            InitClearControl();

                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;

                                            SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);
                                            ClearDefectLV();
                                        }
                                        break;
                                }

                                if (dgProductLot.CurrentCell != null)
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.CurrentCell.Row.Index, dgProductLot.Columns.Count - 1);
                                else if (dgProductLot.Rows.Count > 0)
                                    dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.Rows.Count, dgProductLot.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (Util.NVC(e.Cell.Column.Name).IsNullOrEmpty())
                                return;

                            // 2020.07.07 공정 Interlock - 범례 표시 공정이 아닌 경우 보완 
                            if (WIPCOLORLEGEND == null)
                                return;

                            // 2020.07.06 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                            SolidColorBrush scbZVersionBack = new SolidColorBrush();
                            SolidColorBrush scbZVersionFore = new SolidColorBrush();

                            SolidColorBrush scbCutBack = new SolidColorBrush();
                            SolidColorBrush scbCutFore = new SolidColorBrush();

                            foreach (DataRow dr in WIPCOLORLEGEND.Rows)
                            {
                                if (dr["COLOR_BACK"].ToString().IsNullOrEmpty() || dr["COLOR_FORE"].ToString().IsNullOrEmpty())
                                {
                                    continue;
                                }

                                if (dr["CODE"].ToString().Equals("Z_VER"))
                                {
                                    scbZVersionBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                                    scbZVersionFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                                }
                                else if (dr["CODE"].ToString().Equals("CUT"))
                                {
                                    scbCutBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                                    scbCutFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                                }
                            }

                            if (string.Equals(e.Cell.Column.Name, "LOTID") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CUT"), "1")
                            || string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CUT"), "L1")
                            || string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CUT"), "R1"))
                            {
                                e.Cell.Presenter.Background = scbCutBack;
                                e.Cell.Presenter.Foreground = scbCutFore;
                            }
                            else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                                Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                            {
                                e.Cell.Presenter.Background = scbZVersionBack;
                                e.Cell.Presenter.Foreground = scbZVersionFore;
                            }
                            // [E20230228-000007] CSR - Marking logic in GMES 주석처리
                            // else
                            // {
                            //     e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            // }
                        }
                    }
                }));
            }
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Tag, "N"))
                            {
                                if ((e.Cell.Row.Index - dataGrid.TopRows.Count) > 0)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Transparent);
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }

                            if (dataGrid.Columns["INPUT_VALUE_TYPE"].Index < e.Cell.Column.Index &&
                                dataGrid.Columns["EQPT_TOTL_QTY"].Index > e.Cell.Column.Index && ((e.Cell.Row.Index - dataGrid.TopRows.Count)) == 2)
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name,
                                             Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) -
                                             Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)));

                                if (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 1].DataItem, e.Cell.Column.Name)) !=
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index - 2].DataItem, e.Cell.Column.Name)))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }

        private void dgWipReason_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    // #RollMap 수정 불가 Cell 삭제 방지
                    if (_isRollMapEquipment)
                    {
                        if (dataGrid.Name.Contains("WipReason") && string.Equals(DataTableConverter.GetValue(dataGrid.CurrentCell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                            return;
                    }

                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        //DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                }
            }
        }

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count > 0)
            {
                C1DataGrid dataGrid = sender as C1DataGrid;
                if (dataGrid != null)
                {
                    // Top Loss 기본 수량 변경 로직 적용 (단순에 차감, 증가분 만큼만 움직이도록 변경)
                    if (string.Equals(dataGrid.Tag, "DEFECT_TOP") && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WEB_BREAK_FLAG"), "Y"))
                    {
                        decimal dTopLossQty = Util.NVC_Decimal(_CURRENT_LOTINFO.Rows[0]["INPUT_TOP_QTY"]) - Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                        decimal dRemainQty = GetSumDefectQty(dataGrid, e.Cell.Row.Index);
                        //decimal dWebBreakQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY")) -
                        //                       Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "RESNQTY"));
                        int TOP_LOSS_BAS_DFCT_APPLY_FLAG_IDX = _Util.GetDataGridRowIndex(dataGrid, "TOP_LOSS_BAS_DFCT_APPLY_FLAG", "Y");
                        if (Util.NVC_Decimal(e.Cell.Value) > (dTopLossQty - dRemainQty))
                        {
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", (dTopLossQty - dRemainQty));
                            if (TOP_LOSS_BAS_DFCT_APPLY_FLAG_IDX > 0)
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[TOP_LOSS_BAS_DFCT_APPLY_FLAG_IDX].DataItem, "RESNQTY", 0);
                            }
                        }
                        else
                        {
                            if (TOP_LOSS_BAS_DFCT_APPLY_FLAG_IDX > 0)
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[TOP_LOSS_BAS_DFCT_APPLY_FLAG_IDX].DataItem, "RESNQTY", (dTopLossQty - dRemainQty) - (Util.NVC_Decimal(e.Cell.Value)));
                            }
                        }
                    }

                    if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                    {
                        if (_isRollMapEquipment)
                        {
                            SetWipReasonCommittedEdit(sender, e);
                            GetSumDefectQty();
                            dgLotInfo.Refresh(false);
                        }
                        else
                        {
                            if (Util.NVC_Decimal(e.Cell.Value) == 0 && Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK")) == false)
                            {
                                GetSumDefectQty();
                                dgLotInfo.Refresh(false);
                                return;
                            }

                            for (int i = 0; i < dataGrid.Rows.Count; i++)
                            {
                                if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK")) == true)
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);

                                    if (e.Cell.Row.Index != i)
                                        DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                }
                            }

                            if (Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY")) ==
                                Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_BACK_QTY")))
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESN_TOT_CHK", true);

                            GetSumDefectQty();
                            dgLotInfo.Refresh(false);
                        }
                    }
                }
            }
        }

        private void SetWipReasonCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Column.Index == dataGrid.Columns["RESNQTY"].Index)
                {
                    // 재 조건 조정 배분 로직 추가 [2021-07-27]
                    DataRow[] row = DataTableConverter.Convert(dataGrid.ItemsSource)
                        .Select("DFCT_CODE='" + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_CODE")) + "' AND PRCS_ITEM_CODE='GRP_QTY_DIST'");

                    if (row.Length > 0)
                    {
                        decimal iCurrQty = 0;
                        decimal iResQty = 0;
                        decimal iInitQty = row.Sum(g => g.Field<decimal>("FRST_AUTO_RSLT_RESNQTY"));

                        decimal iDistQty = DataTableConverter.Convert(dataGrid.ItemsSource).AsEnumerable()
                                                .Where(g => g.Field<string>("DFCT_CODE") == Util.NVC(row[0]["DFCT_CODE"]) &&
                                                                   g.Field<string>("PRCS_ITEM_CODE") != "GRP_QTY_DIST" &&
                                                                   g.Field<string>("RESNCODE") != Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE")))
                                                .Sum(g => Util.NVC_Decimal(g.Field<string>("RESNQTY")));

                        if (iInitQty < (iDistQty + Util.NVC_Decimal(e.Cell.Value)))
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", iInitQty - iDistQty);

                        for (int i = 0; i < dataGrid.Rows.Count; i++)
                        {
                            iCurrQty = 0;
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "DFCT_CODE"), row[0]["DFCT_CODE"]) &&
                                string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST"))
                            {
                                iCurrQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                if (iCurrQty <= (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty))
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                    iResQty += iCurrQty;
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", iCurrQty - (iDistQty + Util.NVC_Decimal(e.Cell.Value) - iResQty));
                                    iResQty = iDistQty + Util.NVC_Decimal(e.Cell.Value);
                                }
                            }
                        }
                    }
                }
            }
        }


        private void dgWipReason_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (string.Equals(e.Column.Name, "COUNTQTY") &&
                    !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    e.Cancel = true;

                if ((string.Equals(e.Column.Name, "COUNTQTY") || string.Equals(e.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Column.Name, "RESNQTY")) &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                    e.Cancel = true;

                if (string.Equals(e.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                    e.Cancel = true;

                // Roll Map 설비인 경우 이벤트 추가
                if (_isRollMapEquipment)
                {
                    // RollMap용 수량 변경 금지 처리 [2021-07-27]
                    if (string.Equals(e.Column.Name, "RESNQTY") &&
                        (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST") ||
                         string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y")))
                        e.Cancel = true;
                }

                // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
                if (string.Equals(Util.NVC(_ProcRollmapEqptDectModifyApplyFlag), "Y") && string.Equals(e.Column.Name, "RESN_TOT_CHK") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "ACTID"), "CHARGE_PROD_LOT") )
                {
                    if (string.Equals(GetProcRollmapEqptDectModifyExceptFlag(Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RESNCODE"))), "Y"))
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void dgWipReason_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if ((string.Equals(e.Cell.Column.Name, "COUNTQTY") || string.Equals(e.Cell.Column.Name, "DFCT_TAG_QTY") || string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") || string.Equals(e.Cell.Column.Name, "RESNQTY")) &&
                                        (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"), "Y") || string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y")))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                if (string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT"))
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));

                                // RollMap ActivityReason 수량 변경 금지 처리 [2021-07-27]
                                if (_isRollMapEquipment)
                                {
                                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9");
                                    if (string.Equals(e.Cell.Column.Name, "RESNQTY") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                                        if (convertFromString != null) e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }

                                // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선 
                                if (string.Equals(Util.NVC(_ProcRollmapEqptDectModifyApplyFlag), "Y") && string.Equals(e.Cell.Column.Name, "RESN_TOT_CHK") && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID"), "CHARGE_PROD_LOT") )
                                {
                                    if (string.Equals(GetProcRollmapEqptDectModifyExceptFlag(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE"))), "Y"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                    }
                                }

                            }
                        }
                    }
                }));
            }
        }


        private void dgWipReason_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }));
            }
        }

        private void dgWipReason2_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            Util.MessageConfirm("SFU5128", (vResult) =>         // %1에 전체 수량을 등록 하시겠습니까?
                            {
                                if (vResult == MessageBoxResult.OK)
                                {
                                    // BACK단선은 전체 체크 시 설비에서 올라온 단선BASE수량만 변경하도록 변경 (실수로 체크 시 TOP/BACK수량이 투입량에 반영되어 크게 왜곡 발생) [2019-12-04]
                                    // 코터 공정 단선 조정 시 투입량 변경으로 전체 불량 등록 시 단선 수 차감하고 등록하도록 수정 [2019-01-13]
                                    decimal dWebBreakQty = Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[_Util.GetDataGridRowIndex(dataGrid, "WEB_BREAK_FLAG", "Y")].DataItem, "FRST_AUTO_RSLT_RESNQTY"));

                                    if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                    {
                                        DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                                        DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "FRST_AUTO_RSLT_RESNQTY"));
                                    }
                                    else
                                    {
                                        DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY",
                                                        Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_BACK_QTY")) - dWebBreakQty);
                                    }

                                    for (int i = 0; i < dataGrid.Rows.Count; i++)
                                    {
                                        if (i != idx)
                                        {
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESN_TOT_CHK", false);
                                            DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "RESNQTY", 0);
                                        }
                                    }
                                    GetSumDefectQty();
                                    dgLotInfo.Refresh(false);
                                }
                                else
                                {
                                    cb.IsChecked = false;
                                    DataTableConverter.SetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESN_TOT_CHK", false);
                                }

                            }, new object[] { DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "RESNNAME") });
                        }
                        else
                        {
                            DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "RESNQTY", 0);
                            GetSumDefectQty();
                            dgLotInfo.Refresh(false);
                        }
                    }
                }
            }));
        }

        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = String.Empty;
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02"));
                //sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                if (Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-').Count() == 3)
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0] + "-" +
                        Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[1];
                }
                else
                {
                    sCLCITEM = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTITEM")).ToString().Split('-')[0];
                }
                sCLCNAME = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME1")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2")).ToString() +
                    Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3")).ToString();
            }
            else
            {
                sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            }
            string sCode = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                    isChangeQuality = true;
                }

                if (string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
                {
                    if (dgQuality2.Visibility == Visibility.Visible)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = null;

                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL02", null);

                        if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM") && Convert.ToDouble(sValue) != Double.NaN)
                        {
                            double inputRate = Convert.ToDouble(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_CONV_RATE"));
                            double input = Convert.ToDouble(GetUnitFormatted(sValue)) * inputRate;

                            // Unloading량은 원래 BACK로딩량 - TOP로딩량임, 하지만 현재 요청한 구조로는 처리가 어려워서 오창 자동차동만 하기 LOGIC을 사용한다고 하여 하기와 같은 로직을 내부적으로 반영
                            // TOP 로딩량 존재 시 : BACK LOADING - (TOP LOADING INPUT VALUE * 환산값 * 2) [소수점을 사용안하고 반을 감안하기 위하여 하기와 같이 적용 [2019-03-18]
                            bool isValueComplete = false;
                            //05.02 - 검사분류 코드 임시 하드코팅 처리
                            if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_ITEM_ID"), "E2000-0002"))
                            {
                                DataRow[] rows = (dgQuality.ItemsSource as DataView).Table.Select(string.Format("INSP_ITEM_ID = '{0}' AND CLSS_NAME2 = '{1}' AND CLSS_NAME3 ='{2}'",
                                    new object[] { "E2000-0001", DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2"), DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3") }));

                                if (rows != null && rows.Length > 0 && !string.IsNullOrWhiteSpace(Util.NVC(rows[0]["CLCTVAL01"])) && Convert.ToDouble(rows[0]["CLCTVAL01"]) > 0)
                                {
                                    if (inputRate.ToString().Contains("5"))
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", (input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                    else
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input - Convert.ToDouble(rows[0]["CLCTVAL01"]));

                                    isValueComplete = true;
                                }
                            }
                            else if (string.Equals(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_ITEM_ID"), "SI017"))
                            {
                                DataRow[] rows = (dgQuality.ItemsSource as DataView).Table.Select(string.Format("INSP_ITEM_ID = '{0}' AND CLSS_NAME2 = '{1}' AND CLSS_NAME3 ='{2}'",
                                    new object[] { "SI016", DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME2"), DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLSS_NAME3") }));

                                if (rows != null && rows.Length > 0 && !string.IsNullOrWhiteSpace(Util.NVC(rows[0]["CLCTVAL01"])) && Convert.ToDouble(rows[0]["CLCTVAL01"]) > 0)
                                {
                                    if (inputRate.ToString().Contains("5"))
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", (input * 2) - Convert.ToDouble(rows[0]["CLCTVAL01"]));
                                    else
                                        DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input - Convert.ToDouble(rows[0]["CLCTVAL01"]));

                                    isValueComplete = true;
                                }
                            }

                            if (isValueComplete == false)
                                DataTableConverter.SetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01", input);

                            C1.WPF.DataGrid.DataGridCell inputCell = dgQuality2.GetCell(caller.CurrentRow.Index, caller.Columns["CLCTVAL01"].Index);

                            if (sLSL != "" && Util.NVC_Decimal(input) < Util.NVC_Decimal(sLSL))
                            {
                                inputCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                inputCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                inputCell.Presenter.FontWeight = FontWeights.Bold;
                            }

                            else if (sUSL != "" && Util.NVC_Decimal(input) > Util.NVC_Decimal(sUSL))
                            {
                                inputCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                inputCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                inputCell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                inputCell.Presenter.Background = new SolidColorBrush(Colors.White);
                                inputCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                inputCell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            isChangeQuality = true;
                        }
                    }
                }
            }
        }

        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (string.Equals(dataGrid.CurrentCell.Column.Name, "CLCTVAL02"))
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        int iRowIdx = dataGrid.CurrentCell.Row.Index;
                        if ((dataGrid.CurrentCell.Row.Index + 1) < dataGrid.GetRowCount())
                            iRowIdx++;

                        C1.WPF.DataGrid.DataGridCell currentCell = dataGrid.GetCell(iRowIdx, dataGrid.CurrentCell.Column.Index);
                        Util.SetDataGridCurrentCell(dataGrid, currentCell);
                        dataGrid.CurrentCell = currentCell;
                        dataGrid.Focus();
                    }
                    else if (e.Key == Key.Delete)
                    {
                        if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                        {
                            // 이동중 DEL키 입력 시는 측정값 초기화하도록 변경
                            if (dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Presenter != null && dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Presenter.Content != null &&
                                dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Value != null)
                            {
                                ((C1NumericBox)dataGrid.GetCell(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index).Presenter.Content).Value = 0;
                            }
                            else
                            {
                                DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, "CLCTVAL01", null);
                            }
                        }
                    }
                }
                else
                {
                    if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                    {
                        dataGrid.EndEdit(true);
                    }
                    else if (e.Key == Key.Delete)
                    {
                        if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                        {
                            DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                            dataGrid.BeginEdit(dataGrid.CurrentCell);
                            dataGrid.EndEdit(true);

                            DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                            if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                            {
                                dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                                dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                    {
                        if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                            dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                    }
                }
            }
        }

        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                                C1.WPF.DataGrid.C1DataGrid grid;
                                grid = p.DataGrid;

                                string sCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INSP_VALUE_TYPE_CODE"));
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));
                                string sCSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSL"));
                                string sLSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL_LIMIT"));
                                string sUSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL_LIMIT"));

                                if (panel != null)
                                {
                                    if (string.Equals(sCode, "NUM"))
                                    {
                                        C1NumericBox numeric = panel.Children[0] as C1NumericBox;

                                        // 재설정
                                        if (string.Equals(txtUnit.Text, "EA"))
                                            numeric.Format = "F1";
                                        else
                                            numeric.Format = GetUnitFormatted();

                                        // SRS요청으로 동별 LIMIT값 설정 [2017-11-30]
                                        if (!string.IsNullOrEmpty(sLSL_Limit) && Util.NVC_Decimal(sLSL_Limit) > 0)
                                            numeric.Minimum = Convert.ToDouble(sLSL_Limit);
                                        else
                                            numeric.Minimum = Double.NegativeInfinity;

                                        if (!string.IsNullOrEmpty(sUSL_Limit) && Util.NVC_Decimal(sUSL_Limit) > 0)
                                            numeric.Maximum = Convert.ToDouble(sUSL_Limit);
                                        else
                                            numeric.Maximum = Double.PositiveInfinity;

                                        if (numeric != null && !string.IsNullOrWhiteSpace(Util.NVC(numeric.Value)) && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
                                            // 프레임버그로 값 재 설정 [2017-12-06]
                                            // 액셀 붙여넣기 기능으로 빈칸이 입력될 경우 Convert클래스 이용 시 오류 발생 문제로 체크용 Function 교체 [2019-01-28]
                                            if (!string.IsNullOrWhiteSpace(sValue) && !string.Equals(sValue, "NaN"))
                                            {
                                                //소수점Separator에 따라 분기(우크라이나 언어)
                                                if (sValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.CurrentCulture.NumberFormat);
                                                else
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture.NumberFormat);
                                            }

                                            if (sLSL != "" && Util.NVC_Decimal(numeric.Value) < Util.NVC_Decimal(sLSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }

                                            else if (sUSL != "" && Util.NVC_Decimal(numeric.Value) > Util.NVC_Decimal(sUSL))
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                            }
                                            else
                                            {
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                            }
                                        }
                                        numeric.IsKeyboardFocusWithinChanged -= OnDataCollectGridFocusChanged;
                                        numeric.IsKeyboardFocusWithinChanged += OnDataCollectGridFocusChanged;
                                        numeric.PreviewKeyDown -= OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.PreviewKeyDown += OnDataCollectGridPreviewItmeKeyDown;
                                        numeric.LostKeyboardFocus -= OnDataCollectGridGotKeyboardLost;
                                        numeric.LostKeyboardFocus += OnDataCollectGridGotKeyboardLost;
                                    }
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }

                            if (string.Equals(e.Cell.Column.Name, "INSP_CONV_RATE"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;

                                if (Convert.ToDouble(e.Cell.Value) == 1)
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                else
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);
                            }
                        }
                    }
                }));
            }
        }

        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            if (string.Equals(e.Cell.Column.Name, "INSP_CONV_RATE"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }

                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }));
            }
        }

        private void OnDataCollectGridFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (Convert.ToBoolean(e.NewValue) == true)
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    int iRowIdx = p.Cell.Row.Index;
                    int iColIdx = p.Cell.Column.Index;
                    C1.WPF.DataGrid.C1DataGrid grid = p.DataGrid;

                    if (grid.CurrentCell.Column.Index != iColIdx)
                        grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        if ((iRowIdx + 1) < (grid.GetRowCount() - 1))
                            grid.ScrollIntoView(iRowIdx + 2, grid.Columns["CLCTVAL01"].Index);

                        if (grid.GetRowCount() > ++iRowIdx)
                        {
                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);

                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Up)
                    {
                        if (grid.GetRowCount() > --iRowIdx)
                        {
                            if (iRowIdx > 0)
                                grid.ScrollIntoView(iRowIdx - 1, grid.Columns["CLCTVAL01"].Index);

                            if (iRowIdx < 0)
                            {
                                e.Handled = true;
                                return;
                            }

                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        int iColIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                        item.Value = double.NaN;

                        C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                        currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                        currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        currentCell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        item.Text = string.Empty;
                        item.SelectedIndex = -1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        grid = p.DataGrid;

                        // 액셀파일 PASTE시 공란PASS없이 전체 붙여넣기 추가 [2019-01-28]
                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line.Trim());

                            iRowIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                isDupplicatePopup = true;
                int iRowIdx = 0;
                int iColIdx = 0;
                int iMeanColldx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    iMeanColldx = dgQuality.Columns["MEAN"].Index;

                    grid = p.DataGrid;


                    C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);


                    string sLSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "LSL"));
                    string sCSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "CSL"));
                    string sUSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "USL"));


                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        if (sLSL != "" && Util.NVC_Decimal(item.Value) < Util.NVC_Decimal(sLSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        else if (sUSL != "" && Util.NVC_Decimal(item.Value) > Util.NVC_Decimal(sUSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            currentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        isChangeQuality = true;
                    }
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox item = sender as ComboBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    isChangeQuality = true;
                }
                else
                    return;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                isDupplicatePopup = false;
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void dgLevel_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {
                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            dgLVIndex1 = e.Cell.Row.Index;

                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }

                                                    if (dgWipReason2.Visibility == Visibility.Visible)
                                                    {
                                                        if (dgWipReason2.ItemsSource != null)
                                                        {
                                                            DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME1")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                                {
                                                                    dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                                                                }
                                                                else
                                                                    dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                            DefectVisibleLV(dt, 1, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 1, false);
                                                    }

                                                    if (dgWipReason2.Visibility == Visibility.Visible)
                                                    {
                                                        if (dgWipReason2.ItemsSource != null)
                                                        {
                                                            DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            DefectVisibleLV(dt, 1, true);
                                                        }
                                                    }

                                                }
                                            }

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (dgLVIndex1 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex1 = e.Cell.Row.Index;
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel1.CurrentCell != null)
                                    dgLevel1.CurrentCell = dgLevel1.GetCell(dgLevel1.CurrentCell.Row.Index, dgLevel1.Columns.Count - 1);
                                else if (dgLevel1.Rows.Count > 0)
                                    dgLevel1.CurrentCell = dgLevel1.GetCell(dgLevel1.Rows.Count, dgLevel1.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            dgLVIndex2 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }
                                                    if (dgWipReason2.Visibility == Visibility.Visible)
                                                    {
                                                        if (dgWipReason2.ItemsSource != null)
                                                        {
                                                            DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME2")).Contains(
                                                                    Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                                {
                                                                    dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                                                                }
                                                                else
                                                                    dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                            DefectVisibleLV(dt, 2, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();

                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 2, false);
                                                    }

                                                    if (dgWipReason2.Visibility == Visibility.Visible)
                                                    {
                                                        if (dgWipReason2.ItemsSource != null)
                                                        {
                                                            DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            DefectVisibleLV(dt, 2, true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (dgLVIndex2 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex2 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel2.CurrentCell != null)
                                    dgLevel2.CurrentCell = dgLevel2.GetCell(dgLevel2.CurrentCell.Row.Index, dgLevel2.Columns.Count - 1);
                                else if (dgLevel2.Rows.Count > 0)
                                    dgLevel2.CurrentCell = dgLevel2.GetCell(dgLevel2.Rows.Count, dgLevel2.Columns.Count - 1);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                            if (chk != null)
                            {
                                switch (Convert.ToString(e.Cell.Column.Name))
                                {
                                    case "CHK":
                                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                            chk.IsChecked = true;
                                            dgLVIndex3 = e.Cell.Row.Index;


                                            for (int i = 0; i < dg.Rows.Count; i++)
                                            {
                                                if (!i.Equals(e.Cell.Row.Index))
                                                {
                                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                                    if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                                    {
                                                        chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                        chk.IsChecked = false;
                                                    }
                                                }
                                            }
                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            if (Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                            {
                                                                dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            else
                                                                dgWipReason.Rows[i].Visibility = Visibility.Collapsed;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                    if (dgWipReason2.Visibility == Visibility.Visible)
                                                    {
                                                        if (dgWipReason2.ItemsSource != null)
                                                        {
                                                            DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                if (Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "AREA_RESN_CLSS_NAME3")).Contains(
                                                                    Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LV_NAME"))))
                                                                {
                                                                    dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                                                                }
                                                                else
                                                                    dgWipReason2.Rows[i].Visibility = Visibility.Collapsed;
                                                            }
                                                            DefectVisibleLV(dt, 3, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("True") &&
                                                    (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                                                        for (int i = 0; i < dt.Rows.Count; i++)
                                                        {
                                                            dgWipReason.Rows[i].Visibility = Visibility.Visible;
                                                        }
                                                        DefectVisibleLV(dt, 3, false);
                                                    }
                                                    if (dgWipReason2.Visibility == Visibility.Visible)
                                                    {
                                                        if (dgWipReason2.ItemsSource != null)
                                                        {
                                                            DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                                                            for (int i = 0; i < dt.Rows.Count; i++)
                                                            {
                                                                dgWipReason2.Rows[i].Visibility = Visibility.Visible;
                                                            }
                                                            DefectVisibleLV(dt, 3, true);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                        {
                                            if (dgLVIndex3 != 0)
                                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

                                            dgLVIndex3 = e.Cell.Row.Index;

                                            if (e.Cell.Row.Index != 0)
                                            {
                                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK")).Equals("False") &&
                                                !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                                {
                                                    if (dgWipReason.ItemsSource != null)
                                                    {
                                                        DefectVisibleLVAll();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (dgLevel3.CurrentCell != null)
                                    dgLevel3.CurrentCell = dgLevel3.GetCell(dgLevel3.CurrentCell.Row.Index, dgLevel3.Columns.Count - 1);
                                else if (dgLevel3.Rows.Count > 0)
                                    dgLevel3.CurrentCell = dgLevel3.GetCell(dgLevel3.Rows.Count, dgLevel3.Columns.Count - 1);
                            }
                        }
                    }));
                    break;
            }
        }

        private void dgLevel_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            switch ((sender as C1DataGrid).Name)
            {

                case "dgLevel1":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex1 = 0;
                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel1.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel2":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex2 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel2.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;

                case "dgLevel3":
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        C1DataGrid dg = sender as C1DataGrid;
                        dgLVIndex3 = 0;

                        if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                        {
                            if (e.Cell.Row.Index == 0)
                            {
                                DataTableConverter.SetValue(dgLevel3.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }));
                    break;
            }
        }

        private void dgMaterial_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (!dgMaterial.CurrentCell.IsEditing)
                {
                    if (dgMaterial.CurrentCell.Column.Name.Equals("MTRLID"))
                    {
                        string sMTRLNAME;
                        string vMTRLID = Util.NVC(DataTableConverter.GetValue(dgMaterial.CurrentRow.DataItem, "MTRLID"));

                        if (vMTRLID.Equals(""))
                        {
                            return;
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("MTRLID", typeof(string));

                            DataRow row = dt.NewRow();
                            row["MTRLID"] = vMTRLID;
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL_MTRLDESC", "INDATA", "RSLTDT", dt);
                            if (result.Rows.Count > 0)
                            {
                                sMTRLNAME = result.Rows[0]["MTRLDESC"].ToString();
                                DataTableConverter.SetValue(dgMaterial.Rows[dgMaterial.SelectedIndex].DataItem, "MTRLDESC", sMTRLNAME);

                                DataTable dt2 = (dgMaterial.ItemsSource as DataView).Table;
                                Util.GridSetData(dgMaterial, dt2, FrameOperation, true);

                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // FOIL 선택
        private void radFoil_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radBtn = sender as RadioButton;
            if (radBtn != null)
            {
                if (cboEquipment.SelectedIndex < 1)
                {
                    radBtn.IsChecked = false;
                    return;
                }

                Grid grid = null;
                if (string.Equals(radBtn.Name, "radFoil1"))
                    grid = txtCore2.Parent as Grid;
                else if (string.Equals(radBtn.Name, "radFoil2"))
                    grid = txtCore1.Parent as Grid;

                if (grid == null)
                    return;

                RadioButton unFoilBtn = grid.Children[grid.Children.Count - 1] as RadioButton;

                if (unFoilBtn != null)
                    unFoilBtn.IsChecked = false;
            }
        }

        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            DataRowView drv = dataitem.DataItem as DataRowView;
            string sInputLot;
            string sChildSeq;
            string sLot;

            try
            {
                sInputLot = drv["LOTID_PR"].ToString().Equals(string.Empty) ? drv["LOTID"].ToString() : drv["LOTID_PR"].ToString();
            }
            catch
            {
                sInputLot = string.Empty;
            }

            try
            {
                sChildSeq = string.IsNullOrEmpty(drv["CUT_ID"].ToString()) ? "1" : drv["CUT_ID"].ToString();
            }
            catch
            {
                sChildSeq = "1";
            }

            try
            {
                sLot = drv["LOTID"].ToString();
            }
            catch
            {
                sLot = string.Empty;
            }

            if (!string.IsNullOrEmpty(sInputLot) && !string.IsNullOrEmpty(sChildSeq))
            {
                // 모두 Uncheck 처리 및 동일 자LOT의 경우는 Check 처리.
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    if (dataitem.Index != i)
                    {
                        if (sInputLot == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID_PR")) &&
                            sChildSeq == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CUT_ID")))
                        {
                            if (sInputLot.Equals(""))
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                    dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (bUncheckAll)
                                {
                                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                         dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                         (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                                }
                                else
                                {

                                    if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                        dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                                }

                            }
                        }
                        else
                        {
                            if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;

                            DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }
            return true;
        }

        private void tcDataCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.RemovedItems.Count > 0)
            {
                C1TabItem olditem = e.RemovedItems[0] as C1TabItem;
                if (olditem != null)
                {
                    if (string.Equals(olditem.Name, "tiWipReason"))
                    {
                        dgWipReason.EndEdit(true);
                        if (dgWipReason2.Visibility == Visibility.Visible)
                            dgWipReason2.EndEdit(true);
                    }
                }
            }
        }

        private void dgRemark_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                if (e.Cell.Row.Index == 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkLostKeyboardFocus;
                        }
                    }
                }
                else if (e.Cell.Row.Index > 0 && e.Cell.Column.Index == 1)
                {
                    Grid grid = e.Cell.Presenter.Content as Grid;

                    if (grid != null)
                    {
                        TextBox remarkText = grid.Children[0] as TextBox;

                        if (remarkText != null)
                        {
                            remarkText.LostKeyboardFocus -= OnRemarkChildLostKeyboardFocus;
                            remarkText.LostKeyboardFocus += OnRemarkChildLostKeyboardFocus;
                        }
                    }
                }
            }
        }

        private void OnRemarkLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }

        private void OnRemarkChildLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
                return;

            TextBox remarkText = sender as TextBox;

            if (remarkText != null && !string.IsNullOrEmpty(remarkText.Text))
                isChangeRemark = true;
        }
        #endregion
        #region User Method
        private string GetStatus()
        {
            var status = new List<string>();

            if (chkWait.IsChecked == true)
                status.Add(chkWait.Tag.ToString());

            if (chkRun.IsChecked == true)
                status.Add(chkRun.Tag.ToString());

            if (chkEqpEnd.IsChecked == true)
                status.Add(chkEqpEnd.Tag.ToString());

            if (chkConfirm.IsChecked == true)
                status.Add(chkConfirm.Tag.ToString());

            return string.Join(",", status);
        }

        private void GetWorkOrder()
        {
            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                wo.FrameOperation = FrameOperation;
                wo.EQPTSEGMENT = Util.NVC(cboEquipmentSegment.SelectedValue);
                wo.EQPTID = Util.NVC(cboEquipment.SelectedValue);
                wo.PROCID = Process.COATING;
                // CSR : C20220622-000589 
                if (_isWoParent)
                {
                    wo._UCParent = this;
                    wo.sUiType = "UI_ELEC002";
                }

                wo.GetWorkOrder();

                if (wo.dgWorkOrder.Rows.Count <= 0)
                    return;

                CheckLaneQtyAble(Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRODID")));
            }
        }

        private void SetWipColorLegendCombo()
        {
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow inRow = inTable.NewRow();
            inRow["LANGID"] = LoginInfo.LANGID;
            inRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            inRow["PROCID"] = Process.COATING;

            inTable.Rows.Add(inRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem);
            }
            cboColor.SelectedIndex = 0;

            WIPCOLORLEGEND = dtResult;
        }

        private void SetLotAutoSelected()
        {
            if (dgLargeLot.Visibility == Visibility.Visible)
            {
                if (dgLargeLot != null && dgLargeLot.Rows.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridCell currCell = dgLargeLot.GetCell(0, dgLargeLot.Columns["CHK"].Index);

                    if (currCell != null)
                    {
                        dgLargeLot.SelectedIndex = currCell.Row.Index;
                        dgLargeLot.CurrentCell = currCell;
                    }
                }
            }

            if (dgProductLot.Visibility == Visibility.Visible)
            {
                if (dgProductLot != null && dgProductLot.Rows.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridCell currCell = dgProductLot.GetCell(0, dgProductLot.Columns["CHK"].Index);

                    if (currCell != null)
                    {
                        dgProductLot.SelectedIndex = currCell.Row.Index;
                        dgProductLot.CurrentCell = currCell;

                        if (Equals(DataTableConverter.GetValue(dgProductLot.Rows[currCell.Row.Index].DataItem, "WIPSTAT"), Wip_State.END))
                        {
                            btnRollMap.IsEnabled = true;
                        }
                        else
                        {
                            btnRollMap.IsEnabled = false;
                        }
                    }
                }
            }
        }

        private bool IsValidDispatcher()
        {
            bool bRet = true;

            if (!IsValidGoodQty())
                return false;

            if (!IsValidLimitQty())
                return false;

            if (!IsValidTopQty())
                return false;

            if (!ValidShift())
                return false;

            if (!ValidOperator())
                return false;

            if (IsZeroGoodQty()) //양품량이 0이면 추가 Validation없이 return
                return true;

            if (!ValidQualityRequired())
                return false;

            if (!ValidQualitySpecRequired())
                return false;

            if (!IsValidCollect())
                return false;

            /*
            if (!ValidQualitySpec("Hold"))
            {
                //자동HOLD되도록 설정된 품질검사 결과가 기준치를 만족하지 못했습니다. 완성랏이 홀드됩니다. 계속하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8185"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CMM_ELEC_HOLD_YN wndPopup = new CMM_ELEC_HOLD_YN();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndHoldChk_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }

                        return;
                    }
                });

                return false;
            }

            //LSL, USL SPEC HOLD
            if (!ValidQualitySpecExists())
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8186"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CMM_ELEC_HOLD_YN wndPopup = new CMM_ELEC_HOLD_YN();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndHoldChk_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }

                        return;
                    }
                });

                return false;
            }
            */

            return bRet;
        }

        private bool IsValidGoodQty()
        {
            if (dgLotInfo.GetRowCount() > 0)
            {
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) < 0)
                        {
                            Util.MessageValidation("SFU5129");  //양품량이 0보다 작습니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IsZeroGoodQty()
        {
            if (dgLotInfo.GetRowCount() > 0)
            {
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsValidLimitQty()
        {
            if (dgLotInfo.GetRowCount() > 0)
            {
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_BACK_QTY")) !=
                            (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) +
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM"))))
                        {
                            Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IsValidTopQty()
        {
            if (dgLotInfo.GetRowCount() > 0)
            {
                // 단선수량이 투입량에 반영되면서 TOP LOSS 체크 로직 변경 [2019-12-04]
                if ((Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.Rows.Count - 1].DataItem, "INPUT_TOP_QTY")) + GetDiffWebBreakQty(dgWipReason, "DEFECT_LOT", "TOP")) != 0)
                //if (Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.Rows.Count - 1].DataItem, "INPUT_TOP_QTY")) != 0)
                {
                    Util.MessageValidation("SFU5130");  //Top의 수량 차이가 0이 되도록 불량/Loss 수량 수정 바랍니다.
                    return false;
                }
            }
            return true;
        }

        private bool ValidShift()
        {
            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");  //작업조를 입력하세요.
                return false;
            }

            return true;
        }

        private bool ValidOperator()
        {
            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                return false;
            }

            return true;
        }

        private bool ValidQualityRequired()
        {
            List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality, dgQuality2 };
            foreach (C1DataGrid dg in lst)
            {
                DataView view = DataTableConverter.Convert(dg.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    bool isValid = false;
                    DataRow[] filterRows = DataTableConverter.Convert(dg.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");

                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (!string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) && !string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (isValid == false)
                    {
                        Util.MessageValidation("SFU3601", sItemName);   //해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidQualitySpecRequired()
        {
            List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality, dgQuality2 };
            foreach (C1DataGrid dg in lst)
            {
                DataView view = DataTableConverter.Convert(dg.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    string itemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(dg.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);   //해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool ValidQualitySpec(string validType)
        {
            bool bRet = true;
            try
            {
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;

                List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality, dgQuality2 };
                foreach (C1DataGrid dg in lst)
                {
                    DataTable dt = dg.ItemsSource == null ? null : ((DataView)dg.ItemsSource).ToTable();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            LSL = dt.Rows[i]["LSL"].ToString();
                            USL = dt.Rows[i]["USL"].ToString();
                            AUTO_HOLD_FLAG = dt.Rows[i]["AUTO_HOLD_FLAG"].ToString();

                            //yield LSL USL => 또 팝업 => 예를 누르면 실적확정되면서 무조건 홀드

                            if (!dt.Rows[i]["CLCTVAL01"].ToString().Equals("NaN"))
                            {
                                CLCTVAL = dt.Rows[i]["CLCTVAL01"].ToString();
                            }

                            if (!String.IsNullOrWhiteSpace(LSL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                            {
                                //validType이 Hold면 자동보류여부를 체크하고
                                //아니면 체크 안하고
                                if (Util.NVC_Decimal(LSL) > 0 && Util.NVC_Decimal(LSL) > Util.NVC_Decimal(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                                {
                                    bRet = false;
                                }
                            }
                            if (!String.IsNullOrWhiteSpace(USL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                            {
                                if (Util.NVC_Decimal(USL) > 0 && Util.NVC_Decimal(USL) < Util.NVC_Decimal(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                                {
                                    bRet = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = true;
            }
            return bRet;
        }

        private bool ValidQualitySpecExists()
        {
            bool bRet = true;
            try
            {
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;
                string SPEC_TYPE_CODE = string.Empty;  //201.03.11 추가

                List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality, dgQuality2 };
                foreach (C1DataGrid dg in lst)
                {
                    DataTable dt = dg.ItemsSource == null ? null : ((DataView)dg.ItemsSource).ToTable();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            LSL = dt.Rows[i]["LSL"].ToString();
                            USL = dt.Rows[i]["USL"].ToString();
                            AUTO_HOLD_FLAG = dt.Rows[i]["AUTO_HOLD_FLAG"].ToString();
                            SPEC_TYPE_CODE = dt.Rows[i]["SPEC_TYPE_CODE"].ToString();  //2021.03.11 추가

                            if (!dt.Rows[i]["CLCTVAL01"].ToString().Equals("NaN"))
                            {
                                CLCTVAL = dt.Rows[i]["CLCTVAL01"].ToString();
                            }

                            if (AUTO_HOLD_FLAG.Equals("Y") && !string.IsNullOrWhiteSpace(CLCTVAL))
                            {
                                if (SPEC_TYPE_CODE == "B")
                                {
                                    if (string.IsNullOrWhiteSpace(LSL) || string.IsNullOrWhiteSpace(USL))
                                    {
                                        bRet = false;
                                        break;
                                    }
                                }
                                else if (SPEC_TYPE_CODE == "U")
                                {
                                    if (string.IsNullOrWhiteSpace(USL))
                                    {
                                        bRet = false;
                                        break;
                                    }
                                }
                                else if (SPEC_TYPE_CODE == "L")
                                {
                                    if (string.IsNullOrWhiteSpace(LSL))
                                    {
                                        bRet = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = true;
            }

            return bRet;
        }

        private bool ValidVersion()
        {
            if (string.IsNullOrEmpty(txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                return false;
            }
            return true;
        }

        private bool IsValidCollect()
        {
            if (isChangeQuality)
            {
                Util.MessageValidation("SFU1999");  //품질 정보를 저장하세요.
                return false;
            }

            if (isChangeMaterial)
            {
                Util.MessageValidation("SFU1818");  //자재 정보를 저장하세요.
                return false;
            }

            if (isChangeRemark)
            {
                Util.MessageValidation("SFU2977");  //특이사항 정보를 저장하세요.
                return false;
            }
            return true;
        }

        #region # Roll Map
        private void SetRollMapEquipment()
        {
            _isRollMapResultLink = IsRollMapResultApply();
            _isRollMapEquipment = IsEquipmentAttr(Util.NVC(cboEquipment.SelectedValue));
            _isOriginRollMapEquipment = _isRollMapEquipment;

            // # Roll Map 대상설비에 따른 컨트롤 정의
            VisibleRollMapMode();

            //nathan 2024.01.10 롤맵 코터 수불 실적 적용 start
            // TEST CUT 사용 사이트만 TEST CUT Tab 보이도록 함

            _isTestCutApply = true;

            bool bTestCutUse = IsCommoncodeUse("TEST_CUT_HIST_AREA", LoginInfo.CFG_AREA_ID);

            if (bTestCutUse)
                tiTestcut.Visibility = Visibility.Visible;
            else
                tiTestcut.Visibility = Visibility.Collapsed;

            //nathan 2024.01.10 롤맵 코터 수불 실적 적용 end
        }

        private bool IsEquipmentAttr(string equipmentCode)
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(equipmentCode).Select();
                if (dr?.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            return false;
        }

        private bool IsRollMapResultApply()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Process.COATING;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (CommonVerify.HasTableRow(dtResult))
                    if (Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            return false;
        }

        private void SetRollMapLotAttribute(string lotId)
        {
            try
            {
                bool isRollMapModeChange = false;

                if (_isOriginRollMapEquipment == true)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));

                    DataRow row = dt.NewRow();
                    row["LOTID"] = lotId;
                    dt.Rows.Add(row);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                    if (CommonVerify.HasTableRow(result))
                    {
                        // 실적 연계 여부에 따라 처리 변경 [2021-10-18]
                        if (_isRollMapResultLink == true)
                        {
                            if (Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                            {
                                if (_isRollMapEquipment == false) isRollMapModeChange = true;
                                _isRollMapEquipment = true;
                            }
                            else
                            {
                                if (_isRollMapEquipment == true) isRollMapModeChange = true;
                                _isRollMapEquipment = false;
                            }

                            if (isRollMapModeChange == true)
                                VisibleRollMapMode();
                        }
                        else
                        {
                            _isRollMapEquipment = false;
                            VisibleRollMapMode();

                            if (Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                                btnRollMap.Visibility = Visibility.Visible;
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetEquipmentCode(string lotId, string wipSeq)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIPHISTORY", "RQSTDT", "RSLTDT", inTable);
        }

        private DataTable GetRollMapInputLotCode(string lotId)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = 1;
            dr["EQPT_MEASR_PSTN_ID"] = "UW";
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_COLLECT_INFO", "RQSTDT", "RSLTDT", inTable);
        }

        private void VisibleRollMapMode()
        {

            dgMaterial.Columns["REMAIN_QTY"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["STRT_PSTN"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["END_PSTN"].Visibility = Visibility.Collapsed;

            if (_isRollMapEquipment)
            {
                btnRollMap.Visibility = Visibility.Visible;
                btnAddMaterial.Visibility = Visibility.Collapsed;
                btnDeleteMaterial.Visibility = Visibility.Collapsed;
                btnSaveMaterial.Visibility = Visibility.Collapsed;
                dgMaterial.IsReadOnly = true;
                dgMaterial.Columns["CHK"].Visibility = Visibility.Collapsed;
                btnRollMapInputMaterial.Visibility = Visibility.Visible; // 2021.08.10 Visible 처리
            }
            else
            {
                btnRollMap.Visibility = Visibility.Collapsed;
                btnAddMaterial.Visibility = Visibility.Visible;
                btnDeleteMaterial.Visibility = Visibility.Visible;
                btnSaveMaterial.Visibility = Visibility.Visible;
                dgMaterial.IsReadOnly = false;
                dgMaterial.Columns["CHK"].Visibility = Visibility.Visible;
                btnRollMapInputMaterial.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveDefectForRollMap()
        {
            //string bizRuleName = string.Equals(procId, Process.COATING) ? "BR_PRD_REG_DATACOLLECT_DEFECT_CT" : "BR_PRD_REG_DATACOLLECT_DEFECT_RP";
            const string bizRuleName = "BR_PRD_REG_DATACOLLECT_DEFECT_CT";
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = cboEquipment.SelectedValue;
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(inDataRow);

            DataTable IndataTable = inDataSet.Tables.Add("IN_LOT");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));

            inDataRow = IndataTable.NewRow();
            inDataRow["LOTID"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataRow["WIPSEQ"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetInt();
            IndataTable.Rows.Add(inDataRow);

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetRollMapHold(string processCode)
        {
            string bizRuleName = string.Equals(processCode, Process.ROLL_PRESSING) ? "DA_PRD_SEL_ROLLMAP_RP_HOLD" : "DA_PRD_SEL_ROLLMAP_CT_HOLD";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LOTID"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            dr["WIPSEQ"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetDecimal();
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }
        #endregion


        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            // 전역변수 SET
            DataRow dataRow = _CURRENT_LOTINFO.NewRow();
            dataRow["LOTID"] = Util.NVC(rowview["LOTID"]);
            dataRow["WIPSEQ"] = Util.NVC_Int(rowview["WIPSEQ"]);
            dataRow["WIPSTAT"] = Util.NVC(rowview["WIPSTAT"]);
            dataRow["LOTID_PR"] = Util.NVC(rowview["LOTID_PR"]);
            dataRow["CSTID"] = Util.NVC(rowview["CSTID"]);
            dataRow["OUT_CSTID"] = Util.NVC(rowview["OUT_CSTID"]);
            dataRow["CUT_ID"] = Util.NVC(rowview["CUT_ID"]);
            dataRow["PRODID"] = Util.NVC(rowview["PRODID"]);
            dataRow["CUT"] = Util.NVC(rowview["CUT"]);
            dataRow["INPUT_BACK_QTY"] = Util.NVC_Decimal(rowview["INPUT_BACK_QTY"]);
            dataRow["INPUT_TOP_QTY"] = Util.NVC_Decimal(rowview["INPUT_TOP_QTY"]);
            _CURRENT_LOTINFO.Rows.Add(dataRow);

            // SET VERSION
            DataTable versionDt = GetProcessVersion(Util.NVC(rowview["LOTID"]), Util.NVC(rowview["PRODID"]));
            if (versionDt.Rows.Count > 0)
            {
                txtVersion.Text = Util.NVC(versionDt.Rows[0]["PROD_VER_CODE"]);
                txtLaneQty.Value = string.IsNullOrEmpty(Util.NVC(versionDt.Rows[0]["LANE_QTY"])) ? 0 : Convert.ToInt16(Util.NVC(versionDt.Rows[0]["LANE_QTY"]));
            }

            // CWA 전수 불량 추가
            txtCurLaneQty.Value = getCurrLaneQty(Util.NVC(rowview["LOTID"]));

            if (getDefectLane(LoginInfo.CFG_EQSG_ID))
                btnSaveRegDefectLane.Visibility = Visibility.Visible;

            btnSaveRegDefectLane.Visibility = Visibility.Collapsed; // TO-DO : 전수불량 사용불가로 비활성 처리 함 [2019-11-11]

            btnSaveRegDefectLane.IsEnabled = true;

            // SET TIME
            txtStartDateTime.Text = Convert.ToDateTime(Util.NVC(rowview["WIPDTTM_ST"])).ToString("yyyy-MM-dd HH:mm");

            if (string.IsNullOrEmpty(Util.NVC(rowview["WIPDTTM_ED"])))
                txtEndDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            else
                txtEndDateTime.Text = Convert.ToDateTime(Util.NVC(rowview["WIPDTTM_ED"])).ToString("yyyy-MM-dd HH:mm");

            if (txtWorkDate != null)
                SetCalDate();

            if (string.Equals(rowview["FINAL_CUT_FLAG"], "Y"))
                chkFinalCut.IsChecked = true;

            txtUnit.Text = rowview["UNIT_CODE"].ToString();

            // 완공 상태일 경우만 불량/Loss 저장 가능
            if (string.Equals(rowview["WIPSTAT"], Wip_State.END))
            {
                btnSaveWipReason.IsEnabled = true;
                btnPublicWipSave.IsEnabled = true;
            }

            //20210903 오화백 FastTrack 체크박스 추가

            _FastTrackLot = Util.NVC(rowview["LOTID"]);

            if (ChkFastTrackOWNER())
            {
                titleFastTrack.Visibility = Visibility.Visible;
                chkFastTrack.Visibility = Visibility.Visible;
                chkFastTrack.IsChecked = CheckFastTrackLot();

            }
            else
            {
                titleFastTrack.Visibility = Visibility.Collapsed;
                chkFastTrack.Visibility = Visibility.Collapsed;
            }




            SetUnitFormatted();
            SetResultInfo(rowview);     // SET LOT GRID
        }

        private void SetUnitFormatted()
        {
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                string sFormatted = string.Empty;
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }

                txtInputQty.Format = sFormatted;
                txtParentQty.Format = sFormatted;
                txtRemainQty.Format = sFormatted;

                if (dgLotInfo.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgLotInfo.Columns.Count; i++)
                        if (dgLotInfo.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgLotInfo.Columns[i].Tag, "N"))
                            // 코터공정중에 EA인것은 BACK작업시 TOP의 1/2로직으로 인하여 수정될 여지가 있어서 해당 로직 고정
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgLotInfo.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgLotInfo.Columns[i]).Format = sFormatted;

                if (dgWipReason.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgWipReason.Columns.Count; i++)
                        if (dgWipReason.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReason.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)dgWipReason.Columns[i]).Format = sFormatted;

                if (dgWipReason2.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgWipReason2.Columns.Count; i++)
                        if (dgWipReason2.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgWipReason2.Columns[i].Tag, "N"))
                            ((DataGridNumericColumn)dgWipReason2.Columns[i]).Format = sFormatted;

                if (dgQuality.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgQuality.Columns.Count; i++)
                        if (dgQuality.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQuality.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgQuality.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgQuality.Columns[i]).Format = sFormatted;

                if (dgQuality2.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgQuality2.Columns.Count; i++)
                        if (dgQuality2.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgQuality2.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgQuality2.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgQuality2.Columns[i]).Format = sFormatted;

                if (dgMaterial.Visibility == Visibility.Visible)
                    for (int i = 0; i < dgMaterial.Columns.Count; i++)
                        if (dgMaterial.Columns[i].GetType() == typeof(DataGridNumericColumn) && !string.Equals(dgMaterial.Columns[i].Tag, "N"))
                            if (string.Equals(txtUnit.Text, "EA"))
                                ((DataGridNumericColumn)dgMaterial.Columns[i]).Format = "F1";
                            else
                                ((DataGridNumericColumn)dgMaterial.Columns[i]).Format = sFormatted;
            }
        }

        private string GetUnitFormatted()
        {
            string sFormatted = "0";
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
        }

        private string GetUnitFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (txtUnit.Text)
            {
                case "KG":
                    sFormatted = "{0:#,##0.000}";
                    break;

                case "M":
                    sFormatted = "{0:#,##0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:#,##0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private void SetCauseTitle()
        {
            int causeqty = 0;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable dt = (dgWipReason.ItemsSource as DataView).Table;
                for (int i = dgWipReason.TopRows.Count; i < dt.Rows.Count + dgWipReason.TopRows.Count; i++)
                {
                    string resnname = DataTableConverter.GetValue(dgWipReason.Rows[i].DataItem, "RESNNAME").ToString();
                    if (resnname.IndexOf("*") == 1)
                        causeqty++;
                }
                if (causeqty > 0)
                {
                    if ((grdDataCollect.Children[0] as UcDataCollect).lblTop.Visibility == Visibility.Visible)
                    {
                        (grdDataCollect.Children[0] as UcDataCollect).lblTop.Text = ObjectDic.Instance.GetObjectName("Top(*는 타공정 귀속)");
                    }
                    else
                    {
                        (grdDataCollect.Children[0] as UcDataCollect).lblTop.Visibility = Visibility.Visible;
                        (grdDataCollect.Children[0] as UcDataCollect).lblTop.Text = ObjectDic.Instance.GetObjectName("(*는 타공정 귀속)");
                    }
                }
            }
            if (dgWipReason2.ItemsSource != null)
            {
                DataTable dt = (dgWipReason2.ItemsSource as DataView).Table;
                for (int i = dgWipReason2.TopRows.Count; i < dt.Rows.Count + dgWipReason2.TopRows.Count; i++)
                {
                    string resnname = DataTableConverter.GetValue(dgWipReason2.Rows[i].DataItem, "RESNNAME").ToString();
                    if (resnname.IndexOf("*") == 1)
                        causeqty++;
                }
                if (causeqty > 0)
                    (grdDataCollect.Children[0] as UcDataCollect).lblBack.Text = ObjectDic.Instance.GetObjectName("Back(*는 타공정 귀속)");
            }
        }

        private bool IsWorkOrderValid()
        {
            bool IsValid = true;
            if (grdWorkOrder.Children[0] is UC_WORKORDER_CWA)
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                if (new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK") != -1)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

                    if (Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "WOID")) != Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WOID")))
                        {
                            Util.MessageValidation("SFU1436");
                            IsValid = false;
                        }
                        else
                        {
                            IsValid = true;
                        }
                    }
                }
            }
            return IsValid;
        }

        private bool IsCoaterProdVersion()
        {
            // 1. LOT이 선택되었는지 확인
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return false;

            // 2. 입력된 VERSION 체크
            if (string.IsNullOrEmpty(txtVersion.Text))
                return false;

            // 3. 양산버전 이외는 체크 안함
            System.Text.RegularExpressions.Regex engRegex = new System.Text.RegularExpressions.Regex(@"[a-zA-Z]");
            if (engRegex.IsMatch(txtVersion.Text.Substring(0, 1)) == true)
                return false;

            // 4. 1번 CUT인지 확인
            string sCut = Util.NVC(_CURRENT_LOTINFO.Rows[0]["CUT"]);
            if (string.IsNullOrEmpty(sCut) || !string.Equals(sCut, "1"))
                return false;

            return true;
        }

        private void GetSumDefectQty()
        {
            if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.END))
                return;

            if (_CURRENT_LOTINFO.Rows.Count > 0)
            {
                decimal dTopInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_TOP_QTY"));
                decimal dBackInputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUT_BACK_QTY"));
                decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LANE_QTY"));
                decimal dTopDefectQty = GetSumDefectQty(dgWipReason, "DEFECT_LOT", "TOP");
                decimal dTopLossQty = GetSumDefectQty(dgWipReason, "LOSS_LOT", "TOP");
                decimal dTopChargeProdQty = GetSumDefectQty(dgWipReason, "CHARGE_PROD_LOT", "TOP");
                decimal dTopTotalQty = dTopDefectQty + dTopLossQty + dTopChargeProdQty;
                decimal dBackDefectQty = GetSumDefectQty(dgWipReason2, "DEFECT_LOT", "BACK");
                decimal dBackLossQty = GetSumDefectQty(dgWipReason2, "LOSS_LOT", "BACK");
                decimal dBackChargeProdQty = GetSumDefectQty(dgWipReason2, "CHARGE_PROD_LOT", "BACK");
                decimal dBackWebBreakQty = GetDiffWebBreakQty(dgWipReason2, "DEFECT_LOT", "BACK");
                decimal dBackTotalQty = dBackDefectQty + dBackLossQty + dBackChargeProdQty;

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_TOP_DEFECT", dTopDefectQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_TOP_LOSS", dTopLossQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_TOP_CHARGEPRD", dTopChargeProdQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_TOP_DEFECT_SUM", dTopTotalQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_BACK_DEFECT", dBackDefectQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_BACK_LOSS", dBackLossQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_BACK_CHARGEPRD", dBackChargeProdQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM", dBackTotalQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY", ((dBackInputQty - dBackWebBreakQty) - dBackTotalQty));
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY2", (((dBackInputQty - dBackWebBreakQty) - dBackTotalQty) * dLaneQty));
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_TOP_QTY", dTopTotalQty);
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_BACK_QTY", dBackInputQty - dBackWebBreakQty);

                        //TODO : 오창 전극의 경우 _isRollMapEquipment 의 경우 GOODQTY [양품수량, C/Roll] 가져오는 수식이 있음. 확인 필요 함.
                        //if (isRollMapEquipment) DataTableConverter.SetValue(LOTINFO_GRID.Rows[i].DataItem, "GOODQTY", Convert.ToDouble(DataTableConverter.GetValue(LOTINFO_GRID.Rows[i].DataItem, "INPUTQTY")) - totalSum);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY2",
                            Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY")) * dLaneQty);
                    }
                }
                // Summary 추가
                txtInputQty.Value = Convert.ToDouble(dBackInputQty - dBackTotalQty);
                txtParentQty.Value = Convert.ToDouble(dBackInputQty);
                txtRemainQty.Value = Convert.ToDouble(dBackInputQty - (dBackInputQty - dBackTotalQty));
            }
        }


        private decimal GetSumDefectQty(C1DataGrid dataGrid, int iRow)
        {
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (iRow != i)
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                            if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "TOP_LOSS_BAS_DFCT_APPLY_FLAG"), "Y"))
                                if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));

            return dSumQty;
        }

        private decimal GetSumDefectQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
            return dSumQty;
        }

        private decimal GetDiffWebBreakQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY")) -
                                        Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
            return dSumQty;
        }
        #endregion
        #region DA Biz Call
        // Manual Mode 체크 [USERTYPE = 'G'인 경우만 활성화
        private void SetManualMode(List<Button> buttons)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.Equals(Util.NVC(row["USERTYPE"]), "G"))
                        {
                            foreach (Button button in buttons)
                                if (string.Equals(Util.NVC(button.Tag), Util.NVC(row["USERTYPE"])))
                                    button.Visibility = Visibility.Visible;

                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 동별 공통코드 사용 유무 
        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return false;
        }

        private void SetCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "RQSTDT", "OUTDATA", RQSTDT);

                if (result.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["CALDATE"])))
                {
                    txtWorkDate.Text = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = Convert.ToDateTime(result.Rows[0]["CALDATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtWorkDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    txtWorkDate.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void CheckLaneQtyAble(string sProdID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LANE_QTY_EDITABLE_CT_MODEL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count <= 0)
                {
                    txtLaneQty.IsEnabled = false;

                    return;
                }
                else
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        if (string.Equals(sProdID, row["CBO_CODE"]))
                        {
                            txtLaneQty.IsEnabled = true;
                            break;
                        }
                        else
                        {
                            txtLaneQty.IsEnabled = false;
                        }
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 대LOT 조회
        private void GetLargeLot()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Process.COATING;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COATMLOT_LOT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgLargeLot, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // OUT LOT 조회
        private void GetProductLot(DataRowView drv = null)
        {
            try
            {
                if (string.IsNullOrEmpty(GetStatus()))
                {
                    Util.MessageValidation("SFU1438");  //WIP 상태를 선택하세요.
                    return;
                }

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("LOTID_LARGE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Process.COATING;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["WIPSTAT"] = GetStatus();
                dr["LOTID_LARGE"] = drv["LOTID_LARGE"].ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_ELEC", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgProductLot, dtResult, FrameOperation, true);

                // [E20230228-000007] CSR - Marking logic in GMES
                SetCutLotSampleQAFalgBackground();

            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        // [E20230228-000007] CSR - Marking logic in GMES
        private void SetCutLotSampleQAFalgBackground()
        {
            // 속도 문제 로 인해 dgProductLot_LoadedCellPresenter에서 구현하지 않음
            try
            {                 
                if (dgProductLot != null && dgProductLot.Rows.Count > 0)
                {                

                    for (int i = 0; i < dgProductLot.Rows.Count; i++)
                    {
                        if (dgProductLot.GetCell(i, dgProductLot.Columns["LOTID"].Index).Presenter != null)
                        {
                            if (string.Equals(GetCutLotSampleQAFalg(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")), Process.COATING), "Y"))
                            {
                                for (int k = 0; k < dgProductLot.Columns.Count; k++)
                                {
                                    if (dgProductLot.Columns[k].Visibility == Visibility.Visible)
                                    {
                                        if (dgProductLot.GetCell(i, k).Presenter != null)
                                        {
                                            dgProductLot.GetCell(i, k).Presenter.Background = new SolidColorBrush(Colors.Red);
                                        }

                                        if (dgProductLot.GetCell(i, k).Presenter != null)
                                        {
                                            dgProductLot.GetCell(i, k).Presenter.Foreground = new SolidColorBrush(Colors.White);
                                        }
                                         
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {  }
 
        }
        // 작업자 정보 SET
        private void GetWrkShftUser()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dr["PROCID"] = Process.COATING;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", RQSTDT, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                        {
                            txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                        }
                        else
                        {
                            txtShiftStartTime.Text = string.Empty;
                        }

                        if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                        {
                            txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                        }
                        else
                        {
                            txtShiftEndTime.Text = string.Empty;
                        }

                        if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                        {
                            txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                        }
                        else
                        {
                            txtShiftDateTime.Text = string.Empty;
                        }

                        if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                        }
                        else
                        {
                            txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                            txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                        }

                        if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                        {
                            txtShift.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                        }
                        else
                        {
                            txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                            txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                        }
                    }
                    else
                    {
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        txtShiftDateTime.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        // 설비 장착정보 조회
        private DataTable GetCurrentMount(string sEqptID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT_TYPE", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        private void GetCurrentMount(string sEqptID, string sCoatSide)
        {
            try
            {
                txtCore1.Text = string.Empty;
                txtCore2.Text = string.Empty;
                txtTopSlurry1.Text = string.Empty;
                txtBackSlurry1.Text = string.Empty;
                txtTopSlurry2.Text = string.Empty;
                txtBackSlurry2.Text = string.Empty;

                txtCore1.Tag = null;
                txtCore2.Tag = null;
                txtTopSlurry1.Tag = null;
                txtBackSlurry1.Tag = null;
                txtTopSlurry2.Tag = null;
                txtBackSlurry2.Tag = null;

                btnTopSlurry1.Visibility = Visibility.Visible;
                btnBackSlurry1.Visibility = Visibility.Visible;
                btnMtrlMount.Visibility = Visibility.Visible;
                btnMtrlUnmount.Visibility = Visibility.Visible;

                lblSlurryTop1.Text = "Slurry(Top)";
                lblSlurryBack1.Text = "Slurry(Back)";

                lblSlurryTop2.Visibility = Visibility.Hidden;
                txtTopSlurry2.Visibility = Visibility.Hidden;
                lblSlurryBack2.Visibility = Visibility.Hidden;
                txtBackSlurry2.Visibility = Visibility.Hidden;

                DataTable dt = GetCurrentMount(sEqptID);

                if (dt.Rows.Count > 0)
                {
                    // SET FOIL
                    int idx = 0;

                    foreach (DataRow dr in dt.Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL'"))
                    {
                        if (idx == 0)
                        {
                            txtCore1.Text = Convert.ToString(dr["INPUT_LOTID"]);
                            txtCore1.Tag = "DUMMY";
                            radFoil1.IsChecked = string.Equals(dr["INPUT_STATE_CODE"], "A") ? true : false;
                            idx++;
                        }
                        else if (idx == 1)
                        {
                            txtCore2.Text = Convert.ToString(dr["INPUT_LOTID"]);
                            txtCore2.Tag = "DUMMY";
                            radFoil2.IsChecked = string.Equals(dr["INPUT_STATE_CODE"], "A") ? true : false;

                            break;
                        }
                    }

                    // SET SLURRY
                    idx = 0;

                    foreach (DataRow _iRow in dt.Select("PRDT_CLSS_CODE = 'ASL'"))
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(_iRow["COM_CODE"])))
                        {
                            if (idx == 0)
                            {
                                txtTopSlurry1.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                                txtTopSlurry1.Tag = Convert.ToString(_iRow["MTRLID"]);
                                idx++;
                            }
                            else if (idx == 1)
                            {
                                txtBackSlurry1.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                                txtBackSlurry1.Tag = Convert.ToString(_iRow["MTRLID"]);
                                break;
                            }
                        }
                        else
                        {
                            btnTopSlurry1.Visibility = Visibility.Hidden;
                            btnBackSlurry1.Visibility = Visibility.Hidden;
                            btnMtrlMount.Visibility = Visibility.Hidden;
                            btnMtrlUnmount.Visibility = Visibility.Hidden;


                            lblSlurryTop1.Text = "Slurry(Top1)";
                            lblSlurryBack1.Text = "Slurry(Back1)";

                            lblSlurryTop2.Visibility = Visibility.Visible;
                            txtTopSlurry2.Visibility = Visibility.Visible;
                            lblSlurryBack2.Visibility = Visibility.Visible;
                            txtBackSlurry2.Visibility = Visibility.Visible;

                            if (idx == 0)
                            {
                                txtTopSlurry1.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                                txtTopSlurry1.Tag = Convert.ToString(_iRow["MTRLID"]);
                                idx++;
                            }
                            else if (idx == 1)
                            {
                                txtTopSlurry2.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                                txtTopSlurry2.Tag = Convert.ToString(_iRow["MTRLID"]);
                                idx++;
                            }
                            else if (idx == 2)
                            {
                                txtBackSlurry1.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                                txtBackSlurry1.Tag = Convert.ToString(_iRow["MTRLID"]);
                                idx++;
                            }
                            else if (idx == 3)
                            {
                                txtBackSlurry2.Text = Convert.ToString(_iRow["INPUT_LOTID"]);
                                txtBackSlurry2.Tag = Convert.ToString(_iRow["MTRLID"]);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetIdentInfo()
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    _LDR_LOT_IDENT_BAS_CODE = string.Empty;
                    _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Collapsed;

                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.COATING;
                row["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _UNLDR_LOT_IDENT_BAS_CODE = result.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();

                    switch (_LDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }

                    switch (_UNLDR_LOT_IDENT_BAS_CODE)
                    {
                        case "CST_ID":
                        case "RF_ID":
                            dgProductLot.Columns["OUT_CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["OUT_CSTID"].Visibility = Visibility.Visible;

                            // 코팅 공정만
                            dgLotInfo.Columns["OUT_CSTID"].IsReadOnly = false;
                            btnSaveCarrier.Visibility = Visibility.Visible;

                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetRollMap()
        {

        }

        private void GetDefectLevel()
        {
            try
            {
                string[] Level = { "LV1", "LV2", "LV3" };

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LV_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();


                DataTable dtAddAll = new DataTable();
                dtAddAll.Columns.Add("CHK", typeof(string));
                dtAddAll.Columns.Add("LV_NAME", typeof(string));
                dtAddAll.Columns.Add("LV_CODE", typeof(string));

                DataRow AddData = dtAddAll.NewRow();

                for (int i = 0; i < Level.Count(); i++)
                {
                    AddData["CHK"] = 0;
                    AddData["LV_NAME"] = "ALL";
                    AddData["LV_CODE"] = "ALL";
                    dtAddAll.Rows.Add(AddData);

                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = Process.COATING;
                    Indata["LV_CODE"] = Level[i];

                    IndataTable.Rows.Add(Indata);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_ELEC_LEVEL", "RQSTDT", "RSLTDT", IndataTable);

                    dtAddAll.Merge(dtResult);

                    if (i == 0)
                        Util.GridSetData(dgLevel1, dtAddAll, FrameOperation, true);
                    else if (i == 1)
                        Util.GridSetData(dgLevel2, dtAddAll, FrameOperation, true);
                    else if (i == 2)
                        Util.GridSetData(dgLevel3, dtAddAll, FrameOperation, true);

                    IndataTable.Clear();
                    dtAddAll.Clear();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetProcessVersion(string sLotID, string sProdID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Process.COATING;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dr["LOTID"] = sLotID;
                dr["MODLID"] = sProdID;
                RQSTDT.Rows.Add(dr);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT_V01", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        private Int32 getCurrLaneQty(string sLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotID;
                dr["PROCID"] = Process.COATING;
                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR_CURR_LANEQTY", "RQSTDT", "RSLTDT", RQSTDT);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC_Int(result.Rows[0]["CURR_LANE_QTY"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return 0;
        }

        private bool getDefectLane(string sEQSGID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SCRIBE_DEFECT_LANE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        if (string.Equals(Util.NVC(row["CBO_CODE"]), sEQSGID + ":" + Process.COATING))
                            return true;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return false;
        }

        private void SetResultInfo(DataRowView rowview)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.NVC(rowview["LOTID"]);
                dr["WIPSEQ"] = Util.NVC_Int(rowview["WIPSEQ"]);
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_INFO_CT", "INDATA", "RSLTDT", RQSTDT);
                Util.GridSetData(dgLotInfo, dt, FrameOperation, true);
                _Util.SetDataGridMergeExtensionCol(dgLotInfo, new string[] { "LOTID", "OUT_CSTID", "PR_LOTID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetDefectList(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                Util.gridClear(dgWipReason);
                Util.gridClear(dgWipReason2);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("RESNPOSITION", typeof(string));

                List<C1DataGrid> lst = new List<C1DataGrid> { dgWipReason, dgWipReason2 };
                foreach (C1DataGrid dg in lst)
                {
                    inDataTable.Rows.Clear();

                    DataRow Indata = inDataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = Process.COATING;
                    Indata["LOTID"] = Util.NVC(rowview["LOTID"]);
                    Indata["RESNPOSITION"] = string.Equals(dg.Name, "dgWipReason") ? "DEFECT_TOP" : "DEFECT_BACK";
                    inDataTable.Rows.Add(Indata);

                    //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_ACTIVITYREASON_ELEC -> BR_PRD_SEL_ACTIVITYREASON_ELEC 변경
                    DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

                    if (dg.Visibility == Visibility.Visible)
                        Util.GridSetData(dg, dt, FrameOperation, true);
                }

                SetCauseTitle();

                GetSumDefectQty();
                dgLotInfo.Refresh(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetQualityList(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                Util.gridClear(dgQuality);
                Util.gridClear(dgQuality2);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                IndataTable.Columns.Add("VER_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality, dgQuality2 };
                foreach (C1DataGrid dg in lst)
                {
                    IndataTable.Rows.Clear();

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = Process.COATING;
                    Indata["LOTID"] = Util.NVC(rowview["LOTID"]);
                    Indata["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                    Indata["CLCT_PONT_CODE"] = string.Equals(dg.Name, "dgQuality") ? "T" : "B";

                    if (!string.IsNullOrEmpty(txtVersion.Text))
                    {
                        Indata["VER_CODE"] = txtVersion.Text;
                        Indata["LANEQTY"] = txtLaneQty.Value;
                    }
                    IndataTable.Rows.Add(Indata);

                    DataTable dt = new DataTable();
                    if (IsAreaCommonCodeUse("ELEC_LOT_QCA_INFO_ADD_TWS", "USE_YN"))
                    {
                        dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM_ADD_TWS", "INDATA", "RSLTDT", IndataTable);
                    }
                    else
                    {
                        dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                        if (dt.Rows.Count == 0)
                            dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);
                    }

                    if (dg.Visibility == Visibility.Visible)
                    {
                        if (string.Equals(dg.Name, dgQuality2.Name))
                            dt.Columns.Add("CLCTVAL02", typeof(double));

                        Util.GridSetData(dg, dt, FrameOperation, true);
                        _Util.SetDataGridMergeExtensionCol(dg, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetMaterial(object SelectedItem)
        {
            try
            {
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null) return;
                Util.gridClear(dgMaterial);

                string bizRuleName;
                if (_isRollMapEquipment)
                {
                    bizRuleName = "DA_PRD_SEL_CONSUME_MATERIAL2_RM";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CONSUME_MATERIAL2";
                    SetGridComboItem(dgMaterial.Columns["MTRLID"], Util.NVC(rowview["WOID"]));
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                if (_isRollMapEquipment)
                {
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                }

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = Util.NVC(rowview["LOTID"]);
                if (_isRollMapEquipment)
                {
                    Indata["EQPTID"] = cboEquipment.SelectedValue;
                    Indata["WIPSEQ"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                }
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgMaterial, dtResult, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetGridComboItem(C1.WPF.DataGrid.DataGridColumn col, string sWOID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_TB_SFC_WO_MTRL2", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private void GetRemark(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            Util.gridClear(dgRemark);

            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(String));
            dt.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dt.NewRow();

            inDataRow["LOTID"] = ObjectDic.Instance.GetObjectName("공통특이사항");

            if (rowview != null)
            {
                string[] sWipNote = GetRemarkData(Util.NVC(rowview["LOTID"])).Split('|');
                if (sWipNote.Length > 1)
                    inDataRow["REMARK"] = sWipNote[1];
            }
            dt.Rows.Add(inDataRow);

            inDataRow = dt.NewRow();
            inDataRow["LOTID"] = Util.NVC(rowview["LOTID"]);
            inDataRow["REMARK"] = GetRemarkData(Util.NVC(rowview["LOTID"])).Split('|')[0];
            dt.Rows.Add(inDataRow);

            Util.GridSetData(dgRemark, dt, FrameOperation);
            dgRemark.Rows[0].Visibility = Visibility.Collapsed;

            //[E20230127-000272] 코터 공정진척 2번컷 특이사항 자동 입력 건           
            if (dgRemark != null && dgRemark.Rows.Count > 1) //dgRemark.Rows[0] :공통특이사항,dgRemark.Rows[1] : LOTID
            {
                if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "REMARK")), ""))
                {
                    DataTableConverter.SetValue(dgRemark.Rows[1].DataItem, "REMARK", GetUnstblSectionWupNote(LoginInfo.CFG_AREA_ID, Process.COATING, Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "LOTID"))));
                }
            }
        }

        private string GetRemarkData(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }
        // [E20230127-000272] 코터 공정진척 2번컷 특이사항 자동 입력 건
        private string GetUnstblSectionWupNote(string sAreaID, string sProcID, string sLotID)
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = sAreaID;
                Indata["PROCID"] = sProcID;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE_UNSTBL_WIPNOTE", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["ATTR4"]);
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }


        private string GetLotProdVerCode(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["PROD_VER_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private DataTable GetPrintCount(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private string GetCoaterMaxVersion()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["PRODID"]);
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_MAX_VERSION", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return Util.NVC(dtMain.Rows[0][0]);

            }
            catch (Exception ex) { }

            return "";
        }

        private bool ValidConfirmLotCheck()
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            try
            {
                string sLotID = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", IndataTable);

                if (dt != null && dt.Rows.Count > 0 && !string.Equals(Process.COATING, dt.Rows[0]["PROCID"]) && (string.Equals(INOUT_TYPE.IN, dt.Rows[0]["WIP_TYPE_CODE"]) || string.Equals(INOUT_TYPE.INOUT, dt.Rows[0]["WIP_TYPE_CODE"])))
                    return false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }


        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkFastTrackOWNER()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }


        /// <summary>
        /// FastTrack 적용여부 체크
        /// </summary>
        private bool CheckFastTrackLot()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = _FastTrackLot;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["FAST_TRACK_FLAG"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;


        }

        // [E20230228-000007] CSR - Marking logic in GMES
        private string GetCutLotSampleQAFalg(string sLotID, string sProcID)
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SAMPLE_TARGET_CT", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["SAMPLE_FLAG"]);
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }

        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private void SetProcRollmapEqptDectModifyApplyFlag()
        {
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "COM_TYPE_CODE";
            sCmCode = "COATER_DEFECT_LOSS_CHARGE_REG_ALL"; // COATER_DEFECT_LOSS_CHARGE_REG_ALL

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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _ProcRollmapEqptDectModifyApplyFlag = "Y"; 

                }
                else
                {
                    _ProcRollmapEqptDectModifyApplyFlag = "N";
                }

                return;
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                return;
            }
        }
        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private void SetRollmapEqptDectModifyTarget()
        {

            string sCodeType;
            string sCmCode;
            string[] sAttribute = { Process.COATING };

            sCodeType = "COATER_DEFECT_LOSS_CHARGE_REG_ALL";  // COATER_DEFECT_LOSS_CHARGE_REG_ALL
            sCmCode = null;

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
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                _dtRollmapEqptDectModifyTarget = null;

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _dtRollmapEqptDectModifyTarget = dtResult;
                }


                return;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex); 
                return;
            }
        }

        // [E20230721-001749] [전극/조립MES] GMES 코터 공정진척 화면 내 불량/Loss 등록 기능 개선
        private string GetProcRollmapEqptDectModifyExceptFlag(string ResnCode)
        {
            string sResnCodeFlag = "Y";
            try
            {
                if (_dtRollmapEqptDectModifyTarget != null && _dtRollmapEqptDectModifyTarget.Rows.Count > 0)
                {
                    for (int i = 0; i < _dtRollmapEqptDectModifyTarget.Rows.Count; i++)
                    {
                        if (string.Equals(Util.NVC(ResnCode), Util.NVC(_dtRollmapEqptDectModifyTarget.Rows[i]["ATTR2"].ToString())))  // ResnCode
                        {
                            sResnCodeFlag = "N";
                            break;
                        }
                        else
                        {
                            sResnCodeFlag = "Y";
                        }
                    }
                }
                else
                {
                    sResnCodeFlag = "Y";
                }

                return sResnCodeFlag;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex); 
                return sResnCodeFlag;
            }
        }



        #endregion
        #region BR Biz Call
        private void SaveWIPHistory()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS1QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS2QTY", typeof(decimal));
            inDataTable.Columns.Add("SRS3QTY", typeof(decimal));
            inDataTable.Columns.Add("PROTECT_FILM_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("OUT_CSTID", typeof(string));

            foreach (DataRow dr in _CURRENT_LOTINFO.Rows)
            {
                DataRow inLotDetailDataRow = null;
                inLotDetailDataRow = inDataTable.NewRow();
                inLotDetailDataRow["LOTID"] = Util.NVC(dr["LOTID"]);
                inLotDetailDataRow["PROD_VER_CODE"] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                inLotDetailDataRow["SHIFT"] = Util.NVC(txtShift.Tag);
                inLotDetailDataRow["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                inLotDetailDataRow["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                inLotDetailDataRow["LANE_PTN_QTY"] = 1;
                inLotDetailDataRow["LANE_QTY"] = Util.NVC_Decimal(txtLaneQty.Value);
                inLotDetailDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inLotDetailDataRow);
            }

            new ClientProxy().ExecuteService("BR_ACT_REG_SAVE_LOT", "INDATA", null, inDataTable, (result, resultEx) =>
            {
                try
                {
                    if (resultEx != null)
                    {
                        Util.MessageException(resultEx);
                        return;
                    }
                    int iRow = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (iRow >= 0)
                        DataTableConverter.SetValue(dgProductLot.Rows[iRow].DataItem, "PROD_VER_CODE", Util.NVC(txtVersion.Text));
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        // 코터 Carrier Save
        private void btnSaveCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3552");  //저장 할 DATA가 없습니다.
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));

                foreach (DataRow row in _CURRENT_LOTINFO.Rows)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(row["LOTID"]);
                    newRow["CSTID"] = Util.NVC(row["OUT_CSTID"]);
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                    inTable.Rows.Add(newRow);

                    if (string.IsNullOrEmpty(Util.NVC(row["OUT_CSTID"])))
                    {
                        Util.MessageValidation("SFU6051");  //입력오류 : Carrier ID를 입력 하세요.
                        return;
                    }

                    if (!CheckCstID(Util.NVC(row["LOTID"]), Util.NVC(row["OUT_CSTID"])))
                    {
                        return;
                    }
                    else
                    {
                        if (!CheckLotID(Util.NVC(row["LOTID"]), Util.NVC(row["OUT_CSTID"])))
                            return;
                    }
                }

                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_UI", "INDATA", null, inTable, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                            }
                        });
                    }
                });
            }
        }

        private bool CheckCstID(string sLotID, string sCstID)
        {
            bool bCheck = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = sCstID.Trim();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER", "RQSTDT", "RSLTDT", inTable);

                if (searchResult.Rows.Count == 0)
                {
                    //CSTID[%1]에 해당하는 CST가 없습니다.
                    Util.MessageValidation("SFU7001", sCstID);
                    return false;
                }

                //캐리어 상태 Check
                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("U"))
                {
                    if (Util.NVC(searchResult.Rows[0]["CURR_LOTID"]) == sLotID)
                    {
                        //Carrier[%1]가 이미 할당되어 있습니다[LOT : %2].
                        Util.MessageValidation("SFU5126", sCstID, sLotID);
                        return false;
                    }
                    else
                    {
                        //CSTID[%1] 이 상태가 %2 입니다.
                        Util.MessageValidation("SFU7002", Util.NVC(searchResult.Rows[0]["CSTID"]), Util.NVC(searchResult.Rows[0]["CSTSNAME"]));
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                bCheck = false;
                Util.MessageException(e);
            }
            return bCheck;

        }

        private bool CheckLotID(string sLotID, string sCstID)
        {
            bool bCheck = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID.Trim();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIP", "RQSTDT", "RSLTDT", inTable);

                if (searchResult.Rows.Count == 0)
                {
                    //LOTID[%1]에 해당하는 LOT이 없습니다.
                    Util.MessageValidation("SFU7000", sLotID);
                    return false;
                }

                if (!string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CSTID"])))
                {
                    //Carrier[%1]가 이미 할당되어 있습니다[LOT : %2].
                    Util.MessageValidation("SFU5126", Util.NVC(searchResult.Rows[0]["CSTID"]), sLotID);
                    return false;
                }

            }
            catch (Exception e)
            {
                bCheck = false;
                Util.MessageException(e);
            }
            return bCheck;

        }

        // 자재 장착 처리
        private void SaveMountChange(bool IsCurrentFoil = true, bool IsSlurryTerm = false, bool IsCoreTerm = false, bool IsTopSlurryChange = true, bool IsBackSlurryChange = true,
            bool IsAcoreChange = true, bool IsBcoreChange = true)
        {
            DataTable mountDt = GetCurrentMount(Util.NVC(cboEquipment.SelectedValue));

            BizDataSet bizRule = new BizDataSet();
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("INPUT_LOT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("MTRLID", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("TERM_FLAG", typeof(string));

            DataTable inTable = indataSet.Tables["INDATA"];
            DataRow newRow = inTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            newRow["USERID"] = LoginInfo.USERID;

            inTable.Rows.Add(newRow);
            Grid grid = null;

            DataTable inMaterial = indataSet.Tables["INPUT_LOT"];

            // PSTN ID QUERY해서 변경하여 변경된것만 체크 후 저장하게 변경 ( 2017-01-23 )
            DataRow[] rows = mountDt.Copy().Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL'");

            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2987", new object[] { cboEquipment.SelectedValue });  //해당 설비({%1})의 등록된 Foil정보가 존재하지 않습니다.
                return;
            }

            // SET CORE
            if (txtCore1.Visibility == Visibility.Visible && rows.Length > 0 && IsCurrentFoil == true && IsAcoreChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                grid = txtCore1.Parent as Grid;

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = radFoil1.IsChecked == true ? !string.IsNullOrEmpty(txtCore1.Text.Trim()) ? "A" : "S" : "S";
                newRow["MTRLID"] = txtCore1.Tag;
                newRow["INPUT_LOTID"] = txtCore1.Text;
                newRow["TERM_FLAG"] = IsCoreTerm ? "Y" : string.Empty;
                inMaterial.Rows.Add(newRow);
            }

            if (txtCore2.Visibility == Visibility.Visible && rows.Length > 1 && IsCurrentFoil == true && IsBcoreChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                grid = txtCore2.Parent as Grid;

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = radFoil2.IsChecked == true ? !string.IsNullOrEmpty(txtCore2.Text.Trim()) ? "A" : "S" : "S";
                newRow["MTRLID"] = txtCore2.Tag;
                newRow["INPUT_LOTID"] = txtCore2.Text;
                newRow["TERM_FLAG"] = IsCoreTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            // SET SLURRY
            rows = null;
            rows = mountDt.Copy().Select("PRDT_CLSS_CODE = 'ASL'");

            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2988", new object[] { cboEquipment.SelectedValue });  //해당 설비({%1})의 등록된 Slurry정보가 존재하지 않습니다.
                return;
            }

            if (txtTopSlurry1.Visibility == Visibility.Visible && rows.Length > 0 && IsTopSlurryChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrEmpty(txtTopSlurry1.Text) ? "A" : "S";
                newRow["MTRLID"] = txtTopSlurry1.Tag;
                newRow["INPUT_LOTID"] = txtTopSlurry1.Text;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            if (txtBackSlurry1.Visibility == Visibility.Visible && rows.Length > 1 && IsBackSlurryChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrEmpty(txtBackSlurry1.Text) ? "A" : "S";
                newRow["MTRLID"] = txtBackSlurry1.Tag;
                newRow["INPUT_LOTID"] = txtBackSlurry1.Text;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            if (inMaterial.Rows.Count == 0)
                return;

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_USE_MTRL_LOT_CT", "INDATA,INPUT_LOT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                GetCurrentMount(Util.NVC(cboEquipment.SelectedValue), "");

            }, indataSet);
        }

        // 불량/Loss/물청 저장
        private void SaveDefect(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1578");  //불량 항목이 없습니다.
                return;
            }

            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDataRow = null;
            inDataRow = inDataTable.NewRow();
            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            inDataRow["USERID"] = LoginInfo.USERID;
            inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataRow["PROCID"] = Process.COATING;
            inDataTable.Rows.Add(inDataRow);

            DataTable IndataTable = inDataSet.Tables.Add("INRESN");
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            IndataTable.Columns.Add("WRK_COUNT", typeof(Int16));

            inDataRow = null;
            DataTable dtTop = (dg.ItemsSource as DataView).Table;

            foreach (DataRow dataRow in dtTop.Rows)
            {
                inDataRow = IndataTable.NewRow();
                inDataRow["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                inDataRow["WIPSEQ"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                inDataRow["ACTID"] = dataRow["ACTID"];
                inDataRow["RESNCODE"] = dataRow["RESNCODE"];
                inDataRow["RESNQTY"] = dataRow["RESNQTY"].ToString().Equals("") ? 0 : dataRow["RESNQTY"];
                inDataRow["DFCT_TAG_QTY"] = string.IsNullOrEmpty(Util.NVC(dataRow["DFCT_TAG_QTY"])) ? 0 : dataRow["DFCT_TAG_QTY"];
                inDataRow["LANE_QTY"] = txtLaneQty.Value;
                inDataRow["LANE_PTN_QTY"] = 1;
                inDataRow["COST_CNTR_ID"] = dataRow["COSTCENTERID"];
                inDataRow["WRK_COUNT"] = dataRow["COUNTQTY"].ToString() == "" ? DBNull.Value : dataRow["COUNTQTY"];

                IndataTable.Rows.Add(inDataRow);
            }

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, inDataSet);
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 품질항목 저장
        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);

            // TOP/BACK을 동시 적용하기 위하여 해당 방식으로 변경 (SPLUNK문제로 CMI요청사항) [2018-05-24]
            if (dgQuality2.Visibility == Visibility.Visible)
                inEDCLot.Merge(dtDataCollectOfChildQuality(dgQuality2));

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot);
                isChangeQuality = false;
                dg.EndEdit(true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private DataTable dtDataCollectOfChildQuality(C1DataGrid dg)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;
            foreach (DataRow _iRow in dt.Rows)
            {
                inData = IndataTable.NewRow();

                inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inData["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                inData["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inData["USERID"] = LoginInfo.USERID;
                inData["CLCTITEM"] = _iRow["CLCTITEM"];

                decimal tmp;
                if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                else
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();

                inData["WIPSEQ"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                inData["CLCTSEQ"] = 1;
                IndataTable.Rows.Add(inData);
            }
            return IndataTable;
        }

        // 자재 추가,삭제
        private void SetMaterial(string LotID, string PROC_TYPE)
        {
            if (dgMaterial.Rows.Count < 1)
                return;

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inDataRow["LOTID"] = LotID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataRow inLotDataRow = null;

                DataTable InLotdataTable = inDataSet.Tables.Add("IN_INPUT");
                InLotdataTable.Columns.Add("INPUT_LOTID", typeof(string));
                InLotdataTable.Columns.Add("MTRLID", typeof(string));
                InLotdataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                InLotdataTable.Columns.Add("PROC_TYPE", typeof(string));
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(Int32));

                DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["MTRLID"]).Equals(""))
                        {
                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                            inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                            inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            inLotDataRow["INPUT_SEQNO"] = Util.NVC_Int(row["INPUT_SEQNO"]);
                            InLotdataTable.Rows.Add(inLotDataRow);
                        }
                    }
                }
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                Thread.Sleep(500);

                GetMaterial(LotID);
                isChangeMaterial = false;
                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 특이사항 저장
        private void SaveWipNote()
        {
            if (dgRemark.GetRowCount() < 1)
                return;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIP_NOTE", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
            DataRow inData = null;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                inData = inTable.NewRow();

                inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                if (dgRemark.Rows[0].Visibility == Visibility.Visible)
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]) + "|" + Util.NVC(dt.Rows[0]["REMARK"]);
                else
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);

                inData["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(inData);
            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable);
                isChangeRemark = false;
            }
            catch (Exception ex) { Util.MessageException(ex); }

        }
        #endregion

        #region Button Event
        // 코터 착공
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            string sLargeLotID = string.Empty;
            string sWoID = string.Empty;
            DataTable dt = DataTableConverter.Convert(dgLargeLot.ItemsSource);
            foreach (DataRow dRow in dt.Rows)
            {
                if (Convert.ToBoolean(dRow["CHK"]) == true)
                {
                    sLargeLotID = Util.NVC(dRow["LOTID_LARGE"]);
                    sWoID = Util.NVC(dRow["WO_DETL_ID"]);

                }
            }

            if (!IsWorkOrderValid())
                return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("EQPTID", Util.NVC(cboEquipment.SelectedValue));
            dicParam.Add("EQSGID", Util.NVC(cboEquipmentSegment.SelectedValue));
            dicParam.Add("LARGELOT", sLargeLotID);
            dicParam.Add("WODETIL", sWoID);
            dicParam.Add("COATSIDE", "");
            dicParam.Add("SINGL", "");
            dicParam.Add("LOTID_PR", "");

            ELEC002_002_LOTSTART _LotStart = new ELEC002_002_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;
            _LotStart.IsSingleCoater = false;
            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC002_002_LOTSTART window = sender as ELEC002_002_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        // 코터 착공 취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                    return;
                }

                if (!string.Equals(Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"]), Wip_State.PROC))
                {
                    Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                    return;
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("OUT_CSTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));

                        DataRow inDataRow = null;
                        for (int i = 0; i < _CURRENT_LOTINFO.Rows.Count; i++)
                        {
                            inDataRow = null;
                            inDataRow = inDataTable.NewRow();

                            inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                            inDataRow["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                            if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                                inDataRow["OUT_CSTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["OUT_CSTID"]);

                            inDataRow["USERID"] = LoginInfo.USERID;
                            inDataRow["INPUT_LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID_PR"]);

                            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                                inDataRow["CSTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["CSTID"]);

                            inDataTable.Rows.Add(inDataRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_CT", "INDATA", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetDeleteLargeLot(string LargeLotId)
        {
            if (dgLargeLot.Rows.Count == 0)
                return;

            string sLargeLotID = string.Empty;
            string sWoID = string.Empty;
            DataTable dt = DataTableConverter.Convert(dgLargeLot.ItemsSource);
            foreach (DataRow dRow in dt.Rows)
                if (Convert.ToBoolean(dRow["CHK"]) == true)
                    sLargeLotID = Util.NVC(dRow["LOTID_LARGE"]);

            if (string.IsNullOrEmpty(sLargeLotID))
            {
                Util.MessageValidation("SFU1490");  //대LOT을 선택하십시오.
                return;
            }

            //대LOT 정리를 하시겠습니까?
            Util.MessageConfirm("SFU1488", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    DataRow indata = inTable.NewRow();
                    indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    indata["IFMODE"] = IFMODE.IFMODE_OFF;
                    indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    indata["LOTID"] = LargeLotId;
                    indata["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(indata);

                    new ClientProxy().ExecuteService("BR_PRD_REG_TERM_LARGE_LOT", "INDATA", null, inTable, (result, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    });
                }
            });
        }

        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.PROC))
            {
                Util.MessageValidation("SFU2957");  //진행중인 작업을 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END_COATER wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPT_END_COATER();
            wndEqpComment.FrameOperation = FrameOperation;

            string endLotID = "";
            foreach (DataRow row in _CURRENT_LOTINFO.Rows)
                if (!string.IsNullOrEmpty(Util.NVC(row["LOTID"])))
                    endLotID = Util.NVC(row["LOTID"]) + "," + endLotID;

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[1] = Process.COATING;
                Parameters[2] = endLotID;
                Parameters[3] = txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(txtStartDateTime.Text);    // 시작시간 추가
                Parameters[5] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["CUT_ID"]);
                Parameters[6] = Util.NVC(txtParentQty.Value);
                Parameters[7] = chkFinalCut.IsChecked == true ? "Y" : "N";

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(OnCloseEqptEnd);
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        private void OnCloseEqptEnd(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_EQPT_END_COATER window = sender as LGC.GMES.MES.CMM001.CMM_COM_EQPT_END_COATER;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        private void btnEndCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1938");  //취소할 LOT을 선택하세요.
                    return;
                }

                if (!string.Equals(Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"]), Wip_State.END))
                {
                    Util.MessageValidation("SFU5146", new object[] { _CURRENT_LOTINFO.Rows[0]["LOTID"] });  //해당 Lot[%1]은 실적확정 상태가 아니라 취소 할 수 없습니다.
                    return;
                }

                if (_Util.IsCommonCodeUse("ELEC_CNFM_CANCEL_USER", LoginInfo.USERID) == false)
                {
                    Util.MessageValidation("SFU5148", new object[] { LoginInfo.USERID });  //해당 USER[%1]는 실적취소할 권한이 없습니다. (시스템 담당자에게 문의 바랍니다.)
                    return;
                }

                // 해당 확정 처리 된 Lot을 실적취소하시겠습니까?
                Util.MessageConfirm("SFU5147", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        inDataRow["PROCID"] = Process.COATING;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("CUT_ID", typeof(string));

                        DataRow inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                        InLotdataTable.Rows.Add(inLotDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_END_LOT_ELTR", "INDATA,INLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            RefreshData();
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWebBreak_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            LGC.GMES.MES.ELEC002.ELEC002_002_WEB_BREAK wndWebBreak = new LGC.GMES.MES.ELEC002.ELEC002_002_WEB_BREAK();
            wndWebBreak.FrameOperation = FrameOperation;

            if (wndWebBreak != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[1] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                C1WindowExtension.SetParameters(wndWebBreak, Parameters);

                wndWebBreak.Closed += new EventHandler(OnCloseWebBreak);
                this.Dispatcher.BeginInvoke(new Action(() => wndWebBreak.ShowModal()));
            }

        }

        private void OnCloseWebBreak(object sender, EventArgs e)
        {
            LGC.GMES.MES.ELEC002.ELEC002_002_WEB_BREAK window = sender as LGC.GMES.MES.ELEC002.ELEC002_002_WEB_BREAK;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        private void btnDispatch_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            // 확정 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.END))
            {
                Util.MessageValidation("SFU5131");  //공정이동 대상 Lot 선택 오류 [선택한 Lot이 완공상태 인지 확인 후 처리]
                return;
            }

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            if (!IsValidDispatcher())
                return;

            int returnValue = 0;

            if (_isRollMapEquipment)
            {
                if (CheckTestCut() == false) return; //nathan 2024.01.10 롤맵 코터 수불 실적 적용

                // 2023.10.10 조성근- 롤맵 홀드는 수동 선택이 아닌, 자동 선택 ( E20231005-000782 )
                //DataTable dtHold = GetRollMapHold(Process.COATING);
                //if (CommonVerify.HasTableRow(dtHold))
                //{
                //    //롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////
                //    holdLotClassCode.Clear();
                //    if (dtHold.Columns.Contains("ADJ_LOTID") && dtHold.Columns.Contains("HOLD_CLASS_CODE"))
                //    {
                //        holdLotClassCode.Add(dtHold.Rows[0]["ADJ_LOTID"].ToString(), dtHold.Rows[0]["HOLD_CLASS_CODE"].ToString());
                //    }

                //    //롤맵 Hold 기능 추가/////////////////////////////////////////////////////////////

                //    CMM_ROLLMAP_HOLD popRollMapHold = new CMM_ROLLMAP_HOLD();
                //    popRollMapHold.FrameOperation = FrameOperation;

                //    object[] parameters = new object[10];
                //    parameters[0] = Process.COATING;
                //    parameters[1] = cboEquipment.SelectedValue;
                //    parameters[2] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                //    parameters[3] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                //    parameters[4] = Util.NVC(cboEquipment.Text);
                //    parameters[5] = txtVersion.Text.Trim();
                //    C1WindowExtension.SetParameters(popRollMapHold, parameters);

                //    popRollMapHold.Closed += new EventHandler(popRollMapHold_Closed);
                //    Dispatcher.BeginInvoke(new Action(() => popRollMapHold.ShowModal()));
                //}
                //else
                {
                    CMM_ELEC_HOLD_YN popupHoldYn = new CMM_ELEC_HOLD_YN();
                    popupHoldYn.FrameOperation = FrameOperation;

                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                    C1WindowExtension.SetParameters(popupHoldYn, parameters);

                    popupHoldYn.Closed += new EventHandler(wndHoldChk_Closed);
                    Dispatcher.BeginInvoke(new Action(() => popupHoldYn.ShowModal()));
                }

                return;
            }
            else
            {
                if (!ValidQualitySpec("Hold"))
                {
                    returnValue++;

                    Util.MessageConfirm("SFU8185", (result) => //자동HOLD되도록 설정된 품질검사 결과가 기준치를 만족하지 못했습니다. 완성랏이 홀드됩니다. 계속하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CMM_ELEC_HOLD_YN wndPopup = new CMM_ELEC_HOLD_YN();
                            wndPopup.FrameOperation = FrameOperation;

                            object[] parameters = new object[2];
                            parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                            C1WindowExtension.SetParameters(wndPopup, parameters);

                            wndPopup.Closed += new EventHandler(wndHoldChk_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                        return;
                    });
                }

                //LSL, USL SPEC HOLD
                if (!ValidQualitySpecExists())
                {
                    returnValue++;

                    Util.MessageConfirm("SFU8186", (result) => //LSL, USL 미설정되어 계속 진행할 경우 완성LOT이 HOLD처리 됩니다. 계속하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CMM_ELEC_HOLD_YN wndPopup = new CMM_ELEC_HOLD_YN();
                            wndPopup.FrameOperation = FrameOperation;

                            object[] parameters = new object[2];
                            parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                            C1WindowExtension.SetParameters(wndPopup, parameters);

                            wndPopup.Closed += new EventHandler(wndHoldChk_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                        return;
                    });
                }

                if (returnValue < 1)
                {
                    // 자동 Hold 처리 여부
                    CMM_ELEC_HOLD_YN popupHold = new CMM_ELEC_HOLD_YN();
                    popupHold.FrameOperation = FrameOperation;

                    if (popupHold != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                        C1WindowExtension.SetParameters(popupHold, Parameters);

                        popupHold.Closed += new EventHandler(wndHoldChk_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popupHold.ShowModal()));
                    }
                }
            }
        }

        private void wndHoldChk_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_HOLD_YN window = sender as CMM_ELEC_HOLD_YN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _IS_POSTING_HOLD = window.HOLDYNCHK;
                ValidateCarrierCTUnloader();
            }
        }

        private void popRollMapHold_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_HOLD window = sender as CMM_ROLLMAP_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _IS_POSTING_HOLD = window.HoldCheck;
                ConfirmDispatcher();
            }
        }

        private void ValidateCarrierCTUnloader()
        {
            try
            {
                if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LOTID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                    IndataTable.Rows.Add(Indata);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(result.Rows[0]["CSTID"].ToString()))
                        {
                            CMM_ELEC_CST_RELATION cst = new CMM_ELEC_CST_RELATION();
                            cst.FrameOperation = FrameOperation;

                            if (cst != null)
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);

                                C1WindowExtension.SetParameters(cst, Parameters);

                                cst.Closed += new EventHandler(wndCst_Closed);
                                this.Dispatcher.BeginInvoke(new Action(() => cst.ShowModal()));
                            }
                        }
                        else
                        {
                            ConfirmDispatcher();
                        }
                    }
                }
                else
                {
                    ConfirmDispatcher();
                }
            }
            catch (Exception e)
            {
                Util.MessageException(e);
                return;
            }
            return;
        }

        private void wndCst_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_CST_RELATION window = sender as CMM_ELEC_CST_RELATION;

            if (window.DialogResult == MessageBoxResult.OK)
                ConfirmDispatcher();
        }

        private void ConfirmDispatcher(bool bRealWorkerSelFlag = false)
        {
            // Remark 취합
            Dictionary<string, string> remarkInfo = GetRemarkConvert();
            if (remarkInfo.Count == 0)
            {
                Util.MessageValidation("SFU4257"); // 특이사항 정보를 확인 바랍니다.
                return;
            }

            #region 작업자 실명관리 기능 추가
            if (!bRealWorkerSelFlag && CheckRealWorkerCheckFlag())
            {
                CMM001.CMM_COM_INPUT_USER wndRealWorker = new CMM001.CMM_COM_INPUT_USER();

                wndRealWorker.FrameOperation = FrameOperation;
                object[] Parameters2 = new object[0];
                //Parameters2[0] = "";

                C1WindowExtension.SetParameters(wndRealWorker, Parameters2);

                wndRealWorker.Closed -= new EventHandler(wndRealWorker_Closed);
                wndRealWorker.Closed += new EventHandler(wndRealWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndRealWorker.ShowModal()));

                return;
            }
            #endregion

            // 다음 공정으로 이송 하시겠습니까?
            Util.MessageConfirm("SFU4257", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {
                        // DEFECT 자동 저장
                        SaveDefect(dgWipReason);
                        SaveDefect(dgWipReason2);

                        // IN_DATA
                        DataSet inDataSet = new DataSet();
                        DataTable inData = inDataSet.Tables.Add("IN_DATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("WIP_NOTE", typeof(string));
                        inData.Columns.Add("FINAL_CUT_FLAG", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                        row["PROCID"] = Process.COATING;
                        row["SHIFT"] = txtShift.Tag;
                        row["WRK_USERID"] = Util.NVC(txtWorker.Tag);
                        row["WRK_USER_NAME"] = Util.NVC(txtWorker.Text);
                        row["WIP_NOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "LOTID"))];
                        row["FINAL_CUT_FLAG"] = chkFinalCut.IsChecked == true ? "Y" : "N";
                        row["USERID"] = LoginInfo.USERID;
                        inDataSet.Tables["IN_DATA"].Rows.Add(row);

                        // IN_LOT
                        // 단선 수정 가능으로 공정 이동 시에 투입량이 변경되면 UPDATE를 위하여 PARAMETER 추가 [2019-12-04]
                        DataTable InLot = inDataSet.Tables.Add("IN_LOT");
                        InLot.Columns.Add("LOTID", typeof(string));
                        InLot.Columns.Add("INPUTQTY", typeof(decimal));
                        InLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        InLot.Columns.Add("RESNQTY", typeof(decimal));
                        InLot.Columns.Add("TOPLOSSQTY", typeof(decimal));
                        InLot.Columns.Add("HOLD_YN", typeof(string));

                        for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                        {
                            if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                            {
                                row = InLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "LOTID"));
                                row["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_BACK_QTY"));
                                row["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));
                                row["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM"));
                                row["TOPLOSSQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "INPUT_TOP_QTY"));
                                row["HOLD_YN"] = _IS_POSTING_HOLD;
                                inDataSet.Tables["IN_LOT"].Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_NEXT_PROC_MOVE", "IN_DATA,IN_LOT", null, (result, searchException) =>
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            RefreshData(true);
                        }, inDataSet);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }
            });
        }

        private Dictionary<string, string> GetRemarkConvert()
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();
            if (dgRemark.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < dgRemark.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    sRemark.Append("|");

                    // 코터 롤맵 Hold 기능 추가 Start//////////////////////////////////////////////////////////////////////
                    if(!holdLotClassCode.IsNullOrEmpty() && holdLotClassCode.Count > 0)
                    {
                        string lotId = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "LOTID"));
                        string holdClassCode = holdLotClassCode[lotId];
                        string holdMessage = string.Empty;

                        //if ("E2000_01".Equals(holdClassCode))
                        //{
                        //    // Tag Section Hold Cond Core LOT[%1] SFU9951 
                        //    holdMessage = GetMessageWithSubstitution("SFU9951", lotId);
                        //}
                        //else
                        //{
                        //    // Tag Section Hold Cond Outmost LOT[%1] SFU9952
                        //    holdMessage = GetMessageWithSubstitution("SFU9952", lotId);
                        //}

                        holdMessage = GetMessageWithSubstitution(holdClassCode.Trim(), lotId);

                        sRemark.Append(holdMessage);
                        sRemark.Append("|");
                    }
                    
                    // 코터 롤맵 Hold 기능 추가 End//////////////////////////////////////////////////////////////////////

                    // 3. 조정횟수
                    if (dgWipReason.Visibility == Visibility.Visible && dgWipReason.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < dgWipReason.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(dgWipReason.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (dgWipReason2.Columns["COUNTQTY"] != null)
                        for (int j = 0; j < dgWipReason2.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[j].DataItem, "COUNTQTY"))) &&
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgWipReason2.Rows[j].DataItem, "COUNTQTY")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[j].DataItem, "RESNNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(dgWipReason2.Rows[j].DataItem, "COUNTQTY")) + ",");

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 4. 압연횟수
                    sRemark.Append("|");

                    // 5.색지정보
                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 6.합권이력
                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }

        private void PrintHistoryCard()
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[4];
                Parameters[0] = txtEndLotId.Text; //LOT ID
                Parameters[1] = Process.COATING; //PROCESS ID
                Parameters[2] = string.Empty;
                Parameters[3] = "Y";    // 실적확정 여부

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnBarcodeLabel_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1559");  //발행할 LOT을 선택하십시오.
                return;
            }

            if (string.IsNullOrEmpty(GetLotProdVerCode(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]))))
            {
                Util.MessageValidation("SFU4561"); // 생산실적 화면의 저장버튼 클릭 후(버전 정보 저장) 바코드 출력 하시기 바랍니다.
                return;
            }

            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                return;
            }

            DataTable printDT = GetPrintCount(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]), Process.COATING);

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            int iSamplingCount;
                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            {
                                foreach (DataRow _iRow in _CURRENT_LOTINFO.Rows)
                                {
                                    iSamplingCount = 1;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.COATING, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                            }
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    foreach (DataRow _iRow in _CURRENT_LOTINFO.Rows)
                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_iRow["LOTID"]), Process.COATING);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        private void btnPrintLabel_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Process.COATING;
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // 수작업 모드
        private void btnManualMode_Click(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();

            foreach (Button button in Util.FindVisualChildren<Button>(mainGrid))
                listAuth.Add(button);

            btnExtra.IsDropDownOpen = false;
            SetManualMode(listAuth);

            if (btnStart.Visibility != Visibility.Visible)
            {
                Util.MessageValidation("SFU5142");  //수작업 모드를 진행할 권한이 없습니다.(엔지니어에게 문의 바랍니다.)
                return;
            }
        }

        // 설비 특이 사항
        private void btnEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT wndEqpComment = new LGC.GMES.MES.CMM001.CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = Process.COATING;
                Parameters[3] = _CURRENT_LOTINFO.Rows.Count == 0 ? "" : Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                Parameters[4] = _CURRENT_LOTINFO.Rows.Count == 0 ? "" : Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                Parameters[5] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[6] = Util.NVC(txtShift.Text);
                Parameters[7] = Util.NVC(txtShift.Tag);
                Parameters[8] = Util.NVC(txtWorker.Text);
                Parameters[9] = Util.NVC(txtWorker.Tag);

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }

        // LOT 정리
        private void btnCleanLot_Click(object sender, RoutedEventArgs e)
        {
            if (dgLargeLot.Rows.Count == 0)
                return;

            string ValuetoLotID = string.Empty;
            DataTable dt = DataTableConverter.Convert(dgLargeLot.ItemsSource);
            foreach (DataRow dRow in dt.Rows)
                if (Convert.ToBoolean(dRow["CHK"]) == true)
                    ValuetoLotID = dRow["LOTID_LARGE"].ToString();

            if (string.IsNullOrEmpty(ValuetoLotID))
            {
                Util.MessageValidation("SFU1490");  //대LOT을 선택하십시오.
                return;
            }

            SetDeleteLargeLot(ValuetoLotID);
        }

        // 작업 조건 등록
        private void btnEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            CMM_ELEC_EQPT_COND wndPopup = new CMM_ELEC_EQPT_COND();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.Text);
                Parameters[2] = Process.COATING;
                Parameters[3] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnWipDataCollect_Click(object sender, RoutedEventArgs e)
        {
            //if (cboEquipment.SelectedIndex < 1)
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return;
            //}

            LGC.GMES.MES.CMM001.CMM_COM_ELEC_MIXCONFIRM wndLotIssue = new LGC.GMES.MES.CMM001.CMM_COM_ELEC_MIXCONFIRM();
            wndLotIssue.FrameOperation = FrameOperation;

            if (wndLotIssue != null)
            {
                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = Process.COATING;
                Parameters[3] = "";
                Parameters[4] = "";
                Parameters[5] = cboEquipment.Text;

                C1WindowExtension.SetParameters(wndLotIssue, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndLotIssue.ShowModal()));
            }
        }
        // SLURRY 정보
        private void btnMixerTankInfo_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_MIXER_TANK_INFO wndMixerTankInfo = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_MIXER_TANK_INFO();
            wndMixerTankInfo.FrameOperation = FrameOperation;

            if (wndMixerTankInfo != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[2] = Process.COATING;
                Parameters[3] = _CURRENT_LOTINFO.Rows.Count == 0 ? "" : Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                Parameters[4] = _CURRENT_LOTINFO.Rows.Count == 0 ? "" : Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);

                C1WindowExtension.SetParameters(wndMixerTankInfo, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndMixerTankInfo.ShowModal()));
            }
        }

        // W/O 예약
        private void btnReservation_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION wndReservation = new LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION();
            wndReservation.FrameOperation = FrameOperation;

            if (wndReservation != null)
            {
                UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

                btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];

                if (wo.dgWorkOrder != null && wo.dgWorkOrder.Rows.Count > 0)
                {
                    Parameters[0] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRJT_NAME");
                    Parameters[1] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PROD_VER_CODE");
                    Parameters[2] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "WOID");
                    Parameters[3] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "PRODID");
                    Parameters[4] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "ELECTYPE");
                    Parameters[5] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "LOTYNAME");
                    Parameters[6] = DataTableConverter.GetValue(wo.dgWorkOrder.Rows[0].DataItem, "EQPTID");
                    Parameters[7] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[8] = Process.COATING;
                    Parameters[9] = (wo.dgWorkOrder.ItemsSource as DataView).ToTable();
                }

                C1WindowExtension.SetParameters(wndReservation, Parameters);
                wndReservation.Closed += WndReservation_Closed;

                this.Dispatcher.BeginInvoke(new Action(() => wndReservation.ShowModal()));
            }
        }

        private void WndReservation_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION window = sender as LGC.GMES.MES.CMM001.CMM_COM_ELEC_RESERVATION;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        // FOIL 관리
        private void btnFoil_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.CMM001.CMM_COM_FOIL wndReservation = new LGC.GMES.MES.CMM001.CMM_COM_FOIL();
                wndReservation.FrameOperation = FrameOperation;

                btnExtra.IsDropDownOpen = false;
                this.Dispatcher.BeginInvoke(new Action(() => wndReservation.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 물류반송현황
        private void btnLogisStat_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            CMM_ELEC_LOGIS_STAT popupElecLogisStat = new CMM_ELEC_LOGIS_STAT();
            popupElecLogisStat.FrameOperation = FrameOperation;

            if (popupElecLogisStat != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] parameters = new object[1];
                parameters[0] = Util.NVC(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(popupElecLogisStat, parameters);
                this.Dispatcher.BeginInvoke(new Action(() => popupElecLogisStat.ShowModal()));
            }
        }

        //ScrapLot 재생성
        private void btnScrapLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                CMM_ELEC_RECREATE_SCRAP_LOT popupReScrapLot = new CMM_ELEC_RECREATE_SCRAP_LOT();
                popupReScrapLot.FrameOperation = FrameOperation;

                if (popupReScrapLot != null)
                {
                    btnExtra.IsDropDownOpen = false;

                    object[] parameters = new object[3];
                    parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    parameters[1] = Util.NVC(cboEquipment.SelectedValue);
                    parameters[2] = Process.COATING;
                    C1WindowExtension.SetParameters(popupReScrapLot, parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => popupReScrapLot.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        // 착공 취소 LOT 재 생성
        private void btnCancelDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[3];
                Parameters[0] = Process.COATING; //PROCESS ID
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(OnCloseCancelDeleteLot);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseCancelDeleteLot(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_CANCEL_DELETE_LOT;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();
        }

        // 전수 불량 등록
        private void btnSaveRegDefectLane_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                if (_CURRENT_LOTINFO.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                    return;
                }

                if (!string.Equals(_CURRENT_LOTINFO.Rows[0]["WIPSTAT"], Wip_State.END))
                {
                    Util.MessageValidation("SFU3723");  //작업 가능한 상태가 아닙니다.
                    return;
                }

                LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[6];
                    Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                    Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[2] = Process.COATING;
                    Parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[4] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["PRODID"]);

                    for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                        if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                            Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));

                    C1WindowExtension.SetParameters(wndPopup, Parameters);
                    wndPopup.Closed += new EventHandler(OnCloseRegDefectLane);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnCloseRegDefectLane(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE window = sender as LGC.GMES.MES.CMM001.CMM_ELEC_REG_DFCT_LANE;
            if (window.DialogResult == MessageBoxResult.OK)
                RefreshData();

        }

        // 임시 저장 기능
        private void btnSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_CURRENT_LOTINFO.Rows.Count == 0)
                    return;

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                if (txtInputQty.Value <= 0)
                {
                    if (IsCoaterProdVersion() == true && !string.Equals(GetCoaterMaxVersion(), txtVersion.Text))
                    {
                        // 작업지시 최신 Version과 상이합니다! 그래도 저장하시겠습니까?
                        Util.MessageConfirm("SFU4462", (sResult) =>
                        {
                            if (sResult == MessageBoxResult.OK)
                            {
                                SaveWIPHistory();
                                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                                Util.MessageInfo("SFU1270");    //저장되었습니다.
                            }
                        }, new object[] { GetCoaterMaxVersion(), txtVersion.Text });
                    }
                    else
                    {
                        SaveWIPHistory();
                        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                        Util.MessageInfo("SFU1270");    //저장되었습니다.
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 작업자 선택
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.COATING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플래그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseShift(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }

        // 좌측 확장 버튼
        private void btnLeftExpandFrame_Click(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualWidth != 0)
                ExpandFrame = Content.ColumnDefinitions[0].Width;

            if (btnLeftExpandFrame.IsChecked == true)
            {
                Content.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                Content.ColumnDefinitions[0].Width = ExpandFrame;
            }
        }

        // 상하 확장 버튼
        private void btnExpandFrame_Click(object sender, RoutedEventArgs e)
        {
            if (grdWorkOrder.ActualHeight != 0)
                ExpandFrame = Content.RowDefinitions[1].Height;
            if (btnExpandFrame.IsChecked == true)
            {
                Content.RowDefinitions[1].Height = new GridLength(0);
            }
            else
            {
                Content.RowDefinitions[1].Height = ExpandFrame;
            }
        }

        // CWA 불량등록 필터 그리드 사이즈 조절
        private void chkDefectFilter_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (_CURRENT_LOTINFO.Rows.Count < 1)
            {
                if (isDefectLevel == false)
                {
                    Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                }
                cb.IsChecked = false;
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;
                return;
            }

            DefectVisibleLVAll();

            if (cb.IsChecked == true)
            {
                grdDefectLVFilter.Visibility = Visibility.Visible;
                //CWA 불량등록 필터 그리드
                GetDefectLevel();
                if (isDefectLevel == false)
                    yPosition.Height = new GridLength(yPosition.ActualHeight + grdDefectLVFilter.ActualHeight);
            }
            else
            {
                yPosition.Height = new GridLength(yPosition.ActualHeight - grdDefectLVFilter.ActualHeight);
                grdDefectLVFilter.Visibility = Visibility.Collapsed;

            }
        }

        // 전극 버전 선택
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            CMM_ELECRECIPE wndPopup = new CMM_ELECRECIPE();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["PRODID"]);
                Parameters[1] = Process.COATING;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[4] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                Parameters[5] = "Y";    // 전극 버전 확정 여부
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseVersion);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseVersion(object sender, EventArgs e)
        {
            CMM_ELECRECIPE window = sender as CMM_ELECRECIPE;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (Util.NVC_Decimal(txtLaneQty.Value) != Util.NVC_Decimal(window._ReturnLaneQty))
                {
                    txtVersion.Text = window._ReturnRecipeNo;
                    txtLaneQty.Value = Convert.ToDouble(window._ReturnLaneQty);
                    txtCurLaneQty.Value = getCurrLaneQty(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]));

                    if (dgLotInfo.GetRowCount() > 0)
                        for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                            DataTableConverter.SetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "LANE_QTY", txtLaneQty.Value);

                    GetSumDefectQty();
                    dgLotInfo.Refresh(false);
                }
                else
                {
                    txtVersion.Text = window._ReturnRecipeNo;
                }
            }
        }

        // SLURRY 선택 팝업 오픈
        private void btnSlurry_Click(object sender, RoutedEventArgs e)
        {
            string sWOID = string.Empty;

            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

            if (idx > -1 && string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"), "Y"))
                sWOID = Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WOID"));

            if (string.IsNullOrEmpty(sWOID))
            {
                Util.MessageValidation("SFU1635");  //선택된 W/O가 없습니다.
                return;
            }

            CMM_ELEC_SLURRY popup = new CMM_ELEC_SLURRY();
            popup.FrameOperation = FrameOperation;

            if (cboEquipment.SelectedIndex > 0 && !string.IsNullOrEmpty(sWOID))
            {
                object[] Parameters = new object[8];
                Parameters[0] = Process.COATING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(sWOID);
                Parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[4] = ((Button)sender).Name == "btnTopSlurry1" ? 0 : 1;
                Parameters[5] = ((Button)sender).Name == "btnTopSlurry1" ? txtTopSlurry1.Text : txtBackSlurry1.Text;
                Parameters[6] = "N";
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(cboEquipment.SelectedItem, "WIDE_ROLL_FLAG"));

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(OnClickSlurryClose);
                Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                popup.CenterOnScreen();
            }
        }

        private void OnClickSlurryClose(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY popup = sender as CMM_ELEC_SLURRY;
            Button btn = sender as Button;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (popup._ReturnPosition == 0)
                {
                    txtTopSlurry1.Text = popup._ReturnLotID;
                    txtTopSlurry1.Tag = popup._ReturnPRODID;

                    if (popup._IsAllConfirm == true)
                    {
                        txtBackSlurry1.Text = popup._ReturnLotID;
                        txtBackSlurry1.Tag = popup._ReturnPRODID;
                    }
                }
                else
                {
                    txtBackSlurry1.Text = popup._ReturnLotID;
                    txtBackSlurry1.Tag = popup._ReturnPRODID;

                    if (popup._IsAllConfirm == true)
                    {
                        txtTopSlurry1.Text = popup._ReturnLotID;
                        txtTopSlurry1.Tag = popup._ReturnPRODID;
                    }
                }

                // Slurry만 장착 처리
                if (popup._IsAllConfirm == true)
                    SaveMountChange(false, popup._IsSlurryTerm);
                else
                    SaveMountChange(false, popup._IsSlurryTerm, false, popup._ReturnPosition == 0 ? true : false, popup._ReturnPosition == 1 ? true : false);
            }
        }

        // 자재 장착 버튼
        private void btnMtrlMount_Click(object sender, RoutedEventArgs e)
        {
            string sWOID = string.Empty;
            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

            if (idx > -1 && string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"), "Y"))
                sWOID = Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));

            if (cboEquipment.SelectedIndex > 0 && !string.IsNullOrEmpty(sWOID))
            {
                // 해당 자재를 변경하시겠습니까?
                Util.MessageConfirm("SFU2989", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        SaveMountChange();
                    }
                });
            }
        }

        // 자재 탈착 버튼
        private void btnMtrlUnmount_Click(object sender, RoutedEventArgs e)
        {
            string sWOID = string.Empty;
            UC_WORKORDER_CWA wo = grdWorkOrder.Children[0] as UC_WORKORDER_CWA;

            int idx = new Util().GetDataGridCheckFirstRowIndex(wo.dgWorkOrder, "CHK");

            if (idx > -1 && string.Equals(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "EIO_WO_SEL_STAT"), "Y"))
                sWOID = Util.NVC(DataTableConverter.GetValue(wo.dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));

            CMM_ELEC_SLURRY_TERM popup = new CMM_ELEC_SLURRY_TERM();
            popup.FrameOperation = FrameOperation;

            if (cboEquipment.SelectedIndex > 0)
            {
                object[] Parameters = new object[7];
                Parameters[0] = Process.COATING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(sWOID);
                Parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[4] = ((Button)sender).Name == "btnTopSlurry1" ? 0 : 1;
                Parameters[5] = ((Button)sender).Name == "btnTopSlurry1" ? txtTopSlurry1.Text : txtBackSlurry1.Text;
                Parameters[6] = "N";

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(OnClickMtrlUnmountClose);
                Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                popup.CenterOnScreen();
            }
        }

        private void OnClickMtrlUnmountClose(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY_TERM window = sender as CMM_ELEC_SLURRY_TERM;
            if (window.DialogResult == MessageBoxResult.OK)
                GetCurrentMount(Util.NVC(cboEquipment.SelectedValue), "");
        }

        // TAB 항목 전체 저장
        private void btnPublicWipSave_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                if (isChangeReason == true)
                {
                    SaveDefect(dgWipReason);

                    if (dgWipReason2.Visibility == Visibility.Visible)
                        SaveDefect(dgWipReason2);
                }

                if (isChangeQuality == true)
                    SaveQuality(dgQuality);

                if (isChangeMaterial == true)
                    SetMaterial(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]), "A");

                if (isChangeRemark == true)
                    SaveWipNote();

                if (_isRollMapEquipment)
                {
                    SaveDefectForRollMap();
                    int rowIndex = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (rowIndex >= 0) GetDefectList(dgProductLot.Rows[rowIndex].DataItem);
                }

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 불량/Loss/물청 저장
        private void btnSaveWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                SaveDefect(dgWipReason);

                if (dgWipReason2.Visibility == Visibility.Visible)
                    SaveDefect(dgWipReason2);

                if (_isRollMapEquipment)
                {
                    SaveDefectForRollMap();
                    Util.MessageInfo("SFU1270");    //저장되었습니다.

                    int rowIndex = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (rowIndex >= 0) GetDefectList(dgProductLot.Rows[rowIndex].DataItem);
                }
                else
                {
                    Util.MessageInfo("SFU1270");    //저장되었습니다.
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 품질항목 저장
        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            try
            {
                SaveQuality(dgQuality);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        // 투입자재 추가
        private void btnAddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            if (dgMaterial.ItemsSource == null || dgMaterial.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["CHK"] = true;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["INPUT_DTTM"] = string.Format("{0:yyyy-MM-dd hh:mm}", DateTime.Now);
            dt.Rows.Add(dr);
        }

        // 투입 자재 삭제 버튼
        private void btnDeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs != null)
            {
                //입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                Util.MessageConfirm("SFU1815", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                        SetMaterial(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]), "D");
                });
            }
        }

        // 투입 자재 저장 버튼
        private void btnSaveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            // 진행LOT이 실적 확정 완료이면 저장 전체 방어
            if (ValidConfirmLotCheck() == false)
            {
                Util.MessageValidation("SFU5134");  // 이미 공정이동 완료 된 LOT이라 저장은 불가능 합니다.
                return;
            }

            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs == null)
            {
                Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                return;
            }

            foreach (DataRow dr in drs)
            {
                if (string.IsNullOrEmpty(dr["INPUT_LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU1984");  //투입자재 LOT ID를 입력하세요.
                    return;
                }
            }
            SetMaterial(Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]), "A");
        }

        // 특이사항 저장
        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (_CURRENT_LOTINFO.Rows.Count == 0)
                return;

            try
            {
                SaveWipNote();

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        #endregion

        #region 작업자 실명관리 기능 추가
        private bool CheckRealWorkerCheckFlag()
        {
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Process.COATING;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("REAL_WRKR_CHK_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["REAL_WRKR_CHK_FLAG"]).Equals("Y"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void wndRealWorker_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_INPUT_USER window = sender as CMM001.CMM_COM_INPUT_USER;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveRealWorker(window.USER_NAME);

                    ConfirmDispatcher(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i + dgLotInfo.TopRows.Count].DataItem, "LOTID"));
                        //newRow["WIPSEQ"] = null;
                        newRow["WORKER_NAME"] = sWrokerName;
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);
                    }
                }

                if (inTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        /// <summary>
        /// FastTrack 설정 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkFastTrack_Click(object sender, RoutedEventArgs e)
        {
            if (chkFastTrack.IsChecked == true)
            {
                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU7354", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(true);
                        chkFastTrack.IsChecked = true;
                    }
                    else
                    {
                        chkFastTrack.IsChecked = false;
                    }

                });
            }
            else
            {

                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU7355", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetFastTrace(false);
                        chkFastTrack.IsChecked = false;
                    }
                    else
                    {
                        chkFastTrack.IsChecked = true;
                    }


                });
            }
        }

        /// <summary>
        ///2021-09-03 오화백 FastTrack 설정
        /// FastTrack 설정여부
        /// </summary>
        private void SetFastTrace(bool fasttrackFlag)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FAST_TRACK_FLAG", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                dr["LOTID"] = _FastTrackLot;
                if (fasttrackFlag == true)
                {
                    dr["FAST_TRACK_FLAG"] = "Y";
                }
                else
                {
                    dr["FAST_TRACK_FLAG"] = string.Empty;
                }
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_FAST_TRACK_LOT", "INDATA", null, dtRqst);

                if (fasttrackFlag)
                {
                    Util.MessageInfo("SFU1518");
                }
                else
                {
                    Util.MessageInfo("SFU1937");
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// CSR : C20220622-000589 - Coater Process Progress(New)
        /// </summary>
        public void GetSlurryRefresh()
        {
            try
            {
                GetCurrentMount(Util.NVC(cboEquipment.SelectedValue), "");  // FOIL, SLURRY 조회
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 롤맵 Hold 기능 추가 Start//////////////////////////////////////////////////////////////////////
        private string GetMessageWithSubstitution(string messageId, params object[] parameters)
        {
            DataTable dtMessage = GetMessageFromCommonCode(messageId);
            string message = string.Empty;

            if (CommonVerify.HasTableRow(dtMessage))
            {
                message = dtMessage.Rows[0]["CMCDNAME"].ToString();
            }

            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }

            return message;
        }

        private DataTable GetMessageFromCommonCode(string messageId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_HOLD_CONDITION_MSG";
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {

                var resultCmcode = (from t in dtResult.AsEnumerable()
                                    where messageId.Equals(t.Field<string>("CMCODE"))
                                    orderby t.Field<decimal>("CMCDSEQ") ascending
                                    select t);
                if (resultCmcode.Any())
                {
                    return resultCmcode.CopyToDataTable();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        // 롤맵 Hold 기능 추가 Start/////////////////////////////////////////////////////////////////////

        //nathan 2024.01.10 롤맵 코터 수불 실적 적용 - start
        private void SetControlTestCut()
        {
            SelectLossCombo(cboTCTopLoss, "DEFECT_TOP");
            SelectLossCombo(cboTCBackLoss, "DEFECT_BACK");
        }

        private void SelectLossCombo(C1ComboBox cb, string sResnposition)
        {
            try
            {
                cb.ItemsSource = null;

                const string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_ELEC";
                string[] arrColumn = { "LANGID", "AREAID", "LOTID", "ACTID", "RESNPOSITION" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, Util.NVC(new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString()), "LOSS_LOT", sResnposition };
                string selectedValueText = "RESNCODE";
                string displayMemberText = "RESNNAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cb, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, selectedValue: "SELECT");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectTestCut(C1DataGrid dg)
        {
            try
            {
                Util.gridClear(dg);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("OUTPUT_LOTID", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["OUTPUT_LOTID"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                dt.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_TEST_CUT_HIST", "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dg, dtResult, FrameOperation);
                    DataRow dr = dtResult.Rows[0];
                    if (dr["LOSS_APPLY_CNFM_FLAG"].Equals("Y"))
                    {
                        _isTestCutApply = true;
                        txtTCTopLossQty.Text = (string.IsNullOrEmpty(dr["SUM_TOP_LOSS_QTY"].ToString())) ? "0" : dr["SUM_TOP_LOSS_QTY"].ToString();
                        txtTCBackLossQty.Text = (string.IsNullOrEmpty(dr["SUM_BACK_LOSS_QTY"].ToString())) ? "0" : dr["SUM_BACK_LOSS_QTY"].ToString();
                        txtConfirmTime.Text = dr["CNFM_DTTM"].ToString();
                        cboTCTopLoss.SelectedValue = (string.IsNullOrEmpty(dr["TOP_LOSS_CODE"]?.ToString())) ? "SELECT" : dr["TOP_LOSS_CODE"].ToString();
                        cboTCBackLoss.SelectedValue = (string.IsNullOrEmpty(dr["BACK_LOSS_CODE"]?.ToString())) ? "SELECT" : dr["BACK_LOSS_CODE"].ToString();

                        DataRow[] drN = dtResult.Select("LOSS_APPLY_FLAG = 'N'");
                        if (dtResult.Rows.Count == drN.Length)
                            rdoTCApplyN.IsChecked = true;
                        else
                            rdoTCApplyY.IsChecked = true;
                    }
                    else
                    {
                        _isTestCutApply = false;
                        rdoTCApplyY.IsChecked = true;
                    }
                }
                else
                {
                    rdoTCApplyY.IsChecked = true;
                    _isTestCutApply = true; //Test Cut 이력 없을 경우 true 처리
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSaveTestCut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTestCut(dgTestCut)) return;

            if (rdoTCApplyY.IsChecked == true)
            {
                // 선택한 항목을 Loss로 반영하시겠습니까?
                Util.MessageConfirm("SFU5173", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveTestCut(dgTestCut);
                    }
                });
            }
            else if (rdoTCApplyN.IsChecked == true)
            {
                // 전체 항목을 Loss 미반영 하시겠습니까?
                Util.MessageConfirm("SFU5174", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveTestCut(dgTestCut);
                    }
                });
            }
        }

        private void dgTestCut_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            List<DataRow> drList = dgTestCut.GetCheckedDataRow("CHK");

            decimal dSumTopLoss = drList.Sum(row => row.Field<decimal>("DTL_TOP_LOSS"));
            decimal dSumBackLoss = drList.Sum(row => row.Field<decimal>("DTL_BACK_LOSS"));

            txtTCTopLossQty.Text = dSumTopLoss.ToString();
            txtTCBackLossQty.Text = dSumBackLoss.ToString();
        }

        private void SaveTestCut(C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                DataSet ds = new DataSet();

                DataTable dtData = new DataTable("IN_DATA");
                dtData.Columns.Add("OUTPUT_LOTID", typeof(string));
                dtData.Columns.Add("WIPSEQ", typeof(decimal));
                dtData.Columns.Add("TOP_LOSS_CODE", typeof(string));
                dtData.Columns.Add("BACK_LOSS_CODE", typeof(string));
                dtData.Columns.Add("REMARKS", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));
                dtData.Columns.Add("CNFM_USERID", typeof(string));
                dtData.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtData.NewRow();
                //dr["OUTPUT_LOTID"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                //dr["WIPSEQ"] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("WIPSEQ").GetString();
                dr["OUTPUT_LOTID"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                dr["WIPSEQ"] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                if (!string.IsNullOrEmpty(txtTCTopLossQty.Text) && Convert.ToDecimal(txtTCTopLossQty.Text) != 0)
                    dr["TOP_LOSS_CODE"] = Util.GetCondition(cboTCTopLoss);
                else
                    dr["TOP_LOSS_CODE"] = string.Empty;
                if (!string.IsNullOrEmpty(txtTCBackLossQty.Text) && Convert.ToDecimal(txtTCBackLossQty.Text) != 0)
                    dr["BACK_LOSS_CODE"] = Util.GetCondition(cboTCBackLoss);
                else
                    dr["BACK_LOSS_CODE"] = string.Empty;        // dr["TOP_LOSS_CODE"] = string.Empty;

                dr["USERID"] = LoginInfo.USERID;
                dr["CNFM_USERID"] = LoginInfo.USERID;
                dr["EQPTID"] = cboEquipment.SelectedValue; 
                dtData.Rows.Add(dr);

                DataTable dtCut = new DataTable("IN_CUT");
                dtCut.Columns.Add("CUT_RPT_DTTM", typeof(DateTime));
                dtCut.Columns.Add("LOSS_APPLY_FLAG", typeof(string));

                for (int i = 2; i < dg.Rows.Count; i++)
                {
                    dr = dtCut.NewRow();
                    dr["CUT_RPT_DTTM"] = dg.GetValue(i, "CUT_RPT_DTTM");
                    dr["LOSS_APPLY_FLAG"] = (rdoTCApplyN.IsChecked == true) ? "N" : (dg.GetValue(i, "CHK").ToString().Equals("True") || dg.GetValue(i, "CHK").ToString().Equals("1")) ? "Y" : "N";
                    dtCut.Rows.Add(dr);
                }

                ds.Tables.Add(dtData);
                ds.Tables.Add(dtCut);

                //ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TEST_CUT_APPLY_LOSS", "IN_DATA,IN_CUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        //HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _isTestCutApply = true;

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다
                        SelectTestCut(dg);
                    }
                    catch (Exception ex)
                    {
                        //HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationTestCut(C1DataGrid dg)
        {
            // Loss 반영 – YES : 체크박스 선택유무 확인, Loss Code 선택 확인 후 체크한 항목을 Loss로 반영하시겠습니까? 메시지
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1886");     // 정보가 없습니다.
                return false;
            }

            // Loss 반영
            if (rdoTCApplyY.IsChecked == true)
            {
                List<DataRow> drList = dg.GetCheckedDataRow("CHK");
                if (drList.Count == 0)
                {
                    Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                    return false;
                }

                if (!string.IsNullOrEmpty(txtTCTopLossQty.Text) && Convert.ToDecimal(txtTCTopLossQty.Text) != 0 && cboTCTopLoss.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1639");  // 선택된 불량코드가 없습니다
                    return false;
                }

                if (!string.IsNullOrEmpty(txtTCBackLossQty.Text) && Convert.ToDecimal(txtTCBackLossQty.Text) != 0 && cboTCBackLoss.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1639");  // 선택된 불량코드가 없습니다
                    return false;
                }
            }

            return true;
        }

        private void dgTestCut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null) return;
        }

        private void btnRollMapUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // RollMap 실적 수정 Popup Call
                CMM_RM_CT_RESULT popupRollMapUpdate = new CMM_RM_CT_RESULT { FrameOperation = FrameOperation };

                if (popupRollMapUpdate != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = Process.COATING;
                    Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[3] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["LOTID"]);
                    Parameters[4] = Util.NVC(_CURRENT_LOTINFO.Rows[0]["WIPSEQ"]);
                    Parameters[5] = Util.NVC_Decimal(txtLaneQty.Value);
                    Parameters[6] = Util.NVC(cboEquipment.SelectedValue); 
                    Parameters[7] = txtVersion != null ? Util.NVC(txtVersion.Text) : null;
                    Parameters[8] = "N"; //Test Cut Visible false
                    Parameters[9] = "N"; //Search Mode False
                    popupRollMapUpdate.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                    if (popupRollMapUpdate != null)
                    {
                        popupRollMapUpdate.ShowModal();
                        popupRollMapUpdate.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PopupRollMapUpdate_Closed(object sender, EventArgs e)
        {
            ///////////////////// ROLLMAP 실적 수정시 불량/LOSS/물품청구 재조회
            CMM_RM_CT_RESULT popup = sender as CMM_RM_CT_RESULT;
            if (popup.IsUpdated)
            {
                int rowIndex = new Util().GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (rowIndex >= 0) GetDefectList(dgProductLot.Rows[rowIndex].DataItem);
            }
        }

        private void setRollmapResultLinkBtn()
        {
            btnRollMapUpdate.Visibility = Visibility.Collapsed;

            if (_CURRENT_LOTINFO == null) return;
            if (_CURRENT_LOTINFO.Rows.Count == 0) return;
            if (_CURRENT_LOTINFO.Rows[0]["LOTID"].ToString() == "") return; // 여기서는 LOTID 이후에는 _LOTID, 조회 후에 LOTID 에 PR_LOTID 가 들어감.
            if (_isRollMapResultLink == false) return;
            if (_isRollMapEquipment == false) return;

            string strState = _CURRENT_LOTINFO.Rows[0]["WIPSTAT"].ToString();

            if (strState != Wip_State.END && strState != Wip_State.EQPT_END) return;

            btnRollMapUpdate.Visibility = Visibility.Visible;

        }

        public bool CheckTestCut()
        {
            try
            {
                //TEST CUT 이력 존재하는데 LOSS 반영 여부 미확정 일 때
                if (!_isTestCutApply)
                {
                    Util.MessageValidation("SFU5172");     // Test Cut 이력이 존재합니다. Loss 반영 여부를 선택해 주세요.
                    tcDataCollect.SelectedItem = tiTestcut;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }

        private bool IsCommoncodeUse(string sCodeType, string sCmCode)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = sCmCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        //nathan 2024.01.10 롤맵 코터 수불 실적 적용 - end
    }
}