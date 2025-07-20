/*************************************************************************************
 Created Date : -
      Creator : 
   Decription : 반품승인요청
--------------------------------------------------------------------------------------
 [Change History]
  -           DEVELOPER : Initial Created.
  2021.08.19  김지은    : 반품 시 LOT유형의 시험생산구분코드 Validation
  2023.05.18  김대현    : 반품 승인 요청 목록 조회시 SKID ID로도 조회가능하도록 수정
  2024.05.16  안유수    E20240502-001211 용어 변경, 반품승인 요청 -> 반품 요청, 반품승인 요청 이력 -> 반품 요청 이력
  2024.05.24  배현우    : [E20240524-001573] 대표LOT 적용 AREA일시 대표LOT 반품 요청 기능 추가



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
    /// COM001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_308 : UserControl
    {
        #region Private 변수
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        DataTable dtHistList = new DataTable();
        private bool RepLotUseArea = false;
        #endregion

        #region Form Load & Init Control
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_308()
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


            //CommonCombo.SetDataGridComboItem("DA_BAS_SEL_ACTIVITIREASON_CBO", new string[] { "LANGID", "ACTID" }, new string[] { LoginInfo.LANGID, "HOLD_LOT" }, CommonCombo.ComboStatus.NONE, dgLotList.Columns["HOLD_CODE_LVL1"], "CBO_CODE", "CBO_NAME");
            //CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "MOVE_RTN_TYPE_CODE" }, CommonCombo.ComboStatus.NONE, dgLotList.Columns["MOVE_RTN_TYPE_CODE"], "CBO_CODE", "CBO_NAME");
            
            CommonCombo combo = new CommonCombo();

            string[] sFilter = { "RTN_STAT_CODE" };
            combo.SetCombo(cboRtnStatCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            string[] sFilter1 = { "MOVE_RTN_TYPE_CODE" };
            combo.SetCombo(cboMoveRtnTypeCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            GetLevel1List();

            //2023.05.18 김대현
            rdoLotId.Checked += rdoLotSkidId_Checked;
            rdoSkidId.Checked += rdoLotSkidId_Checked;
            rdoSearchLotId.Checked += rdoSearchLotSkidId_Checked;
            rdoSearchSkidId.Checked += rdoSearchLotSkidId_Checked;

            GetRepLotUseArea();
            if (RepLotUseArea)
            {
                rdoSkidId.Content = ObjectDic.Instance.GetObjectName("대표 LOTID");
                tbSkidId.Text = ObjectDic.Instance.GetObjectName("대표 LOTID");
            }
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

                if (!RepLotUseArea)
                {
                    if (!string.IsNullOrEmpty(txtSkidid.Text.Trim()))
                    {
                        SearchSkidInfo(txtSkidid.Text.Trim());
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(txtLotid.Text.Trim()))
                    {
                        SearchRepLotInfo(txtSkidid.Text.Trim());
                    }
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

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_REQ_HIST", "RQSTDT", "RSLTDT", RQSTDT);

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

                        for (int i = 0; i < dgListHist.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "CHK").ToString() == "1")
                            {
                                DataRow row2 = inLot.NewRow();

                                row2["RTN_REQ_ID"] = DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_REQ_ID").ToString();
                                row2["RTN_CNCL_NOTE"] = string.IsNullOrEmpty(DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_CNCL_NOTE").ToString()) ? null : DataTableConverter.GetValue(dgListHist.Rows[i].DataItem, "RTN_CNCL_NOTE").ToString();
                                indataSet.Tables["INLOT"].Rows.Add(row2);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_RTN_APPR_REQ", "INDATA,INLOT", null, (bizResult, bizException) =>
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
                                    dgListHist.ItemsSource = null;

                                    //정상 처리 되었습니다.
                                    Util.MessageValidation("SFU1275", (action) =>
                                    {
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

                DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_ELTR_RTN", "RQSTDT", "RSLTDT", RQSTDT1);

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

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT", "RQSTDT", "RSLTDT", RQSTDT);

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

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SearchRepLotInfo(string sLotID)
        {
            try
            {
                //스캔한 데이터가 없습니다.
                if (string.IsNullOrEmpty(sLotID))
                {
                    Util.MessageValidation("SFU2060", (action) =>
                    {
                        txtSkidid.Focus();
                        txtSkidid.SelectAll();
                    });
                    return;
                }

                //RFID로 LOTID 가져옴
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LOTID"] = sLotID;

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_REP_LOTID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Init();
                        }
                    });
                    return;
                }
                else
                {
                    for (int i = 0; i < dtLot.Rows.Count; i++)
                    {

                        SearchLotInfo(dtLot.Rows[i]["LOTID"].ToString());
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
                if (string.IsNullOrEmpty(cboMoveRtnTypeCode.SelectedValue.ToString()))
                {
                    //반품유형을 선택해 주세요.
                    Util.MessageValidation("SFU3640", (action) =>
                    {
                        cboMoveRtnTypeCode.Focus();
                    });
                    return;
                }

                if (cboMoveRtnTypeCode.SelectedValue.ToString() == "DR") // 품질이상
                {
                    if ((popSearchLevel2.SelectedValue == null) || (popSearchLevel2.SelectedValue.ToString() == ""))
                    {
                        //품질이상인 경우 HOLD CODE를 선택해야 합니다.
                        Util.MessageValidation("SFU5100", (action) =>
                        {
                            txtLotid.Focus();
                            txtLotid.SelectAll();

                        });
                        return;
                    }
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

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        row["FROM_AREA"] = LoginInfo.CFG_AREA_ID;
                        row["RTN_STAT_CODE"] = "RETURN";
                        row["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("WIPQTY", typeof(decimal));
                        inLot.Columns.Add("MOVE_RTN_TYPE_CODE", typeof(string));
                        inLot.Columns.Add("HOLD_CODE", typeof(string));
                        inLot.Columns.Add("RTN_REQ_NOTE", typeof(string));
                        inLot.Columns.Add("TO_AREA", typeof(string));

                        for (int i = 0; i < dgLotList.GetRowCount(); i++)
                        {
                            DataRow row2 = inLot.NewRow();
                            row2["LOTID"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString();
                            row2["WIPQTY"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "WIPQTY").ToString();
                            //row2["MOVE_RTN_TYPE_CODE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString();
                            row2["MOVE_RTN_TYPE_CODE"] = cboMoveRtnTypeCode.SelectedValue.ToString();

                            if (cboMoveRtnTypeCode.SelectedValue.ToString() == "DR") // 품질이상
                            {
                                row2["HOLD_CODE"] = popSearchLevel2.SelectedValue.ToString();
                            }
                            else
                            {
                                row2["HOLD_CODE"] = "PH01H98";
                            }

                            row2["RTN_REQ_NOTE"] = txtRtnReqNote.Text.Trim();
                            //if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MOVE_RTN_TYPE_CODE").ToString() == "DR")
                            //    row2["HOLD_CODE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "HOLD_CODE").ToString();
                            //else
                            //    row2["HOLD_CODE"] = "PH01H98";

                            //row2["RTN_REQ_NOTE"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTN_REQ_NOTE").ToString();
                            row2["TO_AREA"] = DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "FROM_AREAID").ToString();

                            indataSet.Tables["INLOT"].Rows.Add(row2);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RTN_APPR_REQ", "INDATA,INLOT", null, (bizResult, bizException) =>
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

                    }
                });

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

                popSearchLevel1.ItemsSource = DataTableConverter.Convert(dtRslt);

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

                    if (!RepLotUseArea)
                        SearchSkidInfo(sPasteString);
                    else
                        SearchRepLotInfo(sPasteString);
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

                DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_ELTR_RTN", "RQSTDT", "RSLTDT", RQSTDT1);

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

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT", "RQSTDT", "RSLTDT", RQSTDT);

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

                DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_ELTR_RTN", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Result1.Rows.Count >= 1)
                {
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

                    DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTN_APPR_LOT", "RQSTDT", "RSLTDT", RQSTDT);

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
                }
               
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetRepLotUseArea()
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "REP_LOT_USE_AREA";
            row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

            RQSTDT1.Rows.Add(row);

            DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

            if (dtCommon != null)
            {
                if (dtCommon.Rows.Count > 0)
                {
                    RepLotUseArea = true;
                }
                else
                {
                    RepLotUseArea = false;
                }
            }
        }
    }
}