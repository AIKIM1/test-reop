/*************************************************************************************
 Created Date : 2018.03.01
      Creator : 오화백
   Decription : 파우치 활성화 불량 대차 폐기
--------------------------------------------------------------------------------------
 [Change History]
  2018.03.01  오화백 : Initial Created.
  2021.08.25  공민경 : 설비구분 콤보박스 추가(ESNJ FOX BLACKLIST 전송 관련)




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_219 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        string CTNR_ID = string.Empty;
        string _CHK_CTNR_ID_SEARCH = string.Empty;
        
        public COM001_219()
        {
            InitializeComponent();
            InitCombo();
            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
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

            #region 불량대차 폐기  

            //동
            C1ComboBox[] cboAreaChild = { cboProcessScrap };
            _combo.SetCombo(cboAreaScrap, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboAreaScrap.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboProcessParent = { cboAreaScrap };
            C1ComboBox[] cboScrapLine = { cboEquipmentSegmentScrap };
             _combo.SetCombo(cboProcessScrap, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboScrapLine, cbParent: cboProcessParent);
           
            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaScrap, cboProcessScrap };
            _combo.SetCombo(cboEquipmentSegmentScrap, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParent);
            cboEquipmentSegmentScrap.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //폐기사유코드
            string[] sFilter = { "SCRAP_LOT" };
            _combo.SetCombo(cboActivitiReason, CommonCombo.ComboStatus.SELECT,  sFilter: sFilter);

            //의뢰자 셋팅
            txtUserNameCr.Text = LoginInfo.USERNAME;
            txtUserNameCr.Tag = LoginInfo.USERID;

            //설비구분
            string[] sFilter1 = { "", "FOX_BLACKLIST_EQPT_TYPE_CODE" };
            _combo.SetCombo(cboEqptType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODES");

            if (!FOX_Blacklist_Area_Validation())
            {
                cboEqptType.Visibility = Visibility.Hidden;
                txtEqptType.Visibility = Visibility.Hidden;
            }
            else
            {
                cboEqptType.Visibility = Visibility.Visible;
                txtEqptType.Visibility = Visibility.Visible;
            }

            #endregion

            #region 폐기 이력/취소
            //동
            C1ComboBox[] cboAreaChildHistory = { cboProcessHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");
            cboAreaHistory.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboAreaHistory };
            C1ComboBox[] cboScrapLineHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboScrapLineHistory, cbParent: cboProcessParentHistory);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory, cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParentHistory);
            cboEquipmentSegmentHistory.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //폐기상태
            Set_Combo_Scrap_Stat(cboScrapStat);

            //의뢰자 셋팅
            txtUserNameCr_History.Text = LoginInfo.USERNAME;
            txtUserNameCr_History.Tag = LoginInfo.USERID;

            #endregion
        }
             
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                DataTable dtInfo = ary.GetValue(0) as DataTable;

                foreach (DataRow dr in dtInfo.Rows)
                {
                    txtCtnrIDScrap.Text = Util.NVC(dr["CTNR_ID"]);
                    GetLotList(false);
                }
            }
            

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnScrap);
            listAuth.Add(btnScrapCell);
            listAuth.Add(btnScrapCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }

        #region 불량대차 폐기 

        #region 불량대차 폐기 대상 조회 btnSearchScrap_Click()

        private void btnSearchScrap_Click(object sender, RoutedEventArgs e)
        {
            _CHK_CTNR_ID_SEARCH = string.Empty;
            GetLotList(true);
        }

        #endregion

        #region 불량대차 폐기 대상 조회 대차 ID 클릭 txtCtnrIDScrap_KeyDown()
        //대차 ID 엔터시
        private void txtCtnrIDScrap_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtCtnrIDScrap.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "CTNR_ID").ToString() == sLotid)
                            {

                                dgListInput.SelectedIndex = i;
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                txtCtnrIDScrap.Focus();
                                txtCtnrIDScrap.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    _CHK_CTNR_ID_SEARCH = "Y";
                    GetLotList(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


        }

        #endregion

        #region 불량대차 폐기처리 btnScrap_Click()
        
        //불량대차 폐기
        private void btnScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //폐기하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4191"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ShowLoadingIndicator();

                                if (FOX_Blacklist_Area_Validation())
                                {
                                    BlacklistTrans();
                                }

                                ScrapCtnrID();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 불량대차 폐기 사용자 조회  txtUserName_KeyDown(), btnUser_Click(), wndUser_Closed()
        //사용자 조회
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow("P");
            }
        }
        //사용자 조회 버튼
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow("P");
        }
        // 사용자 팝업닫기
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

            }
        }

        #endregion

        #region 불량대차 폐기 초기화 btnReSet_Click()

        //초기화
        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);
            Util.gridClear(dgSelectInput);
            cboActivitiReason.SelectedIndex = 0;
            cboEqptType.SelectedIndex = 0;
            txtUserNameCr.Text = LoginInfo.USERNAME;
            txtUserNameCr.Tag = LoginInfo.USERID;
            txtReson.Text = string.Empty;
        }
        #endregion

        #region 불량대차 폐기할 대차 선택 dgCtnrChk_Checked()
        //폐기 대차 선택
        private void dgCtnrChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        DataTable TempTable = DataTableConverter.Convert(dgListInput.ItemsSource).Select("CTNR_ID = '" + Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[idx].DataItem, "CTNR_ID")) + "'").CopyToDataTable();
                        
                        //row 색 바꾸기
                        CTNR_ID = TempTable.Rows[0]["CTNR_ID"].ToString();
                        dgListInput.SelectedIndex = idx;
                        dgSelectInput.ItemsSource = DataTableConverter.Convert(TempTable);
                        dgSelectInput.SelectedIndex = 0;

                        cboActivitiReason.SelectedIndex = 0;
                        cboEqptType.SelectedIndex = 0;
                        //의뢰자 셋팅
                        txtUserNameCr.Text = LoginInfo.USERNAME;
                        txtUserNameCr.Tag = LoginInfo.USERID;
                        txtReson.Text = string.Empty;
                    }
                }
                else
                {
                    cboActivitiReason.SelectedIndex = 0;
                    cboEqptType.SelectedIndex = 0;
                    //의뢰자 셋팅
                    txtUserNameCr.Text = LoginInfo.USERNAME;
                    txtUserNameCr.Tag = LoginInfo.USERID;
                    txtReson.Text = string.Empty;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        #endregion

        #region 불량대차 폐기 불량 Cell 등록  btnScrapCell_Click(), popupoutput_Closed()
        //불량Cell 등록
        private void btnScrapCell_Click(object sender, RoutedEventArgs e)
        {
            if (dgSelectInput.Rows.Count > 0)
            {

                DataTable dtscrap = DataTableConverter.Convert(dgSelectInput.ItemsSource);

                COM001_219_DEFECT_CELL_INPUT popupoutput = new COM001_219_DEFECT_CELL_INPUT();
                popupoutput.FrameOperation = this.FrameOperation;
                object[] parameters = new object[1];
                parameters[0] = dtscrap;
                C1WindowExtension.SetParameters(popupoutput, parameters);
                popupoutput.Closed += new EventHandler(popupoutput_Closed);
                grdMain.Children.Add(popupoutput);
                popupoutput.BringToFront();
            }
        }
        //불량Cell 닫기
        private void popupoutput_Closed(object sender, EventArgs e)
        {
            COM001_219_DEFECT_CELL_INPUT popupoutput = sender as COM001_219_DEFECT_CELL_INPUT;


            if (popupoutput.DialogResult == MessageBoxResult.Cancel)
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                if (_CHK_CTNR_ID_SEARCH.Equals("")) //대차ID 가 없는 경우
                {
                    dr["PROCID"] = Util.GetCondition(cboProcessScrap, "SFU1459"); // 공정을 선택하세요
                    if (dr["PROCID"].Equals("")) return;

                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentScrap, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    //if (dr["EQSGID"].Equals("")) return;
                    if (cboEquipmentSegmentScrap.SelectedIndex != 0)
                    {
                        dr["EQSGID"] = cboEquipmentSegmentScrap.SelectedValue.ToString();
                    }
                    dr["PJT_NAME"] = txtPrjtNameScrap.Text;
                    dr["PRODID"] = txtProdidScrap.Text;
                    dr["LOTID_RT"] = txtLotRTDScrap.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["CTNR_ID"] = CTNR_ID;
                    dr["LANGID"] = LoginInfo.LANGID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CTNR_SCRAP_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgListInput, dtRslt, FrameOperation, false);
                for (int i = 0; i < dgListInput.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "CTNR_ID")) == CTNR_ID)
                    {
                        DataTableConverter.SetValue(dgListInput.Rows[i].DataItem, "CHK", 1);
                        dgListInput.SelectedIndex = i;

                        DataTableConverter.SetValue(dgSelectInput.Rows[0].DataItem, "SCAN_QTY", Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "SCAN_QTY")).Replace(",", "")));
                        DataTableConverter.SetValue(dgSelectInput.Rows[0].DataItem, "NON_SCAN_QTY", Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "NON_SCAN_QTY")).Replace(",", "")));
                    }

                }

            }
            this.grdMain.Children.Remove(popupoutput);
        }
        #endregion


        #endregion

        #region 폐기 이력/취소

        #region 폐기 이력/취소 이력 조회 btnSearchHistory_Click()

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {

            GetList_History(true);

        }
        #endregion

        #region 폐기 이력/취소 폐기 취소 btnScrapCancel_Click()
        //대차폐기취소
        private void btnScrapCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCel_Validation())
                {
                    return;
                }
                //불량대차 폐기 취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4605"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Cancel_CtnrScrap();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 폐기 이력/취소 대차ID로 이력 조회 txtCtnrIDHistory_KeyDown()
        private void txtCtnrIDHistory_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                if (e.Key == Key.Enter)
                {

                    GetList_History(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


        }
        #endregion

        #region 폐기 이력/취소 폐기 취소된 ROW 색 변경 dgListHistory_LoadedCellPresenter(), dgListHistory_UnloadedCellPresenter()
        //스프레드 색 변경
        private void dgListHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID")).Equals("SCRAP_CTNR"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));

                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        //스프레드 색 변경
        private void dgListHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        #endregion

        #region 폐기 이력/취소 이력 정보 선택 dgCtnrHistChk_Checked()
        //이력 정보 선택시
        private void dgCtnrHistChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        if (DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "ACTID").ToString() == "CANCEL_SCRAP_CTNR")
                        {
                            rb.IsChecked = false;
                        }
                        else
                        {
                            rb.IsChecked = true;
                            //row 색 바꾸기
                            dgListHistory.SelectedIndex = idx;
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

            }
        }
        #endregion

        #region 폐기 이력/취소 폐기취소 사용자 txtUserNameCr_History_KeyDown(), btnUserCr_History_Click(), wndUser_History_Closed()
        //폐기취소 사용자
        private void txtUserNameCr_History_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow("H");
            }
        }
        //폐기취소 사용자
        private void btnUserCr_History_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow("H");
        }
        //폐기취소 사용자 닫기
        private void wndUser_History_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr_History.Text = wndPerson.USERNAME;
                txtUserNameCr_History.Tag = wndPerson.USERID;

            }
        }
        #endregion


        #endregion


        #endregion

        #region Mehod
 
        #region [불량대차 폐기]

        #region [불량대차 폐기 폐기대상 조회 GetLotList()]
        //불량 대차 ID 조회
        public void GetLotList(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtCtnrIDScrap).Equals("")) //대차ID 가 없는 경우
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaScrap, "SFU4238"); // 동정보를 선택하세요
                    if (dr["AREAID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessScrap, "SFU1459"); // 공정을 선택하세요
                    if (dr["PROCID"].Equals("")) return;
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentScrap, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    //if (dr["EQSGID"].Equals("")) return;
                    if (cboEquipmentSegmentScrap.SelectedIndex != 0)
                    {
                        dr["EQSGID"] = cboEquipmentSegmentScrap.SelectedValue.ToString();
                    }
                    dr["PJT_NAME"] = txtPrjtNameScrap.Text;
                    dr["PRODID"] = txtProdidScrap.Text;
                    dr["LOTID_RT"] = txtLotRTDScrap.Text;
                    dr["LANGID"] = LoginInfo.LANGID;                    
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["CTNR_ID"] = Util.GetCondition(txtCtnrIDScrap);
                    dr["LANGID"] = LoginInfo.LANGID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CTNR_SCRAP_LIST", "INDATA", "OUTDATA", dtRqst);



                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectInput);
                    cboActivitiReason.SelectedIndex = 0;
                    cboEqptType.SelectedIndex = 0;
                    txtUserNameCr.Text = LoginInfo.USERNAME;
                    txtUserNameCr.Tag = LoginInfo.USERID;
                    txtReson.Text = string.Empty;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCtnrIDScrap.Focus();
                            txtCtnrIDScrap.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {

                    DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                    dtSource.Merge(dtRslt);
                    Util.gridClear(dgListInput);
                    //dgListSplit.ItemsSource = DataTableConverter.Convert(dtSource);
                    Util.GridSetData(dgListInput, dtSource, FrameOperation, false);
                    txtCtnrIDScrap.Text = string.Empty;
                    txtCtnrIDScrap.Focus();
                }
                else
                {
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectInput);
                    cboActivitiReason.SelectedIndex = 0;
                    cboEqptType.SelectedIndex = 0;
                    txtUserNameCr.Text = LoginInfo.USERNAME;
                    txtUserNameCr.Tag = LoginInfo.USERID;
                    txtReson.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [불량대차 폐기 폐기대상 폐기 ScrapCtnrID()]
        //대차폐기
        private void ScrapCtnrID()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["USERID"] = LoginInfo.USERID;
            row["IFMODE"] = "OFF";
            row["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "EQSGID"));
            row["ACT_USERID"] = txtUserNameCr.Tag.ToString();
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INRESN");
            inInresn.Columns.Add("CTNR_ID", typeof(string));
            inInresn.Columns.Add("RESNCODE", typeof(string));
            inInresn.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inInresn.Columns.Add("PROCID_CAUSE", typeof(string));
            inInresn.Columns.Add("RESNNOTE", typeof(string));
            inInresn.Columns.Add("EQPT_TYPE", typeof(string));
            for (int i = 0; i < dgSelectInput.Rows.Count; i++)
            {
                row = inInresn.NewRow();
                row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "CTNR_ID"));
                if (cboActivitiReason.SelectedValue.ToString() != "SELECT")
                {
                    row["RESNCODE"] = cboActivitiReason.SelectedValue.ToString();
                }
                row["RESNCODE_CAUSE"] = null;
                row["PROCID_CAUSE"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PROCID"));
                row["RESNNOTE"] = txtReson.Text;
                if (cboEqptType.SelectedValue.ToString() != "SELECT")
                {
                    row["EQPT_TYPE"] = cboEqptType.SelectedValue.ToString();
                }

                inInresn.Rows.Add(row);
            }
            try
            {
                //대차폐기
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SCRAP_MCP", "INDATA,INRESN", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        HiddenLoadingIndicator();
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Util.gridClear(dgListInput);
                            Util.gridClear(dgSelectInput);
                            cboActivitiReason.SelectedIndex = 0;
                            cboEqptType.SelectedIndex = 0;
                            txtUserNameCr.Text = string.Empty;
                            txtUserNameCr.Tag = string.Empty;
                            txtReson.Text = string.Empty;
                            GetLotList(true);
                            HiddenLoadingIndicator();
                        }
                    });

                    return;
                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SCRAP_MCP", ex.Message, ex.ToString());
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [불량대차 폐기 폐기 Valldation Validation() ]
        private bool Validation()
        {

            if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || txtUserNameCr.Tag == null)
            {
                Util.MessageValidation("SFU4151"); //의뢰자를 선택하세요
                return false;
            }


            if (cboActivitiReason.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4193"); //폐기사유코드를 선택하세요
                return false;
            }

            if (FOX_Blacklist_Area_Validation())
            {
                if (cboEqptType.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU8190"); //설비구분을 선택하세요
                    return false;
                }
            }

            if (dgSelectInput.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4194"); //폐기할 데이터가 없습니다.
                return false;
            }
            return true;
        }
        #endregion


        #endregion

        #region [폐기 이력/취소]

        #region [폐기 이력/취소 폐기된 대차 조회 GetList_History()]
        public void GetList_History(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CELL_ID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtCtnrIDHistory).Equals("")) //PalletID 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU4238"); // 동정보를 선택하세요
                    if (dr["AREAID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, "SFU1459"); // 공정을 선택하세요
                    if (dr["PROCID"].Equals("")) return;
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    //if (dr["EQSGID"].Equals("")) return;
                    if (cboEquipmentSegmentHistory.SelectedIndex != 0) 
                    {
                        dr["EQSGID"] = cboEquipmentSegmentHistory.SelectedValue.ToString();
                    }
                    dr["PJT_NAME"] = txtPjtHistory.Text;
                    dr["PRODID"] = txtProdID.Text;
                    dr["LOTID_RT"] = txtLotHistory.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    if (cboScrapStat.SelectedIndex == 0)
                    {
                        dr["ACTID"] = null;
                    }
                    else
                    {
                        dr["ACTID"] = cboScrapStat.SelectedValue.ToString();
                    }

                    string sCellID = Util.ConvertEmptyToNull(txtCellID.Text.Trim());

                    if (!string.IsNullOrEmpty(sCellID))
                        dr["CELL_ID"] = sCellID;

                }
                else
                {
                    dr["CTNR_ID"] = Util.GetCondition(txtCtnrIDHistory);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = null;
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CTNR_SCRAP_HISTORY", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgListHistory);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCtnrIDHistory.Focus();
                            txtCtnrIDHistory.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["ACTQTY"] = dtRslt.Rows[i]["ACTQTY"].ToString().Replace("-", "");
                    }

                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListHistory.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListHistory);
                        Util.GridSetData(dgListHistory, dtSource, FrameOperation, true);
                        txtCtnrIDHistory.Text = string.Empty;
                        txtCtnrIDHistory.Focus();
                        dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                    }
                    else
                    {
                        Util.gridClear(dgListHistory);
                        Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                        txtCtnrIDHistory.Text = string.Empty;
                        txtCtnrIDHistory.Focus();
                        dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                    }

                }
                else
                {
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["ACTQTY"] = dtRslt.Rows[i]["ACTQTY"].ToString().Replace("-", "");
                    }
                    Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                    Util.gridClear(dgListHistory);
                    dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                }

                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        
        #region [폐기 이력/취소 폐기 상태 콤보 조회 Set_Combo_Scrap_Stat()]
        //폐기상태 콤보
        private void Set_Combo_Scrap_Stat(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "ALL";
                row["CBO_NAME"] = "ALL";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "SCRAP_CTNR";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("폐기");
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "CANCEL_SCRAP_CTNR";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("폐기취소");
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

        #region [폐기 이력/취소 폐기취소 Cancel_CtnrScrap()]
        private void Cancel_CtnrScrap()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["USERID"] = LoginInfo.USERID;
            row["IFMODE"] = "OFF";
            row["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
            row["ACT_USERID"] = txtUserNameCr_History.Tag.ToString();
            inDataTable.Rows.Add(row);

            //INRESN
            DataTable inInresn = inData.Tables.Add("INRESN");
            inInresn.Columns.Add("CTNR_ID", typeof(string));
            inInresn.Columns.Add("PROCID_CAUSE", typeof(string));
            inInresn.Columns.Add("RESNCODE", typeof(string));
            inInresn.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inInresn.Columns.Add("RESNNOTE", typeof(string));
            for (int i = 0; i < dgListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inInresn.NewRow();
                    row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CTNR_ID"));
                    row["PROCID_CAUSE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "PROCID"));
                    row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "RESNCODE"));
                    row["RESNCODE_CAUSE"] = null;
                    row["RESNNOTE"] = txtReson_History.Text;
                    inInresn.Rows.Add(row);
                }
            }
            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SCRAP_MCP", "INDATA,INRESN", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Util.gridClear(dgListHistory);
                            GetList_History(true);
                            //의뢰자 셋팅
                            txtUserNameCr_History.Text = LoginInfo.USERNAME;
                            txtUserNameCr_History.Tag = LoginInfo.USERID;
                            txtReson_History.Text = string.Empty;
                        }
                    });
                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_CANCEL_SCRAP_MCP", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region [폐기 이력/취소  폐기 취소 Validation CanCel_Validation()]
        private bool CanCel_Validation()
        {

            if (dgListHistory.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                return false;
            }

            int CheckCount = 0;

            for (int i = 0; i < dgListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            int rowIndex = _Util.GetDataGridFirstRowIndexByCheck(dgListHistory, "CHK");
            if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[rowIndex].DataItem, "ACTID")) == "CANCEL_SCRAP_CTNR")
            {
                Util.MessageValidation("SFU4628"); //폐기취소된 대차입니다.
                return false;

            }


            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUserNameCr_History.Text) || txtUserNameCr_History.Tag == null)
            {
                Util.MessageValidation("SFU4151"); //의뢰자를 선택하세요
                return false;
            }

            return true;
        }
        #endregion

        #region [FOX Blacklist 데이터 전송 BlacklistTrans()]
        //FOX Blacklist 데이터 전송
        private void BlacklistTrans()
        {
            //Blacklist 전송 Count 선언(몇 건씩 끊어서 전송할 것인지)
            int Count = 0;

            DataSet CntData = new DataSet();

            DataTable dtCntRqst = CntData.Tables.Add("RQSTDT");
            dtCntRqst.Columns.Add("CMCDTYPE", typeof(string));
            dtCntRqst.Columns.Add("CMCODE", typeof(string));

            DataRow Cntrow = null;
            Cntrow = dtCntRqst.NewRow();
            Cntrow["CMCDTYPE"] = "FOX_BLACKLIST_AREA";
            Cntrow["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dtCntRqst.Rows.Add(Cntrow);

            DataTable dtCntRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", dtCntRqst);

            if (dtCntRslt.Rows[0]["ATTRIBUTE1"].ToString() != null && dtCntRslt.Rows[0]["ATTRIBUTE1"].ToString() != "")
            {
                Count = Convert.ToInt32(dtCntRslt.Rows[0]["ATTRIBUTE1"].ToString());
            }
            else
            {
                Count = 100;
            }

            //폐기 SUBLOT 전체 개수 확인
            DataSet SubLotData = new DataSet();

            DataTable dtRqst = SubLotData.Tables.Add("RQSTDT");
            dtRqst.Columns.Add("CTNR_ID", typeof(string));
            dtRqst.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));

            DataRow SubLotrow = null;
            SubLotrow = dtRqst.NewRow();
            SubLotrow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "CTNR_ID"));
            SubLotrow["WIP_PRCS_TYPE_CODE"] = "SCRAP";
            dtRqst.Rows.Add(SubLotrow);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_BY_CTNR", "INDATA", "OUTDATA", dtRqst);

            int dtRsltCnt = dtRslt.Rows.Count;
            int CNT = 0;

            if (dtRslt.Rows.Count != 0 && dtRsltCnt % 100 != 0)
            {
                CNT = dtRsltCnt / Count + 1;
            }
            else
            {
                CNT = dtRsltCnt / Count;
            }

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("EQPT_TYPE", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "EQSGID"));
            row["EQPT_TYPE"] = cboEqptType.SelectedValue.ToString();
            inDataTable.Rows.Add(row);

            //INSUBLOT
            DataTable inSublot = inData.Tables.Add("INSUBLOT");
            inSublot.Columns.Add("CELL_ID", typeof(string));

            for (int i = 0; i < CNT; i++)
            {
                int START_CNT = 0;
                int END_CNT = 0;

                if (i == 0)
                {
                    START_CNT = 1;
                }
                else
                {
                    START_CNT = i * Count + 1;
                }

                if (i == 0)
                {
                    END_CNT = Count;
                }
                else
                {
                    END_CNT = (i + 1) * Count;
                }


                DataSet ScrapSubLotData = new DataSet();

                DataTable dtRScrapSubLotRqst = ScrapSubLotData.Tables.Add("RQSTDT");
                dtRScrapSubLotRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRScrapSubLotRqst.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));
                dtRScrapSubLotRqst.Columns.Add("START_CNT", typeof(string));
                dtRScrapSubLotRqst.Columns.Add("END_CNT", typeof(string));

                DataRow ScrapSubLotrow = null;
                ScrapSubLotrow = dtRScrapSubLotRqst.NewRow();
                ScrapSubLotrow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "CTNR_ID"));
                ScrapSubLotrow["WIP_PRCS_TYPE_CODE"] = "SCRAP";
                ScrapSubLotrow["START_CNT"] = START_CNT;
                ScrapSubLotrow["END_CNT"] = END_CNT;
                dtRScrapSubLotRqst.Rows.Add(ScrapSubLotrow);

                DataTable dtRScrapSubLotRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_BY_CTNR_CNT", "INDATA", "OUTDATA", dtRScrapSubLotRqst);

                inSublot.Clear();

                for (int j = 0; j < dtRScrapSubLotRslt.Rows.Count; j++)
                {
                    row = inSublot.NewRow();

                    row["CELL_ID"] = Util.NVC(dtRScrapSubLotRslt.Rows[j]["SUBLOTID"].ToString(), "CELL_ID");

                    inSublot.Rows.Add(row);
                }

                if (inSublot.Rows.Count != 0)
                {
                    try
                    {
                        //BLACKLIST 송신              
                        new ClientProxy().ExecuteService_Multi("BR_PRD_INS_FOX_BLACKLIST", "INDATA,INSUBLOT", null, (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                HiddenLoadingIndicator();
                                return;
                            }
                            inSublot.Clear();

                            return;
                        }, inData);

                    }
                    catch (Exception ex)
                    {
                        Util.AlertByBiz("BR_PRD_INS_FOX_BLACKLIST", ex.Message, ex.ToString());
                        HiddenLoadingIndicator();
                    }
                }
            }
        }

        #endregion

        #region [FOX Blacklist 전송 동 Validation FOX_Blacklist_Area_Validation()]
        private bool FOX_Blacklist_Area_Validation()
        {
            DataSet inData = new DataSet();

            DataTable dtRqst = inData.Tables.Add("RQSTDT");
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("CMCDTYPE", typeof(string));
            dtRqst.Columns.Add("CMCODE", typeof(string));

            DataRow row = null;
            row = dtRqst.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["CMCDTYPE"] = "FOX_BLACKLIST_AREA";
            row["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(row);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", dtRqst);

            if (dtRslt.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        #endregion

        #endregion

        #region[공통]

        private void GetUserWindow(string Check)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
               
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                if (Check == "P")
                {
                    Parameters[0] = txtUserNameCr.Text;
                    wndPerson.Closed += new EventHandler(wndUser_Closed);
                }
                else if(Check == "H")
                {
                    Parameters[0] = txtUserNameCr_History.Text;
                    wndPerson.Closed += new EventHandler(wndUser_History_Closed);
                }
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }



        #endregion

        #endregion



    }
}
