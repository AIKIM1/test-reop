/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_043 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        DataTable _currentData = null;
        private readonly Dictionary<string, string> _pageNum1 = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _pageNum2 = new Dictionary<string, string>();

        #region Declaration & Constructor 



        public BOX001_043()
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
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            InitializeCombo();
            GetSalesOrderInfo();
            //GetEqptWrkInfo();
            cboPageNum1.SelectedValueChanged -= cboPageNum1_SelectedValueChanged;
            cboPageNum2.SelectedValueChanged -= cboPageNum2_SelectedValueChanged;

            nbPrtQty_Pallet.Value = 1;
            InitializePageNumCombo();

            cboPageNum1.SelectedValueChanged += cboPageNum1_SelectedValueChanged;
            cboPageNum2.SelectedValueChanged += cboPageNum2_SelectedValueChanged;
        }

        private void InitializeCombo()
        {

        }
        #endregion


        #region Methods

        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void GetSalesOrderInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHIPMENT_NO");
                RQSTDT.Columns.Add("PO_NO");
                RQSTDT.Columns.Add("SHOPID");
                DataRow inDataRow = RQSTDT.NewRow();
                inDataRow["SHIPMENT_NO"] = txtSalesOrder.Text;
                inDataRow["PO_NO"] = null; //뭐 던져야하는지 확인
                inDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(inDataRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SHIPMENT_INFO", "RQSTDT", "RSLTDT", RQSTDT, (RSLTDT, ex) =>
                 {
                     if (ex != null)
                     {
                         Util.MessageException(ex);
                         return;
                     }
                     Util.GridSetData(dgSalesOrder, RSLTDT, FrameOperation, false);

                 });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void InitializePageNumCombo()
        {
            _pageNum1.Clear();
            _pageNum2.Clear();

            cboPageNum1.SelectedValuePath = cboPageNum2.SelectedValuePath = "Key";
            cboPageNum1.DisplayMemberPath = cboPageNum2.DisplayMemberPath = "Value";

            int pageNum = int.Parse(nbPrtQty_Pallet.Value.ToString());
            if (pageNum > 0)
            {

                //_pageNum1.Add("ALL", "ALL");

                for (int i = 1; i <= pageNum; i++)
                {
                    _pageNum1.Add(i.ToString(), "Page " + i.ToString());
                }

                cboPageNum2.ItemsSource = null;
                cboPageNum2.ItemsSource = _pageNum1;
                cboPageNum2.SelectedIndex = _pageNum1.Count - 1;

                cboPageNum1.ItemsSource = null;
                cboPageNum1.ItemsSource = _pageNum1;
                cboPageNum1.SelectedIndex = 0;

            }

        }

        #endregion

        #region Events
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //Validation 추가
            GetSalesOrderInfo();
        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                /*
                * 2018-09-05 오화백
                * 작업자 정보 저장안하도록 수정
                */
                txtShift.Text = Util.NVC(shiftPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(shiftPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(shiftPopup.USERNAME);
                txtWorker.Tag = Util.NVC(shiftPopup.USERID);


                //GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(shiftPopup);
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = LoginInfo.CFG_EQPT_ID;
                //2018-09-05 오화백 작업자 저장 안되도록 수정
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }
        private void dgSalesOrder_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            //SetMatLabelItems();
            //SetMatLabelPreview();
            if (dgSalesOrder.SelectedItem == null)
                return;
            SetPalletPrivew();
            //GetLabelPrintItem();

        }

        private void SetPalletPrivew()
        {
            try
            {//String.Format("{0:0,0}", 12345.67);       // "12,346"

                txtVendorName.Text = dgSalesOrder.SelectedItem.GetValue("CUSTOMERNAME").ToString();
                txtShipNo.Text = dgSalesOrder.SelectedItem.GetValue("SHIPMENT_NO").ToString();
                txtModel.Text = dgSalesOrder.SelectedItem.GetValue("IEC_PARTNAME").ToString();
                txtRN.Text = dgSalesOrder.SelectedItem.GetValue("PRINT_MODEL_NAME").ToString();
                txtQty.Text = String.Format("{0:0,0}", dgSalesOrder.SelectedItem.GetValue("TOTL_QTY"));
                txtRemark.Text = dgSalesOrder.SelectedItem.GetValue("CUST_MTRLID").ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnShippingNotePrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgSalesOrder.SelectedItem == null)
            {
                Util.MessageValidation("SFU4091"); //S/O 정보를 선택하세요
                return;
            }
            if (cboPageNum1.SelectedItem == null)
            {
                Util.MessageValidation("SFU4095"); //발행 페이지를 선택하세요
                return;
            }
            if (cboPageNum1.SelectedIndex > cboPageNum2.SelectedIndex)
            {
                Util.MessageValidation("SFU4094"); //시작 페이지가 끝 페이지보다 클 수 없습니다.
                return;
            }
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CUSTOMERNAME");
                dt.Columns.Add("SHIPMENT_NO");
                dt.Columns.Add("PRODID");
                dt.Columns.Add("PRINT_MODEL_NAME");
                dt.Columns.Add("TOTL_QTY");
                dt.Columns.Add("PARTNER1");
                dt.Columns.Add("LOGIS1");
                dt.Columns.Add("PARTNER2");
                dt.Columns.Add("LOGIS2");
                dt.Columns.Add("CUSTMTRLID");
                dt.Columns.Add("PageNum");

                DataRow dr = dt.NewRow();
                dr["CUSTOMERNAME"] = txtVendorName.Text;
                dr["SHIPMENT_NO"] = txtShipNo.Text;
                dr["PRODID"] = txtModel.Text;
                dr["PRINT_MODEL_NAME"] = txtRN.Text;
                dr["TOTL_QTY"] = txtQty.Text + "EA";
                dr["PARTNER1"] = txtPartner1.Text;
                dr["LOGIS1"] = txtLogis1.Text;
                dr["PARTNER2"] = txtPartner2.Text;
                dr["LOGIS2"] = txtLogis2.Text;
                dr["CUSTMTRLID"] = txtRemark.Text;
                dr["PageNum"] = txtPageNum.Text;
                dt.Rows.Add(dr);

                BOX001_043_PALLET _print = new BOX001_043_PALLET();
                _print.FrameOperation = this.FrameOperation;

                if (_print != null)
                {
                    // SET PARAMETER
                    object[] Parameters = new object[5];
                    Parameters[0] = dt;
                    Parameters[1] = nbPrtQty_Pallet.Value;
                    Parameters[2] = cboPageNum1.SelectedValue;
                    Parameters[3] = chkShowRemarkYN.IsChecked;
                    Parameters[4] = cboPageNum2.SelectedValue;
                    C1WindowExtension.SetParameters(_print, Parameters);
                    _print.ShowModal();
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }


        }

        private void chkShowRemarkYN_Checked(object sender, RoutedEventArgs e)
        {
            borderRemark.Visibility = Visibility.Visible;
            txtRemark.Visibility = Visibility.Visible;
        }

        private void chkShowRemarkYN_Unchecked(object sender, RoutedEventArgs e)
        {
            borderRemark.Visibility = Visibility.Collapsed;
            txtRemark.Visibility = Visibility.Collapsed;
        }

        private void nbPrtQty_Pallet_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            InitializePageNumCombo();
            txtPageNum.Text = cboPageNum2.SelectedValue.ToString() + " - " + cboPageNum1.SelectedValue.ToString();

        }

        private void cboPageNum1_IsDropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (cboPageNum1.IsDropDownOpen == false)
                return;

            if (cboPageNum1 == null)
                return;

            _pageNum1.Clear();
            cboPageNum1.SelectedValuePath = "Key";
            cboPageNum1.DisplayMemberPath = "Value";

            int pageNum = int.Parse(nbPrtQty_Pallet.Value.ToString());
            if (pageNum > 0)
            {

                // _pageNum.Add("ALL", "ALL");

                for (int i = 1; i <= pageNum; i++)
                {
                    _pageNum1.Add(i.ToString(), "Page " + i.ToString());
                }

                cboPageNum1.ItemsSource = null;
                cboPageNum1.ItemsSource = _pageNum1;
                cboPageNum1.SelectedIndex = 0;
            }
            else
                return;
        }

        private void cboPageNum1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            int startPageNum = int.Parse(cboPageNum1.SelectedValue.ToString());
            txtPageNum.Text = nbPrtQty_Pallet.Value.ToString() + " - " + startPageNum.ToString();
        }
        private void cboPageNum2_IsDropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (cboPageNum2.IsDropDownOpen == false)
                return;

            if (cboPageNum2 == null)
                return;

            _pageNum2.Clear();
            cboPageNum2.SelectedValuePath = "Key";
            cboPageNum2.DisplayMemberPath = "Value";

            //int startPageNum = cboPageNum1.SelectedValue != null ? int.Parse(cboPageNum1.SelectedValue.ToString()) : 1;
            int pageNum = int.Parse(nbPrtQty_Pallet.Value.ToString());
            if (pageNum > 0)
            {

                // _pageNum.Add("ALL", "ALL");

                for (int i = 1; i <= pageNum; i++)
                {
                    _pageNum2.Add(i.ToString(), "Page " + i.ToString());
                }

                cboPageNum2.ItemsSource = null;
                cboPageNum2.ItemsSource = _pageNum2;
                //cboPageNum2.SelectedIndex = 0;
            }
            else
                return;
        }

        private void cboPageNum2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            int startPageNum = int.Parse(cboPageNum1.SelectedValue.ToString());
            int endPageNum = int.Parse(cboPageNum2.SelectedValue.ToString());
            if (startPageNum > endPageNum)
            {
                cboPageNum2.SelectedIndex = cboPageNum1.SelectedIndex;
                return;
            }
        }

        private void dgSalesOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            //dgInpallet.SelectedIndex = idx;
            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }

        private void dgSalesOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                //if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                if (drv != null)
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgSalesOrder.SelectedIndex = idx;
                    SetPalletPrivew();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtSalesOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSalesOrderInfo();
            }
        }
    }


}
