using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_143_InOut.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_143_InOut : C1Window, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_143_InOut()
        {
            InitializeComponent();
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtLotId.Focus();
        }
        private void rdo_Checked(object sender, RoutedEventArgs e)
        {
            if((bool)rdoOutput.IsChecked)
            {
                btnOutput.Visibility =  Visibility.Visible;
                btnInput.Visibility = Visibility.Hidden;
            }
            else
            {
                btnOutput.Visibility = Visibility.Hidden;
                btnInput.Visibility = Visibility.Visible;
            }
            Util.gridClear(dgLotList);
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Util.MessageConfirm("SFU3533", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        Save_List();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }
        }

        private void Save_List()
        {
            string status = string.Empty;
            if (dgLotList.Rows.Count == 0) return;
            if ((bool)rdoOutput.IsChecked)
            {
                status = "O";
            }
            else
            {
                status = "I";
            }
            //update 
            DataSet indataSet = new DataSet(); //INDATA, TB_COMMON
            DataTable INDATA = indataSet.Tables.Add("INDATA");
            INDATA.Columns.Add("TOUT_ID", typeof(string));
            INDATA.Columns.Add("LOTID", typeof(string));
            INDATA.Columns.Add("STAT_CODE", typeof(string));


            DataTable TB_COMMON = indataSet.Tables.Add("TB_COMMON");
            TB_COMMON.Columns.Add("INSUSER", typeof(string));
            TB_COMMON.Columns.Add("LANGID", typeof(string));
            TB_COMMON.Rows.Add(LoginInfo.USERID, "N");

            DataRow row = null;
            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK")).Equals("True"))
                {

                    row = INDATA.NewRow();
                    row["TOUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "TOUT_ID"));
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID"));
                    row["STAT_CODE"] = status;
                    INDATA.Rows.Add(row);
                }
            }
            if (INDATA.Rows.Count == 0)
            {
                Util.MessageInfo("SFU1632");
            }
            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService_Multi("BR_RTLS_SET_TOUT_LOT", "INDATA,TB_COMMON", "TB_CHG_ELSID,TB_RTN", (bizResult, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                if(status.Equals("O"))
                {
                    Util.MessageInfo("SFU1518");
                }
                else
                {
                    Util.MessageInfo("SFU1518");
                }

                Util.gridClear(dgLotList);
            }, indataSet);
        }

        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtLotId.Text)) return;

                    if (!fn_DupChk(txtLotId.Text)) return;

                    // Lot 검색
                    fn_LotSearch(txtLotId.Text);

                }
            }
            catch (Exception ex)
            {

                Util.Alert(ex.ToString());
            }
          
        }

        private void fn_LotSearch(string LotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_SEL_TOUT_LOT_REQUEST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
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
                    if (!fn_Vaildation_Lot(dtResult)) return;

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
                            row["LOTID"] = ro["LOTID"].ToString();
                            row["TOUT_ID"] = ro["TOUT_ID"].ToString();
                            dt.Rows.Add(row);
                        }

                        Util.GridSetData(dgLotList, dt, FrameOperation);
                    }
                    else
                    {
                        Util.GridSetData(dgLotList, dtResult, FrameOperation);
                    }
                    txtLotId.Text = string.Empty;
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private bool fn_Vaildation_Lot(DataTable dtResult)
        {

            if(dtResult.Rows[0]["TOUT_ID"].ToString().Equals(""))
            {
                Util.AlertInfo("RTLS0016");
                return false;
            }

            // 불출요청 상태 체크
            switch (dtResult.Rows[0]["TOUT_APPR_STAT_CODE"].ToString())
            {
                case "R":
                    // 불출요청 승인처리가 안되었습니다.
                    Util.AlertInfo(string.Format("REQ_ID: {0} : {1}", dtResult.Rows[0]["TOUT_ID"].ToString(), MessageDic.Instance.GetMessage("RTLS0010")));
                    return false;
                case "E":
                    // 불출요청이 반려된 LOT 입니다.
                    Util.AlertInfo(string.Format("REQ_ID: {0} : {1}", dtResult.Rows[0]["TOUT_ID"].ToString(), MessageDic.Instance.GetMessage("RTLS0011")));
                    return false;
                case "C":
                    // 불출요청 취소된 LOT 입니다.
                    Util.AlertInfo(string.Format("REQ_ID: {0} : {1}", dtResult.Rows[0]["TOUT_ID"].ToString(), MessageDic.Instance.GetMessage("RTLS0012")));
                    return false;

            }
            if ((bool)rdoOutput.IsChecked)
            {

                //LOT 상태 체크
                switch (dtResult.Rows[0]["TOUT_STAT_CODE"].ToString())
                {
                    case "O":
                        //불출 완료된 LOT 입니다.
                        Util.AlertInfo(string.Format("LOTID: {0} : {1}", dtResult.Rows[0]["LOTID"].ToString(), MessageDic.Instance.GetMessage("RTLS0013")));
                        return false;
                    case "I":
                        // 반납 완료된 LOT 입니다.
                        Util.AlertInfo(string.Format("LOTID: {0} : {1}", dtResult.Rows[0]["LOTID"].ToString(), MessageDic.Instance.GetMessage("RTLS0014")));
                        return false;
                }
            }
            else
            {
                //LOT 상태 체크
                switch (dtResult.Rows[0]["TOUT_STAT_CODE"].ToString())
                {
                    case "R":
                        //불출처리가 안된 LOT 입니다.
                        Util.AlertInfo(string.Format("LOTID: {0} : {1}", dtResult.Rows[0]["LOTID"].ToString(), MessageDic.Instance.GetMessage("RTLS0015")));
                        return false;
                    case "I":
                        // 반납 완료된 LOT 입니다..
                        Util.AlertInfo(string.Format("LOTID: {0} : {1}", dtResult.Rows[0]["LOTID"].ToString(), MessageDic.Instance.GetMessage("RTLS0014")));
                        return false;
                }
            }
            return true;
        }

        private bool fn_DupChk(string LotID)
        {
            bool chk = true;
            if (dgLotList.ItemsSource != null)
            {
                DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                var dt_chk = dt.AsEnumerable().Where(x => x["LOTID"].ToString() == LotID);
                if (dt_chk.Count() > 0)
                {
                    //중복 입력되었습니다.
                    Util.MessageValidation("SFU1376", LotID);  //LOT ID는 중복 입력할수 없습니다.(%1)
                    return false;
                }
            }
            return chk;
        }
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(dgLotList);
                }
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = dgLotList.SelectedIndex;
            if (index != -1)
            {
                DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                dt.Rows.RemoveAt(index);
                dt.AcceptChanges();
                Util.GridSetData(dgLotList, dt, FrameOperation);
            }
            else
            {
                //선택된 랏이 없습니다.
                Util.AlertInfo("SFU1632");
            }
        }

        private void btnLotInput_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (string.IsNullOrEmpty(txtLotId.Text)) return;

                if (!fn_DupChk(txtLotId.Text)) return;

                // Lot 검색
                fn_LotSearch(txtLotId.Text);
                fn_Set_Count();
            }
            catch (Exception ex)
            {

                Util.Alert(ex.ToString());
            }
        }

        private void fn_Set_Count()
        {
            //dgLotList
            Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dgLotList.Rows.Count));
        }
    }
}
