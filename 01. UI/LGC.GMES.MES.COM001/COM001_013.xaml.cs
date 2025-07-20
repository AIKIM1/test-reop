/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 관리
--------------------------------------------------------------------------------------
 [Change History]  
  2022.10.01     조영대     C20220627-000063     활성화 기능 추가 (Copy of COM001_014 - 변경집합 : 50720)
  2022.11.09     정연준                          활성화 조회 가능하도록 기능 수정
  2022.11.22     정연준                          활성화 조회 BIZ 변경 - 연동 설비인 PACK과 동일하게 변경 (WRK_USERNAME 추가 내용 제외)
  2022.12.23     정연준                          설비 LOSS 등록 부동내역 팝업 시 분류에 속한 LOSS만 조회 (조립GMES 수정 내용 동일 반영)
  2023.03.02     이윤중     E20230222-000245     설비 Loss 등록 - BM, PD, FCR Code 선택 기능 추가
  2023.03.07     이윤중     E20230222-000245     FCR 초기화, Loss코드, 부동내용 필수 등록으로 validation 수정
  2023.03.28     윤지해     E20230321-001518	 GMES FCR코드 BM입력 개선 CSR요청_GM자동차 조립만 적용
  2023.03.24     이윤중     E20230322-001797     [활성화] ESNB ESS LINE 로직 추가(공통코드 활용) - SetEquipmentSegmentCombo 호출 bizrule(BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO) 변경
  2023.04.06     최도훈                          cboLossEqsgProc, cboLastLoss null 예외처리 추가 
  2023.08.24     장영철                          GM2 예외로직 추가 (G673 조건 추가)
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
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_013 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        CommonCombo combo = new CommonCombo();

        Hashtable hash_loss_color = new Hashtable();

        DataTable dtMainList = new DataTable();

        DataTable AreaTime;

        DataTable dtShift;

        DataTable dtRemarkMandatory;

        DataTable dtQA;                 // CSR : C20220512-000432
        bool isOnlyRemarkSave = false;  // CSR : C20220512-000432
        bool isTotalSave = false;       // CSR : C20220512-000432

        String sMainEqptID;
        DataSet dsEqptTimeList = null;
        Util _Util = new Util();
        Hashtable org_set;

        bool bPack;
        bool bMPPD = false; // Modifiable person on the previous day : Pack 전용 전일 수정 가능자 (신규 권한)
        string sSearchDay = "";

        bool bFCS;

        List<string> liProcId;

        int iEqptCnt;

        string RunSplit; //동, 공정에 따라 RUN상태를 Split할 수 있는지 구분
        string _grid_eqpt = string.Empty;
        string _grid_area = string.Empty;
        string _grid_proc = string.Empty;
        string _grid_eqsg = string.Empty;
        string _grid_shit = string.Empty;

        // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        string _fTypeCode = "F";
        string _cTypeCode = "C";
        string _rTypeCode = "R";

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


        public COM001_013()
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
                InitUser();
            }
            if (bPack)
            {
                GetPackAuth();
            }
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

            if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            {
                bPack = false;
                chkMain.IsEnabled = true;

                // 22.11.09 활성화인 경우 추가
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                    bFCS = true;
                else
                    bFCS = false;

                /*
                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboShift };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboShift, cboProcess, cboEquipment };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cboProcessChild = { cboShift, cboEquipment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

                //설비
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);

                //작업조
                C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbParent: cboShiftParent);
                */

                SetAreaCombo(cboArea);
                SetEquipmentSegmentCombo(cboEquipmentSegment);

                //공정
                //SetProcessCombo(cboProcess);
                SetProcessFormCombo();

                //설비
                SetEquipmentCombo(cboEquipment);
                //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
                //combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent, sCase: "cboEquipmentEqptLoss");

                SetShiftCombo(cboShift);

                cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
                cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
                SetTroubleUnitColumnDisplay();
            }
            else
            {
                // Pack인 경우
                bPack = true;
                bFCS = false;

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

                cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
                //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
                ldpDatePicker.SelectedDataTimeChanged += ldpDatePicker_SelectedDataTimeChanged;

                //설비 변경 시 공정도 같이 변경 Event
                cboEquipment.SelectedItemChanged += CboEquipment_SelectedItemChanged;

                //공정은 개인이 선택 불가
                cboProcess.IsEnabled = false;
                //처음은 ALL선택
                cboProcess.SelectedIndex = 0;
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
                row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
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



        private void InitInsertCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //loss 코드
            ////C1ComboBox[] cboLossChild = { cboLossDetl };
            ////_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild);
            
            if (LoginInfo.CFG_AREA_ID == "P1" || LoginInfo.CFG_AREA_ID == "P2" || LoginInfo.CFG_AREA_ID == "P6")
            {
                //C1ComboBox[] cboLossChild = { cboLossDetl };
                //string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), Convert.ToString(cboEquipmentSegment.SelectedValue) };
                //_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeProcPack");
                string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue), Convert.ToString(cboEquipmentSegment.SelectedValue) };
                _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProcPack");
            }
            else
            {
                //C1ComboBox[] cboLossChild = { cboLossDetl };
                //string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
                //_combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: cboLossChild, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
                string[] sFilterLoss = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue), Convert.ToString(cboEquipment.SelectedValue) };
                _combo.SetCombo(cboLoss, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilterLoss, sCase: "cboLossCodeProc");
            }


            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경_주석처리
            //부동코드
            //C1ComboBox[] cboLossDetlParent = { cboLoss, cboEquipment };
            //string[] sFilter = { Convert.ToString(cboArea.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
            //_combo.SetCombo(cboLossDetl, CommonCombo.ComboStatus.SELECT, cbParent: cboLossDetlParent, sFilter: sFilter);

            ////현상코드
            //String[] sFilterFailure = { "F" };
            //_combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE");

            ////원인코드
            //String[] sFilterCause = { "C" };
            //_combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE");

            ////조치코드
            //String[] sFilterResolution = { "R" };
            //_combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE");
            #endregion

            //원인설비
            C1ComboBox[] cboOccurEqptParent = { cboEquipment };
            if (string.Equals(GetAreaType(), "P"))   //C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용) 김준겸 A
            {
                _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.SELECT, cbParent: cboOccurEqptParent);
            }
            else
            {
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    String[] sFilterEquipment = { /*cboEquipment.GetStringValue("MAIN_EQPTID")*/Util.GetCondition(cboEquipment) };
                    _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, sFilter: sFilterEquipment);
                }
                else
                {
                    _combo.SetCombo(cboOccurEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboOccurEqptParent);
                }                
            }

            //최근
            C1ComboBox[] cboLastLossParent = { cboEquipment };
            _combo.SetCombo(cboLastLoss, CommonCombo.ComboStatus.SELECT, cbParent: cboLastLossParent);

            //동-라인-공정별 로스 맵핑
            string[] sFilter1 = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString(cboProcess.SelectedValue) };
            _combo.SetCombo(cboLossEqsgProc, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1);


        }
        /// <summary>
        /// 색지도 그리드 초기화
        /// </summary>
        private void InitGrid()
        {
            try
            {
                //2018.12.24
                //int gridRowCount = 220;
                //2019.01.07
                //int gridRowCount = 400;
                //2020.05.20 김준겸 S 설비 LOSS 화면의 설비가 많을 경우 겹치는 현상으로 인한 RowCount조정.
                //int gridRowCount = 500;
                //_grid.Height = gridRowCount * 15;

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

                //for (int i = 0; i < gridRowCount; i++)
                //{
                //    RowDefinition gridRow1 = new RowDefinition();
                //    gridRow1.Height = new GridLength(15);
                //    _grid.RowDefinitions.Add(gridRow1);
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2020.05.14 김준겸 A 설비 Loss 작업자 등록 (PACK 부서만 활성화)
        /// </summary>
        private void InitUser()
        {
            // 뒷배경 비활성화
            bg_txtUser.Visibility = Visibility.Hidden;
            bg_txtPerson.Visibility = Visibility.Hidden;
            bg_btnPerson.Visibility = Visibility.Hidden;

            // 텍스트 박스,버튼 비활성화
            txtUser.Visibility = Visibility.Hidden;
            txtPerson.Visibility = Visibility.Hidden;
            btnPerson.Visibility = Visibility.Hidden;
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
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnTotalSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기



            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();

            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            //row["JOBDATE"] = ldpDatePicker.SelectedDateTime.ToShortDateString();
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


            this.Loaded -= UserControl_Loaded;
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
                //cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                SetEquipment();
                SetShift();
                //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
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

        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
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
            InitInsertCombo();

            ClearGrid();

            txtEqptName.Text = "";
            txtStart.Text = "";
            txtEnd.Text = "";
            txtStartHidn.Text = "";
            txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            //txtLossNote.Text = "";
            txtFCRCode.Text = "";
            txtPerson.Text = "";
            txtPerson.Tag = null;
            _grid_area = Util.GetCondition(cboArea);
            _grid_eqsg = Util.GetCondition(cboEquipmentSegment);
            _grid_eqpt = Util.GetCondition(cboEquipment);
            _grid_proc = Util.GetCondition(cboProcess);
            _grid_shit = Util.GetCondition(cboShift);

            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            //cboLossDetl.SelectedIndex = 0;
            popLossDetl.SelectedValue = string.Empty;
            popLossDetl.SelectedText = string.Empty;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;

            if (this.dtQA != null) this.dtQA.Clear();       // CSR : C20220512-000432
            this.isOnlyRemarkSave = false;                  // CSR : C20220512-000432
            this.isTotalSave = false;                       // CSR : C20220512-000432

            SelectRemarkMandatory();

            SelectLossRunArea();

            GetEqptLossRawList();

            SelectProcess();
            GetEqptLossDetailList();
            sMainEqptID = "A" + Util.GetCondition(cboEquipment);

            sSearchDay = ldpDatePicker.SelectedDateTime.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 색지도 내에서 클릭시 발생
        /// </summary>
        private void _border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //org_set.Add("COL", nCol);
            //org_set.Add("ROW", nRow);
            //org_set.Add("COLOR", _border.Background);
            //org_set.Add("TIME", sEqptTimeList);
            //org_set.Add("STATUS", sStatus);
            //org_set.Add("EQPTID", sTitle);
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

                            case "LossSplit":
                                item.Header = ObjectDic.Instance.GetObjectName("Loss분할");
                                item.Click -= lossSplit_Click;
                                item.Click += lossSplit_Click;
                                break;
                        }
                    }

                }
                if (RunSplit.Equals("Y"))
                {
                    if (org_set["STATUS"].ToString().Equals("R")) //추가
                    {
                        string startTime = txtStartHidn.Text;
                        string endTime = txtEndHidn.Text;
                        if (startTime.Equals("") || endTime.Equals(""))
                        {
                            return;
                        }



                        DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime).CopyToDataTable();
                        if (dt.Select("EIOSTAT <> 'R'").Count() > 0)
                        {
                            Util.MessageValidation("SFU3204"); //운영설비 사이에 Loss가 존재합니다.
                            btnReset_Click(null, null);
                            return;
                        }

                        COM001_014_RUN_SPLIT wndPopup = new COM001_014_RUN_SPLIT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[9];
                            Parameters[0] = org_set["EQPTID"].ToString();
                            Parameters[1] = org_set["TIME"].ToString();
                            Parameters[2] = startTime;
                            Parameters[3] = endTime;
                            Parameters[4] = cboArea.SelectedValue.ToString();
                            Parameters[5] = Util.GetCondition(ldpDatePicker); //ldpDatePicker.SelectedDateTime.ToShortDateString();
                            Parameters[6] = this;
                            Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[8] = cboProcess.SelectedValue.ToString();


                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                }

            }
            else
            {

                if (!org_set["EQPTID"].ToString().Equals(sMainEqptID))//메인설비 아닌경우 선택안되도록
                {
                    Util.AlertInfo("SFU2863");
                }

                if (aa.Background.ToString().Equals("#FF0000FF")) //파란색 다시 누르면 풀기
                {
                    btnReset_Click(null, null);
                }
                else
                {
                    setMapColor(org_set["TIME"].ToString(), "MAP");
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
        private void lossSplit_Click(object sender, RoutedEventArgs e)
        {
            if (!org_set["STATUS"].ToString().Equals("R")) //추가
            {
                string startTime = txtStartHidn.Text;
                string endTime = txtEndHidn.Text;
                if (startTime.Equals("") || endTime.Equals(""))
                {
                    return;
                }

                DataTable tmp = new DataTable();
                tmp.Columns.Add("EQPTID", typeof(string));
                tmp.Columns.Add("STRT_DTTM_YMDHMS", typeof(string));

                DataRow dr = tmp.NewRow();
                dr["EQPTID"] = org_set["EQPTID"].ToString().Substring(1);
                dr["STRT_DTTM_YMDHMS"] = startTime;
                tmp.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_END_DTTM", "RQSTDT", "RSLTDT", tmp);
                if (result.Rows.Count != 0)
                {
                    if (Convert.ToString(result.Rows[0]["END_DTTM"]).Equals(""))
                    {
                        Util.MessageValidation("SFU4244"); // 부동이 끝나지 않아 데이터를 분할 할 수 없습니다.
                        return;
                    }
                }

                DataTable dt = dtMainList.Select("HIDDEN_START >= " + startTime + " and HIDDEN_END <= " + endTime).CopyToDataTable();
                if (dt.Select("EIOSTAT = 'R'").Count() > 0)
                {
                    Util.MessageValidation("SFU3511"); //Run상태가 존재합니다
                    btnReset_Click(null, null);
                    return;
                }

                if (dt.Select().Count() > 1)
                {
                    Util.MessageValidation("SFU3512"); //하나의 부동상태만 선택해주세요
                    btnReset_Click(null, null);
                    return;
                }

                COM001_014_LOSS_SPLIT wndPopup = new COM001_014_LOSS_SPLIT();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = org_set["EQPTID"].ToString();
                    Parameters[1] = org_set["TIME"].ToString();
                    Parameters[2] = startTime;
                    Parameters[3] = endTime;
                    Parameters[4] = cboArea.SelectedValue.ToString();
                    Parameters[5] = Util.GetCondition(ldpDatePicker);
                    Parameters[6] = this;
                    Parameters[7] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[8] = cboProcess.SelectedValue.ToString();



                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(wndPopup_Closed2);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }


        /// <summary>
        /// 초기화 버튼 클릭시
        /// </summary>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            resetMapColor();
            txtEqptName.Text = "";
            txtStart.Text = "";
            txtEnd.Text = "";
            txtStartHidn.Text = "";
            txtEndHidn.Text = "";
            rtbLossNote.Document.Blocks.Clear();
            //txtLossNote.Text = "";
            txtFCRCode.Text = "";
            txtPerson.Text = "";
            txtPerson.Tag = null;

            cboLoss.SelectedIndex = 0;
            cboOccurEqpt.SelectedIndex = 0;
            //cboLossDetl.SelectedIndex = 0;
            popLossDetl.SelectedValue = string.Empty;
            popLossDetl.SelectedText = string.Empty;
            cboFailure.SelectedIndex = 0;
            cboCause.SelectedIndex = 0;
            cboResolution.SelectedIndex = 0;
            cboLastLoss.SelectedIndex = 0;

            if (this.dtQA != null) this.dtQA.Clear();       // CSR : C20220512-000432
            this.isOnlyRemarkSave = false;                  // CSR : C20220512-000432
            this.isTotalSave = false;                       // CSR : C20220512-000432
        }

        private void orginalCbo()
        {
            cboArea.SelectedValue = _grid_area;
            cboEquipment.SelectedValue = _grid_eqpt;
            cboEquipmentSegment.SelectedValue = _grid_eqsg;
            cboProcess.SelectedValue = _grid_proc;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // CSR : C20220512-000432 Popup 띄우는 조건
            // 저장 체크중간에 Popup을 띄워야 하는 조건 때문에 기존에 Save 함수를 Validation 부분과 Transaction 부분으로 분리
            if (!this.SaveValidation())
            {
                return;
            }

            // Remark만 저장하는 경우는 Popup 안띄우고 저장후 종료.
            if (this.isOnlyRemarkSave)
            {
                this.SaveProcessOnlyRemark();
                return;
            }

            if (this.IsOpenQAPopup())
            {
                // Open Popup
                COM001_014_QA wndPopupQA = new COM001_014_QA();
                wndPopupQA.FrameOperation = FrameOperation;

                if (wndPopupQA != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = false;      // 그냥 저장
                    C1WindowExtension.SetParameters(wndPopupQA, Parameters);
                    wndPopupQA.Closed -= new EventHandler(this.wndPopupQA_Closed);
                    wndPopupQA.Closed += new EventHandler(this.wndPopupQA_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopupQA.ShowModal()));
                }
            }
            else
            {
                this.SaveProcess();
            }
        }

        private void btnTotalSave_Click(object sender, RoutedEventArgs e)
        {
            // CSR : C20220512-000432 Popup 띄우는 조건
            // 저장 체크중간에 Popup을 띄워야 하는 조건 때문에 기존에 Save 함수를 Validation 부분과 Transaction 부분으로 분리
            if (!this.TotalSaveValidation())
            {
                return;
            }

            // Remark만 저장하는 경우는 Popup 안띄우고 저장후 종료.
            if (this.isOnlyRemarkSave)
            {
                this.TotalSaveProcessOnlyRemark();
                return;
            }

            //해당Loss를 일괄로 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3488"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    this.isTotalSave = true;
                    if (this.IsOpenQAPopup())
                    {
                        // Open Popup
                        COM001_014_QA wndPopupQA = new COM001_014_QA();
                        wndPopupQA.FrameOperation = FrameOperation;

                        if (wndPopupQA != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = true;      // 여러개 저장
                            C1WindowExtension.SetParameters(wndPopupQA, Parameters);
                            wndPopupQA.Closed -= new EventHandler(this.wndPopupQA_Closed);
                            wndPopupQA.Closed += new EventHandler(this.wndPopupQA_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopupQA.ShowModal()));
                        }
                    }
                    else
                    {
                        this.TotalSaveProcess();
                    }
                }
            });

        }

        // CSR : C20220512-000432 질문 Popup 띄우기 여부 확인.
        private bool IsOpenQAPopup()
        {
            bool returnValue = false;

            if (this.GetPackApplyLIneByUI(_grid_eqsg))
            {
                // 질문답변 다 입력후에 다시 저장버튼 누른 경우, 질문지 popup 안띄우고 그대로 저장.
                if (CommonVerify.HasTableRow(this.dtQA))
                {
                    returnValue = false;
                }
                else
                {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 저장 함수에서 Validation 부분)
        private bool SaveValidation()
        {
            orginalCbo();

            this.isOnlyRemarkSave = false;
            #region 2023.03.07 윤지해 CSR ID E20230220-000068  Loss코드, 부동내용 필수 등록으로 validation 수정_주석처리
            //TextRange textRange = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd);
            //// Remark만 단순히 저장하는 경우에는 질문지 저장 안함.
            //if (!string.IsNullOrEmpty(textRange.Text) && !textRange.Text.Equals("\r\n") && cboLoss.Text.Equals("-SELECT-") && popLossDetl.SelectedValue.IsNullOrEmpty())
            //{
            //    this.isOnlyRemarkSave = true;
            //}
            #endregion

            if (!event_valridtion())
            {
                return false;
            }

            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") > 0)
            {
                Util.MessageValidation("SFU3490"); //하나의 부동내역을 저장 할 경우 check box선택을 모두 해제 후 \r\n 한개의 행만 더블클릭  해주세요
                return false;
            }

            //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
            if (dtRemarkMandatory != null && dtRemarkMandatory.Rows.Count > 0)
            {
                string sLoss = Util.GetCondition(cboLoss);
                //string sLossDetl = Util.GetCondition(cboLossDetl);
                string sLossDetl = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

                DataRow[] rows = dtRemarkMandatory.Select("ATTRIBUTE3 = '" + sLoss + "' AND ATTRIBUTE4 = '" + sLossDetl + "'");

                if (rows.Length > 0)
                {
                    int iLength = int.Parse(rows[0]["ATTRIBUTE5"].ToString());
                    string sLossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();

                    //옵션에 지정된 길이가 0보다 큰 값이고 옵션에 지정된 길이보다 짧은 글자가 입력되면
                    if (iLength > 0 && (string.IsNullOrEmpty(sLossNote) || sLossNote.Length < iLength))
                    {
                        Util.MessageValidation("SFU3801", new object[] { iLength });  //비고를 %1자 이상 입력해 주세요.
                        rtbLossNote.Focus();
                        return false;
                    }
                }
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

            if (cboOccurEqpt.Text.ToString().Equals("-SELECT-")) // C20200728 - 000321 원인설비 - SELECT - 초기화 설정(PACK만 적용)
            {
                Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                return false;
            }

            //작업 담당자 필수 :
            if (string.Equals(GetAreaType(), "P"))
            {
                if (txtPerson.Tag == null || txtPerson.Tag.Equals("\r\n"))
                {
                    Util.MessageInfo("SFU1842"); //작업자를 선택 하세요.
                    return false;
                }
            }

            ValidateNonRegisterLoss("ONE");

            // 리마크만 저장할경우 Loss 및 Loss Detail Validation 체크 안함.
            if (!isOnlyRemarkSave)
            {
                // 설비 Check
                if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) || cboEquipment.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageInfo("SFU1153"); // 설비를 선택하세요
                    return false;
                }

                // Loss Check
                if (cboLoss.SelectedIndex < 0 || string.IsNullOrEmpty(cboLoss.SelectedValue.ToString()) || cboLoss.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageInfo("SFU3513"); // LOSS는필수항목입니다
                    return false;
                }

                // Loss Detail Check
                //if (cboLossDetl.SelectedIndex < 0 || string.IsNullOrEmpty(cboLossDetl.SelectedValue.ToString()) || cboLossDetl.Text.ToString().Equals("-SELECT-"))
                //{
                //    if (cboLossDetl.Items.Count > 1)
                //    {
                //        Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                //        return false;
                //    }
                //}
                if (popLossDetl.SelectedValue.IsNullOrEmpty())
                {
                    Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                    return false;
                }
            }

            return true;
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 저장 함수에서 Transaction 부분 Only Remark)
        private void SaveProcessOnlyRemark()
        {
            DataRow[] dtRow = dtMainList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_END <= '" + txtEndHidn.Text + "'", "");

            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            DataRow dr = RQSTDT.NewRow();

            string msg = "";

            msg = "SFU3441";//"해당 Loss의 비고 항목만 저장하시겠습니까?";

            dr["EQPTID"] = Util.GetCondition(cboEquipment);
            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
            dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
            dr["LOSS_CODE"] = null;
            dr["LOSS_DETL_CODE"] = null;
            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
            dr["SYMP_CODE"] = null;
            dr["CAUSE_CODE"] = null;
            dr["REPAIR_CODE"] = null;
            dr["OCCR_EQPTID"] = null;
            dr["USERID"] = LoginInfo.USERID;

            if (string.Equals(GetAreaType(), "P"))
            {
                dr["WRK_USERNAME"] = txtPerson.Tag;
            }

            RQSTDT.Rows.Add(dr);

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {

                if (result.ToString().Equals("OK"))
                {
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_REMARK_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    else
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_REMARK", "RQSTDT", "RSLTDT", RQSTDT);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    btnSearch_Click(null, null);
                    chkT.IsChecked = false;
                    chkW.IsChecked = false;
                    chkU.IsChecked = false;

                    Util.AlertInfo("SFU1270");  //저장되었습니다.
                }
            });
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 저장 함수에서 Transaction 부분)
        private void SaveProcess()
        {
            DataRow[] dtRow = dtMainList.Select("HIDDEN_START >= '" + txtStartHidn.Text + "' and HIDDEN_END <= '" + txtEndHidn.Text + "'", "");

            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            DataRow dr = RQSTDT.NewRow();

            dr["EQPTID"] = Util.GetCondition(cboEquipment);
            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
            dr["STRT_DTTM"] = Util.GetCondition(txtStartHidn);
            dr["END_DTTM"] = Util.GetCondition(txtEndHidn);
            dr["LOSS_CODE"] = Util.GetCondition(cboLoss);
            //dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
            dr["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
            dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
            dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
            dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
            dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
            dr["USERID"] = LoginInfo.USERID;
            if (string.Equals(GetAreaType(), "P"))
            {
                dr["WRK_USERNAME"] = txtPerson.Tag;
            }
            try
            {

                RQSTDT.Rows.Add(dr);

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true) //일괄등록이 하나라도 체크되어 있으면 Run 은 살린 상태로 개별 저장
                {
                    if (chkT.IsChecked == true)
                    {
                        dr["CHKT"] = "T";
                    }
                    else
                    {
                        dr["CHKT"] = "";
                    }

                    if (chkW.IsChecked == true)
                    {
                        dr["CHKW"] = "W";
                    }
                    else
                    {
                        dr["CHKW"] = "";
                    }

                    if (chkU.IsChecked == true)
                    {
                        dr["CHKU"] = "U";
                    }
                    else
                    {
                        dr["CHKU"] = "";
                    }

                    // UPD 조건 다름..
                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_EACH", "RQSTDT", "RSLTDT", RQSTDT);
                    this.SaveQA(RQSTDT);
                }
                else
                {
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    else
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_EQP_UPD_EQPTLOSS", "RQSTDT", "RSLTDT", RQSTDT);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    this.SaveQA(RQSTDT);
                }

                //UPDATE 처리후 재조회
                btnSearch_Click(null, null);
                chkT.IsChecked = false;
                chkW.IsChecked = false;
                chkU.IsChecked = false;

                dgDetail.ScrollIntoView(idx, 0);

                Util.AlertInfo("SFU1270");  //저장되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 일괄 저장 함수에서 Validation 부분)
        private bool TotalSaveValidation()
        {
            orginalCbo();

            this.isOnlyRemarkSave = false;
            TextRange textRange = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd);
            #region 2023.03.07 윤지해 CSR ID E20230220-000068  Loss코드, 부동내용 필수 등록으로 validation 수정_주석
            // Remark만 단순히 저장하는 경우에는 질문지 저장 안함.
            //if (!string.IsNullOrEmpty(textRange.Text) && !textRange.Text.Equals("\r\n") && cboLoss.Text.Equals("-SELECT-") && popLossDetl.SelectedValue.IsNullOrEmpty())
            //{
            //    this.isOnlyRemarkSave = true;
            //}
            #endregion

            if (!event_valridtion())
            {
                return false;
            }

            //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
            if (dtRemarkMandatory != null && dtRemarkMandatory.Rows.Count > 0)
            {
                string sLoss = Util.GetCondition(cboLoss);
                //string sLossDetl = Util.GetCondition(cboLossDetl);
                string sLossDetl = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

                DataRow[] rows = dtRemarkMandatory.Select("ATTRIBUTE3 = '" + sLoss + "' AND ATTRIBUTE4 = '" + sLossDetl + "'");

                if (rows.Length > 0)
                {
                    int iLength = int.Parse(rows[0]["ATTRIBUTE5"].ToString());
                    string sLossNote = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim();

                    //옵션에 지정된 길이가 0보다 큰 값이고 옵션에 지정된 길이보다 짧은 글자가 입력되면
                    if (iLength > 0 && (string.IsNullOrEmpty(sLossNote) || sLossNote.Length < iLength))
                    {
                        Util.MessageValidation("SFU3801", new object[] { iLength });  //비고를 %1자 이상 입력해 주세요.
                        rtbLossNote.Focus();
                        return false;
                    }
                }
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

            if (cboOccurEqpt.Text.ToString().Equals("-SELECT-")) // C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용)
            {
                Util.AlertInfo("9041");  // 원인설비를 선택하여 주십시오
                return false;
            }

            if (cboLoss.Text.ToString().Equals("-SELECT-") && popLossDetl.SelectedValue.IsNullOrEmpty() && (string.IsNullOrEmpty(textRange.Text) || !textRange.Text.Equals("\r\n")))
            {
                Util.MessageValidation("SFU3485"); //저장내역을 입력해주세요
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
            {
                Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                return false;
            }

            if (_Util.GetDataGridCheckCnt(dgDetail, "CHK") == 1)
            {
                Util.MessageValidation("SFU3487"); //일괄등록의 경우 한개 이상의 부동내역을 선택해주세요
                return false;
            }

            if (string.Equals(GetAreaType(), "P"))
            {
                if (txtPerson.Tag == null || txtPerson.Tag.Equals("\r\n"))
                {
                    Util.MessageInfo("SFU1842"); //작업자를 선택 하세요.
                    return false;
                }
            }

            // 설비 Check
            if (cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) || cboEquipment.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageInfo("SFU3514"); // 설비를 선택하세요
                return false;
            }

            ValidateNonRegisterLoss("TOTAL");

            // 리마크만 저장할경우 Loss 및 Loss Detail Validation 체크 안함.
            if (!isOnlyRemarkSave)
            {
                // Loss Check
                if (cboLoss.SelectedIndex < 0 || string.IsNullOrEmpty(cboLoss.SelectedValue.ToString()) || cboLoss.Text.ToString().Equals("-SELECT-"))
                {
                    Util.MessageInfo("SFU3513"); // LOSS는필수항목입니다
                    return false;
                }

                // Loss Detail Check
                //if (cboLossDetl.SelectedIndex < 0 || string.IsNullOrEmpty(cboLossDetl.SelectedValue.ToString()) || cboLossDetl.Text.ToString().Equals("-SELECT-"))
                //{
                //    if (cboLossDetl.Items.Count > 1)
                //    {
                //        Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                //        return false;
                //    }
                //}
                if (popLossDetl.SelectedValue.IsNullOrEmpty())
                {
                    Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                    return false;
                }
            }

            return true;
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 일괄 저장 함수에서 Transaction 부분 Only Remark)
        private void TotalSaveProcessOnlyRemark()
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3488"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

                    DataSet ds = new DataSet();
                    DataTable RQSTDT = ds.Tables.Add("INDATA");
                    //RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                    RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                    RQSTDT.Columns.Add("END_DTTM", typeof(string));
                    RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
                    RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
                    RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
                    RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
                    RQSTDT.Columns.Add("CHKW", typeof(string));
                    RQSTDT.Columns.Add("CHKT", typeof(string));
                    RQSTDT.Columns.Add("CHKU", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
                    }

                    for (int i = 0; i < dgDetail.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            DataRow dr = RQSTDT.NewRow();

                            dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                            if (dr["EQPTID"].Equals("")) return;
                            dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                            dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                            dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                            dr["LOSS_CODE"] = null;
                            dr["LOSS_DETL_CODE"] = null;
                            dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
                            dr["SYMP_CODE"] = null;
                            dr["CAUSE_CODE"] = null;
                            dr["REPAIR_CODE"] = null;
                            dr["OCCR_EQPTID"] = null;
                            dr["USERID"] = LoginInfo.USERID;
                            if (string.Equals(GetAreaType(), "P"))
                            {
                                dr["WRK_USERNAME"] = txtPerson.Tag;
                            }

                            RQSTDT.Rows.Add(dr);
                        }
                    }

                    if (string.Equals(GetAreaType(), "P"))
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_REMARK_ALL_PACK", "INDATA", null, ds);
                    }
                    else
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_REMARK_ALL", "INDATA", null, ds);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    btnSearch_Click(null, null);
                    chkT.IsChecked = false;
                    chkW.IsChecked = false;
                    chkU.IsChecked = false;

                    dgDetail.ScrollIntoView(idx, 0);

                    Util.MessageInfo("SFU1270");  //저장되었습니다.
                }
            });
        }

        // CSR : C20220512-000432 질문 Popup 띄우는 조건으로 인한 함수 분리 (기존 Loss 일괄 저장 함수에서 Transaction 부분)
        private void TotalSaveProcess()
        {
            if (!this.isTotalSave)
            {
                return;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1 ? 0 : _Util.GetDataGridCheckFirstRowIndex(dgDetail, "CHK");

            DataSet ds = new DataSet();
            DataTable RQSTDT = ds.Tables.Add("INDATA");
            //RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("SYMP_CODE", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CODE", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CODE", typeof(string));
            RQSTDT.Columns.Add("OCCR_EQPTID", typeof(string));
            RQSTDT.Columns.Add("SYMP_CNTT", typeof(string));
            RQSTDT.Columns.Add("CAUSE_CNTT", typeof(string));
            RQSTDT.Columns.Add("REPAIR_CNTT", typeof(string));
            RQSTDT.Columns.Add("CHKW", typeof(string));
            RQSTDT.Columns.Add("CHKT", typeof(string));
            RQSTDT.Columns.Add("CHKU", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            if (string.Equals(GetAreaType(), "P"))
            {
                RQSTDT.Columns.Add("WRK_USERNAME", typeof(string));
            }

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = RQSTDT.NewRow();

                    dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.

                    if (dr["EQPTID"].Equals("")) return;
                    dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                    dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));//Util.GetCondition(txtStartHidn);
                    dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                    dr["LOSS_CODE"] = Util.GetCondition(cboLoss, "SFU3513"); // LOSS는필수항목입니다
                    if (dr["LOSS_CODE"].Equals("")) return;

                    //dr["LOSS_DETL_CODE"] = Util.GetCondition(cboLossDetl);
                    //if (dr["LOSS_DETL_CODE"].Equals(""))
                    //{
                    //    if (cboLossDetl.Items.Count > 1)
                    //    {
                    //        // 부동내용을 입력하세요.
                    //        Util.MessageValidation("SFU3631");
                    //        return;
                    //    }
                    //}
                    dr["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                    if (dr["LOSS_DETL_CODE"].Equals(""))
                    {
                        if (popLossDetl.SelectedValue.IsNullOrEmpty())
                        {
                            // 부동내용을 입력하세요.
                            Util.MessageValidation("SFU3631");
                            return;
                        }
                    }

                    dr["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;//Util.GetCondition(txtLossNote);
                    dr["SYMP_CODE"] = Util.GetCondition(cboFailure);
                    dr["CAUSE_CODE"] = Util.GetCondition(cboCause);
                    dr["REPAIR_CODE"] = Util.GetCondition(cboResolution);
                    dr["OCCR_EQPTID"] = Util.GetCondition(cboOccurEqpt);
                    dr["USERID"] = LoginInfo.USERID;
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        dr["WRK_USERNAME"] = txtPerson.Tag;
                    }
                    RQSTDT.Rows.Add(dr);
                }
            }

            try
            {

                if (chkT.IsChecked == true || chkW.IsChecked == true || chkU.IsChecked == true)
                {
                    Util.MessageValidation("SFU3489");//개별등록일 경우 일괄저장 기능 사용 불가
                    return;

                }
                else
                {
                    if (string.Equals(GetAreaType(), "P"))
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL_PACK", "INDATA", null, ds);
                    }
                    else
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_UPD_LOSS_ALL", "RQSTDT", null, ds);
                    }

                    // 설비로스 변경 이력 저장
                    try
                    {
                        DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_CHG_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    catch (Exception ex9)
                    {
                        Util.MessageException(ex9);
                    }

                    this.SaveQA(ds);        // 질문지 저장.
                }

                //UPDATE 처리후 재조회
                btnSearch_Click(null, null);
                chkT.IsChecked = false;
                chkW.IsChecked = false;
                chkU.IsChecked = false;

                dgDetail.ScrollIntoView(idx, 0);

                Util.MessageInfo("SFU1270");  //저장되었습니다.
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제 더블클릭시
        /// 데이터 원복
        /// </summary>
        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            svMap.ScrollToVerticalOffset(10);
            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            if (dgDetail.CurrentRow != null)
            {
                if (RunSplit.Equals("Y"))
                {
                    if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("Y"))
                    {
                        //가동 데이터를 분할 한 데이터이므로 추가된 Loss가 초기화 됩니다. 그래도 삭제하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3205"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {

                            if (result.ToString().Equals("OK"))
                            {
                                try
                                {
                                    DataSet ds = new DataSet();
                                    DataTable dt = ds.Tables.Add("INDATA");

                                    dt.Columns.Add("STRT_DTTM", typeof(DateTime));
                                    dt.Columns.Add("END_DTTM", typeof(DateTime));
                                    dt.Columns.Add("EQPTID", typeof(string));
                                    dt.Columns.Add("WRK_DATE", typeof(string));
                                    dt.Columns.Add("AREAID", typeof(string));
                                    dt.Columns.Add("USERID", typeof(string));
                                    dt.Columns.Add("START_DTTM_YMDHMS", typeof(string));

                                    DataRow row = dt.NewRow();
                                    row["STRT_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["END_DTTM"] = DateTime.ParseExact(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END")), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                    row["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "WRK_DATE"));
                                    row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                                    row["USERID"] = LoginInfo.USERID;
                                    row["START_DTTM_YMDHMS"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                    dt.Rows.Add(row);

                                    new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_RUN_SPLT_RESET", "INDATA", null, ds);

                                    btnSearch_Click(null, null);

                                    if (dgDetail.GetRowCount() != 0)
                                    {
                                        dgDetail.ScrollIntoView(idx, 0);
                                    }


                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }
                        }
                        );
                    }


                }

                if (dgDetail.CurrentColumn.Name.Equals("CHECK_DELETE") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "CHECK_DELETE")).Equals("DELETE") && dgDetail.CurrentColumn != null && Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.CurrentRow.Index].DataItem, "COND_ADJ_TIME_FLAG")).Equals("N")) //삭제 더블클릭시에 실행
                {
                    //삭제하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataSet ds = new DataSet();
                                DataTable dt = ds.Tables.Add("IN_RESET");

                                dt.Columns.Add("EQPTID", typeof(string));
                                dt.Columns.Add("WRK_DATE", typeof(string));
                                dt.Columns.Add("STRT_DTTM", typeof(string));
                                dt.Columns.Add("END_DTTM", typeof(string));
                                dt.Columns.Add("USERID", typeof(string));

                                DataRow dr = dt.NewRow();
                                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                dr["USERID"] = LoginInfo.USERID;

                                dt.Rows.Add(dr);

                                DataTable inLoss = ds.Tables.Add("IN_CHG_HIST");
                                inLoss.Columns.Add("EQPTID", typeof(string));
                                inLoss.Columns.Add("WRK_DATE", typeof(string));
                                inLoss.Columns.Add("STRT_DTTM", typeof(string));
                                inLoss.Columns.Add("END_DTTM", typeof(string));
                                inLoss.Columns.Add("USERID", typeof(string));

                                dr = inLoss.NewRow();
                                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다.
                                if (dr["EQPTID"].Equals("")) return;
                                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                                dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                dr["USERID"] = LoginInfo.USERID;

                                inLoss.Rows.Add(dr);

                                new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_RESET", "IN_RESET,IN_CHG_HIST", null, ds);

                                //UPDATE 처리후 재조회
                                btnSearch_Click(null, null);
                                if (dgDetail.GetRowCount() != 0)
                                {
                                    dgDetail.ScrollIntoView(idx, 0);
                                }


                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("SPLIT") && dgDetail.CurrentColumn != null) //분할 더블클릭시에 실행
                {
                    //분할하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3120"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            COM001_014_SPLIT wndPopup = new COM001_014_SPLIT();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] Parameters = new object[5];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO"));
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                                Parameters[4] = Convert.ToString(cboArea.SelectedValue);

                                C1WindowExtension.SetParameters(wndPopup, Parameters);

                                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }

                        }
                    }
                    );
                }
                else if (dgDetail.CurrentColumn.Name.Equals("SPLIT") &&
                    Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "SPLIT")).Equals("MERGE") && dgDetail.CurrentColumn != null) //병합 더블클릭시에 실행
                {
                    //병합하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2876"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {

                        if (result.ToString().Equals("OK"))
                        {
                            try
                            {
                                DataSet dsData = new DataSet();

                                DataTable dtIn = dsData.Tables.Add("INDATA");
                                dtIn.Columns.Add("EQPTID", typeof(string));
                                dtIn.Columns.Add("FROM_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("TO_SEQNO", typeof(Int32));
                                dtIn.Columns.Add("USERID", typeof(string));

                                DataRow row = null;
                                row = dtIn.NewRow();
                                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                                row["FROM_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100;
                                row["TO_SEQNO"] = (Convert.ToInt32(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "PRE_LOSS_SEQNO")) / 100) * 100 + 99;
                                row["USERID"] = LoginInfo.USERID;

                                dtIn.Rows.Add(row);



                                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_EQPTLOSS_SPLIT_RESET", "INDATA", "OUTDATA", dsData);


                                if (Convert.ToInt16(dsRslt.Tables["OUTDATA"].Rows[0]["CNT"]) > 0)
                                {
                                    Util.AlertInfo("SFU1516");  //등록된 데이터를 지우고 병합해주세요.
                                    return;
                                }
                                else
                                {
                                    //UPDATE 처리후 재조회
                                    btnSearch_Click(null, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }

                        }
                    }
                    );
                }
                else //선택처리
                {
                    btnReset_Click(null, null);
                    setMapColor(Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START")), "LIST", dgDetail.CurrentRow);

                }
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_014_SPLIT window = sender as COM001_014_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }
        private void wndPopup_Closed2(object sender, EventArgs e)
        {
            COM001_014_LOSS_SPLIT window = sender as COM001_014_LOSS_SPLIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        // CSR : C20220512-000432
        private void wndPopupQA_Closed(object sender, EventArgs e)
        {
            COM001_014_QA window = sender as COM001_014_QA;

            bool? isTotalSave = window.TOTAL_SAVE;
            switch (window.DialogResult)
            {
                case MessageBoxResult.OK:           // 질문답변 Data가 있다면 질문답변 Data 저장하기
                    this.dtQA = window.STANDARD_PROCESS_QUESTION_TABLE;
                    break;
                case MessageBoxResult.None:         // 질문답변 Data가 없으면 그냥 저장.
                    break;
                default:
                    break;
            }

            // Loss Save Transaction...
            if (isTotalSave == null)
            {
                return;
            }
            else if (isTotalSave == true)
            {
                this.TotalSaveProcess();
            }
            else
            {
                this.SaveProcess();
            }
        }

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
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));
                    string loss_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE"));
                    string loss_detl_code = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"));

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

                // 비고 칼럼 사이즈
                if (e.Cell.Column.Name.Equals("txtNote"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                }

            }));

        }

        private void cboLossEqsgProc_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Object.ReferenceEquals(null, cboLossEqsgProc.SelectedValue)) return;

            if (!cboLossEqsgProc.SelectedValue.Equals("SELECT"))
            {
                string[] loss = cboLossEqsgProc.SelectedValue.ToString().Split('-');
                string[] lossText = cboLossEqsgProc.Text.ToString().Split('-');

                cboLoss.SelectedValue = loss[0];

                //if (!loss[1].Equals(""))
                //{
                //    cboLossDetl.SelectedValue = loss[1];
                //}
                if (loss.Length > 1 && !loss[1].Equals(""))
                {
                    popLossDetl.SelectedValue = loss[1];
                    popLossDetl.SelectedText = lossText[0];
                }
            }
        }

        #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
        private void popLossDetl_ValueChanged(object sender, EventArgs e)
        {
            // 부동내용이 바뀌면 현상, 원인, 조치 전체 초기화
            CommonCombo _combo = new CommonCombo();

            string procId = Util.GetCondition(cboProcess);
            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

            string failCode = string.Empty;
            string causeCode = string.Empty;
            string resolCode = string.Empty;
            string selectedText = string.Empty;

            cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
            String[] sFilterFailure = { procId, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, selectedText };
            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
            cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;

            #region 2023.03.28     윤지해     E20230321-001518	 GMES FCR코드 BM입력 개선 CSR요청_GM자동차 조립만 적용
            // 현상이 1:1 매칭일 경우 자동으로 INDEX 세팅
            if (cboFailure.Items.Count == 2 && (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673")))  //GM2추가 2023-08-24  장영철
            {
                SetCboFailure_Index();
            }
            else
            {
                cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                String[] sFilterCause = { procId, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, selectedText };
                _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;

                cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                String[] sFilterResolution = { procId, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, selectedText };
                _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;

            }
            #endregion
        }

        private void cboFailure_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            /*
             * <현상 변경>
             * 1. N/A 선택될 경우              : 현상 콤보박스 초기화 O
             *                                   원인, 조치 콤보박스 초기화 X
             * 2. N/A 가 아닌 값이 선택될 경우 : 현상 콤보박스 초기화 X
             *                                   원인 - 선택된 값이 없을 경우 현상, 조치 값으로 초기화 O
             *                                          선택된 값이 있을 경우 초기화 X
             *                                   조치 - 선택된 값이 없을 경우 현상, 원인 값으로 초기화 O
             *                                          선택된 값이 있을 경우 초기화 X
             */
            if (!popLossDetl.SelectedValue.IsNullOrEmpty())
            {
                CommonCombo _combo = new CommonCombo();

                string procId = Util.GetCondition(cboProcess);
                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();

                if (cboFailure.SelectedValue.IsNullOrEmpty())
                {
                    cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                    String[] sFilterFailure = { procId, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, cboFailure.SelectedValue.ToString() };
                    _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                    cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                }
                else
                {
                    if (cboCause.SelectedValue.IsNullOrEmpty())
                    {
                        cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                        String[] sFilterCause = { procId, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, cboCause.SelectedValue.ToString() };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                        cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                    }
                    if (cboResolution.SelectedValue.IsNullOrEmpty())
                    {
                        cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                        String[] sFilterResolution = { procId, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, cboResolution.SelectedValue.ToString() };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                        cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                    }
                }
            }
        }

        private void cboCause_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!popLossDetl.SelectedValue.IsNullOrEmpty())
            {

                CommonCombo _combo = new CommonCombo();

                string procId = Util.GetCondition(cboProcess);
                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();

                if (cboCause.SelectedValue.IsNullOrEmpty())
                {
                    cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                    String[] sFilterCause = { procId, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, cboCause.SelectedValue.ToString() };
                    _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                    cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                }
                else
                {
                    if (cboFailure.SelectedValue.IsNullOrEmpty())
                    {
                        cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                        String[] sFilterFailure = { procId, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, cboFailure.SelectedValue.ToString() };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                        cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                    }
                    if (cboResolution.SelectedValue.IsNullOrEmpty())
                    {
                        cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                        String[] sFilterResolution = { procId, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, cboResolution.SelectedValue.ToString() };
                        _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                        cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                    }
                }
            }
        }

        private void cboResolution_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!popLossDetl.SelectedValue.IsNullOrEmpty())
            {
                CommonCombo _combo = new CommonCombo();

                string procId = Util.GetCondition(cboProcess);
                string lossCode = Util.GetCondition(cboLoss);
                string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                string failCode = cboFailure.SelectedValue.IsNullOrEmpty() ? string.Empty : cboFailure.SelectedValue.ToString();
                string causeCode = cboCause.SelectedValue.IsNullOrEmpty() ? string.Empty : cboCause.SelectedValue.ToString();
                string resolCode = cboResolution.SelectedValue.IsNullOrEmpty() ? string.Empty : cboResolution.SelectedValue.ToString();

                if (cboResolution.SelectedValue.IsNullOrEmpty())
                {
                    cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                    String[] sFilterResolution = { procId, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, resolCode, cboResolution.SelectedValue.ToString() };
                    _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                    cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                }
                else
                {
                    if (cboFailure.SelectedValue.IsNullOrEmpty())
                    {
                        cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                        String[] sFilterFailure = { procId, lossCode, lossDetlCode, _fTypeCode, failCode, causeCode, resolCode, cboFailure.SelectedValue.ToString() };
                        _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                        cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;
                    }
                    if (cboCause.SelectedValue.IsNullOrEmpty())
                    {
                        cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                        String[] sFilterCause = { procId, lossCode, lossDetlCode, _cTypeCode, failCode, causeCode, resolCode, cboCause.SelectedValue.ToString() };
                        _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                        cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;
                    }
                }
            }
        }

        #endregion
        #endregion

        #region Mehod

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
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU3514"); //설비는필수입니다
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

                // 22.11.09 정연준 활성화도 대표설비 대신 main설비 기준으로 조회
                /*if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") && sEqptType.Equals("A"))
                {
                    sEqptID = cboEquipment.GetStringValue("MAIN_EQPTID");
                }*/
                
                //if (cboProcess.SelectedValue.ToString() == PROC.PACKAGING || cboProcess.SelectedValue.ToString() == PROC.DEGAS ||
                //    cboProcess.SelectedValue.ToString() == PROC.ASSY || cboProcess.SelectedValue.ToString() == PROC.WASHING)
                //{
                //    if ((bool)chkSubEqpt.Checked)
                //    {
                //        sEqptType = "%";
                //    }
                //}

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

                //txtStart.Text = "";
                //txtEnd.Text = "";
                //txtTroubleName.Text = "";
                //txtStartHidn.Text = "";
                //txtEndHidn.Text = "";
                //txtEqptName.Text = "";
                //txtMdesc.Text = "";

                //spdMList.ActiveSheet.RowCount = 0;

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

                //첫줄 생성처리
                //for (int k = 0; k < hash_title.Count; k++)
                //{
                //    RowDefinition gridRow = new RowDefinition();
                //    gridRow.Height = new GridLength(15);
                //    _grid.RowDefinitions.Add(gridRow);
                //}

                for (int i = 0; i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count; i++)
                //for (int i = 0; i < 1000; i++)
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

                    //spdMList.ActiveSheet.Cells[nRow, nCol].HorizontalAlignment = CellHorizontalAlignment.Left;

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
                        //if(i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count)
                        //{
                        //    for (int k = 0; k < hash_title.Count; k++)
                        //    {
                        //        RowDefinition gridRow = new RowDefinition();
                        //        gridRow.Height = new GridLength(15);
                        //        _grid.RowDefinitions.Add(gridRow);
                        //    }

                        //}

                        cnt = 0;
                        inc++;
                        //if (i < dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1)
                        //{
                        //spdMList.ActiveSheet.RowCount = spdMList.ActiveSheet.RowCount + (hash_title.Count) + 1;
                        //}
                    }

                }

                int iTotalRow = inc == 12 ? (hash_title.Count + 1) * inc : (hash_title.Count + 1) * (inc + 1);

                for (int k = 0; k < iTotalRow; k++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(15);
                    _grid.RowDefinitions.Add(gridRow);
                }

                ////--- 위에서 시간 중복 Hastable 처리
                //foreach (DictionaryEntry de in hash_Merge)
                //{
                //    int nRow1 = int.Parse(de.Key.ToString());
                //    spdMList.ActiveSheet.SetRowMerge(nRow1, FarPoint.Win.Spread.Model.MergePolicy.Always);
                //    FarPoint.Win.LineBorder bevelbrdr = new FarPoint.Win.LineBorder(GridBackColor.Color3, 1);
                //    spdMList.ActiveSheet.Rows[nRow1].Border = bevelbrdr;
                //}

                //spdMList.ActiveSheet.Protect = true;
                //spdMList.ResumeLayout();

                //svMap.ScrollToHorizontalOffset =

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
                RQSTDT.Columns.Add("ASC", typeof(string));
                RQSTDT.Columns.Add("REVERSE_CHECK", typeof(string));
                RQSTDT.Columns.Add("MIN_SECONDS", typeof(Int32));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                dr["WRK_DATE"] = Util.GetCondition(ldpDatePicker);
                dr["ASC"] = (bool)chkLossSort.IsChecked ? null : "Y";
                dr["REVERSE_CHECK"] = (bool)chkLossSort.IsChecked ? "Y" : null;
                dr["MIN_SECONDS"] = (bool)chkSearchAll.IsChecked ? 0 : 180;

                RQSTDT.Rows.Add(dr);

                if (string.Equals(GetAreaType(), "P") || bFCS)
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSSDETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                }




                if (cboShift.SelectedValue != null && !string.IsNullOrEmpty(cboShift.SelectedValue.GetString()))
                //if (!cboShift.SelectedValue.ToString().Equals(""))
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
                        if (chkLossSort.IsChecked == true)
                        {
                            RSLTDT = RSLTDT.Select("(HIDDEN_START >=" + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + "and HIDDEN_END <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + ") or ( HIDDEN_START <= " + Convert.ToDateTime(dJobDate_ed).ToString("yyyyMMddHHmmss") + "and HIDDEN_END > " + Convert.ToDateTime(dJobDate_st).ToString("yyyyMMddHHmmss") + ")").Reverse().CopyToDataTable();
                        }

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


                txtRequire.Text = (RSLTDT.Rows.Count - Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) - Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();
                txtWriteEnd.Text = (Convert.ToInt16(RSLTDT.Compute("COUNT(CHECK_DELETE)", "CHECK_DELETE = 'DELETE'")) + Convert.ToInt16(RSLTDT.Compute("COUNT(SRC_TYPE_CODE)", "SRC_TYPE_CODE = 'EQP' AND LOSS_CODE IS NOT NULL AND LOSS_DETL_CODE IS NOT NULL"))).ToString();


                // 2019-09-30 황기근 사원 수정
                restrictSave();


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
                    // String sShift = dtShift.Compute("MIN(SHFT_STRT_HMS)", "").ToString();
                    String sShift = dtShift.Rows[0]["SHFT_STRT_HMS"].ToString();
                    if (sShift.Length > 0)
                    {
                        // dJobDate = (DateTime)result.Rows[0]["JOBDATE_YYYYMMDD"];
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
                dr["EQPTID"] = /*sEqptID*/Util.GetCondition(cboEquipment);
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
                //else if (bFCS)
                //{
                //    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FCS", "RQSTDT", "RSLTDT", RQSTDT);
                //}
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
                //else if (bFCS)
                //{
                //    RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_PROC_EQPTLOSSMAP_FIRST_FCS", "RQSTDT", "RSLTDT", RQSTDT);
                //}
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

        /// <summary>
        /// 색지도 클릭시 색상 초기화
        /// </summary>
        private void resetMapColor()
        {
            foreach (Border _border in _grid.Children.OfType<Border>())
            {

                Hashtable org_set = (Hashtable)_border.Tag as Hashtable;
                _border.Background = org_set["COLOR"] as SolidColorBrush;
            }

            DataRow[] dtRow = dtMainList.Select("CHK = '1'", "");

            foreach (DataRow dr in dtRow)
            {
                dr["CHK"] = "0";
            }

        }

        /// <summary>
        /// 색지도 클릭시 색깔 칠하기
        /// </summary>
        private void setMapColor(String sTime, String sType, C1.WPF.DataGrid.DataGridRow row = null)
        {
            DataRow[] dtRow = dtMainList.Select("HIDDEN_START <= '" + sTime + "' and HIDDEN_END > '" + sTime + "'", "");
            DataRow[] dtRowBefore = dtMainList.Select("CHK = '1'", "HIDDEN_START ASC");

            //Shift 에 따라 변경 되도록 할것
            //전체일경우 20, 나머지는 10
            int inc = 20;

            if (Util.GetCondition(cboShift).Equals(""))
            {
                inc = 20;
            }
            else
            {
                inc = 10;
            }

            try
            {
                if (dtRow.Length > 0)
                {
                    dtRow[0]["CHK"] = "1";

                    double dStartTime = new Double();
                    Double dEndTime = new Double();

                    if (dtRowBefore.Length > 0) //이미 체크가 있는경우
                    {
                        if (Convert.ToDouble(dtRow[0]["HIDDEN_START"]) > Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]))
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRowBefore[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRowBefore[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRowBefore[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();
                        }
                        else
                        {
                            dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                            dEndTime = Math.Truncate(Convert.ToDouble(dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"]) / inc) * inc;

                            txtStart.Text = dtRow[0]["START_TIME"].ToString();
                            txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                            txtEnd.Text = dtRowBefore[dtRowBefore.Length - 1]["END_TIME"].ToString();
                            txtEndHidn.Text = dtRowBefore[dtRowBefore.Length - 1]["HIDDEN_END"].ToString();
                        }
                    }
                    else
                    {
                        dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;
                        dEndTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_END"]) / inc) * inc;

                        txtStart.Text = dtRow[0]["START_TIME"].ToString();
                        txtStartHidn.Text = dtRow[0]["HIDDEN_START"].ToString();

                        txtEnd.Text = dtRow[0]["END_TIME"].ToString();
                        txtEndHidn.Text = dtRow[0]["HIDDEN_END"].ToString();

                        #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                        CommonCombo _combo = new CommonCombo();

                        // 항상 존재하는 값
                        string procId = Util.GetCondition(cboProcess);
                        string lossCode = !Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals("") ? Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")) : string.Empty;
                        string lossDetlCode = !Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals("") ? Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")) : string.Empty;

                        // Optional 값
                        string failCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                        string causeCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                        string resolCode = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                        #endregion

                        if (row != null)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME")).Equals(""))
                            {
                                txtEqptName.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTNAME"));
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID")).Equals(""))
                            {
                                cboOccurEqpt.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OCCR_EQPTID"));
                            }
                            #region 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            //{
                            //    cboLoss.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            //{
                            //    //cboLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                            //    popLossDetl.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE"));
                            //    popLossDetl.SelectedText = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_NAME"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE")).Equals(""))
                            //{
                            //    cboCause.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAUSE_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE")).Equals(""))
                            //{
                            //    cboFailure.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "SYMP_CODE"));
                            //}
                            //if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE")).Equals(""))
                            //{
                            //    cboResolution.SelectedValue = Util.NVC(DataTableConverter.GetValue(row.DataItem, "REPAIR_CODE"));
                            //}
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_CODE")).Equals(""))
                            {
                                cboLoss.SelectedValue = lossCode;
                            }
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_CODE")).Equals(""))
                            {
                                popLossDetl.SelectedValue = lossDetlCode;
                                popLossDetl.SelectedText = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_DETL_NAME"));
                            }
                            // 현상
                            //cboFailure.SelectedItemChanged -= cboFailure_SelectedItemChanged;
                            String[] sFilterFailure = { procId, lossCode, lossDetlCode, _fTypeCode, null, null, null, failCode };
                            _combo.SetCombo(cboFailure, CommonCombo.ComboStatus.NA, sFilter: sFilterFailure, sCase: "FCRCODE_LOSS_LV3");
                            //cboFailure.SelectedItemChanged += cboFailure_SelectedItemChanged;

                            // 원인
                            //cboCause.SelectedItemChanged -= cboCause_SelectedItemChanged;
                            String[] sFilterCause = { procId, lossCode, lossDetlCode, _cTypeCode, failCode, null, null, causeCode };
                            _combo.SetCombo(cboCause, CommonCombo.ComboStatus.NA, sFilter: sFilterCause, sCase: "FCRCODE_LOSS_LV3");
                            //cboCause.SelectedItemChanged += cboCause_SelectedItemChanged;

                            // 조치
                            //cboResolution.SelectedItemChanged -= cboResolution_SelectedItemChanged;
                            String[] sFilterResolution = { procId, lossCode, lossDetlCode, _rTypeCode, failCode, causeCode, null, resolCode };
                            _combo.SetCombo(cboResolution, CommonCombo.ComboStatus.NA, sFilter: sFilterResolution, sCase: "FCRCODE_LOSS_LV3");
                            //cboResolution.SelectedItemChanged += cboResolution_SelectedItemChanged;
                            #endregion
                            if (!Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE")).Equals(""))
                            {
                                new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                                //txtLossNote.Text = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOSS_NOTE"));
                            }
                        }

                    }

                    Border borderS = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStartTime.ToString()) as Border;
                    Border borderE = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dEndTime.ToString()) as Border;

                    if (borderS == null)
                    {
                        borderS = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) as Border;
                        //  dStartTime = Math.Truncate(Convert.ToDouble(dtRow[0]["HIDDEN_START"]) / inc) * inc;  (dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString();
                        dStartTime = Math.Truncate(Convert.ToDouble((dsEqptTimeList.Tables["RSLTDT"].Rows[0][0]).ToString()) / inc) * inc;
                    }
                    if (borderE == null)
                    {
                        borderE = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + (dsEqptTimeList.Tables["RSLTDT"].Rows[dsEqptTimeList.Tables["RSLTDT"].Rows.Count - 1][0]).ToString()) as Border;
                    }

                    Hashtable hashStart = borderS.Tag as Hashtable;
                    Hashtable hashEnd = borderE.Tag as Hashtable;

                    DateTime dStart = new DateTime(Convert.ToInt16(dStartTime.ToString().Substring(0, 4)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(4, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(6, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(8, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(10, 2)),
                                                    Convert.ToInt16(dStartTime.ToString().Substring(12, 2)));

                    //색칠해야할 셀갯수 =  row 차이 / (설비갯수 + 시간디스플레이) * 컬럼수 + 종료컬럼 - 시작컬럼
                    int cellCnt = (Convert.ToInt16(hashEnd["ROW"]) - Convert.ToInt16(hashStart["ROW"])) / (iEqptCnt + 1) * 360 + (Convert.ToInt16(hashEnd["COL"]) - Convert.ToInt16(hashStart["COL"]));


                    for (int j = 0; j < cellCnt; j++)
                    {
                        Border _border = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStart.AddSeconds(j * inc).ToString("yyyyMMddHHmmss")) as Border;

                        _border.Background = new SolidColorBrush(Colors.Blue);
                    }

                    //마지막 칸 정리
                    Border borderEndMinusOne = _grid.FindName("S" + sMainEqptID.Replace("-", "_") + dStart.AddSeconds((cellCnt - 1) * inc).ToString("yyyyMMddHHmmss")) as Border;
                    Hashtable hashEndMinusOne = borderEndMinusOne.Tag as Hashtable;

                    if (hashEnd["COLOR"].ToString().Equals(hashEndMinusOne["COLOR"].ToString()))
                    {
                        borderE.Background = new SolidColorBrush(Colors.Blue);
                    }

                    int iRow = Grid.GetRow(borderS);



                    txtEqptName.Text = GetEqptName(hashStart["EQPTID"].ToString(), "M").Rows[0][1].ToString();

                    if (sType.Equals("LIST"))
                    {
                        svMap.ScrollToVerticalOffset((15 * iRow - 8));
                    }
                }
            }
            catch (Exception e)
            {

            }
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
        #region 2023.03.28     윤지해     E20230321-001518	 GMES FCR코드 BM입력 개선 CSR요청_GM자동차 조립만 적용
        private void SetCboFailure_Index()
        {
            cboFailure.SelectedIndex = 1;
            cboFailure_SelectedItemChanged(null, null);
        }
        #endregion

        #endregion

        #region [Biz]

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

        #endregion


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

            //public static readonly System.Drawing.Color L11000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L12000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L13000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L14000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L15000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L16000 = System.Drawing.Color.FromArgb(0, 32, 96);
            //public static readonly System.Drawing.Color L21000 = System.Drawing.Color.FromArgb(217, 151, 149);
            //public static readonly System.Drawing.Color L22000 = System.Drawing.Color.FromArgb(217, 151, 149);
            //public static readonly System.Drawing.Color L23000 = System.Drawing.Color.FromArgb(217, 151, 149);
            //public static readonly System.Drawing.Color L31000 = System.Drawing.Color.FromArgb(0, 176, 240);
            //public static readonly System.Drawing.Color L32000 = System.Drawing.Color.FromArgb(255, 0, 0);
            //public static readonly System.Drawing.Color L33000 = System.Drawing.Color.FromArgb(255, 0, 0);
            //public static readonly System.Drawing.Color L34000 = System.Drawing.Color.FromArgb(228, 109, 10);
            //public static readonly System.Drawing.Color L35000 = System.Drawing.Color.FromArgb(0, 112, 192);
            //public static readonly System.Drawing.Color L36000 = System.Drawing.Color.FromArgb(83, 142, 213);
            //public static readonly System.Drawing.Color L37000 = System.Drawing.Color.FromArgb(112, 48, 160);
            //public static readonly System.Drawing.Color L38000 = System.Drawing.Color.FromArgb(112, 48, 160);
            //public static readonly System.Drawing.Color L39000 = System.Drawing.Color.FromArgb(148, 39, 84);
            //public static readonly System.Drawing.Color L3A000 = System.Drawing.Color.FromArgb(165, 165, 165);
            //public static readonly System.Drawing.Color L3B000 = System.Drawing.Color.FromArgb(255, 255, 0);
            //public static readonly System.Drawing.Color L41000 = System.Drawing.Color.FromArgb(0, 0, 255);
        }

        private System.Drawing.Color GetColor(string sType)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            if (sType == null) return color;

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
                            //color = System.Drawing.Color.White;
                            color = System.Drawing.Color.FromName(hash_loss_color[sType.Substring(1)].ToString());
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                color = System.Drawing.Color.White;
            }
            return color;
        }

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        private void cboLastLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(Object.ReferenceEquals(null, cboLastLoss.SelectedValue)) return;

            if (!cboLastLoss.SelectedValue.Equals("SELECT"))
            {
                string[] sLastLoss = cboLastLoss.SelectedValue.ToString().Split('-');
                string[] sLastText = cboLastLoss.Text.ToString().Split('-');

                cboLoss.SelectedValue = sLastLoss[0];

                if (sLastLoss.Length > 1 && !sLastLoss[1].Equals(""))
                {
                    //cboLossDetl.SelectedValue = sLastLoss[1];
                    popLossDetl.SelectedValue = sLastLoss[1];
                    popLossDetl.SelectedText = sLastText[0];
                }
            }
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
            Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);

            IndataTable.Rows.Add(Indata);

            string sShiftID = string.Empty;
            string sShiftNM = string.Empty;
            string sWorkerID = string.Empty;
            string sWorkerNM = string.Empty;

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "INDATA", "OUTDATA", IndataTable);

                if (dtRslt.Rows.Count > 0)
                {
                    sShiftID = Util.NVC(dtRslt.Rows[0]["SHFT_ID"]);
                    sShiftNM = Util.NVC(dtRslt.Rows[0]["SHFT_NAME"]);
                    sWorkerID = Util.NVC(dtRslt.Rows[0]["WRK_USERID"]);
                    sWorkerNM = Util.NVC(dtRslt.Rows[0]["WRK_USERNAME"]);
                }
            }
            catch
            {
            }

            if (string.Equals(GetAreaType(), "E"))
            {
                COM001_014_EQPCOMMNET wndEqpComment = new COM001.COM001_014_EQPCOMMNET();
                wndEqpComment.FrameOperation = FrameOperation;

                //if (wndEqpComment != null)
                //{
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = cboProcess.SelectedValue.ToString(); ;
                Parameters[3] = "";
                Parameters[4] = "";
                Parameters[5] = cboEquipment.Text;
                Parameters[6] = sShiftNM; // 작업조명
                Parameters[7] = sShiftID; // 작업조코드
                Parameters[8] = sWorkerNM; // 작업자명
                Parameters[9] = sWorkerID; // 작업자 ID
                                           //C1WindowExtension.SetParameters(wndEqpComment, Parameters);
                this.FrameOperation.OpenMenu("SFU010130090", true, Parameters);

                //wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                //_grid.Children.Add(wndEqpComment);
                //wndEqpComment.BringToFront();
                //}
            }
            else
            {
                CMM_COM_EQPCOMMENT wndEqpComment = new CMM_COM_EQPCOMMENT();
                wndEqpComment.FrameOperation = FrameOperation;

                if (wndEqpComment != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = cboProcess.SelectedValue.ToString(); ;
                    Parameters[3] = "";
                    Parameters[4] = "";
                    Parameters[5] = cboEquipment.Text;
                    Parameters[6] = sShiftNM; // 작업조명
                    Parameters[7] = sShiftID; // 작업조코드
                    Parameters[8] = sWorkerNM; // 작업자명
                    Parameters[9] = sWorkerID; // 작업자 ID
                    C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                    wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                    //_grid.Children.Add(wndEqpComment);
                    wndEqpComment.BringToFront();
                }
            }

        }
        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            if (string.Equals(GetAreaType(), "E"))
            {
                //COM001_014_EQPCOMMNET window = sender as COM001_014_EQPCOMMNET;
                //if (window.DialogResult == MessageBoxResult.OK)
                //{
                //}
            }
            else
            {
                CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                }
            }

        }


        private void btnRegiFcr_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }
            COM001_014_FCR wndFCR = new COM001_014_FCR();
            wndFCR.FrameOperation = FrameOperation;

            if (wndFCR != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                Parameters[2] = Convert.ToString(_grid_eqsg);

                C1WindowExtension.SetParameters(wndFCR, Parameters);

                wndFCR.Closed += new EventHandler(wndFCR_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCR.ShowModal()));
                wndFCR.BringToFront();

            }

        }
        private void wndFCR_Closed(object sender, EventArgs e)
        {
            COM001_014_FCR window = sender as COM001_014_FCR;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void btnSearchLossCode_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }

            COM001_014_FCR_LIST wndFCRList = new COM001_014_FCR_LIST();
            wndFCRList.FrameOperation = FrameOperation;

            if (wndFCRList != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                Parameters[2] = Convert.ToString(_grid_eqsg);

                C1WindowExtension.SetParameters(wndFCRList, Parameters);

                wndFCRList.Closed += new EventHandler(wndFCRList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndFCRList.ShowModal()));
                wndFCRList.BringToFront();

            }

        }
        private void wndFCRList_Closed(object sender, EventArgs e)
        {
            COM001_014_FCR_LIST window = sender as COM001_014_FCR_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboFailure.SelectedValue = window.F_CODE;
                cboCause.SelectedValue = window.C_CODE;
                cboResolution.SelectedValue = window.R_CODE;

                txtFCRCode.Text = window.FCR_GR_CODE;
            }
        }
        private bool ValidateFCR()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.Equals("") || cboArea.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3206"); //동을 선택해주세요
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.Equals("") || cboProcess.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3207"); //공정을 선택해주세요
                return false;
            }
            return true;
        }

        private void txtLossCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchFCRCode();
            }
        }

        private void SearchFCRCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FCR_GR_CODE", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                row["FCR_GR_CODE"] = txtFCRCode.Text;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_FCR_GR", "RQSTDT", "RSLTDT", dt);

                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3208"); //해당 FCR그룹코드가 없습니다. 등록해주세요

                    txtFCRCode.Text = string.Empty;
                    cboFailure.SelectedIndex = 0;
                    cboCause.SelectedIndex = 0;
                    cboResolution.SelectedIndex = 0;

                    return;
                }

                cboFailure.SelectedValue = result.Rows[0]["F_CODE"];
                cboCause.SelectedValue = result.Rows[0]["C_CODE"];
                cboResolution.SelectedValue = result.Rows[0]["R_CODE"];


            }
            catch (Exception ex)
            {

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

        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }
            COM001_014_LOSS_DETL wndLossDetl = new COM001_014_LOSS_DETL();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                #region 2022.08.23 C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                object[] Parameters = new object[4];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                Parameters[2] = Convert.ToString(_grid_eqpt);
                // 2022.08.23 YJH 자동차/ESS 조립 공정, PD(품질부동,EL014)만 적용
                // 2022.12.23 정연준 설비 LOSS 등록 부동내역 팝업 시 분류에 속한 LOSS만 조회 (조립GMES 수정 내용 동일 반영)
                Parameters[3] = (cboLoss.SelectedValue.IsNullOrEmpty() || cboLoss.SelectedValue.ToString().Equals("SELECT")) ? "" : cboLoss.SelectedValue.ToString();
                //if (cboLoss.SelectedValue != null && string.Equals(GetAreaType(), "A") && (LoginInfo.CFG_AREA_ID.ToString().StartsWith("A") || LoginInfo.CFG_AREA_ID.ToString().StartsWith("S")))
                //{
                //    Parameters[3] = cboLoss.SelectedValue.ToString().Equals("EL014") ? cboLoss.SelectedValue.ToString() : "";
                //}
                //else
                //{
                //    Parameters[3] = "";
                //}
                #endregion
                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            COM001_014_LOSS_DETL window = sender as COM001_014_LOSS_DETL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE;
                popLossDetl.SelectedValue = window._LOSS_DETL_CODE;
                popLossDetl.SelectedText = window._LOSS_DETL_NAME;

                // 2023.03.07 윤지해 CSR ID E20230220-000068	FCR 초기화
                popLossDetl_ValueChanged(null, null);
            }
        }

        private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        {
            grUp.RowDefinitions[0].Height = new GridLength(0);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);

            grUp.RowDefinitions[1].Height = new GridLength(0);


        }

        private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            grUp.RowDefinitions[0].Height = new GridLength(300);
            grUp.RowDefinitions[1].Height = new GridLength(8);
            grUp.RowDefinitions[2].Height = GridLength.Auto;
            grUp.RowDefinitions[3].Height = new GridLength(8);
            grUp.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
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
                row["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                //row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) :  ldpDatePicker.SelectedDateTime.ToShortDateString() + " " + txtStart.Text;
                //우크라이나어 세팅시 날짜 포맷형식 에러로 인한 수정 2019.07.19.
                row["STRT_DTTM"] = saveType.Equals("TOTAL") ? ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[idx].DataItem, "START_TIME")) : ldpDatePicker.SelectedDateTime.ToString("yyyy-MM-dd") + " " + txtStart.Text;

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

        private void chkLossSort_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgDetail.GetRowCount() != 0)
            {
                GetEqptLossDetailList();
            }

        }

        private void chkLossSort_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (dgDetail.GetRowCount() != 0)
            {
                GetEqptLossDetailList();
            }
        }

        private void btnChgHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                COM001_014_CHG_HIST wndHist = new COM001_014_CHG_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "EQPTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "PRE_LOSS_SEQNO"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);
                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            COM001_014_CHG_HIST window = sender as COM001_014_CHG_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        /// <summary>
        /// 2019.09.30 황기근 사원 수정
        /// CSR ID: C20190910_82826
        /// PLANT 별로 LOSS 수정 제약을 줌.
        /// 이전 날짜 조회 시 하루 변경 가능 시간 기준 정보 조회 후 가능 시간이 아니면 저장 및 일괄저장 버튼 비활성화.
        /// 단, 설비LOSS 관리 권한 보유자는 수정 가능
        /// </summary>
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
                btnSave.IsEnabled = true;
                btnTotalSave.IsEnabled = true;

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
                        btnSave.IsEnabled = false;
                        btnTotalSave.IsEnabled = false;
                    }
                }
                else
                {
                    if (auth.Rows.Count <= 0)
                    {
                        btnSave.IsEnabled = false;
                        btnTotalSave.IsEnabled = false;
                    }
                }
            }


        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void txtPerson_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                txtPerson.Tag = null;
            }
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;
            //cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;
            //cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;

            SetEquipmentSegmentCombo(cboEquipmentSegment);

            //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            //cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            //cboEquipment.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            //cboShift.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            //SetProcessCombo(cboProcess);
            SetProcessFormCombo();
            SetTroubleUnitColumnDisplay();
            //cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
            //cboShift.SelectedValueChanged += cboEquipment_SelectedValueChanged;
            SetShiftCombo(cboShift);
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            //cboShift.SelectedValueChanged -= cboEquipment_SelectedValueChanged;
            SetEquipmentCombo(cboEquipment);
            SetTroubleUnitColumnDisplay();
            //cboShift.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bPack) return;

            SetShiftCombo(cboShift);


        }


        #region [ PopUp Event ]

        #region < 해체 담당자 찾기 >

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtPerson.Text;

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
                txtPerson.Text = wndPerson.USERNAME;
                txtPerson.Tag = wndPerson.USERID;
            }
        }


        #endregion

        #endregion

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
                        dgDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Visible;
                        dgDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                        dgDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                dgDetail.Columns["UNIT_TRBL_EQPTID"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["UNIT_TRBL_CODE"].Visibility = Visibility.Collapsed;
                Util.MessageException(ex);

            }
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
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                //bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FORM_LOSS_CBO";
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";
            }

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea.SelectedValue;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                dr["INCLUDE_GROUP"] = "AC";
            }
            inTable.Rows.Add(dr);

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

        private void SetProcessCombo(C1ComboBox cbo)
        {
            //cbo.ItemsSource = null;
            //cbo.Items.Clear();

            //string equipmentSegmentCode = cboEquipmentSegment.SelectedValue.GetString();

            //const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";

            //string[] arrColumn = { "LANGID", "EQSGID" };
            //string[] arrCondition = { LoginInfo.LANGID, equipmentSegmentCode };
            //string selectedValueText = "CBO_CODE";
            //string displayMemberText = "CBO_NAME";

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);

            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bizRuleName = "DA_BAS_SEL_PROCESS_CBO_FORM";
            }

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

        private void SetProcessFormCombo()
        {
            string sCase2 = null;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F") // 활성화이면
            {
                sCase2 = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";
            }
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment, cboArea };
            //C1ComboBox[] cbProcessChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE/*, cbChild: cbProcessChild*/, cbParent: cbProcessParent, sCase: sCase2);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {

            cbo.ItemsSource = null;
            cbo.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            // 22.11.09 정연준 활성화도 대표설비 대신 main설비 기준으로 조회
            /*if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_FORM";
            }*/

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            //inTable.Columns.Add("EQPTLEVEL", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            // 22.11.09 정연준 활성화도 대표설비 대신 main설비 기준으로 조회
            /*if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                dr["EQPTLEVEL"] = "M,C";
            }*/
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
        private bool event_valridtion()
        {
            if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
            {
                // 질문1 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            return true;
        }

        private void btnEMSWOReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!event_valridtion())
                {
                    return;
                }

                // 전송 하시겠습니까?
                Util.MessageConfirm("SFU3609", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable inData = new DataTable("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _grid_eqpt;
                        row["USERID"] = LoginInfo.USERID;

                        inData.Rows.Add(row);

                        new ClientProxy().ExecuteService("BR_EQPT_EQPTLOSS_BM_WO_TO_EMS", "INDATA", null, inData, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1880"); //전송 완료 되었습니다.
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

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
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", true);
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

        private void SaveQA(DataSet ds)
        {
            // Line Check
            if (!this.GetPackApplyLIneByUI(_grid_eqsg))
            {
                return;
            }

            foreach (DataTable dt in ds.Tables)
            {
                DataTable dtCopy = dt.Copy();
                this.SaveQA(dtCopy, true);
            }
        }

        private void SaveQA(DataTable dtINDATA, bool isLineCheck = false)
        {
            // Valiation Check...
            // 일괄저장시에는 Line Check를 이미 한 상태이므로 그냥 진행.
            // 단일저장시에는 Line Check를 한 상태가 아니므로 Check후 진행
            if (!isLineCheck)
            {
                if (!this.GetPackApplyLIneByUI(_grid_eqsg))
                {
                    return;
                }
            }

            if (!CommonVerify.HasTableRow(dtINDATA))
            {
                return;
            }

            if (!CommonVerify.HasTableRow(this.dtQA))
            {
                return;
            }

            try
            {
                DataSet dsINDATA = new DataSet();
                string bizRuleName = "BR_EQPT_EQPTLOSS_REG_QN_ANS";
                // DTINDATA
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("AREAID");

                foreach (DataRow drINDATA in dtINDATA.Rows)
                {
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                dtINDATA.AcceptChanges();

                // DTINQA
                DataTable dtINQA = new DataTable("INQA");
                dtINQA.Columns.Add("QUESTION", typeof(string));
                dtINQA.Columns.Add("ANSWER", typeof(string));

                foreach (DataRow drQA in this.dtQA.Select())
                {
                    DataRow drINQA = dtINQA.NewRow();
                    drINQA["QUESTION"] = drQA["CBO_CODE"];
                    drINQA["ANSWER"] = drQA["ANSWER"];
                    dtINQA.Rows.Add(drINQA);
                }

                dsINDATA.Tables.Add(dtINDATA);
                dsINDATA.Tables.Add(dtINQA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetPackApplyLIneByUI(string equipmentSegmentID)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "PACK_APPLY_LINE_BY_UI";
                drRQSTDT["CBO_CODE"] = "COM001_014";
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    returnValue = false;
                }
                else
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Select())
                    {
                        if (drRSLTDT["ATTRIBUTE1"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE2"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE3"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE4"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                        if (drRSLTDT["ATTRIBUTE5"].ToString().Contains(equipmentSegmentID))
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnValue;
        }

        private void cboLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLoss.Text.Equals("-SELECT-"))
            {
                const string bizRuleName = "DA_BAS_SEL_EQPTLOSSDETLCODE_CBO";
                string[] arrColumn = { "LANGID", "AREAID", "PROCID", "EQPTID", "LOSS_CODE" };
                string IN_AREA = _grid_area.IsNullOrEmpty() ? cboArea.SelectedValue.ToString() : _grid_area;
                string IN_PROC = _grid_proc.IsNullOrEmpty() ? cboProcess.SelectedValue.ToString() : _grid_proc;
                string IN_EQPT = _grid_eqpt.IsNullOrEmpty() ? cboEquipment.SelectedValue.ToString() : _grid_eqpt;
                string IN_LOSSCODE = cboLoss.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLoss.SelectedValue.ToString();
                string[] arrCondition = { LoginInfo.LANGID, IN_AREA, IN_PROC, IN_EQPT, IN_LOSSCODE };
                CommonCombo.SetFindPopupCombo(bizRuleName, popLossDetl, arrColumn, arrCondition, (string)popLossDetl.SelectedValuePath, (string)popLossDetl.DisplayMemberPath);

                popLossDetl.SelectedText = string.Empty;
                popLossDetl.SelectedValue = string.Empty;
            }
            else if (cboLoss.Text.Equals("-SELECT-"))
            {
                popLossDetl.SelectedText = string.Empty;
                popLossDetl.SelectedValue = string.Empty;
            }

            // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
            popLossDetl_ValueChanged(null, null);
        }
    }
}
