/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.20  정용석 : 결재요청 순서도 OUTPUT Parameter 추가로 인한 수정
  2024.02.13  박성진 : E20240206-000751 HOLD 목록 조회시 QA 품질검사결과 등록 화면에서 등록한 LOT만 조회되도록 변경
  2024.02.15  안유수 E20240208-001444 LOTID만 입력 후 조회 시 LOTID로만 조회되도록 수정 및 MES HOLD, QA HOLD 컬럼 추가


 
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
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;

//using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_311 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private string _scrapType = string.Empty;
        private string _pcsgid = string.Empty;
        DateTime dCalDate;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public COM001_311()
        {
            InitializeComponent();

            InitCombo();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            //SetbtnRequestScrapYieldVisibility();

        }

        #region Initialize

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent);
            GetPcsgID();

            if (_pcsgid == "E")
            {
                cboProcess.SelectedValue = "E7000"; //전극창고 대기
                cboProcess.IsEnabled = false;

            }
            else
            {
                cboProcess.IsEnabled = true;
            }
            cboArea.SelectedItemChanged += ClearList;
            cboEquipmentSegment.SelectedItemChanged += ClearList;
            cboProcess.SelectedItemChanged += ClearList;

            string[] sFilter = { "UNHOLD_LOT" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            dCalDate = GetComSelCalDate();
            dtCalDate.SelectedDateTime = dCalDate;
             
        }

        private void ClearList(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgListHold);
        }
        #endregion

        #region Event

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                try
                {
                    ShowLoadingIndicator();
                    
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringLot = "";
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        sPasteStringLot += sPasteStrings[i] + ",";
                    }
                    Multi_Create(sPasteStringLot);
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

                e.Handled = true;
            }
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {

                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;

                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }));

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
            chkAll.IsChecked = false;
        }

        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtGrator.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME_QA_MANA", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("APPR_SEQS", typeof(string));
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);
                        for (int i = 0; i < dtTo.Rows.Count; i++)
                        {
                            dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
                        }


                        dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtGrator.Text = "";
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);

                        dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void dtCalDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DATE"] = dtCalDate.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                dtCalDate.SelectedDateTime = dCalDate;
            }
        }

        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 전극의 경우 NCR 전극은 요청 창에 팝업 처리 [2018-09-10]
                string sMessage = string.Empty;
                if (string.Equals(GetAreaType(), "E"))
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    for (int i = 0; i < dgRequest.Rows.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "JUDG_DATE"))))
                        {
                            builder.Append(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID")));
                            builder.Append("\n");
                        }
                    }

                    if (builder.ToString().Length > 0)
                    {
                        sMessage = ObjectDic.Instance.GetObjectName("QMS불량판정LOT") + "\n";
                        sMessage += builder.ToString();
                    }
                }

                
                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                "SFU2924"), string.IsNullOrEmpty(sMessage) ? null : sMessage, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgRequest.ItemsSource);

                        List<DataRow> drInfo = dtInfo.Select("CHK = 0")?.ToList();
                        foreach (DataRow dr in drInfo)
                        {
                            dtInfo.Rows.Remove(dr);
                        }
                        Util.GridSetData(dgRequest, dtInfo, FrameOperation, true);

                        Request();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("APPR_SEQS", typeof(string));
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgGratorSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            for (int i = 0; i < dtTo.Rows.Count; i++)
            {
                dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
            }


            dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = "";
        }

        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNotice.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME_QA", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);

                        dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtNotice.Text = "";
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelect);

                        dgNoticeSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgRequest_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            Decimal dReqQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REQQTY"));

            if (dReqQty <= 0 || dReqQty > dWipQty)
            {
                Util.AlertInfo("SFU1749");  //요청 수량이 잘못되었습니다.

                DataTableConverter.SetValue(dg.CurrentRow.DataItem, "REQQTY", dWipQty);

                dg.CurrentRow.Refresh();
                return;

            }

            Decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_QTY"));
            Decimal dLanePtnQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_PTN_QTY"));
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "WIPQTY2", dReqQty * dLaneQty * dLanePtnQty);
            dg.CurrentRow.Refresh();
        }
        
        private void dgRequest_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            C1.WPF.DataGrid.DataGridNumericColumn dc = dg.Columns["REQQTY"] as C1.WPF.DataGrid.DataGridNumericColumn;

            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            dc.Maximum = Convert.ToDouble(dWipQty);
            dc.Minimum = 0;

        }

        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgNoticeSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            
            dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

            dgNoticeSelect.Visibility = Visibility.Collapsed;

            txtNotice.Text = "";
        }

        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;

                if (DataTableConverter.GetValue(cb.DataContext, "REQ_ING_CNT").Equals("ING"))//진행중인경우
                {
                    Util.AlertInfo("SFU1693");  //승인 진행 중인 LOT입니다.
                    return;
                }

                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK", typeof(Boolean));
                        dtTo.Columns.Add("EQSGNAME", typeof(string));
                        //dtTo.Columns.Add("EQPTNAME", typeof(string));
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("PRODNAME", typeof(string));
                        dtTo.Columns.Add("MODELID", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(string));
                        dtTo.Columns.Add("REQQTY", typeof(string));
                        dtTo.Columns.Add("WIPQTY2", typeof(string));
                        dtTo.Columns.Add("LANE_QTY", typeof(string));
                        dtTo.Columns.Add("LANE_PTN_QTY", typeof(string));
                        dtTo.Columns.Add("CSTID", typeof(string));
                        dtTo.Columns.Add("JUDG_DATE", typeof(string));
                    }

                    if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length > 0) //중복조건 체크
                    {
                        return;
                    }

                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    dtTo.Rows.Add(dr);
                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else//체크 풀릴때
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);

                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgListHold_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QAHOLD")) == "Y")
                {
                    if (dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null)
                        dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Orange);
                }

                //진행중인 색 변경
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_ING_CNT")) == "ING")
                {
                    if (dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null)
                        dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.LightGray);

                    (dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox).Visibility = Visibility.Hidden;
                }
                else
                {
                    (dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox).Visibility = Visibility.Visible;
                }
            }));
        }

        #endregion

        #region Method

        private void GetPcsgID()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRCESSSEGMENT", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0)
            {
                return;
            }

            _pcsgid = Convert.ToString(result.Rows[0]["PCSGID"]);
        }

        private bool Multi_Create(string sLotid)
        {
            string sQMSHoldLot = string.Empty;

            try
            {
                DoEvents();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3206"); //동을 선택해주세요

                if (sLotid != string.Empty)
                {
                    dr["LOTID"] = sLotid;
                }
                else
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                    if (!Util.GetCondition(txtProd).Equals(""))
                        dr["PRODID"] = Util.GetCondition(txtProd);
                    if (!Util.GetCondition(txtModl).Equals(""))
                        dr["MODLID"] = Util.GetCondition(txtModl);
                    if (!Util.GetCondition(txtPjt).Equals(""))
                        dr["PJT"] = Util.GetCondition(txtPjt);
                    dr["USERID"] = LoginInfo.USERID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK_V03", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHold);
                if (dtRslt.Rows.Count == 0)
                {
                    if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                    {
                        Util.AlertInfo("SFU1816");  //입력한 조건에 HOLD LOT이 존재하지 않습니다.
                    }
                    else
                    {
                        Util.AlertInfo("SFU2023"); //해당 LOT은 현재 HOLD 상태가 아닙니다.
                    }
                }
                else
                {
                    //2021-10-05 김대근
                    //MES홀드는 릴리즈 할 수 없도록 수정
                    string sQAHoldLot = string.Empty;
                    DataTable dtQAHold = dtRslt.Copy();

                    for (int i = 0; i < dtQAHold.Rows.Count; i++)
                    {
                        if (!(dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("NG")
                            //|| dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("ETC")
                            //|| dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("DF")
                            ))
                        {
                            sQAHoldLot = sQAHoldLot + dtQAHold.Rows[i]["LOTID"].ToString() + ",";
                            for (int j = 0; j < dtRslt.Rows.Count; j++)
                            {
                                if (dtRslt.Rows[j]["LOTID"].ToString() == dtQAHold.Rows[i]["LOTID"].ToString())
                                {
                                    dtRslt.Rows[j].Delete();
                                    dtRslt.AcceptChanges();
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(sQAHoldLot))
                    {
                        sQAHoldLot = sQAHoldLot.Substring(0, sQAHoldLot.Length - 1);
                        //Util.AlertInfo("SFU8415", sQAHoldLot); //MES Hold는 이 메뉴에서 해제할 수 없습니다. %1
                        Util.AlertInfo("SFU8577"); //[MES HOLD는 LOT Hold릴리즈/폐기/물품청구 요청 화면을 통해 요청 가능합니다. QMS Hold는 QMS System에서 QA가 해지해 주어야 합니다.]
    }

                    dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                }

                Util.GridSetData(dgListHold, dtRslt, FrameOperation);

                txtLot.Text = "";
                return true;
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        public void GetLotList()
        {
            string sQMSHoldLot = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3206"); //동을 선택해주세요
                if (dr["AREAID"].Equals("")) return;

                if (txtLot.Text != string.Empty)
                {
                    dr["LOTID"] = Util.GetCondition(txtLot);
                }
                else
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                    if (!Util.GetCondition(txtProd).Equals(""))
                        dr["PRODID"] = Util.GetCondition(txtProd);
                    if (!Util.GetCondition(txtModl).Equals(""))
                        dr["MODLID"] = Util.GetCondition(txtModl);
                    if (!Util.GetCondition(txtPjt).Equals(""))
                        dr["PJT"] = Util.GetCondition(txtPjt);
                    dr["USERID"] = LoginInfo.USERID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK_V03", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                    {
                        Util.AlertInfo("SFU1816");  //입력한 조건에 HOLD LOT이 존재하지 않습니다.
                    }
                    else
                    {
                        Util.AlertInfo("SFU2023"); //해당 LOT은 현재 HOLD 상태가 아닙니다.
                    }
                }
                else
                {
                    //2021-10-05 김대근
                    //MES홀드는 릴리즈 할 수 없도록 수정
                    string sQAHoldLot = string.Empty;
                    DataTable dtQAHold = dtRslt.Copy();

                    for (int i = 0; i < dtQAHold.Rows.Count; i++)
                    {
                        if (!(dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("NG")
                            //|| dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("ETC")
                            //|| dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("DF")
                            ))
                        {
                            sQAHoldLot = sQAHoldLot + dtQAHold.Rows[i]["LOTID"].ToString() + ",";
                            for (int j = 0; j < dtRslt.Rows.Count; j++)
                            {
                                if (dtRslt.Rows[j]["LOTID"].ToString() == dtQAHold.Rows[i]["LOTID"].ToString())
                                {
                                    dtRslt.Rows[j].Delete();
                                    dtRslt.AcceptChanges();
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(sQAHoldLot))
                    {
                        sQAHoldLot = sQAHoldLot.Substring(0, sQAHoldLot.Length - 1);
                        //Util.AlertInfo("SFU8415", sQAHoldLot); //MES Hold는 이 메뉴에서 해제할 수 없습니다. %1
                        Util.AlertInfo("SFU8577"); //[MES HOLD는 LOT Hold릴리즈/폐기/물품청구 요청 화면을 통해 요청 가능합니다. QMS Hold는 QMS System에서 QA가 해지해 주어야 합니다.]
                    }

                    dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                }

                Util.GridSetData(dgListHold, dtRslt, FrameOperation);

                txtLot.Text = "";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Request()
        {
            string sTo = "";
            string sCC = "";

            if (dgGrator.Rows.Count == 0)
            {
                Util.Alert("SFU1692");  //승인자가 필요합니다.
                return;
            }
            if (dgRequest.Rows.Count == 0)
            {
                Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                return;
            }
            
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("UNHOLD_CALDATE", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = "LOT_RELEASE_QA";// Util.GetCondition(cboReqType);
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["RESNCODE"] = Util.GetCondition(cboResnCode, "SFU1593"); //사유는필수입니다. >> 사유를 선택하세요.
            row["UNHOLD_CALDATE"] = dtCalDate.SelectedDateTime.ToString("yyyy-MM-dd");
            if (row["RESNCODE"].Equals("")) return;

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));

            for (int i = 0; i < dgRequest.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY"));
                row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY")) *
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_QTY")) *
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_PTN_QTY"));
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID"));
                row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODELID"));
                inLot.Rows.Add(row);
            }

            //승인자
            DataTable inProg = inData.Tables.Add("INPROG");
            inProg.Columns.Add("APPR_SEQS", typeof(string));
            inProg.Columns.Add("APPR_USERID", typeof(string));

            for (int i = 0; i < dgGrator.Rows.Count; i++)
            {
                row = inProg.NewRow();
                row["APPR_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "APPR_SEQS"));
                row["APPR_USERID"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                inProg.Rows.Add(row);

                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            for (int i = 0; i < dgNotice.Rows.Count; i++)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID"));
                inRef.Rows.Add(row);

                sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
            }

            try
            {
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            //요청되었습니다.
                            Util.MessageValidation("SFU1747", (action) =>
                            {
                                MailSend mail = new CMM001.Class.MailSend();
                                string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                                string sTitle = Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString()) + " " + LayoutRoot.Tag;

                                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));

                            });
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }, inData
                );

                //초기화
                dgListHold.ItemsSource = null;
                dgGrator.ItemsSource = null;
                dgRequest.ItemsSource = null;
                dgNotice.ItemsSource = null;
                txtNote.Text = string.Empty;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgListHold.ItemsSource);
            dtSelect = dtTo.Copy();

            dgRequest.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
        }

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
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
        
    }
}
