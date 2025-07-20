/*************************************************************************************
 Created Date : 2020.12.
      Creator : 
   Decription : 날짜별 공정정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER    : Initial Created.
  2021.08.19   KDH    : Lot 유형 검색조건 추가
  2022.08.02   LHD    : 합계, 비율(%) 추가 (비율 추가를 위해 기존의 합계함수를 사용하지 못함)
  2022.11.09   조영대 : 정렬 후 더블클릭시 Tray 정보조회 이동 안되는 오류 수정
  2023.05.09   최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.10.20   조영대 : 합계, 비율 로우 Bottom Row 로 변경
  2023.10.29   손동혁 : NA1동 요청 라우트 그룹 코드 콤보 검색조건 추가 
  2023.11.17   손동혁 : 경로그룹 선택 시 날짜 검색 조건 일주일 추가
  2023.11.20   손동혁 : 라인 ALL인 상태에서 경로그룹 선택 시 날짜 일주일 검색 제한 수정
  2024.04.16   지광현 : DataGrid 내 일시(datetime) 컬럼에 대해 시간 포맷을 24시간으로 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_043 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private decimal inputTotal = 0; // 비율계산을 위한 변수
        private bool bUseFlag = false; //2023.10.29 NA1동 라우트 그룹 코드 콤보 검색조건 추가
        Util Util = new Util();
        #endregion

        #region [Initialize]
        public FCS001_043()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            InitCombo();

            bUseFlag = Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_043_NA"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {

                cboRoutGrText.Visibility = Visibility.Visible;
                cboRoutGrCode.Visibility = Visibility.Visible;

            }
            else
            {
                cboRoutGrText.Visibility = Visibility.Collapsed;
                cboRoutGrCode.Visibility = Visibility.Collapsed;
            }

            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                DateTime nowDate = DateTime.Now;

                txtLotID.Text = Util.NVC(parameters[0]);
                cboLine.SelectedValue = Util.NVC(parameters[1]);
                cboModel.SelectedValue = Util.NVC(parameters[2]);
                cboRoute.SelectedValue = Util.NVC(parameters[3]);
                cboOper.SelectedValue = Util.NVC(parameters[4]);
                
                dtpSearchDate.SelectedFromDateTime = Util.StringToDateTime(Util.StringToDateTime(Util.NVC(parameters[5])).ToString("yyyyMMdd") + " " + Util.NVC(parameters[6]), "yyyyMMdd HHmmss");
                dtpSearchDate.SelectedToDateTime = Util.StringToDateTime(Util.StringToDateTime(Util.NVC(parameters[7])).ToString("yyyyMMdd") + " " + Util.NVC(parameters[8]), "yyyyMMdd HHmmss");

                chkHistory.IsChecked = (bool)parameters[9];

                GetList();
            }
            else
            {
                InitControl();
            }
            this.Loaded -= UserControl_Loaded;

        }
        private void InitCombo()
        {
                CommonCombo_Form _combo = new CommonCombo_Form();
                C1ComboBox[] cboLineChild = { cboModel };
                _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

                C1ComboBox[] cboModelChild = { cboRoute };
                C1ComboBox[] cboModelParent = { cboLine };
                _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

                string[] sFilter = { "ROUT_RSLT_GR_CODE" };
                C1ComboBox[] cboRouteSetChild = { cboRoute };
                _combo.SetCombo(cboRouteSet, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter, cbChild: cboRouteSetChild);

                string[] sFilter4 = { "ROUT_TYPE_GR_CODE" };
                _combo.SetCombo(cboRoutGrCode, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter4, cbChild: cboRouteSetChild);

                C1ComboBox nCbo = new C1ComboBox();
                C1ComboBox[] cboRouteParent = { cboLine, cboModel, cboRouteSet, nCbo, cboRoutGrCode };
                C1ComboBox[] cboRouteChild = { cboOper };
                _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

                C1ComboBox[] cboOperParent = { cboRoute };
                _combo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.SELECT, sCase: "ROUTE_OP", cbParent: cboOperParent);

                string[] sFilter1 = { "SPCL_FLAG" };
                _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

                string[] sFilter2 = { "COMBO_PROC_INFO_BY_DATE_SEARCH_CONDITION" }; //E07
                _combo.SetCombo(cboSearch, CommonCombo_Form.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter2);

                string[] sFilter3 = { "COMBO_PROC_INFO_BY_DATE_ORDER_CONDITION" }; //E08
                _combo.SetCombo(cboOrder, CommonCombo_Form.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter3);


                // Lot 유형
                _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
        }

        private void InitControl()
        {
            dtpSearchDate.SelectedFromDateTime = GetJobDateFrom();
            dtpSearchDate.SelectedToDateTime = GetJobDateTo();
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {

                if(Util.GetCondition(cboRoutGrCode, bAllNull: true)!=null&& Util.GetCondition(cboLine, bAllNull: true) == null)//라인 ALL 상태에서 경로그룹 선택 시 날짜 일주일 제한 추가
                if ((dtpSearchDate.SelectedToDateTime - dtpSearchDate.SelectedFromDateTime).Days >= 7)
                {
                    Util.Alert("FM_ME_0231"); //조회기간은 7일을 초과할 수 없습니다.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("CURRENT_YN", typeof(string));
                dtRqst.Columns.Add("HISTORY_YN", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("START_TIME_YN", typeof(string));
                dtRqst.Columns.Add("END_TIME_YN", typeof(string));
                dtRqst.Columns.Add("ORDER_LOT_ID", typeof(string));
                dtRqst.Columns.Add("ORDER_EQP_ID", typeof(string));
                dtRqst.Columns.Add("ORDER_IN_CELL_CNT", typeof(string));
                dtRqst.Columns.Add("ORDER_BAD_CELL_CNT", typeof(string));
                dtRqst.Columns.Add("ORDER_START_TIME", typeof(string));
                dtRqst.Columns.Add("ORDER_END_TIME", typeof(string));
                dtRqst.Columns.Add("ORDER_CREATE_TIME", typeof(string));
                dtRqst.Columns.Add("ORDER_JOB_TIME", typeof(string));
                dtRqst.Columns.Add("ORDER_ROUTE_ID", typeof(string));

                dtRqst.Columns.Add(Util.GetCondition(cboSearch), typeof(string));
                dtRqst.Columns.Add("ORDER_" + Util.GetCondition(cboOrder), typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("ROUT_GR_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboOper, sMsg: "FM_ME_0107");  //공정을 선택해주세요.
                if (string.IsNullOrEmpty(dr["PROCID"].ToString())) return;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["ROUT_RSLT_GR_CODE"] = Util.GetCondition(cboRouteSet, bAllNull: true);
                dr["FROM_TIME"] = dtpSearchDate.SelectedFromDateTime.ToString("yyyy-MM-dd HH:mm:00");
                dr["TO_TIME"] = dtpSearchDate.SelectedToDateTime.ToString("yyyy-MM-dd HH:mm:00");
                if (!string.IsNullOrEmpty(txtLotID.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotID.Text);
                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["ROUT_GR_CODE"] = Util.GetCondition(cboRoutGrCode, bAllNull: true);


                if (!(bool)chkHistory.IsChecked)
                {
                    dr["CURRENT_YN"] = "Y";
                }
                // START_TIME_YN, END_TIME_YN 
                dr[Util.GetCondition(cboSearch)] = "Y";
                dr["ORDER_" + Util.GetCondition(cboOrder)] = "Y";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Rows.Add(dr);

                btnSearch.IsEnabled = false;

                // 백그라운드 실행 쿼리 완료시 dgDateOper_ExecuteDataCompleted 이벤트 진행
                dgDateOper.ExecuteService("DA_SEL_ROUTE_INFO_CONDITION_F_ALL", "RQSTDT", "RSLTDT", dtRqst);
                

            }
            catch (Exception ex)
            {
                btnSearch.IsEnabled = true;
                Util.MessageException(ex);
            }
        }

        public static DataTable ConvertToDataTable(C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn column in dg.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.Name))
                            dt.Columns.Add(column.Name);
                    }
                    return dt;
                }
                else
                {
                    dt = ((DataView)dg.ItemsSource).Table;
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        
        public static Boolean IsNumeric(string pTarget)
        {
            double dNullable;
            return double.TryParse(pTarget, System.Globalization.NumberStyles.Any, null, out dNullable);
        }

        public static Boolean IsNumeric(object oTagraet)
        {
            return IsNumeric(oTagraet.ToString());
        }

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgDateOper_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                DataTable dtResult = e.ResultData as DataTable;

                inputTotal = Util.NVC_Decimal(dtResult.Compute("Sum(INPUT_SUBLOT_QTY)", ""));

                if (!dgDateOper.IsUserConfigUsing) dgDateOper.AllColumnsWidthAuto();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private void dgDateOper_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            inputTotal = Util.NVC_Decimal(dgDateOper.GetDataTable().Compute("Sum(INPUT_SUBLOT_QTY)", ""));
        }

        private void dgDateOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //20220802 합계, 비율(%)의 배경색 변경
                if (e.Cell.Row.Index >= dataGrid.Rows.Count - 2)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                }

                if ((e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("LOTID")) && e.Cell.Row.Index < dataGrid.Rows.Count - 2)
                {

                    int row = e.Cell.Row.Index;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); // CSTID 색상 변경

                    //DUMMY TRAY
                    string _sDummy = Util.NVC(DataTableConverter.GetValue(dgDateOper.Rows[row].DataItem, "DUMMY_FLAG"));
                    if (_sDummy.Equals("Y")) e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);

                    //SPECIAL TRAY
                    string _sSpecial = Util.NVC(DataTableConverter.GetValue(dgDateOper.Rows[row].DataItem, "SPCL_FLAG"));
                    if (_sSpecial.Equals("P"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else if (_sSpecial.Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    }
                }

                if (e.Cell.Row.GetType() == typeof(DataGridSummaryRow))
                {
                    if (e.Cell.Row.Index >= dataGrid.Rows.Count - 1)
                    {
                        if (e.Cell.Column.Name.Equals("CSTID"))
                        {
                            dgDateOper.SetSummaryRowValue(1, "CSTID", ObjectDic.Instance.GetObjectName("비율(%)"));
                        }
                        else
                        {
                            decimal calcValue = 0;
                            if (inputTotal.Equals(0))
                            {
                                calcValue = 0;
                            }
                            else
                            {
                                decimal sumValue = Util.NVC_Int(dgDateOper.GetSummaryRowValue(0, e.Cell.Column.Name).Replace(",", ""));
                                calcValue = Math.Round(sumValue * 100 / inputTotal, 2);
                            }
                            dgDateOper.SetSummaryRowValue(1, e.Cell.Column.Name, calcValue.ToString("#,##0.00"));
                        }
                    }
                }

            }));

        }

        private void dgDateOper_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgDateOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (cell.Text == datagrid.CurrentColumn.Header.ToString()) return;
                                
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "LOTID")))) return;

                if (datagrid.CurrentColumn.Name == "CSTID" || datagrid.CurrentColumn.Name == "LOTID")
                {
                    string cstID = Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "CSTID")); //Tray ID
                    

                    FCS001_021 wndTRAY = new FCS001_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "CSTID")); //Tray ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDateOper.CurrentRow.DataItem, "LOTID")); //Tray No
                    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

    }
}
