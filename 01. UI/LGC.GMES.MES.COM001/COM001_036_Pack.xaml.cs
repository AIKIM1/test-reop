/*************************************************************************************
 Created Date : 2016.12.28
      Creator : 정규환
  Description : LOT Release/물품청구/오만가지 승인 (Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.28    정규환 : Initial Created.
  2023.02.12    정용석 : 승인이력 Tab 추가
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_036_Pack : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private COM001_036_Pack_DataHelper dataHelper = new COM001_036_Pack_DataHelper();
        private DataTable dtReferrer = new DataTable();         // 참조자 List
        private string selectedRequestNo = string.Empty;
        private string selectedApprovalBusinessCode = string.Empty;
        private string selectedRequestUserID = string.Empty;
        private string callControlName = string.Empty;
        #endregion

        #region Constructor 
        public COM001_036_Pack()
        {
            InitializeComponent();
        }
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Member Function Lists...
        // 로딩 그림 나오게끔 해주는거
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        // Form Initialize
        private void Initialize()
        {
            List<Button> lstAuthorize = new List<Button>();
            lstAuthorize.Add(this.btnApproveRequest);
            lstAuthorize.Add(this.btnRejectRequest);
            Util.pageAuth(lstAuthorize, FrameOperation.AUTHORITY);

            // 날짜 씨리즈...
            this.ldpDateFromHist.ApplyTemplate();
            this.ldpDateFromHist.SelectedDateTime = DateTime.Now.AddDays(-7);
            this.ldpDateToHist.ApplyTemplate();
            this.ldpDateToHist.SelectedDateTime = DateTime.Now;

            // ComboBox 씨리즈...
            Util.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaIDHist, false, "SELECT");
            Util.SetC1ComboBox(this.dataHelper.GetApproveBusinessCode("APPR_BIZ_CODE"), this.cboApprovalBusinessCodeHist, false, "ALL");
            Util.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("REQ_RSLT_CODE"), this.cboRequestResultCodeHist, false, "ALL");
            this.cboAreaIDHist.SelectedValue = LoginInfo.CFG_AREA_ID;

            this.txtRowCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }

        // 조회 - 승인 Tab
        private void GetApprovalRequestList()
        {
            try
            {
                string requestUserID = (this.txtRequestUser.Tag == null || string.IsNullOrEmpty(this.txtRequestUser.Tag.ToString())) ? null : this.txtRequestUser.Tag.ToString();
                string approvalUserID = LoginInfo.USERID;

                this.loadingIndicator.Visibility = Visibility.Visible;
                this.txtRowCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(this.dgRequestList);
                Util.gridClear(this.dgApprover);
                Util.gridClear(this.dgRequestLOT);
                this.DoEvents();
                DataTable dt = this.dataHelper.GetApprovalRequestList(requestUserID, approvalUserID);
                Util.GridSetData(this.dgRequestList, dt, FrameOperation);
                this.txtRowCount.Text = "[" + dt.Rows.Count.ToString() + " " + ObjectDic.Instance.GetObjectName("건") + " ]";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 조회 - 요청이력 상세정보 (승인 Tab)
        private void GetApprovalRequestDetail(RadioButton radioButton)
        {
            // 최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(radioButton.DataContext, "CHK").Equals(0))
            {
                foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in ((DataGridCellPresenter)radioButton.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(dataGridRow.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(radioButton.DataContext, "CHK", 1);
                ((DataGridCellPresenter)radioButton.Parent).DataGrid.SelectedIndex = ((DataGridCellPresenter)radioButton.Parent).Cell.Row.Index;
            }

            try
            {
                if (this.dgRequestList == null || this.dgRequestList.GetRowCount() <= 0)
                {
                    return;
                }

                this.selectedRequestNo = Util.NVC(DataTableConverter.GetValue(radioButton.DataContext, "REQ_NO"));
                this.selectedApprovalBusinessCode = Util.NVC(DataTableConverter.GetValue(radioButton.DataContext, "APPR_BIZ_CODE"));

                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgApprover);
                Util.gridClear(this.dgRequestLOT);
                this.DoEvents();
                DataSet ds = this.dataHelper.GetApprovalRequestDetail(this.selectedRequestNo);

                if (CommonVerify.HasTableInDataSet(ds))
                {
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_LOT"])) Util.GridSetData(this.dgRequestLOT, ds.Tables["OUTDATA_LOT"], FrameOperation);
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_APPROVER"])) Util.GridSetData(this.dgApprover, ds.Tables["OUTDATA_APPROVER"], FrameOperation);
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_REFERRER"])) this.dtReferrer = ds.Tables["OUTDATA_REFERRER"];
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 조회 - 승인이력 Tab
        private void GetApproveHistory(string LOTID = null)
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                LOTID = this.txtLOTIDHist.Text.Trim();
            }

            if (string.IsNullOrEmpty(this.cboAreaIDHist.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU3203");  // 동은 필수입니다.
                return;
            }

            if (string.IsNullOrEmpty(this.txtRequestUserHist.Text))
            {
                this.txtRequestUserHist.Tag = string.Empty;
            }

            try
            {
                string areaID = this.cboAreaIDHist.SelectedValue.ToString();
                string fromDate = this.ldpDateFromHist.SelectedDateTime.Date.ToString("yyyyMMdd");
                string toDate = this.ldpDateToHist.SelectedDateTime.Date.ToString("yyyyMMdd");
                //string userName = (this.txtRequestUserHist.Tag == null || string.IsNullOrEmpty(this.txtRequestUserHist.Tag.ToString())) ? null : this.txtRequestUserHist.Tag.ToString();
                string userName = (this.txtRequestUserHist.Text == null || string.IsNullOrEmpty(this.txtRequestUserHist.Text.ToString())) ? null : this.txtRequestUserHist.Text.ToString();
                string approvalbusinessCode = string.IsNullOrEmpty(this.cboApprovalBusinessCodeHist.SelectedValue.ToString()) ? null : this.cboApprovalBusinessCodeHist.SelectedValue.ToString();
                string requestResultCode = string.IsNullOrEmpty(this.cboRequestResultCodeHist.SelectedValue.ToString()) ? null : this.cboRequestResultCodeHist.SelectedValue.ToString();
                string productID = string.IsNullOrEmpty(this.txtProdID.Text) ? null : this.txtProdID.Text;

                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgApproveHistory);
                this.DoEvents();
                DataTable dt = this.dataHelper.GetApproveHistory(areaID, fromDate, toDate, userName, approvalbusinessCode, requestResultCode, productID, LOTID);
                Util.GridSetData(this.dgApproveHistory, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // Popup - 사용자정보
        private void Show_CMM_PERSON(string callControlName)
        {
            CMM_PERSON wndPopup = new CMM_PERSON();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                if (callControlName.ToUpper().Contains("REQUESTUSER"))
                {
                    parameters[0] = this.txtRequestUser.Text;
                }

                if (callControlName.ToUpper().Contains("REQUESTUSERHIST"))
                {
                    parameters[0] = this.txtRequestUserHist.Text;
                }
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(PopupPerson_Closed);
                wndPopup.Closed += new EventHandler(PopupPerson_Closed);
                this.callControlName = callControlName;
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - 사용자 검색 Control 닫았을 때, UserID, UserName Binding
        private void SetUserID(object sender)
        {
            CMM_PERSON wndPopup = sender as CMM_PERSON;
            if (wndPopup.DialogResult == MessageBoxResult.OK && this.callControlName.ToUpper().EndsWith("REQUESTUSER"))
            {
                this.txtRequestUser.Text = wndPopup.USERNAME;
                this.txtRequestUser.Tag = wndPopup.USERID;
            }

            if (wndPopup.DialogResult == MessageBoxResult.OK && this.callControlName.ToUpper().EndsWith("REQUESTUSERHIST"))
            {
                this.txtRequestUserHist.Text = wndPopup.USERNAME;
                this.txtRequestUserHist.Tag = wndPopup.USERID;
            }

            this.callControlName = string.Empty;
        }

        // 승인질
        private void ApproveRequest()
        {
            // Declarations...
            string requestResultCode = "APP";
            string mailMessage = string.Empty;
            string mailTitle = string.Empty;
            string mailRequestUserID = string.Empty;
            string mailReferrerList = string.Empty;

            try
            {
                // Validation...
                if (string.IsNullOrEmpty(this.selectedRequestNo))
                {
                    Util.MessageValidation("SFU1654");  // 선택된 요청이 없습니다.
                    return;
                }

                DataSet ds = this.dataHelper.SetApprovalOrReject(this.selectedRequestNo, LoginInfo.USERID, requestResultCode, this.selectedApprovalBusinessCode, this.txtNote.Text);

                // 메일보내기
                MailSend mailSend = new MailSend();
                mailTitle = this.selectedRequestNo + " " + mailMessage;
                mailReferrerList = (!CommonVerify.HasTableRow(this.dtReferrer)) ? string.Empty : this.dtReferrer.AsEnumerable().Select(x => x.Field<string>("REF_USERID")).Aggregate((current, next) => current + ";" + next);
                if (CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
                {
                    mailMessage = ObjectDic.Instance.GetObjectName("승인요청");         // 다음차수 안내메일
                    mailRequestUserID = ds.Tables["OUTDATA"].AsEnumerable().Select(x => x.Field<string>("APPR_USERID")).FirstOrDefault();
                }
                else
                {
                    mailMessage = ObjectDic.Instance.GetObjectName("완료");             // 완료메일
                    mailRequestUserID = this.selectedRequestUserID;
                }

                mailSend.SendMail(LoginInfo.USERID
                                , LoginInfo.USERNAME
                                , mailRequestUserID
                                , mailReferrerList
                                , mailMessage
                                , mailSend.makeBodyApp(mailTitle, this.txtNote.Text, ds.Tables["LOT_INFO"])
                                  );

                Util.MessageInfo("SFU1690");    // 승인되었습니다.
                this.GetApprovalRequestList();
                this.txtNote.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 반려질
        private void RejectRequest()
        {
            // Declaration
            string requestResultCode = "REJ";
            string mailMessage = string.Empty;
            string mailTitle = string.Empty;
            string mailRequestUserID = string.Empty;
            string mailReferrerList = string.Empty;
            try
            {
                // Validation...
                if (string.IsNullOrEmpty(this.selectedRequestNo))
                {
                    Util.MessageValidation("SFU1654");  // 선택된 요청이 없습니다.
                    return;
                }

                DataSet ds = this.dataHelper.SetApprovalOrReject(this.selectedRequestNo, LoginInfo.USERID, requestResultCode, this.selectedApprovalBusinessCode, this.txtNote.Text);

                mailMessage = ObjectDic.Instance.GetObjectName("반려");
                mailTitle = this.selectedRequestNo + " " + mailMessage;
                mailRequestUserID = this.selectedRequestUserID;
                mailReferrerList = (!CommonVerify.HasTableRow(this.dtReferrer)) ? string.Empty : this.dtReferrer.AsEnumerable().Select(x => x.Field<string>("REF_USERID")).Aggregate((current, next) => current + ";" + next);

                MailSend mailSend = new MailSend();
                mailSend.SendMail(LoginInfo.USERID
                                , LoginInfo.USERNAME
                                , mailRequestUserID
                                , mailReferrerList
                                , mailMessage
                                , mailSend.makeBodyApp(mailTitle, this.txtNote.Text, ds.Tables["LOT_INFO"])
                                  );

                Util.MessageInfo("SFU1541");  // 반려되었습니다.
                this.GetApprovalRequestList();
                this.txtNote.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.GetApprovalRequestList();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(this.dgRequestList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            this.GetApprovalRequestDetail(radioButton);
        }

        private void PopupPerson_Closed(object sender, EventArgs e)
        {
            this.SetUserID(sender);
        }

        private void btnRequestUser_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.Show_CMM_PERSON(button.Name);
        }

        private void txtRequestUser_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key != Key.Enter)
            {
                return;
            }
            this.Show_CMM_PERSON(textBox.Name);
        }

        private void btnApproveRequest_Click(object sender, RoutedEventArgs e)
        {
            // 승인하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2878"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.ApproveRequest();
                }
            });
        }

        private void btnRejectRequest_Click(object sender, RoutedEventArgs e)
        {
            // 반려하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2866"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.RejectRequest();
                }
            });
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            this.GetApproveHistory();
        }
        #endregion
    }

    internal class COM001_036_Pack_DataHelper
    {
        #region Member Variable List
        #endregion

        #region Constructor
        public COM001_036_Pack_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists..
        // 팩동에서 요청가능한 Approve Business Code만 Return (LOT Release 요청, 물품청구, 전공정 Loss, BiZWF LOT 등록 요청, BIZWF LOT 등록 취소 요청, 출고 인터락 해체 요청)
        internal DataTable GetApproveBusinessCode(string cmcdType)
        {
            DataTable dt = this.GetCommonCodeInfo(cmcdType);
            if (!CommonVerify.HasTableRow(dt))
            {
                return dt;
            }

            // 출고 인터락 해체 요청 QA에는 있고 운영에는 없음.
            if (!dt.AsEnumerable().Where(x => x.Field<string>("CBO_CODE").Trim().Equals("REQUEST_SHIPMENT_INTERLOCK")).Any())
            {
                DataRow dr = dt.NewRow();
                dr["CBO_CODE"] = "REQUEST_SHIPMENT_INTERLOCK";
                dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("REQUEST_SHIPMENT_INTERLOCK");
                dt.Rows.Add(dr);
                dt.AcceptChanges();
            }

            return dt.AsEnumerable().Where(x => x.Field<string>("CBO_CODE").Trim().Equals("LOT_RELEASE") ||
                                                x.Field<string>("CBO_CODE").Trim().Equals("LOT_REQ") ||
                                                x.Field<string>("CBO_CODE").Trim().Equals("LOT_SCRAP_YIELD") ||
                                                x.Field<string>("CBO_CODE").Trim().Equals("REQUEST_BIZWF_LOT") ||
                                                x.Field<string>("CBO_CODE").Trim().Equals("REQUEST_CANCEL_BIZWF_LOT") ||
                                                x.Field<string>("CBO_CODE").Trim().Equals("REQUEST_SHIPMENT_INTERLOCK") ||
                                                x.Field<string>("CBO_CODE").Trim().Equals("REQUEST_AGINGHOLD_RELEASE")
                                                ).CopyToDataTable();
        }

        // 순서도 호출 - AreaID
        internal DataTable GetAreaInfo()
        {
            string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["SYSTEM_ID"] = LoginInfo.SYSID;
                drRQSTDT["USERID"] = LoginInfo.USERID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - CommonCode
        internal DataTable GetCommonCodeInfo(string cmcdType)
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Approve List
        internal DataTable GetApprovalRequestList(string userID, string approveUserID)
        {
            string bizRuleName = "DA_PRD_SEL_PACK_RELEASE_REQ_SEARCH";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRQSTDT.Columns.Add("REQ_USERID", typeof(string));
                dtRQSTDT.Columns.Add("APPR_USERID", typeof(string));
                dtRQSTDT.Columns.Add("REQ_DTTM_FROM", typeof(string));
                dtRQSTDT.Columns.Add("REQ_DTTM_TO", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = null;
                drRQSTDT["APPR_BIZ_CODE"] = null;
                drRQSTDT["REQ_USERID"] = userID;
                drRQSTDT["APPR_USERID"] = approveUserID;
                drRQSTDT["REQ_DTTM_FROM"] = null;
                drRQSTDT["REQ_DTTM_TO"] = null;
                drRQSTDT["LOTID"] = null;
                drRQSTDT["PRODID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Approve List Detail
        internal DataSet GetApprovalRequestDetail(string requestNo)
        {
            DataTable dtReturn = new DataTable();
            string bizRuleName = "BR_PRD_SEL_PACK_RELEASE_REQ_DETAIL";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA_LOT,OUTDATA_APPROVER,OUTDATA_REFERRER";

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("REQ_NO", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["REQ_NO"] = requestNo;
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsOUTDATA;
        }

        // 순서도 호출 - Approve History
        internal DataTable GetApproveHistory(string areaID, string fromDate, string toDate, string userName, string approvalBusinessCode, string requestResultCode, string productID, string LOTID)
        {
            string bizRuleName = "DA_PRD_SEL_APPROVAL_APPR_HIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("FROM_DATE", typeof(string));
                dtRQSTDT.Columns.Add("TO_DATE", typeof(string));
                dtRQSTDT.Columns.Add("USERNAME", typeof(string));
                dtRQSTDT.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["FROM_DATE"] = string.IsNullOrEmpty(LOTID) ? fromDate : null;
                drRQSTDT["TO_DATE"] = string.IsNullOrEmpty(LOTID) ? toDate : null;
                drRQSTDT["USERNAME"] = string.IsNullOrEmpty(LOTID) ? userName : null;
                drRQSTDT["APPR_BIZ_CODE"] = string.IsNullOrEmpty(LOTID) ? approvalBusinessCode : null;
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["REQ_RSLT_CODE"] = string.IsNullOrEmpty(LOTID) ? requestResultCode : null;
                drRQSTDT["AREAID"] = string.IsNullOrEmpty(LOTID) ? areaID : null;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                //drRQSTDT["PRODID"] = string.IsNullOrEmpty(LOTID) ? null : productID;
                drRQSTDT["PRODID"] = string.IsNullOrEmpty(productID) ? null : productID; // 2024.11.06. 김영국 - PRODID만 조회조건 넣을 경우도 조회하도록 수정함.
                drRQSTDT["CSTID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Approve Or Reject
        internal DataSet SetApprovalOrReject(string requestNo, string approveUserID, string requestResultCode, string approvalBusinessCode, string approvalNote)
        {
            string bizRuleName = "BR_PRD_UPD_APPR_PACK";
            DataSet dsINDATA = new DataSet();
            DataTable dtINDATA = new DataTable("INDATA");
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA,LOT_INFO";

            try
            {
                dtINDATA.Columns.Add("REQ_NO", typeof(string));
                dtINDATA.Columns.Add("APPR_USERID", typeof(string));
                dtINDATA.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtINDATA.Columns.Add("APPR_NOTE", typeof(string));
                dtINDATA.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["REQ_NO"] = requestNo;
                drINDATA["APPR_USERID"] = approveUserID;
                drINDATA["APPR_RSLT_CODE"] = requestResultCode;
                drINDATA["APPR_NOTE"] = approvalNote;
                drINDATA["APPR_BIZ_CODE"] = approvalBusinessCode;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).Aggregate((current, next) => current + "," + next), outDataSetName, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsOUTDATA;
        }
        #endregion
    }
}
