using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_092_HOLD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_092_HOLD : C1Window, IWorkArea
    {
        #region Initialize
        private Util _Util = new Util();

        public IFrameOperation FrameOperation { get; set; }

        public COM001_092_HOLD()
        {
            InitializeComponent();
            InitCombo();
        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            GetStockLotAbNormal();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
        }
        #endregion

        #region Event
        private void btnHold_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string sHoldCode = Util.GetCondition(cboHoldType, "SFU1342"); //"사유를 선택하세요" >> HOLD 사유를 선택 하세요.
            if (string.IsNullOrEmpty(sHoldCode))
                return;
            
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                    LotHold(sHoldCode);
            });
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
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
                        Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
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
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }
        #endregion
        #region Method Function
        private void GetStockLotAbNormal()
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_IN_ABNORMAL_LOT", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                        throw searchException;

                    Util.GridSetData(dgAnormal, result, FrameOperation, false);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void LotHold(string sHoldCode)
        {
            try
            {
                DataSet inData = new DataSet();

                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTION_USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["LANGID"] = LoginInfo.LANGID;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["ACTION_USERID"] = Util.NVC(txtPersonId.Text);
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOLD_NOTE", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("HOLD_CODE", typeof(string));
                inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                DataTable dt = ((DataView)dgAnormal.ItemsSource).Table;
                foreach (DataRow dataRow in dt.Rows)
                {
                    if (Convert.ToBoolean(dataRow["CHK"]) == true)
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(dataRow["LOTID"]);
                        row["HOLD_NOTE"] = Util.GetCondition(txtHold);
                        row["RESNCODE"] = sHoldCode;
                        row["HOLD_CODE"] = sHoldCode;
                        row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);
                        inLot.Rows.Add(row);
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.MessageInfo("SFU1344");    //HOLD 완료

                        if (chkPrint.IsChecked == true)
                            PrintHoldLabel();

                        GetStockLotAbNormal();
                        cboHoldType.SelectedIndex = 0;
                        dtExpected.SelectedDateTime = DateTime.Now;
                        txtHold.Text = "";
                        txtPerson.Text = "";
                        txtPersonId.Text = "";
                        txtPersonDept.Text = "";
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }, inData);
            }
            catch( Exception ex) { Util.MessageException(ex); }
        }
        #endregion
        #region Hold Label
        private void PrintHoldLabel()
        {
            try
            {
                DataTable dtLabel = new DataTable();
                dtLabel.Columns.Add("LOTID", typeof(string));
                dtLabel.Columns.Add("RESNNAME", typeof(string));
                dtLabel.Columns.Add("MODELID", typeof(string));
                dtLabel.Columns.Add("WIPQTY", typeof(string));
                dtLabel.Columns.Add("PERSON", typeof(string));

                DataRow inRow = null;
                DataTable dt = ((DataView)dgAnormal.ItemsSource).Table;
                foreach (DataRow dataRow in dt.Rows)
                {
                    if (Convert.ToBoolean(dataRow["CHK"]) == true)
                    {
                        inRow = dtLabel.NewRow();
                        inRow["LOTID"] = Util.NVC(dataRow["LOTID"]);
                        inRow["RESNNAME"] = Util.NVC(cboHoldType.Text);
                        inRow["MODELID"] = Util.NVC(DataTableConverter.GetValue(dataRow, "MODLID"));
                        inRow["WIPQTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dataRow, "WIPQTY"));
                        inRow["PERSON"] = Util.NVC(txtPerson.Text);
                        dtLabel.Rows.Add(inRow);
                    }
                }

                DataTable printDt = LoginInfo.CFG_SERIAL_PRINT;

                string startX = "0";
                string startY = "0";
                if (printDt != null && printDt.Rows.Count > 0)
                {
                    startX = printDt.Rows[0]["X"].ToString();
                    startY = printDt.Rows[0]["Y"].ToString();
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                for (int i = 0; i < dtLabel.Rows.Count; i++)
                {
                    dtRqst.Rows[0]["LBCD"] = "LBL0013";
                    dtRqst.Rows[0]["PRMK"] = "Z";
                    dtRqst.Rows[0]["RESO"] = "203";
                    dtRqst.Rows[0]["PRCN"] = "1";
                    dtRqst.Rows[0]["MARH"] = startX;
                    dtRqst.Rows[0]["MARV"] = startY;
                    dtRqst.Rows[0]["ATTVAL001"] = dtLabel.Rows[i]["MODELID"].ToString();
                    dtRqst.Rows[0]["ATTVAL002"] = dtLabel.Rows[i]["LOTID"].ToString();
                    dtRqst.Rows[0]["ATTVAL003"] = dtLabel.Rows[i]["WIPQTY"].ToString();
                    dtRqst.Rows[0]["ATTVAL004"] = dtLabel.Rows[i]["RESNNAME"].ToString();
                    dtRqst.Rows[0]["ATTVAL005"] = DateTime.Now.ToString("yyyy.MM.dd");
                    dtRqst.Rows[0]["ATTVAL006"] = dtExpected.SelectedDateTime.ToString("yyyy.MM.dd");
                    dtRqst.Rows[0]["ATTVAL007"] = dtLabel.Rows[i]["PERSON"].ToString();
                    dtRqst.Rows[0]["ATTVAL008"] = "";
                    dtRqst.Rows[0]["ATTVAL009"] = "";

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    try
                    {
                        // 프린터 정보 조회
                        string sPrt = string.Empty;
                        string sRes = string.Empty;
                        string sCopy = string.Empty;
                        string sXpos = string.Empty;
                        string sYpos = string.Empty;
                        string sDark = string.Empty;
                        DataRow drPrtInfo = null;

                        if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                            return;

                        if (PrintLabel(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo) == false)
                        {
                            Util.MessageValidation("SFU1309"); //Barcode Print 실패.
                            return;
                        }

                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo == null || drPrtInfo.Table == null)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030")); //프린터 환경설정 정보가 없습니다.

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().ToUpper().IndexOf("COM") >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    Util.MessageValidation("SFU3031"); //프린터 환경설정에 포트명 항목이 없습니다.
                }
            }
            else
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031")); // 프린터 환경설정에 포트명 항목이 없습니다.

                Util.MessageValidation("SFU3031"); // 프린터 환경설정에 포트명 항목이 없습니다.
            }
            return brtndefault;
        }
        #endregion
    }
}
