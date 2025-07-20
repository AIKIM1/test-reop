/*************************************************************************************
 Created Date : 2018.01.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 노칭 공정진척 화면 - LOT 분할
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.15  INS 김동일K : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Windows.Input;
using System.Text;
using System;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_LOTCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_LOTCUT : C1Window, IWorkArea
    {   
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineID = string.Empty;

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

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_LOTCUT()
        {
            InitializeComponent();
        }
        
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _ProcID = tmps[0] as string;
            _LineID = tmps[1] as string;
            _EqptID = tmps[2] as string;
        }
        private void txtCutQty_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (e.Text != ".")
                {
                    if (!char.IsDigit(c))
                    {
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMainCutLot();
            }
        }

        private void txtCutQty_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetCutLotSplit();
            }
        }

        private void dgCutList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e != null && string.Equals(e.Cell.Column.Name, "WIPQTY"))
            {
                double dInputQty = Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY"));

                double dTotalCutQty = 0.0;
                for (int i = 0; i < dgCutList.Rows.Count; i++)
                    dTotalCutQty += Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY"));

                if (dInputQty < dTotalCutQty)
                {
                    Util.AlertInfo("SFU2831"); //Cut 수량이 투입량을 초과하였습니다.
                    DataTableConverter.SetValue(dgCutList.Rows[e.Cell.Row.Index].DataItem, "WIPQTY", 0.0);
                    return;
                }
                
                // 잔량 계산
                DataTableConverter.SetValue(dgLotList.Rows[0].DataItem, "WIPQTY2", dInputQty - dTotalCutQty);
            }
        }

        private void btnCut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetCutLotSplit();
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dgLotList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2993"); //현재 조회된 Cut Lot 정보가 없습니다.
                return;
            }

            if (dgCutList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2991"); //현재 Cut 수가 지정되지 않았습니다.
                return;
            }

            for (int i = 0; i < dgCutList.Rows.Count; i++)
            {
                if (Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY")) == 0)
                {
                    Util.MessageValidation("SFU2078"); //Cut할 수량이 0입니다.
                    dgCutList.SelectedIndex = i;
                    return;
                }
            }

            LotCutProcess();
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetMainCutLot()
        {
            try
            {
                Util.gridClear(dgLotList);
                Util.gridClear(dgCutList);
                txtCutQty.Text = "";

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = txtLotId.Text;
                Indata["PROCID"] = _ProcID;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPLIT_LOT_NT", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU2829"); //Cut 가능한 Lot이 존재하지 않습니다.
                    //txtLotId.Focus();
                    return;
                }

                dgLotList.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCutLotSplit()
        {
            if (dgLotList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2993"); //현재 조회된 Cut Lot 정보가 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(txtCutQty.Text) || Convert.ToDouble(txtCutQty.Text) < 1)
            {
                Util.MessageValidation("SFU2830"); //Cut 수량이 0이거나 입력되지 않았습니다.
                //txtCutQty.Focus();
                return;
            }

            Util.gridClear(dgCutList);
            DataTableConverter.SetValue(dgLotList.Rows[0].DataItem, "WIPQTY2", Convert.ToString(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY")));

            if (dgCutList.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                for (int i = 0; i < dgCutList.Columns.Count; i++)
                    colDt.Columns.Add(dgCutList.Columns[i].Name);

                dgCutList.ItemsSource = DataTableConverter.Convert(colDt);
            }

            DataTable dtCut = ((DataView)dgCutList.ItemsSource).Table;
            for (int i = 0; i < Convert.ToInt32(txtCutQty.Text); i++)
            {
                DataRow inputRow = dtCut.NewRow();

                inputRow["SEQNO"] = Convert.ToString(i + 1);
                inputRow["WIPQTY"] = 0.0;
                //inputRow["WIPQTY2"] = 0.0;
                dtCut.Rows.Add(inputRow);
            }
        }

        private void LotCutProcess()
        {
            // DATA SET
            BizDataSet bizRule = new BizDataSet();
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_INLOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("ACTQTY", typeof(double));
            inDataTable.Columns.Add("ACTQTY2", typeof(double));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_OUTLOT");
            inProduct.Columns.Add("ACTQTY", typeof(double));
            inProduct.Columns.Add("ACTQTY2", typeof(double));

            DataTable inTable = indataSet.Tables["IN_INLOT"];
            DataRow newRow = inTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["LOTID"] = txtLotId.Text;
            newRow["PROCID"] = _ProcID;
            newRow["ACTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY2"));
            newRow["ACTQTY2"] = Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY2"));
            newRow["NOTE"] = string.Empty;
            newRow["USERID"] = LoginInfo.USERID;

            inTable.Rows.Add(newRow);

            DataTable inSplit = indataSet.Tables["IN_OUTLOT"];
            for (int i = 0; i < dgCutList.Rows.Count; i++)
            {
                newRow = null;
                newRow = inSplit.NewRow();
                newRow["ACTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY"));
                newRow["ACTQTY2"] = Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY"));

                inSplit.Rows.Add(newRow);
            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_OUT_LOT_NT_S", "IN_INLOT,IN_OUTLOT", "OUT_OUTL0T", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable result = bizResult.Tables["OUT_OUTL0T"];

                    if (result.Rows.Count > 0)
                    {
                        GetWipInfo(result);
                    }

                    Util.gridClear(dgCutList);
                    btnSave.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }, indataSet);
        }

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

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
                newRow["NT_WAIT_YN"] = "N"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NJ", "INDATA", "OUTDATA", inTable);

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
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "NOTCHED PANCAKE";

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

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
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
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
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
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
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
        

        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                string sID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

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
                            Util.MessageValidation("SFU3615"); //프린터 환경설정에 설비 정보를 확인하세요.
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
               

        private void GetWipInfo(DataTable dtLotInfo)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string)); 
                IndataTable.Columns.Add("LOTID", typeof(string));

                string sTmp = "";
                for (int i = 0; i < dtLotInfo.Rows.Count; i++)
                {
                    if (sTmp.Equals(""))
                        sTmp = Util.NVC(dtLotInfo.Rows[i]["LOTID"]);
                    else
                        sTmp = sTmp + "," + Util.NVC(dtLotInfo.Rows[i]["LOTID"]);
                }

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = _ProcID;
                Indata["EQSGID"] = _LineID;
                Indata["EQPTID"] = _EqptID;
                Indata["LOTID"] = sTmp;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_CNF_HISTORY_FOR_CUT", "INDATA", "RSLTDT", IndataTable);

                if (!dt.Columns.Contains("CHK"))
                    dt.Columns.Add("CHK", typeof(bool));

                dgCutResult.ItemsSource = DataTableConverter.Convert(dt);

                if (dt.Rows.Count > 0)
                {
                    dgCutList.Visibility = Visibility.Collapsed;
                    dgCutResult.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            checkAllProcess();
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            uncheckProcess();
        }

        private void checkAllProcess()
        {
            if (dgCutResult == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgCutResult.ItemsSource);

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = true;
                // C1.WPF.DataGrid.DataGridRow row = dgLotInfo.Rows[idx];
                // DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgCutResult.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void uncheckProcess()
        {
            if (dgCutResult == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgCutResult.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgCutResult.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void dgCutResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
    }
}
