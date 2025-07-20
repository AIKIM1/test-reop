/*************************************************************************************
 Created Date : 2022.04.06
      Creator : 신광희
   Decription : 재와인딩 공정진척(설비Online) - ESNB의 기능(UcElectrodeProductionResult_ReWinding) 참조. 재와인딩 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2022.04.06  신광희 : Initial Created.
  2023.07.07  정기동 : 장착취소 기능 추가, 실적 확정 시 잔량 처리 로직 수정
  2023.09.15  정기동 : 불량/LOSS/물품청구 탭에 TAG 수 입력 항목을 추가 (요청자 : 손서강)
  2024.04.23  양영재 : [E20240319-000559] 특이사항란에 NG TAG1, NG TAG2 컬럼 추가
  2024.06.05  유명환 : [E20240528-000508] ESNA 실적확정 Cell 비활성화
  2024.07.08  김지호 : [E20240606-001253] 실적 확정 시 특이사항 탭에 비고 내용(WIPNOTE)을 WIPACTHISTORY 테이블에 저장 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Windows.Documents;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using System.Threading;


namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private string _selectedLotId;
        private string _selectedWipSeq;
        private string _selectedWipState;

        private string _ldrLotIdentBasCode;
        private string _unldrLotIdentBasCode;
        private string _labelPassHoldFlag = string.Empty;

        private bool _isReWindingProcess;
        private bool _isManageSlittingSide;
        private bool _isSideRollDirectionUse;

        private bool _isDupplicatePopup = false;
        private bool _isChangeQuality = false;      // 품질정보          
        private bool _isChangeRemark = false;       // 특이사항                           

        // RollMap 관련 변수 선언
        private bool _isRollMapEquipment = false;
        private bool _isRollMapResultLink = false;  // 동별 공정별 롤맵 실적 연계 여부
        private bool _isOriginRollMapEquipment = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC002_006()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void Initialize()
        {
            SetComboBox();
            SetReWindingProcess();
            SetIdentInfo();
            InitializeControls();
        }

        private void InitializeControls()
        {
            //btnWorkHalfSlitSide.Visibility = Visibility.Collapsed;               // 무지부 방향설정
            btnEmSectionRollDirctn.Visibility = Visibility.Collapsed;            // 권취방향변경
            SetWorkHalfSlittingSide();

            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
                _isSideRollDirectionUse = true;
            else
                _isSideRollDirectionUse = false;
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            ApplyPermissions();
            SetRollMapEquipment();
            Loaded -= UserControl_Loaded;

            // 재와인딩 New 화면에 NG TAG수 칼럼 추가 요청자 : 손서강 (2023.09.15 JEONG KI TONG)
            if (IsAreaCommonCodeUse("RESN_COUNT_USE_YN", Process.SLIT_REWINDING))
            {
                // C20210928-000539 재와인딩 NG TAG수 칼럼 추가
                if (LoginInfo.CFG_SHOP_ID == "G452" || LoginInfo.CFG_SHOP_ID == "G183")
                    dgWipReason.Columns["COUNTQTY"].Visibility = Visibility.Collapsed;
                else
                    dgWipReason.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                dgWipReason.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetIdentInfo();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetReWindingProcess();
            SetIdentInfo();
            SetWorkHalfSlittingSide();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_isManageSlittingSide)
                GetWorkHalfSlittingSide();

            SetRollMapEquipment();

            if (cboEquipment.SelectedIndex > 0)
                btnSearch_Click(btnSearch, null);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            ClearControls();
            GetProductLot();
        }

        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {
            // Roll Map 호출 
            string mainFormPath = "LGC.GMES.MES.COM001";
            string mainFormName = "COM001_ROLLMAP_REWINDER";

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = Process.COATING;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            parameters[3] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            parameters[4] = new Util().GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetInt();
            parameters[5] = txtLaneQty.Text;
            parameters[6] = cboEquipment.Text;
            parameters[7] = txtVersion.Text.Trim();

            C1Window popupRollMap = obj as C1Window;
            popupRollMap.Closed += new EventHandler(popupRollMap_Closed);
            C1WindowExtension.SetParameters(popupRollMap, parameters);
            if (popupRollMap != null)
            {
                popupRollMap.ShowModal();
                popupRollMap.CenterOnScreen();
            }
        }

        private void popupRollMap_Closed(object sender, EventArgs e)
        {

        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgProductLot_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (sender == null || !CommonVerify.HasDataGridRow(dgProductLot)) return;

            if (dgProductLot.SelectedItem != null)
            {
                DataTableConverter.SetValue(dgProductLot.SelectedItem, "CHK", true);
                int idx = dgProductLot.SelectedIndex;

                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", idx == i);
                }

                SelectProductLot(idx);
            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
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
                    //row 색 바꾸기
                    dgProductLot.SelectedIndex = idx;

                    SetControlClear();

                    //상세 정보 조회
                    SelectProductLot(idx);
                    //ProductListClickedProcess(idx);
                }
            }
        }

        private void dgProductResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.IsReadOnly == true)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }
            }));
        }

        private void dgProductResult_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null) return;

            double dInputQty = Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUT_QTY").GetString());
            double dEqptEndQty = Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_END_QTY").GetString());
            double dCnfmQty = Convert.ToDouble(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CNFM_QTY").GetString());

            if (_selectedWipState == Wip_State.EQPT_END)
            {
                if (dInputQty < dCnfmQty)
                {
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CNFM_QTY", 0);
                    Util.MessageValidation("SFU4418");  // 입력수량이 재공수량보다 클 수 없습니다.
                }
            }
            else
            {
                if (dInputQty < dEqptEndQty)
                {
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "EQPT_END_QTY", 0);
                    Util.MessageValidation("SFU4418");  // 입력수량이 재공수량보다 클 수 없습니다.
                }
            }

            // 생산량 합산
            GetProductionQtySum();

            // 불량 합산, 양품량 산출
            GetDefectSum(false);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            PopupStartReWinding();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelReWinding()) return;

            // 선택된 LOT을 작업 취소하시겠습니까? 
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelProcessReWinding();
                }
            });
        }

        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            EqptEndProcessRewinder();
        }

        private void btnEqptEndCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptEndCancel()) return;

            // 선택된 LOT을 작업 취소하시겠습니까?
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EqptEndCancelProcessRewinder();
                }
            });
        }

        private void btnCard_Click(object sender, RoutedEventArgs e)
        {
            PopupReport();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            dgProductResult.EndEdit();
            if (!ValidationConfirm()) return;

            ///////////////////////////////////////////////  실적확정
            CheckLabelPassHold(() =>
            {
                CheckAuthValidation(() =>
                {
                    CheckSpecOutHold(() =>
                    {
                        if (_isReWindingProcess)
                        {
                            PopupConfirmUser();
                        }
                    });
                });
            });

        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
            parameters[3] = Util.NVC(cboProcess.SelectedValue);
            parameters[4] = Util.NVC(txtShift.Tag);
            parameters[5] = Util.NVC(txtWorker.Tag);
            parameters[6] = Util.NVC(cboEquipment.SelectedValue);
            parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
            C1WindowExtension.SetParameters(wndPopup, parameters);

            wndPopup.Closed += new EventHandler(OnCloseShift);
            Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
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

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PopupPrint();
        }

        private void btnWorkHalfSlitSide_Click(object sender, RoutedEventArgs e)
        {
            PopupWorkHalfSlitSide();
        }

        private void btnEmSectionRollDirctn_Click(object sender, RoutedEventArgs e)
        {
            PopupEmSectionRollDirctn();
        }

        private void btnPilotProdSPMode_Click(object sender, RoutedEventArgs e)
        {
            //ESNB 기준 권한에 따른 버튼 속성을 강제하여 보여줌. ESWA는 해당사항 없음.DA_BAS_SEL_BTN_PERMISSION_GRP_DRB 참조
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            PopupInput(true);
        }

        private void btnInputCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupInput(false);
        }

        /// <summary>
        /// 장착취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 2023-07-07 JEONG KI TONG - Rollmap Project
        private void btnUnMount_Click(object sender, RoutedEventArgs e)
        {
            PopupUnMount();
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnSaveWipReason_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProductLotSelect()) return;

            //불량정보를 저장하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDefect(dgWipReason);
                    if (_isRollMapEquipment) SaveDefectForRollMap();
                }
            });
        }

        private void dgWipReason_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (GetDefectSum() == false)
            {
                if (dataGrid != null)
                {
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);
                }
            }
            else
            {
                //20200701 오화백 태그수 입력시 TAG_CONV_RATE 컬럼값을 곱하여 수량정보 자동입력 되도록 수정
                if (e.Cell.Column.Name.Equals("DFCT_TAG_QTY"))
                {
                    string sTagQty = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_TAG_QTY"));
                    string sTagRate = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAG_CONV_RATE"));

                    double dTagQty = 0;
                    double dTagRate = 0;

                    double.TryParse(sTagQty, out dTagQty);
                    double.TryParse(sTagRate, out dTagRate);
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", dTagQty * dTagRate);
                    dataGrid.UpdateLayout();
                }

                if (_isRollMapEquipment)
                {
                    SetWipReasonCommittedEdit(sender, e);
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
                    DataRow[] row = DataTableConverter.Convert(dataGrid.ItemsSource).Select("DFCT_CODE='" +
                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_CODE")) + "' AND PRCS_ITEM_CODE='GRP_QTY_DIST'");

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

                    #region # Coater Back

                    #endregion

                    #region # RollPress

                    #endregion
                }

                dataGrid.EndEdit();
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

                        if (!_isRollMapEquipment)
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

        private void dgWipReason_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                // 수정 가능여부에 따른 칼럼 처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }

                    // RollMap ActivityReason 수량 변경 금지 처리 [2021-07-27]
                    if (_isRollMapEquipment)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9");
                        if (string.Equals(e.Cell.Column.Name, "RESNQTY") && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y"))
                            if (convertFromString != null) e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        private void dgWipReason_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (e.Column.Name.Equals("DFCT_TAG_QTY") || e.Column.Name.Equals("COUNTQTY") || e.Column.Name.Equals("RESNQTY"))
            {
                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG").GetString()) && DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG").Equals("Y"))
                {
                    e.Cancel = true;
                }
            }

            if (string.Equals(e.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
            {
                e.Cancel = true;
            }

            if (_isRollMapEquipment)
            {
                // RollMap용 수량 변경 금지 처리 [2021-07-27]
                if (string.Equals(e.Column.Name, "RESNQTY") &&
                    (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "PRCS_ITEM_CODE"), "GRP_QTY_DIST") ||
                     string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "DFCT_QTY_UI_CHG_BLOCK_FLAG"), "Y")))
                    e.Cancel = true;
            }
        }

        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationProductLotSelect()) return;
            if (!ValidationRemark(dgQuality)) return;

            SaveQuality(dgQuality);
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
                            else if (string.Equals(e.Cell.Column.Name, "CLSS_NAME1"))
                            {
                                // 필수 검사 항목 여부 색상 표시
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MAND_INSP_ITEM_FLAG")) == "Y")
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                else
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);

                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                                C1DataGrid grid;
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
                        }

                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                            if (e.Cell.Column.Index == dataGrid.Columns["CLSS_NAME1"].Index)
                            {
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                    presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                            }
                            else if (e.Cell.Column.Index == dataGrid.Columns["CLCTVAL01"].Index) // 측정값
                            {
                                if (presenter.HorizontalAlignment != HorizontalAlignment.Right)
                                    presenter.HorizontalAlignment = HorizontalAlignment.Right;

                                decimal sumValue = 0;
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                {
                                    if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非?字"))
                                    {
                                        foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
                                            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                                                sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));


                                        if (sumValue == 0)
                                            presenter.Content = 0;
                                        else
                                            presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dataGrid.Rows.Count - dataGrid.BottomRows.Count), "EA"));
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            dg.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e != null && e.Cell != null && e.Cell.Presenter != null)
                        {
                            e.Cell.Presenter.Background = null;

                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }
            }));
        }

        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = string.Empty;
            string sCLCITEM = string.Empty;

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

                    // 자주검사 USL, LSL 체크
                    DataTable dataCollect = DataTableConverter.Convert(caller.ItemsSource);
                    int iHGCount1 = 0;  // H/G
                    int iHGCount2 = 0;  // M/S
                    int iHGCount3 = 0;  // 1차 H/G
                    int iHGCount4 = 0;  // 1차 M/S
                    decimal sumValue1 = 0;
                    decimal sumValue2 = 0;
                    decimal sumValue3 = 0;
                    decimal sumValue4 = 0;
                    foreach (DataRow row in dataCollect.Rows)
                    {
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount1++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                {
                                    sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                    iHGCount2++;
                                }
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount1++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount2++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount3++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount4++;
                            }
                        }
                    }

                    if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);

                    _isChangeQuality = true;
                }
            }
        }

        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Presenter == null) return;

            // CLCTVAL02 ACTION 재 정의 [2019-03-27]
            if (string.Equals(dg.CurrentCell.Column.Name, "CLCTVAL02"))
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;

                    int iRowIdx = dg.CurrentCell.Row.Index;
                    if ((dg.CurrentCell.Row.Index + 1) < dg.GetRowCount())
                        iRowIdx++;

                    C1.WPF.DataGrid.DataGridCell currentCell = dg.GetCell(iRowIdx, dg.CurrentCell.Column.Index);
                    Util.SetDataGridCurrentCell(dg, currentCell);
                    dg.CurrentCell = currentCell;
                    dg.Focus();
                }
                else if (e.Key == Key.Delete)
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false)
                    {
                        // 이동중 DEL키 입력 시는 측정값 초기화하도록 변경 [2019-04-22]
                        if (dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Presenter != null && dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Presenter.Content != null &&
                            dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Value != null)
                        {
                            ((C1NumericBox)dg.GetCell(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index).Presenter.Content).Value = 0;
                        }
                        else
                        {
                            DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, "CLCTVAL01", null);
                        }
                    }
                }
            }
            else
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dg.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, 0);
                        dg.BeginEdit(dg.CurrentCell);
                        dg.EndEdit(true);

                        DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, DBNull.Value);

                        if (dg.CurrentCell != null && dg.CurrentCell.Presenter != null)
                        {
                            dg.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dg.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dg.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dg.CurrentCell.Column.IsReadOnly == false && dg.CurrentCell.IsEditable == false)
                        dg.BeginEdit(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Index);
                }
            }
        }

        private void OnDataCollectGridFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // 자동차 2동 요구사항으로 인하여 Event재 정의를 함으로써 Focus가 정확히 이동 안하는 현상 때문에 해당 이벤트 추가 [2019-05-01]
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

        protected virtual void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
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

                            DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

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
                            DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

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
                        C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
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
                        C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
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

        protected virtual void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (_isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                _isDupplicatePopup = true;
                int iRowIdx = 0;
                int iColIdx = 0;
                int iMeanColldx = 0;
                C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
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
                        _isChangeQuality = true;
                    }
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox item = sender as ComboBox;
                    StackPanel panel = item.Parent as StackPanel;
                    DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    _isChangeQuality = true;
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
                _isDupplicatePopup = false;
            }
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemark(dgRemark)) return;
            SaveWipNote(dgRemark);
        }

        private void dgRemart_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, dgRemark.Columns[e.Cell.Column.Index].Name)).Equals(""))
            {
                _isChangeRemark = true;
            }

            if (dgRemark.Rows.Count < 1) return;
            if (e.Cell.Row.Index != 0) return;

            string strAll = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[e.Cell.Row.Index].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name));
            string strTmp = "";
            for (int i = 1; i < dgRemark.Rows.Count; i++)
            {
                strTmp = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name));

                if (!string.IsNullOrEmpty(strTmp))
                    strTmp += " " + strAll;
                else
                    strTmp = strAll;

                DataTableConverter.SetValue(dgRemark.Rows[i].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name, strTmp);
            }
            DataTableConverter.SetValue(dgRemark.Rows[0].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name, "");
        }
        #endregion

        #region Mehod

        private bool IsAreaCommonCodeUse(string codeType, string codeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = codeType;
                dr["COM_CODE"] = codeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }

        private bool IsCommonCodeUse(string cmcdType, string areaid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = cmcdType;
                dr["CMCODE"] = areaid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetControlClear()
        {
            _selectedLotId = string.Empty;
            _selectedWipSeq = string.Empty;
            _selectedWipState = string.Empty;
            btnRollMap.IsEnabled = false;

            SetResultDetailControlClear();
            SetDataCollectControlClear();
        }

        private void SetResultDetailControlClear()
        {
            Util.gridClear(dgProductResult);
            txtUnit.Text = string.Empty;
            txtLotID.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtLaneQty.Text = string.Empty;

            txtProductionQty.Value = double.NaN;
            txtGoodQty.Value = double.NaN;
            txtDefectQty.Value = double.NaN;
        }

        private void SetDataCollectControlClear()
        {
            Util.gridClear(dgWipReason);
            Util.gridClear(dgQuality);
            Util.gridClear(dgRemark);
        }

        private void SetWorkHalfSlittingSide()
        {
            _isManageSlittingSide = IsAreaCommonCodeUse("MNG_SLITTING_SIDE_AREA", cboProcess.SelectedValue.GetString());

            if (_isManageSlittingSide)
            {
                btnWorkHalfSlitSide.Visibility = Visibility.Visible;
                SetRollDirection();
            }
        }

        private void SetRollDirection()
        {
            if (IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                btnEmSectionRollDirctn.Visibility = Visibility.Visible;
            }
        }

        private void SetComboBox()
        {
            CommonCombo combo = new CommonCombo();
            string[] sFilter = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            string[] sFilter2 = { "REWINDING_PROCID" };
            C1ComboBox[] cboEquipmentChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild, sFilter: sFilter2, sCase: "COMMCODE");

            C1ComboBox[] cboProcessParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

            cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

            if (cboProcess.SelectedIndex < 0)
            {
                cboProcess.SelectedIndex = 0;
            }

            cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cboEquipment.SelectedIndex < 0)
            {
                cboEquipment.SelectedIndex = 0;
            }

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        private void SetGridSelectRow(string lotId)
        {
            if (!string.IsNullOrEmpty(lotId))
            {
                int idx = -1;
                for (int row = 0; row < dgProductLot.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[row].DataItem, "LOTID")) == lotId)
                    {
                        idx = row;
                        break;
                    }
                }

                if (idx < 0) return;

                dgProductLot.GetCell(idx, dgProductLot.Columns["CHK"].Index);
                RadioButton rb = dgProductLot.GetCell(idx, dgProductLot.Columns["CHK"].Index).Presenter.Content as RadioButton;

                if (rb?.DataContext == null) return;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                }
            }
        }

        private void SetIdentInfo()
        {
            try
            {
                _ldrLotIdentBasCode = string.Empty;
                _unldrLotIdentBasCode = string.Empty;

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _ldrLotIdentBasCode = dtResult.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _unldrLotIdentBasCode = dtResult.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReWindingProcess()
        {
            try
            {
                _isReWindingProcess = false;

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));
                inTable.Columns.Add("CMCDIUSE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "REWINDING_PROCID";
                newRow["CMCODE"] = cboProcess.SelectedValue;
                newRow["CMCDIUSE"] = "Y";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    _isReWindingProcess = true;

                if (_isReWindingProcess)
                {
                    btnInput.Visibility = Visibility.Visible;                        // 이동
                    btnInputCancel.Visibility = Visibility.Visible;                  // 이동취소
                    btnStart.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnUnMount.Visibility = Visibility.Visible;                      // 장착 취소
                    btnWorkHalfSlitSide.Visibility = Visibility.Visible;             // 무지부 방향설정
                }
                else
                {
                    btnInput.Visibility = Visibility.Collapsed;                        // 이동
                    btnInputCancel.Visibility = Visibility.Collapsed;                  // 이동취소
                    btnStart.Visibility = Visibility.Collapsed;
                    btnCancel.Visibility = Visibility.Collapsed;
                    btnUnMount.Visibility = Visibility.Collapsed;                         // 장착 취소
                    btnWorkHalfSlitSide.Visibility = Visibility.Collapsed;             // 무지부 방향설정
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void GetProductLot(string processLotId = null)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["WIPSTAT"] = Wip_State.PROC + "," + Wip_State.EQPT_END;
                inTable.Rows.Add(newRow);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_GET_REWINDER_LOT_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgProductLot, bizResult, null, true);

                        // 라디오 버튼 체크
                        SetGridSelectRow(processLotId);

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

        private void GetWorkHalfSlittingSide()
        {
            try
            {
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WRK_HALF_SLIT_SIDE", "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        txtWorkHalfSlittingSide.Text = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"]);
                        txtWorkHalfSlittingSide.Tag = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"]);
                    }
                    else
                    {
                        txtWorkHalfSlittingSide.Text = string.Empty;
                        txtWorkHalfSlittingSide.Tag = null;
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetDefectSum(bool IsMessage = true)
        {

            double valueToDefect = 0F;
            double valueToLoss = 0F;
            double valueToCharge = 0F;
            double valueToExceedLength = 0F; //길이초과수량
            double totalResnQty = 0;

            int laneqty = int.Parse(txtLaneQty.Text);

            SumDefectTotalQty(ref valueToDefect, ref valueToLoss, ref valueToCharge, ref valueToExceedLength);
            totalResnQty = valueToDefect + valueToLoss + valueToCharge;

            if (txtProductionQty.Value < totalResnQty && IsMessage)
            {
                Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                return false;
            }

            // SET LOT GRID
            txtGoodQty.Value = (txtProductionQty.Value + valueToExceedLength) - (valueToDefect + valueToLoss + valueToCharge);
            txtDefectQty.Value = valueToDefect + valueToLoss + valueToCharge;

            return true;
        }

        private void GetProductionQtySum()
        {
            double inputQty = 0;
            double equipmentEndQty = 0;
            double confirmQty = 0;

            DataTable dt = DataTableConverter.Convert(dgProductResult.ItemsSource);

            foreach (DataRow dr in dt.Rows)
            {
                inputQty += double.Parse(Util.NVC(dr["INPUT_QTY"]));
                equipmentEndQty += double.Parse(Util.NVC(dr["EQPT_END_QTY"]));
                confirmQty += double.Parse(Util.NVC(dr["CNFM_QTY"]));
            }

            if (_selectedWipState == Wip_State.EQPT_END)
            {
                txtProductionQty.Value = confirmQty;
            }
            else
            {
                txtProductionQty.Value = equipmentEndQty;
            }
        }

        private string GetWipNote(string lotId)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = lotId;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (CommonVerify.HasTableRow(dt))
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return string.Empty;
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

        private string GetUnitFormatted(object obj, string pattern)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (pattern)
            {
                case "KG":
                    sFormatted = "{0:###0.000}";
                    break;

                case "M":
                    sFormatted = "{0:###0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:###0.0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private void SumDefectTotalQty(ref double defectSum, ref double lossSum, ref double chargeSum, ref double exceedLength)
        {
            defectSum = 0;
            lossSum = 0;
            chargeSum = 0;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable defectDt = ((DataView)dgWipReason.ItemsSource).Table;

                foreach (DataRow dr in defectDt.Rows)
                {
                    if ((!string.IsNullOrEmpty(dr["RESNQTY"].ToString())) && (!string.Equals(Util.NVC(dr["PRCS_ITEM_CODE"]), "OUT_LOT_QTY_INCR")))
                    {
                        if (!string.Equals(Util.NVC(dr["RSLT_EXCL_FLAG"]), "Y"))
                        {
                            if (string.Equals(Util.NVC(dr["ACTID"]), "DEFECT_LOT"))
                                defectSum += Convert.ToDouble(dr["RESNQTY"]);
                            else if (string.Equals(Util.NVC(dr["ACTID"]), "LOSS_LOT"))
                                lossSum += Convert.ToDouble(dr["RESNQTY"]);
                            else if (string.Equals(Util.NVC(dr["ACTID"]), "CHARGE_PROD_LOT"))
                                chargeSum += Convert.ToDouble(dr["RESNQTY"]);
                        }
                        else
                        {
                            if (string.Equals(Util.NVC(dr["ACTID"]), "LOSS_LOT"))
                                exceedLength = Convert.ToDouble(dr["RESNQTY"]);
                        }
                    }
                }
            }

        }

        private void ClearControls()
        {
            Util.gridClear(dgProductLot);
            SetControlClear();
        }

        private void SelectProductLot(int rowIndex)
        {
            if (rowIndex < 0 || !_util.GetDataGridCheckValue(dgProductLot, "CHK", rowIndex)) return;

            string lotId = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            string wipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            string wipState = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "WIPSTAT"));

            _selectedLotId = lotId;
            _selectedWipSeq = wipSeq;
            _selectedWipState = wipState;

            // LOT별 ROLLMAP 속성 구분 (2021-08-18)
            SetRollMapLotAttribute(_selectedLotId);

            if (string.Equals(wipState, Wip_State.EQPT_END))
            {
                btnRollMap.IsEnabled = true;
            }
            else
            {
                btnRollMap.IsEnabled = false;
            }

            SelectResultDetail(dgProductLot.Rows[rowIndex].DataItem);
            SelectDataCollect();
        }

        private void SelectResultDetail(object selectedItem)
        {
            // 생산실적
            DataRowView rowview = selectedItem as DataRowView;
            if (rowview == null) return;

            txtLotID.Text = rowview["LOTID"].GetString();

            txtVersion.Text = rowview["PROD_VER_CODE"].GetString();
            txtLaneQty.Text = Util.NVC(rowview["LANE_QTY"].GetString()).Equals("") ? 1.ToString() : Util.NVC(rowview["LANE_QTY"].GetString());
            //txtLaneQty.Text = rowview["LANE_QTY"].GetString();            

            txtUnit.Text = rowview["UNIT_CODE"].GetString();

            if (rowview["WIPSTAT"].GetString() == Wip_State.EQPT_END)
            {
                dgProductResult.Columns["EQPT_END_QTY"].IsReadOnly = true;

                if (IsAreaCommonCodeUse("IS_DISABLE_REWINDER_RESULT_CONFIRMATION_CELL", "USE_YN"))
                {
                    dgProductResult.Columns["CNFM_QTY"].IsReadOnly = true;
                }
                else
                {
                    dgProductResult.Columns["CNFM_QTY"].IsReadOnly = false;
                }
            }
            else
            {
                dgProductResult.Columns["EQPT_END_QTY"].IsReadOnly = false;
                dgProductResult.Columns["CNFM_QTY"].IsReadOnly = true;
            }

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPT_END_APPLY_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = rowview["LOTID"].GetString();
                newRow["WIPSEQ"] = rowview["WIPSEQ"].GetString();

                if (rowview["WIPSTAT"].GetString() == Wip_State.EQPT_END)
                {
                    newRow["INPUT_LOT_STAT_CODE"] = Wip_State.EQPT_END;
                    newRow["EQPT_END_APPLY_FLAG"] = "Y";
                }
                else
                {
                    newRow["INPUT_LOT_STAT_CODE"] = "PROC,OUT";
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_REWINDING_WRK_HIST_UPDATE", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //설비완공일 때 완공수량을 확정수량에 입력처리
                        if (_selectedWipState == Wip_State.EQPT_END)
                        {
                            foreach (DataRow dr in bizResult.Rows)
                            {
                                double currentValue = double.Parse(Util.NVC(dr["INPUT_QTY"]));
                                dr["CNFM_QTY"] = currentValue;
                            }

                            Util.GridSetData(dgProductResult, bizResult, null);
                        }
                        else
                        {
                            Util.GridSetData(dgProductResult, bizResult, null);
                        }

                        GetProductionQtySum();
                        GetDefectSum();
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

        private void SelectDataCollect()
        {
            // 불량/LOSS/물품청구
            SelectDefect();

            //품질정보 조회
            SelectQuality();

            //특이사항
            SelectRemark();
        }

        private void SelectDefect()
        {
            try
            {
                const string bizRuleName = "BR_PRD_SEL_ACTIVITYREASON_ELEC"; //DA_PRD_SEL_ACTIVITYREASON_ELEC

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("RESNPOSITION", typeof(string));          // TOP/BACK
                inTable.Columns.Add("CODE", typeof(string));                  // MIX 세정 Option

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = _selectedLotId;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgWipReason, bizResult, null, true);

                        GetDefectSum();
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

        private void SelectQuality()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                inTable.Columns.Add("VER_CODE", typeof(string));
                inTable.Columns.Add("LANEQTY", typeof(Int16));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["LOTID"] = _selectedLotId;
                newRow["WIPSEQ"] = _selectedWipSeq;

                if (!string.IsNullOrWhiteSpace(txtVersion.Text))
                {
                    newRow["VER_CODE"] = txtVersion.Text;
                    newRow["LANEQTY"] = txtLaneQty.Text;
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Rows.Count == 0)
                        {
                            bizResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", inTable);
                        }

                        Util.GridSetData(dgQuality, bizResult, null, true);

                        _util.SetDataGridMergeExtensionCol(dgQuality, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
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

        private void SelectRemark()
        {
            if (dgRemark.GetRowCount() > 0) return;

            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("NG_TAG1", typeof(String));
            dtRemark.Columns.Add("NG_TAG2", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));

            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = "ALL";
            dtRemark.Rows.Add(inDataRow);

            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = _selectedLotId;
            inDataRow["REMARK"] = GetWipNote(_selectedLotId);
            dtRemark.Rows.Add(inDataRow);

            if (IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", cboProcess.SelectedValue.ToString()))
            {
                dgRemark.Columns["NG_TAG1"].Visibility = Visibility.Visible;
                dgRemark.Columns["NG_TAG2"].Visibility = Visibility.Visible;
                inDataRow["NG_TAG1"] = GetNgTagQty(Util.NVC(_selectedLotId), Util.NVC(_selectedWipSeq), cboProcess.SelectedValue.ToString());
                inDataRow["NG_TAG2"] = GetNgTagQty(Util.NVC(_selectedLotId), Util.NVC(_selectedWipSeq), null);
            }
            else
            {
                dgRemark.Columns["NG_TAG1"].Visibility = Visibility.Collapsed;
                dgRemark.Columns["NG_TAG2"].Visibility = Visibility.Collapsed;
            }

            Util.GridSetData(dgRemark, dtRemark, FrameOperation);
        }

        private void SaveDefectForRollMap()
        {
            //string bizRuleName = string.Equals(procId, Process.COATING) ? "BR_PRD_REG_DATACOLLECT_DEFECT_CT" : "BR_PRD_REG_DATACOLLECT_DEFECT_RP";
            //코터 이후 공정은 현재 수불에 대한 변경사항 없이 롤프레스와 동일함.
            string bizRuleName = "BR_PRD_REG_DATACOLLECT_DEFECT_RP";

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
            inDataRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetInt();
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

        private void SaveDefect(C1DataGrid dg, bool bAllSave = false)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                string selectedLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                string selectedWipSeq = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                string selectedLaneQty = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<Int32?>("LANE_PTN_QTY").GetString();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable InResn = inDataSet.Tables.Add("INRESN");
                InResn.Columns.Add("LOTID", typeof(string));
                InResn.Columns.Add("WIPSEQ", typeof(Int32));
                InResn.Columns.Add("ACTID", typeof(string));
                InResn.Columns.Add("RESNCODE", typeof(string));
                InResn.Columns.Add("RESNQTY", typeof(double));
                InResn.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_PTN_QTY", typeof(Int32));
                InResn.Columns.Add("COST_CNTR_ID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = cboProcess.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtDefect = DataTableConverter.Convert(dg.ItemsSource);

                foreach (DataRow row in dtDefect.Rows)
                {
                    newRow = InResn.NewRow();
                    newRow["LOTID"] = selectedLotId;
                    newRow["WIPSEQ"] = selectedWipSeq;
                    newRow["ACTID"] = row["ACTID"];
                    newRow["RESNCODE"] = row["RESNCODE"];
                    newRow["RESNQTY"] = row["RESNQTY"].ToString().Equals("") ? 0.ToString() : row["RESNQTY"];

                    if (dg.Columns["DFCT_TAG_QTY"].Visibility == Visibility.Visible)
                    {
                        newRow["DFCT_TAG_QTY"] = row["DFCT_TAG_QTY"].ToString().Equals("") ? 0.ToString() : row["DFCT_TAG_QTY"].ToString();
                    }
                    else
                    {
                        newRow["DFCT_TAG_QTY"] = 0;
                    }

                    newRow["LANE_QTY"] = txtLaneQty.Text;
                    newRow["LANE_PTN_QTY"] = string.IsNullOrEmpty(selectedLaneQty) ? 0 : selectedLaneQty.GetInt();
                    newRow["COST_CNTR_ID"] = row["COSTCENTERID"];
                    InResn.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (!bAllSave)
                            Util.MessageInfo("SFU3532");     // 저장 되었습니다
                        SelectDefect();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);

                //bProductionUpdate = true;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveDefectBeforeConfirm(C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_ALL";

                dg.EndEdit(true);

                string selectedLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                string selectedWipSeq = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                string selectedLaneQty = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<Int32?>("LANE_PTN_QTY").GetString();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable InResn = inDataSet.Tables.Add("INRESN");
                InResn.Columns.Add("LOTID", typeof(string));
                InResn.Columns.Add("WIPSEQ", typeof(Int32));
                InResn.Columns.Add("ACTID", typeof(string));
                InResn.Columns.Add("RESNCODE", typeof(string));
                InResn.Columns.Add("RESNQTY", typeof(double));
                InResn.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_QTY", typeof(Int32));
                InResn.Columns.Add("LANE_PTN_QTY", typeof(Int32));
                InResn.Columns.Add("COST_CNTR_ID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = cboProcess.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtDefect = DataTableConverter.Convert(dg.ItemsSource);

                foreach (DataRow row in dtDefect.Rows)
                {
                    newRow = InResn.NewRow();
                    newRow["LOTID"] = selectedLotId;
                    newRow["WIPSEQ"] = selectedWipSeq;
                    newRow["ACTID"] = row["ACTID"];
                    newRow["RESNCODE"] = row["RESNCODE"];
                    newRow["RESNQTY"] = row["RESNQTY"].ToString().Equals("") ? 0.ToString() : row["RESNQTY"];

                    if (dg.Columns["DFCT_TAG_QTY"].Visibility == Visibility.Visible)
                    {
                        newRow["DFCT_TAG_QTY"] = row["DFCT_TAG_QTY"].ToString().Equals("") ? 0.ToString() : row["DFCT_TAG_QTY"].ToString();
                    }
                    else
                    {
                        newRow["DFCT_TAG_QTY"] = 0;
                    }

                    newRow["LANE_QTY"] = txtLaneQty.Text;
                    newRow["LANE_PTN_QTY"] = string.IsNullOrEmpty(selectedLaneQty) ? 0 : selectedLaneQty.GetInt();
                    newRow["COST_CNTR_ID"] = row["COSTCENTERID"];
                    InResn.Rows.Add(newRow);
                }

                ShowLoadingIndicator();


                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INRESN", null, inDataSet, FrameOperation.MENUID);
                SelectDefect();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveWipNote(C1DataGrid dg)
        {
            try
            {
                dg.EndEdit(true);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                // 0 Row는 공통특이사항
                for (int row = 1; row < dt.Rows.Count; row++)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(dt.Rows[row]["LOTID"]);
                    newRow["WIP_NOTE"] = Util.NVC(dt.Rows[row]["REMARK"]);
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _isChangeRemark = false;

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다
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

        private void SaveQuality(C1DataGrid dg, bool bAllSave = false)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));
                inTable.Columns.Add("CLCTVAL01", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET     SetWCLCTSeq
                DataTable dtQuality = DataTableConverter.Convert(dg.ItemsSource);

                foreach (DataRow row in dtQuality.Rows)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = _selectedLotId;
                    newRow["EQPTID"] = cboEquipment.SelectedValue;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = row["CLCTITEM"];

                    decimal tmp;
                    if (decimal.TryParse(row["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                        newRow["CLCTVAL01"] = Util.NVC(row["CLCTVAL01"]) == double.NaN.ToString() ? "" : decimal.Parse(Util.NVC(row["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                    else
                        newRow["CLCTVAL01"] = Util.NVC(row["CLCTVAL01"]) == double.NaN.ToString() ? "" : Util.NVC(row["CLCTVAL01"]).Trim().ToString();

                    newRow["WIPSEQ"] = _selectedWipSeq;
                    newRow["CLCTSEQ"] = 1;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        _isChangeQuality = false;

                        if (!bAllSave)
                            Util.MessageInfo("SFU1998");     // 품질 정보가 저장되었습니다.
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

        private void CancelProcessReWinding()
        {
            string selectedLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["LOTID"] = selectedLotId;
                newRow["WIPSEQ"] = selectedWipSeq;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                ////////////////////////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_RW_DRB", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    btnSearch_Click(btnSearch, null);
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void EqptEndProcessRewinder()
        {
            try
            {
                if (!ValidationEqptEndRewinder()) return;

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                InInput.Columns.Add("EQPT_END_QTY", typeof(decimal));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgProductResult.ItemsSource);

                foreach (DataRow row in dt.Rows)
                {
                    newRow = InInput.NewRow();
                    newRow["INPUT_LOTID"] = row["INPUT_LOTID"];
                    newRow["EQPT_END_QTY"] = row["EQPT_END_QTY"];
                    InInput.Rows.Add(newRow);
                }

                // 무지부/권취 두 방향 모두 사용하는 AREA에선 재와인딩 완공 시 무지부/권취 방향 저장할 수 있도록 함
                if (_isSideRollDirectionUse)
                {
                    inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                    inTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                    inTable.Rows[0]["HALF_SLIT_SIDE"] = txtWorkHalfSlittingSide.Tag.GetString().Substring(0, 1);
                    inTable.Rows[0]["EM_SECTION_ROLL_DIRCTN"] = txtWorkHalfSlittingSide.Tag.GetString().Substring(1, 1);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_RW_DRB", "IN_EQP,IN_INPUT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    btnSearch_Click(btnSearch, null);

                    //// 생산 Lot 재조회
                    //SelectProductLot(_dvProductLot["LOTID"].ToString());
                    //SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void EqptEndCancelProcessRewinder()
        {
            string selectedLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Decimal));
                inTable.Columns.Add("USERID", typeof(string));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["LOTID"] = selectedLotId;
                newRow["WIPSEQ"] = selectedWipSeq;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_EQPT_END_LOT_RW_DRB", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.
                    btnSearch_Click(btnSearch, null);

                    // 생산 Lot 재조회
                    //SelectProductLot(_dvProductLot["LOTID"].ToString());
                    //SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ConfirmProcessReWinding()
        {
            try
            {
                string shift = txtShift.Tag.GetString();        //drShift[0]["SHFT_ID"].ToString();
                string workUserID = txtWorker.Tag.GetString();  //
                string workUserName = txtWorker.Text;           //drShift[0]["VAL002"].ToString();

                // 실적 확정 하시겠습니까?
                Util.MessageConfirm("SFU1706", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        ////////////////////////////////////////////////////// DEFECT 자동 저장
                        //SaveDefect(dgWipReason, true);
                        SaveDefectBeforeConfirm(dgWipReason);

                        DataSet inDataSet = new DataSet();
                        DataTable inData = inDataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("WIPSEQ", typeof(decimal));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("INPUTQTY", typeof(decimal));
                        inData.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inData.Columns.Add("OUTPUTQTY2", typeof(decimal));
                        inData.Columns.Add("RESNQTY", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("WIPNOTE", typeof(string));

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("INPUT_SEQNO", typeof(Int64));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));
                        inInput.Columns.Add("CNFM_QTY", typeof(decimal));
                        ///////////////////////////////////////////////////////////////////////////////
                        int LaneQty = string.IsNullOrWhiteSpace(_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<Int32?>("LANE_QTY").GetString()) ? 1 : int.Parse(_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<Int32?>("LANE_QTY").GetString());
                        int LanePtnQty = string.IsNullOrWhiteSpace(_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<Int32?>("LANE_PTN_QTY").GetString()) ? 1 : int.Parse(_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<Int32?>("LANE_PTN_QTY").GetString());

                        DataRow newRow = inData.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = cboEquipment.SelectedValue;
                        newRow["PROCID"] = cboProcess.SelectedValue;
                        newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                        newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                        newRow["SHIFT"] = shift;
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["INPUTQTY"] = txtProductionQty.Value;
                        newRow["OUTPUTQTY"] = txtGoodQty.Value;
                        newRow["OUTPUTQTY2"] = txtGoodQty.Value * LaneQty;
                        newRow["RESNQTY"] = txtDefectQty.Value;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["WIPNOTE"] = DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "REMARK");
                        inData.Rows.Add(newRow);
                        DataTable dt = DataTableConverter.Convert(dgProductResult.ItemsSource);

                        foreach (DataRow row in dt.Rows)
                        {
                            newRow = inInput.NewRow();
                            newRow["INPUT_SEQNO"] = row["INPUT_SEQNO"];
                            newRow["INPUT_LOTID"] = row["INPUT_LOTID"];
                            newRow["CNFM_QTY"] = row["CNFM_QTY"];
                            inInput.Rows.Add(newRow);
                        }

                        //string xml = inDataSet.GetXml();
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_RW_DRB", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                btnSearch_Click(btnSearch, null);

                                // 생산 Lot 재조회
                                //SelectProductLot(string.Empty);
                                //SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CheckLabelPassHold(Action callback)
        {
            try
            {
                //라벨링 패스 기능은 기존에도 활성화 되어 있었음
                _labelPassHoldFlag = "Y";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("CMCDTYPE");
                inTable.Columns.Add("CMCODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "LABEL_PASS_HOLD_CHECK";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(rslt))
                {
                    if (!ValidLabelPass())
                    {
                        // Labeling Pass를 진행한 이력이 있습니다. 홀드하시겠습니까?
                        Util.MessageConfirm("SFU8218", result =>
                        {
                            //홀드
                            //전극의 모든 실적확정 로직에 플래그를 넣어서 Y면 자주검사스펙 홀드를 통과하도록하고 N이면 넘긴다
                            //MMD 동별 자주검사에서 자동홀드여부도 Y로 바꿔야 한다.
                            //또한 홀드를 걸 항목에 대해서만 LSL USL을 등록한다.
                            if (result == MessageBoxResult.OK)
                            {
                                _labelPassHoldFlag = "Y";
                            }
                            else
                            {
                                _labelPassHoldFlag = "N";
                            }
                            callback();
                        });
                    }
                    else
                    {
                        callback();
                    }
                }
                else
                {
                    callback();
                }
            }
            catch (Exception) { }
        }

        private void CheckAuthValidation(Action callback)
        {
            try
            {
                // AD 인증 기능 추가 [2019-08-21]
                DataTable confirmDt = GetConfirmAuthVaildation();

                if (confirmDt != null && confirmDt.Rows.Count > 0)
                {
                    // 강제 인터락 체크 (이거는 공용 메세지로 공유하니 필요 시 MES MESSAGE 코드 별도 추가 필요)
                    if (string.Equals(confirmDt.Rows[0]["VALIDATION_FLAG"], "Y"))
                    {
                        // 실적확정은 자동 Interlock 기능에 의하여 보류 되었습니다. [%1]
                        Util.MessageValidation("SFU5125", new object[] { Util.NVC(confirmDt.Rows[0]["RSLT_CNFM_TYPE_CODE"]) });
                        return;
                    }

                    // AD 인증 체크
                    if (string.Equals(confirmDt.Rows[0]["AD_CHK_FLAG"], "Y"))
                    {
                        CMM_COM_AUTH_CONFIRM authConfirm = new CMM_COM_AUTH_CONFIRM();
                        authConfirm.FrameOperation = FrameOperation;
                        authConfirm.sContents = Util.NVC(confirmDt.Rows[0]["DISP_MSG"]);
                        if (authConfirm != null)
                        {
                            // SBC AD 인증
                            if (string.Equals(confirmDt.Rows[0]["AD_CHK_TYPE_CODE"], "SBC_AD"))
                            {
                                object[] Parameters = new object[1];
                                Parameters[0] = Util.NVC(confirmDt.Rows[0]["AUTHID"]);

                                C1WindowExtension.SetParameters(authConfirm, Parameters);

                            }
                            else if (string.Equals(confirmDt.Rows[0]["AD_CHK_TYPE_CODE"], "LGCHEM_AD"))
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = Util.NVC(confirmDt.Rows[0]["AUTHID"]);
                                Parameters[1] = "lgchem.com";

                                C1WindowExtension.SetParameters(authConfirm, Parameters);
                            }
                            authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                            this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
                        }
                    }
                    else
                    {
                        callback();
                    }
                }
                else
                {
                    callback();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM window = sender as CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                CheckSpecOutHold(() => { ConfirmCheck(); });
            }
        }

        private void ConfirmCheck()
        {
            if (cboProcess.SelectedValue.GetString() != Process.MIXING)
            {
                // 선분산믹서, B/S, CMC , Roll Pressing
                PopupConfirmUser();
                return;
            }
        }

        private DataTable GetConfirmAuthVaildation()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
            newRow["PROCID"] = cboProcess.SelectedValue;
            newRow["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CNFM_AUTH", "INDATA", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                // 인증 여부
                bool isAuthConfirm = true;

                // Input용 데이터 테이블 ( INPUTVALUE : 비교할 대상 값, CHK_VALUE1 : SPEC1, CHK_VALUE2 : SPEC2 )
                DataTable inputTable = new DataTable();
                inputTable.Columns.Add("CHK_VALUE1", typeof(decimal));
                inputTable.Columns.Add("CHK_VALUE2", typeof(decimal));
                inputTable.Columns.Add("INPUTVALUE", typeof(decimal));

                foreach (DataRow row in dtResult.Rows)
                {
                    inputTable.Clear();

                    if (!string.IsNullOrEmpty(Util.NVC(row["CHK_VALUE1"])) || !string.IsNullOrEmpty(Util.NVC(row["CHK_VALUE2"])))
                    {
                        DataRow dataRow = inputTable.NewRow();
                        dataRow["CHK_VALUE1"] = row["CHK_VALUE1"];
                        dataRow["CHK_VALUE2"] = row["CHK_VALUE2"];
                        inputTable.Rows.Add(dataRow);
                    }

                    if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_GOOD_QTY_LIMIT"))
                    {
                        // 양품량 기준 체크
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(txtGoodQty.Value);
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);

                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_PROD_QTY_LIMIT"))
                    {
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(txtProductionQty.Value);
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);
                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "QA_SPEC_LIMIT"))
                    {
                        // USL,LSL 제한 체크
                        isAuthConfirm = ValidQualitySpec("Auth");
                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "QA_AVG_LIMIT"))
                    {
                        // 품질평균값 제한 체크

                    }

                    // 인증이 필요한 경우 전체 정보 전달
                    if (isAuthConfirm == false)
                    {
                        DataTable outTable = dtResult.Clone();
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //outTable.Rows.Add(row.ItemArray);
                        outTable.AddDataRow(row);
                        return outTable;
                    }
                }
            }
            return new DataTable();
        }

        private bool ValidQualitySpec(string validType)
        {
            bool bRet = true;
            try
            {
                DataTable qualityList = dgQuality.ItemsSource == null ? null : ((DataView)dgQuality.ItemsSource).ToTable();
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;

                if (qualityList != null && qualityList.Rows.Count > 0)
                {
                    for (int i = 0; i < qualityList.Rows.Count; i++)
                    {
                        LSL = qualityList.Rows[i]["LSL"].ToString();
                        USL = qualityList.Rows[i]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList.Rows[i]["AUTO_HOLD_FLAG"].ToString();

                        //yield LSL USL => 또 팝업 => 예를 누르면 실적확정되면서 무조건 홀드

                        if (!qualityList.Rows[i]["CLCTVAL01"].ToString().Equals("NaN"))
                        {
                            CLCTVAL = qualityList.Rows[i]["CLCTVAL01"].ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(LSL) && !string.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            //validType이 Hold면 자동보류여부를 체크하고
                            //아니면 체크 안하고
                            if (Util.NVC_Int(LSL) > 0 && Util.NVC_Int(LSL) > Util.NVC_Int(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(USL) && !string.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (Util.NVC_Int(USL) > 0 && Util.NVC_Int(USL) < Util.NVC_Int(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                bRet = true;
            }

            return bRet;
        }

        private bool CheckLimitValue(string sCheckType, DataTable inputTable)
        {
            foreach (DataRow row in inputTable.Rows)
            {
                switch (sCheckType)
                {
                    case "LOWER":           // SPEC LOWER
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "UPPER":           // SPEC UPPER
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "BOTH":            // SPEC IN
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE1"]) &&
                            Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE2"]))
                            return false;

                        break;

                    case "NOT_BOTH":        // SPEC OUT
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE1"]) &&
                            Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE2"]))
                            return false;

                        break;
                    case "VALUE":
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) == Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "NOT_VAULE":
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) != Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        private void CheckSpecOutHold(Action callback)
        {
            try
            {
                //자동보류여부에 체크되어 있으면 무조건 홀드를 건다.
                if (!ValidQualitySpec("Hold"))
                {
                    //자동HOLD되도록 설정된 품질검사 결과가 기준치를 만족하지 못했습니다. 완성랏이 홀드됩니다. 계속하시겠습니까?
                    Util.MessageConfirm("SFU8185", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            callback();
                        }
                    });
                }
                else
                {
                    if (!ValidQualitySpecExists())
                    {
                        //LSL, USL 미설정되어 계속 진행할 경우 완성LOT이 HOLD처리 됩니다. 계속하시겠습니까?
                        Util.MessageConfirm("SFU8186", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                callback();
                            }
                        });
                    }
                    else
                    {
                        callback();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidQualitySpecExists()
        {
            bool bRet = true;
            try
            {
                DataTable qualityList = dgQuality.ItemsSource == null ? null : ((DataView)dgQuality.ItemsSource).ToTable();
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;

                if (qualityList != null && qualityList.Rows.Count > 0)
                {
                    for (int i = 0; i < qualityList.Rows.Count; i++)
                    {
                        LSL = qualityList.Rows[i]["LSL"].ToString();
                        USL = qualityList.Rows[i]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList.Rows[i]["AUTO_HOLD_FLAG"].ToString();

                        if (AUTO_HOLD_FLAG.Equals("Y"))
                        {
                            if (string.IsNullOrWhiteSpace(LSL) || string.IsNullOrWhiteSpace(USL))
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                bRet = true;
            }

            return bRet;
        }

        private void PopupStartReWinding()
        {
            if (!ValidationStartReWinding()) return;

            ELEC002_006_LOTSTART popupStartReWinding = new ELEC002_006_LOTSTART();
            popupStartReWinding.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            parameters[3] = cboEquipment.Text;
            C1WindowExtension.SetParameters(popupStartReWinding, parameters);

            popupStartReWinding.Closed += new EventHandler(PopupStartReWinding_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupStartReWinding.ShowModal()));
            popupStartReWinding.CenterOnScreen();
        }

        private void PopupStartReWinding_Closed(object sender, EventArgs e)
        {
            ELEC002_006_LOTSTART popup = sender as ELEC002_006_LOTSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void PopupWorkHalfSlitSide()
        {
            if (!ValidationWorkHalfSlitSide()) return;

            // 무지부/권취 방향 2종류 모두 사용하는 AREA 일 경우
            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN { FrameOperation = FrameOperation };
                popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                btnExtra.IsDropDownOpen = false;

                object[] parameters = new object[2];
                parameters[0] = cboEquipment.SelectedValue;
                parameters[1] = txtWorkHalfSlittingSide.Tag;

                C1WindowExtension.SetParameters(popupWorkHalfSlitSide, parameters);

                popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING { FrameOperation = FrameOperation };

                popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                btnExtra.IsDropDownOpen = false;

                object[] parameters = new object[2];
                parameters[0] = cboEquipment.SelectedValue;
                parameters[1] = txtWorkHalfSlittingSide.Tag;

                C1WindowExtension.SetParameters(popupWorkHalfSlitSide, parameters);

                popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
            }
        }

        private void PopupWorkHalfSlitSide_Closed(object sender, EventArgs e)
        {
            // 무지부/권취 방향 2종류 모두 사용하는 AREA 일 경우
            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popup = sender as CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popup = sender as CMM_ELEC_WORK_HALF_SLITTING;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                }
            }
        }

        private void PopupEmSectionRollDirctn()
        {
            if (!ValidationEmSectionRollDirctn()) return;

            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popupEmSectionRollDirctn = new CMM_ELEC_EM_SECTION_ROLL_DIRCTN { FrameOperation = FrameOperation };
            popupEmSectionRollDirctn.FrameOperation = FrameOperation;
            btnExtra.IsDropDownOpen = false;

            object[] parameters = new object[1];
            parameters[0] = cboEquipment.SelectedValue;

            C1WindowExtension.SetParameters(popupEmSectionRollDirctn, parameters);

            popupEmSectionRollDirctn.Closed += new EventHandler(PopupEmSectionRollDirctn_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupEmSectionRollDirctn.ShowModal()));
        }

        private void PopupEmSectionRollDirctn_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popup = sender as CMM_ELEC_EM_SECTION_ROLL_DIRCTN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupInput(bool isInputYn)
        {
            if (!ValidationInput()) return;

            ELEC002_006_INPUT_CANCEL popupInput = new ELEC002_006_INPUT_CANCEL();
            popupInput.FrameOperation = FrameOperation;

            string selectedLotId = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            object[] parameters = new object[8];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            parameters[3] = cboEquipment.Text;
            parameters[4] = selectedLotId;
            parameters[5] = selectedWipSeq;
            parameters[6] = isInputYn ? "Y" : "N";                // 투입 Y, 투입취소 N
            parameters[7] = _ldrLotIdentBasCode;

            C1WindowExtension.SetParameters(popupInput, parameters);

            popupInput.Closed += new EventHandler(PopupInput_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupInput.ShowModal()));                        
            popupInput.CenterOnScreen();

        }

        /// <summary>
        /// 장착취소
        /// </summary>
        private void PopupUnMount()
        {
            if (!ValidationUnMount()) return;

            CMM_ROLLMAP_RW_UNMOUNT popupUnMount = new CMM_ROLLMAP_RW_UNMOUNT();
            popupUnMount.FrameOperation = FrameOperation;

            object[] parameters = new object[8];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            parameters[3] = cboEquipment.Text;
            //parameters[4] = selectedLotId;
            //parameters[5] = selectedWipSeq;
            //parameters[6] = isInputYn ? "Y" : "N";                // 투입 Y, 투입취소 N
            //parameters[7] = _ldrLotIdentBasCode;

            C1WindowExtension.SetParameters(popupUnMount, parameters);

            popupUnMount.Closed += new EventHandler(PopupUnMount_Closed);           // Close 이벤트 핸들러
            Dispatcher.BeginInvoke(new Action(() => popupUnMount.ShowModal()));
            popupUnMount.CenterOnScreen();
        }

        private void PopupConfirmUser()
        {
            ELEC002_CONFIRM_USER popupConfirmUser = new ELEC002_CONFIRM_USER();
            popupConfirmUser.FrameOperation = FrameOperation;

            if (popupConfirmUser != null)
            {
                // TODO parameters[6] 이후 확인 필요 함.
                object[] parameters = new object[11];
                parameters[0] = cboEquipmentSegment.SelectedValue;
                parameters[1] = cboProcess.SelectedValue;
                parameters[2] = cboProcess.Text;
                parameters[3] = cboEquipment.SelectedValue;
                parameters[4] = cboEquipment.Text;
                parameters[5] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();

                parameters[6] = txtShift.Tag;
                parameters[7] = txtShiftStartTime.Text;
                parameters[8] = txtShiftEndTime.Text;
                parameters[9] = txtWorker.Tag;
                parameters[10] = txtWorker.Text;

                C1WindowExtension.SetParameters(popupConfirmUser, parameters);

                popupConfirmUser.Closed += new EventHandler(PopupConfirmUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupConfirmUser.ShowModal()));
                popupConfirmUser.CenterOnScreen();
            }
        }

        private void PopupConfirmUser_Closed(object sender, EventArgs e)
        {
            ELEC002_CONFIRM_USER popup = sender as ELEC002_CONFIRM_USER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                SaveRealWorker(popup.ConfirmkUserName);

                if (_isReWindingProcess)
                    ConfirmProcessReWinding();
            }
        }


        private void PopupInput_Closed(object sender, EventArgs e)
        {
            ELEC002_006_INPUT_CANCEL popup = sender as ELEC002_006_INPUT_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }
                
        }

        private void PopupUnMount_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_RW_UNMOUNT popup = sender as CMM_ROLLMAP_RW_UNMOUNT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }            
        }


        private void PopupReport()
        {
            if (!ValidationReport()) return;

            CMM_ELEC_REPORT2 popupReport = new CMM_ELEC_REPORT2();
            popupReport.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
            parameters[1] = cboProcess.SelectedValue;

            C1WindowExtension.SetParameters(popupReport, parameters);

            this.Dispatcher.BeginInvoke(new Action(() => popupReport.ShowModal()));
            popupReport.CenterOnScreen();
        }

        private void PopupPrint()
        {
            if (!ValidationPrint()) return;

            CMM_ELEC_BARCODE popupPrint = new CMM_ELEC_BARCODE();
            popupPrint.FrameOperation = FrameOperation;
            object[] parameters = new object[3];
            parameters[0] = cboEquipmentSegment.SelectedValue;
            parameters[1] = cboProcess.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;

            C1WindowExtension.SetParameters(popupPrint, parameters);

            this.Dispatcher.BeginInvoke(new Action(() => popupPrint.ShowModal()));
            popupPrint.CenterOnScreen();
        }

        private bool ValidationSearch()
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return false;
            }

            if (cboProcess.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU3207");  //공정을 선택하세요.
                return false;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationInput()
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (cboProcess.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 장착취소를 위한 값 체크
        /// </summary>
        /// <returns></returns>
        private bool ValidationUnMount()
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (cboProcess.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");     // 설비를 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationStartReWinding()
        {
            if (cboProcess.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");     // 라인을 선택하세요.
                return false;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationCancelReWinding()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }
            return true;
        }

        private bool ValidationEqptEndRewinder()
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("WIPSTAT").GetString() != "PROC")
            {
                Util.MessageValidation("SFU2957");     // 진행중인 작업을 선택하세요.
                return false;
            }

            if (txtProductionQty.Value == double.NaN || txtProductionQty.Value == 0)
            {
                Util.MessageValidation("SFU1609");     // 생산량을 입력하십시오.
                return false;
            }

            if (_isSideRollDirectionUse && txtWorkHalfSlittingSide.Tag == null || string.IsNullOrEmpty(txtWorkHalfSlittingSide.Text))
            {
                Util.MessageValidation("SFU6030");  // 무지부 방향을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEqptEndCancel()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("WIPSTAT").GetString() == "WAIT")
            {
                Util.MessageValidation("SFU1939");     // 취소 할 수 있는 상태가 아닙니다.
                return false;
            }

            if (_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("WIPSTAT").GetString() == "PROC")
            {
                Util.MessageValidation("SFU3464");     // 진행중인 LOT은 장비완료취소 할 수 없습니다. [진행중인 LOT은 시작취소 버튼으로 작업취소 바랍니다.]
                return false;
            }
            return true;
        }

        private bool ValidationConfirm()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("WIPSTAT").GetString() != "EQPT_END")
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(txtShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (txtProductionQty.Value == double.NaN || txtProductionQty.Value == 0)
            {
                Util.MessageValidation("SFU1609");     // 생산량을 입력하십시오.
                return false;
            }

            if (!ValidDataCollect(_isChangeQuality, false, _isChangeRemark)) return false;

            return true;
        }

        private bool ValidDataCollect(bool isChangeQuality, bool isChangeMaterial, bool isChangeRemark)
        {
            if (isChangeQuality)
            {
                Util.MessageValidation("SFU1999");     // 품질 정보를 저장하세요.
                return false;
            }

            if (isChangeRemark)
            {
                Util.MessageValidation("SFU2977");     // 특이사항 정보를 저장하세요.
                return false;
            }

            return true;
        }

        private bool ValidationWorkHalfSlitSide()
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEmSectionRollDirctn()
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationProductLotSelect()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }
            return true;
        }

        private bool ValidationRemark(C1DataGrid dg)
        {
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1886");     // 정보가 없습니다.
                return false;
            }
            return true;
        }

        private bool ValidLabelPass()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID");
            inTable.Columns.Add("EQPTID");

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("CUT_ID").GetString();
            newRow["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(newRow);

            DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DATA_ECLCT", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(rslt))
                return false;
            else
                return true;
        }

        private bool ValidationPrint()
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            //if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationReport()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }
            return true;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnStart,
                btnCancel,
                btnEqptEnd,
                btnEqptEndCancel,
                btnConfirm,
                btnCard,
                btnPrint
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #region 작업자 실명관리 기능 추가
        private void SaveRealWorker(string workerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(dgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["WORKER_NAME"] = workerName;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

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

        #region # Roll Map
        private void SetRollMapEquipment()
        {
            _isRollMapResultLink = IsRollMapResultApply();
            _isRollMapEquipment = IsEquipmentAttr(Util.NVC(cboEquipment.SelectedValue));
            _isOriginRollMapEquipment = _isRollMapEquipment;

            // # Roll Map 대상설비에 따른 컨트롤 정의
            VisibleRollMapMode();
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

        private void VisibleRollMapMode()
        {
            if (_isRollMapEquipment)
            {
                btnRollMap.Visibility = Visibility.Visible;
            }
            else
            {
                btnRollMap.Visibility = Visibility.Collapsed;
            }

            /*
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
            */
        }


        #endregion

        #region NG TAG 수량 조회
        // 20240405 NG TAG수 칼럼 추가
        private string GetNgTagQty(string sLotID, string sWipSeq, string sProcid)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            indata["WIPSEQ"] = sWipSeq;
            indata["PROCID"] = sProcid;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_NG_TAG_QTY", "RQSTDT", "RSLTDT", inTable);

            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["DFCT_TAG_QTY"]);
            }
            else
            {
                return "0";
            }
        }
        #endregion


    }
}
