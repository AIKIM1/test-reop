/*************************************************************************************
 Created Date : 2021.09.14
      Creator : 김민석
   Decription : Cell 공급 이력 조회 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/



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
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_087 : UserControl, IWorkArea
    {
        public bool bClick = false;

        #region Declaration & Constructor & Init

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Initialize 
        public PACK001_087()
        {
            try
            {
                InitializeComponent();
                this.Loaded += UserControl_Loaded;
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("A"))
                {
                    
                    dgCellReqHist.Columns["CELL_A_PRJT"].Visibility = Visibility.Visible;
                    dgCellReqHist.Columns["ASSYP_PRODID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgCellReqHist.Columns["CELL_P_PRJT"].Visibility = Visibility.Visible;
                    dgCellReqHist.Columns["PACKP_MODLID"].Visibility = Visibility.Visible;
                }
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

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                SetMboReqArea();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Button

        #endregion Button

        #region Grid
        private void dgCellReqHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (!(e.Cell.Row.Index > 0)) return;


                    if (e.Cell.Column.Name == "CELL_SPLY_RSPN_QTY")
                    {
                        
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Thin;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Grid

        #region COMBO BOX

        private void mboReqArea_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetMboSplyArea();
                    SetMboProd();
                    mboSplyArea.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void mboSplyArea_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetMboProd();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion 

        #region 수량 선택 변경
        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(null, null);
        }
        #endregion

        #region Event

        #region 조회 버튼 클릭
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mboReqArea.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboSplyArea.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboCellProdId.SelectedItems == null || mboCellProdId.SelectedItemsToString == "" || mboCellProdId.SelectedItemsToString == null)
                {
                    Util.Alert("SFU1895");  //제품을 선택하세요.
                    return;
                }

                SearchCellReqHist();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Excel 버튼 클릭
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.Date.AddDays(+7);
                    HiddenLoadingIndicator();
                    Util.MessageValidation("SFU3567"); // 조회기간은 7일을 초과할 수 없습니다.                    
                    return;
                }

                char sp = ':';
                object[] arrCellPjt = mboCellProdId.SelectedItems.ToArray();
                string[] strCellTemp = null;
                strCellTemp = arrCellPjt.Cast<string>().ToArray();
                string[] strCellProd = new string[1];

                for (int i = 0; i < strCellTemp.Count(); i++)
                {
                    strCellProd[0] = strCellProd[0] + ',' + strCellTemp[i].Split(sp)[1];
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("DATE_ST", typeof(string));
                INDATA.Columns.Add("DATE_ED", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PACK_AREAID", typeof(string));
                INDATA.Columns.Add("ASSY_AREAID", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["DATE_ST"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DATE_ED"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["PRODID"] = strCellProd[0];
                dr["PACK_AREAID"] = Util.NVC(mboReqArea.SelectedItemsToString) == "" ? null : mboReqArea.SelectedItemsToString;
                dr["ASSY_AREAID"] = Util.NVC(mboSplyArea.SelectedItemsToString) == "" ? null : mboSplyArea.SelectedItemsToString;
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_CSLY_TOTAL_HIST", "INDATA", "RSLTDT", INDATA, (dsRslt, bizException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Dictionary<string, string> dicHeader = new Dictionary<string, string>();

                        if (dsRslt.Rows.Count > 0)

                        {
                            foreach (DataColumn dc in dsRslt.Columns)
                            {
                                dicHeader.Add(dc.ColumnName.ToString(), dc.ColumnName.ToString());
                            }

                        }

                        new ExcelExporter().DtToExcel(dsRslt, "CELL_SPLY_HISTORY", dicHeader);

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
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region 응답수량 더블 클릭
        private void dgCellReqHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCellReqHist.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CELL_SPLY_RSPN_QTY" && !(Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "CELL_SPLY_RSPN_QTY")) == null))
                    {
                        ShowTestMode();
                        string sRspnId = Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "CELL_SPLY_RSPN_ID"));
                        string sReqId = Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "CELL_SPLY_REQ_ID"));
                        string sProdId = Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "PRODID"));
                        SearchCellSplyDetlFlow(sRspnId, sReqId, sProdId);
                    }

                }

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Util.MessageException(ex);

            }
        }
        #endregion

        #region 하단부 클릭
        private void dgShipRcv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HideTestMode();
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

        #region 요청동 콤보 조회
        private void SetMboReqArea()
        {

            try
            {
                string sSelectedValue = mboReqArea.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboReqArea.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                    {
                        mboReqArea.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공급동 콤보 조회
        private void SetMboSplyArea()
        {

            try
            {
                string sSelectedValue = mboSplyArea.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.ASSY;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboSplyArea.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                    {
                        mboSplyArea.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region  Cell제품ID 콤보 조회
        private void SetMboProd()
        {
            try
            {
               
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("COND", typeof(Int32));


                DataRow dr = RQSTDT.NewRow();
                //dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["CMCDTYPE"] = "CSLY_CELL_PRJT";
                dr["COND"] = 1;

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_CHK_CMMCD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboCellProdId.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region CELL공급 요청 이력 조회
        private void SearchCellReqHist(string sLotList = "")
        {
            try
            {
                ShowLoadingIndicator();

                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.Date.AddDays(+7);
                    HiddenLoadingIndicator();
                    Util.MessageValidation("SFU3567"); // 조회기간은 7일을 초과할 수 없습니다.                    
                    return;
                }

                char sp = ':';
                object[] arrCellPjt = mboCellProdId.SelectedItems.ToArray();
                string[] strCellTemp = null;
                strCellTemp = arrCellPjt.Cast<string>().ToArray();
                string[] strCellProd = new string[1];

                for (int i = 0; i < strCellTemp.Count(); i++)
                {
                    strCellProd[0] = strCellProd[0] + ',' + strCellTemp[i].Split(sp)[1];
                }

                //strCellProd[0] += strCellProd[0] + ',' + "ACEN1060I-B1-A01";


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("DATE_ST", typeof(string));
                RQSTDT.Columns.Add("DATE_ED", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PACK_AREAID", typeof(string));
                RQSTDT.Columns.Add("ASSY_AREAID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["DATE_ST"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DATE_ED"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["PRODID"] =strCellProd[0];
                dr["PACK_AREAID"] = Util.NVC(mboReqArea.SelectedItemsToString) == "" ? null : mboReqArea.SelectedItemsToString;
                dr["ASSY_AREAID"] = Util.NVC(mboSplyArea.SelectedItemsToString) == "" ? null : mboSplyArea.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_CSLY_HIST", "RQSTDT", "RSLTDT", RQSTDT, (dsRslt, bizException) =>
                {
                    try
                    {
                        dgShipRcv.LoadedCellPresenter += dgCellReqHist_LoadedCellPresenter;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgCellReqHist, dsRslt, FrameOperation, false);
                        Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dgCellReqHist.Rows.Count - 1));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        dgShipRcv.LoadedCellPresenter -= dgCellReqHist_LoadedCellPresenter;
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

        #region CELL공급 상세 이력 조회
        private void SearchCellSplyDetlFlow(string sRspnId, string sReqId, string sProdId)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RSPNID", typeof(string));
                RQSTDT.Columns.Add("REQID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["RSPNID"] = sRspnId;
                dr["REQID"] = sReqId;
                dr["PRODID"] = sProdId;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SPLY_DETL_FLOW", "RQSTDT", "RSLTDT", RQSTDT, (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgShipRcv, dsRslt, FrameOperation, false);
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

        #region ShowLoadingIndicator
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region HiddenLoadingIndicator
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Animation
        private void showTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (GridMain.RowDefinitions[2].Height.Value > 0 && GridMain.RowDefinitions[3].Height.Value > 0) return;

                GridMain.RowDefinitions[3].Height = new GridLength(34, GridUnitType.Pixel);
                GridMain.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
            }));

            //bTestMode = true;
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (GridMain.RowDefinitions[1].Height.Value <= 0) return;

                GridMain.RowDefinitions[3].Height = new GridLength(0);
                GridMain.RowDefinitions[4].Height = new GridLength(0);
            }));


        }
        #endregion

        #endregion Method




    }
}