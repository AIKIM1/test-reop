/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리
--------------------------------------------------------------------------------------
 [Change History]
 -
 2019-11-18 최상민 : GMES Hold, QMS Hold건 보류재고 등록 관리 기능 추가
                     C20191104-000168 + GMES 상 보류재고 관리 시스템 개발(9월 요청 건) +  2200
 2020-08-11 김동일 : C20200711-000007 Cell Hold 해제(Excel) 기능 추가
 2021-01-18 cnskmaru C20210113-000001 GMES 시스템 포장 HOLD 이력 화면 UI 오류 수정 요청
 2022-12-28 안유수 : C20221128-000145 조회된 타입이 BOXID 이면 LOT HOLD 수정,해제 기능 안보이도록 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_213 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo2 = new CMM001.Class.CommonCombo();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string sEQSGID = string.Empty;

        private string maxHoldCell = string.Empty;
        private string cellDivideCnt = string.Empty;
        private DataTable _palletPack = new DataTable();
        private string _emptyPallet = string.Empty;
        private bool chkHold = false;


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
        public BOX001_213()
        {
            InitializeComponent();
        } 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            CellHoldVisible();
            TabPalletHoldVisible();
            if (bNcrHoldUser())
            {
                btnHoldStockRelease_Ncr.Visibility = Visibility.Visible;
            }else
            {
                btnHoldStockRelease_Ncr.Visibility = Visibility.Collapsed;
            }

            //// 자동차만 기능 사용.
            //btnSubLotRelease.Visibility;

            isVisibleBCD(LoginInfo.CFG_AREA_ID);
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo(); 
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase : "ALLAREA");
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase : "ALLAREA");
            _combo.SetCombo(cboArea_Ncr, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");

            //20200706 오화백
            _combo.SetCombo(cboAreaHold, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            string[] sFilter = { "HOLD_YN" };
            _combo.SetCombo(cboHoldYN2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

            string[] sFilter1 = { "HOLD_TRGT_CODE" };
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType.SelectedIndex = 1;
            
            _combo.SetCombo(cboLotType2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType2.SelectedIndex = 1;

            _combo.SetCombo(cboLotType_Ncr, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType_Ncr.SelectedIndex = 1;

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            if (LoginInfo.CFG_SHOP_ID == "A050"|| LoginInfo.CFG_SHOP_ID == "A040")
            {
                tiErpHold.Visibility = Visibility.Visible;
            }
            txtTotalSQty.Text = "0";
            txtChoiceQty.Text = "0";
        }

        private void CellHoldVisible()
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["COM_TYPE_CODE"] = "PACKING_CELL_HOLD_CNT";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_CODE"] = "MAX_CELL_CNT";
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    btnSublotHold.Visibility = Visibility.Visible;
                    btnSublotHold2.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnSublotHold.Visibility = Visibility.Collapsed;
                    btnSublotHold2.Visibility = Visibility.Visible;
                    maxHoldCell = dtResult.Rows[0]["ATTR1"].ToString();
                    cellDivideCnt = dtResult.Rows[0]["ATTR2"].ToString();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        /// <summary>
        /// 공통코드에 등록된 동에 따라 Pallet Hold 정보 탭을 보여줌
        /// 2020-07-25 오화백 추가
        /// </summary>

        private void TabPalletHoldVisible()
        {

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "MULTI_PALLET_HOLD_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {

                    ctbCreate.Visibility = Visibility.Visible;
                    cbtPalletHoldHist.Visibility = Visibility.Visible;

                }
                else
                {
                    ctbCreate.Visibility = Visibility.Collapsed;
                    cbtPalletHoldHist.Visibility = Visibility.Collapsed;
                }

           
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }




        private bool bNcrHoldUser()
        {
            bool user_chk = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "NCR_HOLD_RELEASE_AUTHORITY";
                dr["ATTRIBUTE2"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && LoginInfo.USERID.Equals(dtResult.Rows[0]["CBO_CODE"]))
                {
                    user_chk = true;
                }
                else
                {
                    user_chk = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return user_chk;
        }

        private void cboArea_Ncr_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea_Ncr.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            _combo2.SetCombo(cboEquipmentSegment_Ncr, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");

        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
           
        }
        
        //private void btnShift_Main_Click(object sender, RoutedEventArgs e)
        //{
        //    CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
        //    wndPopup.FrameOperation = this.FrameOperation;

        //    if (wndPopup != null)
        //    {
        //        object[] Parameters = new object[8];
        //        //Parameters[0] = LoginInfo.CFG_SHOP_ID;
        //        //Parameters[1] = LoginInfo.CFG_AREA_ID;
        //        //Parameters[2] = Util.NVC(cboLine.SelectedValue);
        //        //Parameters[3] = _PROCID;
        //        //Parameters[4] = Util.NVC(txtShift_Main.Tag);
        //        //Parameters[5] = Util.NVC(txtWorker_Main.Tag);
        //        //Parameters[6] = Util.NVC(cboEquipment_Search.SelectedValue);
        //        Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

        //        C1WindowExtension.SetParameters(wndPopup, Parameters);

        //        wndPopup.Closed += new EventHandler(wndShift_Main_Closed);

        //        // 팝업 화면 숨겨지는 문제 수정.
        //        //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        //        grdMain.Children.Add(wndPopup);
        //        wndPopup.BringToFront();
        //    }
        //}

        //private void wndShift_Main_Closed(object sender, EventArgs e)
        //{
        //    CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

        //    if (wndPopup.DialogResult == MessageBoxResult.OK)
        //    {
        //        txtShift_Main.Text = Util.NVC(wndPopup.SHIFTNAME);
        //        txtShift_Main.Tag = Util.NVC(wndPopup.SHIFTCODE);
        //        txtWorker_Main.Text = Util.NVC(wndPopup.USERNAME);
        //        txtWorker_Main.Tag = Util.NVC(wndPopup.USERID);              
        //    }
        //    this.grdMain.Children.Remove(wndPopup);
        //}
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom_Ncr_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Ncr.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Ncr.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_Ncr_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Ncr.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Ncr.SelectedDateTime;
                return;
            }
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void btnSearch_Ncr_Click(object sender, RoutedEventArgs e)
        {
            Search_Ncr();
        }
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
        }
        #endregion

        #region Hold 등록 팝업
        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            registeHold("SUBLOT");
        }

        private void puHold_Closed(object sender, EventArgs e)
        {
            BOX001_213_HOLD window = sender as BOX001_213_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnSublotHold2_Click(object sender, RoutedEventArgs e)
        {
            registeHoldCell("SUBLOT");
        }
        private void popupProgress_Closed(object sender, EventArgs e)
        {
            BOX001_213_HOLD_CELL window = sender as BOX001_213_HOLD_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion
        // 
        #region Hold 해제 팝업
        private void btnHoldRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                BOX001_213_UNHOLD puHold = new BOX001_213_UNHOLD();
                puHold.FrameOperation = FrameOperation;              

                object[] Parameters = new object[3];
                Parameters[0] = drList.CopyToDataTable();
                C1WindowExtension.SetParameters(puHold, Parameters);

                puHold.Closed += new EventHandler(puUnHold_Closed);
                                
                grdMain.Children.Add(puHold);
                puHold.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnHoldRelease_Ncr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                // SFU8131 보류재고를 해제하시겠습니까?
                Util.MessageConfirm("SFU8131", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
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
        }

        private void Save()
        {
            try
            {

               // DataTable dr = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);

                // DATA SET 
                DataSet inDataSet = new DataSet();
                //DataTable inTable = inDataSet.Tables.Add("INDATA");
                DataTable inTable = new DataTable("INDATA");

                inTable.Columns.Add("HOLD_GR_ID", typeof(string));              
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRowView rowview = null;
                /////////////////////////////////////////////////////////////////                
                foreach (C1.WPF.DataGrid.DataGridRow row in dgSearchResult_Ncr.Rows)
                {
                    rowview = row.DataItem as DataRowView;

                    //if (!String.IsNullOrEmpty(rowview["HOLD_GR_ID"].ToString()))
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                    {
                        DataRow dr = inTable.NewRow();
                        dr["HOLD_GR_ID"] = DataTableConverter.GetValue(row.DataItem, "HOLD_GR_ID"); ;
                        dr["UPDUSER"] = LoginInfo.USERID;

                        inTable.Rows.Add(dr);

                        //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_ASSY_NCR_HOLD", "INDATA", "OUTDATA", RQSTDT);
                        new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_TB_SFC_ASSY_LOT_HOLD_STCK_RELEASE", "INDATA", null, inTable);
                        inTable.Rows.Clear();
                    }
                }
                Util.AlertInfo("SFU1270");  //저장되었습니다.
                //dgHold.ItemsSource = null;
                Search_Ncr();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void puUnHold_Closed(object sender, EventArgs e)
        {
            BOX001_213_UNHOLD window = sender as BOX001_213_UNHOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }
        private void ERPRelese_Closed(object sender, EventArgs e)
        {
            BOX001_213_UNHOLD window = sender as BOX001_213_UNHOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search3();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion


        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_ASSY_HOLD_HIST     
        /// HOLD_FLAG = ‘Y’ , HOLD_TYPE_CODE = ‘SHIP_HOLD’ 고정
        /// </summary>
        private void Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("PRODID");
                RQSTDT.Columns.Add("HOLD_TYPE_CODE"); //2019.04.19 이제섭 HOLD_TYPE_CODE 추가
                RQSTDT.Columns.Add("SEARCH_GUBUN"); // 탭 조회 구분 ->  춣하HOLD등록 탭 : 'G'  NCR HOLD등록 탭 : 'Q'

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["HOLD_TYPE_CODE"] = "SHIP_HOLD"; //2019.04.19 이제섭 HOLD_TYPE_CODE 추가
                //dr["HOLD_TYPE_CODE"] = LoginInfo.CFG_SHOP_ID == "G182" ? null : "SHIP_HOLD"; //2019.05.07 이제섭 남경 파우치는 QMS HOLD도 해제 가능하게 Validation 해제 

                if (!string.IsNullOrEmpty(txtLotID.Text))
                {
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                }
                else
                {
                    dr["FROM_HOLD_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    //dr["AREAID"] = (string)cboArea.SelectedValue; //2020.07.16 오류 수정
                    dr["AREAID"] = (string)cboArea.SelectedValue == "" ? null : (string)cboArea.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType.SelectedValue;
                }

                if((string)cboLotType.SelectedValue == "BOX")
                {
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : getPalletBCD(txtLotID.Text.Trim());  // 팔레트바코드id -> boxid
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType.SelectedValue;
                    dr["FROM_HOLD_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["AREAID"] = (string)cboArea.SelectedValue == "" ? null : (string)cboArea.SelectedValue;
                }
                    
                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID.Text) ? null : txtCellID.Text;
                dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                dr["SEARCH_GUBUN"] = "G";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                if (dtResult.Rows.Count != 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["HOLD_TRGT_CODE"]) == "BOX")
                    {
                        btnUpdate.Visibility = Visibility.Collapsed;
                        btnHoldRelease.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnUpdate.Visibility = Visibility.Visible;
                        btnHoldRelease.Visibility = Visibility.Visible;
                    }
                }
                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void Search_Ncr()
        {
            try
            {
                if (cboLotType_Ncr.SelectedValue.ToString().Equals("SELECT"))
                {
                    // % 1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("LOT타입"));
                    return;
                }

                txtTotalSQty.Text = "0";
                txtChoiceQty.Text = "0";

                sEQSGID = Util.NVC(cboEquipmentSegment_Ncr.SelectedValue);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("PRODID");
                RQSTDT.Columns.Add("HOLD_TYPE_CODE"); //2019.04.19 이제섭 HOLD_TYPE_CODE 추가
                RQSTDT.Columns.Add("SEARCH_GUBUN"); // 탭 조회 구분 ->  춣하HOLD등록 탭 : 'G'  NCR HOLD등록 탭 : 'Q'
                RQSTDT.Columns.Add("HOLD_GR_ID");
                
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["HOLD_TYPE_CODE"] = "SHIP_HOLD"; //2019.04.19 이제섭 HOLD_TYPE_CODE 추가                
                dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID_Ncr.Text) ? null : txtLotID_Ncr.Text;                
                dr["FROM_HOLD_DTTM"] = dtpDateFrom_Ncr.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_HOLD_DTTM"] = dtpDateTo_Ncr.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = string.IsNullOrEmpty(sAREAID) ? null : sAREAID;
                dr["EQSGID"] = string.IsNullOrEmpty(sEQSGID) ? null : sEQSGID;
                dr["HOLD_TRGT_CODE"] = (string)cboLotType_Ncr.SelectedValue;
                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID_Ncr.Text) ? null : txtCellID_Ncr.Text;
                dr["PRODID"] = string.IsNullOrEmpty(txtProdID_Ncr.Text) ? null : txtProdID_Ncr.Text;
                dr["SEARCH_GUBUN"] = "Q";
                dr["HOLD_GR_ID"] = string.IsNullOrEmpty(txtHold_GR_ID_Ncr.Text) ? null : txtHold_GR_ID_Ncr.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                txtTotalSQty.Text = Convert.ToString(dtResult.Rows.Count);

                Util.GridSetData(dgSearchResult_Ncr, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void Search3()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("PRODID");
                RQSTDT.Columns.Add("HOLD_TYPE_CODE"); //2019.04.19 이제섭 HOLD_TYPE_CODE 추가

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID3.Text) ? null : txtLotID3.Text;
                //dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID.Text) ? null : txtCellID.Text;
                //dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                Util.GridSetData(dgSearchResult3, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }
        public void GetPalletList()
        {
            try
            {
                TextBox tb = txtPalletID;

                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // CELLID를 스캔 또는 입력하세요.
                    Util.MessageInfo("SFU1323");
                    return;
                }
                //20200706 오화백  동정보추가
                if (cboAreaHold.SelectedIndex == 0)
                {
                    // 동정보를 선택하세요
                    Util.MessageInfo("SFU4238");
                    return;
                }


                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_BOX_CP";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["BOXID"] = getPalletBCD(txtPalletID.Text.Trim());  // 팔레트바코드id -> boxid
                dr["AREAID"] = cboAreaHold.SelectedValue.ToString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU3394");
                    return;
                }

                if (dgPalletHoldInfo.GetRowCount() == 0)
                {
                    Util.GridSetData(dgPalletHoldInfo, dtResult, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgPalletHoldInfo.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPalletHoldInfo.Rows[i].DataItem, "BOXID")) == txtPalletID.Text)
                        {
                            Util.MessageInfo("SFU3164");
                            return;
                        }
                    }

                    DataTable dtInfo = DataTableConverter.Convert(dgPalletHoldInfo.ItemsSource);



                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgPalletHoldInfo, dtInfo, FrameOperation);
                }

                    _palletPack = DataTableConverter.Convert(dgPalletHoldInfo.GetCurrentItems());
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

        bool Multi_Cell(string pallet_ID)
        {
            try
            {
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_BOX_CP";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["BOXID"] = getPalletBCD(pallet_ID);  // 팔레트바코드id -> boxid
                dr["AREAID"] = cboAreaHold.SelectedValue.ToString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);


                if (dtResult.Rows.Count == 0)
                {
                    if (string.IsNullOrEmpty(_emptyPallet))
                        _emptyPallet += pallet_ID;
                    else
                        _emptyPallet = _emptyPallet + ", " + pallet_ID;
                }

                if (dgPalletHoldInfo.GetRowCount() == 0)
                {
                    Util.GridSetData(dgPalletHoldInfo, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgPalletHoldInfo.ItemsSource);

                    //20200707 오화백 추가 (동일한 데이터가 들어가면 제외)

                    if (dtInfo.Rows.Count > 0)
                    {
                        if (dtInfo.Select("BOXID = '" + pallet_ID + "'").Length > 0)
                        {
                            dtResult.Rows.Remove(dtResult.Select("BOXID = '" + pallet_ID + "'")[0]);
                         }
                    }
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgPalletHoldInfo, dtInfo, FrameOperation);
                }

                _palletPack = DataTableConverter.Convert(dgPalletHoldInfo.GetCurrentItems());

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName = txtUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += wndUser_Closed;
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private bool CanSave()
        {
                if (dgPalletHoldInfo.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2052");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtPalletNote.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtUserName.Tag?.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            return true;
        }

        private void Save_Pallet(bool chkHold)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                if (chkHold)
                {
                    const string bizRuleName = "BR_PRD_REG_ASSY_HOLD_PALLET";
                    _palletPack = DataTableConverter.Convert(dgPalletHoldInfo.GetCurrentItems());

                    //DataSet inData = new DataSet();

                    ////마스터 정보
                    //DataTable inTable = inData.Tables.Add("INDATA");
                    //inTable.Columns.Add("BOXID", typeof(string));
                    //inTable.Columns.Add("PACK_NOTE", typeof(string));
                    //inTable.Columns.Add("ISS_HOLD_FLAG", typeof(string));
                    //inTable.Columns.Add("USERID", typeof(string));

                    //DataRow row = inTable.NewRow();


                    //for (int i = 0; i < _palletPack.Rows.Count; i++)
                    //{
                    //    row = inTable.NewRow();
                    //    row["BOXID"] = Util.NVC(_palletPack.Rows[i]["BOXID"]);
                    //    row["PACK_NOTE"] = txtPalletNote.Text;
                    //    row["ISS_HOLD_FLAG"] = chkHold == true ? "Y" : "N";
                    //    row["USERID"] = txtUserName.Tag.ToString(); ;
                    //    inTable.Rows.Add(row);
                    //}

                    //new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, inData);

                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                    inDataTable.Columns.Add("UNHOLD_CHARGE_USERID", typeof(string));
                    inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                    inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                    inDataTable.Columns.Add("SHOPID", typeof(string));
                    inDataTable.Columns.Add("PACK_HOLD_FLAG", typeof(string));

                    DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                    inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                    inHoldTable.Columns.Add("STRT_SUBLOTID", typeof(string));
                    inHoldTable.Columns.Add("END_SUBLOTID", typeof(string));
                    inHoldTable.Columns.Add("HOLD_REG_QTY", typeof(Int32));

                    DataTable inTable = inDataSet.Tables["INDATA"];
                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["UNHOLD_CHARGE_USERID"] = LoginInfo.USERID;
                    newRow["HOLD_NOTE"] = txtPalletNote.Text;
                    newRow["HOLD_TRGT_CODE"] = "BOX";
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["PACK_HOLD_FLAG"] = "Y";
                    inDataTable.Rows.Add(newRow);
                    newRow = null;
              
                    for (int i = 0; i < _palletPack.Rows.Count; i++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["ASSY_LOTID"] = Util.NVC(_palletPack.Rows[i]["BOXID"]);
                        inHoldTable.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INHOLD", null, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }
                            //[%1] 개가 정상 처리 되었습니다.
                            Util.MessageInfo("SFU2056", _palletPack.Rows.Count);
                            Util.gridClear(dgPalletHoldInfo);
                            _emptyPallet = string.Empty;
                            _palletPack = new DataTable();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                    }, inDataSet);

                }
                else
                {
                    const string bizRuleName = "BR_PRD_REG_ASSY_UNHOLD_PALLET";
                    _palletPack = DataTableConverter.Convert(dgPalletHoldInfo.GetCurrentItems());

                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("UNHOLD_NOTE");
                    inDataTable.Columns.Add("SHOPID");

                    DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                    inHoldTable.Columns.Add("BOXID");

                    DataTable inTable = inDataSet.Tables["INDATA"];
                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["UNHOLD_NOTE"] = txtPalletNote.Text;
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    inDataTable.Rows.Add(newRow);
                    newRow = null;


                    for (int i = 0; i < _palletPack.Rows.Count; i++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["BOXID"] = Util.NVC(_palletPack.Rows[i]["BOXID"]);
                        inHoldTable.Rows.Add(newRow);
                    }


                    new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INHOLD", null, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            //[%1] 개가 정상 처리 되었습니다.
                            Util.MessageInfo("SFU2056", _palletPack.Rows.Count);
                            Util.gridClear(dgPalletHoldInfo);
                            _emptyPallet = string.Empty;
                            _palletPack = new DataTable();

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, inDataSet);
                }

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
        #endregion

        private void ErpHold()
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult3.ItemsSource);

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_SCHD_DATE");
                inDataTable.Columns.Add("UNHOLD_CHARGE_USERID");
                inDataTable.Columns.Add("HOLD_NOTE");
                inDataTable.Columns.Add("HOLD_TRGT_CODE");
                inDataTable.Columns.Add("SHOPID");


                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("ASSY_LOTID");
                //inHoldTable.Columns.Add("HOLD_TRGT_CODE");
                //inHoldTable.Columns.Add("MKT_TYPE_CODE");
                inHoldTable.Columns.Add("STRT_SUBLOTID");
                //inHoldTable.Columns.Add("END_SUBLOTID");
                inHoldTable.Columns.Add("HOLD_REG_QTY");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_SCHD_DATE"] = dtpUnholdDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["UNHOLD_CHARGE_USERID"] = LoginInfo.USERID;
                newRow["HOLD_NOTE"] = "ERP HOLD";//txtNote.Text;

                List<DataRow> drList = dtInfo.Select("CHK = 'True' AND HOLD_TRGT_CODE = 'LOT'").ToList();

                if (drList.Count > 0)
                {
                    newRow["HOLD_TRGT_CODE"] = "LOT";
                }
                else
                {
                    newRow["HOLD_TRGT_CODE"] = "SUBLOT";
                }
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);

                newRow = null;

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    if (dtInfo.Rows[row]["CHK"].ToString() == "True")
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["ASSY_LOTID"] = dtInfo.Rows[row]["ASSY_LOTID"];
                         newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["STRT_SUBLOTID"];
                        // newRow["END_SUBLOTID"] = dtInfo.Rows[row]["END_SUBLOTID"];
                        newRow["HOLD_REG_QTY"] = dtInfo.Rows[row]["HOLD_REG_QTY"];
                        inHoldTable.Rows.Add(newRow);
                    }
                }


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_ASSY_HOLD_ERP", "INDATA,INHOLD", null, inDataSet);


                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_HOLD_ERP", "INDATA,INHOLD", null, (result, exception) =>
                //{
                //    try
                //    {
                //        if (exception != null)
                //        {
                //            Util.MessageException(exception);
                //            return;
                //        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);

                //    }
                //}, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Search3();
            }
        }
        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("PRODID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtLotID2.Text))
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID2.Text) ? null : getPalletBCD(txtLotID2.Text.Trim());  // 팔레트바코드id -> boxid
                else
                {

                    dr["FROM_HOLD_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    //dr["AREAID"] = (string)cboArea2.SelectedValue; //2020.07.16 오류 수정
                    dr["AREAID"] = (string)cboArea2.SelectedValue == "" ? null : (string)cboArea2.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType2.SelectedValue;
                    dr["HOLD_FLAG"] = (string)cboHoldYN2.SelectedValue == "" ? null : (string)cboHoldYN2.SelectedValue;
                }

                if ((string)cboLotType2.SelectedValue == "BOX")
                {
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID2.Text) ? null : getPalletBCD(txtLotID2.Text.Trim());  // 팔레트바코드id -> boxid
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType2.SelectedValue;
                    dr["FROM_HOLD_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["AREAID"] = (string)cboArea2.SelectedValue == "" ? null : (string)cboArea2.SelectedValue;
                    dr["HOLD_FLAG"] = (string)cboHoldYN2.SelectedValue == "" ? null : (string)cboHoldYN2.SelectedValue;
                }


                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID2.Text) ? null : txtCellID2.Text;
                dr["PRODID"] = string.IsNullOrEmpty(txtProdID2.Text) ? null : txtProdID2.Text;


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                Util.GridSetData(dgHist, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'")?.ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                BOX001_213_UPDATE puUpdate = new BOX001_213_UPDATE();
                puUpdate.FrameOperation = FrameOperation;

                if (puUpdate != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = drList.CopyToDataTable();
                    C1WindowExtension.SetParameters(puUpdate, Parameters);

                    puUpdate.Closed += new EventHandler(puUpdate_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puUpdate);
                    puUpdate.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void puUpdate_Closed(object sender, EventArgs e)
        {
            BOX001_213_UPDATE window = sender as BOX001_213_UPDATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }

        #region  체크박스 선택 이벤트     
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        void checkAll_Ncr_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Ncr_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgSearchResult_Ncr_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Ncr_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Ncr_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Ncr_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Ncr_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgSearchResult_Ncr_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult_Ncr.GetCellFromPoint(pnt);
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                if (cell != null)
                {
                    if (cell.Row.Index < 0)
                        return;   

                    if (cell.Column.Name == "LOT_CNT")
                    {
                        //setDataGridCellToolTip(sender, e);             

                        string sHoldGRid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_GR_ID"].Index).Value);

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LANGID", typeof(string));
                        RQSTDT.Columns.Add("HOLD_GR_ID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["HOLD_GR_ID"] = sHoldGRid;
                        RQSTDT.Rows.Add(dr);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_GR_LIST", "RQSTDT", "OUTDATA", RQSTDT);

                        //txtTotalSQty.Text = Convert.ToString(dtResult.Rows.Count);
                        //Util.GridSetData(dgLotList, dtResult, FrameOperation, true);
                        if (dtResult.Rows.Count <= 0)
                        {
                            return;
                        }

                        string sToolTipText = "";
                        int lsCount = 0;

                        for (lsCount = 0; lsCount < dtResult.Rows.Count; lsCount++)
                        {
                            if(lsCount == 0)
                            {
                                if (Util.NVC(dtResult.Rows[lsCount]["HOLD_TRGT_CODE"]) == "SUBLOT")
                                {
                                    sToolTipText = Util.NVC(dtResult.Rows[lsCount]["STRT_SUBLOTID"]);
                                }else
                                {
                                    sToolTipText = Util.NVC(dtResult.Rows[lsCount]["ASSY_LOTID"]);
                                }
                                
                            }
                            else
                            {
                                if (Util.NVC(dtResult.Rows[lsCount]["HOLD_TRGT_CODE"]) == "SUBLOT")
                                {
                                    sToolTipText += "\n" + Util.NVC(dtResult.Rows[lsCount]["STRT_SUBLOTID"]);
                                }
                                else
                                {
                                    sToolTipText += "\n" + Util.NVC(dtResult.Rows[lsCount]["ASSY_LOTID"]);
                                }
                                    
                             }
                                
                        }
                        /*
                            Size size = new Size(100, 100);
                            ToolTipService.SetPlacementRectangle(cell.Presenter , new Rect(pnt, size));
                            //ToolTipService.SetPlacement(cell.Presenter, System.Windows.Controls.Primitives.PlacementMode.Relative);
                            ToolTipService.SetInitialShowDelay(cell.Presenter, 1);
                            ToolTipService.SetToolTip(cell.Presenter, sToolTipText);
                        */
                        ToolTip toolTip = new ToolTip();
                        Size size = new Size(100, 100);
                        toolTip.PlacementRectangle = new Rect(pnt, size);
                        toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                        toolTip.Content = sToolTipText;
                        toolTip.IsOpen = true;
                        //ToolTip = toolTip;

                        DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 3), IsEnabled = true };
                        timer.Tick += new EventHandler(delegate (object timerSender, EventArgs timerArgs)
                        {
                            if (toolTip != null)
                            {
                                toolTip.IsOpen = false;
                            }
                            toolTip = null;
                            timer = null;
                        });
                    }                       
                }
                //ToolTipService.SetToolTip(e.Cell.Presenter, "테스트테스트테스트테스트테스트테스트테스트테스트,테스트테스트테스트테스트테스트테스트\n테스트테스트\n");                       
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            
        }

        private void dgSearchResult_Ncr_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOT_CNT")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                        
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_STCK_FLAG")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }                        
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
/*
        private void dgSearchResult_Ncr_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                // datagrid.Selection.Add(datagrid.GetCell(datagrid.FrozenTopRowsCount, 0), datagrid.GetCell(datagrid.Rows.Count - 1, datagrid.Columns.Count - 2));
                return;
            }

            string sHoldGRid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["HOLD_GR_ID"].Index).Value);

            if (datagrid.CurrentColumn.Name == "LOT_CNT")
            {
                BOX001_213_LOT_LIST popUp = new BOX001_213_LOT_LIST();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = sHoldGRid;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndPackNote_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
        }

*/
        private void wndPackNote_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_213_LOT_LIST wndPopup = sender as BOX001.BOX001_213_LOT_LIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {               

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void dgSearchResult_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }        

        private void dgSearchResult_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = false;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_Ncr_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                if (chkGroupSelect_Ncr.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = true;
                        }
                    }
                }
                */
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                {
                    if (item["HOLD_STCK_FLAG"].Equals("Y"))
                    {
                       // item["CHK"] = false;
                    }
                }

                int sChoiceQty = 0;
                txtChoiceQty.Text = "0";
                
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("1") 
                        || Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("True")
                        && Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "HOLD_STCK_FLAG")).Equals("N")
                        )
                    {
                        sChoiceQty = sChoiceQty + 1;
                    }
                }
                

                txtChoiceQty.Text = Convert.ToString(sChoiceQty);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_Ncr_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect_Ncr.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                          //  item["CHK"] = false;
                        }

                    }
                }

                int sChoiceQty = 0;
                txtChoiceQty.Text = "0";
                
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("1") 
                        || Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("True")
                         && Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "HOLD_STCK_FLAG")).Equals("N")
                        )
                        
                    {
                        sChoiceQty = sChoiceQty + 1;
                    }
                }
                
                txtChoiceQty.Text = Convert.ToString(sChoiceQty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_Ncr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                if (chkGroupSelect_Ncr.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = true;
                        }
                    }
                }
                */
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearchResult_Ncr.ItemsSource)
                {
                    if (item["HOLD_STCK_FLAG"].Equals("Y"))
                    {
                       // item["CHK"] = false;
                    }
                }

                int sChoiceQty = 0;
                txtChoiceQty.Text = "0";
                for (int i = 0; i < dgSearchResult_Ncr.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("1") 
                        || Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "CHK")).Equals("True")
                         && Util.NVC(DataTableConverter.GetValue(dgSearchResult_Ncr.Rows[i].DataItem, "HOLD_STCK_FLAG")).Equals("N")                        
                        )
                    {
                        sChoiceQty = sChoiceQty + 1;
                    }
                }
                txtChoiceQty.Text = Convert.ToString(sChoiceQty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult3_Checked(object sender, RoutedEventArgs e)
        { 
            try
            {
                //DataTable dtInfo = DataTableConverter.Convert(dgSearchResult3.ItemsSource);

                //List<DataRow> drList = dtInfo.Select("CHK = 'True' AND HOLD_TYPE_CODE = 'ERP_HOLD'").ToList();
                //if (drList.Count > 0)
                //{
                //    //이미 ERP_HOLD 상태입니다.
                //    Util.MessageValidation("SFU2080");
                //    return;
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgSearchResult3_Unchecked(object sender, RoutedEventArgs e)
        {
           
            //try
            //{
            //    if (chkGroupSelect.IsChecked == true)
            //    {
            //        CheckBox cb = sender as CheckBox;
            //        if (cb?.DataContext == null) return;
            //        if (cb.IsChecked == null) return;

            //        DataRowView drv = cb.DataContext as DataRowView;

            //        foreach (DataRowView item in dgSearchResult3.ItemsSource)
            //        {
            //            if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
            //            {
            //                item["CHK"] = false;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

 
        private void btnBoxHold_Click(object sender, RoutedEventArgs e)
        {
            registeHold("BOX");
        }
        private void btnLotHold_Click(object sender, RoutedEventArgs e)
        {
            registeHold("LOT");
        }

        private void btnLotHold_Ncr_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeCell())
                return;

            registeHold_Ncr("NCR");
        } 

        private void btnLotHold_Excel_Click(object sender, RoutedEventArgs e)
        {
            registeHold_Ncr("EXCEL");
        }

        private bool CanChangeCell()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgSearchResult_Ncr, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void registeHold_Ncr(string PGubun)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);
                BOX001_213_NCR_HOLD puNcrHold = new BOX001_213_NCR_HOLD();
                if (PGubun == "NCR")
                {
                    List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                    if (drList.Count <= 0)
                    {
                        //SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }


                    
                    puNcrHold.FrameOperation = FrameOperation;

                    object[] Parameters = new object[2];                  
                    Parameters[0] = drList.CopyToDataTable();
                    Parameters[1] = PGubun;
                    C1WindowExtension.SetParameters(puNcrHold, Parameters);
                }
                else
                {
                    
                    puNcrHold.FrameOperation = FrameOperation;

                    object[] Parameters = new object[2];                    
                    Parameters[0] = "";
                    Parameters[1] = PGubun;
                    C1WindowExtension.SetParameters(puNcrHold, Parameters);
                }
                //Parameters[1] = PGubun;                

                //puNcrHold.Closed += new EventHandler(puUnHold_Closed);
                puNcrHold.Closed += new EventHandler(puNcrHold_Closed);

                grdMain.Children.Add(puNcrHold);
                puNcrHold.BringToFront();
                Search_Ncr();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void puNcrHold_Closed(object sender, EventArgs e)
        {
            BOX001_213_NCR_HOLD window = sender as BOX001_213_NCR_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search_Ncr();
            }
            this.grdMain.Children.Remove(window);            
        }

        private void registeHold(string holdTrgtCode)
        {
            try
            {
                BOX001_213_HOLD puHold = new BOX001_213_HOLD();
                puHold.FrameOperation = FrameOperation;

                if (puHold != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = holdTrgtCode;
                    //Parameters[1] = cboEquipment.SelectedValue.ToString();
                    //Parameters[2] = cboElecType.SelectedValue.ToString();
                    C1WindowExtension.SetParameters(puHold, Parameters);

                    puHold.Closed += new EventHandler(puHold_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puHold);
                    puHold.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void registeHoldCell(string holdTrgtCode)
        {
            try
            {
                BOX001_213_HOLD_CELL popupProgress = new BOX001_213_HOLD_CELL();
                popupProgress.FrameOperation = FrameOperation;

                if (popupProgress != null)
                {
                    object[] parameters = new object[5];
                    parameters[0] = holdTrgtCode;
                    parameters[1] = maxHoldCell;
                    parameters[2] = cellDivideCnt;
                    C1WindowExtension.SetParameters(popupProgress, parameters);
                    popupProgress.Closed += new EventHandler(popupProgress_Closed);
                    grdMain.Children.Add(popupProgress);
                    popupProgress.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnERPHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult3.ItemsSource);

            List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

            if (drList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }
          
     
                List<DataRow> drList2 = dtInfo.Select("CHK = 'True' AND HOLD_TYPE_CODE = 'ERP_HOLD'").ToList();
                if (drList2.Count > 0)
                {
                    //이미 ERP_HOLD 상태입니다.
                    Util.MessageValidation("SFU2080");
                    return;
                }

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {

                    string Lotchk = dtInfo.Rows[row]["ASSY_LOTID"].ToString();
                    string Typechk = dtInfo.Rows[row]["HOLD_TYPE_CODE"].ToString();
                    string Cellchk = dtInfo.Rows[row]["STRT_SUBLOTID"].ToString();
                    string Trgthk = dtInfo.Rows[row]["HOLD_TRGT_CODE"].ToString();

                    if (Lotchk.Length == 8)
                    {
                        //대LOT은 ERP_HOLD 할 수 없습니다.
                        Util.MessageValidation("SFU2082");
                        return;
                    }

                    if (dtInfo.Rows[row]["CHK"].ToString() == "True")
                    {
                        for (int row1 = 0; row1 < dtInfo.Rows.Count; row1++)
                        {
                            if (dtInfo.Rows[row1]["ASSY_LOTID"].ToString() == Lotchk && dtInfo.Rows[row1]["HOLD_TYPE_CODE"].ToString() == "ERP_HOLD" && dtInfo.Rows[row1]["STRT_SUBLOTID"].ToString() == Cellchk)
                            {
                                    //이미 ERP_HOLD 상태입니다.
                                    Util.MessageValidation("SFU2080");
                                    return;
                            }
                            else if(dtInfo.Rows[row1]["CHK"].ToString() == "True" && dtInfo.Rows[row1]["HOLD_TRGT_CODE"].ToString() != Trgthk)
                            {
                                //SUBLOT과 LOT은 같이 HOLD 할 수 없습니다.
                                Util.MessageValidation("SFU2083");
                                return;

                            }
                        }
   

                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            //SFU1345	HOLD 하시겠습니까?
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ErpHold();
                }

            });

        }

        private void btnSearch_Click3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtLotID3.Text))
                {
                    Util.gridClear(dgSearchResult3);
                    return;
                }
                else if(txtLotID3.Text.Length < 8)
                {
                    Util.gridClear(dgSearchResult3);
                    return;
                }                 
                Search3();  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnERPRelese_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult3.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList2 = dtInfo.Select("CHK = 'True' AND HOLD_TYPE_CODE IN ('SHIP_HOLD','QMS_HOLD')").ToList();

                if (drList2.Count > 0)
                {
                    //ERP_HOLD만 해제 할 수 있습니다.
                    Util.MessageValidation("SFU2081");
                    return;
                }

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    string Trgthk = dtInfo.Rows[row]["HOLD_TRGT_CODE"].ToString();

                    if (dtInfo.Rows[row]["CHK"].ToString() == "True")
                    {
                        for (int row1 = 0; row1 < dtInfo.Rows.Count; row1++)
                        {
                            if (dtInfo.Rows[row1]["CHK"].ToString() == "True" && dtInfo.Rows[row1]["HOLD_TRGT_CODE"].ToString() != Trgthk)
                            {
                                //SUBLOT과 LOT은 같이 처리 할 수 없습니다.
                                Util.MessageValidation("SFU2083");
                                return;

                            }
                        }
                    }
                }

                    BOX001_213_UNHOLD puHold = new BOX001_213_UNHOLD();
                puHold.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = drList.CopyToDataTable();
                C1WindowExtension.SetParameters(puHold, Parameters);

                puHold.Closed += new EventHandler(ERPRelese_Closed);

                grdMain.Children.Add(puHold);
                puHold.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgPalletHoldInfo);
            txtPalletID.Focus();
            _palletPack = null;
            txtPalletNote.Text = string.Empty;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((DataGridCellPresenter)((Button)sender).Parent).Row.Index;

                    dgPalletHoldInfo.IsReadOnly = false;
                    dgPalletHoldInfo.RemoveRow(index);
                    dgPalletHoldInfo.IsReadOnly = true;
                    _palletPack = DataTableConverter.Convert(dgPalletHoldInfo.GetCurrentItems());
                    txtPalletID.Focus();
                }
            });
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        private void btnPalletRelease_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            Util.MessageConfirm("SFU4046", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save_Pallet(false);
                }
                txtPalletID.Text = string.Empty;
                txtPalletNote.Text = string.Empty;
            });
        }

        private void btnPalletHold_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save_Pallet(true);
                }
                txtPalletID.Text = string.Empty;
                txtPalletNote.Text = string.Empty;
            });
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetPalletList();
            }
            txtPalletID.Focus();
        }

        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);


                    //20200706 오화백  동정보추가
                    if (cboAreaHold.SelectedIndex == 0)
                    {
                        // 동정보를 선택하세요
                        Util.MessageInfo("SFU4238");
                        return;
                    }

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_Cell(item) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (!string.IsNullOrEmpty(_emptyPallet))
                    {
                        Util.MessageValidation("SFU3588", _emptyPallet);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        _emptyPallet = string.Empty;
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
            txtPalletID.Focus();
        }

        private void txtPalletHold_KeyDown(object sender, KeyEventArgs e)
        {
            //C20210113-000001 
            if (e.Key == Key.Enter)
            {
                btnSearchHoldPallet_Click(null, null);
            }

            //C20210113-000001 이전 소스
            //try
            //{
            //    ShowLoadingIndicator();
            //    DoEvents();

            //    Util.gridClear(dgPalletHoldHistory);

            //    bool bLot = false;

            //    DataTable dtRqst = new DataTable();
            //    dtRqst.Columns.Add("BOXID", typeof(string));

            //    DataRow dr = dtRqst.NewRow();
            //    dr["BOXID"] = "SCRAP_SUBLOT";
            //    dtRqst.Rows.Add(dr);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_HOLD_HISTORY", "INDATA", "OUTDATA", dtRqst);

            //    Util.GridSetData(dgPalletHoldHistory, dtRslt, FrameOperation);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            //finally
            //{
            //    HiddenLoadingIndicator();
            //}
        }

        private void btnSearchHoldPallet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgPalletHoldHistory);

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("BOXID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));


                //C20210113-000001 변수 변경
                DataRow dr = dtRqst.NewRow();
                dr["BOXID"] = String.IsNullOrEmpty(txtPalletHold.Text) ? null : getPalletBCD(txtPalletHold.Text.Trim());
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom3);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo3);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_HOLD_HISTORY", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgPalletHoldHistory, dtRslt, FrameOperation);
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
        private void btnBoxRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_213_RELEASE_BOX_EXCL wndRelease = new BOX001_213_RELEASE_BOX_EXCL();
                wndRelease.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = "";
                C1WindowExtension.SetParameters(wndRelease, Parameters);

                wndRelease.Closed += new EventHandler(wndBoxRelease_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndRelease.ShowModal()));

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

        private void wndBoxRelease_Closed(object sender, EventArgs e)
        {
            //BOX001_213_RELEASE_BOX_EXCL window = sender as BOX001_213_RELEASE_BOX_EXCL;
            //if (window.DialogResult == MessageBoxResult.OK)
            //{
            //    Search();
            //}
        }

        private void btnSubLotRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_213_RELEASE_CELL_EXCL wndRelease = new BOX001_213_RELEASE_CELL_EXCL();
                wndRelease.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = "";
                C1WindowExtension.SetParameters(wndRelease, Parameters);

                wndRelease.Closed += new EventHandler(wndRelease_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndRelease.ShowModal()));

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

        private void wndRelease_Closed(object sender, EventArgs e)
        {
            BOX001_213_RELEASE_CELL_EXCL window = sender as BOX001_213_RELEASE_CELL_EXCL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
        }

        private void CboLotType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgSearchResult.ClearRows();
            if (cboLotType.SelectedValue.ToString().Equals("BOX"))
            {
                btnUpdate.Visibility = Visibility.Collapsed;
                btnHoldRelease.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnUpdate.Visibility = Visibility.Visible;
                btnHoldRelease.Visibility = Visibility.Visible;
            }
        }


        // 팔레트바코드ID -> BoxID
        private string getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
                }
                return palletid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void cboAreaHold_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sAreaID = Util.NVC(cboAreaHold.SelectedValue);
            // 팔레트 바코드 표시 설정
            if (dgPalletHoldInfo.Columns.Contains("PLLT_BCD_ID"))
            {
                if (_util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
                    dgPalletHoldInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                else
                    dgPalletHoldInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }

        }

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            // 팔레트 바코드 표시 설정
            if (dgPalletHoldHistory.Columns.Contains("PLLT_BCD_ID"))
            {
                if (_util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
                    dgPalletHoldHistory.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                else
                    dgPalletHoldHistory.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }
    }
}
