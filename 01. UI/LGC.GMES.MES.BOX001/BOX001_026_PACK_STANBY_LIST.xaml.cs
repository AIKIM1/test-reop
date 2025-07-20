/*************************************************************************************
 Created Date : 2019.04.19
      Creator : 이제섭
   Decription : 포장 대기 BOX 조회 (CNA 종이 박스 포장기 Pjt)
--------------------------------------------------------------------------------------
 [Change History]
 2019.04.19 이제섭 : Initial Created.
    
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_205_ADD_LABEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_026_PACK_STANBY_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public bool QueryCall { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private bool _load = true;

        Util _util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

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

        public BOX001_026_PACK_STANBY_LIST()
        {
            InitializeComponent();
        }

        #endregion


        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                InitControl();
                InitCombo();

                _load = false;
            }
        }

        private void InitializeUserControls()
        {

        }

        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;


        }
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            String[] sFilter = { "PACK_WRK_TYPE_CODE" };

            combo.SetCombo(cboWrkType, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

        }

        #endregion
        #region 
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod  

        /// <summary>
        /// Box List
        /// </summary>
        private void GetBoxInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PACK_WRK_TYPE_CODE"] = cboWrkType.SelectedValue.ToString() == "" ? null : cboWrkType.SelectedValue.ToString();
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_STANBY_LIST_CP", "INDATA", "OUTDATA", RQSTDT);

                Util.GridSetData(dgbox, dtResult, FrameOperation, true);

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

       
        #endregion

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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetBoxInfo();
        }
    }
}
