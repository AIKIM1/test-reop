/*************************************************************************************
 Created Date : 2023.01.28
      Creator : 정용석
  Description : Pack Lot Release 요청 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.28 정용석 : Initial Created.
  2023.02.22 정용석 : [C20220802-000459] - 승인요청할때 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
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
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_PACK : C1Window, IWorkArea
    {
        #region Member Variable List...
        private COM001_035_REQUEST_PACK_DataHelper dataHelper = new COM001_035_REQUEST_PACK_DataHelper();
        private string callControlName = string.Empty;
        private string requestNo = string.Empty;
        private string approvalBusinessCode = string.Empty;

        private DataGridRowHeaderPresenter dataGridRowHeaderPresennterPreHold = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private DataGridRowHeaderPresenter dataGridRowHeaderPresennterPreReleaseRequest = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private CheckBox chkHoldAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        private CheckBox chkReleaseRequestAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public COM001_035_REQUEST_PACK()
        {
            InitializeComponent();
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
            object[] arrParameters = C1WindowExtension.GetParameters(this);
            if (arrParameters == null || arrParameters.Length <= 0)
            {
                return;
            }

            this.requestNo = arrParameters[0].ToString();
            this.approvalBusinessCode = arrParameters[1].ToString();
            this.ldpFromDate.SelectedDateTime = (DateTime)DateTime.Now.AddDays(-7);
            this.ldpToDate.SelectedDateTime = (DateTime)DateTime.Now;
            this.ldpFromDate.ApplyTemplate();
            this.ldpToDate.ApplyTemplate();

            Util.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboEquipmentSegment, false, "ALL");
            Util.SetC1ComboBox(this.dataHelper.GetActivityReasonInfo("UNHOLD_LOT"), this.cboActivityReasonCode, false, "SELECT");
            this.cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
        }

        // 조회
        private void SearchProcess(string LOTID = null)
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                LOTID = this.txtLOTID.Text.Trim();
            }

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgLOTList);
                Util.gridClear(this.dgReleaseRequestLOTList);
                Util.gridClear(this.dgApprover);
                Util.gridClear(this.dgReferrer);
                this.DoEvents();

                string productID = string.IsNullOrEmpty(this.cboProduct.SelectedValue.ToString()) ? null : this.cboProduct.SelectedValue.ToString();
                string fromDate = this.ldpFromDate.SelectedDateTime.Date.ToString("yyyyMMdd");
                string toDate = this.ldpToDate.SelectedDateTime.Date.ToString("yyyyMMdd");
                string equipmentSegmentID = string.IsNullOrEmpty(this.cboEquipmentSegment.SelectedValue.ToString()) ? null : this.cboEquipmentSegment.SelectedValue.ToString();
                string areaID = LoginInfo.CFG_AREA_ID;
                string productClassCode = string.IsNullOrEmpty(this.cboProductClassCode.SelectedValue.ToString()) ? null : this.cboProductClassCode.SelectedValue.ToString();
                string projectModel = string.IsNullOrEmpty(this.cboProjectModel.SelectedValue.ToString()) ? null : this.cboProjectModel.SelectedValue.ToString();
                string approveRequestFlag = "Y";        // 이미 RELEASE 승인 요청중인 LOT 제거하여 보여줌

                DataTable dt = this.dataHelper.GetHoldListByDataAccess(productID, fromDate, toDate, equipmentSegmentID, areaID, productClassCode, projectModel, approveRequestFlag, LOTID);
                if (CommonVerify.HasTableRow(dt))
                {
                    // 불건전 Data 추출
                    var queryUnwholesome = dt.AsEnumerable().Where(x => string.IsNullOrEmpty(x.Field<string>("CHECK_LOTID")) ||
                                                                        x.Field<string>("HOLD_YN") == "N" ||
                                                                        x.Field<string>("WIPSTAT") == "TERM" ||
                                                                        !string.IsNullOrEmpty(x.Field<string>("REQ_NO")));

                    // 불건전 Data와 건전 Data 분리
                    var queryWholeSome = dt.AsEnumerable().Except(queryUnwholesome);

                    // 불건전 Data는 불건전 Popup창에 집어넣고, 건전 Data는 대상 Grid에 집어넣기.
                    if (queryUnwholesome.Count() > 0)
                    {
                        this.Show_COM001_035_PACK_EXCEPTION_POPUP(queryUnwholesome.CopyToDataTable(), "RELEASE_LOT");
                    }

                    if (queryWholeSome.Count() > 0)
                    {
                        Util.GridSetData(this.dgLOTList, queryWholeSome.CopyToDataTable(), FrameOperation);
                    }
                }

                this.txtLOTID.Text = string.Empty;
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

        // LOT Release Request
        private void ReleaseRequest()
        {
            // Declarations...
            string sendMailReferrerUserID = string.Empty;

            // 특수문자 제거 정규식
            string pattern = @"[^\p{L}\p{N}]";  // 문자 및 숫자만 인식 
            int txtLength = Regex.Replace(this.txtNote.Text.ToString(), pattern, "").Length;  //특수문자를 제외한 문자 및 숫자의 길이

            // Validations...
            if (this.dgReleaseRequestLOTList == null || this.dgReleaseRequestLOTList.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1748");  // 요청 목록이 필요합니다.
                return;
            }
            if (this.dgApprover.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1692");  // 승인자가 필요합니다.
                return;
            }
            if (this.cboActivityReasonCode.SelectedIndex < 0 || string.IsNullOrEmpty(this.cboActivityReasonCode.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU3333");  // 선택오류 : 홀드해제사유(필수조건) 콤보를 선택하세요
                return;
            }
            if (this.txtNote.Text.Equals("\r\n") || string.IsNullOrEmpty(this.txtNote.Text))
            {
                Util.MessageValidation("SFU1590");  // 비고를 입력 하세요.
                return;
            }
            else if (txtLength < 4)
            {
                Util.MessageValidation("SFU10014"); // ※ 아래 예시와 같이 상세하게 Hold 해제 사유 작성 바랍니다.~
                return;
            }

            try
            {
                // INDATA
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("REQ_NOTE", typeof(string));
                dtINDATA.Columns.Add("RESNCODE", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("COST_CNTR_ID", typeof(string));
                dtINDATA.Columns.Add("UNHOLD_CALDATE", typeof(DateTime));
                dtINDATA.Columns.Add("CAUSE_EQSGID", typeof(string));
                dtINDATA.Columns.Add("CAUSE_PRODID", typeof(string));
                dtINDATA.Columns.Add("REQ_LOTTYPE", typeof(string));
                dtINDATA.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["APPR_BIZ_CODE"] = "LOT_RELEASE";
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["REQ_NOTE"] = this.txtNote.Text;
                drINDATA["RESNCODE"] = this.cboActivityReasonCode.SelectedValue.ToString();
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtINDATA.Rows.Add(drINDATA);

                // INLOT
                DataTable dtINLOT = new DataTable("INLOT");
                dtINLOT.Columns.Add("LOTID", typeof(string));
                dtINLOT.Columns.Add("WIPQTY", typeof(string));
                dtINLOT.Columns.Add("WIPQTY2", typeof(string));
                dtINLOT.Columns.Add("WOID", typeof(string));
                dtINLOT.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtINLOT.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                dtINLOT.Columns.Add("BIZ_WF_ITEM_SEQNO", typeof(string));
                dtINLOT.Columns.Add("BIZ_WF_SLOC_ID", typeof(string));
                dtINLOT.Columns.Add("BIZ_WF_LOT_SEQNO", typeof(string));
                dtINLOT.Columns.Add("PRODID", typeof(string));
                dtINLOT.Columns.Add("PRODNAME", typeof(string));
                dtINLOT.Columns.Add("MODELID", typeof(string));
                foreach (DataRowView drv in DataTableConverter.Convert(this.dgReleaseRequestLOTList.ItemsSource).AsDataView())
                {
                    DataRow drINLOT = dtINLOT.NewRow();
                    drINLOT["LOTID"] = drv["LOTID"].ToString();
                    drINLOT["WOID"] = drv["WOID"].ToString();
                    drINLOT["PRODID"] = drv["PRODID"].ToString();
                    drINLOT["MODELID"] = null;
                    dtINLOT.Rows.Add(drINLOT);
                }

                // INPROG
                DataTable dtINPROG = new DataTable("INPROG");
                dtINPROG.Columns.Add("APPR_SEQS", typeof(string));
                dtINPROG.Columns.Add("APPR_USERID", typeof(string));
                foreach (DataRowView drv in DataTableConverter.Convert(this.dgApprover.ItemsSource).AsDataView())
                {
                    DataRow drINPROG = dtINPROG.NewRow();
                    drINPROG["APPR_SEQS"] = drv["APPR_SEQS"].ToString();
                    drINPROG["APPR_USERID"] = drv["USERID"].ToString();
                    dtINPROG.Rows.Add(drINPROG);
                }

                // INREF
                DataTable dtINREF = new DataTable("INREF");
                dtINREF.Columns.Add("REF_USERID", typeof(string));
                foreach (DataRowView drv in DataTableConverter.Convert(this.dgReferrer.ItemsSource).AsDataView())
                {
                    DataRow drINREF = dtINREF.NewRow();
                    drINREF["REF_USERID"] = drv["USERID"].ToString();
                    dtINREF.Rows.Add(drINREF);
                }

                DataSet ds = this.dataHelper.SetReleaseRequest(dtINDATA, dtINLOT, dtINPROG, dtINREF);

                // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
                if (CommonVerify.HasTableInDataSet(ds) && CommonVerify.HasTableRow(ds.Tables["OUTDATA_LOT"]))
                {
                    this.Show_COM001_035_PACK_EXCEPTION_POPUP(ds.Tables["OUTDATA_LOT"], "APPR_BIZ");
                    return;
                }

                if (CommonVerify.HasTableInDataSet(ds) && ds.Tables["OUTDATA"].Rows.Count > 0)
                {
                    // 메일보내기 - 최초승인자랑 참조자 전체
                    string requestNo = ds.Tables["OUTDATA"].AsEnumerable().Select(x => x.Field<string>("REQ_NO")).FirstOrDefault();
                    string referrerList = string.Empty;
                    if (CommonVerify.HasTableRow(DataTableConverter.Convert(this.dgReferrer.ItemsSource)))
                    {
                        referrerList = DataTableConverter.Convert(this.dgReferrer.ItemsSource).AsEnumerable().Select(x => x.Field<string>("USERID")).Aggregate((current, next) => current + ";" + next);
                    }

                    MailSend mailSend = new MailSend();
                    mailSend.SendMail(LoginInfo.USERID
                                    , LoginInfo.USERNAME
                                    , DataTableConverter.Convert(this.dgApprover.ItemsSource).AsEnumerable().Select(x => x.Field<string>("USERID")).FirstOrDefault()
                                    , referrerList
                                    , ObjectDic.Instance.GetObjectName("승인요청")
                                    , mailSend.makeBodyApp(requestNo + " " + this.Header, this.txtNote.Text, dtINLOT)
                                      );

                    Util.AlertInfo("SFU1747");  // 요청되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - 사용자정보
        private void Show_COM001_035_PACK_PERSON(string callControlName)
        {
            COM001_035_PACK_PERSON wndPopup = new COM001_035_PACK_PERSON();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                if (callControlName.ToUpper().Contains("APPROVER"))
                {
                    parameters[0] = this.txtApprover.Text;
                    parameters[1] = "APPROVER";
                }

                if (callControlName.ToUpper().Contains("REFERRER"))
                {
                    parameters[0] = this.txtReferrer.Text;
                    parameters[1] = "REFERRER";
                }
                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed -= new EventHandler(PopupPerson_Closed);
                wndPopup.Closed += new EventHandler(PopupPerson_Closed);
                this.callControlName = callControlName;
                wndPopup.ShowModal();
                wndPopup.CenterOnScreen();
                wndPopup.BringToFront();
            }
        }

        // Popup - 사용자 검색 Control 닫았을 때, UserID, UserName Binding
        private void SetPerson(object sender)
        {
            // Declarations...
            string callControlName = string.Empty;
            string userID = string.Empty;
            string userName = string.Empty;
            string departmentID = string.Empty;
            string departmentName = string.Empty;

            try
            {
                COM001_035_PACK_PERSON wndPopup = sender as COM001_035_PACK_PERSON;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    callControlName = wndPopup.GUBUN;
                    userID = wndPopup.USERID;
                    userName = wndPopup.USERNAME;
                    departmentID = wndPopup.DEPTID;
                    departmentName = wndPopup.DEPTNAME;
                }

                if (callControlName.ToUpper().Equals("APPROVER"))
                {
                    this.SetApprover(userID, userName, departmentID, departmentName);
                    this.txtApprover.Text = string.Empty;
                }
                else if (callControlName.ToUpper().Equals("REFERRER"))
                {
                    this.SetReferrer(userID, userName, departmentID, departmentName);
                    this.txtReferrer.Text = string.Empty;
                }

                this.callControlName = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetApprover(string userID, string userName, string departmentID, string departmentName)
        {
            List<Tuple<string, string, string, string>> lstApprover = new List<Tuple<string, string, string, string>>();
            lstApprover.Add(new Tuple<string, string, string, string>(userID, userName, departmentName, "POPUP_USER"));

            try
            {
                // 승인자 리스트에 집어넣은게 없으면
                if (this.dgApprover == null || this.dgApprover.GetRowCount() <= 0)
                {
                    var queryPerson = lstApprover.Select((x, approvalSequence) => new
                    {
                        APPR_SEQS = ++approvalSequence,
                        USERID = x.Item1,
                        USERNAME = x.Item2,
                        DEPTNAME = x.Item3,
                        USER_TYPE = x.Item4
                    }).OrderBy(x => x.APPR_SEQS);

                    Util.GridSetData(this.dgApprover, Util.queryToDataTable(queryPerson), FrameOperation);
                    return;
                }

                // 승인자 리스트에 집어넣은게 있다면
                DataTable dtApprover = DataTableConverter.Convert(this.dgApprover.ItemsSource);
                if (dtApprover.AsEnumerable().Where(x => x.Field<string>("USERID") == userID).Count() > 0)
                {
                    return;
                }

                // 승인자 Insert
                var query = dtApprover.AsEnumerable().OrderBy(x => x.Field<string>("USER_TYPE")).ThenBy(x => x.Field<int>("APPR_SEQS")).Select(x => new
                {
                    USERID = x.Field<string>("USERID"),
                    USERNAME = x.Field<string>("USERNAME"),
                    DEPTNAME = x.Field<string>("DEPTNAME"),
                    USER_TYPE = x.Field<string>("USER_TYPE")
                }).Union(lstApprover.Select(y => new
                {
                    USERID = y.Item1,
                    USERNAME = y.Item2,
                    DEPTNAME = y.Item3,
                    USER_TYPE = y.Item4
                })).OrderBy(z => z.USER_TYPE).Select((z, approvalSequence) => new
                {
                    APPR_SEQS = ++approvalSequence,
                    USERID = z.USERID,
                    USERNAME = z.USERNAME,
                    DEPTNAME = z.DEPTNAME,
                    USER_TYPE = z.USER_TYPE
                }).OrderBy(z => z.USER_TYPE).ThenBy(z => z.APPR_SEQS);

                Util.GridSetData(this.dgApprover, Util.queryToDataTable(query), FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 참조자 그리드에 집어넣기
        private void SetReferrer(string userID, string userName, string departmentID, string departmentName)
        {
            List<Tuple<string, string, string>> lstReferrer = new List<Tuple<string, string, string>>();
            lstReferrer.Add(new Tuple<string, string, string>(userID, userName, departmentName));

            try
            {
                // 참조자 리스트에 집어넣은게 없으면
                if (this.dgReferrer == null || this.dgReferrer.GetRowCount() <= 0)
                {
                    var queryPerson = lstReferrer.Select(x => new
                    {
                        USERID = x.Item1,
                        USERNAME = x.Item2,
                        DEPTNAME = x.Item3
                    });

                    Util.GridSetData(this.dgReferrer, Util.queryToDataTable(queryPerson), FrameOperation);
                    return;
                }

                // 참조자 리스트에 집어넣은게 있다면
                DataTable dtReferrer = DataTableConverter.Convert(this.dgReferrer.ItemsSource);
                if (dtReferrer.AsEnumerable().Where(x => x.Field<string>("USERID") == userID).Count() > 0)
                {
                    return;
                }

                // 참조자 Insert
                var query = dtReferrer.AsEnumerable().Select(x => new
                {
                    USERID = x.Field<string>("USERID"),
                    USERNAME = x.Field<string>("USERNAME"),
                    DEPTNAME = x.Field<string>("DEPTNAME")
                }).Union(lstReferrer.Select(y => new
                {
                    USERID = y.Item1,
                    USERNAME = y.Item2,
                    DEPTNAME = y.Item3
                })).Select(z => new
                {
                    USERID = z.USERID,
                    USERNAME = z.USERNAME,
                    DEPTNAME = z.DEPTNAME
                });

                Util.GridSetData(this.dgReferrer, Util.queryToDataTable(query), FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // LOT 이Grid에서 저Grid로 이동
        private void LOTMoveGrid(bool isMoveDown, C1DataGrid sourceC1DataGrid, C1DataGrid targetC1DataGrid)
        {
            // Validation
            DataTable dtSource = DataTableConverter.Convert(sourceC1DataGrid.ItemsSource);
            DataTable dtTarget = DataTableConverter.Convert(targetC1DataGrid.ItemsSource);

            if (sourceC1DataGrid == null || sourceC1DataGrid.GetRowCount() <= 0)
            {
                return;
            }

            // MES 2.0 CHK 컬럼 Bool 오류 Patch
            if (dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true) ||
                                                   x.Field<bool>("CHK").ToString().ToUpper() == "TRUE").Count() <= 0)
            {
                return;
            }

            DataTable dtUnion = new DataTable();
            if (CommonVerify.HasTableRow(dtTarget))
            {
                // Target Grid에 데이터가 있을 경우 Target Grid Data Union Move Data
                dtUnion = dtTarget.AsEnumerable().Union(dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true) || x.Field<bool>("CHK").ToString().ToUpper() == "TRUE")).CopyToDataTable();// MES 2.0 CHK 컬럼 Bool 오류 Patch
                // Target Data Check 상태 유지
                foreach (DataRow drUnion in dtUnion.Rows)
                {
                    if (dtTarget.AsEnumerable().Where(x => x.Field<string>("LOTID") == drUnion["LOTID"].ToString()).Count() <= 0)
                    {
                        drUnion["CHK"] = false;
                    }
                }
            }
            else
            {
                // Target Grid에 데이터가 없을 경우 Move Data Insert
                dtUnion = dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true) || x.Field<bool>("CHK").ToString().ToUpper() == "TRUE").CopyToDataTable();// MES 2.0 CHK 컬럼 Bool 오류 Patch
                foreach (DataRow drUnion in dtUnion.Rows)
                {
                    drUnion["CHK"] = false;
                }
            }
            Util.GridSetData(targetC1DataGrid, dtUnion, FrameOperation);

            // Source Grid에서 Move 되는 것들 삭제
            sourceC1DataGrid.CanUserRemoveRows = true;
            sourceC1DataGrid.BeginEdit();
            for (int rowIndex = sourceC1DataGrid.GetRowCount() - 1; rowIndex >= 0; rowIndex--)
            {
                if (Convert.ToBoolean(DataTableConverter.GetValue(sourceC1DataGrid.Rows[rowIndex].DataItem, "CHK")))
                {
                    sourceC1DataGrid.SelectedIndex = rowIndex;
                    sourceC1DataGrid.RemoveRow(rowIndex);
                }
            }
            sourceC1DataGrid.EndEdit();
            sourceC1DataGrid.CanUserRemoveRows = false;

            // 승인자 처리
            if (isMoveDown)
            {
                this.ApproverInsert(dtSource);
            }
            else
            {
                this.ApproverDelete(dtSource);
            }

            this.chkHoldAll.IsChecked = false;
            this.chkReleaseRequestAll.IsChecked = false;
        }

        // Hold User 승인자 Grid에다 추가
        private void ApproverInsert(DataTable dtSource)
        {
            // Declarations...
            DataTable dtApprover = new DataTable();
            DataTable dtApproverNew = new DataTable();

            // Validation
            var queryApproverNew = dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true) || x.Field<bool>("CHK").ToString().ToUpper() == "TRUE").GroupBy(grp => grp.Field<string>("HOLD_USERID")).Select(x => x.Key);
            if (queryApproverNew.Count() <= 0)
            {
                return;
            }

            // Start
            foreach (C1.WPF.DataGrid.DataGridColumn dataGridColumn in this.dgApprover.Columns)
            {
                dtApproverNew.Columns.Add(dataGridColumn.Name, typeof(string));
            }

            // 추가될 인간들 정보 Insert
            foreach (var item in queryApproverNew)
            {
                DataTable dt = this.dataHelper.GetPerson(item);
                if (CommonVerify.HasTableRow(dt))
                {
                    foreach (DataRowView drv in dt.AsDataView())
                    {
                        DataRow drApproverNew = dtApproverNew.NewRow();
                        drApproverNew["USERID"] = drv["USERID"];
                        drApproverNew["USERNAME"] = drv["USERNAME"];
                        drApproverNew["DEPTNAME"] = drv["DEPTNAME"];
                        drApproverNew["USER_TYPE"] = "HOLD_USER";
                        dtApproverNew.Rows.Add(drApproverNew);
                    }
                }
            }

            if (!CommonVerify.HasTableRow(dtApproverNew))
            {
                return;
            }

            // 승인자 Grid에 데이터가 있으면 Union 하고 승인차수 정리 후 DataBinding, 그렇지 않으면 그냥 Insert
            if (this.dgApprover.GetRowCount() <= 0)
            {
                // 승인차수 정리
                var queryResult = dtApproverNew.AsEnumerable().OrderBy(z => z.Field<string>("USER_TYPE")).Select((z, approvalSequence) => new
                {
                    APPR_SEQS = ++approvalSequence,
                    USERID = z.Field<string>("USERID"),
                    USERNAME = z.Field<string>("USERNAME"),
                    DEPTNAME = z.Field<string>("DEPTNAME"),
                    USER_TYPE = z.Field<string>("USER_TYPE")
                });

                Util.GridSetData(this.dgApprover, Util.queryToDataTable(queryResult.ToList()), FrameOperation);
            }
            else
            {
                // Popup에서 추가된 User는 User_Type을 Hold User로 변경
                dtApprover = DataTableConverter.Convert(this.dgApprover.ItemsSource);
                dtApprover.AsEnumerable().Where(x => dtApproverNew.AsEnumerable().Where(y => y.Field<string>("USERID") == x.Field<string>("USERID")).Any()).ToList<DataRow>().ForEach(dr => dr["USER_TYPE"] = "HOLD_USER");
                dtApprover.AcceptChanges();

                // 승인자 Union
                var query1 = dtApprover.AsEnumerable().Select(x => new
                {
                    USERID = x.Field<string>("USERID"),
                    USERNAME = x.Field<string>("USERNAME"),
                    DEPTNAME = x.Field<string>("DEPTNAME"),
                    USER_TYPE = x.Field<string>("USER_TYPE")
                });

                var query2 = dtApproverNew.AsEnumerable().Select(y => new
                {
                    USERID = y.Field<string>("USERID"),
                    USERNAME = y.Field<string>("USERNAME"),
                    DEPTNAME = y.Field<string>("DEPTNAME"),
                    USER_TYPE = y.Field<string>("USER_TYPE")
                });

                // 승인차수 정리
                var queryResult = query1.Union(query2).OrderBy(z => z.USER_TYPE).Select((z, approvalSequence) => new
                {
                    APPR_SEQS = ++approvalSequence,
                    USERID = z.USERID,
                    USERNAME = z.USERNAME,
                    DEPTNAME = z.DEPTNAME,
                    USER_TYPE = z.USER_TYPE
                });

                Util.GridSetData(this.dgApprover, Util.queryToDataTable(queryResult.ToList()), FrameOperation);
            }
        }

        // Hold User 승인자 Grid에서 삭제
        private void ApproverDelete(DataTable dtSource)
        {
            // Declarations...
            DataTable dtApprover = new DataTable();
            if (this.dgApprover.GetRowCount() <= 0)
            {
                return;
            }

            dtApprover = DataTableConverter.Convert(this.dgApprover.ItemsSource);
            var queryHoldUser = dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(false) || x.Field<bool>("CHK").ToString().ToUpper() == "FALSE").GroupBy(grp => grp.Field<string>("HOLD_USERID")).Select(x => x.Key);
            dtApprover.AsEnumerable().Where(x => x.Field<string>("USER_TYPE").Equals("HOLD_USER") && !queryHoldUser.Where(y => y.Equals(x.Field<string>("USERID"))).Any()).ToList<DataRow>().ForEach(dr => dr.Delete());
            dtApprover.AcceptChanges();

            if (CommonVerify.HasTableRow(dtApprover))
            {
                // 승인차수 정리
                var query = dtApprover.AsEnumerable().OrderBy(x => x.Field<string>("USER_TYPE")).ThenBy(x => x.Field<int>("APPR_SEQS")).Select((x, approvalSequence) => new
                {
                    APPR_SEQS = ++approvalSequence,
                    USERID = x.Field<string>("USERID"),
                    USERNAME = x.Field<string>("USERNAME"),
                    DEPTNAME = x.Field<string>("DEPTNAME"),
                    USER_TYPE = x.Field<string>("USER_TYPE")
                });

                Util.GridSetData(this.dgApprover, Util.queryToDataTable(query.ToList()), FrameOperation);
            }
            else
            {
                Util.gridClear(this.dgApprover);
            }
        }

        // 승인자, 참조자 삭제버튼 눌렀을 때
        private void DeletePerson(C1DataGrid c1DataGrid, int rowIndex)
        {
            try
            {
                c1DataGrid.SelectedIndex = rowIndex;

                // Validation - 승인자 Grid의 경우 지우려고 하는 인간이 요청목록 작업자에 존재하는 경우 Return
                // C20220421-000526 : Hold Release approval 기능 개선 요청 건
                // 1. AS-IS : Hold release 요청 시 요청자가 hold 처리자의 동의 없이 릴리즈 요청 가능 함.
                // 2. TO-BE : Hold release 요청 Approval line 내 1차 승인자는 hold 처리자로 적용
                //           (단 요청자는 Approval 라인 변경이 가능함 (hold 처리자 부재로 인한 결재 지연 방지) *
                // *표 항목 미적용으로 인하여 삭제시에는 그냥 삭제되도록 함. 혹시나 몰라서 주석처리
                //if (c1DataGrid.Name.ToUpper().EndsWith("APPROVER"))
                //{
                //    string deleteUser = DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "USERID").ToString();
                //    if (DataTableConverter.Convert(this.dgReleaseRequestLOTList.ItemsSource).AsEnumerable().Where(x => x.Field<string>("HOLD_USERID").Equals(deleteUser)).Count() > 0)
                //    {
                //        return;
                //    }
                //}

                // 인간 지우기
                c1DataGrid.IsReadOnly = false;
                c1DataGrid.CanUserAddRows = true;
                c1DataGrid.CanUserRemoveRows = true;
                c1DataGrid.RemoveRow(rowIndex);
                c1DataGrid.CanUserRemoveRows = false;
                c1DataGrid.CanUserAddRows = false;
                c1DataGrid.IsReadOnly = true;

                // 승인자 차수 정리
                if (c1DataGrid.Name.ToUpper().EndsWith("APPROVER") && c1DataGrid.GetRowCount() > 0)
                {
                    var query = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().OrderBy(x => x.Field<string>("USER_TYPE")).ThenBy(x => x.Field<int>("APPR_SEQS")).Select((x, approvalSequence) => new
                    {
                        APPR_SEQS = ++approvalSequence,
                        USERID = x.Field<string>("USERID"),
                        USERNAME = x.Field<string>("USERNAME"),
                        DEPTNAME = x.Field<string>("DEPTNAME"),
                        USER_TYPE = x.Field<string>("USER_TYPE")
                    }).OrderBy(x => x.APPR_SEQS);

                    Util.GridSetData(c1DataGrid, Util.queryToDataTable(query), FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 불건전 Data 표출 Popup Open
        private void Show_COM001_035_PACK_EXCEPTION_POPUP(DataTable dt, string exceptionType)
        {
            COM001_035_PACK_EXCEPTION_POPUP wndPopUp = new COM001_035_PACK_EXCEPTION_POPUP();
            wndPopUp.FrameOperation = FrameOperation;

            if (wndPopUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = exceptionType;

                C1WindowExtension.SetParameters(wndPopUp, Parameters);
                wndPopUp.ShowModal();
                wndPopUp.CenterOnScreen();
                wndPopUp.BringToFront();
            }
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = e.NewValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboProjectModel.SelectedValue)) ? string.Empty : this.cboProjectModel.SelectedValue.ToString();
                string productType = string.IsNullOrEmpty(Util.NVC(this.cboProductClassCode.SelectedValue)) ? string.Empty : this.cboProductClassCode.SelectedValue.ToString();
                Util.SetC1ComboBox(this.dataHelper.GetProductTypeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, Area_Type.PACK), this.cboProductClassCode, true, "ALL");
                Util.SetC1ComboBox(this.dataHelper.GetProjectModelInfo(LoginInfo.CFG_AREA_ID, equipmentSegmentID), this.cboProjectModel, true, "ALL");
                Util.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, productType), this.cboProduct, true, "ALL");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboProjectModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboEquipmentSegment.SelectedValue)) ? string.Empty : this.cboEquipmentSegment.SelectedValue.ToString();
                string projectModel = e.NewValue.ToString();
                string productType = string.IsNullOrEmpty(Util.NVC(this.cboProductClassCode.SelectedValue)) ? string.Empty : this.cboProductClassCode.SelectedValue.ToString();
                Util.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, productType), this.cboProduct, true, "ALL");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboProductClassCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboEquipmentSegment.SelectedValue)) ? string.Empty : this.cboEquipmentSegment.SelectedValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboProjectModel.SelectedValue)) ? string.Empty : this.cboProjectModel.SelectedValue.ToString();
                string productType = e.NewValue.ToString();
                Util.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, productType), this.cboProduct, true, "ALL");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.V || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            try
            {
                string[] stringSeparators = new string[] { "\r\n" };
                string clipboardText = Clipboard.GetText();
                string[] arrClipboardText = clipboardText.Split(stringSeparators, StringSplitOptions.None);

                int maxLOTIDCount = 500;
                string messageID = "SFU8217";
                if (arrClipboardText.Count() > maxLOTIDCount)
                {
                    Util.MessageValidation(messageID, 500);     // 최대 500개 까지 가능합니다.
                    return;
                }

                this.txtLOTID.Text = arrClipboardText.Aggregate((current, next) => current + "," + next).ToString();
                this.SearchProcess();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SearchProcess();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            this.LOTMoveGrid(true, this.dgLOTList, this.dgReleaseRequestLOTList);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            this.LOTMoveGrid(false, this.dgReleaseRequestLOTList, this.dgLOTList);
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            // 요청하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.ReleaseRequest();
                }
            });
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.Show_COM001_035_PACK_PERSON(button.Name);
        }

        private void btnPersonDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.DeletePerson(((DataGridCellPresenter)button.Parent).DataGrid, ((DataGridCellPresenter)button.Parent).Cell.Row.Index);
        }

        private void PopupPerson_Closed(object sender, EventArgs e)
        {
            this.SetPerson(sender);
        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            this.Show_COM001_035_PACK_PERSON(textBox.Name);
        }

        private void dgLOTList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    dataGridRowHeaderPresennterPreHold.Content = chkHoldAll;
                    e.Column.HeaderPresenter.Content = dataGridRowHeaderPresennterPreHold;
                    chkHoldAll.Checked -= new RoutedEventHandler(chkHoldAll_Checked);
                    chkHoldAll.Unchecked -= new RoutedEventHandler(chkHoldAll_Unchecked);
                    chkHoldAll.Checked += new RoutedEventHandler(chkHoldAll_Checked);
                    chkHoldAll.Unchecked += new RoutedEventHandler(chkHoldAll_Unchecked);
                }
            }
        }

        private void dgReleaseRequestLOTList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    dataGridRowHeaderPresennterPreReleaseRequest.Content = chkReleaseRequestAll;
                    e.Column.HeaderPresenter.Content = dataGridRowHeaderPresennterPreReleaseRequest;
                    chkReleaseRequestAll.Checked -= new RoutedEventHandler(chkReleaseRequestAll_Checked);
                    chkReleaseRequestAll.Unchecked -= new RoutedEventHandler(chkReleaseRequestAll_Unchecked);
                    chkReleaseRequestAll.Checked += new RoutedEventHandler(chkReleaseRequestAll_Checked);
                    chkReleaseRequestAll.Unchecked += new RoutedEventHandler(chkReleaseRequestAll_Unchecked);
                }
            }
        }

        private void chkHoldAll_Checked(object sender, RoutedEventArgs e)
        {
            if (this.dgLOTList == null || this.dgLOTList.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < this.dgLOTList.GetRowCount(); i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(this.dgLOTList.Rows[i].DataItem, "CHK").Equals(false) ||
                    DataTableConverter.GetValue(this.dgLOTList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "FALSE")
                {
                    DataTableConverter.SetValue(this.dgLOTList.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkReleaseRequestAll_Checked(object sender, RoutedEventArgs e)
        {
            if (this.dgReleaseRequestLOTList == null || this.dgReleaseRequestLOTList.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < this.dgReleaseRequestLOTList.GetRowCount(); i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(this.dgReleaseRequestLOTList.Rows[i].DataItem, "CHK").Equals(false) ||
                    DataTableConverter.GetValue(this.dgReleaseRequestLOTList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "FALSE")
                {
                    DataTableConverter.SetValue(this.dgReleaseRequestLOTList.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkHoldAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.dgLOTList == null || this.dgLOTList.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < this.dgLOTList.GetRowCount(); i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(this.dgLOTList.Rows[i].DataItem, "CHK").Equals(true) ||
                    DataTableConverter.GetValue(this.dgLOTList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "TRUE")
                {
                    DataTableConverter.SetValue(this.dgLOTList.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void chkReleaseRequestAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.dgReleaseRequestLOTList == null || this.dgReleaseRequestLOTList.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < this.dgReleaseRequestLOTList.GetRowCount(); i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(this.dgReleaseRequestLOTList.Rows[i].DataItem, "CHK").Equals(true) ||
                    DataTableConverter.GetValue(this.dgReleaseRequestLOTList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "TRUE")
                {
                    DataTableConverter.SetValue(this.dgReleaseRequestLOTList.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion
    }

    internal class COM001_035_REQUEST_PACK_DataHelper
    {
        #region Member Variable List
        #endregion

        #region Constructor
        public COM001_035_REQUEST_PACK_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists..
        // 순서도 호출 - 인간들 조회
        internal DataTable GetPerson(string userID)
        {
            string bizRuleName = "DA_BAS_SEL_PERSON_BY_ID";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["USERID"] = userID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - EquipmentSegmentInfo
        internal DataTable GetEquipmentSegmentInfo(string areaID)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Project Model 정보
        internal DataTable GetProjectModelInfo(string areaID, string equipmentSegmentID)
        {
            string bizRuleName = "DA_BAS_SEL_PRJMODEL_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(equipmentSegmentID) || equipmentSegmentID.Equals("ALL")) ? null : equipmentSegmentID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Product Info currespond with Project Model or Project Class Code
        internal DataTable GetProductByProjectModelOrProjectClassCodeInfo(string shopID, string areaID, string equipmentSegmentID, string projectModel, string productClassCode)
        {
            string bizRuleName = "DA_BAS_SEL_PRJPRODUCT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROJECT_MODEL", typeof(string));
                dtRQSTDT.Columns.Add("PRDCLASS", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["SHOPID"] = shopID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(equipmentSegmentID) || equipmentSegmentID.Equals("ALL")) ? null : equipmentSegmentID;
                drRQSTDT["PROJECT_MODEL"] = (string.IsNullOrEmpty(projectModel) || projectModel.Equals("ALL")) ? null : projectModel;
                drRQSTDT["PRDCLASS"] = (string.IsNullOrEmpty(productClassCode) || productClassCode.Equals("ALL")) ? null : productClassCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Product Type
        internal DataTable GetProductTypeInfo(string shopID, string areaID, string equipmentSegmentID, string areaTypeCode)
        {
            string bizRuleName = "DA_BAS_SEL_PRODUCTTYPE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = shopID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(equipmentSegmentID) || equipmentSegmentID.Equals("ALL")) ? null : equipmentSegmentID;
                drRQSTDT["AREA_TYPE_CODE"] = areaTypeCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Activity Reason Code 조회
        internal DataTable GetActivityReasonInfo(string activityID)
        {
            string bizRuleName = "DA_BAS_SEL_ACTIVITIREASON_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["ACTID"] = activityID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Hold List (DA 호출)
        internal DataTable GetHoldListByDataAccess(string productID, string fromDate, string toDate, string equipmentSegmentID, string areaID, string productClassCode, string projectModel, string approveRequestFlag, string LOTID)
        {
            string bizRuleName = "DA_PRD_SEL_PACKHOLD";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("CLASS", typeof(string));
                dtRQSTDT.Columns.Add("PRODTYPE", typeof(string));
                dtRQSTDT.Columns.Add("APPR_REQ_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["PRODID"] = !string.IsNullOrEmpty(LOTID) ? null : productID;
                drRQSTDT["FROMDATE"] = !string.IsNullOrEmpty(LOTID) ? null : fromDate;
                drRQSTDT["TODATE"] = !string.IsNullOrEmpty(LOTID) ? null : toDate;
                drRQSTDT["EQSGID"] = !string.IsNullOrEmpty(LOTID) ? null : equipmentSegmentID;
                drRQSTDT["AREAID"] = !string.IsNullOrEmpty(LOTID) ? null : areaID;
                drRQSTDT["CLASS"] = !string.IsNullOrEmpty(LOTID) ? null : productClassCode;
                drRQSTDT["PRODTYPE"] = !string.IsNullOrEmpty(LOTID) ? null : projectModel;
                drRQSTDT["APPR_REQ_FLAG"] = !string.IsNullOrEmpty(LOTID) ? null : approveRequestFlag;
                drRQSTDT["LOTID"] = !string.IsNullOrEmpty(LOTID) ? LOTID : null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - LOT Release 요청
        internal DataSet SetReleaseRequest(DataTable dtINDATA, DataTable dtINLOT, DataTable dtINPROG, DataTable dtINREF)
        {
            string bizRuleName = "BR_PRD_REG_APPR_REQUEST";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA,OUTDATA_LOT";

            try
            {
                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtINLOT);
                dsINDATA.Tables.Add(dtINPROG);
                dsINDATA.Tables.Add(dtINREF);

                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).Aggregate((current, next) => current + "," + next), outDataSetName, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.ShowExceptionMessages(ex);
            }

            return dsOUTDATA;
        }
        #endregion
    }
}