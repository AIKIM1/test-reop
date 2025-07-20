/*************************************************************************************
 Created Date : 2016.11.21
      Creator : JEONG JONGWON
   Decription : 반제품 투입 팝업
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY002
{
    /// <summary>
    /// ASSY002_001_INPUT_PRODUCT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY002_001_INPUT_PRODUCT : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private DataTable selectDt = null;
        private string woID = string.Empty;
        private string lineID = string.Empty;
        private string procID = string.Empty;
        private string electordeType = string.Empty;
        private string limitCount = string.Empty;
        private string lotID = string.Empty;

        public string ELECTRODETYPE
        {
            get { return electordeType; }
        }

        public DataTable SELECTHALFPRODUCT
        {
            get { return selectDt; }
        }
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public ASSY002_001_INPUT_PRODUCT()
        {
            InitializeComponent();
        }
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            if (string.Equals(procID, Process.WINDING))
            {
                String[] sFilter = { "ELEC_TYPE" };
                C1ComboBox[] cboProductChild = { cboProduct };
                _combo.SetCombo(cboElectordeType, CommonCombo.ComboStatus.SELECT, cbChild: cboProductChild, sFilter: sFilter, sCase: "COMMCODE");

                //String[] sFilter2 = { woID };
                //C1ComboBox[] cboProductParent = { cboElectordeType };
                //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, sFilter: sFilter2, sCase: "WOINPUTPRODUCT");

                String[] sFilter2 = { electordeType, woID };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "WOINPUTPRODUCT");
            }
            else if (string.Equals(procID, Process.ASSEMBLY))
            {
                String[] sFilter = { electordeType, woID };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "WOINPUTPRODUCT");
            }
        }
        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_001_INPUT_PRODUCT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            woID = tmps[0] as string;
            lineID = tmps[1] as string;
            procID = tmps[2] as string;
            electordeType = tmps[3] as string;
            limitCount = tmps[4] as string;
            lotID = tmps[5] as string;

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

            if (string.Equals(electordeType, "JR"))
            {
                ////cboElectordeType.IsEnabled = false;
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

            // GRID VISIBILITY
            if (string.Equals(procID, Process.WINDING))
            {
                ////gdInputProduct.Columns[2].Visibility = Visibility.Collapsed;
                ////gdSelectProduct.Columns[2].Visibility = Visibility.Collapsed;

                gdInputProduct.Columns["ELECTRODECODE"].Visibility = Visibility.Collapsed;
                gdSelectProduct.Columns["ELECTRODECODE"].Visibility = Visibility.Collapsed;

            }
            else if (string.Equals(procID, Process.ASSEMBLY))
            {
                ////gdInputProduct.Columns[4].Visibility = Visibility.Collapsed;
                ////gdSelectProduct.Columns[4].Visibility = Visibility.Collapsed;

                gdInputProduct.Columns["PRODID"].Visibility = Visibility.Collapsed;
                gdSelectProduct.Columns["PRODID"].Visibility = Visibility.Collapsed;
            }

            // SET DATETIME (일주일간격)
            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = System.DateTime.Now;

            // SET DATA
            GetHalfProductList();

            this.BringToFront();
        }
        #endregion

        #region [DataGrid] - gdInputProduct_Click, gdSelectProduct_Click
        private void gdInputProduct_Click(object sender, RoutedEventArgs e)
        {
            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            for (int i = 0; i < gdInputProduct.Rows.Count; i++)
            {
                if (gdInputProduct.GetCell(i, 0).Presenter != null)
                    (gdInputProduct.GetCell(i, 0).Presenter.Content as CheckBox).IsChecked = false;
                else
                    DataTableConverter.SetValue(gdInputProduct.Rows[i].DataItem, "CHK", false);
            }
            
            // VALIDATION
            if (Convert.ToInt32(limitCount) == gdSelectProduct.GetRowCount())
            {
                //Util.AlertInfo("투입 할 반제품을 더 이상 추가할수 없습니다.");
                Util.MessageValidation("SFU1962");
                return;
            }

            if (GetCheckDupplicateValue(gdSelectProduct, gdInputProduct[rowIndex, 1].Text) == false)
            {
                // SET VALUE
                List<string> bindValue = new List<string>();

                for (int i = 0; i < gdInputProduct.Columns.Count; i++)
                    bindValue.Add(gdInputProduct[rowIndex, i].Text);

                GridDataBinding(gdSelectProduct, bindValue, true, true);
            }

            // REMOVE ROW
            DataTable dt = DataTableConverter.Convert(gdInputProduct.ItemsSource);
            dt.Rows[rowIndex].Delete();
            gdInputProduct.ItemsSource = DataTableConverter.Convert(dt);

            // SET TARGET CHECKED
            //DataTableConverter.SetValue(gdSelectProduct.Rows[gdSelectProduct.Rows.Count - 1].DataItem, "CHK", true);
        }

        private void gdSelectProduct_Click(object sender, RoutedEventArgs e)
        {
            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            for (int i = 0; i < gdSelectProduct.Rows.Count; i++)
            {
                if (gdSelectProduct.GetCell(i, 0).Presenter != null)
                    (gdSelectProduct.GetCell(i, 0).Presenter.Content as CheckBox).IsChecked = true;
            }

            if (GetCheckDupplicateValue(gdInputProduct, gdSelectProduct[rowIndex, 1].Text) == false)
            {
                // SET VALUE
                List<string> bindValue = new List<string>();

                for (int i = 0; i < gdSelectProduct.Columns.Count; i++)
                    bindValue.Add(gdSelectProduct[rowIndex, i].Text);

                GridDataBinding(gdInputProduct, bindValue, true, false);
            }

            // REMOVE ROW
            DataTable dt = DataTableConverter.Convert(gdSelectProduct.ItemsSource);
            dt.Rows[rowIndex].Delete();
            gdSelectProduct.ItemsSource = DataTableConverter.Convert(dt);
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // VALIDATION
            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;
            if (timeSpan.Days < 0)
            {
                //Util.AlertInfo("조회일자중 이전일자가 이후일자보다 더 클 수 없습니다.");
                Util.MessageValidation("SFU1908");
                return;
            }

            // SEARCH
            GetHalfProductList();
        }
        #endregion

        #region [선택]
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (gdSelectProduct.Rows.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            selectDt = new DataTable();
            selectDt = DataTableConverter.Convert(gdSelectProduct.ItemsSource);

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

                string sBizName = "";
                if (string.Equals(procID, Process.WINDING))
                    sBizName = "DA_PRD_SEL_HALFPROD_LIST_WN";
                else if (string.Equals(procID, Process.ASSEMBLY))
                    sBizName = "DA_PRD_SEL_HALFPROD_LIST_AS";

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_HALFPROD_LIST_WN();
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = lineID;
                Indata["PROCID"] = procID;

                if ( cboProduct.SelectedIndex > 0 )
                    Indata["PRODID"] = Util.NVC(cboProduct.SelectedValue);

                if ( cboElectordeType.SelectedIndex > 0)
                    Indata["ELECTRODETYPE"] = Util.NVC(cboElectordeType.SelectedValue);

                Indata["LOTID"] = Util.NVC(txtLotId.Text.Trim());

                if (string.IsNullOrEmpty(Util.NVC(txtLotId.Text.Trim())))
                {
                    Indata["STARTDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                    Indata["ENDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                }
                else
                {
                    Indata["STARTDT"] = DBNull.Value;
                    Indata["ENDDT"] = DBNull.Value;
                }

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1901");
                    return;
                }

                ////gdInputProduct.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(gdInputProduct, dt, FrameOperation, true);
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

        #region [Validation]
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
                    catch
                    { }
                }

            }
        }
        #endregion

        #endregion

    }
}
