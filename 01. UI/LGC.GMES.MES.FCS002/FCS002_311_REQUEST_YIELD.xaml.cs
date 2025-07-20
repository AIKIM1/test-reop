/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.18  오화백    : 폴란드, 우크라이나어, 러시아어 숫자형식 관련 수정
  2023.03.15  LEEHJ     : 소형활성화 MES 복사
   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Globalization;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_311_REQUEST_YIELD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private string _scrapType = string.Empty;
        private string _resnGubun = string.Empty;
        private string _area = string.Empty;
        int iRow = 0;
        DataTable dtTemp;

        DateTime dCalDate;

        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public FCS002_311_REQUEST_YIELD()
        {
            InitializeComponent();

            //InitCombo();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            /*
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
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

            //설비
            //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //요청구분
            //string[] sFilter = { "APPR_BIZ_CODE" };
            //_combo.SetCombo(cboReqType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter:sFilter);


            cboArea.SelectedItemChanged += ClearList;
            cboEquipmentSegment.SelectedItemChanged += ClearList;
            cboProcess.SelectedItemChanged += ClearList;
            */
        }

        private void ClearList(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgList);
            Util.gridClear(dgUnAvailableList);
        }
        #endregion

        #region Event
        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //사용자 권한별로 버튼 숨기기
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnSave);
        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //    //여기까지 사용자 권한별로 버튼 숨기기
        //}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo _combo = new CommonCombo();
            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);
                _area = Util.NVC(tmps[2]);

                //this.Header = ObjectDic.Instance.GetObjectName("수율반영폐기");
                this.Header = ObjectDic.Instance.GetObjectName("전공정 LOSS");
                dgRequest.Columns["REQQTY"].IsReadOnly = false;

                //사유
                string[] sFilter = { "SCRAP_LOT_YIELD" };
                C1ComboBox[] cbocboResnCodeChild = { cboCauseProc };
                _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, cbChild: cbocboResnCodeChild, sCase: "YIELDREASON", sFilter: sFilter);

                //원인공정
                C1ComboBox[] cbocbocboCauseProcParent = { cboResnCode };
                _combo.SetCombo(cboCauseProc, CommonCombo.ComboStatus.NONE, cbParent: cbocbocboCauseProcParent, sCase: "YIELDCAUSEPROC", sFilter: sFilter);

                dCalDate = GetComSelCalDate();
                dtCalDate.SelectedDateTime = dCalDate;
            }

            if (!_reqNo.Equals("NEW"))
            {

                if (_reqType.Equals("LOT_RELEASE"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("RELEASE요청취소");


                    string[] sFilter = { "UNHOLD_LOT" };
                    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);


                    dgRequest.Columns["REQQTY"].Visibility = Visibility.Collapsed;
                }
                //else if (_reqType.Equals("LOT_SCRAP_SECTION"))
                //{
                //    this.Header = ObjectDic.Instance.GetObjectName("부분폐기요청취소");

                //    string[] sFilter = { "SCRAP_LOT" };
                //    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
                //}
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("폐기요청취소");

                    string[] sFilter = { "SCRAP_LOT" };
                    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
                }

                SetModify();
                btnReq.Visibility = Visibility.Collapsed;
                btnSearchHold.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;
                grdSearch.Visibility = Visibility.Collapsed;
                txtGrator.Visibility = Visibility.Collapsed;
                txtNotice.Visibility = Visibility.Collapsed;
                dgRequest.Columns["REQQTY"].IsReadOnly = true;
                dgList.Columns["CHK"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["CHK"].Visibility = Visibility.Collapsed;
            }
            else
            {
                btnReqCancel.Visibility = Visibility.Collapsed;
            }

            #region # 전공정 LOSS Workorder Assign (Assign Column Visible)
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals(Area_Type.ASSY))
            {
                dgRequest.Columns["WOID"].Visibility = Visibility.Visible;
                dgRequest.Columns["WO_ASSIGN"].Visibility = Visibility.Visible;
            }
            else
            {
                dgRequest.Columns["WOID"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["WO_ASSIGN"].Visibility = Visibility.Collapsed;
            }
            #endregion

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

        #region [승인자 입력]
        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtGrator.Text.Trim() == string.Empty)
                        return;

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtGrator.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

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

                        if (!ValidationApproval(dtRslt.Rows[0]["USERID"].ToString())) return;

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
        #endregion

        #region [승인자 검색결과 여러개일경우]
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

            if (!ValidationApproval(DataTableConverter.GetValue(rb.DataContext, "USERID").GetString())) return;

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

        #endregion

        #region [참조자 입력]
        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {

                    if (txtNotice.Text.Trim() == string.Empty)
                        return;
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNotice.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

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
        #endregion

        #region [참조자 검색결과 여러개일경우]
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
        #endregion

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        #region [제거 처리]
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



        #endregion

        #region [요청취소]
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReqCancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #region [조회클릭]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
            chkAll.IsChecked = false;
        }
        #endregion

        #region  # 전공정 LOSS Workorder Assign
        private void btnWO_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequest.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            FCS002_311_REQUEST_YIELD_ASSIGN wndPopup = new FCS002_311_REQUEST_YIELD_ASSIGN();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.LANGID;
                Parameters[1] = (DataTableConverter.GetValue(dgRequest.Rows[rowIndex].DataItem, "LOTID") ?? String.Empty).ToString();

                dgRequest.SelectedIndex = rowIndex;
                iRow = rowIndex;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);
                wndPopup.ShowModal();
                wndPopup.CenterOnScreen();
            }

        }
        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_311_REQUEST_YIELD_ASSIGN Window = sender as FCS002_311_REQUEST_YIELD_ASSIGN;
            if (Window.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgRequest.Rows[iRow].DataItem, "WOID", Window.WOID);
            }
        }
        #endregion

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLot.Text.Trim() == string.Empty)
                        return;
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCSTID.Text.Trim() == string.Empty)
                        return;
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [대상 선택하기]
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        //WPF 그지같애
        private void chk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;

                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
                {
                    if (DataTableConverter.GetValue(cb.DataContext, "REQ_ING_CNT").Equals("ING"))//진행중인경우
                    {
                        Util.AlertInfo("SFU1693");  //승인 진행 중인 LOT입니다.

                        cb.IsChecked = false;
                        chkAllClear();
                        return;
                    }

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
                        dtTo.Columns.Add("WIPQTY", typeof(decimal));
                        dtTo.Columns.Add("REQQTY", typeof(decimal));
                        dtTo.Columns.Add("WIPQTY2", typeof(decimal));
                        dtTo.Columns.Add("LANE_QTY", typeof(decimal));
                        dtTo.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                        dtTo.Columns.Add("CSTID", typeof(string));
                        dtTo.Columns.Add("WOID", typeof(string));
                        dtTo.Columns.Add("WIPSTAT", typeof(string));
                        dtTo.Columns.Add("WIPSTAT_NAME", typeof(string));
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

                    if (dtTo.Rows.Count == 0 || dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length == 0)
                    {
                        return;
                    }

                    dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);
                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [요청클릭]
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Request();
                            }
                        });


            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [진행중인 lot 색 변경]
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //진행중인 색 변경  
                if (dtTemp.Rows[e.Cell.Row.Index]["REQ_ING_CNT"].ToString() == "ING")
                {
                    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.LightGray);

                    CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
                    cb.Visibility = Visibility.Hidden;
                }
            }));
        }
        #endregion
        #endregion

        #region Mehod

        #region [작업대상 가져오기]
        public void GetLotList( string lotLists = "", string CstList = "")
        {

            try
            {
                if (string.IsNullOrEmpty(lotLists) && string.IsNullOrEmpty(CstList) && string.IsNullOrEmpty(txtLot.Text) && string.IsNullOrEmpty(txtCSTID.Text))
                {
                    Util.MessageValidation("SFU4917"); //LOTID 또는 SKIDID를 입력하세요
                    return;
                }

                Util.gridClear(dgUnAvailableList);

                const string bizRuleName = "DA_PRD_SEL_HOLD_LOT_LIST";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));

                if (!string.Equals(LoginInfo.CFG_SHOP_ID, "G481") && !string.Equals(LoginInfo.CFG_SHOP_ID, "G482"))
                    inTable.Columns.Add("WIPHOLD", typeof(string));

                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("SKIDID", typeof(string));
                inTable.Columns.Add("PJT", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(lotLists))
                    dr["LOTID"] = lotLists;
                if (!Util.GetCondition(txtLot).Equals(""))
                    dr["LOTID"] = Util.GetCondition(txtLot);
                if (!string.IsNullOrEmpty(CstList))
                    dr["SKIDID"] = CstList;
                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);

                if (!string.Equals(LoginInfo.CFG_SHOP_ID, "G481") && !string.Equals(LoginInfo.CFG_SHOP_ID, "G482"))
                    dr["WIPHOLD"] = "N";

                dr["WIPSTAT"] = "WAIT";

                inTable.Rows.Add(dr);


                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (dtRslt.Rows.Count == 0)
                {

                    if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                    {
                        Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                    }
                }
                else
                {
                    dtTemp = dtRslt; //임시
                    dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
                txtLot.Text = "";
                txtCSTID.Text = "";

                Util.GridSetData(dgList, dtRslt, FrameOperation);

                //요청중인 row 선택 안 되도록 함.
                setIngDisable();

                //C20210317-000117 입력한 LOT 목록중 요청 불가한 LOT 이 있는지 확인 후 조회
                if (!string.IsNullOrEmpty(lotLists))
                {
                    GetUnAvailableLotList(dtRslt, lotLists);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            /*
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
                dtRqst.Columns.Add("SKIDID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3206"); //동을 선택해주세요
                if (dr["AREAID"].Equals("")) return;
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1255"); //라인을 선택 하세요.
                //if (dr["EQSGID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);

                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU3207"); //공정을 선택해주세요.
                if (dr["PROCID"].Equals("")) return;
                //dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);

                if (!Util.GetCondition(txtLot).Equals("")) 
                    dr["LOTID"] = Util.GetCondition(txtLot);

                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);

                if (!Util.GetCondition(txtProd).Equals(""))
                    dr["PRODID"] = Util.GetCondition(txtProd);

                if (!Util.GetCondition(txtModl).Equals(""))
                    dr["MODLID"] = Util.GetCondition(txtModl);

                if (!Util.GetCondition(txtPjt).Equals(""))
                    dr["PJT"] = Util.GetCondition(txtPjt);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK", "INDATA", "OUTDATA", dtRqst);
                
                if (dtRslt.Rows.Count == 0)
                {

                    if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                    {
                        Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                    }                   
                }
                else
                {
                    dtTemp = dtRslt; //임시

                    dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
                //Util.gridClear(dgList);

                //Util.ChkSearchResult(dtRslt);
                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
                txtLot.Text = "";
                txtCSTID.Text = "";

                Util.GridSetData(dgList, dtRslt, FrameOperation);

                //요청중인 row 선택 안 되도록 함.
                setIngDisable();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            */
        }
        #endregion

        private void GetUnAvailableLotList(DataTable pDtRslt, string plotLists) //C20210317-000117
        {
            try
            {
                string[] lotList = plotLists.Split(',');
                string lotLists = string.Empty;

                //요청가능으로 조회된 목록에 있는지 확인. 없으면 요청불가 LOT 임
                for (int inx = 0; inx < lotList.Length; inx++)
                {
                    int iCnt = Convert.ToInt16(pDtRslt.Compute("Count(LOTID)", "LOTID='" + lotList[inx] + "'"));

                    if (iCnt <= 0 && inx == 0)
                    {
                        lotLists = lotList[inx];
                    }
                    else if (iCnt <= 0 && inx != 0)
                    {
                        lotLists = lotLists + "," + lotList[inx];
                    }
                }

                //요청불가 LOT 이 있을경우 요청불가 LOT 의 정보 조회
                if (!string.IsNullOrEmpty(lotLists))
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = lotLists;
                    inTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_ALL_STAT", "RQSTDT", "RSLTDT", inTable);

                    //요청불가 목록에 있는지 확인. 여기도 없으면 그냥 LOTID 만 넣어줌.
                    string[] lotList2 = lotLists.Split(',');

                    for (int jnx = 0; jnx < lotList2.Length; jnx++)
                    {
                        int iCnt2 = Convert.ToInt16(dtRslt.Compute("Count(LOTID)", "LOTID='" + lotList2[jnx] + "'"));

                        if (iCnt2 <= 0)
                        {
                            DataRow drRslt = dtRslt.NewRow();
                            drRslt["LOTID"] = lotList2[jnx];
                            dtRslt.Rows.Add(drRslt);
                        }
                    }

                    if (dtRslt.Rows.Count == 0)
                    {
                    }
                    else
                    {
                        Util.GridSetData(dgUnAvailableList, dtRslt, FrameOperation);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [수정시 조회]
        public void SetModify()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

                Util.gridClear(dgRequest);
                dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);

                Util.gridClear(dgGrator);
                dgGrator.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTPROG"]);

                Util.gridClear(dgNotice);
                dgNotice.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTREF"]);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();

                cboResnCode.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString();
                cboResnCode.IsEditable = false;
                cboResnCode.IsHitTestVisible = false;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        private void Request()
        {
            string sTo = "";
            string sCC = "";

            if (dgRequest.GetRowCount() == 0)
            {
                Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                return;
            }

            #region # # 전공정 LOSS Workorder Assign 
            for (int i = 0; i < dgRequest.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPSTAT")).Equals(Wip_State.END) ||
                    Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPSTAT")).Equals(Wip_State.TERM))
                {
                    Util.Alert("SFU8147");
                    return;
                }
            }
            #endregion

            if (dgGrator.GetRowCount() == 0)
            {
                Util.Alert("SFU1692");  //승인자가 필요합니다.
                return;
            }

            if (Util.GetCondition(cboResnCode, "SFU1593") == "") //사유는필수입니다. >> 사유를 선택하세요.
            {
                return;
            }

            //if (txtCauseProd.Text.Length == 0)
            //{
            //    Util.Alert("SFU4938");  //원인제품이 필요합니다.
            //    return;
            //}

            //if (Util.GetCondition(cboCauseEqsg, "SFU4939") == "") //원인 라인은 필수입니다. >> 원인 라인을 선택하세요
            //{
            //    return;
            //}

            string resnCode = "P" + Util.GetCondition(cboResnCode) + Util.GetCondition(cboCauseProc);


            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("CAUSE_EQSGID", typeof(string));
            inDataTable.Columns.Add("CAUSE_PRODID", typeof(string));
            inDataTable.Columns.Add("UNHOLD_CALDATE", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = _reqType;// Util.GetCondition(cboReqType);
            row["USERID"] = LoginInfo.USERID;
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            row["RESNCODE"] = resnCode;
            if (row["RESNCODE"].Equals("")) return;
            row["AREAID"] = _area; // LoginInfo.CFG_AREA_ID; //Modified By Jaeyoung Ko(2019.07.08) [CSR ID:4032652]

            string sCauseProc = cboCauseProc.SelectedValue.ToString();
            if ("00000".Equals(sCauseProc))
            {
                //처리없음
            }else
            {
                row["CAUSE_EQSGID"] = Util.GetCondition(cboCauseEqsg);
                row["CAUSE_PRODID"] = Util.GetCondition(txtCauseProd);
            }            
            //C20190530_07166 : [CSR ID:4007166] GMES 전기일입력 / 전공정 Loss 사유 추가 및 구분자 변경 요청의 건
            //UNHOLD_CALDATE 는 LOT Release 시 전기일을 지정하는 컬럼이나 전공정 Loss 에서도 전기일을 지정하게 해달라는 요청이 발생해
            //기존에 사용하는 UNHOLD_CALDATE 사용함.
            row["UNHOLD_CALDATE"] = dtCalDate.SelectedDateTime.ToString("yyyy-MM-dd");

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            inLot.Columns.Add("WOID", typeof(string));

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
                row["WOID"] = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WOID"))) ? null : Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WOID"));
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
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA", inData);
                if (dsRslt.Tables[0].Rows.Count > 0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(dsRslt.Tables[0].Rows[0]["REQ_NO"].ToString(), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    //GetLotList();
                    //Util.gridClear(dgRequest);
                    //Util.gridClear(dgGrator);
                    //Util.gridClear(dgNotice);
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }
                Util.AlertInfo("SFU1747");  //요청되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
                //Util.AlertInfo("WIP 관리비즈룰 필요");
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);                
                //Util.AlertInfo(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void ReqCancel()
        {
            string sTo = "";
            string sCC = "";

            //현재상태 체크
            DataTable dtRqstStatus = new DataTable();
            dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

            DataRow drStatus = dtRqstStatus.NewRow();
            drStatus["REQ_NO"] = _reqNo;


            dtRqstStatus.Rows.Add(drStatus);

            DataTable dtRsltStatus = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqstStatus);

            if (!dtRsltStatus.Rows[0]["REQ_RSLT_CODE"].Equals("REQ"))
            {
                Util.AlertInfo("SFU1691");  //승인이 진행 중입니다.
            }
            else
            {
                //여기까지 현재상태 체크

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = _reqNo;
                dr["USERID"] = LoginInfo.USERID;
                dr["REQ_RSLT_CODE"] = "DEL";


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dgGrator.Rows.Count; i++)
                {
                    if (i == 0)//최초 승인자만 메일 가도록
                    {
                        sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                    }
                }

                //참조자

                for (int i = 0; i < dgNotice.Rows.Count; i++)
                {
                    sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
                }


                MailSend mail = new CMM001.Class.MailSend();
                string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
                string sTitle = _reqNo + " " + this.Header;

                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));

                Util.AlertInfo("SFU1937");  //취소되었습니다.
            }
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void setIngDisable()
        {
            try
            {
                if (dgList.GetRowCount() > 0)
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                    {
                        DataRowView drv = row.DataItem as DataRowView;

                        C1.WPF.DataGrid.DataGridCellPresenter cp = dgList.GetCell(row.Index, 0).Presenter;

                        C1.WPF.DataGrid.DataGridCell cell = dgList.GetCell(row.Index, 0);

                        if (cp != null)
                        {
                            if (drv["REQ_ING_CNT"].ToString() == "ING")
                            {
                                CheckBox cb = cp.Content as CheckBox;
                                cb.Visibility = Visibility.Hidden;

                                cp.Background = new SolidColorBrush(Colors.LightGray);

                                //dgList.GetCell(row.Index, 0).Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            }
                        }



                        //if (dgList.GetCell(row.Index, 0).Presenter != null)
                        //    dgList.GetCell(row.Index, 0).Presenter.Background = new SolidColorBrush(Colors.LightGray);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dgList != null && dgList.GetRowCount() > 0)
            {
                LGC.GMES.MES.FCS002.FCS002_311_REQUEST_MODIFY wndPopup = new LGC.GMES.MES.FCS002.FCS002_311_REQUEST_MODIFY();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    DataTable dt = ((DataView)dgList.ItemsSource).Table.DefaultView.ToTable(false, new string[] { "CHK", "LOTID", "HOLD_NOTE", "PJT", "WIPQTY2", "UNHOLD_SCHDDATE", "UNHOLD_USERNAME", "UNHOLD_USERID", "WIPSEQ" });

                    object[] Parameters = new object[1];
                    Parameters[0] = dt;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(OnCloseRequestConfirm);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            else
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return;
            }
        }

        private void OnCloseRequestConfirm(object sender, EventArgs e)
        {
            this.BringToFront();
            LGC.GMES.MES.FCS002.FCS002_311_REQUEST_MODIFY window = sender as LGC.GMES.MES.FCS002.FCS002_311_REQUEST_MODIFY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU3532");    //저장 되었습니다.
                GetLotList();
            }
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        //조용수 추가
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
        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
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
            }));
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgList.ItemsSource);
            dtSelect = dtTo.Copy();

            dgRequest.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
        }

        private void cboCauseProc_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sResnCode = cboResnCode.SelectedValue.ToString();
                if ( string.IsNullOrEmpty(sResnCode) || "SELECT".Equals(sResnCode) )
                {
               
                    return;
                }

                string sCauseProc = cboCauseProc.SelectedValue.ToString();
                if (string.IsNullOrEmpty(sCauseProc) || "00000".Equals(sCauseProc))
                {
                    btnCauseProd.IsEnabled = false;
                    txtCauseProd.Text = string.Empty;

                    cboCauseShop.IsEnabled = false;
                    cboCauseArea.IsEnabled = false;
                    cboCauseEqsg.IsEnabled = false;
                    
                    return;
                }else
                {
                    btnCauseProd.IsEnabled = true;

                    cboCauseShop.IsEnabled = true;
                    cboCauseArea.IsEnabled = true;
                    cboCauseEqsg.IsEnabled = true;
                }

                _resnGubun = sCauseProc.Substring(0, 1);
                BindShop(_resnGubun);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void cboCauseShop_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sShop = cboCauseShop.SelectedValue.ToString();

                BindArea(sShop);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboCauseArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboCauseArea != null && cboCauseArea.SelectedValue != null) {
                    string sArea = cboCauseArea.SelectedValue.ToString();
                    BindEqsg(sArea);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BindShop(string sShop_gubun)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREA_TYPE_CODE"] = sShop_gubun;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_BY_AREATYPE_CBO", "INDATA", "OUTDATA", RQSTDT);

                cboCauseShop.DisplayMemberPath = "CBO_NAME";
                cboCauseShop.SelectedValuePath = "CBO_CODE";
                cboCauseShop.ItemsSource = DataTableConverter.Convert(dtRslt);
                cboCauseShop.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BindArea(string sShop)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sShop;
                dr["AREA_TYPE_CODE"] = _resnGubun;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboCauseArea.DisplayMemberPath = "CBO_NAME";
                cboCauseArea.SelectedValuePath = "CBO_CODE";
                cboCauseArea.ItemsSource = DataTableConverter.Convert(dtResult);
                cboCauseArea.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BindEqsg(string sArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboCauseEqsg.DisplayMemberPath = "CBO_NAME";
                cboCauseEqsg.SelectedValuePath = "CBO_CODE";
                cboCauseEqsg.ItemsSource = DataTableConverter.Convert(dtResult);
                cboCauseEqsg.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnCauseProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRequest.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU4940"); // 요청내용이 없습니다.
                    return;
                }

                string sProdid = string.Empty;

                for (int i = 0; i < dgRequest.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "CHK").ToString() == "True" ||
                        DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "CHK").ToString() == "1")
                    {
                        sProdid = DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID").ToString();
                    }
                }

                FCS002_311_PRODTREE wndPopup = new FCS002_311_PRODTREE();
                wndPopup.FrameOperation = this.FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = LoginInfo.CFG_SHOP_ID;
                    Parameters[1] = sProdid;

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(popup_PRODTREE_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                    wndPopup.BringToFront();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void popup_PRODTREE_Closed(object sender, EventArgs e)
        {
            try
            {
                FCS002_311_PRODTREE popup = sender as FCS002_311_PRODTREE;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtCauseProd.Text = popup.PRODID;
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
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

        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }
                         System.Windows.Forms.Application.DoEvents();
                    }
                    GetLotList(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string CstList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            CstList = sPasteStrings[i];
                        }
                        else
                        {
                            CstList = CstList + "," + sPasteStrings[i];
                        }
                       System.Windows.Forms.Application.DoEvents();
                    }
                    GetLotList(string.Empty, CstList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtCSTID.Text = string.Empty;
            txtLot.Text = string.Empty;
            txtGrator.Text = string.Empty;
            txtNotice.Text = string.Empty;
            txtNote.Text = string.Empty;

            cboResnCode.SelectedIndex = 1;
            cboCauseProc.SelectedIndex = 0;
          
            chkAll.IsChecked = false;
            Util.gridClear(dgList);
            Util.gridClear(dgRequest);
            Util.gridClear(dgGrator);
            Util.gridClear(dgNotice);
            Util.gridClear(dgUnAvailableList);
        }

        private bool ValidationApproval(string approverId)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTHORITYMENU_BY_ID";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("MENUID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["MENUID"] = "SFU010120160";
            dr["USERID"] = approverId;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ACCESS_COUNT"].GetDecimal() > 0)
                {
                    return true;
                }
                else
                {
                    Util.MessageValidation("SUF4969");  //승인권한이 없는 사용자 입니다.
                    return false;
                }
            }
            else
            {
                Util.MessageValidation("SUF4969");
                return false;
            }
        }
    }
}
