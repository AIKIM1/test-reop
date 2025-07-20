/*************************************************************************************
 Created Date : 2020.06.10
      Creator : 염규범
   Decription : Pack LABEL 자동발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.10  염규범 : Initial Created.
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
using System.Management;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_062 : UserControl, IWorkArea 
    {
        #region Declaration & Constructor 
        string now_labelcode = string.Empty;
        string strEqsgId = string.Empty;
        string strProcId = string.Empty;
        string strEqptId = string.Empty;

        string strCurrentLotID = string.Empty;
        string strCurrentReqNo = string.Empty;

        string strAu = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private PACK001_062_PROCESSINFO window_PROCESSINFO = new PACK001_062_PROCESSINFO();
        private DataView dvRootNodes;
        private System.Windows.Threading.DispatcherTimer timer = null;
        //private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        
        private bool blZplPrinting = false;
        /// <summary>
        /// 자동발행중 에러표시를위한 변수 True:에러팝업표시 False:표시안함
        /// </summary>
        private bool bErrorMessage = true;
        /// <summary>
        /// 자동발행중 print여부팝업을 표시하지않기위한 변수
        /// </summary>
        private bool bPrintYnPopupOpen = true;
        private bool bPrintYn_Flag = false;

        //2019.05.23
        private string sEqptLabel = string.Empty;

        public PACK001_062()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnLabel);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                if (!chkAUTH())
                {
                    ContentRight.RowDefinitions[7].Height = new GridLength(0);
                }

                setProcessInfo();
                setYnComboBox();

                //처음로드시 팝업오픈
                if (!(window_PROCESSINFO.PROCID.Length > 0))
                {
                    btnProcessSelect_Click(null, null);
                }

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
                tbWipListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event

        #region Button
        private void btnProcessSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_062_SELECTPROCESS popup = new PACK001_062_SELECTPROCESS();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LABELPRINTUSE", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LABELPRINTUSE"] = "Y";
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnProdutLotSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setWipList();
                setWipPrint();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnKeyPartCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copyClipboardKeypart();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLabelLot.Text.Length > 0)
                {

                    DataTable dtChkComPort = chkComPort(txtLabelProductId.Text.ToString());

                    if (!(dtChkComPort.Rows.Count > 0))
                    {
                        Util.MessageInfo("SFU8219");
                        rdoPageUnFixed.IsChecked = true;
                        rdoPageFixed.IsChecked = false;
                        bPrintYnPopupOpen = false;
                        TimerStatus(false);
                        return;
                    }
                    string sSelectLotid = txtLabelLot.Text;
                    Util.printLabel_Pack_New(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, "N", "1", null, window_PROCESSINFO.EQPTID, window_PROCESSINFO.PROCID,
                        dtChkComPort.Rows[0]["PRTR_DPI"].ToString(), dtChkComPort.Rows[0]["PRT_X"].ToString(), dtChkComPort.Rows[0]["PRT_Y"].ToString(), dtChkComPort.Rows[0]["PRT_DARKNESS"].ToString());

                    if (Util.NVC(txtLabelLot.Tag) == "N")//print 여부 N인경우 Y로 update
                    {
                        updateTB_SFC_LABEL_PRT_REQ_HIST(txtLabelScanID.Text, Util.NVC(txtLabelScanID.Tag));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Button

        #region Grid
        private void dgWipList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    if (e.Cell.Row.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Pink);
                    }
                }));
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgWipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)rdoPageFixed.IsChecked)
                {
                    return;
                }
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgWipList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)rdoPageFixed.IsChecked)
                {
                    return;
                }
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "LOTID"));
                    string sSelectScanid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "SCAN_ID"));
                    string sSelectPRT_REQ = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRT_REQ_SEQNO"));
                    string sSelectProductName = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODNAME"));
                    string sSelectProductId = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sSelectPRT_FLAG = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRT_FLAG"));

                    //Lto 조회treeView 표시
                    txtLabelLot.Text = sSelectLotid;
                    txtLabelLot.Tag = sSelectPRT_FLAG;
                    txtLabelScanID.Text = sSelectScanid;
                    txtLabelScanID.Tag = sSelectPRT_REQ;
                    txtLabelProduct.Text = sSelectProductName;
                    txtLabelProductId.Text = sSelectProductId;

                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        //2019.03.12
        private void dgWipPrint_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)rdoPageFixed.IsChecked)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipPrint.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "LOTID"));
                    string sSelectScanid = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "SCAN_ID"));
                    string sSelectPRT_REQ = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "PRT_REQ_SEQNO"));
                    string sSelectProductName = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "PRODNAME"));
                    string sSelectProductId = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sSelectPRT_FLAG = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "PRT_FLAG"));

                    //Lto 조회treeView 표시
                    txtLabelLot.Text = sSelectLotid;
                    txtLabelLot.Tag = sSelectPRT_FLAG;
                    txtLabelScanID.Text = sSelectScanid;
                    txtLabelScanID.Tag = sSelectPRT_REQ;
                    txtLabelProduct.Text = sSelectProductName;
                    
                    //setLotInputMtrlTracking(sSelectLotid);

                    txtCurrentReqNo.Text = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "PRT_REQ_SEQNO"));
                    txtCurrentLot.Text = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[cell.Row.Index].DataItem, "LOTID"));
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        //2019.03.12
        private void dgWipPrint_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)rdoPageFixed.IsChecked)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipPrint.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        //2019.03.12
        private void dgWipPrint_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    if (e.Cell.Row.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Pink);
                    }
                }));
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion Grid

        #region Check
        private void chkPageFixed_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtChkComPort = chkComPort();

                if (Util.NVC(cboPrintYn.SelectedValue) == "Y")
                {
                    if (dtChkComPort == null)
                    {
                        Util.MessageInfo("SFU8219");
                        rdoPageUnFixed.IsChecked = true;
                        rdoPageFixed.IsChecked = false;
                        bPrintYnPopupOpen = false;
                        cboPrintYn.IsEnabled = true;
                        TimerStatus(false);
                        return;
                    }

                }

                if (!(window_PROCESSINFO.PROCID.Length > 0))
                {
                    TimerStatus(false);
                    Util.MessageInfo("MMD0005");
                    cboPrintYn.IsEnabled = true;
                    rdoPageUnFixed.IsChecked = true;
                }
                else
                {
                    openPopUp_PrintYN();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                Util.MessageException(ex);
            }
        }
        #endregion Check 

        void popup_Closed(object sender, EventArgs e)
        {
            try
            { 
                PACK001_062_SELECTPROCESS popup = sender as PACK001_062_SELECTPROCESS;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    DataRow drSelectedProcess = popup.DrSelectedProcessInfo;

                    if (drSelectedProcess != null)
                    {
                        window_PROCESSINFO.setProcess(drSelectedProcess);

                        tbTitle.Text = popup.SSelectedProcessTitle;

                        strEqsgId = window_PROCESSINFO.EQSGID;
                        strProcId = window_PROCESSINFO.PROCID;
                        strEqptId = window_PROCESSINFO.EQPTID;
                        Refresh();
                        cboLabelCode.IsEnabled = false;
                    }

                }
                else if(popup.DialogResult == MessageBoxResult.Cancel && dgWipList.Rows.Count.Equals(0))
                {
                    cboLabelCode.SelectedIndex = -1;
                    cboLabelCode.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
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

                    if (sYn.Equals("N"))
                    {
                        AutoPrint.Text = "자동조회";
                    }

                    bPrintYnPopupOpen = false;
                    TimerStatus(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
               
                setWipList();
                setWipPrint();

                if (Util.NVC(cboPrintYn.SelectedValue) == "Y")
                {
                    DataTable dt = Util.MakeDataTable(dgWipPrint, true);

                    if (!(dt.Rows.Count > 0)) return;

                    object[] arrProdID = dt.Select().Where(y => y["PRT_FLAG"].ToString() == "N").Select(x => x["PRODID"]).ToArray();

                    if (arrProdID == null) return;

                    string[] arrPalletStr = arrProdID.Cast<string>().ToArray();

                    DataTable dtChkComPort = chkComPort(arrPalletStr[0].ToString());

                    if (dtChkComPort == null)
                    {
                        Util.MessageInfo("SFU8219");
                        rdoPageUnFixed.IsChecked = true;
                        rdoPageFixed.IsChecked = false;
                        bPrintYnPopupOpen = false;
                        TimerStatus(false);
                        return;
                    }
                    TimerLabelPrintNew(dtChkComPort);
                }
            }
            catch (Exception ex)
            {
                //showErrorPopup(ex);
                Util.MessageException(ex);
                rdoPageUnFixed.IsChecked = true;
                rdoPageFixed.IsChecked = false;
                bPrintYnPopupOpen = false;
                TimerStatus(false);
            }
        }

        #endregion Event

        #region Mehod
        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);

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

        #region 선택된 키파트 전체 카피 메뉴
        //private void copyClipboardKeypart()
        //{
        //    try
        //    {

        //        string strAllNodeText = string.Empty;
        //        IList<DependencyObject> items = new List<DependencyObject>();
        //        VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);

        //        foreach (C1TreeViewItem item in items)
        //        {
        //            TreeViewRecusive(item, ref strAllNodeText);
        //        }

        //        Clipboard.SetText(strAllNodeText);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //private void TreeViewRecusive(C1TreeViewItem trViewItem, ref string strAllNodeText)
        //{
        //    strAllNodeText += string.Format("{0} ", trViewItem.Index + 1) + trViewItem.Header.ToString() + trViewItem.DisplayMemberPath + Environment.NewLine;


        //    object oTeset = trViewItem.DataContext;

        //    IList<DependencyObject> items = new List<DependencyObject>();
        //    VTreeHelper.GetChildrenOfType(trViewItem, typeof(C1TreeViewItem), ref items);

        //    IList<DependencyObject> itemText = new List<DependencyObject>();
        //    VTreeHelper.GetChildrenOfType(trViewItem, typeof(TextBlock), ref itemText);

        //    TextBlock textBolock = (TextBlock)itemText[0];
        //    string ssText = textBolock.Text;


        //    foreach (C1TreeViewItem childItem in items)
        //    {
        //            TreeViewRecusive(childItem, ref strAllNodeText);
        //    }
        //}

        private void copyClipboardKeypart()
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
        #endregion

        private void setProcessInfo()
        {
            if (dgWorkInfo.Children.Count == 0)
            {
                window_PROCESSINFO.PACK001_062 = this;
                dgWorkInfo.Children.Add(window_PROCESSINFO);
            }
        }

        public void TimerStatus(bool bOn)
        {
            if (bOn)
            {
                int iInterval = (int)(cboAutoSearchTime.Value) > 2 ? (int)(cboAutoSearchTime.Value) : 3;
                bPrintYnPopupOpen = false;
                FrameOperation.PageFixed(true);
                btnProdutLotSearch.IsEnabled = false;
                btnLabel.IsEnabled = false;
                btnProcessSelect.IsEnabled = false;
                btnKeyPartCopy.IsEnabled = false;
                cboAutoSearchTime.IsEnabled = false;
                cboSearchCount.IsEnabled = false;
                dgLabelPortSetting.IsEnabled = false;
                cboPrintYn.IsEnabled = false;

                if (timer == null)
                {
                    timer = new System.Windows.Threading.DispatcherTimer();
                    timer.IsEnabled = true;
                    timer.Interval = new TimeSpan(0, 0, 0, iInterval);
                    timer.Tick += new EventHandler(timer_Tick);
                }

            }
            else
            {
                bPrintYnPopupOpen = true;
                FrameOperation.PageFixed(false);

                btnProdutLotSearch.IsEnabled = true;
                btnLabel.IsEnabled = true;
                btnProcessSelect.IsEnabled = true;
                btnKeyPartCopy.IsEnabled = true;
                cboAutoSearchTime.IsEnabled = true;
                cboSearchCount.IsEnabled = true;
                dgLabelPortSetting.IsEnabled = true;
                cboPrintYn.IsEnabled = true;

                if (timer != null)
                {
                    timer.Stop();
                    timer.IsEnabled = false;
                    timer = null;
                }

            }
        }

        private void TimerSetting()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += new EventHandler(timer_Tick);
        }

        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgWipList);
                Util.SetTextBlockText_DataGridRowCount(tbWipListCount, "0");

                cboLabelCode.SelectedIndex = -1;

                //2019.03.12
                Util.gridClear(dgWipPrint);

                trvKeypartList.ItemsSource = null;

                txtLabelLot.Text = string.Empty;
                txtLabelLot.Tag = string.Empty;
                txtLabelScanID.Text = string.Empty;
                txtLabelScanID.Tag = string.Empty;
                txtLabelProduct.Text = string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setWipList()
        {
            try
            {
                //DA_PRD_SEL_WIP_PACK_ROUTE
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("TOPCNT", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = window_PROCESSINFO.EQSGID;
                dr["PROCID"] = window_PROCESSINFO.PROCID;
                dr["EQPTID"] = window_PROCESSINFO.EQPTID;
                dr["TOPCNT"] = (Int32)cboSearchCount.Value;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LABEL_PRT_REQ_HIST_BYLOT", "RQSTDT", "RSLTDT", RQSTDT);

                dgWipList.ItemsSource = null;
                dgWipList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dtResult.Rows.Count));
                //dgWipList.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_LABEL_PRT_REQ_HIST_BYLOT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        //2019.03.12
        private void setWipPrint()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("TOPCNT", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = window_PROCESSINFO.EQSGID;
                dr["PROCID"] = window_PROCESSINFO.PROCID;
                dr["EQPTID"] = window_PROCESSINFO.EQPTID;
                dr["TOPCNT"] = (Int32)cboSearchCount.Value;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LABEL_PRT_REQ_HIST_BYPRTFLAG", "RQSTDT", "RSLTDT", RQSTDT);

                dgWipPrint.ItemsSource = null;
                dgWipPrint.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// lot 결합이력 TreeView에 표시
        /// </summary>
        private void setLotInputMtrlTracking(string sLotid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = null;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_MTRL_TRACKING", "RQSTDT", "RSLTDT", RQSTDT);
                
                DataSet dsTree = new DataSet();
                DataRow drTreeTemp = dtResult.NewRow();

                drTreeTemp.ItemArray = new object[] { null, null, sLotid, sLotid, null, null, null, null, null, null, null, null };

                dtResult.Rows.InsertAt(drTreeTemp, 0);

                dsTree.Tables.Add(dtResult.Copy());
                dsTree.Relations.Add("Relations", dsTree.Tables["RSLTDT"].Columns["LOTID_RELATION"], dsTree.Tables["RSLTDT"].Columns["LOTID"]);

                dvRootNodes = dsTree.Tables["RSLTDT"].DefaultView;
                dvRootNodes.RowFilter = "LOTID IS NULL";
                trvKeypartList.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }        

        private void TimerLabelPrintNew(DataTable dt)
        {
            try
            {
                string sScanid = string.Empty;
                string sPRT_SEQ = string.Empty;
                string slotid = string.Empty;
                string sPRT_FLAG = string.Empty;

                setWipPrint();

                if (dgWipPrint.Rows.Count > 0)
                {

                    sScanid = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "SCAN_ID").ToString();
                    sPRT_SEQ = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "PRT_REQ_SEQNO").ToString();
                    slotid = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "LOTID").ToString();
                    sPRT_FLAG = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "PRT_FLAG").ToString();

                    labelPrintNew(slotid, sScanid, sPRT_SEQ, dt);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                rdoPageUnFixed.IsChecked = true;
                rdoPageFixed.IsChecked = false;
                bPrintYnPopupOpen = false;
                TimerStatus(false);
            }
        }

        private void labelPrintNew(string slotid, string sScanid, string sPRT_SEQ, DataTable dt)
        {
            try
            {
                string strProt = string.Empty;
                string strLotid = slotid;
                string strPrtSeq = sPRT_SEQ;

                if(!(dt.Rows.Count== 1))
                {
                    Util.MessageInfo("SFU8219");
                    rdoPageUnFixed.IsChecked = true;
                    rdoPageFixed.IsChecked = false;
                    bPrintYnPopupOpen = false;
                    TimerStatus(false);
                    return;
                }


                DataTable dtZpl = new DataTable();
                dtZpl.Columns.Add("PORTNAME", typeof(string));
                dtZpl.Columns.Add("ZPL", typeof(string));

                DataTable dtResult = Util.getZPL_Pack(sLOTID: slotid
                                                           , sPROCID: window_PROCESSINFO.PROCID
                                                           , sEQPTID: window_PROCESSINFO.EQPTID
                                                           , sLABEL_TYPE: LABEL_TYPE_CODE.PACK
                                                           , sLABEL_CODE: dt.Rows[0]["LABEL_CODE"].ToString()
                                                           , sSAMPLE_FLAG: "N"
                                                           , sPRN_QTY: dt.Rows[0]["PRT_QTY"].ToString()
                                                           , sDPI: dt.Rows[0]["PRTR_DPI"].ToString()
                                                           , sLEFT: dt.Rows[0]["PRT_X"].ToString()
                                                           , sTOP: dt.Rows[0]["PRT_Y"].ToString()
                                                           , sDARKNESS: dt.Rows[0]["PRT_DARKNESS"].ToString()
                                                           );
                if (dt.Rows[0]["PRTR_IP"].ToString().Equals("0.0.0.0"))
                {
                    strProt = "USB";
                }
                else
                {
                    Util.MessageInfo("SFU1310");
                    rdoPageUnFixed.IsChecked = true;
                    rdoPageFixed.IsChecked = false;
                    bPrintYnPopupOpen = false;
                    TimerStatus(false);
                }

                DataRow dr = dtZpl.NewRow();
                dr["PORTNAME"] = strProt;
                dr["ZPL"] = Util.NVC(dtResult.Rows[0]["ZPLSTRING"]);
                dtZpl.Rows.Add(dr);

                bool bPrintSucessFlag = Util.PrintLabel_AutoPrint_PACK(FrameOperation, loadingIndicator, dtZpl);

                if (bPrintSucessFlag)
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(sScanid, sPRT_SEQ);

                    showLabelPrintInfoPopup(slotid, sScanid);
                }
                else
                {
                    rdoPageFixed.IsChecked = false;
                    rdoPageUnFixed.IsChecked = true;
                    timer.Stop();
                    timer.IsEnabled = false;
                    timer = null;

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show("SFU8219", slotid, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            lblHistCanel(strLotid, strPrtSeq);
                            return;
                        }
                        else
                        {
                            return;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                rdoPageUnFixed.IsChecked = true;
                rdoPageFixed.IsChecked = false;
                bPrintYnPopupOpen = false;
                TimerStatus(false);
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
                    newRow["LOTID_TITLE"] = "LOTID";
                    newRow["SCANID_TITLE"] = "SCANID";
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
            catch(Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
            }
        }

        /// <summary>
        /// TB_SFC_LABEL_PRT_REQ_HIST 
        /// PRT_FLAG = 'Y' 로 UPDATE
        /// </summary>
        /// <param name="sScanid"></param>
        /// <param name="sPRT_SEQ"></param>
        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, string sPRT_SEQ)
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
                dr["PRT_REQ_SEQNO"] = Util.NVC_Int(sPRT_SEQ);
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
                Util.MessageException(ex);
            }
        }

        private DataTable chkComPort(string strProdid = null)
        {
            try
            {
                DataTable dReturnTable = null;

                if (window_PROCESSINFO.PROCID == null || window_PROCESSINFO.EQPTID == null || window_PROCESSINFO.EQSGID == null) return null;

                DataSet dsINDATA = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("USE_FLAG", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = window_PROCESSINFO.EQSGID.ToString();
                dr["EQPTID"] = window_PROCESSINFO.EQPTID.ToString();
                dr["PROCID"] = window_PROCESSINFO.PROCID.ToString();
                dr["PRODID"] = strProdid == null ? null : strProdid;
                dr["USE_FLAG"] = "Y";
                INDATA.Rows.Add(dr);

                dsINDATA.Tables.Add(INDATA);
                loadingIndicator.Visibility = Visibility.Visible;


                dReturnTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "INDATA", "OUTDATA", INDATA);

                if (dReturnTable == null && !(dReturnTable.Rows.Count > 1))
                {
                    Util.MessageInfo("SFU8219");
                    rdoPageUnFixed.IsChecked = true;
                    rdoPageFixed.IsChecked = false;
                    bPrintYnPopupOpen = false;
                    TimerStatus(false);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return null;
                }

                loadingIndicator.Visibility = Visibility.Collapsed;
                return dReturnTable;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
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
                Util.MessageException(ex);
                //throw ex;
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


        private void SetGridCbo_LabelDPI(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
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
                else
                {
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLocalPrinter(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dtResult = new DataTable();

            dtResult.Columns.Add("CBO_CODE", typeof(string));
            dtResult.Columns.Add("CBO_NAME", typeof(string));

            var printerQuery = new ManagementObjectSearcher("Select * from Win32_Printer");

            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");

                DataRow newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { name, name };
                dtResult.Rows.Add(newRow);
            }

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
            //바인딩후 라벨코드의 첫번째 값 기본세팅
            if (dtResult.Rows.Count > 0)
            {
                for (int i = 0; i < dgLabelPortSetting.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgLabelPortSetting.Rows[i].DataItem, "PRINTERNAME", Util.NVC(dtResult.Rows[0]["CBO_CODE"]));
                }

            }
            else
            {
                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
            }
        }

        public static List<string> getPermissionList(DataTable dt, string strColumn)
        { 
            List<string> result = (from r in dt.AsEnumerable() select r.Field<string>(strColumn)).ToList();

            return result;

        }

        private void dgWipList_GotFocus(object sender, RoutedEventArgs e)
        {
            cboLabelCode.IsEnabled = true;
        }

        #endregion Method

        #region [ 설비별 LableCode 가져오기 ]
        /// <summary>
        /// 설비별 라벨 프린터 정보 ( PACK001_043.xaml ) 의 세팅되어 있는 Lable Code 조회
        /// </summary>
        /// <param name="sEQSG"></param>
        /// <param name="sEQPT"></param>
        /// <param name="sPROD"></param>
        /// <returns></returns>
        private Boolean GetEqptLabelCode(string strPROCID, string strEQPTID, string strEQSG, string strPROD)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = strPROCID;
                dr["EQPTID"] = strEQPTID;
                dr["EQSGID"] = strEQSG;
                dr["PRODID"] = strPROD;
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);


                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && !(Util.NVC(dtResult.Rows[0]["PRTR_IP"]).ToString().Equals("0.0.0.0")))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region [ 라벨 요청 내용 삭제 ]
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtCurrentReqNo.Text.ToString()) || string.IsNullOrEmpty(txtCurrentLot.Text.ToString()))
                {
                    Util.MessageInfo("SFU1261");
                    return;
                }
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = txtCurrentLot.Text.ToString();
                dr["USERID"] = LoginInfo.USERID;
                dr["PRT_REQ_SEQNO"] = txtCurrentReqNo.Text.ToString();
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_CANCEL", "RQSTDT", "", RQSTDT);


                txtCurrentReqNo.Text = null;
                txtCurrentLot.Text = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void lblHistCanel(string strLotid, string strReqNo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = strLotid;
                dr["USERID"] = LoginInfo.USERID;
                dr["PRT_REQ_SEQNO"] = strReqNo;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_CANCEL", "RQSTDT", "", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [ 라벨 요청 내용 삭제 권한 조회 ]
        private Boolean chkAUTH()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_LBL_PRT_REQ_CANCEL_USERID";
                dr["CMCODE"] = LoginInfo.USERID;
                

                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);

                if (dtAuth.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion
    }
}
