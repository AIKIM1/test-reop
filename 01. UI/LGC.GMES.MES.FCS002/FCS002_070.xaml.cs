/*************************************************************************************
 Created Date : 2022.12.13
      Creator : Kang Dong Hee
   Decription : Route Report
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.13  NAME : Initial Created
**************************************************************************************/
#define SAMPLE_DEV

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
    public partial class FCS002_070 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public FCS002_070()
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
            try
            {
                //Combo Setting
                InitCombo();
                this.Loaded -= UserControl_Loaded;
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
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "MODEL");

            cboModel.SelectedIndexChanged += cboModel_SelectedIndexChanged;
        }

        #endregion

        #region Event


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void cboModel_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //if (!cboModel.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    btnSearch_Click(null, null);
            //}
        }

        private void dgRouteReport_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                Util.gridClear(dgRouteReport);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, sMsg: "FM_ME_0129");  //모델을 선택해주세요.
                dr["CMCDTYPE"] = "ROUT_TYPE_CODE";

                if (string.IsNullOrEmpty(dr["MDLLOT_ID"].ToString())) return;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_REPORT_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgRouteReport, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception e)
            {
                Util.MessageException(e);
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

        private void cboModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboModel.SelectedValue.ToString().Equals("SELECT"))
            {
                btnSearch_Click(null, null);
            }
            else
            {
                Util.gridClear(dgRouteReport);
                //Util.GetCondition(cboModel, sMsg: "FM_ME_0129");  //모델을 선택해주세요.
            }
        }

        private void dgRouteReport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    DataRowView dataRow = (DataRowView)dgRouteReport.SelectedItem;
                    string ROUTID = dataRow.Row.ItemArray[0].ToString();
                    string MDLLOT_ID = dataRow.Row.ItemArray[3].ToString();

                    FCS002_071 wndRunStart = new FCS002_071();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = MDLLOT_ID;
                        Parameters[1] = ROUTID;
                        this.FrameOperation.OpenMenu("SFU010730560", true, Parameters);
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
