/*************************************************************************************
 Created Date : 2020.11.24
      Creator : 
   Decription : Verification
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.24  오화백 : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_345 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

      //  private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();

        private Util _Util = new Util();
        private bool _isSearch = false;
        string ValueToLot = string.Empty;
        DateTime dCalDate;
        private bool _isExistVerifCode = false;
        private bool _isCheckedVerifCode = false;
        CommonCombo _combo = new CommonCombo();

        List<string> LotList = new List<string>();
       
        public COM001_345()
        {
            InitializeComponent();
            InitCombo();
            SetInitColumn();
            this.Loaded += UserControl_Loaded;
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
        private void UserControl_Initialized(object sender, EventArgs e)
        {
           // TimerSetting();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion


        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅


            #region Verification 탭

            //동
            C1ComboBox[] cboAreaChildVerification = { cboEquipmentSegmentVerification };
            _combo.SetCombo(cboAreaVerification, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildVerification, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentVerification = { cboAreaVerification };
            C1ComboBox[] cboEquipmentSegmentChildVerification = { cboProcessVerification };
            _combo.SetCombo(cboEquipmentSegmentVerification, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildVerification, cbParent: cboEquipmentSegmentParentVerification, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentVerification = { cboEquipmentSegmentVerification };
            C1ComboBox[] cboProcessChildVerification = { cboEquipmentVerification };
            _combo.SetCombo(cboProcessVerification, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChildVerification, cbParent: cboProcessParentVerification, sCase: "PROCESS_PCSGID_V", sFilter: new string[] { "VERIF" });

            //설비
            C1ComboBox[] cboEquipmentParentVerification = { cboEquipmentSegmentVerification, cboProcessVerification };
            if(LoginInfo.CFG_AREA_ID.Equals("ED"))
                _combo.SetCombo(cboEquipmentVerification, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParentVerification, sCase: "EQUIPMENT");
            else
                _combo.SetCombo(cboEquipmentVerification, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParentVerification, sCase: "EQUIPMENT");

            // 조회조건 HOLD 사유
            setHoldYN();


            // INPUT VERIFICATION - Verification
            String[] sFilter1 = { "VERIF_CODE" };
            _combo.SetCombo(cboVerification, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            if (cboVerification != null && cboVerification.Items != null && cboVerification.Items.Count > 0)
                cboVerification.SelectedIndex = 0;

            // INPUT VERIFICATION - Reason
            String[] sFilter2 = { "VERIF_RSN_CODE", "Y" };
            _combo.SetCombo(cboVerificationReson, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODEATTR");

            if (cboVerificationReson != null && cboVerificationReson.Items != null && cboVerificationReson.Items.Count > 0)
                cboVerificationReson.SelectedIndex = 0;

            // INPUT VERIFICATION - HOLD 사유
            C1ComboBox[] cboHoldTypeParentVerification = { cboAreaVerification };
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, cbParent: cboHoldTypeParentVerification, sCase: "CBO_AREA_ACTIVITIREASON", sFilter: new string[] { "HOLD_LOT" });

            #endregion


            #region History 탭

            //동
            C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegmentHistory };
            C1ComboBox[] cboProcessChildHistory = { cboEquipmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChildHistory,  cbParent: cboProcessParentHistory, sCase: "PROCESS_PCSGID_V");

            //설비
            C1ComboBox[] cboEquipmentParentHistory = { cboEquipmentSegmentHistory, cboProcessHistory };
            _combo.SetCombo(cboEquipmentHistory, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParentHistory, sCase: "EQUIPMENT");

            #endregion

        }

        /// <summary>
        /// 조회조건 HOLD 사유
        /// </summary>
        private void setHoldYN()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboHoldVerification.ItemsSource = DataTableConverter.Convert(dt);
                cboHoldVerification.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region Event

        #region 공통

        /// <summary>
        /// 화면로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveVerification);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            //예상해제일
            dtExpected.SelectedDateTime = DateTime.Now.AddDays(30);

            //전기일 셋팅
            dCalDate = GetComSelCalDate();
            dtCalDate.SelectedDateTime = dCalDate;

            ldpDateFromHist.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            ldpDateToHist.SelectedDateTime = System.DateTime.Now;

            ldpDateFromHist.SelectedDataTimeChanged += ldpDateFrom_SelectedDataTimeChanged;
            ldpDateToHist.SelectedDataTimeChanged += ldpDateTo_SelectedDataTimeChanged;

            this.Dispatcher.BeginInvoke(new Action(() => {
                if(LoginInfo.USERTYPE == "P" && cboEquipmentHistory.SelectedIndex > 0)
                {
                    Verif_Tab.SelectedIndex = 1;
                    btnSearchHistory_Click(null, null);
                }
                     }));

            this.Loaded -= UserControl_Loaded;
        }


        private void SetInitColumn()
        {
            if(cboAreaVerification.SelectedValue.ToString() == "ED")
            {
                dgList.Columns["CSTID"].Visibility = Visibility.Visible;
                dgList.Columns["JUDG_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["HOLD_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["ST_WIP_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["RP_WIP_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["CT_WIP_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["WH_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["VERIF_SEQS"].Visibility = Visibility.Collapsed;
                dgList.Columns["REWND_QTY"].Visibility = Visibility.Visible;
                
            }
            else
            {
                dgList.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgList.Columns["JUDG_NAME"].Visibility = Visibility.Collapsed;
                dgList.Columns["HOLD_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["ST_WIP_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["RP_WIP_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["CT_WIP_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["WH_NAME"].Visibility = Visibility.Collapsed;
                dgList.Columns["VERIF_SEQS"].Visibility = Visibility.Visible;
                dgList.Columns["REWND_QTY"].Visibility = Visibility.Collapsed;
            }
           
         


            // cboEquipmentHistory.SelectedValue

        }

        /// <summary>
        /// 전기일 셋팅
        /// </summary>
        /// <returns></returns>
        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        #endregion


        #region Verification 탭

        /// <summary>
        /// 조회버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchVerification_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            grdInVerif.IsEnabled = false;
            _isExistVerifCode = false;
            _isCheckedVerifCode = false;

            if(cboProcessVerification.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1459"); //공정을 선택하세요
                return;
            }
            //if(cboEquipmentVerification.SelectedIndex == 0)
            //{
            //    Util.MessageValidation("SFU1153");//설비를  선택하세요
            //    return;
            //}
            GetLotList(txtLotVerification.Text.ToUpper().Trim());
        }


        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveVerification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // List에서 선택한 행의 Vefif_Code가 NULL(N/A)이면 =>  필수입력항목 CHECK
                if (cboVerification.IsEnabled)
                {
                    if (((cboVerification.SelectedIndex == 0) || ((cboVerificationReson.SelectedIndex == 0) && (cboVerification.SelectedValue.ToString() != "OK")) || (string.IsNullOrEmpty(txtUserName.Tag.ToString())))
                        || ((cboVerification.SelectedValue.ToString() == "HOLD") && (cboHoldType.SelectedIndex == 0)) )
                    {
                        Util.MessageValidation("SFU4979");
                        return;
                    }
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                        {
                            if (sresult == MessageBoxResult.OK)
                            {
                                DataTable inTable = new DataTable();
                                inTable.Columns.Add("LOTID", typeof(string));
                                inTable.Columns.Add("VERIF_CODE", typeof(string));
                                inTable.Columns.Add("VERIF_CMPL_FLAG", typeof(string));
                                inTable.Columns.Add("RSN_CODE", typeof(string));
                                inTable.Columns.Add("RSN_NOTE", typeof(string));
                                inTable.Columns.Add("REWND_WRK_ORD", typeof(string));
                                inTable.Columns.Add("REWND_EQPTID", typeof(string));
                                inTable.Columns.Add("REWND_WRK_DTTM", typeof(DateTime));
                                inTable.Columns.Add("REWND_NOTE", typeof(string));
                                inTable.Columns.Add("INSP_USERID", typeof(string));
                                inTable.Columns.Add("USERID", typeof(string));

                                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = True").CopyToDataTable();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    DataRow newRow = inTable.NewRow();
                                    newRow["LOTID"] = dr["LOTID"];
                                    newRow["VERIF_CODE"] = cboVerification.SelectedValue;
                                    newRow["VERIF_CMPL_FLAG"] = "Y";
                                    newRow["REWND_WRK_ORD"] = String.IsNullOrWhiteSpace(dr["REWND_WRK_ORD"].ToString()) ? txtRewndOrd.Text : dr["REWND_WRK_ORD"].ToString() + Environment.NewLine + txtRewndOrd.Text;
                                    if (cboVerificationReson.IsEnabled == true)
                                    {
                                        newRow["RSN_CODE"] = cboVerificationReson.SelectedValue;
                                    }
                                    else
                                    {
                                        if (cboVerification.SelectedValue.ToString() == "OK" && cboVerification.IsEnabled == true)
                                        {
                                            newRow["RSN_CODE"] = "VERIF_RSN_OK";
                                        }
                                        else
                                        {
                                            newRow["RSN_CODE"] = "VERIF_RSN_XX";
                                        }
                                    }

                                    newRow["RSN_NOTE"] = string.IsNullOrEmpty(txtReasonNote.Text) ? null : txtReasonNote.Text;
                                    //newRow["REWND_WRK_ORD"] = string.IsNullOrEmpty(txtRewndOrd.Text) ? null : txtRewndOrd.Text;
                                    newRow["INSP_USERID"] = txtUserName.Tag;
                                    newRow["USERID"] = LoginInfo.USERID;
                                    inTable.Rows.Add(newRow);
                                }

                                if (inTable.Rows.Count != 0)
                                {
                                    new ClientProxy().ExecuteService("BR_PRD_REG_ELTR_VERIF_HIST", "INDATA", "OUTDATA", inTable, (result, bizException) =>
                                    {
                                        try
                                        {
                                            HiddenLoadingIndicator();

                                            if (bizException != null)
                                            {
                                                Util.MessageException(bizException);
                                                return;
                                            }

                                            // List에서 선택한 행의 Vefif_Code가 NULL(N/A)인 것을    
                                            //    Verification 콤보에서 HOLD 선택했을때 추가 처리 (
                                            if (InputHoldArea.IsEnabled)
                                            {
                                                LotHold();
                                            }

                                            Util.MessageInfo("SFU1275"); // 정상처리되었습니다.


                                            SaveClearControl();
                                            grdInVerif.IsEnabled = false;
                                            _isExistVerifCode = false;
                                            _isCheckedVerifCode = false;

                                            if (cboProcessVerification.SelectedIndex == 0)
                                            {
                                                Util.MessageValidation("SFU1459"); //공정을 선택하세요
                                                return;
                                            }
                                            //if (cboEquipmentVerification.SelectedIndex == 0)
                                            //{
                                            //    Util.MessageValidation("SFU1153");//설비를  선택하세요
                                            //    return;
                                            //}
                                            GetLotList(txtLotVerification.Text.ToUpper().Trim());

                                            //btnSearchVerification_Click(null, null);
                                        }
                                        catch (Exception ex)
                                        {
                                            HiddenLoadingIndicator();
                                            Util.MessageException(ex);
                                        }
                                    });
                                }
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearVerification_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
        }


        /// <summary>
        /// 예상해제일 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.AlertInfo("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }


        /// <summary>
        /// 전기일 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtCalDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DATE"] = dtCalDate.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                dtCalDate.SelectedDateTime = dCalDate;
            }
        }


        /// <summary>
        /// LOTID 로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotVerification_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    if (txtLotVerification.Text.Trim() == string.Empty)
                        return;

                    string sLotid = txtLotVerification.Text.Trim();
                    if (dgList.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {
                                Util.Alert("SFU1504");   //동일한 LOT이 스캔되었습니다.
                                return;
                            }
                        }
                    }
                    GetLotList(sLotid);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// 붙여넣기 금지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotVerification_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string _ValueToFind = string.Empty;
                    Util.gridClear(dgList);
                    LotList.Clear();

                    if (sPasteStrings.Length > 0)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetLotList(sPasteStrings[i], false) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        _ValueToFind = string.Join(",", LotList);

                        if (_ValueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", _ValueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
                        }

                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLotVerification.Text = "";
                    txtLotVerification.Focus();

                    //HiddenLoadingIndicator();
                }
            }
        }


        /// <summary>
        /// HOLD 담당자
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

       
        /// <summary>
        /// Inspector 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }


        /// <summary>
        /// Inspector 텍스트박스 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        #region [담당자]
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtPerson.Text.Trim() == string.Empty)
                        return;

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtPerson.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        txtPerson.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                        txtPersonId.Text = dtRslt.Rows[0]["USERID"].ToString();
                        txtPersonDept.Text = dtRslt.Rows[0]["DEPTNAME"].ToString();
                    }
                    else
                    {
                        dgPersonSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgPersonSelect);

                        dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        this.Focusable = true;
                        this.Focus();
                        this.Focusable = false;
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion


        #region [담당자 검색결과 여러개일경우]
        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;

        }
        #endregion


        #region [Check ALL]
        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
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
            }));

        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = 'False'").ToList<DataRow>().ForEach(r => r["CHK"] = "True");
            dt.AcceptChanges();
            chkAll_Click();

            Util.DataGridCheckAllChecked(dgList);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = 'True'").ToList<DataRow>().ForEach(r => r["CHK"] = "False");
            dt.AcceptChanges();
            chkAll_Clear();

            Util.DataGridCheckAllUnChecked(dgList);
        }

        #endregion


        /// <summary>
        /// [대상 선택하기]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region 
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        #endregion
        private void chk_Click(object sender, RoutedEventArgs e)
        {
            dgList.Selection.Clear();

            DataRow[] drchk = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = True"); // CHK 된 ROW 배열

            CheckBox cb = sender as CheckBox;

            if (Convert.ToString(DataTableConverter.GetValue(cb.DataContext, "CHK")) == "True")     // 선택할 경우
            {
                if (!_isCheckedVerifCode) // 체크된게 없을 때
                {
                    _isCheckedVerifCode = true;

                    if ((_isExistVerifCode == false) && DataTableConverter.GetValue(cb.DataContext, "VERIF_CODE").ToString() != "N/A") // verif code 값 있으면 
                    {
                        _isExistVerifCode = true;
                    }
                }
                else if (_isExistVerifCode || DataTableConverter.GetValue(cb.DataContext, "VERIF_CODE").ToString() != "N/A") // 이미 체크된게 있거나 Verif code 값이 있는 row 선택
                {
                    if (drchk.Length == 1 && _isExistVerifCode == false) // chk된 row수가 1개
                    {
                        _isExistVerifCode = true;
                    }
                    else if (drchk.Length >= 1 && (from row in drchk select row["VERIF_CODE"]).Distinct().Count() == 1) // 같은 종류의 verif code 선택
                    {
                        _isCheckedVerifCode = true;
                    }
                    else if (cboProcessVerification.SelectedValue.Equals("E7000"))
                    {
                        _isCheckedVerifCode = true;
                    }
                    else if (cboAreaVerification.SelectedValue.Equals("ED"))
                    {
                        _isCheckedVerifCode = true;
                    }
                    else // 1) verif_code 가 있는 것은 하나만 선택되어야 하므로 더 이상 선택하지 못하게 함  2) 코드가 있는 것과 없은 것를 혼합 선택을 못하게 함
                    {
                        DataTableConverter.SetValue(cb.DataContext, "CHK", false);
                        return;
                    }
                }
            }
            else                  // 해제할 경우
            {
                _isExistVerifCode = false;

                if (drchk.Length == 0)
                {
                    _isCheckedVerifCode = false;
                }
            }

            if (_isExistVerifCode)
            {
                grdInVerif.IsEnabled = true;

                // cboVerification.SelectedIndexChanged -= cboVerification_SelectedIndexChanged;
                if (cboProcessVerification.SelectedValue.ToString() != "E7000" && cboAreaVerification.SelectedValue.ToString() != "ED") // 전극2동은 자동 판정 결과를 바꿀 수 있도록 요청함 (박상욱 사원)
                {
                    cboVerification.SelectedValue = DataTableConverter.GetValue(cb.DataContext, "VERIF_CODE");  // 선택한 값으로 setting
                    cboVerification.IsEnabled = false;
                    cboVerificationReson.IsEnabled = true;
                    txtUserName.IsEnabled = true;
                    btnReqUser.IsEnabled = true;
                    txtReasonNote.IsEnabled = true;
                    btnClearVerification.IsEnabled = false;
                    btnSaveVerification.IsEnabled = true;
                }
                else
                {
                    cboVerification.IsEnabled = true;
                    cboVerificationReson.IsEnabled = true;
                    txtUserName.IsEnabled = true;
                    btnReqUser.IsEnabled = true;
                    txtReasonNote.IsEnabled = true;
                    btnClearVerification.IsEnabled = true;
                    btnSaveVerification.IsEnabled = true;
                }
            }
            else
            {
                if (_isCheckedVerifCode)      // verif_code가 없는 것을 최초 선택했을 경우이거나   다수의 verif_code가 없는 것(N/A)을 선택한 상태에서 특정 하나를 해제할 경우
                {
                    if (grdInVerif.IsEnabled == false)  // 최초이면 입력을 위한 Setting
                    {
                        grdInVerif.IsEnabled = true;
                        cboVerification.IsEnabled = true;
                        cboVerificationReson.IsEnabled = true;
                        txtUserName.IsEnabled = true;
                        btnReqUser.IsEnabled = true;
                        txtReasonNote.IsEnabled = true;

                        btnClearVerification.IsEnabled = true;
                        btnSaveVerification.IsEnabled = true;
                    }
                }
                else     // 선택된 것이 하나도 없는 상태
                {
                    ClearControl();
                    grdInVerif.IsEnabled = false;
                }
            }
        }

        private void chkAll_Click()
        {
            dgList.Selection.Clear();   
            grdInVerif.IsEnabled = true;
            cboVerification.IsEnabled = true;
            cboVerificationReson.IsEnabled = true;
            txtUserName.IsEnabled = true;
            btnReqUser.IsEnabled = true;
            txtReasonNote.IsEnabled = true;

            btnClearVerification.IsEnabled = true;
            btnSaveVerification.IsEnabled = true;
            _isCheckedVerifCode = true;
        }
        private void chkAll_Clear()
        {
            ClearControl();
            grdInVerif.IsEnabled = false;
            _isCheckedVerifCode = false;
            _isExistVerifCode = false;
        }
        /// <summary>
        /// Input Verification Combo SelectedIndexChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboVerification_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (cboProcessVerification.SelectedValue.ToString() == "E7000" && cboVerification.SelectedValue.ToString() == "HOLD")
                    {
                        Util.MessageInfo("SFU8396");
                        cboVerification.SelectedIndexChanged -= cboVerification_SelectedIndexChanged;
                        cboVerification.SelectedIndex = 0;
                        cboVerification.SelectedIndexChanged += cboVerification_SelectedIndexChanged;
                        return;
                    }
                    switch (cboVerification.SelectedValue.ToString())
                    {
                        case "HOLD":
                            InputHoldArea.IsEnabled = true;
                            txtRewndOrd.Text = string.Empty;
                            txtRewndOrd.IsEnabled = false;
                            cboVerificationReson.IsEnabled = true;
                            break;
                        case "REWINDER":
                        case "SCRAP":
                            cboHoldType.SelectedIndex = 0;
                            txtHold.Text = string.Empty;
                            InputHoldArea.IsEnabled = false;
                            txtRewndOrd.IsEnabled = true;
                            cboVerificationReson.IsEnabled = true;
                            break;
                        case "OK":
                            cboHoldType.SelectedIndex = 0;
                            txtHold.Text = string.Empty;
                            InputHoldArea.IsEnabled = false;

                            txtRewndOrd.Text = string.Empty;
                            txtRewndOrd.IsEnabled = false;
                            cboVerificationReson.SelectedIndex = 0;
                            cboVerificationReson.IsEnabled = false;
                            break;
                        default:
                            cboVerificationReson.IsEnabled = true;
                            cboHoldType.SelectedIndex = 0;
                            txtHold.Text = string.Empty;
                            InputHoldArea.IsEnabled = false;
                            txtRewndOrd.Text = string.Empty;
                            txtRewndOrd.IsEnabled = false;
                            break;
                    }

                    SetVerificationResonCombo(cboVerificationReson);
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        
        #region History 탭

        /// <summary>
        /// 이력 조회 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetHoldHistory();
        }

        /// <summary>
        /// 이력 LOTID 텍스트박스 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotHistory_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetHoldHistory();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #endregion


        #region Mehod

        #region Verification 탭

        #region [작업대상 가져오기]

        /// <summary>
        /// 조회 BIZ
        /// </summary>
        /// <param name="sLotId"></param>
        /// <returns></returns>
        public bool GetLotList(string sLotId, bool vOnlyOne = true)
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("HOLD_YN", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaVerification.SelectedValue.ToString();
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegmentVerification.SelectedValue.ToString()) ? null : cboEquipmentSegmentVerification.SelectedValue.ToString();
                dr["PROCID"] = Util.GetCondition(cboProcessVerification) == "" ? null : Util.GetCondition(cboProcessVerification);
                dr["EQPTID"] = Util.GetCondition(cboEquipmentVerification) == "" ? null : Util.GetCondition(cboEquipmentVerification);
                dr["HOLD_YN"] = Util.GetCondition(cboHoldVerification) == "ALL" ? null : Util.GetCondition(cboHoldVerification);
                if (vOnlyOne && !string.IsNullOrWhiteSpace(txtLotVerification.Text))
                    dr["LOTID"] = txtLotVerification.Text;
                else if (!vOnlyOne)
                    dr["LOTID"] = sLotId; 

                if (!string.IsNullOrWhiteSpace(txtPrjtNameVerification.Text))
                    dr["PRJT_NAME"] = txtPrjtNameVerification.Text;
                if (!string.IsNullOrWhiteSpace(txtProdIDVerification.Text))
                    dr["PRODID"] = txtProdIDVerification.Text;
                if (!string.IsNullOrWhiteSpace(txtVersion.Text))
                    dr["PROD_VER_CODE"] = txtVersion.Text;

                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;

                if (dr["PROCID"].ToString().Equals("E7000"))
                    bizRuleName = "DA_PRD_SEL_WIP_FOR_VERIF";
                else
                    bizRuleName = "DA_PRD_SEL_ELTR_VERIF_TRGT";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                if (!vOnlyOne) // 복붙한 경우
                {
                    if (dtRslt.Rows.Count == 0)
                    {
                        LotList.Add(sLotId);
                        return true;

                    }
                    if (dgList.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID").ToString() == dtRslt.Rows[0]["LOTID"].ToString())
                            {
                                LotList.Add(sLotId);
                                return true;
                            }
                        }
                        DataTable dtSource = DataTableConverter.Convert(dgList.ItemsSource);
                        dtSource.Merge(dtRslt);

                        Util.gridClear(dgList);
                        dgList.ItemsSource = DataTableConverter.Convert(dtSource);
                    }
                    else
                    {
                        Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                    }
                    chkAll.IsChecked = true;
                }
                else // lotid 엔터 혹은 search 버튼
                {
                    Util.gridClear(dgList);
                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                        Util.gridClear(dgList);
                        return false;
                    }
                    if (dgList.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID").ToString() == dtRslt.Rows[0]["LOTID"].ToString())
                            {
                                Util.Alert("SFU1504");   //동일한 LOT이 스캔되었습니다.
                                return false;
                            }
                            DataTable dtSource = DataTableConverter.Convert(dgList.ItemsSource);
                            dtSource.Merge(dtRslt);

                            Util.gridClear(dgList);
                            dgList.ItemsSource = DataTableConverter.Convert(dtSource);
                        }
                    }
                    else
                    {
                        Util.GridSetData(dgList, dtRslt, FrameOperation, true);
                    }
                }


                //   ShowLoadingIndicator();

                //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                //{
                //    HiddenLoadingIndicator();

                //    if (bizException != null)
                //    {
                //        Util.MessageException(bizException);
                //        return;
                //    }

                //    Util.GridSetData(dgList, bizResult, FrameOperation, true);

                //});

                return true;
            }
            catch (Exception ex)
            {
             //   HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion


        #region hold 처리 -----  LotHold()
        private void LotHold()
        {
            string sHoldType = Util.GetCondition(cboHoldType, "SFU1342"); //"사유를 선택하세요" >> HOLD 사유를 선택 하세요.
            if (sHoldType.Equals(""))
            {
                return;
            }

            string sPerson = Util.GetCondition(txtPersonId);

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACTION_USERID", typeof(string));
            inDataTable.Columns.Add("CALDATE", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["LANGID"] = LoginInfo.LANGID;
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["ACTION_USERID"] = txtUserName.Tag; // txtPersonId.Text;
            row["CALDATE"] = dtCalDate.SelectedDateTime.ToString("yyyy-MM-dd");

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("HOLD_NOTE", typeof(string));
            inLot.Columns.Add("RESNCODE", typeof(string));
            inLot.Columns.Add("HOLD_CODE", typeof(string));
            inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

            ////라벨 발행용
            //DataRow row1 = null;
            //DataTable dtLabel = new DataTable();
            //dtLabel.Columns.Add("LOTID", typeof(string));
            //dtLabel.Columns.Add("RESNNAME", typeof(string));
            //dtLabel.Columns.Add("MODELID", typeof(string));
            //dtLabel.Columns.Add("WIPQTY", typeof(string));
            //dtLabel.Columns.Add("PERSON", typeof(string));

            DataTable dt = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = True").CopyToDataTable();

            foreach (DataRow dr in dt.Rows)
            {
                //for (int i = 0; i < dgList.Rows.Count; i++)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("True") ||
                //        Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                //    {
                row = inLot.NewRow();
                row["LOTID"] = dr["LOTID"]; // Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                row["HOLD_NOTE"] = !String.IsNullOrWhiteSpace(Util.NVC(dr["RSN_NOTE"])) ? Util.NVC(dr["RSN_NOTE"]) + Environment.NewLine + Util.GetCondition(txtHold) : Util.GetCondition(txtHold);
                row["RESNCODE"] = cboHoldType.SelectedValue;    // sHoldType;
                row["HOLD_CODE"] = cboHoldType.SelectedValue;   // sHoldType;
                row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);

                inLot.Rows.Add(row);

                //row1 = dtLabel.NewRow();
                //row1["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                //row1["RESNNAME"] = cboHoldType.Text;
                //row1["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MODELID"));
                //row1["WIPQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPQTY"))).ToString("###,###,##0.##");
                //row1["PERSON"] = txtPerson.Text;
                //dtLabel.Rows.Add(row1);
                //    }
                //}
            }

            try
            {
                //hold 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, inData);

                ////담당자에게 메일 보내기
                ////MailSend mail = new CMM001.Class.MailSend();
                ////mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, txtPersonId.Text, "", "HOLD 처리", "HOLD 내용");

                ////라벨발행

                //if (chkPrint.IsChecked.Equals(true))
                //{
                //    PrintHoldLabel(dtLabel);
                //}

                //Util.AlertInfo("SFU1344");  //HOLD 완료
                //Util.gridClear(dgList);


                //chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                //chkAll.IsChecked = false;
                //chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                //txtHold.Text = "";
                //txtPerson.Text = "";
                //txtPersonId.Text = "";
                //txtPersonDept.Text = "";
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_HOLD_LOT", ex.Message, ex.ToString());

            }
        }

        #endregion


        #region [Inspector]

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;

            }
        }

        #endregion


        private void ClearControl()
        {
            cboVerification.SelectedIndex = 0;
            cboVerificationReson.SelectedIndex = 0;
            cboVerificationReson.IsEnabled = true;
            txtUserName.Text = string.Empty;
            txtUserName.Tag = string.Empty;
            txtReasonNote.Text = string.Empty;
            txtRewndOrd.Text = string.Empty;
        }
        /// <summary>
        /// 저장 후 등록자명은 유지 시켜달라는 요청으로 추가함
        /// </summary>
        private void SaveClearControl()
        {
            cboVerification.SelectedIndex = 0;
            cboVerificationReson.SelectedIndex = 0;
            cboVerificationReson.IsEnabled = true;
            //txtUserName.Text = string.Empty;
            //txtUserName.Tag = string.Empty;
            txtReasonNote.Text = string.Empty;
        }

        #endregion


        #region History 탭

        #region [이력 가져오기]
        public void GetHoldHistory()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                //dtRqst.Columns.Add("HOLD_YN", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                //dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaHistory.SelectedValue.ToString();
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegmentHistory.SelectedValue.ToString()) ? null : cboEquipmentSegmentHistory.SelectedValue.ToString();
                dr["PROCID"] = Util.GetCondition(cboProcessHistory) == "" ? null : Util.GetCondition(cboProcessHistory);
                dr["EQPTID"] = Util.GetCondition(cboEquipmentHistory) == "" ? null : Util.GetCondition(cboEquipmentHistory);
                //dr["HOLD_YN"] = Util.GetCondition(cboHoldVerification) == "ALL" ? null : Util.GetCondition(cboHoldVerification);
                if (!string.IsNullOrWhiteSpace(txtLotHistory.Text))
                    dr["LOTID"] = txtLotHistory.Text;
                if (!string.IsNullOrWhiteSpace(txtPjtHistory.Text))
                    dr["PRJT_NAME"] = txtPjtHistory.Text;
                if (!string.IsNullOrWhiteSpace(txtProdID.Text))
                    dr["PRODID"] = txtProdID.Text;
                //if (!string.IsNullOrWhiteSpace(txtVersionHistory.Text))
                //    dr["PROD_VER_CODE"] = txtVersionHistory.Text;
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);

                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;

                bizRuleName = "DA_PRD_SEL_ELTR_VERIF_HISTORY";

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgListHistory, bizResult, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// cboVerification Code에  대한  Reason 코드값 콤보박스 셋팅
        /// </summary>
        /// <param name="cbo"></param>
        private void SetVerificationResonCombo(C1ComboBox cbo)
        {

            try
            {
             

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "VERIF_RSN_CODE";
                dr["ATTRIBUTE1"] = "Y";
                dr["ATTRIBUTE2"] = cboVerification.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);


                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = string.Empty;
                dtResult.Rows.InsertAt(drIns, 0);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                cbo.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        
        }


        #endregion
      
       
        /// <summary>
        /// HOLD 일경우 RESON 정보를 비고에 입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboVerification_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboVerification.SelectedValue.ToString() == "HOLD")
            {
                txtHold.Text = cboVerificationReson.Text.ToString() == "-SELECT-" ? string.Empty : cboVerificationReson.Text.ToString();
            }

        }

        private void cboProcessVerification_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // E7000공정이면 라인정보만 넘겨서 Verification으로 지정된 설비를 보여줌
            if (cboProcessVerification.SelectedValue.ToString() == "E7000")
            {
                cboHoldVerification.SelectedValue = "Y";
                cboHoldVerification.IsEnabled = false;
                C1ComboBox[] cboEquipmentParentVerification = { cboEquipmentSegmentVerification };
                _combo.SetCombo(cboEquipmentVerification, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParentVerification, sCase: "EQUIPMENT_VERIFICATION");
            }
            //이외 공정에서는 라인-공정의 설비
            else
            {
                cboHoldVerification.SelectedValue = "ALL";
                cboHoldVerification.IsEnabled = true;
                C1ComboBox[] cboEquipmentParentVerification = { cboEquipmentSegmentVerification, cboProcessVerification };
                _combo.SetCombo(cboEquipmentVerification, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParentVerification, sCase: "EQUIPMENT");
            }
        }

        private void cboAreaVerification_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAreaVerification.SelectedValue.ToString() == "ED")
            {
                dgList.Columns["CSTID"].Visibility = Visibility.Visible;
                dgList.Columns["JUDG_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["HOLD_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["ST_WIP_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["RP_WIP_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["CT_WIP_NOTE"].Visibility = Visibility.Visible;
                dgList.Columns["WH_NAME"].Visibility = Visibility.Visible;
                dgList.Columns["VERIF_SEQS"].Visibility = Visibility.Collapsed;
                dgList.Columns["REWND_QTY"].Visibility = Visibility.Visible;
            }
            else
            {
                dgList.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgList.Columns["JUDG_NAME"].Visibility = Visibility.Collapsed;
                dgList.Columns["HOLD_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["ST_WIP_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["RP_WIP_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["CT_WIP_NOTE"].Visibility = Visibility.Collapsed;
                dgList.Columns["WH_NAME"].Visibility = Visibility.Collapsed;
                dgList.Columns["VERIF_SEQS"].Visibility = Visibility.Visible;
                dgList.Columns["REWND_QTY"].Visibility = Visibility.Collapsed;
            }
        }
    }
}
