/*************************************************************************************
 Created Date : 2018.10.26
      Creator : 
   Decription : 전극 슬러리 제품등록
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.26  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
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
    public partial class CMM_ELEC_SLURRY_PROD : C1Window, IWorkArea
    {

        #region Initialize
        private string _PROCID = string.Empty;
        private string _EQSGID = string.Empty;

        private string _ELEC_TYPE = string.Empty;
        private string _PROD_NAME = string.Empty;

        public string _GetProductName
        {
            get { return _PROD_NAME; }
        }

        public CMM_ELEC_SLURRY_PROD()
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
            _EQSGID = Util.NVC(tmps[1]);

            this.InitCombo();
        }

        private void btnSelect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Validation
            if (cboElecType.SelectedIndex < 0 || cboElecType.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1467");  // 극성을 선택 하세요.
                return;
            }

            if (cboProdName.SelectedIndex <= 0 || cboProdName.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1894");  // 제품을 선택 하세요.
                return;
            }

            // Result
            _ELEC_TYPE = Util.NVC(cboElecType.Text);
            _PROD_NAME = Util.NVC(cboProdName.SelectedValue);

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void cboElecType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            setDataTable();
        }
        #endregion

        #region UserMethod
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "COMMCODE");
        }

        private void setDataTable()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            dr["PROCID"] = Process.MIXING;
            dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_SLURRY_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            DataRow dRow = dtResult.NewRow();
            dRow["CBO_CODE"] = "SELECT";
            dRow["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(dRow, 0);
            cboProdName.ItemsSource = dtResult.Copy().AsDataView();
            cboProdName.SelectedIndex = 0;
        }

        #endregion


    }
}
