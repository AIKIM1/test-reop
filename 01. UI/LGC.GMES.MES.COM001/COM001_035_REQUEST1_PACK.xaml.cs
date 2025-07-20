/*************************************************************************************
 Created Date : 2023.01.28
      Creator : 정용석
  Description : Pack 물품 청구 요청 / 요청취소 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.28 정용석 : Initial Created. (물품청구 요청만 가능, 요청취소는 부모 Form에서 가능)
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST1_PACK : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private COM001_035_REQUEST1_PACK_DataHelper dataHelper = new COM001_035_REQUEST1_PACK_DataHelper();
        private string requestNo = string.Empty;
        private string approvalBusinessCode = string.Empty;
        private string callControlName = string.Empty;

        private DataGridRowHeaderPresenter dataGridRowHeaderPresenterLOTList = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private DataGridRowHeaderPresenter dataGridRowHeaderPresenterMaterialRequest = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private CheckBox chkLOTListAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        private CheckBox chkMaterialRequestAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        #endregion

        #region Constructor
        public COM001_035_REQUEST1_PACK()
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
            object[] arrParameters = C1WindowExtension.GetParameters(this);
            if (arrParameters == null || arrParameters.Length <= 0)
            {
                return;
            }

            this.requestNo = arrParameters[0].ToString();
            this.approvalBusinessCode = arrParameters[1].ToString();

            Util.SetC1ComboBox(this.dataHelper.GetActivityReasonInfo("CHARGE_PROD_LOT"), this.cboActivityReasonCode, false, "SELECT");

            this.SetControlVisibility(true);
        }

        // Set Control Visibility
        private void SetControlVisibility(bool isEnabled)
        {
            this.btnRequest.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.btnSearch.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.btnClear.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.txtApprover.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.txtReferrer.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.dgLOTList.Columns["CHK"].Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.dgMaterialRequestList.Columns["CHK"].Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        // 조회
        private void SearchProcess(string LOTID = null)
        {
            if (!string.IsNullOrEmpty(this.txtLOTID.Text))
            {
                LOTID = this.txtLOTID.Text;
            }

            try
            {
                this.ResetControl();
                this.loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(this.dgLOTList);
                Util.gridClear(this.dgMaterialRequestList);
                Util.gridClear(this.dgApprover);
                Util.gridClear(this.dgReferrer);
                this.DoEvents();
                DataTable dt = this.dataHelper.GetReworkLOTList(LOTID);
                if (CommonVerify.HasTableRow(dt))
                {
                    this.SeparateWholesomeLOTUnwholeSomeLOT(dt);
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

        // 불건전 Data와 건전 Data 분리
        private void SeparateWholesomeLOTUnwholeSomeLOT(DataTable dt)
        {
            // 불건전 Data 추출
            var queryUnwholesome = dt.AsEnumerable().Where(x => string.IsNullOrEmpty(x.Field<string>("CHECK_LOTID")) ||
                                                                x.Field<string>("WIPHOLD") != "N" ||
                                                                x.Field<string>("PROCID") != "PR000" ||
                                                                !("WAIT,PROC").Contains(x.Field<string>("WIPSTAT")) ||
                                                                !string.IsNullOrEmpty(x.Field<string>("REQ_NO")));

            // 불건전 Data와 건전 Data 분리
            var queryWholeSome = dt.AsEnumerable().Except(queryUnwholesome);

            // 불건전 Data는 불건전 Popup창에 집어넣고, 건전 Data는 대상 Grid에 집어넣기.
            if (queryUnwholesome.Count() > 0)
            {
                this.Show_COM001_035_PACK_EXCEPTION_POPUP(queryUnwholesome.CopyToDataTable(), "MATERIAL_REQUEST_LOT");
            }

            if (queryWholeSome.Count() > 0)
            {
                Util.GridSetData(this.dgLOTList, dt, FrameOperation, true);
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

        // Reset Control
        private void ResetControl()
        {
            Util.gridClear(this.dgMaterialRequestList);
            Util.gridClear(this.dgApprover);
            Util.gridClear(this.dgReferrer);
            this.txtLOTID.Text = string.Empty;
            this.txtRequestNote.Text = string.Empty;
        }

        // 승인자 Validation
        private bool ValidationApprover(string userID)
        {
            bool returnValue = true;
            DataTable dt = this.dataHelper.GetAuthorityMenuByUserID(userID);

            if (!CommonVerify.HasTableRow(dt))
            {
                Util.MessageValidation("SUF4969");      // 승인권한이 없는 사용자 입니다.
                returnValue = false;
            }

            if (dt.AsEnumerable().Select(x => x.Field<Int64>("ACCESS_COUNT")).FirstOrDefault() <= 0)
            {
                Util.MessageValidation("SUF4969");      // 승인권한이 없는 사용자 입니다.
                returnValue = false;
            }

            return returnValue;
        }

        // 승인자, 참조자 삭제버튼 눌렀을 때
        private void DeletePerson(C1DataGrid c1DataGrid, int rowIndex)
        {
            try
            {
                c1DataGrid.SelectedIndex = rowIndex;
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
                    var query = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().Select((x, approvalSequence) => new
                    {
                        APPR_SEQS = ++approvalSequence,
                        USERID = x.Field<string>("USERID"),
                        USERNAME = x.Field<string>("USERNAME"),
                        DEPTNAME = x.Field<string>("DEPTNAME")
                    }).OrderBy(x => x.APPR_SEQS);

                    Util.GridSetData(c1DataGrid, Util.queryToDataTable(query), FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        // 물품청구 요청
        private void MaterialRequest()
        {
            // Declarations...
            string sendmailApproverUserID = string.Empty;
            string sendMailReferrerUserID = string.Empty;

            // Validations..
            if (this.dgMaterialRequestList.Rows.Count == 0)
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
                Util.MessageValidation("SFU1593");  // 사유를 선택하세요.
                return;
            }
            if (DataTableConverter.Convert(this.dgMaterialRequestList.ItemsSource).AsEnumerable().GroupBy(x => x.Field<string>("PROD_CLSS_CODE")).Count() > 1)
            {
                Util.MessageValidation("SFU8899");  // 동일 제품유형의 LOT만 이동이 가능합니다.
                return;
            }
            if (DataTableConverter.Convert(this.dgMaterialRequestList.ItemsSource).AsEnumerable().Where(x => string.IsNullOrEmpty(x.Field<string>("COST_CNTR_ID"))).Count() > 0)
            {
                Util.MessageValidation("SFU8903");  // COST CENTER 설정이 되지 않은 라인이 존재합니다.
                return;
            }
            if (DataTableConverter.Convert(this.dgMaterialRequestList.ItemsSource).AsEnumerable().GroupBy(x => x.Field<string>("COST_CNTR_ID")).Count() > 1)
            {
                Util.MessageValidation("SFU8904");  // 하나의 COST CENTER만 선택할 수 있습니다.
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
                drINDATA["APPR_BIZ_CODE"] = "LOT_REQ";
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["REQ_NOTE"] = this.txtRequestNote.Text;
                drINDATA["RESNCODE"] = this.cboActivityReasonCode.SelectedValue.ToString();
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["COST_CNTR_ID"] = DataTableConverter.Convert(this.dgMaterialRequestList.ItemsSource).AsEnumerable().Select(x => x.Field<string>("COST_CNTR_ID")).FirstOrDefault();
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
                foreach (DataRowView drv in DataTableConverter.Convert(this.dgMaterialRequestList.ItemsSource).AsDataView())
                {
                    DataRow drINLOT = dtINLOT.NewRow();
                    drINLOT["LOTID"] = drv["LOTID"].ToString();
                    drINLOT["WIPQTY"] = drv["WIPQTY"].ToString();
                    drINLOT["WIPQTY2"] = drv["WIPQTY"].ToString();
                    drINLOT["PRODID"] = drv["PRODID"].ToString();
                    drINLOT["PRODNAME"] = drv["PRODNAME"].ToString();
                    drINLOT["MODELID"] = drv["MODLID"].ToString();
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

                DataSet ds = this.dataHelper.SetMaterialRequest(dtINDATA, dtINLOT, dtINPROG, dtINREF);

                // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
                if (CommonVerify.HasTableInDataSet(ds) && CommonVerify.HasTableRow(ds.Tables["OUTDATA_LOT"]))
                {
                    this.Show_COM001_035_PACK_EXCEPTION_POPUP(ds.Tables["OUTDATA_LOT"]);
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
                                    , mailSend.makeBodyApp(requestNo + " " + this.Header, this.txtRequestNote.Text, dtINLOT)
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

        // Control Clear
        private void ClearControl()
        {
            this.txtLOTID.Text = string.Empty;
            Util.gridClear(this.dgLOTList);
            Util.gridClear(this.dgMaterialRequestList);
            this.txtApprover.Text = string.Empty;
            Util.gridClear(this.dgApprover);
            this.txtReferrer.Text = string.Empty;
            Util.gridClear(this.dgReferrer);
            this.cboActivityReasonCode.SelectedIndex = 0;
            this.txtRequestNote.Text = string.Empty;
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

        // 승인자 그리드에 집어넣기
        private void SetApprover(string userID, string userName, string departmentID, string departmentName)
        {
            List<Tuple<string, string, string>> lstApprover = new List<Tuple<string, string, string>>();
            lstApprover.Add(new Tuple<string, string, string>(userID, userName, departmentName));

            try
            {
                // 승인자 검증
                if (!this.ValidationApprover(userID))
                {
                    return;
                }

                // 승인자 리스트에 집어넣은게 없으면
                if (this.dgApprover == null || this.dgApprover.GetRowCount() <= 0)
                {
                    var queryPerson = lstApprover.Select((x, approvalSequence) => new
                    {
                        APPR_SEQS = ++approvalSequence,
                        USERID = x.Item1,
                        USERNAME = x.Item2,
                        DEPTNAME = x.Item3
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
                var query = dtApprover.AsEnumerable().OrderBy(x => x.Field<Int64>("APPR_SEQS")).Select(x => new
                {
                    USERID = x.Field<string>("USERID"),
                    USERNAME = x.Field<string>("USERNAME"),
                    DEPTNAME = x.Field<string>("DEPTNAME")
                }).Union(lstApprover.Select(y => new
                {
                    USERID = y.Item1,
                    USERNAME = y.Item2,
                    DEPTNAME = y.Item3
                })).Select((z, approvalSequence) => new
                {
                    APPR_SEQS = ++approvalSequence,
                    USERID = z.USERID,
                    USERNAME = z.USERNAME,
                    DEPTNAME = z.DEPTNAME
                }).OrderBy(z => z.APPR_SEQS);

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

            if (dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK")).Count() <= 0)
            {
                return;
            }

            // Validation - Product Class Code 1호
            if (isMoveDown && dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK")).GroupBy(x => x.Field<string>("PROD_CLSS_CODE")).Count() > 1)
            {
                Util.MessageValidation("SFU8899");  // 동일 제품유형의 LOT만 이동이 가능합니다.
                return;
            }

            // Validation - Product Class Code 2호
            if (isMoveDown && CommonVerify.HasTableRow(dtTarget) && 
                (dtTarget.AsEnumerable().GroupBy(x => x.Field<string>("PROD_CLSS_CODE")).Select(grp => grp.Key).FirstOrDefault() != dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK")).GroupBy(x => x.Field<string>("PROD_CLSS_CODE")).Select(grp =>grp.Key).FirstOrDefault())
               )
            {
                Util.MessageValidation("SFU8899");  // 동일 제품유형의 LOT만 이동이 가능합니다.
                return;
            }

            DataTable dtUnion = new DataTable();
            if (CommonVerify.HasTableRow(dtTarget))
            {
                // Target Grid에 데이터가 있을 경우 Target Grid Data Union Move Data
                dtUnion = dtTarget.AsEnumerable().Union(dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK"))).CopyToDataTable();
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
                dtUnion = dtSource.AsEnumerable().Where(x => x.Field<bool>("CHK")).CopyToDataTable();
                foreach (DataRow drUnion in dtUnion.Rows)
                {
                    drUnion["CHK"] = false;
                }
            }
            Util.GridSetData(targetC1DataGrid, dtUnion, FrameOperation);

            // Source Grid에서 Move 되는 것들 삭제
            sourceC1DataGrid.CanUserAddRows = true;
            sourceC1DataGrid.CanUserRemoveRows = true;
            sourceC1DataGrid.BeginEdit();
            for(int rowIndex = sourceC1DataGrid.GetRowCount() - 1; rowIndex >= 0; rowIndex--)
            {
                if (Convert.ToBoolean(DataTableConverter.GetValue(sourceC1DataGrid.Rows[rowIndex].DataItem, "CHK")))
                {
                    sourceC1DataGrid.SelectedIndex = rowIndex;
                    sourceC1DataGrid.RemoveRow(rowIndex);
                }
            }
            sourceC1DataGrid.EndEdit();
            sourceC1DataGrid.CanUserRemoveRows = false;
            sourceC1DataGrid.CanUserAddRows = false;

            this.chkLOTListAll.IsChecked = false;
            this.chkMaterialRequestAll.IsChecked = false;
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
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= C1Window_Loaded;
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

        private void btnPersonDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.DeletePerson(((DataGridCellPresenter)button.Parent).DataGrid, ((DataGridCellPresenter)button.Parent).Cell.Row.Index);
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.Show_COM001_035_PACK_PERSON(button.Name);
        }

        private void PopupPerson_Closed(object sender, EventArgs e)
        {
            this.SetPerson(sender);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtLOTID.Text))
            {
                return;
            }
            this.SearchProcess();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.ClearControl();
        }

        private void btnMaterialRequest_Click(object sender, RoutedEventArgs e)
        {
            this.MaterialRequest();
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SearchProcess(this.txtLOTID.Text.Trim());
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.V || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            if (this.cboActivityReasonCode.SelectedIndex <= 0)
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
                this.SearchProcess(this.txtLOTID.Text);
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

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            this.LOTMoveGrid(true, this.dgLOTList, this.dgMaterialRequestList);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            this.LOTMoveGrid(false, this.dgMaterialRequestList, this.dgLOTList);
        }

        private void cboActivityReasonCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox c1ComboBox = sender as C1ComboBox;

            if (c1ComboBox.SelectedIndex <= 0)
            {
                this.txtLOTID.Text = string.Empty;
                this.txtLOTID.IsEnabled = false;
                this.btnSearch.IsEnabled = false;
            }
            else
            {
                this.txtLOTID.Text = string.Empty;
                this.txtLOTID.IsEnabled = true;
                this.btnSearch.IsEnabled = true;
            }
        }

        private void dgLOTList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    dataGridRowHeaderPresenterLOTList.Content = chkLOTListAll;
                    e.Column.HeaderPresenter.Content = dataGridRowHeaderPresenterLOTList;
                    chkLOTListAll.Checked -= new RoutedEventHandler(chkHoldAll_Checked);
                    chkLOTListAll.Unchecked -= new RoutedEventHandler(chkHoldAll_Unchecked);
                    chkLOTListAll.Checked += new RoutedEventHandler(chkHoldAll_Checked);
                    chkLOTListAll.Unchecked += new RoutedEventHandler(chkHoldAll_Unchecked);
                }
            }
        }

        private void dgMaterialRequestList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    dataGridRowHeaderPresenterMaterialRequest.Content = chkMaterialRequestAll;
                    e.Column.HeaderPresenter.Content = dataGridRowHeaderPresenterMaterialRequest;
                    chkMaterialRequestAll.Checked -= new RoutedEventHandler(chkReleaseRequestAll_Checked);
                    chkMaterialRequestAll.Unchecked -= new RoutedEventHandler(chkReleaseRequestAll_Unchecked);
                    chkMaterialRequestAll.Checked += new RoutedEventHandler(chkReleaseRequestAll_Checked);
                    chkMaterialRequestAll.Unchecked += new RoutedEventHandler(chkReleaseRequestAll_Unchecked);
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
                if (DataTableConverter.GetValue(this.dgLOTList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "FALSE")
                {
                    DataTableConverter.SetValue(this.dgLOTList.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkReleaseRequestAll_Checked(object sender, RoutedEventArgs e)
        {
            if (this.dgMaterialRequestList == null || this.dgMaterialRequestList.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < this.dgMaterialRequestList.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(this.dgMaterialRequestList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "FALSE")
                {
                    DataTableConverter.SetValue(this.dgMaterialRequestList.Rows[i].DataItem, "CHK", true);
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
                if (DataTableConverter.GetValue(this.dgLOTList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "TRUE")
                {
                    DataTableConverter.SetValue(this.dgLOTList.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void chkReleaseRequestAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.dgMaterialRequestList == null || this.dgMaterialRequestList.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < this.dgMaterialRequestList.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(this.dgMaterialRequestList.Rows[i].DataItem, "CHK").ToString().ToUpper() == "TRUE")
                {
                    DataTableConverter.SetValue(this.dgMaterialRequestList.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion
    }

    internal class COM001_035_REQUEST1_PACK_DataHelper
    {
        #region Member Variable List
        #endregion

        #region Constructor
        public COM001_035_REQUEST1_PACK_DataHelper()
        {
        }
        #endregion

        #region Member Function Lists..
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

        // 순서도 호출 - 수리 공정에 있는 LOT 조회
        internal DataTable GetReworkLOTList(string LOTID)
        {
            string bizRuleName = "DA_PRD_SEL_REWORK_LOT_LIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = LOTID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 승인하는 인간들 메뉴 권한 검증
        internal DataTable GetAuthorityMenuByUserID(string userID)
        {
            string bizRuleName = "DA_BAS_SEL_AUTHORITYMENU_BY_ID";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("MENUID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["MENUID"] = this.GetMenuID().AsEnumerable().Select(x => x.Field<string>("MENUID")).FirstOrDefault();
                drRQSTDT["USERID"] = userID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Namespace, FormID 기준으로 Menu 정보 조회
        internal DataTable GetMenuID()
        {
            string bizRuleName = "DA_BAS_SEL_MENU_BY_ATTRIBUTE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("MENUID", typeof(string));
                dtRQSTDT.Columns.Add("MENUNAME", typeof(string));
                dtRQSTDT.Columns.Add("MENUDESC", typeof(string));
                dtRQSTDT.Columns.Add("FORMID", typeof(string));
                dtRQSTDT.Columns.Add("MENULEVEL", typeof(string));
                dtRQSTDT.Columns.Add("NAMESPACE", typeof(string));
                dtRQSTDT.Columns.Add("PROGRAMTYPE", typeof(string));
                dtRQSTDT.Columns.Add("LINEIUSE", typeof(string));
                dtRQSTDT.Columns.Add("MENUIUSE", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("REF_TBL_LIST", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["MENUID"] = null;
                drRQSTDT["MENUNAME"] = null;
                drRQSTDT["MENUDESC"] = null;
                drRQSTDT["FORMID"] = "COM001_036_Pack";
                drRQSTDT["MENULEVEL"] = null;
                drRQSTDT["NAMESPACE"] = this.GetType().Namespace;
                drRQSTDT["PROGRAMTYPE"] = "SFU";
                drRQSTDT["LINEIUSE"] = null;
                drRQSTDT["MENUIUSE"] = "Y";
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["REF_TBL_LIST"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 물품청구 요청
        internal DataSet SetMaterialRequest(DataTable dtINDATA, DataTable dtINLOT, DataTable dtINPROG, DataTable dtINREF)
        {
            string bizRuleName = "BR_PRD_REG_APPR_REQUEST";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA,OUTDATA_LOT";  // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출

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