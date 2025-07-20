/*************************************************************************************
 Created Date : 2016.12.05
      Creator : cnslss
   Decription : ERP 실적 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.10.17  정재홍    : [C20220831-000559] - ERP 생산실적 화면 개선
  2024.01.12  남기운    : ERP 오류 유형 추가
  2025.02.26  이민형    : Date Control 짤리는 부분 수정
  2025.05.08  이민형    : NG, FAIL 추가 - ESHD법인 PI팀 이준표 책임님 요청
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_029 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string _STATUS = string.Empty;
        private string _LOTID = string.Empty;
        private string _Unit = string.Empty;
        private string _ErrorExc = string.Empty;
        private string _ = string.Empty;

        public COM001_029()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            SetEvent();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            //dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            //dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");

                    //dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);

                    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                    if (LGCdp.Name.Equals("dtpDateTo"))
                        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                    else
                        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

                //// To 일자 변경시 From일자 1일자로 변경
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
        }

        //private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LGCDatePicker dtPik = sender as LGCDatePicker;
        //    if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
        //    {
        //        dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
        //        return;
        //    }
        //}

        //private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LGCDatePicker dtPik = sender as LGCDatePicker;
        //    if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
        //    {
        //        dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
        //        return;
        //    }
        //}

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReserve);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //Shop
            C1ComboBox[] cboShopChild = { cboArea };
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sCase: "SHOP_AUTH");

            //동
            C1ComboBox[] cboAreaParent = { cboShop };
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, cbParent: cboAreaParent);
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbParent: cboAreaParent);

            ////라인
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboShift, cboProcess, cboEquipment, cboStatus };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            ////공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboShift, cboEquipment, cboStatus };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            ////설비
            //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            //C1ComboBox[] cboEquipmentChild = { cboShift, cboStatus };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentChild, cbParent: cboEquipmentParent);

            //작업조
            C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess };
            C1ComboBox[] cboShiftChild = { cboStatus };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbChild: cboShiftChild, cbParent: cboShiftParent);

            //결과
            _combo.SetCombo(cboStatus, CommonCombo.ComboStatus.ALL, sCase: "CboErpStaus");
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
            try
            {
                DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (Util.NVC(row["CMCDNAME"]).Equals("NG") 
                        || Util.NVC(row["CMCDNAME"]).Equals("FAIL"))
                    {
                        for (int idx = 0; idx < dgErpHist.Rows.Count; idx++)
                        {
                            row["CHK"] = true;
                        }
                    }
                }
            }
            catch
            {
            }  
        }


        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgErpHist.Rows.Count; idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgErpHist.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }
        #endregion

        #region Event
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    Set_Combo_Process(cboProcess);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                }
            }));
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_Equipment(cboEquipment);
                }
            }));
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    Set_Combo_Equipment(cboEquipment);
                }
            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (Convert.ToDecimal(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            //{
            //    Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
            //    return;
            //}

            GetList();

        }

        private void btnReserve_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgErpHist, "CHK");

            if (idx < 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            ErrorMessage();
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            ClearGrid();
            Util.gridClear(dgErpHist);
            string sArea = cboArea.SelectedValue.ToString();

            if (sArea == "M1" || sArea == "M2" || sArea == "A1" || sArea == "A2" || sArea == "S2")
            {
                ASSYERP();
            }
            else
            {
                REMAINERP();
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

        private void Set_Combo_Process(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = cboArea.SelectedValue == null ? "" : cboArea.SelectedValue.ToString();
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);

                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_PROC_ID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = cboArea.SelectedValue == null ? "" : cboArea.SelectedValue.ToString();
                drnewrow["PROCID"] = cboProcess.SelectedValue == null ? "" : cboProcess.SelectedValue.ToString();
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();
                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboEquipmentSegment_SelectedItemChanged(cbo, null);
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_Equipment(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue == null ? "" : cboEquipmentSegment.SelectedValue.ToString();
                drnewrow["PROCID"] = cboProcess.SelectedValue == null ? "" : cboProcess.SelectedValue.ToString(); 
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQPT_ID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void REMAINERP()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("SHIFT_ID", typeof(string));
                dtRqst.Columns.Add("STATUSID", typeof(string));
                dtRqst.Columns.Add("USER_CHK_FLAG", typeof(string));
                dtRqst.Columns.Add("TOP_ROW", typeof(Int16));
                dtRqst.Columns.Add("CNCL_FLAG", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("ACT_CHK_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                dr["FROM_DATE"] = chkError.IsChecked == true ? null : dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = chkError.IsChecked == true ? null : dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["SHOPID"] = Util.GetCondition(cboShop, bAllNull: true);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["SHIFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                dr["STATUSID"] = chkError.IsChecked == true ? "FAIL" : Util.GetCondition(cboStatus, bAllNull: true);
                dr["USER_CHK_FLAG"] = chkError.IsChecked == true ? "N" : null;
                dr["TOP_ROW"] = txtCount.Value;
                dr["CNCL_FLAG"] = chkCNCL_FLAG.IsChecked == true ? "Y" : null;
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                dr["ACT_CHK_FLAG"] = chkActivity.IsChecked == true ? "Y" : null;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_ERP_HIST", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        Util.GridSetData(dgErpHist, result, FrameOperation, true);
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

                string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME", "EQPTNAME", "PRODNAME", "UNIT_CODE", "WOID", "LOTID" };

                _Util.SetDataGridMergeExtensionCol(dgErpHist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ASSYERP()
        {
            string sStatus = cboStatus.SelectedValue.ToString();
            string SProcess = cboProcess.SelectedValue.ToString();
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("SHIFT_ID", typeof(string));
                dtRqst.Columns.Add("STATUSID", typeof(string));
                dtRqst.Columns.Add("USER_CHK_FLAG", typeof(string));
                dtRqst.Columns.Add("TOP_ROW", typeof(Int16));
                dtRqst.Columns.Add("CNCL_FLAG", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("ACT_CHK_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;                
                dr["FROM_DATE"] = chkError.IsChecked == true ? null : dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = chkError.IsChecked == true ? null : dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["SHOPID"] = Util.GetCondition(cboShop, bAllNull: true);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["SHIFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                //dr["STATUSID"] = Util.GetCondition(cboStatus, bAllNull: true);
                dr["STATUSID"] = chkError.IsChecked == true ? "FAIL" : Util.GetCondition(cboStatus, bAllNull: true);
                dr["USER_CHK_FLAG"] = chkError.IsChecked == true ? "N" : null;
                dr["TOP_ROW"] = txtCount.Value;
                dr["CNCL_FLAG"] = chkCNCL_FLAG.IsChecked == true ? "Y" : null;
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                dr["ACT_CHK_FLAG"] = chkActivity.IsChecked == true ? "Y" : null;

                dtRqst.Rows.Add(dr);
                
                if (sStatus == "SUCCESS" || sStatus == "FAIL")
                {
                    if (sStatus == "SUCCESS")
                    {
                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService("DA_PRD_SEL_ERP_HIST", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                        {
                            try
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                Util.GridSetData(dgErpHist, result, FrameOperation, true);
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
                    if (sStatus == "FAIL")
                    {
                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService("DA_PRD_SEL_ERP_HIST", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                        {
                            try
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                Util.GridSetData(dgErpHist, result, FrameOperation, true);
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
                }
                else
                {
                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService("DA_PRD_SEL_ERP_HIST", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                    {
                        try
                        {
                            if (Exception != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            Util.GridSetData(dgErpHist, result, FrameOperation, true);
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
                string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME", "EQPTNAME", "PRODNAME", "MODLNAME", "UNIT_CODE", "WOID", "LOTID" };

                _Util.SetDataGridMergeExtensionCol(dgErpHist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        //private void PACK()
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
        //        dtRqst.Columns.Add("TO_DATE", typeof(DateTime));
        //        dtRqst.Columns.Add("SHOPID", typeof(string));
        //        dtRqst.Columns.Add("AREAID", typeof(string));
        //        dtRqst.Columns.Add("PROCID", typeof(string));
        //        dtRqst.Columns.Add("EQSGID", typeof(string));
        //        dtRqst.Columns.Add("EQPTID", typeof(string));
        //        dtRqst.Columns.Add("SHIFT_ID", typeof(string));
        //        dtRqst.Columns.Add("STATUSID", typeof(string));

        //        DataRow dr = dtRqst.NewRow();

        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["FROM_DATE"] = Util.StringToDateTime(Util.GetCondition(dtpDateFrom), "yyyyMMdd");
        //        dr["TO_DATE"] = Util.StringToDateTime(Util.GetCondition(dtpDateTo), "yyyyMMdd");
        //        dr["SHOPID"] = Util.GetCondition(cboShop, bAllNull: true);
        //        dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
        //        dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
        //        dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
        //        dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
        //        dr["SHIFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
        //        dr["STATUSID"] = Util.GetCondition(cboStatus, bAllNull: true);
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_HIST", "INDATA", "OUTDATA", dtRqst);
        //        dgErpHist.ItemsSource = DataTableConverter.Convert(dtRslt);
        //        DataAll();
        //        DefaultHidden();
        //        DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            if (Util.NVC(row["BOXID"]).Equals("") || Util.NVC(row["LOTID"]).Equals(""))
        //            {
        //                if (Util.NVC(row["BOXID"]).Equals(""))
        //                {
        //                    DefaultLotId();

        //                }
        //                else
        //                {
        //                    DefaultBoxId();
        //                }
        //            }
        //            else
        //            {
        //                dgErpHist.Columns[16].Visibility = Visibility.Hidden;
        //            }
        //        }
        //        string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME", "EQPTNAME", "PRODNAME", "PRODNAME", "UNIT_CODE", "WOID", "LOTID" };
        //        _Util.SetDataGridMergeExtensionCol(dgErpHist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void ClearGrid()
        {
            dgErpHist.ItemsSource = null;
        }

        private void DataAll()
        {
            DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                for (int idx = 0; idx < dgErpHist.Columns.Count; idx++)
                {
                    dgErpHist.Columns[idx].Visibility = Visibility.Visible;
                }
            }
        }
        private void DefaultHidden()
        {
            dgErpHist.Columns[2].Visibility = Visibility.Hidden;
            dgErpHist.Columns[4].Visibility = Visibility.Hidden;
            dgErpHist.Columns[6].Visibility = Visibility.Hidden;
            dgErpHist.Columns[8].Visibility = Visibility.Hidden;
            dgErpHist.Columns[10].Visibility = Visibility.Hidden;
        }

        private void DefaultLotId()
        {
            dgErpHist.Columns[11].Visibility = Visibility.Hidden;
            dgErpHist.Columns[15].Visibility = Visibility.Hidden;
            dgErpHist.Columns[17].Visibility = Visibility.Hidden;
        }

        private void DefaultBoxId()
        {
            dgErpHist.Columns[9].Visibility = Visibility.Hidden;
            dgErpHist.Columns[11].Visibility = Visibility.Hidden;
            dgErpHist.Columns[17].Visibility = Visibility.Hidden;
        }

        private void OkNoneNT()
        {
            dgErpHist.Columns[17].Visibility = Visibility.Hidden;
            dgErpHist.Columns[19].Visibility = Visibility.Hidden;
            dgErpHist.Columns[20].Visibility = Visibility.Hidden;
            dgErpHist.Columns[21].Visibility = Visibility.Hidden;
        }
        private void OkHaveNT()
        {
            dgErpHist.Columns[19].Visibility = Visibility.Hidden;
            dgErpHist.Columns[20].Visibility = Visibility.Hidden;
            dgErpHist.Columns[21].Visibility = Visibility.Hidden;
        }

        private void FailHaveNT()
        {
            dgErpHist.Columns[18].Visibility = Visibility.Hidden;
        }

        private void FailNoneNT()
        {
            dgErpHist.Columns[17].Visibility = Visibility.Hidden;
            dgErpHist.Columns[18].Visibility = Visibility.Hidden;
        }
        private void NTVisible()
        {
            DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["PROCID"]).Equals("A5000"))
                {
                    dgErpHist.Columns[16].Visibility = Visibility.Visible;
                }
            }

        }
        private void FailAssy()
        {
            DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["PROCID"]).Equals("A5000"))
                {
                    FailHaveNT();
                }
                else
                {
                    FailNoneNT();
                }
            }
        }
        private void SuccessAssy()
        {
            DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["PROCID"]).Equals("A5000"))
                {
                    OkHaveNT();
                }
                else
                {
                    OkNoneNT();
                }
            }
        }

        private void DefaultAssy()
        {
            DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                if (!Util.NVC(row["PROCID"]).Equals("A5000"))
                {
                    dgErpHist.Columns[16].Visibility = Visibility.Hidden;
                }
            }
        }

        private void ReserveSend()
        {
            try
            {
                DataSet dtRqst = new DataSet();
                DataTable inData = dtRqst.Tables.Add("INDATA");
                inData.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                DataRow row = inData.NewRow();

                DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;

                for (int i = 0; i < dgErpHist.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = inData.NewRow();

                        for (int idx = 0; idx < dgErpHist.Rows.Count; idx++)
                        {
                            row["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "ERP_TRNF_SEQNO"));
                        }
                        inData.Rows.Add(row);
                    }
                }
                new ClientProxy().ExecuteServiceSync("BR_ACT_REG_RESEND_ERP_PROD", "INDATA", null, inData);
                GetList();
                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void ErrorMessage()
        {
            try
            {
                DataTable dts = ((DataView)dgErpHist.ItemsSource).Table;
                foreach (DataRow rows in dts.Rows)
                {
                    for (int i = 0; i < dgErpHist.Rows.Count(); i++)
                    {
                        if (rows["CHK"].ToString().Equals("True") || rows["CHK"].ToString().Equals("1"))
                        {
                            if (!Util.NVC(rows["CMCDNAME"]).Equals("NG")
                                && !Util.NVC(rows["CMCDNAME"]).Equals("FAIL"))
                            {
                                Util.Alert("SFU2948");  //전송실패(NG)된 항목만 재전송 가능합니다.
                                return;
                            }
                        }
                    }
                }
                ReserveSend();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgErpHist, "CHK");

                if (idx < 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                Save();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void Save()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        for (int i = 0; i < dgErpHist.Rows.Count() - dgErpHist.BottomRows.Count; i++)
                        {
                            if (DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK").Equals(true) ||
                                Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("True") ||
                                Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("USER_CHK_FLAG", typeof(String));
                                RQSTDT.Columns.Add("NOTE", typeof(String));
                                RQSTDT.Columns.Add("ERP_TRNF_SEQNO", typeof(String));

                                DataRow dr = RQSTDT.NewRow();
                                dr["USER_CHK_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "USER_CHK_FLAG"));
                                dr["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "NOTE"));
                                dr["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "ERP_TRNF_SEQNO"));
                                RQSTDT.Rows.Add(dr);
                                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_ERP_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                            }
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                        GetList();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkError_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    dtpDateFrom.IsEnabled = false;
                    dtpDateTo.IsEnabled = false;
                }
                else
                {
                    dtpDateFrom.IsEnabled = true;
                    dtpDateTo.IsEnabled = true;
                }
            }
        }

        private void chkActivity_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void cbVal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ComboBox combobox = sender as ComboBox;

        //    C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
        //    IList<FrameworkElement> ilist = combobox.GetAllParents();

        //    foreach (var item in ilist)
        //    {
        //        DataGridRowPresenter presenter = item as DataGridRowPresenter;
        //        if (presenter != null)
        //        {
        //            row = presenter.Row;
        //        }
        //    }

        //    dgErpHist.SelectedItem = row.DataItem;

        //    if (bCheck == false)
        //        DataTableConverter.SetValue(dgErpHist.SelectedItem, "CHK", true);


        //    //if (sender == null)
        //    //    return;

        //    //C1DataGrid dg = (sender as C1DataGrid);

        //    //if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
        //    //    return;

        //    //int idx = dg.CurrentCell.Row.Index;
        //    //if (idx < 0)
        //    //    return;

        //    //DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        //}

    }
}
