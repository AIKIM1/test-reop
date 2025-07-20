/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
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
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_016_ADD : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public DataTable dtAdd;

        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;

        public BOX001_016_ADD()
        {
            InitializeComponent();

            this.Loaded += Window_Loaded;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable; //dtLot

            this.Loaded -= Window_Loaded;

            txtLotid.Focus();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dtAdd = new DataTable();
                dtAdd.TableName = "dtAdd";
                dtAdd.Columns.Add("LOTID", typeof(string));
                dtAdd.Columns.Add("M_WIPQTY", typeof(string));
                dtAdd.Columns.Add("CELL_WIPQTY", typeof(string));
                dtAdd.Columns.Add("PRODID", typeof(string));
                dtAdd.Columns.Add("POSITION", typeof(string));
                dtAdd.Columns.Add("EQSGNAME", typeof(string));
                dtAdd.Columns.Add("WIPSDTTM", typeof(string));
                dtAdd.Columns.Add("PROD_VER_CODE", typeof(string));
                dtAdd.Columns.Add("VLD_DATE", typeof(string));
                dtAdd.Columns.Add("FROM_AREAID", typeof(string));
                dtAdd.Columns.Add("FROM_SHOPID", typeof(string));
                dtAdd.Columns.Add("FROM_SLOC_ID", typeof(string));
                dtAdd.Columns.Add("MODLID", typeof(string));

                for (int i = 0; i < dgAddList.GetRowCount(); i++)
                {
                    DataRow drAdd= dtAdd.NewRow();

                    drAdd["LOTID"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "LOTID");
                    drAdd["M_WIPQTY"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "M_WIPQTY");
                    drAdd["CELL_WIPQTY"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "CELL_WIPQTY");
                    drAdd["PRODID"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "PRODID");
                    drAdd["POSITION"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "POSITION");
                    drAdd["EQSGNAME"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "EQSGNAME");
                    drAdd["WIPSDTTM"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "WIPSDTTM");
                    drAdd["PROD_VER_CODE"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "PROD_VER_CODE");
                    drAdd["VLD_DATE"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "VLD_DATE");
                    drAdd["FROM_AREAID"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "FROM_AREAID");
                    drAdd["FROM_SHOPID"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "FROM_SHOPID");
                    drAdd["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "FROM_SLOC_ID");
                    drAdd["MODLID"] = DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "MODLID");

                    dtAdd.Rows.Add(drAdd);
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            try
            {
                string sLotid = txtLotid.Text.ToString().Trim();

                // 출고 이력 조회
                DataTable RQSTDT0 = new DataTable();
                RQSTDT0.TableName = "RQSTDT";
                RQSTDT0.Columns.Add("CSTID", typeof(String));
                RQSTDT0.Columns.Add("AREAID", typeof(String));

                DataRow dr0 = RQSTDT0.NewRow();
                dr0["CSTID"] = sLotid;
                dr0["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT0.Rows.Add(dr0);

                DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);

                int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());

                if (iCnt <= 0)
                {
                    Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                    return;
                }

                // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("PROCID", typeof(String));
                RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotid;
                dr1["PROCID"] = "E7000";
                dr1["WIPSTAT"] = "WAIT";

                RQSTDT1.Rows.Add(dr1);

                DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Prod_Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                for (int i = 0; i < Prod_Result.Rows.Count; i++)
                {
                    if (Prod_Result.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                    {
                        Util.MessageValidation("SFU1340");   //HOLD 된 LOT ID 입니다.
                        return;
                    }
                }


                // 품질결과 검사 체크
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                inData.Columns.Add("BR_TYPE", typeof(string));

                DataRow row = inData.NewRow();
                row["LOTID"] = sLotid;
                row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                indataSet.Tables["INDATA"].Rows.Add(row);

                //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                //신규 HOLD 적용을 위해 변경 작업
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", indataSet);

                // 재공정보 조회
                //DataTable RQSTDT2 = new DataTable();
                //RQSTDT2.TableName = "RQSTDT";
                //RQSTDT2.Columns.Add("LOTID", typeof(String));

                //DataRow dr2 = RQSTDT2.NewRow();
                //dr2["LOTID"] = sLotid;

                //RQSTDT2.Rows.Add(dr2);

                //DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUT_LIST_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT2);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT"; 
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_PACKING", "RQSTDT", "RSLTDT", RQSTDT);

                if (Lot_Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (tmmp01 != Lot_Result.Rows[0]["PRODID"].ToString())
                {
                    Util.MessageValidation("SFU1502");   //동일 제품이 아닙니다.
                    return;
                }

                for (int icnt = 0; icnt < tmmp02.Rows.Count; icnt++)
                {
                    if (tmmp02.Rows[icnt]["LOTID"].ToString() == sLotid)
                    {
                        Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                        return;
                    }
                }

                if (dgAddList.GetRowCount() == 0)
                {
                    dgAddList.ItemsSource = DataTableConverter.Convert(Lot_Result);
                }
                else
                {
                    for (int i = 0; i < dgAddList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgAddList.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.MessageValidation("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    dgAddList.IsReadOnly = false;
                    dgAddList.BeginNewRow();
                    dgAddList.EndNewRow(true);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "LOTID", Lot_Result.Rows[0]["LOTID"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "PRODID", Lot_Result.Rows[0]["PRODID"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "M_WIPQTY", Lot_Result.Rows[0]["M_WIPQTY"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "CELL_WIPQTY", Lot_Result.Rows[0]["CELL_WIPQTY"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "MODLID", Lot_Result.Rows[0]["MODLID"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "POSITION", Lot_Result.Rows[0]["POSITION"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "EQSGNAME", Lot_Result.Rows[0]["EQSGNAME"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "WIPSDTTM", Lot_Result.Rows[0]["WIPSDTTM"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "PROD_VER_CODE", Lot_Result.Rows[0]["PROD_VER_CODE"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "VLD_DATE", Lot_Result.Rows[0]["VLD_DATE"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "FROM_AREAID", Lot_Result.Rows[0]["FROM_AREAID"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "FROM_SHOPID", Lot_Result.Rows[0]["FROM_SHOPID"]);
                    DataTableConverter.SetValue(dgAddList.CurrentRow.DataItem, "FROM_SLOC_ID", Lot_Result.Rows[0]["FROM_SLOC_ID"]);

                    dgAddList.IsReadOnly = true;
                }

                txtLotid.SelectAll();
                txtLotid.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

    }
}
