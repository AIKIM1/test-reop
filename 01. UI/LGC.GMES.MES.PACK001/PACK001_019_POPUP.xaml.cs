using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_019_POPUP : C1Window, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        string sEmpty_Lot = String.Empty;

        private DataTable isCreateTable = new DataTable();
        private DataTable isDeleteTable = new DataTable();

        string strInputAndMove = string.Empty;
        string strInputNg = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_019_POPUP()
        {
            InitializeComponent();
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    getTagetPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void getTagetPalletInfo()
        {
            try
            {
                ShowLoadingIndicator();

                if (validation(txtPalletID.Text))
                {
                    if (!string.IsNullOrEmpty(txtPalletID.Text) && Multi_Create(txtPalletID.Text) == false)
                    {
                    }
                }
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CELL_PLT_BCD_USE_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtReturn.Rows.Count > 0)
                {
                    PLLT_BCD_ID.Visibility = Visibility.Visible;
                }

                else
                {
                    PLLT_BCD_ID.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 10)
                    {
                        Util.MessageValidation("SFU4643");   //최대 10개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (sPasteStrings[i] != "" && !string.IsNullOrEmpty(sPasteStrings[i])) { 
                            if (validation(sPasteStrings[i]))
                            {
                                if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Create(sPasteStrings[i]) == false)
                                    break;
                            }
                         }
                    }
                    DoEvents();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        bool validation(string sPalletId)
        {
            try
            {
                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                        if (Util.ToString(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID")).ToUpper().Equals(sPalletId.ToUpper()))
                        {
                            Util.MessageValidation("SFU1781", sPalletId);  // 입력한 LOTID[% 1] 정보가 없습니다.
                            return false;
                        }
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        bool Multi_Create(string sLotid)
        {
            try
            {
                DoEvents();

                DataTable inTable = new DataTable();
                //inTable.Columns.Add("FROM_AREA", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));

                DataRow dr = inTable.NewRow();
                //dr["FROM_AREA"] = LoginInfo.CFG_AREA_ID;
                dr["BOXID"] = sLotid;

                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_PALLET_INFO_CWA", "INDATA", "OUTDATA", ds);
                DataTable dtResult = dsResult.Tables["OUTDATA"];

                if (dtResult.Rows.Count < 1)
                {
                    return false;
                }

                if (!dtResult.Columns.Contains("CHK"))
                {
                    dtResult.Columns.Add("CHK", typeof(Boolean));
                }

                if (!dtResult.Columns.Contains("RESULT"))
                {
                    dtResult.Columns.Add("RESULT", typeof(string));
                }

                for(int i=0; dtResult.Rows.Count > i; i++)
                {
                 
                    if (dtResult.Rows[i]["EXIST_AREA"].ToString().Equals(LoginInfo.CFG_AREA_ID))
                    {
                        dtResult.Rows[i]["CHK"] = false;
                        dtResult.Rows[i]["RESULT"] = "해당 동에 입고 상태입니다.";
                    }
                    else if (dtResult.Rows[i]["BOX_RCV_ISS_STAT_CODE"].ToString().Equals("MOVED"))
                    {
                        dtResult.Rows[i]["CHK"] = false;
                        dtResult.Rows[i]["RESULT"] = "완료 처리된 상태입니다.";
                    }
                    else if (dtResult.Rows[i]["FCS_HOLD_CNT"].GetInt() > 0 || dtResult.Rows[i]["WIP_HOLD_CNT"].GetInt() > 0)
                    {
                        dtResult.Rows[i]["CHK"] = false;
                        dtResult.Rows[i]["RESULT"] = "Hold 상태의 Cell이 존재합니다.";
                    }
                    else if ((dtResult.Rows[i]["WAIT_CNT"].ToString().Equals("0") && !(string.IsNullOrEmpty(dtResult.Rows[i]["IWMS_RCV_ID"].ToString()))))
                    {
                        dtResult.Rows[i]["CHK"] = false;
                        dtResult.Rows[i]["RESULT"] = "입고 후 이동 가능한 Pallet 입니다.";
                    }
                    else if (!(dtResult.Rows[i]["WAIT_CNT"].ToString().Equals("0")) && !(dtResult.Rows[i]["WAIT_CNT"].ToString().Equals(dtResult.Rows[i]["PALLET_CNT"].ToString())))
                    {
                        dtResult.Rows[i]["CHK"] = false;
                        dtResult.Rows[i]["RESULT"] = "투입이 진행되기 시작한 Pallet 입니다.";
                    }
                    else if ( string.IsNullOrEmpty(dtResult.Rows[i]["IWMS_RCV_ID"].ToString())
                        && dtResult.Rows[i]["WAIT_CNT"].ToString().Equals(dtResult.Rows[i]["PALLET_CNT"].ToString())
                        && !(dtResult.Rows[i]["EXIST_AREA"].ToString().Equals(LoginInfo.CFG_AREA_ID)))
                    { 
                        dtResult.Rows[i]["CHK"] = true;
                        dtResult.Rows[i]["RESULT"] = "True";
                    }
                    else if ((dtResult.Rows[i]["WAIT_CNT"].ToString().Equals("0")))
                    {
                        dtResult.Rows[i]["CHK"] = true;
                        dtResult.Rows[i]["RESULT"] = "True";
                    }
                }

                DataTable DTTMP = DataTableConverter.Convert(dgTagetList.ItemsSource);

                if(DTTMP.Rows.Count > 10)
                {
                    Util.MessageValidation("SFU4643");   //최대 10개 까지 가능합니다.
                    return false;
                }
                else if(DTTMP.Rows.Count > 0)
                {
                    DTTMP.Merge(dtResult);
                }
                else
                {
                    if (DTTMP?.Rows?.Count < 1) DTTMP = dtResult;
                }

                Util.GridSetData(dgTagetList, DTTMP, FrameOperation, true);


                /*
                if (dtResult.Rows.Count == 0)
                {
                    DataTable DTTMP = DataTableConverter.Convert(dgTagetList.ItemsSource);
                    
                    if (DTTMP?.Rows?.Count < 1) DTTMP = dtResult;
                    if (!DTTMP.Columns.Contains("RESULT"))
                    {
                        DTTMP.Columns.Add("RESULT",typeof(String));
                    }
                    DataRow ds1= DTTMP.NewRow();
                    ds1["CHK"] = true;
                    ds1["PALLETID"] = sLotid;

                    DTTMP.Rows.Add(ds1);
                    
                    Util.GridSetData(dgTagetList, DTTMP, FrameOperation, true);
                    return true;
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgTagetList.ItemsSource);
                    if (!dtInfo.Columns.Contains("RESULT"))
                    {
                        dtInfo.Columns.Add("RESULT", typeof(String));
                    }
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgTagetList, dtInfo, FrameOperation,true);

                    return true;
                }
                */
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void btnTagetMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                Util.MessageConfirm("SFU1763", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int ichkCount = 0;

                        DataRow[] dr = DataTableConverter.Convert(dgTagetList.ItemsSource).Select("CHK = True");
                                               
                        if (dr?.Length > 10)
                        {
                            Util.MessageValidation("SFU4643");   //최대 10개 까지 가능합니다.
                            return;
                        }
                        else if (dr.Length < 1)
                        {
                            Util.MessageInfo("SFU1636");
                        }

                        try
                        {
                            for (int i = 0; i < dgTagetList.Rows.Count; i++)
                            {
                                if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "CHK")), "True"))
                                {
                                    ichkCount++;

                                    string sPallteId = Util.ToString(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID"));
                                    string strExistArea = Util.ToString(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "EXIST_AREA"));
                                    if (!string.IsNullOrEmpty(sPallteId) && sPallteId != "")
                                    {
                                        stockMove2(sPallteId, strExistArea);
                                    }
                                }
                            }
                            Util.gridClear(dgTagetList);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        HiddenLoadingIndicator();
                    }
                    else
                    {
                        HiddenLoadingIndicator();
                        Util.MessageInfo("SFU1937");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                            HiddenLoadingIndicator();
            }

        }

        private void stockMove(string sPalletId, bool bHide = false)
        {
            DoEvents();
            DataSet dsInput = new DataSet();

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("AREAID", typeof(string));


            DataRow dr = INDATA.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["RCV_ISS_ID"] = null;
            dr["BOXID"] = sPalletId;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;


            INDATA.Rows.Add(dr);

            dsInput.Tables.Add(INDATA);

            new ClientProxy().ExecuteService("BR_PRD_REG_RECEIVE_PRODUCT_MOVING", "INDATA", null, INDATA, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                    }
                    DoEvents();

                    DataSet ds = new DataSet();
                    DataTable INDATA1 = new DataTable();
                    INDATA1.TableName = "INDATA";
                    INDATA1.Columns.Add("LANGID", typeof(string));
                    INDATA1.Columns.Add("PALLETID", typeof(string));


                    DataRow dr1 = INDATA1.NewRow();
                    dr1["LANGID"] = LoginInfo.LANGID;
                    dr1["PALLETID"] = sPalletId;

                    INDATA1.Rows.Add(dr1);
                    ds.Tables.Add(INDATA1);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_BOX", "INDATA", "OUTDATA", ds);
                    DataTable dtResult = dsResult.Tables["OUTDATA"];

                    if (dtResult.Rows.Count == 0)
                    {
                        for (int i = 0; i < dgTagetList.Rows.Count; i++)
                        {
                            if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID")), sPalletId))
                            {
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "CHK", "0");
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RESULT", "NG");
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dgTagetList.Rows.Count; i++)
                        {
                            if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID")), sPalletId))
                            {
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "CHK", "0");
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RCV_ISS_ID", dtResult.Rows[0]["RCV_ISS_ID"].ToString());
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "PALLET_CNT", dtResult.Rows[0]["PALLET_CNT"].ToString());
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "PRODID", dtResult.Rows[0]["PRODID"].ToString());
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "PRODNAME", dtResult.Rows[0]["PRODNAME"].ToString());
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RCV_ISS_STAT_CODE", dtResult.Rows[0]["RCV_ISS_STAT_CODE"].ToString());
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "BOX_RCV_ISS_STAT_CODE", dtResult.Rows[0]["BOX_RCV_ISS_STAT_CODE"].ToString());
                                DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RESULT", "OK");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                }
            });
        }

        private void stockMove1(string sPalletId)
        {
            DoEvents();

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("AREAID", typeof(string));


            DataRow dr = INDATA.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["RCV_ISS_ID"] = null;
            dr["BOXID"] = sPalletId;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            INDATA.Rows.Add(dr);

            DataTable bizResult =  new ClientProxy().ExecuteServiceSync("BR_PRD_REG_RECEIVE_PRODUCT_MOVING", "INDATA", null, INDATA);

            DataSet ds = new DataSet();
            DataTable INDATA1 = new DataTable();
            INDATA1.TableName = "INDATA";
            INDATA1.Columns.Add("LANGID", typeof(string));
            INDATA1.Columns.Add("PALLETID", typeof(string));


            DataRow dr1 = INDATA1.NewRow();
            dr1["LANGID"] = LoginInfo.LANGID;
            dr1["PALLETID"] = sPalletId;

            INDATA1.Rows.Add(dr1);
            ds.Tables.Add(INDATA1);

            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_BOX", "INDATA", "OUTDATA", ds);
            DataTable dtResult = dsResult.Tables["OUTDATA"];

            if (dtResult.Rows.Count == 0)
            {
                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID")), sPalletId))
                    {
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "CHK", "0");
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RESULT", "NG");
                    }
                }
            }
            else
            {
                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID")), sPalletId))
                    {
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "CHK", "0");
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RCV_ISS_ID", dtResult.Rows[0]["RCV_ISS_ID"].ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "PALLET_CNT", dtResult.Rows[0]["PALLET_CNT"].ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "PRODID", dtResult.Rows[0]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "PRODNAME", dtResult.Rows[0]["PRODNAME"].ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RCV_ISS_STAT_CODE", dtResult.Rows[0]["RCV_ISS_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "BOX_RCV_ISS_STAT_CODE", dtResult.Rows[0]["BOX_RCV_ISS_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[i].DataItem, "RESULT", "OK");
                    }
                }
            }
        }

        private void stockMove2(string sPalletId, string strExistArea)
        {
            DoEvents();

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SRCTYPE"        , typeof(string));
            INDATA.Columns.Add("LANGID"         , typeof(string));
            INDATA.Columns.Add("SHOPID"         , typeof(string));
            INDATA.Columns.Add("BOXID"          , typeof(string));
            INDATA.Columns.Add("FROM_AREAID"    , typeof(string));
            INDATA.Columns.Add("TO_AREAID"      , typeof(string));
            INDATA.Columns.Add("NOTE"           , typeof(string));
            INDATA.Columns.Add("USERID"         , typeof(string));
            


            DataRow dr = INDATA.NewRow();
            dr["SRCTYPE"]       = "UI";
            dr["LANGID"]        = LoginInfo.LANGID;
            dr["SHOPID"]        = LoginInfo.CFG_SHOP_ID;
            dr["BOXID"]         = sPalletId;
            dr["FROM_AREAID"]   = strExistArea;
            dr["TO_AREAID"]     = LoginInfo.CFG_AREA_ID;
            dr["NOTE"]          = "";
            dr["USERID"]        = LoginInfo.USERID;

            INDATA.Rows.Add(dr);

            DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CELL_MOVE_AREA", "INDATA", "OUTDATA", INDATA);

            
        }

        //전체 행 체크
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgTagetList.Rows)
            {
                if(DataTableConverter.GetValue(dgTagetList.Rows[row.Index].DataItem, "RESULT") != null) { 
                    if (true && DataTableConverter.GetValue(dgTagetList.Rows[row.Index].DataItem, "RESULT").ToString().Equals("True"))
                    {
                            DataTableConverter.SetValue(row.DataItem, "CHK", true);
                    }
                    else if (DataTableConverter.GetValue(dgTagetList.Rows[row.Index].DataItem, "RESULT").ToString().Equals("True"))
                    {
                            DataTableConverter.SetValue(row.DataItem, "CHK", "Y");
                    }
                }
            }
            dgTagetList.EndEdit();
            dgTagetList.EndEditRow(true);
        }

        //전체 행 체크 해제
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgTagetList.Rows)
            {
                if (true)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "N");
                }
            }
            dgTagetList.EndEdit();
            dgTagetList.EndEditRow(true);
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            DoEvents();
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    DataTable DTTMP = DataTableConverter.Convert(dgTagetList.ItemsSource);
                    DTTMP.Rows[index].Delete();

                    Util.GridSetData(dgTagetList, DTTMP, FrameOperation, true);

                    dgTagetList.Focus();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgTagetList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTagetList.GetRowCount() == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTagetList.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int seleted_row = dgTagetList.CurrentRow.Index;

                if (DataTableConverter.GetValue(dgTagetList.Rows[seleted_row].DataItem, "CHK").ToString() == "True" )
                {
                    if (DataTableConverter.GetValue(dgTagetList.Rows[seleted_row].DataItem, "RESULT").ToString().Equals("True"))
                    {
                        DataTableConverter.SetValue(dgTagetList.Rows[seleted_row].DataItem, "CHK", true);
                    }
                    else
                    {
                        Util.MessageInfo(DataTableConverter.GetValue(dgTagetList.Rows[seleted_row].DataItem, "RESULT").ToString());
                        DataTableConverter.SetValue(dgTagetList.Rows[seleted_row].DataItem, "CHK", false);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
