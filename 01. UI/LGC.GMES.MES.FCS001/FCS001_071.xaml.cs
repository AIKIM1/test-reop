/*************************************************************************************
 Created Date : 2020.12.07
      Creator : kang Dong Hee
   Decription : 작업조건 Report
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.07  NAME    : Initial Created
  2021.04.09  KDH     : 조회조건 AREAID 추가 및 화면간 이동 시 초기화 현상 제거
  2023.02.14  박승렬  : Route Report 화면과 LINQ (Route Report에서 MouseDoubleClick 이벤트로 데이터 조회) 
  2024.11.30  배준호  : 불필요 로직 제거 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_071 : UserControl , IWorkArea
    {
        #region [Declaration & Constructor]

        private string _sROUTID; //2023.02.14 Route Report 화면과 LINQ 
        private string _sMDLLOT_ID; //2023.02.14 Route Report 화면과 LINQ 
        #endregion

        #region [Initialize]
        public FCS001_071()
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
            object[] parameters = this.FrameOperation.Parameters;;
            if (parameters != null && parameters.Length >= 1)
            {
                _sROUTID = Util.NVC(parameters[0]);
                _sMDLLOT_ID = Util.NVC(parameters[1]);

                cboModel.SelectedValue = _sMDLLOT_ID;
                cboRoute.SelectedValue = _sROUTID;
                
                GetList();
            } //2023.02.14 Route Report 화면과 LINQ 

            this.Loaded -= UserControl_Loaded; //2021.04.09 화면간 이동 시 초기화 현상 제거
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboModelChild = { cboRoute };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "MODEL", cbChild: cboModelChild);

            C1ComboBox[] cboRouteParent = { cboModel };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_EX", cbParent: cboRouteParent);
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
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_RECIPE_REPORT", "RQSTDT", "RSLTDT", dtRqst);
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

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
    }
}
