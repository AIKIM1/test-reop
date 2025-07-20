/*************************************************************************************
 Created Date : 2018.04.25
      Creator : 신광희
   Decription : 전지 5MEGA-GMES 구축 - Tray LOT 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.25  신광희 : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_TRAYLOT_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private DataTable _dtFromLot;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public CMM_ASSY_TRAYLOT_CHANGE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void CMM_ASSY_TRAYLOT_CHANGE_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            _processCode = tmps[0] as string;
            _equipmentSegmentCode = tmps[1] as string;
            _equipmentCode = tmps[2] as string;

            dgLotTo.CommittedEdit += dgLotTo_CommittedEdit;
            dgTrayFrom.CommittedEdit += dgTrayFrom_CommittedEdit;

        }

        private void txtLotID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    if (textBox.Name == "txtFromLotID")
                    {
                        if (!string.IsNullOrEmpty(txtFromLotID.Text))
                            GetWashingLotInfo("From", txtFromLotID.Text);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(txtToLotID.Text))
                            GetWashingLotInfo("To", txtToLotID.Text);
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn != null)
            {
                if (btn.Name == "btnFromSearch")
                {
                    if (!string.IsNullOrEmpty(txtFromLotID.Text))
                        GetWashingLotInfo("From", txtFromLotID.Text);
                }
                else
                {
                    if (!string.IsNullOrEmpty(txtToLotID.Text))
                        GetWashingLotInfo("To", txtToLotID.Text);
                }
            }
        }

        private void btnRightMove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 TRAY를 이동 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU4919", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveTrayMove();
                }
            });

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// To Lot List - To Tray List 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLotTo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DataTable dtLotTo = DataTableConverter.Convert(dgLotTo.ItemsSource);

            string sLotid = dtLotTo.Rows[e.Cell.Row.Index]["LOTID"].ToString();

            foreach (DataRow row in dtLotTo.Rows)
            {
                if (row["LOTID"].Equals(sLotid))
                    continue;
                else
                    row["CHK"] = "0";
            }

            dtLotTo.AcceptChanges();
            Util.GridSetData(dgLotTo, dtLotTo, null, true);
        }

        /// <summary>
        /// From Tray List - Cell 수량 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTrayFrom_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DataTable dtTray = DataTableConverter.Convert(dgTrayFrom.ItemsSource);
            DataRow[] dr = dtTray.Select("CHK = '1' OR CHK = 'True' ");

            decimal dSum = 0;

            foreach (DataRow row in dr)
            {
                dSum += Util.NVC_Decimal(row["CELLQTY"].ToString());
            }

            txtTrayFromCellSum.Text = dSum.ToString("N0");
        }
        #endregion


        #region User Mehod

        #region [BizCall]

        private void GetWashingLotInfo(string gubun, string lotId)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtFromLotID.Text) && !string.IsNullOrEmpty(txtToLotID.Text) &&
                    txtFromLotID.Text.Equals(txtToLotID.Text))
                {
                    Util.MessageValidation("SFU4922");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                if (string.Equals(gubun, "From"))
                {
                    txtTrayFromCellSum.Text = string.Empty;
                    Util.gridClear(dgLotFrom);
                    Util.gridClear(dgTrayFrom);
                }
                else
                {
                    Util.gridClear(dgLotTo);
                    Util.gridClear(dgTrayTo);
                }

                const string bizRuleName = "BR_PRD_SEL_WASHING_LOT_INFO_WS";
                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["PROD_LOTID"] = lotId;
                inTable.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUT_LOT,OUT_TRAY", indataSet);

                if (CommonVerify.HasTableInDataSet(dsRslt))
                {
                    if (string.Equals(gubun, "From"))
                    {
                        Util.GridSetData(dgLotFrom, dsRslt.Tables["OUT_LOT"], null, true);
                        Util.GridSetData(dgTrayFrom, dsRslt.Tables["OUT_TRAY"], null, true);
                    }
                    else
                    {
                        Util.GridSetData(dgLotTo, dsRslt.Tables["OUT_LOT"], null, true);
                        Util.GridSetData(dgTrayTo, dsRslt.Tables["OUT_TRAY"], null, true);
                    }

                    if (CommonVerify.HasDataGridRow(dgLotFrom) && CommonVerify.HasDataGridRow(dgLotTo)
                        && Util.NVC(DataTableConverter.GetValue(dgLotFrom.Rows[0].DataItem, "PRODID")) ==
                        Util.NVC(DataTableConverter.GetValue(dgLotTo.Rows[0].DataItem, "PRODID")))
                    {
                        btnRightMove.IsEnabled = true;
                    }
                    else
                        btnRightMove.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }



        private void SaveTrayMove()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("IN_DATA");
                inTable.Columns.Add("FROM_LOTID", typeof(string));
                inTable.Columns.Add("TO_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inTrayTable = indataSet.Tables.Add("IN_TRAY");
                inTrayTable.Columns.Add("OUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["FROM_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotFrom.Rows[0].DataItem, "LOTID"));
                newRow["TO_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotTo.Rows[0].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // in_cst 생성
                DataTable dtTray = DataTableConverter.Convert(dgTrayFrom.ItemsSource);
                DataRow[] drTary = dtTray.Select("CHK = '1' OR CHK = 'True' ");

                foreach (DataRow row in drTary)
                {
                    newRow = inTrayTable.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(row["OUT_LOTID"].ToString());
                    inTrayTable.Rows.Add(newRow);
                }

                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOT_CHANGE_OUT_LOT_WS", "IN_DATA,IN_TRAY", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetWashingLotInfo("From",txtFromLotID.Text);
                        GetWashingLotInfo("To", txtToLotID.Text);

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        //this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region[[Validation]
        /// <summary>
        /// Tray 이동 가능 체크
        /// </summary>
        /// <returns></returns>
        private bool CanTrayMove()
        {

            DataTable dtTray = DataTableConverter.Convert(dgTrayFrom.ItemsSource);
            DataRow[] dr = dtTray.Select("CHK = '1' OR CHK = 'True' ");
            if (dr.Length < 1)
            {
                Util.MessageValidation("SFU4920");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgLotTo))
            {
                Util.MessageValidation("SFU4921");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion


    }
}
