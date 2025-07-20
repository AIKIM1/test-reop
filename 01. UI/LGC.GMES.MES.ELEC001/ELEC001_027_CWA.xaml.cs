/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_027_CWA : UserControl, IWorkArea
    {
        #region < Declaration & Constructor >

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public string VersionCheckFlag = string.Empty;
        string _ProdID = string.Empty;
        string _ProcChkEqptID = string.Empty;

        public ELEC001_027_CWA()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        #endregion < Declaration & Constructor >


        #region < Initialize & Load >

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitPermissions();
            initCombo();
        }

        private void InitPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void initCombo()
        {
            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "ProcessCWA");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);
        }

        #endregion <Initialize & Load >


        #region < Event >

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (sender != null)
                {
                    string ProcessID = Util.NVC(cboProcess.SelectedValue);

                    switch (ProcessID)
                    {
                        case "E0400":
                        case "E0410":
                            chkProc.IsChecked = true;
                            break;

                        default:
                            chkProc.IsChecked = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender != null)
            {
                GetVersionCheckFlag();
                GetMixerTank();
                GetBachOrder();
            }                
        }

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            GetBachOrder();
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

            SaveTankInfo(Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "MTRLID"))
                        , Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "MTRL_GRD"))
                        , null
                        , "INPUT");

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

                    if (VersionCheckFlag.Equals("Y"))
                    {
                        string Version = Util.NVC(dtRow["PROD_VER_CODE"]);

                        if (Version.Equals(string.Empty))
                        {
                            Util.MessageValidation("SFU5036");
                        }
                    }

                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgBatchOrder.SelectedIndex = idx;

                    _ProcChkEqptID = Util.NVC(cboEquipment.SelectedValue);
                    //_ProcChkEqptID = (bool)chkProc.IsChecked ? Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "EQPTID")) : string.Empty;
                    _ProdID = Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[idx].DataItem, "PRODID"));

                    GetMaterial();
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

        #endregion < Event >


        #region < Method >
       
        private void GetVersionCheckFlag()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQUIPID", typeof(string));                

                DataRow Indata = IndataTable.NewRow();
                Indata["EQUIPID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_VER_CHK_FLAG", "INDATA", "OUTDATA", IndataTable);

                if(dt.Rows.Count > 0)
                {
                    VersionCheckFlag = dt.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetMixerTank()
        {
            try
            {
                Util.gridClear(dgMixerTank);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
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
                EQPTNAME.Visibility = Visibility.Collapsed;

                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                {
                    isProc = true;
                    EQPTNAME.Visibility = Visibility.Visible;
                }

                Util.gridClear(dgBatchOrder);
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("STRT_DTTM", typeof(string));
                IndataTable.Columns.Add("END_DTTM", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["STRT_DTTM"] = Convert.ToDateTime(ldpDatePickerFrom.SelectedDateTime).ToString("yyyyMMdd");
                Indata["END_DTTM"] = Convert.ToDateTime(ldpDatePickerTo.SelectedDateTime).ToString("yyyyMMdd");
                if (isProc)
                {
                    Indata["EQPTID"] = null;
                    Indata["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                }                    

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_BATCHORDER_CWA", "INDATA", "RSLTDT", IndataTable);

                dgBatchOrder.BeginEdit();

                Util.GridSetData(dgBatchOrder, dtMain, FrameOperation, true);
                dgBatchOrder.EndEdit();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetMaterial()
        {
            try
            {
                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = cboProcess.SelectedValue.ToString();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["WOID"] = DataTableConverter.GetValue(dgBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK")].DataItem, "WO_DETL_ID");
                Indata["PRODID"] = DataTableConverter.GetValue(dgBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK")].DataItem, "PRODID");
                Indata["PROD_VER_CODE"] = DataTableConverter.GetValue(dgBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK")].DataItem, "PROD_VER_CODE");
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BATCHORDID_MATERIAL_SET_CWA", "INDATA", "OUTDATA", IndataTable);
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
                        indata.Columns.Add("WO_DETL_ID", typeof(string));

                        DataRow row = indata.NewRow();
                        row["TANK_ID"] = Util.NVC(DataTableConverter.GetValue(dgMixerTank.Rows[_Util.GetDataGridCheckFirstRowIndex(dgMixerTank, "CHK")].DataItem, "TANK_ID"));
                        row["MTRLID"] = mtrlid;
                        row["MTRL_GRD"] = mtrl_grd;
                        row["BTCH_ORD_ID"] = btch_ord_id;
                        row["NOTE"] = null;
                        row["USERID"] = LoginInfo.USERID;
                        row["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgBatchOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(dgBatchOrder, "CHK")].DataItem, "WO_DETL_ID"));
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

        #endregion < Method >      
    }
}
