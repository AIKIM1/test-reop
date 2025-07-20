/********************************************************************************************************
 Created Date : 2020.11.03
      Creator : 김태균
   Decription : Aging 정보이상 Rack 현황
----------------------------------------------------------------------------------------------------------
 [Change History]
 2020.11.10  DEVELOPER : Initial Created.
 2022.07.21  이정미    : 조회 시, 분기로직 추가로 호출 BIZ 변경 
*********************************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_015()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilterEqpType = { "FORM_AGING_TYPE_CODE" };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilterEqpType);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgEqpStatus_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!string.IsNullOrEmpty(cboEqpKind.SelectedValue.ToString()))
                {
                    dr["CMCODE"] = cboEqpKind.SelectedValue.ToString();
                }
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_ABNOMAL_RACK_MB", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgEqpStatus, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
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
