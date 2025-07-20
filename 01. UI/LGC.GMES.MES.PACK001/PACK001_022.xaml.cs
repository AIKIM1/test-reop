/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.10.17  손우석 CSR ID 3501601 171010_팩GMES 셀 반품 기능 개선요청 요청번호 C20171010_01601
  2020.07.23  김길용 Cell 반품 관련 개선 요청 건 [요청번호]C20200211-000400 서비스 번호 30205
  2020.11.30  김준겸 Cell 반품 관련 개선  및 IWMS 사용유무 Logic 추가. [요청번호] C20201127-000086 [서비스 번호] 119235
  2023.06.22  하예슬 RTD 구축 - BCDID 기능 추가 건
  2025.04.15  김영택 NERP 대응, NERP 적용시 반품 창고 변경, 공통코드 (RETURN_SLOC_MAPPING) (2025.06.23 git 머지)
**************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_022 : UserControl, IWorkArea
    {
        ExcelMng exl = new ExcelMng();
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private string sFromArea = string.Empty;
        private string sToArea = string.Empty;
        private string sEmpty_Lot = string.Empty;
        private string sTabId = string.Empty;
        private bool bTray = false;
        private bool bLoaded = false;
        private string sTrayID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        Util util = new Util();
        public PACK001_022()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        public static void MessageValidationWithCallBack(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region BCRID 
                DataTable dtReturn = GetCommonCode("CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);

                if (dtReturn.Rows.Count > 0)
                {                   
                    borderBCRID.Visibility = Visibility.Visible;
                    borderBCRID2.Visibility = Visibility.Visible;
                    ListBCDID.Visibility = Visibility.Visible;
                    SearchListBCDID.Visibility = Visibility.Visible;
                }
                else
                {            
                    borderBCRID.Visibility = Visibility.Collapsed;
                    borderBCRID2.Visibility = Visibility.Collapsed;
                    ListBCDID.Visibility = Visibility.Collapsed;
                    SearchListBCDID.Visibility = Visibility.Collapsed;
                }

                btnBCDchagne.Visibility = Visibility.Visible;
                btnBCDconfirm.Visibility = Visibility.Hidden;
                #endregion //BCRID

                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnTagetInputComfirm);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                setComboBox();

                tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                #region Tray물류여부 체크
                DataTable dtDiffusionSite  = dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "CELL_TRAY_LOGIS");
                if (null != dtDiffusionSite && dtDiffusionSite.Rows.Count > 0)
                {
                    if (Util.NVC(dtDiffusionSite.Rows[0]["ATTRIBUTE2"]).Contains(LoginInfo.CFG_AREA_ID))
                    {
                        bTray = true;
                        this.rdoTray.Visibility = Visibility.Visible;
                        this.rdoTray.IsChecked = true;
                         
                        this.colCstId.Visibility = Visibility.Visible;
                        this.btnTagetSelectCancel.Visibility = Visibility.Collapsed;
                        this.btnTagetCancel.Visibility = Visibility.Collapsed;
                    }
                }
                #endregion //Tray물류여부 체크

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            finally
            {
                bLoaded = true;
            }
        }

        #region Event - Button & TextBox
        private void btnReturnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSearchResultList.CurrentRow != null)
                {
                    string sRCV_ISS_ID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.CurrentRow.DataItem, "RCV_ISS_ID"));

                    //반품테그정보 조회
                    DataTable dtReturnTagInfo = getReturnTagInfo(sRCV_ISS_ID);

                    //반품테그발행
                    printReturnTag(dtReturnTagInfo);
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("발행할반품번호를선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3398"); //발행할 반품번호를 선택하세요.
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtReturnID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    fnKeyDown();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtReturnResn_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if(txtReturnID.Text.Length>0 && txtReturnResn.Text.Length > 0)
                    {
                        fnKeyDown();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void fnKeyDown()
        {
            if (Check_Input())
            {
                sTrayID = string.Empty;

                DataTable INPUT_ID_LIST = new DataTable();
                INPUT_ID_LIST.TableName = "INPUT_ID_LIST";
                INPUT_ID_LIST.Columns.Add("RCV_ISS_ID", typeof(string));
                INPUT_ID_LIST.Columns.Add("BOXID", typeof(string));
                INPUT_ID_LIST.Columns.Add("LOTID", typeof(string));
                INPUT_ID_LIST.Columns.Add("TRAYID", typeof(string));
                INPUT_ID_LIST.Columns.Add("RTN_RSN_NOTE", typeof(string));

                DataRow drINPUT_ID_LIST = INPUT_ID_LIST.NewRow();
                drINPUT_ID_LIST["RCV_ISS_ID"] = (bool)rdoRcvIss.IsChecked ? txtReturnID.Text : null;
                drINPUT_ID_LIST["BOXID"] = (bool)rdoPallet.IsChecked ? txtReturnID.Text : null;
                drINPUT_ID_LIST["LOTID"] = (bool)rdoCell.IsChecked || (bool)rdoNodata.IsChecked ? txtReturnID.Text : null;
                if ((bool)rdoTray.IsChecked)
                {
                    drINPUT_ID_LIST["TRAYID"] = txtReturnID.Text;
                    sTrayID = txtReturnID.Text;
                }
                drINPUT_ID_LIST["RTN_RSN_NOTE"] = txtReturnResn.Text;
                INPUT_ID_LIST.Rows.Add(drINPUT_ID_LIST);

                getReturnID_getLotInfo(INPUT_ID_LIST);
            }
        }

        private void btnReturnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTagetList.Rows.Count > 0 || txtReturnNumber.Text.Length > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1701"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                    //신규작성하시겠습니까?
                    {
                        if (sResult == MessageBoxResult.OK)
                        {
                            Refresh();
                        }
                    });
                }else
                {
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgTagetList != null)
                {
                    int iReturnValidationQty = ChkReturnCellQty();

                    if (dgTagetList.GetRowCount() > iReturnValidationQty)
                    {
                        Util.MessageValidation("SFU5015", iReturnValidationQty);
                        return;
                    }
                }

                if (Check_ReturnConfirm())
                {
                    DataTable dt = DataTableConverter.Convert(dgTagetList.ItemsSource);

                    DataRow[] drINPUT_LOT = dt.Select("PROC_INPUT_FLAG = 'Y'");

                    if(drINPUT_LOT.Length > 500)
                    {
                        //메시지 입력 500개 이상이면
                        Util.Alert("SFU8102");  //최대 1000자까지 가능합니다.
                        return;

                    }
                    else if (drINPUT_LOT.Length > 0 && drINPUT_LOT.Length < 500)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("신규"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //투입된 CELL이 존재합니다.\n반품처리 하시겠습니까?
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReturnCell();
                            }
                        });
                    }
                    else
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU2074"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //반품 처리 하시겠습니까?
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                if (drINPUT_LOT.Length > 500)
                                {
                                    //메시지 입력 500개 이상이면
                                    Util.Alert("SFU8102");  //최대 1000자까지 가능합니다.
                                    return;

                                }else
                                {
                                    ReturnCell();
                                }
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtTempTagetList = DataTableConverter.Convert(dgTagetList.ItemsSource);

                for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                {
                    if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                        Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                    {

                        dtTempTagetList.Rows[i].Delete();
                        dtTempTagetList.AcceptChanges();
                    }
                }
                dgTagetList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtTempTagetList.Rows.Count));

                if (!(dtTempTagetList.Rows.Count > 0))
                {
                    dgTagetList.ItemsSource = null;
                    Refresh();
                }
                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnBCDIDchange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnBCDchagne.Visibility = Visibility.Hidden;
                btnBCDconfirm.Visibility = Visibility.Visible;
                txtBCRID.IsReadOnly = false;
                txtBCRID.IsEnabled = true; 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnBCDconfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = txtBCRID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_ALL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtReturn.Rows.Count > 0)
                {
                    btnBCDchagne.Visibility = Visibility.Visible;
                    btnBCDconfirm.Visibility = Visibility.Hidden;
                    txtBCRID.IsReadOnly = true;
                    txtBCRID.IsEnabled = false;
                }
                else
                {
                    object[] parameters = new object[1];
                    parameters[0] = txtBCRID.Text.Trim();
                    MessageValidationWithCallBack("SFU7001", (result) =>
                    {
                        txtBCRID.Focus();
                    }, parameters); //CSTID[%1]에 해당하는 CST가 없습니다.

                    return;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //전체 취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Refresh();
                    }
                }
            );
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSelected_RCV_ISS_ID.Text.Length > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3340", txtSelected_RCV_ISS_ID.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //반품 취소 하시겠습니까?\n반품번호[{0}]
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ReturnCell_Cancel();
                        }
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!bLoaded) return;

            try
            {
                if (txtReturnID != null)
                {
                    txtReturnID.Focus();
                    txtReturnID.SelectAll();
                }

                if (rdoCell != null)
                {
                    if ((bool)rdoCell.IsChecked)
                    {
                        if (btnReturnFileUpload != null)
                        {
                            btnReturnFileUpload.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (btnReturnFileUpload != null)
                        {
                            btnReturnFileUpload.IsEnabled = false;
                        }
                    }
                }
                if(rdoNodata != null)
                {
                    if((bool)rdoNodata.IsChecked)
                    {
                        txtReturnResn.Text = "NODATA";
                        txtReturnResn.IsEnabled = false;
                    }
                    else
                    {
                        txtReturnResn.Text = string.Empty;
                        txtReturnResn.IsEnabled = true;
                    }
                }
                if (!(bool)this.rdoTray.IsChecked)
                { 
                    if (this.colCstId.Visibility == Visibility.Visible)
                    { 
                        this.colCstId.Visibility = Visibility.Collapsed; 
                        this.dgTagetList.ItemsSource = null;
                    }
                    this.btnTagetSelectCancel.Visibility = Visibility.Visible;
                    this.btnTagetCancel.Visibility = Visibility.Visible;
                }
                else
                { 
                    this.colCstId.Visibility = Visibility.Visible; 
                    this.dgTagetList.ItemsSource = null;
                    this.btnTagetSelectCancel.Visibility = Visibility.Collapsed;
                    this.btnTagetCancel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkInputCell_Return_YN_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3341"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //투입된 CELL이 포함되어 반품됩니다.\n체크 하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel)
                        {
                            chkInputCell_Return_YN.IsChecked = false;
                        }
                    }
                    );
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - DataGrid

        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "RCV_ISS_ID")
                    { 
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "RCV_ISS_ID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sTrayId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "TAG_ID"));

                            popUpOpenPalletInfo(sRcvIssId, sTrayId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sRCV_ISS_ID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                    string sISS_QTY = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "ISS_QTY"));
                    string sTO_SLOC_NAME = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "TO_SLOC_NAME"));
                    string sFROM_AREAID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "FROM_AREAID"));
                    string sRCV_ISS_STAT_CODE = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_STAT_CODE"));

                    if(sRCV_ISS_STAT_CODE == "SHIPPING")
                    {
                        btnReturnCancel.IsEnabled = true;
                    }
                    else
                    {
                        btnReturnCancel.IsEnabled = false;
                    }

                    txtSelected_RCV_ISS_ID.Text = sRCV_ISS_ID;
                    txtSelected_RCV_ISS_ID.Tag = sFROM_AREAID;
                    txtSelected_ISS_QTY.Text = sISS_QTY;
                    txtSelected_TO_SLOC_NAME.Text = sTO_SLOC_NAME;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            this.GridCheckBoxHeaderClick((CheckBox)sender, true);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.GridCheckBoxHeaderClick((CheckBox)sender, false);
        }
        #endregion
        #endregion

        #region Mehod
        private void setComboBox()
        {
            try
            {
                CommonCombo combo = new CommonCombo();

                C1ComboBox[] cboSearchSLocFrom_Child = { cboSearchSLocTo};
                //2017.10.17
                //string[] sFilter = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, SLOC_TYPE_CODE.SLOC02 };
                string[] sFilter = new string[3];

                if (LoginInfo.CFG_SHOP_ID != "A040")
                {
                    sFilter[0] = LoginInfo.CFG_SHOP_ID;
                    sFilter[1] = LoginInfo.CFG_AREA_ID;
                    sFilter[2] = SLOC_TYPE_CODE.SLOC02 + "," + SLOC_TYPE_CODE.SLOC03;
                }
                else
                {
                    sFilter[0] = LoginInfo.CFG_SHOP_ID;
                    sFilter[1] = LoginInfo.CFG_AREA_ID;
                    sFilter[2] = SLOC_TYPE_CODE.SLOC02;
                }
                combo.SetCombo(cboSearchSLocFrom, CommonCombo.ComboStatus.NONE, sFilter: sFilter,cbChild: cboSearchSLocFrom_Child, sCase: "SLOC");


                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;

                C1ComboBox cboSLOC_TYPE_CODE = new C1ComboBox();
                //2017.10.17
                if (LoginInfo.CFG_SHOP_ID != "A040")
                {
                    cboSLOC_TYPE_CODE.SelectedValue = "SLOC02,SLOC03";
                }
                else
                {
                    cboSLOC_TYPE_CODE.SelectedValue = SLOC_TYPE_CODE.SLOC03;
                }

                C1ComboBox cboSHIP_TYPE_CODE = new C1ComboBox();
                cboSHIP_TYPE_CODE.SelectedValue = Ship_Type.CELL;

                C1ComboBox cboShip_Proc = new C1ComboBox();
                cboShip_Proc.SelectedValue = Process.CELL_BOXING;

                //dr["SHOPID"] = sFilter[0];
                //dr["TO_SLOC_ID"] = sFilter[1];
                //dr["SLOC_TYPE_CODE"] = sFilter[2];
                //dr["SHIP_TYPE_CODE"] = sFilter[3];
                //dr["PROCID"] = sFilter[4];

                C1ComboBox[] cboSearchSLocTo_Parent = { cboSHOPID, cboSearchSLocFrom, cboSLOC_TYPE_CODE , cboSHIP_TYPE_CODE , cboShip_Proc };
                combo.SetCombo(cboSearchSLocTo, CommonCombo.ComboStatus.ALL, cbParent: cboSearchSLocTo_Parent, sCase: "SLOC_BY_TOSLOC_PROC");

                String[] sFilter3 = { "RETURN_RCV_ISS_STAT_CODE" };
                combo.SetCombo(cboReturStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void ReturnChkAndReturnCellCreate_ExcelOpen(DataTable dt)
        {
            try
            {
                if (dt != null)
                {
                    // 테이블생성해야함!!!
                    string INDATA_LENG = string.Empty;
                    int intFirstRow = 0;
                    //Exupload_Flag = "Y";
                    if (dt.Rows.Count > 0 && !(dt.Rows[0][0].ToString().Length > 0))
                    {
                        intFirstRow = 1;
                    }

                    DataTable INPUT_ID_LIST = new DataTable();
                    INPUT_ID_LIST.TableName = "INPUT_ID_LIST";
                    INPUT_ID_LIST.Columns.Add("RCV_ISS_ID", typeof(string));
                    INPUT_ID_LIST.Columns.Add("BOXID", typeof(string));
                    INPUT_ID_LIST.Columns.Add("LOTID", typeof(string));
                    INPUT_ID_LIST.Columns.Add("RTN_RSN_NOTE", typeof(string));

                    
                    for (int i = intFirstRow; i < dt.Rows.Count; i++)
                    {
                        string sLotId = "";
                        string sNote = "";

                        if (dt.Rows[i][0] != null)
                        {
                            if (dt.Rows[i][0].ToString().Length > 0)
                            {
                                sLotId = dt.Rows[i][0].ToString();
                                sEmpty_Lot +=  sLotId + ", ";
                            }
                            else
                            {
                                sEmpty_Lot += sLotId;
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        if (dt.Rows[i][1] != null)
                        {
                            if (dt.Rows[i][1].ToString().Length > 0)
                            {
                                sNote = dt.Rows[i][1].ToString();
                            }
                        }
                        //INPUT_ID_LIST.Rows.Add(new object[] { null, null, dt.Rows[i][0], dt.Rows[i][1] });
                        INPUT_ID_LIST.Rows.Add(new object[] { null, null, sLotId, sNote });
                    }

                    getReturnID_getLotInfo(INPUT_ID_LIST);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Refresh()
        {
            try
            {
                //반품목록 조회
                getReturnList();

                //그리드 clear
                Util.gridClear(dgTagetList);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");

                txtReturnID.Text = string.Empty;
                txtReturnFileName.Text = string.Empty;
                txtReturnNumber.Text = string.Empty;

                if ((bool)rdoNodata.IsChecked)
                {
                    txtReturnResn.Text = "NODATA";
                }
                else
                {
                    txtReturnResn.Text = string.Empty;
                }                

                txtSLocFrom.Text = string.Empty;
                txtSLocFrom.Tag = null;
                txtSLocTo.Text = string.Empty;
                txtSLocTo.Tag = null;
                txtBCRID.Text = string.Empty;                

                txtPRODID.Text = string.Empty;
                sFromArea = string.Empty;
                sToArea = string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getReturnTagetCell_By_Excel()
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";
                
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);

                        if (dtExcelData != null)
                        {
                            ReturnChkAndReturnCellCreate_ExcelOpen(dtExcelData);
                        }

                        //string sFileName = fd.FileName.ToString();

                        //if (sFileName != null && (sFileName.Substring(sFileName.Length - 4, 4).ToUpper() == "XLSX" || sFileName.Substring(sFileName.Length - 3, 3).ToUpper() == "XLS"))
                        //{
                        //    txtReturnFileName.Text = sFileName;
                        //    if (exl != null)
                        //    {
                        //        //이전 연결 해제
                        //        exl.Conn_Close();
                        //    }
                        //    //파일명 Set 으로 연결
                        //    exl.ExcelFileName = sFileName;
                        //    string[] str = exl.GetExcelSheets();
                        //    if (str.Length > 0)
                        //    {
                        //        //DataTable dt = exl.GetSheetData(str[0]);
                        //        ReturnChkAndReturnCellCreate_ExcelOpen(exl.GetSheetData(str[0]));
                        //    }
                        //}
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void ReturnCell()
        {
            try
            { 
                //BR_PRD_REG_RETURN_PRODUCT_PACK
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOP_ID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("FROM_AREAID", typeof(string));
                INDATA.Columns.Add("FROM_SLOC_ID", typeof(string));
                INDATA.Columns.Add("TO_AREAID", typeof(string));
                INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
                INDATA.Columns.Add("ISS_QTY", typeof(string));
                INDATA.Columns.Add("ISS_NOTE", typeof(string));
                INDATA.Columns.Add("SHIPTO_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("PLLT_BCD_ID", typeof(string));
                INDATA.Columns.Add("TAG_ID", typeof(string));
                INDATA.Columns.Add("BEFORE_RCV_ISS_ID", typeof(string));
                
                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                dr["RCV_ISS_ID"] = txtReturnNumber.Text; //반품번호 = 입출고id
                dr["FROM_AREAID"] = sFromArea;
                dr["FROM_SLOC_ID"] = Util.NVC(txtSLocFrom.Tag); //Util.NVC(cboSLocFrom.SelectedValue);
                dr["TO_AREAID"] = sToArea;
                dr["TO_SLOC_ID"] = Util.NVC(txtSLocTo.Tag);//Util.NVC(cboSLocTo.SelectedValue);
                dr["ISS_QTY"] = dgTagetList.Rows.Count;
                dr["ISS_NOTE"] = "";
                dr["SHIPTO_ID"] = "";
                dr["USERID"] = LoginInfo.USERID;
                dr["PLLT_BCD_ID"] = Util.NVC(txtBCRID.Text);
                dr["TAG_ID"] = sTrayID;
                dr["BEFORE_RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "RCV_ISS_ID"));

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataRow drINLOT = null;
                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));
                INLOT.Columns.Add("RTN_RSN_CODE", typeof(string));
                INLOT.Columns.Add("RTN_RSN_NOTE", typeof(string));
                
                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    drINLOT = INLOT.NewRow();
                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID"));
                    drINLOT["RTN_RSN_CODE"] = "";
                    drINLOT["RTN_RSN_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "RTN_RSN_NOTE"));
                    INLOT.Rows.Add(drINLOT);
                }
                dsInput.Tables.Add(INLOT);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_PRODUCT_PACK", "INDATA,INLOT", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            //Util.AlertByBiz("BR_PRD_REG_RETURN_PRODUCT_PACK", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        //CELL을반품하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1324"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                        Refresh();
                                    }
                                }
                            }
                            return;
                        }
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                    
                }, dsInput); 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReturnCell_Cancel()
        {
            try
            {
                //BR_PRD_REG_RETURN_PRODUCT_CANCEL_PACK
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CNCL_NOTE", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = txtSelected_RCV_ISS_ID.Text; //반품번호 = 입출고id
                dr["AREAID"] = txtSelected_RCV_ISS_ID.Tag;
                dr["CNCL_NOTE"] = "";
                dr["NOTE"] = "";
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_PRODUCT_CANCEL_PACK", "INDATA", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            //Util.AlertByBiz("BR_PRD_REG_RETURN_PRODUCT_CANCEL_PACK", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        //반품 취소하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3259"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                        
                                        Refresh_Selected_RCV();

                                        getReturnList();
                                    }
                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput); 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool Check_Input()
        {
            bool bReturn = true;
            try
            {
                //ID 입력확인.
                if (string.IsNullOrEmpty(txtReturnID.Text.Trim()))
                {                    
                    ms.AlertWarning("FM_ME_0039"); //ID를 입력해주세요
                    bReturn = false;
                    txtReturnID.Focus();
                    return bReturn;
                }
                //사유 입력확인.
                if (string.IsNullOrEmpty(txtReturnResn.Text.Trim())) 
                {                   
                    ms.AlertWarning("SFU1594"); //사유를 입력하세요.
                    bReturn = false;
                    txtReturnResn.Focus();
                    return bReturn;
                }                 
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool Check_ReturnConfirm()
        {
            bool bReturn = true;
            try
            {
                //목록 확인
                if (!(dgTagetList.Rows.Count > 0))
                {                   
                    ms.AlertWarning("SFU1553"); //반품CELL을입력하세요.
                    bReturn = false;
                    return bReturn;
                } 
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private void getReturnList()
        {
            try
            {
                //DA_PRD_SEL_TB_SFC_RCV_ISS_RETURN
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("FROM_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_STAT_CODE"] = Util.NVC(cboReturStatus.SelectedValue) == "" ? null : Util.NVC(cboReturStatus.SelectedValue);
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["FROM_SLOC_ID"] = Util.NVC(cboSearchSLocFrom.SelectedValue) == "" ? null : Util.NVC(cboSearchSLocFrom.SelectedValue);
                dr["TO_SLOC_ID"] = Util.NVC(cboSearchSLocTo.SelectedValue) == "" ? null : Util.NVC(cboSearchSLocTo.SelectedValue);
                RQSTDT.Rows.Add(dr);
                 
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT); 
                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            } 
            catch (Exception ex)
            { 
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 반품번호채번생성 , 입력id의 정보 조회
        /// </summary>
        public void getReturnID_getLotInfo(DataTable dtINPUT_ID_LIST)
        {
            try
            {
                string sInput_Flag = "";
                if ((bool)rdoRcvIss.IsChecked) sInput_Flag = "RCV";
                if ((bool)rdoPallet.IsChecked) sInput_Flag = "BOX";
                if ((bool)rdoCell.IsChecked) sInput_Flag = "LOT";
                if ((bool)rdoNodata.IsChecked) sInput_Flag = "NODATA";
                if ((bool)rdoTray.IsChecked) sInput_Flag = "TRAY";

                DataSet dsInput = new DataSet();
                
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RETURN_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("INPUT_ID_FLAG", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));                

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RETURN_ID"] = txtReturnNumber.Text; //반품번호
                dr["USERID"] = LoginInfo.USERID;
                dr["INPUT_ID_FLAG"] = sInput_Flag;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);
                
                dsInput.Tables.Add(dtINPUT_ID_LIST);
                 
                loadingIndicator.Visibility = Visibility.Visible;
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_ID_CREATE", "INDATA,INPUT_ID_LIST", "OUTDATA,LOT_INFO", (dsResult, dataException) =>
                  {
                      try
                      {
                          loadingIndicator.Visibility = Visibility.Collapsed;

                          /*
                           * 반품 CELL 수량 Validation  
                           */
                          if (dsResult != null || dataException == null) { 
                              
                                int iReturnValidationQty = ChkReturnCellQty();
                          
                               if (dgTagetList.GetRowCount() + dsResult.Tables["LOT_INFO"].Rows.Count > iReturnValidationQty) 
                               {
                                   Util.MessageValidation("SFU5015", iReturnValidationQty);  
                                   return;
                               }
                          }

                          DataTable dt = DataTableConverter.Convert(dgTagetList.ItemsSource);
                          
                          if (dataException != null)
                          {                              
                              Util.MessageException(dataException);
                              return;
                          }
                          else
                          {
                              for (int i = 0; i < dt.Rows.Count; i++)
                              {
                                if (dt.Rows.Count > 0)
                                  {
                                      if (Util.NVC(dt.Rows[i]["LOTID"]) == Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["LOTID"]))
                                      {
                                          Util.MessageInfo("SFU8166"); //중복되는 LOT이 존재합니다.
                                          return;
                                      }
                                      if (!(Util.NVC(dt.Rows[i]["PRODID"]) == Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODID"])))
                                      {
                                          Util.MessageInfo("SFU4178"); //동일한 제품이 아닙니다.
                                          return;
                                      }
                                  }
                              }
                              if (dsResult != null && dsResult.Tables.Count > 0)
                              {
                                  if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                  {
                                      if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                      {
                                          txtReturnNumber.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["RETURN_ID"]);
                                      }
                                  }
                                  if ((dsResult.Tables.IndexOf("LOT_INFO") > -1))
                                  {
                                      if (dsResult.Tables["LOT_INFO"].Rows.Count > 0)
                                      {
                                          DataTable dtResult = dsResult.Tables["LOT_INFO"];

                                          string sFromSloc = Util.NVC(dtResult.Rows[0]["FROM_SLOC_ID"]);
                                          string sFromSlocName = Util.NVC(dtResult.Rows[0]["FROM_SLOC_NAME"]);
                                          string sToSloc = Util.NVC(dtResult.Rows[0]["TO_SLOC_ID"]);
                                          string sToSlocName = Util.NVC(dtResult.Rows[0]["TO_SLOC_NAME"]);
                                          string sProdid = Util.NVC(dtResult.Rows[0]["PRODID"]);
                                          string sBcdid = Util.NVC(dtResult.Rows[0]["PLLT_BCD_ID"]);

                                          //입고된 정보에 반대로 set 하여 반품.
                                          sFromArea = Util.NVC(dtResult.Rows[0]["TO_AREAID"]);
                                          sToArea = Util.NVC(dtResult.Rows[0]["FROM_AREAID"]);

                                          // 2025.04.15 반품창고 SLOC_ID 변경 
                                          string rcvIssID = Util.NVC(dtResult.Rows[0]["RCV_ISS_ID"]);

                                          DataSet dsInputSloc = new DataSet();

                                          DataTable INDATA_SLOC = new DataTable();
                                          INDATA_SLOC.TableName = "INDATA";
                                          INDATA_SLOC.Columns.Add("AREAID", typeof(string));
                                          INDATA_SLOC.Columns.Add("SHOPID", typeof(string));
                                          INDATA_SLOC.Columns.Add("RCV_ISS_ID", typeof(string));
                                          INDATA_SLOC.Columns.Add("FROM_SLOC_ID", typeof(string));
                                          INDATA_SLOC.Columns.Add("TO_SLOC_ID", typeof(string));

                                          DataRow drSLOC = INDATA_SLOC.NewRow();
                                          drSLOC["AREAID"] = sFromArea;
                                          drSLOC["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                          drSLOC["RCV_ISS_ID"] = rcvIssID; //반품번호
                                          drSLOC["FROM_SLOC_ID"] = sFromSloc;
                                          drSLOC["TO_SLOC_ID"] = sToSloc;

                                          INDATA_SLOC.Rows.Add(drSLOC);

                                          dsInputSloc.Tables.Add(INDATA_SLOC);

                                          DataSet dsSLOC_MAPPPING = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_RETURN_SLOC_MAPPING", "INDATA", "OUTPUT", dsInputSloc);

                                          string sToSloc2 = Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["TO_SLOC_ID"]);
                                          string sToSlocName2 = String.IsNullOrEmpty(Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["TO_SLOC_NAME"])) ? sToSlocName : Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["TO_SLOC_NAME"]);


                                          string sFromSloc2 = Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["FROM_SLOC_ID"]);
                                          string sFromSlocName2 = String.IsNullOrEmpty(Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["FROM_SLOC_NAME"])) ? sFromSlocName : Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["FROM_SLOC_NAME"]);

                                          if (!(txtSLocFrom.Text.Length > 0))
                                          {
                                              txtSLocFrom.Text = sToSloc2 + " : " + sToSlocName2;
                                              txtSLocFrom.Tag = sToSloc2;
                                              txtSLocTo.Text = sFromSloc2 + " : " + sFromSlocName2;
                                              txtSLocTo.Tag = sFromSloc2;
                                              txtPRODID.Text = sProdid; 

                                              dgTagetList.ItemsSource = DataTableConverter.Convert(dtResult);
                                              Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtResult.Rows.Count));
                                          }
                                          else
                                          {
                                              DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);

                                              DataRow[] drTemp = dtBefore.Select("FROM_SLOC_ID = '" + sFromSloc + "'");
                                              if (!(drTemp.Length > 0))
                                              {                                                  
                                                  ms.AlertWarning("SFU1556"); //반품창고가다른정보가존재합니다.
                                                  return;
                                              }

                                              dtBefore.Merge(dtResult);
                                              dgTagetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                                              Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));
                                          }
                                      }
                                  }
                              }
                          }
                          return;
                      }
                      catch (Exception ex)
                      {
                          throw ex;
                      }

                  }, dsInput);
                if (!(bool)rdoNodata.IsChecked)
                {
                    txtReturnResn.Text = string.Empty;
                }
                txtReturnID.Text = string.Empty;            
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 반품테그 발행정보 조회
        /// </summary>
        /// <returns></returns>
        private DataTable getReturnTagInfo(string sRcv_iss_id)
        {
            DataTable dtReturn = null;
            try
            {
                //DA_PRD_SEL_TB_SFC_RCV_ISS_RETURN
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRcv_iss_id;
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURNTAG_INFO", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch(Exception ex)
            { 
                Util.MessageException(ex);
            }
            return dtReturn;
        }

        private void printReturnTag(DataTable dtReturnTagInfo)
        {
            try
            {
                if (!(dtReturnTagInfo.Rows.Count > 0))
                {
                    //Util.AlertInfo("발행정보가 없습니다.");
                    ms.AlertWarning("SFU3399"); //발행정보가 없습니다.
                    return;
                }
                ArrayList arrColumnNamesTemp = new ArrayList();

                string sNumberString = string.Empty;
                string sColumnName_LOT = string.Empty;
                string sColumnName_RETURN_NOTE = string.Empty;
                string sColumnName_CNT = string.Empty;

                for (int i = 0; i < dtReturnTagInfo.Rows.Count; i++)
                {
                    int iTemp = i + 1;
                    sNumberString = iTemp.ToString("00.##");
                    sColumnName_LOT = "LOT_" + sNumberString;
                    sColumnName_RETURN_NOTE = "RETURN_NOTE_" + sNumberString;
                    sColumnName_CNT = "CNT_" + sNumberString;
                    string[] sColumnNames = { sColumnName_LOT, sColumnName_RETURN_NOTE, sColumnName_CNT };
                    arrColumnNamesTemp.Add(sColumnNames);
                }

                DataTable dtReturnTag = new DataTable();
                dtReturnTag.TableName = "dtReturnTag";
                dtReturnTag.Columns.Add("PRODUCT_NAME", typeof(string));            // 제품명
                dtReturnTag.Columns.Add("RETURN_NUMBER", typeof(string));           // 반품번호
                dtReturnTag.Columns.Add("RETURN_NUMBER_BARCORD", typeof(string));   // 반품번호바코드
                dtReturnTag.Columns.Add("RETURN_DATE", typeof(string));             // 작업일자
                dtReturnTag.Columns.Add("TOTAL_COUNT", typeof(string));             // 제품수량
                dtReturnTag.Columns.Add("PRODUCTID", typeof(string));               // 제품ID
                dtReturnTag.Columns.Add("USER_NAME", typeof(string));               // 작업자
                dtReturnTag.Columns.Add("OUT_POSITION", typeof(string));            // 출고창고
                dtReturnTag.Columns.Add("IN_POSITION", typeof(string));             // 입고창고     

                if (arrColumnNamesTemp != null)
                {
                    for (int i = 0; i < arrColumnNamesTemp.Count; i++)
                    {
                        string[] sColumnNames = (string[])arrColumnNamesTemp[i];

                        dtReturnTag.Columns.Add(sColumnNames[0], typeof(string));
                        dtReturnTag.Columns.Add(sColumnNames[1], typeof(string));
                        dtReturnTag.Columns.Add(sColumnNames[2], typeof(string));
                    }
                }

                DataRow dr = dtReturnTag.NewRow();
                dr["PRODUCT_NAME"] = Util.NVC(dtReturnTagInfo.Rows[0]["PRODNAME"]);            // 제품명
                dr["RETURN_NUMBER"] = Util.NVC(dtReturnTagInfo.Rows[0]["RCV_ISS_ID"]);           // 반품번호
                dr["RETURN_NUMBER_BARCORD"] = Util.NVC(dtReturnTagInfo.Rows[0]["RCV_ISS_ID"]);   // 반품번호바코드
                dr["RETURN_DATE"] = Util.NVC(dtReturnTagInfo.Rows[0]["ISS_DTTM"]);             // 작업일자
                dr["TOTAL_COUNT"] = Util.NVC(dtReturnTagInfo.Rows[0]["TOTAL_QTY"]);             // 제품수량
                dr["PRODUCTID"] = Util.NVC(dtReturnTagInfo.Rows[0]["PRODID"]);               // 제품ID
                dr["USER_NAME"] = Util.NVC(dtReturnTagInfo.Rows[0]["INSUSER"]);               // 작업자
                dr["OUT_POSITION"] = Util.NVC(dtReturnTagInfo.Rows[0]["FROM_SLOC_ID"]);            // 출고창고
                //FROM_SLOC_NAME
                dr["IN_POSITION"] = Util.NVC(dtReturnTagInfo.Rows[0]["TO_SLOC_ID"]);             // 입고창고    
                //TO_SLOC_NAME
                if (arrColumnNamesTemp != null)
                {
                    for (int i = 0; i < arrColumnNamesTemp.Count; i++)
                    {
                        string[] sColumnNames = (string[])arrColumnNamesTemp[i];

                        dr[sColumnNames[0]] = Util.NVC(dtReturnTagInfo.Rows[i]["LOTID"]);
                        dr[sColumnNames[1]] = Util.NVC(dtReturnTagInfo.Rows[i]["RTN_RSN_NOTE"]);
                        dr[sColumnNames[2]] = Util.NVC(dtReturnTagInfo.Rows[i]["LOT_CNT"]);
                    }
                }
                dtReturnTag.Rows.Add(dr);

                LGC.GMES.MES.PACK001.Report_Multi rs = new LGC.GMES.MES.PACK001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "ReturnTag"; // "PalletHis_Tag";
                    Parameters[1] = dtReturnTag;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(printPopUp_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal())); 
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Report_Multi printPopUp = sender as LGC.GMES.MES.PACK001.Report_Multi;
                if (printPopUp.DialogResult == MessageBoxResult.OK)
                {
                    Refresh_Selected_RCV();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void Refresh_Selected_RCV()
        {
            txtSelected_RCV_ISS_ID.Text = string.Empty;
            txtSelected_RCV_ISS_ID.Tag = string.Empty;
            txtSelected_ISS_QTY.Text = string.Empty;
            txtSelected_TO_SLOC_NAME.Text = string.Empty;

            btnReturnCancel.IsEnabled = true;
        }

        private void showLoadingIndicator()
        {
            loadingIndicator.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }));
        }

        private void popUpOpenPalletInfo(string sRcvIssId, string sTrayId)
        {
            try
            {
                PACK001_022_RETURN_PALLETINFO popup = new PACK001_022_RETURN_PALLETINFO();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sRcvIssId;
                    Parameters[1] = sTrayId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Grid CheckBox Header Click
        private void GridCheckBoxHeaderClick(CheckBox checkBox, bool isChecked)
        {
            C1DataGrid c1DataGrid = null;
            IList<FrameworkElement> ilist = checkBox.GetAllParents();
            foreach (var item in ilist)
            {
                if (item.GetType().Name.ToUpper() == "C1DATAGRID")
                {
                    c1DataGrid = (C1DataGrid)item;
                    DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
                    foreach (DataRow dr in dt.Rows)
                    {
                        dr["CHK"] = isChecked;
                    }
                    c1DataGrid.ItemsSource = DataTableConverter.Convert(dt);
                    break;
                }
            }
        }

        private int ChkReturnCellQty()
        {
            int iReturnValidationQty = 0;

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_RETURN_CELL_QTY_VALIDATION";
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);


                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtReturn.Rows.Count > 0)
                {
                    iReturnValidationQty = dtReturn.Rows[0]["ATTRIBUTE1"] == DBNull.Value ? 1000 : Convert.ToInt32(dtReturn.Rows[0]["ATTRIBUTE1"].ToString());
                }

                else
                {
                    iReturnValidationQty = 1000;
                }

                return iReturnValidationQty;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return iReturnValidationQty;
            }
        }

        private DataTable GetCommonCode(string codeType, string cmcode)
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
                dr["CMCDTYPE"] = codeType;
                dr["CMCODE"] = cmcode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }
        #endregion
    }
}
