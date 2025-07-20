/*************************************************************************************
 Created Date : 2020.03.24
      Creator : 
   Decription : 21700 Tesla 자동포장기 실적처리 및 포장출고 화면.
--------------------------------------------------------------------------------------
 [Change History]
  날짜        버젼  수정자   CSR              내용
 -------------------------------------------------------------------------------------
 2020.03.24  0.1   이현호       21700 자동포장기 실적처리 및 포장출고 신규화면 개발.
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.UserControls;
using System.Linq;
using LGC.GMES.MES.CMM001;



namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_242 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _util = new Util();
        private static string CREATED = "CREATED,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private static string FINISH_RECEIVE = "FINISH_RECEIVE,";

        private string _searchStat = string.Empty;
        //private string _searchShipStat = string.Empty;

        private bool bInit = true;
        /*컨트롤 변수 선언*/
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        private string _processCode;

        #region CheckBox
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

        #endregion


        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;
        DataRow _drPrtInfo = null;

        public BOX001_242()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.CircularCharacteristicGrader;
            ApplyPermissions();
            InitControl();
            SetEvent();
            bInit = false;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            SetEquipment();
        }

        private void SetEquipment()
        {
            try
            {
                cboEquipment.ItemsSource = null;

                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = Process.CircularCharacteristicGrader;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "- SELECT -";
                drIns["CBO_CODE"] = null;
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);

                var query = (from t in dtResult.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == LoginInfo.CFG_EQPT_ID
                             select t).FirstOrDefault();
                if (query != null)
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Events

        #region 텍스트 박스 포커스 : text_GotFocus()
        /// <summary>
        /// 텍스트 박스 포커스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            GetPalletList();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 포장대기, 포장완료, 출고요청 이벤트 : chkSearch_Checked(), chkSearch_Unchecked()
        /// <summary>
        /// 체크박스 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSearch_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    _searchStat += CREATED;
                    break;
                case "chkPacked":
                    _searchStat += PACKED;
                    break;
                case "chkShipping":
                    //_searchShipStat += SHIPPING;
                    _searchStat += SHIPPING;
                    break;
                case "chkFinishReceive":
                    //_searchShipStat += FINISH_RECEIVE;
                    _searchStat += FINISH_RECEIVE;
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            // bInit = false;
        }
        /// <summary>
        /// 체크박스 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    if (_searchStat.Contains(CREATED))
                        _searchStat = _searchStat.Replace(CREATED, "");
                    break;
                case "chkPacked":
                    if (_searchStat.Contains(PACKED))
                        _searchStat = _searchStat.Replace(PACKED, "");
                    break;
                case "chkShipping":
                    //if (_searchShipStat.Contains(SHIPPING))
                    //    _searchShipStat = _searchShipStat.Replace(SHIPPING, "");
                    if (_searchStat.Contains(SHIPPING))
                        _searchStat = _searchStat.Replace(SHIPPING, "");
                    break;
                case "lblShipped":
                    //if (_searchShipStat.Contains(FINISH_RECEIVE))
                    //    _searchShipStat = _searchShipStat.Replace(FINISH_RECEIVE, "");
                    if (_searchStat.Contains(FINISH_RECEIVE))
                        _searchStat = _searchStat.Replace(FINISH_RECEIVE, "");
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            //  bInit = false;
        }



        #endregion

        #region 작업 Pallet 스프레드 이벤트
        /// <summary>
        /// Pallet ID 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPalletListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;


            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgPalletList.SelectedIndex = idx;
            }
        }
        #endregion

        #region InBoxtype 유형 설정 창 열기 : btnInboxType_Click()
        private void btnInboxType_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(Util.NVC(cboEquipment.SelectedValue)))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            BOX001_EQPT_INBOX_TYPE popupInboxTyp = new BOX001_EQPT_INBOX_TYPE { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupInboxTyp.Name.ToString()) == false)
                return;


            object[] parameters = new object[3];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(cboEquipment.SelectedValue);
            parameters[2] = Util.NVC(cboEquipment.Text);
            C1WindowExtension.SetParameters(popupInboxTyp, parameters);

            popupInboxTyp.Closed += new EventHandler(popupInboxTyp_Closed);
            grdMain.Children.Add(popupInboxTyp);
            popupInboxTyp.BringToFront();
        }



        #region InBoxtype 유형 설정 창 닫기 :popupInboxTyp_Closed
        private void popupInboxTyp_Closed(object sender, EventArgs e)
        {
            BOX001_EQPT_INBOX_TYPE popup = sender as BOX001_EQPT_INBOX_TYPE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);

        }
        #endregion


        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Pallet 생성 : btnRun_Click(), puRun_Closed()
        /// <summary>
        /// Pallet 생성
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtShift.Text))
            {
                // SFU1845 작업조를 입력하세요.
                Util.MessageValidation("SFU1845");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                // SFU1843 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            BOX001_242_CREATEPALLET popup = new BOX001_242_CREATEPALLET();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = txtWorker.Tag;
                Parameters[2] = txtShift.Tag;

                C1WindowExtension.SetParameters(popup, Parameters);
                popup.Closed += new EventHandler(puRun_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
            else
            {
                //Message: 팔레트 구성 정보가 없습니다.
            }
        }
        /// <summary>
        /// Pallet 생성 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
      
        private void puRun_Closed(object sender, EventArgs e)
        {
            BOX001_242_CREATEPALLET popup = sender as BOX001_242_CREATEPALLET;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", popup.PALLET_ID, true);
            }
            this.grdMain.Children.Remove(popup);
        }

        #endregion

        #region 실적확정 : btnConfirm_Click()
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text) || txtWorker.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU1235	이미 확정 되었습니다.
                    Util.MessageValidation("SFU1235");
                    return;
                }


                // SFU1716 실적확정 하시겠습니까? 
                Util.MessageConfirm("SFU1716", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        string sEquipmentId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["EQPTID"].Index).Value);

                        string sQTY = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["TOTAL_QTY"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("SRCTYPE");
                        RQSTDT.Columns.Add("IFMODE");
                        RQSTDT.Columns.Add("USERID");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("INPUTQTY");
                        RQSTDT.Columns.Add("OUTPUTQTY");
                        RQSTDT.Columns.Add("SHIFT");
                        RQSTDT.Columns.Add("WRK_USERID");
                        RQSTDT.Columns.Add("WRK_USER_NAME");
                        RQSTDT.Columns.Add("AREAID");
                        RQSTDT.Columns.Add("EQPTID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["BOXID"] = sPalletId;
                        newRow["INPUTQTY"] = Util.NVC_Decimal(sQTY);
                        newRow["OUTPUTQTY"] = Util.NVC_Decimal(sQTY);
                        newRow["SHIFT"] = txtShift.Tag.ToString();
                        newRow["WRK_USERID"] = txtWorker.Tag.ToString();
                        newRow["WRK_USER_NAME"] = txtWorker.Text.ToString();
                        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        newRow["EQPTID"] = sEquipmentId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_END_2ND_PLT", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                //   TagPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 실적확정 취소 : btnConfirmCancel_Click()
        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text) || txtWorker.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4262		실적 확정후 작업 가능합니다.	
                    Util.MessageValidation("SFU4262");
                    return;
                }


                //		SFU4263	실적 취소 하시겠습니까?	
                Util.MessageConfirm("SFU4263", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_END_2ND_PLT", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);

                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region  Packing LIst 발행 : btnPrint_Click()
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT_LIST"].Index).Value) == "SHIPPING" ||
                    Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT_LIST"].Index).Value) == "FINISH_RECEIVE")
                {
                    //SFU4416		이미 출고된 팔레트 입니다.	
                    Util.MessageValidation("SFU4416");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4413		포장 완료된 팔레트만 출고 가능합니다.	
                    Util.MessageValidation("SFU4413");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text) || txtWorker.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                TagPrint();
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 출고취소 : btnShipCancel_Click()
        private void btnShipCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Button btn = sender as Button;
                //if (!Validation(btn.Name))
                //    return;
                if (string.IsNullOrEmpty(txtWorker.Text) || string.IsNullOrEmpty(Util.NVC(txtWorker.Tag)))
                {
                    // SFU1843 작업자를 입력 해주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                if (string.IsNullOrEmpty(txtShift.Text) || string.IsNullOrEmpty(Util.NVC(txtShift.Tag)))
                {
                    // SFU1844 작업조를 입력 해주세요.
                    Util.MessageValidation("SFU1844");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT_LIST"].Index).Value) == "FINISH_RECEIVE")
                {
                    //입고 전 출고요청만 취소 가능합니다.
                    Util.MessageValidation("SFU3013");
                    return;
                }

                // SFU4120	출고취소시 물류창고에서 팔레트 태그를 회수하셔야 합니다. 출고취소 하시겠습니까?	
                Util.MessageConfirm("SFU4120", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        loadingIndicator.Visibility = Visibility.Visible;

                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("OUTBOX");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("LANGID");

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["BOXID"] = sPalletId;
                        inDataRow["LANGID"] = LoginInfo.LANGID;
                        inDataTable.Rows.Add(inDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_CANCEL_SHIP_AUTO", "OUTBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                CancelShip();

                                //GetPalletList();
                                //_util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }
        
        private void CancelShip()
        {
            //if (string.IsNullOrWhiteSpace(txtPalletID.Text))
            //{
            //    // SFU3350  입력오류 : PALLETID 를 입력해 주세요.	
            //    Util.MessageValidation("SFU3350");
            //    return;
            //}

            loadingIndicator.Visibility = Visibility.Visible;

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID");
            inDataTable.Columns.Add("SHFT_ID");
            inDataTable.Columns.Add("WRK_SUPPLIERID");
            inDataTable.Columns.Add("LANGID");
            inDataTable.Columns.Add("NOTE");

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["USERID"] = txtWorker.Tag;
            inDataRow["SHFT_ID"] = txtShift.Tag;
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["NOTE"] = string.Empty;
            inDataTable.Rows.Add(inDataRow);

            DataTable OutBoxTable = indataSet.Tables.Add("OUTBOX");
            OutBoxTable.Columns.Add("BOXID");

            DataRow newRow = OutBoxTable.NewRow();
            newRow["BOXID"] = sPalletId;
            OutBoxTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_OUTPALLET_AUTO", "INDATA,OUTBOX", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                    GetPalletList();
                    _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, indataSet);

        }
        #endregion

        #region 작업 Pallet 색깔표시 : dgPalletList_LoadedCellPresenter()

        private void dgPalletList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblCreated.Tag))
                    {
                        e.Cell.Presenter.Background = lblCreated.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacked.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacked.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipped.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipped.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        #endregion

        #region OUTBOX 라벨 재발행 : btnRePrint_Click()
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            BOX001_242_REPRINT popUp = new BOX001_242_REPRINT { FrameOperation = FrameOperation };
            if (popUp != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(popUp, Parameters);
                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
        }

        #endregion

        #region 작업조 조회 : btnShift_Click()
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER3 wndPopup = new CMM_SHIFT_USER3();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        #endregion

        #endregion

        #region [Method]

        #region Pallet 생성 조회 : GetPalletList()
        /// <summary>
        /// Pallet 생성 조회
        /// </summary>
        /// <param name="idx"></param>
        private void GetPalletList(int idx = -1)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof (string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXSTAT_LIST", typeof(string));
                //RQSTDT.Columns.Add("SHIPSTAT_LIST", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["EQSGID"] = Util.NVC(cboLine.SelectedValue); 임시 주석처리.

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                    dr["BOXID"] = txtPalletID.Text;
                else
                {
                    dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                    //dr["SHIPSTAT_LIST"] = string.IsNullOrEmpty(_searchShipStat) ? _searchShipStat : _searchShipStat.Remove(_searchShipStat.Length - 1);
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromDate);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToDate);
                    dr["EQPTID"] = String.IsNullOrEmpty(Util.NVC(cboEquipment.SelectedValue)) ? null : cboEquipment.SelectedValue;
                    dr["PKG_LOTID"] = String.IsNullOrEmpty(Util.NVC(txtLotId.Text)) ? null : Util.NVC(txtLotId.Text);
                }

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_AUTOBOXING_LIST", "INDATA", "OUTDATA", RQSTDT);

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgPalletList, RSLTDT, FrameOperation, true);
                if (idx != -1)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[idx].DataItem, "CHK", true);
                    dgPalletList.SelectedIndex = idx;
                    dgPalletList.ScrollIntoView(idx, 0);
                }
                else
                {
                    dgPalletList.SelectedIndex = -1;
                }


                if (RSLTDT.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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
        #endregion

        #region 작업조 조회 : wndShift_Closed()
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER3 wndPopup = sender as CMM_SHIFT_USER3;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
                txtWorkGroup.Text = Util.NVC(wndPopup.WORKGROUIDNAME);
                txtWorkGroup.Tag = Util.NVC(wndPopup.WORKGROUID);
            }
            this.grdMain.Children.Remove(wndPopup);
        }
        #endregion

        #region Packing LIst 발행 : TagPrint(), popupTagPrint_Closed(), SetShpping()

        private void TagPrint()
        {
            CMM_FORM_OUTBOX_TAG_PRINT popupTagPrint = new CMM_FORM_OUTBOX_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.PrintCount = "1";

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
            string sWipseq = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["WIPSEQ"].Index).Value);

            object[] parameters = new object[2];
            parameters[0] = sPalletId;
            parameters[1] = sWipseq;

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_OUTBOX_TAG_PRINT popup = sender as CMM_FORM_OUTBOX_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                SetShpping();

               // GetPalletList();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void SetShpping()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                loadingIndicator.Visibility = Visibility.Visible;
                string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                string sEquipmentId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["EQPTID"].Index).Value);

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHFT_ID");
                //inDataTable.Columns.Add("WRK_SUPPLIERID");
                //inDataTable.Columns.Add("EXE_DOM_TYPE_CODE");
                inDataTable.Columns.Add("LANGID");

                DataTable inBoxTable = inDataSet.Tables.Add("OUTBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("EQPTID");

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = txtWorker.Tag;
                newRow["SHFT_ID"] = txtShift.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = sPalletId;
                newRow["EQPTID"] = sEquipmentId;
                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_OUTPALLET_FM", "INDATA,OUTBOX", null, (bizResult, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        
                        _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }

                    GetPalletList();

                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion
    }

}
