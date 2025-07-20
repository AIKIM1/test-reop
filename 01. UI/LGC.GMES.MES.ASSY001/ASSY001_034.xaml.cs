/*************************************************************************************
 Created Date : 2017.10.10
      Creator : ������
   Decription : ���� ������ VD����
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.10  ������S : Initial Created.




 
**************************************************************************************/


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
    public partial class ASSY001_034 : UserControl, IWorkArea
    {

        Util _Util = new Util();
        private UC_WORKORDER_LINE winWo = new UC_WORKORDER_LINE();
        CommonCombo combo = new CommonCombo();

        private string _Process;
        private string _EquipElec;
        private string _EquipFlag;
        private string _Unit;
        private string _EQPT_ONLINE_FLAG;
        private string _VD_WRK_COND_GR_CODE = string.Empty;
        private string _PRODID = string.Empty;
        private int _MaxNum = -1;
        DataTable dtAllLot;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_034()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();
            SetWorkOrderWindow();

            initcombo();
        }
        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            //����
            String[] sFilter = { cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            _Process = Convert.ToString(cboVDProcess.SelectedValue);
        }
        private void cboVDProcess_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                //����
                String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
                combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                InitializeControls();

                Util.gridClear(dgCanReserve);
                Util.gridClear(dgLotList);
                Util.gridClear(dgReservedLot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboVDEquipment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (winWo != null)
                {
                    winWo.EQPTSEGMENT = cboVDEquipmentSegment.SelectedValue != null ? Convert.ToString(cboVDEquipmentSegment.SelectedValue) : "";
                    winWo.EQPTID = cboVDEquipment.SelectedValue != null ? Convert.ToString(cboVDEquipment.SelectedValue) : "";
                    winWo.PROCID = cboVDProcess.SelectedValue != null ? Convert.ToString(cboVDProcess.SelectedValue) : "";

                    winWo.ClearWorkOrderInfo();
                }

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
                    Util.MessageValidation("SFU1672");  //���� ������ �����ϴ�.
                    return;
                }
                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1672");  //���� ������ �����ϴ�.
                    return;
                }

                _EquipFlag = Convert.ToString(result.Rows[0]["PRDT_CLSS_CHK_FLAG"]);
                _EquipElec = Convert.ToString(result.Rows[0]["PRDT_CLSS_CODE"]).Equals("") ? null : Convert.ToString(result.Rows[0]["PRDT_CLSS_CODE"]);

                if (_EquipFlag.Equals("Y") && (_EquipElec == null))
                {
                    Util.MessageValidation("SFU1399"); //MMD���� VD���� �ؼ��� ������ּ���
                    return;
                }

                //if (result.Rows[0]["WRK_RSV_UNIT_CODE"].ToString().Equals(""))
                //{
                //    Util.MessageValidation("SFU1400");  //MMD���� VD���� �۾���������� �Է����ּ���

                //    dgCanReserve.ItemsSource = null;
                //    dgReservedLot.ItemsSource = null;

                //    btnSearch.IsEnabled = false;

                //    return;
                //}

                //_Unit = Convert.ToString(result.Rows[0]["WRK_RSV_UNIT_CODE"]);

                //if (_Unit.Equals("LOT"))//�Ŀ�ġ
                //{
                //    SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                //    SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                //    LOTID_LOTLIST.Visibility = Visibility.Visible;
                //    SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                //    tbSKIDID.Visibility = Visibility.Collapsed;
                //    tbLotID.Visibility = Visibility.Visible;
                //}
                //else //������
                //{
                //    SKIDID_CANRESERVE.Visibility = Visibility.Visible;
                //    SKIDID_LOTLIST.Visibility = Visibility.Visible;
                //    LOTID_LOTLIST.Visibility = Visibility.Collapsed;
                //    SKIDID_RESERVED.Visibility = Visibility.Visible;

                //    tbSKIDID.Visibility = Visibility.Visible;
                //    tbLotID.Visibility = Visibility.Collapsed;
                //}


                _EQPT_ONLINE_FLAG = "Y";

                if (!result.Rows[0]["EQPT_ONLINE_FLAG"].ToString().Equals(""))//������ �⺻ �¶���
                {
                    _EQPT_ONLINE_FLAG = result.Rows[0]["EQPT_ONLINE_FLAG"].ToString();

                }

                if (_EQPT_ONLINE_FLAG.Equals("N")) //���������϶� ���ϰ� ���� ����
                {
                    btnReserve.Visibility = Visibility.Collapsed;
                    btnReserveStart.Visibility = Visibility.Visible;

                    btnReserveCancel.Visibility = Visibility.Collapsed;
                    btnRunCancel.Visibility = Visibility.Visible;

                }
                else
                {
                    btnReserve.Visibility = Visibility.Visible;
                    btnReserveStart.Visibility = Visibility.Collapsed;

                    btnReserveCancel.Visibility = Visibility.Visible;
                    btnRunCancel.Visibility = Visibility.Collapsed;
                }


                if (Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]).Equals(""))
                {
                    Util.MessageValidation("SFU1396");   //MMD�� VD���� �۾������ִ밳���� �Է����ּ���
                }
                _MaxNum = Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]).Equals("") ? 20 : int.Parse(Convert.ToString(result.Rows[0]["WRK_RSV_MAX_QTY"]));

                Util.gridClear(dgCanReserve);
                Util.gridClear(dgReservedLot);
                Util.gridClear(dgLotList);
                //dgCanReserve.ItemsSource = null;
                //dgReservedLot.ItemsSource = null;
                //dgLotList.ItemsSource = null;

                btnSearch.IsEnabled = true;



                ClearControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void chkWoProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            SetWorkorderProduct();
        }

        private void txtLOTID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetInfo();
                //for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ?  "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                //    {
                //        DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                //        if (!ValidationLot(i))
                //        {
                //            txtLOTID.Text = "";
                //            return;
                //        }

                //        if (!_Unit.Equals("LOT"))
                //        {
                //            SetChkCutID(i, dgCanReserve);
                //        }

                //        dgCanReserve.ScrollIntoView(i, 0);

                //        AddRow(dgLotList, dgCanReserve, i);

                //        txtLOTID.Text = "";
                //        return;
                //    }
                //}
                //Util.MessageValidation("SFU1734"); //���డ�� LOT ����Ʈ�� �����ϴ�.
                //txtLOTID.Text = "";

            }
        }

        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dgCanReserve.ItemsSource = null;
            dgReservedLot.ItemsSource = null;

            SearchData();
        }
        private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
                if (!ValidationLot(index)) return;

                if (!_Unit.Equals("LOT"))
                {
                    SetChkCutID(index, dgCanReserve);
                }

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
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID"))))
                        {
                            dgLotList.RemoveRow(i);

                        }
                    }
                    

                    for (int i = 0; i < dgLotList.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "RSV_SEQ", i + 1);
                    }

                    dgLotList.IsReadOnly = true;

                    if (!_Unit.Equals("LOT"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgCanReserve.ItemsSource).Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CSTID")) + "'").Count() == 0 ? null : DataTableConverter.Convert(dgCanReserve.ItemsSource).Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "CSTID")) + "'").CopyToDataTable();
                        if (dt == null) return;
                        if (dt.Rows.Count == 0) return;
                    }

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
                inData.Columns.Add("EQPT_ONLINE_FLAG", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQPT_ONLINE_FLAG"] = _EQPT_ONLINE_FLAG;
                indataSet.Tables["IN_EQP"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("RSV_SEQ", typeof(string));

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID"));
                    row["RSV_SEQ"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RSV_SEQ"));
                    indataSet.Tables["IN_LOT"].Rows.Add(row);

                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RESERVE_LOT_VD_NJ_BR", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
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
                // Util.MessageException(ex);   //���� ����
            }
        }
        private void btnReserveCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {

                if (_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632"); //���õ� LOT�� �����ϴ�.
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgReservedLot.ItemsSource);
                if (dt.Rows.Count == 0) return;

                if (dt.Select("CHK = 0").Count() != 0)
                {
                    Util.MessageValidation("SFU3202");//��� LOT�� �����ϼ���
                    return;
                }


                if (Convert.ToString(cboVDEquipment.SelectedValue).Equals(""))
                {
                    Util.MessageValidation("SFU1732");
                    //Util.Alert("SFU1732");  //���� ��� �� ���� �������ּ���.
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
                row["TERM_FLAG"] = ((bool)chkWaterSpecOut.IsChecked && !_Unit.Equals("LOT")) ? "Y" : "N";
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
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK") == -1)
            {
                //Util.Alert("���õ� LOT�� �����ϴ�.");
                Util.MessageValidation("SFU1661");
                return;
            }
            if (!Util.NVC(DataTableConverter.GetValue(dgReservedLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReservedLot, "CHK")].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("�۾����� �� ���� ��� �� �� �ֽ��ϴ�.");
                Util.MessageValidation("SFU3035");
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("SRCTYPE", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("USERID", typeof(string));
            dt.Columns.Add("IFMODE", typeof(string));



            DataRow row = dt.NewRow();
            DataTable dtProductLot = DataTableConverter.Convert(dgReservedLot.ItemsSource);

            foreach (DataRow _iRow in dtProductLot.Rows)
            {

                if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                {
                    row = dt.NewRow();
                    row["LOTID"] = _iRow["LOTID"];
                    row["SRCTYPE"] = "UI";
                    row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                    row["USERID"] = LoginInfo.USERID;
                    row["IFMODE"] = "OFF";
                    dt.Rows.Add(row);
                }

            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_START_LOT_VD", "RQSTDT", null, dt);
                Util.MessageInfo("SFU1839");
                SearchData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        #region[Method]
        private void SetChkCutID(int index, C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "CHK")).Equals("True"))
            {
                //SKID ID ������ ���ÿ� ������ �ǰ�
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

        private void initcombo()
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReserve);
            listAuth.Add(btnReserveCancel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {

            if (grdWorkOrder.Children.Count == 0)
            {
                winWo.FrameOperation = FrameOperation;
                winWo._UCParent = this;
                winWo.PROCID = Convert.ToString(cboVDProcess.SelectedValue);
                grdWorkOrder.Children.Add(winWo);

            }

        }
        public bool GetSearchConditions(out string procid, out string eqsgid, out string eqptid)
        {
            try
            {
                procid = Convert.ToString(cboVDProcess.SelectedValue);//Process.VD_LMN;
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

        private void AddRow(C1.WPF.DataGrid.C1DataGrid toDatagrid, C1.WPF.DataGrid.C1DataGrid fromDatagrid, int i)
        {

            if (_Unit.Equals("LOT"))
            {

                if ((toDatagrid.ItemsSource as DataView).ToTable().Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LOTID")) + "'").Count() != 0)
                {
                    Util.MessageValidation("SFU1384"); //LOT�� �̹� �ֽ��ϴ�.
                    return;
                }
            }
            else
            {
                if ((toDatagrid.ItemsSource as DataView).ToTable().Select("CSTID = '" + Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")) + "'").Count() != 0)
                {
                    Util.MessageValidation("SFU3068");//SKIDID�� �̹� ���õǾ����ϴ�.
                    return;
                }
            }

            toDatagrid.IsReadOnly = false;
            toDatagrid.BeginNewRow();
            toDatagrid.EndNewRow(true);

            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "RSV_SEQ", (toDatagrid.CurrentCell.Row.Index + 1));
            if (_Unit.Equals("LOT"))
            {
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LOTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LOTID")));
            }
            else
            {
                DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "CSTID", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "CSTID")));
            }
            
            DataTableConverter.SetValue(toDatagrid.CurrentRow.DataItem, "LARGELOT", Util.NVC(DataTableConverter.GetValue(fromDatagrid.Rows[i].DataItem, "LARGELOT")));
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

        private bool ValidationLot(int index)
        {
            try
            {
                //DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                //if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
                //{
                //    Util.MessageValidation("SFU1443");  //�۾����ø� �����ϼ���
                //    DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                //    return false;
                //}

                //string prodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);

                //string _VD_WRK_COND_GR_CODE = string.Empty;

                DataTable dt = new DataTable();
                //dt.Columns.Add("EQPTID", typeof(string));

                //DataRow row = dt.NewRow();
                //row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                //dt.Rows.Add(row);

                //DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_CONV_RATE_WO_RECIP", "INDATA", "RSLTDT", dt);

                //if (result.Rows.Count == 0)
                //{
                //    //err; ���� ���� ����
                //}

                //if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")).Equals(prodid))
                //{
                //    _VD_WRK_COND_GR_CODE = Convert.ToString(result.Rows[0]["VD_WRK_COND_GR_CODE"]);
                //    if (_VD_WRK_COND_GR_CODE.Equals(""))
                //    {
                //        Util.MessageValidation("PRODID�� �ٸ��ϴ�.");
                //        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                //        return false;
                //    }
                //    else
                //    {
                //        if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")).Equals(_VD_WRK_COND_GR_CODE))
                //        {
                //            Util.MessageValidation("�ش� LOT�� ������[%1]�� ���õ� �۾������� ������[%2]�� �ٸ��ϴ�.", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")), _VD_WRK_COND_GR_CODE);
                //            DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                //            return false;
                //        }

                //    }
                //}

                #region ���� �ּ� ����
                //if (Convert.ToString(result.Rows[0]["RECIPEID"]).Equals("")) //������ ������ ������ �ش� PRODID�� ���� ����
                //{
                //    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "PRODID")).Equals(prodid))
                //    {
                //        Util.MessageValidation("SFU2905");
                //        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //        return false;
                //    }
                //    //������ǰ�� ���õǰ�
                //    List<System.Data.DataRow> list = DataTableConverter.Convert(dgCanReserveElec.ItemsSource).Select("CHK = 1").ToList();
                //    list = list.GroupBy(c => c["PRODID"]).Select(group => group.First()).ToList();
                //    if (list.Count > 1)
                //    {
                //        Util.MessageValidation("SFU1480"); //�ٸ� ��ǰ�� �����ϼ̽��ϴ�.
                //        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //        SetChkCutID(index, dgCanReserveElec);
                //        return false;
                //    }

                //}
                //else //������ ������ ���� LOT�� ��� ���� ����
                //{
                //    recipe = Convert.ToString(result.Rows[0]["RECIPEID"]);
                //    if (!Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "RECIPEID")).Equals(recipe))
                //    {
                //        Util.MessageValidation("�ش� LOT�� ������[%1]�� ���õ� �۾������� ������[%2]�� �ٸ��ϴ�.", Util.NVC(DataTableConverter.GetValue(dgCanReserveElec.Rows[index].DataItem, "RECIPEID")), recipe);
                //        DataTableConverter.SetValue(dgCanReserveElec.Rows[index].DataItem, "CHK", 0);
                //        return false;
                //    }
                //}
                #endregion



                int firstIndex = _Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK");
                if (firstIndex == -1)
                {
                    dgLotList.IsReadOnly = false;
                    dgLotList.RemoveRow(0);
                    dgLotList.IsReadOnly = true;
                    return false;
                }


                dt = null;
                dt = DataTableConverter.Convert(dgCanReserve.ItemsSource);

                if (dt == null)
                {
                    return false;
                }
                if (dt.Select("CHK = '1'").Count() > _MaxNum)
                {
                    Util.MessageValidation("SFU1658", _MaxNum);//������ LOT�� ������ {0}�� ���� �� �����ϴ�.
                    DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);

                    return false;
                }


                /*
                 
                (1) ���� ��� �Է�
                   1) ù �۾������ Scan
                      - ù ��ĵ ��� ������ LOT, SKID�� ���� ���� Scan ��� ������ ��� Scan ����
                      - ù ��ĵ ��� ���� W/O ���� ���� üũ �� W/O ���� ó�� ǥ�� 
                   2) ù��° ���� �۾���� Scan
                      - ù��° ����� ����(LOT, SKID) �� ���� VD �۾��׷쳻�� �ش��ϰ� W/O �����ϴ� �͸� ��� ���� ����
                      - ù��° ����� Cell Type(����, ����, �ʼ���, �Ŀ�ġ)�� ��ġ�ϴ� �͸� ���� ����
                  �� ���� �۾����� ���� �� ���� ó�� ������ ������ ��
                  �� VD �۾��׷��� ��ϵ��� ���� ���� VD �׷��� ��ϵǾ� �ִ� �Ͱ� ���� �۾��� �� ����
                    
                */

                bool bExistRsv = false;

                // W/O ���� ���� üũ �� W/O ���� ó�� ǥ��
                DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
                DataRow[] dtWorkOrderRow = dgWorkOrder.Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")) + "'");

                if (dtWorkOrderRow.Length == 0)
                {
                    Util.MessageValidation("SFU4249");  //���� ������ �۾����� ������ �����ϴ�.
                    DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                    return false;
                }

                // ó�� Scan �� ��쿡�� W/O �ڵ� ���� ó��....
                if (dgLotList.Rows.Count < 1)
                {
                    if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
                    {
                        // �۾����ð� ���� ��� �ڵ� ���� ó��.
                        winWo.SetWorkOrderForVD(Util.NVC(dtWorkOrderRow[0]["WO_DETL_ID"]), true, false);

                        GetWorkOrder();
                    }
                    else
                    {
                        // �������� Lot ���翩�� Ȯ��
                        if (CheckReserveLot())
                        {
                            //Util.MessageValidation("SFU1917");  //�������� LOT�� �ֽ��ϴ�.
                            //DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                            //return false;

                            bExistRsv = true;
                        }
                        else
                        {
                            // ���� lot ������ �۾����� �ڵ� ���� ó��.
                            winWo.SetWorkOrderForVD(Util.NVC(dtWorkOrderRow[0]["WO_DETL_ID"]), true, false);

                            GetWorkOrder();
                        }
                    }

                    //GetWorkOrder();
                }
                

                // �۾��׷�(������) ���� Ȯ��
                DataTable dtSel = new DataTable();
                dtSel.Columns.Add("EQPTID", typeof(string));

                DataRow row = dtSel.NewRow();
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                dtSel.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_CONV_RATE_WO_RECIP", "INDATA", "RSLTDT", dtSel);

                if (result.Rows.Count == 0)
                {
                    //err; ���� ���� ����
                }
                
                // ù��° Scan
                if (dgLotList.Rows.Count < 1)
                {
                    //// W/O ���� ó�� ǥ��
                    //SetSelectWorkOrder(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID")));
                    
                    if (!bExistRsv)
                        _VD_WRK_COND_GR_CODE = Convert.ToString(result.Rows[0]["VD_WRK_COND_GR_CODE"]);
                    else
                    {
                        // ���� �۾��׷� Ȯ��
                        if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")).Equals(_VD_WRK_COND_GR_CODE))
                        {
                            Util.MessageValidation("SFU4250", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")), _VD_WRK_COND_GR_CODE);   // �ش� LOT�� ������[%1]�� ���õ� �۾������� ������[%2]�� �ٸ��ϴ�.
                            DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                            return false;
                        }
                    }

                    // ù��° ���� ��ǰ ID
                    _PRODID = Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID"));
                }
                else
                {
                    // ù Scan �� �׷� ������ ���� ��쿡�� ���� ��ǰ�� ���� ����.
                    //if (Util.NVC(_VD_WRK_COND_GR_CODE).Trim().Equals(""))
                    //{
                    //    if (!_PRODID.Equals(Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "PRODID"))))
                    //    {
                    //        Util.MessageValidation("SFU2929");  // ������ ��ĵ�� LOT�� ��ǰ�ڵ�� �ٸ��ϴ�.
                    //        DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                    //        return false;
                    //    }
                    //}
                    //else
                    //{
                        // ���� �۾��׷� Ȯ��
                        if (!Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")).Equals(_VD_WRK_COND_GR_CODE))
                        {
                            Util.MessageValidation("SFU4250", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[index].DataItem, "VD_WRK_COND_GR_CODE")), _VD_WRK_COND_GR_CODE);   // �ش� LOT�� ������[%1]�� ���õ� �۾������� ������[%2]�� �ٸ��ϴ�.
                            DataTableConverter.SetValue(dgCanReserve.Rows[index].DataItem, "CHK", 0);
                            return false;
                        }
                    //}

                    // ù��° ����� Cell Type(����, ����, �ʼ���, �Ŀ�ġ)�� ��ġ�ϴ� �͸� ���� ����
                    

                }


                return true;

            } catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        private void SearchData()
        {
            try
            {
                DataTable dt = null;
                DataRow row = null;
                DataTable result = null;

                if (cboVDEquipment.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1673");
                    return;
                }

                GetWorkOrder();


                initGridTable(dgLotList);
                dgReservedLot.ItemsSource = null;

                //���డ��LOT
                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("ELEC", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["ELEC"] = _EquipFlag.Equals("N") ? null : _EquipElec;
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync(Util.NVC(cboVDProcess.SelectedValue).Equals("A6000") ? "DA_PRD_SEL_RESERVE_LOT_LIST_NJ_PU" : "DA_PRD_SEL_RESERVE_LOT_LIST_NJ", "INDATA", "RSLTDT", dt);
                //result = new ClientProxy().ExecuteServiceSync(_Unit.Equals("LOT") ? "DA_PRD_SEL_RESERVE_LOT_LIST_NJ_PU" : "DA_PRD_SEL_RESERVE_LOT_LIST_NJ", "INDATA", "RSLTDT", dt);
                Util.GridSetData(dgCanReserve, result, FrameOperation, true);


                //�����LOT
                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("EQPT_ONLINE_FLAG_FLAG", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                row["EQPT_ONLINE_FLAG_FLAG"] = _EQPT_ONLINE_FLAG;
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_RESERVED_NJ", "INDATA", "RSLTDT", dt);
                dgReservedLot.ItemsSource = DataTableConverter.Convert(result);

                if (chkWoProduct.IsChecked == true)
                {
                    SetWorkorderProduct();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void SetWorkorderProduct()
        {
            DataTable dgWorkOrder = DataTableConverter.Convert(((grdWorkOrder.Children[0] as UC_WORKORDER_LINE).dgWorkOrder).ItemsSource);
            if (dgWorkOrder.Select("EIO_WO_SEL_STAT = 'Y'").Count() == 0)
            {
                return;
            }

            string woprodid = Convert.ToString(dgWorkOrder.Rows[0]["PRODID"]);

            if (chkWoProduct.IsChecked == true)
            {
                try
                {
                    dtAllLot = DataTableConverter.Convert(dgCanReserve.ItemsSource);
                    DataTable dt = dtAllLot.Select("PRODID = '" + woprodid + "'").CopyToDataTable();
                    Util.GridSetData(dgCanReserve, dt, FrameOperation, true);
                }
                catch (Exception ex)
                {

                    dgCanReserve.ItemsSource = null;

                    FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //��ȸ�� Data�� �����ϴ�.
                    return;
                }
            }
            else
            {
                Util.GridSetData(dgCanReserve, dtAllLot, FrameOperation);
            }
        }
        private void GetWorkOrder()
        {
            if (winWo == null)
                return;

            winWo.EQPTSEGMENT = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            winWo.EQPTID = Convert.ToString(cboVDEquipment.SelectedValue);
            winWo.PROCID = Convert.ToString(cboVDProcess.SelectedValue);

            winWo.GetWorkOrder();
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

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgCanReserve);
                //����� LOT clear

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private bool ValidationReserve()
        {
            if (cboVDEquipment.Text.Equals("-ALL-"))
            {
                Util.MessageValidation("SFU1733"); //���� �� ���� �������ּ���
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgCanReserve, "CHK") == -1)
            {
                Util.MessageValidation("SFU1632"); //���õ� LOT�� �����ϴ�.
                return false;
            }

            if (dgReservedLot.Rows.Count != 0)
            {
                Util.MessageValidation("SFU1735"); //����� LOT�� �̹� �ֽ��ϴ�.
                return false;
            }

            for (int i = 0; i < dgCanReserve.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    if (_EquipElec != null && !Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "ELEC")).Equals(_EquipElec))
                    {
                        Util.MessageValidation("SFU3070", Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, "LOTID"))); // [%1]�� ������ �ؼ��� �ٸ��ϴ�.
                        return false;
                    }

                }
            }


            return true;
        }



        private void dgReservedLot_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            if (_Unit.Equals("LOT"))
            {
                return;
            }

            dgReservedLot.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                        SetChkCutID(index, dgReservedLot);

                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        private void SetInfo()
        {
            try
            {
                if (Util.NVC(cboVDEquipment.Text).IndexOf("SELECT") >= 0 || Util.NVC(cboVDEquipment.Text).Equals(""))
                {
                    Util.MessageValidation("SFU1673");
                    txtLOTID.Text = "";
                    return;
                }

                /*
                 * 1. LOT ��ĵ ù��° ��ĵ �̸� ���� Ȯ���� ȭ�� ���� �ڵ� ���� (���� ���� ���δ� �ϴ��� ���� DATA ����� Ȯ��)
                 * 2. ������ �ٲ�� LOT/SKID VIEW ���� �� ��ȸ DATA CLEAR
                 * 3. ������ �����ߴ� ���� �ڵ� ����
                 * 4. �ش� ���� ���� DATA ��ȸ
                 * 5. ��ĵ�� DATA �ڵ� ����
                 * 6. �����ϸ� W/O Ȯ���Ͽ� ���� VIEW ó��
                 * 7. ���� �Ҷ� W/O DATA UPDATE
                 * 8. ���� BIZ���� W/O VALIDATION ���� �� ���� W/O �� ó�� (���񿡼� ������ EIOATTR�� W/O ���� ������ �ϴ� ���� ����?)
                */

                // 1. ù��° Scan Ȯ��
                if (dgLotList.Rows.Count < 1)
                {
                    string sProcid = GetProcessID();

                    if (Util.NVC(sProcid).Equals(""))
                    {
                        Util.MessageValidation("SFU4251");  // �ش� Lot(Skid) ID�� ������ �������� �ʽ��ϴ�.
                        txtLOTID.Text = "";
                        return;
                    }
                    
                    if (sProcid.Equals(Util.NVC(cboVDProcess.SelectedValue)))
                    {
                        #region ��ȸ�� data���� üũ.. ���� ����..
                        for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                            {
                                DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                                if (!ValidationLot(i))
                                {
                                    txtLOTID.Text = "";
                                    return;
                                }

                                if (!_Unit.Equals("LOT"))
                                {
                                    SetChkCutID(i, dgCanReserve);
                                }

                                dgCanReserve.ScrollIntoView(i, 0);

                                AddRow(dgLotList, dgCanReserve, i);

                                txtLOTID.Text = "";
                                return;
                            }
                        }
                        Util.MessageValidation("SFU1734"); //���డ�� LOT ����Ʈ�� �����ϴ�.
                        txtLOTID.Text = "";
                        #endregion
                    }
                    else // 2. ������ �ٲ�� LOT/SKID VIEW ���� �� ��ȸ DATA CLEAR
                    {
                        Util.gridClear(dgCanReserve);
                        Util.gridClear(dgLotList);
                        Util.gridClear(dgReservedLot);


                        string sTmpEqpid = Util.NVC(cboVDEquipment.SelectedValue);

                        // ���� �ڵ� ����
                        cboVDProcess.SelectedValueChanged -= cboVDProcess_SelectedValueChanged;
                        cboVDProcess.SelectedValue = sProcid;

                        if (cboVDProcess.SelectedIndex < 0)
                            cboVDProcess.SelectedIndex = 0;

                        InitializeControls();

                        // ���� ��ȸ
                        cboVDEquipment.SelectedValueChanged -= cboVDEquipment_SelectedValueChanged;

                        String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
                        combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                        cboVDProcess.SelectedValueChanged += cboVDProcess_SelectedValueChanged;
                        
                        // ���� �ڵ� ����
                        cboVDEquipment.SelectedValue = sTmpEqpid;

                        if (cboVDEquipment.SelectedIndex < 0)
                            cboVDEquipment.SelectedIndex = 0;

                        cboVDEquipment.SelectedValueChanged += cboVDEquipment_SelectedValueChanged;

                        if (cboVDEquipment.Text.Equals("-SELECT-"))
                        {
                            Util.MessageValidation("SFU1673");
                            return;
                        }

                        // 4. �ش� ���� ���� DATA ��ȸ
                        btnSearch_Click(null, null);


                        #region 5. ��ĵ�� DATA �ڵ� ����.. ���� ����..
                        for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                            {
                                DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                                if (!ValidationLot(i))
                                {
                                    txtLOTID.Text = "";
                                    return;
                                }

                                if (!_Unit.Equals("LOT"))
                                {
                                    SetChkCutID(i, dgCanReserve);
                                }

                                dgCanReserve.ScrollIntoView(i, 0);

                                AddRow(dgLotList, dgCanReserve, i);

                                txtLOTID.Text = "";
                                return;
                            }
                        }
                        Util.MessageValidation("SFU1734"); //���డ�� LOT ����Ʈ�� �����ϴ�.
                        txtLOTID.Text = "";
                        #endregion
                    }
                }
                else
                {
                    #region 5. ��ĵ�� DATA �ڵ� ����.. ���� ����..
                    for (int i = 0; i < dgCanReserve.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCanReserve.Rows[i].DataItem, _Unit.Equals("LOT") ? "LOTID" : "CSTID")).Equals(txtLOTID.Text))
                        {
                            DataTableConverter.SetValue(dgCanReserve.Rows[i].DataItem, "CHK", true);

                            if (!ValidationLot(i))
                            {
                                txtLOTID.Text = "";
                                return;
                            }

                            if (!_Unit.Equals("LOT"))
                            {
                                SetChkCutID(i, dgCanReserve);
                            }

                            dgCanReserve.ScrollIntoView(i, 0);

                            AddRow(dgLotList, dgCanReserve, i);

                            txtLOTID.Text = "";
                            return;
                        }
                    }
                    Util.MessageValidation("SFU1734"); //���డ�� LOT ����Ʈ�� �����ϴ�.
                    txtLOTID.Text = "";
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetProcessID()
        {
            try
            {
                string sRet = "";

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SCANID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                                
                DataRow newRow = inTable.NewRow();
                newRow["SCANID"] = Util.NVC(txtLOTID.Text);
                newRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);
                
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCID_VD_RESERVE_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sRet = Util.NVC(dtResult.Rows[0]["PROCID"]);
                }

                loadingIndicator.Visibility = Visibility.Collapsed;

                return sRet;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return "";
            }
        }

        private void InitializeControls()
        {
            try
            {
                if (cboVDProcess.SelectedValue == null) return;

                if (Util.NVC(cboVDProcess.SelectedValue).Equals("A6000"))//�Ŀ�ġ
                {
                    SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                    SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                    LOTID_LOTLIST.Visibility = Visibility.Visible;
                    SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                    tbSKIDID.Visibility = Visibility.Collapsed;
                    tbLotID.Visibility = Visibility.Visible;

                    _Unit = "LOT";
                }
                else //������
                {
                    // ���� ����� ���� �� �������� �۾� �ϰڴٰ� �Ͽ� �ӽ� ó��.(2017.11.01)
                    if (GetUseSkid())
                    {
                        SKIDID_CANRESERVE.Visibility = Visibility.Visible;
                        SKIDID_LOTLIST.Visibility = Visibility.Visible;
                        LOTID_LOTLIST.Visibility = Visibility.Collapsed;
                        SKIDID_RESERVED.Visibility = Visibility.Visible;

                        tbSKIDID.Visibility = Visibility.Visible;
                        tbLotID.Visibility = Visibility.Collapsed;

                        _Unit = "SKID";
                    }
                    else
                    {
                        SKIDID_CANRESERVE.Visibility = Visibility.Collapsed;
                        SKIDID_LOTLIST.Visibility = Visibility.Collapsed;
                        LOTID_LOTLIST.Visibility = Visibility.Visible;
                        SKIDID_RESERVED.Visibility = Visibility.Collapsed;

                        tbSKIDID.Visibility = Visibility.Collapsed;
                        tbLotID.Visibility = Visibility.Visible;

                        _Unit = "LOT";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetSelectWorkOrder(string sProdID)
        {
            try
            {
                C1DataGrid dg = new C1DataGrid();

                if (dg.Columns.Contains("CHK"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    if (dt != null)
                    {
                        DataRow[] dtRows = dt.Select("PRODID = '" + sProdID + "' AND CHK <> '1'");

                        for (int i = 0; i < dtRows.Length; i++)
                        {
                            // View ���� ó��..
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckReserveLot()
        {
            try
            {
                bool bRet = false;
                               
                string ValueToCondition = string.Empty;

                //ValueToCondition = "RESERVE";// "PROC,RESERVE,READY";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                //inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                newRow["PROCID"] = cboVDProcess.SelectedValue.ToString();
                //newRow["WIPSTAT"] = ValueToCondition;

                inTable.Rows.Add(newRow);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(Util.NVC(cboVDProcess.SelectedValue).Equals("A6000") ? "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ_PU" : "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ", "RQSTDT", "RSLTDT", inTable);
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync(_Unit.Equals("LOT") ? "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ_PU" : "DA_PRD_SEL_VD_RESV_LOT_INFO_NJ", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    bRet = true;

                    _VD_WRK_COND_GR_CODE = Util.NVC(dtResult.Rows[0]["VD_WRK_COND_GR_CODE"]);
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool GetUseSkid()
        {
            try
            {
                bool bRet = false;
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CR_VD_SKID_USE_YN_NJ", "RQSTDT", "RSLTDT", null);

                if (dtResult != null && dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["NJ_VD_CR_SKID_USE_FLAG"]).Equals("Y"))
                {
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion
    }
}
