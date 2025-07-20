/*************************************************************************************
 Created Date : 2018.04.18
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.18  신광희 : Initial Created.
  2019.04.29  정문교 : 폴란드3동 대응 Carrier ID(CSTID) 조회조건 추가
  2021.07.15  김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
  2022.05.24  오화백 : 조회시   ShowLoadingIndicator(); 위치 변경
**************************************************************************************/

using C1.WPF;
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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_230 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private string _prodLotId = string.Empty;
        private string _prodWipSeq = string.Empty;
        private string _MTRLID = string.Empty;
        private bool _isLoad = true;

        public COM001_230()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeCombo()
        {
            CommonCombo cbo = new CommonCombo();

            // 동 정보 조회
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            cbo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);
            // 라인 정보 조회
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            cbo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);
            // 공정 정보 조회
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            cbo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);
            if (cboProcess.Items.Count < 1)
                SetProcess();
            else
            {
                if (cboProcess != null && cboProcess.ItemsSource != null)
                {
                    DataTable dtTmp = DataTableConverter.Convert(cboProcess.ItemsSource);

                    if (dtTmp != null)
                    {
                        if (dtTmp?.Select("CBO_CODE = '" + Process.CT_INSP + "'")?.Length > 0)
                            dtTmp.Rows.Remove(dtTmp.Select("CBO_CODE = '" + Process.CT_INSP + "'")[0]);

                        cboProcess.ItemsSource = null;
                        cboProcess.ItemsSource = dtTmp.Copy().AsDataView();
                        cboProcess.SelectedIndex = 0;
                    }
                }
            }

            // 설비 정보 조회
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            cbo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);
            //경로유형
            //string[] sFilter = { "FLOWTYPE" };
            //cbo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;




            
            //집계 처리 검색 정보 기본 설정
            C1ComboBox[] cboAreaChild_s = { cboEquipmentSegment_s };
            cbo.SetCombo(cboArea_s, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild_s, sCase: "AREA");

            C1ComboBox[] cboEquipmentSegmentParent_s = { cboArea_s };
            cbo.SetCombo(cboEquipmentSegment_s, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent_s, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboProcessParent_s = { cboEquipmentSegment_s };
            cbo.SetCombo(cboProcess_s, CommonCombo.ComboStatus.SELECT, null, cboProcessParent_s, sCase: "PROCESS") ;
            if (cboProcess_s.Items.Count < 1)
                SetProcess_s();
            else
            {
                if (cboProcess_s != null && cboProcess_s.ItemsSource != null)
                {
                    DataTable dtTmp = DataTableConverter.Convert(cboProcess_s.ItemsSource);

                    if (dtTmp != null)
                    {
                        if (dtTmp?.Select("CBO_CODE = '" + Process.CT_INSP + "'")?.Length > 0)
                            dtTmp.Rows.Remove(dtTmp.Select("CBO_CODE = '" + Process.CT_INSP + "'")[0]);

                        cboProcess_s.ItemsSource = null;
                        cboProcess_s.ItemsSource = dtTmp.Copy().AsDataView();
                        cboProcess_s.SelectedIndex = 0;
                    }
                }
            }

            C1ComboBox[] cboEquipmentParent_s = { cboEquipmentSegment_s, cboProcess_s };
            cbo.SetCombo(cboEquipment_s, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent_s, sCase: "EQUIPMENT");

            cboEquipmentSegment_s.SelectedItemChanged += cboEquipmentSegment_s_SelectedItemChanged;
            cboProcess_s.SelectedItemChanged += cboProcess_s_SelectedItemChanged; 

            //전공정 Loss 처리 하기 위한 값
            cbo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            string[] sFilter = { "APPR_BIZ_CODE" };
            cbo.SetCombo(cboReqTypeHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            string[] sFilter1 = { "REQ_RSLT_CODE" };
            cbo.SetCombo(cboReqRsltHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);


            cboReqTypeHist.SelectedValue = "LOT_SCRAP_YIELD";
            cboReqTypeHist.IsEditable = false;
            cboReqTypeHist.IsEnabled = false;

            //생산구분
            string[] sFilter2 = new string[] { "PRODUCT_DIVISION" };
            cbo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            //생산구분
            string[] sFilter3 = new string[] { "PRODUCT_DIVISION" };
            cbo.SetCombo(cboProductDivHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);

            // 생산구분 Default 정상생산
            if (cboProductDivHist.Items.Count > 1)
                cboProductDivHist.SelectedIndex = 1;

            //생산구분
            string[] sFilter4 = new string[] { "PRODUCT_DIVISION" };
            cbo.SetCombo(cboProductDiv_s, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter4);

            // 생산구분 Default 정상생산
            if (cboProductDiv_s.Items.Count > 1)
                cboProductDiv_s.SelectedIndex = 1;





            // 동 정보 조회
            C1ComboBox[] cboAreaChild_gdw = { cboEquipmentSegment_gdw };
            cbo.SetCombo(cboArea_gdw, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild_gdw, sCase: "cboArea");
            // 라인 정보 조회
            C1ComboBox[] cboEquipmentSegmentChild_gdw = { cboEquipment_gdw };// { cboProcess_gdw, cboEquipment_gdw };
            C1ComboBox[] cboEquipmentSegmentParent_gdw = { cboArea_gdw };
            cbo.SetCombo(cboEquipmentSegment_gdw, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent_gdw, cbChild: cboEquipmentSegmentChild_gdw, sCase: "cboEquipmentSegment");
            // 공정 정보 조회
            //C1ComboBox[] cboProcessChild_gdw = { cboEquipment_gdw };
            //C1ComboBox[] cboProcessParent_gdw = { cboArea_gdw, cboEquipmentSegment_gdw };
            //cbo.SetCombo(cboProcess_gdw, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent_gdw, cbChild: cboProcessChild_gdw,  sCase: "PROCESSWITHAREA");


            // 공정 라미로 임시 고정..
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CBO_CODE"] = Process.LAMINATION;
            dr["CBO_NAME"] = Process.LAMINATION + " : Lamination";
            dt.Rows.Add(dr);

            cboProcess_gdw.ItemsSource = dt.Copy().AsDataView();
            cboProcess_gdw.DisplayMemberPath = "CBO_NAME";
            cboProcess_gdw.SelectedValuePath = "CBO_CODE";
            cboProcess_gdw.SelectedIndex = 0;





            // 설비 정보 조회
            C1ComboBox[] cboEquipmentParent_gdw = { cboEquipmentSegment_gdw, cboProcess_gdw };
            cbo.SetCombo(cboEquipment_gdw, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent_gdw, sCase: "cboEquipment");
            
            cboLotType.ApplyTemplate();
            SetLotTypeMultiCombo(cboLotType);

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            
            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

         
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotList();
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcess();
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                ClearValue();
            }
      
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {
            // 모델 AutoComplete
            if (_isLoad)
            {
                GetModel();
                _isLoad = false;
            }
        }

        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void txtDIFF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void txtCstId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (rb != null && DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0) || DataTableConverter.GetValue(rb.DataContext, "CHK").ToString() == "0")
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                GetSubLot();
                GetInputHistory();
            }
        }


        #region [완성LOT 탭] - dgSubLot_LoadedCellPresenter, dgSubLot_MouseDoubleClick(셀정보팝업), print_Button_Click(재발행)
        private void dgSubLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (cboProcess.SelectedValue.Equals(Process.PACKAGING) && e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));

        }
        private void dgSubLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgSubLot.CurrentRow != null && dgSubLot.CurrentColumn.Name.Equals("CSTID"))
                {
                    COM001_045_CELL wndPopup = new COM001_045_CELL();
                    wndPopup.FrameOperation = FrameOperation;

                    object[] parameters = new object[4];
                    parameters[0] = _prodLotId;
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSubLot.CurrentRow.DataItem, "LOTID"));
                    parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSubLot.CurrentRow.DataItem, "CSTID")); ;

                    C1WindowExtension.SetParameters(wndPopup, parameters);

                    //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    grdMain.Children.Add(wndPopup);
                    wndPopup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            Button bt = sender as Button;

            String sBoxID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

            if (!sBoxID.Equals(""))
            {
                // 발행..
                DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                if (_PROCID.Equals(Process.LAMINATION))
                {
                    dicParam.Add("reportName", "Lami");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "MAGAZINE ID");
                    dicParam.Add("B_prodLotId", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_prodWipSeq", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.
                    dicList.Add(dicParam);

                    CMM_THERMAL_PRINT_LAMI printlami = new CMM_THERMAL_PRINT_LAMI();
                    printlami.FrameOperation = FrameOperation;

                    if (printlami != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.LAMINATION;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.
                        Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                        C1WindowExtension.SetParameters(printlami, Parameters);
                        printlami.Show();
                    }

                }
                else if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    int iCopys = 2;

                    if (LoginInfo.CFG_THERMAL_COPIES > 0)
                    {
                        iCopys = LoginInfo.CFG_THERMAL_COPIES;
                    }

                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");
                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                    dicParam.Add("B_prodLotId", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_prodWipSeq", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.
                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD printfold = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    printfold.FrameOperation = FrameOperation;

                    if (printfold != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.STACKING_FOLDING;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(printfold, Parameters);
                        printfold.ShowModal();
                    }

                }

            }
            */
        }
        #endregion




        #endregion

        #region Mehod

        #region [BizCall]


        private void GetCaldate()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLotList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }
            
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                //dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                //dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("DIFF", typeof(Int16));
                dtRqst.Columns.Add("CSTID", typeof(string));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                ////dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;

                //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                string equipment = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(equipment) ? null : equipment;
                //dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if(!string.IsNullOrEmpty(txtLotId.Text))
                    dr["LOTID"] = Util.GetCondition(txtLotId);

                //if (!string.IsNullOrWhiteSpace(txtPRLOTID.Text))
                //{
                //    dr["PR_LOTID"] = Util.GetCondition(txtPRLOTID);
                //}
                //else if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                //{
                //    dr["LOTID"] = Util.GetCondition(txtLotId);
                //}

                if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                if (txtDIFF.Value > 0)
                    dr["DIFF"] = txtDIFF.Value;

                if (!string.IsNullOrEmpty(txtCstId.Text))
                    dr["CSTID"] = Util.GetCondition(txtCstId);

                // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

                dtRqst.Rows.Add(dr);
                
                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            dgLotList.MergingCells -= DgLotList_MergingCells;

                            ShowLoadingIndicator();
                            Util.GridSetData(dgLotList, searchResult, FrameOperation, true);

                            string[] sColumnName = { "LOTID", "LOTYNAME", "PRODID", "PRODNAME", "MODLID", "PRJT_NAME", "EQPTNAME", "CALDATE",  "MKT_TYPE_CODE", "UNIT_CODE", "INPUT_QTY", "COL_01", "WIPQTY2_ED", "COL_02", "CNFM_DFCT_QTY2", "COL_03", "CNFM_LOSS_QTY2", "COL_04", "CNFM_PRDT_REQ_QTY2", "COL_05", "WOID" };
                            _util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            if(cboEquipmentSegment.SelectedValue.ToString().LastIndexOf("H") != 3)
                            {
                                dgLotList.MergingCells += DgLotList_MergingCells;
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
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // CHK 컬럼 셀 Merge
        private void DgLotList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            if (cboProcess.SelectedValue.ToString() == Process.LAMINATION || cboProcess.SelectedValue.ToString() == Process.STACKING_FOLDING || cboProcess.SelectedValue.ToString() == Process.RWK_LNS)
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowCount = 0;
                for (int row = 0; row < (dg.Rows.Count - dg.TopRows.Count) - 1; row++)
                {
                    rowCount++;
                    int col = 0; // CHK
                    if (rowCount % 2 != 0)
                    {
                        if (dg.Columns[col].Name.Equals("CHK"))
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(row + dg.TopRows.Count, col), dg.GetCell(row + dg.TopRows.Count + 1, col)));
                        }
                        else
                        {
                            continue;
                        }
                    }

                }
            } else
            {
                return;
            }
        }

        private void GetSubLot()
        {
            try
            {
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _prodLotId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EDIT_SUBLOT_LIST", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgSubLot, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputHistory()
        {
            try
            {
                 string bizRuleName = "";

                if (cboProcess.SelectedValue.ToString().Equals(Process.NOTCHING)) {
                    bizRuleName = "DA_PRD_SEL_WIPHISTORYATTR_INPUT_MTRL_NT";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WIPHISTORYATTR_INPUT_MTRL";
                }
          
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("MTRLID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _prodLotId;
                newRow["PROD_WIPSEQ"] = _prodWipSeq;
                newRow["MTRLID"] = _MTRLID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                              ShowLoadingIndicator();
                        Util.GridSetData(dgInputHistory, searchResult, FrameOperation);

                        if (dgInputHistory.CurrentCell != null)
                            dgInputHistory.CurrentCell = dgInputHistory.GetCell(dgInputHistory.CurrentCell.Row.Index, dgInputHistory.Columns.Count - 1);
                        else if (dgInputHistory.Rows.Count > 0)
                            dgInputHistory.CurrentCell = dgInputHistory.GetCell(dgInputHistory.Rows.Count, dgInputHistory.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetModel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //표시 텍스트
                    var keywordString = displayString;
                    txtModlId.AddItem(new AutoCompleteEntry(displayString, keywordString)); //표시 텍스트와 검색어 텍스트(배열)를 AutoCompleteTextBox의 Item에 추가한다.
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

        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", inTable);

                if (dtResult?.Select("CBO_CODE = '" + Process.CT_INSP + "'")?.Length > 0)
                    dtResult.Rows.Remove(dtResult.Select("CBO_CODE = '" + Process.CT_INSP + "'")[0]);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string processCode = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
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

        private void ClearValue()
        {
            _prodLotId = string.Empty;
            _prodWipSeq = string.Empty;
            _MTRLID = string.Empty;

            Util.gridClear(dgSubLot);
            Util.gridClear(dgInputHistory);
        }


        private void SetValue(object oContext)
        {
            _prodLotId = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _prodWipSeq = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _MTRLID = Util.NVC(DataTableConverter.GetValue(oContext, "MTRLID"));
        }


        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
       
            DoEvents();
            GetListHist();
           
        }
        public void GetListHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtLotIDHist).Equals("")) //lot id 가 없는 경우
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["APPR_BIZ_CODE"] = Util.GetCondition(cboReqTypeHist, bAllNull: true);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;

                    if (!Util.GetCondition(txtCSTIDHist).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDHist);

                    // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                    //if (cboProductDivHist.SelectedValue.ToString() == "P")
                    //    dr["NORMAL"] = cboProductDivHist.SelectedValue.ToString();
                    //else if (cboProductDivHist.SelectedValue.ToString() == "X")
                    //    dr["PILOT"] = cboProductDivHist.SelectedValue.ToString();
                    dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDivHist, bAllNull: true);

                    dtRqst.Rows.Add(dr);

                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST", "INDATA", "OUTDATA", dtRqst);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIDHist);

                    dtRqst.Rows.Add(dr);

                    //dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_LIST_BY_LOT", "INDATA", "OUTDATA", dtRqst);
                }
                ShowLoadingIndicator();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_REQ_HIST", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgList);

                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }


        private void dgListHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgListHist.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }

        private void dgInputHistory_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            dgInputHistory.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (!cboProcess.SelectedValue.Equals(Process.NOTCHING))
                {
                    string sInputMtlGubun = Util.NVC(DataTableConverter.GetValue(dgInputHistory.Rows[e.Cell.Row.Index].DataItem, "INPUT_DUPLI_MTL"));
                    if (sInputMtlGubun == "Y")
                    {                
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#EFE4B0"));
                    }
                }

                /*
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_LABEL_PRT_FLAG")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                }
                                else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROC_LABEL_PRT_FLAG")).Equals("N"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                }
                */
            }));
        }

        private void dgListHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgListHist.CurrentRow != null && dgListHist.CurrentColumn.Name.Equals("LOTID") && dgListHist.GetRowCount() > 0)
            {

                COM001_035_READ wndPopup = new COM001_035_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgListHist.CurrentRow.DataItem, "REQ_NO"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgListHist.CurrentRow.DataItem, "REQ_RSLT_CODE"));


                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

        
        //summary 처리
        private void btnSummmary_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GeSummaryList();
        }


        private void GeSummaryList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

              
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_SUMMARY";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                //dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                //dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("DIFF", typeof(Int16));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment_s);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;

                dr["PROCID"] = Util.GetCondition(cboProcess_s, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                string equipment = Util.GetCondition(cboEquipment_s);
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom_s);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo_s);

                if (!string.IsNullOrEmpty(txtLotId_s.Text))
                   dr["LOTID"] = Util.GetCondition(txtLotId_s);

                if (!string.IsNullOrWhiteSpace(txtModlId_s.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId_s.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName_s.Text))
                    dr["PRJT_NAME"] = txtPrjtName_s.Text;

                if (txtDIFF.Value > 0)
                    dr["DIFF"] = txtDIFF_s.Value;

                // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                //if (cboProductDiv_s.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv_s.SelectedValue.ToString();
                //else if (cboProductDiv_s.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv_s.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv_s, bAllNull: true);

                dtRqst.Rows.Add(dr);


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (searchResult, searchException) =>
                {
                    try
                    {
                        ShowLoadingIndicator();
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            HiddenLoadingIndicator();
                            return;
                        }

                        Util.GridSetData(dgSummaryList, searchResult, FrameOperation, true);

                        string[] sColumnName = { "CALDATE", "PRODID", "PRODNAME", "MODLID", "PRJT_NAME", "UNIT_CODE", "MTRLID", "EQSGID", "INPUT_QTY", "WIPQTY2_ED", "CNFM_DFCT_QTY2", "CNFM_LOSS_QTY2", "CNFM_PRDT_REQ_QTY2", "BOM_INPUT_QTY", "MTRL_INPUT_QTY", "DIFF", "YRATE", "BOM_OUT_QTY", "BEFORE_LOSS_QTY" };
                        _util.SetDataGridMergeExtensionCol(dgSummaryList, sColumnName, DataGridMergeMode.NONE);

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
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProcess_s()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea_s);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment_s);

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", inTable);

                if (dtResult?.Select("CBO_CODE = '" + Process.CT_INSP + "'")?.Length > 0)
                    dtResult.Rows.Remove(dtResult.Select("CBO_CODE = '" + Process.CT_INSP + "'")[0]);

                cboProcess_s.DisplayMemberPath = "CBO_NAME";
                cboProcess_s.SelectedValuePath = "CBO_CODE";
                cboProcess_s.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess_s.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess_s.SelectedIndex < 0)
                        cboProcess_s.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess_s.Items.Count > 0)
                        cboProcess_s.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_s_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment_s.Items.Count > 0 && cboEquipmentSegment_s.SelectedValue != null && !cboEquipmentSegment_s.SelectedValue.Equals("SELECT"))
            {
                SetProcess_s();
            }
        }

        private void cboProcess_s_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess_s.Items.Count > 0 && cboProcess_s.SelectedValue != null && !cboProcess_s.SelectedValue.Equals("SELECT"))
            {
                SetEquipment_s();

                Util.gridClear(dgSummaryList);
                ClearValue();
            }
        }

        private void SetEquipment_s()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea_s);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string processCode = Util.GetCondition(cboProcess_s);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment_s.ItemsSource = null;
                    return;
                }

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment_s);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                dr["PROCID"] = cboProcess_s.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment_s.DisplayMemberPath = "CBO_NAME";
                cboEquipment_s.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment_s.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment_s.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment_s.SelectedIndex < 0)
                        cboEquipment_s.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment_s.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotId_gdw_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotListGDW();
            }
        }

        private void txtModlId_gdw_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isLoad)
            {
                GetModelGDW();
                _isLoad = false;
            }
        }

        private void txtPrjtName_gdw_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotListGDW();
            }
        }

        private void btnSearch_gdw_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetLotListGDW();
        }

        private void dgLotListChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList_gdw.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (rb != null && DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                GetOutLotGDW(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "CALDATE")), Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PROCID")), Util.NVC(DataTableConverter.GetValue(rb.DataContext, "LOTID")));
                GetInputHistGDW(Util.NVC(DataTableConverter.GetValue(rb.DataContext, "LOTID")), Util.NVC(DataTableConverter.GetValue(rb.DataContext, "CALDATE")), Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PROCID")), Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MTRLID")));
            }
        }

        private void dgInputHistory_gdw_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void dgSubLot_gdw_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

        }

        private void dgSubLot_gdw_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void print_gdw_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetLotTypeMultiCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void GetLotListGDW()
        {
            try
            {
                if ((dtpDateTo_gdw.SelectedDateTime - dtpDateFrom_gdw.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                Util.gridClear(dgLotList_gdw);
                Util.gridClear(dgInputHistory_gdw);
                Util.gridClear(dgSubLot_gdw);

                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_PROD_LOT_GDW";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CALDATE_FR", typeof(DateTime));
                dtRqst.Columns.Add("CALDATE_TO", typeof(DateTime));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea_gdw, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;
                
                string equipmentSegment = Util.GetCondition(cboEquipmentSegment_gdw);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;

                dr["PROCID"] = Util.GetCondition(cboProcess_gdw, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                string equipment = Util.GetCondition(cboEquipment_gdw);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(equipment) ? null : equipment;

                if (!string.IsNullOrWhiteSpace(txtModlId_gdw.Text))
                    dr["MODLID"] = txtModlId_gdw.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId_gdw.Text))
                    dr["PRODID"] = txtProdId_gdw.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName_gdw.Text))
                    dr["PRJT_NAME"] = txtPrjtName_gdw.Text;


                dr["LOTTYPE"] = Util.NVC(cboLotType.SelectedItemsToString);

                if (!string.IsNullOrEmpty(txtLotId_gdw.Text))
                    dr["PROD_LOTID"] = Util.GetCondition(txtLotId_gdw);

                dr["CALDATE_FR"] = Convert.ToDateTime(dtpDateFrom_gdw.SelectedDateTime).ToString("yyyy-MM-dd");
                dr["CALDATE_TO"] = Convert.ToDateTime(dtpDateTo_gdw.SelectedDateTime).ToString("yyyy-MM-dd");

                //if (txtDIFF.Value > 0)
                //    dr["DIFF"] = txtDIFF.Value;

                //if (!string.IsNullOrEmpty(txtCstId.Text))
                //    dr["CSTID"] = Util.GetCondition(txtCstId);
                
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgLotList_gdw, searchResult, FrameOperation, true);

                        string[] sColumnName = { "LOTID", "LOTYNAME", "PRODID", "PRODNAME", "MODLID", "PRJT_NAME", "EQPTNAME", "CALDATE", "MKT_TYPE_NAME", "UNIT_CODE", "GOOD_QTY" };
                        _util.SetDataGridMergeExtensionCol(dgLotList_gdw, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutLotGDW(string sCalDate, string sProcID, string sLotID)
        {
            try
            {
                Util.gridClear(dgSubLot_gdw);

                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CALDATE", typeof(DateTime));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CALDATE"] = Convert.ToDateTime(sCalDate);
                dr["PROCID"] = sProcID;
                dr["PROD_LOTID"] = sLotID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_OUT_LOT_GDW", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgSubLot_gdw, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputHistGDW(string sLotID, string sCalDate, string sProcID, string sMtrlID)
        {
            try
            {
                Util.gridClear(dgInputHistory_gdw);

                string bizRuleName = "DA_PRD_SEL_LOT_LIST_INPUT_MATERIAL_INPUT_LOT_GDW";
                
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(DateTime));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CALDATE"] = Convert.ToDateTime(sCalDate);
                newRow["PROCID"] = sProcID;
                newRow["PROD_LOTID"] = sLotID;
                newRow["MTRLID"] = sMtrlID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHistory_gdw, searchResult, FrameOperation, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetModelGDW()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //표시 텍스트
                    var keywordString = displayString;
                    txtModlId_gdw.AddItem(new AutoCompleteEntry(displayString, keywordString)); //표시 텍스트와 검색어 텍스트(배열)를 AutoCompleteTextBox의 Item에 추가한다.
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
    }
}