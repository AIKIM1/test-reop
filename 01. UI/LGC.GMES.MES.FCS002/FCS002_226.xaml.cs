/*************************************************************************************
 Created Date : 2023.05.31
      Creator : 
   Decription : Aging 적재 관리
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.31  DEVELOPER : 강동희

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
    public partial class FCS002_226 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        private string _sAGING_TYPE = string.Empty;
        private string _sSCLINE = string.Empty;
        private string _sROW = string.Empty;

        public FCS002_226()
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
         

            InitCombo();
            
            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //string[] sFilter1 = { "FORM_AGING_TYPE_CODE", string.Empty };
            //_combo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter1);

            // 동
            _combo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ALLAREA");


            C1ComboBox[] cboLineChild = { cboModel, cboProcGrpCode }; //2021.04.09 Line별 공정그룹 Setting으로 수정 START
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOper };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            // 공정 그룹
            C1ComboBox[] cboProcGrParent = { cboLine }; //2021.04.09 Line별 공정그룹 Setting으로 수정 START
            C1ComboBox[] cboProcGrChild = { cboOper }; //2021.04.05 공정그룹 추가
            string[] sFilter = { "PROC_GR_CODE_MB", LoginInfo.CFG_AREA_ID ,"3" };
            _combo.SetCombo(cboProcGrpCode, CommonCombo_Form_MB.ComboStatus.NONE, cbChild: cboProcGrChild, cbParent: cboProcGrParent, sFilter: sFilter, sCase: "PROCGRP_BY_LINE");

            //2021.04.09 Line별 공정그룹 Setting으로 수정 END
            C1ComboBox[] cboOperParent = { cboRoute, cboProcGrpCode };
            _combo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE_OP", cbParent: cboOperParent); //2021.04.05 공정그룹 추가


           
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ATTR1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["ATTR1"] = "3";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                cboAgingType.DisplayMemberPath = "CBO_NAME";
                cboAgingType.SelectedValuePath = "CBO_CODE";

                DataRow d = dtResult.NewRow();
                d["CBO_NAME"] = "-SELECT-";
                d["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(d, 0);

                cboAgingType.ItemsSource = dtResult.Copy().AsDataView();
                
                if (cboAgingType.Items.Count > 1)
                {
                    cboAgingType.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            
        }

        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);
                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void cboAgingType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            GetCommonCode();

            cboSCLine.Text = string.Empty;
            string[] sFilter = { EQPT_GR_TYPE_CODE, LANE_ID };
            _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter); //20210331 S/C 호기 필수 값으로 변경
        }

        private void cboSCLine_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            cboRow.Text = string.Empty;
            object[] objParent = { EQPT_GR_TYPE_CODE, LANE_ID, Util.GetCondition(cboSCLine) };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AGING_ROW", objParent: objParent);
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTrayList == null || dgTrayList.CurrentRow == null || !dgTrayList.Columns.Contains("CSTID"))
                {
                    return;
                }

                if (dgTrayList.CurrentRow != null && (dgTrayList.CurrentColumn.Name.Equals("CSTID")))
                {
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "CSTID")).ToString(); // TRAY ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID")).ToString(); // LOTID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void dgTraySpcl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTraySpcl == null || dgTraySpcl.CurrentRow == null || !dgTraySpcl.Columns.Contains("CSTID"))
                {
                    return;
                }

                if (dgTraySpcl.CurrentRow != null && (dgTraySpcl.CurrentColumn.Name.Equals("CSTID")))
                {
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTraySpcl.CurrentRow.DataItem, "CSTID")).ToString(); // TRAY ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTraySpcl.CurrentRow.DataItem, "LOTID")).ToString(); // LOTID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG"))))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).ToString().Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).ToString().Equals("P"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }

                    if (e.Cell.Column.Name.ToString() == "CSTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANE_ID"] = LANE_ID;
                dr["EQPT_GR_TYPE_CODE"] = EQPT_GR_TYPE_CODE;
                dr["EQPTID"] = Util.GetCondition(cboSCLine, bAllNull: true);
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                if (txtLotId.Text.Length < 8)
                {
                    Util.Alert("SFU4075");
                    return;
                }
                    dr["PROD_LOTID"] = txtLotId.Text;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_TRAY_MGR_MANUAL_OUT_MB", "RQSTDT", "RSLTDT", dtRqst);
                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_TRAY_SPCL_MANUAL_OUT_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgTrayList, dtRslt, FrameOperation, true);
                Util.GridSetData(dgTraySpcl, dtRslt2, FrameOperation, true);
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

        private void btnEfficiency_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                // sample 출고로 변경 -240322

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));

                //DataRow drIn = dtInData.NewRow();
                //drIn["USERID"] = LoginInfo.USERID;


                //DataTable dtInLot = ds.Tables.Add("INLOT");
                //dtInLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        // 일괄선택시 합계행 제외
                        if (string.IsNullOrEmpty(sLotid))
                            continue;

                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtInData.Rows.Add(drIn);

                      
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }
                
                //출고 하시겠습니까?
                Util.MessageConfirm("FM_ME_0538", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_SAMPLE_OUT_MB", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "0")
                                {
                                    //출고 지시를 완료하였습니다.
                                    Util.MessageInfo("SFU1747");
                                    GetList();
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "-1")
                                {
                                    //Sample 출고는 Aging 공정에서만 가능합니다. 
                                    Util.Alert("FM_ME_0444");
                                }

                              
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSpecial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("SPCL_FLAG", typeof(string));


                for (int i = 0; i < dgTraySpcl.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[i].DataItem, "LOTID"));
                        drIn["SPCL_FLAG"] = "Y";
                        dtInData.Rows.Add(drIn);

                    
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }
                //Sample 출고 하시겠습니까?
                Util.MessageConfirm("FM_ME_0538", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_AGING_TRAY_MGR_OUT_MB", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "0")
                                {
                                    // 출고 지시를 완료하였습니다.
                                    Util.MessageInfo("SFU1747");
                                    GetList();
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "-1")
                                {
                                    //  // 요청에 실패했습니다.
                                    Util.Alert("FM_ME_0185");
                                }

                             
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTrayList.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "RACK_ID")).Equals(string.Empty)) return;

         
                    SetBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "RACK_ID")), true);
        
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTrayList.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "RACK_ID")).Equals(string.Empty)) return;

                    SetBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "RACK_ID")), false);
          
        }

        private void SetBoxIdCheck(string boxId, bool isCheck)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "RACK_ID")).Equals(boxId))
                {
                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", isCheck);
                }
            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTraySpcl.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[rowIndex].DataItem, "RACK_ID")).Equals(string.Empty)) return;


            SetSPCLBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[rowIndex].DataItem, "RACK_ID")), true);
        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTraySpcl.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[rowIndex].DataItem, "RACK_ID")).Equals(string.Empty)) return;

            SetSPCLBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[rowIndex].DataItem, "RACK_ID")), false);

        }
        private void SetSPCLBoxIdCheck(string boxId, bool isCheck)
        {
            for (int i = 0; i < dgTraySpcl.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTraySpcl.Rows[i].DataItem, "RACK_ID")).Equals(boxId))
                {
                    DataTableConverter.SetValue(dgTraySpcl.Rows[i].DataItem, "CHK", isCheck);
                }
            }
        }
    }
}
