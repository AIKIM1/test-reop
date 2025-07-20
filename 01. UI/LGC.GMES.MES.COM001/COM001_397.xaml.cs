/*************************************************************************************
 Created Date : 2024.01.09
      Creator : 
   Decription : 라미반품승인요청
--------------------------------------------------------------------------------------
 [Change History]
  -           DEVELOPER : COM001_308 Copy
  2024.03.27  김대현 LAMI/VD 대기 반품로직 추가
  2024.05.13  김대현 LAMI/VD 대기 반품 대상 Lot이 아닌 경우 스캔시 경고메시지 발생
  2024.05.16  안유수    E20240502-001211 용어 변경, 반품승인 요청 -> 반품 요청, 반품승인 요청 이력 -> 반품 요청 이력

**************************************************************************************/

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
using System.Windows.Threading;


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_397.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_397 : UserControl
    {
        #region Private 변수
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        DataTable dtHistList = new DataTable();
        #endregion

        #region Form Load & Init Control
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_397()
        {
            InitializeComponent();
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        #endregion

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchReqLotList();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dgLotList.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                    dt.Rows[dgLotList.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    dgLotList.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            SaveReqList();
        }

        private void btnReturnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancelReq();
        }

        #endregion

        #region Functions
        void Init()
        {
            dgLotList.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("CHK", typeof(string));
            emptyTransferTable.Columns.Add("LOTID", typeof(string));
            emptyTransferTable.Columns.Add("PRJT_NAME", typeof(string));
            emptyTransferTable.Columns.Add("PRODID", typeof(string));
            emptyTransferTable.Columns.Add("RTN_AREA", typeof(string));
            emptyTransferTable.Columns.Add("WIPQTY", typeof(decimal));
            emptyTransferTable.Columns.Add("MOVE_RTN_TYPE_CODE", typeof(decimal));
            emptyTransferTable.Columns.Add("HOLD_CODE", typeof(string));
            emptyTransferTable.Columns.Add("RTN_REQ_NOTE", typeof(string));

            dgLotList.ItemsSource = DataTableConverter.Convert(emptyTransferTable);

            CommonCombo combo = new CommonCombo();

            string[] sFilter = { "RTN_STAT_CODE" };
            combo.SetCombo(cboRtnStatCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            string[] sFilter1 = { "MOVE_RTN_TYPE_CODE" };
            combo.SetCombo(cboMoveRtnTypeCode, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { LoginInfo.CFG_AREA_ID,"LAMI_NOTCHING_RETURN_APPROVAL_PERMISSION" };
            combo.SetCombo(cboApprovalPerson, CommonCombo.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter2);

            cboMoveRtnTypeCode.SelectedValue = "DR";

            GetLevel1List();

            rdoLotId.Checked += rdoLotSkidId_Checked;
            rdoSkidId.Checked += rdoLotSkidId_Checked;
            rdoSearchLotId.Checked += rdoSearchLotSkidId_Checked;
            rdoSearchSkidId.Checked += rdoSearchLotSkidId_Checked;

            rdoLami.Checked += RdoLami_Checked;
            rdoVDdispose.Checked += RdoVDdispose_Checked;


        }

        #endregion Functions

        #region TextBox Event
        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtLotid.Text.Trim()))
                {
                    SearchLotInfo(txtLotid.Text.Trim());
                }
            }
        }

        private void txtSkidid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtSkidid.Text.Trim()))
                {
                    SearchSkidInfo(txtSkidid.Text.Trim());
                }
            }
        }
        #endregion

        #region METHOD

        private void SearchReqLotList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("SKIDID", typeof(String));   //2023.05.18 김대현
                RQSTDT.Columns.Add("RTN_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                dr["TO_DATE"] = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                //2023.05.18 김대현
                if(rdoSearchLotId.IsChecked== true)
                {
                    dr["LOTID"] = string.IsNullOrEmpty(txtSearchLOT.Text) ? null : txtSearchLOT.Text;
                    dr["SKIDID"] = null;
                }

                if(rdoSearchSkidId.IsChecked== true)
                {
                    dr["LOTID"] = null;
                    dr["SKIDID"] = string.IsNullOrEmpty(txtSearchSkid.Text) ? null : txtSearchSkid.Text;
                }
                //2023.05.18 김대현
                dr["RTN_STAT_CODE"] = Util.GetCondition(cboRtnStatCode, bAllNull: true);

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_REQ_HIST_ASSY", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgListHist);

                if (Result.Rows.Count >= 1)
                {
                    dgListHist.ItemsSource = DataTableConverter.Convert(Result);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void cancelReq()
        {
            try
            {
                if (dgListHist.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //반품 승인 요청을 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5102"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int iRowCnt = 0;
                        DataRow[] drChk = Util.gridGetChecked(ref dgListHist, "CHK");

                        //DATA CHECK
                        foreach (DataRow r in drChk)
                        {
                            iRowCnt++;

                            if (r["RTN_STAT_CODE"].ToString() != "RETURN")
                            {
                                Util.Alert("SFU1939");  //취소 할 수 있는 상태가 아닙니다.
                                return;
                            }
                        }

                        if (iRowCnt == 0)
                        {
                            Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("RTN_STAT_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["USERID"] = LoginInfo.USERID;
                        row["RTN_STAT_CODE"] = "CANCEL_RETURN";

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                       
                        inLot.Columns.Add("RTN_REQ_ID", typeof(string));
                        inLot.Columns.Add("RTN_CNCL_NOTE", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("WIPQTY", typeof(decimal));
                        inLot.Columns.Add("PRODID", typeof(string));
                        inLot.Columns.Add("PRODNAME", typeof(string));
                        inLot.Columns.Add("MODELID", typeof(string));

                        for (int i = 0; i < dgListHist.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "CHK").ToString() == "1")
                            {
                                DataRow row2 = inLot.NewRow();


                                row2["RTN_REQ_ID"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_REQ_ID").ToString();
                                row2["RTN_CNCL_NOTE"] = string.IsNullOrEmpty(DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_CNCL_NOTE").ToString()) ? null : DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_CNCL_NOTE").ToString();
                                row2["LOTID"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "LOTID").ToString();
                                row2["WIPQTY"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_QTY").ToString();
                                row2["PRODID"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "PRODID").ToString();
                                row2["PRODNAME"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "PRODNAME").ToString();
                                row2["MODELID"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "MODELID").ToString();
                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_RTN_APPR_REQ_ASSY", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                else
                                {
                                    
                                    String sTo="";
                                    //정상 처리 되었습니다.
                                    for (int i = 0; i < dgListHist.GetRowCount(); i++)
                                    {
                                        if (DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "CHK").ToString() == "1")
                                        {
                                            sTo += DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_APPR_USERID").ToString().ToLower() + ";";
                                        }
                                    }


                                    Util.MessageValidation("SFU1275", (action) =>
                                    {
                                        MailSend mail = new CMM001.Class.MailSend();
                                        //String sTo = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_REQ_ID").ToString();
                                        string sMsg = ObjectDic.Instance.GetObjectName("요청취소");
                                        string sTitle = "재와인더(조립) 반품 승인요청 취소";

                                        mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sTo, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRtnReqNote), inLot));


                                        dgListHist.ItemsSource = null;
                                        SearchReqLotList();

                                    });
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );

                    }
                });


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void SearchLotInfo(string sLotID)
        {
            try
            {
                //스캔한 데이터가 없습니다.
                if (string.IsNullOrEmpty(sLotID))
                {
                    Util.MessageValidation("SFU2060", (action) =>
                    {
                        txtLotid.Focus();
                        txtLotid.SelectAll();

                    });
                    return;
                }

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString() == sLotID)
                    {
                        //동일한 LOT이 스캔되었습니다.
                        Util.MessageValidation("SFU1504", (action) =>
                        {
                            txtLotid.Focus();
                            txtLotid.SelectAll();

                        });
                        return;
                    }
                }
                
                //DATA CHECK
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "INLOT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("RTN_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotID;
                dr1["RTN_STAT_CODE"] = "RETURN";

                RQSTDT1.Rows.Add(dr1);

                DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_RTN_REQ", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Result1.Rows.Count >= 1)
                {
                    //이미 승인 요청 진행중인 LOT 입니다.
                    Util.MessageValidation("SFU5104", (action) =>
                    {
                        txtLotid.Focus();
                        txtLotid.SelectAll();

                    });
                    return;
                }

                //반품 요청 승인 LOT 정보
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                //DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable Result = new DataTable();
                if (rdoLami.IsChecked == true)
                {
                    Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else if (rdoVDdispose.IsChecked == true)
                {
                    if (!ValidationWHChk(sLotID))
                    {
                        return;
                    }

                    Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY_VD", "RQSTDT", "RSLTDT", RQSTDT);
                }

                if (Result.Rows.Count >= 1)
                {
                    if (dgLotList.GetRowCount() == 0)
                    {
                        dgLotList.ItemsSource = DataTableConverter.Convert(Result);
                    }
                    else
                    {
                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgLotList.ItemsSource);
                            dtSource.Merge(Result);

                            Util.gridClear(dgLotList);
                            dgLotList.ItemsSource = DataTableConverter.Convert(dtSource);
                        }
                        else
                        {
                            Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                        }
                    }
                }
                else
                {
                    DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtR.Rows.Count > 0)
                    {
                        //반품 대상 LOT이 아닙니다. (LOT : XXXX, 공정 : XXXXXXXX, 상태 : XXXXXX)
                        Util.MessageValidation("SFU5302", new object[] { sLotID, Util.NVC(dtR.Rows[0]["PROCNAME"]), Util.NVC(dtR.Rows[0]["LOTSTAT"]) });
                    }
                    else
                    {
                        //요청하신 Lot은 존재하지 않습니다.
                        Util.MessageValidation("SFU5301");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                txtLotid.Focus();
                txtLotid.SelectAll();
            }
        }

        private void SearchSkidInfo(string sSkidID)
        {
            try
            {
                //스캔한 데이터가 없습니다.
                if (string.IsNullOrEmpty(sSkidID))
                {
                    Util.MessageValidation("SFU2060", (action) =>
                    {
                        txtSkidid.Focus();
                        txtSkidid.SelectAll();
                    });
                    return;
                }

                //반품 요청 승인 LOT 정보
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SKIDID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SKIDID"] = sSkidID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                //DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable Result = new DataTable();
                if (rdoLami.IsChecked == true)
                {
                    Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else if (rdoVDdispose.IsChecked == true)
                {
                    Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY_VD", "RQSTDT", "RSLTDT", RQSTDT);
                }

                if (Result.Rows.Count >= 1)
                {
                    if (dgLotList.GetRowCount() == 0)
                    {
                        dgLotList.ItemsSource = DataTableConverter.Convert(Result);
                    }
                    else
                    {
                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgLotList.ItemsSource);

                            for(int i = 0; i < dtSource.Rows.Count; i++)
                            {
                                string lotid = Util.NVC(dtSource.Rows[i]["LOTID"]);

                                if (!ValidationWHChk(lotid))
                                {
                                    return;
                                }

                                DataRow[] drResult = Result.Select("LOTID='" + lotid + "'");

                                if (drResult.Length > 0)
                                {
                                    Result.Rows.Remove(drResult[0]);
                                }
                            }
                            dtSource.Merge(Result);

                            Util.gridClear(dgLotList);
                            dgLotList.ItemsSource = DataTableConverter.Convert(dtSource);
                        }
                        else
                        {
                            Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                        }
                    }
                }
                else
                {
                    DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtR.Rows.Count > 0)
                    {
                        //반품 대상 LOT이 아닙니다. (LOT : XXXX, 공정 : XXXXXXXX, 상태 : XXXXXX)
                        Util.MessageValidation("SFU5302", new object[] { Util.NVC(dtR.Rows[0]["LOTID"]), Util.NVC(dtR.Rows[0]["PROCNAME"]), Util.NVC(dtR.Rows[0]["LOTSTAT"]) });
            }
                    else
                    {
                        //요청하신 Lot은 존재하지 않습니다.
                        Util.MessageValidation("SFU5301");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                txtSkidid.Focus();
                txtSkidid.SelectAll();
            }
        }

        private void SaveReqList()
        {
            try
            {
                // 2025.03.18 UI Vadlidation 추가
                if (dgLotList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1498");  //데이터가 없습니다
                    return;
                }
                //if (string.IsNullOrEmpty(cboMoveRtnTypeCode.SelectedValue.ToString()))
                //{
                //    //반품유형을 선택해 주세요.
                //    Util.MessageValidation("SFU3640", (action) =>
                //    {
                //        cboMoveRtnTypeCode.Focus();
                //    });
                //    return;
                //}

                //if (cboMoveRtnTypeCode.SelectedValue.ToString() == "DR") // 품질이상
                //{
                    if ((popSearchLevel2.SelectedValue == null) || (popSearchLevel2.SelectedValue.ToString() == ""))
                    {
                        //품질이상인 경우 HOLD CODE를 선택해야 합니다. -> Hold CODE를 선택해주세요로 변경필요!
                        


                        Util.MessageValidation("SFU5100", (action) =>
                        {
                            txtLotid.Focus();
                            txtLotid.SelectAll();

                        });
                        return;
                    }
                //}
                string slotId = string.Empty;

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PROCID").ToString().Equals("A6100"))
                    {
                        slotId += DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString() + ',';
                    }
                }

                if (slotId.Length > 0)
                {
                    slotId = slotId.Substring(0, slotId.Length - 1);
                    //[%1] VD검사대기에서 반품시 전공정 LOSS 처리 및 폐기 됩니다. 
                    Util.MessageConfirm("SFU5959", result1 =>
                    {
                        if (result1 == MessageBoxResult.OK)
                        {
                            //반품 승인 요청을 하시겠습니까?
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5101"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    DataSet indataSet = new DataSet();
                                    DataTable inData = indataSet.Tables.Add("INDATA");
                                    inData.Columns.Add("SRCTYPE", typeof(string));
                                    inData.Columns.Add("FROM_SHOPID", typeof(string));
                                    inData.Columns.Add("FROM_AREA", typeof(string));
                                    inData.Columns.Add("RTN_STAT_CODE", typeof(string));
                                    inData.Columns.Add("USERID", typeof(string));
                                    inData.Columns.Add("APPR_USERID", typeof(string));

                                    DataRow row = inData.NewRow();
                                    row["SRCTYPE"] = "UI";
                                    row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                                    row["FROM_AREA"] = LoginInfo.CFG_AREA_ID;
                                    row["RTN_STAT_CODE"] = "RETURN";
                                    row["USERID"] = LoginInfo.USERID;
                                    row["APPR_USERID"] = cboApprovalPerson.SelectedValue.ToString();

                                    indataSet.Tables["INDATA"].Rows.Add(row);

                                    DataTable inLot = indataSet.Tables.Add("INLOT");
                                    inLot.Columns.Add("LOTID", typeof(string));
                                    inLot.Columns.Add("WIPQTY", typeof(decimal));
                                    inLot.Columns.Add("MOVE_RTN_TYPE_CODE", typeof(string));
                                    inLot.Columns.Add("HOLD_CODE", typeof(string));
                                    inLot.Columns.Add("RTN_REQ_NOTE", typeof(string));
                                    inLot.Columns.Add("TO_AREA", typeof(string));
                                    inLot.Columns.Add("PRODID", typeof(string));
                                    inLot.Columns.Add("PRODNAME", typeof(string));
                                    inLot.Columns.Add("MODELID", typeof(string));


                                    for (int i = 0; i < dgLotList.GetRowCount(); i++)
                                    {
                                        DataRow row2 = inLot.NewRow();
                                        row2["LOTID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString();
                                        row2["WIPQTY"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY").ToString();

                                        //row2["MOVE_RTN_TYPE_CODE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString();
                                        //row2["MOVE_RTN_TYPE_CODE"] = cboMoveRtnTypeCode.SelectedValue.ToString();
                                        row2["MOVE_RTN_TYPE_CODE"] = "DR";

                                        //if (cboMoveRtnTypeCode.SelectedValue.ToString() == "DR") // 품질이상
                                        //{
                                        row2["HOLD_CODE"] = popSearchLevel2.SelectedValue.ToString();
                                        //}
                                        //else
                                        //{
                                        //    row2["HOLD_CODE"] = "PH01H98";
                                        //}

                                        row2["RTN_REQ_NOTE"] = txtRtnReqNote.Text.Trim();
                                        //if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString() == "DR")
                                        //    row2["HOLD_CODE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "HOLD_CODE").ToString();
                                        //else
                                        //row2["HOLD_CODE"] = "PH01H98";

                                        //row2["RTN_REQ_NOTE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_REQ_NOTE").ToString();
                                        row2["TO_AREA"] = LoginInfo.CFG_AREA_ID + "_" + DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PROCID").ToString();

                                        row2["PRODID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODID").ToString();
                                        row2["PRODNAME"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODNAME").ToString();
                                        row2["MODELID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MODELID").ToString();


                                        indataSet.Tables["INLOT"].Rows.Add(row2);
                                    }

                                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RTN_APPR_REQ_ASSY", "INDATA,INLOT", null, (bizResult, bizException) =>
                                    {
                                        try
                                        {
                                            if (bizException != null)
                                            {
                                                Util.MessageException(bizException);
                                                return;
                                            }
                                            else
                                            {
                                                dgLotList.ItemsSource = null;

                                                //정상 처리 되었습니다.
                                                Util.MessageValidation("SFU1275", (action) =>
                                                {
                                                    MailSend mail = new CMM001.Class.MailSend();
                                                    String sTo = cboApprovalPerson.SelectedValue.ToString().ToLower();
                                                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                                                    string sTitle = "재와인더(조립) 반품 승인요청";

                                                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sTo, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRtnReqNote), inLot));




                                                    txtRtnReqNote.Text = string.Empty;
                                                    txtLotid.Focus();
                                                    txtLotid.SelectAll();

                                                });
                                                return;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                        }
                                    }, indataSet
                                    );
                                    //승인자 메일

                                    //MailSend mail = new CMM001.Class.MailSend();
                                    //String sTo = cboApprovalPerson.SelectedValue.ToString().ToLower();
                                    //string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                                    //string sTitle = "재와인더(조립) 반품 승인요청";

                                    //mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sTo, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRtnReqNote), inLot));

                                }
                            });
                        }
                    }, slotId);
                }
                else
                {
                    //반품 승인 요청을 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5101"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("FROM_SHOPID", typeof(string));
                            inData.Columns.Add("FROM_AREA", typeof(string));
                            inData.Columns.Add("RTN_STAT_CODE", typeof(string));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("APPR_USERID", typeof(string));

                            DataRow row = inData.NewRow();
                            row["SRCTYPE"] = "UI";
                            row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            row["FROM_AREA"] = LoginInfo.CFG_AREA_ID;
                            row["RTN_STAT_CODE"] = "RETURN";
                            row["USERID"] = LoginInfo.USERID;
                            row["APPR_USERID"] = cboApprovalPerson.SelectedValue.ToString();

                            indataSet.Tables["INDATA"].Rows.Add(row);

                            DataTable inLot = indataSet.Tables.Add("INLOT");
                            inLot.Columns.Add("LOTID", typeof(string));
                            inLot.Columns.Add("WIPQTY", typeof(decimal));
                            inLot.Columns.Add("MOVE_RTN_TYPE_CODE", typeof(string));
                            inLot.Columns.Add("HOLD_CODE", typeof(string));
                            inLot.Columns.Add("RTN_REQ_NOTE", typeof(string));
                            inLot.Columns.Add("TO_AREA", typeof(string));
                            inLot.Columns.Add("PRODID", typeof(string));
                            inLot.Columns.Add("PRODNAME", typeof(string));
                            inLot.Columns.Add("MODELID", typeof(string));


                            for (int i = 0; i < dgLotList.GetRowCount(); i++)
                            {
                                DataRow row2 = inLot.NewRow();
                                row2["LOTID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString();
                                row2["WIPQTY"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY").ToString();

                                //row2["MOVE_RTN_TYPE_CODE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString();
                                //row2["MOVE_RTN_TYPE_CODE"] = cboMoveRtnTypeCode.SelectedValue.ToString();
                                row2["MOVE_RTN_TYPE_CODE"] = "DR";

                                //if (cboMoveRtnTypeCode.SelectedValue.ToString() == "DR") // 품질이상
                                //{
                                row2["HOLD_CODE"] = popSearchLevel2.SelectedValue.ToString();
                                //}
                                //else
                                //{
                                //    row2["HOLD_CODE"] = "PH01H98";
                                //}

                                row2["RTN_REQ_NOTE"] = txtRtnReqNote.Text.Trim();
                                //if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString() == "DR")
                                //    row2["HOLD_CODE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "HOLD_CODE").ToString();
                                //else
                                //row2["HOLD_CODE"] = "PH01H98";

                                //row2["RTN_REQ_NOTE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_REQ_NOTE").ToString();
                                row2["TO_AREA"] = LoginInfo.CFG_AREA_ID + "_" + DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PROCID").ToString();

                                row2["PRODID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODID").ToString();
                                row2["PRODNAME"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODNAME").ToString();
                                row2["MODELID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MODELID").ToString();


                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RTN_APPR_REQ_ASSY", "INDATA,INLOT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    else
                                    {
                                        dgLotList.ItemsSource = null;

                                        //정상 처리 되었습니다.
                                        Util.MessageValidation("SFU1275", (action) =>
                                        {
                                            MailSend mail = new CMM001.Class.MailSend();
                                            String sTo = cboApprovalPerson.SelectedValue.ToString().ToLower();
                                            string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                                            string sTitle = "재와인더(조립) 반품 승인요청";

                                            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sTo, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRtnReqNote), inLot));




                                            txtRtnReqNote.Text = string.Empty;
                                            txtLotid.Focus();
                                            txtLotid.SelectAll();

                                        });
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                }
                            }, indataSet
                            );
                            //승인자 메일

                            //MailSend mail = new CMM001.Class.MailSend();
                            //String sTo = cboApprovalPerson.SelectedValue.ToString().ToLower();
                            //string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                            //string sTitle = "재와인더(조립) 반품 승인요청";

                            //mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sTo, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRtnReqNote), inLot));

                        }
                    });
                }
                #region MyRegion
                //Data Check
                //for (int i = 0; i < dgLotList.GetRowCount(); i++)
                //{
                //    if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString()))
                //    {
                //        //반품유형을 선택해 주세요.
                //        Util.MessageValidation("SFU3640", (action) =>
                //        {
                //            txtLotid.Focus();
                //            txtLotid.SelectAll();

                //        });
                //        return;
                //    }

                //    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString() == "DR") //품질이상
                //    {
                //        if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "HOLD_CODE").ToString()))
                //        {
                //            //품질이상인 경우 HOLD CODE를 선택해야 합니다.
                //            Util.MessageValidation("SFU5100", (action) =>
                //            {
                //                txtLotid.Focus();
                //                txtLotid.SelectAll();

                //            });
                //            return;
                //        }
                //    }
                //} 
                #endregion


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void GetLevel1List()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "HOLD_LOT";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_DFCT_CODE_CBO", "INDATA", "OUTDATA", dtRqst);



                DataTable dtRqst2 = new DataTable();
                dtRqst2.Columns.Add("LANGID", typeof(string));
                dtRqst2.Columns.Add("AREAID", typeof(string));
                dtRqst2.Columns.Add("USE_FLAG", typeof(string));
                dtRqst2.Columns.Add("COM_TYPE_CODE", typeof(string));


                DataRow dr2 = dtRqst2.NewRow();
                dr2["LANGID"] = LoginInfo.LANGID;
                dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr2["USE_FLAG"] = "Y";
                dr2["COM_TYPE_CODE"] = "LAMI_NOTCHING_RTN_HOLD_CODE";

                dtRqst2.Rows.Add(dr2);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", dtRqst2);

                popSearchLevel1.ItemsSource = DataTableConverter.Convert(dtRslt);



                if(dtRslt2.Rows.Count == 0)
                {
                    popSearchLevel1.SelectedValue = "H99";
                    popSearchLevel1.SelectedText = "기타";
                }
                else
                {
                    popSearchLevel1.SelectedValue = dtRslt2.Rows[0]["ATTR1"].ToString();
                    DataRow[] rows = dtRslt.Select("CBO_CODE = '" + dtRslt2.Rows[0]["ATTR1"].ToString() + "'");
                    if (rows.Length > 0)
                    {
                        popSearchLevel1.SelectedText = rows[0]["CBO_NAME"].ToString();
                    }
                }
                
                

                

                popSearchLevel1_ValueChanged(popSearchLevel1, EventArgs.Empty);


                //popSearchLevel1.SelectedText = popSearchLevel1.SelectedValue.

                //dtRslt2.Rows[0]["ATTR1"];





            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        #endregion

        private void popSearchLevel1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DFCT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "HOLD_LOT";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                
                dr["DFCT_CODE"] = popSearchLevel1.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_HOLD_DFCT_CODE_LVL2_CBO", "INDATA", "OUTDATA", RQSTDT);

                popSearchLevel2.ItemsSource = DataTableConverter.Convert(dtRslt);

                DataTable dtRqst2 = new DataTable();
                dtRqst2.Columns.Add("LANGID", typeof(string));
                dtRqst2.Columns.Add("AREAID", typeof(string));
                dtRqst2.Columns.Add("USE_FLAG", typeof(string));
                dtRqst2.Columns.Add("COM_TYPE_CODE", typeof(string));


                DataRow dr2 = dtRqst2.NewRow();
                dr2["LANGID"] = LoginInfo.LANGID;
                dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr2["USE_FLAG"] = "Y";
                dr2["COM_TYPE_CODE"] = "LAMI_NOTCHING_RTN_HOLD_CODE";

                dtRqst2.Rows.Add(dr2);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", dtRqst2);




                if (dtRslt2.Rows.Count == 0)
                {
                    popSearchLevel2.SelectedValue = "PH99H16";
                    popSearchLevel2.SelectedText = "품질반품자동홀드";
                }
                else
                {
                    popSearchLevel2.SelectedValue = dtRslt2.Rows[0]["COM_CODE"].ToString();
                    DataRow[] rows2 = dtRslt.Select("CBO_CODE = '" + dtRslt2.Rows[0]["COM_CODE"].ToString() + "'");
                    if (rows2.Length > 0)
                    {
                        popSearchLevel2.SelectedText = rows2[0]["CBO_NAME"].ToString();
                    }
                }
                



                



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }


        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_SearchLotInfo(item) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
               

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

                e.Handled = true;
            }
        }

        private void txtSkidid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    string sPasteString = Clipboard.GetText();

                    SearchSkidInfo(sPasteString);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

                e.Handled = true;
            }
        }

        private void rdoLotSkidId_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoLotId.IsChecked == true)
            {
                tbLotId.Visibility = Visibility.Visible;
                txtLotid.Visibility = Visibility.Visible;

                tbSkidId.Visibility = Visibility.Collapsed;
                txtSkidid.Visibility = Visibility.Collapsed;
                txtSkidid.Text = string.Empty;
            }

            if(rdoSkidId.IsChecked== true)
            {
                tbLotId.Visibility = Visibility.Collapsed;
                txtLotid.Visibility = Visibility.Collapsed;

                tbSkidId.Visibility = Visibility.Visible;
                txtSkidid.Visibility = Visibility.Visible;
                txtLotid.Text = string.Empty;
            }
        }

        private void rdoSearchLotSkidId_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoSearchLotId.IsChecked == true)
            {
                tbSearchLot.Visibility = Visibility.Visible;
                txtSearchLOT.Visibility = Visibility.Visible;

                tbSearchSkid.Visibility = Visibility.Collapsed;
                txtSearchSkid.Visibility = Visibility.Collapsed;
                txtSearchSkid.Text = string.Empty;
            }

            if (rdoSearchSkidId.IsChecked == true)
            {
                tbSearchLot.Visibility = Visibility.Collapsed;
                txtSearchLOT.Visibility = Visibility.Collapsed;

                tbSearchSkid.Visibility = Visibility.Visible;
                txtSearchSkid.Visibility = Visibility.Visible;
                txtSearchLOT.Text = string.Empty;
            }
        }

        private void RdoVDdispose_Checked(object sender, RoutedEventArgs e)
        {
            CommonCombo combo = new CommonCombo();

            string[] sFilter2 = { LoginInfo.CFG_AREA_ID, "VD_NOTCHING_RETURN_APPROVAL_PERMISSION" };
            combo.SetCombo(cboApprovalPerson, CommonCombo.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter2);
        }

        private void RdoLami_Checked(object sender, RoutedEventArgs e)
        {
            CommonCombo combo = new CommonCombo();

            string[] sFilter2 = { LoginInfo.CFG_AREA_ID, "LAMI_NOTCHING_RETURN_APPROVAL_PERMISSION" };
            combo.SetCombo(cboApprovalPerson, CommonCombo.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter2);
        }

        private void  MultSearchLotInfo(string sLotID)
        {
            try
            {
                //스캔한 데이터가 없습니다.
                if (string.IsNullOrEmpty(sLotID))
                {
                    Util.MessageValidation("SFU2060", (action) =>
                    {
                        txtLotid.Focus();
                        txtLotid.SelectAll();

                    });
                    return;
                }

                for (int i = 0; i < dgLotList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString() == sLotID)
                    {
                        //동일한 LOT이 스캔되었습니다.
                        Util.MessageValidation("SFU1504", (action) =>
                        {
                            txtLotid.Focus();
                            txtLotid.SelectAll();

                        });
                        return;
                    }
                }

                //DATA CHECK
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "INLOT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("RTN_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotID;
                dr1["RTN_STAT_CODE"] = "RETURN";

                RQSTDT1.Rows.Add(dr1);

                DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_RTN_REQ", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Result1.Rows.Count >= 1)
                {
                    //이미 승인 요청 진행중인 LOT 입니다.
                    Util.MessageValidation("SFU5104", (action) =>
                    {
                        txtLotid.Focus();
                        txtLotid.SelectAll();

                    });
                    return;
                }

                //반품 요청 승인 LOT 정보
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INLOT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count >= 1)
                {
                    if (dgLotList.GetRowCount() == 0)
                    {
                        dgLotList.ItemsSource = DataTableConverter.Convert(Result);
                    }
                    else
                    {
                        //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                        if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgLotList.ItemsSource);
                            dtSource.Merge(Result);

                            Util.gridClear(dgLotList);
                            dgLotList.ItemsSource = DataTableConverter.Convert(dtSource);
                        }
                        else
                        {
                            Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                txtLotid.Focus();
                txtLotid.SelectAll();
            }
        }

        private bool ValidationWHChk(string lotId)
        {
            bool chk = true;

            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LOTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["LOTID"] = lotId;

            RQSTDT.Rows.Add(dr);

            DataTable dtChkResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_WIPATTR_CHK", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtChkResult != null && dtChkResult.Rows.Count > 0)
            {
                if (Util.NVC(dtChkResult.Rows[0]["WH_LOAD_CHK_EXCL_FLAG"]).Equals("Y"))
                {
                    chk = true;
                }
                else
                {
                    //LOT[%1]는 창고[%2]에 입고되어 있습니다. 출고 후 처리 바랍니다.
                    Util.MessageValidation("SFU5958", new object[] { lotId, Util.NVC(dtChkResult.Rows[0]["EQPTID"]) });
                    chk = false;
                }
            }

            return chk;
        }

        bool Multi_SearchLotInfo(string sLotid)
        {
            try
            {
                DoEvents();


                //DATA CHECK
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "INLOT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("RTN_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotid;
                dr1["RTN_STAT_CODE"] = "RETURN";

                RQSTDT1.Rows.Add(dr1);

                DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_RTN_REQ", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Result1.Rows.Count >= 1)
                {
                    //이미 승인 요청 진행중인 LOT 입니다.
                    Util.MessageValidation("SFU5104", (action) =>
                    {
                        txtLotid.Focus();
                        txtLotid.SelectAll();
                    });
                    return true;
                }
                else
                {
                    //반품 요청 승인 LOT 정보
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INLOT";
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("LOTID", typeof(String));
                    RQSTDT.Columns.Add("AREAID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sLotid.Trim();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                    RQSTDT.Rows.Add(dr);

                    //DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                    DataTable Result = new DataTable();
                    if (rdoLami.IsChecked == true)
                    {
                        Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    else if (rdoVDdispose.IsChecked == true)
                    {
                        if (!ValidationWHChk(sLotid))
                        {
                            return false;
                        }

                        Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT_ASSY_VD", "RQSTDT", "RSLTDT", RQSTDT);
                    }

                    if (Result.Rows.Count >= 1)
                    {
                        if (dgLotList.GetRowCount() == 0)
                        {
                            dgLotList.ItemsSource = DataTableConverter.Convert(Result);
                        }
                        else
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgLotList.ItemsSource);

                            if (Result.Rows.Count != 0)
                            {
                                // 중복체크
                                if (dtSource.Select("LOTID = '" + Convert.ToString(Result.Rows[0]["LOTID"]) + "'").Count() == 0)
                                {
                                    //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                    if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(Result.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                    {
                                        dtSource.Merge(Result);
                                        Util.gridClear(dgLotList);
                                        Util.GridSetData(dgLotList, dtSource, FrameOperation);
                                    }
                                    else
                                    {
                                        Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        DataTable dtR = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtR.Rows.Count > 0)
                        {
                            //반품 대상 LOT이 아닙니다. (LOT : XXXX, 공정 : XXXXXXXX, 상태 : XXXXXX)
                            Util.MessageValidation("SFU5302", new object[] { sLotid, Util.NVC(dtR.Rows[0]["PROCNAME"]), Util.NVC(dtR.Rows[0]["LOTSTAT"]) });
                        }
                        else
                        {
                            //요청하신 Lot은 존재하지 않습니다.
                            Util.MessageValidation("SFU5301");
                        }
                    }
                }
               
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}