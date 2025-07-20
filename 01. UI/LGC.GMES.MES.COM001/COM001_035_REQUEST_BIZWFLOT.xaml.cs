/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  모름   : Initial Created.
  2023.02.20  정용석 : 결재요청 순서도 OUTPUT Parameter 추가로 인한 수정
  2023.03.22  정용석 : E20230315-000360 - BIZWF LOT 등록 취소시 XML 오류 발생 방어 내용 적용
  2024.02.22  남기운 : NERP대응 프로젝트  요청자/승인자가 같을 경우 자동 승인기능 적용
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_BIZWFLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        DataTable dtBizWFLotHearder = new DataTable();
        DataTable dtBizWFLotDetail = new DataTable();

        public COM001_035_REQUEST_BIZWFLOT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);
            }

            if (_reqNo.Equals("NEW"))
            {
                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 요청");
                    //reqList.Text = "요청목록"; // 2024.11.25. 김영국 - 다국어 처리.
                    reqList.Text = ObjectDic.Instance.GetObjectName("요청목록");
                    req_Qty.Visibility = Visibility.Visible;
                    txtLot.IsEnabled = true;

                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 취소 요청");
                    //reqList.Text = "요청취소목록"; // 2024.11.25. 김영국 - 다국어 처리.
                    reqList.Text = ObjectDic.Instance.GetObjectName("요청취소목록");
                    req_CancelQty.Visibility = Visibility.Visible;
                    txtLot.IsEnabled = false;
                }

                btnSelectHeader.IsEnabled = true;
                btnClear.IsEnabled = true;
                btnSearch.IsEnabled = true;

                btnReq.Visibility = Visibility.Visible;
                btnReqCancel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 요청 취소");
                    //reqList.Text = "요청목록"; // 2024.11.25. 김영국 - 다국어 처리.
                    reqList.Text = ObjectDic.Instance.GetObjectName("요청목록");
                    req_Qty.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 취소 요청 취소");
                    //reqList.Text = "요청취소목록"; // 2024.11.25. 김영국 - 다국어 처리.
                    reqList.Text = ObjectDic.Instance.GetObjectName("요청취소목록");
                    req_CancelQty.Visibility = Visibility.Visible;
                }

                SetModify();

                btnSelectHeader.IsEnabled = false;
                btnClear.IsEnabled = false;
                btnSearch.IsEnabled = false;
                txtLot.IsEnabled = false;

                btnReq.Visibility = Visibility.Collapsed;
                btnReqCancel.Visibility = Visibility.Visible;
            }

            SetBizWFSplitVisibility();
        }

        private void SetBizWFSplitVisibility()
        {
            DataTable dtInTable = new DataTable();
            dtInTable.Columns.Add("CMCDTYPE", typeof(string));
            dtInTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dtInTable.NewRow();
            dr["CMCDTYPE"] = "BIZWF_SPLIT_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            dtInTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtInTable);


            if (dtRslt.Rows.Count > 0)
            {
                if (_reqNo.Equals("NEW") &&  _reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    btnLotSplit.Visibility = Visibility.Visible;
                    btnLotSplitSearch.Visibility = Visibility.Visible;
                }
            }
            else
            {
                btnLotSplit.Visibility = Visibility.Collapsed;
                btnLotSplitSearch.Visibility = Visibility.Collapsed;
            }
        }


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

                DataTable dtOUTDATA = dsRslt.Tables["OUTDATA"];
                string sBIZ_WF_REQ_DOC_TYPE_CODE = Util.NVC(dtOUTDATA.Rows[0]["BIZ_WF_REQ_DOC_TYPE_CODE"]);
                string sBIZ_WF_REQ_DOC_NO = Util.NVC(dtOUTDATA.Rows[0]["BIZ_WF_REQ_DOC_NO"]);

                SearchBizWFHeader(sBIZ_WF_REQ_DOC_TYPE_CODE, sBIZ_WF_REQ_DOC_NO, "CANCEL");
                SearchBizWFDetail(sBIZ_WF_REQ_DOC_TYPE_CODE, sBIZ_WF_REQ_DOC_NO, "CANCEL");

                Util.gridClear(dgRequest);
                dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);

                Util.gridClear(dgGrator);
                dgGrator.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTPROG"]);

                Util.gridClear(dgNotice);
                dgNotice.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTREF"]);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            //this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSelectHeader_Click(object sender, RoutedEventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT_SEARCH wndPopup = new COM001_035_REQUEST_BIZWFLOT_SEARCH();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {

                object[] Parameters = new object[1];
                Parameters[0] = _reqType;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLotSearch_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopupBizWFLotSearch_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT_SEARCH window = sender as COM001_035_REQUEST_BIZWFLOT_SEARCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Clear();

                Util.gridClear(dgBizWFLotHeader);
                Util.gridClear(dgBizWFLotDetail);

                Util.GridSetData(dgBizWFLotHeader, window.BizWFHeader, FrameOperation);
                Util.GridSetData(dgBizWFLotDetail, window.BizWFDetail, FrameOperation);

                dtBizWFLotHearder = window.BizWFHeader;
                dtBizWFLotDetail = window.BizWFDetail;

                if (_reqType.Equals("REQUEST_BIZWF_LOT") == false)  //등록 취소 일 경우만
                {
                    GetLotList(null);
                }
            }
        }
        private void wndPopupBizWFLotSplit_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT_SPLIT window = sender as COM001_035_REQUEST_BIZWFLOT_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                string[] sArr = window._sBizWFSPlitLotID;

                for(int i = 0; i < sArr.Length; i++)
                {
                    GetLotList(sArr[i]);
                }
            }
        }
        
        private void SearchBizWFHeader(string pBIZ_WF_REQ_DOC_TYPE_CODE, string pBIZ_WF_REQ_DOC_NO, string sType)
        {
            try
            {
                Util.gridClear(dgBizWFLotHeader);

                DataTable dtInTable = new DataTable();
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("SHOPID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("REQ_TYPE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REQ_TYPE"] = sType;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = pBIZ_WF_REQ_DOC_TYPE_CODE;
                dr["BIZ_WF_REQ_DOC_NO"] = pBIZ_WF_REQ_DOC_NO;

                dtInTable.Rows.Add(dr);

                const string bizRuleName = "BR_PRD_SEL_ERP_BIZWF_DOC_HEADER";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }
                else
                {
                    //dgBizWFLotHeader.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgBizWFLotHeader, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchBizWFDetail(string pBIZ_WF_REQ_DOC_TYPE_CODE, string pBIZ_WF_REQ_DOC_NO, string sType)
        {
            try
            {
                Util.gridClear(dgBizWFLotDetail);

                DataTable dtInTable = new DataTable();
                dtInTable.Columns.Add("LANGID", typeof(string));
                //dtInTable.Columns.Add("SHOPID", typeof(string));
                //dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = pBIZ_WF_REQ_DOC_TYPE_CODE;
                dr["BIZ_WF_REQ_DOC_NO"] = pBIZ_WF_REQ_DOC_NO;

                dtInTable.Rows.Add(dr);

                const string bizRuleName = "BR_PRD_SEL_ERP_BIZWF_DOC_DETAIL";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }
                else
                {
                    Util.GridSetData(dgBizWFLotDetail, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

            Clear();

            Util.gridClear(dgBizWFLotHeader);
            Util.gridClear(dgBizWFLotDetail);

            Util.GridSetData(dgBizWFLotHeader, dtBizWFLotHearder, FrameOperation);
            Util.GridSetData(dgBizWFLotDetail, dtBizWFLotDetail, FrameOperation);
   


        }

        private void Clear()
        {
            txtLot.Text = string.Empty;
            txtGrator.Text = string.Empty;
            txtNotice.Text = string.Empty;
            txtNote.Text = string.Empty;
            Util.gridClear(dgList);
            Util.gridClear(dgRequest);
            Util.gridClear(dgGrator);
            Util.gridClear(dgNotice);
            Util.gridClear(dgUnAvailableList);

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
            {
                return;
            }

            Util.gridClear(dgUnAvailableList);
            Util.gridClear(dgList);

            GetLotList(txtLot.Text);
        }

        private bool ValidationSearch(bool isCopyAndPaste = false)
        {
            if (dgBizWFLotHeader.GetRowCount() == 0 || dgBizWFLotDetail.GetRowCount() == 0)
            {
                Util.Alert("SFU3795");  //BizWF 요청서 목록이 없습니다.
                return false;
            }

            if (isCopyAndPaste == false && string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1190"); //조회할 LOT ID 를 입력하세요.
                return false;
            }

            return true;
        }


        private bool UpdateBizWF(string sREQ_NO)
        {
            try
            {                
                DataTable dtRqst = new DataTable();                
                dtRqst.Columns.Add("REQ_NO", typeof(string));
                DataRow dr = dtRqst.NewRow();                
                dr["REQ_NO"] = sREQ_NO;               
                dtRqst.Rows.Add(dr);                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_ERP_BIZWF_LOT_INFO_STAT_SNET", "RQSTDT", "RSLTDT", dtRqst);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_SEL_ERP_BIZWF_LOT 에서 LOT 조회시도 기본 Validation 수행함.
            //Validation 수정시 BR_PRD_SEL_ERP_BIZWF_LOT 도 확인해야 함.

            if (dgBizWFLotHeader.GetRowCount() == 0 || dgBizWFLotDetail.GetRowCount() == 0)
            {
                Util.Alert("SFU3795");  //BizWF 요청서 목록이 없습니다.
                return;
            }

            if (dgRequest.GetRowCount() == 0)
            {
                Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                return;
            }

            if (dgGrator.GetRowCount() == 0)
            {
                Util.Alert("SFU1692");  //승인자가 필요합니다.
                return;
            }

            for (int inx = 0; inx < dgBizWFLotDetail.Rows.Count; inx++)
                {
                    decimal dBizWFLotDetailTotal = Util.NVC_Decimal(DataTableConverter.GetValue(dgBizWFLotDetail.Rows[inx].DataItem, "TOTAL_WIPQTY"));
                    decimal dBizWFLotDetailReq = Util.NVC_Decimal(DataTableConverter.GetValue(dgBizWFLotDetail.Rows[inx].DataItem, "BIZ_WF_REQ_QTY"));

                    string sBizWFLotDetailProdid = Util.NVC(DataTableConverter.GetValue(dgBizWFLotDetail.Rows[inx].DataItem, "PRODID"));

                    DataTable dtRequest = DataTableConverter.Convert(dgRequest.ItemsSource);
                    
                    //decimal dtRequestTotal = Util.NVC_Decimal(dtRequest.Compute("SUM(WIPQTY2)", "PRODID = '" + sBizWFLotDetailProdid + "'"));
                    decimal dtRequestTotal = Util.NVC_Decimal(dtRequest.Compute("SUM(BIZ_WF_PRCS_QTY)", "BIZ_WF_PRODID = '" + sBizWFLotDetailProdid + "'"));


                if (dBizWFLotDetailTotal > dBizWFLotDetailReq)
                {
                    //[%1]의 총수량이 [%2] 요청수량보다 [%3] 큽니다.
                    Util.Alert("SFU8392", new object[] { sBizWFLotDetailProdid, dBizWFLotDetailTotal, dBizWFLotDetailReq });
                    return;
                }
            }
            

            //요청하시겠습니까?
            Util.MessageConfirm("SFU2924", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //추가 
                    if (dgGrator.Rows.Count > 0)
                    {
                        string sUserID = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[0].DataItem, "USERID")).Trim();

                        //if (LoginInfo.USERID.Equals(sUserID))
                        if (LoginInfo.USERID.Equals(sUserID) && (dgGrator.Rows.Count == 1) )
                        {
                            //승인자가 1차 승인자 밖에 없고 자가 승인인 경우
                            loadingIndicator.Visibility = Visibility.Visible;
                            //요청자와 승인자 같은 경우 자동 승인 처리
                            Request_Accept();
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            //기존로직
                            Request();
                        }
                    }
                }
            });
        }

        private void Request()
        {
            try
            {
                bool isAggregate = false;
                string sTo = "";
                string sCC = "";

                DataSet inData = new DataSet();
                
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_NOTE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["APPR_BIZ_CODE"] = _reqType;
                row["USERID"] = LoginInfo.USERID;
                row["REQ_NOTE"] = Util.GetCondition(txtNote);
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                row["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
                inDataTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(string));
                inLot.Columns.Add("WIPQTY2", typeof(string));
                inLot.Columns.Add("PRODID", typeof(string));    //메일 발신용
                inLot.Columns.Add("PRODNAME", typeof(string));  //메일 발신용
                inLot.Columns.Add("MODELID", typeof(string));   //메일 발신용
                inLot.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inLot.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                inLot.Columns.Add("BIZ_WF_ITEM_SEQNO", typeof(string));
                inLot.Columns.Add("BIZ_WF_SLOC_ID", typeof(string));
                inLot.Columns.Add("BIZ_WF_LOT_SEQNO", typeof(string));

                // CSR : E20230315-000360 - BIZWF LOT 등록 취소시 XML 오류 발생 방어 내용 적용 (Row 갯수가 300개 이상인 경우)
                if (this.dgRequest.Rows.Count() > 300 && !this._reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    isAggregate = true;
                    row = inLot.NewRow();
                    row["LOTID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                    row["WIPQTY"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<Int64>("WIPQTY"))).Aggregate((current, next) => current + "," + next);
                    row["WIPQTY2"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<Int64>("BIZ_WF_PRCS_QTY"))).Aggregate((current, next) => current + "," + next);
                    row["PRODID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_PRODID")).Aggregate((current, next) => current + "," + next);
                    row["PRODNAME"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("PRODNAME")).Aggregate((current, next) => current + "," + next);
                    row["MODELID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("MODLID")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_REQ_DOC_TYPE_CODE")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_REQ_DOC_NO")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_ITEM_SEQNO"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<Int64>("BIZ_WF_ITEM_SEQNO"))).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_SLOC_ID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_SLOC_ID")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_LOT_SEQNO"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<string>("BIZ_WF_LOT_SEQNO"))).Aggregate((current, next) => current + "," + next);
                    inLot.Rows.Add(row);
                }
                else
                {
                    for (int i = 0; i < dgRequest.Rows.Count; i++)
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                        row["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY"));
                        row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRCS_QTY"));
                        row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRODID"));
                        row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                        row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODLID"));
                        row["BIZ_WF_REQ_DOC_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE"));
                        row["BIZ_WF_REQ_DOC_NO"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_NO"));
                        row["BIZ_WF_ITEM_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_ITEM_SEQNO")));
                        row["BIZ_WF_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_SLOC_ID"));
                        row["BIZ_WF_LOT_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_LOT_SEQNO")));
                        inLot.Rows.Add(row);
                    }
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

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);

                // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    if (CommonVerify.HasTableInDataSet(dsRslt) && CommonVerify.HasTableRow(dsRslt.Tables["OUTDATA_LOT"]))
                    {
                        this.Show_COM001_035_PACK_EXCEPTION_POPUP(dsRslt.Tables["OUTDATA_LOT"]);
                        return;
                    }
                }

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    if (isAggregate)
                    {
                        inLot.Clear();
                        for (int i = 0; i < dgRequest.Rows.Count; i++)
                        {
                            row = inLot.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                            row["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY"));
                            row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRCS_QTY"));
                            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRODID"));
                            row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                            row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODLID"));
                            row["BIZ_WF_REQ_DOC_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE"));
                            row["BIZ_WF_REQ_DOC_NO"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_NO"));
                            row["BIZ_WF_ITEM_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_ITEM_SEQNO")));
                            row["BIZ_WF_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_SLOC_ID"));
                            row["BIZ_WF_LOT_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_LOT_SEQNO")));
                            inLot.Rows.Add(row);
                        }

                    }
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }

                Util.AlertInfo("SFU1747");  //요청되었습니다.

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void Request_Accept()
        {
            try
            {
                bool isAggregate = false;
                string sTo = "";
                string sCC = "";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_NOTE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["APPR_BIZ_CODE"] = _reqType;
                row["USERID"] = LoginInfo.USERID;
                row["REQ_NOTE"] = Util.GetCondition(txtNote);
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                row["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
                inDataTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(string));
                inLot.Columns.Add("WIPQTY2", typeof(string));
                inLot.Columns.Add("PRODID", typeof(string));    //메일 발신용
                inLot.Columns.Add("PRODNAME", typeof(string));  //메일 발신용
                inLot.Columns.Add("MODELID", typeof(string));   //메일 발신용
                inLot.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inLot.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                inLot.Columns.Add("BIZ_WF_ITEM_SEQNO", typeof(string));
                inLot.Columns.Add("BIZ_WF_SLOC_ID", typeof(string));
                inLot.Columns.Add("BIZ_WF_LOT_SEQNO", typeof(string));

                // CSR : E20230315-000360 - BIZWF LOT 등록 취소시 XML 오류 발생 방어 내용 적용 (Row 갯수가 300개 이상인 경우)
                if (this.dgRequest.Rows.Count() > 300 && !this._reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    isAggregate = true;
                    row = inLot.NewRow();
                    row["LOTID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                    row["WIPQTY"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<Int64>("WIPQTY"))).Aggregate((current, next) => current + "," + next);
                    row["WIPQTY2"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<Int64>("BIZ_WF_PRCS_QTY"))).Aggregate((current, next) => current + "," + next);
                    row["PRODID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_PRODID")).Aggregate((current, next) => current + "," + next);
                    row["PRODNAME"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("PRODNAME")).Aggregate((current, next) => current + "," + next);
                    row["MODELID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("MODLID")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_REQ_DOC_TYPE_CODE")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_REQ_DOC_NO")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_ITEM_SEQNO"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<Int64>("BIZ_WF_ITEM_SEQNO"))).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_SLOC_ID"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => x.Field<string>("BIZ_WF_SLOC_ID")).Aggregate((current, next) => current + "," + next);
                    row["BIZ_WF_LOT_SEQNO"] = DataTableConverter.Convert(this.dgRequest.ItemsSource).AsEnumerable().Select(x => Convert.ToString(x.Field<string>("BIZ_WF_LOT_SEQNO"))).Aggregate((current, next) => current + "," + next);
                    inLot.Rows.Add(row);
                }
                else
                {
                    for (int i = 0; i < dgRequest.Rows.Count; i++)
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                        row["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY"));
                        row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRCS_QTY"));
                        row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRODID"));
                        row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                        row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODLID"));
                        row["BIZ_WF_REQ_DOC_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE"));
                        row["BIZ_WF_REQ_DOC_NO"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_NO"));
                        row["BIZ_WF_ITEM_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_ITEM_SEQNO")));
                        row["BIZ_WF_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_SLOC_ID"));
                        row["BIZ_WF_LOT_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_LOT_SEQNO")));
                        inLot.Rows.Add(row);
                    }
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

                    if (i == 1)//최초 승인자는 승인처리를 완료 했기 때문에  2번째 승인자에게 메일 전송
                    {
                        sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                    }
                }

                //참조자? 메일을 보내야 하나?
                DataTable inRef = inData.Tables.Add("INREF");
                inRef.Columns.Add("REF_USERID", typeof(string));
                string sCCTemp = "";

                for (int i = 0; i < dgNotice.Rows.Count; i++)
                {
                    row = inRef.NewRow();
                    row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID"));
                    inRef.Rows.Add(row);

                    sCCTemp = Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID"));
                    if (!LoginInfo.USERID.Equals(sCCTemp))
                        sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);

                // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
                if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
                {
                    if (CommonVerify.HasTableInDataSet(dsRslt) && CommonVerify.HasTableRow(dsRslt.Tables["OUTDATA_LOT"]))
                    {
                        this.Show_COM001_035_PACK_EXCEPTION_POPUP(dsRslt.Tables["OUTDATA_LOT"]);
                        return;
                    }
                }

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;
                    string sReq_No = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString();

                    if (isAggregate)
                    {
                        inLot.Clear();
                        for (int i = 0; i < dgRequest.Rows.Count; i++)
                        {
                            row = inLot.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                            row["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY"));
                            row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRCS_QTY"));
                            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_PRODID"));
                            row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                            row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODLID"));
                            row["BIZ_WF_REQ_DOC_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE"));
                            row["BIZ_WF_REQ_DOC_NO"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_REQ_DOC_NO"));
                            row["BIZ_WF_ITEM_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_ITEM_SEQNO")));
                            row["BIZ_WF_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_SLOC_ID"));
                            row["BIZ_WF_LOT_SEQNO"] = Convert.ToString(Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "BIZ_WF_LOT_SEQNO")));
                            inLot.Rows.Add(row);
                        }

                    }
                    //메일은 다음 완료 처리 후 발송                  
                    //mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                    //5초 지연

                    //sBIZ_WF_REQ_DOC_TYPE_CODE = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                    //sBIZ_WF_REQ_DOC_NO = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
                    //UpdateBizWF()

                    //자가 승인인경우 SENT 업데이트
                    if (UpdateBizWF(sReq_No))
                    {
                        Delay(1000);
                        //승인 요청
                        if (Accept(sReq_No))
                        {
                            Util.AlertInfo("SFU1690");  //승인되었습니다.
                        }
                        else
                        {
                            // Util.AlertInfo("MMD0173"); //승인건은 처리 할 수 없습니다.
                        }
                    }
                }

                // Util.AlertInfo("SFU1747");  //요청되었습니다.
               
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void Delay(int ms)
        {
            DateTime dateTimeNow = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            DateTime dateTimeAdd = dateTimeNow.Add(duration);
            while (dateTimeAdd >= dateTimeNow)
            {
                System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
            return;
        }

        private bool Accept(string sReq_no)
        {     
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
                //dtRqst.Columns.Add("RESNCODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = sReq_no;// drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "APP";
                dr["APPR_NOTE"] = Util.GetCondition(txtNote);
                dr["APPR_BIZ_CODE"] = _reqType; 
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);
                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR", "INDATA", "OUTDATA,LOT_INFO", dsRqst);

                //Util.AlertInfo("SFU1690");  //승인되었습니다.

                //참조 가지고 오기
                string sCC = "";
                for (int i = 0; i < dgNotice.Rows.Count; i++)
                {
                    sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
                }


                MailSend mail = new CMM001.Class.MailSend();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { //다음차수 안내메일
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = sReq_no + " " + sMsg;//drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, 
                        dsRslt.Tables["OUTDATA"].Rows[0]["APPR_USERID"].ToString(),
                        sCC, 
                        sMsg, 
                        mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), 
                        dsRslt.Tables["LOT_INFO"]));
                    
                    //현재 자기 자신 완료 메일 보내기
                    sMsg = ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    sTitle = sReq_no + " " + sMsg;  //drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, LoginInfo.USERID,
                        sCC,
                        sMsg,
                        mail.makeBodyApp(sTitle, Util.GetCondition(txtNote),
                        dsRslt.Tables["LOT_INFO"]));
                }
                else
                {  //완료메일
                    string sMsg = ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    string sTitle = sReq_no + " " + sMsg;  //drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, LoginInfo.USERID,
                        sCC, 
                        sMsg, 
                        mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), 
                        dsRslt.Tables["LOT_INFO"]));                    
                }

                return true;
          
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }




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

        private void ReqCancel()
        { 
            try
            {
                string sTo = "";
                string sCC = "";

                //현재상태 체크
                DataTable dtRqstStatus = new DataTable();
                dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

                DataRow drStatus = dtRqstStatus.NewRow();
                drStatus["REQ_NO"] = _reqNo;


                dtRqstStatus.Rows.Add(drStatus);

                DataTable dtRsltStatus = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqstStatus);

                if (!dtRsltStatus.Rows[0]["REQ_RSLT_CODE"].Equals("REQ"))
                {
                    Util.AlertInfo("SFU1691");  //승인이 진행 중입니다.
                }
                else
                {
                    //여기까지 현재상태 체크

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("REQ_NO", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));
                    dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                    dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["REQ_NO"] = _reqNo;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["REQ_RSLT_CODE"] = "DEL";
                    dr["APPR_BIZ_CODE"] = _reqType;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_UPD_APPR_BIZWF_LOT", "INDATA", null, dtRqst);

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

                    Util.AlertInfo("SFU1937");  //취소되었습니다.
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!ValidationSearch())
                    {
                        return;
                    }

                    Util.gridClear(dgUnAvailableList);
                    Util.gridClear(dgList);

                    GetLotList(txtLot.Text);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetLotList(string pStrLot)
        {

            try
            {
                //ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_SEL_ERP_BIZWF_LOT";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOT_REG_TYPE", typeof(string));
                inTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                if (_reqType.Equals("REQUEST_BIZWF_LOT") == false)  //등록 취소 일 경우만
                {
                    inTable.Columns.Add("BIZ_WF_ITEM_SEQNO", typeof(int));
                }
                else
                {
                    inTable.Columns.Add("LOTID", typeof(string));
                }

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                dr["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    dr["LOTID"] = pStrLot;
                    dr["LOT_REG_TYPE"] = "REG";
                }
                else
                {
                    dr["LOT_REG_TYPE"] = "CNCL";
                }

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (dtRslt.Rows.Count == 0)
                {
                    SetUnAvailableLotList(dtRslt, pStrLot); //그리드에 디폴트 데이터소스 만들기 위해 dtRslt 도 보냄
                }
                else
                {
                    DataRow[] drUnAvailable = dtRslt.Select("BIZ_WF_CAN_FLAG = 'N'");
                    if (drUnAvailable != null && drUnAvailable.Length > 0)
                    {
                        // PACK 일경우 포장된 LOT요청할 경우 요청 불가 함, message popup은 사용자가 조치를 할수 있도록 가이드 하기 위함.
                        DataRow[] drFindPackedOutdata = dtRslt.Select("INVALID_CAUSE_MSG = 'PACKED STATUS'");
                        Boolean bMessageOpen = true;
                        if (DataTableConverter.Convert(dgUnAvailableList.ItemsSource).Rows.Count > 0)
                        {
                            DataRow[] drFindPackedDataGrid = DataTableConverter.Convert(dgUnAvailableList.ItemsSource).Select("INVALID_CAUSE_MSG = 'PACKED STATUS'");
                            if(drFindPackedDataGrid.Length > 0)
                            {
                                bMessageOpen = false;
                            }
                        }                        
                        if (drFindPackedOutdata.Length > 0 && bMessageOpen)
                        {
                            Util.AlertInfo("SFU8416");  // 포장해체 후 작업 진행이 가능한 LOT이 존재합니다.
                        }
                        DataTable dtUnAvailable = drUnAvailable.CopyToDataTable();
                        SetUnAvailableLotList(dtUnAvailable, pStrLot);
                    }

                    DataRow[] drAvailable = dtRslt.Select("BIZ_WF_CAN_FLAG = 'Y'");
                    if (drAvailable != null && drAvailable.Length > 0)
                    {
                        DataTable dtAvailable = drAvailable.CopyToDataTable();

                        DataTable dtSource = DataTableConverter.Convert(dgList.ItemsSource);
                        dtSource.Merge(dtAvailable);

                        Util.GridSetData(dgList, dtSource, FrameOperation, true);
                    }
                }

                if(dgList.GetRowCount() <= 0 && dgUnAvailableList.GetRowCount() <= 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }

                txtLot.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }

        private void SetUnAvailableLotList(DataTable pDtRslt, string pStrLot)
        {
            if (pDtRslt == null || string.IsNullOrEmpty(pStrLot))
            {
                return;
            }

            DataTable dtSource = DataTableConverter.Convert(dgUnAvailableList.ItemsSource);
            dtSource.Merge(pDtRslt);

            Util.GridSetData(dgUnAvailableList, dtSource, FrameOperation, true);

            //dgUnAvailableList에 넣을 ROW 가 없는 데이터셋을 전달 받았을 경우 LOT 만 넣어줌
            if (pDtRslt.Rows.Count <= 0)
            {
                DataRow drRslt = dtSource.NewRow();
                drRslt["LOTID"] = pStrLot;
                dtSource.Rows.Add(drRslt);

                Util.GridSetData(dgUnAvailableList, dtSource, FrameOperation, true);
            }
        }

        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    int maxLOTIDCount = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? 500 : 100;
                    string messageID = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? "SFU8217" : "SFU3695";
                    if (sPasteStrings.Count() > maxLOTIDCount)
                    {
                        if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK))
                        {
                            Util.MessageValidation(messageID, 500);     // 최대 500개 까지 가능합니다.
                        }
                        else
                        {
                            Util.MessageValidation(messageID);          // 최대 100개 까지 가능합니다.
                        }
                        return;
                    }


                    if (!ValidationSearch(true))
                    {
                        return;
                    }

                    Util.gridClear(dgUnAvailableList);
                    Util.gridClear(dgList);

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;
                        }

                        if (dgList.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgList.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "LOTID").ToString() == sPasteStrings[i])
                                {
                                    dgList.ScrollIntoView(i, dgList.Columns["CHK"].Index);
                                    dgList.SelectedIndex = i;
                                    dgList.Focus();

                                    txtLot.Text = string.Empty;

                                    return;
                                }
                            }
                        }

                        GetLotList(sPasteStrings[i]);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }


       
        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtGrator.Text.Trim() == string.Empty)
                    {
                        return;
                    }

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
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
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
                            Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
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


                        dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtGrator.Text = "";
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);

                        dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {

                    if (txtNotice.Text.Trim() == string.Empty)
                    {
                        return;
                    }

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
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
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
                            Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
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

                        dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtNotice.Text = "";
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelect);

                        dgNoticeSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
        }

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
        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
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
                    Util.MessageException(ex);
                }
            }));
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                row["CHK"] = true;
            }

            dt.AcceptChanges();

            chkAllSelect();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);            

            DataTable dtSelect = new DataTable();
            DataTable dtTo = DataTableConverter.Convert(dgList.ItemsSource);
            DataTable dtBizwf = DataTableConverter.Convert(dgBizWFLotDetail.ItemsSource);


            dtTo.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

            dtSelect = dtTo.Copy();


            for (int idx_detl = 0; idx_detl < dtTo.Rows.Count; idx_detl++)
            {                    
                for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
                {
                    if(idx_detl == 0)
                    {                        
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = 0;
                    }

                    if (dtTo.Rows[idx_detl]["BIZ_WF_PRODID"].Equals(dtBizwf.Rows[idx]["PRODID"]) )
                    {
                        if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                        {
                            dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = Convert.ToDecimal(dtTo.Rows[idx_detl]["BIZ_WF_PRCS_QTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);                                
                        }
                        else
                        {
                            dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = 0;                            
                            dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = Convert.ToDecimal(dtTo.Rows[idx_detl]["BIZ_WF_PRCS_QTY"]);
                        }
                    }                        
                }
            }

            if (_reqType.Equals("REQUEST_BIZWF_LOT"))
            {
                for (int index = 0; index < dgBizWFLotDetail.Rows.Count; index++)
                {                    
                    if (!string.IsNullOrEmpty( dtBizwf.Rows[index]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                    {
                        dtBizwf.Rows[index]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[index]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_CANCEL_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["BIZ_WF_REQ_TOTALQTY"]);
                    }
                    else{                        
                        dtBizwf.Rows[index]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[index]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_CANCEL_WIPQTY"]) ;
                    }                                        
                }
            }
            else
            {
                for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
                {
                    if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                    {
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"]);
                    }else
                    {
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = (Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"])); // - Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);
                    }                    
                    
                }
            }


            dgBizWFLotDetail.ItemsSource = DataTableConverter.Convert(dtBizwf);
            dgRequest.ItemsSource = DataTableConverter.Convert(dtSelect);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dtTo = ((DataView)dgList.ItemsSource).Table;
            DataTable dtBizwf = DataTableConverter.Convert(dgBizWFLotDetail.ItemsSource);

            dtTo.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);

            dtTo.AcceptChanges();

                for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
                {
                    if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                    {
                        dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = 0;
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = 0;
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"]);
                    }
                     
                }


                dgBizWFLotDetail.ItemsSource = DataTableConverter.Convert(dtBizwf);
            
            chkAllClear();
        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
        }

        private void chk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                //if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1) || DataTableConverter.GetValue(cb.DataContext, "CHK").Equals("True") || DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(true))
                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(true) ||
                    DataTableConverter.GetValue(cb.DataContext, "CHK").Nvc().Equals("1") ||
                    Convert.ToString(DataTableConverter.GetValue(cb.DataContext, "CHK")) == "True")
                {
                    //체크되는 경우
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);
                    DataTable dtBizwf = DataTableConverter.Convert(dgBizWFLotDetail.ItemsSource);

                    if (dtTo.Columns.Count == 0) //최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo = DataTableConverter.Convert(dgList.ItemsSource).Copy();
                        dtTo.Clear();
                    }

                    if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length > 0)   //중복조건 체크
                    {
                        return;
                    }

                    for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgBizWFLotDetail.Rows[idx].DataItem, "PRODID")).Equals(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRODID")))
                        {
                            if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                            {
                                dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY")) + Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);

                                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                                {
                                    dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["TOTAL_WIPQTY"]) + Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY"));
                                }
                                else
                                {
                                    dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["TOTAL_WIPQTY"]) - Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY"));
                                }
                            }
                            else
                            {                                
                                dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = 0;

                                dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY"));

                                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                                {
                                    dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = (Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"])) + Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);
                                }
                                else
                                {
                                    dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = (Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"])) - Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);
                                }
                                
                            }
                        }
                    }
                    
                                        
                    
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName) == null ? DBNull.Value : DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    dtTo.Rows.Add(dr);

                    dgBizWFLotDetail.ItemsSource = DataTableConverter.Convert(dtBizwf);
                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    //체크 풀릴때

                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);
                    DataTable dtBizwf = DataTableConverter.Convert(dgBizWFLotDetail.ItemsSource);

                    if (dtTo.Rows.Count == 0 || dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length == 0)
                    {
                        return;
                    }

                    for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgBizWFLotDetail.Rows[idx].DataItem, "PRODID")).Equals(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRODID")))
                        {
                            if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                            {
                                dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]) - Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY"));

                                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                                {
                                    dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["TOTAL_WIPQTY"]) - Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY"));
                                }
                                else
                                {
                                    dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["TOTAL_WIPQTY"]) + Convert.ToDecimal(DataTableConverter.GetValue(cb.DataContext, "BIZ_WF_PRCS_QTY"));
                                }
                            }
                        }
                    }
                    
                    dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);
                    dgBizWFLotDetail.ItemsSource = DataTableConverter.Convert(dtBizwf);
                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            
        }

        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
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
                    Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
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


                dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                dgGratorSelect.Visibility = Visibility.Collapsed;

                txtGrator.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRequest_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void dgRequest_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

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

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
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
                    Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                    dgNoticeSelect.Visibility = Visibility.Collapsed;
                    return;
                }

                DataRow drFrom = dtTo.NewRow();
                drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
                drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
                drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

                dtTo.Rows.Add(drFrom);


                dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

                dgNoticeSelect.Visibility = Visibility.Collapsed;

                txtNotice.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnLotSplit_Click(object sender, RoutedEventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT_SPLIT wndPopup = new COM001_035_REQUEST_BIZWFLOT_SPLIT();
            wndPopup.FrameOperation = FrameOperation;

            if (!ValidationSplit())
            {
                return;
            }

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                Parameters[1] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_TYPE_NAME").ToString();
                Parameters[2] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[0].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLotSplit_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private bool ValidationSplit()
        {
            if (dgBizWFLotHeader.GetRowCount() == 0 || dgBizWFLotDetail.GetRowCount() == 0)
            {
                Util.Alert("SFU3795");  //BizWF 요청서 목록이 없습니다.
                return false;
            }

            return true;
        }

        private void btnLotSplitSearch_Click(object sender, RoutedEventArgs e)
        {
            COM001_035_REQUEST_BIZWFLOT_SPLIT_SEARCH wndPopup = new COM001_035_REQUEST_BIZWFLOT_SPLIT_SEARCH();
            wndPopup.FrameOperation = FrameOperation;


            if (wndPopup != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                // wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
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

            if (CommonVerify.HasTableRow(dtResult))
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

        // 불건전 Data 표출 Popup Open
        private void Show_COM001_035_PACK_EXCEPTION_POPUP(DataTable dt)
        {
            COM001_035_PACK_EXCEPTION_POPUP wndPopUp = new COM001_035_PACK_EXCEPTION_POPUP();
            wndPopUp.FrameOperation = FrameOperation;

            if (wndPopUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = "APPR_BIZ";

                C1WindowExtension.SetParameters(wndPopUp, Parameters);
                wndPopUp.ShowModal();
                wndPopUp.CenterOnScreen();
                wndPopUp.BringToFront();
            }
        }
    }
}
