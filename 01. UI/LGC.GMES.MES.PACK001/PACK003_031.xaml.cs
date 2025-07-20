using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK003_031.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK003_031 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        bool bReGrouping = false;

        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_031()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                setComboBox();
                Limited_Qty_Set();
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);   //일주일 전 날짜
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;                 //오늘 날짜

                btnGroupDisassembley.IsEnabled = false;

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 기준수량 제한수량  값 적용
        private void Limited_Qty_Set()
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["CMCDTYPE"] = "LIMITED_QTY_PACK";
            dr["CBO_CODE"] = "LIMIT_GROUP_QTY";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", RQSTDT);

            STANDARD_QTY.Text = Util.NVC(dtResult.Rows[0]["ATTRIBUTE1"]);
            LIMIT_QTY.Text = Util.NVC(dtResult.Rows[0]["ATTRIBUTE2"]);

        }
        #endregion


        #region Event   
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    tgTargetLOT_Search(txtLotID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    string lotList = string.Empty;
                    if (!string.IsNullOrEmpty(sPasteStrings[0].ToString()))
                    {                    
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (i == 0)
                            {
                                lotList = sPasteStrings[i];
                            }
                            else
                            {
                                lotList = lotList + "," + sPasteStrings[i];
                            }
                            System.Windows.Forms.Application.DoEvents();
                            int Count = i;
                        }    
                    }
                    tgTargetLOT_Search(lotList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                e.Handled = true;
            }
        }

        private void tgTargetLOT_Search(String LotID)
        {
            DataTable dtAddLotList = DataTableConverter.Convert(dgTargetLOT.ItemsSource);

            if (string.IsNullOrWhiteSpace(LotID))
            {
                Util.MessageInfo("SFU1190");    // 조회할 LOT ID 를 입력하세요.
                return;
            }

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LOTID"] = LotID;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GROUP_TARGET", "INDATA", "OUTDATA", RQSTDT);

            if (dtResult.Rows.Count > 0)
            {
                int Total_Count = dtAddLotList.Rows.Count + dtResult.Rows.Count;
                // 기준수량이 초과 되었을 경우
                if (Total_Count > Convert.ToInt32(STANDARD_QTY.Text))
                {
                    Util.MessageConfirm("SFU5951", result =>     // 기준수량 %1개를 초과하여 입력하였습니다. 실물 수량 확인에 유의 하십시오. 그룹 관리 진행 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            tgTargetLOT_SetData(dtResult, dtAddLotList);
                        }
                    }, Convert.ToInt32(STANDARD_QTY.Text));
                }
                else
                {
                    tgTargetLOT_SetData(dtResult, dtAddLotList);
                }
                
            }
            else
            {
                Util.MessageInfo("SFU2816");    // 조회 결과가 없습니다.
                return;
            }
        }

        // tgTargetLOT에 데이터 Set
        private void tgTargetLOT_SetData(DataTable dtResult, DataTable dtAddLotList)
        {
            if (chkRegroup.IsChecked == false && dtResult.Rows[0]["GR_FLAG"].ToString().Equals("Y"))
            {
                // 이미 Group에 포함된 LOTID 입력시 작업자가 조회된 Group을 재포장 기능을 사용 하도록 셋팅함
                // 이미 다른 Group을 선택하여 재포장 중이라면 error 팝업 발생
                if (dgTargetLOT.GetRowCount() == 0 && bReGrouping == false)
                {
                    string sGroupID = dtResult.Rows[0]["GR_ID"].ToString();
                    string sEQPTID = dtResult.Rows[0]["EQPTID"].ToString();
                    string sLogisPackType = dtResult.Rows[0]["LOGIS_PACK_TYPE"].ToString();

                    GetGroupLotList(sGroupID);

                    DataTable dtGroupLotList = DataTableConverter.Convert(dgTargetLOT.ItemsSource);

                    dtAddLotList.Merge(dtGroupLotList);

                    txtGroupID.Text = sGroupID;

                    cboEqptID1.SelectedValue = sEQPTID;
                    cboEqptID1.IsEnabled = false;
                    //cboPackType.SelectedValue = sLogisPackType;
                    //cboPackType.IsEnabled = false;
                    btnGroupDisassembley.IsEnabled = true;

                    bReGrouping = true;
                }
            }

            if (dgTargetLOT.GetRowCount() == 0)
            {
                // 에러 LOTID
                var ErrorLotid = dtResult.AsEnumerable().Where(x => !overLapLot(x.Field<string>("LOTID")) ||
                                                                    (chkRegroup.IsChecked == false && x.Field<string>("GR_FLAG") == "Y") ||
                                                                    x.Field<string>("PROCID") != dtResult.Rows[0]["PROCID"].ToString() ||
                                                                    x.Field<string>("PRODID") != dtResult.Rows[0]["PRODID"].ToString());
                // 정상 LOTID
                var NoErrorLotid = dtResult.AsEnumerable().Except(ErrorLotid);

                DataTable ErrorLotidList = dtResult.Clone();

                if (NoErrorLotid.Count() > 0)
                {
                    dtAddLotList.Merge(NoErrorLotid.CopyToDataTable());
                    tgTargetLOT_Over_CHK(dtAddLotList, ErrorLotidList);
                }

                if (ErrorLotidList.Rows.Count > 0 || ErrorLotid.Count() > 0)
                {
                    if (ErrorLotid.Count() > 0)
                    {
                        ErrorLotidList.Merge(ErrorLotid.CopyToDataTable());
                    }

                    DataTable ErrorLotidMsg = new DataTable();
                    ErrorLotidMsg.TableName = "ErrorLotidMsg";
                    ErrorLotidMsg.Columns.Add("LOTID", typeof(string));
                    ErrorLotidMsg.Columns.Add("MESSAGEID", typeof(string));

                    for (int i = 0; i < ErrorLotidList.Rows.Count; i++)
                    {
                        if (!overLapLot(ErrorLotidList.Rows[i]["LOTID"].ToString()))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU1384");
                        }
                        else if (ErrorLotidList.Rows[i]["GR_FLAG"].Equals("Y"))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU8473");
                        }
                        else if (!ErrorLotidList.Rows[i]["PROCID"].Equals(dtResult.Rows[0]["PROCID"].ToString()))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU4957");
                        }
                        else if (!ErrorLotidList.Rows[i]["PRODID"].Equals(dtResult.Rows[0]["PRODID"].ToString()))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU4956");
                        }
                        else
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU5950");
                        }
                    }
                    this.Show_PACK003_031_EXCEPTION_POPUP(ErrorLotidMsg);
                }
                Util.GridSetData(dgTargetLOT, dtAddLotList, FrameOperation);
            }
            else
            {
                // 에러 LOTID
                var ErrorLotid = dtResult.AsEnumerable().Where(x => !overLapLot(x.Field<string>("LOTID")) ||
                                                                    (chkRegroup.IsChecked == false && x.Field<string>("GR_FLAG") == "Y") ||
                                                                    x.Field<string>("PROCID") != DataTableConverter.GetValue(dgTargetLOT.Rows[0].DataItem, "PROCID").ToString() ||
                                                                    x.Field<string>("PRODID") != DataTableConverter.GetValue(dgTargetLOT.Rows[0].DataItem, "PRODID").ToString());
                // 정상 LOTID
                var NoErrorLotid = dtResult.AsEnumerable().Except(ErrorLotid);

                DataTable ErrorLotidList = dtResult.Clone();

                if (NoErrorLotid.Count() > 0)
                {
                    dtAddLotList.Merge(NoErrorLotid.CopyToDataTable());
                    tgTargetLOT_Over_CHK(dtAddLotList, ErrorLotidList);
                }

                if (ErrorLotidList.Rows.Count > 0 || ErrorLotid.Count() > 0)
                {
                    if (ErrorLotid.Count() > 0)
                    {
                        ErrorLotidList.Merge(ErrorLotid.CopyToDataTable());
                    }

                    DataTable ErrorLotidMsg = new DataTable();
                    ErrorLotidMsg.TableName = "ErrorLotidMsg";
                    ErrorLotidMsg.Columns.Add("LOTID", typeof(string));
                    ErrorLotidMsg.Columns.Add("MESSAGEID", typeof(string));

                    // 해당 LOTID의 오류번호 테이블에 삽입
                    for (int i = 0; i < ErrorLotidList.Rows.Count; i++)
                    {
                        if (!overLapLot(ErrorLotidList.Rows[i]["LOTID"].ToString()))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU1384");
                        }
                        else if (ErrorLotidList.Rows[i]["GR_FLAG"].Equals("Y"))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU8473");
                        }
                        else if (!ErrorLotidList.Rows[i]["PROCID"].Equals(DataTableConverter.GetValue(dgTargetLOT.Rows[0].DataItem, "PROCID").ToString()))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU4957");
                        }
                        else if (!ErrorLotidList.Rows[i]["PRODID"].Equals(DataTableConverter.GetValue(dgTargetLOT.Rows[0].DataItem, "PRODID").ToString()))
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU4956");
                        }
                        else
                        {
                            ErrorLotidMsg.Rows.Add(ErrorLotidList.Rows[i]["LOTID"], "SFU5950");
                        }
                    }
                    this.Show_PACK003_031_EXCEPTION_POPUP(ErrorLotidMsg);
                }
                Util.GridSetData(dgTargetLOT, dtAddLotList, FrameOperation);
            }
        }

        // 최대 입력 가능수량 초과 확인
        private void tgTargetLOT_Over_CHK(DataTable dtAddLotList, DataTable ErrorLotidList)
        {
            if (dtAddLotList.Rows.Count > Convert.ToInt32(LIMIT_QTY.Text))
            {
                int RowNum = dtAddLotList.Rows.Count;
                for (int i = RowNum; i > Convert.ToInt32(LIMIT_QTY.Text); i--)
                {
                    DataRow dr = dtAddLotList.Rows[i - 1];
                    ErrorLotidList.ImportRow(dr);
                    dtAddLotList.Rows.RemoveAt(i - 1);
                }
            }
        }

        // 에러 LOTID 및 에러 내용 PopUp Open
        private void Show_PACK003_031_EXCEPTION_POPUP(DataTable dt)
        {
            COM001_035_PACK_EXCEPTION_POPUP popup = new COM001_035_PACK_EXCEPTION_POPUP();
            popup.FrameOperation = FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = "GROUP_MENAGEMENT";

                C1WindowExtension.SetParameters(popup, Parameters);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void txtSearchGroup_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if(string.IsNullOrWhiteSpace(Util.GetCondition(txtSearchGroup)))
                    {
                        Util.MessageInfo("SFU1801");        // 입력 데이터가 존재하지 않습니다.
                        return;
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANG_ID", typeof(string));
                    RQSTDT.Columns.Add("GR_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANG_ID"] = LoginInfo.LANGID;
                    dr["GR_ID"] = Util.GetCondition(txtSearchGroup);

                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LOGIS_MOD_GR_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                    dgGrouphistory.ItemsSource = null;

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgGrouphistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        Util.MessageInfo("SFU2816");    // 조회 결과가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(bReGrouping) // 이력화면에서 적용 버튼을 이용하여 수정할 경우
                {
                    SetReGroupProcess();
                }
                else  // 기본 Group
                {
                    SetNormalGroupProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnGroupDisassembley_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCheckcnt = 0; // 체크 박스 선택된 수량

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("GR_ID", typeof(string));
                INDATA.Columns.Add("WORK_TYPE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboEqptID1);
                dr["USERID"] = LoginInfo.USERID;
                dr["GR_ID"] = Util.GetCondition(txtGroupID);
                dr["WORK_TYPE"] = "D";      // Disassembley
                INDATA.Rows.Add(dr);

                DataTable IN_LOT = indataSet.Tables.Add("INLOT");
                IN_LOT.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTargetLOT.GetRowCount(); i++)
                {
                    string sProcID = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["PROCID"].Index).Value);
                    string sWipstate = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["WIPSTAT"].Index).Value);

                    
                    if (DataTableConverter.GetValue(dgTargetLOT.Rows[i].DataItem, "CHK").Equals("True"))
                    {
                        // Group Mapping은 PROCID = P5250 이고 WIP상태가 WAIT 또는 PROC 상태일 경우만 가능함
                        //if (sProcID.Equals("P5250") &&
                        //    sWipstate.Equals("WAIT"))
                        //{
                            string sLotId = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["LOTID"].Index).Value);
                            iCheckcnt++;

                            DataRow dr2 = IN_LOT.NewRow();
                            dr2["LOTID"] = sLotId;
                            IN_LOT.Rows.Add(dr2);
                        //}
                        //else
                        //{
                        //    string sWipStat = "WAIT";
                        //    Util.MessageInfo("SFU8475", sWipStat);    // P5250(CMA Trucking) 공정이고 재공상태가 [%1]일 경우만 작업 가능합니다.
                        //    return;
                        //}
                    }
                }

                if (iCheckcnt == 0)
                {
                    ms.AlertInfo("PSS9073"); // 선택된 LOT이 없습니다.
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_GROUP_DISASSEMBLE_CHANGE", "INDATA,IN_LOT", "", indataSet);

                ms.AlertInfo("PSS9072");  // 처리가 완료되었습니다.
                SetReset();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANG_ID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANG_ID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["EQPTID"] = Util.GetCondition(cboEqptIDHist);
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LOGIS_MOD_GR_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgGrouphistory.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgGrouphistory, dtResult, FrameOperation);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetReset();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgTargetLOT.ItemsSource != null)
            {
                for (int i = dgTargetLOT.GetRowCount(); 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgTargetLOT.Rows[i - 1].DataItem, "CHK");

                    if (chkYn == null)
                    {
                        dgTargetLOT.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgTargetLOT.EndNewRow(true);
                        dgTargetLOT.RemoveRow(i - 1);
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgTargetLOT.ItemsSource);
            }
        }

        // 이력 그리드 적용 버튼
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnGroupDisassembley.IsEnabled = true;

                Button btn = sender as Button;

                string grid_name = "dgGrouphistory";
                int seleted_row = 0;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;

                        seleted_row = row.Index;

                        grid_name = presenter.DataGrid.Name;
                    }
                }

                DataRowView drv = row.DataItem as DataRowView;

                string selectGroup = drv["GR_ID"].ToString();
                string selectEQPTID = drv["EQPTID"].ToString();
                string selectUSE_FLAG = drv["USE_FLAG"].ToString();
                string selectLogisPackType = drv["LOGIS_PACK_TYPE"].ToString();

                if (selectUSE_FLAG.Equals("N"))
                {
                    ms.AlertInfo("SFU1669");        // 선택할 수 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgGrouphistory.ItemsSource);

                dt.AcceptChanges();

                foreach (DataRow drDel in dt.Rows)
                {
                    if (drDel["GR_ID"].ToString() == selectGroup)
                    {
                        drDel.Delete();
                        break;
                    }
                }

                dt.AcceptChanges();

                Util.GridSetData(dgGrouphistory, dt, FrameOperation);

                GetGroupLotList(selectGroup);

                txtGroupID.Text = selectGroup;

                cboEqptID1.SelectedValue = selectEQPTID;
                cboEqptID1.IsEnabled = false;
                //cboPackType.SelectedValue = selectLogisPackType;
                //cboPackType.IsEnabled = false;

                bReGrouping = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Mehod
        private void setComboBox()
        {
            setcboEqptID();

            //String[] sFilter = { "LOGIS_PACK_TYPE" };
            //_combo.SetCombo(cboPackType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "COMMCODE");
        }

        private void setcboEqptID()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_BOX_LINE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                cboEqptID1.DisplayMemberPath = "CBO_NAME";
                cboEqptID1.SelectedValuePath = "CBO_CODE";
                cboEqptID1.ItemsSource = DataTableConverter.Convert(dtRSLTDT);

                cboEqptID1.SelectedIndex = 0;

                cboEqptIDHist.DisplayMemberPath = "CBO_NAME";
                cboEqptIDHist.SelectedValuePath = "CBO_CODE";
                cboEqptIDHist.ItemsSource = DataTableConverter.Convert(dtRSLTDT);

                cboEqptIDHist.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void labelprint(string sGroupID)
        {
            List<string> lstPalletID = new List<string>();
            lstPalletID.Add(sGroupID);
            using (PACK003_030 pack003_030 = new PACK003_030())
            {
                pack003_030.GroupLabelPrintBatch(lstPalletID);
            }
        }

        private Boolean overLapLot(string strLotId)
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTargetLOT.ItemsSource);

                if (dt.Rows.Count == 0)
                {
                    return true;
                }

                if (dt.Select("LOTID = '" + strLotId + "'").Count() > 0)
                {
                    return false;

                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetGroupLotList(string sGruopID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("GR_ID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr1 = RQSTDT.NewRow();
                dr1["GR_ID"] = sGruopID;       
                dr1["USE_FLAG"] = "Y";
                dr1["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr1);

                DataTable dtGroupLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LOGIS_MOD_GR_DETL", "INDATA", "OUTDATA", RQSTDT);

                dgTargetLOT.ItemsSource = null;

                Util.GridSetData(dgTargetLOT, dtGroupLots, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReset()
        {
            dgTargetLOT.ItemsSource = null;
            btnGroupDisassembley.IsEnabled = false;
            cboEqptID1.IsEnabled = true;
            cboEqptID1.SelectedIndex = 0;
            //cboPackType.IsEnabled = true;
            //cboPackType.SelectedIndex = 0;

            txtGroupID.Text = "";
            txtLotID.Text = "";

            bReGrouping = false;
        }

        private void SetNormalGroupProcess()
        {
            if (Util.GetCondition(cboEqptID1) == "") // 포장기
            {
                string sBatType = ObjectDic.Instance.GetObjectName(tbEqpt.Text.ToString()).Replace("[#] ", string.Empty);
                Util.MessageValidation("SFU8393", sBatType); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                return;
            }

            //if (Util.GetCondition(cboPackType) == "") // 물류포장구분
            //{
            //    string sBatType = ObjectDic.Instance.GetObjectName(tbPackType.Text.ToString()).Replace("[#] ", string.Empty);
            //    Util.MessageValidation("SFU8393", sBatType); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
            //    return;
            //}

            if (dgTargetLOT.GetRowCount() == 0)
            {
                return;
            }

            DataSet indataSet = new DataSet();

            DataTable INDATA = indataSet.Tables.Add("INDATA");
            INDATA.Columns.Add("EQPTID", typeof(string));       // 포장기 정보
            INDATA.Columns.Add("USERID", typeof(string));       // 사용자 ID
            INDATA.Columns.Add("PROCID", typeof(string));       // 첫번째 ROW 공정ID

            DataRow dr = INDATA.NewRow();
            dr["EQPTID"] = Util.GetCondition(cboEqptID1);
            dr["USERID"] = LoginInfo.USERID;
            dr["PROCID"] = Util.NVC(dgTargetLOT.GetCell(0, dgTargetLOT.Columns["PROCID"].Index).Value);
            INDATA.Rows.Add(dr);

            DataTable IN_LOT = indataSet.Tables.Add("IN_LOT");
            IN_LOT.Columns.Add("LOTID", typeof(string));        // LOTID

            for (int i = 0; i < dgTargetLOT.GetRowCount(); i++)
            {
                string sfistProc = Util.NVC(dgTargetLOT.GetCell(0, dgTargetLOT.Columns["PROCID"].Index).Value); 

                //구분
                //if(Util.GetCondition(cboPackType).Equals("TRU"))
                //{
                string sProcID = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["PROCID"].Index).Value);

                if (!(sfistProc.Equals(sProcID)))
                {
                    Util.MessageInfo("SFU3600");    // 동일 공정이 아닙니다.
                    return;
                }
                    string sWipstate = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["WIPSTAT"].Index).Value);

                    // Trucking의 Group은 PROCID = P5250 이고 WIP상태가 WAIT 또는 PROC 상태일 경우만 가능함
                    //if (sProcID.Equals("P5250") &&
                    //    (sWipstate.Equals("WAIT") || sWipstate.Equals("PROC"))
                    //   )
                    //{
                        DataRow drLOT = IN_LOT.NewRow();
                        drLOT["LOTID"] = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["LOTID"].Index).Value);
                        IN_LOT.Rows.Add(drLOT);
                    //}
                    //else
                    //{
                    //    string sWipStat = "WAIT/PROC";
                    //    Util.MessageInfo("SFU8475", sWipStat);    // P5250(CMA Trucking) 공정이고 재공상태가 [%1]일 경우만 작업 가능합니다.
                    //    return;
                    //}
                //}

            }

            DataSet outdatSet = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_GROUP_MAPPING", "INDATA,IN_LOT", "OUTDATA", indataSet);

            if (outdatSet != null && !string.IsNullOrWhiteSpace(outdatSet.Tables["OUTDATA"].Rows[0]["GR_ID"].ToString()))
            {
                // MMD0117 - 라벨을 발행 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("MMD0117"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        labelprint(outdatSet.Tables["OUTDATA"].Rows[0]["GR_ID"].ToString());

                    }
                    else
                    {
                        return;
                    }
                }
               );
            }

            // 초기화
            dgTargetLOT.ItemsSource = null;
            btnGroupDisassembley.IsEnabled = false;
            btnSearch_Click(null, null);
        }

        private void SetReGroupProcess()
        {
            if (dgTargetLOT.GetRowCount() == 0)
            {
                return;
            }

            DataSet indataSet = new DataSet();

            DataTable INDATA = indataSet.Tables.Add("INDATA");
            INDATA.Columns.Add("EQPTID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("GR_ID", typeof(string));
            INDATA.Columns.Add("WORK_TYPE", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["EQPTID"] = Util.GetCondition(cboEqptID1);
            dr["USERID"] = LoginInfo.USERID;
            dr["GR_ID"] = Util.GetCondition(txtGroupID);
            dr["WORK_TYPE"] = "C";      // Change (Group 수정)
            INDATA.Rows.Add(dr);

            DataTable IN_LOT = indataSet.Tables.Add("INLOT");
            IN_LOT.Columns.Add("LOTID", typeof(string));

            for (int i = 0; i < dgTargetLOT.GetRowCount(); i++)
            {
                string sLotId = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["LOTID"].Index).Value);
                string sProcID = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["PROCID"].Index).Value);
                string sWipstate = Util.NVC(dgTargetLOT.GetCell(i, dgTargetLOT.Columns["WIPSTAT"].Index).Value);

                // Group Mapping은 PROCID = P5250 이고 WIP상태가 WAIT 또는 PROC 상태일 경우만 가능함
                //if (sProcID.Equals("P5250") &&
                //    sWipstate.Equals("WAIT"))
                //{
                    DataRow dr2 = IN_LOT.NewRow();
                    dr2["LOTID"] = sLotId;
                    IN_LOT.Rows.Add(dr2);
                //}
                //else
                //{
                //    string sWipStat = "WAIT";
                //    Util.MessageInfo("SFU8475", sWipStat);    // P5250(CMA Trucking) 공정이고 재공상태가 [%1]일 경우만 작업 가능합니다.
                //    return;
                //}
            }

            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_GROUP_DISASSEMBLE_CHANGE", "INDATA,IN_LOT", "", indataSet);

            ms.AlertInfo("PSS9072");  // 처리가 완료되었습니다.
            SetReset();
        }
        #endregion

    }
}