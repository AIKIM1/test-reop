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
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_032 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();

        #region Declaration & Constructor 
        public BOX001_032()
        {
            InitializeComponent();            
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboEquipmentSegmentParent);


            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "AREA");

            //// ComboBox 추가 필요
            //string[] sFilters = { LoginInfo.CFG_SHOP_ID, "OUTSD_ELTR_TYPE_CODE" };
            //combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilters);
            //combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "AREA", sFilter: sFilters);

            //string[] sFilters1 = { Convert.ToString(cboArea.SelectedValue) };
            //combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, sFilter: sFilters1);

            rdoPancake.IsChecked = true;

            SetEvent();

            txtLotid.Focus();

            //rdoPancake_Click(null, null);
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region Mehod

        #endregion

        #region Event

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotid = string.Empty;
                    sLotid = txtLotid.Text.Trim();

                    if (sLotid == "")
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("입력한 LOT ID 가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("LOTID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LOTID"] = sLotid;

                    RQSTDT.Rows.Add(dr);

                    //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_NJ_RECEIVE_MTRL", "INDATA", "OUTDATA", RQSTDT);
                    new ClientProxy().ExecuteService("BR_PRD_SEL_NJ_RECEIVE_MTRL", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            if (dgReceive.GetRowCount() == 0)
                            {
                                dgReceive.ItemsSource = DataTableConverter.Convert(searchResult);
                            }
                            else
                            {
                                DataTable dtSource = DataTableConverter.Convert(dgReceive.ItemsSource);
                                dtSource.Merge(searchResult);

                                Util.gridClear(dgReceive);
                                dgReceive.ItemsSource = DataTableConverter.Convert(dtSource);
                            }

                            txtLotid.SelectAll();
                            txtLotid.Focus();

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                    );
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (dgReceive.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                    return;
                }

                if (cboArea.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1799");   //입고 할 동을 선택해주세요.
                    return;
                }

                //if (rdoPancake.IsChecked == false)
                //{
                if (cboProcid.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1795");   //입고 공정을 선택해주세요.
                    return;
                }
                //}

                //입고 하시겠습니까?
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2073"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
               Util.MessageConfirm("SFU2073", (result) =>
               {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {

                            decimal dSum = 0;
                            decimal dSum2 = 0;
                            decimal dTotal = 0;
                            decimal dTotal2 = 0;

                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY")));
                                dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY2")));

                                dTotal = dTotal + dSum;
                                dTotal2 = dTotal2 + dSum2;
                            }

                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("WO_DETL_ID", typeof(string));
                            inData.Columns.Add("PRODID", typeof(string));
                            inData.Columns.Add("BOMREV", typeof(string));
                            inData.Columns.Add("PROCID", typeof(string));
                            inData.Columns.Add("PCSGID", typeof(string));
                            inData.Columns.Add("EQSGID", typeof(string));
                            inData.Columns.Add("EQPTID", typeof(string));
                            inData.Columns.Add("RECIPEID", typeof(string));
                            inData.Columns.Add("PROD_VER_CODE", typeof(string));
                            inData.Columns.Add("IFMODE", typeof(string));
                            inData.Columns.Add("TRSF_POST_FLAG", typeof(string));
                            inData.Columns.Add("AREAID", typeof(string));
                            inData.Columns.Add("SLOC_ID", typeof(string));
                            inData.Columns.Add("TOTAL_QTY", typeof(decimal));
                            inData.Columns.Add("TOTAL_QTY2", typeof(decimal));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                            inData.Columns.Add("NOTE", typeof(string));
                            inData.Columns.Add("PANROLL_GUBUN", typeof(string));

                            string[] str = Convert.ToString(cboProcid.SelectedValue).Split('|');

                            DataRow row = inData.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["WO_DETL_ID"] = "";//없음
                            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[0].DataItem, "PRODID"));
                            row["BOMREV"] = "";//없음
                            row["PROCID"] = str[1];//Convert.ToString(cboProcid.SelectedValue);
                            row["PCSGID"] = str[0];// cbo
                            row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                            row["EQPTID"] = ""; //없음
                            row["RECIPEID"] = ""; //없음
                            row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[0].DataItem, "PROD_VER_CODE"));
                            row["IFMODE"] = "";//없음
                            row["TRSF_POST_FLAG"] = "N";
                            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                            row["SLOC_ID"] = "";//없음
                            row["TOTAL_QTY"] = dTotal;
                            row["TOTAL_QTY2"] = dTotal2;
                            row["USERID"] = LoginInfo.USERID;
                            row["PRDT_CLSS_CODE"] = "";
                            row["NOTE"] = txtRemark.Text.ToString();
                            row["PANROLL_GUBUN"] = rdoPancake.IsChecked == true ? "P" : "R";

                            inData.Rows.Add(row);


                            DataTable inLot = indataSet.Tables.Add("INLOT");
                            inLot.Columns.Add("LOTID", typeof(string));
                            inLot.Columns.Add("LOTTYPE", typeof(string));
                            inLot.Columns.Add("LOTID_RT", typeof(string));
                            inLot.Columns.Add("ACTQTY", typeof(decimal));
                            inLot.Columns.Add("ACTQTY2", typeof(decimal));
                            inLot.Columns.Add("ACTUNITQTY", typeof(decimal));
                            inLot.Columns.Add("PR_LOTID", typeof(string));
                            inLot.Columns.Add("WIPNOTE", typeof(string));
                            inLot.Columns.Add("WIP_TYPE_CODE", typeof(string));
                            inLot.Columns.Add("HOTFLAG", typeof(string));
                            inLot.Columns.Add("PROD_LOTID", typeof(string));
                            inLot.Columns.Add("PRJT_NAME", typeof(string));
                            inLot.Columns.Add("SLIT_CUT_ID", typeof(string));
                            inLot.Columns.Add("SLIT_DATE", typeof(string));
                            inLot.Columns.Add("LANE_QTY", typeof(string));
                            inLot.Columns.Add("RT_LOT_CR_DTTM", typeof(string));
                            inLot.Columns.Add("ROLLPRESS_DATE", typeof(string));
                            inLot.Columns.Add("FROM_SHOPID", typeof(string));
                            inLot.Columns.Add("FROM_AREAID", typeof(string));

                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID"));
                                row["LOTTYPE"] = "P";
                                row["LOTID_RT"] = "";
                                row["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY"));
                                row["ACTQTY2"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY2"));
                                row["ACTUNITQTY"] = 0;//없고
                                row["PR_LOTID"] = "";//없고
                                row["WIPNOTE"] = txtRemark.Text.ToString();
                                row["WIP_TYPE_CODE"] = "IN";
                                row["HOTFLAG"] = "";//없고
                                row["PROD_LOTID"] = "";//없고
                                row["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRJT_NAME"));
                                row["SLIT_CUT_ID"] = "";
                                row["SLIT_DATE"] = "";
                                row["LANE_QTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LANE_PTN_QTY"));
                                row["RT_LOT_CR_DTTM"] = "";
                                row["ROLLPRESS_DATE"] = "";
                                row["FROM_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "SHOPID"));
                                row["FROM_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "AREAID"));
                                inLot.Rows.Add(row);
                            }


                            //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_MTRL_NJ", "INDATA,INLOT ", null, indataSet);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_MTRL_NJ", "INDATA,INLOT", null, (bizResult, bizException) =>
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                   // Util.AlertByBiz("BR_PRD_REG_RECEIVE_MTRL_NJ", bizException.Message, bizException.ToString());
                                    return;
                                }

                                Util.MessageInfo("SFU1798");   //입고 처리 되었습니다.
                                Initialize_dgReceive();

                            }, indataSet);
                        }
                        catch (Exception ex)
                        {
                           Util.MessageException(ex);
                          //  Util.Alert(ex.ToString());
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.Alert(ex.ToString());
                return;
            }
        }

        private void Initialize_dgReceive()
        {
            Util.gridClear(dgReceive);
            txtLotid.Text = null;
            txtRemark.Text = null;
            txtLotid.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sAreaID = cboArea2.SelectedValue.ToString();

                //if (cboArea2.SelectedValue.ToString().Trim().Equals("SELECT"))
                //{
                //    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동정보를 선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    Util.Alert("MMD0004");   //동을 선택해 주세요.
                //    return;
                //}
                //else
                //{
                //    sAreaID = cboArea2.SelectedValue.ToString();
                //}

                DataTable dt = null;
                DataRow row = null;

                dt = new DataTable();
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));                             

                row = dt.NewRow();
                row["WIPSTAT"] = Wip_State.WAIT;
                row["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                row["TO_DATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                row["AREAID"] = sAreaID == "" ? null : sAreaID;
                dt.Rows.Add(row);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_HIST_NJ", "RQSTDT", "RSLTDT", dt);

                Util.gridClear(dgReceive_Hist);
                Util.GridSetData(dgReceive_Hist, SearchResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.Alert(ex.ToString());
                return;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgReceive_Hist);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgReceive.IsReadOnly = false;
                    dgReceive.RemoveRow(index);
                    dgReceive.IsReadOnly = true;
                }
            });
        }

        #endregion

        ////private void rdoRoll_Click(object sender, RoutedEventArgs e)
        ////{
        ////    tbProcess.Visibility = Visibility.Visible;
        ////    cboProcid.Visibility = Visibility.Visible;
        ////}
        ////private void rdoPancake_Click(object sender, RoutedEventArgs e)
        ////{
        ////    tbProcess.Visibility = Visibility.Collapsed;
        ////    cboProcid.Visibility = Visibility.Collapsed;
        ////}

    }
}
