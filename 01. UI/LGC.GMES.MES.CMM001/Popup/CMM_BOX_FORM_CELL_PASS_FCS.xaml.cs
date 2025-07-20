/*************************************************************************************
 Created Date : 2023.01.12
      Creator : 김린겸
   Decription : Cell Pass FCS 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.12 김린겸 : 최초생성 - CNJ Cell포장 - 1차 포장 구성 (파우치형) / 자동 포장 구성 (파우치형) - 추가기능
  2023.01.31 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins
  2023.02.03 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins
  2023.02.06 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins : Search()... Date 항상 데이터 입력.
  2023.02.08 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins : Cell 입력시 Date 제외. 일자 31일 제한. Cell 6자리 인터락

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
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Threading;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_BOX_FORM_CELL_PASS_FCS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_BOX_FORM_CELL_PASS_FCS : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _USERID = string.Empty;        // LoginInfo.USERID;

        private readonly Util _util = new Util();

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

        public CMM_BOX_FORM_CELL_PASS_FCS()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetCombo();
                SetEvent();
                _load = false;
            }
        }

        private void C1Window_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_load == false)
            {
                DoEvents();
            }
        }

        private void InitializeUserControls()
        {
//            _cartID = string.Empty;

            dgList.AlternatingRowBackground = null;

            dtpDateFrom.SelectedDateTime = DateTime.Today;
            dtpDateTo.SelectedDateTime = DateTime.Today;
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _USERID = tmps[0] as string;

//            txtProcessOut.Text = tmps[1] as string;
//            txtProcessIn.Text = tmps[1] as string;
        }

        private void SetCombo()
        {
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");


            //.//cboCellPass
            DataTable dtCombo = new DataTable();
            dtCombo.TableName = "dtCombo";
            dtCombo.Columns.Add("CBO_CODE", typeof(string));
            dtCombo.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dtCombo.NewRow();
            dr["CBO_CODE"] = "";
            dr["CBO_NAME"] = "-ALL-";
            dtCombo.Rows.Add(dr);

            //dr = dtCombo.NewRow();
            //dr["CBO_CODE"] = "REG_FCS_PASS";
            //dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("등록");
            //dtCombo.Rows.Add(dr);

            //dr = dtCombo.NewRow();
            //dr["CBO_CODE"] = "CANCEL_FCS_PASS";
            //dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("해제");
            //dtCombo.Rows.Add(dr);

            cboCellPass.DisplayMemberPath = "CBO_NAME";
            cboCellPass.SelectedValuePath = "CBO_CODE";

            cboCellPass.ItemsSource = dtCombo.Copy().AsDataView();

            cboCellPass.SelectedValue = "";
            //.//
        }

        private void SetEvent()
        {
            //this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LGCDatePicker dtPik = sender as LGCDatePicker;
                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                DoEvents();
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LGCDatePicker dtPik = sender as LGCDatePicker;
                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                DoEvents();
            }
        }

        #endregion

        #region [Main]

        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column == null)
                return;
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            Search();
        }

        /// <summary>
        /// Cell 등록
        /// </summary>
        private void btnCellRegister_Click(object sender, RoutedEventArgs e)
        {
            CMM_BOX_FORM_CELL_PASS_FCS_REGISTER popup = new CMM_BOX_FORM_CELL_PASS_FCS_REGISTER();
            popup.FrameOperation = this.FrameOperation;
            popup.Name = "CellRegister_Cancel_popup";

            if (popup != null)
            {
                if (ValidationGridAdd(popup.Name) == false)
                    return;

                btnEnable(false);

                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.USERID;
                C1WindowExtension.SetParameters(popup, Parameters);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        popup.Closed += new EventHandler(puCellRegister_Closed);
                        tmp.Children.Add(popup);
                        popup.Focus();
                        popup.BringToFront();
                        break;
                    }
                }
            }
        }

        private void puCellRegister_Closed(object sender, EventArgs e)
        {
            CMM_BOX_FORM_CELL_PASS_FCS_REGISTER popup = sender as CMM_BOX_FORM_CELL_PASS_FCS_REGISTER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);

                    btnEnable(true);

                    break;
                }
            }
        }

        /// <summary>
        /// Cell 해제
        /// </summary>
        private void btnCellCancel_Click(object sender, RoutedEventArgs e)
        {
            CMM_BOX_FORM_CELL_PASS_FCS_CANCEL popup = new CMM_BOX_FORM_CELL_PASS_FCS_CANCEL();
            popup.FrameOperation = this.FrameOperation;
            popup.Name = "CellRegister_Cancel_popup";

            if (popup != null)
            {
                if (ValidationGridAdd(popup.Name) == false)
                    return;

                btnEnable(false);

                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.USERID;
                C1WindowExtension.SetParameters(popup, Parameters);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        popup.Closed += new EventHandler(puCellCancel_Closed);
                        tmp.Children.Add(popup);
                        popup.Focus();
                        popup.BringToFront();
                        break;
                    }
                }
            }
        }

        private void puCellCancel_Closed(object sender, EventArgs e)
        {
            CMM_BOX_FORM_CELL_PASS_FCS_CANCEL popup = sender as CMM_BOX_FORM_CELL_PASS_FCS_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);

                    btnEnable(true);

                    break;
                }
            }
        }

        #endregion

        #region Mehod

        /// <summary>
        /// 인계조회
        /// </summary>
        private void Search()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtCellID.Text))
                {
                    if (txtCellID.Text.Length < 6)
                    {
                        Util.MessageValidation("SFU4342", "6"); //[%1] 자리수 이상 입력하세요.
                        return;
                    }
                }
                else
                {
                    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                    {
                        Util.MessageValidation("SFU2042", "31"); //기간은 %1일 이내 입니다.
                        return;
                    }
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("ACTID_REG", typeof(string));
                RQSTDT.Columns.Add("ACTID_CANCEL", typeof(string));
                RQSTDT.Columns.Add("ACTDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("ACTDTTM_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["ACTID_REG"] = "REG_FCS_PASS";
                dr["ACTID_CANCEL"] = "CANCEL_FCS_PASS";

                if (!string.IsNullOrWhiteSpace(txtCellID.Text))
                {
                    dr["SUBLOTID"] = txtCellID.Text;
                }
                else  //20230206 김린겸, Date 는 무조건 입력되게 else 주석처리 //20230208 다시해제
                {
                    dr["ACTDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00.000");
                    dr["ACTDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59.997");
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOTACTHISTORY_CELL_PASS_FCS", "INDATA", "OUTDATA", RQSTDT);
                //if (!dtResult.Columns.Contains("CHK"))
                //    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgList, dtResult, FrameOperation, false);

                //                DataTable inTable = new DataTable();
                //                inTable.Columns.Add("LANGID", typeof(string));
                //                inTable.Columns.Add("PROCID", typeof(string));
                //                inTable.Columns.Add("EQSGID", typeof(string));
                //                inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                //                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));
                //                inTable.Columns.Add("WIPSTAT", typeof(string));
                //                inTable.Columns.Add("PJT_NAME", typeof(string));
                //                inTable.Columns.Add("PRODID", typeof(string));
                //                inTable.Columns.Add("CTNR_ID", typeof(string));
                //                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                //                inTable.Columns.Add("INBOX_ID", typeof(string));
                //                inTable.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));

                //                // INDATA SET
                //                DataRow newRow = inTable.NewRow();
                //                newRow["LANGID"] = LoginInfo.LANGID;
                ////                newRow["PROCID"] = _procID;
                ////                newRow["EQSGID"] = Util.NVC(cboEquipmentSegmentOut.SelectedItemsToString);
                ////                newRow["WIP_QLTY_TYPE_CODE"] = Util.NVC(cboQltyTypeOut.SelectedValue);
                ////                newRow["CTNR_STAT_CODE"] = Util.NVC(cboCtnrStatOut.SelectedValue);
                ////                newRow["WIPSTAT"] = Util.NVC(cboInboxStatOut.SelectedValue);
                ////                newRow["PJT_NAME"] = txtPjtNameOut.Text;
                ////                newRow["PRODID"] = txtProdIDOut.Text;
                ////                newRow["CTNR_ID"] = txtCtnrIDOut.Text;
                ////                newRow["ASSY_LOTID"] = txtAssyLotIDOut.Text;
                ////                newRow["INBOX_ID"] = txtInboxIDOut.Text;
                ////                newRow["WIP_PRCS_TYPE_CODE"] = Util.NVC(cboWipPrcsTypeOut.SelectedValue);
                //                inTable.Rows.Add(newRow);

                //                //new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CART_TAKEOVER_OUT_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                //                //{
                //                //    try
                //                //    {
                //                        HiddenLoadingIndicator();

                //                //        if (bizException != null)
                //                //        {
                //                //            Util.MessageException(bizException);
                //                //            return;
                //                //        }

                //                //        // 대차의 Inbox수 산출
                //                //        var summarydata = from row in bizResult.AsEnumerable()
                //                //                          group row by new
                //                //                          {
                //                //                              CartID = row.Field<string>("CTNR_ID"),
                //                //                          } into grp
                //                //                          select new
                //                //                          {
                //                //                              CartID = grp.Key.CartID,
                //                //                              CellSum = grp.Sum(r => r.Field<decimal>("CELL_QTY"))

                //                //                          };

                //                //        foreach (var row in summarydata)
                //                //        {
                //                //            bizResult.Select("CTNR_ID = '" + row.CartID + "'").ToList<DataRow>().ForEach(r => r["CART_CELL_QTY"] = row.CellSum);
                //                //        }
                //                //        bizResult.AcceptChanges();

                //                //        Util.GridSetData(dgList, bizResult, FrameOperation, true);

                //                //        // 대차 개수
                //                //        int CtnrCount = bizResult.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;
                //                //        DataGridAggregate.SetAggregateFunctions(dgList.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("대차") + ": " + CtnrCount.ToString("###,###") } });
                //                //    }
                //                //    catch (Exception ex)
                //                //    {
                //                //        HiddenLoadingIndicator();
                //                //        Util.MessageException(ex);
                //                //    }
                //                //});
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        void btnEnable(bool bEnable = true)
        {
            btnCellRegister.IsEnabled = bEnable;
            btnCellCancel.IsEnabled = bEnable;
            btnClose.IsEnabled = bEnable;
            btnSearch.IsEnabled = bEnable;
        }

        #endregion

        #region [Func]

        private bool ValidationSearch()
        {
            //string sArea = Util.GetCondition(cboArea);
            //if (string.IsNullOrWhiteSpace(sArea) || string.IsNullOrEmpty(sArea))
            //{
            //    Util.MessageValidation("SFU5153");  //선택된 Plant/동 정보가 없습니다.
            //    return false;
            //}

            return true;
        }

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
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    foreach (UIElement ui in tmp.Children)
                    {
                        if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                        {
                            // 프로그램이 이미 실행 중 입니다. 
                            Util.MessageValidation("SFU3193");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion

    }
}