/*************************************************************************************
 Created Date : 2020.03.08
      Creator : 
   Decription : 설비Trouble List
--------------------------------------------------------------------------------------
 [Change History]
  2021.  DEVELOPER : Initial Created.





 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_096_DATA_DISPOSAL : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();
        private string _sLotID;
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

        public FCS002_096_DATA_DISPOSAL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null && tmps.Length >= 1)
                {
                    _sLotID = Util.NVC(tmps[0]);
                }

                txtLotID.Text = _sLotID;
                txtCellQty.Text = "0";

                //조회함수
                GetList();
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
                Util.gridClear(dgCellList);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(txtLotID.Text);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RN_CELL_DETAIL_DRB_MB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgCellList, result, FrameOperation, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0337", (result) => //변경하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();

                        if (_Util.GetDataGridCheckCnt(dgCellList,"CHK") <= 0)
                        {
                            Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                        }
                        else
                        {
                            //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("SET_2D_BCR_SCAN_VERIFY", "INDATA", "OUTDATA", dtRqst);
                            //if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            //{
                            //    Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.
                            //    Util.gridClear(dgBCR);
                            //}
                            //else
                            //{
                            //    Util.AlertInfo("FM_ME_0135");  //변경된 데이터가 없습니다.
                            //}
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void dgCellList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
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
