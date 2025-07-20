using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_038_RESIDUAL_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _AreaID = string.Empty;

        public int vQty = 0;
        public string inbox1 = string.Empty;
        public string inbox2 = string.Empty;

        private int _PalletCellQty = 0;
        private int qty1 = 0;
        private int qty2 = 0;

        private int _getQty = 0;

        public BOX001_038_RESIDUAL_CELL()
        {
            InitializeComponent();
        }

    

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            string sPalletID = Util.NVC(tmps[0]);
            _PalletCellQty = Int32.Parse(tmps[1].ToString());

            if (string.IsNullOrEmpty(sPalletID))
            {
                Util.Alert("SFU3425");
                return;
            }

            //this.Header = "라벨 발행";

            txtInPallet.Text = sPalletID;
        }
        #endregion



        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private string ValidationPrintLabel()
        {
            if (chkEmpty.IsChecked == true)
            {
                if (txtOKNG1.Text.ToString() != "OK")
                {
                    return "INBOX1";
                }
            }
            else
            {
                if (txtOKNG1.Text.ToString() != "OK")
                {
                    return "INBOX1";
                }

                if (txtOKNG2.Text.ToString() != "OK")
                {
                    return "INBOX2";
                }
            }

            return "";
        }

        private void txtInBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                txtOKNG1.Text = "NG";
                txtOKNG1.Foreground = Brushes.Red;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInBox2_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                txtOKNG2.Text = "NG";
                txtOKNG2.Foreground = Brushes.Red;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnOutboxLabel_Click(object sender, RoutedEventArgs e)
        {

            string sValidationResult = ValidationPrintLabel();

            if (sValidationResult != "")
            {
                Util.Alert("SFU8424", sValidationResult);   //[%1]에 대한 Validation 결과가 OK가 아닙니다.
                return;
            }

            if(txtInBox1.Text == txtInBox2.Text)
            {
                Util.Alert("SFU1508");  //동일한 LOT ID가 있습니다.
                return;
            }

            if (!ValidationInboxQty())
            {
                Util.Alert("SFU4018");  //잔량수량은  Pallet 투입 수량보다 작아야 됩니다.
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4093", "1"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
             {
                 if (result == MessageBoxResult.OK)
                 {
                     inbox1 = txtInBox1.Text.ToString();
                     inbox2 = (bool)chkEmpty.IsChecked ? string.Empty : txtInBox2.Text.ToString();
                     vQty = qty1 + qty2;
                     this.DialogResult = MessageBoxResult.OK;
                 }
             });
        }

        private bool ValidationInboxQty()
        {
            if ((bool)chkEmpty.IsChecked)
            {
                if (_PalletCellQty < qty1)
                {
                    return false;
                }
            }
            else
            {
                if (_PalletCellQty < qty1 + qty2)
                {
                    return false;
                }
            }

            return true;
        }
        private void chkEmpty_Checked(object sender, RoutedEventArgs e)
        {
            txtInBox2.IsEnabled = false;
            txtOKNG2.IsEnabled = false;
        }

        private void chkEmpty_UnChecked(object sender, RoutedEventArgs e)
        {
            txtInBox2.IsEnabled = true;
            txtOKNG2.IsEnabled = true;
        }

        private void txtInBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                return;
            }
            txtOKNG1.Text = "NG";
            txtOKNG1.Foreground = Brushes.Red;

            if (e.Key == Key.Enter)
            {
                if (!ValidationInbox(txtInBox1.Text.ToString()))
                {
                    txtOKNG1.Text = "NG";
                    txtOKNG1.Foreground = Brushes.Red;
                    qty1 = 0;
                }
                else
                {
                    txtOKNG1.Text = "OK";
                    txtOKNG1.Foreground = Brushes.Blue;
                    qty1 = _getQty;
                }
            }
        }
        private void txtInBox2_KeyDown(object sender, KeyEventArgs e)
        {
            txtOKNG2.Text = "NG";
            txtOKNG2.Foreground = Brushes.Red;

            if (e.Key == Key.Enter)
            {
                if (!ValidationInbox(txtInBox2.Text.ToString()))
                {
                    txtOKNG2.Text = "NG";
                    txtOKNG2.Foreground = Brushes.Red;
                    qty2 = 0;
                }
                else
                {
                    txtOKNG2.Text = "OK";
                    txtOKNG2.Foreground = Brushes.Blue;
                    qty2 = _getQty;
                }
            }
        }

        private bool ValidationInbox(string sBoxID)
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable RQSTDT = inDataSet.Tables.Add("INDATA");
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("USERID");
                RQSTDT.Columns.Add("BOXID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                dr["BOXID"] = sBoxID;

                RQSTDT.Rows.Add(dr);

                DataSet RSLTDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_PACKING", "INDATA", "OUTDATA", inDataSet, null);

                if(RSLTDS != null && RSLTDS.Tables.Count > 0 && RSLTDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sOutboxID = Util.NVC(RSLTDS.Tables["OUTDATA"].Rows[0]["OUTER_BOXID"]);
                    string sOutboxStat = Util.NVC(RSLTDS.Tables["OUTDATA"].Rows[0]["OUTER_BOX_BOXSTAT"]);
                    if (!string.IsNullOrEmpty(sOutboxID) && sOutboxStat != "DELETED")
                    {
                        Util.MessageValidation("SFU8149", sOutboxID);  //이미 BOX [%1]에 포장된 LOT입니다
                        return false;
                    }

                    string total_qty = Util.NVC(RSLTDS.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"]);
                    if (string.IsNullOrEmpty(total_qty))
                    {
                        _getQty = 0;
                    }
                    else
                    {
                        _getQty = Int32.Parse(total_qty.Substring(0, total_qty.IndexOf('.')));
                    }
                    return true;
                }
                else
                {
                    Util.MessageValidation("SFU1177");  //포장정보가 없습니다.
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

    }

}
