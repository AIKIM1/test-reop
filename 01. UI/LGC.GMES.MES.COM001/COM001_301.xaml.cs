/*************************************************************************************
 Created Date : 2019.05.07
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Cell Map 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.07  정문교 : Initial Created.
  2019.06.28  정문교 : 생산LOT ID 칼럼 제거, 완성 LOT ID 칼럼 추가
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_301 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        private string _carrierID;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize        

        public COM001_301()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgCarrier);
            Util.gridClear(dgCell);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboEquipment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            String[] sFilter = { Process.LAMINATION + "," + Process.STACKING_FOLDING };
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT_PROC");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sFilter: sFilter, sCase: "EQUIPMENT_MAIN_LEVEL");
        }

        private void SetControls()
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                object[] tmps = this.FrameOperation.Parameters;

                cboArea.SelectedValue = tmps[0].ToString();
                cboEquipmentSegment.SelectedValue = tmps[1].ToString();
                cboEquipment.SelectedValue = tmps[2].ToString();
                txtProdLotID.Text = tmps[3].ToString();
                _carrierID = tmps[4].ToString();

                SearchProcess();
                SearchCarrier(txtProdLotID.Text);
                SearchCell(_carrierID);

                _carrierID = string.Empty;
            }
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControls();

            this.Loaded -= UserControl_Loaded;
        }

        private void dgListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                // row 색 바꾸기
                dgList.SelectedIndex = idx;
                // 완성실적 조회
                SearchCarrier(DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "LOTID").ToString());
            }
        }

        private void dgCarrierChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                // row 색 바꾸기
                dgCarrier.SelectedIndex = idx;
                // Cell 정보 조회
                SearchCell(DataTableConverter.GetValue(dgCarrier.Rows[idx].DataItem, "LOTID").ToString());
            }
        }

        private void dgCell_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgCell.GetCellFromPoint(pnt);

            if (cell != null)
            {
                DataRowView dr = dgCell.Rows[cell.Row.Index].DataItem as DataRowView;

                // Cell 팝업호출
                popCell(Util.NVC(dr["PROD_LOTID"]), Util.NVC(dr["REP_CELL_ID"]));
            }
        }

        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        /// <summary>
        /// Carrier 이력 조회 팝업
        /// </summary>
        private void btnCarrierID_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_CARRIER_INFO popCarrier = new CMM_ASSY_CARRIER_INFO();
            popCarrier.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popCarrier.Name.ToString()) == false)
                return;

            object[] parameters = new object[3];
            //parameters[0] = Util.NVC(cboArea_R.SelectedValue);
            //parameters[1] = Util.NVC(cboEquipmentSegment_R.SelectedValue);
            //parameters[2] = Util.NVC(cboProcess_R.SelectedValue);
            C1WindowExtension.SetParameters(popCarrier, parameters);

            popCarrier.Closed += new EventHandler(popCarrier_Closed);
            grdMain.Children.Add(popCarrier);
            popCarrier.BringToFront();
        }

        private void popCarrier_Closed(object sender, EventArgs e)
        { 
            CMM_ASSY_CARRIER_INFO popup = sender as CMM_ASSY_CARRIER_INFO;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        #endregion

        #region Mehod
        /// <summary>
        /// 생산 LOT 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                InitializeGrid();

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_CELLMAP_LOT_LIST_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtProdLotID.Text))
                {
                    dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                    dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                    dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                }
                else
                {
                    dr["PROD_LOTID"] = txtProdLotID.Text;
                }
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count == 1)
                    {
                        bizResult.Rows[0]["CHK"] = 1;
                    }

                    Util.GridSetData(dgList, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Carrier 조회
        /// </summary>
        private void SearchCarrier(string ProdLot)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_CELLMAP_OUTLOT_LIST_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(String));
                dtRqst.Columns.Add("LOTID", typeof(String));
                dtRqst.Columns.Add("CSTID", typeof(String));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = ProdLot;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(_carrierID))
                    {
                        bizResult.Select("CSTID ='" + _carrierID + "'").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                    }

                    Util.GridSetData(dgCarrier, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cell 조회
        /// </summary>
        private void SearchCell(string LotID)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_CELLMAP_CELL_LIST_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(String));
                dtRqst.Columns.Add("CSTID", typeof(String));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgCell, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]

        #region [Validation]
        private bool ValidationSearch()
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            return true;
        }
        #endregion

        #region [Function]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnCarrierID
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

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        private void popCell(string ProdLotID, string CellID)
        {
            CMM_ASSY_CELL_INFO popCell = new CMM_ASSY_CELL_INFO();
            popCell.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popCell.Name.ToString()) == false)
                return;

            popCell.FormCellMap = "Y";

            object[] parameters = new object[2];
            parameters[0] = ProdLotID;
            parameters[1] = CellID;
            C1WindowExtension.SetParameters(popCell, parameters);

            popCell.Closed += new EventHandler(popCell_Closed);
            grdMain.Children.Add(popCell);
            popCell.BringToFront();
        }

        private void popCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO popup = sender as CMM_ASSY_CELL_INFO;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }
        #endregion



        #endregion

    }
}
