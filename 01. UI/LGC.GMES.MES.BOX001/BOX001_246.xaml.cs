/*************************************************************************************
 Created Date : 2020.07.15
      Creator : 
   Decription : Tag 매핑 이력 조회 (자동차)
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.15  이제섭 : Initial Created.
  2022.08.01  이정미 : sStart_date, sEnd_date 수정 및 모델 LOT 콤보박스 수정 
  2023.05.24  최경아 : tag매핑 세팅 버튼 추가
  2023.06.15  권혜정 : BOX, TAG 모두 데이터 입력 시, 2개 데이터 모두 DA에 INPUT DATA로 보내도록 수정
  2023.07.20  김동훈 : TOP_PRODID 제품ID 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_246 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        string sRadioB = string.Empty;
        public BOX001_246()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetEvent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            CommonCombo combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.ALL, cbChild: new C1ComboBox[] { cboLine }, sCase: "ALLAREA");

            combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboAreaAll2 }, cbChild: new C1ComboBox[] { cboModelLot }, sFilter: sFilter, sCase: "LINE_FCS");

            combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine });

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        #endregion

        #region Event

        #region [DatePicker Changed 이벤트]

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        #endregion

        #region [Tag 매핑 이력조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            sRadioB = string.Empty;
            GetTagHist();
        }
        #endregion

        #region [매핑해제]
        private void btnUnMap_Click(object sender, RoutedEventArgs e)
        {
            UnMappingTag();
        }
        #endregion

        #region [RFID Key Down]
        private void txtTagID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetTagHist();
        }
        #endregion

        #region [PalletID Key Down]
        private void txtBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetTagHist();
        }
        #endregion

        #region [라디오버튼 체크 이벤트]
        private void dgTagHistChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            if (rb.IsChecked != null)
            {
                DataRowView drv = rb.DataContext as DataRowView;
                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    sRadioB = "CHK";
                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }

                    //row 색 바꾸기
                    dgTagHist.SelectedIndex = idx;
                }
            }
        }
        #endregion

        #region [매핑세팅]
        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxPallet = -1;
                idxPallet = dgTagHist.SelectedIndex;
                string sBoxID = string.Empty;

                RadioButton rb = sender as RadioButton;

                if (sRadioB == "CHK")
                {
                    sBoxID = Util.NVC(dgTagHist.GetCell(idxPallet, dgTagHist.Columns["BOXID"].Index).Value);
                }
                    
                BOX001_246_TAG_MAPPING popup = new BOX001_246_TAG_MAPPING();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = LoginInfo.CFG_AREA_ID;
                    Parameters[1] = txtWorker.Tag;
                    Parameters[2] = sBoxID;

                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.Closed += new EventHandler(puMap_Closed);
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void puMap_Closed(object sender, EventArgs e)
        {
            BOX001_246_TAG_MAPPING window = sender as BOX001_246_TAG_MAPPING;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetTagHist();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #endregion

        #region Mehod

        #region [Tag 매핑 이력 조회]
        private void GetTagHist()
        {
            try
            {

                loadingIndicator.Visibility = Visibility.Visible;

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";

                string sArea = !string.IsNullOrWhiteSpace(cboAreaAll2.SelectedValue.ToString()) ? cboAreaAll2.SelectedValue.ToString() : null;
                string sLine = !string.IsNullOrWhiteSpace(cboLine.SelectedValue.ToString()) ? cboLine.SelectedValue.ToString() : null;
                string sModelLot = !string.IsNullOrWhiteSpace(cboModelLot.SelectedValue.ToString()) ? cboModelLot.SelectedValue.ToString() : null;


                // 조회 비즈 생성
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("TAGID", typeof(String));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = sLine;
                dr["MDLLOT_ID"] = sModelLot;

                // BOX 또는 TAG 조건 입력한 경우 날짜 조건없이 조회
                if (!string.IsNullOrWhiteSpace(txtBoxID.Text.Trim()) || !string.IsNullOrWhiteSpace(txtTagID.Text.Trim()))
                {
                    if (!string.IsNullOrWhiteSpace(txtBoxID.Text.Trim()))
                    {
                        dr["BOXID"] = txtBoxID.Text.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(txtTagID.Text.Trim())) 
                    {
                        dr["TAGID"] = txtTagID.Text.Trim();
                    }
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                }

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_MAP_HISTORY_CP", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgTagHist);
                Util.GridSetData(dgTagHist, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region [매핑해제]
        private void UnMappingTag()
        {
            try
            {

                loadingIndicator.Visibility = Visibility.Visible;

                int idxPallet = -1;
                idxPallet = dgTagHist.SelectedIndex;

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtWorker.Tag as string))
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }
                
                // 해제 하시겠습니까?
                Util.MessageConfirm("SFU4946", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sBoxID = Util.NVC(dgTagHist.GetCell(idxPallet, dgTagHist.Columns["BOXID"].Index).Value);
                        string sTagID = Util.NVC(dgTagHist.GetCell(idxPallet, dgTagHist.Columns["TAGID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("USERID");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("TAGID");
                        RQSTDT.Columns.Add("SRCTYPE");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker.Tag as string;
                        newRow["BOXID"] = sBoxID;
                        newRow["TAGID"] = sTagID;
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_FORM_REG_UNMAPPING_CSTID_PLTID_UI", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetTagHist();
                                // 정상 처리되었습니다.
                                Util.MessageInfo("SFU1275");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                        });
                    }
                });
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

        #endregion

        #region PopUp

        #region [작업조 팝업 오픈]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        #endregion

        #region [작업조 팝업 닫기]
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }
        #endregion

        #endregion
    }
}
