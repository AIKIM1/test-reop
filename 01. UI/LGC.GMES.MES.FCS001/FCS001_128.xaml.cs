/*************************************************************************************
 Created Date : 2021.12.20
      Creator : 이정미
   Decription : 설비별 공 tray type 관리
--------------------------------------------------------------------------------------------
 [Change History]
  2021.12.20 NAME   : Initial Created.
  2022.04.03 이정미 : 설비화면 UPDATE 조건 변경(EQPTID -> PORT_ID)
  2022.06.28 이정미 : InitCombo(cboEqp) 변경, SetEqptGrTypeCode 함수 및 SetCodeDisplay 추가
                      디자인 변경, Loc_btnSave_Click 변경
  2022.07.01 이정미 : 강제출고 오류 수정
  2022.07.11 조영대 : 설비군 콤보 변경시 조회
  2022.09.13 최도훈 : Eqp_btnSave_Click 저장완료 문구 중복표시 문제 수정
  2022.10.06 이정미 : btnDisableMapping_Click 다중으로 매핑해제되는 오류 수정 
  2022.11.08 이정미 : 설비군 콤보박스 및 활성화 레인 컬럼 동별 공통코드로 변경, 디자인 변경,
                      Loc_btnSave_Click 수정 
  2023.11.17 김용식 : CV 목적지 설명 다국어처리
*******************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_128 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtHeader = new DataTable();
        private string Location_Id = "";
        private string Eqp = "";
        private string Eqpt_Id = "";
        private string Use_Flag = "";
        private bool bUseLangDesc = false; // 2023.11.17 다국어 기능 추가
        public FCS001_128()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter1 = { "USE_FLAG" };
            _combo.SetCombo(cboUseFlag, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilter1);
            LaneSetGridCboItem(dgLOC.Columns["LANE_ID"], "AREA_LANE_CODE");
            SetEqptGrTypeCode(cboEqp);
            SetGridCboItem(dgLOC.Columns["RANGE_LOCATION_FLAG"], "USE_FLAG");
            SetGridCboItem(dgLOC.Columns["REAL_TRAY_RCV_ENABLE_FLAG"], "USE_FLAG");
            SetGridCboItem(dgLOC.Columns["REAL_TRAY_ISS_ENABLE_FLAG"], "USE_FLAG");
            SetGridCboItem(dgLOC.Columns["EMPTY_TRAY_RCV_ENABLE_FLAG"], "USE_FLAG");
            SetGridCboItem(dgLOC.Columns["EMPTY_TRAY_ISS_ENABLE_FLAG"], "USE_FLAG");
            SetGridCboItem(dgLOC.Columns["USE_FLAG"], "USE_FLAG");
        }
        #endregion

        #region Method

        private void LocGetList()
        {
            try
            {
                Util.gridClear(dgLOC);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_GR_TYPE_CODE"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CNVR_LOCATION_UI", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    dtRslt.Columns.Add("FLAG", typeof(string));
                    
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["FLAG"] = "N";
                    }
                }

                 
                Util.GridSetData(dgLOC, dtRslt, FrameOperation, true);

                dgLOC.Columns["LANE_ID"].Width = new C1.WPF.DataGrid.DataGridLength(120);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void EqpGetList(string location_id)
        {
            try
            {
                Util.gridClear(dgEQP);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CNVR_LOCATION_ID"] = location_id;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQUIPMENT_UI", "RQSTDT", "RSLTDT", dtRqst);



                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("FLAG", typeof(string));

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["FLAG"] = "N";
                    }

                }
                Util.GridSetData(dgEQP, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TrayTypeGetList(string Location_Id)
        {
            try
            {
                Util.gridClear(dgTrayType);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["CNVR_LOCATION_ID"] = Location_Id;
                dr["EQPTID"] = Eqpt_Id;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ENABLE_TRAY_TYPE_UI", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("FLAG", typeof(string));

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["FLAG"] = "N";
                    }

                }
                Util.GridSetData(dgTrayType, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                string tag = Util.NVC(e.Cell.Column.Tag);

                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag.Equals("A"))
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                if (e.Cell.Row.Index == dataGrid.Rows.Count() - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));
        }

        private void LocDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void EqpDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                if (Location_Id.Equals(""))
                {
                    return;
                }

                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        // dr["USE_FLAG"] = "Y";
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - dg.TopRows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool LaneSetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = true, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "AREA_LANE_CODE";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetCodeDisplay(dtResult, bCodeDisplay));
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetEqptGrTypeCode(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPT_GR_TYPE_CODE_UI";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
                dr[sDisplay] = "-ALL-";
                dr[sValue] = "";
                dt.Rows.InsertAt(dr, 0);
                return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "] " + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }

        private string ReplaceConvDescription(string Lang, string BaseTxt, string ReplaceTxt)
        {
            // 2023.11.17 CV 목적지 설명 다국어처리
            string Returntxt = string.Empty;
            string TempTxt = string.Empty;

            for (int i = 0; i < BaseTxt.Split('|').Length; i++)
            {
                TempTxt = BaseTxt.Split('|')[i];
                if ("ko-KR".Equals(Lang) && BaseTxt.Split('|')[i].Contains("ko-KR"))
                {
                    TempTxt = "ko-KR\\" + ReplaceTxt;
                }
                else if ("zh-CN".Equals(Lang) && BaseTxt.Split('|')[i].Contains("zh-CN"))
                {
                    TempTxt = "zh-CN\\" + ReplaceTxt;
                }
                else if ("en-US".Equals(Lang) && BaseTxt.Split('|')[i].Contains("en-US"))
                {
                    TempTxt = "en-US\\" + ReplaceTxt;
                }
                else if ("pl-PL".Equals(Lang) && BaseTxt.Split('|')[i].Contains("pl-PL"))
                {
                    TempTxt = "pl-PL\\" + ReplaceTxt;
                }
                else if ("uk_UA".Equals(Lang) && BaseTxt.Split('|')[i].Contains("uk_UA"))
                {
                    TempTxt = "uk_UA\\" + ReplaceTxt;
                }
                else if ("ru-RU".Equals(Lang) && BaseTxt.Split('|')[i].Contains("ru-RU"))
                {
                    TempTxt = "ru-RU\\" + ReplaceTxt;
                }
                else if ("id-ID".Equals(Lang) && BaseTxt.Split('|')[i].Contains("id-ID"))
                {
                    TempTxt = "id-ID\\" + ReplaceTxt;
                }
                else
                {
                    // 최소 1회 셋업용
                    if("ko-KR".Equals(Lang))
                    {
                        Returntxt = "ko-KR\\" + ReplaceTxt + "|zh-CN\\|en-US\\|pl-PL\\|uk-UA\\|ru-RU\\|id-ID\\";
                        break;
                    }
                }
                Returntxt += TempTxt + "|";
            }

            if (Returntxt.Length > 0 && Returntxt.Length == Returntxt.LastIndexOf('|'))
            {
                Returntxt.Substring(0, Returntxt.Length - 1);
            }
            return Returntxt;
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            bUseLangDesc = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_128_LangConvDesc"); // 2023.11.17 Conv 목적지 ID 다국어사용

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgEQP);
            Util.gridClear(dgTrayType);
            txtEQUIPMENT.Text = "";
            txtEQP.Text = "";
            LocGetList();
        }

        private void dgLOC_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgLOC.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgEQP_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgLOC.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void Loc_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            LocDataGridRowAdd(dgLOC, 1);
            SetGridCboItem(dgLOC.Columns["USE_FLAG"], "USE_FLAG");
            SetGridCboItem(dgLOC.Columns["CNVR_LOCATION_ID"], "CNVR_LOCATION_ID");
            DataTableConverter.SetValue(dgLOC.Rows[dgLOC.Rows.Count - 1].DataItem, "FLAG", "Y");
            dgLOC.ScrollIntoView(dgLOC.Rows.Count - 1, 1); //스크롤 하단 고정

        }

        private void Loc_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            string flag = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[dgLOC.Rows.Count - 1].DataItem, "FLAG"));
            if (flag.Equals("Y")) DataGridRowRemove(dgLOC);
            dgLOC.ScrollIntoView(dgLOC.Rows.Count - 1, 1); //스크롤 하단 고정
        }

        private void Loc_btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //편집중인 내역 Commit.
                dgLOC.EndEdit(true);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("RANGE_LOCATION_FLAG", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("REAL_TRAY_RCV_ENABLE_FLAG", typeof(string));
                dtRqst.Columns.Add("REAL_TRAY_ISS_ENABLE_FLAG", typeof(string));
                dtRqst.Columns.Add("REAL_TRAY_RCV_PRIORITY_NO", typeof(string));
                dtRqst.Columns.Add("EMPTY_TRAY_RCV_ENABLE_FLAG", typeof(string));
                dtRqst.Columns.Add("EMPTY_TRAY_ISS_ENABLE_FLAG", typeof(string));
                dtRqst.Columns.Add("EMPTY_TRAY_RCV_PRIORITY_NO", typeof(string));
                dtRqst.Columns.Add("CNVR_BUF_LEN_VALUE", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_DESC", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));


                DataRow dr = null;
                for (int i = 0; i < dgLOC.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "FLAG")).Equals("Y") &&
                        Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                    {
                        dr = dtRqst.NewRow();
                        dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                        dr["RANGE_LOCATION_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "RANGE_LOCATION_FLAG"));
                        dr["EQPT_GR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "EQPT_GR_TYPE_CODE"));
                        dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "LANE_ID"));
                        dr["REAL_TRAY_RCV_ENABLE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "REAL_TRAY_RCV_ENABLE_FLAG"));
                        dr["REAL_TRAY_ISS_ENABLE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "REAL_TRAY_ISS_ENABLE_FLAG"));
                        dr["REAL_TRAY_RCV_PRIORITY_NO"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "REAL_TRAY_RCV_PRIORITY_NO"));
                        dr["EMPTY_TRAY_RCV_ENABLE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "EMPTY_TRAY_RCV_ENABLE_FLAG"));
                        dr["EMPTY_TRAY_ISS_ENABLE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "EMPTY_TRAY_ISS_ENABLE_FLAG"));
                        dr["EMPTY_TRAY_RCV_PRIORITY_NO"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "EMPTY_TRAY_RCV_PRIORITY_NO"));
                        dr["CNVR_BUF_LEN_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "CNVR_BUF_LEN_VALUE"));
                        
                        if (bUseLangDesc)
                        {
                            // 2023.11.17 CV 목적지 설명 다국어처리
                            dr["CNVR_LOCATION_DESC"] = ReplaceConvDescription(LoginInfo.LANGID, Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "CNVR_BASE_DESC_TEXT")), Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "CNVR_LOCATION_DESC")));
                        }
                        else
                        {
                            dr["CNVR_LOCATION_DESC"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "CNVR_LOCATION_DESC"));
                        }

                        

                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgLOC.Rows[i].DataItem, "USE_FLAG"));
                        dtRqst.Rows.Add(dr);
                    }
                }


                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_LOCATION_UI", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
                        {
                            Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                        }

                        LocGetList();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Eqp_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            if (Location_Id.Equals(null) || Location_Id.Equals(""))
            {
                return;
            }
            EqpDataGridRowAdd(dgEQP, 1);
            DataTableConverter.SetValue(dgEQP.Rows[dgEQP.Rows.Count - 1].DataItem, "FLAG", "Y");
            dgEQP.ScrollIntoView(dgEQP.Rows.Count - 1, 1); //스크롤 하단 고정
        }

        private void Eqp_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if (Location_Id.Equals(null) || Location_Id.Equals(""))
            {
                return;
            }
            string flag = Util.NVC(DataTableConverter.GetValue(dgEQP.Rows[dgEQP.Rows.Count - 1].DataItem, "FLAG"));
            if (flag.Equals("Y")) DataGridRowRemove(dgEQP);
            dgEQP.ScrollIntoView(dgEQP.Rows.Count - 1, 1); //스크롤 하단 고정
        }

        private void Eqp_btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("PORT_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                for (int i = 0; i < dgEQP.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgEQP.Rows[i].DataItem, "PORT_ID"));
                    dr["CNVR_LOCATION_ID"] = Location_Id;
                    dr["USERID"] = LoginInfo.USERID;
                    dtRqst.Rows.Add(dr);
                }

                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_MCS_PORT_UI", "INDATA", "OUTDATA", dtRqst);

                        Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        EqpGetList(Location_Id);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            /*DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_DEL_MCS_PORT_UI", "INDATA", "OUTDATA", dtRqst);

            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
            {
                Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
            }
            else
            {
                Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
            }
            EqpGetList(location_id);*/
        }

        private void btnForce_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter a = (DataGridCellPresenter)((Button)sender).Parent;
            C1.WPF.DataGrid.C1DataGrid datagrid = a.DataGrid;
            int idx = a.Row.Index;

            try
            {
                Util.MessageConfirm("FM_ME_0094", (result) => //강제 출고 요청을 하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("CV_INOUT_LOC_ID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("USER_IP", typeof(string));
                    inData.Columns.Add("PC_NAME", typeof(string));
                    inData.Columns.Add("FORM_ID", typeof(string));
                    inData.Columns.Add("MENU_ID", typeof(string));

                    DataRow dr = inData.NewRow();
                    dr["CV_INOUT_LOC_ID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[idx].DataItem, "CNVR_LOCATION_ID"));
                    dr["USERID"] = LoginInfo.USERID;
                    dr["USER_IP"] = "";
                    dr["PC_NAME"] = "";
                    dr["FORM_ID"] = "";
                    dr["MENU_ID"] = LoginInfo.CFG_MENUID;
                    inData.Rows.Add(dr);

                    DataSet dsrslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_EMPTY_TRAY_FORCE_SHIP_MANUAL", "INDATA", "OUTDATA", inDataSet);

                    if (dsrslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                    {
                        Util.MessageInfo("FM_ME_0092"); //  강제 출고 요청을 완료했습니다.
                    }

                    else
                    {
                        Util.AlertInfo("FM_ME_0091", "", "");// 강제 출고 요청에 실패했습니다.  
                    }
                });
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDisableMapping_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter a = (DataGridCellPresenter)((Button)sender).Parent;
            C1.WPF.DataGrid.C1DataGrid datagrid = a.DataGrid;
            int idx = a.Row.Index;

            try
            {   
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("PORT_ID", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgEQP.Rows[idx].DataItem, "PORT_ID"));
                    dr["USERID"] = LoginInfo.USERID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_DEL_MCS_PORT_UI", "INDATA", "OUTDATA", dtRqst);

                    Util.MessageValidation("FM_ME_0215");  //저장하였습니다.

                    EqpGetList(Location_Id);
                    
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLOC_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Util.gridClear(dgTrayType);
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            int row = cell.Row.Index;

            Location_Id = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "CNVR_LOCATION_ID"));
            Eqp = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQPT_GR_TYPE_CODE"));
            Use_Flag = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "USE_FLAG"));
            EqpGetList(Location_Id);
        }

        private void dgEQP_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }
            int row = cell.Row.Index;

            if (Eqpt_Id.Equals(null))
            {
                return;
            }
            Eqpt_Id = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "EQPTID"));
            txtEQUIPMENT.Text = Eqpt_Id;
            txtEQP.Text = Eqp;
            TrayTypeGetList(Location_Id);
        }

        private void dgLOC_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void dgEQP_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void cboEqp_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch.PerformClick();
        }
        #endregion

    }
}
