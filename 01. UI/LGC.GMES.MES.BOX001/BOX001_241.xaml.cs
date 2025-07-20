/*************************************************************************************
 Created Date : 2018.08.27
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.27  DEVELOPER : Initial Created.

 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_241 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        public string RePrintcomment = string.Empty;
        public string pUserInfo = string.Empty;
        Int32 sLaneQty = 0;

        BarcodeLib.Barcode b = new BarcodeLib.Barcode();

        #region Declaration & Constructor 
        public BOX001_241()
        {
            InitializeComponent();

            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnProcess);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            dtpDateFrom3.SelectedDataTimeChanged += dtpDateFrom3_SelectedDataTimeChanged;
            dtpDateTo3.SelectedDataTimeChanged += dtpDateTo3_SelectedDataTimeChanged;

            dtpDateFrom4.SelectedDataTimeChanged += dtpDateFrom4_SelectedDataTimeChanged;
            dtpDateTo4.SelectedDataTimeChanged += dtpDateTo4_SelectedDataTimeChanged;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom3.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo3.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom4.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo4.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            String[] sFilter2 = { "ROLL_PRODID" };

            _combo.SetCombo(cboRollProdid, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            _combo.SetCombo(cboRollProdid2, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

        }
        #endregion

        #region Event

        private void dtpDateFrom3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo3.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo3.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom3.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom3.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom4_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo4.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo4.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo4_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom4.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom4.SelectedDateTime;
                return;
            }
        }

        #region Mehod
        #endregion

        #region Button Event

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgPlanList.GetRowCount() == 0)
                {
                    Util.Alert("SFU2065");  //W/O 조회 데이터가 없습니다.
                    return;
                }

                if(dgPackList.GetRowCount() == 0)
                {
                    Util.Alert("SFU2066");  //포장 실적 데이터가 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPlanList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1636"); //선택된 대상이 없습니다.
                    return;
                }

                string sWoid = drChk[0]["WOID"].ToString();
                string[] sProdid = drChk[0]["PRODID"].ToString().Split('_');
                string sProdid2 = drChk[0]["MTRLID"].ToString();
                string sElec = drChk[0]["PRDT_CLSS_CODE"].ToString();

                for (int i = 0; i < dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        if (sElec != Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "ELEC")))
                        {
                            Util.Alert("SFU3006");  //선택한 W/O와 극성이 다릅니다.                            
                            return;
                        }

                        if (sProdid2 != Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "PRODID")))
                        {
                            Util.Alert("SFU1502");  //동일 제품이 아닙니다.                 
                            return;
                        }
                    }
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("INOUT_FLAG", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("WOID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("WIPQTY", typeof(decimal));
                inData.Columns.Add("WIPQTY2", typeof(decimal));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("LANE_QTY", typeof(Int32));
                for (int i = 0; i < dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        DataRow row = inData.NewRow();
                        row["INOUT_FLAG"] = "I"; // O:출하, I:입고, X:취소
                        row["LOTID"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "LOTID").ToString();
                        row["WOID"] = sWoid;
                        row["PRODID"] = sProdid[0].ToString();
                        row["WIPQTY"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY").ToString();
                        row["WIPQTY2"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY2").ToString();
                        row["USERID"] = LoginInfo.USERID;
                        row["LANE_QTY"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "LANE_QTY").ToString() == ""? "1":DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "LANE_QTY").ToString();

                        indataSet.Tables["INDATA"].Rows.Add(row);
                    }
                }
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_FOR_ROLL", "INDATA", "", indataSet);

                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                Util.gridClear(dgPackList);
                txtLOTID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void dgPlanList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;         

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgPlanList.SelectedIndex = idx;
                Util.gridClear(dgPackList);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom3.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo3.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                RQSTDT.Columns.Add("END_DTTM", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["STRT_DTTM"] = sStartdate;
                dr["END_DTTM"] = sEnddate;
                dr["PRODID"] = cboRollProdid.SelectedValue.ToString() == ""? null : cboRollProdid.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_FOR_ROLL_IN_WO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgPlanList);
                Util.GridSetData(dgPlanList, SearchResult, FrameOperation, true);
                Util.gridClear(dgPackList);
                txtLOTID.Text = string.Empty;
            }


            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        //이력조회
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom4.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo4.SelectedDateTime);
                string sElect = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STARTDATE", typeof(string));
                RQSTDT.Columns.Add("ENDDATE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STARTDATE"] = sStartdate;
                dr["ENDDATE"] = sEnddate;
                dr["PRODID"] = cboRollProdid2.SelectedValue.ToString() == "" ? null : cboRollProdid2.SelectedValue.ToString();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLL_WIPACT_HIST_NJ", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutList);

                Util.GridSetData(dgOutList, SearchResult, FrameOperation);
            }


            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {
            if (!txtLOTID.Text.ToString().Equals(""))
                fn_Search_Pord();
        }

        private void fn_Search_Pord()
        {
            try
            {
                string sElect = string.Empty;
                string sProdid = string.Empty;
                string sWoid = string.Empty;
                string sWoDetlid = string.Empty;

                if (dgPlanList.GetRowCount() > 0)
                {
                    DataRow[] drChk = Util.gridGetChecked(ref dgPlanList, "CHK");

                    if (drChk.Length <= 0)
                    {
                        Util.Alert("SFU2941"); // 작업지시가 선택되지 않았습니다.
                        return;
                    }
                    else
                    {
                        sWoid = drChk[0]["WOID"].ToString();
                        sProdid = drChk[0]["MTRLID"].ToString();
                        sElect = drChk[0]["PRDT_CLSS_CODE"].ToString();
                        sLaneQty = Int32.Parse(drChk[0]["LANE_QTY"].ToString());
                    }
                }
                else
                {
                    sProdid = null;
                    sElect = null;
                }

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = sProdid;
                dr["LOTID"] = txtLOTID.Text.ToString() == "" ? null : txtLOTID.Text.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_FOR_ROLL", "RQSTDT", "RSLTDT", RQSTDT);


                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (dgPackList.GetRowCount() == 0)
                {
                    Util.GridSetData(dgPackList, SearchResult, FrameOperation);
                    dgPackList.IsReadOnly = false;
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "CHK", "1");
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PRODNAME", SearchResult.Rows[0]["PRODNAME"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PROCNAME", SearchResult.Rows[0]["PROCNAME"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "LANE_QTY", sLaneQty.ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PROCID", SearchResult.Rows[0]["PROCID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "MODLID", SearchResult.Rows[0]["MODLID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "WIPQTY", SearchResult.Rows[0]["WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "WIPQTY2", (decimal.Parse(SearchResult.Rows[0]["WIPQTY"].ToString()) * sLaneQty).ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "UNIT_CODE", SearchResult.Rows[0]["UNIT_CODE"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "ELECTRODE", SearchResult.Rows[0]["ELECTRODE"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "ELEC", SearchResult.Rows[0]["ELEC"].ToString());
                    dgPackList.IsReadOnly = true;
                }
                else
                {
                    for (int i = 0; i < dgPackList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "LOTID").ToString() == txtLOTID.Text.ToString())
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }

                        if (SearchResult.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "PRODID").ToString())
                        {
                            Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                            return;
                        }
                    }

                    dgPackList.IsReadOnly = false;
                    dgPackList.BeginNewRow();
                    dgPackList.EndNewRow(true);
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "CHK", "1");
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PRODNAME", SearchResult.Rows[0]["PRODNAME"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PROCNAME", SearchResult.Rows[0]["PROCNAME"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "LANE_QTY", sLaneQty.ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "PROCID", SearchResult.Rows[0]["PROCID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "MODLID", SearchResult.Rows[0]["MODLID"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "WIPQTY", SearchResult.Rows[0]["WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "WIPQTY2", (double.Parse(SearchResult.Rows[0]["WIPQTY"].ToString()) * sLaneQty).ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "UNIT_CODE", SearchResult.Rows[0]["UNIT_CODE"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "ELECTRODE", SearchResult.Rows[0]["ELECTRODE"].ToString());
                    DataTableConverter.SetValue(dgPackList.CurrentRow.DataItem, "ELEC", SearchResult.Rows[0]["ELEC"].ToString());
                    dgPackList.IsReadOnly = true;
            }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #endregion

        //실적취소
        private void btnCancelConfirm_Click(object sender, RoutedEventArgs e)
        {
            if(dgOutList.Rows.Count == 0)
            {
                return;
            }
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgOutList, "CHK");

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("INOUT_FLAG", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("WOID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("WIPQTY", typeof(decimal));
                inData.Columns.Add("WIPQTY2", typeof(decimal));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("LANE_QTY", typeof(Int32));
                for (int i = 0; i < dgOutList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        string[] sProdid = DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "PRODID").ToString().Split('_'); // 원복PRODID
                        DataRow row = inData.NewRow();
                        row["INOUT_FLAG"] = "X"; // O:출하, I:입고, X:취소
                        row["LOTID"] = DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "LOTID").ToString();
                        row["WOID"] = DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "WOID").ToString();
                        row["PRODID"] = DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "PRODID").ToString();
                        row["WIPQTY"] = DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "WIPQTY").ToString();
                        row["WIPQTY2"] = DataTableConverter.GetValue(dgOutList.Rows[i].DataItem, "WIPQTY").ToString();
                        row["USERID"] = LoginInfo.USERID;
                        row["LANE_QTY"] = "1";

                        indataSet.Tables["INDATA"].Rows.Add(row);
                    }
                }
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_FOR_ROLL", "INDATA", "", indataSet);

                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                btnSearch2_Click(null,null);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                                if(dgPlanList.GetRowCount() == 0)
                {
                    Util.Alert("SFU2065");  //W/O 조회 데이터가 없습니다.
                    return;
                }
                if(!txtLOTID.Text.ToString().Equals(""))
                    fn_Search_Pord();
            }
        }

        private int woToLaneQty(string swo)
        {
            // SKID Grid Data 조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("WOID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["WOID"] = swo;

            RQSTDT.Rows.Add(dr);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLL_WO_TO_LANE_QTY", "RQSTDT", "RSLTDT", RQSTDT);

            if (Result.Rows.Count == 0)
            {
                return 1;
            }
            return int.Parse(Result.Rows[0]["LANE_QTY"].ToString());
        }
    }
}
