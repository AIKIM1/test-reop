/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_110 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        public BOX001_110()
        {
            InitializeComponent();
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

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo(); 

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT,cbChild:new C1ComboBox[] { cboLine }); 
            
            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea};
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbParent: cboEquipmentSegmentParent);

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }        
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            //dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
          //  dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                   // if (Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgOutHist.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", true);
                }
            }
            
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                }
            }
            
        }


        private void btnShift_Click(object sender, RoutedEventArgs e)
        {            
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.SELECTING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = LoginInfo.CFG_EQPT_ID;
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }

        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);              
            }
            this.grdMain.Children.Remove(wndPopup);
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_DEFECT_PALLET_LIST_ST
        /// </summary>
        private void Search()
        {
            try
            {
                if(string.IsNullOrWhiteSpace(Util.NVC(cboLine.SelectedValue)) || Util.NVC(cboLine.SelectedValue) == "SELECT" )
                {
                    //SFU1223 라인을 선택 하세요.
                    Util.MessageValidation("SFU1223");
                    return;                       
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                //RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                //RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("LOTID_RT", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("SCRAP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.SELECTING;
                dr["SCRAP"] = "N";

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    dr["LOTID"] = txtPalletID.Text;
                }
                else
                {
                    dr["EQSGID"] = cboLine.SelectedValue;
                    dr["LOTID_RT"] = txtLotID.Text;
                    //dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    //dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_DEFECT_PALLET_LIST_ST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgSearchResult, dtResult, FrameOperation);

                if (dgSearchResult.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["DEFECT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

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

        #endregion

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }


        private void dgSearchResult_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void btnScrap_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

            List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

            if (drList.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            loadingIndicator.Visibility = Visibility.Visible;
            object[] sParam = { drList.CopyToDataTable() };

            this.FrameOperation.OpenMenu("SFU010120360", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnRework_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_CANCEL_REG_DEFECT_LOT_ST
            try
            {
                if (string.IsNullOrEmpty(txtShift.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                List<int> rowList = _util.GetDataGridCheckRowIndex(dgSearchResult, "CHK");

                if (rowList.Count <= 0)
                {
                    //SFU1645	선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE");
                inDataTable.Columns.Add("IFMODE");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHIFT");
                inDataTable.Columns.Add("WRK_USERID");
                inDataTable.Columns.Add("WRK_USER_NAME");
                inDataTable.Columns.Add("POSTDATE");

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = txtShift.Tag;
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text;
                newRow["POSTDATE"] = ldpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("LOTID");

                foreach (int row in rowList)
                {
                    string sBoxID = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[row].DataItem, "PALLETID"));
                    
                    newRow = inBoxTable.NewRow();
                    newRow["LOTID"] = sBoxID;
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_REG_DEFECT_LOT_ST", "INDATA,INRESN", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
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

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker.Text) || string.IsNullOrEmpty(Util.NVC(txtWorker.Tag)))
            {
                // SFU1843 작업자를 입력 해주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (string.IsNullOrEmpty(txtShift.Text) || string.IsNullOrEmpty(Util.NVC(txtShift.Tag)))
            {
                // SFU1844 작업조를 입력 해주세요.
                Util.MessageValidation("SFU1844");
                return;
            }

            BOX001_110_DEFECT wndPopup = new BOX001_110_DEFECT();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = txtShift.Tag;
                Parameters[1] = txtWorker.Tag;
                Parameters[2] = txtWorker.Text;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndDefect_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void wndDefect_Closed(object sender, EventArgs e)
        {
            BOX001_110_DEFECT wndPopup = new BOX001_110_DEFECT();
            Search();
            this.grdMain.Children.Remove(wndPopup);
        }

        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drSelect = DataTableConverter.Convert(dgSearchResult.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

           // _tagPrintCount = drSelect.Count;

            foreach (DataRow drPrint in drSelect)
            {
                TagPrint(drPrint);
            }
        }

        private void TagPrint(DataRow dr)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.DefectPalletYN = "Y";

            object[] parameters = new object[8];
            parameters[0] = dr["PROCID"];
            parameters[1] = null;              // 설비ID
            parameters[2] = dr["PALLETID"];
            parameters[3] = dr["WIPSEQ"].ToString();
            parameters[4] = dr["WIPQTY"].ToString();
            parameters[5] = "N";                                         // 디스패치 처리
            parameters[6] = "Y";                                         // 출력 여부
            parameters[7] = (bool)chkTagPrint.IsChecked ? "N" : "Y";     // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnCost_Click(object sender, RoutedEventArgs e)
        {
            //SFU010120430 비용처리 대상 등록/취소
            DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

            List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

            if (drList.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            loadingIndicator.Visibility = Visibility.Visible;
            object[] sParam = { drList.CopyToDataTable() };

            this.FrameOperation.OpenMenu("SFU010120430", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
    }
}
