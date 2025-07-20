using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_092_ABNORMAL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_092_ABNORMAL : C1Window, IWorkArea
    {
        #region Initialize
        public IFrameOperation FrameOperation { get; set; }

        private DataGridCellPresenter cellPresenter = null;

        public COM001_092_ABNORMAL()
        {
            InitializeComponent();
        }

        public DataGridCellPresenter GetCellPresenter
        {
            get { return cellPresenter; }
        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            txtLotId.Text = Util.NVC(tmps[0]);
            cellPresenter = tmps[1] as DataGridCellPresenter;

            SetAbNormalStockData();
        }
        #endregion

        #region Event
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SetUserInfo();
        }

        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetAbNormalLot();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelAbNormalLot();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region Method Function
        private void SetAbNormalStockData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["LOTID"] = txtLotId.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_IN_ABNORMAL_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtComment.Text = Util.NVC(dtResult.Rows[0]["ABNORM_NOTE"]);
                    txtPersonId.Text = Util.NVC(dtResult.Rows[0]["ABNORM_CHARGE_USERID"]);
                    txtPerson.Text = Util.NVC(dtResult.Rows[0]["ABNORM_CHARGE_USERNAME"]);
                    txtPersonDept.Text = Util.NVC(dtResult.Rows[0]["ABNORM_CHARGE_USERDEPT"]);
                }
            }
            catch (Exception ex) {}
        }

        private void SetUserInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["USERNAME"] = txtPerson.Text;
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                }
                else if (dtRslt.Rows.Count == 1)
                {
                    txtPerson.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    txtPersonId.Text = dtRslt.Rows[0]["USERID"].ToString();
                    txtPersonDept.Text = dtRslt.Rows[0]["DEPTNAME"].ToString();
                }
                else
                {
                    dgPersonSelect.Visibility = Visibility.Visible;

                    Util.gridClear(dgPersonSelect);

                    dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                    this.Focusable = true;
                    this.Focus();
                    this.Focusable = false;
                }

            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetAbNormalLot()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("ABNORM_NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inLotDetailDataRow = inDataTable.NewRow();
            inLotDetailDataRow["LOTID"] = txtLotId.Text;
            inLotDetailDataRow["ABNORM_NOTE"] = txtComment.Text;
            inLotDetailDataRow["USERID"] = txtPersonId.Text;
            inDataTable.Rows.Add(inLotDetailDataRow);

            new ClientProxy().ExecuteService("DA_PRD_UPD_STOCK_IN_ABNORMAL_LOT", "INDATA", null, inDataTable, (result, returnEx) =>
            {
                try
                {
                    if (returnEx != null)
                    {
                        Util.MessageException(returnEx);
                        return;
                    }
                    Util.MessageInfo("SFU1270");    //저장되었습니다.

                    // SEND MAIL
                    /*
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, txtPersonId.Text, "", sMsg, txtComment.Text);
                    */
                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void CancelAbNormalLot()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow inLotDetailDataRow = inDataTable.NewRow();
            inLotDetailDataRow["LOTID"] = txtLotId.Text;
            inDataTable.Rows.Add(inLotDetailDataRow);

            new ClientProxy().ExecuteService("DA_PRD_UPD_STOCK_OUT_ABNORMAL_LOT", "INDATA", null, inDataTable, (result, returnEx) =>
            {
                try
                {
                    if (returnEx != null)
                    {
                        Util.MessageException(returnEx);
                        return;
                    }
                    Util.MessageInfo("SFU1937");    //취소되었습니다.

                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }
        #endregion
    }
}
