/*************************************************************************************
 Created Date : 2020.11.03
      Creator : 김태균
   Decription : Aging 한계시간 초과 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.03  DEVELOPER : Initial Created.
  2022.05.16  이정미 : 조회시 INDATA 변경(LANGID)
  2023.08.14  이의철 : Tray ID 에 dummy, special color 변경 적용



 
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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_014()
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOper };
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            C1ComboBox[] cboOperParent = { cboRoute };
            _combo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent);

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgAgingLimit_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Color 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    string sDUMMY_FLAG = Util.NVC(dr.Row["DUMMY_FLAG"]);
                    string sSPCL_FLAG = Util.NVC(dr.Row["SPECIAL_YN"]);

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        if (sDUMMY_FLAG.Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        if (sSPCL_FLAG.Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }                    
                }
                //Color 변경
                //if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                //{
                    //if (e.Cell.Column.Name.Equals("CSTID"))
                    //{
                    //    //Tray ID 에 dummy, special color 변경 적용
                    //    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DUMMY_FLAG")).Equals("Y"))
                    //    {
                    //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //    }

                    //    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "SPECIAL_YN")).Equals("Y"))
                    //    {
                    //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    //        e.Cell.Presenter.FontFamily = new FontFamily("맑은 고딕");
                    //        e.Cell.Presenter.FontWeight = FontWeights.Bold;                           
                    //    }                        
                    //}

                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DUMMY_FLAG")).ToString().Equals("Y"))
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Blue);
                    //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //}

                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPECIAL_YN")).ToString().Equals("Y"))
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
                    //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //}
                //}                
            }));
            
            #region OLD
            //for (int i = 0; i < fpsAgingLimit.ActiveSheet.RowCount; i++)
            //{
            //    if (fpsAgingLimit.GetValue(i, "DUMMY_YN").ToString().Equals("Y"))
            //    {
            //        fpsAgingLimit.ActiveSheet.Cells[i, 3].ForeColor = Color.Blue;
            //    }

            //    if (fpsAgingLimit.GetValue(i, "SPECIAL_YN").ToString().Equals("Y"))
            //    {
            //        fpsAgingLimit.ActiveSheet.Cells[i, 3].ForeColor = Color.Red;
            //        fpsAgingLimit.ActiveSheet.Cells[i, 3].Font = new Font("맑은 고딕", 9, FontStyle.Bold);
            //    }
            //    else if (fpsAgingLimit.GetValue(i, "SPECIAL_YN").ToString().Equals("Y"))
            //    {
            //        fpsAgingLimit.ActiveSheet.Cells[i, 3].ForeColor = Color.DarkOrange;
            //        fpsAgingLimit.ActiveSheet.Cells[i, 3].Font = new Font("맑은 고딕", 9, FontStyle.Bold);
            //    }
            //    else
            //    {
            //        fpsAgingLimit.ActiveSheet.Cells[i, 3].ForeColor = Color.Black;
            //    }

            //    if (fpsAgingLimit.GetValue(i, "TIME_ALARM").ToString().Equals("Y"))
            //        fpsAgingLimit.ActiveSheet.Rows[i].BackColor = Color.Pink;
            //    else
            //        fpsAgingLimit.ActiveSheet.Rows[i].BackColor = Color.White;
            //} 
            #endregion
        }

        private void dgAgingLimit_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgAgingLimit.ItemsSource == null)
                    return;
                
                if (sender == null)
                    return;

                if (dgAgingLimit.CurrentRow != null && dgAgingLimit.CurrentColumn.Name.Equals("CSTID"))
                {
                    //FCS001_014_AGING_LIMIT_TIME_OVER popupAgingLimitTimeOver = new FCS001_014_AGING_LIMIT_TIME_OVER();
                    //popupAgingLimitTimeOver.FrameOperation = this.FrameOperation;

                    //object[] parameters = new object[2];
                    //parameters[0] = Util.NVC(DataTableConverter.GetValue(dgAgingLimit.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    //parameters[1] = Util.NVC(DataTableConverter.GetValue(dgAgingLimit.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    //C1WindowExtension.SetParameters(popupAgingLimitTimeOver, parameters);

                    //grdMain.Children.Add(popupAgingLimitTimeOver);
                    //popupAgingLimitTimeOver.BringToFront();

                    //Tray 조회
                    object[] parameters = new object[6];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgAgingLimit.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgAgingLimit.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    this.FrameOperation.OpenMenu("SFU010710010", true, parameters);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("NDT", typeof(string));
                dtRqst.Columns.Add("AGING_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dr["NDT"] = (chkDegas.IsChecked == true ? "5" : null);
                dr["AGING_YN"] = (chkAging.IsChecked == true ? "Y" : null);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_LIMIT_TRAY_LIST", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgAgingLimit, dtRslt, FrameOperation, true);
                
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
