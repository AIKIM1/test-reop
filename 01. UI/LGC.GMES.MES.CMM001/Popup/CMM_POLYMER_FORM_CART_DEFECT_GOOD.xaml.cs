/*************************************************************************************
 Created Date : 2018.05.29
      Creator : 정문교
   Decription : 양품화 등록
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
    public partial class CMM_POLYMER_FORM_CART_DEFECT_GOOD : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _areaID = string.Empty;

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

        public CMM_POLYMER_FORM_CART_DEFECT_GOOD()
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
            //Util.GridSetData(dgDefectLot, _defectList, null, true);

            DefectLotList();

            _inBoxHeaderType = CheckBoxHeaderType.Zero;
        }

        private void SetControl()
        {
            dtpDate.SelectedDateTime = GetComSelCalDate();
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

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = _areaID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

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
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("POSTDATE", typeof(string));

                DataTable inRESL = inDataSet.Tables.Add("INRESL");
                inRESL.Columns.Add("LOTID", typeof(string));
                inRESL.Columns.Add("RESNCODE", typeof(string));
                inRESL.Columns.Add("ACTQTY", typeof(decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = _areaID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["ACT_USERID"] = LoginInfo.USERID;
                newRow["CTNR_ID"] = Util.NVC(_cart.Rows[0]["CTNR_ID"]);
                newRow["POSTDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                inTable.Rows.Add(newRow);

                foreach (DataRow dr in drSelect)
                {
                    newRow = inRESL.NewRow();
                    newRow["LOTID"] = Util.NVC(dr["INBOX_ID_DEF"]);
                    newRow["RESNCODE"] = Util.NVC(dr["RESNCODE"]);
                    newRow["ACTQTY"] = Util.NVC_Int(dr["PROCESS_QTY"]);
                    inRESL.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RESEL_FOR_DFCT_MCP", "INDATA,INRESL", null, (bizResult, bizException) =>
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
        /// 불량 LOT 정보
        /// </summary>
        private void DefectLotList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CTNR_ID"] = Util.NVC(_cart.Rows[0]["CTNR_ID"]);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORMATION_DEFECT_CART_LOT_RESNCODE", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {

                    for(int i=0; i< _defectList.Rows.Count; i++)
                    {
                        for(int j=0; j< dtRslt.Rows.Count; j++)
                        {
                            if(_defectList.Rows[i]["INBOX_ID_DEF"].ToString() == dtRslt.Rows[j]["INBOX_ID_DEF"].ToString())
                            {
                                dtRslt.Rows[j]["CHK"] = _defectList.Rows[i]["CHK"];
                            }
                        }
                    }
                    Util.GridSetData(dgDefectLot, dtRslt, null, true);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [Func]

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