/*************************************************************************************
 Created Date : 2021.02.01
      Creator : 조영대
   Decription : 반송지시현황
--------------------------------------------------------------------------------------
 [Change History]
  2021.02.01  조영대 : Initial Created. 
  2021.04.30  조영대 : TB_MHS_TRF_CMD_TRGT_CST Join 추가
  2021.09.09  김동일 : C20210831-000510
  2022.01.26  오화백 : 색상 콤보 및 색상에 따라 조회가 되도록 수정
  2023.09.07  김태우 : NFF 보빈ID, 캐리어ID 대신 대표랏(REP_LOT)사용 하는경우 적용
  2023.09.29  오화백 : HISTORY 탭에서도 색상 콤보 및 색상에 따라 조회가 되도록 수정
  2023.10.10  김태우 : NFF 보빈ID, 캐리어ID 추가 수정. BIZ수정 안하고 컬럼해더만 수정. objectdic 에서 가져오게 수정.
  2023.10.23  김태우 : NFF 황현규사원 요청, Carrier/대표Lot ID 헤더명 변경
  2024.06.10  박성진 : 그리드 정렬 후 캐리어 선택이 정상 CarrierID가 맵핑되도록 수정
  2024.12.06  배준호 : MES 리빌딩으로 DA_MHS_SEL_TRF_CMD_HISTORY_LIST -> BR_MHS_SEL_TRF_CMD_HISTORY_LIST 변경
  2024.12.24  최원석 : MES 리빌딩 dgStatus.SetItemsSource(result, FrameOperation, true)  세번째 파라미터 true 수정함 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Globalization;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_055 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private System.Windows.Threading.DispatcherTimer refreshTimer = null;
        private DataTable dtColor = null;      // MMD에 등록된 색상
        private string _ELAPSED_MIN = string.Empty;
        private string _MAX_ELAPSED = string.Empty;

        private readonly Util _util = new Util();

        public MCS001_055()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            ApplyPermissions();

            InitializeControls();
            InitializeCombo();
        }

        private void InitializeControls()
        {
        }

        private void InitializeCombo()
        {
            CommonCombo comboSet = new CommonCombo();
            String[] sFilter = { string.Empty };

            //동
            comboSet.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // 명령상태
            cboCmdStat.SetCommonCode("MHS_CMD_STAT_CODE", true);

            // 반송분류
            cboTrfClass.SetCommonCode("MHS_TRF_CLSS_CODE", true);

            // 조회 주기
            cboInquiryCycle.SetCommonCode("INTERVAL_MIN", CommonCombo.ComboStatus.NA, false);

            // 반송구간
            //cboTrfSection.SetCommonCode("MHS_TRF_SECTION", true);

            SetEqptCombo();

            

            // 기간구분
            cboTermType.SetCommonCode("MHS_PERIOD_TYPE");

            // 출발설비
            //setFromToEqpPopControl();

            //색상 범례
            SetLegendColorCombo();
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            Loaded -= UserControl_Loaded;
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();
            SetEqptCombo();
            RepLotUseForm();
            //setFromToEqpPopControl();
        }


        private void cboCmdStat_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        private void cboTrfClass_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        //private void cboTrfSection_SelectionChanged(object sender, EventArgs e)
        //{
        //    SetEqptCombo();

        //    ClearDataGrid();
        //}

        private void cboFromEqptId_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
            SetPortComboFrom();
        }

        private void cboToEqptId_SelectionChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
            SetPortComboTo();
        }

        private void cboTermType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();
        }

        private void cboInquiryCycle_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            TimerSetting();
        }
        
        private void btnOrderCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!dgStatus.IsCheckedRow("CHK"))
            {
                // 선택된 요청이 없습니다.
                Util.MessageValidation("SFU1654");
                return;
            }

            int selectRow = dgStatus.GetCheckedRowIndex("CHK").First();

            
            // 팝업 호출
            MCS001_055_CMD_CANCEL popup = new MCS001_055_CMD_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            //parameters[0] = dgStatus.GetDataRow(selectRow);
            parameters[0] = (dgStatus.Rows[selectRow].DataItem as DataRowView).Row;
            if (selectRow < dgStatus.Rows.Count - 1 &&
                Util.NVC(dgStatus.GetValue(selectRow, "CMD_SEQNO")).Equals(Util.NVC(dgStatus.GetValue(selectRow + 1, "CMD_SEQNO"))))
            {
                //parameters[1] = dgStatus.GetDataRow(selectRow + 1);
                parameters[1] = (dgStatus.Rows[selectRow + 1].DataItem as DataRowView).Row;
            }
            C1WindowExtension.SetParameters(popup, parameters);

            popup.Closed += popupCmdCancel_Closed;

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
        }

        private void popupCmdCancel_Closed(object sender, EventArgs e)
        {
            MCS001_055_CMD_CANCEL popup = sender as MCS001_055_CMD_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void dtpStart_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpStart.SelectedDateTime > dtpEnd.SelectedDateTime)
            {
                dtpEnd.SelectedDateTime = dtpStart.SelectedDateTime;
            }
            else
            {
                if (dtpStart.SelectedDateTime.AddDays(7) <= dtpEnd.SelectedDateTime)
                {
                    dtpEnd.SelectedDateTime = dtpStart.SelectedDateTime.AddDays(6);

                    //// 조회 기간은 7일을 초과할 수 없습니다.
                    //Util.MessageValidation("SFU3567");     
                }
            }
        }

        private void dtpEnd_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDateTime < dtpStart.SelectedDateTime)
            {
                dtpStart.SelectedDateTime = dtpEnd.SelectedDateTime;
            }
            else
            {
                if (dtpStart.SelectedDateTime.AddDays(7) <= dtpEnd.SelectedDateTime)
                {
                    dtpStart.SelectedDateTime = dtpEnd.SelectedDateTime.AddDays(-6);

                    //// 조회 기간은 7일을 초과할 수 없습니다.
                    //Util.MessageValidation("SFU3567");                    
                }
            }
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                cboTermType.IsEnabled = grdDate.IsEnabled = false;
            }
            else
            {
                cboTermType.IsEnabled = grdDate.IsEnabled = true;
            }
        }

        private void dgStatus_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgStatus.CurrentRow == null || dgStatus.SelectedIndex < 0) return;
            if (!dgStatus.IsClickedCell()) return;

            tabMain.SelectedIndex = 1;

            SelectHistoryList(true);
        }

        private void dgStatus_CheckedChanged(object sender, RoutedEventArgs e)
        {
            RadioButton rdoButton = sender as RadioButton;
            if (rdoButton.IsChecked.Equals(true))
            {
                rdoButton.Checked -= dgStatus_CheckedChanged;
                rdoButton.Unchecked -= dgStatus_CheckedChanged;

                DataTable dtData = ((DataView)dgStatus.ItemsSource).Table;
                dtData.Select().ToList().ForEach(r => r["CHK"] = false);

                dgStatus.SelectedIndex = ((DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

                rdoButton.IsChecked = true;

                rdoButton.Checked += dgStatus_CheckedChanged;
                rdoButton.Unchecked += dgStatus_CheckedChanged;
            }
        }
        
        private void dgStatus_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid grid = sender as C1DataGrid;
                if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
                {
                    var _mergeList = new List<DataGridCellsRange>();

                    int saveCmdSeq = Util.StringToInt(grid.GetValue(0, "CMD_SEQNO").GetString());
                    int mergeStart = 0, mergeEnd = 0;
                    for (int row = 0; row < grid.Rows.Count; row++)
                    {
                        if (grid.Rows.Count > 1 && row.Equals(grid.Rows.Count - 1) &&
                            Util.NVC(grid.GetValue(row - 1, "CMD_SEQNO")) != Util.NVC(grid.GetValue(row, "CMD_SEQNO")))
                        {
                            break;
                        }

                        if (!saveCmdSeq.Equals(Util.StringToInt(grid.GetValue(row, "CMD_SEQNO").GetString())) ||
                            row.Equals(grid.Rows.Count - 1))
                        {
                            mergeEnd = row.Equals(grid.Rows.Count - 1) ? row : row - 1;

                            if (mergeStart < mergeEnd)
                            {
                                foreach (C1.WPF.DataGrid.DataGridColumn dgCol in grid.Columns)
                                {
                                    switch (dgCol.Name)
                                    {
                                        case "CST_LOAD_LOCATION_CODE":
                                        case "CSTID":
                                        case "LOTID":
                                            break;
                                        default:
                                            _mergeList.Add(new DataGridCellsRange(grid.GetCell(mergeStart, dgCol.Index), grid.GetCell(mergeEnd, dgCol.Index)));
                                            break;
                                    }

                                }
                            }
                            mergeStart = row;
                            saveCmdSeq = Util.StringToInt(grid.GetValue(row, "CMD_SEQNO").GetString());
                        }
                    }
                    foreach (var range in _mergeList)
                    {
                        e.Merge(range);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistory_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid grid = sender as C1DataGrid;
                if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
                {
                    var _mergeList = new List<DataGridCellsRange>();

                    int saveCmdSeq = Util.StringToInt(grid.GetValue(0, "CMD_SEQNO").ToString());
                    int mergeStart = 0, mergeEnd = 0;
                    for (int row = 0; row < grid.Rows.Count; row++)
                    {
                        if (grid.Rows.Count > 1 && row.Equals(grid.Rows.Count - 1) &&
                            Util.NVC(grid.GetValue(row - 1, "CMD_SEQNO")) != Util.NVC(grid.GetValue(row, "CMD_SEQNO")))
                        {
                            break;
                        }

                        if (!saveCmdSeq.Equals(Util.StringToInt(grid.GetValue(row, "CMD_SEQNO").ToString())) ||
                            row.Equals(grid.Rows.Count - 1))
                        {
                            mergeEnd = row.Equals(grid.Rows.Count - 1) ? row : row - 1;

                            if (mergeStart < mergeEnd)
                            {
                                foreach (C1.WPF.DataGrid.DataGridColumn dgCol in grid.Columns)
                                {
                                    switch (dgCol.Name)
                                    {
                                        case "CST_LOAD_LOCATION_CODE":
                                        case "CSTID":
                                        case "LOTID":
                                            break;
                                        default:
                                            _mergeList.Add(new DataGridCellsRange(grid.GetCell(mergeStart, dgCol.Index), grid.GetCell(mergeEnd, dgCol.Index)));
                                            break;
                                    }

                                }
                            }
                            mergeStart = row;
                            saveCmdSeq = Util.StringToInt(grid.GetValue(row, "CMD_SEQNO").ToString());
                        }
                    }
                    foreach (var range in _mergeList)
                    {
                        e.Merge(range);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnOrderCancel
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void ClearDataGrid()
        {
            dgStatus.ClearRows();
            dgHistory.ClearRows();
        }

        private void TimerSetting()
        {
            if (cboInquiryCycle.GetBindValue() == null)
            {
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer = null;
                }
                return;
            }

            if (refreshTimer == null)
            {
                refreshTimer = new System.Windows.Threading.DispatcherTimer();

                int interval = Convert.ToInt32(cboInquiryCycle.SelectedValue);

                refreshTimer.Interval = TimeSpan.FromSeconds(interval);
                refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            }

            refreshTimer.Start();

            Refresh();
        }

        private void Refresh()
        {
            try
            {
                if (!ChkValidation())
                {
                    if (refreshTimer != null) refreshTimer.Stop();
                    cboInquiryCycle.SelectedIndex = 0;
                    return;
                }

                if (tabMain.SelectedIndex == 0)
                {
                    SelectStatusList();
                }
                else
                {
                    SelectHistoryList(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqptCombo()
        {

            try
            {
                if (cboArea.GetBindValue() == null) return;

                DataTable dtInData = new DataTable();
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("MES_SYSTEM_TYPE_CODE", typeof(string));

                DataRow inData = dtInData.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.GetBindValue();
                inData["MES_SYSTEM_TYPE_CODE"] = LoginInfo.CFG_SYSTEM_TYPE_CODE;
                dtInData.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_TRF_CMD_EQPT_CBO_2", "INDATA", "OUTDATA", dtInData);

                if (dtResult != null)
                {
                    cboFromEqptId.DisplayMemberPath = "CBO_NAME";
                    cboFromEqptId.SelectedValuePath = "CBO_CODE";
                    cboFromEqptId.ItemsSource = dtResult.Copy().AsDataView();
                    cboFromEqptId.CheckAll();

                    cboToEqptId.DisplayMemberPath = "CBO_NAME";
                    cboToEqptId.SelectedValuePath = "CBO_CODE";
                    cboToEqptId.ItemsSource = dtResult.Copy().AsDataView();
                    cboToEqptId.CheckAll();


                    //SetPortComboFrom(); // 2024.11.18. 김영국 - 최원석 책임 요청.
                    //SetPortComboTo(); // 2024.11.18. 김영국 - 최원석 책임 요청.
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPortComboFrom()
        {

            try
            {
                if (cboArea.GetBindValue() == null) return;

                DataTable dtInData = new DataTable();
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("EQPTID", typeof(string));
               
                DataRow inData = dtInData.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["EQPTID"] = Util.ConvertEmptyToNull(cboFromEqptId.SelectedItemsToString);
              
                dtInData.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MCS_PORT_CBO", "INDATA", "OUTDATA", dtInData);

                if (dtResult != null)
                {
                    cboFromPort.DisplayMemberPath = "CBO_NAME";
                    cboFromPort.SelectedValuePath = "CBO_CODE";
                    cboFromPort.ItemsSource = dtResult.Copy().AsDataView();
                    cboFromPort.CheckAll();
               }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetPortComboTo()
        {

            try
            {
                if (cboArea.GetBindValue() == null) return;

                DataTable dtInData = new DataTable();
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("EQPTID", typeof(string));

                DataRow inData = dtInData.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["EQPTID"] = Util.ConvertEmptyToNull(cboToEqptId.SelectedItemsToString);

                dtInData.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MCS_PORT_CBO", "INDATA", "OUTDATA", dtInData);

                if (dtResult != null)
                {
                    cboToPort.DisplayMemberPath = "CBO_NAME";
                    cboToPort.SelectedValuePath = "CBO_CODE";
                    cboToPort.ItemsSource = dtResult.Copy().AsDataView();
                    cboToPort.CheckAll();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private bool ChkValidation()
        {
            if (cboArea.SelectedValue==null || cboArea.SelectedValue.Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                cboArea.Focus();
                return false;
            }

            if ((dtpEnd.SelectedDateTime - dtpStart.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (!Util.IsNVC(txtCarrierId.Text) && txtCarrierId.Text.Length < 3)
            {
                // [%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");
                txtCarrierId.Focus();
                txtCarrierId.SelectAll();
                return false;
            }
            return true;
        }

        private void SelectStatusList()
        {
            try
            {
                ShowLoadingIndicator();

                dgStatus.ClearRows();

                DataTable INDATA = new DataTable("RQSTDT");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CMD_STAT_CODE", typeof(string));
                INDATA.Columns.Add("TRF_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("TRF_SECTION", typeof(string));
                INDATA.Columns.Add("FROM_EQPTID", typeof(string));
                INDATA.Columns.Add("FROM_PORTID", typeof(string));
                INDATA.Columns.Add("TO_EQPTID", typeof(string));
                INDATA.Columns.Add("TO_PORTID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("MIN_ELAPSED_TIME", typeof(Decimal));
                INDATA.Columns.Add("MAX_ELAPSED_TIME", typeof(Decimal));
                INDATA.Columns.Add("MAX_ELAPSED_TIME_YN", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.SelectedValue;
                inData["CMD_STAT_CODE"] = cboCmdStat.GetBindValue();
                inData["TRF_CLSS_CODE"] = cboTrfClass.GetBindValue();
                //inData["TRF_SECTION"] = cboTrfSection.GetBindValue();
                inData["FROM_EQPTID"] = cboFromEqptId.GetBindValue();//Util.NVC(popSearchFromEqpt.SelectedValue).Equals("") ? null : Util.NVC(popSearchFromEqpt.SelectedValue); 
                inData["FROM_PORTID"] = cboFromPort.GetBindValue();
                inData["TO_EQPTID"] = cboToEqptId.GetBindValue(); //Util.NVC(popSearchToEqpt.SelectedValue).Equals("") ? null : Util.NVC(popSearchToEqpt.SelectedValue); 
                inData["TO_PORTID"] = cboToPort.GetBindValue();
                inData["CSTID"] = txtCarrierId.GetBindValue();
                inData["LOTID"] = txtLotId.GetBindValue();
                if (cboColor.SelectedIndex != 0)
                {
                    if (_ELAPSED_MIN == _MAX_ELAPSED)  //최대시간 : 1시간이후 
                    {
                        inData["MIN_ELAPSED_TIME"] = Convert.ToDecimal(_ELAPSED_MIN);
                        //inData["MAX_ELAPSED_TIME"] = null;
                        inData["MAX_ELAPSED_TIME_YN"] = "Y";

                    }
                    else
                    {
                        inData["MIN_ELAPSED_TIME"] = Convert.ToDecimal(_ELAPSED_MIN);
                        inData["MAX_ELAPSED_TIME"] = Convert.ToDecimal(_ELAPSED_MIN) + 10;
                        inData["MAX_ELAPSED_TIME_YN"] = null;

                    }
                }
                INDATA.Rows.Add(inData);

                new ClientProxy().ExecuteService("DA_MHS_SEL_TRF_CMD_STATUS_LIST", "RQSTDT", "RSLTDT", INDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    if (!result.Columns.Contains("CHK"))
                    {
                        result.Columns.Add("CHK", typeof(bool));
                        result.Select().ToList().ForEach(r => r["CHK"] = false);
                    }
                    //MMD 공통코드의 색상정보를 조회 데이터 테이블의 색상정보컬럼에 넣기 (색상표시 속도문제 및 데이터수에 따라 DB조회하는것을 피하기 위하여 이렇게 처리)
                    if(result.Rows.Count >0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            if (Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) != 0)
                            {
                                for (int j = 0; j < dtColor.Rows.Count; j++)
                                {
                                     if (Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) >= dtColor.Rows[j]["ATTRIBUTE1"].GetDecimal() && Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) < dtColor.Rows[j]["ATTRIBUTE1"].GetDecimal() + 10)
                                    {
                                        result.Rows[i]["ELAPSED_COLOR"] = dtColor.Rows[j]["ATTRIBUTE2"].GetString();
                                        result.Rows[i]["ELAPSED_FOREGROUND"] = dtColor.Rows[j]["ATTRIBUTE3"].GetString();
                                    }
                                }

                                if (Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) > dtColor.Rows[dtColor.Rows.Count - 1]["ATTRIBUTE1"].GetDecimal())
                                {
                                    result.Rows[i]["ELAPSED_COLOR"] = dtColor.Rows[dtColor.Rows.Count - 1]["ATTRIBUTE2"].GetString();
                                    result.Rows[i]["ELAPSED_FOREGROUND"] = dtColor.Rows[dtColor.Rows.Count - 1]["ATTRIBUTE3"].GetString();

                                }
                             }
                            //반송지시자가 NULL 이면  RTD로 치환
                            if(result.Rows[i]["CMD_USER_NAME"].GetString() == string.Empty)
                            {
                                result.Rows[i]["CMD_USER_NAME"] = "RTD";
                            }
                            //반송 요청일시가 Null이고 RTD 반송이면 '-(Batch 반송),  
                            if (result.Rows[i]["TRF_REQ_DTTM"].GetString() == string.Empty && result.Rows[i]["TRF_CLSS_CODE"].GetString() == "R")
                            {
                                //result.Rows[i]["TRF_REQ_DTTM"] = ObjectDic.Instance.GetObjectName("-(RTD 반송)");
                                result.Rows[i]["TRF_REQ_DTTM"] = result.Rows[i]["TRF_CREATE_DTTM"];
                                result.Rows[i]["REQ_DELAY_DTTM"] = "000:00:00";
                                
                            }
                            //반송 요청일시가 Null이고 MCS 반송이면 '-(MCS 반송)'
                            else if (result.Rows[i]["TRF_REQ_DTTM"].GetString() == string.Empty && result.Rows[i]["TRF_CLSS_CODE"].GetString() == "M")
                            {
                                //result.Rows[i]["TRF_REQ_DTTM"] = ObjectDic.Instance.GetObjectName("-(MCS 반송)");
                                result.Rows[i]["TRF_REQ_DTTM"] = result.Rows[i]["TRF_CREATE_DTTM"];
                                result.Rows[i]["REQ_DELAY_DTTM"] = "000:00:00";
                            }
                            ////반송 대기시간이 Null이고 RTD 반송이면 '-(Batch 반송),  
                            //if (result.Rows[i]["TRF_DELAY_DTTM"].GetString() == string.Empty && result.Rows[i]["TRF_CLSS_CODE"].GetString() == "R")
                            //{
                            //    result.Rows[i]["TRF_DELAY_DTTM"] = ObjectDic.Instance.GetObjectName("-(RTD 반송)");
                            //}
                            ////반송 대기시간이 Null이고 MCS 반송이면 '-(MCS 반송)'
                            //else if (result.Rows[i]["TRF_DELAY_DTTM"].GetString() == string.Empty && result.Rows[i]["TRF_CLSS_CODE"].GetString() == "M")
                            //{
                            //    result.Rows[i]["TRF_DELAY_DTTM"] = ObjectDic.Instance.GetObjectName("-(MCS 반송)");
                            //}

                            
                        }
                   }
                    if(cboColor.SelectedIndex >0)
                    {
                        DataView dvSource = result.AsDataView();
                        dvSource.Sort = "ELAPSED_MIN DESC";
                        result = dvSource.ToTable();
                    }
                    dgStatus.SetItemsSource(result, FrameOperation, true);  //2024-12-24 CWS 수정 false --> true
                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectHistoryList(bool isDoubleClick)
        {
            try
            {
                ShowLoadingIndicator();

                dgHistory.ClearRows();

                DataTable INDATA = new DataTable("RQSTDT");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CMD_STAT_CODE", typeof(string));
                INDATA.Columns.Add("TRF_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("TRF_SECTION", typeof(string));
                INDATA.Columns.Add("FROM_EQPTID", typeof(string));
                INDATA.Columns.Add("TO_EQPTID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("TERM_TYPE", typeof(string));
                INDATA.Columns.Add("FROM_DATE", typeof(string));
                INDATA.Columns.Add("TO_DATE", typeof(string));
                INDATA.Columns.Add("REQ_SEQNO", typeof(string));
                INDATA.Columns.Add("MIN_ELAPSED_TIME", typeof(Decimal));
                INDATA.Columns.Add("MAX_ELAPSED_TIME", typeof(Decimal));
                INDATA.Columns.Add("MAX_ELAPSED_TIME_YN", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.SelectedValue;
                inData["CMD_STAT_CODE"] = cboCmdStat.GetBindValue();
                inData["TRF_CLSS_CODE"] = cboTrfClass.GetBindValue();
                //inData["TRF_SECTION"] = cboTrfSection.GetBindValue();
                inData["FROM_EQPTID"] =  cboFromEqptId.GetBindValue(); //Util.NVC(popSearchFromEqpt.SelectedValue).Equals("") ? null : Util.NVC(popSearchFromEqpt.SelectedValue);
                inData["TO_EQPTID"] = cboToEqptId.GetBindValue();//Util.NVC(popSearchToEqpt.SelectedValue).Equals("") ? null : Util.NVC(popSearchToEqpt.SelectedValue); 
                inData["CSTID"] = txtCarrierId.GetBindValue();
                inData["LOTID"] = txtLotId.GetBindValue();
                inData["TERM_TYPE"] = cboTermType.GetBindValue();
                inData["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                inData["TO_DATE"] = dtpEnd.SelectedDateTime.ToString("yyyyMMdd");
                if (isDoubleClick && dgStatus.CurrentRow != null)
                {
                    inData["REQ_SEQNO"] = dgStatus.GetValue("REQ_SEQNO");
                }
                if (cboColor.SelectedIndex != 0)
                {
                    if (_ELAPSED_MIN == _MAX_ELAPSED)  //최대시간 : 1시간이후 
                    {
                        inData["MIN_ELAPSED_TIME"] = Convert.ToDecimal(_ELAPSED_MIN);
                        //inData["MAX_ELAPSED_TIME"] = null;
                        inData["MAX_ELAPSED_TIME_YN"] = "Y";

                    }
                    else
                    {
                        inData["MIN_ELAPSED_TIME"] = Convert.ToDecimal(_ELAPSED_MIN);
                        inData["MAX_ELAPSED_TIME"] = Convert.ToDecimal(_ELAPSED_MIN) + 10;
                        inData["MAX_ELAPSED_TIME_YN"] = null;

                    }
                }
                INDATA.Rows.Add(inData);

                new ClientProxy().ExecuteService("BR_MHS_SEL_TRF_CMD_HISTORY_LIST", "INDATA", "RSLTDT", INDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    //MMD 공통코드의 색상정보를 조회 데이터 테이블의 색상정보컬럼에 넣기 (색상표시 속도문제 및 데이터수에 따라 DB조회하는것을 피하기 위하여 이렇게 처리)
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            if (Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) != 0)
                            {
                                for (int j = 0; j < dtColor.Rows.Count; j++)
                                {
                                    if (Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) >= dtColor.Rows[j]["ATTRIBUTE1"].GetDecimal() && Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) < dtColor.Rows[j]["ATTRIBUTE1"].GetDecimal() + 10)
                                    {
                                        result.Rows[i]["ELAPSED_COLOR"] = dtColor.Rows[j]["ATTRIBUTE2"].GetString();
                                        result.Rows[i]["ELAPSED_FOREGROUND"] = dtColor.Rows[j]["ATTRIBUTE3"].GetString();
                                    }
                                }

                                if (Convert.ToDecimal(result.Rows[i]["ELAPSED_MIN"].GetDecimal()) > dtColor.Rows[dtColor.Rows.Count - 1]["ATTRIBUTE1"].GetDecimal())
                                {
                                    result.Rows[i]["ELAPSED_COLOR"] = dtColor.Rows[dtColor.Rows.Count - 1]["ATTRIBUTE2"].GetString();
                                    result.Rows[i]["ELAPSED_FOREGROUND"] = dtColor.Rows[dtColor.Rows.Count - 1]["ATTRIBUTE3"].GetString();

                                }
                            }
                            //반송지시자가 NULL 이면  RTD로 치환
                            if (result.Rows[i]["CMD_USER_NAME"].GetString() == string.Empty)
                            {
                                result.Rows[i]["CMD_USER_NAME"] = "RTD";
                            }
                            //반송 요청일시가 Null이고 RTD 반송이면 '-(Batch 반송),  
                            if (result.Rows[i]["TRF_REQ_DTTM"].GetString() == string.Empty && result.Rows[i]["TRF_CLSS_CODE"].GetString() == "R")
                            {
                                //result.Rows[i]["TRF_REQ_DTTM"] = ObjectDic.Instance.GetObjectName("-(RTD 반송)");
                                result.Rows[i]["TRF_REQ_DTTM"] = result.Rows[i]["TRF_CREATE_DTTM"];
                                result.Rows[i]["REQ_DELAY_DTTM"] = "000:00:00";

                            }
                            //반송 요청일시가 Null이고 MCS 반송이면 '-(MCS 반송)'
                            else if (result.Rows[i]["TRF_REQ_DTTM"].GetString() == string.Empty && result.Rows[i]["TRF_CLSS_CODE"].GetString() == "M")
                            {
                                //result.Rows[i]["TRF_REQ_DTTM"] = ObjectDic.Instance.GetObjectName("-(MCS 반송)");
                                result.Rows[i]["TRF_REQ_DTTM"] = result.Rows[i]["TRF_CREATE_DTTM"];
                                result.Rows[i]["REQ_DELAY_DTTM"] = "000:00:00";
                            }
                        }
                    }
                    if (cboColor.SelectedIndex > 0)
                    {
                        DataView dvSource = result.AsDataView();
                        dvSource.Sort = "TRF_PASS_DTTM, ELAPSED_MIN DESC";
                        result = dvSource.ToTable();
                    }

                    dgHistory.SetItemsSource(result, FrameOperation, true);

                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }



        #endregion

        private void popSearchFromEqpt_ValueChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        private void popSearchToEqpt_ValueChanged(object sender, EventArgs e)
        {
            ClearDataGrid();
        }

        //private void setFromToEqpPopControl()
        //{
            //const string bizRuleName = "DA_MHS_SEL_TRF_CMD_EQPT_CBO_2";
            //string[] arrColumn = { "LANGID", "AREAID", "MES_SYSTEM_TYPE_CODE" };
            //string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboArea.GetBindValue()), LoginInfo.CFG_SYSTEM_TYPE_CODE };
            //CommonCombo.SetFindPopupCombo(bizRuleName, popSearchFromEqpt, arrColumn, arrCondition, (string)popSearchFromEqpt.SelectedValuePath, (string)popSearchFromEqpt.DisplayMemberPath);

            //string[] arrColumn2 = { "LANGID", "AREAID", "MES_SYSTEM_TYPE_CODE" };
            //string[] arrCondition2 = { LoginInfo.LANGID, Util.NVC(cboArea.GetBindValue()), LoginInfo.CFG_SYSTEM_TYPE_CODE };
            //CommonCombo.SetFindPopupCombo(bizRuleName, popSearchToEqpt, arrColumn2, arrCondition2, (string)popSearchToEqpt.SelectedValuePath, (string)popSearchToEqpt.DisplayMemberPath);
        //}

        private void SetLegendColorCombo()
        {
            cboColor.Items.Clear();
            //C1ComboBoxItem cbItemTitle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            //C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            //cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            //cboColor.Items.Add(cbItemTitle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            DataRow drColor = inTable.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;
            drColor["CMCDTYPE"] = "TRANSFER_ORDER_TIME_LEGEND";

            inTable.Rows.Add(drColor);
            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", inTable);

            dtColor = dtColorResult.Copy();
            _MAX_ELAPSED = dtColorResult.Rows[dtColorResult.Rows.Count-1]["ATTRIBUTE1"].ToString();

            DataRow dRow = dtColorResult.NewRow();
            dRow["CBO_CODE"] = "";
            dRow["CBO_NAME"] = "LEGEND";
            dRow["ATTRIBUTE1"] = "";
            dRow["ATTRIBUTE2"] = "#FFFFFF";
            dRow["ATTRIBUTE3"] = "";
            dRow["ATTRIBUTE4"] = "";
            dRow["ATTRIBUTE5"] = "";
            dtColorResult.Rows.InsertAt(dRow, 0);

            foreach (DataRow row in dtColorResult.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem();
                cbItem1.Content = row["CBO_NAME"].GetString();
                cbItem1.Tag = row["ATTRIBUTE1"].GetString();
                //cbItem1.Name = row["ATTRIBUTE3"].GetString();

                cbItem1.FontWeight = FontWeights.Bold;
                cbItem1.Background = new BrushConverter().ConvertFromString(row["ATTRIBUTE2"].ToString()) as SolidColorBrush;
                if (row["ATTRIBUTE3"].GetString() != string.Empty)
                {
                    cbItem1.Foreground = new BrushConverter().ConvertFromString(row["ATTRIBUTE3"].ToString()) as SolidColorBrush;
                }
                cboColor.Items.Add(cbItem1);
            }
            if (cboColor.Items != null && cboColor.Items.Count > 0)
            {
                cboColor.SelectedIndex = 0;
            }
     
        }

        private void dgStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
              
                    if (DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_MIN").GetDecimal() >= dtColor.Rows[0]["ATTRIBUTE1"].GetDecimal()) //MMD에 등록되어 있는 가장최소의 시간단위 이상일경우 색상표시 
                    {

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_COLOR").GetString()));

                        if (DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_FOREGROUND").GetString() != string.Empty)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_FOREGROUND").GetString()));
                        }

                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                }
            }));
        }

        private void dgStatus_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }
        private void cboColor_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue == -1) return;
            try
            {
                int currentRowIndex = e.NewValue;
                if (cboColor != null && cboColor.SelectedItem != null)
                {
                 _ELAPSED_MIN = ((ContentControl)(cboColor.Items[currentRowIndex])).Tag.GetString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_MIN").GetDecimal() >= dtColor.Rows[0]["ATTRIBUTE1"].GetDecimal()) //MMD에 등록되어 있는 가장최소의 시간단위 이상일경우 색상표시 
                    {

                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_COLOR").GetString()));

                        if (DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_FOREGROUND").GetString() != string.Empty)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ELAPSED_FOREGROUND").GetString()));
                        }

                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                }
            }));
        }

        private void dgHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }


        private void RepLotUseForm()
        {

            if (_util.IsCommonCodeUseAttr("REP_LOT_USE_AREA", cboArea.SelectedValue.ToString()))  //NFF 추가
            {

                dgStatus.Columns["CSTID"].Header = "CARRIER_REP_LOTID"; //ObjectDic.Instance.GetObjectName("CARRIER_REP_LOTID");
                dgHistory.Columns["CSTID"].Header = "CARRIER_REP_LOTID"; //ObjectDic.Instance.GetObjectName("CARRIER_REP_LOTID");
                txtBlockCarrierId.Text = ObjectDic.Instance.GetObjectName("CARRIER_REP_LOTID");

            }
            else
            {
                dgStatus.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                dgHistory.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                txtBlockCarrierId.Text = ObjectDic.Instance.GetObjectName("Carrier ID"); ;

            }
        }

    }
}