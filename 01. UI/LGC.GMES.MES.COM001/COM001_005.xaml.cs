/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 이진선
   Decription : VD예약
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.



 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_005 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        private string _EquipElec = string.Empty;
        private string _EquipFlag = string.Empty; //공통으로 쓰는지
        private string _Unit = string.Empty;
        private int _MaxNum = -1;
        private string _Process = string.Empty;
        int i = 1;
        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();
        DataTable dtAllLot = null;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public COM001_005()
        {
            InitializeComponent();

        }


        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            SetWorkOrderWindow();

        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initcombo();

           
            chkWaterSpecOut.Visibility = Visibility.Visible;
            dgCanReserveElec.Visibility = Visibility.Visible;
            dgReservedLotElec.Visibility = Visibility.Visible;

            dgLotListElec.Visibility = Visibility.Visible;
            tbSkidID.Visibility = Visibility;


        }

        #region Initialize

        #region[전극 전체]
        C1.WPF.DataGrid.DataGridRowHeaderPresenter preElec = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAllElec = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion

        #endregion

        #region Event

        #region[LOT예약]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgCanReserveElec.ItemsSource = null;
            dgReservedLotElec.ItemsSource = null;
            
            
            SearchData();
        }

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //공정
            String[] sFilter = { cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            _Process = Convert.ToString(cboVDProcess.SelectedValue);

        }
        private void cboVDProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
                if (!ValidationLot(index)) return;

            

                try
                {
                    SetChkCutID(index, dgCanReserveElec);

                    if (Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "CHK")).Equals("True"))
                    {
                        ValidateQaSkid(Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "CSTID")));
                        AddRow(dgLotListElec, dgCanReserveElec, index);

                    }
                    else
                    {
                        dgLotListElec.IsReadOnly = false;

                        //remove
                        for (int i = 0; i < dgLotListElec.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgLotListElec.Rows[i].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "CSTID"))))
                            {
                                dgLotListElec.RemoveRow(i);
                            }
                        }

                        for (int i = 0; i < dgLotListElec.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgLotListElec.Rows[i].DataItem, "RSV_SEQ", i + 1);
                        }

                        dgLotListElec.IsReadOnly = true;

                        DataTable dt = DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "CSTID")) + "'").Count() == 0 ? null : DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "CSTID")) + "'").CopyToDataTable();
                        if (dt == null) return;
                        if (dt.Rows.Count == 0) return;

                        #region # VD 짝맞춤 - 대LOT의 첫번째 SKID 없는 경우 선택된 SKID 샘플링 Lot 처리 (C20200520-000232)
                        // (C20200520-000232) VD 짝맞춤 
                        DataTable indt = new DataTable();
                        indt.Columns.Add("CSTID", typeof(string));

                        DataRow row = indt.NewRow();
                        row["CSTID"] = Util.NVC(dt.Rows[0]["CSTID"]);
                        indt.Rows.Add(row);

                        DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVE_QA_TRGT_LOT", "INDATA", "RSLTDT", indt);
                        if (result == null) return;
                        if (result.Rows.Count == 0) return;

                        //해당LOT들 QA_INSP_TRGT_FLAG null처리;
                        CancelQaTarget(result);

                        if (CommonVerify.HasDataGridRow(dgLotListElec))
                        {
                            DataTable dt1 = ((DataView)dgLotListElec.ItemsSource).Table;
                            var queryEdit = (from t in dt1.AsEnumerable()
                                             where t.Field<string>("LARGELOT") == Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "LARGELOT")) &&
                                                   t.Field<string>("QA_INSP_TRGT_FLAG") == "Y"
                                             select t).ToList();

                            if (!queryEdit.Any())
                            {
                                DataRow[] dRow = dt1.Select("LARGELOT = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "LARGELOT")) + "'");
                                if (dRow.Length > 0)
                                    ValidateQaSkid(Util.NVC(dRow[0]["CSTID"]));
                                dgLotListElec.Refresh(true);

                                string qa_flag = string.Empty;
                                if (result.Rows.Count != 0)
                                {
                                    if (result.Select("QA_INSP_TRGT_FLAG = 'Y'").Count() != 0)
                                        qa_flag = "Y";
                                    if (dRow.Length > 0)
                                    {
                                        int iLotIdx = Util.gridFindDataRow(ref dgLotListElec, "CSTID", Util.NVC(dRow[0]["CSTID"]), false);
                                        DataTableConverter.SetValue(dgLotListElec.Rows[iLotIdx].DataItem, "QA_INSP_TRGT_FLAG", string.Equals(qa_flag, string.Empty) ? "X" : qa_flag);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

               


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }

        }

        private void CancelQaTarget(DataTable dt)
        {
            try
            {
                DataSet ds = new DataSet();

                DataTable indata = ds.Tables.Add("INDATA");
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));
                indata.Columns.Add("PROCID", typeof(string));
                indata.Columns.Add("EQPTID", typeof(string));
                indata.Columns.Add("USERID", typeof(string));

                DataRow row = indata.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                indata.Rows.Add(row);

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                row = null;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = dt.Rows[i]["LOTID"];
                    inLot.Rows.Add(row);
                }


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_LOT_SAMPLING_VD_CANCEL", "INDATA,IN_LOT", null, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void btnReserve_Click(object sender, RoutedEventArgs e)
        {
            
            if (!ValidationReserve())
            {
                return;
            }

            try
            {

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("WATER_SPEC_FLAG", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["WATER_SPEC_FLAG"] = (!_Unit.Equals("LOT") && chkWaterSpecOut.IsChecked == true) ? "Y" : "N";

                indataSet.Tables["IN_EQP"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("RSV_SEQ", typeof(string));


                for (int i = 0; i < dgLotListElec.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotListElec.Rows[i].DataItem, "CSTID"));
                    row["RSV_SEQ"] = Util.NVC(DataTableConverter.GetValue(dgLotListElec.Rows[i].DataItem, "RSV_SEQ"));
                    indataSet.Tables["IN_LOT"].Rows.Add(row);
                }
             
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RESERVE_LOT_VD", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1731");

                        dgLotListElec.ItemsSource = null;
                        initGridTable(dgLotListElec);

                        SearchData();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // Util.MessageException(ex);   //예약 오류
            }
     
               
        }
        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

               

                ValidateQaSkid(txtLOTID.Text);

                //if (chkWaterSpecOut.IsChecked == true)
                //{
                    if (!txtLOTID.Text.Equals(""))
                    {
                        if (DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select().Count() !=  0 ? DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CSTID = '" + txtLOTID.Text + "'").Count() == 0 : true)
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("CSTID", typeof(string));

                            DataRow row = dt.NewRow();
                            row["CSTID"] = txtLOTID.Text;
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPATTR_FOR_TERM_VD_REWORK", "INDATA", "RSLTDT", dt);
                            if (result.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU3521"); //해당SKID의 정보가 없습니다.
                                txtLOTID.Text = "";
                                return;
                            }


                            DataTable tmp = DataTableConverter.Convert(dgCanReserveElec.ItemsSource);

                            if(tmp.Rows.Count == 0)
                            { 

                                for (int i = 0; i < dgCanReserveElec.Columns.Count; i++)
                                {
                                    tmp.Columns.Add( dgCanReserveElec.Columns[i].Name, dgCanReserveElec.Columns[i].Name.Equals("CHK") ? typeof(bool) : typeof(string));
                                }
                            }
                               
                            if (tmp != null)
                            {
                                for (int i = 0; i < result.Rows.Count; i++)
                                {
                                    row = tmp.NewRow();
                                    row["CSTID"] = Convert.ToString(result.Rows[i]["CSTID"]);
                                    row["LOTID"] = Convert.ToString(result.Rows[i]["LOTID"]);
                                    row["LARGELOT"] = Convert.ToString(result.Rows[i]["LOTID_RT"]);
                                    row["PROCNAME"] = Convert.ToString(result.Rows[i]["PROCNAME"]);
                                    row["WIPSNAME"] = Convert.ToString(result.Rows[i]["WIPSNAME"]);
                                    row["WIPQTY"] = Convert.ToString(result.Rows[i]["WIPQTY"]);
                                    row["UNIT"] = Convert.ToString(result.Rows[i]["UNIT_CODE"]);
                                    row["PRJT_NAME"] = Convert.ToString(result.Rows[i]["PRJT_NAME"]);
                                    row["MODLID"] = Convert.ToString(result.Rows[i]["MODLID"]);
                                    row["PRODID"] = Convert.ToString(result.Rows[i]["PRODID"]);
                                    row["PRODNAME"] = Convert.ToString(result.Rows[i]["PRODNAME"]);
                                    row["ELEC"] = Convert.ToString(result.Rows[i]["PRDT_CLSS_CODE"]);
                                    row["ELECNAME"] = Convert.ToString(result.Rows[i]["ELECNAME"]);
                                    row["QA_INSP_TRGT_FLAG"] = Convert.ToString(result.Rows[i]["QA_INSP_TRGT_FLAG"]);
                                    row["WIPSTAT"] = Convert.ToString(result.Rows[i]["WIPSTAT"]);
                                    tmp.Rows.Add(row);
                                }
                            }
                            dgCanReserveElec.ItemsSource = DataTableConverter.Convert(tmp);

                        }
                    }


                    for (int i = 0; i < dgCanReserveElec.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[i].DataItem, "CSTID")).Equals(txtLOTID.Text))
                        {
                            DataTableConverter.SetValue(dgCanReserveElec.Rows[i].DataItem, "CHK", true);
                            //SetChkCutID(i);
                            if (!ValidationLot(i))
                            {
                                dgCanReserveElec.ScrollIntoView(i, 0);
                                txtLOTID.Text = "";
                                return;
                            }

                            SetChkCutID(i, dgCanReserveElec);
                            dgCanReserveElec.ScrollIntoView(i, 0);
                            AddRow(dgLotListElec, dgCanReserveElec, i);
                            txtLOTID.Text = "";
                            return;
                        }
                    }

                   

                }



           // }
        }
        private void ValidateQaSkid(string lotid)
        {
            DataSet ds = new DataSet();
            DataTable indata = ds.Tables.Add("INDATA");
            indata.Columns.Add("EQPTID", typeof(string));
            indata.Columns.Add("PROCID", typeof(string));
            indata.Columns.Add("USERID", typeof(string));

            DataRow row = indata.NewRow();
            row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
            row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
            row["USERID"] = LoginInfo.USERID;

            indata.Rows.Add(row);

            DataTable inLot = ds.Tables.Add("IN_LOT");
            inLot.Columns.Add("LOTID");

            DataRow newRow = inLot.NewRow();
            newRow["LOTID"] = lotid;
            inLot.Rows.Add(newRow);

            DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_LOT_SAMPLING_VD_ELEC", "INDATA, IN_LOT", "OUTDATA", ds);


        }


        private void initGridTable(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            datagrid.BeginEdit();
            datagrid.ItemsSource = DataTableConverter.Convert(dt);
            datagrid.EndEdit();
        }

        #endregion
        #region[예약취소]
        private void btnReserveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {

             
                if (_Util.GetDataGridCheckFirstRowIndex(dgReservedLotElec, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632");
                    // Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }
                DataTable dt = DataTableConverter.Convert(dgReservedLotElec.ItemsSource);
                if (dt.Rows.Count == 0) return;

                if (dt.Select("CHK = 0").Count() != 0)
                {
                    Util.MessageValidation("SFU3202"); //모든 LOT을 선택하세요
                    return;
                }
              

                if (Convert.ToString(cboVDEquipment.SelectedValue).Equals(""))
                {
                    Util.MessageValidation("SFU1732");
                    //Util.Alert("SFU1732");  //예약 취소 할 설비를 선택해주세요.
                    return;
                }


                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("TERM_FLAG", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgReservedLotElec.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReservedLotElec, "CHK")].DataItem, "EQPTID"));
                row["USERID"] = LoginInfo.USERID;
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["TERM_FLAG"] = ((bool)chkWaterSpecOut.IsChecked && !_Unit.Equals("LOT")) ? "Y" : "N";
                indataSet.Tables["IN_EQP"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

             
                for (int i = 0; i < dgReservedLotElec.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReservedLotElec.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReservedLotElec.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReservedLotElec.Rows[i].DataItem, "LOTID"));
                        indataSet.Tables["IN_LOT"].Rows.Add(row);
                    }
                }
              


                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_RESERVE_LOT", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1736");
                            SearchData();

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, indataSet);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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

        #endregion
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReserve);
            listAuth.Add(btnReserveCancel);
           
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void initcombo()
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CBO_CODE"] = "SKID";
            dr["CBO_NAME"] = "SKID";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "LANE";
            dr["CBO_NAME"] = "LANE";
            dt.Rows.Add(dr);


            cboSkid.DisplayMemberPath = "CBO_NAME";
            cboSkid.SelectedValuePath = "CBO_CODE";
            cboSkid.ItemsSource = dt.Copy().AsDataView();

            cboSkid.SelectedIndex = 0;

        }
        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable dt = null;
                DataRow row = null;
                DataTable result = null;

                if (cboVDEquipment.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1673");
                    return;
                }

                  GetWorkOrder();

              
                DataTable preElec = null;
                if ( dgLotListElec.GetRowCount() != 0)
                {
                    preElec = DataTableConverter.Convert(dgLotListElec.ItemsSource);
               
                }
                dgReservedLotElec.ItemsSource = null;
                         
                initGridTable(dgLotListElec);

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("ELEC", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue) + ",E7000";
                row["ELEC"] = _EquipElec;
                dt.Rows.Add(row);


               

                if (Convert.ToString(cboSkid.SelectedValue).Equals("SKID"))
                {
                    cLotid.Visibility = Visibility.Collapsed;
                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESERVE_LOT_LIST_ELEC_SB_SKID", "INDATA", "RSLTDT", dt);

                }
                else
                {
                    cLotid.Visibility = Visibility.Visible;
                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESERVE_LOT_LIST_ELEC_SB", "INDATA", "RSLTDT", dt);
                }

                Util.GridSetData(dgCanReserveElec, result, FrameOperation);


                //예약된LOT
                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVED_ELEC", "INDATA", "RSLTDT", dt);
                dgReservedLotElec.ItemsSource = DataTableConverter.Convert(result);

                if (preElec != null) //QA_TRGT_FLAG 해제
                {
                    DataTable tmp = new DataTable();
                    tmp.Columns.Add("LOTID", typeof(string));

                    DataRow tmpRow = null;

                       
                    for (int i = 0; i < preElec.Rows.Count; i++)
                    {
                        for (int j = 0; j < dgCanReserveElec.GetRowCount(); j++)
                        {
                            if (preElec.Rows[i]["CSTID"].Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[j].DataItem, "CSTID"))))
                            {
                                tmpRow = tmp.NewRow();
                                tmpRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[j].DataItem, "LOTID"));
                                tmp.Rows.Add(tmpRow);

                            }
                        }
                    }


                    CancelQaTarget(tmp);

                }



                if (chkWoProduct.IsChecked == true)
                {
                    SetWorkorderProduct();
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
        private void dgReservedLotElec_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink")); 
                } 
               
            }));
        }

        private bool ValidationReserve()
        {
            if (cboVDEquipment.Text.Equals("-ALL-"))
            {
                Util.MessageValidation("SFU1733");
                //Util.Alert("SFU1733");  //예약 할 설비를 선택해주세요
                return false;
            }

          
            if (_Util.GetDataGridCheckFirstRowIndex(dgCanReserveElec, "CHK") == -1)
            {
                Util.MessageValidation("SFU1632");
                //Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                return  false;
            }

            if (dgReservedLotElec.Rows.Count != 0)
            {
                Util.MessageValidation("SFU1735");
                //Util.Alert("SFU1735");  //예약된 LOT이 이미 있습니다.
                return false;
            }

             
          
           
            return true;
        }
        private void AddRow(C1.WPF.DataGrid.C1DataGrid toDatagrid, C1.WPF.DataGrid.C1DataGrid fromDatagrid, int i)
        {


            if (_Unit.Equals("LOT"))//자동차 전극/조립
            {
                if ((toDatagrid.ItemsSource as DataView).ToTable().Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LOTID")) + "'").Count() != 0)
                {
                    Util.MessageValidation("SFU1384");
                    //Util.Alert("SFU1384"); // LOT이 이미 있습니다.
                    return;
                }

                toDatagrid.IsReadOnly = false;
                toDatagrid.BeginNewRow();
                toDatagrid.EndNewRow(true);

                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "RSV_SEQ", (toDatagrid.CurrentCell.Row.Index + 1));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LOTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LOTID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LARGELOT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LARGELOT")));
                //DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PR_LOTID", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "PR_LOTID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "WIPSTAT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPSTAT")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "WIPSNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPSNAME")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "WIPQTY", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPQTY")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "MODLID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "MODLID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRODID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRODID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRODNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRODNAME")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "ELEC", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "ELEC")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "ELECNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "ELECNAME")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "S12", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "S12")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "UNIT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "UNIT")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRJT_NAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRJT_NAME")));
                toDatagrid.IsReadOnly = true;
            }
            else //소형
            {

                if ((toDatagrid.ItemsSource as DataView).ToTable().Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")) + "'").Count() != 0)
                {
                    Util.MessageValidation("SFU3068");//SKIDID가 이미 선택되었습니다.
                                                      //Util.Alert("SKIDID가 이미 선택되었습니다.");
                    return;
                }

                string qa_flag = "";
                DataTable canceldt = new DataTable();
                if (!Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "WIPSTAT")).Equals("TERM")) {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CSTID", typeof(string));

                    DataRow row = dt.NewRow();
                    row["CSTID"] = Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID"));
                    dt.Rows.Add(row);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVE_QA_TRGT_LOT", "INDATA", "RSLTDT", dt);
                    canceldt = result.Copy();
                    if (result.Rows.Count != 0)
                    {
                        if (result.Select("QA_INSP_TRGT_FLAG = 'Y'").Count() != 0)
                        {
                            qa_flag = "Y";
                        }

                    }
                }

                #region # VD 짝맞춤
                if (CommonVerify.HasDataGridRow(toDatagrid))
                {
                    DataTable dt = ((DataView)toDatagrid.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                                     where t.Field<string>("LARGELOT") == Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LARGELOT")) &&
                                           t.Field<string>("QA_INSP_TRGT_FLAG") == "Y"
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        //해당LOT들 QA_INSP_TRGT_FLAG null처리;
                        CancelQaTarget(canceldt);
                        qa_flag = string.Empty;
                    }
                }
                #endregion

                toDatagrid.IsReadOnly = false;
                toDatagrid.BeginNewRow();
                toDatagrid.EndNewRow(true);

                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "RSV_SEQ", (toDatagrid.CurrentCell.Row.Index + 1));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "CSTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRJT_NAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRJT_NAME")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "MODLID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "MODLID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRODID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRODID")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "PRODNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "PRODNAME")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "ELEC", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "ELEC")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "ELECNAME", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "ELECNAME")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "S12", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "S12")));
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "QA_INSP_TRGT_FLAG", qa_flag.Equals("") ? "X" : qa_flag);
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LARGELOT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LARGELOT")));

                toDatagrid.IsReadOnly = true;
            }
        }


        #endregion
        private void chkWoProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            SetWorkorderProduct();
          

        }
        private void SetWorkorderProduct()
        {
            DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER).dgWorkOrder).ItemsSource);
            if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
            {
                // Util.Alert("작업지시를 선택해주세요");
                return;
            }
            string woprodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);//(grdWorkOrder.Children[0] as UC_WORKORDER).PRODID;

            if (chkWoProduct.IsChecked == true)
            {
                try
                {
                    dtAllLot = DataTableConverter.Convert(dgCanReserveElec.ItemsSource);
                    DataTable dt = dtAllLot.Select("PRODID = '" + woprodid + "'").AsEnumerable(). OrderBy(x=> x.Field<string>("VLD_DATE")).CopyToDataTable();
                    Util.GridSetData( dgCanReserveElec, dt, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    
                    dgCanReserveElec.ItemsSource = null;
                    FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //조회된 Data가 없습니다.
                    return;
                }
            }
            else
            {
                Util.GridSetData(dgCanReserveElec, dtAllLot, FrameOperation);
            }
        }

        private void cboVDEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboVDEquipment.Text.Equals("-SELECT-"))
            {
                return;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("EQPTID", typeof(string));

            DataRow row = dt.NewRow();
            row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_VD_INFO", "INDATA", "RSLTDT", dt);
            if (result == null)
            {
                Util.MessageValidation("SFU1672");  //설비 정보가 없습니다.
                return;
            }
            if (result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1672");  //설비 정보가 없습니다.
                return;
            }

            _EquipFlag = Convert.ToString(result.Rows[0]["PRDT_CLSS_CHK_FLAG"]);
            _EquipElec = Convert.ToString(result.Rows[0]["PRDT_CLSS_CODE"]).Equals("") ? null : Convert.ToString(result.Rows[0]["PRDT_CLSS_CODE"]);
            if (result.Rows[0]["WRK_RSV_UNIT_CODE"].ToString().Equals(""))
            {
                Util.MessageValidation("SFU1400");  //MMD에서 VD설비 작업예약단위를 입력해주세요
              
                dgCanReserveElec.ItemsSource = null;
                dgReservedLotElec.ItemsSource = null;

                btnSearch.IsEnabled = false;

                return;
            }
            if (_EquipFlag.Equals("Y") && (_EquipElec == null))
            {
                Util.MessageValidation("SFU1399"); //MMD에서 VD설비 극성을 등록해주세요
                return;
            }
            _Unit = Convert.ToString(result.Rows[0]["WRK_RSV_UNIT_CODE"]);

            if (Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]).Equals(""))
            {
                Util.MessageValidation("SFU1396");   //MMD에 VD설비 작업예약최대개수를 입력해주세요
                 // return;
            }
            _MaxNum = Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]).Equals("") ? 20 : int.Parse(Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]));

         
            dgCanReserveElec.ItemsSource = null;
            dgReservedLotElec.ItemsSource = null;

            btnSearch.IsEnabled = true;

            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboVDEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboVDEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.VD_LMN;

            winWorkOrder.ClearWorkOrderInfo();

            ClearControls();

        }
        private void dgReservedLotElec_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preElec.Content = chkAllElec;
                        e.Column.HeaderPresenter.Content = preElec;
                        chkAllElec.Checked -= new RoutedEventHandler(checkAllElec_Checked);
                        chkAllElec.Unchecked -= new RoutedEventHandler(checkAllElec_Unchecked);
                        chkAllElec.Checked += new RoutedEventHandler(checkAllElec_Checked);
                        chkAllElec.Unchecked += new RoutedEventHandler(checkAllElec_Unchecked);
                    }
                }
            }));
        }
  
        void checkAllElec_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAllElec.IsChecked)
            {
                for (int i = 0; i < dgReservedLotElec.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReservedLotElec.Rows[i].DataItem, "CHK", true);
                }
            }
        }

   
        void checkAllElec_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllElec.IsChecked)
            {

                for (int i = 0; i < dgReservedLotElec.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReservedLotElec.Rows[i].DataItem, "CHK", false);
                }

            }

        }

        private void dgReservedLotElec_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            dgReservedLotElec.Dispatcher.BeginInvoke(new Action(() =>
               {
                   try
                   {
                       if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                       {
                           C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                           CheckBox cb = cell.Presenter.Content as CheckBox;

                           int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                           SetChkCutID(index, dgReservedLotElec);

                       }

                   }
                   catch (Exception ex)
                   {
                       Util.MessageException(ex);
                   }

               }));

        }
        private void SetChkCutID(int index, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("True"))
            {
                //SKID ID 같으면 동시에 선택이 되게
                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    if (index != i)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "CSTID"))) &&
                             (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("True")))
                        {
                            DataTableConverter.SetValue(datagrid.Rows[i].DataItem, "CHK", 1);
                        }
                    }
                }
            }
            else if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("False"))
            {
                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    if (index != i)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "CSTID"))) &&
                             (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("False")))
                        {
                            DataTableConverter.SetValue(datagrid.Rows[i].DataItem, "CHK", 0);
                        }
                    }
                }
            }
        }

        #region[workorder]

        private void SetWorkOrderWindow()
        {

            if (grdWorkOrder.Children.Count == 0)
            {
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
          
        }
        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Convert.ToString(cboVDProcess.SelectedValue);
                sEqsg = cboVDEquipmentSegment.SelectedIndex >= 0 ? Convert.ToString(cboVDEquipmentSegment.SelectedValue) : "";
                sEqpt = cboVDEquipment.SelectedIndex >= 0 ? Convert.ToString(cboVDEquipment.SelectedValue) : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }

        }
     
        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgCanReserveElec);
                Util.gridClear(dgLotListElec);
                Util.gridClear(dgReservedLotElec);
                //예약된 LOT clear

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }
       
        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            winWorkOrder.EQPTID = Convert.ToString(cboVDEquipment.SelectedValue);
            winWorkOrder.PROCID = Convert.ToString(cboVDProcess.SelectedValue);

            winWorkOrder.GetWorkOrder();
        }
        #endregion

        private bool ValidationLot(int index)
        {
            DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER).dgWorkOrder).ItemsSource);
            if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
            {
                Util.MessageValidation("SFU1443");  //작업지시를 선택하세요
                DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                return false;
            }

            string prodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);

           

                string recipe = string.Empty;

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_CONV_RATE_WO_RECIP", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count == 0)
                {
                    //err; 설비 정보 없음
                }

                if (Convert.ToString(result.Rows[0]["RECIPEID"]).Equals("")) //레시피 정보가 없으면 해당 PRODID만 예약 가능
                {
                 
                    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "PRODID")).Equals(prodid))
                    {
                        Util.MessageValidation("SFU2905");
                        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                        return false;
                    }

                  
                        //같은제품만 선택되게
                    List<System.Data.DataRow> list = DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CHK = 1").ToList();
                    list = list.GroupBy(c => c["PRODID"]).Select(group => group.First()).ToList();
                    if (list.Count > 1)
                    {
                        Util.MessageValidation("SFU1480"); //다른 제품을 선택하셨습니다.
                        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                        SetChkCutID(index, dgCanReserveElec);
                        return false;
                    }
                    
                }
                else //레시피 정보가 같은 LOT은 모두 예약 가능
                {
                    recipe = Convert.ToString(result.Rows[0]["RECIPEID"]);
                    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "RECIPEID")).Equals(recipe))
                    {
                        Util.MessageValidation("해당 LOT의 레시피[%1]와 선택된 작업지시의 레시피[%2]가 다릅니다.", Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "RECIPEID")), recipe );
                        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                        return false;
                    }
                }
                

                if (!txtLOTID.Text.Equals(string.Empty))
                {
                    if (dgCanReserveElec.ItemsSource == null)
                    {
                        Util.MessageValidation("SFU1734"); //예약가능 LOT 리스트에 없습니다.
                        txtLOTID.Text = "";
                        return false;
                    }
                    if (dgCanReserveElec.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU1734"); //예약가능 LOT 리스트에 없습니다.
                        txtLOTID.Text = "";
                        return false;
                    }

                    if (DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CSTID = '" + txtLOTID.Text + "'").Count() == 0)
                    {
                        Util.MessageValidation("SFU1734");  //예약가능 LOT 리스트에 없습니다.
                        txtLOTID.Text = "";
                        return false;
                    }
                }
               

                int firstIndex = _Util.GetDataGridCheckFirstRowIndex(dgCanReserveElec, "CHK");
                if (firstIndex == -1)
                {
                    dgLotListElec.IsReadOnly = false;
                    dgLotListElec.RemoveRow(0);
                    dgLotListElec.IsReadOnly = true;
                    return false;
                }

                if (dgCanReserveElec.ItemsSource == null) return false;
                List<System.Data.DataRow> listCut = DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CHK = 1").ToList();
                listCut = listCut.GroupBy(c => c["CSTID"]).Select(group => group.First()).ToList();
                if (listCut.Count > _MaxNum)
                {
                    Util.MessageValidation("SFU1630"); //선택된 CUT의 수가 예약가능 최대설비 수량보다 많습니다.
                    DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                    SetChkCutID(index, dgCanReserveElec);
                    return false;
                }

                ////같은제품만 선택되게
                //List<System.Data.DataRow> list = DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CHK = 1").ToList();
                //list = list.GroupBy(c => c["PRODID"]).Select(group => group.First()).ToList();
                //if (list.Count > 1)
                //{
                //    Util.MessageValidation("SFU1480"); //다른 제품을 선택하셨습니다.
                //    DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //    SetChkCutID(index, dgCanReserveElec);
                //    return false;
                //}
                return true;
        }

        private void dgLotListElec_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink")); ;
                }

            }));
        }

    
    }
}
