/*************************************************************************************
 Created Date : 2019.12.16
      Creator : 염규범
   Decription : LOT Hold (Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.16 염규범 : Initial Created.
  2022.09.19 정용석 : 특이보류재고 관리 Tab 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_060 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK001_060_DataHelper dataHelper = new PACK001_060_DataHelper();
        private const string HOLD_LOT = "HOLD_LOT";
        private bool isForceUnchecked = false;
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_060()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void Initialize()
        {
            this.SetControlTag();

            // Hold Tab
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboHoldEquipmentSegment, true, "ALL");
            PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("PRDT_CLSS_CODE_PACK"), this.cboHoldProjectClassCode, true, "ALL");
            this.cboHoldEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            this.dtpHoldDateFrom.ApplyTemplate();
            this.dtpHoldDateTo.ApplyTemplate();
            this.dtpHoldDateFrom.SelectedDateTime = DateTime.Now;
            this.dtpHoldDateTo.SelectedDateTime = DateTime.Now;
            Util.GridSetData(this.dgHoldGroup1, this.dataHelper.GetAreaDefectCode(HOLD_LOT), FrameOperation, true);       // Hold Code Level 1

            // LOT Hold / Relase 이력 Tab
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboHoldHistoryEquipmentSegment, true, "ALL");
            PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("PRDT_CLSS_CODE_PACK"), this.cboHoldHistoryProjectClassCode, true, "ALL");
            this.cboHoldHistoryEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            this.dtpHoldHistoryDateFrom.ApplyTemplate();
            this.dtpHoldHistoryDateTo.ApplyTemplate();
            this.dtpHoldHistoryDateFrom.SelectedDateTime = DateTime.Now;
            this.dtpHoldHistoryDateTo.SelectedDateTime = DateTime.Now;

            // QMS Hold / Relase 이력 Tab
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboQMSHoldHistoryEquipmentSegment, true, "ALL");
            PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("PRDT_CLSS_CODE_PACK"), this.cboQMSHoldHistoryProjectClassCode, true, "ALL");
            this.cboQMSHoldHistoryEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            this.dtpQMSHoldHistoryDateFrom.ApplyTemplate();
            this.dtpQMSHoldHistoryDateTo.ApplyTemplate();
            this.dtpQMSHoldHistoryDateFrom.SelectedDateTime = DateTime.Now;
            this.dtpQMSHoldHistoryDateTo.SelectedDateTime = DateTime.Now;

            // 특이보류재고 Tab
            PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboUnusualHoldStockEquipmentSegment, true, "ALL");
            PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("PRDT_CLSS_CODE_PACK"), this.cboUnusualHoldStockProjectClassCode, true, "ALL");
            this.cboUnusualHoldStockEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            this.dtpUnusualHoldStockDateFrom.ApplyTemplate();
            this.dtpUnusualHoldStockDateTo.ApplyTemplate();
            this.dtpUnusualHoldStockDateFrom.SelectedDateTime = DateTime.Now;
            this.dtpUnusualHoldStockDateTo.SelectedDateTime = DateTime.Now;
        }

        // Button Tag 설정
        private void SetControlTag()
        {
            this.btnHoldSearch.Tag = "HOLDLIST";
            this.btnHoldHistorySearch.Tag = "HOLDHISTORY";
            this.btnQMSHoldHistorySearch.Tag = "QMSHOLDHISTORY";
            this.btnUnusualHoldStockSearch.Tag = "UNUSUALHOLDSTOCK";

            this.btnHoldExcel.Tag = "HOLDLIST";
            this.btnHoldHistoryExcel.Tag = "HOLDHISTORY";
            this.btnQMSHoldHistoryExcel.Tag = "QMSHOLDHISTORY";
            this.btnUnusualHoldStockExcel.Tag = "UNUSUALHOLDSTOCK";

            this.txtHoldLOTID.Tag = "HOLDLIST";
            this.txtHoldHistoryLOTID.Tag = "HOLDHISTORY";
            this.txtQMSHoldHistoryLOTID.Tag = "QMSHOLDHISTORY";
            this.txtUnusualHoldStockLOTID.Tag = "UNUSUALHOLDSTOCK";
        }

        // Hold LOT 입력
        private void AddHoldLOTGrid(List<string> lstLOTID = null)
        {
            try
            {
                if (lstLOTID == null || lstLOTID.Count <= 0)
                {
                    lstLOTID.Add(this.txtHoldLOTID.Text);
                }

                // Validation Check...
                if (lstLOTID.Count() > 500)
                {
                    Util.MessageValidation("SFU8102");   // 최대 500개 까지 가능합니다.
                    return;
                }


                // Validation Check...
                this.txtHoldLOTID.Text = string.Empty;
                DataTable dt = this.dataHelper.GetLOTIDValidation(string.Join(",", lstLOTID), "N");
                if (!CommonVerify.HasTableRow(dt))
                {
                    var query = lstLOTID.Select(x => new
                    {
                        INPUT_LOTID = x,
                        LOTID = string.Empty,
                        WIPSTAT = string.Empty,
                        NOTE = MessageDic.Instance.GetMessage("SFU1905")  // SFU1905 : 조회된 Data가 없습니다.
                    });

                    this.ShowExceptionPopup(PackCommon.queryToDataTable(query.ToList()));
                    return;
                }

                this.loadingIndicator.Visibility = Visibility.Visible;

                // 불건전 Data 1호 : SFU1905 : 조회된 Data가 없습니다.
                var queryUnwholesome1 = lstLOTID.Where(x => !dt.AsEnumerable().Where(y => x == y.Field<string>("INPUT_LOTID")).Any()).Select(x => new
                {
                    INPUT_LOTID = x,
                    LOTID = string.Empty,
                    WIPSTAT = string.Empty,
                    NOTE = MessageDic.Instance.GetMessage("SFU1905")
                });

                // 불건전 Data 2호 : SFU3335 : 입력오류 : LOGIN 사용자의 동정보와 LOT의 동정보가 다릅니다.
                var queryUnwholesome2 = dt.AsEnumerable().Where(x => x.Field<string>("AREAID") != LoginInfo.CFG_AREA_ID).Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("INPUT_LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    WIPSTAT = string.Empty,
                    NOTE = MessageDic.Instance.GetMessage("SFU3335")
                });

                // 불건전 Data 3호 : SFU8367 : HOLD 할수 없는 WIP 상태입니다.
                var queryUnwholesome3 = dt.AsEnumerable().Where(x => x.Field<string>("WIPSTAT") == "TERM" || x.Field<string>("WIPSTAT") == "MOVING").Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("INPUT_LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    WIPSTAT = x.Field<string>("WIPSTAT"),
                    NOTE = MessageDic.Instance.GetMessage("SFU8367")
                });

                // 불건전 Data 4호 : SFU2835 : Grid에 중복된 ID가 있습니다.
                var queryUnwholesome4 = dt.AsEnumerable().Where(x => Util.MakeDataTable(this.dgHoldLOT, true).AsEnumerable().Where(y => y.Field<string>("LOTID") == x.Field<string>("LOTID")).Any()).Select(x => new
                {
                    INPUT_LOTID = x.Field<string>("INPUT_LOTID"),
                    LOTID = x.Field<string>("LOTID"),
                    WIPSTAT = string.Empty,
                    NOTE = MessageDic.Instance.GetMessage("SFU2835")
                });

                // 건전 Data와 불건전 Data 분리
                DataTable dtUnwholeSome = PackCommon.queryToDataTable(queryUnwholesome1.Union(queryUnwholesome2).Union(queryUnwholesome3).Union(queryUnwholesome4).ToList());
                DataTable dtWholeSome = new DataTable();
                if (CommonVerify.HasTableRow(dtUnwholeSome))
                {
                    var queryCheckWholeSome = dt.AsEnumerable().Where(x => !dtUnwholeSome.AsEnumerable().Where(y => y.Field<string>("LOTID") == x.Field<string>("LOTID")).Any());
                    if (queryCheckWholeSome.Count() > 0)
                    {
                        dtWholeSome = queryCheckWholeSome.CopyToDataTable();
                    }
                }
                else
                {
                    dtWholeSome = dt.Copy();
                }

                // 건전 Data는 Hold Grid에 집어넣고, 불건전 Data는 예외 Popup에 표시
                if (CommonVerify.HasTableRow(dtWholeSome))
                {
                    this.AddHoldLOTGrid(dtWholeSome);
                }

                if (CommonVerify.HasTableRow(dtUnwholeSome))
                {
                    this.ShowExceptionPopup(dtUnwholeSome);
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

        // Hold LOT Grid에 Hold 대상 Data Add
        private void AddHoldLOTGrid(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            int totalRow = this.dgHoldLOT.GetRowCount();
            if (this.dgHoldLOT.GetRowCount() <= 0)
            {
                PackCommon.SearchRowCount(ref this.txtHoldLOTRowCount, dt.Rows.Count);
                Util.GridSetData(this.dgHoldLOT, dt, FrameOperation);
                return;
            }

            foreach (DataRowView drv in dt.AsDataView())
            {
                Util.DataGridRowAdd(this.dgHoldLOT, 1);
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "CHK", drv["CHK"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "LOTID", drv["LOTID"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "PRODID", drv["PRODID"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "WOID", drv["WOID"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "PRODNAME", drv["PRODNAME"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "EQSGID", drv["EQSGID"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "EQSGNAME", drv["EQSGNAME"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "PROCNAME", drv["PROCNAME"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "WIPSNAME", drv["WIPSNAME"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "BOXID", drv["BOXID"].ToString());
                DataTableConverter.SetValue(this.dgHoldLOT.Rows[totalRow].DataItem, "WIPSTAT", drv["WIPSTAT"].ToString());
                totalRow++;
            }

            PackCommon.SearchRowCount(ref this.txtHoldLOTRowCount, this.dgHoldLOT.GetRowCount());
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
                Parameters[1] = HOLD_LOT;

                C1WindowExtension.SetParameters(popUp, Parameters);
                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }

        // 조회질 - Main (버튼 눌렀을 때)
        private void SearchProcess(Button button)
        {
            switch (button.Tag.ToString())
            {
                case "HOLDLIST":
                    this.SearchPackHoldList();
                    break;
                case "HOLDHISTORY":
                    this.SearchHoldHistory();
                    break;
                case "QMSHOLDHISTORY":
                    this.SearchPackQMSHoldHistory();
                    break;
                case "UNUSUALHOLDSTOCK":
                    this.SearchUnusualHoldStockHistory();
                    break;
                default:
                    break;
            }
        }

        // 조회질 - Main (LOTID 입력하고 Enter키 쳤을 때)
        private void SearchProcess(TextBox textBox)
        {
            switch (textBox.Tag.ToString())
            {
                case "HOLDHISTORY":
                    this.SearchHoldHistory();
                    break;
                case "QMSHOLDHISTORY":
                    this.SearchPackQMSHoldHistory();
                    break;
                case "UNUSUALHOLDSTOCK":
                    this.SearchUnusualHoldStockHistory();
                    break;
                default:
                    break;
            }
        }

        // 조회질 - LOT Hold Tab
        private void SearchPackHoldList()
        {
            try
            {
                string productID = (string.IsNullOrEmpty(this.cboHoldProduct.SelectedValue.ToString()) || this.cboHoldProduct.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldProduct.SelectedValue.ToString();
                DateTime dteFromDate = this.dtpHoldDateFrom.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpHoldDateTo.SelectedDateTime.Date.AddDays(1);
                string equipmentSegmentID = (string.IsNullOrEmpty(this.cboHoldEquipmentSegment.SelectedValue.ToString()) || this.cboHoldEquipmentSegment.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldEquipmentSegment.SelectedValue.ToString();
                string productClassCode = (string.IsNullOrEmpty(this.cboHoldProjectClassCode.SelectedValue.ToString()) || this.cboHoldProjectClassCode.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldProjectClassCode.SelectedValue.ToString();

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.tbHoldListRowCount, 0);
                Util.gridClear(this.dgHoldList);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.GetPackHoldList(productID, dteFromDate, dteToDate, equipmentSegmentID, productClassCode);
                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgHoldList, dt, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(tbHoldListRowCount, Util.NVC(dt.Rows.Count));
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

        // 조회질 - LOT Hold Relase 이력 Tab
        private void SearchHoldHistory(List<string> lstLOTID = null)
        {
            try
            {

                if (lstLOTID != null)
                {
                    if (lstLOTID.Count() > 500)
                    {
                        Util.MessageValidation("SFU8102");   // 최대 500개 까지 가능합니다.
                        return;
                    }

                    txtHoldHistoryLOTID.Text = string.Join(",", lstLOTID);
                }
                            
                string productClassCode = (string.IsNullOrEmpty(this.cboHoldHistoryProjectClassCode.SelectedValue.ToString()) || this.cboHoldHistoryProjectClassCode.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldHistoryProjectClassCode.SelectedValue.ToString();
                string productModel = (string.IsNullOrEmpty(this.cboHoldHistoryProductModel.SelectedValue.ToString()) || this.cboHoldHistoryProductModel.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldHistoryProductModel.SelectedValue.ToString();
                string productID = (string.IsNullOrEmpty(this.cboHoldHistoryProduct.SelectedValue.ToString()) || this.cboHoldHistoryProduct.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldHistoryProduct.SelectedValue.ToString();
                string LOTID = txtHoldHistoryLOTID.Text;
                string equipmentSegmentID = (string.IsNullOrEmpty(this.cboHoldHistoryEquipmentSegment.SelectedValue.ToString()) || this.cboHoldHistoryEquipmentSegment.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboHoldHistoryEquipmentSegment.SelectedValue.ToString();
                DateTime dteFromDate = this.dtpHoldHistoryDateFrom.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpHoldHistoryDateTo.SelectedDateTime.Date;
                bool? termCheckFlag = this.chkHoldHistoryTerminate.IsChecked;

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtHoldHistoryRowCount, 0);
                Util.gridClear(this.dgHoldHistory);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.GetPackHoldReleaseHistory(productClassCode, productModel, productID, LOTID, equipmentSegmentID, dteFromDate, dteToDate, termCheckFlag);
                if (CommonVerify.HasTableRow(dt))
                {
                    txtHoldHistoryLOTID.Clear();
                    Util.GridSetData(this.dgHoldHistory, dt, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(txtHoldHistoryRowCount, Util.NVC(dt.Rows.Count));
                }

                this.loadingIndicator.Visibility = Visibility.Collapsed;
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

        // 조회질 - QMS Hold Relase 이력 Tab
        private void SearchPackQMSHoldHistory(List<string> lstLOTID = null)
        {
            try
            {

                if (lstLOTID != null)
                {
                    if (lstLOTID.Count() > 500)
                    {
                        Util.MessageValidation("SFU8102");   // 최대 500개 까지 가능합니다.
                        return;
                    }

                    txtQMSHoldHistoryLOTID.Text = string.Join(",", lstLOTID);
                }

                string productClassCode = (string.IsNullOrEmpty(this.cboQMSHoldHistoryProjectClassCode.SelectedValue.ToString()) || this.cboQMSHoldHistoryProjectClassCode.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboQMSHoldHistoryProjectClassCode.SelectedValue.ToString();
                string productModel = (string.IsNullOrEmpty(this.cboQMSHoldHistoryProductModel.SelectedValue.ToString()) || this.cboQMSHoldHistoryProductModel.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboQMSHoldHistoryProductModel.SelectedValue.ToString();
                string productID = (string.IsNullOrEmpty(this.cboQMSHoldHistoryProduct.SelectedValue.ToString()) || this.cboQMSHoldHistoryProduct.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboQMSHoldHistoryProduct.SelectedValue.ToString();
                string LOTID = this.txtQMSHoldHistoryLOTID.Text;
                string equipmentSegmentID = (string.IsNullOrEmpty(this.cboQMSHoldHistoryEquipmentSegment.SelectedValue.ToString()) || this.cboQMSHoldHistoryEquipmentSegment.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboQMSHoldHistoryEquipmentSegment.SelectedValue.ToString();
                DateTime dteFromDate = this.dtpQMSHoldHistoryDateFrom.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpQMSHoldHistoryDateTo.SelectedDateTime.Date.AddDays(1);
                bool? termCheckFlag = this.chkQMSHoldHistoryTerminate.IsChecked;

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtQMSHoldHistoryRowCount, 0);
                Util.gridClear(this.dgQMSHoldHistory);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.GetPackQMSHoldReleaseHistory(productClassCode, productModel, productID, LOTID, equipmentSegmentID, dteFromDate, dteToDate, termCheckFlag);
                if (CommonVerify.HasTableRow(dt))
                {
                    txtQMSHoldHistoryLOTID.Clear();                        
                    Util.GridSetData(this.dgQMSHoldHistory, dt, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(txtQMSHoldHistoryRowCount, Util.NVC(dt.Rows.Count));
                }

                this.loadingIndicator.Visibility = Visibility.Collapsed;
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

        // 조회질 - 특이보류재고 Tab
        private void SearchUnusualHoldStockHistory()
        {
            try
            {
                string productClassCode = (string.IsNullOrEmpty(this.cboUnusualHoldStockProjectClassCode.SelectedValue.ToString()) || this.cboUnusualHoldStockProjectClassCode.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboUnusualHoldStockProjectClassCode.SelectedValue.ToString();
                string productModel = (string.IsNullOrEmpty(this.cboUnusualHoldStockProductModel.SelectedValue.ToString()) || this.cboUnusualHoldStockProductModel.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboUnusualHoldStockProductModel.SelectedValue.ToString();
                string productID = (string.IsNullOrEmpty(this.cboUnusualHoldStockProduct.SelectedValue.ToString()) || this.cboUnusualHoldStockProduct.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboUnusualHoldStockProduct.SelectedValue.ToString();
                string LOTID = this.txtUnusualHoldStockLOTID.Text;
                string equipmentSegmentID = (string.IsNullOrEmpty(this.cboUnusualHoldStockEquipmentSegment.SelectedValue.ToString()) || this.cboUnusualHoldStockEquipmentSegment.SelectedValue.ToString().Equals("ALL")) ? string.Empty : this.cboUnusualHoldStockEquipmentSegment.SelectedValue.ToString();
                DateTime dteFromDate = this.dtpUnusualHoldStockDateFrom.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpUnusualHoldStockDateTo.SelectedDateTime.Date.AddDays(1);
                bool? termCheckFlag = this.chkUnusualHoldStockTerminate.IsChecked;

                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtUnsualHoldStockRowCount, 0);
                Util.gridClear(this.dgUnusualHoldStock);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.GetPackHoldReleaseHistory(productClassCode, productModel, productID, LOTID, equipmentSegmentID, dteFromDate, dteToDate, termCheckFlag);
                if (CommonVerify.HasTableRow(dt))
                {
                    Util.GridSetData(this.dgUnusualHoldStock, dt, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(txtUnsualHoldStockRowCount, Util.NVC(dt.Rows.Count));
                }

                this.loadingIndicator.Visibility = Visibility.Collapsed;
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

        // 엑셀질
        private void ExcelExport(Button button)
        {
            switch (button.Tag.ToString())
            {
                case "HOLDLIST":
                    new ExcelExporter().Export(this.dgHoldList);
                    break;
                case "HOLDHISTORY":
                    new ExcelExporter().Export(this.dgHoldHistory);
                    break;
                case "QMSHOLDHISTORY":
                    new ExcelExporter().Export(this.dgQMSHoldHistory);
                    break;
                case "UNUSUALHOLDSTOCK":
                    new ExcelExporter().Export(this.dgUnusualHoldStock);
                    break;
                default:
                    break;
            }
        }

        // 담당자 찾기
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] parameters = new object[1];
                parameters[0] = this.txtHoldPerson.Text;
                C1WindowExtension.SetParameters(wndPerson, parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        // 저장질 - Hold Transaction
        private void HoldProcess()
        {
            try
            {
                // Validation Check...
                if (this.dgHoldLOT.ItemsSource == null || this.dgHoldLOT.GetRowCount() <= 0)
                {
                    Util.MessageInfo("SFU3537");
                    return;
                }

                // 해체 예정 담당자 필수
                if (this.txtHoldPerson.Tag == null || string.IsNullOrEmpty(this.txtHoldPerson.Tag.ToString()) || this.txtHoldPerson.Tag.Equals("\r\n"))
                {
                    Util.MessageInfo("SFU4350"); // 해체 예정 담당자를 선택하세요.
                    return;
                }

                // 홀드 사유 필수
                DataTable dtHoldGroup2 = DataTableConverter.Convert(this.dgHoldGroup2.ItemsSource);
                if (dtHoldGroup2.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true)).Count() <= 0)
                {
                    Util.MessageInfo("SFU1342"); // HOLD 사유를 선택 하세요.
                    return;
                }

                // HOLD 비고를 입력하세요
                TextRange textRange = new TextRange(rtbHoldCompare.Document.ContentStart, rtbHoldCompare.Document.ContentEnd);
                if (textRange.Text.Equals("\r\n") || textRange.Text.Equals(""))
                {
                    Util.MessageInfo("SFU1341");
                    return;
                }

                string actionUserID = this.txtHoldPerson.Tag.ToString();
                DateTime calDate = this.dtpCalDate.SelectedDateTime.Date;
                DataTable dt = DataTableConverter.Convert(this.dgHoldLOT.ItemsSource);
                string holdNote = textRange.Text;
                string resnCode = dtHoldGroup2.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true)).Select(x => x.Field<string>("RESNCODE")).FirstOrDefault();
                string holdCode = resnCode;                     // HOLDCODE = REASONCODE
                DateTime dteUnHoldScheduleDate = this.dtpDate.SelectedDateTime.Date;
                bool? unusualHoldStockFlag = this.chkUnusualHoldStock.IsChecked;

                if (this.dataHelper.HoldTransaction(actionUserID, calDate, dt, holdNote, resnCode, holdCode, dteUnHoldScheduleDate, unusualHoldStockFlag))
                {
                    this.txtHoldLOTID.Text = string.Empty;
                    this.rtbHoldCompare.Document.Blocks.Clear();
                    Util.gridClear(this.dgHoldLOT);
                    PackCommon.SearchRowCount(ref this.txtHoldLOTRowCount, 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 저장질 - 특이보류재고
        private void SetUnusualHoldStock()
        {
            try
            {
                // Validation Check...
                if (this.dgUnusualHoldStock.ItemsSource == null || this.dgUnusualHoldStock.GetRowCount() <= 0)
                {
                    Util.MessageInfo("SFU3537");
                    return;
                }

                DataTable dt = DataTableConverter.Convert(this.dgUnusualHoldStock.ItemsSource);
                var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => new
                {
                    LOTID = x.Field<string>("LOTID"),
                    WIPSEQ = x.Field<decimal>("WIPSEQ"),
                    HOLD_DTTM = x.Field<DateTime>("HOLD_DTTM"),
                    SPCL_HOLD_STCK_FLAG = x.Field<bool>("SPCL_HOLD_STCK_FLAG") ? "Y" : "N",
                    UPDUSER = LoginInfo.USERID,
                    UPDDTTM = DateTime.Now
                });

                if (query.Count() <= 0)
                {
                    Util.MessageInfo("SFU3537");
                    return;
                }

                if (this.dataHelper.SetUnusualHoldStock(query))
                {
                    Util.MessageInfo("SFU1270");  // 저장되었습니다.
                    this.SearchUnusualHoldStockHistory();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void cboHoldEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = e.NewValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboHoldProductModel.SelectedValue)) ? string.Empty : this.cboHoldProductModel.SelectedValue.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboHoldProjectClassCode.SelectedValue)) ? string.Empty : this.cboHoldProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProjectModelInfo(LoginInfo.CFG_AREA_ID, equipmentSegmentID), this.cboHoldProductModel, true, "ALL");    // Project Model
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboHoldProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHoldProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = this.cboHoldEquipmentSegment.SelectedValue.ToString();
                string projectModel = e.NewValue?.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboHoldProjectClassCode.SelectedValue)) ? string.Empty : this.cboHoldProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboHoldProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHoldProjectClassCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = this.cboHoldEquipmentSegment.SelectedValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboHoldProductModel.SelectedValue)) ? string.Empty : this.cboHoldProductModel.SelectedValue.ToString();
                string projectClassCode = e.NewValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboHoldProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHoldHistoryEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = e.NewValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboHoldHistoryProductModel.SelectedValue)) ? string.Empty : this.cboHoldHistoryProductModel.SelectedValue.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboHoldHistoryProjectClassCode.SelectedValue)) ? string.Empty : this.cboHoldHistoryProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProjectModelInfo(LoginInfo.CFG_AREA_ID, equipmentSegmentID), this.cboHoldHistoryProductModel, true, "ALL");    // Project Model
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboHoldHistoryProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHoldHistoryProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboHoldHistoryEquipmentSegment.SelectedValue)) ? string.Empty : this.cboHoldHistoryEquipmentSegment.SelectedValue.ToString();
                string projectModel = e.NewValue?.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboHoldHistoryProjectClassCode.SelectedValue)) ? string.Empty : this.cboHoldHistoryProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboHoldHistoryProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboHoldHistoryProjectClassCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboHoldHistoryEquipmentSegment.SelectedValue)) ? string.Empty : this.cboHoldHistoryEquipmentSegment.SelectedValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboHoldHistoryProductModel.SelectedValue)) ? string.Empty : this.cboHoldHistoryProductModel.SelectedValue.ToString();
                string projectClassCode = e.NewValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboHoldHistoryProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboQMSHoldHistoryEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = e.NewValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboQMSHoldHistoryProductModel.SelectedValue)) ? string.Empty : this.cboQMSHoldHistoryProductModel.SelectedValue.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboQMSHoldHistoryProjectClassCode.SelectedValue)) ? string.Empty : this.cboQMSHoldHistoryProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProjectModelInfo(LoginInfo.CFG_AREA_ID, equipmentSegmentID), this.cboQMSHoldHistoryProductModel, true, "ALL");    // Project Model
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboQMSHoldHistoryProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboQMSHoldHistoryProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboQMSHoldHistoryEquipmentSegment.SelectedValue)) ? string.Empty : this.cboQMSHoldHistoryEquipmentSegment.SelectedValue.ToString();
                string projectModel = e.NewValue?.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboQMSHoldHistoryProjectClassCode.SelectedValue)) ? string.Empty : this.cboQMSHoldHistoryProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboQMSHoldHistoryProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboQMSHoldHistoryProjectClassModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboQMSHoldHistoryEquipmentSegment.SelectedValue)) ? string.Empty : this.cboQMSHoldHistoryEquipmentSegment.SelectedValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboQMSHoldHistoryProductModel.SelectedValue)) ? string.Empty : this.cboQMSHoldHistoryProductModel.SelectedValue.ToString();
                string projectClassCode = e.NewValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboQMSHoldHistoryProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboUnusualHoldStockEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = e.NewValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboUnusualHoldStockProductModel.SelectedValue)) ? string.Empty : this.cboUnusualHoldStockProductModel.SelectedValue.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboUnusualHoldStockProjectClassCode.SelectedValue)) ? string.Empty : this.cboUnusualHoldStockProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProjectModelInfo(LoginInfo.CFG_AREA_ID, equipmentSegmentID), this.cboUnusualHoldStockProductModel, true, "ALL");    // Project Model
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboUnusualHoldStockProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboUnusualHoldStockProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboUnusualHoldStockEquipmentSegment.SelectedValue)) ? string.Empty : this.cboUnusualHoldStockEquipmentSegment.SelectedValue.ToString();
                string projectModel = e.NewValue?.ToString();
                string projectClassCode = string.IsNullOrEmpty(Util.NVC(this.cboUnusualHoldStockProjectClassCode.SelectedValue)) ? string.Empty : this.cboUnusualHoldStockProjectClassCode.SelectedValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboUnusualHoldStockProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboUnusualHoldStockProjectClassCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string equipmentSegmentID = string.IsNullOrEmpty(Util.NVC(this.cboUnusualHoldStockEquipmentSegment.SelectedValue)) ? string.Empty : this.cboUnusualHoldStockEquipmentSegment.SelectedValue.ToString();
                string projectModel = string.IsNullOrEmpty(Util.NVC(this.cboUnusualHoldStockProductModel.SelectedValue)) ? string.Empty : this.cboUnusualHoldStockProductModel.SelectedValue.ToString();
                string projectClassCode = e.NewValue.ToString();
                PackCommon.SetC1ComboBox(this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, equipmentSegmentID, projectModel, projectClassCode), this.cboUnusualHoldStockProduct, true, "ALL");    // Product
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Hold_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) || e.Key != Key.V)
            {                
                return;
            }

            TextBox textBox = (TextBox)sender;
            this.Search_PreviewKeyDown(textBox);

            e.Handled = true;

        }

        private void Search_PreviewKeyDown(TextBox textBox)
        {
            try
            {
                string[] stringSeparators = new string[] { "\r\n" };
                var lstClipboardData = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None).ToList<string>();
                var lstLOTID = lstClipboardData.Where(x => !string.IsNullOrEmpty(x));       // 빈값으로 들어온것 제거

                switch (textBox.Tag.ToString())
                {
                    case "HOLDLIST":
                        this.AddHoldLOTGrid(lstLOTID.ToList<string>());
                        break;
                    case "HOLDHISTORY":
                        this.SearchHoldHistory(lstLOTID.ToList<string>());
                        break;
                    case "QMSHOLDHISTORY":
                        this.SearchPackQMSHoldHistory(lstLOTID.ToList<string>());
                        break;
                }

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }
    

        private void txtHoldLOTID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.txtHoldLOTID.Text))
            {
                Util.MessageInfo("SFU1190");  // 조회할 LOT ID 를 입력하세요.
                return;
            }

            try
            {
                string[] stringSeparators = new string[] { "\r\n" };
                var lstClipboardData = this.txtHoldLOTID.Text.Split(stringSeparators, StringSplitOptions.None).ToList<string>();
                var lstLOTID = lstClipboardData.Where(x => !string.IsNullOrEmpty(x));       // 빈값으로 들어온것 제거
                this.AddHoldLOTGrid(lstLOTID.ToList<string>());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHoldSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgHoldLOT == null || this.dgHoldLOT.ItemsSource == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(this.dgHoldLOT.ItemsSource);
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            if (dt.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true)).Count() <= 0)
            {
                Util.MessageInfo("SFU1137");
                return;
            }

            DataTable dtSelectCancel = dt.Clone();
            if (dt.AsEnumerable().Where(x => !x.Field<bool>("CHK").Equals(true)).Count() > 0)
            {
                dtSelectCancel = dt.AsEnumerable().Where(x => !x.Field<bool>("CHK").Equals(true)).CopyToDataTable();
            }

            if (CommonVerify.HasTableRow(dtSelectCancel))
            {
                this.dgHoldLOT.ItemsSource = dtSelectCancel.AsDataView();
                PackCommon.SearchRowCount(ref this.txtHoldLOTRowCount, dtSelectCancel.Rows.Count);
            }
            else
            {
                this.dgHoldLOT.ItemsSource = null;
                PackCommon.SearchRowCount(ref this.txtHoldLOTRowCount, 0);
            }
        }

        private void btnHoldAllCancel_Click(object sender, RoutedEventArgs e)
        {
            // SFU3440 초기화 하시겠습니까?
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(this.dgHoldLOT);
                    PackCommon.SearchRowCount(ref this.txtHoldLOTRowCount, 0);
                }
            });
        }

        private void btnHoldExcelLoad_Click(object sender, RoutedEventArgs e)
        {
            HoldExcelImportEditor holdExcelImportEditor = new HoldExcelImportEditor();
            holdExcelImportEditor.FrameOperation = FrameOperation;
            holdExcelImportEditor.Closed += new EventHandler(holdExcelImportEditor_Closed);

            if (holdExcelImportEditor != null)
            {
                DataTable dt = new DataTable();
                if (this.dgHoldLOT.ItemsSource != null && this.dgHoldLOT.GetRowCount() > 0)
                {
                    dt = DataTableConverter.Convert(this.dgHoldLOT.ItemsSource);
                }
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = HOLD_LOT;


                C1WindowExtension.SetParameters(holdExcelImportEditor, Parameters);
                grdMain.Children.Add(holdExcelImportEditor);
                holdExcelImportEditor.BringToFront();
            }
        }

        private void txtHoldPerson_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.GetUserWindow();
        }

        private void btnHoldPerson_Click(object sender, RoutedEventArgs e)
        {
            this.GetUserWindow();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;

            if (this.isForceUnchecked)
            {
                return;
            }

            if (radioButton.DataContext == null)
            {
                return;
            }

            if (radioButton.IsChecked != true)
            {
                return;
            }

            DataGridCellPresenter dataGridCellPresenter = (DataGridCellPresenter)radioButton.Parent;
            if (dataGridCellPresenter == null)
            {
                return;
            }

            C1DataGrid dataGrid = dataGridCellPresenter.DataGrid;
            int selectedIndex = dataGridCellPresenter.Row.Index;

            // Check Value 모두 False 치고, 선택된 Index의 Check Value True 침
            this.isForceUnchecked = true;
            for (int i = 0; i < dataGrid.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "CHK", false);
            }
            DataTableConverter.SetValue(dataGrid.Rows[selectedIndex].DataItem, "CHK", true);
            this.isForceUnchecked = false;

            // 선택된 Grid의 항목이 HoldGroup Level 1이면
            if (string.Equals(radioButton.GroupName, "radHoldGroup1"))
            {
                string defectCode = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[selectedIndex].DataItem, "DFCT_CODE"));
                Util.GridSetData(this.dgHoldGroup2, this.dataHelper.GetAreaDefectDetailCode(HOLD_LOT, defectCode), FrameOperation, true);
            }
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            this.HoldProcess();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.SearchProcess(button);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            this.ExcelExport(button);
        }

        private void btnUnusualHoldStockSave_Click(object sender, RoutedEventArgs e)
        {
            this.SetUnusualHoldStock();
        }

        private void txtLOTID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            TextBox textBox = (TextBox)sender;
            this.SearchProcess(textBox);
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                this.txtHoldPerson.Text = wndPerson.USERNAME;
                this.txtHoldPerson.Tag = wndPerson.USERID;
            }
        }

        private void holdExcelImportEditor_Closed(object sender, EventArgs e)
        {
            HoldExcelImportEditor holdExcelImportEditor = (HoldExcelImportEditor)sender;
            if (holdExcelImportEditor != null && holdExcelImportEditor.DialogResult == MessageBoxResult.OK)
            {
                DataTable dt = holdExcelImportEditor.ImportHoldData;
                this.AddHoldLOTGrid(dt);
            }
        }
        #endregion
    }

    internal class PACK001_060_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_060_DataHelper()
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

        // 순서도 호출 - Project Model 정보
        internal DataTable GetProjectModelInfo(string areaID, string eqsgID)
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
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
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
        internal DataTable GetProductByProjectModelOrProjectClassCodeInfo(string shopID, string areaID, string eqsgID, string projectModel, string productClassCode)
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
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
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
        internal DataTable GetProductTypeInfo(string shopID, string areaID, string eqsgID, string areaTypeCode, string productClassCode)
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
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Activity Reason
        internal DataTable GetActivityReasonInfo(string actID)
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
                drRQSTDT["ACTID"] = actID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Level 1 Defect Code (Hold 사유)
        internal DataTable GetAreaDefectCode(string actID)
        {
            string bizRuleName = "DA_BAS_SEL_AREA_HOLD_DFCT_CODE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["ACTID"] = actID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Level 2 Defect Code (Hold 사유)
        internal DataTable GetAreaDefectDetailCode(string actID, string defectCode)
        {
            string bizRuleName = "DA_BAS_SEL_AREA_HOLD_DFCT_DETL_CODE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("DFCT_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["ACTID"] = actID;
                drRQSTDT["DFCT_CODE"] = defectCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Get LOTID Check
        internal DataTable GetLOTIDValidation(string LOTID, string holdFlag)
        {
            string bizRuleName = "DA_BAS_SEL_CELLID_VALI";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("DFCT_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = LOTID;
                drRQSTDT["HOLD_FLAG"] = string.IsNullOrEmpty(holdFlag) ? null : holdFlag;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Get Pack Hold List
        internal DataTable GetPackHoldList(string productID, DateTime dteFromDate, DateTime dteToDate, string equipmentSegmentID, string productClassCode)
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

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["PRODID"] = string.IsNullOrEmpty(productID) ? null : productID;
                drRQSTDT["FROMDATE"] = dteFromDate.ToString("yyyyMMdd");
                drRQSTDT["TODATE"] = dteToDate.ToString("yyyyMMdd");
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["CLASS"] = string.IsNullOrEmpty(productClassCode) ? null : productClassCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Hold Release History
        internal DataTable GetPackHoldReleaseHistory(string productClassCode, string productModel, string productID, string LOTID, string equipmentSegmentID, DateTime dteFromDate, DateTime dteToDate, bool? termCheckFlag)
        {
            string bizRuleName = "DA_PRD_SEL_HOLD_RELEASE_HIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CLASS", typeof(string));
                dtRQSTDT.Columns.Add("PJT", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                dtRQSTDT.Columns.Add("TO_DTTM", typeof(string));
                dtRQSTDT.Columns.Add("TERMCHK_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CLASS"] = string.IsNullOrEmpty(productClassCode) ? null : productClassCode;
                drRQSTDT["PJT"] = string.IsNullOrEmpty(productModel) ? null : productModel;
                drRQSTDT["PRODID"] = string.IsNullOrEmpty(productID) ? null : productID;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["FROM_DTTM"] = dteFromDate.ToString("yyyyMMdd");
                drRQSTDT["TO_DTTM"] = dteToDate.ToString("yyyyMMdd");
                drRQSTDT["TERMCHK_FLAG"] = (termCheckFlag == true) ? "Y" : null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

             return dtRSLTDT;
        }

        // 순서도 호출 - QMS Release History
        internal DataTable GetPackQMSHoldReleaseHistory(string productClassCode, string productModel, string productID, string LOTID, string equipmentSegmentID, DateTime dteFromDate, DateTime dteToDate, bool? termCheckFlag)
        {
            string bizRuleName = "DA_PRD_SEL_QMS_HOLD_RELEASE_HIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CLASS", typeof(string));
                dtRQSTDT.Columns.Add("PJT", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                dtRQSTDT.Columns.Add("TO_DTTM", typeof(string));
                dtRQSTDT.Columns.Add("TERMCHK_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CLASS"] = string.IsNullOrEmpty(productClassCode) ? null : productClassCode;
                drRQSTDT["PJT"] = string.IsNullOrEmpty(productModel) ? null : productModel;
                drRQSTDT["PRODID"] = string.IsNullOrEmpty(productID) ? null : productID;
                drRQSTDT["LOTID"] = string.IsNullOrEmpty(LOTID) ? null : LOTID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(equipmentSegmentID) ? null : equipmentSegmentID;
                drRQSTDT["FROM_DTTM"] = dteFromDate.ToString("yyyyMMdd");
                drRQSTDT["TO_DTTM"] = dteToDate.ToString("yyyyMMdd");
                drRQSTDT["TERMCHK_FLAG"] = (termCheckFlag == true) ? "Y" : null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Hold Transaction
        internal bool HoldTransaction(string actionUserID, DateTime calDate, DataTable dt, string holdNote, string resnCode, string holdCode, DateTime dteUnHoldScheduleDate, bool? unusualHoldStockFlag)
        {
            // Declarations...
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_HOLD_LOT";
            DataSet dsINDATA = new DataSet();

            try
            {
                // Make INDATA
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("IFMODE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("ACTION_USERID", typeof(string));
                dtINDATA.Columns.Add("CALDATE", typeof(DateTime));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["IFMODE"] = IFMODE.IFMODE_OFF;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["ACTION_USERID"] = actionUserID;
                drINDATA["CALDATE"] = calDate;
                dtINDATA.Rows.Add(drINDATA);

                // Make INLOT
                var query_INLOT = dt.AsEnumerable().Select(x => new
                {
                    LOTID = x.Field<string>("LOTID"),
                    HOLD_NOTE = holdNote.Replace("\r\n", string.Empty).Trim(),
                    RESNCODE = resnCode,
                    HOLD_CODE = holdCode,
                    UNHOLD_SCHD_DATE = dteUnHoldScheduleDate.ToString("yyyyMMdd"),
                    SPCL_HOLD_STCK_FLAG = (unusualHoldStockFlag == true) ? "Y" : "N"
                });

                DataTable dtINLOT = PackCommon.queryToDataTable(query_INLOT.ToList());
                dtINLOT.TableName = "INLOT";

                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtINLOT);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // 순서도 호출 - 특이보류재고 설정
        internal bool SetUnusualHoldStock(IEnumerable<dynamic> records)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_UPD_WIPHOLDHISTORY_MANAGER";
                DataTable dtRQSTDT = PackCommon.queryToDataTable(records.ToList());
                dtRQSTDT.TableName = "RQSTDT";
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }
        #endregion
    }
}