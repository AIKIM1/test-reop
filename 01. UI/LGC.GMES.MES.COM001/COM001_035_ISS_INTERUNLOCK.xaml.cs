/*************************************************************************************
 Created Date : 2022.06.10
      Creator : 정규환
   Decription : 출고 인터락 해체 요청(Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.10  염규범S Initial Created.
  2023.02.22 정용석 : 승인요청할때 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_ISS_INTERUNLOCK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        

        private struct MOVE_TYPE
        {
            public const string DOWN = "DOWN";
            public const string UP = "UP";
        }

        public COM001_035_ISS_INTERUNLOCK()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Load Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null && tmps.Length >= 1)
                {
                    
                }

                CommonCodeUser();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            
        }
        #endregion

        #region [승인자 입력]
        private void btnGrator_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void GetUserWindow()
        {
            COM001_035_PACK_SHIP_UNINTERLOCK_PERSON wndPerson = new COM001_035_PACK_SHIP_UNINTERLOCK_PERSON();
            wndPerson.FrameOperation = this.FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtGratorLoss.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                wndPerson.ShowModal();
                wndPerson.CenterOnScreen();
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            COM001_035_PACK_SHIP_UNINTERLOCK_PERSON wndPerson = sender as COM001_035_PACK_SHIP_UNINTERLOCK_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                DataTable dtTo = null;
                dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("APPR_SEQS", typeof(string));
                    dtTo.Columns.Add("USERID", typeof(string));
                    dtTo.Columns.Add("USERNAME", typeof(string));
                    dtTo.Columns.Add("DEPTNAME", typeof(string));
                    dtTo.Columns.Add("CAN_DEL_YN", typeof(string));
                }

                if (dtTo.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
                {
                    Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                    return;
                }

                DataRow drFrom = dtTo.NewRow();
                drFrom["APPR_SEQS"] = dtTo.Rows.Count + 1;
                drFrom["USERID"] = wndPerson.USERID;
                drFrom["USERNAME"] = wndPerson.USERNAME;
                drFrom["DEPTNAME"] = wndPerson.DEPTNAME;
                drFrom["CAN_DEL_YN"] = "Y";
                dtTo.Rows.Add(drFrom);

                dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                txtGratorLoss.Text = string.Empty;
                txtGratorLoss.Tag = string.Empty;
            }
        }

        #endregion

        #region [참조자 입력]
        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNoticeLoss.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);

                        dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtNoticeLoss.Text = string.Empty;
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelect);

                        dgNoticeSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        private void CommonCodeUser()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("USERNAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["USERNAME"] = "PACK_UI_SHIP_INTERUNLOCK_APPR";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIP_INTERUNLOCK_PERSON", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach(DataRow drPerson in dtResult.Rows)
                    {
                        DataTable INDATA = new DataTable();
                        INDATA.Columns.Add("LANGID", typeof(string));
                        INDATA.Columns.Add("AREAID", typeof(string));
                        INDATA.Columns.Add("USERNAME", typeof(string));

                        DataRow drIn = INDATA.NewRow();
                        drIn["LANGID"] = LoginInfo.LANGID;
                        drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                        drIn["USERNAME"] = drPerson["USERID"].ToString();
                        INDATA.Rows.Add(drIn);

                        DataTable dtPerson = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_RELEASE_PERSON", "RQSTDT", "RSLTDT", INDATA);

                        if (dtPerson != null && dtPerson.Rows.Count > 0)
                        {
                            DataTable dtTo = null;
                            dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                            {
                                dtTo.Columns.Add("APPR_SEQS", typeof(string));
                                dtTo.Columns.Add("USERID", typeof(string));
                                dtTo.Columns.Add("USERNAME", typeof(string));
                                dtTo.Columns.Add("DEPTNAME", typeof(string));
                                dtTo.Columns.Add("CAN_DEL_YN", typeof(string));
                            }

                            if (!(dtTo.Select("USERID = '" + drPerson["USERID"].ToString() + "'").Length > 0)) //중복조건 체크
                            {
                                DataRow drFrom = dtTo.NewRow();
                                drFrom["APPR_SEQS"] = dtTo.Rows.Count + 1;
                                drFrom["USERID"] = dtPerson.Rows[0]["USERID"].ToString();
                                drFrom["USERNAME"] = dtPerson.Rows[0]["USERNAME"].ToString();
                                drFrom["DEPTNAME"] = dtPerson.Rows[0]["DEPTNAME"].ToString(); ;
                                drFrom["CAN_DEL_YN"] = "N";
                                dtTo.Rows.Add(drFrom);

                                dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);
                                //Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                                //return;
                            }
                        }
                        else
                        {
                            Util.MessageInfo("SFU8503", (result) =>
                            {
                                if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                                {
                                    this.Close();
                                }
                            });
                        }
                    }
                }
                else
                {
                    Util.MessageInfo("SFU8503", (result) =>
                    {
                        if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                        {
                            this.Close();
                        }
                    });
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [추가(다운)/삭제(업) 버튼]

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            LossLotMove(MOVE_TYPE.DOWN);

        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            LossLotMove(MOVE_TYPE.UP);
        }

        private void LossLotMove(string sMoveArrow)
        {

            if (sMoveArrow == MOVE_TYPE.DOWN)
            {
                if (dgList.ItemsSource == null) return;
                if (dgList.GetRowCount() == 0) return;
            }
            else
            {
                if (dgRequest.ItemsSource == null) return;
                if (dgRequest.GetRowCount() == 0) return;
            }

            DataTable dtTarget = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dgRequest.ItemsSource : dgList.ItemsSource);
            DataTable dtSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dgList.ItemsSource : dgRequest.ItemsSource);
            DataRow newRow = null;

            if (dtTarget.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTarget.Columns.Add("CHK",          typeof(Boolean));
                dtTarget.Columns.Add("LOTID",        typeof(string));
                dtTarget.Columns.Add("LOTSTAT",      typeof(string));
                dtTarget.Columns.Add("AREAID",       typeof(string));
                dtTarget.Columns.Add("PRODID",       typeof(string));
                dtTarget.Columns.Add("EQSGID",       typeof(string));
                dtTarget.Columns.Add("PROCID",       typeof(string));
                dtTarget.Columns.Add("EQPTID",       typeof(string));
                dtTarget.Columns.Add("WOID",         typeof(string));
                dtTarget.Columns.Add("WIPHOLD",      typeof(string));
                dtTarget.Columns.Add("WIPSTAT",      typeof(string));
                dtTarget.Columns.Add("BOXID",        typeof(string));
                dtTarget.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtTarget.Columns.Add("WIPSNAME",     typeof(string));
                dtTarget.Columns.Add("EQPTNAME",     typeof(string));
                dtTarget.Columns.Add("EQSGNAME",     typeof(string));
                dtTarget.Columns.Add("PROCNAME",     typeof(string));
                dtTarget.Columns.Add("AREANAME",     typeof(string));





            }

            for (int i = dtSource.Rows.Count; i > 0; i--)
            {
                if (string.Equals(dtSource.Rows[i - 1]["CHK"].ToString(), "True"))
                {
                    newRow = dtTarget.NewRow();
                    newRow["CHK"]           = false;
                    newRow["LOTID"]         = dtSource.Rows[i - 1]["LOTID"].ToString();
                    newRow["LOTSTAT"]       = dtSource.Rows[i - 1]["LOTSTAT"].ToString();
                    newRow["AREAID"]        = dtSource.Rows[i - 1]["AREAID"].ToString();
                    newRow["PRODID"]        = dtSource.Rows[i - 1]["PRODID"].ToString();
                    newRow["EQSGID"]        = dtSource.Rows[i - 1]["EQSGID"].ToString();
                    newRow["PROCID"]        = dtSource.Rows[i - 1]["PROCID"].ToString();
                    newRow["EQPTID"]        = dtSource.Rows[i - 1]["EQPTID"].ToString();
                    newRow["WOID"]          = dtSource.Rows[i - 1]["WOID"].ToString();
                    newRow["WIPHOLD"]       = dtSource.Rows[i - 1]["WIPHOLD"].ToString();
                    newRow["WIPSTAT"]       = dtSource.Rows[i - 1]["WIPSTAT"].ToString();
                    newRow["BOXID"]         = dtSource.Rows[i - 1]["BOXID"].ToString();
                    newRow["ISS_RSV_FLAG"]  = dtSource.Rows[i - 1]["ISS_RSV_FLAG"].ToString();
                    newRow["WIPSNAME"]      = dtSource.Rows[i - 1]["WIPSNAME"].ToString();
                    newRow["EQPTNAME"]      = dtSource.Rows[i - 1]["EQPTNAME"].ToString();
                    newRow["EQSGNAME"]      = dtSource.Rows[i - 1]["EQSGNAME"].ToString();
                    newRow["PROCNAME"]      = dtSource.Rows[i - 1]["PROCNAME"].ToString();
                    newRow["AREANAME"]      = dtSource.Rows[i - 1]["AREANAME"].ToString();


                    dtTarget.Rows.Add(newRow);

                    dtSource.Rows[i - 1].Delete();
                }
            }

            dgRequest.ItemsSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dtTarget : dtSource);
            dgList.ItemsSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dtSource : dtTarget);

            chkAll_HOLD.IsChecked = false;
            chkAll_REQ.IsChecked = false;

        }
        #endregion

        #region [참조자 검색결과 여러개일경우]
        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);
                dgNoticeSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);

            dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);
            dgNoticeSelect.Visibility = Visibility.Collapsed;

            txtNoticeLoss.Text = string.Empty;
        }
        #endregion

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        #region [제거 처리]
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {
                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                if(dt.Rows[dg.SelectedIndex]["CAN_DEL_YN"].ToString().Equals("N"))
                {
                    Util.MessageInfo("SFU8498");
                    return;
                }

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                DataTable dtGrator = DataTableConverter.Convert(dg.ItemsSource);

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    
                    for (int i = 0; i < dtGrator.Rows.Count; i++)
                    {
                        dtGrator.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);

                    dg.ItemsSource = DataTableConverter.Convert(dtGrator);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region EVENT

        private void dgRequest_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgRequest.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
            }));
        }
        #endregion

        #region [Loss요청클릭]
        private void btnIssInterUnlockReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRequest.GetRowCount() == 0)
                {
                    Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                    return;
                }

                if (dgGrator.GetRowCount() == 0)
                {
                    Util.Alert("SFU1692");  //승인자가 필요합니다.
                    return;
                }

                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                InterUnlockRequest();
                            }
                        });


            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [진행중인 lot 색 변경]
        private void dgListHold_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //진행중인 색 변경
                if (e.Cell.Column.Name.Equals("REQ_ING_CNT"))
                {
                    string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_ING_CNT"));
                    if (sCheck.Equals("ING"))
                    {
                        foreach (C1.WPF.DataGrid.DataGridColumn dc in dataGrid.Columns)
                        {
                            if (dc.Visibility == Visibility.Visible)
                            {
                                if (dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter != null)
                                    dataGrid.GetCell(e.Cell.Row.Index, dc.Index).Presenter.Background = new SolidColorBrush(Colors.LightGray);
                            }
                        }


                        CheckBox cb = dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Content as CheckBox;
                        cb.Visibility = Visibility.Hidden;
                    }
                }


            }));
        }
        #endregion

        #region Mehod

        public void GetInterUnlockLotList(string strLotList)
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inData = new DataTable();
                inData.TableName = "INDATA";

                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("ISS_RSV_FLAG", typeof(string));

                DataRow newInData         = inData.NewRow();
                newInData["LANGID"]       = LoginInfo.LANGID;
                newInData["ISS_RSV_FLAG"] = "Y";

                inData.Rows.Add(newInData);

                DataTable inLot = new DataTable();
                inLot.TableName = "INLOT";

                inLot.Columns.Add("LOTID", typeof(string));

                DataRow newInLot = inLot.NewRow();

                newInLot["LOTID"] = strLotList;

                inLot.Rows.Add(newInLot);

                inDataSet.Tables.Add(inData);
                inDataSet.Tables.Add(inLot);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_SHIP_INTERLOCK", "INDATA,INLOT", "OUTDATA,OUT_ERROR", (bizResult, ex) =>
                {
                    try
                    {
                        txtLotId.Text = "";

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        AddLotList(bizResult.Tables["OUTDATA"]);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                }, inDataSet);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InterUnlockRequest()
        {
            string sTo = "";
            string sCC = "";
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = "REQUEST_SHIPMENT_INTERLOCK";
            row["USERID"] = LoginInfo.USERID;
            row["REQ_NOTE"] = Util.GetCondition(txtLossNote);
            row["AREAID"] = LoginInfo.CFG_AREA_ID; //Modified By Jaeyoung Ko(2019.07.08) [CSR ID:4032652]

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            inLot.Columns.Add("WOID", typeof(string));

            for (int i = 0; i < dgRequest.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID"));
                //row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODELDESC"));
                inLot.Rows.Add(row);
            }

            //승인자
            DataTable inProg = inData.Tables.Add("INPROG");
            inProg.Columns.Add("APPR_SEQS", typeof(string));
            inProg.Columns.Add("APPR_USERID", typeof(string));

            for (int i = 0; i < dgGrator.Rows.Count; i++)
            {
                row = inProg.NewRow();
                row["APPR_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "APPR_SEQS"));
                row["APPR_USERID"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                inProg.Rows.Add(row);

                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
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
                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);
                
                // CSR : C20220802-000459 - 요청한 LOT이 승인요청 진행중에 있는 다른 요청번호에 묶여있을 경우 불건전 LOT Popup 표출
                if (CommonVerify.HasTableInDataSet(ds) && CommonVerify.HasTableRow(ds.Tables["OUTDATA_LOT"]))
                {
                    this.Show_COM001_035_PACK_EXCEPTION_POPUP(ds.Tables["OUTDATA_LOT"]);
                    return;
                }

                if (ds.Tables["OUTDATA"].Rows.Count > 0)
                {
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = ds.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString();

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtLossNote), inLot));
                }

                Util.AlertInfo("SFU1747");  //요청되었습니다.
                SetLossClear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLossClear()
        {
            txtLotId.Text = string.Empty;
            txtGratorLoss.Text = string.Empty;
            txtNoticeLoss.Text = string.Empty;

            chkAll_HOLD.IsChecked = false;
            chkAll_REQ.IsChecked = false;
            Util.gridClear(dgList);
            Util.gridClear(dgRequest);
            //Util.gridClear(dgGrator);
            Util.gridClear(dgNotice);
            ReSetdgGrator();

        }
        #endregion

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter preReq = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll_HOLD = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        CheckBox chkAll_REQ = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    preHold.Content = chkAll_HOLD;
                    e.Column.HeaderPresenter.Content = preHold;
                    chkAll_HOLD.Checked -= new RoutedEventHandler(chkAll_HOLD_Checked);
                    chkAll_HOLD.Unchecked -= new RoutedEventHandler(chkAll_HOLD_Unchecked);
                    chkAll_HOLD.Checked += new RoutedEventHandler(chkAll_HOLD_Checked);
                    chkAll_HOLD.Unchecked += new RoutedEventHandler(chkAll_HOLD_Unchecked);
                }
            }
        }

        private void dgRequest_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Column.Name) == false)
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    preReq.Content = chkAll_REQ;
                    e.Column.HeaderPresenter.Content = preReq;
                    chkAll_REQ.Checked -= new RoutedEventHandler(chkAll_REQ_Checked);
                    chkAll_REQ.Unchecked -= new RoutedEventHandler(chkAll_REQ_Unchecked);
                    chkAll_REQ.Checked += new RoutedEventHandler(chkAll_REQ_Checked);
                    chkAll_REQ.Unchecked += new RoutedEventHandler(chkAll_REQ_Unchecked);
                }
            }
        }

        private void chkAll_HOLD_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            int idgcount = DataTableConverter.Convert(dgList.ItemsSource).Rows.Count;

            // 홀드상태의 경우에도 작업 가능하도록 변경 요청
            for (int i = 0; i < idgcount; i++)
            {    
                if (!(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPSTAT").Equals("TERM") 
                      || DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPSTAT").Equals("MOVING")
                      || DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPSTAT").Equals("BIZWF"))
                    && DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BOXING_YN").Equals("N")
                    && !(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROCID").Equals("PB000") 
                       || DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PROCID").Equals("PD000"))
                   )
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                }
            }
            
        }
        private void chkAll_REQ_Checked(object sender, RoutedEventArgs e)
        {
            if (dgRequest.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRequest.ItemsSource).Table;

            dt.Select("CHK = false").ToList<DataRow>().ForEach(r => r["CHK"] = true);
            dt.AcceptChanges();
        }
        private void chkAll_HOLD_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

        private void chkAll_REQ_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgRequest.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRequest.ItemsSource).Table;

            dt.Select("CHK = true").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            SetLossClear();
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId.Text.Trim() == string.Empty)
                        return;

                    GetInterUnlockLotList(txtLotId.Text.ToString());
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    int maxLOTIDCount = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? 500 : 100;
                    string messageID = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK) ? "SFU8217" : "SFU3695";
                    if (sPasteStrings.Count() > maxLOTIDCount)
                    {
                        if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals(Area_Type.PACK))
                        {
                            Util.MessageValidation(messageID, 500);     // 최대 500개 까지 가능합니다.
                        }
                        else
                        {
                            Util.MessageValidation(messageID);          // 최대 100개 까지 가능합니다.
                        }
                        return;
                    }

                    string lotList = string.Empty;
                    Boolean bdgListChk = true;
                    Boolean bdgRequestChk = true;

                    if(dgList.ItemsSource == null || dgList.GetRowCount() == 0)
                    {
                        bdgListChk = false;
                    }

                    if (dgRequest.ItemsSource == null || dgRequest.GetRowCount() == 0)
                    {
                        bdgRequestChk = false;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        

                        if(bdgListChk && DataTableConverter.Convert(dgList.ItemsSource).Select("LOTID = '" + sPasteStrings[i] + "'") .Length > 0 )
                        {
                            Util.MessageInfo("SFU1384");
                            return;
                        }

                        if (bdgRequestChk && DataTableConverter.Convert(dgRequest.ItemsSource).Select("LOTID = '" + sPasteStrings[i] + "'").Length > 0)
                        {
                            Util.MessageInfo("SFU1384");
                            return;
                        }

                        
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }
                        System.Windows.Forms.Application.DoEvents();
                    }

                    GetInterUnlockLotList(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }
        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        //private void btnCauseProd_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (dgRequest.GetRowCount() == 0)
        //        {
        //            Util.MessageValidation("SFU4940"); // 요청내용이 없습니다.
        //            return;
        //        }

        //        string sProdid = string.Empty;

        //        for (int i = 0; i < dgRequest.GetRowCount(); i++)
        //        {
        //            if (DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "CHK").ToString() == "True" ||
        //                DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "CHK").ToString() == "1")
        //            {
        //                sProdid = DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID").ToString();
        //            }
        //        }

        //        COM001_035_PRODTREE wndPopup = new COM001_035_PRODTREE();
        //        wndPopup.FrameOperation = this.FrameOperation;

        //        if (wndPopup != null)
        //        {
        //            object[] Parameters = new object[2];
        //            Parameters[0] = LoginInfo.CFG_SHOP_ID;
        //            Parameters[1] = sProdid;

        //            C1WindowExtension.SetParameters(wndPopup, Parameters);

        //            wndPopup.Closed += new EventHandler(popup_PRODTREE_Closed);

        //            // 팝업 화면 숨겨지는 문제 수정.
        //            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

        //            wndPopup.BringToFront();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //void popup_PRODTREE_Closed(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        COM001_035_PRODTREE popup = sender as COM001_035_PRODTREE;
        //        if (popup.DialogResult == MessageBoxResult.OK)
        //        {
        //            txtCauseProd.Text = popup.PRODID;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.AlertInfo(ex.Message);
        //    }
        //}

        //private void cboCauseProc_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    try
        //    {
        //        string sLossResnCode = cboLossResnCode.SelectedValue.ToString();
        //        if (string.IsNullOrEmpty(sLossResnCode) || "SELECT".Equals(sLossResnCode))
        //        {
        //            return;
        //        }

        //        string sCauseProc = cboCauseProc.SelectedValue.ToString();
        //        if (string.IsNullOrEmpty(sCauseProc) || "00000".Equals(sCauseProc))
        //        {
        //            //btnCauseProd.IsEnabled = false;
        //            txtCauseProd.Text = string.Empty;
        //            cboCauseEqsg.IsEnabled = false;

        //            return;
        //        }
        //        else
        //        {
        //            //btnCauseProd.IsEnabled = true;
        //            cboCauseEqsg.IsEnabled = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        private void dgList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgList.CurrentRow == null || dgList.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgList.CurrentColumn.Name;
                string sChkValue = string.Empty;

                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgList.CurrentRow.Index;
                    int indexColumn = dgList.CurrentColumn.Index;

                    sChkValue = DataTableConverter.GetValue(dgList.Rows[indexRow].DataItem, sColName).ToString();

                    if (sChkValue == "0" || sChkValue == "False")
                    {
                        DataTableConverter.SetValue(dgList.Rows[indexRow].DataItem, sColName, true);
                    }
                    else if (sChkValue == "1" || sChkValue == "True")
                    {
                        DataTableConverter.SetValue(dgList.Rows[indexRow].DataItem, sColName, false);
                    }
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReSetdgGrator()
        {
            try
            {
                if (dgGrator == null || dgGrator.ItemsSource == null || dgGrator.GetRowCount() <= 0) return;

                DataTable dtGrator = new DataTable();

                dtGrator = DataTableConverter.Convert(dgGrator.ItemsSource);


                for (int i = 0; i < dtGrator.Rows.Count; i++)
                {
                    if (dtGrator.Rows[i]["CAN_DEL_YN"].Equals("Y"))
                    {
                        dtGrator.Rows.Remove(dtGrator.Rows[i]);
                    }
                }

                if (dtGrator == null || dtGrator.Rows.Count <= 0) return;
             
                for (int i = 0; i < dgGrator.Rows.Count; i++)
                {
                    dtGrator.Rows[i]["APPR_SEQS"] = (i + 1);
                }

                Util.GridSetData(dgGrator, dtGrator, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchApprover()
        {
            try
            {

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void AddLotList(DataTable dtRslt = null)
        {
            try
            {
                if (dtRslt == null) return;

                DataTable dtList = DataTableConverter.Convert(dgList.ItemsSource);

                //DataTable dtClone = new DataTable();

                

                foreach (DataRow dr in dtRslt.Rows)
                {

                    DataRow newRow = null;

                    if (dtList.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtList.Columns.Add("CHK", typeof(Boolean));
                        dtList.Columns.Add("LOTID", typeof(string));
                        dtList.Columns.Add("LOTSTAT", typeof(string));
                        dtList.Columns.Add("AREAID", typeof(string));
                        dtList.Columns.Add("PRODID", typeof(string));
                        dtList.Columns.Add("EQSGID", typeof(string));
                        dtList.Columns.Add("PROCID", typeof(string));
                        dtList.Columns.Add("EQPTID", typeof(string));
                        dtList.Columns.Add("WOID", typeof(string));
                        dtList.Columns.Add("WIPHOLD", typeof(string));
                        dtList.Columns.Add("WIPSTAT", typeof(string));
                        dtList.Columns.Add("BOXID", typeof(string));
                        dtList.Columns.Add("ISS_RSV_FLAG", typeof(string));
                        dtList.Columns.Add("WIPSNAME", typeof(string));
                        dtList.Columns.Add("EQPTNAME", typeof(string));
                        dtList.Columns.Add("EQSGNAME", typeof(string));
                        dtList.Columns.Add("PROCNAME", typeof(string));
                        dtList.Columns.Add("AREANAME", typeof(string));
                    }

                    newRow = dtList.NewRow();
                    newRow["CHK"] = false;
                    newRow["LOTID"]         = dr["LOTID"].ToString();
                    newRow["LOTSTAT"]       = dr["LOTSTAT"].ToString();
                    newRow["AREAID"]        = dr["AREAID"].ToString();  
                    newRow["PRODID"]        = dr["PRODID"].ToString();     
                    newRow["EQSGID"]        = dr["EQSGID"].ToString();     
                    newRow["PROCID"]        = dr["PROCID"].ToString();     
                    newRow["EQPTID"]        = dr["EQPTID"].ToString();
                    newRow["WOID"]          = dr["WOID"].ToString();
                    newRow["WIPHOLD"]       = dr["WIPHOLD"].ToString();
                    newRow["WIPSTAT"]       = dr["WIPSTAT"].ToString();
                    newRow["BOXID"]         = dr["BOXID"].ToString();
                    newRow["ISS_RSV_FLAG"]  = dr["ISS_RSV_FLAG"].ToString();
                    newRow["WIPSNAME"]      = dr["WIPSNAME"].ToString();   
                    newRow["EQPTNAME"]      = dr["EQPTNAME"].ToString();   
                    newRow["EQSGNAME"]      = dr["EQSGNAME"].ToString();   
                    newRow["PROCNAME"]      = dr["PROCNAME"].ToString();   
                    newRow["AREANAME"]      = dr["AREANAME"].ToString();

                    dtList.Rows.Add(newRow);
                }

                Util.GridSetData(dgList, dtList, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgList);
        }

        // 불건전 Data 표출 Popup Open
        private void Show_COM001_035_PACK_EXCEPTION_POPUP(DataTable dt)
        {
            COM001_035_PACK_EXCEPTION_POPUP wndPopUp = new COM001_035_PACK_EXCEPTION_POPUP();
            wndPopUp.FrameOperation = FrameOperation;

            if (wndPopUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = "APPR_BIZ";

                C1WindowExtension.SetParameters(wndPopUp, Parameters);
                wndPopUp.ShowModal();
                wndPopUp.CenterOnScreen();
                wndPopUp.BringToFront();
            }
        }
    }    
}

