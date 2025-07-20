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
    public partial class ASSY001_037 : UserControl, IWorkArea
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


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public ASSY001_037()
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

        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initcombo();

        
            dgCanReserve.Visibility = Visibility.Visible;
            dgReservedLot.Visibility = Visibility.Visible;

            dgLotList.Visibility = Visibility.Visible;
            tbLotID.Visibility = Visibility.Visible;

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



                SetChkCutID(index, dgCanReserve);

                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CHK")).Equals("True"))
                {
                    AddRow(dgLotList, dgCanReserve, index);

                    for(int i = 0; i<dgCanReserve.Rows.Count; i++)
                    {
                        if(i != index && Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CSTID"))))
                        {
                            AddRow(dgLotList, dgCanReserve, i);
                        }
                    }

                }
                else
                {
                    dgLotList.IsReadOnly = false;

                    //remove
                    for (int i = dgLotList.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CSTID"))))
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


                List<System.Data.DataRow> list = DataTableConverter.Convert(dgLotList.ItemsSource).Select().ToList();
                list = list.GroupBy(c => c["CSTID"]).Select(group => group.First()).ToList();

                for (int i = 0; i < list.Count(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = list[i]["CSTID"].ToString();
                    row["RSV_SEQ"] = list[i]["RSV_SEQ"].ToString();
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
        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                bool searchFlag = false;

                for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CSTID")).Equals(txtLOTID.Text))
                    {
                        DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                        if (!ValidationLot(i))
                        {
                            txtLOTID.Text = "";
                            return;
                        }

                        dgCanReserve.ScrollIntoView(i, 0);

                        AddRow(dgLotList, dgCanReserve, i);

                        searchFlag = true;

                      //  txtLOTID.Text = "";
                      //  return;
                    }
                }

                if(!searchFlag)
                {
                    Util.MessageValidation("SFU1734");
                    //Util.Alert("SFU1734");  //예약가능 LOT 리스트에 없습니다.
                    txtLOTID.Text = "";
                }
                else
                { 
                    txtLOTID.Text = "";
                }

            



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

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESERVE_LOT_LIST_CWA", "INDATA", "RSLTDT", dt);
                Util.GridSetData(dgCanReserve, result, FrameOperation, true);



                //예약가능LOT
                //dt = new DataTable();
                //dt.Columns.Add("LANGID", typeof(string));
                //dt.Columns.Add("EQSGID", typeof(string));
                //dt.Columns.Add("PROCID", typeof(string));
                //dt.Columns.Add("ELEC", typeof(string));

                //row = dt.NewRow();
                //row["LANGID"] = LoginInfo.LANGID;
                //row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                //row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                //row["ELEC"] = _EquipFlag.Equals("N") ? null : _EquipElec;
                //dt.Rows.Add(row);

                //result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESERVE_LOT_LIST_FIFO", "INDATA", "RSLTDT", dt);
                //Util.GridSetData(dgCanReserve, result, FrameOperation, true);


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

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVED_ELEC", "INDATA", "RSLTDT", dt);
                dgReservedLot.ItemsSource = DataTableConverter.Convert(result);

                //if (chkWoProduct.IsChecked == true)
                //{
                //    SetWorkorderProduct();
                //}
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

                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "CSTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")));
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

            List<System.Data.DataRow> listCut = DataTableConverter.Convert(dgCanReserve.ItemsSource).Select("CHK = 1").ToList();
            listCut = listCut.GroupBy(c => c["CSTID"]).Select(group => group.First()).ToList();
            if (listCut.Count > _MaxNum)
            {
                Util.MessageValidation("SFU4477"); //선택된 SKID의 수가 예약가능 최대설비 수량보다 많습니다.
                DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                SetChkCutID(index, dgCanReserve);
                return false;
            }

            /*
            if (dt.Select("CHK = '1'").Count() > _MaxNum)
            {
                Util.MessageValidation("SFU1658", _MaxNum);//선택한 LOT의 개수가 {0}을 넘을 수 없습니다.
                DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);

                return false;

            }
            */

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



        private void btnSkidConfig_Click(object sender, RoutedEventArgs e)
        {
            //대기 리스트 조회

            ASSY001_037_SKDPNMAPPING wndSkidConfig = new ASSY001_037_SKDPNMAPPING();
            wndSkidConfig.FrameOperation = FrameOperation;

            if (wndSkidConfig != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = cboVDEquipmentSegment.SelectedValue.ToString();
                C1WindowExtension.SetParameters(wndSkidConfig, Parameters);

                wndSkidConfig.Closed += new EventHandler(wndSkidConfig_Closed);

                grdMain.Children.Add(wndSkidConfig);
                wndSkidConfig.BringToFront();
            }
        }

        private void wndSkidConfig_Closed(object sender, EventArgs e)
        {
            ASSY001_037_SKDPNMAPPING window = sender as ASSY001_037_SKDPNMAPPING;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }
        private void btnQTYPerSkid_Click(object sender, RoutedEventArgs e)
        {
            //대기 리스트 조회

            ASSY001_037_SKID2PANCAKE_QTY wndQTYPERSKID = new ASSY001_037_SKID2PANCAKE_QTY();
            wndQTYPERSKID.FrameOperation = FrameOperation;

            if (wndQTYPERSKID != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = cboVDEquipmentSegment.SelectedValue.ToString();
                C1WindowExtension.SetParameters(wndQTYPERSKID, Parameters);

                wndQTYPERSKID.Closed += new EventHandler(wndQTYPerSkid_Closed);

                grdMain.Children.Add(wndQTYPERSKID);
                wndQTYPERSKID.BringToFront();
            }
        }

        private void wndQTYPerSkid_Closed(object sender, EventArgs e)
        {
            ASSY001_037_SKID2PANCAKE_QTY window = sender as ASSY001_037_SKID2PANCAKE_QTY;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }
    }

   
}
