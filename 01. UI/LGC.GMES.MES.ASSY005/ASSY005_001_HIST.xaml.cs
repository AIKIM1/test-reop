/*************************************************************************************
 Created Date : 2020.11.17
      Creator : 신광희
   Decription : CNB2동 증설 - 노칭 공정진척 화면 - 라벨 발행(ASSY004_001_HIST Copy 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.17  신광희 : Initial Created.

**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_001_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_001_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        string _EQPT_CELL_PRINT_FLAG = string.Empty; // 라벨 프린트 적용에대한 FLAG

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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

        public ASSY005_001_HIST()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        //2019.04.22 김대근 수정
        //그리드에 CarrierID 추가
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 4)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _LineID = Util.NVC(tmps[1]);
                    _EqptID = Util.NVC(tmps[2]);
                    //_UNLDR코드를 가져온다.
                    _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[3]);
                }

                //UNLDR코드가 RF_ID인지 유무를 통해 CSTID칼럼 Visibility 설정
                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    dgList.Columns["CSTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgList.Columns["CSTID"].Visibility = Visibility.Collapsed;
                }

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.04.22 김대근 수정
        //기존에 있던 것 이름만 수정
        private void txtCondition_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    GetLotHistoryByLotCstId();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.04.22 김대근 수정
        //CarrierID로 조회 기능 추가
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //LOTID와 CSTID 둘 다 사용할 수 있도록 수정된 DA를 사용하는 메소드를 호출
                GetLotHistoryByLotCstId();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    //row 색 바꾸기
                                    dg.SelectedIndex = e.Cell.Row.Index;
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                }

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                    }
                }
            }));
        }

        private void btnPrint_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                eqptPrintLabelChk();

                if (_EQPT_CELL_PRINT_FLAG.Equals("Y"))
                {
                    // 프린트는 하지 않고 파일만 내려 보내주기
                    int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");

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

                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                        sCopy = Convert.ToString(Convert.ToInt32(sCopy) + 1);

                    string sZPL = GetPrintInfo(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);
                    string sLot = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID"));

                    Util.SendZplBarcode(sLot, sZPL);
                }
                else
                {
                    PrintZPL();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("CHK"))
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("LOTID") ||
                            Util.NVC(e.Cell.Column.Name).Equals("CSTID"))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_LABEL_PRT_FLAG")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_LABEL_PRT_FLAG")).Equals("N"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                        }
                    }
                }
            }));
        }


        #endregion

        #region Mehod
        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void PrintZPL()
        {
            try
            {
                if (dgList == null || dgList.Rows.Count < 1)
                    return;

                ShowLoadingIndicator();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1651");	//선택된 항목이 없습니다.
                    return;
                }

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

                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                    sCopy = Convert.ToString(Convert.ToInt32(sCopy) + 1);

                string sZPL = GetPrintInfo(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);

                if (sZPL.Equals(""))
                {
                    Util.MessageValidation("SFU1498");
                    return;
                }

                if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                {
                    if (PrintLabel(sZPL.Substring(2), drPrtInfo))
                        SetLabelPrtHist(sZPL.Substring(2), drPrtInfo, Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "WIPSEQ")), sLBCD);
                }
                else
                {
                    Util.Alert(sZPL.Substring(2));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
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
        #endregion

        #region [BizCall]
        
        //2019.04.22 김대근
        //CarrierID로 조회 기능 추가
        //기존 메소드에 input parameter만 수정
        //LOTID와 CSTID 둘 다 사용할 수 있도록 수정된 DA를 사용하는 메소드를 호출
        private void GetLotHistoryByLotCstId()
        {
            try
            {
                if (dtpDateFrom == null || dtpDateFrom.SelectedDateTime == null ||
                    dtpDateTo == null || dtpDateTo.SelectedDateTime == null)
                    return;

                string sBizName = string.Empty;

                if (_ProcID.Equals(Process.NOTCHING))
                    sBizName = "DA_PRD_SEL_WIP_CNF_HISTORY_L";
                else
                    sBizName = "DA_PRD_SEL_WIP_CNF_HISTORY_VD_PANCAKE_L";

                ShowLoadingIndicator();

                DataTable inTable = null;

                //BizDataSet에 메소드를 추가하지 않고 직접 칼럼을 작성을 했습니다.
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOT_CST_ID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));

                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOT_CST_ID"] = txtLotCst.Text;
                newRow["LOTID"] = txtLotCst.Text;
                newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgList, searchResult, null, true);

                        if (dgList.CurrentCell != null)
                            dgList.CurrentCell = dgList.GetCell(dgList.CurrentCell.Row.Index, dgList.Columns.Count - 1);
                        else if (dgList.Rows.Count > 0 && dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1) != null)
                            dgList.CurrentCell = dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness

                //  노칭 라벨 바코드 품질 테스트를 하기 위한 신규건
                // 염규범                
                if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals(""))
                    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                else
                    newRow["LBCD"] = "LBL0001"; // LABEL CODE
                newRow["NT_WAIT_YN"] = "N"; // 대기 팬케익 재발행 여부.


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
                HideLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "NOTCHED PANCAKE";
                //newRow["PRT_ITEM04"] = "";
                //newRow["PRT_ITEM05"] = "";
                //newRow["PRT_ITEM06"] = "";
                //newRow["PRT_ITEM07"] = "";
                //newRow["PRT_ITEM08"] = "";
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
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

                        GetLotHistoryByLotCstId();

                        //Util.AlertInfo("정상 처리 되었습니다.");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void eqptPrintLabelChk()
        {
            string cEqpt = string.Empty;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");

            cEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "EQPTID"));


            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = cEqpt;

            inTable.Rows.Add(newRow);


            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR_CELL_ID_PRT_FLAG", "INDATA", "OUTDATA", inTable);

            if (dtResult?.Rows?.Count > 0)
                _EQPT_CELL_PRINT_FLAG = dtResult.Rows[0]["CELL_ID_PRT_FLAG"].ToString();

        }
        #endregion
        #endregion

    }
}
