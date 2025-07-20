using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_RP_RETURN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_RP_RETURN : C1Window, IWorkArea
    {
        #region Initialize
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_RP_RETURN()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.Key == Key.Enter)
                SetRollPressLotReturnConfirm();
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3691", (result) =>  // R/P 공정으로 이동 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                    LotReturn();
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region Biz Call
        private void SetRollPressLotReturnConfirm()
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LOTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = txtLotID.Text.Trim();
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_MOVE_RETURN_RP", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                        throw searchException;

                    if ( result == null || result.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU2025");  //해당하는 LOT정보가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgLot, result, FrameOperation, true);
                    txtLotID.Text = string.Empty;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void LotReturn()
        {
            try
            {
                DataSet inData = new DataSet();

                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("PROCID_TO", typeof(string));
                inDataTable.Columns.Add("WIP_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["PROCID_TO"] = Process.ROLL_PRESSING;
                row["WIP_TYPE_CODE"] = INOUT_TYPE.IN; //MES 2.0 IN 으로 변경(고해선 책임 요청)
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataTable dt = ((DataView)dgLot.ItemsSource).Table;
                foreach (DataRow dataRow in dt.Rows)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(dataRow["LOTID"]);
                    inLot.Rows.Add(row);
                }

                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CHANGE_PROC_RP", "INDATA,INLOT", null, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.MessageInfo("SFU3692");    //공정이동 완료 하였습니다.
                        Util.gridClear(dgLot);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }, inData);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        #endregion
        #region Auth
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMove);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion
    }
}
