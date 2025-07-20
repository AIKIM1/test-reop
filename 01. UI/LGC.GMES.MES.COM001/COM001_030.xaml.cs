/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.01.12  남기운    : ERP 오류 유형 추가
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_030 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtSearchResult;
        DataTable dtGridChek;
        Util _Util = new Util();

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        bool fullCheck = false;
        public COM001_030()
        {
            InitializeComponent();
            Loaded += COM001_030_Loaded;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        private void COM001_030_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= COM001_030_Loaded;

            Initialize();
        }


        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };


        private void dgData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }



        private void dgData_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":

                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                            if (!chk.IsChecked.Value)
                            {
                                chkAll.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll.IsChecked = true;
                            }

                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            break;
                    }
                }
            }
        }
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = ((DataView)dgSearchResult.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (Util.NVC(row["ERP_ERR_NAME"]).Equals("NG"))
                    {
                        for (int idx = 0; idx < dgSearchResult.Rows.Count; idx++)
                        {
                            row["CHK"] = true;
                        }
                    }
                }

            }
            catch
            {
                
            }
            
        }


        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgSearchResult.Rows.Count; idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgSearchResult.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }

        #region Initialize
        private void Initialize()
        {
            String[] sFilter = { "TRSF_POST_TYPE_CODE" };
            _combo.SetCombo(cboTranGubun, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            String[] sFilter1 = { "ERP_STATUS_CODE" };
            _combo.SetCombo(cboErpStatusCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReTran);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            this.Loaded -= UserControl_Loaded;
        }
   
        #endregion

        #region Mehod

        private void ClearGrid()
        {
            dgSearchResult.ItemsSource = null;
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        private void btnReTran_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK");

            if (idx < 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            ErrorMessage();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            if (Convert.ToDecimal(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            ClearGrid();
            try
            {
                Util.gridClear(dgSearchResult);

                string sTrsfPostTypeCode = Util.NVC(cboTranGubun.SelectedValue);
                if (sTrsfPostTypeCode == string.Empty)
                {
                    sTrsfPostTypeCode = null;
                }

                string sErpStatusCode = Util.NVC(cboErpStatusCode.SelectedValue);
                if (sErpStatusCode == string.Empty)
                {
                    sErpStatusCode = null;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));
                IndataTable.Columns.Add("TRSF_POST_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("USER_CHK_FLAG", typeof(string));
                IndataTable.Columns.Add("ERP_ERR_CODE", typeof(string));
                IndataTable.Columns.Add("TOP_ROW", typeof(Int16));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["STDT"] = chkError.IsChecked == true ? null : dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");//  Util.StringToDateTime(Util.GetCondition(dtpDateFrom), "yyyyMMdd");
                Indata["EDDT"] = chkError.IsChecked == true ? null : dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");// Util.StringToDateTime(Util.GetCondition(dtpDateTo), "yyyyMMdd");
                Indata["TRSF_POST_TYPE_CODE"] = sTrsfPostTypeCode;
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["USER_CHK_FLAG"] = chkError.IsChecked == true ? "N" : null;
                //Indata["ERP_ERR_CODE"] = chkError.IsChecked == true ? "FAIL" : null;
                if (chkError.IsChecked == true)
                {
                    Indata["ERP_ERR_CODE"] = "FAIL";
                }else
                {
                    Indata["ERP_ERR_CODE"] = sErpStatusCode;
                }
                Indata["TOP_ROW"] = txtCount.Value;
                Indata["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                Indata["ERP_TRNF_SEQNO"] = string.IsNullOrWhiteSpace(txtEepTrnfSeqno.Text) ? null : txtEepTrnfSeqno.Text;

                IndataTable.Rows.Add(Indata);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRSF_RSLT", "INDATA", "RSLTDT", IndataTable);               
                //Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);


                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_TRSF_RSLT", "RQSTDT", "RSLTDT", IndataTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        Util.GridSetData(dgSearchResult, result, FrameOperation, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnExcelActHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResult);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResult_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e?.Column != null)
            {
                //DataRowView drv = e.Row.DataItem as DataRowView;
                //if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                //{
                //    e.Cancel = e.Column.Name != "CHK" && drv["CHK"].GetString() != "True";
                //}
                //else
                //{
                //    if (e.Column.Name == "CHK")
                //    {
                //        e.Cancel = false;
                //    }
                //    else
                //    {
                //        if (exceptionColumns != null)
                //        {
                //            e.Cancel = exceptionColumns.Contains(e.Column.Name) || drv != null && drv["CHK"].GetString() != "True";
                //        }
                //        else
                //        {
                //            e.Cancel = drv != null && drv["CHK"].GetString() != "True";
                //        }
                //    }
                //}
            }
        }

        private void dgSearchResult_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgSearchResult.CurrentRow == null || dgSearchResult.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgSearchResult.CurrentColumn.Name == "CHK")
                {
                    string chkValue = Util.NVC(dgSearchResult.GetCell(dgSearchResult.CurrentRow.Index, dgSearchResult.Columns["CHK"].Index).Value);
                    string sErrCode = Util.NVC(dgSearchResult.GetCell(dgSearchResult.CurrentRow.Index, dgSearchResult.Columns["ERP_ERR_CODE"].Index).Value);

                    //for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                    //{
                    //    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                    //}

                    if (chkValue == "0" && sErrCode == "FAIL")
                    {
                        DataTableConverter.SetValue(dgSearchResult.Rows[dgSearchResult.CurrentRow.Index].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgSearchResult.Rows[dgSearchResult.CurrentRow.Index].DataItem, "CHK", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
            finally
            {
                dgSearchResult.CurrentRow = null;
            }
        }


        private void ErrorMessage()
        {
            try
            {
                DataTable dts = ((DataView)dgSearchResult.ItemsSource).Table;
                foreach (DataRow rows in dts.Rows)
                {
                    for (int i = 0; i < dgSearchResult.Rows.Count(); i++)
                    {
                        if (rows["CHK"].ToString().Equals("True") || rows["CHK"].ToString().Equals("1"))
                        {
                            //if (!Util.NVC(rows["ERP_ERR_NAME"]).Equals("NG"))
                            if (!Util.NVC(rows["ERP_ERR_CODE"]).Equals("FAIL"))
                            {
                                Util.Alert("SFU2948");  //전송실패(NG)된 항목만 재전송 가능합니다.
                                return;
                            }
                        }
                    }
                }
                ReserveSend();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void ReserveSend()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    if (Util.NVC(dgSearchResult.GetCell(i, dgSearchResult.Columns["CHK"].Index).Value).Equals("True") ||
                        Util.NVC(dgSearchResult.GetCell(i, dgSearchResult.Columns["CHK"].Index).Value) == "1")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["ERP_TRNF_SEQNO"] = Util.NVC(dgSearchResult.GetCell(i, dgSearchResult.Columns["ERP_TRNF_SEQNO"].Index).Value);
                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("BR_ACT_REG_RESEND_TRSF_POST", "RQSTDT", "", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex == null)
                    {
                        for (int i = 0; i < dgSearchResult.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.

                        btnSearch_Click(null, null);
                    }
                    else
                    {
                        Util.Alert(ex.ToString());
                    }
            });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK");

                if (idx < 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                Save();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void Save()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataTable dts = ((DataView)dgSearchResult.ItemsSource).Table;
                        foreach (DataRow rows in dts.Rows)
                        {
                            for (int i = 0; i < dgSearchResult.Rows.Count(); i++)
                            {
                                if (rows["CHK"].ToString().Equals("True") || rows["CHK"].ToString().Equals("1"))
                                {
                                    DataTable RQSTDT = new DataTable();
                                    RQSTDT.TableName = "RQSTDT";
                                    RQSTDT.Columns.Add("USER_CHK_FLAG", typeof(String));
                                    RQSTDT.Columns.Add("NOTE", typeof(String));
                                    RQSTDT.Columns.Add("ERP_TRNF_SEQNO", typeof(String));
                                    RQSTDT.Columns.Add("USERID", typeof(String));

                                    DataRow dr = RQSTDT.NewRow();
                                    dr["USER_CHK_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "USER_CHK_FLAG"));
                                    dr["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "NOTE"));
                                    dr["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[i].DataItem, "ERP_TRNF_SEQNO"));
                                    dr["USERID"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);
                                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TRSF_RSLT", "RQSTDT", "RSLTDT", RQSTDT);

                                    //    new ClientProxy().ExecuteService("DA_PRD_UPD_ERP_HIST", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
                                    //    {
                                    //        try
                                    //        {
                                    //            if (searchException != null)
                                    //            {
                                    //                Util.MessageException(searchException);
                                    //                return;
                                    //            }

                                    //            Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                    //            GetList();
                                    //        }
                                    //        catch (Exception ex)
                                    //        {
                                    //            Util.MessageException(ex);
                                    //        }
                                    //    }
                                    //);
                                }
                            }
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                        btnSearch_Click(null, null);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkError_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    dtpDateFrom.IsEnabled = false;
                    dtpDateTo.IsEnabled = false;
                    cboErpStatusCode.SelectedValue = "FAIL";
                    cboErpStatusCode.IsEnabled = false;
                }
                else
                {
                    dtpDateFrom.IsEnabled = true;
                    dtpDateTo.IsEnabled = true;
                    cboErpStatusCode.SelectedValue = "";
                    cboErpStatusCode.IsEnabled = true;
                }
            }
        }
    }
}
