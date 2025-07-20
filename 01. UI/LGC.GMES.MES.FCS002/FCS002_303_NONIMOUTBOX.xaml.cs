/*************************************************************************************
 Created Date : 2023.07.24
      Creator : 이병윤
   Decription : E20230419-000979 : 자동 포장 구성(원/각형) > 추가기능 > Non IM OUTBOX발행 클릭시 OUTBOX 생성 팝업화면
--------------------------------------------------------------------------------------
 [Change History]
 

**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Windows.Controls;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_303_NONIMOUTBOX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_303_NONIMOUTBOX : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _util = new Util();
        
        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        string sPGMID  = string.Empty;
        int chkShipToPop = 0;
        public string AREAID
        {
            get;
            set;
        }
        public string PALLETID
        {
            get;
            set;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_303_NONIMOUTBOX()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

        }
        #endregion

        #region [Events]
        /// <summary>
        /// OUTBOX생성 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Util.NVC(popShipto.SelectedValue)))
            {
                //	SFU4096	출하처를 선택하세요.	
                Util.MessageValidation("SFU4096");
                return;
            }

            if (string.IsNullOrWhiteSpace((string)cboLabelType.SelectedValue) || (string)cboLabelType.SelectedValue == "SELECT")
            {
                //SFU1522 라벨 타입을 선택하세요.	
                Util.MessageValidation("SFU1522");
                return;
            }

            //if (Util.NVC_Int(txtSoc.Value) == 0)
            //{
            //    //SFU4203   SOC 정보를 입력하세요.	
            //    Util.MessageValidation("SFU4203");
            //    return;
            //}

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            txtInInboxID.Text = string.Empty;
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// 투입 INBOX 입력 후 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInInboxID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtInInboxID.Text))
                    {
                        //SFU3350	입력오류 : PALLETID 를 입력해 주세요.
                        Util.MessageValidation("SFU3350", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtInInboxID.Focus();
                                txtInInboxID.Text = string.Empty;
                            }
                        });
                        return;
                    }
                    string[] inboxMuti = { txtInInboxID.Text };
                    SearchInbox(inboxMuti);
                    txtInInboxID.Focus();
                    txtInInboxID.Text = string.Empty;
                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Ctrl C + V로 처리할 수 있게(다건처리)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInInboxID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    SearchInbox(sPasteStrings); // 조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(true))
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                dgResult.ItemsSource = DataTableConverter.Convert(dt);

                if (dt.Rows.Count == 0)
                {
                    txtProdID.Text = String.Empty;
                    txtProject.Text = String.Empty;
                    chkShipToPop = 0; // 출하처 초기화
                    txtSoc.Value = 0;
                    SetShipToClear();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 출하처 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popShipto_ValueChanged(object sender, EventArgs e)
        {
            // MMD : 출하처 인쇄관리 항목(Cell) 조회
            DataTable dtPrtDt = new DataTable();
            dtPrtDt.TableName = "RQSTDT";
            dtPrtDt.Columns.Add("LANGID", typeof(string));
            dtPrtDt.Columns.Add("PRODID", typeof(string));
            dtPrtDt.Columns.Add("SHOPID", typeof(string));
            dtPrtDt.Columns.Add("SHIPTO_ID", typeof(string));

            DataRow drPrtrow = dtPrtDt.NewRow();
            drPrtrow["LANGID"] = LoginInfo.LANGID;
            drPrtrow["PRODID"] = txtProdID.Text;
            drPrtrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drPrtrow["SHIPTO_ID"] = popShipto.SelectedValue;

            dtPrtDt.Rows.Add(drPrtrow);
            DataTable dtMmdEx = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", "RQSTDT", "RSLTDT", dtPrtDt);

            string attr4 = string.Empty;
            if (dtMmdEx.Rows.Count == 0)
            {
                //// 초기화일 경우 경고메세지 보이지 않게 하기
                //if (!string.IsNullOrEmpty(popShipto.SelectedValue.ToString()))
                //{
                //    //SFU9994 : Contact engineers to configure the label
                //    Util.MessageValidation("SFU9994");
                //}

                //attr4 = ",";
                //MMD:인쇄관리 항목에 등록된 라벨이 없는 경우 Default label: LBL0097 보여준다.
                attr4 = "LBL0097";
            }
            else
            {
                for (int i = 0; i < dtMmdEx.Rows.Count; i++)
                {
                    attr4 += dtMmdEx.Rows[i]["CBO_CODE"].ToString() + ",";
                }
            }

            // 라벨타입조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MOBILE_INBOX_LABEL_TYPE";
            dr["ATTRIBUTE4"] = attr4;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

            // SELECT 추가
            DataRow drResult = dtResult.NewRow();
            drResult["CBO_NAME_ONLY"] = "-SELECT-";
            drResult["CBO_CODE"] = "SELECT";
            dtResult.Rows.InsertAt(drResult, 0);
            // 콤보박스 세팅
            cboLabelType.DisplayMemberPath = "CBO_NAME_ONLY";
            cboLabelType.SelectedValuePath = "CBO_CODE";
            cboLabelType.ItemsSource = dtResult.Copy().AsDataView();
            if (dtResult.Rows.Count <= 1)
            {
                cboLabelType.SelectedIndex = 0;
            }
            else
            {
                cboLabelType.SelectedValue = dtResult.Rows[1]["CBO_CODE"].ToString();

                if (cboLabelType.SelectedIndex < 0)
                    cboLabelType.SelectedIndex = 0;
            }
        }
        #endregion

        #region [Method]

        /// <summary>
        /// 출하처 조회
        /// </summary>
        /// <param name="prodID"></param>
        private void setShipToPopControl(string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
        }

        /// <summary>
        /// 투입 Inbox 조회
        /// </summary>
        /// <param name="inboxList"></param>
        /// <param name="inbox"></param>
        private void SearchInbox(string[] inboxMuti)
        {
            try
            {
                
                for (int i = 0; i < inboxMuti.Length; i++)
                {
                    // 라벨타입조회
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("BOXID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = inboxMuti[i];
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_INBOX_NJ", "INDATA", "OUTDATA", RQSTDT);

                    if (!dtRslt.Columns.Contains("CHK"))
                        dtRslt = _util.gridCheckColumnAdd(dtRslt, "CHK");

                    if (chkShipToPop == 0)
                    {
                        txtProdID.Text = dtRslt.Rows[0]["PRODID"].ToString();
                        txtProject.Text = dtRslt.Rows[0]["PROJECT"].ToString();
                        setShipToPopControl(txtProdID.Text.Trim()); // 출하처 조회
                        chkShipToPop = 1;
                    }

                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgResult.ItemsSource);
                        var query = (from t in dtSource.AsEnumerable()
                                     where t.Field<string>("BOXID") == inboxMuti[i]
                                     select t).Distinct();
                        if (query.Any())
                        {
                            // 중복 데이터가 존재 합니다.[%1]
                            Util.MessageValidation("SFU2051", inboxMuti[i]);
                            return;
                        }
                        if (!dtRslt.Rows[0]["PRODID"].ToString().Equals(txtProdID.Text) || !dtRslt.Rows[0]["PROJECT"].ToString().Equals(txtProject.Text))
                        {
                            //SFU4338		동일한 제품만 작업 가능합니다.
                            Util.MessageValidation("SFU4338");
                            return;
                        }

                        dtRslt.Merge(dtSource);

                        Util.GridSetData(dgResult, dtRslt, FrameOperation, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInInboxID.Focus();
                txtInInboxID.Text = string.Empty;
                Clipboard.Clear();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void RunStart()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SOC", typeof(string));
                inDataTable.Columns.Add("SHITO_ID", typeof(string));
                inDataTable.Columns.Add("LABEL_ID", typeof(string));

                DataTable inBoxTable = inDataSet.Tables.Add("IN_INBOX");
                inBoxTable.Columns.Add("INBOXID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI"; // EQ : 설비, UI : UI
                newRow["IFMODE"] = "OFF"; // 인터페이스 모드 ON : Online, OFF : Offline, TEST : Test
                newRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                newRow["USERID"] = sUSERID;
                newRow["SOC"] = Util.NVC_Int(txtSoc.Value);
                newRow["SHITO_ID"] = popShipto.SelectedValue;
                newRow["LABEL_ID"] = cboLabelType.SelectedValue;


                inDataTable.Rows.Add(newRow);

                newRow = null;

                DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["INBOXID"] = Util.NVC(dgResult.GetCell(i, dgResult.Columns["BOXID"].Index).Value);
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTBOX_M50L_NJ", "IN_EQP,IN_INBOX", "OUT_BOX", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        String outboxId = (string)searchResult.Tables["OUT_BOX"].Rows[0]["OUTBOXID"];
                        String zplCode = (string)searchResult.Tables["OUT_BOX"].Rows[0]["ZPLCODE"];

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275", result =>
                        {
                            // 초기화
                            SetAllClear();
                            // 출력팝업
                            popup_LbL0085(outboxId, zplCode);
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void popup_LbL0085(string outboxid, string zplCode)
        {

            FCS002_303_LBL0085 puLbl0085 = new FCS002_303_LBL0085();
            puLbl0085.FrameOperation = FrameOperation;
            if (puLbl0085 != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = outboxid;
                Parameters[1] = zplCode;

                C1WindowExtension.SetParameters(puLbl0085, Parameters);
                puLbl0085.Closed += new EventHandler(puLbl0085_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(puLbl0085);
                        puLbl0085.BringToFront();
                        break;
                    }
                }
            }
            else
            {
                //Message: 
            }
        }

        private void puLbl0085_Closed(object sender, EventArgs e)
        {
            FCS002_303_LBL0085 puLbl0085 = sender as FCS002_303_LBL0085;

            if (puLbl0085.DialogResult == MessageBoxResult.OK)
            {
                puLbl0085.DialogResult = MessageBoxResult.Cancel;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(puLbl0085);
                    break;
                }
            }
        }

        /// <summary>
        /// 출하처,라벨 초기화
        /// </summary>
        private void SetShipToClear()
        {
            // 거래처,라벨타입 초기화처리 
            popShipto.SelectedValue = "";
            popShipto.SelectedText = "";
            setShipToPopControl("");
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            
        }

        /// <summary>
        /// 전체 초기화
        /// </summary>
        private void SetAllClear()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {   
                    dt.Rows.Remove(dr);
                }
                dgResult.ItemsSource = DataTableConverter.Convert(dt);

                if (dt.Rows.Count == 0)
                {
                    txtProdID.Text = String.Empty;
                    txtProject.Text = String.Empty;
                    chkShipToPop = 0; // 출하처 초기화
                    txtSoc.Value = 0;
                    SetShipToClear(); 
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


    }
}
