/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.15  LEEHJ     : 소형활성화 MES 복사

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;


namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_311_REQUEST : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private string _scrapType = string.Empty;
        private string _areaTypeCode = string.Empty;

        private bool _bIsWorkDate = false;

        private bool _bIsOcap = false;

        DateTime dCalDate;

        public FCS002_311_REQUEST()
        {
            InitializeComponent();

            InitCombo();

            GetAreaTypeCode();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
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
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent);

            //설비
            //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //요청구분
            //string[] sFilter = { "APPR_BIZ_CODE" };
            //_combo.SetCombo(cboReqType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter:sFilter);


            cboArea.SelectedItemChanged += ClearList;
            cboEquipmentSegment.SelectedItemChanged += ClearList;
            cboProcess.SelectedItemChanged += ClearList;
        }

        private void ClearList(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgListHold);
        }
        #endregion

        #region [Event]
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
            if (_bIsWorkDate)
                dtWorkDate.IsEnabled = true;
            else
                dtWorkDate.IsEnabled = false;

            txtSelCnt.Text = "0";

            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo _combo = new CommonCombo();
            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);

                if (_reqType.Equals("LOT_RELEASE"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("RELEASE승인요청");

                    string[] sFilter = { "UNHOLD_LOT" };
                    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

                    dgRequest.Columns["REQQTY"].Visibility = Visibility.Collapsed;

                    // 전극 추가 요청 사항 반영 [2018-02-19]
                    if (string.Equals(GetAreaType(), "E"))
                    {
                        dgListHold.Columns["COATING_DT"].Visibility = Visibility.Visible;
                        dgListHold.Columns["VLD_DATE"].Visibility = Visibility.Visible;
                        dgListHold.Columns["NOTE"].Visibility = Visibility.Visible;
                    }

                }
                //else if(_reqType.Equals("LOT_SCRAP_SECTION"))
                //{
                //    this.Header = ObjectDic.Instance.GetObjectName("부분폐기승인요청");
                //    dgRequest.Columns["WIPQTY"].IsReadOnly = false;

                //    string[] sFilter = { "SCRAP_LOT" };
                //    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
                //}
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("폐기승인요청");
                    dgRequest.Columns["REQQTY"].IsReadOnly = false;

                    string[] sFilter = { "SCRAP_LOT" };
                    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
                }

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
                dgListHold.Columns["CHK"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["CHK"].Visibility = Visibility.Collapsed;
            }
            else
            {
                btnReqCancel.Visibility = Visibility.Collapsed;
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

        #region [승인자 입력]
        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtGrator.Text.Trim()))
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
                        Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
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
                            Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다.
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

                        Util.GridSetData(dgGrator, dtTo, FrameOperation, true);

                        txtGrator.Text = string.Empty;
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);
                        Util.GridSetData(dgGratorSelect, dtRslt, FrameOperation, true);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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
                Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다.
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

            Util.GridSetData(dgGrator, dtTo, FrameOperation, true);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = string.Empty;
        }

        #endregion

        #region [참조자 입력]
        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtNotice.Text.Trim()))
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
                        Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
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
                            Util.MessageValidation("SFU1780");  //이미 추가 된 참조자 입니다.
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

                        Util.GridSetData(dgNotice, dtTo, FrameOperation, true);

                        txtNotice.Text = string.Empty;
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelect);

                        Util.GridSetData(dgNoticeSelect, dtRslt, FrameOperation, true);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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
                Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다.
                dgNoticeSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);

            Util.GridSetData(dgNotice, dtTo, FrameOperation, true);

            dgNoticeSelect.Visibility = Visibility.Collapsed;

            txtNotice.Text = string.Empty;
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

                    Util.GridSetData(dg, dt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                Util.MessageException(ex);
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

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLot.Text.Trim().Equals(string.Empty))
                        return;

                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtCSTID.Text.Trim()))
                        return;

                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [대상 선택하기]
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        //WPF 그지같애
        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

                if (DataTableConverter.GetValue(cb.DataContext, "REQ_ING_CNT").Equals("ING"))//진행중인경우
                {
                    Util.MessageValidation("SFU1693");  //승인 진행 중인 LOT입니다.                    

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Util.NVC(row["REQ_ING_CNT"]).Equals("ING"))
                        {
                            row["CHK"] = false;
                        }
                    }

                    return;
                }

                if (DataTableConverter.GetValue(cb.DataContext, "REL_SYSTEM_ID").Equals("OCAP"))
                {
                    Util.MessageValidation("SUF9013", DataTableConverter.GetValue(cb.DataContext, "LOTID").ToString());   //OCAP 처리 대상은 요청 목록에서 제외됩니다. (LOT ID : %1)

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Util.NVC(row["REL_SYSTEM_ID"]).Equals("OCAP"))
                        {
                            row["CHK"] = false;
                        }
                    }

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
                        dtTo.Columns.Add("WIPQTY", typeof(decimal));
                        dtTo.Columns.Add("REQQTY", typeof(decimal));
                        dtTo.Columns.Add("WIPQTY2", typeof(decimal));
                        dtTo.Columns.Add("LANE_QTY", typeof(decimal));
                        dtTo.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                        dtTo.Columns.Add("CSTID", typeof(string));
                        dtTo.Columns.Add("JUDG_DATE", typeof(string));
                        dtTo.Columns.Add("QMS_INSP_ID", typeof(string));
                        dtTo.Columns.Add("REL_SYSTEM_ID", typeof(string));
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
                    Util.GridSetData(dgRequest, dtTo, FrameOperation, true);
                    txtSelCnt.Text = dtTo.Rows.Count.ToString();
                }
                else//체크 풀릴때
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);
                    
                    DataRow[] drListTo = dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'");
                    for (int i = 0; i < drListTo.Length; i++)
                    {
                        dtTo.Rows.Remove(drListTo[i]);
                    }

                    Util.GridSetData(dgRequest, dtTo, FrameOperation, true);
                    txtSelCnt.Text = dtTo.Rows.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [요청클릭]
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GetTotalInspectionArea()) //전수검사 AREA
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Request();
                        }
                    });
                }
                else
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
                                string ncrNum = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "QMS_INSP_ID"));

                                builder.Append(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID")));
                                builder.Append(" : ");

                                if (!string.IsNullOrWhiteSpace(ncrNum))
                                {
                                    builder.Append(ncrNum);
                                }
                                else
                                {
                                    builder.Append(ObjectDic.Instance.GetObjectName("미검사"));
                                }
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
                                    Request();
                                }
                            });
                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                Util.MessageException(ex);
            }
        }

        private void dgRequest_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            Decimal dReqQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REQQTY"));

            if (dReqQty <= 0 || dReqQty > dWipQty)
            {
                Util.MessageValidation("SFU1749");  //요청 수량이 잘못되었습니다.

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
            if (dgListHold != null && dgListHold.GetRowCount() > 0)
            {
                LGC.GMES.MES.FCS002.FCS002_311_REQUEST_MODIFY wndPopup = new LGC.GMES.MES.FCS002.FCS002_311_REQUEST_MODIFY();
                wndPopup.FrameOperation = FrameOperation;
                if (wndPopup != null)
                {
                    DataTable dt = ((DataView)dgListHold.ItemsSource).Table.DefaultView.ToTable(false, new string[] { "CHK", "LOTID", "HOLD_NOTE", "PJT", "WIPQTY2", "UNHOLD_SCHDDATE", "ACTION_USERID", "ACTION_USERNAME", "WIPSEQ" });

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

        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(e.Column.Name))
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

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            bool bIsOcap = false;
            string sOcapLot = string.Empty;

            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            //dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);

            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["REQ_ING_CNT"]).Equals("ING"))
                {
                    row["CHK"] = false;
                }
                else
                {
                    row["CHK"] = true;
                }
                
                if (Util.NVC(row["REL_SYSTEM_ID"]).Equals("OCAP"))
                {
                    bIsOcap = true;

                    if (string.IsNullOrEmpty(sOcapLot))
                        sOcapLot = Util.NVC(row["LOTID"]);
                    else
                        sOcapLot += "," + Util.NVC(row["LOTID"]);

                    row["CHK"] = false;
                }
                else
                {
                    row["CHK"] = true;
                }
            }
            
            dt.AcceptChanges();

            chkAllSelect();

            if (bIsOcap)
                Util.MessageValidation("SUF9013", sOcapLot);   //OCAP 처리 대상은 요청 목록에서 제외됩니다. (LOT ID : %1)
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);

            dt.AcceptChanges();

            chkAllClear();
        }

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
                    string sPasteStringLot = string.Empty;
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        sPasteStringLot += sPasteStrings[i] + ",";
                    }
                    Multi_Create(sPasteStringLot, string.Empty);
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
        #endregion

        #region [진행중인 lot 색 변경]
        private void dgListHold_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //진행중인 색 변경
                if (e.Cell.Column.Name.Equals("REQ_ING_CNT"))
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_ING_CNT"));
                    if (sCheck.Equals("ING"))
                    {
                        foreach (C1.WPF.DataGrid.DataGridColumn dc in dataGrid.Columns)
                        {
                            if (dc.Visibility == Visibility.Visible)
                            {
                                if (dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter != null)
                                    dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            }
                        }


                        CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
                        cb.Visibility = Visibility.Hidden;
                    }
                }


            }));
        }
        #endregion

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringCst = string.Empty;

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        sPasteStringCst += sPasteStrings[i] + ",";
                    }

                    Multi_Create(string.Empty, sPasteStringCst);
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtCSTID.Text = string.Empty;
            txtLot.Text = string.Empty;
            txtModl.Text = string.Empty;
            txtPjt.Text = string.Empty;
            txtProd.Text = string.Empty;
            txtGrator.Text = string.Empty;
            txtNotice.Text = string.Empty;
            txtNote.Text = string.Empty;

            cboResnCode.SelectedIndex = 0;
            chkAll.IsChecked = false;

            Util.gridClear(dgListHold);
            Util.gridClear(dgRequest);
            Util.gridClear(dgGrator);
            Util.gridClear(dgNotice);

            if (_bIsWorkDate)
                dtWorkDate.IsEnabled = true;
            else
                dtWorkDate.IsEnabled = false;

            txtSelCnt.Text = "0";
        }

        private void chkWorkDate_Checked(object sender, RoutedEventArgs e)
        {
            _bIsWorkDate = true;
            dtWorkDate.IsEnabled = true;
        }

        private void chkWorkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            _bIsWorkDate = false;
            dtWorkDate.IsEnabled = false;
        }

        #endregion

        #region [Mehod]

        #region [작업대상 가져오기]
        public void GetLotList()
        {
            string sQMSHoldLot = string.Empty;
            string sQAHoldLot = string.Empty;

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
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3206"); //동을 선택해주세요
                if (dr["AREAID"].Equals("")) return;

                if(txtLot.Text != string.Empty)
                {
                    dr["LOTID"] = Util.GetCondition(txtLot);
                }
                else if(txtCSTID.Text != string.Empty)
                {
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);
                }
                else
                {
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1255"); //라인을 선택 하세요.
                    //if (dr["EQSGID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    //dr["PROCID"] = Util.GetCondition(cboProcess, "SFU3207"); //공정을 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                    //if (!Util.GetCondition(txtLot).Equals(""))
                    //    dr["LOTID"] = Util.GetCondition(txtLot);
                    //if (!Util.GetCondition(txtCSTID).Equals(""))
                    //    dr["SKIDID"] = Util.GetCondition(txtCSTID);
                    if (!Util.GetCondition(txtProd).Equals(""))
                        dr["PRODID"] = Util.GetCondition(txtProd);
                    if (!Util.GetCondition(txtModl).Equals(""))
                        dr["MODLID"] = Util.GetCondition(txtModl);
                    if (!Util.GetCondition(txtPjt).Equals(""))
                        dr["PJT"] = Util.GetCondition(txtPjt);
                    dr["USERID"] = LoginInfo.USERID;
                }
                if (chkWorkDate.IsChecked == true)
                        dr["WRK_DATE"] = Util.GetCondition(dtWorkDate);

                dtRqst.Rows.Add(dr);

                if (GetTotalInspectionArea()) //전수검사 AREA
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK_V02", "INDATA", "OUTDATA", dtRqst);

                    Util.gridClear(dgListHold);
                    if (dtRslt.Rows.Count == 0)
                    {
                        if (string.IsNullOrEmpty(Util.GetCondition(txtLot))) //lot id 가 없는 경우
                        {
                            Util.MessageValidation("SFU1816");  //입력한 조건에 HOLD LOT이 존재하지 않습니다.
                        }
                        else
                        {
                            Util.MessageValidation("SFU2023"); //해당 LOT은 현재 HOLD 상태가 아닙니다.
                        }
                    }
                    else
                    {
                        DataTable dtQMS = dtRslt.Copy();

                        for (int i = 0; i < dtQMS.Rows.Count; i++)
                        {
                            if (dtQMS.Rows[i]["QMS_HOLD_FLAG"].ToString().Equals("Y"))
                            {
                                sQMSHoldLot = sQMSHoldLot + dtQMS.Rows[i]["LOTID"].ToString() + ",";
                                for (int j = 0; j < dtRslt.Rows.Count; j++)
                                {
                                    if (dtRslt.Rows[j]["LOTID"].ToString().Equals(dtQMS.Rows[i]["LOTID"].ToString()))
                                    {
                                        dtRslt.Rows[j].Delete();
                                        dtRslt.AcceptChanges();
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sQMSHoldLot))
                            Util.MessageValidation("SFU5094", sQMSHoldLot); //QMS Hold는 UI에서 해제할 수 없습니다. %1

                        //2021-11-23 김대근
                        //전극 동일 경우, QA홀드는 릴리즈 할 수 없도록 수정
                        if (_areaTypeCode.Equals("E"))
                        {
                            DataTable dtQAHold = dtRslt.Copy();

                            for (int i = 0; i < dtQAHold.Rows.Count; i++)
                            {
                                if (dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("NG")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("ETC")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("DF"))
                                {
                                    sQAHoldLot = sQAHoldLot + dtQAHold.Rows[i]["LOTID"].ToString() + ",";
                                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                                    {
                                        if (dtRslt.Rows[j]["LOTID"].ToString().Equals(dtQAHold.Rows[i]["LOTID"].ToString()))
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
                                Util.MessageValidation("SFU8414", sQAHoldLot); //QA Hold는 이 메뉴에서 해제할 수 없습니다. %1
                            }
                        }

                        Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                    }

                    Util.GridSetData(dgListHold, dtRslt, FrameOperation);
                }
                else
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK_V01", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                        {
                            Util.MessageValidation("SFU1816");  //입력한 조건에 HOLD LOT이 존재하지 않습니다.
                        }
                        else
                        {
                            Util.MessageValidation("SFU2023"); //해당 LOT은 현재 HOLD 상태가 아닙니다.
                        }
                    }
                    else
                    {
                        //2021-11-23 김대근
                        //전극 동일 경우, QA홀드는 릴리즈 할 수 없도록 수정
                        if (_areaTypeCode.Equals("E"))
                        {
                            DataTable dtQAHold = dtRslt.Copy();

                            for (int i = 0; i < dtQAHold.Rows.Count; i++)
                            {
                                if (dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("NG")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("ETC")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("DF"))
                                {
                                    sQAHoldLot = sQAHoldLot + dtQAHold.Rows[i]["LOTID"].ToString() + ",";
                                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                                    {
                                        if (dtRslt.Rows[j]["LOTID"].ToString().Equals(dtQAHold.Rows[i]["LOTID"].ToString()))
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
                                Util.MessageValidation("SFU8414", sQAHoldLot); //QA Hold는 이 메뉴에서 해제할 수 없습니다. %1
                            }
                        }

                        Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                    }

                    Util.GridSetData(dgListHold, dtRslt, FrameOperation);
                }

                txtLot.Text = string.Empty;
                txtCSTID.Text = string.Empty;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

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
                Util.GridSetData(dgRequest, dsRslt.Tables["OUTLOT"], FrameOperation, true);

                Util.gridClear(dgGrator);
                Util.GridSetData(dgGrator, dsRslt.Tables["OUTPROG"], FrameOperation, true);

                Util.gridClear(dgNotice);
                Util.GridSetData(dgNotice, dsRslt.Tables["OUTREF"], FrameOperation, true);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();

                cboResnCode.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString();
                cboResnCode.IsEditable = false;
                cboResnCode.IsHitTestVisible = false;

                txtSelCnt.Text = dsRslt.Tables["OUTLOT"].Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

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

        private void Request()
        {
            string sTo = string.Empty;
            string sCC = string.Empty;

            if (dgGrator.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1692");  //승인자가 필요합니다.
                return;
            }
            if (dgRequest.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1748");  //요청 목록이 필요합니다.
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
            row["APPR_BIZ_CODE"] = _reqType;// Util.GetCondition(cboReqType);
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


            decimal dREQQTY = 0;
            decimal dLANE_QTY = 0;
            decimal dLANE_PTN_QTY = 0;


            for (int i = 0; i < dgRequest.Rows.Count; i++)
            {

                row = inLot.NewRow();


                dREQQTY = Util.NVC_Decimal(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY")).Replace(",", "."));
                dLANE_QTY = Util.NVC_Decimal(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_QTY")).Replace(",", "."));
                dLANE_PTN_QTY = Util.NVC_Decimal(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_PTN_QTY")).Replace(",", "."));

                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                row["WIPQTY"] = Util.NVC_Decimal(Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY")).Replace(",", "."));
                row["WIPQTY2"] = dREQQTY * dLANE_QTY * dLANE_PTN_QTY;
                /*
                 * row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY")) *
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_QTY")) *
                                    Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_PTN_QTY"));
                */

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
                Util.MessageValidation("SFU1747");  //요청되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
                //Util.MessageValidation("WIP 관리비즈룰 필요");
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                //                Util.MessageValidation(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void ReqCancel()
        {
            string sTo = string.Empty;
            string sCC = string.Empty;

            //현재상태 체크
            DataTable dtRqstStatus = new DataTable();
            dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

            DataRow drStatus = dtRqstStatus.NewRow();
            drStatus["REQ_NO"] = _reqNo;


            dtRqstStatus.Rows.Add(drStatus);

            DataTable dtRsltStatus = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqstStatus);

            if (!dtRsltStatus.Rows[0]["REQ_RSLT_CODE"].Equals("REQ"))
            {
                Util.MessageValidation("SFU1691");  //승인이 진행 중입니다.
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

                Util.MessageValidation("SFU1937");  //취소되었습니다.
            }
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void GetAreaTypeCode()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow drRqst = dtRqst.NewRow();
                drRqst["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dtRqst);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_AREA_TYPE_CODE", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    _areaTypeCode = Util.NVC(dtRslt.Rows[0]["AREA_TYPE_CODE"]);
                }
            }
            catch(Exception ex)
            {
                _areaTypeCode = string.Empty;
            }
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgListHold.ItemsSource);

            dtTo.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

            dtSelect = dtTo.Copy();

            Util.GridSetData(dgRequest, dtSelect, FrameOperation, true);
            txtSelCnt.Text = dtSelect.Rows.Count.ToString();
        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
            txtSelCnt.Text = "0";
        }

        // 동간 구분을 위하여 추가 [2018-02-19]
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
            return string.Empty;
        }

        //추가
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

        bool Multi_Create(string sLotid, string sCstID)
        {
            string sQMSHoldLot = string.Empty;
            string sQAHoldLot = string.Empty;

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
                dtRqst.Columns.Add("SKIDID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3206"); //동을 선택해주세요

                if (sLotid != string.Empty)
                {
                    dr["LOTID"] = sLotid;
                }
                else if (sCstID != string.Empty)
                {
                    dr["SKIDID"] = sCstID;
                }
                else
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                    //if (!Util.GetCondition(txtCSTID).Equals(""))
                    //    dr["SKIDID"] = Util.GetCondition(txtCSTID);
                    if (!Util.GetCondition(txtProd).Equals(""))
                        dr["PRODID"] = Util.GetCondition(txtProd);
                    if (!Util.GetCondition(txtModl).Equals(""))
                        dr["MODLID"] = Util.GetCondition(txtModl);
                    if (!Util.GetCondition(txtPjt).Equals(""))
                        dr["PJT"] = Util.GetCondition(txtPjt);
                    dr["USERID"] = LoginInfo.USERID;
                }

                if (chkWorkDate.IsChecked == true)
                    dr["WRK_DATE"] = Util.GetCondition(dtWorkDate);

                dtRqst.Rows.Add(dr);

                if (GetTotalInspectionArea()) //전수검사 AREA
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK_V02", "INDATA", "OUTDATA", dtRqst);

                    Util.gridClear(dgListHold);
                    if (dtRslt.Rows.Count == 0)
                    {
                        if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                        {
                            Util.MessageValidation("SFU1816");  //입력한 조건에 HOLD LOT이 존재하지 않습니다.
                        }
                        else
                        {
                            Util.MessageValidation("SFU2023"); //해당 LOT은 현재 HOLD 상태가 아닙니다.
                        }
                    }
                    else
                    {
                        DataTable dtQMS = dtRslt.Copy();

                        for (int i = 0; i < dtQMS.Rows.Count; i++)
                        {
                            if (dtQMS.Rows[i]["QMS_HOLD_FLAG"].ToString().Equals("Y"))
                            {
                                sQMSHoldLot = sQMSHoldLot + dtQMS.Rows[i]["LOTID"].ToString() + ",";
                                //원 Table DataRow 삭제
                                for (int j = 0; j < dtRslt.Rows.Count; j++)
                                {
                                    if (dtRslt.Rows[j]["LOTID"].ToString().Equals(dtQMS.Rows[i]["LOTID"].ToString()))
                                    {
                                        dtRslt.Rows[j].Delete();
                                        dtRslt.AcceptChanges();
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sQMSHoldLot))
                            Util.MessageValidation("SFU5094", sQMSHoldLot); //QMS Hold는 UI에서 해제할 수 없습니다. %1

                        //2021-11-23 김대근
                        //전극 동일 경우, QA홀드는 릴리즈 할 수 없도록 수정
                        if (_areaTypeCode.Equals("E"))
                        {
                            DataTable dtQAHold = dtRslt.Copy();

                            for (int i = 0; i < dtQAHold.Rows.Count; i++)
                            {
                                if (dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("NG")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("ETC")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("DF"))
                                {
                                    sQAHoldLot = sQAHoldLot + dtQAHold.Rows[i]["LOTID"].ToString() + ",";
                                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                                    {
                                        if (dtRslt.Rows[j]["LOTID"].ToString().Equals(dtQAHold.Rows[i]["LOTID"].ToString()))
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
                                Util.MessageValidation("SFU8414", sQAHoldLot); //QA Hold는 이 메뉴에서 해제할 수 없습니다. %1
                            }
                        }

                        Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                    }

                    Util.GridSetData(dgListHold, dtRslt, FrameOperation);
                }
                else
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_APP_CHK_V01", "INDATA", "OUTDATA", dtRqst);

                    Util.gridClear(dgListHold);
                    if (dtRslt.Rows.Count == 0)
                    {
                        if (Util.GetCondition(txtLot).Equals("")) //lot id 가 없는 경우
                        {
                            Util.MessageValidation("SFU1816");  //입력한 조건에 HOLD LOT이 존재하지 않습니다.
                        }
                        else
                        {
                            Util.MessageValidation("SFU2023"); //해당 LOT은 현재 HOLD 상태가 아닙니다.
                        }
                    }
                    else
                    {
                        //2021-11-23 김대근
                        //전극 동일 경우, QA홀드는 릴리즈 할 수 없도록 수정
                        if (_areaTypeCode.Equals("E"))
                        {
                            DataTable dtQAHold = dtRslt.Copy();

                            for (int i = 0; i < dtQAHold.Rows.Count; i++)
                            {
                                if (dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("NG")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("ETC")
                                    || dtQAHold.Rows[i]["QA_INSP_JUDG_VALUE"].ToString().Equals("DF"))
                                {
                                    sQAHoldLot = sQAHoldLot + dtQAHold.Rows[i]["LOTID"].ToString() + ",";
                                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                                    {
                                        if (dtRslt.Rows[j]["LOTID"].ToString().Equals(dtQAHold.Rows[i]["LOTID"].ToString()))
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
                                Util.MessageValidation("SFU8414", sQAHoldLot); //QA Hold는 이 메뉴에서 해제할 수 없습니다. %1
                            }
                        }

                        Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                    }

                    Util.GridSetData(dgListHold, dtRslt, FrameOperation);
                }

                txtLot.Text = string.Empty;
                txtCSTID.Text = string.Empty;

                return true;
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool GetTotalInspectionArea()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "TOTAL_INSPECTION_AREA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(row["CBO_CODE"], LoginInfo.CFG_AREA_ID))
                        return true;
            }
            catch (Exception ex) { }

            return false;
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

            if(CommonVerify.HasTableRow(dtResult))
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

        #endregion
    }
}
