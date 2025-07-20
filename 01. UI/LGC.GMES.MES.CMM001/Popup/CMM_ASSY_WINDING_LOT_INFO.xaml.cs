/*************************************************************************************
 Created Date : 2018.01.12
      Creator : 신광희
   Decription : Winding LOT Info
--------------------------------------------------------------------------------------
 [Change History]
   2018.01.12   신광희C : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_WINDING_LOT_INFO : C1Window, IWorkArea
    {
        #region Declaration

        private readonly Util _util = new Util();
        private readonly BizDataSet _bizRule = new BizDataSet();

        private DataTable _dtequipment;
        private int _selectdindex;

        private DataTable _dtEquipmentSegment;
        private int _equipmentSegmentselectedIndex;

        private string _processCode = string.Empty;
        private bool _isSmallType;
        private bool _isLoaded = false;

        private string _selectedEquipmentCode = string.Empty;
        private string _selectedEquipmentSegmentCode = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public CMM_ASSY_WINDING_LOT_INFO()
        {
            InitializeComponent();
        }

        private void InitializeControl()
        {
            dtpDateTo.SelectedDateTime = GetSystemTime();
            dtpDateFrom.SelectedDateTime = GetSystemTime().AddDays(-7);

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void InitCombo()
        {
            cboEquipment.ItemsSource = DataTableConverter.Convert(_dtequipment);

            if (string.IsNullOrEmpty(_selectedEquipmentCode))
                cboEquipment.SelectedIndex = _selectdindex;
            else
                cboEquipment.SelectedValue = _selectedEquipmentCode;

            cboEquipmentSegmentAssy.ItemsSource = DataTableConverter.Convert(_dtEquipmentSegment);

            if (string.IsNullOrEmpty(_selectedEquipmentSegmentCode))
                cboEquipmentSegmentAssy.SelectedIndex = _equipmentSegmentselectedIndex;
            else
                cboEquipmentSegmentAssy.SelectedValue = _selectedEquipmentSegmentCode;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _dtequipment = tmps[0] as DataTable;

            if (CommonVerify.HasTableRow(_dtequipment))
            {
                if (_dtequipment != null)
                {
                    _dtequipment?.Rows.RemoveAt(0);
                    DataRow drSelect = _dtequipment.NewRow();
                    drSelect["CBO_NAME"] = "-ALL-";
                    drSelect["CBO_CODE"] = null;
                    _dtequipment.Rows.InsertAt(drSelect, 0);
                }
            }

            _selectdindex = string.IsNullOrEmpty(tmps[1].ToString()) ? 0 : Convert.ToInt16(tmps[1].ToString());
            _dtEquipmentSegment = tmps[2] as DataTable;
            _equipmentSegmentselectedIndex = string.IsNullOrEmpty(tmps[3].GetString()) ? 0 : Convert.ToInt16(tmps[3]);
            _processCode = tmps[4].GetString();
            _isSmallType = (bool) tmps[5];

            if(tmps.Length > 6)
            {
                _selectedEquipmentCode = tmps[6].GetString();
                _selectedEquipmentSegmentCode = tmps[7].GetString();

            }

            InitializeControl();
            InitCombo();
            //GetWindingLotList();
            //dtpProdDate.SelectedDateTime = System.DateTime.Now;
            BringToFront();

            _isLoaded = true;
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationWindingLotSearch()) return;
            GetWindingLotList();
        }

        private void cboEquipmentSegmentAssy_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_isLoaded)
                SetEquipmentCombo(cboEquipment);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (dtPik == null) return;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
            //dtpDateFrom.Focus();
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;

            if (dtPik == null) return;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
            //dtpDateTo.Focus();
            //int day = dtpDateFrom.SelectedDateTime.Subtract(dtPik.SelectedDateTime).Days;
            //if (Math.Abs(day) > 30)
            //{
            //    Util.MessageValidation("SFU3567");
            //    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
            //    return;
            //}
        }

        #endregion

        #region User Method

        #region [BizCall]
        private void GetWindingLotList()
        {
            if (!ValidationWindingLotSearch()) return;
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_WINDING_LOT_INFO_WN";
                DataTable indataTable = new DataTable("RQSTDT");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("CA_PANCAKE", typeof(string));
                indataTable.Columns.Add("AN_PANCAKE", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));
                indataTable.Columns.Add("OUT_LOTID", typeof(string));
                indataTable.Columns.Add("FROM_DATE", typeof(string));
                indataTable.Columns.Add("TO_DATE", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));

                string fromMonth = dtpDateFrom.SelectedDateTime.Month.GetString();
                if (dtpDateFrom.SelectedDateTime.Month.GetString().Length < 2)
                    fromMonth = "0" + dtpDateFrom.SelectedDateTime.Month.GetString();

                string fromDay = dtpDateFrom.SelectedDateTime.Day.GetString();
                if (dtpDateFrom.SelectedDateTime.Day.GetString().Length < 2)
                    fromDay = "0" + dtpDateFrom.SelectedDateTime.Day.GetString();

                string fromDate = dtpDateFrom.SelectedDateTime.Year.GetString() + "-" + fromMonth + "-" + fromDay;

                string toMonth = dtpDateTo.SelectedDateTime.Month.GetString();
                if (dtpDateTo.SelectedDateTime.Month.GetString().Length < 2)
                    toMonth = "0" + dtpDateTo.SelectedDateTime.Month.GetString();

                string toDay = dtpDateTo.SelectedDateTime.Day.GetString();
                if (dtpDateTo.SelectedDateTime.Day.GetString().Length < 2)
                    toDay = "0" + dtpDateTo.SelectedDateTime.Day.GetString();

                string toDate = dtpDateTo.SelectedDateTime.Year.GetString() + "-" + toMonth + "-" + toDay;

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["CA_PANCAKE"] = !string.IsNullOrEmpty(txtCaPancake.Text.Trim()) ? txtCaPancake.Text : null;
                dr["AN_PANCAKE"] = !string.IsNullOrEmpty(txtAnPancake.Text.Trim()) ? txtAnPancake.Text : null;
                dr["PROD_LOTID"] = !string.IsNullOrEmpty(txtWindingLot.Text.Trim()) ? txtWindingLot.Text : null;
                dr["OUT_LOTID"] = !string.IsNullOrEmpty(txtCartLot.Text.Trim()) ? txtCartLot.Text : null;
                dr["FROM_DATE"] = fromDate;
                dr["TO_DATE"] = toDate;
                dr["PROCID"] = _processCode;
                indataTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    //if (!CommonVerify.HasTableRow(bizResult))
                    //{
                    //    Util.MessageValidation("SFU1195");
                    //    return;
                    //}

                    Util.GridSetData(dgWindingLotInfo, bizResult, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]

        private bool ValidationWindingLotSearch()
        {
            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;
            if (timeSpan.Days < 0)
            {
                //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                Util.MessageValidation("SFU3569");
                return false;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");
                return false;
            }

            if (cboEquipmentSegmentAssy.SelectedIndex < 0 || cboEquipmentSegmentAssy.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            return true;
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

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            //원각 : CR , 초소형 : CS
            //string gubun = _isSmallType ? "CS" : "CR";

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegmentAssy.SelectedValue.GetString(), _processCode, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #endregion

        private void txtWindingLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtWindingLot.Text.Length < 4)
                {
                    Util.MessageInfo("SFU4342", 3);
                    txtWindingLot.Text = string.Empty;
                }
                else
                {
                    GetWindingLotList();
                }
            }
        }

        private void txtCartLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtCartLot.Text.Length < 4)
                {
                    Util.MessageInfo("SFU4342", 3);
                    txtCartLot.Text = string.Empty;
                }
                else
                {
                    GetWindingLotList();
                }
            }
        }

        private void txtCaPancake_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtCaPancake.Text.Length < 4)
                {
                    Util.MessageInfo("SFU4342", 3);
                    txtCaPancake.Text = string.Empty;
                }
                else
                {
                    GetWindingLotList();
                }
            }
        }

        private void txtAnPancake_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtAnPancake.Text.Length < 4)
                {
                    Util.MessageInfo("SFU4342", 3);
                    txtAnPancake.Text = string.Empty;
                }
                else
                {
                    GetWindingLotList();
                }
            }
        }
    }

}
