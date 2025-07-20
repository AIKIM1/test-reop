/*************************************************************************************
 Created Date : 2017.07.04
      Creator : 이슬아
   Decription : 포장 공정실적 관리(초소형)
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.04  이슬아 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Globalization;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_103.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_103 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_103()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_AREA_ID }, sCase: "LINE_CP");
           // _combo.SetCombo(cboBoxStat, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "BOXSTAT" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }

        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
         private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;    
        }
        #endregion

        #region [Button]
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtShift.Text))
            {
                // SFU1845 작업조를 입력하세요.
                Util.MessageValidation("SFU1845");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                // SFU1843 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            BOX001_103_RUNSTART popup = new BOX001.BOX001_103_RUNSTART();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[13];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = string.Empty; //boxid
                Parameters[2] = string.Empty; //작업구분
                Parameters[3] = string.Empty; //inbox종류
                Parameters[4] = string.Empty; //모델lot
                Parameters[5] = string.Empty; //prjt
                Parameters[6] = string.Empty; //prodid
                Parameters[7] = txtShift.Tag; // 작업조id
                Parameters[8] = txtShift.Text; // 작업조name
                Parameters[9] = txtWorker.Tag; // 작업자id
                Parameters[10] = txtWorker.Text; // 작업자name
                Parameters[11] = txtWorkGroup.Text; // 작업그룹명
                Parameters[12] = txtWorkGroup.Tag; // 작업그룹ID

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puRunStart_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }

        }
        private void puRunStart_Closed(object sender, EventArgs e)
        {
            BOX001_103_RUNSTART popup = sender as BOX001_103_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetInPallet();

                int row = _util.GetDataGridRowIndex(dgInPallet,"BOXID", popup.BOXID);
                DataTableConverter.SetValue(dgInPallet.Rows[row].DataItem, "CHK", true);
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelInPallet();
        }

        /// <summary>
        /// 실적확정
        /// Biz : BR_PRD_REG_END_INPALLET_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    // SFU1845 작업조를 입력하세요.
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sBoxID = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXID"].Index).Value);
                int sQty = Util.NVC_Int(dgInPallet.GetCell(row, dgInPallet.Columns["TOTAL_QTY"].Index).Value);

                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("LANGID");
                inBoxTable.Columns.Add("INPALLETID");
                inBoxTable.Columns.Add("TOTAL_QTY");
                inBoxTable.Columns.Add("USERID");

                newRow = inBoxTable.NewRow();
                newRow["INPALLETID"] = sBoxID;
                newRow["TOTAL_QTY"] = sQty;
                newRow["USERID"] = txtWorker.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPALLET_FM", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetInPallet();
                        DataTableConverter.SetValue(dgInPallet.Rows[row].DataItem, "CHK", true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 실적확정 취소
        /// Biz : BR_PRD_REG_CANCEL_END_INPALLET_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    // SFU1845 작업조를 입력하세요.
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sBoxID = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXID"].Index).Value);

                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("LANGID");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("USERID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = sBoxID;
                newRow["USERID"] = txtWorker.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_END_INPALLET_FM", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetInPallet();

                        DataTableConverter.SetValue(dgInPallet.Rows[row].DataItem, "CHK", true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 작업일지 발행
        /// Biz : BR_PRD_GET_INPALLET_LABEL_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    // SFU1845 작업조를 입력하세요.
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sBoxID = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXID"].Index).Value);
                string sBoxStat = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXSTAT"].Index).Value);

                if (sBoxStat != "PACKED")
                {
                    Util.MessageValidation("1134", new string[] { sBoxID });
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LANGID", typeof(string));
                inBox.Columns.Add("BOXID", typeof(string));

                DataRow dr = inBox.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBoxID;
                inBox.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INPALLET_LABEL_FM", "INBOX", "OUTBOX,OUTLOT", (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    // Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    BOX001_101_REPORT _print = new BOX001_101_REPORT();
                    _print.FrameOperation = this.FrameOperation;

                    if (_print != null)
                    {
                        // SET PARAMETER
                        object[] Parameters = new object[2];
                        Parameters[0] = result;
                        Parameters[1] = txtWorker.Text;

                        C1WindowExtension.SetParameters(_print, Parameters);

                        _print.ShowModal();

                    }

                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearMtrlPalleGrid();
            GetInPallet();
        }

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        #endregion

        #region [투입 Pallet]
        private void txtMPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                GetMPallet();
        }

        private void btnMPallet_Click(object sender, RoutedEventArgs e)
        {
            GetMPallet();
        }

        private void dgMPalletChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgMPallet.SelectedIndex = idx;
                }
                GetPKGLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        #endregion

        #region [작업 Pallet]
        private void dgInPalletChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgInPallet.SelectedIndex = idx;
                }

                GetInBox();                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }
        #endregion

        #region [투입 LOT]
        private void dgPKGLot_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
        }
        private void dgPKGLot_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "CHK")
            {
                dgPKGLot.Columns["PACKQTY"].IsReadOnly = Util.NVC(DataTableConverter.GetValue(dgPKGLot.Rows[e.Cell.Row.Index].DataItem, "CHK")) != bool.TrueString ;
            }

            if (e.Cell.Column.Name == "PACKQTY")
            {
                SetPackQty(e.Cell.Row.Index);
            }
        }
        private void btnAdd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
           // int iRow = _util.GetDataGridCheckFirstRowIndex(dgPKGLot, "CHK");
            DataRow dr = Util.gridGetChecked(ref dgPKGLot, "CHK").FirstOrDefault();

            if (dgPKGLot.GetRowCount() <= 0 || dr == null)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                e.Handled = true;
                return;
            }

            DataTable dtInfo = DataTableConverter.Convert(dgInBox.ItemsSource);
            DataRow drInfo = dtInfo.Select("MTRL_LOTID = '" + dr["MTRL_LOTID"] + "' AND PKG_LOTID = '" + dr["PKG_LOTID"] + "' AND PRDT_GRD_CODE = '" + dr["PRDT_GRD_CODE"] + "'").FirstOrDefault();

            if (drInfo != null)
            {
                //10017	입력하려는 값이 이미 존재합니다.
                Util.MessageValidation("10017");
                e.Handled = true;
                return;
            }

            if (dgPKGLot.CurrentRow?.Index >= 0) SetPackQty(dgPKGLot.CurrentRow.Index);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

            if (row < 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            SetInbox();     
        }

        #endregion

        #region [포장 LOT]    
        private void dgInBoxChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgInBox.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    // SFU1845 작업조를 입력하세요.
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int iRow = _util.GetDataGridCheckFirstRowIndex(dgInBox, "CHK");                

                if (iRow < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sInPalletID = Util.NVC(DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "INPALLETID"));

                DeleteInbox(iRow);          

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetInbox();
            GetInBox();
        }
        #endregion


        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_PROC_INPALLET_FM
        /// </summary>
        private void GetInPallet()
        {
            try
            {
                string sBoxStat = string.Empty;

                if (chkPacking.IsChecked.HasValue && (bool)chkPacking.IsChecked)
                    sBoxStat = "PACKING";

                if (chkPacked.IsChecked.HasValue && (bool)chkPacked.IsChecked)
                    sBoxStat = string.IsNullOrWhiteSpace(sBoxStat) ? "PACKED" : null;

                if (sBoxStat == string.Empty)
                {
                    //
                    Util.MessageValidation("포장상태를 선택해주세요.");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXSTAT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.CELL_BOXING;

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    dr["BOXID"] = txtPalletID.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["PKG_LOTID"] = txtLotID.Text;
                }
                else
                {
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                    dr["BOXSTAT"] = sBoxStat;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROC_INPALLET_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgInPallet, dtResult, FrameOperation, true);

                ClearInBoxGrid();

                if (dgInPallet.Rows.Count > 0)
                {                    
                    DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_MTRL_PALLET_FM
        /// </summary>
        private void GetMPallet(string prodId = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMPalletID.Text) && string.IsNullOrWhiteSpace(prodId))
                {
                    // SFU1411 PALLETID를 입력해주세요.
                    Util.MessageValidation("SFU1411");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MTRL_LOTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRL_LOTID"] = txtMPalletID.Text;
                dr["PRODID"] = prodId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MTRL_PALLET_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgMPallet, dtResult, FrameOperation, true);
                ClearPGKLotGrid();

                if (dgMPallet.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgMPallet.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgMPallet.Columns["WIPQTY_IN"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_MTRL_PKT_LOT__FM
        /// </summary>
        private void GetPKGLot()
        {
            try
            {
                int row = _util.GetDataGridCheckFirstRowIndex(dgMPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sLotID = Util.NVC(dgMPallet.GetCell(row, dgMPallet.Columns["MTRL_LOTID"].Index).Value);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MTRL_LOTID", typeof(string));
                RQSTDT.TableName = "INDATA";

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRL_LOTID"] = sLotID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MTRL_PKG_LOT_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("QTY"))
                {
                    DataColumn dc = new DataColumn("QTY");
                    dc.DefaultValue = 0;
                    dtResult.Columns.Add(dc);
                }
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                if (dtResult.Rows.Count > 0)
                    dtResult.Rows[0]["CHK"] = 1;

                Util.GridSetData(dgPKGLot, dtResult, FrameOperation);

                if (dgPKGLot.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPKGLot.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPKGLot.Columns["PACKQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPKGLot.Columns["QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_INBOX_BY_INPALLET_FM
        /// </summary>
        private void GetInBox()
        {
            try
            {
                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sBoxID = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["BOXID"].Index).Value);
                string sProdID = Util.NVC(dgInPallet.GetCell(row, dgInPallet.Columns["PRODID"].Index).Value);


                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("INPALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPALLETID"] = sBoxID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INBOX_BY_INPALLET_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("YN"))
                {
                    DataColumn dc = new DataColumn("YN");
                    dc.DefaultValue = "N";
                    dtResult.Columns.Add(dc);
                }
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgInBox, dtResult, FrameOperation);

                if (dgInBox.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgInBox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

                GetMPallet(sProdID);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }
        /// <summary>
        /// 작업취소
        /// BIZ : BR_PRD_CANCEL_INPALLET_FM
        /// </summary>
        private void CancelInPallet()
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    // SFU1845 작업조를 입력하세요.
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgInPallet.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();

                if (dr == null)
                { 
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("USERID");
                inBoxTable.Columns.Add("LANGID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = dr["BOXID"];
                newRow["USERID"] = txtWorker.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_INPALLET_FM", "INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetInPallet();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                },indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 포장LOT 삭제
        /// BIZ : BR_PRD_REG_UNPACKING_INBOX_FM
        /// </summary>
        private void DeleteInbox(int iRow)
        {
            try
            {
                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }           

                string sInPalletID = Util.NVC(DataTableConverter.GetValue(dgInPallet.Rows[row].DataItem, "BOXID"));
                string sProdID = Util.NVC(DataTableConverter.GetValue(dgInPallet.Rows[row].DataItem, "PRODID"));
                string sMPallet = string.Empty;

                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("LANGID");
                inBoxTable.Columns.Add("INPALLETID");
                inBoxTable.Columns.Add("MTRL_LOTID");
                inBoxTable.Columns.Add("INBOXID");
                inBoxTable.Columns.Add("PKG_LOTID");
                inBoxTable.Columns.Add("PRDT_GRD_CODE");
                inBoxTable.Columns.Add("TOTAL_QTY");
                inBoxTable.Columns.Add("USERID");

                newRow = inBoxTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["INPALLETID"] = DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "INPALLETID");
                newRow["MTRL_LOTID"] = sMPallet = Util.NVC(DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "MTRL_LOTID"));
                newRow["INBOXID"] = DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "INBOXID");
                newRow["PKG_LOTID"] = DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "PKG_LOTID");
                newRow["PRDT_GRD_CODE"] = DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "PRDT_GRD_CODE");
                newRow["TOTAL_QTY"] = DataTableConverter.GetValue(dgInBox.Rows[iRow].DataItem, "TOTAL_QTY");
                newRow["USERID"] = txtWorker.Tag;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACKING_INBOX_FM", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        GetInPallet(); //sInPalletID
                        DataTableConverter.SetValue(dgInPallet.Rows[row].DataItem, "CHK", true);

                        //txtMPalletID.Text = sMPallet;
                        GetMPallet(sProdID);
                       // GetInBox();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 포장LOT 저장
        /// BIZ : BR_PRD_REG_PACKING_INBOX_FM
        /// </summary>
        private void SetInbox()
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    // SFU1845 작업조를 입력하세요.
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int row = _util.GetDataGridCheckFirstRowIndex(dgInPallet, "CHK");

                if (row < 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgPKGLot.ItemsSource);

                List<DataRow> drInfo = dtInfo?.Select("CHK = 'True'").ToList();

                if (drInfo == null || drInfo.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sInPalletID = Util.NVC(DataTableConverter.GetValue(dgInPallet.Rows[row].DataItem, "BOXID"));

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("INPALLETID");
                inDataTable.Columns.Add("TOTAL_QTY");
                inDataTable.Columns.Add("USERID");

                DataRow newRow = inDataTable.NewRow();
                newRow["INPALLETID"] = sInPalletID;
                newRow["TOTAL_QTY"] = Util.NVC_Int(dtInfo.Compute("SUM(PACKQTY)", ""));
                newRow["USERID"] = txtWorker.Tag;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("MTRL_LOTID");
                inBoxTable.Columns.Add("PKG_LOTID");
                inBoxTable.Columns.Add("PRDT_GRD_CODE");
                inBoxTable.Columns.Add("PRODID");
                inBoxTable.Columns.Add("TOTAL_QTY");

                foreach (DataRow dr in drInfo)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["MTRL_LOTID"] = dr["MTRL_LOTID"];
                    newRow["PKG_LOTID"] = dr["PKG_LOTID"];
                    newRow["PRDT_GRD_CODE"] = dr["PRDT_GRD_CODE"];
                    newRow["PRODID"] = dr["PRODID"];
                    newRow["TOTAL_QTY"] = dr["PACKQTY"];
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PACKING_INBOX_FM", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        GetInPallet();
                      //  _util.SetDataGridCheck(dgInPallet, "CHK", "BOXID", sInPalletID);
                        DataTableConverter.SetValue(dgInPallet.Rows[row].DataItem, "CHK", true);

                        //txtMPalletID.Text = (string)drInfo[0]["MTRL_LOTID"];
                        GetInBox();                       
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [Validation]

        #endregion

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER3 wndPopup = new CMM_SHIFT_USER3();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
              //  Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER3 wndPopup = sender as CMM_SHIFT_USER3;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
                txtWorkGroup.Text = Util.NVC(wndPopup.WORKGROUIDNAME);
                txtWorkGroup.Tag = Util.NVC(wndPopup.WORKGROUID);
            }
            this.grdMain.Children.Remove(wndPopup);
        }        
    
        #endregion

        #region [Func]
      
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRunStart);
            listAuth.Add(btnRunCancel);
            listAuth.Add(btnConfirm);            

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetPackQty(int iRow)
        {
            int iWipQty = Util.NVC_Int(DataTableConverter.GetValue(dgPKGLot.Rows[iRow].DataItem, "WIPQTY"));
            int iPackQty = Util.NVC_Int(DataTableConverter.GetValue(dgPKGLot.Rows[iRow].DataItem, "PACKQTY"));

            if (iWipQty < iPackQty)
            {
                DataTableConverter.SetValue(dgPKGLot.Rows[iRow].DataItem, "PACKQTY", iWipQty);
                DataTableConverter.SetValue(dgPKGLot.Rows[iRow].DataItem, "QTY", 0);
                return;
            }
            DataTableConverter.SetValue(dgPKGLot.Rows[iRow].DataItem, "QTY", iWipQty - iPackQty);
            // dgPKGLot.Columns["CHK"].IsReadOnly = true;
        }

        private void ClearInBoxGrid()
        {
            Util.gridClear(dgInBox);
        }

        private void ClearPGKLotGrid()
        {
            Util.gridClear(dgPKGLot);
          //  dgPKGLot.Columns["CHK"].IsReadOnly = false;
            dgPKGLot.Columns["PACKQTY"].IsReadOnly = true;
        }

        private void ClearMtrlPalleGrid()
        {
            Util.gridClear(dgMPallet);
            ClearPGKLotGrid();
        }

        #endregion

        #endregion    
    }
}
