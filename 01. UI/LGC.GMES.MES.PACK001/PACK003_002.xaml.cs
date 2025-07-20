/*************************************************************************************
 Created Date : 2020.09.02
      Creator : 
   Decription : CELL 반송,투입이력관리
--------------------------------------------------------------------------------------
 [Change History]
  Date         Author      CSR         Description...
  2020.09.02  담당자        SI           Initial Created.
  2021.01.15  김길용A      SI           ESWA PACK2 셀동간이동 CELL 반송이력 그리드 수정(2분할 화면)
  2021.01.22  김길용A      SI           CELL반송요청 시 기준설정정보 INDATA 추가
  2021.01.25  김길용A      SI           반송요청 시 INSUSER , UPDUSER 변경
  2021.02.04  정용석        SI           기요청수량 컬럼 삭제, 활동기준컬럼 추가
  2021.03.05  김길용        SI           CELL반송요청현황 진행상태 Multi Box 수정
  2025.03.28  윤주일       SI          MES 2.0 POPUP INPUT 정보를 GETVALUE로 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        string sBeforeUse_flag = null;
        string Req = null;
        DataTable dt = new DataTable();
        DataTable _dr = new DataTable();
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        //string sBefore_possqty = string.Empty;
        //private string sPossqty = string.Empty;
        

        public PACK003_002()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //CommonCombo _combo = new CommonCombo();
        Util _Util = new Util();
        #endregion

        #region Initialize


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm); 
            listAuth.Add(btnReturnCencel);
            listAuth.Add(btnReturnEnd);
            listAuth.Add(btnConfirmReservation);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            //this.ucPersonInfo.Title = "요청자";

            //SetAreaByAreaType();
            InitCombo();

            btnConfirmReservation.Visibility = Visibility.Visible;
            btnConfirm.Visibility = Visibility.Visible;
            btnReturnCencel.Visibility = Visibility.Hidden;
            btnReturnEnd.Visibility = Visibility.Hidden;
            ResnArea.Visibility = Visibility.Collapsed;


            // DatePicker 날짜변경 안됨.
            //날자 초기값 세팅 : 반송요청
            this.dtpDateFrom.ApplyTemplate();
            this.dtpDateTo.ApplyTemplate();
            this.dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-3); //일주일 전 날짜
            this.dtpDateTo.SelectedDateTime = DateTime.Now;//오늘 날짜
            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            //콤보 없는애 사용용


            C1ComboBox cboAREA = new C1ComboBox();
            cboAREA.SelectedValue = LoginInfo.CFG_AREA_ID;
            //동
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cAreaChild= { cboEqsgId };
            _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.NONE, cbChild: cAreaChild,  sFilter: sFilter);
            //this.cboAreaByAreaType.IsEnabled = false;

            C1ComboBox[] cEqsgIdParent = { cboAreaByAreaType };
            //C1ComboBox[] cEqsgIdChild = { cboprodid };
            _combo.SetCombo(cboEqsgId, CommonCombo.ComboStatus.ALL, cbParent: cEqsgIdParent, sCase: "LOGIS_EQSG_FOR_MEB");

            //C1ComboBox[] cboProdIdParent = { cboEqsgId };
            //string[] strProdIdFilter = { LoginInfo.CFG_SHOP_ID, null };
            //SetMtrl_CBO(this.cboprodid, CommonCombo.ComboStatus.ALL, strProdIdFilter);

            ////제품ID
            //SetMtrl_CBO();

            //종료여부
            //setEndYN();

            //초창기 컨셉인가..?
            //C1ComboBox[] cboEqsgIdHistChild = { cboprod };
            String[] sEqsgIdHistFilter = { LoginInfo.CFG_AREA_ID, null, null };
            C1ComboBox[] cboEqsgIdHistChild = { cboprod };
            _combo.SetCombo(cboEqsgIdHist, CommonCombo.ComboStatus.ALL, cbChild: cboEqsgIdHistChild, sFilter: sEqsgIdHistFilter, sCase: "LOGIS_EQSG_FOR_MEB");

            C1ComboBox[] cboProdParent = { cboEqsgIdHist };
            string[] strProdFilter = { "Y", null };
            _combo.SetCombo(cboprod, CommonCombo.ComboStatus.ALL, cbParent: cboProdParent, sFilter: strProdFilter, sCase: "LOGIS_PROD");

            ////Cell반송요청현황
            ////제품ID
            //String[] sFilter_bas = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, null, null, null, null, "CELL" };
            //_combo.SetCombo(cboprod, CommonCombo.ComboStatus.ALL, sFilter: sFilter_bas, sCase: "PRODUCT");

            //현진행상태 cboReqstat
            //String[] sFilter_stat = { "PACK_LOGIS_TRF_REQ_STAT_CODE", null, "A", null, null, null };
            //_combo.SetCombo(cboReqstat, CommonCombo.ComboStatus.ALL, sFilter: sFilter_stat, sCase: "COMMCODEATTRS");
            this.cboRequestStatus.isAllUsed = true;
            cboRequestStatus.ApplyTemplate();
            this.SetMultiSelectionBoxRequestStatus(this.cboRequestStatus);
        }
        #endregion Initialize

        private void SetMultiSelectionBoxRequestStatus(MultiSelectionBox cboMulti)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_LOGIS_TRF_REQ_STAT_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                
                var query = dtResult.AsEnumerable().Select(x => new
                {
                    CBO_CODE = x.Field<string>("CBO_CODE"),
                    CBO_NAME = x.Field<string>("CBO_NAME"),
                }).Where(x => x.CBO_CODE.Equals("CANCELLED_LOGIS") || 
                                    x.CBO_CODE.Equals("RECEIVED_LOGIS") || 
                                    x.CBO_CODE.Equals("RECEIVING_LOGIS") || 
                                    x.CBO_CODE.Equals("REQUEST_LOGIS") || 
                                    x.CBO_CODE.Equals("COMPLETED_LOGIS") || 
                                    x.CBO_CODE.Equals("CLOSED_LOGIS")
                ).ToList();

                DataTable dtQuery = new DataTable();
                dtQuery.Columns.Add("CBO_CODE", typeof(string));
                dtQuery.Columns.Add("CBO_NAME", typeof(string));

                foreach (var item in query)
                {
                    DataRow drIndata = dtQuery.NewRow();
                    drIndata["CBO_CODE"] = item.CBO_CODE;
                    drIndata["CBO_NAME"] = item.CBO_NAME;
                    dtQuery.Rows.Add(drIndata);
                }
                if (dtQuery.Rows.Count != 0)
                {
                    cboMulti.ItemsSource = DataTableConverter.Convert(dtQuery);
                    for (int i=0; i < dtQuery.Rows.Count; i++)
                    {
                        if ("REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS".Contains(dtQuery.Rows[i]["CBO_CODE"].ToString()))
                        {
                            cboMulti.Check(i);
                        }
                        else
                        {
                            cboMulti.Uncheck(i);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }

                //if (dtResult.Rows.Count != 0)
                //{
                //    if (dtResult.Rows.Count == 1)
                //    {
                //        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                //        cboMulti.Uncheck(-1);
                //    }
                //    else
                //    {
                //        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);

                //        for (int i = 0; i < dtResult.Rows.Count; ++i)
                //        {
                //            if("REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS".Contains(dtResult.Rows[i]["CBO_CODE"].ToString()))
                //            {
                //                cboMulti.Check(i);
                //            }   
                //            else
                //            {
                //                cboMulti.Uncheck(i);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    cboMulti.ItemsSource = null;
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #region Event
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgReturnCell_Hist.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgReturnCell_Hist.EndEdit();
            dgReturnCell_Hist.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgReturnCell_Hist.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dgReturnCell_Hist.EndEdit();
            dgReturnCell_Hist.EndEditRow(true);
        }

        #endregion

        #region Mehod
        //Page 1
        //private void SetPKGCBO()
        //{
        //    string[] filter = { "A", "PKG", ""};
        //    SetEQPT_CBO(cboAssy, CommonCombo.ComboStatus.ALL, filter);
        //}
        //private void SetCOT_A_CBO()
        //{
        //    string[] filter = { "E", "COT", "C" };
        //    SetEQPT_CBO(cboElecAnode, CommonCombo.ComboStatus.ALL, filter);
        //}
        //private void SetCOT_C_CBO()
        //{
        //    string[] filter = { "E", "COT", "A" };
        //    SetEQPT_CBO(cboElecCathode, CommonCombo.ComboStatus.ALL, filter);
        //}
        //Page 2
        //private void SetPKG_H_CBO()
        //{
        //    string[] filter = { "A", "PKG", "" };
        //    SetEQPT_CBO(cboAssyLine, CommonCombo.ComboStatus.ALL, filter);
        //}
        //private void SetCOT_A_H_CBO()
        //{
        //    string[] filter = { "E", "COT", "C" };
        //    SetEQPT_CBO(cboElecA, CommonCombo.ComboStatus.ALL, filter);
        //}
        //private void SetCOT_C_H_CBO()
        //{
        //    string[] filter = { "E", "COT", "A" };
        //    SetEQPT_CBO(cboElecC, CommonCombo.ComboStatus.ALL, filter);
        //}
        private void SetMtrl_CBO()
        {
            if (this.cboEqsgId.SelectedValue == null)
            {
                return;
            }
            string[] filter = { LoginInfo.CFG_SHOP_ID , this.cboAreaByAreaType.SelectedValue.ToString(), this.cboEqsgId.SelectedValue.ToString()};
            SetMtrl_CBO(cboprodid, CommonCombo.ComboStatus.ALL, filter);
        }

        //private void setEndYN()
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("CBO_NAME", typeof(string));
        //        dt.Columns.Add("CBO_CODE", typeof(string));

        //        DataRow dr_ = dt.NewRow();
        //        dr_["CBO_NAME"] = "-ALL-";
        //        dr_["CBO_CODE"] = "ALL";
        //        dt.Rows.Add(dr_);

        //        DataRow dr = dt.NewRow();
        //        dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("Y");
        //        dr["CBO_CODE"] = "Y";
        //        dt.Rows.Add(dr);

        //        DataRow dr1 = dt.NewRow();
        //        dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("N");
        //        dr1["CBO_CODE"] = "N";
        //        dt.Rows.Add(dr1);

        //        dt.AcceptChanges();

        //        cboEndYN.ItemsSource = DataTableConverter.Convert(dt);
        //        cboEndYN.SelectedIndex = 0; //default Y
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        private static DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        private bool Check_Input()
        {
            bool bRetrun = true;
            try
            {
                //if (DateTime.Parse(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"))
                //    > DateTime.Parse(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd")))
                //{
                //    Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                //    bRetrun = false;
                //    dtpDateFrom.Focus();
                //    return bRetrun;
                //}
                //if (DateTime.Parse(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"))
                //    == DateTime.Parse(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd")))
                //{
                //    Util.Alert("9180");  //시작시간과 종료시간이 같습니다.수정해주십시오.
                //    bRetrun = false;
                //    dtpDateFrom.Focus();
                //    return bRetrun;
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRetrun;
        }
        //미사용
        private void SetAreaByAreaType()
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

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dtDr = dt.NewRow();
                    dtDr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                    dtDr["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                    dt.Rows.Add(dtDr);

                    cboAreaByAreaType.ItemsSource = DataTableConverter.Convert(dt);
                    cboAreaByAreaType.SelectedIndex = 0;
                }
                else
                {
                    cboAreaByAreaType.ItemsSource = DataTableConverter.Convert(dtResult);
                    cboAreaByAreaType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //미사용
        private void SetEQPT_CBO(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREA_TYPE_CODE"] = sFilter[0] == "" ? null : sFilter[0];
                dr["EQGRID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["ELTR_TYPE_CODE"] = sFilter[2] == "" ? null : sFilter[2];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_AL_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //제품ID 콤보박스를 위한 처리
        private void SetMtrl_CBO(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LOGIS_FLAG", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sFilter[0] == "" ? null : sFilter[0];
                dr["AREAID"] = sFilter[1] == "" ? null : sFilter[1];
                dr["EQSGID"] = sFilter[2] == "" ? null : sFilter[2];
                dr["LOGIS_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_INPUT_MIX_MTRL_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }

        private void cboAreaByAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 일단 막음.
            //SetMtrl_CBO();
        }
        //색상처리
        private void dgReturnCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "POSSQTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgReturnCell_Hist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("TRF_REQ_STAT_CODE"))
                    {
                        switch(e.Cell.Text.ToUpper())
                        {
                            // REQUEST_LOGIS : 파랑, RECEIVING_LOGIS : 녹색, CLOSED_LOGIS : 회색, CANCELLED_LOGIS : 빨강, COMPLETED_LOGIS : 빨강, RECEIVED_LOGIS : 검정
                            case "REQUEST_LOGIS":
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                //this.dgReturnCell_Hist.GetCell(e.Cell.Row.Index, e.Cell.Column.Index + 1).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                break;
                            case "RECEIVING_LOGIS":
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                //this.dgReturnCell_Hist.GetCell(e.Cell.Row.Index, e.Cell.Column.Index + 1).Presenter.Foreground = new SolidColorBrush(Colors.Green);
                                break;
                            case "CLOSED_LOGIS":
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                //this.dgReturnCell_Hist.GetCell(e.Cell.Row.Index, e.Cell.Column.Index + 1).Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                                break;
                            case "CANCELLED_LOGIS":
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                //this.dgReturnCell_Hist.GetCell(e.Cell.Row.Index, e.Cell.Column.Index + 1).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                break;
                            case "COMPLETED_LOGIS":
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                //this.dgReturnCell_Hist.GetCell(e.Cell.Row.Index, e.Cell.Column.Index + 1).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                break;
                            case "RECEIVED_LOGIS":
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                //this.dgReturnCell_Hist.GetCell(e.Cell.Row.Index, e.Cell.Column.Index + 1).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                break;
                            default:
                                break;
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //반송요청 그리드 체크박스 값처리
        private void dgReturnCellList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

            if (e.Column == this.dgReturnCellList.Columns["CHK"])
            {
                sBeforeUse_flag = Convert.ToString(dgReturnCellList.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;



            if (drv["CHK"].SafeToString() != "True" && e.Column != dgReturnCellList.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgReturnCellList.Columns["CHK"]
                 && e.Column != this.dgReturnCellList.Columns["POSSQTY"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;

                }
            }
        }

        #endregion

        //private void popup_closed(object sender, EventArgs e)
        //{
        //    PACK003_002_POPUP window = sender as PACK003_002_POPUP;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //        Comfrim_ReturnCell(window.sNOTE);
        //    }
        //}

        //private void btnInputset_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        PACK003_002_POPUP popup = new PACK003_002_POPUP();
        //        popup.FrameOperation = this.FrameOperation;

        //        if (popup != null)
        //        {
        //            popup.ShowModal();
        //            popup.CenterOnScreen();

        //            //popup.Closed += new EventHandler();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        //반송요청 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Check_Input())
                {
                    _dr.Clear();
                    Util.gridClear(this.dgReturnPrdList);
                    Util.gridClear(this.dgReturnCellList);
                    getInputHist();
                    getReturnCellList();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getInputHist()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("LOGIS_FLAG", typeof(string));
                


                DataRow dr = INDATA.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboAreaByAreaType) == "" ? null : Util.GetCondition(cboAreaByAreaType);
                dr["LOGIS_FLAG"] = null;

                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_RPT_CELL_DATA_LOGIS_FOR_REQ", "INDATA", "OUTDATA", INDATA, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count > 0)
                    {
                        //dt.Tables.Add(dtResult.Copy());
                        Util.GridSetData(dgReturnPrdList, dtResult, FrameOperation);
                    }
                    else
                    {
                        Util.gridClear(this.dgReturnPrdList);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void getReturnCellList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));



                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboEqsgId) == "" ? null : Util.GetCondition(cboEqsgId);
                dr["MTRLID"] = Util.GetCondition(cboprodid) == "" ? null : Util.GetCondition(cboprodid);

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CELL_LOGIS_STOCK_REQ", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count > 0)
                    {
                        _dr = dtResult.Copy();
                        Util.GridSetData(dgReturnCellList, dtResult, FrameOperation);
                    }
                    else
                    {
                        Util.gridClear(this.dgReturnCellList);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        //반송요청 현황 이동중 cell 수량 더블클릭 시 POPUP 처리 ( 요청중인 상세이력 팝업 ) 
        private void dgReturnCell_Hist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
            C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.GetCellFromPoint(pnt);

            if (crrCell != null)
            {
                if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                {
                    if (Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "TRF_LOT_QTY")) != "0")
                    {
                        DataRowView drv = dgReturnCell_Hist.CurrentRow.DataItem as DataRowView;

                        if (drv != null && drv.Row.ItemArray.Length > 0)
                        {
                            PACK003_002_HISTORY_POPUP popup = new PACK003_002_HISTORY_POPUP();
                            popup.FrameOperation = this.FrameOperation;
                            if (popup != null)
                            {
                                object[] Parameters = new object[11];
                                //Parameters[0] = drv.Row.ItemArray[1];   // 요청번호
                                //Parameters[1] = drv.Row.ItemArray[11];  // 요청 Cell수량
                                //Parameters[2] = drv.Row.ItemArray[12];  // 이동완료 PLT수량
                                //Parameters[3] = drv.Row.ItemArray[13];  // 이동완료 Cell수량
                                //Parameters[4] = drv.Row.ItemArray[14];  // 이동중 PLT수량
                                //Parameters[5] = drv.Row.ItemArray[15];  // 이동중 Cell수량
                                //Parameters[6] = drv.Row.ItemArray[6];  // 조립동
                                //Parameters[7] = drv.Row.ItemArray[29];  // 조립LINE
                                //Parameters[8] = drv.Row.ItemArray[30];  // 전극LINE
                                //Parameters[9] = drv.Row.ItemArray[16];  // 요청자
                                //Parameters[10] = drv.Row.ItemArray[17];  // 요청일시
                                // MES 2.0 POPUP INPUT 정보를 GETVALUE로 변경
                                Parameters[0] = drv.Row.GetValue("TRF_REQ_NO");   // 요청번호
                                Parameters[1] = drv.Row.GetValue("TRF_LOT_QTY");   // 요청 Cell수량
                                Parameters[2] = drv.Row.GetValue("REV_PLT_QTY");   // 이동완료 PLT수량
                                Parameters[3] = drv.Row.GetValue("REV_LOT_QTY");   // 이동완료 Cell수량
                                Parameters[4] = drv.Row.GetValue("MOVE_PLT_QTY");   // 이동중 PLT수량
                                Parameters[5] = drv.Row.GetValue("MOVE_LOT_QTY");   // 이동중 Cell수량
                                Parameters[6] = drv.Row.GetValue("AREA_ASSY");   // 조립동
                                Parameters[7] = drv.Row.GetValue("PKG_EQPT");   // 조립LINE
                                Parameters[8] = drv.Row.GetValue("COT_EQPT");   // 전극LINE
                                Parameters[9] = drv.Row.GetValue("REQ_USER");   // 요청자
                                Parameters[10] = drv.Row.GetValue("REQ_DTTM");  // 요청일시

                                C1WindowExtension.SetParameters(popup, Parameters);
                                popup.ShowModal();
                                popup.CenterOnScreen();
                            }
                        }
                    }
                }
            }
        }
        //체크 시 요청가능 수량 입력 가능상태 처리
        private void dgReturnCell_HistChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                C1DataGrid dg = dgReturnCellList as C1DataGrid;

                if (dgReturnCellList.Rows.Count > 2)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        C1.WPF.DataGrid.DataGridRow row = dgReturnCellList.Rows[i + 2];
                        double sPos = double.Parse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "POSSQTY")));
                        double sdts = double.Parse(dt.Rows[i].ItemArray[22].ToString());
                        if (sPos != sdts)
                        {
                            DataTableConverter.SetValue(row.DataItem, "POSSQTY", sdts);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //private void getRetrunHist()
        //{
        //    
        //    try
        //    {
        //        loadingIndicator.Visibility = Visibility.Visible;
        //        //조회
        //        Util.gridClear(dgReturnCell_Hist);
        //        DataTable dtSearchBot = DataTableConverter.Convert(dgReturnCellList.ItemsSource);
        //
        //        DataTable RQSTDT = new DataTable();
        //
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("PRODID", typeof(string));
        //        RQSTDT.Columns.Add("AREA_ASSY", typeof(string));
        //        RQSTDT.Columns.Add("PKG_EQPTID", typeof(string));
        //        RQSTDT.Columns.Add("COT_EQPTID", typeof(string));
        //
        //
        //        for (int i=0; i < dtSearchBot.Rows.Count; i++)
        //        {
        //            string r = dtSearchBot.Rows[i].ItemArray[0].ToString();
        //            if(r == "True")
        //            {
        //                DataRow drv = dtSearchBot.Rows[i] as DataRow;
        //                DataRow dr = RQSTDT.NewRow();
        //
        //                dr["LANGID"] = LoginInfo.LANGID;
        //                dr["PRODID"] = drv["MTRLID"].ToString();
        //                dr["AREA_ASSY"] = drv["AREA_ASSY"].ToString() == "ALL" ? null : drv["AREA_ASSY"].ToString();
        //                dr["PKG_EQPTID"] = drv["PKG_EQPT"].ToString() == "ALL" ? null : drv["PKG_EQPT"].ToString();
        //                dr["COT_EQPTID"] = drv["COT_EQPT"].ToString() == "ALL" ? null : drv["COT_EQPT"].ToString();
        //                RQSTDT.Rows.Add(dr);
        //
        //            }
        //        }
        //        
        //        
        //        new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_REQ_DATA", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
        //        {
        //            if (ex != null)
        //            {
        //                loadingIndicator.Visibility = Visibility.Collapsed;
        //                Util.MessageException(ex);
        //                return;
        //            }
        //
        //            if (dtResult.Rows.Count != 0)
        //            {
        //                Util.GridSetData(dgReturnCell_Hist, dtResult, FrameOperation);
        //            }
        //
        //            loadingIndicator.Visibility = Visibility.Collapsed;
        //        });
        //
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        Util.MessageException(ex);
        //    }
        //}

        private void dgReturnCell_Hist_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column == this.dgReturnCell_Hist.Columns["CHK"])
            {
                sBeforeUse_flag = Convert.ToString(dgReturnCell_Hist.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgReturnCell_Hist.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgReturnCell_Hist.Columns["CHK"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
        //입력값 숫자체크
        private bool chk_num(string e, uint b)
        {
            bool bRetrun = true;
            //uint numformat = 0;

            bool canConvert = uint.TryParse(e, out b);
            if (canConvert != true)
            {
                Util.Alert("SFU5160"); //입력실패
                bRetrun = false;
                return bRetrun;
            }
            return bRetrun;
        }
        
        private void dgReturnCellList_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            //사용안함
            C1DataGrid dg = sender as C1DataGrid;
            
            if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CHK")) == "True")
            {
                if (dg.CurrentCell.Column.Name == "POSSQTY")
                {
                    //string numstring = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "POSSQTY"));
                    //if (chk_num(numstring, be_confirmQty))
                    //{
                    //    uint sPos = uint.Parse(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "POSSQTY")));
                    //    if (sPos > be_confirmQty)
                    //    {
                    //        //dg.SetValue("POSSQTY", be_confirmQty);
                    //        //Util.Alert("SFU1749"); //요청 수량이 잘못되었습니다.
                    //        //DataTableConverter.SetValue(dg.CurrentRow.DataItem, "POSSQTY", be_confirmQty);
                    //        //return;
                    //    }
                    //    else
                    //    {
                    //        dg.SetValue("POSSQTY", sPos);
                    //        DataTableConverter.SetValue(dgReturnCellList.CurrentCell.Row.DataItem, "POSSQTY", sPos);
                    //        return;
                    //    }
                    //}
                    //else
                    //{
                    //    dg.SetValue("POSSQTY", be_confirmQty);
                    //    DataTableConverter.SetValue(dgReturnCellList.CurrentCell.Row.DataItem, "POSSQTY", be_confirmQty);
                    //    return;
                    //}
                }
            }
        }
        private void CellRtAndEclAndEnd(string btntype, C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (ucPersonInfo.UserID == null)
                {
                    Util.MessageInfo("SFU1843");
                    return;
                }
                DataTable dtSearchResult = DataTableConverter.Convert(dg.ItemsSource);

                //DataTable dtBe = dtSearchResult.Select("CHK = 'true'").CopyToDataTable();
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("INSUSER", typeof(string));
                INDATA.Columns.Add("UPDUSER", typeof(string));
                INDATA.Columns.Add("REQ_TYPE", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("TRF_REQ_TYPE_CODE", typeof(string));

                //INDATA
                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["INSUSER"] = Util.NVC(btntype.ToString()) == "R" ? ucPersonInfo.UserID  : LoginInfo.USERID;
                drINDATA["UPDUSER"] = Util.NVC(btntype.ToString()) == "R" ? LoginInfo.USERID : ucPersonInfo.UserID;
                drINDATA["REQ_TYPE"] = btntype.ToString();
                drINDATA["NOTE"] = Util.NVC(btntype.ToString()) == "R" ? null : txtNote.Text;
                drINDATA["TRF_REQ_TYPE_CODE"] = "REQ_CELL_MOVE";

                INDATA.Rows.Add(drINDATA);

                DataTable INREQ = new DataTable();
                INREQ.TableName = "IN_REQ";
                //INREQ.Columns.Add("TRF_REQ_TYPE_CODE", typeof(string));
                INREQ.Columns.Add("EQSGID", typeof(string));
                INREQ.Columns.Add("MTRLID", typeof(string));
                INREQ.Columns.Add("AREA_ASSY", typeof(string));
                INREQ.Columns.Add("PKG_EQPT", typeof(string));
                INREQ.Columns.Add("COT_EQPT", typeof(string));
                INREQ.Columns.Add("ASSY_STOCK_QTY", typeof(string));
                INREQ.Columns.Add("REQ_POSSIBLITY_QTY", typeof(string));
                INREQ.Columns.Add("WOID", typeof(string));
                INREQ.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));

                DataTable INUPD = new DataTable();
                INUPD.TableName = "IN_UPD";
                INUPD.Columns.Add("TRF_REQ_NO", typeof(string));

                int chk_idx = 0;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    string e = dtSearchResult.Rows[i].ItemArray[0].ToString();
                    if (e == "True")
                    {
                        DataRow drv = dtSearchResult.Rows[i] as DataRow;

                        //INREQ
                        DataRow drINREQ = INREQ.NewRow();
                        drINREQ["EQSGID"] = drv["EQSGID"].ToString();
                        drINREQ["MTRLID"] = drv["MTRLID"].ToString();
                        drINREQ["AREA_ASSY"] = drv["AREA_ASSY"].ToString();
                        drINREQ["PKG_EQPT"] = drv["PKG_EQPT"].ToString();
                        drINREQ["COT_EQPT"] = drv["COT_EQPT"].ToString();
                        drINREQ["ASSY_STOCK_QTY"] = "240000";       //CELL반송요청재고 최대수량지정
                        drINREQ["WOID"] = drv["WOID"].ToString();
                        if (Util.NVC(btntype.ToString()) == "R")
                            drINREQ["INPUT_MIX_CHK_MTHD_CODE"] = Util.NVC(drv["INPUT_MIX_CHK_MTHD_CODE"].ToString()) == "ALL" ? "ALL" : drv["INPUT_MIX_CHK_MTHD_CODE"].ToString();
                        if (Util.NVC(btntype.ToString()) == "R")
                            drINREQ["REQ_POSSIBLITY_QTY"] = Util.NVC(drv["POSSQTY"].ToString()) == "" ? null : drv["POSSQTY"].ToString();
                        INREQ.Rows.Add(drINREQ);

                        //INUPD
                        if (btntype.ToString() != "R")
                        {
                            DataRow drINUPD = INUPD.NewRow();
                            drINUPD["TRF_REQ_NO"] = Util.NVC(drv["TRF_REQ_NO"].ToString()) == "" ? null : drv["TRF_REQ_NO"].ToString();
                            INUPD.Rows.Add(drINUPD);
                        }

                        chk_idx++;

                    }
                }
                if (chk_idx == 0)
                {
                    return;
                }
                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(INREQ);
                dsInput.Tables.Add(INUPD);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOGIS_MANUAL_REQ", "INDATA,IN_REQ,IN_UPD", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {

                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            if (btntype == "C" || btntype == "Y")
                            {
                                btnSearchReturn_Click(null, null);
                                txtNote.Text = "";
                                txtConfNum.Text = "";
                            }
                            else
                            {
                                btnSearch_Click(null, null);
                            }

                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (dsResult.Tables["OUTDATA"].Rows.Count == 0)
                            {
                                if (btntype == "C" || btntype == "Y")
                                {
                                    btnSearchReturn_Click(null, null);
                                    txtNote.Text = "";
                                    txtConfNum.Text = "";
                                }
                                else
                                {
                                    btnSearch_Click(null, null);
                                }
                                ms.AlertInfo("SFU1275"); //정상처리되었습니다.
                                this.ucPersonInfo.Clear();
                                return;
                            }

                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
                {
                    Util.Alert("SFU4591"); //작업자를 입력하세요
                    this.ucPersonInfo.Focus();
                    return;
                }
               
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                dg = dgReturnCellList;
                string Req = "R";
                DataTable dt = DataTableConverter.Convert(dgReturnCellList.ItemsSource);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("CHK = 'true'").Length > 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU5161"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>

                        {
                            if (sResult == MessageBoxResult.OK)
                            {
                                CellRtAndEclAndEnd(Req, dg);
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (chk_Search())
                {
                    getSearch();
                }
            }
            catch
            {
            }
        }
        private bool chk_Search()
        {
            bool bRetrun = true;

            if (DateTime.Parse(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd")))
            {
                Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                bRetrun = false;
                dtpDateFrom.Focus();
                return bRetrun;
            }
            //if (txtConfNum.Text == "")
            //{
            //    Util.Alert("1105");  //입력된 ID 데이터가 없습니다.
            //    bRetrun = false;
            //    dtpDateFrom.Focus();
            //    return bRetrun;
            //}
            return bRetrun;
        }

        private void getSearch()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TRF_REQ_NO", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PKG_EQPTID", typeof(string));
                RQSTDT.Columns.Add("COT_EQPTID", typeof(string));
                RQSTDT.Columns.Add("TRF_REQ_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("DATE_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRF_REQ_NO"] = txtConfNum.Text == "" ? null : txtConfNum.Text;
                dr["PRODID"] = null;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEqsgIdHist.SelectedValue.ToString()) ? null : cboEqsgIdHist.SelectedValue.ToString();
                dr["PKG_EQPTID"] = null;
                dr["COT_EQPTID"] = null;
                dr["TRF_REQ_STAT_CODE"] = Convert.ToString(this.cboRequestStatus.SelectedItemsToString) == "" ? null : Convert.ToString(this.cboRequestStatus.SelectedItemsToString);
                dr["DATE_FROM"] = Util.GetCondition(dtpDateFrom);
                dr["DATE_TO"] = Util.GetCondition(dtpDateTo);


                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_REQ_DATA", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgReturnCell_Hist, dtResult, FrameOperation);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        Util.gridClear(dgReturnCell_Hist);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageInfo("SFU3537");
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void btnReturnCencel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                dg = dgReturnCell_Hist;
                string Req = "C";
                DataTable dt = DataTableConverter.Convert(dgReturnCell_Hist.ItemsSource);

                //사용자 수정
                if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
                {
                    Util.Alert("SFU4591"); //작업자를 입력하세요
                    this.ucPersonInfo.Focus();
                    return;
                }

                if (dt.Rows.Count <= 0)
                {
                    return;
                }

                if (dt.Select("CHK = 'true'").Length <= 0)
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                // 반송취소사유 입력 체크
                if (string.IsNullOrEmpty(this.txtNote.Text))
                {
                    Util.MessageValidation("SFU5162");
                    return;
                }

                //현 반송진행 상태가 요청 상태인 것만 취소가능
                var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE") &&
                                                         x.Field<string>("TRF_REQ_STAT_CODE").ToUpper().Equals("REQUEST_LOGIS"));
                if (query.Count() <= 0)
                {
                    Util.MessageValidation("SFU5163");
                    return;
                }

                // 반송요청취소
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("MCS1005"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        CellRtAndEclAndEnd(Req, dg);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReturnEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
                dg = dgReturnCell_Hist;
                string Req = "C";
                DataTable dt = DataTableConverter.Convert(dgReturnCell_Hist.ItemsSource);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("CHK = 'true'").Length > 0)
                    {
                        var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE") &&
                                                        x.Field<string>("TRF_REQ_STAT_CODE").ToUpper().Equals("REQUEST_LOGIS"));
                        var query2 = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE") &&
                                                        x.Field<string>("TRF_REQ_STAT_CODE").ToUpper().Equals("RECEIVING_LOGIS"));
                        var query3 = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE") &&
                                                        x.Field<string>("TRF_REQ_STAT_CODE").ToUpper().Equals("CLOSED_LOGIS"));
                        if (query.Count() <= 0 && query2.Count() <= 0 && query3.Count() <=0)
                        {
                            Util.MessageValidation("선택된 데이터가 없습니다");
                            return;
                        }
                        // 사용자 체크
                        if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
                        {
                            Util.Alert("SFU4591"); //작업자를 입력하세요
                            this.ucPersonInfo.Focus();
                            return;
                        }
                        if (string.IsNullOrEmpty(txtNote.Text) == true)
                        {
                            Util.Alert("SFU2076"); //변경사유는 필수 입력항목입니다.
                            return;
                        }
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU5164"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                                {
                                    if (sResult == MessageBoxResult.OK)
                                    {
                                        CellRtAndEclAndEnd(Req, dg);
                                    }
                                });
                    }
                    else
                    {
                        Util.MessageValidation("선택된 데이터가 없습니다");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgReturnCellList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //int rowIndex = e.Cell.Row.Index;
            //string edittedColumnName = e.Cell.Column.Name;
            //ValidationRequestPossibleQty(rowIndex, edittedColumnName);
        }

        private bool ValidationRequestPossibleQty()
        {
            bool returnValue = true;

            DataTable dt = DataTableConverter.Convert(dgReturnCellList.ItemsSource);
            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));

            foreach (var item in query)
            {
                string mtrlID = item.Field<string>("MTRLID");
                string mtrlidPrjtName = item.Field<string>("MTRLID_PRJT_NAME");
                string areaAssy = item.Field<string>("AREA_ASSY");
                string pkgEqpt = item.Field<string>("PKG_EQPT");
                string cotEqpt = item.Field<string>("COT_EQPT");
                decimal planQty = Convert.ToDecimal(item.Field<string>("PLANQTY"));
                decimal cellAvaQty = Convert.ToDecimal(item.Field<string>("CELL_AVA_QTY"));
                decimal avaQty = Convert.ToDecimal(item.Field<string>("AVAQTY"));
                decimal inputQty = Convert.ToDecimal(item.Field<string>("INPUTQTY"));
                decimal reqQty = Convert.ToDecimal(item.Field<string>("REQQTY"));
                decimal inputPossQty = Convert.ToDecimal(item.Field<string>("POSSQTY"));

                bool interlockFlag = false;
                // 요청가능수량 = PACK계획수량 - (PACK가용수량 + PACK기투입수량 + PACK기요청수량)
                decimal possQty = planQty - (avaQty + inputQty);

                // 입력된 값이 음수이면 interlock
                if (inputPossQty < 0)
                {
                    interlockFlag = true;
                }
                // 입력한 값이 계획수량보다 크다면 interlock
                else if (inputPossQty > planQty)
                {
                    interlockFlag = true;
                }
                else
                {
                    interlockFlag = false;
                }

                if (interlockFlag)
                {
                    Util.Alert("SFU1749"); //요청 수량이 잘못되었습니다.
                    returnValue = false;
                }
            }

            return returnValue;
        }

        private void tabReturnMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (btnConfirm == null || btnReturnCencel == null || btnReturnEnd == null) return;

                if (tabInputHist.IsSelected)
                {
                    btnConfirm.Visibility = Visibility.Hidden;
                    btnConfirmReservation.Visibility = Visibility.Hidden;
                    ResnArea.Visibility = Visibility.Visible;
                    btnReturnCencel.Visibility = Visibility.Hidden;
                    btnReturnEnd.Visibility = Visibility.Visible;
                }

                if (tabReturnCell.IsSelected)
                {
                    btnConfirm.Visibility = Visibility.Visible;
                    btnConfirmReservation.Visibility = Visibility.Visible;
                    ResnArea.Visibility = Visibility.Collapsed;
                    btnReturnCencel.Visibility = Visibility.Hidden;
                    btnReturnEnd.Visibility = Visibility.Hidden;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirmReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK003_002_RESERVATION popup = new PACK003_002_RESERVATION();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = ucPersonInfo.UserID;
                    Parameters[1] = ucPersonInfo.UserName;
                    
                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEqsgId_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMtrl_CBO();
        }

        private void dgReturnCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 기요청수량 -> 현재 STOCKER에 들어와 있는 수량으로 변경 -> 컬럼숨김
            if (DateTime.Now.Equals(DateTime.Now.AddDays(9999)))
            {
                Point point = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1DataGrid = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(point);

                if (dataGridCell != null)
                {
                    if (c1DataGrid.GetRowCount() > 0 && dataGridCell.Row.Index >= 0)
                    {

                        if (dataGridCell.Column.Name.Equals("POSSQTY"))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturnCellList.CurrentRow.DataItem, "CHK")) == "False")
                            {
                                return;
                            }
                            //be_confirmQty = uint.Parse(Util.NVC(DataTableConverter.GetValue(dgReturnCellList.CurrentRow.DataItem, "POSSQTY")));
                        }
                        if (dataGridCell.Column.Name.Equals("REQQTY"))
                        {
                            DataRowView drv = c1DataGrid.CurrentRow.DataItem as DataRowView;

                            if (drv != null && drv.Row.ItemArray.Length > 0)
                            {
                                PACK003_002_REQUEST_HISTORY popup = new PACK003_002_REQUEST_HISTORY();
                                popup.FrameOperation = this.FrameOperation;
                                if (popup != null)
                                {
                                    object[] Parameters = new object[2];
                                    Parameters[0] = drv.Row.ItemArray[3];   // EQSGID
                                    Parameters[1] = drv.Row.ItemArray[1];  //  PRODID
                                    C1WindowExtension.SetParameters(popup, Parameters);
                                    popup.ShowModal();
                                    popup.CenterOnScreen();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void dgReturnCellList_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
