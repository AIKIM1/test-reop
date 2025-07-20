/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 조립 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_079_REQUESTCONF : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        public bool bClick = false;
        string sPackAreaId = string.Empty;
        string sCellPrjt = string.Empty;
        string sCellAreaId = string.Empty;
        string sMtrlId = string.Empty;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_079_REQUESTCONF()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RegisterName("redBrush", redBrush);

                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnDisclaim);
                listAuth.Add(btnConf);
                listAuth.Add(btnCancel);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    this.dgCellRspn.Columns["PACK_MTRLID"].Visibility = Visibility.Visible;
                    this.dgCellRspn.Columns["ASSY_MTRLID"].Visibility = Visibility.Collapsed;
                    this.dgRespDetail.Columns["CONF_PACK_MTRLID"].Visibility = Visibility.Visible;
                    this.dgRespDetail.Columns["CONF_ASSY_MTRLID"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.dgCellRspn.Columns["PACK_MTRLID"].Visibility = Visibility.Collapsed;
                    this.dgCellRspn.Columns["ASSY_MTRLID"].Visibility = Visibility.Visible;
                    this.dgRespDetail.Columns["CONF_PACK_MTRLID"].Visibility = Visibility.Collapsed;
                    this.dgRespDetail.Columns["CONF_ASSY_MTRLID"].Visibility = Visibility.Visible;
                }

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    sPackAreaId = Util.NVC(tmps[0]);
                    sCellPrjt = Util.NVC(tmps[1]);
                    sCellAreaId = Util.NVC(tmps[2]);
                    sMtrlId = Util.NVC(tmps[3]);

                    setCellRequest(sPackAreaId, sCellPrjt,sCellAreaId, sMtrlId);
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

        #region 닫기 클릭
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bClick = false;

                if (bClick == false)
                {
                    bClick = true;
                    if (bClick == true)
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

        #region 공급 승인 클릭
        private void btnRspn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgCellRspn, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgCellRspn.ItemsSource).ToTable();

                int j = 0;

                for (int i = 0; i < dtReqList.Rows.Count; i++)
                {
                    if (dtReqList.Rows[i]["CHK"].ToString() == "1")
                    {
                        if (string.IsNullOrEmpty(dtReqList.Rows[i]["RESP_PLT_QTY"].ToString().Trim()))
                        {
                            Util.Alert("SFU1154");//수량을 입력하세요
                            return;
                        }
                        if (!int.TryParse(dtReqList.Rows[i]["RESP_PLT_QTY"].ToString(), out j))
                        {
                            Util.Alert("SFU3435");//숫자만 입력해주세요
                            return;
                        }
                        if (Convert.ToInt32(dtReqList.Rows[i]["RESP_PLT_QTY"]) < 0)
                        {
                            Util.Alert("SFU4209");//숫자만 입력해주세요
                            return;
                        }
                    }
                }

                rspnSply();
                Refresh();
                setCellRequest(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }
        #endregion

        #region 취소 클릭
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgRespDetail, "CONF_CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgRespDetail.ItemsSource).ToTable();

                var query = from order in dtReqList.AsEnumerable()
                            where order.Field<Int32>("CONF_CHK") == 1
                            group order by new
                            {
                                RSPNQTY = order.Field<String>("CONF_RSPNQTY")
                            } into g

                            select new
                            {
                                RSPNQTY = g.Key.RSPNQTY
                            };

                foreach (var x in query)
                {
                    if (Convert.ToDouble(x.RSPNQTY.ToString()) == 0)
                    {
                        Util.Alert("SFU8370");//현재 응답한 수량이 0개 입니다. 응답 여부를 확인해주세요
                        return;
                    }

                }


                PACK001_079_CANCEL popup = new PACK001_079_CANCEL();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= cancelPopup_Closed;
                    popup.Closed += cancelPopup_Closed;

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

        #region 공급 포기 클릭
        private void btnDisclaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgCellRspn, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgCellRspn.ItemsSource).ToTable();

                PACK001_079_REJECT popup = new PACK001_079_REJECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[0];
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;

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

        #endregion

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

        #endregion

        #region Mehod

        #region 팝업상단(공급승인) 조회
        private void setCellRequest(string sPackAreaId, string sCellPrjt, string sCellAreaId, string sMtrlId)
        {
            try
            {
                #region 임시 데이터
                //DataTable inTable = new DataTable();
                ////테이블에, 컬럼 선언
                //inTable.Columns.Add("REQDATE", typeof(string));
                //inTable.Columns.Add("PACKBLDG", typeof(string));
                //inTable.Columns.Add("CELLID", typeof(string));
                //inTable.Columns.Add("CELL_PJT", typeof(string));
                //inTable.Columns.Add("PLTQTY", typeof(string));
                //inTable.Columns.Add("CELLQTY", typeof(string));
                //inTable.Columns.Add("OPERATOR", typeof(string));
                //inTable.Columns.Add("REMARK", typeof(string));
                //inTable.Columns.Add("AVAIL_FB_PLT_QTY", typeof(string));
                //inTable.Columns.Add("CHK", typeof(string));
                //inTable.Columns.Add("CELL_BLDG", typeof(string));
                //inTable.Columns.Add("RESP_PLT_QTY", typeof(Int64));
                //inTable.Columns.Add("RESP_CELL_QTY", typeof(Int64));
                //inTable.Columns.Add("CELL_AVAIL_STOCK", typeof(string));
                //inTable.Columns.Add("CELL_QA", typeof(string));
                //inTable.Columns.Add("CELL_HOLD", typeof(string));

                //inTable.Rows.Add("2021-03-25 14:00:00", "P6", "ACEN1078I-A1-C04", "E78-C04", "20PLT", "3,600", "김민석", "NONE", "34PLT", "false", "C1", 20, 1800, "2,700", "3,000", "100");
                ////inTable.Rows.Add("2021-03-25 14:00:00", "P6", "ACEN1078I-A1-C04", "20PLT", "3,600", "김민석", "NONE", "34PLT", "false", "A7", 0, 0, "500", "20", "10");

                //Util.GridSetData(dgCellReq, inTable, FrameOperation);
                #endregion
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("PACK_AREAID", typeof(string));
                indata.Columns.Add("MTRLID", typeof(string));
                indata.Columns.Add("CELL_PJT", typeof(string));
                indata.Columns.Add("CELL_AREAID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));

                DataRow dr = indata.NewRow();
                dr["PACK_AREAID"] = sPackAreaId;
                dr["MTRLID"] = sMtrlId;
                dr["CELL_PJT"] = sCellPrjt;
                dr["CELL_AREAID"] = sCellAreaId;
                dr["LANGID"] = LoginInfo.LANGID;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_SPLY_STATUS", "INDATA", "RSLTDT", (dsRslt, bizException) =>
                {
                    try
                    {

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgCellRspn, dsRslt.Tables["RSLTDT"], FrameOperation, false);
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

        #region 팝업하단(공급상세) 조회
        private void setCellConf(string sPackAreaId, string sCellPrjt, string sCellAreaId, string sMtrlId, string sReqId)
        {
            try
            {
                #region 임시 데이터
                //DataTable inTable = new DataTable();
                ////테이블에, 컬럼 선언
                //inTable.Columns.Add("CONF_CHK", typeof(string));
                //inTable.Columns.Add("CONF_REQNO", typeof(string));
                //inTable.Columns.Add("CONF_REQDATE", typeof(string));
                //inTable.Columns.Add("CONF_PACKBLDG", typeof(string));
                //inTable.Columns.Add("CONF_CELLID", typeof(string));
                //inTable.Columns.Add("CONF_CELL_PJT", typeof(string));
                //inTable.Columns.Add("CONF_REQPLTQTY", typeof(string));
                //inTable.Columns.Add("CONF_REFREQCELLQTY", typeof(string));
                //inTable.Columns.Add("CONF_AVAILFBPLTQTY", typeof(string));
                //inTable.Columns.Add("CONF_OPERATOR", typeof(string));
                //inTable.Columns.Add("CONF_REMARK", typeof(string));
                //inTable.Columns.Add("CONF_FBNO", typeof(string));
                //inTable.Columns.Add("CONF_STATE", typeof(string));
                //inTable.Columns.Add("CONF_CELLBLDG", typeof(string));
                //inTable.Columns.Add("CONF_CONFCMPLT", typeof(string));
                //inTable.Columns.Add("CONF_SENDCMPLT", typeof(string));
                //inTable.Columns.Add("CONF_RCVCMPLT", typeof(string));

                //inTable.Rows.Add("false", "20210223-P7-CellA-001", "2021-02-23  9:10:00 AM", "P6", "ACEN1078I-A1-C04", "E78-C04", "34PLT", "6120", "4PLT", "김민석", "NONE", "20210223-P7-CellA-001-C#1", "Shipment", "A6", "10PLT", "10PLT", "10PLT");
                //inTable.Rows.Add("false", "20210223-P7-CellA-001", "2021-02-23  9:10:00 AM", "P6", "ACEN1078I-A1-C04", "E78-C04", "34PLT", "6120", "4PLT", "김민석", "NONE", "20210223-P7-CellA-001-C#2", "Confirm", "A7", "20PLT", "0PLT", "0PLT");

                //Util.GridSetData(dgCellConf, inTable, FrameOperation);
                #endregion
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "INDATA";

                indata.Columns.Add("PACK_AREAID", typeof(string));
                indata.Columns.Add("MTRLID", typeof(string));
                indata.Columns.Add("CELL_PJT", typeof(string));
                indata.Columns.Add("CELL_AREAID", typeof(string));
                indata.Columns.Add("REQ_ID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));

                DataRow dr = indata.NewRow();
                dr["PACK_AREAID"] = sPackAreaId;
                dr["MTRLID"] = sMtrlId;
                dr["CELL_PJT"] = sCellPrjt;
                dr["CELL_AREAID"] = sCellAreaId;
                dr["REQ_ID"] = sReqId;
                dr["LANGID"] = LoginInfo.LANGID;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);
                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_ASSY_CELL_RSPN_DETAIL", "INDATA", "RSLTDT", (dsRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.GridSetData(dgRespDetail, dsRslt.Tables["RSLTDT"], FrameOperation, false);
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

        #region 공급 승인
        private void rspnSply()
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgCellRspn, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgCellRspn.ItemsSource).ToTable();

                var query = from order in dtReqList.AsEnumerable()
                            where order.Field<Int64>("CHK") == 1
                            group order by new
                            {
                                AREAID = order.Field<String>("CELL_AREAID"),
                                REQID = order.Field<String>("REQID"),
                                PRODID = order.Field<String>("MTRLID"),
                                RSPNQTY = order.Field<string>("RESP_PLT_QTY")
                            } into g
                            select new
                            {
                                AREAID = g.Key.AREAID,
                                REQID = g.Key.REQID,
                                PRODID = g.Key.PRODID,
                                RSPNQTY = g.Key.RSPNQTY
                            };

                int j = 0;

                for(int i = 0; i < dtReqList.Rows.Count; i++)
                {
                    if(dtReqList.Rows[i]["CHK"].ToString() == "1")
                    {
                        if (string.IsNullOrEmpty(dtReqList.Rows[i]["RESP_PLT_QTY"].ToString()))
                        {
                            Util.Alert("SFU1154");//수량을 입력하세요
                            return;
                        }
                        if (!int.TryParse(dtReqList.Rows[i]["RESP_PLT_QTY"].ToString(), out j))
                        {
                            Util.Alert("SFU3435");//숫자만 입력해주세요
                            return;
                        }
                        if (Convert.ToInt32(dtReqList.Rows[i]["RESP_PLT_QTY"]) < 0)
                        {
                            Util.Alert("SFU3435");//숫자만 입력해주세요
                            return;
                        }
                    }
                }

                DataSet ds = new DataSet();
                DataTable indata = ds.Tables.Add("IN_RSPN");

                indata.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_STAT_CODE", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_QTY", typeof(string));
                //indaa1.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
                indata.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                indata.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("NOTE", typeof(string));
                indata.Columns.Add("INSUSER", typeof(string));
                indata.Columns.Add("UPDUSER", typeof(string));



                foreach (var x in query)
                {
                    DataRow dr = indata.NewRow();

                    dr["CELL_SPLY_RSPN_AREAID"] = x.AREAID;
                    dr["CELL_SPLY_RSPN_STAT_CODE"] = "RESPOND";
                    dr["CELL_SPLY_RSPN_QTY"] = x.RSPNQTY;
                    dr["CELL_SPLY_REQ_ID"] = x.REQID;
                    dr["CELL_SPLY_TYPE_CODE"] = "MES";
                    dr["AUTO_LOGIS_FLAG"] = "N";
                    dr["PRODID"] = x.PRODID;
                    dr["INSUSER"] = LoginInfo.USERID;
                    dr["UPDUSER"] = LoginInfo.USERID;

                    ds.Tables["IN_RSPN"].Rows.Add(dr);
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_CELL_SPLY_RSPN_STATUS", "IN_RSPN", null, (dsRslt, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }else
                    {
                        //공급승인 RESPOND 확인 처리
                        _setRespond(ds.Tables["IN_RSPN"], "RESPOND");
                        setCellRequest(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId);
                        Util.MessageInfo("SFU8376");//선택하신 요청서의 공급이 승인되었습니다.
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

        #region 공급 포기
        private void rspnDisclaim(string sDisclaimReason)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgCellRspn, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgCellRspn.ItemsSource).ToTable();

                var query = from order in dtReqList.AsEnumerable()
                            where order.Field<Int64>("CHK") == 1
                            group order by new
                            {
                                REQID = order.Field<String>("REQID"),
                                PRODID = order.Field<String>("MTRLID"),
                                PACKAREAID = order.Field<String>("PACK_AREAID"),
                                CELLAREAID = order.Field<String>("CELL_AREAID")
                            } into g

                            select new
                            {
                                REQID = g.Key.REQID,
                                PRODID = g.Key.PRODID,
                                PACKAREAID = g.Key.PACKAREAID,
                                CELLAREAID = g.Key.CELLAREAID
                            };

                //string sAreaId = dtReqList.Rows[0]["CELL_AREAID"].ToString();
                //string sProdId = dtReqList.Rows[0]["MTRLID"].ToString();
                //string sRspnQty = dtReqList.Rows[0]["RESP_PLT_QTY"].ToString();
                //string sReqID = dtReqList.Rows[0]["REQID"].ToString();

                //string sNote = sDisclaimReason;
                //string sInsUser = LoginInfo.USERID;
                //string sUpdUser = LoginInfo.USERID;

                //int i = 0;

                //if (string.IsNullOrEmpty(sRspnQty))
                //{
                //    Util.Alert("SFU1154");//수량을 입력하세요
                //    return;
                //}
                //if (!int.TryParse(sRspnQty, out i))
                //{
                //    Util.Alert("SFU3435");//숫자만 입력해주세요
                //    return;
                //}

                DataSet ds = new DataSet();
                DataTable indata = ds.Tables.Add("IN_RSPN");

                indata.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_STAT_CODE", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_QTY", typeof(string));
                //indaa1.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
                indata.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                indata.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("NOTE", typeof(string));
                indata.Columns.Add("INSUSER", typeof(string));
                indata.Columns.Add("UPDUSER", typeof(string));


                foreach (var x in query)
                {
                    DataRow dr = indata.NewRow();

                    dr["CELL_SPLY_RSPN_AREAID"] = x.CELLAREAID;
                    dr["CELL_SPLY_RSPN_STAT_CODE"] = "WAIVER_RESPOND";
                    dr["CELL_SPLY_REQ_ID"] = x.REQID;
                    dr["CELL_SPLY_TYPE_CODE"] = "MES";
                    dr["AUTO_LOGIS_FLAG"] = "N";
                    dr["PRODID"] = x.PRODID;
                    dr["NOTE"] = sDisclaimReason;
                    dr["INSUSER"] = LoginInfo.USERID;
                    dr["UPDUSER"] = LoginInfo.USERID;

                    ds.Tables["IN_RSPN"].Rows.Add(dr);
                }

                //DataRow dr = indata.NewRow();
                //dr["CELL_SPLY_RSPN_AREAID"] = sAreaId;
                //dr["CELL_SPLY_RSPN_STAT_CODE"] = "WAIVER_RESPOND";
                //dr["CELL_SPLY_RSPN_QTY"] = sRspnQty;
                //dr["CELL_SPLY_REQ_ID"] = sReqID;
                //dr["CELL_SPLY_TYPE_CODE"] = "MES";
                //dr["AUTO_LOGIS_FLAG"] = "N";
                //dr["PRODID"] = sProdId;
                //dr["NOTE"] = sNote;
                //dr["INSUSER"] = sInsUser;
                //dr["UPDUSER"] = sUpdUser;

                //ds.Tables["IN_RSPN"].Rows.Add(dr);
                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_CELL_SPLY_RSPN_STATUS", "IN_RSPN", null, (dsRslt, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }else
                    {
                        //공급포기 RESPOND 확인 처리
                        _setRespond(ds.Tables["IN_RSPN"], "WAIVER_RESPOND");
                        setCellRequest(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId);
                        Util.MessageInfo("SFU8377");//선택하신 요청서의 공급 포기가 완료되었습니다.
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

        #region 공급 취소
        private void rspncCancel(string sCancelReason)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgRespDetail, "CONF_CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                DataTable dtReqList = ((DataView)dgRespDetail.ItemsSource).ToTable();

                var query = from order in dtReqList.AsEnumerable()
                            where order.Field<Int32>("CONF_CHK") == 1
                            group order by new
                            {
                                REQID = order.Field<String>("CONF_REQID"),
                                PRODID = order.Field<String>("CONF_MTRLID"),
                                PACKAREAID = order.Field<String>("CONF_PACK_AREAID"),
                                CELLAREAID = order.Field<String>("CONF_CELL_AREAID"),
                                RSPNID = order.Field<String>("CONF_RSPNID"),
                                RSPNQTY = order.Field<String>("CONF_RSPNQTY")
                            } into g

                            select new
                            {
                                REQID = g.Key.REQID,
                                PRODID = g.Key.PRODID,
                                PACKAREAID = g.Key.PACKAREAID,
                                CELLAREAID = g.Key.CELLAREAID,
                                RSPNID = g.Key.RSPNID,
                                RSPNQTY = g.Key.RSPNQTY
                            };


                DataSet ds = new DataSet();
                DataTable indata = ds.Tables.Add("IN_RSPN");

                indata.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_STAT_CODE", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_QTY", typeof(string));
                indata.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
                indata.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                indata.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                indata.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("NOTE", typeof(string));
                indata.Columns.Add("INSUSER", typeof(string));
                indata.Columns.Add("UPDUSER", typeof(string));


                foreach (var x in query)
                {
                     DataRow dr = indata.NewRow();

                    dr["CELL_SPLY_RSPN_AREAID"] = x.CELLAREAID;
                    dr["CELL_SPLY_RSPN_STAT_CODE"] = "CANCEL_RESPOND";  
                    dr["CELL_SPLY_RSPN_ID"] = x.RSPNID;
                    dr["CELL_SPLY_REQ_ID"] = x.REQID;
                    dr["CELL_SPLY_TYPE_CODE"] = "MES";
                    dr["AUTO_LOGIS_FLAG"] = "N";
                    dr["CELL_SPLY_RSPN_QTY"] = x.RSPNQTY;
                    dr["PRODID"] = x.PRODID;
                    dr["NOTE"] = sCancelReason;
                    dr["INSUSER"] = LoginInfo.USERID;
                    dr["UPDUSER"] = LoginInfo.USERID;


                    if (Convert.ToDouble(x.RSPNQTY.ToString()) == 0)
                    {
                        Util.Alert("SFU8379");//해당 요청서에 대하여 응답된 응답서가 없습니다.
                        return;
                    }

                    ds.Tables["IN_RSPN"].Rows.Add(dr);
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_CELL_SPLY_RSPN_STATUS", "IN_RSPN", null, (dsRslt, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    else
                    {
                        _setCancelRequest(ds.Tables["IN_RSPN"]);
                        setCellRequest(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId);
                        Util.MessageInfo("SFU8378");//선택하신 공급서의 공급 취소가 완료되었습니다.
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

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgCellRspn);
                Util.gridClear(dgRespDetail);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Refresh

        #region 팝업 닫기
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_079_REJECT popup = sender as PACK001_079_REJECT;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    string strChkItem = string.Empty;
                    strChkItem = Util.NVC((popup.DataContext).ToString().Trim());

                    rspnDisclaim(strChkItem);
                   
                    Refresh();
                    setCellRequest(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 취소 팝업 닫기
        void cancelPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_079_CANCEL popup = sender as PACK001_079_CANCEL;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    string sCancelReason = string.Empty;
                    sCancelReason = Util.NVC((popup.DataContext).ToString().Trim());

                    rspncCancel(sCancelReason);

                    Refresh();
                    setCellRequest(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Grid
        private void dgCellRspn_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (!(e.Cell.Row.Index > 1)) return;

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name == "REQID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name == "RESP_PLT_QTY")
                    {
                        string _status = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CELL_SPLY_RSPN_STAT_CODE"));
                        if (!_status.Equals("REQUEST"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gainsboro);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgCellRspn_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
            if (dg.CurrentRow != null)
            {
                if (colName == "RESP_PLT_QTY")
                {
                    string _status = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CELL_SPLY_RSPN_STAT_CODE"));
                    if (!_status.Equals("REQUEST"))
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        e.Cancel = false;
                    }
                }

            }
        }
        #endregion

        private void dgCellRspn_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {

                ShowLoadingIndicator();
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCellRspn.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "REQID" && !(Util.NVC(DataTableConverter.GetValue(dgCellRspn.Rows[cell.Row.Index].DataItem, "REQID")) == null))
                    {
                        string sPackAreaId = Util.NVC(DataTableConverter.GetValue(dgCellRspn.Rows[cell.Row.Index].DataItem, "PACK_AREAID"));
                        string sMtrlId = Util.NVC(DataTableConverter.GetValue(dgCellRspn.Rows[cell.Row.Index].DataItem, "MTRLID"));
                        string sCellPrjt = Util.NVC(DataTableConverter.GetValue(dgCellRspn.Rows[cell.Row.Index].DataItem, "CELL_PRJT"));
                        string sCellAreaId = Util.NVC(DataTableConverter.GetValue(dgCellRspn.Rows[cell.Row.Index].DataItem, "CELL_AREAID"));
                        string sReqId = Util.NVC(DataTableConverter.GetValue(dgCellRspn.Rows[cell.Row.Index].DataItem, "REQID"));

                        setCellConf(sPackAreaId, sCellPrjt, sCellAreaId, sMtrlId, sReqId);
                    }

                }
                HiddenLoadingIndicator();
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }

        private void dgRespDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }
        /// <summary>
        /// 응답동 전체 RESPOND 이면 Request 상태 RESPOND 처리
        /// </summary>
        /// <param name="_dt"></param>
        private void _setRespond(DataTable _dt, string _StatusCode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                RQSTDT.Columns.Add("CELL_SPLY_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));

                foreach (DataRow row in _dt.Rows)
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["CELL_SPLY_REQ_ID"] = row["CELL_SPLY_REQ_ID"].ToString();
                    dr["CELL_SPLY_STAT_CODE"] = _StatusCode;
                    dr["UPDUSER"] = LoginInfo.USERID;
                    RQSTDT.Rows.Add(dr);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_UPD_CELL_SPLY_REQ_RESPOND", "RQSTDT", null, RQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// CANCEL_RESPOND RSPN 정보 Request처리
        /// </summary>
        /// <param name="_dt"></param>
        private void _setCancelRequest(DataTable _dt)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
                RQSTDT.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                RQSTDT.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));
                RQSTDT.Columns.Add("CELL_SPLY_RSPN_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("CELL_SPLY_RSPN_QTY", typeof(decimal));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));

                foreach (DataRow row in _dt.Rows)
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["CELL_SPLY_RSPN_ID"] = row["CELL_SPLY_RSPN_ID"].ToString();
                    dr["CELL_SPLY_REQ_ID"] = row["CELL_SPLY_REQ_ID"].ToString();
                    dr["CELL_SPLY_RSPN_AREAID"] = row["CELL_SPLY_RSPN_AREAID"].ToString();
                    dr["CELL_SPLY_RSPN_STAT_CODE"] = "REQUEST";
                    dr["CELL_SPLY_TYPE_CODE"] = row["CELL_SPLY_TYPE_CODE"].ToString();
                    dr["AUTO_LOGIS_FLAG"] = row["AUTO_LOGIS_FLAG"].ToString();
                    dr["NOTE"] = string.Empty;
                    dr["CELL_SPLY_RSPN_QTY"] = 0;
                    dr["UPDUSER"] = LoginInfo.USERID;
                    RQSTDT.Rows.Add(dr);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_UPD_CELL_SPLY_RSPN_CANCEL_REQUEST", "RQSTDT", null, RQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
    }
}
