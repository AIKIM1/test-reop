/*************************************************************************************
 Created Date : 2018.06.04
      Creator : 
   Decription : 전극 샘플링 제품 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.04  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_SAMPLING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SAMPLING_PROD : C1Window, IWorkArea
    {

        #region Initialize
        private string _PROCID = string.Empty;
        private string _ELEC_TYPE = string.Empty;
        private string _PRJT_NAME = string.Empty;
        private string _PROD_NAME = string.Empty;

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

        public CMM_ELEC_SAMPLING_PROD()
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

            _PROCID = Util.NVC(tmps[0]);

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
            //this.SetConvInfo();

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
            _combo.SetCombo(cboProdName, CommonCombo.ComboStatus.SELECT, cbParent: cboProdParent, sCase: "PRJT_NAME_BY_PRODCODE");

        }
        #endregion

    }
}
