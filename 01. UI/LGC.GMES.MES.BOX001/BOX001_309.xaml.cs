/*************************************************************************************
 Created Date : 2021.01.07
      Creator : 이제섭
   Decription : 포장 Hold 관리 
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.24  DEVELOPER : Initial Created.
  2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
  2023.02.10  이윤중 : Pallet Hold 제약 조건 추가
  2023.06.01  조영대 : Pallet Barcode ID 컬럼 추가
  2023.06.21  박나연 : PALLET HOLD 이력 조회 시 기간 설정되지 않는 오류 수정
  2023.06.26  홍석원 : Pallet Barcode ID 컬럼 추가 (조회오류 수정)
  2023.07.24  홍석원 : TOP_PRODID 컬럼 및 조회 조건 추가
  2023.07.24  김동훈 : PALLET HOLD 이력 TOP_PRODID 추가
  2023.09.06  박나연 : 조립에서 사용하는 PALLET HOLD BR을 활성화 BR로 변경
  2023.10.27  김최일 : E20230727-001105 IRS/GMES 시스템 간 포장홀드관리 내 CELL ID 홀드/릴리즈 기능 인터페이스 개발 요청 건
  2024.05.16  남형희 : E20240422-000342 HOLD 해제 사유 Validation 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_309 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo2 = new CommonCombo();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string sEQSGID = string.Empty;

        private string maxHoldCell = string.Empty;
        private string cellDivideCnt = string.Empty;
        private DataTable _palletPack = new DataTable();
        private string _emptyPallet = string.Empty;


        DataGridRowHeaderPresenter pre = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center

        };
        public BOX001_309()
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
            SetEvent();


            txtTotalSQty.Text = "0";
            txtChoiceQty.Text = "0";

            //dgSearchResult.SetColumnVisibleForCommonCode("PLLT_BCD_ID", "CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);
            //dgHist.Columns["PLLT_BCD_ID"].Visibility = dgSearchResult.Columns["PLLT_BCD_ID"].Visibility;
            dgPalletHoldInfo.SetColumnVisibleForCommonCode("PLLT_BCD_ID", "CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);
            dgPalletHoldHistory.Columns["PLLT_BCD_ID"].Visibility = dgPalletHoldInfo.Columns["PLLT_BCD_ID"].Visibility;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
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

            // 2022.10.14  김용준: 보류재고 HOLD 등록 기능 추가
            _combo.SetCombo(cboArea_Ncr, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");

            //20200706 오화백
            _combo.SetCombo(cboAreaHold, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");



            string[] sFilter = { "HOLD_YN" };
            _combo.SetCombo(cboHoldYN2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

            string[] sFilter1 = { "HOLD_TRGT_CODE_FORM" };
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType.SelectedIndex = 1;
            
            _combo.SetCombo(cboLotType2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType2.SelectedIndex = 1;

            // 2022.10.14  김용준: 보류재고 HOLD 등록 기능 추가
            _combo.SetCombo(cboLotType_Ncr, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType_Ncr.SelectedIndex = 1;

            rdProdid.IsChecked = true;
            rdProdid2.IsChecked = true;
            rdProdid3.IsChecked = true;

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

         private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
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
            BOX001_309_HOLD window = sender as BOX001_309_HOLD;
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

                BOX001_309_UNHOLD puHold = new BOX001_309_UNHOLD();
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

        private void puUnHold_Closed(object sender, EventArgs e)
        {
            BOX001_309_UNHOLD window = sender as BOX001_309_UNHOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion


        #region Method
        /// <summary>
        /// 조회
        /// BIZ : DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST_F     
        /// HOLD_FLAG = ‘Y’ , HOLD_TYPE_CODE = ‘SHIP_HOLD’ 고정
        /// </summary>
        private void Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("FROM_HOLD_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_HOLD_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ASSY_LOTID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("HOLD_TYPE_CODE", typeof(string)); //2019.04.19 이제섭 HOLD_TYPE_CODE 추가
                RQSTDT.Columns.Add("TOP_PRODID", typeof(string)); // 2023.07.27 완제품 추가

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["HOLD_TYPE_CODE"] = "SHIP_HOLD";

                if (!string.IsNullOrEmpty(txtLotID.Text))
                {
                    dr["ASSY_LOTID"] = ConvertBarcodeId(txtLotID.Text.Trim());
                }
                else
                {
                    dr["FROM_HOLD_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd") + "000000";
                    dr["TO_HOLD_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd") + "235959";
                    dr["AREAID"] = (string)cboArea.SelectedValue == "" ? null : (string)cboArea.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType.SelectedValue;
                }

                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID.Text) ? null : txtCellID.Text;
                //dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                if (rdProdid.IsChecked == true)
                {
                    dr["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                }
                else
                {
                    dr["TOP_PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST_F", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
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

                string palletID = ConvertBarcodeId(txtPalletID.Text.Trim());

                const string bizRuleName = "DA_BAS_SEL_BOX_FOR_PALLET_HOLD"; //"DA_PRD_SEL_BOX_CP";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["BOXID"] = palletID;
                dr["AREAID"] = cboAreaHold.SelectedValue.ToString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU3394");
                    return;
                }

                foreach(DataRow drResult in dtResult.Rows)
                {
                    if (Util.NVC(drResult["RCV_ISS_ID"].ToString()) != ""
                        && Util.NVC(drResult["SHIP_ENABLE_FLAG"].ToString()) == "Y" )
                    {
                        // 활성화 창고에 있는 대상만 Hold가 가능합니다. 양품창고에 있는경우 Hold 할 수 없습니다.
                        //string message = "활성화 창고에 있는 대상만 Hold가 가능합니다. 양품창고(5000)에 있는경우 Hold 할 수 없습니다.";
                        //ControlsLibrary.MessageBox.Show(message, "", "Error", MessageBoxButton.OK, MessageBoxIcon.None, null);
                        Util.MessageInfo("SFU33941");
                        return;
                    }
                }

                if (dgPalletHoldInfo.GetRowCount() == 0)
                {
                    Util.GridSetData(dgPalletHoldInfo, dtResult, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgPalletHoldInfo.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPalletHoldInfo.Rows[i].DataItem, "BOXID")) == palletID)
                        {
                            //이미 입력된 Cell 입니다.
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

                pallet_ID = ConvertBarcodeId(pallet_ID);

                const string bizRuleName = "DA_PRD_SEL_BOX_CP";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["BOXID"] = pallet_ID;
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

                foreach (DataRow drResult in dtResult.Rows)
                {
                    if (Util.NVC(drResult["RCV_ISS_ID"].ToString()) != ""
                        && Util.NVC(drResult["SHIP_ENABLE_FLAG"].ToString()) == "Y")
                    {
                        // 활성화 창고에 있는 대상만 Hold가 가능합니다. 양품창고 있는경우 Hold 할 수 없습니다.
                        //string message = "활성화 창고에 있는 대상만 Hold가 가능합니다. 양품창고에 있는경우 Hold 할 수 없습니다.";
                        //ControlsLibrary.MessageBox.Show(message, "", "Error", MessageBoxButton.OK, MessageBoxIcon.None, null);
                        Util.MessageInfo("SFU33941");
                        return false;
                    }
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

            //if (string.IsNullOrWhiteSpace(txtPalletNote.Text))
            //{
            //    // 사유를 입력하세요.
            //    Util.MessageValidation("SFU1594");
            //    return false;
            //}
            //2024.05.16  남형희: E20240422 - 000342 HOLD 해제 사유 Validation 추가
            if (string.IsNullOrEmpty(txtPalletNote.Text) || txtPalletNote.Text.Trim().Length <= 3)
            {
                // HOLD 해제사유 예시                
                Util.MessageValidation("SFU10014");
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

                const string bizRuleName = "BR_SET_PALLET_HOLD_NOTE";

                _palletPack = DataTableConverter.Convert(dgPalletHoldInfo.GetCurrentItems());

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("PACK_NOTE", typeof(string));
                inTable.Columns.Add("ISS_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow row = inTable.NewRow();


                for (int i = 0; i < _palletPack.Rows.Count; i++)
                {
                    row = inTable.NewRow();
                    row["BOXID"] = Util.NVC(_palletPack.Rows[i]["BOXID"]);
                    row["PACK_NOTE"] = txtPalletNote.Text;
                    row["ISS_HOLD_FLAG"] = chkHold == true ? "Y" : "N";
                    row["USERID"] = txtUserName.Tag.ToString(); ;
                    inTable.Rows.Add(row);
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, inData);

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
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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
                RQSTDT.Columns.Add("HOLD_TYPE_CODE");
                RQSTDT.Columns.Add("SEARCH_GUBUN"); // 탭 조회 구분 ->  춣하HOLD등록 탭 : 'G'  NCR HOLD등록 탭 : 'Q'
                RQSTDT.Columns.Add("HOLD_GR_ID");
                RQSTDT.Columns.Add("TOP_PRODID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["HOLD_TYPE_CODE"] = "SHIP_HOLD";
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

                if (rdProdid2.IsChecked == true)
                {
                    dr["PRODID"] = string.IsNullOrEmpty(txtProdID_Ncr.Text) ? null : txtProdID_Ncr.Text;
                }
                else
                {
                    dr["TOP_PRODID"] = string.IsNullOrEmpty(txtProdID_Ncr.Text) ? null : txtProdID_Ncr.Text;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_NCR_LOT_HOLD_HIST_F", "INDATA", "OUTDATA", RQSTDT);
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void registeHold_Ncr(string PGubun)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult_Ncr.ItemsSource);
                BOX001_309_NCR_HOLD puNcrHold = new BOX001_309_NCR_HOLD();
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

        // 바코드 ID ==> Pallet ID 입력 변환
        private string ConvertBarcodeId(string lotId)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow drRqst = dtRqst.NewRow();
            drRqst["CSTID"] = lotId;
            dtRqst.Rows.Add(drRqst);

            DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", dtRqst);
            if (dtPallet != null && dtPallet.Rows.Count > 0)
            {
                return Util.NVC(dtPallet.Rows[0]["CURR_LOTID"]);
            }
            return lotId;
        }


        #endregion


        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }
        
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("FROM_HOLD_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_HOLD_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ASSY_LOTID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("TOP_PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtLotID2.Text))
                {
                    dr["ASSY_LOTID"] = ConvertBarcodeId(txtLotID2.Text.Trim());
                }                    
                else
                {
                    dr["FROM_HOLD_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd") + "000000";
                    dr["TO_HOLD_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd") + "235959";
                    //dr["AREAID"] = (string)cboArea2.SelectedValue; //2020.07.16 오류 수정
                    dr["AREAID"] = (string)cboArea2.SelectedValue == "" ? null : (string)cboArea2.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType2.SelectedValue;
                    dr["HOLD_FLAG"] = (string)cboHoldYN2.SelectedValue == "" ? null : (string)cboHoldYN2.SelectedValue;
                }
                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID2.Text) ? null : txtCellID2.Text;
                //dr["PRODID"] = string.IsNullOrEmpty(txtProdID2.Text) ? null : txtProdID2.Text;
                if (rdProdid3.IsChecked == true)
                {
                    dr["PRODID"] = string.IsNullOrEmpty(txtProdID2.Text) ? null : txtProdID2.Text;
                }
                else
                {
                    dr["TOP_PRODID"] = string.IsNullOrEmpty(txtProdID2.Text) ? null : txtProdID2.Text;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST_F", "INDATA", "OUTDATA", RQSTDT);
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
                HiddenLoadingIndicator();
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

        private void wndPackNote_Closed(object sender, EventArgs e)
        {
            BOX001_213_LOT_LIST wndPopup = sender as BOX001_213_LOT_LIST;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {               

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void dgSearchResult_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //E20230727-001105
                CheckBox cb = sender as CheckBox;

                if (((System.Data.DataRowView)cb.DataContext).Row["HOLD_REG_SYSTEM"].ToString().Equals("IRS_HOLD"))
                {
                    cb.IsChecked = false;

                    //SFU9335 IRS에서 보류한 건은 해제 할 수 없습니다.
                    Util.MessageValidation("SFU9335");
                }

                if (chkGroupSelect.IsChecked == true)
                {
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            if (!item["HOLD_REG_SYSTEM"].Equals("IRS_HOLD"))
                                item["CHK"] = true;
                            else
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
        private void btnLotHold_Click(object sender, RoutedEventArgs e)
        {
            registeHold("LOT");
        }

        private void registeHold(string holdTrgtCode)
        {
            try
            {
                BOX001_309_HOLD puHold = new BOX001_309_HOLD();
                puHold.FrameOperation = FrameOperation;

                if (puHold != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = holdTrgtCode;
                    C1WindowExtension.SetParameters(puHold, Parameters);

                    puHold.Closed += new EventHandler(puHold_Closed);               
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
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgPalletHoldHistory);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("BOXID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["BOXID"] = "SCRAP_SUBLOT";
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

        private void btnSearchHoldPallet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgPalletHoldHistory);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("BOXID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["BOXID"] = String.IsNullOrEmpty(txtPalletHold.Text) ? null : ConvertBarcodeId(txtPalletHold.Text);
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

        #region 보류재고 등록 Event

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void btnSearch_Ncr_Click(object sender, RoutedEventArgs e)
        {
            Search_Ncr();
        }


        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void dgSearchResult_Ncr_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void dgSearchResult_Ncr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void btnLotHold_Ncr_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeCell())
                return;

            registeHold_Ncr("NCR");
        }

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void puNcrHold_Closed(object sender, EventArgs e)
        {
            BOX001_309_NCR_HOLD window = sender as BOX001_309_NCR_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search_Ncr();
            }
            this.grdMain.Children.Remove(window);
        }

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void btnLotHold_Excel_Click(object sender, RoutedEventArgs e)
        {
            registeHold_Ncr("EXCEL");
        }

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void btnHoldRelease_Ncr_Click(object sender, RoutedEventArgs e)
        {
            
        }
        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
        private void dgSearchResult_Ncr_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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

        // 2022.10.14  김용준 : 보류재고 HOLD 등록 기능 추가
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
    }
}
