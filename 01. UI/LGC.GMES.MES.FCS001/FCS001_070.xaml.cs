/*************************************************************************************
 Created Date : 2020.12.16
      Creator : Kang Dong Hee
   Decription : Route Report
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.16  NAME    : Initial Created
  2021.04.09  KDH     : 조회조건 AREAID 추가
  2023.02.14  박승렬  : MouseDoubleClick 이벤트 추가 (선택한 ROW의 작업조건 Report 조회)
**************************************************************************************/
#define SAMPLE_DEV

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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_070 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private DataTable _dtCopy; //2023.02.14 MouseDoubleClick 이벤트 추가 

        public FCS001_070()
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
            CommonCombo_Form ComCombo = new CommonCombo_Form();
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.SELECT, sCase: "MODEL");

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
            if (!cboModel.SelectedValue.ToString().Equals("SELECT"))
            {
                btnSearch_Click(null, null);
            }
        }

        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgRouteReport);

                dgRouteReport.ItemsSource = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2021.04.09 조회조건 AREAID 추가
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2021.04.09 조회조건 AREAID 추가
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, sMsg: "FM_ME_0129");  //모델을 선택해주세요.
                dr["CMCDTYPE"] = "ROUT_TYPE_CODE";

                if (string.IsNullOrEmpty(dr["MDLLOT_ID"].ToString())) return;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_REPORT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgRouteReport, dtRslt, FrameOperation, true);

                
                _dtCopy = dtRslt.Copy(); //2023.02.14 MouseDoubleClick 이벤트 추가 
                dgRouteReport.ItemsSource = DataTableConverter.Convert(_dtCopy); //2023.02.14 MouseDoubleClick 이벤트 추가 

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

        private void dgRouteReport_MouseDoubleClick(object sender, MouseButtonEventArgs e) //2023.02.14 MouseDoubleClick 이벤트 추가 
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

                    FCS001_071 wndRunStart = new FCS001_071();
                    wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = ROUTID;
                            Parameters[1] = MDLLOT_ID;
                            this.FrameOperation.OpenMenu("SFU010730020", true, Parameters);
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
