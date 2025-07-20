/*************************************************************************************
 Created Date : 2018.04.18
      Creator : 오화백
   Decription : 활성화 비재공 폐기
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.18  오화백 : Initial Created.





 
**************************************************************************************/

using C1.WPF;

using LGC.GMES.MES.CMM001;
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
    public partial class COM001_231 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        string CTNR_ID = string.Empty;
        string _CHK_CTNR_ID_SEARCH = string.Empty;
        private int _tagPrintCount;
        public COM001_231()
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

            #region 비재공 폐기  

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
            #endregion

            #region 비재공 폐기 이력/취소
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
                    txtNonWorkScrap.Text = Util.NVC(dr["CTNR_ID"]);
                    GetLotList(false);
                }
            }


            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSheet);
            listAuth.Add(btnNonWorkInput);
            listAuth.Add(btnNonWorkUpdate);
            listAuth.Add(btnNonWorkDelete);
            listAuth.Add(btnNonWorkScrap);
            listAuth.Add(btnScrapCell);
            listAuth.Add(btnScrapCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }

        #region 비재공 폐기 

        #region 비재공 폐기 대상 조회 btnSearchScrap_Click()

        private void btnSearchScrap_Click(object sender, RoutedEventArgs e)
        {
            _CHK_CTNR_ID_SEARCH = string.Empty;
            GetLotList(true);
        }

        #endregion

        #region 비재공 폐기 대상 조회 비재공 ID 엔터 txtNonWorkScrap_KeyDown()
        //비재공 ID 엔터시
        private void txtNonWorkScrap_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtNonWorkScrap.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "CTNR_ID").ToString() == sLotid)
                            {

                                dgListInput.SelectedIndex = i;
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                txtNonWorkScrap.Focus();
                                txtNonWorkScrap.Text = string.Empty;
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

        #region 비재공 폐기 대상 비재공 등록 btnNonWorkInput_Click(), popupInplut_Closed()

        /// <summary>
        /// 비재공 입력 팝업 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNonWorkInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_231_INPUT_UPDATE popupInplut = new COM001_231_INPUT_UPDATE();
                popupInplut.INPUT_UPDATE_CHK = "INPUT";
                popupInplut.FrameOperation = this.FrameOperation;
                popupInplut.Header = ObjectDic.Instance.GetObjectName("비재공등록");
                popupInplut.Closed += new EventHandler(popupInplut_Closed);

                grdMain.Children.Add(popupInplut);
                popupInplut.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        /// <summary>
        /// 비재공 입력 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupInplut_Closed(object sender, EventArgs e)
        {
            COM001_231_INPUT_UPDATE popupInplut = sender as COM001_231_INPUT_UPDATE;
            // 재조회
            _CHK_CTNR_ID_SEARCH = string.Empty;
            GetLotList(true);
     
            this.grdMain.Children.Remove(popupInplut);
        }

        #endregion

        #region 비재공 폐기 대상 비재공 수정 btnNonWorkUpdate_Click(), popupUpdate_Closed()
        /// <summary>
        ///  비재공 수정 팝업열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNonWorkUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!NonWipUpdateValidation())
                {
                    return;
                }
                COM001_231_INPUT_UPDATE popupUpdate = new COM001_231_INPUT_UPDATE();
                popupUpdate.FrameOperation = this.FrameOperation;
                popupUpdate.INPUT_UPDATE_CHK = "UPDATE";
                popupUpdate.Header = ObjectDic.Instance.GetObjectName("비재공수정");
                popupUpdate.DgNonWip = dgListInput;
                popupUpdate.Closed += new EventHandler(popupUpdate_Closed);
                grdMain.Children.Add(popupUpdate);
                popupUpdate.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        /// <summary>
        ///  비재공 수정 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void popupUpdate_Closed(object sender, EventArgs e)
        {
            COM001_231_INPUT_UPDATE popupUpdate = sender as COM001_231_INPUT_UPDATE;

            // 재조회
            _CHK_CTNR_ID_SEARCH = string.Empty;
            GetLotList(true);
            this.grdMain.Children.Remove(popupUpdate);
        }
        #endregion

        #region 비재공 폐기 대상 비재공 삭제 btnNonWorkDelete_Click(), popupDelete_Closed()
        /// <summary>
        /// 비재공 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNonWorkDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!NonWipDeleteValidation())
                {
                    return;
                }
                COM001_231_DELETE_SCRAP popupDelete = new COM001_231_DELETE_SCRAP();
                popupDelete.FrameOperation = this.FrameOperation;
                popupDelete.DELETE_SCRAP_CHK = "DELETE";
                popupDelete.Header = ObjectDic.Instance.GetObjectName("비재공삭제");
                popupDelete.DgNonWip = dgListInput;
                popupDelete.Closed += new EventHandler(popupDelete_Closed);
                grdMain.Children.Add(popupDelete);
                popupDelete.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        /// <summary>
        ///  비재공 삭제 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void popupDelete_Closed(object sender, EventArgs e)
        {
            COM001_231_DELETE_SCRAP popupDelete = new COM001_231_DELETE_SCRAP();


           
                // 재조회
                _CHK_CTNR_ID_SEARCH = string.Empty;
                GetLotList(true);
          
            this.grdMain.Children.Remove(popupDelete);
        }



        #endregion
        
        #region 비재공 폐기 대상 비재공 폐기 btnNonWorkScrap_Click(), popupScrap_Closed()
        /// <summary>
        /// 비재공 폐기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnNonWorkScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!NonWipScrapValidation())
                {
                    return;
                }
                COM001_231_DELETE_SCRAP popupScrap = new COM001_231_DELETE_SCRAP();
                popupScrap.FrameOperation = this.FrameOperation;
                popupScrap.DELETE_SCRAP_CHK = "SCRAP";
                popupScrap.Header = ObjectDic.Instance.GetObjectName("비재공폐기");
                popupScrap.DgNonWip = dgListInput;
                popupScrap.Closed += new EventHandler(popupScrap_Closed);
                grdMain.Children.Add(popupScrap);
                popupScrap.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 비재공 폐기 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupScrap_Closed(object sender, EventArgs e)
        {
            COM001_231_DELETE_SCRAP popupScrap = new COM001_231_DELETE_SCRAP();


           
                // 재조회
                _CHK_CTNR_ID_SEARCH = string.Empty;
                GetLotList(true);
          
            this.grdMain.Children.Remove(popupScrap);
        }

        #endregion

        #region 비재공 폐기 불량 Cell 등록  btnScrapCell_Click(), popupoutput_Closed()
        //불량Cell 등록
        private void btnScrapCell_Click(object sender, RoutedEventArgs e)
        {
            if (dgListInput.Rows.Count > 0)
            {
                DataTable dtscrap = DataTableConverter.Convert(dgListInput.ItemsSource).Select("CHK = '1'").CopyToDataTable();
               
                COM001_219_DEFECT_CELL_INPUT popupoutput = new COM001_219_DEFECT_CELL_INPUT();
                popupoutput.FrameOperation = this.FrameOperation;
                popupoutput.NON_WIP_CHK = "Y";
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
                // 재조회
                _CHK_CTNR_ID_SEARCH = string.Empty;
                GetLotList(true);

            }
            this.grdMain.Children.Remove(popupoutput);
        }
        #endregion

        #region 비재공 폐기 폐기 시트 발행  btnSheet_Click(), popupCartPrint_Closed()
        /// <summary>
        /// 비재공 시트 발행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnSheet_Click(object sender, RoutedEventArgs e)
        {
            if (dgListInput.ItemsSource == null)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgListInput.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            _tagPrintCount = drSelect.Length;

            foreach (DataRow drPrint in drSelect)
            {
                POLYMER_TagPrint(drPrint);
            }
        }

        /// <summary>
        /// 비재공 시트 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
        #endregion

        #region 비재공 폐기 스프레드 선택 dgCtnrChk_Checked()

        private void dgCtnrChk_Checked(object sender, RoutedEventArgs e)
        {
            dgListInput.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

            }
        }
        #endregion



        #endregion

        #region 비재공 폐기 이력/취소

        #region 비재공 폐기 이력/취소 이력 조회 btnSearchHistory_Click()

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {

            GetList_History(true);

        }
        #endregion

        #region 비재공 폐기 이력/취소 폐기 취소 btnScrapCancel_Click()
        //비재공폐기취소
        private void btnScrapCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCel_Validation())
                {
                    return;
                }
                //비재공 폐기 취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4640"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        #region 비재공 폐기 이력/취소 대차ID로 이력 조회 txtNonWorkHistory_KeyDown()

        private void txtNonWorkHistory_KeyDown(object sender, KeyEventArgs e)
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

        #region 비재공 폐기 이력/취소 폐기 취소된 ROW 색 변경 dgListHistory_LoadedCellPresenter(), dgListHistory_UnloadedCellPresenter()
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
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID")).Equals("SCRAP_NON_WIP"))
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

        #region 비재공 폐기 이력/취소 이력 정보 선택 dgCtnrHistChk_Checked()
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

                        if (DataTableConverter.GetValue(dgListHistory.Rows[idx].DataItem, "ACTID").ToString() == "CANCEL_SCRAP_NON_WIP")
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

        #region 비재공 폐기 이력/취소 폐기취소 사용자 txtUserNameCr_History_KeyDown(), btnUserCr_History_Click(), wndUser_History_Closed()
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

        #region [비재공 폐기]

        #region [비재공 폐기 폐기대상 조회 GetLotList()]
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
                dtRqst.Columns.Add("NON_WIP_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
             
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtNonWorkScrap).Equals("")) //비재공ID가 없는경우
                {
                    dr["PROCID"] = Util.GetCondition(cboProcessScrap, "SFU1459"); // 공정을 선택하세요
                    if (dr["PROCID"].Equals("")) return;
                    if (cboEquipmentSegmentScrap.SelectedIndex != 0)
                    {
                        dr["EQSGID"] = cboEquipmentSegmentScrap.SelectedValue.ToString();
                    }
                    dr["PJT_NAME"] = txtPrjtNameScrap.Text;
                    dr["PRODID"] = txtProdidScrap.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["NON_WIP_ID"] = Util.GetCondition(txtNonWorkScrap);
                    dr["LANGID"] = LoginInfo.LANGID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_NON_WIP_SCRAP_LIST", "INDATA", "OUTDATA", dtRqst);



                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, false);
                     return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtNonWorkScrap.Focus();
                            txtNonWorkScrap.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {

                    DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                    dtSource.Merge(dtRslt);
                    Util.gridClear(dgListInput);
                    Util.GridSetData(dgListInput, dtSource, FrameOperation, false);
                    txtNonWorkScrap.Text = string.Empty;
                    txtNonWorkScrap.Focus();

                }
                else
                {
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, false);
                 
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion
      
        #region [Valldation]
       
        /// <summary>
        ///  비재공 수정
        /// </summary>
        /// <returns></returns>
        private bool NonWipUpdateValidation()
        {
            if (dgListInput.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgListInput, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }


            return true;
        }
   
        /// <summary>
        ///  비재공 삭제 Validation
        /// </summary>
        /// <returns></returns>
        private bool NonWipDeleteValidation()
        {
            if (dgListInput.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다
                return false;
            }
            DataRow[] drInfo = Util.gridGetChecked(ref dgListInput, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            return true;
        }


        /// <summary>
        ///  비재공 폐기 Validation
        /// </summary>
        /// <returns></returns>
        private bool NonWipScrapValidation()
        {
            if (dgListInput.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다
                return false;
            }
            DataRow[] drInfo = Util.gridGetChecked(ref dgListInput, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            return true;
        }

        #endregion

        #region [비재공 폐기 시트 팝업 열기 POLYMER_TagPrint()]

        /// <summary>
        /// 비재공 시트 팝업 열기
        /// </summary>
        /// <param name="dr"></param>
        private void POLYMER_TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;
            popupCartPrint.NonWipYN = "Y";


            object[] parameters = new object[5];
            parameters[0] = LoginInfo.CFG_PROC_ID;
            parameters[1] = string.Empty;
            parameters[2] = dr["NON_WIP_ID"];
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }

        #endregion

        #endregion

        #region [비재공 폐기 이력/취소]

        #region [비재공 폐기 이력/취소 폐기된 비재공 조회 GetList_History()]
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
                dtRqst.Columns.Add("NON_WIP_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
          
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtNonWorkHistory).Equals("")) //PalletID 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU4238"); // 동정보를 선택하세요
                    if (dr["AREAID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, "SFU1459"); // 공정을 선택하세요
                    if (dr["PROCID"].Equals("")) return;
                    if (cboEquipmentSegmentHistory.SelectedIndex != 0)
                    {
                        dr["EQSGID"] = cboEquipmentSegmentHistory.SelectedValue.ToString();
                    }
                    dr["PJT_NAME"] = txtPjtHistory.Text;
                    dr["PRODID"] = txtProdID.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    if (cboScrapStat.SelectedIndex == 0)
                    {
                        dr["ACTID"] = null;
                    }
                    else
                    {
                        dr["ACTID"] = cboScrapStat.SelectedValue.ToString();
                    }

                }
                else
                {
                    dr["NON_WIP_ID"] = Util.GetCondition(txtNonWorkHistory);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = null;
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_NON_WIP_SCRAP_HISTORY", "INDATA", "OUTDATA", dtRqst);
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
                            txtNonWorkHistory.Focus();
                            txtNonWorkHistory.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                   if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListHistory.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListHistory);
                        Util.GridSetData(dgListHistory, dtSource, FrameOperation, true);
                        txtNonWorkHistory.Text = string.Empty;
                        txtNonWorkHistory.Focus();
                        dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                    }
                    else
                    {
                        Util.gridClear(dgListHistory);
                        Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                        txtNonWorkHistory.Text = string.Empty;
                        txtNonWorkHistory.Focus();
                        dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                    }

                }
                else
                {
                    Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                    Util.gridClear(dgListHistory);
                    dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                }

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                dgListHistory.Columns["ACTQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [비재공 폐기 이력/취소  상태 콤보 조회 Set_Combo_Scrap_Stat()]
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
                row["CBO_CODE"] = "SCRAP_NON_WIP";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("폐기");
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "CANCEL_SCRAP_NON_WIP";
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

        #region [비재공 폐기 이력/취소 폐기취소 Cancel_CtnrScrap()]
        private void Cancel_CtnrScrap()
        {
            DataSet inData = new DataSet();

           //마스터 정보
           DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("NON_WIP_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));


            DataRow row = null;
            
            for (int i = 0; i < dgListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inDataTable.NewRow();
                    row["NON_WIP_ID"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "NON_WIP_ID"));
                    row["USERID"] = LoginInfo.USERID;
                    row["ACT_USERID"] = txtUserNameCr_History.Tag.ToString();
                    row["NOTE"] = txtReson_History.Text;
                    inDataTable.Rows.Add(row);
                }
            }
           try
            {
                //비재공 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_SCRAP_NON_WIP", "INDATA", null, (Result, ex) =>
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
                Util.AlertByBiz("BR_ACT_REG_CANCEL_SCRAP_NON_WIP", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region [비재공 폐기 이력/취소  폐기 취소 Validation CanCel_Validation()]
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
            if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[rowIndex].DataItem, "ACTID")) == "CANCEL_SCRAP_NON_WIP")
            {
                Util.MessageValidation("SFU4642"); //폐기취소된 대차입니다.
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

                if (Check == "H")
                {
                    Parameters[0] = txtUserNameCr_History.Text;
                    wndPerson.Closed += new EventHandler(wndUser_History_Closed);
                }
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

















        #endregion

        #endregion

     
    }
}
