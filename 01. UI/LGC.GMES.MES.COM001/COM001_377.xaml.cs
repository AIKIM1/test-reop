/*************************************************************************************
 Created Date : 2023.04.25
      Creator : 이다혜
   Decription : 설비 Loss 수정 (설비 Loss 수정 / 설비 Loss 수정 이력)
--------------------------------------------------------------------------------------
 [Change History]
  2023.04.25  이다혜 : Initial Created
  2023.06.07  이다혜 : 승인 요청 시, Insert Data 추가
  2023.06.13  이다혜 : 승인 요청 가능 기간 -3일에서 -7일로 변경
  2023.07.17  김대현 : MES 시스템의 설비 Loss 수정 승인 요청 기능을 위한 신규 개발/기능 변경
  2023.08.09  김대현 : Pack 공장일때 승인자 목록 조회하도록 수정
  2023.08.24  임시혁 : E20230711-000645. 변경 Loss 분류 콤보를 설비 Loss 등록 Loss 분류 콤보와 동일하게 설정하기 위해 수정.
  2023.08.25  김대현 : Pack 공장 콤보박스 로직 수정
  2023.08.28  주동석 : 요청취소 기능 추가
  2023.09.15  안유수 E20230913-000991 설비 LOSS 수정 이력 화면 라인, 설비 콤보박스 MULTI 조회 및 날짜 조회 조건 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.COM001;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_377 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        int iEqptCnt;

        bool bPack;
        bool bMPPD = false; // Modifiable person on the previous day : Pack 전용 전일 수정 가능자 (신규 권한)
        bool bForm;
        bool isOnlyRemarkSave = false;  // CSR : C20220512-000432
        bool isTotalSave = false;       // CSR : C20220512-000432

        string sMainEqptID;
        string sSearchDay = "";

        string RunSplit; //동, 공정에 따라 RUN상태를 Split할 수 있는지 구분
        string _grid_eqpt = string.Empty;
        string _grid_area = string.Empty;
        string _grid_proc = string.Empty;
        string _grid_eqsg = string.Empty;
        string _grid_shit = string.Empty;

        List<string> liProcId;

        DataTable dtMainList = new DataTable();
        DataTable AreaTime;
        DataTable dtShift;
        DataTable dtRemarkMandatory;
        DataTable dtQA;                 // CSR : C20220512-000432
        Hashtable hash_loss_color = new Hashtable();
        Hashtable org_set;

        DataSet dsEqptTimeList = null;

        string strAttr1 = string.Empty;
        string strAttr2 = string.Empty;
        string sNowDay = string.Empty;
        bool bUseEqptLossAppr = false; // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        string MachineEqptChk = string.Empty;   //Machine 설비 Loss 수정 가능여부 Flag :    2023.03.16 오화백
        string MachineEqptChkHist = string.Empty;

        /// <summary>
        /// ///
        /// </summary>

        public COM001_377()
        {
            InitializeComponent();

            InitCombo();
            InitGrid();
            GetLossColor();

            if (string.Equals(GetAreaType(), "E"))
            {
                lotTname.Visibility = Visibility.Visible;
            }
            if (!string.Equals(GetAreaType(), "P"))  //PACK 부서를 제외한 작업자 버튼 및 TEXTBOX 비활성화.
            {
                //InitUser();
            }
            if (bPack)
            {
                GetPackAuth();
            }

            GetEqptLossApprAuth();  // CSR : E20230420-001240, 설비 LOSS 수정 화면 추가에 따른 Validation 추가
        }

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
            //cboProcessHist1.ApplyTemplate();
            cboEquipmentSegmentHist1.ApplyTemplate();
            cboEquipmentHist.ApplyTemplate();

            if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            {
                bPack = false;
                chkMain.IsEnabled = true;

                //2023.03.07 활성화 구분 추가
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                    bForm = true;
                else
                    bForm = false;

                // 설비 Loss 수정
                SetAreaCombo(cboArea);

                SetEquipmentSegmentCombo(cboEquipmentSegment);
                SetProcessCombo(cboProcess);
                SetEquipmentCombo(cboEquipment);
                SetShiftCombo(cboShift);
                SetMachineEqptCombo(cboEquipment_Machine);
                SetApprUserCombo(cboApprUser);

                cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
                cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
                SetTroubleUnitColumnDisplay();

                SetUpperLossCodeGridCombo();
                SetLossCodeGridCombo();
                SetLossDetlCodeGridCombo();

                // 설비 Loss 수정 이력

                SetAreaCombo(cboAreaHist);

                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    cboEquipmentSegmentHist.Visibility = Visibility.Visible;
                    cboEquipmentSegmentHist1.Visibility = Visibility.Collapsed;
                    
                    SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist);
                    SetProcessHistComboFCS(cboProcessHist);
                    //SetProcessHistCombo(cboProcessHist);
                }
                else
                {
                    cboEquipmentSegmentHist.Visibility = Visibility.Collapsed;
                    cboEquipmentSegmentHist1.Visibility = Visibility.Visible;

                    SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist1);
                    SetProcessHistCombo(cboProcessHist);
                    //SetProcessHistCombo(cboProcessHist1);
                }

                
                SetEquipmentHistCombo(cboEquipmentHist);
                SetShiftHistCombo(cboShiftHist);
                SetMachineEqptHistCombo(cboEquipment_MachineHist);

                cboAreaHist.SelectedValueChanged += cboAreaHist_SelectedValueChanged;
                cboEquipmentSegmentHist.SelectedValueChanged += cboEquipmentSegmentHist_SelectedValueChanged;
                cboProcessHist.SelectedValueChanged += cboProcessHist_SelectedValueChanged;
                cboEquipmentHist.SelectionChanged += cboEquipmentHist_SelectionChanged;

            }
            else
            {
                // Pack인 경우
                bPack = true;
                bForm = false;
                // 2021.11.12 김건식 - PACK 전체사용으로 IF분기 제거
                chkMain.IsEnabled = true;

                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent);

                SetEquipment();
                //SetShift();

                cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
                cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
                //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
                ldpDatePicker.SelectedDataTimeChanged += ldpDatePicker_SelectedDataTimeChanged;

                //설비 변경 시 공정도 같이 변경 Event
                cboEquipment.SelectedItemChanged += CboEquipment_SelectedItemChanged;
                SetApprUserCombo(cboApprUser);

                //공정은 개인이 선택 불가
                cboProcess.IsEnabled = false;
                //처음은 ALL선택
                cboProcess.SelectedIndex = 0;

                //2023.08.21
                SetUpperLossCodeGridCombo();
                SetLossCodeGridCombo();
                SetLossDetlCodeGridCombo();

                // 설비 Loss 수정 이력
                SetAreaCombo(cboAreaHist);
                SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist1);
                SetProcessHistCombo(cboProcessHist);
                SetEquipmentHistCombo(cboEquipmentHist);
                SetShiftHistCombo(cboShiftHist);
                SetMachineEqptHistCombo(cboEquipment_MachineHist);

                cboEquipmentSegmentHist.Visibility = Visibility.Collapsed;
                cboEquipmentSegmentHist1.Visibility = Visibility.Visible;

                cboAreaHist.SelectedValueChanged += cboAreaHist_SelectedValueChanged;
                //cboEquipmentSegmentHist.SelectedValueChanged += cboEquipmentSegmentHist_SelectedValueChanged;
                cboProcessHist.SelectedValueChanged += cboProcessHist_SelectedValueChanged;
                cboEquipmentHist.SelectionChanged += cboEquipmentHist_SelectionChanged;

            }

            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboColor.Items.Add(cbItemTitle);

            C1ComboBoxItem cbItemRun = new C1ComboBoxItem();
            cbItemRun.Content = "Run";
            cbItemRun.Background = ColorToBrush(GridBackColor.R);
            cboColor.Items.Add(cbItemRun);

            C1ComboBoxItem cbItemWait = new C1ComboBoxItem();
            cbItemWait.Content = "Wait";
            cbItemWait.Background = ColorToBrush(GridBackColor.W);
            cboColor.Items.Add(cbItemWait);

            C1ComboBoxItem cbItemTrouble = new C1ComboBoxItem();
            cbItemTrouble.Content = "Trouble";
            cbItemTrouble.Background = ColorToBrush(GridBackColor.T);
            cboColor.Items.Add(cbItemTrouble);

            C1ComboBoxItem cbItemOff = new C1ComboBoxItem();
            cbItemOff.Content = "OFF";
            cbItemOff.Background = ColorToBrush(GridBackColor.F);
            cboColor.Items.Add(cbItemOff);

            C1ComboBoxItem cbItemUserStop = new C1ComboBoxItem();
            cbItemUserStop.Content = "UserStop";
            cbItemUserStop.Background = ColorToBrush(GridBackColor.U);
            cboColor.Items.Add(cbItemUserStop);

            cboColor.SelectedIndex = 0;

            //작업조 Default 셋팅
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();

                // Machine 설비수정 가능할 경우 - by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                    }
                    else
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment_Machine.SelectedValue);
                    }
                }
                else
                {
                    row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                }

                dt.Rows.Add(row);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_SET_SHIFT", "RQSTDT", "RSLTDT", dt);

                if (dtRslt.Rows.Count != 0)
                {
                    cboShift.SelectedValue = Convert.ToString(dtRslt.Rows[0]["SHFT_ID"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 색지도 그리드 초기화
        /// </summary>
        private void InitGrid()
        {
            try
            {
                _grid.Width = 3000;

                for (int i = 0; i < 361; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    if (i == 0)
                    {
                        gridCol1.Width = GridLength.Auto;
                    }
                    else { gridCol1.Width = new GridLength(3); }

                    _grid.ColumnDefinitions.Add(gridCol1);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용할 색상정보 가져오기
        /// </summary>
        private void GetLossColor()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOSS_COLR", "RQSTDT", "RSLTDT", dtRqst);

                hash_loss_color = DataTableConverter.ToHash(dtRslt);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["LOSS_NAME"];
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["DISPCOLOR"].ToString()));
                    cboColor.Items.Add(cbItem);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetPackAuth()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("AUTHID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "PACK_LOSS_ENGR_CWA";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count != 0)
                {
                    bMPPD = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd");
            dt.Rows.Add(row);

            AreaTime = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (AreaTime.Rows.Count == 0) { }
            if (Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return;
            }
            TimeSpan tmp = DateTime.Parse(System.DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;//DateTime.Parse("06:59:59").TimeOfDay;//
            if (tmp < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
            {
                ldpDatePicker.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            }
        }

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
                SetShift();
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }

            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
            }
        }
        #endregion

        #region [설비] - 조회 조건
        private void CboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!bPack)
            {
                return;
            }
            else
            {
                if (cboEquipment.SelectedIndex == 0)
                    cboProcess.SelectedIndex = 0;
                else
                    cboProcess.SelectedValue = liProcId[cboEquipment.SelectedIndex - 1].ToString();
            }
        }
        #endregion

        #region [작업일] - 조회 조건
        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!bPack)
            {
                return;
            }

            if (ldpDatePicker.SelectedDateTime.Year > 1 && ldpDatePicker.SelectedDateTime.Year > 1)
            {
                SetShift();
            }
        }
        #endregion

        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                //C20210723 - 000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment) || string.IsNullOrEmpty(sEquipmentSegment))
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                string sProcess = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProcess) || string.IsNullOrEmpty(sProcess))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                string sEqpt = Util.GetCondition(cboEquipment, "SFU1153"); //설비를 선택하세요
                if (sEqpt.Equals("")) return;

                //초기화
                ClearGrid();

                _grid_area = Util.GetCondition(cboArea);
                _grid_eqsg = Util.GetCondition(cboEquipmentSegment);
                _grid_eqpt = Util.GetCondition(cboEquipment);
                _grid_proc = Util.GetCondition(cboProcess);
                _grid_shit = Util.GetCondition(cboShift);

                if (this.dtQA != null) this.dtQA.Clear();       // CSR : C20220512-000432
                this.isOnlyRemarkSave = false;                  // CSR : C20220512-000432
                this.isTotalSave = false;                       // CSR : C20220512-000432

                SelectRemarkMandatory();

                SelectLossRunArea();
                // Machine 설비수정 가능할 경우  Main 설비, Machine 설비 전체 조회가 되도록 - by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    GetEqptLossRawList_Machine();
                    SelectProcess_Machine();
                }
                else
                {
                    GetEqptLossRawList();
                    SelectProcess();
                }
                GetEqptLossDetailList();
                sMainEqptID = "A" + Util.GetCondition(cboEquipment);

                sSearchDay = ldpDatePicker.SelectedDateTime.ToString("yyyyMMdd");
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelInputRequest();
        }

        public void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!this.SaveValidation())
            {
                return;
            }

            // 설비 LOSS 수정 승인 요청 하시겠습니까?
            Util.MessageConfirm("SFU5178", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.SaveProcess();
                }
            }
            );
        }

        public void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if(SearchDatechk())
                {
                    return;
                }
                
                ShowLoadingIndicator();
                DoEvents();

                GetEqptLossChangeApprHistList();
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

        #region 조회 조건

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEqptLossApprAuth();
            SetApprUserCombo(cboApprUser);

            if (bPack) return;

            SetEquipmentSegmentCombo(cboEquipmentSegment);    
        }

        private void CancelInputRequest()
        {
            try
            {
                if (!CheckCancelValidation()) return;

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("RQSTDT");
                inTable.Columns.Add("APPR_STAT", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("APPR_SEQNO", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STRT_DTTM", typeof(string));
                inTable.Columns.Add("END_DTTM", typeof(string));
                inTable.Columns.Add("WRK_DATE", typeof(string));
                inTable.Columns.Add("LOSS_SEQNO", typeof(string));

                for (int i = 0; i < dgDetailHist.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgDetailHist, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["APPR_STAT"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "APPR_STAT"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["APPR_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "APPR_SEQNO"));
                    newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "EQPTID"));
                    newRow["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "WRK_DATE"));
                    newRow["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetailHist.Rows[i].DataItem, "LOSS_SEQNO"));

                    inTable.Rows.Add(newRow);
                }

                // 취소하시겠습니까?
                Util.MessageConfirm("SFU4616", sResult =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_APPR_CANCEL", "RQSTDT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 취소되었습니다.
                                Util.MessageInfo("SFU1937");

                                // 재조회
                                GetEqptLossChangeApprHistList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool CheckCancelValidation()
        {
            DataRow[] drInfo = Util.gridGetChecked(ref dgDetailHist, "CHK");

            if (drInfo.Length == 0)
            {
                Util.MessageValidation("SFU1636");  // 선택된 대상이 없습니다.
                return false;
            }
            else
            {
                if (CheckManagerAuth())
                    return true;

                foreach (DataRow dr in drInfo)
                {
                    if (dr["APPR_REQ_USERID"].ToString() != LoginInfo.USERID)
                    {
                        Util.MessageValidation("SFU5184");  // 요청자만 요청 취소가 가능합니다.
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CheckManagerAuth()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                //dr["AUTHID"] = "ASSYAU_MANA,MESADMIN";
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            SetProcessCombo(cboProcess);
            SetTroubleUnitColumnDisplay();
            SetShiftCombo(cboShift);
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            SetEquipmentCombo(cboEquipment);
            SetTroubleUnitColumnDisplay();
            //Machine 설비 Loss 수정 가능 여부 by 오화백  2023 03 16
            MachineEqpt_Loss_Modify_Chk(cboProcess.SelectedValue.GetString());
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            SetShiftCombo(cboShift);
            SetMachineEqptCombo(cboEquipment_Machine);
        }

        private void cboAreaHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (bPack) return;

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist);
            }
            else
            {
                SetEquipmentSegmentHistCombo(cboEquipmentSegmentHist1);
            }
        }

        private void cboEquipmentSegmentHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                SetProcessHistComboFCS(cboProcessHist);
            }
            else
            {
                SetProcessHistCombo(cboProcessHist);
            }
            SetShiftHistCombo(cboShiftHist);
        }

        private void cboProcessHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (bPack) return;

            SetEquipmentHistCombo(cboEquipmentHist);
            MachineEqptHist_Loss_Modify_Chk(cboProcessHist.SelectedValue.GetString());
        }

        private void cboEquipmentHist_SelectionChanged(object sender, EventArgs e)
        {
            if (bPack) return;

            SetShiftHistCombo(cboShiftHist);
            SetMachineEqptHistCombo(cboEquipment_MachineHist);
        }

        /// <summary>
        /// Main 체크박스 클릭시 - Machine 설비Loss 여부 체크 후  Machine 설비 콤보박스 Visible 여부  오화백 2023-03.16 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkMain_Click(object sender, RoutedEventArgs e)
        {
            if (chkMain.IsChecked == true)
            {
                Machine.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (MachineEqptChk == "Y")
                {
                    Machine.Visibility = Visibility.Visible;
                }
                else

                {
                    Machine.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void chkMainHist_Click(object sender, RoutedEventArgs e)
        {
            if (chkMainHist.IsChecked == true)
            {
                MachineHist.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (MachineEqptChkHist == "Y")
                {
                    MachineHist.Visibility = Visibility.Visible;
                }
                else

                {
                    MachineHist.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region 조회 결과 Grid

        /// <summary>
        /// 상세 데이터 색변환하기
        /// </summary>
        private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Row 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));
                    string loss_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE"));
                    string loss_detl_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"));
                    string appr_stat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "APPR_STAT"));

                    if (sCheck.Equals("DELETE"))
                    {
                        System.Drawing.Color color = GridBackColor.Color4;
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                    }
                    else if (!sCheck.Equals("DELETE") && !loss_code.Equals("") && !loss_detl_code.Equals(""))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lightBlue"));
                    }
                    else
                    {
                        int originSeconds = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ORIGIN_SECONDS"));
                        int eqptLossMandRegBasOverTime = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_LOSS_MAND_REG_BAS_OVER_TIME"));

                        if (eqptLossMandRegBasOverTime > 0 && eqptLossMandRegBasOverTime != 180 && originSeconds >= eqptLossMandRegBasOverTime)
                        {
                            //C20210126-000047 경과시간(초) 가 필수입력 기준시간(초) 보다 크면 색 지정. 180(초)는 전사 기본값이기 때문에 색 변경하지 않음.
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 102, 0, 51));
                        }
                        else
                        {
                            System.Drawing.Color color = GridBackColor.Color6;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        }
                    }

                    // 승인 대기 상태일 경우, 승인 요청 못하도록
                    if (appr_stat.Equals("W"))
                    {
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }

                //link 색변경
                if (e.Cell.Column.Name != null)
                {
                    if (e.Cell.Column.Name.Equals("CHECK_DELETE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name.Equals("SPLIT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }

                if (e.Cell.Column.Name.Equals("TRBL_NAME") || e.Cell.Column.Name.Equals("APPR_REQ_LOSS_CNTT"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(200, DataGridUnitType.Pixel);

                }
            }));
        }

        private void dgDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll.Checked -= new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll1_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll1_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll1_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgDetail.GetRowCount(); i++)
                {
                    // 결재 상태가 승인대기가 아닐 경우에만 수정 가능
                    string appr_stat = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "APPR_STAT"));
                    if (appr_stat.Equals("W"))
                    {
                        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", true);
                    }
                }
            }
        }

        private void checkAll1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgDetail.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        /// </summary>
        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border aa = sender as Border;

            org_set = aa.Tag as Hashtable;

            if (e.ChangedButton == MouseButton.Right)
            {
                if (!org_set["STATUS"].ToString().Equals("R"))
                {
                    ContextMenu menu = this.FindResource("_gridMenu") as ContextMenu;
                    menu.PlacementTarget = sender as Border;
                    menu.IsOpen = true;

                    for (int i = 0; i < menu.Items.Count; i++)
                    {
                        MenuItem item = menu.Items[i] as MenuItem;

                        switch (item.Name.ToString())
                        {
                            case "LossDetail":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss내역보기");
                                item.Click -= lossDetail_Click;
                                item.Click += lossDetail_Click;
                                break;
                        }
                    }
                }
            }
        }

        private void lossDetail_Click(object sender, RoutedEventArgs e)
        {
            COM001_014_TROUBLE wndPopup = new COM001_014_TROUBLE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = org_set["EQPTID"].ToString();
                Parameters[1] = org_set["TIME"].ToString();
                Parameters[2] = cboShift.SelectedValue.ToString().Equals("") ? 20 : 10;
                Parameters[3] = Util.GetCondition(ldpDatePicker);//ldpDatePicker.SelectedDateTime.ToShortDateString();


                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.Show()));
            }
        }


        /// <summary>
        /// 스프레드 색 표현
        /// </summary>
        private void dgDetailHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string appr_stat = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "APPR_STAT"));
                    if (appr_stat.Equals("R"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightCoral);
                    }
                    if (appr_stat.Equals("A"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                }
            }));
        }


        #endregion

        #endregion

        #region Method

        /// <summary>
        /// 색지도초기화
        /// </summary>
        private void ClearGrid()
        {
            try
            {
                foreach (Border _border in _grid.Children.OfType<Border>())
                {
                    _grid.UnregisterName(_border.Name);
                }

                NameScope.SetNameScope(_grid, new NameScope());

                _grid.Children.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 부동내역 전체 조회 ( 가동 Trend 마우스 선택 시 범위 지정 용으로 사용 )
        /// </summary>
        private void GetEqptLossRawList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                if (dr["EQPTID"].Equals("")) return;
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// Machine 설비도 같이 조회 되도록 DA가 변경   - by 오화백 2023.01.20
        /// 부동내역 전체 조회 ( 가동 Trend 마우스 선택 시 범위 지정 용으로 사용 )
        /// </summary>
        private void GetEqptLossRawList_Machine()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    dr["EQPTID"] = cboEquipment.SelectedValue.GetString();
                }
                else
                {
                    dr["EQPTID"] = cboEquipment_Machine.SelectedValue.GetString();
                }
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                dtMainList = RunSplit.Equals("Y") ? new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_RUN_UNIT", "RQSTDT", "RSLTDT", RQSTDT) : new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSRAW_UNIT", "RQSTDT", "RSLTDT", RQSTDT); //case로 다르게 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 색지도 처리
        /// </summary>
        private void SelectProcess()
        {
            try
            {
                _grid.RowDefinitions.Clear();
                _grid.Children.Clear();

                string sEqptID = Util.GetCondition(cboEquipment);
                string sEqptType = (chkMain.IsChecked.Equals(true)) ? "M" : "A";
                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);

                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();

                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName(sEqptID, sEqptType);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dtEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                //spdMList.SuspendLayout();

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                //spdMList.ActiveSheet.RowCount = (hash_title.Count) + 1;

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정
                    hash_color.Add(sTitle, sID);
                }

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (cboShift.SelectedValue.ToString().Equals("") ? 30 : 60) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);

                        _grid.Children.Add(_lable);

                    }

                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);
                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];

                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정

                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);

                        int min = int.Parse(sEqptTimeList.Substring(10, 2));
                        int sec = int.Parse(sEqptTimeList.Substring(12, 2));

                        if (min % 20 == 0 && sec == 0)
                        {
                            _border.BorderBrush = new SolidColorBrush(Colors.Black);
                            _border.BorderThickness = new Thickness(1, 0, 0, 0);
                        }

                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;

                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);
                        }
                    }

                    cnt++;

                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        cnt = 0;
                        inc++;
                    }
                }

                int iTotalRow = inc == 12 ? (hash_title.Count + 1) * inc : (hash_title.Count + 1) * (inc + 1);

                for (int k = 0; k < iTotalRow; k++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 색지도 처리 
        /// Machine 설비수정 가능할 경우 SelectProcess복사해서 사용  - by 오화백 2023-03-16
        /// </summary>
        private void SelectProcess_Machine()
        {
            try
            {
                string sEqptID = string.Empty;
                string sEqptType = string.Empty;
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    sEqptID = Util.GetCondition(cboEquipment);
                    sEqptType = "A";
                }
                else
                {
                    sEqptID = Util.GetCondition(cboEquipment_Machine);
                    sEqptType = "M";
                }

                string sJobDate = Util.GetCondition(ldpDatePicker);
                string sShiftCode = Util.GetCondition(cboShift);

                Hashtable hash_color = new Hashtable();
                Hashtable hash_first_list = new Hashtable();
                Hashtable hash_list = new Hashtable();
                Hashtable hash_title = new Hashtable();
                Hashtable hash_loss_color = new Hashtable();

                #region ...[HashTable 초기화]
                hash_first_list.Clear();
                hash_title.Clear();
                hash_list.Clear();
                hash_color.Clear();
                #endregion

                #region ...[Data 조회]

                //-- 일자 별 초 단위(00) 조회 ( 주,야간은 10초 간격 , 전체는 20초 간격)
                dsEqptTimeList = GetEqptTimeList(sJobDate, sShiftCode);
                if (dsEqptTimeList.Tables["RSLTDT"] == null) return;

                //-- 설비 타이틀 명 조회
                DataTable dtEqptName = GetEqptName_Machine(sEqptID);
                hash_title = DataTableConverter.ToHash(dtEqptName);

                iEqptCnt = dtEqptName.Rows.Count;

                //-- 설비 가동 Trend 조회
                DataTable dtEqptLossList = GetEqptLossList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_list = rsToHash2(dtEqptLossList);

                //-- 설비 가동 Trend 조회 (일자 별 최초 가동 정보)
                DataTable dtEqptLossFirstList = GetEqptLossFirstList(sEqptID, sEqptType, sJobDate, sShiftCode);
                hash_first_list = DataTableConverter.ToHashByColName(dtEqptLossFirstList);

                #endregion

                #region ...[색지도 처리]
                int cnt = 0;
                int inc = 0;
                int nRow = 0;
                int nCol = 0;

                Hashtable hash_Merge = new Hashtable();     //--- 같은 시간  Merge 기능 용
                Hashtable hash_rs = new Hashtable();        //--- 설비 Trend 정보 임시 저장

                for (int k = 0; k < hash_title.Count; k++)
                {
                    string sTitle = dtEqptName.Rows[k][0].ToString();
                    string sID = (string)hash_first_list[sTitle];   //--- 처음 기준으로 색깔 지정                    
                    hash_color.Add(sTitle, sID);
                }

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                {
                    nCol = cnt + 1;
                    nRow = inc * (hash_title.Count) + inc;

                    //--- 시간 단위 셋팅 (10 분 단위로 스프레드 설정
                    string sEqptTimeList = dsEqptTimeList.Tables["RSLTDT"].Rows[i][0].ToString();

                    int nTime = int.Parse(sEqptTimeList.Substring(10, 2));
                    if ((i) % (cboShift.SelectedValue.ToString().Equals("") ? 30 : 60) == 0)
                    {
                        Label _lable = new Label();
                        if (nTime / 10 * 10 == 0)
                        {
                            _lable.Content = sEqptTimeList.Substring(8, 2) + ":00";
                        }
                        else
                        {
                            _lable.Content = (nTime / 10 * 10).ToString();
                        }
                        _lable.FontSize = 10;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 0, 0, 0);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        Grid.SetColumn(_lable, nCol);
                        Grid.SetRow(_lable, nRow);
                        Grid.SetColumnSpan(_lable, 30);

                        _grid.Children.Add(_lable);
                    }
                    //--- 연속적인 Data 설정
                    if (!hash_Merge.ContainsKey(nRow))
                    {
                        hash_Merge.Add(nRow, nRow);
                    }

                    hash_rs.Clear();

                    //--- 가동 Trend 대표 시간 가동상태 및 LOSS 코드 설정
                    if (hash_list.ContainsKey(sEqptTimeList))
                    {
                        hash_rs = (Hashtable)hash_list[sEqptTimeList];
                        for (int k = 0; k < hash_title.Count; k++)
                        {
                            string sTitle = dtEqptName.Rows[k][0].ToString();
                            string sID = (string)hash_rs[sTitle];
                            if (!string.IsNullOrEmpty(sID))
                            {
                                hash_color.Remove(sTitle);
                                hash_color.Add(sTitle, sID);
                            }
                        }
                    }
                    //--- 가동 Trend 스프레드 색깔 설정
                    for (int k = 0; k < hash_title.Count; k++)
                    {
                        string sTitle = dtEqptName.Rows[k][0].ToString();
                        nRow = k + inc * (hash_title.Count) + inc + 1;
                        string sStatus = (string)hash_color[sTitle];

                        System.Drawing.Color color = GetColor(sStatus);

                        Border _border = new Border();

                        _border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        _border.Margin = new Thickness(-1, 0, 0, 3);

                        int min = int.Parse(sEqptTimeList.Substring(10, 2));
                        int sec = int.Parse(sEqptTimeList.Substring(12, 2));

                        if (min % 20 == 0 && sec == 0)
                        {
                            _border.BorderBrush = new SolidColorBrush(Colors.Black);
                            _border.BorderThickness = new Thickness(1, 0, 0, 0);
                        }

                        Grid.SetColumn(_border, nCol);
                        Grid.SetRow(_border, nRow);

                        Hashtable org_set = new Hashtable();

                        org_set.Add("COL", nCol);
                        org_set.Add("ROW", nRow);
                        org_set.Add("COLOR", _border.Background);
                        org_set.Add("TIME", sEqptTimeList);
                        org_set.Add("STATUS", sStatus);
                        org_set.Add("EQPTID", sTitle);

                        _border.Tag = org_set;

                        _border.Name = "S" + sTitle.Replace("-", "_") + sEqptTimeList.ToString();

                        _border.MouseDown += _border_MouseDown;

                        _grid.Children.Add(_border);

                        _grid.RegisterName(_border.Name, _border);

                        if (cnt == 0)
                        {
                            string sEqptName = dtEqptName.Rows[k][1].ToString();

                            TextBlock _text = new TextBlock();
                            _text.Text = sEqptName;
                            _text.FontSize = 10;
                            _text.Margin = new Thickness(10, 0, 10, 0);
                            Grid.SetColumn(_text, 0);
                            Grid.SetRow(_text, nRow);

                            _grid.Children.Add(_text);
                        }
                    }

                    cnt++;
                    //--- 마지막 칼럼 인 경우 다음 Row 수 지정 (설비 건수 별)
                    if (cnt == 360)
                    {
                        cnt = 0;
                        inc++;
                    }
                }
                int iTotalRow = inc == 12 ? (hash_title.Count + 1) * inc : (hash_title.Count + 1) * (inc + 1);

                for (int k = 0; k < iTotalRow; k++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 부동 내역 조회
        /// - 시간 차이가 180초 이상 인 경우
        /// - OP Key-In 인 경우
        /// - LOSS CODE ( 38000)  자재교체인 경우
        /// 색깔 구분
        /// - 분홍색 (2)
        ///   : 180초 이상
        ///   : 기준정보 기준시간 초과인 경우 ( 시작시간이 0 인  기준정보 )
        /// - 회색 (1)
        ///   : OP Key-In
        ///   : 기준정보 기준시간 이내인 경우 ( 시작시간이 0 인  기준정보 )
        /// </summary>
        private void GetEqptLossDetailList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                /// Machine 설비수정 가능할 경우 - by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        dr["EQPTID"] = Util.GetCondition(cboEquipment);
                    }
                    else
                    {
                        dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine);
                    }
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEquipment);
                }
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["MIN_SECONDS"] = 180; // 설비 Loss 등록 화면에서는 설정해주지만 수정 화면에서는 설정 X

                RQSTDT.Rows.Add(dr);


                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_FOR_APPR", "RQSTDT", "RSLTDT", RQSTDT);
                //if (string.Equals(GetAreaType(), "P"))
                //{
                //    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                //}
                //else
                //{
                //    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_FOR_APPR", "RQSTDT", "RSLTDT", RQSTDT);
                //}

                if (cboShift.SelectedValue != null && !string.IsNullOrEmpty(cboShift.SelectedValue.GetString()))
                {
                    DateTime dJobDate_st = new DateTime();
                    DateTime dJobDate_ed = new DateTime();

                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift_st = drShift[0]["SHFT_STRT_HMS"].ToString();
                        String sShift_ed = drShift[0]["SHFT_END_HMS"].ToString();

                        dJobDate_st = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_st.Substring(0, 2) + ":" + sShift_st.Substring(2, 2) + ":" + sShift_st.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        dJobDate_ed = DateTime.ParseExact(Util.GetCondition(ldpDatePicker) + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);

                        //작업조의 end시간이 기준시간 보다 작을때
                        if (TimeSpan.Parse(sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2)) < DateTime.Parse(Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(0, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(2, 2) + ":" + Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Substring(4, 2)).TimeOfDay)
                        {
                            dJobDate_ed = DateTime.ParseExact(ldpDatePicker.SelectedDateTime.AddDays(1).ToString("yyyyMMdd") + " " + sShift_ed.Substring(0, 2) + ":" + sShift_ed.Substring(2, 2) + ":" + sShift_ed.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                        }
                    }

                    try
                    {
                        RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").CopyToDataTable();
                    }
                    catch (Exception ex)
                    {
                        DataTable dt = new DataTable();
                        foreach (DataColumn col in RSLTDT.Columns)
                        {
                            dt.Columns.Add(Convert.ToString(col.ColumnName));
                        }
                        RSLTDT = dt;
                    }
                }

                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);

                // 2019-09-30 황기근 사원 수정
                restrictSave();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetEqptLossChangeApprHistList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable RSLTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                /// Machine 설비수정 가능할 경우 - by 오화백 2023 03 16
                if (MachineEqptChkHist == "Y" && chkMainHist.IsChecked == false)
                {
                    if (cboEquipment_MachineHist.SelectedValue.GetString() == string.Empty)
                    {
                        dr["EQPTID"] = SelectEquipment(); // Util.GetCondition(cboEquipmentHist);
                    }
                    else
                    {
                        dr["EQPTID"] = Util.GetCondition(cboEquipment_MachineHist);
                    }
                }
                else
                {
                    dr["EQPTID"] = SelectEquipment(); // Util.GetCondition(cboEquipmentHist);
                }
                dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateTo);
                
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_APPROVAL_TARGET_MULTI_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgDetailHist, RSLTDT, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetNowDate()
        {
            string nowDate = string.Empty;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

            nowDate = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

            return nowDate;
        }

        private void GetEqptLossApprAuth()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["AREAID"] = Util.NVC(cboArea.SelectedValue);
                drRQSTDT["COM_TYPE_CODE"] = "EQPT_LOSS_CHG_APPR_USE_SYSTEM";    // 해당 동, 시스템의 '설비 Loss 수정' 화면 사용 여부 확인
                drRQSTDT["COM_CODE"] = LoginInfo.SYSID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (dtRSLTDT != null && dtRSLTDT.Rows.Count > 0)
                {
                    bUseEqptLossAppr = true;

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR1"])))
                    {
                        strAttr1 = Util.NVC(dtRSLTDT.Rows[0]["ATTR1"]);
                    }
                    else
                    {
                        strAttr1 = "1";
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(dtRSLTDT.Rows[0]["ATTR2"])))
                    {
                        strAttr2 = Util.NVC(dtRSLTDT.Rows[0]["ATTR2"]);
                    }
                    else
                    {
                        strAttr2 = "7";
                    }
                }
                else
                {
                    bUseEqptLossAppr = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 시간 목록
        /// </summary>
        private DataSet GetEqptTimeList(string sJobDate, string sShiftCode)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = sJobDate;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0) { }
            if (Convert.ToString(result.Rows[0]["JOBDATE_YYYYMMDD"]).Equals(""))
            {
                Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                return null;
            }

            DataSet ds = new DataSet();

            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                if (!bPack)
                {
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                }
                else
                {
                    // Pack
                    RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                    RQSTDT.Columns.Add("FROMDATE", typeof(string));
                    RQSTDT.Columns.Add("TODATE", typeof(string));
                }

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);

                if (!bPack)
                {
                    dr["PROCID"] = Util.GetCondition(cboProcess);
                    RQSTDT.Rows.Add(dr);

                    dtShift = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    dr["FROMDATE"] = sJobDate;
                    dr["TODATE"] = sJobDate;
                    RQSTDT.Rows.Add(dr);

                    dtShift = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);
                }

                DataTable RSLTDT = new DataTable("RSLTDT");

                RSLTDT.Columns.Add("STARTTIME", typeof(string));
                RSLTDT.Columns.Add("ENDTIME", typeof(string));

                int iTerm = 0;
                int iIncrease = 0;

                DateTime dJobDate = new DateTime();

                if (sShiftCode.Equals(""))
                {
                    String sShift = dtShift.Rows[0]["SHFT_STRT_HMS"].ToString();
                    if (sShift.Length > 0)
                    {
                        dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                    }
                    else
                    {
                        dJobDate = DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                    }
                    iTerm = 20;
                    iIncrease = 20;
                }
                else
                {
                    DataRow[] drShift = dtShift.Select("SHFT_ID='" + Util.GetCondition(cboShift) + "'", "");

                    if (drShift.Length > 0)
                    {
                        String sShift = drShift[0]["SHFT_STRT_HMS"].ToString();
                        dJobDate = DateTime.ParseExact(sJobDate + " " + sShift.Substring(0, 2) + ":" + sShift.Substring(2, 2) + ":" + sShift.Substring(4, 2), "yyyyMMdd HH:mm:ss", null);
                    }
                    else
                    {
                        dJobDate = (DateTime)result.Rows[0]["JOBDATE_YYYYMMDD"];//DateTime.ParseExact(sJobDate + " 06:00:00", "yyyyMMdd HH:mm:ss", null);
                    }

                    if (drShift[0]["SHFT_GR_CODE"].ToString().Equals(""))
                    {
                        Util.MessageValidation("SFU3442"); //  "해당 조의 교대 수가 없습니다.
                                                           // ds.Tables. // = null;
                        return ds;
                    }

                    iTerm = int.Parse(drShift[0]["SHFT_GR_CODE"].ToString()) * 10;
                    iIncrease = 10;
                }

                DataTable dtGetDate = new ClientProxy().ExecuteServiceSync("COR_SEL_GETDATE", null, "RSLTDT", null);

                for (int i = 0; i < 24 * 60 * 60 / iTerm; i++)
                {
                    RSLTDT.Rows.Add(dJobDate.AddSeconds(i * iIncrease).ToString("yyyyMMddHHmmss"), dJobDate.AddSeconds(i * iIncrease + (iIncrease - 1)).ToString("yyyyMMddHHmmss"));

                }//

                DataTable RSLTDT1 = RSLTDT.Select("STARTTIME <=" + Convert.ToDateTime(dtGetDate.Rows[0]["SYSTIME"]).ToString("yyyyMMddHHmmss"), "").CopyToDataTable();
                ds.Tables.Add(RSLTDT1);
                ds.Tables[0].TableName = "RSLTDT";
                return ds;
            }
            catch (Exception ex)
            {
                //---commMessage.Show(ex.Message);
                return ds;
            }
        }

        /// <summary>
        /// 설비명 가져오기
        /// </summary>
        private DataTable GetEqptName(string sEqptID, string sEqptType)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _grid_eqpt;
                dr["EQPTTYPE"] = sEqptType;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }


        /// <summary>
        ///  Machine 설비명 가져오기  - by 오화백 2023 03.16
        /// </summary>
        private DataTable GetEqptName_Machine(string sEqptID)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_EQPTTITLE_UNIT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }


        /// <summary>
        /// 시간별 상태 목록 가져오기
        /// </summary>
        private DataTable GetEqptLossList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                if (bPack)
                {
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                }

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;

                if (bPack)
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = Util.GetCondition(cboArea);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                }
                RQSTDT.Rows.Add(dr);

                if (bPack)
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP", "RQSTDT", "RSLTDT", RQSTDT);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;

        }

        /// <summary>
        /// 최초 상태 가져오기
        /// </summary>
        private DataTable GetEqptLossFirstList(string sEqptID, string sEqptType, string sJobDate, string sShiftCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));

                if (bPack)
                {
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                }

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = sEqptID;
                dr["EQPTTYPE"] = sEqptType;
                dr["WRK_DATE"] = sJobDate;
                dr["SHIFT"] = sShiftCode;

                if (bPack)
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = Util.GetCondition(cboArea);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                }
                RQSTDT.Rows.Add(dr);

                if (bPack)
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST", "RQSTDT", "RSLTDT", RQSTDT);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return RSLTDT;
        }

        private Hashtable rsToHash2(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Hashtable hash_rs = new Hashtable();
                    for (int j = 0; j < dt.Columns.Count - 1; j++)
                    {
                        hash_rs.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());
                    }
                    hash_return.Add(dt.Rows[i]["STARTTIME"].ToString(), hash_rs);
                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                hash_return = null;
            }
            return hash_return;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #region [### 설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                //Pack일 경우 PROCID Parameter 제외
                if (!bPack)
                    RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEquipmentSegment;

                if (!bPack)
                    dr["PROCID"] = Util.GetCondition(cboProcess);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                if (bPack)
                {
                    liProcId = new List<string>();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        liProcId.Add(dtResult.Rows[i]["PROCID"].ToString());
                    }

                    DataRow drIns = dtResult.NewRow();
                    drIns["CBO_NAME"] = "-SELECT-";
                    drIns["CBO_CODE"] = "";
                    dtResult.Rows.InsertAt(drIns, 0);
                }

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                //Pack일 경우 설비 초기 세팅 -SELECT-, 설비 변경 시 공정 변경 이벤트 활용을 위해서
                if (!LoginInfo.CFG_EQPT_ID.Equals("") && !bPack)
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### Shift 정보 가져오기]
        private void SetShift()
        {
            try
            {
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = sEquipmentSegment;
                dr["FROMDATE"] = Util.GetCondition(ldpDatePicker);
                dr["TODATE"] = Util.GetCondition(ldpDatePicker);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_PACK_LOSS", "RQSTDT", "RSLTDT", RQSTDT);

                cboShift.DisplayMemberPath = "SHFT_NAME";
                cboShift.SelectedValuePath = "SHFT_ID";

                DataRow drIns = dtResult.NewRow();
                drIns["SHFT_NAME"] = "-ALL-";
                drIns["SHFT_ID"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboShift.ItemsSource = dtResult.Copy().AsDataView();

                if (cboShift.SelectedIndex < 0)
                    cboShift.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region  Machine 설비 LOSS 가능 공정 확인 :  MachineEqpt_Loss_Modify_Chk()  by 오화백 2023.03.16
        /// <summary>
        /// Machine 설비 LOSS 가능 공정 확인
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        public void MachineEqpt_Loss_Modify_Chk(string Procid)
        {
            if (string.IsNullOrEmpty(Procid)) return;

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
                dr["COM_CODE"] = Procid;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    MachineEqptChk = "Y";
                }
                else
                {
                    MachineEqptChk = string.Empty;
                }
                // Main 체크 해제 및 Machine Loss 수정 가능여부 확인 
                if (chkMain.IsChecked == false && MachineEqptChk == "Y")
                {
                    Machine.Visibility = Visibility.Visible;
                }
                else
                {
                    Machine.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        private void MachineEqptHist_Loss_Modify_Chk(string ProcId)
        {
            if (string.IsNullOrEmpty(ProcId)) return;

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
                dr["COM_CODE"] = ProcId;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    MachineEqptChkHist = "Y";
                }
                else
                {
                    MachineEqptChkHist = string.Empty;
                }
                // Main 체크 해제 및 Machine Loss 수정 가능여부 확인 
                if (chkMainHist.IsChecked == false && MachineEqptChkHist == "Y")
                {
                    MachineHist.Visibility = Visibility.Visible;
                }
                else
                {
                    MachineHist.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }

        public static class GridBackColor
        {
            public static readonly System.Drawing.Color Color1 = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color Color2 = System.Drawing.Color.FromArgb(0, 0, 225);
            public static readonly System.Drawing.Color Color3 = System.Drawing.Color.FromArgb(185, 185, 185);

            public static readonly System.Drawing.Color Color4 = System.Drawing.Color.FromArgb(150, 150, 150);
            public static readonly System.Drawing.Color Color5 = System.Drawing.Color.FromArgb(255, 255, 155);
            public static readonly System.Drawing.Color Color6 = System.Drawing.Color.FromArgb(255, 127, 127);

            public static readonly System.Drawing.Color R = System.Drawing.Color.FromArgb(0, 255, 0);
            public static readonly System.Drawing.Color W = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color T = System.Drawing.Color.FromArgb(255, 0, 0);
            public static readonly System.Drawing.Color F = System.Drawing.Color.FromArgb(0, 0, 0);
            public static readonly System.Drawing.Color N = System.Drawing.Color.FromArgb(255, 255, 255);
            public static readonly System.Drawing.Color U = System.Drawing.Color.FromArgb(128, 128, 128);

            public static readonly System.Drawing.Color I = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color P = System.Drawing.Color.FromArgb(255, 255, 0);
            public static readonly System.Drawing.Color O = System.Drawing.Color.FromArgb(0, 0, 0);
        }

        private System.Drawing.Color GetColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            try
            {
                switch (sType)
                {
                    case "R":
                        color = GridBackColor.R;
                        break;
                    case "W":
                        color = GridBackColor.W;
                        break;
                    case "T":
                        color = GridBackColor.T;
                        break;
                    case "F":
                        color = GridBackColor.F;
                        break;
                    case "N":
                        color = GridBackColor.N;
                        break;
                    case "U":
                        color = GridBackColor.U;
                        break;
                    case "I":
                        color = GridBackColor.I;
                        break;
                    case "P":
                        color = GridBackColor.P;
                        break;
                    case "O":
                        color = GridBackColor.O;
                        break;
                    default:
                        if (sType.Equals(""))
                        {
                            color = System.Drawing.Color.White;
                        }
                        else
                        {
                            color = System.Drawing.Color.FromName(hash_loss_color[sType.Substring(1)].ToString());
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                color = System.Drawing.Color.White;
            }
            return color;
        }

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                //bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FORM_LOSS_CBO";
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["INCLUDE_GROUP"] = "AC";

                inTable.Rows.Add(dr);
            }
            else
            {
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                inTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        //private void SetEquipmentSegmentCombo(MultiSelectionBox cbo)
        //{
        //    cbo.ItemsSource = null;

        //    string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

        //    DataTable inTable = new DataTable("RQSTDT");
        //    inTable.Columns.Add("LANGID", typeof(string));
        //    inTable.Columns.Add("AREAID", typeof(string));

        //    DataRow dr = inTable.NewRow();
        //    dr["LANGID"] = LoginInfo.LANGID;
        //    dr["AREAID"] = cboArea.SelectedValue;

        //    inTable.Rows.Add(dr);


        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

        //    cbo.DisplayMemberPath = "CBO_NAME";
        //    cbo.SelectedValuePath = "CBO_CODE";

        //    if (CommonVerify.HasTableRow(dtResult))
        //    {
        //        cbo.ItemsSource = DataTableConverter.Convert(dtResult);
        //    }
        //}

        private void SetProcessCombo(C1ComboBox cbo)
        {
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F") // 활성화이면
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                string bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            //Machine 설비 Loss 수정 가능 여부  by 오화백 2023 03.16
            MachineEqpt_Loss_Modify_Chk(cboProcess.SelectedValue.GetString());
        }

        //private void SetProcessCombo(MultiSelectionBox cbo)
        //{
        //    cbo.ItemsSource = null;

        //    const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
        //    DataTable inTable = new DataTable("RQSTDT");
        //    inTable.Columns.Add("LANGID", typeof(string));
        //    inTable.Columns.Add("EQSGID", typeof(string));

        //    DataRow dr = inTable.NewRow();
        //    dr["LANGID"] = LoginInfo.LANGID;
        //    dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
        //    inTable.Rows.Add(dr);

        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

        //    cbo.DisplayMemberPath = "CBO_NAME";
        //    cbo.SelectedValuePath = "CBO_CODE";

        //    cbo.ItemsSource = dtResult.Copy().AsDataView();


        //    //Machine 설비 Loss 수정 가능 여부  by 오화백 2023 03.16
        //    MachineEqpt_Loss_Modify_Chk(cboProcess.SelectedValue.GetString());
        //}

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
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

        private void SetShiftCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }


        /// <summary>
        /// Machine 설비 조회  - by 오화백 2023.02.20
        /// </summary>
        /// <param name="cbo"></param>
        private void SetMachineEqptCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_MACHINE_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
            dr["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) ? null : cboEquipment.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetEquipmentSegmentHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaHist.SelectedValue;
                dr["INCLUDE_GROUP"] = "AC";

                inTable.Rows.Add(dr);
            }
            else
            {
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaHist.SelectedValue;

                inTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        private void SetEquipmentSegmentHistCombo(MultiSelectionBox cbo)
        {
            cbo.ItemsSource = null;

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

           
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboAreaHist.SelectedValue;

            inTable.Rows.Add(dr);
          

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();
        }

        private void SetProcessHistComboFCS(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegmentHist.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }

            //Machine 설비 Loss 수정 가능 여부  by 오화백 2023 03.16
            MachineEqptHist_Loss_Modify_Chk(cboProcessHist.SelectedValue.GetString());
        }

        private void SetProcessHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string str = string.Empty;
            str = SelectEquipmentSegmentHist1();

            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = str;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            if (CommonVerify.HasTableRow(dtResult))
            {
                cbo.ItemsSource = dtResult.Copy().AsDataView();
            }

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }

            //Machine 설비 Loss 수정 가능 여부  by 오화백 2023 03.16
            MachineEqptHist_Loss_Modify_Chk(cboProcessHist.SelectedValue.GetString());
        }

        private void SetEquipmentHistCombo(MultiSelectionBox cbo)
        {
            cbo.ItemsSource = null;
            //cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_MULTI_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                dr["EQSGID"] = cboEquipmentSegmentHist.SelectedValue;
            }
            else
            {
                dr["EQSGID"] = SelectEquipmentSegmentHist1();
            }
            dr["PROCID"] = cboProcessHist.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            if (CommonVerify.HasTableRow(dtResult))
            {
                cbo.ItemsSource = DataTableConverter.Convert(dtResult); //dtResult.Copy().AsDataView();
            }
            //if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
            //{
            //    cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

            //    if (cbo.SelectedIndex < 0)
            //        cbo.SelectedIndex = 0;
            //}
            //else
            //{
            //    cbo.SelectedIndex = 0;
            //}
        }

        private void SetShiftHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = string.IsNullOrEmpty(cboAreaHist.SelectedValue.GetString()) ? null : cboAreaHist.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegmentHist.SelectedValue.GetString()) ? null : cboEquipmentSegmentHist.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcessHist.SelectedValue.GetString()) ? null : cboProcessHist.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }


        /// <summary>
        /// Machine 설비 조회  - by 오화백 2023.02.20
        /// </summary>
        /// <param name="cbo"></param>
        private void SetMachineEqptHistCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_MACHINE_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboAreaHist.SelectedValue.GetString()) ? null : cboAreaHist.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegmentHist.SelectedValue.GetString()) ? null : cboEquipmentSegmentHist.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcessHist.SelectedValue.GetString()) ? null : cboProcessHist.SelectedValue.GetString();
            dr["EQPTID"] = SelectEquipment();// string.IsNullOrEmpty(cboEquipmentHist.SelectedValue.GetString()) ? null : cboEquipmentHist.SelectedValue.GetString();
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow newRow = dtResult.NewRow();
            newRow["CBO_CODE"] = "";
            newRow["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(newRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        private void SetApprUserCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_PRD_SEL_EQPT_LOSS_CHG_APPR_USER";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("SYSID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? LoginInfo.CFG_AREA_ID : cboArea.SelectedValue.GetString();
            dr["SYSID"] = LoginInfo.SYSID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.DisplayMemberPath = "USERNAME";
            cbo.SelectedValuePath = "USERID";
            cbo.SelectedIndex = 0;
        }

        private void SetUpperLossCodeGridCombo()
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "UPPR_LOSS_CODE";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", dt);

            C1.WPF.DataGrid.DataGridComboBoxColumn dgcol = dgDetail.Columns["UPPER_LOSS_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            dgcol.ItemsSource = dtResult.Copy().Select("CBO_CODE = '10000'").CopyToDataTable().AsDataView();
            dgcol.DisplayMemberPath = "CBO_NAME";
            dgcol.SelectedValuePath = "CBO_CODE";
        }

        private void SetLossCodeGridCombo()
        {
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("LANGID", typeof(string));
            //dt.Columns.Add("USERID", typeof(string));
            //dt.Columns.Add("ALLFLAG", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            //dr["USERID"] = LoginInfo.USERID;
            //dr["ALLFLAG"] = "Y";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_LOSSCODE_FOR_APPR_CBO", "RQSTDT", "RSLTDT", dt);
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { "CBO_CODE", "CBO_NAME" });

            C1.WPF.DataGrid.DataGridComboBoxColumn dgcolz = dgDetail.Columns["APPR_REQ_LOSS_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            dgcolz.ItemsSource = dtResult.Copy().AsDataView();
            dgcolz.DisplayMemberPath = "CBO_NAME";
            dgcolz.SelectedValuePath = "CBO_CODE";
        }

        private void SetLossDetlCodeGridCombo()
        {
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("LANGID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_LOSS_DETL_CODE_FOR_APPR_CBO", "RQSTDT", "RSLTDT", dt);
            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { "CBO_CODE", "CBO_NAME" });

            C1.WPF.DataGrid.DataGridComboBoxColumn dgcolz = dgDetail.Columns["APPR_REQ_LOSS_DETL_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            dgcolz.ItemsSource = dtResult.Copy().AsDataView();
            dgcolz.DisplayMemberPath = "CBO_NAME";
            dgcolz.SelectedValuePath = "CBO_CODE";
        }

        private void SetTroubleUnitColumnDisplay()
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["EQPT_LOSS_UNIT_ALARM_DISP_FLAG"].GetString() == "Y")
                    {
                        //2022.11.23
                        //dgDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Visible;
                        //dgDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Visible;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                dgDetail.Columns["TROUBLE_CAUSE_EQPTID"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_EQPTNAME"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_ALARMCODE"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["TROUBLE_CAUSE_ALARMNAME"].Visibility = Visibility.Collapsed;
                Util.MessageException(ex);

            }
        }

        private void SelectRemarkMandatory()
        {
            try
            {
                dtRemarkMandatory = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["CMCDTYPE"] = "EQPT_LOSS_REMARK_MANDATORY";
                row["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                row["PROCID"] = Util.GetCondition(cboProcess);

                dt.Rows.Add(row);

                dtRemarkMandatory = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_REMARK_MANDATORY", "INDATA", "RSLT", dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //운영설비도 Split되는지 확인
        private void SelectLossRunArea()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["CMCDTYPE"] = "LOSS_RUN_AREA";
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_LOSSRUNAREA", "INDATA", "RSLT", dt);

                RunSplit = result.Rows.Count == 0 ? "N" : "Y";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void restrictSave()
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "LOSS_MODIFY_RESTRICT_SHOP";
            RQSTDT1.Rows.Add(row);

            DataTable shopList = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT1);
            DataRow[] shop = shopList.Select("CBO_Code = '" + LoginInfo.CFG_SHOP_ID + "'");

            if (shop.Count() > 0)   // LOSS 수정 제한하는 PLANT일 때
            {
                DateTime pickedDate = ldpDatePicker.SelectedDateTime;
                if (pickedDate.ToString("yyyy-MM-dd").Equals(DateTime.Today.ToString("yyyy-MM-dd")))
                    return;

                DateTime dtCaldate = Convert.ToDateTime(AreaTime.Rows[0]["JOBDATE_YYYYMMDD"]);
                string sCaldate = dtCaldate.ToString("yyyy-MM-dd");
                //btnSave.IsEnabled = true;
                //btnTotalSave.IsEnabled = true;

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.Columns.Add("USERID", typeof(string));
                RQSTDT2.Columns.Add("AUTHID", typeof(string));

                DataRow row2 = RQSTDT2.NewRow();
                row2["USERID"] = LoginInfo.USERID;
                row2["AUTHID"] = "EQPTLOSS_MGMT";
                RQSTDT2.Rows.Add(row2);

                DataTable auth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT2);

                if (pickedDate.ToString("yyyy-MM-dd").Equals(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")))
                {
                    TimeSpan due = DateTime.Parse(Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(0, 2) + ":" + Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(2, 2) + ":" + Convert.ToString(shop[0]["ATTRIBUTE1"]).Substring(4, 2)).TimeOfDay;
                    TimeSpan searchTime = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;
                    if (searchTime >= due && auth.Rows.Count <= 0)
                    {
                        //btnSave.IsEnabled = false;
                        //btnTotalSave.IsEnabled = false;
                    }
                }
                else
                {
                    if (auth.Rows.Count <= 0)
                    {
                        //btnSave.IsEnabled = false;
                        //btnTotalSave.IsEnabled = false;
                    }
                }
            }
        }



        private bool SaveValidation()
        {
            this.isOnlyRemarkSave = false;

            if (!event_valridtion())
            {
                return false;
            }

            //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
            //if (dtRemarkMandatory != null && dtRemarkMandatory.Rows.Count > 0)
            //{
            //    string sLoss = Util.GetCondition(cboLoss);
            //    string sLossDetl = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

            //    DataRow[] rows = dtRemarkMandatory.Select("ATTRIBUTE3 = '" + sLoss + "' AND ATTRIBUTE4 = '" + sLossDetl + "'");

            //    if (rows.Length > 0)
            //    {
            //        int iLength = int.Parse(rows[0]["ATTRIBUTE5"].ToString());
            //        string sLossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();

            //        //옵션에 지정된 길이가 0보다 큰 값이고 옵션에 지정된 길이보다 짧은 글자가 입력되면
            //        if (iLength > 0 && (string.IsNullOrEmpty(sLossNote) || sLossNote.Length < iLength))
            //        {
            //            Util.MessageValidation("SFU3801", new object[] { iLength });  //비고를 %1자 이상 입력해 주세요.
            //            rtbLossNote.Focus();
            //            return false;
            //        }
            //    }
            //}

            if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
            {
                Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                return false;
            }

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return false;
            }

            // C20210213-000002 : 폴란드 PACK에 대하여 신규 권한 추가 | 김건식
            if (bPack && LoginInfo.CFG_SHOP_ID.Equals("G481"))
            {
                if (!CeckPackProcessing())
                {
                    Util.MessageValidation("SFU8333"); // 전일에 대하여 저장기능을 사용할 수 없는 사용자입니다. \n권한이 있는 사용자에게 문의 하십시오.
                    return false;
                }

            }

            //if (!CeckAvailableDate())
            //{
            //    Util.MessageInfo("SFU5175"); // 3일 전 LOSS건 까지만 수정 승인 요청이 가능합니다.
            //    return false;
            //}
            if (bUseEqptLossAppr)
            {
                if (ValidationChkDDay().Equals("ALL"))
                {
                    Util.MessageValidation("SFU5181"); //당일 설비 Loss 등록은 설비 Loss 등록 화면에서 등록가능합니다.
                    return false;
                }

                if (ValidationChkDDay().Equals("AUTH_ONLY") && LoginInfo.USERTYPE.Equals("P"))
                {
                    Util.MessageValidation("SFU5179"); // 공용 PC 사용자권한으로는 Loss 등록이 불가합니다. \n개인권한 사용자로 로그인하여 등록해주시기 바랍니다. 
                    return false;
                }

                if (ValidationChkDDay().Equals("NO_REG"))
                {
                    string strParam = (Convert.ToDouble(strAttr2) + 1).ToString();
                    Util.MessageValidation("SFU5175", ObjectDic.Instance.GetObjectName(strParam)); // %1일 전 LOSS건 까지만 수정 승인 요청이 가능합니다. 
                    return false;
                }
            }

            ValidateNonRegisterLoss("ONE");

            // 설비 Check
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) || cboEquipment.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageInfo("SFU1153"); // 설비를 선택하세요
                return false;
            }

            // 승인 선택 Check
            if (string.IsNullOrEmpty(cboApprUser.Text))
            {
                Util.MessageInfo("SFU1692"); // 승인자가 필요합니다.
                return false;
            }

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    string lossCode = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "APPR_REQ_LOSS_CODE"));
                    string lossDetlCode = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "APPR_REQ_LOSS_DETL_CODE"));

                    if(string.IsNullOrEmpty(lossCode))
                    {
                        Util.MessageInfo("SFU3513"); // LOSS는필수항목입니다
                        return false;
                    }

                    if (string.IsNullOrEmpty(lossDetlCode))
                    {
                        Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                        return false;
                    }

                    if (DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "END_DTTM") == null)
                    {
                        Util.MessageValidation("SFU1912"); // 종료시간을 확인 하세요.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool event_valridtion()
        {
            //Machine 설비 사용 체크 by 오화백 2023 02 20
            if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
            {
                if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                {
                    if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
                    {
                        // 질문1 조회된 데이터가 없습니다.
                        Util.MessageValidation("SFU1905");
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Util.GetCondition(cboEquipment_Machine)) || Util.GetCondition(cboEquipment_Machine).Equals(""))
                    {
                        // 질문1 조회된 데이터가 없습니다.
                        Util.MessageValidation("SFU1905");
                        return false;
                    }

                }
            }
            else
            {
                if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
                {
                    // 질문1 조회된 데이터가 없습니다.
                    Util.MessageValidation("SFU1905");
                    return false;
                }
            }

            return true;
        }

        private string VadliationERPEnd()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("WRKDATE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = cboArea.SelectedValue.ToString();
            dr["WRKDATE"] = Util.GetCondition(ldpDatePicker);
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_LOSS_CLOSE", "RQSTDT", "RSLT", dt);

            if (result.Rows.Count != 0)
            {
                return Convert.ToString(result.Rows[0]["ERP_CLOSING_FLAG"]);
            }

            return "OPEN";
        }

        private void ValidateNonRegisterLoss(string saveType)
        {
            try
            {
                int idx = -1;
                if (saveType.Equals("TOTAL"))
                {
                    //제일 마지막에 클릭한 index찾기
                    for (int i = dgDetail.GetRowCount() - 1; i > 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            idx = i;
                            break;
                        }
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));

                DataRow row = dt.NewRow();
                //Machine 설비 사용 체크 by 오화백 2023 03 16
                if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                {
                    if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                    }
                    else
                    {
                        row["EQPTID"] = Convert.ToString(cboEquipment_Machine.SelectedValue);
                    }
                }
                else
                {
                    row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                }
                //row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) : ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + txtStart.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSETAIL_VALID", "RQSTDT", "RSLT", dt);

                if (result.Rows.Count != 0)
                {
                    Util.MessageValidation("SFU3515", Convert.ToString(result.Rows[0]["STRT_DTTM"]));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string ValidationChkDDay()
        {
            string bDDay = string.Empty;
            DateTime dtNowDay = DateTime.ParseExact(GetNowDate(), "yyyyMMdd", null);
            DateTime dtSearchDay = DateTime.ParseExact(sSearchDay, "yyyyMMdd", null);

            if ((dtNowDay - dtSearchDay).TotalDays >= 0 && (dtNowDay - dtSearchDay).TotalDays < Convert.ToDouble(strAttr1))
            {
                bDDay = "ALL";
            }
            else if ((dtNowDay - dtSearchDay).TotalDays >= Convert.ToDouble(strAttr1) && (dtNowDay - dtSearchDay).TotalDays <= Convert.ToDouble(strAttr2))
            {
                bDDay = "AUTH_ONLY";
            }
            else
            {
                bDDay = "NO_REG";
            }

            return bDDay;
        }

        private bool ValidateLossEndDttm(string eqptId, string startDttm)
        {
            DataTable tmp = new DataTable();
            tmp.Columns.Add("EQPTID", typeof(string));
            tmp.Columns.Add("STRT_DTTM_YMDHMS", typeof(string));

            DataRow dr = tmp.NewRow();
            dr["EQPTID"] = eqptId;
            dr["STRT_DTTM_YMDHMS"] = startDttm;
            tmp.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_END_DTTM", "RQSTDT", "RSLTDT", tmp);
            if (result.Rows.Count != 0)
            {
                if (Convert.ToString(result.Rows[0]["END_DTTM"]).Equals(""))
                {
                    return false;
                }
            }
            return true;
        }

        private void SaveProcess()
        {
            DataSet ds = new DataSet();
            DataTable RQSTDT = ds.Tables.Add("INDATA");
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("LOSS_SEQNO", typeof(string));
            RQSTDT.Columns.Add("APPR_STAT", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("APPR_REQ_LOSS_CNTT", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("APPR_USERID", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("TRBL_CODE", typeof(string));
            RQSTDT.Columns.Add("EIOSTAT", typeof(string));

            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = RQSTDT.NewRow();

                    //Machine 설비 사용 체크 by 오화백 2023.03.16
                    if (MachineEqptChk == "Y" && chkMain.IsChecked == false)
                    {
                        if (cboEquipment_Machine.SelectedValue.GetString() == string.Empty)
                        {
                            dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                        }
                        else
                        {
                            dr["EQPTID"] = Util.GetCondition(cboEquipment_Machine, "SFU3514"); //설비는필수입니다.
                        }
                    }
                    else
                    {
                        dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                    }

                    dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                    dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));//DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    dr["LOSS_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "PRE_LOSS_SEQNO"));
                    dr["APPR_STAT"] = "W"; // 승인 대기 상태로 INSERT
                    dr["APPR_REQ_LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "APPR_REQ_LOSS_CODE"));
                    dr["APPR_REQ_LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "APPR_REQ_LOSS_DETL_CODE"));
                    dr["APPR_REQ_LOSS_CNTT"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "APPR_REQ_LOSS_CNTT"));
                    dr["USERID"] = LoginInfo.USERID;
                    dr["APPR_USERID"] = Util.NVC(Util.GetCondition(cboApprUser));
                    dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_CODE"));
                    dr["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOSS_DETL_CODE"));
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    dr["TRBL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "TRBL_CODE"));
                    dr["EIOSTAT"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "EIOSTAT"));

                    if (string.Equals(GetAreaType(), "P"))
                    {
                        //dr["WRK_USERNAME"] = txtPerson.Tag;
                    }
                    RQSTDT.Rows.Add(dr);
                }
            }

            try
            {
                new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_INS_APPR", "INDATA", null, ds);

                //if (string.Equals(GetAreaType(), "P"))
                //{
                //    //new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL_PACK", "INDATA", null, ds);
                //}
                //else
                //{
                //    new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_INS_APPR", "INDATA", null, ds);
                //}

                //this.SaveQA(ds);        // 질문지 저장.

                //INSERT 처리후 재조회
                btnSearch_Click(null, null);

                Util.MessageInfo("SFU1747");  //요청되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        
        private bool CeckAvailableDate()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

                string sNowDay = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

                // 작업자가 당일로 조회 날짜를 변경하고 저장할수 있으므로 조회당시(Search 버튼 이벤트)의 날짜를 가져와 비교함.
                /*
                if(sNowDay.Equals(sSearchDay) || String.IsNullOrEmpty(sSearchDay))
                {
                    return true;
                }
                */

                // -7 일 까지 수정 가능 (-3일에서 변경)
                DateTime dtNowDay = DateTime.ParseExact(sNowDay, "yyyyMMdd", null);
                DateTime dtSearchDay = DateTime.ParseExact(sSearchDay, "yyyyMMdd", null);
                
                if (((dtNowDay - dtSearchDay).TotalDays > 7) || String.IsNullOrEmpty(sSearchDay))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// return true : 정상작업, false : 작업 불가능
        /// " * 폴란드 PACK에 대해서만" 전일의 설비 LOSS 저장할수 있는 신규 권한을 추가하여 관리함.
        /// 신규 쓰기 권한을 가지고 있는 인원은 전일 / 당일 모두 저장기능 사용 가능
        /// 기존 쓰기 권한을 가지고 있는 인원은 당일의 데이터만 저장기능 가능
        /// </summary>
        private Boolean CeckPackProcessing()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

                string sNowDay = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

                // 작업자가 당일로 조회 날짜를 변경하고 저장할수 있으므로 조회당시(Search 버튼 이벤트)의 날짜를 가져와 비교함.
                /*
                if(sNowDay.Equals(sSearchDay) || String.IsNullOrEmpty(sSearchDay))
                {
                    return true;
                }
                */

                // 전기일 -2 일 까지 수정 가능 | 홍기룡
                DateTime dtNowDay = DateTime.ParseExact(sNowDay, "yyyyMMdd", null);
                DateTime dtSearchDay = DateTime.ParseExact(sSearchDay, "yyyyMMdd", null);

                if (((dtNowDay - dtSearchDay).TotalDays < 3) || String.IsNullOrEmpty(sSearchDay))
                {
                    return true;
                }

                if (!bMPPD)
                {
                    COM001_014_AUTH_PERSON wndPerson = new COM001_014_AUTH_PERSON();
                    wndPerson.FrameOperation = this.FrameOperation;

                    if (wndPerson != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = "PACK_LOSS_ENGR_CWA";
                        C1WindowExtension.SetParameters(wndPerson, Parameters);

                        wndPerson.Closed += new EventHandler(wndUser_Closed);
                        wndPerson.ShowModal();
                        wndPerson.CenterOnScreen();
                        wndPerson.BringToFront();
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }

        #endregion

        private void dgDetail_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
            string bizRuleName = string.Empty;   // 2023.08.24 임시혁. 변경 Loss 분류 콤보를 설비 Loss 등록 Loss 분류 콤보와 동일하게 설정하기 위해 추가.
            
            if (cbo == null)
            {
                return;
            }

            if (Convert.ToString(e.Column.Name) == "APPR_REQ_LOSS_CODE")
            {
                if (Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "APPR_STAT")).Equals("W"))
                {
                    return;
                }

                cbo.ItemsSource = null;

                string upperLossCode = Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "UPPER_LOSS_CODE"));


                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // 2023.08.24. 임시혁. 변경 Loss 분류 조회기능을 설비 Loss 등록화면의 Loss 분류 콤보와 동일하게 설정하기위해 변경함.
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //-------------------------------------------------------------------------------------------------------------------
                // Old Start
                //-------------------------------------------------------------------------------------------------------------------
                //DataTable dt = new DataTable();
                //dt.TableName = "RQSTDT";
                //dt.Columns.Add("LANGID", typeof(string));
                //dt.Columns.Add("UPPR_LOSS_CODE", typeof(string));
                ////dt.Columns.Add("USERID", typeof(string));
                ////dt.Columns.Add("ALLFLAG", typeof(string));

                //DataRow dr = dt.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["UPPR_LOSS_CODE"] = upperLossCode;
                ////dr["USERID"] = LoginInfo.USERID;
                ////dr["ALLFLAG"] = "N";
                //dt.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_LOSSCODE_FOR_APPR_CBO", "RQSTDT", "RSLTDT", dt);
                //DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { "CBO_CODE", "CBO_NAME"});
                //cbo.SelectedValuePath = "CBO_CODE";
                //cbo.DisplayMemberPath = "CBO_NAME";
                //cbo.ItemsSource = dtBinding.Copy().AsDataView();
                //cbo.SelectedIndex = 0;
                //-------------------------------------------------------------------------------------------------------------------
                // Old End
                //-------------------------------------------------------------------------------------------------------------------

                //-------------------------------------------------------------------------------------------------------------------
                // New Start
                //-------------------------------------------------------------------------------------------------------------------
                // DA_BAS_SEL_EQPTLOSSCODE_CBO_PROC_PACK_ALL에서는 Loss Level1이 선택 안된 상태에서 콤보 클릭시에는 Level1에 상관없는 데이타가 조회됨.
                //    --> Level1 선택안한 경우 비조업, 비가동 등의 Level2가 조회되어 출력됨.
                // (폼로딩이후)Loss Level1 선택안한경우에는 출력되지 않도록 Level1 선택시에만만 나오도록 방어코드 추가.
                // 2023.08.28. 임시혁
                if (Util.NVC(upperLossCode).Equals(""))
                {
                    upperLossCode = "-";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("UPPR_LOSS_CODE", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.GetString();
                dr["PROCID"] = cboProcess.SelectedValue.GetString();
                dr["EQPTID"] = cboEquipment.SelectedValue.GetString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.GetString();
                dr["UPPR_LOSS_CODE"] = upperLossCode;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                if (LoginInfo.CFG_AREA_ID == "P1" || LoginInfo.CFG_AREA_ID == "P2" || LoginInfo.CFG_AREA_ID == "P6")
                {
                    if (bUseEqptLossAppr && !string.IsNullOrEmpty(strAttr1) && !string.IsNullOrEmpty(strAttr2))
                    {
                        bizRuleName = "DA_BAS_SEL_EQPTLOSSCODE_CBO_PROC_PACK_ALL";
                    }
                    else
                    {
                        bizRuleName = "DA_BAS_SEL_EQPTLOSSCODE_CBO_PROC_PACK";
                    }
                }
                else
                {
                    if (bUseEqptLossAppr && !string.IsNullOrEmpty(strAttr1) && !string.IsNullOrEmpty(strAttr2))
                    {
                        bizRuleName = "DA_BAS_SEL_EQPTLOSSCODE_CBO_ALL";
                    }
                    else
                    {
                        bizRuleName = "DA_BAS_SEL_EQPTLOSSCODE_CBO_PROC";
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { "CBO_CODE", "CBO_NAME" });
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.ItemsSource = dtBinding.Copy().AsDataView();
                cbo.SelectedIndex = 0;

                //-------------------------------------------------------------------------------------------------------------------
                // New End
                //-------------------------------------------------------------------------------------------------------------------
            }

            if (Convert.ToString(e.Column.Name) == "APPR_REQ_LOSS_DETL_CODE")
            {
                if (Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "APPR_STAT")).Equals("W"))
                {
                    return;
                }

                cbo.ItemsSource = null;

                string lossCode = Convert.ToString(DataTableConverter.GetValue(e.Row.DataItem, "APPR_REQ_LOSS_CODE"));

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // 2023.08.24. 임시혁. 변경 부동내용 조회기능을 설비 Loss 등록화면의 부동내용과 동일하게 설정하기위해 변경함.
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //-------------------------------------------------------------------------
                // Old Start
                //-------------------------------------------------------------------------
                //DataTable dt = new DataTable();
                //dt.TableName = "RQSTDT";
                //dt.Columns.Add("LANGID", typeof(string));
                //dt.Columns.Add("LOSS_CODE", typeof(string));

                //DataRow dr = dt.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["LOSS_CODE"] = lossCode;
                //dt.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQPT_LOSS_DETL_CODE_FOR_APPR_CBO", "RQSTDT", "RSLTDT", dt);
                //DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { "CBO_CODE", "CBO_NAME" });
                //cbo.SelectedValuePath = "CBO_CODE";
                //cbo.DisplayMemberPath = "CBO_NAME";
                //cbo.ItemsSource = dtBinding.Copy().AsDataView();
                //cbo.SelectedIndex = 0;
                //-------------------------------------------------------------------------
                // Old End
                //-------------------------------------------------------------------------


                //-------------------------------------------------------------------------
                // New Start
                //-------------------------------------------------------------------------
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LOSS_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.GetString();
                dr["PROCID"] = cboProcess.SelectedValue.GetString();
                dr["EQPTID"] = cboEquipment.SelectedValue.GetString();
                dr["LOSS_CODE"] = lossCode;
                dt.Rows.Add(dr);

                bizRuleName = "DA_BAS_SEL_EQPTLOSSDETLCODE_CBO";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { "CBO_CODE", "CBO_NAME" });
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.ItemsSource = dtBinding.Copy().AsDataView();
                cbo.SelectedIndex = 0;
                //-------------------------------------------------------------------------
                // New End
                //-------------------------------------------------------------------------


            }

            // 설비 Loss 앞 Level 선택시 뒷 콤보 Clear
            cbo.EditCompleted += delegate (object sender1, EventArgs e1)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.UpdateRowView2(e.Row, e.Column);
                }));

            };
        }

        // 설비 Loss 앞 Level 선택시 뒷 콤보 Clear
        private void UpdateRowView2(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "UPPER_LOSS_CODE")
                {
                    drv["APPR_REQ_LOSS_CODE"] = string.Empty;
                    drv["APPR_REQ_LOSS_DETL_CODE"] = string.Empty;
                    this.dgDetail.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "APPR_REQ_LOSS_CODE")
                {
                    drv["APPR_REQ_LOSS_DETL_CODE"] = string.Empty;
                    this.dgDetail.EndEditRow(true);
                }
            }
            finally
            {
            }
        }

        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            svMap.ScrollToVerticalOffset(10);
            //setMapColor(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "LIST", dgDetail.CurrentRow);
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                //txtPerson.Text = wndPerson.USERNAME;
                //txtPerson.Tag = wndPerson.USERID;
            }
        }

        private string SelectEquipment()
        {
            string sEqptID = string.Empty;

            for (int i = 0; i < cboEquipmentHist.SelectedItems.Count; i++)
            {
                if (i < cboEquipmentHist.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(cboEquipmentHist.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(cboEquipmentHist.SelectedItems[i]);
                }
            }

            return sEqptID;
        }

        private string SelectEquipmentSegmentHist1()
        {
            string sEqsgID = string.Empty;
            for (int i = 0; i < cboEquipmentSegmentHist1.SelectedItems.Count; i++)
            {
                if (i != cboEquipmentSegmentHist1.SelectedItems.Count - 1)
                {
                    sEqsgID += cboEquipmentSegmentHist1.SelectedItems[i] + ",";
                }
                else
                {
                    sEqsgID += cboEquipmentSegmentHist1.SelectedItems[i];
                }
            }

            return sEqsgID;
        }

        private void cboEquipmentSegmentHist1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                cboProcessHist.ItemsSource = null;
                cboProcessHist.Items.Clear();

                string str = string.Empty;

                str = SelectEquipmentSegmentHist1();

                if (string.IsNullOrEmpty(str)) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = str;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcessHist.DisplayMemberPath = "CBO_NAME";
                cboProcessHist.SelectedValuePath = "CBO_CODE";

                if (CommonVerify.HasTableRow(dtResult))
                {
                    cboProcessHist.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
                }

                if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
                {
                    cboProcessHist.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cboProcessHist.SelectedIndex < 0)
                    {
                        cboProcessHist.SelectedIndex = 0;
                    }
                }
                else
                {
                    cboProcessHist.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private bool SearchDatechk()
        {
            if ((ldpDateTo.SelectedDateTime - ldpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return true;
            }
            return false;
        }

        //private void cboProcessHist1_SelectionChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        cboEquipmentHist.ItemsSource = null;

        //        string strEqsg = string.Empty;
        //        for (int i = 0; i < cboEquipmentSegmentHist1.SelectedItems.Count; i++)
        //        {
        //            if (i != cboEquipmentSegmentHist1.SelectedItems.Count - 1)
        //            {
        //                strEqsg += cboEquipmentSegmentHist1.SelectedItems[i] + ",";
        //            }
        //            else
        //            {
        //                strEqsg += cboEquipmentSegmentHist1.SelectedItems[i];
        //            }
        //        }

        //        string str = string.Empty;
        //        for (int i = 0; i < cboProcessHist1.SelectedItems.Count; i++)
        //        {
        //            if (i != cboProcessHist1.SelectedItems.Count - 1)
        //            {
        //                str += cboProcessHist1.SelectedItems[i] + ",";
        //            }
        //            else
        //            {
        //                str += cboProcessHist1.SelectedItems[i];
        //            }
        //        }

        //        if (string.IsNullOrEmpty(str)) return;

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("EQSGID", typeof(string));
        //        RQSTDT.Columns.Add("PROCID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["EQSGID"] = strEqsg;
        //        dr["PROCID"] = str;
        //        RQSTDT.Rows.Add(dr);

        //        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_EQPT_LOSS_CBO", "RQSTDT", "RSLTDT", RQSTDT);
        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_MAIN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        cboEquipmentHist.DisplayMemberPath = "CBO_NAME";
        //        cboEquipmentHist.SelectedValuePath = "CBO_CODE";

        //        cboEquipmentHist.ItemsSource = DataTableConverter.Convert(dtResult);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }
}
