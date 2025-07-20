/*************************************************************************************
 Created Date : 2023.04.18
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.04.18  이병윤 : Initial Created.(E20230328-000742)
  2024.05.14  오수현 : E20240312-000577 : Data Matrix Code Sequence 변경, Grid에 PRODUCTION_DATE 항목 추가, 2D BCR 부분 링크 제거 및 수정
                                       : tbP -> tbBP로 변경. biz에 SHOPID 추가




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Globalization;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_220 : UserControl, IWorkArea
    {
        Util _util = new Util();

        string _sPGM_ID = "BOX001_220";

        #region Declaration & Constructor 
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


        public BOX001_220()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeCombo();
            SetEvents();

        }
        private void SetEvents()
        {
            /*
                txt12S.MouseEnter += BcrTextMouseEnter;
                txtP.MouseEnter += BcrTextMouseEnter;
                txt1P.MouseEnter += BcrTextMouseEnter;
                txt31P.MouseEnter += BcrTextMouseEnter;
                txt12V.MouseEnter += BcrTextMouseEnter;
                txt10V.MouseEnter += BcrTextMouseEnter;
                txt2P.MouseEnter += BcrTextMouseEnter;
                txt20P.MouseEnter += BcrTextMouseEnter;
                txt6D.MouseEnter += BcrTextMouseEnter;
                txt14D.MouseEnter += BcrTextMouseEnter;
                txt30P.MouseEnter += BcrTextMouseEnter;
                txtZ.MouseEnter += BcrTextMouseEnter;
                txtK.MouseEnter += BcrTextMouseEnter;
                txt16K.MouseEnter += BcrTextMouseEnter;
                txtV.MouseEnter += BcrTextMouseEnter;
                txt3S.MouseEnter += BcrTextMouseEnter;
                txtQ.MouseEnter += BcrTextMouseEnter;
                txt20T.MouseEnter += BcrTextMouseEnter;
                txt1T.MouseEnter += BcrTextMouseEnter;
                txt2T.MouseEnter += BcrTextMouseEnter;
                txt1Z.MouseEnter += BcrTextMouseEnter;

                txt12S.MouseLeave += BcrTextMouseLeave;
                txtP.MouseLeave += BcrTextMouseLeave;
                txt1P.MouseLeave += BcrTextMouseLeave;
                txt31P.MouseLeave += BcrTextMouseLeave;
                txt12V.MouseLeave += BcrTextMouseLeave;
                txt10V.MouseLeave += BcrTextMouseLeave;
                txt2P.MouseLeave += BcrTextMouseLeave;
                txt20P.MouseLeave += BcrTextMouseLeave;
                txt6D.MouseLeave += BcrTextMouseLeave;
                txt14D.MouseLeave += BcrTextMouseLeave;
                txt30P.MouseLeave += BcrTextMouseLeave;
                txtZ.MouseLeave += BcrTextMouseLeave;
                txtK.MouseLeave += BcrTextMouseLeave;
                txt16K.MouseLeave += BcrTextMouseLeave;
                txtV.MouseLeave += BcrTextMouseLeave;
                txt3S.MouseLeave += BcrTextMouseLeave;
                txtQ.MouseLeave += BcrTextMouseLeave;
                txt20T.MouseLeave += BcrTextMouseLeave;
                txt1T.MouseLeave += BcrTextMouseLeave;
                txt2T.MouseLeave += BcrTextMouseLeave;
                txt1Z.MouseLeave += BcrTextMouseLeave;
                */
        }
        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: new C1ComboBox[] { cboLine }, sCase: "ALLAREA");
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea }, cbChild: new C1ComboBox[] { cboEqpt }, sCase: "LINE_FCS");
            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { Process.CELL_BOXING }, sCase: "EQUIPMENT");

            //_combo.SetCombo(cboBoxType, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "BOXTYPE_NJ" }, sCase: "COMMCODE_WITHOUT_CODE");
        }
        #endregion


        #region Methods

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private void GetPallet()
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("BOXTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    dr["BOXID"] = txtPalletID.Text;
                }
                else
                {
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) ? null : cboArea.SelectedValue;
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtLotID.Text) ? null : txtLotID.Text;
                    dr["BOXTYPE"] = "SHIP_PLT";
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKED_PALLET_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgPallet, dtResult, FrameOperation, true);
                if (dgPallet.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU2816");
                }
                GetOutBoxList();
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
        private void GetOutBoxList()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");
                if (idxPallet < 0)
                {
                    dgOutboxList.ItemsSource = null;
                    return;
                }


                string sPallet = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);
                string sAreaID = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["AREAID"].Index).Value);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("LANGID");
                DataRow dr = inDataTable.NewRow();
                dr["BOXID"] = sPallet;
                dr["AREAID"] = sAreaID;
                dr["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKED_ASSY_LOT_HIST", "RQSTDT", "RSLTDT", inDataTable);
                if (dtResult != null)
                {
                    if (!dtResult.Columns.Contains("CHK"))
                        dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                    Util.GridSetData(dgOutboxList, dtResult, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private void PrintMatLabel(string BoxID, string batch1, string batch2, string quantity, string dataCode, string expData, string productionDate, string areaId, string copyCnt)
        {
            string zplCode = string.Empty;
            string lblCode = string.Empty;
            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty; // 발행수량
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;

            if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            try
            {

                string sBizRule = "BR_PRD_GET_BOSCH_LABEL_ITEM";

                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("BOXID");
                inData.Columns.Add("BATCH_NO1"); // BATCH_NUMBER
                inData.Columns.Add("BATCH_NO2"); // CELL_RANKING
                inData.Columns.Add("QUANTITY");
                inData.Columns.Add("DATE_CODE");
                inData.Columns.Add("EXP_DATE");
                inData.Columns.Add("PRODUCTION_DATE");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("SHOPID");
                inData.Columns.Add("AREAID");
                inData.Columns.Add("USERID");
                inData.Columns.Add("LANGID");
                inData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용
                DataRow inDataRow = inData.NewRow();

                inDataRow["BOXID"] = BoxID;
                inDataRow["BATCH_NO1"] = Util.NVC(batch1);
                inDataRow["BATCH_NO2"] = Util.NVC(batch2);
                inDataRow["QUANTITY"] = Util.NVC(quantity);
                inDataRow["DATE_CODE"] = Util.NVC(dataCode);
                inDataRow["EXP_DATE"] = Util.NVC(expData);
                inDataRow["PRODUCTION_DATE"] = Util.NVC(productionDate);
                inDataRow["SHIPTO_ID"] = Util.NVC("M1008"); // BOSH
                inDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataRow["AREAID"] = areaId;
                inDataRow["USERID"] = txtWorker.Tag;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["PGM_ID"] = _sPGM_ID;
                inDataRow["BZRULE_ID"] = sBizRule;
                inData.Rows.Add(inDataRow);

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");
                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                // outbox 수량을 발행수로 
                inPrintDr["PRCN"] = String.IsNullOrEmpty(copyCnt) ? sCopy : copyCnt;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT", "OUTITEM,OUTZPL", inDataSet, null);

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if ((resultSet.Tables.IndexOf("OUTZPL") > -1) && resultSet.Tables["OUTZPL"].Rows.Count > 0)
                    {
                        zplCode = resultSet.Tables["OUTZPL"].Rows[0]["LABELCD"].ToString();
                        if (zplCode.Split(',')[0].Equals("1"))
                        {
                            ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return;
                        }
                        else
                        {
                            zplCode = zplCode.Substring(2);
                        }

                        zplCode = zplCode.Replace("^GFA", "^GFA,01660,01660,00020," +
                        "00, 00000000000000000C, 000000000000000033, 0000000000000000C0C0, 00000000000000010020, 00000000000000060018, 00000000000000180006, 00000000000000200001, 0000000000"
                        + "0000C00000C0, 0000000000000300000030, 0000000000000C0000000C, 0000000000001000000002, 000000000000600000000180, 000000000001800000000060, 000000000002000000000010, 00"
                        + "000000000C00000000000C, 000000000030000000000003, 00000000004000000000000080, 00000000018000000000000060, 00000000060000000000000018, 00000000080000000000000004, 00"
                        + "000000300000000000000003, 00000000C00000000000000000C0, 0000000100000000000000000020, 0000000600000000000000000018, 0000001800000000000000000006, 0000002000000000"
                        + "000000000001, 000000C000000000000000000000C0, 000003000000000000000000000030, 000004000000000000000000000008, 000018000000000000000000000006, 00006000000000000000"
                        + "000000000180, 00018000000000000000000000000060, 00020000000000000000000000000010, 000C000000000000000000000000000C, 00300000000000000000000000000003, 004000000000"
                        + "0000000000000000000080, 0180000000000000000000000000000060, 0600000000000000000000000000000018, 0800000000000000000000000000000004, 3000000000000000000000000000"
                        + "000003, 400000000000000000000000000000000080, 3000000000000000000000000000000003, 0800000000000000000000000000000004, 0600000000000000000000000000000018, 01800000"
                        + "00000000000000000000000060, 0040000000000000000000000000000080, 00300000000000000000000000000003, 000C000000000000000000000000000C, 0002000000000000000000000000"
                        + "0010, 00018000000000000000000000000060, 00006000000000000000000000000180, 000018000000000000000000000006, 000004000000000000000000000008, 000003000000000000000000"
                        + "000030, 000000C000000000000000000000C0, 0000002000000000000000000001, 0000001800000000000000000006, 0000000600000000000000000018, 0000000100000000000000000020, 0000"
                        + "0000C00000000000000000C0, 00000000300000000000000003, 00000000080000000000000004, 00000000060000000000000018, 00000000018000000000000060, 000000000040000000000000"
                        + "80, 000000000030000000000003, 00000000000C00000000000C, 000000000002000000000010, 000000000001800000000060, 000000000000600000000180, 0000000000001000000002, 00000000"
                        + "00000C0000000C, 0000000000000300000030, 00000000000000C00000C0, 00000000000000200001, 00000000000000180006, 00000000000000060018, 00000000000000010020, 000000000000"
                        + "0000C0C0, 000000000000000033, 00000000000000000C, 00, ");
                    }

                }
                PrintLabel(zplCode, drPrtInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMatLabelPreview(DependencyObject obj, DataTable dt)
        {
            try
            {
                Label lb = obj as Label;
                TextBox tBox = obj as TextBox;
                TextBlock tBlock = obj as TextBlock;
                C1.WPF.BarCode.C1BarCode bcd = obj as C1.WPF.BarCode.C1BarCode;

                if (lb != null && lb.ToolTip != null)
                {
                    lb.Content = dt.Select("MNGT_ITEM_NAME='" + lb.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + lb.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }
                else if (tBox != null && tBox.ToolTip != null)
                {
                    tBox.Text = dt.Select("MNGT_ITEM_NAME='" + tBox.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + tBox.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }
                else if (tBlock != null && tBlock.ToolTip != null)
                {
                    tBlock.Text = dt.Select("MNGT_ITEM_NAME='" + tBlock.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + tBlock.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }
                else if (bcd != null && bcd.ToolTip != null)
                {
                    bcd.Text = dt.Select("MNGT_ITEM_NAME='" + bcd.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + bcd.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                    if (bcd.ToolTip.Equals("BCD_MATRIX"))
                        txtBCR.Text = bcd.Text;
                }
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    SetMatLabelPreview(VisualTreeHelper.GetChild(obj, i), dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region Events
        private void dgOutboxList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutboxList.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutboxList.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgOutboxList.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgOutboxList.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutboxList.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutboxList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutboxList.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPallet();
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            string boxID = string.Empty;
            string batch1 = string.Empty;
            string batch2 = string.Empty;
            string quantity = string.Empty;
            string dateCode = string.Empty;
            string productionDate = string.Empty;
            string expDate = string.Empty;
            string areaId = string.Empty;
            string copyCnt = string.Empty;
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }

            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgOutboxList, "CHK");

            foreach (int idx in idxBoxList)
            {
                boxID = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["BOXID"].Index).Value);
                batch1 = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["PKG_LOTID"].Index).Value);
                batch2 = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["CELL_RANK"].Index).Value);
                quantity = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["TOTAL_QTY"].Index).Value);
                dateCode = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["PROD_DATE"].Index).Value);
                expDate = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["VALID_DATE"].Index).Value);
                productionDate = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["PRODUCTION_DATE"].Index).Value);
                areaId = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["AREAID"].Index).Value);
                copyCnt = Util.NVC(dgOutboxList.GetCell(idx, dgOutboxList.Columns["BOX_CNT"].Index).Value);
                // 출력
                PrintMatLabel(boxID, batch1, batch2, quantity, dateCode, expDate, productionDate, areaId, copyCnt);
            }

        }
        private void txtBCR_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBCR.Text))
            {
                ClearBCRView(grd2DBCRContent);
                return;
            }
            /*
            string[] bcrText = txtBCR.Text.Split('@');
            txtHead.Text = bcrText[0] + bcrText[1];
            txt12S.Text = "@" + bcrText[2];
            txtP.Text = "@" + bcrText[3];
            txt1P.Text = "@" + bcrText[4];
            txt31P.Text = "@" + bcrText[5];
            txt12V.Text = "@" + bcrText[6];
            txt10V.Text = "@" + bcrText[7];
            txt2P.Text = "@" + bcrText[8];
            txt20P.Text = "@" + bcrText[9];
            txt6D.Text = "@" + bcrText[10];
            txt14D.Text = "@" + bcrText[11];
            txt30P.Text = "@" + bcrText[12];
            txtZ.Text = "@" + bcrText[13];
            txtK.Text = "@" + bcrText[14];
            txt16K.Text = "@" + bcrText[15];
            txtV.Text = "@" + bcrText[16];
            txt3S.Text = "@" + bcrText[17];
            txtQ.Text = "@" + bcrText[18];
            txt20T.Text = "@" + bcrText[19];
            txt1T.Text = "@" + bcrText[20];
            txt2T.Text = "@" + bcrText[21];
            txt1Z.Text = "@" + bcrText[22];
            txtTail.Text = "@@";
            */

            /* Data Matrix Code Sequence : 
            [)><RS>06<GS>12PGTL3<GS>9K14<GS>1J<GS>14D<GS>16Dproductiondate
            <GS>Pboschpartnumber
            <GS>2P<GS>1Tbatchnumber
            <GS>Qquantity
            <GS>3Q<GS>K<GS>4K<GS>2S<GS>15K<GS>Vsuppliernumber
            <GS>13V<GS>7Q<GS>2K<GS>23PEB/PT
            <GS>30P<GS>12V<GS>1P<GS>33T
            <GS>1Pcellname
            <GS>10Vproductionplant
            <GS>20Pplantcode
            <GS>2Tcellranking<RS><EOT>
            */
            string strBCR = txtBCR.Text.Replace("&lt;RS&gt;", "<RS>").Replace("&lt;GS&gt;", "<GS>");
            int strLength = strBCR.Length;
            txtMatrixCode1.Text = strBCR.Substring(0, strLength / 2);
            txtMatrixCode2.Text = strBCR.Substring(strLength / 2);
        }
        private void ClearBCRView(DependencyObject obj)
        {
            try
            {
                TextBlock tb = obj as TextBlock;

                if (tb != null)
                    tb.Text = string.Empty;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    ClearBCRView(VisualTreeHelper.GetChild(obj, i));

            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private void BcrTextMouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            switch (tb.Name)
            {
                case "txt12S":
                    {
                        tb.ToolTip = "Label Version";
                        break;
                    }
                case "txtP":
                    {
                        tbBP.Foreground = Brushes.Black;
                        tbBP.FontSize = 12;
                        break;
                    }
                case "txt1P":
                    {
                        tb1P.Foreground = Brushes.Black;
                        tb1P.FontSize = 12;
                        break;
                    }
                case "txt31P":
                    {
                        tb31P.Foreground = Brushes.Black;
                        tb31P.FontSize = 12;
                        break;
                    }
                case "txt12V":
                    {
                        tb.ToolTip = "Manufacturer Number";
                        break;
                    }
                case "txt10V":
                    {
                        tb10V.Foreground = Brushes.Black;
                        tb10V.FontSize = 12;
                        break;
                    }
                case "txt2P":
                    {
                        tb2P.Foreground = Brushes.Black;
                        tb2P.FontSize = 12;
                        break;
                    }
                case "txt20P":
                    {
                        tb20P.Foreground = Brushes.Black;
                        tb20P.FontSize = 12;
                        break;
                    }
                case "txt6D":
                    {
                        tb6D.Foreground = Brushes.Black;
                        tb6D.FontSize = 12;
                        break;
                    }
                case "txt14D":
                    {
                        tb14D.Foreground = Brushes.Black;
                        tb14D.FontSize = 12;
                        break;
                    }
                case "txt30P":
                    {
                        tb.ToolTip = "RoHS";
                        break;
                    }
                case "txtZ":
                    {
                        tb.ToolTip = "MS-Level";
                        break;
                    }
                case "txtK":
                    {
                        tbK.Foreground = Brushes.Black;
                        tbK.FontSize = 12;
                        break;
                    }
                case "txt16K":
                    {
                        tb16K.Foreground = Brushes.Black;
                        tb16K.FontSize = 12;
                        break;
                    }
                case "txtV":
                    {
                        tbV.Foreground = Brushes.Black;
                        tbV.FontSize = 12;
                        break;
                    }
                case "txt3S":
                    {
                        tb3S.Foreground = Brushes.Black;
                        tb3S.FontSize = 12;
                        break;
                    }
                case "txtQ":
                    {
                        tbQ.Foreground = Brushes.Black;
                        tbQ.FontSize = 12;
                        break;
                    }
                case "txt20T":
                    {
                        tb.ToolTip = "Batch-Counter";
                        break;
                    }
                case "txt1T":
                    {
                        tb1T.Foreground = Brushes.Black;
                        tb1T.FontSize = 12;
                        break;
                    }
                case "txt2T":
                    {
                        tb2T.Foreground = Brushes.Black;
                        tb2T.FontSize = 12;
                        break;
                    }
                case "txt1Z":
                    {
                        tb1Z.Foreground = Brushes.Black;
                        tb1Z.FontSize = 12;
                        break;
                    }
                default:
                    break;
            }
        }
        void BcrTextMouseEnter(object sender, EventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            switch (tb.Name)
            {
                case "txt12S":
                    {
                        tb.ToolTip = "Label Version";
                        break;
                    }
                case "txtP":
                    {
                        tbBP.Foreground = Brushes.Red;
                        tbBP.FontSize = 15;
                        tbBP.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt1P":
                    {
                        tb1P.Foreground = Brushes.Red;
                        tb1P.FontSize = 15;
                        tb1P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt31P":
                    {
                        tb31P.Foreground = Brushes.Red;
                        tb31P.FontSize = 15;
                        tb31P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt12V":
                    {
                        tb.ToolTip = "Manufacturer Number";
                        break;
                    }
                case "txt10V":
                    {
                        tb10V.Foreground = Brushes.Red;
                        tb10V.FontSize = 15;
                        tb10V.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt2P":
                    {
                        tb2P.Foreground = Brushes.Red;
                        tb2P.FontSize = 15;
                        tb2P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt20P":
                    {
                        tb20P.Foreground = Brushes.Red;
                        tb20P.FontSize = 15;
                        tb20P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt6D":
                    {
                        tb6D.Foreground = Brushes.Red;
                        tb6D.FontSize = 15;
                        tb6D.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt14D":
                    {
                        tb14D.Foreground = Brushes.Red;
                        tb14D.FontSize = 15;
                        tb14D.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt30P":
                    {
                        tb.ToolTip = "RoHS";
                        break;
                    }
                case "txtZ":
                    {
                        tb.ToolTip = "MS-Level";
                        break;
                    }
                case "txtK":
                    {
                        tbK.Foreground = Brushes.Red;
                        tbK.FontSize = 15;
                        tbK.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt16K":
                    {
                        tb16K.Foreground = Brushes.Red;
                        tb16K.FontSize = 15;
                        tb16K.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txtV":
                    {
                        tbV.Foreground = Brushes.Red;
                        tbV.FontSize = 15;
                        tbV.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt3S":
                    {
                        tb3S.Foreground = Brushes.Red;
                        tb3S.FontSize = 15;
                        tb3S.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txtQ":
                    {
                        tbQ.Foreground = Brushes.Red;
                        tbQ.FontSize = 15;
                        tbQ.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt20T":
                    {
                        tb.ToolTip = "Batch-Counter";
                        break;
                    }
                case "txt1T":
                    {
                        tb1T.Foreground = Brushes.Red;
                        tb1T.FontSize = 15;
                        tb1T.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt2T":
                    {
                        tb2T.Foreground = Brushes.Red;
                        tb2T.FontSize = 15;
                        tb2T.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt1Z":
                    {
                        tb1Z.Foreground = Brushes.Red;
                        tb1Z.FontSize = 15;
                        tb1Z.FontWeight = FontWeights.Bold;
                        break;
                    }
                default:
                    break;
            }
        }
        private void btnCopy2DBcr_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(bcBoxID.Text);
        }
        private void dgPalletChoice_Checked(object sender, RoutedEventArgs e)
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
                dgPallet.SelectedIndex = idx;

                GetOutBoxList();
            }
        }

        #endregion

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = "";
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = "";
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER wndPopup = sender as CMM_SHIFT_USER;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
            this.grdMain.Children.Remove(wndPopup);
        }

        #endregion


        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            try
            {
                string sBizRule = "BR_PRD_GET_BOSCH_LABEL_ITEM";

                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("BOXID");
                inData.Columns.Add("BATCH_NO1"); // BATCH_NUMBER
                inData.Columns.Add("BATCH_NO2"); // CELL_RANKING
                inData.Columns.Add("QUANTITY");
                inData.Columns.Add("DATE_CODE");
                inData.Columns.Add("EXP_DATE");
                inData.Columns.Add("PRODUCTION_DATE");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("SHOPID");
                inData.Columns.Add("AREAID");
                inData.Columns.Add("USERID");
                inData.Columns.Add("LANGID");
                inData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용
                DataRow inDataRow = inData.NewRow();

                inDataRow["BOXID"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["BOXID"].Index).Value);
                inDataRow["BATCH_NO1"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["PKG_LOTID"].Index).Value);
                inDataRow["BATCH_NO2"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["CELL_RANK"].Index).Value);
                inDataRow["QUANTITY"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["TOTAL_QTY"].Index).Value);
                inDataRow["DATE_CODE"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["PROD_DATE"].Index).Value);
                inDataRow["EXP_DATE"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["VALID_DATE"].Index).Value);
                inDataRow["PRODUCTION_DATE"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["PRODUCTION_DATE"].Index).Value);
                inDataRow["SHIPTO_ID"] = Util.NVC("M1008"); // BOSH
                inDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataRow["AREAID"] = Util.NVC(dgOutboxList.GetCell(index, dgOutboxList.Columns["AREAID"].Index).Value);
                inDataRow["USERID"] = txtWorker.Tag;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["PGM_ID"] = _sPGM_ID;
                inDataRow["BZRULE_ID"] = sBizRule;
                inData.Rows.Add(inDataRow);

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");
                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = "";
                inPrintDr["RESO"] = "";
                inPrintDr["PRCN"] = "";
                inPrintDr["MARH"] = "";
                inPrintDr["MARV"] = "";
                inPrintDr["DARK"] = "";
                inPrintTable.Rows.Add(inPrintDr);

                DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT", "OUTITEM,OUTZPL", inDataSet, null);

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if ((resultSet.Tables.IndexOf("OUTITEM") > -1) && resultSet.Tables["OUTITEM"].Rows.Count > 0)
                    {
                        DataTable dt = resultSet.Tables["OUTITEM"];
                        SetMatLabelPreview(grd_LabelPreview, dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetPallet();
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetPallet();
            }
        }
    }
}
