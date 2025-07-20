/*************************************************************************************
 Created Date : 2017.03.06
      Creator : 유관수
   Decription : 전지 5MEGA-GMES 구축 - 추가기능 - 믹서 자주검사 입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.06  유관수 : Initial Created.
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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_ELEC_MIXCONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_ELEC_RESERVATION : C1Window, IWorkArea
    {
        #region < Declaration & Constructor >

        string WorkOrder = string.Empty;
        string ProductID = string.Empty;
        string EquipSegmentID = string.Empty;
        string EquipID = string.Empty;
        string WO = string.Empty;
        string ProcessID = string.Empty;
        string ERPUseFlag = string.Empty;
        string Process_Plan_Mngt_Type_Code = string.Empty;
        string FP_REF_PROCID = string.Empty;
        DataTable Source = null;
        bool Load = false;

        public C1DataGrid COLORTAG_GRID { get; set; }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion < Declaration & Constructor >


        #region < Initialize >

        public CMM_COM_ELEC_RESERVATION()
        {
            InitializeComponent();
        }
        
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeControls();
            SetWorkOrderList(Source);
            GetProcessFPInfo();
          //  Set_Combo_Ver(cboPlanVer);
            Loaded -= C1Window_Loaded;
            Load = true;
        }

        private void InitializeControls()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 10)
            {
                txtProject.Text = Util.NVC(tmps[0]);
                txtVersion.Text = Util.NVC(tmps[1]);
                txtWorkOrder.Text = Util.NVC(tmps[2]);
                txtProductID.Text = Util.NVC(tmps[3]);
                txtElectrodType.Text = Util.NVC(tmps[4]);
                txtType.Text = Util.NVC(tmps[5]);
                EquipID = Util.NVC(tmps[6]);
                EquipSegmentID = Util.NVC(tmps[7]);
                ProcessID = Util.NVC(tmps[8]);
                Source = tmps[9] as DataTable;
            }
        }

        #endregion < Initialize >


        #region < Event >

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = GetWorkOrderList();
                SetWorkOrderList(dt);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
        }

        private void btnReserve_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            if (WO.Equals("") || EquipID.Equals(""))
            {
                return;
            }

            Button btn = sender as Button;
            string Flag = btn.Tag.ToString();

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SRCTYPE"] = "UI";
            Indata["EQPTID"] = EquipID;
            Indata["WO_DETL_ID"] = WO;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["PROD_VER_CODE"] = cboPlanVer.SelectedValue;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("BR_PRD_REG_EIO_NEXT_WO_DETL_ID", "INDATA", null, IndataTable, (bizResult, bizException) =>
            {
                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

                string message = "";

                if (Flag.Equals("Y"))
                {
                    message = "SFU5031";                    
                }
                else
                {
                    message = "SFU5032";
                }
                
                Util.MessageInfo(message, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        this.DialogResult = MessageBoxResult.OK;
                    }
                });
            });
        }

        private void btnReserveCancel_Click(object sender, RoutedEventArgs e)
        {
            if (WO.Equals("") || EquipID.Equals(""))
            {
                return;
            }

            Util.MessageConfirm("SFU5037", (result) =>
            {
                if(result == MessageBoxResult.OK)
                {
                    DataTable InDataTable = new DataTable();
                    InDataTable.Columns.Add("SRCTYPE", typeof(string));
                    InDataTable.Columns.Add("EQPTID", typeof(string));
                    InDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                    InDataTable.Columns.Add("USERID", typeof(string));

                    DataRow Indata = InDataTable.NewRow();
                    Indata["SRCTYPE"] = "UI";
                    Indata["EQPTID"] = EquipID;
                    Indata["WO_DETL_ID"] = WO;
                    Indata["USERID"] = LoginInfo.USERID;
                    InDataTable.Rows.Add(Indata);

                    new ClientProxy().ExecuteService("BR_PRD_REG_EIO_NEXT_WO_DETL_ID_CANCEL", "INDATA", null, InDataTable, (bizResult, bizException) =>
                     {
                         if(bizException != null)
                         {
                             Util.MessageException(bizException);
                             return;
                         }
                         else
                         {
                             Util.MessageInfo("SFU5032", MessageResult =>
                             {
                                 this.DialogResult = MessageBoxResult.OK;
                             });
                         }
                     });

                    this.DialogResult = MessageBoxResult.OK;
                }
            });            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void chkProc_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Load)
                    return;

                DataTable dt = GetWorkOrderList();
                SetWorkOrderList(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!Load)
                    return;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (sender == null)
                        return;

                    LGCDatePicker dtPik = (sender as LGCDatePicker);

                    if (string.Equals(dtPik.Tag, "CHANGE"))
                    {
                        dtPik.Tag = null;
                        return;
                    }

                    // BASETIME 기준설정
                    DateTime currDate = DateTime.Now;
                    DateTime baseDate = DateTime.Now;
                    string sCurrTime = string.Empty;
                    string sBaseTime = string.Empty;

                    GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                    if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                        baseDate = currDate.AddDays(-1);

                    // W/O 공정인 경우에만 체크.
                    if (Process_Plan_Mngt_Type_Code.Equals("WO"))
                    {
                        if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))                        
                        {
                            dtPik.Text = baseDate.ToLongDateString();
                            dtPik.SelectedDateTime = baseDate;
                            Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                            return;
                        }
                    }
                    else
                    {                        
                        if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                        {
                            dtPik.Text = baseDate.ToLongDateString();
                            dtPik.SelectedDateTime = baseDate;
                            Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                            return;
                        }
                    }

                    if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다
                                                            //e.Handled = false;
                        return;
                    }

                    //GetWorkOrderList();
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!Load)
                    return;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (sender == null)
                        return;

                    LGCDatePicker dtPik = (sender as LGCDatePicker);

                    if (string.Equals(dtPik.Tag, "CHANGE"))
                    {
                        dtPik.Tag = null;
                        return;
                    }

                    // BASETIME 기준설정
                    DateTime currDate = DateTime.Now;
                    DateTime baseDate = DateTime.Now;
                    string sCurrTime = string.Empty;
                    string sBaseTime = string.Empty;

                    GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                    if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                        baseDate = currDate.AddDays(-1);

                    if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        baseDate = dtpDateFrom.SelectedDateTime;

                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.
                                                            //e.Handled = false;
                        return;
                    }

                    //GetWorkOrderList();
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;            
            
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
            DataRow dtRow = (rb.DataContext as DataRowView).Row;

            for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                { 
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    Set_Combo_Ver(cboPlanVer);
                }
                else
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
            }
            
            dgWorkOrder.SelectedIndex = idx;

            if(dtRow["EIO_WO_SEL_STAT"].ToString().Equals("E"))
            {
                btnReserve.IsEnabled = false;
                btnReserveCancel.IsEnabled = true;
            }
            else
            {
                btnReserve.IsEnabled = true;
                btnReserveCancel.IsEnabled = false;
            }

            WO = dtRow["WOID"].ToString();
        }

        #endregion < Event >


        #region < Mehod >
        private void Set_Combo_Ver(C1ComboBox cbo)
        {
            try
            {
                //DataTable dtRQSTDT = new DataTable();
                //dtRQSTDT.TableName = "RQSTDT";
                //dtRQSTDT.Columns.Add("PRODID", typeof(string));
                //dtRQSTDT.Columns.Add("AREAID", typeof(string));

                //DataRow drnewrow = dtRQSTDT.NewRow();


                //for (int i = 0; i < dgWorkOrder.GetRowCount(); i++)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK")).Equals("1"))
                //    {
                //        drnewrow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PRODID"));
                //    }
                //}
                //drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;

                //dtRQSTDT.Rows.Add(drnewrow);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_CONV_RATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //{
                //    if (Exception != null)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                //        return;
                //    }

                //    cbo.ItemsSource = DataTableConverter.Convert(result);
                //    cbo.SelectedIndex = 0;

                //});
                string tmpPRODID = string.Empty;

                for (int i = 0; i < dgWorkOrder.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "CHK").IsTrue())
                    {
                        tmpPRODID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[i].DataItem, "PRODID"));
                    }
                }
                const string bizRuleName = "DA_PRD_SEL_CONV_RATE";
                string[] arrColumn = { "PRODID" , "AREAID" };
                string[] arrCondition = { tmpPRODID, LoginInfo.CFG_AREA_ID };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnReserve, btnClose };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderList(DataTable dt)
        {
            try
            {
                if (dt == null) return;

                loadingIndicator.Visibility = Visibility.Visible;

                DataView view = dt.AsDataView();
                view.RowFilter = "EIO_WO_SEL_STAT <> 'Y'";
                
                if (view.Count > 0 && view[0][0].ToString() == "0")
                {
                    view[0].SetValue("CHK", 1);
                }

                Util.GridSetData(dgWorkOrder, view.ToTable(), FrameOperation, true);
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
        private bool Validation()
        {
            if (Util.NVC(cboPlanVer.SelectedValue) == "SELECT" || cboPlanVer.SelectedIndex == 0 || Util.NVC(cboPlanVer.SelectedValue) == null)
            {
                Util.MessageInfo("SFU6031");
                return false;
            }
            else
            {
                return true;
            }
        }
        private void GetProcessFPInfo()
        {
            try
            {
                DataTable InDataTable = new DataTable();
                InDataTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = InDataTable.NewRow();
                dr["PROCID"] = ProcessID;
                InDataTable.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", InDataTable);

                if (Result == null || Result.Rows.Count < 1)
                {
                    return;
                }

                // WorkOrder 사용여부
                ERPUseFlag = Util.NVC(Result.Rows[0]["ERPRPTIUSE"]);                
                Process_Plan_Mngt_Type_Code = Util.NVC(Result.Rows[0]["PLAN_MNGT_TYPE_CODE"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
        }

        private DataTable GetWorkOrderList()
        {
            try
            {
                DataTable InDataTable = new DataTable();
                InDataTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = InDataTable.NewRow();
                dr["PROCID"] = ProcessID;
                InDataTable.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", InDataTable);

                if (Result == null || Result.Rows.Count < 1)
                {
                    return null;
                }

                // WorkOrder 사용여부
                string ERPUseFlag = Util.NVC(Result.Rows[0]["ERPRPTIUSE"]);                

                if (ERPUseFlag.Equals("Y"))  // ERP 실적 전송인 경우는 Workorder Inner Join..
                {
                    if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                        Result = GetEquipmentWorkOrderByProcWithInnerJoin();
                    else
                        Result = GetEquipmentWorkOrderWithInnerJoin();
                }
                else
                {
                    if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                        Result = GetEquipmentWorkOrderByProc();
                    else
                        Result = GetEquipmentWorkOrder();
                }

                return Result;                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrder()
        {
            try
            {                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = FP_REF_PROCID.Equals("") ? ProcessID : FP_REF_PROCID;
                searchCondition["EQSGID"] = EquipSegmentID;
                searchCondition["EQPTID"] = EquipID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inDataTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_CWA", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderByProc()
        {
            try
            {                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = FP_REF_PROCID.Equals("") ? ProcessID : FP_REF_PROCID;
                searchCondition["EQSGID"] = EquipSegmentID;
                searchCondition["EQPTID"] = EquipID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inDataTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_CWA", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderWithInnerJoin()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));
                inDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = FP_REF_PROCID.Equals("") ? ProcessID : FP_REF_PROCID;
                searchCondition["EQSGID"] = EquipSegmentID;
                searchCondition["EQPTID"] = EquipID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                //if (!string.IsNullOrEmpty(_CoatSideType))
                //    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;
                inDataTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP_CWA", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetEquipmentWorkOrderByProcWithInnerJoin()
        {
            try
            {                                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));
                inDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = FP_REF_PROCID.Equals("") ? ProcessID : FP_REF_PROCID;
                searchCondition["EQSGID"] = EquipSegmentID;
                searchCondition["EQPTID"] = EquipID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                //if (!string.IsNullOrEmpty(_CoatSideType))
                //    searchCondition["COAT_SIDE_TYPE"] = _CoatSideType;

                inDataTable.Rows.Add(searchCondition);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP_CWA", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetChangeDatePlan(out DateTime currDate, out string sCurrTime, out string sBaseTime)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            currDate = GetCurrentTime();
            sCurrTime = currDate.ToString("HHmmss");
            sBaseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);
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

        #endregion < Mehod >
    }
}