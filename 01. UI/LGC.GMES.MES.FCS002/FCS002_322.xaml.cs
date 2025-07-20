/*************************************************************************************
 Created Date : 2017.08.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.07  오화백: Initial Created.
  2017.11.10  오화백: 시장유형 추가, 작업구분정보 공통코드 => TB_MMD_AREA_COM_CODE에서 가져오도록 수정, Tag관련 남경 /오창 구분해서 발행되도록함
  2018.12.19  정문교: 원각형 특성/Grading 공정진척 Inbox 등록 팝업 호출
  2023.03.27  이홍주: 소형활성화MES 복사


 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using System.Collections;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_322 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
               
        private string _StackingYN = string.Empty;
        string _AREAID = string.Empty;
        string _PROCID = string.Empty;
        string _PRODID = string.Empty;
        string _EQSGID = string.Empty;
        string _EQPTID = string.Empty;
        string _EQPTNAME = string.Empty;
        string _LOTID = string.Empty;
        string _WIPSEQ = string.Empty;
        string _WIP_NOTE = string.Empty;
        string _LOTID_RT = string.Empty;
        string _SOC_VALUE = string.Empty;
        string _LOTDTTM_CR = string.Empty;
        string _LINE_GROUP_CODE = string.Empty;
        string _MKT_TYPE_CODE = string.Empty;
        string _LOTTYPE = string.Empty;
        string _SHIFT = string.Empty;
        string _WRK_USERID = string.Empty;
        string _WRK_USER_NAME = string.Empty;

        string _PROCNAME = string.Empty;
        string _MODLID = string.Empty;
        string _PRJT_NAME = string.Empty;
        string _MKT_TYPE_NAME = string.Empty;
        string _CARDATE = string.Empty;
        string _SHFT_NAME = string.Empty;
        string _EQPTSHORTNAME = string.Empty;
        string _CTNR_TYPE_CODE = string.Empty;
        string _MIX_LOT_FLAG = string.Empty;
        string _INSP_PROC_CHECK = string.Empty;
        string _WIPSTAT = string.Empty;
        string _LOT_OUTPUT_FLAG = string.Empty;
        string _LOT_INPUT_FLAG = string.Empty;
        string _OUTPUT_LOT_GNRT_FLAG = string.Empty;


        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private readonly Util _util = new Util();
        Decimal _INPUTQTY = 0;
        DataTable _dtLengthExceeed = new DataTable();
        private System.DateTime dtCaldate;
        DataTable _dtResult;
        DataTable _dtPallet;
        private BizDataSet _Biz = new BizDataSet();
        Util _Util = new Util();
        int _maxColumn = 0;
        private int _tagPrintCount;
        private CheckBoxHeaderType _inBoxHeaderType;
      
        //불량탭
        private int _defectGradeCount;
        private bool _IsDefectSave;
        private DataTable _defectList;
        private bool _defectTabButtonClick;
        public FCS002_322()
        {
            InitializeComponent();
         }
     
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public string ProcessCode { get; set; }
        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboProcess, cboJob, cboWorkType };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);
           //공정
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sCase: "AREA_PROCESS_SORT", cbParent: cboLineParent);
           // 작업구분
            //C1ComboBox[] cboWorkTypeParent = { cboEquipmentSegment };
            _combo.SetCombo(cboJob, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent, sCase: "FORM_WRK_TYPE_CODE");
            _combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, cbParent: cboLineParent, sCase: "FORM_WRK_TYPE_CODE");
            _combo.SetCombo(cboWorkType_PY, CommonCombo.ComboStatus.SELECT, cbParent: cboLineParent, sCase: "FORM_WRK_TYPE_CODE");
            //LOTTYPE
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT);
            _combo.SetCombo(cboLotType_PY, CommonCombo.ComboStatus.SELECT,sCase: "LOTTYPE");
            //작업조
            String[] sFilterShift = { string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, sFilter: sFilterShift, sCase: "SHIFT");

         
        }
      

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            InitCombo();
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnSave_Result);
            listAuth.Add(btnDefectSave);
             Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            if (LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_AREA_ID.Equals("S5"))
            {
                Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "08:00:00");
                Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "20:00:00");
            }
            else
            {
                Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
                Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "18:00:00");
            }
            dtpCaldate.SelectedDataTimeChanged += dtpCaldate_SelectedDataTimeChanged;
            _inBoxHeaderType = CheckBoxHeaderType.Zero;
            _defectTabButtonClick = false;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList(true);
        }
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            if (chk.IsChecked == true)
            {
                cboTimeFrom.Visibility = Visibility.Visible;
                cboTimeTo.Visibility = Visibility.Visible;
            }
            else
            {
                cboTimeFrom.Visibility = Visibility.Collapsed;
                cboTimeTo.Visibility = Visibility.Collapsed;
            }
        }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chk.IsChecked == true)
            {
                cboTimeFrom.Visibility = Visibility.Visible;
                cboTimeTo.Visibility = Visibility.Visible;
            }
            else
            {
                cboTimeFrom.Visibility = Visibility.Collapsed;
                cboTimeTo.Visibility = Visibility.Collapsed;
            }
        }

        public static void Set_Pack_cboTimeList(C1ComboBox cb, string sDisplay, string sValue, string sDefaultSelected)
        {
            DataTable dtTimeList = new DataTable();
            dtTimeList.Columns.Add(sDisplay);
            dtTimeList.Columns.Add(sValue);

            DataRow dr = dtTimeList.NewRow();
            for (int i = 0; i < 24; i++)
            {
                dr = dtTimeList.NewRow();
                dr[sDisplay] = (i).ToString("D2");
                if (i == 23)
                {
                    dr[sValue] = "22:59:59";
                }
                else
                {
                    dr[sValue] = (i).ToString("D2") + ":00:00";
                }

                dtTimeList.Rows.Add(dr);
            }

            cb.ItemsSource = DataTableConverter.Convert(dtTimeList);

            if (sDefaultSelected.Length > 0)
            {
                cb.SelectedValue = sDefaultSelected;
            }
        }



        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                Util.gridClear(dgLotList);
                ClearValue();
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetEquipment(cboEquipment, "ALL", cboEquipmentSegment.SelectedItemsToString, cboProcess.SelectedValue.ToString(), cboArea.SelectedValue.ToString());
                    SetEquipment(cboEquipment_PY, "SELECT", cboEquipmentSegment.SelectedItemsToString, cboProcess.SelectedValue.ToString(), cboArea.SelectedValue.ToString());
                    SetEquipment(cboEquipment_MC, "SELECT", cboEquipmentSegment.SelectedItemsToString, cboProcess.SelectedValue.ToString(), cboArea.SelectedValue.ToString());
                }));
            }
            catch
            {
            }

        }
        private void SetEquipment(C1ComboBox cbo, string GUBUN, string Eqsgid, string process, string areaid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if(Eqsgid == string.Empty)
                {
                    dr["EQSGID"] = null;
                }
                else
                {
                    dr["EQSGID"] = Eqsgid;
                }
              
                dr["PROCID"] = process;
              
                if(areaid == string.Empty)
                {
                    dr["AREAID"] = null;
                }
                else
                {
                    dr["AREAID"] = areaid;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_MULT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, GUBUN, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
          
                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cbo.SelectedIndex < 0)
                        cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private DataTable AddStatus(DataTable dt, string cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        #region [공정] - 조회 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {

                    Util.gridClear(dgLotList);
                    ClearValue();

                    String[] sFilter = new String[3];
                    sFilter[0] = cboArea.SelectedValue.ToString();
                    sFilter[1] = cboProcess.SelectedValue.ToString();

                    SetcboProcess(cboEquipmentSegment, sFilter);


                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
       }
        private void SetcboProcess(MultiSelectionBox cbo, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter[0];
                dr["PROCID"] = sFilter[1];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cbo.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                        cbo.Check(-1);
                    }
                    else
                    {
                        cbo.isAllUsed = true;
                        cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cbo.Check(i);

                        }

                    }
                   
                }
                else
                {
                    cbo.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [작업일] - 조회조건 ####### Visibility="Collapsed" #######
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "7");

                //    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region [LOT] - 조회 조건
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList(false);
            }


        }
        private void txtCtnrID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList(false);
            }
        }
        #endregion

        #region [작업대상 목록 에서 선택]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {

            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));

                //선택값 셋팅
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                TabSetting(_LINE_GROUP_CODE, _PROCID);
                SetGridCombo();
                //완성 INBOX
                SetGridCombo_INBOX();
                SelectPalletList();
                SelectInputPalletList();
                GetDefectInfo();
                SelectInpuMaterialList();
                SelectPalletSummary();
                GetGQualityInfo();
                //완성 INBOX
                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                {
                    SelectInbox_FINALEXTERNAL();
                }
                else
                {
                    SelectInbox();
                }
                //검사공정 완성INBOX
                GetOutInboxList();
                //투입 INBOX
                SelectInputInboxList();
                //불량
                SelectDefectList();

                if (_PROCID == Process.CircularCharacteristicGrader)
                {
                    dgProductionPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                    dgInputPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                    dgPalletSummary.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgProductionPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                    dgInputPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                    dgPalletSummary.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                }
              
                    btnSave_Result.Visibility = Visibility.Visible;
                

            }
        }
      
        #endregion

        #region [작업일자]
        private void dtpCaldate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
                else
                    dtPik.Focus();
            }));
        }
        #endregion

        #region [작업조선택]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (txtSelectLot.Text.Equals(""))
            {
                //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            CMM_SHIFT wndPopup = new CMM_SHIFT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = _AREAID;
                Parameters[2] = _EQSGID;
                Parameters[3] = _PROCID;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!txtShift.Tag.Equals(window.SHIFTCODE))
                {
                    //SetChgFont(txtShift);
                }
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }


        private void btnShift_PY_Click(object sender, RoutedEventArgs e)
        {
            if (txtSelectLot.Text.Equals(""))
            {
                //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            CMM_SHIFT wndPopup = new CMM_SHIFT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = _AREAID;
                Parameters[2] = _EQSGID;
                Parameters[3] = _PROCID;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_PY_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShift_PY_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!txtShift_PY.Tag.Equals(window.SHIFTCODE))
                {
                    //SetChgFont(txtShift);
                }
                txtShift_PY.Tag = window.SHIFTCODE;
                txtShift_PY.Text = window.SHIFTNAME;
            }
        }

        #endregion

        #region [작업자]
        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            if (txtSelectLot.Text.Equals(""))
            {
                //Util.AlertInfo("SFU1381");  //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.AlertInfo("SFU1646");  //선택된 작업조가 없습니다.
                Util.MessageValidation("SFU1646");
                return;
            }

            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_EQSGID);
                Parameters[3] = Util.NVC(_PROCID);
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EQPTID);  //EQPTID 추가 
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();

            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
        }
        #endregion

        #region [sublot 수량 변경]
        private void dgSubLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                // 수정 가능여부에 따른 CHK 칼럼 처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MODIFY_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    }
                }
            }));

        }

        private void dgSubLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("CHK") || e.Column.Name.Equals("WIPQTY"))
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "MODIFY_YN").Equals("N"))
                {
                    e.Cancel = true;
                }
            }

        }

      
        #endregion

     
        #endregion

        #region Mehod

        #region [BizCall]

        #region [### 작업대상 가져오기 ###]
        public void GetLotList(bool bButton)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("MODELID", typeof(string));
                dtRqst.Columns.Add("SUPLIER", typeof(string));
                dtRqst.Columns.Add("SHIFT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("CHK", typeof(string));
                dtRqst.Columns.Add("UNCHK", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("ZERO_YN", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;


                //if(!Util.GetCondition(txtLotId).Equals("")) //lot id 가 있는경우 다른 조건 모두 무시
                //{
                //    dr["LOTID"] = Util.GetCondition(txtLotId);
                //}
                //else if (!Util.GetCondition(txtCtnrID).Equals("")) //대차 ID가 존재할 경우
                //{
                //    dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                //    if (dr["PROCID"].Equals("")) return;
                //    dr["CTNR_ID"] = Util.GetCondition(txtCtnrID);
                //}
                //else  //LOTID, 대차ID가 존재하지 않을 경우
                //{
                    //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                    dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                    if (dr["PROCID"].Equals("")) return;


                    if (cboEquipmentSegment.SelectedItemsToString == string.Empty)
                    {
                        Util.MessageValidation("SFU1223"); // 라인을선택하세요
                        return;
                    }
                    else
                    {
                        dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;  //인계라인
                    }

                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                    dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboJob, bAllNull: true);
                    dr["MODELID"] = txtModel.Text;
                    dr["SUPLIER"] = txtCompany.Text;
                    dr["SHIFT"] = Util.GetCondition(cboShift, bAllNull: true);

                    dr["LOTID"] = txtLotId.Text;
                    if (txtCtnrID.Text != string.Empty)
                    {
                     dr["CTNR_ID"] = txtCtnrID.Text;
                    }
                   
                   if (chkProc.IsChecked == false)
                        dr["RUNYN"] = "Y";
                    dr["LOTID_RT"] = txtLotId_RT.Text;

                    if (chk.IsChecked == false)
                    {
                        dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                        dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                        dr["UNCHK"] = "Y";
                    }
                    else if (chk.IsChecked == true)
                    {
                        dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                        dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
                        dr["CHK"] = "Y";
                    }
                    if (chkZero.IsChecked == true)
                        dr["ZERO_YN"] = "Y";
                //}
            
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTLOT_RESULT", "INDATA", "OUTDATA", dtRqst);

                if(bButton)
                {
                    if (dtRslt.Rows.Count > 0)
                    {
                        if (dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCS") // 초소형
                        {
                            LotInfo.Visibility = Visibility.Visible;
                            LotInfo_PY.Visibility = Visibility.Collapsed;
                            dgLotList.Columns["WND_GR_CODE"].Visibility = Visibility.Visible;
                            dgLotList.Columns["SOC_VALUE"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_LOSS_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_ETC"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["HOLD_FLAG_NAME"].Visibility = Visibility.Collapsed;
                            LOTID_RT.Visibility = Visibility.Visible; 
                            LOTTYPE.Visibility = Visibility.Visible;
                            SOC.Visibility = Visibility.Visible;
                            WORK_TYPE.Visibility = Visibility.Visible;
                            txtLot_PR.Visibility = Visibility.Visible;
                            cboLotType.Visibility = Visibility.Visible;
                            txtSoc.Visibility = Visibility.Visible;
                            cboWorkType.Visibility = Visibility.Visible;

                            ClearValue();
                            TabSetting(dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString(), cboProcess.SelectedValue == null ? string.Empty : cboProcess.SelectedValue.ToString());
                            Util.gridClear(dgLotList);
                            Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

                            dgLotList.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_ETC"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["INPUT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            
                        }
                        else if (dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCC" || dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCR" || dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCM")  //원형, 각형
                        {
                            LotInfo.Visibility = Visibility.Visible;
                            LotInfo_PY.Visibility = Visibility.Collapsed;
                            dgLotList.Columns["WND_GR_CODE"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["SOC_VALUE"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_LOSS_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_ETC"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["HOLD_FLAG_NAME"].Visibility = Visibility.Collapsed;

                            LOTID_RT.Visibility = Visibility.Visible;
                            LOTTYPE.Visibility = Visibility.Visible;
                            SOC.Visibility = Visibility.Visible;
                            WORK_TYPE.Visibility = Visibility.Visible;
                            txtLot_PR.Visibility = Visibility.Visible;
                            cboLotType.Visibility = Visibility.Visible;
                            txtSoc.Visibility = Visibility.Visible;
                            cboWorkType.Visibility = Visibility.Visible;

                             ClearValue();
                            TabSetting(dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString(), cboProcess.SelectedValue == null ? string.Empty : cboProcess.SelectedValue.ToString());
                            Util.gridClear(dgLotList);
                            Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                            dgLotList.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_ETC"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["INPUT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);

                        }
                       else // 폴리머
                        {

                            LotInfo.Visibility = Visibility.Collapsed;
                            LotInfo_PY.Visibility = Visibility.Visible;
                            dgLotList.Columns["WND_GR_CODE"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["SOC_VALUE"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_LOSS_ETC"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["HOLD_FLAG_NAME"].Visibility = Visibility.Visible;
                            dgLotList.Columns["PKG_EQSGNAME"].Visibility = Visibility.Visible;

                            ClearValue();
                            TabSetting(dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString(), cboProcess.SelectedValue == null ? string.Empty : cboProcess.SelectedValue.ToString());
                            Util.gridClear(dgLotList);

                            for(int i=0; i< dtRslt.Rows.Count; i++)
                            {

                                if(dtRslt.Rows[i]["LOT_INPUT_FLAG"].ToString() == string.Empty)
                                {
                                    dtRslt.Rows[i]["INPUT_QTY"] = Convert.ToDecimal(dtRslt.Rows[i]["PRODUCT_QTY"]) + Convert.ToDecimal(dtRslt.Rows[i]["CNFM_LOSS_ETC"]);

                                }
                            }
                            Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                            dgLotList.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_ETC"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["INPUT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            //작업유형
                            CommonCombo _combo = new CommonCombo();
                            string[] sFilter = {cboArea.SelectedValue.ToString()};
                            _combo.SetCombo(cboWorkType_PY, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);


                        }
                    }
                    else
                    {
                        Util.gridClear(dgLotList);
                    }
                }
              else
                {
                    if (dtRslt.Rows.Count > 0)
                    {
                        if (dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCS") // 초소형
                        {
                            LotInfo.Visibility = Visibility.Visible;
                            LotInfo_PY.Visibility = Visibility.Collapsed;

                            dgLotList.Columns["WND_GR_CODE"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_LOSS_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_ETC"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["HOLD_FLAG_NAME"].Visibility = Visibility.Collapsed;
                            LOTID_RT.Visibility = Visibility.Visible;
                            LOTTYPE.Visibility = Visibility.Visible;
                            SOC.Visibility = Visibility.Visible;
                            WORK_TYPE.Visibility = Visibility.Visible;
                            txtLot_PR.Visibility = Visibility.Visible;
                            cboLotType.Visibility = Visibility.Visible;
                            txtSoc.Visibility = Visibility.Visible;
                            cboWorkType.Visibility = Visibility.Visible;

                            ClearValue();
                            TabSetting(_LINE_GROUP_CODE, _PROCID);
                            Util.gridClear(dgLotList);
                            Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                            dgLotList.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_ETC"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["INPUT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            
                        }
                        else if (dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCC" || dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCR" || dtRslt.Rows[0]["LINE_GROUP_CODE"].ToString() == "MCM")  //원형, 각형
                        {
                            LotInfo.Visibility = Visibility.Visible;
                            LotInfo_PY.Visibility = Visibility.Collapsed;

                            dgLotList.Columns["WND_GR_CODE"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_ETC"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["HOLD_FLAG_NAME"].Visibility = Visibility.Collapsed;

                            LOTID_RT.Visibility = Visibility.Visible;
                            LOTTYPE.Visibility = Visibility.Visible;
                            SOC.Visibility = Visibility.Visible;
                            WORK_TYPE.Visibility = Visibility.Visible;
                            txtLot_PR.Visibility = Visibility.Visible;
                            cboLotType.Visibility = Visibility.Visible;
                            txtSoc.Visibility = Visibility.Visible;
                            cboWorkType.Visibility = Visibility.Visible;

                            ClearValue();
                            TabSetting(_LINE_GROUP_CODE, _PROCID);
                            Util.gridClear(dgLotList);
                            Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                            dgLotList.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_ETC"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                        }
                        else
                        {

                            LotInfo.Visibility = Visibility.Collapsed;
                            LotInfo_PY.Visibility = Visibility.Visible;

                            dgLotList.Columns["WND_GR_CODE"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["CNFM_LOSS_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CNFM_LOSS_ETC"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["HOLD_FLAG_NAME"].Visibility = Visibility.Visible;

                            ClearValue();
                            TabSetting(_LINE_GROUP_CODE, _PROCID);
                            Util.gridClear(dgLotList);
                            for (int i = 0; i < dtRslt.Rows.Count; i++)
                            {

                                if (dtRslt.Rows[i]["LOT_INPUT_FLAG"].ToString() == string.Empty)
                                {
                                    dtRslt.Rows[i]["INPUT_QTY"] = Convert.ToDecimal(dtRslt.Rows[i]["PRODUCT_QTY"]) + Convert.ToDecimal(dtRslt.Rows[i]["CNFM_LOSS_ETC"]);

                                }
                            }

                            Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                            dgLotList.Columns["PRODUCT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["GOOD_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["DEFECT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["CNFM_LOSS_ETC"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            dgLotList.Columns["INPUT_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                            
                        }
                    }
                    else
                    {
                        Util.gridClear(dgLotList);
                    }
                }
                 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtSoc_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtSoc.Text, 0))
                {
                    txtSoc.Text = string.Empty;
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLot_PR_PY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidationAssyLot();
            }
        }

        private bool ValidationAssyLot()
        {
            if (string.IsNullOrWhiteSpace(txtLot_PR_PY.Text))
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            if (txtLot_PR_PY.Text.Length != 8)
            {
                // 조립LOTID는 8자리 입니다.
                Util.MessageValidation("SFU4478");
                return false;
            }

            if(txtLot_PR_PY.Text.Trim().Substring(0,3) != _LOTID_RT.Substring(0,3))
            {
                // 제품이 다른 조립LOT입니다.
                Util.MessageValidation("SFU4479");
                txtLot_PR_PY.Text = _LOTID_RT;
                return false;
            }



            DataTable dt = LOT_CHECK();
            if (dt.Rows.Count == 0)
            {
               // 존재하지 않는 조립LOT 입니다.
               Util.MessageValidation("SFU4479");
               txtLot_PR_PY.Text = string.Empty;
               return false;
             }

            return true;
        }

        private DataTable LOT_CHECK()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = txtLot_PR_PY.Text;
                newRow["PRODID"] = _PRODID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_ASSY_LOT_INFO_RESULT", "INDATA", "OUTDATA", inTable);

                // Mix LOT일경우 DA_PRD_SEL_INPUT_ASSY_LOT_INFO_RESULT 실행후 제품이 없을 경우 모델 LOT으로 한번더 찾는다.
                if (_MIX_LOT_FLAG == "Y" && (dtResult == null || dtResult.Rows.Count == 0))
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_MDLLOT_RESULT", "INDATA", "OUTDATA", inTable);
                }

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }
        #endregion

        #region [### 팔레트 완성 조회 ###]

        public void SelectPalletList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = txtSelectLot.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_FO", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgProductionPallet, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionPallet.CurrentCell = dgProductionPallet.GetCell(0, 1);
                dgProductionPallet.Columns["CHK"].Width = new C1.WPF.DataGrid.DataGridLength(40);
                dgProductionPallet.Columns["WIP_NOTE"].Width = new C1.WPF.DataGrid.DataGridLength(280);
                dgProductionPallet.Columns["CELL_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgProductionPallet.Columns["CAPA_GRD_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(120);
                dgProductionPallet.Columns["VLTG_GRD_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(120);
            
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        
        private void cTabMail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cTabProductionPallet.IsSelected == true)
            {
                dgProductionPallet.Columns["CHK"].Width = new C1.WPF.DataGrid.DataGridLength(40);
                dgProductionPallet.Columns["WIP_NOTE"].Width = new C1.WPF.DataGrid.DataGridLength(280);
                dgProductionPallet.Columns["CELL_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgProductionPallet.Columns["CAPA_GRD_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(120);
                dgProductionPallet.Columns["VLTG_GRD_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(120);
            }
        }

        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drSelect = DataTableConverter.Convert(dgProductionPallet.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (LoginInfo.CFG_SHOP_ID.Equals("G182")) //남경
            {
               
                if (_LOTID_RT.Length == 8) //원형
                {
                    int PageCount = 0;
                    int RowCount = 0;

                    // Page수 산출
                    PageCount = drSelect.Length % 18.0 != 0 ? (drSelect.Length / 18) + 1 : drSelect.Length / 18;

                    // Pallet List
                    string[] PalletList = new string[PageCount];

                    // Page 수만큼 Pallet List를 채운다
                    for (int cnt = 0; cnt < PageCount; cnt++)
                    {
                        for (int row = RowCount; row < RowCount + 18; row++)
                        {
                            if (drSelect.Length <= row)
                                break;

                            PalletList[cnt] += drSelect[row]["PALLETE_ID"] + ",";
                        }

                        RowCount += 18;
                    }

                    // grdMain.Children.Add라서 처음 호출이 마지막에 뒤에 보인다.  
                    for (int print = PalletList.Length - 1; print > -1; print--)
                    {
                        TagPrint(PalletList[print].Substring(0, PalletList[print].Length - 1), print + 1);
                    }
                }
                else // 초소형
                {
                    _tagPrintCount = drSelect.Length;

                    foreach (DataRow drPrint in drSelect)
                    {
                        TagPrint(drPrint);
                    }
                }

            }

            else // 오창
            {
                _tagPrintCount = drSelect.Length;

                foreach (DataRow drPrint in drSelect)
                {
                    TagPrint(drPrint);
                }
            }
        }

        private void TagPrint(string pallet, int pageCnt)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.PrintCount = pageCnt.ToString();

            object[] parameters = new object[8];
            parameters[0] = _PROCID;
            parameters[1] = null;
            parameters[2] = pallet;
            parameters[3] = null;
            parameters[4] = null;
            //parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString();
            parameters[5] = "N";      // 디스패치 처리
            parameters[6] = "Y";      // 재발행 여부
            parameters[7] = (bool)chkTagPrint.IsChecked ? "N" : "Y";     // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }


        private void TagPrint(DataRow dr)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            //popupTagPrint.QMSRequestPalletYN = "Y";
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[8];
            parameters[0] = _PROCID;
            parameters[1] = null;              // 설비ID
            parameters[2] = dr["PALLETE_ID"];
            parameters[3] = dr["WIPSEQ"].ToString();
            parameters[4] = dr["CELL_QTY"].ToString();
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletEdit()) return;
            try
            {
                ShowLoadingIndicator();
              
                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("PROCID", typeof(string));   //공정
                inDataTable.Columns.Add("PROD_LOTID", typeof(string)); //생산 LOT ID
                inDataTable.Columns.Add("CALDATE", typeof(string));  //작업일자
                inDataTable.Columns.Add("WIPNOTE", typeof(string));  //비고
                inDataTable.Columns.Add("USERID", typeof(string));   //사용자ID
                inDataTable.Columns.Add("SHIFT", typeof(string));    // SHIFT
                inDataTable.Columns.Add("WRK_USERID", typeof(string)); // 작업자 ID
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string)); //작업자 이름
                inDataTable.Columns.Add("ASSY_LOTID", typeof(string)); //조립 LOTID
                inDataTable.Columns.Add("LOTTYPE", typeof(string)); //LOT유형
                inDataTable.Columns.Add("SOC_VALUE", typeof(string)); //SOC 값
                inDataTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string)); //활성화 작업 유형
                inDataTable.Columns.Add("INPUTQTY", typeof(decimal)); //Input Quantity
                inDataTable.Columns.Add("OUTPUTQTY", typeof(decimal)); //End Quantity
                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["PROCID"] = Util.NVC(_PROCID);
                row["PROD_LOTID"] = txtSelectLot.Text.Trim();  //생산 LOT ID
                row["CALDATE"] = dtpCaldate.Text;//작업일자
                row["WIPNOTE"] = txtNote.Text;      //비고
                row["USERID"] = LoginInfo.USERID; //사용자ID
                row["SHIFT"] = txtShift.Tag.ToString();   //SHIFT
                row["WRK_USERID"] = txtWorker.Tag.ToString(); //작업자 ID
                row["WRK_USER_NAME"] = txtWorker.Text.ToString(); //작업자 이름
                row["ASSY_LOTID"] = txtLot_PR.Text.ToString();  //조립 LOTID
                row["LOTTYPE"] = cboLotType.SelectedValue.ToString();  //LOTTYPE
                row["SOC_VALUE"] = txtSoc.Text.ToString();      //SOC 값
                row["FORM_WRK_TYPE_CODE"] = cboWorkType.SelectedValue.ToString(); //활성화 작업 유형
                row["INPUTQTY"] = _INPUTQTY;   //Input Quantity

                decimal OutQty = 0;
                for (int i = 0; i < dgProductionPallet.Rows.Count-1; i++)
                {
                    OutQty =  Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "CELL_QTY")))+ OutQty;
                }
            
                row["OUTPUTQTY"] = OutQty + Convert.ToDecimal(txtDefectQty.Value.ToString().Replace(",",""));      //End Quantity
                inDataTable.Rows.Add(row);
                //팔레트 완성 정보
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("PALLETID", typeof(string));//팔레트ID
                inLot.Columns.Add("WIPQTY", typeof(Decimal)); //수량
                inLot.Columns.Add("WIPNOTE", typeof(string)); //비고
                inLot.Columns.Add("SHIPTO_NOTE", typeof(string)); //출하처 제안
                inLot.Columns.Add("CAPA_GRD_CODE", typeof(string)); //용량등급
                inLot.Columns.Add("VLTG_GRD_CODE", typeof(string)); //전압등급
                inLot.Columns.Add("RSST_GRD_CODE", typeof(string)); //저항등급
                inLot.Columns.Add("INBOX_QTY", typeof(string)); //인박스 수량
                inLot.Columns.Add("INBOX_TYPE_CODE", typeof(string)); //인박스 유형

                for (int i = 0; i < dgProductionPallet.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "CHK")) == "1")
                    {
                        row = inLot.NewRow();
                        row["PALLETID"]        = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "PALLETE_ID"));
                        row["WIPQTY"]          = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "CELL_QTY")));
                        row["WIPNOTE"]         = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "WIP_NOTE"));
                        row["SHIPTO_NOTE"]     = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "SHIPTO_NOTE"));
                        row["CAPA_GRD_CODE"]   = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "CAPA_GRD_CODE"));
                        row["VLTG_GRD_CODE"]   = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "VLTG_GRD_CODE"));
                        row["RSST_GRD_CODE"]   = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "RSST_GRD_CODE"));
                        row["INBOX_QTY"]       = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "INBOX_QTY")));
                        row["INBOX_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgProductionPallet.Rows[i].DataItem, "INBOX_TYPE_CODE"));
                        inLot.Rows.Add(row);
                    }
                }
                DataTable inDfct = inData.Tables.Add("INDFCT");
                inDfct.Columns.Add("PALLETID", typeof(string));//팔레트ID
                inDfct.Columns.Add("WIPQTY", typeof(Decimal)); //불량수량
                inDfct.Columns.Add("RESNCODE", typeof(string)); //불량코드

                //실적확정 저장 : 원각형 (특성, SMALL)/Grading 공정진척 인 경우 Box 테이블도 수정
                //string bizRuleName = _PROCID.Equals(Process.CircularCharacteristicGrader) ? "BR_PRD_REG_MODIFY_PROD_NEW_MB" : ;

                string bizRuleName = "BR_PRD_REG_MODIFY_PROD";

                if (_PROCID == Process.CircularGrader || _PROCID == Process.SmallGrader || _PROCID == Process.CircularCharacteristicGrader)
                {
                    bizRuleName = "BR_PRD_REG_MODIFY_PROD_NEW_MB";
                }
                
                
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT,INDFCT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        SelectPalletList();
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        string LotID = _LOTID;
                        GetLotList(false);
                      
                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", LotID);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                SelectPalletList();
                                SelectInputPalletList();
                                GetDefectInfo();
                                SelectInpuMaterialList();
                                SelectPalletSummary();
                                GetGQualityInfo();
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                            }
                        }
                    });
                    return;

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 원,각형 : 원각형 특성/Grading 공정진척 Inbox 등록 팝업 호출
        /// </summary>
        private void btnInbox_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxCreate()) return;

            //CMM_FORM_INBOX --> CMM_FORM_MB_INBOX 2023.03
            CMM_FORM_MB_INBOX popupInboxCreate = new CMM_FORM_MB_INBOX { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupInboxCreate.Name.ToString()) == false)
                return;

            DataRow dr = _util.GetDataGridFirstRowBycheck(dgProductionPallet, "CHK");
            popupInboxCreate.ShiftID = Util.NVC(txtShift.Tag);
            popupInboxCreate.ShiftName = Util.NVC(txtShift.Text);
            popupInboxCreate.WorkerID = Util.NVC(txtWorker.Tag);
            popupInboxCreate.WorkerName = Util.NVC(txtWorker.Text);

            object[] parameters = new object[4];
            parameters[0] = _PROCID;
            parameters[1] = _EQSGID;
            parameters[2] = _EQPTID;
            parameters[3] = dr["PALLETE_ID"];
            C1WindowExtension.SetParameters(popupInboxCreate, parameters);

            popupInboxCreate.Closed += new EventHandler(popupInboxCreate_Closed);
            grdMain.Children.Add(popupInboxCreate);
            popupInboxCreate.BringToFront();
        }

        private void popupInboxCreate_Closed(object sender, EventArgs e)
        {
            //CMM_FORM_INBOX --> CMM_FORM_MB_INBOX 2023.03
            CMM_FORM_MB_INBOX popup = sender as CMM_FORM_MB_INBOX;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            //}
            SelectPalletList();

            this.grdMain.Children.Remove(popup);
        }


        private bool ValidationPalletEdit()
        {


            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgProductionPallet, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            //DataRow[] drChk = DataTableConverter.Convert(dgProductionPallet.ItemsSource).Select("CHK = 1");
            //int chkCount = 0;
            //foreach (DataRow row in drChk)
            //{
            //    chkCount = 0;

            //    if (string.IsNullOrWhiteSpace(row["CAPA_GRD_CODE"].ToString()))
            //    {
            //        //// 용량등급을 선택해 주세요.
            //        //Util.MessageValidation("SFU4022");
            //        //return false;
            //        chkCount++;
            //    }

            //    if (string.IsNullOrWhiteSpace(row["RSST_GRD_CODE"].ToString()))
            //    {
            //        //// 저항등급을 선택해 주세요.
            //        //Util.MessageValidation("SFU4242");
            //        //return false;
            //        chkCount++;
            //    }

            //    if (string.IsNullOrWhiteSpace(row["VLTG_GRD_CODE"].ToString()))
            //    {
            //        //// 전압등급을 선택해 주세요.
            //        //Util.MessageValidation("SFU4279");
            //        //return false;
            //        chkCount++;
            //    }

            //    if (chkCount == 3)
            //    {
            //        // 한 개 이상의 등급을 입력 하세요.
            //        Util.MessageValidation("SFU4302");
            //        return false;
            //    }

            //}



            return true;
        }

        private bool ValidationInboxCreate()
        {
            int rowChkCount = DataTableConverter.Convert(dgProductionPallet.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (rowChkCount == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (rowChkCount > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
                return false;
            }

            if (txtShift.Tag.ToString() == string.Empty)
            {
                Util.MessageValidation("SFU4200"); //작업조 정보를 입력하세요
                return false;
            }
            if (txtWorker.Text.ToString() == string.Empty)
            {
                Util.MessageValidation("SFU4201"); //작업자 정보를 입력하세요
                return false;
            }

            return true;
        }

        #endregion

        #region [### 실적확정 저장 ###]

        private void btnSave_Result_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if(_LINE_GROUP_CODE == "MCP")
                {
                    if (!Validation_Tray())
                    {
                        return;
                    }
                   // if(txtLot_PR_PY.Text.Trim() != _LOTID_RT) //조립LOT이 변경이 되었으면
                   // {

                   //     if (txtLot_PR_PY.Text.Trim().Substring(0, 3) != _LOTID_RT.Substring(0, 3))
                   //     {
                   //         // 제품이 다른 조립LOT입니다.
                   //         Util.MessageValidation("SFU4479");
                   //         txtLot_PR_PY.Text = string.Empty;
                   //         return;
                   //     }
                   //     else
                            
                   //         {
                   //         //실적확정 저장하시겠습니까?
                   //         LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                   //             "SFU4198"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                   //             {
                   //                 if (result == MessageBoxResult.OK)
                   //                 {
                   //                     GoodQty_Modify();
                   //                 }
                   //             });
                   //     }

                       
                   // }
                   //else
                   // {
                        //실적확정 저장하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU4198"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    GoodQty_Modify();
                                }
                            });
                    //}

                }
                else
                {
                    if (!Validation())
                    {
                        return;
                    }
                    //실적확정 저장하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU4198"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Result_Confirm_Save();
                                }
                            });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Validation()
        {
            DataRow[] drSelectChange = null;

            //if (dgLotList.IsVisible == true)
            //{
                drSelectChange =  DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'");
            //}
            //else
            //{
            //    drSelectChange = DataTableConverter.Convert(dgLotList_CR.ItemsSource).Select("CHK = '1'");
            //}
            if (drSelectChange.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }
            if(Util.GetCondition(dtpCaldate) == string.Empty)
            {
                Util.MessageValidation("SFU4199"); //작업일자를 입력하세요
                return false;
            }

            if (txtShift.Tag.ToString() == string.Empty)
            {
                Util.MessageValidation("SFU4200"); //작업조 정보를 입력하세요
                return false;
            }
            if (txtWorker.Text.ToString() == string.Empty)
            {
                Util.MessageValidation("SFU4201"); //작업자 정보를 입력하세요
                return false;
            }
            if (txtOutQty.Value.ToString() == "NaN")
            {
                Util.MessageValidation("SFU2921"); //실적수량을 입력하세요 
                return false;
            }
            if (txtOutQty.Value.ToString() == "0")
            {
                Util.MessageValidation("SFU2921"); //실적수량을 입력하세요 
                return false;
            }

            if (txtLot_PR.Text.ToString() == string.Empty)
            {
                Util.MessageValidation("SFU4202"); //조립LOT정보를 입력하세요
                return false;
            }

            if (cboWorkType.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4204"); //작업구분 정보를 선택하세요
                return false;
            }

            if (cboEquipment_MC.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1153"); //설비정보를 선택하세요
                return false;
            }

            return true;
        }

        private bool Validation_Tray()
        {
            DataRow[] drSelectChange = null;
            
            drSelectChange = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'");
           if (drSelectChange.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }
            if(txtOutQty_PY.Value.ToString() == "NaN")
            {
                Util.MessageValidation("SFU2921"); //실적수량을 입력하세요 
                return false;
            }
         
                //if (txtOutQty_PY.Value.ToString() == "0")
                //{
                //    Util.MessageValidation("SFU2921"); //실적수량을 입력하세요 
                //    return false;
                //}
          
            if (txtLot_PR_PY.Text.ToString() == string.Empty)
            {
                Util.MessageValidation("SFU4202"); //조립LOT 정보를 입력하세요
                return false;
            }
           if (cboWorkType_PY.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4204"); //작업구분 정보를 선택하세요
                return false;
            }
            if (cboEquipment_PY.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1153"); //설비정보를 선택하세요
                return false;
            }
            if (txtLot_PR_PY.Text.Trim().Substring(0, 3) != _LOTID_RT.Substring(0, 3))
            {
                // 제품이 다른 조립LOT입니다.
                Util.MessageValidation("SFU4479");
                txtLot_PR_PY.Text = _LOTID_RT;
                return false;
            }
            return true;
        }

        private void Result_Confirm_Save()
        {
            DataSet inData = new DataSet();
            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
            inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
            inDataTable.Columns.Add("PROCID", typeof(string));   //공정
            inDataTable.Columns.Add("PROD_LOTID", typeof(string)); //생산 LOT ID
            inDataTable.Columns.Add("CALDATE", typeof(DateTime));  //작업일자
            inDataTable.Columns.Add("WIPNOTE", typeof(string));  //비고
            inDataTable.Columns.Add("USERID", typeof(string));   //사용자ID
            inDataTable.Columns.Add("SHIFT", typeof(string));    // SHIFT
            inDataTable.Columns.Add("WRK_USERID", typeof(string)); // 작업자 ID
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string)); //작업자 이름
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string)); //조립 LOTID
            inDataTable.Columns.Add("LOTTYPE", typeof(string)); //LOT유형
            inDataTable.Columns.Add("SOC_VALUE", typeof(string)); //SOC 값
            inDataTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string)); //활성화 작업 유형
            inDataTable.Columns.Add("INPUTQTY", typeof(decimal)); //Input Quantity
            inDataTable.Columns.Add("OUTPUTQTY", typeof(decimal)); //End Quantity
            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["PROCID"] = Util.NVC(_PROCID);
            row["PROD_LOTID"] = txtSelectLot.Text.Trim();  //생산 LOT ID
            row["CALDATE"] = dtpCaldate.Text;//작업일자
            row["WIPNOTE"] = txtNote.Text;      //비고
            row["USERID"] = LoginInfo.USERID; //사용자ID
            row["SHIFT"] = txtShift.Tag.ToString();   //SHIFT
            row["WRK_USERID"] = txtWorker.Tag.ToString(); //작업자 ID
            row["WRK_USER_NAME"] = txtWorker.Text.ToString(); //작업자 이름
            row["ASSY_LOTID"] = txtLot_PR.Text.ToString();  //조립 LOTID
            row["LOTTYPE"] = cboLotType.SelectedValue.ToString();  //LOTTYPE
            row["SOC_VALUE"] = txtSoc.Text.ToString();      //SOC 값
            row["FORM_WRK_TYPE_CODE"] = cboWorkType.SelectedValue.ToString(); //활성화 작업 유형
            row["INPUTQTY"] = _INPUTQTY;   //Input Quantity
            row["OUTPUTQTY"] = Convert.ToDecimal(txtOutQty.Value.ToString().Replace(",", ""));      //End Quantity
            inDataTable.Rows.Add(row);

            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("PALLETID", typeof(string));//팔레트ID
            inLot.Columns.Add("WIPQTY", typeof(Decimal)); //수량
            inLot.Columns.Add("WIPNOTE", typeof(string)); //비고
            inLot.Columns.Add("SHIPTO_NOTE", typeof(string)); //출하처 제안
            inLot.Columns.Add("CAPA_GRD_CODE", typeof(string)); //용량등급
            inLot.Columns.Add("VLTG_GRD_CODE", typeof(string)); //전압등급
            inLot.Columns.Add("INBOX_QTY", typeof(string)); //인박스 수량
            inLot.Columns.Add("INBOX_TYPE_CODE", typeof(string)); //인박스 유형

            DataTable inDfct = inData.Tables.Add("INDFCT");
            inDfct.Columns.Add("PALLETID", typeof(string));//팔레트ID
            inDfct.Columns.Add("WIPQTY", typeof(Decimal)); //불량수량
            inDfct.Columns.Add("RESNCODE", typeof(string)); //불량코드
            try
            {
                string LotID = _LOTID;
                //실적확정 저장
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_PROD", "INDATA,INLOT,INDFCT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {

                        GetLotList(false);

                            if (dgLotList.Rows.Count > 0)
                            {
                                int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", LotID);
                                if (idx >= 0)
                                {
                                    DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                    dgLotList.ScrollIntoView(idx, dgLotList.Columns["CHK"].Index);
                                    dgLotList.SelectedIndex = idx;
                                    DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                    SetValue_Update(TempTable);
                                    TabSetting(_LINE_GROUP_CODE, _PROCID);
                                    SelectPalletList();
                                    SelectInputPalletList();
                                    GetDefectInfo();
                                    SelectInpuMaterialList();
                                    SelectPalletSummary();
                                    GetGQualityInfo();
                                    if (_LINE_GROUP_CODE.Equals("MCC") || _LINE_GROUP_CODE.Equals("MCS") || _LINE_GROUP_CODE.Equals("MCR") || _LINE_GROUP_CODE.Equals("MCM"))
                                    {

                                        btnSave_Result.Visibility = Visibility.Visible;
                                        //txtOutQty.IsEnabled = true;
                                }
                               }
                            }
                        
                    });
                    return;

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MODIFY_PROD", ex.Message, ex.ToString());

            }
        }


        private void GoodQty_Modify()
        {
            DataSet inData = new DataSet();
            DataTable inDataTable = inData.Tables.Add("INDATA");

            inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
            inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
            inDataTable.Columns.Add("PROCID", typeof(string));   //공정
            inDataTable.Columns.Add("PROD_LOTID", typeof(string)); //생산 LOT ID
            inDataTable.Columns.Add("CALDATE", typeof(DateTime));  //작업일자
            inDataTable.Columns.Add("WIPNOTE", typeof(string));  //비고
            inDataTable.Columns.Add("USERID", typeof(string));   //사용자ID
            inDataTable.Columns.Add("SHIFT", typeof(string));   //작업조
            inDataTable.Columns.Add("WRK_USERID", typeof(string));   //작업자ID
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));   //작업자 이름
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string));   //조립LOTID
            inDataTable.Columns.Add("LOTTYPE", typeof(string)); //LOT TYPE
            inDataTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string)); //활성화 작업유형
            inDataTable.Columns.Add("EQPTID", typeof(string));   //설비
            inDataTable.Columns.Add("OUTPUTQTY", typeof(decimal));   //양품수량

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"]    = SRCTYPE.SRCTYPE_UI;              //소스Type
            row["IFMODE"]     = IFMODE.IFMODE_OFF;               //인터페이스 모드
            row["PROCID"]     = Util.NVC(_PROCID);               //공정
            row["PROD_LOTID"] = txtSelectLot_PY.Text.Trim();     //생산 LOT ID
            row["CALDATE"]    = dtpCaldate_PY.Text;              //작업일자
            row["WIPNOTE"]    = txtNote_PY.Text;                 //비고
            row["USERID"]     = LoginInfo.USERID;                //사용자ID
            row["SHIFT"]      = txtShift_PY.Tag.ToString();      //Shift
            row["WRK_USERID"] = txtWorker_PY.Tag.ToString();     //작업자ID
            row["WRK_USER_NAME"] = txtWorker_PY.Text.ToString(); //작업자명
            row["ASSY_LOTID"] = txtLot_PR_PY.Text.Trim();        //조립LOT
            row["LOTTYPE"] = cboLotType_PY.SelectedValue.ToString(); // LOT 유형
            row["FORM_WRK_TYPE_CODE"] = cboWorkType_PY.SelectedValue.ToString(); // 작업유형
            row["EQPTID"] = cboEquipment_PY.SelectedValue.ToString(); //설비    
            row["OUTPUTQTY"] = Convert.ToDecimal(txtOutQty_PY.Value.ToString().Replace(",", "")); //양품수량
            inDataTable.Rows.Add(row);
          
            try
            {
                string LotID = _LOTID;
                //실적확정 저장
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_PROD_PC", "INDATA", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {

                        GetLotList(false);
                    
                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", LotID);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                dgLotList.ScrollIntoView(idx, dgLotList.Columns["CHK"].Index);
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                SelectPalletList();
                                SelectInputPalletList();
                                GetDefectInfo();
                                SelectInpuMaterialList();
                                SelectPalletSummary();
                                GetGQualityInfo();
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();

                                //Degas Tray 일 경우 양품수량 수정
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }
                        }
                        
                    });
                    return;

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SAVE_PROD", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region [### 팔레트 투입 정보 탭 조회 ###]
        private void SelectInputPalletList() //string sLot, string sWipSeq)
        {
            try
            {
                ////InitializeControls();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = txtSelectLot.Text;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["INPUT_LOT_TYPE_CODE"] = "PROD";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INPUT_HISTORY_FO", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgInputPallet, dtResult, FrameOperation, true);
                Decimal SumCellQty = 0;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dgInputPallet.CurrentCell = dgInputPallet.GetCell(0, 1);
                  
                    for(int i=0; i< dtResult.Rows.Count; i++)
                    {
                        SumCellQty = Convert.ToDecimal(dtResult.Rows[i]["INPUT_QTY"].ToString() == string.Empty ? "0" : dtResult.Rows[i]["INPUT_QTY"].ToString()) + SumCellQty;
                    }
                }
                _INPUTQTY = SumCellQty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### 불량정보 탭 조회 ###]
        private void GetDefectInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["AREAID"] = _AREAID;
                newRow["EQPTID"] = _EQPTID;
                newRow["ACTID"] = "DEFECT_LOT";
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
               
                inTable.Rows.Add(newRow);

                _dtResult = new DataTable();
                _dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_INFO_FORMATION", "INDATA", "OUTDATA", inTable);

                // Pallet ID, 재공수량 Setting
                //_dtResult.Columns.Add("LOTID", typeof(string));
                _dtResult.Columns.Add("WIPQTY", typeof(double));
                _dtResult.Columns.Add("SUMQTY", typeof(double));

                if (_dtResult.Rows.Count > 0)
                {
                    SetPalletID();
                    Util.GridSetData(dgDefect, _dtResult, null);

                    (dgDefect.Columns["cboPallet"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(_dtPallet.DefaultView.ToTable(true, "LOTID").Copy());
                }
               

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetPalletID()
        {
            try
            {
                DataTable dtDefect = new DataTable();
                string ResnCode = string.Empty;

                foreach (DataRow dRow in _dtResult.Rows)
                {
                    ResnCode += dRow["RESNCODE"] + ",";
                }

                if (string.IsNullOrWhiteSpace(ResnCode))
                    return;



                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIP_DFCT_CODE", typeof(string));
                inTable.Columns.Add("ASSY_PROC_LOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SOC_VALUE", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("LOTDTTM_CR", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _PROCID;
                newRow["WIP_DFCT_CODE"] = ResnCode.Substring(0, ResnCode.Length - 1);
                newRow["ASSY_PROC_LOTID"] = _LOTID_RT;
                newRow["PRODID"] = _PRODID;
                if(_LINE_GROUP_CODE == "MCS") //초소형
                {
                    newRow["SOC_VALUE"] = null;

                }
                else
                {
                    newRow["SOC_VALUE"] = _SOC_VALUE;
                }
                //newRow["LOTDTTM_CR"] = _LOTDTTM_CR;
                newRow["MKT_TYPE_CODE"] = _MKT_TYPE_CODE;
                newRow["LOTTYPE"] = _LOTTYPE;
                newRow["AREAID"] = _AREAID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_PALLET_RESN_FO_MOD", "INDATA", "OUTDATA", inTable);

                //////////////////////////////////// 불량 코드에 LOT이 없는경우 NEW로 넣어 준다
                _dtPallet = dtResult.Copy();
              
                for (int row = 0; row < _dtResult.Rows.Count; row++)
                {
                    DataRow[] dr = _dtPallet.Select("WIP_DFCT_CODE ='" + _dtResult.Rows[row]["RESNCODE"].ToString() + "'  AND LOTID='" + _dtResult.Rows[row]["LOTID"].ToString() + "'" );

                    DataRow drAdd = _dtPallet.NewRow();
                    drAdd["WIP_DFCT_CODE"] = _dtResult.Rows[row]["RESNCODE"];
                    drAdd["LOTID"] = "NEW";
                    drAdd["WIPQTY"] = 0;
                    _dtPallet.Rows.Add(drAdd);

                    if (dr.Length == 0)
                    {
                        _dtResult.Rows[row]["LOTID"] = "NEW";
                        _dtResult.Rows[row]["WIPQTY"] = 0;
                        _dtResult.Rows[row]["SUMQTY"] = Util.NVC_Decimal(_dtResult.Rows[row]["RESNQTY"].ToString());

                        //DataRow drAdd = _dtPallet.NewRow();
                        //drAdd["WIP_DFCT_CODE"] = _dtResult.Rows[row]["RESNCODE"];
                        //drAdd["LOTID"] = "NEW";
                        //_dtPallet.Rows.Add(drAdd);

                        //_dtResult.Rows[row]["LOTID"] = "NEW";
                        //_dtResult.Rows[row]["WIPQTY"] = 0;
                        //_dtResult.Rows[row]["SUMQTY"] = 0;
                    }
                    else
                    {
                        _dtResult.Rows[row]["LOTID"] = dr[0]["LOTID"];
                        _dtResult.Rows[row]["WIPQTY"] = dr[0]["WIPQTY"];
                        _dtResult.Rows[row]["SUMQTY"] = dr[0]["WIPQTY"];
                    }
                }

                _dtPallet.AcceptChanges();
                _dtResult.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Cell.Column.Name.Equals("cboPallet"))
            {
                if (((C1.WPF.DataGrid.C1DataGrid)sender).Focused)
                    return;

                DataRow[] dr = _dtPallet.Select("WIP_DFCT_CODE ='" + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE").ToString()) + "' AND LOTID ='" +
                                                                     Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID").ToString()) + "'");

                if (dr.Length > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);
                    dt.Rows[e.Cell.Row.Index]["RESNQTY"] = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY").ToString());
                    dt.Rows[e.Cell.Row.Index]["WIPQTY"] = dr[0]["WIPQTY"];
                    dt.Rows[e.Cell.Row.Index]["SUMQTY"] = dr[0]["WIPQTY"];
                    dt.AcceptChanges();

                    int Index = e.Cell.Row.Index;

                    Util.GridSetData(dgDefect, dt, null);
                    dgDefect.SelectedIndex = Index;

                }

            }
            else if (e.Cell.Column.Name.Equals("RESNQTY"))
            {
                int resnQty = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY").ToString());
                decimal wipQty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY").ToString());

                DataTableConverter.SetValue(e.Cell.Row.DataItem, "SUMQTY", resnQty + wipQty);


            }
        }
        private void dgDefect_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;

            if (cbo != null)
            {
                if (e.Column.Name == "cboPallet")
                {
                    DataTable dt = _dtPallet.Copy();
                    dt.Select("WIP_DFCT_CODE <> '" + Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RESNCODE")) + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    dt.AcceptChanges();

                    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());

                    int Index = 0;

                    if (cbo.SelectedValue != null)
                    {
                        DataRow[] drIndex = dt.Select("LOTID ='" + cbo.SelectedValue.ToString() + "'");

                        if (drIndex.Length > 0)
                        {
                            Index = dt.Rows.IndexOf(drIndex[0]);
                        }
                    }

                    cbo.SelectedIndex = Index;
                }
            }
        }
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }
            }));
        }
        private void dgDefect_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int rIdx = 0;
                int cIdx = 0;

                C1DataGrid grid = sender as C1DataGrid;

                rIdx = grid.CurrentCell.Row.Index;
                cIdx = grid.CurrentCell.Column.Index;

                if (grid.CurrentCell.Column.Name.Equals("RESNQTY"))
                {
                    if (grid.GetRowCount() > ++rIdx)
                    {
                        grid.Selection.Clear();
                        grid.CurrentCell = grid.GetCell(rIdx, cIdx);
                        grid.Selection.Add(grid.GetCell(rIdx, cIdx));

                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }
                    }
                }
            }
        }
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID")).Equals("NEW"))
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4025");
                return;
            }

            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.DefectPalletYN = "Y";

            object[] parameters = new object[8];
            parameters[0] = _PROCID;
            parameters[1] = _EQPTID;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
            parameters[3] = _WIPSEQ;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNQTY"));
            parameters[5] = "N";      // 디스패치 처리
            parameters[6] = "N";      // 출력여부
            parameters[7] = "N";      // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Defect_Closed);

            ////popupTagPrint.Show();
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupTagPrint);
                    popupTagPrint.BringToFront();
                    break;
                }
            }
        }
        private void popupTagPrint_Defect_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
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
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                dgDefect.EndEdit();
                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("PROCID", typeof(string));   //공정
                inDataTable.Columns.Add("PROD_LOTID", typeof(string)); //생산 LOT ID
                inDataTable.Columns.Add("CALDATE", typeof(string));  //작업일자
                inDataTable.Columns.Add("WIPNOTE", typeof(string));  //비고
                inDataTable.Columns.Add("USERID", typeof(string));   //사용자ID
                inDataTable.Columns.Add("SHIFT", typeof(string));    // SHIFT
                inDataTable.Columns.Add("WRK_USERID", typeof(string)); // 작업자 ID
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string)); //작업자 이름
                inDataTable.Columns.Add("ASSY_LOTID", typeof(string)); //조립 LOTID
                inDataTable.Columns.Add("LOTTYPE", typeof(string)); //LOT유형
                inDataTable.Columns.Add("SOC_VALUE", typeof(string)); //SOC 값
                inDataTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string)); //활성화 작업 유형
                inDataTable.Columns.Add("INPUTQTY", typeof(decimal)); //Input Quantity
                inDataTable.Columns.Add("OUTPUTQTY", typeof(decimal)); //End Quantity
                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["PROCID"] = Util.NVC(_PROCID);
                row["PROD_LOTID"] = txtSelectLot.Text.Trim();  //생산 LOT ID
                row["CALDATE"] = dtpCaldate.Text;//작업일자
                row["WIPNOTE"] = txtNote.Text;      //비고
                row["USERID"] = LoginInfo.USERID; //사용자ID
                row["SHIFT"] = txtShift.Tag.ToString();   //SHIFT
                row["WRK_USERID"] = txtWorker.Tag.ToString(); //작업자 ID
                row["WRK_USER_NAME"] = txtWorker.Text.ToString(); //작업자 이름
                row["ASSY_LOTID"] = txtLot_PR.Text.ToString();  //조립 LOTID
                row["LOTTYPE"] = cboLotType.SelectedValue.ToString();  //LOTTYPE
                row["SOC_VALUE"] = txtSoc.Text.ToString();      //SOC 값
                row["FORM_WRK_TYPE_CODE"] = cboWorkType.SelectedValue.ToString(); //활성화 작업 유형
                row["INPUTQTY"] = _INPUTQTY;   //Input Quantity

                decimal OutQty = 0;
                for (int i = 0; i < dgDefect.Rows.Count - 1; i++)
                {
                    OutQty = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY"))) + OutQty;
                }
                row["OUTPUTQTY"] = OutQty + Convert.ToDecimal(txtOutQty.Value.ToString().Replace(",", ""));      //End Quantity
                inDataTable.Rows.Add(row);
                //팔레트 완성 정보
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("PALLETID", typeof(string));//팔레트ID
                inLot.Columns.Add("WIPQTY", typeof(Decimal)); //수량
                inLot.Columns.Add("WIPNOTE", typeof(string)); //비고
                inLot.Columns.Add("SHIPTO_NOTE", typeof(string)); //출하처 제안
                inLot.Columns.Add("CAPA_GRD_CODE", typeof(string)); //용량등급
                inLot.Columns.Add("VLTG_GRD_CODE", typeof(string)); //전압등급
                inLot.Columns.Add("INBOX_QTY", typeof(string)); //인박스 수량
                inLot.Columns.Add("INBOX_TYPE_CODE", typeof(string)); //인박스 유형
                //불량정보 저장
                DataTable inDfct = inData.Tables.Add("INDFCT");
                inDfct.Columns.Add("PALLETID", typeof(string));//팔레트ID
                inDfct.Columns.Add("WIPQTY", typeof(Decimal)); //불량수량
                inDfct.Columns.Add("RESNCODE", typeof(string)); //불량코드

                for (int i = 0; i < dgDefect.Rows.Count-1; i++)
                {
                    row = inDfct.NewRow();
                    row["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "LOTID"));
                    row["WIPQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    inDfct.Rows.Add(row);
               }
             
                string LotID = _LOTID;
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MODIFY_PROD", "INDATA,INLOT,INDFCT", null, inData);

                Util.MessageInfo("SFU1275");

                GetLotList(false);
             
                    if (dgLotList.Rows.Count > 0)
                    {
                        int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", LotID);
                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                            //row 색 바꾸기
                            dgLotList.SelectedIndex = idx;
                            DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                            ClearValue();
                            SetValue_Update(TempTable);
                            TabSetting(_LINE_GROUP_CODE, _PROCID);
                            SelectPalletList();
                            SelectInputPalletList();
                            GetDefectInfo();
                            SelectInpuMaterialList();
                            SelectPalletSummary();
                            GetGQualityInfo();
                            cTabDefect.IsSelected = true;
                        }

                    }
               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                GetDefectInfo();
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 자재 투입 탭 조회###]
        private void SelectInpuMaterialList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = txtSelectLot.Text;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["INPUT_LOT_TYPE_CODE"] = "MTRL";
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INPUT_HISTORY_FO", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgMaterial, dtResult, null);
                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgMaterial.CurrentCell = dgMaterial.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //자재 투입
        private void txtMaterialID_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (!ValidationMaterialInput()) return;
                InputMaterial();
              
            }

         
           
        }
        //자재 투입
        private void InputMaterial()
        {
            try
            {
                DataSet inDataSet = GetBR_PRD_CHK_INPUT_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["IN_INPUT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_EQPTID);
                newRow["PROD_LOTID"] = _LOTID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["MOD_FLAG"] = "Y";
                inTable.Rows.Add(newRow);

             

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = txtMaterialID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = cboLocation.SelectedValue.ToString();

                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_TW", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        txtMaterialID.Text = string.Empty;

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabMaterial.IsSelected = true;

                                //Degas Tray 일 경우 양품수량 수정
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //자재 투입 DataTable
        private DataSet GetBR_PRD_CHK_INPUT_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("MOD_FLAG", typeof(string));
            DataTable inBox = indataSet.Tables.Add("IN_INPUT");
            inBox.Columns.Add("INPUT_LOTID", typeof(string));
            inBox.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

            return indataSet;
        }

        private void txtMaterialID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        //자재 투입 버튼
        private void btnMaterialInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMaterialInput()) return;
            InputMaterial();
        }
        //자재 투입 취소
        private void btnMaterialHistoryCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMaterialHistoryCancel()) return;

            // 투입을 취소 하시겠습니까?
            Util.MessageConfirm("SFU1982", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelMaterialHistory();
                }
            });
        }
        //자재 투입 취소 Validation
        private bool ValidationMaterialHistoryCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgMaterial))
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK") < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        //자재 투입 취소 함수
        private void CancelMaterialHistory()
        {
            try
            {
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgMaterial, "CHK");

                DataSet inDataSet = GetBR_PRD_REG_CANCEL_TERMINATE_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["INLOT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "INPUT_SEQNO").GetString());
                newRow["LOTID"] = DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "PALLETID").GetString();
                newRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "INPUT_QTY").GetString());
                newRow["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "INPUT_QTY2").GetString());
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_LOT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");
                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabInbox.IsSelected = true;
                                cTabMaterial.IsSelected = true;

                                //Degas Tray 일 경우 양품수량 수정
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //자재 투입 취소 Data Table
        private DataSet GetBR_PRD_REG_CANCEL_TERMINATE_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inLot = indataSet.Tables.Add("INLOT");
            inLot.Columns.Add("INPUT_SEQNO", typeof(string));
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(Decimal));
            inLot.Columns.Add("WIPQTY2", typeof(Decimal));

            return indataSet;
        }

        private bool ValidationMaterialInput()
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }
            if(cboLocation.SelectedIndex == 0)
            {
                // 자재 투입위치를 선택하세요.
                Util.MessageValidation("SFU1820");
                return false;
            }


            if (string.IsNullOrWhiteSpace(txtMaterialID.Text))
            {
                // 투입자재 LOT ID를 입력하세요.
                Util.MessageValidation("SFU1984");
                return false;
            }

            return true;
        }




        #endregion

        #region [### 등급별 수량 집계탭 조회###]
        public void SelectPalletSummary()
        {
            try
            {
                //컬럼변경
                SetControlHeader();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = txtSelectLot.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_SUM_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgPalletSummary, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region [### 자주검사탭 조회###]

        private void GetGQualityInfo()
        {
           try
            {
                string sBizName = "DA_QCA_SEL_SELF_INSP_TAB";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["CLCTITEM_CLSS4"] = "A";
                newRow["LOTID"] = txtSelectLot.Text;
                newRow["WIPSEQ"] = _WIPSEQ;
            

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                if(dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgQuality, dtResult, FrameOperation, true);

                    // 검사 항목의 Max Column까지만 보이게
                    _maxColumn = 0;
                    _maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);

                    int Startcol = dgQuality.Columns["CLCT_COUNT"].Index;
                    int ForCount = 0;

                    for (int col = Startcol + 1; col < dgQuality.Columns.Count; col++)
                    {
                        ForCount++;

                        if (ForCount > _maxColumn)
                            dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                    }

                    //dgQuality.AlternatingRowBackground = null;

                    // Merge
                    string[] sColumnName = new string[] { "CLCTSEQ", "ACTDTTM", "CLCTITEM_CLSS_NAME1" };
                    _Util.SetDataGridMergeExtensionCol(dgQuality, sColumnName, DataGridMergeMode.VERTICAL);
                }
               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion
        
        #region [### 완성 INBOX 탭 조회 ###]
        private void SelectInbox() //string sLot, string sWipSeq)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("DELETE_YN", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["DELETE_YN"] = "N";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_RESULT", "INDATA", "OUTDATA", inTable);
                              
                Util.GridSetData(dgProductionInbox, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);
          
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectInbox_FINALEXTERNAL() //string sLot, string sWipSeq)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                //inTable.Columns.Add("WIPSEQ", typeof(string));
            
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LOTID;
                //newRow["WIPSEQ"] = _WIPSEQ;
            
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_FINALEXTERNA_INFO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionInbox, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductionInbox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAKEOVER_YN")).Equals("Y")
                        || Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STORED_YN")).Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));

        }

        private void dgProductionInbox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgProductionInbox_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if ((e.Cell.Column.Name.Equals("CELL_QTY") || e.Cell.Column.Name.Equals("CAPA_GRD_CODE")) && e.Cell.IsEditable == true)
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                    //e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }
            }

        }


        private void chkAllInbox_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgProductionInbox);
        }

        private void chkAllInbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionInbox);
        }

        private void SetGridCombo_INBOX()
        {
            SetGridCapaGrade_INBOX();
        }

        private void SetGridCapaGrade_INBOX()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AREAID;
                newRow["EQSGID"] = _EQSGID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

              

                (dgProductionInbox.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnInboxSave_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            dgProductionInbox.EndEditRow(true);

            if (!ValidationInboxModify())
                return;

            // Inbox를 수정 하시겠습니까?
            Util.MessageConfirm("SFU4481", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyInbox();
                }
            });
        }

        private bool ValidationInboxModify()
        {

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                SelectInbox();
                return false;
            }

            if (dr.Length > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
                SelectInbox();
                return false;
            }

            if (dr[0]["TAKEOVER_YN"].ToString().Equals("Y") && dr[0]["WIPSEQ_YN"].ToString().Equals("Y"))
            {
                // 재공이 다음 공정으로 이동 되어 수정할 수 없습니다.
                Util.MessageValidation("SFU4367");
                SelectInbox();
                return false;
            }

            if (dr[0]["WIPSEQ_YN"].Equals("Y"))
            {
                // 재작업을 위해 투입되어 수정 할 수 없습니다.
                Util.MessageValidation("SFU4504");
                SelectInbox();
                return false;
            }
            if (dr[0]["WIPQTY_YN"].Equals("Y"))
            {
                // 분할 또는 병합되어 수정 할 수 없습니다
                Util.MessageValidation("SFU4505");
                SelectInbox();
                return false;
            }

            return true;
        }

        private void ModifyInbox()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("WIPQTY", typeof(Decimal));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIPTO_NOTE", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inTable.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inTable.Columns.Add("RSST_GRD_CODE", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                inTable.Columns.Add("INBOX_QTY", typeof(Decimal));

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = _PROCID;
                newRow["PALLETID"] = dr[0]["INBOX_ID"];
                newRow["WIPQTY"] = dr[0]["CELL_QTY"];
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = _SHIFT;
                newRow["WRK_USERID"] = _WRK_USERID;
                newRow["WRK_USER_NAME"] = _WRK_USER_NAME;
                newRow["CAPA_GRD_CODE"] = dr[0]["CAPA_GRD_CODE"];
                newRow["INBOX_TYPE_CODE"] = dr[0]["INBOX_TYPE_CODE"];
                newRow["INBOX_QTY"] = dr[0]["INBOX_QTY"];
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_MODIFY_PALLET", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabInbox.IsSelected = true;
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void btnTagPrint_InBox_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;

            string processName = _PROCNAME ;
            string modelId = _MODLID;
            string projectName = _PRJT_NAME;
            string marketTypeName = _MKT_TYPE_NAME;
            string assyLotId = _LOTID_RT;
            string calDate = _CARDATE;
            string shiftName = _SHFT_NAME;
            string equipmentShortName = _EQPTSHORTNAME;
            string inspectorId = Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductionInbox, "CHK")].DataItem, "INSPECTORID"));   
            //string inspectorId = Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductionInbox, "CHK")].DataItem, "VISL_INSP_USERID"));
            if (string.IsNullOrEmpty(calDate))
            {
                calDate = dtpCaldate.SelectedDateTime.Year.GetString() + "." +
                          dtpCaldate.SelectedDateTime.Month.GetString() + "." +
                          dtpCaldate.SelectedDateTime.Day.GetString();
            }

            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));     //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dgProductionInbox.Rows)
            {
                if (row.Type == DataGridRowType.Item &&
                    (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                     DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                {
                    DataRow dr = dtLabelItem.NewRow();
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID_RT"));
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "CELL_QTY")).GetString();
                    dr["ITEM005"] = equipmentShortName;
                    dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                    dr["ITEM007"] = inspectorId;
                    dr["ITEM008"] = marketTypeName;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM010"] = string.Empty;
                    dr["ITEM011"] = string.Empty;
                    dtLabelItem.Rows.Add(dr);
                 

                    // 라벨 발행 이력 저장
                    DataRow newRow = inTable.NewRow();
                    newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                    newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));    // Cell ID
                    newRow["PRT_ITEM02"] = _WIPSEQ;
                    newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));                                                // 재발행 여부
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    inTable.Rows.Add(newRow);

                    


                }
            }
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            //string portName = dr["PORTNAME"].GetString();
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                // 라벨 발행이력 저장
                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
               
            }

        }

        private bool ValidationPrint()
        {

            if (_util.GetDataGridCheckFirstRowIndex(dgProductionInbox, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
           if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == "LBL0106"
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            return true;
        }

        // INBOX 삭제
        private void btnInboxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxDelete()) return;
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            // 삭제시 실적 수량이 변경됩니다. Inbox를 삭제하시겠습니까?
            Util.MessageConfirm("SFU4499", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteInbox();
                }
            }, parameters);
        }
        // 삭제 Valldation
        private bool ValidationInboxDelete()
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                SelectInbox();
                return false;
            }

            foreach (DataRow drChk in dr)
            {
                if (drChk["TAKEOVER_YN"].Equals("Y") && drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재공이 다음 공정으로 이동 되어 삭제할 수 없습니다.
                    Util.MessageValidation("SFU1871");
                    SelectInbox();
                    return false;
                }
                if ( drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재작업을 위해 투입되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU4500");
                    SelectInbox();
                    return false;
                }
                if (drChk["WIPQTY_YN"].Equals("Y"))
                {
                    // 분할 또는 병합되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU4501");
                    SelectInbox();
                    return false;
                }

            }

            return true;
        }

        // 삭제 함수
        private void DeleteInbox()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("MOD_FLAG", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("PALLETID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPTID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["MOD_FLAG"] = "Y";
                newRow["PROCID"] = _PROCID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["PALLETID"] = drDel["INBOX_ID"].ToString();
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_PALLET", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabInbox.IsSelected = true;
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        //INBOX 생성
        private void btnInboxCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (_CTNR_TYPE_CODE == "T")
                    return;
                if (dgProductionInbox.Rows.Count == 0)
                    return;

                FCS002_322_CREATE_INBOX popupInboxCreate = new FCS002_322_CREATE_INBOX();
                popupInboxCreate.FrameOperation = this.FrameOperation;

                object[] parameters = new object[10];
                parameters[0] = _LOTID;             //생산LOT
                parameters[1] = _EQPTID;            //설비정보
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[0].DataItem, "INBOX_TYPE_CODE")); //BOX 코드
                parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductionInbox.Rows[0].DataItem, "INBOX_TYPE_NAME")); //BOX 코드명
                parameters[4] = _AREAID; // 동정보
                parameters[5] = _EQSGID; // 라인정보
                parameters[6] = _PROCID; // 공정정보
                parameters[7] = _SHIFT; // 작업조
                parameters[8] = _WRK_USERID; // 작업자ID
                parameters[9] = _WRK_USER_NAME; // 작업자명
                C1WindowExtension.SetParameters(popupInboxCreate, parameters);

                popupInboxCreate.Closed += new EventHandler(popupInboxCreate_Select_Closed);
                grdMain.Children.Add(popupInboxCreate);
                popupInboxCreate.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        // INBOX 생성 함수
        private void popupInboxCreate_Select_Closed(object sender, EventArgs e)
        {
            FCS002_322_CREATE_INBOX popupCreateInbox = sender as FCS002_322_CREATE_INBOX;


            if (popupCreateInbox.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                string Lotid = _LOTID;
                GetLotList(false);

                if (dgLotList.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                        //row 색 바꾸기
                        dgLotList.SelectedIndex = idx;
                        DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                        ClearValue();
                        SetValue_Update(TempTable);
                        TabSetting(_LINE_GROUP_CODE, _PROCID);
                        if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                        {
                            SelectInbox_FINALEXTERNAL();
                        }
                        else
                        {
                            SelectInbox();
                        }
                        GetOutInboxList();
                        SelectInputInboxList();
                        SelectDefectList();
                        SelectInpuMaterialList();
                        GetGQualityInfo();
                        cTabInbox.IsSelected = true;
                        if (_LINE_GROUP_CODE.Equals("MCP"))
                        {
                            btnSave_Result.Visibility = Visibility.Visible;
                        }
                    }

                }
            }
           this.grdMain.Children.Remove(popupCreateInbox);
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgProductionInbox;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", DataTableConverter.GetValue(row.DataItem, "PRINT_YN").GetString() != "Y");
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.Two:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Two;
                    break;
                case CheckBoxHeaderType.Two:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        #endregion

        #region [### 검사공정 완성 INBOX 탭 조회 ###]

        private void GetOutInboxList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
             
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LOTID;
              
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_FINALEXTERNA_INFO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionInbox_Out, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgProductionInbox_Out.CurrentCell = dgProductionInbox_Out.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (CommonVerify.HasDataGridRow(dgProductionInbox_Out))
                    {
                        int rowIndex = _util.GetDataGridFirstRowIndexByColumnValue(dgProductionInbox_Out, "INBOX_ID", txtInBoxId.Text);
                        if (rowIndex > -1)
                        {
                            DataTableConverter.SetValue(dgProductionInbox_Out.Rows[rowIndex].DataItem, "CHK", true);
                            dgProductionInbox_Out.SelectedIndex = rowIndex;
                        }
                        else
                        {
                            Util.MessageValidation("SFU2060");
                        }
                    }
                    else
                    {
                        Util.MessageValidation("SFU2060");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInBoxSave_Out_Click(object sender, RoutedEventArgs e)
        {
            dgProductionInbox_Out.EndEdit();
            dgProductionInbox_Out.EndEditRow(true);

            if (!ValidationInBoxSave()) return;

            // Inbox를 수정 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4331", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyInbox_Out();
                }
            }, parameters);
        }

        private bool ValidationInBoxSave()
        {
            
           DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
           foreach (DataRow drChk in dr)
            {
                if (drChk["TAKEOVER_YN"].Equals("Y") && drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재공이 다음 공정으로 이동 되어 수정 할 수 없습니다.
                    Util.MessageValidation("SFU4367");
                    GetOutInboxList();
                    return false;
                }


                if (drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재작업을 위해 투입되어 저장 할 수 없습니다.
                    Util.MessageValidation("SFU4502");
                    GetOutInboxList();
                    return false;
                }
                if (drChk["WIPQTY_YN"].Equals("Y"))
                {
                    // 분할 또는 병합되어 저장 할 수 없습니다.
                    Util.MessageValidation("SFU4503");
                    GetOutInboxList();
                    return false;
                }

                if (drChk["CELL_QTY"].Equals("") || drChk["CELL_QTY"].Equals("0"))
                {
                    // 수량은 0보다 큰 정수로 입력 하세요.
                    Util.MessageValidation("SFU3092");
                    GetOutInboxList();
                    return false;
                }
            }

            return true;
        }

        private void ModifyInbox_Out()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LOTID", typeof(string));
                inBox.Columns.Add("ACTQTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["VISL_INSP_USERID"] = null;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inBox.NewRow();
                    newRow["LOTID"] = drDel["INBOX_ID"].ToString();
                    newRow["ACTQTY"] = Util.NVC_Int(drDel["CELL_QTY"]);
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_INBOX", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabOutPallet.IsSelected = true;
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void btnInBoxDelete_Out_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInBoxDelete()) return;

            // IInbox를 삭제 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4332", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteInbox_Out();
                }
            }, parameters);
        }
        private bool ValidationInBoxDelete()
        {
             DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            foreach (DataRow drChk in dr)
            {
               
               if (drChk["TAKEOVER_YN"].Equals("Y") && drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재공이 다음 공정으로 이동 되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU1871");
                    GetOutInboxList();
                    return false;
                }


                if (drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재작업을 위해 투입되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU4500");
                    GetOutInboxList();
                    return false;
                }
                if (drChk["WIPQTY_YN"].Equals("Y"))
                {
                    // 분할 또는 병합되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU4501");
                    GetOutInboxList();
                    return false;
                }

            }

            return true;
        }

        private void DeleteInbox_Out()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LOTID", typeof(string));
                inBox.Columns.Add("ACTQTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["VISL_INSP_USERID"] = null;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inBox.NewRow();
                    newRow["LOTID"] = drDel["INBOX_ID"].ToString();
                    newRow["ACTQTY"] = 0;
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_INBOX", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabOutPallet.IsSelected = true;
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        //삭제 취소
        private void btnInBoxDeleteCancel_Out_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInBoxDeleteCancel()) return;

            // 삭제 취소 하시겠습니까?
            Util.MessageConfirm("SFU4369", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelDeleteInbox_Out();
                }
            });
        }
        private bool ValidationInBoxDeleteCancel()
        {
           
            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            foreach (DataRow drChk in dr)
            {

                if (drChk["TAKEOVER_YN"].Equals("Y")  && drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재공이 다음 공정으로 이동 되어 삭제 취소 할 수 없습니다.
                    Util.MessageValidation("SFU4506");
                    GetOutInboxList();
                    return false;
                }

                if (drChk["WIPSEQ_YN"].Equals("Y"))
                {
                    // 재작업을 위해 투입되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU4507");
                    GetOutInboxList();
                    return false;
                }

                if (drChk["WIPQTY_YN"].Equals("Y"))
                {
                    // 재작업을 위해 투입되어 삭제 할 수 없습니다.
                    Util.MessageValidation("SFU4508");
                    GetOutInboxList();
                    return false;
                }
            }
            return true;
        }
        private void CancelDeleteInbox_Out()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LOTID", typeof(string));
                inBox.Columns.Add("ACTQTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");

                foreach (DataRow drCancel in dr)
                {
                    newRow = inBox.NewRow();
                    newRow["LOTID"] = drCancel["INBOX_ID"].ToString();
                    newRow["ACTQTY"] = Util.NVC_Int(drCancel["BEFORE_QTY"]);
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_FV", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabOutPallet.IsSelected = true;
                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void dgProductionInbox_Out_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DELETE_YN")).Equals("Y"))
                    {
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgProductionInbox_Out_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgProductionInbox_Out_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if (e.Cell.Column.Name.Equals("CELL_QTY") && e.Cell.IsEditable == true)
                {                 
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                }
            }

        }

        private void dgProductionInbox_Out_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("CELL_QTY"))
            {
                //DataRow dr = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");

                //// 완성된 생산LOT Inbox는 수정 불가
                //if (!Util.NVC(dr["WIPSTAT"]).Equals("PROC"))
                //{
                //    e.Cancel = true;
                //}

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DELETE_YN")).Equals("Y"))
                {
                    e.Cancel = true;
                }

            }
        }

        private void chkAllInbox_Out_Checked(object sender, RoutedEventArgs e)
        {
            //DataRow dr = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            //if (!Util.NVC(dr["WIPSTAT"]).Equals("PROC")) return;

            Util.DataGridCheckAllChecked(dgProductionInbox_Out);
        }

        private void chkAllInbox_Out_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionInbox_Out);
        }


        private void btnTagPrint_Out_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectPrint(dgProductionInbox_Out, true)) return;

            PrintLabel("G", dgProductionInbox_Out);
        }

        private bool ValidationDefectPrint(C1DataGrid dg, bool IsGoodTag)
        {
            
            if (_util.GetDataGridCheckFirstRowIndex(dg, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
 

            if (IsGoodTag)
            {
                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox_Out.ItemsSource).Select("CHK = 1");
                foreach (DataRow drrow in dr)
                {
                    if (Util.NVC_Int(drrow["CELL_QTY"]) != Util.NVC_Int(drrow["ORG_CELL_QTY"])
                        || Util.NVC(drrow["CAPA_GRD_CODE"]) != Util.NVC(drrow["ORG_CAPA_GRD_CODE"]))
                    {
                        // 변경된 데이터가 존재합니다.\r\n먼저 저장한 후 태그 발행하세요.
                        Util.MessageValidation("SFU4447");
                        return false;
                    }
                }
            }

            return true;
        }


        private void PrintLabel(string IsQultType, C1DataGrid dg)
        {
           
            string processName = _PROCNAME;
            string modelId = _MODLID;
            string projectName = _PRJT_NAME;
            string marketTypeName = _MKT_TYPE_NAME;
            string assyLotId = _LOTID_RT;
            string calDate = _CARDATE;
            string shiftName = _SHFT_NAME;
            string equipmentShortName = _EQPTSHORTNAME;
            string inspectorId = Util.NVC(DataTableConverter.GetValue(dgProductionInbox_Out.Rows[_util.GetDataGridCheckFirstRowIndex(dgProductionInbox_Out, "CHK")].DataItem, "INSPECTORID"));


            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 양품 Tag인 경우 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item &&
                    (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                     DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                {
                    DataRow dr = dtLabelItem.NewRow();

                    if (IsQultType.Equals("G"))
                    {
                        dr["LABEL_CODE"] = "LBL0106";
                        dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                        dr["ITEM002"] = modelId + "(" + projectName + ") ";
                        dr["ITEM003"] = assyLotId;
                        dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "CELL_QTY")).GetString();
                        dr["ITEM005"] = equipmentShortName;
                        //dr["ITEM006"] = calDate + "(" + shiftName + ")";
                        dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                        dr["ITEM007"] = inspectorId;
                        dr["ITEM008"] = marketTypeName;
                        dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                        dr["ITEM010"] = null;
                        dr["ITEM011"] = null;

                        // 라벨 발행 이력 저장
                        DataRow newRow = inTable.NewRow();
                        newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                        newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));    // Cell ID
                        newRow["PRT_ITEM02"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                        newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));                                                // 재발행 여부
                        newRow["INSUSER"] = LoginInfo.USERID;
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                        inTable.Rows.Add(newRow);

                    }
                    else
                    {
                        dr["LABEL_CODE"] = "LBL0107";
                        dr["ITEM001"] = modelId + "(" + projectName + ") ";
                        dr["ITEM002"] = assyLotId;
                        dr["ITEM003"] = marketTypeName;
                        dr["ITEM004"] = string.Empty;
                        dr["ITEM005"] = equipmentShortName;
                        dr["ITEM006"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "DFCT_CODE_DETL_NAME"));
                        dr["ITEM007"] = calDate + "(" + shiftName + ")";
                        dr["ITEM008"] = inspectorId;
                        dr["ITEM009"] = string.Empty;
                        dr["ITEM010"] = string.Empty;
                        dr["ITEM011"] = string.Empty;
                    }

                    dtLabelItem.Rows.Add(dr);
                }
            }

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                string Lotid = _LOTID;
                GetLotList(false);

                if (dgLotList.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                        //row 색 바꾸기
                        dgLotList.SelectedIndex = idx;
                        DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                        ClearValue();
                        SetValue_Update(TempTable);
                        TabSetting(_LINE_GROUP_CODE, _PROCID);
                        if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                        {
                            SelectInbox_FINALEXTERNAL();
                        }
                        else
                        {
                            SelectInbox();
                        }
                        GetOutInboxList();
                        SelectInputInboxList();
                        SelectDefectList();
                        SelectInpuMaterialList();
                        GetGQualityInfo();
                        cTabOutPallet.IsSelected = true;
                        if (_LINE_GROUP_CODE.Equals("MCP"))
                        {
                            btnSave_Result.Visibility = Visibility.Visible;
                        }
                    }

                }

            }
        }



        #endregion

        #region [### 투입 INBOX 탭 조회 ###]
        public void SelectInputInboxList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                newRow["INPUT_LOT_TYPE_CODE"] = "PROD";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INPUT_HISTORY_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgInputInbox, dtResult, null, true);

              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //Pallet ID 조회
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.Enter)
            {
                if (!ValidationPalletInput()) return;

                // 투입
                if ((bool)rdoPallet.IsChecked)
                {
                    // 대차투입
                    InputCart();
                }
                else
                {
                    // Inbox 투입
                    InputPallet();
                }
            }
        }
        // 조회체크
        private bool ValidationPalletInput()
        {
            if (string.IsNullOrWhiteSpace(_LOTID))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPalletID.Text))
            {
                if ((bool)rdoPallet.IsChecked)
                {
                    // 투입 Pallet를 입력 하세요.
                    Util.MessageValidation("SFU4015");
                }
                else
                {
                    // 투입 Inbox를 입력 하세요.
                    Util.MessageValidation("SFU4015");
                }

                return false;
            }

            return true;
        }
        //Pallet 입력
        private void InputPallet()
        {
            try
            {
                string bizRuleName = string.Empty;

                //if ((bool)rdoPallet.IsChecked)
                //{
                //    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CTNR";
                //}
                //else
                //{
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_INBOX";
                //}

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("MOD_FLAG", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_EQPTID);
                newRow["PROD_LOTID"] = _LOTID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["MOD_FLAG"] = "Y";
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = txtPalletID.Text;
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_BOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        txtPalletID.Text = string.Empty;

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabInput_Inbox.IsSelected = true;

                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //Pallet 입력
        private void btnPalletInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletInput()) return;
        
            // 투입
            if ((bool)rdoPallet.IsChecked)
            {
                // 대차투입
                InputCart();
            }
            else
            {
                // Inbox 투입
                InputPallet();
            }
        }

        private void chkAllInbox_Inbox_Checked(object sender, RoutedEventArgs e)
        {

            Util.DataGridCheckAllChecked(dgInputInbox);
        }

        private void chkAllInbox_Inbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgInputInbox);
        }
        
        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (txtPalletID == null) return;

            txtPalletID.SelectAll();
            txtPalletID.Focus();
        }

        private void rdoInbox_Checked(object sender, RoutedEventArgs e)
        {
            if (txtPalletID == null) return;

            txtPalletID.SelectAll();
            txtPalletID.Focus();
        }

        private void InputCart()
        {
            CMM_POLYMER_FORM_CART_INPUT popupCartInput = new CMM_POLYMER_FORM_CART_INPUT();
            popupCartInput.FrameOperation = this.FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = _PROCID;
            parameters[1] = _PROCNAME;
            parameters[2] = _EQPTID;
            parameters[3] = _EQPTNAME; ;
            parameters[4] = _LOTID;
            parameters[5] = Util.NVC(txtPalletID.Text);

            C1WindowExtension.SetParameters(popupCartInput, parameters);

            popupCartInput.Closed += new EventHandler(popupCartInput_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartInput);
                    popupCartInput.BringToFront();
                    break;
                }
            }
        }

        private void popupCartInput_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_INPUT popup = sender as CMM_POLYMER_FORM_CART_INPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            txtPalletID.Text = string.Empty;

            string Lotid = _LOTID;
            GetLotList(false);

            if (dgLotList.Rows.Count > 0)
            {
                int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                if (idx >= 0)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                    //row 색 바꾸기
                    dgLotList.SelectedIndex = idx;
                    DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                    ClearValue();
                    SetValue_Update(TempTable);
                    TabSetting(_LINE_GROUP_CODE, _PROCID);
                    if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                    {
                        SelectInbox_FINALEXTERNAL();
                    }
                    else
                    {
                        SelectInbox();
                    }
                    GetOutInboxList();
                    SelectInputInboxList();
                    SelectDefectList();
                    SelectInpuMaterialList();
                    GetGQualityInfo();
                    cTabInput_Inbox.IsSelected = true;

                    if (_LINE_GROUP_CODE.Equals("MCP"))
                    {
                        btnSave_Result.Visibility = Visibility.Visible;
                    }
                }

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

        /// <summary>
        /// 투입 취소 팝업
        /// </summary>
        private void InputCancelCart()
        {
            CMM_POLYMER_FORM_CART_INPUT_CANCEL popupCartInputCancel = new CMM_POLYMER_FORM_CART_INPUT_CANCEL();
            popupCartInputCancel.FrameOperation = this.FrameOperation;

            DataRow[] dr = DataTableConverter.Convert(dgInputInbox.ItemsSource).Select("CHK = 1");

            object[] parameters = new object[5];
            parameters[0] = _PROCID;
            parameters[1] = _PROCNAME;
            parameters[2] = _EQPTID;
            parameters[3] = _EQPTNAME;
            parameters[4] = dr;

            C1WindowExtension.SetParameters(popupCartInputCancel, parameters);

            popupCartInputCancel.Closed += new EventHandler(popupCartInputCancel_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartInputCancel);
                    popupCartInputCancel.BringToFront();
                    break;
                }
            }
        }

        private void popupCartInputCancel_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_INPUT_CANCEL popup = sender as CMM_POLYMER_FORM_CART_INPUT_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            txtPalletID.Text = string.Empty;

            string Lotid = _LOTID;
            GetLotList(false);

            if (dgLotList.Rows.Count > 0)
            {
                int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                if (idx >= 0)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                    //row 색 바꾸기
                    dgLotList.SelectedIndex = idx;
                    DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                    ClearValue();
                    SetValue_Update(TempTable);
                    TabSetting(_LINE_GROUP_CODE, _PROCID);
                    if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                    {
                        SelectInbox_FINALEXTERNAL();
                    }
                    else
                    {
                        SelectInbox();
                    }
                    GetOutInboxList();
                    SelectInputInboxList();
                    SelectDefectList();
                    SelectInpuMaterialList();
                    GetGQualityInfo();
                    cTabInput_Inbox.IsSelected = true;

                    if (_LINE_GROUP_CODE.Equals("MCP"))
                    {
                        btnSave_Result.Visibility = Visibility.Visible;
                    }
                }

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

        //취소 이벤트
        private void btnPalletInputCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletCancel()) return;

            //// 투입취소 하시겠습니까?
            //Util.MessageConfirm("SFU1988", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        // Pallet 투입 취소
            //        CancelPallet();
            //    }
            //});
            InputCancelCart();
        }
        //취소 체크
        private bool ValidationPalletCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgInputInbox))
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgInputInbox, "CHK") < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        //취소함수
        private void CancelPallet()
        {
            try
            {
                string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";

                //int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgPallet, "CHK");

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("INPUT_SEQNO", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(Decimal));
                inLot.Columns.Add("WIPQTY2", typeof(Decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgInputInbox.ItemsSource).Select("CHK = 1");
                foreach (DataRow drRow in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC_Int(drRow["INPUT_SEQNO"]).GetString();
                    newRow["LOTID"] = Util.NVC(drRow["CELLID"]).GetString();
                    newRow["WIPQTY"] = Util.NVC_Decimal(drRow["INPUT_QTY"]).GetString();
                    newRow["WIPQTY2"] = Util.NVC_Decimal(drRow["INPUT_QTY2"]).GetString();
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabInput_Inbox.IsSelected = true;

                                if (_LINE_GROUP_CODE.Equals("MCP"))
                                {
                                    btnSave_Result.Visibility = Visibility.Visible;
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
      
        //잔량대기
        private void btnPalletRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletRemain()) return;

            FCS002_322_WAIT_REMAIN popupPalletRemain = new FCS002_322_WAIT_REMAIN { FrameOperation = FrameOperation };

            popupPalletRemain.ShiftID = Util.NVC(txtShift_PY.Tag);
            popupPalletRemain.ShiftName = Util.NVC(txtShift_PY.Text);
            popupPalletRemain.WorkerID = Util.NVC(txtWorker_PY.Tag);
            popupPalletRemain.WorkerName = Util.NVC(txtWorker_PY.Text);
            popupPalletRemain.ShiftDateTime = string.Empty;


            object[] parameters = new object[5];
            parameters[0] = _PROCID;
            parameters[1] = _PROCNAME;
            parameters[2] = Util.NVC(cboEquipment_PY.SelectedValue);
            parameters[3] = Util.NVC(cboEquipment_PY.Text);
            parameters[4] = GetSelectInputPalletRow();
            C1WindowExtension.SetParameters(popupPalletRemain, parameters);

            popupPalletRemain.Closed += popupPalletRemainWait_Closed;
            grdMain.Children.Add(popupPalletRemain);
            popupPalletRemain.BringToFront();
        }
      
        //잔량대기 닫기
        private void popupPalletRemainWait_Closed(object sender, EventArgs e)
        {
            FCS002_322_WAIT_REMAIN popup = sender as FCS002_322_WAIT_REMAIN;
            if (popup.ConfirmSave)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                string Lotid = _LOTID;
                GetLotList(false);
                int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                if (idx >= 0)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                    //row 색 바꾸기
                    dgLotList.SelectedIndex = idx;
                    DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                    ClearValue();
                    SetValue_Update(TempTable);
                    TabSetting(_LINE_GROUP_CODE, _PROCID);
                    if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                    {
                        SelectInbox_FINALEXTERNAL();
                    }
                    else
                    {
                        SelectInbox();
                    }
                    GetOutInboxList();
                    SelectInputInboxList();
                    SelectDefectList();
                    SelectInpuMaterialList();
                    GetGQualityInfo();
                    cTabInput_Inbox.IsSelected = true;
                }
            }

            this.grdMain.Children.Remove(popup);

        }

        public DataRow GetSelectInputPalletRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgInputInbox.ItemsSource).Select("CHK = 1");

                DataTable test = new DataTable();
                if (dr == null || dr.Length < 1)
                    row = null;
                else
                    row = dr[0];

                return row;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
    
        // 잔량대기 valldation
        private bool ValidationPalletRemain()
        {

            int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgInputInbox, "CHK");
            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }
            int CheckCount = 0;

            for (int i = 0; i < dgInputInbox.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInputInbox.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }

            return true;
        }
        #endregion

        #region [### 불량 탭 조회 ###]
        public void SelectDefectList()
        {
            try
            {
                ShowLoadingIndicator();
                // 해더 정보 조회
                GetDefectHeader();

                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT_INFO_PC";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 불량 저장시 비교용
                        _defectList = result;

                        Util.GridSetData(dgProductionDefect, result, null, true);

                        if (CommonVerify.HasTableRow(result))
                        {
                            //C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgProductionDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                            //StackPanel allPanel = allColumn?.Header as StackPanel;
                            //CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                            //if (allCheck != null) allCheck.IsChecked = true;

                            // 저장,조회시 현상태 유지
                            if (!_defectTabButtonClick)
                                SetDefectGridSelect(-1, -1, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgProductionDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row.Type != DataGridRowType.Item)
                return;

            if (e.Cell.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                CheckBox cbo = e.Cell.Presenter.Content as CheckBox;
                if (cbo != null && cbo.IsChecked == true)
                {
                    cbo.Visibility = DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID").GetString() != "CHARGE_PROD_LOT" ? Visibility.Visible : Visibility.Collapsed;
                    //cbo.IsChecked = string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN").GetString(), "N");
                }
            }

            dgProductionDefect?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // IsReadOnly=True 색상표시
                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible || e.Cell.Column.Name.Equals("CHK"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }
                if (e.Cell.Column.Name.IndexOf("RESNQTY") > -1 && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(50);
                }
                // 출력 색상 표시
                if (e.Cell.Column.Name.IndexOf("GRADE") > -1 && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45);

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN" + e.Cell.Column.Name.Substring(5, e.Cell.Column.Name.Length - 5))).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F7C8"));
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").Equals(1))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0DA"));
                    }
                    else
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEBEB"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }

            }));
        }

        private void dgProductionDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgProductionDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkAllDefect_Checked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllChecked(dgProductionDefect);

            // 2018-03-06 불량탭 추가
            SetDefectGridSelect();
        }

        private void chkAllDefect_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionDefect);

            // 2018-03-06 불량탭 추가
            // Load Event 호출
            DataTable dt = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
            Util.GridSetData(dgProductionDefect, dt, null, true);
        }

        private void btnDefectSave_D_Click(object sender, RoutedEventArgs e)
        {
            dgProductionDefect.EndEdit();
            dgProductionDefect.EndEditRow(true);

            if (!ValidationDefectSave()) return;

            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDefect();
                }
            });
        }
        
        private bool ValidationDefectSave()
        {

            if (!CommonVerify.HasDataGridRow(dgProductionDefect))
            {
                // 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return false;
            }

            return true;
        }

        private void SaveDefect()
        {
            try
            {
                ShowLoadingIndicator();
                //const string bizRuleName = "BR_QCA_REG_WIPREASONCOLLECT_ALL";

                const string bizRuleName = "BR_PRD_REG_MODIFY_DFCT_PC";

                // 변경값 비교
                DataTable defect = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
                DataTable defectSave = new DataTable();
                defectSave.Columns.Add("NEW_ROW", typeof(int));
                defectSave.Columns.Add("NEW_COL", typeof(int));


                int ColStart = defect.Columns.IndexOf("GRADE1");
                int ColEnd = defect.Columns.IndexOf("GRADE1") + _defectGradeCount;

                for (int row = 0; row < defect.Rows.Count; row++)
                {
                    for (int col = ColStart; col < ColEnd; col++)
                    {
                        if (Util.NVC(defect.Rows[row][col]) != Util.NVC(_defectList.Rows[row][col]))
                        {
                            DataRow newrow = defectSave.NewRow();
                            newrow["NEW_ROW"] = row;
                            newrow["NEW_COL"] = col;
                            defectSave.Rows.Add(newrow);
                        }
                    }
                }
                // 변경자료가 없으면 Return
                if (defectSave.Rows.Count == 0)
                {
                    HiddenLoadingIndicator();

                    // 변경내용이 없습니다.
                    Util.MessageInfo("SFU1226");
                    return;
                }


                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["PROD_LOTID"] = _LOTID;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                 // 불량 코드별 불량수량
                DataTable inDefectTable = ds.Tables.Add("INRESN");
                inDefectTable.Columns.Add("ACTID", typeof(string));
                inDefectTable.Columns.Add("RESNCODE", typeof(string));
                inDefectTable.Columns.Add("RESNQTY", typeof(double));

                // 불량 코드, 등급별 수량
                DataTable inDefectGrdTable = ds.Tables.Add("INGRD");
                inDefectGrdTable.Columns.Add("ACTID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNGRID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNCODE", typeof(string));
                inDefectGrdTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inDefectGrdTable.Columns.Add("RESNQTY", typeof(double));
                inDefectGrdTable.Columns.Add("RESNGR_ABBR_CODE", typeof(string));

                foreach (DataRow row in defectSave.Rows)
                {
                    // `불량코드별 수량
                    string lotid = _LOTID;
                    string wipseq = _WIPSEQ;
                    string actid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["ACTID"]);
                    string resncode = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNCODE"]);
                    string resnqty = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNQTY"]);
                    string costcntrid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["COST_CNTR_ID"]);
                    string capagrdcode = Util.NVC(dgProductionDefect.Columns[int.Parse(row["NEW_COL"].ToString())].Header);
                    string graderesnqty = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())][int.Parse(row["NEW_COL"].ToString())]);
                    string columnheadername = Util.NVC(defect.Columns[int.Parse(row["NEW_COL"].ToString())].ColumnName);
                    string resngrid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNGRID"]);
                    string resngr_abbr_code = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNGR_ABBR_CODE"]);

                    DataRow[] drSelect = inDefectTable.Select("ACTID = '" + actid + "' And RESNCODE = '" + resncode + "'");
                    if (drSelect.Length == 0)
                    {
                        DataRow newRow = inDefectTable.NewRow();
                        newRow["ACTID"] = actid;
                        newRow["RESNCODE"] = resncode;
                        newRow["RESNQTY"] = resnqty.Equals("") ? 0 : double.Parse(resnqty);
                       inDefectTable.Rows.Add(newRow);
                    }

                    // 불량 등급별 수량
                    DataRow newRowGrd = inDefectGrdTable.NewRow();
                    newRowGrd["ACTID"] = actid;
                    newRowGrd["RESNGRID"] = resngrid;
                    newRowGrd["RESNCODE"] = resncode;
                    newRowGrd["CAPA_GRD_CODE"] = capagrdcode;
                    newRowGrd["RESNQTY"] = graderesnqty.Equals("") ? 0 : double.Parse(graderesnqty);
                    newRowGrd["RESNGR_ABBR_CODE"] = resngr_abbr_code;
                    inDefectGrdTable.Rows.Add(newRowGrd);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN,INGRD", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");
                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabDefect_F.IsSelected = true;
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnDefectPrint_Click(object sender, RoutedEventArgs e)
        {
            // 선택 추가
            SetDefectGridCheckSelect();
            if (!ValidationDefectPrint()) return;

            // 선택 불량저장
            //SaveDefectPrint();
            if (_IsDefectSave.Equals(false)) return;

            string processName = _PROCNAME;
            string modelId = _MODLID;
            string projectName = _PRJT_NAME;
            string marketTypeName = _MKT_TYPE_NAME;
            string assyLotId = _LOTID_RT;
            string calDate = _CARDATE;
            string shiftName = _SHFT_NAME;
            string equipmentShortName = _EQPTSHORTNAME;
            string inspectorId = "";

            // 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();
         
            foreach (C1.WPF.DataGrid.DataGridCell cell in dgProductionDefect.Selection.SelectedCells)
            {
                if (cell.Row.Index >= dgProductionDefect.Rows.Count + dgProductionDefect.FrozenBottomRowsCount)
                    continue;

                if (cell.Column.Index < dgProductionDefect.Columns["GRADE1"].Index)
                    continue;

                if (cell.Column.Index > dgProductionDefect.Columns["GRADE1"].Index + _defectGradeCount)
                    continue;

                DataRow dr = dtLabelItem.NewRow();

                dr["LABEL_CODE"] = "LBL0107";
                dr["ITEM001"] = modelId + "(" + projectName + ") ";
                dr["ITEM002"] = assyLotId;
                dr["ITEM003"] = marketTypeName;
                dr["ITEM004"] = cell.Value.ToString().Equals("0") ? string.Empty : cell.Value.ToString();
                dr["ITEM005"] = equipmentShortName;
                dr["ITEM006"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "DFCT_CODE_DETL_NAME"));
                dr["ITEM007"] = calDate + "(" + shiftName + ")";
                dr["ITEM008"] = inspectorId;
                dr["ITEM009"] = string.Empty;
                dr["ITEM010"] = cell.Column.Header.ToString();     // 등급
                dr["ITEM011"] = _LOTID + "+" +
                                Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNGR_ABBR_CODE")) + "+" +
                                cell.Column.Header.ToString();
                dtLabelItem.Rows.Add(dr);

                // 라벨 발행 이력 저장
                DataRow newRow = inTable.NewRow();
                newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNGRID"));
                newRow["PRT_ITEM03"] = cell.Column.Header.ToString();
                newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = _LOTID;
                inTable.Rows.Add(newRow);
            }
           
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);
            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                // 라벨 발행이력 저장
                string BizRuleName = string.Empty;
                BizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST_DEFECT";

                new ClientProxy().ExecuteService(BizRuleName, "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabDefect_F.IsSelected = true;
                            }

                        }
                        //SelectResultListLocal("PRINT_N");


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }

           
        }
        
        private bool ValidationDefectPrint()
        {

            //if (_util.GetDataGridCheckFirstRowIndex(dgProductionDefect, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == "LBL0107"
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            //_labelCode

            return true;
        }

        private void btnDefectInput_Click(object sender, RoutedEventArgs e)
        {
            btnDefectInput.Background = new SolidColorBrush(Colors.Red);
            btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Black);

            SetDefectGridInput(false);
        }

        private void btnDefectPrintSelect_Click(object sender, RoutedEventArgs e)
        {
            btnDefectInput.Background = new SolidColorBrush(Colors.Black);
            btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Red);

            SetDefectGridInput(true);
        }

        private void SetDefectGridInput(bool breadonly)
        {
            if (dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount == 0)
            {
                return;
            }
            _defectTabButtonClick = true;

            dgProductionDefect.Selection.Clear();

            DataTable dt = DataTableConverter.Convert(dgProductionDefect.ItemsSource);
            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            for (int row = 0; row < dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount; row++)
            {
                dt.Rows[row]["CHK"] = 0;
            }

            dgProductionDefect.Columns["CHK"].IsReadOnly = breadonly == true ? false : true;

            for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
            {
                dgProductionDefect.Columns[col].IsReadOnly = breadonly;
            }

            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgProductionDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            allCheck.IsChecked = false;
            allCheck.IsEnabled = breadonly;

            Util.GridSetData(dgProductionDefect, dt, null, true);

        }

        private void dgProductionDefect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                if (cell.Column.IsReadOnly == true)
                {
                    return;
                }

                SetDefectGridSelect(cell.Row.Index, cell.Column.Index);
            }
        }

        private void dgProductionDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (e.Cell.Column.Name.IndexOf("GRADE") > -1)
                {
                    double ResnQtyColumnSum = 0;
                    int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

                    for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        ResnQtyColumnSum += DataTableConverter.GetValue(e.Cell.Row.DataItem, dgProductionDefect.Columns[col].Name).GetDouble();
                    }
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", ResnQtyColumnSum);
                }
            }
        }

        private void SetDefectGridSelect(int CheckRow = -1, int CheckCol = -1, bool IsraedInly = false)
        {
            if (dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount == 0)
                return;

            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgProductionDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;

            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            if (CheckRow.Equals(-1))
            {
                btnDefectInput.Background = new SolidColorBrush(Colors.Black);
                btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Red);

                // 조회 후 전체 또는 헤더의 전체 선택시
                dgProductionDefect.Selection.Clear();

                for (int row = 0; row < dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount; row++)
                {
                    for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        if (row == 0 && IsraedInly)
                        {
                            dgProductionDefect.Columns[col].IsReadOnly = true;
                        }
                        // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                        if (dgProductionDefect.Columns[col].Visibility == Visibility.Collapsed)
                            continue;

                        if (!Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[row].DataItem, "PRINT_YN" + dgProductionDefect.Columns[col].Name.Substring(5, dgProductionDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                        {
                            C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCell(row, col);

                            if (!IsraedInly)
                            {
                                // 조회후 조회시는 제외, 전체선택 Click시만 적용
                                DataTableConverter.SetValue(dgProductionDefect.Rows[row].DataItem, "CHK", true);
                                dgProductionDefect.Selection.Add(cell);
                            }
                        }
                    }
                }

            }
            else
            {
                if (allCheck.IsChecked == true)
                    return;

                if (CheckCol == dgProductionDefect.Columns["CHK"].Index)
                {
                    // Check 칼럼인 경우 출력을 제외한 Column 전체 선택
                    for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {

                        // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                        if (dgProductionDefect.Columns[col].Visibility == Visibility.Collapsed)
                            continue;
                        C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCell(CheckRow, col);

                        if (Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[CheckRow].DataItem, "PRINT_YN" + dgProductionDefect.Columns[col].Name.Substring(5, dgProductionDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                        {
                            cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgProductionDefect.Rows[CheckRow].DataItem, "CHK").Equals(0))
                            {
                                dgProductionDefect.Selection.Add(cell);

                                cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0DA"));
                            }
                            else
                            {
                                dgProductionDefect.Selection.Remove(cell);

                                if (cell.Column.IsReadOnly == true)
                                {
                                    cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEBEB"));
                                }
                                else
                                {
                                    cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                    }
                }

            }

            // header 체크
            int rowChkCount = DataTableConverter.Convert(dgProductionDefect.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (allCheck != null && allCheck.IsChecked == false && rowChkCount == dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount)
            {
                allCheck.Checked -= chkAllDefect_Checked;
                allCheck.IsChecked = true;
                allCheck.Checked += chkAllDefect_Checked;
            }

            dgProductionDefect.EndEdit();
            dgProductionDefect.EndEditRow(true);

        }

        private void SetDefectGridCheckSelect()
        {
            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            for (int row = 0; row < dgProductionDefect.Rows.Count - dgProductionDefect.FrozenBottomRowsCount; row++)
            {
                for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                {
                    if (!DataTableConverter.GetValue(dgProductionDefect.Rows[row].DataItem, "CHK").Equals(1))
                        continue;

                    // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                    if (dgProductionDefect.Columns[col].Visibility == Visibility.Collapsed)
                        continue;

                    if (!Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[row].DataItem, "PRINT_YN" + dgProductionDefect.Columns[col].Name.Substring(5, dgProductionDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                    {
                        C1.WPF.DataGrid.DataGridCell cell = dgProductionDefect.GetCell(row, col);
                        dgProductionDefect.Selection.Add(cell);
                    }
                }
            }
        }

        private void SaveDefectPrint()
        {
            try
            {
                _IsDefectSave = true;
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DEFECT_MCP";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _EQPTID;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                // 불량 코드별 불량수량
                DataTable inDefectTable = ds.Tables.Add("INRESN");
                inDefectTable.Columns.Add("LOTID", typeof(string));
                inDefectTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectTable.Columns.Add("ACTID", typeof(string));
                inDefectTable.Columns.Add("RESNCODE", typeof(string));
                inDefectTable.Columns.Add("RESNQTY", typeof(double));
                inDefectTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDefectTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDefectTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectTable.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDefectTable.Columns.Add("COST_CNTR_ID", typeof(string));
                inDefectTable.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDefectTable.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                // 불량 코드, 등급별 수량
                DataTable inDefectGrdTable = ds.Tables.Add("INGRD");
                inDefectGrdTable.Columns.Add("LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectGrdTable.Columns.Add("ACTID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNCODE", typeof(string));
                inDefectGrdTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inDefectGrdTable.Columns.Add("RESNQTY", typeof(double));
                inDefectGrdTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectGrdTable.Columns.Add("DFCT_GR_LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("LABEL_PRT_FLAG", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridCell cell in dgProductionDefect.Selection.SelectedCells)
                {
                    if (cell.Row.Index >= dgProductionDefect.Rows.Count + dgProductionDefect.FrozenBottomRowsCount)
                        continue;

                    if (cell.Column.Index < dgProductionDefect.Columns["GRADE1"].Index)
                        continue;

                    if (cell.Column.Index > dgProductionDefect.Columns["GRADE1"].Index + _defectGradeCount)
                        continue;

                    DataRow newRow = inDefectGrdTable.NewRow();
                    newRow["LOTID"] = _LOTID;
                    newRow["WIPSEQ"] = _WIPSEQ;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                    newRow["CAPA_GRD_CODE"] = cell.Column.Header.ToString();
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, dgProductionDefect.Columns[cell.Column.Index].Name.ToString())).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, dgProductionDefect.Columns[cell.Column.Index].Name.ToString())));
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_GR_LOTID"] = string.Empty;
                    newRow["LABEL_PRT_FLAG"] = "Y";
                    inDefectGrdTable.Rows.Add(newRow);

                    DataRow[] drSelect = inDefectTable.Select("ACTID ='" + Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID")) + "' And " +
                                                              "RESNCODE ='" + Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE")) + "'");

                    if (drSelect.Length == 0)
                    {
                        newRow = inDefectTable.NewRow();
                        newRow["LOTID"] = _LOTID;
                        newRow["WIPSEQ"] = _WIPSEQ;
                        newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID"));
                        newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                        newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "RESNQTY")));
                        newRow["RESNCODE_CAUSE"] = string.Empty;
                        newRow["PROCID_CAUSE"] = string.Empty;
                        newRow["RESNNOTE"] = string.Empty;
                        newRow["DFCT_TAG_QTY"] = 0;
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;

                        if (Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                        {
                            newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgProductionDefect.Rows[cell.Row.Index].DataItem, "COST_CNTR_ID"));
                        }
                        else
                        {
                            newRow["COST_CNTR_ID"] = string.Empty;
                        }

                        newRow["A_TYPE_DFCT_QTY"] = 0;
                        newRow["C_TYPE_DFCT_QTY"] = 0;
                        inDefectTable.Rows.Add(newRow);

                    }

                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN,INGRD", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            _IsDefectSave = false;
                            return;
                        }

                        Util.MessageInfo("SFU1275");
                        string Lotid = _LOTID;
                        GetLotList(false);

                        if (dgLotList.Rows.Count > 0)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgLotList, "LOTID", Lotid);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgLotList.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgLotList.SelectedIndex = idx;
                                DataTable TempTable = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = '1'").CopyToDataTable();
                                ClearValue();
                                SetValue_Update(TempTable);
                                TabSetting(_LINE_GROUP_CODE, _PROCID);
                                if (FINALEXTERNAL_PROCESS(_PROCID) != string.Empty)
                                {
                                    SelectInbox_FINALEXTERNAL();
                                }
                                else
                                {
                                    SelectInbox();
                                }
                                GetOutInboxList();
                                SelectInputInboxList();
                                SelectDefectList();
                                SelectInpuMaterialList();
                                GetGQualityInfo();
                                cTabDefect_F.IsSelected = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        _IsDefectSave = false;
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                _IsDefectSave = false;
            }
        }

        private void GetDefectHeader()
        {
            try
            {
                _defectGradeCount = 0;
                _IsDefectSave = true;

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_FORM_GRADE_TYPE_CODE_HEADER";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _EQSGID;
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(result))
                        {
                            _defectGradeCount = result.Rows.Count;

                            for (int row = 0; row < result.Rows.Count; row++)
                            {
                                string ColumnName = "GRADE" + (row + 1).ToString();
                                dgProductionDefect.Columns[ColumnName].Header = Util.NVC(result.Rows[row]["GRD_CODE"]);
                                dgProductionDefect.Columns[ColumnName].Visibility = Visibility.Visible;
                            }
                            // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                            SetDefectGridGradeColumn();
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void cboGradeSelect_SelectionChanged(object sender, EventArgs e)
        {
            SetDefectGridGradeColumn();
        }


        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void SetDefectGridGradeColumn()
        {
            string gradeSelect = Util.NVC(cboGradeSelect.SelectedItemsToString).Replace(",", "");

            if (string.IsNullOrWhiteSpace(gradeSelect)) return;

            dgProductionDefect.Selection.Clear();
            int GradeColumn = _defectGradeCount + dgProductionDefect.Columns["GRADE1"].Index;

            for (int col = dgProductionDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
            {
                if (gradeSelect.IndexOf(Util.NVC(dgProductionDefect.Columns[col].Header)) < 0)
                {
                    dgProductionDefect.Columns[col].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgProductionDefect.Columns[col].Visibility = Visibility.Visible; ;
                }
            }
        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void SetGradeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _EQSGID;
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            mcb.Check(i);
                        }
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region [Func]
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

  

        #region [Clear]
        private void ClearValue()
        {
            //원각/초소형
            txtSelectLot.Text = string.Empty;
            txtWorkorder.Text = string.Empty;
            txtShift.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtWorker.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtOutQty.Value = 0;
            txtDefectQty.Value = 0;
            txtNote.Text = string.Empty;
            txtLot_PR.Text = string.Empty;
            txtSoc.Text = string.Empty;
            cboLotType.SelectedIndex = 0;
            cboWorkType.SelectedIndex = 0;
            cboEquipment_MC.SelectedIndex = 0;
            //폴리머
            txtSelectLot_PY.Text = string.Empty;
            txtWorkorder_PY.Text = string.Empty;
            txtShift_PY.Text = string.Empty;
            txtStartTime_PY.Text = string.Empty;
            txtWorker_PY.Text = string.Empty;
            txtEndTime_PY.Text = string.Empty;
            txtOutQty_PY.Value = 0;
            txtDefectQty_PY.Value = 0;
            txtNote_PY.Text = string.Empty;
            txtLot_PR_PY.Text = string.Empty;
            cboWorkType_PY.SelectedIndex = 0;
            txtLossQty_PY.Value = 0;
            txtOrderQty_PY.Value = 0;
            cboLotType_PY.SelectedIndex = 0;
            cboEquipment_PY.SelectedIndex = 0;




            _dtLengthExceeed.Clear();
            _AREAID = string.Empty;
            _PROCID = string.Empty;
            _PRODID = string.Empty;
            _EQSGID = string.Empty;
            _EQPTID = string.Empty;
            _EQPTNAME = string.Empty;
            _LOTID = string.Empty;
            _WIPSEQ = string.Empty;
            _WIP_NOTE = string.Empty;
            _LOTID_RT = string.Empty;
            _SOC_VALUE = string.Empty;
            _LOTDTTM_CR = string.Empty;
            _LINE_GROUP_CODE = string.Empty;
            _MKT_TYPE_CODE = string.Empty;
            _LOTTYPE = string.Empty;
            _SHIFT = string.Empty;
            _WRK_USERID = string.Empty;
            _WRK_USER_NAME = string.Empty;

            _PROCNAME = string.Empty;
            _MODLID = string.Empty;
            _PRJT_NAME = string.Empty;
            _MKT_TYPE_NAME = string.Empty;
            _CARDATE = string.Empty;
            _SHFT_NAME = string.Empty;
            _EQPTSHORTNAME = string.Empty;
            _CTNR_TYPE_CODE = string.Empty;
            _MIX_LOT_FLAG = string.Empty;
            _INSP_PROC_CHECK = string.Empty;
            _WIPSTAT = string.Empty;
            _LOT_OUTPUT_FLAG = string.Empty;
            _LOT_INPUT_FLAG = string.Empty;
            _OUTPUT_LOT_GNRT_FLAG = string.Empty;

            Util.gridClear(dgProductionPallet);
            Util.gridClear(dgInputPallet);
            Util.gridClear(dgDefect);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgPalletSummary);
            Util.gridClear(dgQuality);

            Util.gridClear(dgProductionInbox);
            Util.gridClear(dgInputInbox);
            Util.gridClear(dgProductionDefect);
                        
            Util.gridClear(dgProductionInbox_Out);

            btnSave.IsEnabled = true;
            txtOutQty.IsEnabled = false;
            btnSave_Result.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region [Tab 셋팅]
        private void TabSetting(string sLINE_GROUP_CODE, string sProcess)
        {
            if (sLINE_GROUP_CODE == "MCS") // 초소형
            {
                if (sProcess == Process.SmallGrader) // 초소형 Grader
                {
                    cTabProductionPallet.Visibility = Visibility.Visible; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Collapsed; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Visible; //불량정보
                    cTabMaterial.Visibility = Visibility.Collapsed; //자재투입
                    cTabPalletSummary.Visibility = Visibility.Visible; //등급별 수량집계
                    cTabQuality.Visibility = Visibility.Collapsed; //자주검사
                    cTabInbox.Visibility = Visibility.Collapsed; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Collapsed; //투입Inbox 
                    cTabDefect_F.Visibility = Visibility.Collapsed; //불량 폴리머  
                    cTabOutPallet.Visibility = Visibility.Collapsed; //검사공정 완성Inbox
                    cTabProductionPallet.IsSelected = true;
                }
                else if (sProcess == Process.SmallExternalTab) // 외부탭
                {
                    cTabProductionPallet.Visibility = Visibility.Visible; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Visible; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Visible; //불량정보
                    cTabMaterial.Visibility = Visibility.Visible; //자재투입
                    cTabPalletSummary.Visibility = Visibility.Visible; //등급별 수량집계
                    cTabQuality.Visibility = Visibility.Visible; //자주검사
                    cTabInbox.Visibility = Visibility.Collapsed; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Collapsed; //투입Inbox 
                    cTabDefect_F.Visibility = Visibility.Collapsed; //불량 폴리머 
                    cTabOutPallet.Visibility = Visibility.Collapsed; //검사공정 완성Inbox 
                    cTabProductionPallet.IsSelected = true;

                }
                else
                {
                    cTabProductionPallet.Visibility = Visibility.Visible; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Visible; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Visible; //불량정보
                    cTabMaterial.Visibility = Visibility.Collapsed; //자재투입
                    cTabPalletSummary.Visibility = Visibility.Visible; //등급별 수량집계
                    cTabQuality.Visibility = Visibility.Collapsed; //자주검사
                    cTabInbox.Visibility = Visibility.Collapsed; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Collapsed; //투입Inbox 
                    cTabDefect_F.Visibility = Visibility.Collapsed; //불량 폴리머 
                    cTabOutPallet.Visibility = Visibility.Collapsed; //검사공정 완성Inbox 
                    cTabProductionPallet.IsSelected = true;
                }

            }
            else if (sLINE_GROUP_CODE == "MCC" || sLINE_GROUP_CODE == "MCR" || sLINE_GROUP_CODE == "MCM") // 원형/각형
            {
                if (sProcess == Process.CircularGrader) //원형 Grader
                {
                    cTabProductionPallet.Visibility = Visibility.Visible; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Collapsed; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Collapsed; //불량정보
                    cTabMaterial.Visibility = Visibility.Collapsed; //자재투입
                    cTabPalletSummary.Visibility = Visibility.Visible; //등급별 수량집계
                    cTabQuality.Visibility = Visibility.Collapsed; //자주검사
                    cTabInbox.Visibility = Visibility.Collapsed; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Collapsed; //투입Inbox 
                    cTabDefect_F.Visibility = Visibility.Collapsed; //불량 폴리머  
                    cTabOutPallet.Visibility = Visibility.Collapsed; //검사공정 완성Inbox
                    cTabProductionPallet.IsSelected = true;
                }
                else  //원각형특성 Grader
                {
                    cTabProductionPallet.Visibility = Visibility.Visible; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Visible; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Visible; //불량정보
                    cTabMaterial.Visibility = Visibility.Collapsed; //자재투입
                    cTabPalletSummary.Visibility = Visibility.Visible; //등급별 수량집계
                    cTabQuality.Visibility = Visibility.Collapsed; //자주검사
                    cTabInbox.Visibility = Visibility.Collapsed; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Collapsed; //투입Inbox 
                    cTabDefect_F.Visibility = Visibility.Collapsed; //불량 폴리머  
                    cTabOutPallet.Visibility = Visibility.Collapsed; //검사공정 완성Inbox
                    cTabProductionPallet.IsSelected = true;
                }
            }
            else
            {

                // 외관검사 공정
                if (FINALEXTERNAL_PROCESS(sProcess) != string.Empty)
                {

                    cTabOutPallet.Visibility = Visibility.Visible; //검사공정 완성Inbox
                    cTabDefect_F.Visibility = Visibility.Visible; //불량 폴리머  
                    cTabQuality.Visibility = Visibility.Collapsed; //자주검사
                    cTabProductionPallet.Visibility = Visibility.Collapsed; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Collapsed; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Collapsed; //불량정보
                    cTabPalletSummary.Visibility = Visibility.Collapsed; //등급별 수량집계
                    cTabInbox.Visibility = Visibility.Collapsed; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Collapsed; //투입Inbox 
                    cTabMaterial.Visibility = Visibility.Collapsed; //자재투입
                    cTabOutPallet.IsSelected = true;
                }
                else
                {
                    cTabInbox.Visibility = Visibility.Visible; //완성Inbox
                    cTabInput_Inbox.Visibility = Visibility.Visible; //투입Inbox 
                    cTabDefect_F.Visibility = Visibility.Visible; //불량 폴리머  
                    //DSF, TCO, TAPING ,SIDE TAPING 
                    if (sProcess == "F7000" || sProcess == "F7100" || sProcess == "F7200" || sProcess == "F7300" || sProcess == string.Empty)
                    {
                        cTabMaterial.Visibility = Visibility.Visible; //자재투입
                    }
                    else
                    {
                        cTabMaterial.Visibility = Visibility.Collapsed; //자재투입
                    }
                       

                    cTabQuality.Visibility = Visibility.Collapsed; //자주검사
                    cTabProductionPallet.Visibility = Visibility.Collapsed; //팔레트완성
                    cTabInputPallet.Visibility = Visibility.Collapsed; // 팔레트투입
                    cTabDefect.Visibility = Visibility.Collapsed; //불량정보
                    cTabPalletSummary.Visibility = Visibility.Collapsed; //등급별 수량집계
                    cTabOutPallet.Visibility = Visibility.Collapsed; //검사공정 완성Inbox
                    cTabInbox.IsSelected = true;
                }
            }
        }
        #endregion

        #region [조회 데이터 바인딩]
        private void SetValue(object oContext)
        {

            //--------------------------------------원각/초소형----------------------------------
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { Util.NVC(DataTableConverter.GetValue(oContext, "AREAID")) };
            _combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);
            SetEquipment(cboEquipment_MC, "SELECT", string.Empty, Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")), Util.NVC(DataTableConverter.GetValue(oContext, "AREAID")));

            txtSelectLot.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID")); ;
            txtWorkorder.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WOID"));
            dtpCaldate.Text = Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE"));
            dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE")));
            dtCaldate = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE")));
            txtShift.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SHFT_NAME"));
            txtShift.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFT"));
            txtStartTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ST"));
            txtWorker.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USER_NAME"));
            txtWorker.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USERID"));
            txtEndTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ED"));
            txtOutQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "GOOD_QTY").ToString()));
            txtDefectQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "DEFECT_QTY").ToString()));
            txtProductQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "PRODUCT_QTY").ToString()));
            txtLot_PR.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID_RT"));
            txtSoc.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SOC_VALUE"));
            cboLotType.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "LOTTYPE"));
            cboWorkType.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "FORM_WRK_TYPE_CODE"));
            cboEquipment_MC.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            //----------------------------------------- 폴리머---------------------------------
            //설비
            SetEquipment(cboEquipment_PY, "SELECT", string.Empty, Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")), Util.NVC(DataTableConverter.GetValue(oContext, "AREAID")));


            //작업유형
            _combo.SetCombo(cboWorkType_PY, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);

            if (Util.NVC(DataTableConverter.GetValue(oContext, "CTNR_TYPE_CODE")) != "T")
            {
                SetFormWorkType(cboWorkType_PY, "SELECT", Util.NVC(DataTableConverter.GetValue(oContext, "AREAID")), Util.NVC(DataTableConverter.GetValue(oContext, "FORM_WRK_TYPE_CODE")));

            }
            else
            {
                _combo.SetCombo(cboWorkType_PY, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);
            }
            //자재위치
            SetLocation(cboLocation, Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID")));

            txtSelectLot_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID")); ;
            txtWorkorder_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WOID"));
            dtpCaldate_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE"));
            dtpCaldate_PY.SelectedDateTime = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE")));
            txtShift_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SHFT_NAME"));
            txtShift_PY.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFT"));
            txtStartTime_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ST"));
            txtWorker_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USER_NAME"));
            txtWorker_PY.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USERID"));
            txtEndTime_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPDTTM_ED"));
            txtOutQty_PY.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "GOOD_QTY").ToString()));
            txtDefectQty_PY.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "DEFECT_QTY").ToString()));
            txtProductQty_PY.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "PRODUCT_QTY").ToString()));
            txtLot_PR_PY.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID_RT"));
            cboWorkType_PY.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "FORM_WRK_TYPE_CODE"));
            txtLossQty_PY.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_LOSS_QTY").ToString()));
            txtOrderQty_PY.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_PRDT_REQ_QTY").ToString()));
            cboLotType_PY.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "LOTTYPE"));
            cboEquipment_PY.SelectedValue = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));




            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _LOTID_RT = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID_RT"));
            _SOC_VALUE = Util.NVC(DataTableConverter.GetValue(oContext, "SOC_VALUE"));
            _LOTDTTM_CR = Util.NVC(DataTableConverter.GetValue(oContext, "LOTDTTM_CR"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _PRODID = Util.NVC(DataTableConverter.GetValue(oContext, "PRODID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _EQPTNAME = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTNAME"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _WIP_NOTE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));
            _LINE_GROUP_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "LINE_GROUP_CODE"));
            _MKT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "MKT_TYPE_CODE"));
            _LOTTYPE = Util.NVC(DataTableConverter.GetValue(oContext, "LOTTYPE"));
            _SHIFT = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFT"));
            _WRK_USERID = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USERID"));
            _WRK_USER_NAME = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USER_NAME"));
            _PROCNAME = Util.NVC(DataTableConverter.GetValue(oContext, "PROCNAME"));
            _MODLID = Util.NVC(DataTableConverter.GetValue(oContext, "MODLID"));
            _PRJT_NAME = Util.NVC(DataTableConverter.GetValue(oContext, "PRJT_NAME"));
            _MKT_TYPE_NAME = Util.NVC(DataTableConverter.GetValue(oContext, "MKT_TYPE_NAME"));
            _CARDATE = Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE"));
            _SHFT_NAME = Util.NVC(DataTableConverter.GetValue(oContext, "SHFT_NAME"));
            _EQPTSHORTNAME = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTSHORTNAME"));
            _CTNR_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(oContext, "CTNR_TYPE_CODE"));
            _MIX_LOT_FLAG = Util.NVC(DataTableConverter.GetValue(oContext, "MIX_LOT_FLAG"));
            _INSP_PROC_CHECK = Util.NVC(DataTableConverter.GetValue(oContext, "INSP_PROC_CHECK"));
            _WIPSTAT = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSTAT"));
            _LOT_OUTPUT_FLAG = Util.NVC(DataTableConverter.GetValue(oContext, "LOT_OUTPUT_FLAG"));
            _LOT_INPUT_FLAG = Util.NVC(DataTableConverter.GetValue(oContext, "LOT_INPUT_FLAG"));
            _OUTPUT_LOT_GNRT_FLAG = Util.NVC(DataTableConverter.GetValue(oContext, "OUTPUT_LOT_GNRT_FLAG"));
            txtNote.Text = GetWipNote();
            txtNote_PY.Text = GetWipNote();

            //if (_CTNR_TYPE_CODE == "T")
            //{
            //    txtOutQty_PY.IsEnabled = true;
            //    cboWorkType_PY.IsEnabled = true;
            //}
            //else
            //{
            //    txtOutQty_PY.IsEnabled = false;

            //}
            //생산LOT의 LOT의 완성 여부가 N 일경우 조립LOT 수정가능함
            if (_LOT_OUTPUT_FLAG == "N")
            {
                txtLot_PR_PY.IsEnabled = true;
                txtOutQty_PY.IsEnabled = true;
            }
            else
            {
                txtLot_PR_PY.IsEnabled = false;
                txtOutQty_PY.IsEnabled = false;
            }
            //생산 LOT의 LOT 투입 여부 = "N" 로 작업유형 여부를 수정가능함
            if (_LOT_INPUT_FLAG == "N")
            {
                cboWorkType_PY.IsEnabled = true;
            }
            else
            {
                cboWorkType_PY.IsEnabled = false;
            }

            //INBOX 추가 : 공정의 OUT 생성 여부 = Y이고 생산 LOT의 LOT 완성 여부 = Y인 경우만 처리 가능하도록 로직 추가
            if (_OUTPUT_LOT_GNRT_FLAG == "Y" && _LOT_OUTPUT_FLAG == "Y")
            {

                btnInboxCreate.IsEnabled = true;
            }
            else
            {
                btnInboxCreate.IsEnabled = false;
            }

            // -생산 LOT의 LOT 투입 여부로 투입이 없는 경우는 처리될 수 없도록 변경
            // (공정의 OUT 생성 여부 = Y이고 LOT의 LOT 투입 여부 = Y인 경우)
            if (_OUTPUT_LOT_GNRT_FLAG == "Y" && _LOT_INPUT_FLAG == "Y")
            {

                btnPalletInput.IsEnabled = true;
            }
            else
            {
                btnPalletInput.IsEnabled = false;
            }

            // 원,각형 : 원각형 특성/Grading 공정진척 수정시 Inbox 등록 팝업 버튼 활성화
            if (_PROCID.Equals(Process.CircularCharacteristicGrader))
            {
                btnInbox.Visibility = Visibility.Visible;
            }
            else
            {
                btnInbox.Visibility = Visibility.Collapsed;
            }

        }
        private void SetValue_Update(DataTable dt)
        {
            //원각/초소형
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { dt.Rows[0]["AREAID"].ToString() };
            _combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);
            //설비
            SetEquipment(cboEquipment_MC, "SELECT", string.Empty, dt.Rows[0]["PROCID"].ToString(), dt.Rows[0]["AREAID"].ToString());
            txtSelectLot.Text = dt.Rows[0]["LOTID"].ToString();       
            txtWorkorder.Text = dt.Rows[0]["WOID"].ToString();  
            dtpCaldate.Text = dt.Rows[0]["CALDATE"].ToString(); 
            dtpCaldate.SelectedDateTime = Convert.ToDateTime(dt.Rows[0]["CALDATE"].ToString());  
            dtCaldate = Convert.ToDateTime(dt.Rows[0]["CALDATE"].ToString()); 
            txtShift.Text = dt.Rows[0]["SHFT_NAME"].ToString();  
            txtShift.Tag = dt.Rows[0]["SHIFT"].ToString();
            txtStartTime.Text = dt.Rows[0]["WIPDTTM_ST"].ToString(); 
            txtWorker.Text = dt.Rows[0]["WRK_USER_NAME"].ToString();  
            txtWorker.Tag = dt.Rows[0]["WRK_USERID"].ToString();  
            txtEndTime.Text = dt.Rows[0]["WIPDTTM_ED"].ToString();  
            txtOutQty.Value = Double.Parse(dt.Rows[0]["GOOD_QTY"].ToString());  
            txtDefectQty.Value = Double.Parse(dt.Rows[0]["DEFECT_QTY"].ToString()); 
            txtProductQty.Value = Double.Parse(dt.Rows[0]["PRODUCT_QTY"].ToString());
            txtLot_PR.Text = dt.Rows[0]["LOTID_RT"].ToString();
            txtSoc.Text = dt.Rows[0]["SOC_VALUE"].ToString();
            cboLotType.SelectedValue = dt.Rows[0]["LOTTYPE"].ToString();
            cboWorkType.SelectedValue = dt.Rows[0]["FORM_WRK_TYPE_CODE"].ToString();
            cboEquipment_MC.SelectedValue = dt.Rows[0]["EQPTID"].ToString();
            //폴리머

            //설비
            SetEquipment(cboEquipment_PY, "SELECT", string.Empty, dt.Rows[0]["PROCID"].ToString(), dt.Rows[0]["AREAID"].ToString());
          
            _combo.SetCombo(cboWorkType_PY, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);
            //작업유형
            if (dt.Rows[0]["CTNR_TYPE_CODE"].ToString() != "T")
            {
                SetFormWorkType(cboWorkType_PY, "SELECT", dt.Rows[0]["AREAID"].ToString(), dt.Rows[0]["FORM_WRK_TYPE_CODE"].ToString());

            }
            else
            {
                _combo.SetCombo(cboWorkType_PY, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE", sFilter: sFilter);
            }

            //자재위치
            SetLocation(cboLocation, dt.Rows[0]["EQPTID"].ToString());

            txtSelectLot_PY.Text = dt.Rows[0]["LOTID"].ToString();
            txtWorkorder_PY.Text = dt.Rows[0]["WOID"].ToString();
            dtpCaldate_PY.Text = dt.Rows[0]["CALDATE"].ToString();
            dtpCaldate_PY.SelectedDateTime = Convert.ToDateTime(dt.Rows[0]["CALDATE"].ToString());
            txtShift_PY.Text = dt.Rows[0]["SHFT_NAME"].ToString();
            txtShift_PY.Tag = dt.Rows[0]["SHIFT"].ToString();
            txtStartTime_PY.Text = dt.Rows[0]["WIPDTTM_ST"].ToString();
            txtWorker_PY.Text = dt.Rows[0]["WRK_USER_NAME"].ToString();
            txtWorker_PY.Tag = dt.Rows[0]["WRK_USERID"].ToString();
            txtEndTime_PY.Text = dt.Rows[0]["WIPDTTM_ED"].ToString();
            txtOutQty_PY.Value = Double.Parse(dt.Rows[0]["GOOD_QTY"].ToString());
            txtDefectQty_PY.Value = Double.Parse(dt.Rows[0]["DEFECT_QTY"].ToString());
            txtProductQty_PY.Value = Double.Parse(dt.Rows[0]["PRODUCT_QTY"].ToString());
            txtLot_PR_PY.Text = dt.Rows[0]["LOTID_RT"].ToString();
            cboWorkType_PY.SelectedValue = dt.Rows[0]["FORM_WRK_TYPE_CODE"].ToString();
            txtLossQty_PY.Value = Double.Parse(dt.Rows[0]["CNFM_LOSS_QTY"].ToString());
            txtOrderQty_PY.Value = Double.Parse(dt.Rows[0]["CNFM_PRDT_REQ_QTY"].ToString());
            cboLotType_PY.SelectedValue = dt.Rows[0]["LOTTYPE"].ToString(); 
            cboEquipment_PY.SelectedValue = dt.Rows[0]["EQPTID"].ToString();  

            _WIPSEQ = dt.Rows[0]["WIPSEQ"].ToString();  
            _LOTID_RT = dt.Rows[0]["LOTID_RT"].ToString();  
            _SOC_VALUE = dt.Rows[0]["SOC_VALUE"].ToString();  
            _LOTDTTM_CR = dt.Rows[0]["LOTDTTM_CR"].ToString();  
            _EQSGID = dt.Rows[0]["EQSGID"].ToString(); 
            _PROCID = dt.Rows[0]["PROCID"].ToString();  
            _PRODID = dt.Rows[0]["PRODID"].ToString();  
            _EQPTID = dt.Rows[0]["EQPTID"].ToString(); 
            _EQPTNAME = dt.Rows[0]["EQPTNAME"].ToString();
            _LOTID = dt.Rows[0]["LOTID"].ToString();
            _AREAID = dt.Rows[0]["AREAID"].ToString();
            _WIP_NOTE = dt.Rows[0]["WIP_NOTE"].ToString();
            _LINE_GROUP_CODE = dt.Rows[0]["LINE_GROUP_CODE"].ToString();
            _MKT_TYPE_CODE = dt.Rows[0]["MKT_TYPE_CODE"].ToString(); 
            _LOTTYPE = dt.Rows[0]["LOTTYPE"].ToString();
            _SHIFT = dt.Rows[0]["SHIFT"].ToString();
            _WRK_USERID = dt.Rows[0]["WRK_USERID"].ToString();
            _WRK_USER_NAME = dt.Rows[0]["WRK_USER_NAME"].ToString();
            _PROCNAME = dt.Rows[0]["PROCNAME"].ToString();
            _MODLID = dt.Rows[0]["MODLID"].ToString();
            _PRJT_NAME = dt.Rows[0]["PRJT_NAME"].ToString();
            _MKT_TYPE_NAME = dt.Rows[0]["MKT_TYPE_NAME"].ToString();
            _CARDATE = dt.Rows[0]["CALDATE"].ToString();
            _SHFT_NAME = dt.Rows[0]["SHFT_NAME"].ToString();
            _EQPTSHORTNAME = dt.Rows[0]["EQPTSHORTNAME"].ToString();
            _CTNR_TYPE_CODE = dt.Rows[0]["CTNR_TYPE_CODE"].ToString();
            _MIX_LOT_FLAG = dt.Rows[0]["MIX_LOT_FLAG"].ToString();
            _INSP_PROC_CHECK = dt.Rows[0]["INSP_PROC_CHECK"].ToString();
            _WIPSTAT = dt.Rows[0]["WIPSTAT"].ToString();
            _LOT_OUTPUT_FLAG = dt.Rows[0]["LOT_OUTPUT_FLAG"].ToString();
            _LOT_INPUT_FLAG = dt.Rows[0]["LOT_INPUT_FLAG"].ToString();
            _OUTPUT_LOT_GNRT_FLAG = dt.Rows[0]["OUTPUT_LOT_GNRT_FLAG"].ToString();

            txtNote.Text = GetWipNote();
            txtNote_PY.Text = GetWipNote();

            //if (_CTNR_TYPE_CODE == "T")
            //{
            //    txtOutQty_PY.IsEnabled = true;
            //    cboWorkType_PY.IsEnabled = true;
            //}
            //else
            //{
            //    txtOutQty_PY.IsEnabled = false;
            
            //}

            //생산LOT의 LOT의 완성 여부가 N 일경우 조립LOT 수정가능함
            if (_LOT_OUTPUT_FLAG == "N")
            {
                txtLot_PR_PY.IsEnabled = true;
                txtOutQty_PY.IsEnabled = true;
            }
            else
            {
                txtLot_PR_PY.IsEnabled = false;
                txtOutQty_PY.IsEnabled = false;
            }

            //생산 LOT의 LOT 투입 여부 = "N" 로 작업유형 여부를 수정가능함
            if (_LOT_INPUT_FLAG == "N")
            {
                cboWorkType_PY.IsEnabled = true;
            }
            else
            {
                cboWorkType_PY.IsEnabled = false;
            }
           
            
            //INBOX 추가 : 공정의 OUT 생성 여부 = Y이고 생산 LOT의 LOT 완성 여부 = Y인 경우만 처리 가능하도록 로직 추가
            if (_OUTPUT_LOT_GNRT_FLAG == "Y" && _LOT_OUTPUT_FLAG == "Y")
            {
               
                btnInboxCreate.IsEnabled = true;
            }
            else
            {
                 btnInboxCreate.IsEnabled = false;
            }

            // -생산 LOT의 LOT 투입 여부로 투입이 없는 경우는 처리될 수 없도록 변경
            // (공정의 OUT 생성 여부 = Y이고 LOT의 LOT 투입 여부 = Y인 경우)
            if (_OUTPUT_LOT_GNRT_FLAG == "Y" && _LOT_INPUT_FLAG == "Y")
            {

                btnPalletInput.IsEnabled = true;
            }
            else
            {
                btnPalletInput.IsEnabled = false;
            }

        }



        private void SetFormWorkType(C1ComboBox cbo, string GUBUN, string Areaid, string ComCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("RT_Y", typeof(string));
                RQSTDT.Columns.Add("RT_N", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Areaid;
                if(ComCode == "FORM_WORK_RT")
                {
                    dr["RT_Y"] = "Y";
                }
                else
                {
                    dr["RT_N"] = "Y";
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WRK_TYPE_CODE_CONFIRM", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, GUBUN, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetLocation(C1ComboBox cbo, string Eqptid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
           
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Eqptid;
              
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_MTRL_LOCATION", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, "SELECT", "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [저장 Biz DataSet 생성]
        private DataSet GetSaveDataSet()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataTable.Columns.Add("CALDATE", typeof(string));
            inDataTable.Columns.Add("WIPDTTM_ED", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("WIPQTY_ED", typeof(Decimal));
            inDataTable.Columns.Add("WIPQTY2_ED", typeof(Decimal));
            inDataTable.Columns.Add("LOSS_QTY", typeof(Decimal));
            inDataTable.Columns.Add("LOSS_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("DFCT_QTY", typeof(Decimal));
            inDataTable.Columns.Add("DFCT_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("PRDT_REQ_QTY", typeof(Decimal));
            inDataTable.Columns.Add("PRDT_REQ_QTY2", typeof(Decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(Int16));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("REQ_USERID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("CHANGE_WIPQTY_FLAG", typeof(string));
            inDataTable.Columns.Add("INPUT_DIFF_QTY", typeof(Decimal));
            inDataTable.Columns.Add("FORCE_FLAG", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("WIPQTY_ED_DIFF", typeof(Decimal));
            inDataTable.Columns.Add("WIPQTY2_ED_DIFF", typeof(Decimal));

            // IN_LOSS
            DataTable inDataLoss = indataSet.Tables.Add("IN_LOSS");
            inDataLoss.Columns.Add("LOTID", typeof(string));
            inDataLoss.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataLoss.Columns.Add("RESNCODE", typeof(string));
            inDataLoss.Columns.Add("RESNQTY", typeof(Decimal));
            inDataLoss.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataLoss.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataLoss.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataLoss.Columns.Add("RESNNOTE", typeof(string));

            // IN_DFCT
            DataTable inDataDfct = indataSet.Tables.Add("IN_DFCT");
            inDataDfct.Columns.Add("LOTID", typeof(string));
            inDataDfct.Columns.Add("WIPSEQ", typeof(Decimal));
            inDataDfct.Columns.Add("RESNCODE", typeof(string));
            inDataDfct.Columns.Add("RESNQTY", typeof(Decimal));
            inDataDfct.Columns.Add("RESNQTY2", typeof(Decimal));
            inDataDfct.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataDfct.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataDfct.Columns.Add("RESNNOTE", typeof(string));
            inDataDfct.Columns.Add("DFCT_TAG_QTY", typeof(string));
            inDataDfct.Columns.Add("A_TYPE_DFCT_QTY", typeof(string));
            inDataDfct.Columns.Add("C_TYPE_DFCT_QTY", typeof(string));

            // IN_PRDT_REQ
            DataTable inDataPrdtReq = indataSet.Tables.Add("IN_PRDT_REQ");
            inDataPrdtReq.Columns.Add("LOTID", typeof(string));
            inDataPrdtReq.Columns.Add("WIPSEQ", typeof(string));
            inDataPrdtReq.Columns.Add("RESNCODE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY", typeof(string));
            inDataPrdtReq.Columns.Add("RESNQTY2", typeof(string));
            inDataPrdtReq.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("PROCID_CAUSE", typeof(string));
            inDataPrdtReq.Columns.Add("RESNNOTE", typeof(string));
            inDataPrdtReq.Columns.Add("COST_CNTR_ID", typeof(string));

            return indataSet;
        }




        #endregion
             
        #region [그리드내 콤보박스 셋팅]
        private void SetGridCombo()
        {
            //팔레트 완성 용량등급
            C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn_CAPA_GRD_CODE = dgProductionPallet.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;

            //용량등급
            if (portTypeColumn_CAPA_GRD_CODE != null)
            {
                DataTable dtCapa = dtCommonCode("CAPA_GRD_CODE");
                DataRow drCapa = dtCapa.NewRow();
                drCapa["CBO_CODE"] = "";
                drCapa["CBO_NAME"] = "";
                dtCapa.Rows.InsertAt(drCapa, 0);
                portTypeColumn_CAPA_GRD_CODE.ItemsSource = DataTableConverter.Convert(dtCapa);
            }


            //팔레트 완성 전압등급
            C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn_VLTG_GRD_CODE = dgProductionPallet.Columns["VLTG_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;

            //전압등급
            if (portTypeColumn_VLTG_GRD_CODE != null)
            {
                DataTable dtVltg = dtCommonCode("VLTG_GRD_CODE");
                DataRow drVltg = dtVltg.NewRow();
                drVltg["CBO_CODE"] = "";
                drVltg["CBO_NAME"] = "";
                dtVltg.Rows.InsertAt(drVltg, 0);
                portTypeColumn_VLTG_GRD_CODE.ItemsSource = DataTableConverter.Convert(dtVltg);
            }


            //팔레트 완성 저항등급
            C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn_RSST_GRD_CODE = dgProductionPallet.Columns["RSST_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;

            //전압등급
            if (portTypeColumn_RSST_GRD_CODE != null)
            {
                DataTable dtRsst = dtCommonCode("RSST_GRD_CODE");
                DataRow drRsst = dtRsst.NewRow();
                drRsst["CBO_CODE"] = "";
                drRsst["CBO_NAME"] = "";
                dtRsst.Rows.InsertAt(drRsst, 0);
                portTypeColumn_RSST_GRD_CODE.ItemsSource = DataTableConverter.Convert(dtRsst);
            }

            // 뷸량 등록시 대상등급선택 콤보 2018-04-05
            cboGradeSelect.SelectionChanged -= cboGradeSelect_SelectionChanged;
            cboGradeSelect.ApplyTemplate();
            SetGradeCombo(cboGradeSelect);
            cboGradeSelect.SelectionChanged += cboGradeSelect_SelectionChanged;
        }

        DataTable dtCommonCode(string CmcdType)
        {

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("ATTR1", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["AREAID"] = _AREAID;
            newRow["COM_TYPE_CODE"] = CmcdType;
            newRow["ATTR1"] = null;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_CBO", "INDATA", "OUTDATA", inTable);
            return dtResult;
        }
        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private string GetWipNote()
        {
            string sReturn = string.Empty;
            string[] sWipNote = _WIP_NOTE.Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = _WIP_NOTE;
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }

        private string SetWipNote()
        {
            string sReturn = string.Empty;
            string[] sWipNote = _WIP_NOTE.Split('|');

            sReturn = txtNote.Text + "|";

            for (int nlen = 1; nlen < sWipNote.Length; nlen++)
            {
                sReturn += sWipNote[nlen] + "|";
            }

            return sReturn.Substring(0, sReturn.Length-1) ;
        }


        public void SetControlHeader()
        {
            if (string.Equals(_PROCID, Process.SmallOcv) || string.Equals(_PROCID, Process.SmallLeak) || string.Equals(_PROCID, Process.SmallDoubleTab))
            {
                // 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭
               dgPalletSummary.Columns["PALLET_QTY"].Header = ObjectDic.Instance.GetObjectName("대차 수량");
            }
            else
            {
               dgPalletSummary.Columns["PALLET_QTY"].Header = ObjectDic.Instance.GetObjectName("Pallet 수량");
            }

        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }


        private string FINALEXTERNAL_PROCESS(string procid)
        {
            string ReturnValue = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
              

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_INSP_PROCID";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataRow[] drprocess = dtRslt.Select("CBO_CODE ='" + procid + "'");
                    if (drprocess.Length > 0)
                    {
                        ReturnValue = drprocess[0]["CBO_CODE"].ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


            return ReturnValue;

        }


        #endregion

        #endregion

        private void btnAssyLot_Click(object sender, RoutedEventArgs e)
        {
            AssyLotPopUp();
        }

        private void AssyLotPopUp()
        {
            //if (txtAssyLotID.Text.Length < 4)
            //{
            //    // Lot ID는 4자리 이상 넣어 주세요.
            //    Util.MessageValidation("SFU3450");
            //    return;
            //}

            //FORM001_ASSYLOT popupAssyLot = new FORM001_ASSYLOT { FrameOperation = this.FrameOperation };

            //C1WindowExtension.SetParameters(popupAssyLot, null);
            //popupAssyLot.AssyLot = txtAssyLotID.Text;
            //popupAssyLot.PolymerYN = "Y";

            //popupAssyLot.Closed += new EventHandler(popupAssyLot_Closed);

            //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //{
            //    if (tmp.Name == "grdMain")
            //    {
            //        tmp.Children.Add(popupAssyLot);
            //        popupAssyLot.BringToFront();
            //        break;
            //    }
            //}
        }
        private void popupAssyLot_Closed(object sender, EventArgs e)
        {
            // FORM001_ASSYLOT popup = sender as FORM001_ASSYLOT;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            //    txtAssyLotID.Text = popup.AssyLot;
               
            //}

            //foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //{
            //    if (tmp.Name == "grdMain")
            //    {
            //        tmp.Children.Remove(popup);
            //        break;
            //    }
            //}

            //txtAssyLotID.Focus();
        }

    }
}
