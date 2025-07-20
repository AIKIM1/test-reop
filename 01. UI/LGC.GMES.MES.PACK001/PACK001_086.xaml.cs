/*************************************************************************************
 Created Date : 2021.09.07
      Creator : 김민석
   Decription : 폐기장 입고 현황/이력 조회(Pack)
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

//#define TEST

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;



namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_086 : UserControl, IWorkArea
    {
        public bool bClick = false;

        #region Declaration & Constructor & Init

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #region Initialize 
        public PACK001_086()
        {
            try
            {
                InitializeComponent();
                this.Loaded += UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            InitCombo();

            tbWipListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbRecent.Text = ObjectDic.Instance.GetObjectName("최신");

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }


        #endregion 

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                String[] sFiltercboAreaRslt = { Area_Type.PACK };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboAreaRslt, sCase: "AREA_PACK");

                //상태구분 콤보박스 HARD CODING
                DataTable inTable = new DataTable();
                DataTable dt_Return = new DataTable();

                inTable.Columns.Add("CBO_NAME", typeof(string));
                inTable.Columns.Add("CBO_CODE", typeof(string));

                inTable.Rows.Add(ObjectDic.Instance.GetObjectName("WAIT : 폐기장 입고 대기"), "WAIT");
                inTable.Rows.Add(ObjectDic.Instance.GetObjectName("CMPL : 폐기장 입고 완료"), "CMPL");

                mboStat.ItemsSource = DataTableConverter.Convert(inTable);


                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 1000, 10000, 1000);
                this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #region Button

        //#region 조회 버튼 클릭
        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        bClick = false;
        //        loadingIndicator.Visibility = Visibility.Visible;

        //        if (bClick == false)
        //        {
        //            Action act = () =>
        //            {
        //                bClick = true;

        //                Refresh();
        //                HideTestMode();
        //                SearchData();

        //            };

        //            btnSearch.Dispatcher.Invoke(act);
        //        }
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);

        //        HiddenLoadingIndicator();

        //        bClick = false;
        //    }
        //}
        //#endregion

        //#region 요청 버튼 클릭
        //private void btnRequest_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        PACK001_078_CELLREQUEST popup = new PACK001_078_CELLREQUEST();
        //        popup.FrameOperation = this.FrameOperation;

        //        if (popup != null)
        //        {
        //            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 3;
        //            DataTable dtReqCond = new DataTable();
        //            dtReqCond = DataTableConverter.Convert(dgPlan.ItemsSource);

        //            object[] Parameters = new object[3];
        //            Parameters[0] = cboArea.SelectedValue;
        //            Parameters[1] = dtReqCond.Rows[index]["MTRLID"].ToString();
        //            Parameters[2] = rdoToday.IsChecked == true ? "1" : "0";
        //            C1WindowExtension.SetParameters(popup, Parameters);

        //            popup.Closed -= popup_Closed;
        //            popup.Closed += popup_Closed;

        //            popup.ShowModal();
        //            popup.CenterOnScreen();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //#endregion

        //#region PACK 생산현황 숨기기
        //private void dgStock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    HideTestMode();
        //}
        //#endregion

        //#region PACK 생산현황 조회 이벤트
        //private void dgPlan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    ShowLoadingIndicator();
        //    Point pnt = e.GetPosition(null);
        //    C1.WPF.DataGrid.DataGridCell cell = dgPlan.GetCellFromPoint(pnt);

        //    if (cell != null)
        //    {
        //        if(cell.Column.Name == "CELL_PRJT" && !(Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT")) == null))
        //        {
        //            ShowTestMode();
        //            string sCellPrjt = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT"));
        //            string sPackPjt = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "PACK_PRJT"));
        //            string sProdId = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "MTRLID"));
        //            int iToday = rdoToday.IsChecked == true ? 1 : 0;
        //            SearchData2(sProdId, sCellPrjt, sPackPjt, iToday);
        //        }

        //    }
        //    HiddenLoadingIndicator();
        //}
        //#endregion
        #endregion Button

        #region Grid

        #endregion Grid

        #region COMBO BOX
        //#region COMBO BOX SETTING
        //private void SetCboPrj()
        //{
        //    try
        //    {
        //        string sSelectedValue = cboArea.SelectedValue.ToString();
        //        string[] sSelectedList = sSelectedValue.Split(',');

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));
        //        RQSTDT.Columns.Add("SHOPID", typeof(string));
        //        RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
        //        RQSTDT.Columns.Add("USERID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = sSelectedValue;
        //        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        dr["SYSTEM_ID"] = LoginInfo.SYSID;
        //        dr["USERID"] = LoginInfo.USERID;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        cboPackPjt.DisplayMemberPath = "CBO_NAME";
        //        cboPackPjt.SelectedValuePath = "CBO_CODE";

        //        for (int i = 0; i < dtResult.Rows.Count; i++)
        //        {
        //            if (sSelectedList.Length > 0 && sSelectedList[0] != "")
        //            {
        //                for (int j = 0; j < sSelectedList.Length; j++)
        //                {
        //                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
        //                    {
        //                        cboPackPjt.Check(i);
        //                        break;
        //                    }
        //                }
        //            }
        //            else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
        //            {
        //                cboPackPjt.Check(i);
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        #endregion

        #endregion Event

        #region 동 콤보 선택 변경
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    mboEqsgId.isAllUsed = true;
                    SetMboEQSG();
                    SetmboPjt();
                    SetmboPrdtType();
                    SetmboProd();
                    mboPjt.isAllUsed = true;
                    mboProd.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 라인 콤보 선택 변경
        private void mboEqsgId_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetmboPjt();
                    SetmboPrdtType();
                    SetmboProd();
                    mboPrdtType.isAllUsed = true;
                    mboPjt.isAllUsed = true;
                    mboProd.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private void mboPjt_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetmboProd();
                    mboProd.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void mboPrdtType_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetmboProd();
                    SetmboPjt();
                    mboProd.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(null, null);
        }


        #region Event
        #region 조회 버튼 클릭
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedValue.ToString() == "" || cboArea.SelectedValue.ToString() == null)
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboEqsgId.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if (mboPrdtType.SelectedItemsToString == "")
                {
                    Util.Alert("SFU2950");  //제품구분을 먼저 선택하세요
                    return;
                }


                if (mboPjt.SelectedItemsToString == "")
                {
                    Util.Alert("SFU3478"); //PJT을 선택하세요.
                    return;
                }


                if (mboProd.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1895");  //제품을 선택하세요.
                    return;
                }

                if (mboStat.SelectedItemsToString == "")
                {
                    Util.Alert("SFU5059");  //상태를 선택하세요.
                    return;
                }


                SearchScrapInputData();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Method

        #region 팝업 닫기
        void cellPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_084_CELL_POPUP popup = sender as PACK001_084_CELL_POPUP;
                if (popup.DialogResult == MessageBoxResult.Cancel)
                {

                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 팝업 닫기
        void abnormalPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_084_ABNORMAL_POPUP popup = sender as PACK001_084_ABNORMAL_POPUP;
                if (popup.DialogResult == MessageBoxResult.Cancel)
                {

                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 라인 콤보 조회
        private void SetMboEQSG()
        {
            try
            {
                string sSelectedValue = mboEqsgId.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboEqsgId.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboEqsgId.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        mboEqsgId.Check(i);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region 팩 프로젝트 콤보 조회
        private void SetmboPjt()
        {
            try
            {
                this.mboPjt.SelectionChanged -= new System.EventHandler(this.mboPjt_SelectionChanged);

                string sSelectedValue = mboPjt.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = mboEqsgId.SelectedItemsToString;
                dr["PRDT_CLSS_CODE"] = mboPrdtType.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                mboPjt.ItemsSource = DataTableConverter.Convert(dtResult);


                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboPjt.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        mboPjt.Check(i);
                        break;
                    }
                }
                this.mboPjt.SelectionChanged += new System.EventHandler(this.mboPjt_SelectionChanged);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region 제품구분 콤보 조회
        private void SetmboPrdtType()
        {
            try
            {
                this.mboPrdtType.SelectionChanged -= new System.EventHandler(this.mboPrdtType_SelectionChanged);
                string sSelectedValue = mboPrdtType.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = mboEqsgId.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboPrdtType.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboPrdtType.Check(i);
                                break;
                            }
                        }
                    }
                }
                this.mboPrdtType.SelectionChanged += new System.EventHandler(this.mboPrdtType_SelectionChanged);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region 제품ID 콤보 조회
        private void SetmboProd()
        {
            try
            {
                string sSelectedValue = mboProd.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = mboEqsgId.SelectedItemsToString;
                dr["MODLID"] = mboPjt.SelectedItemsToString == "" ? null : mboPjt.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = mboPrdtType.SelectedItemsToString == "" ? null : mboPrdtType.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_PACK_MULTI_CBO_V2", "RQSTDT", "RSLTDT", RQSTDT);
                mboProd.ItemsSource = DataTableConverter.Convert(dtResult);


                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboProd.Check(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }


        #endregion

        #region 폐기장 입고 현황 조회
        private void SearchScrapInputData(string sLotList = "")
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("FROM_ACT_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_ACT_DTTM", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRDTTYPE", typeof(string));
                RQSTDT.Columns.Add("PJT", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SCRAP_STAT", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COUNT"] = cboListCount.SelectedValue;

                if (!string.IsNullOrEmpty(sLotList))
                {
                    dr["LOTID"] = sLotList;
                }
                else
                {
                    dr["FROM_ACT_DTTM"] = dtpDateFrom.SelectedDateTime.AddDays(-1).ToString("yyyyMMdd");
                    dr["TO_ACT_DTTM"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                    dr["AREAID"] = cboArea.SelectedValue.ToString();
                    dr["EQSGID"] = mboEqsgId.SelectedItemsToString;
                    dr["PRDTTYPE"] = Util.NVC(mboPrdtType.SelectedItemsToString) == "" ? null : mboPrdtType.SelectedItemsToString;
                    dr["PJT"] = Util.NVC(mboPjt.SelectedItemsToString) == "" ? null : mboPjt.SelectedItemsToString;
                    dr["PRODID"] = Util.NVC(mboProd.SelectedItemsToString) == "" ? null : mboProd.SelectedItemsToString;
                    dr["SCRAP_STAT"] = Util.NVC(mboStat.SelectedItemsToString) == "" ? null : mboStat.SelectedItemsToString;
                }
                RQSTDT.Rows.Add(dr);


                new ClientProxy().ExecuteService("BR_PRD_SEL_SCRAP_CMPL", "INDATA", "OUTDATA", RQSTDT, (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgScrapInputRslt, dsRslt, FrameOperation, false);
                        Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dgScrapInputRslt.Rows.Count - 1));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #endregion Method

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgScrapInputRslt);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    ShowLoadingIndicator();

                    string sLOTID = txtLotIdBox.Text.Trim();

                    if (string.IsNullOrEmpty(sLOTID))
                    {
                        Util.MessageInfo("SFU1190");  // 조회할 LOT ID 를 입력하세요.
                        return;
                    }

                    SearchScrapInputData(sLOTID);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {

                }
            }
        }

        private void txtLotIdBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    Clipboard.Clear();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    //if (sPasteStrings.Count() > 100)
                    //{
                    //    Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                    //    return;
                    //}

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
                    SearchScrapInputData(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {

                }
            }
        }
    }
}