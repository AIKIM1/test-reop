/*************************************************************************************
 Created Date : 2023.09.08
      Creator : 백광영
   Decription : Cell Pallet 반송명령 생성 현황
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
    /// <summary>
    /// PACK003_045.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK003_045 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public bool bClick = false;
        string _sReqID = string.Empty;
        decimal _dReqQty = 0;

        Util _util = new Util();
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_045()
        {
            try
            {
                InitializeComponent();
                //this.Loaded += UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime firstOfDay = DateTime.Now.AddDays(-7);
            DateTime endOfDay = DateTime.Now;

            dtpDateFrom.IsNullInitValue = true;
            dtpDateTo.IsNullInitValue = true;

            dtpDateFrom.SelectedDateTime = firstOfDay;
            dtpDateTo.SelectedDateTime = endOfDay;

            dtpDateFromHis.IsNullInitValue = true;
            dtpDateToHis.IsNullInitValue = true;

            dtpDateFromHis.SelectedDateTime = firstOfDay;
            dtpDateToHis.SelectedDateTime = endOfDay;

            InitCombo();

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }

        private void InitCombo()
        {
            try
            {
                SetMboReqArea1();
                SetMboReqAreaHis();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Grid
        private void dgCellReqHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    if (e.Cell.Presenter == null)
                //    {
                //        return;
                //    }

                //    if (!(e.Cell.Row.Index > 0)) return;


                //    if (e.Cell.Column.Name == "CELL_SPLY_RSPN_QTY")
                //    {

                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                //        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                //    }
                //    else
                //    {
                //        e.Cell.Presenter.FontWeight = FontWeights.Thin;
                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                //    }

                //}));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Grid

        #region COMBO BOX

        private void mboReqArea1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetMboSplyArea1();
                    SetMboProd1();
                    mboSplyArea1.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void mboReqAreaHis_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetMboSplyAreaHis();
                    SetMboProdHis();
                    mboSplyAreaHis.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void mboSplyArea1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //SetMboProd1();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void mboSplyAreaHis_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //SetMboProd();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 응답수량 더블 클릭
        private void dgCellReqHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                //Point pnt = e.GetPosition(null);
                //C1.WPF.DataGrid.DataGridCell cell = dgCellReqHist.GetCellFromPoint(pnt);

                //if (cell != null)
                //{
                //    if (cell.Column.Name == "CELL_SPLY_RSPN_QTY" && !(Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "CELL_SPLY_RSPN_QTY")) == null))
                //    {
                //        ShowTestMode();
                //        string sRspnId = Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "CELL_SPLY_RSPN_ID"));
                //        string sReqId = Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "CELL_SPLY_REQ_ID"));
                //        string sProdId = Util.NVC(DataTableConverter.GetValue(dgCellReqHist.Rows[cell.Row.Index].DataItem, "PRODID"));
                //        SearchCellSplyDetlFlow(sRspnId, sReqId, sProdId);
                //    }

                //}

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

        #region Event

        #region 조회 버튼 클릭
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mboReqArea1.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboSplyArea1.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboCellProdId1.SelectedItems == null || mboCellProdId1.SelectedItemsToString == "" || mboCellProdId1.SelectedItemsToString == null)
                {
                    Util.Alert("SFU1895");  //제품을 선택하세요.
                    return;
                }

                SearchCellReq();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mboReqAreaHis.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboSplyAreaHis.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (mboCellProdIdHis.SelectedItems == null || mboCellProdIdHis.SelectedItemsToString == "" || mboCellProdIdHis.SelectedItemsToString == null)
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

        #region 취소, 수량변경
        private void btnChangeQty_Click(object sender, RoutedEventArgs e)
        {
            if (dgCellReq.GetRowCount() == 0)
            {
                Util.Alert("SFU1651");   //선택된 데이터가 없습니다.
                return;
            }

            if (_sReqID.Equals(string.Empty) || _dReqQty.Equals(0))
            {
                Util.Alert("SFU1651");   //선택된 데이터가 없습니다.
                return;
            }
            PACK003_045_CHANGEQTY _popupLoad = new PACK003_045_CHANGEQTY();
            _popupLoad.FrameOperation = FrameOperation;

            if (_popupLoad != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Util.NVC(_sReqID); ;
                Parameters[1] = Util.NVC_Decimal(_dReqQty); ;

                C1WindowExtension.SetParameters(_popupLoad, Parameters);

                _popupLoad.Closed += new EventHandler(_popup_Closed);
                _popupLoad.ShowModal();
                _popupLoad.CenterOnScreen();
            }
        }

        private void _popup_Closed(object sender, EventArgs e)
        {
            PACK003_045_CHANGEQTY runStartWindow = sender as PACK003_045_CHANGEQTY;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                SearchCellReq();
            }
            _sReqID = string.Empty;
            _dReqQty = 0;
        }

        private void btnCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellReq.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");   //선택된 데이터가 없습니다.
                    return;
                }

                if (_sReqID.Equals(string.Empty) || _dReqQty.Equals(0))
                {
                    Util.Alert("SFU1651");   //선택된 데이터가 없습니다.
                    return;
                }

                // 선택하신 Job을 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU8914"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        _JobCancel();
                    }
                }
            );
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void _JobCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CELL_SPLY_REQ_ID"] = _sReqID;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_UPD_CELL_SPLY_REQ_CANCEL", "INDATA", "", INDATA);
                //Util.MessageInfo("SFU1270");    //저장되었습니다.

                SearchCellReq();
            }
            catch (Exception ex)
            {
                //Util.MessageInfo(ex.Message.ToString());
                Util.MessageInfo(ex.Data["CODE"].ToString());
            }
            finally
            {
                HiddenLoadingIndicator();
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

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        dgCellReq.SelectedIndex = idx;

                        _sReqID = Util.NVC(DataTableConverter.GetValue(dgCellReq.Rows[idx].DataItem, "CELL_SPLY_REQ_ID"));
                        _dReqQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgCellReq.Rows[idx].DataItem, "REQ_QTY"));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        #region 요청동 콤보 조회
        private void SetMboReqArea1()
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
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                mboReqArea1.ItemsSource = DataTableConverter.Convert(dtResult);
                //mboReqArea1.CheckAll();

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                    {
                        mboReqArea1.Check(i);
                    }
                }
                mboReqArea1.IsEnabled = false;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void SetMboReqAreaHis()
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
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboReqAreaHis.ItemsSource = DataTableConverter.Convert(dtResult);
                mboReqAreaHis.CheckAll();

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                //    {
                //        mboReqAreaHis.Check(i);
                //    }
                //}
            }
            catch (Exception ex)
            {
                bClick = false;
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region 공급동 콤보 조회
        private void SetMboSplyArea1()
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
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.ASSY;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboSplyArea1.ItemsSource = DataTableConverter.Convert(dtResult);

                mboSplyArea1.CheckAll();
                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                //    {
                //        mboSplyArea1.Check(i);
                //    }
                //}
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void SetMboSplyAreaHis()
        {

            try
            {
                string sSelectedValue = mboSplyAreaHis.SelectedItemsToString;
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

                mboSplyAreaHis.ItemsSource = DataTableConverter.Convert(dtResult);

                mboSplyAreaHis.CheckAll();

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                //    {
                //        mboSplyAreaHis.Check(i);
                //    }
                //}
            }
            catch (Exception ex)
            {
                bClick = false;
                //Util.MessageException(ex);
            }
        }

        #endregion

        #region  Cell제품ID 콤보 조회
        private void SetMboProd1()
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

                mboCellProdId1.ItemsSource = DataTableConverter.Convert(dtResult);
                mboCellProdId1.CheckAll();
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void SetMboProdHis()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("COND", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["CMCDTYPE"] = "CSLY_CELL_PRJT";
                dr["COND"] = 1;

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_CHK_CMMCD_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                mboCellProdIdHis.ItemsSource = DataTableConverter.Convert(dtResult);
                mboCellProdIdHis.CheckAll();
            }
            catch (Exception ex)
            {
                bClick = false;
                //Util.MessageException(ex);
            }
        }


        #endregion

        #region CELL공급 요청 이력 조회
        private void SearchCellReq(string sLotList = "")
        {
            try
            {
                ShowLoadingIndicator();
                Util.gridClear(dgCellReq);

                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.Date.AddDays(+7);
                    HiddenLoadingIndicator();
                    Util.MessageValidation("SFU3567"); // 조회기간은 7일을 초과할 수 없습니다.                    
                    return;
                }

                _sReqID = string.Empty;
                _dReqQty = 0;

                char sp = ':';
                object[] arrCellPjt = mboCellProdId1.SelectedItems.ToArray();
                string[] strCellTemp = null;
                strCellTemp = arrCellPjt.Cast<string>().ToArray();
                string[] strCellProd = new string[1];

                for (int i = 0; i < strCellTemp.Count(); i++)
                {
                    strCellProd[0] = strCellProd[0] + ',' + strCellTemp[i].Split(sp)[1];
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DATE_ST", typeof(string));
                RQSTDT.Columns.Add("DATE_ED", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PACK_AREAID", typeof(string));
                RQSTDT.Columns.Add("ASSY_AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_ST"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DATE_ED"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["PRODID"] = strCellProd[0];
                dr["PACK_AREAID"] = Util.NVC(mboReqArea1.SelectedItemsToString) == "" ? null : mboReqArea1.SelectedItemsToString;
                dr["ASSY_AREAID"] = Util.NVC(mboSplyArea1.SelectedItemsToString) == "" ? null : mboSplyArea1.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_CSLY_HIST_RTD", "RQSTDT", "RSLTDT", RQSTDT, (dsRslt, bizException) =>
                {
                    try
                    {
                        dgShipRcv.LoadedCellPresenter += dgCellReqHist_LoadedCellPresenter;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (dsRslt.Rows.Count == 0 || dsRslt == null)
                        {
                            Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다
                            return;
                        }

                        Util.GridSetData(dgCellReq, dsRslt, FrameOperation, true);
                        Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dgCellReq.Rows.Count - 1));
                        dgCellReq.Columns["CELL_SPLY_REQ_DTTM"].Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                        dgCellReq.Columns["CELL_SPLY_RSPN_DTTM"].Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                        dgCellReq.Columns["ELAPSED_TIME"].Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        _sReqID = string.Empty;
                        _dReqQty = 0;
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

        private void SearchCellReqHist()
        {
            try
            {
                ShowLoadingIndicator();
                Util.gridClear(dgCellReqHist);

                TimeSpan timeSpan = dtpDateToHis.SelectedDateTime.Date - dtpDateFromHis.SelectedDateTime.Date;

                if (timeSpan.Days > 7)
                {
                    dtpDateToHis.SelectedDateTime = dtpDateFromHis.SelectedDateTime.Date.AddDays(+7);
                    HiddenLoadingIndicator();
                    Util.MessageValidation("SFU3567"); // 조회기간은 7일을 초과할 수 없습니다.                    
                    return;
                }

                char sp = ':';
                object[] arrCellPjt = mboCellProdIdHis.SelectedItems.ToArray();
                string[] strCellTemp = null;
                strCellTemp = arrCellPjt.Cast<string>().ToArray();
                string[] strCellProd = new string[1];

                for (int i = 0; i < strCellTemp.Count(); i++)
                {
                    strCellProd[0] = strCellProd[0] + ',' + strCellTemp[i].Split(sp)[1];
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DATE_ST", typeof(string));
                RQSTDT.Columns.Add("DATE_ED", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PACK_AREAID", typeof(string));
                RQSTDT.Columns.Add("ASSY_AREAID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_ST"] = dtpDateFromHis.SelectedDateTime.ToString("yyyyMMdd");
                dr["DATE_ED"] = dtpDateToHis.SelectedDateTime.ToString("yyyyMMdd");
                dr["PRODID"] = strCellProd[0];
                dr["PACK_AREAID"] = Util.NVC(mboReqAreaHis.SelectedItemsToString) == "" ? null : mboReqAreaHis.SelectedItemsToString;
                dr["ASSY_AREAID"] = Util.NVC(mboSplyAreaHis.SelectedItemsToString) == "" ? null : mboSplyAreaHis.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_CSLY_HIST_RTD_HISTORY", "RQSTDT", "RSLTDT", RQSTDT, (dsRslt, bizException) =>
                {
                    try
                    {
                        dgShipRcvHis.LoadedCellPresenter += dgCellReqHist_LoadedCellPresenter;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        if (dsRslt.Rows.Count == 0 || dsRslt == null)
                        {
                            Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다
                            return;
                        }
                        Util.GridSetData(dgCellReqHist, dsRslt, FrameOperation, false);
                        Util.SetTextBlockText_DataGridRowCount(tbWipListCountHis, Util.NVC(dgCellReqHist.Rows.Count - 1));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        dgShipRcvHis.LoadedCellPresenter -= dgCellReqHist_LoadedCellPresenter;
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

                DataTable dsRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPLY_DETL_FLOW_RTD", "RQSTDT", "RSLTDT", RQSTDT);

                if (dsRslt.Rows.Count == 0)
                {
                    Util.Alert("SFU1498");   //데이터가 없습니다.
                    return;
                }

                Util.GridSetData(dgShipRcvHis, dsRslt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
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
