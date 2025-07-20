/*************************************************************************************
 Created Date : 2017.06.27
      Creator : 이진선
   Decription : 조립 VD예약
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
namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_003 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        private string _EquipElec = string.Empty;
        private string _EquipFlag = string.Empty; //공통으로 쓰는지
        private string _Unit = string.Empty;
        private int _MaxNum = -1;
        private string _Process = string.Empty;
        int i = 1;
        private UC_WORKORDER_LINE winWo = new UC_WORKORDER_LINE();
        DataTable dtAllLot = null;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty; //투입부
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public ASSY001_003()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;

        }


        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
            SetWorkOrderWindow();
            SetChkVisibile();

            GetLotIdentBasCode();

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                tbCstID.Visibility = Visibility.Visible;
                txtCSTID.Visibility = Visibility.Visible;
                dgCanReserve.Columns["CSTID"].Visibility = Visibility.Visible;
                dgReservedLot.Columns["CSTID"].Visibility = Visibility.Visible;
                dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                tbCstID.Visibility = Visibility.Collapsed;
                txtCSTID.Visibility = Visibility.Collapsed;
                dgCanReserve.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgReservedLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }
        }

        private void SetChkVisibile()
        {
            try
            {
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("USERID", typeof(String));
                dt.Columns.Add("AUTHID",typeof(String));

                DataRow dr = dt.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "DOE_RSV";
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC","INDATA","OUTDATA",dt);
                if(dtResult.Rows.Count > 0)
                    chkIsDoe.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initcombo();
        
            dgCanReserve.Visibility = Visibility.Visible;
            dgReservedLot.Visibility = Visibility.Visible;

            dgLotList.Visibility = Visibility.Visible;
            tbLotID.Visibility = Visibility.Visible;

            chkIsDoe.Visibility = Visibility.Collapsed;
            //chkWoProduct.IsChecked = true;
        }

        #region Initialize

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        #endregion

        #region Event

        #region[LOT예약]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgCanReserve.ItemsSource = null;
            dgReservedLot.ItemsSource = null;


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

             
                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("True"))
                {
                    AddRow(dgLotList, dgCanReserve, index);
                }
                else
                {
                    dgLotList.IsReadOnly = false;

                    //remove
                    for (int i = 0; i < dgLotList.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "LOTID"))))
                        {
                            dgLotList.RemoveRow(i);
                        }
                    }



                    for (int i = 0; i < dgLotList.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "RSV_SEQ", i + 1);
                    }
                    dgLotList.IsReadOnly = true;

                }

              

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

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                indataSet.Tables["IN_EQP"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("RSV_SEQ", typeof(string));
                inLot.Columns.Add("DOE_RSV_FLAG", typeof(string));

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID"));
                    row["RSV_SEQ"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RSV_SEQ"));
                    row["DOE_RSV_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "DOE_RSV_FLAG"));
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

                        dgLotList.ItemsSource = null;
                        initGridTable(dgLotList);

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

               

                for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "LOTID")).Equals(txtLOTID.Text))
                    {
                        DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                        if (!ValidationLot(i))
                        {
                            txtLOTID.Text = "";
                            return;
                        }

                        dgCanReserve.ScrollIntoView(i, 0);

                        AddRow(dgLotList, dgCanReserve, i);

                        txtLOTID.Text = "";
                        return;
                    }
                }
                Util.MessageValidation("SFU1734");
                //Util.Alert("SFU1734");  //예약가능 LOT 리스트에 없습니다.
                txtLOTID.Text = "";

            



            }
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

               
                if (_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632");
                    // Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgReservedLot.ItemsSource);
                if (dt.Rows.Count == 0) return;

                if (dt.Select("CHK = 0").Count() != 0)
                {
                    Util.MessageValidation("LOT 전체 선택 후 진행");//모든 LOT을 선택하세요
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
                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK")].DataItem, "EQPTID"));
                row["USERID"] = LoginInfo.USERID;
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["TERM_FLAG"] = "N";//((bool)chkWaterSpecOut.IsChecked && !_Unit.Equals("LOT")) ? "Y" : "N";
                indataSet.Tables["IN_EQP"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

             
                for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[i].DataItem, "LOTID"));
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
                chkAll.IsChecked = false;
            }
        }

        #endregion

        private void txtCSTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotID = GetLotIDbyCSTID(Util.NVC(txtCSTID.Text), "WAIT");

                    if (!string.IsNullOrEmpty(sLotID))
                    {
                        txtLOTID.Text = sLotID;
                        txtLOTID_KeyUp(txtLOTID, e);
                    }
                    else
                    {
                        txtCSTID.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    txtCSTID.Text = "";
                }
            }
        }

        private void txtCSTID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCSTID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCSTID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuthButton = new List<Button>();
            listAuthButton.Add(btnReserve);
            listAuthButton.Add(btnReserveCancel);
            Util.pageAuth(listAuthButton, FrameOperation.AUTHORITY);
        }

        private void initcombo()
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }
        private void SearchData()
        {
            try
            {
                if (cboVDEquipment.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1673");
                    return;
                }

                GetLotIdentBasCode();

                GetWorkOrder();

            
                initGridTable(dgLotList);
                dgReservedLot.ItemsSource = null;



                DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                string woprodid = dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0 ? null : Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);


                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                dr["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                dr["PRODID"] = woprodid;
                dr["WIPSTAT"] = Wip_State.WAIT;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESERVE_LOT_LIST_FIFO", "INDATA", "RSLTDT", dt);
                Util.GridSetData(dgCanReserve, result, FrameOperation, true);


                //예약된LOT
                dt = null;
                dr = null;

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                dr["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                dr["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                dt.Rows.Add(dr);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVED", "INDATA", "RSLTDT", dt);
                dgReservedLot.ItemsSource = DataTableConverter.Convert(result);

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    tbCstID.Visibility = Visibility.Visible;
                    txtCSTID.Visibility = Visibility.Visible;
                    dgCanReserve.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgReservedLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    tbCstID.Visibility = Visibility.Collapsed;
                    txtCSTID.Visibility = Visibility.Collapsed;
                    dgCanReserve.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgReservedLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["CSTID"].Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private bool ValidationReserve()
        {
            if (cboVDEquipment.Text.Equals("-ALL-"))
            {
                Util.MessageValidation("SFU1733");
                //Util.Alert("SFU1733");  //예약 할 설비를 선택해주세요
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK") == -1)
            {
                Util.MessageValidation("SFU1632");
                // Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            if (dgReservedLot.Rows.Count != 0)
            {
                Util.MessageValidation("SFU1735");
                // Util.Alert("SFU1735");  //예약된 LOT이 이미 있습니다.
                return false;
            }

            //2016.12.19 수정  자동차일때만 설비 극성 존재
            for (int i = 0; i < dgCanReserve.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    if (_EquipElec != null && !Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "ELEC")).Equals(_EquipElec))
                    {
                        Util.MessageValidation("SFU3070", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "LOTID")));
                        //Util.Alert(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "LOTID")) + "와 설비의 극성이 다릅니다.");
                        return false;
                    }

                }
            }

            try
            {
                DataSet ds = new DataSet();
                DataTable IN_EQP = ds.Tables.Add("IN_EQP");
                IN_EQP.Columns.Add("EQPTID", typeof(string));
                IN_EQP.Columns.Add("PRODID", typeof(string));

                DataRow row = IN_EQP.NewRow();
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PRODID"));
                ds.Tables["IN_EQP"].Rows.Add(row);

                DataTable INDATA = ds.Tables.Add("INDATA");
                INDATA.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    DataRow dr = INDATA.NewRow();
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID"));
                    INDATA.Rows.Add(dr);

                }

                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FIFO_WIPDTTM_ED_RANK", "IN_EQP,INDATA", "OUTDATA", ds);
                if (result.Tables["OUTDATA"].Rows.Count == 0)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


            return true;
        }
        private void AddRow(C1.WPF.DataGrid.C1DataGrid toDatagrid, C1.WPF.DataGrid.C1DataGrid fromDatagrid, int i)
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
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "CSTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")));
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

            //DOE생산
            if (chkIsDoe.IsChecked.Value)
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "DOE_RSV_FLAG", "Y");
            else
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "DOE_RSV_FLAG", "N");

            toDatagrid.IsReadOnly = true;
        }

        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.VD_LMN;
                dtRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }

        private string GetLotIDbyCSTID(string sCSTID, string sWipStat)
        {
            try
            {
                string sLotID = "";

                if (string.IsNullOrEmpty(sCSTID))
                {
                    Util.MessageValidation("SFU1244");  //카세트ID를 입력 하세요.
                    return sLotID;
                }
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("CSTID", typeof(string));
                dtRQSTDT.Columns.Add("WIPSTAT", typeof(string));


                DataRow dr = dtRQSTDT.NewRow();
                dr["CSTID"] = Util.NVC(sCSTID);
                dr["WIPSTAT"] = Util.NVC(sWipStat);
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_BY_CSTID", "RQSTDT", "RSLTDT", dtRQSTDT); //CSTID로 LOTID 찾기.

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLotID = dtRslt.Rows[0]["LOTID"].ToString();
                }
                else
                {
                    Util.MessageValidation("100182", new object[] { sCSTID }); //카세트[%1]에 연결된 LOTID 가 존재하지 않습니다.                    
                }

                return sLotID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        #endregion
        //private void chkWoProduct_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender == null) return;

        //    chkWoProduct.IsChecked = true;

        //   // SetWorkorderProduct();


        //}
        //private void SetWorkorderProduct()
        //{
        //    DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
        //    if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
        //    {
        //        return;
        //    }
        //    string woprodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);

        //    //if (chkWoProduct.IsChecked == true)
        //    //{
        //    //    try
        //    //    {
        //    //        dtAllLot = DataTableConverter.Convert(dgCanReserve.ItemsSource);
        //    //        DataTable dt = dtAllLot.Select("PRODID = '" + woprodid + "'").CopyToDataTable();
        //    //        Util.GridSetData(dgCanReserve, dt, FrameOperation, true);
        //    //    }
        //    //    catch (Exception ex)
        //    //    {

        //    //       dgCanReserve.ItemsSource = null;

        //    //       FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //조회된 Data가 없습니다.
        //    //       return;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    Util.GridSetData(dgCanReserve, dtAllLot, FrameOperation);
        //    //}
        //}

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
          
                dgCanReserve.ItemsSource = null;
                dgReservedLot.ItemsSource = null;

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

            
            dgCanReserve.ItemsSource = null;
            dgReservedLot.ItemsSource = null;

            btnSearch.IsEnabled = true;

           

            ClearControls();

        }

        private void dgReservedLot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }


      
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll.IsChecked)
            {

                for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReservedLot.Rows[i].DataItem, "CHK", true);
                }
            }


        }
     

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgReservedLot.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReservedLot.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        #region[workorder]

        private void SetWorkOrderWindow()
        {

            if (grdWorkOrder.Children.Count == 0)
            {
                winWo.FrameOperation = FrameOperation;
                winWo._UCParent = this;
                winWo.PROCID = Process.VD_LMN;
                grdWorkOrder.Children.Add(winWo);
               
            }

        }

        public bool GetSearchConditions(out string procid, out string eqsgid, out string eqptid)
        {
            try
            {
                procid = Process.VD_LMN;
                eqsgid = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                eqptid = Convert.ToString(cboVDEquipment.SelectedValue);

                return true;
            }
            catch (Exception ex)
            {
                procid = "";
                eqsgid = "";
                eqptid = "";

                return false;
                throw ex;

            }

        }


        //public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        //{
        //    try
        //    {
        //        sProc = Convert.ToString(cboVDProcess.SelectedValue);
        //        sEqsg = cboVDEquipmentSegment.SelectedIndex >= 0 ? Convert.ToString(cboVDEquipmentSegment.SelectedValue) : "";
        //        sEqpt = cboVDEquipment.SelectedIndex >= 0 ? Convert.ToString(cboVDEquipment.SelectedValue) : "";

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        sProc = "";
        //        sEqsg = "";
        //        sEqpt = "";
        //        return false;
        //        throw ex;
        //    }

        //}

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgCanReserve);
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
            if (winWo == null)
                return;

            winWo.EQPTSEGMENT = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            winWo.EQPTID = Convert.ToString(cboVDEquipment.SelectedValue);
            winWo.PROCID = Process.VD_LMN;

            winWo.GetWorkOrder();
        }
        #endregion

        private bool ValidationLot(int index)
        {
            DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
            if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
            {
                Util.MessageValidation("SFU1443");  //작업지시를 선택하세요
                DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                return false;
            }

            string prodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);
            string woid = Convert.ToString(dgWorkOrder.Rows[0]["WOID"]);

            if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")).Equals(prodid))
            {
                Util.MessageValidation("SFU2905");
                DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                return false;
            }

            int firstIndex = _Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK");
            if (firstIndex == -1)
            {
                dgLotList.IsReadOnly = false;
                dgLotList.RemoveRow(0);
                dgLotList.IsReadOnly = true;
                return false;
            }

            if (dgLotList.Rows.Count != 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")).Equals(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PRODID"))))
                    {
                        Util.MessageValidation("SFU1656"); //선택한 LOTID의 제품코드가 다릅니다.
                        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                        return false;
                    }
                }
            }

            DataTable dt = DataTableConverter.Convert(dgCanReserve.ItemsSource);
            if (dt == null)
            {
                return false;
            }
            if (dt.Select("CHK = '1'").Count() > _MaxNum)
            {
                Util.MessageValidation("SFU1658", _MaxNum);//선택한 LOT의 개수가 {0}을 넘을 수 없습니다.
                DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);

                return false;

            }


            try
            {
                DataSet ds = new DataSet();
                DataTable IN_EQP = ds.Tables.Add("IN_EQP");
                IN_EQP.Columns.Add("EQPTID", typeof(string));
                IN_EQP.Columns.Add("PRODID", typeof(string));

                DataRow row = IN_EQP.NewRow();
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["PRODID"] = prodid;
                ds.Tables["IN_EQP"].Rows.Add(row);

                DataTable INDATA = ds.Tables.Add("INDATA");
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "LOTID"));
                INDATA.Rows.Add(dr);

                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FIFO_WIPDTTM_ED_RANK", "IN_EQP,INDATA", "OUTDATA", ds);
                if (result.Tables["OUTDATA"].Rows.Count == 0)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                Util.MessageException(ex);
            }
            
            //DataSet ds = new DataSet();
            //DataTable IN_EQP = ds.Tables.Add("IN_EQP");
            //IN_EQP.Columns.Add("EQPTID", typeof(string));
            //IN_EQP.Columns.Add("WOID", typeof(string));
            //IN_EQP.Columns.Add("PRODID", typeof(string));

            //DataRow row = IN_EQP.NewRow();
            //row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
            //row["WOID"] = woid;
            //row["PRODID"] = prodid;
            //ds.Tables["IN_EQP"].Rows.Add(row);

            //DataTable INDATA = ds.Tables.Add("INDATA");
            //INDATA.Columns.Add("LOTID", typeof(string));

            //DataRow dr = INDATA.NewRow();
            //dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "LOTID"));
            //INDATA.Rows.Add(dr);

           


            return true;

        
        }

        private void dgCanReserve_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PASS_YN")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightblue")); ;
                }

            }));
        }

        private void btnTotalLotList_Click(object sender, RoutedEventArgs e)
        {
            //대기 리스트 조회

            ASSY001_003_WAITLOT wndWait = new ASSY001_003_WAITLOT();
            wndWait.FrameOperation = FrameOperation;

            if (wndWait != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = cboVDEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = _Unit == null ? "LOT" : _Unit;
                C1WindowExtension.SetParameters(wndWait, Parameters);

                wndWait.Closed += new EventHandler(wndWait_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
                grdMain.Children.Add(wndWait);
                wndWait.BringToFront();
            }
        }
        private void wndWait_Closed(object sender, EventArgs e)
        {
            ASSY001_003_WAITLOT window = sender as ASSY001_003_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }        
    }
}
