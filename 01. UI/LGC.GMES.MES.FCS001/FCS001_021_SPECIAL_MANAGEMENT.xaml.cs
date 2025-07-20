/*************************************************************************************
 Created Date : 2020.10.23
      Creator : PSM
   Decription : 특별관리등록
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.6    DEVELOPER : Initial Created.
  2022.12.07   형준우    : 특별관리등록 시 팝업창 안나오는 부분 수정
  2023.02.13   주훈종    : [E20230213-000118] 특별관리등록 기존 200개 제한에서 1000개 처리로 변경, 200개씩 나눠서 비즈룰 호출하는 방식으로 변경
  2023.04.03   권순범    : 특별관리등록 시 요청자 필수입력 수정
  2023.05.24   주훈종    : [E20230213-000118] 한번에 처리하는 건수를 1000 건으로 긴급 수정. 사유는 특별관리 트레이 번호가 별도로 따져서 임시조치먼저하고.
                           근본해결조치함. UI에서 특별관리 그룹ID를 먼저 채번하고 비즈룰 호출하는 방식으로 변경함   
  2024.01.04   이윤중    : [E20231218-000223] 특별관리등록 발번Rule 변경 : 동코드(2)+년(2)+월(2)+SEQ(4) > ex) A824010008
                           INDATA - CSTID 값 추가 (TRAY 정보로 동코드를 구분하기 위함)
  2024.05.03   임정훈    : [E20240417-001146] 특별관리유형 콤보박스에서 'N' 삭제, 등록/변경 및 권한에 따라 해제 버튼 추가, 단일 Tray 선택 시 콤보에서 이전 특별관리유형 값 기본으로 선택
  2024.08.27   이지은    : [E20240808-001322] 활성화 수동예약된 Tray 특별관리 지정시 수동예약 자동 취소
 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_021_SPECIAL_MANAGEMENT : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private string sTrayID = string.Empty;
        private string sSpcialType = string.Empty;
        private string sSpcialDesc = string.Empty;
        private string sShipmentYN = string.Empty;
        private string sSameEqp = string.Empty;
        public string sResultReturn = string.Empty;

        System.ComponentModel.BackgroundWorker bgWorker = null;

        public FCS001_021_SPECIAL_MANAGEMENT()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                sTrayID = Util.NVC(tmps[0]);
                sSameEqp = Util.NVC(tmps[1]);
            }

            //Combo Setting
            InitCombo();

            //Special정보 조회
            GetSpecialInfo();

            CheckFormAuthority(LoginInfo.USERID);
        }
        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            string[] sFilter = { "SPCL_FLAG" };
            CommonCombo_Form _combo = new CommonCombo_Form();
            _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);
            foreach (DataRowView row in cboSpecial.ItemsSource)//'N'인 combo 삭제
            {
                if (row["CBO_CODE"].ToString() == "N")
                {
                    row.Delete();
                }
            }
            cboSpecial.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(sSpcialType))
                cboSpecial.SelectedValue = sSpcialType;
        }
        #endregion

        #region [Method]
        private void GetSpecialInfo()
        {
            try
            {
                string[] arrTray = sTrayID.Split('|');

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                inDataTable.Rows.Add(dr);
                for (int i = 0; i < arrTray.Length; i++)
                {
                    inDataTable.Rows[0]["CSTID"] += arrTray[i] + ",";
                }
                //Tray 1 일경우 감안하여 마지막 , 제거
                inDataTable.Rows[0]["CSTID"] = inDataTable.Rows[0]["CSTID"].ToString().Substring(0, inDataTable.Rows[0]["CSTID"].ToString().Length - 1);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_SPECIAL_INFO_BY_TRAY_ID", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count == 1)
                {
                    //combo 기본 index를 기존 유형으로 선택
                    DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_SPECIAL_INFO_OP110", "INDATA", "OUTDATA", inDataTable);
                    string spclTypeCode = dtRslt2.Rows[0]["SPCL_TYPE_CODE"].ToString();
                    if (!string.IsNullOrEmpty(spclTypeCode))
                    {
                        int index = 0;
                        foreach (DataRowView row in cboSpecial.ItemsSource)//'N'인 combo 삭제
                        {
                            if (row["CBO_CODE"].ToString() == spclTypeCode)
                            {
                                cboSpecial.SelectedIndex = index;
                            }
                            index++;
                        }
                        txtSpecialDesc.Text = dtRslt.Rows[0]["SPCL_NOTE_CNTT"].ToString();  //SPECIAL_DESC
                        //txtSelReqID.Text = dtRslt.Rows[0]["REQ_USERID"].ToString().ToLower();  //REGUSERID
                        //txtSelReq.Text = dtRslt.Rows[0]["REGUSERNAME"].ToString();  //REGUSERNAME
                        sShipmentYN = dtRslt.Rows[0]["SHIP_ENABLE_FLAG"].ToString(); // SHIP_ENABLE_FLAG                  

                    }

                    if (sShipmentYN.Equals("N"))
                        chkShip.IsChecked = true;
                    else
                        chkShip.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSave.IsEnabled = false;
                //처리대상 선별
                if (string.IsNullOrWhiteSpace(txtSpecialDesc.Text) && !cboSpecial.SelectedValue.Equals("N"))
                {
                    Util.GetCondition(txtSpecialDesc, sMsg: "SFU1990"); //특별관리내역을 입력하세요.
                    btnSave.IsEnabled = true;
                    return;
                }
                else
                {
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("CSTID", typeof(string));             //TRAY_ID
                    inDataTable.Columns.Add("SPCL_TYPE_CODE", typeof(string));    //SPECIAL_YN
                    inDataTable.Columns.Add("SPCL_NOTE_CNTT", typeof(string));    //SPECIAL_DESC
                    inDataTable.Columns.Add("SHIP_ENABLE_FLAG", typeof(string));  //SHIPMENT_YN
                    inDataTable.Columns.Add("SAME_EQP_TRAY", typeof(string));     //SAME_EQP_TRAY 
                    inDataTable.Columns.Add("INSUSER", typeof(string));           //USERID
                    inDataTable.Columns.Add("UPDUSER", typeof(string));           //USERID
                    inDataTable.Columns.Add("REQ_USERID", typeof(string));        //REQ_USERID 추가
                    inDataTable.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(DateTime));//특별 예상 해제일 추가 2021.06.30 PSM
                    inDataTable.Columns.Add("SPCL_GR_ID", typeof(string));        //특별 그룹 ID

                    DataRow dr = inDataTable.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial);     //SPECIAL_YN
                    dr["SPCL_NOTE_CNTT"] = Util.GetCondition(txtSpecialDesc).Trim();  //SPECIAL_DESC
                    dr["SHIP_ENABLE_FLAG"] = (bool)chkShip.IsChecked ? "N" : "Y";  //SHIPMENT_YN
                    if (!string.IsNullOrEmpty(sSameEqp)) dr["SAME_EQP_TRAY"] = sSameEqp;

                    string sReqID = Util.GetCondition(txtSelReqID, sMsg: "FM_ME_0355"); //요청자를 입력해주세요.
                    if (string.IsNullOrEmpty(sReqID))
                    {
                        btnSave.IsEnabled = true;
                        return;
                    }
                    dr["REQ_USERID"] = sReqID;
                    dr["INSUSER"] = sReqID;
                    dr["UPDUSER"] = sReqID;

                    if ((bool)chkReleaseDate.IsChecked)  //특별 예상 해제일 추가 2021.06.30 PSM
                    {
                        dr["FORM_SPCL_REL_SCHD_DTTM"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                    }
                    inDataTable.Rows.Add(dr);


                    // [E20240808-001322] 특별관리 지정/변경 전 사용자가 출하 금지를 선택하였고(chkShip.lsChecked) 수동예약된 Tray가 있으면 팝업확인 후 예약취소 후 특별관리 지정함
                    DataTable dtManualTray = GetManualTray(sTrayID);

                    if ((bool)chkShip.IsChecked == true && dtManualTray.Rows.Count > 0)
                    {
                        Util.MessageConfirm("FM_ME_0621", (resultManual) => //수동 예약 걸린 트레이가 있습니다. 특별관리 지정시 수동예약은 취소됩니다. 계속 하시겠습니까? [Tray ID Count: {0} ]
                        {
                            if (resultManual != MessageBoxResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                try
                                {
                                    CancelReservationTray(dtManualTray);

                                    //bgWorker Asynd 통해서 Method Call
                                    xProgress.Percent = 0;
                                    xProgress.ProgressText = string.Empty;
                                    xProgress.Visibility = Visibility.Visible;
                                    loadingIndicator.Visibility = Visibility.Visible; //처리 전 LoadingIndicator 실행
                                    object[] argument = new object[5];
                                    argument[0] = inDataTable;

                                    if (!bgWorker.IsBusy)
                                    {
                                        bgWorker.RunWorkerAsync(argument);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }

                        }, new string[] { Convert.ToString(dtManualTray.Rows.Count) });

                    }
                    else
                    {
                        //bgWorker Asynd 통해서 Method Call
                        xProgress.Percent = 0;
                        xProgress.ProgressText = string.Empty;
                        xProgress.Visibility = Visibility.Visible;
                        loadingIndicator.Visibility = Visibility.Visible; //처리 전 LoadingIndicator 실행
                        object[] argument = new object[5];
                        argument[0] = inDataTable;

                        if (!bgWorker.IsBusy)
                        {
                            bgWorker.RunWorkerAsync(argument);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

            loadingIndicator.Visibility = Visibility.Collapsed;
            //Close();
        }
        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {

            //확인 메세지 팝업

            Util.MessageConfirm("SFU5092", (result) =>  //특별관리 해제를 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        btnSave.IsEnabled = false;
                        //처리대상 선별
                        //if (string.IsNullOrWhiteSpace(txtSpecialDesc.Text) && !cboSpecial.SelectedValue.Equals("N"))
                        //{
                        //    Util.GetCondition(txtSpecialDesc, sMsg: "SFU1990"); //특별관리내역을 입력하세요.
                        //    btnSave.IsEnabled = true;
                        //    return;
                        //}
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));             //TRAY_ID
                        inDataTable.Columns.Add("SPCL_TYPE_CODE", typeof(string));    //SPECIAL_YN
                        inDataTable.Columns.Add("SPCL_NOTE_CNTT", typeof(string));    //SPECIAL_DESC
                        inDataTable.Columns.Add("SHIP_ENABLE_FLAG", typeof(string));  //SHIPMENT_YN
                        inDataTable.Columns.Add("SAME_EQP_TRAY", typeof(string));     //SAME_EQP_TRAY 
                        inDataTable.Columns.Add("INSUSER", typeof(string));           //USERID
                        inDataTable.Columns.Add("UPDUSER", typeof(string));           //USERID
                        inDataTable.Columns.Add("REQ_USERID", typeof(string));        //REQ_USERID 추가
                        inDataTable.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(string));//특별 예상 해제일 추가 2021.06.30 PSM
                        inDataTable.Columns.Add("SPCL_GR_ID", typeof(string));        //특별 그룹 ID

                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["SPCL_TYPE_CODE"] = "N";    //SPECIAL_YN
                        dr["SPCL_NOTE_CNTT"] = Util.GetCondition(txtSpecialDesc).Trim();  //SPECIAL_DESC
                        dr["SHIP_ENABLE_FLAG"] = (bool)chkShip.IsChecked ? "N" : "Y";  //SHIPMENT_YN
                        if (!string.IsNullOrEmpty(sSameEqp)) dr["SAME_EQP_TRAY"] = sSameEqp;

                        string sReqID = Util.GetCondition(txtSelReqID, sMsg: "FM_ME_0355"); //요청자를 입력해주세요.
                        if (string.IsNullOrEmpty(sReqID))
                        {
                            btnSave.IsEnabled = true;
                            return;
                        }
                        dr["REQ_USERID"] = sReqID;
                        dr["INSUSER"] = sReqID;
                        dr["UPDUSER"] = sReqID;

                        if ((bool)chkReleaseDate.IsChecked)  //특별 예상 해제일 추가 2021.06.30 PSM
                        {
                            dr["FORM_SPCL_REL_SCHD_DTTM"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                        }
                        inDataTable.Rows.Add(dr);


                        //bgWorker Asynd 통해서 Method Call
                        xProgress.Percent = 0;
                        xProgress.ProgressText = string.Empty;
                        xProgress.Visibility = Visibility.Visible;
                        loadingIndicator.Visibility = Visibility.Visible; //처리 전 LoadingIndicator 실행
                        object[] argument = new object[5];
                        argument[0] = inDataTable;

                        if (!bgWorker.IsBusy)
                        {
                            bgWorker.RunWorkerAsync(argument);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }


                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                else
                {
                    return;
                }
            });
        }

        private int RegSpecialTrayInfo(object arg)
        {
            int bResult = 0; //0: false 미처리(오류), 1: true 정상처리

            try
            {
                //[E20230213-000118] 특별관리등록 기존 200개 제한에서 1000개 처리로 변경, 200개씩 나눠서 비즈룰 호출하는 방식으로 변경
                int iProcessingCnt = 100; //한번에 처리하는 Tray 건수
                double dNumberOfProcessingCnt = 0.0;
                double dArrayCnt = 0.0;
                String sArrayList = "";
                object[] argument = (object[])arg;
                DataTable inDataTable = (DataTable)argument[0];
                string cstid = string.Empty;

                //2024.01.04 - [E20231218-000223] 특별관리등록 발번Rule 변경 : 동코드(2)+년(2)+월(2)+SEQ(4) > ex) A824010008
                //              INDATA - CSTID 값 추가 (TRAY 정보로 동코드를 구분하기 위함)
                if (sTrayID.Split('|').Length > 0)
                {
                    //맨 첫번째 TRAY의 정보를 대표로 특별관리번호 발번
                    cstid = sTrayID.Split('|')[0];
                    inDataTable.Rows[0]["CSTID"] = cstid;
                }

                //특별관리 그룹ID 채번
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TRAY_SPCL_GR_ID", "INDATA", "OUTDATA", inDataTable);


                //CSR_ID: E20230213-000118
                string[] arrTray = sTrayID.Split('|');
                dNumberOfProcessingCnt = Math.Ceiling(arrTray.Length / (double)iProcessingCnt);//처리수량
                DataTable dsData = null;
                int runCount = 0; //progress_bar current count
                int totalCount = arrTray.Length;//total cnt
                for (int k = 0; k < dNumberOfProcessingCnt; k++) //나눠서 처리
                {
                    sArrayList = ""; //Initialization
                    bgWorker.ReportProgress((runCount * 100) / totalCount, "Processing");
                    for (int i = (k * (int)iProcessingCnt); i < (k * iProcessingCnt + iProcessingCnt); i++)
                    {
                        if (i >= arrTray.Length) break;
                        sArrayList += arrTray[i] + ","; //TRAY_ID
                        runCount++;//for progress bar
                    }

                    inDataTable.Rows[0]["CSTID"] = sArrayList;
                    //Tray 1 일경우 감안하여 마지막 , 제거
                    inDataTable.Rows[0]["CSTID"] = inDataTable.Rows[0]["CSTID"].ToString().Substring(0, inDataTable.Rows[0]["CSTID"].ToString().Length - 1); //TRAY_ID

                    inDataTable.Rows[0]["SPCL_GR_ID"] = dtRslt.Rows[0]["SPCL_GR_ID"].ToString();
                    dsData = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_SPECIAL", "INDATA", "OUTDATA", inDataTable);
                    if (dsData.Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        bResult = 1;
                    }
                    else
                    {
                        bResult = 0;
                    }


                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

            return bResult;

        }



        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        #region [ PopUp Event ]

        #region < 담당자 찾기 >

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtSelReq.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            //grdMain.Children.Add(wndPerson); _grid     
            this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtSelReq.Text = wndPerson.USERNAME;
                txtSelReqID.Text = wndPerson.USERID;
            }
        }

        #endregion

        #endregion

        private void cboSpecial_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            /* if (e.NewValue.ToString().Equals("N"))
             {
                 txtSpecialDesc.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                 txtSelReq.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
                 txtSelReqID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
             }
             else
             {
                 txtSpecialDesc.Style = Application.Current.Resources["Content_InputForm_MandatoryTextBoxStyle"] as Style;
                 txtSelReq.Style = Application.Current.Resources["Content_InputForm_MandatoryTextBoxStyle"] as Style;
                 txtSelReqID.Style = Application.Current.Resources["Content_InputForm_MandatoryTextBoxStyle"] as Style;
             }
             txtSpecialDesc.Text = null;
             txtSelReq.Text = null;
             txtSelReqID.Text = null;*/
        }

        private void chkReleaseDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
        }

        private void chkReleaseDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
        }


        private void BgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = RegSpecialTrayInfo(e.Argument);
        }

        private void BgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            xProgress.Percent = e.ProgressPercentage;
            xProgress.ProgressText = e.UserState == null ? "" : e.UserState.ToString();
        }

        private void BgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException((Exception)e.Result);
                }
                else if (e.Result != null && e.Result is int)
                {
                    int iResult = (int)e.Result;

                    if (iResult == 1)
                    {
                        Util.MessageInfo("FM_ME_0423"); //특별등록 완료되었습니다.
                        sResultReturn = "Y";
                    }
                    else
                    {
                        Util.MessageInfo("FM_ME_0424"); //특별등록 실패하였습니다.
                        sResultReturn = "N";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            xProgress.Visibility = Visibility.Collapsed;
            xProgress.Percent = 0;
            Close();
        }

        // [ksb1223] 엔터입력 후 ID찾기
        private void txtSelReq_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtSelReq.Text))
                        return;

                    //초기화
                    txtSelReqID.Text = "";

                    //
                    GetUserWindow();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void CheckFormAuthority(string sUserID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = sUserID;
                newRow["AUTHID"] = "FORM_AUTO_MANA";
                newRow["USE_FLAG"] = 'Y';
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    btnRelease.Visibility = Visibility.Visible;
                }
                else
                {
                    btnRelease.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnRelease.Visibility = Visibility.Collapsed;
            }
        }

        #region #20240826 수동예약중인 Tray가 있는지 확인 후 수동 예약 취소
        private DataTable GetManualTray(string sCstID)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("PRIO_MANUAL_USE_Y", typeof(string));
                RQSTDT.Columns.Add("AGING_USE_Y", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; ;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTID"] = sCstID;
                dr["PRIO_MANUAL_USE_Y"] = "Y";
                dr["AGING_USE_Y"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_SAME_EQP_MANUAL", "RQSTDT", "RSLTDT", RQSTDT);

                return dtRslt;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                throw ex;
            }
        }

        private int CancelReservationTray(DataTable ManualTrayList)
        {
            int bResult = 0; //0: false 미처리(오류), 1: true 정상처리

            try
            {
                DataTable dtCancelTray = new DataTable();
                dtCancelTray.TableName = "INDATA";
                dtCancelTray.Columns.Add("SRCTYPE", typeof(string));
                dtCancelTray.Columns.Add("IFMODE", typeof(string));
                dtCancelTray.Columns.Add("LOTID", typeof(string));
                dtCancelTray.Columns.Add("UNITID", typeof(string));
                dtCancelTray.Columns.Add("CANCEL_YN", typeof(string));
                dtCancelTray.Columns.Add("USERID", typeof(string));
                dtCancelTray.Columns.Add("MENUID", typeof(string));
                dtCancelTray.Columns.Add("USER_IP", typeof(string));
                dtCancelTray.Columns.Add("PC_NAME", typeof(string));
                dtCancelTray.Columns.Add("AREAID", typeof(string));

                foreach (DataRow drar in ManualTrayList.Rows)
                {
                    DataRow drCancel = dtCancelTray.NewRow();
                    drCancel["SRCTYPE"] = "UI";
                    drCancel["IFMODE"] = "OFF";
                    drCancel["LOTID"] = Util.NVC(drar["LOTID"]);
                    drCancel["UNITID"] = Util.NVC(drar["EQPTID"]);
                    drCancel["CANCEL_YN"] = "Y";
                    drCancel["USERID"] = LoginInfo.USERID;
                    drCancel["MENUID"] = LoginInfo.CFG_MENUID;
                    drCancel["USER_IP"] = LoginInfo.USER_IP;
                    drCancel["PC_NAME"] = LoginInfo.PC_NAME;
                    drCancel["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dtCancelTray.Rows.Add(drCancel);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_AGING_UNLOAD_RESERVATION", "INDATA", "OUTDATA", dtCancelTray, menuid: FrameOperation.MENUID.ToString());
                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    return Util.NVC_Int(dtRslt.Rows[0]["RESER_CNT"]);
                }
                else
                {
                    return 0;
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return bResult;

        }
        #endregion

    }
    #endregion

}
