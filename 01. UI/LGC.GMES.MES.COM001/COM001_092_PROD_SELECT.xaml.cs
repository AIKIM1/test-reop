using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Text;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_092_PROD_SELECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_092_PROD_SELECT : C1Window, IWorkArea
    {
        #region Initialize
        private int _SETTING_GRID = -1;
        private string _PROCID = string.Empty;
        private string _ELEC_TYPE = string.Empty;
        private string _PRJT_NAME = string.Empty;
        private string _PROD_NAME = string.Empty;
        private string _LANE_QTY = string.Empty;
        private string _SKID_CONF = string.Empty;
        private string _HOT_ORDER = string.Empty;
        private string _CONVINFO = string.Empty;

        public int _GetSettingGrid
        {
            get { return _SETTING_GRID; }
        }

        public string _GetElecType
        {
            get { return _ELEC_TYPE; }
        }

        public string _GetPrjtName
        {
            get { return _PRJT_NAME; }
        }

        public string _GetProductName
        {
            get { return _PROD_NAME; }
        }

        public string _GetLaneQty
        {
            get { return _LANE_QTY; }
        }

        public string _GetSkidConfig
        {
            get { return _SKID_CONF; }
        }

        public string _GetHotOrder
        {
            get { return _HOT_ORDER; }
        }

        public string _GetConvInfo
        {
            get { return _CONVINFO; }
        }

        public COM001_092_PROD_SELECT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }
        #endregion
        #region Event
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _SETTING_GRID = Util.NVC_Int(tmps[0]);
            _PROCID = Util.NVC(tmps[1]);

            if (!string.Equals(_PROCID, Process.ELEC_STORAGE))
                cboLaneQty.IsEnabled = false;

            // Combo Setting
            this.InitCombo();
        }

        private void btnSelect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Validation
            if (cboElecType.SelectedIndex <= 0)
            {
                Util.MessageValidation("SFU1467");  // 극성을 선택 하세요.
                return;
            }

            if (cboPjtName.SelectedIndex <= 0)
            {
                Util.MessageValidation("SFU3478");  // PJT을 선택하세요.
                return;
            }

            if (cboProdName.SelectedIndex <= 0)
            {
                Util.MessageValidation("SFU1894");  // 제품을 선택 하세요.
                return;
            }

            // Result
            _ELEC_TYPE = Util.NVC(cboElecType.Text);
            _PRJT_NAME = Util.NVC(cboPjtName.SelectedValue);
            _PROD_NAME = Util.NVC(cboProdName.SelectedValue);
            _LANE_QTY = Util.NVC(cboLaneQty.SelectedValue);
            _SKID_CONF = chkSkid.IsChecked == true ? "Y" : "N";
            _HOT_ORDER = chkOrder.IsChecked == true ? "Y" : "N";
            this.SetConvInfo();

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region UserMethod
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "ELTR_TYPE_CODE" };
            C1ComboBox[] cboPrjtChild = { cboPjtName };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.SELECT, cbChild: cboPrjtChild, sFilter: sFilter, sCase: "COMMCODE");

            C1ComboBox[] cboPrjtParent = { cboElecType };
            C1ComboBox[] cboPrjtProd = { cboProdName };
            _combo.SetCombo(cboPjtName, CommonCombo.ComboStatus.SELECT, cbParent: cboPrjtParent, cbChild: cboPrjtProd, sCase: "PRJT_NAME_ELEC");

            C1ComboBox[] cboProdParent = { cboPjtName, cboElecType };
            C1ComboBox[] cboProdChild = { cboLaneQty };
            _combo.SetCombo(cboProdName, CommonCombo.ComboStatus.SELECT, cbParent: cboProdParent, cbChild: cboProdChild, sCase: "PRJT_NAME_BY_PRODCODE");

            C1ComboBox[] cboLaneParent = { cboProdName };
            _combo.SetCombo(cboLaneQty, CommonCombo.ComboStatus.ALL, cbParent: cboLaneParent, sCase: "PRDT_LANE");
        }

        private void SetConvInfo()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = _PROCID;
                Indata["PRODID"] = _PROD_NAME;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVING_CONV", "INDATA", "RSLTDT", IndataTable);

                if ( result != null && result.Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(result.Rows[0]["COMMON_MOVING_IN_OUT_CNT"])))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(ObjectDic.Instance.GetObjectName("가대"));
                    builder.Append(" : ");
                    builder.Append(Util.NVC(result.Rows[0]["COMMON_MOVING_IN_OUT_CNT"]));
                    builder.Append("/");
                    builder.Append(Util.NVC(result.Rows[0]["COMMON_MOVING_IN_CNT"]));
                    builder.Append(", ");
                    builder.Append(ObjectDic.Instance.GetObjectName("컨베이어"));
                    builder.Append(" : ");
                    builder.Append(Util.NVC(result.Rows[0]["CONV_MOVING_IN_OUT_CNT"]));
                    builder.Append("/");
                    builder.Append(Util.NVC(result.Rows[0]["CONV_MOVING_IN_CNT"]));

                    _CONVINFO = builder.ToString();
                }
            }
            catch (Exception ex) { }
        }
        #endregion
    }
}
