/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack LABEL 자동발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2017.10.27 손우석 CSR ID:3516073] 오창 팩11호 BMW12V BMA 수작업 조립공정 작업성 개선을 위한 GMES 수정 요청 | [요청번호]C20171027_16073
  2018.06.16 손우석 SM 폴란드 라벨 발행시 SHOPID 추가 요청
  2018.06.21 염규범 폴란드 라벨 발행 SHOPID 오류 수정
  2019.03.12 손우석 Timeout 현상 대책으로 이력 조회 와 발행 대상 조회 비즈 및 화면 분리 표시
  2019.05.06 손우석 비즈 에러 발생시 Timer 종료 추가
  2019.05.23 손우석 CSR ID 3997009 [G.MES] LGCWA_Audi C-BEV_MES 라벨 코드 선택 방식 변경 | [요청번호]C20190518_97009
  2019.07.07 염규범 이정진 책임 요청의 건 ( 선처리 요청 - 김정균 책임님 ) 
  2019.10.14 강호운 바코드 인쇄시 네트워크 프린터 화면에서 설정되어 있는 라벨코드 우선으로 설정후 출하처 인쇄항목 기준 조회 GetEqptLabelInfo()
  2019.12.11 염규범 자동 조회, 자동 발행 구분 및, 바코드 발행 여부 정확히 Validation 처리
  2020.03.19 염규범 라벨 발행 요청 취소 기능 추가, 공통코드 : PACK_LBL_PRT_REQ_CANCEL_USERID 로 권한 관리
  2020.03.31 염규범 과거 Message 처리 변경 ( Alert -> MessageException )
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
//2017.10.27
using System.Management;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_002 : UserControl, IWorkArea 
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        string now_labelcode = string.Empty;
        string strEqsgId = string.Empty;
        string strProcId = string.Empty;
        string strEqptId = string.Empty;
        string strProdId = string.Empty;

        string strCurrentLotID = string.Empty;
        string strCurrentReqNo = string.Empty;

        string strAu = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private PACK001_001_PROCESSINFO window_PROCESSINFO = new PACK001_001_PROCESSINFO();
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

        public PACK001_002()
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

                setComPort();
                //TimerSetting();
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
                PACK001_001_SELECTPROCESS popup = new PACK001_001_SELECTPROCESS();
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

                //2019.03.12
                setWipPrint();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
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
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLabelLot.Text.Length > 0)
                {
                    string sSelectLotid = txtLabelLot.Text;
                    //2019.05.23
                    //string sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                    string sLabelCode = string.Empty;

                     switch (sEqptLabel != "")
                    {
                        case true:
                            sLabelCode = sEqptLabel;
                            break;

                        case false:
                            sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                            break;

                        default:
                            sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                            break;
                    }

                    now_labelcode = sLabelCode;

                    if (sLabelCode.Length > 0)
                    {
                        Util.printLabel_Pack(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", window_PROCESSINFO.PRODUCTID);
                    }
                    else
                    {
                        Util.printLabel_Pack(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, "N", "1", null);
                    }

                    if (Util.NVC(txtLabelLot.Tag) == "N")//print 여부 N인경우 Y로 update
                    {
                        updateTB_SFC_LABEL_PRT_REQ_HIST(txtLabelScanID.Text, Util.NVC_Int(txtLabelScanID.Tag.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

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
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
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

                    //2019-07-03
                    //
                    DataTable dTLableCode = GetEqptLabelInfo(window_PROCESSINFO.EQSGID, window_PROCESSINFO.PROCID, window_PROCESSINFO.EQPTID, window_PROCESSINFO.PRODUCTID);

                    if (dTLableCode != null)
                    {
                        List<string> lStrLableCode = getPermissionList(dTLableCode, "LABEL_CODE");

                        string strLableCode = string.Empty;
                        strLableCode = string.Join(",", lStrLableCode.ToArray());
                        setLabelCode(sSelectProductId, strLableCode);
                    }
                    else
                    {
                        setLabelCode(sSelectProductId, string.Empty);
                    }


                    setLotInputMtrlTracking(sSelectLotid);
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

                    //2019-07-03
                    DataTable dTLableCode = GetEqptLabelInfo(window_PROCESSINFO.EQSGID, window_PROCESSINFO.PROCID, window_PROCESSINFO.EQPTID, window_PROCESSINFO.PRODUCTID);

                    if (dTLableCode != null)
                    {
                        List<string> lStrLableCode = getPermissionList(dTLableCode, "LABEL_CODE");

                        string strLableCode = string.Empty;
                        strLableCode = string.Join(",", lStrLableCode.ToArray());
                        setLabelCode(sSelectProductId, strLableCode);
                    }
                    else
                    {
                        setLabelCode(sSelectProductId, string.Empty);
                    }
                    
                    setLotInputMtrlTracking(sSelectLotid);

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
                if (!(window_PROCESSINFO.PROCID.Length > 0))
                {
                    TimerStatus(false);
                    Util.MessageInfo("MMD0005");
                    //ms.AlertWarning("MMD0005"); //공정을 선택해 주세요.
                    rdoPageUnFixed.IsChecked = true;
                }
                else
                {
                    if (Util.NVC(cboPrintYn.SelectedValue) == "Y")
                    {
                        if (bPrintYnPopupOpen)
                        {
                            openPopUp_PrintYN();
                            //popupPrintYn_Closed 에서 TimerStatus(true); 실행 차후 발행pc IP 기준정보화 되면 수정해야함!!! 임시로 Y인경우만 확인 팝업으로 알림.
                            //TimerStatus(true);
                        }
                    }
                    else
                    {
                        TimerStatus(true);
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
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
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion Check 

        void popup_Closed(object sender, EventArgs e)
        {
            try
            { 
                PACK001_001_SELECTPROCESS popup = sender as PACK001_001_SELECTPROCESS;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    DataRow drSelectedProcess = popup.DrSelectedProcessInfo;

                    if (drSelectedProcess != null)
                    {
                        window_PROCESSINFO.setProcess(drSelectedProcess);

                        tbTitle.Text = popup.SSelectedProcessTitle;

                        //SetGridCbo_LabelDPI(dgLabelPortSetting.Columns["DPI"]);

                        //2019.05.23
                        DataTable dTLableCode = GetEqptLabelInfo(window_PROCESSINFO.EQSGID, window_PROCESSINFO.PROCID, window_PROCESSINFO.EQPTID, window_PROCESSINFO.PRODUCTID);

                        if (dTLableCode != null)
                        {
                            List<string> lStrLableCode = getPermissionList(dTLableCode, "LABEL_CODE");

                            string strLableCode = string.Empty;
                            strLableCode = string.Join(",", lStrLableCode.ToArray());
                            //처음 아래 세팅 제거
                            //setLabelCode(window_PROCESSINFO.PRODUCTID, strLableCode);

                            //그리드 라벨 코드 setting
                            SetGridCbo_LabelCode(dgLabelPortSetting.Columns["LABEL_CODE"], strLableCode);
                        }
                        else
                        {
                            SetGridCbo_LabelCode(dgLabelPortSetting.Columns["LABEL_CODE"], string.Empty);
                        }

                        strEqsgId = window_PROCESSINFO.EQSGID;
                        strProcId = window_PROCESSINFO.PROCID;
                        strEqptId = window_PROCESSINFO.EQPTID;
                        strProdId = window_PROCESSINFO.PRODUCTID;

                        Refresh();
                        cboLabelCode.IsEnabled = false;
                    }

                    //2019.12.10
                    if (!GetEqptLabelCode(window_PROCESSINFO.PROCID, window_PROCESSINFO.EQPTID, window_PROCESSINFO.EQSGID, window_PROCESSINFO.PRODUCTID))
                    {
                        AutoPrint.Text = "자동발행";
                        cboPrintYn.SelectedValue = 'Y';
                        cboPrintYn.IsEnabled = true;
                    }
                    else
                    {   
                        AutoPrint.Text = "자동조회";
                        cboPrintYn.SelectedValue = 'N';
                        cboPrintYn.IsEnabled = false;
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
                //Util.Alert(ex.ToString());
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
                //Util.Alert(ex.ToString    ());
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
                    TimerLabelPrint();
                }
            }
            catch (Exception ex)
            {
                showErrorPopup(ex);
                //Util.Alert(ex.ToString());
                //2019.05.06
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
                window_PROCESSINFO.PACK001_002 = this;
                dgWorkInfo.Children.Add(window_PROCESSINFO);
            }
        }

        public void TimerStatus(bool bOn)
        {
            if (bOn)
            {
                int iInterval = (int)(cboAutoSearchTime.Value) > 2 ? (int)(cboAutoSearchTime.Value) : 3;
                bPrintYnPopupOpen = false;
                //rdoPageFixed.IsChecked = true;
                //rdoPageUnFixed.IsChecked = false;
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

                //timer.IsEnabled = true;
                //timer.Interval = new TimeSpan(0, 0, 0, iInterval);
                //timer.Tick += new EventHandler(timer_Tick);
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

                btnProdutLotSearch.IsEnabled = true;
                btnLabel.IsEnabled = true;
                btnProcessSelect.IsEnabled = true;
                btnKeyPartCopy.IsEnabled = true;
                cboAutoSearchTime.IsEnabled = true;
                cboSearchCount.IsEnabled = true;
                dgLabelPortSetting.IsEnabled = true;

                if (!GetEqptLabelCode(strProcId, strEqptId, strEqsgId, strProdId))
                {
                    cboPrintYn.IsEnabled = true;
                }
                else
                {
                    cboPrintYn.IsEnabled = false;
                }

                //timer.IsEnabled = false;
                //timer.Tick -= new EventHandler(timer_Tick);

                if (timer != null)
                {
                    timer.Stop();
                    timer.IsEnabled = false;
                    timer = null;
                }

                //cboAutoSearch.SelectedValue = "N";
                //cboAutoSearch.IsEnabled = true;
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

                //실적 수량 재조회
                window_PROCESSINFO.setPlanQty();
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

                //dsTree.Relations.Add("Relations", dsTree.Tables["RSLTDT"].Columns["INPUT_LOTID"], dsTree.Tables["RSLTDT"].Columns["LOTID"]);
                dsTree.Relations.Add("Relations", dsTree.Tables["RSLTDT"].Columns["LOTID_RELATION"], dsTree.Tables["RSLTDT"].Columns["LOTID"]);

                dvRootNodes = dsTree.Tables["RSLTDT"].DefaultView;
                dvRootNodes.RowFilter = "LOTID IS NULL";
                trvKeypartList.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_INPUT_MTRL_TRACKING", ex.Message, ex.ToString());
                Util.MessageException(ex);
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

                //2019.03.12
                setWipPrint();

                if (dgWipPrint.Rows.Count > 0)
                {

                    sScanid = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "SCAN_ID").ToString();
                    sPRT_SEQ = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "PRT_REQ_SEQNO").ToString();
                    slotid = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "LOTID").ToString();
                    sPRT_FLAG = DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "PRT_FLAG").ToString();

                    //바코드 인쇄 시점에서 선택된 LOTID 의 제품코드 기준으로 바코드 라벨ID 를 산출하여 설정후 인쇄 강호운 책임 2019-10-10
                    //string sProductId = Util.NVC(DataTableConverter.GetValue(dgWipPrint.Rows[0].DataItem, "PRODID"));

                    //DataTable dTLableCode = GetEqptLabelInfo(window_PROCESSINFO.EQSGID, window_PROCESSINFO.PROCID, window_PROCESSINFO.EQPTID, sProductId);

                    //if (dTLableCode != null)
                    //{
                    //    List<string> lStrLableCode = getPermissionList(dTLableCode, "LABEL_CODE");

                    //    string strLableCode = string.Empty;
                    //    strLableCode = string.Join(",", lStrLableCode.ToArray());
                    //    setLabelCode(sProductId, strLableCode);
                    //}
                    //else
                    //{
                    //    setLabelCode(sProductId, string.Empty);
                    //}

                    labelPrint(slotid, sScanid, Util.NVC_Int(sPRT_SEQ));

                    //string sFiter = "PRT_FLAG = 'N'";
                    //string sSort = "INSDTTM ASC";
                    //DataRow[] drPrint = dtTemp.Select(sFiter, sSort);

                    //if (drPrint.Length > 0)
                    //{
                    //    sScanid = Util.NVC(drPrint[0]["SCAN_ID"]);
                    //    sPRT_SEQ = Util.NVC(drPrint[0]["PRT_REQ_SEQNO"]);
                    //    slotid = Util.NVC(drPrint[0]["LOTID"]);
                    //    sPRT_FLAG = Util.NVC(drPrint[0]["PRT_FLAG"]);

                    //    if (slotid != "" && sPRT_FLAG == "N" && !blZplPrinting && bPrintYn_Flag)
                    //    {
                    //        labelPrint(slotid, sScanid, sPRT_SEQ);
                    //    }
                    //}
                    //else
                    //{
                    //    sScanid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[0].DataItem, "SCAN_ID"));
                    //    sPRT_SEQ = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[0].DataItem, "PRT_REQ_SEQNO"));
                    //    slotid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[0].DataItem, "LOTID"));

                    //    sPRT_FLAG = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[0].DataItem, "PRT_FLAG"));

                    //    if (slotid != "" && sPRT_FLAG == "N" && !blZplPrinting && bPrintYn_Flag)
                    //    {
                    //        labelPrint(slotid, sScanid, sPRT_SEQ);
                    //    }
                    //}
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void labelPrint(string slotid , string sScanid, Int32 sPRT_SEQ)
        {
            try
            {
                blZplPrinting = true;

                DataTable dtZpl = new DataTable();
                dtZpl.Columns.Add("PORTNAME", typeof(string));
                dtZpl.Columns.Add("ZPL", typeof(string));

                int iDataGridCount_LabelPortSetting = dgLabelPortSetting.Rows.Count;

                for (int i=0; i< iDataGridCount_LabelPortSetting; i++)
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

                        DataTable dtResult = Util.getZPL_Pack(sLOTID : slotid
                                                            , sLABEL_TYPE :  LABEL_TYPE_CODE.PACK
                                                            , sLABEL_CODE : sLABEL_CODE
                                                            , sSAMPLE_FLAG : "N"
                                                            , sPRN_QTY : sPRN_QTY
                                                            , sDPI: sDPI
                                                            , sLEFT: sX
                                                            , sTOP: sY
                                                            , sDARKNESS: sDARKNESS
                                                            );
                        //2017.10.27 ZPL이 없는 경우 체크
                        if (Util.NVC(dtResult.Rows[0]["ZPLSTRING"]).ToString().Contains("is not exist"))
                        {
                            Util.MessageInfo("Design information for Label (" + sLABEL_CODE + ") is not exist !");
                            return;
                        }

                        DataRow dr = dtZpl.NewRow();
                        dr["PORTNAME"] = sPORTNAME;
                        dr["ZPL"] = Util.NVC(dtResult.Rows[0]["ZPLSTRING"]);
                        dtZpl.Rows.Add(dr);
                    }
                    //Util.printLabel_Pack(FrameOperation, loadingIndicator, sScanid, "PROC", "N", "1");
                }

                //2017.10.27 Setting에 체크되어 있는 기준으로 인쇄
                //bool bPrintSucessFlag = Util.PrintLabel(FrameOperation, loadingIndicator, dtZpl);
                bool bPrintSucessFlag = Util.PrintLabel_AutoPrint_PACK(FrameOperation, loadingIndicator, dtZpl);

                if (bPrintSucessFlag)
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(sScanid, sPRT_SEQ);

                    showLabelPrintInfoPopup(slotid, sScanid);
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("BARCODEPRINT실패\n프린트연결상태를확인하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    throw new System.InvalidOperationException(ms.AlertRetun("SFU1310")); //BARCODEPRINT실패\r\n프린트연결상태를확인하세요.
                    Util.MessageInfo("SFU1310");

                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
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

        private void setYnComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                String[] sFilter3 = { "IUSE" };
                _combo.SetCombo(cboPrintYn, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");


                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["CMCDTYPE"] = "IUSE";
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //cboAutoSearch.DisplayMemberPath = "CBO_NAME";
                //cboAutoSearch.SelectedValuePath = "CBO_CODE";
                //cboAutoSearch.ItemsSource = DataTableConverter.Convert(dtResult);

                //cboAutoSearch.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void setComPort()
        {
            try
            {
                //세팅전 콤보박스 세팅
                SetGridCbo_LabelDPI(dgLabelPortSetting.Columns["DPI"]);
                //2017.10.27
                SetLocalPrinter(dgLabelPortSetting.Columns["PRINTERNAME"]);

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
                //2017.10.27
                dtLabelPortSetting.Columns.Add("PRINTERNAME", typeof(string));

                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    DataRow drLabelPortSetting = dtLabelPortSetting.NewRow();
                    drLabelPortSetting["CHK"] = Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString());
                    drLabelPortSetting["PORTNAME"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME];
                    drLabelPortSetting["LABEL_CODE"] = "";
                    drLabelPortSetting["LABEL_CNT"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES];
                    drLabelPortSetting["DPI"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI];
                    drLabelPortSetting["X"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X];
                    drLabelPortSetting["Y"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y];
                    drLabelPortSetting["DARKNESS"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS];
                    //2017.10.27
                    drLabelPortSetting["PRINTERNAME"] = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME];

                    dtLabelPortSetting.Rows.Add(drLabelPortSetting);
                }

                dgLabelPortSetting.ItemsSource = DataTableConverter.Convert(dtLabelPortSetting);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
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

        private void SetGridCbo_LabelCode(C1.WPF.DataGrid.DataGridColumn col, string strLableCode)
        {
            try
            {
                if (window_PROCESSINFO.PRODUCTID.Length > 0)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    
                    //김정균 책임님 소스수정 요청
                    //RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("LABEL_TYPE_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PRODID"] = window_PROCESSINFO.PRODUCTID;
                    //2018.06.16
                    //dr["SHIPTO_ID"] = null;

                    // 김정균 책임님 소스 수정 요청 
                    //dr["SHIPTO_ID"] = LoginInfo.CFG_SHOP_ID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["PROCID"] = null;
                    dr["LABEL_TYPE_CODE"] = LABEL_TYPE_CODE.PACK;

                    //2019-07-03
                    //이정진 책임 요청의건
                    if (!string.IsNullOrEmpty(strLableCode))
                    {
                        RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                        dr["LABEL_CODE"] = strLableCode;
                    }

                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                    //바인딩후 라벨코드의 첫번째 값 기본세팅
                    if (dtResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgLabelPortSetting.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgLabelPortSetting.Rows[i].DataItem, "LABEL_CODE", Util.NVC(dtResult.Rows[0]["CBO_CODE"]));
                        }

                        //2019.05.23
                        if (sEqptLabel != "")
                        {
                            DataTableConverter.SetValue(dgLabelPortSetting.Rows[0].DataItem, "LABEL_CODE", sEqptLabel);
                        }
                    }
                }
                else
                {
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                }
                
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", ex.Message, ex.ToString());
                Util.MessageException(ex);
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

                //if (window_PROCESSINFO.PRODUCTID.Length > 0)
                //{
                //    DataTable RQSTDT = new DataTable();
                //    RQSTDT.TableName = "RQSTDT";
                //    RQSTDT.Columns.Add("LANGID", typeof(string));
                //    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                //    DataRow dr = RQSTDT.NewRow();
                //    dr["LANGID"] = LoginInfo.LANGID;
                //    dr["CMCDTYPE"] = "PRINTER_RESOLUTION";
                //    RQSTDT.Rows.Add(dr);

                //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                //    //바인딩후 라벨코드의 첫번째 값 기본세팅
                //    if (dtResult.Rows.Count > 0)
                //    {
                //        for (int i = 0; i < dgLabelPortSetting.Rows.Count; i++)
                //        {
                //            DataTableConverter.SetValue(dgLabelPortSetting.Rows[i].DataItem, "DPI", Util.NVC(dtResult.Rows[0]["CBO_CODE"]));
                //        }
                         
                //    }
                //}
                //else
                //{
                //    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                //}

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_BAS_SEL_COMMCODE_CBO", ex.Message, ex.ToString());
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

        private void setLabelCode(string sProdID, string strLableCode)
        {
            try
            {
                if (sProdID.Length > 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //라벨 세팅
                    //String[] sFilter = { sProdID, null, null, LABEL_TYPE_CODE.PACK };
                    //_combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");
                    String[] sFilter = { sProdID, null, null, LABEL_TYPE_CODE.PACK, strLableCode };
                    _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD_MULTI");

                    int combo_cnt = cboLabelCode.Items.Count;

                    for (int i = 0; i < combo_cnt; i++)
                    {
                        DataRowView drv = cboLabelCode.Items[i] as DataRowView;
                        string temp = drv["CBO_CODE"].ToString();

                        if (now_labelcode == temp)
                        {
                            cboLabelCode.SelectedValue = now_labelcode;
                            break;
                        }
                        else
                        {
                            //2019.05.23
                            if (sEqptLabel != "")
                            {
                                cboLabelCode.SelectedValue = sEqptLabel;
                            }
                            else
                            {
                                cboLabelCode.SelectedIndex = 0;
                            }
                            
                        }
                    }
                }
                else
                {
                    cboLabelCode.ItemsSource = null;
                    cboLabelCode.SelectedValue = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
            }
        }

        //2019.05.23
        private DataTable GetEqptLabelInfo(string sEQSG, string sPROC, string sEQPT, string sPROD)
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_EQPT_LABEL_INFO";
                dr["EQSGID"] = sEQSG;
                dr["PROCID"] = sPROC;
                dr["EQPTID"] = sEQPT;
                dr["PRODID"] = sPROD;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult  = null;
                // 공통 코드상 설정 데이타를 제외 처리 하고 [설비별 라벨 프린터 정보] 화면 내역 조회 20191007 강호운
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "RQSTDT", "RSLTDT", RQSTDT);
                //    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_LABEL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    sEqptLabel = Util.NVC(dtResult.Rows[0]["LABEL_CODE"]);
                    return dtResult;
                }
                else
                {
                    sEqptLabel = "";
                    return null;
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /*
        /// <summary>
        /// PACK 에서 사용
        /// LOTID 필수입력
        /// LABELCODE미입력시 등록된 LABELCODE수만큼 ZPL
        /// </summary>
        /// <param name="sLOTID"></param>
        /// <param name="sPROCID"></param>
        /// <param name="sEQPTID"></param>
        /// <param name="sEQSGID"></param>
        /// <param name="sLABEL_TYPE"></param>
        /// <param name="sLABEL_CODE"></param>
        /// <param name="sSAMPLE_FLAG">샘플발행여부Y N</param>
        /// <param name="sPRN_QTY">프린트수량</param>
        /// <returns>DataTable LABEL_TYPE,ZPLSTRING</returns>
        private DataTable getZPL_Pack(string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG , string sPRN_QTY)
        {
            DataTable dtResult = null;
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LABEL_TYPE", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                RQSTDT.Columns.Add("SAMPLE_FLAG", typeof(string));
                RQSTDT.Columns.Add("PRN_QTY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLOTID;
                dr["PROCID"] = sPROCID;
                dr["EQPTID"] = sEQPTID;
                dr["EQSGID"] = sEQSGID;
                dr["LABEL_TYPE"] = sLABEL_TYPE;
                dr["LABEL_CODE"] = sLABEL_CODE;
                dr["SAMPLE_FLAG"] = sSAMPLE_FLAG;
                dr["PRN_QTY"] = sPRN_QTY;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        /// <summary>
        /// PACK에서 사용
        /// zpl조회후 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="sLOTID"></param>
        /// <param name="sLABEL_TYPE">PROC,PLT,OUTBOX</param>
        /// <param name="sSAMPLE_FLAG">프린트수량</param>
        /// <param name="sPRN_QTY">프린트수량</param>
        private void printLabel_Pack(string sLOTID, string sLABEL_TYPE,string sSAMPLE_FLAG, string sPRN_QTY)
        {
            try
            {
                DataTable dtResult = getZPL_Pack(sLOTID, null, null, null, sLABEL_TYPE, null, sSAMPLE_FLAG, sPRN_QTY);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                    PrintLabel(zpl);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// PACK에서 사용
        /// zpl조회후 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="sLOTID"></param>
        /// <param name="sLABEL_TYPE">PROC,PLT,OUTBOX</param>
        /// <param name="sLABEL_CODE">라벨양식코드</param>
        /// <param name="sSAMPLE_FLAG">샘플출력Y/N</param>
        private void printLabel_Pack(string sLOTID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG,string sPRN_QTY)
        {
            try
            {
                DataTable dtResult = getZPL_Pack(sLOTID, null, null, null, sLABEL_TYPE, sLABEL_CODE, sSAMPLE_FLAG, sPRN_QTY);

                for(int i=0; i < dtResult.Rows.Count; i++)
                {
                    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                    PrintLabel(zpl);
                }
            }
            catch(Exception ex)
            {                
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="sZPL"></param>
        private void PrintLabel(string sZPL)
        {
            try
            {
                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    {
                        FrameOperation.PrintFrameMessage(string.Empty);
                        bool brtndefault = FrameOperation.Barcode_ZPL_Print(dr, sZPL);
                        if (brtndefault == false)
                        {
                            loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                            FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("BARCODEPRINT실패"));
                            return;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        */
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
    }
}
