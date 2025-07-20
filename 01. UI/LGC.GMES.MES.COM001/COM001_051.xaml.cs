/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.21  김대현      라벨 출력시 동별 공통코드 PRD_MIX_VLD_DATE_MNGT 등록되어 있으면 출력일자에 ATTR1 값을 더한 날짜로 출력






**************************************************************************************/

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

using C1.WPF;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_051 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        public DataTable dtPackingCard;
        public Boolean bCheck = false;

        public COM001_051()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            setvalue();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnTank);
            listAuth.Add(btnAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            string[] sFilter = { "E0700", Wip_State.WAIT };
            combo.SetCombo(cboMixProd, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
        }

        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            dtpDateFrom2.SelectedDateTime = DateTime.Now;
            dtpDateTo2.SelectedDateTime = DateTime.Now;

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void setvalue()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "TANK_SET_QTY";

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_VALUE", "RQSTDT", "RSLTDT", RQSTDT);

                txtQTY.Text = SearchResult.Rows[0]["QTY"].ToString();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region 선분산 믹서 포장
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                    string sLot_id = txtLotID.Text.Trim();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("PROCID", typeof(String));
                    RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                    RQSTDT.Columns.Add("WIP_TYPE_CODE", typeof(String));
                    RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                    RQSTDT.Columns.Add("TO_DATE", typeof(String));
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("PRODID", typeof(String));
                    RQSTDT.Columns.Add("LOTID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["PROCID"] = "E0700";
                    dr["WIPSTAT"] = Wip_State.WAIT;
                    dr["WIP_TYPE_CODE"] = "IN";
                    dr["FROM_DATE"] = sLot_id != "" ? null : sStart_date;
                    dr["TO_DATE"] = sLot_id != "" ? null : sEnd_date;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PRODID"] = cboMixProd.SelectedValue.ToString() == "" ? null : cboMixProd.SelectedValue.ToString();
                    dr["LOTID"] = sLot_id == "" ? null : sLot_id;

                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count == 0)
                    {
                        Util.Alert("SFU1870");      //재공 정보가 없습니다.
                        return;
                    }
                    else if (SearchResult.Rows.Count > 1)
                    {
                        Util.AlertInfo("SFU2887", new object[] { "2" });   //{0}건 이상이 조회되었습니다.
                        return;
                    }

                    if (dgList1.GetRowCount() >= 1)
                    {
                        for (int i = 0; i < dgList1.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgList1.Rows[i].DataItem, "LOTID").ToString() == sLot_id)
                            {
                                Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                                return;
                            }

                            if (DataTableConverter.GetValue(dgList1.Rows[i].DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                            {
                                Util.Alert("SFU1502");   //동일 제품이 아닙니다. //이전에 스캔한 LOT의 제품코드와 다릅니다.
                                return;
                            }
                        }
                    }

                    if (dgList1.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgList1, SearchResult, FrameOperation);
                    }
                    else
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgList1.ItemsSource);
                        dtSource.Merge(SearchResult);

                        Util.gridClear(dgList1);
                        dgList1.ItemsSource = DataTableConverter.Convert(dtSource);

                    }

                    txtLotID.SelectAll();

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_Info();
        }

        private void Search_Info()
        {
            try
            {
                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                string sLot_id = txtLotID.Text.Trim();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                RQSTDT.Columns.Add("WIP_TYPE_CODE_IN", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = "E0700";
                dr["WIPSTAT"] = Wip_State.WAIT;
                dr["WIP_TYPE_CODE_IN"] = "IN";
                dr["FROM_DATE"] = sLot_id != "" ? null : sStart_date;
                dr["TO_DATE"] = sLot_id != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = cboMixProd.SelectedValue.ToString() == "" ? null : cboMixProd.SelectedValue.ToString();
                dr["LOTID"] = sLot_id == "" ? null : sLot_id;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");      //재공 정보가 없습니다.
                    return;
                }

                Util.gridClear(dgList1);
                Util.gridClear(dgList2);

                Util.GridSetData(dgList1, SearchResult, FrameOperation);

                txtLotID.SelectAll();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            bCheck = true;

            if (DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "CHK").ToString() == "1")
            {
                if (dgList2.GetRowCount() == 0)
                {

                    DataTable dtAdd = new DataTable();
                    dtAdd.Columns.Add("LOTID", typeof(string));
                    dtAdd.Columns.Add("PRODID", typeof(string));
                    dtAdd.Columns.Add("WIPQTY", typeof(string));
                    dtAdd.Columns.Add("INPUTQTY", typeof(string));

                    dgList2.ItemsSource = DataTableConverter.Convert(dtAdd);

                    dgList2.IsReadOnly = false;
                    dgList2.BeginNewRow();
                    dgList2.EndNewRow(true);
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "LOTID"));
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "PRODID"));
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "WIPQTY", DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "WIPQTY"));
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "INPUTQTY", 0);
                    //dgList2.IsReadOnly = true;                   

                    dgList2.Columns["INPUTQTY"].IsReadOnly = false;

                    DataGrid_Summary();
                }
                else
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgList2.Rows[0].DataItem, "PRODID")) !=
                        Util.NVC(DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "PRODID")))
                    {
                        Util.Alert("SFU1893");  //제품ID가 같지 않습니다.
                        return;
                    }

                    dgList2.IsReadOnly = false;
                    dgList2.BeginNewRow();
                    dgList2.EndNewRow(true);
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "LOTID"));
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "PRODID"));
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "WIPQTY", DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "WIPQTY"));
                    DataTableConverter.SetValue(dgList2.CurrentRow.DataItem, "INPUTQTY", 0);
                    //dgList2.IsReadOnly = true;

                    dgList2.Columns["INPUTQTY"].IsReadOnly = false;

                    DataGrid_Summary();
                }
            }
            else
            {
                for (int i = dgList2.GetRowCount(); i > 0; i--)
                {
                    int k = 0;
                    k = i - 1;
                    if (DataTableConverter.GetValue(dgList2.Rows[k].DataItem, "LOTID").ToString() == 
                        DataTableConverter.GetValue(dgList1.Rows[checkIndex].DataItem, "LOTID").ToString())
                    {
                        dgList2.IsReadOnly = false;
                        dgList2.RemoveRow(k);
                        //dgList2.IsReadOnly = true;

                        dgList2.Columns["INPUTQTY"].IsReadOnly = false;

                        DataGrid_Summary();
                    }
                }
            }

            bCheck = false;
        }
        
        private void DataGrid_Summary()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgList2.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgList2.Columns["INPUTQTY"], dac);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Initialize_Out();
        }

        private void Initialize_Out()
        {
            Util.gridClear(dgList1);
            Util.gridClear(dgList2);

            txtLotID.Text = "";
            txtLotID.Focus();
        }

        private void dgPalletList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgPalletList.SelectedIndex = idx;

                Search_Tank_List(DataTableConverter.GetValue(dgPalletList.Rows[idx].DataItem, "LOTID").ToString(), dgLotList);
            }
        }

        private void Search_Tank_List(String sLot_ID, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("DEL_FLAG", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLot_ID;
                dr["DEL_FLAG"] = "N";

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TANK_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLotList, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Search_Tank();
        }

        private void Search_Tank()
        {
            try
            {
                Util.gridClear(dgPalletList);
                Util.gridClear(dgLotList);

                string sStart_date = dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd");

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                RQSTDT.Columns.Add("WIP_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = "E0700";
                dr["WIPSTAT"] = Wip_State.WAIT;
                dr["WIP_TYPE_CODE"] = "OUT";
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgPalletList, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private int GetPrdMixVldDateMngtNum()
        {
            int iNum = 0;

            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));
            inTable.Columns.Add("USE_FLAG", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "PRD_MIX_VLD_DATE_MNGT";
            dr["COM_CODE"] = "PRD_MIX_VLD_DATE_MNGT";
            dr["USE_FLAG"] = "Y";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (!string.IsNullOrEmpty(Util.NVC(dtResult.Rows[0]["ATTR1"])))
                {
                    iNum = Convert.ToInt32(Util.NVC(dtResult.Rows[0]["ATTR1"]));
                }
                else
                {
                    iNum = 0;
                }
            }

            return iNum;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sTank_ID = string.Empty;
                string sModel = string.Empty;
                DateTime sDate;

                if (dgPalletList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (dgLotList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPalletList, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }
                else
                {
                    sTank_ID = drChk[0]["LOTID"].ToString();
                    //sDate = drChk[0]["WIPSDTTM"].ToString();
                    sDate = Convert.ToDateTime(drChk[0]["WIPSDTTM"].ToString());
                }

                string sPrintDate = sDate.Year.ToString() + "-" + sDate.Month.ToString("00") + "-" + sDate.Day.ToString("00");

                // 유효일자 추가 [2017-08-24]
                string sValidDate = sDate.AddDays(7 * 27).ToString("yyyy-MM-dd");
                // 2023-02-21 김대현
                int iVldDateMngtNum = this.GetPrdMixVldDateMngtNum();   //유효일자 추가
                if(iVldDateMngtNum > 0)
                {
                    sValidDate = sDate.AddDays(iVldDateMngtNum).ToString("yyyy-MM-dd");
                }

                //string sPrintDate = sDate.ToString().Substring(0, 4) + "-" + sDate.ToString().Substring(4, 2) + "-" + sDate.ToString().Substring(6, 2);

                dtPackingCard = new DataTable();
                dtPackingCard.TableName = "dtPackingCard";
                dtPackingCard.Columns.Add("TITLE", typeof(string));
                dtPackingCard.Columns.Add("BARCODE", typeof(string));
                dtPackingCard.Columns.Add("TANK", typeof(string));

                dtPackingCard.Columns.Add("T_DATE", typeof(string));
                dtPackingCard.Columns.Add("T_VOL", typeof(string));
                dtPackingCard.Columns.Add("T_MTRL", typeof(string));
                dtPackingCard.Columns.Add("T_MAKE", typeof(string));
                dtPackingCard.Columns.Add("T_LOT", typeof(string));
                dtPackingCard.Columns.Add("T_MODEL", typeof(string));
                dtPackingCard.Columns.Add("T_PER", typeof(string));

                dtPackingCard.Columns.Add("DATE", typeof(string));
                dtPackingCard.Columns.Add("VALID_DATE", typeof(string));
                dtPackingCard.Columns.Add("DATA01", typeof(string));
                dtPackingCard.Columns.Add("DATA02", typeof(string));

                dtPackingCard.Columns.Add("VOL01", typeof(string));
                dtPackingCard.Columns.Add("VOL02", typeof(string));

                dtPackingCard.Columns.Add("MTRL01", typeof(string));
                dtPackingCard.Columns.Add("MTRL02", typeof(string));
                dtPackingCard.Columns.Add("MTRL03", typeof(string));
                dtPackingCard.Columns.Add("MTRL04", typeof(string));

                dtPackingCard.Columns.Add("MAKE01", typeof(string));
                dtPackingCard.Columns.Add("MAKE02", typeof(string));
                dtPackingCard.Columns.Add("MAKE03", typeof(string));
                dtPackingCard.Columns.Add("MAKE04", typeof(string));

                dtPackingCard.Columns.Add("LOT01", typeof(string));
                dtPackingCard.Columns.Add("LOT02", typeof(string));
                dtPackingCard.Columns.Add("LOT03", typeof(string));
                dtPackingCard.Columns.Add("LOT04", typeof(string));
                dtPackingCard.Columns.Add("LOT05", typeof(string));
                dtPackingCard.Columns.Add("LOT06", typeof(string));
                dtPackingCard.Columns.Add("LOT07", typeof(string));
                dtPackingCard.Columns.Add("LOT08", typeof(string));

                dtPackingCard.Columns.Add("MODEL", typeof(string));
                dtPackingCard.Columns.Add("PERCENT", typeof(string));

                //발행하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2873"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        for (int icnt = 0; icnt < dgLotList.GetRowCount(); icnt++)
                        {
                            //
                            DataTable RQSTDT2 = new DataTable();
                            RQSTDT2.TableName = "RQSTDT";
                            RQSTDT2.Columns.Add("PRODID", typeof(String));

                            DataRow dr2 = RQSTDT2.NewRow();
                            dr2["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));
                            
                            RQSTDT2.Rows.Add(dr2);

                            DataTable SearchModel = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRLID_BY_MODEL", "RQSTDT", "RSLTDT", RQSTDT2);

                            if (SearchModel.Rows[0]["MODEL"].ToString() == "Null")
                            {
                                sModel = null;
                            }
                            else
                            {
                                sModel = SearchModel.Rows[0]["MODEL"].ToString();
                            }

                            //
                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("PRODID", typeof(String));
                            RQSTDT1.Columns.Add("SHOPID", typeof(String));
                            RQSTDT1.Columns.Add("LOTID", typeof(String));

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));
                            dr1["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            dr1["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOTID"));

                            RQSTDT1.Rows.Add(dr1);

                            DataTable SearchCnt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_BATCH_CNT", "RQSTDT", "RSLTDT", RQSTDT1);

                            if (SearchCnt.Rows.Count == 0)
                            {
                                Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
                                return;
                            }

                            //
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("PRODID", typeof(String));
                            RQSTDT.Columns.Add("SHOPID", typeof(String));
                            RQSTDT.Columns.Add("LOTID", typeof(String));

                            DataRow dr = RQSTDT.NewRow();
                            dr["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));
                            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOTID"));

                            RQSTDT.Rows.Add(dr);

                            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_BATCH_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                            if (SearchResult.Rows.Count == 0)
                            {
                                Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
                                return;
                            }

                            //decimal dSum = 0;
                            //decimal dTotal = 0;

                            //for (int i = 0; i < dgLotList.GetRowCount(); i++)
                            //{
                            //    dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOT_QTY")));

                            //    dTotal = dTotal + dSum;
                            //}

                            //decimal dSum2 = 0;
                            //decimal dTotal2 = 0;

                            //for (int i = 0; i < SearchResult.Rows.Count; i++)
                            //{
                            //    dSum2 = Convert.ToDecimal(SearchResult.Rows[0]["CALC"].ToString());

                            //    dTotal2 = dTotal2 + dSum2;
                            //}

                            //Print(sTank_ID, sPrintDate, SearchResult, SearchCnt.Rows.Count, icnt, dTotal, sModel, dTotal2);
                            Print(sTank_ID, sPrintDate, sValidDate, SearchResult, SearchCnt.Rows.Count, icnt, sModel);

                        }

                        LGC.GMES.MES.COM001.Report_Tank rs = new LGC.GMES.MES.COM001.Report_Tank();
                        rs.FrameOperation = this.FrameOperation;

                        if (rs != null)
                        {
                            // 태그 발행 창 화면에 띄움.
                            object[] Parameters = new object[2];
                            Parameters[0] = "Report_Mixing";
                            Parameters[1] = dtPackingCard;

                            C1WindowExtension.SetParameters(rs, Parameters);

                            rs.Closed += new EventHandler(Print_Result);
                            this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                        }

                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_Tank wndPopup = sender as LGC.GMES.MES.COM001.Report_Tank;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {

                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                    Util.gridClear(dgPalletList);
                    Util.gridClear(dgLotList);
                    Search_Tank();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Print(string sTank_ID, string sPrintDate, string sValidDate, DataTable DTResult, int iRowcnt, int icnt, string sModel)
        {
            try
            {
                if (iRowcnt == 1)
                {

                    DataRow drCrad = dtPackingCard.NewRow();
                    drCrad["TITLE"] = ObjectDic.Instance.GetObjectName("도전재선분산액");
                    drCrad["BARCODE"] = sTank_ID;
                    drCrad["TANK"] = sTank_ID;

                    drCrad["T_DATE"] = ObjectDic.Instance.GetObjectName("제조일/유효일");
                    drCrad["T_VOL"] = ObjectDic.Instance.GetObjectName("용량");
                    drCrad["T_MTRL"] = ObjectDic.Instance.GetObjectName("원재료");
                    drCrad["T_MAKE"] = ObjectDic.Instance.GetObjectName("조성");
                    drCrad["T_LOT"] = ObjectDic.Instance.GetObjectName("LOT");
                    drCrad["T_MODEL"] = ObjectDic.Instance.GetObjectName("모델");
                    drCrad["T_PER"] = ObjectDic.Instance.GetObjectName("고형분");

                    drCrad["DATE"] = sPrintDate;
                    drCrad["VALID_DATE"] = sValidDate;
                    drCrad["DATA01"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOTID"));
                    drCrad["DATA02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));

                    //drCrad["VOL01"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOT_QTY")) + " Kg";
                    drCrad["VOL01"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOT_QTY")), 2))) + " Kg";
                    drCrad["VOL02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "MILL_COUNT")) + " pass";

                    drCrad["MTRL01"] = DTResult.Rows[0]["S21"].ToString();
                    drCrad["MTRL02"] = "";
                    drCrad["MTRL03"] = "";
                    drCrad["MTRL04"] = "";

                    drCrad["MAKE01"] = DTResult.Rows[0]["CALC"].ToString();
                    drCrad["MAKE02"] = "";
                    drCrad["MAKE03"] = "";
                    drCrad["MAKE04"] = "";

                    if (DTResult.Rows.Count == 1 )
                    {
                        drCrad["LOT01"] = DTResult.Rows[0]["INPUT_LOTID"].ToString();
                        drCrad["LOT02"] = "";
                    }
                    else
                    {
                        drCrad["LOT01"] = DTResult.Rows[0]["INPUT_LOTID"].ToString();
                        drCrad["LOT02"] = DTResult.Rows[1]["INPUT_LOTID"].ToString();
                    }
                    
                    drCrad["LOT03"] = "";
                    drCrad["LOT04"] = "";
                    drCrad["LOT05"] = "";
                    drCrad["LOT06"] = "";
                    drCrad["LOT07"] = "";
                    drCrad["LOT08"] = "";

                    drCrad["MODEL"] = sModel;
                    drCrad["PERCENT"] = DTResult.Rows[0]["PRODDESC"].ToString();             

                    dtPackingCard.Rows.Add(drCrad);

                }
                else if (iRowcnt == 2)
                {
                    DataRow drCrad = dtPackingCard.NewRow();
                    drCrad["TITLE"] = ObjectDic.Instance.GetObjectName("도전재선분산액");
                    drCrad["BARCODE"] = sTank_ID;
                    drCrad["TANK"] = sTank_ID;

                    drCrad["T_DATE"] = ObjectDic.Instance.GetObjectName("제조일/유효일");
                    drCrad["T_VOL"] = ObjectDic.Instance.GetObjectName("용량");
                    drCrad["T_MTRL"] = ObjectDic.Instance.GetObjectName("원재료");
                    drCrad["T_MAKE"] = ObjectDic.Instance.GetObjectName("조성");
                    drCrad["T_LOT"] = ObjectDic.Instance.GetObjectName("LOT");
                    drCrad["T_MODEL"] = ObjectDic.Instance.GetObjectName("모델");
                    drCrad["T_PER"] = ObjectDic.Instance.GetObjectName("고형분");

                    drCrad["DATE"] = sPrintDate;
                    drCrad["VALID_DATE"] = sValidDate;
                    drCrad["DATA01"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOTID"));
                    drCrad["DATA02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));

                    drCrad["VOL01"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOT_QTY")), 2))) + " Kg";
                    drCrad["VOL02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "MILL_COUNT")) + " pass";

                    drCrad["MTRL01"] = DTResult.Rows[0]["S21"].ToString();
                    drCrad["MTRL02"] = DTResult.Rows[1]["S21"].ToString();
                    drCrad["MTRL03"] = "";
                    drCrad["MTRL04"] = "";

                    drCrad["MAKE01"] = DTResult.Rows[0]["CALC"].ToString();
                    drCrad["MAKE02"] = DTResult.Rows[1]["CALC"].ToString();
                    drCrad["MAKE03"] = "";
                    drCrad["MAKE04"] = "";


                    for (int i = 0; i < DTResult.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            drCrad["LOT01"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                        }
                        else if (i == 1)
                        { 
                            if (DTResult.Rows[i - 1]["S21"].ToString() == DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT02"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                            else if (DTResult.Rows[i - 1]["S21"].ToString() != DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT02"] = "";
                                drCrad["LOT03"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();                                
                            }
                        }
                        else if (i == 2)
                        {
                            drCrad["LOT04"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                        }
                    }
                    
                    drCrad["LOT05"] = "";
                    drCrad["LOT06"] = "";
                    drCrad["LOT07"] = "";
                    drCrad["LOT08"] = "";

                    drCrad["MODEL"] = sModel;
                    drCrad["PERCENT"] = DTResult.Rows[0]["PRODDESC"].ToString();

                    dtPackingCard.Rows.Add(drCrad);
                }
                else if (iRowcnt == 3)
                {
                    DataRow drCrad = dtPackingCard.NewRow();
                    drCrad["TITLE"] = ObjectDic.Instance.GetObjectName("도전재선분산액");
                    drCrad["BARCODE"] = sTank_ID;
                    drCrad["TANK"] = sTank_ID;

                    drCrad["T_DATE"] = ObjectDic.Instance.GetObjectName("제조일/유효일");
                    drCrad["T_VOL"] = ObjectDic.Instance.GetObjectName("용량");
                    drCrad["T_MTRL"] = ObjectDic.Instance.GetObjectName("원재료");
                    drCrad["T_MAKE"] = ObjectDic.Instance.GetObjectName("조성");
                    drCrad["T_LOT"] = ObjectDic.Instance.GetObjectName("LOT");
                    drCrad["T_MODEL"] = ObjectDic.Instance.GetObjectName("모델");
                    drCrad["T_PER"] = ObjectDic.Instance.GetObjectName("고형분");

                    drCrad["DATE"] = sPrintDate;
                    drCrad["VALID_DATE"] = sValidDate;
                    drCrad["DATA01"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOTID"));
                    drCrad["DATA02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));

                    drCrad["VOL01"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOT_QTY")), 2))) + " Kg";
                    drCrad["VOL02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "MILL_COUNT")) + " pass";

                    drCrad["MTRL01"] = DTResult.Rows[0]["S21"].ToString();
                    drCrad["MTRL02"] = DTResult.Rows[1]["S21"].ToString();
                    drCrad["MTRL03"] = DTResult.Rows[2]["S21"].ToString();
                    drCrad["MTRL04"] = "";

                    drCrad["MAKE01"] = DTResult.Rows[0]["CALC"].ToString();
                    drCrad["MAKE02"] = DTResult.Rows[1]["CALC"].ToString();
                    drCrad["MAKE03"] = DTResult.Rows[2]["CALC"].ToString();
                    drCrad["MAKE04"] = "";


                    for (int i = 0; i < DTResult.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            drCrad["LOT01"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                        }
                        else if (i == 1)
                        {
                            if (DTResult.Rows[i - 1]["S21"].ToString() == DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT02"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                            else if (DTResult.Rows[i - 1]["S21"].ToString() != DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT02"] = "";
                                drCrad["LOT03"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                        }
                        else if (i == 2)
                        {
                            if (DTResult.Rows[i - 1]["S21"].ToString() == DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT04"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                            else if (DTResult.Rows[i - 1]["S21"].ToString() != DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT04"] = "";
                                drCrad["LOT05"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                        }
                        else if (i == 3)
                        {
                            drCrad["LOT06"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                        }
                    }

                    drCrad["LOT07"] = "";
                    drCrad["LOT08"] = "";

                    drCrad["MODEL"] = sModel;
                    drCrad["PERCENT"] = DTResult.Rows[0]["PRODDESC"].ToString();

                    dtPackingCard.Rows.Add(drCrad);
                }
                else if (iRowcnt == 4)
                {
                    DataRow drCrad = dtPackingCard.NewRow();
                    drCrad["TITLE"] = ObjectDic.Instance.GetObjectName("도전재선분산액");
                    drCrad["BARCODE"] = sTank_ID;
                    drCrad["TANK"] = sTank_ID;

                    drCrad["T_DATE"] = ObjectDic.Instance.GetObjectName("제조일/유효일");
                    drCrad["T_VOL"] = ObjectDic.Instance.GetObjectName("용량");
                    drCrad["T_MTRL"] = ObjectDic.Instance.GetObjectName("원재료");
                    drCrad["T_MAKE"] = ObjectDic.Instance.GetObjectName("조성");
                    drCrad["T_LOT"] = ObjectDic.Instance.GetObjectName("LOT");
                    drCrad["T_MODEL"] = ObjectDic.Instance.GetObjectName("모델");
                    drCrad["T_PER"] = ObjectDic.Instance.GetObjectName("고형분");

                    drCrad["DATE"] = sPrintDate;
                    drCrad["VALID_DATE"] = sValidDate;
                    drCrad["DATA01"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOTID"));
                    drCrad["DATA02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "PRODID"));

                    drCrad["VOL01"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "LOT_QTY")), 2))) + " Kg";
                    drCrad["VOL02"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[icnt].DataItem, "MILL_COUNT")) + " pass";

                    drCrad["MTRL01"] = DTResult.Rows[0]["S21"].ToString();
                    drCrad["MTRL02"] = DTResult.Rows[1]["S21"].ToString();
                    drCrad["MTRL03"] = DTResult.Rows[2]["S21"].ToString();
                    drCrad["MTRL04"] = DTResult.Rows[3]["S21"].ToString();

                    drCrad["MAKE01"] = DTResult.Rows[0]["CALC"].ToString();
                    drCrad["MAKE02"] = DTResult.Rows[1]["CALC"].ToString();
                    drCrad["MAKE03"] = DTResult.Rows[2]["CALC"].ToString();
                    drCrad["MAKE04"] = DTResult.Rows[3]["CALC"].ToString();


                    for (int i = 0; i < DTResult.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            drCrad["LOT01"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                        }
                        else if (i == 1)
                        {
                            if (DTResult.Rows[i - 1]["S21"].ToString() == DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT02"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                            else if (DTResult.Rows[i - 1]["S21"].ToString() != DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT02"] = "";
                                drCrad["LOT03"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                        }
                        else if (i == 2)
                        {
                            if (DTResult.Rows[i - 1]["S21"].ToString() == DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT04"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                            else if (DTResult.Rows[i - 1]["S21"].ToString() != DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT04"] = "";
                                drCrad["LOT05"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                        }
                        else if (i == 3)
                        {
                            if (DTResult.Rows[i - 1]["S21"].ToString() == DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT06"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                            else if (DTResult.Rows[i - 1]["S21"].ToString() != DTResult.Rows[i]["S21"].ToString())
                            {
                                drCrad["LOT07"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                            }
                        }
                        else if (i == 4)
                        {
                            drCrad["LOT08"] = DTResult.Rows[i]["INPUT_LOTID"].ToString();
                        }
                    }

                    drCrad["MODEL"] = sModel;
                    drCrad["PERCENT"] = DTResult.Rows[0]["PRODDESC"].ToString();

                    dtPackingCard.Rows.Add(drCrad);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnTank_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgList2.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //TANK 구성 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2846"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        decimal dSum = 0;
                        decimal dTotal = 0;

                        for (int i = 0; i < dgList2.GetRowCount(); i++)
                        {
                            if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList2.Rows[i].DataItem, "INPUTQTY"))) == 0)
                            {
                                Util.Alert("SFU2973");   //투입수량 정보가 없습니다.
                                return;
                            }
                            else
                            { 
                                dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList2.Rows[i].DataItem, "INPUTQTY")));

                                dTotal = dTotal + dSum;
                            }
                        }

                        if ( dTotal > Convert.ToDecimal(txtQTY.Text))
                        {
                            Util.Alert("SFU3024");   //기준정보 수량을 초과하였습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("IN_LOT");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("PRODID", typeof(string));
                        inData.Columns.Add("OUT_LOTID", typeof(string));
                        inData.Columns.Add("OUTPUT_QTY", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["IFMODE"] = "OFF";
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["PROCID"] = "E0700";
                        row["PRODID"] = DataTableConverter.GetValue(dgList2.Rows[0].DataItem, "PRODID").ToString();
                        row["OUT_LOTID"] = "";
                        row["OUTPUT_QTY"] = dTotal;
                        row["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["IN_LOT"].Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("IN_INPUT");
                        inLot.Columns.Add("INPUT_LOTID", typeof(string));
                        inLot.Columns.Add("INPUT_QTY", typeof(string));

                        for (int i = 0; i < dgList2.GetRowCount(); i++)
                        {

                            DataRow row2 = inLot.NewRow();
                            row2["INPUT_LOTID"] = DataTableConverter.GetValue(dgList2.Rows[i].DataItem, "LOTID").ToString();
                            row2["INPUT_QTY"] = DataTableConverter.GetValue(dgList2.Rows[i].DataItem, "INPUTQTY").ToString();

                            indataSet.Tables["IN_INPUT"].Rows.Add(row2);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_LOT_TANK_PM", "IN_LOT,IN_INPUT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_CREATE_LOT_TANK_PM", searchException.Message, searchException.ToString());
                                    return;
                                }

                                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                                Util.gridClear(dgList1);
                                Util.gridClear(dgList2);
                                Search_Info();

                                Util.gridClear(dgPalletList);
                                Util.gridClear(dgLotList);
                                Search_Tank();
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPalletList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPalletList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                string sTank_ID = drChk[0]["LOTID"].ToString();
                string sModel = drChk[0]["PRODID"].ToString();

                COM001_051_ADD wndConfirm = new COM001_051_ADD();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = sModel;
                    Parameters[1] = sTank_ID;
                    Parameters[2] = txtQTY.Text;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            try
            {
                string sTank_ID = string.Empty;

                COM001_051_ADD window = sender as COM001_051_ADD;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    DataTable dtSend = window.dtAdd;

                    if (dgPalletList.GetRowCount() == 0)
                    {
                        Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                        return;
                    }

                    //TANK 구성 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2846"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {

                            DataRow[] drChk = Util.gridGetChecked(ref dgPalletList, "CHK");
                            if (drChk.Length <= 0)
                            {
                                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                                return;
                            }

                            sTank_ID = drChk[0]["LOTID"].ToString();

                            decimal dSum = 0;
                            decimal dSum2 = 0;
                            decimal dTotal = 0;
                            decimal dTotal2 = 0;

                            for (int i = 0; i < dgLotList.GetRowCount(); i++)
                            {
                                dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOT_QTY")));

                                dTotal = dTotal + dSum;
                            }

                            for (int i = 0; i < dtSend.Rows.Count; i++)
                            {
                                dSum2 = Convert.ToDecimal(dtSend.Rows[i]["INPUTQTY"]);

                                dTotal2 = dTotal2 + dSum2;
                            }

                            if (dTotal + dTotal2 > Convert.ToDecimal(txtQTY.Text))
                            {
                                Util.Alert("SFU3024");   //기준정보 수량을 초과하였습니다.
                                return;
                            }

                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("IN_LOT");
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("IFMODE", typeof(string));
                            inData.Columns.Add("AREAID", typeof(string));
                            inData.Columns.Add("PROCID", typeof(string));
                            inData.Columns.Add("PRODID", typeof(string));
                            inData.Columns.Add("OUT_LOTID", typeof(string));
                            inData.Columns.Add("OUTPUT_QTY", typeof(decimal));
                            inData.Columns.Add("USERID", typeof(string));

                            DataRow row = inData.NewRow();
                            row["SRCTYPE"] = "UI";
                            row["IFMODE"] = "OFF";
                            row["AREAID"] = LoginInfo.CFG_AREA_ID;
                            row["PROCID"] = "E0700";
                            row["PRODID"] = DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PRODID").ToString();
                            row["OUT_LOTID"] = sTank_ID;
                            row["OUTPUT_QTY"] = dTotal + dTotal2;
                            row["USERID"] = LoginInfo.USERID;

                            indataSet.Tables["IN_LOT"].Rows.Add(row);


                            DataTable inLot = indataSet.Tables.Add("IN_INPUT");
                            inLot.Columns.Add("INPUT_LOTID", typeof(string));
                            inLot.Columns.Add("INPUT_QTY", typeof(string));

                            for (int i = 0; i < dtSend.Rows.Count; i++)
                            {

                                DataRow row2 = inLot.NewRow();
                                row2["INPUT_LOTID"] = dtSend.Rows[i]["LOTID"];
                                row2["INPUT_QTY"] = dtSend.Rows[i]["INPUTQTY"];

                                indataSet.Tables["IN_INPUT"].Rows.Add(row2);
                            }

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_LOT_TANK_PM", "IN_LOT,IN_INPUT", null, (searchResult, searchException) =>
                            {
                                try
                                {
                                    if (searchException != null)
                                    {
                                        Util.AlertByBiz("BR_PRD_REG_CREATE_LOT_TANK_PM", searchException.Message, searchException.ToString());
                                        return;
                                    }

                                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                                    Search_Tank();
                                    Search_Check(sTank_ID);

                                }
                                catch (Exception ex)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                }
                            }, indataSet
                            );
                        }
                    });

                    Search_Check(sTank_ID);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Search_Check(string sTankID)
        {
            try
            {
                for (int i = dgPalletList.GetRowCount(); i > 0; i--)
                {
                    int k = 0;
                    k = i - 1;
                    if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[k].DataItem, "LOTID")) == sTankID)
                    {
                        dgPalletList.IsReadOnly = false;
                        DataTableConverter.SetValue(dgPalletList.Rows[k].DataItem, "CHK", true);
                        dgPalletList.IsReadOnly = true;

                        dgPalletList.SelectedIndex = k;

                        Search_Tank_List(DataTableConverter.GetValue(dgPalletList.Rows[k].DataItem, "LOTID").ToString(), dgLotList);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        #endregion      

        private void dgList2_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (bCheck == true)
                    return;

                int idx = e.Cell.Row.Index;

                decimal dWipqty = 0;
                decimal dInputqty = 0;
                decimal dInit = 0;
                
                if (idx >= 0)
                {
                    if (e.Cell.Column.Name.Equals("INPUTQTY") )
                    {
                        dWipqty = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList2.Rows[e.Cell.Row.Index].DataItem, "WIPQTY")));
                        dInputqty = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList2.Rows[e.Cell.Row.Index].DataItem, "INPUTQTY")));
                        dInit = Convert.ToDecimal(txtQTY.Text);


                        if (dWipqty < dInputqty)
                        {
                            Util.AlertInfo("SFU3025");  //투입수량이 재공수량을 초과하였습니다.
                            DataTableConverter.SetValue(dgList2.Rows[idx].DataItem, "INPUTQTY", 0);
                            return;
                        }
                        else if (dInit < dInputqty)
                        {
                            Util.AlertInfo("SFU3139");  //투입수량이 기준수량을 초과하였습니다.
                            DataTableConverter.SetValue(dgList2.Rows[idx].DataItem, "INPUTQTY", 0);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnPackCancel_Click(object sender, RoutedEventArgs e)
        {
            // 포장취소
            if (_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK") == -1)
            {
                Util.MessageValidation("선택된 Tank가 없습니다.");
                return;
            }

            //TANK 포장취소 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("TANK 포장취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    try
                    {

                        DataSet ds = new DataSet();
                        DataTable dt = ds.Tables.Add("IN_LOT");
                        dt.Columns.Add("SRCTYPE", typeof(string));
                        dt.Columns.Add("IFMODE", typeof(string));
                        dt.Columns.Add("AREAID", typeof(string));
                        dt.Columns.Add("PROCID", typeof(string));
                        dt.Columns.Add("PRODID", typeof(string));
                        dt.Columns.Add("OUT_LOTID", typeof(string));
                        dt.Columns.Add("OUTPUT_QTY", typeof(Decimal));
                        dt.Columns.Add("USERID", typeof(string));

                        DataRow row = null;

                        for (int i = 0; i < dgPalletList.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                row = dt.NewRow();
                                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                row["IFMODE"] = IFMODE.IFMODE_OFF;
                                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                                row["PROCID"] = Process.PRE_MIXING_PACK;
                                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "PRODID"));
                                row["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "LOTID"));
                                row["OUTPUT_QTY"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "WIPQTY")));
                                row["USERID"] = LoginInfo.USERID;
                                dt.Rows.Add(row);


                            }
                        }

                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_LOT_TANK_PM", "IN_LOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_CANCEL_LOT_TANK_PM", searchException.Message, searchException.ToString());
                                    return;
                                }

                                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                                Search_Tank();
                                Search_Info();

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
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });


        }

        private void btnWipInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_051_Pre_Mix_Wipinfo wndWipInfo = new COM001_051_Pre_Mix_Wipinfo();
                wndWipInfo.FrameOperation = FrameOperation;

                if (wndWipInfo != null)
                {
                    object[] Parameters = new object[3];

                    C1WindowExtension.SetParameters(wndWipInfo, Parameters);

                    wndWipInfo.Closed += new EventHandler(wndWipInfo_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndWipInfo.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void wndWipInfo_Closed(object sender, EventArgs e)
        {
            COM001_051_Pre_Mix_Wipinfo window = sender as COM001_051_Pre_Mix_Wipinfo;
            if (window.DialogResult == MessageBoxResult.OK)
            {
               
            }
        }
    }
}
