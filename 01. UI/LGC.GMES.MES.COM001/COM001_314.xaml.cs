/*************************************************************************************
 Created Date : 2019.09.25
      Creator : LG CNS 김대근
   Decription : 금형관리 화면 신규 생성
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.25  LG CNS 김대근 : 금형관리 화면 신규 생성
  2021.09.02  정재홍 : C20210824-000521 - 탈착이력 조회 Tap 추가
  2022.09.30  정재홍 : C20220729-000561 - GMES 시스템 Tool 장착 위치 상세 표시를 위한 화면 변경 건
  2022.10.28  정재홍 : C20221019-000244 - 심 다이 전산화를 위한 CSR 요청 件
  2022.12.27  정재홍 : C20221128-000143 - 등록 Tool 목록 항목에 비고 컬럼 추가
  2023.03.21  윤지해 : C20221125-000006 - [생산PI] 금형관리시스템 개선 요청 (개별 금형의 상태정보 기록추가)_금형 상태 변경 관련 추가
  2023.05.19  양영재 : E20230404-000453 - [연마관리] 현재 투입 설비, 비고, 위치정보 컬럼 및 위치 변경 버튼 추가
  2023.07.06  정재홍 : E20230627-000498 - [생산PI] 연마관리 항목 내 누적사용횟수 추가
  2023.12.06  정재홍 : E20231211-000182 - 신규 표준 공구 Tool ID 추가
  2024.01.12  정재홍 : E20231227-000375 - [MES] MMD > Tool Master 표준화에 따른 특이작업 > Tool 사용이력 및 연마관리 화면의 User 편의성 증대를 위한 Interface 개선 건
  2024.01.18  오수현 : E20231031-000594 - WINDING 공정시 등록 Tool 목록 항목에 컬럼 추가 (양극 한계 사용 횟수, 음극 한계 사용 횟수)
  2024.04.19  안유수 : E20240321-001184 - TOOL 사용이력 및 처리자 TEXT CLEAR 로직 제거
  2025.03.06  이민형 : HD_OSS_0052 연마상세이력 타이틀 다국어 수정
  2025.03.12  이민형 : 선택 체크 박스 더블 클릭 시 에러, 수정.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_314.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_314 : UserControl, IWorkArea
    {
        #region [Variables & Constructor]
        private string _selectedWorkerID = string.Empty;
        private string _selectedPolishToolID = string.Empty;
        private bool isPolished = false;
        private Util _Util = new Util();
        private CommonCombo cbo = new CommonCombo();
        private string tarStatCode = string.Empty;  // 2023.03.21 추가
        private string _selectedCurrStatCode = string.Empty;  // 2023.03.21 추가
        private string _selectedLocationID = string.Empty; // 2023.05.18 추가
        bool isTabChang = false;

        public COM001_314()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }
        #endregion

        #region [Init Method]
        private void InitCombos()
        {
            /*============= 등록 Tool 목록 =============*/
            String[] sFilterT = { "", "TOOL_TYPE_CODE" };
            // Tool유형
            cbo.SetCombo(cb: cboToolTypeCode, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilterT, sCase: "COMMCODES");

            /// CSR : C20221019-000244 조회 추가
            // 라인
            String[] sFilterT1 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] childrenT1 = { cboToolEquipment };
            cbo.SetCombo(cboToolEquipmentSegment, sFilter: sFilterT1, cbChild: childrenT1, cs: CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT");

            // 공정
            cbo.SetCombo(cboToolProcess, cs: CommonCombo.ComboStatus.SELECT, sFilter: sFilterT1, cbChild: childrenT1, sCase: "PROCESS_TOOL");

            // 설비
            C1ComboBox[] parentsT = { cboToolEquipmentSegment, cboToolProcess };
            cbo.SetCombo(cboToolEquipment, cbParent: parentsT, cs: CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENT");

            String[] sFilterT2 = { "" };
            cbo.SetCombo(cboToolEquipmentMount, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilterT2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_ALL");


            /*============= 연마관리 =============*/
            String[] sFilter1 = { "", "TOOL_TYPE_CODE" };
            cbo.SetCombo(cb: cboPolishTypeCode, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODES");

            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", new string[] { "LANGID", "AREAID", "COM_TYPE_CODE" }, new string[] { LoginInfo.LANGID, Util.NVC(LoginInfo.CFG_AREA_ID), "REPAIR_TOOL_LOCATION_INFO" }, CommonCombo.ComboStatus.NONE, dgPolishList.Columns["LOCATION_ID"], "CBO_CODE", "CBO_NAME");

            //사용여부
            string[] sFilterPolUse = { "USE_FLAG" };
            cbo.SetCombo(cboPolishUse, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilterPolUse);

            // 라인
            String[] sFilterPolEqsg = { LoginInfo.CFG_AREA_ID };
            //C1ComboBox[] childrenPolEqsg = { cboPolishEquipmentSegment };
            cbo.SetCombo(cboPolishEquipmentSegment, sFilter: sFilterPolEqsg, cs: CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT");

            //공정
            cbo.SetCombo(cboPolishProcess, cs: CommonCombo.ComboStatus.NONE, sFilter: sFilterPolEqsg, sCase: "PROCESS_TOOL");       //ALL제외, 2024-11-28, 김선영

            /*============= Lot별 사용이력 =============*/
            string[] sFilter2 = { "", "TOOL_TYPE_CODE" };
            cbo.SetCombo(cb: cboToolTypeCode2, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

            String[] sFilter3 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] children = { cboEquipment };
            cbo.SetCombo(cb: cboEquipmentSegment, sFilter: sFilter3, cbChild: children, cs: CommonCombo.ComboStatus.ALL);

            cbo.SetCombo(cb: cboProcess, cs: CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, cbChild: children, sCase: "PROCESS_TOOL");

            C1ComboBox[] parents = { cboEquipmentSegment, cboProcess };
            cbo.SetCombo(cb: cboEquipment, cbParent: parents, cs: CommonCombo.ComboStatus.ALL);

            /*============= 탈착이력 조회 조건 =============*/
            //Tool유형
            cbo.SetCombo(cb: cboToolTypeCode4, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

            String[] sFilter4 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] children2 = { cboEquipment2 };
            //라인
            cbo.SetCombo(cboEquipmentSegment2, sFilter: sFilter4, cbChild: children2, cs: CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT");

            //공정
            cbo.SetCombo(cboProcess2, cs: CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, cbChild: children2, sCase: "PROCESS_TOOL");

            C1ComboBox[] parents2 = { cboEquipmentSegment2, cboProcess2 };
            //설비
            cbo.SetCombo(cboEquipment2, cbParent: parents2, cs: CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENT");

            //탈착사유
            SetCombo_AreaComCode(cboDesorption);

        }

        private void InitControls()
        {
            tbPolishAreaID.Text = LoginInfo.CFG_AREA_NAME;
            txtToolAreaID.Text = LoginInfo.CFG_AREA_NAME;
            txtToolAttrText.IsEnabled = false;
        }
        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombos();
            InitControls();
            this.Loaded -= UserControl_Loaded;
            ShowToolStatChange();
        }

        private void dtpFrom_Loaded(object sender, RoutedEventArgs e)
        {
            //Tab안에 있어서 Tab이 Loaded될 때마다 오늘 날짜로 초기화 되어서 따로 이벤트 활용하여 초기값 설정.
            LGCDatePicker datePicker = sender as LGCDatePicker;

            if (datePicker == null)
            {
                return;
            }

            //Lot별 사용이력 탭이 열릴 때만 시간 초기값으로 세팅
            if (tabToolUsageHistByLot.IsFocused)
            {
                datePicker.SelectedDateTime = DateTime.Today.AddDays(-7); //일주일
                datePicker.Loaded -= dtpFrom_Loaded;
            }
        }

        private void dtpFrom2_Loaded(object sender, RoutedEventArgs e)
        {
            //Tab안에 있어서 Tab이 Loaded될 때마다 오늘 날짜로 초기화 되어서 따로 이벤트 활용하여 초기값 설정.
            LGCDatePicker datePicker = sender as LGCDatePicker;

            if (datePicker == null)
            {
                return;
            }

            //Lot별 사용이력 탭이 열릴 때만 시간 초기값으로 세팅
            if (tabToolDesorptionHist.IsFocused)
            {
                datePicker.SelectedDateTime = DateTime.Today.AddDays(-7); //일주일
                datePicker.Loaded -= dtpFrom2_Loaded;
            }
        }

        private void dgToolList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToolList.CurrentRow != null && dgToolList.CurrentColumn.Name.Equals("TOOL_ID"))
            {
                COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                wndDetail.FrameOperation = this.FrameOperation;

                if (wndDetail == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndDetail, parameters);

                grdMain.Children.Add(wndDetail);
                wndDetail.BringToFront();
            }
            else if (dgToolList.CurrentRow != null && dgToolList.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "REMARKS"));
                //parameters[2] = getLoactionID(Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID")));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnCloseToolRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
        }

        private void dgToolListNM_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToolListNM.CurrentRow != null && dgToolListNM.CurrentColumn.Name.Equals("TOOL_ID"))
            {

                COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                wndDetail.FrameOperation = this.FrameOperation;

                if (wndDetail == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListNM.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListNM.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndDetail, parameters);

                grdMain.Children.Add(wndDetail);
                wndDetail.BringToFront();
            }
            else if (dgToolListNM.CurrentRow != null && dgToolListNM.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListNM.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListNM.CurrentRow.DataItem, "REMARKS"));
                //parameters[2] = getLoactionID(Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID")));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgToolListNM.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnCloseToolRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
        }

        private void dgToolListLC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToolListLC.CurrentRow != null && dgToolListLC.CurrentColumn.Name.Equals("TOOL_ID"))
            {

                COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                wndDetail.FrameOperation = this.FrameOperation;

                if (wndDetail == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListLC.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListLC.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndDetail, parameters);

                grdMain.Children.Add(wndDetail);
                wndDetail.BringToFront();
            }
            else if (dgToolListLC.CurrentRow != null && dgToolListLC.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListLC.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListLC.CurrentRow.DataItem, "REMARKS"));
                //parameters[2] = getLoactionID(Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID")));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgToolListLC.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnCloseToolRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
        }

        private void dgToolListPC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToolListPC.CurrentRow != null && dgToolListPC.CurrentColumn.Name.Equals("TOOL_ID"))
            {

                COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                wndDetail.FrameOperation = this.FrameOperation;

                if (wndDetail == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListPC.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListPC.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndDetail, parameters);

                grdMain.Children.Add(wndDetail);
                wndDetail.BringToFront();
            }
            else if (dgToolListPC.CurrentRow != null && dgToolListPC.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListPC.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListPC.CurrentRow.DataItem, "REMARKS"));
                //parameters[2] = getLoactionID(Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID")));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgToolListPC.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnCloseToolRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
        }

        private void dgToolListHO_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToolListHO.CurrentRow != null && dgToolListHO.CurrentColumn.Name.Equals("TOOL_ID"))
            {

                COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                wndDetail.FrameOperation = this.FrameOperation;

                if (wndDetail == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListHO.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListHO.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndDetail, parameters);

                grdMain.Children.Add(wndDetail);
                wndDetail.BringToFront();
            }
            else if (dgToolListHO.CurrentRow != null && dgToolListHO.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgToolListHO.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgToolListHO.CurrentRow.DataItem, "REMARKS"));
                //parameters[2] = getLoactionID(Util.NVC(DataTableConverter.GetValue(dgToolList.CurrentRow.DataItem, "TOOL_ID")));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgToolListHO.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnCloseToolRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
        }

        private void dgDynamicList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDynamicList.CurrentRow != null && dgDynamicList.CurrentColumn.Name.Equals("TOOL_ID"))
            {

                COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                wndDetail.FrameOperation = this.FrameOperation;

                if (wndDetail == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDynamicList.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDynamicList.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndDetail, parameters);

                grdMain.Children.Add(wndDetail);
                wndDetail.BringToFront();
            }
            else if (dgDynamicList.CurrentRow != null && dgDynamicList.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[2];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDynamicList.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDynamicList.CurrentRow.DataItem, "REMARKS"));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDynamicList.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnCloseToolRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
        }
        #endregion

        #region [등록TOOL목록]
        private void GetToolList()
        {
            try
            {
                // CSR : C20221019-000244 조회 추가
                if (cboToolEquipmentSegment.SelectedValue == null || cboToolEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
                {
                    // Line을선택하세요
                    Util.MessageValidation("SFU1223");
                    HideLoadingIndicator();
                    return;
                }

                //if (cboToolTypeCode.SelectedValue == null || cboToolTypeCode.SelectedValue.ToString().Equals(""))
                //{
                //    // Tool 유형을 선택 하세요
                //    Util.MessageValidation("SFU6056");
                //    HideLoadingIndicator();
                //    return;
                //}

                if (cboToolProcess.SelectedValue == null || cboToolProcess.SelectedValue.ToString().Equals("") || cboToolProcess.SelectedValue.ToString().Equals("SELECT"))
                {
                    // PROCESS
                    Util.MessageValidation("SFU1459");
                    HideLoadingIndicator();
                    return;
                }

                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("TOOL_MDL_CODE", typeof(string));
                inData.Columns.Add("ELTR_TYPE", typeof(string));
                // CSR : C20221019-000244 조회 추가
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("EQPT_MOUNT", typeof(string));
                inData.Columns.Add("DYNAMICCOLUMN", typeof(string));
                inData.Columns.Add("DANAMICVALUE", typeof(string));
                inData.Columns.Add("STD_TOOL_ID", typeof(string));

                DataRow row = inData.NewRow();
                //row["TOOL_ID"] = string.IsNullOrEmpty(txtToolID.Text) ? null : txtToolID.Text;
                //row["TOOL_TYPE_CODE"] = Util.NVC(cboToolTypeCode.SelectedValue);
                //row["AREAID"] = Util.NVC(LoginInfo.CFG_AREA_ID);
                //// CSR : C20221019-000244 조회 추가
                //row["PROCID"] = Util.NVC(cboToolProcess.SelectedValue);
                //row["EQPTID"] = Util.NVC(cboToolEquipment.SelectedValue);
                //row["EQPT_MOUNT"] = Util.NVC(cboToolEquipmentMount.SelectedValue);
                //row["DYNAMICCOLUMN"] = Util.NVC(cboToolAttr.SelectedValue);
                //row["DANAMICVALUE"] = string.IsNullOrEmpty(Util.NVC(cboToolAttr.SelectedValue)) ? "" : txtToolAttrText.Text;
                //row["STD_TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(txtStdToolID.Text)) ? null : Util.NVC(txtStdToolID.Text);

                row["LANGID"] = LoginInfo.LANGID;
                row["TOOL_ID"] = string.IsNullOrEmpty(txtToolID.Text) ? null : txtToolID.Text;
                row["TOOL_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(cboToolTypeCode.SelectedValue)) ? null : cboToolTypeCode.SelectedValue.ToString();
                row["AREAID"] = string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID) ? null : LoginInfo.CFG_AREA_ID;
                row["PROCID"] = string.IsNullOrEmpty(Util.NVC(cboToolProcess.SelectedValue)) ? null : cboToolProcess.SelectedValue.ToString();
                row["EQPTID"] = string.IsNullOrEmpty(Util.NVC(cboToolEquipment.SelectedValue)) ? null : cboToolEquipment.SelectedValue.ToString();
                row["EQPT_MOUNT"] = string.IsNullOrEmpty(Util.NVC(cboToolEquipmentMount.SelectedValue)) ? null : cboToolEquipmentMount.SelectedValue.ToString();
                row["DYNAMICCOLUMN"] = string.IsNullOrEmpty(Util.NVC(cboToolAttr.SelectedValue)) ? null : cboToolAttr.SelectedValue.ToString();
                string strToolAttr = string.IsNullOrEmpty(txtToolAttrText.Text) ? null : txtToolAttrText.Text;
                row["DANAMICVALUE"] = string.IsNullOrEmpty(Util.NVC(cboToolAttr.SelectedValue)) ? null : strToolAttr;
                row["STD_TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(txtStdToolID.Text)) ? null : txtStdToolID.Text;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_TOOL_MST_DYNAMIC", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        bool flag = false;

                        grdToolList.Visibility = Visibility.Visible;
                        grdDanamicList.Visibility = Visibility.Collapsed;

                        foreach (C1DataGrid dg in grdToolList.Children)
                        {
                            if (dg.Tag.Equals(cboToolTypeCode.SelectedValue as string))
                            {
                                // E20231031-000594 WINDING 공정시 [등록 Tool 목록] 컬럼 추가
                                SetGridAddColumn(dg);

                                dg.Visibility = Visibility.Visible;
                                Util.gridClear(dg);
                                Util.GridSetData(dg, result, this.FrameOperation);
                                flag = true;
                                break;
                            }
                            else
                            {
                                dg.Visibility = Visibility.Collapsed;
                            }
                        }

                        if (!flag)
                        {

                            #region 2024.10.15. 김영국 - GM1, 2, HM 에 데이터가 없으므로 주석처리함. (이상권 책임 요청)
                            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_AREA_TOOL_ATTR_MNGT_COLUMN", "RQSTDT", "RSLTDT", inData);

                            //if (dtResult.Rows.Count > 0)
                            //{
                            //    grdToolList.Visibility = Visibility.Collapsed;
                            //    grdDanamicList.Visibility = Visibility.Visible;

                            //    Util.gridClear(dgDynamicList);

                            //    dgDynamicList.Columns.Clear();

                            //    for (int i = 0; i < dtResult.Rows.Count; i++)
                            //    {
                            //        C1.WPF.DataGrid.DataGridTextColumn textColumn = new C1.WPF.DataGrid.DataGridTextColumn();
                            //        // DataGrid의 컬럼헤드
                            //        textColumn.Header = dtResult.Rows[i]["TOOL_TYPE_ATTR_CODE"].ToString();
                            //        // 데이터 바인딩
                            //        textColumn.Binding = new Binding(dtResult.Rows[i]["COLUMNNAME"].ToString());
                            //        // 컬럼 폭
                            //        //textColumn.Width = int32(100);
                            //        // DataGrid에 컬럼 추가
                            //        dgDynamicList.Columns.Add(textColumn);
                            //    }

                            //    // E20231031-000594 WINDING 공정시 [등록 Tool 목록] 컬럼 추가
                            //    SetGridAddColumn(dgDynamicList);

                            //    Util.GridSetData(dgDynamicList, result, FrameOperation, true);
                            //}
                            //else
                            //{
                            //    grdToolList.Visibility = Visibility.Visible;
                            //    grdDanamicList.Visibility = Visibility.Collapsed;

                            //    foreach (C1DataGrid dg in grdToolList.Children)
                            //    {
                            //        if (dg.Tag.Equals(""))
                            //        {
                            //            // E20231031-000594 WINDING 공정시 [등록 Tool 목록] 컬럼 추가
                            //            SetGridAddColumn(dg);

                            //            dg.Visibility = Visibility.Visible;
                            //            Util.gridClear(dg);
                            //            Util.GridSetData(dg, result, this.FrameOperation);
                            //            break;
                            //        }
                            //    }
                            //} 
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void btnSearchTool_Click(object sender, RoutedEventArgs e)
        {
            isTabChang = true;

            ShowLoadingIndicator();
            GetToolList();
        }

        private void tbToolID_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Double Click으로 만들어줌
            if (e.ClickCount == 2)
            {
                try
                {
                    TextBlock tb = sender as TextBlock;

                    if (tb == null)
                        return;

                    COM001_314_DETAIL wndDetail = new COM001_314_DETAIL();
                    wndDetail.FrameOperation = this.FrameOperation;

                    if (wndDetail == null)
                        return;

                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(tb.Text);
                    C1WindowExtension.SetParameters(wndDetail, parameters);

                    grdMain.Children.Add(wndDetail);
                    wndDetail.BringToFront();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }
        #endregion

        #region E20231031-000594 WINDING 공정시 [등록 Tool 목록] 컬럼 추가
        private void SetGridAddColumn(C1DataGrid dg)
        {
            if (Util.NVC(cboToolProcess.SelectedValue).Equals(Process.WINDING))
            {
                dg.Columns["CA_LIMIT_USE_COUNT"].Visibility = Visibility.Visible; // 양극 한계 사용 횟수
                dg.Columns["AN_LIMIT_USE_COUNT"].Visibility = Visibility.Visible; // 음극 한계 사용 횟수
            }
            else
            {
                dg.Columns["CA_LIMIT_USE_COUNT"].Visibility = Visibility.Collapsed;
                dg.Columns["AN_LIMIT_USE_COUNT"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [연마관리]
        private void GetPolishList()
        {
            try
            {
                // CSR : E20231227-000375 조회 추가
                if (cboPolishEquipmentSegment.SelectedValue == null || cboPolishEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
                {
                    // Line을선택하세요
                    Util.MessageValidation("SFU1223");
                    HideLoadingIndicator();
                    return;
                }

                DataTable inData = new DataTable();
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("STD_TOOL_ID", typeof(string));

                // CSR : E20231227-000375
                inData.Columns.Add("USE_FLAG", typeof(string));
                //inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                //////////////////////////////////////////////////////////

                DataRow row = inData.NewRow();
                row["AREAID"] = Util.NVC(LoginInfo.CFG_AREA_ID);
                row["TOOL_ID"] = string.IsNullOrEmpty(tbPolishToolID.Text) ? null : tbPolishToolID.Text;
                row["TOOL_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(cboPolishTypeCode.SelectedValue)) ? null : Util.NVC(cboPolishTypeCode.SelectedValue);
                row["LANGID"] = LoginInfo.LANGID;
                row["STD_TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(tbPolishStdToolID.Text)) ? null : Util.NVC(tbPolishStdToolID.Text);

                // CSR : E20231227-000375
                row["USE_FLAG"] = string.IsNullOrEmpty(Util.NVC(cboPolishUse.SelectedValue)) ? null : Util.NVC(cboPolishUse.SelectedValue);
                //row["EQSGID"] = string.IsNullOrEmpty(Util.NVC(cboPolishEquipmentSegment.SelectedValue)) ? null : Util.NVC(cboPolishEquipmentSegment.SelectedValue);
                row["PROCID"] = string.IsNullOrEmpty(Util.NVC(cboPolishProcess.SelectedValue)) ? null : Util.NVC(cboPolishProcess.SelectedValue);
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_POLISH_LIST_TOOL_L", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgPolishList);
                        Util.GridSetData(dgPolishList, result, this.FrameOperation);

                        if (isPolished)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgPolishList, "TOOL_ID", _selectedPolishToolID);
                            _selectedPolishToolID = string.Empty;
                            DataTableConverter.SetValue(dgPolishList.Rows[idx].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgPolishList.Rows[idx].DataItem, "CHK", true);
                            dgPolishList.ScrollIntoView(idx, 0);
                            isPolished = false;
                        }
                        if (!string.Equals(GetAreaType(), "E"))
                        {
                            dgPolishList.Columns["LOCATION_ID"].Visibility = Visibility.Collapsed;
                            btnLocationChange.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void GetPolishDetailHistory()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("LANGID", typeof(string));

                DataRow row = inData.NewRow();
                row["TOOL_ID"] = Util.NVC(_selectedPolishToolID);
                row["LANGID"] = LoginInfo.LANGID;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_POLISH_LIST_DETL_TOOL_L", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgPolishDetailHistory);
                        Util.GridSetData(dgPolishDetailHistory, result, this.FrameOperation);

                        tbAccumCnt.Text = GetAccumSum(result).ToString();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void Polish()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("RSN_NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["TOOL_ID"] = Util.NVC(_selectedPolishToolID);
                row["USERID"] = Util.NVC(_selectedWorkerID);
                row["RSN_NOTE"] = txtPolishRsn.Text;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("BR_PRD_REG_POLISH_TOOL_L", "INDATA", null, inData, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //정상처리되었습니다.
                        Util.MessageValidation("SFU1275", (result) =>
                        {
                            Util.gridClear(dgPolishDetailHistory);
                            isPolished = true;
                            GetPolishList();
                            GetPolishDetailHistory();

                            //txtPolishRsn.Clear();
                            //txtWorker.Clear();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //txtPolishRsn.Clear();
                        //txtWorker.Clear();
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void btnPolish_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedPolishToolID))
            {
                //연마할 금형을 선택하세요.
                Util.MessageValidation("SFU8132");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //선택된 연마처리자가 없습니다.
                Util.MessageValidation("SFU8133");
                return;
            }

            //[%1]을 연마처리 하시겠습니까?
            object[] _id = { _selectedPolishToolID };
            Util.MessageConfirm("SFU8134", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ShowLoadingIndicator();
                    Polish();
                }
            }, _id);

        }

        private void btnSearchPolishList_Click(object sender, RoutedEventArgs e)
        {
            isTabChang = true;

            _selectedPolishToolID = string.Empty;
            Util.gridClear(dgPolishDetailHistory);

            ShowLoadingIndicator();
            GetPolishList();

        }

        private void rbPolishList_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;
            if (rb == null || rb.DataContext == null)
                return;

            DataGridCellPresenter parentCell = rb.Parent as DataGridCellPresenter;
            if (parentCell == null || parentCell.Row == null || parentCell.DataGrid == null)
                return;

            if (rb.IsChecked.HasValue && rb.IsChecked.Value)
            {
                int idx = parentCell.Row.Index;
                string prevPolishToolID = _selectedPolishToolID;
                string currPolishToolID = Util.NVC(DataTableConverter.GetValue(parentCell.DataGrid.Rows[idx].DataItem, "TOOL_ID"));
                string currStatCode = Util.NVC(DataTableConverter.GetValue(parentCell.DataGrid.Rows[idx].DataItem, "CURR_TOOL_STAT_CODE"));
                tbLimitCnt.Text = Util.NVC(DataTableConverter.GetValue(parentCell.DataGrid.Rows[idx].DataItem, "LIMIT_POLISH_COUNT"));
                _selectedLocationID = Util.NVC(DataTableConverter.GetValue(parentCell.DataGrid.Rows[idx].DataItem, "LOCATION_ID"));

                if (!prevPolishToolID.Equals(currPolishToolID))
                {
                    ShowLoadingIndicator();

                    int prevIdx = _Util.GetDataGridRowIndex(dgPolishList, "TOOL_ID", prevPolishToolID);
                    if (prevIdx > -1)
                    {
                        DataTableConverter.SetValue(dgPolishList.Rows[prevIdx].DataItem, "CHK", false);
                    }

                    _selectedPolishToolID = currPolishToolID;
                    _selectedCurrStatCode = currStatCode;
                    dgPolishList.SelectedIndex = _Util.GetDataGridRowIndex(dgPolishList, "TOOL_ID", _selectedPolishToolID);
                    HideLoadingIndicator();
                }
            }
        }

        private void dgDynamicList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgDynamicList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LIMIT_USE_COUNT")))
                <= Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACCU_USE_COUNT"))))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF9A00"));
                }

                //비고 색변경
                if (e.Cell.Column.Name.Equals("REMARKS") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REMARKS")) != string.Empty)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgToolList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgToolList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LIMIT_USE_COUNT")))
                <= Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACCU_USE_COUNT"))))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF9A00"));
                }

                //비고 색변경
                if (e.Cell.Column.Name.Equals("REMARKS") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REMARKS")) != string.Empty)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    //if (e.Cell.Row.Type == DataGridRowType.Item)
                    //{
                    //    e.Cell.Presenter.Background = null;
                    //}
                }
            }));
        }

        private void dgPolishList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgPolishList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name != null && e.Cell.Column.Name.Equals("ACCU_POLISH_COUNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgPolishList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgPolishList.CurrentRow == null) return;
            if (dgPolishList.CurrentColumn == null) return;
            if (dgPolishList.CurrentColumn.Name == null) return;

            if (dgPolishList.CurrentColumn.Name.Equals("ACCU_POLISH_COUNT"))
            {
                //GetPolishDetailHistory();
                try
                {
                    ShowLoadingIndicator();
                    DataTable inData = new DataTable();
                    inData.Columns.Add("TOOL_ID", typeof(string));
                    inData.Columns.Add("LANGID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["TOOL_ID"] = Util.NVC(DataTableConverter.GetValue(dgPolishList.CurrentRow.DataItem, "TOOL_ID"));
                    row["LANGID"] = LoginInfo.LANGID;
                    inData.Rows.Add(row);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_POLISH_LIST_DETL_TOOL_L", "INDATA", "OUTDATA", inData, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                                throw exception;

                            Util.gridClear(dgPolishDetailHistory);
                            Util.GridSetData(dgPolishDetailHistory, result, this.FrameOperation);

                            tbAccumCnt.Text = GetAccumSum(result).ToString();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HideLoadingIndicator();
                        }
                    });

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    HideLoadingIndicator();
                }
            }
            #region 2023.03.21 C20221125-000006 - [생산PI] 금형관리시스템 개선 요청 (개별 금형의 상태정보 기록추가)_금형 상태 변경 관련 추가
            else if (dgPolishList.CurrentColumn.Name.Equals("CURR_TOOL_STAT_NAME"))
            {
                COM001_314_STAT_HIST wndStatHist = new COM001_314_STAT_HIST();
                wndStatHist.FrameOperation = this.FrameOperation;

                if (wndStatHist == null)
                    return;

                object[] parameters = new object[1];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgPolishList.CurrentRow.DataItem, "TOOL_ID"));
                C1WindowExtension.SetParameters(wndStatHist, parameters);

                wndStatHist.Closed += new EventHandler(OnCloseToolStatHist);
                this.Dispatcher.BeginInvoke(new Action(() => wndStatHist.ShowModal()));
            }
            // 비고란 추가 2023.05.08
            else if (dgPolishList.CurrentColumn.Name.Equals("REMARKS"))
            {
                COM001_314_REMARK wndRemark = new COM001_314_REMARK();
                wndRemark.FrameOperation = this.FrameOperation;

                if (wndRemark == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgPolishList.CurrentRow.DataItem, "TOOL_ID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgPolishList.CurrentRow.DataItem, "REMARKS"));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgPolishList.CurrentRow.DataItem, "STD_TOOL_ID"));
                C1WindowExtension.SetParameters(wndRemark, parameters);

                wndRemark.Closed += new EventHandler(OnClosePolishRemark);
                this.Dispatcher.BeginInvoke(new Action(() => wndRemark.ShowModal()));
            }
            #endregion
        }

        private void btnSearchWorker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn == null)
                    return;

                COM001_WORKER wndWorker = new COM001_WORKER();
                wndWorker.FrameOperation = this.FrameOperation;
                wndWorker.Closed += new EventHandler(WndWorker_Closed);

                if (wndWorker == null)
                    return;

                object[] parameters = new object[3];
                parameters[0] = string.IsNullOrEmpty(Util.NVC(txtWorker.Text)) ? null : Util.NVC(txtWorker.Text);
                parameters[1] = Util.NVC(LoginInfo.CFG_AREA_ID);
                parameters[2] = string.IsNullOrEmpty(Util.NVC(cboProcess.SelectedValue)) ? null : Util.NVC(cboProcess.SelectedValue);
                C1WindowExtension.SetParameters(wndWorker, parameters);

                grdMain.Children.Add(wndWorker);
                wndWorker.BringToFront();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void WndWorker_Closed(object sender, EventArgs e)
        {
            COM001_WORKER wndWorker = sender as COM001_WORKER;

            if (wndWorker == null)
                return;

            if (wndWorker.DialogResult == MessageBoxResult.OK)
            {
                _selectedWorkerID = Util.NVC(wndWorker.SelectedUserID);
                txtWorker.Text = Util.NVC(wndWorker.SelectedUserName);
            }
        }

        private void cboProcess2_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCombo_AreaComCode(cboDesorption);
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboToolEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    // 자재 투입위치 코드
                    string sEQPTID = cboToolEquipment.SelectedValue.ToString();
                    if (!Util.IsNVC(sEQPTID) && !sEQPTID.Equals("-ALL-"))
                    {
                        String[] sFilter = { sEQPTID };
                        cbo.SetCombo(cboToolEquipmentMount, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "EQPT_CURR_MOUNT_MTRL_CBO_ALL");
                    }
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 2023.03.21 C20221125-000006 - [생산PI] 금형관리시스템 개선 요청 (개별 금형의 상태정보 기록추가)_금형 상태 변경 관련 추가
        #region 상태조회 팝업 닫기 이벤트
        private void OnCloseToolStatHist(object sender, EventArgs e)
        {
        }
        #endregion
        #region 필수입력 validation
        private bool StateChangeValidation()
        {
            if (string.IsNullOrEmpty(_selectedPolishToolID))
            {
                // 상태를 변경할 금형을 선택하세요.
                Util.MessageValidation("SFU8193");
                return false;
            }

            if (string.IsNullOrEmpty(_selectedWorkerID))
            {
                // 선택된 처리자가 없습니다.
                Util.MessageValidation("SFU8194");
                return false;
            }

            return true;
        }
        #endregion

        #region biz call
        private void ChangeState(string toolStat)
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("TAR_TOOL_STAT", typeof(string));
                inData.Columns.Add("ACT_USERID", typeof(string));
                inData.Columns.Add("RSN_NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["TOOL_ID"] = Util.NVC(_selectedPolishToolID);
                row["TAR_TOOL_STAT"] = toolStat;
                row["ACT_USERID"] = Util.NVC(_selectedWorkerID);
                row["RSN_NOTE"] = txtPolishRsn.Text;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("BR_PRD_REG_TOOL_STAT_CHANGE", "INDATA", null, inData, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상처리되었습니다.
                        Util.MessageValidation("SFU1275", (result) =>
                        {
                            if (toolStat.Equals("WAIT") && _selectedCurrStatCode.Equals("OUT"))
                            {
                                Util.gridClear(dgPolishDetailHistory);
                                GetPolishDetailHistory();
                            }
                            isPolished = true;
                            GetPolishList();
                            //txtPolishRsn.Clear();
                            //txtWorker.Clear();
                            //_selectedWorkerID = string.Empty;
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //txtPolishRsn.Clear();
                        //txtWorker.Clear();
                        //_selectedWorkerID = string.Empty;
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region 연마반출 EVENT
        // UNMOUNT > OUT
        private void btnPolishOut_Click(object sender, RoutedEventArgs e)
        {
            tarStatCode = "OUT";

            if (StateChangeValidation())
            {
                if (_selectedCurrStatCode.Equals("UNMOUNT") || string.IsNullOrEmpty(_selectedCurrStatCode))
                {

                    // [%1]의 상태를 변경하시겠습니까?
                    object[] _id = { _selectedPolishToolID };
                    Util.MessageConfirm("SFU8195", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ShowLoadingIndicator();
                            ChangeState(tarStatCode);
                        }
                    }, _id);
                }
                else
                {
                    // 현재 UNMOUNT 상태가 아닙니다.
                    Util.MessageValidation("SFU8197");
                }
            }
        }
        #endregion

        #region 연마 후 반입 EVENT
        // OUT > WAIT
        private void btnFinishPolish_Click(object sender, RoutedEventArgs e)
        {
            tarStatCode = "WAIT";

            if (StateChangeValidation())
            {
                if (_selectedCurrStatCode.Equals("OUT") || string.IsNullOrEmpty(_selectedCurrStatCode))
                {
                    // [%1]의 상태를 변경하시겠습니까?
                    object[] _id = { _selectedPolishToolID };
                    Util.MessageConfirm("SFU8195", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ShowLoadingIndicator();
                            ChangeState(tarStatCode);
                        }
                    }, _id);
                }
                else
                {
                    // 현재 POLISHING(OUT) 상태가 아닙니다.
                    Util.MessageValidation("SFU8196");
                }
            }
        }
        #endregion

        #region 장착대기 EVENT
        // UNMOUNT > WAIT
        private void btnWait_Click(object sender, RoutedEventArgs e)
        {
            tarStatCode = "WAIT";

            if (StateChangeValidation())
            {
                if (_selectedCurrStatCode.Equals("UNMOUNT") || string.IsNullOrEmpty(_selectedCurrStatCode))
                {
                    // [%1]의 상태를 변경하시겠습니까?
                    object[] _id = { _selectedPolishToolID };
                    Util.MessageConfirm("SFU8195", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ShowLoadingIndicator();
                            ChangeState(tarStatCode);
                        }
                    }, _id);
                }
                else
                {
                    // 현재 UNMOUNT 상태가 아닙니다.
                    Util.MessageValidation("SFU8197");
                }
            }
        }
        #endregion

        #region 반출대기 EVENT
        // WAIT > UNMOUNT
        private void btnUnmount_Click(object sender, RoutedEventArgs e)
        {
            tarStatCode = "UNMOUNT";

            if (StateChangeValidation())
            {
                if (_selectedCurrStatCode.Equals("WAIT") || string.IsNullOrEmpty(_selectedCurrStatCode))
                {

                    // [%1]의 상태를 변경하시겠습니까?
                    object[] _id = { _selectedPolishToolID };
                    Util.MessageConfirm("SFU8195", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ShowLoadingIndicator();
                            ChangeState(tarStatCode);
                        }
                    }, _id);
                }
                else
                {
                    // 현재 WAIT 상태가 아닙니다.
                    Util.MessageValidation("SFU8198");
                }
            }
        }
        #endregion

        #region 연마반출 취소 EVENT
        // OUT > UNMOUNT
        private void btnCancelPolish_Click(object sender, RoutedEventArgs e)
        {
            tarStatCode = "UNMOUNT";

            if (StateChangeValidation())
            {
                if (_selectedCurrStatCode.Equals("OUT") || string.IsNullOrEmpty(_selectedCurrStatCode))
                {
                    // [%1]의 상태를 변경하시겠습니까?
                    object[] _id = { _selectedPolishToolID };
                    Util.MessageConfirm("SFU8195", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ShowLoadingIndicator();
                            ChangeState(tarStatCode);
                        }
                    }, _id);
                }
                else
                {
                    // 현재 POLISHING(OUT) 상태가 아닙니다.
                    Util.MessageValidation("SFU8196");
                }
            }
        }
        #endregion

        #endregion
        #endregion

        #region [LOT별 사용이력]
        private void btnToolUsageHist_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboProcess.SelectedValue).Equals("SELECT"))
            {
                //공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return;
            }

            ShowLoadingIndicator();
            GetToolUsageHistByLot();
        }

        private void GetToolUsageHistByLot()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("FROM_DTTM", typeof(DateTime));
                inData.Columns.Add("TO_DTTM", typeof(DateTime));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                inData.Columns.Add("ROW_COUNT", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("CUT_ID", typeof(string));
                inData.Columns.Add("STD_TOOL_ID", typeof(string));

                DataRow row = inData.NewRow();
                if (!string.IsNullOrEmpty(Util.NVC(txtLotID.Text)))
                {
                    row["LANGID"] = LoginInfo.LANGID;
                    row["FROM_DTTM"] = dtpFrom.SelectedDateTime;
                    row["TO_DTTM"] = dtpTo.SelectedDateTime;
                    row["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                    row["ROW_COUNT"] = Util.NVC(txtRowCount.Text);
                    if (cboProcess.SelectedValue.Equals("E4000"))
                        row["CUT_ID"] = string.IsNullOrEmpty(Util.NVC(txtLotID.Text)) ? null : Util.NVC(txtLotID.Text);
                    else
                        row["LOTID"] = string.IsNullOrEmpty(Util.NVC(txtLotID.Text)) ? null : Util.NVC(txtLotID.Text);
                }
                else
                {
                    row["LANGID"] = LoginInfo.LANGID;
                    row["FROM_DTTM"] = dtpFrom.SelectedDateTime;
                    row["TO_DTTM"] = dtpTo.SelectedDateTime;
                    row["EQSGID"] = string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment.SelectedValue)) ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    row["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                    row["EQPTID"] = string.IsNullOrEmpty(Util.NVC(cboEquipment.SelectedValue)) ? null : Util.NVC(cboEquipment.SelectedValue);
                    row["TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(txtToolID2.Text)) ? null : Util.NVC(txtToolID2.Text);
                    row["TOOL_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(cboToolTypeCode2.SelectedValue)) ? null : Util.NVC(cboToolTypeCode2.SelectedValue);
                    row["ROW_COUNT"] = Util.NVC(txtRowCount.Text);
                    row["STD_TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(txtStdToolID2.Text)) ? null : Util.NVC(txtStdToolID2.Text);
                }
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_USED_HIST_BY_LOTID_TOOL_L", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgToolUsageHistByLot);
                        Util.GridSetData(dgToolUsageHistByLot, result, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region [탈착이력 조회]
        private void btnDesorptionSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboProcess2.SelectedValue).Equals("SELECT"))
            {
                //공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return;
            }

            ShowLoadingIndicator();
            GetDesorptionHistory();
        }

        private void GetDesorptionHistory()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("FROM_DTTM", typeof(DateTime));
                inData.Columns.Add("TO_DTTM", typeof(DateTime));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                inData.Columns.Add("TOOL_DESORPTION", typeof(string));
                inData.Columns.Add("ROW_COUNT", typeof(string));
                inData.Columns.Add("STD_TOOL_ID", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["FROM_DTTM"] = dtpFrom2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                row["TO_DTTM"] = dtpTo2.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                row["EQSGID"] = string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment2.SelectedValue)) ? null : Util.NVC(cboEquipmentSegment2.SelectedValue);
                row["PROCID"] = Util.NVC(cboProcess2.SelectedValue);
                row["EQPTID"] = string.IsNullOrEmpty(Util.NVC(cboEquipment2.SelectedValue)) ? null : Util.NVC(cboEquipment2.SelectedValue);
                row["TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(tbToolID3.Text)) ? null : Util.NVC(tbToolID3.Text);
                row["TOOL_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(cboToolTypeCode4.SelectedValue)) ? null : Util.NVC(cboToolTypeCode4.SelectedValue);
                row["TOOL_DESORPTION"] = string.IsNullOrEmpty(Util.NVC(cboDesorption.SelectedValue)) ? null : Util.NVC(cboDesorption.SelectedValue);
                row["ROW_COUNT"] = Util.NVC(txtRowCount2.Text);
                row["STD_TOOL_ID"] = string.IsNullOrEmpty(Util.NVC(tbStdToolID3.Text)) ? null : Util.NVC(tbStdToolID3.Text);
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DESORPTION_HIST_TOOL_L", "RQSTDT", "RSLTDT", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgdesorptionHist);
                        Util.GridSetData(dgdesorptionHist, result, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region [Util Method]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Visible)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Collapsed)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private int GetAccumSum(DataTable dt)
        {
            int sum = 0;
            foreach (DataRowView drv in dt.DefaultView)
            {
                int tmp = Util.NVC_Int(drv["ACCU_USE_COUNT"]);
                sum += tmp;
            }
            return sum;
        }

        private void SetCombo_AreaComCode(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTR1", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                drnewrow["COM_TYPE_CODE"] = "TOOL_UNMNT_RSN_CODE";
                drnewrow["ATTR1"] = Util.NVC(cboProcess2.SelectedValue);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void OnCloseToolRemark(object sender, EventArgs e)
        {
            COM001_314_REMARK window = sender as COM001_314_REMARK;

            //if (window.DialogResult == MessageBoxResult.OK)
            {
                GetToolList();
            }
        }
        #region 2023.03.21 C20221125-000006 - [생산PI] 금형관리시스템 개선 요청 (개별 금형의 상태정보 기록추가)_금형 상태 변경 관련 추가
        #region Tool 상태 변경 관련 UI Visibility 적용
        private void ShowToolStatChange()
        {
            if (ChkToolStatChangeArea())
            {
                btnPolish.Visibility = Visibility.Collapsed;

                btnPolishOut.Visibility = Visibility.Visible;
                btnFinishPolish.Visibility = Visibility.Visible;
                btnCancelPolish.Visibility = Visibility.Visible;
                btnWait.Visibility = Visibility.Visible;
                btnUnmount.Visibility = Visibility.Visible;
                btnLocationChange.Visibility = Visibility.Visible;

                dgPolishList.Columns["CURR_TOOL_STAT_NAME"].Visibility = Visibility.Visible;
            }
            else
            {
                btnPolish.Visibility = Visibility.Visible;

                btnPolishOut.Visibility = Visibility.Collapsed;
                btnFinishPolish.Visibility = Visibility.Collapsed;
                btnCancelPolish.Visibility = Visibility.Collapsed;
                btnWait.Visibility = Visibility.Collapsed;
                btnUnmount.Visibility = Visibility.Collapsed;
                btnLocationChange.Visibility = Visibility.Collapsed;

                dgPolishList.Columns["CURR_TOOL_STAT_NAME"].Visibility = Visibility.Collapsed;
                dgPolishList.Columns["LOCATION_ID"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion 
        #region TOOL 상태 변경 적용동인지 확인
        private bool ChkToolStatChangeArea()
        {
            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "TOOL_STAT_CHANGE_AREA";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }
        #endregion
        #endregion
        #endregion

        private void cboToolTypeCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sToolTypeCode = Util.NVC(cboToolTypeCode.SelectedValue);

            string[] sFilter2 = { LoginInfo.LANGID, sToolTypeCode };
            cbo.SetCombo(cb: cboToolAttr, cs: CommonCombo.ComboStatus.NA, sFilter: sFilter2, sCase: "TOOL_TYPE_CODE_ATTR_CBO");
        }

        private void cboToolAttr_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboToolAttr.SelectedValue.Equals(""))
                txtToolAttrText.IsEnabled = false;
            else
                txtToolAttrText.IsEnabled = true;

            txtToolAttrText.Text = String.Empty;
        }

        //5월 19일 추가
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return "";
        }

        private void btnLocationChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("TOOL_ID", typeof(string));
                RQSTDT.Columns.Add("REMARKS", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("LOCATION_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TOOL_ID"] = Util.NVC(_selectedPolishToolID);
                dr["REMARKS"] = Convert.ToString(DataTableConverter.GetValue(dgPolishList.SelectedItem, "REMARKS"));
                dr["USERID"] = LoginInfo.USERID;
                dr["LOCATION_ID"] = Util.NVC(_selectedLocationID);
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_REG_TOOL_INFO", "RQSTDT", null, RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.AlertInfo("SFU1270");  //저장되었습니다.
                    GetPolishList();
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

        private void dgPolishList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (!dg.CurrentCell.IsEditing)
                {
                    switch (dg.CurrentCell.Column.Name)
                    {
                        case "LOCATION_ID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOCATION_ID")?.ToString().Length > 0)
                                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", new string[] { "LANGID", "AREAID", "COM_TYPE_CODE" }, new string[] { LoginInfo.LANGID, Util.NVC(LoginInfo.CFG_AREA_ID), "REPAIR_TOOL_LOCATION_INFO" }, CommonCombo.ComboStatus.NONE, dgPolishList.Columns["CBO_NAME"], "CBO_CODE", "CBO_NAME");

                            int idx = dg.CurrentCell.Row.Index;
                            _selectedLocationID = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.DataGrid.Rows[idx].DataItem, "LOCATION_ID"));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OnClosePolishRemark(object sender, EventArgs e)
        {
            COM001_314_REMARK window = sender as COM001_314_REMARK;

            //if (window.DialogResult == MessageBoxResult.OK)
            {
                GetPolishList();
            }
        }

        private string getLoactionID(string toolID)
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                inData.Columns.Add("LANGID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = Util.NVC(LoginInfo.CFG_AREA_ID);
                row["TOOL_ID"] = toolID;
                row["TOOL_TYPE_CODE"] = null;
                row["LANGID"] = LoginInfo.LANGID;
                inData.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLISH_LIST_TOOL_L", "INDATA", "OUTDATA", inData);

                if (dtResult != null)
                    return Util.NVC(dtResult.Rows[0]["LOCATION_ID"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return "";
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isTabChang == false) return;

            C1TabControl tb = sender as C1TabControl;

            if (tb.SelectedIndex == 0)
            {
                GetToolList();
            }
            else if (tb.SelectedIndex == 1)
            {
                _selectedPolishToolID = string.Empty;
                Util.gridClear(dgPolishDetailHistory);

                ShowLoadingIndicator();
                GetPolishList();
            }

        }
    }
}