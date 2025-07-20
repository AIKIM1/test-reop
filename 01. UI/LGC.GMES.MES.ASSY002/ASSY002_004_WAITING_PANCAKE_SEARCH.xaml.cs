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
using System.Windows.Shapes;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Data;

namespace LGC.GMES.MES.ASSY002
{
    /// <summary>
    /// ASSY002_004_WAITING_PANCAKE_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY002_004_WAITING_PANCAKE_SEARCH : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private DataTable selectDt = null;
        public DataRow DrSelectedDataRow = null;
        private string woID = string.Empty;
        private string lineID = string.Empty;
        private string procID = string.Empty;
        private string electordeType = string.Empty;
        private string limitCount = string.Empty;
        private string lotID = string.Empty;
        private string sFlag = string.Empty;
        private string eqptID = string.Empty;
        private string prodID = string.Empty;

        public string ELECTRODETYPE
        {
            get { return electordeType; }
        }

        public DataTable SELECTHALFPRODUCT
        {
            get { return selectDt; }
        }

        public DataRow SELECTEDROW
        {
            get { return DrSelectedDataRow; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion
        
        public ASSY002_004_WAITING_PANCAKE_SEARCH()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            const string gubun = "CS"; // 초소형, 원각 : CR

            if (string.Equals(procID, Process.WINDING))
            {
                String[] sFilter1 = { "ELEC_TYPE" };
                _combo.SetCombo(cboElectordeType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

                String[] sFilter2 = { LoginInfo.CFG_AREA_ID, gubun, procID };
                _combo.SetCombo(cboEquipmentSegmentAssy, CommonCombo.ComboStatus.NONE, sFilter: sFilter2);
                cboEquipmentSegmentAssy.SelectedValue = lineID;
            }
        }

        #region Event

        #region [Form Load]
        private void ASSY002_004_WAITING_PANCAKE_SEARCH_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            woID = tmps[0] as string;
            lineID = tmps[1] as string;
            procID = tmps[2] as string;
            electordeType = tmps[3] as string;
            limitCount = tmps[4] as string;
            lotID = tmps[5] as string;
            sFlag = tmps[6] as string;
            eqptID = tmps[7] as string;
            prodID = tmps[8] as string;

            // INIT COMBO
            InitCombo();

            if (!string.IsNullOrEmpty(electordeType))
            {
                for (int i = 0; i < cboElectordeType.Items.Count; i++)
                {
                    if (string.Equals(electordeType, ((DataRowView)cboElectordeType.Items[i]).Row.ItemArray[0].ToString()))
                    {
                        cboElectordeType.SelectedIndex = i;
                        cboElectordeType.IsEnabled = false;
                        break;
                    }
                }
            }

            // 메뉴에서 호출하는 경우는 그리드의 라디오 버튼과 하단의 선택 버튼은 보여주지 않는다.
            if (string.Equals(sFlag, "A"))
            {
                rdoChk.Visibility = Visibility.Hidden;
                btnSelect.Visibility = Visibility.Hidden;
            }

            if (string.Equals(sFlag, "S"))
            {
                cboElectordeType.IsEnabled = true;
                rdoChk.Visibility = Visibility.Collapsed;
                btnSelect.Visibility = Visibility.Collapsed;
            }

            if (string.Equals(electordeType, "JR"))
            {
                lblGubun.Visibility = Visibility.Collapsed;
                cboElectordeType.Visibility = Visibility.Collapsed;
            }

            // INIT TEXTBOX
            if (!string.IsNullOrEmpty(lotID))
            {
                txtLotId.Text = lotID;
                txtLotId.Focus();
                txtLotId.SelectAll();
            }

            // SET DATA
            GetHalfProductList();

            this.BringToFront();
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtLotId.Text.Length != 0 && txtLotId.Text.Length < 3)
            {
                Util.AlertInfo("LOTID는 3자리 이상 입력하세요.");
                return;
            }
            // SEARCH
            GetHalfProductList();
        }

        //private void cboElectordeType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    CommonCombo _combo = new CommonCombo();

        //    string cboElectordeTypeValue = cboElectordeType.SelectedValue.ToString();

        //    if (string.Equals(cboElectordeTypeValue, "C"))
        //    {
        //        String[] sFilter1 = { "C", woID };
        //        _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "WOINPUTPRODUCT");
        //    }
        //    else if (string.Equals(cboElectordeTypeValue, "SELECT"))
        //    {
        //        String[] sFilter1 = { "SELECT", woID };
        //        _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "WOINPUTPRODUCT");
        //    }
        //    else
        //    {
        //        String[] sFilter1 = { "A", woID };
        //        _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "WOINPUTPRODUCT");
        //    }
        //}

