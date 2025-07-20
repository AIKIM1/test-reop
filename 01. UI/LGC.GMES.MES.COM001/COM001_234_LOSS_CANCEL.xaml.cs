/*************************************************************************************
 Created Date : 2018.07.27
      Creator : 오화백
   Decription : 불량LOSS 이력 - 불량LOSS 취소
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_234_LOSS_CANCEL : C1Window, IWorkArea
    {
        #region Declaration
      
        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private string _AreaID = string.Empty;
        private DataTable CANCEL_LOSS_DATA = null;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public COM001_234_LOSS_CANCEL()
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
                object[] parameters = C1WindowExtension.GetParameters(this);
                DataTable dtDefect_Loss = parameters[0] as DataTable;
                _AreaID = parameters[1] as string;
                if (dtDefect_Loss == null)
                    return;

                SetGridDefectInfo(dtDefect_Loss);
                dtpDate.SelectedDateTime = GetComSelCalDate();


                //InboxInfo();
                _load = false;
            }

        }
         //선택된 불량 LOSS 셋팅
        private void SetGridDefectInfo(DataTable dt)
        {
            try
            {
               Util.GridSetData(dgDefectLoss, dt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }


        #endregion

        #region 불량 Loss 취소 btnCancel_Click()
        /// <summary>
        ///  Loss 불량 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCancel())
                return;
            // 불량 LOSS 취소하시겠습니까?
            Util.MessageConfirm("SFU4616", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LossCancel();
                }
            });
        }

        #endregion
        
        #region 팝업 닫기 btnClose_Click()
        /// <summary>
        /// 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region 팝업 닫기(x 버튼 클릭시) C1Window_Closing()
        /// <summary>
        /// 팝업닫기 (x 버튼 클릭시) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(this.DialogResult != MessageBoxResult.OK)
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }

        }
        #endregion

        #region 작업자 팝업(텍스트박스 엔터) txtUserName_KeyDown()
        /// <summary>
        /// 작업자 팝업 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        #endregion

        #region 작업자 팝업 (버튼클릭) btnUser_Click()
        /// <summary>
        /// 작업자 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }



        #endregion

        #region 작업자 팝업닫기 wndUser_Closed()
        /// <summary>
        /// 작업자 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        #endregion

        #endregion

        #region User Method

        #region 전기일 셋팅 GetComSelCalDate()
        /// <summary>
        /// 전기일 셋팅
        /// </summary>
        /// <returns></returns>
        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[0].DataItem, "AREAID"));
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }


        #endregion

        #region 불량 LOSS 취소 LossCancel()

        /// <summary>
        ///  Loss 취소
        /// </summary>
        private void LossCancel()
        {
            try
            {
                ShowLoadingIndicator();
                //동일한 LOT, 동일한 불량코드이면 수량을 합침
                SumLossQty();


                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("POSTDATE", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["AREAID"] = _AreaID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WRK_USERID"] = txtUserNameCr.Tag;
                newRow["POSTDATE"] = dtpDate.SelectedDateTime;
                inTable.Rows.Add(newRow);

                DataTable inRESN = inDataSet.Tables.Add("INRESN");
                inRESN.Columns.Add("LOTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(Decimal));
                inRESN.Columns.Add("CTNR_ID", typeof(string));
             
                for (int i = 0; i < CANCEL_LOSS_DATA.Rows.Count; i++)
                {
                    newRow = inRESN.NewRow();
                    newRow["LOTID"] = CANCEL_LOSS_DATA.Rows[i]["LOTID"];
                    newRow["RESNCODE"] = CANCEL_LOSS_DATA.Rows[i]["RESNCODE"];
                    newRow["RESNQTY"] = CANCEL_LOSS_DATA.Rows[i]["RESNQTY"];
                    newRow["CTNR_ID"] = CANCEL_LOSS_DATA.Rows[i]["CTNR_ID"];
                    inRESN.Rows.Add(newRow);
                }

                DataTable inRESN_IDV = inDataSet.Tables.Add("INRESN_IDV");
                inRESN_IDV.Columns.Add("LOTID", typeof(string));
                inRESN_IDV.Columns.Add("HIST_SEQNO", typeof(string));
                inRESN_IDV.Columns.Add("RESNCODE", typeof(string));
                inRESN_IDV.Columns.Add("RESNQTY", typeof(Decimal));
                inRESN_IDV.Columns.Add("RESNQTY2", typeof(Decimal));


                for (int i = 0; i < dgDefectLoss.Rows.Count; i++)
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "INSDTTM")).ToString() != string.Empty)
                    {
                        newRow = inRESN_IDV.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "LOTID")).ToString();
                        newRow["HIST_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "HIST_SEQNO")).ToString();
                        newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "RESNCODE")).ToString();
                        newRow["RESNQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "CANCEL_QTY")).ToString().Replace(",",""));
                        newRow["RESNQTY2"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "CANCEL_QTY")).ToString().Replace(",", ""));
                        inRESN_IDV.Rows.Add(newRow);
                    }
                    
                }


                new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_LOSS_LOT_PC", "INDATA, INRESN,INRESN_IDV", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 작업자 팝업 GetUserWindow()
       /// <summary>
       /// 작업자 팝업
       /// </summary>
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            wndPerson.Width = 600;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUserNameCr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }


            }
        }


        #endregion

        #region Validation 
        private bool ValidateCancel()
        {
            if (txtUserNameCr.Text == string.Empty)
            {
                // 작업자 정보를 입력하세요
                Util.MessageValidation("SFU4201");
                return false;
            }
            if (txtUserNameCr.Tag == null)
            {
                // 작업자 정보를 입력하세요
                Util.MessageValidation("SFU4201");
                return false;
            }

            string ChkCancelQty = string.Empty;
          
            for (int i = 0; i < dgDefectLoss.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "LOTID")) != string.Empty && Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "CANCEL_QTY")) == string.Empty)
                {
                    ChkCancelQty = "Y";
                }


            }

            if (ChkCancelQty == "Y")
            {
                Util.MessageValidation("LOSS취소 수량을 입력하세요");
                return false;
            }
           
            return true;
        }
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

        #endregion

        private void dgDefectLoss_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.Equals("CANCEL_QTY"))
            {
                
                double LossQty = 0;
                double CancelQty = 0;

                LossQty = Convert.ToDouble(Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectLoss.Rows[e.Cell.Row.Index].DataItem, "RESNQTY")));
                CancelQty = Convert.ToDouble(Util.NVC_Decimal(DataTableConverter.GetValue(dgDefectLoss.Rows[e.Cell.Row.Index].DataItem, "CANCEL_QTY")));

                if (LossQty < CancelQty )
                {
                    DataTableConverter.SetValue(dgDefectLoss.Rows[e.Cell.Row.Index].DataItem, "CANCEL_QTY", 0);
                   
                }
                else
                {
                    DataTableConverter.SetValue(dgDefectLoss.Rows[e.Cell.Row.Index].DataItem, "CANCEL_QTY", CancelQty);
                }
                
            }
        }

      

        private void dgDefectLoss_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int rIdx = 0;
                int cIdx = 0;

                C1DataGrid grid = sender as C1DataGrid;

                rIdx = grid.CurrentCell.Row.Index;
                cIdx = grid.CurrentCell.Column.Index;

                if (grid.GetRowCount() > ++rIdx)
                {
                    grid.Selection.Clear();
                    grid.CurrentCell = grid.GetCell(rIdx, cIdx);
                    grid.Selection.Add(grid.GetCell(rIdx, cIdx));

                    if (grid.GetRowCount() - 1 != rIdx)
                    {
                        grid.ScrollIntoView(rIdx + 1, cIdx);
                    }
                }
            }
        }


        private void SumLossQty()
        {
            try
            {
                //조립LOT  계산
                //여러개의 같은데이터를 GROUP BY 
                DataTable LinQ = new DataTable();
                DataRow Linqrow = null;
                LinQ = DataTableConverter.Convert(dgDefectLoss.ItemsSource).Clone();

                for (int i = 0; i < dgDefectLoss.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "LOTID")) != string.Empty)
                    {
                        Linqrow = LinQ.NewRow();
                        Linqrow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "LOTID"));
                        Linqrow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "RESNCODE"));
                        Linqrow["RESNQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "CANCEL_QTY")).Replace(",", ""));
                        Linqrow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLoss.Rows[i].DataItem, "CTNR_ID"));
                        LinQ.Rows.Add(Linqrow);
                    }

                }
                var summarydata = from SUMrow in LinQ.AsEnumerable()
                                  group SUMrow by new
                                  {
                                      LOTID = SUMrow.Field<string>("LOTID")
                                      ,
                                      RESNCODE = SUMrow.Field<string>("RESNCODE")
                                      ,
                                      CTNR_ID = SUMrow.Field<string>("CTNR_ID")
                                  } into grp
                                  select new
                                  {
                                      LOTID = grp.Key.LOTID
                                      ,
                                      RESNCODE = grp.Key.RESNCODE
                                      ,
                                      CTNR_ID = grp.Key.CTNR_ID
                                      ,
                                      RESNQTY = grp.Sum(r => r.Field<decimal>("RESNQTY"))
                                  };


                CANCEL_LOSS_DATA = new DataTable();
                CANCEL_LOSS_DATA = LinQ.Clone();
                foreach (var data in summarydata)
                {
                    DataRow nrow = CANCEL_LOSS_DATA.NewRow();
                    nrow["LOTID"] = data.LOTID;
                    nrow["RESNCODE"] = data.RESNCODE;
                    nrow["RESNQTY"] = data.RESNQTY;
                    nrow["CTNR_ID"] = data.CTNR_ID;
                    CANCEL_LOSS_DATA.Rows.Add(nrow);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }



    }
}
