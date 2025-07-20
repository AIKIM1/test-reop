/*************************************************************************************
 Created Date : 2022.12.14
      Creator : 강동희
   Decription : 특별관리등록
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  DEVELOPER : Initial Created.
 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_021_SPECIAL_MANAGEMENT : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        Util _Util = new Util();
        private string sTrayID = string.Empty;
        private string sLotID = string.Empty;
        private string sSpcialType = string.Empty;
        private string sSpcialDesc = string.Empty;
        private string sShipmentYN = string.Empty;
        private string sSameEqp = string.Empty;

        public FCS002_021_SPECIAL_MANAGEMENT()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                sTrayID = Util.NVC(tmps[0]);
                sSameEqp = Util.NVC(tmps[1]);
            }

            //Combo Setting
            InitCombo();

            dtpFromDate.SelectedDateTime = DateTime.Now;
            dtpFromTime.DateTime = DateTime.Now;

            ////Special정보 조회
            //GetSpecialInfo();

            //GetSpecialMappingInfo();
        }
        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            string[] sFilter = { "FORM_SPCL_FLAG_MCC" };
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter);
            if (!string.IsNullOrEmpty(sSpcialType))
                cboSpecial.SelectedValue = sSpcialType;
        }
        #endregion

        #region [Method]
        private void GetSpecialInfo()
        {
            try
            {
                string[] arrTray = sTrayID.Split('|');

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                inDataTable.Rows.Add(dr);
                for (int i = 0; i < arrTray.Length; i++)
                {
                    inDataTable.Rows[0]["CSTID"] += arrTray[i] + ",";
                }
                //Tray 1 일경우 감안하여 마지막 , 제거
                inDataTable.Rows[0]["CSTID"] = inDataTable.Rows[0]["CSTID"].ToString().Substring(0, inDataTable.Rows[0]["CSTID"].ToString().Length - 1);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_SPECIAL_INFO_BY_TRAY_ID", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count == 1)
                {
                    txtSpecialDesc.Text = dtRslt.Rows[0]["SPCL_NOTE_CNTT"].ToString();  //SPECIAL_DESC
                    txtSelReqID.Text = dtRslt.Rows[0]["REQ_USERID"].ToString().ToLower();  //REGUSERID
                    txtSelReq.Text = dtRslt.Rows[0]["REGUSERNAME"].ToString();  //REGUSERNAME
                    sShipmentYN = dtRslt.Rows[0]["SHIP_ENABLE_FLAG"].ToString(); // SHIP_ENABLE_FLAG

                    if (sShipmentYN.Equals("N"))
                        chkShip.IsChecked = true;
                    else
                        chkShip.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSpecialMappingInfo()
        {
            try
            {
                string[] arrTray = sTrayID.Split('|');

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                inDataTable.Rows.Add(dr);
                for (int i = 0; i < arrTray.Length; i++)
                {
                    inDataTable.Rows[0]["CSTID"] += arrTray[i] + ",";
                }
                //Tray 1 일경우 감안하여 마지막 , 제거
                inDataTable.Rows[0]["CSTID"] = inDataTable.Rows[0]["CSTID"].ToString().Substring(0, inDataTable.Rows[0]["CSTID"].ToString().Length - 1);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SPECIAL_MAPPING_INFO_BY_TRAY_ID_MB", "INDATA", "OUTDATA", inDataTable);

                Util.GridSetData(dgSpclLotList, dsResult, FrameOperation, true);

                string[] sColumnName = new string[] { "LOTID" };
                _Util.SetDataGridMergeExtensionCol(dgSpclLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSpecialInfoByTray()
        {
            try
            {
                string[] arrTray = sTrayID.Split('|');

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                inDataTable.Rows.Add(dr);
                for (int i = 0; i < arrTray.Length; i++)
                {
                    inDataTable.Rows[0]["CSTID"] += arrTray[i] + ",";
                }
                //Tray 1 일경우 감안하여 마지막 , 제거
                inDataTable.Rows[0]["CSTID"] = inDataTable.Rows[0]["CSTID"].ToString().Substring(0, inDataTable.Rows[0]["CSTID"].ToString().Length - 1);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SPECIAL_INFO_BY_TRAY_MB", "INDATA", "OUTDATA", inDataTable);

                Util.GridSetData(dgSpclLotList, dsResult, FrameOperation, true);

                string[] sColumnName = new string[] { "LOTID" };
                _Util.SetDataGridMergeExtensionCol(dgSpclLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Event]

        #region [체크박스 관련]
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSpclLotList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgSpclLotList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSpclLotList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgSpclLotList.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int chkcnt = 0;

                btnSave.IsEnabled = false;

                DataTable RsltTable = DataTableConverter.Convert(dgSpclLotList.ItemsSource);
                RsltTable.Columns.Add("RESULT", typeof(string));

                if (cboSpecial.SelectedValue.Equals("N"))
                {
                    string sMsg = string.Empty;

                    for (int i = 0; i < dgSpclLotList.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgSpclLotList.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgSpclLotList.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            chkcnt++;

                            DataTable inDataTable = new DataTable();
                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("IFMODE", typeof(string));
                            inDataTable.Columns.Add("CSTID", typeof(string));                   //TRAY_ID
                            inDataTable.Columns.Add("LOTID", typeof(string));                   //TRAY_LOT_ID
                            inDataTable.Columns.Add("SPCL_GR_ID", typeof(string));              //SPECIAL_GROUP_ID
                            inDataTable.Columns.Add("SPCL_TYPE_CODE", typeof(string));          //SPECIAL_YN
                            inDataTable.Columns.Add("SPCL_NOTE_CNTT", typeof(string));          //SPECIAL_DESC
                            inDataTable.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(string)); //특별 예상 해제일 추가 2021.06.30 PSM
                            inDataTable.Columns.Add("INSUSER", typeof(string));                 //USERID
                            inDataTable.Columns.Add("UPDUSER", typeof(string));                 //USERID

                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSpclLotList.Rows[i].DataItem, "LOTID"));               //TRAY_LOT_ID
                            dr["SPCL_GR_ID"] = Util.NVC(DataTableConverter.GetValue(dgSpclLotList.Rows[i].DataItem, "SPCL_GR_ID"));     //SPECIAL_GROUP_ID
                            dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial);             //SPECIAL_YN
                            dr["SPCL_NOTE_CNTT"] = Util.GetCondition(txtSpecialDesc).Trim();  //SPECIAL_DESC
                            dr["INSUSER"] = LoginInfo.USERID;
                            dr["UPDUSER"] = LoginInfo.USERID;
                            inDataTable.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_SPECIAL_CODE_N_MB", "INDATA", "OUTDATA", inDataTable);

                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                // Util.MessageInfo("FM_ME_0530"); //특별등록 해제되었습니다.
                                sMsg = "FM_ME_0530";
                                RsltTable.Rows[i]["RESULT"] = "O";
                            }
                            else
                            {
                                // Util.MessageInfo("FM_ME_0424"); //특별등록 실패하였습니다.
                                sMsg = "FM_ME_0424";
                                RsltTable.Rows[i]["RESULT"] = "X";
                                Util.GridSetData(dgSpclLotList, RsltTable, FrameOperation, true); // 실패시 팝업닫히지 않고 결과표시
                                Util.MessageInfo(sMsg);
                                return;
                            }
                        }
                    }
                    if(chkcnt == 0 )
                    {
                        sMsg = "FM_ME_0165"; // 선택된 데이터가 없습니다.
                        Util.MessageInfo(sMsg);
                        btnSave.IsEnabled = true;
                        return;
                    }                    
                    Util.MessageInfo(sMsg);
                    DialogResult = MessageBoxResult.Yes;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(txtSpecialDesc.Text))
                    {
                        Util.GetCondition(txtSelReq, sMsg: "SFU1990"); //특별관리내역을 입력하세요.
                        btnSave.IsEnabled = true;
                        return;
                    }
                    else
                    {
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));                   //TRAY_ID
                        inDataTable.Columns.Add("SPCL_TYPE_CODE", typeof(string));          //SPECIAL_YN
                        inDataTable.Columns.Add("SPCL_NOTE_CNTT", typeof(string));          //SPECIAL_DESC
                        inDataTable.Columns.Add("SHIP_ENABLE_FLAG", typeof(string));        //SHIPMENT_YN
                        inDataTable.Columns.Add("SAME_EQP_TRAY", typeof(string));           //SAME_EQP_TRAY 
                        inDataTable.Columns.Add("INSUSER", typeof(string));                 //USERID
                        inDataTable.Columns.Add("UPDUSER", typeof(string));                 //USERID
                        inDataTable.Columns.Add("REQ_USERID", typeof(string));              //REQ_USERID 추가
                        inDataTable.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(string)); //특별 예상 해제일 추가 2021.06.30 PSM

                        string[] arrTray = sTrayID.Split('|');

                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial);     //SPECIAL_YN
                        dr["SPCL_NOTE_CNTT"] = Util.GetCondition(txtSpecialDesc).Trim();  //SPECIAL_DESC
                        dr["SHIP_ENABLE_FLAG"] = (bool)chkShip.IsChecked ? "N" : "Y";  //SHIPMENT_YN
                        if(chkSameRack.IsChecked == true)
                            dr["SAME_EQP_TRAY"] = "Y";
                        if (!string.IsNullOrEmpty(sSameEqp)) dr["SAME_EQP_TRAY"] = sSameEqp;
                        string sReqID = Util.GetCondition(txtSelReqID, sMsg: "FM_ME_0355"); //요청자를 입력해주세요.
                        if (string.IsNullOrEmpty(sReqID))
                        {
                            btnSave.IsEnabled = true;
                            return;
                        }
                        dr["REQ_USERID"] = sReqID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;

                        if ((bool)chkReleaseDate.IsChecked)  //특별 예상 해제일 추가 2021.06.30 PSM
                        {
                            DateTime _SysDttm = DateTime.Now;
                            if (dtpFromDate.SelectedDateTime.Date < _SysDttm.Date)
                            {
                                // 선택오류 : 시간외 항목을 선택했습니다. (시간 항목만 선택)
                                Util.MessageValidation("SFU3744");
                                btnSave.IsEnabled = true;
                                return;
                            }
                            else if (_SysDttm.Date <= dtpFromDate.SelectedDateTime.Date)
                            {
                                if (dtpFromTime.DateTime.Value.TimeOfDay < _SysDttm.TimeOfDay)
                                {
                                    // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 시간 확인)
                                    Util.MessageValidation("SFU3743");
                                    btnSave.IsEnabled = true;
                                    return;
                                }
                            }
                            dr["FORM_SPCL_REL_SCHD_DTTM"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                        }
                        inDataTable.Rows.Add(dr);

                        for (int i = 0; i < arrTray.Length; i++)
                        {
                            inDataTable.Rows[0]["CSTID"] += arrTray[i] + ","; //TRAY_ID
                        }

                        //Tray 1 일경우 감안하여 마지막 , 제거
                        inDataTable.Rows[0]["CSTID"] = inDataTable.Rows[0]["CSTID"].ToString().Substring(0, inDataTable.Rows[0]["CSTID"].ToString().Length - 1); //TRAY_ID

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_SPECIAL_MB", "INDATA", "OUTDATA", inDataTable);
                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageInfo("FM_ME_0423"); //특별등록 완료되었습니다.
                        }
                        else
                        {
                            Util.MessageInfo("FM_ME_0424"); //특별등록 실패하였습니다.
                        }

                        DialogResult = MessageBoxResult.Yes;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            Close();
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        #region [ PopUp Event ]

        #region < 담당자 찾기 >

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtSelReq.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            //grdMain.Children.Add(wndPerson); _grid     
            this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtSelReq.Text = wndPerson.USERNAME;
                txtSelReqID.Text = wndPerson.USERID;
            }
        }

        #endregion

        #endregion

        private void cboSpecial_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboSpecial.SelectedValue)))
            {
                if (Util.NVC(cboSpecial.SelectedValue).Equals("N"))
                {
                    GetSpecialInfo();
                    GetSpecialMappingInfo();
                    dgSpclLotList.Columns["CHK"].Visibility = Visibility.Visible;
                    chkShip.IsChecked = false;
                    chkShip.IsEnabled = false;
                    chkReleaseDate.IsChecked = false;
                    chkReleaseDate.IsEnabled = false;
                    chkSameRack.IsChecked = false;
                    chkSameRack.IsEnabled = false;
                    
                }
                else
                {
                    GetSpecialInfo();
                    GetSpecialInfoByTray();
                    dgSpclLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
                    chkShip.IsEnabled = true;
                    chkReleaseDate.IsEnabled = true;
                    chkSameRack.IsEnabled = true;
                }
            }
        }

        private void chkReleaseDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
        }

        private void chkReleaseDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
        }

        private void dgSpclLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////
                }
            }));
        }

        private void dgSpclLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK") && e.Column.Visibility.Equals(Visibility.Visible))
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

        private void dgSpclLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgSpclLotList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void txtSelReq_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtSelReq.Text))
                        return;

                    //초기화
                    txtSelReqID.Text = "";

                    //
                    GetUserWindow();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboSpecial_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboSpecial.SelectedValue)))
            {   
                if (Util.NVC(cboSpecial.SelectedValue).Equals("N"))
                {  
                    GetSpecialInfo();
                    GetSpecialMappingInfo();
                    dgSpclLotList.Columns["CHK"].Visibility = Visibility.Visible;
                    chkShip.IsChecked = false;
                    chkShip.IsEnabled = false;
                    chkReleaseDate.IsChecked = false;
                    chkReleaseDate.IsEnabled = false;
                    chkSameRack.IsChecked = false;
                    chkSameRack.IsEnabled = false;
                }
                else
                {
                    GetSpecialInfo();
                    GetSpecialInfoByTray();
                    dgSpclLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
                    chkShip.IsEnabled = true;
                    chkReleaseDate.IsEnabled = true;
                    chkSameRack.IsEnabled = true;
                }
            }
        }
    }
    #endregion

}
