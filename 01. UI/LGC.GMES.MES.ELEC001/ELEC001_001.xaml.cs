/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 이진선
   Decription : SRS이송탱크
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_001 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo combo = new CommonCombo();

        private System.Windows.Threading.DispatcherTimer _Timer = new System.Windows.Threading.DispatcherTimer();

        public ELEC001_001()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initcombo();
            GetInputLot();
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            string SRSTANK = cboSRSTank.SelectedValue.ToString();


            for (int i = 0; i < dgSRSCoaterProcLot.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[i].DataItem, "TANKID")).Equals(SRSTANK))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[i].DataItem, "STATUS")).Equals("PROC"))
                    {
                        Util.MessageValidation("SFU2022");  //해당 이송탱크에 투입된 LOT이 있습니다.
                        return;
                    }
                }
            }

            //저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    DataSet indataSet = new DataSet();
                    DataTable indata = indataSet.Tables.Add("INDATA");
                    indata.Columns.Add("SRCTYPE", typeof(string));
                    indata.Columns.Add("IFMODE", typeof(string));
                    indata.Columns.Add("USERID", typeof(string));
                    indata.Columns.Add("EQPTID", typeof(string));
                    indata.Columns.Add("PROCID", typeof(string));

                    DataRow row = indata.NewRow();
                    row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    row["IFMODE"] = "OFF";
                    row["USERID"] = LoginInfo.USERID;
                    row["EQPTID"] = cboSRSTank.SelectedValue.ToString();
                    row["PROCID"] = Process.SRS_COATING;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable lot = indataSet.Tables.Add("IN_INPUT");
                    lot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    lot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    lot.Columns.Add("MTRLID", typeof(string));
                    lot.Columns.Add("INPUT_LOTID", typeof(string));
                    lot.Columns.Add("ACTQTY", typeof(string));

                    row = lot.NewRow();
                    row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgSRSMixerEndLot.Rows[index].DataItem, "EQPTMOUNTPSTNID"));
                    row["EQPT_MOUNT_PSTN_STATE"] = Util.NVC(DataTableConverter.GetValue(dgSRSMixerEndLot.Rows[index].DataItem, "EQPTMOUNTPSTNSTATE"));
                    row["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgSRSMixerEndLot.Rows[index].DataItem, "PRODID"));
                    row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSRSMixerEndLot.Rows[index].DataItem, "LOTID"));
                    row["ACTQTY"] = 0;
                    indataSet.Tables["IN_INPUT"].Rows.Add(row);


                    try
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_RPD_REG_START_LOT_SRSTANK", "INDATA,IN_INPUT ", null, indataSet);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                    GetProductLot();
                    GetInputLot();

                }
            });
            //투입

        }
        private void cboSRSTank_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetProductLot();

            string value = (sender as C1ComboBox).SelectedValue.ToString();



        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            string value = cboSRSTank.SelectedValue.ToString();

            /*
            if (index != cboSRSTank.SelectedIndex)
            {
                Util.Alert("SFU2028", new object[] { cboSRSTank.SelectedValue.ToString(), cboSRSTank.Text }); //현재 이송탱크 [{0}]{1}가 선택되어 있습니다.
                return;
            }
            */

            if (Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "LOTID")).Equals(""))
            {
                Util.MessageValidation("SFU1969");  //투입된 LOT이 없습니다.
                return;
            }

            try
            {
                //투입을 취소 하시겠습니까?
                Util.MessageConfirm("SFU1982", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataSet indataSet = new DataSet();
                        DataTable indata = indataSet.Tables.Add("INDATA");
                        indata.Columns.Add("SRCTYPE", typeof(string));
                        indata.Columns.Add("IFMODE", typeof(string));
                        indata.Columns.Add("USERID", typeof(string));
                        indata.Columns.Add("EQPTID", typeof(string));
                        indata.Columns.Add("PROCID", typeof(string));

                        DataRow row = indata.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = "OFF";
                        row["USERID"] = LoginInfo.USERID;
                        //row["EQPTID"] = cboSRSTank.SelectedValue.ToString();
                        row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "TANKID"));
                        row["PROCID"] = Process.SRS_COATING;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable lot = indataSet.Tables.Add("IN_INPUT");
                        lot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        lot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        lot.Columns.Add("MTRLID", typeof(string));
                        lot.Columns.Add("INPUT_LOTID", typeof(string));
                        lot.Columns.Add("ACTQTY", typeof(string));

                        row = lot.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "EQPTMOUNTPSTNID"));
                        row["EQPT_MOUNT_PSTN_STATE"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "EQPTMOUNTPSTNSTATE"));
                        row["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "PRODID"));
                        row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "LOTID"));
                        row["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "WIPQTY"));
                        indataSet.Tables["IN_INPUT"].Rows.Add(row);


                        try
                        {
                            new ClientProxy().ExecuteServiceSync_Multi("BR_RPD_REG_CANCEL_LOT_SRSTANK", "INDATA,IN_INPUT ", null, indataSet);
                            Util.AlertInfo("SFU1983");  //투입이 취소되었습니다.
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                        GetProductLot();
                        GetInputLot();

                    }
                });
                //투입

            }
            catch (Exception ex) { Util.MessageException(ex); }


        }

        private void btnEND_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            string SRSTANK = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "TANKID"));

            /*
            string value = cboSRSTank.SelectedValue.ToString();
            if (index != cboSRSTank.SelectedIndex)
            {
                Util.Alert("SFU2028", new object[] { cboSRSTank.SelectedValue.ToString(), cboSRSTank.Text });   //현재 이송탱크 [{0}]{1}가 선택되어 있습니다.
                return;
            }
            */

            if (Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "LOTID")).Equals(""))
            {
                Util.MessageValidation("SFU1969");  //투입된 LOT이 없습니다.
                return;
            }

            //완료 처리 하시겠습니까?
            Util.MessageConfirm("SFU1745", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {

                        DataSet indataSet = new DataSet();
                    DataTable indata = indataSet.Tables.Add("INDATA");
                    indata.Columns.Add("SRCTYPE", typeof(string));
                    indata.Columns.Add("IFMODE", typeof(string));
                    indata.Columns.Add("USERID", typeof(string));
                    indata.Columns.Add("EQPTID", typeof(string));
                    indata.Columns.Add("PROCID", typeof(string));

                    DataRow row = indata.NewRow();
                    row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    row["IFMODE"] = "OFF";
                    row["USERID"] = LoginInfo.USERID;
                    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "TANKID"));
                    row["PROCID"] = Process.SRS_COATING;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable lot = indataSet.Tables.Add("IN_INPUT");
                    lot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    lot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    lot.Columns.Add("MTRLID", typeof(string));
                    lot.Columns.Add("INPUT_LOTID", typeof(string));
                    lot.Columns.Add("ACTQTY", typeof(string));

                    row = lot.NewRow();
                    row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "EQPTMOUNTPSTNID"));
                    row["EQPT_MOUNT_PSTN_STATE"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "EQPTMOUNTPSTNSTATE"));
                    row["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "PRODID"));
                    row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSRSCoaterProcLot.Rows[index].DataItem, "LOTID"));
                    row["ACTQTY"] = 0;
                    indataSet.Tables["IN_INPUT"].Rows.Add(row);

                    
                        new ClientProxy().ExecuteServiceSync_Multi("BR_RPD_REG_END_LOT_SRSTANK", "INDATA,IN_INPUT ", null, indataSet);
                        Util.AlertInfo("SFU1973");  //투입완료되었습니다.

                        GetProductLot();
                        GetInputLot();
                    }
                    catch (Exception ex) { Util.MessageException(ex); }

                }
            });
            

        }
        #endregion

        #region Method

        private void initcombo()
        {
            CommonCombo combo = new CommonCombo();
            combo.SetCombo(cboSRSTank, CommonCombo.ComboStatus.NONE, sCase: "SRSTANK");
        }
        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_001_LOTSTART window = sender as ELEC001_001_LOTSTART;
            if (window.DialogResult == MessageBoxResult.Cancel)
            {
                GetProductLot();
            }
        }

        private void GetProductLot()
        {

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("SRSTANK", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.SRS_COATING;
                row["SRSTANK"] = cboSRSTank.SelectedValue.ToString();

                dt.Rows.Add(row);

        
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_SRS_TANK_WAIT_LOT", "RQSTDT", "RSTDT", dt);
                if (result == null) return;

                if (result.Rows.Count >0)
                {
                    DataTable dt2 = new DataTable();
                    dt2.Columns.Add("SRSTANK", typeof(string));

                    DataRow row2 = dt2.NewRow();
                    row2["SRSTANK"] = cboSRSTank.SelectedValue.ToString();

                    dt2.Rows.Add(row2);

                    DataTable mtrlinfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SRSTANK_MTRL", "RQSTDT", "RSTDT", dt);
                    if ((mtrlinfo.Rows.Count <= 0) || (mtrlinfo == null))
                    {
                        Util.MessageValidation("SFU2020");  //해당 설비의 장착위치부 정보가 없습니다.
                        return;
                    }

                    result.Columns.Add("EQPTMOUNTPSTNID", typeof(string));
                    result.Columns.Add("EQPTMOUNTPSTNSTATE", typeof(string));

                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        result.Rows[i]["EQPTMOUNTPSTNID"] = mtrlinfo.Rows[0]["EQPTMOUNTPSTNID"].ToString();
                        result.Rows[i]["EQPTMOUNTPSTNSTATE"] = "A";
                    }
                }

                Util.GridSetData(dgSRSMixerEndLot, result, FrameOperation, true);
              
               
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        private void GetInputLot()
        {
            try
            {
                DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["PROCID"] = Process.SRS_COATING;

            dt.Rows.Add(row);

        
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SRSCOATER_PROC", "RQSTDT", "RSTDT", dt);

                Util.GridSetData(dgSRSCoaterProcLot, result, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }








        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputLot();
            GetProductLot();
        }
    }
}
