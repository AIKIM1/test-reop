using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_050_RWK_BOX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050_RWK_BOX : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private UserControl _UCParent = null;
        private Util _Util = null;
        private DataTable releasedTable = null;
        private bool IsReload = false;

        public ASSY004_050_RWK_BOX(UserControl uc)
        {
            _UCParent = uc;
            _Util = new Util();
            InitTable();
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get; set;
        }
        #endregion

        #region [Initialize Method]
        private void InitCombos()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.RWK_LNS};
            C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbChild: cboEquipmentSegmentChild, sCase: "PROCESSEQUIPMENTSEGMENT");


            String[] sFilter3 = { Process.RWK_LNS };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_BY_EQSGID_PROCID");


            String[] sFilter2 = { LoginInfo.CFG_AREA_ID, Process.PACKAGING };
            _combo.SetCombo(cboReleasedLine, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "EQUIPMENTSEGMENT_PROC");
            cboReleasedLine.SelectedIndex = 0;

            if (cboEquipmentSegment.Items.Count >= 2)
                cboEquipmentSegment.SelectedIndex = 1;
        }
        private void InitControls()
        {
            txtArea.Text = LoginInfo.CFG_AREA_NAME;
        }
        private void InitTable()
        {
            releasedTable = new DataTable();
            releasedTable.Columns.Add("CHK", typeof(bool));
            releasedTable.Columns.Add("ImageUrl", typeof(string));
            releasedTable.Columns.Add("WIPDTTM_IN", typeof(string));
            releasedTable.Columns.Add("LOTID", typeof(string));
            releasedTable.Columns.Add("CSTID", typeof(string));
            releasedTable.Columns.Add("WIPQTY", typeof(int));
            releasedTable.Columns.Add("PRODID", typeof(string));
            releasedTable.Columns.Add("PRJT_NAME", typeof(string));
        }
        #endregion

        #region [Event]

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if ((sender as C1ComboBox).SelectedIndex == -1 || cboEquipmentSegment.SelectedValue.ToString() == "SELECT" || cboEquipmentSegment.SelectedIndex == 0)
            {
                Util.gridClear(dgWaitReleasedWip);
                return;
            }
            else
            {
                Util.gridClear(dgWaitReleasedWip);
                btnSearch_Click(null, null);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsReload)
                return;
            IsReload = true;

            InitCombos();
            InitControls();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWaitReleasedWip();
            txtCstID.Clear();
            txtCstID.Focusable = true;
            txtCstID.Focus();

            if (!cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                txtEquipmentSegment.Text = cboEquipmentSegment.SelectedValue.ToString();
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            DataRowView checkedDrv = ((sender as Button).Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;
            int idx = _Util.GetDataGridRowIndex(dgWaitReleasedWip, "LOTID", DataTableConverter.GetValue(checkedDrv, "LOTID") as string);

            DataTable tmpTable = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource);
            tmpTable.Rows[idx]["CHK"] = false;
            Util.GridSetData(dgWaitReleasedWip, tmpTable, FrameOperation);
        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            ReleasedWips();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataRowView checkedDrv = ((sender as CheckBox).Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;
            DataRow newRow = releasedTable.NewRow();
            newRow["CHK"] = DataTableConverter.GetValue(checkedDrv, "CHK");
            newRow["ImageUrl"] = @".\Images\icon_close.png";
            newRow["WIPDTTM_IN"] = DataTableConverter.GetValue(checkedDrv, "WIPDTTM_IN");
            newRow["LOTID"] = DataTableConverter.GetValue(checkedDrv, "LOTID");
            newRow["CSTID"] = DataTableConverter.GetValue(checkedDrv, "CSTID");
            newRow["WIPQTY"] = DataTableConverter.GetValue(checkedDrv, "WIPQTY");
            newRow["PRODID"] = DataTableConverter.GetValue(checkedDrv, "PRODID");
            newRow["PRJT_NAME"] = DataTableConverter.GetValue(checkedDrv, "PRJT_NAME");
            releasedTable.Rows.Add(newRow);
            RefreshDgReleasedWip();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataRowView checkedDrv = ((sender as CheckBox).Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;
            int idx = _Util.GetDataGridRowIndex(dgReleasedWip, "LOTID", DataTableConverter.GetValue(checkedDrv, "LOTID") as string);
            releasedTable.Rows.RemoveAt(idx);
            RefreshDgReleasedWip();
        }

        private void txtCstID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string cstID = txtCstID.Text;
                int idx = _Util.GetDataGridRowIndex(dgWaitReleasedWip, "CSTID", cstID);

                if(idx == -1)
                {
                    //해당하는 LOT정보가 없습니다.
                    Util.MessageValidation("SFU2025", (result) =>
                    {
                        if(result == MessageBoxResult.OK)
                        {
                            txtCstID.Clear();
                            txtCstID.Focusable = true;
                            txtCstID.Focus();
                        }
                    });
                    return;
                }

                DataTable tmpTable = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource);
                tmpTable.Rows[idx]["CHK"] = true;
                Util.GridSetData(dgWaitReleasedWip, tmpTable, FrameOperation);
                txtCstID.Clear();
                txtCstID.Focusable = true;
                txtCstID.Focus();
            }
        }
        #endregion

        #region [Method]
        private void RefreshDgReleasedWip()
        {
            Util.gridClear(dgReleasedWip);
            Util.GridSetData(dgReleasedWip, releasedTable, FrameOperation);
        }

        private void GetWaitReleasedWip()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                if (cboEquipment.SelectedIndex == -1)
                    return;

                ShowLoadingIndicator();

                DataTable inData = new DataTable();
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("WIPSTAT", typeof(string));
                inData.Columns.Add("WIP_TYPE_CODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue as string;
                newRow["WIPSTAT"] = "END";
                newRow["WIP_TYPE_CODE"] = "OUT";
                //newRow["EQPTID"] = cboEquipment.SelectedValue.ToString().Trim().Equals("-ALL-") ? null : cboEquipment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedIndex == 0 ? null : cboEquipment.SelectedValue.ToString();
                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_BY_EQSGID_PROCID_FOR_RWK_ST", "INDATA", "OUTDATA", inData, (searchResult, searchException) =>
                {
                    try
                    {
                        Util.GridSetData(dgWaitReleasedWip, searchResult, FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ReleasedWips()
        {
            try
            {
                if (!CanRelease())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(int));
                inInput.Columns.Add("ACTQTY2", typeof(int));
                inInput.Columns.Add("ACTUQTY", typeof(int));
                inInput.Columns.Add("WIPNOTE", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQSGID"] = cboReleasedLine.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = Process.PACKAGING;
                inData.Rows.Add(newRow);
                newRow = null;

                foreach (DataRowView drv in releasedTable.DefaultView)
                {
                    newRow = inInput.NewRow();
                    newRow["LOTID"] = DataTableConverter.GetValue(drv, "LOTID") as string;
                    newRow["ACTQTY"] = Util.NVC_Int(DataTableConverter.GetValue(drv, "WIPQTY"));
                    newRow["ACTQTY2"] = Util.NVC_Int(DataTableConverter.GetValue(drv, "WIPQTY"));
                    newRow["ACTUQTY"] = 0;
                    inInput.Rows.Add(newRow);
                    newRow = null;
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DISPATCH_OUT_LOT_RWK_ST_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.gridClear(dgWaitReleasedWip);
                        Util.gridClear(dgReleasedWip);
                        GetWaitReleasedWip();
                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Util Method]
        private bool CanSearch()
        {
            bool bRet = false;

            if ((cboEquipmentSegment.SelectedValue as string).Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanRelease()
        {
            bool bRet = false;

            if ((cboEquipmentSegment.SelectedValue as string).Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if ((cboReleasedLine.SelectedValue as string).Trim().Equals("SELECT"))
            {
                ControlsLibrary.MessageBox.Show("출고할 라인을 선택하세요.",null,"caution", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet;
            }

            if (dgReleasedWip.Rows.Count == 0)
            {
                ControlsLibrary.MessageBox.Show("출고할 재공을 하나 이상 선택하세요..", null, "caution", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Collapsed)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Visible)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion


    }
}
