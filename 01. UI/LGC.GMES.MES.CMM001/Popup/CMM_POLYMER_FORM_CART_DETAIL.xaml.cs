/*************************************************************************************
 Created Date : 2017.12.13
      Creator : 정문교
   Decription : 대차 상세
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.17 김린겸 CSR C20220729_000194 - Tag reissue A grade is not display 건 : 대차상세 - 태그재발생시 AOMM_GRD_CODE 출력
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private CheckBoxHeaderType _inBoxHeaderType;
        private DataTable _inboxList;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        public bool QueryCall { get; set; }

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_DETAIL()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
            if (QueryCall)
            {
                btnInboxChange.Visibility = Visibility.Collapsed;
            }
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtCartID.Text = tmps[4] as string;

            cboArea.IsEnabled = false;
            cboTakeOverUser.IsEnabled = false;

            SetGridCartList();
          
            if ( dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
                SetGridProductInboxList();
                SetShipToPopControl();
            }
            if (string.Equals(_procID, Process.CELL_BOXING) || string.Equals(_procID, Process.CELL_BOXING_RETURN))
            {
                grdCartMove.Visibility = Visibility.Collapsed;
                btnMove.Visibility = Visibility.Collapsed;
                dgProductionInbox.Columns["TAKEOVER_YN"].Visibility = Visibility.Collapsed;
            }
        }
        private void SetCombo()
        {
            // 인계, 인수화면에서 이동처리 -> 콤보 주석처리
            //SetAreaCombo();
            //SetMoveProcessCombo();
            //SetMoveLineCombo();
            //SetInspectorCombo();
        }

        private void SetDataGridColumnVisibility()
        {
            if (dgCart.Rows.Count > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("G"))
                {
                    tbInboxTitle.Text = ObjectDic.Instance.GetObjectName("INBOX");
                    dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("InBox ID");

                    // 양품
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Visible;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Collapsed;

                    dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["PRINT_YN"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Collapsed;

                    btnInboxLoss.Visibility = Visibility.Collapsed;
                    btnTagPrint.Visibility = Visibility.Visible;
                    btnCartCellRegister.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbInboxTitle.Text = ObjectDic.Instance.GetObjectName("불량그룹LOT");
                    dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("불량그룹LOT");

                    // 불량
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Visible;

                    dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["PRINT_YN"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Visible;

                    btnInboxLoss.Visibility = Visibility.Visible;
                    btnTagPrint.Visibility = Visibility.Collapsed;
                    btnCartCellRegister.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region [대차ID KeyDown]
        private void txtCartID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtCartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtCartID.Text)) return;

                SetGridCartList();
                if (dgCart != null && dgCart.Rows.Count > 0)
                {
                    SetGridAssyLotList();
                    SetGridProductInboxList();
                    SetMoveProcessCombo();
                    SetShipToPopControl();
                }
                else
                {
                    Util.gridClear(dgAssyLot);
                    Util.gridClear(dgProductionInbox);

                    // 이 대차는 현재공정에 존재하지 않습니다.
                    Util.MessageValidation("SFU4420");
                }
            }
        }
        #endregion

        #region 출하처 변경
        private void btnShiptoUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationShiptoUpdate()) return;

            ShiptoUpdate();
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

        #region Cell 등록 해제
        private void btnCellDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellDelete()) return;

            // 해제 하시겠습니까 ?
            Util.MessageConfirm("SFU4946", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteCell();
                }
            });
        }
        #endregion

        #region Inbox 정보 변경
        private void btnInboxChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxChange()) return;

            InboxChange();
        }
        #endregion

        #region Inbox Loss처리
        private void btnInboxLoss_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxLoss()) return;

            InboxLoss();
        }
        #endregion

        #region 대차재발행
        private void btnCartRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }

            // Page수 산출
            int PageMax = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("N") ? 50 : 40;
            int PageCount = dt.Rows.Count % PageMax != 0 ? (dt.Rows.Count / PageMax) + 1 : dt.Rows.Count / PageMax;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * PageMax) + 1;
                end = ((cnt + 1) * PageMax);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);

                // 대차Sheet발행
                CartRePrint(dr, cnt + 1);
            }

        }
        #endregion

        #region 조립Lot CHeck
        private void dgAssyLot_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAssyLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgAssyLot.ItemsSource);

                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1))
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 0;
                }
                else
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 1;
                }

                dt.AcceptChanges();
                Util.GridSetData(dgAssyLot, dt, null, true);

                // 완성 Inbox 조회
                GetGridProductInboxList(dt);
            }

        }
        #endregion

        #region Inbox Header CHeck
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
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAKEOVER_YN")).Equals("Y"))
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

        #endregion

        #region 양품 태그 발행
        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTagPrint()) return;

            // 체크된 조립LOT 만큼 반복 출력
            DataRow[] dr = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = 1");

            if (dr.Length > 0)
            {
                foreach (DataRow row in dr)
                {
                    PrintLabel(row);
                }
            }

        }
        #endregion

        #region [공장 변경]
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveProcessCombo();
            SetMoveLineCombo();

        }
        #endregion

        #region [공정 변경]
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveLineCombo();

        }
        #endregion

        #region [공장이동 체크]
        private void chkMoveArea_Checked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = true;
            cboTakeOverUser.IsEnabled = true;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        private void chkMoveArea_Unchecked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = false;
            cboTakeOverUser.IsEnabled = false;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }
        #endregion

        #region [이동]
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove())
                return;

            // 이동 하시겠습니까?
            Util.MessageConfirm("SFU1763", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartMove();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void SetAreaCombo()
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            string selectedValueText = cboArea.SelectedValuePath;
            string displayMemberText = cboArea.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboArea, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
        }

        private void SetMoveProcessCombo()
        {
            try
            {
                if (dgCart.Rows.Count == 0)
                    return;

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ROUTID", typeof(string));
                inTable.Columns.Add("FLOWID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue ?? null;
                newRow["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "ROUTID"));
                newRow["FLOWID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "FLOWID"));
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_MOVE_PROC_PC", "RQSTDT", "RSLTDT", inTable);

                //DataRow drSelect = dtResult.NewRow();
                //drSelect[cboProcess.SelectedValuePath] = "SELECT";
                //drSelect[cboProcess.DisplayMemberPath] = "- SELECT -";
                //dtResult.Rows.InsertAt(drSelect, 0);

                //cboProcess.ItemsSource = null;
                //cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                //cboProcess.SelectedIndex = 0;

                //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;

                cboProcess.ItemsSource = null;
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                int index = 0;

                if (cboProcess.Items.Count > 1 || cboProcess.Items.Count == 0)
                {
                    DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);
                    DataRow dr = dt.NewRow();
                    dr[cboProcess.SelectedValuePath] = "SELECT";
                    dr[cboProcess.DisplayMemberPath] = "- SELECT -";
                    dt.Rows.InsertAt(dr, 0);

                    cboProcess.ItemsSource = null;
                    cboProcess.ItemsSource = dt.Copy().AsDataView();
                }

                cboProcess.SelectedIndex = index;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMoveLineCombo()
        {
            try
            {
                string EqsgCode = string.Empty;

                if ((bool)chkMoveArea.IsChecked)
                {
                    EqsgCode = cboArea.SelectedValue == null ? null : cboArea.SelectedValue.ToString();
                }
                else
                {
                    EqsgCode = LoginInfo.CFG_AREA_ID;
                }

                const string bizRuleName = "DA_PRD_SEL_CART_MOVE_EQSG_PC";
                string[] arrColumn = { "LANGID", "SHOPID", "PROCID", "AREAID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, Util.NVC(cboProcess.SelectedValue), EqsgCode };
                string selectedValueText = cboLine.SelectedValuePath;
                string displayMemberText = cboLine.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cboLine, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, null);

                if (cboLine.Items.Count > 1 || cboLine.Items.Count == 0)
                {
                    DataTable dt = DataTableConverter.Convert(cboLine.ItemsSource);
                    DataRow dr = dt.NewRow();
                    dr[cboLine.SelectedValuePath] = "SELECT";
                    dr[cboLine.DisplayMemberPath] = "- SELECT -";
                    dt.Rows.InsertAt(dr, 0);

                    cboLine.ItemsSource = null;
                    cboLine.ItemsSource = dt.Copy().AsDataView();

                    int index = 0;
                    cboLine.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetInspectorCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_INSPECTOR_PC";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, _eqptID };
            string selectedValueText = cboTakeOverUser.SelectedValuePath;
            string displayMemberText = cboTakeOverUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboTakeOverUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SetBoxingWorkerCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_BOXING_WORKER_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID };
            string selectedValueText = cboTakeOverUser.SelectedValuePath;
            string displayMemberText = cboTakeOverUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboTakeOverUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SetShipToPopControl()
        {
            if (dgCart.Rows.Count > 0)
            {
                string ProdID = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "PRODID"));

                const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
                string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
                string[] arrCondition = { LoginInfo.CFG_SHOP_ID, ProdID, LoginInfo.LANGID };
                CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
            }
        }

        /// <summary>
        /// Cart List
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LOT List
        /// </summary>
        private void SetGridAssyLotList()
        {
            try
            {
                string bizRuleName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                if (QueryCall)
                {
                    // 공정진행이 아닌 조회에서 Calld인 경우 공정을 넣어준다
                    newRow["PROCID"] = _procID;
                }

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT_CART_LOAD", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtResult.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                }
                Util.GridSetData(dgAssyLot, dtResult, null, true);

                SetDataGridColumnVisibility();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 조립LOT 완성 Inbox List
        /// </summary>
        private void SetGridProductInboxList()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("LOTSTAT", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                if (QueryCall)
                {
                    // 공정진행이 아닌 조회에서 Calld인 경우 공정을 넣어준다
                    newRow["PROCID"] = _procID;
                }

                inTable.Rows.Add(newRow);

                _inboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_CART_LOAD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionInbox, _inboxList, FrameOperation, true);                

                if (_inboxList != null && _inboxList.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

                _inBoxHeaderType = CheckBoxHeaderType.Zero;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetGridProductInboxList(DataTable dt)
        {
            DataTable dtinbox = _inboxList.Copy();
            DataRow[] drDel = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = 0");

            foreach (DataRow rowdel in drDel)
            {
                dtinbox.Select("ASSY_LOTID = '" + Util.NVC(rowdel["ASSY_LOTID"]) + "'").ToList<DataRow>().ForEach(row => row.Delete());
            }
            dtinbox.AcceptChanges();

            Util.GridSetData(dgProductionInbox, dtinbox, FrameOperation, true);
        }


        /// <summary>
        /// 대차 이동 => 사용 안함... 
        /// </summary>
        private void CartMove()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_ROUTID", typeof(string));
                inTable.Columns.Add("TO_FLOWID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("MOVE_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _eqptID;
                newRow["TO_PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["TO_ROUTID"] = dt.Rows[cboProcess.SelectedIndex]["ROUTID_TO"].ToString();
                newRow["TO_FLOWID"] = dt.Rows[cboProcess.SelectedIndex]["FLOWID_TO"].ToString();
                newRow["TO_EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["MOVE_USERID"] = cboTakeOverUser.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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
        /// 출하처 변경
        /// </summary>
        private void ShiptoUpdate()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drUpdate in dr)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = drUpdate["LOTID"].ToString();
                    newRow["SHIPTO_ID"] = Util.NVC(popShipto.SelectedValue);
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_UPD_LOT_LOTATTR_PC", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 재조회
                        SetGridProductInboxList();
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
        /// Inbox 삭제
        /// </summary>
        private void DeleteInbox()
        {
            try
            {
                ShowLoadingIndicator();

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
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = drDel["LOTID"].ToString();
                    inLot.Rows.Add(newRow);
                }

                // BR_PRD_REG_DELETE_PALLET -> BR_PRD_REG_DELETE_INBOX
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_INBOX", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 재조회
                        SetGridAssyLotList();
                        SetGridProductInboxList();
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
        /// CCell 삭제
        /// </summary>
        private void DeleteCell()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inInbox = inDataSet.Tables.Add("ININBOX");
                inInbox.Columns.Add("FORM_INBOXID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inInbox.NewRow();
                    newRow["FORM_INBOXID"] = drDel["LOTID"].ToString();
                    inInbox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_SUBLOT_INBOX_NJ", "INDATA,ININBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 재조회
                        SetGridAssyLotList();
                        SetGridProductInboxList();
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

        private bool ChkCartStatus()
        {
            bool IsInspProc = false;

            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDTYPE"] = "FORM_INSP_PROCID";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow[] dr = dtResult.Select("CBO_CODE = '" + _procID + "'");

                    if (dr.Length > 0)
                    {
                        IsInspProc = true;
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

        /// <summary>
        /// 대차 출력 자료
        /// </summary>
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = txtCartID.Text;
                newRow["PROCID"] = Util.NVC(_procID);
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }
        #endregion

        #region [Func]

        private bool ValidationShiptoUpdate()
        {
            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrWhiteSpace(popShipto.SelectedValue.ToString()))
            {
                // 출하처를 선택하세요.
                Util.MessageValidation("SFU4096");
                return false;
            }

            return true;
        }

        private bool ValidationInboxDelete()
        {
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
            }

            return true;
        }

        private bool ValidationCellDelete()
        {
            DataRow[] drInbox = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (drInbox.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationInboxChange()
        {
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

            return true;
        }

        private bool ValidationInboxLoss()
        {
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

            if (Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_STAT_CODE")).Equals("CREATED") ||
                Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_STAT_CODE")).Equals("STORED"))
            {
            }
            else
            {
                // [%1] 해당 공정 대기와 보관 중 대차가 아닙니다.
                Util.MessageValidation("SFU4446", _procName);
                return false;
            }

            return true;
        }

        private bool ValidationCartRePrint()
        {
            if (dgCart == null || dgCart.Rows.Count == 0)
            { 
                // 재발행 대차를 선택하세요.
                Util.MessageValidation("SFU4360");
                return false;
            }

            return true;
        }

        private bool ValidationTagPrint()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgProductionInbox, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            string labelCode = dgProductionInbox.Name == "dgProductionInbox" ? "LBL0106" : "LBL0107";

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

            return true;
        }

        private bool ValidationMove()
        {
            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
            DataRow[] dr = dt.Select("CTNR_ID = '" + txtCartID.Text + "'");

            if (dr.Length == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return false;
            }

            if (Util.NVC(dr[0]["CART_SHEET_PRT_FLAG"]).Equals("N"))
            {
                // 대차 발행후 이동 처리가 가능 합니다.
                Util.MessageValidation("SFU4406");
                return false;
            }

            // 대차 라벨 발행여부 체크
            //if (string.Equals(_procID, Process.PolymerOffLineCharacteristic) ||
            //    string.Equals(_procID, Process.PolymerFinalExternalDSF) ||
            //    string.Equals(_procID, Process.PolymerFinalExternal))
            //{
            if (ChkCartStatus())
            {
                if (Util.NVC_Int(dr[0]["PROC_COUNT"]) != 0)
                {
                    // 생산중인 대차는 이동할 수 없습니다.
                    Util.MessageValidation("SFU4376");
                    return false;
                }
            }
            else
            {
                if (Util.NVC_Int(dr[0]["NO_PRINT_COUNT"]) != 0)
                {
                    // Inbox 태그를 전부 발행해야 이동 처리가 가능 합니다.
                    Util.MessageValidation("SFU4368");
                    return false;
                }
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 공정을 선택하세요.
                Util.MessageValidation("SFU4362");
                return false;
            }

            if (cboLine.SelectedValue == null || cboLine.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 라인을 선택하세요.
                Util.MessageValidation("SFU4363");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 재발행 팝업
        /// </summary>
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;
            if (Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("N"))
            {
                popupCartPrint.DefectCartYN = "Y";
            }

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _eqptID;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }

        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            SetGridCartList();

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
        /// Inbox 정보변경 팝업
        /// </summary>
        private void InboxChange()
        {
            CMM_POLYMER_FORM_INBOX_INFO_CHANGE popupInboxChange = new CMM_POLYMER_FORM_INBOX_INFO_CHANGE();
            popupInboxChange.FrameOperation = this.FrameOperation;

            popupInboxChange.ProcessCall = true;

            object[] parameters = new object[4];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = _util.GetDataGridFirstRowBycheck(dgProductionInbox, "CHK");

            C1WindowExtension.SetParameters(popupInboxChange, parameters);

            popupInboxChange.Closed += new EventHandler(popupInboxChange_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupInboxChange);
                    popupInboxChange.BringToFront();
                    break;
                }
            }
        }

        private void popupInboxChange_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_INBOX_INFO_CHANGE popup = sender as CMM_POLYMER_FORM_INBOX_INFO_CHANGE;

            SetGridCartList();

            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
                SetGridProductInboxList();
            }

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
        /// Inbox Loss처리 팝업
        /// </summary>
        private void InboxLoss()
        {
            CMM_POLYMER_FORM_CART_DEFECT_LOSS popupInboxLoss = new CMM_POLYMER_FORM_CART_DEFECT_LOSS();
            popupInboxLoss.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = txtCartID.Text;
            parameters[4] = _util.GetDataGridFirstRowBycheck(dgProductionInbox, "CHK");

            C1WindowExtension.SetParameters(popupInboxLoss, parameters);

            popupInboxLoss.Closed += new EventHandler(popupInboxLoss_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupInboxLoss);
                    popupInboxLoss.BringToFront();
                    break;
                }
            }
        }

        private void popupInboxLoss_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_FORM_CART_DEFECT_LOSS popup = sender as CMM_POLYMER_FORM_CART_DEFECT_LOSS;

            SetGridCartList();

            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
                SetGridProductInboxList();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        private void PrintLabel(DataRow drPrint)
        {
            string processName = _procName;
            string modelId = drPrint["MODLID"].GetString();
            string projectName = drPrint["PRJT_NAME"].GetString();
            string marketTypeName = drPrint["MKT_TYPE_NAME"].GetString();
            string assyLotId = drPrint["ASSY_LOTID"].GetString();
            string calDate = drPrint["CALDATE"].GetString();
            //string shiftName = drPrint["SHFT_NAME"].GetString();
            string equipmentShortName = drPrint["EQPTSHORTNAME"].GetString();
            //string inspectorId = drPrint["INSPECTORID"].GetString();

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
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 양품 Tag인 경우 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            DataRow[] drInbox = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK=1");

            foreach (DataRow row in drInbox)
            {
                DataRow dr = dtLabelItem.NewRow();

                dr["LABEL_CODE"] = "LBL0106";
                dr["ITEM001"] = Util.NVC(row["CAPA_GRD_CODE"]);
                dr["ITEM002"] = modelId + "(" + projectName + ") ";
                //dr["ITEM003"] = assyLotId;
                dr["ITEM003"] = Util.NVC(row["ASSY_LOTID"]);
                dr["ITEM004"] = Util.NVC_Int(row["CELL_QTY"]).GetString();
                dr["ITEM005"] = equipmentShortName;
                dr["ITEM006"] = calDate + "(" + Util.NVC(row["SHFT_NAME"]) + ")"; // 테스트
                dr["ITEM007"] = Util.NVC(row["INSPECTORID"]);
                dr["ITEM008"] = marketTypeName;
                dr["ITEM009"] = Util.NVC(row["LOTID"]);
                //dr["ITEM010"] = string.Empty;
                dr["ITEM010"] = Util.NVC(row["AOMM_GRD_CODE"]);
                dr["ITEM011"] = string.Empty;

                // 라벨 발행 이력 저장
                DataRow newRow = inTable.NewRow();
                newRow["LABEL_PRT_COUNT"] = 1;                       // 발행 수량
                newRow["PRT_ITEM01"] = Util.NVC(row["LOTID"]);       // Cell ID
                newRow["PRT_ITEM02"] = Util.NVC(row["WIPSEQ"]);  
                newRow["PRT_ITEM04"] = Util.NVC(row["PRINT_YN"]);    // 재발행 여부
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = Util.NVC(row["LOTID"]);
                inTable.Rows.Add(newRow);

                dtLabelItem.Rows.Add(dr);
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
                // 라벨 발행이력 저장
                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        SetGridProductInboxList();
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

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
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

        private void btnCartCellRegister_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string ctnrID = txtCartID.Text.ToString();
                //int loss_qty = Int32.Parse(Util.NVC(dr[0]["WIPQTY"]));

                CMM_ASSY_LOSS_CELL_INPUT popupoutput = new CMM_ASSY_LOSS_CELL_INPUT();

                //popupoutput.isInputQty = true;

                popupoutput.FrameOperation = this.FrameOperation;
                object[] parameters = new object[1];
                parameters[0] = ctnrID;
                //parameters[1] = loss_qty;
                C1WindowExtension.SetParameters(popupoutput, parameters);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupoutput);
                        popupoutput.BringToFront();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
    }
}