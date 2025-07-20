/*************************************************************************************
 Created Date : 2024.07.10
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.07.10  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using System.Windows.Threading;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using C1.WPF.DataGrid;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_048 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public PACK003_048()
        {
            InitializeComponent();
            Initialize();         
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private DataTable dtCombo = null;

        private void InitializeCombo()
        {
            CommonCombo cbo = new CommonCombo();

            B2Bi_ShipTo_Combo();
            SetComboData();
            If_Flag_Combo();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeComponent();
            InitializeCombo();
        }
        #endregion

        #region B2Bi(LOT) 재전송

        private void txtSendLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string clipboardText = Clipboard.GetText();
                    string[] arrClipboardText = clipboardText.Split(stringSeparators, StringSplitOptions.None);

                    int maxLOTIDCount = 299;
                    string messageID = "SFU8217";
                    if (arrClipboardText.Count() > maxLOTIDCount)
                    {
                        Util.MessageValidation(messageID, 299);     // 최대 500개 까지 가능합니다. 
                        this.txtSendLotID.Clear();
                        this.txtSendLotID.Focus();
                        return;
                    }

                    var distinctWords = (from w in arrClipboardText where !string.IsNullOrEmpty(w) select w).Distinct().ToList();

                    if (null == distinctWords || distinctWords.Count() == 0)
                    {
                        this.txtSendLotID.Clear();
                        this.txtSendLotID.Focus();
                        return;
                    }

                    this.txtSendLotID.Text = distinctWords.Aggregate((current, next) => current + "," + next).ToString();

                    this.txtSendLotID_KeyDown(this, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }

        private void txtSendLotID_KeyDown(object sender, KeyEventArgs e)
        {            
            if (null == e || e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtSendLotID.Text))
                    {
                        // LOTID를 입력하세요
                        Util.MessageInfo("SFU2839");
                        return;
                    }

                    this.loadingIndicator.Visibility = Visibility.Visible;

                    #region 중복LOT Check
                    string sScan = this.txtSendLotID.Text.Trim();
                    string sLotDs = sScan;
                    if (sScan.Contains(","))
                    {
                        string[] splt = sScan.Split(',');
                        var distinctWords = (from w in splt where !string.IsNullOrEmpty(w) select w).Distinct().ToList();
                        sLotDs = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                    }
                    #endregion

                    SendSearchRowDataHeader(sLotDs);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    this.txtSendLotID.Clear();
                    this.txtSendLotID.Focus();
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnSendSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSendLotID.Text))
            {
                // LOTID를 입력하세요
                Util.MessageInfo("SFU2839");
                return;
            }

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                #region 중복LOT Check
                string sScan = this.txtSendLotID.Text.Trim();
                string sLotDs = sScan;
                if (sScan.Contains(","))
                {
                    string[] splt = sScan.Split(',');
                    var distinctWords = (from w in splt where !string.IsNullOrEmpty(w) select w).Distinct().ToList();
                    sLotDs = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                }
                #endregion

                SendSearchRowDataHeader(sLotDs);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtSendLotID.Clear();
                this.txtSendLotID.Focus();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #region BizCall - DA_PRD_SEL_TB_SFC_B2BI_LOT_PGM_TRNF_HDR
        private void SendSearchRowDataHeader(string Lots)
        {
            try
            {
                tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
               // Util.gridClear(dgSendRowdata);                

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Lots;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_B2BI_LOT_PGM_TRNF_HDR", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        DataTable dtMerge = dgSendRowdata.GetDataTable(true);

                        if (dtMerge == null)
                            dtMerge = dtResult;
                        else
                            dtMerge.Merge(dtResult);

                        Util.GridSetData(dgSendRowdata, dtMerge, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(tbRowDataHeaderCount, Util.NVC(dtMerge.Rows.Count));
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


        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgSendRowdata.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }

            dgSendRowdata.EndEdit();
            dgSendRowdata.EndEditRow(true);
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgSendRowdata.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgSendRowdata.EndEdit();
            dgSendRowdata.EndEditRow(true);
        }

        private void btnReMake_Click(object sender, RoutedEventArgs e)
        {
            if (Util.pageAuthCheck(FrameOperation.AUTHORITY))
            {
                RemakeRowdataProcess();
            }
            else
            {
                //해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                Util.MessageInfo("SFU3520", new object[] { LoginInfo.USERID, "ReMake" });
            }
        }

        #region BizCall - BR_PRD_REG_REMAKE_ROWDATA_LOTID
        private void RemakeRowdataProcess()
        {
            if (dgSendRowdata == null || dgSendRowdata.GetRowCount() == 0) return;

            int iRowCount = dgSendRowdata.GetRowCount();

            DataSet dsInput = new DataSet();

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SHIPTO", typeof(string));
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("LOTID", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            var distinctWords = (from row in dgSendRowdata.GetDataTable(true).AsEnumerable()
                                 where row.Field<string>("CHK") == "True"
                                 select new
                                 {
                                      SHIPTO = row.Field<string>("SHIPTO_NAME")//variable
                                     ,LOTID = row.Field<string>("LOTID")
                                 }).ToList();

            if (distinctWords.Select(dw => dw.SHIPTO).Distinct().Count() != 1)
            {
                //동일한 배송처 LOT만 함께 재전송이 가능합니다. 
                Util.MessageInfo("SFU3520", new object[] { LoginInfo.USERID, "ReMake" });
                return;
            }

            string Lots = string.Join(",", distinctWords.Select(dw => dw.LOTID));

            DataRow dr = INDATA.NewRow();
            dr["SHIPTO"] = distinctWords.Select(dw => dw.SHIPTO);
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LOTID"] = Lots;
            dr["NOTE"] = txtRemakr.Text.Trim();
            dr["USERID"] = LoginInfo.USERID;
            INDATA.Rows.Add(dr);
            dsInput.Tables.Add(INDATA);

            if (INDATA.Rows.Count == 0) return;

            //선택한 항목 데이터를 다시 생성 하시겠습니까?
            Util.MessageConfirm("SFU8261", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    RemakeRowdataBiz(dsInput);
                }
            });

        }
        private void RemakeRowdataBiz(DataSet dsInput)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REMAKE_ROWDATA_LOTID", "INDATA", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }

                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                        //화면 초기화
                        ClearSendData();

                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }, dsInput);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void btnSendClear_Click(object sender, RoutedEventArgs e)
        {
            ClearSendData();
        }

        private void ClearSendData()
        {
            Util.gridClear(dgSendRowdata);
            txtSendLotID.Text = string.Empty;
            tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }

        #endregion

        #region B2Bi(LOT) 재전송 이력

        private void cboLine_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            B2Bi_Line2ProdID_Combo();
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchRowDataHeader();
                txtPalletId.SelectAll();
            }
        }

        private void txtSearchLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchRowDataHeader();
                txtSearchLotId.SelectAll();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchRowDataHeader();
        }

        #region BizCall - DA_PRD_SEL_TB_SFC_B2BI_PGM_TRNF_HDR
        private void SearchRowDataHeader()
        {
            try
            {
                tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgSearchRowdataHd);
                Util.gridClear(dgRowdata);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_TO", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("B2BI_IF_GNRT_FLAG", typeof(string));
                RQSTDT.Columns.Add("IS_ERR_CNT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtPalletId.Text))
                {
                    dr["EQSGID"] = cboLine.SelectedValue;
                    dr["PRODID"] = cboProduct.SelectedValue;
                    dr["BOXID"] = null;

                    dr["ISS_DTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["ISS_DTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    dr["EQSGID"] = null;
                    dr["PRODID"] = null;
                    dr["ISS_DTTM_FROM"] = System.DBNull.Value;
                    dr["ISS_DTTM_TO"] = System.DBNull.Value;
                    dr["BOXID"] = txtPalletId.Text.Trim();
                }

                if (string.IsNullOrWhiteSpace(txtSearchLotId.Text))
                {
                    dr["LOTID"] = null;
                }
                else
                {
                    dr["LOTID"] = txtSearchLotId.Text.Trim();
                }
                dr["SHIPTO_ID"] = cboB2BiShipTo.SelectedValue;
                dr["B2BI_IF_GNRT_FLAG"] = cboIfFlag.SelectedValue;
                dr["IS_ERR_CNT"] = chkViewErrOnly.IsChecked ?? false ? "Y" : null;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_B2BI_PGM_TRNF_HDR", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgSearchRowdataHd, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbRowDataHeaderCount, Util.NVC(dtResult.Rows.Count));
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

        private void btnSaveExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgRowdata);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.GetCellFromPoint(pnt);

                if (crrCell.Row.Index == 0) return;

                if (crrCell != null)
                {
                    if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                    {
                        string sBoxId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "BOXID"));
                        string sRcvIssId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "RCV_ISS_ID"));
                        string sPgmId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "CUST_TRNF_PGM_ID"));
                        string sIfId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "IF_ID"));

                        SearchRowDataDetail(sPgmId, sIfId, sRcvIssId, sBoxId);
                    }
                }
            }
            catch { }
        }

        #region BizCall - BR_PRD_GET_B2BI_ROWDATA
        private void SearchRowDataDetail(string sPrgId, string sIfId, string sRcvIssId, string sBoxId)
        {
            try
            {
                txtRowdataCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgRowdata);

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CUST_TRNF_PGM_ID", typeof(string));
                INDATA.Columns.Add("IF_ID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("INPUT_SEQNO", typeof(Int64));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CUST_TRNF_PGM_ID"] = sPrgId.Trim();
                dr["IF_ID"] = sIfId.Trim();
                dr["RCV_ISS_ID"] = sRcvIssId.Trim();
                dr["BOXID"] = sBoxId.Trim();
                dr["INPUT_SEQNO"] = DBNull.Value;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);


                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_B2BI_ROWDATA", "INDATA", "OUT_HEADER,OUT_DATA", (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else if (dsResult.Tables["OUT_DATA"].Rows.Count != 0)
                        {
                            //Header columns 변경
                            for (int i = 0; i < dgRowdata.Columns.Count; i++)
                            {
                                dgRowdata.Columns[i].Tag = "N";
                                new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                dgRowdata.Columns[i].Visibility = Visibility.Collapsed;
                            }

                            for (int i = 0; i < dsResult.Tables["OUT_HEADER"].Rows.Count; i++)
                            {
                                string sTbCol = dsResult.Tables["OUT_HEADER"].Rows[i]["COLNAME"].ToString();
                                string sColName = dsResult.Tables["OUT_HEADER"].Rows[i]["TRNF_ITEM_NAME"].ToString();

                                dgRowdata.Columns[sTbCol].Tag = dsResult.Tables["OUT_HEADER"].Rows[i]["MAND_ITEM_FLAG"].ToString();

                                dgRowdata.Columns[sTbCol].Header = sColName;
                                dgRowdata.Columns[sTbCol].Visibility = Visibility.Visible;

                            }

                            Util.GridSetData(dgRowdata, dsResult.Tables["OUT_DATA"], FrameOperation);
                            Util.SetTextBlockText_DataGridRowCount(txtRowdataCount, Util.NVC(dsResult.Tables["OUT_DATA"].Rows.Count));

                            dgRowdata.Refresh();
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }
                }, dsInput);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        private void dgSearchRowdataHd_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (dgSearchRowdataHd == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);

                dgSearchRowdataHd.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Type != DataGridRowType.Item) return;

                    switch (e.Cell.Column.Name)
                    {
                        case "MAND_ERR_CNT":
                            {
                                if (e.Cell.Text.Equals("0"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                            }
                            break;
                        default:
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            break;

                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkViewErrOnly_Click(object sender, RoutedEventArgs e)
        {
            //SearchRowDataHeader();
        }

        private void dgRowdata_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (dgRowdata == null) return;

                dgRowdata.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Type != DataGridRowType.Item) return;

                    switch (e.Cell.Column.Tag.ToString())
                    {
                        case "Y":
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                break;
                            }
                        default:
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                break;
                            }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion     

        #region 콤보 박스 세팅
        private void If_Flag_Combo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtAllRow.Rows.Add(new object[] { null, "-ALL-" });
                dtAllRow.Rows.Add(new object[] { "Y", "Y" });
                dtAllRow.Rows.Add(new object[] { "N", "N" });

                cboIfFlag.DisplayMemberPath = "CBO_NAME";
                cboIfFlag.SelectedValuePath = "CBO_CODE";

                cboIfFlag.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboIfFlag.SelectedIndex = 0;

            }
            catch { }
        }

        private void SetComboData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                dtCombo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_B2BI_LINE_PRODID_COMBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtCombo.Rows.Count != 0)
                {
                    //Line 정보 Set
                    DataTable dtLineGp = dtCombo.DefaultView.ToTable(true, new string[] { "EQSGID", "EQSGNAME" });
                    DataTable dtAllRow = new DataTable();
                    dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("EQSGID"), new DataColumn("EQSGNAME") });
                    dtAllRow.Rows.Add(new object[] { null, "-ALL-" });

                    if (dtLineGp != null && dtLineGp.Rows.Count > 0)
                    {
                        dtAllRow.Merge(dtLineGp);
                    }

                    cboLine.DisplayMemberPath = "EQSGNAME";
                    cboLine.SelectedValuePath = "EQSGID";
                    cboLine.ItemsSource = DataTableConverter.Convert(dtAllRow);

                    cboLine.SelectedIndex = 0;

                    //제품 정보 Set
                    B2Bi_Line2ProdID_Combo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void B2Bi_ShipTo_Combo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("SHIPTO_ID"), new DataColumn("SHIPTO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { null, "-ALL-" });

                cboB2BiShipTo.DisplayMemberPath = "SHIPTO_NAME";
                cboB2BiShipTo.SelectedValuePath = "SHIPTO_ID";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_B2BI_SHIPTOID_COMBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboB2BiShipTo.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboB2BiShipTo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void B2Bi_Line2ProdID_Combo()
        {
            try
            {
                cboProduct.ItemsSource = null;
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("PRODID"), new DataColumn("PRODNAME") });
                dtAllRow.Rows.Add(new object[] { null, "-ALL-" });

                if (dtCombo.Rows.Count != 0)
                {
                    string sEqsgId = Util.NVC(cboLine.SelectedValue);
                    if (!string.IsNullOrWhiteSpace(sEqsgId))
                    {
                        DataTable dtPrdt = dtCombo.Select("EQSGID = '" + sEqsgId + "'").CopyToDataTable().DefaultView.ToTable(true, new string[] { "PRODID", "PRODNAME" });


                        if (dtPrdt != null && dtPrdt.Rows.Count > 0)
                        {
                            dtAllRow.Merge(dtPrdt);
                        }
                    }
                }

                cboProduct.DisplayMemberPath = "PRODNAME";
                cboProduct.SelectedValuePath = "PRODID";

                cboProduct.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
