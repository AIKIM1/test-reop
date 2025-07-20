/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.07.07  이상훈    C20200619-000144 설비 선택 후 자동 조회 기능




 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        string inputLot = string.Empty;
        DataTable dtMain = new DataTable();
        CurrentLotInfo cLot = new CurrentLotInfo();

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty; //투입부
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부


        #region CurrentLotInfo
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _txtShift = string.Empty;
        private string _txtWorker = string.Empty;
        private string _predver = string.Empty;
        private string _wipdttm = string.Empty;
        private string _wipnote = string.Empty;
        private string _resonqty = "0";
        private string _inputqty = string.Empty;
        private string _wipqty = string.Empty;
        private string _txtissue = string.Empty;
        private string cancelflag = string.Empty;
        private string _REMARK = string.Empty;
        private string _VERSION = string.Empty;
        #endregion

        private string _Unit = string.Empty;



        public ASSY001_002()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
      

        private void InitCombo()
        {
            string[] sFilter = { "A1000," + Process.VD_LMN + "," + Process.VD_ELEC, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
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


        private void dgData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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



        private void dgData_CommittedEdit(object sender, DataGridCellEventArgs e)
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

                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                            if (!chk.IsChecked.Value)
                            {
                                chkAll.IsChecked = false;
                                ClearData();

                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll.IsChecked = true;
                            }

                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            break;
                    }
                }
            }


        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            checkAllProcess();
            if ((bool)chkRun.IsChecked)
            {
                ProcessDetail();
            }

        }

        private void checkAllProcess()
        {
            if (dgLotInfo == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = true;
               // C1.WPF.DataGrid.DataGridRow row = dgLotInfo.Rows[idx];
               // DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {

            uncheckProcess();

        }
        private void uncheckProcess()
        {
            if (dgLotInfo == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
            ClearData();
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            GetProductLot();
            getworkorder();
            ClearData();
        }


        private void dg_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CHK")).Equals("1"))
                    {
                        //chkAll.IsChecked = true;
                        if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem , "WIPSTAT")).Equals("RESERVE"))
                        {
                            ProcessDetail();
                        }

                    } else
                    {
                     //   chkAll.IsChecked = false;
                      //  uncheckProcess();
                        
                    }
                   
                }
            }));

          
        }




        #endregion

       

        #region MainWindow
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                //Util.Alert("선택 된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return;
            }
            Util.MessageConfirm("SFU1435", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("EQPTID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));



                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                    row["USERID"] = LoginInfo.USERID;
                    row["IFMODE"] = "OFF";
                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inMtrl = indataSet.Tables.Add("IN_LOT");
                    inMtrl.Columns.Add("LOTID", typeof(string));

                    DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                    if (_Unit.Equals("LOT"))
                    {

                        foreach (DataRow _iRow in dtProductLot.Rows)
                        {
                            string a = _iRow["CHK"].ToString();
                            if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                            {
                                row = inMtrl.NewRow();

                                row["LOTID"] = _iRow["LOTID"];
                                indataSet.Tables["IN_LOT"].Rows.Add(row);
                            }
                        }
                    }
                    else
                    {
                        //소형VD
                        if (dgLotInfo.ItemsSource == null) return;
                        List<System.Data.DataRow> list = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select("CHK = 'True' or CHK = '1'").ToList();
                        list = list.GroupBy(c => c["CSTID"]).Select(group => group.First()).ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            row = inMtrl.NewRow();
                            row["LOTID"] = list[i]["CSTID"];
                            indataSet.Tables["IN_LOT"].Rows.Add(row);
                        }
                    }

                    try
                    {
                        ShowLoadingIndicator();

                        string bizRuleName = "BR_PRD_REG_START_LOT_VD";

                        if ((_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID")) && (LoginInfo.CFG_AREA_ID == "A3"))
                        {
                            bizRuleName = "BR_PRD_REG_START_LOT_VD_NA";
                        }

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }



                                Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.

                                _LOTID = string.Empty;
                                chkRun.IsChecked = true;
                                chkReserve.IsChecked = false;
                                GetProductLot();
                                chkAll.IsChecked = false;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);

                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
         
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                //Util.Alert("선택된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return;
            }
            if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "WIPSTAT")).Equals("RESERVE") ||  Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "WIPSTAT")).Equals("READY"))
            {
                //Util.Alert("작업시작 후 시작 취소 할 수 있습니다.");
                Util.MessageValidation("SFU3035");
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("SRCTYPE", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("USERID", typeof(string));
            dt.Columns.Add("IFMODE", typeof(string));



            DataRow row = dt.NewRow();
            DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            foreach (DataRow _iRow in dtProductLot.Rows)
            {

                if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                {
                    row = dt.NewRow();
                    row["LOTID"] = _iRow["LOTID"];
                    row["SRCTYPE"] = "UI";
                    row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                    row["USERID"] = LoginInfo.USERID;
                    row["IFMODE"] = "OFF";
                    dt.Rows.Add(row);
                }

            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_START_LOT_VD", "RQSTDT", null, dt);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업 시작 취소"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageInfo("SFU1839");
                GetProductLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion



        #region Event


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitCombo();

            ApplyPermissions();

            if (_Unit.Equals("LOT"))
            {
                SKIDID.Visibility = Visibility.Hidden;
            }
            else
            {
                SKIDID.Visibility = Visibility.Visible;
            }

            if (cboVDProcess.Equals(Process.VD_ELEC))
            {
                tbShift.Visibility = Visibility.Visible;
                tbWorker.Visibility = Visibility.Visible;

                inputShift.Visibility = Visibility.Visible;
                inputWorker.Visibility = Visibility.Visible;
            }
        }





        #endregion


        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }



        private void GetProductLot()
        {
            GetLotIdentBasCode();

            string ValueToCondition = string.Empty;
            var sCond = new List<string>();

            if ((bool)chkReserve.IsChecked)
            {
                sCond.Add(chkReserve.Tag.ToString() + ",READY");
            }

            else if((bool)chkRun.IsChecked)
            {
                sCond.Add(chkRun.Tag.ToString());
            }

            ValueToCondition = string.Join(",", sCond);

            if (ValueToCondition.Trim().Equals(""))
            {
                //Util.AlertInfo("WIP상태를 선택 하십시오");
                Util.MessageValidation("SFU1438");
                return;
            }


            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("WIPSTAT", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
            newRow["PROCID"] = cboVDProcess.SelectedValue.ToString();
            newRow["WIPSTAT"] = ValueToCondition;

            txtRemark.Text = _REMARK;

            inTable.Rows.Add(newRow);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_LOTINFO", "INDATA", "OUTDATA", inTable);

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                dgLotInfo.Columns["CSTID_O"].Visibility = Visibility.Visible;
            }
            else
            {
                dgLotInfo.Columns["CSTID_O"].Visibility = Visibility.Collapsed;
            }

            Util.GridSetData(dgLotInfo, dtMain, FrameOperation);


        }

        private void getworkorder()
        {
            try
            {
                ShowLoadingIndicator();


                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_LINE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                searchCondition["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                searchCondition["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["PROC_EQPT_FLAG"] = "LINE"; // chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked ? "PROC" : "EQPT";
                searchCondition["OTHER_EQSGID"] = "";

                inTable.Rows.Add(searchCondition);


                new ClientProxy().ExecuteService("DA_PRD_SEL_WORKORDER_LIST_WITH_FP_BY_LINE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWorkOrder, searchResult, FrameOperation, true);
                       // dgWorkOrder.ItemsSource = DataTableConverter.Convert(searchResult);
                        for (int i = 0; i < dgWorkOrder.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "EIO_WO_SEL_STAT")).Equals("Y"))
                            {
                                dgWorkOrder.SelectedIndex = i;
                                dgWorkOrder.FrozenTopRowsCount = 1;
                                return;
                            }
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
            );
                dgWorkOrder.ItemsSource = DataTableConverter.Convert(dtMain);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }



        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            if (!_Util.GetDataGridCheckValue(dgLotInfo, "CHK", iRow))
            {
                return;
            }

        }

        private void ProcessDetail()
        {
            if (dgLotInfo == null) { return; }
            if (dgLotInfo.Rows.Count == 0) { return; }
            cLot.ENDTIME_CHAR = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            int idx = _Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK");
            if (idx < 0)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된LOT이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageValidation("SFU1261");
                return;
            }

            cLot.LOTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID"));
            cLot.INPUTQTY = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPQTY"));
            cLot.PRODID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID"));
            cLot.EQPTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID"));
            cLot.STARTTIME_CHAR = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "STARTTIME"));
            cLot.STATUS = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPSTAT"));
            _predver = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PROD_VER_CODE"));
            _VERSION = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PROD_VER_CODE"));

            if (cLot.STATUS.Equals("EQPT_END"))
            {
                cLot.ENDTIME_CHAR = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "ENDTIME"));
            }

            cLot.EQPTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "EQPTID"));
            txtWorkdate.Text = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CALDATE"));


            txtStartTime.Text = cLot.STARTTIME_CHAR;

            FillLotInfo();


        }

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //공정
            String[] sFilter = { cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

        }
        private void cboVDProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }


        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT wndPopup = new CMM001.Popup.CMM_SHIFT();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                Parameters[3] = Convert.ToString(cboVDProcess.SelectedValue);

               
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT window = sender as CMM001.Popup.CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(txtShift.Text) == string.Empty)
            {
                // 선택된 작업조가 없습니다.
                Util.MessageValidation("SFU1646");
                return;
            }

            CMM001.Popup.CMM_SHIFT_USER wndPopup = new CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Convert.ToString(cboVDProcess.SelectedValue);
                Parameters[4] = Util.NVC(txtShift.Text);
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = wndPopup.USERNAME;
            }
        }


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidConfirm()) return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("완공처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1744", (result) =>
            {

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable ineqp = indataSet.Tables.Add("INDATA");
                        ineqp.Columns.Add("SRCTYPE", typeof(string));
                        ineqp.Columns.Add("IFMODE", typeof(string));
                        ineqp.Columns.Add("EQPTID", typeof(string));
                        ineqp.Columns.Add("USERID", typeof(string));
                        ineqp.Columns.Add("PROD_VER_CODE", typeof(string));
                        ineqp.Columns.Add("SHIFT", typeof(string));
                        ineqp.Columns.Add("WRK_USER_NAME", typeof(string));
                        ineqp.Columns.Add("WIPNOTE", typeof(string));



                        DataRow row = null;

                        row = ineqp.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_VER_CODE"] = _VERSION;
                        row["SHIFT"] = txtShift.Tag;
                        row["WRK_USER_NAME"] = txtWorker.Text;
                        row["WIPNOTE"] = txtRemark.Text;
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable INLOT = indataSet.Tables.Add("INLOT");
                        INLOT.Columns.Add("LOTID", typeof(string));
                        INLOT.Columns.Add("INPUTQTY", typeof(decimal));
                        INLOT.Columns.Add("OUTPUTQTY", typeof(decimal));
                        INLOT.Columns.Add("RESNQTY", typeof(decimal));


                        DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                        foreach (DataRow _iRow in dtProductLot.Rows)
                        {

                            if (_iRow["CHK"].ToString().Equals("1") || _iRow["CHK"].ToString().Equals("True"))
                            {
                                row = INLOT.NewRow();
                                row["LOTID"] = _iRow["LOTID"];
                                row["RESNQTY"] = _resonqty;
                                row["INPUTQTY"] = _iRow["WIPQTY"];
                                row["OUTPUTQTY"] = _iRow["WIPQTY"];
                                row["RESNQTY"] = dgLotInfo.GetRowCount();
                                indataSet.Tables["INLOT"].Rows.Add(row);
                            }
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_VD", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }


                                GetProductLot();
                                ClearData();


                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);

                      
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                    
            });
        }

        private bool ValidConfirm()
        {
            if (txtShift.Text.Equals(""))
            {
             //   Util.Alert("근무조를 선택해주세요");
               // return false;

            }
            if (txtWorker.Text.Equals(""))
            {
              //  Util.Alert("근무자를 선택해주세요");
              //  return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                //Util.Alert("선택 된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return false;

            }
            if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "WIPSTAT")).Equals("RESERVE") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "WIPSTAT")).Equals("READY"))
            {
                //Util.Alert("작업시작 후 실적확정 해주세요.");
                Util.MessageValidation("SFU3036");
                return false;
            }



            return true;

        }
     

        private void FillLotInfo()
        {
            try
            {

                double oper = 0;
                if (!cLot.STARTTIME_CHAR.Equals("") && !cLot.ENDTIME_CHAR.Equals(""))
                {

                    oper =
                        Math.Truncate(
                            DateTime.Parse(cLot.ENDTIME_CHAR).Subtract(DateTime.Parse(cLot.STARTTIME_CHAR)).TotalMinutes);
                    if (oper < 0)
                    {
                        //Util.Alert("종료시간이 시작시간보다 전 시간 일 수 없습니다.");
                        Util.MessageValidation("SFU3037");
                        return;
                    }
                    txtWorkMinute.Text = oper.ToString();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// CST ID VISIBLE 처리 (C20211126-000384)
        /// </summary>
        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.VD_LMN;
                dtRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        private void chkReserve_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkReserve.IsChecked)
                chkReserve.IsChecked = true;
            chkRun.IsChecked = false;
        }

        private void chkRun_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkRun.IsChecked)
                chkReserve.IsChecked = false;
            chkRun.IsChecked = true;
        }



        private void ClearData()
        {
            _LOTID = "";
            _EQPTID = "";
            _WIPSTAT = "";
            _txtShift = "";
            _txtWorker = "";
            _predver = "";
            _wipdttm = "";
            _wipnote = "";
            _inputqty = "";
            _wipqty = "";
            _txtissue = "";
            cancelflag = "";
            _REMARK = "";
            _VERSION = "";
            txtWorkdate.Text = "";
            txtShift.Text = "";
            txtRemark.Text = "";
            txtStartTime.Text = "";
            txtWorkdate.Text = "";
           // txtWorkorder.Text = "";
            //txtLotStatus.Text = "";
            txtWorkMinute.Text = "";
            chkAll.IsChecked = false;
            txtWorker.Text = "";

        }

        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            FillLotInfo();
        }

        private void teTimeEditor_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            FillLotInfo();
           
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboVDEquipment.SelectedValue == null || cboVDProcess.SelectedValue == null) { return; }
            getworkorder();
        }

        private void cboVDEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EQPTID", typeof(string));

            DataRow row = dt.NewRow();
            row["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_VD_INFO", "INDATA", "RSLTDT", dt);
            if (result == null)
            {
                //Util.Alert("설비 정보가 없습니다.");
                Util.MessageValidation("SFU1672");
                return;
            }
            if (result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1672");
                return;
            }

            _Unit = Convert.ToString(result.Rows[0]["WRK_RSV_UNIT_CODE"]);

            //C20200619-000144 조회 기능 추가
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                btnSearch_Click(null, null);
            }));

        }

        private void btnReayCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU1661"); //선택된 LOT이 없습니다.
                return;
            }

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("READY"))
                    {
                        Util.MessageValidation("SFU3498"); //착공대기 상태만 취소 가능 합니다.
                        return;
                    }
                }
            }
            try
            {
               

                DataSet ds = new DataSet();
                DataTable IN_EQP = ds.Tables.Add("IN_EQP");
                IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                IN_EQP.Columns.Add("IFMODE", typeof(string));
                IN_EQP.Columns.Add("USERID", typeof(string));

                DataRow row = IN_EQP.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;

                IN_EQP.Rows.Add(row);

                DataTable IN_LOT = ds.Tables.Add("IN_LOT");
                IN_LOT.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = IN_LOT.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));

                        IN_LOT.Rows.Add(row);
                    }
                }

                //ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_READY_LOT_VD", "IN_EQP,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//레디 취소 완료
                        GetProductLot();




                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }   
    }
}
