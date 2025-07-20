/*************************************************************************************
 Created Date : 2023.05.09
      Creator : 김선준
   Decription : AgingHold Release 요청
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.12   DEVELOPER : Initial Created.
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_AGINGHOLD : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private bool bNew = false;
        private bool bApprover = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_035_REQUEST_AGINGHOLD()
        {
            InitializeComponent(); 
        }
        #endregion
         
        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnClear_Click(this, null);

            //해제사유
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = {"", "ILT_LOT_RELEASE_REASON" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODES", sFilter: sFilter);
            
            object[] tmps = C1WindowExtension.GetParameters(this); 
            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);

                if (_reqNo.Equals("NEW"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("AGINGHOLDRELEASE승인요청");
                    this.bNew = true;
                    this.btnReqCancel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("AGINGHOLDRELEASE승인요청") + string.Format("[{0}]", _reqNo);

                    SetModify();

                    this.bNew = false;
                    this.btnClear.Visibility = Visibility.Collapsed;
                    this.btnReq.Visibility = Visibility.Collapsed;
                    this.btnReturnFileUpload.Visibility = Visibility.Collapsed;
                    this.btnReqCancel.Visibility = Visibility.Visible;
                    this.txtLot.Visibility = Visibility.Collapsed;
                    this.txtApprover.Visibility = Visibility.Collapsed;
                    this.txtNotice.Visibility = Visibility.Collapsed;
                    this.txtNote.IsEnabled = false;
                    this.cboResnCode.IsEnabled = false;

                }
            } 
        }         

        #region [승인자 / 참조자입력]
        /// <summary>
        /// 승인자입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtApprover_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (Keyboard.IsKeyDown(Key.Enter))
                    {
                        this.bApprover = true;
                        GetUserWindow(txtApprover.Text.Trim());
                        e.Handled = true;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        } 

        /// <summary>
        /// 참조자입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (Keyboard.IsKeyDown(Key.Enter))
                    {
                        this.bApprover = false;
                        GetUserWindow(txtNotice.Text.Trim());
                        e.Handled = true;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        /// <summary>
        /// 사용자 조회
        /// </summary>
        /// <param name="sUserID"></param>
        private void GetUserWindow(string sUserID)
        { 
            COM001_035_PACK_PERSON wndPopup = new COM001_035_PACK_PERSON();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] parameters = new object[4];
                if (bApprover)
                {
                    parameters[0] = sUserID;
                    parameters[1] = "APPROVER";
                    C1WindowExtension.SetParameters(wndPopup, parameters);
                    wndPopup.Closed += new EventHandler(wndUser_Closed);
                }
                else
                {
                    parameters[0] = sUserID;
                    parameters[1] = "REFERRER";
                    C1WindowExtension.SetParameters(wndPopup, parameters);
                    wndPopup.Closed += new EventHandler(wndUserNotice_Closed);
                }

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        /// <summary>
        /// 승인자
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            COM001_035_PACK_PERSON wndPerson = sender as COM001_035_PACK_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                DataTable dtAp = DataTableConverter.Convert(dgNotice.ItemsSource);
                if (null != dtAp && dtAp.Rows.Count > 0)
                {
                    if (dtAp.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
                    {
                        Util.MessageValidation("SFU1780");  //이미 추가 된 참조자 입니다.
                        return;
                    }
                }

                DataTable dtTo = DataTableConverter.Convert(dgApprover.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("APPR_SEQS", typeof(string));
                    dtTo.Columns.Add("USERID", typeof(string));
                    dtTo.Columns.Add("USERNAME", typeof(string));
                    dtTo.Columns.Add("DEPTNAME", typeof(string));
                }

                if (dtTo.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
                {
                    Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다. 
                    return;
                }

                if (!ValidationApproval(wndPerson.USERID)) return;

                DataRow drFrom = dtTo.NewRow();
                drFrom["USERID"] = Util.NVC(wndPerson.USERID);
                drFrom["USERNAME"] = Util.NVC(wndPerson.USERNAME);
                drFrom["DEPTNAME"] = Util.NVC(wndPerson.DEPTNAME);

                dtTo.Rows.Add(drFrom);
                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
                }

                Util.GridSetData(dgApprover, dtTo, FrameOperation, true); 

                this.txtApprover.Text = string.Empty;
            }
        }


        /// <summary>
        /// 승인자
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndUserNotice_Closed(object sender, EventArgs e)
        {
            COM001_035_PACK_PERSON wndPerson = sender as COM001_035_PACK_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                DataTable dtAp = DataTableConverter.Convert(dgApprover.ItemsSource);
                if (null != dtAp && dtAp.Rows.Count > 0)
                {
                    if (dtAp.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
                    {
                        Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다. 
                        return;
                    }
                }

                DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("APPR_SEQS", typeof(string));
                    dtTo.Columns.Add("USERID", typeof(string));
                    dtTo.Columns.Add("USERNAME", typeof(string));
                    dtTo.Columns.Add("DEPTNAME", typeof(string));
                }

                if (dtTo.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
                {
                    Util.MessageValidation("SFU1780");  //이미 추가 된 참조자 입니다.
                    return;
                }

                if (!ValidationApproval(wndPerson.USERID)) return;

                DataRow drFrom = dtTo.NewRow();
                drFrom["USERID"] = Util.NVC(wndPerson.USERID);
                drFrom["USERNAME"] = Util.NVC(wndPerson.USERNAME);
                drFrom["DEPTNAME"] = Util.NVC(wndPerson.DEPTNAME);

                dtTo.Rows.Add(drFrom); 

                Util.GridSetData(dgNotice, dtTo, FrameOperation, true);

                this.txtNotice.Text = string.Empty;
            }
        }

        private bool ValidationApproval(string approverId)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTHORITYMENU_BY_ID";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("MENUID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["MENUID"] = "SFU010120160";
            dr["USERID"] = approverId;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ACCESS_COUNT"].GetDecimal() > 0)
                {
                    return true;
                }
                else
                {
                    Util.MessageValidation("SUF4969");  //승인권한이 없는 사용자 입니다.
                    return false;
                }
            }
            else
            {
                Util.MessageValidation("SUF4969");
                return false;
            }

        }
        #endregion

        #region [제거 처리]
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button bt = sender as System.Windows.Controls.Button;
            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;
            try
            {
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;                
                //승인자 차수 정리
                if (dg.Name.Equals("dgApprover"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);

                    Util.GridSetData(dg, dt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [요청취소]
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReqCancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
      
        /// <summary>
        /// LOTID입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtLot.Text.Trim())) return;

                    GetLotNewList(txtLot.Text.Trim());
                    //checkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [요청클릭]
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Request();
                    }
                });                
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                Util.MessageException(ex);
            }
        }
             
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        { 
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;             

            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["STATUS"]).Equals("OK"))
                {
                    row["CHK"] = true;
                }
                else
                {
                    row["CHK"] = false;
                } 
            }
            
            dt.AcceptChanges();
            setCount();
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;
            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
            setCount();
        } 
        #endregion

        #region [진행중인 lot 색 변경]
        private void dgListHold_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            #region 주석  
            //if (!this.bNew) return;
            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            //dataGrid.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.Column;
            //dataGrid.RowHeaderWidth = 0;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null) return;

            //    //진행중인 색 변경
            //    if (e.Cell.Column.Name.Equals("STATUS_NM"))
            //    {
            //        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "STATUS_NM"));
            //        if (sCheck.Contains("[NG]"))
            //        {
            //            foreach (C1.WPF.DataGrid.DataGridColumn dc in dataGrid.Columns)
            //            {
            //                if (dc.Visibility == Visibility.Visible)
            //                {
            //                    if (dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter != null)
            //                    {
            //                        dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                    }
            //                }
            //            }

            //            CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
            //            cb.IsChecked = false;
            //            cb.Visibility = Visibility.Hidden;
            //        }
            //        else
            //        {
            //            CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
            //            cb.IsChecked = true;
            //        }
            //    }
            //}));

            #endregion
        }
        #endregion 

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        { 
            this.txtLot.Text = string.Empty;  
            this.txtApprover.Text = string.Empty;
            this.txtNotice.Text = string.Empty;
            this.txtNote.Text = string.Empty;

            this.cboResnCode.SelectedIndex = 0;
            //this.checkAll.IsChecked = false;

            Util.gridClear(dgListHold); 
            Util.gridClear(dgApprover);
            Util.gridClear(dgNotice);

            this.txtSelCnt.Text = "0";
            this.txtErrCnt.Text = "0";
            this.txtCnt.Text = "0";
        }
        #endregion

        #region [Mehod]

        #region [작업대상 가져오기]
        public void GetLotNewList(string scan)
        {
            if (string.IsNullOrEmpty(scan)) return;

            try
            { 
                #region 변수선언
                DataSet dsResult;
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA"; 
                INDATA.Columns.Add("LANGID", typeof(string)); 
                INDATA.Columns.Add("LOTIDS", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                #endregion //변수선언

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                #region 중복LOT제거
                string sScanLots = scan;
                if (null != this.dgListHold.ItemsSource && this.dgListHold.Rows.Count > 0)
                {
                    #region LOTID로 1차 체크
                    DataTable dtData1 = DataTableConverter.Convert(this.dgListHold.ItemsSource);
                    var scanLists = sScanLots.Split(',').Select(x => new { LOTID = x });
                    var query_lot1 = dtData1.AsEnumerable().ToDictionary(p => p["LOTID"]);
                    var query_scanLots = from lot in scanLists.AsEnumerable()
                                         where query_lot1.ContainsKey(lot.LOTID)
                                         select new { LOTID = lot.LOTID };
                    if (null != query_scanLots && query_scanLots.Count() > 0)
                    {
                        //Lot 이 이미 추가되었습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1196"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                        return;
                    }
                    #endregion
                }
                #endregion //중복LOT제거

                if (!string.IsNullOrEmpty(sScanLots))
                {
                    #region Lot조회 
                    dr["LOTIDS"] = sScanLots;

                    INDATA.Rows.Add(dr);
                    dsInput.Tables.Add(INDATA);

                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_HOLDRACK_LOT", "INDATA", "OUT_LOTLIST,OUTPROG,OUTREF", dsInput, null);

                    if (null != dsResult && dsResult.Tables["OUT_LOTLIST"].Rows.Count > 0)
                    {
                        DataTable dtLot = dsResult.Tables["OUT_LOTLIST"];
                        if (null != this.dgListHold.ItemsSource && this.dgListHold.Rows.Count > 0)
                        {
                            #region 다중Scan 
                            DataTable dtData = DataTableConverter.Convert(this.dgListHold.ItemsSource);
                            #endregion // 다중Scan

                            #region LOTID로 2차 체크
                            if (!sScanLots.Contains(","))
                            {
                                var query_lot1 = dtLot.AsEnumerable().Where(x=>x.Field<string>("STATUS_NM").Contains("[OK]")).ToDictionary(p => p["LOTID"]);
                                var query_scanLots = from lot in dtData.AsEnumerable()
                                                     where query_lot1.ContainsKey(lot.Field<string>("LOTID"))
                                                     select new { LOTID = lot.Field<string>("LOTID") };
                                if (null != query_scanLots && query_scanLots.Count() > 0)
                                {
                                    //Lot 이 이미 추가되었습니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1196"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                    return;
                                }
                            }
                            #endregion //LOTID로 2차 체크
                        }

                        //Error건수 체크
                        int eCnt = dtLot.AsEnumerable().Where(x => x.Field<string>("STATUS_NM").Contains("[NG]")).Count();
                        if (eCnt > 0)
                        {
                            COM001_035_REQUEST_AGINGHOLD_NG wndPopup = new COM001_035_REQUEST_AGINGHOLD_NG();
                            wndPopup.FrameOperation = FrameOperation;

                            if (wndPopup != null)
                            {
                                object[] parameters = new object[1];
                                parameters[0] = dtLot;
                                C1WindowExtension.SetParameters(wndPopup, parameters);
                                wndPopup.Closed += new EventHandler(wndNg_Closed);                                

                                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                            }
                        }
                        else
                        {
                            //Grid Data
                            if (null == this.dgListHold.ItemsSource || this.dgListHold.Rows.Count == 0)
                            {
                                #region 최초Scan 
                                Util.GridSetData(this.dgListHold, dtLot, FrameOperation, true);
                                #endregion
                            }
                            else
                            {
                                //Grid출력
                                DataTable dtData = DataTableConverter.Convert(this.dgListHold.ItemsSource);

                                dtData.Merge(dtLot);
                                Util.GridSetData(this.dgListHold, dtData, FrameOperation, true);
                            }
                            setCount();
                        }                        
                    }
                    #endregion //Lot조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtLot.Clear();
                this.txtLot.Focus();
            }
        }
        /// <summary>
        /// Lot 상태 Count
        /// </summary>
        private void setCount()
        {
            if (null == this.dgListHold.ItemsSource) return;

            DataTable dtData1 = DataTableConverter.Convert(this.dgListHold.ItemsSource);
            this.txtSelCnt.Text = dtData1.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true) || x.Field<bool>("CHK").ToString().ToLower().Equals("true")).Count().ToString();// MES 2.0 CHK 컬럼 Bool 오류 Patch
            this.txtErrCnt.Text = dtData1.AsEnumerable().Where(x => x.Field<string>("STATUS_NM").Contains("[NG]")).Count().ToString();
            this.txtCnt.Text = dtData1.Rows.Count.ToString();
        }

        /// <summary>
        /// OK대상 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndNg_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_AGINGHOLD_NG wndLotOK = sender as COM001_035_REQUEST_AGINGHOLD_NG;
            if (wndLotOK.DialogResult == MessageBoxResult.OK)
            {
                var query = wndLotOK.dtLot.AsEnumerable().Where(x => x.Field<string>("STATUS_NM").Contains("[OK]"));

                if (null != query && query.Count() > 0)
                {
                    DataTable dtOk = query.CopyToDataTable();
                    //Grid Data
                    if (null == this.dgListHold.ItemsSource || this.dgListHold.Rows.Count == 0)
                    {
                        #region 최초Scan 
                        Util.GridSetData(this.dgListHold, dtOk, FrameOperation, true);
                        #endregion
                    }
                    else
                    {
                        //Grid출력
                        DataTable dtData = DataTableConverter.Convert(this.dgListHold.ItemsSource);

                        dtData.Merge(dtOk);
                        Util.GridSetData(this.dgListHold, dtData, FrameOperation, true);
                    }
                    setCount();
                }
            }
        }
        #endregion

        #region [수정시 조회]
        public void SetModify()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_HOLDRACK_LOT", "INDATA", "OUT_LOTLIST,OUTPROG,OUTREF", inData);

                Util.gridClear(this.dgListHold);
                Util.GridSetData(this.dgListHold, dsRslt.Tables["OUT_LOTLIST"], FrameOperation, true);

                Util.gridClear(this.dgApprover);
                Util.GridSetData(this.dgApprover, dsRslt.Tables["OUTPROG"], FrameOperation, true);

                Util.gridClear(dgNotice);
                Util.GridSetData(dgNotice, dsRslt.Tables["OUTREF"], FrameOperation, true);

                txtNote.Text = dsRslt.Tables["OUT_LOTLIST"].Rows[0]["REQ_NOTE"].ToString();

                cboResnCode.SelectedValue = dsRslt.Tables["OUT_LOTLIST"].Rows[0]["RESNCODE"].ToString();
                cboResnCode.IsEditable = false;
                cboResnCode.IsHitTestVisible = false;

                this.txtCnt.Text = dsRslt.Tables["OUT_LOTLIST"].Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        /// <summary>
        /// 해제요청
        /// </summary>
        private void Request()
        {
            string sTo = string.Empty;
            string sCC = string.Empty;

            if (this.dgApprover.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1692");  //승인자가 필요합니다.
                return;
            }
            if (dgListHold.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1748");  //요청 목록이 필요합니다.
                return;
            }
                         
            if (null == this.dgListHold || this.dgListHold.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1748");  //요청 목록이 필요합니다.
                return;
            }            

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string)); 

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = _reqType; 
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["RESNCODE"] = Util.GetCondition(cboResnCode, "SFU1593"); //사유는필수입니다. >> 사유를 선택하세요. 
            if (row["RESNCODE"].Equals("")) return;

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            DataTable dtData = DataTableConverter.Convert(this.dgListHold.ItemsSource);
            foreach (DataRow dr in dtData.Rows)
            { 
                row = inLot.NewRow();                
                row["LOTID"] = dr["LOTID"].ToString();
                row["WIPQTY"] = 1;
                row["PRODID"] = dr["PRODID"].ToString();
                row["PRODNAME"] = dr["PRODNAME"].ToString();
                row["MODELID"] = dr["MODELID"].ToString();
                inLot.Rows.Add(row);
            }
             
            //승인자
            DataTable inProg = inData.Tables.Add("INPROG");
            inProg.Columns.Add("APPR_SEQS", typeof(string));
            inProg.Columns.Add("APPR_USERID", typeof(string));

            for (int i = 0; i < this.dgApprover.Rows.Count; i++)
            {
                row = inProg.NewRow();
                row["APPR_SEQS"] = Util.NVC(DataTableConverter.GetValue(this.dgApprover.Rows[i].DataItem, "APPR_SEQS"));
                row["APPR_USERID"] = Util.NVC(DataTableConverter.GetValue(this.dgApprover.Rows[i].DataItem, "USERID"));
                inProg.Rows.Add(row);

                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(this.dgApprover.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            for (int i = 0; i < dgNotice.Rows.Count; i++)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID"));
                inRef.Rows.Add(row);

                sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);
                if (null != dsRslt && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { 
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }
                Util.MessageValidation("SFU1747");  //요청되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 해제요청취소
        /// </summary>
        private void ReqCancel()
        {
            string sTo = string.Empty;
            string sCC = string.Empty;

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA"); 
            inDataTable.Columns.Add("REQ_NO", typeof(string));
            inDataTable.Columns.Add("APPR_USERID", typeof(string));
            inDataTable.Columns.Add("APPR_RSLT_CODE", typeof(string));
            inDataTable.Columns.Add("APPR_NOTE", typeof(string));
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["REQ_NO"] = _reqNo;
            dr["APPR_USERID"] = LoginInfo.USERID;
            dr["APPR_RSLT_CODE"] = "DEL";
            dr["APPR_NOTE"] = this.txtNote.Text.Trim();
            dr["APPR_BIZ_CODE"] = _reqType;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            inDataTable.Rows.Add(dr);

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR_PACK", "INDATA", "OUTDATA,LOT_INFO", inData);

                if (null != dsRslt && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < this.dgApprover.Rows.Count; i++)
                    {
                        if (i == 0)//최초 승인자만 메일 가도록
                        {
                            sTo = Util.NVC(DataTableConverter.GetValue(this.dgApprover.Rows[i].DataItem, "USERID"));
                        }
                    }

                    //참조자
                    for (int i = 0; i < dgNotice.Rows.Count; i++)
                    {
                        sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
                    }

                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
                    string sTitle = _reqNo + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));
                }
                Util.MessageValidation("SFU1937");  //취소되었습니다.

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
        #endregion

        #region ExcelUpload  
        private void btnReturnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);

                        if (null != dtExcelData && dtExcelData.Rows.Count > 0)
                        {
                            List<string> lotList = dtExcelData.AsEnumerable().Select(x => x[0].ToString()).ToList();
                            string sLotIds = String.Join(",", lotList);

                            Util.gridClear(dgListHold); 
                            this.txtSelCnt.Text = "0";
                            this.txtErrCnt.Text = "0";
                            this.txtCnt.Text = "0";

                            GetLotNewList(sLotIds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        } 
        #endregion //ExcelUpload 
    }
}
