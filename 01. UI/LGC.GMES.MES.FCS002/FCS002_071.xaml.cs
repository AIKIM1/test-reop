/*************************************************************************************
 Created Date : 2022.12.13
      Creator : kang Dong Hee
   Decription : 작업조건 Report
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.13  NAME : Initial Created
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_071 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sModelID; //TRAY_ID
        private string _sRoutID; //TRAY_NO

        public string MODELID
        {
            set { this._sModelID = value; }
            get { return this._sModelID; }
        }

        public string ROUTID
        {
            set { this._sRoutID = value; }
            get { return this._sRoutID; }
        }

        #endregion

        #region [Initialize]
        public FCS002_071()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            //다른 화면에서 넘어온 경우
            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                MODELID = Util.NVC(parameters[0]);
                ROUTID = Util.NVC(parameters[1]);
                if (!string.IsNullOrEmpty(_sModelID)) { cboModel.SelectedValue = _sModelID; }
                if (!string.IsNullOrEmpty(_sRoutID)) { cboRoute.SelectedValue = _sRoutID; }
                btnSearch_Click(null, null);
            }

            this.Loaded -= UserControl_Loaded; //2021.04.09 화면간 이동 시 초기화 현상 제거
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            C1ComboBox[] cboModelChild = { cboRoute };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "MODEL", cbChild: cboModelChild);

            C1ComboBox[] cboRouteParent = { cboModel };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_EX", cbParent: cboRouteParent);
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgCondReport);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2021.04.09 조회조건 AREAID 추가
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2021.04.09 조회조건 AREAID 추가
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_RECIPE_REPORT_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgCondReport, dtRslt, FrameOperation, true);

                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "ROUTID" };
                    _Util.SetDataGridMergeExtensionCol(dgCondReport, sColumnName, DataGridMergeMode.VERTICAL);
                }
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

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void dgCondReport_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

    }
}
