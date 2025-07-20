/*************************************************************************************
 Created Date : 2017.09.27
      Creator : 정규환
   Decription : 이전전기
--------------------------------------------------------------------------------------
 [Change History]
 2023.09.19   조영대    IWorkArea 추가
 2024-07-29   김영택    이력 KEY_LOTID 추가 
 2025-03-03   이민형    KEY_LOTID 를 LOTID 로 변경.
 2025.03.21  오화백 : LOAD 이벤트는 한번만 타도록 수정
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

using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_117 : UserControl, IWorkArea
    {
        private Util _Util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre2 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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
        CheckBox chkAll2 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_117()
        {
            InitializeComponent();
        }

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            this.Loaded -= UserControl_Loaded;
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
                        case "FROM_SHOPID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "FROM_SHOPID")?.ToString().Length > 0)
                                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SLOC_BY_SHOP", new string[] { "SHOPID" }, new string[] { DataTableConverter.GetValue(e.Cell.Row.DataItem, "FROM_SHOPID") as string }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["FROM_SLOC_ID"], "CBO_CODE", "CBO_NAME");
                            break;

                        case "TO_SHOPID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TO_SHOPID")?.ToString().Length > 0)
                                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SLOC_BY_SHOP", new string[] { "SHOPID" }, new string[] { DataTableConverter.GetValue(e.Cell.Row.DataItem, "TO_SHOPID") as string }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["TO_SLOC_ID"], "CBO_CODE", "CBO_NAME");
                            break;
                        case "LOTID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID")?.ToString().Length > 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "LOTID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID").ToString().ToUpper());
                            }
                            break;

                        case "PRODID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString().Length > 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "PRODID", DataTableConverter.GetValue(e.Cell.Row.DataItem,"PRODID").ToString().ToUpper());
                                SetProdUnit(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString());
                            }
                            break;
                        case "FROM_SLOC_ID":
                            if(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FROM_SLOC_ID")?.ToString().Length > 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "FROM_SLOC_ID").ToString().ToUpper());
                            }
                            break;
                        case "TO_SLOC_ID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TO_SLOC_ID")?.ToString().Length > 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "TO_SLOC_ID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "TO_SLOC_ID").ToString().ToUpper());
                            }
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

            string mvType = string.Empty;
            string calDate = string.Empty;
            string Lotid = string.Empty;
            string prodId = string.Empty;
            decimal moveQty = 0;
            string unit = string.Empty;
            string fromShopId = string.Empty;
            string toShopId = string.Empty;
            string fromSlocid = string.Empty;
            string toSlocid = string.Empty;
            string fromPurProdType = string.Empty;
            string toPurProdType = string.Empty;

            // 전송 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
            {
                if (vResult == MessageBoxResult.OK)
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("BUDAT", typeof(string));     // 전기일(YYYYMMDD)
                    inDataTable.Columns.Add("MVTYPE", typeof(string));    // 이동타입 "O : OUT, I : IN, H: HOLD, D:폐기"
                    inDataTable.Columns.Add("MATNR", typeof(string));     // 자재
                    inDataTable.Columns.Add("ERFMG", typeof(decimal));    // 입고수량
                    inDataTable.Columns.Add("ERFME", typeof(string));     // 입력 단위
                    inDataTable.Columns.Add("WERKS", typeof(string));     // 출고플랜트
                    inDataTable.Columns.Add("LGORT", typeof(string));     // 출고저장위치
                    inDataTable.Columns.Add("UMWRK", typeof(string));     // 입고플랜트
                    inDataTable.Columns.Add("UMLGO", typeof(string));     // 입고저장위치
                    inDataTable.Columns.Add("CRRT_NOTE", typeof(string)); // 사유
                    inDataTable.Columns.Add("REQ_USERID", typeof(string));// 요청자
                    inDataTable.Columns.Add("FROM_PUR_PROD_TYPE", typeof(string));
                    inDataTable.Columns.Add("TO_PUR_PROD_TYPE", typeof(string));
                    inDataTable.Columns.Add("LOTID", typeof(string)); //실LOTID


                    for (int nrow = 0; nrow < dgTransfer.Rows.Count; nrow++)
                    {
                        mvType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "MVTYPE") as string;
                        calDate = Convert.ToDateTime(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE")).ToString("yyyyMMdd") as string;
                        prodId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "PRODID") as string;
                        moveQty = Convert.ToDecimal(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "MOVE_QTY"));
                        unit = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "UNIT_CODE") as string;
                        fromShopId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_SHOPID") as string;
                        toShopId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "TO_SHOPID") as string;
                        fromSlocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_SLOC_ID") as string;
                        toSlocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "TO_SLOC_ID") as string;
                        fromPurProdType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_STOCK_VALUATION_TYPE") as string;
                        toPurProdType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "TO_STOCK_VALUATION_TYPE") as string;
                        Lotid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "LOTID") as string;

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["BUDAT"] = calDate;
                        inDataRow["MVTYPE"] = mvType;
                        if (string.IsNullOrEmpty(prodId))
                        {
                            inDataRow["MATNR"] = prodId;
                        }
                        else
                        {
                            inDataRow["MATNR"] = prodId.ToUpper();
                        }
                        inDataRow["ERFMG"] = moveQty;
                        inDataRow["ERFME"] = unit;
                        inDataRow["WERKS"] = fromShopId;
                        inDataRow["LGORT"] = fromSlocid;
                        if (string.IsNullOrEmpty(toShopId))
                        {
                            inDataRow["UMWRK"] = toShopId;
                        }
                        else
                        {
                            inDataRow["UMWRK"] = toShopId.ToUpper();
                        }
                        if (string.IsNullOrEmpty(toSlocid))
                        {
                            inDataRow["UMLGO"] = toSlocid;
                        }
                        else
                        {
                            inDataRow["UMLGO"] = toSlocid.ToUpper();
                        }
                        inDataRow["CRRT_NOTE"] = txtNote.Text;
                        inDataRow["REQ_USERID"] = txtReqUser.Tag;
                        inDataRow["FROM_PUR_PROD_TYPE"] = fromPurProdType;
                        inDataRow["TO_PUR_PROD_TYPE"] = toPurProdType;

                        if (string.IsNullOrEmpty(Lotid))
                        {
                            inDataRow["LOTID"] = Lotid;
                        }
                        else
                        {
                            inDataRow["LOTID"] = Lotid.ToUpper();
                        }

                       // inDataRow["LOTID"] = Lotid.ToUpper();

                        inDataTable.Rows.Add(inDataRow);
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_TRANSFER_POSTING", "INDATA", null, (result, ex) =>
                    {
                        try
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_STOCK_TRANSFER_POSTING", ex.Message, ex.ToString());
                                return;
                            }

                            // 전송 완료 되었습니다.
                            Util.MessageInfo("SFU1880");

                            Init();
                        }
                        catch (Exception ex1)
                        {
                            Util.MessageException(ex1);
                        }
                        finally
                        {
                        }
                    }, inDataSet);

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

        #endregion

        #region Functions
        void Init()
        {
            dgTransfer.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("MVTYPE", typeof(string));
            emptyTransferTable.Columns.Add("CALDATE", typeof(string));
            emptyTransferTable.Columns.Add("LOTID", typeof(string));
            emptyTransferTable.Columns.Add("PRODID", typeof(string));
            emptyTransferTable.Columns.Add("MOVE_QTY", typeof(decimal));
            emptyTransferTable.Columns.Add("UNIT_CODE", typeof(string));
            emptyTransferTable.Columns.Add("FROM_SHOPID", typeof(string));
            emptyTransferTable.Columns.Add("FROM_SLOC_ID", typeof(string));
            emptyTransferTable.Columns.Add("TO_SHOPID", typeof(string));
            emptyTransferTable.Columns.Add("TO_SLOC_ID", typeof(string));
            emptyTransferTable.Columns.Add("CHK", typeof(string));
            emptyTransferTable.Columns.Add("FROM_STOCK_VALUATION_TYPE", typeof(string));
            emptyTransferTable.Columns.Add("TO_STOCK_VALUATION_TYPE", typeof(string));
            emptyTransferTable.Columns.Add("VERIFICATION_RESULT", typeof(string));

            dgTransfer.ItemsSource = DataTableConverter.Convert(emptyTransferTable);

            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SHOP_BY_COMPANY_DVZN_CODE_CBO", new string[] { "LANGID", "SHOPID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["FROM_SHOPID"], "CBO_CODE", "CBO_NAME");
            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SHOP_BY_COMPANY_DVZN_CODE_CBO", new string[] { "LANGID", "SHOPID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["TO_SHOPID"], "CBO_CODE", "CBO_NAME");
            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_ERP_MOVE_TYPE_CBO", new string[] { "LANGID" }, new string[] { LoginInfo.LANGID }, CommonCombo.ComboStatus.NONE, dgTransfer.Columns["MVTYPE"], "CBO_CODE", "CBO_NAME");

            (dgTransfer.Columns["FROM_STOCK_VALUATION_TYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtSTOCK_VALUATION_TYPECombo("STOCK_VALUATION_TYPE"));
            (dgTransfer.Columns["TO_STOCK_VALUATION_TYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtSTOCK_VALUATION_TYPECombo("STOCK_VALUATION_TYPE"));
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

        void SetProdUnit(string prodId)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("MTRLID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["MTRLID"] = prodId;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_MATERIAL", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
                DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "UNIT_CODE", "");
            else
                DataTableConverter.SetValue(dgTransfer.Rows[dgTransfer.CurrentCell.Row.Index].DataItem, "UNIT_CODE", Util.NVC(dt.Rows[0]["MTRLUNIT"]));

        }

        void SetStoreLocation(String shopid, string slocid, string gubun, int iRow)
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
                if (gubun == "from")
                    DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "FROM_SLOC_ID", "");
                else
                    DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "TO_SLOC_ID", "");
            }
            else
            {
                if (gubun == "from")
                    DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "FROM_SLOC_ID", slocid);
                else
                    DataTableConverter.SetValue(dgTransfer.Rows[iRow].DataItem, "TO_SLOC_ID", slocid);
            }
        }

        bool ConfirmSend()
        {
            string mvType = string.Empty;
            string calDate = string.Empty;
            string Lotid = string.Empty;
            string prodId = string.Empty;
            decimal moveQty = 0;
            string unit = string.Empty;
            string fromShopId = string.Empty;
            string fromSlocid = string.Empty;
            string toShopId = string.Empty;
            string toSlocid = string.Empty;
            string fromStockValuationType = string.Empty;
            string toStockValuationType = string.Empty;

            if (dgTransfer.Rows.Count == 0)
                return false;

            for (int nrow = 0; nrow < dgTransfer.Rows.Count; nrow++)
            {
                mvType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "MVTYPE") as string;
                //calDate = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE") as string;
                calDate = Convert.ToDateTime(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "CALDATE")).ToString("yyyyMMdd") as string;
                Lotid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "LOTID") as string;
                prodId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "PRODID") as string;
                moveQty = Convert.ToDecimal(DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "MOVE_QTY"));
                unit = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "UNIT_CODE") as string;
                fromShopId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_SHOPID") as string;
                fromSlocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_SLOC_ID") as string;
                toShopId = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "TO_SHOPID") as string;
                toSlocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "TO_SLOC_ID") as string;
                fromStockValuationType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_STOCK_VALUATION_TYPE") as string;
                toStockValuationType = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "TO_STOCK_VALUATION_TYPE") as string;

                //출고SLOC_ID 재설정
                SetStoreLocation(fromShopId, fromSlocid, "from", nrow);
                fromSlocid = DataTableConverter.GetValue(dgTransfer.Rows[nrow].DataItem, "FROM_SLOC_ID") as string;

                // 이동구분을 선택해 주세요.
                if (string.IsNullOrEmpty(mvType))
                {
                    Util.MessageValidation("SFU3702", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("이동구분"));
                    return false;
                }

                // {0}이 입력되지 않았습니다.
                if (string.IsNullOrWhiteSpace(fromShopId))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("From플랜트"));
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
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("단위"));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(fromSlocid))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("From저장위치"));
                    return false;
                }
                //if (string.IsNullOrWhiteSpace(toSlocid))
                //{
                //    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("To저장위치"));
                //    return false;
                //}
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

                if (moveQty == 0)
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("수량"));
                    return false;
                }

                if(mvType != "H" && mvType != "G")
                {
                    if (string.IsNullOrWhiteSpace(toShopId))
                    {
                        Util.MessageValidation("SFU3810", (nrow + 1).ToString());  //%1 열 입고플랜트는 필수 입니다.
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(toSlocid))
                    {
                        Util.MessageValidation("SFU3811", (nrow + 1).ToString());  //%1 열 입고저장위치는 필수 입니다.
                        return false;
                    }
                }

                string fromResult = ValidateSTOCK_VALUATION_PRODUCT(fromShopId, prodId, fromStockValuationType);
                if (fromResult != "P")
                {
                    if (fromResult == "M")
                    {
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값을 입력하시기 바랍니다.
                        Util.MessageValidation("SFU3808", new object[] { (nrow + 1).ToString(), fromShopId, prodId });
                    }
                    else if (fromResult == "O")
                    {
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값은 입력될 수 없습니다.
                        Util.MessageValidation("SFU3809", new object[] { (nrow + 1).ToString(), fromShopId, prodId });
                    }

                    return false;
                }

                string toResult = ValidateSTOCK_VALUATION_PRODUCT(toShopId, prodId, toStockValuationType);
                if (toResult != "P")
                {
                    if (toResult == "M")
                    {
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값을 입력하시기 바랍니다.
                        Util.MessageValidation("SFU3808", new object[] { (nrow + 1).ToString(), toShopId, prodId });
                    }
                    else if (toResult == "O")
                    {
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값은 입력될 수 없습니다.
                        Util.MessageValidation("SFU3809", new object[] { (nrow + 1).ToString(), toShopId, prodId });
                    }

                    return false;
                }
                //NERP
                string result_Lot = ChkLotid(Lotid, prodId, fromSlocid, fromShopId);
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
                        //잘못된 저장위치 입니다.
                        Util.MessageValidation("SFU2091");
                    }

                    return false;
                }
            }

            return true;
        }

        bool ConfirmCancel()
        {
            for (int nrow = 0; nrow < dgListHist.Rows.Count; nrow++)
            {
                if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK").Equals("True"))
                {
                    if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "IF_FLAG") == null ||
                       !DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "IF_FLAG").Equals("SUCCESS"))
                    {
                        Util.MessageValidation("SFU8247", (nrow + 1).ToString() + "row (" + Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "MATNR")) + ")");  // [%1] ERP I/F가 성공일 경우에만 취소 가능합니다.
                        return false;
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CNCL_FLAG")), "Y"))
                    {
                        Util.MessageValidation("SFU8248", (nrow + 1).ToString() + "row (" + Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "MATNR")) + ")");  // [%1] 이미 취소처리 되었습니다.
                        return false;
                    }

                    string calDate = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "BUDAT_CONVERT")).Replace("-", "").Substring(0, 8);
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
                if (DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "CHK").Equals("True"))
                {
                    if (DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "IF_FLAG") == null ||
                        !DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "IF_FLAG").Equals("FAIL"))
                    {
                        Util.MessageValidation("SFU8246", (inx + 1).ToString() + "row (" + Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[inx].DataItem, "MATNR")) + ")"); // [%1] ERP I/F가 실패일 경우에만 재전송 가능합니다.
                        return false;
                    }
                }
            }
            return true;
        }

        void ConfirmSendCheck(int nstartrow, int nendrow)
        {
            string mvType = string.Empty;
            string calDate = string.Empty;
            string prodId = string.Empty;
            string Lotid = string.Empty;
            decimal moveQty = 0;
            string unit = string.Empty;
            string fromShopId = string.Empty;
            string fromSlocid = string.Empty;
            string toShopId = string.Empty;
            string toSlocid = string.Empty;
            string fromStockValuationType = string.Empty;
            string toStockValuationType = string.Empty;

            // 제품단위, 저장위치 재설정
            for (int irow = nstartrow; irow < nendrow; irow++)
            {
                fromShopId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "FROM_SHOPID"));
                Lotid = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "LOTID"));
                prodId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "PRODID"));
                fromSlocid = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "FROM_SLOC_ID"));
                toShopId = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "TO_SHOPID"));
                toSlocid = Util.NVC(DataTableConverter.GetValue(dgTransfer.Rows[irow].DataItem, "TO_SLOC_ID"));

                SetProdUnit(prodId);
                SetStoreLocation(fromShopId, fromSlocid, "from", irow);
                //SetStoreLocation(toShopId, toSlocid, "to", irow);
            }

            DataTable dtchk = DataTableConverter.Convert(dgTransfer.ItemsSource);

            for (int nrow = nstartrow; nrow < nendrow; nrow++)
            {
                dtchk.Rows[nrow]["CHK"] = "OK";

                mvType = Util.NVC(dtchk.Rows[nrow]["MVTYPE"]);
                //calDate = Util.NVC(dtchk.Rows[nrow]["CALDATE"]);
                if(dtchk.Rows[nrow]["CALDATE"].ToString() == "")
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0116"), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                    return;
                }

                calDate = Util.NVC(Convert.ToDateTime(dtchk.Rows[nrow]["CALDATE"]).ToString("yyyyMMdd"));
                Lotid = Util.NVC(dtchk.Rows[nrow]["LOTID"]);
                prodId = Util.NVC(dtchk.Rows[nrow]["PRODID"]);
                moveQty = Util.NVC_Decimal(dtchk.Rows[nrow]["MOVE_QTY"]);
                unit = Util.NVC(dtchk.Rows[nrow]["UNIT_CODE"]);
                fromShopId = Util.NVC(dtchk.Rows[nrow]["FROM_SHOPID"]);
                fromSlocid = Util.NVC(dtchk.Rows[nrow]["FROM_SLOC_ID"]);
                toShopId = Util.NVC(dtchk.Rows[nrow]["TO_SHOPID"]);
                toSlocid = Util.NVC(dtchk.Rows[nrow]["TO_SLOC_ID"]);
                fromStockValuationType = Util.NVC(dtchk.Rows[nrow]["FROM_STOCK_VALUATION_TYPE"]);
                toStockValuationType = Util.NVC(dtchk.Rows[nrow]["TO_STOCK_VALUATION_TYPE"]);

                if (string.IsNullOrEmpty(mvType))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3702", ObjectDic.Instance.GetObjectName("이동구분"));   //이동구분을 선택해 주세요.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fromShopId))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("From플랜트"));  //%1이 입력되지 않았습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(calDate))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("전기일"));  //%1이 입력되지 않았습니다.
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
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("제품"));  //%1이 입력되지 않았습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(unit))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("단위"));  //%1이 입력되지 않았습니다.
                    continue;
                }



                if (string.IsNullOrWhiteSpace(fromSlocid))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("From저장위치"));  //%1이 입력되지 않았습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("사유"));   //%1이 입력되지 않았습니다.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(txtReqUser.Text) || txtReqUser.Tag == "")
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("요청자"));   //%1이 입력되지 않았습니다.
                    continue;
                }


                if (moveQty == 0)
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("수량"));  //%1이 입력되지 않았습니다.
                    continue;
                }

                if (mvType != "H" && mvType != "G")
                {
                    if (string.IsNullOrWhiteSpace(toShopId))
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3810", (nrow + 1).ToString());  //%1 열 입고플랜트는 필수 입니다.
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(toSlocid))
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3811", (nrow + 1).ToString());  //%1 열 입고저장위치는 필수 입니다.
                        continue;
                    }
                }

                string fromResult = ValidateSTOCK_VALUATION_PRODUCT(fromShopId, prodId, fromStockValuationType);
                if (fromResult != "P")
                {
                    if (fromResult == "M")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값을 입력하시기 바랍니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3808", new object[] { (nrow + 1).ToString(), fromShopId, prodId });
                        continue;
                    }
                    else if (fromResult == "O")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값은 입력될 수 없습니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3809", new object[] { (nrow + 1).ToString(), fromShopId, prodId });
                        continue;
                    }
                }

                string toResult = ValidateSTOCK_VALUATION_PRODUCT(toShopId, prodId, toStockValuationType);
                if (toResult != "P")
                {
                    if (toResult == "M")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값을 입력하시기 바랍니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3808", new object[] { (nrow + 1).ToString(), toShopId, prodId });
                        continue;
                    }
                    else if (toResult == "O")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //%1 열 %2 플랜트 %3 제품의 재고평가유형값은 입력될 수 없습니다.
                        dtchk.Rows[nrow]["VERIFICATION_RESULT"] = MessageDic.Instance.GetMessage("SFU3809", new object[] { (nrow + 1).ToString(), toShopId, prodId });
                        continue;
                    }
                }

                //NERP
                string result_Lot = ChkLotid(Lotid, prodId, fromSlocid, fromShopId);
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

                dtchk.Rows[nrow]["VERIFICATION_RESULT"] = null;
            }

            dtchk.AcceptChanges();
            Util.GridSetData(dgTransfer, dtchk, FrameOperation,false);

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
                dr2["MOVE_QTY"] = 0;
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
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("MATNR", typeof(string));
                dtRqst.Columns.Add("CNCL_FLAG", typeof(string));  // 취소여부

                DataRow dr = dtRqst.NewRow();

                dr["LANG_TXT"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                //dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString("yyyyMMdd").Replace("-", "");
                //dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToShortDateString("yyyyMMdd").Replace("-", "");
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd").Replace("-", ""); // 2024.11.25. 김영국 - 날짜 Type 형식 지정.
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd").Replace("-", ""); // 2024.11.25. 김영국 - 날짜 Type 형식 지정.
                if (!string.IsNullOrEmpty(txtProdIDHist.Text))
                    dr["MATNR"] = txtProdIDHist.Text.ToUpper();
                if (chkCancel.IsChecked == true)
                    dr["CNCL_FLAG"] = "Y";  // 취소여부

                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_STOCK_TRANSFER_POSTING_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
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
                //int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                //if (DataTableConverter.GetValue(dgListHist.Rows[row_index].DataItem, "CHK").Equals("True"))
                //{
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

        private void btnReSend_Click(object sender, RoutedEventArgs e)
        {
            //if (ConfirmSend() == false)
            //{
            //    return;
            //}
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

                        if (chk == "True")
                        {
                            DataRow inDataRow = inDataTable.NewRow();
                            inDataRow["ERP_TRNF_SEQNO"] = erp_trnf_seqno;

                            inDataTable.Rows.Add(inDataRow);

                            chk_cnt++;
                        }
                    }

                    if (chk_cnt > 0)
                    {                        
                        try
                        {
                            new ClientProxy().ExecuteServiceSync("BR_ACT_REG_RESEND_TRSF_POST", "INDATA", null, inDataTable);                           

                            Util.MessageInfo("SFU1880"); // 전송 완료 되었습니다.

                            Init();
                            GetListHist();
                        }
                        catch(Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                    else
                    {
                        // 전송 완료 되었습니다.
                        Util.MessageInfo("SFU1636"); // 선택된 대상이 없습니다.
                    }
                }
            }, false, false, string.Empty);
        }

        private void CalDate_SelectedChange(object sender, SelectionChangedEventArgs e)
        {
            DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;
            if (drv == null) return;
            
        }

        private void btnCancelSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConfirmCancel() == false)
                {
                    return;
                }

                string lotid = string.Empty;
                string crr_note = string.Empty;
                string chk = string.Empty;
                int chk_cnt = 0;

                decimal wipseq;
                string actdttm = string.Empty;

                // 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1243"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {

                        DataTable inDataTable = new DataTable("INDATA");
                        inDataTable.Columns.Add("LOTID", typeof(string));
                        inDataTable.Columns.Add("NOTE", typeof(string));
                        inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
                        inDataTable.Columns.Add("ACTDTTM", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        for (int nrow = 0; nrow < dgListHist.GetRowCount(); nrow++)
                        {
                            chk = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK") as string;
                            //lotid = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "LOTID") as string;
                            //lotid = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "ERP_TRNF_SEQNO") as string;
                        
                            lotid = DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "KEY_LOTID") as string; // 2024-07-29: KEY_LOTID 로 변경   
                            wipseq = Util.NVC_Decimal(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "WIPSEQ"));
                            actdttm = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "ACTDTTM"));
                            crr_note = Util.NVC(DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CRRT_NOTE"));

                            if (DataTableConverter.GetValue(dgListHist.Rows[nrow].DataItem, "CHK").IsTrue())
                            {
                                DataRow inDataRow = inDataTable.NewRow();
                                inDataRow["LOTID"] = lotid;
                                inDataRow["NOTE"] = crr_note + "[" + Util.NVC(txtCancelNote.Text) + "]";
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
                                //new ClientProxy().ExecuteServiceSync("BR_ACT_REG_CANCEL_TRSF_POST", "INDATA", null, inDataTable);
                                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_STOCK_TRANSFER_POSTING", "INDATA", null, inDataTable);

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

    }
}
