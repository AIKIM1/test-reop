/*************************************************************************************
 Created Date : 2021.09.14
      Creator : 이재호
   Decription : 라인내 재공조회 (FN_SFC_WIP) 프로시저 사용
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.14 이재호 3739488  [GMES생산PI팀] GMES시스템의 재공현황 조회 UI의 이력화면 기능 변경 건 요청번호 C20210914-000316
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Management;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_088 : UserControl, IWorkArea
    {
        private string sSHOPID = string.Empty;
        private string sAREAID = string.Empty;
        private string sLINEID = string.Empty;
        //202.007.19
        private string sPRODID = string.Empty;

        string strRdoType = string.Empty;
        string strSumDate = string.Empty;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        string now_labelcode = string.Empty;

        public PACK001_088()
        {
            InitializeComponent();
            tbRetrunListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbPackInfoCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbRetrunListCount_H1.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbPackInfoCount_M1.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            InitCombo();
        }
        #endregion Declaration & Constructor 

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;
        }

        private void InitCombo()
        {
            sSHOPID = LoginInfo.CFG_SHOP_ID;


            Set_Combo_Area(cboAreaByAreaType);
            SetCboEQSG(cboEquipmentSegment);

            Set_Combo_Area_Move(cboArea_Areatype);
            Set_Combo_Product(cboProduct);
        }
        #endregion

        #region Event

        #region Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                if (cboAreaByAreaType.SelectedValue.ToString() == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (cboEquipmentSegment.SelectedValue.ToString() == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }
                //getWipList();
                getWipList_Procedure();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgWipList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcel1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgPackInfo);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018.12.08
        private void btnSearch_Move_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboAreaByAreaType.SelectedValue.ToString() == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (cboEquipmentSegment.SelectedValue.ToString() == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }
                getWipMovingList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2018.12.08
        private void btnExcel_M1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgWipMoving);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018.12.08
        private void btnExcel1_M_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellInfo);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion Button

        #region Grid
        private void dgWipList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                //2020.07.16
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgWipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null && !cell.Value.ToString().Equals("0") )
                {
                    string sAREAID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "AREAID"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sPRDT_CLSS_CODE = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRDT_CLSS_CODE_S08"));
                    string strEqsgId = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "EQSGID"));
                    string strRoutId = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "ROUTID"));
                    //string sTOTL_VALUE = Util.NVC(cell.Column.Name.ToString());
                    string sWIPSTAT_TYPE_CODE = Util.NVC(cell.Column.Name.ToString());
                                        
                    getWipList_Detail_Procedure(sAREAID, sPRODID, sPRDT_CLSS_CODE, sWIPSTAT_TYPE_CODE, strEqsgId, strRoutId);


                    //string[] switchStrings = { "WIP_TOTL_STAT_CODE", "WIP_RCV_ISS_STAT_CODE", "WIP_HOLD_STAT_CODE" };

                    //switch (switchStrings.FirstOrDefault<string>(s => message.ToUpper().Contains(s)))
                    //{
                    //    case "D_WIP_TOTL_STAT_CODE":
                    //        sWIPSTAT_TYPE_CODE = switchStrings[0].ToString();                           
                    //        break;
                    //    case "D_WIP_RCV_ISS_STAT_CODE":
                    //        sWIPSTAT_TYPE_CODE = switchStrings[1].ToString();
                    //        break;
                    //    case "D_WIP_HOLD_STAT_CODE":
                    //        sWIPSTAT_TYPE_CODE = switchStrings[2].ToString();
                    //        break;
                    //}

                    //return;
                    //getWipList_Detail_Procedure(sAREAID, sPRODID, sPRDT_CLSS_CODE, sWIPSTAT_TYPE_CODE, strEqsgId, sTOTL_VALUE);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //private void dgWipList_H_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        Point pnt = e.GetPosition(null);
        //        C1.WPF.DataGrid.DataGridCell cell = dgWipList_H.GetCellFromPoint(pnt);

        //        if (cell != null)
        //        {
        //            string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList_H.Rows[cell.Row.Index].DataItem, "PRODID"));
        //            string sPRODTYPE = Util.NVC(DataTableConverter.GetValue(dgWipList_H.Rows[cell.Row.Index].DataItem, "PRDTYPE"));
        //            string strEqsgID = Util.NVC(DataTableConverter.GetValue(dgWipList_H.Rows[cell.Row.Index].DataItem, "EQSGID"));
        //            string sWIPSTATFLAG = "";

        //            if (cell.Column.Name.Equals("WAIT_QTY"))
        //            {
        //                sWIPSTATFLAG = "0";
        //            }
        //            else if (cell.Column.Name.Equals("PROC_QTY"))
        //            {
        //                sWIPSTATFLAG = "1";
        //            }
        //            else if (cell.Column.Name.Equals("HOLD_QTY"))
        //            {
        //                sWIPSTATFLAG = "2";
        //            }

        //            getWipSnapList_Detail(sPRODID, sPRODTYPE, sWIPSTATFLAG, strEqsgID);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        //2018.12.08
        private void dgWipMoving_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "RCV_ISS_ID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018.12.08
        private void dgWipMoving_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipMoving.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    //string sRCVISS = Util.NVC(DataTableConverter.GetValue(dgWipMoving.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipMoving.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sToArea = Util.NVC(DataTableConverter.GetValue(dgWipMoving.Rows[cell.Row.Index].DataItem, "TO_AREAID"));
                    string sFROMArea = Util.NVC(DataTableConverter.GetValue(dgWipMoving.Rows[cell.Row.Index].DataItem, "FROM_AREAID"));
                    //string sBOXID = Util.NVC(DataTableConverter.GetValue(dgWipMoving.Rows[cell.Row.Index].DataItem, "BOXID"));
                    //int iCnt = Convert.ToInt32(DataTableConverter.GetValue(dgWipMoving.Rows[cell.Row.Index].DataItem, "CNT")));

                    //getWipMovingList_Detail(sRCVISS, sPRODID, sToArea, sBOXID);
                    getWipMovingList_Detail(sPRODID, sToArea, sFROMArea);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2019.06.11
        private void dgPackInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "PACK_STORAGE_PERIOD" && e.Cell.Value != null && e.Cell.Value.ToString() != "")
                    {
                        string value = e.Cell.Value.ToString();

                        if (value != null && !value.Equals(""))
                        {
                            if (e.Cell.Column.Name == "PACK_STORAGE_PERIOD")
                            {
                                //e.Cell.Column.Name;
                                if (value.Equals("장기진부화재고"))  //달성률 < 90
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                                else if (value.Equals("4개월") || value.Equals("5개월") || value.Equals("6개월")) // 90 <= 달성률 <100
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion Grid

        #region Combo
        private void cboAreaByAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAreaByAreaType.SelectedIndex > -1)
            {
                sAREAID = Convert.ToString(cboAreaByAreaType.SelectedValue);
                SetCboEQSG(cboEquipmentSegment);
            }
            else
            {
                sAREAID = string.Empty;
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex > -1)
            {
                sLINEID = Convert.ToString(cboEquipmentSegment.SelectedValue);

                if ((sSHOPID != null) & (sAREAID != null))
                {

                }
            }
            else
            {
                sLINEID = string.Empty;
            }
        }

        //2020.07.16
        private void cboArea_Areatype_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProduct.SelectedIndex > -1)
            {
                sPRODID = Convert.ToString(cboArea_Areatype.SelectedValue);
                Set_Combo_Product(cboProduct);
            }
            else
            {
                sPRODID = string.Empty;
            }
        }

        #endregion Combo

        #endregion Event

        #region Mehod
        private void getWipList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                //2018.10.01
                //RQSTDT.Columns.Add("CALDATE", typeof(string));
                //RQSTDT.Columns.Add("MODLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                //2018.10.01
                //dr["CALDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                //dr["CALDATE_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                //dr["CALDATE_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
                //dr["MODLID"] = Util.NVC(cboProductModel.SelectedItemsToString) == "" ? null : cboProductModel.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_STATUS_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgWipList, dtResult, FrameOperation, true);

                if (dtResult.Rows.Count > 0)
                {
                    tbRetrunListCount.Text = "[ " + dtResult.Rows.Count.ToString() + " 건 ]";
                }

                //loadingIndicator.Visibility = Visibility.Visible;
                //new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //    if (ex != null)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }

                //    Util.GridSetData(dgWipList, dtResult, FrameOperation, true);

                //    tbRetrunListCount.Text = 
                //});
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getWipList_Procedure()
        {
            try
            {
                Util.gridClear(dgPackInfo);

                DataTable dsResult = new DataTable();

                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WIP_TYPE", typeof(string));
                INDATA.Columns.Add("WIP_DETL_TYPE", typeof(string));
                INDATA.Columns.Add("UI_TYPE", typeof(string));
                INDATA.Columns.Add("SUM_DATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PKG_LOTID", typeof(string));
                INDATA.Columns.Add("GRP_MKT_TYPE_CODE_FLAG", typeof(string));
                INDATA.Columns.Add("GRP_PROD_VER_CODE_FLAG", typeof(string));
                INDATA.Columns.Add("GRP_RP_SEQ_FLAG", typeof(string));
                INDATA.Columns.Add("SUM_NO_INSP_FLAG", typeof(string));
                INDATA.Columns.Add("SUM_2ND_OCV_FLAG", typeof(string));
                INDATA.Columns.Add("SUM_QLTY_HOLD_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_RP_N_SEQ_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_RENTAL_RACK_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_MOVING_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_RETURN_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_VLD_DATE_OVER_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_BOX_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_PROC_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_NO_INSP_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_BIZWF_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_HOLD_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_PILOT_FLAG", typeof(string));

                INDATA.Columns.Add("P_WIP_TOTL_STAT_CODE", typeof(string));
                INDATA.Columns.Add("P_ATTR_01", typeof(string));
                INDATA.Columns.Add("P_ATTR_02", typeof(string));
                INDATA.Columns.Add("P_ATTR_03", typeof(string));
                INDATA.Columns.Add("P_ATTR_04", typeof(string));
                INDATA.Columns.Add("P_ATTR_05", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIP_TYPE"] = rdoWIP.IsChecked.Equals(true) ? "WIP" : "WIP_SNAP" ;
                dr["WIP_DETL_TYPE"] = "SUM";
                dr["UI_TYPE"] = "PACK";
                dr["SUM_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                dr["WH_ID"] = null;
                dr["RACK_ID"] = null;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = null;
                dr["PRDT_CLSS_CODE"] = null;
                dr["FORM_WRK_TYPE_CODE"] = null;
                dr["PRODID"] = null;
                dr["MODLID"] = null;
                dr["PRJT_NAME"] = null;
                dr["LOTID"] = null;
                dr["PKG_LOTID"] = null;
                dr["GRP_MKT_TYPE_CODE_FLAG"] = null;
                dr["GRP_PROD_VER_CODE_FLAG"] = null;
                dr["GRP_RP_SEQ_FLAG"] = "N";
                dr["SUM_NO_INSP_FLAG"] = "N";
                dr["SUM_2ND_OCV_FLAG"] = "Y";
                dr["SUM_QLTY_HOLD_FLAG"] = "A";
                dr["CHK_RP_N_SEQ_FLAG"] = "A";
                dr["CHK_RENTAL_RACK_FLAG"] = "A";
                dr["CHK_MOVING_FLAG"] = "A";
                dr["CHK_RETURN_FLAG"] = "A";
                dr["CHK_VLD_DATE_OVER_FLAG"] = "A";
                dr["CHK_BOX_FLAG"] = "A";
                dr["CHK_PROC_FLAG"] = "A";
                dr["CHK_NO_INSP_FLAG"] = "A";
                dr["CHK_BIZWF_FLAG"] = "A";
                dr["CHK_HOLD_FLAG"] = "A";
                dr["CHK_PILOT_FLAG"] = "A";

                dr["P_WIP_TOTL_STAT_CODE"] = null;
                dr["P_ATTR_01"] = null;
                dr["P_ATTR_02"] = null;
                dr["P_ATTR_03"] = null;
                dr["P_ATTR_04"] = null;
                dr["P_ATTR_05"] = null;

                INDATA.Rows.Add(dr);

                dsResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_STATUS_QUALTITY2", "INDATA", "OUTDATA", INDATA);

                Util.gridClear(dgWipList);
                Util.gridClear(dgPackInfo);
                Util.GridSetData(dgWipList, dsResult, FrameOperation, true);

                strRdoType = rdoWIP.IsChecked.Equals(true) ? "WIP" : "WIP_SNAP";
                strSumDate = dtpDate.SelectedDateTime.ToString("yyyyMMdd");

                if (dsResult.Rows.Count > 0)
                {
                    tbRetrunListCount.Text = "[ " + dsResult.Rows.Count.ToString() + ObjectDic.Instance.GetObjectName("건") + " ]";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getWipList_Detail_Procedure(string strAreaId, string strProdid, string sPRDT_CLSS_CODE, string sSTAT_TYPE_CODE, string strEqsgId, string strRoutId)
        {
            try
            {
                DataTable dsResult = new DataTable();

                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WIP_TYPE", typeof(string));
                INDATA.Columns.Add("WIP_DETL_TYPE", typeof(string));
                INDATA.Columns.Add("UI_TYPE", typeof(string));
                INDATA.Columns.Add("SUM_DATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PKG_LOTID", typeof(string));
                INDATA.Columns.Add("GRP_MKT_TYPE_CODE_FLAG", typeof(string));
                INDATA.Columns.Add("GRP_PROD_VER_CODE_FLAG", typeof(string));
                INDATA.Columns.Add("GRP_RP_SEQ_FLAG", typeof(string));
                INDATA.Columns.Add("SUM_NO_INSP_FLAG", typeof(string));
                INDATA.Columns.Add("SUM_2ND_OCV_FLAG", typeof(string));
                INDATA.Columns.Add("SUM_QLTY_HOLD_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_RP_N_SEQ_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_RENTAL_RACK_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_MOVING_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_RETURN_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_VLD_DATE_OVER_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_BOX_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_PROC_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_NO_INSP_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_BIZWF_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_HOLD_FLAG", typeof(string));
                INDATA.Columns.Add("CHK_PILOT_FLAG", typeof(string));
                INDATA.Columns.Add("P_WIP_TOTL_STAT_CODE", typeof(string));                


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIP_TYPE"] = strRdoType;
                dr["WIP_DETL_TYPE"] = "LOT";
                dr["UI_TYPE"] = "PACK";
                dr["SUM_DATE"] = strSumDate;
                dr["AREAID"] = strAreaId;
                dr["WH_ID"] = null;
                dr["RACK_ID"] = null;
                dr["EQSGID"] = strEqsgId;
                dr["ROUTID"] = strRoutId;
                dr["PROCID"] = null; //조건별로 공정ID 지정은 비즈 안에서 구분
                dr["PRDT_CLSS_CODE"] = sPRDT_CLSS_CODE;
                dr["FORM_WRK_TYPE_CODE"] = null;
                dr["PRODID"] = strProdid;
                dr["MODLID"] = null;
                dr["PRJT_NAME"] = null;
                dr["LOTID"] = null;
                dr["PKG_LOTID"] = null;
                dr["GRP_MKT_TYPE_CODE_FLAG"] = null;
                dr["GRP_PROD_VER_CODE_FLAG"] = null;
                dr["GRP_RP_SEQ_FLAG"] = "N";
                dr["SUM_NO_INSP_FLAG"] = "N";
                dr["SUM_2ND_OCV_FLAG"] = "Y";
                dr["SUM_QLTY_HOLD_FLAG"] = "A";
                dr["CHK_RP_N_SEQ_FLAG"] = "A";
                dr["CHK_RENTAL_RACK_FLAG"] = "A";
                dr["CHK_MOVING_FLAG"] = "A";
                dr["CHK_RETURN_FLAG"] = "A";
                dr["CHK_VLD_DATE_OVER_FLAG"] = "A";
                dr["CHK_BOX_FLAG"] = "A";
                dr["CHK_PROC_FLAG"] = "A";
                dr["CHK_NO_INSP_FLAG"] = "A";
                dr["CHK_BIZWF_FLAG"] = "A";
                dr["CHK_HOLD_FLAG"] = "A";
                dr["CHK_PILOT_FLAG"] = "A";
                dr["P_WIP_TOTL_STAT_CODE"] = sSTAT_TYPE_CODE.Trim();                

                INDATA.Rows.Add(dr);

                dsResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_STATUS_PACK_DETAIL_QUANTITY", "INDATA", "OUTDATA", INDATA);

                Util.gridClear(dgPackInfo);

                Util.GridSetData(dgPackInfo, dsResult, FrameOperation, true);

                tbPackInfoCount.Text = "[ " + dsResult.Rows.Count.ToString() + ObjectDic.Instance.GetObjectName("건") + " ]";
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.12.08
        private void getWipMovingList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                dr["PRODID"] = Util.NVC(cboProduct.SelectedValue.ToString()) == "" ? null : cboProduct.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_CELLMOVING", "RQSTDT", "RSLTDT", RQSTDT);
                Util.gridClear(dgWipMoving);
                Util.gridClear(dgCellInfo);                
                Util.GridSetData(dgWipMoving, dtResult, FrameOperation, true);

                if (dtResult.Rows.Count > 0)
                {
                    tbRetrunListCount_H1.Text = "[ " + dtResult.Rows.Count.ToString() + " 건 ]";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.12.08
        //private void getWipMovingList_Detail(string strRCVISSid, string strProdid, string strToAreaid, string strBOXid)
        private void getWipMovingList_Detail(string strProdid, string strToAreaid, string strFromAreaid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("AREAID2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["RCV_ISS_ID"] = strRCVISSid;
                dr["PRODID"] = strProdid;
                dr["AREAID"] = strToAreaid;
                //dr["BOXID"] = strBOXid;
                dr["AREAID2"] = strFromAreaid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult3 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_CELLMOVING_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCellInfo, dtResult3, FrameOperation, true);

                tbPackInfoCount_M1.Text = "[ " + dtResult3.Rows.Count.ToString() + " 건 ]";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Combo

        private void Set_Combo_Area(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { sSHOPID, Area_Type.PACK };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        private void SetCboEQSG(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                //20202.07.16
                //String[] sFilter = { cboAreaByAreaType.SelectedValue.ToString() };
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
                if (tcMain.SelectedIndex == 0)
                {
                    String[] sFilter = { cboAreaByAreaType.SelectedValue.ToString() };
                    _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
                }
                //else if (tcMain.SelectedIndex == 1) //이력 탭 삭제로 뺴야함
                //{
                //    String[] sFilter = { AREA_AREATYPE.SelectedValue.ToString() };
                //    _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
                //}
                else if (tcMain.SelectedIndex == 1)
                {

                }
            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        //2018.12.08
        private void Set_Combo_Area_Move(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { sSHOPID, Area_Type.PACK };
                _combo.SetCombo(cboArea_Areatype, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        //2018.12.08
        private void Set_Combo_Product(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { sSHOPID, cboArea_Areatype.SelectedValue.ToString(), null, null, null, Area_Type.PACK, "CELL" };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        #endregion Combo

        #endregion Mehod

        //2020.07.16
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tab_idx = tcMain.SelectedIndex;

            if (tab_idx < 0)
            {
                return;
            }
        }

        private void chk2ndOcv_Checked(object sender, RoutedEventArgs e)
        {
            if (chk2ndOcv.IsChecked.Equals(true))
            {
                dgWipList.Columns["TOTL_2ND_OCV"].Visibility = Visibility.Visible;
            }
            else
            {
                dgWipList.Columns["TOTL_2ND_OCV"].Visibility = Visibility.Collapsed;
            }
        }

        private void rdoWIP_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpDate != null && rdoWIP.IsChecked.Equals(true))
            {
                dtpDate.IsEnabled = false;
                dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;
                dgWipList.Columns["TOTL_RCV_SHIPPING"].Visibility = Visibility.Visible;
            }
        }
        private void rdoWIP_SNAP_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoWIP_SNAP.IsChecked.Equals(true))
            {
                dtpDate.IsEnabled = true;
                dgWipList.Columns["TOTL_RCV_SHIPPING"].Visibility = Visibility.Collapsed;
            }
        }

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }
    }
}
