/*************************************************************************************
 Created Date : 2017.06.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching 공정진척 화면 - 대기LOT조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  INS 김동일K : Initial Created.
  
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_001_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_001_WAITLOT : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        string _sPGM_ID = "ASSY003_001_WAITLOT";

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY003_001_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }
            ApplyPermissions();

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.NOTCHING, null, _LineID };
            _combo.SetCombo(cboOtherEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_WITHOUT_SEL_EQSGID");

            GetWaitLot();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private void rdoAllType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void rdoCType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void rdoAType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = chkOtherLine?.IsChecked != null && (bool)chkOtherLine?.IsChecked ? cboOtherEquipmentSegment.SelectedValue : _LineID;

                if (rdoAType != null && rdoAType.IsChecked.HasValue && (bool)rdoAType.IsChecked)
                    newRow["ELECTYPE"] = "A";
                else if (rdoCType != null && rdoCType.IsChecked.HasValue && (bool)rdoCType.IsChecked)
                    newRow["ELECTYPE"] = "C";

                newRow["LOTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_NT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitLot, searchResult, null, true);

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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion

        #endregion

        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                String sID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sID.Equals(""))
                {

                    // 프린터 정보 조회
                    string sPrt = string.Empty;
                    string sRes = string.Empty;
                    string sCopy = string.Empty;
                    string sXpos = string.Empty;
                    string sYpos = string.Empty;
                    string sDark = string.Empty;
                    string sLBCD = string.Empty;    // 리턴 라벨 타입 코드
                    string sEqpt = string.Empty;
                    DataRow drPrtInfo = null;

                    // 2017-07-04 Lee. D. R
                    // Line별 라벨 독립 발행 기능
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                        return;
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                            return;
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(_EqptID))
                            {
                                sPrt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                sRes = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                sCopy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                sXpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                sYpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                sDark = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                sEqpt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                drPrtInfo = dr;
                            }
                        }

                        if (sEqpt.Equals(""))
                        {
                            Util.MessageValidation("SFU3615");  //프린터 환경설정에 설비 정보를 확인하세요.
                            return;
                        }
                    }

                    string sZPL = GetPrintInfo(sID, Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);

                    if (sZPL.Equals(""))
                    {
                        Util.MessageValidation("SFU1498");
                        return;
                    }

                    if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                    {
                        if (PrintLabel(sZPL.Substring(2), drPrtInfo))
                            SetLabelPrtHist(sZPL.Substring(2), drPrtInfo, sID, Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ")), sLBCD);
                    }
                    else
                    {
                        Util.Alert(sZPL.Substring(2));
                    }

                }
                else
                {
                    Util.MessageValidation("SFU1361");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals(""))
                    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                else
                    newRow["LBCD"] = "LBL0001"; // LABEL CODE
                newRow["NT_WAIT_YN"] = "Y"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                string sBizRule = "BR_PRD_REG_LABEL_PRINT_HIST";

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "WAIT PANCAKE";

                // 프린터 셋팅정보
                if (drPrtInfo?.Table != null)
                {
                    newRow["PRT_ITEM04"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT]) : "";          // DEFAULTYN
                    newRow["PRT_ITEM05"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME]) : "";        // PORTNAME
                    newRow["PRT_ITEM06"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME]) : "";  // PRINTERNAME
                    newRow["PRT_ITEM07"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE]) : "";  // PRINTERTYPE
                    newRow["PRT_ITEM08"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI]) : "";                  // DPI
                    newRow["PRT_ITEM09"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_X) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_X]) : "";                      // XPOS
                    newRow["PRT_ITEM10"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_Y) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y]) : "";                      // YPOS
                    newRow["PRT_ITEM11"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS]) : "";        // DARKNESS
                    newRow["PRT_ITEM12"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]) : "";      // EQUIPMENT
                    newRow["PRT_ITEM13"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY]) : "";    // PRINTERKEY
                    newRow["PRT_ITEM14"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE]) : "";        // ISACTIVE
                }
                
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;

                inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                new ClientProxy().ExecuteService(sBizRule, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
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

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
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
                if (Util.NVC(drPrtInfo["PORTNAME"]).ToUpper().Equals("USB"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(sZPL, _EqptID);
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
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

        private void chkOtherLine_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkOtherLine?.IsChecked == null)
                    return;

                if ((bool)chkOtherLine.IsChecked)
                    cboOtherEquipmentSegment.IsEnabled = true;
                else
                    cboOtherEquipmentSegment.IsEnabled = false;

                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkOtherLine_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkOtherLine?.IsChecked == null)
                    return;

                if ((bool)chkOtherLine.IsChecked)
                    cboOtherEquipmentSegment.IsEnabled = true;
                else
                    cboOtherEquipmentSegment.IsEnabled = false;

                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboOtherEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboOtherEquipmentSegment?.SelectedValue == null || Util.NVC(cboOtherEquipmentSegment?.SelectedValue).Equals("") || Util.NVC(cboOtherEquipmentSegment?.SelectedValue).Equals("SELECT"))
                    return;

                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
