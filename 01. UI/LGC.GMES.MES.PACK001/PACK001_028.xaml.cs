/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.01.28  염규봉 : Pallet / 출하 번호 입력시 Progressbar 활성화 부분 추가




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using System.Windows.Threading;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_028 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        private int isPalletQty = 0;
        private double isCellQty = 0;

        //C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        //{
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        //};

        //CheckBox chkAll = new CheckBox()
        //{
        //    Content = "ALL",
        //    IsChecked = true,
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    VerticalAlignment = System.Windows.VerticalAlignment.Center,
        //    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        //};

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public PACK001_028()
        {
            InitializeComponent();
            Initialize();

            Loaded += BOX001_012_Loaded;
        }

        private void BOX001_012_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_012_Loaded;

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                string sAreaid = ary.GetValue(0).ToString();
                cboAreaAll.SelectedValue = sAreaid;
                if (cboAreaAll.SelectedIndex < 0)
                {
                    cboAreaAll.SelectedIndex = 0;
                }

                this.txtRCVID.Text = ary.GetValue(1).ToString();
                // ScanID에 의한 PALLET 작업이력 조회 함수 호출
                ScanPalletInfo(string.Empty, this.txtRCVID.Text);
            }

        }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);


        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
            //dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            //dtpDateTo.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            // ComboBox 추가 필요
            CommonCombo combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA");

            txtPALLETID.Focus();
            txtPALLETID.SelectAll();
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //((DateTime)dtpDateFrom.SelectedDate).ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
            string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //((DateTime)dtpDateTo.SelectedDate).ToString("yyyyMMdd");  //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

            string sArea = string.Empty;
            string sLot_Type = string.Empty;
            string sLine_ID = string.Empty;

            if (cboAreaAll2.SelectedIndex < 0 || cboAreaAll2.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                sArea = null;
            }
            else
            {
                sArea = cboAreaAll2.SelectedValue.ToString();
            }

            try
            {
                // 조회 비즈 생성
                // 기존 Biz name : QR_GETRELEASE_CANCEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_AREAID"] = sArea;

                if (txtBoxID.Text.Trim() != "")
                {
                    dr["BOXID"] = txtBoxID.Text.Trim();
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;            
                }

                //if (dr["BOXTYPE"].Equals("") ) return;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CANCEL_PACK_SHIP_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgCancelHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgCancelHist, SearchResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCancel);

            Init_Data();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCancel.GetRowCount() > 0)
                {
                    string sArea = string.Empty;

                    // 동 선택 확인
                    if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {                        
                        ms.AlertWarning("SFU1499"); //동을 선택하세요.
                        return;
                    }
                    else
                    {
                        sArea = cboAreaAll.SelectedValue.ToString();
                    }
                    ShowLoadingIndicator();
                    // 출고 취소 처리
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("RCV_ISS_ID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("CNCL_QTY", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["RCV_ISS_ID"] = DataTableConverter.GetValue(dgCancel.Rows[0].DataItem, "RCV_ISS_ID").ToString();
                    row["AREAID"] = sArea;
                    row["CNCL_QTY"] = isCellQty.ToString();
                    row["NOTE"] = "";
                    //row["USERID"] = txtUserID.Text.ToString();
                    row["USERID"] = LoginInfo.USERID;//txtUserID.Tag;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inPallet = indataSet.Tables.Add("INPALLET");
                    inPallet.Columns.Add("BOXID", typeof(string));

                    for (int i = 0; i < dgCancel.GetRowCount(); i++)
                    {
                        DataRow row2 = inPallet.NewRow();
                        row2["BOXID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "BOXID").ToString();

                        indataSet.Tables["INPALLET"].Rows.Add(row2);
                    }

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_SHIP_PACK", "INDATA,INPALLET", null, indataSet);
                                        
                    ms.AlertInfo("SFU1275"); //정상처리되었습니다.

                    Util.gridClear(dgCancel);
                    Init_Data();
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 대상이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3353"); //작업오류 : 취소 대상이 없습니다. [ 출고 대상 PALLET 입력 ]
                    return;
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtPALLETID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanPalletInfo(txtPALLETID.Text,string.Empty);
                txtPALLETID.Clear();
            }
        }
        private void txtRCVID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanPalletInfo(string.Empty, txtRCVID.Text);
                txtRCVID.Clear();
            }
        }

        private void ScanPalletInfo(string sPalletID,string sRcvIssID)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();
                //string sPalletID = string.Empty;
                //string sRcvIssID = string.Empty;
                //sPalletID = txtPALLETID.Text.Trim();
                //sRcvIssID = txtRCVID.Text.Trim();

                if (sPalletID == null && sRcvIssID == null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Pallet ID 가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3350"); //입력오류 : PALLETID 를 입력해 주세요.
                    HiddenLoadingIndicator();
                    return;
                }

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                if (!string.IsNullOrWhiteSpace(sPalletID))
                {
                    dr["BOXID"] = sPalletID;
                }else
                {
                    dr["BOXID"] = null;
                }

                if (!string.IsNullOrWhiteSpace(sRcvIssID))
                {
                    dr["RCV_ISS_ID"] = sRcvIssID;
                }
                else
                {
                    dr["RCV_ISS_ID"] = null;
                }

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_BOX_INFO_FOR_CANCEL_SHIP", "INDATA", "OUTDATA,OUTDATA_RCV", ds);

                if (dsRslt.Tables["OUTDATA"].Rows.Count == 0 )
                {                    
                    ms.AlertWarning("SFU1905"); //조회된 Data가 없습니다.
                    HiddenLoadingIndicator();
                    return;
                }

                if (dgCancel.GetRowCount() != 0)
                {
                    for (int i = 0; i < dgCancel.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "BOXID").ToString() == sPalletID)
                        {                            
                            ms.AlertWarning("SFU1914"); //중복 스캔되었습니다.
                            HiddenLoadingIndicator();
                            return;
                        }

                        if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "RCV_ISS_ID").ToString() == sRcvIssID)
                        {
                            ms.AlertWarning("SFU1914"); //중복 스캔되었습니다.
                            HiddenLoadingIndicator();
                            return;
                        }

                        if (DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "RCV_ISS_ID").ToString() != dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString())
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동일한 출고 ID 만 출고 취소 가능합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            ms.AlertWarning("SFU3014"); //동일한 출고 ID 만 출고 취소 가능합니다.
                            HiddenLoadingIndicator();
                            return;
                        }
                    }

                    dgCancel.IsReadOnly = false;
                    dgCancel.BeginNewRow();
                    dgCancel.EndNewRow(true);
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "BOXID", dsRslt.Tables["OUTDATA"].Rows[0]["BOXID"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "RCV_ISS_ID", dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "PROJECTNAME", dsRslt.Tables["OUTDATA"].Rows[0]["PROJECTNAME"].ToString());
                    DataTableConverter.SetValue(dgCancel.CurrentRow.DataItem, "TOTAL_QTY", dsRslt.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"].ToString());
                    dgCancel.IsReadOnly = true;
                }
                else
                {
                   dgCancel.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA"]);                        
                }

                for (int i = 0; i < dsRslt.Tables["OUTDATA"].Rows.Count; i++)
                {
                    isPalletQty = isPalletQty + 1;
                    //isCellQty = isCellQty + Convert.ToInt32(SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                    isCellQty = isCellQty + Util.NVC_Int(DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "TOTAL_QTY").ToString());
                }
                

                txtPALLET_QTY.Text = isPalletQty.ToString();
                txtTotal_QTY.Text = isCellQty.ToString();
                txtPALLETID.Text = "";
                txtPALLETID.Focus();

            }
            catch (Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            //삭제하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    ShowLoadingIndicator();
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;


                    double dqty = Convert.ToDouble(DataTableConverter.GetValue(dgCancel.Rows[index].DataItem, "TOTAL_QTY").ToString());

                    isPalletQty = isPalletQty - 1;
                    txtPALLET_QTY.Text = isPalletQty.ToString();

                    isCellQty = isCellQty - dqty;
                    txtTotal_QTY.Text = isCellQty.ToString();

                    dgCancel.IsReadOnly = false;
                    dgCancel.RemoveRow(index);
                    dgCancel.IsReadOnly = true;
                    HiddenLoadingIndicator();

                }
            });
        }
        
        #endregion

        #region Mehod

        private void Init_Data()
        {
            isPalletQty = 0;
            isCellQty = 0;

            txtPALLETID.Text = null;
            txtPALLET_QTY.Text = null;
            txtTotal_QTY.Text = null;

            txtPALLETID.Focus();
            txtPALLETID.SelectAll();
        }

        #endregion

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtUserID.Tag = window.USERID;
                txtUserID.Text = window.USERNAME;
            }
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
    }
}
