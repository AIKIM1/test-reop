/*************************************************************************************
 Created Date :2020.10.20
      Creator :최우석
   Decription : B2BI 전송 정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.20  최우석    SI          Initialize
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_080 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private DataTable dtCombo = null;
        #endregion

        #region [ Initialize ]
        public PACK001_080()
        {
            InitializeComponent();
            tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            txtRowdataCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
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

            B2Bi_ShipTo_Combo();
            SetComboData();
            If_Flag_Combo();


        }
        #endregion

        #region [UserControl_Loaded]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }

        #endregion

        #region Process Event

        private void chkViewErrOnly_Click(object sender, RoutedEventArgs e)
        {
            //SearchRowDataHeader();
        }

        private void cboLine_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            B2Bi_Line2ProdID_Combo();
        }
        //검색 버튼
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchRowDataHeader();
        }
        //팔레트 Enter
        private void txtPalletId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchRowDataHeader();
                txtPalletId.SelectAll();
            }
        }
        //제외처리
        private void btnExcept_Click(object sender, RoutedEventArgs e)
        {
            ExceptPallet();
        }
        //재생성 버튼
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
        //스프레드 숫자 클릭
        private void dgCellList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        #region ( 선택 전체 취소 )
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgRowdataHd.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }

            dgRowdataHd.EndEdit();
            dgRowdataHd.EndEditRow(true);
        }
        #endregion

        #region ( 체크박스 전체 선택 )
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgRowdataHd.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgRowdataHd.EndEdit();
            dgRowdataHd.EndEditRow(true);
        }
        #endregion

        private void dgRowdata_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgRowdataHd_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (dgRowdataHd == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);

                dgRowdataHd.Dispatcher.BeginInvoke(new Action(() =>
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
                        case "CHK":
                            {
                                CheckBox ckFlag = e.Cell.Presenter.Content as CheckBox;

                                if (ckFlag != null && Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CUST_DATA_TRNF_FLAG")).Equals("Y"))
                                {
                                    ckFlag.IsEnabled = false;
                                    ckFlag.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    ckFlag.IsEnabled = true;
                                    ckFlag.Visibility = Visibility.Visible;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        #region 엑셀 다운 로드
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

        #endregion

        #region BizCall - BR_PRD_REG_B2BI_TRNF_EXCEPT (미구현)
        private void ExceptPallet()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region BizCall - BR_PRD_REG_REMAKE_ROWDATA_BOXID
        private void RemakeRowdataProcess()
        {
            if (dgRowdataHd == null || dgRowdataHd.GetRowCount() == 0) return;

            bool isSendConfirm = false;
            int iRowCount = dgRowdataHd.GetRowCount();

            DataSet dsInput = new DataSet();

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("CUST_TRNF_PGM_ID", typeof(string));
            INDATA.Columns.Add("IF_ID", typeof(string));
            INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));


            for (int i = 1; i < iRowCount + 1; i++)
            {
                if (DataTableConverter.GetValue(dgRowdataHd.Rows[i].DataItem, "CHK").ToString().Equals("True"))
                {
                    string sIf_Trnf_Flag = DataTableConverter.GetValue(dgRowdataHd.Rows[i].DataItem, "CUST_DATA_TRNF_FLAG").ToString();
                    if (sIf_Trnf_Flag.Equals("Y"))
                    {
                        isSendConfirm = true;
                    }
                    DataRow dr = INDATA.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CUST_TRNF_PGM_ID"] = DataTableConverter.GetValue(dgRowdataHd.Rows[i].DataItem, "CUST_TRNF_PGM_ID").ToString();
                    dr["IF_ID"] = DataTableConverter.GetValue(dgRowdataHd.Rows[i].DataItem, "IF_ID").ToString();
                    dr["RCV_ISS_ID"] = DataTableConverter.GetValue(dgRowdataHd.Rows[i].DataItem, "RCV_ISS_ID").ToString();
                    dr["BOXID"] = DataTableConverter.GetValue(dgRowdataHd.Rows[i].DataItem, "BOXID").ToString();
                    dr["USERID"] = LoginInfo.USERID;
                    INDATA.Rows.Add(dr);
                }
            }

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
                    //이미 전송 완료 항목인경우 확인 메시지
                    if (isSendConfirm)
                    {
                        //전송 완료된 항목이 존재 합니다. 작업 하시겠습니까?
                        Util.MessageConfirm("SFU8260", (result2) =>
                        {
                            if (result2 != MessageBoxResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                RemakeRowdataBiz(dsInput);
                            }
                        });
                    }
                    else
                    {
                        RemakeRowdataBiz(dsInput);
                    }
                }
            });

        }
        private void RemakeRowdataBiz(DataSet dsInput)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REMAKE_ROWDATA_BOXID", "INDATA", null, (dsResult, dataException) =>
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

                        SearchRowDataHeader();

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

        #region BizCall - DA_PRD_SEL_TB_SFC_B2BI_PGM_TRNF_HDR
        private void SearchRowDataHeader()
        {
            try
            {
                tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgRowdataHd);
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
                        Util.GridSetData(dgRowdataHd, dtResult, FrameOperation);

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




    }
}
