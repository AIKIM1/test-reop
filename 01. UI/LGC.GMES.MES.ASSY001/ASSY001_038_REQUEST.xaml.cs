/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.20  정용석 : 결재요청 순서도 OUTPUT Parameter 추가로 인한 수정




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_038_REQUEST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string lotid;
        DateTime dCalDate;

        Util util = new Util();
        CommonCombo combo = new CommonCombo();



        public ASSY001_038_REQUEST()
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


        private void ClearList(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgGrator);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                lotid = Util.NVC(tmps[1]);

            }

            initComto();
           
        }

        private void initComto()
        {
            string[] sFilter = { LoginInfo.LANGID, "APPR_MOVE_LOT" };
            combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.NONE, sCase: "COMMCODES", sFilter: sFilter);
        }
        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
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


        #region [승인자 입력]
        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtGrator.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("APPR_SEQS", typeof(string));
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
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
                        for (int i = 0; i < dtTo.Rows.Count; i++)
                        {
                            dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
                        }


                        dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtGrator.Text = "";
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);

                        dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [승인자 검색결과 여러개일경우]
        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("APPR_SEQS", typeof(string));
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgGratorSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            for (int i = 0; i < dtTo.Rows.Count; i++)
            {
                dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
            }


            dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = "";
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
                    dr["USERNAME"] = txtNotice.Text;
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

                        txtNotice.Text = "";
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelect);

                        dgNoticeSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [참조자 검색결과 여러개일경우]
        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
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

            txtNotice.Text = "";
        }
        #endregion

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }


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

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        #endregion

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
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        
        private void Request()
        {

            if (dgGrator.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1692");  //승인자가 필요합니다.
                return;
            }


            string sTo = string.Empty;
            string sCC = string.Empty;

            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = Convert.ToString(cboResnCode.SelectedValue);
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(string));
            inLot.Columns.Add("WIPQTY2", typeof(string));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));

            DataTable lot = GetLotInfo();

            DataRow row2 = null;

            for (int i = 0; i < lot.Rows.Count; i++)
            {
                row2 = inLot.NewRow();
                row2["LOTID"] = Convert.ToString(lot.Rows[i]["LOTID"]);
                row2["WIPQTY"] = Decimal.Parse(Convert.ToString(lot.Rows[i]["WIPQTY"]));
                row2["WIPQTY2"] = Decimal.Parse(Convert.ToString(lot.Rows[i]["WIPQTY2"]));
                row2["PRODID"] = Convert.ToString(lot.Rows[i]["PRODID"]);
                row2["PRODNAME"] = Convert.ToString(lot.Rows[i]["PRODNAME"]);
                row2["MODELID"] = Convert.ToString(lot.Rows[i]["MODELID"]);

                inLot.Rows.Add(row2);
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

            ////참조자
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
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {

                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }

                Util.MessageInfo("SFU1747");  //요청되었습니다.
                this.DialogResult = MessageBoxResult.OK;
            //    this.Close();

               
               

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //private void ReqCancel()
        //{
        //    string sTo = "";
        //    string sCC = "";

        //    //현재상태 체크
        //    DataTable dtRqstStatus = new DataTable();
        //    dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

        //    DataRow drStatus = dtRqstStatus.NewRow();
        //    drStatus["REQ_NO"] = _reqNo;


        //    dtRqstStatus.Rows.Add(drStatus);

        //    DataTable dtRsltStatus = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqstStatus);

        //    if (!dtRsltStatus.Rows[0]["REQ_RSLT_CODE"].Equals("REQ"))
        //    {
        //        Util.AlertInfo("SFU1691");  //승인이 진행 중입니다.
        //    }
        //    else
        //    {
        //        //여기까지 현재상태 체크

        //        DataTable dtRqst = new DataTable();
        //        dtRqst.Columns.Add("REQ_NO", typeof(string));
        //        dtRqst.Columns.Add("USERID", typeof(string));
        //        dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["REQ_NO"] = _reqNo;
        //        dr["USERID"] = LoginInfo.USERID;
        //        dr["REQ_RSLT_CODE"] = "DEL";


        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqst);

        //        for (int i = 0; i < dgGrator.Rows.Count; i++)
        //        {
        //            if (i == 0)//최초 승인자만 메일 가도록
        //            {
        //                sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
        //            }
        //        }

        //        //참조자

        //        for (int i = 0; i < dgNotice.Rows.Count; i++)
        //        {
        //            sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
        //        }


        //        MailSend mail = new CMM001.Class.MailSend();
        //        string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
        //        string sTitle = _reqNo + " " + this.Header;

        //        mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));

        //        Util.AlertInfo("SFU1937");  //취소되었습니다.
        //    }
        //    this.DialogResult = MessageBoxResult.OK;
        //    this.Close();
        //}

       

        private void dgRequest_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            Decimal dReqQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REQQTY"));

            if (dReqQty <= 0 || dReqQty > dWipQty)
            {
                Util.AlertInfo("SFU1749");  //요청 수량이 잘못되었습니다.

                DataTableConverter.SetValue(dg.CurrentRow.DataItem, "REQQTY", dWipQty);

                dg.CurrentRow.Refresh();
                return;

            }

            Decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_QTY"));
            Decimal dLanePtnQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_PTN_QTY"));
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "WIPQTY2", dReqQty * dLaneQty * dLanePtnQty);
            dg.CurrentRow.Refresh();
        }


        private void dgRequest_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            C1.WPF.DataGrid.DataGridNumericColumn dc = dg.Columns["REQQTY"] as C1.WPF.DataGrid.DataGridNumericColumn;

            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            dc.Maximum = Convert.ToDouble(dWipQty);
            dc.Minimum = 0;

        }

        private DataTable GetLotInfo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotid;
            dt.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_EQPT_BTCH_WRK_NO_WIPINFO", "INDATA", "OUTDATA", dt);


            return dtRslt;

        }













    }
}
