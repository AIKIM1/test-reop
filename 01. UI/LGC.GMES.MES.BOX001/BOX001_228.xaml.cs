/*************************************************************************************
 Created Date : 2023.10.04
      Creator : 이병윤
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.04  이병윤 : Initial Created.





 
**************************************************************************************/

using C1.WPF.DataGrid.Summaries;
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
using System.Windows.Threading;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_228 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        string seleted_RCV_ISS_ID = string.Empty;
        string seleted_Prodid = string.Empty;
        RadioButton rb = new RadioButton();
        private string sEmpty_Lot = string.Empty;
        private DataTable isCreateTable = new DataTable();

        #region Declaration & Constructor 
        public BOX001_228()
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
            InitCombo();
            InitControl();
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "cboArea");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            dtpDateFrom2.SelectedDateTime = DateTime.Now;
            dtpDateTo2.SelectedDateTime = DateTime.Now;
        }

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;
        }

        #region 날짜 변경시 이벤트
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

        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region 조회시 이벤트
        /// <summary>
        /// [전극반품]탭에서 조회버튼 클릭시 발행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// [전극반품]탭에서 조회버튼 클릭시 발행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();

            rb = null;
        }
        #endregion

        /// <summary>
        /// [전극반품]탭에서 왼쪽그리드 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkResult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                dgResult.SelectedIndex = idx;
                Util.gridClear(dgLotInfo);

                seleted_RCV_ISS_ID = Util.NVC(dgResult.GetCell(idx, dgResult.Columns["RCV_ISS_ID"].Index).Text);
                seleted_Prodid = Util.NVC(dgResult.GetCell(idx, dgResult.Columns["PRODID"].Index).Text);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = seleted_RCV_ISS_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OWMS_RETURN_LOT_INFO_NJ", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLotInfo, dtResult, FrameOperation);

                if (dtResult.Rows.Count == 0) //pancake ID 직접 입력 가능하도록 열어줌.
                {
                    tbPanCakeID.Visibility = Visibility.Visible;
                    txtPanCakeID.Visibility = Visibility.Visible;
                    txtPanCakeID.Text = "";
                    dgLotInfo.IsReadOnly = false;
                }
                else
                {
                    tbPanCakeID.Visibility = Visibility.Hidden;
                    txtPanCakeID.Visibility = Visibility.Hidden;

                    dgLotInfo.IsReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        /// <summary>
        /// [전극반품]탭에서 반품 확정시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (rb == null)
            {
                Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                return;
            }

            int idx = dgResult.SelectedIndex;// ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

            if (dgLotInfo.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                return;
            }

            int sum = 0;

            foreach (C1.WPF.DataGrid.DataGridRow dr in dgLotInfo.Rows)
            {
                DataRowView drv = dr.DataItem as DataRowView;

                if (drv != null)
                {
                    sum += Convert.ToInt32(drv["WIPQTY2"]);
                }
            }

            if (Convert.ToInt32(DataTableConverter.GetValue(dgResult.Rows[idx].DataItem, "ISS_QTY2")) != sum)
            {
                Util.Alert("SFU1555");//반품수량과 LOT 수량이 일치하지 않습니다.
                return;
            }

            //반품확정 하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2869"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU2869", (result) =>
            {
                if (result == MessageBoxResult.OK)
                    Save();
            }
            );
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom2.SelectedDateTime.ToShortDateString();
                dr["TO_DATE"] = dtpDateTo2.SelectedDateTime.ToShortDateString();
                dr["AREAID"] = cboArea2.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OWMS_RETURN_LOT_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                Util.gridClear(dgLotInfo2);
                Util.GridSetData(dgRetrunList, dtResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void dgRetrunList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                dgRetrunList.SelectedIndex = idx;
                Util.gridClear(dgLotInfo);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = Util.NVC(dgRetrunList.GetCell(idx, dgRetrunList.Columns["RCV_ISS_ID"].Index).Text);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OWMS_RETURN_LOT_INFO_NJ", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLotInfo2, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void txtPanCakeID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (Util.GetCondition(txtPanCakeID).Trim().Length == 0)
                    {
                        txtPanCakeID.Text = "";
                        return;
                    }

                    //real version 일때 살려줌.
                    DataTable dt = getReturnLot_Info(Util.GetCondition(txtPanCakeID));

                    if (dt.Rows.Count == 0)
                    {
                        //Util.Alert("SFU4256"); //입력하신 pancakeid는 존재하지 않습니다.
                        return;
                    }

                    if (seleted_Prodid != dt.Rows[0]["PRODID"].ToString())
                    {
                        Util.Alert("SFU1480"); //다른 제품을 선택하셨습니다.
                        return;
                    }

                    DataTable dtLotInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                    if (dtLotInfo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtLotInfo.Columns.Add("BOXID", typeof(string));
                        dtLotInfo.Columns.Add("LOTID", typeof(string));
                        dtLotInfo.Columns.Add("PRODID", typeof(string));
                        dtLotInfo.Columns.Add("WIPQTY2", typeof(string));
                        dtLotInfo.Columns.Add("PRODDESC", typeof(string));
                    }

                    if (dtLotInfo.Select("LOTID = '" + Util.GetCondition(txtPanCakeID) + "'").Length > 0) //중복조건 체크
                    {
                        return;
                    }

                    DataRow dr = dtLotInfo.NewRow();
                    foreach (DataColumn dc in dtLotInfo.Columns)
                    {
                        dr[dc.ColumnName] = dt.Rows[0][dc.ColumnName].ToString();
                    }

                    dtLotInfo.Rows.Add(dr);
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtLotInfo);

                    DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                    DataGridAggregateSum dagsum = new DataGridAggregateSum();
                    dagsum.ResultTemplate = dgLotInfo.Resources["ResultTemplate"] as DataTemplate;
                    dac.Add(dagsum);
                    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["WIPQTY2"], dac);

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPanCakeID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                            continue;

                        DataTable dt = getReturnLot_Info(sPasteStrings[i]);

                        if (dt.Rows.Count == 0)
                        {
                            //Util.Alert("SFU4256"); //입력하신 pancakeid는 존재하지 않습니다.
                            return;
                        }

                        if (seleted_Prodid != dt.Rows[0]["PRODID"].ToString())
                        {
                            Util.Alert("SFU1480"); //다른 제품을 선택하셨습니다.
                            return;
                        }

                        DataTable dtLotInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                        if (dtLotInfo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtLotInfo.Columns.Add("BOXID", typeof(string));
                            dtLotInfo.Columns.Add("LOTID", typeof(string));
                            dtLotInfo.Columns.Add("PRODID", typeof(string));
                            dtLotInfo.Columns.Add("WIPQTY2", typeof(string));
                            dtLotInfo.Columns.Add("PRODDESC", typeof(string));
                        }

                        if (dtLotInfo.Select("LOTID = '" + sPasteStrings[i] + "'").Length <= 0) //중복조건 체크
                        {
                            DataRow dr = dtLotInfo.NewRow();
                            foreach (DataColumn dc in dtLotInfo.Columns)
                            {
                                dr[dc.ColumnName] = dt.Rows[0][dc.ColumnName].ToString();
                            }

                            dtLotInfo.Rows.Add(dr);
                            dgLotInfo.ItemsSource = DataTableConverter.Convert(dtLotInfo);

                            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                            DataGridAggregateSum dagsum = new DataGridAggregateSum();
                            dagsum.ResultTemplate = dgLotInfo.Resources["ResultTemplate"] as DataTemplate;
                            dac.Add(dagsum);
                            DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["WIPQTY2"], dac);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }
        }

        #endregion

        #region Mehod
        private void Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["AREAID"] = cboArea.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OWMS_RETURN_LOT_LIST", "RQSTDT", "RSLTDT", RQSTDT);
                Util.gridClear(dgLotInfo);
                Util.GridSetData(dgResult, dtResult, FrameOperation);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgResult.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgResult.Columns["LOTQTY"], dac);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Save()
        {
            C1.WPF.DataGrid.DataGridRow drInfo = dgResult.Rows[dgResult.SelectedIndex];
            DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            string sISSQTY = Util.NVC(DataTableConverter.GetValue(drInfo.DataItem, "ISS_QTY"));
            string sISSQTY2 = Util.NVC(DataTableConverter.GetValue(drInfo.DataItem, "ISS_QTY2"));

            DataSet indataSet = new DataSet();
            DataTable inTable = indataSet.Tables.Add("INDATA");
            inTable.Columns.Add("SRCTYPE", typeof(String));
            inTable.Columns.Add("RCV_ISS_ID", typeof(String));
            inTable.Columns.Add("AREAID", typeof(String));
            inTable.Columns.Add("RCV_QTY", typeof(decimal));
            inTable.Columns.Add("RCV_QTY2", typeof(decimal));
            inTable.Columns.Add("USERID", typeof(String));

            DataTable pTable = indataSet.Tables.Add("INPALLET");
            pTable.Columns.Add("BOXID", typeof(String));

            DataTable bTable = indataSet.Tables.Add("INBOX");
            bTable.Columns.Add("BOXID", typeof(String));
            bTable.Columns.Add("LOTID", typeof(String));
            bTable.Columns.Add("RCV_QTY", typeof(String));
            bTable.Columns.Add("RCV_QTY2", typeof(String));

            DataRow iRow = inTable.NewRow();
            iRow["SRCTYPE"] = "UI";
            iRow["RCV_ISS_ID"] = DataTableConverter.GetValue(drInfo.DataItem, "RCV_ISS_ID");
            iRow["AREAID"] = cboArea.SelectedValue;
            iRow["RCV_QTY"] = string.IsNullOrWhiteSpace(sISSQTY) ? 0 : DataTableConverter.GetValue(drInfo.DataItem, "ISS_QTY");
            iRow["RCV_QTY2"] = string.IsNullOrWhiteSpace(sISSQTY2) ? 0 : DataTableConverter.GetValue(drInfo.DataItem, "ISS_QTY2");
            iRow["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(iRow);


            foreach (string dr in dtInfo.AsEnumerable().Select(c => c.Field<string>("BOXID")).Distinct().ToList())
            {
                DataRow pRow = pTable.NewRow();
                pRow["BOXID"] = dr;
                pTable.Rows.Add(pRow);
            }

            foreach (DataRow row in dtInfo.Rows)
            {
                DataRow bRow = bTable.NewRow();
                bRow["BOXID"] = row["BOXID"];
                bRow["LOTID"] = row["LOTID"];
                bRow["RCV_QTY"] = row["WIPQTY2"];
                bRow["RCV_QTY2"] = row["WIPQTY2"];
                bTable.Rows.Add(bRow);
            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CONFIRM_RETURN_PRODUCT_ELEC", "INDATA,INPALLET,INBOX", null, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        //Util.AlertByBiz("BR_PRD_REG_CONFIRM_RETURN_PRODUCT_ELEC", searchException.Message, searchException.ToString());
                        Util.MessageException(searchException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                    Search();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }, indataSet
            );
        }

        bool Multi_Create(string sLotid)
        {
            try
            {
                DoEvents();

                string sWipstat = "TERM";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    if (sEmpty_Lot == "")
                        sEmpty_Lot += sLotid;
                    else
                        sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                }

                if (dgLotInfo.GetRowCount() == 0)
                {
                    Util.GridSetData(dgLotInfo, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgLotInfo, dtInfo, FrameOperation);
                }

                isCreateTable = DataTableConverter.Convert(dgLotInfo.GetCurrentItems());

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

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

        private DataTable getReturnLot_Info(string sLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = seleted_RCV_ISS_ID;
                dr["LOTID"] = sLotID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RETURN_LOT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
