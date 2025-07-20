/*************************************************************************************
 Created Date : 2019.08.14
      Creator : 정문교
   Decription : 원자재관리 - Foil 공급,반품 현황 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.14  정문교    : 최초생성
  2022.05.10  정재홍    : C20211213-000307 - Foil 입고수량 컬럼 추가
  2024-05-16  남재현    : E20240417-000971 - Port ID 컬럼 추가.

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_102 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_102()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgHistory);
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent);

            //극성
            string[] sFilter = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboEltrTypeCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //설비
            SetEquipmentCombo(cboEquipment);

            //상태
            SetReqStatCodeCombo(cboReqStatCode, rdoRequest.Tag.ToString(), null);
        }

        private void SetControl(bool isVisibility = false)
        {
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboEltrTypeCode.SelectedValueChanged += cboEltrTypeCode_SelectedValueChanged;
            rdoRequest.Checked += rdoRequest_Checked;
            rdoReturn.Checked += rdoReturn_Checked;

            GridColumnVisibility(false);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControls();
            InitializeGrid();
            InitCombo();
            SetControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }

        private void cboEltrTypeCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }

        private void rdoRequest_Checked(object sender, RoutedEventArgs e)
        {
            //상태
            SetReqStatCodeCombo(cboReqStatCode, rdoRequest.Tag.ToString(), null);
        }

        private void rdoReturn_Checked(object sender, RoutedEventArgs e)
        {
            //상태
            SetReqStatCodeCombo(cboReqStatCode, null, rdoReturn.Tag.ToString());
        }

        /// <summary>
        /// dgHistory Event
        /// </summary>
        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Foreground 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("MTRL_ELTR_TYPE_NAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        /// <summary>
        /// 요청 이력 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchProcess();
        }

        /// <summary>
        /// 엑셀
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationExcel())
                    return;

                new ExcelExporter().Export(dgHistory);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 설비 콤보
        /// </summary>
        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID
                                    , string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString()
                                    , Process.COATING
                                    , cboEltrTypeCode.SelectedValue.ToString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        /// <summary>
        /// 상태 콤보
        /// </summary>
        private void SetReqStatCodeCombo(C1ComboBox cbo, string attr2, string attr3)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE2", "ATTRIBUTE3" };
            string[] arrCondition = { LoginInfo.LANGID, "MTRL_SPLY_REQ_STAT_CODE", attr2, attr3 };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        /// <summary>
        /// 공급,반품 이력 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                // Clear
                InitializeGrid();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateTo);
                newRow["AREAID"] = cboArea.SelectedValue.ToString();
                newRow["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                newRow["MTRL_ELTR_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboEltrTypeCode.SelectedValue.ToString()) ? null : cboEltrTypeCode.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue?? null;
                newRow["MTRL_SPLY_REQ_STAT_CODE"] = cboReqStatCode.SelectedValue ?? null;
                newRow["MTRL_SPLY_REQ_TYPE_CODE"] = (bool)rdoRequest.IsChecked ? rdoRequest.Tag.ToString() : rdoReturn.Tag.ToString();
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_MTR_SEL_MATERIAL_REQUEST_HISTORY_FOIL", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);

                        if ((bool)rdoRequest.IsChecked)
                            GridColumnVisibility(true);
                        else
                            GridColumnVisibility(false);
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

        #region Function

        #region [Validation]
        private bool ValidationSearch()
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private bool ValidationExcel()
        {
            if (dgHistory.Rows.Count - dgHistory.TopRows.Count - dgHistory.BottomRows.Count == 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
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

        private void GridColumnVisibility(bool isVisibility = false)
        {
            if (isVisibility)
            {
                dgHistory.Columns["PRJT_NAME"].Visibility = Visibility.Visible;
            }
            else
            {
                dgHistory.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
