/*************************************************************************************
 Created Date :2019.12.28
      Creator :염규범
   Decription : CWA 오경석 책임님 요청의건, 
               재공 현황 화면 
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.28  염규범    SI          Initialize
  2020.01.03  염규범    SI          MultiSelectionBox 추가 및 엑셀 다운로드 구현
  2020.01.24  염규범    SI          HOLD 컬럼 추가
  2020.01.29  염규범    SI          컬럼명 안맞는 부분 수정의 건 
  2020.03.16  염규범    SI          Combo box에 따른, 컬럼내용 변경 처리
  2020.04.20  염규범    SI          DataTable 집계함수 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_059 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private string sEmpty_Lot = string.Empty;
        private string sTabId = string.Empty;
        private string strAreaId = LoginInfo.CFG_AREA_ID;

        private object lockObject = new object();


        int iByDay = 5;
        DateTime dtNow = new DateTime();

        string sCellSetDay = "1W";
        string sNoboxSetDay = "1W";
        string sBoxSetDay = "1W";

        string sNoBoxSetEqsgId = "";
        string sBoxSetEqsgId = "";
        #endregion

        #region [ Initialize ]
        public PACK001_059()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitializeCombo()
        {
            CommonCombo cbo = new CommonCombo();
            SetComboDay(cboCellDay);
            SetComboDay(cboUnBoxingDay);
            SetComboDay(cboBoxingDay);
            cboBoxingEquipmentSegment.ApplyTemplate();
            SetCboEQSG(cboBoxingEquipmentSegment);
            cboUnBoxingLine.ApplyTemplate();
            SetCboEQSG(cboUnBoxingLine);
        }
        #endregion

        #region [UserControl_Loaded]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbCellDetlListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbNoBoxListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbNoBoxDetlListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbBoxListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbBoxDetlListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            //SetHeaderLineBreak(dgBoxingList);
        }

        #endregion

        #region Cell 재공 Event
        //검색 버튼
        private void btnCellSearch_Click(object sender, RoutedEventArgs e)
        {
            dtNow = DateTime.Now;
            iByDay = int.Parse(cboCellDay.SelectedValue.ToString());

            sCellSetDay = SetHeaderReplace(dgCellList, cboCellDay);

            SetCellStock(dtNow);
        }

        //스프레드 숫자 클릭
        private void dgCellList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.GetCellFromPoint(pnt);

                if (crrCell != null)
                {
                    if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                    {
                        string sCrrPjtGrp = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PACK_PRDT_GR_CODE"));
                        string sCrrPjtName = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PRJT_NAME"));
                        string sCrrProdid = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PRODID"));

                        SetCellStockDetail(sCrrPjtGrp, sCrrPjtName, sCrrProdid, crrCell.Column.Tag.ToString(), dtNow);
                    }
                }
            }
            catch { }
        }
        #endregion

        #region No Boxing 재공 Event
        private void btnUnBoxingSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                    dtNow = DateTime.Now;
                    iByDay = int.Parse(cboUnBoxingDay.SelectedValue.ToString());

                    sNoboxSetDay = SetHeaderReplace(dgUnBoxingList, cboUnBoxingDay);

                    sNoBoxSetEqsgId = Convert.ToString(cboUnBoxingLine.SelectedItemsToString);

                    if (string.IsNullOrEmpty(sNoBoxSetEqsgId))
                    {
                        Util.MessageInfo("SFU1223");
                        return;
                    }

                    //SetNoboxStock(dtNow, sNoBoxSetEqsgId);

                    string[] saLine = sNoBoxSetEqsgId.Split(new char[] { ',' });

                    DataTable dtTemp = new DataTable();
                    DataTable dtInputDataTable = new DataTable();
                    DataTable dt_Return = new DataTable();

                    loadingIndicator.Visibility = Visibility.Visible;
                        DoEvents();
                     
                            foreach (string strLineTemp in saLine)
                            {
                                dtTemp = SetNoboxStockMulti(dtNow, strLineTemp);
                                if (dtTemp.Rows.Count > 0)
                                {
                                    if (dtInputDataTable.Columns.Count == 0)
                                    {
                                        dtInputDataTable = dtTemp.Clone();
                                    }

                                   dtTemp.AsEnumerable().CopyToDataTable(dtInputDataTable, LoadOption.Upsert);
                                }
                            }

                    string[] grCode = new string[2] { "PACK_PRDT_GR_CODE", "PRODTYPE" };

                    string[] colName = dtInputDataTable.Columns.Cast<DataColumn>()
                                                                        .Where(x => !(x.ColumnName.Contains("PACK_PRDT_GR_CODE")) && !(x.ColumnName.Contains("PRODTYPE")))
                                                                        .Select(x => x.ColumnName)
                                                                        .ToArray();

                    dt_Return = Util.GetGroupBySum(dtInputDataTable, colName, grCode, true);

                    if (dtInputDataTable.Rows.Count > 0)
                        {
                            Util.GridSetData(dgUnBoxingList, dt_Return, FrameOperation, false);
                            Util.SetTextBlockText_DataGridRowCount(tbNoBoxListCount, Util.NVC(dt_Return.Rows.Count));
                        }
                        else
                        {
                            Util.MessageInfo("SFU1498");
                        }

                   loadingIndicator.Visibility = Visibility.Collapsed;
                     
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
            //SetNoboxStock(dtNow, sNoBoxSetEqsgId);
        }

    private void dgUnBoxingList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.GetCellFromPoint(pnt);

                if (crrCell != null)
                {
                    if (crrCell.Row.Index == -1)
                    {
                        return;
                    }

                    if (4 <= Convert.ToInt32(crrCell.Column.Tag.ToString()) && Convert.ToInt32(crrCell.Column.Tag.ToString()) <= 9)
                    {
                        NOBOX_HOLDFLAG.Visibility = Visibility.Visible;
                        NOBOX_NOTE.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        NOBOX_HOLDFLAG.Visibility = Visibility.Collapsed;
                        NOBOX_NOTE.Visibility = Visibility.Collapsed;
                    }

                    if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                    {
                        string sCrrPjtGrp = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PACK_PRDT_GR_CODE"));
                        string sPrdType = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PRODTYPE"));

                        SetNoBoxStockDetail(sCrrPjtGrp, sPrdType, crrCell.Column.Tag.ToString(), dtNow);
                    }
                }
            }
            catch { }
        }
        #endregion

        #region Boxing 재공 Event
        private void btnBoxingSearch_Click(object sender, RoutedEventArgs e)
        {
            dtNow = DateTime.Now;
            iByDay = int.Parse(cboBoxingDay.SelectedValue.ToString());

            sBoxSetDay = SetHeaderReplace(dgBoxingList, cboBoxingDay);

            sBoxSetEqsgId = Convert.ToString(cboBoxingEquipmentSegment.SelectedItemsToString);

            if (string.IsNullOrEmpty(sBoxSetEqsgId))
            {
                Util.MessageInfo("SFU1223");
                return;
            }

            string[] saLine = sBoxSetEqsgId.Split(new char[] { ',' });

            DataTable dtTemp = new DataTable();
            DataTable dtInputDataTable = new DataTable();
            DataTable dt_Return = new DataTable();

            loadingIndicator.Visibility = Visibility.Visible;
            DoEvents();

            foreach (string strLineTemp in saLine)
            {
                dtTemp = SetboxStockMulti(dtNow, strLineTemp);
                if (dtTemp.Rows.Count > 0)
                {
                    if (dtInputDataTable.Columns.Count == 0)
                    {
                        dtInputDataTable = dtTemp.Clone();
                    }

                    dtTemp.AsEnumerable().CopyToDataTable(dtInputDataTable, LoadOption.Upsert);
                }
            }

            string[] grCode = new string[2] { "PACK_PRDT_GR_CODE", "PRODTYPE" };

            string[] colName = dtInputDataTable.Columns.Cast<DataColumn>()
                                                                .Where(x => !(x.ColumnName.Contains("PACK_PRDT_GR_CODE")) && !(x.ColumnName.Contains("PRODTYPE")))
                                                                .Select(x => x.ColumnName)
                                                                .ToArray();

            dt_Return = Util.GetGroupBySum(dtInputDataTable, colName, grCode,  true);

            if (dtInputDataTable.Rows.Count > 0)
            {
                Util.GridSetData(dgBoxingList, dt_Return, FrameOperation, false);
                Util.SetTextBlockText_DataGridRowCount(tbBoxListCount, Util.NVC(dt_Return.Rows.Count));
            }
            else
            {
                Util.MessageInfo("SFU1498");
            }

            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void dgBoxingList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.GetCellFromPoint(pnt);

                if (crrCell != null)
                {

                    if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                    {

                        string sLineId = sBoxSetEqsgId;
                        string sCrrPjtGrp = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PACK_PRDT_GR_CODE"));
                        string sPrdType = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "PRODTYPE"));

                        if( 10 <= Convert.ToInt32(crrCell.Column.Tag.ToString()) && Convert.ToInt32(crrCell.Column.Tag.ToString()) <= 21)
                        {
                            BOX_HOLDFLAG.Visibility = Visibility.Visible;
                            BOX_NOTE.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            BOX_HOLDFLAG.Visibility = Visibility.Collapsed;
                            BOX_NOTE.Visibility = Visibility.Collapsed;
                        }

                        SetBoxStockDetail(sLineId, sCrrPjtGrp, sPrdType, crrCell.Column.Tag.ToString(), dtNow);
                    }
                }
            }
            catch { }
        }
        #endregion

        #region 헤드명 변경
        /// <summary>
        /// 선택날짜별로 Header 명 변경
        /// </summary>
        /// <param name="gd">변경 Grid Header</param>
        /// <param name="cb">날짜Combo</param>
        /// <returns></returns>
        /// 
        private string SetHeaderReplace(C1.WPF.DataGrid.C1DataGrid gd, C1ComboBox cb)
        {
            string SetDay = cb.SelectedValue.ToString() + "D";

            for (int i = 0; i < gd.Columns.Count; i++)
            {
                List<string> sHeaderTxt = (List<string>)gd.Columns[i].Header;
                for (int j = 0; j < sHeaderTxt.Count; j++)
                {
                    if (sHeaderTxt[j].Contains(sCellSetDay))
                    {
                        sHeaderTxt[j] = sHeaderTxt[j].Replace(sCellSetDay, SetDay);
                    }
                    /*
                     *  2020-03-16 염규범 선임
                     *  조회시에, 날짜 Combo box에 따른 컬럼 변경 처리의 건
                     *  
                     */
                    else if(sHeaderTxt[j].Contains(sNoboxSetDay))
                    {
                        sHeaderTxt[j] = sHeaderTxt[j].Replace(sNoboxSetDay, SetDay);
                    }
                    else if (sHeaderTxt[j].Contains(sBoxSetDay))
                    {
                        sHeaderTxt[j] = sHeaderTxt[j].Replace(sBoxSetDay, SetDay);
                    }
                }

                gd.Columns[i].Header = sHeaderTxt;

            }

            gd.Refresh();

            return SetDay;
        }

        private string SetHeaderLineBreak(C1.WPF.DataGrid.C1DataGrid gd)
        {
            string strTmep = string.Empty;

            for (int i = 0; i < gd.Columns.Count; i++)
            {
                List<string> sHeaderTxt = (List<string>)gd.Columns[i].Header;
                string strTemp = string.Empty;

                for (int j = 0; j < sHeaderTxt.Count; j++)
                {
                    if (sHeaderTxt[j].Contains("\\t"))
                    {
                        sHeaderTxt[j] = sHeaderTxt[j].Replace("\\t", "\r\n");
                        strTemp = sHeaderTxt[j].Replace("\\t", string.Empty);
                        strTemp += "t";
                        strTemp += "\r\n";
                        //sHeaderTxt[j] = sHeaderTxt[j].Replace(sCellSetDay, SetDay);
                    }
                    string temp = strTemp.Replace("System.Data.DataRowView", string.Empty).ToString();
                    Clipboard.SetText(strTemp.Replace("System.Data.DataRowView", string.Empty));
                }


                //gd.Columns[i].Header = sHeaderTxt;
                

            }

            gd.Refresh();

            return null;
        }


        #endregion

        #region 콤보 박스 세팅

        private void SetComboDay(C1ComboBox cb)
        {
            cb.SelectedValuePath = "Key";
            cb.DisplayMemberPath = "Value";

            for (int i = 1; i <= 60; i++)
            {

                cb.Items.Add(new KeyValuePair<string, string>(i.ToString(), i.ToString()));
            }

            cb.SelectedIndex = 4;
        }

        private void SetCboEQSG(MultiSelectionBox cboMulti)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = strAreaId;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMulti.Uncheck(-1);
                    }
                    else
                    {
                        //cboMulti.isAllUsed = true;
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; ++i)
                        {
                            cboMulti.Uncheck(i - 1);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background , new System.Threading.ThreadStart(delegate { }));
        }


        #region 엑셀 다운 로드
        private void btnCellDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgCellList);
                //new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellDetailDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgCellDetailList);
                //new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellDetailList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUnBoxingDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgUnBoxingList);
                //new LGC.GMES.MES.Common.ExcelExporter().Export(dgUnBoxingList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUnBoxingDetailDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgUnBoxingDetailList);
                //new LGC.GMES.MES.Common.ExcelExporter().Export(dgUnBoxingDetailList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnBoxingDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgBoxingList);
                //new LGC.GMES.MES.Common.ExcelExporter().Export(dgBoxingList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnBoxingDetailDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgBoxingDetailList);
                //new LGC.GMES.MES.Common.ExcelExporter().Export(dgBoxingDetailList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region BizCall - DA_REPORT_CELL_STOCK_BYDAY
        private void SetCellStock(DateTime date)
        {
            try
            {
                tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("PJTGR", typeof(string));
                RQSTDT.Columns.Add("PJTNAME", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["PJTGR"] = txtCellPjtGruop.GetBindValue();// MES 2.0 오류 Patch
                dr["PJTNAME"] = txtCellPjt.Text.Trim();
                dr["PRODID"] = txtCellProdId.Text.Trim();

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_REPORT_CELL_STOCK_BYDAY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgCellList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtResult.Rows.Count));
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
        #endregion

        #region BizCall - DA_REPORT_NOBOX_STOCK_BYDAY
        private void SetNoboxStock(DateTime date, string strLine)
        {
            try
            {
                tbNoBoxListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("PJTGR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = strLine;
                //dr["EQSGID"] = sNoBoxSetEqsgId;
                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["PJTGR"] = txtUnBoxingPjtGruop.GetBindValue();// MES 2.0 오류 Patch

                RQSTDT.Rows.Add(dr);


                new ClientProxy().ExecuteService("DA_REPORT_NOBOX_STOCK_BYDAY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgUnBoxingList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbNoBoxListCount, Util.NVC(dtResult.Rows.Count));
                        //dtUnBoxingTemp = dtResult;
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_REPORT_NOBOX_STOCK_BYDAY", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        #region ( 대용량 )
        /// <summary>
        /// Line 수가 늘어나는 만큼 SQL 속도로 인해서, LINE 수만큼 해당 BIZ 다시 콜 
        /// 1분 넘어서 Time Out 대비책
        /// </summary>
        /// <param name="date"> 옵션 박스 날짜 </param>
        /// <param name="strLine"> Multi Selection Box Line </param>
        /// <returns></returns>
        private DataTable SetNoboxStockMulti(DateTime date, string strLine)
        {
            
            try
            {
                tbNoBoxListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("PJTGR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = strLine;
                //dr["EQSGID"] = sNoBoxSetEqsgId;
                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["PJTGR"] = txtUnBoxingPjtGruop.GetBindValue();// MES 2.0 오류 Patch

                RQSTDT.Rows.Add(dr);
                DataTable dtTemp = new DataTable();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_REPORT_NOBOX_STOCK_BYDAY", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;

            }
            catch (Exception ex)
            {
                return null;
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion

        #region BizCall - DA_REPORT_BOX_STOCK_BYDAY
        private void SetboxStock(DateTime date)
        {
            try
            {
                

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("PJTGR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = sBoxSetEqsgId;
                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["PJTGR"] = txtCellPjtGruop.GetBindValue();// MES 2.0 오류 Patch

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_REPORT_BOX_STOCK_BYDAY", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgBoxingList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbBoxListCount, Util.NVC(dtResult.Rows.Count));
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


        private DataTable SetboxStockMulti(DateTime date, string strLine)
        {
            try
            {
                tbBoxListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("PJTGR", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = strLine;
                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["PJTGR"] = txtBoxingPjtGruop.GetBindValue();// MES 2.0 오류 Patch

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                //DataTable dtTemp = new DataTable();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_REPORT_BOX_STOCK_BYDAY", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
                return null;
            }
        }
        #endregion

        #region BizCall - DA_REPORT_CELL_STOCK_DETL
        private void SetCellStockDetail(string sPjtGrp, string PjtName, string Prodid, string sRptType, DateTime date)
        {
            try
            {
                tbCellDetlListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgCellDetailList);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("RPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PRODID"] = Prodid;
                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["RPTTYPE"] = sRptType;


                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_REPORT_CELL_STOCK_DETL", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgCellDetailList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbCellDetlListCount, Util.NVC(dtResult.Rows.Count));
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
        #endregion

        #region BizCall - DA_REPORT_NOBOX_STOCK_DETL
        private void SetNoBoxStockDetail(string sPjtGrp, string sPrdType, string sRptType, DateTime date)
        {
            try
            {
                tbNoBoxDetlListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgUnBoxingDetailList);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PACK_PRDT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODTYPE", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("RPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dr["EQSGID"] = sNoBoxSetEqsgId;
                dr["PACK_PRDT_GR_CODE"] = sPjtGrp;
                dr["PRODTYPE"] = sPrdType;

                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["RPTTYPE"] = sRptType;


                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_REPORT_NOBOX_STOCK_DETL", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgUnBoxingDetailList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbNoBoxDetlListCount, Util.NVC(dtResult.Rows.Count));
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
        #endregion

        #region BizCall - DA_REPORT_BOX_STOCK_DETL
        private void SetBoxStockDetail(string sLineId, string sPjtGrp, string sPrdType, string sRptType, DateTime date)
        {
            try
            {
                tbBoxDetlListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgBoxingDetailList);

                string strBizName = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PACK_PRDT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODTYPE", typeof(string));
                RQSTDT.Columns.Add("BYDAY", typeof(int));
                RQSTDT.Columns.Add("DATE", typeof(DateTime));
                RQSTDT.Columns.Add("RPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dr["EQSGID"] = sLineId;
                dr["PACK_PRDT_GR_CODE"] = sPjtGrp;
                dr["PRODTYPE"] = sPrdType;

                dr["BYDAY"] = iByDay;
                dr["DATE"] = date;
                dr["RPTTYPE"] = sRptType;


                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                if(Convert.ToInt32(sRptType) > 10)
                {
                    strBizName = "DA_REPORT_BOX_STOCK_DETL2";
                }
                else
                {
                    strBizName = "DA_REPORT_BOX_STOCK_DETL";
                }

                new ClientProxy().ExecuteService(strBizName, "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgBoxingDetailList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbBoxDetlListCount, Util.NVC(dtResult.Rows.Count));
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

        #endregion

    }

   
}

