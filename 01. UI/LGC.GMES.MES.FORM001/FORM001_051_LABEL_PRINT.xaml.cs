/*************************************************************************************
 Created Date : 2017.07.03
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - DSF 대기창고 관리 - 라벨발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.03  두잇 이선규K : Initial Created.
  2017.09.18  INS  김동일K : 조립 Prj 에서 활성화 Prj 로 소스코드 이동
 **************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_051_LABEL_PRINT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_051_LABEL_PRINT : C1Window, IWorkArea
    {   
        private string _ProcID = string.Empty;
        private string _WarehouseID = string.Empty;
        private string _WarehouseRackID = string.Empty;
        private string _LotID = string.Empty;
        private string _StartDttm = string.Empty;
        private string _EndDttm = string.Empty;
        private string _LineID = string.Empty;
        private string _WipYn = string.Empty;

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_051_LABEL_PRINT()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboDsfWarehouseChild = { cboDsfWarehouseRack };
            String[] sDsfWarehouseFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboDsfWarehouse, CommonCombo.ComboStatus.SELECT, cbChild: cboDsfWarehouseChild, sFilter: sDsfWarehouseFilter);

            C1ComboBox[] cboDsfWarehouseRackParent = { cboDsfWarehouse };
            String[] sDsfWarehouseRackFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboDsfWarehouseRack, CommonCombo.ComboStatus.SELECT, cbParent: cboDsfWarehouseRackParent, sFilter: sDsfWarehouseRackFilter);
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 8)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _WarehouseID = Util.NVC(tmps[1]);
                    _WarehouseRackID = Util.NVC(tmps[2]);
                    _LotID = Util.NVC(tmps[3]);

                    cboDsfWarehouse.SelectedValue = _WarehouseID;

                    if (_WarehouseRackID.Length > 0 && !_WarehouseRackID.ToUpper().Equals("SELECT") && !_WarehouseRackID.ToUpper().Equals("ALL"))
                        cboDsfWarehouseRack.SelectedValue = _WarehouseRackID;

                    _StartDttm = Util.NVC(tmps[4]);
                    _EndDttm = Util.NVC(tmps[5]);
                    _LineID = Util.NVC(tmps[6]);
                    _WipYn = Util.NVC(tmps[7]);
                }
                else
                {
                    _ProcID = string.Empty;
                    _WarehouseID = string.Empty;
                    _WarehouseRackID = string.Empty;
                    _LotID = string.Empty;

                    _StartDttm = string.Empty;
                    _EndDttm = string.Empty;
                    _LineID = string.Empty;
                    _WipYn = string.Empty;
                }

                ApplyPermissions();
                GetLabelPrintTarget();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion [Initialize]

        #region [Main Window Event]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetLabelPrintTarget();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPrint_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintZPL();
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

        #endregion [Main Window Event]

        #region [Main Grid Event]

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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_LABEL_PRT_FLAG")).Equals("Y"))
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    //}
                    //else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_LABEL_PRT_FLAG")).Equals("N"))
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    //}
                }
            }));
        }

        private void dgList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            {
                                if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null)
                                    break;

                                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                                if (chk == null || !(chk.IsChecked.HasValue))
                                    break;

                                chk.IsChecked = !((bool)(chk.IsChecked));
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", (bool)(chk.IsChecked));

                                if ((bool)(chk.IsChecked))
                                {
                                    dg.SelectedIndex = e.Cell.Row.Index;
                                }

                                C1.WPF.DataGrid.DataGridCell cell = null;
                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (idx.Equals(e.Cell.Row.Index))
                                        continue;

                                    cell = dg.GetCell(idx, e.Cell.Column.Index);
                                    if (cell.Presenter != null && cell.Presenter.Content != null && (cell.Presenter.Content as CheckBox) != null)
                                    {
                                        (cell.Presenter.Content as CheckBox).IsChecked = false;
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
            }));
        }

        #endregion [Main Grid Event]

        #region [Biz]

        private void GetLabelPrintTarget()
        {
            try
            {
                if (_LotID.Length < 1 || _WarehouseID.Length < 1)
                {
                    dgList.ItemsSource = null;
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                //inTable.Columns.Add("DATE_FROM", typeof(string));
                //inTable.Columns.Add("DATE_TO", typeof(string));
                //inTable.Columns.Add("WH_ID", typeof(string));
                //inTable.Columns.Add("RACK_ID", typeof(string));
                //inTable.Columns.Add("LINEID", typeof(string));
                //inTable.Columns.Add("WIPYN", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _ProcID;
                //dr["DATE_FROM"] = _StartDttm;
                //dr["DATE_TO"] = _EndDttm;

                //dr["WH_ID"] = _WarehouseID;
                //if (Util.NVC(dr["WH_ID"]).Length < 1 || Util.NVC(dr["WH_ID"]).ToUpper().Equals("ALL") || Util.NVC(dr["WH_ID"]).ToUpper().Equals("SELECT"))
                //    dr["WH_ID"] = DBNull.Value;

                //dr["RACK_ID"] = _WarehouseRackID;
                //if (Util.NVC(dr["RACK_ID"]).Length < 1 || Util.NVC(dr["RACK_ID"]).ToUpper().Equals("ALL") || Util.NVC(dr["RACK_ID"]).ToUpper().Equals("SELECT"))
                //    dr["RACK_ID"] = DBNull.Value;

                //dr["LINEID"] = _LineID;
                //if (Util.NVC(dr["LINEID"]).Length < 1 || Util.NVC(dr["LINEID"]).ToUpper().Equals("ALL") || Util.NVC(dr["LINEID"]).ToUpper().Equals("SELECT"))
                //    dr["LINEID"] = DBNull.Value;

                //dr["WIPYN"] = _WipYn.ToUpper().Equals("TRUE") ? "Y" : null;

                dr["LOTID"] = _LotID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LABEL_PRINT_TARGET_STOCK", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
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
                        HiddenLoadingIndicator();
                    }
                });

                //DataTable inTable = _Biz.GetDA_PRD_SEL_LABEL_PRINT_TARGET_STOCK();

                //DataRow newRow = inTable.NewRow();
                //newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["PROCID"] = _ProcID;
                //newRow["WH_ID"] = _WarehouseID;
                //newRow["LOTID"] = _LotID;

                //inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_LABEL_PRINT_TARGET_STOCK", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                //{
                //    try
                //    {
                //        if (searchException != null)
                //        {
                //            Util.MessageException(searchException);
                //            return;
                //        }

                //        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                //        Util.GridSetData(dgList, searchResult, null, true);

                //        if (dgList.CurrentCell != null)
                //            dgList.CurrentCell = dgList.GetCell(dgList.CurrentCell.Row.Index, dgList.Columns.Count - 1);
                //        else if (dgList.Rows.Count > 0 && dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1) != null)
                //            dgList.CurrentCell = dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //}
                //);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";

            try
            {
                ShowLoadingIndicator();

                //DataTable inTable = _Biz.GetBR_PRD_GET_PROCESS_LOT_LABEL_STOCK();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("PRMK", typeof(string));
                inTable.Columns.Add("RESO", typeof(string));
                inTable.Columns.Add("PRCN", typeof(string));
                inTable.Columns.Add("MARH", typeof(string));
                inTable.Columns.Add("MARV", typeof(string));
                inTable.Columns.Add("DARK", typeof(string));
                inTable.Columns.Add("LBCD", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.DSF;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt;  // Print type   : "ZEBRA"
                newRow["RESO"] = sRes;  // DPI          : "203"
                newRow["PRCN"] = sCopy; // Print Count  : "1"
                newRow["MARH"] = sXpos; // Horizone pos : "0"
                newRow["MARV"] = sYpos; // Vertical pos : "0"
                newRow["DARK"] = sDark; // darkness     :

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "PRODID"));
                newRow["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "EQSGID"));

                //if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals("")) // DSF 대기창고 라벨은 그냥 LBL0083 고정
                //    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE;
                //else
                //    newRow["LBCD"] = "LBL0001";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_STOCK", "INDATA", "OUTDATA", inTable);

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

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "DSF Stocker";

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

                        GetLabelPrintTarget();
                        Util.MessageValidation("SFU1275");  // 정상 처리되었습니다.
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

        private void SetLotInfoStock(string sLotID, string sRackID)
        {
            try
            {
                DataRow newRow = null;
                DataSet inDataSet = _Biz.GetBR_PRD_REG_RACK_STOCK();

                DataTable inTable = inDataSet.Tables["INDATA"];
                newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                newRow = null;
                DataTable inputLOT = inDataSet.Tables["INLOT"];
                newRow = inputLOT.NewRow();
                newRow["LOTID"] = sLotID;
                newRow["RACK_ID"] = sRackID;

                inputLOT.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RACK_STOCK", "INDATA,INLOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion [Biz]

        #region [Validation]

        #endregion [Validation]

        #region [FUNC]

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

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion [FUNC]

        #region [Label Print]

        private void PrintZPL()
        {
            try
            {
                if (dgList == null || dgList.Rows.Count < 1)
                    return;

                ShowLoadingIndicator();

                string sWarehouseRackID = Util.NVC(cboDsfWarehouseRack.SelectedValue);
                if (sWarehouseRackID.Length < 1 || sWarehouseRackID.ToUpper().Equals("ALL") || sWarehouseRackID.ToUpper().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1751"); // 위치를 선택 하세요.
                    return;
                }

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
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
                DataRow drPrtInfo = null;

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                string sZPL = GetPrintInfo(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);

                if (sZPL.Equals(""))
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                    return;
                }

                if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                {
                    if (PrintLabel(sZPL.Substring(2), drPrtInfo))
                    {
                        SetLotInfoStock(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")), Util.NVC(cboDsfWarehouseRack.SelectedValue)); // 라벨 발행 이력과는 별개로..
                        SetLabelPrtHist(sZPL.Substring(2), drPrtInfo, Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "WIPSEQ")), sLBCD);
                    }
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
                HiddenLoadingIndicator();
            }
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030")); // 프린터 환경설정 정보가 없습니다.

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); // Barcode Print 실패
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); // Barcode Print 실패
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); // Barcode Print 실패
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031")); // 프린터 환경설정에 포트명 항목이 없습니다.
                Util.MessageValidation("SFU3031"); // 프린터 환경설정에 포트명 항목이 없습니다.
            }

            return brtndefault;
        }

        #endregion [Label Print] 
    }
}
