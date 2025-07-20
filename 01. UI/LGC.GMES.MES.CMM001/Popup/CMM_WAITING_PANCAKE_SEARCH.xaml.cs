/*************************************************************************************
 Created Date : 2017.07.01
      Creator : 이대영D
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.01   INS 이대영D : Initial Created.
   2023.03.27    IS           이홍주    소형활성화MES 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_WAITING_PANCAKE_SEARCH : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private DataTable selectDt = null;
        public DataRow DrSelectedDataRow = null;
        private string woID = string.Empty;
        private string lineID = string.Empty;
        private string procID = string.Empty; 
        private string electordeType = string.Empty;
        private string limitCount = string.Empty;
        private string lotID = string.Empty;
        private string sFlag = string.Empty;
        private string eqptID = string.Empty;
        private string prodID = string.Empty;
        private string gubun = string.Empty;
        private string SearchCheck = string.Empty; //조회여부 체크 Y : 엔터키 N: 버튼키
        private bool isSmallType = false;

        string _sPGM_ID = "CMM_WAITING_PANCAKE_SEARCH";

        public string ELECTRODETYPE
        {
            get { return electordeType; }
        }

        public DataTable SELECTHALFPRODUCT
        {
            get { return selectDt; }
        }

        public DataRow SELECTEDROW
        {
            get { return DrSelectedDataRow; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion
        
        public CMM_WAITING_PANCAKE_SEARCH()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            if (!isSmallType) // 원각
                gubun = "CR";
            else // 초소형
                gubun = "CS";

            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElectordeType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter2 = { LoginInfo.CFG_AREA_ID, gubun, procID };
            _combo.SetCombo(cboEquipmentSegmentAssy, CommonCombo.ComboStatus.NONE, sFilter: sFilter2);
            cboEquipmentSegmentAssy.SelectedValue = lineID;
        }

        #region Event

        #region [Form Load]
        private void CMM_WAITING_PANCAKE_SEARCH_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            woID = tmps[0] as string;
            lineID = tmps[1] as string;
            procID = tmps[2] as string;
            electordeType = tmps[3] as string;
            limitCount = tmps[4] as string;
            lotID = tmps[5] as string;
            sFlag = tmps[6] as string;
            eqptID = tmps[7] as string;
            prodID = tmps[8] as string;
            isSmallType = bool.Parse(Util.NVC(tmps[9]));
             SearchCheck = tmps[10] as string; //조회여부 체크 Y : 엔터키 N: 버튼키
            // INIT COMBO
            InitCombo();

            if (!string.IsNullOrEmpty(electordeType))
            {
                for (int i = 0; i < cboElectordeType.Items.Count; i++)
                {
                    if (string.Equals(electordeType, ((DataRowView)cboElectordeType.Items[i]).Row.ItemArray[0].ToString()))
                    {
                        cboElectordeType.SelectedIndex = i;
                        cboElectordeType.IsEnabled = false;
                        break;
                    }
                }
            }

            //메뉴에서 호출하는 경우는 그리드의 라디오 버튼과 하단의 선택 버튼은 보여주지 않는다.
            if (string.Equals(sFlag, "A"))
            {
                rdoChk.Visibility = Visibility.Hidden;
                btnSelect.Visibility = Visibility.Hidden;
            }

            if (string.Equals(sFlag, "S"))
            {
                cboElectordeType.IsEnabled = true;
                rdoChk.Visibility = Visibility.Collapsed;
                btnSelect.Visibility = Visibility.Collapsed;
            }

            if (string.Equals(electordeType, "JR"))
            {
                lblGubun.Visibility = Visibility.Collapsed;
                cboElectordeType.Visibility = Visibility.Collapsed;
            }

            // INIT TEXTBOX
            if (SearchCheck == "Y") //조회여부 체크 Y : 엔터키 N: 버튼키
            {
                if (!string.IsNullOrEmpty(lotID))
                {
                    txtLotId.Text = lotID;
                    txtLotId.Focus();
                    txtLotId.SelectAll();
                }
            }
            else
            {
                txtLotId.Text = string.Empty;
            }

            if (string.Equals(sFlag, "A") || string.Equals(sFlag, "S"))
            {
                chkWoMaterial.IsChecked = false;
                chkWoMaterial.Visibility = Visibility.Collapsed;
                
            }
            else
            {
                gdInputProduct.Columns["PRINT"].Visibility = Visibility.Collapsed;
            }


            // SET DATA
            GetSearchGrid();

            this.BringToFront();
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtLotId.Text.Length != 0 && txtLotId.Text.Length < 3)
            {
                Util.MessageValidation("SFU3624"); // LOTID는 3자리 이상 입력하세요.
                return;
            }
            // SEARCH
            GetSearchGrid();
        }

        #region [선택]

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (gdInputProduct.Rows.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (selectDt != null)
            {
                for (int i = 0; i < selectDt.Rows.Count; i++)
                {
                    if (string.Equals(selectDt.Rows[i]["CHK"].ToString(), "True"))
                    {
                        selectDt.Rows[i]["CHK"] = 1;
                    }
                    else
                    {
                        selectDt.Rows[i]["CHK"] = 0;
                    }
                }
            }

            DataRow[] dr = Util.gridGetChecked(ref gdInputProduct, "CHK");

            if (dr.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (dr == null || dr.Length < 1)
                DrSelectedDataRow = null;
            else
                DrSelectedDataRow = dr[0];

            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region [닫기]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void GetSearchGrid()
        {
            // SELECT HALF PRODUCT
            try
            {
                //ShowLoadingIndicator();

                Util.gridClear(gdInputProduct);

                string sBizName = string.Empty;

                if ((string.Equals(procID, Process.WINDING) || string.Equals(procID, Process.WINDING_POUCH)) && chkWoMaterial.IsChecked == true)
                    sBizName = "DA_PRD_SEL_HALFPROD_LIST_WO_WN";    // W/O 체크
                else
                    sBizName = "DA_PRD_SEL_HALFPROD_LIST_WN";       // W/O 체크해제


                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("ELECTRODETYPE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                if ((string.Equals(procID, Process.WINDING) || string.Equals(procID, Process.WINDING_POUCH)) && chkWoMaterial.IsChecked == true)
                {
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                }
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["EQSGID"] = lineID;
                inDataRow["PROCID"] = Util.NVC(procID);

                if ((string.Equals(procID, Process.WINDING) || string.Equals(procID, Process.WINDING_POUCH)) && chkWoMaterial.IsChecked == true)
                    inDataRow["PRODID"] = null;
                else
                    inDataRow["PRODID"] = string.IsNullOrEmpty(prodID) ? null : Util.NVC(prodID);

                if (cboElectordeType.SelectedIndex > 0)
                    inDataRow["ELECTRODETYPE"] = Util.NVC(cboElectordeType.SelectedValue);

                inDataRow["LOTID"] = Util.NVC(txtLotId.Text.Trim());

                if ((string.Equals(procID, Process.WINDING) || string.Equals(procID, Process.WINDING_POUCH)) && chkWoMaterial.IsChecked == true)
                {
                    inDataRow["EQPTID"] = Util.NVC(eqptID);
                }
                inDataTable.Rows.Add(inDataRow);

                DataSet ds = new DataSet();
                inDataTable.TableName = "RQSTDT";
                ds.Tables.Add(inDataTable);
                //string sTestXml = ds.GetXml();

                selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);

                if (selectDt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1901");
                    return;
                }
                gdInputProduct.ItemsSource = DataTableConverter.Convert(selectDt);

                //Util.GridSetData(gdInputProduct, selectDt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]

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

        private void GridDataBinding(C1DataGrid dataGrid, List<String> bindValues, bool isNewFlag, bool isInput)
        {
            if (dataGrid.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                    colDt.Columns.Add(dataGrid.Columns[i].Name);

                dataGrid.ItemsSource = DataTableConverter.Convert(colDt);
            }

            DataTable inputDt = ((DataView)dataGrid.ItemsSource).Table;
            DataRow inputRow = inputDt.NewRow();

            for (int i = 0; i < inputDt.Columns.Count; i++)
                inputRow[inputDt.Columns[i].Caption] = bindValues[i];

            // ADD DATA
            inputDt.Rows.Add(inputRow);

            if (isNewFlag)
            {
                if (isInput)
                {
                    dataGrid.ItemsSource = DataTableConverter.Convert(inputDt);
                }
                else
                {
                    // 취소시 원래 순서에 맞게 Sorting
                    DataView inputVm = inputDt.DefaultView;
                    inputVm.Sort = "ROW_SEQ";

                    dataGrid.ItemsSource = DataTableConverter.Convert(inputVm.ToTable());

                    dataGrid.SelectedIndex = Util.StringToInt(inputRow["ROW_SEQ"].ToString()) - 1;
                    try
                    {
                        dataGrid.ScrollIntoView(dataGrid.SelectedIndex, 0);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }

            }
        }

        #endregion

        #endregion

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        gdInputProduct.SelectedIndex = idx;

                        selectDt = dtRow.Table;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void gdInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowIndex = 0;
                int colIndex = 0;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null && gdInputProduct.SelectedIndex >= 0)
                {
                    rowIndex = e.Cell.Row.Index;
                    colIndex = e.Cell.Column.Index;

                    int selectedIdx = gdInputProduct.SelectedIndex;

                    for (int row = 0; row < selectDt.Rows.Count; row++)
                    {
                        if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "True"))
                        {
                            if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                            {
                                gdInputProduct.SelectedIndex = row;
                            }
                        }
                    }
                }
            }));
        }

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string inputLotId = txtLotId.Text.Trim();

                if (inputLotId.Length < 3)
                {
                    Util.MessageValidation("SFU3624"); // LOTID는 3자리 이상 입력하세요.
                    return;
                }
                // SEARCH
                GetSearchGrid();
            }
        }

        private void gdInputProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int selectedIdx = gdInputProduct.SelectedIndex;

                if (selectedIdx < 0)
                    return;

                for (int row = 0; row < selectDt.Rows.Count; row++)
                {
                    if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "True"))
                    {
                        if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                        {
                            gdInputProduct.SelectedIndex = row;
                        }
                    }
                }
            }));
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                string printName = string.Empty;
                string dpi = string.Empty;
                string copy = string.Empty;
                string xposition = string.Empty;
                string yposition = string.Empty;
                string darkness = string.Empty;
                string equipmentCode = string.Empty;
                DataRow drPrintInfo = null;

                string lotId = string.Empty;
                string wipSeq = string.Empty;
                if (btn != null)
                {
                    lotId = Util.NVC(DataTableConverter.GetValue(btn.DataContext, "LOTID"));
                    wipSeq = Util.NVC(DataTableConverter.GetValue(btn.DataContext, "WIPSEQ"));
                }

                if (!string.IsNullOrEmpty(lotId))
                {
                    if (!CommonVerify.HasTableRow(LoginInfo.CFG_SERIAL_PRINT))
                    {
                        Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                        return;
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        if (!_Util.GetConfigPrintInfo(out printName, out dpi, out copy, out xposition, out yposition, out darkness, out drPrintInfo))
                            return;
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(eqptID))
                            {
                                printName = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                dpi = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                copy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                xposition = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                yposition = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                darkness = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                equipmentCode = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                drPrintInfo = dr;
                            }
                        }

                        //if (sEqpt.Equals(""))
                        if(string.IsNullOrEmpty(equipmentCode))
                        {
                            Util.MessageValidation("SFU3615");  //프린터 환경설정에 설비 정보를 확인하세요.
                            return;
                        }
                    }

                    string outLabelCode;
                    string zplCode = GetPrintInfo(lotId, wipSeq, printName, dpi, copy, xposition, yposition, darkness, out outLabelCode);

                    if(string.IsNullOrEmpty(zplCode))
                    {
                        Util.MessageValidation("SFU1498");
                        return;
                    }

                    if (zplCode.StartsWith("0,"))  // ZPL 정상 코드 확인.
                    {
                        if (PrintLabel(zplCode.Substring(2), drPrintInfo))
                            SetLabelPrintHistory(zplCode.Substring(2), drPrintInfo, lotId, wipSeq, outLabelCode);
                    }
                    else
                    {
                        //Util.Alert(zplCode.Substring(2));
                        Util.MessageInfo(zplCode.Substring(2));
                    }
                }
                else
                {
                    Util.MessageValidation("SFU1361");
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private string GetPrintInfo(string lotId, string wipSeq, string printName, string dpi, string copy, string xposition, string yposition, string darkness, out string outLabelCode)
        {
            outLabelCode = string.Empty;

            try
            {
                const string bizRuleName = "BR_PRD_GET_PROCESS_LOT_LABEL_WN";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("PRMK", typeof(string));
                inTable.Columns.Add("RESO", typeof(string));
                inTable.Columns.Add("PRCN", typeof(string));
                inTable.Columns.Add("MARH", typeof(string));
                inTable.Columns.Add("MARV", typeof(string));
                inTable.Columns.Add("DARK", typeof(string));
                inTable.Columns.Add("LBCD", typeof(string));
                inTable.Columns.Add("NT_WAIT_YN", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = procID;
                newRow["EQPTID"] = eqptID;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                newRow["PRMK"] = printName;
                newRow["RESO"] = dpi;
                newRow["PRCN"] = copy;
                newRow["MARH"] = xposition;
                newRow["MARV"] = yposition;
                newRow["DARK"] = darkness;
                //if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals(""))
                //    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                //else
                //    newRow["LBCD"] = "LBL0001"; // LABEL CODE
                newRow["NT_WAIT_YN"] = "Y"; // 대기 팬케익 재발행 여부.
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        outLabelCode = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private bool PrintLabel(string zplCode, DataRow drPrintInfo)
        {
            if (drPrintInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrintInfo.Table.Columns.Contains("PORTNAME") && drPrintInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (Util.NVC(drPrintInfo["PORTNAME"]).ToUpper().Equals("USB"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zplCode);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(zplCode, eqptID);
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrintInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrintInfo, zplCode);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrintInfo, zplCode);
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

        private void SetLabelPrintHistory(string zplCode, DataRow drPrintInfo, string lotId, string wipSeq, string outLabelCode)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST_WN";
                                            
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LABEL_CODE", typeof(string));
                inTable.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                inTable.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                inTable.Columns.Add("PRT_ITEM01", typeof(string));
                inTable.Columns.Add("PRT_ITEM02", typeof(decimal));
                inTable.Columns.Add("PRT_ITEM03", typeof(string));
                inTable.Columns.Add("PRT_ITEM04", typeof(string));
                inTable.Columns.Add("PRT_ITEM05", typeof(string));
                inTable.Columns.Add("PRT_ITEM06", typeof(string));
                inTable.Columns.Add("PRT_ITEM07", typeof(string));
                inTable.Columns.Add("PRT_ITEM08", typeof(string));
                inTable.Columns.Add("PRT_ITEM09", typeof(string));
                inTable.Columns.Add("PRT_ITEM10", typeof(string));
                inTable.Columns.Add("PRT_ITEM11", typeof(string));
                inTable.Columns.Add("PRT_ITEM12", typeof(string));
                inTable.Columns.Add("PRT_ITEM13", typeof(string));
                inTable.Columns.Add("PRT_ITEM14", typeof(string));
                inTable.Columns.Add("PRT_ITEM15", typeof(string));
                inTable.Columns.Add("PRT_ITEM16", typeof(string));
                inTable.Columns.Add("PRT_ITEM17", typeof(string));
                inTable.Columns.Add("PRT_ITEM18", typeof(string));
                inTable.Columns.Add("PRT_ITEM19", typeof(string));
                inTable.Columns.Add("PRT_ITEM20", typeof(string));
                inTable.Columns.Add("PRT_ITEM21", typeof(string));
                inTable.Columns.Add("PRT_ITEM22", typeof(string));
                inTable.Columns.Add("PRT_ITEM23", typeof(string));
                inTable.Columns.Add("PRT_ITEM24", typeof(string));
                inTable.Columns.Add("PRT_ITEM25", typeof(string));
                inTable.Columns.Add("PRT_ITEM26", typeof(string));
                inTable.Columns.Add("PRT_ITEM27", typeof(string));
                inTable.Columns.Add("PRT_ITEM28", typeof(string));
                inTable.Columns.Add("PRT_ITEM29", typeof(string));
                inTable.Columns.Add("PRT_ITEM30", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PGM_ID", typeof(string));
                inTable.Columns.Add("BZRULE_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = outLabelCode;
                newRow["LABEL_ZPL_CNTT"] = zplCode;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrintInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrintInfo["COPIES"]);
                newRow["PRT_ITEM01"] = lotId;
                newRow["PRT_ITEM02"] = wipSeq.GetDecimal();
                newRow["PRT_ITEM03"] = "WINDER PANCAKE";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = lotId;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = bizRuleName;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

    }
}
