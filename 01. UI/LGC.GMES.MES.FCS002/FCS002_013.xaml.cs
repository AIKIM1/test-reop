/*************************************************************************************
 Created Date : 2023.01.18
      Creator : KANG DONG HEE
   Decription : Aging Rack 예약현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2023.01.18  DEVELOPER : Initial Created.
**************************************************************************************/
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
    public partial class FCS002_013 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public FCS002_013()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Combo Setting
                InitCombo();
                this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE");

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "3" };
            C1ComboBox[] cboEqpKindChild = { cboRow };
            ComCombo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

            object[] oFilterRow = { cboEqpKind };
            ComCombo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AGING_ROW", objParent: oFilterRow);
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgAgingRes_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));
                dtRqst.Columns.Add("X_PSTN", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                dr["X_PSTN"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RESERVATION_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgAgingRes, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
