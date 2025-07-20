/*************************************************************************************
 Created Date :
      Creator :
   Decription : 투입요청서 상세
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  
 [미적용 사항]
     1. 화면 종료 시 확인정보 Table Update 처리 

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_006_LOTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_002_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public string _REQID = string.Empty;
        public string _EQPTID = string.Empty;
        bool ChekRequest = false;

        Util _Util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ELEC001_002_DETAIL()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {

        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            _REQID = Util.NVC(tmps[0]);
            //_EQPTID = Util.NVC(tmps[1]);

            InitializeControls();
            GetRequestList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if(ChekRequest)
            {
                UpdateChkRequest();
            }
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }
        #endregion

        #region Mehod
        private void GetRequestList()
        {
            try
            {
                Util.gridClear(dgMaterialList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["REQ_ID"] = _REQID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_DETAIL", "INDATA", "RSLTDT", IndataTable);
                if(dtMain == null || dtMain.Rows.Count <= 0)
                {
                    return;
                }
                dgMaterialList.ItemsSource = DataTableConverter.Convert(dtMain);
                txtRemark.Text = dtMain.Rows[0]["NOTE"].ToString();
                _EQPTID = dtMain.Rows[0]["EQPTID"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void UpdateChkRequest()
        {
            try
            {
                Util.gridClear(dgMaterialList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["REQ_ID"] = _REQID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_TB_SFC_RMTRL_INPUT_REQ_CHK_FLAG", "INDATA", "RSLTDT", IndataTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void chkRequest_Checked(object sender, RoutedEventArgs e)
        {
            this.ChekRequest = true;
        }

        private void chkRequest_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ChekRequest = false;
        }
    }
}
