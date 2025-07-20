/*************************************************************************************
 Created Date : 2018.03.19
      Creator : 장만철
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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
    public partial class COM001_223 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_223()
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
            // string[] sFilter = { "APPR_BIZ_CODE" };
            //_combo.SetCombo(cboReqType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
            //_combo.SetCombo(cboReqTypeHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

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
        }
        #endregion

            #region [요청]-TAB EVENT
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnRequestHot_Click(object sender, RoutedEventArgs e)
        {
            COM001_223_REQUEST_HOT wndPopup = new COM001_223_REQUEST_HOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = "NEW";
                Parameters[1] = "BOX_REQ_HOT";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupHot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("REQ_NO") && dgList.GetRowCount() > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_USER_ID")).ToString().Equals(LoginInfo.USERID)
                    && Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_RSLT_CODE")).ToString().Equals("REQ"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "APPR_BIZ_CODE")).ToString().Equals("BOX_REQ_HOT"))
                    {
                        COM001_223_REQUEST_HOT wndPopup = new COM001_223_REQUEST_HOT();
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
                }
                else
                {
                    COM001_223_READ wndPopup = new COM001_223_READ();
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
                COM001_223_READ wndPopup = new COM001_223_READ();
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

        private void wndPopupHot_Closed(object sender, EventArgs e)
        {
            COM001_223_REQUEST_HOT window = sender as COM001_223_REQUEST_HOT;

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
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtBoxID).Equals("")) //Box id 가 없는 경우
                {
                    dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                    dr["USERNAME"] = Util.GetCondition(txtReqUser);
                    dr["APPR_BIZ_CODE"] = "BOX_REQ_HOT"; // 요청 구분        
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRslt, bAllNull:true);                    
                    dr["CSTID"] = null; //box hold 요청시는 사용 안함.

                    dtRqst.Rows.Add(dr);
                }
                else //Box id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtBoxID); //LOTID 컬럼에 BOXID 입력 (기존에 DA가 LOT으로 조회되서 DA를 그대로 사용하기 위해)

                    dtRqst.Rows.Add(dr);
                }

                Util.gridClear(dgList);
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST", "INDATA", "OUTDATA", dtRqst);
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
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));   
                dtRqst.Columns.Add("PRODID", typeof(string));               
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtBoxIDHist).Equals("")) //box id 가 없는 경우
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["APPR_BIZ_CODE"] = "BOX_REQ_HOT"; // 요청구분                   
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                  
                    dtRqst.Rows.Add(dr);
                }
                else //box id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtBoxIDHist);

                    dtRqst.Rows.Add(dr);
                }

                Util.gridClear(dgListHist);
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_BOX_HIST", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

    }
}
