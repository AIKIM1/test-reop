/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
    일자       개발자           CSR번호                        내용
 ----------- ------------ ------------------ --------------------------------------------
  2016.06.16  DEVELOPER : Initial Created.
  2019.04.19                                 소스원복
  2019.04.19                                 변경 집합번호 7625와 동일하게 복사하여 체크인
  2022.03.17  정연재     C20220314-000565     사외반품용 체크박스/비즈추가  및 사외반품현황 Tab 추가    

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_017 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        public bool bScanflag = false;
        public bool bLoadflag = false;
        public string pREMARK = string.Empty;
        public string pUSER = string.Empty;
        public string pRCV_ISS_ID = string.Empty;
        public string sSAVE_SEQNO = string.Empty;
        public int iOld_Index = 0;
        public int iNow_Index = 0;
        public int iBack = 0;

        private string sRETURN_TYPE_CODE = string.Empty;
        private string sRETURN_TYPE_CODE_HIST = string.Empty;

        #region Declaration & Constructor 
        public BOX001_017()
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
            listAuth.Add(btnTempStorage);
            listAuth.Add(btnConfrim);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
            dtpDateTo.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

            dtpDateFrom2.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
            dtpDateTo2.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

            //
            dtpDateFrom3.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo3.Text = DateTime.Now.ToString("yyyy-MM-dd");

            CommonCombo combo = new CommonCombo();

            //combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL);
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            C1ComboBox[] cboToChild = { cboTransLoc };
            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, cbChild: cboToChild, sCase: "AREA_CP");

            //combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT);
            C1ComboBox[] cboCompParent = { cboArea2 };
            combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

            if(LoginInfo.CFG_SHOP_ID == "G184")
            {
                // 반품유형 콤보
                cboReturnType.IsEnabled = true;
                cboReturnType.Visibility = Visibility.Visible;
                txtReturnType.Visibility = Visibility.Visible;

                cboReturnTypeHist.IsEnabled = true;
                cboReturnTypeHist.Visibility = Visibility.Visible;
                txtReturnTypeHist.Visibility = Visibility.Visible;

                SetReturnTypeCombo();
            }
            else
            {
                cboReturnType.IsEnabled = false;
                cboReturnType.Visibility = Visibility.Collapsed;
                txtReturnType.Visibility = Visibility.Collapsed;

                cboReturnTypeHist.IsEnabled = false;
                cboReturnTypeHist.Visibility = Visibility.Collapsed;
                txtReturnTypeHist.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void SetReturnTypeCombo()
        {
            try
            {
                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string)); ;

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "RETURN_TYPE_CODE";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "SELECT";
                dr["CBO_NAME"] = "- SELECT -";
                dtResult.Rows.InsertAt(dr, 0);

                cboReturnType.DisplayMemberPath = "CBO_NAME";
                cboReturnType.SelectedValuePath = "CBO_CODE";
                cboReturnType.ItemsSource = dtResult.Copy().AsDataView();
                cboReturnType.SelectedIndex = 0;

                cboReturnTypeHist.DisplayMemberPath = "CBO_NAME";
                cboReturnTypeHist.SelectedValuePath = "CBO_CODE";
                cboReturnTypeHist.ItemsSource = dtResult.Copy().AsDataView();
                cboReturnTypeHist.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region Event

        /*
        private void SearchCell_ReturnList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["RCV_ISS_ID"] = null;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgRetrunCellList);
                //    dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        */

        // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
        private void dgRetrunCellList_Choice_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox rb = sender as CheckBox;
            int idx = 0;

            if (rb.DataContext == null)
                return;

            if (!(bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
            {
                idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                iNow_Index = idx;

                DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);
                

                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        if (idx == i)
                        {
                            dt.Rows[i]["CHK"] = false;
                            break;
                        }
                    }

                    dgRetrunCellList.BeginEdit();
                    //  dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                    Util.GridSetData(dgRetrunCellList, dt, FrameOperation, true);
                    dgRetrunCellList.EndEdit();
                }

                //row 색 바꾸기
                //dgRetrunCellList.SelectedIndex = idx;

                //Util.gridClear(dgReturnBoxList);
                //Util.gridClear(dgReturnLotList);
                //Util.gridClear(dgCellInfo);

                Return_Box_List_detach(idx);

                txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                //iOld_Index = iNow_Index;
                //iBack = 0;
            }
        }

        private void dgRetrunCellList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            int idx = 0;

            if (sender == null)
                return;

            // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
            //RadioButton rb = sender as RadioButton;
            CheckBox rb = sender as CheckBox;

            if (rb.DataContext == null)
                return;


            if (bScanflag == true && iBack == 1)
            {
                //if (iOld_Index == iNow_Index)
                iBack = 0;
                return;
            }

            if (bLoadflag == true && iBack == 1)
            {
                //if (iOld_Index == iNow_Index)
                iBack = 0;
                return;
            }


            if (bScanflag == true || bLoadflag == true)
            {
                //스캔한 데이터를 초기화 하시겠습니까?
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3133"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3133", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                        {

                            idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

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

                            //    dgRetrunCellList.BeginEdit();
                            //    dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                            //    dgRetrunCellList.EndEdit();
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
                            dgRetrunCellList.SelectedIndex = idx;

                            Util.gridClear(dgReturnBoxList);
                            Util.gridClear(dgReturnLotList);
                            Util.gridClear(dgCellInfo);

                            Return_Box_List(idx);

                            bScanflag = false;
                            bLoadflag = false;

                            txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                            iNow_Index = idx;
                            iOld_Index = idx;
                            iBack = 0;
                        }
                    }
                    else
                    {

                        if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                        {

                            //int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                            DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                            if (dt != null)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    DataRow row = dt.Rows[i];

                                    if (iOld_Index == i)
                                        dt.Rows[i]["CHK"] = true;
                                    else
                                        dt.Rows[i]["CHK"] = false;
                                }

                                dgRetrunCellList.BeginEdit();
                                // dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                                Util.GridSetData(dgRetrunCellList, dt, FrameOperation, true);
                                dgRetrunCellList.EndEdit();

                                iBack = 1;
                            }

                            //row 색 바꾸기
                            //dgRetrunCellList.SelectedIndex = iIndex;

                            //Util.gridClear(dgReturnBoxList);
                            //Util.gridClear(dgReturnLotList);
                            //Util.gridClear(dgCellInfo);

                            //Return_Box_List(idx);

                            //bScanflag = false;

                            //txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());
                        }
                    }

                });
            }
            else
            {
                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    iNow_Index = idx;

                    DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);
                    
                    // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
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

                    //    dgRetrunCellList.BeginEdit();
                    //    //  dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                    //    Util.GridSetData(dgRetrunCellList, dt, FrameOperation, true);
                    //    dgRetrunCellList.EndEdit();
                    //}

                    ////row 색 바꾸기
                    //dgRetrunCellList.SelectedIndex = idx;

                    //Util.gridClear(dgReturnBoxList);
                    //Util.gridClear(dgReturnLotList);
                    //Util.gridClear(dgCellInfo);

                    //Return_Box_List(idx);

                    //txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                    //iOld_Index = iNow_Index;
                    //iBack = 0;

                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];

                            if (idx == i)
                            {
                                dt.Rows[i]["CHK"] = true;
                                break;
                            }
                        }

                        dgRetrunCellList.BeginEdit();
                        //  dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                        Util.GridSetData(dgRetrunCellList, dt, FrameOperation, true);
                        dgRetrunCellList.EndEdit();
                    }

                    //row 색 바꾸기
                    dgRetrunCellList.SelectedIndex = idx;

                    //Util.gridClear(dgReturnBoxList);
                    //Util.gridClear(dgReturnLotList);
                    //Util.gridClear(dgCellInfo);

                    Return_Box_List(idx);

                    txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                    iOld_Index = iNow_Index;
                    iBack = 0;
                }
            }
        }

        private void Return_Box_List_detach(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgRetrunCellList.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

                // 반품 BOX 리스트 제외
                if (dgReturnBoxList.ItemsSource != null)
                {
                    DataTable BOXList = ((DataView)dgReturnBoxList.ItemsSource).ToTable();

                    BOXList.AcceptChanges();

                    foreach (DataRow dr in BOXList.Rows)
                    {
                        if (dr["RCV_ISS_ID"].ToString().Equals(sRCV_ISS_ID))
                            dr.Delete();
                    }

                    BOXList.AcceptChanges();

                    Util.GridSetData(dgReturnBoxList, BOXList, FrameOperation, true);
                }

                // 반품 LOT 리스트 제외
                if (dgReturnLotList.ItemsSource != null)
                {
                    DataTable LOTList = ((DataView)dgReturnLotList.ItemsSource).ToTable();

                    LOTList.AcceptChanges();

                    foreach (DataRow dr in LOTList.Rows)
                    {
                        if (dr["RCV_ISS_ID"].ToString().Equals(sRCV_ISS_ID))
                            dr.Delete();
                    }

                    LOTList.AcceptChanges();

                    Util.GridSetData(dgReturnLotList, LOTList, FrameOperation, true);
                }

                if (DataTableConverter.GetValue(dgRetrunCellList.Rows[idx].DataItem, "CELL_INFO").ToString() == "Y")
                {
                    // 반품 CELL 리스트 제외
                    if (dgCellInfo.ItemsSource != null)
                    {
                        DataTable CELLList = ((DataView)dgCellInfo.ItemsSource).ToTable();

                        CELLList.AcceptChanges();

                        foreach (DataRow dr in CELLList.Rows)
                        {
                            if (dr["RCV_ISS_ID"].ToString().Equals(sRCV_ISS_ID))
                                dr.Delete();
                        }

                        CELLList.AcceptChanges();

                        Util.GridSetData(dgCellInfo, CELLList, FrameOperation, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Return_Box_List(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;
                string sBizLotCall = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgRetrunCellList.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_BOX_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                //Util.gridClear(dgReturnBoxList);

                if (BOXList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1547");  //반품 BOX 리스트 조회 항목이 없습니다.
                    return;
                }
                // dgReturnBoxList.ItemsSource = DataTableConverter.Convert(BOXList);

                // 기존 Item에 Add
                if (dgReturnBoxList.ItemsSource != null)
                    BOXList.Merge(((DataView)dgReturnBoxList.ItemsSource).ToTable());                

                Util.GridSetData(dgReturnBoxList, BOXList, FrameOperation, true);


                // 반품 LOT 리스트 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT1.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr1["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";

                RQSTDT1.Rows.Add(dr1);

                sBizLotCall = "DA_PRD_SEL_RETURN_LOT_LIST";

                //2022.03.17  정연재 C20220314-000565 사외반품체크 박스 True
                if (chkOutCellRtn.IsChecked == true)
                {
                    sBizLotCall = "DA_PRD_SEL_OUT_CELL_RETURN_LOT_LIST";
                }

                DataTable LOTList = new ClientProxy().ExecuteServiceSync(sBizLotCall, "RQSTDT", "RSLTDT", RQSTDT1);

                //Util.gridClear(dgReturnLotList);

                if (LOTList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                    return;
                }

                //  dgReturnLotList.ItemsSource = DataTableConverter.Convert(LOTList);
                
                // 기존 Item에 Add
                if (dgReturnLotList.ItemsSource != null)
                    LOTList.Merge(((DataView)dgReturnLotList.ItemsSource).ToTable());

                Util.GridSetData(dgReturnLotList, LOTList, FrameOperation, true);

                if (DataTableConverter.GetValue(dgRetrunCellList.Rows[idx].DataItem, "CELL_INFO").ToString() == "Y")
                {
                    // 반품 CELL 리스트 조회
                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.TableName = "RQSTDT";
                    RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT2.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                    DataRow dr2 = RQSTDT2.NewRow();
                    dr2["RCV_ISS_ID"] = sRCV_ISS_ID;
                    dr2["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";

                    RQSTDT2.Rows.Add(dr2);

                    DataTable CELLList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                    //Util.gridClear(dgCellInfo);

                    if (CELLList.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU1548");  //반품 CELL 리스트 조회 항목이 없습니다.
                        return;
                    }
                    //dgCellInfo.ItemsSource = DataTableConverter.Convert(CELLList);

                    // 기존 Item에 Add
                    if (dgCellInfo.ItemsSource != null)
                        CELLList.Merge(((DataView)dgCellInfo.ItemsSource).ToTable());

                    Util.GridSetData(dgCellInfo, CELLList, FrameOperation, true);

                    txtCell_ID.IsReadOnly = true;
                    txtCell_ID.Text = "";
                }
                else
                {
                    txtCell_ID.IsReadOnly = false;
                    txtCell_ID.Text = "";
                    txtCell_ID.Focus();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRetrunCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                // check 한 항목의 iss_qty 총 합으로 수정
                //double dReturnQty = Convert.ToDouble(drChk[0]["ISS_QTY"].ToString());
                double dReturnQty = 0.0;

                foreach (DataRow dr in drChk)
                {
                    dReturnQty += Convert.ToDouble(dr["ISS_QTY"].ToString());
                }

                int iRowCnt = dgCellInfo.GetRowCount();
                
                if (sRETURN_TYPE_CODE != "RMA" && dReturnQty != iRowCnt)
                {
                    Util.MessageValidation("SFU3134");  //반품수량과 Cell 수량이 일치하지 않습니다.
                    return;
                }

                //string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                BOX001_017_CONFIRM wndConfirm = new BOX001_017_CONFIRM(iRowCnt);
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                    //object[] Parameters = new object[1];
                    //Parameters[0] = sRCV_ISS_ID;

                    object[] Parameters = new object[drChk.Length + 1];

                    Parameters[0] = sRETURN_TYPE_CODE;

                    for (int i = 1; i < drChk.Length + 1; i++)
                    {
                        Parameters[i] = drChk[i - 1]["RCV_ISS_ID"].ToString();
                    }

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //사외반품확정 버튼  2022.03.15
        private void btnOutSideConfrim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sExceptMsg = string.Empty;
                string sRSO_NO = string.Empty;
                string sBOX_ID = string.Empty;

                if (dgRetrunCellList3.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList3, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }
                else
                {
                    sRSO_NO = drChk[0].ItemArray[4].ToString();
                    sBOX_ID = drChk[0].ItemArray[5].ToString();
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }


                //반품확정 하시겠습니까?
                Util.MessageConfirm("SFU2869", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    { UpdateRtnCell(sBOX_ID, sRSO_NO); }
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


        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_017_CONFIRM window = sender as BOX001_017_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                System.Windows.Forms.Application.DoEvents();
                Comfrim_ReturnCell(window.sNOTE);
            }
            grdMain.Children.Remove(window);
        }

        private void Comfrim_ReturnCell(string sNote)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                int iExceptCnt = 0;
                string sExceptMsg = string.Empty;

                // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                foreach (DataRow dr in drChk)
                {
                    string sRCV_ISS_ID = string.Empty;

                    //sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();
                    sRCV_ISS_ID = dr["RCV_ISS_ID"].ToString();

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("RCV_ISS_ID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("RCV_QTY", typeof(string));
                    inData.Columns.Add("PROCID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("RCV_NOTE", typeof(string));
                    inData.Columns.Add("RETURN_TYPE_CODE", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["RCV_ISS_ID"] = sRCV_ISS_ID;
                    row["AREAID"] = sAREAID;//LoginInfo.CFG_AREA_ID;
                    //row["RCV_QTY"] = dgCellInfo.GetRowCount();
                    if(sRETURN_TYPE_CODE == "RMA")
                    {
                        row["RCV_QTY"] = dr["ISS_QTY"].ToString();
                    }
                    else
                    {
                        row["RCV_QTY"] = ((DataView)dgCellInfo.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'").Length;
                    }
                    row["PROCID"] = Process.CELL_BOXING;//"B1000";
                    row["USERID"] = txtWorker.Tag as string; //LoginInfo.USERID;
                    row["RCV_NOTE"] = sNote;
                    row["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                    indataSet.Tables["INDATA"].Rows.Add(row);


                    DataTable inPallet = indataSet.Tables.Add("INPALLET");
                    inPallet.Columns.Add("BOXID", typeof(string));
                    inPallet.Columns.Add("RCV_QTY", typeof(string));

                    //for (int i = 0; i < dgReturnLotList.GetRowCount(); i++)
                    //{
                    DataRow row2 = inPallet.NewRow();
                    //row2["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[0].DataItem, "BOXID").ToString();
                    //row2["RCV_QTY"] = dgCellInfo.GetRowCount();
                    row2["BOXID"] = ((DataView)dgReturnLotList.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'")[0]["BOXID"].ToString();
                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        row2["RCV_QTY"] = dr["ISS_QTY"].ToString();
                    }
                    else
                    {
                        row2["RCV_QTY"] = ((DataView)dgCellInfo.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'").Length;
                    }

                    indataSet.Tables["INPALLET"].Rows.Add(row2);
                    //}


                    DataTable inBox = indataSet.Tables.Add("INBOX");
                    inBox.Columns.Add("BOXID", typeof(string));
                    inBox.Columns.Add("LOTID", typeof(string));

                    //for (int i = 0; i < dgReturnLotList.GetRowCount(); i++)
                    //{
                    //    DataRow row3 = inBox.NewRow();
                    //    row3["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "BOXID").ToString();
                    //    row3["LOTID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString();

                    //    indataSet.Tables["INBOX"].Rows.Add(row3);
                    //}
                    foreach (DataRow drReturnLot in ((DataView)dgReturnLotList.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'"))
                    {
                        DataRow row3 = inBox.NewRow();
                        //row3["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "BOXID").ToString();
                        //row3["LOTID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString();
                        row3["BOXID"] = drReturnLot["BOXID"].ToString();
                        row3["LOTID"] = drReturnLot["LOTID"].ToString();

                        indataSet.Tables["INBOX"].Rows.Add(row3);
                    }


                    DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                    inLot.Columns.Add("BOXID", typeof(string));
                    inLot.Columns.Add("SUBLOTID", typeof(string));

                    //for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                    //{
                    //    DataRow row4 = inLot.NewRow();
                    //    row4["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[0].DataItem, "BOXID").ToString();
                    //    row4["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString();

                    //    indataSet.Tables["INSUBLOT"].Rows.Add(row4);
                    //}
                    if (sRETURN_TYPE_CODE != "RMA")
                    {
                        foreach (DataRow drCellInfo in ((DataView)dgCellInfo.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'"))
                        {
                            DataRow row4 = inLot.NewRow();
                            //row4["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[0].DataItem, "BOXID").ToString();
                            //row4["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString();
                            row4["BOXID"] = ((DataView)dgReturnLotList.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'")[0]["BOXID"].ToString();
                            row4["SUBLOTID"] = drCellInfo["SUBLOTID"].ToString();

                            indataSet.Tables["INSUBLOT"].Rows.Add(row4);
                        }
                    }


                    System.Windows.Forms.Application.DoEvents();

                    //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_SHIP_CELL", "INDATA,INPALLET,INBOX,INSUBLOT", null, (result, ex) => {

                    //    loadingIndicator.Visibility = Visibility.Collapsed;

                    //    if (ex == null)
                    //    {
                    //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    //        Init_Form();
                    //    }
                    //    else
                    //    {
                    //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.Message, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //    }


                    //}, indataSet);

                    // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_SHIP_CELL", "INDATA,INPALLET,INBOX,INSUBLOT", null, indataSet);                        
                        
                    }
                    catch(Exception ex)
                    {
                        // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                        // 로직 처리 중 Exception 발생 시 한번에 Alert 보여주기 위하여 추가
                        iExceptCnt++;

                        if (!sExceptMsg.Equals("") || sExceptMsg != string.Empty)
                            sExceptMsg += Environment.NewLine;

                        sExceptMsg += sRCV_ISS_ID + " : " + MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]));

                        //Util.MessageException(ex);

                    }
                    //정상 처리 되었습니다.
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                }

                if (iExceptCnt > 0)
                {
                    Util.Alert(sExceptMsg);
                }
                else
                {
                    Util.MessageInfo("SFU1275");                    
                }

                Init_Form();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Scan_CellID()
        {
            try
            {
                string sCell_ID = string.Empty;
                string sRCV_ISS_ID = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk == null || drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtCell_ID.Text.ToString() == "")
                {
                    //입력한 CELL ID 가 없습니다. >> CELL ID를 입력 하세요.
                    Util.MessageValidation("SFU1319");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1319"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dgReturnLotList.GetRowCount() == 0)
                {
                    //반품 LOT 정보가 없습니다.
                    Util.MessageValidation("SFU1195");
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1195"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                double dReturnQty = Convert.ToDouble(drChk[0]["ISS_QTY"].ToString());

                int iRowCnt = dgCellInfo.GetRowCount();

                if (dReturnQty <= iRowCnt)
                {
                    //반품 수량을 넘었습니다.
                    Util.MessageValidation("SFU1551");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1551"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();
                sCell_ID = txtCell_ID.Text.ToString();

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("SRCTYPE", typeof(String));
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["SUBLOTID"] = sCell_ID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOXID"] = DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "BOXID").ToString();

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_INFO_FOR_RETURN", "INDATA", "OUTDATA", RQSTDT);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_CELL_INFO_FOR_RETURN", "INDATA", "OUTDATA", ds);

                if (dsRslt.Tables["OUTDATA"].Rows.Count == 0)
                {
                    //스캔한 CELL ID의 정보가 없습니다.
                    Util.MessageValidation("SFU1689");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1689"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString() == sCell_ID)
                    {
                        //중복 스캔되었습니다.
                        Util.MessageValidation("SFU1914");
                        //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1914"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                #region 완제품 ID 존재 여부 체크
                if (ChkUseTopProdID() && dgReturnBoxList.Columns.Contains("TOP_PRODID"))
                {
                    // RCS_ISS_PLLT 는 조회 DA에서 PRODID 는 반제품, TOP_PRODID 는 완제품으로 전환..
                    // Sublot 의 제품 코드는 일부 마이그 하기로 함에 따라 반제품일 수도, 완제품일 수도 있음....
                    if (!(Util.NVC(DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "PRODID")) == Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"]) || 
                          Util.NVC(DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "TOP_PRODID")) == Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"])))
                    {
                        //반품 제품과 다른 제품 입니다. >> 다른 제품을 선택하셨습니다.
                        Util.MessageValidation("SFU1480");
                        //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1480"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    if (DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "PRODID").ToString() != dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString())
                    {
                        //반품 제품과 다른 제품 입니다. >> 다른 제품을 선택하셨습니다.
                        Util.MessageValidation("SFU1480");
                        //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1480"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }                
                #endregion


                bool bLotCheck = false;
                for (int i = 0; i < dgReturnLotList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString() == " ")
                    {
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "QTY", 1);
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "RETURN_QTY", 1);
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "LOTID", dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString());
                        bLotCheck = true;
                        break;
                    }

                    if (DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString() == dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString())
                    {
                        int iQty = 0;

                        iQty = Convert.ToInt32(DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "QTY").ToString()) + 1;

                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "QTY", iQty);
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "RETURN_QTY", iQty);
                        bLotCheck = true;
                        break;
                    }
                }

                if (bLotCheck == false)
                {
                    dgReturnLotList.IsReadOnly = false;
                    dgReturnLotList.BeginNewRow();
                    dgReturnLotList.EndNewRow(true);
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "RCV_ISS_ID", dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "BOXID", DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "BOXID").ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "LOTID", dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "PROJECTNAME", (DataTableConverter.GetValue(dgReturnLotList.Rows[0].DataItem, "PROJECTNAME").ToString()));
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "RETURN_QTY", 1);
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "QTY", 1);
                    dgReturnLotList.IsReadOnly = true;
                }


                if (dgCellInfo.GetRowCount() == 0)
                {
                    //dgCellInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA"]);
                    Util.GridSetData(dgCellInfo, dsRslt.Tables["OUTDATA"], FrameOperation, true);
                }
                else
                {
                    dgCellInfo.IsReadOnly = false;
                    dgCellInfo.BeginNewRow();
                    dgCellInfo.EndNewRow(true);
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "RCV_ISS_ID", dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "SUBLOTID", dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOTID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "NOTE", dsRslt.Tables["OUTDATA"].Rows[0]["NOTE"].ToString());
                    dgCellInfo.IsReadOnly = true;
                }

                bScanflag = true;
                txtCell_ID.Focus();
                txtCell_ID.SelectAll();

                //dgRetrunCellList.IsEnabled = false;

                txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtCell_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Scan_CellID();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRetrunCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                pRCV_ISS_ID = null;

                BOX001_017_LOAD wndConfirm = new BOX001_017_LOAD();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    //object[] Parameters = new object[2];
                    //Parameters[0] = sRCV_ISS_ID;
                    //Parameters[1] = dtTempInfo;

                    //C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Load);
                    // 팝업 화면 숨겨지는 문제 수정.
                    // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Load(object sender, EventArgs e)
        {
            BOX001_017_LOAD window = sender as BOX001_017_LOAD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                sSAVE_SEQNO = window.pSave_Seqno;
                Search_LoadList(sSAVE_SEQNO);

                bLoadflag = true;
            }
            grdMain.Children.Remove(window);
        }

        private void Search_LoadList(string sSave_no)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {   //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SAVE_SEQNO", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SAVE_SEQNO"] = sSave_no;
                dr["BOXID"] = DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "BOXID").ToString();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LOAD_CELL", "RQSTDT", "RSLTDT", RQSTDT);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TMP_SAVE_FOR_RETURN", "INDATA", "OUTDATA_LOT,OUTDATA_SUBLOT", ds);

                Util.gridClear(dgCellInfo);
                // dgCellInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA_SUBLOT"]);
                Util.GridSetData(dgCellInfo, dsRslt.Tables["OUTDATA_SUBLOT"], FrameOperation, true);

                Util.gridClear(dgReturnLotList);
                //   dgReturnLotList.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA_LOT"]);
                Util.GridSetData(dgReturnLotList, dsRslt.Tables["OUTDATA_LOT"], FrameOperation, true);

                txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void btnTempStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgRetrunCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                pREMARK = null;
                pUSER = null;

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                if (dgCellInfo.GetRowCount() == 0)
                {   //Cell 정보가 없습니다.
                    Util.MessageValidation("SFU1209");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1209"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataTable dtTempInfo = new DataTable();
                dtTempInfo.TableName = "RQSTDT";
                dtTempInfo.Columns.Add("RCV_ISS_ID", typeof(String));
                dtTempInfo.Columns.Add("BOXID", typeof(String));
                dtTempInfo.Columns.Add("LOTID", typeof(String));
                dtTempInfo.Columns.Add("PRODID", typeof(String));
                dtTempInfo.Columns.Add("RETURN_QTY", typeof(String));
                dtTempInfo.Columns.Add("QTY", typeof(String));
                //dtTempInfo.Columns.Add("SCAN_QTY", typeof(String));

                for (int i = 0; i < dgReturnLotList.GetRowCount(); i++)
                {
                    DataRow dr = dtTempInfo.NewRow();
                    dr["RCV_ISS_ID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "RCV_ISS_ID").ToString();
                    dr["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "BOXID").ToString();
                    dr["LOTID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString();
                    dr["PRODID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "PRODID").ToString();
                    dr["RETURN_QTY"] = DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "RETURN_QTY").ToString();
                    dr["QTY"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "QTY").ToString();
                    //dr["SCAN_QTY"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "SCAN_QTY").ToString();

                    dtTempInfo.Rows.Add(dr);
                }

                BOX001_017_TEMP_CELL wndConfirm = new BOX001_017_TEMP_CELL();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sRCV_ISS_ID;
                    Parameters[1] = dtTempInfo;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Temp);
                    // 팝업 화면 숨겨지는 문제 수정
                    // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }

                bScanflag = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Temp(object sender, EventArgs e)
        {
            BOX001_017_TEMP_CELL window = sender as BOX001_017_TEMP_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                pREMARK = window.pRemark;
                //pUSER = window.pUser;
                TempStorage(pREMARK);
            }
            grdMain.Children.Remove(window);
        }

        private void TempStorage(string sRemark)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");

                string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("PACK_TMP_TYPE_CODE", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("NOTE", typeof(string));
                inData.Columns.Add("RCV_ISS_ID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["PACK_TMP_TYPE_CODE"] = "RETURN_CELL";
                row["USERID"] = txtWorker.Tag as string;
                row["NOTE"] = sRemark;
                row["RCV_ISS_ID"] = sRCV_ISS_ID;
                row["PRODID"] = DataTableConverter.GetValue(dgCellInfo.Rows[0].DataItem, "PRODID").ToString();

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                inLot.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                {
                    DataRow row2 = inLot.NewRow();
                    row2["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString();

                    indataSet.Tables["INSUBLOT"].Rows.Add(row2);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TMP_PACK_CELL", "INDATA,INSUBLOT", null, indataSet);
                //정상 처리 되었습니다.
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageInfo("SFU1275");
                Init_Form();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void dgRetrunCellList2_Choice_Checked(object sender, RoutedEventArgs e)
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

                //    dgRetrunCellList2.BeginEdit();
                //    dgRetrunCellList2.ItemsSource = DataTableConverter.Convert(dt);
                //    dgRetrunCellList2.EndEdit();
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
                dgRetrunCellList2.SelectedIndex = idx;

                Util.gridClear(dgReturnBoxList2);
                Util.gridClear(dgReturnLotList2);
                Util.gridClear(dgCellInfo2);

                Return_Hist(idx);

                txtCount2.Text = "Count : " + Convert.ToString(dgCellInfo2.GetRowCount());

            }
        }

        //사외반품셀
        private void dgRetrunCellList3_Choice_Checked(object sender, RoutedEventArgs e)
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

                //row 색 바꾸기
                dgRetrunCellList3.SelectedIndex = idx;

                Util.gridClear(dgReturnBoxList3);
                Util.gridClear(dgReturnLotList3);
                Util.gridClear(dgCellInfo3);

                Return_OutHist(idx);

                txtCount3.Text = "Count : " + Convert.ToString(dgCellInfo3.GetRowCount());

            }
        }


        private void Return_Hist(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgRetrunCellList2.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOX_RCV_ISS_STAT_CODE"] = "END_RECEIVE";

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_BOX_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgReturnBoxList2);

                if (BOXList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1547");  //반품 BOX 리스트 조회 항목이 없습니다.
                    return;
                }
                //  dgReturnBoxList2.ItemsSource = DataTableConverter.Convert(BOXList);
                Util.GridSetData(dgReturnBoxList2, BOXList, FrameOperation, true);

                // 반품 LOT 리스트 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT1.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr1["BOX_RCV_ISS_STAT_CODE"] = "END_RECEIVE";

                RQSTDT1.Rows.Add(dr1);

                string sBizName = "DA_PRD_SEL_RETURN_LOT_HIST_LIST";

                if (sRETURN_TYPE_CODE_HIST == "RMA")
                {
                    sBizName = "DA_PRD_SEL_RETURN_LOT_HIST_LIST_RMA";
                }
                else
                {
                    sBizName = "DA_PRD_SEL_RETURN_LOT_HIST_LIST";
                }

                DataTable LOTList = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT1);

                Util.gridClear(dgReturnLotList2);

                if (LOTList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                    return;
                }
                //    dgReturnLotList2.ItemsSource = DataTableConverter.Convert(LOTList);
                Util.GridSetData(dgReturnLotList2, LOTList, FrameOperation, true);

                // 반품 CELL 리스트 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT2.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr2["BOX_RCV_ISS_STAT_CODE"] = "END_RECEIVE";

                RQSTDT2.Rows.Add(dr2);

                DataTable CELLList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                Util.gridClear(dgCellInfo2);

                if (CELLList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1548");  //반품 CELL 리스트 조회 항목이 없습니다.
                    return;
                }
                //   dgCellInfo2.ItemsSource = DataTableConverter.Convert(CELLList);
                Util.GridSetData(dgCellInfo2, CELLList, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //사외반품셀
        private void Return_OutHist(int idx)
        {
            try
            {

                string sOCOP_RTN_CNSN_APPR_NO = string.Empty;
                string sRSO_NO = string.Empty;
                string sBOXID = string.Empty;

                string sMES_CNFM_FLAG = "N";


                sOCOP_RTN_CNSN_APPR_NO = DataTableConverter.GetValue(dgRetrunCellList3.Rows[idx].DataItem, "OCOP_RTN_CNSN_APPR_NO").ToString();
                sRSO_NO = DataTableConverter.GetValue(dgRetrunCellList3.Rows[idx].DataItem, "RTN_SALES_ORD_NO").ToString();
                sBOXID = DataTableConverter.GetValue(dgRetrunCellList3.Rows[idx].DataItem, "BOXID").ToString();
                sMES_CNFM_FLAG = DataTableConverter.GetValue(dgRetrunCellList3.Rows[idx].DataItem, "MES_CNFM_FLAG").ToString();

                if (sMES_CNFM_FLAG == "N")
                {
                    btnOutSideConfrim.IsEnabled = true;
                }
                else
                {
                    btnOutSideConfrim.IsEnabled = false;
                }

                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("OCOP_RTN_CNSN_APPR_NO", typeof(String));
                RQSTDT.Columns.Add("RTN_SALES_ORD_NO", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["OCOP_RTN_CNSN_APPR_NO"] = sOCOP_RTN_CNSN_APPR_NO;
                dr["RTN_SALES_ORD_NO"] = sRSO_NO;
                dr["BOXID"] = sBOXID;

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUTSIDE_RETURN_BOX_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgReturnBoxList3);

                if (BOXList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1547");  //반품 BOX 리스트 조회 항목이 없습니다.
                    return;
                }
                //  dgReturnBoxList2.ItemsSource = DataTableConverter.Convert(BOXList);
                Util.GridSetData(dgReturnBoxList3, BOXList, FrameOperation, true);

                // 반품 LOT 리스트 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("RTN_SALES_ORD_NO", typeof(String));
                RQSTDT1.Columns.Add("BOXID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["RTN_SALES_ORD_NO"] = sRSO_NO;
                dr1["BOXID"] = sBOXID;


                RQSTDT1.Rows.Add(dr1);

                string sBizName = "DA_PRD_SEL_OUTSIDE_RETURN_LOT_HIST_LIST";

                DataTable LOTList = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT1);

                Util.gridClear(dgReturnLotList3);

                if (LOTList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                    return;
                }
                //    dgReturnLotList2.ItemsSource = DataTableConverter.Convert(LOTList);
                Util.GridSetData(dgReturnLotList3, LOTList, FrameOperation, true);

                // 반품 CELL 리스트 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("RTN_SALES_ORD_NO", typeof(String));
                RQSTDT2.Columns.Add("BOXID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["RTN_SALES_ORD_NO"] = sRSO_NO;
                dr2["BOXID"] = sBOXID;


                RQSTDT2.Rows.Add(dr2);

                DataTable CELLList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUTSIDE_RETURN_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                Util.gridClear(dgCellInfo3);

                if (CELLList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1548");  //반품 CELL 리스트 조회 항목이 없습니다.
                    return;
                }
                //   dgCellInfo2.ItemsSource = DataTableConverter.Convert(CELLList);
                Util.GridSetData(dgCellInfo3, CELLList, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchList(txtRCVISSID.Text);

                /*
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    if (cboReturnType.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3640");
                        return;
                    }
                }

                Util.gridClear(dgRetrunCellList);
                Util.gridClear(dgReturnBoxList);
                Util.gridClear(dgReturnLotList);
                Util.gridClear(dgCellInfo);

                sRETURN_TYPE_CODE = string.Empty;

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                    if (Util.NVC(cboReturnType.SelectedValue) == "RMA")
                    {
                        RQSTDT.Columns.Add("IN", typeof(string));
                    }
                    else
                    {
                        RQSTDT.Columns.Add("NOTIN", typeof(string));
                    }
                }

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["RCV_ISS_ID"] = String.IsNullOrEmpty(txtRCVISSID.Text) ? null : txtRCVISSID.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    sRETURN_TYPE_CODE = Util.NVC(cboReturnType.SelectedValue);
                    dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        dr["IN"] = "Y";
                    }
                    else
                    {
                        dr["NOTIN"] = "Y";
                    }
                }
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SearchList(txtRCVISSID.Text);

                    txtRCVISSID.Text = string.Empty;

                    /*
                    if (LoginInfo.CFG_SHOP_ID == "G184")
                    {
                        if (cboReturnType.SelectedValue.Equals("SELECT"))
                        {
                            Util.MessageValidation("SFU3640");
                            return;
                        }
                    }

                    Util.gridClear(dgRetrunCellList);
                    Util.gridClear(dgReturnBoxList);
                    Util.gridClear(dgReturnLotList);
                    Util.gridClear(dgCellInfo);

                    sRETURN_TYPE_CODE = string.Empty;

                    string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                    string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("AREAID", typeof(String));
                    RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                    RQSTDT.Columns.Add("TO_DATE", typeof(String));
                    if (LoginInfo.CFG_SHOP_ID == "G184")
                    {
                        RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                        if (Util.NVC(cboReturnType.SelectedValue) == "RMA")
                        {
                            RQSTDT.Columns.Add("IN", typeof(string));
                        }
                        else
                        {
                            RQSTDT.Columns.Add("NOTIN", typeof(string));
                        }
                    }

                    DataRow dr = RQSTDT.NewRow();
                    dr["AREAID"] = sAREAID;
                    dr["RCV_ISS_ID"] = String.IsNullOrEmpty(txtRCVISSID.Text) ? null : txtRCVISSID.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    if (LoginInfo.CFG_SHOP_ID == "G184")
                    {
                        sRETURN_TYPE_CODE = Util.NVC(cboReturnType.SelectedValue);
                        dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                        if (sRETURN_TYPE_CODE == "RMA")
                        {
                            dr["IN"] = "Y";
                        }
                        else
                        {
                            dr["NOTIN"] = "Y";
                        }
                    }
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                    //dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                    Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                    DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                    DataGridAggregateSum dagsum = new DataGridAggregateSum();
                    dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                    dac.Add(dagsum);
                    DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);

                    txtRCVISSID.Text = string.Empty;
                    */
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }
        private void txtRCVISSID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    SearchList(Clipboard.GetText());

                    /*
                    if (LoginInfo.CFG_SHOP_ID == "G184")
                    {
                        if (cboReturnType.SelectedValue.Equals("SELECT"))
                        {
                            Util.MessageValidation("SFU3640");
                            return;
                        }
                    }

                    Util.gridClear(dgRetrunCellList);
                    Util.gridClear(dgReturnBoxList);
                    Util.gridClear(dgReturnLotList);
                    Util.gridClear(dgCellInfo);

                    sRETURN_TYPE_CODE = string.Empty;

                    string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                    string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                    string sPasteString = Clipboard.GetText();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("AREAID", typeof(String));
                    RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                    RQSTDT.Columns.Add("TO_DATE", typeof(String));
                    if (LoginInfo.CFG_SHOP_ID == "G184")
                    {
                        RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                        if (Util.NVC(cboReturnType.SelectedValue) == "RMA")
                        {
                            RQSTDT.Columns.Add("IN", typeof(string));
                        }
                        else
                        {
                            RQSTDT.Columns.Add("NOTIN", typeof(string));
                        }
                    }

                    DataRow dr = RQSTDT.NewRow();
                    dr["AREAID"] = sAREAID;
                    dr["RCV_ISS_ID"] = String.IsNullOrEmpty(sPasteString) ? null : sPasteString;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    if (LoginInfo.CFG_SHOP_ID == "G184")
                    {
                        sRETURN_TYPE_CODE = Util.NVC(cboReturnType.SelectedValue);
                        dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                        if (sRETURN_TYPE_CODE == "RMA")
                        {
                            dr["IN"] = "Y";
                        }
                        else
                        {
                            dr["NOTIN"] = "Y";
                        }
                    }
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                    //dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                    Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                    DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                    DataGridAggregateSum dagsum = new DataGridAggregateSum();
                    dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                    dac.Add(dagsum);
                    DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);

                    txtRCVISSID.Text = string.Empty;
                    */
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        #endregion

        private void SearchList(string RcvIssId)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    if (cboReturnType.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3640");
                        return;
                    }
                }

                Util.gridClear(dgRetrunCellList);
                Util.gridClear(dgReturnBoxList);
                Util.gridClear(dgReturnLotList);
                Util.gridClear(dgCellInfo);

                sRETURN_TYPE_CODE = string.Empty;

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sPasteString = Clipboard.GetText();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                    if (Util.NVC(cboReturnType.SelectedValue) == "RMA")
                    {
                        RQSTDT.Columns.Add("IN", typeof(string));
                    }
                    else
                    {
                        RQSTDT.Columns.Add("NOTIN", typeof(string));
                    }
                }

                string sBizName = "DA_PRD_SEL_RETURN_CELL_LIST";

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["RCV_ISS_ID"] = String.IsNullOrEmpty(RcvIssId) ? null : RcvIssId;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    sRETURN_TYPE_CODE = Util.NVC(cboReturnType.SelectedValue);
                    dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        dr["IN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_CELL_LIST_RMA";
                    }
                    else
                    {
                        dr["NOTIN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_CELL_LIST";

                        //2022.03.17  정연재 C20220314-000565 사외반품체크 박스 True
                        if (chkOutCellRtn.IsChecked == true)
                        {
                            sBizName = "DA_PRD_SEL_OUT_CERLL_RETURN_PALLET_LIST";
                        }
                    }
                }
                else
                {
                    //2022.03.17  정연재 C20220314-000565 사외반품체크 박스 True
                    if (chkOutCellRtn.IsChecked == true)
                    {
                        sBizName = "DA_PRD_SEL_OUT_CERLL_RETURN_PALLET_LIST";
                    }

                }

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);

                txtRCVISSID.Text = string.Empty;

                if (SearchResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2951");  //조회결과가 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #region Mehod

        //사외반품확정
        private void UpdateRtnCell(string sBOX_ID, string sRSO_NO)
        {

            int iExceptCnt = 0;
            string sExceptMsg = string.Empty;

            //MES 확인 FLAG Update
            try
            {
                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RTN_SALES_ORD_NO", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sBOX_ID;
                dr["RTN_SALES_ORD_NO"] = sRSO_NO;

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_RTNCL_RCV_ISS", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {

                iExceptCnt++;

                if (!sExceptMsg.Equals("") || sExceptMsg != string.Empty)
                    sExceptMsg += Environment.NewLine;

                sExceptMsg += sRSO_NO + " : " + MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]));
            }
            finally
            {
                if (iExceptCnt > 0)
                {
                    Util.Alert(sExceptMsg);
                }
                else
                {
                    btnOutSideConfrim.IsEnabled = false;
                    Util.MessageInfo("SFU1275");
                    //정상 처리 되었습니다.
                }
            }
        }


        #endregion

        private void dgRetrunCellList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //Checked = "dgRetrunCellList_Choice_Checked"

            dgRetrunCellList.Dispatcher.BeginInvoke(new Action(() =>
            {
                //C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    //RadioButton rdo = e.Cell.Presenter.Content as RadioButton;
                    CheckBox rdo = e.Cell.Presenter.Content as CheckBox;
                    if (rdo != null)
                    {
                        rdo.Checked -= dgRetrunCellList_Choice_Checked;
                        rdo.Checked += dgRetrunCellList_Choice_Checked;
                    }
                }
            }));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Init_Form();
                }
            });
        }

        private void Init_Form()
        {
            //dgRetrunCellList.IsEnabled = true;
            bScanflag = false;
            bLoadflag = false;

            Util.gridClear(dgRetrunCellList);
            Util.gridClear(dgReturnBoxList);
            Util.gridClear(dgReturnLotList);
            Util.gridClear(dgCellInfo);

            //SearchCell_ReturnList();

            txtCount1.Text = "Count : 0";
            txtCount2.Text = "Count : 0";

            txtCell_ID.Text = null;
            txtCell_ID.Focus();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    if (cboReturnTypeHist.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3640");
                        return;
                    }
                }

                Util.gridClear(dgRetrunCellList2);
                Util.gridClear(dgReturnBoxList2);
                Util.gridClear(dgReturnLotList2);
                Util.gridClear(dgCellInfo2);

                sRETURN_TYPE_CODE_HIST = string.Empty;

                string sStart_date = dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom2.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateFrom2.SelectedDateTime);
                string sEnd_date = dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo2.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);
                string sLoc = string.Empty;

                if (cboTransLoc.SelectedIndex < 0 || cboTransLoc.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    sLoc = null;
                }
                else
                {
                    sLoc = cboTransLoc.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_SLOC_ID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                    if (Util.NVC(cboReturnTypeHist.SelectedValue) == "RMA")
                    {
                        RQSTDT.Columns.Add("IN", typeof(string));
                    }
                    else
                    {
                        RQSTDT.Columns.Add("NOTIN", typeof(string));
                    }
                }

                string sBizName = "DA_PRD_SEL_RETURN_HIST";

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_AREAID"] = sAREAID2;
                dr["FROM_SLOC_ID"] = sLoc;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    sRETURN_TYPE_CODE_HIST = Util.NVC(cboReturnTypeHist.SelectedValue);
                    dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE_HIST;

                    if (sRETURN_TYPE_CODE_HIST == "RMA")
                    {
                        dr["IN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_HIST_RMA";
                    }
                    else
                    {
                        dr["NOTIN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_HIST";
                    }
                }
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                //  dgRetrunCellList2.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgRetrunCellList2, SearchResult, FrameOperation, true);

                if (SearchResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2951");  //조회결과가 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //사외반품 조회 
        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgRetrunCellList3);
                Util.gridClear(dgReturnBoxList3);
                Util.gridClear(dgReturnLotList3);
                Util.gridClear(dgCellInfo3);

                sRETURN_TYPE_CODE_HIST = string.Empty;


                string sStart_date = dtpDateFrom3.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom2.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateFrom2.SelectedDateTime);
                string sEnd_date = dtpDateTo3.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo2.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);
                string sLoc = string.Empty;

                if (cboTransLoc3.SelectedIndex < 0 || cboTransLoc3.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    sLoc = null;
                }
                else
                {
                    sLoc = cboTransLoc3.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("RTN_SALES_ORD_NO", typeof(String));
                RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                string sBizName = "DA_PRD_SEL_OUTSIDE_RETURN_CELL_LIST";

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["RTN_SALES_ORD_NO"] = '%' + txtRsoNo.Text.Trim() + '%';
                dr["RETURN_TYPE_CODE"] = "";
              
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                
                Util.GridSetData(dgRetrunCellList3, SearchResult, FrameOperation, true);

                if (SearchResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2951");  //조회결과가 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        
        private string sAREAID = string.Empty;
        private string sSHOPID = string.Empty;

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }
        }

        private string sAREAID2 = string.Empty;
        private string sSHOPID2 = string.Empty;

        private void cboArea2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea2.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID2 = "";
                sSHOPID2 = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID2 = sArry[0];
                sSHOPID2 = sArry[1];
            }
        }

        private void btnFileReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRetrunCellList.ItemsSource == null || dgRetrunCellList.Rows.Count == dgRetrunCellList.BottomRows.Count)
                {
                    // 조회 결과가 없습니다.
                    Util.MessageValidation("SFU2816");
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {   //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("SUBLOTID", typeof(string));

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            DataRow dataRow = dataTable.NewRow();
                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                dataRow["SUBLOTID"] = cell.Text;
                            }

                            dataTable.Rows.Add(dataRow);
                        }

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            txtCell_ID.Text = dataTable.Rows[i]["SUBLOTID"].ToString();

                            Scan_CellID();

                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void chkOutCellRtn_Click(object sender, RoutedEventArgs e)
        {
            if(chkOutCellRtn.IsChecked == true)
            {

            }
        }

        private bool ChkUseTopProdID()
        {
            try
            {
                bool bTop_ProdID_Use_Flag = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_TOP_PRODID_USE", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["TOP_PRODID_USE_FLAG"]).Equals("Y"))
                {
                    bTop_ProdID_Use_Flag = true;
                }

                return bTop_ProdID_Use_Flag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}
