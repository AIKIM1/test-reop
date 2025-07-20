/*************************************************************************************
 Created Date : 2021.03.21
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

//#define TEST

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_084 : UserControl, IWorkArea
    {
        public bool bClick = false;
        bool isFirstFetchFlag = false;

        #region Declaration & Constructor & Init

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();

        #region Initialize 
        public PACK001_084()
        {
            try
            {
                InitializeComponent();

                //cboArea.SelectedItemChanged -= cboArea_SelectedValueChanged;

                InitCombo();

                this.Loaded += UserControl_Loaded;
                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.isFirstFetchFlag = false;
            tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbWipListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            this.Loaded -= UserControl_Loaded;
            this.isFirstFetchFlag = true;
        }


        #endregion 

        private void InitCombo()
        {
            try
            {
                String[] sFiltercboAreaRslt = { Area_Type.PACK };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboAreaRslt, sCase: "AREA_PACK");

                String[] sFiltercboEqsgIdRslt = { cboArea.SelectedValue.ToString() };
                _combo.SetCombo(cboEqsgId, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboEqsgIdRslt, sCase: "EQSGID_PACK");


                // 2021.11.12 | 김건식 | MulitSelectionBox -> 기본 commboBox로 변경
                //if (mboEqsgId.ApplyTemplate())ㅁ
                //{
                //    SetComboEqsgid(mboEqsgId);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }



        #region Button

        //#region 조회 버튼 클릭
        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        bClick = false;
        //        loadingIndicator.Visibility = Visibility.Visible;

        //        if (bClick == false)
        //        {
        //            Action act = () =>
        //            {
        //                bClick = true;

        //                Refresh();
        //                HideTestMode();
        //                SearchData();

        //            };

        //            btnSearch.Dispatcher.Invoke(act);
        //        }
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);

        //        HiddenLoadingIndicator();

        //        bClick = false;
        //    }
        //}
        //#endregion

        //#region 요청 버튼 클릭
        //private void btnRequest_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        PACK001_078_CELLREQUEST popup = new PACK001_078_CELLREQUEST();
        //        popup.FrameOperation = this.FrameOperation;

        //        if (popup != null)
        //        {
        //            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index - 3;
        //            DataTable dtReqCond = new DataTable();
        //            dtReqCond = DataTableConverter.Convert(dgPlan.ItemsSource);

        //            object[] Parameters = new object[3];
        //            Parameters[0] = cboArea.SelectedValue;
        //            Parameters[1] = dtReqCond.Rows[index]["MTRLID"].ToString();
        //            Parameters[2] = rdoToday.IsChecked == true ? "1" : "0";
        //            C1WindowExtension.SetParameters(popup, Parameters);

        //            popup.Closed -= popup_Closed;
        //            popup.Closed += popup_Closed;

        //            popup.ShowModal();
        //            popup.CenterOnScreen();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //#endregion

        //#region PACK 생산현황 숨기기
        //private void dgStock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    HideTestMode();
        //}
        //#endregion

        //#region PACK 생산현황 조회 이벤트
        //private void dgPlan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    ShowLoadingIndicator();
        //    Point pnt = e.GetPosition(null);
        //    C1.WPF.DataGrid.DataGridCell cell = dgPlan.GetCellFromPoint(pnt);

        //    if (cell != null)
        //    {
        //        if(cell.Column.Name == "CELL_PRJT" && !(Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT")) == null))
        //        {
        //            ShowTestMode();
        //            string sCellPrjt = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "CELL_PRJT"));
        //            string sPackPjt = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "PACK_PRJT"));
        //            string sProdId = Util.NVC(DataTableConverter.GetValue(dgPlan.Rows[cell.Row.Index].DataItem, "MTRLID"));
        //            int iToday = rdoToday.IsChecked == true ? 1 : 0;
        //            SearchData2(sProdId, sCellPrjt, sPackPjt, iToday);
        //        }

        //    }
        //    HiddenLoadingIndicator();
        //}
        //#endregion
        #endregion Button

        #region Grid
        //private void dgPlan_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    try
        //    {
        //        C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

        //        dataGrid.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            if (e.Cell.Presenter == null)
        //            {
        //                return;
        //            }

        //            if (!(e.Cell.Row.Index > 2)) return;

        //            if (e.Cell.Column.Name != null && e.Cell.Column.Name.Contains("CELL_I"))
        //            {
        //                char sp = '/';
        //                string strTempValue = string.Empty;
        //                strTempValue = e.Cell.Value.ToString().Substring(e.Cell.Value.ToString().IndexOf("(") + 1, e.Cell.Value.ToString().IndexOf(")") - (e.Cell.Value.ToString().IndexOf("(") + 1));
        //                string[] spstring = strTempValue.Split(sp);
        //                spstring[1].ToString();

        //                if(spstring[0].ToString().Equals("0") && spstring[1].ToString().Equals("0"))
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                }
        //                else if (spstring[1].ToString().Equals(spstring[0].ToString()))
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
        //                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                }
        //                else
        //                {

        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                }
        //            }     
        //            if (e.Cell.Column.Name == "CELL_PRJT")
        //            {
        //                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
        //                e.Cell.Presenter.FontWeight = FontWeights.Bold;
        //            }

        //        }));



        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        //private void dgStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    try
        //    {

        //        if (sender == null)
        //            return;

        //        C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

        //        dataGrid.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            if (e.Cell.Presenter == null || e.Cell.Column.Name == null && e.Cell.Value == null)
        //            {
        //                return;
        //            }

        //            int i = 0;


        //            if (e.Cell.Column.Name.Contains("LINE") && e.Cell.Value.ToString() == "TOTAL")
        //            {
        //                sTotalRowIndex = e.Cell.Row.Index;
        //            }

        //            if (e.Cell.Row.Index == sTotalRowIndex && !e.Cell.Column.Name.Contains("AREAID") && !e.Cell.Column.Name.Contains("MTRLID") && !e.Cell.Column.Name.Contains("CELL_PRJT_NAME") && !e.Cell.Column.Name.Contains("PACK_PRJT_NAME"))
        //            {
        //                e.Cell.Presenter.FontSize = 12;
        //                e.Cell.Presenter.FontWeight = FontWeights.UltraBold;
        //                e.Cell.Presenter.Height = 60;
        //                e.Cell.Row.Presenter.BorderBrush = new SolidColorBrush(Colors.Black);
        //                e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.White);
        //                e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.White);

        //            }

        //            if (e.Cell.Row.Type == DataGridRowType.Item && int.TryParse(e.Cell.Column.Name.Substring(1,1), out i) && !e.Cell.Column.Name.Contains("CELL"))
        //            {


        //                if (Util.NVC_Decimal(e.Cell.Value) > 0)
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
        //                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                }
        //                else
        //                {

        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
        //                    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                    dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);

        //                    for (int j = 1; 3 >= j; j++)
        //                    {
        //                        if (int.TryParse(dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.Substring(1,1), out i) && !dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Column.Name.Contains("CELL"))
        //                        {
        //                            if (dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Value.GetDecimal() > 0)
        //                            {
        //                                if (dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter == null) return;

        //                                dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Background = new SolidColorBrush(Colors.Yellow);
        //                                dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                                dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                                dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                                dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                                dgStock.GetCell(e.Cell.Row.Index, e.Cell.Column.Index - j).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                            }
        //                        }
        //                        else if (Util.NVC_Decimal(e.Cell.Value) < 0)
        //                        {
        //                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
        //                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
        //                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                        }
        //                        else
        //                        {
        //                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
        //                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                            e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
        //                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
        //                e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
        //                e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
        //                e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
        //                dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //                dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Colors.Black);
        //            }

        //        }));


        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        //#endregion Grid

        //#region COMBO BOX
        //#region COMBO BOX SETTING
        //private void SetCboPrj()
        //{
        //    try
        //    {
        //        string sSelectedValue = cboArea.SelectedValue.ToString();
        //        string[] sSelectedList = sSelectedValue.Split(',');

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));
        //        RQSTDT.Columns.Add("SHOPID", typeof(string));
        //        RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
        //        RQSTDT.Columns.Add("USERID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = sSelectedValue;
        //        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        dr["SYSTEM_ID"] = LoginInfo.SYSID;
        //        dr["USERID"] = LoginInfo.USERID;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        cboPackPjt.DisplayMemberPath = "CBO_NAME";
        //        cboPackPjt.SelectedValuePath = "CBO_CODE";

        //        for (int i = 0; i < dtResult.Rows.Count; i++)
        //        {
        //            if (sSelectedList.Length > 0 && sSelectedList[0] != "")
        //            {
        //                for (int j = 0; j < sSelectedList.Length; j++)
        //                {
        //                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
        //                    {
        //                        cboPackPjt.Check(i);
        //                        break;
        //                    }
        //                }
        //            }
        //            else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
        //            {
        //                cboPackPjt.Check(i);
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        #endregion



        #endregion Event

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (cboArea.SelectedItem.IsNullOrEmpty() || mboEqsgId.SelectedItemsToString.IsNullOrEmpty())
                if (cboArea.SelectedItem.IsNullOrEmpty())
                {
                    bClick = false;
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                /*
                char[] Csplit = {','};
                string [] strMboEqsgId =  mboEqsgId.SelectedItemsToString.Split(Csplit);

                if(strMboEqsgId.Length > 2 && cboArea.SelectedValue.ToString().Equals("P8"))
                {
                    bClick = false;
                    Util.MessageInfo("SFU8400", new object[] { cboArea.SelectedValue.ToString(), "2" });
                    return;
                }

                if (strMboEqsgId.Length > 4 )
                {
                    bClick = false;
                    Util.MessageInfo("SFU8400", new object[] { cboArea.SelectedValue.ToString(), "4" });
                    return;
                }
                */

                dgCellRefresh();
                dgWipOfLineRefresh();

                if (bClick == false)
                {

                    Action act = () =>
                    {
                        bClick = true;
                        ShowLoadingIndicator();
                        //Refresh();
                        
                        SearchCellData();
                        SearchWipOfLineData();
                    };

                    btnSearch.Dispatcher.Invoke(act);
                    HideTestMode();
                    HiddenLoadingIndicator();
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        #region Cell 조회 
        private void SearchCellData()
        {
            DataSet dsRslt = null;

            try
            {
                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                //dr["EQSGID"] = mboEqsgId.SelectedItemsToString;
                dr["EQSGID"] = cboEqsgId.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_LOC_MNT_CELL", "RQSTDT", "RSLTDT", ds, null);

                if (dsRslt != null)
                {
                    Util.GridSetData(dgCell, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC((dgCell.Rows.Count) - 2));
                    bClick = false;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 라인 재공 조회
        private void SearchWipOfLineData()
        {
            DataSet dsRslt = null;

            try
            {
                

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                //dr["EQSGID"] = mboEqsgId.SelectedItemsToString;
                dr["EQSGID"] = cboEqsgId.SelectedValue.ToString();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_LOC_MNT_WIP_OF_LINE", "RQSTDT", "RSLTDT", ds, null);

                if (dsRslt != null)
                {
                    Util.GridSetData(dgWipOfLine, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC((dgWipOfLine.Rows.Count)-3));
                    bClick = false;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 하단 조회
        private void SearchBottomData(string sSrchType, string strEqsgId, string strEqsgName, string strSubTitle)
        {
            DataSet dsRslt = null;

            try
            {
                string strBizName = string.Empty;

                if(Int64.Parse(sSrchType)>= 10)
                {
                    if(Int64.Parse(sSrchType) >= 21 && Int64.Parse(sSrchType) <= 23)
                    {
                        strBizName = "DA_SEL_LOC_MNT_LOTPLT_DETAIL";
                    }
                    else
                    {
                        strBizName = "DA_SEL_LOC_MNT_LOTPLT_DETAIL2";
                    }
                }
                else 
                {
                    strBizName =  "DA_SEL_LOC_MNT_LOTPLT_DETAIL";
                }

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCHYTYPE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                //dr["EQSGID"] = cboEqsgId.SelectedValue.ToString();
                dr["EQSGID"] = strEqsgId;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCHYTYPE"] = sSrchType;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                dsRslt = new ClientProxy().ExecuteServiceSync_Multi(strBizName, "RQSTDT", "RSLTDT", ds, null);

                if (dsRslt != null)
                {
                    Util.GridSetData(dgLotPallet, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    tbLotPalletCount.Text = strEqsgName + "_" + strSubTitle + "_[" + (Util.NVC((dgLotPallet.Rows.Count) - 2)) + ObjectDic.Instance.GetObjectName("개") + "]";

                    //Util.SetTextBlockText_DataGridRowCount(tbLotPalletCount, Util.NVC((dgLotPallet.Rows.Count) - 2));
                    bClick = false;

                    
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Util.MessageException(ex);
            }

        }

        #endregion

        #region dfCell Refresh
        private void dgCellRefresh()
        {
            try
            {
                Util.gridClear(dgCell);

            }
            catch (Exception ex)
            {
                bClick = false;
                throw ex;
            }
        }
        #endregion 

        #region dgWipOfLine Refresh
        private void dgWipOfLineRefresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgWipOfLine);

            }
            catch (Exception ex)
            {
                bClick = false;
                throw ex;
            }
        }
        #endregion Refresh

        #region 팝업 닫기
        void cellPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_084_CELL_POPUP popup = sender as PACK001_084_CELL_POPUP;
                if (popup.DialogResult == MessageBoxResult.Cancel)
                {

                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 팝업 닫기
        void abnormalPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_084_ABNORMAL_POPUP popup = sender as PACK001_084_ABNORMAL_POPUP;
                if (popup.DialogResult == MessageBoxResult.Cancel)
                {

                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion Method
        


        #region 하단 그리드 SHOW/HIDE

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cellSupply.RowDefinitions[3].Height.Value > 0 && cellSupply.RowDefinitions[4].Height.Value > 0) return;

                //GridMain.RowDefinitions[3].Height = new GridLength(8, GridUnitType.Pixel);
                //GridMain.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

                cellSupply.RowDefinitions[3].Height = new GridLength(8, GridUnitType.Pixel);
                cellSupply.RowDefinitions[4].Height = new GridLength(350, GridUnitType.Pixel);
                GridMain.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Pixel);
                RowSplitter.Visibility = Visibility.Visible;
                //GridMain.RowDefinitions[4].Height = new GridLength(34, GridUnitType.Pixel);
                //GridMain.RowDefinitions[5].Height = new GridLength(34, GridUnitType.Pixel);
                //GridMain.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);

            }));

            //bTestMode = true;
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                //if (GridMain.RowDefinitions[1].Height.Value <= 0) return;
                if (cellSupply.RowDefinitions[3].Height.Value <= 0 && cellSupply.RowDefinitions[4].Height.Value <=  0) return;

                cellSupply.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Pixel);
                cellSupply.RowDefinitions[4].Height = new GridLength(0, GridUnitType.Pixel);
                GridMain.RowDefinitions[3].Height = new GridLength(8, GridUnitType.Pixel);
                //GridMain.RowDefinitions[3].Height = new GridLength(0);
                //GridMain.RowDefinitions[4].Height = new GridLength(0, GridUnitType.Star);
                RowSplitter.Visibility = Visibility.Collapsed;
            }));


        }
        #endregion

        private void dgWipOfLine_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell cell = c1Gd.GetCellFromPoint(pnt);
                C1.WPF.DataGrid.DataGridCell cell2 = dgCell.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string strEqsgId = string.Empty;
                    strEqsgId = (Util.NVC(DataTableConverter.GetValue(dgWipOfLine.Rows[cell.Row.Index].DataItem, "EQSGID")));

                    string strEqsgName = string.Empty;
                    strEqsgName = (Util.NVC(DataTableConverter.GetValue(dgWipOfLine.Rows[cell.Row.Index].DataItem, "EQSGNAME")));

                    string strSubTitle = string.Empty;
                    strSubTitle = ObjectDic.Instance.GetObjectName(cell.Column.Name.ToString());

                    BottomSubTitle.Text = ObjectDic.Instance.GetObjectName(cell.Column.Name.ToString());


                    if (cell.Column.Name == "ABN01_QTY" || cell.Column.Name == "ABN02_QTY" && !(Util.NVC(DataTableConverter.GetValue(dgWipOfLine.Rows[cell.Row.Index].DataItem, "ABN01_QTY")) == null) && !(Util.NVC(DataTableConverter.GetValue(dgWipOfLine.Rows[cell.Row.Index].DataItem, "ABN02_QTY")) == null))
                    {
                        PACK001_084_ABNORMAL_POPUP popup = new PACK001_084_ABNORMAL_POPUP();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            object[] Parameters = new object[1];
                            //Parameters[0] = cboEqsgId.SelectedValue.ToString();
                            Parameters[0] = strEqsgId;
                            C1WindowExtension.SetParameters(popup, Parameters);

                            popup.Closed -= abnormalPopup_Closed;
                            popup.Closed += abnormalPopup_Closed;

                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                    if (c1Gd.GetRowCount() > 0 && cell.Row.Index >= 0 && cell.Column.Name != "ABN01_QTY" && cell.Column.Name != "ABN02_QTY")
                    {
                        if (cell.Column.Tag == null) return;

                        ShowTestMode();
                        SearchBottomData(cell.Column.Tag.ToString() , strEqsgId, strEqsgName, strSubTitle);

                    }
                }
            }
            catch
            {

            }

        }


        private void dgCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Point pnt = e.GetPosition(null);
                //C1.WPF.DataGrid.DataGridCell cell = dgCell.GetCellFromPoint(pnt);

                //if (cell != null)
                //{
                //    if (cell.Column.Name == "CELL_SHIPMENT" && !(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[cell.Row.Index].DataItem, "CELL_SHIPMENT")) == null) || cell.Column.Name == "RCV_WAIT" && !(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[cell.Row.Index].DataItem, "RCV_WAIT")) == null))
                //    {
                //        PACK001_084_CELL_POPUP popup = new PACK001_084_CELL_POPUP();
                //        popup.FrameOperation = this.FrameOperation;

                //        if (popup != null)
                //        {
                //            object[] Parameters = new object[2];
                //            //Parameters[0] = cboEqsgId.SelectedValue.ToString();
                //            Parameters[0] = mboEqsgId.SelectedItemsToString;
                //            Parameters[1] = (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[cell.Row.Index].DataItem, "PRJT_NAME")));
                //            C1WindowExtension.SetParameters(popup, Parameters);

                //            popup.Closed -= cellPopup_Closed;
                //            popup.Closed += cellPopup_Closed;

                //            popup.ShowModal();
                //            popup.CenterOnScreen();
                //        }
                //    }

                //}
            }
            catch
            {

            }

        }

        //private void SetComboEqsgid(MultiSelectionBox cboMulti)
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = cboArea.SelectedValue.ToString();
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (dtResult.Rows.Count != 0)
        //        {
        //            if (dtResult.Rows.Count == 1)
        //            {
        //                cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
        //                cboMulti.Uncheck(-1);
        //            }
        //            else
        //            {
        //                //cboMulti.isAllUsed = true;
        //                if (isFirstFetchFlag)
        //                {
        //                    cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
        //                    for (int i = 0; i < dtResult.Rows.Count; ++i)
        //                    {
        //                        cboMulti.Uncheck(i - 1);
        //                    }
        //                }
        //                else if(mboEqsgId.ApplyTemplate())
        //                {
        //                    cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
        //                    for (int i = 0; i < dtResult.Rows.Count; ++i)
        //                    {
        //                        cboMulti.Uncheck(i - 1);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            cboMulti.ItemsSource = null;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        //private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    try
        //    {                
        //        SetComboEqsgid(mboEqsgId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
    }
}