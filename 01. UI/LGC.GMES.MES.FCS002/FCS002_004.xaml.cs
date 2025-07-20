/*************************************************************************************
 Created Date : 2022.12.20
      Creator : 강동희
   Decription : 충방전기 가동현황
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.20  DEVELOPER : 강동희
  2023.06.10  강동희 : Line 선택에 따른 Box ID List Setting 부분 수정
**************************************************************************************/

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
    public partial class FCS002_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        public FCS002_004()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //콤보 셋팅
            InitCombo();

            InitControl();
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboBoxID };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLineChild);
            
            C1ComboBox[] cboEqpParent = { cboLane };
            string[] sFilter1 = {"1", null, "M" };
            _combo.SetCombo(cboBoxID, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEMB", cbParent: cboEqpParent, sFilter: sFilter1);
        }

        private void InitControl()
        {
            dtpDate.SelectedDateTime = DateTime.Now.AddDays(-1);
        }

        #endregion

        #region Event
        private void btnSearchLane_Click(object sender, RoutedEventArgs e)
        {
            GetListLane();
        }

        private void cboSearchBox_Click(object sender, RoutedEventArgs e)
        {
            GetListBox();
        }
        #endregion

        #region Method
        private void GetListLane()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ALL_FORMATION_STATUS_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgLane, dtRslt, FrameOperation, true);
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

        private void GetListBox()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("ACTDTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboBoxID, bAllNull: true);
                dr["ACTDTTM"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_RATE_OPER_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgBox, dtRslt, FrameOperation, true);
                string[] sColumnName = new string[] { "LANE_NAME"};
                _Util.SetDataGridMergeExtensionCol(dgLane, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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


        // 라인ID 다중선택 콤보박스 초기화        

        #endregion

    }
}
