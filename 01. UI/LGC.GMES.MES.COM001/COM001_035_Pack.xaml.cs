/*************************************************************************************
 Created Date : 2016.06.16
      Creator : Developer
  Description : LOT Release 요청
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  Developer : Initial Created.
  2023.02.01  정용석    : LOT Release 요청 / 요청이력 기능은 별도 Popup으로 전환, 조립동 LOT Release 요청 UI와 동일한 구성으로 변경.
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_Pack : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private COM001_035_PACK_DataHelper dataHelper = new COM001_035_PACK_DataHelper();
        private string callControlName = string.Empty;
        private string selectedRequestNo = string.Empty;
        private string selectedApprovalBusinessCode = string.Empty;
        private const string constAgingBizCode = "REQUEST_AGINGHOLD_RELEASE";
        #endregion

        #region Constructor 
        public COM001_035_Pack()
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

        // Initialize
        private void Initialize()
        {
            // 날짜 씨리즈...
            this.ldpDateFrom.ApplyTemplate();
            this.ldpDateFrom.SelectedDateTime = DateTime.Now;
            this.ldpDateTo.ApplyTemplate();
            this.ldpDateTo.SelectedDateTime = DateTime.Now;
            this.ldpDateFromHist.ApplyTemplate();
            this.ldpDateFromHist.SelectedDateTime = DateTime.Now;
            this.ldpDateToHist.ApplyTemplate();
            this.ldpDateToHist.SelectedDateTime = DateTime.Now;

            // ComboBox 씨리즈...
            Util.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaID, false, "SELECT");
            Util.SetC1ComboBox(this.dataHelper.GetAreaInfo(), this.cboAreaIDHist, false, "SELECT");
            Util.SetC1ComboBox(this.dataHelper.GetApproveBusinessCode("APPR_BIZ_CODE"), this.cboApproveBusinessCode, false, "ALL");
            Util.SetC1ComboBox(this.dataHelper.GetApproveBusinessCode("APPR_BIZ_CODE"), this.cboApproveBusinessCodeHist, false, "ALL");
            Util.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("REQ_RSLT_CODE"), this.cboRequestResultCode, false, "ALL");

            this.cboAreaID.SelectedValue = LoginInfo.CFG_AREA_ID;
            this.cboAreaIDHist.SelectedValue = LoginInfo.CFG_AREA_ID;

            this.btnCancelRequest.IsEnabled = false;

            // 임시기능 - 2023-03-07 : 물품청구 기능 DW 수율 변경 Logic 반영이 되기 전까지 막기 - 이것이 지나면 이부분은 없애도 됨.
            this.btnChargeProdLOTRequest.IsEnabled = CommonVerify.HasTableRow(this.dataHelper.GetCommonCodeInfo()) ? false : true;

            btnAgingReleaseRequest.Visibility = Visibility.Collapsed; // 2024.12.17. 김영국 - AgingReleaseRequest 버튼 비활성화.
            btnShipmentUnInterlockRequest.Visibility = Visibility.Collapsed;
        }

        // 조회 - LOT N빵
        private void GetApprovalRequestByMultiLOT(TextBox textBox)
        {
            try
            {
                string[] arrClipboard = Clipboard.GetText().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                int maxLOTIDCount = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? 500 : 100;
                string messageID = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? "SFU8217" : "SFU3695";
                if (arrClipboard.Count() > maxLOTIDCount)
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

                string LOTIDList = string.Join(",", arrClipboard);
                switch (textBox.Name.ToUpper())
                {
                    case "TXTLOTID":
                        this.GetApprovalRequestList(LOTIDList);
                        break;
                    case "TXTLOTIDHIST":
                        this.GetApprovalRequestHistoryList(LOTIDList);
                        break;
                    default:
                        break;
                }

                textBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        // 조회 - 요청 Tab
        private void GetApprovalRequestList(string LOTID = null)
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                LOTID = this.txtLotID.Text.Trim();
            }

            if (string.IsNullOrEmpty(this.cboAreaID.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU3203");  // 동은 필수입니다.
                return;
            }

            if (string.IsNullOrEmpty(this.txtRequestUser.Text))
            {
                this.txtRequestUser.Tag = string.Empty;
            }

            try
            {
                string areaID = this.cboAreaID.SelectedValue.ToString();
                string fromDate = this.ldpDateFrom.SelectedDateTime.Date.ToString("yyyyMMdd");
                string toDate = this.ldpDateTo.SelectedDateTime.Date.ToString("yyyyMMdd");
                string userName = (this.txtRequestUser.Text == null || string.IsNullOrEmpty(this.txtRequestUser.Text.ToString())) ? null : this.txtRequestUser.Text.ToString();
                string approvalbusinessCode = string.IsNullOrEmpty(this.cboApproveBusinessCode.SelectedValue.ToString()) ? null : this.cboApproveBusinessCode.SelectedValue.ToString();
                string requestResultCode = string.IsNullOrEmpty(this.cboRequestResultCode.SelectedValue.ToString()) ? null : this.cboRequestResultCode.SelectedValue.ToString();

                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgApprovalRequest);
                this.DoEvents();
                DataTable dt = this.dataHelper.GetApproveRequestList(areaID, fromDate, toDate, userName, approvalbusinessCode, requestResultCode, LOTID);
                Util.GridSetData(this.dgApprovalRequest, dt, FrameOperation);
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

        // 조회 - Popup Closed 했을 때
        private void GetApprovalRequestList(object popupFormID)
        {
            bool isValidPopupForm = false;
            string popupFormName = popupFormID.GetType().Name;

            switch (popupFormName)
            {
                case "COM001_035_REQUEST_RESERVATION":
                case "COM001_035_REQUEST_BIZWFLOT":
                case "COM001_035_REQUEST_PACK":
                case "COM001_035_REQUEST1_PACK":
                case "COM001_035_ISS_INTERUNLOCK":
                case "COM001_035_REQUEST_AGINGHOLD":
                    isValidPopupForm = true;
                    break;
                default:
                    break;
            }

            if (!isValidPopupForm)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBoxResult.None;
            switch (popupFormName)
            {
                case "COM001_035_REQUEST_RESERVATION":
                    messageBoxResult = ((popupFormID as COM001_035_REQUEST_RESERVATION)).DialogResult;
                    break;
                case "COM001_035_REQUEST_BIZWFLOT":
                    messageBoxResult = ((popupFormID as COM001_035_REQUEST_BIZWFLOT)).DialogResult;
                    break;
                case "COM001_035_REQUEST_PACK":
                    messageBoxResult = ((popupFormID as COM001_035_REQUEST_PACK)).DialogResult;
                    break;
                case "COM001_035_REQUEST1_PACK":
                    messageBoxResult = ((popupFormID as COM001_035_REQUEST1_PACK)).DialogResult;
                    break;
                case "COM001_035_ISS_INTERUNLOCK":
                    messageBoxResult = ((popupFormID as COM001_035_ISS_INTERUNLOCK)).DialogResult;
                    break;
                case "COM001_035_REQUEST_AGINGHOLD":
                    messageBoxResult = ((popupFormID as COM001_035_REQUEST_AGINGHOLD)).DialogResult;
                    break;
                default:
                    break;
            }

            if (messageBoxResult == MessageBoxResult.OK)
            {
                this.GetApprovalRequestList();
            }
        }

        // 조회 - 요청 이력 Tab
        private void GetApprovalRequestHistoryList(string LOTID = null)
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                LOTID = this.txtLotIDHist.Text.Trim();
            }

            this.selectedRequestNo = string.Empty;
            this.selectedApprovalBusinessCode = string.Empty;

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
                string userName = (this.txtRequestUserHist.Tag == null || string.IsNullOrEmpty(this.txtRequestUserHist.Tag.ToString())) ? null : this.txtRequestUserHist.Tag.ToString();
                string approvalbusinessCode = string.IsNullOrEmpty(this.cboApproveBusinessCodeHist.SelectedValue.ToString()) ? null : this.cboApproveBusinessCodeHist.SelectedValue.ToString();
                string productID = string.IsNullOrEmpty(this.txtProdID.Text) ? null : this.txtProdID.Text;

                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgApprovalRequestHist);
                Util.gridClear(this.dgLOTList);
                Util.gridClear(this.dgApproverHist);
                Util.gridClear(this.dgReferrerHist);
                this.DoEvents();
                DataTable dt = this.dataHelper.GetApproveRequestHistory(areaID, fromDate, toDate, userName, approvalbusinessCode, productID, LOTID);
                Util.GridSetData(this.dgApprovalRequestHist, dt, FrameOperation);
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

        // 조회 - 요청이력 상세정보 (요청이력 Tab)
        private void GetApprovalRequestDetail(RadioButton radioButton)
        {
            try
            {
                if (this.dgApprovalRequestHist == null || this.dgApprovalRequestHist.GetRowCount() <= 0)
                {
                    return;
                }

                this.selectedRequestNo = Util.NVC(DataTableConverter.GetValue(radioButton.DataContext, "REQ_NO"));
                this.selectedApprovalBusinessCode = Util.NVC(DataTableConverter.GetValue(radioButton.DataContext, "APPR_BIZ_CODE"));
                string selectedRequestUserID = Util.NVC(DataTableConverter.GetValue(radioButton.DataContext, "REQ_USER_ID"));
                string selectedRequestResultCode = Util.NVC(DataTableConverter.GetValue(radioButton.DataContext, "REQ_RSLT_CODE"));


                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgLOTList);
                Util.gridClear(this.dgApproverHist);
                Util.gridClear(this.dgReferrerHist);
                this.DoEvents();
                DataSet ds = this.dataHelper.GetApprovalRequestDetail(this.selectedRequestNo);

                if (CommonVerify.HasTableInDataSet(ds))
                {
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_LOT"])) Util.GridSetData(this.dgLOTList, ds.Tables["OUTDATA_LOT"], FrameOperation);
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_APPROVER"])) Util.GridSetData(this.dgApproverHist, ds.Tables["OUTDATA_APPROVER"], FrameOperation);
                    if (CommonVerify.HasTableRow(ds.Tables["OUTDATA_REFERRER"])) Util.GridSetData(this.dgReferrerHist, ds.Tables["OUTDATA_REFERRER"], FrameOperation);
                }

                // 20210723 | 김건식 | 긴급대응으로 인한 취소버튼 비활성화 로직 추후 삭제 예정
                // 20230220 | 정용석 | 승인유형코드가 BIZWF%%가 아닌것들 요청취소시 요청자 ID와 Login User가 같고, 요청상태가 Request인 것만 요청가능하게 변경 
                btnCancelRequest.IsEnabled = ("REQUEST_BIZWF_LOT.REQUEST_CANCEL_BIZWF_LOT".Contains(this.selectedApprovalBusinessCode)) ? false : true;
                btnCancelRequest.IsEnabled = (!"REQUEST_BIZWF_LOT.REQUEST_CANCEL_BIZWF_LOT".Contains(this.selectedApprovalBusinessCode) && selectedRequestResultCode.Equals("REQ") && selectedRequestUserID.Equals(LoginInfo.USERID)) ? true : false;
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

        // 요청취소버튼 활성/비활성화
        private void SetCancelRequestButtonControl()
        {
            // 20210723 | 김건식 | 긴급대응으로 인한 취소버튼 비활성화 로직 추후 삭제 예정
            btnCancelRequest.IsEnabled = ("REQUEST_BIZWF_LOT.REQUEST_CANCEL_BIZWF_LOT".Contains(this.selectedApprovalBusinessCode)) ? false : true;
            btnCancelRequest.IsEnabled = (!"REQUEST_BIZWF_LOT.REQUEST_CANCEL_BIZWF_LOT".Contains(this.selectedApprovalBusinessCode) && selectedApprovalBusinessCode.Equals("REQ")) ? true : false;
        }

        // 요청취소 (요청이력 Tab)
        private void CancelApprovalRequest()
        {
            try
            {
                if (!this.dataHelper.CancelApprovalRequest(this.selectedRequestNo, this.selectedApprovalBusinessCode))
                {
                    return;
                }

                // 메일보내기 - 최초승인자랑 참조자 전체
                string refererList = string.Empty;
                if (CommonVerify.HasTableRow(DataTableConverter.Convert(this.dgReferrerHist.ItemsSource)))
                {
                    refererList = DataTableConverter.Convert(this.dgReferrerHist.ItemsSource).AsEnumerable().Select(x => x.Field<string>("REF_USERID")).Aggregate((current, next) => current + ";" + next);
                }

                MailSend mailSend = new MailSend();
                mailSend.SendMail(LoginInfo.USERID
                                , LoginInfo.USERNAME
                                , DataTableConverter.Convert(this.dgApproverHist.ItemsSource).AsEnumerable().Select(x => x.Field<string>("APPR_USERID")).FirstOrDefault()
                                , refererList
                                , ObjectDic.Instance.GetObjectName("요청취소")
                                , mailSend.makeBodyApp(this.selectedRequestNo + " " + ObjectDic.Instance.GetObjectName("요청취소"), string.Empty)
                                  );

                this.GetApprovalRequestHistoryList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - 오만가지 Popup (요청 Tab)
        /// <summary>
        /// Grid Click시 호출
        /// </summary>
        /// <param name="c1DataGrid"></param>
        private void ShowPopUp(C1DataGrid c1DataGrid)
        {
            if (c1DataGrid.CurrentRow == null || c1DataGrid.GetRowCount() <= 0)
            {
                return;
            }

            if (!c1DataGrid.CurrentColumn.Name.Equals("REQ_NO"))
            {
                return;
            }

            string selectedRequestNo = Util.NVC(DataTableConverter.GetValue(c1DataGrid.CurrentRow.DataItem, "REQ_NO"))?.ToString();
            string selectedApproveBusinessCode = Util.NVC(DataTableConverter.GetValue(c1DataGrid.CurrentRow.DataItem, "APPR_BIZ_CODE"))?.ToString();
            string selectedRequestUserID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.CurrentRow.DataItem, "REQ_USER_ID"))?.ToString();
            string selectedRequestResultCode = Util.NVC(DataTableConverter.GetValue(c1DataGrid.CurrentRow.DataItem, "REQ_RSLT_CODE"))?.ToString();

            if (selectedRequestUserID != LoginInfo.USERID || selectedRequestResultCode != "REQ")
            {
                this.Show_COM001_035_READ(selectedRequestNo, selectedRequestResultCode);
                return;
            }

            // Login 사용자의 요청정보중에 상태가 요청인 것인 경우
            if (selectedRequestUserID == LoginInfo.USERID && selectedRequestResultCode == "REQ")
            {
                switch (selectedApproveBusinessCode.ToUpper())
                {
                    case "LOT_RELEASE":                 // LOT Release 요청
                    case "LOT_REQ":                     // 물품청구
                        this.Show_COM001_035_READ(selectedRequestNo, selectedRequestResultCode);
                        break;
                    case "REQUEST_BIZWF_LOT":           // BIZWF LOT 요청
                    case "REQUEST_CANCEL_BIZWF_LOT":    // BIZWF LOT 요청 취소
                        this.Show_COM001_035_REQUEST_BIZWFLOT(selectedRequestNo, selectedApproveBusinessCode);
                        break;
                    case constAgingBizCode: // "REQUEST_AGINGHOLD_RELEASE":
                        //2023-05-09 김선준 추가
                        this.Show_COM001_035_REQUEST_AGINGHOLD(selectedRequestNo, selectedApproveBusinessCode);
                        break;
                    default:
                        break;
                }
            }
        }

        // Popup - 요청정보
        private void Show_COM001_035_READ(params string[] arrParameters)
        {
            COM001_035_READ wndPopup = new COM001_035_READ();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = arrParameters[0];
                parameters[1] = arrParameters[1];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - LOT Release 요청
        private void Show_COM001_035_REQUEST_PACK(params string[] arrParameters)
        {
            COM001_035_REQUEST_PACK wndPopup = new COM001_035_REQUEST_PACK();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = arrParameters[0];
                parameters[1] = arrParameters[1];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - 물품청구
        private void Show_COM001_035_REQUEST1_PACK(params string[] arrParameters)
        {
            COM001_035_REQUEST1_PACK wndPopup = new COM001_035_REQUEST1_PACK();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = arrParameters[0];
                parameters[1] = arrParameters[1];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - BIZWF LOT 요청, BIZWF LOT 요청 취소
        private void Show_COM001_035_REQUEST_BIZWFLOT(params string[] arrParameters)
        {
            COM001_035_REQUEST_BIZWFLOT wndPopup = new COM001_035_REQUEST_BIZWFLOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = arrParameters[0];
                parameters[1] = arrParameters[1];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - 출고 인터락 해체 요청
        private void Show_COM001_035_ISS_INTERUNLOCK(params string[] arrParameters)
        {
            COM001_035_ISS_INTERUNLOCK wndPopup = new COM001_035_ISS_INTERUNLOCK();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - AgingHold Release 요청
        private void Show_COM001_035_REQUEST_AGINGHOLD(params string[] arrParameters)
        {
            COM001_035_REQUEST_AGINGHOLD wndPopup = new COM001_035_REQUEST_AGINGHOLD();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = arrParameters[0];
                parameters[1] = arrParameters[1];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        // Popup - 전공정 Loss
        private void Show_COM001_035_REQUEST_YIELD_PACK(params string[] arrParameters)
        {
            COM001_035_REQUEST_YIELD_PACK wndPopup = new COM001_035_REQUEST_YIELD_PACK();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                parameters[0] = arrParameters[0];
                parameters[1] = arrParameters[1];
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(Popup_Closed);
                wndPopup.Closed += new EventHandler(Popup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
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
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.GetApprovalRequestList();
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            this.GetApprovalRequestHistoryList();
        }

        private void btnRequestUser_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.Show_CMM_PERSON(button.Name);
        }

        private void btnCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            // 취소 하시겠습니까?
            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.CancelApprovalRequest();
                }
            });
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

        private void dgApprovalRequest_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1DataGrid c1DataGrid = sender as C1DataGrid;
            c1DataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("REQ_NO"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
            }));
        }

        private void dgApprovalRequest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = sender as C1DataGrid;
            this.ShowPopUp(c1DataGrid);
        }

        private void btnLOTReleaseRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Show_COM001_035_REQUEST_PACK("NEW", "LOT_RELEASE");
        }

        private void btnChargeProdLOTRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Show_COM001_035_REQUEST1_PACK("NEW", "LOT_REQ");
        }

        private void btnBizWFLOTRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Show_COM001_035_REQUEST_BIZWFLOT("NEW", "REQUEST_BIZWF_LOT");
        }

        private void btnBizWFLOTCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Show_COM001_035_REQUEST_BIZWFLOT("NEW", "REQUEST_CANCEL_BIZWF_LOT");
        }

        private void btnShipmentUnInterlockRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Show_COM001_035_ISS_INTERUNLOCK(null);
        }

        private void btnScrapYieldRequest_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = this.dataHelper.GetStockEndInfo();
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            decimal diffDay = 0;
            string closeYN = string.Empty;

            foreach (DataRowView drv in dt.AsDataView())
            {
                diffDay = Convert.ToDecimal(drv["DIFF_DAY"]);
                closeYN = drv["CLOSE_INFO"].ToString().Equals("Y") ? drv["CLOSE_INFO"].ToString() : null;
            }

            this.Show_COM001_035_REQUEST_YIELD_PACK(diffDay.ToString(), closeYN);
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            this.GetApprovalRequestList(sender);
        }

        private void PopupPerson_Closed(object sender, EventArgs e)
        {
            this.SetUserID(sender);
        }

        private void rdoApprovalRequestHist_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            this.GetApprovalRequestDetail(radioButton);
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key != Key.Enter)
            {
                return;
            }
            this.GetApprovalRequestList();
        }

        private void txtLotIDHist_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key != Key.Enter)
            {
                return;
            }
            this.GetApprovalRequestHistoryList();
        }

        private void txtLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.GetApprovalRequestByMultiLOT(textBox);
            }
        }

        private void txtLotIDHist_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.GetApprovalRequestByMultiLOT(textBox);
            }
        }

        /// <summary>
        /// Aging Hold 해제 요청
        /// 2023-05-09 김선준
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAgingReleaseRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Show_COM001_035_REQUEST_AGINGHOLD("NEW", constAgingBizCode);            
        }
        #endregion

    }

    internal class COM001_035_PACK_DataHelper
    {
        #region Member Variable List
        #endregion

        #region Constructor
        public COM001_035_PACK_DataHelper()
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

        // 순서도 호출 - Approve Request
        internal DataTable GetApproveRequestList(string areaID, string fromDate, string toDate, string userName, string approvalBusinessCode, string requestResultCode, string LOTID)
        {
            string bizRuleName = "DA_PRD_SEL_APPROVAL_REQ_LIST";
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

        // 순서도 호출 - Approve Request History
        internal DataTable GetApproveRequestHistory(string areaID, string fromDate, string toDate, string userID, string approvalBusinessCode, string productID, string LOTID)
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
                drRQSTDT["AREAID"] = string.IsNullOrEmpty(LOTID) ? areaID : null;
                drRQSTDT["APPR_BIZ_CODE"] = string.IsNullOrEmpty(LOTID) ? approvalBusinessCode : null;
                drRQSTDT["REQ_USERID"] = string.IsNullOrEmpty(LOTID) ? userID : null;
                drRQSTDT["APPR_USERID"] = null;
                drRQSTDT["REQ_DTTM_FROM"] = string.IsNullOrEmpty(LOTID) ? fromDate : null;
                drRQSTDT["REQ_DTTM_TO"] = string.IsNullOrEmpty(LOTID) ? toDate : null;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                drRQSTDT["PRODID"] = string.IsNullOrEmpty(LOTID) ? productID : null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Approve Request History
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

        // 순서도 호출 - 전공정 Loss ERP 마감 정보?
        internal DataTable GetStockEndInfo()
        {
            string bizRuleName = "DA_PRD_SEL_STOCK_END_INFO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Cancel Approval Request
        internal bool CancelApprovalRequest(string requestNo, string approvalBusinessCode)
        {
            bool returnValue = false;

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

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["REQ_NO"] = requestNo;
                drINDATA["APPR_USERID"] = LoginInfo.USERID;
                drINDATA["APPR_RSLT_CODE"] = "DEL";
                drINDATA["APPR_BIZ_CODE"] = approvalBusinessCode;
                drINDATA["LANGID"] = LoginInfo.LANGID;

                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).Aggregate((current, next) => current + "," + next), outDataSetName, dsINDATA);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 순서도 호출 - CommonCode 정보
        internal DataTable GetCommonCodeInfo()
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "PACK_APPLY_LINE_BY_UI";
                drRQSTDT["CBO_CODE"] = "COM001_035_PACK";
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

        #endregion
    }
}