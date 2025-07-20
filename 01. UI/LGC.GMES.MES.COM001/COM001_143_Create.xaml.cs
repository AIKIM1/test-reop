using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Data;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_143_Create.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_143_Create : C1Window, IWorkArea
    {
        public string _reqNo = string.Empty;
        string _reqUserName = string.Empty;
        string _reqUserID = string.Empty;
        string _ApprUserName = string.Empty;
        string _ApprUserID = string.Empty;
        string _title = string.Empty;
        string _remark = string.Empty;
        DateTime _InData = DateTime.Now;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_143_Create()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null && tmps.Length >= 1)
                {
                    _reqNo = Util.NVC(tmps[0]);
                    _reqUserName = Util.NVC(tmps[1]);
                    _reqUserID = Util.NVC(tmps[2]);
                    _ApprUserName = Util.NVC(tmps[3]);
                    _ApprUserID = Util.NVC(tmps[4]);
                    _title = Util.NVC(tmps[5]);
                    _remark = Util.NVC(tmps[6]);
                    _InData = Convert.ToDateTime(Util.NVC(tmps[7]));
                }
                fn_Init(); // 기본값 셋팅

            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }

        }

        private void fn_Init()
        {
            //신규
            if (_reqNo.Equals(""))
            {
                txtRequestId.Text = "New";
                txtRequestUser.Text = _reqUserName;
                txtRequestUser.Tag = _reqUserID;
                txtApprover.Text = _ApprUserName;
                txtApprover.Tag = _ApprUserID;
                txtTitle.Text = _title;
                txtRemark.Text = _remark;
                //btnDelete.Visibility = Visibility.Hidden;
                btnRequest.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Hidden;
            }
            else //수정
            {
                txtRequestId.Text = _reqNo;
                txtRequestUser.Text = _reqUserName;
                txtRequestUser.Tag = _reqUserID;
                txtApprover.Text = _ApprUserName;
                txtApprover.Tag = _ApprUserID;
                txtTitle.Text = _title;
                txtRemark.Text = _remark;
                dtpCrryDate.SelectedDateTime = _InData;

                btnRequest.Visibility = Visibility.Hidden;
                btnSave.Visibility = Visibility.Visible;
                // 조회
                fn_Tout_Lot_Search();
            }
        }

        private void fn_Tout_Lot_Search()
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("TOUT_ID", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["TOUT_ID"] = _reqNo;

            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_GET_TOUT_LOT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dtResult.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                    return;
                }
                Util.GridSetData(dgLotList, dtResult, FrameOperation);
                fn_Set_Count();
            });

        }

        private void btnReturnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void getReturnTagetCell_By_Excel()
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();
                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);

                        if (dtExcelData != null)
                        {
                            //정보 조회
                            var lot = string.Join(",", dtExcelData.AsEnumerable().Select(x => x["Column1"].ToString()));
                            fn_LotSearch(lot);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void fn_LotSearch(string lot)
        {
            if(string.IsNullOrEmpty(lot))
            {
                Util.Alert("SFU1009");
                return;
            }
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lot;

            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_SEL_OUT_PRODUCT_REQUEST_LOT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dtResult.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                    return;
                }
                if (!fn_Vaildation(dtResult)) return;


                if (dgLotList.ItemsSource != null)
                {
                    DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                    //dt.Merge(dtResult);
                    DataRow row = null;
                    foreach (DataRow ro in dtResult.Rows)
                    {
                        row = dt.NewRow();
                        row["CHK"] = ro["CHK"].ToString();
                        row["PROCNAME"] = ro["PROCNAME"].ToString();
                        row["PRODUCTNAME"] = ro["PRODUCTNAME"].ToString();
                        row["ITEM"] = ro["ITEM"].ToString();
                        row["LOTID"] = ro["ITEM"].ToString();
                        row["STATUS"] = ro["STATUS"].ToString();
                        row["STATUSNAME"] = ro["STATUSNAME"].ToString();
                        dt.Rows.Add(row);
                    }

                    Util.GridSetData(dgLotList, dt, FrameOperation);
                }
                else
                {
                    Util.GridSetData(dgLotList, dtResult, FrameOperation);
                }
                txtLot.Text = "";
                fn_Set_Count();
            });
        }

        private bool fn_Vaildation(DataTable dtResult)
        {
            //기존에 있는 데이터 입력시 체크
            if (dgLotList.ItemsSource != null)
            {
                DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);

                var tmp = (from a in dt.AsEnumerable()
                           join b in dtResult.AsEnumerable()
                              on
                                  new
                                  {
                                      LOTID = a.Field<string>("LOTID")
                                  }
                              equals
                                  new
                                  {
                                      LOTID = b.Field<string>("ITEM")
                                  }
                           select new
                           {
                               LOTID = a.Field<string>("LOTID")
                           }).ToArray();
                if (tmp.Count() > 0)
                {
                    //중복 입력되었습니다.
                    Util.MessageValidation("SFU1376", tmp[0]);  //LOT ID는 중복 입력할수 없습니다.(%1)
                    return false;

                }

            }

            //이미 불출요청이 있는 Lot이  있는지 확인(반려 및 취소된 랏은 재 요청 가능)
            //TOUT_STAT_CODE : R- 생성, O-불출 
            //TOUT_APPR_STAT_CODE: R - 생성, A - 승인
            string ReqDup = dtResult.AsEnumerable().Where(x => new string[] {"R","O" }.Contains(x["TOUT_STAT_CODE"].ToString()) && new string[] {"R","A" }.Contains(x["TOUT_APPR_STAT_CODE"].ToString())).Select(z => z["ITEM"].ToString()).FirstOrDefault();
            if (!string.IsNullOrEmpty(ReqDup))
            {
                //이미 불출 요청이된 LotID 입니다.
                Util.AlertInfo(string.Format("{0} : {1}", ReqDup, MessageDic.Instance.GetMessage("RTLS0006")));
                return false;
            }
            //WIP 존재 확인
            string wipLot = dtResult.AsEnumerable().Where(x => x["WIP_LOT"].ToString() == "").Select(z => z["ITEM"].ToString()).FirstOrDefault();
            if (!string.IsNullOrEmpty(wipLot))
            {
                //존재하지 않는 LOT 입니다.
                Util.AlertInfo(string.Format("{0} : {1}", wipLot, MessageDic.Instance.GetMessage("RTLS0005")));
                return false;
            }
            return true;
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            //정합성 비즈 호출 
            string lotID = txtLot.Text;
            fn_LotSearch(lotID);
        }

        private void fn_Set_Count()
        {
            //dgLotList
            Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dgLotList.Rows.Count));
        }
 
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        private void GetUserWindow()
        {
            COM001_143_Auth wndPerson = new COM001_143_Auth();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = string.Empty;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            COM001_143_Auth wndPerson = sender as COM001_143_Auth;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtApprover.Text = wndPerson.USERNAME;
                txtApprover.Tag = wndPerson.USERID;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU3533", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        SaveList();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }
        }

        private void SaveList()
        {
            if (!fn_Save_Valid())
            {
                return;
            }
            DataSet indataSet = new DataSet();
            DataTable TB_TOUT = indataSet.Tables.Add("TB_TOUT");
            TB_TOUT.Columns.Add("TOUT_ID", typeof(string));
            TB_TOUT.Columns.Add("TOUT_REQ_TITLE", typeof(string));
            TB_TOUT.Columns.Add("TOUT_REQ_USER_ID", typeof(string));
            TB_TOUT.Columns.Add("TOUT_RSN_CNTT", typeof(string));
            TB_TOUT.Columns.Add("TOUT_APPR_USER_ID", typeof(string));
            TB_TOUT.Columns.Add("TOUT_APPR_STAT_CODE", typeof(string));
            TB_TOUT.Columns.Add("TOUT_APPR_AUTHID", typeof(string));
            TB_TOUT.Columns.Add("CRRY_SCHD_DTTM", typeof(string));
            TB_TOUT.Columns.Add("INSUSER", typeof(string));
            TB_TOUT.Columns.Add("DEL_FLAG", typeof(string));

            DataRow row = TB_TOUT.NewRow();
            row["TOUT_ID"] = _reqNo;
            row["TOUT_REQ_TITLE"] = txtTitle.Text;
            row["TOUT_REQ_USER_ID"] = txtRequestUser.Tag;
            row["TOUT_RSN_CNTT"] = txtRemark.Text;
            row["TOUT_APPR_USER_ID"] = txtApprover.Tag;
            row["TOUT_APPR_STAT_CODE"] = "R";
            row["TOUT_APPR_AUTHID"] = txtApprover.Tag.ToString();
            row["CRRY_SCHD_DTTM"] = dtpCrryDate.SelectedDateTime.ToShortDateString();
            row["INSUSER"] = LoginInfo.USERID;
            row["DEL_FLAG"] = "N";

            TB_TOUT.Rows.Add(row);

            DataTable TB_TOUT_LOT = indataSet.Tables.Add("TB_TOUT_LOT");
            TB_TOUT_LOT.TableName = "TB_TOUT_LOT";
            TB_TOUT_LOT.Columns.Add("LOTID", typeof(string));
            TB_TOUT_LOT.Columns.Add("DEL_FLAG", typeof(string));
            TB_TOUT_LOT.Columns.Add("TOUT_STAT_CODE", typeof(string));

            if (_reqNo.Equals("")) //신규이면
            {
                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "STATUS")).Equals("D"))
                        {
                            row = TB_TOUT_LOT.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "ITEM"));
                            row["DEL_FLAG"] = "N";
                            row["TOUT_STAT_CODE"] = "R"; // 생성
                            indataSet.Tables["TB_TOUT_LOT"].Rows.Add(row);
                        }
                    }
                }
            }
            else
            {

                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {

                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "STATUS")).Equals("D"))
                        {
                            row = TB_TOUT_LOT.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "ITEM"));
                            row["DEL_FLAG"] = "Y";
                            row["TOUT_STAT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "TOUT_STAT_CODE"));
                            indataSet.Tables["TB_TOUT_LOT"].Rows.Add(row);
                        }
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "STATUS")).Equals("C"))
                        {
                            row = TB_TOUT_LOT.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "ITEM"));
                            row["DEL_FLAG"] = "N";
                            row["TOUT_STAT_CODE"] = "R";
                            indataSet.Tables["TB_TOUT_LOT"].Rows.Add(row);
                        }
                    }
                }
            }

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService_Multi("BR_RTLS_REG_TOUT_REQUEST", "TB_TOUT,TB_TOUT_LOT", "RSLTDT", (bizResult, ex) =>
             {
                 loadingIndicator.Visibility = Visibility.Collapsed;

                 if (ex != null)
                 {
                     Util.MessageException(ex);
                     return;
                 }
                 Util.MessageInfo("SFU1518");
                //수정이냐 신규나에 따라서 초기화 분기
                if (string.IsNullOrEmpty(_reqNo))
                 {
                     dgLotList.ItemsSource = null;
                 }
                else
                 {
                     fn_Tout_Lot_Search();
                 }
                 _reqNo = bizResult.Tables["RSLTDT"].Rows[0]["TOUT_ID"].ToString();
                 this.DialogResult = MessageBoxResult.OK;

             }, indataSet);
        }
        private bool fn_Save_Valid()
        {
            if (_reqNo.Equals(""))
            {
                int iListIndex = Util.gridFindDataRow(ref dgLotList, "CHK", "True", false);


                if (iListIndex == -1)
                {
                    //선택된 Lot이 없습니다.
                    Util.MessageInfo("SFU1261");
                    return false;
                }
            }
            if (string.IsNullOrEmpty(txtApprover.Tag.ToString()))
            {
                //승인자가 필요합니다.
                Util.MessageInfo("SFU1692");
                txtRequestUser.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                //요청 제목을 입력하십시오.
                Util.MessageInfo("RTLS0009");
                txtTitle.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtRemark.Text))
            {
                Util.MessageInfo("RTLS0008");
                txtRemark.Focus();
                return false;
            }
            return true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                fn_Delete_Lot();
            }
            catch (Exception ex)
            {

                Util.AlertError(ex);
            }

        }

        private void fn_Delete_Lot()
        {
            //DataRowView rowview = null;
            //foreach (C1.WPF.DataGrid.DataGridRow row in dgLotList.Rows)
            //{
            //    rowview = row.DataItem as DataRowView;
            //    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True")
            //    {
            //        DataTableConverter.SetValue(row.DataItem, "STATUS", "D");
            //        DataTableConverter.SetValue(row.DataItem, "STATUSNAME", "Delete");
            //        //row.DataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.Black);
            //        row.Presenter.Background = new SolidColorBrush(Colors.Black);
            //        //row.Presenter.Background = new SolidColorBrush(Colors.Black);
            //        row.ce
            //    }
            //}
            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "STATUS", "D");
                    DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "STATUSNAME", "Delete");
                    dgLotList.GetCell(i, dgLotList.Columns["STATUSNAME"].Index).Presenter.Background = new SolidColorBrush(Colors.Gray);


                }
            }
        }

        private void dgLotList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STATUS")) == "D")
                {
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STATUSNAME"].Index).Presenter.Background = new SolidColorBrush(Colors.Gray);
                }
           
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STATUS")) == "C")
                {
                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STATUSNAME"].Index).Presenter.Background = new SolidColorBrush(Colors.LightPink);
                }
            }));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (_reqNo.Equals(""))
                    {
                        Util.gridClear(dgLotList);
                    }
                    else
                    {
                        fn_Tout_Lot_Search();
                    }
                    //Util.gridClear(dgLotList);
                }
            });
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtLot.Text)) return;

                    // Lot 검색
                    fn_LotSearch(txtLot.Text);

                }
            }
            catch (Exception ex)
            {

                Util.Alert(ex.ToString());
            }
        }
    }
}
