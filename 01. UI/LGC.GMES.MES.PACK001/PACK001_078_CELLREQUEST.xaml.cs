/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_078_CELLREQUEST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        public bool bClick = false;
        string strAreaID = string.Empty;
        string strMtrlID = string.Empty;
        string sToday = string.Empty;
        string strBoxQty = string.Empty;
        private Util _Util = new Util();
        private int iOpenPopup = 0;

        private object lockObject = new object();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_078_CELLREQUEST()
        {
            InitializeComponent();
            
        }

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if(bClick == false)
                {
                    bClick = true;

                    this.RegisterName("redBrush", redBrush);


                    #region 권한별 보여주기
                    List<Button> listAuth = new List<Button>();
                    listAuth.Add(btnReq);
                    listAuth.Add(btnReqClose);
                    listAuth.Add(btnReqCancel);
                    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                    #endregion

                    object[] tmps = C1WindowExtension.GetParameters(this);
                    if (tmps != null)
                    {
                        strAreaID = Util.NVC(tmps[0]);
                        strMtrlID = Util.NVC(tmps[1]);
                        sToday = Util.NVC(tmps[2]);

                        setCellRequest(strAreaID, strMtrlID, sToday);
                        setCellConf(strAreaID, strMtrlID);
                        //this.Loaded -= new System.Windows.RoutedEventHandler(this.C1Window_Loaded);
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region Button

        #region 요청버튼
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                reqSply();
                Refresh();

                setCellRequest(strAreaID, strMtrlID, sToday);
                setCellConf(strAreaID, strMtrlID);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 마감클릭
        private void btnReqClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgCellConf, "CONF_CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                PACK001_078_CLOSE_REASON popup = new PACK001_078_CLOSE_REASON();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= ClosePopup_Closed;
                    popup.Closed += ClosePopup_Closed;

                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.BringToFront();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }
        #endregion

        #region 취소클릭
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_078_CANCEL_REASON popup = new PACK001_078_CANCEL_REASON();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= CancelPopup_Closed;
                    popup.Closed += CancelPopup_Closed;

                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bClick = false;

                if (bClick == false)
                {
                    bClick = true;
                    if(bClick == true)
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                //HiddenLoadingIndicator();

                bClick = false;
            }
        }
        #endregion

        #endregion

        #region 로딩 인디케이터 보여주기
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region 로딩 인디케이터 숨기기
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region 취소 팝업 닫기
        void CancelPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_078_CANCEL_REASON popup = sender as PACK001_078_CANCEL_REASON;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    string sCancelReason = string.Empty;
                    sCancelReason = Util.NVC(((System.Windows.Documents.TextRange)popup.DataContext).Text.Trim());

                    reqCancel(sCancelReason);
                    Refresh();
                    setCellRequest(strAreaID, strMtrlID, sToday);
                    setCellConf(strAreaID, strMtrlID);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 마감 팝업 닫기
        void ClosePopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_078_CLOSE_REASON popup = sender as PACK001_078_CLOSE_REASON;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    string sCloseReason = string.Empty;
                    sCloseReason = Util.NVC((popup.DataContext).ToString().Trim());


                    
                    reqClose(sCloseReason);
                    Refresh();               
                    setCellRequest(strAreaID, strMtrlID, sToday);
                    setCellConf(strAreaID, strMtrlID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region 팝업 상단(공급 요청) 조회
        private void setCellRequest(string AreaId, string MtrlId, string Today)
        {
            try
            {
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("MTRLID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("OPERATOR", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("TODAY", typeof(string));

                DataRow dr = indata.NewRow();
                dr["MTRLID"] = MtrlId;
                dr["AREAID"] = AreaId;
                dr["OPERATOR"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TODAY"] = Today;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_PACK_CELL_REQ", "INDATA", "RSLTDT", (dsRslt, bizException) =>
                {
                    try
                    {
                        dgCellReq.LoadedCellPresenter += dgCellReq_LoadedCellPresenter;

                        if (bizException != null)
                        {
                            bClick = false;
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            for (int i = 0; i < dsRslt.Tables["RSLTDT"].Rows.Count; i++)
                            {
                                if (dsRslt.Tables["RSLTDT"].Rows[i]["RESULT"].ToString() != "OK")
                                {
                                    Util.Alert("SFU8380");//해당 제품에 대한 박스 전체 셀 수량 기준 정보가 누락되어 있습니다.
                                }
                                
                            }

                                Util.GridSetData(dgCellReq, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        bClick = false;
                        Util.MessageException(ex);
                    }

                    bClick = false;
                    dgCellReq.LoadedCellPresenter -= dgCellReq_LoadedCellPresenter;

                }, ds);

                bClick = false;
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 팝업 하단(요청 상세) 조회
        private void setCellConf(string AreaId, string MtrlId)
        {
            try
            {
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("MTRLID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));

                DataRow dr = indata.NewRow();
                dr["MTRLID"] = MtrlId;
                dr["AREAID"] = AreaId;
                dr["LANGID"] = LoginInfo.LANGID;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_PACK_CELL_REQ_DETAIL", "INDATA", "RSLTDT", (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            bClick = false;
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgCellConf, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        bClick = false;
                        Util.MessageException(ex);
                    }
                    bClick = false;

                }, ds);

                bClick = false;
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Cell 공급 요청
        private void reqSply()
        {
            try
            {
                ShowLoadingIndicator();
                DataRow[] drChk = Util.gridGetChecked(ref dgCellReq, "CHK");
                if (drChk.Length <= 0)
                {
                    bClick = false;
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgCellReq.ItemsSource).ToTable();

                var query = from order in dtReqList.AsEnumerable()
                            where order.Field<int>("CHK") == 1
                            group order by new
                            {
                                AREAID = order.Field<String>("CELLBLDG")
                            } into g

                            select new
                            {
                                AREAID = g.Key.AREAID
                            };

                string sAreaId = dtReqList.Rows[0]["PACKBLDG"].ToString();
                string sProdId = dtReqList.Rows[0]["MTRLID"].ToString();
                string sReqQty = dtReqList.Rows[0]["SUGGEST_PLT"].ToString();
                string sNote = dtReqList.Rows[0]["REMARK"].ToString();
                string sInsUser = dtReqList.Rows[0]["OPERATOR"].ToString();
                string sUpdUser = dtReqList.Rows[0]["OPERATOR"].ToString();
                decimal dPltCellQty = dtReqList.Rows[0]["BOX_QTY"].GetDecimal();
                int i = 0;

                if (sReqQty.IsNullOrEmpty())
                {
                    bClick = false;
                    Util.Alert("SFU1154");//수량을 입력하세요
                    return;
                }
                if (!int.TryParse(sReqQty, out i))
                {
                    bClick = false;
                    Util.Alert("SFU3435");//숫자만 입력해주세요
                    return;
                }

                DataSet ds = new DataSet();
                DataTable indata1 = ds.Tables.Add("IN_REQ");

                indata1.Columns.Add("CELL_SPLY_STAT_CODE", typeof(string));
                indata1.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata1.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata1.Columns.Add("AREAID", typeof(string));
                indata1.Columns.Add("PRODID", typeof(string));
                indata1.Columns.Add("REQ_QTY", typeof(string));
                indata1.Columns.Add("NOTE", typeof(string));
                indata1.Columns.Add("INSUSER", typeof(string));
                indata1.Columns.Add("UPDUSER", typeof(string));
                indata1.Columns.Add("PLLT_UNIT_CELL_REQ_QTY", typeof(decimal));

                DataRow dr1 = indata1.NewRow();
                dr1["CELL_SPLY_STAT_CODE"] = "REQUEST";
                dr1["CELL_SPLY_TYPE_CODE"] = "MES";
                dr1["AUTO_LOGIS_FLAG"] = "N";
                dr1["AREAID"] = sAreaId;
                dr1["PRODID"] = sProdId;
                dr1["REQ_QTY"] = sReqQty;
                dr1["NOTE"] = sNote;
                dr1["INSUSER"] = sInsUser;
                dr1["UPDUSER"] = sUpdUser;
                dr1["PLLT_UNIT_CELL_REQ_QTY"] = dPltCellQty;

                ds.Tables["IN_REQ"].Rows.Add(dr1);

                DataTable indata2 = ds.Tables.Add("IN_RSPN_AREA");
                indata2.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));

                foreach (var x in query)
                {
                    DataRow dr2 = indata2.NewRow();
                    dr2["CELL_SPLY_RSPN_AREAID"] = x.AREAID;
                    ds.Tables["IN_RSPN_AREA"].Rows.Add(dr2);
                }


                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CELL_SPLY_REQ", "IN_REQ,IN_RSPN_AREA", null, (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            bClick = false;
                            Util.MessageException(bizException);
                            return;
                        }else
                        {
                            Util.MessageInfo("SFU8381");//공급 요청이 완료 됐습니다.
                        }
                    }
                    catch(Exception ex)
                    {
                        bClick = false;
                        Util.MessageException(ex);
                    }

                    bClick = false;

                }, ds);

                HiddenLoadingIndicator();

            }
            catch (Exception ex)
            {
                bClick = false;
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        //취소 기능 사용 안함
        #region 요청 취소
        private void reqCancel(string sCancelReason)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgCellConf, "CONF_CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtCloseList = ((DataView)dgCellConf.ItemsSource).ToTable();

                var query = from order in dtCloseList.AsEnumerable()
                            where order.Field<int>("CONF_CHK") == 1
                            group order by new
                            {
                                REQID = order.Field<String>("CONF_REQNO"),
                                MTRLID = order.Field<String>("CONF_CELLID"),
                                PACKAREAID = order.Field<String>("CONF_PACKBLDG"),
                                //CELLAREAID = order.Field<String>("CONF_CELLBLDG")
                            } into g

                            select new
                            {
                                REQID = g.Key.REQID,
                                MTRLID = g.Key.MTRLID,
                                PACKAREAID = g.Key.PACKAREAID,
                                // CELLAREAID = g.Key.CELLAREAID
                            };

                //INDATA DATATABLE 생성
                string sAreaId = dtCloseList.Rows[0]["CONF_PACKBLDG"].ToString();
                string sProdId = dtCloseList.Rows[0]["CONF_CELLID"].ToString();
                string sReqQty = dtCloseList.Rows[0]["CONF_REQPLTQTY"].ToString();
                string sNote = sCancelReason;
                string sInsUser = dtCloseList.Rows[0]["CONF_OPERATOR"].ToString();
                string sUpdUser = dtCloseList.Rows[0]["CONF_OPERATOR"].ToString();

                if (sReqQty.IsNullOrEmpty())
                {
                    return;
                }

                DataSet ds = new DataSet();
                DataTable indata1 = ds.Tables.Add("INDATA");

                indata1.Columns.Add("SPLY_STAT_CODE", typeof(string));
                indata1.Columns.Add("REQ_QTY", typeof(string));
                indata1.Columns.Add("NOTE", typeof(string));
                indata1.Columns.Add("INSUSER", typeof(string));
                indata1.Columns.Add("UPDUSER", typeof(string));

                DataRow dr1 = indata1.NewRow();
                dr1["SPLY_STAT_CODE"] = "CANCEL_REQUEST";
                dr1["REQ_QTY"] = sReqQty;
                dr1["NOTE"] = sNote;
                dr1["INSUSER"] = LoginInfo.USERID;
                dr1["UPDUSER"] = LoginInfo.USERID;

                ds.Tables["INDATA"].Rows.Add(dr1);

                //IN_REQ DATATABLE 생성
                DataTable indata2 = ds.Tables.Add("IN_REQ");
                indata2.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                indata2.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata2.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata2.Columns.Add("AREAID", typeof(string));
                indata2.Columns.Add("PRODID", typeof(string));

                foreach (var x in query)
                {
                    DataRow dr2 = indata2.NewRow();
                    dr2["CELL_SPLY_REQ_ID"] = x.REQID;
                    dr2["CELL_SPLY_TYPE_CODE"] = "MES";
                    dr2["AUTO_LOGIS_FLAG"] = "N";
                    dr2["AREAID"] = x.PACKAREAID;
                    dr2["PRODID"] = x.MTRLID;
                    ds.Tables["IN_REQ"].Rows.Add(dr2);
                }


                var query2 = from order in dtCloseList.AsEnumerable()
                            where order.Field<int>("CONF_CHK") == 1
                            group order by new
                            {
                                RSPNID = order.Field<String>("CONF_RSPNID"),
                                REQID = order.Field<String>("CONF_REQNO"),
                                MTRLID = order.Field<String>("CONF_CELLID"),
                                PACKAREAID = order.Field<String>("CONF_PACKBLDG"),
                                CELLAREAID = order.Field<String>("CONF_CELLBLDG")
                            } into g

                            select new
                            {
                                RSPNID = g.Key.RSPNID,
                                REQID = g.Key.REQID,
                                MTRLID = g.Key.MTRLID,
                                PACKAREAID = g.Key.PACKAREAID,
                                CELLAREAID = g.Key.CELLAREAID
                            };

                DataTable indata3 = ds.Tables.Add("IN_RSPN");
                indata3.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
                indata3.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                indata3.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));
                indata3.Columns.Add("CELL_SPLY_RSPN_STAT_CODE", typeof(string));
                indata3.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata3.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata3.Columns.Add("PRODID", typeof(string));

                foreach (var x in query2)
                {
                    DataRow dr3 = indata3.NewRow();
                    dr3["CELL_SPLY_RSPN_ID"] = x.RSPNID;
                    dr3["CELL_SPLY_REQ_ID"] = x.REQID;
                    dr3["CELL_SPLY_RSPN_AREAID"] = x.CELLAREAID;
                    dr3["CELL_SPLY_RSPN_STAT_CODE"] = "CANCEL_REQUEST";
                    dr3["CELL_SPLY_TYPE_CODE"] = "MES";
                    dr3["AUTO_LOGIS_FLAG"] = "N";
                    dr3["PRODID"] = x.MTRLID;
                    ds.Tables["IN_RSPN"].Rows.Add(dr3);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_CELL_SPLY_REQ_STATUS", "INDATA,IN_REQ,IN_RSPN", "OUTDATA", (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }else
                        {
                            ms.AlertWarning("SFU1937");//취소되었습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                    

                }, ds);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 요청 마감
        private void reqClose(string sCloseReason)
        {
            try
            {
                ShowLoadingIndicator();

                DataRow[] drChk = Util.gridGetChecked(ref dgCellConf, "CONF_CHK");
                if (drChk.Length <= 0)
                {
                    bClick = false;
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtCloseList = ((DataView)dgCellConf.ItemsSource).ToTable();

                var query = from order in dtCloseList.AsEnumerable()
                            where order.Field<int>("CONF_CHK") == 1
                            group order by new
                            {
                                REQID = order.Field<String>("CELL_SPLY_REQ_ID"),
                                MTRLID = order.Field<String>("PRODID"),
                                PACKAREAID = order.Field<String>("AREAID"),
                                //CELLAREAID = order.Field<String>("CONF_CELLBLDG")
                            } into g

                            select new
                            {
                                REQID = g.Key.REQID,
                                MTRLID = g.Key.MTRLID,
                                PACKAREAID = g.Key.PACKAREAID
                                // CELLAREAID = g.Key.CELLAREAID
                            };

                //INDATA DATATABLE 생성
                DataSet ds = new DataSet();
                DataTable indata1 = ds.Tables.Add("INDATA");

                indata1.Columns.Add("SPLY_STAT_CODE", typeof(string));
                indata1.Columns.Add("REQ_QTY", typeof(string));
                indata1.Columns.Add("NOTE", typeof(string));
                indata1.Columns.Add("INSUSER", typeof(string));
                indata1.Columns.Add("UPDUSER", typeof(string));

                DataRow dr1 = indata1.NewRow();
                dr1["SPLY_STAT_CODE"] = "CLOSE";
                dr1["NOTE"] = sCloseReason;
                dr1["INSUSER"] = LoginInfo.USERID;
                dr1["UPDUSER"] = LoginInfo.USERID;

                ds.Tables["INDATA"].Rows.Add(dr1);

                //IN_REQ DATATABLE 생성
                DataTable indata2 = ds.Tables.Add("IN_REQ");
                indata2.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                indata2.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata2.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata2.Columns.Add("AREAID", typeof(string));
                indata2.Columns.Add("PRODID", typeof(string));


                foreach (var x in query)
                {
                    DataRow dr2 = indata2.NewRow();
                    dr2["CELL_SPLY_REQ_ID"] = x.REQID;
                    dr2["CELL_SPLY_TYPE_CODE"] = "MES";
                    dr2["AUTO_LOGIS_FLAG"] = "N";
                    dr2["AREAID"] = x.PACKAREAID;
                    dr2["PRODID"] = x.MTRLID;
                    ds.Tables["IN_REQ"].Rows.Add(dr2);
                }


                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_CELL_SPLY_REQ_STATUS", "INDATA,IN_REQ", "OUTDATA", (dsRslt, bizException) =>
                {
                    try
                    {

                        if (bizException != null)
                        {
                            bClick = false;
                            Util.MessageException(bizException);
                            return;
                        }else
                        {
                            Util.MessageInfo("SFU8382");//선택하신 요청서가 마감되었습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        bClick = false;
                        Util.MessageException(ex);
                    }

                    bClick = false;

                }, ds);

                

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgCellReq);
                Util.gridClear(dgCellConf);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Refresh

        #endregion

        #region Grid
        private void dgCellReq_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (!(e.Cell.Row.Index > 1)) return;

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name == "BOX_QTY")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        private void setGridInit(string strProID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = strProID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO_CSLY_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dgCellReq.Rows.Count > 0)
                {
                    (dgCellReq.Columns["BOX_QTY"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }

        private void dgCellReq_LostFocus(object sender, RoutedEventArgs e)
        {
            DataTable dt = sender as DataTable;

            if (dgCellReq.CurrentRow == null)
            {
                return;
            }

            string sColName = dgCellReq.CurrentColumn.Name;
            int iSuggetQty;

            try
            {
                int indexRow = dgCellReq.CurrentRow.Index;
                int indexColumn = dgCellReq.CurrentColumn.Index;

                if (sColName.Equals("BOX_QTY") || sColName.Equals("SUGGEST_PLT"))
                {
                    if (dgCellReq.GetCell(indexRow, indexColumn).Value == null) return;

                    Regex regex = new System.Text.RegularExpressions.Regex(@"[^0-9]");
                    Boolean ismatch = regex.IsMatch(int.Parse(dgCellReq.GetCell(indexRow, indexColumn).Value.ToString()).ToString());

                    if (!ismatch == false) return;

                    iSuggetQty = Convert.ToInt32(DataTableConverter.GetValue(dgCellReq.Rows[indexRow].DataItem, "SUGGEST_PLT"));

                    if (iSuggetQty == 0 || iSuggetQty.Equals("")) return;

                    int iBoxQty = Convert.ToInt32(DataTableConverter.GetValue(dgCellReq.Rows[indexRow].DataItem, "BOX_QTY"));

                    if (iBoxQty == 0 || iBoxQty.Equals("")) return;


                    DataTableConverter.SetValue(dgCellReq.Rows[dgCellReq.CurrentRow.Index].DataItem, "SUGGEST_QTY", iBoxQty * iSuggetQty);

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellReq_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null) return;

                lock (lockObject)
                {

                    Point pnt = e.GetPosition(null);
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                    if (cell != null && cell.Column.Name.ToString() == "BOX_QTY")
                    {
                        PACK001_078_SELCELLQTY popup = new PACK001_078_SELCELLQTY();
                        popup.FrameOperation = this.FrameOperation;



                        string strParam = string.Empty;
                        strParam = Util.NVC(DataTableConverter.GetValue(dgCellReq.Rows[cell.Row.Index].DataItem, "MTRLID"));

                        if (popup != null)
                        {
                            DataTable dtProdCond = new DataTable();
                            dtProdCond = DataTableConverter.Convert(dgCellReq.ItemsSource);

                            object[] Parameters = new object[1];
                            Parameters[0] = strParam;
                            C1WindowExtension.SetParameters(popup, Parameters);

                            popup.Closed -= popup_Closed;
                            popup.Closed += popup_Closed;

                            popup.ShowModal();
                            popup.CenterOnScreen();
                            popup.BringToFront();


                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 팝업 닫기
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_078_SELCELLQTY popup = sender as PACK001_078_SELCELLQTY;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    DataTable tempDt = DataTableConverter.Convert(dgCellReq.ItemsSource);
                    foreach(DataRow dr in tempDt.Rows)
                    {
                        if(dr["MTRLID"].ToString() == strMtrlID)
                        {
                            dr["BOX_QTY"] = popup.BOXQTY.ToString();
                        }
                    }
                    Util.GridSetData(dgCellReq, tempDt, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                bClick = false;
                iOpenPopup = 0;
            }
        }
        #endregion

        private void dgCellReq_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (bClick == false)
                {
                    if (iOpenPopup != 0) return;

                        
                    C1DataGrid dg = sender as C1DataGrid;

                    if (dg == null) return;

                    Point pnt = e.GetPosition(null);

                        

                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                    if (cell != null && cell.Column.Name.ToString() == "BOX_QTY" && cell.Row.Index > 1)
                    {

                        PACK001_078_SELCELLQTY popup = new PACK001_078_SELCELLQTY();
                        popup.FrameOperation = this.FrameOperation;
                        bClick = true;

                        string strParam = string.Empty;
                        strParam = Util.NVC(DataTableConverter.GetValue(dgCellReq.Rows[cell.Row.Index].DataItem, "MTRLID"));

                        if (popup != null)
                        {
                            DataTable dtProdCond = new DataTable();
                            dtProdCond = DataTableConverter.Convert(dgCellReq.ItemsSource);


                            object[] Parameters = new object[1];
                            Parameters[0] = strParam;
                            C1WindowExtension.SetParameters(popup, Parameters);

                            popup.Closed -= popup_Closed;
                            popup.Closed += popup_Closed;

                            popup.ShowModal();
                            popup.CenterOnScreen();
                            popup.BringToFront();
                            iOpenPopup = 1;

                        }
                    }
                    // 공급요청동 Single 선택 
                    if (cell != null && cell.Column.Name.ToString() == "CHK" && cell.Row.Index > 1)
                    {
                        int _rowIndex = dgCellReq.CurrentRow.Index;

                        for (int i = 0; i < dgCellReq.Rows.Count; i++)
                        {
                            if (i != _rowIndex)
                            {
                                DataTableConverter.SetValue(dgCellReq.Rows[i].DataItem, "CHK", false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                bClick = false;
            }

        }


    }
}
