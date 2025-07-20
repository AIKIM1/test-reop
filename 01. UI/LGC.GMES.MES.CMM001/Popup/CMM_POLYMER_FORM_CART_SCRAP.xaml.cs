/*************************************************************************************
 Created Date : 2018.05.29
      Creator : 정문교
   Decription : 폐기 대차 등록
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
    /// CMM_POLYMER_FORM_CART_SCRAP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_SCRAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _areaID = string.Empty;
        private string _sfiftID = string.Empty;
        private string _workerID = string.Empty;
        private string _workerName = string.Empty;

        private CheckBoxHeaderType _inBoxHeaderType;
        private DataTable _cart;
        private DataTable _defectList;

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

        public CMM_POLYMER_FORM_CART_SCRAP()
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
                SetParameters();
                SetControl();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _areaID = tmps[0] as string;

            // 대차 정보
            DataRow Cart = tmps[1] as DataRow;
            _cart = Cart.Table.Clone();
            _cart.ImportRow(Cart);
            Util.GridSetData(dgCart, _cart, null, true);

            // 불량 LOT 정보
            DataRow[] DefectLot = tmps[2] as DataRow[];
            _defectList = DefectLot.CopyToDataTable<DataRow>();
            Util.GridSetData(dgDefectLot, _defectList, null, true);

            _sfiftID = tmps[3] as string;
            _workerID = tmps[4] as string;
            _workerName = tmps[5] as string;

            _inBoxHeaderType = CheckBoxHeaderType.Zero;
        }

        private void SetControl()
        {
            btnCartRePrint.IsEnabled = false;
        }

        #endregion

        #region Inbox Header CHeck
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefectLot;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
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
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void dgCart_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                }
            }));
        }

        private void dgDefectLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("PROCESS_QTY"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }
            }));
        }

        private void dgDefectLot_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if (e.Cell.Column.Name.Equals("PROCESS_QTY") && e.Cell.IsEditable == true)
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                }
            }
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

        #region [등록]
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            dgDefectLot.EndEditRow(true);

            if (!ValidationInput())
                return;
            
            // 처리 하시겠습니까?
            Util.MessageConfirm("SFU1925", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartInput();
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

        /// <summary>
        /// 등록 
        /// </summary>
        private void CartInput()
        {
            try
            {
                ShowLoadingIndicator();

                DataRow[] drSelect = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");

                // DATA SET  
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("SHFT", typeof(string));

                DataTable inScrap = inDataSet.Tables.Add("INSCRAP");
                inScrap.Columns.Add("LOTID", typeof(string));
                inScrap.Columns.Add("ACTQTY", typeof(decimal));

                DataTable inResnGR = inDataSet.Tables.Add("INRESNGR");
                inResnGR.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                inResnGR.Columns.Add("RSN_GR_QTY", typeof(decimal));
                inResnGR.Columns.Add("ASSY_LOTID", typeof(string));

                // INDATA
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = _areaID;
                newRow["EQSGID"] = Util.NVC(_cart.Rows[0]["EQSGID"]);
                newRow["PROCID"] = Util.NVC(_cart.Rows[0]["PROCID"]);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CTNR_ID"] = Util.NVC(_cart.Rows[0]["CTNR_ID"]);
                newRow["WRK_USERID"] = _workerID;
                newRow["WRK_USER_NAME"] = _workerName;
                newRow["SHFT"] = _sfiftID;
                inTable.Rows.Add(newRow);

                // INSCRAP
                foreach (DataRow dr in drSelect)
                {
                    newRow = inScrap.NewRow();
                    newRow["LOTID"] = Util.NVC(dr["INBOX_ID_DEF"]);
                    newRow["ACTQTY"] = Util.NVC_Int(dr["PROCESS_QTY"]);
                    inScrap.Rows.Add(newRow);
                }

                // INRESNGR
                var summarydata = from row in drSelect.AsEnumerable()
                                  group row by new
                                  {
                                      AssyLot = row.Field<string>("LOTID_RT"),
                                      DfctRsnGrID = row.Field<string>("DFCT_RSN_GR_ID"),
                                  } into grp
                                  select new
                                  {
                                      AssyLot = grp.Key.AssyLot,
                                      DfctRsnGrID = grp.Key.DfctRsnGrID,
                                      QtySum = grp.Sum(r => r.Field<int>("PROCESS_QTY"))
                                  };

                foreach (var row in summarydata)
                {
                    newRow = inResnGR.NewRow();
                    newRow["DFCT_RSN_GR_ID"] = row.DfctRsnGrID;
                    newRow["RSN_GR_QTY"] = row.QtySum;
                    newRow["ASSY_LOTID"] = row.AssyLot;
                    inResnGR.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SCRAP_CTNR_MCP", "INDATA,INSCRAP,INRESNGR", "OUTCTNR", (bizResult, bizException) =>
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

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        if (bizResult.Tables["OUTCTNR"].Rows.Count > 0)
                        {
                            txtCartID.Text = Util.NVC(bizResult.Tables["OUTCTNR"].Rows[0]["CTNR_ID"]);
                        }

                        btnInput.IsEnabled = false;
                        btnCartRePrint.IsEnabled = true;
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

        private bool ValidationInput()
        {
            DataRow[] drSelect = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            foreach (DataRow dr in drSelect)
            {
                if (Util.NVC(dr["LOTID_RT"]).Length < 6)
                {
                    // 조립LOT 정보는 8자리 입니다.
                    Util.MessageValidation("SFU4228");
                    return false;
                }

                if (Util.NVC_Int(dr["PROCESS_QTY"]) == 0)
                {
                    // 수량은 0보다 커야 합니다.
                    Util.MessageValidation("SFU1683");
                    return false;
                }

                if (Util.NVC_Int(dr["WIPQTY"]) < Util.NVC_Int(dr["PROCESS_QTY"]))
                {
                    // 기존 수량보다 큽니다.
                    Util.MessageValidation("SFU4486");
                    return false;
                }
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
            popupCartPrint.DefectCartYN = "Y";

            object[] parameters = new object[5];
            parameters[0] = Util.NVC(_cart.Rows[0]["PROCID"]);
            parameters[1] = null;
            parameters[2] = Util.NVC(_cart.Rows[0]["CTNR_ID"]);
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

    }
}