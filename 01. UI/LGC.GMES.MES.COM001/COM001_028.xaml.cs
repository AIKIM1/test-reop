/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.12.07  박성진   E20231130-001144   인수시 확인팝업 추가, 인수시 우측화면 필터와 관련없이 초기 조회목록 인수하도록 수정, 인수처리 후 인수한 수량 표시하도록 수정




 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_028 : UserControl, IWorkArea
    {

        Util _Util = new Util();
        bool _QMS_AREA_YN = false;

        #region Declaration & Constructor 
        public COM001_028()
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
            listAuth.Add(btnConfrim);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom3.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo3.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo combo = new CommonCombo();

            combo.SetCombo(cboMoveToArea2, CommonCombo.ComboStatus.ALL, sCase: "MOVETOAREA");

            C1ComboBox[] cboAreaChildFrom = { cboMoveToEquipmentSegment };
            combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChildFrom);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentFrom = { cboMoveToArea };
            combo.SetCombo(cboMoveToEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom, sCase: "EQUIPMENTSEGMENT");

            string[] sFilter = { "MOVE_ORD_STAT_CODE" };
            combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sCase: "MOVERECEIVE", sFilter: sFilter);

            string[] sFilter1 = { "MOVE_MTHD_CODE" };
            combo.SetCombo(cboTransType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);


            C1ComboBox[] cboAreaChildFrom2 = { cboHistToEquipmentSegment2 };
            combo.SetCombo(cboHistToArea2, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChildFrom2, sCase: "MOVETOAREA");

            C1ComboBox[] cboEquipmentSegmentParentFrom2 = { cboHistToArea2 };
            combo.SetCombo(cboHistToEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentFrom2, sCase: "EQUIPMENTSEGMENT");

            combo.SetCombo(cboStat2, CommonCombo.ComboStatus.ALL, sCase: "MOVERECEIVE", sFilter: sFilter);

            combo.SetCombo(cboTransType4, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            Set_Combo_WHID(cboWHID);

            // 전극만 해당 인수 시 RACK 입고 보이게 변경
            if (string.Equals(GetAreaType(), "E") && GetStkInVisibility())
                stRack.Visibility = Visibility.Visible;

            InitQMSYN();//품질정보 체크 동인지 확인
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom3.SelectedDataTimeChanged += dtpDateFrom3_SelectedDataTimeChanged;
            dtpDateTo3.SelectedDataTimeChanged += dtpDateTo3_SelectedDataTimeChanged;
        }

        private void InitQMSYN()
        {
            DataTable dtQmsChkTarget = Qms_Chk_Target(); //common에 등록된 품질검사 대상 동 담아둠

            if (dtQmsChkTarget != null && dtQmsChkTarget.Rows.Count > 0)
            {
                for (int i = 0; i < dtQmsChkTarget.Rows.Count; i++)
                {
                    if (dtQmsChkTarget.Rows[i]["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID))
                    {
                        _QMS_AREA_YN = true;
                    }
                }
            }
        }

        private DataTable Qms_Chk_Target()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "QMS_CHECK_AREA_MOVE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Event
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
                return;
            }
        }

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
        #endregion

        #region Mehod
        private void Set_Combo_WHID(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PRDT_WH_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.AlertByBiz("DA_BAS_SEL_PRDT_WH_CBO", Exception.Message, Exception.ToString());
                        return;
                    }
                    if (cbo.Name.Equals("cboWHID"))
                    {
                        if (result.Rows.Count > 0)
                        {
                            if (result.Rows[0][1].Equals("SRS"))
                            {
                                result.Rows.RemoveAt(0);
                            }
                        }
                    }
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    cbo.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private bool GetStkInVisibility()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "STOCK_IN_UNUSE_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0)
                    return false;
            }
            catch (Exception ex) { }

            return true;
        }

        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        private void Search_data()
        {
            Util.gridClear(dgMove_Detail);
            Util.gridClear(dgMove_Master);
            
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("FROM_AREAID", typeof(String));            
            RQSTDT.Columns.Add("TO_AREAID", typeof(String));
            RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
            RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["FROM_AREAID"] = Util.GetCondition(cboMoveToArea2, bAllNull: true);
            dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
            dr["MOVE_ORD_STAT_CODE"] = "MOVING";

            if (!string.IsNullOrEmpty(txtCarrierID.Text))
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr1 = dt.NewRow();
                dr1["CSTID"] = txtCarrierID.Text;

                dt.Rows.Add(dr1);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID_MOVE", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            
                            txtCarrierID.Focus();
                            txtCarrierID.SelectAll();
                        }
                    });
                    return;
                }
                else
                {
                    RQSTDT.Columns.Add("LOTID", typeof(String));
                    dr["LOTID"] = dtLot.Rows[0]["LOTID"].ToString(); 
                }
            }

            RQSTDT.Rows.Add(dr);

            //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);
            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_REC", "RQSTDT", "RSLTDT", RQSTDT);

            Util.GridSetData(dgMove_Master, SearchResult, FrameOperation, true);
            txtCarrierID.Text = string.Empty;
        }

        /// <summary>
        /// 입고처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU4273", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        btnConfrim.IsEnabled = false;
                        string sMoveOrderID = string.Empty;
                        string sArea = string.Empty;

                        if (dgMove_Master.GetRowCount() == 0 || dgMove_Detail_Hidden.GetRowCount() == 0)
                        {
                            Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                            return;
                        }

                        for (int i = 0; i < dgMove_Detail_Hidden.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgMove_Detail_Hidden.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() != "MOVING")
                            {
                                Util.Alert("SFU2926");     //이동중이 아닌 대상이 선택되었습니다.
                                return;
                            }
                        }

                        DataView dvRevCnt = DataTableConverter.Convert(dgMove_Detail_Hidden.ItemsSource).DefaultView;
                        dvRevCnt.RowFilter = "CHK = 'True' OR CHK = '1'";

                        if (stRack.Visibility == Visibility.Visible)
                        {
                            if (dvRevCnt.ToTable().Rows.Count > 0 && string.IsNullOrEmpty(txtRackID.Text.Trim()))
                            {
                                Util.Alert("70003");  //Rack 정보가 없습니다.
                                txtRackID.Focus();
                                return;
                            }
                        }

                        sMoveOrderID = DataTableConverter.GetValue(dgMove_Detail_Hidden.Rows[0].DataItem, "MOVE_ORD_ID").ToString();

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["MOVE_ORD_ID"] = sMoveOrderID;
                        row["AREAID"] = LoginInfo.CFG_AREA_ID; //Util.GetCondition(cboArea);
                        row["USERID"] = LoginInfo.USERID;
                        row["NOTE"] = txtRemark.Text.ToString();

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));

                        //2018-01-07 전극,조립 모두 Lot List에 있는 모든 항목 인수하도록 변경.
                        for (int i = 0; i < dgMove_Detail_Hidden.GetRowCount(); i++)
                        {
                            //if(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK").ToString() == "True" || DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK").ToString() == "1")
                            //{
                            //if (DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() == "MOVING")
                            //{
                            DataRow row2 = inLot.NewRow();
                            row2["LOTID"] = DataTableConverter.GetValue(dgMove_Detail_Hidden.Rows[i].DataItem, "LOTID").ToString();

                            indataSet.Tables["INLOT"].Rows.Add(row2);
                            //  }
                            //}
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_PACKLOT_AREA", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_RECEIVE_PACKLOT_AREA", bizException.Message, bizException.ToString());
                                    Util.MessageException(bizException);
                                    return;
                                }

                                DataTable dtReceive = ((DataView)dgMove_Detail_Hidden.ItemsSource).Table;

                                if (stRack.Visibility == Visibility.Visible)
                                {
                                    //2018-01-24 전극일 경우 Lot List에 있는 모든 항목(인수된 모든 항목)을 rack에 입고 하도록 변경
                                    //if (dtReceive.Select("CHK = 'True'").Count() > 0 || dtReceive.Select("CHK = '1'").Count() > 0)
                                    //{
                                    Receive();
                                    //}
                                    //else
                                    //{
                                    //    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                    //    Util.gridClear(dgMove_Detail);
                                    //    Util.gridClear(dgMove_Master);

                                    //    Search_data();
                                    //}
                                }
                                else
                                {
                                    //Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                    Util.MessageInfo("SFU2056", dgMove_Detail_Hidden.GetRowCount());
                                    Util.gridClear(dgMove_Detail);
                                    Util.gridClear(dgMove_Detail_Hidden);
                                    Util.gridClear(dgMove_Master);

                                    Search_data();
                                }
                            }
                            catch (Exception ex)
                            {
                                btnConfrim.IsEnabled = true;
                                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }

                        }, indataSet);

                        btnConfrim.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        btnConfrim.IsEnabled = true;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            });
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Search2_data();            
        }

        private void Search2_data()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom2.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);
                //string sPRJTNAME = txtPRJT_NAME.Text.Trim();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("END_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("END_TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_MTHD_CODE", typeof(String));
                //RQSTDT.Columns.Add("PRJT_NAME", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["END_FROM_DATE"] = sStart_date;
                dr["END_TO_DATE"] = sEnd_date;
                dr["FROM_AREAID"] = Util.GetCondition(cboMoveToArea, bAllNull: true);
                dr["FROM_EQSGID"] = Util.GetCondition(cboMoveToEquipmentSegment, bAllNull: true);
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat, bAllNull: true);
                dr["MOVE_MTHD_CODE"] = Util.GetCondition(cboTransType, bAllNull: true);
                //dr["PRJT_NAME"] = sPRJTNAME == "" ? null : sPRJTNAME;
                dr["PRODID"] = txtProd_ID.Text == "" ? null : txtProd_ID.Text;

                RQSTDT.Rows.Add(dr);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_REC", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMove_Master_Hist, SearchResult, FrameOperation, true);

                Util.gridClear(dgMove_Detail_Hist);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        /// <summary>
        /// 입고 처리 화면
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMove_MasterChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgMove_Master.BeginEdit();
                //    dgMove_Master.ItemsSource = DataTableConverter.Convert(dt);
                //    dgMove_Master.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgMove_Master.SelectedIndex = idx;

                SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail);
                SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail_Hidden);

                //for (int i = 0; i < dgMove_Detail.Rows.Count; i++)
                //{
                //    C1.WPF.DataGrid.DataGridRow row = dgMove_Detail.Rows[idx];
                //    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                //}
            }
            //else
            //{
            //    Util.gridClear(dgMove_Detail);
            //}
        }

        /// <summary>
        /// 이력 조회 화면
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgMove_Master_HistChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgMove_Master_Hist.BeginEdit();
                //    dgMove_Master_Hist.ItemsSource = DataTableConverter.Convert(dt);
                //    dgMove_Master_Hist.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i + 2)
                        DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i + 2].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgMove_Master_Hist.SelectedIndex = idx;

                SearchMoveLOTList(DataTableConverter.GetValue(dgMove_Master_Hist.Rows[idx].DataItem, "MOVE_ORD_ID").ToString(), dgMove_Detail_Hist);
            }
            //else
            //{
            //    Util.gridClear(dgMove_Detail_Hist);
            //}
        }

        private void SearchMoveLOTList(String sMoveOrderID, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            {
                //if (DataGrid == dgMove_Detail)
                //{
                //    DataTable RQSTDT = new DataTable();
                //    RQSTDT.TableName = "RQSTDT";
                //    RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));
                //    RQSTDT.Columns.Add("LANGID", typeof(String));

                //    DataRow dr = RQSTDT.NewRow();
                //    dr["MOVE_ORD_ID"] = sMoveOrderID;
                //    dr["LANGID"] = LoginInfo.LANGID;

                //    RQSTDT.Rows.Add(dr);

                //    DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                //    Util.GridSetData(DataGrid, DetailResult, FrameOperation, true);

                //}
                //else
                //{
                //    DataTable RQSTDT = new DataTable();
                //    RQSTDT.TableName = "RQSTDT";
                //    RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));
                //    RQSTDT.Columns.Add("LANGID", typeof(String));

                //    DataRow dr = RQSTDT.NewRow();
                //    dr["MOVE_ORD_ID"] = sMoveOrderID;
                //    dr["LANGID"] = LoginInfo.LANGID;

                //    RQSTDT.Rows.Add(dr);

                //    DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                //    Util.GridSetData(DataGrid, DetailResult, FrameOperation, true);
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["MOVE_ORD_ID"] = sMoveOrderID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(DataGrid, DetailResult, FrameOperation, true);

                //#region [R/P 미검사 색 지정]
                //for (int i = 0; i < DetailResult.Rows.Count; i++)
                //{
                //    if (string.Equals(DetailResult.Rows[i]["QMS_JUDG_VALUE"], "RNO") && DataGrid.GetCell(i, DataGrid.Columns["LOTID"].Index).Presenter != null)
                //    {
                //        DataGrid.GetCell(i, DataGrid.Columns["LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                //        DataGrid.GetCell(i, DataGrid.Columns["LOTID"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                //    }
                //}
                //#endregion

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //private void txtPRJT_NAME_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        if (txtPRJT_NAME.Text != "")
        //        {
        //            Search2_data();
        //        }
        //        else
        //        {
        //            Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
        //            return;
        //        }
        //    }
        //}

        private void txtProd_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID.Text != "")
                {
                    Search2_data();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            Search_MoveInfo();
        }

        private void txtProd_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID2.Text != "")
                {
                    Search_MoveInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            cboWHID.IsEnabled = true;
            txtRackID.Clear();
            txtRackID.IsReadOnly = false;
            txtRackID.Focus();

            foreach (C1.WPF.DataGrid.DataGridRow row in dgMove_Detail.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", "False");
            }

            dgMove_Detail.EndEdit();
            dgMove_Detail.EndEditRow(true);
        }

        private void Search_MoveInfo()
        {
            try
            {
                Util.gridClear(dgMove_Info);

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom3.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo3.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("END_FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("END_TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_MTHD_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["END_FROM_DATE"] = txtLotID2.Text != "" ? null : sStart_date;
                dr["END_TO_DATE"] = txtLotID2.Text != "" ? null : sEnd_date;
                dr["FROM_AREAID"] = Util.GetCondition(cboHistToArea2, bAllNull: true);
                dr["FROM_EQSGID"] = Util.GetCondition(cboHistToEquipmentSegment2, bAllNull: true);
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MOVE_TYPE_CODE"] = "MOVE_AREA";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat2, bAllNull: true);
                dr["MOVE_MTHD_CODE"] = Util.GetCondition(cboTransType4, bAllNull: true);
                dr["PRODID"] = txtProd_ID2.Text == "" ? null : txtProd_ID2.Text;
                dr["LOTID"] = txtLotID2.Text == "" ? null : txtLotID2.Text;

                RQSTDT.Rows.Add(dr);

                try
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("DA_PRD_SEL_MOVEINFO_REC_HIST", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.GridSetData(dgMove_Info, bizResult, FrameOperation);

                            string[] sColumnName = new string[] { "MOVE_ORD_ID" };
                            _Util.SetDataGridMergeExtensionCol(dgMove_Info, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    });

                }
                catch (Exception ex)
                {
                    //조회 오류
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void txtLotID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotID2.Text != "")
                {
                    Search_MoveInfo();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtCarrierID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtCarrierID.Text != "")
                {
                    Search_data();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void dgMove_Master_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                //if (e.Cell.Row.Type == DataGridRowType.Item)
                //{
                //    if (e.Cell.Column.Name.Equals("MOVE_STRT_DTTM"))
                //    {
                //        string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FLAG"));
                //        if (sDataType.Equals("Y"))
                //        {
                //            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                //        }
                //        else if (sDataType.Equals("N"))
                //        {
                //            e.Cell.Presenter.Background = null;
                //        }
                //    }
                //    else
                //    {
                //        e.Cell.Presenter.Background = null;
                //    }
                //}

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FLAG"));
                    if (sDataType.Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (sDataType.Equals("N"))
                    {
                        e.Cell.Presenter.Background = null;
                    }                    
                }
            }));
        }

        #region 입고처리
        private void cboWHID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtRackID.Focus();
        }

        private void txtRackID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string RackID = this.txtRackID.Text.Trim();
                    string WHID = cboWHID.SelectedValue.ToString();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACKID", typeof(string));
                    RQSTDT.Columns.Add("WH_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RACKID"] = RackID;
                    dr["WH_ID"] = WHID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_RACK", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        this.txtRackID.Text = RackID;
                        this.txtRackID.IsReadOnly = true;
                        this.cboWHID.IsEnabled = false;
                        //this.txtLotid.Focus();
                    }
                    else
                    {
                        //20170417 RACK ID 스캔 시 RACK에 해당하는 창고가 자동으로 선택되게 수정(권병훈C 요청사항)
                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT1";
                        RQSTDT1.Columns.Add("RACK_ID", typeof(string));
                        RQSTDT1.Columns.Add("AREAID", typeof(string));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["RACK_ID"] = RackID;
                        dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
                        RQSTDT1.Rows.Add(dr1);

                        DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_RACK_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

                        if (dtResult1.Rows.Count > 0)
                        {
                            this.txtRackID.Text = RackID;
                            this.txtRackID.IsReadOnly = true;
                            this.cboWHID.IsEnabled = false;
                            this.cboWHID.SelectedValue = dtResult1.Rows[0]["WH_ID"].ToString();
                            //this.txtLotid.Focus();
                        }
                        else
                        {
                            //{0}은(는) 창고에 없는 RACK ID 입니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2894", new object[] { RackID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                this.txtRackID.IsReadOnly = false;
                                this.cboWHID.IsEnabled = true;
                                this.txtRackID.Clear();
                                this.txtRackID.Focus();
                            });
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                    {
                        this.txtRackID.Clear();
                        this.txtRackID.Focus();
                    });
                }
            }
        }

        private void Receive()
        {
            //입고 하시겠습니까?
            Util.MessageConfirm("SFU2073", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("WH_ID", typeof(string));
                        inData.Columns.Add("RACK_ID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));


                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["WH_ID"] = Util.NVC(cboWHID.SelectedValue.ToString());                     
                        row["RACK_ID"] = txtRackID.Text.Trim() == "" ? null : Util.NVC(txtRackID.Text.Trim());                       
                        row["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgMove_Detail.GetRowCount(); i++)
                        {
                            //2018-01-24 Lot List에 있는 모든 항목 rack에 입고 하도록 변경
                            //if (Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK")) == "True")
                            //{
                            row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "LOTID"));
                                inLot.Rows.Add(row);
                            //}
                        }
                        
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_IN", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_STOCK_IN", bizException.Message, bizException.Message);
                                return;
                            }

                            Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                            Util.gridClear(dgMove_Detail);
                            Util.gridClear(dgMove_Master);
                            cboWHID.IsEnabled = true;
                            txtRackID.Clear();
                            txtRackID.IsReadOnly = false;
                            txtRackID.Focus();

                            Search_data();

                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }
        #endregion

        private void dgMove_Detail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (!_QMS_AREA_YN)
            {
                return;
            }

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                ////link 색변경
                //if (e.Cell.Column.Name == "QMS_JUDG_VALUE")
                //{
                //    if (e.Cell.Value != null)
                //    {
                //        if (e.Cell.Value.ToString() != "P")
                //        {
                //            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                //        }
                //    }
                //}
                #region [QMS 판정값 FAIL]
                int row_idx = e.Cell.Row.Index;
                var qms_vlaue = DataTableConverter.GetValue(dataGrid.Rows[row_idx].DataItem, "QMS_JUDG_VALUE");

                if (qms_vlaue != null)
                {
                    if (qms_vlaue.ToString() != "P")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
                #endregion

            }));
        }

       
    }
}
