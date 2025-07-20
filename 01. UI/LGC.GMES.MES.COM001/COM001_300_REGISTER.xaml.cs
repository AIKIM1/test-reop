/*************************************************************************************
 Created Date : 2019.04.22
      Creator : 정문교
   Decription : 월력 등록
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.22  정문교 : Initial Created.
  2019.10.08  정문교 : 작업조 그룹 콤보  공통코드 CommonCode의 SHFT_GR_CODE에서 
                                         동별 공통코드  TB_MMD_AREA_COM_CODE의 SHFT_GR_CODE로 변경 
  2019.12.01  정문교 : 공정콤보 조립 공정만 조회 되게 수정
  2020.01.30  정문교 : 공정 콤보 변경시에도 작업조 콤보 재조회 추가

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_300_REGISTER : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private Util _Util = new Util();
        public string YearMonth { get; set; }
        public string Area { get; set; }
        public string EquipmentSegment { get; set; }
        public string Process { get; set; }
        public bool Save { get; set; }

        private CheckBoxHeaderType _HeaderType;
        private CheckBoxHeaderType _HeaderTypeAdd;

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public COM001_300_REGISTER()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            SetParameters();
            SetCombo();
            SetControl();

            this.Loaded -= C1Window_Loaded;
        }

        private void InitializeControls()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgADDList);
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            Area = tmps[0] as string;
            EquipmentSegment = tmps[1] as string;
            Process = tmps[2] as string;
        }

        private void SetCombo()
        {
            CommonCombo combo = new CommonCombo();
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);
            C1ComboBox[] cboProcessParent = { cboArea };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent, sCase: "PROCESS_BY_AREAID_PCSG");

            //작업조그룹
            //string[] sFilter = { "SHFT_GR_CODE" };
            //C1ComboBox[] cbocboShiftGrCodeChild = { cboShift };
            //combo.SetCombo(cboShiftGrCode, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
            SetShiftGrCombo(cboShiftGrCode);

            //작업조
            //C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess, cboShiftGrCode };
            //combo.SetCombo(cboShift, CommonCombo.ComboStatus.SELECT, cbParent: cboShiftParent, sCase: "SHIFTGRCODE");
            SetShiftCombo(cboShift);

            //작업자그룹
            SetWorkGroupCombo(cboWorkGroup);

            dtpWorkStartDay.SelectedDataTimeChanged += dtpWorkStartDay_SelectedDataTimeChanged;
            dtpWorkEndDay.SelectedDataTimeChanged += dtpWorkEndDay_SelectedDataTimeChanged;
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboShiftGrCode.SelectedValueChanged += cboShiftGrCode_SelectedValueChanged;
            cboShift.SelectedValueChanged += cboShift_SelectedValueChanged;
            cboWorkGroup.SelectedValueChanged += cboWorkGroup_SelectedValueChanged;
        }

        private void SetControl()
        {
            if (!string.IsNullOrWhiteSpace(Area))
                cboArea.SelectedValue = Area;
            if (!string.IsNullOrWhiteSpace(EquipmentSegment))
                cboEquipmentSegment.SelectedValue = EquipmentSegment;
            if (!string.IsNullOrWhiteSpace(Process))
                cboProcess.SelectedValue = Process;

            dtpWorkStartDay.SelectedDateTime = DateTime.Now;
            dtpWorkEndDay.SelectedDateTime = DateTime.Now.AddDays(1);

            _HeaderType = CheckBoxHeaderType.Zero;
            _HeaderTypeAdd = CheckBoxHeaderType.Zero;
        }

        /// <summary>
        /// 일자 변경시 
        /// </summary>
        private void dtpWorkStartDay_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            InitializeControls();
        }
        private void dtpWorkEndDay_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            InitializeControls();
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShiftGrCombo(cboShiftGrCode);
            SetWorkGroupCombo(cboWorkGroup);
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShiftCombo(cboShift);
            SetWorkGroupCombo(cboWorkGroup);
        }

        private void cboShiftGrCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitializeControls();
            SetShiftCombo(cboShift);
        }

        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitializeControls();
        }

        private void cboWorkGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitializeControls();
        }

        /// <summary>
        /// DataGrid 해더 체크 
        /// </summary>
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgList;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        /// <summary>
        /// DataGrid 해더 체크 
        /// </summary>
        private void tbCheckHeaderAddAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgADDList;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeAdd)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeAdd)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeAdd = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeAdd = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            // 등록자 정보 조회
            SearchProcess("1");
            // 미등록자 정보 조회
            SearchProcess("2");
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete())
                return;

            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteProcess();
                }
            });
        }

        /// <summary>
        /// 작업자추가
        /// </summary>
        private void btnADD_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 추가하시겠습니까?
            Util.MessageConfirm("SFU2965", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        /// <summary>
        /// 작업조그룹 콤보 cboShiftGrCode
        /// </summary>
        private void SetShiftGrCombo(C1ComboBox cbo)
        {
            if (cboArea.SelectedValue == null) return;

            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";
            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID,  cboArea.SelectedValue.ToString(), "SHFT_GR_CODE" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// 작업조 콤보
        /// </summary>
        private void SetShiftCombo(C1ComboBox cbo)
        {
            if (cboArea.SelectedValue == null) return;
            if (cboProcess.SelectedValue == null) return;

            const string bizRuleName = "DA_BAS_SEL_SHIFT_CBO_L";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID", "EQSGID", "PROCID", "SHFT_GR_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, cboArea.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString(), cboShiftGrCode.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// 작업조 그룹 콤보
        /// </summary>
        private void SetWorkGroupCombo(C1ComboBox cbo)
        {
            if (cboArea.SelectedValue == null) return;
            if (cboProcess.SelectedValue == null) return;

            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_PROC_GR";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID", "WRK_GR_ID" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.ToString(), cboProcess.SelectedValue.ToString(), null };
            string selectedValueText = "WRK_GR_ID";
            string displayMemberText = "WRK_GR_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void SearchProcess(string Select)
        {
            try
            {
                _HeaderType = CheckBoxHeaderType.Zero;
                _HeaderTypeAdd = CheckBoxHeaderType.Zero;

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WORKER_REGISTER_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRK_GR_ID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("SELECT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                dr["SHFT_ID"] =cboShift.SelectedValue.ToString();
                dr["WRK_GR_ID"] =cboWorkGroup.SelectedValue.ToString();
                dr["DATE_FR"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["DATE_TO"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["SELECT"] = Select;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (Select.Equals("1"))
                        Util.GridSetData(dgList, bizResult, null, true);
                    else
                        Util.GridSetData(dgADDList, bizResult, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void DeleteProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CALDATE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHFT_ID", typeof(string));
                inTable.Columns.Add("WRK_GR_ID", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));

                ///////////////////////////////// 삭제 자료 Setting
                DataRow[] drSelect = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

                foreach (DataRow dr in drSelect)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["CALDATE"] = dr["CALDATE"];
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["AREAID"] = dr["AREAID"];
                    newRow["EQSGID"] = dr["EQSGID"];
                    newRow["PROCID"] = dr["PROCID"];
                    newRow["SHFT_ID"] = dr["SHFT_ID"];
                    newRow["WRK_GR_ID"] = dr["WRK_GR_ID"];
                    newRow["WRK_USERID"] = dr["WRK_USERID"];
                    inTable.Rows.Add(newRow);
                }
                ////////////////////////////////////////////////////////////////////

                new ClientProxy().ExecuteService("BR_PRD_DEL_WORK_CALENDAR_USER_L", "INDATA", "", inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Area = cboArea.SelectedValue.ToString();
                        EquipmentSegment = cboEquipmentSegment.SelectedValue.ToString();
                        Process = cboProcess.SelectedValue.ToString();
                        YearMonth = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM");
                        Save = true;

                        // 재조회
                        SearchProcess("1");
                        SearchProcess("2");
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

        /// <summary>
        /// 작업자추가
        /// </summary>
        private void SaveProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHFT_ID", typeof(string));
                inTable.Columns.Add("WRK_GR_ID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inWorkUser = inDataSet.Tables.Add("INWORKUSER");
                inWorkUser.Columns.Add("WRK_USERID", typeof(string));

                /////////////////////////////////////////////////////// Data Setting
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["SHFT_ID"] = cboShift.SelectedValue.ToString();
                newRow["WRK_GR_ID"] = cboWorkGroup.SelectedValue.ToString();
                newRow["FROMDATE"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd");
                newRow["TODATE"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd");
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] drSelect = DataTableConverter.Convert(dgADDList.ItemsSource).Select("CHK = 1");

                foreach(DataRow dr in drSelect)
                {
                    newRow = inWorkUser.NewRow();
                    newRow["WRK_USERID"] = dr["WRK_USERID"];
                    inWorkUser.Rows.Add(newRow);
                }
                ////////////////////////////////////////////////////////////////////

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_WORK_CALENDAR_L", "INDATA,INWORKUSER", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Area = cboArea.SelectedValue.ToString();
                        EquipmentSegment = cboEquipmentSegment.SelectedValue.ToString();
                        Process = cboProcess.SelectedValue.ToString();
                        YearMonth = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM");
                        Save = true;

                        // 재조회
                        SearchProcess("1");
                        SearchProcess("2");
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

        #endregion

        #region [Func]

        #region [Validation]
        private bool ValidationSearch()
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboShift.SelectedIndex < 0 || cboShift.SelectedValue.GetString().Equals("SELECT"))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }

            if (cboWorkGroup.SelectedIndex < 0 || cboWorkGroup.SelectedValue.GetString().Equals("SELECT"))
            {
                // 선택된 근무자그룹이 없습니다.
                Util.MessageValidation("SFU2049");
                return false;
            }

            if (dtpWorkStartDay.SelectedDateTime > dtpWorkEndDay.SelectedDateTime)
            {
                // 조회일자중 이전일자가 이후일자보다 더 클 수 없습니다.
                Util.MessageValidation("SFU1908");
                return false;
            }

            return true;
        }

        private bool ValidationDelete()
        {
            DataRow[] drSelect = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            DateTime dtNowDate = GetSystemTime();

            foreach (DataRow dr in drSelect)
            {
                if (dtNowDate > DateTime.Parse(dr["WRK_STRT_DTTM"].ToString()))
                {
                    // 오늘 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1738");

                    return false;
                }
            }

            return true;
        }

        private bool ValidationSave()
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboShift.SelectedIndex < 0 || cboShift.SelectedValue.GetString().Equals("SELECT"))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }

            if (cboWorkGroup.SelectedIndex < 0 || cboWorkGroup.SelectedValue.GetString().Equals("SELECT"))
            {
                // 선택된 근무자그룹이 없습니다.
                Util.MessageValidation("SFU2049");
                return false;
            }

            if (dtpWorkStartDay.SelectedDateTime > dtpWorkEndDay.SelectedDateTime)
            {
                // 종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU1913");
                return false;
            }

            int rowChkCount = DataTableConverter.Convert(dgADDList.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (rowChkCount == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }
        #endregion

        #region [팝업]
        #endregion

        #region [Function]

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
        #endregion

        #endregion

    }

}
