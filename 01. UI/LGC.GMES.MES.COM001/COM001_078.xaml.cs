/*************************************************************************************
  Decription : 재고 조정 
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.27  김건식 : 이력 관리 시작, W/O 조뢰 결과 수정 및 저장위치 자릿수 Validation 기능 추가 (4자리가 아니면 error 팝업 발생.)
  2024.05.27  이대근 : 재고조정 시 ERP 전송 대상 LOT 체크 기능 추가/ 원부자재일 경우 WOID 필수 입력 기능 추가
  2024.07.26  김영택 : 이력 조회시 KEY_LOTID 속성 추가 (24.08.20 다시 체크인)
  2024.08.26  김수용 : E20240823-001393,  SUCCESS_CANCEL 건수를 성공건수에 포함
  2025.03.21  오화백 : LOAD 이벤트는 한번만 타도록 수정
  2025.06.18  조영대 : Catch Up [E20241021-000263] 적용
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;
using System.Windows.Media;
using System.Globalization;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;
using System.Linq;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_078 : UserControl, IWorkArea
    {
        #region Private 변수
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        DataTable dtHistList = new DataTable();
        int _iRowIndex = 0;
        private string AREATYPE = string.Empty;
        Util _Util = new Util();
        #endregion

        #region Form Load & Init Control
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_078()
        {
            InitializeComponent();
            String[] sFilter1 = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "SHOP");

            //// 환경설정 Shop 선택
            //DataTable dt = DataTableConverter.Convert(cboShop.ItemsSource);
            //DataTable dtShop = dt.Clone();
            //var drTmp = dt?.Select("CBO_CODE = '" + LoginInfo.CFG_SHOP_ID + "'");
            //if (drTmp.Length > 0)
            //    dtShop = drTmp.CopyToDataTable();

            //cboShop.ItemsSource = dtShop.Copy().AsDataView();
            //cboShop.SelectedIndex = 0;

            (dgTransfer.Columns["STOCK_VALUATION_TYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtSTOCK_VALUATION_TYPECombo("STOCK_VALUATION_TYPE"));
            GetStoredLocationInfo();
        }

        private DataTable dtSTOCK_VALUATION_TYPECombo(string sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
            AddStatus(dtResult, CommonCombo.ComboStatus.NA, "CBO_CODE", "CBO_NAME");
            return dtResult;
        }

        private void GetStoredLocationInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_MMD_SLOC_STCK_ADJ_TRGT_FLAG";

                string selectedValueText = (string)((PopupFindDataColumn)dgTransfer.Columns["SLOC_ID"]).SelectedValuePath;
                string selectedDisplayText = (string)((PopupFindDataColumn)dgTransfer.Columns["SLOC_ID"]).DisplayMemberPath;

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("SLOC_ID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["SLOC_ID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRQSTDT);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, selectedDisplayText });

                PopupFindDataColumn column = dgTransfer.Columns["SLOC_ID"] as PopupFindDataColumn;
                column.AddMemberPath = "SLOC_ID";
                column.ItemsSource = DataTableConverter.Convert(dtBinding);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
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

        #endregion

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            GetAreaType();
            this.Loaded -= UserControl_Loaded;
            // ERP Stock Valuation Type 
            /*
            chkStockType.IsChecked = false;
            if (_Util.IsCommonCodeUse("STOCK_VALUATION_TYPE_VISIBLE", LoginInfo.CFG_SHOP_ID) == true)
                chkStockType.Visibility = Visibility.Visible;
            else
                chkStockType.Visibility = Visibility.Collapsed;
            */
        }

        private void dgTransfer_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (!dg.CurrentCell.IsEditing)
                {
                    switch (dg.CurrentCell.Column.Name)
                    {
                        /*
                        case "SHOPID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHOPID")?.ToString().Length > 0)
                            {
                                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SLOC_BY_SHOP", new string[] { "SHOPID" }, new string[] { DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHOPID") as string }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["SLOC_ID"], "CBO_CODE", "CBO_NAME");
                                // ERP Stock Valuation Type 
                                if (_Util.IsCommonCodeUse("STOCK_VALUATION_TYPE_VISIBLE", Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHOPID"))) == true)
                                    chkStockType.Visibility = Visibility.Visible;
                                else
                                    chkStockType.Visibility = Visibility.Collapsed;
                            }                                
                            break;
                        */
                        case "PRODID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString().Length > 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "PRODID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID").ToString().ToUpper());
                                SetProdUnit(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString(), e.Cell.Row.Index);
                            }
                            break;
                        //case "LOTID":
                        //    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID")?.ToString().Length > 0)
                        //    {
                        //        DataTableConverter.SetValue(e.Cell.Row.DataItem, "LOTID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID").ToString().ToUpper());
                        //        SetProdUnit(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID")?.ToString(), e.Cell.Row.Index);
                        //    }
                        //    break;
                        case "SLOC_ID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SLOC_ID")?.ToString().Length == 4)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "SLOC_ID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "SLOC_ID").ToString().ToUpper());
                                SetStoreLocation(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHOPID")?.ToString(), DataTableConverter.GetValue(e.Cell.Row.DataItem, "SLOC_ID")?.ToString(), e.Cell.Row.Index);

                                SetWorkOrder(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHOPID")?.ToString(), DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString(), DataTableConverter.GetValue(e.Cell.Row.DataItem, "SLOC_ID")?.ToString(), DataTableConverter.GetValue(e.Cell.Row.DataItem, "CALDATE")?.ToString(),
                                    e.Cell.Row.DataItem);
                            }
                            //else
                            //{
                            //    Util.MessageValidation("SFU8232"); // 저장위치 4개 자릿수를 모두 입력 후 다시 시도하십시오.
                            //    return;
                            //}
                            break;
                    }
                }

                //dg.EndEdit();
                //ConfirmSendCheck(e.Cell.Row.Index, e.Cell.Row.Index + 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmSend() == false)
            {
                return;
            }

            string shopId = string.Empty;
            string calDate = string.Empty;
            string Lotid = string.Empty;
            string prodId = string.Empty;
            string unit = string.Empty;
            decimal defectQty = 0;
            decimal lossQty = 0;
            decimal lenesceed = 0;
            string slocid = string.Empty;
            string defectslocid = string.Empty;
            string woId = string.Empty;
            // string StckType = string.Empty;
            // string StckTypeName = string.Empty;

            // 전송 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
            {
                if (vResult == MessageBoxResult.OK)
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SHOPID", typeof(string));
                    inDataTable.Columns.Add("CALDATE", typeof(string));
                    inDataTable.Columns.Add("PRODID", typeof(string));
                    // ERP Stock Valuation Type 
                    inDataTable.Columns.Add("BATCH", typeof(string));
                    inDataTable.Columns.Add("UNIT_CODE", typeof(string));
                    inDataTable.Columns.Add("CNFM_DFCT_QTY2", typeof(decimal));
                    inDataTable.Columns.Add("CNFM_LOSS_QTY2", typeof(decimal));
                    inDataTable.Columns.Add("LENGTH_EXCEED2", typeof(decimal));
                    inDataTable.Columns.Add("SLOC_ID", typeof(string));
                    inDataTable.Columns.Add("WOID", typeof(string));
                    inDataTable.Columns.Add("DFCT_SLOC_ID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("CRRT_NOTE", typeof(string));
                    inDataTable.Columns.Add("LOTID", typeof(string));
                    //  inDataTable.Columns.Add("STCK_TYPE", typeof(string));


                    for (int nrow = 0; nrow < dgTransfer.Rows.Count; nrow++)
                    {
                        shopId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SHOPID") as string;
                        //calDate = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE") as string;
                        calDate = Convert.ToDateTime(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE")).ToString("yyyMMdd") as string;
                        Lotid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "LOTID") as string;
                        prodId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "PRODID") as string;
                        unit = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "UNIT_CODE") as string;
                        lossQty = Convert.ToDecimal(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CNFM_LOSS_QTY2"));
                        lenesceed = Convert.ToDecimal(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "LENGTH_EXCEED2"));
                        if (DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SLOC_ID").ToString().Length != 4)
                        {
                            Util.MessageValidation("SFU8232"); // 저장위치 4개 자릿수를 모두 입력 후 다시 시도하십시오.
                            return;
                        }
                        slocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SLOC_ID") as string;
                        woId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "WOID") as string;
                        // StckType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "STCK_TYPE") as string;
                        // StckTypeName = dgTransfer.GetCell(nrow, dgTransfer.Columns["STCK_TYPE"].Index).Text;
                        defectslocid = null;

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SHOPID"] = shopId;
                        inDataRow["CALDATE"] = calDate;
                        if (string.IsNullOrEmpty(prodId))
                        {
                            inDataRow["PRODID"] = prodId;
                        }
                        else
                        {
                            inDataRow["PRODID"] = prodId.ToUpper();
                        }
                        // ERP Stock Valuation Type
                        //inDataRow["BATCH"] = (chkStockType.Visibility == Visibility.Visible && (bool)chkStockType.IsChecked == true) ? "PUR" : null;  
                        inDataRow["BATCH"] = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "STOCK_VALUATION_TYPE") as string;
                        inDataRow["UNIT_CODE"] = unit;
                        inDataRow["CNFM_DFCT_QTY2"] = 0;
                        inDataRow["CNFM_LOSS_QTY2"] = lossQty;
                        inDataRow["LENGTH_EXCEED2"] = lenesceed;
                        if (string.IsNullOrEmpty(slocid))
                        {
                            inDataRow["SLOC_ID"] = slocid;
                        }
                        else
                        {
                            inDataRow["SLOC_ID"] = slocid.ToUpper();
                        }
                        inDataRow["WOID"] = woId;
                        inDataRow["DFCT_SLOC_ID"] = defectslocid;
                        inDataRow["CRRT_NOTE"] = txtNote.Text;

                        if (txtReqUser.Tag == null)
                        {
                            Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("요청자"));
                            return;
                        }
                        inDataRow["USERID"] = txtReqUser.Tag;
                        if (string.IsNullOrEmpty(Lotid))
                        {
                            inDataRow["LOTID"] = Lotid;
                        }
                        else
                        {
                            inDataRow["LOTID"] = Lotid.ToUpper();
                        }

                        //if (string.IsNullOrEmpty(StckType))
                        //{
                        //    inDataRow["STCK_TYPE"] = null;
                        //}
                        //else
                        //{
                        //    if (StckTypeName.Equals(""))
                        //    {
                        //        inDataRow["STCK_TYPE"] = null;
                        //    }
                        //    else
                        //    {

                        //        inDataRow["STCK_TYPE"] = StckType;
                        //    }
                        //}


                        inDataTable.Rows.Add(inDataRow);
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_ADJUST", "INDATA", null, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.AlertByBiz("BR_PRD_REG_STOCK_ADJUST", ex.Message, ex.ToString());
                            return;
                        }
                    }, inDataSet);

                    // 전송 완료 되었습니다.
                    Util.MessageInfo("SFU1880");

                    Init();
                }
            }, false, false, string.Empty);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            for (int addrow = 0; addrow < txtRowCnt.Value; addrow++)
                DataGridRowAdd(dgTransfer);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgTransfer);
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSendCheck(0, dgTransfer.Rows.Count);
        }

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        private void dgTransfer_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK")).ToString().Equals("ERROR"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
                }

            }));

        }

        private void btnCancelSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConfirmCancel() == false)
                {
                    return;
                }

                string erp_trnf_seqno = string.Empty;
                string crr_note = string.Empty;
                string chk = string.Empty;
                int chk_cnt = 0;

                string lotid = string.Empty;
                decimal wipseq;
                DateTime actdttm = new DateTime();

                // 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1243"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {

                        DataTable inDataTable = new DataTable("INDATA");
                        inDataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                        inDataTable.Columns.Add("NOTE", typeof(string));
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
                        inDataTable.Columns.Add("ACTDTTM", typeof(DateTime));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        for (int nrow = 0; nrow < dgListHist.GetRowCount(); nrow++)
                        {
                            chk = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK") as string;
                            erp_trnf_seqno = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "ERP_TRNF_SEQNO") as string;
                            crr_note = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CRRT_NOTE"));
                            //lotid = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "LOTID"));
                            lotid = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "KEY_LOTID"));
                            wipseq = Util.NVC_Decimal(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "WIPSEQ"));
                            //actdttm = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "ACTDTTM"));
                            actdttm = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "ACTDTTM")), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                            if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK").IsTrue())
                            {
                                DataRow inDataRow = inDataTable.NewRow();
                                inDataRow["ERP_TRNF_SEQNO"] = erp_trnf_seqno;
                                inDataRow["NOTE"] = crr_note + "[" + Util.NVC(txtCancelNote.Text) + "]";
                                //inDataRow["LOTID"] = erp_trnf_seqno;
                                inDataRow["LOTID"] = lotid;
                                inDataRow["WIPSEQ"] = wipseq;
                                inDataRow["ACTDTTM"] = actdttm;
                                inDataRow["USERID"] = LoginInfo.USERID;

                                inDataTable.Rows.Add(inDataRow);

                                chk_cnt++;
                            }
                        }

                        if (chk_cnt > 0)
                        {
                            try
                            {
                                new ClientProxy().ExecuteServiceSync("BR_ACT_REG_CANCEL_RSLT_ERP_PROD", "INDATA", null, inDataTable);

                                Util.MessageInfo("SFU1889"); // 정상 처리 되었습니다

                                Init();
                                GetListHist();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                        else
                        {
                            Util.MessageInfo("SFU1636"); // 선택된 대상이 없습니다.
                        }
                    }
                }, false, false, string.Empty);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReSend_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmReSend() == false)
            {
                return;
            }

            string erp_trnf_seqno = string.Empty;
            string chk = string.Empty;
            int chk_cnt = 0;

            // 전송 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
            {
                if (vResult == MessageBoxResult.OK)
                {

                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                    for (int nrow = 0; nrow < dgListHist.GetRowCount(); nrow++)
                    {
                        chk = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK") as string;
                        erp_trnf_seqno = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "ERP_TRNF_SEQNO") as string;

                        if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK").IsTrue())
                        {
                            DataRow inDataRow = inDataTable.NewRow();
                            inDataRow["ERP_TRNF_SEQNO"] = erp_trnf_seqno;

                            inDataTable.Rows.Add(inDataRow);

                            chk_cnt++;
                        }
                    }

                    if (chk_cnt > 0)
                    {
                        new ClientProxy().ExecuteServiceSync("BR_ACT_REG_RESEND_ERP_PROD", "INDATA", null, inDataTable);

                        Util.MessageInfo("SFU1880"); // 전송 완료 되었습니다.

                        Init();
                        GetListHist();
                    }
                    else
                    {
                        // 전송 완료 되었습니다.
                        Util.MessageInfo("SFU1636"); // 선택된 대상이 없습니다.
                    }
                }
            }, false, false, string.Empty);
        }

        private void dgListHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int inx = 0; inx < dgListHist.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListHist.Rows[inx].DataItem, "CHK", true);

                    //if (Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "IF_FLAG")).Equals("FAIL"))
                    //{
                    //    DataTableConverter.SetValue(dgListHist.Rows[inx].DataItem, "CHK", true);
                    //}
                }
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int inx = 0; inx < dgListHist.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListHist.Rows[inx].DataItem, "CHK", false);
            }
        }

        private void CHK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                //if (DataTableConverter.GetValue(dgListHist.Rows[row_index].DataItem, "CHK").Equals("True"))
                //{
                //    //재전송
                //    if (DataTableConverter.GetValue(dgListHist.Rows[row_index].DataItem, "IF_FLAG") == null || !DataTableConverter.GetValue(dgListHist.Rows[row_index].DataItem, "IF_FLAG").Equals("FAIL"))
                //    {
                //        DataTableConverter.SetValue(dgListHist.Rows[row_index].DataItem, "CHK", "False");
                //        Util.AlertInfo("SFU4911"); //ERP I/F가 실패일 경우에만 재전송 가능합니다.
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        /*
        #region # ERP Stock Valuation Type
        private void chkStockType_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkStockType_Unchecked(object sender, RoutedEventArgs e)
        {

        }
        #endregion
        */

        #endregion

        #region Functions
        void Init()
        {
            dgTransfer.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("SHOPID", typeof(string));
            emptyTransferTable.Columns.Add("CALDATE", typeof(string));
            emptyTransferTable.Columns.Add("PRODID", typeof(string));
            emptyTransferTable.Columns.Add("LOTID", typeof(string));
            emptyTransferTable.Columns.Add("UNIT_CODE", typeof(string));
            emptyTransferTable.Columns.Add("CNFM_DFCT_QTY2", typeof(decimal));
            emptyTransferTable.Columns.Add("CNFM_LOSS_QTY2", typeof(decimal));
            emptyTransferTable.Columns.Add("LENGTH_EXCEED2", typeof(decimal));
            emptyTransferTable.Columns.Add("SLOC_ID", typeof(string));
            emptyTransferTable.Columns.Add("DFCT_SLOC_ID", typeof(string));
            emptyTransferTable.Columns.Add("CHK", typeof(string));
            emptyTransferTable.Columns.Add("WOID", typeof(string));
            emptyTransferTable.Columns.Add("STOCK_VALUATION_TYPE", typeof(string));
            emptyTransferTable.Columns.Add("VERIFICATION_RESULT", typeof(string));
            //emptyTransferTable.Columns.Add("STCK_TYPE", typeof(string));

            dgTransfer.ItemsSource = DataTableConverter.Convert(emptyTransferTable);

            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SHOP_CBO", new string[] { "LANGID", "SHOPID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["SHOPID"], "CBO_CODE", "CBO_NAME");
            // CommonCombo.SetDataGridComboItem("DA_BAS_SEL_STCK_TYPE_CBO", new string[] { "LANGID", "CODE" }, new string[] { LoginInfo.LANGID, "STCK_ADJ_TYPE" }, CommonCombo.ComboStatus.NA, dgTransfer.Columns["STCK_TYPE"], "CBO_CODE", "CBO_NAME");
        }
        void SetProdUnit(string prodId, int iRow)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("MTRLID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["MTRLID"] = prodId;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_MATERIAL", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
                //DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "UNIT_CODE", "");
                DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "UNIT_CODE", "");
            else
                DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "UNIT_CODE", Util.NVC(dt.Rows[0]["MTRLUNIT"]));

        }

        void SetWorkOrder(string shopid, string prodId, string SlocId, string sCalDate, object oItem)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("ISS_SLOC_ID", typeof(string));
            IndataTable.Columns.Add("DFLT_FLAG", typeof(string));
            IndataTable.Columns.Add("CALDATE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = shopid;
            Indata["MTRLID"] = prodId;
            Indata["ISS_SLOC_ID"] = SlocId;
            Indata["DFLT_FLAG"] = "Y";
            Indata["CALDATE"] = sCalDate;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_MTRL_INPUT", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
                DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "WOID", "");
            else
            {
                if (DataTableConverter.GetValue(oItem, "LENGTH_EXCEED2")?.GetDecimal() > 0)
                {
                    DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "WOID", Util.NVC(dt.Rows[dt.Rows.Count - 1]["WOID"]));
                }
                else if (Util.NVC(dt.Rows[0]["WOID"]) != "PPLO" && DataTableConverter.GetValue(oItem, "CNFM_LOSS_QTY2")?.GetDecimal() > 0)
                {
                    DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "WOID", Util.NVC(dt.Rows[0]["WOID"]));
                }
                else
                {
                    DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "WOID", Util.NVC(dt.Rows[0]["WOID"]));
                }
            }

        }

        void SetWorkOrder_send(string shopid, string prodId, string SlocId, string sCalDate, int iRow)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("ISS_SLOC_ID", typeof(string));
            IndataTable.Columns.Add("DFLT_FLAG", typeof(string));
            IndataTable.Columns.Add("CALDATE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = shopid;
            Indata["MTRLID"] = prodId;
            Indata["ISS_SLOC_ID"] = SlocId;
            Indata["DFLT_FLAG"] = "Y";
            Indata["CALDATE"] = sCalDate;
            IndataTable.Rows.Add(Indata);


            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_MTRL_INPUT", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
                DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "WOID", "");
            else
            {
                DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "WOID", Util.NVC(dt.Rows[0]["WOID"]));
            }

        }

        void SetStoreLocation(String shopid, string slocid, int iRow)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("SLOC_ID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SHOPID"] = shopid;
            Indata["SLOC_ID"] = slocid;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SLOC", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
            {
                DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "SLOC_ID", "");
            }
            else
            {
                if (slocid == null)
                {
                    DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "SLOC_ID", "");
                }
                else
                {
                    DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "SLOC_ID", slocid.ToUpper());
                }
            }

        }

        //void SetDefectStoreLocation(String shopid)
        //{
        //    DataTable IndataTable = new DataTable("INDATA");
        //    IndataTable.Columns.Add("SHOPID", typeof(string));

        //    DataRow Indata = IndataTable.NewRow();
        //    Indata["SHOPID"] = shopid;
        //    IndataTable.Rows.Add(Indata);

        //    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SLOC_DEFECT", "INDATA", "RSLTDT", IndataTable);

        //    if (dt == null || dt.Rows.Count == 0)
        //        DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "DFCT_SLOC_ID","");
        //    else
        //        DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "DFCT_SLOC_ID", Util.NVC(dt.Rows[0]["DFCT_SLOC_ID"]));

        //}

        void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                // 해더 제외
                DataTable dt = DataTableConverter.Convert(dgTransfer.ItemsSource).Clone();

                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    DataRow dr = dt.NewRow();

                    if (sheet.GetCell(rowInx, 0) == null)
                        dr["SHOPID"] = "";
                    else
                        dr["SHOPID"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);

                    if (sheet.GetCell(rowInx, 1) == null)
                        dr["CALDATE"] = "";
                    else
                        dr["CALDATE"] = Util.NVC(sheet.GetCell(rowInx, 1).Text.Substring(0, 4) + "-" + sheet.GetCell(rowInx, 1).Text.Substring(4, 2) + "-" + sheet.GetCell(rowInx, 1).Text.Substring(6, 2));
                    //dr["CALDATE"] = Util.NVC(sheet.GetCell(rowInx, 1).Text);

                    if (sheet.GetCell(rowInx, 2) == null)
                        dr["LOTID"] = "";
                    else
                        dr["LOTID"] = Util.NVC(sheet.GetCell(rowInx, 2).Text);

                    if (sheet.GetCell(rowInx, 3) == null)
                        dr["PRODID"] = "";
                    else
                        dr["PRODID"] = Util.NVC(sheet.GetCell(rowInx, 3).Text);

                    if (sheet.GetCell(rowInx, 4) == null)
                        dr["UNIT_CODE"] = "";
                    else
                        dr["UNIT_CODE"] = Util.NVC(sheet.GetCell(rowInx, 4).Text);

                    if (sheet.GetCell(rowInx, 5) == null)
                        dr["CNFM_LOSS_QTY2"] = 0;
                    else
                        dr["CNFM_LOSS_QTY2"] = Util.NVC(sheet.GetCell(rowInx, 5).Text);

                    if (sheet.GetCell(rowInx, 6) == null)
                        dr["LENGTH_EXCEED2"] = 0;
                    else
                        dr["LENGTH_EXCEED2"] = Util.NVC(sheet.GetCell(rowInx, 6).Text);

                    if (sheet.GetCell(rowInx, 7) == null)
                        dr["SLOC_ID"] = "";
                    else
                        dr["SLOC_ID"] = Util.NVC(sheet.GetCell(rowInx, 7).Text);

                    if (sheet.GetCell(rowInx, 8) == null)
                        dr["STOCK_VALUATION_TYPE"] = "";
                    else
                        dr["STOCK_VALUATION_TYPE"] = Util.NVC(sheet.GetCell(rowInx, 8).Text);

                    //                    if (sheet.GetCell(rowInx, 7) == null)
                    //                        dr["DFCT_SLOC_ID"] = "";
                    //                    else
                    //                        dr["DFCT_SLOC_ID"] = Util.NVC(sheet.GetCell(rowInx, 7).Text);

                    //if (!string.IsNullOrWhiteSpace(dr["SHOPID"].ToString()))
                    //    CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SLOC_BY_SHOP", new string[] { "SHOPID" }, new string[] { dr["SHOPID"].ToString() }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["SLOC_ID"], "CBO_CODE", "CBO_NAME");

                    //if (sheet.GetCell(rowInx, 11) == null)
                    //    dr["STCK_TYPE"] = "";
                    //else
                    //    dr["STCK_TYPE"] = Util.NVC(sheet.GetCell(rowInx, 11).Text);
                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
                Util.GridSetData(dgTransfer, dt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        bool ConfirmSend()
        {
            string shopId = string.Empty;
            string calDate = string.Empty;
            string Lotid = string.Empty;
            string prodId = string.Empty;
            string unit = string.Empty;
            decimal defectQty = 0;
            decimal lossQty = 0;
            decimal lenesceed = 0;
            string slocid = string.Empty;
            string stockValuationType = string.Empty;
            string defectslocid = string.Empty;
            string woid = string.Empty;

            if (dgTransfer.Rows.Count == 0)
                return false;

            // 제품단위, 저장위치 재설정
            for (int nrow = 0; nrow < dgTransfer.Rows.Count; nrow++)
            {
                shopId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SHOPID"));
                prodId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "PRODID"));
                slocid = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SLOC_ID"));
                //calDate = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE"));
                calDate = Util.NVC(Convert.ToDateTime(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE")).ToString("yyyyMMdd"));

                SetProdUnit(prodId, nrow);
                SetStoreLocation(shopId, slocid, nrow);

                //NERP 원부자재일 경우 WOID 자동입력
                string result_Mtrl = ChkMTRL(prodId);
                if (result_Mtrl == "MTRL")
                {
                    SetWorkOrder_send(shopId, prodId, slocid, calDate, nrow);
                }
            }

            for (int nrow = 0; nrow < dgTransfer.Rows.Count; nrow++)
            {


                shopId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SHOPID") as string;
                //calDate = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE") as string;
                calDate = Convert.ToDateTime(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE")).ToString("yyyyMMdd") as string;
                Lotid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "LOTID") as string;
                prodId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "PRODID") as string;
                unit = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "UNIT_CODE") as string;
                defectQty = 0;
                lossQty = Convert.ToDecimal(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CNFM_LOSS_QTY2"));
                lenesceed = Convert.ToDecimal(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "LENGTH_EXCEED2"));
                slocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SLOC_ID") as string;
                stockValuationType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "STOCK_VALUATION_TYPE") as string;
                //                defectslocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "DFCT_SLOC_ID") as string;
                woid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "WOID") as string;
                defectslocid = null;

                DataTable dtchk = DataTableConverter.Convert(dgTransfer.ItemsSource);

                //전송시 SLOC_ID CHECK
                SetStoreLocation(shopId, slocid, nrow);
                slocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "SLOC_ID") as string;

                // {0}이 입력되지 않았습니다.
                if (string.IsNullOrWhiteSpace(shopId))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("플랜트"));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(calDate))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("전기일"));
                    return false;
                }
                // 전기일 
                try
                {
                    DateTime date = DateTime.ParseExact(calDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                    if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) < Convert.ToDecimal(date.ToString("yyyyMMdd")))
                    {
                        Util.MessageValidation("SFU1739");  //오늘 이후 날짜는 선택할 수 없습니다.
                        return false;
                    }
                    if (CloseCalDate(calDate))
                    {
                        Util.MessageValidation("SFU3494");  // ERP 생산실적이 마감 되었습니다.
                        return false;
                    }
                }
                catch
                {
                    Util.MessageValidation("SFU3241", calDate);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(prodId))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("제품ID"));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(unit))
                {
                    //Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("단위"));
                    Util.MessageValidation("1", (nrow + 1).ToString() + "row " + MessageDic.Instance.GetMessage("SFU9930"));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(slocid))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("저장위치"));
                    return false;
                }

                if (defectQty == 0 && lossQty == 0 && lenesceed == 0)
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("Loss량") + " or " + ObjectDic.Instance.GetObjectName("불량량") + " or " + ObjectDic.Instance.GetObjectName("길이초과"));
                    return false;
                }

                //                if (defectQty != 0 && string.IsNullOrWhiteSpace(defectslocid))
                //                {
                //                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("불량저장위치"));
                //                    return false;
                //                }

                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("사유"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtReqUser.Text) || txtReqUser.Tag == "")
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("요청자"));
                    return false;
                }

                string result = ValidateSTOCK_VALUATION_PRODUCT(shopId, prodId, stockValuationType);
                if (result != "P")
                {
                    if (result == "M")
                    {
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값을 입력하시기 바랍니다.
                        Util.MessageValidation("SFU3808", new object[] { (nrow + 1).ToString(), shopId, prodId });
                    }
                    else if (result == "O")
                    {
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값은 입력될 수 없습니다.
                        Util.MessageValidation("SFU3809", new object[] { (nrow + 1).ToString(), shopId, prodId });
                    }

                    return false;
                }

                //NERP
                if (Lotid == null)
                {
                    Lotid = "";
                }
                string result_Lot = ChkLotid(Lotid.ToString().Trim(), prodId, slocid, shopId);
                if (result_Lot != "P")
                {
                    if (result_Lot == "N")
                    {
                        //선택된 LOT과 제품코드가 다릅니다.
                        Util.MessageValidation("SFU2907");
                    }
                    else if (result_Lot == "F")
                    {
                        //LOT정보가 없습니다.
                        Util.MessageValidation("SFU1386");
                    }
                    else if (result_Lot == "C")
                    {
                        //전송 대상 LOT이 아닙니다.
                        Util.MessageValidation("SFU2090");
                    }
                    else if (result_Lot == "X")
                    {
                        //전송 대상 LOT이 아닙니다.
                        Util.MessageValidation("SFU2091");
                    }

                    return false;
                }
                string result_Mtrl = ChkMTRL(prodId);
                if (result_Mtrl == "MTRL")
                {
                    if (string.IsNullOrWhiteSpace(woid))
                    {
                        //Workorder ID가 선택되지 않았습니다.
                        Util.MessageValidation("SFU1441");
                        return false;
                    }
                }
            }

            return true;
        }

        bool ConfirmCancel()
        {
            for (int nrow = 0; nrow < dgListHist.Rows.Count; nrow++)
            {
                if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK").IsTrue())
                {
                    if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "IF_FLAG") == null ||
                       !DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "IF_FLAG").Equals("SUCCESS"))
                    {
                        Util.MessageValidation("SFU8247", (nrow + 1).ToString() + "row (" + Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "MATERIAL")) + ")");  // [%1] ERP I/F가 성공일 경우에만 취소 가능합니다.
                        return false;
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CNCL_FLAG")), "Y"))
                    {
                        Util.MessageValidation("SFU8248", (nrow + 1).ToString() + "row (" + Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "MATERIAL")) + ")");  // [%1] 이미 취소처리 되었습니다.
                        return false;
                    }

                    string calDate = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "POSTG_DATE")).Replace("-", "");
                    if (CloseCalDate(calDate))
                    {
                        Util.MessageValidation("SFU3494");  // ERP 생산실적이 마감 되었습니다.
                        return false;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(txtCancelNote.Text))
            {
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("사유"));
                return false;
            }
            return true;
        }
        bool ConfirmReSend()
        {
            // 전송대상 Check : FAIL
            for (int inx = 0; inx < dgListHist.GetRowCount(); inx++)
            {
                if (DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "CHK").IsTrue())
                {
                    if (DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "IF_FLAG") == null ||
                        !DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "IF_FLAG").Equals("FAIL"))
                    {
                        Util.MessageValidation("SFU8246", (inx + 1).ToString() + "row (" + Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "MATERIAL")) + ")"); // [%1] ERP I/F가 실패일 경우에만 재전송 가능합니다.
                        return false;
                    }
                }
            }
            return true;
        }
        void ConfirmSendCheck(int nstartrow, int nendrow)
        {
            string shopId = string.Empty;
            string calDate = string.Empty;
            string prodId = string.Empty;
            string unit = string.Empty;
            decimal defectQty = 0;
            decimal lossQty = 0;
            decimal lenesceed = 0;
            string slocid = string.Empty;
            string stockValuationType = string.Empty;
            string defectslocid = string.Empty;
            string Lotid = string.Empty;
            string woid = string.Empty;


            // string StckType = string.Empty;

            // 제품단위, 저장위치 재설정
            for (int irow = nstartrow; irow < nendrow; irow++)
            {
                shopId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "SHOPID"));
                prodId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "PRODID"));
                slocid = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "SLOC_ID"));
                //calDate = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "CALDATE"));
                calDate = Util.NVC(Convert.ToDateTime(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "CALDATE")).ToString("yyyyMMdd"));

                SetProdUnit(prodId, irow);
                SetStoreLocation(shopId, slocid, irow);

                //NERP 원부자재일 경우 WOID 자동입력
                string result_Mtrl = ChkMTRL(prodId);
                if (result_Mtrl == "MTRL")
                {
                    SetWorkOrder_send(shopId, prodId, slocid, calDate, irow);
                }

            }

            DataTable dtchk = DataTableConverter.Convert(dgTransfer.ItemsSource);

            for (int nrow = nstartrow; nrow < nendrow; nrow++)
            {
                dtchk.Rows[nrow]["CHK"] = "OK";

                shopId = Util.NVC(dtchk.Rows[nrow]["SHOPID"]);
                //calDate = Util.NVC(dtchk.Rows[nrow]["CALDATE"]);
                if (dtchk.Rows[nrow]["CALDATE"].ToString() == "")
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0116"), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                    return;
                }
                calDate = Util.NVC(Convert.ToDateTime(dtchk.Rows[nrow]["CALDATE"]).ToString("yyyyMMdd"));
                prodId = Util.NVC(dtchk.Rows[nrow]["PRODID"]);
                unit = Util.NVC(dtchk.Rows[nrow]["UNIT_CODE"]);
                defectQty = 0;
                lossQty = Util.NVC_Decimal(dtchk.Rows[nrow]["CNFM_LOSS_QTY2"]);
                lenesceed = Util.NVC_Decimal(dtchk.Rows[nrow]["LENGTH_EXCEED2"]);
                slocid = Util.NVC(dtchk.Rows[nrow]["SLOC_ID"]);
                stockValuationType = Util.NVC(dtchk.Rows[nrow]["STOCK_VALUATION_TYPE"]);
                Lotid = Util.NVC(dtchk.Rows[nrow]["LOTID"]);
                woid = Util.NVC(dtchk.Rows[nrow]["WOID"]);
                //StckType = Util.NVC(dtchk.Rows[nrow]["STCK_TYPE"]);

                defectslocid = null;

                //                if (defectQty != 0 && string.IsNullOrWhiteSpace(defectslocid))
                //                {
                //                    SetDefectStoreLocation(shopId);
                //                }

                if (string.IsNullOrWhiteSpace(shopId))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("플랜트"));   //%1이 입력되지 않았습니다.
                    continue;
                }


                if (string.IsNullOrWhiteSpace(calDate))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("전기일"));   //%1이 입력되지 않았습니다.
                    continue;
                }
                try
                {
                    DateTime date = DateTime.ParseExact(calDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                    if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) < Convert.ToDecimal(date.ToString("yyyyMMdd")))
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1739");    //오늘 이후 날짜는 선택할 수 없습니다.
                        continue;
                    }
                }
                catch
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3241", calDate);    //DATE 형식이 맞지 않습니다. [날짜 형식(YYYYMMDD)이 맞지 않습니다.][DATE: %1]
                    continue;
                }

                if (!ValidateCalDate(calDate))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("100907");    //선택한 전기일로 처리할 수 없습니다.
                    continue;
                }

                if (CloseCalDate(calDate))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3494");    //ERP 생산실적이 마감 되었습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(prodId))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("제품"));   //%1이 입력되지 않았습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(unit))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("단위"));   //%1이 입력되지 않았습니다.
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU9930");   //미사용 제품입니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(slocid))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("저장위치"));   //%1이 입력되지 않았습니다.
                    continue;
                }


                if (defectQty == 0 && lossQty == 0 && lenesceed == 0)
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("수량"));    //%1이 입력되지 않았습니다.
                    continue;
                }

                string result = ValidateSTOCK_VALUATION_PRODUCT(shopId, prodId, stockValuationType);
                if (result != "P")
                {
                    if (result == "M")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값을 입력하시기 바랍니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3808", new object[] { (nrow + 1).ToString(), shopId, prodId });
                        continue;
                    }
                    else if (result == "O")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값은 입력될 수 없습니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3809", new object[] { (nrow + 1).ToString(), shopId, prodId });
                        continue;
                    }
                }

                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("사유"));   //%1이 입력되지 않았습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(txtReqUser.Text) || txtReqUser.Tag == null)
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("요청자"));   //%1이 입력되지 않았습니다.
                    continue;
                }
                //NERP ChkLotid  LOT 필수 입력 여부 확인 및 LOT 관련 체크
                string result_Lot = ChkLotid(Lotid.Trim(), prodId, slocid, shopId);
                if (result_Lot != "P")
                {
                    if (result_Lot == "N")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //LOT과 제품코드가 다릅니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU2907");
                        continue;
                    }
                    else if (result_Lot == "F")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //LOT정보가 없습니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1386");
                        continue;
                    }
                    else if (result_Lot == "C")
                    {
                        //전송대상 LOT이 아닙니다.
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU2090");
                        continue;
                    }
                    else if (result_Lot == "X")
                    {
                        //잘못된 저장위치 입니다.
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU2091");
                        continue;
                    }
                    else if (result_Lot == "E")
                    {
                        //INPUT DATA ERROR
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU4974");
                        continue;
                    }

                }
                //NERP ChkMTRL 원부자재의 경우 WOID 필수 입력
                string result_Mtrl = ChkMTRL(prodId);
                if (result_Mtrl == "MTRL")
                {
                    if (string.IsNullOrWhiteSpace(woid) || woid.Equals("PPLO"))
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1441");
                        continue;
                    }
                }

                dtchk.Rows[nrow]["VERIFICATION_RESULT"] = "OK";
            }

            dtchk.AcceptChanges();
            Util.GridSetData(dgTransfer, dtchk, FrameOperation, false);

        }

        bool CloseCalDate(string caldate)
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("DATE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["DATE"] = caldate;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", IndataTable);

                foreach (DataRow row in dt.Rows)
                {
                    if (string.Equals(Util.NVC(dt.Rows[0]["CLOSING_FLAG"]), "CLOSE"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }
        bool ValidateCalDate(string caldate)
        {
            try
            {
                DateTime outdate;
                if (DateTime.TryParseExact(caldate
                    , "yyyyMMdd"
                    , System.Globalization.CultureInfo.InvariantCulture
                    , System.Globalization.DateTimeStyles.None
                    , out outdate))
                {
                    if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) < Convert.ToDecimal(outdate.ToString("yyyyMMdd")))
                    {
                        //Util.MessageValidation("SFU1739");  //오늘 이후 날짜는 선택할 수 없습니다.
                        return false;
                    }
                }
                else
                {
                    //Util.MessageValidation("SFU3241", caldate);
                    return false;
                }
            }
            catch (Exception ex) { }
            return true;
        }

        string ValidateSTOCK_VALUATION_PRODUCT(string pShopId, string pPordId, string pStockValuationType)
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = pShopId;
                Indata["MTRLID"] = pPordId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_VALUATION_PRODUCT", "RQSTDT", "RSLTDT", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(pStockValuationType) || string.IsNullOrEmpty(pStockValuationType))
                    {
                        return "M"; //필수인데 안 넣음
                    }
                    else
                    {
                        return "P";//통과
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(pStockValuationType) || string.IsNullOrEmpty(pStockValuationType))
                    {
                        return "P"; //통과
                    }
                    else
                    {
                        return "O"; //넣으면 안되는데 넣음
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "F";
            }
        }

        string ChkLotid(string sLotid, string sPordId, string sSlocid, string sShopId)
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("SLOCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = sShopId;
                Indata["SLOCID"] = sSlocid;
                Indata["LOTID"] = sLotid;
                Indata["PRODID"] = sPordId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_TRNF_LOT", "INDATA", "OUTDATA", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    return Util.NVC(dt.Rows[0]["RESULT"]).ToString();
                }

                return "E";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "E";
            }
        }

        string ChkMTRL(string sPordId)
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["MTRLID"] = sPordId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL", "RQSTDT", "RSLTDT", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    return Util.NVC(dt.Rows[0]["MTRLTYPE"]).ToString();
                }

                return "X";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "X";
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr2 = dt.NewRow();
                dr2["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr2["CNFM_LOSS_QTY2"] = 0;
                dr2["LENGTH_EXCEED2"] = 0;
                dt.Rows.Add(dr2);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dg.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion Functions

        #region 요청자
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtReqUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtReqUser.Text;
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
                txtReqUser.Text = wndPerson.USERNAME;
                txtReqUser.Tag = wndPerson.USERID;
            }
        }
        #endregion

        #region 이력 조회
        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearchBoxPrint())
                return;

            GetListHist();
        }

        private bool ValidationSearchBoxPrint()
        {
            DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
            DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

            if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
            {
                //종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU1913");
                return false;
            }
            return true;
        }

        public void GetListHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANG_TXT", typeof(string));
                dtRqst.Columns.Add("PLANT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("MATERIAL", typeof(string));
                dtRqst.Columns.Add("CNCL_FLAG", typeof(string)); // 취소여부
                DataRow dr = dtRqst.NewRow();

                dr["LANG_TXT"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                //dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString().Replace("-", "");
                //dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToShortDateString().Replace("-", "");
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd").Replace("-", ""); // 2024.11.25. 김영국 - 날짜 Type 형식 지정.
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd").Replace("-", ""); // 2024.11.25. 김영국 - 날짜 Type 형식 지정.

                if (cboShop.SelectedIndex > 0)
                {
                    dr["PLANT"] = Convert.ToString(cboShop.SelectedValue);
                }

                if (!string.IsNullOrEmpty(txtProdIDHist.Text))
                    dr["MATERIAL"] = txtProdIDHist.Text.ToUpper();

                if (chkCancel.IsChecked == true)
                    dr["CNCL_FLAG"] = "Y"; // 취소여부 제외

                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_STOCK_ADJUST_HIST", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgListHist, dtRslt, FrameOperation);



                if (CommonVerify.HasTableRow(dtRslt))
                {

                    //Decimal totalOKQty = dtRslt.AsEnumerable().Where(w => w.Field<string>("IF_FLAG").Equals("SUCCESS")).Count(s => s.Field<int>("IF_FLAG") == 1);
                    //Decimal totalNGQty = dtRslt.AsEnumerable().Where(w => w.Field<string>("IF_FLAG").Equals("SUCCESS")).Count(s => s.Field<int>("IF_FLAG") == 1);

                    Decimal totalOKQty = dtRslt.Select("IF_FLAG = 'SUCCESS'").Count();
                    totalOKQty += dtRslt.Select("IF_FLAG = 'SUCCESS_CANCEL'").Count(); //E20240823-001393,  SUCCESS_CANCEL 건수를 성공건수에 포함
                    Decimal totalNGQty = dtRslt.Select("IF_FLAG = 'FAIL'").Count();
                    Decimal totalSENTQty = dtRslt.Rows.Count - (totalOKQty + totalNGQty);
                    Util.MessageValidation("SFU4889", dtRslt.Rows.Count.ToString("#,##0"), totalOKQty.ToString("#,##0"), totalSENTQty.ToString("#,##0"), totalNGQty.ToString("#,##0"));
                    //메시지 출력
                }

                dtHistList = dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region TextBox Event
        private void txtProdIDHist_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtProdIDHist.Text.Trim()))
                {

                }
            }
        }

        #endregion

        private void btnAddWOID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _iRowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                dgTransfer.SelectedIndex = _iRowIndex;
                if (String.IsNullOrEmpty(DataTableConverter.GetValue(dgTransfer.Rows[_iRowIndex].DataItem, "SHOPID").SafeToString()) ||
                    String.IsNullOrEmpty(DataTableConverter.GetValue(dgTransfer.Rows[_iRowIndex].DataItem, "PRODID").SafeToString()) ||
                    String.IsNullOrEmpty(DataTableConverter.GetValue(dgTransfer.Rows[_iRowIndex].DataItem, "SLOC_ID").SafeToString()))
                {
                    //필수입력항목을 모두 입력하십시오.
                    Util.MessageValidation("SFU4979");
                    return;
                }
                //if (DataTableConverter.GetValue(dgTransfer.Rows[_iRowIndex].DataItem, "SLOC_ID").ToString().Length != 4)
                //{
                //    Util.MessageValidation("SFU8232"); // 저장위치 4개 자릿수를 모두 입력 후 다시 시도하십시오.
                //    return;
                //}

                COM001_078_WO wndEmptySlotList = new COM001_078_WO();

                if (wndEmptySlotList != null)
                {
                    DataTable dtTrayLayout = DataTableConverter.Convert(dgTransfer.ItemsSource);
                    object[] Parameters = new object[8];
                    Parameters[0] = Util.NVC(dtTrayLayout.Rows[_iRowIndex]["SHOPID"]);
                    Parameters[1] = Util.NVC(dtTrayLayout.Rows[_iRowIndex]["PRODID"]);
                    Parameters[2] = Util.NVC(dtTrayLayout.Rows[_iRowIndex]["SLOC_ID"]);
                    Parameters[3] = Util.NVC(dtTrayLayout.Rows[_iRowIndex]["WOID"]);
                    Parameters[4] = Util.NVC(dtTrayLayout.Rows[_iRowIndex]["CALDATE"]);

                    C1WindowExtension.SetParameters(wndEmptySlotList, Parameters);

                    wndEmptySlotList.Closed += new EventHandler(wndSETTING_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndEmptySlotList.ShowModal()));
                    wndEmptySlotList.BringToFront();
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void wndSETTING_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_078_WO wndSETTING = sender as COM001_078_WO;
                if (wndSETTING.DialogResult == MessageBoxResult.OK)
                {
                    DataTableConverter.SetValue(dgTransfer.Rows[_iRowIndex].DataItem, "WOID", wndSETTING._pWOID);
                    this.dgTransfer.EndEdit();
                    this.dgTransfer.EndEditRow(true);
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetAreaType()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
                AREATYPE = Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
        }
    }
}