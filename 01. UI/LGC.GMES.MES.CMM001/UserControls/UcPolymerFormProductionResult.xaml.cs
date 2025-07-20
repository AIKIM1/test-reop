using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcFormProductionPalette.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPolymerFormProductionResult
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public UcPolymerFormCart UcPolymerFormCart { get; set; }

        public C1DataGrid DgProductionInbox { get; set; }
        public C1DataGrid DgProductionDefect { get; set; }
        public DataRow DataRowProductLot { get; set; }

        public string ProdCartId { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string InspectorCode { get; set; }
        public string InspectorId { get; set; }
        public string ProdLotId { get; set; }
        public string WipSeq { get; set; }
        public string EquipmentSegmentCode { get; set; }

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }

        public bool AommGrdVisibility { get; set; }

        public string ShiptoID { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private string _inboxTypeCode;
        private int _inboxLoadQty;
        private CheckBoxHeaderType _inBoxHeaderType;
        private bool _finalExternal;
        private bool _defectTabButtonClick;

        /// 2018-03-06 불량탭 추가
        private int _defectGradeCount;
        private bool _IsDefectSave;
        private DataTable _defectList;

        bool _AommGrdChkFlag = false;

        string _sPGM_ID = "UcPolymerFormProductionResult";

        public UcPolymerFormProductionResult()
        {
            InitializeComponent();
            SetControl();
            SetButtons();
            _inBoxHeaderType = CheckBoxHeaderType.Zero;
            SetAommGradeCombo();
            //SetCombo();
            //SetGrideCombo();
        }

        #endregion

        #region Initialize

        private void SetControl()
        {
            DgProductionInbox = dgProductionInbox;
            DgProductionDefect = dgProductionDefect;
        }
        private void SetButtons()
        {
        }

        public void SetAommGradeCombo()
        {
            try
            {
                cboAommType.ItemsSource = null;
                cboAommType.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "AOMM_GRADE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                cboAommType.DisplayMemberPath = "CBO_NAME";
                cboAommType.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);


                cboAommType.ItemsSource = dtResult.Copy().AsDataView();
                cboAommType.SelectedIndex = 0;
                (dgProductionInbox.Columns["AOMM_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAommGrdVisibility(string sProdID)
        {
            try
            {
                if (string.IsNullOrEmpty(sProdID))
                {
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "GRADE_CHK_PROD";
                dr["CMCODE"] = sProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _AommGrdChkFlag = true;
                    tbAommType.IsEnabled = true;
                    cboAommType.IsEnabled = true;
                }
                else
                {
                    _AommGrdChkFlag = false;
                    tbAommType.IsEnabled = false;
                    cboAommType.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetGradeCombo()
        {
            cboCapaType.ItemsSource = null;
            cboCapaType.Text = string.Empty;

            // 제품별일 경우 생산 Lot 선택시 변경 필요
            CommonCombo _combo = new CMM001.Class.CommonCombo();
            string[] sFilterCapa = { LoginInfo.CFG_AREA_ID, EquipmentSegmentCode, "G"};
            _combo.SetCombo(cboCapaType, CommonCombo.ComboStatus.NONE, sCase: "FORM_GRADE_TYPE_CODE_LINE", sFilter: sFilterCapa);

            SeInboxMaxQty();
            SetGridCapaGrade();

            // 뷸량 등록시 대상등급선택 콤보 2018-04-05
            cboGradeSelect.SelectionChanged -= cboGradeSelect_SelectionChanged;
            cboGradeSelect.ApplyTemplate();
            SetGradeCombo(cboGradeSelect);
            cboGradeSelect.SelectionChanged += cboGradeSelect_SelectionChanged;
        }

        //private void SetGrideCombo()
        //{
        //    SetGridCapaGrade();
        //}

        public void SetControlVisibility()
        {
            // 작업유형이 일반 재작업인경우 처리
            //if (ChkCartStatus())
            //{
            //    tbCapaType.Visibility = Visibility.Collapsed;
            //    cboCapaType.Visibility = Visibility.Collapsed;
            //    numAddCount.Visibility = Visibility.Collapsed;
            //    //tiDefect.Visibility = Visibility.Collapsed;
            //}

            //if (DataRowProductLot == null) return;

            //if (ChkCartStatus())
            //{
            //    // 양품화 공정에선 불량 탭 안보이게
            //    tiDefect.Visibility = Visibility.Collapsed;
            //}

            if (DataRowProductLot == null || Util.NVC(DataRowProductLot["FORM_WRK_TYPE_CODE"]).Equals("FORM_WORK_RW"))
            {
                tbCapaType.IsEnabled = false;
                cboCapaType.IsEnabled = false;
                numAddCount.IsEnabled = false;
            }
            else
            {
                tbCapaType.IsEnabled = true;
                cboCapaType.IsEnabled = true;
                numAddCount.IsEnabled = true;
            }

            // 포장모드인 경우
            if (DataRowProductLot != null && Util.NVC(DataRowProductLot["CTNR_TYPE_CODE"]).Equals("B"))
            {
                btnCellInsert.Visibility = Visibility.Visible;
                dgProductionInbox.Columns["CELL_IN_QTY"].Visibility = Visibility.Visible;
            }
            else
            {
                btnCellInsert.Visibility = Visibility.Collapsed;
                dgProductionInbox.Columns["CELL_IN_QTY"].Visibility = Visibility.Collapsed;
            }

            if (AommGrdVisibility == true || ProcessCode == Process.PolymerTaping)
            {
                tbAommType.Visibility = Visibility.Visible;
                cboAommType.Visibility = Visibility.Visible;
                dgProductionInbox.Columns["AOMM_GRD_CODE"].Visibility = Visibility.Visible;

                if (DataRowProductLot == null)
                {
                    tbAommType.IsEnabled = false;
                    cboAommType.IsEnabled = false;
                }
                else
                {
                    SetAommGrdVisibility(Util.NVC(DataRowProductLot["PRODID"]));
                }
            }

            if (ProcessCode.Equals(Process.DEGASING))
            {
                btnInboxComplete.Visibility = Visibility.Visible;
            }
            else
            {
                btnInboxComplete.Visibility = Visibility.Collapsed;
            }

        }

        public void InitializeControls()
        {
            SetDataGridCheckHeaderInitialize(dgProductionInbox);
            SetDataGridCheckHeaderInitialize(dgProductionDefect);

            Util.gridClear(dgProductionInbox);
            Util.gridClear(dgProductionDefect);
            Util.gridClear(dgProductionGrade);

            _defectTabButtonClick = false;
        }

        public void ChangeEquipment(string equipmentCode, string equipmentSegment)
        {
            try
            {
                EquipmentCode = equipmentCode;
                EquipmentSegmentCode = equipmentSegment;
                ProdLotId = string.Empty;

                InitializeControls();

                SetGradeCombo();             // 라인별 용량등급
                SetEqptInboxType();          // 설비별 Inbox 유형
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region ### [완성 Inbox] ###

        #region 대차변경
        private void btnCartChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartChange())
                return;

            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("대차변경");
            //대차 변경 하시겠습니까?
            Util.MessageConfirm("SFU4329", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangeCart();
                }
            }, parameters);
        }
        #endregion

        #region Cell등록
        private void btnCellInsert_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellInput())
                return;

            CellInputPopup();
        }
        #endregion

        #region Inbox 생성
        private void btnInboxCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxInput()) return;

            // Inbox 생성
            //if (_finalExternal)
            //{
            //    // 양품화/C생산 공정인 경우 Inbox 생성 팝업 호출
            //    CraetInboxPopup();
            //}
            //else
            //{
            //    InputInbox();
            //}

            if ( Util.NVC(DataRowProductLot["FORM_WRK_TYPE_CODE"]).Equals("FORM_WORK_RW") ||
                (Util.NVC(DataRowProductLot["FORM_WRK_TYPE_CODE"]).Equals("FORM_WORK_RT") && (ProcessCode.Equals(Process.CELL_BOXING) || ProcessCode.Equals(Process.CELL_BOXING_RETURN))))
            {
                // 일반 재작업, 반품 재작업이고 포장(Cell포장,물류반품)인 경우 팝업호출
                CraetInboxPopup();
            }
            else
            {
                InputInbox();
            }
        }
        #endregion

        #region Inbox 삭제
        private void btnInboxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxDelete()) return;

            // IInbox를 삭제 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4332", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteInbox();
                }
            }, parameters);
        }
        #endregion

        #region Inbox 수정
        private void btnInboxSave_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            dgProductionInbox.EndEditRow(true);

            if (!ValidationInboxModify())
                return;

            // Inbox를 수정 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4331", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyInbox();
                }
            }, parameters);
        }
        #endregion

        #region 양품 태그 발행
        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectPrint(dgProductionInbox, true)) return;

            PrintLabel("G", dgProductionInbox);
        }
        #endregion

        #region Grid Event
        private void dgProductionInbox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAKEOVER_YN")).Equals("Y")
                        || Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STORED_YN")).Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgProductionInbox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgProductionInbox_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if ((e.Cell.Column.Name.Equals("CELL_QTY") || e.Cell.Column.Name.Equals("CAPA_GRD_CODE")) && e.Cell.IsEditable == true)
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                    //e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }
            }

        }

        #endregion

        #endregion

        #region ### [불량] ###

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void cboGradeSelect_SelectionChanged(object sender, EventArgs e)
        {
            SetDefectGridGradeColumn();
        }

        /// <summary>
        /// 2018-03-06 불량탭 관련 추가
        /// </summary>
        private void btnDefectInput_Click(object sender, RoutedEventArgs e)
        {
            btnDefectInput.Background = new SolidColorBrush(Colors.Red);
            btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Black);

            SetDefectGridInput(false);
        }

        private void btnDefectPrintSelect_Click(object sender, RoutedEventArgs e)
        {
            btnDefectInput.Background = new SolidColorBrush(Colors.Black);
            btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Red);

            SetDefectGridInput(true);
        }

        #region 불량 태그 라벨 발행
        private void btnDefectPrint_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationDefectPrint(dgProductionDefect, false)) return;

            PrintLabel("N", dgProductionDefect);
        }

        #endregion

        #region 불량 저장
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            dgProductionDefect.EndEdit();
            dgProductionDefect.EndEditRow(true);

            if (!ValidationDefectSave()) return;

            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDefect();
                }
            });
        }

        #endregion

        #region Grid Event
        private void dgProductionDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row.Type != DataGridRowType.Item)
                return;

            if (e.Cell.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                CheckBox cbo = e.Cell.Presenter.Content as CheckBox;
                if (cbo != null && cbo.IsChecked == true)
                {
                    cbo.Visibility = DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID").GetString() != "CHARGE_PROD_LOT" ? Visibility.Visible : Visibility.Collapsed;
                    //cbo.IsChecked = string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN").GetString(), "N");
                }
            }

            dgProductionDefect?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // IsReadOnly=True 색상표시
                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible || e.Cell.Column.Name.Equals("CHK"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                // 출력 색상 표시
                if (e.Cell.Column.Name.IndexOf("GRADE") > -1 && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45);

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN" + e.Cell.Column.Name.Substring(5, e.Cell.Column.Name.Length - 5))).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F7C8"));
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").Equals(1))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0DA"));
                    }
                    else
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEBEB"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }
            }));
        }

        private void dgProductionDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    }
                }
            }));
        }

        private void dgProductionDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2018-03-06 불량탭 추가
        /// </summary>
        private void dgProductionDefect_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                if (cell.Column.IsReadOnly == true)
                {
                    return;
                }

                SetDefectGridSelect(cell.Row.Index, cell.Column.Index);
            }

        }

        /// <summary>
        /// 2018-03-06 불량탭 추가
        /// </summary>
        private void dgProductionDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (e.Cell.Column.Name.IndexOf("GRADE") > -1)
                {
                    double ResnQtyColumnSum = 0;
                    int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

                    for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        ResnQtyColumnSum += DataTableConverter.GetValue(e.Cell.Row.DataItem, dgProductionDefect.Columns[col].Name).GetDouble();
                    }
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", ResnQtyColumnSum);
                }
            }
        }


        #endregion

        #endregion

        #endregion

        #region Mehod

        #region [외부 호출]
        public void SetTabVisibility()
        {
            if (string.Equals(ProcessCode, Process.PolymerFairQuality))
            {
                tiDefect.Visibility = Visibility.Collapsed;
            }
        }

        public void SetButtonVisibility()
        {
        }
        public void SetGridColumnVisibility()
        {
            if (string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || string.Equals(ProcessCode, Process.CELL_BOXING))
            {
                
                dgProductionInbox.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
                dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Collapsed;
                dgProductionInbox.Columns["TAKEOVER_YN"].Visibility = Visibility.Collapsed;
            }
        }

        public void SetControlHeader()
        {
        }

        public void SetInboxType()
        {
            SetEqptInboxType();
        }

        public void SelectResultList()
        {
            SelectInboxList();
            ////if (!string.Equals(ProcessCode, Process.PolymerFairQuality))
            SelectDefectList();
            SelectGradeList();
        }

        private void SelectResultListLocal(string job)
        {
            if (job.Equals("CHANGE"))
            {
                SelectInboxList();
                GetProductCart();
            }
            else if (job.Equals("MODIFY"))
            {
                GetProductLot();  
                SelectInboxList(); 
                SelectGradeList();
                GetProductCart();
            }
            else if (job.Equals("PRINT_G"))
            {
                GetProductCart();
                SelectInboxList();
            }
            else if (job.Equals("PRINT_N"))
            {
                GetProductCart();
                SelectDefectList();
            }
            else if (job.Equals("CELL"))
            {
                SelectInboxList();
            }
            else
            {
                GetProductLot(); 
                GetProductCart(); 
                SelectInboxList(); 
                SelectGradeList(); 
            }

        }

        public DataRow GetSelectPalletLotRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

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

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                ////for (int i = 0; i < parameterArrys.Length; i++)
                ////{
                ////    parameterArrys[i] = true;
                ////}

                parameterArrys[0] = true;
                parameterArrys[1] = null;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void GetProductCart()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductCart");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                ////for (int i = 0; i < parameterArrys.Length; i++)
                ////{
                ////    parameterArrys[i] = true;
                ////}

                parameterArrys[0] = null;
                parameterArrys[1] = true;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [BizCall]

        #region 양품화 공정 여부 체크
        private bool ChkCartStatus()
        {
            bool IsInspProc = false;
            _finalExternal = false;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDTYPE"] = "FORM_REWORK_PROCID";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow[] dr = dtResult.Select("CBO_CODE = '" + ProcessCode + "'");

                    if (dr.Length > 0)
                    {
                        IsInspProc = true;
                        _finalExternal = true;
                    }
                }

                return IsInspProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsInspProc;
            }
        }
        #endregion

        #region 완성 Pallet
        /// <summary>
        /// 설비 InBox 유형 조회
        /// </summary>
        private void SetEqptInboxType()
        {
            try
            {
                _inboxTypeCode = string.Empty;
                txtInboxType.Text = string.Empty;
                _inboxLoadQty = 0;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _inboxTypeCode = dtResult.Rows[0]["INBOX_TYPE_CODE"].ToString();
                    txtInboxType.Text = dtResult.Rows[0]["INBOX_TYPE_NAME"].ToString();

                    if (!int.TryParse(dtResult.Rows[0]["INBOX_LOAD_QTY"].ToString(), out _inboxLoadQty))
                        _inboxLoadQty = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Inbox Max 생성수 조회
        /// </summary>
        private void SeInboxMaxQty()
        {
            try
            {
                int inboxMaxQty = 0;

                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string)); ;

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "INBOX_MAX_PRT_QTY";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null || dtResult.Rows.Count > 0)
                {
                    if (!int.TryParse(dtResult.Rows[0]["ATTRIBUTE1"].ToString(), out inboxMaxQty)) inboxMaxQty = 0;
                }

                numAddCount.Maximum = inboxMaxQty;
                numAddCount.Minimum = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 용량 등급
        /// </summary>
        private void SetGridCapaGrade()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                ////DataRow dr = dtResult.NewRow();
                ////dr["CBO_CODE"] = "";
                ////dr["CBO_NAME"] = " - SELECT-";
                ////dtResult.Rows.InsertAt(dr, 0);

                (dgProductionInbox.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void SetGradeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            mcb.Check(i);
                        }
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectInboxList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("DELETE_YN", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = WipSeq;
                newRow["DELETE_YN"] = "N";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_PC", "INDATA", "OUTDATA", inTable);

                //dgProductionInbox.CurrentCellChanged -= dgProductionInbox_CurrentCellChanged;

                Util.GridSetData(dgProductionInbox, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

                _inBoxHeaderType = CheckBoxHeaderType.Zero;
                //dgProductionInbox.CurrentCellChanged += dgProductionInbox_CurrentCellChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 대차 변경
        /// </summary>
        private void ChangeCart()
        {
            try
            {
                ShowParentLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CTNR_ID"] = ProdCartId;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = drDel["INBOX_ID"].ToString();
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CHANGE_CTNR", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenParentLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        SelectResultListLocal("CHANGE");
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox 투입
        /// </summary>
        private void InputInbox()
        {
            try
            {
                string sAommGrade = cboAommType.SelectedValue == null ? "" : cboAommType.SelectedValue.ToString();

                ShowParentLoadingIndicator();
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("MIX_LOT_FLAG", typeof(string));
                inTable.Columns.Add("MIX_LOT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("LOAD_QTY", typeof(decimal));
                inTable.Columns.Add("INBOX_QTY", typeof(decimal));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inBox.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inBox.Columns.Add("CELL_QTY", typeof(decimal));
                inBox.Columns.Add("INBOX_QTY", typeof(decimal));
                inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                inBox.Columns.Add("AOMM_GRD_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = ShiftID;
                newRow["WRK_USERID"] = WorkerID;
                newRow["WRK_USER_NAME"] = WorkerName;
                newRow["CTNR_ID"] = ProdCartId;
                newRow["VISL_INSP_USERID"] = InspectorCode;
                newRow["PROCID"] = Util.NVC(ProcessCode);
                newRow["SHIPTO_ID"] = ShiptoID;
                inTable.Rows.Add(newRow);

                int TotalInboxQty = 0;

                for (int i = 0; i < numAddCount.Value.GetInt(); i++)
                {
                    TotalInboxQty++;

                    DataRow dr = inBox.NewRow();
                    dr["CAPA_GRD_CODE"] = cboCapaType.SelectedValue.ToString();
                    dr["CELL_QTY"] = _inboxLoadQty;
                    dr["INBOX_QTY"] = 1;
                    dr["INBOX_TYPE_CODE"] = _inboxTypeCode;
                    dr["AOMM_GRD_CODE"] = sAommGrade;
                    inBox.Rows.Add(dr);
                }
                //string xml = inDataSet.GetXml();

                inTable.Rows[0]["LOAD_QTY"] = _inboxLoadQty;
                inTable.Rows[0]["INBOX_QTY"] = TotalInboxQty;
                inTable.AcceptChanges();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_INBOX", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenParentLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SelectResultListLocal("CREATE");
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox 삭제
        /// </summary>
        private void DeleteInbox()
        {
            try
            {
                ShowParentLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CTNR_ID"] = ProdCartId; 
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach(DataRow drDel in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = drDel["INBOX_ID"].ToString();
                    inLot.Rows.Add(newRow);
                }

                // BR_PRD_REG_DELETE_PALLET -> BR_PRD_REG_DELETE_INBOX
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_INBOX", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenParentLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SelectResultListLocal("DELETE");

                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox 수정
        /// </summary>
        private void ModifyInbox()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || string.Equals(ProcessCode, Process.CELL_BOXING))
                    bizRuleName = "BR_PRD_REG_MODIFY_PALLET_NJ";
                else
                    bizRuleName = "BR_PRD_REG_MODIFY_PALLET";


                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("WIPQTY", typeof(Decimal));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIPTO_NOTE", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inTable.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inTable.Columns.Add("RSST_GRD_CODE", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                inTable.Columns.Add("INBOX_QTY", typeof(Decimal));
                inTable.Columns.Add("AOMM_GRD_CODE", typeof(string));


                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = ProcessCode;
                newRow["PALLETID"] = dr[0]["INBOX_ID"];
                newRow["WIPQTY"] = dr[0]["CELL_QTY"];
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = ShiftID;
                newRow["WRK_USERID"] = WorkerID;
                newRow["WRK_USER_NAME"] = WorkerName;
                newRow["CAPA_GRD_CODE"] = dr[0]["CAPA_GRD_CODE"];
                newRow["INBOX_TYPE_CODE"] = dr[0]["INBOX_TYPE_CODE"];
                newRow["INBOX_QTY"] = dr[0]["INBOX_QTY"];
                newRow["AOMM_GRD_CODE"] = dr[0]["AOMM_GRD_CODE"];
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenParentLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        SelectResultListLocal("MODIFY");
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 불량, 물청
        public void SelectDefectList()
        {
            try
            {
                ShowParentLoadingIndicator();

                // 해더 정보 조회
                GetDefectHeader();

                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT_INFO_PC";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 불량 저장시 비교용
                        _defectList = result;

                        Util.GridSetData(dgProductionDefect, result, null, true);

                        if (CommonVerify.HasTableRow(result))
                        {
                            //C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgProductionDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                            //StackPanel allPanel = allColumn?.Header as StackPanel;
                            //CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                            //if (allCheck != null) allCheck.IsChecked = true;

                            // 저장,조회시 현상태 유지
                            if (!_defectTabButtonClick)
                                SetDefectGridSelect(-1, -1, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetDefectHeader()
        {
            try
            {
                _defectGradeCount = 0;
                _IsDefectSave = true;

                ShowParentLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_FORM_GRADE_TYPE_CODE_HEADER";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(result))
                        {
                            _defectGradeCount = result.Rows.Count;

                            for (int row = 0; row < result.Rows.Count; row++)
                            {
                                string ColumnName = "GRADE" + (row + 1).ToString();
                                dgProductionDefect.Columns[ColumnName].Header = Util.NVC(result.Rows[row]["GRD_CODE"]);
                                dgProductionDefect.Columns[ColumnName].Visibility = Visibility.Visible;
                            }

                            // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                            SetDefectGridGradeColumn();
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefect()
        {
            try
            {
                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DEFECT_MCP";

                // 변경값 비교
                DataTable defect = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
                DataTable defectSave = new DataTable();
                defectSave.Columns.Add("NEW_ROW", typeof(int));
                defectSave.Columns.Add("NEW_COL", typeof(int));

                int ColStart = defect.Columns.IndexOf("GRADE1");
                int ColEnd = defect.Columns.IndexOf("GRADE1") + _defectGradeCount;

                for (int row = 0; row < defect.Rows.Count; row++)
                {
                    for (int col = ColStart; col < ColEnd; col++)
                    {
                        if (Util.NVC(defect.Rows[row][col]) != Util.NVC(_defectList.Rows[row][col]))
                        {
                            DataRow newrow = defectSave.NewRow();
                            newrow["NEW_ROW"] = row;
                            newrow["NEW_COL"] = col;
                            defectSave.Rows.Add(newrow);
                        }
                    }
                }

                // 변경자료가 없으면 Return
                if (defectSave.Rows.Count == 0)
                {
                    HiddenParentLoadingIndicator();

                    // 변경내용이 없습니다.
                    Util.MessageInfo("SFU1226");
                    return;
                }

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                // 불량 코드별 불량수량
                DataTable inDefectTable = ds.Tables.Add("INRESN");
                inDefectTable.Columns.Add("LOTID", typeof(string));
                inDefectTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectTable.Columns.Add("ACTID", typeof(string));
                inDefectTable.Columns.Add("RESNCODE", typeof(string));
                inDefectTable.Columns.Add("RESNQTY", typeof(double));
                inDefectTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDefectTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDefectTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectTable.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDefectTable.Columns.Add("COST_CNTR_ID", typeof(string));
                inDefectTable.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDefectTable.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                // 불량 코드, 등급별 수량
                DataTable inDefectGrdTable = ds.Tables.Add("INGRD");
                inDefectGrdTable.Columns.Add("LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectGrdTable.Columns.Add("ACTID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNCODE", typeof(string));
                inDefectGrdTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inDefectGrdTable.Columns.Add("RESNQTY", typeof(double));
                inDefectGrdTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectGrdTable.Columns.Add("DFCT_GR_LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("LABEL_PRT_FLAG", typeof(string));

                foreach (DataRow row in defectSave.Rows)
                {
                    // `불량코드별 수량
                    string lotid = ProdLotId;
                    string wipseq = WipSeq;
                    string actid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["ACTID"]);
                    string resncode = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNCODE"]);
                    string resnqty = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNQTY"]);
                    string costcntrid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["COST_CNTR_ID"]);
                    string capagrdcode = Util.NVC(dgProductionDefect.Columns[int.Parse(row["NEW_COL"].ToString())].Header);
                    string graderesnqty = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())][int.Parse(row["NEW_COL"].ToString())]);
                    string columnheadername = Util.NVC(defect.Columns[int.Parse(row["NEW_COL"].ToString())].ColumnName);

                    DataRow[] drSelect = inDefectTable.Select("ACTID = '" + actid + "' And RESNCODE = '" + resncode + "'");
                    if (drSelect.Length == 0)
                    {
                        DataRow newRow = inDefectTable.NewRow();
                        newRow["LOTID"] = lotid;
                        newRow["WIPSEQ"] = wipseq;
                        newRow["ACTID"] = actid;
                        newRow["RESNCODE"] = resncode;
                        newRow["RESNQTY"] = resnqty.Equals("") ? 0 : double.Parse(resnqty);
                        newRow["RESNCODE_CAUSE"] = string.Empty;
                        newRow["PROCID_CAUSE"] = string.Empty;
                        newRow["RESNNOTE"] = string.Empty;
                        newRow["DFCT_TAG_QTY"] = 0;
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;

                        if (actid.Equals("CHARGE_PROD_LOT"))
                        {
                            newRow["COST_CNTR_ID"] = costcntrid;
                        }
                        else
                        {
                            newRow["COST_CNTR_ID"] = string.Empty;
                        }

                        newRow["A_TYPE_DFCT_QTY"] = 0;
                        newRow["C_TYPE_DFCT_QTY"] = 0;
                        inDefectTable.Rows.Add(newRow);
                    }

                    // 불량 등급별 수량
                    DataRow newRowGrd = inDefectGrdTable.NewRow();
                    newRowGrd["LOTID"] = lotid;
                    newRowGrd["WIPSEQ"] = wipseq;
                    newRowGrd["ACTID"] = actid;
                    newRowGrd["RESNCODE"] = resncode;
                    newRowGrd["CAPA_GRD_CODE"] = capagrdcode;
                    newRowGrd["RESNQTY"] = graderesnqty.Equals("") ? 0 : double.Parse(graderesnqty);
                    newRowGrd["RESNNOTE"] = string.Empty;
                    newRowGrd["DFCT_GR_LOTID"] = string.Empty;
                    newRowGrd["LABEL_PRT_FLAG"] = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["PRINT_YN" + columnheadername.Substring(5, columnheadername.Length - 5)]);
                    inDefectGrdTable.Rows.Add(newRowGrd);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN,INGRD", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");
                        SelectDefectList();
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveDefectPrint()
        {
            try
            {
                _IsDefectSave = true;

                ShowParentLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DEFECT_MCP";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                // 불량 코드별 불량수량
                DataTable inDefectTable = ds.Tables.Add("INRESN");
                inDefectTable.Columns.Add("LOTID", typeof(string));
                inDefectTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectTable.Columns.Add("ACTID", typeof(string));
                inDefectTable.Columns.Add("RESNCODE", typeof(string));
                inDefectTable.Columns.Add("RESNQTY", typeof(double));
                inDefectTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDefectTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDefectTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectTable.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDefectTable.Columns.Add("COST_CNTR_ID", typeof(string));
                inDefectTable.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDefectTable.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                // 불량 코드, 등급별 수량
                DataTable inDefectGrdTable = ds.Tables.Add("INGRD");
                inDefectGrdTable.Columns.Add("LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectGrdTable.Columns.Add("ACTID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNCODE", typeof(string));
                inDefectGrdTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inDefectGrdTable.Columns.Add("RESNQTY", typeof(double));
                inDefectGrdTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectGrdTable.Columns.Add("DFCT_GR_LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("LABEL_PRT_FLAG", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridCell cell in dgProductionDefect.Selection.SelectedCells)
                {
                    if (cell.Row.Index >= dgProductionDefect.Rows.Count + dgProductionDefect.FrozenBottomRowsCount)
                        continue;

                    if (cell.Column.Index < dgProductionDefect.Columns["GRADE1"].Index)
                        continue;

                    if (cell.Column.Index > dgProductionDefect.Columns["GRADE1"].Index + _defectGradeCount)
                        continue;

                    DataRow newRow = inDefectGrdTable.NewRow();
                    newRow["LOTID"] = ProdLotId;
                    newRow["WIPSEQ"] = WipSeq;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                    newRow["CAPA_GRD_CODE"] = cell.Column.Header.ToString();
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, dgProductionDefect.Columns[cell.Column.Index].Name.ToString())).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, dgProductionDefect.Columns[cell.Column.Index].Name.ToString())));
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_GR_LOTID"] = string.Empty;
                    newRow["LABEL_PRT_FLAG"] = "Y";
                    inDefectGrdTable.Rows.Add(newRow);

                    DataRow[] drSelect = inDefectTable.Select("ACTID ='" + Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID")) + "' And " +
                                                              "RESNCODE ='" + Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE")) + "'");

                    if (drSelect.Length == 0)
                    {
                        newRow = inDefectTable.NewRow();
                        newRow["LOTID"] = ProdLotId;
                        newRow["WIPSEQ"] = WipSeq;
                        newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID"));
                        newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                        newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNQTY")));
                        newRow["RESNCODE_CAUSE"] = string.Empty;
                        newRow["PROCID_CAUSE"] = string.Empty;
                        newRow["RESNNOTE"] = string.Empty;
                        newRow["DFCT_TAG_QTY"] = 0;
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;

                        if (Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                        {
                            newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "COST_CNTR_ID"));
                        }
                        else
                        {
                            newRow["COST_CNTR_ID"] = string.Empty;
                        }

                        newRow["A_TYPE_DFCT_QTY"] = 0;
                        newRow["C_TYPE_DFCT_QTY"] = 0;
                        inDefectTable.Rows.Add(newRow);

                    }

                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN,INGRD", null, (bizResult, bizException) =>
                {
                    HiddenParentLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            _IsDefectSave = false;
                            return;
                        }

                        //Util.MessageInfo("SFU1275");
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        _IsDefectSave = false;
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
                _IsDefectSave = false;
            }
        }
        #endregion

        #region 등급별 수량
        private void SelectGradeList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = WipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_GRADE_PC", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionGrade, dtResult, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion;

        #region[[Validation]
        private bool ValidationCartChange()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            foreach(DataRow drchk in dr)
            {
                if (ProdCartId == drchk["CTNR_ID"].ToString())
                {
                    // 변경전 대차와 변경할 대차가 동일 합니다.
                    Util.MessageValidation("SFU4366");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationCellInput()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            //DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            //if (dr.Length == 0)
            //{
            //    // 선택된 항목이 없습니다.
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            //if (dr.Length > 1)
            //{
            //    // 한행만 선택 가능 합니다.
            //    Util.MessageValidation("SFU4023");
            //    return false;
            //}

            return true;
        }

        private bool ValidationInboxInput()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtInboxType.Text))
            {
                // 설비의 Inbox 유형을 설정 하세요.
                Util.MessageValidation("SFU4354");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProdCartId))
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (string.IsNullOrWhiteSpace(WorkerID))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if ((string.IsNullOrWhiteSpace(InspectorCode)  || InspectorCode.Equals("SELECT")) && 
                !(string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || string.Equals(ProcessCode, Process.CELL_BOXING)))
            {
                // 검사자를 입력해주세요
                Util.MessageValidation("SFU1452");
                return false;
            }

            if (cboCapaType.SelectedValue == null)
            {
                // 용량 등급을 선택하세요
                Util.MessageValidation("SFU4092");
                return false;
            }

            if(_AommGrdChkFlag == true)
            {
                if (string.IsNullOrEmpty(Util.NVC(cboAommType.SelectedValue)))
                {
                    Util.Alert("SFU8401");
                    return false;
                }

                //AOMM 등급 혼입안되도록
                DataTable dt = DataTableConverter.Convert(dgProductionInbox.ItemsSource);
                for (int inx = 0; inx < dt.Rows.Count; inx++)
                {
                    if(Util.NVC(dt.Rows[inx]["AOMM_GRD_CODE"]) != Util.NVC(cboAommType.SelectedValue))
                    {
                        //동일한 AOMM 등급을 선택하세요.
                        Util.MessageValidation("SFU3803");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidationInboxModify()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr.Length > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
                return false;
            }

            if (dr[0]["TAKEOVER_YN"].ToString().Equals("Y"))
            {
                // 재공이 다음 공정으로 이동 되어 수정할 수 없습니다.
                Util.MessageValidation("SFU4367");
                return false;
            }

            if (dr[0]["STORED_YN"].ToString().Equals("Y"))
            {
                // 대차에 보관중인 Inbox는 수정할 수 없습니다.
                Util.MessageValidation("SFU4435");
                return false;
            }

            if (_AommGrdChkFlag == true)
            {
                string AommGrdCode = dr[0]["AOMM_GRD_CODE"].ToString();

                if (string.IsNullOrEmpty(Util.NVC(AommGrdCode)))
                {
                    Util.Alert("SFU8401");
                    return false;
                }

                //AOMM 등급 혼입안되도록
                DataTable dt = DataTableConverter.Convert(dgProductionInbox.ItemsSource);
                for (int inx = 0; inx < dt.Rows.Count; inx++)
                {
                    if (Util.NVC(dt.Rows[inx]["AOMM_GRD_CODE"]) != AommGrdCode)
                    {
                        //동일한 AOMM 등급을 선택하세요.
                        Util.MessageValidation("SFU3803");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidationInboxDelete()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            foreach (DataRow drChk in dr)
            {
                if (drChk["TAKEOVER_YN"].Equals("Y"))
                {
                    // 재공이 다음 공정으로 이동 되어 삭제할 수 없습니다.
                    Util.MessageValidation("SFU1871");
                    return false;
                }

                if (drChk["STORED_YN"].Equals("Y"))
                {
                    //대차에 보관중인 Inbox는 삭제할 수 없습니다.
                    Util.MessageValidation("SFU4434");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDefectPrint(C1DataGrid dg, bool IsGoodTag, bool isCompleteValidation = false)
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrEmpty(ShiftID))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(WorkerID))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            //if ((string.IsNullOrEmpty(InspectorCode) || InspectorCode.Equals("SELECT")) && !(string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || string.Equals(ProcessCode, Process.CELL_BOXING)))
            //{
            //    //검사자를 입력해주세요.
            //    Util.MessageValidation("SFU1452");
            //    return false;
            //}

            if (IsGoodTag)
            {
                // 양품 태그
                if (_util.GetDataGridCheckFirstRowIndex(dg, "CHK") < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");
                foreach (DataRow drrow in dr)
                {
                    if (Util.NVC_Int(drrow["CELL_QTY"]) != Util.NVC_Int(drrow["ORG_CELL_QTY"])
                        || Util.NVC(drrow["CAPA_GRD_CODE"]) != Util.NVC(drrow["ORG_CAPA_GRD_CODE"]))
                    {
                        // 변경된 데이터가 존재합니다.\r\n먼저 저장한 후 태그 발행하세요.
                        Util.MessageValidation("SFU4447");
                        return false;
                    }
                }
            }
            else
            {
                if (dg.Selection.SelectedCells.Count == 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if ((string.IsNullOrEmpty(InspectorCode) || InspectorCode.Equals("SELECT")) && !(string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || string.Equals(ProcessCode, Process.CELL_BOXING)))
                {
                    //검사자를 입력해주세요.
                    Util.MessageValidation("SFU1452");
                    return false;
                }
            }

            // 완료 버튼 클릭일때는 라벨출력 프린트 확인 안함
            if (isCompleteValidation == true)
            {
                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                    return false;
                }

                string labelCode = dg.Name == "dgProductionInbox" ? "LBL0106" : "LBL0107";

                var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                             where t.Field<string>("LABELID") == labelCode
                             select t).ToList();

                if (!query.Any())
                {
                    Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                    return false;
                }
                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
                {
                    Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDefectSave()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgProductionDefect))
            {
                // 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return false;
            }

            if (dgProductionDefect.ItemsSource == null || dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount == 0)
            {
                // 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return false;
            }

            //if (_util.GetDataGridCheckFirstRowIndex(dgProductionDefect, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            return true;
        }

        #endregion;

        #region [Func]

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgProductionInbox;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", DataTableConverter.GetValue(row.DataItem, "PRINT_YN").GetString() != "Y");
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.Two:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Two;
                    break;
                case CheckBoxHeaderType.Two:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }


        private void chkAllInbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionInbox);
        }
        private void chkAllDefect_Checked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllChecked(dgProductionDefect);

            // 2018-03-06 불량탭 추가
            SetDefectGridSelect();
        }

        private void chkAllDefect_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionDefect);

            // 2018-03-06 불량탭 추가
            // Load Event 호출
            DataTable dt = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
            Util.GridSetData(dgProductionDefect, dt, null, true);
        }

        private void PrintLabel(string IsQultType, C1DataGrid dg)
        {
            if (IsQultType.Equals("N"))
            {
                // 선택 추가
                SetDefectGridCheckSelect();
                if (!ValidationDefectPrint(dg, false)) return;

                // 선택 불량저장
                SaveDefectPrint();
                if (_IsDefectSave.Equals(false)) return;
            }

            string processName = ProcessName;
            string modelId = DataRowProductLot["MODLID"].GetString();
            string projectName = DataRowProductLot["PRJT_NAME"].GetString();
            string marketTypeName = DataRowProductLot["MKT_TYPE_NAME"].GetString();
            string assyLotId = DataRowProductLot["LOTID_RT"].GetString();
            string calDate = DataRowProductLot["CALDATE"].GetString();
            string shiftName = ShiftName;
            string equipmentShortName = DataRowProductLot["EQPTSHORTNAME"].GetString();
            string inspectorId = InspectorId;

            // 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //AOMM
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            // 라벨 발행이력 저장
            string BizRuleName = string.Empty;
            if (IsQultType.Equals("G"))
            {
                BizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST";
            }
            else
            {
                BizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST_DEFECT";
            }

            if (IsQultType.Equals("G"))
            {
                foreach (DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item &&
                        (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                         DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                    {
                        DataRow dr = dtLabelItem.NewRow();

                        dr["LABEL_CODE"] = "LBL0106";
                        dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                        dr["ITEM002"] = modelId + "(" + projectName + ") ";
                        //dr["ITEM003"] = assyLotId;
                        dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "ASSY_LOT"));
                        dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "CELL_QTY")).GetString();
                        dr["ITEM005"] = equipmentShortName;
                        dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                        //dr["ITEM007"] = inspectorId;
                        dr["ITEM007"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INSPECTORID"));
                        dr["ITEM008"] = marketTypeName;
                        dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                        dr["ITEM010"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "AOMM_GRD_CODE"));
                        dr["ITEM011"] = string.Empty;

                        // 라벨 발행 이력 저장
                        DataRow newRow = inTable.NewRow();
                        newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                        newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));    // Cell ID
                        newRow["PRT_ITEM02"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                        newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));                                                // 재발행 여부
                        newRow["INSUSER"] = LoginInfo.USERID;
                        newRow["LOTID"] = ProdLotId;
                        newRow["LABEL_CODE"] = "LBL0106";
                        newRow["PGM_ID"] = _sPGM_ID;
                        newRow["BZRULE_ID"] = BizRuleName;
                        inTable.Rows.Add(newRow);

                        dtLabelItem.Rows.Add(dr);
                    }
                }
            }
            else
            {
                foreach (C1.WPF.DataGrid.DataGridCell cell in dgProductionDefect.Selection.SelectedCells)
                {
                    if (cell.Row.Index >= dgProductionDefect.Rows.Count + dgProductionDefect.FrozenBottomRowsCount)
                        continue;

                    if (cell.Column.Index < dgProductionDefect.Columns["GRADE1"].Index)
                        continue;

                    if (cell.Column.Index > dgProductionDefect.Columns["GRADE1"].Index + _defectGradeCount)
                        continue;

                    DataRow dr = dtLabelItem.NewRow();

                    dr["LABEL_CODE"] = "LBL0107";
                    dr["ITEM001"] = modelId + "(" + projectName + ") ";
                    dr["ITEM002"] = assyLotId;
                    dr["ITEM003"] = marketTypeName;
                    dr["ITEM004"] = cell.Value.ToString().Equals("0") ? string.Empty: cell.Value.ToString();
                    dr["ITEM005"] = equipmentShortName;
                    dr["ITEM006"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "DFCT_CODE_DETL_NAME"));
                    dr["ITEM007"] = calDate + "(" + shiftName + ")";
                    dr["ITEM008"] = inspectorId;
                    dr["ITEM009"] = string.Empty;
                    dr["ITEM010"] = cell.Column.Header.ToString();     // 등급
                    dr["ITEM011"] = ProdLotId +"+" + 
                                    Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNGR_ABBR_CODE")) + "+" + 
                                    cell.Column.Header.ToString();
                    dtLabelItem.Rows.Add(dr);

                    // 라벨 발행 이력 저장
                    DataRow newRow = inTable.NewRow();
                    newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                    newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNGRID")); 
                    newRow["PRT_ITEM03"] = cell.Column.Header.ToString();
                    newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["LOTID"] = ProdLotId;
                    newRow["LABEL_CODE"] = "LBL0107";
                    newRow["PGM_ID"] = _sPGM_ID;
                    newRow["BZRULE_ID"] = BizRuleName;
                    inTable.Rows.Add(newRow);
                }
            }

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
               return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);
            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                new ClientProxy().ExecuteService(BizRuleName, "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (IsQultType.Equals("G"))
                            SelectResultListLocal("PRINT_G");
                        else
                            SelectResultListLocal("PRINT_N");

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
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                if (dg.Name.Equals("dgProductionInbox"))
                {
                    allCheck.Unchecked -= chkAllInbox_Unchecked;
                    allCheck.IsChecked = false;
                    allCheck.Unchecked += chkAllInbox_Unchecked;
                }
                else
                {
                    allCheck.Unchecked -= chkAllDefect_Unchecked;
                    allCheck.IsChecked = false;
                    allCheck.Unchecked += chkAllDefect_Unchecked;
                }
            }
        }

        private void ShowParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HiddenParentLoadingIndicator()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("HiddenLoadingIndicator");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    for (int i = 0; i < parameterArrys.Length; i++)
                    {
                        parameterArrys[i] = null;
                    }

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 완성 Inbox 팝업
        /// </summary>
        private void CraetInboxPopup()
        {
            Popup.CMM_POLYMER_FORM_INBOX_COMPLET popupCraetInbox = new Popup.CMM_POLYMER_FORM_INBOX_COMPLET();
            popupCraetInbox.FrameOperation = this.FrameOperation;

            DataTable dt = DataTableConverter.Convert(dgProductionInbox.ItemsSource);

            // 생산 LOT의 첫 Inbox 생성 FIRST, 등급별 생성 GRADE, 수량으로 생성 QTY
            string CreatType = string.Empty;

            if (dt == null || dt.Rows.Count == 0)
                CreatType = "FIRST";
            else if (Util.NVC(dt.Rows[0]["CAPA_GRD_CODE"]).Equals(""))
                CreatType = "QTY";
            else
                CreatType = "GRADE";

            popupCraetInbox.ShiftID = ShiftID;
            popupCraetInbox.ShiftName = ShiftName;
            popupCraetInbox.WorkerName = WorkerName;
            popupCraetInbox.WorkerID = WorkerID;
            popupCraetInbox.ShiftDateTime = ShiftDateTime;
            popupCraetInbox.InspectorCode = InspectorCode;
            popupCraetInbox.ShiptoID = ShiptoID;

            object[] parameters = new object[9];
            parameters[0] = ProcessCode;
            parameters[1] = ProcessName;
            parameters[2] = EquipmentCode;
            parameters[3] = EquipmentName;
            parameters[4] = EquipmentSegmentCode;
            parameters[5] = ProdCartId;
            parameters[6] = CreatType;
            parameters[7] = DataRowProductLot;
            if (_AommGrdChkFlag == true)
            {
                parameters[8] = Util.NVC(cboAommType.SelectedValue);
            }
            else
            {
                parameters[8] = null;
            }
            

            C1WindowExtension.SetParameters(popupCraetInbox, parameters);

            popupCraetInbox.Closed += new EventHandler(popupCartDefectComplet_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCraetInbox);
                    popupCraetInbox.BringToFront();
                    break;
                }
            }
        }

        private void popupCartDefectComplet_Closed(object sender, EventArgs e)
        {
            Popup.CMM_POLYMER_FORM_INBOX_COMPLET popup = sender as Popup.CMM_POLYMER_FORM_INBOX_COMPLET;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            SelectResultListLocal("CREATE");

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        /// <summary>
        /// Cell 등록 팝업
        /// </summary>
        private void CellInputPopup()
        {
            Popup.CMM_POLYMER_FORM_CELL_IINSERT popupCellInput = new Popup.CMM_POLYMER_FORM_CELL_IINSERT();
            popupCellInput.FrameOperation = this.FrameOperation;

            if (ValidationGridAdd(popupCellInput.Name) == false)
                return;

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductionInbox, "CHK");

            object[] parameters = new object[2];
            parameters[0] = ProcessCode;
            parameters[1] = iRow == -1 ? null : DataTableConverter.GetValue(DgProductionInbox.Rows[iRow].DataItem, "INBOX_ID").GetString();
            C1WindowExtension.SetParameters(popupCellInput, parameters);

            popupCellInput.Closed += new EventHandler(popupCellInput_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCellInput);
                    popupCellInput.BringToFront();
                    break;
                }
            }
        }

        private void popupCellInput_Closed(object sender, EventArgs e)
        {
            Popup.CMM_POLYMER_FORM_CELL_IINSERT popup = sender as Popup.CMM_POLYMER_FORM_CELL_IINSERT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            SelectResultListLocal("CELL");

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }
        public void ChangeWorkerInfoByBox(string[] workerInfo)
        {
            this.ShiftID = workerInfo[0];
            this.ShiftName = workerInfo[1];
            this.WorkerID = workerInfo[2];
            this.WorkerName = workerInfo[3];
        }

        private void SetDefectGridSelect(int CheckRow = -1, int CheckCol = -1, bool IsraedInly = false)
        {
            if (dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount == 0)
                return;

            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgProductionDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;

            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            if (CheckRow.Equals(-1))
            {
                btnDefectInput.Background = new SolidColorBrush(Colors.Black);
                btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Red);

                // 조회 후 전체 또는 헤더의 전체 선택시
                dgProductionDefect.Selection.Clear();

                for (int row = 0; row < dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount; row++)
                {
                    for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        if (row == 0 && IsraedInly)
                        {
                            dgProductionDefect.Columns[col].IsReadOnly = true;
                        }

                        // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                        if (dgProductionDefect.Columns[col].Visibility == Visibility.Collapsed)
                            continue;

                        if (!Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[row].DataItem, "PRINT_YN" + dgProductionDefect.Columns[col].Name.Substring(5, dgProductionDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                        {
                            C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCell(row, col);

                            if (!IsraedInly)
                            {
                                // 조회후 조회시는 제외, 전체선택 Click시만 적용
                                DataTableConverter.SetValue(dgProductionDefect.Rows[row].DataItem, "CHK", true);
                                dgProductionDefect.Selection.Add(cell);
                            }
                        }
                    }
                }

            }
            else
            {
                if (allCheck.IsChecked == true)
                    return;

                if (CheckCol == dgProductionDefect.Columns["CHK"].Index)
                {
                    // Check 칼럼인 경우 출력을 제외한 Column 전체 선택
                    for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                        if (dgProductionDefect.Columns[col].Visibility == Visibility.Collapsed)
                            continue;

                        C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCell(CheckRow, col);

                        if (Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[CheckRow].DataItem, "PRINT_YN" + dgProductionDefect.Columns[col].Name.Substring(5, dgProductionDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                        {
                            cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgProductionDefect.Rows[CheckRow].DataItem, "CHK").Equals(0))
                            {
                                dgProductionDefect.Selection.Add(cell);

                                cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0DA"));
                            }
                            else
                            {
                                dgProductionDefect.Selection.Remove(cell);

                                if (cell.Column.IsReadOnly == true)
                                {
                                    cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEBEB"));
                                }
                                else
                                {
                                    cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                    }
                }

            }

            // header 체크
            int rowChkCount = DataTableConverter.Convert(dgProductionDefect.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (allCheck != null && allCheck.IsChecked == false && rowChkCount == dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount)
            {
                allCheck.Checked -= chkAllDefect_Checked;
                allCheck.IsChecked = true;
                allCheck.Checked += chkAllDefect_Checked;
            }

            dgProductionDefect.EndEdit();
            dgProductionDefect.EndEditRow(true);

        }

        private void SetDefectGridInput(bool breadonly)
        {
            if (dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount == 0)
            {
                return;
            }
            _defectTabButtonClick = true;

            dgProductionDefect.Selection.Clear();

            DataTable dt = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            for (int row = 0; row < dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount; row++)
            {
                dt.Rows[row]["CHK"] = 0;
            }

            dgProductionDefect.Columns["CHK"].IsReadOnly = breadonly == true ? false : true;

            for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
            {
                dgProductionDefect.Columns[col].IsReadOnly = breadonly;
            }

            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgProductionDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            allCheck.IsChecked = false;
            allCheck.IsEnabled = breadonly;

            Util.GridSetData(dgProductionDefect, dt, null, true);

        }

        /// <summary>
        /// 2018-03-06 불량탭 추가
        /// </summary>
        private void SetDefectGridCheckSelect()
        {
            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            for (int row = 0; row < dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount; row++)
            {
                for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                {
                    if (!DataTableConverter.GetValue(dgProductionDefect.Rows[row].DataItem, "CHK").Equals(1))
                        continue;

                    // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                    if (dgProductionDefect.Columns[col].Visibility == Visibility.Collapsed)
                        continue;

                    if (!Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[row].DataItem, "PRINT_YN" + dgProductionDefect.Columns[col].Name.Substring(5, dgProductionDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                    {
                        C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCell(row, col);
                        dgProductionDefect.Selection.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void SetDefectGridGradeColumn()
        {
            string gradeSelect = Util.NVC(cboGradeSelect.SelectedItemsToString).Replace(",", "");

            if (string.IsNullOrWhiteSpace(gradeSelect)) return;

            dgProductionDefect.Selection.Clear();
            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
            {
                if (gradeSelect.IndexOf(Util.NVC(dgProductionDefect.Columns[col].Header)) < 0)
                {
                    dgProductionDefect.Columns[col].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgProductionDefect.Columns[col].Visibility = Visibility.Visible; ;
                }
            }
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    foreach (UIElement ui in tmp.Children)
                    {
                        if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                        {
                            // 프로그램이 이미 실행 중 입니다. 
                            Util.MessageValidation("SFU3193");
                            return false;
                        }
                    }
                }
            }

            return true;
        }


















        #endregion;

        #endregion

        private void btnInboxComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectPrint(dgProductionInbox, true, true))
            {
                return;
            }

            Util.MessageConfirm("SFU3800", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CompleteInbox();
                }
            });
        }

        private void CompleteInbox()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable dtINDATA = inDataSet.Tables.Add("INDATA");
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataTable dtINLOT = inDataSet.Tables.Add("INLOT");
                dtINLOT.Columns.Add("LOTID", typeof(string));
                dtINLOT.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                foreach (DataGridRow row in dgProductionInbox.Rows)
                {
                    if (row.Type == DataGridRowType.Item &&
                        (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" || DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                    {
                        DataRow drINLOT = dtINLOT.NewRow();
                        drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                        drINLOT["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                        dtINLOT.Rows.Add(drINLOT);
                    }
                }

                ShowParentLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_COMP_INBOX_MCP", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenParentLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1889");    //정상 처리 되었습니다

                        SelectResultListLocal("PRINT_G");
                    }
                    catch (Exception ex)
                    {
                        HiddenParentLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);

            }
        }
    }
}
