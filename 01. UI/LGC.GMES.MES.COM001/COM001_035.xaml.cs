/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.09.06  정재홍    : [E20230802-000826] - 조회 조건 Box ID 추가
  2024.05.12  김도형    : [E20240403-001190] [자동차_전극][ESWA PI] Authorities adjustment 
  2025.02.10  이민형    : 물품청구 버튼 전극은 조립 처럼 보이지 않게 처리
  2025.02.11  이민형    : LOT REALEASE 버튼 전극,조립 보이지 않게 처리
   
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_035()
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


            //동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase:"AREA");

            ////라인
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            ////공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            ////설비
            //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            //요청구분
            string[] sFilter = { "APPR_BIZ_CODE" };
            _combo.SetCombo(cboReqType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            _combo.SetCombo(cboReqTypeHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //요청구분
            string[] sFilter1 = { "REQ_RSLT_CODE" };
            _combo.SetCombo(cboReqRslt, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            _combo.SetCombo(cboReqRsltHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
        }
        #endregion

        #region Event

            #region LOADED EVENT
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            SetbtnRequestScrapYieldVisibility();

            if (string.Equals(GetAreaType(), "P"))
            {
                btnRequestRelease.Visibility = Visibility.Collapsed;
                btnRequestReq.Visibility = Visibility.Collapsed;
                btnRequestScrap.Visibility = Visibility.Collapsed;
                btnRequestScrapYield.Visibility = Visibility.Collapsed;
                btnRequestHot.Visibility = Visibility.Collapsed;
                btnRequestReleaseReservation.Visibility = Visibility.Collapsed;                
            }
            else if (string.Equals(GetAreaType(), "A"))
            {
                btnRequestReq.Visibility = Visibility.Collapsed;
                btnRequestReleaseReservation.Visibility = Visibility.Collapsed;
            }
            else if (string.Equals(GetAreaType(), "E"))
            {
                btnRequestReq.Visibility = Visibility.Collapsed;
                btnRequestReleaseReservation.Visibility = Visibility.Collapsed;
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
                {
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return "";
        }

        private void SetbtnRequestScrapYieldVisibility()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                newRow["CMCDTYPE"] = "REQUEST_SCRAP_YIELD_VISIBLE";

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

                if (searchResult != null && searchResult.Rows.Count > 0)
                {
                    btnRequestScrapYield.Visibility = Visibility.Visible;
                }
                else
                {
                    btnRequestScrapYield.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [요청]-TAB EVENT
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnRequestRelease_Click(object sender, RoutedEventArgs e) // RELEASE요청
        {
            COM001_035_REQUEST wndPopup = new COM001_035_REQUEST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_RELEASE";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnRequestReq_Click(object sender, RoutedEventArgs e) //물품청구
        {

            // [E20240403-001190] [자동차_전극][ESWA PI] Authorities adjustment  
            string[] sAttrbute = { null ,"COM001_035", "MATERIAL_REQUEST_W"};
            if (IsButtonPermissionGroupByBtnGroupUse("PERMISSIONS_PER_BUTTON_FORM", "", sAttrbute))
            {
                if (!CheckButtonPermissionGroupByBtnGroupID("MATERIAL_REQUEST_W"))
                {
                    return;
                }
            }

            COM001_035_REQUEST1 wndPopup = new COM001_035_REQUEST1();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_REQ";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup1_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnRequestScrap_Click(object sender, RoutedEventArgs e) //폐기요청
        {

            // [E20240403-001190] [자동차_전극][ESWA PI] Authorities adjustment  
            string[] sAttrbute = { null, "COM001_035", "SCRAP_REQUEST_W" };
            if (IsButtonPermissionGroupByBtnGroupUse("PERMISSIONS_PER_BUTTON_FORM", "", sAttrbute))
            {
                if (!CheckButtonPermissionGroupByBtnGroupID("SCRAP_REQUEST_W"))
                {
                    return;
                }
            }

            COM001_035_REQUEST wndPopup = new COM001_035_REQUEST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_SCRAP";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnRequestScrapSection_Click(object sender, RoutedEventArgs e)  //부분폐기
        {
            COM001_035_REQUEST wndPopup = new COM001_035_REQUEST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_SCRAP_SECTION";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnRequestScrapYield_Click(object sender, RoutedEventArgs e) //수율반영폐기
        {
            COM001_035_REQUEST_YIELD wndPopup = new COM001_035_REQUEST_YIELD();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_SCRAP_YIELD";
                Parameters[2] = Util.GetCondition(cboArea, bAllNull: true);

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupYield_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnRequestHot_Click(object sender, RoutedEventArgs e)
        {
            COM001_035_REQUEST_HOT wndPopup = new COM001_035_REQUEST_HOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_REQ_HOT";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupHot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnRequestReleaseReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_035_REQUEST_RESERVATION popReservation = new COM001_035_REQUEST_RESERVATION {FrameOperation = FrameOperation};

                object[] parameters = new object[4];
                parameters[0] = "NEW";
                parameters[1] = "LOT_RELEASE_RESERVE";

                C1WindowExtension.SetParameters(popReservation, parameters);
                popReservation.Closed += popReservation_Closed;

                Dispatcher.BeginInvoke(new Action(() => popReservation.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popReservation_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_RESERVATION window = sender as COM001_035_REQUEST_RESERVATION;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("REQ_NO") && dgList.GetRowCount() > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_USER_ID")).ToString().Equals(LoginInfo.USERID)
                    && Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_RSLT_CODE")).ToString().Equals("REQ"))
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE")).ToString().Equals("LOT_REQ"))
                    {
                        COM001_035_REQUEST1 wndPopup = new COM001_035_REQUEST1();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[4];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE"));

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopup1_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE")).ToString().Equals("LOT_REQ_HOT"))
                    {
                        COM001_035_REQUEST_HOT wndPopup = new COM001_035_REQUEST_HOT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE"));

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopupHot_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE")).ToString().Equals("REQUEST_BIZWF_LOT"))
                    {
                        COM001_035_REQUEST_BIZWFLOT wndPopup = new COM001_035_REQUEST_BIZWFLOT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE"));

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE")).ToString().Equals("REQUEST_CANCEL_BIZWF_LOT"))
                    {
                        COM001_035_REQUEST_BIZWFLOT wndPopup = new COM001_035_REQUEST_BIZWFLOT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE"));

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else
                    {
                        COM001_035_REQUEST wndPopup = new COM001_035_REQUEST();
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
                }
                else
                {
                    COM001_035_READ wndPopup = new COM001_035_READ();
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

            #region [요청이력]-TAB EVENT
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            object[] Parameters = new object[1];

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            //dicParam.Add("PROC", Process.STACKING_FOLDING);
            dicParam.Add("reportName", "Fold");
            dicParam.Add("LOTID", "LOT212");
            dicParam.Add("QTY", "123");
            dicParam.Add("MAGID", "MAG123");
            dicParam.Add("MAGIDBARCODE", "MAG123");
            dicParam.Add("LARGELOT", "LARGELOT");
            dicParam.Add("MODEL", "MODEL123");
            dicParam.Add("REGDATE", "2016-08-08");
            dicParam.Add("EQPTNO", "EQP111");
            dicParam.Add("TITLEX", "TITLEX123");

            //Parameters[0] = dicParam;
            //C1WindowExtension.SetParameters(print, Parameters);

            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT(dicParam);
            print.FrameOperation = FrameOperation;

            this.Dispatcher.BeginInvoke(new Action(() => print.ShowModal()));
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        private void dgListHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgListHist.CurrentRow != null && dgListHist.CurrentColumn.Name.Equals("LOTID") && dgListHist.GetRowCount() > 0)
            {

                COM001_035_READ wndPopup = new COM001_035_READ();
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
        #endregion

            #region TAB 공통 EVENT
        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST window = sender as COM001_035_REQUEST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        private void wndPopup1_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST1 window = sender as COM001_035_REQUEST1;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        private void wndPopupYield_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_YIELD window = sender as COM001_035_REQUEST_YIELD;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        private void wndPopupHot_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_HOT window = sender as COM001_035_REQUEST_HOT;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [작업대상 가져오기]
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
                dtRqst.Columns.Add("BOXID", typeof(string));
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
                    dr["APPR_BIZ_CODE"] = Util.GetCondition(cboReqType, bAllNull:true);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRslt, bAllNull:true);
                    if (!Util.GetCondition(txtCSTID).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTID);

                    // [E20230802-000826] - speical work UI improvement
                    if (!Util.GetCondition(txtBoxid).Equals(""))
                        dr["BOXID"] = Util.GetCondition(txtBoxid);

                    dtRqst.Rows.Add(dr);

                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST", "INDATA", "OUTDATA", dtRqst);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotID);

                    dtRqst.Rows.Add(dr);

                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST_BY_LOT", "INDATA", "OUTDATA", dtRqst);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgList);

                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgList, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


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
                    dr["APPR_BIZ_CODE"] = Util.GetCondition(cboReqTypeHist, bAllNull: true);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;

                    if (!Util.GetCondition(txtCSTIDHist).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDHist);

                    dtRqst.Rows.Add(dr);

                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST", "INDATA", "OUTDATA", dtRqst);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIDHist);

                    dtRqst.Rows.Add(dr);

                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST_BY_LOT", "INDATA", "OUTDATA", dtRqst);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_HIST", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgList);

                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [E20240403-001190] [자동차_전극][ESWA PI] Authorities adjustment 

        private bool IsButtonPermissionGroupByBtnGroupUse(string sCodeType, string sCodeName, string[] sAttrbute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';

                if (sAttrbute != null)
                {
                    for (int i = 0; i < sAttrbute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttrbute[i]) ? (object)DBNull.Value : sAttrbute[i];
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        /// <summary>
        /// 권한 그룹별 권한 체크
        /// </summary>
        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string)); 

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = "COM001_035";
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_FORM", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    if (sBtnGrpID == "MATERIAL_REQUEST_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("물품청구");
                    else if (sBtnGrpID == "SCRAP_REQUEST_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("폐기요청"); 

                    Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion [E20240403-001190] [자동차_전극][ESWA PI] Authorities adjustment 

        #endregion

        private void btnBizWFLotRequest_Click(object sender, RoutedEventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT wndPopup = new COM001_035_REQUEST_BIZWFLOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "REQUEST_BIZWF_LOT";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopupBizWFLot_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT window = sender as COM001_035_REQUEST_BIZWFLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        private void btnBizWFLotCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT wndPopup = new COM001_035_REQUEST_BIZWFLOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "REQUEST_CANCEL_BIZWF_LOT";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
    }
}
