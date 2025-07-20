/*************************************************************************************
 Created Date : 2020.11.23
      Creator : Kang Dong Hee
   Decription : BCR 검증
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  NAME : Initial Created
  2021.03.31  KDH : GetTestData 함수 호출 로직 제거
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_064_BCR_CHECK : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_064_BCR_CHECK()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtCellId.SelectAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                int iRow = -1;

                string sCellID = Util.GetCondition(txtCellId).ToString();
                iRow = _Util.GetDataGridRowIndex(dgBCR, "SUBLOTID", sCellID);

                if (iRow > -1)
                {
                    Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                }
                else
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("READ_BCR", typeof(string));
                    dtRqst.Columns.Add("LABEL_CODE_1", typeof(string));
                    dtRqst.Columns.Add("LABEL_CODE_2", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = Util.GetCondition(txtCellId);
                    dr["READ_BCR"] = Util.GetCondition(txtBCR);
                    dr["LABEL_CODE_1"] = "GBT";
                    dr["LABEL_CODE_2"] = "2D";
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_2D_BCR_READING_VERIFY", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count > 0)
                    {
                        DataTable preTable = DataTableConverter.Convert(dgBCR.ItemsSource);

                        if (preTable.Rows.Count > 0)
                        {
                            DataRow row = preTable.NewRow();
                            row["CHK"] = Util.NVC(dtRslt.Rows[0]["CHK"]);
                            row["SUBLOTID"] = Util.NVC(dtRslt.Rows[0]["SUBLOTID"]);
                            row["PRINT_BCR"] = Util.NVC(dtRslt.Rows[0]["PRINT_BCR"]);
                            row["SCAN_BCR"] = Util.NVC(dtRslt.Rows[0]["SCAN_BCR"]);
                            row["READ_BCR"] = Util.NVC(dtRslt.Rows[0]["READ_BCR"]);
                            row["SCAN_STATUS"] = Util.NVC(dtRslt.Rows[0]["SCAN_STATUS"]);
                            row["PRINT_STATUS"] = Util.NVC(dtRslt.Rows[0]["PRINT_STATUS"]);
                            row["VERIF_GRD_VALUE"] = Util.NVC(dtRslt.Rows[0]["VERIF_GRD_VALUE"]);
                            row["VERIF_GRD_VALUE_2D"] = Util.NVC(dtRslt.Rows[0]["VERIF_GRD_VALUE_2D"]);
                            preTable.Rows.Add(row);
                            Util.GridSetData(dgBCR, preTable, FrameOperation, true);
                        }
                        else
                        {
                            Util.GridSetData(dgBCR, dtRslt, FrameOperation, true);
                        }
                    }
                }
                txtCellId.Text = string.Empty;
                txtBCR.Text = string.Empty;
                txtCellId.SelectAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void txtCellId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && FrameOperation.AUTHORITY.Equals("W"))
            {
                txtBCR.SelectAll();

                if (!string.IsNullOrEmpty(Util.GetCondition(txtCellId)) && !string.IsNullOrEmpty(Util.GetCondition(txtBCR)))
                {
                    GetList();
                }
                else
                {
                    txtBCR.Focus();
                }
            }
        }

        private void txtBCR_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && FrameOperation.AUTHORITY.Equals("W"))
            {
                txtCellId.SelectAll();

                if (!string.IsNullOrEmpty(Util.GetCondition(txtCellId)) && !string.IsNullOrEmpty(Util.GetCondition(txtBCR)))
                {
                    GetList();
                }
                else
                {
                    txtCellId.Focus();
                }
            }
        }


        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtCellId.Text = string.Empty;
            txtBCR.Text = string.Empty;
            Util.gridClear(dgBCR);
            txtCellId.SelectAll();
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0337", (result) => //변경하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();

                        if (_Util.GetDataGridCheckCnt(dgBCR,"CHK") <= 0)
                        {
                            Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                        }
                        else
                        {
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("SET_2D_BCR_SCAN_VERIFY", "INDATA", "OUTDATA", dtRqst);
                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.
                                Util.gridClear(dgBCR);
                            }
                            else
                            {
                                Util.AlertInfo("FM_ME_0135");  //변경된 데이터가 없습니다.
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgBCR_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                string SCAN_STATUS = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SCAN_STATUS"));
                string PRINT_STATUS = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_STATUS"));

                if (!(Util.NVC(SCAN_STATUS).Equals("NG") && Util.NVC(PRINT_STATUS).Equals("OK")))
                {
                    dgBCR.GetCell(e.Cell.Row.Index, dgBCR.Columns["CHK"].Index).Presenter.IsEnabled = false;
                }
                else
                {
                    dgBCR.GetCell(e.Cell.Row.Index, dgBCR.Columns["CHK"].Index).Presenter.IsEnabled = true;
                }
            }));
        }

        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            dt.Columns.Add("CHK", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["CHK"] = "False";
            row1["SUBLOTID"] = "UAB3310020";
            row1["PRINT_BCR"] = "TEST_PRINT_BCR";
            row1["SCAN_BCR"] = "TEST_SCAN_BCR";
            row1["READ_BCR"] = "178934333162104909365217210481165910(Y)";
            row1["SCAN_STATUS"] = "OK";
            row1["PRINT_STATUS"] = "NG";
            row1["VERIF_GRD_VALUE"] = "A";
            row1["VERIF_GRD_VALUE_2D"] = "B";
            dt.Rows.Add(row1);

            #endregion

        }

        private void dgBCR_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            try
            {
                bool bCheck = (bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
