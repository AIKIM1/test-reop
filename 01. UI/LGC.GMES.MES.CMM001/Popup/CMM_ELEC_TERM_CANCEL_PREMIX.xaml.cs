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
    /// CMM_ELEC_TERM_CANCEL_PREMIX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_TERM_CANCEL_PREMIX : C1Window, IWorkArea
    {
        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_TERM_CANCEL_PREMIX()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();
                txtLotID.Focus();
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
            if (e.Key == Key.Enter)
                GetConfirmedLot();
        }

        private void btnTermCancel_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3724", (result) =>  //Lot 잔량 처리를 진행하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                    SetCancelTerm();
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region Biz Call
        private void GetConfirmedLot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["LOTID"] = txtLotID.Text.Trim();

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_TERMINATE_PREMIX", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgConfirmLot, result, FrameOperation, true);
                });

                txtLotID.Text = string.Empty;
                txtLotID.Focus();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return;
            }
        }

        private void SetCancelTerm()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                dgConfirmLot.EndEdit();

                DataSet IndataSet = new DataSet();
                DataTable IndataTable = IndataSet.Tables.Add("INDATA");
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow indataNewRow = IndataTable.NewRow();
                indataNewRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                indataNewRow["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(indataNewRow);

                DataTable InlotTable = IndataSet.Tables.Add("INLOT");
                InlotTable.Columns.Add("LOTID", typeof(string));
                InlotTable.Columns.Add("LOTSTAT", typeof(string));
                InlotTable.Columns.Add("WIPQTY", typeof(decimal));
                InlotTable.Columns.Add("WIPQTY2", typeof(decimal));

                DataRow inlotNewRow = InlotTable.NewRow();

                for (int i = 0; i < dgConfirmLot.Rows.Count - dgConfirmLot.BottomRows.Count; i++)
                {
                    inlotNewRow = null;

                    inlotNewRow = InlotTable.NewRow();
                    inlotNewRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgConfirmLot.Rows[i].DataItem, "LOTID"));
                    inlotNewRow["LOTSTAT"] = "RELEASED";
                    inlotNewRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgConfirmLot.Rows[0].DataItem, "WIPQTY"));
                    inlotNewRow["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgConfirmLot.Rows[0].DataItem, "WIPQTY"));

                    InlotTable.Rows.Add(inlotNewRow);
                }

                if (IndataTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_CANCEL_TERMINATE_LOT", "INDATA,INLOT", null, IndataSet);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                Util.gridClear(dgConfirmLot);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
        #region Authrity
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnTermCancel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion
    }
}
