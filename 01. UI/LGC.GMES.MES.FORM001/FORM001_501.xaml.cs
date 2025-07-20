/*************************************************************************************
 Created Date : 2018.09.17
      Creator : 
   Decription : 자동차 활성화 후공정 - 직행 외관 불량 Lot 관리
--------------------------------------------------------------------------------------
 [Change History]
 2019-01-17 : 1.검색조건 text box 길이 고정
              2.직행,재작업 불량 등록시 불량코드 선택 가능하게 수정
 2019-01-23 : 1.실적일자 칼럼 추가
              2.불량 Cell 스캔시 라인 체크 추가
 2019-05-13 : 1.이력 조회 시, Grid 초기화 후 조회하도록 수정
 
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.UserControls;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_501 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private CheckBoxHeaderType _HeaderTypeCell;
        private CheckBoxHeaderType _HeaderTypeCellDefect;
        private CheckBoxHeaderType _HeaderTypeCellUpdate;

        private bool _IsKeyCV = false;
        private bool _IsLoopEnd = false;

        DataTable _dtCell;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }


        public FORM001_501()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            _HeaderTypeCell = CheckBoxHeaderType.One;
            _HeaderTypeCellDefect = CheckBoxHeaderType.Zero;
            _HeaderTypeCellUpdate = CheckBoxHeaderType.Zero;

            if (((System.Windows.FrameworkElement)tabDefect.SelectedItem).Name.Equals("ctbCreate"))
            {
                Util.gridClear(dgCreate);

                txtCellID.Text = string.Empty;
                txtScanQtyCre.Value = 0;
                //txtWorkUserCre.Text = string.Empty;
                //cboShiftCre.SelectedIndex = 0;

                if (_dtCell != null)
                    _dtCell.Clear();

                txtCellID.Focus();
            }
            else if (((System.Windows.FrameworkElement)tabDefect.SelectedItem).Name.Equals("ctbDefectLot"))
            {
                cboResnCode.SelectedIndex = 0;
                txtInputQty.Value = 0;
                txtWorkUser.Text = string.Empty;
                txtResnNote.Text = string.Empty;
                txtCellIDLot.Text = string.Empty;
                cboShift.SelectedIndex = -1;

                Util.gridClear(dgDefectCell);
            }
            if (((System.Windows.FrameworkElement)tabDefect.SelectedItem).Name.Equals("ctbCell"))
            {
                txtScanQty.Value = 0;
                Util.gridClear(dgDefectCellCancel);
            }


        }
        private void SetControl()
        {
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //////////////////////////////////////// 불량LOT 생성
            SetcboEquipmentSegmentCreCombo(cboEquipmentSegmentCre);
            cboEquipmentSegmentCre.SelectedValueChanged += cboEquipmentSegmentCre_SelectedValueChanged;

            SetProcessCombo(cboProcessCre);
            cboProcessCre.SelectedValueChanged += cboProcessCre_SelectedValueChanged;

            SetEquipmentCombo(cboEquipmentCre, cboProcessCre);
            cboEquipmentCre.SelectedValueChanged += cboEquipmentCre_SelectedValueChanged;

            //SetShiftCreateCombo(cboShiftCre);
            SetResnCodeCreateCombo(cboResnCodeCre);

            //////////////////////////////////////// 외관 불량 Lot 정보
            //동
            C1ComboBox[] cboAreaLotChild = { cboEquipmentSegmentLot };
            _combo.SetCombo(cboAreaLot, CommonCombo.ComboStatus.NONE, cbChild: cboAreaLotChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentLotParent = { cboAreaLot };
            //C1ComboBox[] cboEquipmentSegmentLotChild = { cboProcessLot };
            _combo.SetCombo(cboEquipmentSegmentLot, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_AUTO", cbParent: cboEquipmentSegmentLotParent);

            //공정
            //C1ComboBox[] cboProcessLotParent = { cboEquipmentSegmentLot };
            //_combo.SetCombo(cboProcessLot, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbParent: cboProcessLotParent);
            SetProcessCombo(cboProcessLot);

            //////////////////////////////////////// 외관 불량 Lot 생성 이력
            //동
            C1ComboBox[] cboAreaHisChild = { cboEquipmentSegmentHis };
            _combo.SetCombo(cboAreaHis, CommonCombo.ComboStatus.NONE, cbChild: cboAreaHisChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentHisParent = { cboAreaHis };
            //C1ComboBox[] cboEquipmentSegmentHisChild = { cboProcessHis };
            _combo.SetCombo(cboEquipmentSegmentHis, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_AUTO", cbParent: cboEquipmentSegmentHisParent);

            //공정
            //C1ComboBox[] cboProcessHisParent = { cboEquipmentSegmentHis };
            //_combo.SetCombo(cboProcessHis, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbParent: cboProcessHisParent);
            SetProcessCombo(cboProcessHis);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDelete);
            listAuth.Add(btnSave);
            listAuth.Add(btnPrint);
            listAuth.Add(btnSearchLot);
            listAuth.Add(btnModify);
            listAuth.Add(btnPrintLot);
            listAuth.Add(btnSearchHis);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void tabDefect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1TabControl c1Tab = sender as C1TabControl;

            if (((System.Windows.FrameworkElement)tabDefect.SelectedItem).Name.Equals("ctbDefectLot"))
            {
                SetUpdateCancelEnable();
            }
            else if (((System.Windows.FrameworkElement)tabDefect.SelectedItem).Name.Equals("ctbCell"))
            {
                txtDefectCell.Focus();
            }
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        #region [외관 불량 Lot 생성]
        private void cboEquipmentSegmentCre_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShiftCreateCombo(cboShiftCre);
            txtCellID.Focus();
        }

        private void cboProcessCre_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitializeUserControls();

            //if (cboProcessCre.SelectedIndex == 0 || cboProcessCre.SelectedValue.ToString().Equals("SELECT")) return;

            SetEquipmentCombo(cboEquipmentCre, cboProcessCre);
            SetShiftCreateCombo(cboShiftCre);
            SetResnCodeCreateCombo(cboResnCodeCre);

            txtCellID.Focus();
        }

        private void cboEquipmentCre_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtCellID.Focus();
        }

        private void txtCellID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    _IsKeyCV = true;
                    _IsLoopEnd = false;

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    ShowLoadingIndicator();

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        txtCellID.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtCellID.Text))
                            txtCellID_KeyDown(txtCellID, null);

                        if (_IsLoopEnd) break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    HiddenLoadingIndicator();

                    _IsKeyCV = false;

                    if (_dtCell == null) return;

                    Util.GridSetData(dgCreate, _dtCell, null, true);
                    // 스캔수량
                    txtScanQtyCre.Value = _dtCell.Rows.Count;
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();

                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    // 라인, 공정 체크
                    if (!ValidationScanValue())
                    {
                        txtCellID.Text = string.Empty;
                        txtCellID.Focus();
                        return;
                    }

                    string sCellID = txtCellID.Text.Trim();

                    // DataTable 칼럼 생성
                    if (_dtCell == null)
                    {
                        _dtCell = new DataTable();
                        for (int col = 0; col < dgCreate.Columns.Count; col++)
                        {
                            if (dgCreate.Columns[col].Name.Equals("CHK"))
                                _dtCell.Columns.Add(dgCreate.Columns[col].Name.ToString(), typeof(Boolean));
                            else
                                _dtCell.Columns.Add(dgCreate.Columns[col].Name.ToString(), typeof(string));
                        }
                    }

                    // 불량코드, SUBLOT 체크
                    DataSet inDataSet = new DataSet();
                    DataTable inTable = inDataSet.Tables.Add("INDATA");
                    inTable.Columns.Add("LANGID");
                    inTable.Columns.Add("AREAID");
                    inTable.Columns.Add("PROCID");
                    inTable.Columns.Add("SCANVALUE");
                    inTable.Columns.Add("RWK_FLAG");
                    inTable.Columns.Add("EQSGID");

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PROCID"] = cboProcessCre.SelectedValue.ToString();
                    dr["SCANVALUE"] = sCellID;
                    dr["RWK_FLAG"] = "N";
                    dr["EQSGID"] = cboEquipmentSegmentCre.SelectedValue.ToString();
                    inTable.Rows.Add(dr);

                    DataSet drResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_SCANVALUE_AUTO", "INDATA", "OUT_RESNCODE,OUT_SUBLOT", inDataSet);

                    if (drResult == null)
                    {
                        txtCellID.Text = string.Empty;
                        txtCellID.Focus();
                        return;
                    }

                    ////////////////////////////////////////////////
                    txtCellID.SelectAll();

                    if (drResult.Tables["OUT_RESNCODE"].Rows.Count > 0)
                    {
                        _IsLoopEnd = true;
                        cboResnCodeCre.SelectedValue = drResult.Tables["OUT_RESNCODE"].Rows[0]["RESNCODE"].ToString();
                    }
                    else if (drResult.Tables["OUT_SUBLOT"].Rows.Count > 0)
                    {
                        if (cboResnCodeCre.SelectedValue == null || cboResnCodeCre.SelectedValue.ToString().Equals("SELECT"))
                        {
                            // 불량항목이 없습니다.
                            _IsLoopEnd = true;

                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU1588"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                }
                            });

                            return;
                        }

                        // 중복 체크
                        DataRow[] drList = _dtCell.Select("SUBLOTID = '" + drResult.Tables["OUT_SUBLOT"].Rows[0]["SUBLOTID"] + "'");

                        if (drList.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            _IsLoopEnd = true;

                            txtCellID.Text = string.Empty;

                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                }
                            });

                            return;
                        }

                        // 같은 제품인지 체크
                        DataRow[] drDup = _dtCell.Select("RESNCODE = '" + cboResnCodeCre.SelectedValue.ToString() + "'");

                        if (drDup.Length > 0 && drDup[0]["PRODID"].ToString() != drResult.Tables["OUT_SUBLOT"].Rows[0]["PRODID"].ToString())
                        {
                            // 동일 제품이 아닙니다.
                            _IsLoopEnd = true;

                            txtCellID.Text = string.Empty;

                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU1502"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                }
                            });

                            return;
                        }

                        //시장유형 체크
                        if (_dtCell.Rows.Count > 0)
                        {
                            DataRow[] drMktType = _dtCell.Select("MKT_TYPE_CODE = '" + drResult.Tables["OUT_SUBLOT"].Rows[0]["MKT_TYPE_CODE"] + "'");

                            if (drMktType.Length <= 0)
                            {
                                _IsLoopEnd = true;

                                txtCellID.Text = string.Empty;

                                //동일한 시장유형이 아닙니다.
                                ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU4271"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtCellID.Focus();
                                    }
                                });

                                return;
                            }
                        }

                        // 불량코드 추가
                        drResult.Tables["OUT_SUBLOT"].Rows[0]["CHK"] = 1;
                        drResult.Tables["OUT_SUBLOT"].Rows[0]["RESNCODE"] = cboResnCodeCre.SelectedValue.ToString();
                        drResult.Tables["OUT_SUBLOT"].Rows[0]["RESNNAME"] = cboResnCodeCre.Text.ToString();
                        _dtCell.Merge(drResult.Tables["OUT_SUBLOT"]);

                        if (!_IsKeyCV)
                        {
                            Util.GridSetData(dgCreate, _dtCell, null, true);

                            // 스캔수량
                            txtScanQtyCre.Value = _dtCell.Rows.Count;
                        }
                    }

                    txtCellID.Text = string.Empty;
                    txtCellID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtCellID.Text = string.Empty;
                        txtCellID.Focus();
                    }
                });
            }
            finally
            {
            }
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgCreate;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCell)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCell)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCell = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCell = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete())
                return;

            _dtCell = DataTableConverter.Convert(dgCreate.ItemsSource);
            _dtCell.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            _dtCell.AcceptChanges();

            Util.GridSetData(dgCreate, _dtCell, null, true);

            txtScanQtyCre.Value = _dtCell.Rows.Count;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            //  저장 하시겠습니까?
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint(true))
                return;

            DataRow[] dr = DataTableConverter.Convert(dgCreate.ItemsSource).Select("Scan수량 > 0");

            PopupPrint(dr);
        }

        #endregion

        #region [외관 불량 Lot 정보]
        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchLotProcess(false, tb);
            }
        }
        private void txtCellIDLotScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter) && !string.IsNullOrWhiteSpace(txtCellIDLotScan.Text))
            {
                // Cell 체크
                SetCellGridCheck(txtCellIDLotScan.Text);
            }
        }

        private void dgDefectLotChoice_Checked(object sender, RoutedEventArgs e)
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
                        dgDefectLot.SelectedIndex = idx;

                        // Cell 정보 조회
                        SearchCellProcess(Util.NVC(drv.Row["LOTID"].ToString()));

                        // 불량명 콤보
                        SetResnCodeCombo(cboResnCode);
                        cboResnCode.SelectedValue = Util.NVC(drv.Row["RESNCODE"].ToString());

                        // 작업조 콤보
                        SetShiftCombo(cboShift);

                        // 입력 수량
                        txtInputQty.Value = Util.NVC_Int(drv.Row["INPUTQTY"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tbCheckHeaderAllUpdate_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefectCell;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCellUpdate)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCellUpdate)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCellUpdate = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCellUpdate = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void rdoUpdate_Checked(object sender, RoutedEventArgs e)
        {
            SetUpdateCancelEnable();
        }

        private void rdoCancel_Checked(object sender, RoutedEventArgs e)
        {
            SetUpdateCancelEnable();
        }

        /// <summary>
        /// 불량수정
        /// </summary>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationModify())
                return;

            // 수정 하시겠습니까?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyProcess();
                }
            });
        }

        /// <summary>
        /// 불량취소
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel())
                return;

            // 취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelProcess();
                }
            });
        }

        private void btnPrintLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint(false))
                return;

            DataRow[] dr = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");

            PopupPrint(dr);
        }

        private void btnSearchLot_Click(object sender, RoutedEventArgs e)
        {
            SearchLotProcess(true);
        }

        #endregion

        #region [외관 불량 Cell 불량 취소]
        private void txtDefectCell_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        txtDefectCell.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtDefectCell.Text))
                            txtDefectCell_KeyDown(txtDefectCell, null);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtDefectCell_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    string sSubLotID = txtDefectCell.Text.Trim();

                    DataTable dtSubLot = DataTableConverter.Convert(dgDefectCellCancel.ItemsSource);

                    if (dtSubLot.Rows.Count > 0)
                    {
                        DataRow[] drDup = dtSubLot.Select("SUBLOTID = '" + sSubLotID + "'");

                        if (drDup.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtDefectCell.Focus();
                                    txtDefectCell.Text = string.Empty;
                                }
                            });

                            txtDefectCell.Text = string.Empty;
                            return;
                        }
                    }

                    // Sublot 정보 조회
                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("LANGID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("OPERATION");

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SUBLOTID"] = sSubLotID;
                    dr["OPERATION"] = "G";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_AUTO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            if (dtSubLot.Rows.Count > 0)
                            {
                                // 중복 체크
                                DataRow[] drList = dtSubLot.Select("SUBLOTID = '" + dtResult.Rows[0]["SUBLOTID"] + "'");

                                if (drList.Length > 0)
                                {
                                    // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtDefectCell.Focus();
                                            txtDefectCell.Text = string.Empty;
                                        }
                                    });

                                    txtDefectCell.Text = string.Empty;
                                    return;
                                }

                                // Row 추가
                                dtResult.Merge(dtSubLot);
                            }

                            Util.GridSetData(dgDefectCellCancel, dtResult, null, true);
                            txtScanQty.Value = dtResult.Rows.Count;
                        }
                    }

                    txtDefectCell.Text = string.Empty;
                    txtDefectCell.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtDefectCell.Text = string.Empty;
                        txtDefectCell.Focus();
                    }
                });
            }
            finally
            {
            }
        }

        private void tbCheckHeaderAllDefect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefectCellCancel;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCellDefect)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCellDefect)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCellDefect = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCellDefect = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        /// <summary>
        /// 엑셀 파일 오픈
        /// </summary>
        private void btnOpenExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();

            try
            {
                txtScanQty.Value = 0;

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
                        // DataTable dtResult = exl.GetSheetData(str[0]);

                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable inSubLot = new DataTable();
                        inSubLot.Columns.Add("CHK", typeof(Boolean));
                        inSubLot.Columns.Add("SUBLOTID", typeof(string));

                        DataRow newRow = null;

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text))
                                break;

                            newRow = inSubLot.NewRow();
                            newRow["CHK"] = 0;
                            newRow["SUBLOTID"] = sheet.GetCell(rowInx, 0).Text;
                            inSubLot.Rows.Add(newRow);
                        }

                        Util.GridSetData(dgDefectCellCancel, inSubLot, null, true);
                        txtScanQty.Value = inSubLot.Rows.Count;

                    }
                }
            }
            catch (Exception ex)
            {
                if (exl != null)
                {
                    //이전 연결 해제
                    exl.Conn_Close();
                }
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDeleteCell_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellDelete())
                return;

            DataTable dt = DataTableConverter.Convert(dgDefectCellCancel.ItemsSource);
            dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();

            Util.GridSetData(dgDefectCellCancel, dt, null, true);

            txtScanQty.Value = dt.Rows.Count;
        }

        /// <summary>
        /// 불량취소
        /// </summary>
        private void btnCancelCell_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellCancel())
                return;

            // %1 (을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CellCancelProcess();
                }
            }, ObjectDic.Instance.GetObjectName("불량취소"));
        }

        #endregion

        #region [외관 불량 Lot 생성 이력]
        private void txtHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchHistoryProcess(false, tb);
            }
        }

        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            // Grid 초기화 후 조회
            Util.gridClear(dgHistory);
            Util.gridClear(dgHistoryCell);

            SearchHistoryProcess(true);
        }

        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("CELLQTY"))
                    {
                        if (Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CELLQTY").GetString()).Equals(-1))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
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
                }
            }));
        }

        private void dgDefectHistoryChoice_Checked(object sender, RoutedEventArgs e)
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
                        dgHistory.SelectedIndex = idx;

                        // Cell 정보 조회
                        DataTable dtMin = DataTableConverter.Convert(dgHistory.ItemsSource).Select("LOTID = '" + drv.Row["LOTID"].ToString() + "' And ACTID = '" + drv.Row["ACTID"].ToString() + "'").CopyToDataTable();

                        var MinMax = from dt in dtMin.AsEnumerable()
                                     group dt by dt.Field<string>("LOTID") into grp
                                     select new
                                     {
                                         Min = grp.Min(T => T.Field<string>("ACTDTTM_MIN")),
                                         Max = grp.Max(T => T.Field<string>("ACTDTTM"))
                                     };

                        foreach (var data in MinMax)
                        {
                            SearchCellHistoryProcess(data.Min, data.Max, Util.NVC(drv.Row["LOTID"].ToString())
                                                   , Util.NVC(drv.Row["ACTID"].ToString())
                                                   , Util.NVC(drv.Row["LOTID_RT"].ToString()));
                            break;
                        }
                    }
                }
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

        /// <summary>
        /// 라인 콤보
        /// </summary>
        private void SetcboEquipmentSegmentCreCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_EQUIPMENTSEGMENT_AUTO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        /// <summary>
        /// 공정 콤보
        /// </summary>
        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";
            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE", "COM_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "FORM_INSP_PROCID_AUTO", null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);

            if (cbo.Items.Count > 0)
            {
                // 선택 공정이 없을 경우 F4000 : Degas
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }

        }

        /// <summary>
        /// 설비 콤보
        /// </summary>
        private void SetEquipmentCombo(C1ComboBox cbo, C1ComboBox cboParent)
        {
            if (cboParent.SelectedValue == null) return;

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_FORM_AUTO";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, Util.NVC(cboParent.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        /// <summary>
        /// 불량코드 콤보
        /// </summary>
        private void SetResnCodeCreateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT_INFO_AUTO";
            string[] arrColumn = { "LANGID", "ACTID", "AREAID", "PROCID", "RESNGRID" };
            string[] arrCondition = { LoginInfo.LANGID, "DEFECT_LOT", LoginInfo.CFG_AREA_ID, Util.NVC(cboProcessCre.SelectedValue), "DEFECT_SURFACE_AUTO" };
            string selectedValueText = "RESNCODE";
            string displayMemberText = "FORM_DFCT_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// 작업조 콤보
        /// </summary>
        private void SetShiftCreateCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_SHFT_AUTO";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, Util.NVC(cboEquipmentSegmentCre.SelectedValue), Util.NVC(cboProcessCre.SelectedValue), "Y" };
            string selectedValueText = "SHFT_ID";
            string displayMemberText = "SHFT_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }


        /// <summary>
        /// 불량LOT 생성
        /// </summary>
        private void SaveProcess()
        {
            try
            {
                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("RWK_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(DateTime));

                DataTable inRESN = inDataSet.Tables.Add("INRESN");
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("PRODID", typeof(string));
                inRESN.Columns.Add("SCANQTY", typeof(decimal));
                inRESN.Columns.Add("INPUTQTY", typeof(decimal));
                inRESN.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("RESNCODE", typeof(string));
                inSubLot.Columns.Add("PRODID", typeof(string));
                inSubLot.Columns.Add("SUBLOTID", typeof(string));
                inSubLot.Columns.Add("MKT_TYPE_CODE", typeof(string));

                ////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow[] drSubLot = DataTableConverter.Convert(dgCreate.ItemsSource).Select("CHK = 1");

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = cboEquipmentSegmentCre.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipmentCre.SelectedValue.ToString();
                newRow["PROCID"] = cboProcessCre.SelectedValue.ToString();
                newRow["WRK_USER_NAME"] = txtWorkUserCre.Text;
                newRow["RWK_FLAG"] = "N";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = cboShiftCre.SelectedValue.ToString();
                newRow["CALDATE"] = dtpDateCre.SelectedDateTime.ToString("yyyy-MM-dd");
                inTable.Rows.Add(newRow);

                ////////////////////////////////////////////////////////////////////////////////////////////////
                // 불량코드, 제품별 Grouping
                var summarydata = from row in drSubLot.AsEnumerable()
                                  group row by new
                                  {
                                      ResnCode = row.Field<string>("RESNCODE"),
                                      ProdID = row.Field<string>("PRODID"),
                                      MktTypeCode = row.Field<string>("MKT_TYPE_CODE"),
                                  } into grp
                                  select new
                                  {
                                      ResnCode = grp.Key.ResnCode,
                                      ProdID = grp.Key.ProdID,
                                      MktTypeCode = grp.Key.MktTypeCode,
                                      CellQty = grp.Count()
                                  };

                foreach (var data in summarydata)
                {
                    newRow = inRESN.NewRow();
                    newRow["RESNCODE"] = data.ResnCode;
                    newRow["PRODID"] = data.ProdID;
                    newRow["SCANQTY"] = data.CellQty;
                    newRow["INPUTQTY"] = 0;
                    newRow["MKT_TYPE_CODE"] = data.MktTypeCode;
                    inRESN.Rows.Add(newRow);
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////
                foreach (DataRow dr in drSubLot)
                {
                    newRow = inSubLot.NewRow();
                    newRow["RESNCODE"] = Util.NVC(dr["RESNCODE"]);
                    newRow["PRODID"] = Util.NVC(dr["PRODID"]);
                    newRow["SUBLOTID"] = Util.NVC(dr["SUBLOTID"]);
                    newRow["MKT_TYPE_CODE"] = Util.NVC(dr["MKT_TYPE_CODE"]);
                    inSubLot.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DEFECT_LOT_AUTO", "INDATA,INRESN,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        InitializeUserControls();
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

        /// <summary>
        /// 외관 불량 Lot 조회
        /// </summary>
        private void SearchLotProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                string CellScan = string.Empty;

                //if ((dtpDateToLot.SelectedDateTime - dtpDateFromLot.SelectedDateTime).TotalDays > 31)
                //{
                //    // 기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "31");
                //    return;
                //}

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtDefectLot"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량LOTID"));
                            return;
                        }
                        else if (tb.Name.Equals("txtCellIDLot"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("Cell ID"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량명"));
                            return;
                        }
                    }

                    if (tb.Name.Equals("txtCellIDLot"))
                    {
                        CellScan = tb.Text;
                    }
                }

                _HeaderTypeCellUpdate = CheckBoxHeaderType.Zero;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("YYYYMM", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DEFECT_LOT", typeof(string));
                inTable.Columns.Add("RESNNAME", typeof(string));
                inTable.Columns.Add("RWK_FLAG", typeof(string));
                inTable.Columns.Add("CELLID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["YYYYMM"] = dtpDateFromLot.SelectedDateTime.ToString("yyyy-MM");
                newRow["RWK_FLAG"] = "N";

                if (!string.IsNullOrWhiteSpace(txtDefectLot.Text))
                {
                    newRow["DEFECT_LOT"] = txtDefectLot.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtCellIDLot.Text))
                {
                    newRow["CELLID"] = txtCellIDLot.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtResnNameLot.Text))
                {
                    newRow["RESNNAME"] = txtResnNameLot.Text;
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegmentLot, MessageDic.Instance.GetMessage("SFU1223"));
                    if (newRow["EQSGID"].Equals("")) return;

                    newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessLot.SelectedValue.ToString()) ? null : cboProcessLot.SelectedValue.ToString();
                }
                inTable.Rows.Add(newRow);

                // Cell 정보 Clear
                //Util.gridClear(dgDefectCell);
                InitializeUserControls();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SURFACE_DEFECT_LOT_LIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgDefectLot, bizResult, null, false);

                        // Cell Count(중복제거)
                        DataTable dt = bizResult.DefaultView.ToTable(true, new string[] { "LOTID", "WIPQTY", "SCANQTY", "INPUTQTY" });
                        decimal ScanQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("SCANQTY"));
                        decimal InputQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("INPUTQTY"));
                        decimal WipQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));

                        DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["SCANQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ScanQty.ToString("###,###") } });
                        DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["INPUTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = InputQty.ToString("###,###") } });
                        DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = WipQty.ToString("###,###") } });

                        if (!string.IsNullOrWhiteSpace(CellScan))
                        {
                            if (bizResult.Rows.Count > 0)
                            {
                                DataTableConverter.SetValue(dgDefectLot.Rows[0].DataItem, "CHK", true);

                                // row 색 바꾸기
                                dgDefectLot.SelectedIndex = 0;

                                // 불량명 콤보
                                SetResnCodeCombo(cboResnCode);
                                cboResnCode.SelectedValue = Util.NVC(bizResult.Rows[0]["RESNCODE"].ToString());

                                // 작업조 콤보
                                SetShiftCombo(cboShift);

                                // 입력 수량
                                txtInputQty.Value = Util.NVC_Int(bizResult.Rows[0]["INPUTQTY"].ToString());

                                // Cell 정보 조회
                                SearchCellProcess(Util.NVC(bizResult.Rows[0]["LOTID"].ToString()), CellScan);
                            }
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
        /// 외관 불량 Cell 조회
        /// </summary>
        private void SearchCellProcess(string DefectLot, string CellID = null)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DefectLot;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_DEFECT_LOT_CELL_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgDefectCell, bizResult, null, true);

                        if (!string.IsNullOrWhiteSpace(CellID))
                        {
                            SetCellGridCheck(CellID);
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
        /// 불량 콤보
        /// </summary>
        private void SetResnCodeCombo(C1ComboBox cbo)
        {
            DataRow dr = _Util.GetDataGridFirstRowBycheck(dgDefectLot, "CHK");

            if (dr == null) return;

            const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT_INFO_AUTO";
            string[] arrColumn = { "LANGID", "ACTID", "AREAID", "PROCID", "RESNGRID" };
            string[] arrCondition = { LoginInfo.LANGID, "DEFECT_LOT", dr["AREAID"].ToString(), dr["PROCID"].ToString(), "DEFECT_SURFACE_AUTO" };
            string selectedValueText = "RESNCODE";
            string displayMemberText = "FORM_DFCT_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);

            //// 기존 불량코드는 제외 한다.
            //DataTable dt = DataTableConverter.Convert(cbo.ItemsSource);
            //dt.Select("RESNCODE = '" + dr["RESNCODE"].ToString() + "'").ToList<DataRow>().ForEach(row => row.Delete());
            //cbo.ItemsSource = null;
            //cbo.ItemsSource = dt.Copy().AsDataView();
            //cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 작업조 콤보
        /// </summary>
        private void SetShiftCombo(C1ComboBox cbo)
        {
            DataRow dr = _Util.GetDataGridFirstRowBycheck(dgDefectLot, "CHK");

            if (dr == null) return;

            const string bizRuleName = "DA_BAS_SEL_TB_MMD_SHFT_AUTO";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, dr["AREAID"].ToString(), dr["EQSGID"].ToString(), dr["PROCID"].ToString(), "Y" };
            string selectedValueText = "SHFT_ID";
            string displayMemberText = "SHFT_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// 불량 수정
        /// </summary>
        private void ModifyProcess()
        {
            try
            {
                DataRow dr = _Util.GetDataGridFirstRowBycheck(dgDefectLot, "CHK");

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(DateTime));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("NEW_RESNCODE", typeof(string));
                inLot.Columns.Add("INPUTQTY", typeof(double));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = dr["AREAID"];
                newRow["EQSGID"] = dr["EQSGID"];
                newRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                newRow["PROCID"] = dr["PROCID"];
                newRow["WRK_USER_NAME"] = txtWorkUser.Text;
                newRow["WIPNOTE"] = txtResnNote.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CALDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                newRow["SHIFT"] = cboShift.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                /////////////////////////////////////////////////////////////////
                newRow = inLot.NewRow();
                newRow["LOTID"] = dr["LOTID"];
                newRow["RESNCODE"] = dr["RESNCODE"];
                newRow["NEW_RESNCODE"] = cboResnCode.SelectedValue.ToString();
                newRow["INPUTQTY"] = txtInputQty.Value;
                inLot.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_DEFECT_LOT_AUTO", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        // 외관불량LOT정보 재조회
                        InitializeUserControls();

                        SearchLotProcess(true);
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

        /// <summary>
        /// 불량 취소
        /// </summary>
        private void CancelProcess()
        {
            try
            {
                DataRow dr = _Util.GetDataGridFirstRowBycheck(dgDefectLot, "CHK");

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(DateTime));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = dr["AREAID"];
                newRow["EQSGID"] = dr["EQSGID"];
                newRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                newRow["PROCID"] = dr["PROCID"];
                newRow["WRK_USER_NAME"] = txtWorkUser.Text;
                newRow["WIPNOTE"] = txtResnNote.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CALDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                newRow["SHIFT"] = cboShift.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                /////////////////////////////////////////////////////////////////
                newRow = inLot.NewRow();
                newRow["LOTID"] = dr["LOTID"];
                newRow["RESNCODE"] = dr["RESNCODE"];
                inLot.Rows.Add(newRow);

                /////////////////////////////////////////////////////////////////
                DataRow[] drSubLot = DataTableConverter.Convert(dgDefectCell.ItemsSource).Select("CHK = 1");

                foreach (DataRow drIns in drSubLot)
                {
                    newRow = inSubLot.NewRow();
                    newRow["SUBLOTID"] = drIns["SUBLOTID"];
                    inSubLot.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_CAN_DEFECT_LOT_AUTO", "INDATA,INLOT,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        // 외관불량LOT정보 재조회
                        InitializeUserControls();

                        SearchLotProcess(true);
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

        /// <summary>
        /// Cell 불량 취소
        /// </summary>
        private void CellCancelProcess()
        {
            try
            {
                // DATA SET 
                DataTable dtCancel = DataTableConverter.Convert(dgDefectCellCancel.ItemsSource);
                if (dtCancel == null || dtCancel.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // XML string 500건당 1개 생성
                int rowCount = 0;
                string sXML = string.Empty;

                for (int row = 0; row < dtCancel.Rows.Count; row++)
                {
                    if (row == 0 || row % 500 == 0)
                    {
                        sXML = "<root>";
                    }

                    sXML += "<DT><L>" + dtCancel.Rows[row]["SUBLOTID"] + "</L></DT>";

                    if ((row + 1) % 500 == 0 || row + 1 == dtCancel.Rows.Count)
                    {
                        sXML += "</root>";

                        newRow = inSubLot.NewRow();
                        newRow["SUBLOTID"] = sXML;
                        inSubLot.Rows.Add(newRow);
                    }

                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_DEFECT_SUBLOT_CANCEL_AUTO", "INDATA,INSUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        InitializeUserControls();

                        bizResult.Tables["OUTDATA"].Columns.Add("CHK", typeof(Boolean));
                        Util.GridSetData(dgDefectCellCancel, bizResult.Tables["OUTDATA"], null, true);
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

        /// <summary>
        /// 외관 불량 Lot 생성 이력
        /// </summary>
        private void SearchHistoryProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtDefectHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량LOTID"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량명"));
                            return;
                        }
                    }
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DEFECT_LOT", typeof(string));
                inTable.Columns.Add("RESNNAME", typeof(string));
                inTable.Columns.Add("RWK_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToHis);
                newRow["RWK_FLAG"] = "N";

                if (!string.IsNullOrWhiteSpace(txtDefectHis.Text))
                {
                    newRow["DEFECT_LOT"] = txtDefectHis.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtResnNameHis.Text))
                {
                    newRow["RESNNAME"] = txtResnNameHis.Text;
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHis, MessageDic.Instance.GetMessage("SFU1223"));
                    if (newRow["EQSGID"].Equals("")) return;

                    newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessHis.SelectedValue.ToString()) ? null : cboProcessHis.SelectedValue.ToString();
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SURFACE_DEFECT_LOT_HIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, null, true);

                        // Cell Count(중복제거)
                        //DataTable dt = bizResult.DefaultView.ToTable(true, new string[] { "LOTID", "WIPQTY", "SCANQTY", "INPUTQTY" });
                        //decimal ScanQty = dt.AsEnumerable().Sum(r => r.Field<int>("SCANQTY"));
                        //decimal InputQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("INPUTQTY"));
                        //decimal WipQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));

                        //DataGridAggregate.SetAggregateFunctions(dgHistory.Columns["SCANQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ScanQty.ToString("###,###") } });
                        //DataGridAggregate.SetAggregateFunctions(dgHistory.Columns["INPUTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = InputQty.ToString("###,###") } });
                        //DataGridAggregate.SetAggregateFunctions(dgHistory.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = WipQty.ToString("###,###") } });
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
        /// 외관 불량 이력 Cell 조회
        /// </summary>
        private void SearchCellHistoryProcess(string FromDT, string ToDT, string DefectLot, string ActID, string AssyLot)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_ACTDTTM", typeof(string));
                inTable.Columns.Add("TO_ACTDTTM", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("RESNGRID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_ACTDTTM"] = FromDT;
                newRow["TO_ACTDTTM"] = ToDT;
                newRow["LOTID"] = DefectLot;
                newRow["RESNGRID"] = "DEFECT_SURFACE_AUTO";
                newRow["ACTID"] = ActID;
                newRow["LOTID_RT"] = AssyLot;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT_CELL_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistoryCell, bizResult, null, true);
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
        #endregion

        #region [Validation]
        private bool ValidationScanValue()
        {
            if (cboEquipmentSegmentCre.SelectedValue == null || cboEquipmentSegmentCre.SelectedValue.ToString().Equals("SELECT"))
            {
                // 라인을 선택해주세요
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboProcessCre.SelectedValue == null || cboProcessCre.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            return true;
        }

        private bool ValidationDelete()
        {
            int rowChkCount = DataTableConverter.Convert(dgCreate.ItemsSource).AsEnumerable().Count(r => r.Field<bool>("CHK") == true);

            if (rowChkCount == 0)
            {
                // 삭제할 항목이 없습니다.
                Util.MessageValidation("SFU1597");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            int rowChkCount = DataTableConverter.Convert(dgCreate.ItemsSource).AsEnumerable().Count(r => r.Field<bool>("CHK") == true);

            if (rowChkCount == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (cboEquipmentSegmentCre.SelectedValue == null || cboEquipmentSegmentCre.SelectedValue.ToString().Equals("SELECT"))
            {
                // 라인을 선택해주세요
                Util.MessageValidation("SFU4050");
                return false;
            }

            if (cboProcessCre.SelectedValue == null || cboProcessCre.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboEquipmentCre.SelectedValue == null || cboEquipmentCre.SelectedValue.ToString().Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWorkUserCre.Text))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if (cboShiftCre.SelectedValue == null || cboShiftCre.SelectedValue.ToString().Equals("SELECT"))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }


            return true;
        }

        private bool ValidationPrint(bool create)
        {
            if (create)
            {
                DataRow[] dr = DataTableConverter.Convert(dgCreate.ItemsSource).Select("SCANQTY > 0 or INPUTQTY > 0");

                if (dr.Length == 0)
                {
                    // 셀 인쇄내용 정보가 없습니다.
                    Util.MessageValidation("SFU4522");
                    return false;
                }
            }
            else
            {
                int rowChkCount = DataTableConverter.Convert(dgDefectLot.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

                if (rowChkCount == 0)
                {
                    // 선택된 대상이 없습니다.
                    Util.MessageValidation("SFU1636");
                    return false;
                }

            }

            return true;
        }

        private bool ValidationModify()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectLot, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (cboResnCode.SelectedValue.ToString().Equals("SELECT"))
            {
                // % 1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("변경불량명"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWorkUser.Text))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if (cboShift.SelectedValue == null || cboShift.SelectedValue.ToString().Equals("SELECT"))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            int rowChkCount = DataTableConverter.Convert(dgDefectCell.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (rowChkCount == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWorkUser.Text))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if (cboShift.SelectedValue == null || cboShift.SelectedValue.ToString().Equals("SELECT"))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }

            return true;
        }

        private bool ValidationCellDelete()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectCellCancel, "CHK", true);

            if (rowIndex < 0 || dgDefectCellCancel.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationCellCancel()
        {
            if (dgDefectCellCancel.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }


            // 중복 CELL 체크
            DataTable dt = DataTableConverter.Convert(dgDefectCellCancel.ItemsSource);
            var dupSublot = dt.AsEnumerable().GroupBy(x => x["SUBLOTID"]).Where(x => x.Count() > 1);

            if (dupSublot.Count() > 0)
            {
                foreach (var data in dupSublot)
                {
                    // 중복 데이터가 존재 합니다. % 1
                    Util.MessageValidation("SFU2051", data.Key);
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region [팝업]

        /// <summary>
        /// Sheet발행 팝업
        /// </summary>
        private void PopupPrint(DataRow[] drPrint)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupPrint.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = "";
            parameters[1] = "";
            parameters[2] = "";   // ButtonCertSelect.Tag;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupPrint, parameters);

            popupPrint.Closed += new EventHandler(popupPrint_Closed);
            grdMain.Children.Add(popupPrint);
            popupPrint.BringToFront();
        }

        private void popupPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //DataTableConverter.SetValue(dgCreate.Rows[_ResnGridRow].DataItem, "ASSYLOT", popup.a);
            }

            grdMain.Children.Remove(popup);
        }

        ///// <summary>
        ///// 불량 수정 
        ///// </summary>
        //private void PopupModify()
        //{
        //    CMM_POLYMER_FORM_CAR_DEFECT_EXTERNAL popupModify = new CMM_POLYMER_FORM_CAR_DEFECT_EXTERNAL();
        //    popupModify.FrameOperation = this.FrameOperation;

        //    object[] parameters = new object[2];
        //    parameters[0] = _Util.GetDataGridFirstRowBycheck(dgDefectLot, "CHK");
        //    parameters[1] = DataTableConverter.Convert(dgDefectCell.ItemsSource).Select();

        //    C1WindowExtension.SetParameters(popupModify, parameters);

        //    popupModify.Closed += new EventHandler(popupModify_Closed);
        //    grdMain.Children.Add(popupModify);
        //    popupModify.BringToFront();
        //}

        //private void popupModify_Closed(object sender, EventArgs e)
        //{
        //    CMM_POLYMER_FORM_CAR_DEFECT_EXTERNAL popup = sender as CMM_POLYMER_FORM_CAR_DEFECT_EXTERNAL;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //        // 다시 조회
        //        SearchLotProcess(true);
        //    }

        //    grdMain.Children.Remove(popup);
        //}
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

        /// <summary>
        /// 불량 수정, 불량 취소
        /// </summary>
        private void SetUpdateCancelEnable()
        {
            if (dgDefectCell == null) return;

            if ((bool)rdoUpdate.IsChecked)
            {
                dgDefectCell.Columns["CHK"].Visibility = Visibility.Collapsed;
                cboResnCode.IsEnabled = true;
                txtInputQty.IsEnabled = true;
                btnModify.IsEnabled = true;
                btnCancel.IsEnabled = false;
            }
            else
            {
                dgDefectCell.Columns["CHK"].Visibility = Visibility.Visible;
                cboResnCode.IsEnabled = false;
                txtInputQty.IsEnabled = false;
                btnModify.IsEnabled = false;
                btnCancel.IsEnabled = true;
            }

        }

        private void SetCellGridCheck(string CellID)
        {
            DataTable dt = DataTableConverter.Convert(dgDefectCell.ItemsSource);

            if (dt == null || dt.Rows.Count == 0) return;

            dt.Select("SUBLOTID = '" + CellID + "'").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
            Util.GridSetData(dgDefectCell, dt, null, true);

            txtCellIDLotScan.Text = string.Empty;
            txtCellIDLotScan.Focus();

            _HeaderTypeCellUpdate = CheckBoxHeaderType.One;
        }

        #endregion

        #endregion

    }
}
