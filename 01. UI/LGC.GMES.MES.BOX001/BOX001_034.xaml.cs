/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020-08-12 김동일 : C20200513-000185 취소 선택 후 저장 처리 안되는 문제 수정
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using C1.WPF.Excel;
using System.IO;
using System.Configuration;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using Microsoft.Win32;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_034 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();

        private string _PRODID = string.Empty;
        private string _AREA_LOCATION_CODE = string.Empty;
        private string _PREV_CLOSING_DATE = string.Empty;
        private string _CLOSING_DATE = string.Empty;
        private string sAREAID = string.Empty;
        private string sSHOPID = string.Empty;
        private string sAREAID2 = string.Empty;
        private string sSHOPID2 = string.Empty;

        private bool _bBinding = false;

        public BOX001_034()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
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

        #region Initialize
        private void Initialize()
        {

         //   dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
         //   dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA_CP");

            string[] sFilter = { LoginInfo.CFG_SHOP_ID };
            combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "MODEL_FOR_FCS_HOLD_FRM");

            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.NONE, sCase: "AREA_CP");

            combo.SetCombo(cboModel2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "MODEL_FOR_FCS_HOLD_FRM");

            string[] sFilter2 = { "CELL_DFCT_PRCS_CODE" };
            combo.SetCombo(cboDfctPrcsCode, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE_WITHOUT_CODE");

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetValue();
            SetEvent();
          
        }
        #endregion

        #region [Tab 변경]
        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("tbcWIPControl"))
            {
                if (!_bBinding)
                {
                    _bBinding = true;

                    DataTable dtResult = dtLocCombo();
                    (dgManage.Columns["AREA_LOCATION_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);

                    DataTable dtResult2 = dtTypeCombo("CELL_DFCT_PRCS_CODE");
                    (dgManage.Columns["CELL_DFCT_PRCS_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult2);


                    const string bizRuleName = "DA_BAS_SEL_MODEL";
                    string[] arrColumn = { "PRODID" };
                    string[] arrCondition = { null };
                    string selectedValueText = (string)((PopupFindDataColumn)dgManage.Columns["PRODID"]).SelectedValuePath;
                    string displayMemberText = (string)((PopupFindDataColumn)dgManage.Columns["PRODID"]).DisplayMemberPath;

                    SetFindGridCombo(bizRuleName, dgManage.Columns["PRODID"], arrColumn, arrCondition, selectedValueText, displayMemberText);
                }
            }

        }
        #endregion

        #region [보류재공조회]
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);

            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area            
            combo.SetCombo(cboLocation, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter: sFilter, sCase: "LOCATION");
        }
        
        private void txtPjt_KeyDown(object sender, KeyEventArgs e)
        {
            Search();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Row.ParentGroup != null && (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("AREA_LOCATION_NAME")))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if( dg.CurrentColumn != null)
                {
                    if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                    {
                        _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                        _AREA_LOCATION_CODE = string.Empty;
                        GetDetail(_PRODID);
                    }

                    else if (dg.CurrentColumn.Name.Equals("AREA_LOCATION_NAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                    {
                        _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                        _AREA_LOCATION_CODE = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "AREA_LOCATION_CODE"));

                        GetDetail(_PRODID);
                    }
                }               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region [보류/폐기관리]
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void cboArea2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea2.SelectedValue);

            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID2 = "";
                sSHOPID2 = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID2 = sArry[0];
                sSHOPID2 = sArry[1];
            }

            String[] sFilter = { sAREAID2 };    // Area            
            combo.SetCombo(cboLocation2, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter: sFilter, sCase: "LOCATION");

            DataTable dtResult = dtLocCombo();
            (dgManage.Columns["AREA_LOCATION_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
        }
        
        private void txtPjt2_KeyDown(object sender, KeyEventArgs e)
        {
            Search2();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Search2();
        }

        private void dgManage_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        private void dgManage_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    return;
                }

                else if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")).ToUpper().Equals(bool.FalseString.ToUpper()))
                {
                    e.Cancel = true;
                    return;
                }

                // 신규 행인경우 선택전이라 Null
                else if (DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE") == null)
                {
                    if (e.Column.Name.Equals("CELL_DEFECT_QTY") ||
                        e.Column.Name.Equals("CELL_RESTORE_QTY") ||
                        e.Column.Name.Equals("CELL_DISPOSAL_QTY") ||
                        e.Column.Name.Equals("CELL_FCS_IF_DEFECT_QTY") ||
                        e.Column.Name.Equals("CELL_FCS_IF_CANCEL_DEFECT_QTY")
                       )
                        e.Cancel = true;

                    return;
                }

                // 수량 변경없이 내용/LOT/공정/위치 변경 가능
                else if (e.Column.Name.Equals("NOTE") || e.Column.Name.Equals("LOT_NOTE") || e.Column.Name.Equals("FCS_PROC_NAME") || e.Column.Name.Equals("AREA_LOCATION_CODE"))
                {
                    // I/F Data 위치 수정 불가.
                    if (e.Column.Name.Equals("AREA_LOCATION_CODE"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE")).Equals("FCS_IF_DFCT") ||
                            Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE")).Equals("FCS_IF_CANCEL_DFCT"))
                            e.Cancel = true;
                    }
                    
                    return;
                }

                else if (e.Column.Name.Equals("CNCL_FLAG"))
                {
                    // 등록일이 전마감 ~ 당월마감일 이고 ERP_STAT_CODE = SENDED 이고 폐기인 경우만 가능
                    bool bModify = true;
                    object objState = DataTableConverter.GetValue(e.Row.DataItem, "ERP_STAT_CODE");
                    object objCell = DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE");
                    if (objState == null || objCell == null)
                    {
                        bModify = true;
                    }
                    else
                    {
                        if (Convert.ToDateTime(DataTableConverter.GetValue(e.Row.DataItem, "REG_DATE").ToString()) > Convert.ToDateTime(_PREV_CLOSING_DATE) &&
                            Convert.ToDateTime(DataTableConverter.GetValue(e.Row.DataItem, "REG_DATE").ToString()) < Convert.ToDateTime(_CLOSING_DATE) &&
                            DataTableConverter.GetValue(e.Row.DataItem, "ERP_STAT_CODE").Equals("SENT") &&
                            (DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("SCRAP") || 
                             DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("FCS_IF_DFCT") || 
                             DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("FCS_IF_CANCEL_DFCT")))
                            bModify = false;

                    }
                    e.Cancel = bModify;
                }

                else if (DataTableConverter.GetValue(e.Row.DataItem, "TYPE").Equals("Y"))
                {
                    if (e.Column.Name.Equals("CELL_DEFECT_QTY"))
                    {
                        // DEFECT 보류수량
                        if (!DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("DEFECT"))
                            e.Cancel = true;
                    }
                    else if (e.Column.Name.Equals("CELL_RESTORE_QTY"))
                    {
                        // 재투입수량
                        if (!DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("GOOD"))
                            e.Cancel = true;
                    }
                    else if (e.Column.Name.Equals("CELL_DISPOSAL_QTY"))
                    {
                        // 폐기수량
                        if (!DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("SCRAP"))
                            e.Cancel = true;
                    }
                    else if (e.Column.Name.Equals("CELL_FCS_IF_DEFECT_QTY"))
                    {
                        // 불량수량
                        if (!DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("FCS_IF_DFCT"))
                            e.Cancel = true;
                    }
                    else if (e.Column.Name.Equals("CELL_FCS_IF_CANCEL_DEFECT_QTY"))
                    {
                        // 불량취소 (양품화) 수량
                        if (!DataTableConverter.GetValue(e.Row.DataItem, "CELL_DFCT_PRCS_CODE").Equals("FCS_IF_CANCEL_DFCT"))
                            e.Cancel = true;
                    }
                }
                else
                    e.Cancel =true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgManage_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                string sProjectname;

                if (e.Cell.Column.Name.Equals("PRODID"))
                {
                    SetProjectName(e.Cell.Row.Index, (string)DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID"));

                    //DataTableConverter.SetValue(dgManage.Rows[nIndex].DataItem, "PROJECT_NAME", sProjectname);                    
                }

                //if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").ToString().Equals("N"))
                //{
                //    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CNCL_FLAG").ToString().Equals("True"))
                //    {
                //        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CELL_DFCT_PRCS_CODE").ToString().Equals("SCRAP"))
                //            DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                //    }
                //    //else
                //    //{
                //    //    //  DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", false);
                //    //}
                //}
                //else
                //{
                //    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                //}

               // dgManage.EndEdit();
              //  dgManage.Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();
            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }
                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcelDataFixed(dgManage, stream, 0, 1);
                    }
                }               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        private void btnTemplate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgManage);
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgManage);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                Util.MessageConfirm("SFU1241", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();

                        DataTable dtIndata = indataSet.Tables.Add("INDATA");

                        dtIndata.Columns.Add("AREAID", typeof(string));
                        dtIndata.Columns.Add("USERID", typeof(string));

                        DataRow drIndata = dtIndata.NewRow();
                        drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                        drIndata["USERID"] = LoginInfo.USERID;
                        dtIndata.Rows.Add(drIndata);

                        DataTable dtInformdfet = indataSet.Tables.Add("INFORMDFCT");

                        dtInformdfet.Columns.Add("HIST_SEQNO", typeof(int));
                        dtInformdfet.Columns.Add("REGDATE", typeof(string));
                        dtInformdfet.Columns.Add("PRODID", typeof(string));
                        dtInformdfet.Columns.Add("PROJECT_NAME", typeof(string));
                        dtInformdfet.Columns.Add("AREA_LOCATION_CODE", typeof(string));
                        dtInformdfet.Columns.Add("FCS_PROC_NAME", typeof(string));
                        dtInformdfet.Columns.Add("NOTE", typeof(string));
                        dtInformdfet.Columns.Add("LOT_NOTE", typeof(string));
                        dtInformdfet.Columns.Add("CELL_DFCT_PRCS_CODE", typeof(string));
                        dtInformdfet.Columns.Add("CNCL_FLAG", typeof(string));
                        dtInformdfet.Columns.Add("PILOT_FLAG", typeof(string));
                        dtInformdfet.Columns.Add("CELL_DEFECT_QTY", typeof(Decimal));
                        dtInformdfet.Columns.Add("CELL_RESTORE_QTY", typeof(Decimal));
                        dtInformdfet.Columns.Add("CELL_DISPOSAL_QTY", typeof(Decimal));

                        dtInformdfet.Columns.Add("CELL_FCS_IF_DEFECT_QTY", typeof(Decimal));
                        dtInformdfet.Columns.Add("CELL_FCS_IF_CANCEL_DEFECT_QTY", typeof(Decimal));

                        for (int i = 0; i < dgManage.GetRowCount(); i++)
                        {
                            DataRow drInformdfet = null;
                            if (Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                drInformdfet = dtInformdfet.NewRow();

                                if( DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "HIST_SEQNO") == null)
                                {
                                    //drInformdfet["HIST_SEQNO"] = DBNull; // Util.NVC_Int(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "HIST_SEQNO"));
                                }
                                else
                                {
                                    drInformdfet["HIST_SEQNO"] =  Util.NVC_Int(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "HIST_SEQNO"));
                                }
                                
                                string sRegDate = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "REG_DATE"));
                                DateTime dRegDate = new DateTime();

                                if (string.IsNullOrWhiteSpace(sRegDate))
                                {
                                    // 필수입력항목을 모두 입력하십시오.
                                    Util.MessageValidation("9048");
                                    return;
                                }
                                else if (!DateTime.TryParse(sRegDate, out dRegDate))
                                {
                                    //날짜 형식이 아닙니다.
                                    Util.MessageValidation("SFU3566");
                                    return;
                                }

                                drInformdfet["REGDATE"] = dRegDate.ToString("yyyyMMdd");
                                drInformdfet["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "PRODID"));
                                drInformdfet["PROJECT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "PROJECT_NAME"));
                                drInformdfet["AREA_LOCATION_CODE"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "AREA_LOCATION_CODE"));
                                drInformdfet["FCS_PROC_NAME"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "FCS_PROC_NAME"));
                                drInformdfet["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "NOTE"));
                                drInformdfet["LOT_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "LOT_NOTE"));

                                drInformdfet["CELL_DFCT_PRCS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CELL_DFCT_PRCS_CODE"));
                                string sFlag = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CNCL_FLAG"));
                                if(sFlag.ToUpper() == "TRUE")
                                {
                                    drInformdfet["CNCL_FLAG"] = "Y";
                                }
                                else
                                {
                                    drInformdfet["CNCL_FLAG"] = "N";
                                }
                                string pFlag = Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "PILOT_FLAG"));
                                if (pFlag.ToUpper() == "TRUE")
                                {
                                    drInformdfet["PILOT_FLAG"] = "Y";
                                }
                                else
                                {
                                    drInformdfet["PILOT_FLAG"] = "N";
                                }

                                drInformdfet["CELL_DEFECT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CELL_DEFECT_QTY"));
                                drInformdfet["CELL_RESTORE_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CELL_RESTORE_QTY"));
                                drInformdfet["CELL_DISPOSAL_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CELL_DISPOSAL_QTY"));

                                drInformdfet["CELL_FCS_IF_DEFECT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CELL_FCS_IF_DEFECT_QTY"));
                                drInformdfet["CELL_FCS_IF_CANCEL_DEFECT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CELL_FCS_IF_CANCEL_DEFECT_QTY"));

                                dtInformdfet.Rows.Add(drInformdfet);
                            }
                        }
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_FORM_CELL_DFCT", "INDATA,INFORMDFCT", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            else
                            {
                                Search2();
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [Biz]
        private void SetValue()
        {
            try
            {
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CLOSING_DATE", "RQSTDT", "RSLTDT", null);

                DateTime Cls_Date = Convert.ToDateTime(SearchResult.Rows[0]["CLOSING_DATE"].ToString());
                string sCls_Date = Cls_Date.Year.ToString() + "-" + Cls_Date.Month.ToString("00") + "-" + Cls_Date.Day.ToString("00");

                txtDate.Text = sCls_Date;

                _PREV_CLOSING_DATE = (dtpDateFrom.SelectedDateTime = Convert.ToDateTime(SearchResult.Rows[0]["PREV_CLOSING_DATE"])).ToString("yyyy-MM-dd");
                _CLOSING_DATE = (dtpDateTo.SelectedDateTime = Convert.ToDateTime(SearchResult.Rows[0]["CLOSING_DATE"])).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable dtLocCombo()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = sAREAID2;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        private DataTable dtTypeCombo(string sFilter)
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

            return dtResult;
        }

        private void SetProjectName(int idx, string sprodid)
        {
            string sReturnValue = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = sAREAID2;
            dr["PRODID"] = sprodid;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL_IN_AREA", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgManage.Rows[idx].DataItem, "PROJECT_NAME", Util.NVC(dtResult.Rows[0]["PRJT_NAME"]));
                DataTableConverter.SetValue(dgManage.Rows[idx].DataItem, "MDLLOT_ID", Util.NVC(dtResult.Rows[0]["MDLLOT_ID"]));
                // dgManage.GetCell(idx, dgManage.Columns["PROJECT_NAME"].Index).Value = (string)dtResult.Rows[0]["PRJT_NAME"];
                // dgManage.GetCell(idx, dgManage.Columns["MDLLOT_ID"].Index).Value = (string)dtResult.Rows[0]["MDLLOT_ID"];     
                dgManage.EndEditRow(true);
            }
        }

        private void Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PROJECT_NAME", typeof(String));
                RQSTDT.Columns.Add("AREA_LOCATION_CODE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["PRODID"] = Util.NVC(cboModel.SelectedValue) == "" ? null : Util.NVC(cboModel.SelectedValue);
                dr["PROJECT_NAME"] = txtPjt.Text.Trim() == "" ? null : txtPjt.Text;
                dr["AREA_LOCATION_CODE"] = Util.NVC(cboLocation.SelectedValue) == "" ? null : Util.NVC(cboLocation.SelectedValue);
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_CELL_DFCT_SUM", "INDATA", "OUTDATA", RQSTDT);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgHist);

                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "PROJECT_NAME", "PRODID" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["PROJECT_NAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("TOTAL") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["AREA_LOCATION_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CELL_DFCT_PRCS_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });

                }

                DataGrid_Summary();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetDetail(string sProdid)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("AREA_LOCATION_CODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                //dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["AREAID"] = sAREAID;
                dr["PRODID"] = sProdid;
               if(!string.IsNullOrWhiteSpace(_AREA_LOCATION_CODE))  dr["AREA_LOCATION_CODE"] = _AREA_LOCATION_CODE;
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_CELL_DFCT_DETAIL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgHist, dtRslt, FrameOperation, true);

                DataGrid_Summary2();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Search2()
        {
            try
            {
                Util.gridClear(dgManage);

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(String));
                RQSTDT.Columns.Add("PROJECT_NAME", typeof(String));
                RQSTDT.Columns.Add("CELL_LOCATION_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("CELL_DFCT_PRCS_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID2;
                dr["PRODID"] = Util.NVC(cboModel2.SelectedValue) == "" ? null : Util.NVC(cboModel2.SelectedValue);
                dr["MDLLOT_ID"] = Util.NVC(txtMDLLot2.Text) == "" ? null : Util.NVC(txtMDLLot2.Text);
                dr["PROJECT_NAME"] = txtPjt2.Text.Trim() == "" ? null : txtPjt2.Text;
                dr["CELL_LOCATION_CODE"] = Util.NVC(cboLocation2.SelectedValue) == "" ? null : Util.NVC(cboLocation2.SelectedValue);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["CELL_DFCT_PRCS_CODE"] = Util.NVC(cboDfctPrcsCode.SelectedValue) == "" ? null : Util.NVC(cboDfctPrcsCode.SelectedValue);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_CELL_DFCT_HIST", "INDATA", "OUTDATA", RQSTDT);

                Util.GridSetData(dgManage, dtRslt, FrameOperation, true);

               
              //  (dgManage.Columns["AREA_LOCATION_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).BeginEdit();
             //   DataTableConverter.SetValue(dgManage.Rows[0].DataItem, "AREA_LOCATION_CODE", "1동 2층");
                // Util.SetGridColumnText(dgManage, "AREA_LOCATION_CODE", null, "1동 2층", true, false, false, true, 65, HorizontalAlignment.Center, Visibility.Visible);
                //:                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", 0);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region [Func]

        private void DataGrid_Summary()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgSummary.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CELL_DFCT_PRCS_QTY"], dac);
        }

        private void DataGrid_Summary2()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgHist.Resources["ResultTemplate2"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgHist.Columns["CELL_DEFECT_QTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgHist.Columns["CELL_RESTORE_QTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgHist.Columns["CELL_DISPOSAL_QTY"], dac);
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgManage.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgManage.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgManage.Rows[i].DataItem, "CHK", true);
                }
            }

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgManage.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgManage.Rows[i].DataItem, "CHK", false);
                }
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
                dr2["CHK"] = true;
               // dr2["REG_DATE"] = DateTime.Now.ToString("yyyy-MM-dd");
                dr2["TYPE"] = "Y";
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
                if (dg.SelectedIndex > -1 && DataTableConverter.GetValue(dg.Rows[dg.SelectedIndex].DataItem, "TYPE").Equals("Y"))
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

        private void SetFindGridCombo(string bizRuleName, C1.WPF.DataGrid.DataGridColumn dgcol, string[] arrColumn, string[] arrCondition, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                    {
                        inDataTable.Columns.Add(col, typeof(string));
                    }

                    DataRow dr = inDataTable.NewRow();
                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                    {
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];
                    }
                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText });

                PopupFindDataColumn column = null;
                column = dgcol as PopupFindDataColumn;
                column.AddMemberPath = "PRODID";
                column.ItemsSource = DataTableConverter.Convert(dtBinding);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        public void LoadExcelDataFixed(C1.WPF.DataGrid.C1DataGrid dataGrid, Stream excelFileStream, int sheetNo, int headercnt)
        {
            try
            {
                DataTable dtLocComboResult = dtLocCombo();
                DataTable dtTypeComboResult = dtTypeCombo("CELL_DFCT_PRCS_CODE");

                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, C1.WPF.Excel.FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("CHK", typeof(bool));
                dataTable.Columns.Add("HIST_SEQNO", typeof(int));
                dataTable.Columns.Add("REG_DATE", typeof(string));
                dataTable.Columns.Add("PROJECT_NAME", typeof(string));
                dataTable.Columns.Add("PRODID", typeof(string));
                dataTable.Columns.Add("PRONAME", typeof(string));
                dataTable.Columns.Add("AREA_LOCATION_CODE", typeof(string));
                dataTable.Columns.Add("AREA_LOCATION_NAME", typeof(string));
                dataTable.Columns.Add("FCS_PROC_NAME", typeof(string));
                dataTable.Columns.Add("NOTE", typeof(string));
                dataTable.Columns.Add("LOT_NOTE", typeof(string));
                dataTable.Columns.Add("CELL_DFCT_PRCS_CODE", typeof(string));
                dataTable.Columns.Add("CELL_DFCT_PRCS_NAME", typeof(string));
                dataTable.Columns.Add("CNCL_FLAG", typeof(bool));
                dataTable.Columns.Add("CELL_DEFECT_QTY", typeof(Decimal));
                dataTable.Columns.Add("CELL_RESTORE_QTY", typeof(Decimal));
                dataTable.Columns.Add("CELL_DISPOSAL_QTY", typeof(Decimal));

                dataTable.Columns.Add("CELL_FCS_IF_DEFECT_QTY", typeof(Decimal));
                dataTable.Columns.Add("CELL_FCS_IF_CANCEL_DEFECT_QTY", typeof(Decimal));

                dataTable.Columns.Add("ERP_STAT_CODE", typeof(string));

                dataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                dataTable.Columns.Add("ERP_ERR_CODE", typeof(string));
                dataTable.Columns.Add("ERP_ERR_CAUSE_CNTT", typeof(string));

                dataTable.Columns.Add("TYPE", typeof(string));
                dataTable.Columns.Add("CNCL_FLAG_VALUE", typeof(string)); 

                for (int rowInx = headercnt; rowInx < sheet.Rows.Count; rowInx++)
                {
                    DataRow dataRow = dataTable.NewRow();
                    string strCheck = sheet.GetCell(rowInx, 0).Text.Trim();

                    dataRow["CHK"] = true;
                    dataRow["REG_DATE"] = DateTime.Now.ToString("yyyy-MM-dd");
                    dataRow["PROJECT_NAME"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                    dataRow["PRODID"] = Util.NVC(sheet.GetCell(rowInx, 1).Text);
                    string sAreaLocName = Util.NVC(sheet.GetCell(rowInx, 2).Text);
                    string sAreaLocCode = string.Empty;
                    dataRow["AREA_LOCATION_NAME"] = sAreaLocName;

                    if (dtLocComboResult.Rows.Count > 0)
                    {
                        string sExpression = "CBO_NAME =" + "'" + sAreaLocName + "'";
                        string sSort = "CBO_NAME DESC";
                        DataRow[] drLocComboRows = dtLocComboResult.Select(sExpression);
                        if (drLocComboRows.Length > 0)
                        {
                            // 주의  drLocComboRows[0].ItemArray[1]
                            sAreaLocCode = drLocComboRows[0].ItemArray[1].ToString();
                            dataRow["AREA_LOCATION_CODE"] = sAreaLocCode;
                        }
                    }

                    dataRow["NOTE"] = Util.NVC(sheet.GetCell(rowInx, 3).Text);
                    dataRow["LOT_NOTE"] = Util.NVC(sheet.GetCell(rowInx, 4).Text);
                    string sCellDeftPrcsName = Util.NVC(sheet.GetCell(rowInx, 5).Text);
                    string sCellDeftPrcsCode = string.Empty;
                    dataRow["CELL_DFCT_PRCS_NAME"] = sCellDeftPrcsName;

                    if (dtTypeComboResult.Rows.Count > 0)
                    {
                        string sExpression = "CBO_NAME =" + "'" + sCellDeftPrcsName + "'";
                        string sSort = "CBO_NAME DESC";
                        DataRow[] drTypeComboRows = dtTypeComboResult.Select(sExpression, sSort);
                        if (drTypeComboRows.Length > 0)
                        {
                            // 주의 drTypeComboRows[0].ItemArray[0]                       
                            sCellDeftPrcsCode = drTypeComboRows[0].ItemArray[0].ToString();
                            dataRow["CELL_DFCT_PRCS_CODE"] = sCellDeftPrcsCode;
                        }
                    }

                    dataRow["CNCL_FLAG"] = false;                                                 //  Util.NVC(sheet.GetCell(rowInx, 13).Text);
                    string sPrcsCode = dataRow["CELL_DFCT_PRCS_CODE"].ToString();

                    if (sPrcsCode.ToUpper() == "DEFECT")
                    {
                        dataRow["CELL_DEFECT_QTY"] = Util.NVC_Decimal(sheet.GetCell(rowInx, 6).Text);
                    }
                    else if (sPrcsCode.ToUpper() == "SCRAP")
                    {
                        dataRow["CELL_DISPOSAL_QTY"] = Util.NVC_Decimal(sheet.GetCell(rowInx, 6).Text);
                    }
                    else if (sPrcsCode.ToUpper() == "GOOD")
                    {
                        dataRow["CELL_RESTORE_QTY"] = Util.NVC_Decimal(sheet.GetCell(rowInx, 6).Text);
                    }
                    else if (sPrcsCode.ToUpper() == "FCS_IF_DFCT")
                    {
                        dataRow["CELL_FCS_IF_DEFECT_QTY"] = Util.NVC_Decimal(sheet.GetCell(rowInx, 6).Text);
                    }
                    else if (sPrcsCode.ToUpper() == "FCS_IF_CANCEL_DFCT")
                    {
                        dataRow["CELL_FCS_IF_CANCEL_DEFECT_QTY"] = Util.NVC_Decimal(sheet.GetCell(rowInx, 6).Text);
                    }

                    dataRow["TYPE"] = "Y"; // Util.NVC(sheet.GetCell(rowInx, 18).Text);
                    dataTable.Rows.Add(dataRow);
                }
                dataTable.AcceptChanges();
                dataGrid.ItemsSource = DataTableConverter.Convert(dataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #endregion

        private void txtMDLLot_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtMDLLot2_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
