/*************************************************************************************
 Created Date : 2023.10.01
      Creator : 정용석
  Description : MTOM (제품변경)
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.01 정용석 : Initial Created. (조립동의 제품변경 화면 참조)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_100 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_100_DataHelper dataHelper = new PACK001_100_DataHelper();
        private string modelID = string.Empty;          // 변경 제품 ID에 해당하는 MODEL ID
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_100()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // Initialize
        private void Initialize()
        {
            PackCommon.SetPopupDraggable(this.popupWorkorder, this.pnlTitleWorkorder);
            PackCommon.SetC1ComboBox(this.dataHelper.GetFromProductInfo(LoginInfo.CFG_AREA_ID), this.cboFromProductID, true, "-SELECT-");
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboFromEquipmentSegmentID, true, "-SELECT-");
            this.dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-3);
            this.dtpDateTo.SelectedDateTime = DateTime.Now;
            this.dtpWOPlanDateFrom.SelectedDateTime = DateTime.Now.AddMonths(-1);
            this.dtpWOPlanDateTo.SelectedDateTime = DateTime.Now;
            this.txtToWorkorder.IsReadOnly = true;
            this.txtToPalletWorkorder.IsReadOnly = true;
            this.InitializePopupGrid();         // Workorder Popup Grid
        }

        // Popup - CreateGrid
        private void InitializePopupGrid()
        {
            // 제목땡기기
            this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("WORKORDER");

            // Grid 설정
            this.dgWorkorderList.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
            this.dgWorkorderList.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.dgWorkorderList.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            // Column 추가
            PackCommon.AddGridColumn(this.dgWorkorderList, "RADIOBUTTON", ObjectDic.Instance.GetObjectName("선택"), "CHK", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("작업지시번호"), "WOID", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("작업지시상태"), "WO_STAT_NAME", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("제품ID"), "PRODID", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("제품설명"), "PRODDESC", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("계획시작일"), "PLANSTDTTM", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("계획종료일"), "PLANEDDTTM", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("계획수량"), "PLANQTY", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("생산수량"), "OUTQTY", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("작업지시TYPE"), "LOTYNAME", true);
            PackCommon.AddGridColumn(this.dgWorkorderList, "TEXT", ObjectDic.Instance.GetObjectName("시장유형"), "MKT_TYPE_CODE", true);
        }

        // ValidationCheck
        private bool ValidationCheck()
        {
            if (!this.ValidationCheckComboBox(this.cboFromProductID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("제품")); // %1(을)를 선택하세요.
                return false;
            }

            if (!this.ValidationCheckComboBox(this.cboToProductID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("제품")); // %1(을)를 선택하세요.
                return false;
            }

            if (!this.ValidationCheckComboBox(this.cboFromEquipmentSegmentID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                return false;
            }

            if (string.IsNullOrEmpty(this.txtToWorkorder.Text))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("Workorder")); // %1(을)를 선택하세요.
                return false;
            }

            if (string.IsNullOrEmpty(this.txtToPalletWorkorder.Text))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("Workorder")); // %1(을)를 선택하세요.
                return false;
            }

            return true;
        }

        // Validation Check (For C1ComboBox)
        private bool ValidationCheckComboBox(C1ComboBox c1ComboBox)
        {
            bool returnValue = true;

            if (c1ComboBox == null || c1ComboBox.SelectedValue == null || string.IsNullOrEmpty(c1ComboBox.SelectedValue.ToString()))
            {
                returnValue = false;
            }

            return returnValue;
        }

        // 조회 (MTOM 이력)
        private void SearchMTOMHistory()
        {
            DateTime fromDate = this.dtpDateFrom.SelectedDateTime.Date;
            DateTime toDate = this.dtpDateTo.SelectedDateTime.Date;
            string LOTID = this.txtLOTIDHist.Text;

            try
            {
                Util.gridClear(this.dgAdjustLOTHistory);
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();
                DataTable dt = this.dataHelper.GetMTOMHistory(LOTID, fromDate, toDate);

                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgAdjustLOTHistory, dt, FrameOperation);
                }
                else
                {
                    Util.MessageInfo("SFU3536");        // 조회된 결과가 없습니다
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

        // 조회
        private void SearchProcess(string LOTID)
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                return;
            }

            if (!this.ValidationCheck())
            {
                return;
            }

            Util.gridClear(this.dgAdjustLOTList);
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            try
            {
                string fromProductID = this.cboFromProductID.SelectedValue.ToString();
                string toProductID = this.cboToProductID.SelectedValue.ToString();
                string fromEquipmentSegmentID = this.cboFromEquipmentSegmentID.SelectedValue.ToString();

                DataTable dt = this.dataHelper.GetMTOMLOTInfo(LOTID, fromProductID, toProductID, fromEquipmentSegmentID);
                if (CommonVerify.HasTableRow(dt))
                {
                    // 불건전 Data 추출
                    var queryUnWholeSome = dt.AsEnumerable().Where(x => x.Field<string>("CHK_VALID") != "N");

                    // 불건전 Data와 건전 Data 분리
                    var queryWholeSome = dt.AsEnumerable().Except(queryUnWholeSome);

                    // 불건전 Data는 불건전 Popup창에 집어넣고, 건전 Data는 대상 Grid에 집어넣기.
                    if (queryUnWholeSome.Count() > 0)
                    {
                        this.ShowExceptionPopup(queryUnWholeSome.CopyToDataTable());
                    }

                    if (queryWholeSome.Count() > 0)
                    {
                        Util.GridSetData(this.dgAdjustLOTList, queryWholeSome.CopyToDataTable(), FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                this.txtLOTID.Text = string.Empty;
            }
        }

        // 불건전 Data 표출 Popup Open
        private void ShowExceptionPopup(DataTable dt)
        {
            COM001_035_PACK_EXCEPTION_POPUP popUp = new COM001_035_PACK_EXCEPTION_POPUP();
            popUp.FrameOperation = FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = "MTOM";

                C1WindowExtension.SetParameters(popUp, Parameters);
                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }

        // 저장
        private void SaveProcess()
        {
            if (this.dgAdjustLOTList == null || this.dgAdjustLOTList.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1379");       // LOT을 입력해주세요.
                return;
            }

            if (string.IsNullOrEmpty(this.txtAdjustLOTNote.Text))
            {
                Util.MessageValidation("SFU1594");       // 사유를 입력하세요.
                return;
            }

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            try
            {
                DataTable dtAdjustLOT = DataTableConverter.Convert(this.dgAdjustLOTList.ItemsSource);
                string LOTID = dtAdjustLOT.AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                string fromProductID = dtAdjustLOT.AsEnumerable().Select(x => x.Field<string>("FROM_PRODID")).FirstOrDefault();
                string toProductID = dtAdjustLOT.AsEnumerable().Select(x => x.Field<string>("TO_PRODID")).FirstOrDefault();
                string fromEquipmentSegmentID = dtAdjustLOT.AsEnumerable().Select(x => x.Field<string>("EQSGID")).FirstOrDefault();
                string toWorkorder = this.txtToWorkorder.Text;
                string toPalletWorkorder = this.txtToPalletWorkorder.Text;
                string userID = LoginInfo.USERID;
                string ReasonNote = this.txtAdjustLOTNote.Text;

                DataTable dt = this.dataHelper.SetLOTMTOM(LOTID, fromProductID, toProductID, fromEquipmentSegmentID, toWorkorder, toPalletWorkorder, userID, ReasonNote);

                // 해당 함수에서는 LOT List에 있는 항목에 있는 LOT만 해당됨.
                // Transaction 수행시 Return DataSet에 Data(불건전 Data)가 존재하는 경우에는 Transaction 발생하지 않음.
                if (CommonVerify.HasTableRow(dt))
                {
                    // 불건전 Data 추출
                    var queryUnWholeSome = dtAdjustLOT.AsEnumerable().Where(x => x.Field<string>("CHK_VALID") != "N");

                    // 불건전 Data와 건전 Data 분리
                    var queryWholeSome = dtAdjustLOT.AsEnumerable().Except(queryUnWholeSome);

                    // 불건전 Data는 불건전 Popup창에 집어넣고, Popup 띄우고 종료
                    if (queryUnWholeSome.Count() > 0)
                    {
                        this.ShowExceptionPopup(queryUnWholeSome.CopyToDataTable());
                    }
                }
                // Return된 불건전 Data가 없는 경우
                else
                {
                    Util.MessageInfo("SFU3532");        // 저장 되었습니다.
                    this.ClearControl();
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

        // Control Clear
        private void ClearControl()
        {
            this.cboFromProductID.SelectedIndex = 0;
            this.cboToProductID.SelectedIndex = 0;
            this.cboFromEquipmentSegmentID.SelectedIndex = 0;
            this.cboToProductID.SelectedIndex = 0;
            this.txtLOTID.Text = string.Empty;
            this.txtAdjustLOTNote.Text = string.Empty;
            Util.gridClear(this.dgAdjustLOTList);
        }

        // Popup - Close Popup
        private void HideWorkorderPopup()
        {
            this.popupWorkorder.Tag = null;
            this.popupWorkorder.IsOpen = false;
            this.popupWorkorder.HorizontalOffset = 0;
            this.popupWorkorder.VerticalOffset = 0;
        }

        private void SelectedItemProcess()
        {
            try
            {
                string workorderID = DataTableConverter.Convert(this.dgWorkorderList.ItemsSource).AsEnumerable().Where(x => x.Field<int>("CHK") == 1).Select(x => x.Field<string>("WOID")).FirstOrDefault();
                if (string.IsNullOrEmpty(workorderID))
                {
                    return;
                }

                if (this.popupWorkorder.Tag.ToString() == "M")
                {
                    this.txtToWorkorder.Text = workorderID;
                }
                else if (this.popupWorkorder.Tag.ToString() == "B")
                {
                    this.txtToPalletWorkorder.Text = workorderID;
                }

                this.popupWorkorder.Tag = null;
                this.HideWorkorderPopup();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowWorkorderSelectPopup(string productID)
        {
            this.GetWorkorderList(productID);

            // Popup 표출 위치 (정가운데)
            if (!this.popupWorkorder.IsOpen)
            {
                this.popupWorkorder.Placement = PlacementMode.Center;
                this.popupWorkorder.IsOpen = true;
            }
        }

        private bool ValidationCheckForShowWorkorderPopup()
        {
            if (!this.ValidationCheckComboBox(this.cboFromProductID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("제품")); // %1(을)를 선택하세요.
                return false;
            }

            if (!this.ValidationCheckComboBox(this.cboToProductID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("제품")); // %1(을)를 선택하세요.
                return false;
            }

            if (!this.ValidationCheckComboBox(this.cboFromEquipmentSegmentID))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                return false;
            }

            return true;
        }

        // 선택가능한 Workorder List 조회
        private void GetWorkorderList(string productID)
        {
            // 변경제품, 선택한 라인의 포장 Workorder 리스트 조회
            string equipmentSegmentID = this.cboFromEquipmentSegmentID.SelectedValue.ToString();

            // Workorder List 조회
            Util.gridClear(this.dgWorkorderList);
            DataTable dt = this.dataHelper.GetWorkorderInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, productID, this.popupWorkorder.Tag.ToString(), this.dtpWOPlanDateFrom.SelectedDateTime.Date, this.dtpWOPlanDateTo.SelectedDateTime.Date);
            Util.GridSetData(this.dgWorkorderList, dt, FrameOperation);
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> lstButton = new List<Button>();
            lstButton.Add(this.btnAdjustLOT);
            Util.pageAuth(lstButton, FrameOperation.AUTHORITY);

            this.Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SearchProcess(this.txtLOTID.Text);
        }

        private void txtLOTIDHist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SearchMTOMHistory();
        }

        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.V || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                {
                    Util.MessageInfo("FM_ME_0183");
                    return;
                }

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

        private void txtLOTIDHist_PreviewKeyDown(object sender, KeyEventArgs e)
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

                this.txtLOTIDHist.Text = arrClipboardText.Aggregate((current, next) => current + "," + next).ToString();
                this.SearchMTOMHistory();
                this.txtLOTIDHist.Text = string.Empty;
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

        private void cboFromProductID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            // 변경될 제품 ComboBox Data 변경
            string productID = e.NewValue.ToString();
            PackCommon.SetC1ComboBox(this.dataHelper.GetToProductInfo(LoginInfo.CFG_AREA_ID, productID), this.cboToProductID, true, "-SELECT-");
            this.txtToWorkorder.Text = string.Empty;
            this.txtToPalletWorkorder.Text = string.Empty;
        }

        private void cboToProductID_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            // 선택된 제품의 모델 ID 가져오기
            string productID = e.NewValue.ToString();
            DataTable dt = this.dataHelper.GetProductInfo(productID);
            if (CommonVerify.HasTableRow(dt))
            {
                this.modelID = dt.AsEnumerable().Select(x => x.Field<string>("MODLID")).FirstOrDefault();
            }
            else
            {
                this.modelID = string.Empty;
            }
            this.txtToWorkorder.Text = string.Empty;
            this.txtToPalletWorkorder.Text = string.Empty;
        }

        private void cboFromEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            this.txtToWorkorder.Text = string.Empty;
            this.txtToPalletWorkorder.Text = string.Empty;
        }

        private void cboToWorkOrder_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

        }

        private void cboToPalletWorkorder_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

        }

        private void btnAdjustLOT_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchMTOMHistory();
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            this.HideWorkorderPopup();
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HideWorkorderPopup();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedItemProcess();
        }

        private void popupWorkorder_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupWorkorder.StaysOpen = true;
        }

        private void btnWorkorderSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheckForShowWorkorderPopup())
            {
                return;
            }
            this.popupWorkorder.Tag = "M";      // ProcessSegmentID
            string productID = this.cboToProductID.SelectedValue.ToString();
            this.ShowWorkorderSelectPopup(productID);
        }

        private void btnPalletWorkorderSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheckForShowWorkorderPopup())
            {
                return;
            }
            this.popupWorkorder.Tag = "B";      // ProcessSegmentID
            string productID = this.modelID;
            this.ShowWorkorderSelectPopup(productID);
        }

        private void btnSearchWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            string productID = string.Empty;
            switch (this.popupWorkorder.Tag.ToString())
            {
                case "M":
                    productID = this.cboToProductID.SelectedValue.ToString();
                    break;
                case "B":
                    productID = this.modelID;
                    break;
                default:
                    break;
            }

            this.GetWorkorderList(productID);
        }
        #endregion
    }

    internal class PACK001_100_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_100_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode 정보
        internal DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

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
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 변경전 제품
        internal DataTable GetFromProductInfo(string areaID)
        {
            DataTable dtReturn = new DataTable();
            const string COMMON_TYPE_CODE = "MTOM_PRODUCT_PACK";

            DataTable dt = this.GetAreaCommonCodeInfo(areaID, COMMON_TYPE_CODE, string.Empty);
            if (CommonVerify.HasTableRow(dt))
            {
                dtReturn = PackCommon.queryToDataTable(dt.AsEnumerable().Select(x => new
                {
                    PRODID = x.Field<string>("COM_CODE"),
                    PRODNAME = x.Field<string>("COM_CODE")
                }).OrderBy(x => x.PRODID).ToList());
            }

            return dtReturn;
        }

        // 순서도 호출 - 제품 정보
        internal DataTable GetProductInfo(string productID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_VW_PRODUCT";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["PRODID"] = productID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 변경후 제품
        internal DataTable GetToProductInfo(string areaID, string fromProductID)
        {
            DataTable dtReturn = new DataTable();
            const string COMMON_TYPE_CODE = "MTOM_PRODUCT_PACK";

            if (string.IsNullOrEmpty(fromProductID))
            {
                dtReturn.Columns.Add("PRODID", typeof(string));
                dtReturn.Columns.Add("PRODNAME", typeof(string));
                return dtReturn;
            }
            else
            {
                DataTable dt = this.GetAreaCommonCodeInfo(areaID, COMMON_TYPE_CODE, fromProductID);
                if (CommonVerify.HasTableRow(dt))
                {
                    dtReturn = PackCommon.queryToDataTable(dt.AsEnumerable().Select(x => x.Field<string>("ATTR1") + ","
                                                                                       + x.Field<string>("ATTR2") + ","
                                                                                       + x.Field<string>("ATTR3") + ","
                                                                                       + x.Field<string>("ATTR4") + ","
                                                                                       + x.Field<string>("ATTR5")).FirstOrDefault().ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => new
                                                                                       {
                                                                                           PRODID = x,
                                                                                           PRODNAME = x
                                                                                       }).OrderBy(x => x.PRODID).ToList());
                }
            }

            return dtReturn;
        }

        // 순서도 호출 - 동별 공통코드 정보
        internal DataTable GetAreaCommonCodeInfo(string areaID, string comTypeCode, string comCode)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["COM_TYPE_CODE"] = comTypeCode;
                drRQSTDT["COM_CODE"] = string.IsNullOrEmpty(comCode) ? null : comCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - EquipmentSegment 정보
        internal DataTable GetEquipmentSegmentInfo(string areaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Workorder (To Workorder)
        internal DataTable GetWorkorderInfo(string shopID, string areaID, string equipmentSegmentID, string productID, string processSegmentID, DateTime workorderPlanStartDate, DateTime workorderPlandEndDate)
        {
            string bizRuleName = "DA_PRD_SEL_WORKORDER_PLAN_LIST_PACK";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            DataTable dtReturn = new DataTable();

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("STRT_DTTM_FROM", typeof(string));
                dtRQSTDT.Columns.Add("STRT_DTTM_TO", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PCSGID", typeof(string));
                dtRQSTDT.Columns.Add("PRJ_NAME", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["STRT_DTTM_FROM"] = workorderPlanStartDate.ToString("yyyy-MM-dd");
                drRQSTDT["STRT_DTTM_TO"] = workorderPlandEndDate.ToString("yyyy-MM-dd");
                drRQSTDT["SHOPID"] = shopID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(equipmentSegmentID) || equipmentSegmentID.Equals("ALL")) ? null : equipmentSegmentID;
                drRQSTDT["PCSGID"] = processSegmentID;
                drRQSTDT["PRJ_NAME"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("PRODID") == productID);
                    dtReturn = (query.Count() <= 0) ? dtRSLTDT.Clone() : dtRSLTDT.AsEnumerable().Where(x => x.Field<string>("PRODID") == productID).CopyToDataTable();
                }
                else
                {
                    dtReturn = dtRSLTDT.Clone();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
            return dtReturn;
        }

        // 순서도 호출 - 입력받은 LOTID 조회
        internal DataTable GetMTOMLOTInfo(string LOTID, params string[] arrParameter)
        {
            string bizRuleName = "DA_PRD_SEL_MTOM_LOT_CHECK";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            DataTable dtReturn = new DataTable();
            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_PRODID", typeof(string));
                dtRQSTDT.Columns.Add("TO_PRODID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = LOTID; 
                drRQSTDT["FROM_PRODID"] = string.IsNullOrEmpty(arrParameter[00]) ? null : arrParameter[00];
                drRQSTDT["TO_PRODID"]   = string.IsNullOrEmpty(arrParameter[01]) ? null : arrParameter[01];
                drRQSTDT["FROM_EQSGID"] = string.IsNullOrEmpty(arrParameter[02]) ? null : arrParameter[02];
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - MTOM Transaction
        internal DataTable SetLOTMTOM(string LOTID, params string[] arrParameter)
        {
            string bizRuleName = "BR_PRD_REG_MTOM_LOT_FOR_PACK";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";
            DataTable dtReturn = new DataTable();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));
                dtINDATA.Columns.Add("FROM_PRODID", typeof(string));
                dtINDATA.Columns.Add("TO_PRODID", typeof(string));
                dtINDATA.Columns.Add("FROM_EQSGID", typeof(string));
                dtINDATA.Columns.Add("TO_WORKORDER", typeof(string));
                dtINDATA.Columns.Add("TO_PALLETWORKORDER", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("RESN_NOTE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["LOTID"] = LOTID;
                drINDATA["FROM_PRODID"] = string.IsNullOrEmpty(arrParameter[00]) ? null : arrParameter[00];
                drINDATA["TO_PRODID"] = string.IsNullOrEmpty(arrParameter[01]) ? null : arrParameter[01];
                drINDATA["FROM_EQSGID"] = string.IsNullOrEmpty(arrParameter[02]) ? null : arrParameter[02];
                drINDATA["TO_WORKORDER"] = string.IsNullOrEmpty(arrParameter[03]) ? null : arrParameter[03];
                drINDATA["TO_PALLETWORKORDER"] = string.IsNullOrEmpty(arrParameter[04]) ? null : arrParameter[04];
                drINDATA["USERID"] = string.IsNullOrEmpty(arrParameter[05]) ? null : arrParameter[05];
                drINDATA["RESN_NOTE"] = string.IsNullOrEmpty(arrParameter[06]) ? null : arrParameter[06];
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
                if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    dtReturn = dsOUTDATA.Tables["OUTDATA"].Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - 입력받은 LOTID 조회
        internal DataTable GetMTOMHistory(string LOTID, DateTime fromDate, DateTime toDate)
        {
            string bizRuleName = "DA_PRD_SEL_MTOM_HISTORY_PACK";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            DataTable dtReturn = new DataTable();
            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRQSTDT.Columns.Add("TO_DATE", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                drRQSTDT["FROM_DATE"] = fromDate;
                drRQSTDT["TO_DATE"] = toDate;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }
        #endregion
    }
}