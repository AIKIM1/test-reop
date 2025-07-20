/*************************************************************************************
 Created Date : 2022.12.15
      Creator : 이윤중
   Decription : Route 조회 화면 추가
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.15  이윤중 : Initial Created. (MMD - [Route 관리] 화면 기반)
  2023.10.18  조영대 : 다른화면에서 호출기능 추가 (EQSGID, MDLLOT_ID, ROUTID)
  2024.07.11  주경호 : Convert.ToDouble 호출 시 폴란드인 경우 Exception 발생하는 경우가 생겨서 정상 형변환 되도록 소스 수정함
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Globalization;

using C1.WPF;
using C1.WPF.DataGrid;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_ROUTE_MMD : UserControl, IWorkArea
    {
        #region Declaration & Constructor 


        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();

        Util _util = new Util();

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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public MessageBoxResult DialogResult { get; private set; }

        //LGC.GMES.MMD.MMD001.Common.CommonDataSet _Com = new LGC.GMES.MMD.MMD001.Common.CommonDataSet();

        DataView _dvCMCD { get; set; }
        DataView _dvCMCD_AREA { get; set; }
        DataView _dvSHOP { get; set; }
        DataView _dvAREA { get; set; }
        DataView _dvEQSG { get; set; }
        DataView _dvMODL { get; set; }
        DataView _dvPROC { get; set; }
        DataView _dvPROC_ROUT { get; set; }
        DataView _dvPROC_UNIT { get; set; }

        public DataTable TreeNodes
        {
            get;
            set;
        }

        private string _sTp1SHOPID = null;
        private string _sTp1AREAID = null;
        private string _sTp1AREANAME = null;
        private string _sTp1EQSGID = null;
        private string _sTp1MDLLOT_ID = null;

        private int _iRowIndex_Route = 0;
        private string _sCurProcId_Recipe = null;
        private string _sCurProcGr_Recipe = null;
        private string _sCurProcDetlType_Recipe = null;

        private string _sCurMeasrType = null;

        private string _sFormRoutProcRecipeEndTime = null;

        private ContextMenu contextMenu_Add_Judge_Proc = null;
        private ContextMenu contextMenu_Del_Judge_Proc = null;
        private ContextMenu contextMenu_Add_RJudge_Proc = null;
        private ContextMenu contextMenu_Del_RJudge_Proc = null;

        private C1TreeViewItem contextTreeItem_Judge_Proc = null;
        private C1TreeViewItem contextTreeItem_RJudge_Proc = null;

        private DataTable _dtRoutOper = new DataTable();

        private Dictionary<string, string> routeInfoArgument = null;

        public FORM001_ROUTE_MMD()
        {
            InitializeComponent();

            routeInfoArgument = null;
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        private void Initialize()
        {
            #region 다른 화면에서 호출시 처리
            object[] parameters = this.FrameOperation.Parameters;
            if (routeInfoArgument == null && parameters != null && parameters.Length >= 1)
            {
                string sEQSGID = Util.NVC(parameters[0]);
                string sMDLLOT_ID = Util.NVC(parameters[1]);
                string sROUT = Util.NVC(parameters[2]);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REPSTR_ID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["USERID"] = LoginInfo.USERID;
                inData["REPSTR_ID"] = LoginInfo.CFG_AREA_ID;
                inData["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["EQSGID"] = sEQSGID;
                inData["MDLLOT_ID"] = sMDLLOT_ID;
                inData["USE_FLAG"] = "Y";
                inDataTable.Rows.Add(inData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("MMD_SEL_FORM_ROUTE", "INDATA", "RSLTDT", inDataTable);
                DataRow drRoute = dtRslt.AsEnumerable().Where(w => w.Field<string>("ROUTID").Equals(sROUT)).FirstOrDefault();

                routeInfoArgument = new Dictionary<string, string>();
                routeInfoArgument.Add("AREAID", Util.NVC(drRoute["AREAID"]));
                routeInfoArgument.Add("EQSGID", Util.NVC(drRoute["EQSGID"]));
                routeInfoArgument.Add("MDLLOT_ID", Util.NVC(drRoute["MDLLOT_ID"]));
                routeInfoArgument.Add("ROUTID", Util.NVC(drRoute["ROUTID"]));
                routeInfoArgument.Add("ROUT_TYPE_CODE", Util.NVC(drRoute["ROUT_TYPE_CODE"])); ;
                routeInfoArgument.Add("DISP_AREA", Util.NVC(drRoute["DISP_AREA"]));
                routeInfoArgument.Add("DISP_ROUT", Util.NVC(drRoute["DISP_ROUT"]));

                TapRoute.SelectedItem = tpOpRoute;

                loadingIndicator2.Visibility = Visibility.Visible;
            }
            #endregion

            InitCombo();
            //InitControl();
            //SetEvent();
            //CommonUtil.DisableControlsByAuth(this, FrameOperation); //권한
        }

        #region 콤보박스

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                #region ACTION_COMPLETED

                #region CommonCode
                //IUSE : 사용유무, ROUT_TYPE_CODE : 라우트 유형 코드, ROUT_TYPE_GR_CODE : 라우트 그룹 코드, ROUT_RSLT_GR_CODE : 라우트 실적 그룹 코드, GRD_SLCTR_LOCATION_CODE : 선별 위치,  DFCT_GRD_FLAG : 불량 등급 여부, MEASR_TYPE_CODE : 측정유형코드, JUDG_MTHD_CODE : 상대판정 등급, JUDG_TYPE_CODE : 판정종류, REF_VALUE_CODE : 참조 기준값 코드, RJUDG_BAS_CODE : 상대판정 기준코드, STDEV1_FIX_FLAG : 표준편차 고정값, PROC_DETL_TYPE_CODE : 공정 상세 유형 코드, EQPT_GR_DETL_TYPE_CODE : 설비 그룹 상세 유형 코드
                Get_MMD_SEL_CMCD_TYPE_MULT("'IUSE','ROUT_TYPE_CODE', 'ROUT_TYPE_GR_CODE','ROUT_RSLT_GR_CODE', 'GRD_SLCTR_LOCATION_CODE', 'DFCT_GRD_FLAG','MEASR_TYPE_CODE', 'JUDG_MTHD_CODE','JUDG_TYPE_CODE','REF_VALUE_CODE', 'RJUDG_BAS_CODE', 'STDEV1_FIX_FLAG', 'PROC_DETL_TYPE_CODE', 'EQPT_GR_DETL_TYPE_CODE'",null,(dt, ex) =>
                    {
                        if (ex != null)
                        {
                            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = null;

                        _dvCMCD = dt.DefaultView;

                        _dvCMCD.RowFilter = "CMCDTYPE = 'IUSE' AND USE_FLAG = 'Y'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlag, CommonCombo.ComboStatus.ALL, "Y");
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlagTp301, CommonCombo.ComboStatus.ALL, "Y");
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlagTp302, CommonCombo.ComboStatus.ALL, "Y");
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlagTp401, CommonCombo.ComboStatus.ALL, "Y");
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlagTp402, CommonCombo.ComboStatus.ALL, "Y");
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlagTp502, CommonCombo.ComboStatus.ALL, "Y");
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cboUseFlagTp503, CommonCombo.ComboStatus.ALL, "Y");

                        _dvCMCD.RowFilter = "CMCDTYPE = 'IUSE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRoute.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgTestRoute.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRouteOper.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRecipe.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        //CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRecipe.Columns["SUBLOT_DFCT_LIMIT_USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgLoc.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgUjudg.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgCjudg.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgOp.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgMethod.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgGrade.Columns["USE_FLAG"], CommonCombo.ComboStatus.NONE);


                        _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_TYPE_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRoute.Columns["ROUT_TYPE_CODE"], CommonCombo.ComboStatus.EMPTY);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgTestRoute.Columns["ROUT_TYPE_CODE"], CommonCombo.ComboStatus.EMPTY);

                        _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_RSLT_GR_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRoute.Columns["ROUT_RSLT_GR_CODE"], CommonCombo.ComboStatus.EMPTY);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgTestRoute.Columns["ROUT_RSLT_GR_CODE"], CommonCombo.ComboStatus.EMPTY);

                        _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_TYPE_GR_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRoute.Columns["ROUT_GR_CODE"], CommonCombo.ComboStatus.EMPTY);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgTestRoute.Columns["ROUT_GR_CODE"], CommonCombo.ComboStatus.EMPTY);

                        _dvCMCD.RowFilter = "CMCDTYPE = 'GRD_SLCTR_LOCATION_CODE'";
                        cboColumn = this.dgLoc.Columns["GRD_SLCTR_LOCATION_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                        cboColumn.ItemsSource = DataTableConverter.Convert(_dvCMCD.ToTable());

                        _dvCMCD.RowFilter = "CMCDTYPE = 'DFCT_GRD_FLAG'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgLoc.Columns["DFCT_GRD_FLAG"], CommonCombo.ComboStatus.NONE);

                        _dvCMCD.RowFilter = "CMCDTYPE = 'EQPT_GR_DETL_TYPE_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRouteOper.Columns["EQPT_GR_DETL_TYPE_CODE"], CommonCombo.ComboStatus.EMPTY);

                        #region [판정등급관리 Tabpage]

                        #region 측정유형 코드
                        _dvCMCD.RowFilter = "CMCDTYPE = 'MEASR_TYPE_CODE' AND USE_FLAG = 'Y'";

                        tvDataType.Items.Clear();
                        DataRow[] level1 = _dvCMCD.ToTable().Select();

                        foreach (DataRow Row01 in level1)
                        {
                            C1TreeViewItem item01 = new C1TreeViewItem();
                            item01.Header = Row01["CMCDNAME"].SafeToString();
                            item01.DataContext = Row01;
                            item01.Click += tvDataType_Level1_Click;

                            item01.IsExpanded = true;
                            tvDataType.Items.Add(item01);
                        }

                        //측정 유형 코드
                        _dvCMCD.RowFilter = "CMCDTYPE = 'MEASR_TYPE_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgUjudg.Columns["MEASR_TYPE_CODE"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgCjudg.Columns["MEASR_TYPE_CODE"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgGrade.Columns["MEASR_TYPE_CODE"], CommonCombo.ComboStatus.NONE);

                        #endregion

                        //판정종류
                        _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_TYPE_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgCjudg.Columns["JUDG_TYPE_CODE"], CommonCombo.ComboStatus.NONE);

                        //상대판정 기준 코드
                        _dvCMCD.RowFilter = "CMCDTYPE = 'RJUDG_BAS_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgCjudg.Columns["RJUDG_BAS_CODE"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgMethod.Columns["RJUDG_BAS_CODE"], CommonCombo.ComboStatus.NONE);
                        #endregion

                        #region Tabpage05 : 상대판정 등급
                        _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_MTHD_CODE' AND USE_FLAG = 'Y'";

                        tvRJudgMethod.Items.Clear();
                        DataRow[] level2 = _dvCMCD.ToTable().Select();

                        foreach (DataRow Row01 in level2)
                        {
                            C1TreeViewItem item01 = new C1TreeViewItem();
                            item01.Header = Row01["CMCDNAME"].SafeToString();
                            item01.DataContext = Row01;
                            item01.Click += tvRJudgMethod_Level1_Click;

                            item01.IsExpanded = true;
                            tvRJudgMethod.Items.Add(item01);
                        }

                        //판정방법
                        _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_MTHD_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgMethod.Columns["JUDG_MTHD_CODE"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgGrade.Columns["JUDG_MTHD_CODE"], CommonCombo.ComboStatus.NONE);

                        //판정기준값
                        _dvCMCD.RowFilter = "CMCDTYPE = 'REF_VALUE_CODE'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgMethod.Columns["REF_VALUE_CODE"], CommonCombo.ComboStatus.NONE);

                        //표준편차 고정값 여부
                        _dvCMCD.RowFilter = "CMCDTYPE = 'STDEV1_FIX_FLAG'";
                        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), dgRJudgMethod.Columns["STDEV1_FIX_FLAG"], CommonCombo.ComboStatus.NONE);

                        #endregion

                    });
                #endregion

                #region Plant

                Get_MMD_SEL_SHOP_WITH_USER(null, null, "F", null, (dtShop, exShop) =>
                {
                    if (exShop != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(exShop), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _dvSHOP = dtShop.DefaultView;
                    CommonCombo.SetDtCommonCombo(_dvSHOP.ToTable(), dgRoute.Columns["SHOPID"], CommonCombo.ComboStatus.NONE);
                    CommonCombo.SetDtCommonCombo(_dvSHOP.ToTable(), dgTestRoute.Columns["SHOPID"], CommonCombo.ComboStatus.NONE);

                    _dvSHOP.RowFilter = "USE_FLAG = 'Y'";
                    CommonCombo.SetDtCommonCombo(_dvSHOP.ToTable(), cboPlant, CommonCombo.ComboStatus.NONE, null);
                    cboPlant.SelectedValue = LoginInfo.CFG_SHOP_ID.ToString();

                    #region Area
                    Get_MMD_SEL_AREA_WITH_USER(null, null, "F", null, (dtArea, exArea) =>
                    {
                        if (exArea != null)
                        {
                            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + exArea.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        _dvAREA = dtArea.DefaultView;
                        CommonCombo.SetDtCommonCombo(_dvAREA.ToTable(), dgRoute.Columns["AREAID"], CommonCombo.ComboStatus.NONE);
                        CommonCombo.SetDtCommonCombo(_dvAREA.ToTable(), dgTestRoute.Columns["AREAID"], CommonCombo.ComboStatus.NONE);

                        _dvAREA.RowFilter = GetArea_FilterFormat((string)cboPlant.SelectedValue, "Y");
                        CommonCombo.SetDtCommonCombo(_dvAREA.ToTable(), cboArea, CommonCombo.ComboStatus.NONE, null);
                        cboArea.SelectedValue = LoginInfo.CFG_AREA_ID.ToString();

                        #region EQUIPMENTSEGMENT
                        Get_MMD_SEL_EQSG_WITH_USER(null, null, null, "F", null, "F", null, (dtEQSG, exEQSG) =>
                        {
                            if (exEQSG != null)
                            {
                                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(exEQSG), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            _dvEQSG = dtEQSG.DefaultView;
                            CommonCombo.SetDtCommonCombo(_dvEQSG.ToTable(), dgRoute.Columns["EQSGID"], CommonCombo.ComboStatus.NONE);
                            CommonCombo.SetDtCommonCombo(_dvEQSG.ToTable(), dgTestRoute.Columns["EQSGID"], CommonCombo.ComboStatus.NONE);
                        });
                        #endregion

                        #region 활성화 모델
                        Get_FORM_MDL_CBO(null, null, null, null, (dtMDL, exMDL) =>
                        {
                            if (exMDL != null)
                            {
                                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(exMDL), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            _dvMODL = dtMDL.DefaultView;

                        });
                        #endregion

                        #region 동별 공통 코드
                        //LANE_FOR_FORM_ROUT_PROC : ROUTE 관리에 공정별 LANE 에 보여주는 LANE 관리
                        Get_MMD_SEL_AREA_CMCD_TYPE_MULT("'LANE_FOR_FORM_ROUT_PROC'", null, null, (dtAreaComcode, exAreaComcode) =>
                        {
                            if (exAreaComcode != null)
                            {
                                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + exAreaComcode.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            _dvCMCD_AREA = dtAreaComcode.DefaultView;

                        });
                        #endregion
                    });
                    #endregion
                });
                #endregion

                #region Process 
                Get_PROC_CBO(null, null, "'R1', 'W1', 'A1'", (dtProc, exProc) =>
                {
                    if (exProc != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + exProc.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _dvPROC = dtProc.DefaultView;
                    //DataTable dtProceqsgGrid = GetProceqsg_FilterFormat(_dvPROC, null, null, null, null);
                    //CommonCombo.SetDtCommonCombo(dtProceqsgGrid, dgRouteOper.Columns["PROCID"], CommonCombo.ComboStatus.NONE);
                });
                #endregion

                #region ContextMenu
                //string sObjectName = string.Empty;

                //sObjectName = ObjectDic.Instance.GetObjectName("UC_0008");
                //this.contextMenu_Add_Judge_Proc = new ContextMenu();
                //MenuItem menuAddJudge1 = new MenuItem() { Header = sObjectName, Tag = "ADD_JUDGE" };//판정공정에 추가(등급 복합 판정 설정)
                //menuAddJudge1.Click += new RoutedEventHandler(this.contextMenu_Add_Judge_Proc_Click);
                //this.contextMenu_Add_Judge_Proc.Items.Add(menuAddJudge1);

                //sObjectName = ObjectDic.Instance.GetObjectName("UC_0009");
                //this.contextMenu_Del_Judge_Proc = new ContextMenu();
                //MenuItem menuDelJudge1 = new MenuItem() { Header = sObjectName, Tag = "DEL_JUDGE" };//현재판정공정에서 제거(등급 복합 판정 설정)
                //menuDelJudge1.Click += new RoutedEventHandler(this.contextMenu_Del_Judge_Proc_Click);
                //this.contextMenu_Del_Judge_Proc.Items.Add(menuDelJudge1);

                //sObjectName = ObjectDic.Instance.GetObjectName("UC_0008");
                //this.contextMenu_Add_RJudge_Proc = new ContextMenu();
                //MenuItem menuAddRJudge1 = new MenuItem() { Header = sObjectName, Tag = "ADD_RJUDGE" }; //판정공정에 추가(등급 그룹 LOT 상대판정 설정)
                //menuAddRJudge1.Click += new RoutedEventHandler(this.contextMenu_Add_RJudge_Proc_Click);
                //this.contextMenu_Add_RJudge_Proc.Items.Add(menuAddRJudge1);

                //sObjectName = ObjectDic.Instance.GetObjectName("UC_0009");
                //this.contextMenu_Del_RJudge_Proc = new ContextMenu();
                //MenuItem menuDelRJudge1 = new MenuItem() { Header = sObjectName, Tag = "DEL_RJUDGE" };//현재판정공정에서 제거(등급 그룹 LOT 상대판정 설정)
                //menuDelRJudge1.Click += new RoutedEventHandler(this.contextMenu_Del_RJudge_Proc_Click);
                //this.contextMenu_Del_RJudge_Proc.Items.Add(menuDelRJudge1);
                #endregion

                Task<bool> task = WaitCallback();

                task.ContinueWith(_ => {

                    this.loadingIndicator.Visibility = Visibility.Hidden;
                    //this.LayoutRoot_SearchClick(null, null);
                }, TaskScheduler.FromCurrentSynchronizationContext());

                #endregion
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }

            //CommonCombo _combo = new CommonCombo();
            //_combo.SetCombo(cboPlant, CommonCombo.ComboStatus.SELECT, sCase: "SHOP");
            //C1ComboBox[] cboToChild = { cboLocFrom };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA");
            //SetComboBox(cboUseFlag);
        }

        async Task<bool> WaitCallback()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                if (
                    this.cboPlant.ItemsSource != null &&
                    this.cboArea.ItemsSource != null &&
                    this.cboUseFlag.ItemsSource != null &&
                    _dvEQSG != null &&
                    _dvMODL != null &&
                    _dvCMCD_AREA != null
                    )
                {
                    succeeded = true;

                    if (routeInfoArgument != null) SetRouteInfo(routeInfoArgument);
                }
                await Task.Delay(500);
            }
            return succeeded;
        }


        //private void InitControl()
        //{
        //}

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            //dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            //dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void SetComboBox(C1ComboBox cbo)
        {
            DataSet dsData = new DataSet();
            DataTable dtData = dsData.Tables.Add("ALL");
            dtData.Columns.Add("CBO_NAME", typeof(string));
            dtData.Columns.Add("CBO_CODE", typeof(string));

            DataRow drnewrow = dtData.NewRow();

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "-ALL-";
            drnewrow["CBO_CODE"] = DBNull.Value;
            dtData.Rows.Add(drnewrow);

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "Y : 사용";
            drnewrow["CBO_CODE"] = "Y";
            dtData.Rows.Add(drnewrow);

            drnewrow = dtData.NewRow();
            drnewrow["CBO_NAME"] = "N : 사용 안함";
            drnewrow["CBO_CODE"] = "N";
            dtData.Rows.Add(drnewrow);

            cbo.ItemsSource = DataTableConverter.Convert(dtData);
            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.SelectedIndex = 0;
        }


        #endregion


        #region LoadingIndicator
        private void ShowLoadingIndicator()
        {
            //if (loadingIndicator != null)
            //    loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            //if (loadingIndicator != null)
            //    loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                _sTp1SHOPID = cboPlant.SelectedValue.SafeToString();
                _sTp1AREAID = cboArea.SelectedValue.SafeToString();
                _sTp1AREANAME = cboArea.Text;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;

                if (!string.IsNullOrWhiteSpace(_sTp1SHOPID))
                {
                    dr["SHOPID"] = _sTp1SHOPID;
                }
                else
                {
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                }

                if (!string.IsNullOrWhiteSpace(_sTp1AREAID))
                {
                    dr["AREAID"] = _sTp1AREAID;
                }

                dr["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlag.SelectedValue.ToString()) ? DBNull.Value: cboUseFlag.SelectedValue;

                RQSTDT.Rows.Add(dr);
                string bizName = string.Empty;

                //오창 소형과 나머지 법인 분기 처리
                //if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
                //{
                //    bizName = "DA_PRD_SEL_QMS_INSP_LIST_CKO";
                //}
                //else
                //{
                bizName = "MMD_SEL_EQSG_MDL_TREE";
                //}

                //loadingIndicator.Visibility = Visibility.Collapsed;
                //return;

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        tvLineModelRoute.Items.Clear();
                        DataRow[] level1 = dt.Select("SORT_NO = 1");

                        foreach (DataRow Row01 in level1)
                        {
                            C1TreeViewItem item01 = new C1TreeViewItem();
                            item01.Header = Row01["NAME_VAL"].SafeToString();
                            item01.DataContext = Row01;
                            item01.Click += tvLineModelRoute_Level1_Click;

                            DataRow[] level2 = dt.Select("PKEY_VAL = '" + Row01["KEY_VAL"].SafeToString() + "' AND SORT_NO = 2");

                            foreach (DataRow Row02 in level2)
                            {
                                C1TreeViewItem item02 = new C1TreeViewItem();
                                item02.Header = Row02["NAME_VAL"].SafeToString();
                                item02.DataContext = Row02;
                                item02.Click += tvLineModelRoute_Level2_Click;

                                item01.Items.Add(item02);
                            }

                            item01.IsExpanded = true;
                            tvLineModelRoute.Items.Add(item01);
                        }

                    }
                    catch (Exception except)
                    {
                        Util.MessageException(except);

                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
              );              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }

            //_Com.Get_EQSG_MDL_TREE(_sTp1SHOPID, _sTp1AREAID, (dt, ex) =>
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;

            //    if (ex != null)
            //    {
            //        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        return;
            //    }


            //});
        }

        #endregion

        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            //if ((bool)chkAll.IsChecked)
            //{
            //    for (int i = 0; i < dgSearhResult.GetRowCount(); i++)
            //    {
            //        // 기존 저장자료는 제외
            //        if (Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
            //            DataTableConverter.SetValue(dgSearhResult.Rows[i].DataItem, "CHK", true);
            //    }
            //}
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            //if (!(bool)chkAll.IsChecked)
            //{
            //    for (int i = 0; i < dgSearhResult.GetRowCount(); i++)
            //    {
            //        DataTableConverter.SetValue(dgSearhResult.Rows[i].DataItem, "CHK", false);
            //    }
            //}
        }

        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            //    dgSearhResult.Selection.Clear();

            //    CheckBox cb = sender as CheckBox;

            //    if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            //    {
            //        DataTable dtTo = DataTableConverter.Convert(dgSearhResult.ItemsSource);

            //        if (dtTo.Columns.Count == 0)
            //        {
            //            dtTo.Columns.Add("CHK", typeof(Boolean));
            //        }

            //        DataRow dr = dtTo.NewRow();
            //        foreach (DataColumn dc in dtTo.Columns)
            //        {
            //            if (dc.DataType.Equals(typeof(Boolean)))
            //            {
            //                dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
            //            }
            //            else
            //            {
            //                dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
            //            }
            //        }

            //        dtTo.Rows.Add(dr);
            //        dgSearhResult.ItemsSource = DataTableConverter.Convert(dtTo);

            //        DataRow[] drUnchk = DataTableConverter.Convert(dgSearhResult.ItemsSource).Select("CHK = 0");

            //        if (drUnchk.Length == 0)
            //        {
            //            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
            //            chkAll.IsChecked = true;
            //            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
            //        }

            //    }
            //    else//체크 풀릴때
            //    {
            //        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
            //        chkAll.IsChecked = false;
            //        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

            //        DataTable dtTo = DataTableConverter.Convert(dgSearhResult.ItemsSource);

            //        dgSearhResult.ItemsSource = DataTableConverter.Convert(dtTo);
            //    }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            allControlClear();

            //if (_dvMODL != null)
            //{
            //    _dvMODL.RowFilter = null;
            //    _dvMODL.RowFilter = string.Format("SHOPID = '{0}' AND AREAID = '{1}' ", cboPlant.SelectedValue.SafeToString(), cboArea.SelectedValue.SafeToString());
            //    CommonCombo.SetDtCommonCombo(_dvMODL.ToTable(), dgRoute.Columns["MDLLOT_ID"], CommonCombo.ComboStatus.NONE);
            //    CommonCombo.SetDtCommonCombo(_dvMODL.ToTable(), dgTestRoute.Columns["MDLLOT_ID"], CommonCombo.ComboStatus.NONE);
            //}

            //if (_dvCMCD_AREA != null)
            //{
            //    string sAREAID = cboArea.SelectedValue.SafeToString();
            //    _dvCMCD_AREA.RowFilter = string.Format("CMCDTYPE = 'LANE_FOR_FORM_ROUT_PROC' AND AREAID = '{0}'", sAREAID);
            //    CommonCombo.SetDtCommonCombo(_dvCMCD_AREA.ToTable(), dgRouteOper.Columns["LANE_ID"], CommonCombo.ComboStatus.EMPTY);
            //}
        }

        private void tvLineModelRoute_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tvLineModelRoute_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvLineModelRoute_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void slDefRouteCol01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slDefRouteCol01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp101_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRoute);
                //CommonUtil.ExcelExport(dgRoute);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp101_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //int addRowCount = this.numAddCountTp101.Value.SafeToInt32();

        //        //for (int i = 0; i < addRowCount; i++)
        //        //{
        //        //    this.dgRoute.BeginNewRow();
        //        //    this.dgRoute.EndNewRow(true);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp101_Click(object sender, RoutedEventArgs e)
        //{
        //    //int subRowCount = this.numAddCountTp101.Value.SafeToInt32();
        //    //CommonUtil.MinusDataGridRow(dgRoute, subRowCount);
        //}

        //private void btnRouteSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (!Common.CommonVerify.HasDataGridRow(dgRoute)) return;
        //        //if (!Route_Validation()) return;

        //        //this.dgRoute.EndEdit();
        //        //this.dgRoute.EndEditRow(true);



        //        //const string bizRuleName = "BR_MMD_REG_FORM_ROUTE";
        //        //const string sBAS_ITEM_ID = "TB_MMD_FORM_ROUT";

        //        //DataTable inDataTable = new DataTable();
        //        //inDataTable.Columns.Add("REPSTR_ID", typeof(string));
        //        //inDataTable.Columns.Add("BAS_ITEM_ID", typeof(string));
        //        //inDataTable.Columns.Add("KEY_SEQ_NO", typeof(string));
        //        //inDataTable.Columns.Add("USE_FLAG", typeof(string));
        //        //inDataTable.Columns.Add("UPDUSER", typeof(string));
        //        //inDataTable.Columns.Add("DATASTATE", typeof(string));
        //        //inDataTable.Columns.Add("AREAID", typeof(string));
        //        //inDataTable.Columns.Add("ROUTID", typeof(string));
        //        //inDataTable.Columns.Add("ROUT_NAME", typeof(string));
        //        //inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
        //        //inDataTable.Columns.Add("ROUT_TYPE_CODE", typeof(string));
        //        //inDataTable.Columns.Add("EQSGID", typeof(string));
        //        //inDataTable.Columns.Add("ROUT_GR_CODE", typeof(string));
        //        //inDataTable.Columns.Add("RWK_AVAIL_GRD_CODE", typeof(string));
        //        //inDataTable.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
        //        //inDataTable.Columns.Add("MAVD_USE_FLAG", typeof(string));
        //        //inDataTable.Columns.Add("SLT_GRD", typeof(string));

        //        //foreach (object added in dgRoute.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = DataTableConverter.GetValue(added, "AREAID");
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["AREAID"] = DataTableConverter.GetValue(added, "AREAID");
        //        //        param["ROUTID"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["MDLLOT_ID"] = DataTableConverter.GetValue(added, "MDLLOT_ID");
        //        //        param["ROUT_TYPE_CODE"] = DataTableConverter.GetValue(added, "ROUT_TYPE_CODE");
        //        //        param["ROUT_NAME"] = DataTableConverter.GetValue(added, "ROUT_NAME");
        //        //        param["EQSGID"] = DataTableConverter.GetValue(added, "EQSGID");
        //        //        param["ROUT_GR_CODE"] = DataTableConverter.GetValue(added, "ROUT_GR_CODE");
        //        //        param["RWK_AVAIL_GRD_CODE"] = DataTableConverter.GetValue(added, "RWK_AVAIL_GRD_CODE");
        //        //        param["ROUT_RSLT_GR_CODE"] = DataTableConverter.GetValue(added, "ROUT_RSLT_GR_CODE");
        //        //        param["MAVD_USE_FLAG"] = DataTableConverter.GetValue(added, "MAVD_USE_FLAG");
        //        //        param["SLT_GRD"] = DataTableConverter.GetValue(added, "SLT_GRD");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //foreach (object modified in dgRoute.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = DataTableConverter.GetValue(modified, "AREAID");
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["AREAID"] = DataTableConverter.GetValue(modified, "AREAID");
        //        //        param["ROUTID"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["MDLLOT_ID"] = DataTableConverter.GetValue(modified, "MDLLOT_ID");
        //        //        param["ROUT_TYPE_CODE"] = DataTableConverter.GetValue(modified, "ROUT_TYPE_CODE");
        //        //        param["ROUT_NAME"] = DataTableConverter.GetValue(modified, "ROUT_NAME");
        //        //        param["EQSGID"] = DataTableConverter.GetValue(modified, "EQSGID");
        //        //        param["ROUT_GR_CODE"] = DataTableConverter.GetValue(modified, "ROUT_GR_CODE");
        //        //        param["RWK_AVAIL_GRD_CODE"] = DataTableConverter.GetValue(modified, "RWK_AVAIL_GRD_CODE");
        //        //        param["ROUT_RSLT_GR_CODE"] = DataTableConverter.GetValue(modified, "ROUT_RSLT_GR_CODE");
        //        //        param["MAVD_USE_FLAG"] = DataTableConverter.GetValue(modified, "MAVD_USE_FLAG");
        //        //        param["SLT_GRD"] = DataTableConverter.GetValue(modified, "SLT_GRD");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    //Helper.ShowInfo("10008");
        //        //    //return;
        //        //}

        //        loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //            SelectRoute(sDEFAULT_CHECK: "Y"); // Default Route
        //        //            btnbtnSearch_Click(null, null);


        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_FORM_ROUTE", ex);
        //    }
        //}

        //private void btnRouteCopy_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //FormRouteCopy wndRoutCopy = new FormRouteCopy();

        //        //if (wndRoutCopy != null)
        //        //{
        //        //    wndRoutCopy.FrameOperation = this.FrameOperation;

        //        //    object[] Parameters = new object[3];
        //        //    Parameters[0] = _sTp1SHOPID;
        //        //    Parameters[1] = _sTp1AREAID;
        //        //    Parameters[2] = _sTp1EQSGID;

        //        //    C1WindowExtension.SetParameters(wndRoutCopy, Parameters);

        //        //    //wndRoutCopy.Closed += new EventHandler(wndReworkGrade_Closed);

        //        //    // 팝업 화면 숨겨지는 문제 수정.
        //        //    this.Dispatcher.BeginInvoke(new Action(() => wndRoutCopy.ShowModal()));
        //        //    wndRoutCopy.BringToFront();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void dgRoute_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (//drv["CHK"].SafeToString() != R.TRUE && 
                e.Column != this.dgRoute.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgRoute.Columns["CHK"]
                 && e.Column != this.dgRoute.Columns["USE_FLAG"]
                 && e.Column != this.dgRoute.Columns["ROUT_TYPE_CODE"]
                 && e.Column != this.dgRoute.Columns["ROUT_NAME"]
                 && e.Column != this.dgRoute.Columns["ROUT_GR_CODE"]
                 && e.Column != this.dgRoute.Columns["RWK_AVAIL_GRD_CODE"]
                 && e.Column != this.dgRoute.Columns["ROUT_RSLT_GR_CODE"]
                 && e.Column != this.dgRoute.Columns["MAVD_USE_FLAG"])
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void dgRoute_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("SHOPID", _sTp1SHOPID);
            //e.Item.SetValue("AREAID", _sTp1AREAID);
            //e.Item.SetValue("EQSGID", _sTp1EQSGID);
            //e.Item.SetValue("MDLLOT_ID", _sTp1MDLLOT_ID);
            //e.Item.SetValue("MAVD_USE_FLAG", "N");

            //e.Item.SetValue("ROUTID", _sTp1AREAID);
        }

        private void dgRoute_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;

                //string sSelectedValue = drv[e.Column.Name].SafeToString();
                //string sSHOPID = (string)DataTableConverter.GetValue(e.Row.DataItem, "SHOPID");
                //string sAREAID = (string)DataTableConverter.GetValue(e.Row.DataItem, "AREAID");

                if (cbo != null)
                {
                    //if (Convert.ToString(e.Column.Name) == "SHOPID")
                    //{
                    //    _dvSHOP.RowFilter = "USE_FLAG = 'Y'";
                    //    CommonCombo.SetDtCommonCombo(_dvSHOP.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                    //}

                    //if (Convert.ToString(e.Column.Name) == "AREAID")
                    //{
                    //    _dvAREA.RowFilter = _Com.GetArea_FilterFormat(sSHOPID, "Y");
                    //    CommonCombo.SetDtCommonCombo(_dvAREA.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                    //}

                    //if (Convert.ToString(e.Column.Name) == "EQSGID")
                    //{
                    //    _dvEQSG.RowFilter = _Com.GetEqsg_FilterFormat(sSHOPID, sAREAID, "Y");
                    //    CommonCombo.SetDtCommonCombo(_dvEQSG.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                    //}

                    //if (Convert.ToString(e.Column.Name) == "MDLLOT_ID")
                    //{
                    //    _dvMODL.RowFilter = string.Format("SHOPID = '{0}' AND AREAID = '{1}' AND USE_FLAG = 'Y'", sSHOPID, sAREAID);
                    //    CommonCombo.SetDtCommonCombo(_dvMODL.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                    //}


                    //if (Convert.ToString(e.Column.Name) == "ROUT_TYPE_CODE")
                    //{
                    //    _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_TYPE_CODE' AND USE_FLAG = 'Y'";
                    //    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                    //}

                    //if (Convert.ToString(e.Column.Name) == "ROUT_RSLT_GR_CODE")
                    //{
                    //    _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_RSLT_GR_CODE' AND USE_FLAG = 'Y'";
                    //    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                    //}

                    //if (Convert.ToString(e.Column.Name) == "ROUT_GR_CODE")
                    //{
                    //    _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_TYPE_GR_CODE' AND USE_FLAG = 'Y'";
                    //    CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                    //}

                    //cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                    //{
                    //    Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        this.UpdateRowView_Route(e.Row, e.Column);
                    //    }));

                    //};
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgRoute_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "SHOPID", "AREAID", "EQSGID", "MDLLOT_ID", "ROUTID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRoute, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRoute, Col);
            //}
        }

        private void dgRoute_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRoute);
        }

        private void dgRoute_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            //if (_manualCommit_Route == false)
            //{
            //    this.UpdateRowView_Route(e.Row, e.Column);
            //}
        }

        private void dgRoute_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dgData = sender as C1DataGrid;

                if (!CommonUtil.HasDataGridRow(dgData) || dgData.SelectedItem == null) return;

                string sAREA = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "AREAID"));
                string sEQSG = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "EQSGID"));
                string sMODL = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "MDLLOT_ID"));
                string sROUT = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "ROUTID"));
                string sROUT_TYPE = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "ROUT_TYPE_CODE"));

                if (string.IsNullOrEmpty(sAREA) || string.IsNullOrEmpty(sEQSG) || string.IsNullOrEmpty(sMODL) || string.IsNullOrEmpty(sROUT) || string.IsNullOrEmpty(sROUT_TYPE)) return;

                string sDISP_AREA = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "DISP_AREA"));
                string sDISP_ROUT = Util.NVC(DataTableConverter.GetValue(dgData.SelectedItem, "DISP_ROUT"));

                Dictionary<string, string> routeInfo = new Dictionary<string, string>();
                routeInfo.Add("AREAID", sAREA);
                routeInfo.Add("EQSGID", sEQSG);
                routeInfo.Add("MDLLOT_ID", sMODL);
                routeInfo.Add("ROUTID", sROUT);
                routeInfo.Add("ROUT_TYPE_CODE", sROUT_TYPE);
                routeInfo.Add("DISP_AREA", sDISP_AREA);
                routeInfo.Add("DISP_ROUT", sDISP_ROUT);

                SetRouteInfo(routeInfo);

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 다른 화면에서 Route 정보 연결을 위해 더블클릭 부분을 메소드로 뺌.
        private void SetRouteInfo(Dictionary<string, string> routeInfo)
        {
            try
            {
                string sAREA = Util.NVC(routeInfo["AREAID"]);
                string sEQSG = Util.NVC(routeInfo["EQSGID"]);
                string sMODL = Util.NVC(routeInfo["MDLLOT_ID"]);
                string sROUT = Util.NVC(routeInfo["ROUTID"]);
                string sROUT_TYPE = Util.NVC(routeInfo["ROUT_TYPE_CODE"]);

                if (string.IsNullOrEmpty(sAREA) || string.IsNullOrEmpty(sEQSG) || string.IsNullOrEmpty(sMODL) || string.IsNullOrEmpty(sROUT) || string.IsNullOrEmpty(sROUT_TYPE)) return;

                string sDISP_AREA = Util.NVC(routeInfo["DISP_AREA"]);
                string sDISP_ROUT = Util.NVC(routeInfo["DISP_ROUT"]);

                string sDISP_EQSG = string.Empty;
                if (_dvEQSG != null)
                {
                    _dvEQSG.RowFilter = string.Format("CMCODE = '{0}'", sEQSG);
                    sDISP_EQSG = _dvEQSG[0]["CMCDNAME1"].SafeToString();
                }
                string sDISP_MODL = string.Empty;
                if (_dvEQSG != null)
                {
                    _dvMODL.RowFilter = string.Format("CMCODE = '{0}'", sMODL);
                    sDISP_MODL = _dvMODL[0]["CMCDNAME1"].SafeToString();
                }
                string sDISP_ROUT_TYPE = string.Empty;
                if (_dvEQSG != null)
                {
                    _dvCMCD.RowFilter = string.Format("CMCDTYPE = 'ROUT_TYPE_CODE' AND CMCODE = '{0}'", sROUT_TYPE);
                    sDISP_ROUT_TYPE = _dvCMCD[0]["CMCDNAME"].SafeToString();
                }

                //기본 경로 관리 탭
                Set_TextBox_Name(txtTp1AREAID, sDISP_AREA, sAREA);
                Set_TextBox_Name(txtTp1EQSGID, sDISP_EQSG, sEQSG);
                Set_TextBox_Name(txtTp1MDLLOT_ID, sDISP_MODL, sMODL);
                Set_TextBox_Name(txtTp1ROUTID, sDISP_ROUT, sROUT);

                //공정 경로 관리 탭
                Set_TextBox_Name(txtTp2AREAID, sDISP_AREA, sAREA);
                Set_TextBox_Name(txtTp2EQSGID, sDISP_EQSG, sEQSG);
                Set_TextBox_Name(txtTp2MDLLOT_ID, sDISP_MODL, sMODL);
                Set_TextBox_Name(txtTp2ROUTID, sDISP_ROUT, sROUT);
                Set_TextBox_Name(txtTp2ROUT_TYPE_CODE, sDISP_ROUT_TYPE, sROUT_TYPE);

                //작업 조건 관리 탭
                Set_TextBox_Name(txtTp3AREAID, sDISP_AREA, sAREA);
                Set_TextBox_Name(txtTp3EQSGID, sDISP_EQSG, sEQSG);
                Set_TextBox_Name(txtTp3MDLLOT_ID, sDISP_MODL, sMODL);
                Set_TextBox_Name(txtTp3ROUTID, sDISP_ROUT, sROUT);
                Set_TextBox_Name(txtTp3ROUT_TYPE_CODE, sDISP_ROUT_TYPE, sROUT_TYPE);

                //판정 등급 관리 탭
                Set_TextBox_Name(txtTp4AREAID, sDISP_AREA, sAREA);
                Set_TextBox_Name(txtTp4EQSGID, sDISP_EQSG, sEQSG);
                Set_TextBox_Name(txtTp4MDLLOT_ID, sDISP_MODL, sMODL);
                Set_TextBox_Name(txtTp4ROUTID, sDISP_ROUT, sROUT);
                Set_TextBox_Name(txtTp4ROUT_TYPE_CODE, sDISP_ROUT_TYPE, sROUT_TYPE);

                //상대 판정 등급 관리 탭
                Set_TextBox_Name(txtTp5AREAID, sDISP_AREA, sAREA);
                Set_TextBox_Name(txtTp5EQSGID, sDISP_EQSG, sEQSG);
                Set_TextBox_Name(txtTp5MDLLOT_ID, sDISP_MODL, sMODL);
                Set_TextBox_Name(txtTp5ROUTID, sDISP_ROUT, sROUT);
                Set_TextBox_Name(txtTp5ROUT_TYPE_CODE, sDISP_ROUT_TYPE, sROUT_TYPE);

                #region TAB #2 공정 경로 관리

                //TAP2 : 공정경로 Tree 조회
                SelectProcTree();

                //TAB2 : 공정경로 관리 조회
                SelectRouteProc();

                #endregion TAB #2 공정 경로 관리

                #region TAB #3 작업 조건 조회

                //TAB3 : Recipe Tree 조회
                selectRecipeTree();

                //TAB3 : 판정등급
                selectSublotGrade();

                //TAB3 : 선별공정(하단), TAB4 : 단위공정(상단)
                selectProcRout(sROUT);

                //TAB3 : 셀 선별 정보
                SelectGradSelecterLoadLocSet();

                #endregion TAB #3 작업 조건 조회

                #region TAB #4 판정 등급 관리

                //TAB4 : 판정 Tree 조회(하단)
                selectGradeMJudgTree();

                //TAB4 : 단위 공정(하단), TAB5 : 단위 공정(하단)
                selectProcUnit(sROUT);

                //TAB4 : 판정등급(시작등급, 종료등급)
                selectGradeCJudg();

                #endregion TAB #4 판정 등급 관리

                #region TAB #5 상대판정 등급 관리

                //TAB5 : 상대 판정 공정 정보 조회(Grid 상)
                selectRJudgProc();

                //TAB5 : 상대판정 등급(Tree)
                selectGradeRJudgTree();

                //TAB5 : 상대판정 등급(CASE ID)
                selectRJugeGradeCaseId();

                #endregion TAB #5 상대판정 등급 관리

                dgRecipe.ItemsSource = null;
                dgUjudg.ItemsSource = null;
                dgCjudg.ItemsSource = null;
                txtJUDG_OP.Text = null;
                txtJUDG_OP.Tag = null;
                txtJUDG_GRADE.Text = null;
                txtJUDG_GRADE.Text = null;
                dgRJudgMethod.ItemsSource = null;
                dgRJudgGrade.ItemsSource = null;

                TapRoute.SelectedItem = tpOpRoute;

                loadingIndicator2.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkHeaderAllTp101_Checked(object sender, RoutedEventArgs e)
        {
            //CommonUtil.DataGridCheckAllChecked(dgRoute);
        }

        private void chkHeaderAllTp101_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgRoute);
        }

        private void slDefRouteRow01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slDefRouteRow01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp102_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //new LGC.GMES.MMD.Common.ExcelExporter().Export(dgTestRoute);
                CommonUtil.ExcelExport(dgTestRoute);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp102_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //int addRowCount = this.numAddCountTp102.Value.SafeToInt32();

        //        //for (int i = 0; i < addRowCount; i++)
        //        //{
        //        //    this.dgTestRoute.BeginNewRow();
        //        //    this.dgTestRoute.EndNewRow(true);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp102_Click(object sender, RoutedEventArgs e)
        //{
        //    //int subRowCount = this.numAddCountTp102.Value.SafeToInt32();
        //    //CommonUtil.MinusDataGridRow(dgTestRoute, subRowCount);
        //}

        //private void btnTestRouteSave_Click(object sender, RoutedEventArgs e)
        //{
        //    //try
        //    //{
        //        //if (!Common.CommonVerify.HasDataGridRow(dgTestRoute)) return;
        //        //if (!TestRoute_Validation()) return;

        //    //    this.dgTestRoute.EndEdit();
        //    //    this.dgTestRoute.EndEditRow(true);

        //    //    const string bizRuleName = "BR_MMD_REG_FORM_ROUTE";
        //    //    const string sBAS_ITEM_ID = "TB_MMD_FORM_ROUT";

        //    //    DataTable inDataTable = new DataTable();
        //    //    inDataTable.Columns.Add("REPSTR_ID", typeof(string));
        //    //    inDataTable.Columns.Add("BAS_ITEM_ID", typeof(string));
        //    //    inDataTable.Columns.Add("KEY_SEQ_NO", typeof(string));
        //    //    inDataTable.Columns.Add("USE_FLAG", typeof(string));
        //    //    inDataTable.Columns.Add("UPDUSER", typeof(string));
        //    //    inDataTable.Columns.Add("DATASTATE", typeof(string));
        //    //    inDataTable.Columns.Add("AREAID", typeof(string));
        //    //    inDataTable.Columns.Add("ROUTID", typeof(string));
        //    //    inDataTable.Columns.Add("ROUT_NAME", typeof(string));
        //    //    inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
        //    //    inDataTable.Columns.Add("ROUT_TYPE_CODE", typeof(string));
        //    //    inDataTable.Columns.Add("EQSGID", typeof(string));
        //    //    inDataTable.Columns.Add("ROUT_GR_CODE", typeof(string));
        //    //    inDataTable.Columns.Add("RWK_AVAIL_GRD_CODE", typeof(string));
        //    //    inDataTable.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
        //    //    inDataTable.Columns.Add("MAVD_USE_FLAG", typeof(string));
        //    //    inDataTable.Columns.Add("SLT_GRD", typeof(string));

        //    //    foreach (object added in dgTestRoute.GetAddedItems())
        //    //    {
        //    //        if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //    //        {
        //    //            DataRow param = inDataTable.NewRow();
        //    //            param["REPSTR_ID"] = DataTableConverter.GetValue(added, "AREAID");
        //    //            param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //    //            param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //    //            param["UPDUSER"] = LoginInfo.USERID;
        //    //            param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //    //            param["AREAID"] = DataTableConverter.GetValue(added, "AREAID");
        //    //            param["ROUTID"] = DataTableConverter.GetValue(added, "ROUTID");
        //    //            param["MDLLOT_ID"] = DataTableConverter.GetValue(added, "MDLLOT_ID");
        //    //            param["ROUT_TYPE_CODE"] = DataTableConverter.GetValue(added, "ROUT_TYPE_CODE");
        //    //            param["ROUT_NAME"] = DataTableConverter.GetValue(added, "ROUT_NAME");
        //    //            param["EQSGID"] = DataTableConverter.GetValue(added, "EQSGID");
        //    //            param["ROUT_GR_CODE"] = DataTableConverter.GetValue(added, "ROUT_GR_CODE");
        //    //            param["RWK_AVAIL_GRD_CODE"] = DataTableConverter.GetValue(added, "RWK_AVAIL_GRD_CODE");
        //    //            param["ROUT_RSLT_GR_CODE"] = DataTableConverter.GetValue(added, "ROUT_RSLT_GR_CODE");
        //    //            param["MAVD_USE_FLAG"] = DataTableConverter.GetValue(added, "MAVD_USE_FLAG");
        //    //            param["SLT_GRD"] = DataTableConverter.GetValue(added, "SLT_GRD");
        //    //            inDataTable.Rows.Add(param);
        //    //        }
        //    //    }

        //    //    foreach (object modified in dgTestRoute.GetModifiedItems())
        //    //    {
        //    //        if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //    //        {
        //    //            DataRow param = inDataTable.NewRow();
        //    //            param["REPSTR_ID"] = DataTableConverter.GetValue(modified, "AREAID");
        //    //            param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //    //            param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //    //            param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //    //            param["UPDUSER"] = LoginInfo.USERID;
        //    //            param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //    //            param["AREAID"] = DataTableConverter.GetValue(modified, "AREAID");
        //    //            param["ROUTID"] = DataTableConverter.GetValue(modified, "ROUTID");
        //    //            param["MDLLOT_ID"] = DataTableConverter.GetValue(modified, "MDLLOT_ID");
        //    //            param["ROUT_TYPE_CODE"] = DataTableConverter.GetValue(modified, "ROUT_TYPE_CODE");
        //    //            param["ROUT_NAME"] = DataTableConverter.GetValue(modified, "ROUT_NAME");
        //    //            param["EQSGID"] = DataTableConverter.GetValue(modified, "EQSGID");
        //    //            param["ROUT_GR_CODE"] = DataTableConverter.GetValue(modified, "ROUT_GR_CODE");
        //    //            param["RWK_AVAIL_GRD_CODE"] = DataTableConverter.GetValue(modified, "RWK_AVAIL_GRD_CODE");
        //    //            param["ROUT_RSLT_GR_CODE"] = DataTableConverter.GetValue(modified, "ROUT_RSLT_GR_CODE");
        //    //            param["MAVD_USE_FLAG"] = DataTableConverter.GetValue(modified, "MAVD_USE_FLAG");
        //    //            param["SLT_GRD"] = DataTableConverter.GetValue(modified, "SLT_GRD");
        //    //            inDataTable.Rows.Add(param);
        //    //        }
        //    //    }

        //    //    if (inDataTable.Rows.Count < 1)
        //    //    {
        //    //        Helper.ShowInfo("10008");
        //    //        return;
        //    //    }

        //    //    loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //    //    new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //    //    {
        //    //        try
        //    //        {
        //    //            if (outException != null)
        //    //            {
        //    //                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    //                Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE", outException);
        //    //                return;
        //    //            }
        //    //            else
        //    //            {
        //    //                Helper.ShowInfo("10004");          //저장이 완료되었습니다.
        //    //                SelectRoute(sTEST_CHECK: "Y");     // Test Route
        //    //                btnbtnSearch_Click(null, null);
        //    //            }
        //    //        }
        //    //        catch (Exception ex)
        //    //        {
        //    //            loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //    //            ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE", ex);
        //    //        }
        //    //        finally
        //    //        {
        //    //            loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //    //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE", Logger.MESSAGE_OPERATION_END);
        //    //        }
        //    //    }, FrameOperation.MENUID);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    loadingIndicator.Visibility = Visibility.Collapsed;
        //    //    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    //    Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_FORM_ROUTE", ex);
        //    //}
        //}

        private void dgTestRoute_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgTestRoute.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgTestRoute.Columns["CHK"]
            //     && e.Column != this.dgTestRoute.Columns["USE_FLAG"]
            //     && e.Column != this.dgTestRoute.Columns["ROUT_TYPE_CODE"]
            //     && e.Column != this.dgTestRoute.Columns["ROUT_NAME"]
            //     && e.Column != this.dgTestRoute.Columns["ROUT_GR_CODE"]
            //     && e.Column != this.dgTestRoute.Columns["RWK_AVAIL_GRD_CODE"]
            //     && e.Column != this.dgTestRoute.Columns["ROUT_RSLT_GR_CODE"]
            //     && e.Column != this.dgTestRoute.Columns["MAVD_USE_FLAG"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}
        }

        private void dgTestRoute_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("SHOPID", (string)cboPlant.SelectedValue);
            //e.Item.SetValue("AREAID", (string)cboArea.SelectedValue);
            //e.Item.SetValue("EQSGID", _sTp1EQSGID);
            //e.Item.SetValue("MDLLOT_ID", _sTp1MDLLOT_ID);
            //e.Item.SetValue("MAVD_USE_FLAG", "N");

            //e.Item.SetValue("ROUTID", _sTp1AREAID);
        }

        private void dgTestRoute_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                //C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                //DataRowView drv = e.Row.DataItem as DataRowView;

                //string sSelectedValue = drv[e.Column.Name].SafeToString();
                //string sSHOPID = (string)DataTableConverter.GetValue(e.Row.DataItem, "SHOPID");
                //string sAREAID = (string)DataTableConverter.GetValue(e.Row.DataItem, "AREAID");

                //if (cbo != null)
                //{
                //    if (Convert.ToString(e.Column.Name) == "SHOPID")
                //    {
                //        _dvSHOP.RowFilter = "USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvSHOP.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "AREAID")
                //    {
                //        _dvAREA.RowFilter = _Com.GetArea_FilterFormat(sSHOPID, "Y");
                //        CommonCombo.SetDtCommonCombo(_dvAREA.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "EQSGID")
                //    {
                //        _dvEQSG.RowFilter = _Com.GetEqsg_FilterFormat(sSHOPID, sAREAID, "Y");
                //        CommonCombo.SetDtCommonCombo(_dvEQSG.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "MDLLOT_ID")
                //    {
                //        _dvMODL.RowFilter = string.Format("SHOPID = '{0}' AND AREAID = '{1}' AND USE_FLAG = 'Y'", sSHOPID, sAREAID);
                //        CommonCombo.SetDtCommonCombo(_dvMODL.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "ROUT_TYPE_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_TYPE_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "ROUT_RSLT_GR_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_RSLT_GR_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "ROUT_GR_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_TYPE_GR_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                //    }

                //    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                //    {
                //        Dispatcher.BeginInvoke(new Action(() =>
                //        {
                //            this.UpdateRowView_TestRoute(e.Row, e.Column);
                //        }));

                //    };
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgTestRoute_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "SHOPID", "AREAID", "EQSGID", "MDLLOT_ID", "ROUTID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRoute, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRoute, Col);
            //}
        }

        private void dgTestRoute_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgTestRoute);
        }

        private bool _manualCommit_TestRoute = false;
        private void dgTestRoute_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_manualCommit_TestRoute == false)
            {
                this.UpdateRowView_TestRoute(e.Row, e.Column);
            }
        }

        void UpdateRowView_TestRoute(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "SHOPID")
                {
                    _manualCommit_TestRoute = true;
                    drv["AREAID"] = string.Empty;
                    drv["EQSGID"] = string.Empty;
                    drv["MDLLOT_ID"] = string.Empty;

                    this.dgTestRoute.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "AREAID")
                {
                    _manualCommit_TestRoute = true;
                    drv["EQSGID"] = string.Empty;
                    drv["MDLLOT_ID"] = string.Empty;
                    this.dgTestRoute.EndEditRow(true);
                }
            }
            finally
            {
                _manualCommit_TestRoute = false;
            }
        }

        private void chkHeaderAllTp102_Checked(object sender, RoutedEventArgs e)
        {
            //CommonUtil.DataGridCheckAllChecked(dgTestRoute);
        }

        private void chkHeaderAllTp102_Unchecked(object sender, RoutedEventArgs e)
        {
            //CommonUtil.DataGridCheckAllUnChecked(dgTestRoute);
        }
        

        //기본경로관리 Tree Click
        private void tvLineModelRoute_Level1_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;
                DataRow data = tvi.DataContext as DataRow;

                _sTp1EQSGID = null;
                _sTp1MDLLOT_ID = null;

                _sTp1EQSGID = data["KEY_VAL"].SafeToString();

                SelectRoute(sDEFAULT_CHECK: "Y"); // Default Route
                SelectRoute(sTEST_CHECK: "Y");     // Test Route
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void tvLineModelRoute_Level2_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;

                DataRow parent = tvi.ParentItem.DataContext as DataRow;
                DataRow data = tvi.DataContext as DataRow;

                _sTp1EQSGID = null;
                _sTp1MDLLOT_ID = null;

                _sTp1EQSGID = parent["KEY_VAL"].SafeToString();
                _sTp1MDLLOT_ID = data["KEY_VAL"].SafeToString();

                SelectRoute(sDEFAULT_CHECK: "Y");  // Default Route
                SelectRoute(sTEST_CHECK: "Y");     // Test Route
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void cboPlant_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
           allControlClear();

            if (_dvAREA != null)
            {
                _dvAREA.RowFilter = GetArea_FilterFormat((string)cboPlant.SelectedValue, "Y");
                CommonCombo.SetDtCommonCombo(_dvAREA.ToTable(), cboArea, CommonCombo.ComboStatus.NONE, null);
            }
        }

        private void allControlClear()
        {
            //tab1 기본 경로 조회
            tvLineModelRoute.Items.Clear();
            Util.gridClear(dgRoute);
            Util.gridClear(dgTestRoute);

            Set_TextBox_Name(txtTp1AREAID, "", "");
            Set_TextBox_Name(txtTp1EQSGID, "", "");
            Set_TextBox_Name(txtTp1MDLLOT_ID, "", "");
            Set_TextBox_Name(txtTp1ROUTID, "", "");

            //tab2 공정 경로 조회
            tvOp.Items.Clear();
            Util.gridClear(dgRouteOper);

            Set_TextBox_Name(txtTp2AREAID, "", "");
            Set_TextBox_Name(txtTp2EQSGID, "", "");
            Set_TextBox_Name(txtTp2MDLLOT_ID, "", "");
            Set_TextBox_Name(txtTp2ROUTID, "", "");
            Set_TextBox_Name(txtTp2ROUT_TYPE_CODE, "", "");

            //tab3 작업 조건 조회
            tvRecipe.Items.Clear();
            Util.gridClear(dgRecipe);
            Util.gridClear(dgLoc);

            Set_TextBox_Name(txtTp3AREAID, "", "");
            Set_TextBox_Name(txtTp3EQSGID, "", "");
            Set_TextBox_Name(txtTp3MDLLOT_ID, "", "");
            Set_TextBox_Name(txtTp3ROUTID, "", "");
            Set_TextBox_Name(txtTp3ROUT_TYPE_CODE, "", "");

            //tab4 판정 등급 조회
            //tvDataType.Items.Clear();
            Util.gridClear(dgUjudg);
            tvGrade.Items.Clear();
            Util.gridClear(dgCjudg);

            Set_TextBox_Name(txtTp4AREAID, "", "");
            Set_TextBox_Name(txtTp4EQSGID, "", "");
            Set_TextBox_Name(txtTp4MDLLOT_ID, "", "");
            Set_TextBox_Name(txtTp4ROUTID, "", "");
            Set_TextBox_Name(txtTp4ROUT_TYPE_CODE, "", "");

            //tab5 상대 판정 등급 조회
            Util.gridClear(dgRJudgOp);
            //tvRJudgMethod.Items.Clear();
            Util.gridClear(dgRJudgMethod);
            tvRJudgGrade.Items.Clear();
            Util.gridClear(dgRJudgGrade);

            Set_TextBox_Name(txtTp5AREAID, "", "");
            Set_TextBox_Name(txtTp5EQSGID, "", "");
            Set_TextBox_Name(txtTp5MDLLOT_ID, "", "");
            Set_TextBox_Name(txtTp5ROUTID, "", "");
            Set_TextBox_Name(txtTp5ROUT_TYPE_CODE, "", "");
        }

        private void tvOp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tvOp_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvOp_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void tvOp_Drop(object sender, DragEventArgs e)
        {

        }

        private void tvOp_DragDrop(object source, DragDropEventArgs e)
        {

        }

        private void slOpCol01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slOpCol01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp201_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRouteOper);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        bool _bMouseMove = false;
        string _sRouteOp_ProcId = null;
        string _sRouteOp_ProcName = null;
        string _sRouteOp_GroupCode = null;
        string _sRouteOp_DetlType = null;
        private void tvOp_DragOver(object source, DragDropEventArgs e)
        {
            //C1TreeViewItem tvi = (C1TreeViewItem)e.DragSource;
            //DataRow data = tvi.DataContext as DataRow;

            //_sRouteOp_ProcId = data["KEY_VAL"].SafeToString();
            //_sRouteOp_ProcName = data["NAME_VAL"].SafeToString();
            //_sRouteOp_GroupCode = data["PROC_GR_CODE"].SafeToString();
            //_sRouteOp_DetlType = data["PROC_DETL_TYPE_CODE"].SafeToString();

            //_bMouseMove = true;
        }

        private void dgRouteOper_MouseMove(object sender, MouseEventArgs e)
        {
            //try
            //{
            //    if (_bMouseMove)
            //    {
            //        if (_sRouteOp_DetlType == "J0" || _sRouteOp_DetlType == "J9")
            //        {
            //            _bMouseMove = false;
            //            return;
            //        }

            //        int iSelIndx = dgRouteOper.SelectedIndex;

            //        if (iSelIndx < 0)
            //        {
            //            dgRouteOper.SelectedIndex = dgRouteOper.Rows.Count;
            //            iSelIndx = dgRouteOper.SelectedIndex;
            //        }

            //        DataTable dtRouterOper = DataTableConverter.Convert(dgRouteOper.ItemsSource);
            //        DataRow newRow = null;

            //        //int iJigProcChk = dtRouterOper.Select(string.Format("PROC_GR_CODE = '{0}' OR PROC_DETL_TYPE_CODE = '{1}'", _sRouteOp_GroupCode, _sRouteOp_DetlType)).Count();
            //        int iJigGroupCodeChk = dtRouterOper.Select(string.Format("PROC_GR_CODE = '{0}' ", "J")).Count();
            //        int iJigDetlTypeChk = dtRouterOper.Select(string.Format(" PROC_DETL_TYPE_CODE = '{0}'", "BJ")).Count();

            //        DataRow[] drJigStart = dtRouterOper.Select("PROCID = 'FFJ001'");
            //        DataRow[] drJigEnd = dtRouterOper.Select("PROCID = 'FFJ901'");

            //        int iJigStart = 0;
            //        foreach (DataRow dr in drJigStart)
            //        {
            //            iJigStart = dr["PROC_SEQNO"].SafeToInt32();
            //        }

            //        int iJigEnd = 0;
            //        foreach (DataRow dr in drJigEnd)
            //        {
            //            iJigEnd = dr["PROC_SEQNO"].SafeToInt32();
            //        }

            //        if ((_sRouteOp_GroupCode.Equals("J") || _sRouteOp_DetlType.Equals("BJ")) && (iJigGroupCodeChk == 0 && iJigDetlTypeChk == 0))
            //        {
            //            newRow = dtRouterOper.NewRow();
            //            newRow["ROUTID"] = txtTp2ROUTID.Tag;
            //            newRow["PROCID"] = "FFJ001";
            //            newRow["LANE_ID"] = null;
            //            newRow["SAS_TRNF_FLAG"] = "N";
            //            newRow["USE_FLAG"] = "Y";
            //            newRow["PROC_GR_CODE"] = "J";
            //            newRow["PROC_DETL_TYPE_CODE"] = "J0";
            //            newRow["INSUSER"] = LoginInfo.USERNAME;
            //            newRow["INSDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            newRow["UPDUSER"] = LoginInfo.USERNAME;
            //            newRow["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            dtRouterOper.Rows.InsertAt(newRow, iSelIndx + 1);

            //            newRow = dtRouterOper.NewRow();
            //            newRow["ROUTID"] = txtTp2ROUTID.Tag;
            //            newRow["PROCID"] = _sRouteOp_ProcId;
            //            newRow["LANE_ID"] = null;
            //            newRow["SAS_TRNF_FLAG"] = "N";
            //            newRow["USE_FLAG"] = "Y";
            //            newRow["PROC_GR_CODE"] = _sRouteOp_GroupCode;
            //            newRow["PROC_DETL_TYPE_CODE"] = _sRouteOp_DetlType;
            //            newRow["INSUSER"] = LoginInfo.USERNAME;
            //            newRow["INSDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            newRow["UPDUSER"] = LoginInfo.USERNAME;
            //            newRow["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            dtRouterOper.Rows.InsertAt(newRow, iSelIndx + 2);

            //            newRow = dtRouterOper.NewRow();
            //            newRow["ROUTID"] = txtTp2ROUTID.Tag;
            //            newRow["PROCID"] = "FFJ901";
            //            newRow["LANE_ID"] = null;
            //            newRow["SAS_TRNF_FLAG"] = "N";
            //            newRow["USE_FLAG"] = "Y";
            //            newRow["PROC_GR_CODE"] = "J";
            //            newRow["PROC_DETL_TYPE_CODE"] = "J9";
            //            newRow["INSUSER"] = LoginInfo.USERNAME;
            //            newRow["INSDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            newRow["UPDUSER"] = LoginInfo.USERNAME;
            //            newRow["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            dtRouterOper.Rows.InsertAt(newRow, iSelIndx + 3);

            //        }
            //        else if ((_sRouteOp_GroupCode == "J" || _sRouteOp_DetlType == "BJ") && (iJigStart > iSelIndx || iJigEnd <= iSelIndx))
            //        {
            //            //Helper.ShowInfo("FM_ME_0042");  //Jig관련 공정은 Jig Start와 Jig End사이에만 등록할 수 있습니다.
            //            _bMouseMove = false;
            //            return;
            //        }
            //        else if ((_sRouteOp_GroupCode != "J" && _sRouteOp_DetlType != "BJ") && (iJigStart <= iSelIndx && iJigEnd > iSelIndx))
            //        {
            //            //Helper.ShowInfo("FM_ME_0043");  //Jig와 관련되지 않은 공정은 Jig Start와 Jig End사이에 등록할 수 없습니다.
            //            _bMouseMove = false;
            //            return;
            //        }
            //        else
            //        {
            //            newRow = dtRouterOper.NewRow();
            //            newRow["ROUTID"] = txtTp2ROUTID.Tag;
            //            newRow["PROCID"] = _sRouteOp_ProcId;
            //            newRow["LANE_ID"] = null;
            //            newRow["SAS_TRNF_FLAG"] = "N";
            //            newRow["USE_FLAG"] = "Y";
            //            newRow["PROC_GR_CODE"] = _sRouteOp_GroupCode;
            //            newRow["PROC_DETL_TYPE_CODE"] = _sRouteOp_DetlType;
            //            newRow["INSUSER"] = LoginInfo.USERNAME;
            //            newRow["INSDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            newRow["UPDUSER"] = LoginInfo.USERNAME;
            //            newRow["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //            dtRouterOper.Rows.InsertAt(newRow, iSelIndx + 1);
            //        }

            //        int iProcSeq = 1;
            //        for (int i = 0; i < dtRouterOper.Rows.Count; i++)
            //        {
            //            if (_sRouteOp_DetlType == dtRouterOper.Rows[i]["PROC_DETL_TYPE_CODE"].SafeToString())
            //            {
            //                if (!(dtRouterOper.Rows[i]["PROC_DETL_TYPE_CODE"].Equals("J0") || dtRouterOper.Rows[i]["PROC_DETL_TYPE_CODE"].Equals("J9")))
            //                {
            //                    dtRouterOper.Rows[i]["PROCID"] = string.Format("{0}{1}", _sRouteOp_ProcId, iProcSeq.ToString("D2"));
            //                }

            //                int iProcChk = _dvPROC.ToTable().Select("CMCODE = '" + dtRouterOper.Rows[i]["PROCID"] + "'").Count();

            //                if (iProcChk < 1)
            //                {
            //                    //Helper.ShowInfo("SFU1456");  //공정 정보가 없습니다.
            //                    dtRouterOper.Rows.RemoveAt(i);
            //                    break;
            //                }

            //                iProcSeq++;
            //            }

            //            dtRouterOper.Rows[i]["PROC_SEQNO"] = i;
            //            dtRouterOper.Rows[i]["STRT_PROC_FLAG"] = "N";
            //            dtRouterOper.Rows[i]["END_PROC_FLAG"] = "N";
            //        }

            //        dtRouterOper.Rows[0]["STRT_PROC_FLAG"] = "Y";
            //        dtRouterOper.Rows[dtRouterOper.Rows.Count - 1]["END_PROC_FLAG"] = "Y";
            //        dtRouterOper.AcceptChanges();

            //        dgRouteOper.ItemsSource = DataTableConverter.Convert(dtRouterOper);
            //        dgRouteOper.SelectedIndex = dgRouteOper.Rows.Count - 2;

            //        this.dgRouteOper.EndEdit();
            //        this.dgRouteOper.EndEditRow(true);

            //        _bMouseMove = false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        //private void btnMinusTp201_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //int iSelIndx = dgRouteOper.SelectedIndex;

        //        //if (iSelIndx < 0)
        //        //{
        //        //    return;
        //        //}

        //        //DataTable dtRouterOper = DataTableConverter.Convert(dgRouteOper.ItemsSource);
        //        //DataTable dtResult = dtRouterOper.Clone();

        //        //string sPROC_DETL_TYPE_CODE = dtRouterOper.Rows[iSelIndx]["PROC_DETL_TYPE_CODE"].SafeToString();
        //        //dtRouterOper.Rows.RemoveAt(iSelIndx);

        //        //int iProcSeq = 1;
        //        //for (int i = 0; i < dtRouterOper.Rows.Count; i++)
        //        //{
        //        //    DataRow newRow = dtResult.NewRow();
        //        //    newRow["ROUTID"] = dtRouterOper.Rows[i]["ROUTID"].SafeToString();
        //        //    newRow["PROCID"] = dtRouterOper.Rows[i]["PROCID"].SafeToString();
        //        //    if (sPROC_DETL_TYPE_CODE == dtRouterOper.Rows[i]["PROC_DETL_TYPE_CODE"].SafeToString())
        //        //    {
        //        //        newRow["PROCID"] = string.Format("FF{0}{1}", sPROC_DETL_TYPE_CODE, iProcSeq.ToString("D2"));

        //        //        iProcSeq++;
        //        //    }
        //        //    newRow["PROC_SEQNO"] = i;
        //        //    newRow["LANE_ID"] = dtRouterOper.Rows[i]["LANE_ID"].SafeToString();
        //        //    newRow["SAS_TRNF_FLAG"] = dtRouterOper.Rows[i]["SAS_TRNF_FLAG"].SafeToString();
        //        //    newRow["EQPT_GR_DETL_TYPE_CODE"] = dtRouterOper.Rows[i]["EQPT_GR_DETL_TYPE_CODE"].SafeToString();


        //        //    newRow["USE_FLAG"] = dtRouterOper.Rows[i]["USE_FLAG"].SafeToString();
        //        //    newRow["PROC_GR_CODE"] = dtRouterOper.Rows[i]["PROC_GR_CODE"].SafeToString();
        //        //    newRow["PROC_DETL_TYPE_CODE"] = dtRouterOper.Rows[i]["PROC_DETL_TYPE_CODE"].SafeToString();

        //        //    newRow["INSUSER"] = dtRouterOper.Rows[i]["INSUSER"].SafeToString();
        //        //    newRow["INSDTTM"] = dtRouterOper.Rows[i]["INSDTTM"].SafeToString();
        //        //    newRow["UPDUSER"] = dtRouterOper.Rows[i]["UPDUSER"].SafeToString();
        //        //    newRow["UPDDTTM"] = dtRouterOper.Rows[i]["UPDDTTM"].SafeToString();

        //        //    newRow["STRT_PROC_FLAG"] = "N";
        //        //    newRow["END_PROC_FLAG"] = "N";

        //        //    dtResult.Rows.Add(newRow);
        //        //}

        //        //if (dtResult.Rows.Count > 0)
        //        //{
        //        //    dtResult.Rows[0]["STRT_PROC_FLAG"] = "Y";
        //        //    dtResult.Rows[dtRouterOper.Rows.Count - 1]["END_PROC_FLAG"] = "Y";
        //        //}

        //        //dtResult.AcceptChanges();
        //        //dgRouteOper.ItemsSource = DataTableConverter.Convert(dtResult);
        //        //dgRouteOper.SelectedIndex = dgRouteOper.Rows.Count - 2;

        //        //this.dgRouteOper.EndEdit();
        //        //this.dgRouteOper.EndEditRow(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void dgRouteOper_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if( MouseButton.Right)
            //{

            //dgRouteOper.ContextMenu.Items.Add("FormRoutProc");

            //}
        }

        private void dgRouteOper_DragOver(object sender, DragEventArgs e)
        {

        }

        private void dgRouteOper_Drop(object sender, DragEventArgs e)
        {

        }

        //private void btnRefresh_Click(object sender, RoutedEventArgs e)
        //{
        //    //공정경로 Tree 조회
        //    SelectProcTree();

        //    //공정경로 관리 조회
        //    SelectRouteProc();
        //}

        //private void btnDeltaOCV_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //FormRouteProcDeltaOCV wndDeltaOCVList = new FormRouteProcDeltaOCV();

        //        //if (wndDeltaOCVList != null)
        //        //{
        //        //    wndDeltaOCVList.FrameOperation = this.FrameOperation;

        //        //    object[] Parameters = new object[4];

        //        //    Parameters[0] = Util.NVC(txtTp2AREAID.Tag);
        //        //    Parameters[1] = Util.NVC(txtTp2AREAID.Text);

        //        //    Parameters[2] = Util.NVC(txtTp2ROUTID.Tag);
        //        //    Parameters[3] = Util.NVC(txtTp2ROUTID.Text);

        //        //    C1WindowExtension.SetParameters(wndDeltaOCVList, Parameters);

        //        //    // 팝업 화면 숨겨지는 문제 수정.
        //        //    this.Dispatcher.BeginInvoke(new Action(() => wndDeltaOCVList.ShowModal()));
        //        //    wndDeltaOCVList.BringToFront();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FORM001_ROUTE_MMD_FormRouteProcRecipe wndProcRecipeList = new FORM001_ROUTE_MMD_FormRouteProcRecipe();

                if (wndProcRecipeList != null)
                {
                    wndProcRecipeList.FrameOperation = this.FrameOperation;

                    object[] Parameters = new object[10];

                    Parameters[0] = Util.NVC(txtTp2AREAID.Tag);
                    Parameters[1] = Util.NVC(txtTp2AREAID.Text);

                    Parameters[2] = Util.NVC(txtTp2EQSGID.Tag);
                    Parameters[3] = Util.NVC(txtTp2EQSGID.Text);

                    Parameters[4] = Util.NVC(txtTp2MDLLOT_ID.Tag);
                    Parameters[5] = Util.NVC(txtTp2MDLLOT_ID.Text);

                    Parameters[6] = Util.NVC(txtTp2ROUTID.Tag);
                    Parameters[7] = Util.NVC(txtTp2ROUTID.Text);

                    Parameters[8] = Util.NVC(txtTp2ROUT_TYPE_CODE.Tag);
                    Parameters[9] = Util.NVC(txtTp2ROUT_TYPE_CODE.Text);

                    C1WindowExtension.SetParameters(wndProcRecipeList, Parameters);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndProcRecipeList.ShowModal()));
                    wndProcRecipeList.BringToFront();
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnSaveRouteOP_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {

        //        //if (!Common.CommonVerify.HasDataGridRow(dgRouteOper)) return;
        //        //if (!RouteOper_Validation()) return;

        //        //Helper.ShowConfirm("9085", (MessageBoxResult mbr) =>
        //        //{
        //        //    if (mbr == MessageBoxResult.OK)
        //        //    {
        //        //        this.dgRouteOper.EndEdit();
        //        //        this.dgRouteOper.EndEditRow(true);

        //        //        const string bizRuleName = "BR_MMD_REG_FORM_ROUTE_PROC";
        //        //        const string sBAS_ITEM_ID = "TB_MMD_FORM_ROUT_PROC";

        //        //        string sREPSTR_ID = txtTp2AREAID.Tag.SafeToString();

        //        //        DataTable inDataTable = new DataTable();
        //        //        inDataTable.Columns.Add("REPSTR_ID", typeof(string));
        //        //        inDataTable.Columns.Add("BAS_ITEM_ID", typeof(string));
        //        //        inDataTable.Columns.Add("KEY_SEQ_NO", typeof(string));
        //        //        inDataTable.Columns.Add("USE_FLAG", typeof(string));
        //        //        inDataTable.Columns.Add("UPDUSER", typeof(string));
        //        //        inDataTable.Columns.Add("DATASTATE", typeof(string));

        //        //        inDataTable.Columns.Add("AREAID", typeof(string));
        //        //        inDataTable.Columns.Add("ROUTID", typeof(string));
        //        //        inDataTable.Columns.Add("PROCID", typeof(string));
        //        //        inDataTable.Columns.Add("PROC_SEQNO", typeof(string));
        //        //        inDataTable.Columns.Add("LANE_ID", typeof(string));
        //        //        inDataTable.Columns.Add("SAS_TRNF_FLAG", typeof(string));
        //        //        inDataTable.Columns.Add("STRT_PROC_FLAG", typeof(string));
        //        //        inDataTable.Columns.Add("END_PROC_FLAG", typeof(string));
        //        //        inDataTable.Columns.Add("EQPT_GR_DETL_TYPE_CODE", typeof(string));

        //        //        DataTable dtRouteProc = DataTableConverter.Convert(dgRouteOper.ItemsSource);

        //        //        for (int iCnt = 0; iCnt < dtRouteProc.Rows.Count; iCnt++)
        //        //        {
        //        //            DataRow param = inDataTable.NewRow();
        //        //            param["REPSTR_ID"] = sREPSTR_ID;
        //        //            param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //            //param["KEY_SEQ_NO"] = null;
        //        //            param["USE_FLAG"] = "Y";
        //        //            param["UPDUSER"] = LoginInfo.USERID;
        //        //            //param["DATASTATE"] = null;

        //        //            param["AREAID"] = txtTp2AREAID.Tag.SafeToString();
        //        //            param["ROUTID"] = dtRouteProc.Rows[iCnt]["ROUTID"];
        //        //            param["PROCID"] = dtRouteProc.Rows[iCnt]["PROCID"];
        //        //            param["PROC_SEQNO"] = dtRouteProc.Rows[iCnt]["PROC_SEQNO"];
        //        //            param["LANE_ID"] = dtRouteProc.Rows[iCnt]["LANE_ID"];
        //        //            param["SAS_TRNF_FLAG"] = string.IsNullOrEmpty(dtRouteProc.Rows[iCnt]["SAS_TRNF_FLAG"].SafeToString()) == true ? "N" : dtRouteProc.Rows[iCnt]["SAS_TRNF_FLAG"].SafeToString();
        //        //            param["STRT_PROC_FLAG"] = dtRouteProc.Rows[iCnt]["STRT_PROC_FLAG"];
        //        //            param["END_PROC_FLAG"] = dtRouteProc.Rows[iCnt]["END_PROC_FLAG"];
        //        //            param["EQPT_GR_DETL_TYPE_CODE"] = dtRouteProc.Rows[iCnt]["EQPT_GR_DETL_TYPE_CODE"];
        //        //            inDataTable.Rows.Add(param);
        //        //        }


        //        //        //목록에서 제외된 공정 ROUTE
        //        //        string sROUT = string.Empty;
        //        //        string sPROC = string.Empty;
        //        //        int iFlagCnt = 0;

        //        //        for (int iNCnt = 0; iNCnt < _dtRoutOper.Rows.Count; iNCnt++)
        //        //        {
        //        //            sROUT = _dtRoutOper.Rows[iNCnt]["ROUTID"].SafeToString();
        //        //            sPROC = _dtRoutOper.Rows[iNCnt]["PROCID"].SafeToString();

        //        //            iFlagCnt = inDataTable.Select(string.Format("ROUTID = '{0}' AND PROCID = '{1}'", sROUT, sPROC)).Count();

        //        //            if (iFlagCnt == 0)
        //        //            {
        //        //                DataRow param = inDataTable.NewRow();
        //        //                param["REPSTR_ID"] = sREPSTR_ID;
        //        //                param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //                //param["KEY_SEQ_NO"] = null;
        //        //                param["USE_FLAG"] = "N";
        //        //                param["UPDUSER"] = LoginInfo.USERID;
        //        //                //param["DATASTATE"] = null;

        //        //                param["AREAID"] = txtTp2AREAID.Tag.SafeToString();
        //        //                param["ROUTID"] = _dtRoutOper.Rows[iNCnt]["ROUTID"];
        //        //                param["PROCID"] = _dtRoutOper.Rows[iNCnt]["PROCID"];
        //        //                param["PROC_SEQNO"] = "0";// _dtRoutOper.Rows[iNCnt]["PROC_SEQNO"];
        //        //                param["LANE_ID"] = _dtRoutOper.Rows[iNCnt]["LANE_ID"];
        //        //                param["SAS_TRNF_FLAG"] = "N";   // _dtRoutOper.Rows[iNCnt]["SAS_TRNF_FLAG"];
        //        //                param["STRT_PROC_FLAG"] = "N";   // _dtRoutOper.Rows[iNCnt]["STRT_PROC_FLAG"];
        //        //                param["END_PROC_FLAG"] = "N";    // _dtRoutOper.Rows[iNCnt]["END_PROC_FLAG"];
        //        //                param["EQPT_GR_DETL_TYPE_CODE"] = _dtRoutOper.Rows[iNCnt]["EQPT_GR_DETL_TYPE_CODE"];
        //        //                inDataTable.Rows.Add(param);
        //        //            }
        //        //        }

        //        //        if (inDataTable.Rows.Count < 1)
        //        //        {
        //        //            Helper.ShowInfo("10008");
        //        //            return;
        //        //        }

        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //        new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //        {
        //        //            try
        //        //            {
        //        //                if (outException != null)
        //        //                {
        //        //                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //                    Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE_PROC", outException);
        //        //                    return;
        //        //                }
        //        //                else
        //        //                {
        //        //                    Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //                    SelectRouteProc();
        //        //                    selectRecipeTree();
        //        //                    selectRJudgProc();
        //        //                }
        //        //            }
        //        //            catch (Exception ex)
        //        //            {
        //        //                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //                Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE_PROC", ex);
        //        //            }
        //        //            finally
        //        //            {
        //        //                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //                Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE_PROC", Logger.MESSAGE_OPERATION_END);
        //        //            }
        //        //        }, FrameOperation.MENUID);
        //        //    }
        //        //});
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_FORM_ROUTE_PROC", ex);
        //    }
        //}

        //private void btnGrade_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //FormRouteProcGrade wndGradeList = new FormRouteProcGrade();

        //        //if (wndGradeList != null)
        //        //{
        //        //    wndGradeList.FrameOperation = this.FrameOperation;

        //        //    object[] Parameters = new object[10];

        //        //    Parameters[0] = Util.NVC(txtTp2AREAID.Tag);
        //        //    Parameters[1] = Util.NVC(txtTp2AREAID.Text);

        //        //    Parameters[2] = Util.NVC(txtTp2EQSGID.Tag);
        //        //    Parameters[3] = Util.NVC(txtTp2EQSGID.Text);

        //        //    Parameters[4] = Util.NVC(txtTp2MDLLOT_ID.Tag);
        //        //    Parameters[5] = Util.NVC(txtTp2MDLLOT_ID.Text);

        //        //    Parameters[6] = Util.NVC(txtTp2ROUTID.Tag);
        //        //    Parameters[7] = Util.NVC(txtTp2ROUTID.Text);

        //        //    Parameters[8] = Util.NVC(txtTp2ROUT_TYPE_CODE.Tag);
        //        //    Parameters[9] = Util.NVC(txtTp2ROUT_TYPE_CODE.Text);

        //        //    C1WindowExtension.SetParameters(wndGradeList, Parameters);

        //        //    // 팝업 화면 숨겨지는 문제 수정.
        //        //    this.Dispatcher.BeginInvoke(new Action(() => wndGradeList.ShowModal()));
        //        //    wndGradeList.BringToFront();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void dgRouteOper_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //if (e.Column == this.dgRouteOper.Columns["SAS_TRNF_FLAG"])
            //{
            //    //용량 기본 Route일 경우
            //    if (txtTp2ROUT_TYPE_CODE.Tag.Equals("C"))
            //    {
            //        e.Cancel = false;
            //    }
            //    else
            //    {
            //        e.Cancel = true;
            //    }
            //}
        }

        private void dgRouteOper_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {


            //e.Item.SetValue("ROUTID", txtTp2ROUTID.Tag);
            //e.Item.SetValue("PROCID", _sRouteOp_ProcId);
            //e.Item.SetValue("PROCNAME", _sRouteOp_ProcName);



            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);




        }

        private void dgRouteOper_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                //C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                //DataRowView drv = e.Row.DataItem as DataRowView;

                //if (cbo != null)
                //{
                //    string sSelectedValue = drv[e.Column.Name].SafeToString();
                //    string sPROC_DETL_TYPE_CODE = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROC_DETL_TYPE_CODE");

                //    if (Convert.ToString(e.Column.Name) == "LANE_ID")
                //    {
                //        //_dvCMCD_AREA.RowFilter = string.Format("CMCDTYPE = 'LANE_FOR_FORM_ROUT_PROC' AND AREAID = '{0}' AND USE_FLAG = 'Y' ", txtTp2AREAID.Tag);
                //        //CommonCombo.SetDtCommonCombo(_dvCMCD_AREA.ToTable(), cbo, CommonCombo.ComboStatus.EMPTY, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "EQPT_GR_DETL_TYPE_CODE")
                //    {
                //        _dvCMCD.RowFilter = string.Format("CMCDTYPE = 'PROC_DETL_TYPE_CODE' AND CMCODE = '{0}' AND USE_FLAG = 'Y'", sPROC_DETL_TYPE_CODE);
                //        string sPROC_GR_CODE = _dvCMCD[0]["ATTR4"].ToString();

                //        _dvCMCD.RowFilter = string.Format("CMCDTYPE = 'EQPT_GR_DETL_TYPE_CODE' AND ATTR2 = '{0}' AND USE_FLAG = 'Y'", sPROC_GR_CODE);
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.EMPTY, sSelectedValue);
                //    }
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgRouteOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //string[] Col = { "ROUTID", "PROCID", "STRT_PROC_FLAG", "END_PROC_FLAG", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRouteOper, Col);
        }

        private void dgRouteOper_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRouteOper);
        }

        private void dgRouteOper_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {

        }

        private void chkHeaderAllTp201_Checked(object sender, RoutedEventArgs e)
        {
            //CommonUtil.DataGridCheckAllChecked(dgRouteOper);
        }

        private void chkHeaderAllTp201_Unchecked(object sender, RoutedEventArgs e)
        {
            //CommonUtil.DataGridCheckAllUnChecked(dgRouteOper);
        }

        #region Event : TapPage03 > 작업조건관리
        private void tvRecipe_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tvRecipe_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvRecipe_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void slRecipeCol01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slRecipeCol01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp301_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRecipe);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp301_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //_dvCMCD.RowFilter = string.Format("CMCDTYPE = 'PROC_DETL_TYPE_CODE' AND CMCODE = '{0}'", _sCurProcDetlType_Recipe);

        //        if (_dvCMCD.ToTable().Rows.Count > 0)
        //        {
        //            string sRowPlus = _dvCMCD.ToTable().Rows[0]["ATTR3"].ToString();

        //            if (sRowPlus == "ONLY_1")
        //            {
        //                if (((DataView)dgRecipe.ItemsSource).ToTable().Rows.Count > 0)
        //                    return;

        //                this.dgRecipe.BeginNewRow();
        //                this.dgRecipe.EndNewRow(true);
        //            }
        //            else if (sRowPlus == "PLUS")
        //            {
        //                int addRowCount = this.numAddCountTp301.Value.SafeToInt32();

        //                for (int i = 0; i < addRowCount; i++)
        //                {
        //                    this.dgRecipe.BeginNewRow();
        //                    this.dgRecipe.EndNewRow(true);
        //                }
        //            }
        //            else
        //            {
        //                //NONE 포함 : ROW 생성 않함.
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp301_Click(object sender, RoutedEventArgs e)
        //{
        //    int subRowCount = this.numAddCountTp301.Value.SafeToInt32();
        //    CommonUtil.MinusDataGridRow(dgRecipe, subRowCount);
        //}

        //private void btnRecipeSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (!CommonUtil.HasDataGridRow(dgRecipe)) return;
        //        if (!Validation_Recipe()) return;

        //        this.dgRecipe.EndEdit();
        //        this.dgRecipe.EndEditRow(true);

        //        const string bizRuleName = "BR_MMD_REG_ITEM_MST_BY_GRID";
        //        const string sBAS_ITEM_ID = "TB_MMD_FORM_ROUT_PROC_RECIPE";

        //        string sREPSTR_ID = txtTp3AREAID.Tag.SafeToString();

        //        //DataTable inDataTable = _Com.GetBR_MMD_REG_ITEM_MST_BY_GRID();

        //        //foreach (object added in dgRecipe.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(added, "PROCID");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(added, "PROC_STEP_NO");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(added, "END_TIME", ",", "");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(added, "CCURNT_VALUE", ",", "");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(added, "CVLTG_VALUE", ",", "");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(added, "END_CAPA_VALUE", ",", "");
        //        //        param["BAS_ATTR6"] = DataTableConverter.GetValue(added, "END_CURNT_VALUE", ",", "");
        //        //        param["BAS_ATTR7"] = DataTableConverter.GetValue(added, "END_VLTG_VALUE", ",", "");
        //        //        param["BAS_ATTR8"] = DataTableConverter.GetValue(added, "VLTG_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR9"] = DataTableConverter.GetValue(added, "VLTG_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR10"] = DataTableConverter.GetValue(added, "JUDG_VLTG_VALUE", ",", "");
        //        //        param["BAS_ATTR11"] = DataTableConverter.GetValue(added, "OCV_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR12"] = DataTableConverter.GetValue(added, "OCV_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR13"] = DataTableConverter.GetValue(added, "OCV_OFFSET_VALUE", ",", "");
        //        //        param["BAS_ATTR14"] = DataTableConverter.GetValue(added, "CURNT_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR15"] = DataTableConverter.GetValue(added, "CURNT_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR16"] = DataTableConverter.GetValue(added, "MGFORM_CURNT_VALUE", ",", "");
        //        //        param["BAS_ATTR17"] = DataTableConverter.GetValue(added, "MGFORM_TIME", ",", "");
        //        //        param["BAS_ATTR18"] = DataTableConverter.GetValue(added, "MGFORM_MEASR_TIME", ",", "");
        //        //        param["BAS_ATTR19"] = DataTableConverter.GetValue(added, "LIMIT_TIME", ",", "");
        //        //        param["BAS_ATTR20"] = DataTableConverter.GetValue(added, "WAIT_TIME", ",", "");
        //        //        param["BAS_ATTR21"] = DataTableConverter.GetValue(added, "AGING_MIN_TIME", ",", "");
        //        //        param["BAS_ATTR22"] = DataTableConverter.GetValue(added, "AGING_MAX_TIME", ",", "");
        //        //        param["BAS_ATTR23"] = DataTableConverter.GetValue(added, "TMPR_VALUE", ",", "");
        //        //        param["BAS_ATTR24"] = DataTableConverter.GetValue(added, "PRESS_VALUE", ",", "");
        //        //        param["BAS_ATTR25"] = DataTableConverter.GetValue(added, "SET_AFTER_PROG_TIME", ",", "");
        //        //        param["BAS_ATTR26"] = DataTableConverter.GetValue(added, "SUBLOT_DFCT_LIMIT_QTY", ",", "");
        //        //        param["BAS_ATTR27"] = DataTableConverter.GetValue(added, "SUBLOT_DFCT_LIMIT_USE_FLAG");
        //        //        param["BAS_ATTR28"] = DataTableConverter.GetValue(added, "PRFL_VLTG_DEVL_VALUE", ",", "");
        //        //        param["BAS_ATTR29"] = DataTableConverter.GetValue(added, "RECIPE_DESC");
        //        //        param["BAS_ATTR30"] = DataTableConverter.GetValue(added, "BAS_PROCID");
        //        //        param["BAS_ATTR31"] = DataTableConverter.GetValue(added, "WETTING_HOLD_TIME");
        //        //        param["BAS_ATTR32"] = DataTableConverter.GetValue(added, "WETTING_VENT_TIME");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //foreach (object modified in dgRecipe.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(modified, "PROCID");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(modified, "PROC_STEP_NO");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(modified, "END_TIME", ",", "");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(modified, "CCURNT_VALUE", ",", "");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(modified, "CVLTG_VALUE", ",", "");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(modified, "END_CAPA_VALUE", ",", "");
        //        //        param["BAS_ATTR6"] = DataTableConverter.GetValue(modified, "END_CURNT_VALUE", ",", "");
        //        //        param["BAS_ATTR7"] = DataTableConverter.GetValue(modified, "END_VLTG_VALUE", ",", "");
        //        //        param["BAS_ATTR8"] = DataTableConverter.GetValue(modified, "VLTG_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR9"] = DataTableConverter.GetValue(modified, "VLTG_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR10"] = DataTableConverter.GetValue(modified, "JUDG_VLTG_VALUE", ",", "");
        //        //        param["BAS_ATTR11"] = DataTableConverter.GetValue(modified, "OCV_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR12"] = DataTableConverter.GetValue(modified, "OCV_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR13"] = DataTableConverter.GetValue(modified, "OCV_OFFSET_VALUE", ",", "");
        //        //        param["BAS_ATTR14"] = DataTableConverter.GetValue(modified, "CURNT_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR15"] = DataTableConverter.GetValue(modified, "CURNT_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR16"] = DataTableConverter.GetValue(modified, "MGFORM_CURNT_VALUE", ",", "");
        //        //        param["BAS_ATTR17"] = DataTableConverter.GetValue(modified, "MGFORM_TIME", ",", "");
        //        //        param["BAS_ATTR18"] = DataTableConverter.GetValue(modified, "MGFORM_MEASR_TIME", ",", "");
        //        //        param["BAS_ATTR19"] = DataTableConverter.GetValue(modified, "LIMIT_TIME", ",", "");
        //        //        param["BAS_ATTR20"] = DataTableConverter.GetValue(modified, "WAIT_TIME", ",", "");
        //        //        param["BAS_ATTR21"] = DataTableConverter.GetValue(modified, "AGING_MIN_TIME", ",", "");
        //        //        param["BAS_ATTR22"] = DataTableConverter.GetValue(modified, "AGING_MAX_TIME", ",", "");
        //        //        param["BAS_ATTR23"] = DataTableConverter.GetValue(modified, "TMPR_VALUE", ",", "");
        //        //        param["BAS_ATTR24"] = DataTableConverter.GetValue(modified, "PRESS_VALUE", ",", "");
        //        //        param["BAS_ATTR25"] = DataTableConverter.GetValue(modified, "SET_AFTER_PROG_TIME", ",", "");
        //        //        param["BAS_ATTR26"] = DataTableConverter.GetValue(modified, "SUBLOT_DFCT_LIMIT_QTY", ",", "");
        //        //        param["BAS_ATTR27"] = DataTableConverter.GetValue(modified, "SUBLOT_DFCT_LIMIT_USE_FLAG");
        //        //        param["BAS_ATTR28"] = DataTableConverter.GetValue(modified, "PRFL_VLTG_DEVL_VALUE", ",", "");
        //        //        param["BAS_ATTR29"] = DataTableConverter.GetValue(modified, "RECIPE_DESC");
        //        //        param["BAS_ATTR30"] = DataTableConverter.GetValue(modified, "BAS_PROCID");
        //        //        param["BAS_ATTR31"] = DataTableConverter.GetValue(modified, "WETTING_HOLD_TIME");
        //        //        param["BAS_ATTR32"] = DataTableConverter.GetValue(modified, "WETTING_VENT_TIME");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    Helper.ShowInfo("10008");
        //        //    return;
        //        //}

        //        //Aging 종료시간은 1건만 존재
        //        //_sFormRoutProcRecipeEndTime = inDataTable.Rows[0]["BAS_ATTR2"].SafeToString();

        //        //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            if ((_sCurProcGr_Recipe == R.FcsComCode.PREAGING_EQPTYPE) || (_sCurProcGr_Recipe == R.FcsComCode.LOWAGING_EQPTYPE) || (_sCurProcGr_Recipe == R.FcsComCode.HIGHAGING_EQPTYPE) || (_sCurProcGr_Recipe == R.FcsComCode.OUTAGING_EQPTYPE))
        //        //            {
        //        //                //현재 작업중인 Tray에 변경된 Plan Time을 반영하시겠습니까?
        //        //                Helper.ShowConfirm("FM_ME_0265", (MessageBoxResult mbr) =>
        //        //                {
        //        //                    if (mbr == MessageBoxResult.OK)
        //        //                    {
        //        //                        SaveWipAttrAgingIssSchdDttm(_sFormRoutProcRecipeEndTime);
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //                        SelectFormRoutProcRecipe();
        //        //                        selectSublotGrade();
        //        //                    }
        //        //                });
        //        //            }
        //        //            else
        //        //            {
        //        //                Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //                SelectFormRoutProcRecipe();
        //        //                selectSublotGrade();
        //        //            }
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //    }
        //}

        private void dgRecipe_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv == null)
            //{
            //    e.Cancel = true;
            //    int subRowCount = this.numAddCountTp301.Value.SafeToInt32();
            //    CommonUtil.MinusDataGridRow(dgRecipe, subRowCount);
            //    dgRecipe.EndEditRow(false);
            //    return;
            //}

            ////if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgRecipe.Columns["CHK"])
            ////{
            ////    e.Cancel = true;
            ////    return;
            ////}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgRecipe.Columns["CHK"]
            //     && e.Column != this.dgRecipe.Columns["USE_FLAG"]
            //     && e.Column != this.dgRecipe.Columns["END_TIME"]
            //     && e.Column != this.dgRecipe.Columns["CCURNT_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["CVLTG_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["END_CAPA_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["END_CURNT_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["END_VLTG_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["VLTG_MAX_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["VLTG_MIN_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["JUDG_VLTG_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["OCV_MIN_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["OCV_MAX_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["OCV_OFFSET_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["CURNT_MIN_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["CURNT_MAX_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["MGFORM_CURNT_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["MGFORM_TIME"]
            //     && e.Column != this.dgRecipe.Columns["MGFORM_MEASR_TIME"]
            //     && e.Column != this.dgRecipe.Columns["LIMIT_TIME"]
            //     && e.Column != this.dgRecipe.Columns["WAIT_TIME"]
            //     && e.Column != this.dgRecipe.Columns["AGING_MIN_TIME"]
            //     && e.Column != this.dgRecipe.Columns["AGING_MAX_TIME"]
            //     && e.Column != this.dgRecipe.Columns["TMPR_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["PRESS_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["SET_AFTER_PROG_TIME"]
            //     && e.Column != this.dgRecipe.Columns["SUBLOT_DFCT_LIMIT_QTY"]
            //     && e.Column != this.dgRecipe.Columns["SUBLOT_DFCT_LIMIT_USE_FLAG"]
            //     && e.Column != this.dgRecipe.Columns["PRFL_VLTG_DEVL_VALUE"]
            //     && e.Column != this.dgRecipe.Columns["RECIPE_DESC"]
            //     && e.Column != this.dgRecipe.Columns["BAS_PROCID"]
            //     && e.Column != this.dgRecipe.Columns["WETTING_HOLD_TIME"]
            //     && e.Column != this.dgRecipe.Columns["WETTING_VENT_TIME"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}
        }

        private void dgRecipe_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("ROUTID", txtTp3ROUTID.Tag);
            //e.Item.SetValue("PROCID", _sCurProcId_Recipe);
            //e.Item.SetValue("SUBLOT_DFCT_LIMIT_USE_FLAG", "N");
        }

        private void dgRecipe_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1.WPF.C1ComboBox combo = e.EditingElement as C1.WPF.C1ComboBox;

            if (combo != null)
            {
                DataRowView drv = e.Row.DataItem as DataRowView;

                //if (e.Column.Name.ContainsValue("BAS_PROCID"))
                //{
                //    C1.WPF.DataGrid.DataGridComboBoxColumn comboColumn = this.dgRecipe.Columns[e.Column.Name] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                //    if (comboColumn != null)
                //    {
                //        combo.Tag = drv[e.Column.Name].SafeToString();

                //        DataView view = comboColumn.ItemsSource as DataView;
                //        DataTable copy = view.Table.Copy();
                //        copy.DefaultView.RowFilter = "USE_FLAG = 'Y'";

                //        combo.ItemsSource = DataTableConverter.Convert(copy.DefaultView.ToTable());
                //        combo.BindWithStatus(ComboBoxExtension.ComboStatus.EMPTY);

                //        combo.IsDropDownOpenChanged += delegate (object sender1, C1.WPF.PropertyChangedEventArgs<bool> pcea)
                //        {
                //            if (pcea.NewValue)
                //            {
                //                combo.SelectedIndex = 0;
                //                combo.SelectedValue = combo.Tag.SafeToString();
                //            }

                //        };
                //    }
                //}
            }
        }

        private void dgRecipe_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRecipe, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRecipe, Col);
            //}

            DataRowView drv = e.Cell.Row.DataItem as DataRowView;

            if (drv != null && drv.Row.RowState != DataRowState.Added)
            {
                string[] Col = { "ROUTID", "PROCID", "PROC_STEP_NO", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
                CommonUtil.DataGridReadOnlyBackgroundColor(e, this.dgRecipe, Col);
            }
            else
            {
                string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
                CommonUtil.DataGridReadOnlyBackgroundColor(e, this.dgRecipe, Col);
            }
        }

        private void dgRecipe_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRecipe);
        }

        private void dgRecipe_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {

        }

        private void chkHeaderAllTp301_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgRecipe);
        }

        private void chkHeaderAllTp301_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgRecipe);
        }

        private void slRecipeRow01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slRecipeRow01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp302_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLoc);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp302_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        int addRowCount = this.numAddCountTp302.Value.SafeToInt32();

        //        for (int i = 0; i < addRowCount; i++)
        //        {
        //            this.dgLoc.BeginNewRow();
        //            this.dgLoc.EndNewRow(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp302_Click(object sender, RoutedEventArgs e)
        //{
        //    int subRowCount = this.numAddCountTp302.Value.SafeToInt32();
        //    CommonUtil.MinusDataGridRow(dgLoc, subRowCount);
        //}

        //private void btnLocSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (!CommonUtil.HasDataGridRow(dgLoc)) return;
        //        if (!Validation_Loc()) return;

        //        this.dgLoc.EndEdit();
        //        this.dgLoc.EndEditRow(true);

        //        const string bizRuleName = "BR_MMD_REG_ITEM_MST_BY_GRID";
        //        const string sBAS_ITEM_ID = "TB_MMD_GRD_SLCTR_PSTN_SET";

        //        string sREPSTR_ID = txtTp3AREAID.Tag.SafeToString();

        //        //DataTable inDataTable = _Com.GetBR_MMD_REG_ITEM_MST_BY_GRID();

        //        //foreach (object added in dgLoc.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(added, "SUBLOT_GRD_CODE");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(added, "PROCID");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(added, "GRD_SLCTR_LOCATION_CODE");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(added, "DFCT_GRD_FLAG");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //foreach (object modified in dgLoc.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(modified, "SUBLOT_GRD_CODE");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(modified, "PROCID");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(modified, "GRD_SLCTR_LOCATION_CODE");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(modified, "DFCT_GRD_FLAG");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    Helper.ShowInfo("10008");
        //        //    return;
        //        //}

        //        loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //            SelectGradSelecterLoadLocSet();
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //    }
        //}

        private void dgLoc_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgLoc.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgLoc.Columns["CHK"]
            //     && e.Column != this.dgLoc.Columns["USE_FLAG"]
            //     && e.Column != this.dgLoc.Columns["GRD_SLCTR_LOCATION_CODE"]
            //     && e.Column != this.dgLoc.Columns["DFCT_GRD_FLAG"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}
        }

        private void dgLoc_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("ROUTID", txtTp3ROUTID.Tag);
        }

        private void dgLoc_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                //C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                //DataRowView drv = e.Row.DataItem as DataRowView;

                //string sSelectedValue = drv[e.Column.Name].SafeToString();

                //if (cbo != null)
                //{
                //    if (Convert.ToString(e.Column.Name) == "SUBLOT_GRD_CODE")
                //    {
                //        C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = this.dgLoc.Columns[e.Column.Name] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                //        DataView dv = cboColumn.ItemsSource as DataView;
                //        DataTable dtCopy = dv.Table.Copy();

                //        dtCopy.DefaultView.RowFilter = "USE_FLAG = 'Y'";

                //        //CommonCombo.SetDtCommonCombo(dtCopy, cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "PROCID")
                //    {
                //        _dvPROC_ROUT.RowFilter = string.Format("USE_FLAG = 'Y' AND PROC_GR_CODE IN ({0}) AND CMCODE NOT IN ({1})", "'9','3','4','5','6','D','J','G','7'", "'FFJ000','FFJ909','FF7101','FF7102'");
                //        CommonCombo.SetDtCommonCombo(_dvPROC_ROUT.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "GRD_SLCTR_LOCATION_CODE")
                //    {
                //        string sPROCID = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROCID");
                //        string sProcGrCd = null;

                //        if (sPROCID.Substring(2, 1).Equals("5"))
                //        {
                //            //EOL 공정
                //            sProcGrCd = "AND ATTR1 = '5'";
                //        }
                //        else
                //        {
                //            sProcGrCd = "AND ATTR1 <> '5'";
                //        }

                //        _dvCMCD.RowFilter = string.Format("CMCDTYPE = 'GRD_SLCTR_LOCATION_CODE' AND USE_FLAG = 'Y' {0}", sProcGrCd);
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "DFCT_GRD_FLAG")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'DFCT_GRD_FLAG' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                //    {
                //        Dispatcher.BeginInvoke(new Action(() =>
                //        {
                //            this.UpdateRowView_Loc(e.Row, e.Column);
                //        }));

                //    };
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void UpdateRowView_Loc(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "PROCID")
                {
                    _dgLocCommit = true;
                    drv["GRD_SLCTR_LOCATION_CODE"] = string.Empty;
                    this.dgLoc.EndEditRow(true);
                }
            }
            finally
            {
                _dgLocCommit = false;
            }
        }

        private void dgLoc_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "SUBLOT_GRD_CODE", "PROCID", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgLoc, Col);
            ////}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgLoc, Col);
            //}
        }

        private void dgLoc_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgLoc);
        }

        private bool _dgLocCommit = false;
        private void dgLoc_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_dgLocCommit == false)
            {
                this.UpdateRowView_Loc(e.Row, e.Column);
            }
        }

        private void chkHeaderAllTp302_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgLoc);
        }

        private void chkHeaderAllTp302_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgLoc);
        }

        //작업조건 관리 Tree Click        
        private void tvRecipe_Level1_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                _sCurProcId_Recipe = null;
                _sCurProcGr_Recipe = null;
                _sCurProcDetlType_Recipe = null;

                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;
                DataRow data = tvi.DataContext as DataRow;

                _sCurProcId_Recipe = data["KEY_VAL"].SafeToString();
                _sCurProcGr_Recipe = data["PROC_GR_CODE"].SafeToString();
                _sCurProcDetlType_Recipe = data["PROC_DETL_TYPE_CODE"].SafeToString();

                SetRecipeColumn(_sCurProcDetlType_Recipe);
                SelectFormRoutProcRecipe();

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void cboUseFlagTp301_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectFormRoutProcRecipe();
        }

        private void cboUseFlagTp302_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectGradSelecterLoadLocSet();
        }

        private void btnMaxTimeRecipe_Click(object sender, RoutedEventArgs e)
        {
            //int iRowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            //dgRecipe.SelectedIndex = iRowIndex;

            //if (DataTableConverter.GetValue(dgRecipe.Rows[iRowIndex].DataItem, "CHK").SafeToBoolean() == false) return;

            //DataTableConverter.SetValue(dgRecipe.Rows[iRowIndex].DataItem, "END_TIME", "999999");

            //this.dgRecipe.EndEdit();
            //this.dgRecipe.EndEditRow(true);
        }

        #endregion

        #region Event : TapPage04 > 판정등급관리
        private void tvDataType_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tvDataType_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvDataType_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void slJudgCol01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slJudgCol01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnImportTp401_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //활성화 라우트관련 엑설 업로드 금지!
                //ExcelImportEditor frm = new ExcelImportEditor("TB_MMD_GRD_UNIT_JUDG_SPEC", this.dgUjudg);

                //frm.FrameOperation = this.FrameOperation;
                //frm.FormClosed += delegate ()
                //{
                //    SelectGradeUnitJudgSpec();
                //};

                //frm.ShowModal();
                //frm.CenterOnScreen();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void btnExportTp401_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgUjudg);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp401_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        int addRowCount = this.numAddCountTp401.Value.SafeToInt32();

        //        for (int i = 0; i < addRowCount; i++)
        //        {
        //            this.dgUjudg.BeginNewRow();
        //            this.dgUjudg.EndNewRow(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp401_Click(object sender, RoutedEventArgs e)
        //{
        //    int subRowCount = this.numAddCountTp401.Value.SafeToInt32();
        //    CommonUtil.MinusDataGridRow(dgUjudg, subRowCount);
        //}

        private DataTable GetFittedGrid()
        {
            DataTable dtFittedGrid = new DataTable();

            dtFittedGrid.Columns.Add("ROUTID", typeof(string));
            dtFittedGrid.Columns.Add("PROCID", typeof(string));
            dtFittedGrid.Columns.Add("MEASR_TYPE_CODE", typeof(string));
            dtFittedGrid.Columns.Add("UNIT_JUDG_GRD_CODE", typeof(string));
            dtFittedGrid.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
            dtFittedGrid.Columns.Add("GRD_ROW_NO", typeof(string));
            dtFittedGrid.Columns.Add("GRD_COL_NO", typeof(string));

            return dtFittedGrid;
        }

        //private void btnUjudgeSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (!CommonUtil.HasDataGridRow(dgUjudg)) return;
        //        if (!Validation_GradeUnitJudgSpac()) return;

        //        this.dgUjudg.EndEdit();
        //        this.dgUjudg.EndEditRow(true);

        //        const string bizRuleName = "BR_MMD_REG_ITEM_MST_BY_GRID";
        //        const string sBAS_ITEM_ID = "TB_MMD_GRD_UNIT_JUDG_SPEC";

        //        string sREPSTR_ID = txtTp4AREAID.Tag.SafeToString();

        //        //DataTable inDataTable = _Com.GetBR_MMD_REG_ITEM_MST_BY_GRID();
        //        //DataTable dtFittedGrid = GetFittedGrid();

        //        //foreach (object added in dgUjudg.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(added, "PROCID");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(added, "MEASR_TYPE_CODE");
        //        //        param["BAS_KEY_ATTR4"] = DataTableConverter.GetValue(added, "UNIT_JUDG_GRD_CODE");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(added, "MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(added, "MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(added, "RJUDG_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(added, "RJUDG_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(added, "RJUDG_ABS_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR6"] = DataTableConverter.GetValue(added, "RJUDG_ABS_MIN_VALUE", ",", "");
        //        //        inDataTable.Rows.Add(param);

        //        //        DataRow drFittedGrid = dtFittedGrid.NewRow();
        //        //        drFittedGrid["ROUTID"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        drFittedGrid["PROCID"] = DataTableConverter.GetValue(added, "PROCID");
        //        //        drFittedGrid["MEASR_TYPE_CODE"] = DataTableConverter.GetValue(added, "MEASR_TYPE_CODE");
        //        //        drFittedGrid["UNIT_JUDG_GRD_CODE"] = DataTableConverter.GetValue(added, "UNIT_JUDG_GRD_CODE");
        //        //        dtFittedGrid.Rows.Add(drFittedGrid);
        //        //    }
        //        //}

        //        //foreach (object modified in dgUjudg.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(modified, "PROCID");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(modified, "MEASR_TYPE_CODE");
        //        //        param["BAS_KEY_ATTR4"] = DataTableConverter.GetValue(modified, "UNIT_JUDG_GRD_CODE");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(modified, "MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(modified, "MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(modified, "RJUDG_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(modified, "RJUDG_MIN_VALUE", ",", "");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(modified, "RJUDG_ABS_MAX_VALUE", ",", "");
        //        //        param["BAS_ATTR6"] = DataTableConverter.GetValue(modified, "RJUDG_ABS_MIN_VALUE", ",", "");
        //        //        inDataTable.Rows.Add(param);

        //        //        DataRow drFittedGrid = dtFittedGrid.NewRow();
        //        //        drFittedGrid["ROUTID"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        drFittedGrid["PROCID"] = DataTableConverter.GetValue(modified, "PROCID");
        //        //        drFittedGrid["MEASR_TYPE_CODE"] = DataTableConverter.GetValue(modified, "MEASR_TYPE_CODE");
        //        //        drFittedGrid["UNIT_JUDG_GRD_CODE"] = DataTableConverter.GetValue(modified, "UNIT_JUDG_GRD_CODE");
        //        //        dtFittedGrid.Rows.Add(drFittedGrid);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    Helper.ShowInfo("10008");
        //        //    return;
        //        //}

        //        //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            if (txtTp4ROUT_TYPE_CODE.Tag.SafeToString() == "C")
        //        //            {
        //        //                //제품관리 > 활성화 모형 Fitted 등급 관리(머신러닝) 화면 확인
        //        //                //TB_MMD_FORM_FITTED_JUDG_GRD_SPEC
        //        //                SaveFittedJudgGrdSpec(sBAS_ITEM_ID, dtFittedGrid);
        //        //            }
        //        //            else
        //        //            {
        //        //                Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //                SelectGradeUnitJudgSpec();
        //        //            }
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //    }
        //}

        private void btnUjudgDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgUjudg_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgUjudg.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgUjudg.Columns["CHK"]
            //     && e.Column != this.dgUjudg.Columns["USE_FLAG"]
            //     && e.Column != this.dgUjudg.Columns["MAX_VALUE"]
            //     && e.Column != this.dgUjudg.Columns["MIN_VALUE"]
            //     && e.Column != this.dgUjudg.Columns["RJUDG_MAX_VALUE"]
            //     && e.Column != this.dgUjudg.Columns["RJUDG_MIN_VALUE"]
            //     && e.Column != this.dgUjudg.Columns["RJUDG_ABS_MAX_VALUE"]
            //     && e.Column != this.dgUjudg.Columns["RJUDG_ABS_MIN_VALUE"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}
        }
        private void dgUjudg_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("ROUTID", txtTp4ROUTID.Tag);
            //e.Item.SetValue("MEASR_TYPE_CODE", _sCurMeasrType);
        }

        private void dgUjudg_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;

                string sSelectedValue = drv[e.Column.Name].SafeToString();

                //if (cbo != null)
                //{
                //    if (Convert.ToString(e.Column.Name) == "PROCID")
                //    {
                //        _dvPROC_ROUT.RowFilter = "USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvPROC_ROUT.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "MEASR_TYPE_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'MEASR_TYPE_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                //    }
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgUjudg_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "PROCID", "MEASR_TYPE_CODE", "UNIT_JUDG_GRD_CODE", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRecipe, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRecipe, Col);
            //}
        }

        private void dgUjudg_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgUjudg);
        }

        private void dgUjudg_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {

        }

        private void chkHeaderAllTp401_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgUjudg);
        }

        private void chkHeaderAllTp401_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgUjudg);
        }

        private void slJudgRow01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slJudgRow01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void tvGrade_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    C1TreeView tv = (C1TreeView)sender;

            //    C1TreeViewItem tvi = tv.GetNode(e.GetPosition(null));
            //    this.contextTreeItem_Judge_Proc = tvi;

            //    if (tvi != null)
            //    {
            //        DataRow data = tvi.DataContext as DataRow;

            //        if (data["SORT_NO"].SafeToString() == "1") return;

            //        if (data["PKEY_VAL"].SafeToString() == "000" && data["SORT_NO"].SafeToString() == "2")
            //        {
            //            this.contextMenu_Add_Judge_Proc.IsOpen = true;
            //            ContextMenu contextMenu = this.contextMenu_Add_Judge_Proc;
            //            Point position = e.GetPosition(null);
            //            contextMenu.HorizontalOffset = position.X;
            //            ContextMenu contextMenu1 = this.contextMenu_Add_Judge_Proc;
            //            position = e.GetPosition(null);
            //            contextMenu1.VerticalOffset = position.Y;
            //        }

            //        if (data["PKEY_VAL"].SafeToString() != "000" && data["SORT_NO"].SafeToString() == "2")
            //        {

            //            this.contextMenu_Del_Judge_Proc.IsOpen = true;
            //            ContextMenu contextMenu = this.contextMenu_Del_Judge_Proc;
            //            Point position = e.GetPosition(null);
            //            contextMenu.HorizontalOffset = position.X;
            //            ContextMenu contextMenu1 = this.contextMenu_Del_Judge_Proc;
            //            position = e.GetPosition(null);
            //            contextMenu1.VerticalOffset = position.Y;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void tvGrade_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvGrade_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void slJudgCol02_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slJudgCol02_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp402_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgCjudg);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp402_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(txtTp4ROUTID.Tag.SafeToString())) return;
        //        if (string.IsNullOrEmpty(txtJUDG_GRADE.Tag.SafeToString())) return;

        //        int addRowCount = 1;//this.numAddCountTp402.Value.SafeToInt32();

        //        for (int i = 0; i < addRowCount; i++)
        //        {
        //            this.dgCjudg.BeginNewRow();
        //            this.dgCjudg.EndNewRow(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp402_Click(object sender, RoutedEventArgs e)
        //{
        //    int subRowCount = 1;// this.numAddCount.Value.SafeToInt32();
        //    CommonUtil.MinusDataGridRow(dgCjudg, subRowCount);
        //}

        //private void btnCjudgSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (!Common.CommonVerify.HasDataGridRow(dgCjudg)) return;
        //        if (!CommonUtil.HasDataGridRow(dgCjudg)) return;
        //        if (!Validation_Cjudg()) return;

        //        this.dgCjudg.EndEdit();
        //        this.dgCjudg.EndEditRow(true);

        //        const string bizRuleName = "BR_MMD_REG_ITEM_MST_BY_GRID";
        //        const string sBAS_ITEM_ID = "TB_MMD_ROUT_GRD_MJUDG_SET";

        //        string sREPSTR_ID = txtTp4AREAID.Tag.SafeToString();

        //        //DataTable inDataTable = _Com.GetBR_MMD_REG_ITEM_MST_BY_GRID();
        //        //DataTable dtFittedGrid = GetFittedGrid();

        //        //foreach (object added in dgCjudg.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(added, "SUBLOT_GRD_CODE");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(added, "GRD_ROW_NO");
        //        //        param["BAS_KEY_ATTR4"] = DataTableConverter.GetValue(added, "GRD_COL_NO");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(added, "PROCID");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(added, "MEASR_TYPE_CODE");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(added, "MAX_UNIT_JUDG_GRD_CODE");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(added, "MIN_UNIT_JUDG_GRD_CODE");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(added, "JUDG_TYPE_CODE");
        //        //        param["BAS_ATTR6"] = DataTableConverter.GetValue(added, "RJUDG_BAS_CODE");
        //        //        inDataTable.Rows.Add(param);

        //        //        DataRow drFittedGrid = dtFittedGrid.NewRow();
        //        //        drFittedGrid["ROUTID"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        drFittedGrid["SUBLOT_GRD_CODE"] = DataTableConverter.GetValue(added, "SUBLOT_GRD_CODE");
        //        //        drFittedGrid["GRD_ROW_NO"] = DataTableConverter.GetValue(added, "GRD_ROW_NO");
        //        //        drFittedGrid["GRD_COL_NO"] = DataTableConverter.GetValue(added, "GRD_COL_NO");
        //        //        dtFittedGrid.Rows.Add(drFittedGrid);
        //        //    }
        //        //}

        //        //foreach (object modified in dgCjudg.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(modified, "SUBLOT_GRD_CODE");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(modified, "GRD_ROW_NO");
        //        //        param["BAS_KEY_ATTR4"] = DataTableConverter.GetValue(modified, "GRD_COL_NO");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(modified, "PROCID");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(modified, "MEASR_TYPE_CODE");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(modified, "MAX_UNIT_JUDG_GRD_CODE");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(modified, "MIN_UNIT_JUDG_GRD_CODE");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(modified, "JUDG_TYPE_CODE");
        //        //        param["BAS_ATTR6"] = DataTableConverter.GetValue(modified, "RJUDG_BAS_CODE");
        //        //        inDataTable.Rows.Add(param);

        //        //        DataRow drFittedGrid = dtFittedGrid.NewRow();
        //        //        drFittedGrid["ROUTID"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        drFittedGrid["SUBLOT_GRD_CODE"] = DataTableConverter.GetValue(modified, "SUBLOT_GRD_CODE");
        //        //        drFittedGrid["GRD_ROW_NO"] = DataTableConverter.GetValue(modified, "GRD_ROW_NO");
        //        //        drFittedGrid["GRD_COL_NO"] = DataTableConverter.GetValue(modified, "GRD_COL_NO");
        //        //        dtFittedGrid.Rows.Add(drFittedGrid);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    Helper.ShowInfo("10008");
        //        //    return;
        //        //}

        //        //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            if (txtTp4ROUT_TYPE_CODE.Tag.SafeToString() == "C")
        //        //            {
        //        //                //제품관리 > 활성화 모형 Fitted 등급 관리(머신러닝) 화면 확인
        //        //                //TB_MMD_FORM_FITTED_JUDG_GRD_SPEC
        //        //                SaveFittedJudgGrdSpec(sBAS_ITEM_ID, dtFittedGrid);
        //        //            }
        //        //            else
        //        //            {
        //        //                Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //                SelectRouteGradeMJudgSet();
        //        //            }
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //    }
        //}

        private void btnCjudgDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgCjudg_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgCjudg.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgCjudg.Columns["CHK"]
            //     && e.Column != this.dgCjudg.Columns["USE_FLAG"]
            //     && e.Column != this.dgCjudg.Columns["PROCID"]
            //     && e.Column != this.dgCjudg.Columns["MEASR_TYPE_CODE"]
            //     && e.Column != this.dgCjudg.Columns["MAX_UNIT_JUDG_GRD_CODE"]
            //     && e.Column != this.dgCjudg.Columns["MIN_UNIT_JUDG_GRD_CODE"]
            //     && e.Column != this.dgCjudg.Columns["JUDG_TYPE_CODE"]
            //     && e.Column != this.dgCjudg.Columns["RJUDG_BAS_CODE"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}

            //if (drv != null && e.Column.Name.Equals("RJUDG_BAS_CODE"))
            //{
            //    string sRJUDG_BAS_USE_FLAG = string.Empty;
            //    string sJUDG_TYPE_CODE = drv["JUDG_TYPE_CODE"].SafeToString();

            //    _dvCMCD.RowFilter = string.Format("CMCDTYPE = 'JUDG_TYPE_CODE' AND CMCODE = '{0}'", sJUDG_TYPE_CODE);

            //    if (_dvCMCD.ToTable().Rows.Count > 0)
            //    {
            //        sRJUDG_BAS_USE_FLAG = _dvCMCD.ToTable().Rows[0]["ATTR1"].ToString();

            //        //Tray 상대판정(A),(Z) 일때만 상대조건 선택 가능
            //        if (sRJUDG_BAS_USE_FLAG == "Y")
            //        {
            //            e.Cancel = false;
            //        }
            //        else
            //        {
            //            e.Cancel = true;
            //        }
            //    }
            //}
        }

        private void dgCjudg_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("ROUTID", txtTp4ROUTID.Tag.SafeToString());
            //e.Item.SetValue("SUBLOT_GRD_CODE", txtJUDG_GRADE.Tag.SafeToString());


            //DataTable dtCjudg = DataTableConverter.Convert(dgCjudg.ItemsSource);

            //if (dtCjudg.Rows.Count > 0)
            //{
            //    if (rdoCjudgAnd.IsChecked == false && rdoCjudgOr.IsChecked == false)
            //    {
            //        Helper.ShowInfo("MMD0124");  //AND/OR 조건을 선택해주세요.             
            //        return;
            //    }
            //}

            //if (dtCjudg.Rows.Count == 0)
            //{
            //    e.Item.SetValue("GRD_ROW_NO", "1");
            //    e.Item.SetValue("GRD_COL_NO", "1");
            //}
            //else
            //{
            //    string sGRD_ROW_NO = null;
            //    string sGRD_COL_NO = null;

            //    if (rdoCjudgAnd.IsChecked == true)
            //    {
            //        sGRD_ROW_NO = dtCjudg.Rows[dtCjudg.Rows.Count - 1]["GRD_ROW_NO"].SafeToString();
            //        sGRD_COL_NO = dtCjudg.Rows[dtCjudg.Rows.Count - 1]["GRD_COL_NO"].SafeToString();
            //        e.Item.SetValue("GRD_ROW_NO", sGRD_ROW_NO);
            //        e.Item.SetValue("GRD_COL_NO", Convert.ToInt16(sGRD_COL_NO) + 1);
            //    }
            //    else if (rdoCjudgOr.IsChecked == true)
            //    {
            //        sGRD_ROW_NO = dtCjudg.Rows[dtCjudg.Rows.Count - 1]["GRD_ROW_NO"].SafeToString();
            //        sGRD_COL_NO = dtCjudg.Rows[dtCjudg.Rows.Count - 1]["GRD_COL_NO"].SafeToString();
            //        e.Item.SetValue("GRD_ROW_NO", Convert.ToInt16(sGRD_ROW_NO) + 1);
            //        e.Item.SetValue("GRD_COL_NO", "1");
            //    }
            //}
        }

        private void dgCjudg_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                PopupFindControl pop = e.EditingElement as PopupFindControl;
                DataRowView drv = e.Row.DataItem as DataRowView;

                string sSelectedValue = drv[e.Column.Name].SafeToString();
                string sMEASR_TYPE_CODE = (string)DataTableConverter.GetValue(e.Row.DataItem, "MEASR_TYPE_CODE");
                string sPROCID = (string)DataTableConverter.GetValue(e.Row.DataItem, "PROCID");

                //if (cbo != null)
                //{
                //    if (Convert.ToString(e.Column.Name) == "MEASR_TYPE_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'MEASR_TYPE_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "PROCID")
                //    {
                //        _dvPROC_UNIT.RowFilter = string.Format("USE_FLAG = 'Y' AND MEASR_TYPE_CODE = '{0}'", sMEASR_TYPE_CODE);
                //        CommonCombo.SetDtCommonCombo(_dvPROC_UNIT.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "MAX_UNIT_JUDG_GRD_CODE" || Convert.ToString(e.Column.Name) == "MIN_UNIT_JUDG_GRD_CODE")
                //    {
                //        C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = this.dgCjudg.Columns[e.Column.Name] as C1.WPF.DataGrid.DataGridComboBoxColumn;

                //        DataView dvJudgGrd = cboColumn.ItemsSource as DataView;
                //        dvJudgGrd.RowFilter = string.Format("USE_FLAG = 'Y' AND MEASR_TYPE_CODE = '{0}' AND PROCID = '{1}'", sMEASR_TYPE_CODE, sPROCID);

                //        CommonCombo.SetDtCommonCombo(dvJudgGrd.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "JUDG_TYPE_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_TYPE_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "RJUDG_BAS_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'RJUDG_BAS_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.Empty, sSelectedValue);
                //    }

                //    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                //    {
                //        Dispatcher.BeginInvoke(new Action(() =>
                //        {
                //            this.UpdateRowView_Cjudg(e.Row, e.Column);
                //        }));

                //    };
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void UpdateRowView_Cjudg(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                //DataRowView drv = dgr.DataItem as DataRowView;

                //if (drv != null && Convert.ToString(dgc.Name) == "MEASR_TYPE_CODE")
                //{
                //    _manualCommit_Cjudg = true;
                //    drv["PROCID"] = string.Empty;
                //    drv["MAX_UNIT_JUDG_GRD_CODE"] = string.Empty;
                //    drv["MIN_UNIT_JUDG_GRD_CODE"] = string.Empty;
                //    this.dgCjudg.EndEditRow(true);
                //}

                //if (drv != null && Convert.ToString(dgc.Name) == "PROCID")
                //{
                //    _manualCommit_Cjudg = true;
                //    drv["MAX_UNIT_JUDG_GRD_CODE"] = string.Empty;
                //    drv["MIN_UNIT_JUDG_GRD_CODE"] = string.Empty;
                //}
            }
            finally
            {
                _manualCommit_Cjudg = false;
            }
        }

        private void dgCjudg_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "SUBLOT_GRD_CODE", "GRD_ROW_NO", "GRD_COL_NO", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgCjudg, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgCjudg, Col);
            //}
        }

        private void dgCjudg_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgCjudg);
        }

        private bool _manualCommit_Cjudg = false;
        private void dgCjudg_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_manualCommit_Cjudg == false)
            {
                this.UpdateRowView_Cjudg(e.Row, e.Column);
            }
        }

        private void chkHeaderAllTp402_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgCjudg);
        }

        private void chkHeaderAllTp402_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgCjudg);
        }

        private void cboUseFlagTp401_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectGradeUnitJudgSpec();
        }
        private void tvDataType_Level1_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                _sCurMeasrType = null;

                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;
                DataRow data = tvi.DataContext as DataRow;

                _sCurMeasrType = data["CMCODE"].SafeToString();

                SelectGradeUnitJudgSpec();

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void tvGradeMJudg_Level1_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;
                DataRow data = tvi.DataContext as DataRow;

                txtJUDG_OP.Text = data["NAME_VAL"].SafeToString();
                txtJUDG_OP.Tag = data["KEY_VAL"].SafeToString();
                txtJUDG_GRADE.Text = "";
                txtJUDG_GRADE.Tag = "";

                Util.gridClear(dgCjudg);
                SetDataGridCheckHeaderInitialize(dgCjudg);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void tvGradeMJudg_Level2_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;

                DataRow parent = tvi.ParentItem.DataContext as DataRow;
                DataRow data = tvi.DataContext as DataRow;

                txtJUDG_OP.Text = parent["NAME_VAL"].SafeToString();
                txtJUDG_OP.Tag = parent["KEY_VAL"].SafeToString();
                txtJUDG_GRADE.Text = data["NAME_VAL"].SafeToString();
                txtJUDG_GRADE.Tag = data["KEY_VAL"].SafeToString(); ;

                SelectRouteGradeMJudgSet();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void btnCjudgSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FORM001_ROUTE_MMD_RouteGradeMJudgSet wndGradeMjudgList = new FORM001_ROUTE_MMD_RouteGradeMJudgSet();

                if (wndGradeMjudgList != null)
                {
                    wndGradeMjudgList.FrameOperation = this.FrameOperation;

                    object[] Parameters = new object[10];

                    Parameters[0] = Util.NVC(txtTp4AREAID.Tag);
                    Parameters[1] = Util.NVC(txtTp4AREAID.Text);

                    Parameters[2] = Util.NVC(txtTp4EQSGID.Tag);
                    Parameters[3] = Util.NVC(txtTp4EQSGID.Text);

                    Parameters[4] = Util.NVC(txtTp4MDLLOT_ID.Tag);
                    Parameters[5] = Util.NVC(txtTp4MDLLOT_ID.Text);

                    Parameters[6] = Util.NVC(txtTp4ROUTID.Tag);
                    Parameters[7] = Util.NVC(txtTp4ROUTID.Text);

                    Parameters[8] = Util.NVC(txtTp4ROUT_TYPE_CODE.Tag);
                    Parameters[9] = Util.NVC(txtTp4ROUT_TYPE_CODE.Text);

                    C1WindowExtension.SetParameters(wndGradeMjudgList, Parameters);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndGradeMjudgList.ShowModal()));
                    wndGradeMjudgList.BringToFront();
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboUseFlagTp402_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectRouteGradeMJudgSet();
        }
        #endregion

        #region Event : TapPage05 > 상대판정 등급관리
        private void btnExportTp501_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRJudgOp);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPlusTp501_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMinusTp501_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void btnRJudgSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (!Common.CommonVerify.HasDataGridRow(dgRJudgOp)) return;
        //        if (!CommonUtil.HasDataGridRow(dgRJudgOp)) return;
        //        //if (!Validation_RoutGrLotRJudgProc()) return;

        //        //Helper.ShowConfirm("9085", (MessageBoxResult mbr) =>
        //        //{
        //        //    if (mbr == MessageBoxResult.OK)
        //        //    {

        //        //        this.dgRJudgOp.EndEdit();
        //        //        this.dgRJudgOp.EndEditRow(true);

        //        //        const string bizRuleName = "BR_MMD_REG_FORM_ROUTE_RJUDG_PROC";
        //        //        const string sBAS_ITEM_ID = "TB_MMD_ROUT_GR_LOT_RJUDG_PROC";

        //        //        string sREPSTR_ID = txtTp5AREAID.Tag.SafeToString();

        //        //        DataTable inDataTable = new DataTable();
        //        //        inDataTable.Columns.Add("REPSTR_ID", typeof(string));
        //        //        inDataTable.Columns.Add("BAS_ITEM_ID", typeof(string));
        //        //        inDataTable.Columns.Add("KEY_SEQ_NO", typeof(string));
        //        //        inDataTable.Columns.Add("USE_FLAG", typeof(string));
        //        //        inDataTable.Columns.Add("UPDUSER", typeof(string));
        //        //        inDataTable.Columns.Add("DATASTATE", typeof(string));

        //        //        inDataTable.Columns.Add("ROUTID", typeof(string));
        //        //        inDataTable.Columns.Add("JUDG_PROG_PROCID", typeof(string));
        //        //        inDataTable.Columns.Add("SPEC_OUTPUT_STRT_RATE", typeof(string));
        //        //        inDataTable.Columns.Add("JUDG_TMP_STOP_FLAG", typeof(string));
        //        //        inDataTable.Columns.Add("SHIP_PROTECT_FLAG", typeof(string));

        //        //        DataTable dtRJudgOp = DataTableConverter.Convert(dgRJudgOp.ItemsSource);

        //        //        for (int iRowCnt = 0; iRowCnt < dtRJudgOp.Rows.Count; iRowCnt++)
        //        //        {
        //        //            DataRow param = inDataTable.NewRow();
        //        //            param["REPSTR_ID"] = sREPSTR_ID;
        //        //            param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //            //param["KEY_SEQ_NO"] = null;
        //        //            param["USE_FLAG"] = dtRJudgOp.Rows[iRowCnt]["USE_FLAG"];
        //        //            param["UPDUSER"] = LoginInfo.USERID;
        //        //            //param["DATASTATE"] = null;

        //        //            param["ROUTID"] = dtRJudgOp.Rows[iRowCnt]["ROUTID"];
        //        //            param["JUDG_PROG_PROCID"] = dtRJudgOp.Rows[iRowCnt]["JUDG_PROG_PROCID"];
        //        //            param["SPEC_OUTPUT_STRT_RATE"] = dtRJudgOp.Rows[iRowCnt]["SPEC_OUTPUT_STRT_RATE"];
        //        //            param["JUDG_TMP_STOP_FLAG"] = string.IsNullOrEmpty(dtRJudgOp.Rows[iRowCnt]["JUDG_TMP_STOP_FLAG"].SafeToString()) == true ? "N" : dtRJudgOp.Rows[iRowCnt]["JUDG_TMP_STOP_FLAG"];
        //        //            param["SHIP_PROTECT_FLAG"] = string.IsNullOrEmpty(dtRJudgOp.Rows[iRowCnt]["SHIP_PROTECT_FLAG"].SafeToString()) == true ? "N" : dtRJudgOp.Rows[iRowCnt]["SHIP_PROTECT_FLAG"];
        //        //            inDataTable.Rows.Add(param);
        //        //        }

        //        //        if (inDataTable.Rows.Count < 1)
        //        //        {
        //        //            Helper.ShowInfo("10008");
        //        //            return;
        //        //        }

        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //        new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //        {
        //        //            try
        //        //            {
        //        //                if (outException != null)
        //        //                {
        //        //                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //                    Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE_RJUDG_PROC", outException);
        //        //                    return;
        //        //                }
        //        //                else
        //        //                {
        //        //                    Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //                    selectRJudgProc();
        //        //                    selectGradeRJudgTree();
        //        //                }
        //        //            }
        //        //            catch (Exception ex)
        //        //            {
        //        //                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //                Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE_RJUDG_PROC", ex);
        //        //            }
        //        //            finally
        //        //            {
        //        //                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //                Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_ROUTE_RJUDG_PROC", Logger.MESSAGE_OPERATION_END);
        //        //            }
        //        //        }, FrameOperation.MENUID);
        //        //    }
        //        //});
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_FORM_ROUTE_RJUDG_PROC", ex);
        //    }
        //}

        private void dgRJudgOp_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgRJudgOp.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgRJudgOp.Columns["CHK"] 
            //     && e.Column != this.dgRJudgOp.Columns["USE_FLAG"]
            //     && e.Column != this.dgRJudgOp.Columns["SPEC_OUTPUT_STRT_RATE"]
            //     && e.Column != this.dgRJudgOp.Columns["JUDG_TMP_STOP_FLAG"]
            //     && e.Column != this.dgRJudgOp.Columns["SHIP_PROTECT_FLAG"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}
            //if (drv != null && (e.Column.Name.Equals("SPEC_OUTPUT_STRT_RATE") || e.Column.Name.Equals("SHIP_PROTECT_FLAG") || e.Column.Name.Equals("JUDG_TMP_STOP_FLAG")))
            //{
            //    if (drv != null && drv["USE_FLAG"].SafeToString() == "Y")
            //    {
            //        e.Cancel = false;
            //    }
            //    else
            //    {
            //        e.Cancel = true;
            //    }
            //}
        }

        private void dgRJudgOp_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {

        }

        private void dgRJudgOp_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgRJudgOp_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "JUDG_PROG_PROCNAME", "JUDG_PROCNAME", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgOp, Col);
            //}
            //else
            //{
            //    string[] Col = { "ROUTID", "JUDG_PROG_PROCNAME", "JUDG_PROCNAME", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgOp, Col);
            //}
        }

        private void dgRJudgOp_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgOp);
        }

        private void dgRJudgOp_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {

        }

        private void chkHeaderAllTp501_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgRJudgOp);
        }

        private void chkHeaderAllTp501_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgRJudgOp);
        }

        private void slRJudgRow01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slRJudgRow01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void tvRJudgMethod_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tvRJudgMethod_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvRJudgMethod_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void slRJudgCol01_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slRJudgCol01_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnExportTp502_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRJudgMethod);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnPlusTp502_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        int addRowCount = this.numAddCountTp502.Value.SafeToInt32();

        //        for (int i = 0; i < addRowCount; i++)
        //        {
        //            this.dgRJudgMethod.BeginNewRow();
        //            this.dgRJudgMethod.EndNewRow(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        //private void btnMinusTp502_Click(object sender, RoutedEventArgs e)
        //{
        //    int subRowCount = this.numAddCountTp502.Value.SafeToInt32();
        //    CommonUtil.MinusDataGridRow(dgRJudgMethod, subRowCount);
        //}

        //private void btnRJudgeMSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (!Common.CommonVerify.HasDataGridRow(dgRJudgMethod)) return;
        //        if (!CommonUtil.HasDataGridRow(dgRJudgMethod)) return;
        //        if (!Validation_RoutGrLotRJudgMthd()) return;

        //        this.dgRJudgMethod.EndEdit();
        //        this.dgRJudgMethod.EndEditRow(true);

        //        const string bizRuleName = "BR_MMD_REG_ITEM_MST_BY_GRID";
        //        const string sBAS_ITEM_ID = "TB_MMD_ROUT_GR_LOT_RJUDG_MTHD";

        //        string sREPSTR_ID = txtTp5AREAID.Tag.SafeToString();

        //        //DataTable inDataTable = _Com.GetBR_MMD_REG_ITEM_MST_BY_GRID();

        //        //foreach (object added in dgRJudgMethod.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(added, "JUDG_CASE_ID");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(added, "JUDG_MTHD_CODE");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(added, "REF_VALUE_CODE");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(added, "RJUDG_BAS_CODE");
        //        //        param["BAS_ATTR3"] = CommonUtil.GetCheckValue(added, "STDEV_MAX_CONST_VALUE");
        //        //        param["BAS_ATTR4"] = CommonUtil.GetCheckValue(added, "STDEV_MIN_CONST_VALUE");
        //        //        param["BAS_ATTR5"] = CommonUtil.GetCheckValue(added, "ABS_MAX_VALUE");
        //        //        param["BAS_ATTR6"] = CommonUtil.GetCheckValue(added, "ABS_MIN_VALUE");
        //        //        param["BAS_ATTR7"] = CommonUtil.GetCheckValue(added, "PCT_VALUE");
        //        //        param["BAS_ATTR8"] = CommonUtil.GetCheckValue(added, "BICELL_REG_VALUE");
        //        //        param["BAS_ATTR9"] = CommonUtil.GetCheckValue(added, "STDEV1_FIX_FLAG");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //foreach (object modified in dgRJudgMethod.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(modified, "JUDG_CASE_ID");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(modified, "JUDG_MTHD_CODE");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(modified, "REF_VALUE_CODE");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(modified, "RJUDG_BAS_CODE");
        //        //        param["BAS_ATTR3"] = CommonUtil.GetCheckValue(modified, "STDEV_MAX_CONST_VALUE");
        //        //        param["BAS_ATTR4"] = CommonUtil.GetCheckValue(modified, "STDEV_MIN_CONST_VALUE");
        //        //        param["BAS_ATTR5"] = CommonUtil.GetCheckValue(modified, "ABS_MAX_VALUE");
        //        //        param["BAS_ATTR6"] = CommonUtil.GetCheckValue(modified, "ABS_MIN_VALUE");
        //        //        param["BAS_ATTR7"] = CommonUtil.GetCheckValue(modified, "PCT_VALUE");
        //        //        param["BAS_ATTR8"] = CommonUtil.GetCheckValue(modified, "BICELL_REG_VALUE");
        //        //        param["BAS_ATTR9"] = CommonUtil.GetCheckValue(modified, "STDEV1_FIX_FLAG");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    Helper.ShowInfo("10008");
        //        //    return;
        //        //}

        //        //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //            selectRJudgMthd();
        //        //            selectRJugeGradeCaseId();
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //    }
        //}

        private void dgRJudgMethod_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgRJudgMethod.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgRJudgMethod.Columns["CHK"]
                 && e.Column != this.dgRJudgMethod.Columns["USE_FLAG"]
                 && e.Column != this.dgRJudgMethod.Columns["REF_VALUE_CODE"]
                 && e.Column != this.dgRJudgMethod.Columns["RJUDG_BAS_CODE"]
                 && e.Column != this.dgRJudgMethod.Columns["STDEV_MAX_CONST_VALUE"]
                 && e.Column != this.dgRJudgMethod.Columns["STDEV_MIN_CONST_VALUE"]
                 && e.Column != this.dgRJudgMethod.Columns["ABS_MAX_VALUE"]
                 && e.Column != this.dgRJudgMethod.Columns["ABS_MIN_VALUE"]
                 && e.Column != this.dgRJudgMethod.Columns["PCT_VALUE"]
                 && e.Column != this.dgRJudgMethod.Columns["BICELL_REG_VALUE"]
                 && e.Column != this.dgRJudgMethod.Columns["STDEV1_FIX_FLAG"])
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void dgRJudgMethod_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("ROUTID", txtTp5ROUTID.Tag);
            //e.Item.SetValue("JUDG_MTHD_CODE", txtJUDG_TYPE.Tag);
            //e.Item.SetValue("STDEV_MAX_CONST_VALUE", "0");
            //e.Item.SetValue("STDEV_MIN_CONST_VALUE", "0");
            //e.Item.SetValue("ABS_MAX_VALUE", "0.00");
            //e.Item.SetValue("ABS_MIN_VALUE", "0.00");
            //e.Item.SetValue("PCT_VALUE", "0");
            //e.Item.SetValue("BICELL_REG_VALUE", "0");
        }

        private void dgRJudgMethod_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                PopupFindControl pop = e.EditingElement as PopupFindControl;
                DataRowView drv = e.Row.DataItem as DataRowView;

                string sSelectedValue = drv[e.Column.Name].SafeToString();

                //if (cbo != null)
                //{
                //    if (Convert.ToString(e.Column.Name) == "JUDG_MTHD_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_MTHD_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "REF_VALUE_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'REF_VALUE_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "RJUDG_BAS_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'RJUDG_BAS_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "STDEV1_FIX_FLAG")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'STDEV1_FIX_FLAG' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgRJudgMethod_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "JUDG_CASE_ID", "JUDG_MTHD_CODE", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgMethod, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgMethod, Col);
            //}
        }

        private void dgRJudgMethod_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgMethod);
        }

        private void dgRJudgMethod_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {

        }

        private void chkHeaderAllTp502_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgRJudgMethod);
        }

        private void chkHeaderAllTp502_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgRJudgMethod);
        }

        private void slRJudgRow02_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slRJudgRow02_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        private void btnRjudgList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FORM001_ROUTE_MMD_RouteGradeRJudgSet wndRjudgList = new FORM001_ROUTE_MMD_RouteGradeRJudgSet();

                if (wndRjudgList != null)
                {
                    wndRjudgList.FrameOperation = this.FrameOperation;

                    object[] Parameters = new object[10];

                    Parameters[0] = Util.NVC(txtTp5AREAID.Tag);
                    Parameters[1] = Util.NVC(txtTp5AREAID.Text);

                    Parameters[2] = Util.NVC(txtTp5EQSGID.Tag);
                    Parameters[3] = Util.NVC(txtTp5EQSGID.Text);

                    Parameters[4] = Util.NVC(txtTp5MDLLOT_ID.Tag);
                    Parameters[5] = Util.NVC(txtTp5MDLLOT_ID.Text);

                    Parameters[6] = Util.NVC(txtTp5ROUTID.Tag);
                    Parameters[7] = Util.NVC(txtTp5ROUTID.Text);

                    Parameters[8] = Util.NVC(txtTp5ROUT_TYPE_CODE.Tag);
                    Parameters[9] = Util.NVC(txtTp5ROUT_TYPE_CODE.Text);

                    C1WindowExtension.SetParameters(wndRjudgList, Parameters);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndRjudgList.ShowModal()));
                    wndRjudgList.BringToFront();
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void tvRJudgGrade_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1TreeView tv = (C1TreeView)sender;

                C1TreeViewItem tvi = tv.GetNode(e.GetPosition(null));
                this.contextTreeItem_RJudge_Proc = tvi;

                if (tvi != null)
                {
                    DataRow data = tvi.DataContext as DataRow;

                    if (data["SORT_NO"].SafeToString() == "1") return;

                    if (data["PKEY_VAL"].SafeToString() == "000" && data["SORT_NO"].SafeToString() == "2")
                    {
                        this.contextMenu_Add_RJudge_Proc.IsOpen = true;
                        ContextMenu contextMenu = this.contextMenu_Add_RJudge_Proc;
                        Point position = e.GetPosition(null);
                        contextMenu.HorizontalOffset = position.X;
                        ContextMenu contextMenu1 = this.contextMenu_Add_RJudge_Proc;
                        position = e.GetPosition(null);
                        contextMenu1.VerticalOffset = position.Y;
                    }

                    if (data["PKEY_VAL"].SafeToString() != "000" && data["SORT_NO"].SafeToString() == "2")
                    {

                        this.contextMenu_Del_Judge_Proc.IsOpen = true;
                        ContextMenu contextMenu = this.contextMenu_Del_RJudge_Proc;
                        Point position = e.GetPosition(null);
                        contextMenu.HorizontalOffset = position.X;
                        ContextMenu contextMenu1 = this.contextMenu_Del_RJudge_Proc;
                        position = e.GetPosition(null);
                        contextMenu1.VerticalOffset = position.Y;
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void tvRJudgGrade_ItemExpanding(object sender, SourcedEventArgs e)
        {

        }

        private void tvRJudgGrade_ItemExpanded(object sender, SourcedEventArgs e)
        {

        }

        private void slRJudgCol02_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void slRJudgCol02_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {

        }

        //private void btnPlusTp503_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(txtTp5ROUTID.Tag.SafeToString())) return;
        //        if (string.IsNullOrEmpty(txtRJUDG_GRADE_CD.Tag.SafeToString())) return;

        //        if (((DataView)dgRJudgGrade.ItemsSource).ToTable().Rows.Count > 0)
        //            return;

        //        int addRowCount = 1;//this.numAddCountTp402.Value.SafeToInt32();

        //        for (int i = 0; i < addRowCount; i++)
        //        {
        //            this.dgRJudgGrade.BeginNewRow();
        //            this.dgRJudgGrade.EndNewRow(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
        //    }
        //}

        private void btnExportTp503_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRJudgGrade);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private void btnMinusTp503_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void btnRJudgeGSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (!Common.CommonVerify.HasDataGridRow(dgRJudgGrade)) return;
        //        if (!CommonUtil.HasDataGridRow(dgRJudgGrade)) return;
        //        //if (!Validation()) return;

        //        this.dgRJudgGrade.EndEdit();
        //        this.dgRJudgGrade.EndEditRow(true);

        //        const string bizRuleName = "BR_MMD_REG_ITEM_MST_BY_GRID";
        //        const string sBAS_ITEM_ID = "TB_MMD_ROUT_GRD_GR_LOT_RJUDG_SET";

        //        string sREPSTR_ID = txtTp5AREAID.Tag.SafeToString();

        //        //DataTable inDataTable = _Com.GetBR_MMD_REG_ITEM_MST_BY_GRID();

        //        //foreach (object added in dgRJudgGrade.GetAddedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(added, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Added);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(added, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(added, "SUBLOT_GRD_CODE");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(added, "GRD_ROW_NO");
        //        //        param["BAS_KEY_ATTR4"] = DataTableConverter.GetValue(added, "GRD_COL_NO");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(added, "JUDG_PRIORITY");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(added, "PROCID");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(added, "MEASR_TYPE_CODE");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(added, "JUDG_MTHD_CODE");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(added, "JUDG_CASE_ID");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //foreach (object modified in dgRJudgGrade.GetModifiedItems())
        //        //{
        //        //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
        //        //    {
        //        //        DataRow param = inDataTable.NewRow();
        //        //        param["REPSTR_ID"] = sREPSTR_ID;
        //        //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
        //        //        param["KEY_SEQ_NO"] = DataTableConverter.GetValue(modified, "KEY_SEQ_NO");
        //        //        param["USE_FLAG"] = DataTableConverter.GetValue(modified, "USE_FLAG");
        //        //        param["UPDUSER"] = LoginInfo.USERID;
        //        //        param["DATASTATE"] = Convert.ToString(CommonDataSet.TransactionType.Modified);

        //        //        param["BAS_KEY_ATTR1"] = DataTableConverter.GetValue(modified, "ROUTID");
        //        //        param["BAS_KEY_ATTR2"] = DataTableConverter.GetValue(modified, "SUBLOT_GRD_CODE");
        //        //        param["BAS_KEY_ATTR3"] = DataTableConverter.GetValue(modified, "GRD_ROW_NO");
        //        //        param["BAS_KEY_ATTR4"] = DataTableConverter.GetValue(modified, "GRD_COL_NO");
        //        //        param["BAS_ATTR1"] = DataTableConverter.GetValue(modified, "JUDG_PRIORITY");
        //        //        param["BAS_ATTR2"] = DataTableConverter.GetValue(modified, "PROCID");
        //        //        param["BAS_ATTR3"] = DataTableConverter.GetValue(modified, "MEASR_TYPE_CODE");
        //        //        param["BAS_ATTR4"] = DataTableConverter.GetValue(modified, "JUDG_MTHD_CODE");
        //        //        param["BAS_ATTR5"] = DataTableConverter.GetValue(modified, "JUDG_CASE_ID");
        //        //        inDataTable.Rows.Add(param);
        //        //    }
        //        //}

        //        //if (inDataTable.Rows.Count < 1)
        //        //{
        //        //    Helper.ShowInfo("10008");
        //        //    return;
        //        //}

        //        //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

        //        //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
        //        //{
        //        //    try
        //        //    {
        //        //        if (outException != null)
        //        //        {
        //        //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", outException);
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            Helper.ShowInfo("10004");  //저장이 완료되었습니다.
        //        //            selectRouteGradeLotRJudgSet();
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //        //    }
        //        //    finally
        //        //    {
        //        //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //        //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_ITEM_MST_BY_GRID", Logger.MESSAGE_OPERATION_END);
        //        //    }
        //        //}, FrameOperation.MENUID);
        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_ITEM_MST_BY_GRID", ex);
        //    }
        //}


        private void dgRJudgGrade_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            //DataRowView drv = e.Row.DataItem as DataRowView;

            //if (drv["CHK"].SafeToString() != R.TRUE && e.Column != this.dgRJudgGrade.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            //{
            //    e.Cancel = false;
            //}
            //else
            //{
            //    if (e.Column != this.dgRJudgGrade.Columns["CHK"]
            //     && e.Column != this.dgRJudgGrade.Columns["USE_FLAG"]
            //     && e.Column != this.dgRJudgGrade.Columns["JUDG_PRIORITY"]
            //     && e.Column != this.dgRJudgGrade.Columns["PROCID"]
            //     && e.Column != this.dgRJudgGrade.Columns["MEASR_TYPE_CODE"]
            //     && e.Column != this.dgRJudgGrade.Columns["JUDG_MTHD_CODE"]
            //     && e.Column != this.dgRJudgGrade.Columns["JUDG_CASE_ID"])
            //    {
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        e.Cancel = false;
            //    }
            //}
        }

        private void dgRJudgGrade_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            //e.Item.SetValue("CHK", R.TRUE);
            //e.Item.SetValue("USE_FLAG", R.Y);
            //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            //e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);

            //e.Item.SetValue("ROUTID", txtTp5ROUTID.Tag.SafeToString());
            //e.Item.SetValue("PROCID", txtRJUDG_OP.Tag.SafeToString());
            //e.Item.SetValue("SUBLOT_GRD_CODE", txtRJUDG_GRADE_CD.Tag.SafeToString());
            //e.Item.SetValue("JUDG_PRIORITY", "1");


            //DataTable dtRJudgGrade = DataTableConverter.Convert(dgRJudgGrade.ItemsSource);

            //if (dtRJudgGrade.Rows.Count > 0)
            //{
            //    return;

                //현재 복합판정 사용 안함 - 사용시 주석 해제 하고 위의 return; 삭제
                //if (rdoCjudgAnd.IsChecked == false && rdoCjudgOr.IsChecked == false)
                //{
                //    Helper.ShowInfo("MMD0124");  //AND/OR 조건을 선택해주세요.             
                //    return;
                //}
            //}

            //if (dtRJudgGrade.Rows.Count == 0)
            //{
            //    e.Item.SetValue("GRD_ROW_NO", "1");
            //    e.Item.SetValue("GRD_COL_NO", "1");
            //}
            //else
            //{
            //    string sGRD_ROW_NO = null;
            //    string sGRD_COL_NO = null;

            //    if (rdoCjudgAnd.IsChecked == true)
            //    {
            //        sGRD_ROW_NO = dtRJudgGrade.Rows[dtRJudgGrade.Rows.Count - 1]["GRD_ROW_NO"].SafeToString();
            //        sGRD_COL_NO = dtRJudgGrade.Rows[dtRJudgGrade.Rows.Count - 1]["GRD_COL_NO"].SafeToString();
            //        e.Item.SetValue("GRD_ROW_NO", sGRD_ROW_NO);
            //        e.Item.SetValue("GRD_COL_NO", Convert.ToInt16(sGRD_COL_NO) + 1);
            //    }
            //    else if (rdoCjudgOr.IsChecked == true)
            //    {
            //        sGRD_ROW_NO = dtRJudgGrade.Rows[dtRJudgGrade.Rows.Count - 1]["GRD_ROW_NO"].SafeToString();
            //        sGRD_COL_NO = dtRJudgGrade.Rows[dtRJudgGrade.Rows.Count - 1]["GRD_COL_NO"].SafeToString();
            //        e.Item.SetValue("GRD_ROW_NO", Convert.ToInt16(sGRD_ROW_NO) + 1);
            //        e.Item.SetValue("GRD_COL_NO", "1");
            //    }
            //}
        }

        private void dgRJudgGrade_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
                //C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
                //DataRowView drv = e.Row.DataItem as DataRowView;

                //string sSelectedValue = drv[e.Column.Name].SafeToString();

                //if (cbo != null)
                //{
                //    if (Convert.ToString(e.Column.Name) == "MEASR_TYPE_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'MEASR_TYPE_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "PROCID")
                //    {
                //        string sMEASR_TYPE_CODE = drv["MEASR_TYPE_CODE"].SafeToString();
                //        string sMEASR_TYPE_CODE_Filter = string.IsNullOrEmpty(sMEASR_TYPE_CODE) == true ? "" : string.Format(" AND MEASR_TYPE_CODE = '{0}'", sMEASR_TYPE_CODE);
                //        _dvPROC_UNIT.RowFilter = string.Format("USE_FLAG = 'Y' {0}", sMEASR_TYPE_CODE_Filter);
                //        CommonCombo.SetDtCommonCombo(_dvPROC_UNIT.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "JUDG_MTHD_CODE")
                //    {
                //        _dvCMCD.RowFilter = "CMCDTYPE = 'JUDG_MTHD_CODE' AND USE_FLAG = 'Y'";
                //        CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    if (Convert.ToString(e.Column.Name) == "JUDG_CASE_ID")
                //    {
                //        string sJUDG_MTHD_CODE = drv["JUDG_MTHD_CODE"].SafeToString();

                //        C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = this.dgRJudgGrade.Columns[e.Column.Name] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                //        DataView dvCaseId = cboColumn.ItemsSource as DataView;

                //        string sMEASR_TYPE_CODE_Filter = string.IsNullOrEmpty(sJUDG_MTHD_CODE) == true ? "" : string.Format(" AND JUDG_MTHD_CODE = '{0}'", sJUDG_MTHD_CODE);
                //        dvCaseId.RowFilter = string.Format("USE_FLAG = 'Y' {0}", sMEASR_TYPE_CODE_Filter);

                //        CommonCombo.SetDtCommonCombo(dvCaseId.ToTable(), cbo, CommonCombo.ComboStatus.None, sSelectedValue);
                //    }

                //    cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                //    {
                //        Dispatcher.BeginInvoke(new Action(() =>
                //        {
                //            this.UpdateRowView_RJudgGrade(e.Row, e.Column);
                //        }));

                //    };

                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void UpdateRowView_RJudgGrade(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "MEASR_TYPE_CODE")
                {
                    _manualCommit_RJudgGrade = true;
                    drv["PROCID"] = string.Empty;

                    this.dgRJudgGrade.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "JUDG_MTHD_CODE")
                {
                    _manualCommit_RJudgGrade = true;
                    drv["JUDG_CASE_ID"] = string.Empty;
                    this.dgRJudgGrade.EndEditRow(true);
                }
            }
            finally
            {
                _manualCommit_RJudgGrade = false;
            }
        }

        private void dgRJudgGrade_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (!((DataRowView)e.Cell.Row.DataItem).Row.RowState.ToString().Equals(CommonDataSet.TransactionType.Added.ToString()))
            //{
            //    string[] Col = { "ROUTID", "SUBLOT_GRD_CODE", "GRD_ROW_NO", "GRD_COL_NO", "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgGrade, Col);
            //}
            //else
            //{
            //    string[] Col = { "INSUSER", "INSDTTM", "UPDUSER", "UPDDTTM" };
            //    CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgGrade, Col);
            //}
        }

        private void dgRJudgGrade_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            CommonUtil.DataGridReadOnlyBackgroundColor(e, dgRJudgGrade);
        }

        private bool _manualCommit_RJudgGrade = false;
        private void dgRJudgGrade_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_manualCommit_RJudgGrade == false)
            {
                this.UpdateRowView_RJudgGrade(e.Row, e.Column);
            }
        }

        private void chkHeaderAllTp503_Checked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllChecked(dgRJudgGrade);
        }

        private void chkHeaderAllTp503_Unchecked(object sender, RoutedEventArgs e)
        {
            CommonUtil.DataGridCheckAllUnChecked(dgRJudgGrade);
        }

        private void tvRJudgMethod_Level1_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;
                DataRow data = tvi.DataContext as DataRow;

                txtJUDG_TYPE.Text = data["CMCDNAME"].SafeToString();
                txtJUDG_TYPE.Tag = data["CMCODE"].SafeToString();

                selectRJudgMthd();

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void cboUseFlagTp502_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectRJudgMthd();
        }

        private void tvGradeRJudg_Level1_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;
                DataRow data = tvi.DataContext as DataRow;

                txtRJUDG_OP.Text = data["NAME_VAL"].SafeToString();
                txtRJUDG_OP.Tag = data["KEY_VAL"].SafeToString();
                txtRJUDG_GRADE_CD.Text = "";
                txtRJUDG_GRADE_CD.Tag = "";

                Util.gridClear(dgRJudgGrade);
                SetDataGridCheckHeaderInitialize(dgRJudgGrade);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void tvGradeRJudg_Level2_Click(object sender, SourcedEventArgs e)
        {
            try
            {
                C1TreeViewItem tvi = (C1TreeViewItem)e.Source;

                DataRow parent = tvi.ParentItem.DataContext as DataRow;
                DataRow data = tvi.DataContext as DataRow;

                txtRJUDG_OP.Text = parent["NAME_VAL"].SafeToString();
                txtRJUDG_OP.Tag = parent["KEY_VAL"].SafeToString();
                txtRJUDG_GRADE_CD.Text = data["NAME_VAL"].SafeToString();
                txtRJUDG_GRADE_CD.Tag = data["KEY_VAL"].SafeToString();

                selectRouteGradeLotRJudgSet();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void cboUseFlagTp503_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectRouteGradeLotRJudgSet();
        }
        #endregion

        #region Mehod

        #region Mehod : 공통
        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            if (allColumn != null)
            {
                StackPanel allPanel = allColumn.Header as StackPanel;
                if (allPanel != null)
                {
                    CheckBox allCheck = allPanel.Children[0] as CheckBox;
                    if (allCheck.IsChecked == true)
                    {
                        allCheck.Unchecked -= this.chkHeaderAllTp101_Unchecked;
                        allCheck.IsChecked = false;
                        allCheck.Unchecked += this.chkHeaderAllTp101_Unchecked;
                    }
                }
            }
        }

        #region 선별위치선정

        //TapPage3 : 판정등급
        private void selectSublotGrade()
        {
            try
            {
                DataTable dtSubLotGrd = Get_SUBLOT_GRD_CBO(txtTp3AREAID.Tag.ToString(), txtTp3ROUTID.Tag.ToString());

                C1.WPF.DataGrid.DataGridComboBoxColumn column = null;
                column = this.dgLoc.Columns["SUBLOT_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                column.ItemsSource = DataTableConverter.Convert(dtSubLotGrd);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //TAB3 : 선별공정(하단), TAB4 : 단위공정(상단)
        private void selectProcRout(string sROUTID)
        {
            try
            {
                _dvPROC_ROUT = Get_FORM_ROUT_PROC_CBO(sROUTID, null, null, null, null).DefaultView;

                CommonCombo.SetDtCommonCombo(_dvPROC_ROUT.ToTable(), dgLoc.Columns["PROCID"], CommonCombo.ComboStatus.NONE);
                CommonCombo.SetDtCommonCombo(_dvPROC_ROUT.ToTable(), dgUjudg.Columns["PROCID"], CommonCombo.ComboStatus.NONE);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //Tpapage4 : 단위공정(하단), Tpapage5 : 단위공정(하단)
        private void selectProcUnit(string sROUTID)
        {
            try
            {
                _dvPROC_UNIT = Get_GRD_UNIT_JUDG_SPEC_PROC_CBO(sROUTID, null, null).DefaultView;

                CommonCombo.SetDtCommonCombo(_dvPROC_UNIT.ToTable(), dgCjudg.Columns["PROCID"], CommonCombo.ComboStatus.NONE);
                CommonCombo.SetDtCommonCombo(_dvPROC_UNIT.ToTable(), dgRJudgGrade.Columns["PROCID"], CommonCombo.ComboStatus.NONE);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //Tpapage4 : 판정등급(시작등급, 종료등급)
        private void selectGradeCJudg()
        {
            try
            {
                DataTable dtJugeGrade = Get_GRD_UNIT_JUDG_SPEC_GRD_CBO(txtTp4ROUTID.Tag.ToString(), null, null, null);

                C1.WPF.DataGrid.DataGridComboBoxColumn column1 = null;
                column1 = this.dgCjudg.Columns["MIN_UNIT_JUDG_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                column1.ItemsSource = DataTableConverter.Convert(dtJugeGrade);

                C1.WPF.DataGrid.DataGridComboBoxColumn column2 = null;
                column2 = this.dgCjudg.Columns["MAX_UNIT_JUDG_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                column2.ItemsSource = DataTableConverter.Convert(dtJugeGrade);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #endregion

        #region Mehod : TabPage01 > 기본경로관리
        private void SelectRoute(string sDEFAULT_CHECK = null, string sTEST_CHECK = null)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgRoute);
                Util.gridClear(dgTestRoute);

                SetDataGridCheckHeaderInitialize(dgRoute);
                SetDataGridCheckHeaderInitialize(dgTestRoute);

                const string bizRuleName = "MMD_SEL_FORM_ROUTE";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REPSTR_ID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable.Columns.Add("DEFAULT_CHECK", typeof(string));
                inDataTable.Columns.Add("TEST_CHECK", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["USERID"] = LoginInfo.USERID;
                inData["REPSTR_ID"] = _sTp1AREAID;
                inData["SHOPID"] = _sTp1SHOPID;
                inData["AREAID"] = _sTp1AREAID;
                inData["EQSGID"] = _sTp1EQSGID;
                inData["MDLLOT_ID"] = _sTp1MDLLOT_ID;
                inData["DEFAULT_CHECK"] = sDEFAULT_CHECK;
                inData["TEST_CHECK"] = sTEST_CHECK;
                inData["USE_FLAG"] = cboUseFlag.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    CommonUtil.DataConvertToBoolwithDataTable(result);

                    if (sDEFAULT_CHECK == "Y")
                    {
                        dgRoute.ItemsSource = DataTableConverter.Convert(result);
                        txtRowCntTp101.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);
                    }
                    else
                    {
                        dgTestRoute.ItemsSource = DataTableConverter.Convert(result);
                        txtRowCntTp102.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Route_Validation()
        {
            foreach (object added in dgRoute.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "AREAID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("AREAID")); //AREA를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("ROUTE_ID")); //ROUTID를 입력해 주세요.
                //        return false;
                //    }

                //    if (Util.NVC(DataTableConverter.GetValue(added, "ROUTID")).Length != 6)
                //    {
                //        CommonUtil.MessageInfo("MMD0155", CommonUtil.GetObjectName("ROUTE_ID"), 6);// ROUTID는 6자리 입니다.
                //        return false;
                //    }

                //    if (Util.NVC(DataTableConverter.GetValue(added, "AREAID")) != Util.NVC(DataTableConverter.GetValue(added, "ROUTID")).Substring(0, 2))
                //    {
                //        CommonUtil.MessageInfo("MMD0156", CommonUtil.GetObjectName("ROUTE_ID"), 2, CommonUtil.GetObjectName("AREAID"));// ROUTID는 앞 2자리가 동정보와 일치하지 않습니다.
                //        return false;
                //    }
                //}
            }

            foreach (object modified in dgRoute.GetModifiedItems())
            {
                //if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
                //{
                //    //*** USE_FLAG = 'N' 이면 TC_TRAY테이블 FIN_CD = 'C' 체크 존재하면 에러 : 삭제하고자 하는 ROUTE의 TRAY가 활성화에 존재합니다. 
                //    //return false;
                //}
            }

            return true;
        }

        private bool TestRoute_Validation()
        {
            foreach (object added in dgTestRoute.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "AREAID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("AREAID")); //AREA를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("ROUTID")); //ROUTID를 입력해 주세요.
                //        return false;
                //    }

                //    if (Util.NVC(DataTableConverter.GetValue(added, "ROUTID")).Length != 6)
                //    {
                //        CommonUtil.MessageInfo("MMD0155", CommonUtil.GetObjectName("ROUTE_ID"), 6);// ROUTID는 6자리 입니다.
                //        return false;
                //    }

                //    if (Util.NVC(DataTableConverter.GetValue(added, "AREAID")) != Util.NVC(DataTableConverter.GetValue(added, "ROUTID")).Substring(0, 2))
                //    {
                //        CommonUtil.MessageInfo("MMD0156", CommonUtil.GetObjectName("ROUTE_ID"), 2, CommonUtil.GetObjectName("AREAID"));// ROUTID는 앞 2자리가 동정보와 일치하지 않습니다.
                //        return false;
                //    }
                //}
            }

            foreach (object modified in dgTestRoute.GetModifiedItems())
            {
                //if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
                //{
                //    //*** USE_FLAG = 'N' 이면 TC_TRAY테이블 FIN_CD = 'C' 체크 존재하면 에러 : 삭제하고자 하는 ROUTE의 TRAY가 활성화에 존재합니다. 
                //    //return false;
                //}
            }

            return true;
        }




        #endregion

        #region Mehod : TapPage02 > 공정경로관리
        //공정경로관리 Tree
        private void SelectProcTree()
        {
            try
            {
                Get_PROC_TREE(null, (dt, ex) =>
                {
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    tvOp.Items.Clear();
                    DataRow[] level1 = dt.Select("SORT_NO = 1");

                    foreach (DataRow Row01 in level1)
                    {
                        C1TreeViewItem item01 = new C1TreeViewItem();
                        item01.Header = Row01["NAME_VAL"].SafeToString();
                        item01.DataContext = Row01;

                        item01.IsExpanded = true;
                        tvOp.Items.Add(item01);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //공정경로 관리 조회
        private void SelectRouteProc()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgRouteOper);
                _dtRoutOper.Clear();

                const string bizRuleName = "MMD_SEL_FORM_ROUTE_PROC";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp2ROUTID.Tag;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //CommonUtil.DataConvertToBoolwithDataTable(result);
                    //CommonUtil.DataConvertToBool(result, "STRT_PROC_FLAG");
                    //CommonUtil.DataConvertToBool(result, "END_PROC_FLAG");
                    //CommonUtil.DataConvertToBool(result, "SAS_TRNF_FLAG");

                    #region PROCID, LANE_ID 값 NAME 붙혀서 치환 
                    _dvCMCD_AREA.RowFilter = string.Format("CMCDTYPE = 'LANE_FOR_FORM_ROUT_PROC' AND AREAID = '{0}' AND USE_FLAG = 'Y' ", txtTp2AREAID.Tag);

                    foreach(DataRow dr1 in result.Rows)
                    {
                        foreach (DataRow dr2 in _dvCMCD_AREA.ToTable().Rows)
                        {
                            if (dr1["LANE_ID"].ToString() == dr2["CMCODE"].ToString())
                                dr1["LANE_ID"] = dr2["CMCDNAME1"].ToString();
                        }

                        foreach (DataRow dr2 in _dvPROC.ToTable().Rows)
                        {
                            if (dr1["PROCID"].ToString() == dr2["CMCODE"].ToString())
                                dr1["PROCID"] = string.Format("{0} : {1}", dr1["PROCID"].ToString(), dr2["CMCDNAME"].ToString());
                        }
                    }
                    #endregion

                    CommonUtil.DataConvertToBoolwithDataTable(result);

                    dgRouteOper.ItemsSource = DataTableConverter.Convert(result);
                    _dtRoutOper = DataTableConverter.Convert(dgRouteOper.ItemsSource);
                    txtRowCntTp201.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void Set_TextBox_Name(TextBox TXT, string sText, string sTag)
        {
            TXT.Text = sText;
            TXT.Tag = sTag;
        }

        private bool RouteOper_Validation()
        {
            DataTable dtRouterOper = DataTableConverter.Convert(dgRouteOper.ItemsSource);

            for (int i = 0; i < dtRouterOper.Rows.Count; i++)
            {
                //if (string.IsNullOrEmpty(dtRouterOper.Rows[i]["ROUTID"].ToString()) == true)
                //{
                //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //ROUTID를 선택해 주세요.
                //    return false;
                //}

                //if (string.IsNullOrEmpty(dtRouterOper.Rows[i]["PROCID"].ToString()) == true)
                //{
                //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("PROCID")); //PROCID를 선택해 주세요.
                //    return false;
                //}

                //if (dtRouterOper.Rows[i]["PROC_GR_CODE"].ToString() == "6" && string.IsNullOrEmpty(dtRouterOper.Rows[i]["EQPT_GR_DETL_TYPE_CODE"].ToString()) == true)
                //{
                //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("EQPT_GR_DETL_TYPE_CODE")); //설비 그룹 상세 유형 코드를 선택해 주세요.
                //    return false;
                //}
            }

            return true;
        }
        #endregion

        #region Mehod : TapPage03 > 작업조건관리

        private void SetRecipeColumn(string PROC_DETL_TYPE_CODE)
        {
            try
            {
                if (string.IsNullOrEmpty(PROC_DETL_TYPE_CODE))
                    return;

                #region 고정 칼럼 제외한 가변 칼럼 삭제, 버튼 칼럼 임시저장
                string[] FixedColumn = { "CHK", "USE_FLAG", "ROUTID", "PROCID", "PROC_STEP_NO" };
                string ButtonColumn = "EmptySlotList";

                C1.WPF.DataGrid.DataGridColumn temp_Col = null;

                for (int i = 0; i < dgRecipe.Columns.Count; i++)
                {
                    if (!dgRecipe.Columns[i].Name.ContainsValue(FixedColumn))
                    {
                        if (dgRecipe.Columns[i].Name.Equals(ButtonColumn)) //버튼 칼럼이면 임시 저장
                        {
                            temp_Col = dgRecipe.Columns[i];
                            temp_Col.Header = "MAX_TIME"; //헤더에 초기화
                            temp_Col.Visibility = Visibility.Collapsed;
                        }
                        dgRecipe.Columns.Remove(dgRecipe.Columns[i]); //가변 칼럼 삭제
                        i--;
                    }
                }

                #endregion

                #region 가변 칼럼 추가
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "MMD_SEL_ROUT_PROC_RECIPE_COL_INFO";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("RECIPE_PROC", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["RECIPE_PROC"] = PROC_DETL_TYPE_CODE;

                inDataTable.Rows.Add(inData);

                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

                if (result.Rows.Count > 0)
                {
                    dgRecipe.TopRows.Clear();
                    DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                    dgRecipe.TopRows.Add(HR); //칼럼 헤더 로우 1줄

                    foreach (DataRow dr in result.Rows)
                    {
                        string sColId = dr["RECIPE_ID"].ToString();
                        string sColName = (string.IsNullOrEmpty(dr["RECIPE_NAME"].ToString())) ? dr["RECIPE_ID"].ToString() : dr["RECIPE_NAME"].ToString();
                        string colAttr1 = dr["ATTR1"].ToString().Trim();  //DATA_TYPE: NUMBER - 최소값, COMBOBOX - 공통코드의 그룹코드, CHECKBOX - HeaderCheck(Y/N), MASK - Mask형식
                        string colAttr2 = dr["ATTR2"].ToString().Trim();  //DATA_TYPE: NUMBER - 최대값, COMBOBOX - 사용할코드(NULL일경우 전체), CHECKBOX - CheckValue(ex]Y|N, Null일경우 체크박스만 표시)
                        string colAttr3 = dr["ATTR3"].ToString().Trim();  //DATA_TYPE: NUMBER - 소수점아래 자리수
                        //string colAttr4 = dr["OPTION4"].ToString().Trim();

                        //if (sColId.Equals("PROC_STEP_NO"))
                        //{
                        //    //dgRecipe.GetColumn(drRslt["RECIPE_ID"].ToString()).Visible = true;
                        //}

                        switch (dr["COLTYPE"].ToString().Trim())
                        {
                            case "TEXT":
                                var column_TEXT = CommonUtil.CreateTextColumn(sColName, null, sColId, sColId, bReadOnly: true);
                                dgRecipe.Columns.Add(column_TEXT);
                                break;

                            case "NUMBER":

                                double dMin = 0;

                                    try
                                    {
                                        dMin = (string.IsNullOrEmpty(colAttr1)) ? 0 : Convert.ToDouble(colAttr1);
                                    } catch (Exception e)
                                    {
                                        dMin = (string.IsNullOrEmpty(colAttr1)) ? 0 : Convert.ToDouble(colAttr1, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                
                                double dMax = (string.IsNullOrEmpty(colAttr2)) ? 9999999999999.9999 : Convert.ToDouble(colAttr2);
                                int idemPlc = (string.IsNullOrEmpty(colAttr3)) ? 0 : Convert.ToInt32(colAttr3);

                                var column_NUMERIC = CommonUtil.CreateNumericColumn(sColName, null, sColId, sColId, bReadOnly: true, dMax: dMax, dMin: dMin, decimalPlaces: idemPlc);
                                dgRecipe.Columns.Add(column_NUMERIC);

                                if (sColId.Equals("END_TIME"))
                                {
                                    if (temp_Col != null)
                                    {
                                        temp_Col.Visibility = Visibility.Visible;
                                        dgRecipe.Columns.Add(temp_Col); //END_TIME 칼럼 뒤에 MAX_TIME 버튼칼럼 추가}
                                    }
                                }

                                break;

                            case "COMBOBOX":
                                string sDMPath = "CMCDNAME1";
                                string sSVPath = "CMCODE";

                                var column_COMBOBOX = CommonUtil.CreateComboBoxColumn(sColName, null, sColId, sColId, bReadOnly: true, sDisplayMemberPath: sDMPath, sSelectedValuePath: sSVPath);

                                if (colAttr1 == "BAS_PROCID")
                                {
                                    #region 기준 공정 아이디
                                    DataTable inDataTable2 = new DataTable();
                                    inDataTable2.Columns.Add("LANGID", typeof(string));
                                    inDataTable2.Columns.Add("ROUTID", typeof(string));
                                    inDataTable2.Columns.Add("USE_FLAG", typeof(string));

                                    DataRow inData2 = inDataTable2.NewRow();
                                    inData2["LANGID"] = LoginInfo.LANGID;
                                    inData2["ROUTID"] = txtTp3ROUTID.Tag;
                                    inData2["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp301.SelectedValue.ToString()) ? DBNull.Value : cboUseFlagTp301.SelectedValue;

                                    inDataTable2.Rows.Add(inData2);

                                    DataTable outResult2 = new ClientProxy().ExecuteServiceSync("MMD_SEL_RECIPE_BAS_PROC_CBO", "INDATA", "OUTDATA", inDataTable2);
                                    column_COMBOBOX.ItemsSource = DataTableConverter.Convert(outResult2);
                                    #endregion
                                }
                                else
                                {
                                    #region 공통 콤보박스 아이템 추가
                                    DataTable inDataTable2 = new DataTable();
                                    inDataTable2.Columns.Add("CMCDTYPE", typeof(string));
                                    inDataTable2.Columns.Add("CMCODE", typeof(string));
                                    inDataTable2.Columns.Add("LANGID", typeof(string));
                                    inDataTable2.Columns.Add("USE_FLAG", typeof(string));

                                    DataRow inData2 = inDataTable2.NewRow();
                                    inData2["CMCDTYPE"] = (string.IsNullOrEmpty(colAttr1)) ? null : colAttr1;
                                    inData2["CMCODE"] = (string.IsNullOrEmpty(colAttr2)) ? null : colAttr2;
                                    inData2["LANGID"] = LoginInfo.LANGID;
                                    inData2["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp301.SelectedValue.ToString()) ? DBNull.Value : cboUseFlagTp301.SelectedValue;

                                    inDataTable2.Rows.Add(inData2);

                                    DataTable outResult2 = new ClientProxy().ExecuteServiceSync("MMD_SEL_COMMONCODE", "INDATA", "OUTDATA", inDataTable2);
                                    column_COMBOBOX.ItemsSource = DataTableConverter.Convert(outResult2);
                                    #endregion
                                }

                                dgRecipe.Columns.Add(column_COMBOBOX);

                                break;

                            case "CHECKBOX":
                                var column_CHECKBOX = CommonUtil.CreateCheckBoxColumn(sColName, null, sColId, sColId, bReadOnly: true);
                                dgRecipe.Columns.Add(column_CHECKBOX);
                                break;

                            case "MASK":
                                var column_MASK = CommonUtil.CreateTextColumn(sColName, null, sColId, sColId, bReadOnly: true);
                                //dicMask.Add(sColId, new string[] { colAttr1, colAttr2, colAttr3 });
                                dgRecipe.Columns.Add(column_MASK);
                                break;
                        }
                    }

                    #region 버튼 칼럼 추가유무 확인
                    bool check = false; //버튼 칼럼 추가됐는지 확인
                    foreach (var col in dgRecipe.Columns)
                    {
                        if (col.Name.Equals(ButtonColumn))
                            check = true;
                    }
                    if (check == false) //추가 안됐으면 다음 칼럼생성할 때를 위해 숨김처리로 칼럼 추가
                    {
                        if (temp_Col != null)
                        {
                            temp_Col.Visibility = Visibility.Collapsed;
                            dgRecipe.Columns.Add(temp_Col);
                        }
                    }
                    #endregion

                    var column_INSUSER = CommonUtil.CreateTextColumn("INSUSER", null, "INSUSER", "INSUSER", bReadOnly: true, bEditable: false);
                    var column_INSDTTM = CommonUtil.CreateTextColumn("INSDTTM", null, "INSDTTM", "INSDTTM", bReadOnly: true, bEditable: false);
                    var column_UPDUSER = CommonUtil.CreateTextColumn("UPDUSER", null, "UPDUSER", "UPDUSER", bReadOnly: true, bEditable: false);
                    var column_UPDDTTM = CommonUtil.CreateTextColumn("UPDDTTM", null, "UPDDTTM", "UPDDTTM", bReadOnly: true, bEditable: false);
                    dgRecipe.Columns.Add(column_INSUSER);
                    dgRecipe.Columns.Add(column_INSDTTM);
                    dgRecipe.Columns.Add(column_UPDUSER);
                    dgRecipe.Columns.Add(column_UPDDTTM);
                }

                #endregion
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void selectRecipeTree()
        {
            try
            {
                Get_PROC_RECIPE_TREE(txtTp3ROUTID.Tag.SafeToString(), (dt, ex) =>
                {
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    tvRecipe.Items.Clear();
                    DataRow[] level1 = dt.Select();

                    foreach (DataRow Row01 in level1)
                    {
                        C1TreeViewItem item01 = new C1TreeViewItem();
                        item01.Header = Row01["NAME_VAL"].SafeToString();
                        item01.DataContext = Row01;
                        item01.Click += tvRecipe_Level1_Click;

                        item01.IsExpanded = true;
                        tvRecipe.Items.Add(item01);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectFormRoutProcRecipe()
        {
            try
            {
                if (string.IsNullOrEmpty(_sCurProcId_Recipe))
                    return;

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgRecipe);
                SetDataGridCheckHeaderInitialize(dgRecipe);

                const string bizRuleName = "MMD_SEL_FORM_ROUT_PROC_RECIPE";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp3ROUTID.Tag;
                inData["PROCID"] = _sCurProcId_Recipe;
                inData["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp301.SelectedValue.ToString()) ? DBNull.Value: cboUseFlagTp301.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgRecipe.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp301.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectGradSelecterLoadLocSet()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgLoc);
                SetDataGridCheckHeaderInitialize(dgLoc);

                const string bizRuleName = "MMD_SEL_GRD_SLCTR_PSTN_SET";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp3ROUTID.Tag;
                inData["USE_FLAG"] = string.IsNullOrEmpty(Util.NVC(cboUseFlagTp302.SelectedValue)) ? DBNull.Value : cboUseFlagTp302.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgLoc.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp302.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Validation_Recipe()
        {
            foreach (object added in dgRecipe.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //라우트를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PROCID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("PROCID")); //공정 선택해 주세요.
                //        return false;
                //    }

                //    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "SUBLOT_DFCT_LIMIT_USE_FLAG"))))
                //    //{
                //    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("SUBLOT_DFCT_LIMIT_USE_FLAG")); //SUBLOT 불량 한계 사용 여부를 선택해 주세요.
                //    //    return false;
                //    //}
                //}
            }

            foreach (object modified in dgRecipe.GetModifiedItems())
            {
                //if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
                //{
                //    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "SUBLOT_DFCT_LIMIT_USE_FLAG"))))
                //    //{
                //    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("SUBLOT_DFCT_LIMIT_USE_FLAG")); //SUBLOT 불량 한계 사용 여부를 선택해 주세요.
                //    //    return false;
                //    //}
                //}
            }

            //Aging Limit Time Validation Check
            if ((_sCurProcGr_Recipe.Equals("3") || _sCurProcGr_Recipe.Equals("4") || _sCurProcGr_Recipe.Equals("7")))
            {
                DataTable dtRecipe = DataTableConverter.Convert(dgRecipe.ItemsSource);
                foreach (DataRow dr in dtRecipe.Rows)
                {
                    if (Convert.ToInt32(dr["AGING_MIN_TIME"]) != 0 && Convert.ToInt32(dr["AGING_MAX_TIME"]) != 0)
                    {
                        //if (Convert.ToInt32(dr["END_TIME"]) + Convert.ToInt32(dr["LIMIT_TIME"]) < Convert.ToInt16(dr["AGING_MIN_TIME"]) || Convert.ToInt32(dr["END_TIME"]) + Convert.ToInt32(dr["LIMIT_TIME"]) > Convert.ToInt32(dr["AGING_MAX_TIME"]))
                        //{
                        //    Helper.ShowInfo("FM_ME_0016");  //Aging 상/하한 시간을 초과하였습니다.
                        //    return false;
                        //}

                        //if (Convert.ToInt32(dr["AGING_MIN_TIME"]) > Convert.ToInt32(dr["AGING_MAX_TIME"]))
                        //{
                        //    Helper.ShowInfo("FM_ME_0017");  //Aging 상한시간이 하한값 보다 작습니다. 재설정 해주세요.
                        //    return false;
                        //}
                    }
                }
            }

            return true;
        }

        private bool Validation_Loc()
        {
            foreach (object added in dgLoc.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //라우트를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "SUBLOT_GRD_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("SUBLOT_GRD_CODE")); //판증등급 코드 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PROCID"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("PROCID")); //선별공정을 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "GRD_SLCTR_LOCATION_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("GRD_SLCTR_LOCATION_CODE")); //선별위치를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "DFCT_GRD_FLAG"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("DFCT_GRD_FLAG")); //등급불량여부를 선택해 주세요.
                    //    return false;
                    //}
                //}
            }

            foreach (object modified in dgLoc.GetModifiedItems())
            {
                //if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "GRD_SLCTR_LOCATION_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("GRD_SLCTR_LOCATION_CODE")); //선별위치를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "DFCT_GRD_FLAG"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("DFCT_GRD_FLAG")); //등급불량여부를 선택해 주세요.
                //        return false;
                //    }
                //}
            }

            return true;
        }

        private void SaveWipAttrAgingIssSchdDttm(string sFormRoutProcRecipeEndTime)
        {
            try
            {
                //string sEND_TIME = sFormRoutProcRecipeEndTime;

                //if (String.IsNullOrEmpty(sEND_TIME))
                //{
                //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("END_TIME")); //종료 시간을 입력해 주세요.
                //    return;
                //}

                //const string bizRuleName = "BR_MMD_REG_WIPATTR_AGING_ISS_SCHD_DTTM";

                //DataTable inDataTable = new DataTable();

                //inDataTable.Columns.Add("AREAID", typeof(string));
                //inDataTable.Columns.Add("EQSGID", typeof(string));
                //inDataTable.Columns.Add("ROUTID", typeof(string));
                //inDataTable.Columns.Add("PROCID", typeof(string));
                //inDataTable.Columns.Add("END_TIME", typeof(Int32));
                //inDataTable.Columns.Add("UPDUSER", typeof(string));

                //DataRow param = inDataTable.NewRow();
                //param["AREAID"] = txtTp3AREAID.Tag;
                //param["EQSGID"] = txtTp3EQSGID.Tag;
                //param["ROUTID"] = txtTp3ROUTID.Tag;
                //param["PROCID"] = _sCurProcId_Recipe;
                //param["END_TIME"] = sEND_TIME.SafeToInt32();
                //param["UPDUSER"] = LoginInfo.USERID;

                //inDataTable.Rows.Add(param);

                //if (inDataTable.Rows.Count < 1)
                //{
                //    Helper.ShowInfo("10008");
                //    return;
                //}

                //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

                //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
                //{
                //    try
                //    {
                //        if (outException != null)
                //        {
                //            //ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //            Helper.ShowInfo("FM_ME_0055");  //Plan Time 반영에 실패하였습니다.
                //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_WIPATTR_AGING_ISS_SCHD_DTTM", outException);
                //            return;
                //        }
                //        else
                //        {
                //            Helper.ShowInfo("FM_ME_0056");  //Plan Time 반영이 완료하였습니다.
                //            SelectFormRoutProcRecipe();
                //            selectSublotGrade();
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_WIPATTR_AGING_ISS_SCHD_DTTM", ex);
                //    }
                //    finally
                //    {
                //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_WIPATTR_AGING_ISS_SCHD_DTTM", Logger.MESSAGE_OPERATION_END);
                //    }
                //}, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_WIPATTR_AGING_ISS_SCHD_DTTM", ex);
            }
        }
        #endregion

        #region Mehod : TapPage04 > 판정등급관리
        private void SelectGradeUnitJudgSpec()
        {
            try
            {
                if (string.IsNullOrEmpty(_sCurMeasrType))
                    return;

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgUjudg);
                SetDataGridCheckHeaderInitialize(dgUjudg);

                const string bizRuleName = "MMD_SEL_GRD_UNIT_JUDG_SPEC";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));
                inDataTable.Columns.Add("MEASR_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp4ROUTID.Tag;
                inData["MEASR_TYPE_CODE"] = _sCurMeasrType;
                inData["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp401.SelectedValue.ToString()) ? DBNull.Value : cboUseFlagTp401.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgUjudg.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp401.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Validation_GradeUnitJudgSpac()
        {
            foreach (object added in dgUjudg.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //라우트를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PROCID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("PROCID")); //PROCID 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "MEASR_TYPE_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("MEASR_TYPE_CODE")); //측정 유형을 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "UNIT_JUDG_GRD_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("UNIT_JUDG_GRD_CODE")); //단위 판정 등급를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "MAX_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("MAX_VALUE")); //최대값를 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "MIN_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("MIN_VALUE")); //최소값를 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "RJUDG_MAX_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("RJUDG_MAX_VALUE")); //상대판정 최대값를 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "RJUDG_MIN_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("RJUDG_MIN_VALUE")); //상대판정 최소값를 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "RJUDG_ABS_MAX_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("RJUDG_ABS_MAX_VALUE")); //상대판정 절대 최대값 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "RJUDG_ABS_MIN_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("RJUDG_ABS_MIN_VALUE")); //상대판정 절대 최소값  입력해 주세요.
                //        return false;
                //    }

                //}
            }

            return true;
        }


        private void selectGradeMJudgTree()
        {
            Get_GRD_MJUDG_TREE(txtTp4AREAID.Tag.SafeToString(), txtTp4ROUTID.Tag.SafeToString(), (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                tvGrade.Items.Clear();
                DataRow[] level1 = dt.Select("SORT_NO = 1");

                foreach (DataRow Row01 in level1)
                {
                    C1TreeViewItem item01 = new C1TreeViewItem();
                    item01.Header = Row01["NAME_VAL"].SafeToString();
                    item01.DataContext = Row01;
                    item01.Click += tvGradeMJudg_Level1_Click;

                    DataRow[] level2 = dt.Select("PKEY_VAL = '" + Row01["KEY_VAL"].SafeToString() + "' AND SORT_NO = 2");

                    foreach (DataRow Row02 in level2)
                    {
                        C1TreeViewItem item02 = new C1TreeViewItem();
                        item02.Header = Row02["NAME_VAL"].SafeToString();
                        item02.DataContext = Row02;
                        item02.Click += tvGradeMJudg_Level2_Click;

                        item01.Items.Add(item02);
                    }

                    item01.IsExpanded = true;
                    tvGrade.Items.Add(item01);
                }
            });
        }

        private void SelectRouteGradeMJudgSet()
        {
            try
            {
                if (string.IsNullOrEmpty(txtJUDG_GRADE.Tag.SafeToString()))
                    return;

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgCjudg);
                SetDataGridCheckHeaderInitialize(dgCjudg);

                const string bizRuleName = "MMD_SEL_ROUT_GRD_MJUDG_SET";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));
                inDataTable.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp4ROUTID.Tag;
                inData["SUBLOT_GRD_CODE"] = txtJUDG_GRADE.Tag;
                inData["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp402.SelectedValue.ToString()) ? DBNull.Value : cboUseFlagTp402.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgCjudg.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp402.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Validation_Cjudg()
        {
            foreach (object added in dgCjudg.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //라우트를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "SUBLOT_GRD_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("판정등급")); //판증등급 코드 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "GRD_ROW_NO"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("열")); //열 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "GRD_COL_NO"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("연")); //연 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PROCID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("PROCID")); //선별공정을 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "MEASR_TYPE_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("MEAS_TYPE_CD")); //측정유형를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "MAX_UNIT_JUDG_GRD_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("END_GRADE")); //등급불량여부를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "MIN_UNIT_JUDG_GRD_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("START_GRADE")); //등급불량여부를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "JUDG_TYPE_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("JUDG_TYPE_CODE")); //등급불량여부를 선택해 주세요.
                //        return false;
                //    }
                //}
            }

            //foreach (object modified in dgCjudg.GetModifiedItems())
            //{
            //    if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
            //    {
                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "PROCID"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("PROCID")); //선별공정을 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "MEASR_TYPE_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("MEAS_TYPE_CD")); //측정유형를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "MAX_UNIT_JUDG_GRD_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("END_GRADE")); //등급불량여부를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "MIN_UNIT_JUDG_GRD_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("START_GRADE")); //등급불량여부를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "JUDG_TYPE_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("JUDG_TYPE_CODE")); //등급불량여부를 선택해 주세요.
                    //    return false;
                    //}
            //    }
            //}

            return true;
        }

        private void SaveFittedJudgGrdSpec(string sBAS_ITEM_ID, DataTable SourceDataTable)
        {
            try
            {
                //const string bizRuleName = "BR_MMD_REG_FORM_FITTED_JUDG_GRD_SPEC";
                //string sREPSTR_ID = txtTp4AREAID.Tag.SafeToString();

                //DataTable inDataTable = new DataTable();

                //inDataTable.Columns.Add("REPSTR_ID", typeof(string));
                //inDataTable.Columns.Add("BAS_ITEM_ID", typeof(string));
                //inDataTable.Columns.Add("ROUTID", typeof(string));
                //inDataTable.Columns.Add("PROCID", typeof(string));
                //inDataTable.Columns.Add("MEASR_TYPE_CODE", typeof(string));
                //inDataTable.Columns.Add("UNIT_JUDG_GRD_CODE", typeof(string));
                //inDataTable.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                //inDataTable.Columns.Add("GRD_ROW_NO", typeof(string));
                //inDataTable.Columns.Add("GRD_COL_NO", typeof(string));
                //inDataTable.Columns.Add("UPDUSER", typeof(string));

                //if (sBAS_ITEM_ID == "TB_MMD_GRD_UNIT_JUDG_SPEC")
                //{
                //    for (int iRowCnt = 0; iRowCnt < SourceDataTable.Rows.Count; iRowCnt++)
                //    {
                //        DataRow param = inDataTable.NewRow();
                //        param["REPSTR_ID"] = sREPSTR_ID;
                //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
                //        param["ROUTID"] = SourceDataTable.Rows[iRowCnt]["ROUTID"].ToString();
                //        param["PROCID"] = SourceDataTable.Rows[iRowCnt]["PROCID"].ToString();
                //        param["MEASR_TYPE_CODE"] = SourceDataTable.Rows[iRowCnt]["MEASR_TYPE_CODE"].ToString();
                //        param["UNIT_JUDG_GRD_CODE"] = SourceDataTable.Rows[iRowCnt]["UNIT_JUDG_GRD_CODE"].ToString();
                //        param["UPDUSER"] = LoginInfo.USERID;

                //        inDataTable.Rows.Add(param);
                //    }
                //}

                //if (sBAS_ITEM_ID == "TB_MMD_ROUT_GRD_MJUDG_SET")
                //{
                //    for (int iRowCnt = 0; iRowCnt < SourceDataTable.Rows.Count; iRowCnt++)
                //    {
                //        DataRow param = inDataTable.NewRow();
                //        param["REPSTR_ID"] = sREPSTR_ID;
                //        param["BAS_ITEM_ID"] = sBAS_ITEM_ID;
                //        param["ROUTID"] = SourceDataTable.Rows[iRowCnt]["ROUTID"].ToString();
                //        param["SUBLOT_GRD_CODE"] = SourceDataTable.Rows[iRowCnt]["SUBLOT_GRD_CODE"].ToString();
                //        param["GRD_ROW_NO"] = SourceDataTable.Rows[iRowCnt]["GRD_ROW_NO"].ToString();
                //        param["GRD_COL_NO"] = SourceDataTable.Rows[iRowCnt]["GRD_COL_NO"].ToString();
                //        param["UPDUSER"] = LoginInfo.USERID;

                //        inDataTable.Rows.Add(param);
                //    }
                //}

                //if (inDataTable.Rows.Count < 1)
                //{
                //    Helper.ShowInfo("10008");
                //    return;
                //}

                //loadingIndicator.Visibility = System.Windows.Visibility.Visible;

                //new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (outResult, outException) =>
                //{
                //    try
                //    {
                //        if (outException != null)
                //        {
                //            ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(outException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //            Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_FITTED_JUDG_GRD_SPEC", outException);
                //            return;
                //        }
                //        else
                //        {
                //            Helper.ShowInfo("10004");  //저장이 완료되었습니다.

                //            if (sBAS_ITEM_ID == "TB_MMD_GRD_UNIT_JUDG_SPEC")
                //            {
                //                SelectGradeUnitJudgSpec();
                //            }
                //            {
                //                SelectRouteGradeMJudgSet();
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_FITTED_JUDG_GRD_SPEC", ex);
                //    }
                //    finally
                //    {
                //        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                //        Logger.Instance.WriteLine(Logger.OPERATION_C + "BR_MMD_REG_FORM_FITTED_JUDG_GRD_SPEC", Logger.MESSAGE_OPERATION_END);
                //    }
                //}, FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_U + "BR_MMD_REG_FORM_FITTED_JUDG_GRD_SPEC", ex);
            }
        }
        #region Copy & paste
        private void dgUjudg_KeyDown(object sender, KeyEventArgs e)
        {
            //try
            //{
            //    if (e.Key == Key.V && KeyboardUtil.Ctrl)
            //    {
            //        foreach (C1.WPF.DataGrid.DataGridCell cell in dgUjudg.Selection.SelectedCells)
            //        {
            //            if (cell.Column.Name.SafeToString() == "ROUTID" || cell.Column.Name.SafeToString() == "PROCID" || cell.Column.Name.SafeToString() == "MEASR_TYPE_CODE" || cell.Column.Name.SafeToString() == "UNIT_JUDG_GRD_CODE")
            //                return;
            //        }

            //        var clipboard = System.Windows.Clipboard.GetText();
            //        var values = GetValues(clipboard);
            //        int x = 0, y = 0;

            //        //Paste data to C1DataGrid  
            //        if (sender is C1.WPF.DataGrid.C1DataGrid)
            //        {
            //            var _grid = sender as C1.WPF.DataGrid.C1DataGrid;
            //            var table = ((_grid.ItemsSource) as DataView).Table as DataTable;
            //            if (_grid.Selection.SelectedCells.Count > 0)
            //            {
            //                var firstSelectedCell = _grid.Selection.SelectedCells[0];
            //                x = firstSelectedCell.Column.Index;
            //                y = firstSelectedCell.Row.Index;
            //            }

            //            for (int i = 0; i < values.GetLength(0); i++)
            //            {
            //                for (int j = 0; j < values.GetLength(1); j++)
            //                {
            //                    table.Rows[y + i][x + j] = values[i, j];
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private static string[,] GetValues(string clipboard)
        {
            var rows = clipboard.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var columns = rows[0].Split(new string[1] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            string[,] result = new string[rows.Length, columns.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                columns = rows[i].Split(new string[1] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < columns.Length; j++)
                {
                    result[i, j] = columns[j];
                }
            }
            return result;
        }
        #endregion

        #endregion

        #region Mehod : TapPage05 > 상대판정 등급관리
        //Route Group Lot 상대판정 공정 조회
        private void selectRJudgProc()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgRouteOper);
                SetDataGridCheckHeaderInitialize(dgRouteOper);

                const string bizRuleName = "MMD_SEL_ROUT_GR_LOT_RJUDG_PROC";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = txtTp5AREAID.Tag;
                inData["ROUTID"] = txtTp5ROUTID.Tag;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    CommonUtil.DataConvertToBoolwithDataTable(result);

                    dgRJudgOp.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp501.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //private bool Validation_RoutGrLotRJudgProc()
        //{
            //DataTable dtRJudgOp = DataTableConverter.Convert(dgRJudgOp.ItemsSource);

            //for (int iRowCnt = 0; iRowCnt < dtRJudgOp.Rows.Count; iRowCnt++)
            //{
            //    if (dtRJudgOp.Rows[iRowCnt]["USE_FLAG"].ToString() == "Y")
            //    {
            //        if (String.IsNullOrEmpty(dtRJudgOp.Rows[iRowCnt]["ROUTID"].ToString()) == true)
            //        {
            //            CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //라우트를 선택해 주세요.
            //            return false;
            //        }

            //        if (String.IsNullOrEmpty(dtRJudgOp.Rows[iRowCnt]["JUDG_PROG_PROCID"].ToString()) == true)
            //        {
            //            CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ACT_OP")); //판정시기을 선택해 주세요.
            //            return false;
            //        }

            //        //if (String.IsNullOrEmpty(dtRJudgOp.Rows[iRowCnt]["JUDG_PROCID"].ToString()) == true)
            //        //{
            //        //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("JUDG_OP")); //판정공정을 선택해 주세요.
            //        //    return false;
            //        //}

            //        if (String.IsNullOrEmpty(dtRJudgOp.Rows[iRowCnt]["SPEC_OUTPUT_STRT_RATE"].ToString()) == true)
            //        {
            //            CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("WORK_PER2")); //진행분위(%)을 입력해 주세요.
            //            return false;
            //        }
            //    }
            //}
            //return true;
        //}

        private void selectRJudgMthd()
        {
            try
            {
                if (string.IsNullOrEmpty(txtJUDG_TYPE.Tag.SafeToString()))
                    return;

                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgRJudgMethod);
                SetDataGridCheckHeaderInitialize(dgRJudgMethod);

                const string bizRuleName = "MMD_SEL_ROUT_GR_LOT_RJUDG_MTHD";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));
                inDataTable.Columns.Add("JUDG_MTHD_CODE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp5ROUTID.Tag;
                inData["JUDG_MTHD_CODE"] = txtJUDG_TYPE.Tag;
                inData["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp502.SelectedValue.ToString()) ? DBNull.Value : cboUseFlagTp502.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgRJudgMethod.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp502.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool Validation_RoutGrLotRJudgMthd()
        {
            foreach (object added in dgRJudgMethod.GetAddedItems())
            {
                //if (DataTableConverter.GetValue(added, "CHK").Equals(R.TRUE))
                //{
                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ROUTID"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("ROUTID")); //라우트를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "JUDG_CASE_ID"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("JUDG_CASE_ID")); //판정 사례 아이디을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "JUDG_MTHD_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("JUDG_MTHD_CODE")); //판정방법코드를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "REF_VALUE_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("REF_VALUE_CODE")); //참조 값 코드를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "RJUDG_BAS_CODE"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("RJUDG_BAS_CODE")); //상대판정 기준 코드를 선택해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "STDEV_MAX_CONST_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("STDEV_MAX_CONST_VALUE")); //표준편차 최대 상수값을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "STDEV_MIN_CONST_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("STDEV_MIN_CONST_VALUE")); //표준편차 최소 상수값을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ABS_MAX_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("ABS_MAX_VALUE")); //절대 최대 값을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "ABS_MIN_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("ABS_MIN_VALUE")); //절대 최소 값을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "PCT_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("PCT_VALUE")); //백분율값을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "BICELL_REG_VALUE"))))
                //    {
                //        CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("BICELL_REG_VALUE")); //바이셀 등록값을 입력해 주세요.
                //        return false;
                //    }

                //    if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(added, "STDEV1_FIX_FLAG"))))
                //    {
                //        CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("STDEV1_FIX_FLAG")); //표준편자 고정값 여부를 선택해 주세요.
                //        return false;
                //    }
                //}
            }

            foreach (object modified in dgRJudgMethod.GetModifiedItems())
            {
                //if (DataTableConverter.GetValue(modified, "CHK").Equals(R.TRUE))
                //{
                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "REF_VALUE_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("REF_VALUE_CODE")); //참조 값 코드를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "RJUDG_BAS_CODE"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("RJUDG_BAS_CODE")); //상대판정 기준 코드를 선택해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "STDEV_MAX_CONST_VALUE"))))
                    //{
                    //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("STDEV_MAX_CONST_VALUE")); //표준편차 최대 상수값을 입력해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "STDEV_MIN_CONST_VALUE"))))
                    //{
                    //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("STDEV_MIN_CONST_VALUE")); //표준편차 최소 상수값을 입력해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "ABS_MAX_VALUE"))))
                    //{
                    //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("ABS_MAX_VALUE")); //절대 최대 값을 입력해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "ABS_MIN_VALUE"))))
                    //{
                    //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("ABS_MIN_VALUE")); //절대 최소 값을 입력해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "PCT_VALUE"))))
                    //{
                    //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("PCT_VALUE")); //백분율값을 입력해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "BICELL_REG_VALUE"))))
                    //{
                    //    CommonUtil.MessageInfo("10013", CommonUtil.GetObjectName("BICELL_REG_VALUE")); //바이셀 등록값을 입력해 주세요.
                    //    return false;
                    //}

                    //if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(modified, "STDEV1_FIX_FLAG"))))
                    //{
                    //    CommonUtil.MessageInfo("10012", CommonUtil.GetObjectName("STDEV1_FIX_FLAG")); //표준편자 고정값 여부를 선택해 주세요.
                    //    return false;
                    //}
                //}
            }

            return true;
        }


        private void selectGradeRJudgTree()
        {
            Get_GRD_RJUDG_TREE(txtTp5AREAID.Tag.SafeToString(), txtTp5ROUTID.Tag.SafeToString(), (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                tvRJudgGrade.Items.Clear();
                DataRow[] level1 = dt.Select("SORT_NO = 1");

                foreach (DataRow Row01 in level1)
                {
                    C1TreeViewItem item01 = new C1TreeViewItem();
                    item01.Header = Row01["NAME_VAL"].SafeToString();
                    item01.DataContext = Row01;
                    item01.Click += tvGradeRJudg_Level1_Click;

                    DataRow[] level2 = dt.Select("PKEY_VAL = '" + Row01["KEY_VAL"].SafeToString() + "' AND SORT_NO = 2");

                    foreach (DataRow Row02 in level2)
                    {
                        C1TreeViewItem item02 = new C1TreeViewItem();
                        item02.Header = Row02["NAME_VAL"].SafeToString();
                        item02.DataContext = Row02;
                        item02.Click += tvGradeRJudg_Level2_Click;

                        item01.Items.Add(item02);


                    }
                    item01.IsExpanded = true;
                    tvRJudgGrade.Items.Add(item01);
                }
            });
        }

        private void selectRouteGradeLotRJudgSet()
        {
            try
            {
                if (string.IsNullOrEmpty(txtRJUDG_GRADE_CD.Tag.SafeToString()))
                    return;

                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgRJudgGrade);
                SetDataGridCheckHeaderInitialize(dgRJudgGrade);

                const string bizRuleName = "MMD_SEL_ROUT_GRD_GR_LOT_RJUDG_SET";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ROUTID", typeof(string));
                inDataTable.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                inDataTable.Columns.Add("GRD_ROW_NO", typeof(string));
                inDataTable.Columns.Add("GRD_COL_NO", typeof(string));
                inDataTable.Columns.Add("MEASR_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("JUDG_CASE_ID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["ROUTID"] = txtTp5ROUTID.Tag;
                inData["SUBLOT_GRD_CODE"] = txtRJUDG_GRADE_CD.Tag;
                inData["USE_FLAG"] = string.IsNullOrEmpty(cboUseFlagTp503.SelectedValue.ToString()) ? DBNull.Value : cboUseFlagTp503.SelectedValue;

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    CommonUtil.DataConvertToBoolwithDataTable(result, new List<string> { "JUDG_MTHD_CODE" }); // JUDG_MTHD_CODE 컬럼 제외

                    dgRJudgGrade.ItemsSource = DataTableConverter.Convert(result);
                    txtRowCntTp503.Text = MessageDic.Instance.GetMessage("MMD0001", result.Rows.Count);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //[양산Route] 재작업 등급 선택
        private void btnRwkAvailGrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!FrameOperation.AUTHORITY.Equals("W"))
                //    return;

                //재작업 가용 등급 선택
                _iRowIndex_Route = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                dgRoute.SelectedIndex = _iRowIndex_Route;

                if (_iRowIndex_Route == -1)
                    return;

                //if (DataTableConverter.GetValue(dgRoute.Rows[_iRowIndex_Route].DataItem, "CHK").SafeToBoolean() == false) return;

                //Route Type가 디가스전 재작업 Route인 경우 선택.
                string sROUT_TYPE_CODE = DataTableConverter.GetValue(dgRoute.Rows[_iRowIndex_Route].DataItem, "ROUT_TYPE_CODE").SafeToString();
                string sROUT_RSLT_GR_CODE = DataTableConverter.GetValue(dgRoute.Rows[_iRowIndex_Route].DataItem, "ROUT_RSLT_GR_CODE").SafeToString();

                _dvCMCD.RowFilter = "CMCDTYPE = 'ROUT_RSLT_GR_CODE' AND CMCODE = '" + sROUT_RSLT_GR_CODE + "' AND USE_FLAG = 'Y'";

                //2022.05.11
                //if (sROUT_TYPE_CODE == "R" && _dvCMCD.ToTable().Rows.Count > 0 && _dvCMCD.ToTable().Rows[0]["ATTR1"].ToString() == "Y")
                if ((sROUT_TYPE_CODE == "R" || sROUT_TYPE_CODE == "K") && _dvCMCD.ToTable().Rows.Count > 0 && _dvCMCD.ToTable().Rows[0]["ATTR1"].ToString() == "Y")
                {
                    FORM001_ROUTE_MMD_RouteReworkGrade wndReworkGrade = new FORM001_ROUTE_MMD_RouteReworkGrade();

                    if (wndReworkGrade != null)
                    {
                        DataTable dtRoute = DataTableConverter.Convert(dgRoute.ItemsSource);
                        object[] Parameters = new object[5];
                        Parameters[0] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["AREAID"]);
                        Parameters[1] = _sTp1AREANAME;
                        Parameters[2] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["ROUTID"]);
                        Parameters[3] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["ROUT_NAME"]);
                        Parameters[4] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["RWK_AVAIL_GRD_CODE"]);

                        C1WindowExtension.SetParameters(wndReworkGrade, Parameters);

                        wndReworkGrade.Closed += new EventHandler(wndReworkGrade_Closed);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndReworkGrade.ShowModal()));
                        wndReworkGrade.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndReworkGrade_Closed(object sender, EventArgs e)
        {
            try
            {
                //RouteReworkGrade wndReworkGrade = sender as RouteReworkGrade;
                //if (wndReworkGrade.DialogResult == MessageBoxResult.OK)
                //{
                //    DataTableConverter.SetValue(dgRoute.Rows[_iRowIndex_Route].DataItem, "RWK_AVAIL_GRD_CODE", wndReworkGrade._pRWK_AVAIL_GRD_CODE);
                //    this.dgRoute.EndEdit();
                //    this.dgRoute.EndEditRow(true);
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //[양산 Route] SelectGrad 선택
        private void btnRoutSltGrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!FrameOperation.AUTHORITY.Equals("W")) return;

                //Selector 등급 선택
                _iRowIndex_Route = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                dgRoute.SelectedIndex = _iRowIndex_Route;

                if (_iRowIndex_Route == -1)
                    return;

                //if (DataTableConverter.GetValue(dgRoute.Rows[_iRowIndex_Route].DataItem, "CHK").SafeToBoolean() == false) return;

                FORM001_ROUTE_MMD_RouteSelectorGrade wndRoutSltGrade = new FORM001_ROUTE_MMD_RouteSelectorGrade();

                if (wndRoutSltGrade != null)
                {
                    DataTable dtRoute = DataTableConverter.Convert(dgRoute.ItemsSource);
                    object[] Parameters = new object[5];
                    Parameters[0] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["AREAID"]);
                    Parameters[1] = _sTp1AREANAME;
                    Parameters[2] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["ROUTID"]);
                    Parameters[3] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["ROUT_NAME"]);
                    Parameters[4] = Util.NVC(dtRoute.Rows[_iRowIndex_Route]["SLT_GRD"]);

                    C1WindowExtension.SetParameters(wndRoutSltGrade, Parameters);

                    wndRoutSltGrade.Closed += new EventHandler(wndRouteSltGrade_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndRoutSltGrade.ShowModal()));
                    wndRoutSltGrade.BringToFront();
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndRouteSltGrade_Closed(object sender, EventArgs e)
        {
            try
            {
                //RouteSelectorGrade wndRouteSltGrade = sender as RouteSelectorGrade;
                //if (wndRouteSltGrade.DialogResult == MessageBoxResult.OK)
                //{
                //    DataTableConverter.SetValue(dgRoute.Rows[_iRowIndex_Route].DataItem, "SLT_GRD", wndRouteSltGrade._pSLT_GRD_CODE);
                //    this.dgRoute.EndEdit();
                //    this.dgRoute.EndEditRow(true);
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //[Test Route] SelectGrad 선택
        private void btnTestRoutSltGrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!FrameOperation.AUTHORITY.Equals("W")) return;

                //Selector 등급 선택
                _iRowIndex_Route = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                dgTestRoute.SelectedIndex = _iRowIndex_Route;

                if (_iRowIndex_Route == -1)
                    return;

                //if (DataTableConverter.GetValue(dgTestRoute.Rows[_iRowIndex_Route].DataItem, "CHK").SafeToBoolean() == false) return;

                FORM001_ROUTE_MMD_RouteSelectorGrade wndTestRoutSltGrade = new FORM001_ROUTE_MMD_RouteSelectorGrade();

                if (wndTestRoutSltGrade != null)
                {
                    DataTable dtTestRoute = DataTableConverter.Convert(dgTestRoute.ItemsSource);
                    object[] Parameters = new object[5];
                    Parameters[0] = Util.NVC(dtTestRoute.Rows[_iRowIndex_Route]["AREAID"]);
                    Parameters[1] = _sTp1AREANAME;
                    Parameters[2] = Util.NVC(dtTestRoute.Rows[_iRowIndex_Route]["ROUTID"]);
                    Parameters[3] = Util.NVC(dtTestRoute.Rows[_iRowIndex_Route]["ROUT_NAME"]);
                    Parameters[4] = Util.NVC(dtTestRoute.Rows[_iRowIndex_Route]["SLT_GRD"]);

                    C1WindowExtension.SetParameters(wndTestRoutSltGrade, Parameters);

                    wndTestRoutSltGrade.Closed += new EventHandler(wndTestRouteSltGrade_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndTestRoutSltGrade.ShowModal()));
                    wndTestRoutSltGrade.BringToFront();
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndTestRouteSltGrade_Closed(object sender, EventArgs e)
        {
            try
            {
                //RouteSelectorGrade wndTestRouteSltGrade = sender as RouteSelectorGrade;
                //if (wndTestRouteSltGrade.DialogResult == MessageBoxResult.OK)
                //{
                //    DataTableConverter.SetValue(dgTestRoute.Rows[_iRowIndex_Route].DataItem, "SLT_GRD", wndTestRouteSltGrade._pSLT_GRD_CODE);
                //    this.dgTestRoute.EndEdit();
                //    this.dgTestRoute.EndEditRow(true);
                //}
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //Tpapage5 : Case Id
        private void selectRJugeGradeCaseId()
        {
            try
            {
                DataTable dtChoiceProc = Get_ROUT_GR_LOT_RJUDG_MTHD_CASEID_CBO(txtTp5ROUTID.Tag.ToString(), null, null);

                C1.WPF.DataGrid.DataGridComboBoxColumn column = null;
                column = this.dgRJudgGrade.Columns["JUDG_CASE_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                //CommonUtil.DataConvertToBoolwithDataTable(dtChoiceProc);
                column.ItemsSource = DataTableConverter.Convert(dtChoiceProc);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0003" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #endregion

        //private void SourceGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    DataGrid parent = (DataGrid)sender;
        //    dragSource = parent;
        //    object data = GetDataFromSourceGrid(dragSource, e.GetPosition(parent));

        //    if (data != null)
        //    {
        //        DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
        //    }
        //}
        //private static object GetDataFromSourceGrid(DataGrid source, Point point)
        //{
        //    UIElement element = source.InputHitTest(point) as UIElement;
        //    if (element != null)
        //    {
        //        object data = DependencyProperty.UnsetValue;
        //        while (data == DependencyProperty.UnsetValue)
        //        {
        //            data = source.ItemContainerGenerator.ItemFromContainer(element);

        //            if (data == DependencyProperty.UnsetValue)
        //            {
        //                element = VisualTreeHelper.GetParent(element) as UIElement;
        //            }

        //            if (element == source)
        //            {
        //                return null;
        //            }
        //        }

        //        if (data != DependencyProperty.UnsetValue)
        //        {
        //            return data;
        //        }
        //    }

        //    return null;
        //}

        //private void DestGrid_Drop(object sender, DragEventArgs e)
        //{
        //    DataGrid parent = (DataGrid)sender;
        //    object data = e.Data.GetData(typeof(string));
        //    ((IList)dragSource.ItemsSource).Remove(data);
        //    parent.Items.Add(data);
        //}


        #region SEL COMMONCODE(Multi)
        public void Get_MMD_SEL_CMCD_TYPE_MULT(string sCMCDTYPE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCMCDTYPE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("MMD_SEL_CMCD_TYPE_MULT", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region SEL SHOP BY USER
        public void Get_MMD_SEL_SHOP_WITH_USER(string sPLANT_ID, string sAREAID, string sARE_TYPE_CODE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("PLANT_ID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("AREA_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["PLANT_ID"] = sPLANT_ID;
            Indata["AREAID"] = sAREAID;
            Indata["AREA_TYPE_CODE"] = sARE_TYPE_CODE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_SHOP_WITH_USER", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region SEL AREA BY USER
        public void Get_MMD_SEL_AREA_WITH_USER(string sPLANT_ID, string sAREAID, string sAREA_TYPE_CODE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("PLANT_ID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("AREA_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["PLANT_ID"] = sPLANT_ID;
            Indata["AREAID"] = sAREAID;
            Indata["AREA_TYPE_CODE"] = sAREA_TYPE_CODE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_AREA_WITH_USER", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region COMMON - AREA Filter
        public String GetArea_FilterFormat(string sPlant, string sUseFlag)
        {
            string sPLANT_ID = string.IsNullOrEmpty(sPlant) == true ? "" : " AND PLANT_ID = '" + sPlant + "'";
            string sUSE_FLAG = string.IsNullOrEmpty(sUseFlag) == true ? "" : " AND USE_FLAG = '" + sUseFlag + "'";
            string sStringFormat = String.Format("1=1 {0}{1}", sPLANT_ID, sUSE_FLAG);
            return sStringFormat;
        }
        #endregion

        #region SEL EQUIPMENTSEGMENT BY USER
        public void Get_MMD_SEL_EQSG_WITH_USER(string sPLANT_ID, string sAREAID, string sEQSGID, string sAREA_TYPE_CODE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("PLANT_ID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("AREA_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["PLANT_ID"] = sPLANT_ID;
            Indata["AREAID"] = sAREAID;
            Indata["EQSGID"] = sEQSGID;
            Indata["AREA_TYPE_CODE"] = sAREA_TYPE_CODE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_EQSG_WITH_USER", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion 

        #region SEL EQUIPMENTSEGMENT BY USER : LINE_GROUP_CODE, MES_SYS_TYPE_CODE 추가
        public void Get_MMD_SEL_EQSG_WITH_USER(string sPLANT_ID, string sAREAID, string sEQSGID, string sAREA_TYPE_CODE, string sLINE_GROUP_CODE, string sMES_SYSTEM_TYPE_CODE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("PLANT_ID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("AREA_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("LINE_GROUP_CODE", typeof(string));
            IndataTable.Columns.Add("MES_SYSTEM_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["PLANT_ID"] = sPLANT_ID;
            Indata["AREAID"] = sAREAID;
            Indata["EQSGID"] = sEQSGID;
            Indata["AREA_TYPE_CODE"] = sAREA_TYPE_CODE;
            Indata["LINE_GROUP_CODE"] = sLINE_GROUP_CODE;
            Indata["MES_SYSTEM_TYPE_CODE"] = sMES_SYSTEM_TYPE_CODE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_EQSG_WITH_USER", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion 

        #region Get_FORM_MDL_CBO
        public void Get_FORM_MDL_CBO(string sSHOPID, string sAREAID, string sMDL_ID, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("MDL_ID", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["SHOPID"] = sSHOPID;
            Indata["AREAID"] = sAREAID;
            Indata["MDL_ID"] = sMDL_ID;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_FORM_MDL_CBO", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region SEL AREA COMMONCODE(Multi)
        public void Get_MMD_SEL_AREA_CMCD_TYPE_MULT(string asCMCDTYPE, string sAREAID, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["AREAID"] = sAREAID;
            Indata["CMCDTYPE"] = asCMCDTYPE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("MMD_SEL_AREA_CMCD_TYPE_MULT", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region COMMON PROCESS FilterFormat - PROCESSEQUIPMENTSEGMENT
        public DataTable GetProceqsg_FilterFormat(DataView dv, string sPlant, string sArea, string sEqsg, string sUseFlag)
        {
            if (dv == null) return null;

            string sPLANT_ID = string.IsNullOrEmpty(sPlant) == true ? "" : " AND PLANT_ID = '" + sPlant + "'";
            string sAREAID = string.IsNullOrEmpty(sArea) == true ? "" : " AND AREAID = '" + sArea + "'";
            string sEQSGID = string.IsNullOrEmpty(sEqsg) == true ? "" : " AND EQSGID = '" + sEqsg + "'";
            string sUSE_FLAG = string.IsNullOrEmpty(sUseFlag) == true ? "" : " AND USE_FLAG = '" + sUseFlag + "'";

            string sFilter = String.Format("1=1 {0}{1}{2}{3}", sPLANT_ID, sAREAID, sEQSGID, sUSE_FLAG);
            dv.RowFilter = sFilter;

            DataView dvPROCEQSG = GetCOMMON_CODE_DATAVIEW_UNION(dv, null).DefaultView.ToTable(true).DefaultView;
            dvPROCEQSG.Sort = "CMCODE";
            DataTable dtPROCEQSG = dvPROCEQSG.ToTable();

            return dtPROCEQSG;
        }
        #endregion

        #region COMMON CODE DATAVIEW UNION
        public DataTable GetCOMMON_CODE_DATAVIEW_UNION(DataView dv1, DataView dv2)
        {
            DataTable dt = GetCOMMON_CODE_TYPE();

            foreach (DataRowView row in dv1)
            {
                DataRow param = dt.NewRow();
                param["CMCODE"] = row["CMCODE"];
                param["CMCDNAME"] = row["CMCDNAME"];
                param["CMCDNAME1"] = row["CMCDNAME1"];
                param["USE_FLAG"] = row["USE_FLAG"];
                dt.Rows.Add(param);
            }

            if (dv2 != null)
            {
                foreach (DataRowView row in dv2)
                {
                    DataRow param = dt.NewRow();
                    param["CMCODE"] = row["CMCODE"];
                    param["CMCDNAME"] = row["CMCDNAME"];
                    param["CMCDNAME1"] = row["CMCDNAME1"];
                    param["USE_FLAG"] = row["USE_FLAG"];
                    dt.Rows.Add(param);
                }
            }

            return dt;
        }
        #endregion

        #region COMMON CODE COLUMNS TYPE
        public DataTable GetCOMMON_CODE_TYPE()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("CMCODE", typeof(string));
            inDataTable.Columns.Add("CMCDNAME", typeof(string));
            inDataTable.Columns.Add("CMCDNAME1", typeof(string));
            inDataTable.Columns.Add("CMCDNAME2", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));

            return inDataTable;
        }
        #endregion

        #region Get_FORM_PROC_CBO
        public void Get_PROC_CBO(string sPROC_GR_CODE, string sPROC_DETL_TYPE_CODE, string sNOT_IN_PROC_DETL_TYPE_CODE, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("PROC_GR_CODE", typeof(string));
            IndataTable.Columns.Add("PROC_DETL_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("NOT_IN_PROC_DETL_TYPE_CODE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["PROC_GR_CODE"] = sPROC_GR_CODE;
            Indata["PROC_DETL_TYPE_CODE"] = sPROC_DETL_TYPE_CODE;
            Indata["NOT_IN_PROC_DETL_TYPE_CODE"] = sNOT_IN_PROC_DETL_TYPE_CODE;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_PROC_CBO", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region Get_PROC_TREE
        public void Get_PROC_TREE(string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_PROC_TREE", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region Get_PROC_RECIPE_TREE
        public void Get_PROC_RECIPE_TREE(string sROUTID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["ROUTID"] = sROUTID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_PROC_RECIPE_TREE", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }
        #endregion

        #region Get_SUBLOT_GRD_CBO
        public DataTable Get_SUBLOT_GRD_CBO(string sAREAID, string sROUTID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["AREAID"] = sAREAID;
            Indata["ROUTID"] = sROUTID;

            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("MMD_SEL_SUBLOT_JUDG_CBO", "INDATA", "OUTDATA", IndataTable);

            return dtResult;

        }
        #endregion

        #region Get_FORM_ROUT_PROC_CBO
        public DataTable Get_FORM_ROUT_PROC_CBO(string sROUTID, string sPROCID, string sPROC_GR_CODE, string sPROC_DETL_TYPE_CODE, string sNOT_IN_PROCID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("PROC_GR_CODE", typeof(string));
            IndataTable.Columns.Add("PROC_DETL_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("NOT_IN_PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["ROUTID"] = sROUTID;
            Indata["PROCID"] = sPROCID;
            Indata["PROC_GR_CODE"] = sPROC_GR_CODE;
            Indata["PROC_DETL_TYPE_CODE"] = sPROC_DETL_TYPE_CODE;
            Indata["NOT_IN_PROCID"] = sNOT_IN_PROCID;

            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("MMD_SEL_FORM_ROUT_PROC_CBO", "INDATA", "OUTDATA", IndataTable);

            return dtResult;

        }
        #endregion

        #region Get_GRD_MJUDG_TREE
        public void Get_GRD_MJUDG_TREE(string sAREAID, string sROUTID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = sAREAID;
            Indata["ROUTID"] = sROUTID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_GRD_MJUDG_TREE", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        #endregion

        #region Get_GRD_UNIT_JUDG_SPEC_CBO
        public DataTable Get_GRD_UNIT_JUDG_SPEC_GRD_CBO(string sROUTID, string sPROCID, string sMEASR_TYPE_CODE, string sUSE_FLAG)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("MEASR_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));


            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["ROUTID"] = sROUTID;
            Indata["PROCID"] = sPROCID;
            Indata["MEASR_TYPE_CODE"] = sMEASR_TYPE_CODE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("MMD_SEL_GRD_UNIT_JUDG_SPEC_GRD_CBO", "INDATA", "OUTDATA", IndataTable);

            return dtResult;

        }
        #endregion

        #region Get_GRD_UNIT_JUDG_SPEC_CBO
        public DataTable Get_GRD_UNIT_JUDG_SPEC_PROC_CBO(string sROUTID, string sPROCID, string sUSE_FLAG)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));


            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["ROUTID"] = sROUTID;
            Indata["PROCID"] = sPROCID;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("MMD_SEL_GRD_UNIT_JUDG_SPEC_PROC_CBO", "INDATA", "OUTDATA", IndataTable);

            return dtResult;

        }
        #endregion

        #region Get_GRD_RJUDG_TREE
        public void Get_GRD_RJUDG_TREE(string sAREAID, string sROUTID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("ROUTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = sAREAID;
            Indata["ROUTID"] = sROUTID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("MMD_SEL_GRD_RJUDG_TREE", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        #endregion

        #region Get_GRD_UNIT_JUDG_SPEC_CBO
        public DataTable Get_ROUT_GR_LOT_RJUDG_MTHD_CASEID_CBO(string sROUTID, string sJUDG_MTHD_CODE, string sUSE_FLAG)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("ROUTID", typeof(string));
            IndataTable.Columns.Add("JUDG_MTHD_CODE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));


            DataRow Indata = IndataTable.NewRow();
            Indata["ROUTID"] = sROUTID;
            Indata["JUDG_MTHD_CODE"] = sJUDG_MTHD_CODE;
            Indata["USE_FLAG"] = sUSE_FLAG;

            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("MMD_SEL_ROUT_GR_LOT_RJUDG_MTHD_CASEID_CBO", "INDATA", "OUTDATA", IndataTable);

            return dtResult;

        }
        #endregion
    }

    public static class ObjectExtension
    {
        public static bool IsNullOrEmpty(this object item)
        {
            bool flag;
            try
            {
                if (item != null)
                {
                    flag = (!(item.ToString().Trim() == "") ? false : true);
                }
                else
                {
                    flag = true;
                }
            }
            catch (Exception exception)
            {
                flag = false;
            }
            return flag;
        }


        public static void Each<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var elem in list)
                action(elem);
        }

        public static bool ContainsAll<T>(this IList<T> list, IEnumerable<T> subList)
        {
            foreach (var t in subList)
                if (!list.Contains(t)) return false;
            return true;
        }

        public static T[] EmptyArray<T>()
        {
            return new T[] { };
        }



        public static T ChangeType<T>(this object value, IFormatProvider provider) where T : IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T), provider);
        }
        public static T ChangeType<T>(this object value) where T : IConvertible
        {
            return ChangeType<T>(value, System.Globalization.CultureInfo.CurrentCulture);
        }
        public static bool SafeToBoolean(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return false;
            }

            string s = obj.ToString().ToUpper();

            if (s.Equals("TRUE") || s.Equals("Y") || s.Equals("1"))
            {
                return true;
            }

            return false;
        }


        public static double SafeToDouble(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return 0;
            }


            return Convert.ToDouble(obj, System.Globalization.CultureInfo.CurrentCulture);
        }

        public static int SafeToInt32(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return 0;
            }

            return Convert.ToInt32(obj, System.Globalization.CultureInfo.CurrentCulture);
        }

        public static decimal SafeToDecimal(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return 0;
            }

            decimal val;
            if (decimal.TryParse(obj.ToString(), out val))
            {
                return val;
            }

            return 0;


            // return Convert.ToDecimal(obj, CultureInfo.CurrentCulture);
        }

        public static string SafeToString(this object obj, bool trim = false, bool removeSpecial = false)
        {
            string result = string.Empty;


            if (obj == null || obj == DBNull.Value)
            {
                return result;
            }

            result = obj.ToString();

            if (removeSpecial == true) result = System.Text.RegularExpressions.Regex.Replace(result, "[^a-zA-Z0-9%._]", string.Empty); // 공백은 제외 "[^a-zA-Z0-9%. _]"

            if (trim == true) result = result.Trim();

            return result;
        }

        public static string SafeFormat(string message, params object[] args)
        {
            if (args == null || args.Length == 0)
                return message;
            try
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args);
            }
            catch (Exception ex)
            {
                return message + " (System error: failed to format message. " + ex.Message + ")";
            }
        }

        public static void SetValue(this object obj, string property, object value)
        {
            if (obj != null)
            {
                if (obj is System.Data.DataRowView)
                {
                    System.Data.DataRowView drv = obj as System.Data.DataRowView;
                    drv[property] = value == null ? DBNull.Value : value;
                }
                else
                {
                    System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
                    if (propertyInfo != null)
                    {
                        try
                        {
                            obj.GetType().InvokeMember(propertyInfo.Name, System.Reflection.BindingFlags.SetProperty, null, obj, new object[] { value });
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        public static object GetValue(Dictionary<string, object> data, string key)
        {
            return data[key];
        }

        public static object GetValue(this object value, string name)
        {
            object obj;
            try
            {
                if (value == null)
                {
                    obj = "";
                }
                else
                {
                    if (value.GetType() == typeof(System.Data.DataRow))
                    {
                        obj = ((System.Data.DataRow)value)[name];

                    }
                    else if (value.GetType() == typeof(System.Data.DataRowView))
                    {
                        obj = ((System.Data.DataRowView)value)[name];
                    }
                    else
                    {
                        obj = value.GetType().GetProperty(name).GetValue(value, null);
                    }
                }

            }
            catch (Exception exception)
            {
                obj = null;
            }
            return obj;
        }

        public static bool ContainsValue(this string value, params string[] values)
        {
            foreach (string one in values)
            {
                if (value == one)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class CommonUtil
    {
        private static readonly ObjectDicConverter ObjectConverter = new ObjectDicConverter();

        public static void ExcelExport(C1DataGrid dg, List<int> collapsedcols = null, bool bVisiblecols = false)
        {

            if (collapsedcols == null && bVisiblecols == false)
            {
                collapsedcols = new List<int>() { 0 };

                if (dg.Resources.Contains("ExportRemove"))
                {
                    dg.Resources.Remove("ExportRemove");
                }

                dg.Resources.Add("ExportRemove", collapsedcols);
            }

            //new ExcelExporter().Export(dg);

            int iMultLangColCnt = 0;
            DataTable dtGrid = DataTableConverter.Convert(dg.ItemsSource);
            DataTable dtGridCopy = dtGrid.Copy();

            ////다국어 처리.
            for (int dgCol = 0; dgCol < dg.Columns.Count; dgCol++)
            {
                if (dg.Columns[dgCol] is LGC.GMES.MES.ControlsLibrary.DataGridMultiLangColumn)
                {
                    string sMultiColName = dg.Columns[dgCol].Name;

                    const string sDefaultLangId = "ko-KR";

                    for (int iRowCnt = 0; iRowCnt < dtGrid.Rows.Count; iRowCnt++)
                    {
                        string sMultLangValue = dtGrid.Rows[iRowCnt][sMultiColName].ToString();
                        string sDefaultLangValue = GetTextFromMultiLangText(sMultLangValue, sDefaultLangId);
                        string sLangValue = GetTextFromMultiLangText(sMultLangValue, LoginInfo.LANGID);

                        dtGrid.Rows[iRowCnt][sMultiColName] = string.IsNullOrEmpty(sLangValue) ? sDefaultLangValue : sLangValue;
                    }

                    iMultLangColCnt++;
                }
            }

            if (iMultLangColCnt > 0)
            {
                Util.gridClear(dg);
                dtGrid.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dtGrid);
                new ExcelExporter().Export(dg);

                Util.gridClear(dg);
                dtGridCopy.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dtGridCopy);
            }
            else
            {
                new ExcelExporter().Export(dg);
            }
        }

        public static void MinusDataGridRow(C1DataGrid dg, int iRowCnt)
        {
            try
            {
                for (int i = 0; i < iRowCnt; i++)
                {
                    DataRowView drv = dg.SelectedItem as DataRowView;
                    if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                    {
                        if (dg.SelectedIndex > -1)
                        {
                            dg.EndNewRow(true);
                            dg.RemoveRow(dg.SelectedIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static bool HasDataGridRow(C1DataGrid dg)
        {
            return dg.ItemsSource != null && dg.Rows.Count > 0;
        }

        public static void DataGridReadOnlyBackgroundColor(C1.WPF.DataGrid.DataGridCellEventArgs e, C1DataGrid dg, string[] Col)
        {
            try
            {
                if (e.Cell.Presenter != null && Col.Contains(e.Cell.Column.Name))
                {
                    //if (Col.Contains(e.Cell.Column.Name))
                    //{
                    foreach (string column in Col)
                    {
                        var bgColor = new BrushConverter();
                        e.Cell.Presenter.Background = (Brush)bgColor.ConvertFrom("#fff8f8f8");
                        break;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static void DataGridReadOnlyBackgroundColor(C1.WPF.DataGrid.DataGridCellEventArgs e, C1DataGrid dg)
        {
            try
            {
                if (e.Cell.Presenter != null)
                {
                    e.Cell.Presenter.Background = null;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static void DataGridCheckAllChecked(C1DataGrid dg, bool ischeckType = true)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (ischeckType)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "Y");
                }
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        public static void DataGridCheckAllUnChecked(C1DataGrid dg, bool ischeckType = true)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (ischeckType)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "N");
                }

            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        public static string multiPattern = string.Empty;

        public static string GetTextFromMultiLangText(string multiLangText, string langID)
        {
            string result = string.Empty;
            string defaultResult = string.Empty;

            if (multiPattern == string.Empty)
            {
                int check = 0;
                multiPattern = "(";
                for (int i = 0; i < LGC.GMES.MES.Common.LoginInfo.SUPPORTEDLANGLIST.Length; i++)
                {
                    string s = LGC.GMES.MES.Common.LoginInfo.SUPPORTEDLANGLIST[i];
                    if (multiLangText.IndexOf(s) >= 0) check++;

                    multiPattern += s;
                    multiPattern += @"\\";
                    if (i + 1 != LGC.GMES.MES.Common.LoginInfo.SUPPORTEDLANGLIST.Length)
                    {
                        multiPattern += @"|";
                    }
                }

                multiPattern += ")(.+)";
            }


            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(multiPattern);
            char[] chrArray = new char[] { '|' };
            string[] strArrays = multiLangText.Split(chrArray);
            bool matched = false;

            foreach (string str in strArrays)
            {
                System.Text.RegularExpressions.Match match = reg.Match(str);

                if (match.Success)
                {
                    matched = true;
                    string[] ss = str.Split(new char[] { '\\' });
                    if (langID == ss[0])
                    {
                        result = ss[1];
                    }
                    if (ss[0] == "ko-KR")
                    {
                        defaultResult = ss[1];
                    }
                }
            }

            if (matched == false)
            {
                return multiLangText;
            }

            if (result.Equals(string.Empty))
            {
                return defaultResult;
            }

            return result;


        }

        public static void DataConvertToBoolwithDataTable(DataTable dt)
        {
            DataConvertToBoolwithDataTable(dt, new List<string>{ string.Empty });
        }

        public static void DataConvertToBoolwithDataTable(DataTable dt, List<string> lstExceptColumn)
        {
            foreach (DataColumn dc in dt.Columns)
            {
                bool bFlagCol = false;

                foreach (DataRow dr in dt.Rows)
                {
                    if (dc.ColumnName.Trim().Equals("USE_FLAG"))
                        continue;

                    bool bPass = false;
                    foreach (string s in lstExceptColumn)
                    {
                        if (!string.IsNullOrEmpty(s) && (dc.ColumnName.Trim().Equals(s)))
                            bPass = true;
                    }
                    if (bPass)
                        continue;

                    if (dr[dc].ToString() == "Y" || dr[dc].ToString() == "N")
                    {
                        bFlagCol = true;
                    }
                }

                if(bFlagCol)
                    DataConvertToBool(dt, dc.ColumnName);
            }
        }

        public static void DataConvertToBool(DataTable dt, string colName)
        {
            foreach (DataRow dr in dt.Rows)
            {
                if (dr[colName].ToString() == "Y")
                    dr[colName] = "True";
                else if (dr[colName].ToString() == "N")
                    dr[colName] = "False";
            }
        }

        public static C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn
            (string Single_Header, List<string> Multi_Header, string sName, string sBinding, bool bReadOnly = false, bool bEditable = true, bool bVisible = true,
            HorizontalAlignment HorizonAlign = HorizontalAlignment.Center, VerticalAlignment VerticalAlign = VerticalAlignment.Center)
        {
            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Name = sName,
                Binding = new Binding(sBinding),
                IsReadOnly = bReadOnly,
                EditOnSelection = bEditable,
                Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed,
                HorizontalAlignment = HorizonAlign,
                VerticalAlignment = VerticalAlign
            };

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;

            else
                Col.Header = Multi_Header;

            return Col;
        }

        public static C1.WPF.DataGrid.DataGridNumericColumn CreateNumericColumn
            (string Single_Header, List<string> Multi_Header, string sName, string sBinding, bool bReadOnly = false, bool bEditable = true, bool bVisible = true,
            double dMax = 9999999999999.9999, double dMin = 0, int decimalPlaces = 0,
            HorizontalAlignment HorizonAlign = HorizontalAlignment.Right, VerticalAlignment VerticalAlign = VerticalAlignment.Center)
        {
            string sFormat = "#,##0";
            if (decimalPlaces > 0)
            {
                sFormat += ".";
                for (int i = 0; i < decimalPlaces; i++)
                {
                    sFormat += "0";
                }
            }

            C1.WPF.DataGrid.DataGridNumericColumn Col = new C1.WPF.DataGrid.DataGridNumericColumn()
            {
                Name = sName,
                Binding = new Binding(sBinding),
                IsReadOnly = bReadOnly,
                EditOnSelection = bEditable,
                Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed,
                Maximum = dMax,
                Minimum = dMin,
                Format = sFormat,
                HorizontalAlignment = HorizonAlign,
                VerticalAlignment = VerticalAlign
            };

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;

            else
                Col.Header = Multi_Header;

            return Col;
        }

        public static C1.WPF.DataGrid.DataGridComboBoxColumn CreateComboBoxColumn
            (string Single_Header, List<string> Multi_Header, string sName, string sBinding, bool bReadOnly = false, bool bEditable = true, bool bVisible = true,
            string sDisplayMemberPath = null, string sSelectedValuePath = null,
            HorizontalAlignment HorizonAlign = HorizontalAlignment.Center, VerticalAlignment VerticalAlign = VerticalAlignment.Center)
        {
            C1.WPF.DataGrid.DataGridComboBoxColumn Col = new C1.WPF.DataGrid.DataGridComboBoxColumn()
            {
                Name = sName,
                Binding = new Binding(sBinding),
                IsReadOnly = bReadOnly,
                EditOnSelection = bEditable,
                Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed,
                DisplayMemberPath = sDisplayMemberPath,
                SelectedValuePath = sSelectedValuePath,
                HorizontalAlignment = HorizonAlign,
                VerticalAlignment = VerticalAlign
            };

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;

            else
                Col.Header = Multi_Header;

            return Col;
        }

        public static C1.WPF.DataGrid.DataGridCheckBoxColumn CreateCheckBoxColumn
            (string Single_Header, List<string> Multi_Header, string sName, string sBinding, bool bReadOnly = false, bool bEditable = true, bool bVisible = true,
            HorizontalAlignment HorizonAlign = HorizontalAlignment.Center, VerticalAlignment VerticalAlign = VerticalAlignment.Center)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn Col = new C1.WPF.DataGrid.DataGridCheckBoxColumn()
            {
                Name = sName,
                Binding = new Binding(sBinding),
                IsReadOnly = bReadOnly,
                EditOnSelection = bEditable,
                Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed,
                HorizontalAlignment = HorizonAlign,
                VerticalAlignment = VerticalAlign
            };

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;

            else
                Col.Header = Multi_Header;

            return Col;
        }

        public static C1.WPF.DataGrid.DataGridTemplateColumn CreateButtonColumn
            (string Single_Header, List<string> Multi_Header, string sName, string sBinding, FrameworkElementFactory buttonTemplate)
        {
            C1.WPF.DataGrid.DataGridTemplateColumn Col = new C1.WPF.DataGrid.DataGridTemplateColumn
            {
                Name = sName,
                Header = sBinding,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = sBinding
            };

            Col.CellTemplate = new DataTemplate() { VisualTree = buttonTemplate };
            Col.CellTemplate.Seal();

            ////if (!string.IsNullOrEmpty(Single_Header))
            ////    Col.Header = Single_Header;

            ////else
            ////    Col.Header = Multi_Header;

            return Col;
        }
    }

}