        #region [선택]

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (gdInputProduct.Rows.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (selectDt != null)
            {
                for (int i = 0; i < selectDt.Rows.Count; i++)
                {
                    if (string.Equals(selectDt.Rows[i]["CHK"].ToString(), "True"))
                    {
                        selectDt.Rows[i]["CHK"] = 1;
                    }
                    else
                    {
                        selectDt.Rows[i]["CHK"] = 0;
                    }
                }
            }

            DataRow[] dr = Util.gridGetChecked(ref gdInputProduct, "CHK");

            if (dr.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (dr == null || dr.Length < 1)
                DrSelectedDataRow = null;
            else
                DrSelectedDataRow = dr[0];

            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region [닫기]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void GetHalfProductList()
        {
            // SELECT HALF PRODUCT
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(gdInputProduct);

                string sBizName = string.Empty;

                if (string.Equals(procID, Process.WINDING) && chkWoMaterial.IsChecked == true)
                    sBizName = "DA_PRD_SEL_HALFPROD_LIST_WO_WN";    // W/O 체크
                else
                    sBizName = "DA_PRD_SEL_HALFPROD_LIST_WN";       // W/O 체크해제

                //DataTable IndataTable = _bizRule.GetDA_PRD_SEL_HALFPROD_LIST_WN();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("ELECTRODETYPE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;
                inDataRow["PROCID"] = Util.NVC(procID);
                if (string.IsNullOrEmpty(prodID))
                    inDataRow["PRODID"] = null;
                else
                    inDataRow["PRODID"] = Util.NVC(prodID);

                if (cboElectordeType.SelectedIndex > 0)
                    inDataRow["ELECTRODETYPE"] = Util.NVC(cboElectordeType.SelectedValue);

                inDataRow["LOTID"] = Util.NVC(txtLotId.Text.Trim());
                inDataRow["WOID"] = Util.NVC(woID);
                inDataRow["EQPTID"] = Util.NVC(eqptID);

                inDataTable.Rows.Add(inDataRow);

                //DataSet ds = new DataSet();
                //inDataTable.TableName = "RQSTDT";
                //ds.Tables.Add(inDataTable);
                //string sTestXml = ds.GetXml();

                //selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", inDataTable);
                selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);

                if (selectDt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1901");
                    return;
                }
                
                Util.GridSetData(gdInputProduct, selectDt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
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

        private bool GetCheckDupplicateValue(C1DataGrid targetGrid, string sLotID)
        {
            for (int i = 0; i < targetGrid.Rows.Count; i++)
            {
                if (string.Equals(targetGrid[i, 1].Text, sLotID))
                    return true;
            }
            return false;
        }

        private void GridDataBinding(C1DataGrid dataGrid, List<String> bindValues, bool isNewFlag, bool isInput)
        {
            if (dataGrid.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                    colDt.Columns.Add(dataGrid.Columns[i].Name);

                dataGrid.ItemsSource = DataTableConverter.Convert(colDt);
            }

            DataTable inputDt = ((DataView)dataGrid.ItemsSource).Table;
            DataRow inputRow = inputDt.NewRow();

            for (int i = 0; i < inputDt.Columns.Count; i++)
                inputRow[inputDt.Columns[i].Caption] = bindValues[i];

            // ADD DATA
            inputDt.Rows.Add(inputRow);

            if (isNewFlag)
            {
                if (isInput)
                {
                    dataGrid.ItemsSource = DataTableConverter.Convert(inputDt);
                }
                else
                {
                    // 취소시 원래 순서에 맞게 Sorting
                    DataView inputVm = inputDt.DefaultView;
                    inputVm.Sort = "ROW_SEQ";

                    dataGrid.ItemsSource = DataTableConverter.Convert(inputVm.ToTable());

                    dataGrid.SelectedIndex = Util.StringToInt(inputRow["ROW_SEQ"].ToString()) - 1;
                    try
                    {
                        dataGrid.ScrollIntoView(dataGrid.SelectedIndex, 0);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }

            }
        }

        #endregion

        #endregion

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
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
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        gdInputProduct.SelectedIndex = idx;

                        selectDt = dtRow.Table;
                    }
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

        private void gdInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowIndex = 0;
                int colIndex = 0;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null && gdInputProduct.SelectedIndex >= 0)
                {
                    rowIndex = e.Cell.Row.Index;
                    colIndex = e.Cell.Column.Index;

                    int selectedIdx = gdInputProduct.SelectedIndex;

                    for (int row = 0; row < selectDt.Rows.Count; row++)
                    {
                        if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "True"))
                        {
                            if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                            {
                                gdInputProduct.SelectedIndex = row;
                            }
                        }
                    }
                }
            }));
        }

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string inputLotId = txtLotId.Text.Trim();

                if (inputLotId.Length < 3)
                {
                    Util.AlertInfo("LOTID는 3자리 이상 입력하세요.");
                    return;
                }
                // SEARCH
                GetHalfProductList();
            }
        }

        private void gdInputProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int selectedIdx = gdInputProduct.SelectedIndex;

                for (int row = 0; row < selectDt.Rows.Count; row++)
                {
                    if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "True"))
                    {
                        if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                        {
                            gdInputProduct.SelectedIndex = row;
                        }
                    }
                }
            }));
        }
    }
}
