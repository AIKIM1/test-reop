/*************************************************************************************
 Created Date : 2020.09.22
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.22  김길용           Initialize
**************************************************************************************/


using System;
using System.Data;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_001_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string sBeforeUse_flag = null;
        private string sINSHOPID = string.Empty;
        private string sINAREAID = string.Empty;
        private string sAreaTypecode = string.Empty;
        string sEqsgid = string.Empty;
        string sProdid = string.Empty;
        string sMixtype = string.Empty;
        string sMixmthd = string.Empty;
        string sItemtype = string.Empty;
        string sCelltype = string.Empty;
        string sWorkuserid = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private DataTable isCreateTable = new DataTable();
        private Util _Util = new Util();
        
        private DataTable isListTable = new DataTable();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_001_POPUP()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        //Radio Button 체크 시 변수 선언 및 그리드 초기화
        private void rdoAssy_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoAssy != null)
            {
                if ((bool)rdoAssy.IsChecked)
                {
                    Util.gridClear(dgGridone);
                    Util.gridClear(dgGridtwo);
                    Util.gridClear(dgGridThree);
                    sINSHOPID = "A";
                    dgGridone.Visibility = Visibility.Visible;
                    SetShop(sINSHOPID);

                }

            }
        }

        private void rdoAssyLine_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoAssyLine != null)
            {
                if ((bool)rdoAssyLine.IsChecked)
                {
                    Util.gridClear(dgGridone);
                    Util.gridClear(dgGridtwo);
                    Util.gridClear(dgGridThree);
                    sINSHOPID = "A";
                    dgGridone.Visibility = Visibility.Visible;
                    SetShop(sINSHOPID);

                }

            }
        }

        private void rdoElecAnode_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoElecAnode != null)
            {
                if ((bool)rdoElecAnode.IsChecked)
                {
                    Util.gridClear(dgGridone);
                    Util.gridClear(dgGridtwo);
                    Util.gridClear(dgGridThree);
                    sINSHOPID = "E";
                    dgGridone.Visibility = Visibility.Visible;
                    SetShop(sINSHOPID);

                }

            }
        }

        //private void rdoElecCathode_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (rdoElecCathode != null)
        //    {
        //        if ((bool)rdoElecCathode.IsChecked)
        //        {
        //            Util.gridClear(dgGridone);
        //            Util.gridClear(dgGridtwo);
        //            Util.gridClear(dgGridThree);
        //            sINSHOPID = "E";
        //            dgGridone.Visibility = Visibility.Visible;
        //            SetShop(sINSHOPID);

        //        }

        //    }
        //}
        //Radio Button 언체크 시 그리드 초기화
        private void rdoAssy_Unchecked(object sender, RoutedEventArgs e)
        {

            if (!(bool)rdoAssy.IsChecked)
            {
                Util.gridClear(dgGridone);
                Util.gridClear(dgGridtwo);
                Util.gridClear(dgGridThree);
                //Util.gridClear(dgGridCell);
                dgGridone.Visibility = Visibility.Hidden;
                dgGridtwo.Visibility = Visibility.Hidden;
                dgGridThree.Visibility = Visibility.Hidden;
                //dgGridCell.Visibility = Visibility.Hidden;
                btnNext1.Visibility = Visibility.Hidden;
                btnNext2.Visibility = Visibility.Hidden;
            }

        }

        private void rdoAssyLine_Unchecked(object sender, RoutedEventArgs e)
        {

            if (!(bool)rdoAssyLine.IsChecked)
            {
                Util.gridClear(dgGridone);
                Util.gridClear(dgGridtwo);
                Util.gridClear(dgGridThree);
                //Util.gridClear(dgGridCell);
                dgGridone.Visibility = Visibility.Hidden;
                dgGridtwo.Visibility = Visibility.Hidden;
                dgGridThree.Visibility = Visibility.Hidden;
                //dgGridCell.Visibility = Visibility.Hidden;
                btnNext1.Visibility = Visibility.Hidden;
                btnNext2.Visibility = Visibility.Hidden;
            }

        }

        private void rdoElecAnode_Unchecked(object sender, RoutedEventArgs e)
        {

            if (!(bool)rdoElecAnode.IsChecked)
            {
                Util.gridClear(dgGridone);
                Util.gridClear(dgGridtwo);
                Util.gridClear(dgGridThree);
                //Util.gridClear(dgGridCell);
                dgGridone.Visibility = Visibility.Hidden;
                dgGridtwo.Visibility = Visibility.Hidden;
                dgGridThree.Visibility = Visibility.Hidden;
                //dgGridCell.Visibility = Visibility.Hidden;
                btnNext1.Visibility = Visibility.Hidden;
                btnNext2.Visibility = Visibility.Hidden;
            }

        }

        //private void rdoElecCathode_Unchecked(object sender, RoutedEventArgs e)
        //{

        //    if (!(bool)rdoElecCathode.IsChecked)
        //    {
        //        Util.gridClear(dgGridone);
        //        Util.gridClear(dgGridtwo);
        //        Util.gridClear(dgGridThree);
        //        //Util.gridClear(dgGridCell);
        //        dgGridone.Visibility = Visibility.Hidden;
        //        dgGridtwo.Visibility = Visibility.Hidden;
        //        dgGridThree.Visibility = Visibility.Hidden;
        //        //dgGridCell.Visibility = Visibility.Hidden;
        //        btnNext1.Visibility = Visibility.Hidden;
        //        btnNext2.Visibility = Visibility.Hidden;
        //    }

        //}
        //private void rdoCellprod_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (!(bool)rdoCellprod.IsChecked)
        //    {
        //        Util.gridClear(dgGridone);
        //        Util.gridClear(dgGridtwo);
        //        Util.gridClear(dgGridThree);
        //        Util.gridClear(dgGridCell);
        //        dgGridone.Visibility = Visibility.Hidden;
        //        dgGridtwo.Visibility = Visibility.Hidden;
        //        dgGridThree.Visibility = Visibility.Hidden;
        //        dgGridCell.Visibility = Visibility.Hidden;
        //        btnNext1.Visibility = Visibility.Hidden;
        //        btnNext2.Visibility = Visibility.Hidden;
        //    }
        //}
        //첫번째 스텝버튼 입력값 및 결과값 출력을 위한 Click 이벤트 처리
        private void btnNext1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgGridtwo);
                dgGridtwo.Visibility = Visibility.Visible;
                DataTable dt = DataTableConverter.Convert(dgGridone.ItemsSource);
                DataRow[] drINPUT_LOT = dt.Select(" CHK = true ");
                if (drINPUT_LOT != null && drINPUT_LOT.Length > 0)
                {
                    for (int i = 0; i < drINPUT_LOT.Length; i++)
                    {

                        sINSHOPID = drINPUT_LOT[i].ItemArray[1].ToString();
                        if (rdoAssy.IsChecked == true)
                            sAreaTypecode = "A";
                        if (rdoAssyLine.IsChecked == true)
                            sAreaTypecode = "A";
                        if (rdoElecAnode.IsChecked == true)
                            sAreaTypecode = "E";
                        //if (rdoElecCathode.IsChecked == true)
                        //    sAreaTypecode = "E";

                        SetArea(sINSHOPID, sAreaTypecode);

                    }
                }
                else
                {
                    dgGridtwo.Visibility = Visibility.Hidden;
                }
            }
            catch
            {
            }
        }
        //두번째 스텝버튼 입력값 및 결과값 출력을 위한 Click 이벤트 처리
        private void btnNext2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgGridThree);
                DataTable dt = DataTableConverter.Convert(dgGridtwo.ItemsSource);
                DataRow[] drINPUT_LOT = dt.Select(" CHK = true ");
                if (drINPUT_LOT != null && drINPUT_LOT.Length > 0)
                {
                    dgGridThree.Visibility = Visibility.Visible;
                    for (int i = 0; i < drINPUT_LOT.Length; i++)
                    {
                        sINSHOPID = drINPUT_LOT[i].ItemArray[1].ToString();
                        sINAREAID = drINPUT_LOT[i].ItemArray[2].ToString();
                        string sEqgrid = string.Empty;
                        string sElecode = string.Empty;

                        if (rdoAssyLine.IsChecked == true)
                        {
                            sAreaTypecode = "A";
                            sEqgrid = "PKG";
                            sElecode = "";
                        }
                            
                        if (rdoElecAnode.IsChecked == true)
                        {
                            sAreaTypecode = "E";
                            sEqgrid = "COT";
                            sElecode = "";
                        }
                            
                        //if (rdoElecCathode.IsChecked == true)
                        //{
                        //    sAreaTypecode = "E";
                        //    sEqgrid = "COT";
                        //    sElecode = "A";
                        //}
                            

                        SetEqsg(sINSHOPID, sINAREAID, sAreaTypecode, sEqgrid, sElecode);

                    }
                }
                else
                {
                    dgGridThree.Visibility = Visibility.Hidden;
                }
            }

            catch
            {
            }
        }
        //private void chkHeaderAllList_one_Checked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridone.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", true);
        //    }

        //    dgGridone.EndEdit();
        //    dgGridone.EndEditRow(true);
        //}

        //private void chkHeaderAllList_one_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridone.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", false);
        //    }

        //    dgGridone.EndEdit();
        //    dgGridone.EndEditRow(true);
        //}

        //private void chkHeaderAllList_two_Checked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridtwo.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", true);
        //    }

        //    dgGridtwo.EndEdit();
        //    dgGridtwo.EndEditRow(true);
        //}

        //private void chkHeaderAllList_two_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridtwo.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", false);
        //    }

        //    dgGridtwo.EndEdit();
        //    dgGridtwo.EndEditRow(true);
        //}

        //private void chkHeaderAllList_three_Checked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridThree.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", true);
        //    }

        //    dgGridThree.EndEdit();
        //    dgGridThree.EndEditRow(true);
        //}

        //private void chkHeaderAllList_three_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridThree.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", false);
        //    }

        //    dgGridThree.EndEdit();
        //    dgGridThree.EndEditRow(true);
        //}
        //private void chkHeaderAllList_Cell_Checked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridCell.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", true);
        //    }

        //    dgGridCell.EndEdit();
        //    dgGridCell.EndEditRow(true);
        //}

        //private void chkHeaderAllList_Cell_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    foreach (C1.WPF.DataGrid.DataGridRow row in dgGridCell.Rows)
        //    {
        //        DataTableConverter.SetValue(row.DataItem, "CHK", false);
        //    }

        //    dgGridCell.EndEdit();
        //    dgGridCell.EndEditRow(true);
        //}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            string[] tmp = null;

            if (tmps != null && tmps.Length >= 1)
            {
                sMixtype = Util.NVC(tmps[0]);
                sEqsgid = Util.NVC(tmps[1]);
                sProdid = Util.NVC(tmps[2]);
                sMixmthd = Util.NVC(tmps[3]);
                sCelltype = Util.NVC(tmps[4]);
                sWorkuserid = Util.NVC(tmps[5]);
            }
            else
            {
            }
        }
        #endregion

        #region Method

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        //Grid 내에 체크에 대한 값 처리
        private void dgGridone_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column == this.dgGridone.Columns["USE_FLAG"])
            {
                sBeforeUse_flag = Convert.ToString(dgGridone.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgGridone.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgGridone.Columns["CHK"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
        private void dgGridtwo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column == this.dgGridtwo.Columns["USE_FLAG"])
            {
                sBeforeUse_flag = Convert.ToString(dgGridtwo.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgGridtwo.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgGridtwo.Columns["CHK"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
        private void dgGridThree_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column == this.dgGridThree.Columns["USE_FLAG"])
            {
                sBeforeUse_flag = Convert.ToString(dgGridThree.CurrentCell.Value);
            }
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgGridThree.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgGridThree.Columns["CHK"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
        //private void dgGridCell_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        //{
        //    if (e.Column == this.dgGridCell.Columns["USE_FLAG"])
        //    {
        //        sBeforeUse_flag = Convert.ToString(dgGridCell.CurrentCell.Value);
        //    }
        //    DataRowView drv = e.Row.DataItem as DataRowView;

        //    if (drv["CHK"].SafeToString() != "True" && e.Column != dgGridCell.Columns["CHK"])
        //    {
        //        e.Cancel = true;
        //        return;
        //    }

        //    if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
        //    {
        //        e.Cancel = false;
        //    }
        //    else
        //    {
        //        if (e.Column != this.dgGridCell.Columns["CHK"]
        //         )
        //        {
        //            e.Cancel = true;
        //        }
        //        else
        //        {
        //            e.Cancel = false;
        //        }
        //    }
        //}
        #endregion
        //활동기준이 될 수 있는 SHOPID 출력
        private void SetShop(string sShoptype)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREA_TYPE_CODE"] = sShoptype.ToString();

                RQSTDT.Rows.Add(dr);
                
                new ClientProxy().ExecuteService("DA_BAS_SEL_SHOP_A_CBO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgGridone, dtResult, FrameOperation);

                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        //활동기준이 될 수 있는 AREA 출력
        private void SetArea(string sShoptype, string sAreatype)
        {
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREA_TYPE_CODE"] = sAreatype.ToString();
                dr["SHOPID"] = sShoptype.ToString();

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_BAS_SEL_AREA_T_CBO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        if (dgGridtwo.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgGridtwo, dtResult, FrameOperation, true);
                        }
                        else
                        {
                            if (dtResult.Rows.Count > 0)
                            {
                                DataTable dtBefore = DataTableConverter.Convert(dgGridtwo.ItemsSource);
                                dtBefore.Merge(dtResult);
                                Util.GridSetData(dgGridtwo, dtBefore, FrameOperation, true);
                            }
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        //활동기준이 될 수 있는 EQPT 출력(극성구분코드)
        private void SetEqsg(string sShoptype, string sAreatype, string sEqsgtype, string sEqgrid, string sElecode)
        {
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = sShoptype.ToString();
                dr["AREAID"] = sAreatype.ToString();
                dr["AREA_TYPE_CODE"] = sEqsgtype.ToString();
                dr["EQGRID"] = sEqgrid.ToString();
                dr["ELTR_TYPE_CODE"] = sElecode.ToString() == "" ? null : sElecode.ToString();

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQSG_L_CBO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        if (dgGridThree.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgGridThree, dtResult, FrameOperation, true);
                        }
                        else
                        {
                            if (dtResult.Rows.Count > 0)
                            {
                                DataTable dtBefore = DataTableConverter.Convert(dgGridThree.ItemsSource);
                                dtBefore.Merge(dtResult);
                                Util.GridSetData(dgGridThree, dtBefore, FrameOperation, true);
                            }
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        //적용라인의 투입가능한 셀정보 출력(미사용)
        //private void SetProd(string sMixtypecode, string sEqsgid)
        //{
        //    try
        //    {
                
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("SHOPID", typeof(string));
        //        RQSTDT.Columns.Add("EQSGID", typeof(string));
        //        RQSTDT.Columns.Add("MIX_TYPE_CODE", typeof(string));
        //        RQSTDT.Columns.Add("NOTINTYPE", typeof(string));
        //        RQSTDT.Columns.Add("LOGIS_FLAG", typeof(string));


        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        dr["EQSGID"] = sEqsgid.ToString();
        //        dr["MIX_TYPE_CODE"] = sMixtypecode.ToString();
        //        dr["NOTINTYPE"] = "CELL";
        //        dr["LOGIS_FLAG"] = "Y";

        //        RQSTDT.Rows.Add(dr);

        //        loadingIndicator.Visibility = Visibility.Visible;

        //        new ClientProxy().ExecuteService("DA_SEL_TB_SFC_INPUT_MIX_MTRL_CBO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
        //        {

        //            if (ex != null)
        //            {
        //                Util.MessageException(ex);
        //                loadingIndicator.Visibility = Visibility.Collapsed;
        //                return;
        //            }
                    
        //            if (dtResult.Rows.Count != 0)
        //            {
        //                if (dgGridCell.GetRowCount() == 0)
        //                {
        //                    //dtAllRow.Merge(dtResult);
        //                    Util.GridSetData(dgGridCell, dtResult, FrameOperation, true);
        //                }
        //            }
        //            loadingIndicator.Visibility = Visibility.Collapsed;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //    }
        //}

        //Save 시 Commoncode로 관리하여 입력값 분류 ( 그룹코드 : PACK_UI_INPUT_MIX_CHK_ITEM_CODE -ATTRIBUTE1으로 관리 )
        private void Popsave()
        {
            ShowLoadingIndicator();
            DoEvents();

            if (rdoAssy.IsChecked == true)
            {
                sItemtype = "1";
                isCreateTable = DataTableConverter.Convert(dgGridtwo.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgGridtwo)) return;
                this.dgGridtwo.EndEdit();
                this.dgGridtwo.EndEditRow(true);
            }
                
            if (rdoAssyLine.IsChecked == true)
            {
                sItemtype = "2";
                isCreateTable = DataTableConverter.Convert(dgGridThree.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgGridThree)) return;
                this.dgGridThree.EndEdit();
                this.dgGridThree.EndEditRow(true);
            }
            if (rdoElecAnode.IsChecked == true)
            {
                sItemtype = "3";
                isCreateTable = DataTableConverter.Convert(dgGridThree.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgGridThree)) return;
                this.dgGridThree.EndEdit();
                this.dgGridThree.EndEditRow(true);
            }
            //if (rdoElecCathode.IsChecked == true)
            //{
            //    sItemtype = "3";
            //    isCreateTable = DataTableConverter.Convert(dgGridThree.GetCurrentItems());
            //    if (!CommonVerify.HasDataGridRow(dgGridThree)) return;
            //    this.dgGridThree.EndEdit();
            //    this.dgGridThree.EndEditRow(true);
            //}
            //if (rdoCellprod.IsChecked == true)
            //{
            //    sItemtype = "4";
            //    isCreateTable = DataTableConverter.Convert(dgGridCell.GetCurrentItems());
            //    if (!CommonVerify.HasDataGridRow(dgGridCell)) return;
            //    this.dgGridCell.EndEdit();
            //    this.dgGridCell.EndEditRow(true);
            //}
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("SQLTYPE", typeof(string));
                inDataTable.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                inDataTable.Columns.Add("USER", typeof(string));
                inDataTable.Columns.Add("WORKUSER", typeof(string));

                DataTable inDtlTable = indataSet.Tables.Add("IN_DTL_INFO");

                inDtlTable.Columns.Add("INPUT_MIX_CHK_ITEM_CODE", typeof(string));
                inDtlTable.Columns.Add("CHK_VALUE", typeof(string));
                inDtlTable.Columns.Add("CHK_SEQNO", typeof(string));
                inDtlTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow param = inDataTable.NewRow();

                param["LANGID"] = LoginInfo.LANGID;
                param["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                param["AREAID"] = LoginInfo.CFG_AREA_ID;
                param["SQLTYPE"] = "I";
                param["INPUT_MIX_TYPE_CODE"] = sMixtype;
                param["EQSGID"] = sEqsgid;
                param["PRODID"] = sProdid;
                param["INPUT_MIX_CHK_MTHD_CODE"] = sMixmthd;
                param["USER"] = LoginInfo.USERID;
                param["WORKUSER"] = sWorkuserid;
                
                inDataTable.Rows.Add(param);

                if (rdoAssy.IsChecked == true)
                {
                    DataTable dt = DataTableConverter.Convert(dgGridtwo.ItemsSource);
                    var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                    foreach (var item in query)
                    {
                        DataRow rparam = inDtlTable.NewRow();
                        rparam["INPUT_MIX_CHK_ITEM_CODE"] = sItemtype;
                        rparam["CHK_VALUE"] = item.Field<string>("CBO_CODE");
                        rparam["CHK_SEQNO"] = null;
                        rparam["USE_FLAG"] = "Y";

                        inDtlTable.Rows.Add(rparam);
                    }
                    //foreach (object modified in dgGridtwo.GetModifiedItems())
                    //{
                    //    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    //    {
                    //        DataRow rparam = inDtlTable.NewRow();
                    //        rparam["INPUT_MIX_CHK_ITEM_CODE"] = sItemtype;
                    //        rparam["CHK_VALUE"] = DataTableConverter.GetValue(modified, "CBO_CODE");
                    //        rparam["CHK_SEQNO"] = null;
                    //        rparam["USE_FLAG"] = "Y";

                    //        inDtlTable.Rows.Add(rparam);
                    //    }
                    //}
                }
                if (rdoAssyLine.IsChecked == true || rdoElecAnode.IsChecked == true )
                {
                    DataTable dt = DataTableConverter.Convert(dgGridThree.ItemsSource);
                    var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                    foreach (var item in query)
                    {
                        DataRow rparam = inDtlTable.NewRow();
                        rparam["INPUT_MIX_CHK_ITEM_CODE"] = sItemtype;
                        rparam["CHK_VALUE"] = item.Field<string>("CBO_CODE");
                        rparam["CHK_SEQNO"] = null;
                        rparam["USE_FLAG"] = "Y";

                        inDtlTable.Rows.Add(rparam);
                    }
                    //foreach (object modified in dgGridThree.GetModifiedItems())
                    //{
                    //    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    //    {
                    //        DataRow rparam = inDtlTable.NewRow();
                    //        rparam["INPUT_MIX_CHK_ITEM_CODE"] = sItemtype;
                    //        rparam["CHK_VALUE"] = DataTableConverter.GetValue(modified, "CBO_CODE");
                    //        rparam["CHK_SEQNO"] = null;
                    //        rparam["USE_FLAG"] = "Y";
                            
                    //        inDtlTable.Rows.Add(rparam);
                    //    }
                    //}
                }
                if (inDataTable.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }
                if (inDtlTable.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TB_SFC_INPUT_MIX_CHK_MST_DETL", "INDATA,IN_DTL_INFO", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU2056", inDtlTable.Rows.Count);
                        inDtlTable = new DataTable();
                        inDataTable = new DataTable();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, indataSet);
                
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_TB_SFC_INPUT_MIX_CHK_MST_DETL", ex.Message, ex.ToString());
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validation Check...
            if (string.IsNullOrEmpty(sWorkuserid))
            {
                Util.Alert("SFU4591"); // 작업자를 입력하세요
                return;
            }

            // 조립동 데이터
            if (rdoAssy.IsChecked == true)
            {                
                DataTable dt = DataTableConverter.Convert(dgGridtwo.ItemsSource);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return;
                }
                //DataRow[] dtINPUT_LOT = dt.Select(" CHK = true ");
                //if (dtINPUT_LOT.Length == 0) return;
                var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                if (query.Count() <= 0)
                {
                    return;
                }
            }
            if (rdoAssyLine.IsChecked == true || rdoElecAnode.IsChecked == true )
            {
                if (dgGridThree.Visibility == Visibility.Visible)
                {
                    DataTable dt = DataTableConverter.Convert(dgGridThree.ItemsSource);
                    if (dt == null || dt.Rows.Count <= 0)
                    {
                        return;
                    }
                    //DataRow[] dtINPUT_LOT = dt.Select(" CHK = true ");
                    //if (dtINPUT_LOT.Length == 0) return;
                    var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                    if (query.Count() <= 0)
                    {
                        return;
                    }
                }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dgGridtwo.ItemsSource);
                    if (dt == null || dt.Rows.Count <= 0)
                    {
                        return;
                    }
                    //DataRow[] dtINPUT_LOT = dt.Select(" CHK = true ");
                    //if (dtINPUT_LOT.Length == 0) return;
                    var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                    if (query.Count() <= 0)
                    {
                        return;
                    }
                }
            }
            //if (rdoCellprod.IsChecked == true )
            //{
            //    DataTable dt = DataTableConverter.Convert(dgGridCell.ItemsSource);
            //    if (dt.Rows.Count > 0)
            //    {
            //        DataRow[] drINPUT_LOT = dt.Select(" CHK = true ");
            //        if (drINPUT_LOT.Length == 0)  return;
            //    }
            //    else
            //    {
            //        return;
            //    }
                
            //}
            
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Popsave();
                    DoEvents();

                    //this.Close();

                }
            });
        }

        //private void rdoCellprod_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (rdoCellprod != null)
        //    {
        //        if ((bool)rdoCellprod.IsChecked)
        //        {
        //            Util.gridClear(dgGridone);
        //            Util.gridClear(dgGridtwo);
        //            Util.gridClear(dgGridThree);
        //            Util.gridClear(dgGridCell);
        //            dgGridCell.Visibility = Visibility.Visible;
        //            if (sCelltype != "CELL")
        //            {
        //                SetProd(sMixtype, sEqsgid);
        //            }
        //            else
        //            {
        //                Util.MessageValidation("혼입 활동구분만 등록가능합니다.");
        //                return;
        //            }
        //        }

        //    }
        //}
        //자동으로 다음버튼 누르게 동작변경
        private void dgGridone_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgGridone.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sCHK = Util.NVC(DataTableConverter.GetValue(dgGridone.Rows[cell.Row.Index].DataItem, "CHK"));

                    if (sCHK == "True" || sCHK == "False")
                    {
                        Util.gridClear(dgGridtwo);
                        Util.gridClear(dgGridThree);
                        btnNext1_Click(null,null);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgGridtwo_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgGridtwo.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (rdoAssy.IsChecked == false)
                    {
                        string sCHK = Util.NVC(DataTableConverter.GetValue(dgGridtwo.Rows[cell.Row.Index].DataItem, "CHK"));
                        if (sCHK == "True" || sCHK == "False")
                        {
                            Util.gridClear(dgGridThree);
                            btnNext2_Click(null, null);
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }
}
