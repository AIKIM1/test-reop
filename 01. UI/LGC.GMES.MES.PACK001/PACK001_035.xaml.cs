/*************************************************************************************
 Created Date : 2018.08.30
      Creator : 손우석
   Decription : 라인내 재공조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.30 손우석 3739488 GMES 라인내 재공 정보 조회 화면 구현 件 요청번호 C20180713_39488
  2018.10.01 손우석 [PJ20180352I 전지 GMES 기능 추가 개선(New) 프로젝트] - 재공조회(Pack)
  2018.10.05 손우석 [PJ20180352I 전지 GMES 기능 추가 개선(New) 프로젝트] - 재공조회(Pack)
  2018.12.08 손우석 CSR ID 3865530 CWA Pack 이동중 재고 조회기능 화면 추가 요청 [요청번호] C20181207_65530
  2018.12.24 손우석 CSR ID 3865530 CWA Pack 이동중 재고 조회기능 화면 추가 요청 [요청번호] C20181207_65530
  2018.12.27 손우석 CSR ID 3865530 CWA Pack 이동중 재고 조회기능 화면 추가 요청 [요청번호] C20181207_65530
  2019.04.25 손우석 재공현황 조회 이력 조회 호출 Method 변경
  2020.06.08 김준겸 UI전환시 기존에 설정된 값 디폴트 값으로 변경되는 오류 해결.
  2020.07.19 손우석 동 관련 이벤트 처리 오류 수정
  2021.06.19 최우석 집계, 상세 정보 조회 기준 수정 [요청번호] C20210517-000184
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
    public partial class PACK001_035 : UserControl, IWorkArea
    {
        private string sSHOPID = string.Empty;
        private string sAREAID = string.Empty;
        private string sLINEID = string.Empty;
        //202.007.19
        private string sPRODID = string.Empty;

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

        public PACK001_035()
        {
            InitializeComponent();

            InitCombo();
        }
        #endregion Declaration & Constructor 

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //2018.10.01
            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            //2018.09.07
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            //dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            //Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
            //Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

            // InitCombo(); 
        }

        private void InitCombo()
        {
            sSHOPID = LoginInfo.CFG_SHOP_ID;


            Set_Combo_Area(cboAreaByAreaType);
            SetCboEQSG(cboEquipmentSegment);

            //2018.10.01
            Set_Combo_Area_His(AREA_AREATYPE);
            SetCboEQSG_His(cboLine);

            //2018.12.08
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
                if (cboAreaByAreaType.SelectedIndex == 0)
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (cboEquipmentSegment.SelectedIndex == 0)
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }

                getWipList();
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

        private void btnSearch_H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AREA_AREATYPE.SelectedValue.ToString() == "")
                {
                    Util.Alert("SFU1499");  //동을 선택하세요.
                    return;
                }

                if (cboLine.SelectedValue.ToString() == "")
                {
                    String AreaId = AREA_AREATYPE.SelectedValue.ToString();
                    getWipSnapList(AreaId);  //라인을 선택하세요.
                    return;
                }

                //2019.04.25
                //getWipList();
                getWipSnapList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnExcel_H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgWipList_H);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcel1_H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgPackInfo_H);
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

                if (cell != null && !cell.Value.ToString().Equals("0") && cell.Column.Tag != null)
                {
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sPRODTYPE = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRDTYPE"));
                    string strEqsgId = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "EQSGID"));
                    string sWIPSTATFLAG = Util.NVC(cell.Column.Tag.ToString()).Trim();

                    /* xaml tag값으로 설정
                    WAIT_QTY    = 1
                    PROC_QTY    = 2
                    PR000_QTY   = 3
                    PS000_QTY   = 4
                    P5400_QTY   = 5
                    END_QTY     = 6
                    PACKING_QTY = 7
                    PB000_QTY   = 8
                    MESHOLD_QTY = 9
                    QMSHOLD_QTY = 10
                    DBLHOLD_QTY = 11   
                    BIZWF_QTY   = 13                  

                    if (cell.Column.Name.Equals("WAIT_QTY"))
                    {
                        sWIPSTATFLAG = "0";
                    }
                    else if (cell.Column.Name.Equals("PROC_QTY"))
                    {
                        sWIPSTATFLAG = "1";
                    }
                    else if (cell.Column.Name.Equals("HOLD_QTY"))
                    {
                        sWIPSTATFLAG = "2";
                    }
                    else if (cell.Column.Name.Equals("PR_PROC_QTY"))
                    {
                        sWIPSTATFLAG = "3";
                    }
                    else if (cell.Column.Name.Equals("PS_PROC_QTY"))
                    {
                        sWIPSTATFLAG = "4";
                    }
                    */

                    getWipList_Detail(sPRODID, sPRODTYPE, sWIPSTATFLAG, strEqsgId);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgWipList_H_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList_H.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList_H.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sPRODTYPE = Util.NVC(DataTableConverter.GetValue(dgWipList_H.Rows[cell.Row.Index].DataItem, "PRDTYPE"));
                    string strEqsgID = Util.NVC(DataTableConverter.GetValue(dgWipList_H.Rows[cell.Row.Index].DataItem, "EQSGID"));
                    string sWIPSTATFLAG = "";

                    if (cell.Column.Name.Equals("WAIT_QTY"))
                    {
                        sWIPSTATFLAG = "0";
                    }
                    else if (cell.Column.Name.Equals("PROC_QTY"))
                    {
                        sWIPSTATFLAG = "1";
                    }
                    else if (cell.Column.Name.Equals("HOLD_QTY"))
                    {
                        sWIPSTATFLAG = "2";
                    }

                    getWipSnapList_Detail(sPRODID, sPRODTYPE, sWIPSTATFLAG, strEqsgID);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

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

                    if (e.Cell.Column.Name == "UPDDTTM_DETL" && e.Cell.Value != null && e.Cell.Value.ToString() != "")
                    {
                        string value = e.Cell.Value.ToString();

                        if (value != null && !value.Equals(""))
                        {
                            if (e.Cell.Column.Name == "UPDDTTM_DETL")
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

        #region Check
        //2018.10.01
        //private void chkToday_Checked(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
        //        //dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
        //        //cboTimeTo.SelectedValue = "23:59:59";
        //        dtpDateFrom.IsEnabled = false;
        //        //dtpDateTo.IsEnabled = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        private void chkToday_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDate.IsEnabled = true;
                //dtpDateTo.IsEnabled = true;
            }
            catch (Exception ex)
            {
                //2020.07.16
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion Check

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

        //2018.10.01
        private void AREA_AREATYPE_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (AREA_AREATYPE.SelectedIndex > -1)
            {
                sAREAID = Convert.ToString(AREA_AREATYPE.SelectedValue);
                SetCboEQSG(cboLine);
            }
            else
            {
                sAREAID = string.Empty;
            }
        }

        private void cboLine_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLine.SelectedIndex > -1)
            {
                sLINEID = Convert.ToString(cboLine.SelectedValue);

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
                sPRODID = Convert.ToString(cboLine.SelectedValue);
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

        private void getWipList_Detail(string strProdid, string strProdType, string strWipStatFlag, string strEqsgId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRODTYPE", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = strEqsgId;
                dr["PRODID"] = strProdid;
                dr["PRODTYPE"] = strProdType;
                dr["WIPSTAT_FLAG"] = strWipStatFlag;

                RQSTDT.Rows.Add(dr);

                Util.gridClear(dgPackInfo);

                //loadingIndicator.Visibility = Visibility.Visible;

                //DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_DETAIL_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_STATUS_DETAIL_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgPackInfo, dtResult1, FrameOperation, true);
                if(strWipStatFlag.Equals("12"))
                {
                    if(dgPackInfo.Columns[2].Tag.Equals("1"))
                        dgPackInfo.Columns[2].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgPackInfo.Columns[2].Tag.Equals("1"))
                        dgPackInfo.Columns[2].Visibility = Visibility.Collapsed;
                }

                tbPackInfoCount.Text = "[ " + dtResult1.Rows.Count.ToString() + " 건 ]";

                //new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_DETAIL_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //if (ex != null)
                //{
                //    Util.MessageException(ex);
                //    return;
                //}

                //   Util.GridSetData(dgWipList_H, dtResult, FrameOperation, true);
                //});
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getWipSnapList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SUM_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboLine.SelectedValue.ToString();
                dr["SUM_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPSNAP_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgWipList_H, dtResult2, FrameOperation, true);

                if (dtResult2.Rows.Count > 0)
                {
                    tbRetrunListCount_H.Text = "[ " + dtResult2.Rows.Count.ToString() + " 건 ]";
                }

                //loadingIndicator.Visibility = Visibility.Visible;
                //new ClientProxy().ExecuteService("DA_PRD_SEL_WIPSNAP_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //    if (ex != null)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }

                //    Util.GridSetData(dgWipList_H, dtResult, FrameOperation, true);
                //});
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2019.06.13
        private void getWipSnapList(string AreaId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SUM_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = AreaId + "%";
                dr["SUM_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPSNAP_QUANTITY_ALL", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgWipList_H, dtResult2, FrameOperation, true);

                if (dtResult2.Rows.Count > 0)
                {
                    tbRetrunListCount_H.Text = "[ " + dtResult2.Rows.Count.ToString() + " 건 ]";
                }

                //loadingIndicator.Visibility = Visibility.Visible;
                //new ClientProxy().ExecuteService("DA_PRD_SEL_WIPSNAP_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //    if (ex != null)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }

                //    Util.GridSetData(dgWipList_H, dtResult, FrameOperation, true);
                //});
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getWipSnapList_Detail(string strProdid, string strProdType, string strWipStatFlag, string strEqsgId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRODTYPE", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT_FLAG", typeof(string));
                RQSTDT.Columns.Add("SUM_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = strEqsgId;
                dr["PRODID"] = strProdid;
                dr["PRODTYPE"] = strProdType;
                dr["WIPSTAT_FLAG"] = strWipStatFlag;
                dr["SUM_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult3 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPSNAP_DETAIL_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgPackInfo_H, dtResult3, FrameOperation, true);

                tbPackInfoCount_H.Text = "[ " + dtResult3.Rows.Count.ToString() + " 건 ]";

                //loadingIndicator.Visibility = Visibility.Visible;

                //new ClientProxy().ExecuteService("DA_PRD_SEL_WIPSNAP_DETAIL_QUANTITY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //{
                //    loadingIndicator.Visibility = Visibility.Collapsed;

                //    if (ex != null)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }

                //    Util.GridSetData(dgPackInfo_H, dtResult, FrameOperation, true);
                //});
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

                //if (cbo.SelectedIndex > 0)
                //{
                //    sAREAID = cboAreaByAreaType.SelectedValue.ToString();
                //}
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
                else if (tcMain.SelectedIndex == 1)
                {
                    String[] sFilter = { AREA_AREATYPE.SelectedValue.ToString() };
                    _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
                }
                else if (tcMain.SelectedIndex == 2)
                {

                }

                //if (cbo.SelectedIndex > 0)
                //{
                //    sLINEID = cboEquipmentSegment.SelectedValue.ToString();
                //}
            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        private void Set_Combo_Area_His(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { sSHOPID, Area_Type.PACK };
                _combo.SetCombo(AREA_AREATYPE, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        private void SetCboEQSG_His(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { AREA_AREATYPE.SelectedValue.ToString() };
                _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                //2020.07.16
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageException(ex);
                return;
            }
        }

        private void SetcboProductModel()
        {
            try
            {
                //this.cboProductModel.SelectionChanged -= new System.EventHandler(this.cboProductModel_SelectionChanged);

                //string sSelectedValue = cboProductModel.SelectedItemsToString;
                //string[] sSelectedList = sSelectedValue.Split(',');

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));
                //RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                //RQSTDT.Columns.Add("USERID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["AREAID"] = cboAreaByAreaType.SelectedItemsToString;
                //dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["SYSTEM_ID"] = LoginInfo.SYSID;
                //dr["USERID"] = LoginInfo.USERID;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                //cboProductModel.ItemsSource = DataTableConverter.Convert(dtResult);


                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboProductModel.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                //    {
                //        cboProductModel.Check(i);
                //        break;
                //    }
                //}
                //this.cboProductModel.SelectionChanged += new System.EventHandler(this.cboProductModel_SelectionChanged);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                dgWipList.Columns[15].Visibility = Visibility.Visible;
            }
            else
            {
                dgWipList.Columns[15].Visibility = Visibility.Collapsed;
            }
        }
    }
}
