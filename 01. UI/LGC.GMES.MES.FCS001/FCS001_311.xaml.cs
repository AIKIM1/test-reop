/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.15  LEEHJ     : 소형활성화MES 복사
  2023.07.04  조영대    : FCS002_311 => FCS001_311 복사
  2024.05.28  조영대    : GroupBy 제거.
  2024.09.09  조영대    : LOTID 조회조건 추가, 이력 Tab의 Lot ID => Tray Lot ID 로 수정
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_311 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS001_311()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnBizWFLotRequest);
            listAuth.Add(btnBizWFLotCancelRequest);
            listAuth.Add(btnRequestScrapYield);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            InitCombo();

            InitControls();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase:"AREA");

            //요청구분
            cboReqType.SetCommonCode("APPR_BIZ_CODE", "ATTR2='Y'", CommonCombo.ComboStatus.ALL, false);
            cboReqTypeHist.SetCommonCode("APPR_BIZ_CODE", "ATTR2='Y'", CommonCombo.ComboStatus.ALL, false);

            //상태
            cboReqRslt.SetCommonCode("REQ_RSLT_CODE", CommonCombo.ComboStatus.ALL, false);
            cboReqRsltHist.SetCommonCode("REQ_RSLT_CODE", CommonCombo.ComboStatus.ALL, false);

            //ERP 상태
            cboErpState.SetCommonCode("BIZ_WF_REQ_DOC_STAT_CODE", CommonCombo.ComboStatus.ALL, false);
        }

        private void InitControls()
        {
            dtpSearchDate.SelectedToDateTime = DateTime.Today;
            dtpSearchDate.SelectedFromDateTime = DateTime.Today.AddDays(-7);
            dtpSearchDateHist.SelectedToDateTime = DateTime.Today;
            dtpSearchDateHist.SelectedFromDateTime = DateTime.Today.AddDays(-7);
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {            
            GetList();
        }

        private void btnRequestScrapYield_Click(object sender, RoutedEventArgs e) 
        {
            //수율반영폐기
            FCS001_311_REQUEST_YIELD wndPopup = new FCS001_311_REQUEST_YIELD();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = "LOT_SCRAP_YIELD";
                Parameters[2] = Util.GetCondition(cboArea, bAllNull: true);

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupYield_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopupYield_Closed(object sender, EventArgs e)
        {
            FCS001_311_REQUEST_YIELD window = sender as FCS001_311_REQUEST_YIELD;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        private void btnBizWFLotRequest_Click(object sender, RoutedEventArgs e)
        {
            FCS001_311_REQUEST_BIZWFLOT wndPopup = new FCS001_311_REQUEST_BIZWFLOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "NEW";
                Parameters[1] = string.Empty;
                Parameters[2] = "REQUEST_BIZWF_LOT";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnBizWFLotCancelRequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.GetRowCount() == 0 || !dgList.IsCheckedRow("CHK"))
            {
                Util.Alert("SFU1636");  //선택된 대상이 없습니다.
                return;
            }

            FCS001_311_REQUEST_BIZWFLOT wndPopup = new FCS001_311_REQUEST_BIZWFLOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = "NEW";
                Parameters[1] = dgList.GetValue("REQ_NO").Nvc();
                Parameters[2] = "REQUEST_CANCEL_BIZWF_LOT";

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopupBizWFLot_Closed(object sender, EventArgs e)
        {
            FCS001_311_REQUEST_BIZWFLOT window = sender as FCS001_311_REQUEST_BIZWFLOT;
            
            if (window.DialogResult.Equals(MessageBoxResult.OK)) GetList();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow.Type != DataGridRowType.Item) return;

            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("REQ_NO") && dgList.GetRowCount() > 0)
            {
                if (dgList.GetValue("REQ_USER_ID").Nvc().Equals(LoginInfo.USERID) && 
                    dgList.GetValue("REQ_RSLT_CODE").Nvc().Equals("REQ"))
                {
                    if (dgList.GetValue("APPR_BIZ_CODE").Nvc().Equals("LOT_REQ"))
                    {
                        //FCS001_311_REQUEST1 wndPopup = new FCS001_311_REQUEST1();
                        //wndPopup.FrameOperation = FrameOperation;

                        //if (wndPopup != null)
                        //{
                        //    object[] Parameters = new object[4];
                        //    Parameters[0] = dgList.GetValue("REQ_NO").Nvc();
                        //    Parameters[1] = dgList.GetValue("APPR_BIZ_CODE").Nvc;

                        //    C1WindowExtension.SetParameters(wndPopup, Parameters);

                        //    wndPopup.Closed += new EventHandler(wndPopup1_Closed);

                        //    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        //}
                    }
                    else if (dgList.GetValue("APPR_BIZ_CODE").Nvc().Equals("LOT_REQ_HOT"))
                    {
                        //FCS001_311_REQUEST_HOT wndPopup = new FCS001_311_REQUEST_HOT();
                        //wndPopup.FrameOperation = FrameOperation;

                        //if (wndPopup != null)
                        //{
                        //    object[] Parameters = new object[2];
                        //    Parameters[0] = dgList.GetValue("REQ_NO").Nvc();
                        //    Parameters[1] = dgList.GetValue("APPR_BIZ_CODE").Nvc();

                        //    C1WindowExtension.SetParameters(wndPopup, Parameters);

                        //    wndPopup.Closed += new EventHandler(wndPopupHot_Closed);

                        //    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        //}
                    }
                    else if (dgList.GetValue("APPR_BIZ_CODE").Nvc().Equals("REQUEST_BIZWF_LOT"))
                    {
                        FCS001_311_REQUEST_BIZWFLOT wndPopup = new FCS001_311_REQUEST_BIZWFLOT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[3];
                            Parameters[0] = "MODIFY";
                            Parameters[1] = dgList.GetValue("REQ_NO").Nvc();
                            Parameters[2] = dgList.GetValue("APPR_BIZ_CODE").Nvc();

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else if (dgList.GetValue("APPR_BIZ_CODE").Nvc().Equals("REQUEST_CANCEL_BIZWF_LOT"))
                    {
                        FCS001_311_REQUEST_BIZWFLOT wndPopup = new FCS001_311_REQUEST_BIZWFLOT();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[3];
                            Parameters[0] = "MODIFY";
                            Parameters[1] = dgList.GetValue("REQ_NO").Nvc();
                            Parameters[2] = dgList.GetValue("APPR_BIZ_CODE").Nvc();

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else if (dgList.GetValue("APPR_BIZ_CODE").Nvc().Equals("LOT_SCRAP_YIELD"))
                    {
                        FCS001_311_REQUEST_YIELD wndPopup = new FCS001_311_REQUEST_YIELD();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[4];
                            Parameters[0] = dgList.GetValue("REQ_NO").Nvc();
                            Parameters[1] = "LOT_SCRAP_YIELD";
                            Parameters[2] = Util.GetCondition(cboArea, bAllNull: true);

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopupYield_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    else
                    {
                        //FCS001_311_REQUEST wndPopup = new FCS001_311_REQUEST();
                        //wndPopup.FrameOperation = FrameOperation;

                        //if (wndPopup != null)
                        //{
                        //    object[] Parameters = new object[4];
                        //    Parameters[0] = dgList.GetValue("REQ_NO").Nvc();
                        //    Parameters[1] = dgList.GetValue("APPR_BIZ_CODE").Nvc();

                        //    C1WindowExtension.SetParameters(wndPopup, Parameters);

                        //    wndPopup.Closed += new EventHandler(wndPopup_Closed);

                        //    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        //}
                    }
                }
                else
                {
                    FCS001_311_READ wndPopup = new FCS001_311_READ();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[4];
                        Parameters[0] = dgList.GetValue("REQ_NO").Nvc();
                        Parameters[1] = dgList.GetValue("REQ_RSLT_CODE").Nvc();
                        Parameters[2] = dgList.GetValue("APPR_BIZ_CODE").Nvc();
                        Parameters[3] = dgList.GetValue("APPR_NAME").Nvc();

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
            }
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Column.Name.Equals("REQ_NO"))
                {
                    e.Cell.Presenter.Foreground = Brushes.Blue;
                }
                else
                {
                    e.Cell.Presenter.Foreground = Brushes.Black;
                }
            }));
        }

        private void dgList_RowIndexChanged(object sender, int beforeRow, int currentRow)
        {
            SetRowCheck(currentRow);
                        
            if (dgList.Rows[currentRow].Type == DataGridRowType.Item &&
                dgList.GetValue("APPR_BIZ_CODE").Equals("REQUEST_BIZWF_LOT") &&
                dgList.GetValue("REQ_RSLT_CODE").Equals("END"))
            {
                if (dgList.GetValue("BIZ_WF_REQ_DOC_STAT_CODE").Nvc().Equals("2") || // 결재진행중
                    dgList.GetValue("BIZ_WF_REQ_DOC_STAT_CODE").Nvc().Equals("3") || // 결재승인
                    dgList.GetValue("BIZ_WF_REQ_DOC_STAT_CODE").Nvc().Equals("6"))   // 출고완료
                {
                    btnBizWFLotCancelRequest.IsEnabled = false;
                }
                else
                {
                    btnBizWFLotCancelRequest.IsEnabled = true;
                }
            }
            else
            {
                btnBizWFLotCancelRequest.IsEnabled = false;
            }           
        }

        private void dgListCheck_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rdoButton = sender as RadioButton;
            C1.WPF.DataGrid.DataGridCellPresenter dgcp = rdoButton.Parent as C1.WPF.DataGrid.DataGridCellPresenter;

            dgList.SelectCell(dgcp.Row.Index, dgList.Columns["CHK"].Index);

            if (dgList.GetValue(dgcp.Row.Index, "APPR_BIZ_CODE").Equals("REQUEST_BIZWF_LOT") &&
                dgList.GetValue(dgcp.Row.Index, "REQ_RSLT_CODE").Equals("END"))
            {
                if (dgList.GetValue("BIZ_WF_REQ_DOC_STAT_CODE").Nvc().Equals("2") || // 결재진행중
                    dgList.GetValue("BIZ_WF_REQ_DOC_STAT_CODE").Nvc().Equals("3") || // 결재승인
                    dgList.GetValue("BIZ_WF_REQ_DOC_STAT_CODE").Nvc().Equals("6"))   // 출고완료
                {
                    btnBizWFLotCancelRequest.IsEnabled = false;
                }
                else
                {
                    btnBizWFLotCancelRequest.IsEnabled = true;
                }
            }
            else
            {
                btnBizWFLotCancelRequest.IsEnabled = false;
            }
        }

        private void dgList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (e.Exception == null)
            {
                SetGroupView(chkGroupView.IsChecked.Equals(true));
            }

            btnSearch.IsEnabled = true;
        }

        private void chkGroupView_CheckedChanged(object sender, CMM001.Controls.UcBaseCheckBox.CheckedChangedEventArgs e)
        {
            SetGroupView(chkGroupView.IsChecked.Equals(true));
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        private void dgListHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgListHist.CurrentRow.Type != DataGridRowType.Item) return;

            if (dgListHist.CurrentRow != null && dgListHist.CurrentColumn.Name.Equals("SUBLOTID") && dgListHist.GetRowCount() > 0)
            {

                FCS001_311_READ wndPopup = new FCS001_311_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = dgListHist.GetValue("REQ_NO").Nvc();
                    Parameters[1] = dgListHist.GetValue("REQ_RSLT_CODE").Nvc();
                    Parameters[2] = dgListHist.GetValue("APPR_BIZ_CODE").Nvc();
                    Parameters[3] = dgListHist.GetValue("APPR_NAME").Nvc();

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

        private void dgListHist_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgListHist.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("SUBLOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }

        private void dgListHist_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearchHist.IsEnabled = true;
        }

        #endregion


        #region Mehod

        private void GetList()
        {
            try
            {
                this.ClearValidation();
                btnBizWFLotCancelRequest.IsEnabled = false;

                if (cboArea.GetBindValue() == null)
                {
                    //동은필수입니다.
                    cboArea.SetValidation("SFU3203");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("BIZ_WF_REQ_DOC_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (txtCellID.GetBindValue() == null  && txtLotID.GetBindValue() == null) //CellId, LotId 가 없는 경우
                {
                    dr["AREAID"] = cboArea.GetBindValue();
                    dr["FROM_DATE"] = dtpSearchDate.SelectedFromDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_DATE"] = dtpSearchDate.SelectedToDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["USERNAME"] = txtReqUser.GetBindValue();
                    dr["APPR_BIZ_CODE"] = cboReqType.GetBindValue();
                    dr["REQ_RSLT_CODE"] = cboReqRslt.GetBindValue();
                    dr["BIZ_WF_REQ_DOC_STAT_CODE"] = cboErpState.GetBindValue();
                    dr["CSTID"] = txtCSTID.GetBindValue();
                }
                else //Cell ID, Lot ID 가 있는경우 다른 조건 모두 무시
                {
                    dr["SUBLOTID"] = txtCellID.GetBindValue();
                    dr["LOTID"] = txtLotID.GetBindValue();
                }
                dtRqst.Rows.Add(dr);

                btnSearch.IsEnabled = false;
                dgList.ExecuteService("DA_SEL_APPROVAL_REQ_LIST", "INDATA", "OUTDATA", dtRqst);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListHist()
        {
            try
            {
                this.ClearValidation();
                
                if (cboAreaHist.GetBindValue() == null)
                {
                    //동은필수입니다.
                    cboAreaHist.SetValidation("SFU3203");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (txtCellIDHist.GetBindValue() == null) //lot id 가 없는 경우
                {
                    dr["AREAID"] = cboAreaHist.GetBindValue();                    
                    dr["FROM_DATE"] = dtpSearchDateHist.SelectedFromDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_DATE"] = dtpSearchDateHist.SelectedToDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["USERNAME"] = txtReqUserHist.GetBindValue();
                    dr["APPR_BIZ_CODE"] = cboReqTypeHist.GetBindValue();
                    dr["REQ_RSLT_CODE"] = cboReqRsltHist.GetBindValue();
                    dr["PRODID"] = txtProdID.GetBindValue();
                    dr["CSTID"] = txtCSTIDHist.GetBindValue();
                }
                else //Cell Id 가 있는경우 다른 조건 모두 무시
                {
                    dr["SUBLOTID"] = txtCellIDHist.GetBindValue();
                }
                dtRqst.Rows.Add(dr);

                btnSearchHist.IsEnabled = false;
                dgListHist.ExecuteService("DA_SEL_APPROVAL_REQ_HIST", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRowCheck(int row)
        {            
            DataTable dt = dgList.GetDataTable(false);
            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);

            dgList.SetValue(row, "CHK", 1);
        }

        private void SetGroupView(bool isGroup)
        {
            if (chkGroupView.IsChecked.Equals(true))
            {
                dgList.Columns["BIZ_WF_REQ_DOC_NO2"].Visibility = Visibility.Collapsed;
                dgList.GroupBy(dgList.Columns["BIZ_WF_REQ_DOC_NO"]);
            }
            else
            {
                dgList.Columns["BIZ_WF_REQ_DOC_NO2"].Visibility = Visibility.Visible;
                DataTable dt = dgList.GetDataTable();
                dgList.ItemsSource = dt.DefaultView;
            }
        }

        #endregion
    }
}
