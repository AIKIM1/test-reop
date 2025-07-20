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
    /// COM001_035_REQUEST_MODIFY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_035_REQUEST_MODIFY : C1Window, IWorkArea
    {
        #region Initialize
        public IFrameOperation FrameOperation { get; set; }

        public COM001_035_REQUEST_MODIFY()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataTable dt = tmps[0] as DataTable;
            dt.Columns.Add("UPD_FLAG");

            if (dt != null && dt.Rows.Count > 0)
                Util.GridSetData(dgReleaseList, dt, FrameOperation, true);
        }
        #endregion

        #region Event
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
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
                        Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
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

                        Util.GridSetData(dgPersonSelect, dtRslt, FrameOperation, true);
                        this.Focusable = true;
                        this.Focus();
                        this.Focusable = false;
                    }

                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
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

        private void dgReleaseList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    if (dataGrid.CurrentCell.Column.IsReadOnly == true)
                        e.Handled = true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] rows = Util.gridGetChecked(ref dgReleaseList, "CHK");
            if (rows == null || rows.Length == 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            if ( string.IsNullOrEmpty(txtPersonId.Text))
            {
                Util.MessageValidation("SFU4011");  //담당자를 입력 하세요.
                return;
            }

            DataTable dt = ((DataView)dgReleaseList.ItemsSource).Table;
            foreach (DataRow inRow in dt.Rows)
            {
                if (Convert.ToBoolean(inRow["CHK"]) == true)
                {
                    inRow["UNHOLD_SCHDDATE"] = dtExpected.SelectedDateTime.ToString("yyyy-MM-dd");
                    inRow["ACTION_USERNAME"] = Util.NVC(txtPerson.Text);
                    inRow["ACTION_USERID"] = Util.NVC(txtPersonId.Text);
                    inRow["UPD_FLAG"] = "Y";
                }
                inRow["CHK"] = false;
            }
            dgReleaseList.Refresh(false);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3533", (result) =>  //저장 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                    LotChangeHoldHistory();
            });
        }
        #endregion
        #region User Method
        private void LotChangeHoldHistory()
        {
            //'21.05.11 WipActHistory의 WipNote도 UPDATE하기 위한 수정
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("WIPSEQ", typeof(string));
                inData.Columns.Add("HOLD_NOTE", typeof(string));
                inData.Columns.Add("UNHOLD_DATE", typeof(string));
                inData.Columns.Add("ACTION_USER", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("SHOPID", typeof(string));

                DataTable dt = ((DataView)dgReleaseList.ItemsSource).Table;
                DataRow row = null;

                foreach (DataRow inRow in dt.Rows)
                {
                    if (string.Equals(inRow["UPD_FLAG"], "Y"))
                    {
                        row = inData.NewRow();

                        row["LOTID"] = Util.NVC(inRow["LOTID"]);
                        row["WIPSEQ"] = Util.NVC(inRow["WIPSEQ"]);
                        row["HOLD_NOTE"] = Util.NVC(inRow["HOLD_NOTE"]);
                        row["UNHOLD_DATE"] = Convert.ToDateTime(inRow["UNHOLD_SCHDDATE"]).ToString("yyyyMMdd");
                        row["ACTION_USER"] = Util.NVC(inRow["ACTION_USERID"]);
                        //param 추가
                        row["USERID"] = LoginInfo.USERID;
                        row["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                        indataSet.Tables["INDATA"].Rows.Add(row);
                    }
                }

                //DA -> BR로 수정
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_HOLD_LOT_HIST", "INDATA", null, indataSet);

                this.DialogResult = MessageBoxResult.OK;
            }
            catch(Exception ex) { Util.MessageException(ex); }
        }
        #endregion
    }
}
