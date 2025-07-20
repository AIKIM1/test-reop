/*************************************************************************************
 Created Date : 2020.06.04
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.04  조영대 : Initial Created.(Copy by ELEC001_027)
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
    public partial class ELEC001_027_CNB : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string _ProdID;
        string _ProcChkEqptID;

        public ELEC001_027_CNB()
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
            ApplyPermissions();

            InitCombo();

            InitVisibleColumn();

            this.Loaded -= UserControl_Loaded;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRegister);
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_Util.IsCommonCodeUse("MIXER_MTRL_MANL_MAPP_BLOCK_AREA", LoginInfo.CFG_AREA_ID))
            {
                dgMixerTank.Columns["MTRLID"].IsReadOnly = true;
            }
        }

        #endregion
        private void InitCombo()
        {
            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: Filter);

            //공정
            string[] arrColumn1 = { "LANGID", "EQSGID" };
            string[] arrCondition1 = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID };
            cboProcess.SetDataComboItem("DA_BAS_SEL_PROCESS_CBO_CNB", arrColumn1, arrCondition1, CommonCombo.ComboStatus.SELECT, false);
            cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;
            if (cboProcess.SelectedIndex < 0) cboProcess.SelectedIndex = 0;

            //설비
            string[] arrColumn2 = { "LANGID", "EQSGID", "PROCID" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, cboProcess.GetStringValue() };
            cboEquipment.SetDataComboItem("DA_BAS_SEL_EQUIPMENT_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.SELECT, false);
            cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cboEquipment.SelectedIndex < 0) cboEquipment.SelectedIndex = 0;
        }

        private void InitVisibleColumn()
        {
            DataTable dtCommon = GetCommonCode("MIXER_MATERIAL_COLUMN_VISIBLE", LoginInfo.CFG_AREA_ID);
            if (dtCommon != null && dtCommon.Rows.Count > 0)
            {
                dgMixerTank.Columns["AGV_STATION"].Visibility = Visibility.Visible;
                dgMixerTank.Columns["HOPPER_TYPE"].Visibility = Visibility.Visible;
                dgMixerTank.Columns["REQ_QTY"].Visibility = Visibility.Visible;
            }
            else
            {
                dgMixerTank.Columns["AGV_STATION"].Visibility = Visibility.Collapsed;
                dgMixerTank.Columns["HOPPER_TYPE"].Visibility = Visibility.Collapsed;
                dgMixerTank.Columns["REQ_QTY"].Visibility = Visibility.Collapsed;

            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            string[] arrColumn2 = { "LANGID", "EQSGID", "PROCID" };
            string[] arrCondition2 = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, cboProcess.GetStringValue() };
            cboEquipment.SetDataComboItem("DA_BAS_SEL_EQUIPMENT_CBO", arrColumn2, arrCondition2, CommonCombo.ComboStatus.SELECT, false);
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
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

            if (cboEquipment.Text.Equals("-SELECT-"))
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
                if (cboEquipment.GetBindValue() == null) return;

                Util.gridClear(dgMixerTank);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXER_TANK_MTRL_SET_TANK_LIST_CNB", "RSLT", "RQST", dt);
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
                    Indata["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);

                Indata["MES_CLOSE_FLAG"] = "N";
                Indata["STRT_DTTM"] = Convert.ToDateTime(ldpDatePickerFrom.SelectedDateTime).ToString("yyyyMMdd");
                Indata["END_DTTM"] = Convert.ToDateTime(ldpDatePickerTo.SelectedDateTime).ToString("yyyyMMdd");
                Indata["PROCID"] = cboProcess.GetBindValue();
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

        private DataTable GetCommonCode(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CBO_CODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
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
                    if (!string.IsNullOrEmpty(Util.NVC(dgBatchOrder.GetCell(x, dgBatchOrder.Columns["BTCH_ORD_ID"].Index).Value)))
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
                    else
                    {
                        if (Util.NVC(dgBatchOrder.GetCell(x, dgBatchOrder.Columns["PRODID"].Index).Value) == Util.NVC(dgBatchOrder.GetCell(i, dgBatchOrder.Columns["PRODID"].Index).Value))
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
                    GetMaterial(Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "WOID"))
                        , Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "BTCH_ORD_ID")));
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

                if ((bool)rb.IsChecked && (
                    (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1") ||
                    (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("False") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("True")
                    )) // 2024.10.10. 김영국 - 값이 True, False로 넘어오는 것들에 대한 문제 해결을 위하여 로직 추가.
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

                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("BATCHORDID", typeof(string));
                IndataTable.Columns.Add("FLAG", typeof(string));
                IndataTable.Columns.Add("ALL_FLAG", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("AREA_ID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(BACH_ORD_ID))
                {
                    Indata["BATCHORDID"] = BACH_ORD_ID;
                }
                Indata["FLAG"] = null;
                Indata["ALL_FLAG"] = "Y";
                Indata["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREA_ID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQPTID"] = chkProc.IsChecked == true ? _ProcChkEqptID : Convert.ToString(cboEquipment.SelectedValue);
                Indata["PRODID"] = _ProdID;
                Indata["PROCID"] = cboProcess.GetBindValue();

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

            SaveTankInfo(Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "MTRLID"))
                        , Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "MTRL_GRD"))
                        , null
                        , "INPUT");

        }

        private void SaveTankInfo(string mtrlid, string mtrl_grd, string btch_ord_id, string btnType)
        {
            string msg = btnType.Equals("SELECT") ? "SFU1241" : "SFU3564";//"저장하시겠습니까?" : "직접 입력하신 내용을 저장하시겠습니까?";

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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
                        //2021.11.12 한종현 requsted by 동의항 사원. ESNJ 1동 생산시스템 구축 프로젝트. 알람기준 소수점 입력 가능하도록 수정 요청
                        //row["PRP_WEIGHT"] = Util.NVC_Int(dgMixerTank.GetCheckedFirstValue("CHK", "PRP_WEIGHT"));
                        row["PRP_WEIGHT"] = Util.NVC_Decimal(dgMixerTank.GetCheckedFirstValue("CHK", "PRP_WEIGHT"));

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
