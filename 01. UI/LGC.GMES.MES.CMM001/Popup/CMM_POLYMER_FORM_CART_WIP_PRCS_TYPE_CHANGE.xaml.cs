/*************************************************************************************
 Created Date : 2018.04.28
      Creator : 정문교
   Decription : 대차 재공처리 유형 변경
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;          // 공정코드
        private string _procName = string.Empty;        // 공정명
        private string _eqptID = string.Empty;          // 설비코드
        private string _wipPrcsTypeCode = string.Empty;    // 재공처리유형

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_WIP_PRCS_TYPE_CHANGE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetParameters();
                SetControl();
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _wipPrcsTypeCode = tmps[5] as string;

            txtCartID.Text = tmps[4] as string;
            txtWipPrcsTypeBefore.Text = tmps[6] as string;
        }
        private void SetControl()
        {
            SetGridCartList();
        }
        private void SetCombo()
        {
            SetWipPrcsTypeCombo();
            // 재공처리 유형이 있는경우 해당 재공처리 유형은 콤보에서 제외 한다.
            if (_wipPrcsTypeCode != null && !string.IsNullOrWhiteSpace(_wipPrcsTypeCode))
            {
                DataTable dt = DataTableConverter.Convert(cboWipPrcsType.ItemsSource);
                dt.Select("CBO_CODE = '" + _wipPrcsTypeCode + "'").ToList<DataRow>().ForEach(row => row.Delete());
                cboWipPrcsType.ItemsSource = dt.Copy().AsDataView();
            }

            SetInspectorCombo();
        }
        #endregion

        #region [변경]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChange())
                return;

            // 변경하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangeProcess();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void SetWipPrcsTypeCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_COMMONCODE_FO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "WIP_PRCS_TYPE_CODE", "WH" };
            string selectedValueText = cboWipPrcsType.SelectedValuePath;
            string displayMemberText = cboWipPrcsType.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboWipPrcsType, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SetInspectorCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_INSPECTOR_PC";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, _eqptID };
            string selectedValueText = cboWorkUser.SelectedValuePath;
            string displayMemberText = cboWorkUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboWorkUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// Cart List
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 변경
        /// </summary>
        private void ChangeProcess()
        {
            try
            {
                ShowLoadingIndicator();

                //DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);

                // DATA SET
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                newRow["WIP_PRCS_TYPE_CODE"] = Util.NVC(cboWipPrcsType.SelectedValue);
                newRow["UPDUSER"] = Util.NVC(cboWorkUser.SelectedValue);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_UPD_CART_WIP_PRCS_TYPE_CHANGE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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

        private bool ValidationChange()
        {
            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
            DataRow[] dr = dt.Select("CTNR_ID = '" + txtCartID.Text + "'");

            if (dr.Length == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (cboWipPrcsType.SelectedValue == null || cboWipPrcsType.SelectedValue.ToString().Equals("SELECT"))
            {
                // %1(을)를 선택하세요.
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("재공처리유형");

                Util.MessageValidation("SFU4925", parameters);
                return false;
            }

            if (cboWorkUser.SelectedValue == null || cboWorkUser.SelectedValue.ToString().Equals("SELECT"))
            {
                // 작업자 정보를 입력하세요.
                Util.MessageValidation("SFU4201");
                return false;
            }

            return true;
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