/*************************************************************************************
 Created Date : 2019.05.28
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 불량포트 CELL 정보
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.28  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ASSY_EQPT_DFCT_CELL_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_EQPT_DFCT_CELL_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        string _EqptID = string.Empty;
        string _PortID = string.Empty;
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

        public CMM_ASSY_EQPT_DFCT_CELL_INFO()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            SetParameters();
            SetControl();
            SearchProcess();
            Loaded -= C1Window_Loaded;

        }

        private void InitializeControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            txtLotID.Text = tmps[0].ToString();
            _EqptID = tmps[1].ToString();
            _PortID = tmps[2].ToString();
        }

        private void SetControl()
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Cell 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_CELL_DFCT_CLCT_HIST_INFO_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(String));
                dtRqst.Columns.Add("PORT_ID", typeof(String));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = txtLotID.Text;
                dr["EQPTID"] = _EqptID;
                dr["PORT_ID"] = _PortID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
