/*************************************************************************************
 Created Date : 2018.12.26
      Creator : 오화백
   Decription : 자재공급
--------------------------------------------------------------------------------------
 [Change History]
  2018.12.26  DEVELOPER : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Windows.Media;



namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        #endregion


        #region Initialize

        public MCS001_021()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();


                //자재타입
                String[] sFilter1 = { "AGV_SPLY_MTRLCLSS_CODE", LoginInfo.CFG_PROC_ID };
                _combo.SetCombo(cboMtrlType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODEATTR_MCS");


                //요청상태
                String[] sFilter2 = { "MCS_MTRL_SPLY_REQ_STAT_CODE" };
                _combo.SetCombo(cboReqStat, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetResult();
        }
        #endregion

        #region 자재공급 팝업 : btnMtrlReq_Click(), popupReq_Closed()
        /// <summary>
        /// 자재요청 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSupply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation()) return;
                MCS001_021_SUPPLY popupSupply = new MCS001_021_SUPPLY();
                popupSupply.FrameOperation = this.FrameOperation;

                object[] parameters = new object[5];
                parameters[0] = string.Empty;
                parameters[1] = string.Empty;
                parameters[2] = string.Empty;
                parameters[3] = string.Empty;
                parameters[4] = string.Empty;
                C1WindowExtension.SetParameters(popupSupply, parameters);

                popupSupply.Closed += new EventHandler(popupSupply_Closed);
                grdMain.Children.Add(popupSupply);
                popupSupply.BringToFront();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 자재요청 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupSupply_Closed(object sender, EventArgs e)
        {
           
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_021_SUPPLY popupSupply = sender as MCS001_021_SUPPLY;
            if (popupSupply.DialogResult == MessageBoxResult.OK)
            {

                this.grdMain.Children.Remove(popupSupply);
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                GetResult();
            }
            else
            {
                this.grdMain.Children.Remove(popupSupply);
            }
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 스프레드 이벤트 : dgReqList_BeginningEdit()
        /// <summary>
        /// BeginningEdit 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgReqList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (dgReqList.CurrentCell != null && dgReqList.SelectedIndex > -1)
            {
                if (dgReqList.CurrentCell.Column.Name == "CHK")
                {
                    string sStatCode = Util.NVC(dgReqList.GetCell(dgReqList.CurrentRow.Index, dgReqList.Columns["MTRL_SPLY_REQ_STAT_CODE"].Index).Value);
                    if (sStatCode == "CREATED" || sStatCode == "SUPPLY")
                    {
                        e.Cancel = false;   // Editing 가능

                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Mehod
        
        #region 자재요청 조회 : GetResult()
        /// <summary>
        /// 조회
        /// </summary>
        private void GetResult()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            string sMtrlType = cboMtrlType.SelectedValue.ToString();
            string sState = cboReqStat.SelectedValue.ToString();
            if (sMtrlType == "")
            {
                sMtrlType = null;
            }

            if (sState == "")
            {
                sState = null;
            }
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("FROM_DATE", typeof(string));
            RQSTDT.Columns.Add("TO_DATE", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("MTRL_TYPE", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow newRow = RQSTDT.NewRow();
            newRow["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
            newRow["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
            newRow["EQPTID"] = null;
            newRow["STAT_CODE"] = sState;
            newRow["MTRL_TYPE"] = sMtrlType;
            newRow["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_MCS_SEL_MTRL_SUPPLY_REQ", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                try
                {
                    if (exception != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(exception);
                        return;
                    }

                    Util.GridSetData(dgReqList, result, FrameOperation, true);
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

        #endregion

        #region Validation : Validation()
        private bool Validation()
        {

            
            if (dgReqList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgReqList.ItemsSource).Select("CHK = '1'");

            int CheckCount = 0;


            for (int i = 0; i < dgReqList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgReqList.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }

            return true;
        }
        #endregion

        #region LoadingIndicator : ShowLoadingIndicator(), HiddenLoadingIndicator()

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

        #endregion

        private void btnSupplyTaget_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_MTRL_SPLY_REQ_TARGET_FLAG_CELL";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                inDataTable.Columns.Add("TARGET_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                int rowIndex = _Util.GetDataGridCheckFirstRowIndex(dgReqList, "CHK");

                DataRow dr = inDataTable.NewRow();
                dr["MTRL_SPLY_REQ_ID"] = DataTableConverter.GetValue(dgReqList.Rows[rowIndex].DataItem, "MTRL_SPLY_REQ_ID").ToString();
                dr["TARGET_FLAG"] = "Y";
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        GetResult();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgReqList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1DataGrid dg = sender as C1DataGrid;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {

                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString() == "CANCEL")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString() == "SUPPLY")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPLY_TRGT_FLAG").ToString() == "Y")
                            {
                                if (e.Cell.Column.Name.Equals("SPLY_TRGT_FLAG"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }

                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_SPLY_REQ_STAT_CODE").ToString() == "COMPLETED")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Purple);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                       else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPLY_TRGT_FLAG").ToString() == "Y")
                            {
                                if (e.Cell.Column.Name.Equals("SPLY_TRGT_FLAG"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }
}
