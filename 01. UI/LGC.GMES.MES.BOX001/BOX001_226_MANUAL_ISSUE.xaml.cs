/*************************************************************************************
 Created Date : 2023.08.11
      Creator : 
   Decription : TBOX 자동창고 모니터링 수동출고 예약 팝업
--------------------------------------------------------------------------------------
 [Change History]
    0000.00.00  홍길동 차장 : 수정 내용 작성.
**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_226_MANUAL_ISSUE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_226_MANUAL_ISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        CommonCombo _combo = new CommonCombo();

        private bool _isLoaded;
        private string _equipmentCode;
        private DataTable _dtRowColumnLayer;
        private DataTable _dtRackInfo;
        public bool IsUpdated;
        #endregion

        public BOX001_226_MANUAL_ISSUE()
        {
            InitializeComponent();
        }

        #region Initialize 

        private void InitializeControls()
        {
            SetStockerCombo(cboArea);
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                _dtRackInfo = parameters[1] as DataTable;

                if (_dtRackInfo != null)
                    SetRackInfo();
            }
        }

        private void SetRackInfo()
        {
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void btnIssueReservation_Click(object sender, RoutedEventArgs e)
        {
            IssueReservation_TBox();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cboRow_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboRow?.SelectedValue != null)
            {
                GetColumnList();
            }
        }        

        private void cboColumn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboColumn?.SelectedValue != null)
            {
                GetLayerList();
            }
        }

        #endregion

        #region Mehod

        private void GetColumnList()
        {
            var queryColumn = (from t in _dtRowColumnLayer.AsEnumerable()
                               where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString()
                               orderby t.Field<string>("Y_PSTN").GetInt()
                               select new { CBO_NAME = t.Field<string>("Y_PSTN"), CBO_CODE = t.Field<string>("Y_PSTN")}).Distinct().ToList();

            cboColumn.ItemsSource = queryColumn.ToDataTable().Copy().AsDataView();
            cboColumn.SelectedIndex = 0;
        }

        private void GetLayerList()
        {
            var queryLayer = (from t in _dtRowColumnLayer.AsEnumerable()
                              where t.Field<string>("X_PSTN") == cboRow.SelectedValue.GetString() 
                              && t.Field<string>("Y_PSTN") == cboColumn.SelectedValue.GetString()
                              orderby t.Field<string>("Z_PSTN").GetInt()
                              select new { CBO_NAME = t.Field<string>("Z_PSTN"), CBO_CODE = t.Field<string>("Z_PSTN") }).ToList();

            cboLayer.ItemsSource = queryLayer.ToDataTable().Copy().AsDataView();
            cboLayer.SelectedIndex = 0;
        }

        private void SetIssueReservation()
        {
            IssueReservation_TBox();
        }

        /// <summary>
        /// T-Box STK 출고
        /// </summary>
        private void IssueReservation_TBox()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName;

                bizRuleName = "BR_MCS_REG_TBOX_STK_PSR_PSP_NJ";

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add("INDATA");
                indataSet.Tables.Add("IN_RACK");

                indataSet.Tables["INDATA"].Columns.Add("SRCTYPE", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("TO_AREA", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("EQPTID", typeof(string));
                indataSet.Tables["INDATA"].Columns.Add("USERID", typeof(string));

                indataSet.Tables["IN_RACK"].Columns.Add("EQPTID", typeof(string));
                indataSet.Tables["IN_RACK"].Columns.Add("FROM_RACK_ID", typeof(string));

                DataTable dtEqpt = _dtRackInfo.DefaultView.ToTable(true, new string[] { "EQPTID" });
                DataTable dtRack = _dtRackInfo.DefaultView.ToTable(true, new string[] { "EQPTID", "RACK_ID" });

                foreach (DataRow r in dtEqpt.Rows)
                {
                    DataRow dr = indataSet.Tables["INDATA"].NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["TO_AREA"] = cboArea.SelectedValue;
                    dr["EQPTID"] = r["EQPTID"].ToString();
                    dr["USERID"] = LoginInfo.USERID;

                    indataSet.Tables["INDATA"].Rows.Add(dr);
                }

                foreach (DataRow r in dtRack.Rows)
                {
                    DataRow dr = indataSet.Tables["IN_RACK"].NewRow();
                    dr["EQPTID"] = r["EQPTID"].ToString();
                    dr["FROM_RACK_ID"] = r["RACK_ID"].ToString();

                    indataSet.Tables["IN_RACK"].Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_RACK", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

                    if (string.Equals(tabItem, "TabRackMove"))
                    {

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectRowColumnlayerInfo(string equipmentCode)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_BY_XYZ_PSTN";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = equipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    _dtRowColumnLayer = searchResult.Copy();

                    var queryRow = (from t in _dtRowColumnLayer.AsEnumerable()
                            orderby t.Field<string>("X_PSTN").GetInt()
                            select new { CBO_CODE = t.Field<string>("X_PSTN")
                            , CBO_NAME = t.Field<string>("X_PSTN") }).Distinct().ToList()
                        ;
                    cboRow.ItemsSource = queryRow.ToDataTable().Copy().AsDataView();
                    cboRow.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationIssueReservation()
        {
            return true;
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        { 
            const string bizRuleName = "DA_BAS_SEL_SHIPTO_INFO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);
        }

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
    }
}
 
