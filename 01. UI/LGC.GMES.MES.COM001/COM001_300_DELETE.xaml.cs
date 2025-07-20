/*************************************************************************************
 Created Date : 2019.04.23
      Creator : 정문교
   Decription : 월력 삭제
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.22  정문교 : Initial Created.
  2019.12.01  정문교 : 공정콤보 조립 공정만 조회 되게 수정
  2019.12.19  정문교 : 월력 조회에서 클릭시 오류 수정
  2020.02.26  정문교 : 월력 등록에서 조회된 작업조 클릭, 삭제 팝업 클릭시 오류 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_300_DELETE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private Util _Util = new Util();
        private string _area = string.Empty;               // 동
        private string _equipmentSegment = string.Empty;   // 라인
        private string _process = string.Empty;            // 공정
        private string _shift = string.Empty;              // 작업조
        private string _workGroup = string.Empty;          // 작업자그룹
        private DateTime _startDate;                       // 시작일자
        private DateTime _endDate;                         // 종료일자
        public string YearMonth { get; set; }
        public bool Save { get; set; }

        private CheckBoxHeaderType _HeaderType;

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

        public COM001_300_DELETE()
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
            SearchProcess();

            this.Loaded -= C1Window_Loaded;
        }

        private void InitializeControls()
        {
            Util.gridClear(dgList);
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _area = tmps[0] as string;
            _equipmentSegment = tmps[1] as string;
            _process = tmps[2] as string;
            _shift = tmps[3] as string;
            _workGroup = tmps[4] as string;
            _startDate = DateTime.Parse(tmps[5].ToString());
            _endDate = DateTime.Parse(tmps[6].ToString());
        }

        private void SetCombo()
        {
            CommonCombo combo = new CommonCombo();
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);
            C1ComboBox[] cboProcessParent = { cboArea };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent, sCase: "PROCESS_BY_AREAID_PCSG");

            cboArea.SelectedValue = _area;
            cboEquipmentSegment.SelectedValue = _equipmentSegment;
            cboProcess.SelectedValue = _process;

            //작업조
            C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbParent: cboShiftParent);

            dtpWorkStartDay.SelectedDataTimeChanged += dtpWorkStartDay_SelectedDataTimeChanged;
            dtpWorkEndDay.SelectedDataTimeChanged += dtpWorkEndDay_SelectedDataTimeChanged;
            cboShift.SelectedValueChanged += cboShift_SelectedValueChanged;
        }

        private void SetControl()
        {
            //cboArea.SelectedValue = _area;
            //cboEquipmentSegment.SelectedValue = _equipmentSegment;
            //cboProcess.SelectedValue = _process;

            if (!string.IsNullOrWhiteSpace(_shift))
                cboShift.SelectedValue = _shift;

            dtpWorkStartDay.SelectedDateTime = _startDate;
            dtpWorkEndDay.SelectedDateTime = _endDate;

            _HeaderType = CheckBoxHeaderType.Zero;
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

        private void cboShift_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
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
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
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
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

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
        private void SearchProcess()
        {
            try
            {
                _HeaderType = CheckBoxHeaderType.Zero;

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WORKER_DELETE_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRK_GR_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FR"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["DATE_TO"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                dr["SHFT_ID"] = cboShift.SelectedValue.ToString() == "" ? null : cboShift.SelectedValue.ToString();
                dr["WRK_GR_ID"] = _workGroup;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgList, bizResult, null, true);
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

                ///////////////////////////////// 삭제 자료 Setting
                //삭제 일자
                TimeSpan TSpan = dtpWorkEndDay.SelectedDateTime - dtpWorkStartDay.SelectedDateTime;
                int DayCount = TSpan.Days + 1;

                //삭제 작업조
                DataRow[] drSelect = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

                for (int cnt = 0; cnt < DayCount; cnt++)
                {
                    foreach (DataRow dr in drSelect)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CALDATE"] = dtpWorkStartDay.SelectedDateTime.AddDays(cnt).ToString("yyyy-MM-dd");
                        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        newRow["AREAID"] = dr["AREAID"];
                        newRow["EQSGID"] = dr["EQSGID"];
                        newRow["PROCID"] = dr["PROCID"];
                        newRow["SHFT_ID"] = dr["SHFT_ID"];
                        inTable.Rows.Add(newRow);
                    }
                }
                ////////////////////////////////////////////////////////////////////

                new ClientProxy().ExecuteService("BR_PRD_DEL_WORK_CALENDAR_L", "INDATA", "", inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        YearMonth = dtpWorkStartDay.SelectedDateTime.ToString("yyyy-MM");
                        Save = true;

                        // 재조회
                        SearchProcess();
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
                if (dtNowDate > DateTime.Parse( dr["WRK_STRT_DTTM"].ToString()))
                {
                    // 오늘 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1738");

                    return false;
                }
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
