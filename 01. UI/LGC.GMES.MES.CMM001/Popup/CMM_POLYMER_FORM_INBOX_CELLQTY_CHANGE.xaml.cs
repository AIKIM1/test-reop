/*************************************************************************************
 Created Date : 2017.12.13
      Creator : 정문교
   Decription : 대차 이동공정 선택
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_INBOX_CELLQTY_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
   
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_INBOX_CELLQTY_CHANGE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                 _load = false;

            }

        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            DataRow Inbox = tmps[1] as DataRow;

            if (Inbox == null)
                return;

            DataTable InboxBind = new DataTable();
            InboxBind = Inbox.Table.Clone();
            InboxBind.ImportRow(Inbox);

            Util.GridSetData(dgCart, InboxBind, null, true);

            CommonCombo _combo = new CommonCombo();
            // 변경사유
            String[] sFilterReason = { "MODIFY_WIPQTY" };
            _combo.SetCombo(cboChangeReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilterReason, sCase: "ACTIVITIREASON");

        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod


        #endregion

        #region [Func]
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

        #endregion

        private void txtMQtyChange_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.CheckDecimal(txtMQtyChange.Text, 0))
            {
                txtMQtyChange.Text = string.Empty;
                txtAfterChangeQty.Text = string.Empty;
                txtMQtyChange.Focus();
                return;
            }
            if (dgCart.Rows.Count == 0)
            {
                return;
            }
            if(Convert.ToDouble(txtMQtyChange.Text) > Convert.ToDouble(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CELL_QTY").ToString().Replace(",", "")) )
            {
                Util.MessageValidation("SFU4486");
                txtMQtyChange.Text = string.Empty;
                txtAfterChangeQty.Text = string.Empty;
                return;
            }
            txtAfterChangeQty.Text = String.Format("{0:#,##0}", Convert.ToDouble(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CELL_QTY").ToString().Replace(",", "")) - Convert.ToDouble(txtMQtyChange.Text) );
        }

        private void txtAfterChangeQty_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.CheckDecimal(txtAfterChangeQty.Text, 0))
            {
                txtAfterChangeQty.Text = string.Empty;
                txtMQtyChange.Text = string.Empty;
                txtAfterChangeQty.Focus();
                return;
            }
            if (dgCart.Rows.Count == 0)
            {
                return;
            }
            if (Convert.ToDouble(txtAfterChangeQty.Text) > Convert.ToDouble(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CELL_QTY").ToString().Replace(",", "")))
            {
                Util.MessageValidation("SFU4486");
                txtMQtyChange.Text = string.Empty;
                txtAfterChangeQty.Text = string.Empty;
                return;
            }
            txtMQtyChange.Text = String.Format("{0:#,##0}", Convert.ToDouble(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CELL_QTY").ToString().Replace(",", "")) - Convert.ToDouble(txtAfterChangeQty.Text));
        }

        private void btnChange_qty_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationInbox())
                {
                    return;
                }
                //수량변경하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4177"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                QtyChange();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void QtyChange()
        {
            DataSet inData = new DataSet();

            //마스터 정보

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PALLETID", typeof(string));
            inDataTable.Columns.Add("WIPQTY", typeof(decimal));
            inDataTable.Columns.Add("INBOX_QTY", typeof(decimal));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["PROCID"] = _procID;
            row["PALLETID"] = DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "LOTID").ToString();
            row["WIPQTY"] = Convert.ToDecimal(txtMQtyChange.Text);
            row["RESNCODE"] = cboChangeReason.SelectedValue.ToString();
            row["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(row);


            try
            {
                //Pallet수량변경
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_WIPQTY_PALLET", "INDATA", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.DialogResult = MessageBoxResult.OK;

                    });
                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MODIFY_WIPQTY_PALLE", ex.Message, ex.ToString());

            }
        }
        private bool ValidationInbox()
        {
            //if(Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "WIPQTY_YN")) == "Y")
            //{
            //    Util.MessageValidation("분할 또는 병합되어 수량을 수정할 수 없습니다"); //"분할 또는 병합되어 수량을 수정할 수 없습니다"
            //    return false;
            //}

            if (txtMQtyChange.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU4187"); //변경수량을 입력하세요.
                return false;
            }
            if (txtMQtyChange.Text == "0")
            {
                Util.MessageValidation("SFU4187"); //변경수량을 입력하세요.
                return false;
            }
            if (cboChangeReason.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4189"); //변경사유를 선택하세요.
                return false;
            }


            return true;
        }
    }
}
