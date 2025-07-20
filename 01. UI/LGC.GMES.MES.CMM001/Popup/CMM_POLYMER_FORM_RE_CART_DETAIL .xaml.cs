/*************************************************************************************
 Created Date : 2017.12.30
      Creator : 오화백
   Decription : 활성화 대차 재공관리 - 대차 상세
--------------------------------------------------------------------------------------
 [Change History]
    
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
    public partial class CMM_POLYMER_FORM_RE_CART_DETAIL : C1Window, IWorkArea
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
        public string WorkingCheck { get; set; }  //작업중 확인
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

        public CMM_POLYMER_FORM_RE_CART_DETAIL()
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

        }
        private void SetControl()
        {
            // 작업중일 경우 버튼 숨김
            if(WorkingCheck == "Y")
            {
                btnCartRePrint.Visibility = Visibility.Collapsed;
                btnTagPrint.Visibility = Visibility.Collapsed;
                btnMove.Visibility = Visibility.Collapsed;
                btnInboxChange.Visibility = Visibility.Collapsed;
                chkMoveArea.IsEnabled = false;
                cboProcess.IsEnabled = false;
                cboLine.IsEnabled = false;
            }




            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtCartID.Text = tmps[4] as string;

            cboArea.IsEnabled = false;
            //cboTakeOverUser.IsEnabled = false;
            txtUserNameCr.IsEnabled = false;
            btnUserCr.IsEnabled = false;

            SetGridCartList();
          
            if ( dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
                SetGridProductInboxList();
            }
            if (string.Equals(_procID, Process.CELL_BOXING) || string.Equals(_procID, Process.CELL_BOXING_RETURN))
            {
                grdCartMove.Visibility = Visibility.Collapsed;
                btnMove.Visibility = Visibility.Collapsed;
            }
        }
        private void SetCombo()
        {
            SetAreaCombo();
            SetMoveProcessCombo();
            SetMoveLineCombo();
           
        }

        #endregion

        #region [대차ID KeyDown]
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

        #region 대차재발행
        private void btnCartRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            // 대차 재발행
            CartRePrint();
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
            //cboTakeOverUser.IsEnabled = true;
            txtUserNameCr.IsEnabled = true;
            btnUserCr.IsEnabled = true;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        private void chkMoveArea_Unchecked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = false;
            //cboTakeOverUser.IsEnabled = false;
            txtUserNameCr.IsEnabled = false;
            btnUserCr.IsEnabled = false;
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
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SELF_CHECK", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue ?? null;
                newRow["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "ROUTID"));

                if (cboArea.SelectedValue.ToString() == LoginInfo.CFG_AREA_ID)
                {
                    newRow["PROCID"] = _procID;
                    newRow["SELF_CHECK"] = "Y";
                }
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_MOVE_PROC_RE", "RQSTDT", "RSLTDT", inTable);

                DataRow drSelect = dtResult.NewRow();
                drSelect[cboProcess.SelectedValuePath] = "SELECT";
                drSelect[cboProcess.DisplayMemberPath] = "- SELECT -";
                dtResult.Rows.InsertAt(drSelect, 0);

                cboProcess.ItemsSource = null;
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                cboProcess.SelectedIndex = 0;

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

                CommonCombo.CommonBaseCombo(bizRuleName, cboLine, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_RE", "INDATA", "OUTDATA", inTable);
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
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT_CART_LOAD_RE", "INDATA", "OUTDATA", inTable);

                dtResult.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                Util.GridSetData(dgAssyLot, dtResult, null, true);
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
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                _inboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_CART_LOAD_RE", "INDATA", "OUTDATA", inTable);

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
        /// 대차 이동
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
                newRow["MOVE_USERID"] = txtUserNameCr.Tag == null ? string.Empty : txtUserNameCr.Tag.ToString();
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
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("PALLETID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _eqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["PALLETID"] = drDel["LOTID"].ToString();
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_PALLET", "INDATA,INLOT", null, (bizResult, bizException) =>
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

        #endregion

        #region [Func]

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

            //if (Util.NVC(dr[0]["CART_SHEET_PRT_FLAG"]).Equals("N"))
            //{
            //    // 대차 발행후 이동 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4406");
            //    return false;
            //}

            // 대차 라벨 발행여부 체크
            //if (string.Equals(_procID, Process.PolymerOffLineCharacteristic) ||
            //    string.Equals(_procID, Process.PolymerFinalExternalDSF) ||
            //    string.Equals(_procID, Process.PolymerFinalExternal))
            //{
            //    if (Util.NVC_Int(dr[0]["PROC_COUNT"]) != 0)
            //    {
            //        // 생산중인 대차는 이동할 수 없습니다.
            //        Util.MessageValidation("SFU4376");
            //        return false;
            //    }
            //}
            //else
            //{
            //    if (Util.NVC_Int(dr[0]["NO_PRINT_COUNT"]) != 0)
            //    {
            //        // Inbox 태그를 전부 발행해야 이동 처리가 가능 합니다.
            //        Util.MessageValidation("SFU4368");
            //        return false;
            //    }
            //}

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
        private void CartRePrint()
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = "1";
            popupCartPrint.CART_RE = "Y";

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
                dr["ITEM003"] = assyLotId;
                dr["ITEM004"] = Util.NVC_Int(row["CELL_QTY"]).GetString();
                dr["ITEM005"] = equipmentShortName;
                dr["ITEM006"] = row["CALDATE"].GetString() + "(" + Util.NVC(row["SHFT_NAME"]) + ")";
                dr["ITEM007"] = Util.NVC(row["INSPECTORID"]);
                dr["ITEM008"] = marketTypeName;
                dr["ITEM009"] = Util.NVC(row["LOTID"]);
                dr["ITEM010"] = string.Empty;
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

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUserNameCr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }

              
            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnInboxChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeQty()) return;

            InboxQtyChange();

        }
        private void InboxQtyChange()
        {
            CMM_POLYMER_FORM_INBOX_CELLQTY_CHANGE popupInbxoChange = new CMM_POLYMER_FORM_INBOX_CELLQTY_CHANGE();
            popupInbxoChange.FrameOperation = this.FrameOperation;
            object[] parameters = new object[2];
            parameters[0] = _procID;
            parameters[1] = GetSelectInputPalletRow();

            C1WindowExtension.SetParameters(popupInbxoChange, parameters);

            popupInbxoChange.Closed += new EventHandler(popupInbxoChange_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupInbxoChange);
                    popupInbxoChange.BringToFront();
                    break;
                }
            }
        }

        private void popupInbxoChange_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_INBOX_CELLQTY_CHANGE popup = sender as CMM_POLYMER_FORM_INBOX_CELLQTY_CHANGE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

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
        public DataRow GetSelectInputPalletRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                DataTable test = new DataTable();
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
        private bool ValidationChangeQty()
        {
           int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgProductionInbox, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }
            int CheckCount = 0;

            for (int i = 0; i < dgProductionInbox.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }
            //if (Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[rowIndex].DataItem, "TAKEOVER_YN")) == "Y" && Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[rowIndex].DataItem, "WIPSEQ_YN")) == "Y")
            //{
            //    Util.MessageValidation("SFU1871"); //"분할 또는 병합되어 수량을 수정할 수 없습니다"
            //    return false;
            //}

            //if (Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[rowIndex].DataItem, "WIPSEQ_YN")) == "Y")
            //{
            //    Util.MessageValidation("재작업을 위해 투입되어 수량을 수정할 수 없습니다."); //"분할 또는 병합되어 수량을 수정할 수 없습니다"
            //    return false;
            //}

            //if (Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[rowIndex].DataItem, "WIPQTY_YN")) == "Y")
            //{
            //    Util.MessageValidation("분할 또는 병합되어 수량을 수정할 수 없습니다."); //"분할 또는 병합되어 수량을 수정할 수 없습니다"
            //    return false;
            //}



            return true;


        }
    }
}