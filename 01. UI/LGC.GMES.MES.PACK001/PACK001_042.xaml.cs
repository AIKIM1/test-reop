/*************************************************************************************
 Created Date : 2019.06.17
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2019.06.17 손우석 CSR ID 3970086 GMES 내 보류재고 관리 시스템화 기능 구현_기간별 현황표시, 보류재고 현황 관리 탭 추가. [요청번호]C20190409_70086
 2019.08.29 손우석 CSR ID 3970086 GMES 내 보류재고 관리 시스템화 기능 구현_기간별 현황표시, 보류재고 현황 관리 탭 추가. [요청번호]C20190409_70086
 2019.08.30 손우석 CSR ID 3970086 GMES 내 보류재고 관리 시스템화 기능 구현_기간별 현황표시, 보류재고 현황 관리 탭 추가. [요청번호]C20190409_70086
 2020.01.06 손우석 CSR ID 17266 GMES 보류재고 Summary 기능 개선 요청 건 [요청번호 C20200102-000782]
 2020.01.16 손우석 CSR ID 17266 GMES 보류재고 Summary 기능 개선 요청 건 [요청번호 C20200102-000782]
 2020.01.28 손우석 CSR ID 17266 GMES 보류재고 Summary 기능 개선 요청 건 [요청번호 C20200102-000782]
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_042 : UserControl
    {
        private object lockObject = new object();
        private string sMODELID = string.Empty;
        private string sLINEID = string.Empty;

        #region Declaration & Constructor 
        public PACK001_042()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion Declaration & Constructor 

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //2019.08.30
                dtpDateFrom.SelectedDateTime = DateTime.Today.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Today;

                setComboBox();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                ////동
                String[] sFilterTabLine = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                C1ComboBox[] cboLineabAreaChild = { cboEquipmentSegment };
                //2019.08.29
                //_combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.NONE, cbChild: cboLineabAreaChild, sFilter: sFilterTabLine, sCase: "AREA_AREATYPE");
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.ALL, cbChild: cboLineabAreaChild, sFilter: sFilterTabLine, sCase: "AREA_AREATYPE");

                ////라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                //2019.08.29
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent, sCase: "EQUIPMENTSEGMENT");
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent, sCase: "EQUIPMENTSEGMENT");

                Set_Combo_ProductModel(cboProductModel);

                //2019.08.30
                SetcboCompleteCode();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Initialize

        #region Method
        private void Set_Combo_ProductModel(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["USERID"] = LoginInfo.USERID;
                dtRQSTDT.Rows.Add(drnewrow);

                //2019.08.29
                //new ClientProxy().ExecuteService("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //{
                //    if (Exception != null)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                //        return;
                //    }
                //    cbo.ItemsSource = DataTableConverter.Convert(result);
                //    cbo.SelectedIndex = 0;
                //});

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                DataRow dRow = dtRslt.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = "";
                dtRslt.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            //if (loadingIndicator != null)
            //    loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            //if (loadingIndicator != null)
            //    loadingIndicator.Visibility = Visibility.Collapsed;
        }
        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_042_POPUP popup = new PACK001_042_POPUP();
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    //2020.01.06
                    Search();
                }
                else
                {
                    //2020.01.28
                    Search();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.08.30
        private void SetcboCompleteCode()
        {
            try
            {
                //2020.01.06
                string sSelectedValue = cboComplete.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_STAT";
                dr["ATTRIBUTE1"] = "STAT";
                dr["ATTRIBUTE2"] = null;
                dr["ATTRIBUTE3"] = null;
                dr["ATTRIBUTE4"] = null;
                dr["ATTRIBUTE5"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_V2", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                //2020.01.06
                //dRow["CBO_NAME"] = "-ALL-";
                //dRow["CBO_CODE"] = "";
                //dtResult.Rows.InsertAt(dRow, 0);

                cboComplete.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboComplete.Check(i);
                                break;
                            }
                        }
                    }
                }

                //2020.01.06
                //cboComplete.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Method

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2020.01.06
                ////ShowLoadingIndicator();

                ////2019.08.28
                //string strEQSG = string.Empty;
                //string strModel = string.Empty;
                ////2019.08.30
                //string strCMPL = string.Empty;

                //DataSet dsInput = new DataSet();
                //DataTable INDATA = new DataTable();

                //INDATA.TableName = "INDATA";
                //INDATA.Columns.Add("LANGID", typeof(string));
                //INDATA.Columns.Add("EQSGID", typeof(string));
                //INDATA.Columns.Add("MODEL", typeof(string));
                //INDATA.Columns.Add("OCCR_DATE", typeof(string));
                ////2019.08.30
                //INDATA.Columns.Add("OCCR_TO_DATE", typeof(string));
                //INDATA.Columns.Add("PACK_HOLD_STCK_CMPL_CODE", typeof(string));

                //DataRow dr = INDATA.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["OCCR_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                ////2019.08.30
                //dr["OCCR_TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                //switch (cboEquipmentSegment.SelectedValue.ToString())
                //{
                //    case "":
                //        strEQSG = null;
                //        break;
                //    default:
                //        strEQSG = cboEquipmentSegment.SelectedValue.ToString();
                //        break;
                //}
                //dr["EQSGID"] = strEQSG;

                //switch (cboProductModel.SelectedValue.ToString())
                //{
                //    case "":
                //        strModel = null;
                //        break;
                //    default:
                //        strModel = cboProductModel.SelectedValue.ToString();
                //        break;
                //}
                //dr["MODEL"] = strModel;
                ////2019.08.30
                //switch (cboComplete.SelectedValue.ToString())
                //{
                //    case "":
                //        strCMPL = null;
                //        break;
                //    default:
                //        strCMPL = cboComplete.SelectedValue.ToString();
                //        break;
                //}
                //dr["PACK_HOLD_STCK_CMPL_CODE"] = strCMPL;

                //INDATA.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_HOLD_SUMMARY", "RQSTDT", "RSLTDT", INDATA);
                //Util.GridSetData(dgLineTabSearch, dtRslt, FrameOperation);
                Search();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2020.01.06
        private void Search()
        {
            try
            {
                //ShowLoadingIndicator();

                //2019.08.28
                string strEQSG = string.Empty;
                string strModel = string.Empty;
                //2019.08.30
                string strCMPL = string.Empty;
                //2020.01.28
                string strAREA = string.Empty;

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("MODEL", typeof(string));
                INDATA.Columns.Add("OCCR_DATE", typeof(string));
                //2019.08.30
                INDATA.Columns.Add("OCCR_TO_DATE", typeof(string));
                INDATA.Columns.Add("PACK_HOLD_STCK_CMPL_CODE", typeof(string));
                //2020.01.28
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OCCR_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                //2019.08.30
                dr["OCCR_TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                switch (cboEquipmentSegment.SelectedValue.ToString())
                {
                    case "":
                        strEQSG = null;
                        break;
                    default:
                        strEQSG = cboEquipmentSegment.SelectedValue.ToString();
                        break;
                }
                dr["EQSGID"] = strEQSG;

                switch (cboProductModel.SelectedValue.ToString())
                {
                    case "":
                        strModel = null;
                        break;
                    default:
                        strModel = cboProductModel.SelectedValue.ToString();
                        break;
                }
                dr["MODEL"] = strModel;
                //2020.01.06
                strCMPL = cboComplete.SelectedItemsToString == "" ? null : cboComplete.SelectedItemsToString;
                ////2019.08.30
                //switch (cboComplete.SelectedValue.ToString())
                //{
                //    case "":
                //        strCMPL = null;
                //        break;
                //    default:
                //        strCMPL = cboComplete.SelectedValue.ToString();
                //        break;
                //}
                dr["PACK_HOLD_STCK_CMPL_CODE"] = strCMPL;

                //2020.01.28
                switch (cboAreaByAreaType.SelectedValue.ToString())
                {
                    case "":
                        strAREA = null;
                        break;
                    default:
                        strAREA = cboAreaByAreaType.SelectedValue.ToString();
                        break;
                }
                dr["AREAID"] = strAREA;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_HOLD_SUMMARY", "RQSTDT", "RSLTDT", INDATA);
                Util.GridSetData(dgLineTabSearch, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                PACK001_042_POPUP popup = new PACK001_042_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = "C";
                    Parameters[1] = LoginInfo.CFG_SHOP_ID.ToString();
                    //Parameters[2] = cboAreaByAreaType.SelectedValue.ToString();
                    //Parameters[3] = cboEquipmentSegment.SelectedValue.ToString();
                    //Parameters[4] = cboProductModel.SelectedValue.ToString();

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
                else
                {
                    object[] Parameters = new object[20];
                    Parameters[0] = "U";
                    Parameters[1] = LoginInfo.CFG_SHOP_ID.ToString();
                    //Parameters[2] = cboAreaByAreaType.SelectedValue.ToString();
                    //Parameters[3] = cboEquipmentSegment.SelectedValue.ToString();
                    //Parameters[4] = cboProductModel.SelectedValue.ToString();

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex > -1)
            {
                sLINEID = Convert.ToString(cboEquipmentSegment.SelectedValue);
                Set_Combo_ProductModel(cboProductModel);
            }
            else
            {
                sLINEID = string.Empty;
            }
        }

        private void dgLineTabSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
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
                        #region DiffDay
                        //2020.01.06
                        if (e.Cell.Column.Name == "DiffDay" && e.Cell.Value != null && e.Cell.Value.ToString() != "")
                        {
                            int nValue = Int32.Parse(e.Cell.Value.ToString());

                            if (e.Cell.Column.Name == "DiffDay")
                            {
                                // (0개월 ~1개월 미만 - 녹색 / 1개월이상~ 3개월 미만 - 노란색 / 3개월 이상 - 빨간색)
                                if (nValue < 1)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                                else if ((nValue >= 1) && (nValue < 3))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                                else if (nValue >= 3)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        #endregion DiffDay

                        #region PACK_HOLD_STCK_CMPL_CODE
                        else if (e.Cell.Column.Name == "PACK_HOLD_STCK_CMPL_CODE" && e.Cell.Value != null && e.Cell.Value.ToString() != "")
                        {
                            if (e.Cell.Column.Name == "PACK_HOLD_STCK_CMPL_CODE")
                            {
                                string sValue = e.Cell.Value.ToString();
                                //PACK001_042_04 완료 - 녹색 / PACK001_042_02 진행중 - 노란색 / PACK001_042_03 지연 - 빨간색

                                if (sValue == "PACK001_042_04")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                                else if (sValue == "PACK001_042_02")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                                else if (sValue == "PACK001_042_03")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        #endregion PACK_HOLD_STCK_CMPL_CODE

                        #region PACK_HOLD_STCK_CMPL_NAME
                        else if (e.Cell.Column.Name == "PACK_HOLD_STCK_CMPL_NAME" && e.Cell.Value != null && e.Cell.Value.ToString() != "")
                        {
                            if (e.Cell.Column.Name == "PACK_HOLD_STCK_CMPL_NAME")
                            {
                                string sValue = e.Cell.Value.ToString();
                                //PACK001_042_04 완료 - 녹색 / PACK001_042_02 진행중 - 노란색 / PACK001_042_03 지연 - 빨간색

                                if (sValue == "완료")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                                else if (sValue == "진행중")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                                else if (sValue == "지연")
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                        #endregion PACK_HOLD_STCK_CMPL_NAME

                        #region PRODID
                        else if (e.Cell.Column.Name == "PRODID" && e.Cell.Value != null && e.Cell.Value.ToString() != "")
                        {
                            if (e.Cell.Column.Name == "PRODID")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            }
                        }
                        #endregion PRODID

                        //2020.01.16
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }
                ));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgLineTabSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string sComplete = string.Empty;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgLineTabSearch.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    //2020.01.06
                    if (cell.Column.Name == "PRODID")
                    {
                        sComplete = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PACK_HOLD_STCK_CMPL_CODE"));

                        PACK001_042_POPUP popup = new PACK001_042_POPUP();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            //2019.08.30
                            //if (sComplete == "PACK001_042_02")  //진행중
                            //{
                            //    object[] Parameters = new object[22];

                            //    Parameters[0] = "U";
                            //    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "SHOPID"));
                            //    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "EQSGID").ToString().Substring(0, 2));
                            //    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "EQSGID"));
                            //    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MODEL"));
                            //    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "SLOC_ID"));
                            //    Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MTRLTYPE"));
                            //    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRODID"));
                            //    Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRODNAME"));
                            //    Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MTRLID"));
                            //    Parameters[10] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "OCCR_YMD"));
                            //    Parameters[11] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "OCCR_RSN_CNTT"));
                            //    Parameters[12] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "STCK_QTY"));
                            //    Parameters[13] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRDT_BAS_QTY"));
                            //    Parameters[14] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_TEAM"));
                            //    Parameters[15] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_TEAMNAME"));
                            //    Parameters[16] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_USERID"));
                            //    Parameters[17] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_USERNAME"));
                            //    Parameters[18] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PROG_CNTT"));
                            //    Parameters[19] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CMPL_SCHD_YMD"));
                            //    Parameters[20] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PACK_HOLD_STCK_CMPL_CODE"));
                            //    Parameters[21] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "NOTE"));

                            //    C1WindowExtension.SetParameters(popup, Parameters);
                            //}
                            //else
                            //{
                            //    object[] Parameters = new object[22];

                            //    switch (sComplete)
                            //    {
                            //        case "PACK001_042_04":   //완료

                            //            Parameters[0] = "S";
                            //            break;

                            //        case "PACK001_042_03":    //지연
                            //        case "PACK001_042_01":    //대기
                            //            Parameters[0] = "W";
                            //            break;
                            //    }

                            //    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "SHOPID"));
                            //    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "EQSGID").ToString().Substring(0, 2));
                            //    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "EQSGID"));
                            //    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MODEL"));
                            //    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "SLOC_ID"));
                            //    Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MTRLTYPE"));
                            //    Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRODID"));
                            //    Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRODNAME"));
                            //    Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MTRLID"));
                            //    Parameters[10] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "OCCR_YMD"));
                            //    Parameters[11] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "OCCR_RSN_CNTT"));
                            //    Parameters[12] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "STCK_QTY"));
                            //    Parameters[13] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRDT_BAS_QTY"));
                            //    Parameters[14] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_TEAM"));
                            //    Parameters[15] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_TEAMNAME"));
                            //    Parameters[16] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_USERID"));
                            //    Parameters[17] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_USERNAME"));
                            //    Parameters[18] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PROG_CNTT"));
                            //    Parameters[19] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CMPL_SCHD_YMD"));
                            //    Parameters[20] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PACK_HOLD_STCK_CMPL_CODE"));
                            //    Parameters[21] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "NOTE"));

                            //    C1WindowExtension.SetParameters(popup, Parameters);
                            //}

                            //2020.01.06
                            //object[] Parameters = new object[22];
                            object[] Parameters = new object[23];

                            switch (sComplete)
                            {
                                case "PACK001_042_01": //대기
                                    Parameters[0] = "U";
                                    break;
                                case "PACK001_042_02": //진행 중
                                    Parameters[0] = "U";
                                    break;
                                case "PACK001_042_03": //지연
                                    Parameters[0] = "U";
                                    break;
                                case "PACK001_042_04": //완료
                                    Parameters[0] = "S";
                                    break;
                            }

                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "SHOPID"));
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "EQSGID").ToString().Substring(0, 2));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "EQSGID"));
                            Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MODEL"));
                            Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "SLOC_ID"));
                            Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MTRLTYPE"));
                            Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRODID"));
                            Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRODNAME"));
                            Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "MTRLID"));
                            Parameters[10] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "OCCR_YMD"));
                            Parameters[11] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "OCCR_RSN_CNTT"));
                            Parameters[12] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "STCK_QTY"));
                            Parameters[13] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PRDT_BAS_QTY"));
                            Parameters[14] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_TEAM"));
                            Parameters[15] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_TEAMNAME"));
                            Parameters[16] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_USERID"));
                            Parameters[17] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CHARGE_USERNAME"));
                            Parameters[18] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PROG_CNTT"));
                            Parameters[19] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "CMPL_SCHD_YMD"));
                            Parameters[20] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "PACK_HOLD_STCK_CMPL_CODE"));
                            Parameters[21] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "NOTE"));
                            //2020.01.06
                            Parameters[22] = Util.NVC(DataTableConverter.GetValue(dgLineTabSearch.Rows[cell.Row.Index].DataItem, "TOTL_PRICE"));

                            C1WindowExtension.SetParameters(popup, Parameters);

                            popup.Closed -= popup_Closed;
                            popup.Closed += popup_Closed;
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        #endregion Event

        private void btnExcel_Compare_Detail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgLineTabSearch);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}