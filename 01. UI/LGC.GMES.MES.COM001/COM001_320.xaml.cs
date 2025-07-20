/*************************************************************************************
 Created Date : 2019.10.30
      Creator : 오화백
   Decription : Test Lot 변경
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.30  DEVELOPER : Initial Created.





 
**************************************************************************************/

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
using C1.WPF;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Linq;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_320 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_320()
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

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //요청상태
            string[] sFilter1 = { "REQ_RSLT_CODE" };
            #region [요청]-TAB EVENT
            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            //요청상태
            _combo.SetCombo(cboReqRslt, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            #endregion

            #region [요청이력]-TAB EVENT
            //동
            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            //요청상태
            _combo.SetCombo(cboReqRsltHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            #endregion

            #region [승인이력]-TAB EVENT
            //동
            _combo.SetCombo(cboAreaHist_Confirm, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            //요청상태
            _combo.SetCombo(cboReqRsltHist_Confirm, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            
            #endregion
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }
        #endregion

        #region Event

        #region [요청]-TAB EVENT

        #region 승인요청 조회 : btnSearch_Click()
        /// <summary>
        /// 승인요청 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        #endregion

        #region 승인요청 팝업열기 및 닫기 - btnReq_Click(), wndPopup_Closed()
        /// <summary>
        /// LOT상태 변경 승인요청
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReq_Click(object sender, RoutedEventArgs e) //물품청구
        {
            COM001_320_TEST_LOT wndPopup = new COM001_320_TEST_LOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_REQ";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        /// <summary>
        /// LOT상태 변경 승인요청 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_320_TEST_LOT window = sender as COM001_320_TEST_LOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        #endregion

        #region 승인요청 취소 및 상세내용 팝업 - dgList_MouseDoubleClick()
        /// <summary>
        /// 승인요청 취소 및 상세내용 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("REQ_NO") && dgList.GetRowCount() > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_USER_ID")).ToString().Equals(LoginInfo.USERID)
                    && Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_RSLT_CODE")).ToString().Equals("REQ"))
                {

                    COM001_320_TEST_LOT wndPopup = new COM001_320_TEST_LOT();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[4];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE"));

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        wndPopup.Closed += new EventHandler(wndPopup_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                else
                {
                    COM001_320_READ wndPopup = new COM001_320_READ();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[4];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_RSLT_CODE"));


                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
            }
        }

        #endregion

        #region  요청ID 색깔변경 - dgList_LoadedCellPresenter()
        /// <summary>
        /// 요청ID 색깔변경 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

            dgList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("REQ_NO"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }

        #endregion

        #endregion

        #region [요청이력]-TAB EVENT

        #region 요청이력 조회 - btnSearchHist_Click()
        /// <summary>
        /// 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        #endregion

        #region 요청이력 상세조회 - dgListHist_MouseDoubleClick()
        private void dgListHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgListHist.CurrentRow != null && dgListHist.CurrentColumn.Name.Equals("LOTID") && dgListHist.GetRowCount() > 0)
            {

                COM001_320_READ wndPopup = new COM001_320_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgListHist.CurrentRow.DataItem, "REQ_NO"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgListHist.CurrentRow.DataItem, "REQ_RSLT_CODE"));


                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

        #endregion

        #region 요청이력 리스트 색변경 - dgListHist_LoadedCellPresenter(), dgListHist_UnloadedCellPresenter()
        private void dgListHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgListHist.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }
        private void dgListHist_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        #endregion

        #endregion

        #region [승인]-TAB EVENT

        #region 승인 조회 : 버튼  - btnSearch_Confirm_Click()

        /// <summary>
        /// 요청승인 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Confirm_Click(object sender, RoutedEventArgs e)
        {
            GetList_Confirm();
        }
        #endregion

        #region 승인 조회 : 요청자  - txtReqUser_Confirm_KeyDown()

        /// <summary>
        /// 요청승인 조회 : 요청자
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtReqUser_Confirm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetList_Confirm();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region 승인 상세정보 - dgList_Confirm_MouseDoubleClick()

        private void dgList_Confirm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList_Confirm.CurrentRow != null && dgList_Confirm.CurrentColumn.Name.Equals("REQ_NO"))
            {

                COM001_320_READ wndPopup = new COM001_320_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList_Confirm.CurrentRow.DataItem, "REQ_NO"));

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

        #endregion

        #region 링크글씨 색 바꾸기 - dgList_Confirm_LoadedCellPresenter()
        private void dgList_Confirm_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("REQ_NO"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
    
                //HOLD LOT 포함여부 색변경
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_YN")).Equals("Y"))
                {
                    dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_YN"].Index).Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                }
                else
                {
                    dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_YN"].Index).Presenter.Background = new SolidColorBrush(Colors.White);
                }


            }));
        }

        #endregion

        #region 요청승인 전체선택 - dgChoice_Checked()
        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sReqNo = DataTableConverter.GetValue(rb.DataContext, "REQ_NO").ToString();

                //승인내용 조회
                GetApprovalList(sReqNo);

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                txtRemark.Text = "";


            }
        }

        #endregion

        #region 승인 클릭 - btnAccept_Click()
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgList_Confirm, "CHK");
            if (drChk.Length == 0)
            {
                Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                return;
            }

            if(drChk[0]["HOLD_YN"].ToString().Equals("Y") && drChk[0]["LOTTYPE_CHG_UNHOLD_FLAG"].ToString().Equals("Y"))
            {
                //자동 Release 됩니다. 승인하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU5143"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Accept();
                            }
                        });
            }
            else if (drChk[0]["HOLD_YN"].ToString().Equals("Y") && !drChk[0]["LOTTYPE_CHG_UNHOLD_FLAG"].ToString().Equals("Y"))
            {
                //HOLD 상태가 유지됩니다. 승인하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU5144"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Accept();
                            }
                        });
            }
            else
            {
                //승인하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU2878"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Accept();

                        }
                    });
            }
       }

        #endregion

        #region 반려클릭 - btnReject_Click()
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgList_Confirm, "CHK");
            if (drChk.Length == 0)
            {
                Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                return;
            }

            //반려하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2866"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Reject();
                            }
                        });
        }


        #endregion

        #endregion

        #region [승인이력]-TAB EVENT

        #region 승인이력 조회 - btnSearchHist_Confirm_Click()
        /// <summary>
        /// 승인이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchHist_Confirm_Click(object sender, RoutedEventArgs e)
        {
            GetListHist_Confirm();
        }
        #endregion

        #endregion

        #endregion

        #region Mehod

        #region [승인요청]-TAB EVENT

        #region 승인요청 내용 조회 - GetList();
        /// <summary>
        /// 승인요청 내용 조회
        /// </summary>
        public void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtLotID).Equals("")) //lot id 가 없는 경우
                {

                    dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                    dr["USERNAME"] = Util.GetCondition(txtReqUser);
                    dr["APPR_BIZ_CODE"] = "LOTTYPE_CHANGE";
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRslt, bAllNull: true);
                    if (!Util.GetCondition(txtCSTID).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTID);

                    dtRqst.Rows.Add(dr);

                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotID);
                    dtRqst.Rows.Add(dr);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST_LOTTYPE_CHANGE", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #endregion

        #region [승인요청이력]-TAB EVENT

        #region 승인요청이력 조회 - GetListHist()
     
        /// <summary>
        /// 승인요청 이력 조회
        /// </summary>
        public void GetListHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtLotIDHist).Equals("")) //lot id 가 없는 경우
                {

                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["APPR_BIZ_CODE"] = "LOTTYPE_CHANGE";
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);

                    if (!Util.GetCondition(txtCSTIDHist).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDHist);

                    dtRqst.Rows.Add(dr);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIDHist);

                    dtRqst.Rows.Add(dr);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_HIST_LOTTYPE_CHANGE", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgListHist, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion



        #endregion

        #region [승인]-TAB EVENT
     
        #region 승인 List 조회: GetList_Confirm()
        public void GetList_Confirm()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERNAME"] = Util.GetCondition(txtReqUser);
                dr["USERID"] = LoginInfo.USERID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_TARGET_LIST_LOTTYPE_CHANGE", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgList_Confirm, dtRslt, FrameOperation, true);
                Util.gridClear(dgAccept);

                txtRemark.Text = string.Empty;
                txtReqUser_Confirm.Text = string.Empty;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 승인자 정보 가져오기 : GetApprovalList()
        private void GetApprovalList(string sReqNo)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = sReqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_DETAIL_LIST", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgAccept, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 승인: Accept()
        /// <summary>
        /// 승인
        /// </summary>
        private void Accept()
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgList_Confirm, "CHK");

            try
            {


                DataSet dsRqst = new DataSet();

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "INDATA";

                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("APPR_USERID", typeof(string));
                dtRqst.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("APPR_NOTE", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "APP";
                dr["APPR_NOTE"] = Util.GetCondition(txtRemark) + " "+ "Auto Release - LOT Type Change";
                dr["APPR_BIZ_CODE"] = drChk[0]["APPR_BIZ_CODE"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR", "INDATA", "OUTDATA,LOT_INFO", dsRqst);

                Util.AlertInfo("SFU1690");  //승인되었습니다.

                MailSend mail = new CMM001.Class.MailSend();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { //다음차수 안내메일
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, dsRslt.Tables["OUTDATA"].Rows[0]["APPR_USERID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                }
                else
                {  //완료메일
                    string sMsg = ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                }

                //Util.AlertInfo("WIP 관리비즈룰 필요");
                GetList_Confirm();
                Util.gridClear(dgAccept);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 반려 : Reject()
        /// <summary>
        /// 반려
        /// </summary>
        private void Reject()
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgList_Confirm, "CHK");

            try
            {
                DataSet dsRqst = new DataSet();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("APPR_USERID", typeof(string));
                dtRqst.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("APPR_NOTE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "REJ";
                dr["APPR_NOTE"] = txtRemark.Text;
                dtRqst.Rows.Add(dr);
                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR", "INDATA", "OUTDATA,LOT_INFO", dsRqst);

                MailSend mail = new CMM001.Class.MailSend();
                string sMsg = ObjectDic.Instance.GetObjectName("반려");
                string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));

                Util.AlertInfo("SFU1541");  //반려되었습니다.

                //Util.AlertInfo("WIP 관리비즈룰 필요");

                GetList_Confirm();
                Util.gridClear(dgAccept);
                txtRemark.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 참조자 가져오기 : GetCC()
        private string GetCC(string sReqNo)
        {
            string sCC = "";
            try
            {


                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = sReqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REF", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    sCC += dtRslt.Rows[i]["USERID"].ToString() + ";";
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return sCC;

        }


        #endregion

        #endregion

        #region [승인이력]-TAB EVENT

        #region 승인이력 List 조회 -GetListHist_Confirm()
        public void GetListHist_Confirm()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtLotIDHist_Confirm).Equals("")) //lot id 가 없는 경우
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist_Confirm);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist_Confirm);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist_Confirm, bAllNull: true);
                    if (!Util.GetCondition(txtCSTIDHist_Confirm).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDHist_Confirm);
                    dtRqst.Rows.Add(dr);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIDHist_Confirm);
                    dtRqst.Rows.Add(dr);
                }
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_APPR_HIST_LOTTYPE_CHANGE", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgListHist_Confirm, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #endregion

       
    }
}
