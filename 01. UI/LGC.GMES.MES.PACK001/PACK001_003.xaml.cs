/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Label자동발행(포장기) 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.06.27  손우석    SM   폴란드 요청으로 설비ID 추가
  2021.03.22 염규범 DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG 타입 아웃 이슈 해결로 인한 타입 변경 처리 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_003 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private DataView dvRootNodes;
        //private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer timer = null;
        private System.Timers.Timer _timer_DataSearch;
        private bool blZplPrinting = false;
        private bool bErrorMessage = true;
        private bool bPrintYnPopupOpen = true;
        private bool bPrintYn_Flag = false;
        public PACK001_003()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        #region Event - UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnLabel);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                setComPort();

                //TimerSetting();

                CommonCombo _combo = new CommonCombo();
                string[] area = { LoginInfo.CFG_AREA_ID };
                
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcessBox };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, sFilter: area);

                C1ComboBox[] cboProcessBoxParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcessBox, CommonCombo.ComboStatus.NONE, cbParent: cboProcessBoxParent, sCase: "BOX_PROCESS");
                
                setYnComboBox();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - TextBox
        
        #endregion

        #region Event - Button & CheckBox
        private void btnKeyPartCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copyClipboardTreeView();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnBoxListSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setBoxResultList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLabelBox.Text.Length > 0)
                {
                    Util.printLabel_Pack(FrameOperation, loadingIndicator, txtLabelBox.Text, LABEL_TYPE_CODE.PACK_INBOX, "N", "1", null);

                    if (Util.NVC(txtLabelBoxSeq.Tag) == "N")//print 여부 N인경우 Y로 update
                    {
                        updateTB_SFC_LABEL_PRT_REQ_HIST(txtLabelBox.Text, Util.NVC_Int(txtLabelBoxSeq.Text));
                    }
                }
                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                setBoxResultList();

                TimerLabelPrint();
            }
            catch (Exception ex)
            {
                showErrorPopup(ex);
                //Util.Alert(ex.ToString());
            }
        }

        private void chkPageFixed_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                bPrintYn_Flag = false;
                TimerStatus(false);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkPageFixed_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC(cboPrintYn.SelectedValue) == "Y")
                {
                    if (bPrintYnPopupOpen)
                    {
                        openPopUp_PrintYN();
                        //popupPrintYn_Closed 에서 TimerStatus(true); 실행 차후 발행pc IP 기준정보화 되면 수정해야함!!! 임시로 Y인경우만 확인 팝업으로 알림.
                        TimerStatus(true);
                    }
                }
                else
                {
                    TimerStatus(true);

                    EnableChangeSelectLine(false);
                }

                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        
        private void chkToday_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.SelectedDateTime = DateTime.Today;
                dtpDateTo.SelectedDateTime = DateTime.Today;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkLineSelectConfirm_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                cboEquipmentSegment.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkLineSelectConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                cboEquipmentSegment.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - ComboBox
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //그리드 라벨 코드 setting
                SetGridCbo_LabelCode(dgLabelPortSetting.Columns["LABEL_CODE"]);

                SetGridCbo_LabelDPI(dgLabelPortSetting.Columns["DPI"]);

                setBoxResultQty();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboProcessBox_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //그리드 라벨 코드 setting
                SetGridCbo_LabelCode(dgLabelPortSetting.Columns["LABEL_CODE"]);

                SetGridCbo_LabelDPI(dgLabelPortSetting.Columns["DPI"]);

                setBoxResultQty();

                //2018.06.27
                Set_Combo_Equipment(cboEquipment);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void cboAutoSearch_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                //if (cboAutoSearch.SelectedIndex == 0)
                //{
                //    rdoPageFixed.IsChecked = true;
                //}

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - DataGrid
        private void dgBoxList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "BOXID")
                    {
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    if(e.Cell.Row.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Pink);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgBoxList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)rdoPageFixed.IsChecked)
                {
                    return;
                }
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if(cell.Column.Name == "BOXID")
                        {
                            PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                            popup.FrameOperation = this.FrameOperation;

                            if (popup != null)
                            {
                                DataTable dtData = new DataTable();
                                dtData.Columns.Add("BOXID", typeof(string));

                                DataRow newRow = null;
                                newRow = dtData.NewRow();
                                newRow["BOXID"] = cell.Text;

                                dtData.Rows.Add(newRow);

                                //========================================================================
                                object[] Parameters = new object[1];
                                Parameters[0] = dtData;
                                C1WindowExtension.SetParameters(popup, Parameters);
                                //========================================================================

                                //popup.Closed -= popup_Closed;
                                //popup.Closed += popup_Closed;
                                popup.ShowModal();
                                popup.CenterOnScreen();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void dgBoxList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)rdoPageFixed.IsChecked)
                {
                    return;
                }


                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        string sBoxid = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[cell.Row.Index].DataItem, "BOXID"));

                        if (Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[cell.Row.Index].DataItem, "TOTAL_QTY")))== 0)
                        {
                            txtLabelBox.Text = sBoxid;
                            return;
                        }

                        
                        string sPRT_REQ_SEQNO = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[cell.Row.Index].DataItem, "PRT_REQ_SEQNO"));
                        string sPRT_FLAG = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[cell.Row.Index].DataItem, "PRT_FLAG"));
                        //string sBoxid = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[cell.Row.Index].DataItem, "BOXID"));
                        txtLabelBox.Text = sBoxid;
                        txtLabelBoxSeq.Text = sPRT_REQ_SEQNO;
                        txtLabelBoxSeq.Tag = sPRT_FLAG;

                        getLotInputBoxTracking(sBoxid);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - Grid

        private void btnRight_Checked(object sender, RoutedEventArgs e)
        {

            if (Content != null)
            {
                Content.ColumnDefinitions[4].Width = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(4, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                Content.ColumnDefinitions[5].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        private void btnRight_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Content != null)
            {
                //Content.ColumnDefinitions[5].Width = new GridLength(300);
                //Content.ColumnDefinitions[4].Width = new GridLength(8);

                Content.ColumnDefinitions[4].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(4, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                Content.ColumnDefinitions[5].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        #endregion

        void popupPrintYn_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_002_PRINT_YN_SELECT popup = sender as PACK001_002_PRINT_YN_SELECT;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    bPrintYn_Flag = popup.PRINTYN_FLAG;
                    string sYn = bPrintYn_Flag ? "Y" : "N";
                    cboPrintYn.SelectedValue = sYn;
                    bPrintYnPopupOpen = false;
                    TimerStatus(true);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod

        private void setYnComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                String[] sFilter3 = { "IUSE" };
                _combo.SetCombo(cboPrintYn, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TimerSetting()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += new EventHandler(timer_Tick);

            //int interval = Convert.ToInt32(cboAutoSearchTime.Value.ToString());

            //_timer_DataSearch = new System.Timers.Timer(interval *1000); //기준 : 초
            //_timer_DataSearch.AutoReset = true;
            //_timer_DataSearch.Elapsed += (s, arg) =>
            //{
            //    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            //    {
            //        setBoxResultList();

            //        TimerLabelPrint();
            //    }));
               
            //};
            //_timer_DataSearch.Start();
        }

        private void EnableChangeSelectLine(bool bEnable)
        {
            try
            {
                cboEquipmentSegment.IsEnabled = bEnable;
                cboProcessBox.IsEnabled = bEnable;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void TimerStatus(bool bOn)
        {
            //if (bOn)
            //{
            //    timer.IsEnabled = true;
            //    bPrintYnPopupOpen = false;
            //    int iInterval = (int)(cboAutoSearchTime.Value) > 2 ? (int)(cboAutoSearchTime.Value) : 3;
            //    // Timer Start
            //    timer.Interval = new TimeSpan(0, 0, 0, iInterval);

            //    rdoPageFixed.IsChecked = true;
            //    FrameOperation.PageFixed(true);
            //    btnBoxListSearch.IsEnabled = false;
                
            //}
            //else
            //{
            //    bPrintYnPopupOpen = true;
            //    rdoPageFixed.IsChecked = false;
            //    FrameOperation.PageFixed(false);
            //    btnBoxListSearch.IsEnabled = true;
            //    cboEquipmentSegment.IsEnabled = true;
            //    timer.IsEnabled = false;
            //}


            if (bOn)
            {
                int iInterval = (int)(cboAutoSearchTime.Value) > 2 ? (int)(cboAutoSearchTime.Value) : 3;
                bPrintYnPopupOpen = false;
                //rdoPageFixed.IsChecked = true;
                //rdoPageUnFixed.IsChecked = false;
                FrameOperation.PageFixed(true);
                btnBoxListSearch.IsEnabled = false;
                btnLabel.IsEnabled = false;
                btnKeyPartCopy.IsEnabled = false;
                cboAutoSearchTime.IsEnabled = false;
                cboSearchCount.IsEnabled = false;
                dgLabelPortSetting.IsEnabled = false;
                cboEquipmentSegment.IsEnabled = false;
                cboProcessBox.IsEnabled = false;
                cboPrintYn.IsEnabled = false;

                //TimerSetting();
                if (timer == null)
                {
                    timer = new System.Windows.Threading.DispatcherTimer();
                    timer.IsEnabled = true;
                    timer.Interval = new TimeSpan(0, 0, 0, iInterval);
                    timer.Tick += new EventHandler(timer_Tick);
                }
                   
                //cboAutoSearch.SelectedIndex = 0;
                //cboAutoSearch.SelectedValue = "Y";
                //cboAutoSearch.IsEnabled = false;
            }
            else
            {
                bPrintYnPopupOpen = true;
                //rdoPageFixed.IsChecked = false;
                //rdoPageUnFixed.IsChecked = true;
                FrameOperation.PageFixed(false);
                btnBoxListSearch.IsEnabled = true;
                btnLabel.IsEnabled = true;
                btnKeyPartCopy.IsEnabled = true;
                cboAutoSearchTime.IsEnabled = true;
                cboSearchCount.IsEnabled = true;
                dgLabelPortSetting.IsEnabled = true;
                cboEquipmentSegment.IsEnabled = true;
                cboProcessBox.IsEnabled = true;
                cboPrintYn.IsEnabled = true;

                //if (_timer_DataSearch != null)
                //{
                //    _timer_DataSearch.Stop();
                //    _timer_DataSearch.Dispose();
                //    _timer_DataSearch = null;
                //}

                if(timer != null)
                {
                    timer.Stop();
                    timer.IsEnabled = false;
                    timer = null;
                }
                                  
                //timer.Tick -= new EventHandler(timer_Tick);
                //cboAutoSearch.SelectedValue = "N";
                //cboAutoSearch.IsEnabled = true;
            }
        }

        /// <summary>
        /// 선택 라인 의 포장실적수량 화면에 표시
        /// </summary>
        private void setBoxResultQty()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = null;
                //2018.06.27
                //dr["EQPTID"] = null;
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_RESULT_CNT_BY_EQSG", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    txtCaldate.Text = Util.NVC(dtResult.Rows[0]["CALDATE_DATE"]);
                    txtShift.Text = Util.NVC(dtResult.Rows[0]["SHFT_NAME"]);
                    txtGoodQty.Text = Util.NVC(dtResult.Rows[0]["BOXCNT"]);
                }
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_BOX_RESULT_CNT_BY_EQSG", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 포장완료목록 조회
        /// </summary>
        private void setBoxResultList()
        {
            try
            {
                //DA_PRD_SEL_BOX_LIST_FOR_LABEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("TOPCNT", typeof(Int32));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcessBox.SelectedValue);
                dr["EQPTID"] = null;
                dr["TOPCNT"] = (Int32)cboSearchCount.Value;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL", "RQSTDT", "RSLTDT", RQSTDT);

                dgBoxList.ItemsSource = null;
                dgBoxList.ItemsSource = DataTableConverter.Convert(dtboxList);
                Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dtboxList.Rows.Count));
                setBoxResultQty();

             
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_BOX_LIST_FOR_LABEL", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getLotInputBoxTracking(string sBoxID)
        {
            try
            {
                //DA_PRD_SEL_BOXLOT_TRACKING

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sBoxID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOT_TRACKING", "RQSTDT", "RSLTDT", RQSTDT);

                if(!Convert.ToBoolean(rdoPageFixed.IsChecked))
                {
                    setLotInputBoxTracking(dtboxList);
                }
                
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_BOXLOT_TRACKING", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void setLotInputBoxTracking(DataTable dtBoxLot)
        {
            try
            {
                int row_count = dtBoxLot.Rows.Count;
                DataSet ds = new DataSet();
                dtBoxLot.TableName = "BOXLOT";
                DataRow dr = dtBoxLot.NewRow();
                dr.ItemArray = new object[] { null, null, row_count == 0 ? null :Util.NVC(dtBoxLot.Rows[0]["BOXID"]), null, null, null, null, null };
                dtBoxLot.Rows.InsertAt(dr, 0);

                ds.Tables.Add(dtBoxLot.Copy());
                ds.Relations.Add("Relations", ds.Tables["BOXLOT"].Columns["LOTID"], ds.Tables["BOXLOT"].Columns["BOXID"]);

                dvRootNodes = ds.Tables["BOXLOT"].DefaultView;
                dvRootNodes.RowFilter = "BOXID IS NULL";
                trvKeypartList.ItemsSource = dvRootNodes;

                if(row_count > 0)
                {
                    TreeItemExpandAll();
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        C1TreeViewItem Box_items;
        C1TreeViewItem Lot_items;

        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);

            if(items.Count ==1)
            {
                
            }
            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }

        private void copyClipboardTreeView()
        {
            try
            {

                string strAllNodeText = string.Empty;
                //IList<DependencyObject> items = new List<DependencyObject>();
                //VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);


                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(TextBlock), ref itemText);

                for (int i = 0; i < itemText.Count; i++)
                {
                    TextBlock textBolock = (TextBlock)itemText[i];
                    strAllNodeText += string.Format("{0} ", i) + textBolock.Text + Environment.NewLine;
                }

                Clipboard.SetText(strAllNodeText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TimerLabelPrint()
        {
            try
            {
                string sScanid = string.Empty;
                string sPRT_SEQ = string.Empty;
                string slotid = string.Empty;
                string sPRT_FLAG = string.Empty;
                if (dgBoxList.Rows.Count > 0)
                {
                    sScanid = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[0].DataItem, "BOXID"));
                    sPRT_SEQ = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[0].DataItem, "PRT_REQ_SEQNO"));
                    slotid = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[0].DataItem, "LOTID"));

                    sPRT_FLAG = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[0].DataItem, "PRT_FLAG"));

                    if (slotid != "" && sPRT_FLAG == "N" && !blZplPrinting && bPrintYn_Flag)
                    {
                        labelPrint(slotid, sScanid, sPRT_SEQ);
                    }
                    getLotInputBoxTracking(sScanid);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void labelPrint(string slotid, string sScanid, string sPRT_SEQ)
        {
            try
            {
                blZplPrinting = true;

                DataTable dtZpl = new DataTable();
                dtZpl.Columns.Add("PORTNAME", typeof(string));
                dtZpl.Columns.Add("ZPL", typeof(string));


                int iDataGridCount_LabelPortSetting = dgLabelPortSetting.Rows.Count;
                for (int i = 0; i < iDataGridCount_LabelPortSetting; i++)
                {
                    bool bCHK = false;
                    if (Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "CHK")) != "")
                    {
                        bCHK = Convert.ToBoolean(Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "CHK")));
                    }


                    if (bCHK)
                    {
                        string sPORTNAME = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "PORTNAME"));
                        string sLABEL_CODE = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "LABEL_CODE"));
                        string sPRN_QTY = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "LABEL_CNT"));
                        string sDPI = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "DPI"));
                        string sX = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "X"));
                        string sY = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "Y"));
                        string sDARKNESS = Util.NVC(DataTableConverter.GetValue(dgLabelPortSetting.Rows[i].DataItem, "DARKNESS"));
                        
                        DataTable dtResult = Util.getZPL_Pack(sLOTID: slotid
                                                            , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                                            , sLABEL_CODE: sLABEL_CODE
                                                            , sSAMPLE_FLAG: "N"
                                                            , sPRN_QTY: sPRN_QTY
                                                            , sDPI: sDPI
                                                            , sLEFT: sX
                                                            , sTOP: sY
                                                            , sDARKNESS: sDARKNESS
                                                            );

                        DataRow dr = dtZpl.NewRow();
                        dr["PORTNAME"] = sPORTNAME;
                        dr["ZPL"] = Util.NVC(dtResult.Rows[0]["ZPLSTRING"]);
                        dtZpl.Rows.Add(dr);

                    }
                    //Util.printLabel_Pack(FrameOperation, loadingIndicator, sScanid, "PROC", "N", "1");
                }
                bool bPrintSucessFlag = Util.PrintLabel(FrameOperation, loadingIndicator, dtZpl);

                if (bPrintSucessFlag)
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(sScanid, Util.NVC_Int(sPRT_SEQ));

                    showLabelPrintInfoPopup(slotid, sScanid);
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("BARCODEPRINT실패\n프린트연결상태를확인하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    throw new System.InvalidOperationException(ms.AlertRetun("SFU1310")); //BARCODEPRINT실패\r\n프린트연결상태를확인하세요.
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                blZplPrinting = false;
            }
        }

        private void showLabelPrintInfoPopup(string sLotid, string sScanid)
        {
            try
            {
                PACK001_002_PRINTINFOMATION popup = new PACK001_002_PRINTINFOMATION();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("SCANID", typeof(string));
                    dtData.Columns.Add("LOTID_TITLE", typeof(string));
                    dtData.Columns.Add("SCANID_TITLE", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;
                    newRow["SCANID"] = sScanid;
                    newRow["LOTID_TITLE"] = "BOXID";
                    newRow["SCANID_TITLE"] = string.Empty;
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, Int32 sPRT_SEQ)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = sPRT_SEQ;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 타이머로 조회시 오류발생일 타이머종료
        /// 에러확인시 타이머 재실행.
        /// </summary>
        /// <param name="ex"></param>
        private void showErrorPopup(Exception ex)
        {
            string exMessageDic = MessageDic.Instance.GetMessage(ex);
            if (bErrorMessage)
            {
                TimerStatus(false);
                bErrorMessage = false;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(exMessageDic), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (sResult) =>
                {
                    bErrorMessage = true;
                    TimerStatus(true);
                });
            }
        }

        private void setComPort()
        {
            try
            {
                DataTable dtConfigPrint = LoginInfo.CFG_SERIAL_PRINT;

                DataTable dtLabelPortSetting = new DataTable();
                dtLabelPortSetting.Columns.Add("CHK", typeof(bool));
                dtLabelPortSetting.Columns.Add("PORTNAME", typeof(string));
                dtLabelPortSetting.Columns.Add("LABEL_CODE", typeof(string));
                dtLabelPortSetting.Columns.Add("LABEL_CNT", typeof(Int32));
                dtLabelPortSetting.Columns.Add("DPI", typeof(string));
                dtLabelPortSetting.Columns.Add("X", typeof(string));
                dtLabelPortSetting.Columns.Add("Y", typeof(string));
                dtLabelPortSetting.Columns.Add("DARKNESS", typeof(string));


                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    DataRow drLabelPortSetting = dtLabelPortSetting.NewRow();
                    drLabelPortSetting["CHK"] = Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString());
                    drLabelPortSetting["PORTNAME"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME];
                    drLabelPortSetting["LABEL_CODE"] = "";
                    drLabelPortSetting["LABEL_CNT"] = 1;
                    drLabelPortSetting["DPI"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI];
                    drLabelPortSetting["X"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X];
                    drLabelPortSetting["Y"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y];
                    drLabelPortSetting["DARKNESS"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS];

                    dtLabelPortSetting.Rows.Add(drLabelPortSetting);
                }

                dgLabelPortSetting.ItemsSource = DataTableConverter.Convert(dtLabelPortSetting);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetGridCbo_LabelCode(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                if (Util.NVC(cboProcessBox.SelectedValue).Length > 0)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    dr["PROCID"] = Util.NVC(cboProcessBox.SelectedValue) == "" ? null : Util.NVC(cboProcessBox.SelectedValue);
                    dr["LABEL_TYPE_CODE"] = LABEL_TYPE_CODE.PACK_INBOX;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_EQSGPROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                    //바인딩후 라벨코드의 첫번째 값 기본세팅
                    if (dtResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgLabelPortSetting.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgLabelPortSetting.Rows[i].DataItem, "LABEL_CODE", Util.NVC(dtResult.Rows[0]["CBO_CODE"]));
                        }
                    }
                }
                else
                {
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void SetGridCbo_LabelDPI(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                if (Util.NVC(cboProcessBox.SelectedValue).Length > 0)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = "PRINTER_RESOLUTION";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                    //바인딩후 라벨코드의 첫번째 값 기본세팅
                    if (dtResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgLabelPortSetting.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgLabelPortSetting.Rows[i].DataItem, "DPI", Util.NVC(dtResult.Rows[0]["CBO_CODE"]));
                        }
                    }
                }
                else
                {
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_BAS_SEL_COMMCODE_CBO", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void openPopUp_PrintYN()
        {
            try
            {
                PACK001_002_PRINT_YN_SELECT popup = new PACK001_002_PRINT_YN_SELECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    popup.Closed -= popupPrintYn_Closed;
                    popup.Closed += popupPrintYn_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.06.27
        private void Set_Combo_Equipment(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                string sLINEID = cboEquipmentSegment.SelectedValue.ToString();
                string sPROCID = cboProcessBox.SelectedValue.ToString();
                String[] sFilter = { sLINEID, sPROCID };

                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "EQUIPMENT_BY_EQSGID_PROCID");

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        //2018.06.27
        #endregion
    }
}
