/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.12.15  조영대    : 자재 적정중량 컬럼 추가.
  2021.05.20  조영대    : AGV Station, 호퍼구분, 소요량 추가
  2024.01.03  정재홍    : [CSR] : E20231211-000313 - 전극 우선/대체 자재에 따른 확인창 개선.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_027 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string _ProdID;
        string _ProcChkEqptID;

        // [CSR] : E20231211-000313
        bool isMessage = false;
        string sAlt_Rank_YN = string.Empty;


        public ELEC001_027()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
            initCombo();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRegister);
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion
        private void initCombo()
        {
            string[] sFilter = { LoginInfo.CFG_EQSG_ID, Process.MIXING };
            combo.SetCombo(cboMixerEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            dgMixerTank.Columns["AGV_STATION"].Visibility = Visibility.Collapsed;
            dgMixerTank.Columns["HOPPER_TYPE"].Visibility = Visibility.Collapsed;
            dgMixerTank.Columns["REQ_QTY"].Visibility = Visibility.Collapsed;

            // [CSR] : E20231211-000313
            string [] strAttr  = { "" };
            if (_Util.IsAreaCommoncodeAttrUse("MIX_ALT_MTRL_MESG", "MIX_COL_MESG", strAttr))
            {
                isMessage = true;
                dgMaterial.Columns["ALT_ITEM_RANK"].Visibility = Visibility.Visible;
            }
            else
            {
                isMessage = false;
                dgMaterial.Columns["ALT_ITEM_RANK"].Visibility = Visibility.Collapsed;
            }
        }

        private void cboMixerEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender != null) GetMixerTank();

            GetBachOrder();
        }

        private void dgTankChoice_Checked(object sender, RoutedEventArgs e)
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

                //row 색 바꾸기
                dgMixerTank.SelectedIndex = idx;
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (cboMixerEquipment.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1673"); //설비를 선택하세요
                return;
            }

            GetMixerTank();
            GetBachOrder();

        }

        private void GetMixerTank()
        {
            try
            {
                if (cboMixerEquipment.GetBindValue() == null) return;

                Util.gridClear(dgMixerTank);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = Convert.ToString(cboMixerEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXER_TANK_MTRL_SET_TANK_LIST", "RSLT", "RQST", dt);
                Util.GridSetData(dgMixerTank, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetBachOrder()
        {
            try
            {
                bool isProc = false;
                dgBatchOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                {
                    isProc = true;
                    dgBatchOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }


                Util.gridClear(dgBatchOrder);
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MES_CLOSE_FLAG", typeof(string));
                IndataTable.Columns.Add("STRT_DTTM", typeof(string));
                IndataTable.Columns.Add("END_DTTM", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;

                if (isProc == false)
                    Indata["EQPTID"] = Convert.ToString(cboMixerEquipment.SelectedValue);

                Indata["MES_CLOSE_FLAG"] = "N";
                Indata["STRT_DTTM"] = Convert.ToDateTime(ldpDatePickerFrom.SelectedDateTime).ToString("yyyyMMdd");
                Indata["END_DTTM"] = Convert.ToDateTime(ldpDatePickerTo.SelectedDateTime).ToString("yyyyMMdd");
                Indata["PROCID"] = Process.MIXING;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(isProc ? "DA_PRD_SEL_MIXMTRL_BATCHORDER_PROC" : "DA_PRD_SEL_MIXMTRL_BATCHORDER_EQPT", "INDATA", "RSLTDT", IndataTable);

                dgBatchOrder.BeginEdit();

                Util.GridSetData(dgBatchOrder, dtMain, FrameOperation, true);
                dgBatchOrder.EndEdit();

                dgBatchOrder.MergingCells -= dgBatchOrder_MergingCells;
                dgBatchOrder.MergingCells += dgBatchOrder_MergingCells;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgBatchOrder_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgBatchOrder.Rows.Count <= 0)
                {
                    return;
                }
                int x = 0;
                int x1 = 0;
                for (int i = x1; i < dgBatchOrder.GetRowCount(); i++)
                {
                    if (Util.NVC(dgBatchOrder.GetCell(x, dgBatchOrder.Columns["BTCH_ORD_ID"].Index).Value) == Util.NVC(dgBatchOrder.GetCell(i, dgBatchOrder.Columns["BTCH_ORD_ID"].Index).Value))
                    {
                        x1 = i;
                    }
                    else
                    {
                        for (int j = 0; j < dgBatchOrder.Columns.Count - 2; j++)
                        {
                            e.Merge(new DataGridCellsRange(dgBatchOrder.GetCell((int)x, (int)j), dgBatchOrder.GetCell((int)x1, (int)j)));
                        }

                        x = x1 + 1;
                        i = x1;
                    }
                }
                for (int j = 0; j < dgBatchOrder.Columns.Count - 2; j++)
                {
                    e.Merge(new DataGridCellsRange(dgBatchOrder.GetCell((int)x, (int)j), dgBatchOrder.GetCell((int)x1, (int)j)));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            GetBachOrder();
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            GetBachOrder();
        }

        private void dgBatchOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgBatchOrder.SelectedIndex = idx;

                    _ProcChkEqptID = (bool)chkProc.IsChecked ? Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "EQPTID")) : string.Empty;
                    _ProdID = Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "PRODID"));
                    GetMaterial(Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "WOID")), Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "BTCH_ORD_ID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgMaterialChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgMaterial.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetMaterial(string WOID, string BACH_ORD_ID)
        {
            try
            {
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("BATCHORDID", typeof(string));
                IndataTable.Columns.Add("FLAG", typeof(string));
                IndataTable.Columns.Add("ALL_FLAG", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("AREA_ID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["BATCHORDID"] = BACH_ORD_ID;
                Indata["FLAG"] = null;
                Indata["ALL_FLAG"] = "Y";
                Indata["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREA_ID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQPTID"] = chkProc.IsChecked == true ? _ProcChkEqptID : Convert.ToString(cboMixerEquipment.SelectedValue);

                IndataTable.Rows.Add(Indata);

                string sBizName = string.Empty;
                if (LoginInfo.CFG_SHOP_ID == "G183" || _ProdID.Substring(3, 2).Equals("CA"))
                {
                    sBizName = "DA_PRD_SEL_BATCHORDID_MATERIAL_LIST_FOR_TANK";
                }
                else
                {
                    sBizName = "DA_PRD_SEL_BATCHORDID_MATERIAL_ANODE_FOR_TANK";
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", IndataTable);
                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgMaterial, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK") == -1)
            {
                Util.MessageValidation("SFU3522"); //배치오더를 선택해주세요
                return;
            }
            if (_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK") == -1)
            {
                Util.MessageValidation("SFU3523"); //자재를 선택해주세요
                return;
            }
            if (_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK") == -1)
            {
                Util.MessageValidation("SFU3524"); //탱크를 선택해주세요
                return;
            }

            if (dgMixerTank.Columns["REQ_QTY"].Visibility.Equals(Visibility.Visible))
            {
                if (!Util.IsNVC(dgMixerTank.GetCheckedFirstValue("CHK", "HOPPER_TYPE")) && 
                    !dgMixerTank.GetCheckedFirstValue("CHK", "HOPPER_TYPE").Equals("(N/A)") &&
                    Util.IsNVC(dgMixerTank.GetCheckedFirstValue("CHK", "REQ_QTY")))
                {
                    dgMixerTank.SetValidation(dgMixerTank.GetCheckedRowIndex("CHK").First(), "REQ_QTY");

                    Util.MessageValidation("SFU8352"); //입력되지 않은 소요량을 입력하세요.
                    return;
                }
            }

            if (isMessage)
            {
                sAlt_Rank_YN = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK")].DataItem, "ALT_ITEM_RANK"));
            }

            SaveTankInfo(Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK")].DataItem, "MTRLID"))
                         , Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK")].DataItem, "GRADE"))
                         , Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK")].DataItem, "BTCH_ORD_ID"))
                         , "SELECT");

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK") == -1)
            {
                Util.MessageValidation("SFU3524"); //탱크를 선택해주세요
                return;
            }

            if (dgMixerTank.Columns["REQ_QTY"].Visibility.Equals(Visibility.Visible))
            {
                if (!Util.IsNVC(dgMixerTank.GetCheckedFirstValue("CHK", "HOPPER_TYPE")) &&
                    !dgMixerTank.GetCheckedFirstValue("CHK", "HOPPER_TYPE").Equals("(N/A)") &&
                    Util.IsNVC(dgMixerTank.GetCheckedFirstValue("CHK", "REQ_QTY")))
                {
                    dgMixerTank.SetValidation(dgMixerTank.GetCheckedRowIndex("CHK").First(), "REQ_QTY");

                    Util.MessageValidation("SFU8352"); //입력되지 않은 소요량을 입력하세요.
                    return;
                }
            }

            if (isMessage)
            {
                sAlt_Rank_YN = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK")].DataItem, "ALT_ITEM_RANK"));
            }

            SaveTankInfo(Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "MTRLID"))
                        , Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "MTRL_GRD"))
                        , null
                        , "INPUT");

        }

        private void SaveTankInfo(string mtrlid, string mtrl_grd, string btch_ord_id, string btnType)
        {
            // [CSR] : E20231211-000313
            string msg = string.Empty;
            string sInfoMsg = string.Empty;

            if (btnType.Equals("SELECT"))
            {
                if (sAlt_Rank_YN == "Y" && isMessage == true)
                {
                    msg = "SFU8925"; // "선택하신 [%1] 자재는 대체 자재입니다. 투입하시겠습니까?"
                    sInfoMsg = mtrlid;
                }
                else
                {
                    msg = "SFU1241"; // "저장하시겠습니까?"
                    sInfoMsg = null;
                }
            }
            else
            {
                msg = "SFU3564"; // "직접 입력하신 내용을 저장하시겠습니까?";
                sInfoMsg = null;
            }

            //string msg = btnType.Equals("SELECT") ? "SFU1241" : "SFU3564";//"저장하시겠습니까?" : "직접 입력하신 내용을 저장하시겠습니까?";

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg, sInfoMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet ds = new DataSet();
                        DataTable indata = ds.Tables.Add("INDATA");
                        indata.Columns.Add("TANK_ID", typeof(string));
                        indata.Columns.Add("MTRLID", typeof(string));
                        indata.Columns.Add("MTRL_GRD", typeof(string));
                        indata.Columns.Add("BTCH_ORD_ID", typeof(string));
                        indata.Columns.Add("NOTE", typeof(string));
                        indata.Columns.Add("USERID", typeof(string));
                        indata.Columns.Add("PRP_WEIGHT", typeof(string));
                        indata.Columns.Add("BTCH_REQR_QTY", typeof(string));

                        DataRow row = indata.NewRow();
                        row["TANK_ID"] = Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "TANK_ID"));
                        row["MTRLID"] = mtrlid;
                        row["MTRL_GRD"] = mtrl_grd;
                        row["BTCH_ORD_ID"] = btch_ord_id;
                        row["NOTE"] = null;
                        row["USERID"] = LoginInfo.USERID;
                        row["PRP_WEIGHT"] = Util.NVC_Int(dgMixerTank.GetCheckedFirstValue("CHK", "PRP_WEIGHT"));

                        if (dgMixerTank.Columns["REQ_QTY"].Visibility.Equals(Visibility.Visible))
                        {
                            row["BTCH_REQR_QTY"] = Util.NVC_Int(dgMixerTank.GetCheckedFirstValue("CHK", "REQ_QTY"));
                            if (Util.IsNVC(row["BTCH_REQR_QTY"]))
                            {
                                row["BTCH_REQR_QTY"] = null;
                            }
                        }
                        else
                        {
                            row["BTCH_REQR_QTY"] = null;
                        }

                        indata.Rows.Add(row);


                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_MAT_REG_MIXER_TANK_MTRL_SET", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");//정상처리되었습니다.

                                GetMixerTank();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void dgMixerTank_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Presenter == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                switch (e.Cell.Column.Name)
                {
                    case "REQ_QTY":
                        if (Util.IsNVC(dgMixerTank.GetValue(e.Cell.Row.Index, "HOPPER_TYPE")) ||
                            dgMixerTank.GetValue(e.Cell.Row.Index, "HOPPER_TYPE").Equals("(N/A)"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        break;
                }
            }));
        }

        private void dgMixerTank_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            switch (e.Column.Name)
            {
                case "REQ_QTY":
                    if (Util.IsNVC(dgMixerTank.GetValue(e.Row.Index, "HOPPER_TYPE")) ||
                        dgMixerTank.GetValue(e.Row.Index, "HOPPER_TYPE").Equals("(N/A)"))
                    {
                        e.Cancel = true;
                    }
                    break;
            }
        }
    }
}
