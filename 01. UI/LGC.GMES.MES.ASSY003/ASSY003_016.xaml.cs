/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 이진선
   Decription : 남경 폴리머 VD공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.06.25  조영대    : 설비 Loss Level 2 Code 사용 체크 및 변환
  2024.05.07  성민식    : [E20240313-000889] ESOC2 NFF Pilot 시생산 설정/해제 추가




 
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY003
{
    public partial class ASSY003_016 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private int iTime = 0;

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        string inputLot = string.Empty;
        DataTable dtMain = new DataTable();
        CurrentLotInfo cLot = new CurrentLotInfo();

        private bool bTestMode = false;
        private string sTestModeType = string.Empty;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

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

        private string _Unit;
        private string _EQPT_ONLINE_FLAG;



        public ASSY003_016()
        {
            InitializeComponent();

            if(LoginInfo.CFG_SHOP_ID == "F030" && LoginInfo.CFG_AREA_ID == "M9")
            {
                btnPilotProdMode.Visibility = Visibility.Visible;
            }

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
            try
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

                                // 동일 배치 전체 선택
                                if (!chk.IsChecked.Value)
                                {
                                    chkAll.IsChecked = false;
                                    ClearData();


                                    // 진행중인 경우..
                                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("PROC"))
                                    {
                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "EQPT_BTCH_WRK_NO")).Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO"))))
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    // 진행중인 경우..
                                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("PROC"))
                                    {
                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "EQPT_BTCH_WRK_NO")).Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO"))))
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
                                            }
                                            else
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }

                                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                                if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
            try
            {
                iTime = 0;
                btnReayCancel.Visibility = Visibility.Collapsed;

                GetProductLot();
                getworkorder();
                ClearData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dg_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    if (cell == null || cell.Presenter == null || cell.Presenter.Content == null) return;
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CHK")).Equals("1"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem , "WIPSTAT")).Equals("RESERVE"))
                        {
                            ProcessDetail();
                        }

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

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                    {
                        Util.MessageValidation("SFU4286"); // 이미 진행 상태인 LOT 입니다.
                        return;
                    }
                }
                else
                {
                    Util.MessageValidation("SFU4287"); // 모두 선택 후 진행 하세요.
                    return;
                }
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

                    //if (_Unit.Equals("LOT"))
                    //{

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
                    //}
                    //else
                    //{
                    //    //소형VD
                    //    if (dgLotInfo.ItemsSource == null) return;
                    //    List<System.Data.DataRow> list = DataTableConverter.Convert(dgLotInfo.ItemsSource).Select("CHK = 'True' or CHK = '1'").ToList();
                    //    list = list.GroupBy(c => c["CSTID"]).Select(group => group.First()).ToList();
                    //    for (int i = 0; i < list.Count; i++)
                    //    {
                    //        row = inMtrl.NewRow();
                    //        row["LOTID"] = list[i]["CSTID"];
                    //        indataSet.Tables["IN_LOT"].Rows.Add(row);
                    //    }
                    //}

                    try
                    {
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_VD_S", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
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
            
            string sTmp = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("RESERVE") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("READY"))
                    {
                        //Util.Alert("작업시작 후 시작 취소 할 수 있습니다.");
                        Util.MessageValidation("SFU3035");
                        return;
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    {
                        Util.MessageValidation("SFU4288", Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));  // 선택한 Lot[%]은 동일한 작업배치번호가 아닙니다. 같은 작업배치번호 단위로 시작취소할 수 있습니다.
                        return;
                    }
                }
                else
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    {
                        Util.MessageValidation("SFU4289");  // 동일한 작업배치번호를 모두 선택 후 시작취소하세요.
                        return;
                    }
                }
            } 

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
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
                        new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_START_LOT_VD_NJ", "RQSTDT", null, dt);
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업 시작 취소"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                        Util.MessageInfo("SFU1839");
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        #endregion



        #region Event


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitCombo();

            ApplyPermissions();

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

            string ValueToCondition = string.Empty;
            var sCond = new List<string>();

            if ((bool)chkReserve.IsChecked)
            {
                sCond.Add(chkReserve.Tag.ToString() + ",READY");

                if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                    dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Collapsed;
            }

            else if((bool)chkRun.IsChecked)
            {
                sCond.Add(chkRun.Tag.ToString());

                if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                    dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Visible;
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
                searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode();
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

        private string GetFpPlanGnrtBasCode()
        {
            try
            {
                string sPlanType = "";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                dr["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_PLAN_GNRT_BAS_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("FP_PLAN_GNRT_BAS_CODE"))
                {
                    if (Util.NVC(dtResult.Rows[0]["FP_PLAN_GNRT_BAS_CODE"]).Equals("E"))
                        sPlanType = "EQPT";
                    else
                        sPlanType = "LINE";
                }

                return sPlanType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "LINE";
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
                Util.MessageValidation("SFU1261"); //선택된LOT이 없습니다
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

            //if (Util.NVC(cboVDProcess.SelectedValue).Equals("A6000"))//파우치
            //{
            //    _Unit = "LOT";
            //    dgcSKIDID.Visibility = Visibility.Collapsed;
            //}
            //else //원각형
            //{
            //    // 원각 사용자 교육 중 랏단위로 작업 하겠다고 하여 임시 처리.(2017.11.01)
            //    if (GetUseSkid())
            //    {
            //        _Unit = "SKID";
            //        dgcSKIDID.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        _Unit = "LOT";
            //        dgcSKIDID.Visibility = Visibility.Collapsed;
            //    }                    
            //}
        }
        private bool GetUseSkid()
        {
            try
            {
                bool bRet = false;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CR_VD_SKID_USE_YN_NJ", "RQSTDT", "RSLTDT", null);

                if (dtResult != null && dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["NJ_VD_CR_SKID_USE_FLAG"]).Equals("Y"))
                {
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidConfirm()) return;

            Util.MessageConfirm("SFU1744", (result) => //완공처리 하시겠습니까?
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
                        
                        ineqp.Columns.Add("PROD_VER_CODE", typeof(string));
                        ineqp.Columns.Add("SHIFT", typeof(string));
                        ineqp.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        ineqp.Columns.Add("WRK_USER_NAME", typeof(string));
                        ineqp.Columns.Add("WIPNOTE", typeof(string));
                        ineqp.Columns.Add("USERID", typeof(string));
                        ineqp.Columns.Add("SHOPID", typeof(string));



                        DataRow row = null;

                        row = ineqp.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                        row["PROD_VER_CODE"] = _VERSION;
                        row["SHIFT"] = "";
                        row["WIPDTTM_ED"] = Convert.ToDateTime(GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss"));
                        row["WRK_USER_NAME"] = LoginInfo.USERID;
                        row["WIPNOTE"] = txtRemark.Text;
                        row["USERID"] = LoginInfo.USERID;
                        row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable INLOT = indataSet.Tables.Add("INLOT");
                        INLOT.Columns.Add("LOTID", typeof(string));
                        INLOT.Columns.Add("INPUTQTY", typeof(string));
                        INLOT.Columns.Add("OUTPUTQTY", typeof(string));
                        INLOT.Columns.Add("RESNQTY", typeof(string));


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
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_VD_NJ", "INDATA,INLOT", null, (bizResult, bizException) =>
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

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU1661"); //선택 된 LOT이 없습니다.
                return false;

            }
            if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "WIPSTAT")).Equals("RESERVE") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "WIPSTAT")).Equals("READY"))
            {
                Util.MessageValidation("SFU3036"); //작업시작 후 실적확정 해주세요
                return false;
            }

            string sTmp = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    {
                        Util.MessageValidation("SFU4288", Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));  // 선택한 Lot[%1]은 동일한 작업배치번호가 아닙니다. 같은 작업배치번호 단위로 진행할 수 있습니다.
                        return false;
                    }
                }
                else
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    {
                        Util.MessageValidation("SFU4289");  // 동일한 작업배치번호를 모두 선택 후 진행 하세요.
                        return false;
                    }
                }
            }

            return true;

        }


        #region 계획정지, 테스트 Run, 시생산
        private bool CheckProcessWip(out string processLotId)
        {
            processLotId = string.Empty;

            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                dtRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    processLotId = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationPilotProdMode()
        {
            if (string.IsNullOrEmpty(cboVDEquipmentSegment.SelectedValue.ToString()))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(cboVDEquipment.SelectedValue.ToString()))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            string processLotId = string.Empty;
            if (CheckProcessWip(out processLotId))
            {
                Util.MessageValidation("SFU3199", processLotId); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
                return false;
            }

            return true;
        }

        private string GetPilotProdMode()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                //if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("PILOT_PROD_MODE"))
                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("EQPT_OPER_MODE"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"])))
                    {
                        //ShowPilotProdMode();
                        return Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"]);
                    }
                    else
                    {
                        //HidePilotProdMode();
                        return string.Empty;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private bool SetPilotProdMode(string sMode)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PILOT_PRDC_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                newRow["PILOT_PRDC_MODE"] = sMode; //isMode ? "PILOT" : "";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097"); // 변경되었습니다.
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void btnPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;
            
            string sFromMode = GetPilotProdMode();
            string messageCode;
            string sToMode = string.Empty;

            if (sFromMode.Equals("PILOT"))
            {
                messageCode = "SFU8304";    //[%1]을 시생산 해제하시겠습니까?
                sToMode = string.Empty;
            }
            else
            {
                messageCode = "SFU8303";    //[%1]을 시생산 설정하시겠습니까?
                sToMode = "PILOT";
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (SetPilotProdMode(sToMode) == false)
                    {
                        return;
                    }
                    Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                }
            }, cboVDEquipment.SelectedValue.ToString());
        }

        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTestMode()) return;

            if (bTestMode)
            {
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

                        SetTestMode(true);
                        GetTestMode();
                    }
                });
            }
        }

        private void btnScheduledShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanScheduledShutdownMode()) return;

                if (bTestMode)
                {
                    SetTestMode(false, bShutdownMode: true);
                    GetTestMode();
                }
                else
                {
                    Util.MessageConfirm("SFU4460", (result) => // 계획정지를 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

                            SetTestMode(true, bShutdownMode: true);
                            GetTestMode();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanTestMode()
        {
            bool bRet = false;

            if (cboVDEquipmentSegment.SelectedIndex < 0 || cboVDEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboVDEquipment.SelectedIndex < 0 || cboVDEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (bScheduledShutdown)
            //{
            //    Util.MessageValidation("SFU4464"); // 계획정지중 입니다. 계획정지를 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CanScheduledShutdownMode()
        {
            bool bRet = false;

            if (cboVDEquipmentSegment.SelectedIndex < 0 || cboVDEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboVDEquipment.SelectedIndex < 0 || cboVDEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (bTestMode)
            //{
            //    Util.MessageValidation("SFU4465"); // 테스트 Run 중 입니다. 테스트 Run 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void HideScheduledShutdown()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowScheduledShutdown()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(false);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(false);
        }

        private void HideScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ColorAnimationInRectangle(bool bTest)
        {
            try
            {
                string sname = string.Empty;
                if (bTest)
                {
                    recTestMode.Fill = redBrush;
                    sname = "redBrush";
                }
                else
                {
                    recTestMode.Fill = yellowBrush;
                    sname = "yellowBrush";
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1.0;
                opacityAnimation.To = 0.0;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
                opacityAnimation.AutoReverse = true;
                opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTargetName(opacityAnimation, sname);
                Storyboard.SetTargetProperty(
                    opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowTestMode()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(true);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(true);
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private bool SetTestMode(bool bOn, bool bShutdownMode = false)
        {
            try
            {
                string sBizName = string.Empty;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_MODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bShutdownMode)
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE_LOSS";

                    newRow["IFMODE"] = "ON";
                    newRow["UI_LOSS_MODE"] = bOn ? "ON" : "OFF";
                    newRow["UI_LOSS_CODE"] = bOn ? Util.ConvertEqptLossLevel2Change("LC003") : ""; // 계획정지 loss 코드.
                }
                else
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE";

                    newRow["IFMODE"] = bOn ? "TEST" : "ON";
                }

                newRow["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(sBizName, "IN_EQP", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboVDEquipment == null || cboVDEquipment.SelectedValue == null) return;
                if (Util.NVC(cboVDEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboVDEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO_S", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN"))
                {
                    sTestModeType = Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        //HideTestMode();

                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            ShowScheduledShutdown();
                        }
                        else
                        {
                            HideScheduledShutdown();
                        }
                    }
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
            txtRemark.Text = "";
            txtStartTime.Text = "";
            txtWorkdate.Text = "";
            txtWorkMinute.Text = "";
            chkAll.IsChecked = false;

        }
       

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboVDEquipment.SelectedValue == null || cboVDProcess.SelectedValue == null) { return; }
            getworkorder();
        }

        private void cboVDEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

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


                //_Unit = Convert.ToString(result.Rows[0]["WRK_RSV_UNIT_CODE"]);

                //if (_Unit.Equals("LOT"))
                //{
                //    dgcSKIDID.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    dgcSKIDID.Visibility = Visibility.Visible;
                //}

                _EQPT_ONLINE_FLAG = "Y";
                if (!Convert.ToString(result.Rows[0]["EQPT_ONLINE_FLAG"]).Equals(""))
                {
                    _EQPT_ONLINE_FLAG = Convert.ToString(result.Rows[0]["EQPT_ONLINE_FLAG"]);
                }

                if (_EQPT_ONLINE_FLAG.Equals("N"))
                {
                    btnRunStart.Visibility = Visibility.Collapsed;
                    btnRunCancel.Visibility = Visibility.Visible;
                    btnReayCancel.Visibility = Visibility.Collapsed;


                    chkReserve.IsChecked = false;
                    chkRun.IsChecked = true;

                    btnSearch_Click(null, null);
                    // chkRun_Click(null, null);

                }
                else
                {
                    btnRunStart.Visibility = Visibility.Visible;
                    btnRunCancel.Visibility = Visibility.Visible;
                    btnReayCancel.Visibility = Visibility.Collapsed;


                    chkReserve.IsChecked = true;
                    chkRun.IsChecked = false;

                    btnSearch_Click(null, null);
                    // chkRun_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                else
                {
                    Util.MessageValidation("SFU4287"); // 모두 선택 후 진행 하세요.
                    return;
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

        private void chkShowSkid_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if ((sender as CheckBox).IsChecked.HasValue && (bool)(sender as CheckBox).IsChecked)
                {
                    dgcSKIDID.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkShowSkid_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if ((sender as CheckBox).IsChecked.HasValue && !(bool)(sender as CheckBox).IsChecked)
                {
                    dgcSKIDID.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Run_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
               //(Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
               //(Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
               )
            {
                if (chkShowSkid != null && chkShowSkid.Visibility == Visibility.Collapsed)
                {
                    chkShowSkid.Visibility = Visibility.Visible;
                }
                else if (chkShowSkid != null && chkShowSkid.Visibility == Visibility.Visible)
                {
                    chkShowSkid.Visibility = Visibility.Collapsed;
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (iTime > 100)
                    iTime = 0;

                iTime++;

                if (iTime > 9)
                {
                    btnReayCancel.Visibility = Visibility.Visible;
                    iTime = 0;
                }
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);
            
            //HideTestMode();
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }
    }
}
