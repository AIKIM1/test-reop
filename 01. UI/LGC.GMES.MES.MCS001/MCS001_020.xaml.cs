/*************************************************************************************
 Created Date : 2018.12.26
      Creator : 오화백
   Decription : 자재공급 요청
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
using System.Threading;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_020 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        #endregion

        #region Initialize

        public MCS001_020()
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


            txtR_Eqpt.Text = LoginInfo.CFG_EQPT_NAME;
            txtR_Eqpt.Tag = LoginInfo.CFG_EQPT_ID;

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

        #region 자재요청 팝업 : btnMtrlReq_Click(), popupReq_Closed()
        /// <summary>
        /// 자재요청 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMtrlReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_020_REQ popupReq = new MCS001_020_REQ();
                popupReq.FrameOperation = this.FrameOperation;

                object[] parameters = new object[2];
                parameters[0] = txtR_Eqpt.Tag.ToString();  // 설비ID
                parameters[1] = txtR_Eqpt.Text.ToString();  // 설비명
                C1WindowExtension.SetParameters(popupReq, parameters);

                popupReq.Closed += new EventHandler(popupReq_Closed);
                grdMain.Children.Add(popupReq);
                popupReq.BringToFront();

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
        private void popupReq_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_020_REQ popupReq = sender as MCS001_020_REQ;
            if (popupReq.DialogResult == MessageBoxResult.OK)
            {

                this.grdMain.Children.Remove(popupReq);
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                GetResult();
            }
            else
            {
                this.grdMain.Children.Remove(popupReq);
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
                    if (sStatCode == "CREATED")
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

        #region 요청취소 : btnCancelReq_Click()
        /// <summary>
        /// 요청취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancelReq_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;
            //취소하시겠습니까
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU4616"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RequestCancel();

                        }
                    });
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
            newRow["EQPTID"] = txtR_Eqpt.Tag.ToString();
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

        #region 요청취소 : RequestCancel()

        /// <summary>
        ///  요청취소2
        /// </summary>
        private void RequestCancel()
        {
            ShowLoadingIndicator();
            int rowcount = _Util.GetDataGridCheckCnt(dgReqList, "CHK");
            int countqty = 0;
            foreach (DataRow row in ((System.Data.DataView)dgReqList.ItemsSource).Table.Rows)
            {
                if (row["CHK"].ToString() == "1")
                {
                    countqty++;
                    DataSet inData = new DataSet();

                    //INMLOT
                    DataTable inMlot = inData.Tables.Add("INMLOT");
                    inMlot.Columns.Add("MLOTID", typeof(string));
                    DataRow newrow = null;

                    newrow = inMlot.NewRow();
                    newrow["MLOTID"] = null;
                    inMlot.Rows.Add(newrow);

                    //INDATA
                    DataTable inDataTable = inData.Tables.Add("INDATA");
                    inDataTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    inDataTable.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(string));
                    inDataTable.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
                    inDataTable.Columns.Add("UPDUSER", typeof(string));

                    newrow = inDataTable.NewRow();
                    newrow["MTRL_SPLY_REQ_ID"] = row["MTRL_SPLY_REQ_ID"].ToString();  
                    newrow["EQPTID"] = txtR_Eqpt.Tag.ToString();
                    newrow["WO_DETL_ID"] = row["WO_DETL_ID"].ToString();  
                    newrow["MTRLID"] = row["MTRLID"].ToString();
                    newrow["MTRL_SPLY_REQ_QTY"] = Convert.ToDecimal(row["MTRL_SPLY_REQ_QTY"].ToString());
                    newrow["MTRL_ELTR_TYPE_CODE"] = null;
                    newrow["UPDUSER"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(newrow);

                    //TYPE
                    DataTable inType = inData.Tables.Add("INTYPE");
                    inType.Columns.Add("SPLY_TYPE_CODE", typeof(string));

                    newrow = inType.NewRow();
                    newrow["SPLY_TYPE_CODE"] = "CNL";
                    inType.Rows.Add(newrow);

                    try
                    {
                        //자재요청취소
                        new ClientProxy().ExecuteService_Multi("BR_MCS_REG_MTRL_SPLY", "INMLOT,INDATA,INTYPE", "OUTDATA", (Result, ex) =>
                        {
                            if (ex != null)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                                return;
                            }
                        }, inData);

                        if (rowcount == countqty)
                        {
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            GetResult();
                        }

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.AlertByBiz("BR_MCS_REG_MTRL_SPLY", ex.Message, ex.ToString());
                    }
                }
              
            }
          
         
            HiddenLoadingIndicator();

        }









        #endregion

        #endregion
        /// <summary>
        /// 상태에 대한 색깔 변경
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
