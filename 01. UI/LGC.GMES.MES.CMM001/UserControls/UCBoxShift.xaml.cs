using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using LGC.GMES.MES.Common;
using System;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using System.Reflection;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCBoxShift
    {
        public UserControl UcParentControl;

        public TextBox TextShift { get; set; }
        public TextBox TextShiftDateTime { get; set; }
        public TextBox TextWorker { get; set; }
        public TextBox TextShiftStartTime { get; set; }
        public TextBox TextShiftEndTime { get; set; }
        public TextBox TextLossCnt { get; set; }
        public Button ButtonShift { get; set; }

        public string ProcessCode { get; set; }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public UCBoxShift()
        {
            InitializeComponent();
            SetControl();
        }

        private void SetControl()
        {
            TextShift = txtShift;
            TextWorker = txtWorker;
            ButtonShift = btnShift;
            TextShiftStartTime = txtShiftStartTime;
            TextShiftEndTime = txtShiftEndTime;
            TextShiftDateTime = txtShiftDateTime;
        }


        public void ClearShiftControl()
        {
            txtWorker.Text = string.Empty;
            txtWorker.Tag = string.Empty;
            txtShift.Text = string.Empty;
            txtShift.Tag = string.Empty;
        }

        private void btnSaveUser_Click(object sender, RoutedEventArgs e)
        {
            if (txtWorker.IsReadOnly == true)
                return;
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
                return;
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("USERID");
                dt.Columns.Add("WRK_GR_ID");
                dt.Columns.Add("USE_FLAG");
                dt.Columns.Add("LANGID");
                DataRow dr = null;
                string[] arrID = txtWorker.Text.Split(',');
                for (int i = 0; i < arrID.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(arrID[i]))
                    {
                        txtWorker.Text = txtWorker.Text.Substring(0, txtWorker.Text.Length - 1);
                        continue;
                    }
                    dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PROCID"] = ProcessCode;
                    dr["USERID"] = arrID[i].ToString().Trim();
                    dr["WRK_GR_ID"] = LoginInfo.CFG_AREA_ID;
                    dr["USE_FLAG"] = "Y";
                    dr["LANGID"] = LoginInfo.LANGID;
                    dt.Rows.Add(dr);
                }
                new ClientProxy().ExecuteService("BR_PRD_GET_ACTUSER_NAME_NJ", "RQSTDT", "RSLTDT", dt, (result, ex) => {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        txtWorker.Tag = string.Empty;
                        txtWorker.Text = string.Empty;
                        return;
                    }
                    if (result != null && result.Rows.Count == 1)
                    {
                        txtWorker.Tag = txtWorker.Text;
                        txtWorker.Text = result.Rows[0]["USERNAME"].ToString();
                        txtWorker.IsReadOnly = true;
                    }
                        SelectShft();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            txtWorker.IsReadOnly = false;
            txtShift.Text = string.Empty;
            txtShift.Tag = string.Empty;
            txtShiftDateTime.Text = string.Empty;

            if (txtWorker.Tag != null)
                txtWorker.Text = txtWorker.Tag.ToString();
            txtWorker.Focus();
            txtWorker.SelectAll();

        }

        private void btnResetUser_Click(object sender, RoutedEventArgs e)
        {
            if (txtWorker.IsReadOnly == true)
                return;
            txtShift.Tag = string.Empty;
            txtShift.Text = string.Empty;
            txtWorker.Tag = string.Empty;
            txtWorker.Text = string.Empty;
            txtWorker.Focus();
        }

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtWorker.Text = txtWorker.Text + ",";
                txtWorker.SelectionStart = txtWorker.Text.Length;
            }
        }
        private void SelectShft()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SHOPID");
                dt.Columns.Add("AREAID");
                dt.Columns.Add("EQSGID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("LANGID");
                DataRow dr = dt.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = Process.CELL_BOXING;
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);
                new ClientProxy().ExecuteService_Multi("BR_BAS_SEL_TB_MMD_SHFT_BY_CUR_TIME", "INDATA", "OUTDATA",(result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (result == null || result.Tables.Count < 1 || result.Tables["OUTDATA"].Rows.Count < 1 )
                    {
                        Util.MessageValidation("SFU1646");
                        return;
                    }
                    txtShift.Tag = result.Tables["OUTDATA"].Rows[0]["SHFT_ID"].ToString();
                    txtShift.Text = result.Tables["OUTDATA"].Rows[0]["SHFT_NAME"].ToString();
                    txtShiftStartTime.Text = result.Tables["OUTDATA"].Rows[0]["SHFT_STRT_HMS"].ToString();
                    txtShiftEndTime.Text = result.Tables["OUTDATA"].Rows[0]["SHFT_END_HMS"].ToString();
                    txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                    SetMainWorkerInfo();

                }, ds);
            }
            catch (Exception ex)
            {
                    Util.MessageException(ex);
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_BOX_SHFT shiftPopup = new CMM001.Popup.CMM_BOX_SHFT();

            if (this.UcParentControl != null)
                shiftPopup.FrameOperation = (this.UcParentControl as IWorkArea).FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(TextShift.Tag);
                Parameters[5] = Util.NVC(TextWorker.Tag);
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(shiftPopup);
                        shiftPopup.BringToFront();
                        break;
                    }
                }
            }
        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_BOX_SHFT window = sender as CMM001.Popup.CMM_BOX_SHFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                TextWorker.Text = window.USERNAME;
                TextWorker.Tag = window.USERID;
                TextShift.Text = window.SHFTNAME;
                TextShift.Tag = window.SHFTID;
                TextShiftStartTime.Text = window.SHFT_STRT_HMS;
                TextShiftEndTime.Text = window.SHFT_END_HMS;
                TextShiftDateTime.Text = window.SHFT_STRT_HMS + " ~ " + window.SHFT_END_HMS;
                TextWorker.IsReadOnly = true;
                SetMainWorkerInfo();
            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                }
            }
        }


        protected virtual void SetMainWorkerInfo()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("ChangeShiftByBox");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];
                    string[] workerInfo = new string[4];
                    workerInfo[0] = txtShift.Tag.ToString(); // shftID
                    workerInfo[1] = txtShift.Text; // shftName
                    workerInfo[2] = txtWorker.Tag.ToString(); // workerID
                    workerInfo[3] = txtWorker.Text; // workerName;

                    parameterArrys[0] = workerInfo;

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
