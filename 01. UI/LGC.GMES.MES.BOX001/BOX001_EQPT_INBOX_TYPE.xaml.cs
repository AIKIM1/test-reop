using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_EQPT_INBOX_TYPE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_EQPT_INBOX_TYPE : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;  // 공정코드
        private string _eqptID = string.Empty;  // 설비코드
        private bool _load = true;

        public string EquipmentSegmentCode { get; set; }

        //<summary>
        //    Frame과 상호작용하기 위한 객체
        //</summary>    
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public BOX001_EQPT_INBOX_TYPE()
        {
            InitializeComponent();
        }
        #endregion


        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
            }
        }

        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[1] as string;

            // 설비 설정
            SetEquipmentCombo();

            // Inbox type
            string[] sFilter = { LoginInfo.CFG_AREA_ID, _procID };
            _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            // 설비에 설정된 Inbox Type
            SetEqptInboxType();

            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;

        }

        #region [설비 변경 시] >> Inbox Type이 변경
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipment.SelectedValue != null)
                SetEqptInboxType();

        }
        #endregion


        #region [설정]
        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSetting())
                return;

            // Inbox 유형을 설정 하시겠습니까?
            Util.MessageConfirm("SFU4122", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EqptInboxTypeSetting();
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


        #region [설비 콤보 설정]
        private void SetEquipmentCombo()
        {
            string EqsgID = EquipmentSegmentCode == null ? LoginInfo.CFG_EQSG_ID : EquipmentSegmentCode;

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, EqsgID, _procID, null };
            string selectedValueText = cboEquipment.SelectedValuePath;
            string displayMemberText = cboEquipment.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboEquipment, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            if (cboEquipment.Items.Count > 0)
            {
                if (_eqptID.Equals("SELECT"))
                    cboEquipment.SelectedIndex = 0;
                else
                    cboEquipment.SelectedValue = _eqptID;
            }
        }
        #endregion


        #region [설비 InBox 유형 조회]
        private void SetEqptInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue ?? "";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (cboInboxType.Items.Count > 0)
                        cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE"].ToString();
                }

                if (cboInboxType.SelectedValue == null)
                    cboInboxType.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion



        /// <summary>
        /// Inbox Type 변경
        /// </summary>
        private void EqptInboxTypeSetting()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INBOX_TYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["INBOX_TYPE"] = cboInboxType.SelectedValue;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_UPD_EQPT_INBOX_TYPE", "INDATA", null, inTable, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });


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

        #region [Validation]
        private bool ValidateSetting()
        {
            if (cboEquipment.SelectedValue == null)
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.Equals("SELECT"))
            {
                // InBox를 선택해 주세요.
                Util.MessageValidation("SFU4005");
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
        #endregion

    }
}
