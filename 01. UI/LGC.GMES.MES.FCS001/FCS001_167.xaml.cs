/*************************************************************************************
 Created Date : 2023.11.10
      Creator : 김태오
   Decription : 物流效率分配list (ESNA 법인 별도 화면)
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.10 김태오 : Initial Created.  
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_167 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private object oPortid;

        private DataTable _dtCopy; //2023.02.14 MouseDoubleClick 이벤트 추가 

        public FCS001_167()
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

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            _combo.SetCombo(cboGroup, CommonCombo_Form.ComboStatus.SELECT, sCase: "GROUP");

            //cboGroup.SelectedIndexChanged += cboGroup_SelectedIndexChanged;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitButton();
            this.Loaded -= UserControl_Loaded;                                   

            
        }

        private void InitButton()
        {
            if (ValidationApproval()) // 동별 권한 W / R 확인 후 Write 기능 조회.
            {
                btnSave.Visibility = Visibility.Visible;
                dgOutStationList.Columns["MAX_INPUT_TRAY_QTY"].IsReadOnly = false;
                dgOutStationList.Columns["RACK_USE_RATE"].IsReadOnly = false;
                //dgOutStationList.Columns["CURR_PASS_TRAY_QTY"].IsReadOnly = false;
            }
        }

        private bool ValidationApproval()
        {
            int iResult = 0;
            const string bizRuleName = "DA_BAS_SEL_AUTHORITYMENU_BY_ID";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("MENUID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["MENUID"] = "SFU010181018";
            dr["USERID"] = LoginInfo.USERID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            iResult = int.Parse(dtResult.Rows[0]["ACCESS_COUNT"].ToString());


            if (dtResult != null && dtResult.Rows.Count > 0 && iResult > 0)  //결과값이 존재하고 , Write  권한이 하나라도 존재해야 함.
            {
                return true;
            }
            else
            {                    
                return false;
            }
          
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!cboGroup.Text.Equals("-SELECT-"))
            {
                GetList();
            }                
        }

        private void cboGroup_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (!cboGroup.SelectedValue.ToString().Equals(""))
            //if (cboGroup.Text == "-ALL-")
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgOutStationList.IsCheckedRow("CHK"))
                {
                    Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("DVRTR_VIRT_PORT_ID", typeof(string));
                dtRqst.Columns.Add("DVRTR_ZONE_ID", typeof(string));
                dtRqst.Columns.Add("MAX_INPUT_TRAY_QTY", typeof(string));
                dtRqst.Columns.Add("MAX_PASS_TRAY_QTY", typeof(string));
                dtRqst.Columns.Add("CURR_PASS_TRAY_QTY", typeof(string));
                dtRqst.Columns.Add("PC_NAME", typeof(string));
                dtRqst.Columns.Add("USER_PC_IP", typeof(string));
                dtRqst.Columns.Add("MDF_ID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));                
                dtRqst.Columns.Add("RACK_USE_RATE", typeof(string));

                for (int i = 0; i < dgOutStationList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["DVRTR_VIRT_PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "DVRTR_VIRT_PORT_ID"));
                        dr["DVRTR_ZONE_ID"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "DVRTR_ZONE_ID"));
                        dr["MAX_INPUT_TRAY_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "MAX_INPUT_TRAY_QTY"));
                        dr["MAX_PASS_TRAY_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "MAX_PASS_TRAY_QTY"));
                        dr["CURR_PASS_TRAY_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "CURR_PASS_TRAY_QTY"));
                        dr["PC_NAME"] = LoginInfo.PC_NAME;
                        dr["USER_PC_IP"] = LoginInfo.USER_IP;
                        dr["MDF_ID"] = SRCTYPE.SRCTYPE_UI;
                        dr["USERID"] = LoginInfo.USERID;
                        //2023.11.13 김태오 RATAIO 추가
                        dr["RACK_USE_RATE"] = Util.NVC(DataTableConverter.GetValue(dgOutStationList.Rows[i].DataItem, "RACK_USE_RATE"));                        

                        dtRqst.Rows.Add(dr);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_EQP_ZONE_CB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null)
                {
                    if (dtRslt.Rows[0]["RESULT"].ToString().Equals("1"))
                    {
                        Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        GetList();
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                        return;
                    }
                }
                else
                {
                    Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgOutStationList);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgOutStationList);
        }

        private void dgOutStationList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgOutStationList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgOutStationList);

                dgOutStationList.ItemsSource = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQP_ZONE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["EQP_ZONE"] = Util.NVC(cboGroup.SelectedValue);
                dr["EQP_ZONE"] = Util.GetCondition(cboGroup);

                dtRqst.Rows.Add(dr);

                //ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_EFF_DIV_ZONE_LIST", "RQSTDT", "RSLTDT", dtRqst);

                dtRslt.Columns.Add("CHK", typeof(bool));
                dtRslt.Columns.Add("FLAG", typeof(string));

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["FLAG"] = "N";
                }

                Util.GridSetData(dgOutStationList, dtRslt, FrameOperation, true);

                //_dtCopy = dtRslt.Copy(); //2023.02.14 MouseDoubleClick 이벤트 추가 
                //dgOutStationList.ItemsSource = DataTableConverter.Convert(_dtCopy); //2023.02.14 MouseDoubleClick 이벤트 추가 

                //HiddenLoadingIndicator();
            }
            catch (Exception e)
            {
                Util.MessageException(e);
                //HiddenLoadingIndicator();
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0 && !dt.Columns.Contains("FLAG"))
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["PORT_ID"] = Util.GetCondition(cboGroup, bAllNull: true);
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["FLAG"] = "N";
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["PORT_ID"] = Util.GetCondition(cboGroup, bAllNull: true);
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["FLAG"] = "N";
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - 1].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
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

        private void dgOutStationList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                int iMaxLoadingRate = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RACK_USE_RATE_STATUS"));

                dgOutStationList.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "MAX_INPUT_TRAY_QTY" || e.Cell.Column.Name == "RACK_USE_RATE")
                    {
                        //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }                    
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    // 2023.11.15 김태오 S  추후 적재율 현황  컬럼 특정 % 이상일 경우 아래 IF문 주석 제거 후 사용.
                    /*
                    if(e.Cell.Column.Name.ToString() == "RACK_USE_RATE_STATUS" && iMaxLoadingRate >= 97)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    */
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
